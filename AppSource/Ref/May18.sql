PGDMP         )    	            q            pos2013    9.0.3    9.0.3    �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                       false            �           0    0 
   STDSTRINGS 
   STDSTRINGS     )   SET standard_conforming_strings = 'off';
                       false            �           1262    234853    pos2013    DATABASE     �   CREATE DATABASE pos2013 WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252';
    DROP DATABASE pos2013;
             devTeam    false                        2615    234854    pgagent    SCHEMA        CREATE SCHEMA pgagent;
    DROP SCHEMA pgagent;
             postgres    false            �           0    0    SCHEMA pgagent    COMMENT     6   COMMENT ON SCHEMA pgagent IS 'pgAgent system tables';
                  postgres    false    6                        2615    2200    public    SCHEMA        CREATE SCHEMA public;
    DROP SCHEMA public;
             postgres    false            �           0    0    SCHEMA public    COMMENT     6   COMMENT ON SCHEMA public IS 'standard public schema';
                  postgres    false    7            �           0    0    public    ACL     �   REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;
                  postgres    false    7            /           2612    11574    plpgsql    PROCEDURAL LANGUAGE     /   CREATE OR REPLACE PROCEDURAL LANGUAGE plpgsql;
 "   DROP PROCEDURAL LANGUAGE plpgsql;
             postgres    false                        1255    234856    pga_exception_trigger()    FUNCTION     
  CREATE FUNCTION pga_exception_trigger() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE

    jobid int4 := 0;

BEGIN

     IF TG_OP = 'DELETE' THEN

        SELECT INTO jobid jscjobid FROM pgagent.pga_schedule WHERE jscid = OLD.jexscid;

        -- update pga_job from remaining schedules
        -- the actual calculation of jobnextrun will be performed in the trigger
        UPDATE pgagent.pga_job
           SET jobnextrun = NULL
         WHERE jobenabled AND jobid=jobid;
        RETURN OLD;
    ELSE

        SELECT INTO jobid jscjobid FROM pgagent.pga_schedule WHERE jscid = NEW.jexscid;

        UPDATE pgagent.pga_job
           SET jobnextrun = NULL
         WHERE jobenabled AND jobid=jobid;
        RETURN NEW;
    END IF;
END;
$$;
 /   DROP FUNCTION pgagent.pga_exception_trigger();
       pgagent       postgres    false    6    559            �           0    0     FUNCTION pga_exception_trigger()    COMMENT     p   COMMENT ON FUNCTION pga_exception_trigger() IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    19                        1255    234857    pga_is_leap_year(smallint)    FUNCTION       CREATE FUNCTION pga_is_leap_year(smallint) RETURNS boolean
    LANGUAGE plpgsql IMMUTABLE
    AS $_$
BEGIN
    IF $1 % 4 != 0 THEN
        RETURN FALSE;
    END IF;

    IF $1 % 100 != 0 THEN
        RETURN TRUE;
    END IF;

    RETURN $1 % 400 = 0;
END;
$_$;
 2   DROP FUNCTION pgagent.pga_is_leap_year(smallint);
       pgagent       postgres    false    559    6            �           0    0 #   FUNCTION pga_is_leap_year(smallint)    COMMENT     W   COMMENT ON FUNCTION pga_is_leap_year(smallint) IS 'Returns TRUE is $1 is a leap year';
            pgagent       postgres    false    20                        1255    234858    pga_job_trigger()    FUNCTION       CREATE FUNCTION pga_job_trigger() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF NEW.jobenabled THEN
        IF NEW.jobnextrun IS NULL THEN
             SELECT INTO NEW.jobnextrun
                    MIN(pgagent.pga_next_schedule(jscid, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths))
               FROM pgagent.pga_schedule
              WHERE jscenabled AND jscjobid=OLD.jobid;
        END IF;
    ELSE
        NEW.jobnextrun := NULL;
    END IF;
    RETURN NEW;
END;
$$;
 )   DROP FUNCTION pgagent.pga_job_trigger();
       pgagent       postgres    false    6    559            �           0    0    FUNCTION pga_job_trigger()    COMMENT     M   COMMENT ON FUNCTION pga_job_trigger() IS 'Update the job''s next run time.';
            pgagent       postgres    false    21                        1255    234859 �   pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    FUNCTION     g:  CREATE FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) RETURNS timestamp with time zone
    LANGUAGE plpgsql
    AS $_$
DECLARE
    jscid           ALIAS FOR $1;
    jscstart        ALIAS FOR $2;
    jscend          ALIAS FOR $3;
    jscminutes      ALIAS FOR $4;
    jschours        ALIAS FOR $5;
    jscweekdays     ALIAS FOR $6;
    jscmonthdays    ALIAS FOR $7;
    jscmonths       ALIAS FOR $8;

    nextrun         timestamp := '1970-01-01 00:00:00-00';
    runafter        timestamp := '1970-01-01 00:00:00-00';

    bingo            bool := FALSE;
    gotit            bool := FALSE;
    foundval        bool := FALSE;
    daytweak        bool := FALSE;
    minutetweak        bool := FALSE;

    i                int2 := 0;
    d                int2 := 0;

    nextminute        int2 := 0;
    nexthour        int2 := 0;
    nextday            int2 := 0;
    nextmonth       int2 := 0;
    nextyear        int2 := 0;


BEGIN
    -- No valid start date has been specified
    IF jscstart IS NULL THEN RETURN NULL; END IF;

    -- The schedule is past its end date
    IF jscend IS NOT NULL AND jscend < now() THEN RETURN NULL; END IF;

    -- Get the time to find the next run after. It will just be the later of
    -- now() + 1m and the start date for the time being, however, we might want to
    -- do more complex things using this value in the future.
    IF date_trunc('MINUTE', jscstart) > date_trunc('MINUTE', (now() + '1 Minute'::interval)) THEN
        runafter := date_trunc('MINUTE', jscstart);
    ELSE
        runafter := date_trunc('MINUTE', (now() + '1 Minute'::interval));
    END IF;

    --
    -- Enter a loop, generating next run timestamps until we find one
    -- that falls on the required weekday, and is not matched by an exception
    --

    WHILE bingo = FALSE LOOP

        --
        -- Get the next run year
        --
        nextyear := date_part('YEAR', runafter);

        --
        -- Get the next run month
        --
        nextmonth := date_part('MONTH', runafter);
        gotit := FALSE;
        FOR i IN (nextmonth) .. 12 LOOP
            IF jscmonths[i] = TRUE THEN
                nextmonth := i;
                gotit := TRUE;
                foundval := TRUE;
                EXIT;
            END IF;
        END LOOP;
        IF gotit = FALSE THEN
            FOR i IN 1 .. (nextmonth - 1) LOOP
                IF jscmonths[i] = TRUE THEN
                    nextmonth := i;

                    -- Wrap into next year
                    nextyear := nextyear + 1;
                    gotit := TRUE;
                    foundval := TRUE;
                    EXIT;
                END IF;
           END LOOP;
        END IF;

        --
        -- Get the next run day
        --
        -- If the year, or month have incremented, get the lowest day,
        -- otherwise look for the next day matching or after today.
        IF (nextyear > date_part('YEAR', runafter) OR nextmonth > date_part('MONTH', runafter)) THEN
            nextday := 1;
            FOR i IN 1 .. 32 LOOP
                IF jscmonthdays[i] = TRUE THEN
                    nextday := i;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
        ELSE
            nextday := date_part('DAY', runafter);
            gotit := FALSE;
            FOR i IN nextday .. 32 LOOP
                IF jscmonthdays[i] = TRUE THEN
                    nextday := i;
                    gotit := TRUE;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
            IF gotit = FALSE THEN
                FOR i IN 1 .. (nextday - 1) LOOP
                    IF jscmonthdays[i] = TRUE THEN
                        nextday := i;

                        -- Wrap into next month
                        IF nextmonth = 12 THEN
                            nextyear := nextyear + 1;
                            nextmonth := 1;
                        ELSE
                            nextmonth := nextmonth + 1;
                        END IF;
                        gotit := TRUE;
                        foundval := TRUE;
                        EXIT;
                    END IF;
                END LOOP;
            END IF;
        END IF;

        -- Was the last day flag selected?
        IF nextday = 32 THEN
            IF nextmonth = 1 THEN
                nextday := 31;
            ELSIF nextmonth = 2 THEN
                IF pgagent.pga_is_leap_year(nextyear) = TRUE THEN
                    nextday := 29;
                ELSE
                    nextday := 28;
                END IF;
            ELSIF nextmonth = 3 THEN
                nextday := 31;
            ELSIF nextmonth = 4 THEN
                nextday := 30;
            ELSIF nextmonth = 5 THEN
                nextday := 31;
            ELSIF nextmonth = 6 THEN
                nextday := 30;
            ELSIF nextmonth = 7 THEN
                nextday := 31;
            ELSIF nextmonth = 8 THEN
                nextday := 31;
            ELSIF nextmonth = 9 THEN
                nextday := 30;
            ELSIF nextmonth = 10 THEN
                nextday := 31;
            ELSIF nextmonth = 11 THEN
                nextday := 30;
            ELSIF nextmonth = 12 THEN
                nextday := 31;
            END IF;
        END IF;

        --
        -- Get the next run hour
        --
        -- If the year, month or day have incremented, get the lowest hour,
        -- otherwise look for the next hour matching or after the current one.
        IF (nextyear > date_part('YEAR', runafter) OR nextmonth > date_part('MONTH', runafter) OR nextday > date_part('DAY', runafter) OR daytweak = TRUE) THEN
            nexthour := 0;
            FOR i IN 1 .. 24 LOOP
                IF jschours[i] = TRUE THEN
                    nexthour := i - 1;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
        ELSE
            nexthour := date_part('HOUR', runafter);
            gotit := FALSE;
            FOR i IN (nexthour + 1) .. 24 LOOP
                IF jschours[i] = TRUE THEN
                    nexthour := i - 1;
                    gotit := TRUE;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
            IF gotit = FALSE THEN
                FOR i IN 1 .. nexthour LOOP
                    IF jschours[i] = TRUE THEN
                        nexthour := i - 1;

                        -- Wrap into next month
                        IF (nextmonth = 1 OR nextmonth = 3 OR nextmonth = 5 OR nextmonth = 7 OR nextmonth = 8 OR nextmonth = 10 OR nextmonth = 12) THEN
                            d = 31;
                        ELSIF (nextmonth = 4 OR nextmonth = 6 OR nextmonth = 9 OR nextmonth = 11) THEN
                            d = 30;
                        ELSE
                            IF pgagent.pga_is_leap_year(nextyear) = TRUE THEN
                                d := 29;
                            ELSE
                                d := 28;
                            END IF;
                        END IF;

                        IF nextday = d THEN
                            nextday := 1;
                            IF nextmonth = 12 THEN
                                nextyear := nextyear + 1;
                                nextmonth := 1;
                            ELSE
                                nextmonth := nextmonth + 1;
                            END IF;
                        ELSE
                            nextday := nextday + 1;
                        END IF;

                        gotit := TRUE;
                        foundval := TRUE;
                        EXIT;
                    END IF;
                END LOOP;
            END IF;
        END IF;

        --
        -- Get the next run minute
        --
        -- If the year, month day or hour have incremented, get the lowest minute,
        -- otherwise look for the next minute matching or after the current one.
        IF (nextyear > date_part('YEAR', runafter) OR nextmonth > date_part('MONTH', runafter) OR nextday > date_part('DAY', runafter) OR nexthour > date_part('HOUR', runafter) OR daytweak = TRUE) THEN
            nextminute := 0;
            IF minutetweak = TRUE THEN
        d := 1;
            ELSE
        d := date_part('YEAR', runafter)::int2;
            END IF;
            FOR i IN d .. 60 LOOP
                IF jscminutes[i] = TRUE THEN
                    nextminute := i - 1;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
        ELSE
            nextminute := date_part('MINUTE', runafter);
            gotit := FALSE;
            FOR i IN (nextminute + 1) .. 60 LOOP
                IF jscminutes[i] = TRUE THEN
                    nextminute := i - 1;
                    gotit := TRUE;
                    foundval := TRUE;
                    EXIT;
                END IF;
            END LOOP;
            IF gotit = FALSE THEN
                FOR i IN 1 .. nextminute LOOP
                    IF jscminutes[i] = TRUE THEN
                        nextminute := i - 1;

                        -- Wrap into next hour
                        IF (nextmonth = 1 OR nextmonth = 3 OR nextmonth = 5 OR nextmonth = 7 OR nextmonth = 8 OR nextmonth = 10 OR nextmonth = 12) THEN
                            d = 31;
                        ELSIF (nextmonth = 4 OR nextmonth = 6 OR nextmonth = 9 OR nextmonth = 11) THEN
                            d = 30;
                        ELSE
                            IF pgagent.pga_is_leap_year(nextyear) = TRUE THEN
                                d := 29;
                            ELSE
                                d := 28;
                            END IF;
                        END IF;

                        IF nexthour = 23 THEN
                            nexthour = 0;
                            IF nextday = d THEN
                                nextday := 1;
                                IF nextmonth = 12 THEN
                                    nextyear := nextyear + 1;
                                    nextmonth := 1;
                                ELSE
                                    nextmonth := nextmonth + 1;
                                END IF;
                            ELSE
                                nextday := nextday + 1;
                            END IF;
                        ELSE
                            nexthour := nexthour + 1;
                        END IF;

                        gotit := TRUE;
                        foundval := TRUE;
                        EXIT;
                    END IF;
                END LOOP;
            END IF;
        END IF;

        -- Build the result, and check it is not the same as runafter - this may
        -- happen if all array entries are set to false. In this case, add a minute.

        nextrun := (nextyear::varchar || '-'::varchar || nextmonth::varchar || '-' || nextday::varchar || ' ' || nexthour::varchar || ':' || nextminute::varchar)::timestamptz;

        IF nextrun = runafter AND foundval = FALSE THEN
                nextrun := nextrun + INTERVAL '1 Minute';
        END IF;

        -- If the result is past the end date, exit.
        IF nextrun > jscend THEN
            RETURN NULL;
        END IF;

        -- Check to ensure that the nextrun time is actually still valid. Its
        -- possible that wrapped values may have carried the nextrun onto an
        -- invalid time or date.
        IF ((jscminutes = '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}' OR jscminutes[date_part('MINUTE', nextrun) + 1] = TRUE) AND
            (jschours = '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}' OR jschours[date_part('HOUR', nextrun) + 1] = TRUE) AND
            (jscmonthdays = '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}' OR jscmonthdays[date_part('DAY', nextrun)] = TRUE OR
            (jscmonthdays = '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,t}' AND
             ((date_part('MONTH', nextrun) IN (1,3,5,7,8,10,12) AND date_part('DAY', nextrun) = 31) OR
              (date_part('MONTH', nextrun) IN (4,6,9,11) AND date_part('DAY', nextrun) = 30) OR
              (date_part('MONTH', nextrun) = 2 AND ((pgagent.pga_is_leap_year(date_part('DAY', nextrun)::int2) AND date_part('DAY', nextrun) = 29) OR date_part('DAY', nextrun) = 28))))) AND
            (jscmonths = '{f,f,f,f,f,f,f,f,f,f,f,f}' OR jscmonths[date_part('MONTH', nextrun)] = TRUE)) THEN


            -- Now, check to see if the nextrun time found is a) on an acceptable
            -- weekday, and b) not matched by an exception. If not, set
            -- runafter = nextrun and try again.

            -- Check for a wildcard weekday
            gotit := FALSE;
            FOR i IN 1 .. 7 LOOP
                IF jscweekdays[i] = TRUE THEN
                    gotit := TRUE;
                    EXIT;
                END IF;
            END LOOP;

            -- OK, is the correct weekday selected, or a wildcard?
            IF (jscweekdays[date_part('DOW', nextrun) + 1] = TRUE OR gotit = FALSE) THEN

                -- Check for exceptions
                SELECT INTO d jexid FROM pgagent.pga_exception WHERE jexscid = jscid AND ((jexdate = nextrun::date AND jextime = nextrun::time) OR (jexdate = nextrun::date AND jextime IS NULL) OR (jexdate IS NULL AND jextime = nextrun::time));
                IF FOUND THEN
                    -- Nuts - found an exception. Increment the time and try again
                    runafter := nextrun + INTERVAL '1 Minute';
                    bingo := FALSE;
                    minutetweak := TRUE;
            daytweak := FALSE;
                ELSE
                    bingo := TRUE;
                END IF;
            ELSE
                -- We're on the wrong week day - increment a day and try again.
                runafter := nextrun + INTERVAL '1 Day';
                bingo := FALSE;
                minutetweak := FALSE;
                daytweak := TRUE;
            END IF;

        ELSE
            runafter := nextrun + INTERVAL '1 Minute';
            bingo := FALSE;
            minutetweak := TRUE;
        daytweak := FALSE;
        END IF;

    END LOOP;

    RETURN nextrun;
END;
$_$;
 �   DROP FUNCTION pgagent.pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]);
       pgagent       postgres    false    6    559            �           0    0 �   FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    COMMENT     �   COMMENT ON FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) IS 'Calculates the next runtime for a given schedule';
            pgagent       postgres    false    22                        1255    234861    pga_schedule_trigger()    FUNCTION     7  CREATE FUNCTION pga_schedule_trigger() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        -- update pga_job from remaining schedules
        -- the actual calculation of jobnextrun will be performed in the trigger
        UPDATE pgagent.pga_job
           SET jobnextrun = NULL
         WHERE jobenabled AND jobid=OLD.jscjobid;
        RETURN OLD;
    ELSE
        UPDATE pgagent.pga_job
           SET jobnextrun = NULL
         WHERE jobenabled AND jobid=NEW.jscjobid;
        RETURN NEW;
    END IF;
END;
$$;
 .   DROP FUNCTION pgagent.pga_schedule_trigger();
       pgagent       postgres    false    559    6            �           0    0    FUNCTION pga_schedule_trigger()    COMMENT     m   COMMENT ON FUNCTION pga_schedule_trigger() IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    23                        1255    234862    pgagent_schema_version()    FUNCTION     �   CREATE FUNCTION pgagent_schema_version() RETURNS smallint
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- RETURNS PGAGENT MAJOR VERSION
    -- WE WILL CHANGE THE MAJOR VERSION, ONLY IF THERE IS A SCHEMA CHANGE
    RETURN 3;
END;
$$;
 0   DROP FUNCTION pgagent.pgagent_schema_version();
       pgagent       postgres    false    559    6                        1255    234863 7   checkserialnumber(character varying, character varying)    FUNCTION     %  CREATE FUNCTION checkserialnumber("partNumber" character varying, "serialNumber" character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $_$BEGIN
RETURN (SELECT COUNT(*)
FROM 
  stockadjustmentdetailserial sads
WHERE 
  sads.serialnumber = $2 AND 
  sads.partnumber = $1) > 0;
END;$_$;
 j   DROP FUNCTION public.checkserialnumber("partNumber" character varying, "serialNumber" character varying);
       public       postgres    false    559    7                        1255    234864    newid()    FUNCTION     �   CREATE FUNCTION newid() RETURNS uuid
    LANGUAGE sql
    AS $$
 SELECT CAST(md5(current_database()|| user ||current_timestamp ||random()) as uuid)
$$;
    DROP FUNCTION public.newid();
       public       postgres    false    7            �           1259    234865    pga_exception    TABLE     �   CREATE TABLE pga_exception (
    jexid integer NOT NULL,
    jexscid integer NOT NULL,
    jexdate date,
    jextime time without time zone
);
 "   DROP TABLE pgagent.pga_exception;
       pgagent         postgres    false    6            �           1259    234868    pga_exception_jexid_seq    SEQUENCE     y   CREATE SEQUENCE pga_exception_jexid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE pgagent.pga_exception_jexid_seq;
       pgagent       postgres    false    1751    6            �           0    0    pga_exception_jexid_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE pga_exception_jexid_seq OWNED BY pga_exception.jexid;
            pgagent       postgres    false    1752            �           0    0    pga_exception_jexid_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('pga_exception_jexid_seq', 1, false);
            pgagent       postgres    false    1752            �           1259    234870    pga_job    TABLE     �  CREATE TABLE pga_job (
    jobid integer NOT NULL,
    jobjclid integer NOT NULL,
    jobname text NOT NULL,
    jobdesc text DEFAULT ''::text NOT NULL,
    jobhostagent text DEFAULT ''::text NOT NULL,
    jobenabled boolean DEFAULT true NOT NULL,
    jobcreated timestamp with time zone DEFAULT now() NOT NULL,
    jobchanged timestamp with time zone DEFAULT now() NOT NULL,
    jobagentid integer,
    jobnextrun timestamp with time zone,
    joblastrun timestamp with time zone
);
    DROP TABLE pgagent.pga_job;
       pgagent         postgres    false    2175    2176    2177    2178    2179    6            �           0    0    TABLE pga_job    COMMENT     .   COMMENT ON TABLE pga_job IS 'Job main entry';
            pgagent       postgres    false    1753            �           0    0    COLUMN pga_job.jobagentid    COMMENT     S   COMMENT ON COLUMN pga_job.jobagentid IS 'Agent that currently executes this job.';
            pgagent       postgres    false    1753            �           1259    234881    pga_job_jobid_seq    SEQUENCE     s   CREATE SEQUENCE pga_job_jobid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE pgagent.pga_job_jobid_seq;
       pgagent       postgres    false    1753    6            �           0    0    pga_job_jobid_seq    SEQUENCE OWNED BY     9   ALTER SEQUENCE pga_job_jobid_seq OWNED BY pga_job.jobid;
            pgagent       postgres    false    1754            �           0    0    pga_job_jobid_seq    SEQUENCE SET     9   SELECT pg_catalog.setval('pga_job_jobid_seq', 1, false);
            pgagent       postgres    false    1754            �           1259    234883    pga_jobagent    TABLE     �   CREATE TABLE pga_jobagent (
    jagpid integer NOT NULL,
    jaglogintime timestamp with time zone DEFAULT now() NOT NULL,
    jagstation text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobagent;
       pgagent         postgres    false    2181    6            �           0    0    TABLE pga_jobagent    COMMENT     6   COMMENT ON TABLE pga_jobagent IS 'Active job agents';
            pgagent       postgres    false    1755            �           1259    234890    pga_jobclass    TABLE     U   CREATE TABLE pga_jobclass (
    jclid integer NOT NULL,
    jclname text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobclass;
       pgagent         postgres    false    6            �           0    0    TABLE pga_jobclass    COMMENT     7   COMMENT ON TABLE pga_jobclass IS 'Job classification';
            pgagent       postgres    false    1756            �           1259    234896    pga_jobclass_jclid_seq    SEQUENCE     x   CREATE SEQUENCE pga_jobclass_jclid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_jobclass_jclid_seq;
       pgagent       postgres    false    1756    6            �           0    0    pga_jobclass_jclid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_jobclass_jclid_seq OWNED BY pga_jobclass.jclid;
            pgagent       postgres    false    1757            �           0    0    pga_jobclass_jclid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobclass_jclid_seq', 5, true);
            pgagent       postgres    false    1757            �           1259    234898 
   pga_joblog    TABLE     v  CREATE TABLE pga_joblog (
    jlgid integer NOT NULL,
    jlgjobid integer NOT NULL,
    jlgstatus character(1) DEFAULT 'r'::bpchar NOT NULL,
    jlgstart timestamp with time zone DEFAULT now() NOT NULL,
    jlgduration interval,
    CONSTRAINT pga_joblog_jlgstatus_check CHECK ((jlgstatus = ANY (ARRAY['r'::bpchar, 's'::bpchar, 'f'::bpchar, 'i'::bpchar, 'd'::bpchar])))
);
    DROP TABLE pgagent.pga_joblog;
       pgagent         postgres    false    2183    2184    2186    6            �           0    0    TABLE pga_joblog    COMMENT     0   COMMENT ON TABLE pga_joblog IS 'Job run logs.';
            pgagent       postgres    false    1758            �           0    0    COLUMN pga_joblog.jlgstatus    COMMENT     �   COMMENT ON COLUMN pga_joblog.jlgstatus IS 'Status of job: r=running, s=successfully finished, f=failed, i=no steps to execute, d=aborted';
            pgagent       postgres    false    1758            �           1259    234904    pga_joblog_jlgid_seq    SEQUENCE     v   CREATE SEQUENCE pga_joblog_jlgid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE pgagent.pga_joblog_jlgid_seq;
       pgagent       postgres    false    6    1758            �           0    0    pga_joblog_jlgid_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE pga_joblog_jlgid_seq OWNED BY pga_joblog.jlgid;
            pgagent       postgres    false    1759            �           0    0    pga_joblog_jlgid_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('pga_joblog_jlgid_seq', 1, false);
            pgagent       postgres    false    1759            �           1259    234906    pga_jobstep    TABLE       CREATE TABLE pga_jobstep (
    jstid integer NOT NULL,
    jstjobid integer NOT NULL,
    jstname text NOT NULL,
    jstdesc text DEFAULT ''::text NOT NULL,
    jstenabled boolean DEFAULT true NOT NULL,
    jstkind character(1) NOT NULL,
    jstcode text NOT NULL,
    jstconnstr text DEFAULT ''::text NOT NULL,
    jstdbname name DEFAULT ''::name NOT NULL,
    jstonerror character(1) DEFAULT 'f'::bpchar NOT NULL,
    jscnextrun timestamp with time zone,
    CONSTRAINT pga_jobstep_check CHECK ((((jstconnstr <> ''::text) AND (jstkind = 's'::bpchar)) OR ((jstconnstr = ''::text) AND ((jstkind = 'b'::bpchar) OR (jstdbname <> ''::name))))),
    CONSTRAINT pga_jobstep_check1 CHECK ((((jstdbname <> ''::name) AND (jstkind = 's'::bpchar)) OR ((jstdbname = ''::name) AND ((jstkind = 'b'::bpchar) OR (jstconnstr <> ''::text))))),
    CONSTRAINT pga_jobstep_jstkind_check CHECK ((jstkind = ANY (ARRAY['b'::bpchar, 's'::bpchar]))),
    CONSTRAINT pga_jobstep_jstonerror_check CHECK ((jstonerror = ANY (ARRAY['f'::bpchar, 's'::bpchar, 'i'::bpchar])))
);
     DROP TABLE pgagent.pga_jobstep;
       pgagent         postgres    false    2187    2188    2189    2190    2191    2193    2194    2195    2196    6            �           0    0    TABLE pga_jobstep    COMMENT     ;   COMMENT ON TABLE pga_jobstep IS 'Job step to be executed';
            pgagent       postgres    false    1760            �           0    0    COLUMN pga_jobstep.jstkind    COMMENT     L   COMMENT ON COLUMN pga_jobstep.jstkind IS 'Kind of jobstep: s=sql, b=batch';
            pgagent       postgres    false    1760            �           0    0    COLUMN pga_jobstep.jstonerror    COMMENT     �   COMMENT ON COLUMN pga_jobstep.jstonerror IS 'What to do if step returns an error: f=fail the job, s=mark step as succeeded and continue, i=mark as fail but ignore it and proceed';
            pgagent       postgres    false    1760            �           1259    234921    pga_jobstep_jstid_seq    SEQUENCE     w   CREATE SEQUENCE pga_jobstep_jstid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE pgagent.pga_jobstep_jstid_seq;
       pgagent       postgres    false    1760    6            �           0    0    pga_jobstep_jstid_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE pga_jobstep_jstid_seq OWNED BY pga_jobstep.jstid;
            pgagent       postgres    false    1761            �           0    0    pga_jobstep_jstid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobstep_jstid_seq', 1, false);
            pgagent       postgres    false    1761            �           1259    234923    pga_jobsteplog    TABLE     �  CREATE TABLE pga_jobsteplog (
    jslid integer NOT NULL,
    jsljlgid integer NOT NULL,
    jsljstid integer NOT NULL,
    jslstatus character(1) DEFAULT 'r'::bpchar NOT NULL,
    jslresult integer,
    jslstart timestamp with time zone DEFAULT now() NOT NULL,
    jslduration interval,
    jsloutput text,
    CONSTRAINT pga_jobsteplog_jslstatus_check CHECK ((jslstatus = ANY (ARRAY['r'::bpchar, 's'::bpchar, 'i'::bpchar, 'f'::bpchar, 'd'::bpchar])))
);
 #   DROP TABLE pgagent.pga_jobsteplog;
       pgagent         postgres    false    2197    2198    2200    6            �           0    0    TABLE pga_jobsteplog    COMMENT     9   COMMENT ON TABLE pga_jobsteplog IS 'Job step run logs.';
            pgagent       postgres    false    1762            �           0    0    COLUMN pga_jobsteplog.jslstatus    COMMENT     �   COMMENT ON COLUMN pga_jobsteplog.jslstatus IS 'Status of job step: r=running, s=successfully finished,  f=failed stopping job, i=ignored failure, d=aborted';
            pgagent       postgres    false    1762            �           0    0    COLUMN pga_jobsteplog.jslresult    COMMENT     I   COMMENT ON COLUMN pga_jobsteplog.jslresult IS 'Return code of job step';
            pgagent       postgres    false    1762            �           1259    234932    pga_jobsteplog_jslid_seq    SEQUENCE     z   CREATE SEQUENCE pga_jobsteplog_jslid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE pgagent.pga_jobsteplog_jslid_seq;
       pgagent       postgres    false    6    1762            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE pga_jobsteplog_jslid_seq OWNED BY pga_jobsteplog.jslid;
            pgagent       postgres    false    1763            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('pga_jobsteplog_jslid_seq', 1, false);
            pgagent       postgres    false    1763            �           1259    234934    pga_schedule    TABLE       CREATE TABLE pga_schedule (
    jscid integer NOT NULL,
    jscjobid integer NOT NULL,
    jscname text NOT NULL,
    jscdesc text DEFAULT ''::text NOT NULL,
    jscenabled boolean DEFAULT true NOT NULL,
    jscstart timestamp with time zone DEFAULT now() NOT NULL,
    jscend timestamp with time zone,
    jscminutes boolean[] DEFAULT '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}'::boolean[] NOT NULL,
    jschours boolean[] DEFAULT '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}'::boolean[] NOT NULL,
    jscweekdays boolean[] DEFAULT '{f,f,f,f,f,f,f}'::boolean[] NOT NULL,
    jscmonthdays boolean[] DEFAULT '{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}'::boolean[] NOT NULL,
    jscmonths boolean[] DEFAULT '{f,f,f,f,f,f,f,f,f,f,f,f}'::boolean[] NOT NULL,
    CONSTRAINT pga_schedule_jschours_size CHECK ((array_upper(jschours, 1) = 24)),
    CONSTRAINT pga_schedule_jscminutes_size CHECK ((array_upper(jscminutes, 1) = 60)),
    CONSTRAINT pga_schedule_jscmonthdays_size CHECK ((array_upper(jscmonthdays, 1) = 32)),
    CONSTRAINT pga_schedule_jscmonths_size CHECK ((array_upper(jscmonths, 1) = 12)),
    CONSTRAINT pga_schedule_jscweekdays_size CHECK ((array_upper(jscweekdays, 1) = 7))
);
 !   DROP TABLE pgagent.pga_schedule;
       pgagent         postgres    false    2201    2202    2203    2204    2205    2206    2207    2208    2210    2211    2212    2213    2214    6            �           0    0    TABLE pga_schedule    COMMENT     <   COMMENT ON TABLE pga_schedule IS 'Job schedule exceptions';
            pgagent       postgres    false    1764            �           1259    234953    pga_schedule_jscid_seq    SEQUENCE     x   CREATE SEQUENCE pga_schedule_jscid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_schedule_jscid_seq;
       pgagent       postgres    false    6    1764            �           0    0    pga_schedule_jscid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_schedule_jscid_seq OWNED BY pga_schedule.jscid;
            pgagent       postgres    false    1765            �           0    0    pga_schedule_jscid_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('pga_schedule_jscid_seq', 1, false);
            pgagent       postgres    false    1765                       1259    245147    base_UOM    TABLE     �  CREATE TABLE "base_UOM" (
    "Id" integer NOT NULL,
    "Code" character varying(10) NOT NULL,
    "Name" character varying(30) NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now(),
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now(),
    "UserUpdated" character varying(30),
    "IsActived" boolean NOT NULL,
    "Resource" uuid DEFAULT newid()
);
    DROP TABLE public."base_UOM";
       public         postgres    false    2295    2296    2297    7            c           1259    271777    TestView    VIEW       CREATE VIEW "TestView" AS
    SELECT "base_UOM"."Id", "base_UOM"."Code", "base_UOM"."Name", "base_UOM"."DateCreated", "base_UOM"."UserCreated", "base_UOM"."DateUpdated", "base_UOM"."UserUpdated", "base_UOM"."IsActived", "base_UOM"."Resource" FROM "base_UOM";
    DROP VIEW public."TestView";
       public       postgres    false    1982    7            �           1259    244946    base_Attachment    TABLE       CREATE TABLE "base_Attachment" (
    "Id" bigint NOT NULL,
    "FileOriginalName" character varying(20) NOT NULL,
    "FileName" character varying(250) NOT NULL,
    "FileExtension" character varying(5),
    "VirtualFolderId" integer NOT NULL,
    "IsActived" boolean NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now(),
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now(),
    "UserUpdated" character varying(30),
    "Counter" smallint DEFAULT 0 NOT NULL
);
 %   DROP TABLE public."base_Attachment";
       public         postgres    false    2272    2273    2274    7            �           1259    244944    base_Attachment_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Attachment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Attachment_Id_seq";
       public       postgres    false    7    1784            �           0    0    base_Attachment_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Attachment_Id_seq" OWNED BY "base_Attachment"."Id";
            public       postgres    false    1783            �           0    0    base_Attachment_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_Attachment_Id_seq"', 40012, true);
            public       postgres    false    1783            /           1259    256168    base_Authorize    TABLE     �   CREATE TABLE "base_Authorize" (
    "Id" bigint NOT NULL,
    "Resource" character varying(36) NOT NULL,
    "Code" character varying(10) NOT NULL
);
 $   DROP TABLE public."base_Authorize";
       public         postgres    false    7            .           1259    256166    base_Authorize_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Authorize_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Authorize_Id_seq";
       public       postgres    false    7    1839            �           0    0    base_Authorize_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Authorize_Id_seq" OWNED BY "base_Authorize"."Id";
            public       postgres    false    1838            �           0    0    base_Authorize_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_Authorize_Id_seq"', 289, true);
            public       postgres    false    1838                       1259    254557    base_Configuration    TABLE     0	  CREATE TABLE "base_Configuration" (
    "CompanyName" character varying(200),
    "Address" character varying(150),
    "City" character varying(30),
    "State" smallint,
    "ZipCode" character varying(15),
    "CountryId" smallint,
    "Phone" character varying(20),
    "Fax" character varying(20),
    "Email" character varying(30),
    "Website" character varying(30),
    "EmailPop3Server" character varying(100),
    "EmailPop3Port" integer,
    "EmailAccount" character varying(30),
    "EmailPassword" character varying(100),
    "IsBarcodeScannerAttached" boolean,
    "IsEnableTouchScreenLayout" boolean,
    "IsAllowTimeClockAttached" boolean,
    "IsAllowCollectTipCreditCard" boolean,
    "IsAllowMutilUOM" boolean,
    "DefaultMaximumSticky" integer DEFAULT 0,
    "DefaultPriceSchema" smallint DEFAULT 0,
    "DefaultPaymentMethod" smallint DEFAULT 0,
    "DefaultSaleTaxLocation" smallint DEFAULT 0,
    "DefaultTaxCodeNewDepartment" character(3),
    "DefautlImagePath" character varying(300),
    "DefautlDiscountScheduleTime" integer DEFAULT 12 NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now(),
    "UserCreated" character varying(30),
    "TotalStore" smallint DEFAULT 1,
    "IsRequirePromotionCode" boolean DEFAULT false,
    "DefaultDiscountType" smallint DEFAULT 0 NOT NULL,
    "DefaultDiscountStatus" smallint DEFAULT 0 NOT NULL,
    "LoginAllow" smallint,
    "Logo" bytea,
    "DefaultScanMethod" smallint,
    "TipPercent" numeric(4,2) DEFAULT 0 NOT NULL,
    "AcceptedPaymentMethod" integer,
    "AcceptedCardType" integer,
    "IsRequireDiscountReason" boolean DEFAULT true,
    "WorkHour" smallint DEFAULT 8 NOT NULL,
    "Id" integer NOT NULL,
    "DefaultShipUnit" smallint DEFAULT 0,
    "DefaultCashiedUserName" boolean DEFAULT false,
    "KeepLog" smallint DEFAULT 7,
    "IsAllowShift" boolean DEFAULT false NOT NULL,
    "DefaultLanguage" character varying(2),
    "TimeOutMinute" integer DEFAULT 0,
    "IsAutoLogout" boolean DEFAULT false,
    "IsBackupWhenExit" boolean DEFAULT false,
    "BackupEvery" integer DEFAULT 0,
    "BackupPath" character varying(300),
    "IsAllowRGO" boolean,
    "PasswordLength" smallint DEFAULT 8,
    "IsAllowChangeOrder" boolean DEFAULT false,
    "IsAllowNegativeStore" boolean DEFAULT false,
    "AcceptedGiftCardMethod" integer DEFAULT 0 NOT NULL
);
 (   DROP TABLE public."base_Configuration";
       public         postgres    false    2365    2366    2367    2368    2369    2370    2371    2372    2373    2374    2375    2376    2377    2378    2380    2381    2382    2383    2384    2385    2386    2387    2388    2389    2390    7            �           0    0 .   COLUMN "base_Configuration"."DefautlImagePath"    COMMENT     T   COMMENT ON COLUMN "base_Configuration"."DefautlImagePath" IS 'Apply to Attachment';
            public       postgres    false    1819            �           0    0 9   COLUMN "base_Configuration"."DefautlDiscountScheduleTime"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."DefautlDiscountScheduleTime" IS 'Apply to Discount Schedule Time';
            public       postgres    false    1819            �           0    0 (   COLUMN "base_Configuration"."LoginAllow"    COMMENT     \   COMMENT ON COLUMN "base_Configuration"."LoginAllow" IS 'So lan cho phep neu dang nhap sai';
            public       postgres    false    1819            �           0    0 5   COLUMN "base_Configuration"."IsRequireDiscountReason"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRequireDiscountReason" IS 'Reason box apprear when changing deactive to active status';
            public       postgres    false    1819            �           0    0 -   COLUMN "base_Configuration"."DefaultShipUnit"    COMMENT     f   COMMENT ON COLUMN "base_Configuration"."DefaultShipUnit" IS 'Don vi tinh trong luong khi van chuyen';
            public       postgres    false    1819            �           0    0 +   COLUMN "base_Configuration"."TimeOutMinute"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."TimeOutMinute" IS 'The time out application';
            public       postgres    false    1819            �           0    0 *   COLUMN "base_Configuration"."IsAutoLogout"    COMMENT     U   COMMENT ON COLUMN "base_Configuration"."IsAutoLogout" IS 'Combine to TimeOutMinute';
            public       postgres    false    1819            �           0    0 .   COLUMN "base_Configuration"."IsBackupWhenExit"    COMMENT     ]   COMMENT ON COLUMN "base_Configuration"."IsBackupWhenExit" IS 'Backup when exit application';
            public       postgres    false    1819            �           0    0 )   COLUMN "base_Configuration"."BackupEvery"    COMMENT     R   COMMENT ON COLUMN "base_Configuration"."BackupEvery" IS 'The time when back up ';
            public       postgres    false    1819            �           0    0 (   COLUMN "base_Configuration"."IsAllowRGO"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsAllowRGO" IS 'Is allow receive the quantity more than order quantity';
            public       postgres    false    1819            4           1259    257302    base_Configuration_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_Configuration_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_Configuration_Id_seq";
       public       postgres    false    7    1819            �           0    0    base_Configuration_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_Configuration_Id_seq" OWNED BY "base_Configuration"."Id";
            public       postgres    false    1844            �           0    0    base_Configuration_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_Configuration_Id_seq"', 3, true);
            public       postgres    false    1844                       1259    245754    base_CostAdjustment    TABLE     �  CREATE TABLE "base_CostAdjustment" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "NewCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "OldCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "ItemCount" integer DEFAULT 0,
    "LoggedTime" timestamp without time zone NOT NULL,
    "Reason" character varying(30) NOT NULL,
    "StoreNumber" integer
);
 )   DROP TABLE public."base_CostAdjustment";
       public         postgres    false    2352    2353    2354    2355    2356    7            �           0    0 -   COLUMN "base_CostAdjustment"."CostDifference"    COMMENT     Q   COMMENT ON COLUMN "base_CostAdjustment"."CostDifference" IS 'NewCost - OldCost';
            public       postgres    false    1814            �           0    0 &   COLUMN "base_CostAdjustment"."NewCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."NewCost" IS 'NewCost*NewQty';
            public       postgres    false    1814            �           0    0 &   COLUMN "base_CostAdjustment"."OldCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."OldCost" IS 'OldCost*OldQty';
            public       postgres    false    1814            �           0    0 (   COLUMN "base_CostAdjustment"."ItemCount"    COMMENT     ]   COMMENT ON COLUMN "base_CostAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1814            �           0    0 )   COLUMN "base_CostAdjustment"."LoggedTime"    COMMENT     w   COMMENT ON COLUMN "base_CostAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
            public       postgres    false    1814                       1259    245766    base_CostAdjustmentItem    TABLE     �  CREATE TABLE "base_CostAdjustmentItem" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductCode" character varying(20) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentNewCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentOldCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "LoggedTime" timestamp without time zone DEFAULT now() NOT NULL,
    "ParentResource" character varying(36) NOT NULL
);
 -   DROP TABLE public."base_CostAdjustmentItem";
       public         postgres    false    2357    2359    2360    2361    2362    7            �           0    0 1   COLUMN "base_CostAdjustmentItem"."CostDifference"    COMMENT     i   COMMENT ON COLUMN "base_CostAdjustmentItem"."CostDifference" IS 'AdjustmentNewCost - AdjustmentOldCost';
            public       postgres    false    1816                       1259    245764    base_CostAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CostAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_CostAdjustmentItem_Id_seq";
       public       postgres    false    7    1816            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_CostAdjustmentItem_Id_seq" OWNED BY "base_CostAdjustmentItem"."Id";
            public       postgres    false    1815            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_CostAdjustmentItem_Id_seq"', 1, false);
            public       postgres    false    1815                       1259    245752    base_CostAdjustment_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_CostAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_CostAdjustment_Id_seq";
       public       postgres    false    1814    7            �           0    0    base_CostAdjustment_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_CostAdjustment_Id_seq" OWNED BY "base_CostAdjustment"."Id";
            public       postgres    false    1813            �           0    0    base_CostAdjustment_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_CostAdjustment_Id_seq"', 1, false);
            public       postgres    false    1813            `           1259    271738    base_CountStock    TABLE     �  CREATE TABLE "base_CountStock" (
    "Id" bigint NOT NULL,
    "DocumentNo" character varying(12) NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30) NOT NULL,
    "CompletedDate" timestamp without time zone,
    "UserCounted" character varying(30),
    "Status" smallint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL
);
 %   DROP TABLE public."base_CountStock";
       public         postgres    false    2567    2568    7            �           0    0 !   COLUMN "base_CountStock"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_CountStock"."Status" IS 'Get from "CountStockStatus" tag in XML';
            public       postgres    false    1888            b           1259    271745    base_CountStockDetail    TABLE     F  CREATE TABLE "base_CountStockDetail" (
    "Id" bigint NOT NULL,
    "CountStockId" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "StoreId" smallint DEFAULT 0 NOT NULL,
    "Quantity" integer DEFAULT 0 NOT NULL,
    "CountedQuantity" integer DEFAULT 0 NOT NULL
);
 +   DROP TABLE public."base_CountStockDetail";
       public         postgres    false    2570    2571    2572    7            a           1259    271743    base_CountStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CountStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_CountStockDetail_Id_seq";
       public       postgres    false    7    1890            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CountStockDetail_Id_seq" OWNED BY "base_CountStockDetail"."Id";
            public       postgres    false    1889            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CountStockDetail_Id_seq"', 125, true);
            public       postgres    false    1889            _           1259    271736    base_CountStock_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_CountStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_CountStock_Id_seq";
       public       postgres    false    7    1888            �           0    0    base_CountStock_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_CountStock_Id_seq" OWNED BY "base_CountStock"."Id";
            public       postgres    false    1887            �           0    0    base_CountStock_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_CountStock_Id_seq"', 27, true);
            public       postgres    false    1887                       1259    245340    base_Department    TABLE       CREATE TABLE "base_Department" (
    "Id" integer NOT NULL,
    "Name" character varying(200),
    "ParentId" integer DEFAULT 0,
    "TaxCodeId" character(3),
    "Margin" numeric(4,2) DEFAULT 0 NOT NULL,
    "MarkUp" numeric(4,2) DEFAULT 0 NOT NULL,
    "LevelId" smallint DEFAULT 0 NOT NULL,
    "IsActived" boolean DEFAULT false,
    "UserCreated" character varying(30),
    "UserUpdated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "DateUpdated" timestamp without time zone DEFAULT now()
);
 %   DROP TABLE public."base_Department";
       public         postgres    false    2317    2318    2319    2320    2321    2322    2323    7            �           0    0    TABLE "base_Department"    COMMENT     ,   COMMENT ON TABLE "base_Department" IS '

';
            public       postgres    false    1804            �           0    0 "   COLUMN "base_Department"."LevelId"    COMMENT     8   COMMENT ON COLUMN "base_Department"."LevelId" IS 'ddd';
            public       postgres    false    1804                       1259    245338    base_Department_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Department_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Department_Id_seq";
       public       postgres    false    7    1804            �           0    0    base_Department_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Department_Id_seq" OWNED BY "base_Department"."Id";
            public       postgres    false    1803            �           0    0    base_Department_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Department_Id_seq"', 395, true);
            public       postgres    false    1803            �           1259    238237 
   base_Email    TABLE     �  CREATE TABLE "base_Email" (
    "Id" uuid NOT NULL,
    "Recipient" character varying(200),
    "CC" character varying(200),
    "BCC" character varying(200),
    "Subject" character varying(200),
    "Body" text,
    "IsHasAttachment" boolean DEFAULT false NOT NULL,
    "UserCreated" character varying(30),
    "DateCreated" character varying(30) DEFAULT now(),
    "UserUpdated" character varying(30),
    "DateUpdated" character varying(30) DEFAULT now(),
    "AttachmentType" character varying(20),
    "AttachmentResult" character varying(20),
    "GuestId" integer,
    "Sender" character varying(30),
    "Status" smallint DEFAULT 0,
    "Importance" smallint,
    "Sensitivity" smallint,
    "IsRequestDelivery" boolean DEFAULT false NOT NULL,
    "IsRequestRead" boolean DEFAULT false NOT NULL,
    "IsMyFlag" boolean,
    "FlagTo" smallint,
    "FlagStartDate" integer DEFAULT 0,
    "FlagDueDate" integer,
    "IsAllowReminder" boolean DEFAULT false,
    "RemindOn" timestamp without time zone,
    "MyRemindTimes" smallint DEFAULT 0,
    "IsRecipentFlag" boolean,
    "RecipentFlagTo" smallint,
    "IsAllowRecipentReminder" boolean DEFAULT false,
    "RecipentRemindOn" timestamp without time zone,
    "RecipentRemindTimes" smallint DEFAULT 0
);
     DROP TABLE public."base_Email";
       public         postgres    false    2215    2216    2217    2218    2219    2220    2221    2222    2223    2224    2225    7            �           0    0 %   COLUMN "base_Email"."IsHasAttachment"    COMMENT     p   COMMENT ON COLUMN "base_Email"."IsHasAttachment" IS 'Nếu có file đính kèm thì sẽ bật lên là true';
            public       postgres    false    1767            �           0    0 $   COLUMN "base_Email"."AttachmentType"    COMMENT     [   COMMENT ON COLUMN "base_Email"."AttachmentType" IS 'Sử dụng khi IsHasAttachment=true';
            public       postgres    false    1767            �           0    0 &   COLUMN "base_Email"."AttachmentResult"    COMMENT     y   COMMENT ON COLUMN "base_Email"."AttachmentResult" IS 'Sử dụng khi IsHasAttachment=true và phụ thuộc vào Type';
            public       postgres    false    1767            �           0    0    COLUMN "base_Email"."Sender"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Sender" IS 'Thông tin người gủi dựa và GuestId';
            public       postgres    false    1767            �           0    0    COLUMN "base_Email"."Status"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Status" IS '0: Outbox
1: Inbox
2: Sent
3: Draft
4: Trash';
            public       postgres    false    1767            �           0    0     COLUMN "base_Email"."Importance"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Importance" IS 'Message Option
0: Normal
1: Importance
';
            public       postgres    false    1767            �           0    0 !   COLUMN "base_Email"."Sensitivity"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Sensitivity" IS 'Message Option
0: Personal
1: Bussiness';
            public       postgres    false    1767            �           0    0 '   COLUMN "base_Email"."IsRequestDelivery"    COMMENT     o   COMMENT ON COLUMN "base_Email"."IsRequestDelivery" IS 'Message Option
Request a delivery receipt for message';
            public       postgres    false    1767            �           0    0 #   COLUMN "base_Email"."IsRequestRead"    COMMENT     g   COMMENT ON COLUMN "base_Email"."IsRequestRead" IS 'Message Option
Request a read receipt for message';
            public       postgres    false    1767            �           0    0    COLUMN "base_Email"."IsMyFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsMyFlag" IS 'Custom Reminder Active Flag For Me';
            public       postgres    false    1767            �           0    0    COLUMN "base_Email"."FlagTo"    COMMENT     >   COMMENT ON COLUMN "base_Email"."FlagTo" IS 'My Flag Options';
            public       postgres    false    1767            �           0    0 #   COLUMN "base_Email"."FlagStartDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagStartDate" IS 'Active My Flag Date';
            public       postgres    false    1767            �           0    0 !   COLUMN "base_Email"."FlagDueDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagDueDate" IS 'DeActive My Flag Date';
            public       postgres    false    1767            �           0    0 %   COLUMN "base_Email"."IsAllowReminder"    COMMENT     L   COMMENT ON COLUMN "base_Email"."IsAllowReminder" IS 'Allow remind my flag';
            public       postgres    false    1767            �           0    0    COLUMN "base_Email"."RemindOn"    COMMENT     X   COMMENT ON COLUMN "base_Email"."RemindOn" IS 'My Flag is going to remind on this date';
            public       postgres    false    1767            �           0    0 #   COLUMN "base_Email"."MyRemindTimes"    COMMENT     H   COMMENT ON COLUMN "base_Email"."MyRemindTimes" IS 'The reminder times';
            public       postgres    false    1767            �           0    0 $   COLUMN "base_Email"."IsRecipentFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsRecipentFlag" IS 'Custom Reminder For Recipent';
            public       postgres    false    1767            �           0    0 $   COLUMN "base_Email"."RecipentFlagTo"    COMMENT     L   COMMENT ON COLUMN "base_Email"."RecipentFlagTo" IS 'Recipent Flag Options';
            public       postgres    false    1767            �           0    0 -   COLUMN "base_Email"."IsAllowRecipentReminder"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."IsAllowRecipentReminder" IS 'Allow remind Recipent Flag';
            public       postgres    false    1767            �           0    0 &   COLUMN "base_Email"."RecipentRemindOn"    COMMENT     f   COMMENT ON COLUMN "base_Email"."RecipentRemindOn" IS 'Recipent Flag is going to remind on this date';
            public       postgres    false    1767            �           0    0 )   COLUMN "base_Email"."RecipentRemindTimes"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."RecipentRemindTimes" IS 'The Reminder Times of Recipent';
            public       postgres    false    1767            �           1259    238137    base_EmailAttachment    TABLE     p   CREATE TABLE "base_EmailAttachment" (
    "Id" uuid NOT NULL,
    "EmailId" uuid,
    "AttachmentId" integer
);
 *   DROP TABLE public."base_EmailAttachment";
       public         postgres    false    7            �           1259    244817 
   base_Guest    TABLE     �  CREATE TABLE "base_Guest" (
    "Id" bigint NOT NULL,
    "FirstName" character varying(20),
    "MiddleName" character(2),
    "LastName" character varying(20),
    "Company" character varying(100),
    "Phone1" character varying(20),
    "Ext1" character(6),
    "Phone2" character varying(20),
    "Ext2" character(6),
    "Fax" character varying(14),
    "CellPhone" character varying(14),
    "Email" character varying(30),
    "Website" character varying(30),
    "UserCreated" character varying(30),
    "UserUpdated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "DateUpdated" timestamp without time zone DEFAULT now(),
    "IsPurged" boolean DEFAULT false NOT NULL,
    "GuestTypeId" smallint,
    "IsActived" boolean DEFAULT false NOT NULL,
    "GuestNo" character varying(12),
    "PositionId" smallint DEFAULT 0,
    "Department" character varying(30),
    "Mark" character(1),
    "AccountNumber" character varying(30),
    "ParentId" bigint,
    "IsRewardMember" boolean DEFAULT false NOT NULL,
    "CheckLimit" numeric(15,2) DEFAULT 0 NOT NULL,
    "CreditLimit" numeric(15,2) DEFAULT 0 NOT NULL,
    "BalanceDue" numeric(15,2) DEFAULT 0 NOT NULL,
    "AvailCredit" numeric(15,2) DEFAULT 0 NOT NULL,
    "PastDue" numeric(15,2) DEFAULT 0 NOT NULL,
    "IsPrimary" boolean DEFAULT false NOT NULL,
    "CommissionPercent" numeric(4,2) DEFAULT 0 NOT NULL,
    "Resource" uuid DEFAULT newid(),
    "TotalRewardRedeemed" numeric(15,2) DEFAULT 0 NOT NULL,
    "PurchaseDuringTrackingPeriod" numeric(15,2) DEFAULT 0 NOT NULL,
    "RequirePurchaseNextReward" numeric(15,2) DEFAULT 0 NOT NULL,
    "HireDate" timestamp without time zone,
    "IsBlockArriveLate" boolean DEFAULT false NOT NULL,
    "IsDeductLunchTime" boolean DEFAULT false NOT NULL,
    "IsBalanceOvertime" boolean DEFAULT false NOT NULL,
    "LateMinutes" smallint DEFAULT 0 NOT NULL,
    "OvertimeOption" integer DEFAULT 0 NOT NULL,
    "OTLeastMinute" smallint DEFAULT 0 NOT NULL,
    "IsTrackingHour" boolean DEFAULT false NOT NULL,
    "TermDiscount" numeric(4,2) DEFAULT 0 NOT NULL,
    "TermNetDue" smallint DEFAULT 0 NOT NULL,
    "TermPaidWithinDay" smallint DEFAULT 0 NOT NULL,
    "PaymentTermDescription" character varying(30),
    "SaleRepId" bigint
);
     DROP TABLE public."base_Guest";
       public         postgres    false    2228    2229    2230    2231    2232    2233    2234    2235    2236    2237    2239    2240    2241    2242    2243    2244    2245    2246    2247    2248    2249    2250    2251    2252    2253    2254    2255    7            �           0    0    COLUMN "base_Guest"."GuestNo"    COMMENT     <   COMMENT ON COLUMN "base_Guest"."GuestNo" IS 'YYMMDDHHMMSS';
            public       postgres    false    1772            �           0    0     COLUMN "base_Guest"."PositionId"    COMMENT     >   COMMENT ON COLUMN "base_Guest"."PositionId" IS 'Chức vụ';
            public       postgres    false    1772            �           0    0     COLUMN "base_Guest"."Department"    COMMENT     =   COMMENT ON COLUMN "base_Guest"."Department" IS 'Phòng ban';
            public       postgres    false    1772            �           0    0    COLUMN "base_Guest"."Mark"    COMMENT     [   COMMENT ON COLUMN "base_Guest"."Mark" IS '-- E: Employee C: Company V: Vendor O: Contact';
            public       postgres    false    1772            �           0    0    COLUMN "base_Guest"."IsPrimary"    COMMENT     ^   COMMENT ON COLUMN "base_Guest"."IsPrimary" IS 'Áp dụng nếu đối tượng là contact';
            public       postgres    false    1772            �           0    0 '   COLUMN "base_Guest"."CommissionPercent"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."CommissionPercent" IS 'Apply khi Mark = E';
            public       postgres    false    1772            �           0    0 )   COLUMN "base_Guest"."TotalRewardRedeemed"    COMMENT     o   COMMENT ON COLUMN "base_Guest"."TotalRewardRedeemed" IS 'Total reward redeemed earned during tracking period';
            public       postgres    false    1772            �           0    0 2   COLUMN "base_Guest"."PurchaseDuringTrackingPeriod"    COMMENT     `   COMMENT ON COLUMN "base_Guest"."PurchaseDuringTrackingPeriod" IS '= Total(SaleOrderSubAmount)';
            public       postgres    false    1772            �           0    0 /   COLUMN "base_Guest"."RequirePurchaseNextReward"    COMMENT     �   COMMENT ON COLUMN "base_Guest"."RequirePurchaseNextReward" IS 'F = RewardAmount - PurchaseDuringTrackingPeriod Mod RewardAmount';
            public       postgres    false    1772            �           0    0 '   COLUMN "base_Guest"."IsBlockArriveLate"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBlockArriveLate" IS '-- Apply to TimeClock';
            public       postgres    false    1772            �           0    0 '   COLUMN "base_Guest"."IsDeductLunchTime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsDeductLunchTime" IS '-- Apply to TimeClock';
            public       postgres    false    1772            �           0    0 '   COLUMN "base_Guest"."IsBalanceOvertime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBalanceOvertime" IS '-- Apply to TimeClock';
            public       postgres    false    1772            �           0    0 !   COLUMN "base_Guest"."LateMinutes"    COMMENT     I   COMMENT ON COLUMN "base_Guest"."LateMinutes" IS '-- Apply to TimeClock';
            public       postgres    false    1772            �           0    0 $   COLUMN "base_Guest"."OvertimeOption"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."OvertimeOption" IS '-- Apply to TimeClock';
            public       postgres    false    1772                        0    0 #   COLUMN "base_Guest"."OTLeastMinute"    COMMENT     K   COMMENT ON COLUMN "base_Guest"."OTLeastMinute" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0    COLUMN "base_Guest"."SaleRepId"    COMMENT     C   COMMENT ON COLUMN "base_Guest"."SaleRepId" IS 'Apply to customer';
            public       postgres    false    1772                       1259    245376    base_GuestAdditional    TABLE        CREATE TABLE "base_GuestAdditional" (
    "Id" integer NOT NULL,
    "TaxRate" numeric(10,2),
    "IsNoDiscount" boolean,
    "FixDiscount" numeric(10,2) DEFAULT 0,
    "Unit" smallint,
    "PriceSchemeId" smallint,
    "Custom1" character varying(30),
    "Custom2" character varying(30),
    "Custom3" character varying(30),
    "Custom4" character varying(30),
    "Custom5" character varying(30),
    "Custom6" character varying(30),
    "Custom7" character varying(30),
    "Custom8" character varying(30),
    "GuestId" bigint,
    "LayawayNo" character varying(20),
    "ChargeACNo" character varying(20),
    "FedTaxId" character varying(20),
    "IsTaxExemption" boolean DEFAULT false NOT NULL,
    "SaleTaxLocation" integer DEFAULT 0 NOT NULL,
    "TaxExemptionNo" character varying(20)
);
 *   DROP TABLE public."base_GuestAdditional";
       public         postgres    false    2325    2326    2327    7                       0    0 $   COLUMN "base_GuestAdditional"."Unit"    COMMENT     K   COMMENT ON COLUMN "base_GuestAdditional"."Unit" IS '0: Amount 1: Percent';
            public       postgres    false    1806                       0    0 .   COLUMN "base_GuestAdditional"."IsTaxExemption"    COMMENT     N   COMMENT ON COLUMN "base_GuestAdditional"."IsTaxExemption" IS 'Miễn thuế';
            public       postgres    false    1806                       0    0 .   COLUMN "base_GuestAdditional"."TaxExemptionNo"    COMMENT     a   COMMENT ON COLUMN "base_GuestAdditional"."TaxExemptionNo" IS 'Require if IsTaxExemption = true';
            public       postgres    false    1806                       1259    245374    base_GuestAdditional_Id_seq    SEQUENCE        CREATE SEQUENCE "base_GuestAdditional_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_GuestAdditional_Id_seq";
       public       postgres    false    1806    7                       0    0    base_GuestAdditional_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_GuestAdditional_Id_seq" OWNED BY "base_GuestAdditional"."Id";
            public       postgres    false    1805                       0    0    base_GuestAdditional_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestAdditional_Id_seq"', 102, true);
            public       postgres    false    1805            �           1259    244863    base_GuestAddress    TABLE     �  CREATE TABLE "base_GuestAddress" (
    "Id" integer NOT NULL,
    "GuestId" bigint NOT NULL,
    "AddressTypeId" integer NOT NULL,
    "AddressLine1" character varying(60) NOT NULL,
    "AddressLine2" character varying(60),
    "City" character varying(30) NOT NULL,
    "StateProvinceId" integer NOT NULL,
    "PostalCode" character varying(15),
    "CountryId" integer NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30),
    "IsDefault" boolean DEFAULT false NOT NULL
);
 '   DROP TABLE public."base_GuestAddress";
       public         postgres    false    2257    2258    2259    7            �           1259    244861    base_GuestAddress_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestAddress_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestAddress_Id_seq";
       public       postgres    false    1774    7                       0    0    base_GuestAddress_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestAddress_Id_seq" OWNED BY "base_GuestAddress"."Id";
            public       postgres    false    1773                       0    0    base_GuestAddress_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestAddress_Id_seq"', 218, true);
            public       postgres    false    1773            �           1259    238413    base_GuestFingerPrint    TABLE     3  CREATE TABLE "base_GuestFingerPrint" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "FingerIndex" integer NOT NULL,
    "HandFlag" boolean NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdaed" character varying(30),
    "FingerPrintImage" bytea
);
 +   DROP TABLE public."base_GuestFingerPrint";
       public         postgres    false    2226    7            �           1259    238411    base_GuestFingerPrint_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestFingerPrint_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestFingerPrint_Id_seq";
       public       postgres    false    7    1769            	           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestFingerPrint_Id_seq" OWNED BY "base_GuestFingerPrint"."Id";
            public       postgres    false    1768            
           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestFingerPrint_Id_seq"', 12, true);
            public       postgres    false    1768            �           1259    244873    base_GuestHiringHistory    TABLE     Q  CREATE TABLE "base_GuestHiringHistory" (
    "Id" bigint NOT NULL,
    "GuestId" bigint DEFAULT 0,
    "StartDate" timestamp with time zone,
    "RenewDate" timestamp without time zone,
    "PromotionDate" timestamp without time zone,
    "TerminateDate" timestamp without time zone,
    "IsTerminate" boolean,
    "ManagerId" bigint
);
 -   DROP TABLE public."base_GuestHiringHistory";
       public         postgres    false    2261    7            �           1259    244871    base_GuestHiringHistory_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestHiringHistory_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_GuestHiringHistory_Id_seq";
       public       postgres    false    7    1776                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_GuestHiringHistory_Id_seq" OWNED BY "base_GuestHiringHistory"."Id";
            public       postgres    false    1775                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_GuestHiringHistory_Id_seq"', 1, false);
            public       postgres    false    1775            �           1259    244884    base_GuestPayRoll    TABLE     �  CREATE TABLE "base_GuestPayRoll" (
    "Id" integer NOT NULL,
    "PayrollName" character varying(20),
    "PayrollType" character(1),
    "Rate" numeric(12,0) DEFAULT 0 NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30),
    "GuestId" bigint
);
 '   DROP TABLE public."base_GuestPayRoll";
       public         postgres    false    2263    2264    2265    7            �           1259    244882    base_GuestPayRoll_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestPayRoll_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestPayRoll_Id_seq";
       public       postgres    false    7    1778                       0    0    base_GuestPayRoll_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestPayRoll_Id_seq" OWNED BY "base_GuestPayRoll"."Id";
            public       postgres    false    1777                       0    0    base_GuestPayRoll_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_GuestPayRoll_Id_seq"', 1, false);
            public       postgres    false    1777            6           1259    257325    base_GuestPaymentCard    TABLE     Z  CREATE TABLE "base_GuestPaymentCard" (
    "Id" integer NOT NULL,
    "GuestId" bigint,
    "CardTypeId" smallint NOT NULL,
    "CardNumber" character varying(25),
    "ExpMonth" smallint,
    "ExpYear" smallint NOT NULL,
    "CCID" character varying(5),
    "BillingAddress" character varying(200),
    "NameOnCard" character varying(100),
    "ZipCode" character varying(15),
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30)
);
 +   DROP TABLE public."base_GuestPaymentCard";
       public         postgres    false    2408    2409    7            5           1259    257323    base_GuestPaymentCard_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestPaymentCard_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestPaymentCard_Id_seq";
       public       postgres    false    7    1846                       0    0    base_GuestPaymentCard_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestPaymentCard_Id_seq" OWNED BY "base_GuestPaymentCard"."Id";
            public       postgres    false    1845                       0    0    base_GuestPaymentCard_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestPaymentCard_Id_seq"', 14, true);
            public       postgres    false    1845            �           1259    244922    base_ResourcePhoto    TABLE        CREATE TABLE "base_ResourcePhoto" (
    "Id" integer NOT NULL,
    "ThumbnailPhoto" bytea,
    "ThumbnailPhotoFilename" character varying(60),
    "LargePhoto" bytea,
    "LargePhotoFilename" character varying(60),
    "SortId" smallint DEFAULT 0,
    "Resource" character varying(36)
);
 (   DROP TABLE public."base_ResourcePhoto";
       public         postgres    false    2267    7            �           1259    244920    base_GuestPhoto_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_GuestPhoto_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_GuestPhoto_Id_seq";
       public       postgres    false    7    1780                       0    0    base_GuestPhoto_Id_seq    SEQUENCE OWNED BY     L   ALTER SEQUENCE "base_GuestPhoto_Id_seq" OWNED BY "base_ResourcePhoto"."Id";
            public       postgres    false    1779                       0    0    base_GuestPhoto_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_GuestPhoto_Id_seq"', 231, true);
            public       postgres    false    1779            �           1259    244934    base_GuestProfile    TABLE     �  CREATE TABLE "base_GuestProfile" (
    "Id" bigint NOT NULL,
    "Gender" smallint,
    "Marital" smallint,
    "SSN" character varying(20),
    "Identification" character varying(20),
    "DOB" timestamp without time zone,
    "IsSpouse" boolean DEFAULT false,
    "FirstName" character varying(30),
    "LastName" character varying(30),
    "MiddleName" character(1),
    "State" character(3),
    "SGender" smallint,
    "SFirstName" character varying(30),
    "SLastName" character varying(30),
    "SMiddleName" character(1),
    "SPhone" character varying(20),
    "SCellPhone" character varying(20),
    "SSSN" character varying(20),
    "SState" character(3),
    "SEmail" character varying(30),
    "IsEmergency" boolean DEFAULT false,
    "EFirstName" character varying(30),
    "ELastName" character varying(30),
    "EMiddleName" character(1),
    "EPhone" character varying(20),
    "ECellPhone" character varying(20),
    "ERelationship" character varying(30),
    "GuestId" bigint
);
 '   DROP TABLE public."base_GuestProfile";
       public         postgres    false    2269    2270    7            �           1259    244932    base_GuestProfile_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestProfile_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestProfile_Id_seq";
       public       postgres    false    7    1782                       0    0    base_GuestProfile_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestProfile_Id_seq" OWNED BY "base_GuestProfile"."Id";
            public       postgres    false    1781                       0    0    base_GuestProfile_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestProfile_Id_seq"', 151, true);
            public       postgres    false    1781            N           1259    268354    base_GuestReward    TABLE     �  CREATE TABLE "base_GuestReward" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "RewardId" integer NOT NULL,
    "Amount" numeric(15,2) DEFAULT 0 NOT NULL,
    "IsApply" boolean DEFAULT false NOT NULL,
    "EearnedDate" timestamp without time zone,
    "RedeemedDate" timestamp without time zone,
    "RewardValue" numeric(15,2) DEFAULT 0 NOT NULL,
    "SaleOrderResource" character varying(36),
    "SaleOrderNo" character varying(15),
    "Remark" character varying(30) NOT NULL
);
 &   DROP TABLE public."base_GuestReward";
       public         postgres    false    2501    2502    2503    7            M           1259    268352    base_GuestReward_Id_seq    SEQUENCE     {   CREATE SEQUENCE "base_GuestReward_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."base_GuestReward_Id_seq";
       public       postgres    false    7    1870                       0    0    base_GuestReward_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE "base_GuestReward_Id_seq" OWNED BY "base_GuestReward"."Id";
            public       postgres    false    1869                       0    0    base_GuestReward_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestReward_Id_seq"', 2056, true);
            public       postgres    false    1869            -           1259    256013    base_GuestSchedule    TABLE     �   CREATE TABLE "base_GuestSchedule" (
    "GuestId" bigint NOT NULL,
    "WorkScheduleId" integer NOT NULL,
    "StartDate" timestamp without time zone NOT NULL,
    "AssignDate" timestamp without time zone NOT NULL,
    "Status" integer NOT NULL
);
 (   DROP TABLE public."base_GuestSchedule";
       public         postgres    false    7            �           1259    244815    base_Guest_Id_seq    SEQUENCE     u   CREATE SEQUENCE "base_Guest_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."base_Guest_Id_seq";
       public       postgres    false    1772    7                       0    0    base_Guest_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Guest_Id_seq" OWNED BY "base_Guest"."Id";
            public       postgres    false    1771                       0    0    base_Guest_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"base_Guest_Id_seq"', 275, true);
            public       postgres    false    1771            �           1259    244997    base_MemberShip    TABLE       CREATE TABLE "base_MemberShip" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "MemberType" character(1),
    "CardNumber" character varying(30),
    "Status" smallint NOT NULL,
    "IsPurged" boolean NOT NULL,
    "UserCreated" character varying(30),
    "UserUpdated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "Code" character varying(30),
    "TotalRewardRedeemed" numeric
);
 %   DROP TABLE public."base_MemberShip";
       public         postgres    false    2276    2277    7                       0    0 %   COLUMN "base_MemberShip"."MemberType"    COMMENT     f   COMMENT ON COLUMN "base_MemberShip"."MemberType" IS 'P = Platium, G = Gold, S = Silver, B = Bronze.';
            public       postgres    false    1786                       0    0 !   COLUMN "base_MemberShip"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_MemberShip"."Status" IS '-1 = Pending
0 = DeActived
1 = Actived';
            public       postgres    false    1786            �           1259    244995    base_MemberShip_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_MemberShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_MemberShip_Id_seq";
       public       postgres    false    1786    7                       0    0    base_MemberShip_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_MemberShip_Id_seq" OWNED BY "base_MemberShip"."Id";
            public       postgres    false    1785                       0    0    base_MemberShip_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_MemberShip_Id_seq"', 1, false);
            public       postgres    false    1785            P           1259    268511    base_PricingChange    TABLE     �  CREATE TABLE "base_PricingChange" (
    "Id" bigint NOT NULL,
    "PricingManagerId" integer NOT NULL,
    "PricingManagerResource" character varying(36),
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36),
    "Cost" numeric(12,2) DEFAULT 0,
    "CurrentPrice" numeric(12,2) DEFAULT 0,
    "NewPrice" numeric(12,2) DEFAULT 0,
    "PriceChanged" numeric(12,2) DEFAULT 0,
    "DateCreated" timestamp without time zone
);
 (   DROP TABLE public."base_PricingChange";
       public         postgres    false    2504    2506    2507    2508    7            O           1259    268509    base_PricingChange_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PricingChange_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PricingChange_Id_seq";
       public       postgres    false    7    1872                       0    0    base_PricingChange_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PricingChange_Id_seq" OWNED BY "base_PricingChange"."Id";
            public       postgres    false    1871                       0    0    base_PricingChange_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingChange_Id_seq"', 533, true);
            public       postgres    false    1871            L           1259    268185    base_PricingManager    TABLE       CREATE TABLE "base_PricingManager" (
    "Id" integer NOT NULL,
    "Name" character varying(36),
    "Description" numeric(12,2) DEFAULT 0,
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(36),
    "DateApplied" timestamp without time zone,
    "UserApplied" character varying(36),
    "DateRestored" timestamp without time zone,
    "UserRestored" character varying(36),
    "AffectPricing" smallint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "PriceLevel" character varying(30) NOT NULL,
    "Status" character varying(30),
    "BasePrice" smallint NOT NULL,
    "CalculateMethod" smallint,
    "AmountChange" numeric(12,2) DEFAULT 0,
    "AmountUnit" smallint,
    "ItemCount" integer,
    "Reason" character varying(400)
);
 )   DROP TABLE public."base_PricingManager";
       public         postgres    false    2497    2498    2499    7                       0    0 %   COLUMN "base_PricingManager"."Status"    COMMENT     u   COMMENT ON COLUMN "base_PricingManager"."Status" IS '- Pending
- Applied
- Restored

-> Get From PricingStatus Tag';
            public       postgres    false    1868                        0    0 (   COLUMN "base_PricingManager"."BasePrice"    COMMENT     H   COMMENT ON COLUMN "base_PricingManager"."BasePrice" IS 'Cost or Price';
            public       postgres    false    1868            !           0    0 .   COLUMN "base_PricingManager"."CalculateMethod"    COMMENT     j   COMMENT ON COLUMN "base_PricingManager"."CalculateMethod" IS '+-*/
- Get from PricingAdjustmentType Tag';
            public       postgres    false    1868            "           0    0 )   COLUMN "base_PricingManager"."AmountUnit"    COMMENT     D   COMMENT ON COLUMN "base_PricingManager"."AmountUnit" IS '- % or $';
            public       postgres    false    1868            #           0    0 (   COLUMN "base_PricingManager"."ItemCount"    COMMENT     W   COMMENT ON COLUMN "base_PricingManager"."ItemCount" IS 'Tong so product duoc ap dung';
            public       postgres    false    1868            K           1259    268183    base_PricingManager_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_PricingManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_PricingManager_Id_seq";
       public       postgres    false    1868    7            $           0    0    base_PricingManager_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_PricingManager_Id_seq" OWNED BY "base_PricingManager"."Id";
            public       postgres    false    1867            %           0    0    base_PricingManager_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingManager_Id_seq"', 46, true);
            public       postgres    false    1867                       1259    245412    base_Product    TABLE     �  CREATE TABLE "base_Product" (
    "Id" bigint NOT NULL,
    "Code" character varying(15),
    "ItemTypeId" smallint NOT NULL,
    "ProductDepartmentId" integer NOT NULL,
    "ProductCategoryId" integer NOT NULL,
    "ProductBrandId" integer,
    "StyleModel" character varying(30) NOT NULL,
    "ProductName" character varying(300) NOT NULL,
    "Description" character varying(2000) NOT NULL,
    "Barcode" character varying(30) NOT NULL,
    "Attribute" character varying(30) NOT NULL,
    "Size" character varying(10) NOT NULL,
    "IsSerialTracking" boolean NOT NULL,
    "IsPublicWeb" boolean NOT NULL,
    "OnHandStore1" integer NOT NULL,
    "OnHandStore2" integer NOT NULL,
    "OnHandStore3" integer NOT NULL,
    "OnHandStore4" integer NOT NULL,
    "OnHandStore5" integer NOT NULL,
    "OnHandStore6" integer NOT NULL,
    "OnHandStore7" integer NOT NULL,
    "OnHandStore8" integer NOT NULL,
    "OnHandStore9" integer NOT NULL,
    "OnHandStore10" integer NOT NULL,
    "QuantityOnHand" integer NOT NULL,
    "QuantityOnOrder" integer NOT NULL,
    "CompanyReOrderPoint" integer NOT NULL,
    "IsUnOrderAble" boolean NOT NULL,
    "IsEligibleForCommission" boolean NOT NULL,
    "IsEligibleForReward" boolean NOT NULL,
    "RegularPrice" numeric(12,2) NOT NULL,
    "Price1" numeric(12,2) NOT NULL,
    "Price2" numeric(12,2) NOT NULL,
    "Price3" numeric(12,2) NOT NULL,
    "Price4" numeric(12,2) NOT NULL,
    "OrderCost" numeric(12,2) NOT NULL,
    "AverageUnitCost" numeric(12,2) NOT NULL,
    "TaxCode" character(3) NOT NULL,
    "MarginPercent" numeric(5,2) NOT NULL,
    "MarkupPercent" numeric(5,2) NOT NULL,
    "BaseUOMId" integer NOT NULL,
    "GroupAttribute" uuid,
    "Custom1" character varying(30),
    "Custom2" character varying(30),
    "Custom3" character varying(30),
    "Custom4" character varying(30),
    "Custom5" character varying(30),
    "Custom6" character varying(30),
    "Custom7" character varying(30),
    "Resource" uuid DEFAULT newid() NOT NULL,
    "UserCreated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "WarrantyType" smallint,
    "WarrantyNumber" smallint DEFAULT 0,
    "WarrantyPeriod" smallint DEFAULT 0,
    "PartNumber" character varying(20),
    "SellUOMId" integer,
    "OrderUOMId" integer,
    "IsPurge" boolean,
    "VendorId" bigint DEFAULT 0 NOT NULL,
    "UserAssignedCommission" character varying(15),
    "AssignedCommissionPercent" numeric(5,2) DEFAULT 0,
    "AssignedCommissionAmount" numeric(10,2) DEFAULT 0,
    "Serial" character varying(30),
    "OrderUOM" character varying(10),
    "MarkdownPercent1" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkdownPercent2" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkdownPercent3" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkdownPercent4" numeric(10,2) DEFAULT 0 NOT NULL,
    "IsOpenItem" boolean DEFAULT false NOT NULL,
    "Location" character varying(200)
);
 "   DROP TABLE public."base_Product";
       public         postgres    false    2328    2330    2331    2332    2333    2334    2335    2336    2337    2338    2339    2340    2341    7            &           0    0 &   COLUMN "base_Product"."QuantityOnHand"    COMMENT     b   COMMENT ON COLUMN "base_Product"."QuantityOnHand" IS 'Total From OnHandStore1 to OnHandStore 10';
            public       postgres    false    1808            '           0    0 '   COLUMN "base_Product"."QuantityOnOrder"    COMMENT     [   COMMENT ON COLUMN "base_Product"."QuantityOnOrder" IS 'Quantity on active purchase order';
            public       postgres    false    1808            (           0    0 $   COLUMN "base_Product"."RegularPrice"    COMMENT     I   COMMENT ON COLUMN "base_Product"."RegularPrice" IS 'Apply to Base Unit';
            public       postgres    false    1808            )           0    0    COLUMN "base_Product"."Price1"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price1" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            *           0    0    COLUMN "base_Product"."Price2"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price2" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            +           0    0    COLUMN "base_Product"."Price3"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price3" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            ,           0    0    COLUMN "base_Product"."Price4"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price4" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            -           0    0 !   COLUMN "base_Product"."OrderCost"    COMMENT     F   COMMENT ON COLUMN "base_Product"."OrderCost" IS 'Apply to Base Unit';
            public       postgres    false    1808            .           0    0 '   COLUMN "base_Product"."AverageUnitCost"    COMMENT     L   COMMENT ON COLUMN "base_Product"."AverageUnitCost" IS 'Apply to Base Unit';
            public       postgres    false    1808            /           0    0    COLUMN "base_Product"."TaxCode"    COMMENT     D   COMMENT ON COLUMN "base_Product"."TaxCode" IS 'Apply to Base Unit';
            public       postgres    false    1808            0           0    0 %   COLUMN "base_Product"."MarginPercent"    COMMENT     q   COMMENT ON COLUMN "base_Product"."MarginPercent" IS 'Margin =100*(RegularPrice - AverageUnitCode)/RegularPrice';
            public       postgres    false    1808            1           0    0 %   COLUMN "base_Product"."MarkupPercent"    COMMENT     t   COMMENT ON COLUMN "base_Product"."MarkupPercent" IS 'Markup =100*(RegularPrice - AverageUnitCost)/AverageUnitCost';
            public       postgres    false    1808            2           0    0 "   COLUMN "base_Product"."IsOpenItem"    COMMENT     Q   COMMENT ON COLUMN "base_Product"."IsOpenItem" IS 'Can change price during sale';
            public       postgres    false    1808                       1259    255536    base_ProductStore    TABLE     �   CREATE TABLE "base_ProductStore" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL
);
 '   DROP TABLE public."base_ProductStore";
       public         postgres    false    2391    2393    7                       1259    255534    base_ProductStore_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ProductStore_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ProductStore_Id_seq";
       public       postgres    false    1821    7            3           0    0    base_ProductStore_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ProductStore_Id_seq" OWNED BY "base_ProductStore"."Id";
            public       postgres    false    1820            4           0    0    base_ProductStore_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_ProductStore_Id_seq"', 53, true);
            public       postgres    false    1820            ^           1259    270252    base_ProductUOM    TABLE     p  CREATE TABLE "base_ProductUOM" (
    "Id" bigint NOT NULL,
    "ProductStoreId" bigint,
    "ProductId" bigint NOT NULL,
    "UOMId" integer NOT NULL,
    "BaseUnitNumber" integer DEFAULT 0 NOT NULL,
    "RegularPrice" numeric(12,2) DEFAULT 0 NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "AverageCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price1" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price2" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price3" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price4" numeric(12,2) DEFAULT 0 NOT NULL,
    "MarkDownPercent1" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkDownPercent2" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkDownPercent3" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkDownPercent4" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarginPercent" numeric(10,2) DEFAULT 0 NOT NULL,
    "MarkupPercent" numeric(10,2) DEFAULT 0 NOT NULL
);
 %   DROP TABLE public."base_ProductUOM";
       public         postgres    false    2552    2553    2554    2555    2556    2557    2558    2559    2560    2561    2562    2563    2564    2565    7            5           0    0    TABLE "base_ProductUOM"    COMMENT     B   COMMENT ON TABLE "base_ProductUOM" IS 'Use when allow multi UOM';
            public       postgres    false    1886            ]           1259    270250    base_ProductUOM_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_ProductUOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_ProductUOM_Id_seq";
       public       postgres    false    7    1886            6           0    0    base_ProductUOM_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_ProductUOM_Id_seq" OWNED BY "base_ProductUOM"."Id";
            public       postgres    false    1885            7           0    0    base_ProductUOM_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_ProductUOM_Id_seq"', 37, true);
            public       postgres    false    1885                       1259    245410    base_Product_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_Product_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_Product_Id_seq";
       public       postgres    false    7    1808            8           0    0    base_Product_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_Product_Id_seq" OWNED BY "base_Product"."Id";
            public       postgres    false    1807            9           0    0    base_Product_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Product_Id_seq"', 250187, true);
            public       postgres    false    1807            
           1259    245169    base_Promotion    TABLE     �  CREATE TABLE "base_Promotion" (
    "Id" integer NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(200) NOT NULL,
    "PromotionTypeId" smallint NOT NULL,
    "TakeOffOption" smallint NOT NULL,
    "TakeOff" numeric NOT NULL,
    "BuyingQty" integer NOT NULL,
    "GetingValue" integer NOT NULL,
    "IsApplyToAboveQuantities" boolean NOT NULL,
    "Status" smallint NOT NULL,
    "AffectDiscount" smallint NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30) NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30),
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CouponExpire" timestamp without time zone,
    "IsCouponExpired" boolean DEFAULT false NOT NULL,
    "PriceSchemaRange" integer,
    "ReasonReActive" character varying(200),
    "Sold" integer DEFAULT 0 NOT NULL,
    "TotalPrice" numeric(12,2) DEFAULT 0 NOT NULL,
    "CategoryId" integer,
    "VendorId" bigint,
    "CouponBarCode" character varying(15),
    "BarCodeNumber" character varying(15),
    "BarCodeImage" bytea
);
 $   DROP TABLE public."base_Promotion";
       public         postgres    false    2310    2311    2312    2313    2314    2315    7            :           0    0     COLUMN "base_Promotion"."Status"    COMMENT     U   COMMENT ON COLUMN "base_Promotion"."Status" IS '0: Deactived
1: Actived
2: Pending';
            public       postgres    false    1802            ;           0    0 (   COLUMN "base_Promotion"."AffectDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Promotion"."AffectDiscount" IS '0: All items
1: All items in category
2: All items from vendors
3: Custom';
            public       postgres    false    1802                       1259    245155    base_PromotionAffect    TABLE     j  CREATE TABLE "base_PromotionAffect" (
    "Id" integer NOT NULL,
    "PromotionId" integer NOT NULL,
    "ItemId" bigint NOT NULL,
    "Price1" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price2" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price3" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price4" numeric(12,2) DEFAULT 0 NOT NULL,
    "Price5" numeric(12,2) DEFAULT 0 NOT NULL,
    "Discount1" numeric(12,2) DEFAULT 0 NOT NULL,
    "Discount2" numeric(12,2) DEFAULT 0 NOT NULL,
    "Discount3" numeric(12,2) DEFAULT 0 NOT NULL,
    "Discount4" numeric(12,2) DEFAULT 0 NOT NULL,
    "Discount5" numeric(12,2) DEFAULT 0 NOT NULL
);
 *   DROP TABLE public."base_PromotionAffect";
       public         postgres    false    2298    2299    2301    2302    2303    2304    2305    2306    2307    2308    7                       1259    245153    base_PromotionAffect_Id_seq    SEQUENCE        CREATE SEQUENCE "base_PromotionAffect_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_PromotionAffect_Id_seq";
       public       postgres    false    1800    7            <           0    0    base_PromotionAffect_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_PromotionAffect_Id_seq" OWNED BY "base_PromotionAffect"."Id";
            public       postgres    false    1799            =           0    0    base_PromotionAffect_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_PromotionAffect_Id_seq"', 609, true);
            public       postgres    false    1799            �           1259    245023    base_PromotionSchedule    TABLE     �   CREATE TABLE "base_PromotionSchedule" (
    "Id" integer NOT NULL,
    "PromotionId" integer NOT NULL,
    "EndDate" timestamp without time zone,
    "StartDate" timestamp without time zone
);
 ,   DROP TABLE public."base_PromotionSchedule";
       public         postgres    false    7            �           1259    245021    base_PromotionSchedule_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PromotionSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 6   DROP SEQUENCE public."base_PromotionSchedule_Id_seq";
       public       postgres    false    7    1788            >           0    0    base_PromotionSchedule_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE "base_PromotionSchedule_Id_seq" OWNED BY "base_PromotionSchedule"."Id";
            public       postgres    false    1787            ?           0    0    base_PromotionSchedule_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_PromotionSchedule_Id_seq"', 53, true);
            public       postgres    false    1787            	           1259    245167    base_Promotion_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Promotion_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Promotion_Id_seq";
       public       postgres    false    7    1802            @           0    0    base_Promotion_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Promotion_Id_seq" OWNED BY "base_Promotion"."Id";
            public       postgres    false    1801            A           0    0    base_Promotion_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_Promotion_Id_seq"', 53, true);
            public       postgres    false    1801            D           1259    266551    base_PurchaseOrder    TABLE     �  CREATE TABLE "base_PurchaseOrder" (
    "Id" bigint NOT NULL,
    "PurchaseOrderNo" character varying(15) NOT NULL,
    "VendorId" bigint NOT NULL,
    "VendorCode" character varying(20) NOT NULL,
    "Status" smallint NOT NULL,
    "ShipAddress" character varying(200),
    "PurchasedDate" timestamp without time zone DEFAULT now() NOT NULL,
    "TermDiscountPercent" numeric(4,2) DEFAULT 0 NOT NULL,
    "TermNetDue" smallint DEFAULT 0 NOT NULL,
    "TermPaidWithinDay" smallint DEFAULT 0 NOT NULL,
    "PaymentTermDescription" character varying(30),
    "PaymentDueDate" timestamp without time zone,
    "PaymentMethodId" integer NOT NULL,
    "Remark" character varying(200),
    "ShipDate" timestamp without time zone,
    "SubTotal" numeric(12,2) NOT NULL,
    "DiscountPercent" numeric(5,2) NOT NULL,
    "DiscountAmount" numeric(12,2) NOT NULL,
    "Freight" numeric(10,2) NOT NULL,
    "Fee" numeric(12,2) NOT NULL,
    "Total" numeric(12,2) NOT NULL,
    "Paid" numeric(12,2),
    "Balance" numeric(12,2),
    "ItemCount" integer NOT NULL,
    "QtyOrdered" integer NOT NULL,
    "QtyDue" integer NOT NULL,
    "QtyReceived" integer NOT NULL,
    "UnFilled" numeric(5,2) DEFAULT 0 NOT NULL,
    "UserCreated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "DateUpdate" timestamp without time zone DEFAULT now(),
    "UserUpdated" character varying(30),
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CancelDate" timestamp without time zone,
    "IsFullWorkflow" boolean DEFAULT false NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL,
    "RecRemark" character varying(200),
    "PaymentName" character varying(30)
);
 (   DROP TABLE public."base_PurchaseOrder";
       public         postgres    false    2475    2477    2478    2479    2480    2481    2482    2483    2484    2485    7            B           0    0 (   COLUMN "base_PurchaseOrder"."QtyOrdered"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyOrdered" IS 'Order Quantity: In the purchase order item list. Enter the quantity being ordered for the item.
';
            public       postgres    false    1860            C           0    0 $   COLUMN "base_PurchaseOrder"."QtyDue"    COMMENT     q   COMMENT ON COLUMN "base_PurchaseOrder"."QtyDue" IS 'Due Quantity: The item quantity remaining to be received.
';
            public       postgres    false    1860            D           0    0 )   COLUMN "base_PurchaseOrder"."QtyReceived"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyReceived" IS 'Received Quantity: The ordered item quantity already received on receiving vouchers.
';
            public       postgres    false    1860            E           0    0 &   COLUMN "base_PurchaseOrder"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_PurchaseOrder"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100

';
            public       postgres    false    1860            B           1259    266530    base_PurchaseOrderDetail    TABLE     2  CREATE TABLE "base_PurchaseOrderDetail" (
    "Id" bigint NOT NULL,
    "PurchaseOrderId" bigint NOT NULL,
    "ProductResource" character varying(36),
    "ItemCode" character varying(15),
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "BaseUOM" character varying(10) NOT NULL,
    "UOMId" integer NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "Quantity" integer DEFAULT 0 NOT NULL,
    "ReceivedQty" integer DEFAULT 0 NOT NULL,
    "DueQty" integer DEFAULT 0 NOT NULL,
    "UnFilledQty" numeric(5,2) DEFAULT 0 NOT NULL,
    "Amount" numeric(12,2) DEFAULT 0,
    "Serial" text,
    "LastReceived" timestamp without time zone,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "IsFullReceived" boolean DEFAULT false NOT NULL
);
 .   DROP TABLE public."base_PurchaseOrderDetail";
       public         postgres    false    2466    2467    2468    2469    2471    2472    2473    2474    7            F           0    0 *   COLUMN "base_PurchaseOrderDetail"."Amount"    COMMENT     S   COMMENT ON COLUMN "base_PurchaseOrderDetail"."Amount" IS 'Amount = Cost*Quantity';
            public       postgres    false    1858            A           1259    266528    base_PurchaseOrderDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_PurchaseOrderDetail_Id_seq";
       public       postgres    false    7    1858            G           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_PurchaseOrderDetail_Id_seq" OWNED BY "base_PurchaseOrderDetail"."Id";
            public       postgres    false    1857            H           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_PurchaseOrderDetail_Id_seq"', 87, true);
            public       postgres    false    1857            J           1259    267535    base_PurchaseOrderReceive    TABLE     o  CREATE TABLE "base_PurchaseOrderReceive" (
    "Id" bigint NOT NULL,
    "PurchaseOrderDetailId" bigint NOT NULL,
    "POResource" character varying(36) NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "ItemCode" character varying(15) NOT NULL,
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "RecQty" integer DEFAULT 0 NOT NULL,
    "IsReceived" boolean DEFAULT false NOT NULL,
    "ReceiveDate" timestamp without time zone NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL
);
 /   DROP TABLE public."base_PurchaseOrderReceive";
       public         postgres    false    2491    2492    2493    2495    7            I           0    0 *   COLUMN "base_PurchaseOrderReceive"."Price"    COMMENT     G   COMMENT ON COLUMN "base_PurchaseOrderReceive"."Price" IS 'Sale Price';
            public       postgres    false    1866            I           1259    267533     base_PurchaseOrderReceive_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderReceive_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_PurchaseOrderReceive_Id_seq";
       public       postgres    false    7    1866            J           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_PurchaseOrderReceive_Id_seq" OWNED BY "base_PurchaseOrderReceive"."Id";
            public       postgres    false    1865            K           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_PurchaseOrderReceive_Id_seq"', 72, true);
            public       postgres    false    1865            C           1259    266549    base_PurchaseOrder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PurchaseOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PurchaseOrder_Id_seq";
       public       postgres    false    1860    7            L           0    0    base_PurchaseOrder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PurchaseOrder_Id_seq" OWNED BY "base_PurchaseOrder"."Id";
            public       postgres    false    1859            M           0    0    base_PurchaseOrder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_PurchaseOrder_Id_seq"', 44, true);
            public       postgres    false    1859                       1259    245733    base_QuantityAdjustment    TABLE     �  CREATE TABLE "base_QuantityAdjustment" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "NewQuantity" integer DEFAULT 0 NOT NULL,
    "OldQuantity" integer DEFAULT 0 NOT NULL,
    "ItemCount" integer DEFAULT 0,
    "LoggedTime" timestamp without time zone NOT NULL,
    "Reason" character varying(30) NOT NULL,
    "StoreNumber" integer
);
 -   DROP TABLE public."base_QuantityAdjustment";
       public         postgres    false    2342    2344    2345    2346    2347    7            N           0    0 1   COLUMN "base_QuantityAdjustment"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustment"."CostDifference" IS 'if(QtyChanged) AverageUnitCost*(NewQty - OldQty) elseif(CostChanged) Quantity*(NewCost - OldCost)
';
            public       postgres    false    1810            O           0    0 ,   COLUMN "base_QuantityAdjustment"."ItemCount"    COMMENT     a   COMMENT ON COLUMN "base_QuantityAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1810            P           0    0 -   COLUMN "base_QuantityAdjustment"."LoggedTime"    COMMENT     {   COMMENT ON COLUMN "base_QuantityAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
            public       postgres    false    1810                       1259    245745    base_QuantityAdjustmentItem    TABLE     �  CREATE TABLE "base_QuantityAdjustmentItem" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductCode" character varying(20) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentNewQty" integer NOT NULL,
    "AdjustmentOldQty" integer NOT NULL,
    "AdjustmentQtyDiff" integer NOT NULL,
    "LoggedTime" timestamp without time zone NOT NULL,
    "ParentResource" character varying(36) NOT NULL
);
 1   DROP TABLE public."base_QuantityAdjustmentItem";
       public         postgres    false    2348    2350    7            Q           0    0 5   COLUMN "base_QuantityAdjustmentItem"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."CostDifference" IS '-- AverageUnitCost*OldQuantity - AverageUnitCost*NewQuantity';
            public       postgres    false    1812            R           0    0 8   COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff"    COMMENT     n   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff" IS 'AdjustmentNewQty - AdjustmentOldQty';
            public       postgres    false    1812                       1259    245743 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_QuantityAdjustmentItem_Id_seq";
       public       postgres    false    1812    7            S           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_QuantityAdjustmentItem_Id_seq" OWNED BY "base_QuantityAdjustmentItem"."Id";
            public       postgres    false    1811            T           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_QuantityAdjustmentItem_Id_seq"', 1, false);
            public       postgres    false    1811                       1259    245731    base_QuantityAdjustment_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_QuantityAdjustment_Id_seq";
       public       postgres    false    7    1810            U           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_QuantityAdjustment_Id_seq" OWNED BY "base_QuantityAdjustment"."Id";
            public       postgres    false    1809            V           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_QuantityAdjustment_Id_seq"', 1, false);
            public       postgres    false    1809            1           1259    256178    base_ResourceAccount    TABLE       CREATE TABLE "base_ResourceAccount" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "UserResource" character varying(36) NOT NULL,
    "LoginName" character varying(50) NOT NULL,
    "Password" character varying(150) NOT NULL,
    "ExpiredDate" timestamp without time zone,
    "IsLocked" boolean DEFAULT false,
    "IsExpired" boolean DEFAULT false
);
 *   DROP TABLE public."base_ResourceAccount";
       public         postgres    false    2403    2404    2405    7            0           1259    256176    base_ResourceAccount_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourceAccount_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourceAccount_Id_seq";
       public       postgres    false    7    1841            W           0    0    base_ResourceAccount_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourceAccount_Id_seq" OWNED BY "base_ResourceAccount"."Id";
            public       postgres    false    1840            X           0    0    base_ResourceAccount_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceAccount_Id_seq"', 27, true);
            public       postgres    false    1840                       1259    246083    base_ResourceNote    TABLE     �   CREATE TABLE "base_ResourceNote" (
    "Id" bigint NOT NULL,
    "Note" character varying(300),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "Color" character(9),
    "Resource" character varying(36) NOT NULL
);
 '   DROP TABLE public."base_ResourceNote";
       public         postgres    false    2364    7                       1259    246081    base_ResourceNote_id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ResourceNote_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ResourceNote_id_seq";
       public       postgres    false    1818    7            Y           0    0    base_ResourceNote_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ResourceNote_id_seq" OWNED BY "base_ResourceNote"."Id";
            public       postgres    false    1817            Z           0    0    base_ResourceNote_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ResourceNote_id_seq"', 684, true);
            public       postgres    false    1817            Z           1259    270150    base_ResourcePayment    TABLE     �  CREATE TABLE "base_ResourcePayment" (
    "Id" bigint NOT NULL,
    "DocumentResource" character varying(36) NOT NULL,
    "DocumentNo" character varying(15) NOT NULL,
    "TotalAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "TotalPaid" numeric(12,2) DEFAULT 0 NOT NULL,
    "Balance" numeric(12,2) DEFAULT 0 NOT NULL,
    "Change" numeric(12,2) DEFAULT 0 NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30) NOT NULL,
    "Remark" character varying(200),
    "Resource" uuid DEFAULT newid() NOT NULL,
    "SubTotal" numeric(12,2) DEFAULT 0 NOT NULL,
    "DiscountPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "DiscountAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "Mark" character varying(2),
    "IsDeposit" boolean,
    "TaxCode" character varying(3),
    "TaxAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "LastRewardAmount" numeric(12,2) DEFAULT 0 NOT NULL
);
 *   DROP TABLE public."base_ResourcePayment";
       public         postgres    false    2529    2530    2532    2533    2534    2535    2536    2537    2538    2539    2540    7            [           0    0 $   COLUMN "base_ResourcePayment"."Mark"    COMMENT     <   COMMENT ON COLUMN "base_ResourcePayment"."Mark" IS 'SO/PO';
            public       postgres    false    1882            X           1259    270072    base_ResourcePaymentDetail    TABLE       CREATE TABLE "base_ResourcePaymentDetail" (
    "Id" bigint NOT NULL,
    "PaymentType" character(1),
    "ResourcePaymentId" bigint NOT NULL,
    "PaymentMethodId" smallint NOT NULL,
    "PaymentMethod" character varying(60) NOT NULL,
    "CardType" smallint NOT NULL,
    "Paid" numeric(12,2) DEFAULT 0 NOT NULL,
    "Change" numeric(12,2) DEFAULT 0 NOT NULL,
    "Tip" numeric(12,2) DEFAULT 0 NOT NULL,
    "GiftCardNo" character varying(30),
    "Reason" character varying(200),
    "Reference" character varying(50)
);
 0   DROP TABLE public."base_ResourcePaymentDetail";
       public         postgres    false    2526    2527    2528    7            \           0    0 1   COLUMN "base_ResourcePaymentDetail"."PaymentType"    COMMENT     W   COMMENT ON COLUMN "base_ResourcePaymentDetail"."PaymentType" IS 'P:Payment
C:Correct';
            public       postgres    false    1880            ]           0    0 ,   COLUMN "base_ResourcePaymentDetail"."Reason"    COMMENT     ^   COMMENT ON COLUMN "base_ResourcePaymentDetail"."Reason" IS 'Apply to Correct payment action';
            public       postgres    false    1880            W           1259    270070 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_ResourcePaymentDetail_Id_seq";
       public       postgres    false    7    1880            ^           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_ResourcePaymentDetail_Id_seq" OWNED BY "base_ResourcePaymentDetail"."Id";
            public       postgres    false    1879            _           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentDetail_Id_seq"', 200, true);
            public       postgres    false    1879            g           1259    272122    base_ResourcePaymentProduct    TABLE       CREATE TABLE "base_ResourcePaymentProduct" (
    "Id" bigint NOT NULL,
    "ResourcePaymentId" bigint,
    "ProductResource" character varying(36),
    "ItemCode" character varying(15),
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "BaseUOM" character varying(10) NOT NULL,
    "UOMId" integer NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "Quantity" integer DEFAULT 0 NOT NULL,
    "Amount" numeric(12,2) DEFAULT 0 NOT NULL
);
 1   DROP TABLE public."base_ResourcePaymentProduct";
       public         postgres    false    2579    2580    2581    7            f           1259    272120 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_ResourcePaymentProduct_Id_seq";
       public       postgres    false    7    1895            `           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_ResourcePaymentProduct_Id_seq" OWNED BY "base_ResourcePaymentProduct"."Id";
            public       postgres    false    1894            a           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentProduct_Id_seq"', 51, true);
            public       postgres    false    1894            Y           1259    270148    base_ResourcePayment_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourcePayment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourcePayment_Id_seq";
       public       postgres    false    7    1882            b           0    0    base_ResourcePayment_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourcePayment_Id_seq" OWNED BY "base_ResourcePayment"."Id";
            public       postgres    false    1881            c           0    0    base_ResourcePayment_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_ResourcePayment_Id_seq"', 119, true);
            public       postgres    false    1881            \           1259    270193    base_ResourceReturn    TABLE     �  CREATE TABLE "base_ResourceReturn" (
    "Id" bigint NOT NULL,
    "DocumentResource" character varying(36) NOT NULL,
    "DocumentNo" character varying(15) NOT NULL,
    "TotalAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "TotalRefund" numeric(12,2) DEFAULT 0 NOT NULL,
    "Balance" numeric(12,2) DEFAULT 0 NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserCreated" character varying(30) NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "Mark" character(2) NOT NULL,
    "DiscountPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "DiscountAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "Freight" numeric(10,2) DEFAULT 0 NOT NULL,
    "SubTotal" numeric(12,2) DEFAULT 0 NOT NULL
);
 )   DROP TABLE public."base_ResourceReturn";
       public         postgres    false    2541    2543    2544    2545    2546    2547    2548    2549    2550    7            d           0    0 #   COLUMN "base_ResourceReturn"."Mark"    COMMENT     ;   COMMENT ON COLUMN "base_ResourceReturn"."Mark" IS 'SO/PO';
            public       postgres    false    1884            e           1259    272099    base_ResourceReturnDetail    TABLE     x  CREATE TABLE "base_ResourceReturnDetail" (
    "Id" bigint NOT NULL,
    "ResourceReturnId" bigint NOT NULL,
    "OrderDetailResource" character varying(36) NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "ItemCode" character varying(15) NOT NULL,
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "ReturnQty" integer DEFAULT 0 NOT NULL,
    "Amount" numeric(12,2) DEFAULT 0 NOT NULL,
    "IsReturned" boolean DEFAULT false NOT NULL,
    "ReturnedDate" timestamp without time zone NOT NULL
);
 /   DROP TABLE public."base_ResourceReturnDetail";
       public         postgres    false    2574    2575    2576    2577    7            d           1259    272097     base_ResourceReturnDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourceReturnDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_ResourceReturnDetail_Id_seq";
       public       postgres    false    1893    7            e           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_ResourceReturnDetail_Id_seq" OWNED BY "base_ResourceReturnDetail"."Id";
            public       postgres    false    1892            f           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_ResourceReturnDetail_Id_seq"', 48, true);
            public       postgres    false    1892            [           1259    270191    base_ResourceReturn_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_ResourceReturn_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_ResourceReturn_Id_seq";
       public       postgres    false    7    1884            g           0    0    base_ResourceReturn_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_ResourceReturn_Id_seq" OWNED BY "base_ResourceReturn"."Id";
            public       postgres    false    1883            h           0    0    base_ResourceReturn_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_ResourceReturn_Id_seq"', 76, true);
            public       postgres    false    1883            H           1259    266843    base_RewardManager    TABLE     �  CREATE TABLE "base_RewardManager" (
    "Id" integer NOT NULL,
    "StoreCode" character varying(3),
    "PurchaseThreshold" numeric(19,2) NOT NULL,
    "RewardAmount" numeric(19,2) NOT NULL,
    "RewardAmtType" integer NOT NULL,
    "RewardExpiration" integer NOT NULL,
    "IsAutoEnroll" boolean NOT NULL,
    "IsPromptEnroll" boolean NOT NULL,
    "IsInformCashier" boolean NOT NULL,
    "IsRedemptionLimit" boolean NOT NULL,
    "RedemptionLimitAmount" numeric(19,2) NOT NULL,
    "IsBlockRedemption" boolean NOT NULL,
    "RedemptionAfterDays" integer NOT NULL,
    "IsBlockPurchaseRedeem" boolean NOT NULL,
    "IsTrackingPeriod" boolean DEFAULT false NOT NULL,
    "StartDate" timestamp without time zone,
    "EndDate" timestamp without time zone,
    "IsNoEndDay" boolean DEFAULT false NOT NULL,
    "TotalRewardRedeemed" numeric(10,2) DEFAULT 0 NOT NULL,
    "IsActived" boolean NOT NULL,
    "ReasonReActive" character varying(200),
    "DateCreated" timestamp without time zone
);
 (   DROP TABLE public."base_RewardManager";
       public         postgres    false    2488    2489    2490    7            G           1259    266841    base_RewardManager_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_RewardManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_RewardManager_Id_seq";
       public       postgres    false    1864    7            i           0    0    base_RewardManager_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_RewardManager_Id_seq" OWNED BY "base_RewardManager"."Id";
            public       postgres    false    1863            j           0    0    base_RewardManager_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_RewardManager_Id_seq"', 2, true);
            public       postgres    false    1863            F           1259    266606    base_SaleCommission    TABLE     �  CREATE TABLE "base_SaleCommission" (
    "Id" integer NOT NULL,
    "GuestResource" character varying(36),
    "SOResource" character varying(36),
    "SONumber" character varying(12),
    "SOTotal" numeric(12,2),
    "SODate" timestamp without time zone,
    "ComissionPercent" numeric(5,2),
    "CommissionAmount" numeric(12,2),
    "Sign" character(1),
    "Remark" character varying(50)
);
 )   DROP TABLE public."base_SaleCommission";
       public         postgres    false    7            E           1259    266604    base_SaleCommission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_SaleCommission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_SaleCommission_Id_seq";
       public       postgres    false    7    1862            k           0    0    base_SaleCommission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_SaleCommission_Id_seq" OWNED BY "base_SaleCommission"."Id";
            public       postgres    false    1861            l           0    0    base_SaleCommission_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_SaleCommission_Id_seq"', 438, true);
            public       postgres    false    1861            :           1259    266093    base_SaleOrder    TABLE     �	  CREATE TABLE "base_SaleOrder" (
    "Id" bigint NOT NULL,
    "SONumber" character varying(12),
    "OrderDate" timestamp without time zone,
    "OrderStatus" smallint NOT NULL,
    "BillAddressId" bigint,
    "BillAddress" character varying(200),
    "ShipAddressId" bigint,
    "ShipAddress" character varying(200),
    "PromotionCode" character varying(20),
    "SaleRep" character varying(30),
    "CustomerResource" character varying(36) NOT NULL,
    "PriceSchemaId" smallint NOT NULL,
    "DueDate" timestamp without time zone,
    "RequestShipDate" timestamp without time zone NOT NULL,
    "SubTotal" numeric(14,2) DEFAULT 0 NOT NULL,
    "TaxLocation" integer NOT NULL,
    "TaxCode" character varying(3) NOT NULL,
    "DiscountAmount" numeric(10,2) DEFAULT 0 NOT NULL,
    "TaxAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "DiscountPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "TaxPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "Shipping" numeric(14,2) DEFAULT 0 NOT NULL,
    "Total" numeric(14,2) DEFAULT 0 NOT NULL,
    "Paid" numeric(14,2) DEFAULT 0 NOT NULL,
    "Balance" numeric(14,2) DEFAULT 0 NOT NULL,
    "RefundFee" numeric(14,2) DEFAULT 0 NOT NULL,
    "IsMultiPayment" boolean DEFAULT false NOT NULL,
    "Remark" character varying(200),
    "IsFullWorkflow" boolean DEFAULT false NOT NULL,
    "QtyOrdered" integer NOT NULL,
    "QtyDue" integer NOT NULL,
    "QtyReceived" integer NOT NULL,
    "UnFilled" numeric(5,2) DEFAULT 0 NOT NULL,
    "UserCreated" character varying(30),
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "BookingChanel" smallint NOT NULL,
    "ShippedCount" smallint DEFAULT 0 NOT NULL,
    "Deposit" numeric(12,2) DEFAULT 0,
    "Transaction" character varying(20),
    "TermDiscountPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "TermNetDue" smallint DEFAULT 0 NOT NULL,
    "TermPaidWithinDay" smallint DEFAULT 0 NOT NULL,
    "PaymentTermDescription" character varying(30),
    "IsTaxExemption" boolean DEFAULT false NOT NULL,
    "TaxExemption" character varying(20),
    "ShippedBox" smallint DEFAULT 0 NOT NULL,
    "PackedQty" smallint DEFAULT 0 NOT NULL,
    "TotalWeight" numeric(10,2) DEFAULT 0 NOT NULL,
    "WeightUnit" smallint DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL,
    "IsRedeeem" boolean DEFAULT false NOT NULL
);
 $   DROP TABLE public."base_SaleOrder";
       public         postgres    false    2423    2425    2426    2427    2428    2429    2430    2431    2432    2433    2434    2435    2436    2437    2438    2439    2440    2441    2442    2443    2444    2445    2446    2447    2448    2449    2450    2451    7            8           1259    266084    base_SaleOrderDetail    TABLE     9  CREATE TABLE "base_SaleOrderDetail" (
    "Id" bigint NOT NULL,
    "SaleOrderId" bigint NOT NULL,
    "ProductResource" character varying(36),
    "ItemCode" character varying(15),
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "TaxCode" character varying(3),
    "Quantity" integer DEFAULT 0 NOT NULL,
    "PickQty" integer DEFAULT 0 NOT NULL,
    "DueQty" integer DEFAULT 0 NOT NULL,
    "UnFilled" numeric(5,2) DEFAULT 0 NOT NULL,
    "RegularPrice" numeric(10,2) DEFAULT 0 NOT NULL,
    "SalePrice" numeric(10,2) NOT NULL,
    "UOMId" integer,
    "BaseUOM" character varying(10),
    "DiscountPercent" numeric(4,2) DEFAULT 0 NOT NULL,
    "DiscountAmount" numeric(10,2) DEFAULT 0 NOT NULL,
    "SubTotal" numeric(10,2) DEFAULT 0 NOT NULL,
    "OnHandQty" integer DEFAULT 0 NOT NULL,
    "SerialTracking" text,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "BalanceShipped" integer DEFAULT 0 NOT NULL,
    "Comment" character varying(100),
    "TotalDiscount" numeric(12,2) DEFAULT 0 NOT NULL
);
 *   DROP TABLE public."base_SaleOrderDetail";
       public         postgres    false    2410    2411    2412    2413    2414    2415    2416    2417    2418    2420    2421    2422    7            m           0    0 (   COLUMN "base_SaleOrderDetail"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_SaleOrderDetail"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100';
            public       postgres    false    1848            n           0    0 .   COLUMN "base_SaleOrderDetail"."SerialTracking"    COMMENT     Z   COMMENT ON COLUMN "base_SaleOrderDetail"."SerialTracking" IS 'Apply to Serial Tracking ';
            public       postgres    false    1848            o           0    0 .   COLUMN "base_SaleOrderDetail"."BalanceShipped"    COMMENT     s   COMMENT ON COLUMN "base_SaleOrderDetail"."BalanceShipped" IS 'Số lượng sản phẩm được vận chuyển';
            public       postgres    false    1848            7           1259    266082    base_SaleOrderDetail_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleOrderDetail_Id_seq";
       public       postgres    false    1848    7            p           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleOrderDetail_Id_seq" OWNED BY "base_SaleOrderDetail"."Id";
            public       postgres    false    1847            q           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderDetail_Id_seq"', 380, true);
            public       postgres    false    1847            >           1259    266236    base_SaleOrderInvoice    TABLE     }  CREATE TABLE "base_SaleOrderInvoice" (
    "Id" bigint NOT NULL,
    "InvoiceNo" character varying(15),
    "SaleOrderId" bigint NOT NULL,
    "SaleOrderResource" character varying(36),
    "ItemCount" integer,
    "SubTotal" numeric(14,2) DEFAULT 0 NOT NULL,
    "DiscountAmount" numeric(10,2) DEFAULT 0 NOT NULL,
    "TaxAmount" numeric(10,2) DEFAULT 0 NOT NULL,
    "DiscountPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "TaxPercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "Shipping" numeric(14,2) DEFAULT 0 NOT NULL,
    "Total" numeric(14,2) DEFAULT 0 NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL
);
 +   DROP TABLE public."base_SaleOrderInvoice";
       public         postgres    false    2456    2457    2458    2459    2460    2461    2462    2463    7            =           1259    266234    base_SaleOrderInvoice_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderInvoice_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_SaleOrderInvoice_Id_seq";
       public       postgres    false    7    1854            r           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_SaleOrderInvoice_Id_seq" OWNED BY "base_SaleOrderInvoice"."Id";
            public       postgres    false    1853            s           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderInvoice_Id_seq"', 1, false);
            public       postgres    false    1853            <           1259    266180    base_SaleOrderShip    TABLE     �  CREATE TABLE "base_SaleOrderShip" (
    "Id" bigint NOT NULL,
    "SaleOrderId" bigint NOT NULL,
    "SaleOrderResource" character varying(36),
    "Weight" numeric(10,3) NOT NULL,
    "TrackingNo" character varying(30),
    "IsShipped" boolean,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "Remark" character varying(200),
    "Carrier" character varying(100),
    "ShipDate" timestamp without time zone,
    "BoxNo" smallint DEFAULT 1 NOT NULL
);
 (   DROP TABLE public."base_SaleOrderShip";
       public         postgres    false    2453    2454    7            @           1259    266357    base_SaleOrderShipDetail    TABLE     2  CREATE TABLE "base_SaleOrderShipDetail" (
    "Id" bigint NOT NULL,
    "SaleOrderShipId" bigint,
    "SaleOrderShipResource" character varying(36),
    "SaleOrderDetailResource" character varying(36),
    "ProductResource" character varying(36),
    "ItemCode" character varying(15),
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "Description" character varying(100),
    "SerialTracking" character varying(30),
    "PackedQty" integer,
    "IsPaid" boolean DEFAULT false NOT NULL
);
 .   DROP TABLE public."base_SaleOrderShipDetail";
       public         postgres    false    2465    7            ?           1259    266355    base_SaleOrderShipDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderShipDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_SaleOrderShipDetail_Id_seq";
       public       postgres    false    1856    7            t           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_SaleOrderShipDetail_Id_seq" OWNED BY "base_SaleOrderShipDetail"."Id";
            public       postgres    false    1855            u           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_SaleOrderShipDetail_Id_seq"', 251, true);
            public       postgres    false    1855            ;           1259    266178    base_SaleOrderShip_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_SaleOrderShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_SaleOrderShip_Id_seq";
       public       postgres    false    7    1852            v           0    0    base_SaleOrderShip_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_SaleOrderShip_Id_seq" OWNED BY "base_SaleOrderShip"."Id";
            public       postgres    false    1851            w           0    0    base_SaleOrderShip_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_SaleOrderShip_Id_seq"', 191, true);
            public       postgres    false    1851            9           1259    266091    base_SaleOrder_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_SaleOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_SaleOrder_Id_seq";
       public       postgres    false    1850    7            x           0    0    base_SaleOrder_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_SaleOrder_Id_seq" OWNED BY "base_SaleOrder"."Id";
            public       postgres    false    1849            y           0    0    base_SaleOrder_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_SaleOrder_Id_seq"', 211, true);
            public       postgres    false    1849                        1259    245103    base_SaleTaxLocation    TABLE     n  CREATE TABLE "base_SaleTaxLocation" (
    "Id" integer NOT NULL,
    "ParentId" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "IsShipingTaxable" boolean DEFAULT false NOT NULL,
    "ShippingTaxCodeId" integer NOT NULL,
    "IsActived" boolean DEFAULT true NOT NULL,
    "LevelId" smallint DEFAULT 0 NOT NULL,
    "TaxCode" character(3),
    "TaxCodeName" character varying(20),
    "TaxPrintMark" character(1),
    "TaxOption" smallint DEFAULT 0 NOT NULL,
    "IsPrimary" boolean DEFAULT false NOT NULL,
    "SortIndex" character varying(10),
    "IsTaxAfterDiscount" boolean DEFAULT false NOT NULL
);
 *   DROP TABLE public."base_SaleTaxLocation";
       public         postgres    false    2281    2283    2284    2285    2286    2287    7            z           0    0 )   COLUMN "base_SaleTaxLocation"."SortIndex"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocation"."SortIndex" IS 'ParentId ==0 -> Id"-"DateTime
ParnetId !=0 -> ParentId"-"DateTime
Order By Asc';
            public       postgres    false    1792            �           1259    245084    base_SaleTaxLocationOption    TABLE     (  CREATE TABLE "base_SaleTaxLocationOption" (
    "Id" integer NOT NULL,
    "SaleTaxLocationId" integer NOT NULL,
    "ParentId" integer NOT NULL,
    "TaxRate" integer DEFAULT 0 NOT NULL,
    "TaxComponent" character varying(30) NOT NULL,
    "TaxAgency" character varying(30) NOT NULL,
    "TaxCondition" numeric NOT NULL,
    "IsApplyAmountOver" boolean NOT NULL,
    "IsAllowSpecificItemPriceRange" boolean NOT NULL,
    "IsAllowAmountItemPriceRange" boolean NOT NULL,
    "PriceFrom" numeric(10,0) NOT NULL,
    "PriceTo" numeric(10,0) NOT NULL
);
 0   DROP TABLE public."base_SaleTaxLocationOption";
       public         postgres    false    2280    7            {           0    0 .   COLUMN "base_SaleTaxLocationOption"."ParentId"    COMMENT     h   COMMENT ON COLUMN "base_SaleTaxLocationOption"."ParentId" IS 'Apply For Multi-rate has multi tax code';
            public       postgres    false    1790            |           0    0 -   COLUMN "base_SaleTaxLocationOption"."TaxRate"    COMMENT     k   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxRate" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1790            }           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxComponent"    COMMENT     Y   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxComponent" IS 'Apply For Multi-rate';
            public       postgres    false    1790            ~           0    0 /   COLUMN "base_SaleTaxLocationOption"."TaxAgency"    COMMENT     m   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxAgency" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1790                       0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxCondition"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxCondition" IS 'Apply For Price-Depedent: Collect this tax on an item if the unit price or shiping is more than';
            public       postgres    false    1790            �           0    0 7   COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver" IS 'Apply For Price-Depedent: Apply sale tax only to the amount over the pricing unit or shipping threshold';
            public       postgres    false    1790            �           0    0 C   COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to a specific item price range';
            public       postgres    false    1790            �           0    0 A   COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to the mount of an item''s price within this range';
            public       postgres    false    1790            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."PriceFrom"    COMMENT     V   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceFrom" IS 'Apply For Multi-rate';
            public       postgres    false    1790            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."PriceTo"    COMMENT     T   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceTo" IS 'Apply For Multi-rate';
            public       postgres    false    1790            �           1259    245082 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleTaxLocationOption_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_SaleTaxLocationOption_Id_seq";
       public       postgres    false    7    1790            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_SaleTaxLocationOption_Id_seq" OWNED BY "base_SaleTaxLocationOption"."Id";
            public       postgres    false    1789            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_SaleTaxLocationOption_Id_seq"', 111, true);
            public       postgres    false    1789            �           1259    245101    base_SaleTaxLocation_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleTaxLocation_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleTaxLocation_Id_seq";
       public       postgres    false    1792    7            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleTaxLocation_Id_seq" OWNED BY "base_SaleTaxLocation"."Id";
            public       postgres    false    1791            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleTaxLocation_Id_seq"', 328, true);
            public       postgres    false    1791                       1259    255675 
   base_Store    TABLE     �   CREATE TABLE "base_Store" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(30),
    "Street" character varying(200),
    "City" character varying(200)
);
     DROP TABLE public."base_Store";
       public         postgres    false    7                       1259    255673    base_Store_Id_seq    SEQUENCE     u   CREATE SEQUENCE "base_Store_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."base_Store_Id_seq";
       public       postgres    false    1823    7            �           0    0    base_Store_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Store_Id_seq" OWNED BY "base_Store"."Id";
            public       postgres    false    1822            �           0    0    base_Store_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"base_Store_Id_seq"', 46, true);
            public       postgres    false    1822            T           1259    269925    base_TransferStock    TABLE     �  CREATE TABLE "base_TransferStock" (
    "Id" bigint NOT NULL,
    "TransferNo" character varying(12) NOT NULL,
    "FromStore" smallint DEFAULT 0 NOT NULL,
    "ToStore" smallint DEFAULT 0 NOT NULL,
    "TotalQuantity" integer DEFAULT 0 NOT NULL,
    "ShipDate" timestamp without time zone,
    "Carier" character varying(200),
    "ShippingFee" numeric(12,2) DEFAULT 0 NOT NULL,
    "Comment" character varying(200),
    "Resource" uuid DEFAULT newid() NOT NULL,
    "UserCreated" character varying(30) NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now() NOT NULL,
    "Status" smallint DEFAULT 0 NOT NULL,
    "SubTotal" numeric(12,2) DEFAULT 0 NOT NULL,
    "Total" numeric(12,2) DEFAULT 0 NOT NULL,
    "DateApplied" timestamp without time zone,
    "UserApplied" character varying(30),
    "DateReversed" timestamp without time zone,
    "UserReversed" character varying(30)
);
 (   DROP TABLE public."base_TransferStock";
       public         postgres    false    2512    2513    2514    2515    2516    2517    2518    2519    2520    7            V           1259    269941    base_TransferStockDetail    TABLE     |  CREATE TABLE "base_TransferStockDetail" (
    "Id" bigint NOT NULL,
    "TransferStockId" bigint NOT NULL,
    "TransferStockResource" character varying(36) NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "ItemCode" character varying(15),
    "ItemName" character varying(100),
    "ItemAtribute" character varying(30),
    "ItemSize" character varying(30),
    "Quantity" integer DEFAULT 0 NOT NULL,
    "UOMId" integer NOT NULL,
    "BaseUOM" character varying(10) NOT NULL,
    "Amount" numeric(12,2) DEFAULT 0 NOT NULL,
    "SerialTracking" character varying(30),
    "AvlQuantity" integer DEFAULT 0 NOT NULL
);
 .   DROP TABLE public."base_TransferStockDetail";
       public         postgres    false    2522    2523    2524    7            U           1259    269939    base_TransferStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_TransferStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_TransferStockDetail_Id_seq";
       public       postgres    false    7    1878            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_TransferStockDetail_Id_seq" OWNED BY "base_TransferStockDetail"."Id";
            public       postgres    false    1877            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_TransferStockDetail_Id_seq"', 36, true);
            public       postgres    false    1877            S           1259    269923    base_TransferStock_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_TransferStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_TransferStock_Id_seq";
       public       postgres    false    7    1876            �           0    0    base_TransferStock_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_TransferStock_Id_seq" OWNED BY "base_TransferStock"."Id";
            public       postgres    false    1875            �           0    0    base_TransferStock_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_TransferStock_Id_seq"', 24, true);
            public       postgres    false    1875                       1259    245145    base_UOM_Id_seq    SEQUENCE     s   CREATE SEQUENCE "base_UOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."base_UOM_Id_seq";
       public       postgres    false    1798    7            �           0    0    base_UOM_Id_seq    SEQUENCE OWNED BY     ;   ALTER SEQUENCE "base_UOM_Id_seq" OWNED BY "base_UOM"."Id";
            public       postgres    false    1797            �           0    0    base_UOM_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"base_UOM_Id_seq"', 115, true);
            public       postgres    false    1797                       1259    245131    base_UserLog    TABLE     #  CREATE TABLE "base_UserLog" (
    "Id" bigint NOT NULL,
    "IpSource" character varying(17),
    "ConnectedOn" timestamp without time zone DEFAULT now() NOT NULL,
    "DisConnectedOn" timestamp without time zone,
    "ResourceAccessed" character varying(36),
    "IsDisconected" boolean
);
 "   DROP TABLE public."base_UserLog";
       public         postgres    false    2293    7            �           1259    244282    base_UserLogDetail    TABLE     �   CREATE TABLE "base_UserLogDetail" (
    "Id" uuid NOT NULL,
    "UserLogId" bigint,
    "AccessedTime" timestamp without time zone,
    "ModuleName" character varying(30),
    "ActionDescription" character varying(200)
);
 (   DROP TABLE public."base_UserLogDetail";
       public         postgres    false    7                       1259    245129    base_UserLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_UserLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_UserLog_Id_seq";
       public       postgres    false    1796    7            �           0    0    base_UserLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_UserLog_Id_seq" OWNED BY "base_UserLog"."Id";
            public       postgres    false    1795            �           0    0    base_UserLog_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_UserLog_Id_seq"', 1773, true);
            public       postgres    false    1795            3           1259    256244    base_UserRight    TABLE     �   CREATE TABLE "base_UserRight" (
    "Id" integer NOT NULL,
    "Code" character varying(5) NOT NULL,
    "Name" character varying(200)
);
 $   DROP TABLE public."base_UserRight";
       public         postgres    false    7            2           1259    256242    base_UserRight_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_UserRight_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_UserRight_Id_seq";
       public       postgres    false    1843    7            �           0    0    base_UserRight_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_UserRight_Id_seq" OWNED BY "base_UserRight"."Id";
            public       postgres    false    1842            �           0    0    base_UserRight_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_UserRight_Id_seq"', 187, true);
            public       postgres    false    1842            Q           1259    269643    base_VendorProduct    TABLE       CREATE TABLE "base_VendorProduct" (
    "Id" integer NOT NULL,
    "ProductId" bigint NOT NULL,
    "VendorId" bigint NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "VendorResource" character varying(36) NOT NULL
);
 (   DROP TABLE public."base_VendorProduct";
       public         postgres    false    2510    7            R           1259    269646    base_VendorProduct_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VendorProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VendorProduct_Id_seq";
       public       postgres    false    7    1873            �           0    0    base_VendorProduct_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VendorProduct_Id_seq" OWNED BY "base_VendorProduct"."Id";
            public       postgres    false    1874            �           0    0    base_VendorProduct_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VendorProduct_Id_seq"', 40, true);
            public       postgres    false    1874                       1259    245115    base_VirtualFolder    TABLE     �  CREATE TABLE "base_VirtualFolder" (
    "Id" integer NOT NULL,
    "ParentFolderId" integer,
    "FolderName" character varying(50) NOT NULL,
    "IsActived" boolean NOT NULL,
    "DateCreated" timestamp without time zone DEFAULT now(),
    "UserCreated" character varying(30),
    "DateUpdated" timestamp without time zone DEFAULT now(),
    "UserUpdated" character varying(30),
    "Resource" uuid DEFAULT newid() NOT NULL
);
 (   DROP TABLE public."base_VirtualFolder";
       public         postgres    false    2289    2290    2291    7                       1259    245113    base_VirtualFolder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VirtualFolder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VirtualFolder_Id_seq";
       public       postgres    false    7    1794            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VirtualFolder_Id_seq" OWNED BY "base_VirtualFolder"."Id";
            public       postgres    false    1793            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VirtualFolder_Id_seq"', 66, true);
            public       postgres    false    1793            !           1259    255696    tims_Holiday    TABLE     #  CREATE TABLE "tims_Holiday" (
    "Id" integer NOT NULL,
    "Title" character varying(100) NOT NULL,
    "Description" text,
    "HolidayOption" integer NOT NULL,
    "FromDate" timestamp without time zone,
    "ToDate" timestamp without time zone,
    "Month" integer,
    "Day" integer,
    "DayOfWeek" integer,
    "WeekOfMonth" integer,
    "ActiveFlag" boolean NOT NULL,
    "CreatedDate" timestamp without time zone NOT NULL,
    "CreatedById" integer NOT NULL,
    "ModifiedDate" timestamp without time zone,
    "ModifiedByID" integer
);
 "   DROP TABLE public."tims_Holiday";
       public         postgres    false    7            "           1259    255705    tims_HolidayHistory    TABLE     {   CREATE TABLE "tims_HolidayHistory" (
    "Date" timestamp without time zone NOT NULL,
    "Name" character varying(200)
);
 )   DROP TABLE public."tims_HolidayHistory";
       public         postgres    false    7                        1259    255694    tims_Holiday_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_Holiday_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_Holiday_Id_seq";
       public       postgres    false    1825    7            �           0    0    tims_Holiday_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_Holiday_Id_seq" OWNED BY "tims_Holiday"."Id";
            public       postgres    false    1824            �           0    0    tims_Holiday_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"tims_Holiday_Id_seq"', 8, true);
            public       postgres    false    1824            *           1259    255849    tims_TimeLog    TABLE     <  CREATE TABLE "tims_TimeLog" (
    "Id" integer NOT NULL,
    "EmployeeId" bigint,
    "WorkScheduleId" integer,
    "PayrollId" integer,
    "ClockIn" timestamp without time zone NOT NULL,
    "ClockOut" timestamp without time zone,
    "ManualClockInFlag" boolean NOT NULL,
    "ManualClockOutFlag" boolean NOT NULL,
    "WorkTime" real NOT NULL,
    "LunchTime" real NOT NULL,
    "OvertimeBefore" real NOT NULL,
    "Reason" text,
    "DeductLunchTimeFlag" boolean NOT NULL,
    "LateTime" real,
    "LeaveEarlyTime" real,
    "ActiveFlag" boolean NOT NULL,
    "ModifiedDate" timestamp without time zone,
    "ModifiedById" integer,
    "OvertimeAfter" real NOT NULL,
    "OvertimeLunch" real NOT NULL,
    "OvertimeDayOff" real NOT NULL,
    "OvertimeOptions" integer NOT NULL,
    "GuestResource" character varying(36)
);
 "   DROP TABLE public."tims_TimeLog";
       public         postgres    false    7            ,           1259    255865    tims_TimeLogPermission    TABLE     u   CREATE TABLE "tims_TimeLogPermission" (
    "TimeLogId" integer NOT NULL,
    "WorkPermissionId" integer NOT NULL
);
 ,   DROP TABLE public."tims_TimeLogPermission";
       public         postgres    false    7            +           1259    255863 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE     �   CREATE SEQUENCE "tims_TimeLogPermission_TimeLogId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 =   DROP SEQUENCE public."tims_TimeLogPermission_TimeLogId_seq";
       public       postgres    false    7    1836            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE "tims_TimeLogPermission_TimeLogId_seq" OWNED BY "tims_TimeLogPermission"."TimeLogId";
            public       postgres    false    1835            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('"tims_TimeLogPermission_TimeLogId_seq"', 1, false);
            public       postgres    false    1835            )           1259    255847    tims_TimeLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_TimeLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_TimeLog_Id_seq";
       public       postgres    false    1834    7            �           0    0    tims_TimeLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_TimeLog_Id_seq" OWNED BY "tims_TimeLog"."Id";
            public       postgres    false    1833            �           0    0    tims_TimeLog_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"tims_TimeLog_Id_seq"', 8, true);
            public       postgres    false    1833            (           1259    255795    tims_WorkPermission    TABLE     Z  CREATE TABLE "tims_WorkPermission" (
    "Id" integer NOT NULL,
    "EmployeeId" bigint NOT NULL,
    "PermissionType" integer NOT NULL,
    "FromDate" timestamp without time zone NOT NULL,
    "ToDate" timestamp without time zone NOT NULL,
    "Note" text,
    "NoOfDays" smallint NOT NULL,
    "HourPerDay" real NOT NULL,
    "PaidFlag" boolean NOT NULL,
    "ActiveFlag" boolean NOT NULL,
    "CreatedDate" timestamp without time zone NOT NULL,
    "CreatedById" integer NOT NULL,
    "ModifiedDate" timestamp without time zone,
    "ModifiedById" integer,
    "OvertimeOptions" integer NOT NULL
);
 )   DROP TABLE public."tims_WorkPermission";
       public         postgres    false    7            '           1259    255793    tims_WorkPermission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "tims_WorkPermission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."tims_WorkPermission_Id_seq";
       public       postgres    false    7    1832            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "tims_WorkPermission_Id_seq" OWNED BY "tims_WorkPermission"."Id";
            public       postgres    false    1831            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"tims_WorkPermission_Id_seq"', 6, true);
            public       postgres    false    1831            $           1259    255738    tims_WorkSchedule    TABLE     �  CREATE TABLE "tims_WorkSchedule" (
    "Id" integer NOT NULL,
    "WorkScheduleName" character varying(200) NOT NULL,
    "WorkScheduleType" integer NOT NULL,
    "Rotate" integer NOT NULL,
    "Status" integer NOT NULL,
    "CreatedDate" timestamp without time zone NOT NULL,
    "CreatedById" integer NOT NULL,
    "ModifiedDate" timestamp without time zone,
    "ModifiedById" integer
);
 '   DROP TABLE public."tims_WorkSchedule";
       public         postgres    false    7            #           1259    255736    tims_WorkSchedule_Id_seq    SEQUENCE     |   CREATE SEQUENCE "tims_WorkSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."tims_WorkSchedule_Id_seq";
       public       postgres    false    7    1828            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "tims_WorkSchedule_Id_seq" OWNED BY "tims_WorkSchedule"."Id";
            public       postgres    false    1827            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"tims_WorkSchedule_Id_seq"', 28, true);
            public       postgres    false    1827            &           1259    255781    tims_WorkWeek    TABLE     �  CREATE TABLE "tims_WorkWeek" (
    "Id" integer NOT NULL,
    "WorkScheduleId" integer NOT NULL,
    "Week" integer NOT NULL,
    "Day" integer NOT NULL,
    "WorkIn" timestamp without time zone NOT NULL,
    "WorkOut" timestamp without time zone NOT NULL,
    "LunchOut" timestamp without time zone,
    "LunchIn" timestamp without time zone,
    "LunchBreakFlag" boolean NOT NULL
);
 #   DROP TABLE public."tims_WorkWeek";
       public         postgres    false    7            %           1259    255779    tims_WorkWeek_Id_seq    SEQUENCE     x   CREATE SEQUENCE "tims_WorkWeek_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."tims_WorkWeek_Id_seq";
       public       postgres    false    1830    7            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "tims_WorkWeek_Id_seq" OWNED BY "tims_WorkWeek"."Id";
            public       postgres    false    1829            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"tims_WorkWeek_Id_seq"', 187, true);
            public       postgres    false    1829            ~           2604    235589    jexid    DEFAULT     g   ALTER TABLE pga_exception ALTER COLUMN jexid SET DEFAULT nextval('pga_exception_jexid_seq'::regclass);
 C   ALTER TABLE pgagent.pga_exception ALTER COLUMN jexid DROP DEFAULT;
       pgagent       postgres    false    1752    1751            �           2604    235590    jobid    DEFAULT     [   ALTER TABLE pga_job ALTER COLUMN jobid SET DEFAULT nextval('pga_job_jobid_seq'::regclass);
 =   ALTER TABLE pgagent.pga_job ALTER COLUMN jobid DROP DEFAULT;
       pgagent       postgres    false    1754    1753            �           2604    235591    jclid    DEFAULT     e   ALTER TABLE pga_jobclass ALTER COLUMN jclid SET DEFAULT nextval('pga_jobclass_jclid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_jobclass ALTER COLUMN jclid DROP DEFAULT;
       pgagent       postgres    false    1757    1756            �           2604    235592    jlgid    DEFAULT     a   ALTER TABLE pga_joblog ALTER COLUMN jlgid SET DEFAULT nextval('pga_joblog_jlgid_seq'::regclass);
 @   ALTER TABLE pgagent.pga_joblog ALTER COLUMN jlgid DROP DEFAULT;
       pgagent       postgres    false    1759    1758            �           2604    235593    jstid    DEFAULT     c   ALTER TABLE pga_jobstep ALTER COLUMN jstid SET DEFAULT nextval('pga_jobstep_jstid_seq'::regclass);
 A   ALTER TABLE pgagent.pga_jobstep ALTER COLUMN jstid DROP DEFAULT;
       pgagent       postgres    false    1761    1760            �           2604    235594    jslid    DEFAULT     i   ALTER TABLE pga_jobsteplog ALTER COLUMN jslid SET DEFAULT nextval('pga_jobsteplog_jslid_seq'::regclass);
 D   ALTER TABLE pgagent.pga_jobsteplog ALTER COLUMN jslid DROP DEFAULT;
       pgagent       postgres    false    1763    1762            �           2604    235595    jscid    DEFAULT     e   ALTER TABLE pga_schedule ALTER COLUMN jscid SET DEFAULT nextval('pga_schedule_jscid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_schedule ALTER COLUMN jscid DROP DEFAULT;
       pgagent       postgres    false    1765    1764            �           2604    244949    Id    DEFAULT     k   ALTER TABLE "base_Attachment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Attachment_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Attachment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1783    1784    1784            a	           2604    256171    Id    DEFAULT     i   ALTER TABLE "base_Authorize" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Authorize_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Authorize" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1838    1839    1839            K	           2604    257304    Id    DEFAULT     q   ALTER TABLE "base_Configuration" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Configuration_Id_seq"'::regclass);
 H   ALTER TABLE public."base_Configuration" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1844    1819            /	           2604    245757    Id    DEFAULT     s   ALTER TABLE "base_CostAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustment_Id_seq"'::regclass);
 I   ALTER TABLE public."base_CostAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1814    1813    1814            6	           2604    245769    Id    DEFAULT     {   ALTER TABLE "base_CostAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustmentItem_Id_seq"'::regclass);
 M   ALTER TABLE public."base_CostAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1816    1815    1816            
           2604    271741    Id    DEFAULT     k   ALTER TABLE "base_CountStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStock_Id_seq"'::regclass);
 E   ALTER TABLE public."base_CountStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1887    1888    1888            	
           2604    271748    Id    DEFAULT     w   ALTER TABLE "base_CountStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStockDetail_Id_seq"'::regclass);
 K   ALTER TABLE public."base_CountStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1890    1889    1890            	           2604    245343    Id    DEFAULT     k   ALTER TABLE "base_Department" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Department_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Department" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1803    1804    1804            �           2604    244820    Id    DEFAULT     a   ALTER TABLE "base_Guest" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Guest_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Guest" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1771    1772    1772            	           2604    245379    Id    DEFAULT     u   ALTER TABLE "base_GuestAdditional" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAdditional_Id_seq"'::regclass);
 J   ALTER TABLE public."base_GuestAdditional" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1806    1805    1806            �           2604    244866    Id    DEFAULT     o   ALTER TABLE "base_GuestAddress" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAddress_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestAddress" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1774    1773    1774            �           2604    238416    Id    DEFAULT     w   ALTER TABLE "base_GuestFingerPrint" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestFingerPrint_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestFingerPrint" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1768    1769    1769            �           2604    244876    Id    DEFAULT     {   ALTER TABLE "base_GuestHiringHistory" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestHiringHistory_Id_seq"'::regclass);
 M   ALTER TABLE public."base_GuestHiringHistory" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1775    1776    1776            �           2604    244887    Id    DEFAULT     o   ALTER TABLE "base_GuestPayRoll" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPayRoll_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestPayRoll" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1778    1777    1778            g	           2604    257328    Id    DEFAULT     w   ALTER TABLE "base_GuestPaymentCard" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPaymentCard_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestPaymentCard" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1845    1846    1846            �           2604    244937    Id    DEFAULT     o   ALTER TABLE "base_GuestProfile" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestProfile_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestProfile" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1781    1782    1782            �	           2604    268357    Id    DEFAULT     m   ALTER TABLE "base_GuestReward" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestReward_Id_seq"'::regclass);
 F   ALTER TABLE public."base_GuestReward" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1870    1869    1870            �           2604    245000    Id    DEFAULT     k   ALTER TABLE "base_MemberShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_MemberShip_Id_seq"'::regclass);
 E   ALTER TABLE public."base_MemberShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1786    1785    1786            �	           2604    268514    Id    DEFAULT     q   ALTER TABLE "base_PricingChange" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingChange_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PricingChange" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1871    1872    1872            �	           2604    268188    Id    DEFAULT     s   ALTER TABLE "base_PricingManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingManager_Id_seq"'::regclass);
 I   ALTER TABLE public."base_PricingManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1867    1868    1868            	           2604    245415    Id    DEFAULT     e   ALTER TABLE "base_Product" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Product_Id_seq"'::regclass);
 B   ALTER TABLE public."base_Product" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1807    1808    1808            X	           2604    255539    Id    DEFAULT     o   ALTER TABLE "base_ProductStore" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductStore_Id_seq"'::regclass);
 G   ALTER TABLE public."base_ProductStore" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1820    1821    1821            �	           2604    270255    Id    DEFAULT     k   ALTER TABLE "base_ProductUOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductUOM_Id_seq"'::regclass);
 E   ALTER TABLE public."base_ProductUOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1885    1886    1886            	           2604    245172    Id    DEFAULT     i   ALTER TABLE "base_Promotion" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Promotion_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Promotion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1801    1802    1802            �           2604    245158    Id    DEFAULT     u   ALTER TABLE "base_PromotionAffect" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionAffect_Id_seq"'::regclass);
 J   ALTER TABLE public."base_PromotionAffect" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1800    1799    1800            �           2604    245026    Id    DEFAULT     y   ALTER TABLE "base_PromotionSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionSchedule_Id_seq"'::regclass);
 L   ALTER TABLE public."base_PromotionSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1788    1787    1788            �	           2604    266554    Id    DEFAULT     q   ALTER TABLE "base_PurchaseOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PurchaseOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1859    1860    1860            �	           2604    266533    Id    DEFAULT     }   ALTER TABLE "base_PurchaseOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_PurchaseOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1857    1858    1858            �	           2604    267538    Id    DEFAULT        ALTER TABLE "base_PurchaseOrderReceive" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderReceive_Id_seq"'::regclass);
 O   ALTER TABLE public."base_PurchaseOrderReceive" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1866    1865    1866            '	           2604    245736    Id    DEFAULT     {   ALTER TABLE "base_QuantityAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustment_Id_seq"'::regclass);
 M   ALTER TABLE public."base_QuantityAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1810    1809    1810            -	           2604    245748    Id    DEFAULT     �   ALTER TABLE "base_QuantityAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustmentItem_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_QuantityAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1812    1811    1812            b	           2604    256181    Id    DEFAULT     u   ALTER TABLE "base_ResourceAccount" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceAccount_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourceAccount" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1840    1841    1841            ;	           2604    246086    Id    DEFAULT     o   ALTER TABLE "base_ResourceNote" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceNote_id_seq"'::regclass);
 G   ALTER TABLE public."base_ResourceNote" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1817    1818    1818            �	           2604    270153    Id    DEFAULT     u   ALTER TABLE "base_ResourcePayment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePayment_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourcePayment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1882    1881    1882            �	           2604    270075    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentDetail_Id_seq"'::regclass);
 P   ALTER TABLE public."base_ResourcePaymentDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1880    1879    1880            
           2604    272125    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentProduct_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_ResourcePaymentProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1894    1895    1895            �           2604    244925    Id    DEFAULT     n   ALTER TABLE "base_ResourcePhoto" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPhoto_Id_seq"'::regclass);
 H   ALTER TABLE public."base_ResourcePhoto" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1780    1779    1780            �	           2604    270196    Id    DEFAULT     s   ALTER TABLE "base_ResourceReturn" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturn_Id_seq"'::regclass);
 I   ALTER TABLE public."base_ResourceReturn" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1884    1883    1884            
           2604    272102    Id    DEFAULT        ALTER TABLE "base_ResourceReturnDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturnDetail_Id_seq"'::regclass);
 O   ALTER TABLE public."base_ResourceReturnDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1892    1893    1893            �	           2604    266846    Id    DEFAULT     q   ALTER TABLE "base_RewardManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_RewardManager_Id_seq"'::regclass);
 H   ALTER TABLE public."base_RewardManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1863    1864    1864            �	           2604    266609    Id    DEFAULT     s   ALTER TABLE "base_SaleCommission" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleCommission_Id_seq"'::regclass);
 I   ALTER TABLE public."base_SaleCommission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1861    1862    1862            x	           2604    266096    Id    DEFAULT     i   ALTER TABLE "base_SaleOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrder_Id_seq"'::regclass);
 D   ALTER TABLE public."base_SaleOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1850    1849    1850            s	           2604    266087    Id    DEFAULT     u   ALTER TABLE "base_SaleOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderDetail_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1847    1848    1848            �	           2604    266239    Id    DEFAULT     w   ALTER TABLE "base_SaleOrderInvoice" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderInvoice_Id_seq"'::regclass);
 K   ALTER TABLE public."base_SaleOrderInvoice" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1853    1854    1854            �	           2604    266183    Id    DEFAULT     q   ALTER TABLE "base_SaleOrderShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShip_Id_seq"'::regclass);
 H   ALTER TABLE public."base_SaleOrderShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1852    1851    1852            �	           2604    266360    Id    DEFAULT     }   ALTER TABLE "base_SaleOrderShipDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShipDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_SaleOrderShipDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1855    1856    1856            �           2604    245106    Id    DEFAULT     u   ALTER TABLE "base_SaleTaxLocation" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocation_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleTaxLocation" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1791    1792    1792            �           2604    245087    Id    DEFAULT     �   ALTER TABLE "base_SaleTaxLocationOption" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocationOption_Id_seq"'::regclass);
 P   ALTER TABLE public."base_SaleTaxLocationOption" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1790    1789    1790            Z	           2604    255678    Id    DEFAULT     a   ALTER TABLE "base_Store" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Store_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Store" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1823    1822    1823            �	           2604    269928    Id    DEFAULT     q   ALTER TABLE "base_TransferStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStock_Id_seq"'::regclass);
 H   ALTER TABLE public."base_TransferStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1875    1876    1876            �	           2604    269944    Id    DEFAULT     }   ALTER TABLE "base_TransferStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStockDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_TransferStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1877    1878    1878            �           2604    245150    Id    DEFAULT     ]   ALTER TABLE "base_UOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UOM_Id_seq"'::regclass);
 >   ALTER TABLE public."base_UOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1797    1798    1798            �           2604    245134    Id    DEFAULT     e   ALTER TABLE "base_UserLog" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserLog_Id_seq"'::regclass);
 B   ALTER TABLE public."base_UserLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1795    1796    1796            f	           2604    256247    Id    DEFAULT     i   ALTER TABLE "base_UserRight" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserRight_Id_seq"'::regclass);
 D   ALTER TABLE public."base_UserRight" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1843    1842    1843            �	           2604    269648    Id    DEFAULT     q   ALTER TABLE "base_VendorProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VendorProduct_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VendorProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1874    1873            �           2604    245118    Id    DEFAULT     q   ALTER TABLE "base_VirtualFolder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VirtualFolder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VirtualFolder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1794    1793    1794            [	           2604    255699    Id    DEFAULT     e   ALTER TABLE "tims_Holiday" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_Holiday_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_Holiday" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1825    1824    1825            _	           2604    255852    Id    DEFAULT     e   ALTER TABLE "tims_TimeLog" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_TimeLog_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_TimeLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1834    1833    1834            `	           2604    255868 	   TimeLogId    DEFAULT     �   ALTER TABLE "tims_TimeLogPermission" ALTER COLUMN "TimeLogId" SET DEFAULT nextval('"tims_TimeLogPermission_TimeLogId_seq"'::regclass);
 S   ALTER TABLE public."tims_TimeLogPermission" ALTER COLUMN "TimeLogId" DROP DEFAULT;
       public       postgres    false    1835    1836    1836            ^	           2604    255798    Id    DEFAULT     s   ALTER TABLE "tims_WorkPermission" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkPermission_Id_seq"'::regclass);
 I   ALTER TABLE public."tims_WorkPermission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1831    1832    1832            \	           2604    255741    Id    DEFAULT     o   ALTER TABLE "tims_WorkSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkSchedule_Id_seq"'::regclass);
 G   ALTER TABLE public."tims_WorkSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1827    1828    1828            ]	           2604    255784    Id    DEFAULT     g   ALTER TABLE "tims_WorkWeek" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkWeek_Id_seq"'::regclass);
 C   ALTER TABLE public."tims_WorkWeek" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1829    1830    1830            I          0    234865    pga_exception 
   TABLE DATA               B   COPY pga_exception (jexid, jexscid, jexdate, jextime) FROM stdin;
    pgagent       postgres    false    1751   ��      J          0    234870    pga_job 
   TABLE DATA               �   COPY pga_job (jobid, jobjclid, jobname, jobdesc, jobhostagent, jobenabled, jobcreated, jobchanged, jobagentid, jobnextrun, joblastrun) FROM stdin;
    pgagent       postgres    false    1753   ȳ      K          0    234883    pga_jobagent 
   TABLE DATA               A   COPY pga_jobagent (jagpid, jaglogintime, jagstation) FROM stdin;
    pgagent       postgres    false    1755   �      L          0    234890    pga_jobclass 
   TABLE DATA               /   COPY pga_jobclass (jclid, jclname) FROM stdin;
    pgagent       postgres    false    1756   �      M          0    234898 
   pga_joblog 
   TABLE DATA               P   COPY pga_joblog (jlgid, jlgjobid, jlgstatus, jlgstart, jlgduration) FROM stdin;
    pgagent       postgres    false    1758   j�      N          0    234906    pga_jobstep 
   TABLE DATA               �   COPY pga_jobstep (jstid, jstjobid, jstname, jstdesc, jstenabled, jstkind, jstcode, jstconnstr, jstdbname, jstonerror, jscnextrun) FROM stdin;
    pgagent       postgres    false    1760   ��      O          0    234923    pga_jobsteplog 
   TABLE DATA               t   COPY pga_jobsteplog (jslid, jsljlgid, jsljstid, jslstatus, jslresult, jslstart, jslduration, jsloutput) FROM stdin;
    pgagent       postgres    false    1762   ��      P          0    234934    pga_schedule 
   TABLE DATA               �   COPY pga_schedule (jscid, jscjobid, jscname, jscdesc, jscenabled, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths) FROM stdin;
    pgagent       postgres    false    1764   ��      [          0    244946    base_Attachment 
   TABLE DATA               �   COPY "base_Attachment" ("Id", "FileOriginalName", "FileName", "FileExtension", "VirtualFolderId", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Counter") FROM stdin;
    public       postgres    false    1784   ޴      x          0    256168    base_Authorize 
   TABLE DATA               =   COPY "base_Authorize" ("Id", "Resource", "Code") FROM stdin;
    public       postgres    false    1839   ��      m          0    254557    base_Configuration 
   TABLE DATA               *  COPY "base_Configuration" ("CompanyName", "Address", "City", "State", "ZipCode", "CountryId", "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", "IsAllowRGO", "PasswordLength", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod") FROM stdin;
    public       postgres    false    1819   �      j          0    245754    base_CostAdjustment 
   TABLE DATA               �   COPY "base_CostAdjustment" ("Id", "Resource", "CostDifference", "NewCost", "OldCost", "ItemCount", "LoggedTime", "Reason", "StoreNumber") FROM stdin;
    public       postgres    false    1814   E�      k          0    245766    base_CostAdjustmentItem 
   TABLE DATA               �   COPY "base_CostAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewCost", "AdjustmentOldCost", "LoggedTime", "ParentResource") FROM stdin;
    public       postgres    false    1816   b�      �          0    271738    base_CountStock 
   TABLE DATA               �   COPY "base_CountStock" ("Id", "DocumentNo", "DateCreated", "UserCreated", "CompletedDate", "UserCounted", "Status", "Resource") FROM stdin;
    public       postgres    false    1888   �      �          0    271745    base_CountStockDetail 
   TABLE DATA               �   COPY "base_CountStockDetail" ("Id", "CountStockId", "ProductId", "ProductResource", "StoreId", "Quantity", "CountedQuantity") FROM stdin;
    public       postgres    false    1890   ��      e          0    245340    base_Department 
   TABLE DATA               �   COPY "base_Department" ("Id", "Name", "ParentId", "TaxCodeId", "Margin", "MarkUp", "LevelId", "IsActived", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated") FROM stdin;
    public       postgres    false    1804   ��      R          0    238237 
   base_Email 
   TABLE DATA               �  COPY "base_Email" ("Id", "Recipient", "CC", "BCC", "Subject", "Body", "IsHasAttachment", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "AttachmentType", "AttachmentResult", "GuestId", "Sender", "Status", "Importance", "Sensitivity", "IsRequestDelivery", "IsRequestRead", "IsMyFlag", "FlagTo", "FlagStartDate", "FlagDueDate", "IsAllowReminder", "RemindOn", "MyRemindTimes", "IsRecipentFlag", "RecipentFlagTo", "IsAllowRecipentReminder", "RecipentRemindOn", "RecipentRemindTimes") FROM stdin;
    public       postgres    false    1767   #�      Q          0    238137    base_EmailAttachment 
   TABLE DATA               J   COPY "base_EmailAttachment" ("Id", "EmailId", "AttachmentId") FROM stdin;
    public       postgres    false    1766   @�      U          0    244817 
   base_Guest 
   TABLE DATA                 COPY "base_Guest" ("Id", "FirstName", "MiddleName", "LastName", "Company", "Phone1", "Ext1", "Phone2", "Ext2", "Fax", "CellPhone", "Email", "Website", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "IsPurged", "GuestTypeId", "IsActived", "GuestNo", "PositionId", "Department", "Mark", "AccountNumber", "ParentId", "IsRewardMember", "CheckLimit", "CreditLimit", "BalanceDue", "AvailCredit", "PastDue", "IsPrimary", "CommissionPercent", "Resource", "TotalRewardRedeemed", "PurchaseDuringTrackingPeriod", "RequirePurchaseNextReward", "HireDate", "IsBlockArriveLate", "IsDeductLunchTime", "IsBalanceOvertime", "LateMinutes", "OvertimeOption", "OTLeastMinute", "IsTrackingHour", "TermDiscount", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "SaleRepId") FROM stdin;
    public       postgres    false    1772   ]�      f          0    245376    base_GuestAdditional 
   TABLE DATA               3  COPY "base_GuestAdditional" ("Id", "TaxRate", "IsNoDiscount", "FixDiscount", "Unit", "PriceSchemeId", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Custom8", "GuestId", "LayawayNo", "ChargeACNo", "FedTaxId", "IsTaxExemption", "SaleTaxLocation", "TaxExemptionNo") FROM stdin;
    public       postgres    false    1806   ��      V          0    244863    base_GuestAddress 
   TABLE DATA               �   COPY "base_GuestAddress" ("Id", "GuestId", "AddressTypeId", "AddressLine1", "AddressLine2", "City", "StateProvinceId", "PostalCode", "CountryId", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsDefault") FROM stdin;
    public       postgres    false    1774   ��      S          0    238413    base_GuestFingerPrint 
   TABLE DATA               �   COPY "base_GuestFingerPrint" ("Id", "GuestId", "FingerIndex", "HandFlag", "DateUpdated", "UserUpdaed", "FingerPrintImage") FROM stdin;
    public       postgres    false    1769   ;�      W          0    244873    base_GuestHiringHistory 
   TABLE DATA               �   COPY "base_GuestHiringHistory" ("Id", "GuestId", "StartDate", "RenewDate", "PromotionDate", "TerminateDate", "IsTerminate", "ManagerId") FROM stdin;
    public       postgres    false    1776   ��      X          0    244884    base_GuestPayRoll 
   TABLE DATA               �   COPY "base_GuestPayRoll" ("Id", "PayrollName", "PayrollType", "Rate", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "GuestId") FROM stdin;
    public       postgres    false    1778   ��      {          0    257325    base_GuestPaymentCard 
   TABLE DATA               �   COPY "base_GuestPaymentCard" ("Id", "GuestId", "CardTypeId", "CardNumber", "ExpMonth", "ExpYear", "CCID", "BillingAddress", "NameOnCard", "ZipCode", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1846   �      Z          0    244934    base_GuestProfile 
   TABLE DATA               s  COPY "base_GuestProfile" ("Id", "Gender", "Marital", "SSN", "Identification", "DOB", "IsSpouse", "FirstName", "LastName", "MiddleName", "State", "SGender", "SFirstName", "SLastName", "SMiddleName", "SPhone", "SCellPhone", "SSSN", "SState", "SEmail", "IsEmergency", "EFirstName", "ELastName", "EMiddleName", "EPhone", "ECellPhone", "ERelationship", "GuestId") FROM stdin;
    public       postgres    false    1782   0�      �          0    268354    base_GuestReward 
   TABLE DATA               �   COPY "base_GuestReward" ("Id", "GuestId", "RewardId", "Amount", "IsApply", "EearnedDate", "RedeemedDate", "RewardValue", "SaleOrderResource", "SaleOrderNo", "Remark") FROM stdin;
    public       postgres    false    1870   ��      w          0    256013    base_GuestSchedule 
   TABLE DATA               i   COPY "base_GuestSchedule" ("GuestId", "WorkScheduleId", "StartDate", "AssignDate", "Status") FROM stdin;
    public       postgres    false    1837   �      \          0    244997    base_MemberShip 
   TABLE DATA               �   COPY "base_MemberShip" ("Id", "GuestId", "MemberType", "CardNumber", "Status", "IsPurged", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "Code", "TotalRewardRedeemed") FROM stdin;
    public       postgres    false    1786   M      �          0    268511    base_PricingChange 
   TABLE DATA               �   COPY "base_PricingChange" ("Id", "PricingManagerId", "PricingManagerResource", "ProductId", "ProductResource", "Cost", "CurrentPrice", "NewPrice", "PriceChanged", "DateCreated") FROM stdin;
    public       postgres    false    1872   j      �          0    268185    base_PricingManager 
   TABLE DATA               +  COPY "base_PricingManager" ("Id", "Name", "Description", "DateCreated", "UserCreated", "DateApplied", "UserApplied", "DateRestored", "UserRestored", "AffectPricing", "Resource", "PriceLevel", "Status", "BasePrice", "CalculateMethod", "AmountChange", "AmountUnit", "ItemCount", "Reason") FROM stdin;
    public       postgres    false    1868   �      g          0    245412    base_Product 
   TABLE DATA               �  COPY "base_Product" ("Id", "Code", "ItemTypeId", "ProductDepartmentId", "ProductCategoryId", "ProductBrandId", "StyleModel", "ProductName", "Description", "Barcode", "Attribute", "Size", "IsSerialTracking", "IsPublicWeb", "OnHandStore1", "OnHandStore2", "OnHandStore3", "OnHandStore4", "OnHandStore5", "OnHandStore6", "OnHandStore7", "OnHandStore8", "OnHandStore9", "OnHandStore10", "QuantityOnHand", "QuantityOnOrder", "CompanyReOrderPoint", "IsUnOrderAble", "IsEligibleForCommission", "IsEligibleForReward", "RegularPrice", "Price1", "Price2", "Price3", "Price4", "OrderCost", "AverageUnitCost", "TaxCode", "MarginPercent", "MarkupPercent", "BaseUOMId", "GroupAttribute", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Resource", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "WarrantyType", "WarrantyNumber", "WarrantyPeriod", "PartNumber", "SellUOMId", "OrderUOMId", "IsPurge", "VendorId", "UserAssignedCommission", "AssignedCommissionPercent", "AssignedCommissionAmount", "Serial", "OrderUOM", "MarkdownPercent1", "MarkdownPercent2", "MarkdownPercent3", "MarkdownPercent4", "IsOpenItem", "Location") FROM stdin;
    public       postgres    false    1808   �      n          0    255536    base_ProductStore 
   TABLE DATA               X   COPY "base_ProductStore" ("Id", "ProductId", "QuantityOnHand", "StoreCode") FROM stdin;
    public       postgres    false    1821   �      �          0    270252    base_ProductUOM 
   TABLE DATA               /  COPY "base_ProductUOM" ("Id", "ProductStoreId", "ProductId", "UOMId", "BaseUnitNumber", "RegularPrice", "QuantityOnHand", "AverageCost", "Price1", "Price2", "Price3", "Price4", "MarkDownPercent1", "MarkDownPercent2", "MarkDownPercent3", "MarkDownPercent4", "MarginPercent", "MarkupPercent") FROM stdin;
    public       postgres    false    1886   �       d          0    245169    base_Promotion 
   TABLE DATA               �  COPY "base_Promotion" ("Id", "Name", "Description", "PromotionTypeId", "TakeOffOption", "TakeOff", "BuyingQty", "GetingValue", "IsApplyToAboveQuantities", "Status", "AffectDiscount", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource", "CouponExpire", "IsCouponExpired", "PriceSchemaRange", "ReasonReActive", "Sold", "TotalPrice", "CategoryId", "VendorId", "CouponBarCode", "BarCodeNumber", "BarCodeImage") FROM stdin;
    public       postgres    false    1802   t"      c          0    245155    base_PromotionAffect 
   TABLE DATA               �   COPY "base_PromotionAffect" ("Id", "PromotionId", "ItemId", "Price1", "Price2", "Price3", "Price4", "Price5", "Discount1", "Discount2", "Discount3", "Discount4", "Discount5") FROM stdin;
    public       postgres    false    1800   V#      ]          0    245023    base_PromotionSchedule 
   TABLE DATA               X   COPY "base_PromotionSchedule" ("Id", "PromotionId", "EndDate", "StartDate") FROM stdin;
    public       postgres    false    1788   s#      �          0    266551    base_PurchaseOrder 
   TABLE DATA               B  COPY "base_PurchaseOrder" ("Id", "PurchaseOrderNo", "VendorId", "VendorCode", "Status", "ShipAddress", "PurchasedDate", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "PaymentDueDate", "PaymentMethodId", "Remark", "ShipDate", "SubTotal", "DiscountPercent", "DiscountAmount", "Freight", "Fee", "Total", "Paid", "Balance", "ItemCount", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "DateUpdate", "UserUpdated", "Resource", "CancelDate", "IsFullWorkflow", "StoreCode", "RecRemark", "PaymentName") FROM stdin;
    public       postgres    false    1860   �#      �          0    266530    base_PurchaseOrderDetail 
   TABLE DATA                  COPY "base_PurchaseOrderDetail" ("Id", "PurchaseOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "ReceivedQty", "DueQty", "UnFilledQty", "Amount", "Serial", "LastReceived", "Resource", "IsFullReceived") FROM stdin;
    public       postgres    false    1858   [&      �          0    267535    base_PurchaseOrderReceive 
   TABLE DATA               �   COPY "base_PurchaseOrderReceive" ("Id", "PurchaseOrderDetailId", "POResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "RecQty", "IsReceived", "ReceiveDate", "Resource", "Price") FROM stdin;
    public       postgres    false    1866   �(      h          0    245733    base_QuantityAdjustment 
   TABLE DATA               �   COPY "base_QuantityAdjustment" ("Id", "Resource", "CostDifference", "NewQuantity", "OldQuantity", "ItemCount", "LoggedTime", "Reason", "StoreNumber") FROM stdin;
    public       postgres    false    1810   ,      i          0    245745    base_QuantityAdjustmentItem 
   TABLE DATA               �   COPY "base_QuantityAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewQty", "AdjustmentOldQty", "AdjustmentQtyDiff", "LoggedTime", "ParentResource") FROM stdin;
    public       postgres    false    1812   9,      y          0    256178    base_ResourceAccount 
   TABLE DATA               �   COPY "base_ResourceAccount" ("Id", "Resource", "UserResource", "LoginName", "Password", "ExpiredDate", "IsLocked", "IsExpired") FROM stdin;
    public       postgres    false    1841   V,      l          0    246083    base_ResourceNote 
   TABLE DATA               X   COPY "base_ResourceNote" ("Id", "Note", "DateCreated", "Color", "Resource") FROM stdin;
    public       postgres    false    1818   $.      �          0    270150    base_ResourcePayment 
   TABLE DATA               (  COPY "base_ResourcePayment" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalPaid", "Balance", "Change", "DateCreated", "UserCreated", "Remark", "Resource", "SubTotal", "DiscountPercent", "DiscountAmount", "Mark", "IsDeposit", "TaxCode", "TaxAmount", "LastRewardAmount") FROM stdin;
    public       postgres    false    1882   C0      �          0    270072    base_ResourcePaymentDetail 
   TABLE DATA               �   COPY "base_ResourcePaymentDetail" ("Id", "PaymentType", "ResourcePaymentId", "PaymentMethodId", "PaymentMethod", "CardType", "Paid", "Change", "Tip", "GiftCardNo", "Reason", "Reference") FROM stdin;
    public       postgres    false    1880   �5      �          0    272122    base_ResourcePaymentProduct 
   TABLE DATA               �   COPY "base_ResourcePaymentProduct" ("Id", "ResourcePaymentId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "Amount") FROM stdin;
    public       postgres    false    1895   97      Y          0    244922    base_ResourcePhoto 
   TABLE DATA               �   COPY "base_ResourcePhoto" ("Id", "ThumbnailPhoto", "ThumbnailPhotoFilename", "LargePhoto", "LargePhotoFilename", "SortId", "Resource") FROM stdin;
    public       postgres    false    1780   �8      �          0    270193    base_ResourceReturn 
   TABLE DATA               �   COPY "base_ResourceReturn" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalRefund", "Balance", "DateCreated", "UserCreated", "Resource", "Mark", "DiscountPercent", "DiscountAmount", "Freight", "SubTotal") FROM stdin;
    public       postgres    false    1884   c?      �          0    272099    base_ResourceReturnDetail 
   TABLE DATA               �   COPY "base_ResourceReturnDetail" ("Id", "ResourceReturnId", "OrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Price", "ReturnQty", "Amount", "IsReturned", "ReturnedDate") FROM stdin;
    public       postgres    false    1893   nK      �          0    266843    base_RewardManager 
   TABLE DATA               �  COPY "base_RewardManager" ("Id", "StoreCode", "PurchaseThreshold", "RewardAmount", "RewardAmtType", "RewardExpiration", "IsAutoEnroll", "IsPromptEnroll", "IsInformCashier", "IsRedemptionLimit", "RedemptionLimitAmount", "IsBlockRedemption", "RedemptionAfterDays", "IsBlockPurchaseRedeem", "IsTrackingPeriod", "StartDate", "EndDate", "IsNoEndDay", "TotalRewardRedeemed", "IsActived", "ReasonReActive", "DateCreated") FROM stdin;
    public       postgres    false    1864   IN      �          0    266606    base_SaleCommission 
   TABLE DATA               �   COPY "base_SaleCommission" ("Id", "GuestResource", "SOResource", "SONumber", "SOTotal", "SODate", "ComissionPercent", "CommissionAmount", "Sign", "Remark") FROM stdin;
    public       postgres    false    1862   �N      }          0    266093    base_SaleOrder 
   TABLE DATA               7  COPY "base_SaleOrder" ("Id", "SONumber", "OrderDate", "OrderStatus", "BillAddressId", "BillAddress", "ShipAddressId", "ShipAddress", "PromotionCode", "SaleRep", "CustomerResource", "PriceSchemaId", "DueDate", "RequestShipDate", "SubTotal", "TaxLocation", "TaxCode", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "Paid", "Balance", "RefundFee", "IsMultiPayment", "Remark", "IsFullWorkflow", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "Resource", "BookingChanel", "ShippedCount", "Deposit", "Transaction", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "IsTaxExemption", "TaxExemption", "ShippedBox", "PackedQty", "TotalWeight", "WeightUnit", "StoreCode", "IsRedeeem") FROM stdin;
    public       postgres    false    1850   {b      |          0    266084    base_SaleOrderDetail 
   TABLE DATA               x  COPY "base_SaleOrderDetail" ("Id", "SaleOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "TaxCode", "Quantity", "PickQty", "DueQty", "UnFilled", "RegularPrice", "SalePrice", "UOMId", "BaseUOM", "DiscountPercent", "DiscountAmount", "SubTotal", "OnHandQty", "SerialTracking", "Resource", "BalanceShipped", "Comment", "TotalDiscount") FROM stdin;
    public       postgres    false    1848   �j                0    266236    base_SaleOrderInvoice 
   TABLE DATA               �   COPY "base_SaleOrderInvoice" ("Id", "InvoiceNo", "SaleOrderId", "SaleOrderResource", "ItemCount", "SubTotal", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "DateCreated") FROM stdin;
    public       postgres    false    1854   q      ~          0    266180    base_SaleOrderShip 
   TABLE DATA               �   COPY "base_SaleOrderShip" ("Id", "SaleOrderId", "SaleOrderResource", "Weight", "TrackingNo", "IsShipped", "Resource", "Remark", "Carrier", "ShipDate", "BoxNo") FROM stdin;
    public       postgres    false    1852   #q      �          0    266357    base_SaleOrderShipDetail 
   TABLE DATA               �   COPY "base_SaleOrderShipDetail" ("Id", "SaleOrderShipId", "SaleOrderShipResource", "SaleOrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Description", "SerialTracking", "PackedQty", "IsPaid") FROM stdin;
    public       postgres    false    1856   �v      _          0    245103    base_SaleTaxLocation 
   TABLE DATA               �   COPY "base_SaleTaxLocation" ("Id", "ParentId", "Name", "IsShipingTaxable", "ShippingTaxCodeId", "IsActived", "LevelId", "TaxCode", "TaxCodeName", "TaxPrintMark", "TaxOption", "IsPrimary", "SortIndex", "IsTaxAfterDiscount") FROM stdin;
    public       postgres    false    1792   �      ^          0    245084    base_SaleTaxLocationOption 
   TABLE DATA               �   COPY "base_SaleTaxLocationOption" ("Id", "SaleTaxLocationId", "ParentId", "TaxRate", "TaxComponent", "TaxAgency", "TaxCondition", "IsApplyAmountOver", "IsAllowSpecificItemPriceRange", "IsAllowAmountItemPriceRange", "PriceFrom", "PriceTo") FROM stdin;
    public       postgres    false    1790   8�      o          0    255675 
   base_Store 
   TABLE DATA               G   COPY "base_Store" ("Id", "Code", "Name", "Street", "City") FROM stdin;
    public       postgres    false    1823   ��      �          0    269925    base_TransferStock 
   TABLE DATA                 COPY "base_TransferStock" ("Id", "TransferNo", "FromStore", "ToStore", "TotalQuantity", "ShipDate", "Carier", "ShippingFee", "Comment", "Resource", "UserCreated", "DateCreated", "Status", "SubTotal", "Total", "DateApplied", "UserApplied", "DateReversed", "UserReversed") FROM stdin;
    public       postgres    false    1876   F�      �          0    269941    base_TransferStockDetail 
   TABLE DATA               �   COPY "base_TransferStockDetail" ("Id", "TransferStockId", "TransferStockResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Quantity", "UOMId", "BaseUOM", "Amount", "SerialTracking", "AvlQuantity") FROM stdin;
    public       postgres    false    1878   1�      b          0    245147    base_UOM 
   TABLE DATA               �   COPY "base_UOM" ("Id", "Code", "Name", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsActived", "Resource") FROM stdin;
    public       postgres    false    1798   �      a          0    245131    base_UserLog 
   TABLE DATA               y   COPY "base_UserLog" ("Id", "IpSource", "ConnectedOn", "DisConnectedOn", "ResourceAccessed", "IsDisconected") FROM stdin;
    public       postgres    false    1796   ��      T          0    244282    base_UserLogDetail 
   TABLE DATA               m   COPY "base_UserLogDetail" ("Id", "UserLogId", "AccessedTime", "ModuleName", "ActionDescription") FROM stdin;
    public       postgres    false    1770   Q�      z          0    256244    base_UserRight 
   TABLE DATA               9   COPY "base_UserRight" ("Id", "Code", "Name") FROM stdin;
    public       postgres    false    1843   �      �          0    269643    base_VendorProduct 
   TABLE DATA               t   COPY "base_VendorProduct" ("Id", "ProductId", "VendorId", "Price", "ProductResource", "VendorResource") FROM stdin;
    public       postgres    false    1873   ��      `          0    245115    base_VirtualFolder 
   TABLE DATA               �   COPY "base_VirtualFolder" ("Id", "ParentFolderId", "FolderName", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource") FROM stdin;
    public       postgres    false    1794   ��      p          0    255696    tims_Holiday 
   TABLE DATA               �   COPY "tims_Holiday" ("Id", "Title", "Description", "HolidayOption", "FromDate", "ToDate", "Month", "Day", "DayOfWeek", "WeekOfMonth", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedByID") FROM stdin;
    public       postgres    false    1825   ��      q          0    255705    tims_HolidayHistory 
   TABLE DATA               8   COPY "tims_HolidayHistory" ("Date", "Name") FROM stdin;
    public       postgres    false    1826   c�      u          0    255849    tims_TimeLog 
   TABLE DATA               �  COPY "tims_TimeLog" ("Id", "EmployeeId", "WorkScheduleId", "PayrollId", "ClockIn", "ClockOut", "ManualClockInFlag", "ManualClockOutFlag", "WorkTime", "LunchTime", "OvertimeBefore", "Reason", "DeductLunchTimeFlag", "LateTime", "LeaveEarlyTime", "ActiveFlag", "ModifiedDate", "ModifiedById", "OvertimeAfter", "OvertimeLunch", "OvertimeDayOff", "OvertimeOptions", "GuestResource") FROM stdin;
    public       postgres    false    1834   3�      v          0    255865    tims_TimeLogPermission 
   TABLE DATA               L   COPY "tims_TimeLogPermission" ("TimeLogId", "WorkPermissionId") FROM stdin;
    public       postgres    false    1836   �      t          0    255795    tims_WorkPermission 
   TABLE DATA               �   COPY "tims_WorkPermission" ("Id", "EmployeeId", "PermissionType", "FromDate", "ToDate", "Note", "NoOfDays", "HourPerDay", "PaidFlag", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById", "OvertimeOptions") FROM stdin;
    public       postgres    false    1832   :�      r          0    255738    tims_WorkSchedule 
   TABLE DATA               �   COPY "tims_WorkSchedule" ("Id", "WorkScheduleName", "WorkScheduleType", "Rotate", "Status", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById") FROM stdin;
    public       postgres    false    1828   ��      s          0    255781    tims_WorkWeek 
   TABLE DATA               �   COPY "tims_WorkWeek" ("Id", "WorkScheduleId", "Week", "Day", "WorkIn", "WorkOut", "LunchOut", "LunchIn", "LunchBreakFlag") FROM stdin;
    public       postgres    false    1830   ��      
           2606    235700    pga_exception_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_pkey PRIMARY KEY (jexid);
 K   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_pkey;
       pgagent         postgres    false    1751    1751            
           2606    235702    pga_job_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_pkey PRIMARY KEY (jobid);
 ?   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_pkey;
       pgagent         postgres    false    1753    1753            
           2606    235704    pga_jobagent_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY pga_jobagent
    ADD CONSTRAINT pga_jobagent_pkey PRIMARY KEY (jagpid);
 I   ALTER TABLE ONLY pgagent.pga_jobagent DROP CONSTRAINT pga_jobagent_pkey;
       pgagent         postgres    false    1755    1755             
           2606    235706    pga_jobclass_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_jobclass
    ADD CONSTRAINT pga_jobclass_pkey PRIMARY KEY (jclid);
 I   ALTER TABLE ONLY pgagent.pga_jobclass DROP CONSTRAINT pga_jobclass_pkey;
       pgagent         postgres    false    1756    1756            #
           2606    235708    pga_joblog_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_pkey PRIMARY KEY (jlgid);
 E   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_pkey;
       pgagent         postgres    false    1758    1758            &
           2606    235710    pga_jobstep_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_pkey PRIMARY KEY (jstid);
 G   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_pkey;
       pgagent         postgres    false    1760    1760            )
           2606    235712    pga_jobsteplog_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_pkey PRIMARY KEY (jslid);
 M   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_pkey;
       pgagent         postgres    false    1762    1762            ,
           2606    235714    pga_schedule_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_pkey PRIMARY KEY (jscid);
 I   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_pkey;
       pgagent         postgres    false    1764    1764            q
           2606    245348    FK_base_Department_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_Id";
       public         postgres    false    1804    1804            �
           2606    256188    FPK_base_ResourceAccount_Id 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "FPK_base_ResourceAccount_Id" PRIMARY KEY ("Id", "Resource");
 ^   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "FPK_base_ResourceAccount_Id";
       public         postgres    false    1841    1841    1841            ]
           2606    245266    PF_base_SaleTaxLocation 
   CONSTRAINT     i   ALTER TABLE ONLY "base_SaleTaxLocation"
    ADD CONSTRAINT "PF_base_SaleTaxLocation" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_SaleTaxLocation" DROP CONSTRAINT "PF_base_SaleTaxLocation";
       public         postgres    false    1792    1792            �
           2606    255762    PF_tims_Holiday_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "tims_Holiday"
    ADD CONSTRAINT "PF_tims_Holiday_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."tims_Holiday" DROP CONSTRAINT "PF_tims_Holiday_Id";
       public         postgres    false    1825    1825            u
           2606    245385    PK_GuestAdditional_Id 
   CONSTRAINT     g   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "PK_GuestAdditional_Id" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "PK_GuestAdditional_Id";
       public         postgres    false    1806    1806            7
           2606    244286    PK_UserLogDetail_Id 
   CONSTRAINT     c   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "PK_UserLogDetail_Id" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "PK_UserLogDetail_Id";
       public         postgres    false    1770    1770            c
           2606    245136    PK_UserLog_Id 
   CONSTRAINT     W   ALTER TABLE ONLY "base_UserLog"
    ADD CONSTRAINT "PK_UserLog_Id" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_UserLog" DROP CONSTRAINT "PK_UserLog_Id";
       public         postgres    false    1796    1796            Q
           2606    244954    PK_base_Attachment_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "PK_base_Attachment_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "PK_base_Attachment_Id";
       public         postgres    false    1784    1784            �
           2606    256191    PK_base_Authorize_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Authorize"
    ADD CONSTRAINT "PK_base_Authorize_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Authorize" DROP CONSTRAINT "PK_base_Authorize_Id";
       public         postgres    false    1839    1839            �
           2606    245771    PK_base_CostAdjustmentItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_CostAdjustmentItem"
    ADD CONSTRAINT "PK_base_CostAdjustmentItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_CostAdjustmentItem" DROP CONSTRAINT "PK_base_CostAdjustmentItem_Id";
       public         postgres    false    1816    1816            �
           2606    245763    PK_base_CostAdjustment_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "PK_base_CostAdjustment_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "PK_base_CostAdjustment_Id";
       public         postgres    false    1814    1814            
           2606    271757    PK_base_CounStockDetail_Id 
   CONSTRAINT     m   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "PK_base_CounStockDetail_Id" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "PK_base_CounStockDetail_Id";
       public         postgres    false    1890    1890                       2606    271755    PK_base_CounStock_Id 
   CONSTRAINT     a   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "PK_base_CounStock_Id" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "PK_base_CounStock_Id";
       public         postgres    false    1888    1888            .
           2606    238143    PK_base_EmailAttachment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "PK_base_EmailAttachment" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "PK_base_EmailAttachment";
       public         postgres    false    1766    1766            0
           2606    238253    PK_base_Email_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Email"
    ADD CONSTRAINT "PK_base_Email_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Email" DROP CONSTRAINT "PK_base_Email_Id";
       public         postgres    false    1767    1767            4
           2606    238418    PK_base_GuestFingerPrint_Id 
   CONSTRAINT     n   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "PK_base_GuestFingerPrint_Id" PRIMARY KEY ("Id");
 _   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "PK_base_GuestFingerPrint_Id";
       public         postgres    false    1769    1769            D
           2606    244879    PK_base_GuestHiringHistory_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "PK_base_GuestHiringHistory_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "PK_base_GuestHiringHistory_Id";
       public         postgres    false    1776    1776            I
           2606    244890    PK_base_GuestPayRoll_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "PK_base_GuestPayRoll_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "PK_base_GuestPayRoll_Id";
       public         postgres    false    1778    1778            N
           2606    244941    PK_base_GuestProfile_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "PK_base_GuestProfile_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "PK_base_GuestProfile_Id";
       public         postgres    false    1782    1782            �
           2606    268362    PK_base_GuestReward_Id 
   CONSTRAINT     d   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "PK_base_GuestReward_Id" PRIMARY KEY ("Id");
 U   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "PK_base_GuestReward_Id";
       public         postgres    false    1870    1870            �
           2606    256030    PK_base_GuestSchedule 
   CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "PK_base_GuestSchedule" PRIMARY KEY ("GuestId", "WorkScheduleId", "StartDate");
 V   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "PK_base_GuestSchedule";
       public         postgres    false    1837    1837    1837    1837            A
           2606    244869    PK_base_Guest_Id 
   CONSTRAINT     _   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "PK_base_Guest_Id" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "PK_base_Guest_Id";
       public         postgres    false    1774    1774            U
           2606    245005    PK_base_MemberShip 
   CONSTRAINT     _   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "PK_base_MemberShip" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "PK_base_MemberShip";
       public         postgres    false    1786    1786            �
           2606    268520    PK_base_PricingChange_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "PK_base_PricingChange_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "PK_base_PricingChange_Id";
       public         postgres    false    1872    1872            �
           2606    268194    PK_base_PricingManager_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "PK_base_PricingManager_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "PK_base_PricingManager_Id";
       public         postgres    false    1868    1868            �
           2606    255541    PK_base_ProductStore_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "PK_base_ProductStore_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "PK_base_ProductStore_Id";
       public         postgres    false    1821    1821                       2606    270271    PK_base_ProductUOM_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "PK_base_ProductUOM_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "PK_base_ProductUOM_Id";
       public         postgres    false    1886    1886            w
           2606    255615    PK_base_Product_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "PK_base_Product_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "PK_base_Product_Id";
       public         postgres    false    1808    1808            k
           2606    245160    PK_base_PromotionAffect_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "PK_base_PromotionAffect_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "PK_base_PromotionAffect_Id";
       public         postgres    false    1800    1800            X
           2606    245030    PK_base_PromotionSchedule_Id 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "PK_base_PromotionSchedule_Id" PRIMARY KEY ("Id");
 a   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "PK_base_PromotionSchedule_Id";
       public         postgres    false    1788    1788            n
           2606    245177    PK_base_Promotion_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Promotion"
    ADD CONSTRAINT "PK_base_Promotion_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Promotion" DROP CONSTRAINT "PK_base_Promotion_Id";
       public         postgres    false    1802    1802            �
           2606    266538    PK_base_PurchaseOrderItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "PK_base_PurchaseOrderItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "PK_base_PurchaseOrderItem_Id";
       public         postgres    false    1858    1858            �
           2606    267544    PK_base_PurchaseOrderReceive_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "PK_base_PurchaseOrderReceive_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "PK_base_PurchaseOrderReceive_Id";
       public         postgres    false    1866    1866            �
           2606    266567    PK_base_PurchaseOrder_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "PK_base_PurchaseOrder_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "PK_base_PurchaseOrder_Id";
       public         postgres    false    1860    1860            �
           2606    245750 !   PK_base_QuantityAdjustmentItem_Id 
   CONSTRAINT     z   ALTER TABLE ONLY "base_QuantityAdjustmentItem"
    ADD CONSTRAINT "PK_base_QuantityAdjustmentItem_Id" PRIMARY KEY ("Id");
 k   ALTER TABLE ONLY public."base_QuantityAdjustmentItem" DROP CONSTRAINT "PK_base_QuantityAdjustmentItem_Id";
       public         postgres    false    1812    1812            
           2606    245742    PK_base_QuantityAdjustment_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "PK_base_QuantityAdjustment_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "PK_base_QuantityAdjustment_Id";
       public         postgres    false    1810    1810            �
           2606    246089    PK_base_ResourceNote_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ResourceNote"
    ADD CONSTRAINT "PK_base_ResourceNote_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ResourceNote" DROP CONSTRAINT "PK_base_ResourceNote_Id";
       public         postgres    false    1818    1818            �
           2606    270163     PK_base_ResourcePaymentDetail_Id 
   CONSTRAINT     x   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "PK_base_ResourcePaymentDetail_Id" PRIMARY KEY ("Id");
 i   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "PK_base_ResourcePaymentDetail_Id";
       public         postgres    false    1880    1880                       2606    272130     PK_base_ResourcePaymentProductId 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "PK_base_ResourcePaymentProductId" PRIMARY KEY ("Id");
 j   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "PK_base_ResourcePaymentProductId";
       public         postgres    false    1895    1895            �
           2606    270161    PK_base_ResourcePayment_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_ResourcePayment"
    ADD CONSTRAINT "PK_base_ResourcePayment_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_ResourcePayment" DROP CONSTRAINT "PK_base_ResourcePayment_Id";
       public         postgres    false    1882    1882            K
           2606    270190    PK_base_ResourcePhoto_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_ResourcePhoto"
    ADD CONSTRAINT "PK_base_ResourcePhoto_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_ResourcePhoto" DROP CONSTRAINT "PK_base_ResourcePhoto_Id";
       public         postgres    false    1780    1780                       2606    272108    PK_base_ResourceReturnDetail_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "PK_base_ResourceReturnDetail_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "PK_base_ResourceReturnDetail_Id";
       public         postgres    false    1893    1893            �
           2606    270203    PK_base_ResourceReturn_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "PK_base_ResourceReturn_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "PK_base_ResourceReturn_Id";
       public         postgres    false    1884    1884            �
           2606    266851    PK_base_RewardManager 
   CONSTRAINT     e   ALTER TABLE ONLY "base_RewardManager"
    ADD CONSTRAINT "PK_base_RewardManager" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_RewardManager" DROP CONSTRAINT "PK_base_RewardManager";
       public         postgres    false    1864    1864            �
           2606    266611    PK_base_SaleCommission 
   CONSTRAINT     g   ALTER TABLE ONLY "base_SaleCommission"
    ADD CONSTRAINT "PK_base_SaleCommission" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_SaleCommission" DROP CONSTRAINT "PK_base_SaleCommission";
       public         postgres    false    1862    1862            �
           2606    266090    PK_base_SaleOrderDetail_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "PK_base_SaleOrderDetail_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "PK_base_SaleOrderDetail_Id";
       public         postgres    false    1848    1848            �
           2606    266249    PK_base_SaleOrderInvoice 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "PK_base_SaleOrderInvoice" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "PK_base_SaleOrderInvoice";
       public         postgres    false    1854    1854            �
           2606    266362    PK_base_SaleOrderShipDetail 
   CONSTRAINT     q   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "PK_base_SaleOrderShipDetail" PRIMARY KEY ("Id");
 b   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "PK_base_SaleOrderShipDetail";
       public         postgres    false    1856    1856            �
           2606    266219    PK_base_SaleOrderShip_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "PK_base_SaleOrderShip_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "PK_base_SaleOrderShip_Id";
       public         postgres    false    1852    1852            �
           2606    266117    PK_base_SaleOrder_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_SaleOrder"
    ADD CONSTRAINT "PK_base_SaleOrder_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_SaleOrder" DROP CONSTRAINT "PK_base_SaleOrder_Id";
       public         postgres    false    1850    1850            [
           2606    245268    PK_base_SaleTaxLocationOption 
   CONSTRAINT     u   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "PK_base_SaleTaxLocationOption" PRIMARY KEY ("Id");
 f   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "PK_base_SaleTaxLocationOption";
       public         postgres    false    1790    1790            �
           2606    255680    PK_base_Store_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "PK_base_Store_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "PK_base_Store_Id";
       public         postgres    false    1823    1823            �
           2606    269949    PK_base_TransferStockDetail_Id 
   CONSTRAINT     t   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "PK_base_TransferStockDetail_Id" PRIMARY KEY ("Id");
 e   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "PK_base_TransferStockDetail_Id";
       public         postgres    false    1878    1878            �
           2606    269936    PK_base_TransferStock_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "PK_base_TransferStock_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "PK_base_TransferStock_Id";
       public         postgres    false    1876    1876            e
           2606    245152    PK_base_UOM_Id 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "PK_base_UOM_Id" PRIMARY KEY ("Id");
 E   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "PK_base_UOM_Id";
       public         postgres    false    1798    1798            �
           2606    256249    PK_base_UserRight_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "PK_base_UserRight_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "PK_base_UserRight_Id";
       public         postgres    false    1843    1843            �
           2606    269660    PK_base_VendorProduct_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "PK_base_VendorProduct_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "PK_base_VendorProduct_Id";
       public         postgres    false    1873    1873            a
           2606    245122    PK_base_VirtualFolder 
   CONSTRAINT     e   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "PK_base_VirtualFolder" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "PK_base_VirtualFolder";
       public         postgres    false    1794    1794            �
           2606    255709    PK_tims_HolidayHistory_Date 
   CONSTRAINT     n   ALTER TABLE ONLY "tims_HolidayHistory"
    ADD CONSTRAINT "PK_tims_HolidayHistory_Date" PRIMARY KEY ("Date");
 ]   ALTER TABLE ONLY public."tims_HolidayHistory" DROP CONSTRAINT "PK_tims_HolidayHistory_Date";
       public         postgres    false    1826    1826            �
           2606    255743    PK_tims_WorkSchedule_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "tims_WorkSchedule"
    ADD CONSTRAINT "PK_tims_WorkSchedule_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."tims_WorkSchedule" DROP CONSTRAINT "PK_tims_WorkSchedule_Id";
       public         postgres    false    1828    1828            �
           2606    255786    PK_tims_WorkWeek_Id 
   CONSTRAINT     ^   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "PK_tims_WorkWeek_Id" PRIMARY KEY ("Id");
 O   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "PK_tims_WorkWeek_Id";
       public         postgres    false    1830    1830            �
           2606    257312    base_Configuration_pkey 
   CONSTRAINT     g   ALTER TABLE ONLY "base_Configuration"
    ADD CONSTRAINT "base_Configuration_pkey" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_Configuration" DROP CONSTRAINT "base_Configuration_pkey";
       public         postgres    false    1819    1819            �
           2606    257332    base_GuestPaymentCard_Id 
   CONSTRAINT     k   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "base_GuestPaymentCard_Id" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "base_GuestPaymentCard_Id";
       public         postgres    false    1846    1846            ;
           2606    244846    base_Guest_pkey 
   CONSTRAINT     W   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "base_Guest_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "base_Guest_pkey";
       public         postgres    false    1772    1772            �
           2606    255870    key 
   CONSTRAINT     p   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT key PRIMARY KEY ("TimeLogId", "WorkPermissionId");
 F   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT key;
       public         postgres    false    1836    1836    1836            �
           2606    255857    pk_tims_timelog 
   CONSTRAINT     W   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT pk_tims_timelog PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT pk_tims_timelog;
       public         postgres    false    1834    1834            �
           2606    255803    pk_tims_workpermission 
   CONSTRAINT     e   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT pk_tims_workpermission PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT pk_tims_workpermission;
       public         postgres    false    1832    1832            �
           2606    245773    uni_baseQuantityAdjustment 
   CONSTRAINT     p   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "uni_baseQuantityAdjustment" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "uni_baseQuantityAdjustment";
       public         postgres    false    1810    1810            �
           2606    245775    uni_baseQuantityAdjustmentItem 
   CONSTRAINT     x   ALTER TABLE ONLY "base_QuantityAdjustmentItem"
    ADD CONSTRAINT "uni_baseQuantityAdjustmentItem" UNIQUE ("Resource");
 h   ALTER TABLE ONLY public."base_QuantityAdjustmentItem" DROP CONSTRAINT "uni_baseQuantityAdjustmentItem";
       public         postgres    false    1812    1812            �
           2606    245783    uni_base_CostAdjustment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "uni_base_CostAdjustment" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "uni_base_CostAdjustment";
       public         postgres    false    1814    1814            �
           2606    245785    uni_base_CostAdjustmentItem 
   CONSTRAINT     q   ALTER TABLE ONLY "base_CostAdjustmentItem"
    ADD CONSTRAINT "uni_base_CostAdjustmentItem" UNIQUE ("Resource");
 a   ALTER TABLE ONLY public."base_CostAdjustmentItem" DROP CONSTRAINT "uni_base_CostAdjustmentItem";
       public         postgres    false    1816    1816                       2606    271770    uni_base_CountStock_Resource 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "uni_base_CountStock_Resource" UNIQUE ("Resource");
 Z   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "uni_base_CountStock_Resource";
       public         postgres    false    1888    1888            ?
           2606    256327    uni_base_Guest_Resource 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "uni_base_Guest_Resource" UNIQUE ("Resource");
 P   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "uni_base_Guest_Resource";
       public         postgres    false    1772    1772            �
           2606    268201    uni_base_PricingManager 
   CONSTRAINT     i   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "uni_base_PricingManager" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "uni_base_PricingManager";
       public         postgres    false    1868    1868            }
           2606    269972    uni_base_Product_Resource 
   CONSTRAINT     d   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "uni_base_Product_Resource" UNIQUE ("Resource");
 T   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "uni_base_Product_Resource";
       public         postgres    false    1808    1808            �
           2606    266569    uni_base_PurchaseOrder_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "uni_base_PurchaseOrder_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "uni_base_PurchaseOrder_Resource";
       public         postgres    false    1860    1860            �
           2606    256317 !   uni_base_ResourceAccount_Resource 
   CONSTRAINT     t   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "uni_base_ResourceAccount_Resource" UNIQUE ("Resource");
 d   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "uni_base_ResourceAccount_Resource";
       public         postgres    false    1841    1841            �
           2606    270205     uni_base_ResourceReturn_Resource 
   CONSTRAINT     r   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "uni_base_ResourceReturn_Resource" UNIQUE ("Resource");
 b   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "uni_base_ResourceReturn_Resource";
       public         postgres    false    1884    1884            �
           2606    266303    uni_base_SaleOrderDetail 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "uni_base_SaleOrderDetail" UNIQUE ("Resource");
 [   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "uni_base_SaleOrderDetail";
       public         postgres    false    1848    1848            �
           2606    266221    uni_base_SaleOrderShip_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "uni_base_SaleOrderShip_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "uni_base_SaleOrderShip_Resource";
       public         postgres    false    1852    1852            �
           2606    255948    uni_base_Store_Code 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "uni_base_Store_Code" UNIQUE ("Code");
 L   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "uni_base_Store_Code";
       public         postgres    false    1823    1823            �
           2606    269938    uni_base_TransferStock_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "uni_base_TransferStock_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "uni_base_TransferStock_Resource";
       public         postgres    false    1876    1876            h
           2606    254600    uni_base_UOM_Code 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "uni_base_UOM_Code" UNIQUE ("Code");
 H   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "uni_base_UOM_Code";
       public         postgres    false    1798    1798            �
           2606    256251    uni_base_UserRight_Code 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "uni_base_UserRight_Code" UNIQUE ("Code");
 T   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "uni_base_UserRight_Code";
       public         postgres    false    1843    1843            �
           2606    269675 5   uni_base_VendorProduct_VendorResource_ProductResource 
   CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource" UNIQUE ("ProductResource", "VendorResource");
 v   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource";
       public         postgres    false    1873    1873    1873            
           1259    235939    pga_exception_datetime    INDEX     \   CREATE UNIQUE INDEX pga_exception_datetime ON pga_exception USING btree (jexdate, jextime);
 +   DROP INDEX pgagent.pga_exception_datetime;
       pgagent         postgres    false    1751    1751            
           1259    235940    pga_exception_jexscid    INDEX     K   CREATE INDEX pga_exception_jexscid ON pga_exception USING btree (jexscid);
 *   DROP INDEX pgagent.pga_exception_jexscid;
       pgagent         postgres    false    1751            
           1259    235941    pga_jobclass_name    INDEX     M   CREATE UNIQUE INDEX pga_jobclass_name ON pga_jobclass USING btree (jclname);
 &   DROP INDEX pgagent.pga_jobclass_name;
       pgagent         postgres    false    1756            !
           1259    235942    pga_joblog_jobid    INDEX     D   CREATE INDEX pga_joblog_jobid ON pga_joblog USING btree (jlgjobid);
 %   DROP INDEX pgagent.pga_joblog_jobid;
       pgagent         postgres    false    1758            *
           1259    235943    pga_jobschedule_jobid    INDEX     K   CREATE INDEX pga_jobschedule_jobid ON pga_schedule USING btree (jscjobid);
 *   DROP INDEX pgagent.pga_jobschedule_jobid;
       pgagent         postgres    false    1764            $
           1259    235944    pga_jobstep_jobid    INDEX     F   CREATE INDEX pga_jobstep_jobid ON pga_jobstep USING btree (jstjobid);
 &   DROP INDEX pgagent.pga_jobstep_jobid;
       pgagent         postgres    false    1760            '
           1259    235945    pga_jobsteplog_jslid    INDEX     L   CREATE INDEX pga_jobsteplog_jslid ON pga_jobsteplog USING btree (jsljlgid);
 )   DROP INDEX pgagent.pga_jobsteplog_jslid;
       pgagent         postgres    false    1762            �
           1259    255547 .   FKI_baseProductStore_ProductId_base_Product_Id    INDEX     p   CREATE INDEX "FKI_baseProductStore_ProductId_base_Product_Id" ON "base_ProductStore" USING btree ("ProductId");
 D   DROP INDEX public."FKI_baseProductStore_ProductId_base_Product_Id";
       public         postgres    false    1821            i
           1259    245166 5   FKI_basePromotionAffect_PromotionId_base_Promotion_Id    INDEX     |   CREATE INDEX "FKI_basePromotionAffect_PromotionId_base_Promotion_Id" ON "base_PromotionAffect" USING btree ("PromotionId");
 K   DROP INDEX public."FKI_basePromotionAffect_PromotionId_base_Promotion_Id";
       public         postgres    false    1800            O
           1259    246209 9   FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    INDEX        CREATE INDEX "FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" ON "base_Attachment" USING btree ("VirtualFolderId");
 O   DROP INDEX public."FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public         postgres    false    1784                       1259    271763 8   FKI_base_CounStockDetail_CountStockId_base_CountStock_id    INDEX     �   CREATE INDEX "FKI_base_CounStockDetail_CountStockId_base_CountStock_id" ON "base_CountStockDetail" USING btree ("CountStockId");
 N   DROP INDEX public."FKI_base_CounStockDetail_CountStockId_base_CountStock_id";
       public         postgres    false    1890            o
           1259    245354    FKI_base_Department_Id_ParentId    INDEX     ^   CREATE INDEX "FKI_base_Department_Id_ParentId" ON "base_Department" USING btree ("ParentId");
 5   DROP INDEX public."FKI_base_Department_Id_ParentId";
       public         postgres    false    1804            s
           1259    245391 &   FKI_base_GuestAdditional_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestAdditional_base_Guest_Id" ON "base_GuestAdditional" USING btree ("GuestId");
 <   DROP INDEX public."FKI_base_GuestAdditional_base_Guest_Id";
       public         postgres    false    1806            G
           1259    244891 +   FKI_base_GuestPayRoll_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestPayRoll_GuestId_base_Guest_Id" ON "base_GuestPayRoll" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestPayRoll_GuestId_base_Guest_Id";
       public         postgres    false    1778            L
           1259    244942 +   FKI_base_GuestProfile_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestProfile_GuestId_base_Guest_Id" ON "base_GuestProfile" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestProfile_GuestId_base_Guest_Id";
       public         postgres    false    1782            �
           1259    268373 *   FKI_base_GuestReward_GuestId_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestReward_GuestId_base_Guest_Id" ON "base_GuestReward" USING btree ("GuestId");
 @   DROP INDEX public."FKI_base_GuestReward_GuestId_base_Guest_Id";
       public         postgres    false    1870            9
           1259    245510 %   FKI_base_Guest_ParentId_base_Guest_Id    INDEX     _   CREATE INDEX "FKI_base_Guest_ParentId_base_Guest_Id" ON "base_Guest" USING btree ("ParentId");
 ;   DROP INDEX public."FKI_base_Guest_ParentId_base_Guest_Id";
       public         postgres    false    1772            S
           1259    245006 )   FKI_base_MemberShip_GuestId_base_Guest_Id    INDEX     g   CREATE INDEX "FKI_base_MemberShip_GuestId_base_Guest_Id" ON "base_MemberShip" USING btree ("GuestId");
 ?   DROP INDEX public."FKI_base_MemberShip_GuestId_base_Guest_Id";
       public         postgres    false    1786            �
           1259    268532 >   FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id    INDEX     �   CREATE INDEX "FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id" ON "base_PricingChange" USING btree ("PricingManagerId");
 T   DROP INDEX public."FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public         postgres    false    1872                        1259    270282 .   FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id    INDEX     s   CREATE INDEX "FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id" ON "base_ProductUOM" USING btree ("BaseUnitNumber");
 D   DROP INDEX public."FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id";
       public         postgres    false    1886                       1259    270283 -   FKI_base_ProductUOM_ProductId_base_Product_Id    INDEX     m   CREATE INDEX "FKI_base_ProductUOM_ProductId_base_Product_Id" ON "base_ProductUOM" USING btree ("ProductId");
 C   DROP INDEX public."FKI_base_ProductUOM_ProductId_base_Product_Id";
       public         postgres    false    1886            V
           1259    245041 8   FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id    INDEX     �   CREATE INDEX "FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id" ON "base_PromotionSchedule" USING btree ("PromotionId");
 N   DROP INDEX public."FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public         postgres    false    1788            l
           1259    245178 8   FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id    INDEX     }   CREATE INDEX "FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id" ON "base_Promotion" USING btree ("PromotionTypeId");
 N   DROP INDEX public."FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id";
       public         postgres    false    1802            �
           1259    266544 ?   FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder" ON "base_PurchaseOrderDetail" USING btree ("PurchaseOrderId");
 U   DROP INDEX public."FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder";
       public         postgres    false    1858            �
           1259    267550 ?   FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha" ON "base_PurchaseOrderReceive" USING btree ("PurchaseOrderDetailId");
 U   DROP INDEX public."FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha";
       public         postgres    false    1866                       1259    272136 ?   FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource    INDEX     �   CREATE INDEX "FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource" ON "base_ResourcePaymentProduct" USING btree ("ResourcePaymentId");
 U   DROP INDEX public."FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource";
       public         postgres    false    1895            �
           1259    266128 6   FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    INDEX     }   CREATE INDEX "FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderDetail" USING btree ("SaleOrderId");
 L   DROP INDEX public."FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1848            �
           1259    266265 7   FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    INDEX        CREATE INDEX "FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderInvoice" USING btree ("SaleOrderId");
 M   DROP INDEX public."FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1854            �
           1259    266368 ?   FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip    INDEX     �   CREATE INDEX "FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip" ON "base_SaleOrderShipDetail" USING btree ("SaleOrderShipId");
 U   DROP INDEX public."FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip";
       public         postgres    false    1856            �
           1259    266227 4   FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    INDEX     y   CREATE INDEX "FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderShip" USING btree ("SaleOrderId");
 J   DROP INDEX public."FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1852            Y
           1259    245099 1   FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id    INDEX     �   CREATE INDEX "FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id" ON "base_SaleTaxLocationOption" USING btree ("SaleTaxLocationId");
 G   DROP INDEX public."FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id";
       public         postgres    false    1790            �
           1259    269955 ?   FKI_base_TransferStockDetail_TransferStockId_base_TransferStock    INDEX     �   CREATE INDEX "FKI_base_TransferStockDetail_TransferStockId_base_TransferStock" ON "base_TransferStockDetail" USING btree ("TransferStockId");
 U   DROP INDEX public."FKI_base_TransferStockDetail_TransferStockId_base_TransferStock";
       public         postgres    false    1878            �
           1259    269666 .   FKI_base_VendorProduct_ProductId_base_Guest_Id    INDEX     q   CREATE INDEX "FKI_base_VendorProduct_ProductId_base_Guest_Id" ON "base_VendorProduct" USING btree ("ProductId");
 D   DROP INDEX public."FKI_base_VendorProduct_ProductId_base_Guest_Id";
       public         postgres    false    1873            �
           1259    256148 0   FKI_tims_WorkPermission_EmployeeId_base_Guest_Id    INDEX     u   CREATE INDEX "FKI_tims_WorkPermission_EmployeeId_base_Guest_Id" ON "tims_WorkPermission" USING btree ("EmployeeId");
 F   DROP INDEX public."FKI_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public         postgres    false    1832            5
           1259    244035    idx_GuestFingerPrint_GuestId    INDEX     `   CREATE INDEX "idx_GuestFingerPrint_GuestId" ON "base_GuestFingerPrint" USING btree ("GuestId");
 2   DROP INDEX public."idx_GuestFingerPrint_GuestId";
       public         postgres    false    1769            <
           1259    244839    idx_GuestName    INDEX     _   CREATE INDEX "idx_GuestName" ON "base_Guest" USING btree ("FirstName", "LastName", "Company");
 #   DROP INDEX public."idx_GuestName";
       public         postgres    false    1772    1772    1772            8
           1259    244292    idx_UserLogDetail    INDEX     T   CREATE INDEX "idx_UserLogDetail" ON "base_UserLogDetail" USING btree ("UserLogId");
 '   DROP INDEX public."idx_UserLogDetail";
       public         postgres    false    1770            R
           1259    255513    idx_base_Attachment    INDEX     S   CREATE UNIQUE INDEX "idx_base_Attachment" ON "base_Attachment" USING btree ("Id");
 )   DROP INDEX public."idx_base_Attachment";
       public         postgres    false    1784            �
           1259    256319    idx_base_Authorize_Code    INDEX     Q   CREATE INDEX "idx_base_Authorize_Code" ON "base_Authorize" USING btree ("Code");
 -   DROP INDEX public."idx_base_Authorize_Code";
       public         postgres    false    1839            �
           1259    256318    idx_base_Authorize_Resource    INDEX     Y   CREATE INDEX "idx_base_Authorize_Resource" ON "base_Authorize" USING btree ("Resource");
 1   DROP INDEX public."idx_base_Authorize_Resource";
       public         postgres    false    1839            �
           1259    245792    idx_base_CostAdjustment    INDEX     Z   CREATE INDEX "idx_base_CostAdjustment" ON "base_CostAdjustment" USING btree ("Resource");
 -   DROP INDEX public."idx_base_CostAdjustment";
       public         postgres    false    1814            r
           1259    255517    idx_base_Department_Id    INDEX     W   CREATE INDEX "idx_base_Department_Id" ON "base_Department" USING btree ("Id", "Name");
 ,   DROP INDEX public."idx_base_Department_Id";
       public         postgres    false    1804    1804            1
           1259    238254    idx_base_Email    INDEX     B   CREATE INDEX "idx_base_Email" ON "base_Email" USING btree ("Id");
 $   DROP INDEX public."idx_base_Email";
       public         postgres    false    1767            2
           1259    238260    idx_base_Email_Address    INDEX     N   CREATE INDEX "idx_base_Email_Address" ON "base_Email" USING btree ("Sender");
 ,   DROP INDEX public."idx_base_Email_Address";
       public         postgres    false    1767            B
           1259    244870    idx_base_GuestAddress_Id    INDEX     S   CREATE INDEX "idx_base_GuestAddress_Id" ON "base_GuestAddress" USING btree ("Id");
 .   DROP INDEX public."idx_base_GuestAddress_Id";
       public         postgres    false    1774            E
           1259    244880     idx_base_GuestHiringHistory_Date    INDEX     �   CREATE INDEX "idx_base_GuestHiringHistory_Date" ON "base_GuestHiringHistory" USING btree ("StartDate", "RenewDate", "PromotionDate");
 6   DROP INDEX public."idx_base_GuestHiringHistory_Date";
       public         postgres    false    1776    1776    1776            F
           1259    244881    idx_base_GuestHiringHistory_Id    INDEX     d   CREATE INDEX "idx_base_GuestHiringHistory_Id" ON "base_GuestHiringHistory" USING btree ("GuestId");
 4   DROP INDEX public."idx_base_GuestHiringHistory_Id";
       public         postgres    false    1776            �
           1259    257338 !   idx_base_GuestPaymentCard_GuestId    INDEX     e   CREATE INDEX "idx_base_GuestPaymentCard_GuestId" ON "base_GuestPaymentCard" USING btree ("GuestId");
 7   DROP INDEX public."idx_base_GuestPaymentCard_GuestId";
       public         postgres    false    1846            =
           1259    256328    idx_base_Guest_Resource    INDEX     Q   CREATE INDEX "idx_base_Guest_Resource" ON "base_Guest" USING btree ("Resource");
 -   DROP INDEX public."idx_base_Guest_Resource";
       public         postgres    false    1772            x
           1259    257571    idx_base_Product_Code    INDEX     M   CREATE INDEX "idx_base_Product_Code" ON "base_Product" USING btree ("Code");
 +   DROP INDEX public."idx_base_Product_Code";
       public         postgres    false    1808            y
           1259    245794    idx_base_Product_Id    INDEX     I   CREATE INDEX "idx_base_Product_Id" ON "base_Product" USING btree ("Id");
 )   DROP INDEX public."idx_base_Product_Id";
       public         postgres    false    1808            z
           1259    254639    idx_base_Product_Name    INDEX     c   CREATE INDEX "idx_base_Product_Name" ON "base_Product" USING btree ("ProductName", "Description");
 +   DROP INDEX public."idx_base_Product_Name";
       public         postgres    false    1808    1808            {
           1259    271771    idx_base_Product_Resource    INDEX     U   CREATE INDEX "idx_base_Product_Resource" ON "base_Product" USING btree ("Resource");
 /   DROP INDEX public."idx_base_Product_Resource";
       public         postgres    false    1808            �
           1259    245793    idx_base_QuantityAdjustment    INDEX     b   CREATE INDEX "idx_base_QuantityAdjustment" ON "base_QuantityAdjustment" USING btree ("Resource");
 1   DROP INDEX public."idx_base_QuantityAdjustment";
       public         postgres    false    1810            �
           1259    256315 !   idx_base_ResourceAccount_Resource    INDEX     u   CREATE INDEX "idx_base_ResourceAccount_Resource" ON "base_ResourceAccount" USING btree ("Resource", "UserResource");
 7   DROP INDEX public."idx_base_ResourceAccount_Resource";
       public         postgres    false    1841    1841            �
           1259    270298 ,   idx_base_ResourcePayment_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourcePayment_DocumentResource_No" ON "base_ResourcePayment" USING btree ("DocumentNo", "DocumentResource");
 B   DROP INDEX public."idx_base_ResourcePayment_DocumentResource_No";
       public         postgres    false    1882    1882            �
           1259    270208    idx_base_ResourcePayment_Id    INDEX     Y   CREATE INDEX "idx_base_ResourcePayment_Id" ON "base_ResourcePayment" USING btree ("Id");
 1   DROP INDEX public."idx_base_ResourcePayment_Id";
       public         postgres    false    1882            �
           1259    271706 +   idx_base_ResourceReturn_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourceReturn_DocumentResource_No" ON "base_ResourceReturn" USING btree ("DocumentNo", "DocumentResource");
 A   DROP INDEX public."idx_base_ResourceReturn_DocumentResource_No";
       public         postgres    false    1884    1884            �
           1259    266266    idx_base_SaleOrder_Resource    INDEX     Y   CREATE INDEX "idx_base_SaleOrder_Resource" ON "base_SaleOrder" USING btree ("Resource");
 1   DROP INDEX public."idx_base_SaleOrder_Resource";
       public         postgres    false    1850            ^
           1259    245314    idx_base_SaleTaxLocation_Id    INDEX     Y   CREATE INDEX "idx_base_SaleTaxLocation_Id" ON "base_SaleTaxLocation" USING btree ("Id");
 1   DROP INDEX public."idx_base_SaleTaxLocation_Id";
       public         postgres    false    1792            _
           1259    245313     idx_base_SaleTaxLocation_TaxCode    INDEX     c   CREATE INDEX "idx_base_SaleTaxLocation_TaxCode" ON "base_SaleTaxLocation" USING btree ("TaxCode");
 6   DROP INDEX public."idx_base_SaleTaxLocation_TaxCode";
       public         postgres    false    1792            f
           1259    245807    idx_base_UOM_Id    INDEX     A   CREATE INDEX "idx_base_UOM_Id" ON "base_UOM" USING btree ("Id");
 %   DROP INDEX public."idx_base_UOM_Id";
       public         postgres    false    1798            �
           1259    256314    idx_base_UserRight_Code    INDEX     Q   CREATE INDEX "idx_base_UserRight_Code" ON "base_UserRight" USING btree ("Code");
 -   DROP INDEX public."idx_base_UserRight_Code";
       public         postgres    false    1843            �
           1259    255787    idx_tims_WorkWeek_ScheduleId    INDEX     _   CREATE INDEX "idx_tims_WorkWeek_ScheduleId" ON "tims_WorkWeek" USING btree ("WorkScheduleId");
 2   DROP INDEX public."idx_tims_WorkWeek_ScheduleId";
       public         postgres    false    1830            F           2620    235953    pga_exception_trigger    TRIGGER     �   CREATE TRIGGER pga_exception_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_exception FOR EACH ROW EXECUTE PROCEDURE pga_exception_trigger();
 =   DROP TRIGGER pga_exception_trigger ON pgagent.pga_exception;
       pgagent       postgres    false    19    1751            �           0    0 .   TRIGGER pga_exception_trigger ON pga_exception    COMMENT     ~   COMMENT ON TRIGGER pga_exception_trigger ON pga_exception IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    2886            G           2620    235954    pga_job_trigger    TRIGGER     j   CREATE TRIGGER pga_job_trigger BEFORE UPDATE ON pga_job FOR EACH ROW EXECUTE PROCEDURE pga_job_trigger();
 1   DROP TRIGGER pga_job_trigger ON pgagent.pga_job;
       pgagent       postgres    false    21    1753            �           0    0 "   TRIGGER pga_job_trigger ON pga_job    COMMENT     U   COMMENT ON TRIGGER pga_job_trigger ON pga_job IS 'Update the job''s next run time.';
            pgagent       postgres    false    2887            H           2620    235955    pga_schedule_trigger    TRIGGER     �   CREATE TRIGGER pga_schedule_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_schedule FOR EACH ROW EXECUTE PROCEDURE pga_schedule_trigger();
 ;   DROP TRIGGER pga_schedule_trigger ON pgagent.pga_schedule;
       pgagent       postgres    false    1764    23            �           0    0 ,   TRIGGER pga_schedule_trigger ON pga_schedule    COMMENT     z   COMMENT ON TRIGGER pga_schedule_trigger ON pga_schedule IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    2888                       2606    235956    pga_exception_jexscid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_jexscid_fkey FOREIGN KEY (jexscid) REFERENCES pga_schedule(jscid) ON UPDATE RESTRICT ON DELETE CASCADE;
 S   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_jexscid_fkey;
       pgagent       postgres    false    1764    2603    1751                       2606    235961    pga_job_jobagentid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobagentid_fkey FOREIGN KEY (jobagentid) REFERENCES pga_jobagent(jagpid) ON UPDATE RESTRICT ON DELETE SET NULL;
 J   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobagentid_fkey;
       pgagent       postgres    false    1755    2588    1753                       2606    235966    pga_job_jobjclid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobjclid_fkey FOREIGN KEY (jobjclid) REFERENCES pga_jobclass(jclid) ON UPDATE RESTRICT ON DELETE RESTRICT;
 H   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobjclid_fkey;
       pgagent       postgres    false    1753    2591    1756                       2606    235971    pga_joblog_jlgjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_jlgjobid_fkey FOREIGN KEY (jlgjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 N   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_jlgjobid_fkey;
       pgagent       postgres    false    1753    1758    2586                       2606    235976    pga_jobstep_jstjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_jstjobid_fkey FOREIGN KEY (jstjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 P   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_jstjobid_fkey;
       pgagent       postgres    false    2586    1753    1760                       2606    235981    pga_jobsteplog_jsljlgid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljlgid_fkey FOREIGN KEY (jsljlgid) REFERENCES pga_joblog(jlgid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljlgid_fkey;
       pgagent       postgres    false    1758    2594    1762                       2606    235986    pga_jobsteplog_jsljstid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljstid_fkey FOREIGN KEY (jsljstid) REFERENCES pga_jobstep(jstid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljstid_fkey;
       pgagent       postgres    false    1762    1760    2597                       2606    235991    pga_schedule_jscjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_jscjobid_fkey FOREIGN KEY (jscjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 R   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_jscjobid_fkey;
       pgagent       postgres    false    2586    1753    1764            (           2606    255621 -   FK_baseProductStore_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id";
       public       postgres    false    2678    1821    1808                        2606    246204 8   FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" FOREIGN KEY ("VirtualFolderId") REFERENCES "base_VirtualFolder"("Id");
 v   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public       postgres    false    1794    1784    2656            C           2606    271772 7   FK_base_CounStockDetail_CountStockId_base_CountStock_id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id" FOREIGN KEY ("CountStockId") REFERENCES "base_CountStock"("Id");
 {   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id";
       public       postgres    false    2820    1888    1890            &           2606    245349 .   FK_base_Department_ParentId_base_Department_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_ParentId_base_Department_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Department"("Id");
 l   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_ParentId_base_Department_Id";
       public       postgres    false    2672    1804    1804                       2606    238255 -   FK_base_EmailAttachment_EmailId_base_Email_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id" FOREIGN KEY ("EmailId") REFERENCES "base_Email"("Id");
 p   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id";
       public       postgres    false    2607    1766    1767            '           2606    256202 %   FK_base_GuestAdditional_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id";
       public       postgres    false    2618    1772    1806                       2606    256207 "   FK_base_GuestAddress_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "FK_base_GuestAddress_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "FK_base_GuestAddress_base_Guest_Id";
       public       postgres    false    1774    1772    2618                       2606    256212 .   FK_base_GuestFingerPrint_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id";
       public       postgres    false    1772    1769    2618                       2606    256217 0   FK_base_GuestHiringHistory_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id";
       public       postgres    false    1776    1772    2618                       2606    256222 *   FK_base_GuestPayRoll_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id";
       public       postgres    false    1772    1778    2618            1           2606    257333 .   FK_base_GuestPaymentCard_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id";
       public       postgres    false    1772    1846    2618                       2606    256197 *   FK_base_GuestProfile_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id";
       public       postgres    false    2618    1782    1772            8           2606    268363 )   FK_base_GuestReward_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id";
       public       postgres    false    2618    1772    1870            9           2606    268368 2   FK_base_GuestReward_RewardId_base_RewardManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id" FOREIGN KEY ("RewardId") REFERENCES "base_RewardManager"("Id");
 q   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id";
       public       postgres    false    1870    1864    2778            0           2606    256031 +   FK_base_GuestSchedule_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id";
       public       postgres    false    1837    1772    2618            /           2606    256023 9   FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2719    1828    1837                       2606    245511 $   FK_base_Guest_ParentId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Guest"("Id");
 ]   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id";
       public       postgres    false    1772    1772    2618            !           2606    245230 (   FK_base_MemberShip_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id");
 f   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id";
       public       postgres    false    2618    1786    1772            :           2606    268533 =   FK_base_PricingChange_PricingManagerId_base_PricingManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id" FOREIGN KEY ("PricingManagerId") REFERENCES "base_PricingManager"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public       postgres    false    1872    1868    2783            ;           2606    268526 /   FK_base_PricingChange_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id";
       public       postgres    false    2678    1808    1872            B           2606    270272 ,   FK_base_ProductUOM_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 j   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductId_base_Product_Id";
       public       postgres    false    1886    2678    1808            @           2606    270285 6   FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id" FOREIGN KEY ("ProductStoreId") REFERENCES "base_ProductStore"("Id");
 t   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id";
       public       postgres    false    2709    1821    1886            A           2606    270277 $   FK_base_ProductUOM_UOMId_base_UOM_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id" FOREIGN KEY ("UOMId") REFERENCES "base_UOM"("Id");
 b   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id";
       public       postgres    false    1886    2660    1798            %           2606    245248 5   FK_base_PromotionAffect_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 x   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id";
       public       postgres    false    1802    1800    2669            "           2606    245253 7   FK_base_PromotionSchedule_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 |   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public       postgres    false    1788    1802    2669            6           2606    266570 ?   FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_" FOREIGN KEY ("PurchaseOrderId") REFERENCES "base_PurchaseOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_";
       public       postgres    false    2772    1860    1858            7           2606    267545 ?   FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas" FOREIGN KEY ("PurchaseOrderDetailId") REFERENCES "base_PurchaseOrderDetail"("Id");
 �   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas";
       public       postgres    false    2770    1858    1866            ?           2606    270170 ?   FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id");
 �   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa";
       public       postgres    false    1882    2807    1880            E           2606    272137 ?   FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP";
       public       postgres    false    1895    1882    2807            D           2606    272109 ?   FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu" FOREIGN KEY ("ResourceReturnId") REFERENCES "base_ResourceReturn"("Id");
 �   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu";
       public       postgres    false    1884    2811    1893            2           2606    266129 5   FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2755    1850    1848            4           2606    266260 6   FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2755    1850    1854            5           2606    266363 ?   FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_" FOREIGN KEY ("SaleOrderShipId") REFERENCES "base_SaleOrderShip"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_";
       public       postgres    false    1856    2759    1852            3           2606    266222 3   FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1850    2755    1852            >           2606    270034 ?   FK_base_TransferStockDetail_TransferStockId_base_TransferStock_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_" FOREIGN KEY ("TransferStockId") REFERENCES "base_TransferStock"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_";
       public       postgres    false    1878    2798    1876                       2606    266390 /   FK_base_UserLogDetail_UserLogId_base_UserLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id" FOREIGN KEY ("UserLogId") REFERENCES "base_UserLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id";
       public       postgres    false    2658    1770    1796            =           2606    270029 /   FK_base_VendorProduct_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 p   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id";
       public       postgres    false    2678    1873    1808            <           2606    269667 ,   FK_base_VendorProduct_VendorId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id" FOREIGN KEY ("VendorId") REFERENCES "base_Guest"("Id");
 m   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id";
       public       postgres    false    1873    1772    2618            $           2606    245123 9   FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId" FOREIGN KEY ("ParentFolderId") REFERENCES "base_VirtualFolder"("Id");
 z   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId";
       public       postgres    false    1794    2656    1794            ,           2606    256119 (   FK_tims_TimeLog_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 c   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id";
       public       postgres    false    2618    1772    1834            +           2606    255858 3   FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2719    1828    1834            -           2606    255871 3   FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id" FOREIGN KEY ("TimeLogId") REFERENCES "tims_TimeLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id";
       public       postgres    false    2727    1834    1836            .           2606    255876 >   FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission" FOREIGN KEY ("WorkPermissionId") REFERENCES "tims_WorkPermission"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission";
       public       postgres    false    1836    2725    1832            *           2606    256143 /   FK_tims_WorkPermission_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id");
 q   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public       postgres    false    1832    2618    1772            )           2606    255788 4   FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2719    1830    1828            #           2606    245269 ?   base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati" FOREIGN KEY ("SaleTaxLocationId") REFERENCES "base_SaleTaxLocation"("Id");
 �   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati";
       public       postgres    false    1790    1792    2652            I      x������ � �      J      x������ � �      K      x������ � �      L   X   x�3��/-��KU�M��+I�K�KN�2�tI,IT��-�/*�2��\+�</�477�(�8�$3?�˔�7�895''1/5���+F��� �B�      M      x������ � �      N      x������ � �      O      x������ � �      P      x������ � �      [   �   x���kj�0F���*��of4��"���p�]�!M�_U��5�RB�"�  $P"s��ΏC�x�h�f������\�k�̆��nd���QA,,dl4����@�>EJ��S���U�p\r�z�8U���M���TH���~�k�^a��R���s����?����zx�/ㆼn>e)D>�3���M�J�Y���$�-ߞEKw�M��{��'��l�      x   H  x���;�1�X�\��Пe8��	����HAWWש��ԩ�v��^a4G@�m϶9�O�/���zc�Ŕ���{����:x>�:-|b�~�P-3�O
��ހm~Ũ�D���?V�ßZ9͹�N�}r�F����G�f�v�����w��9:��8c(�bN�n�$�BGڬ+g5�����C�W��3g�Ri�r2�5��s�|���[�ҧu&w�s)R�Y���z�ۋ�n:���d�l	�=:�E��I�'�'�-[7�㽶Y�*�`-�Upu�.��М��X���N�x�dv��7h~��d7��x�^� Q�      m      x�u�[�\�u��[�B�����0`$"��y
���FEæ�8�>�:C��h(�g��t��k�K���?���O������WL��~���?��÷���߿��ݏ���h1�Tjy���c���~������^�����������?�������_�������?��?�]��������=��/���Wz�۟�|�+��x���?�~������������޽��G���ߤ���c�Ko˯w������=�5��,|��|ê����n��G��>/����{�_���G�˿��Z��z�o���jq������'%�tSH9�4rO+m�qr̖C�l�u��<��u��𬠤������o}��~������k}}���k�_��ޥ���C��=d���a�*L�߼�j����������<�H\Ũm{{g�����x�2��������\�Oz)f
I�"�IŌ���Z,�)��I�N�VsK*_�_V;�+�g���j�V�Uۗw�C����7����xU�y�!W���g���v!�N����F��ڹ��Q�TR^5�Sk^-F畉�U*�Z#��N���1��8{ߞ�o����͋��I�K���Lj��˩����zvc{�{���u�O����L}2:q/P;G�9Z.֋5+�}���}ʲY.])5��Շ�d�cw�����w���L������d��Vo� i�L��-=u��������vsϛ~w��U�1,����m����ӌ9Q����)���e��f�l������6��u�=$�Ӣ�9)�צ)%�`��qjW�td��h��KJ�^RYb����}h���G���׾)����6O��ʸ��{�_�R���E�R�A�����G��4�U�_�s� ܏Ls�s��ɋW��k�S��'�}��4��.ÔWp$;O�r�i�:����x����<�L����}�Z;7�]R�t��	��^a)��8/kZ
V��j�Ě��)�]�\��/�a)�B���pf
��Q��<͏ Ũ���O�֝��kz�-K��lL�|zKX�rXW9�z�i��L��^S��魕����.��U�Fs>/<��@=���GeE�p��zO{e�孳���������m�LE-\ê1�y�1��e�e�k;�˕ך�����1������療��o-�̊���3�sEz���sS9��ɜ����uzz�B��.Zy��@DJ���q�;��r�
�tJa�Mkh~��Ȯцߦ�ob����f4 t� �+B�*Eah����!��\GU6fF���!���(���f����l/R@�7�cy�e����Ǭ�{T�C�/��d���Y���ii�׸m�8(&s�CW@��t�i�u�{[��%��9;>�x]e� ��Ñ.��4F=��`,S3�ڳ�����&��6���n�D9lQ+6f���u��T���r4�)�	�摫1��	w@����V�3�.��. :m m�Tt�>�1 Qe��Q�3��~���K����^��BS|���y*�w�"aC<���ǃ�;ю�U�Pf�Tk�=�+�l*7v�4i۠���a�;ԃ?��Y$B�jcL���̥�u��d4s@[��>$��[�5�G#2H�X��Q+ʻ,!�M�Q�mw�}���G*�YV�|��g &F^�`㤋����U�H� îI��C��1ai<,�Wك����v���'���܊,G�d`j�	8o�ȄP�<()�.7ʛ4�>�-��Tׁ%����0LXH�C�0㡂,�s(څϨ�˔��;ۍ+ìl:]�a�.f%�� (zs�D�����@/@3ױ8H
�\+O���r8�w>�lw86��׃F&�};pY(�͘WГn͌�*�N��i(�[�Y����-;_��nv\.��3foC���s�8��=~��o�[G�KQK����TC��>��	��R0��x���`���dˀc #��$��pCw���m!P$�EY��0\L�ʝe��W��5?�6��P���������鍕'ހZ����HC��Q%^I擡���bFe6u���=ū蠜W=��]=��4	����Rj%ӏpb�W������R� h}3v�h �n�9ྸLa]������y��bHX"��^���%�zAiKM��e�ґT[(�˔�Q�G��ΰ�g@�F��Ι��
ʥ�^��į	d4@?��Ġ�K��1����f�nu�1��˜�ւz4�,��8˃�TG����Ģ��r�|l>�zT�pZ���l��N�z�ְ��o�*H��z���h�w�� �J('�@eP�:����y�<�U�u�u8���CD�����#d�+)SƜr]tg��+{`�;D�(�h��y�Fϛ�.�w�/Rr��F���c��=�
i�g��ƪHA��2帙���2X����OCo*\U�(�}�ԔmL�*��,��Z��-ž(�5����:���>s��.�૑S�����i�d_�d��DX˂�f����f�>"�'���hY��S�Ml��fg���MM�qWƶ�Uu��0��DUp@�`Έ%�I�c�t�9 ��K)�p�Ͷ1��|�M��B a>F�əB�=�I)�h���X��X�(�*���$NQԑ�6d��^1�ݕ̵�)e!�|S������4$��s.�7�/e Hz�u>w"F����*+
"����
�R�Q��`�u��ƛ� ׭�|;N�<l-�7��d��".�Y?c jbj�^�۲�PY�e�ACH��P�F�W�op���7��0u�a|A��Z�|dk��0����i7��C��ג��]ɼU̔6p�Pe#.�G�"���\�������
Xe�y!-�~�R)�*��fq�����$fxH\tF)ڀ	w���<�}�VX!�a�?g��`�G�e���kH��]���'�Q��XcAH,����܋�I��y.�G�C���$�a�xV�l">��9��� q����E��Uf�"&b�S��X�R['��T
|8�� ��CL�p�M-�-2��6^�hAg$��q���H*OT�0�BS*1��S\nk�)[�� tEZ��М2��ej�|��+���T&7���-P��y8#��Q:��g�W�h��"��H@msoH�@ee^PY� J(�F����Lؠ�,O��Q����T6��Q�5�N���w�X:�C+2V[�%UG�m�H���0f��B�B�j��x��&Awf�ŝ�W��ɦ��%o�4�oDA�ұ�#@Kq���G�d�c��˞e ��'e.�%�t�h6�b���`krL���Q��t�)��B��9���B��;Ⱥu���7Ỳ[l��k89$8�	�ڮ?t 5zC��]o���-�k����Wr�#�c���aE�#X;���$��5t��l�W(	�-�6�����֫�:������Qmc|����:��]�@��-S�ʄ�Be%(��	�,�@.��CS�aK���:�
;�5��^��i�g�C���]:(x���ą�wu�#�`{�������#F�D�cj��I������S6�����
�w�9��C|�CP�x|Y�x�P�k���d�qz�2�V	c�m�L�����й����ѩ�Hj��v�"�u��t+7D�d�&w2��J �@a(PPOY!���
���N7�ۨ×U����b `-�ކf�:�����"\u&@���8Id�{>�U�x �|7�20���ʑH7�����9`Ę��[	��s���<L�X��\�w�������c��\��Mn���o_j��W��A�?�T��@J�'ŕ��-�0*�W9;���7.���Q"ܚ�,����&vw������G�#����)�;C��9��Bc{3�:��ah�:��̔C�d��cȌ�OCʣ}5�.�@�1��mT���xV�c;B:ڃ�MF,��qՂm�\Ԉ�c�������d$�7V"����   ��+��n���c�D)_�����1"Sǁ�:"qm��zw�t�ȕ������Ҟ7� hif3�?�H�V��$��(2T�>�銗��9� �	j��Sl0�::�F�h~�8�!
�"�=��L�ˇ1��p�X��M� �A�
�x)d��Pi2�槸g�k���%nֳg�{��0B`�(y��Rc����P��������d�D�uT�Qj���k1��#Z�[},F�����f%Ƞ9L8d-�Ҵ��	��P|=�>vz�6���"(Ԧ�e��i� ��"�,��@-�&y��b�i'O�����2��+�s�O�O�jt��(��N"�U`d����ʤP|2�V�J��Y�I
Q�ة_�bH@z#�Is����OzP��2Nڻ��3"�9�*&���n�oOk(Φ�@rȘ�6��	X�9ɨ�@�O�4~�r D$�ɮ����>��!B�P*��5��,)�lW��B���t�u�c#���^���ar��������*�Σ#*D��XE������o<��d�&�_Xu<{�	��p̀��`Yݹ! b� �(#�1��L�i�h9�������bT�M�m�I\oX`���.� �9u� Z2��B�%;���|�窣0͒;4d%b֙Z�4���@��sW6`���a�b���v��%� Y���T<N�YJ�̾�,n��B+�Hx#=aP�\�<��M���:�HG`�-�'�
�z����b�q1>!nR9��@��@�h7&	���k��uǑI�A���ka��|�MtWl�U��<�b@R���a��u܅1���I4d�صU[Zz߬�1[zH�t�L�_�p��ρ�&��z�N#4ndVEJb�D����3v������q4�t�<�� 1I�	 ���Ag�V��e�����HIbR���Kƾ`�lv>��.��}�*U ��&6JN��t��.�R���Ro4��� ��dԾq$�OCp�S�����3F���n��V~O������?kwt�{�d
w(�J'�
 D%~$�V�;���5lH^d)�3�8�'�E�*�>����5���R=q���l�s�6���	Nh0
s�P�g�I�h	~M�o2�$kzu�{��*\ţ�����WPCͨ?����fX�ɋ�w�\���b�.���S1�´4uJ	C j��%�+���|��F���Wl2�M.��V�Ab�}�E6�I�):�������� Յ�"d*6��j��$�
�m7���4������߭!�A\ˢ~�"P�� a��'��yt�?>�<3���1fH�u�wxX�n���԰t6�7c������VF�t� �G0A 5D�s:��{J�u��
�@���n����B�0���_��-�2�����:��G��@����f���L�G
e>;���Q����E��F'��~8Q�>�-����ѷ逇���a��7]eՋS�<�[և�{t�x~Q+|«��^�_�_㺯�����<�o��u_��?��7���o��g�      j      x������ � �      k      x������ � �      �     x���K�TAD�}W�R�.�8�/8��ͧ�-<l��&���!"2��F�FJ�(zc$4 ����������'7r��s	&��jLЍ&�Z�d˰~.�?(I�Qδ���텠}�8���'Ҋv��B��!h�D���Z��y��$P���j���N��/��(r��x�7��iDeG䠕 �>���+<Zc\�(7y5Dt�9�����uԀ��F9���(U%����Y�18-��ɣRt�gG�d����mn��3׈5n*{��&�m-h�qFz��/�;ʅ_D��j?PʐN���&�o�m��<�h�O�|~;����5hܪ��k�Z" ��͘~�<dÞ�u�M�5D�/#N�Fth.|f��w���YJ��	�+|3��倪��4@l��ɵ-����E�E�4H�	��І,\�ף�sWX�_����� ��zwS���U��\��xj5Ua׸6&;�˕�Q"��D��˶̶�
��u�>���J��5�8��_�c�e����{u�Ů������!9�����W�      �   �  x���K��0Dת�0��l������nͮ*�ϙ$i�Z���06l7�&�@�Q��b��r҄�D|�aҏZ"��#���T���$�%���r";��.Mk��r6&p�X}�U���x��X-�(՜�v��P�n�&
:w�.��h�f?���5�&�-D�s�`�	t/3�slף���׬���K�bg�
M�C��Yrw�$0	�=Z��DN��Σ9��s��p=����&�r$1R_͒؛��:��0�C�PU��Y�������jZ*mt�L�+B��K/�g������f�m�BD��Z�fJavZ9;����()v�f��j��c�Ƀ^\�o!.GQ�U�wHJ�q���Ͱ�1V��.M�1��� �X��*b�!�*�=j7|�<�t�E�� Kh��)ͭ�5����x�1�e�E�;�s��o}~[uJZ(����O����W�<��V�n�au�����B��7�nJ�f*�v'Nt�d�y.���z�#1����G���uu�9��~@�gf�Zk��\�'	�%*[�4���Έ�J/*ie�o5ʳ�f�����T�R���'��*��}ƃ�c��K4!\���?�-�%)|H��4��*���;�(�߻� �%����qr'j�`�n��E��
ә�� �%?h~�xA߾ۿ5�%o5�}�]�/y�?��i�K^iF�ܢrxz�y����R������`{�؊^kP^�B?�|>�� V�      e   x  x���Mn�F���S�MTի_��t�r4���ٔe�EX"��g�EN0��%r�d�'��EZn˖-�	��į^��G�do���M=+؏g��3λ?��>�\�+�^	�
�k��)
܎���A}YN�כ��2!#\@f���[�ͤ�^�;O/��bq�/��O�M<Ji�w,!\��=�:�+���B w;�nP���T���T楄�*�ZB�eJ�/�uP:8^��Rmq�����tR~���Y:�|��6�>��?&���q�0���ԙT��c)q��tT��a�3��3�/�9w9��{����s�gP��H<;��*M��[�͌ޟ�s-"Y�J����G�r��O��.���K�,l�\���:�D���2=U'�l%� �T�Wk�l"�d���Vwޛ*|.]�8���,�M=���)}Ρ�r��Jn�K�Ӳhm�.}����^���.W2�`%�D(vV��I=K�&���pw��Ls�E����.밸�؛ ��^d�r�2a�R�nþ�^^��L&2"�L�R;~wOɜ)���,�[����#�����d���� ���`P�a����{��k�"���2�M�L+�K�g��ꪨ�2L���^j��'R���,z�Ң�1���y�AH6�����Z�IȔ9���K�I�8�^�~��&U��
�W�^Ŏj��U�����;F�c$$R��X0�b[��z����%��B�/���$�m��RY�Բ�U=NϮ�c���2�H ��&��/�1���40Ղ�u�L ��cVQ��3lTTU�츻:��π��� ���"=G�X��(;�_�58`7��c?���=�U	��A`i2��!b���&n;�?�h��ñ�;�xz{	��^��/{çZ�#%J��1g!F�1��{-��]a'Jcl�0n��%�ۼ�$*�;'ۮ]����%�r?���1� �?����־�?n_$�nK��T���K	X�.?�Z��O����`��˘F�/}��5�s�Q�3� �n/i<���6N0e�뻽� O���#���f�[����$����˴�|��ǝ�1�Qˀ(�8�XJDן��%��T*6�X"0��A1/�:}S3Bh�NǓ���m6�[B�A�b�;�Y��k���I��N��I���������r�a�1R��)���Y���x(S��jh��]�WaIK�����!ه����i�~sz2*��1�\�E��-�}����jB����*:$�T���)T�ي*���¸Ɏ�8m���)%Q��0[�0/���޾��a^Z���D���ꖒ�:��}[��x���z6��V]��1f��iA�Л�c�4��7oʀ�"RoB�j�P����>tH��"ѳA�ԋzJi��|�7��bA)�ƙttB	�l8)��|I	��^N�KT)]ɵ1k�â�a��d��+�ý�9�Ly��ٱ�x�ҟ��s1�������\[K��y��\�2�5��Q��R��O�bN�a�Ju[S"�*,�Sb��k頮�4ѳ��u��m
B����)��H��~�%6��
�_eu�lꊒ�~_��Ujɠ��)԰wŲ)%���'��a����@��֯]�����*���i�H!�	�H����%��	�>�,L��n:���}B��BX�Ԛ�&�C[v���%��eI6L`��M:ۓ�����ЄX���9�nUB(�3�$k}�j�C���m��T�L��K:��w y��)ʪ�)��d��� ���Qy�Rs�1I�4y`�b�,�$kD|,�0�s3Ո�g�������7��*��p 4��0b�g竟V7%)U�;�iU���r� :��mH/�>��>�7}�k���0�~�����������2>s!��=��ٿA�d.��7�!��$I���F      R      x������ � �      Q      x������ � �      U   �	  x��Y�r��]C_1/u�q�1+=*eűd'ae�Mw�A�&9
9�#��Y�R�]���Or�0C�3���� ��{n���(���6RmwX�6M�����m��i��vߪHjAF(��n)Ւt���4�l?n�&���"�Pm�n��-NRK��f=�|�t���i'�g/�rEx�N'/�5����u�a��d��o�)6�7���C=���߿?��ϻ������ۆy����A1i^߿_�]>Ϸ�q\����Y*�*� ���"�t ��њ?l��&}�#(.g|]�0��������1���2��[����a���l�#I$>O�B-$UV0�����𔮧�ee�-J��4e����Kѩ������^u�h�ع,	�;�t�{׼����o����c�і�7G��������C���ÂK�Ѥ�a@��xIA���hc�U|�v
q�]�Cg�^D��0���w�E2l:��8�'�)�u���x[�����R�EyX7�h���m�$���M[7���t��-s�*W�R�R�ֳ96�X��4�4��A�	J�h%�9%ճϖ��!�] ��$T��9�TTg�Ț6/�7ױ���,/R�>O��䖚Z��C�� �oI���)"Pa�߹�p�=���u$L�N����6&�>�;�#®Y����juyS��AX�uګG�r9t�C�i�ȥP�V� k����)B. ��:����� ��t�!כN�����pEkU��]�$@�7�bx��ub=x���HB���,;?"��G����K'�m��A���̹>!}I�:���{�B�%��B�9�x�O�j�3y�2��Hd& ����%������P�'aTC�k0l��fP(7F�j���|�h��rh� 	j�L�}��4�:��x�ZHޕ_�����a8lb��`奡�	-$ީ��P��Mk���&A��U6d��/%��ܰO>u���$����]/ٗ��I��k�OKrA�V<�z��X��6|��@i-��������pbrwө���K�Z6Ґ��&&M��F~�:�_�am��X�0��95���S~1}&P�FTb!�|V��_���R{�J͋w�nʢ����J*�N��y������c���p��e��U��i��X#�����R]��()��r�l�*�W�$Bv�,��:�+V`���71��r���[���IU?u�j�T�~B�w�sT&0�.5�Ar��Zd�'aG��,�d�S��HJodE@�
�]o�KF���"�����>���]�z�����։���rW����4�adm�;0�-�廩�6��]��ܱNR��!�������ᴗ�K��ӵ�A����f�*g�@)�L$�R5��N�(�(
a`3���Dh.��u=��*�.��t1��1/mu���0�A��9*�,��E�Z3�Af0�����M�������\�nL2��f�80TK��A�"�B�2A].�Ϫ�ؽ�9��l�q��y}�:��ú��Ϛ���PƘ%�֡`��@Uk^������U����;�d���$���Kq�����������(���X�b��)���q�o
�������"u�c&dDFam�1�tj��\Zd|8�ijD�͠�4����0~(E%��d��t��	��C$�ç<��}r�?�YX��0��*��i��j���Г��o�आs�{>"�e[D�#y�/#+xg�+R��X$�^��ϒ���VA�����OՃ�j����7{��Su-��{Cu���#T�橁BS�BQF�l�t/S
���0/:a���܋�����f�)/#ikf$M�L��J��˛U�7���C��U�R?6��~�O�b4�N�V��\��̡��'s]I��>�>�(2e�����J!:�� J@�Pn��If9H�1.��Ї�\Z�V+}X-����jAA��K5��p�D�7�*u�΂��X�xϊ�_-mf�� �fg�^�0���],��b��0�z,?�o�7gI[m�Z�� a�8�$ub��r熔��k��"eU 4���(�}�^Q��H�R3����eѼ���aq�o~��ͯ�)fD���1K�QW��xe�옚��t��-�\�B�"z��)�ń�%j����ef��u��X��I����qM���w÷S0BW����W=�r.1p���y�~#�6�
������Q(J��N:W�	�#>��T�WS�^}�����Y1�������u�F��W�0{��t.8A9�M��E
$:�5Q����$Q����5B��LVUj���z��7o�j�k�Ų���z(|��MςW���i�x�������v:�86\�֗���	ǡ|;�f��h��mk����N�Z�Ձ9������|w6�R1�+�!�`����(A�2{��a� �C�L��G�	>M�~h�={�v�w      f   �   x�����0Eϟ�TcY���	�?ɉ� �(@���I�����V>7,���29����~��?<N����;� o�Q9��H��&j�.r�h�GgoWc��rt���g�G�<��F�	�؊y�t������H_�<y	����UL�<]KO�(�h(lL�5�@˵풕���AD/���,      V   m  x��W�n#E>O��`=tU���Pr0"�B��(��a&8c�y $/ �H+!ą����7��'v�v2N�r+V���߯�L�2�}:SG���P�e��>Ɇ�/3ȐLw2̴�0HGi]��=��o�##;��Gꨬ԰�'K����A��Xϰ���q� �����R����P�@/���ͻZ�~��G�u8��ˊ�{h{H}`�ȗّ�z(j��6D6� *��	�˥1;�!��P��~P_7�o2�!g�ف�"�]�陼����X�����ͨV����I�i0��G���A3�ayY��i]����6�V�yzhR`
��qy��.���Ro��R�N��W 3������O%���w�=٫iY�ጹ��f��ӊ�ګ(�B;|�Υ5w:��9Q��՝�H�t*�O>3�����`n��?�z��1HT�|*˦�<��$=�x���It����4�F̜��ρ�X2�T��D����<�?�v�vV�:�/�/���eX�'rj�H���:=:�_�ﯰ�D8�G�p���4.YXh�Q���A"IB-��`+�Fp�
�)r|��X(c������b���-j�U�ۦ��Tɛ��t��~QDm�gU�R�~���TH�UHyo[p �>�)g��vW��R �r�f,�Iҹ����⢷X`̍'G�7e���ԁ��"���&Ȳ���vqǚ�:]?�`�ꄽD�+���Jǂbam��lcv Kg��h�餺���ԗ�tԕ�We;�^6�&<�N��d�Q�98Fh@[�����?/n�8c�[���Ҽ���=�tmJ޲�Kp�{��XV�=
���Wu��V�\�y;�M۪nv�����d����S���Z}7{���q�\r����.�=�n���"9��VHL\=��_n��gwcr���Xs���0<��o�(��&.|�W��r0����F��G���t*v���5�j����������/��ߛ�w�<��{�"`3-v[��L�{	,��u��U��dXP�A��t�$x�	߃�WԐ��=z��ԁ4�����'M��x��ὄ�7y�%,ʆ�n�l�diѲ�.���_Y9�K��Q���~�q%X�]��8㝒Ǚ���u~pp�?�%*      S      x��[�%7�E�3GQ(C)R� z�уA}��{���HWU�B��:�|��9!r�\[��k־�o�[+U�\��M�T��?��d��:���/�~��_KyC��3�
q�g�>e6��g��t/oo4�uf_{�k��-1l�ҷԨ�ݫ{�[żιW�S}\پ�ly��m�q�]�]��)��t��=[uI}c��v�z�*'|z�^s�ޯ�]c�U3��]i�lOqS�C#���Ÿ�*��WmI�zK�����J�:���r9�.Co�^�,KE�<oc��̷��u.C���0���p�Q��hb����}T��|���xL��g��=�3J�m��b��ٛk"����nv��&�����7�"v��|��Lت�6b��R��(��;�Yghf"���l6�����ك��>d�Xk%:7-�v>?��SN�����Hg���)��0�'���XQ�p�+Fs����Pb�[+�>UQ���y��G�=�����c3�L�l�s�bTN�#,u{Q�BO��z��V�K�vGKm.�v�e��QZ;]��3��<�����ޒ�o���Ta����_�Z_{ޅ�Z݄x��q���ĝ��J�׽�u�݉a����E�3�K�t�X�����2���d��er��>��4��ښj��h�ڏ���[�֚u6טq���o��&_�����Â[�uP$�urQ��ݎv�=Q\���÷���O���j������
{m5�_�EN=Z�Y%|�џa��xOI�9�9'�F�{5��T2y��K(�E{�o��eFE=�L�v�y\L.DB�(U�<��V������:E��|���ݖ� ".Ep�Eo�8"PӬ�:1�z��o<���r!�P��1_����7�e������?�.?������o^�/�u��\�O�������������E�8�����!ǘ���u�؋�>D�KQ��p˻����aok���B�[��\��◩���|R��Zی�P���1^Ȍ���"jZJ�*e�(eO�5^�T��Cn�z+y���@Ʈ��en�ްz7�rN�k	�*�S���ʧ�ߓ�ϣΕ�%�f�L0 ~X�*�x�e���~�*��	@�4�\��h��Jζ�Y �b:��JU�G%�����l��}�4��֒�F�����������^|T�c�����NY.�m��E�m�ZG�^�F�&��P �Z�o2^��<?J�;%K9lp�%K3���"ش5��>k�*v?��"�G-*�X�CǌY6�
�V�*�O�lBp%��mLCb��(��?�"a�9��Ǻ����!Ԍ�i���>�^�� "����'��h���f#}!��m!�YO+X�!E�&��)�F,V��E
T�ݭ�?��-\�� ��@�m'��hsR�Cf�40�t�r���5;��X�HM�rV�k�y��~MB���6�\+��iޙ��"#Ezϩм�'����6���{b���l��|`n�X_|F�*^���;�i �) i�$��o�D�zoX��2��nS��l[����Q߽M�����7�ѹ��vAR�4&38k��`��
Q�.ܫ̷/��Q�G����9>�?��>�5�o��u����S�_^G~,���:��X�o��

ʨ�R�XV��C�S߱����-�i��.(���������H�3�h=z���
�,�U�6�~A�_B���9%ml:� 萸�h�T�ĬϢ�\��H��bR��f��Jۘ����[%���rJ��(FctySc���4N��p��X�݁s�j�+TU��ߨ��s-��\�Xt�� ���r��v���ZPf�Q�~�=�6ÿ�ni4H�3l�.b*��w���Y
�۳��\l���Ņ�UZ������~����
2N�&��}�@��wy���>w>��4Jl3 �x����&U��Z�����f@�x���2֣>�!@���+~t��������t��ԞE9%Vn�iE3j�{��PO�\xcJ�圠�>�k~�5�L�P	�S�*։��R �޵e�;��TM+�V�w�����Z>sڰ�0�3�\h�,��+�\8���]��f���%<4�(����r:c���IS������	��<g4�ǣ;�y�N[��_��H��T� 3-Wp·�5#�$ ǈ4�0{��>3���c�Q�sAWDXn�,tJ2�� AӉM�o�%�s�⛙�$��b��F����
���b͍̈���!kCr� ��h5��M��cX�5^�ł���ϕK�	&`�^����g{P\�����>��i�s|���G?�������z�&?ɐf򟚝�i��1�)�/��Ή�
ht�U~��,��f��Y �0��g=@Y=W(%֚�S�mJ������z�{[j�)�J�w���ykLG�z�7U(��@1@*Wu�����hl7U�� =(N�x����?��j�w�\�9ۼ�8�%���"���2ͅ;�1jY������G�v�7�������nm�4�7�yh����f��Χⴉy�M�m�w��fd���r-no�S�C�4I�w�|��%G�~o�_>ݶ]���������v@��BU��T��ͫ)���+4A��;��d��Y��s�������_�v՗1�HӉQS�n�����裣�����uD���#J�������*�RTx��w�]��m�nȥ֯����J��a�%33����6 ����_CS6Sv����K`�Y��~Ed#���.�j�7��wJ�W��[Ks��Ἱah矃SF˾@�:����_�k�+,H:�0	Z&����Z�jE�̋y�n]����2*�}���M�[�,��aOgb�K����H�inh�M+g6�;Q�*.
XK�xe�27T�4A���x;� ���݁��/#}x�;���/�1����דt1YHn �}�qp����R;�5My����#\��r�j��s��2>��;����s�3�k�F��Vx�S�����=�hG~�����B��E��HMmt~��Ri��x�t���(�*�tT�.�u�Gn��[U���c�'-����]�4���+G�}+k�H���`T�2W�r�-A ~�c��]Hu,^�IU���J���vZU3�}��N�1��C��r�J,��/Wig�Rkˍ@*��ma;���
]H�>J��^���9���_{��*Z�_.@���QH�^�zW����یi.�.�v��lmA�[���"L��l�X���
#yw�B���Ϲ	i���\"�}��^t�������>��@��4�]��ͽ
[ct��z���c}e��X��zr�/ 1��:LsUoy���p�~�ml����m�^wn�:��*��hA�x�AD��ƬD0����6��&?���i2_��A�8��m��%�]�]xpW�!	X�n!�[��p��'V5?�{�����F��k��|���G
���|xb��{Q�N?z�E.�B�I�=Hx���0��d`[֝����kԕ���b����v`q���$�$�i�����M�k�Hk�e֫���&��e�ǜ�%��_XhP?�b��1�Qv�m>8 �;�]�Ǯ�-/���MK�i��Q��qN�fn�⊇/�_fQ��;j����'KWÛo۹��0�}0O�(WpၺH�{'��4�+Q��:���rch��*�P'�ֆZp����;��5&�Ռi>w�yFt�<�S���#wd�
��1q��{,��Lϭ�3�U���2[���T���}T�Q���
?���c�k�����o�N+?z��hv��~�q/�O��w�H����~-f+'�h�R���7��mjd�I�%��W�� �p��T�I��Cj����1�e�걿ުv&X�w�A�ڡ�2mR8��R6���t�&����Z+�ؘ��XB]O��|mq��/I�9��~�m�G:gRn��\�7��'5�Z�i_�����*r^��Q�s�2%iS�(׾�k�6�k�Z��ff �o��E{xW��=��g�K�Aa��r���	�J��QVQ�Jn�L���-��7�6? �n� ������y����9H��b��W�<�5��v�� �  /���\�/.��pwn�"L�vj� x>���\kW{����G/�����et��-�u_�������?�^���>k�;fz)s��;%�6s�>�j�z���|�狺��������iZ�oɁ��t��g�I��象cmk�Jbj��=�0��~r��v���Es��⹊n���t��ec�b�/�O�%�;�ו+���܎���p5�H*(��{CH1:�-%Sq��r�w/�.�*H�a�1�c�]�?L�ӎh1k$BU�AL�6,<�×��"��ՒoSx��������pԔ{�����^�H��Z�t��cn�tALzvK�v T�^O����A�cpb���QF�=55�	������V>Z�/i�s|����~|���߿�g�_$      W      x������ � �      X      x������ � �      {      x������ � �      Z   j  x��U;N�@��S��l�=�����l�@h�A�tH��"�7�3�����6"�q1��=��C��b'u��ѕ(%U+���^�A��6�_��G��T� 2g�d	�#���W�×���j5�@��e�X���Ơl$4��n|됌I��O�������|;�ҡ_Ő�2x���3��x��z��ۮ)Cy3�`��]���Ν���pth�~݃�G�r�)�h��6����M�+�F#�d:���,�|�h|�͂'Ѽ�!�Tqt�m��a������a����a}���$�	�):/bxwi�8%ac��&�L���kwopLU��M�V@o�"-i��s%Z������ၙ      �   	  x���A��8��u�s)���O��U��Z�2��M �G�J�R�,�hI���>�R��Z�Cj�RGi��Z�����������{��G�2�F��,�f9����4�QM�h�~Dm�R{ �IT�/��T��+�w}�__�������P���������r�}����|��[~W��f�JG*�Rq�He���.��ʼ|��z�r�����,�q]�u�M�>.{��̨Vߙ���D�1a2�d:�LƘ�3�`2��(`��(`��(۟�c��>_��9K?�^��5��x�M���>K��[{/���'-�����2۟�\f����l�r���M.�}��e��7����&�AX�f2��S�GێTR1�1ݘ��1[c��,l�Y�:��ufa���֙��3[gpFgpD�Q�����yTd�<*�~^�FLY?����Gݾ��n��e��,�Z+S�*��o;�S���9�+Ӷok���m-��V ��V ��V ��V ��V ��V ��V �aFaFaFaFaFaPFePFePFePFePF�V@�<�Ӿ��u�>�ZN{��<�ٻ�?��d�mS�2}[�\f[�\f[�\f[�\f[�\f[�\f[�\f[�\f[�\�Q`�+�a؟��e���rF��i�\�Q`�+�a؟��e���rF��i�\�Q`���\�Q`���\��:�����uS?�;�W1���1g�yt/��k�q�����_��������;��Q�ӡ΀:u�ԁ<ȃ�<ȃ�<ȃ�<ȃ�<ƃ��$~l?%�2�$~0���L�3�����3�$~0���L�*�Ъ`B��	�
&�*�Ъ`B��	�
&�*��S�d<��x`���*�U���&�&�&�&�&�
y��
y��
y��
y��
y���C|��r���گ?��LN�ş�Kw�r��*���9��F�|�c��o}���ww�L����;�B�Uȷ
�V!��[�|k�z�A��w�A�<h��A�@�@��g���;u�t�3������C�=/���y�y�y�y��`BLȃ	y0!&���<���`2xe<��x����+�W���^�2xe<�
y� �A�<h��A4ȃy� �@�@�@�@�@�B(�B(�B(�B(�B(�A�<��Ctȃy�!:�A�<��`@ȃy0 ���<��`@�A�A�A@�/8���C�/8���C�/84���|�C��':4���|�C��':4���|�C��':4���|�C�N�'v�hɠ}$����G2hɠ}$����G2hɠ}$����G2hɠ}$����G2hɠ}$����G2hɠ}$���>��?t$��̞���*�6VGf9�!�5���U[�ק��~u�L��y��;��Q�ӡ΀:u &��d<��x�� *�ATƃ��Q�2De<��x�A4ȃy� �A�<h��A4��<��<��<��<��<��<P��<P��<P��<P��<Pȃy�!:�A�<��Ctȃy�!���<��`@ȃy0 ���<0��<0��<0��<0��<0��<p��<p��<p��<p��<p��<ȃ�<ȃ�<ȃ�<�h>1��Ā��Oh>1��Ā��Oh>1��Ā��Oh>qVƃ	�'���׽�y����l����%^���8n�Z<���||o}�;���:���:��g���g���g���g��$ο�u��3�u �_�:���sȃ��׹�A���T'q�u�y�8�:ׁ<H���@$ο�u �_�:���sȃ��׹�A���\� q�u�y�8�:ׁ<H���@$ο�u ��:��s�rȃĹE��A�ܢ\� qnQ�y�8�(ׁ<H�[��@$�-�u ��:��s�rȃĹE��A���\� q�u�y�8�:ׁ<H���@$ο�u �_�:���sȃ��׹�A���\� q�u�y�8�:ׁ<ȃ�<���`BLȃ	y0!&���<������P�`]�`]�`]�`]�`]�`]�`]�`]�O\W�<`���u ���uE�f>q]�O���'��3��:��|��@0�y��'��3��:��|��@0�y��'��3��:��|��@0�y��'��3����?~��k��      w   �   x���M
�0�ur�^ A�����cl6-�x+��P���6�A;�[@BW'Lq)\az;k<�]��`�A��p�Y�����7���l�˩�8sv�y�mOC��5`ߗn<{^����鋉ճ�Z?0=P      \      x������ � �      �   '  x���kr��;��D�����X�=���,��:7g�6@�0$�������!,�,ç\��cNǣG'Q���s�K[B�^��C� ��>+;�H��>ݘ�m�84��{�~��D�YS���7�1x��'��7�5�5�%����	P�x]%z@V�D��HZjkQK�;�,-Ւ�_������㢳�9�2i�tgJ�~,\nǠ��Q܈�\9\B �Hт��Yz�zU�VRN�6q��K�$9-達HJ�v�9>Gb[�p�R��eߐ�Z��px� ��I��@*$�Eo8�s�d����L٭����E�Ņ1R�!i�E�lc�e�RB	�ҍi��*����>&R��"S��..�Ur��!O�c��Z/��fB�Q��
RZZ������+�t�kE��ZS�I
��ڐ��)�z�TWd]�xqs�����������@kA���@J������:�D}z�lHyQ/�2\�QK��Z��"S��Y"�9��s ���T�:��TYkߤI\#�NV�A�8�sj��d�L/3����=3����w$1�W��NKeߵ#]����k*N�L���\���8��Z��S�$�\�˺b�5^*㑈:Bm���>$-�����_1L�}��#���t��`w��IƦ۽#8>��YWRd��ZX���fN)	�Κ��X��VҀ܃�\u���Em9���j�	����*���b�,�#�������Q/9R��H��=���7�'ZY/%�H�������o�|�	?G���e�:�`(QZ����\�%9�� ��'iǜ�y܅��ԛK� �ٻP}9��u�ν�����躹�r�'b`�[�|�r�(���aX>R�B�-C'�~��>`�S�dR�����`�ȕ_���+P�\��������&ן��Yy��EvgnO��m�<�MZ�YlP'�I�{�y:�trq+t'�ρ�V_'���@x7&��+��$ڧ���5$&�>����J~rW3 �T���v�7�"���^�,D&�>���t?D&�>��Y�Lj����I�͖__�="�y``�8,P}JI��i�9)%�����ͽ�ˇ�}�&�$x�@a������9'
 *�����:��/R�W�V�-H��8�E^�-;�<��fZ��a�D��D�R\˫+�/8����'
Q��pN7����w�$�l�A�lK�I�x���dZԢ	Dl�:�¨�C<���j����� �`�)����I_xƨ�'�A����� �Q�-�J�@L�{� �1i��-� �MMF��+1i�������)a��{Ҥ�1)/A�5��='G4��u�����[�"��$��q6�ļ}qҺS��M
k��w�A�C>S[p>��0��Y�˶����a�)Jo��{����>x�6����L�zy��Ð�>x�l8���{����a�Z�~��1j�{}�Ĥ����{��~��al��f�1i�'|�&���>xc�����=�Iy?��0P޼�Q
�H��0irsm��"�[��:x�T�oJWQ��r������� yr��      �   �  x��TKn9\�O��x_��w�@$[o��d`�q��\^�-y���PKbA�z�*�@,L�>���gP_	�����|ۗجeL���}@IE��0ږ�)EjҢ�e�V�U��-F�r�ڿ�yX�_����~?�|�-��8���$v����ù9 �(�!q�����1Y5�z	��i&I*"� ��m�E#/E�pU�`��m]�%��$ �j��&������jI* �MĻ�lD׀��D����VQ�F̺�(��,ZbE�P�p�_(�] ������T�PU�70��W ���O�sRf`=�ݲ]b��7��ԋ+�6��ܒ��?}	㌶�ʳ8+�Ԭ���`�['�+������ �čڈb��Z�#<�ʽ\:�ӯ_����w�'q�7�qq�֌�6;٣˥�E��n)��ܷ*bQ���C��@��s��������9���*���Z���Y��VJ������F����J��A/X|��Sr~N�� �,1����%V�/G� �y�`Bw_�� ��Q�4�0��1T���(��XWv�-h������/�G{���g������g�� ��~��^A��g��x�*�.jW���+��
 ͥEk�]�6��+F26:�����Y<��ϓ����n܂Δ����)ֽ�!Kyg߼
Sq��T�{�\Bۭ�o�F��V�ue+ۇ-����w�q���4M�-!��      g      x��[Mo#�u]���Z&�_�}���]�	��Ho�g��ijң�d���j�xaF��=�I`c �#���*E�%�Z�F�D��d�U���{ι�Zj.���$��jo��D��ğ�'F����7����_]�z�?�������������7�ߜ\���~�Mg��*~u��w�d��|�#oƭ�nqK�o�=m�?�����cե��U�0eUd�dQ1.D�����O��d=s>qF&U�Ҍ���2��>�$�qbR�B��_��t�yzr��l��{m/�J�&N�pj�0ݶ��Ir��N���U���ė�zG8a���x~�8?����>���\�_��d�ן�L'��_lN�g'�_>�?�}���G/K��3ſc�"�� �3Db���[���~����]z�f��T���ժ����5�Z\�>���*qY9�F>��,����:�}���ii=�0�BD+R���?D� "����z�l�z���g�-F\�T�T��b�0����9H�	;�c�� ,i��#����Pd�u#����W�x��"k�ù+g���Z|�1 |~�n�X:�����z��S���Fpɜ	��C��j�)O5!�V�]�z
w)2-Ee�g�(�V��2>�D),�zaW�I5(+��]��כ�b�A���`_GefHY��u�L{@�(�#�����T�z�jqS���:.�ރr�:Xcxa.H0�#������5#�����b��-��$�u&��RG���t<w\D��qF1�9�Jf���GU�N:vph�$!y���7��9x�r�'�}x��}�:�r��� X�.~�?]�5�y:>����OW_�>����e�?�<���:�]�q�ͦ���>]~��]�a�ty�����ο�`����^@��^]�Sv���p�_ȁtr
mxu�;�����!7��t���˰��9^ٜ|��&)�|��	������5�ꤜ�9��/�Mu~�����o_�C�p��p2�?�gu>��������f:�>_���g�OC;���?�x=���vl|��M���� ͧ��I��
����F%ٚ�q�A�i:Wo�p��C�m-J~q��g���~��uQ7F�P#>��R ]��.2I0�w&+!b���Rs��M���u���NV�Lʥ%�A2�7�+�#��x���d���yA]'��fC޼儴'����m'!ރ�C>8C�x����<܈��X]u�Q�#�܊�0	�&��!e��zΫa�;��D�̡Y��KH��=�������g��Xkl/�Њ�����+T�Ik
O׭�:�)A_��b¯}���;&����������9K�ҳ=z����> ;�ķ 
��Pt����yJ��A�FF����,k25�
�v�U�]#K5#����G���2�(r�j!o�Rߊ�ZQ�~ļ9�Ɲ��.����\�R�(���Y�	J�E;W|K�e˓���Y���'�����)�����Ɨg����?����j��ս��t�j0S���P�+ɪ�� w}fI-R
���L�>�R4���B�3x�gHM�)#7w�=�b�:��A��g��M�\l������Q�=�r�5�����(��&����Y�/u6���,��]���
�A���gc�&�H����&�	Ñc���� �������6e�AP�	~��Eف8���_gs���u_������ŷ������R�x~���-羚��'(a2�<�ȭe"�
��dBY��#B�c���N*�����o���-��]c�ښ}�(��C��:��f���%$�^�~�����v���U$l�Fu2p�vY��2.�6�J�� ��|R�����@)
�"���*������~r�ifi��V��C(��Z����--�'V���}w'�����.'ĎTi��:"��~���d�=�w����s8K.#r(�}�L�,y��
Y� �O�x+�� K	��S���J�.���Zq��5�۷�\��͇��l�.�q�>�������c��XR�זb����]�!� &�tO��{�xMJh�=��_~v��M�/����A��-�/ף;���f�g��r��z�q��7�T�.m'�`9��[~07�f"֎heO�2L����-�TA��B�a$���f�rE��hR)zd�`>@�∣�y�n�5ф���A�ɻ�� {I������t�m�W�vcEţ5�D4Y9_��q�<�T��qp���޲���/䢝{@�Yʚ�e��%*	~0Rb1��ig�+���23r��`A&.�PB�J],5��p{���+2�Tp(��x�^���<v�L�M!�L��{P.�4T�0�h���� �:r�����墠���0�t�ɡ���K���-��~��π�<|�r�'���V��h�;V7�7���!�~i�oZ�Ӈ����b��fQޣ�PO�8�!�3�	��� e+w��Xi�P"c䅇�,4Eg��	U��sq�0	�u�l�Iza������0��f�[q�Q��!h����3
XPmh �ɥ���	l;^�3GXj�����	��i�6*�r#q�d��-�#_I3 X
�}x#Š��������<Hh# -��j3�z�`~[%����_�<�0�*��Q�m�lDb^�*:<��Nhrp�x1��Y���.�i�F9�i.`W�VJ��r���­tc4;#�^n#�p{�<]	:�=i�v��#�r���]���f���ۥ�%��mG��%��]�Kߦ���d-��ˢ�o�w�S��6���E�(��֖�\c���Y\E��iA4�� �/�t�ִP��=��^{�b�k�cd{���gvK�(�Ͼ[����2�J@'j+�,x@�bƑ|	ɶ�2m��R�.�7�'|�K��8�=CdgeQЋ7<�J}����<�w�_�����6�
�=�Ϳ��ο�q����%����k60U��:���M1�Tސb�*���V��p��H2��/���"`@%�%��ZM�k�ǨI�(� ����p�c%�1�N Ė��~pж����]I2�edw�|�E�=ż��B�����S0[7	fࠌF-���0� �
a�n�~L�Ip�p������	kJ�������@ZRU��f�I�pJ�.T-��G>6+-� :e�=d\�����,���O׬ �c`�>>��񫶏�z,E�΁�si�J硩2&� ��{��׼fE�`�W�r���+$��Z6er���~|R�M��>�x��4|4Cwmy�\/��>�:�8�f��<* O[��[�- s��8_A9���6���pB��Ԁ$��p3@�"�U@%G0y2P]�K82!.�y|���qk�K�&׮ؐJ*�׿7��t[~�N{KwVؘ���jwn���O��_�7'�6ss���\hnG�M��ՏH���Ϛ��I3Y8��"���y�Ġ`fp`������^�r�Q0��s6�T��]�n�v��0:�Xr$S䲍�-����+����ĉ��yOzW��"��ֶ������:�*btV�}^oq���~��o�������ٸ��- 	��拋/Ã��!��107��~T��"	ns���ğ3+
�h��3�h��5�#�8C�+,(������\�� 9 Fd���P�ree�^89����E�;&�J�'�?���?}mn1���[b�s�j�Z�a�-i�+�b���`�"m��Iא�eU�q�����听�L��2�Ef�+>�S,v�l����q�d�L͐����Ȗ������
�]����
U+LK�ͭp�~����a�J!����l�J�����ҫr�U{�=ʂ��R�i��]{q�'�^~����Lmiir2>)�ۘ�W�%��r��"*"�K�F��Dt��|�C�ނE�|N�˙¬���L�����i�x�pr�MW�w�����,��b����C��Я���Oz�+i�>�_'ѥ�2�k[Js�
tQ��16��S�-Yk�b����P�,��#���Oz!�v=/k�8�%~�	�Ѳ��J֮M��+� >   ����RVU����L�p�!%׀��R�Ʀ�MT�ՋQP�KDs�"��V�6��|�����      n   �   x�E��q 1Cϸg���K��#�,��ހ�+�4X�6�����GR4�%�h�D�_ �M�xwU���� {X�}��|��驴ʹ	'jU9h�9\��Y��(�W�i�׾c��Tj�tEqu1�z�Qn�F�+�h^�L�C�X�>�w����p3�-������6����Dޮ��h����4$_4BSn�q�����Ksa��vc�Y��N���#�6�{��RK��{q�BZ� �O��w$�F<�[_�����#w]�      �   �  x���[�� E�u1�����CԤ}ĚT��Iu��� 62Ā�b0l������ޣ�U@���Z�s��&)G_K5u�B)Ԁi,qNP7��喖u�JА.X3"�U�M,�8���7FF��j��P�%���Ps���K0�:x�C�+/L<<`�1=�n���Q��ǣ�D��A��F�~��n����Q8Ud��rتceWK�o�䆝Pܡzi��%��(�FM�d|�A�hX�;/�������@�`�H�^����|��^��X�*�@�Y�]xĥ�i�2<$�8؏���HZ��{��j��%=,���W�T�4���CR�*��g�ކ���[J�nkdO��=��I�Ӈ�����k��N���\�Ȯ���C���`?�Z��
��      d   �   x�uλJ1�z�)��ۦ<��h��i�� (Z�ʷ7aED����b>����������[7c �,O�j��)Ċ䐓ᐝfv��w���#�ᘕ�$�8)x�,��f^Jo8�+�c����Ջp�Z�>�K��<j���Wo^�i��0��-$��!N��iȢ�|�N�O�S4�X��S��=��tt>vܥ6,M�Ķ;)�T��ʫݶ�)�IH      c      x������ � �      ]   A   x�35�45�4204�50�52U0��20 "���\��Ԙ� 2�@I�0B5P�X���:F��� O�      �   �  x����n�0�g�)��(�_�$��t�<Aٖ�]zK;��e'i�$E

ɴ%R�ß$�<*
���T�c {i��?���_�/O]�������o�!@v�j�/�������[%��0.�.��zc5^!�>��m�ˏ���H=���~�7.�B��K=�c��r��ɘ��Y�eb�<E�������~'p�瑱!V|���������Gsr��[���Ƙ4z���^R�La�Ҟ�S��S=8�P0���1''0x���bJ2�X��wrx$�ٓ	��#9��.��	��sՠ?�+�|ϡ'�T��n\½HG	c��`�cvyv2���پH�B���Frt'�8��FTR�_��_S����v_^[�$�G|����N;�w	� ��#Q��p-�@�\9%��8[�l���b�%�%@�!�Fx|�vu�LY���g��ZU��:Oq�L{������Lr�E��L�Uw�qAQ�0����!8��]��\����\�����
x���݁,�-9���
$���<�#WхZ�@{�.YS!9�l��'u�]Ι\C�yk&�r��*@��p��k��?d%bM���N�m\��`�j;��Yb��17��YS�$��)D�F57���������m��n���b��      �   �  x���;�G���S�H_�:Q����0d(�,��7{v,�ț�Q� kЍ����
/"�`�� �D�Ƭ
8�w^��B���]�W-��X>����b̅Q��C�\��^6��R|z�.�O��N�У,��{be<�<y�e�#���A�"f�ʦ�:�GA�M��gM�_����U�˦w�ȅ�V
�k2��X�*
�hcU��:����~�1�P��#��Н�[�h?�*�y���7�>?��w4�]w���WpG:��	���3M�o���v�3.�Vj+C����<@B��r�v�6�q�'m�E�o�����?�,ם�?h�ç����!R;���O�}mkfI�:��=�Ms}G{��&jF�8Ӄ�����!�wk�B���2{����
�6�X����4�zD�7и��-��Xb*�_�T�@1 ��U��3s��M��n�&�d`�9w��J��cR���t�%�^p�0��"e{��׾T���&����3�e���L�=�=D������A̕�F%m�v竎��H�Ԋ�w��뾟�?�۠31����`��0[#k�Y/8��t�`��=^�!V��g�i��fA�r5"Ԍt����/~w���U|���G�a�l0�<EZ�
2�-�ާOOO� uOx�      �     x����ndE�ו����o��K6�"�M�Jh�,�?���Y0�U���?�:>�Rq/m�AbG@Go0�X�����¬�jg�8��Cw��0�����UA�^�����[�_�u��Ga$4�����xsf/��U�M46A�ա�F�C��:xC|�����]�ToA�e�d]��y'G���
Am4�o��.�����<�|��;%Y�ہݸZst�X����]�C{C.w��E��P/��i��jHb�sP6t��R�Y�:���SE�G��b�ܹ��z���jե.c{���(8Mǝ�u�!N:��hN��U�u��+��4}����]y�CoD�o�����/)V+���8]nQbb�űR��.)Q��w�����l�kI�>D~���bb֝��{�Tim�/��D�.V�k�����_Ke~'z�����(DR�3E�+�Z��W�ഁMm�'F��:��Y�̼	�6�͞�a�w��'g�Љ���_�wO�h�-F�9�%١�7��^�:��}����.r�;"�YJ�8iI�%��	�ҖT�x�zn(�~Z����JA+�<�zZD�w�F���o-�Ю�Ωǥ��T���H�`N�	fZ������o�1��|�f��]�լ��2P�܈��?b�kˀ������o�n�6���T(R�ي���5���
����9��6%��J��������ur�+_P�:�N�����{�׀ɅmZhEfIR�]�����E��܎!��;Y]۝���k9Cp�th��l���2�mu=�����r{zz�v� �      h      x������ � �      i      x������ � �      y   �  x��Q�r@�Ͽ��^�"� 2qR�"�tl�"���s�B�F��v�*��>�^������&&׻%gkoD4T��+u(�#F��f����o�Y���|�3�-��^���>��+�tk�(�f0�J�j�!:���tG]]�b�RRf��}h��RB� REJ���o�p8�7�����u]��g'��?~/^r�S�Ճ��·�1c��Ѻ�)ژ��=V�⻀���K�Cf���"渔�y��~�p�Z��闛�s@��M2�*�L���e�jGlB>�ԌF�}Nm��o��Z �mdђ�n�g�����0�0�b�>u��@3Ĥ�74�Er@�<���A\.�4�Y�s6��y�T�ܬ���w�����O;rm��%m4G(Z(֑��anX�zת8W��V��ʫ�w@��P��Z�Jl�m�s�{=ѧ����/>~�|      l     x���Kn�0�}����7E ˞ ۲.���	���D0f��q����{!@. �膭�w�ڜ,�_/o`�a�[i|�"G	��8y�A6�U=����>��o����U�f���O�吹��mC
�F�8�m5��u�V�-�.�9Ǒu�;�8oec��'3�f�VӯW�7�.֩U@�+(��Hp�Ȅ���^p�<��v&�A�А�yP�1(U�GJ<������H��;�E�哴�=��9j��58;;��DLr@&�D)*-(�s*&���Ǭg|	�ge��٪5T��͸���g(]�#�u�6�<��8�|o��R�\`ϳ��y��t��9�vm~�|�lV�cF���Q��|㑏3M�(�_�:S8?��u���p�̘v��F�כ��4Z	�\p~�������sVf	��_T�c�|���_�/s�?c~�r���R��~�� �1 ����3`Θ'e�j~Y�	ڽw�޹rr��o�Y���{Nà씾����]�Z�u�%�j�      �   �  x�ŗ�rk�E��~� ,�4�ǩ��<��w������$uI����S�X"x$��Z��|E��-	�j	�k#�ZUez�$&2��Q��b��7���^��$!�@�M���V�O��y�������"jA�ŠKkh��`S;�h�s�������m��m�}R��+��q|�+�m��9�PgMA{���j���|�Ǹ�+���/k��ɡ�2ˌ!Z�N<�h)ה���M���"o��W1�bH�#�k|}rCy��b0��a��*����?�g�����]�o��FGU$�N������֥M���ҫ�-��+SC�ص�~K"��X�����kx��ݝW����Ghj3�{%�J�H�O_^�6�A�[t��JuDHf�
��5[���%�$�otE���,葝�Y���2��3���U$,�5����C�K�α���RE�@7o�����F�Ǣ8X�Q�U>���&�'�?	����0˲���@ U�q�>stiYn!h����t�D������h?t\�1�
�V~��x�m��V�f�iP	���<x��f�������i��S`C
p\=
�X(,]i7O�eaF1x��	,Ų�*RTN�*AK�P�bӦ���E)�#do5�Zk_2�����NN�0�?���}�i� =�a�%`��ڂܔ!9HGݶ%C���ݶ�}�9��*E��Z{�6��U��§h�(|:G;��4���[�4���r��;
`��[k�c����L�g�!3x-��4�5SIs�Եz���G���J��'�v%��W�R�N��S��fd�g��J�|(R�uk!%t��3NP\lkY����a�G�S<s:A�j�>FO�LB/(Y�P[Z P6�::/=�sk>l������لE�2TY�~�O8M����Q	�?�Z�����M��G�7��Po3�����Y�!�tf$޽��Cu�.��6ʕ�b��%_�2xNo�ϱ����o:�46�����v��a��YK�ҍnлq��
̙C�����:MKzK����f�r)���+|ֳ�)�d�;|�d����pfl°h�Ze���C�I�� ��د�wL�$h�[�=R�y�FV(��zu��9';e����,�
��������G�mS�f�5��h���C���֕Q8Ƣ��+�~c��2��+|}�~��>��᫋��Zh@�W!\x�&9q/�W�������9�	����/�R	�MP\�B��1f*���9�_6�p��I��3{���P�ݤZ���g\�iK����9|>Z2;Y�D���K��IX�k��)ә:R��0�O�P�K�m���q$o{c���G���5z���5��l�z����e��{��S��w�N%�#��7QNu�9���O�>�
���      �   T  x���;n�0D��)t � )~�@1��R�ba�ĐT���G�hIt����3�b��0����o@� �������UX"Ϫ";�"�x��gIK=��,mD�eK���Ɍu��'["ؖ���]�rѽ��~�����a�2<��𪯿��Ʀ��*&�Sx3�{S���N���W��9[��R�K�9,��ɨ�6eW~���e����$7K�$m�RC6A�G���}b�F8��<�������U�/	]��"K�͈-���e�D�O�KW�KS%n��-�k]��1F�e7[�i��qs�ɻ��.:y���8t�4e�k5"�pk�m1R��-�UU��Z      �   �  x���=n�0�k�.\�I�m� �џ� A�_Fv�'ؗ�Zw#�Ϝ!%��$��@�� ���2�k�V�RQI���-<g���^�[�
h~	QO���%�4=(�af�l ���&�����G�`0&DNz���7�M���i�ѐy��h��4]Q�`�����}VϽkE�)׾A�2Y$����t�Lu�U���"�#_�cW��Zd�$g�.�|�&�s��'fh�7�J�ԑ���\�8��r��a���"�c;�O[�i;����cꡋZd{�A�̥&;s��9|��-�I��}L��Bf���'��ޮvUsϳ�������{�W�Y-����^^G���N���j���
HI	J�
󄞹�f���-�}h?pn��;-��	�Ň�      Y   ~  x�mWIn$�<��F���	�`.�
�A���dAMe��H�DdZ�b�7�����IR����Hl6m�������f��9�H�,�3-�g��;V���E��h졕s����<<{+��(ꦥ��e����C�4{CRr
;g��x���BIEQ�V朦�̧
��-H(9r�S����WgL��tg�֗�\�y��2�j}���U�it����Wm�q��U�B:�,�x��ʃ�n��Z}CS��fKr�ny�娭nZ%�q��+:�R��K3�	�@�O4�닝�Q�ІxM�XL
����n.N�4W����V}��q^h�U�<.s�����o�׶V[W�|���F�^;a4�)�ۺ|k%���9�� A M@s�֮�/4��V+cGE�����!o�m���h�y��o�����!T9�K���y��i����Zz=$�x��>�,�,x5�F�+�S�Yo����M*D4�����Ǡ��vk�V�d��zH*]��_�6�����V��g�����=Jje���Yڻ.�[����N�6'ͲH���@�c���_�����S׎������B���S���&��PV��6;�%�@��1W��b����I=W�Y�v�Ufq��x*�z��f��o���&�%D�Q����c3m�ɱh�Z n�#9���B����fة/+ma�no��Z�;V��{���ٹd`ڂJ?sa�s���\�O-@T�v�}��f=���`H�y=$
��g�Sh�Z��a���%�QV��0��4�@���� �N��wڧ��΀PgVԨs�(�ދa�N��s�7x"h�Z������ө�����r���j����!���yL0�[wR��[ȫӊo!�}�+�w�:����^�Q�S��k��^-Ȅ#U-C�;��ʢw��.I������֋�z�FR���A����n�w��#�k ��W�F�H��l�1���#O�
Z<�@{g/����;�q˰l�j+Ց{zNw��:�@5����7P[�&�3#�q˓p���qw
�+=:��7.��� 흽@�	�O��T�~�^b����[(�	��+��������#0C}��0�����̎�h��pJY��4�z�B��DTk�T�tyO��i)W��$�
�}��ːw-\4��y{{��q41�A�)#������!���h�� |���ֱa�J򸗃Bn�a��%��~W�7����p�GY8M�9��o����P�SmxF�񻲀�.x%"�}���s��Ɨ�����f����Ɵ;�{�;���O��ҩ� s�ɪ7&ZV�>�h��j��q:�T��:= A�:����&ra.�L��we����q����!�3��H���v/�w�oL�v����֌A&�!�����E��VJE���.l�f̳����F�K��"ǅ���q�7C&{�͡A�����U��M�%.�u	]�xrA`c��Q�o��N�v\f���@�O4�����>+�4���<��mܲN/X��S���O���c�(x�,i��-�џ�Nbg�8�7�6��R?А��?3������-
t���v�|�� +墽i���a��3�q$X�;�����/�U��ͳp|�������r�      �   �  x����nd�r��GO�`��`p藰o��h��������̂Z� �
�L�Č�����YZI.�..v���0]I�d���+�����^���G|P�����|�{��?������?��o�$s+.uq�r�7�NӔ���z���~>����UZ
)���\�Ų�k�����n���[q�bK(���/���U�q����g�ŵ�Z���5�8�kj�:bp]�f-�R-9
�=��Qףs��(/��{{Q춉�����1�:xdl��rw=������(U|?5�9%�����S���b�UU5)n��(6��.��j��(�������+L>l�#�fl4\-�bֲz����b��בK��fFa��nK\�<��b�1b5�?q.{������.s���t�M�[,i�.U�f�v��s}�W]��c��yf?*n���d�c�8}w�3�aZ8T{����WfA�Zqҵ����l�En}V?|)�}�,� �^��B���xw;Z_����l��Xxe�C�h!�y�^�5��� ;lvγ�[bNl0���9c���彤ǋ��3ʟ��c��j����U%�꾸�;4�E]��ܮ��
���������!/���|_a�/�_�!y`MGU��~��y�O(W{_C��V�{��j�ݔ�G��\��lzݮ�P]^�*{�s�/��q�V�m����蘭��޺��S�,��lmYi5W��<`�Bkޅ�e��cނE�C"fPt�,J�� �I��b�V�O� �E��
���R8J��t�KȒY�r[������v�`�X4�^l�.���sy�Y��_���x?^|���%dOb�����u!]��b�5~�����zE�❔]xD�HQ��,��Sֲ�����ߋ|��'kNߢ9��	��6Vݡ�b[F��ȏ�_,\I!�~:���\�Z3��z�%�A����8�/����S'D�v�e���(���u�c.֫�OWk���Z`l�6�;��ɗK�;��c��A���(�J�G�<���'���kV��5�ǔ����6D��3�TAH�ޕ�7�oy���4�z�������[$7�?��%Vo��oi��Oӵ�JQ;�t�~�n�6a�s������i8�8&�R}b26���s�~������#��ROe!o����9��Y�(,��o��>e0������ɕ������}���K۪�Nqz$XZ���3$+,�ϖ��������-I��2�=-���2�0�W3�?ׅmd�b���^����A���a�-��~������;x�8�D�3������-P5^���ܘ�xZ�U��Ւ��Nx�'�e�����X�6�0�:N�_��F�x?lt��"�����+;�Ix.��+�t���t�ާ4-��oS[�Pyn���c`c�X�)�nĨ�&���ߐ5P>�M�P��`�v��[�ώ��ػ�7-��Ϩ�	~�����G�J��4����U{�:�D(�2CPJ�����.�/�.V���r�T��ոyr�jk1&�i��`m�:���х�;i��ql���S^4�}&�,�R����d+0%AO,Y�g��,3"�+(mc�};YS$��q�&�`�R�$酁N�0R)O5e#�;+�޺�dQ:|b?�t�b�ͅ��j�>����uvM��z�#�+��2��Tg\9����!��}0p*�����Y��( b�&�������N��Ʌ'1���A� 9(�*�k���(����&��J0���,�q�e�G[���G[���;�Т��� ȉک����~w���IҊ���Y�z"�m�|A����	(��M��N�B��G^�y �b|,���N{���m����a'V��݌��@B����J>N~��c���Y�gG��/���u"�~��p���H}�1Ys��_[�X�1Y�6i#ڽ� ��1/����-�����(���.<�4�����S��h�GKS�]Y�YIoqj��r�9;�y��ʹ!b��\o]�S���,�5�zM��f������k�Y�N��SX�d]����H[1�[��Ow �Q�O�g��@}.+��qo�:��:cܦ1ݚ�p⻔&�,t���*�y�Q}����di�����p+$�6��4��EJ���ފ�+o<��ə/��a���kx$	s=���T\A�-5VX�ah�VH���os���mq�N��:��V�0��շB�E�q�3�YԷ6u����^�N�~�i'N���t",���G�gZ.��%el�sX����L����:�\?�*��u[3t?VBj]I�F�g>ʜ�����㴰��(N����%w��+4�m��t�ki�uK�. �"����匕��њ��PCyF�$��,5D"d�!�'��s�>�uIm�`���ҕ��	s��tN��1!�[�c�P�*�g��0�?��8�(��S9,�8��������I�ֻ��V|pY1���"�6���� f#c���\����^�V��� .�{����<�t.)k?�1]��LO��9>��׿�(���ر+��,FV+�chф������||�K*�~��GԓEp�4��ʐQbh�	����oiڷ�:v.a�]������d�*���;�T�l�d��������ʌ[�(�|�l� �	�_M�܀�_������u~���I9���n#X�׶-�Fg����|�L��Ѓ���Y\���7ׄ��	}� �\���װ�*>�[c��\�|T�G{��@�����]z����?�.Li�+#?`,��<B�-�XŰ`}n���rƘӟ|.Gظ�L�ڈg곰�=�{-,�`��v?`�8�Fחb���� 5�+���1K�Q.�Py��$�l������lV\'[��&���Q�C�ҝ�q[�]����X} �xn�����k�ga?A�ؽ앩DUO�XA���%���[��ʎ������fg�Νo���4�/�aF�̶�Z�4�/��+�T������=KWǅܲ;�����Hu��`�����3�'��%ud��4����Ph2J]L`7m���?�xyy�W��      �   �  x�͕M�$5���S�"�a��8 ������ECw��H,F�,��r���/,QX��b����n��@�a�q�X/Fs�;�6v�E}���o��Z�42B	D+O����K��ˣD<�T�F@�O��E~J�vH-\�t�����A�7T�����l�:�a�����A���y��uJ�\�=Z��xQ������@�]J��ɪZ��з����qYI�P����i�\�˘�"ߘ�v�-���ٺ�[�����y��d��}�Gaɳ��_�)n%�w��aEP_m�*���T���/�K�IHM��l�l�X�� Ϟ[ڶG�����QǊ2��;��hM�-8�G���<�M���3���sr�/��z#.>a��/��ku�C���g(5� _���0FB����-�E[�>��l�4�����a��%4rы���B�6�����w�����h�P���]�IskWwO	-{օ�+��E��rk�m�c6hkV൲�X"�Ƃ,7�w�8�5/��oz*�P���7*\>�E�k�bi�!���Kv�'�q�꬞�윝��"���/�6;48��󒭗��)g�Hv#�e�| �ꝙ�W�E
T�8ʗ��9�C�<S�+ԡ�yU�׸]ro�~7\D�������:��}�5�S����S+��z��Yu��m�� �]Vo�5o�/���";%�V?~:������      �   C   x�3���450�30�4F�&�%Ph.�4���uLt-�������ȉ������ �[`      �      x��[�rd�r��_��'P�*<�'dȕ��%�����C���n(pel�7��ӝ�GVf'�+-�û.�:�\�C����$��cF����დ�⚦�V��Z�F�^I|!	�C�����;�x�?}y�<B�T^�q����?>���������?�ď��Q��i�t�r��l<g�s}p��
��XN�.�[Ɨ����~����8Kx��㟔���x�-&�2ũ 2S�xQ��6
.u�<�g]� I`"�A�ݟD��W$J�� ���ǚ!(R1��B�����C��c���*3;�-��x:�L�T[Z�R $E�>�'�IjiS|������P>���b� ����C�5��Ĝ�7 a��C��P�`}��	�����4�K�r%�:�\]^��쁳��Zbb.��)�v�_�	�C��C�BR9�(�g�8�P_ӡ����/����;RQ(����/���G�f�(�����
/$.*%�p���ƨ�+������(���ǿg��a"t�^^4G���@�Z�wGf�����ԝG5�~�"�mǊ*�Д�ԝ>E���%����0}��4��������.�/�N����
���J�L�`�$1�n����H/�
nS���P� �RQ�*�}�'g"�#��L�-���(�Hs�F=T�x��ʼ�����=����Q�y�)=B�*0o�$�|�s��Vw4|P��:+��"B��t��!/#�wH�'����f�@�<`蜅)��K�h�XPN`��e��fWS�S�aR�i3��?4]�0�S<S-MfcvE��
���م��4Ue�`
�Y�=��*�rI֨;L�,NJ��RCB�M�SK��0�0�Z����q�_	F0m0�*�fa�zb �H�{���8�W�B|��/��`�@uy�Iv%�w8��L�M7W_�aP&�̉�|!��qB����)?|4"[��D�p��Tm�$b�g��R�-���R�	ڇ��å/�*6�@N�w*�:B��?/�r��
��]���2~�W��(����%��-$=�4SLL��(3�.H���h���@��������i�;=�ݿ�}�����{����0[�2fp��V���F+�S��[�}Do��6.�p����g�e��F��3�W�a�&p�L�{��T��@�|d���)�@�>��e;^¡u�qL� O��A�(�Y�i\d����(�&�_5���q�]��5�u��XB�%� �.	/����I�@��P�2��=(X�|Q.Q��?�Y�c�B�`�d�R�9�!Ő��1«�o� C����:�]�a��r�b:�JꞢ[l��*��G�C�շ��|jt�$��j���W]d2q+��!Kq�~�P���%c0ϵ��m>�'޴���AF	��I�v��H��%�!M��ʩs@JJ+J*#�0�Cw�&?���}��7&#��R�%R=j��F(��A%5G���K�i<��^�~S'Ԧb_s���q:�^�+���x��ex��:�)�2�&�g�`���%R�Nn�@���Y�tXR��%h��R\A�ì��tU��������������"C�l�g>���������0��-}k5��o�di0S�s�XA6�O��I��~򡱊�(�v�-��1�`.3����<��n�(� ����x���V��1��8�L-�!4�W��ʌ�8��� {~.�<�A��ݑAp*~]?o�ɖ �������CL��7 ��f�\�Q�	M����&TpI�o>���
Kۂ*�Ί|X`"P��>!:�=y�Q� �TM?��JH/����r�o3�!��Oʰ�6�^��HȒ�¸ͧ
�43
�p�o�@�҅���a(�}���Zt�q���k���&��8~����{�J�y��a]�u8�0�
�TH���/��2�c�4�BUl�x��oP�k���
�J�N==��q)������y���0��eK���v1/�	�v1�Σ=�C[�Gs��������/���]�7(�S�*�3\�b�z!F
F��Ӂ�.�v������Z=��$��U|��=K<��9��B].tۘ���#8�+!j�|�L������7�E��oz��ﺝ��Ыע�.��DxQ'0T^!��/_�\��<�����q�������]b9�(rT��$I�"�&.��Ӝ0������6�~��̃��x��e�7��aտ_�L���q��S�����"��8��B���49��H�!�ٺZ� ��}�F��}��f��<�ǅL���*!��擹�����o��׊�/q@|�\��`�)����hL;��6ewvJ����\�0��l���`�LD���}�wC���c�K	ѡ��y�d��6�0\F��u��;�1�o6�N�����3|�a��J�Nqk�S��*E-TF��%��C"�K����뼲��t�<H�q�<9C7�P�1�ȷ]t��OLx��I� T���U���?��u�	�R��Z#Kd3V��ܤ�~c"	~C�����?픘�~G�O+���e�� ��PV�Z�#�'(Ȩ��>A���8�lA��a�v��7���È�g���*~�5b�.s���f((�!�m��C��3)���d�NƮ ���L���F�Yy�	�;�p!Ʋ���mTˈ��ÇFT�	|����#l��'��*����V�>p*[q�;/�T�\f_VD��t��Nuv��4�Doـ��O���ƾ���k�N0�w~*�t�*(��襮��ya�Σ߀R���b�t���eT���D4G@�
yY��C����K��m�t2�p��]����Gr�
��-Ȧ�Ȣ�Z�*��ˤ#Phu����[�N�/_ I�UO���̠�OU^|�����B2�oP�<�d���xy�?4�K���m*�}y�'��'ҙ�������wW��L5��ók�����.,��hs�&�&��/:PBcL��(��l�M��ϻ34�S.cA_V۬�Ў�5C,��=�����2��6e~�������9�O��H�(�]�[Qi:EF�3�����Л��|~
�'�I\�e�P�9c�]��6D�$��>!�C��;Ќ?��ƣ`���f}~F)tP&޺�p���< K�o��3RBE��K���Q������t�`�f�]��"����Ў��iצ�ofT��7���ٞɲ��<hqB�f�m\Sf�aH`�mx����S6��xSP�r��f[*oJ)
����ն]e��bN�A%Cc��,(or$��kA�]`;�܀�����.�Z������@	�~*<E*b�d}7a�s!�O�M�2�����5G��ݵk��"D/#jz<A�q*����T���Aq!�����K�<qw-"���,���ߛ��T�$�o6��]�Ʉ�ة`��|0��1�rI1�%3�+�Y���p#$���}`��|��-����/{S����c�4��i�03��4��!4�S���|�1E#)�3mO���֕�m�N"�z���+�<FM�{y�|�����/P�q���/��|j@A��P�cx��9��n(��G3��@e��F�k�2��P�4��fI��(]6l3�>
�]��iҗC#$㊘Ƴ���ԮC���<=28G������F�v�X�Ciiה�!��)�G(��Ò~e�<��1�Tg�Vd���*����Y���U�p��%���b�P՗�>A�h��{���������04��m�_��J�q���"撪�g��t�:�>ωo:/)"�;P�pO�<,]����Hf�(�	
����ۂ�@�o-�������-K���G+��zq���Ur��7"����f̋a��t�0�����劤$�9�:�Q[s�n��3���Ly�l�Y�Oy��C9����S���G�,�]"Sxu��}@Z�D��X������P���ށ:\ ФTĸ����h;F���5;�	r����v��)��vo{����؊4�5���W��s�T�V�[��S���$u/�D �  .��%�����|l���|@s]�Ҡs��&x���1�-/U;
�b:|�����
��ɮ�G�y��� �6E��8ћ��OPv7�\I��ֈm;uTWt��a�3Yxa�J�n��`?�@��1�F���]��C���p�{P��U�2ga�H��]�p1M�X�xPŠOPʰ�/B�Ƥ��xA�m7w��8�	���$����F�nڳB��Im�����=s��I�'CB��*�<��<A�Ikb"�1�\�u���M�{��%_�Ey;_��lu�֋P�N��ݶh���%C����ׂ��2��u{u�5�>��z=�,��2��e%�(
s�V�	��z�v�wJ��Ä����w���2&�t1����+U�~"��pUm<9
Tn7p��Q�����>l{�N-kB ���5��\^��3���?�
�wzc\L�~~{�G���ij[��Y졸6�t	bo�>S�	�~�k�~��2#���/�U�ߙF�b���)��d(ͨ�,��n�S`+))�.$*)0�N[��^�<���H�Z��3��/�y�q�w���z(F�V�K8l��?�.0�[C�TL�Y���=��SRl�Wi�m�nP|?_��*���j�(�)�&�wV�#�<�ܲ�w��X��>x�mI�ɬ^%��=ۻ�2���i�+��O���,s9Vg���=���3���7�X�}�.�;���p�aO��� ��v́0�!`
L�t����Kd�㽞��{s��U��^�V�g�}�
2�F�'�G<P�np�>��P@?��흾�g<�N>~��mW[6��⤒[��� |�l�e�IR�R{?� (>�䷠�-�lq�><��s�w��s�C�##֐�?��c�OL7!`8��
��^O1̖�!�6��&tK����g��/�*���F1}�.������1����*A׍e�` =�AU�֖�'K���&{l*�a��������
T�      }     x�͚͎�8���S���P`�X�n��9d�={ʅ��tc6 �9�>�m��RK;L��hXb���O�KC�8� @�,vh��d4�,��#�t����v��~����z�p�v��_���s_�?�/��}z�L�Ӈ���?�,*�>�ݽ���y�1�B)�'G��L&UkM��CH%�.u�>���`����Ρt�������9I�զ�v������gzxI���=XD�Z�	�5���L�c1�Z*�fg�Ӌ;jS�c:{�D��h���oкGtb���jl����b�ޗ��t�v��J��;xƪ|,���P���l"�d��\�"%?G����x��m('�g Y�[Y; �.t�*My�&E�y)&�M��	J�#�޿����?v��P����:b/A�=�,iA�����xP��������g�f:{@����������?~ƽH��j���/��["<��$t99ȱ�T����I��.n��*^�o�'x
��C���=�Ȫ�f.�\�(�NQ�qC8����UL'�Ev�2���!&�\<f�N�D/VM:�(41�4���(�%3.P����V��.ʅ��k(��5��y���z��d4"co%0��Ĝ���h�cC1[�}�&�0��ɽH�$�D�z����W�Ḻ��uH�Lؒ���4���rFT�
+�괜	�E�8��28_��'�S9��`��` �Y#�-����:$g&l��ࠏ�K�4��9�P�dr-l��R��ZG{Ut�'�Q߅�8p��u�$��|%��&lEW��]�1k��ď>����`"��؊�V�SF��������x�M���7r3$�y�3�Ӊ㕺UϮ�$-%�@N��X__��lؒ	Z��Ni������/�(����D;7F�Ɛ��2J���'��-U�C�)��o�J�:��'�ו��1��ߐ���kL��R�k��mh���ĊՈ�L.:�0��bk8a�G���i��XP��-S<7� ����bG�j@��7P8�l���(�ɓ/F|(u��S��!�±���O��o�c�7q%���tZ
>��ήQ�҄h���&�\���>DF���&Oc��M6��Û�l4q��0��D���D�k���8 �,1�rr<���_��vv��w� ���>[�?�	�ݪ-����D�(c���~��
�`���є�Y��B�/���H��HO@ˠ��4q���fH�Dՙ�?VO�ӈ�:�3�d��u���W�~I̔���IL(��TE=r���q��$D�4em��b<h�L	Q+,�f��>�x��؞�_.����.}�}د|�K��������!����Q�
�g���Wƭ����'�-�1�>kA�h�袭���9;�b5j���eNVﹱE4��U�OF��ȉ
N<�x�����l����q�%����SO����ݟZ��X����h��B�\�ߒqڀh>m��re,՛�6��촙�I�1+ЫR%?Q$
K�4PPihO�1��B������>�������[z�=�����˹-�����Zu�κ��GgH�H��	F�MR�Z��ʫ��|ܘ�gJ\EȂ�h����� ��E��\��ZF��h��i�d&�!���ەYN;�\=sB���Qu�_#E�!�kn4���/�g���_��x�n}�|�q߇N����L�~�Q��d �$'5���]F��D�/Z5ʷ������UQ���7�	����#�Y��K6$�F�E�44�~48ْF���h-A�j�� 3�6(��jea���������\��zlq$n�d� ����k�,c61��AL��]�(l/�UV%�'�Ͽ���&x������=�WKȵ�6��ն�.Cz�{}]�/��'K��Lȥ�+�L���J�8[Q�b��
�6<!g�f��V�����X�Iae;��±�G8N(�����2�i�ыXƖ7c�#Z1kz��8J��b)�id?�����8�i`#����nv�����VP���I�+Gc<�ט7v2d ;8�~���t��z!h�j�N�~�39�2k����:���������
�      |   e  x��X�n�<����ʬ��ʫ�>�?`/��5�����Q-�fQXK@��i�93�ё��D/*~��$��9���Ь�н*g-�-^��P�̮�=^_�����g^��������W�����J�.��NR��Y��/����W2�-�Hy暔f���_�ܟ�)�]�t�d�J��d�j�%��ڐ^5[�Е���������k���Q�=�����z����^W�)�T
Bc�೶0x��mi|�>�`���ؘCo�Kh���*��"���g����=���g���������z�������F3�l%H�c�'��"���v0_�<_J}挷�s�#�|t�,�.�k�Y����bd�W����ӟ����  .���g	�A1"9�BJXbe�ң�����G0��aJ1�f��
_h��z�F�����7��g\\�$YP�f0� �MB��h\�΁��E���l�`[/�c;G������4��1�tO�t]��ü�u���� ��J��5��V��iM��+�Y�[	��i�Ua��6r%�Kv�7�~ ��r]C�ř�r�� ]���z�3:\�� 
~�8�Aĳl�/�\ʷ+ܢi�iju�\;,��֥�jc�,\���n�[Pi��L���}:��Jc4�ְ:�E0V��
e&��~@@������6��G���G����]u�āc�jx�PVBR�i���خ|��?n��~L>n�d9�c��C�y��S�y����|�3˽r�5�Y��7qafĳ\�|���i�X��$%h)-�k�%&�Z�'�;���`v_ݩb/���.m|Sƪ�`Ї�q����? �w����馱�y�L	���٨�rlZ�uܵw7|��%�f��,�m;�1�d�$K�@s`O7�GR���q���ڃb����k�Ȑ��"�G�G�������l�)��)v,ڈ��������VR@�Ә#M����G��]�C���r{�]���-ӽ��L�|H�Ǫ�T}���h����P4~z�!'�l��`n);�,ڿƔb�Z�<���u���l;ޥMP�0wF��Ѷv��_��)�r��lފ�wDJ��'��Z�o�<U��5����z}w�`Ǧs��;����\���R@����m?�@4�ei7�Q�ۤ�B�N�~�[2f��;3���[B'� ������������?���d�Q�ii 
�u�Ӭ�p�܂t��|��w����dؒOۏ�X�l.u�������_�t"��3Z�B��x�i�s�ٵ�I�G���[�+�l�����K���j����6�B�i�Q�肖n�dX�bAٖ�9�چa]-d�{6��~��=C$Xn�.��|
�!�腛k�߰������Ke[yW��Q>q�3����P	��� 4R���^�����[53���(gBͶ��袔[�S'�j��8�qÏo�b���e��°V�Mp�BIy;�����}R��kbA/{��n��-����lIN����A��f�Pb�ܢ�h&t���-�wUW�#L��o�����X�!P�-5��Bap��$�v܌��((�6%���6o�[�Z��h���5���l���|�<y��|�����i��            x������ � �      ~   �  x��W�q-7;�"0#J�$��T�)���	��e��3��H� �:�����y��	린u(�ș����?K)�??�>#�Z���G�Ds�9�vk�2�����Z�QQ��G)}�<���6~�[�ҔBr�Lo�!Fzڶ͞3�w�>��h�j뛤�I����=x�������V��J�P�r_��������q�(��R��8�菝>X�io���?�Um~�����<r;��r���\������0*l�y��;G�5=��3W���5,)V���P�(��1K�����~��O�������Cq��q6����W\�<����z���d�/*�If�*��mV�9�9��u���A����N֜�h����z,T� ����X�RL]Nݺ�㲸H�C�n2+������rV��v��Sk��~y�e�Т����m����t̀�=����M��Xcז�w\�������R}6�d.��ڲ�kƞ�q��V )8��8F!��H�&�o���|�.]8�֯�c�cu�K�..Uc���#\?��ݞ���6avfȦ�@�f-�}W7�3�lK,��y�H���1!�W�y��l�@-����.�!��ch~�U-�{�W70cpS#�����Ƣ������^g�m�qe�:
p�'���g(&LAw&�J��^ݰ��k���!$G�!�h�V>]��h�Yӈ��v�U���e�e���qO�	�`i�߼{��]�U��X<���g?ؽI;����#6�S[�����w�'O��hl�;2 ��b^�iA�!��ye�G����^��;��B�0����� �6IX6J-v�<+�:��c�Z;�G���G��_*�� G���Bɼ}l7��wBVJ}�F	?�0أ=ha��s`�;y}�)H��]���!��%N�^��c�7�?��/D	�p�v�K��*�a��`�;�7\�n�>�
a�E��h��q��)����=�x\ڣ���tc}��[�T+P >�,��o�]�U�'qN�#t�p�l� :�{��O�:�
�Y�p�@9&����~���5sS��B�$�
ũ8�#�r��z!�E�Ԭ��/ծ��h��Á뚽|�+W�[�j����k4
8U��K�^�ի�`��i�˿�?M�.��|Θ���0���{w���z�{���s�Ս�84u���G7�aҌ|Uw9�dh�/􃓜�ԑ�I�8�{��F����=�iE����w�0���{��ԡ}��,���2�V�m��Q�Qo����#.���� E&�(xu�Oĭ1���|�SV"s���n����C$ڭ�G�KL������df����#h��j_�;|��yH�+oC���&����0s���q�v�@ź��,��] ���\�+�>����`���g�����R���ۏܾ�l\�H�⾐�}#Tf�+�������q�U�o`�w�<D��3a���Mv������_�~����      �   t  x��Y�nd�<k�E�)�Kn�|�^(Q��$��R;��>u��̡=��+YŢ̥$Ҟڤ!ƒ���u��~:�q����gZGc����Y�Ɗ<E,��6�=-�uj�S��sAy�~��lڛN�RJ�N�C�FI?�K���������������� o$kKk�,�Z��O��~�&�hΩl�N���e�<��|���v��#���k7t�ʾV�=dF]�xՅ��D\ʰa�g���[§L�FH���J���Rc�K��:r絁��5񤻊�9&���$w�|���WQMJk��;6�L
*5������i�p���ࣤ*���l���.�	ϫP�����U��uO���f�U�����c�4�̵��{C�N�e�lxKCc���r|
|��Mvx�O�谉vσ�x��{�����3�I�%����^{d f{�����O�zz�ze�����ӹj9����5���kڙdPs$���^���OI
���G�(qZ�c�(��]���`p�k�6���V ��Pt�MP6�-�F+�(�2׆�.�D�/�:ӫ�K8���82~�!E؋Tv���CڨsK��T������q�����qxw�5ե�X��.0@� q,����>��v��?���/��=��"&����#��W���}�K�x�Eӵ�z�tvF��c�����΋
��_��By�s�Ve�t+ɷ�r͹�;w#Bl��b$���o�C�L�d���Ut&2<d��@sζxI{��ΰO�c!��M�m���' ��aW��y]~0�鶳`��F��)Nkܘ��"<�&�5L�^�lܡ�a����!�Y7��I� ��>\�4��e<�:ZO���q�����h�����8��#,Ky���H f��3����2��J�P��1�l�G��2s,�`�S�1�1�2�~Sل�Ԑ*���*�	J�w�F�}��M���C��?a4�4Է�I�9����*B�o��o�R���O#���z��S���7���ę�\Ő����u��ñ��G(�}�6��Y��k����	�~���CK�vf���ȴ����Q8bVAXIحfp����뇈?�!�T���K��=��:L�te�r����X��vRm�����aKC6���Ϻ��'���0:u�t��9|�r�s����!P�2��e�1��T���آ� +d\�рR6�2�i�?�W�!26#}ݗc5r0S��6�36OCFhx�Zqp�o��EX�G�JB)�Ws�-��M㎠��l
RWq���)�Δ��X/=Nn������t��ӚT��>�B~�7q�tlC�����aŘ���^�.}��ߡe����~�!�rG��i>3�/|�Oyހ=do����du�?��  9ջ��_����g��B��rg��ZV?�O �=��Wi�AMs%l������ޚ���w�o��X�	˲��"�.^��9׮aܮ�,�3l���������9�p�%���3r+h@.K�H���{�jv�e�Xb� �OqԖ�_]bLdl��τ��=,���zk�'�>�To(T�8�(�*�3���tJ?�ڽ�}��u�qEs��ܢ@o"�7"�i@����{2~G߽(�lw��FV�����g��}�@<-�a�6����: ����<<g����� ��H䘖��{mm�s3�ۨ���@��������G���-�<F0f:n؜��F�1	m2���@Kutkn���e���Y��-ۢ�^!��Gȭt� 5�W�`aPș�8�ж�Z��
��<�b1fT�a�Ok�s�Q��')RXBDB�p�V�Ok�6V|UF�9��W!Rذ�~@��u������Gī��V�C���m��u�A���0d���ݐ~�)�*�,a�lA��z-p$�P�U��`����������ͬ8�����]�{�w����)h�W+�Nᖚǝ����H��!Ӝe��
�=�^"��Z�y�_ހ�>P8��E@���wK��Kڽq����fڂC��&h�1�	�?�d��b�Z)��w��.L-��tA����&��y�����ùӣܿ��y/g_�6G{��(� $�]c7�������Ys�E�������_���ǿ PW�      _   �   x�}ϱ
�0���S��K�$�XDq(]�.R-�*%��PEE��?>.wZ�P.�~[���W�#$�M��V�f�4M2�L-�2/�S�Md��1u��1m��9���P5WfO\W� M�O"����o���Fq[�is���T;C�i7z�*��m�뢭�B�YF\      ^   `   x�34��46��4�44��,I-.2��ЀӀ�������"�X�`�霑��������\	V[Ć�f@��)�9�U���@up��F\1z\\\ Ο�      o   �   x�E�;
�0D��S�	�?��I�Bv�ʍ
a�Y����>�@0l���0ҡk����tX��	ʛWv%����%�$��2�
���?}FU�Y�d�\��y#c�k�V8Z�z�;���/v$-Z��>)���T�̏�x�s�}5      �   �  x���Yn#9��˧�(�*�>D� /Zg�t���tʮ8��Y`*�U�'��dP0�j,�?� �~ �?���s�߅��"�
�.(� �Y��r���E�T�������Zޫ�A^��d�Uf��kg�v����NW��@���n�����`�����)%��
@�
����S~��?��{������|x��p��s�xb�@/���;�w�2f�:����9R��_<%���=���"���A���pс�$|/�j��5;-��<O�83\���-����=�Q��?<���@���0��ڨ��i@�D05���3�1���CN��T�s��HZ��A� ��PS��z�nj��ߜ��i
�qfc���r��-�R�A���Z�nw8�q��g��ĞPt��uKάz�"�Qs�ٝ%�\+�a�V���6s���5{<{H�E�#\�d�^G��$�{�G"��d��9��W���ɶ�:��n�z,�X�^����O�Y{�qR��%�������O�(�${�-�h���h0J�E��'��6����n[�9�<v���.dC�u��ܼ(��C��Ð\8灘�/ݫ�X	6V��D�v^�-���2�'7���P$��6��e��[�6�n�#���;��	��~�t�Jϕ��g�������#�	�h7{O����OɁ�����D.z(�UF1c�����V��f���G������?���Ucm��G������8��'���f�#�10v�J2��W�����Z-�#Vڒ�[�ݡ�-��^t�[����/����+��6�:��6���'��g����F����$�D��L�����]@B�.	��=�ɓ��l�N���N�����F1��i �=7`��C�;�\�b:B�u����e����|�lѽ�I�Ԃ#y�,��*g�'g
>?{��Cў��)��`v�V���v+���v����e���F���      �   �  x�����\7�k�S�L
EIT��p�0�֍DQ��k ����:��H�ܙ���̒jZaZ�3�u!�i�촺/�s-OSهo�>�!=<iq�w?��h��ra͖����F�����ߧ!�2��Kz�~��2�k���>��o�*o�Z��ͤ�Y�Jr�f^ �55�&V����J����>:��e���%a�VoI�qR5�gM=Mo:��I�<h�&T�Pm�����R�s"��	|�v'���r��s�Hc�ETE����l����m��Cj�P*ʜ��㫒~z|����<F��᯸�)W��H%��3�}�R�"�a��R�eh�hT���
Ӝ'h�H�w?O2�< 3�Ě����|����24yő�e"�IE5@wTGA���(|�Kx������(xۜ��޳m���(+�dd?8��A���Q
B$��L�
��"�3Ʌ���㈷�?do[ҩ���7Dx���l�I��U��Ջ�<
NiB;�@�7����u�4=�pY��(H|MsA-�J���ޘ�,%���)��܉/�Rh��H]���.Xц%�l���6$�W����}�߸
~_a��;:��#�P�9�"T��p�F�Y(0l##�W��3f�m?�J%��_]ES�!��P g	c��iA^=�A@���c=b�����Zpj$("{�M����hhn��.�U�B/"�r��.ۅ#�n���#��|��=���oI��S�u�j��`��Y1�b�^�Z^��<�`�k'̂RPA�$������S� 2K��/"�� W�������o��0��hDu�1�M�b��-ǰ|�~�!7][j��F�(���2��`�qK���L�N�nLꮓ��j���y�m�}�件'�W?�����x�So*t�����a!6Y�M,h��:*C�3OMf/����!d��~���=�W��π*������W�y��%z�����o�Vf�      b   r   x�344�440��LMN�4204�50�5�P04�20�20�357130��# [$�M��8����fh�gh`jll��4Y�i���@Ӝ�+Е���[as�,ذ=... �/z      a   �   x�eͻmD1D�X�b���ȧ"\�&�T���N���I.0�ܱ�ōz4zma$4 ��A�جk�M<4�2��2q��l�����|���إsL,������_�:̇I3���8�Ʌ���+0B3A%&I�u�b�s��sy�>��`�MM��<?Jvƻ��{��n�{��N���>��g��~ZQC�      T      x��}K�-9r�z�+����d�q���y��� A#X3��#xn�J=zZ3=��������ZgRKnj.��\�A�_�J��zi����/�����!����'jќ�����O��������_��o��Ok��o��׿�����o��/�<����o���/���ż+Z��M�k3�;��4R]�O�_����K�r����>ӜM5�
��!�L%i��/��TI���Z�_���������9�Cr�q��]>�XwpMw�s�����B��!���ү����������%N�Ɗ.���ZQ���G�7
q�� ��`xg<H|,%\�����t������t%�R��\~�i���S��r�}b(�3�ͧ%.���Ɛ��M�/ٯ�>�fo�����y9�߸�vv5s�'�>ϼ��y������'Z�����4�����Y�خ��J�[Z�[�/ϒ�8�ܻ�C�k�mL��͑\}� ������ax��+�����7�w�~x������\W�n�C��e{� �_!}�ܯ��Y�Ò0�5ޗh۵�`Э�c�y��+��D�I&"�����%�覷��6�+�,פ[�������4���d\�ۃ�ȵ4�I_"n�������U[{��<��<�h�������v��y��c.�'��h�3=^����x��?ܭQ�	OS<<�����a�XyJ{��%�Oޜ��ۘPs+�P�ǈFߙpE�r�M�w�c���� �e�M���'���#���ۂ_�]�=�Tf���p���=z�rۏ���#�I��J.��&����C�?%T���`��m��l��B2�V�YO��<�B���X�o��/i�f�gL�A�bȢ޿�UD�>Y��r��p)g�&o�gos�͌x=G�����|�i�OI>�^�$�6��'�� (��Jј��6_|���'���m$�#�M;��+�Au%��l~yx���� |�A,	�<�����?��?��hC&NF|�ʠ�+.�~�05^nj��T5\} kV�j�����mt�*�Z�h���~<�~����]05��]����f}?���˵s�p�=þ>>~6m�`إH�рz~y���P����Z���&�m"�,�\�]Ҫ���p��,D���|�e��T3� B/��窾+�a�8Ѵ൅Pn]u���L�!�������A���{����§X�Ro�N���m<�A�?�)���y> �W�}���ֺ|=@4�P�8�1�.��� �_R>E�?�␅W���0:�'�-��ox�ǵ���1��5,�oį��&���Ӵ(�;�>�^>�U�h�[��%��O�v������e��T�!�>���g�Io-\;�� o�x~�4>�,p�6���G]|��k�� �9;G`o�V�������ح��!�<js�]�B]p
k�f����V�σ@|����s�4�3 #��1㐀��`��g��0\���8�04
��Ә���.��a����p�rg{��L���cj�֮-�KN_\�1�����2�:���O��o˼�x�j��76�Â[C�I�y�4�q�Zw�u좾�����G$��\>MF��}D��pK@K��P�������
�Z�?r{Kw&���3c���!!)�"��o��9��Bʍ����&(+������ ,el���(j�MJ�\ڴ#�<��4�'� ,��cn}�T��@Aľ�6\�@~c��@G�d�*��+�����/��d�
�z����kf��7:�<����a�8���z�{��M�RP�7 kD�V�a�<��>����nZ	��m4��dn�nJ��*L�4f>M?	����p*�=��7��J�>( i#3���DK`V�^5��GJ�CHAxҍI��tx5s�^'���:��
 ��U\��!*�k+������&�z�(��#��m�	K+9�/�� X��z�/�L%�Z�H�]�0`!{��~��b��*�W�^�!�e�����R����j��!��ۘݾ6�1�C3Z��s���YА	䓠�. ��i�ve��k Q�L�nB��6�z{��DA�	7ti�`��>#zk3��w������BH ����ۊ�8�(
������e�����_@ .�&���B�^�T�i`����_�����l��`O�z�v�:��X�Һ%���ע?$X�7(j�RV�(��0����i��R��0�=$0���n)Hjm���wo`�D���}�-��A>0��ޤ��`h)��Y�W?K��Z_�M���8^�6�::��F���Z�I =Y>�/�3�/����F�2�xj�!l��� p��5��W�X9�5�-�zh�#G�+kLG�<-�L3��˽��9j���[�1
l@2�MԍoS�4ͅ0^|
/��ŏI�7�l&l �^���6�>�ƃ�:ޮA��r��q�FШ����n\�m �]a"
�I��SO��7ΦJm8�Vp={ˮ�6]ȣ#���׋��{��f��+c.e�92n~卉�k@8RD.y�/�߄iO���k��W�d`lq��t&[-�}����jZ$�P��|[s3şߍ��6
*vu��庣���Ռ���n���X�1JR��2Wؓ�L�NA�N�#0��㠓]S8��z�=����e@N�]<Q����#$��T�㉾���Q�k��k��A�#@��Q�fP�R�������]��^����=�v�z{Sn&�9�����yЉp�l5���Wf��I,'�Z	h�pq����� Nn�e�"l<�zX��K����A���s�`��p_Q�f�7GjP+��A6c]�s��U�ˬ~2��<NH͕~*px��p��qW�AQ>0l�0��@DA�Zͬ��ifg�ү*�����!���&�나�p��&.��!��
��W�e׵z/�/�?� ���r���
�n���ݗ�-a��rGI�q�>o��ѧ�Q@���L����ZMԆ_I_"����H� ��`��v7@~v�ixD�p:1�to�J~/�g�Ō��'�e����~_ρ����]� �v㧀pX�^^�;�< ��'5<�Du%��H⪌P�C( ��j Ա�g6�ٛ�=َ\Y3��:>U�stĺ�p�5��� �	��A�k\�
��Y�y�X3S5���1�9��>�G`��bynyM���6��}x|� '�˪��NoQ�k�<�}�5�Z�p�ym�n�\���cu��_����k�v���>�Ȁ����Մ�	1b#���'�o�L�xՒ%�Ϝ&�H�6��5� 1<)�~1��O��/_��e�[S�W�$��N-y�Y��8iT -\=M��rN.��b9�t��0�8�����WV4�M��X[�&>B5!�9/զ�y��P�gO^��VpI�q��Kq6�C�M��ͷ�� �tV�]�וk;��*��8hä�t
��aƗ�?p�r�ŭ���;>� ���s,@�P�E{x%l5^��HVwf��Q�q�w�ZR�s>Q{�ð����.�@��6D��cpԠ%PG˖���鏃���vc�1O����6�Q�K���1Lp�\G}�e2��C���Km@�0j&�O�
~�Φ �����a������xM��`
@���efPXw�fp�3���ydL�!$\e��V@��X:�c�-	>Y���c�cf�����<g�ag�IEkg�c���"k��\`���������Ȝl`M�^�;O?p҅�Rغts���*��+/\��x��R��F�ro觲+�}M֨&��L�k����	���+����|��ɮ�3��]%���X�>���@� �)�@Y7����(�I�� ���T�d��]����y�����l�$,^�������?[��l��$�O"� HnAD��ce���� �=�7�|z���4mA�o�tCl����0�J ��c�-��@�8$�x�@�n    f��g��M�\M@���u�(����!R�I��u|�Y��d�@+m��������5��,0\�o��^���u��� ���Z�BܪpB�!��	b�m� k6&%��t�5@��
���o#�jM���l`�}���Qt��^b�����Ȅ!��u������<�V���|�8�9��P�Q�UG�T��"�3�(5��A�w�Yˣ:O?&����9]=azd
o;=�t�6��=�
_��ukl3J�[pZ����z���,ei*����B%`�kckdp� ��)L\�
���o�� �d,�G�1�U��w�r�%�|()2�U�"_I�RܔA��C^Ol$Kp= ��M��]��ӷ)��O��C�\���&|�H�ք�~-�*A7���!d��Ȧ���|�%��������ҕ��o_��~x�@�vk���	x�	[�b)xk�U��v:{z �sxAa;�Mb�*t�cnp3�`�Ԣ��������\n�{v�̛Y�6��4���4ps_2���DP�}\3!�-&���uk#��=�3N ��	ҥup�Y�
�	/��0AD��������OL �7�vl�0�d
ĳǁ��W���l���z�}�a,��R�9�">b'f�.pS�+��]�� V��֨�y�A�c�I�x䌂��H{�;������Ʈ5��%*����"��`���쁲X'}��C
*9�!,\�*�|�%�c��̶i�Ͳ�x�/��n�R����f�?��6Z-��M��@Vf�g�C3�N���5#����.���.��̭j�'J4m�>xq��OS�E��/vz$2��6��"[��~|�s�Q2SCC*��2����'�h�$����|�	�~C�`o����Xϙ��������8<@[QV�_h����@���`P�,�:��;{SGXC�?����K�/O��J�rC.��� �"�}��L�g��c�ȼ�ѯ謃 K�


��|͊����k�B��ٛޢ���U��R�-��"�~6��(Q����T�O��<$���G͙��\�`WS����l�%:D|���$�-!�(k�l-��*�4wO�p��>�c�9�����>����H��
1����HE�w����-�OԸ\�LL�<M�@:ZH��g����>���`-� �����]�_� GZ#`��$f£!r�{��@b��,�DN���0z�6�Cy`�3�fģA���e��F��.`p`����Uy�8�6�����!p�L��
kX���'yI�C�i�
�T>�d ±Ƃ��,5g���pN�����G���u��N�����*����ȳb݋����1� Dz�D��id{!�����ϓ������� Xk��b�h߾��<����������W9yb�	*�zh'��c���=�=VqR�c���3���#�����3��~��mck�G�-�?��_[8|	_��j�uT��t
Iu���m����1�"<+�D� ��âA���6�����apb�{�S|��8Q.�p�( ���vlƪ��5'����:��AT� x��]����'�*��D�������� {a|/��o�2�~K�]��Uu*�R ��c��-�B�fxu�m��z�|U��2��&Ҕ��Ex�/�dX�)�+F��H�Z�X7�Ob.~=��3�/^>!�����w`��үE��� ���T��"�� �V�pM��4.e�xm���ش&Y�H����G܎dC@�`oz����JP�ܸY��p��4XXlo�����r�Em���8� `ɩUQ�{�?������ܜB�V�ؙ�1�c���[�ua���0��8�8��c�Ί�ހԘ߉��! ٭��r+����A��9 ����\[it5k1�5��ʔ_G8��N���o-��}R�J�T������!U[�� ���&���c�� D+������e�d��B�b3 a�����a�0��2�#uI�2I ��&w�kD8ѕ�!�cl3�����}�P�A�YY����8�= Ȯ��[�,��T�����Ւ�ڻQ�Biugk��Y�ou�õA&��L��!Z�Pl䁭�&bS����P_��f#j��py��R�2cl7\Q�>ܩ�i���W�ik>�$���e�lr�vm'�e�|�l�N����z>%lps|N�!��k�S{Y_<B,��c�D��#��6������~��+��S�$�G8��'�8�^��7���P��͑�Wݝy**/p��Ns��`��D�~��J@e=F����{*ִ�mZ���:�F_�?)�^�V��sX�5y�N�t"���8?�<� �@	�F���,���)+����p�����v��� ���w�5�����FC\#fx�"�$��~{�t�A)�pk�{-� �,�r �Pnn��LDT������sȢ1�|���h-H0n�����S1T718���:ӫQU��%�Y�	:��K	��U���p8P���_�����ؗq{s�dy�w6�@4E�|6^">�����yB=o����AE,vL4޸�dNo�/q�Zybg,��������[s��X=0[d� �I�c��r�}�@�+�8�. ��N���
��d
~fk��"��9���Ԙ[{��&�C	o��p��� J�O�� �X!���<��4�X!B�!m�Q	1���^��\�F%]���و6J!)�Ә�H ���7��x�c� ��:P�B1�"���� .tD��L�zb.�Pl5LJ��~�YgY�1�MT�Ƈ�,��)r4��6�[���ė'��yq�Z&��l��G���<!N�b�Jk����q��C*뱈�;p<HR�~���G�Ʈ-zwB[��t
hA���N����堣Sr�!�:�l�X����+j�9��R+�yȷ�մ��84,]�EXw�}�������e�G��9�[/��>�F�aS{3x� �Y��0$�X�|9'Q%г�k�jV�u-��N%��-�Q�?��Q�w��r
�*����Df0�<�-N[�^t� nv4$¡�5�E(:�B�)�h P �O@�V�T�il_�
_>Ňb7�":�	�VUO�P�,:��:��}9���4�:K`p0�Cn��]89�pGI�Lq�5�s�gç����%�T�nGf�>R�>��v_�*��@߃"���X_��^�\,�.��{����yD�������%y��: x��(n�Ā����`����^FN��4�k��d%߀�e�v)�<<�
ynQ�/)��KE!�1��f��G�]w[5i�?��!�a2X��5{+Xkq�Ϝ� ĉ�-�y�*��?=�z�N̾�9 ����<9�)�Zzv`c+j�,��!�Tw2��,�π�rH��=à�?w�ix��C��~� �-# ߄�@���olQ�d	4Oۊ{<���q�`�$�ī�۱<�^�E�EQ����|�,�p8r���̻��OS/=o3��@PN�r��J����(�"�
u�A��R"�����M��{��B���'�8H��C��ğhD5c�8.�>�x�m����s�l$`^�.���V<����
C��Y��Ԯ̸C�y��H��.��J���9s�h�����v\������?�ʋ��߬w�
H����Z�����_���#���O3L�d� (�<���V�Vzy`B��LԽz�֔	ޡ�E��#��°�N���{�%5,�&���n��k�~�	T��LA�DNx>QJ/|8	s���6e\��q��Ƞ��q`#���t��w>{Kלz�+�p�!~,��������6����鈍�d��O96�� R@�lttɆ���'n�A��.ĨW��}�5k�-���qu�vӎ0t�o��*v�Ľ��#4
����-5p��;D5�X���_�O<��@t�>>��������q:��(� ��[��#3y�\[�]�B�`�Z��q��%)�_̚Y�$\�k�� 8��pص;X�-�ݜ�[    TW��Ys!� ޸6�J��d��3�,�5\�)�.���;3����^�A��@}c��4�G��� ƙ �b�r	�� ��+5XC��W_	 ��#����^g�Ƿ�߂I���,�����8bW2y(,A�h�R����t� 7��=�5��9�('���@ �\�k^�s��O�g����A���%�&���HcX��<�=͙�g+Ε*#K�:lrԍC酊�)�LvO֘,X���u�
Y­�9 ��l�"5��;�����^��S1�@�@4g����/�4i��:���oo�T@*�%xHZdb]O��:�L�L��������b�H4���IF�E��HRl�S�=��3���+��̧5_�Y
��2��j�'c�*���s}�f��
;�o�9�Rz�/ 8ˬ�3���`?�`����}}��u/hޞ���:;�k�ypm�d�s��9$��Oއ�B�E��l7p穽��4��T7�Z����<S�\�bOw��ڭP�����Y)�­
%�zf�<e7���O,>�[s�00g��5
p��9��W�,�>[I��W���W)ۡ�ԝo���k��tnT�-����h�ʿ���'%�����+�A��iR�W*�K��OP��C�CM�-�!T���^/|D��(���3��V��w)W��}�ؚP�>���������� ��_'Va0�CjVz6�`�a���:�OP� Ы�%���9�;S23nHx���- �������N���;�)9��.pN(6\β��ß���S@$M*�.�u�M�i*U~Xn��i�qne�k��ߌ�*�NV�V��Lu�P�|�O�N�s 7��ͤ��>���J-q���u�4��G]��,6��j��!�7j��{|��B\^��朵m�����D��c<�e3'���$#Z�8O�V�w9r�Q:�WV0�`�׮BU��ך�~����}1�;��,p)��tσ�щ�Z׌�S-�u��Ƞ��Q�rLN�M:��A��+渖.��ڳ�5p�R*�`A���j��O�r=�]f�p���B�G|��:e��ǼS�� ��ׂot�
ٽ�O��7�gI3������H�&�~�B�� s��j�)�@[MHX���?ڳ3ԍ�5�gzG�s��W��E���B9M9,/��Z;ӯ�sU�ؙ�򋨀��*!�Y�(�$~;H�`���h�@�R7��8s�!N98e��/�8�h��O׍${4a��[�����>ΓZ I��p�\��$��P�{�,}���*�����-�rn���v���ɤ%���=�H {:���B������&��y1X��Ms���S�ݚD�B�c��,���r榒��A#��G�����2���Ep�/QԾ�ƈ��(g X�'*r�V��=&
N�KO[�nn΃��^�۞�M�ߓ[5V�c"��γ�s�N�V����G�P��� �CA~%R�=��oEVv���ﷁf�]�)�o�;��ְG��'������ᓃ���vT�~��v���̶ �;3��'�A�7mqj�� �u��-f<:X��J���mK�ϞO�x�|���:\`��ih�C�K��&������tt�}�幖0�(�8mxp���O4:8 ����Υo�J ��U�^5�;�_9ҏ���D��8,P6�� =V17^��D�	�k o���ف��� �[��.Ń��@/rC������F��91e�m�ώb���-L�Ӽ�cWJ�%6I��:BC�^6r~=�0��VU%68���3�ƒ�!�[�?�q�=Mf&7��Q��'_p�.E
�����7��������ۏk�h�J>ud��i�C��h�{����l�{4f�SU,���6i����@�*�G���&�(���1����G�_[n�^@�R�	0����Nf���,S���D����=���Mt�r�^�'m�GnkXm���_Q:�b`V�Z�\���g
�z{�L��a�0u]|�R�^HCIx�����6��	O�h�<�D�jr��x�)O�ʶ�!��_R  ]"x�L{�Q.����4��w
O�>]�q�u3����;+l���������g1��pn��@&@^^�Y�+��r8 ��w!q�B��e7�/�s�q?�ܸ��P'՚��FR�3�k�Q�jʥ�T�x�}>���IMy<�iɼ{������qe��b#^3�I�x}xP2�Ӈ~qC�`�/�Mg���%��+QQ���O*�ݯ+L~)�'���hK��.H_�Z�g�vPe�UL�:W ��g*�4e�(�V��-�	(����9��:"�¦mkCtq	qX��Dֈ6����o#���F�uC�O�)�RN��:֒�^��:�"��Kg	1s��M��~�48��N�{C���tn���[���F��N�x�ga�Xh��T�D_�X��Uy�}�����+��pf�4��P3�35ٲ�j�Ss��*6j��fu���IH�=df�=8_xϺCg��4wL� N/q��˕_�x7h�Ln~�VX
�����Rz���҄(�����PO���r���k{�m�:��uR�[�;��i~����'N+��xn輆QpZ�W�LG U��3|������,?E�}�?7|�I�����a�F�ц���8���ٳ��D_�Ƥ��X��I�Q��\��B�\=f�!l��pߵB<&#S���$���g�E��;x&}6�D��|���`R�b5`�;�x^^�޳���a��M.�i�����B�9j-���Ew.Ԛ�
b��圃D>I�^�ڵN���?]�� jQ|��4{z�i�r�5������@E���M�5��[�S}����dx�kр�X�"�tD�c�ʵ�(B�����|4����2�ݍ�.�F|<�NsV���Ϧ�h0�X�w�����B4�(G�c���k'��ot&l�\�nn�[�v��Dy)D�o��;�����D�߃*��\�b��'N!��� lfI���������3�OJZ�}�<do��*����Yoo:�0�z�w���Uo;��Dp�*;q?���56u�@B�fl�,O����+��F�uu�!āWV|�`��63|��!_�kĪ�v�يMB�f'�+\i��0���s�7p��(��t�����gpsi�2_� �����©��b���1�Y<�o�Ơ�E��8�0�>4܏60w���r5 �~�3�K�p�n>�ܸ�1����<W\��Q���n9�E��@M|L�տ�@9�6��]X��d�տ���� Avug�u��v���ɧ<!j�D�}m�3
���P�@C��ZǺ���pAU�Y�KG�8�_�ͳC�Ykbc#�h��k{A���'o��#�o�*��<ڀCF��{����D��k���x|V,9墸:�%�M2��_��w`�7kE�l-�����q-�x���uǬ���=� �*v����f�"~os�ؚ�����1��kE����A��pI1�6�YƖd��n�H���e���u�z �@dD�N�k����\�UY�Y/�����	�/yk��2s�l�Rj�����#�����DG��T��!8W�۹i϶�;_��})�mK�'#�l��|���~Zg^�[(�׳�%�q05q����,;q0Dә��%bK��2е��#ҝVj���~mD�̚��'��ӻ\vO�6�q�4�rB�ߍE���1��zR��(A�X�Y�<�rg^8r_����@��l��C`l�<��o>2���)�$�l���ٶ����7�y�+%�S�c���q� ݵD&^oɜF��|O�'D�>�K =mX��} D=��+Q�z �y�j��L��(t�R3P
0�>�;)�F)	�{���ی �{J������UF���a�z�|�Oճ�����.f�Q�r���wk�#�.MA�b�̊r��u�T�LӨ'4�Y�.�%Ӆ)�rų�GBDϸ�����w�M@E�s9�m�|	#PP_:���gf��2�#�E��l�    @���*{#�Z�PJ@��٧����4�D��~l/.���f������R}{��'J�O�mn�!T��ZHOE4A���܏�i[墆[?��g�۬�s����i�gKOR�Ӫ%��ކ��rE�>�Kbo0o?�R���:��3�O�U`�kՏ�*�(�Ss�?W�ٜ���{�bުg��-7�4��#�>�Jچv=�02�������*z�������1�r�RbN�؆S��+ ���DVMS��5�}DD��to� v
t���e���G+�zfD������sMt�G;r��4[�6s�[�?���Z��5c,@7#�\�с�}�3$ϥU/�(�|Ԃ\笲1������d|s���X�Nʫ= �И����U�?�31"�SI�|��z�|9(�҄h_�~�Rsr�$E�&�KY^.vP}yu�pM�r���畹F��p+���+��K�T�����̈}�O��,\�.��]��C�%g.o�Z򲺕�,���� ���=�[ b���ǆ��a���Ƽ3��󧂨\�	\�����X���xs�Cnl��tܑ��3Kx�������S B���r��bw�g&B�(/��p�"p���Į{����#��Lfr�c.��������[?7��N���nL0/�	�ox_�"��ea.��r%v����}��$����7E�ϳ>��{J���o��Q!d���`�`�^#gE�3[��Қs�!�ҙ%?Bk�?s�0���R�8��Գ�(����+�(:'�)9��sT<W������>Tý�3@gK����	<�:�>��K��j�9NvF��F4�z�O�J@���n_�r�Lp5� �֋������c��pb�V��uƹ���(OG��g��N*.���:٣��c�������l�ɑ;�T�?�>���E�{����'
��Xx"���F�x��L�����?�dv]JT�l)9��M���4�vA�؟[*���ΰ���F�e݅�8^�^��;*��p�#�����d���@�7#�'9�;�}����g51��#pb�rH�[��Ҏ��GK�t1�#
��<(�N��Y��e�l6A�;�� ��I{�����ݎ���4tӕ
�9=A�+�͠��u�k�kx���L����>�{�VZ�Ģ%&IeBm;=��4?�hf�z]>�k�S��sn�t���:�eE�;��8Q�Ch"���rmkrw��u����Sî�岞)�ow��$
\lZo��ƍI�
t,e�7W�Bΰ��,5�#�f���k�q/�"�b�p9 	��ʭ��p��O��DH1`�k�P�q��vp1\��Yj����lIk��)st)�^+��P���٣uF�k7�68<`k��o�!bk-W�p°����|�d�J��԰��α?�N4�|��)1jr�:��f��S�U���-5��_�^����w17�"�@HK���
"T���σ���H��ϗ����i���o1+!w����e�vs��?���͔�`��ͯ� ���O�mg:�L^>Q�(�]�� ����hq��	|�I��?*.Wm�}��4W-���6eu� �$=�=��>V����6�����{�xn����c�,S���s��q "�ߘ� �����;R�7�3K�ʉ��N��i��em$����q�4D��'-�"^�/��0���O��I�[a��3�P���*�EO���\�$@���!ƀ��d�N�� �|������C�'���S��W��5H�d;�r�{��q�&�����ȩnS
Fp�ݗ�l۳�C�ƻ��@�g0���MG�l���u�m�����XS��y�i��H�㐣zx�i��<��	?�F�+�\��eO�s��'`*�ޚ��W��Xn�:N?�px����l��,q��пB�d<�r�8�����9��SXMc���T�x���.a���GҐG P_�SG��;\{�G��ojM��Q	�� �GG��F�c�����F�}���Cߎ��;aI⵰�Rķ�B�fᔐ D��
!�h/:Gl���h��Y��y�8s>����� �ͫ�H{>��3J�AX��Ҷ�3���͢v��	K���.�����u��i����piA Q`^�!r�/��v��Q���y�5�,��3����u�)P�,�EQ�����CV;��rxJ�Qg�rK<�H�}�I5;bJ03=F�-�8@w�r���xm��2+"_��!_�}��N�^'>�Q�&��*܈N����1�y8г�5�������֨�Ou�AE�:(��$M���Jy&fZo�'�Mӆ�o�γv���ʔ�wms��N�~EN��{}����ɎM]��i�D����<E��v��r� .���QS8����vA'��V
50���<$��m�r�Հ0��[�lq!dg9�Sh����g��O�r=��"1E�
1��y;{�z�K(T�9'MR*�
xzm�l{�bs�LQm4��y{c�g
7�_'%�����Se΃
-��j�6-���k��7O�﫽�t+��M��N �e�~ؚObpb�	��5�., ����\��\m��&���/9��o>������fp�L����`仭��a�98�<
7���o4W��b������eY�hѫz�@R�h{w^���.b�ǝi?��I��0��s=��ި��'{nQ�A� �C;���U��8�v�.3���X��l����r�d��.JZ@��e�٣pF@<���|���o��� J�)i����@UR�i�)�Osz�	��%)9��Ġ^��f;�H�6��S������R��Aӛq�WL�Y��-P�*�L�sg�����'1�n3I�{����dn�*���!VS$�O"�Oc�S��cH{�&�u��Ȭ�������=� Ve��I�]fb�O�ws��b`�9"�}�b��H3��6�gP�-d�g�qw'`�R*�� �4^W�eV�cs�V=(s�޴�Qtf8;�O��J�����8;~����a�N���l\ڗ��r�	Ů*�vz��tx �&3�t���H�mC;�@O��_��4ٹ��������CKa���qFy��r6cPC�~/�M��f`A�v���}l�^�����kת��W���*_)2���F=�}��<�ޟ����z[���>6������{�>��v�#E�2�j$�� ��l�S>~D����b �>���!gE�熰��,���Ei�s7���<Dm��������7�����,��/ ����YSΑ^����9琘�C��� 8�ip
�M��V�+L�rC��X	��d>���^<6�R+��:�n��Pjr奴���[�G�*�x�ݘ�G��-~��� `�&��=n��$����Js��aR�zC��y�R�h�?���,��J��3�
��\�;E��ɧ��G���>�w�PS�w"�����
�3r���\
��+�p��j@�	���wJ1I��Gym:ф�S��n������h`߆P����Oi�Z8�>;���:]?�K��b��˷�3��k�+���̓K�"�9�!Qw����xk�;
[�)�s]�B�*H9���wX��:[�Bh���X��l��AӵI��2"bs[���
�A�pl:1��rЉ=,�\�,v~�%;�E��&9^%���~ޞH�cC(W�v��
ⴸ���t�&��W��_�`H��+�r��Ё��)�:@�g�V��\�CXs+<��m�H5I�Lxr�m���]�"��~��s<�������m�)vc� �ƄN���3<bO9u�Ļ����2��;uW�T���Y�.�qA���̱ �\m�)��繬�J���2����C�yA��d5�F�����2�V|&��f_� �6@D-��-�Ͱ�9Y�]^q1r�k�b�5�(�c��E޻�<DϪ�/P�F \��]h�]�T&��*��glxy1���Xr���i��@�\R�P�7?����s�2�    ޘ�Q:j��4�T�G�r����.S����X޿�MF�U2OZҹ(e)�N��4��^�7I?�ҩ�}W8^v��|��7DL@[6gl�xn�پ��6�BG�0V\�&���S�-V)"Mۀ�+M�т��ǵ�����%������0J�����v[�(l����|dq���*H�w[r�蠺�#�3�N1�[�`�S�>9�/�^7�%?J���r"Ʉ��b��G�|;�(i��{�
PG��=�����c�������#��Ngj�jG8l�p����5u�愛e��A_�M�_���r�a*Em���877��?*�J��E����^�է�M�1���Uzl�*���­��j��l���O����t� �e���<1��~@¢���kC��kSfo�����H�=�������|%������rJ�w�>ߤ�2N�r�I <q�h)�w&,ߞ�]���?ȵ�2fE)�0��^��w0*����^���NN\��wwd8Ͻ��;����4��T~�m�n  ��R.��ph���# �\�������}�<(�z՝;��G��t��6`��u���=����|g��[	��< '����x�TAF��3y9訍D�����eQ���&<�A�oN�0��zy{��/�ĘpS��H� �ır�Vj8�9��$ʋ��D�������̳��,�ႂ�9��ُ���e�9�N�J��D�Z�����΢�	�pS;!>ӟ8������F\���\�V����Iu�ƌ��6U�s���%��j���yyj���Ev��U�`h[1�=��w��q�kM�`A�6������#��AH]��|�?�M+F�#��I��s尺qr�{� @H�&� Aw�����J�[қ�gQ�6P��H5�r��}��s|��zPN�t]�#�䬓�.�j&�M):�]��⑾<���=��*A������,�q��^�L<���剘��xCWu�\��	<�s��5ש�p��o`�?:ο����a7<:A�6��6��6� e!����7ݡ�^	��p-�+]''Μo��.aR
�G���P�~���?֏֏���������ٕ���{�g{���߾M9	)���!l�wɸ;\�B��x�: �K#���<��XIa�k\��-՘�悑k+�Cv�s�	�T_����������s��<�<����Тt6��؋�wg\kO�Ixa�N��ϳ�p��Z)���Y��+���r��o�oI����&";�k��;ʋ/�S���k�+җU�8	Ԛ,�U��,5+B���q��>��}9�����?�􏿿M�(�{��Z5�i����]`�������G]`p�ݧq`�ˈ9Q��w!pCp�c�M�����Y	v����r!%�ؕX��l-�{H�i>z3�[�'d��yN2���}�"Sԅ��`�����k��k����H{8���ykD���
�?xBy|c��0D�_V���Tx����:)K7�f����|J_�퉬/z �X|ǌ��(|)WznO`���������zL��p!A�Re�wr ��ɥ����|:��"�^�۞#��|4�(�	�z5ʨ�}��C`E1���u�/5\�[lɉkrI}����nD�z;�S�0'9]�%�l�6"�S!O�P��Bv��}�C��_`��ף�5��cE e;F�ӄ�K�ck�'�>��ј�LR���8��B6�T:y���Q��ףؕA}^9r��zGč�d�2Z�*0�-�pn-�G��9̇��k�� l��*�r�)��F�٩�
��4�eV�+�?jT���4���f� �F7�b��h����/i��W&���6�*��^��3�'�� *e	�ŷ�C;{!��6gz�v3D�4x�C�|�˹��j��x4�@�D����"j�O���bB�t���	��
욧lR
��Rl��s�D��&ҸQ�n���5|�����	N���E����%�聇 �r�&}��K`�O�s�c��ɍO�GB�����JX}��7q�l3�
��Pb�9��=踜��b��I�=�3��C�Љ�1��{�m�t�,�Ru��(��ƕ9���iR^q�>�����!�(�פ�~8��Msg�*�P�̠Q�Բy�R�/f��y�QnK����|��C/��Ta�fZ��"���ϒ�̄�m���;8��!�A�"��$n8�YK_�X���ߧ������R�Q��l8ұfح�/��X�����\z�zgY��mP�.Q�j��/�<�����k�3��	1�ff�[m2S��T����^����a�O�|k�K�x L����0�T<ԙ�$��A��+W+ v��O[8Z,g�_�Z?R����Ofx�{{����FA������z�9⮔/�y�Q.6f�F�`V}�'�u6 ��:��^j(���o��Z����r�1�V���)�X`��?;���,�<U�4�9#y�4��͑��p��8
kG]�RV^5>�P:�zX2�r�G����q;)k\&/�t����j.�>:,ʾ����xv m���0O���a��N{��l����2,�:�;���ה�?"����Q���;O�N7��w��0Y���X�gc� �T�y/��Vr��?���8#�?1N�%@?�O��JZ=:xp��N�i�W-MP*��*}�)(pa���8{�ow�2S������B�YTJ���y�Ps��㎽q��{m�ͧx�U�:(� ��(l����X�ͫ�G��;�ƔA�������܇��3��.�����}M��p=��D�x��XP@Pc�ؑ=HI�9a��A1��t��tm&=9�6T|g�^�����l$�~w��}�L��@?�������M��b��rA�ۥ��$7��n`P�D�c<`#,�}�f;����۰� X x����R�*�Q}�;`�	rH�j3�˷�g�k��>h0�o�e�4��
�ך�o��K/e�~Y��`pI��&�*�Uqg��rﶻZq�g�ۂ��:M9u��|a6�7��!����v����zO1�+f�3��/��j�n�)ox�^���� �WPJ��ӵ*�����ruhG�,�����������T�[|�}3�A�)���h����������K.��|/f%�l\7��n���s�Lm:Ov}B>}�xmjw����3�(��<��*jN�Y��嵝J�Ӏ��"9I�������Sjp�>�� \����AL!Wj��-p�������#�4����g�A�9� E��]��2�����1�w\e?77�<��B�Q{���u�Le�7�H!@J3��R�ay.ϳ�}���꭭j?��=Z4'�*�����Yz�]��6>��u�D(s�|xg5V�CB�f���xf�q��ꔺ3x'�t�j����!�׍+?�-�[z7���̀�Tn��H���#2B�+��ZoOs���� W�YWV�`8�Rƙ���Sb�p��1����
�Υ�7F�n�fn|e�]=�s��h`��j�Y����p=t��l�Y]��K�xm�����d��A���ȗ������%`�9R/���,)F��ښOm������W3<@el�c���iJKTc�g��	h���sL��l:�8�p�����@�l�7[�����"� o_Y$��ă�ږy!$ΎZ�5gݯ�Xڷ����!J>��tTc	�4W�I�{�F���&- ��WK����ـ��)�qE9�� }ψ��L��Ҏ��(��<�ES+��M�Ҩ�]�0"op����p7O�v����Ƥh��(*���:{ꕲy{��d;� ��^oU\�mI;9����(�0��N����֣����/���*�G˶P��5J� ���ƪ��=G����V�3[$�������n�Y�PV�O�n<��j7��,}q��ͳbuM�(37�@��oo�f�uk�l7�&���w�3����n�遑�;����J<��h׮ �ޅ[�|cG(70�#[I �"!���,qf��Է
�C��<�R_���қTp���0�nH�4�����Kc;�V�<�^�2 �
  ���#��f�@�%����ڷ���]K�;��,�߽����_B�>���� @��T�-Q�H�(��D��Gw���e�=���w��,}��@�֊c3{���)���������w!�8��fj�fRGwf"g>� p6���"$%�O�"�5;�J��tu��MD�>)��P;�<&v��V���M�3��%-R��dR��(��2VC�{� �DI�-H{nV]��f(؅G�)Ev@".f���6��[��Z���k����>� #������87�J?��i���bUH~R��J݆p�wi��z����q�����#ԆP �1��B�YPaN�)|���^MO���$O)l$�AggEՓ�����.�y�����O��kb/)u?�6�&Z��h���P�O�˷�>��Nݲ �*��F��J6Jn���+%�AUU~�}z�f� ,��N�Ym������ ��o���C���D�Y']u�z��68���ӘWZ�з����;�ͳ���A�yX�u�_�гo[�2��z�~�4��E(=��V{��I;��'�D��!����e�C*�O-d��V����'7)�%&��t��eNs��i�K�@��ޟЭ%7�%wD�3��d&(Wn�A�8B��
�N�-u�Q0���	ɪ�(�����["R*���6�b�"6c��1�* ʕ����͓֓�pIb<lq����߶�� �rmDK��`]K"���
�������Y}y*����]��6�V��FALDnMX)��'j���4��E=s�>	/��
�J�m�������B?�4���Ym�'PG�ͷ-n��fx!�#Ro�}����z�RҨ�`��"�6���{z4/�n�g�4�}������Q��%� ��O#"��=��g\����Q��^�v����ɻķI �x�ٖˢ���m�y:��qn��g!�Է��kV�늳��H���s��9�3�X(��D����$�O�I��q��˷�B����W�r��& 6�����ըU��λ��{�뙏ڽF��H�\��e�J(��_�zj�-u����|�m|f����E��c(5JM�B����M�tԉ��禨†x��w`�|r���T8�e<�e�f��d«���s�,s���Նu����N�,��R����Y"{��k��x�C@=T��Bp�7J����6�n"
�
��������a��](�I�m!A2u��o�]W��o��R���g�9{����J�qg�k�r��98�%O)ꝧS��Z���D~_����!#�v�@k#QI�g�������;B7��t��4|}��e�N�ƿej���g���Em[��+�� G��>��L�_�
y|�k����Y���K32[eL�_�@ҥSXM�i�G�8ps�6�J��$�eEG����uO�*��}�6�Џ%�]��m���$y�!�	�P�H>�?[�ЌD_w-A�҄b�+�+�Ri& ��	A�8g?�b����j	�/ώ�P��Z��M_zd�ޔ��Sj���z��B-(dv��~y� >H��" S����.�Ba�4!��+�p쯺���y|�r��,uq8/���e��R��ȵ
L�bO���/�T;�����#����l���\/�hm�q.����zd4D� �2�,�ؚȑV���p}�?|O@V�<OF�*-c׻��,��l+W��x�1�M&�%H�� �k�cd#Br�gį���6��ٔ��O3�@-�}�5�KY��6-���&-�Pr�5k�hl�ԭ�{ZE���@��)��!+��A�8)�X�$:段ǜc9�"��7���P@O�����mP���G7b�#�c�x[;�����m�D��|,�"ipЙi�[>LT@��s�#�Β�9i�	kt�,=7ԍ��ŀo��=K
M��Y������ܷ��
�.)��H�)p�� m�K���\�NM��G����ڄz���K�Z����+�'�^��U�W�RV�^Ǵ7E��{i: ��U�_���m���T*xit5�	$]�hB���d��V�x]JyY����a3�*���F�>�!�|�-��:y��cz�x*v��15�	Y�t��!קHXϚ�oi[��ab,p��˸k!J/dW���AҐ8�iD�@"çyБ��r���W���C&[��NvF���@������Q�-�I<B��*tq�����P��s�r�ʱ��f�m��ĩ�D��� yp�҄��ӈ�l����z Eخ��ӑ��y�;�!<��o9Yv�zځ޿G(〼����p��O��MVl�O\c�U'j�#W�MVE��R��<(�\� \wJ,�B"10v�ʨ��(~�.�\�'�u�ܭS����Jy��\h:$5����tK�� q��nE�ɓdH���d�L�_U;I���"L�����L9tT��q]O����,�^P�=o"BBT3K�J��^.�������(����>��m�K����@�Yv 8��2�W�p�"?���7��6��G��'��]%-�5������{L�\�wJ'`�a����e�A�\��V��@������,��M�U�W�ϐC]��{�5�c1�?�v�v ���W�fK֢f�約����E�X�R�{��d^P��^�n�~����`ؘ3      z   �  x���MS�H��ү���̡K�,��&XF�M�c/�\�
dɣ��ϓ�/�fc�@���*�}3+3�9�x�23�Tz���ĨR�FU�n��|�eūY��(vJWU��w&��)���Z5{S��3Cg:����Q�z�E��7p�X�n^UZ����7�m��w�K�?�|��4un��_3gY��I�A'B8��^�M�/�b�;�Tdi�J�/J�c��T��u^�;uk�]
�"w=�9g�m��u��ƷJ'5˕�MY����3?ԛ�9����Fhnu�iU����+���lR������H�z!¹���}��j_T`X [�%~Jۿno�캰�E΅7�L�3�<7{����s	�΢�h���t�-rc����s��}Zg���|�.kK�?C2%M^��9s�k��3ן�8gm��r��� n������ZsV�aCU4%����JU��7�t� r�u^�4�o�sE�-�N��zY캸vb�xɖ��������!�AC��u�����v�+Bv�7I�3(/_$6������{͕��)@N,W�%5��p�#{��qs�#3������Ɂ0˥�i�
�B�v[ �J��k` K!$�lHL]�]!N�Me?�Nŝ����Q������|��d`#�����F�jL�tIk- �.IIvVu�����ڟ$5�!�T��M�tB��6}��3����\��I�f8!�:�� �@�gM��m��ʽ��J��YW���yV� V�tf���H���X�98��gZ�� \6���)O.���]�ս]F 4�l���H���۲�ػ[�TSu�Gb��ڦ{w\-H1��rm�y�I�N)"�Ps)UźI(���6Zs ͧ�7�$e>Y�9q�E���ֹ�)厇�>�Hj��I.�y�jm�g7$��gi�	�!��kަԳ1{+���s>Ctj�u��0D9��͖�լS*�¹�\��n��s rnp�.�7�lM�,�Sj��r��۔�zԙ���w�V�Q	m�r| n�8��o�KvثLI'����Y�x�z��t�xP�9tM5S�c[~+$T����M�8��Y�BREA��~�.B�����{���T����Űoa�"�s�(�v�%N��u��y�0����s#�ǎ��4���;Jn�oh4sbZɏJj\���g�s�K7�#;Je$�����58ȿ��,�Z&[e���/ǾA�ڐKm]�3��v�.Wt)�Vo��>L������)�]����U0A��$Zz*�c1�l�0H��6[=<�!���aD�zK�_?/o�`ӛe~?��yz���1�<��t�oW�9�FbK����Ǵ��uC�����r!U�p�`E�D��u2��&���T<������c�'v�E�vL�&��.zxۺ(1{�f��>� �;�`���������E�Ǵ��<�h�`=�����N�1��G�fi�6�<�����uGC��h�<��rM�zb��A�<�gA�$q��u_�<�����R댌ob���u���})�4�vx[���Z�kZo�����T !E���i7y�i5�m�é�>�.���ˬK���
���l�b܅�M�7o{1��}Qdb�~�D�C�θ�MF�����HAͼ����9�}�_�cb����a�iu�sj뾇O1C�З�K$`:1���;3���N��Š0�L�%��@W��gcyt�SxyT�H����Ĥ{q&t�q4R�׺ֈ�������Y�F�Pld ����4�.i�C��<�J�_����m�(9�:�GJ3��_�W�X3@s��q��Q��/iE��B�y��� yT�T�&'��7���6������������[yN��]��So��������H��!%e�V�CV�u����N*2H���� �>���w�ih�T��x�1Mڷϡ��1&y��u���~����t����mw��Pa�/'�o�C���e߽�,�a׶:0:�h8�MV��Lj�FV"Y�Oo�Ӳ'YfoCڿZ%WRa[#L~���H�����U����Wn�}Q���0��Te]I)VG�}p�Q�q�f��� !�ʛ���t�ӡP�.�������?�	�c~�s�؆Caz��y<�za8�9Z�|`�]�d����!�1���?!�J�"i�wO�Ǭ�6�2�� m�f�R35W�*����?��)w����;H�F6t��ä����u� Vp��      �   �  x���9v%!E㪽�G#��t"��/�����v�#�����ņ���\��x�{�-���iЦu"�����un��4f����t�X7�x�<�Jm@�:A��`��Dx������������>���V�WΣ�Ӱ�w�M׾̨G���dQd���������%_~��7fiEdK�/��'��]}����!�>|�'��>����#��̵�B��0�tĶɋ��o~�|D��_i�>�d�y��h�
>�.8�ʞ7��_�?qS�U@�|� D�~lyԱי��������/��=��|1���g_���E p$�*r_�%��Y�"d7|��	=�	��2���-���C��������[m��
ۦ�Q��3F����B�����?���������m��5?9t.Ρ"����(��fg�o���v?k>�����9T      `   G   x�33���t��Qp��II-*�,�4204�50�50V04�20�2��362573��/k �X������ rF�      p   W  x�}��N�0�g�)����N�L�	��R5������#��ؑ�!�K��Cx�	N�R
�t�����ي]L�dts/Y\o�0��K�-�٠ߘd\tǂ	�Ae#��p4�|�Ph��I���hH��Pp��Iv���)��~�ǐ����!�3��70��j=�;��S�X��!;]���ab�����I���޺D��6�]�p����I�Йd�SF=�������upm�L�x����u��"B-X�f����)��C������$ЪNt}���Z[9�-��C%��^���lDĜo��n@�:�{Rd0�I��A��l�⣐�47|��>j���      q   �   x�3204�50�54R00�#N����Ԣ��J.#�����9Bְ�DG�=��ř
�7�+�f>�ݘ�����Pvx�Ba����J�گ����pd���3���Y L3�K��4K�i�E��fl�0ͤ$�"�Lu��Rh��i�X(,=���$h��R���L�z#�pƫ��B�<��0��.gh�)���� ���_      u   �   x���Mn� ��p
v]a��!z�l���*[Qoߡ�dGU�!x0o>�z��ʫ��F� e ��g0����2� �R�˪�K�����؊�n����R����9�`0��>~Է2$F�;
iC;
��ܮ��r=�����R�a B��O1B�+"�3��]2E�3�z
3���2�9��#���qmo�]���l_��&
ʸܻq��!��)�7�ZM      v      x������ � �      t      x�����0C����u[�!��3����/qA�)��e
z��E���ӯ�x59B�Ъ�9�L���>B�p&��.�&������g�nnpj�A��:'|��t+��Ǧ�62�T���9�t\<      r     x�e��n�0���S�r�Jd;vN�'�4����
H@U��@WvP.����:�C���?4�z�q<�l�c�MWjr�p9hk���F��$x![�5J"�2�\�j�R�mʭ=��T.O�ZK	ϻo�G���S3_ǵ�ҢPT����.�\�.�Q
4dgk�gZȜ�����vj�K�k�U$	9Q41k,�sdI�&��G��[ٲ�����M����lݴ^gW�Db�;���xmrBkl 	+jPs;�v���IB�p>C@~]{SU���j�      s   V  x����m1��L��D��pj�9����^^����	}���B�O��E�yP'�����L@�~���sr\FAxxxZ��x���eų�id�g�g���CxjK����w�d�Q� W�+�u�9�GAxxx_qv��0Eܲ��{��?���(���O������uZ"+s~g��a�=�?h��*�y=���uū���s0��ŗ�Q�u��mW[Zs|�9��������l���=m鎷Q �Ȳ��%�u����L���ẳ����ٌ��c���e���(�8>m(8\�E.^�ژ�Q�����Ƿ̹V��|������v��7Ĵ��     