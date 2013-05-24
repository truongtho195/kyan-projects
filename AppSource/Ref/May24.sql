PGDMP     %                    q            pos2013    9.0.3    9.0.3    �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
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
       pgagent       postgres    false    6    559            �           0    0 #   FUNCTION pga_is_leap_year(smallint)    COMMENT     W   COMMENT ON FUNCTION pga_is_leap_year(smallint) IS 'Returns TRUE is $1 is a leap year';
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
       pgagent       postgres    false    559    6            �           0    0 �   FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    COMMENT     �   COMMENT ON FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) IS 'Calculates the next runtime for a given schedule';
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
       pgagent       postgres    false    6    559            �           0    0    FUNCTION pga_schedule_trigger()    COMMENT     m   COMMENT ON FUNCTION pga_schedule_trigger() IS 'Update the job''s next run time whenever a schedule changes';
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
       pgagent       postgres    false    6    1751            �           0    0    pga_exception_jexid_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE pga_exception_jexid_seq OWNED BY pga_exception.jexid;
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
       pgagent       postgres    false    6    1753            �           0    0    pga_job_jobid_seq    SEQUENCE OWNED BY     9   ALTER SEQUENCE pga_job_jobid_seq OWNED BY pga_job.jobid;
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
       pgagent       postgres    false    1758    6            �           0    0    pga_joblog_jlgid_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE pga_joblog_jlgid_seq OWNED BY pga_joblog.jlgid;
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
       pgagent       postgres    false    1762    6            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE pga_jobsteplog_jslid_seq OWNED BY pga_jobsteplog.jslid;
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
       pgagent       postgres    false    1764    6            �           0    0    pga_schedule_jscid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_schedule_jscid_seq OWNED BY pga_schedule.jscid;
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
       public       postgres    false    1784    7            �           0    0    base_Attachment_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Attachment_Id_seq" OWNED BY "base_Attachment"."Id";
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
            public       postgres    false    1838                       1259    254557    base_Configuration    TABLE     
  CREATE TABLE "base_Configuration" (
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
    "AcceptedGiftCardMethod" integer DEFAULT 0 NOT NULL,
    "IsRewardOnTax" boolean DEFAULT false NOT NULL,
    "IsRewardOnMultiPayment" boolean DEFAULT false NOT NULL,
    "IsIncludeReturnFee" boolean DEFAULT false NOT NULL,
    "ReturnFeePercent" boolean DEFAULT false NOT NULL
);
 (   DROP TABLE public."base_Configuration";
       public         postgres    false    2368    2369    2370    2371    2372    2373    2374    2375    2376    2377    2378    2379    2380    2381    2383    2384    2385    2386    2387    2388    2389    2390    2391    2392    2393    2394    2395    2396    2397    7            �           0    0 .   COLUMN "base_Configuration"."DefautlImagePath"    COMMENT     T   COMMENT ON COLUMN "base_Configuration"."DefautlImagePath" IS 'Apply to Attachment';
            public       postgres    false    1819            �           0    0 9   COLUMN "base_Configuration"."DefautlDiscountScheduleTime"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."DefautlDiscountScheduleTime" IS 'Apply to Discount Schedule Time';
            public       postgres    false    1819            �           0    0 (   COLUMN "base_Configuration"."LoginAllow"    COMMENT     \   COMMENT ON COLUMN "base_Configuration"."LoginAllow" IS 'So lan cho phep neu dang nhap sai';
            public       postgres    false    1819            �           0    0 5   COLUMN "base_Configuration"."IsRequireDiscountReason"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRequireDiscountReason" IS 'Reason box apprear when changing deactive to active status';
            public       postgres    false    1819            �           0    0 -   COLUMN "base_Configuration"."DefaultShipUnit"    COMMENT     f   COMMENT ON COLUMN "base_Configuration"."DefaultShipUnit" IS 'Don vi tinh trong luong khi van chuyen';
            public       postgres    false    1819            �           0    0 +   COLUMN "base_Configuration"."TimeOutMinute"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."TimeOutMinute" IS 'The time out application';
            public       postgres    false    1819            �           0    0 *   COLUMN "base_Configuration"."IsAutoLogout"    COMMENT     U   COMMENT ON COLUMN "base_Configuration"."IsAutoLogout" IS 'Combine to TimeOutMinute';
            public       postgres    false    1819            �           0    0 .   COLUMN "base_Configuration"."IsBackupWhenExit"    COMMENT     ]   COMMENT ON COLUMN "base_Configuration"."IsBackupWhenExit" IS 'Backup when exit application';
            public       postgres    false    1819            �           0    0 )   COLUMN "base_Configuration"."BackupEvery"    COMMENT     R   COMMENT ON COLUMN "base_Configuration"."BackupEvery" IS 'The time when back up ';
            public       postgres    false    1819            �           0    0 (   COLUMN "base_Configuration"."IsAllowRGO"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsAllowRGO" IS 'Is allow receive the quantity more than order quantity';
            public       postgres    false    1819            �           0    0 2   COLUMN "base_Configuration"."IsAllowNegativeStore"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."IsAllowNegativeStore" IS 'Cho phép kho âm';
            public       postgres    false    1819            �           0    0 +   COLUMN "base_Configuration"."IsRewardOnTax"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsRewardOnTax" IS 'T: SubTotal - Discount + Tax
S: SubTotal - Discount';
            public       postgres    false    1819            �           0    0 .   COLUMN "base_Configuration"."ReturnFeePercent"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."ReturnFeePercent" IS 'Phần trăm phí trả hàng phải trả';
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
    "StoreNumber" integer DEFAULT 0 NOT NULL
);
 )   DROP TABLE public."base_CostAdjustment";
       public         postgres    false    2353    2354    2355    2356    2357    2358    7            �           0    0    TABLE "base_CostAdjustment"    COMMENT     `   COMMENT ON TABLE "base_CostAdjustment" IS 'Chi show nhung record co IsQuantityChanged = false';
            public       postgres    false    1814            �           0    0 -   COLUMN "base_CostAdjustment"."CostDifference"    COMMENT     Q   COMMENT ON COLUMN "base_CostAdjustment"."CostDifference" IS 'NewCost - OldCost';
            public       postgres    false    1814            �           0    0 &   COLUMN "base_CostAdjustment"."NewCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."NewCost" IS 'NewCost*NewQty';
            public       postgres    false    1814            �           0    0 &   COLUMN "base_CostAdjustment"."OldCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."OldCost" IS 'OldCost*OldQty';
            public       postgres    false    1814            �           0    0 (   COLUMN "base_CostAdjustment"."ItemCount"    COMMENT     ]   COMMENT ON COLUMN "base_CostAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1814            �           0    0 )   COLUMN "base_CostAdjustment"."LoggedTime"    COMMENT     w   COMMENT ON COLUMN "base_CostAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
            public       postgres    false    1814                       1259    245766    base_CostAdjustmentItem    TABLE       CREATE TABLE "base_CostAdjustmentItem" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductCode" character varying(20) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentNewCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentOldCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "LoggedTime" timestamp without time zone DEFAULT now() NOT NULL,
    "ParentResource" character varying(36) NOT NULL,
    "IsQuantityChanged" boolean DEFAULT false
);
 -   DROP TABLE public."base_CostAdjustmentItem";
       public         postgres    false    2359    2361    2362    2363    2364    2365    7            �           0    0 1   COLUMN "base_CostAdjustmentItem"."CostDifference"    COMMENT     i   COMMENT ON COLUMN "base_CostAdjustmentItem"."CostDifference" IS 'AdjustmentNewCost - AdjustmentOldCost';
            public       postgres    false    1816                       1259    245764    base_CostAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CostAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_CostAdjustmentItem_Id_seq";
       public       postgres    false    1816    7            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_CostAdjustmentItem_Id_seq" OWNED BY "base_CostAdjustmentItem"."Id";
            public       postgres    false    1815            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_CostAdjustmentItem_Id_seq"', 1, false);
            public       postgres    false    1815                       1259    245752    base_CostAdjustment_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_CostAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_CostAdjustment_Id_seq";
       public       postgres    false    7    1814            �           0    0    base_CostAdjustment_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_CostAdjustment_Id_seq" OWNED BY "base_CostAdjustment"."Id";
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
       public         postgres    false    2580    2581    7            �           0    0 !   COLUMN "base_CountStock"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_CountStock"."Status" IS 'Get from "CountStockStatus" tag in XML';
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
       public         postgres    false    2583    2584    2585    7            a           1259    271743    base_CountStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CountStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_CountStockDetail_Id_seq";
       public       postgres    false    1890    7            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CountStockDetail_Id_seq" OWNED BY "base_CountStockDetail"."Id";
            public       postgres    false    1889            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CountStockDetail_Id_seq"', 125, true);
            public       postgres    false    1889            _           1259    271736    base_CountStock_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_CountStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_CountStock_Id_seq";
       public       postgres    false    1888    7            �           0    0    base_CountStock_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_CountStock_Id_seq" OWNED BY "base_CountStock"."Id";
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
            public       postgres    false    1767                        0    0 $   COLUMN "base_Email"."RecipentFlagTo"    COMMENT     L   COMMENT ON COLUMN "base_Email"."RecipentFlagTo" IS 'Recipent Flag Options';
            public       postgres    false    1767                       0    0 -   COLUMN "base_Email"."IsAllowRecipentReminder"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."IsAllowRecipentReminder" IS 'Allow remind Recipent Flag';
            public       postgres    false    1767                       0    0 &   COLUMN "base_Email"."RecipentRemindOn"    COMMENT     f   COMMENT ON COLUMN "base_Email"."RecipentRemindOn" IS 'Recipent Flag is going to remind on this date';
            public       postgres    false    1767                       0    0 )   COLUMN "base_Email"."RecipentRemindTimes"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."RecipentRemindTimes" IS 'The Reminder Times of Recipent';
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
       public         postgres    false    2228    2229    2230    2231    2232    2233    2234    2235    2236    2237    2239    2240    2241    2242    2243    2244    2245    2246    2247    2248    2249    2250    2251    2252    2253    2254    2255    7                       0    0    COLUMN "base_Guest"."GuestNo"    COMMENT     <   COMMENT ON COLUMN "base_Guest"."GuestNo" IS 'YYMMDDHHMMSS';
            public       postgres    false    1772                       0    0     COLUMN "base_Guest"."PositionId"    COMMENT     >   COMMENT ON COLUMN "base_Guest"."PositionId" IS 'Chức vụ';
            public       postgres    false    1772                       0    0     COLUMN "base_Guest"."Department"    COMMENT     =   COMMENT ON COLUMN "base_Guest"."Department" IS 'Phòng ban';
            public       postgres    false    1772                       0    0    COLUMN "base_Guest"."Mark"    COMMENT     [   COMMENT ON COLUMN "base_Guest"."Mark" IS '-- E: Employee C: Company V: Vendor O: Contact';
            public       postgres    false    1772                       0    0    COLUMN "base_Guest"."IsPrimary"    COMMENT     ^   COMMENT ON COLUMN "base_Guest"."IsPrimary" IS 'Áp dụng nếu đối tượng là contact';
            public       postgres    false    1772            	           0    0 '   COLUMN "base_Guest"."CommissionPercent"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."CommissionPercent" IS 'Apply khi Mark = E';
            public       postgres    false    1772            
           0    0 )   COLUMN "base_Guest"."TotalRewardRedeemed"    COMMENT     o   COMMENT ON COLUMN "base_Guest"."TotalRewardRedeemed" IS 'Total reward redeemed earned during tracking period';
            public       postgres    false    1772                       0    0 2   COLUMN "base_Guest"."PurchaseDuringTrackingPeriod"    COMMENT     `   COMMENT ON COLUMN "base_Guest"."PurchaseDuringTrackingPeriod" IS '= Total(SaleOrderSubAmount)';
            public       postgres    false    1772                       0    0 /   COLUMN "base_Guest"."RequirePurchaseNextReward"    COMMENT     �   COMMENT ON COLUMN "base_Guest"."RequirePurchaseNextReward" IS 'F = RewardAmount - PurchaseDuringTrackingPeriod Mod RewardAmount';
            public       postgres    false    1772                       0    0 '   COLUMN "base_Guest"."IsBlockArriveLate"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBlockArriveLate" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0 '   COLUMN "base_Guest"."IsDeductLunchTime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsDeductLunchTime" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0 '   COLUMN "base_Guest"."IsBalanceOvertime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBalanceOvertime" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0 !   COLUMN "base_Guest"."LateMinutes"    COMMENT     I   COMMENT ON COLUMN "base_Guest"."LateMinutes" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0 $   COLUMN "base_Guest"."OvertimeOption"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."OvertimeOption" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0 #   COLUMN "base_Guest"."OTLeastMinute"    COMMENT     K   COMMENT ON COLUMN "base_Guest"."OTLeastMinute" IS '-- Apply to TimeClock';
            public       postgres    false    1772                       0    0    COLUMN "base_Guest"."SaleRepId"    COMMENT     C   COMMENT ON COLUMN "base_Guest"."SaleRepId" IS 'Apply to customer';
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
       public         postgres    false    2325    2326    2327    7                       0    0 $   COLUMN "base_GuestAdditional"."Unit"    COMMENT     K   COMMENT ON COLUMN "base_GuestAdditional"."Unit" IS '0: Amount 1: Percent';
            public       postgres    false    1806                       0    0 .   COLUMN "base_GuestAdditional"."IsTaxExemption"    COMMENT     N   COMMENT ON COLUMN "base_GuestAdditional"."IsTaxExemption" IS 'Miễn thuế';
            public       postgres    false    1806                       0    0 .   COLUMN "base_GuestAdditional"."TaxExemptionNo"    COMMENT     a   COMMENT ON COLUMN "base_GuestAdditional"."TaxExemptionNo" IS 'Require if IsTaxExemption = true';
            public       postgres    false    1806                       1259    245374    base_GuestAdditional_Id_seq    SEQUENCE        CREATE SEQUENCE "base_GuestAdditional_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_GuestAdditional_Id_seq";
       public       postgres    false    1806    7                       0    0    base_GuestAdditional_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_GuestAdditional_Id_seq" OWNED BY "base_GuestAdditional"."Id";
            public       postgres    false    1805                       0    0    base_GuestAdditional_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestAdditional_Id_seq"', 103, true);
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
       public       postgres    false    7    1774                       0    0    base_GuestAddress_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestAddress_Id_seq" OWNED BY "base_GuestAddress"."Id";
            public       postgres    false    1773                       0    0    base_GuestAddress_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestAddress_Id_seq"', 221, true);
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
       public       postgres    false    7    1769                       0    0    base_GuestFingerPrint_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestFingerPrint_Id_seq" OWNED BY "base_GuestFingerPrint"."Id";
            public       postgres    false    1768                       0    0    base_GuestFingerPrint_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestFingerPrint_Id_seq"', 12, true);
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
       public       postgres    false    1776    7                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_GuestHiringHistory_Id_seq" OWNED BY "base_GuestHiringHistory"."Id";
            public       postgres    false    1775                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_GuestHiringHistory_Id_seq"', 1, false);
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
       public       postgres    false    1778    7                       0    0    base_GuestPayRoll_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestPayRoll_Id_seq" OWNED BY "base_GuestPayRoll"."Id";
            public       postgres    false    1777                        0    0    base_GuestPayRoll_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_GuestPayRoll_Id_seq"', 1, false);
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
       public         postgres    false    2415    2416    7            5           1259    257323    base_GuestPaymentCard_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestPaymentCard_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestPaymentCard_Id_seq";
       public       postgres    false    1846    7            !           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestPaymentCard_Id_seq" OWNED BY "base_GuestPaymentCard"."Id";
            public       postgres    false    1845            "           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestPaymentCard_Id_seq"', 14, true);
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
       public       postgres    false    7    1780            #           0    0    base_GuestPhoto_Id_seq    SEQUENCE OWNED BY     L   ALTER SEQUENCE "base_GuestPhoto_Id_seq" OWNED BY "base_ResourcePhoto"."Id";
            public       postgres    false    1779            $           0    0    base_GuestPhoto_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_GuestPhoto_Id_seq"', 231, true);
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
       public       postgres    false    1782    7            %           0    0    base_GuestProfile_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestProfile_Id_seq" OWNED BY "base_GuestProfile"."Id";
            public       postgres    false    1781            &           0    0    base_GuestProfile_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestProfile_Id_seq"', 152, true);
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
       public         postgres    false    2513    2514    2515    7            M           1259    268352    base_GuestReward_Id_seq    SEQUENCE     {   CREATE SEQUENCE "base_GuestReward_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."base_GuestReward_Id_seq";
       public       postgres    false    1870    7            '           0    0    base_GuestReward_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE "base_GuestReward_Id_seq" OWNED BY "base_GuestReward"."Id";
            public       postgres    false    1869            (           0    0    base_GuestReward_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestReward_Id_seq"', 2219, true);
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
       public       postgres    false    7    1772            )           0    0    base_Guest_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Guest_Id_seq" OWNED BY "base_Guest"."Id";
            public       postgres    false    1771            *           0    0    base_Guest_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"base_Guest_Id_seq"', 276, true);
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
       public         postgres    false    2276    2277    7            +           0    0 %   COLUMN "base_MemberShip"."MemberType"    COMMENT     f   COMMENT ON COLUMN "base_MemberShip"."MemberType" IS 'P = Platium, G = Gold, S = Silver, B = Bronze.';
            public       postgres    false    1786            ,           0    0 !   COLUMN "base_MemberShip"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_MemberShip"."Status" IS '-1 = Pending
0 = DeActived
1 = Actived';
            public       postgres    false    1786            �           1259    244995    base_MemberShip_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_MemberShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_MemberShip_Id_seq";
       public       postgres    false    7    1786            -           0    0    base_MemberShip_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_MemberShip_Id_seq" OWNED BY "base_MemberShip"."Id";
            public       postgres    false    1785            .           0    0    base_MemberShip_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_MemberShip_Id_seq"', 1, false);
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
       public         postgres    false    2516    2518    2519    2520    7            O           1259    268509    base_PricingChange_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PricingChange_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PricingChange_Id_seq";
       public       postgres    false    7    1872            /           0    0    base_PricingChange_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PricingChange_Id_seq" OWNED BY "base_PricingChange"."Id";
            public       postgres    false    1871            0           0    0    base_PricingChange_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingChange_Id_seq"', 533, true);
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
       public         postgres    false    2509    2510    2511    7            1           0    0 %   COLUMN "base_PricingManager"."Status"    COMMENT     u   COMMENT ON COLUMN "base_PricingManager"."Status" IS '- Pending
- Applied
- Restored

-> Get From PricingStatus Tag';
            public       postgres    false    1868            2           0    0 (   COLUMN "base_PricingManager"."BasePrice"    COMMENT     H   COMMENT ON COLUMN "base_PricingManager"."BasePrice" IS 'Cost or Price';
            public       postgres    false    1868            3           0    0 .   COLUMN "base_PricingManager"."CalculateMethod"    COMMENT     j   COMMENT ON COLUMN "base_PricingManager"."CalculateMethod" IS '+-*/
- Get from PricingAdjustmentType Tag';
            public       postgres    false    1868            4           0    0 )   COLUMN "base_PricingManager"."AmountUnit"    COMMENT     D   COMMENT ON COLUMN "base_PricingManager"."AmountUnit" IS '- % or $';
            public       postgres    false    1868            5           0    0 (   COLUMN "base_PricingManager"."ItemCount"    COMMENT     W   COMMENT ON COLUMN "base_PricingManager"."ItemCount" IS 'Tong so product duoc ap dung';
            public       postgres    false    1868            K           1259    268183    base_PricingManager_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_PricingManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_PricingManager_Id_seq";
       public       postgres    false    1868    7            6           0    0    base_PricingManager_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_PricingManager_Id_seq" OWNED BY "base_PricingManager"."Id";
            public       postgres    false    1867            7           0    0    base_PricingManager_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingManager_Id_seq"', 46, true);
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
       public         postgres    false    2328    2330    2331    2332    2333    2334    2335    2336    2337    2338    2339    2340    2341    7            8           0    0 &   COLUMN "base_Product"."QuantityOnHand"    COMMENT     b   COMMENT ON COLUMN "base_Product"."QuantityOnHand" IS 'Total From OnHandStore1 to OnHandStore 10';
            public       postgres    false    1808            9           0    0 '   COLUMN "base_Product"."QuantityOnOrder"    COMMENT     [   COMMENT ON COLUMN "base_Product"."QuantityOnOrder" IS 'Quantity on active purchase order';
            public       postgres    false    1808            :           0    0 $   COLUMN "base_Product"."RegularPrice"    COMMENT     I   COMMENT ON COLUMN "base_Product"."RegularPrice" IS 'Apply to Base Unit';
            public       postgres    false    1808            ;           0    0    COLUMN "base_Product"."Price1"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price1" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            <           0    0    COLUMN "base_Product"."Price2"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price2" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            =           0    0    COLUMN "base_Product"."Price3"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price3" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            >           0    0    COLUMN "base_Product"."Price4"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price4" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1808            ?           0    0 !   COLUMN "base_Product"."OrderCost"    COMMENT     F   COMMENT ON COLUMN "base_Product"."OrderCost" IS 'Apply to Base Unit';
            public       postgres    false    1808            @           0    0 '   COLUMN "base_Product"."AverageUnitCost"    COMMENT     L   COMMENT ON COLUMN "base_Product"."AverageUnitCost" IS 'Apply to Base Unit';
            public       postgres    false    1808            A           0    0    COLUMN "base_Product"."TaxCode"    COMMENT     D   COMMENT ON COLUMN "base_Product"."TaxCode" IS 'Apply to Base Unit';
            public       postgres    false    1808            B           0    0 %   COLUMN "base_Product"."MarginPercent"    COMMENT     q   COMMENT ON COLUMN "base_Product"."MarginPercent" IS 'Margin =100*(RegularPrice - AverageUnitCode)/RegularPrice';
            public       postgres    false    1808            C           0    0 %   COLUMN "base_Product"."MarkupPercent"    COMMENT     t   COMMENT ON COLUMN "base_Product"."MarkupPercent" IS 'Markup =100*(RegularPrice - AverageUnitCost)/AverageUnitCost';
            public       postgres    false    1808            D           0    0 "   COLUMN "base_Product"."IsOpenItem"    COMMENT     Q   COMMENT ON COLUMN "base_Product"."IsOpenItem" IS 'Can change price during sale';
            public       postgres    false    1808                       1259    255536    base_ProductStore    TABLE     �   CREATE TABLE "base_ProductStore" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL
);
 '   DROP TABLE public."base_ProductStore";
       public         postgres    false    2398    2400    7                       1259    255534    base_ProductStore_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ProductStore_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ProductStore_Id_seq";
       public       postgres    false    1821    7            E           0    0    base_ProductStore_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ProductStore_Id_seq" OWNED BY "base_ProductStore"."Id";
            public       postgres    false    1820            F           0    0    base_ProductStore_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_ProductStore_Id_seq"', 59, true);
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
       public         postgres    false    2565    2566    2567    2568    2569    2570    2571    2572    2573    2574    2575    2576    2577    2578    7            G           0    0    TABLE "base_ProductUOM"    COMMENT     B   COMMENT ON TABLE "base_ProductUOM" IS 'Use when allow multi UOM';
            public       postgres    false    1886            ]           1259    270250    base_ProductUOM_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_ProductUOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_ProductUOM_Id_seq";
       public       postgres    false    1886    7            H           0    0    base_ProductUOM_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_ProductUOM_Id_seq" OWNED BY "base_ProductUOM"."Id";
            public       postgres    false    1885            I           0    0    base_ProductUOM_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_ProductUOM_Id_seq"', 37, true);
            public       postgres    false    1885                       1259    245410    base_Product_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_Product_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_Product_Id_seq";
       public       postgres    false    7    1808            J           0    0    base_Product_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_Product_Id_seq" OWNED BY "base_Product"."Id";
            public       postgres    false    1807            K           0    0    base_Product_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Product_Id_seq"', 250191, true);
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
       public         postgres    false    2310    2311    2312    2313    2314    2315    7            L           0    0     COLUMN "base_Promotion"."Status"    COMMENT     U   COMMENT ON COLUMN "base_Promotion"."Status" IS '0: Deactived
1: Actived
2: Pending';
            public       postgres    false    1802            M           0    0 (   COLUMN "base_Promotion"."AffectDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Promotion"."AffectDiscount" IS '0: All items
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
       public       postgres    false    1800    7            N           0    0    base_PromotionAffect_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_PromotionAffect_Id_seq" OWNED BY "base_PromotionAffect"."Id";
            public       postgres    false    1799            O           0    0    base_PromotionAffect_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_PromotionAffect_Id_seq"', 609, true);
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
       public       postgres    false    1788    7            P           0    0    base_PromotionSchedule_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE "base_PromotionSchedule_Id_seq" OWNED BY "base_PromotionSchedule"."Id";
            public       postgres    false    1787            Q           0    0    base_PromotionSchedule_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_PromotionSchedule_Id_seq"', 53, true);
            public       postgres    false    1787            	           1259    245167    base_Promotion_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Promotion_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Promotion_Id_seq";
       public       postgres    false    7    1802            R           0    0    base_Promotion_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Promotion_Id_seq" OWNED BY "base_Promotion"."Id";
            public       postgres    false    1801            S           0    0    base_Promotion_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_Promotion_Id_seq"', 53, true);
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
    "PaymentName" character varying(30),
    "IsPurge" boolean DEFAULT false NOT NULL,
    "IsLocked" boolean DEFAULT false NOT NULL
);
 (   DROP TABLE public."base_PurchaseOrder";
       public         postgres    false    2485    2487    2488    2489    2490    2491    2492    2493    2494    2495    2496    2497    7            T           0    0 (   COLUMN "base_PurchaseOrder"."QtyOrdered"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyOrdered" IS 'Order Quantity: In the purchase order item list. Enter the quantity being ordered for the item.
';
            public       postgres    false    1860            U           0    0 $   COLUMN "base_PurchaseOrder"."QtyDue"    COMMENT     q   COMMENT ON COLUMN "base_PurchaseOrder"."QtyDue" IS 'Due Quantity: The item quantity remaining to be received.
';
            public       postgres    false    1860            V           0    0 )   COLUMN "base_PurchaseOrder"."QtyReceived"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyReceived" IS 'Received Quantity: The ordered item quantity already received on receiving vouchers.
';
            public       postgres    false    1860            W           0    0 &   COLUMN "base_PurchaseOrder"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_PurchaseOrder"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100

';
            public       postgres    false    1860            B           1259    266530    base_PurchaseOrderDetail    TABLE     c  CREATE TABLE "base_PurchaseOrderDetail" (
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
    "IsFullReceived" boolean DEFAULT false NOT NULL,
    "Discount" numeric(10,2) DEFAULT 0 NOT NULL
);
 .   DROP TABLE public."base_PurchaseOrderDetail";
       public         postgres    false    2475    2476    2477    2478    2480    2481    2482    2483    2484    7            X           0    0 *   COLUMN "base_PurchaseOrderDetail"."Amount"    COMMENT     S   COMMENT ON COLUMN "base_PurchaseOrderDetail"."Amount" IS 'Amount = Cost*Quantity';
            public       postgres    false    1858            A           1259    266528    base_PurchaseOrderDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_PurchaseOrderDetail_Id_seq";
       public       postgres    false    7    1858            Y           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_PurchaseOrderDetail_Id_seq" OWNED BY "base_PurchaseOrderDetail"."Id";
            public       postgres    false    1857            Z           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_PurchaseOrderDetail_Id_seq"', 122, true);
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
       public         postgres    false    2503    2504    2505    2507    7            [           0    0 *   COLUMN "base_PurchaseOrderReceive"."Price"    COMMENT     G   COMMENT ON COLUMN "base_PurchaseOrderReceive"."Price" IS 'Sale Price';
            public       postgres    false    1866            I           1259    267533     base_PurchaseOrderReceive_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderReceive_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_PurchaseOrderReceive_Id_seq";
       public       postgres    false    7    1866            \           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_PurchaseOrderReceive_Id_seq" OWNED BY "base_PurchaseOrderReceive"."Id";
            public       postgres    false    1865            ]           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_PurchaseOrderReceive_Id_seq"', 106, true);
            public       postgres    false    1865            C           1259    266549    base_PurchaseOrder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PurchaseOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PurchaseOrder_Id_seq";
       public       postgres    false    7    1860            ^           0    0    base_PurchaseOrder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PurchaseOrder_Id_seq" OWNED BY "base_PurchaseOrder"."Id";
            public       postgres    false    1859            _           0    0    base_PurchaseOrder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_PurchaseOrder_Id_seq"', 59, true);
            public       postgres    false    1859                       1259    245733    base_QuantityAdjustment    TABLE     �  CREATE TABLE "base_QuantityAdjustment" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "NewQuantity" integer DEFAULT 0 NOT NULL,
    "OldQuantity" integer DEFAULT 0 NOT NULL,
    "ItemCount" integer DEFAULT 0,
    "LoggedTime" timestamp without time zone NOT NULL,
    "Reason" character varying(30) NOT NULL,
    "StoreNumber" integer DEFAULT 0 NOT NULL
);
 -   DROP TABLE public."base_QuantityAdjustment";
       public         postgres    false    2342    2344    2345    2346    2347    2348    7            `           0    0 1   COLUMN "base_QuantityAdjustment"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustment"."CostDifference" IS 'if(QtyChanged) AverageUnitCost*(NewQty - OldQty) elseif(CostChanged) Quantity*(NewCost - OldCost)
';
            public       postgres    false    1810            a           0    0 ,   COLUMN "base_QuantityAdjustment"."ItemCount"    COMMENT     a   COMMENT ON COLUMN "base_QuantityAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1810            b           0    0 -   COLUMN "base_QuantityAdjustment"."LoggedTime"    COMMENT     {   COMMENT ON COLUMN "base_QuantityAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
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
       public         postgres    false    2349    2351    7            c           0    0 5   COLUMN "base_QuantityAdjustmentItem"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."CostDifference" IS '-- AverageUnitCost*OldQuantity - AverageUnitCost*NewQuantity';
            public       postgres    false    1812            d           0    0 8   COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff"    COMMENT     n   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff" IS 'AdjustmentNewQty - AdjustmentOldQty';
            public       postgres    false    1812                       1259    245743 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_QuantityAdjustmentItem_Id_seq";
       public       postgres    false    7    1812            e           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_QuantityAdjustmentItem_Id_seq" OWNED BY "base_QuantityAdjustmentItem"."Id";
            public       postgres    false    1811            f           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_QuantityAdjustmentItem_Id_seq"', 1, false);
            public       postgres    false    1811                       1259    245731    base_QuantityAdjustment_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_QuantityAdjustment_Id_seq";
       public       postgres    false    7    1810            g           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_QuantityAdjustment_Id_seq" OWNED BY "base_QuantityAdjustment"."Id";
            public       postgres    false    1809            h           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_QuantityAdjustment_Id_seq"', 1, false);
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
       public         postgres    false    2410    2411    2412    7            0           1259    256176    base_ResourceAccount_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourceAccount_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourceAccount_Id_seq";
       public       postgres    false    7    1841            i           0    0    base_ResourceAccount_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourceAccount_Id_seq" OWNED BY "base_ResourceAccount"."Id";
            public       postgres    false    1840            j           0    0    base_ResourceAccount_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceAccount_Id_seq"', 27, true);
            public       postgres    false    1840                       1259    246083    base_ResourceNote    TABLE     �   CREATE TABLE "base_ResourceNote" (
    "Id" bigint NOT NULL,
    "Note" character varying(300),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "Color" character(9),
    "Resource" character varying(36) NOT NULL
);
 '   DROP TABLE public."base_ResourceNote";
       public         postgres    false    2367    7                       1259    246081    base_ResourceNote_id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ResourceNote_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ResourceNote_id_seq";
       public       postgres    false    1818    7            k           0    0    base_ResourceNote_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ResourceNote_id_seq" OWNED BY "base_ResourceNote"."Id";
            public       postgres    false    1817            l           0    0    base_ResourceNote_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ResourceNote_id_seq"', 687, true);
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
       public         postgres    false    2541    2542    2544    2545    2546    2547    2548    2549    2550    2551    2552    7            m           0    0 $   COLUMN "base_ResourcePayment"."Mark"    COMMENT     <   COMMENT ON COLUMN "base_ResourcePayment"."Mark" IS 'SO/PO';
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
       public         postgres    false    2538    2539    2540    7            n           0    0 1   COLUMN "base_ResourcePaymentDetail"."PaymentType"    COMMENT     W   COMMENT ON COLUMN "base_ResourcePaymentDetail"."PaymentType" IS 'P:Payment
C:Correct';
            public       postgres    false    1880            o           0    0 ,   COLUMN "base_ResourcePaymentDetail"."Reason"    COMMENT     ^   COMMENT ON COLUMN "base_ResourcePaymentDetail"."Reason" IS 'Apply to Correct payment action';
            public       postgres    false    1880            W           1259    270070 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_ResourcePaymentDetail_Id_seq";
       public       postgres    false    7    1880            p           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_ResourcePaymentDetail_Id_seq" OWNED BY "base_ResourcePaymentDetail"."Id";
            public       postgres    false    1879            q           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentDetail_Id_seq"', 259, true);
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
       public         postgres    false    2593    2594    2595    7            f           1259    272120 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_ResourcePaymentProduct_Id_seq";
       public       postgres    false    1895    7            r           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_ResourcePaymentProduct_Id_seq" OWNED BY "base_ResourcePaymentProduct"."Id";
            public       postgres    false    1894            s           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentProduct_Id_seq"', 69, true);
            public       postgres    false    1894            Y           1259    270148    base_ResourcePayment_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourcePayment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourcePayment_Id_seq";
       public       postgres    false    1882    7            t           0    0    base_ResourcePayment_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourcePayment_Id_seq" OWNED BY "base_ResourcePayment"."Id";
            public       postgres    false    1881            u           0    0    base_ResourcePayment_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_ResourcePayment_Id_seq"', 184, true);
            public       postgres    false    1881            \           1259    270193    base_ResourceReturn    TABLE     
  CREATE TABLE "base_ResourceReturn" (
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
    "SubTotal" numeric(12,2) DEFAULT 0 NOT NULL,
    "ReturnFee" numeric(12,2) DEFAULT 0 NOT NULL
);
 )   DROP TABLE public."base_ResourceReturn";
       public         postgres    false    2553    2554    2556    2557    2558    2559    2560    2561    2562    2563    7            v           0    0 #   COLUMN "base_ResourceReturn"."Mark"    COMMENT     ;   COMMENT ON COLUMN "base_ResourceReturn"."Mark" IS 'SO/PO';
            public       postgres    false    1884            e           1259    272099    base_ResourceReturnDetail    TABLE     �  CREATE TABLE "base_ResourceReturnDetail" (
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
    "ReturnedDate" timestamp without time zone NOT NULL,
    "Discount" numeric(12,2) DEFAULT 0 NOT NULL
);
 /   DROP TABLE public."base_ResourceReturnDetail";
       public         postgres    false    2586    2588    2589    2590    2591    7            d           1259    272097     base_ResourceReturnDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourceReturnDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_ResourceReturnDetail_Id_seq";
       public       postgres    false    1893    7            w           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_ResourceReturnDetail_Id_seq" OWNED BY "base_ResourceReturnDetail"."Id";
            public       postgres    false    1892            x           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_ResourceReturnDetail_Id_seq"', 57, true);
            public       postgres    false    1892            [           1259    270191    base_ResourceReturn_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_ResourceReturn_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_ResourceReturn_Id_seq";
       public       postgres    false    1884    7            y           0    0    base_ResourceReturn_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_ResourceReturn_Id_seq" OWNED BY "base_ResourceReturn"."Id";
            public       postgres    false    1883            z           0    0    base_ResourceReturn_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceReturn_Id_seq"', 119, true);
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
       public         postgres    false    2500    2501    2502    7            G           1259    266841    base_RewardManager_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_RewardManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_RewardManager_Id_seq";
       public       postgres    false    1864    7            {           0    0    base_RewardManager_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_RewardManager_Id_seq" OWNED BY "base_RewardManager"."Id";
            public       postgres    false    1863            |           0    0    base_RewardManager_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_RewardManager_Id_seq"', 2, true);
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
       public       postgres    false    7    1862            }           0    0    base_SaleCommission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_SaleCommission_Id_seq" OWNED BY "base_SaleCommission"."Id";
            public       postgres    false    1861            ~           0    0    base_SaleCommission_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_SaleCommission_Id_seq"', 554, true);
            public       postgres    false    1861            :           1259    266093    base_SaleOrder    TABLE     
  CREATE TABLE "base_SaleOrder" (
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
    "IsRedeeem" boolean DEFAULT false NOT NULL,
    "IsPurge" boolean DEFAULT false NOT NULL,
    "IsHold" boolean DEFAULT false
);
 $   DROP TABLE public."base_SaleOrder";
       public         postgres    false    2430    2432    2433    2434    2435    2436    2437    2438    2439    2440    2441    2442    2443    2444    2445    2446    2447    2448    2449    2450    2451    2452    2453    2454    2455    2456    2457    2458    2459    2460    7            8           1259    266084    base_SaleOrderDetail    TABLE     9  CREATE TABLE "base_SaleOrderDetail" (
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
       public         postgres    false    2417    2418    2419    2420    2421    2422    2423    2424    2425    2427    2428    2429    7                       0    0 (   COLUMN "base_SaleOrderDetail"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_SaleOrderDetail"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100';
            public       postgres    false    1848            �           0    0 .   COLUMN "base_SaleOrderDetail"."SerialTracking"    COMMENT     Z   COMMENT ON COLUMN "base_SaleOrderDetail"."SerialTracking" IS 'Apply to Serial Tracking ';
            public       postgres    false    1848            �           0    0 .   COLUMN "base_SaleOrderDetail"."BalanceShipped"    COMMENT     s   COMMENT ON COLUMN "base_SaleOrderDetail"."BalanceShipped" IS 'Số lượng sản phẩm được vận chuyển';
            public       postgres    false    1848            7           1259    266082    base_SaleOrderDetail_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleOrderDetail_Id_seq";
       public       postgres    false    7    1848            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleOrderDetail_Id_seq" OWNED BY "base_SaleOrderDetail"."Id";
            public       postgres    false    1847            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderDetail_Id_seq"', 455, true);
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
       public         postgres    false    2465    2466    2467    2468    2469    2470    2471    2472    7            =           1259    266234    base_SaleOrderInvoice_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderInvoice_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_SaleOrderInvoice_Id_seq";
       public       postgres    false    1854    7            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_SaleOrderInvoice_Id_seq" OWNED BY "base_SaleOrderInvoice"."Id";
            public       postgres    false    1853            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderInvoice_Id_seq"', 1, false);
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
       public         postgres    false    2462    2463    7            @           1259    266357    base_SaleOrderShipDetail    TABLE     2  CREATE TABLE "base_SaleOrderShipDetail" (
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
       public         postgres    false    2474    7            ?           1259    266355    base_SaleOrderShipDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderShipDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_SaleOrderShipDetail_Id_seq";
       public       postgres    false    7    1856            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_SaleOrderShipDetail_Id_seq" OWNED BY "base_SaleOrderShipDetail"."Id";
            public       postgres    false    1855            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_SaleOrderShipDetail_Id_seq"', 302, true);
            public       postgres    false    1855            ;           1259    266178    base_SaleOrderShip_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_SaleOrderShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_SaleOrderShip_Id_seq";
       public       postgres    false    1852    7            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_SaleOrderShip_Id_seq" OWNED BY "base_SaleOrderShip"."Id";
            public       postgres    false    1851            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_SaleOrderShip_Id_seq"', 238, true);
            public       postgres    false    1851            9           1259    266091    base_SaleOrder_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_SaleOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_SaleOrder_Id_seq";
       public       postgres    false    1850    7            �           0    0    base_SaleOrder_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_SaleOrder_Id_seq" OWNED BY "base_SaleOrder"."Id";
            public       postgres    false    1849            �           0    0    base_SaleOrder_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_SaleOrder_Id_seq"', 278, true);
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
       public         postgres    false    2281    2283    2284    2285    2286    2287    7            �           0    0 )   COLUMN "base_SaleTaxLocation"."SortIndex"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocation"."SortIndex" IS 'ParentId ==0 -> Id"-"DateTime
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
       public         postgres    false    2280    7            �           0    0 .   COLUMN "base_SaleTaxLocationOption"."ParentId"    COMMENT     h   COMMENT ON COLUMN "base_SaleTaxLocationOption"."ParentId" IS 'Apply For Multi-rate has multi tax code';
            public       postgres    false    1790            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."TaxRate"    COMMENT     k   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxRate" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1790            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxComponent"    COMMENT     Y   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxComponent" IS 'Apply For Multi-rate';
            public       postgres    false    1790            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."TaxAgency"    COMMENT     m   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxAgency" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1790            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxCondition"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxCondition" IS 'Apply For Price-Depedent: Collect this tax on an item if the unit price or shiping is more than';
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
            public       postgres    false    1789            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_SaleTaxLocationOption_Id_seq"', 117, true);
            public       postgres    false    1789            �           1259    245101    base_SaleTaxLocation_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleTaxLocation_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleTaxLocation_Id_seq";
       public       postgres    false    1792    7            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleTaxLocation_Id_seq" OWNED BY "base_SaleTaxLocation"."Id";
            public       postgres    false    1791            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleTaxLocation_Id_seq"', 365, true);
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
       public         postgres    false    2524    2525    2526    2527    2528    2529    2530    2531    2532    7            V           1259    269941    base_TransferStockDetail    TABLE     |  CREATE TABLE "base_TransferStockDetail" (
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
       public         postgres    false    2534    2535    2536    7            U           1259    269939    base_TransferStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_TransferStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_TransferStockDetail_Id_seq";
       public       postgres    false    7    1878            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_TransferStockDetail_Id_seq" OWNED BY "base_TransferStockDetail"."Id";
            public       postgres    false    1877            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_TransferStockDetail_Id_seq"', 37, true);
            public       postgres    false    1877            S           1259    269923    base_TransferStock_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_TransferStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_TransferStock_Id_seq";
       public       postgres    false    1876    7            �           0    0    base_TransferStock_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_TransferStock_Id_seq" OWNED BY "base_TransferStock"."Id";
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
            public       postgres    false    1795            �           0    0    base_UserLog_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_UserLog_Id_seq"', 1987, true);
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
       public         postgres    false    2522    7            R           1259    269646    base_VendorProduct_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VendorProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VendorProduct_Id_seq";
       public       postgres    false    1873    7            �           0    0    base_VendorProduct_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VendorProduct_Id_seq" OWNED BY "base_VendorProduct"."Id";
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
       public       postgres    false    1794    7            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VirtualFolder_Id_seq" OWNED BY "base_VirtualFolder"."Id";
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
       public       postgres    false    7    1825            �           0    0    tims_Holiday_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_Holiday_Id_seq" OWNED BY "tims_Holiday"."Id";
            public       postgres    false    1824            �           0    0    tims_Holiday_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_Holiday_Id_seq"', 10, true);
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
       public       postgres    false    7    1834            �           0    0    tims_TimeLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_TimeLog_Id_seq" OWNED BY "tims_TimeLog"."Id";
            public       postgres    false    1833            �           0    0    tims_TimeLog_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_TimeLog_Id_seq"', 10, true);
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
       public       postgres    false    1832    7            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "tims_WorkPermission_Id_seq" OWNED BY "tims_WorkPermission"."Id";
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
       public       postgres    false    1828    7            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "tims_WorkSchedule_Id_seq" OWNED BY "tims_WorkSchedule"."Id";
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
       public       postgres    false    7    1830            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "tims_WorkWeek_Id_seq" OWNED BY "tims_WorkWeek"."Id";
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
       public       postgres    false    1784    1783    1784            h	           2604    256171    Id    DEFAULT     i   ALTER TABLE "base_Authorize" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Authorize_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Authorize" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1838    1839    1839            N	           2604    257304    Id    DEFAULT     q   ALTER TABLE "base_Configuration" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Configuration_Id_seq"'::regclass);
 H   ALTER TABLE public."base_Configuration" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1844    1819            0	           2604    245757    Id    DEFAULT     s   ALTER TABLE "base_CostAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustment_Id_seq"'::regclass);
 I   ALTER TABLE public."base_CostAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1814    1813    1814            8	           2604    245769    Id    DEFAULT     {   ALTER TABLE "base_CostAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustmentItem_Id_seq"'::regclass);
 M   ALTER TABLE public."base_CostAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1816    1815    1816            
           2604    271741    Id    DEFAULT     k   ALTER TABLE "base_CountStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStock_Id_seq"'::regclass);
 E   ALTER TABLE public."base_CountStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1888    1887    1888            
           2604    271748    Id    DEFAULT     w   ALTER TABLE "base_CountStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStockDetail_Id_seq"'::regclass);
 K   ALTER TABLE public."base_CountStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1890    1889    1890            	           2604    245343    Id    DEFAULT     k   ALTER TABLE "base_Department" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Department_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Department" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1803    1804    1804            �           2604    244820    Id    DEFAULT     a   ALTER TABLE "base_Guest" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Guest_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Guest" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1772    1771    1772            	           2604    245379    Id    DEFAULT     u   ALTER TABLE "base_GuestAdditional" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAdditional_Id_seq"'::regclass);
 J   ALTER TABLE public."base_GuestAdditional" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1806    1805    1806            �           2604    244866    Id    DEFAULT     o   ALTER TABLE "base_GuestAddress" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAddress_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestAddress" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1774    1773    1774            �           2604    238416    Id    DEFAULT     w   ALTER TABLE "base_GuestFingerPrint" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestFingerPrint_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestFingerPrint" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1768    1769    1769            �           2604    244876    Id    DEFAULT     {   ALTER TABLE "base_GuestHiringHistory" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestHiringHistory_Id_seq"'::regclass);
 M   ALTER TABLE public."base_GuestHiringHistory" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1775    1776    1776            �           2604    244887    Id    DEFAULT     o   ALTER TABLE "base_GuestPayRoll" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPayRoll_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestPayRoll" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1777    1778    1778            n	           2604    257328    Id    DEFAULT     w   ALTER TABLE "base_GuestPaymentCard" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPaymentCard_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestPaymentCard" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1846    1845    1846            �           2604    244937    Id    DEFAULT     o   ALTER TABLE "base_GuestProfile" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestProfile_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestProfile" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1781    1782    1782            �	           2604    268357    Id    DEFAULT     m   ALTER TABLE "base_GuestReward" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestReward_Id_seq"'::regclass);
 F   ALTER TABLE public."base_GuestReward" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1869    1870    1870            �           2604    245000    Id    DEFAULT     k   ALTER TABLE "base_MemberShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_MemberShip_Id_seq"'::regclass);
 E   ALTER TABLE public."base_MemberShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1786    1785    1786            �	           2604    268514    Id    DEFAULT     q   ALTER TABLE "base_PricingChange" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingChange_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PricingChange" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1872    1871    1872            �	           2604    268188    Id    DEFAULT     s   ALTER TABLE "base_PricingManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingManager_Id_seq"'::regclass);
 I   ALTER TABLE public."base_PricingManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1867    1868    1868            	           2604    245415    Id    DEFAULT     e   ALTER TABLE "base_Product" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Product_Id_seq"'::regclass);
 B   ALTER TABLE public."base_Product" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1808    1807    1808            _	           2604    255539    Id    DEFAULT     o   ALTER TABLE "base_ProductStore" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductStore_Id_seq"'::regclass);
 G   ALTER TABLE public."base_ProductStore" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1821    1820    1821            
           2604    270255    Id    DEFAULT     k   ALTER TABLE "base_ProductUOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductUOM_Id_seq"'::regclass);
 E   ALTER TABLE public."base_ProductUOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1885    1886    1886            	           2604    245172    Id    DEFAULT     i   ALTER TABLE "base_Promotion" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Promotion_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Promotion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1802    1801    1802            �           2604    245158    Id    DEFAULT     u   ALTER TABLE "base_PromotionAffect" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionAffect_Id_seq"'::regclass);
 J   ALTER TABLE public."base_PromotionAffect" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1800    1799    1800            �           2604    245026    Id    DEFAULT     y   ALTER TABLE "base_PromotionSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionSchedule_Id_seq"'::regclass);
 L   ALTER TABLE public."base_PromotionSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1788    1787    1788            �	           2604    266554    Id    DEFAULT     q   ALTER TABLE "base_PurchaseOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PurchaseOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1859    1860    1860            �	           2604    266533    Id    DEFAULT     }   ALTER TABLE "base_PurchaseOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_PurchaseOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1857    1858    1858            �	           2604    267538    Id    DEFAULT        ALTER TABLE "base_PurchaseOrderReceive" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderReceive_Id_seq"'::regclass);
 O   ALTER TABLE public."base_PurchaseOrderReceive" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1865    1866    1866            '	           2604    245736    Id    DEFAULT     {   ALTER TABLE "base_QuantityAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustment_Id_seq"'::regclass);
 M   ALTER TABLE public."base_QuantityAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1810    1809    1810            .	           2604    245748    Id    DEFAULT     �   ALTER TABLE "base_QuantityAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustmentItem_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_QuantityAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1812    1811    1812            i	           2604    256181    Id    DEFAULT     u   ALTER TABLE "base_ResourceAccount" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceAccount_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourceAccount" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1841    1840    1841            >	           2604    246086    Id    DEFAULT     o   ALTER TABLE "base_ResourceNote" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceNote_id_seq"'::regclass);
 G   ALTER TABLE public."base_ResourceNote" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1818    1817    1818            �	           2604    270153    Id    DEFAULT     u   ALTER TABLE "base_ResourcePayment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePayment_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourcePayment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1882    1881    1882            �	           2604    270075    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentDetail_Id_seq"'::regclass);
 P   ALTER TABLE public."base_ResourcePaymentDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1879    1880    1880             
           2604    272125    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentProduct_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_ResourcePaymentProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1894    1895    1895            �           2604    244925    Id    DEFAULT     n   ALTER TABLE "base_ResourcePhoto" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPhoto_Id_seq"'::regclass);
 H   ALTER TABLE public."base_ResourcePhoto" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1779    1780    1780            �	           2604    270196    Id    DEFAULT     s   ALTER TABLE "base_ResourceReturn" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturn_Id_seq"'::regclass);
 I   ALTER TABLE public."base_ResourceReturn" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1884    1883    1884            
           2604    272102    Id    DEFAULT        ALTER TABLE "base_ResourceReturnDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturnDetail_Id_seq"'::regclass);
 O   ALTER TABLE public."base_ResourceReturnDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1893    1892    1893            �	           2604    266846    Id    DEFAULT     q   ALTER TABLE "base_RewardManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_RewardManager_Id_seq"'::regclass);
 H   ALTER TABLE public."base_RewardManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1864    1863    1864            �	           2604    266609    Id    DEFAULT     s   ALTER TABLE "base_SaleCommission" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleCommission_Id_seq"'::regclass);
 I   ALTER TABLE public."base_SaleCommission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1861    1862    1862            	           2604    266096    Id    DEFAULT     i   ALTER TABLE "base_SaleOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrder_Id_seq"'::regclass);
 D   ALTER TABLE public."base_SaleOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1849    1850    1850            z	           2604    266087    Id    DEFAULT     u   ALTER TABLE "base_SaleOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderDetail_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1848    1847    1848            �	           2604    266239    Id    DEFAULT     w   ALTER TABLE "base_SaleOrderInvoice" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderInvoice_Id_seq"'::regclass);
 K   ALTER TABLE public."base_SaleOrderInvoice" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1853    1854    1854            �	           2604    266183    Id    DEFAULT     q   ALTER TABLE "base_SaleOrderShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShip_Id_seq"'::regclass);
 H   ALTER TABLE public."base_SaleOrderShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1851    1852    1852            �	           2604    266360    Id    DEFAULT     }   ALTER TABLE "base_SaleOrderShipDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShipDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_SaleOrderShipDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1855    1856    1856            �           2604    245106    Id    DEFAULT     u   ALTER TABLE "base_SaleTaxLocation" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocation_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleTaxLocation" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1792    1791    1792            �           2604    245087    Id    DEFAULT     �   ALTER TABLE "base_SaleTaxLocationOption" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocationOption_Id_seq"'::regclass);
 P   ALTER TABLE public."base_SaleTaxLocationOption" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1790    1789    1790            a	           2604    255678    Id    DEFAULT     a   ALTER TABLE "base_Store" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Store_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Store" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1823    1822    1823            �	           2604    269928    Id    DEFAULT     q   ALTER TABLE "base_TransferStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStock_Id_seq"'::regclass);
 H   ALTER TABLE public."base_TransferStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1876    1875    1876            �	           2604    269944    Id    DEFAULT     }   ALTER TABLE "base_TransferStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStockDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_TransferStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1878    1877    1878            �           2604    245150    Id    DEFAULT     ]   ALTER TABLE "base_UOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UOM_Id_seq"'::regclass);
 >   ALTER TABLE public."base_UOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1797    1798    1798            �           2604    245134    Id    DEFAULT     e   ALTER TABLE "base_UserLog" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserLog_Id_seq"'::regclass);
 B   ALTER TABLE public."base_UserLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1796    1795    1796            m	           2604    256247    Id    DEFAULT     i   ALTER TABLE "base_UserRight" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserRight_Id_seq"'::regclass);
 D   ALTER TABLE public."base_UserRight" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1843    1842    1843            �	           2604    269648    Id    DEFAULT     q   ALTER TABLE "base_VendorProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VendorProduct_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VendorProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1874    1873            �           2604    245118    Id    DEFAULT     q   ALTER TABLE "base_VirtualFolder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VirtualFolder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VirtualFolder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1794    1793    1794            b	           2604    255699    Id    DEFAULT     e   ALTER TABLE "tims_Holiday" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_Holiday_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_Holiday" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1824    1825    1825            f	           2604    255852    Id    DEFAULT     e   ALTER TABLE "tims_TimeLog" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_TimeLog_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_TimeLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1834    1833    1834            g	           2604    255868 	   TimeLogId    DEFAULT     �   ALTER TABLE "tims_TimeLogPermission" ALTER COLUMN "TimeLogId" SET DEFAULT nextval('"tims_TimeLogPermission_TimeLogId_seq"'::regclass);
 S   ALTER TABLE public."tims_TimeLogPermission" ALTER COLUMN "TimeLogId" DROP DEFAULT;
       public       postgres    false    1835    1836    1836            e	           2604    255798    Id    DEFAULT     s   ALTER TABLE "tims_WorkPermission" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkPermission_Id_seq"'::regclass);
 I   ALTER TABLE public."tims_WorkPermission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1832    1831    1832            c	           2604    255741    Id    DEFAULT     o   ALTER TABLE "tims_WorkSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkSchedule_Id_seq"'::regclass);
 G   ALTER TABLE public."tims_WorkSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1828    1827    1828            d	           2604    255784    Id    DEFAULT     g   ALTER TABLE "tims_WorkWeek" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkWeek_Id_seq"'::regclass);
 C   ALTER TABLE public."tims_WorkWeek" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1829    1830    1830            W          0    234865    pga_exception 
   TABLE DATA               B   COPY pga_exception (jexid, jexscid, jexdate, jextime) FROM stdin;
    pgagent       postgres    false    1751   ~�      X          0    234870    pga_job 
   TABLE DATA               �   COPY pga_job (jobid, jobjclid, jobname, jobdesc, jobhostagent, jobenabled, jobcreated, jobchanged, jobagentid, jobnextrun, joblastrun) FROM stdin;
    pgagent       postgres    false    1753   ��      Y          0    234883    pga_jobagent 
   TABLE DATA               A   COPY pga_jobagent (jagpid, jaglogintime, jagstation) FROM stdin;
    pgagent       postgres    false    1755   ��      Z          0    234890    pga_jobclass 
   TABLE DATA               /   COPY pga_jobclass (jclid, jclname) FROM stdin;
    pgagent       postgres    false    1756   ջ      [          0    234898 
   pga_joblog 
   TABLE DATA               P   COPY pga_joblog (jlgid, jlgjobid, jlgstatus, jlgstart, jlgduration) FROM stdin;
    pgagent       postgres    false    1758   =�      \          0    234906    pga_jobstep 
   TABLE DATA               �   COPY pga_jobstep (jstid, jstjobid, jstname, jstdesc, jstenabled, jstkind, jstcode, jstconnstr, jstdbname, jstonerror, jscnextrun) FROM stdin;
    pgagent       postgres    false    1760   Z�      ]          0    234923    pga_jobsteplog 
   TABLE DATA               t   COPY pga_jobsteplog (jslid, jsljlgid, jsljstid, jslstatus, jslresult, jslstart, jslduration, jsloutput) FROM stdin;
    pgagent       postgres    false    1762   w�      ^          0    234934    pga_schedule 
   TABLE DATA               �   COPY pga_schedule (jscid, jscjobid, jscname, jscdesc, jscenabled, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths) FROM stdin;
    pgagent       postgres    false    1764   ��      i          0    244946    base_Attachment 
   TABLE DATA               �   COPY "base_Attachment" ("Id", "FileOriginalName", "FileName", "FileExtension", "VirtualFolderId", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Counter") FROM stdin;
    public       postgres    false    1784   ��      �          0    256168    base_Authorize 
   TABLE DATA               =   COPY "base_Authorize" ("Id", "Resource", "Code") FROM stdin;
    public       postgres    false    1839   ��      {          0    254557    base_Configuration 
   TABLE DATA                 COPY "base_Configuration" ("CompanyName", "Address", "City", "State", "ZipCode", "CountryId", "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", "IsAllowRGO", "PasswordLength", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", "ReturnFeePercent") FROM stdin;
    public       postgres    false    1819   �      x          0    245754    base_CostAdjustment 
   TABLE DATA               �   COPY "base_CostAdjustment" ("Id", "Resource", "CostDifference", "NewCost", "OldCost", "ItemCount", "LoggedTime", "Reason", "StoreNumber") FROM stdin;
    public       postgres    false    1814   �      y          0    245766    base_CostAdjustmentItem 
   TABLE DATA               �   COPY "base_CostAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewCost", "AdjustmentOldCost", "LoggedTime", "ParentResource", "IsQuantityChanged") FROM stdin;
    public       postgres    false    1816   ;�      �          0    271738    base_CountStock 
   TABLE DATA               �   COPY "base_CountStock" ("Id", "DocumentNo", "DateCreated", "UserCreated", "CompletedDate", "UserCounted", "Status", "Resource") FROM stdin;
    public       postgres    false    1888   X�      �          0    271745    base_CountStockDetail 
   TABLE DATA               �   COPY "base_CountStockDetail" ("Id", "CountStockId", "ProductId", "ProductResource", "StoreId", "Quantity", "CountedQuantity") FROM stdin;
    public       postgres    false    1890   w�      s          0    245340    base_Department 
   TABLE DATA               �   COPY "base_Department" ("Id", "Name", "ParentId", "TaxCodeId", "Margin", "MarkUp", "LevelId", "IsActived", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated") FROM stdin;
    public       postgres    false    1804   t�      `          0    238237 
   base_Email 
   TABLE DATA               �  COPY "base_Email" ("Id", "Recipient", "CC", "BCC", "Subject", "Body", "IsHasAttachment", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "AttachmentType", "AttachmentResult", "GuestId", "Sender", "Status", "Importance", "Sensitivity", "IsRequestDelivery", "IsRequestRead", "IsMyFlag", "FlagTo", "FlagStartDate", "FlagDueDate", "IsAllowReminder", "RemindOn", "MyRemindTimes", "IsRecipentFlag", "RecipentFlagTo", "IsAllowRecipentReminder", "RecipentRemindOn", "RecipentRemindTimes") FROM stdin;
    public       postgres    false    1767   �      _          0    238137    base_EmailAttachment 
   TABLE DATA               J   COPY "base_EmailAttachment" ("Id", "EmailId", "AttachmentId") FROM stdin;
    public       postgres    false    1766   !�      c          0    244817 
   base_Guest 
   TABLE DATA                 COPY "base_Guest" ("Id", "FirstName", "MiddleName", "LastName", "Company", "Phone1", "Ext1", "Phone2", "Ext2", "Fax", "CellPhone", "Email", "Website", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "IsPurged", "GuestTypeId", "IsActived", "GuestNo", "PositionId", "Department", "Mark", "AccountNumber", "ParentId", "IsRewardMember", "CheckLimit", "CreditLimit", "BalanceDue", "AvailCredit", "PastDue", "IsPrimary", "CommissionPercent", "Resource", "TotalRewardRedeemed", "PurchaseDuringTrackingPeriod", "RequirePurchaseNextReward", "HireDate", "IsBlockArriveLate", "IsDeductLunchTime", "IsBalanceOvertime", "LateMinutes", "OvertimeOption", "OTLeastMinute", "IsTrackingHour", "TermDiscount", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "SaleRepId") FROM stdin;
    public       postgres    false    1772   >�      t          0    245376    base_GuestAdditional 
   TABLE DATA               3  COPY "base_GuestAdditional" ("Id", "TaxRate", "IsNoDiscount", "FixDiscount", "Unit", "PriceSchemeId", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Custom8", "GuestId", "LayawayNo", "ChargeACNo", "FedTaxId", "IsTaxExemption", "SaleTaxLocation", "TaxExemptionNo") FROM stdin;
    public       postgres    false    1806   �      d          0    244863    base_GuestAddress 
   TABLE DATA               �   COPY "base_GuestAddress" ("Id", "GuestId", "AddressTypeId", "AddressLine1", "AddressLine2", "City", "StateProvinceId", "PostalCode", "CountryId", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsDefault") FROM stdin;
    public       postgres    false    1774   ��      a          0    238413    base_GuestFingerPrint 
   TABLE DATA               �   COPY "base_GuestFingerPrint" ("Id", "GuestId", "FingerIndex", "HandFlag", "DateUpdated", "UserUpdaed", "FingerPrintImage") FROM stdin;
    public       postgres    false    1769   ��      e          0    244873    base_GuestHiringHistory 
   TABLE DATA               �   COPY "base_GuestHiringHistory" ("Id", "GuestId", "StartDate", "RenewDate", "PromotionDate", "TerminateDate", "IsTerminate", "ManagerId") FROM stdin;
    public       postgres    false    1776   1      f          0    244884    base_GuestPayRoll 
   TABLE DATA               �   COPY "base_GuestPayRoll" ("Id", "PayrollName", "PayrollType", "Rate", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "GuestId") FROM stdin;
    public       postgres    false    1778   N      �          0    257325    base_GuestPaymentCard 
   TABLE DATA               �   COPY "base_GuestPaymentCard" ("Id", "GuestId", "CardTypeId", "CardNumber", "ExpMonth", "ExpYear", "CCID", "BillingAddress", "NameOnCard", "ZipCode", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1846   k      h          0    244934    base_GuestProfile 
   TABLE DATA               s  COPY "base_GuestProfile" ("Id", "Gender", "Marital", "SSN", "Identification", "DOB", "IsSpouse", "FirstName", "LastName", "MiddleName", "State", "SGender", "SFirstName", "SLastName", "SMiddleName", "SPhone", "SCellPhone", "SSSN", "SState", "SEmail", "IsEmergency", "EFirstName", "ELastName", "EMiddleName", "EPhone", "ECellPhone", "ERelationship", "GuestId") FROM stdin;
    public       postgres    false    1782   �      �          0    268354    base_GuestReward 
   TABLE DATA               �   COPY "base_GuestReward" ("Id", "GuestId", "RewardId", "Amount", "IsApply", "EearnedDate", "RedeemedDate", "RewardValue", "SaleOrderResource", "SaleOrderNo", "Remark") FROM stdin;
    public       postgres    false    1870   
      �          0    256013    base_GuestSchedule 
   TABLE DATA               i   COPY "base_GuestSchedule" ("GuestId", "WorkScheduleId", "StartDate", "AssignDate", "Status") FROM stdin;
    public       postgres    false    1837   '      j          0    244997    base_MemberShip 
   TABLE DATA               �   COPY "base_MemberShip" ("Id", "GuestId", "MemberType", "CardNumber", "Status", "IsPurged", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "Code", "TotalRewardRedeemed") FROM stdin;
    public       postgres    false    1786   �      �          0    268511    base_PricingChange 
   TABLE DATA               �   COPY "base_PricingChange" ("Id", "PricingManagerId", "PricingManagerResource", "ProductId", "ProductResource", "Cost", "CurrentPrice", "NewPrice", "PriceChanged", "DateCreated") FROM stdin;
    public       postgres    false    1872   �      �          0    268185    base_PricingManager 
   TABLE DATA               +  COPY "base_PricingManager" ("Id", "Name", "Description", "DateCreated", "UserCreated", "DateApplied", "UserApplied", "DateRestored", "UserRestored", "AffectPricing", "Resource", "PriceLevel", "Status", "BasePrice", "CalculateMethod", "AmountChange", "AmountUnit", "ItemCount", "Reason") FROM stdin;
    public       postgres    false    1868         u          0    245412    base_Product 
   TABLE DATA               �  COPY "base_Product" ("Id", "Code", "ItemTypeId", "ProductDepartmentId", "ProductCategoryId", "ProductBrandId", "StyleModel", "ProductName", "Description", "Barcode", "Attribute", "Size", "IsSerialTracking", "IsPublicWeb", "OnHandStore1", "OnHandStore2", "OnHandStore3", "OnHandStore4", "OnHandStore5", "OnHandStore6", "OnHandStore7", "OnHandStore8", "OnHandStore9", "OnHandStore10", "QuantityOnHand", "QuantityOnOrder", "CompanyReOrderPoint", "IsUnOrderAble", "IsEligibleForCommission", "IsEligibleForReward", "RegularPrice", "Price1", "Price2", "Price3", "Price4", "OrderCost", "AverageUnitCost", "TaxCode", "MarginPercent", "MarkupPercent", "BaseUOMId", "GroupAttribute", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Resource", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "WarrantyType", "WarrantyNumber", "WarrantyPeriod", "PartNumber", "SellUOMId", "OrderUOMId", "IsPurge", "VendorId", "UserAssignedCommission", "AssignedCommissionPercent", "AssignedCommissionAmount", "Serial", "OrderUOM", "MarkdownPercent1", "MarkdownPercent2", "MarkdownPercent3", "MarkdownPercent4", "IsOpenItem", "Location") FROM stdin;
    public       postgres    false    1808   �      |          0    255536    base_ProductStore 
   TABLE DATA               X   COPY "base_ProductStore" ("Id", "ProductId", "QuantityOnHand", "StoreCode") FROM stdin;
    public       postgres    false    1821   �       �          0    270252    base_ProductUOM 
   TABLE DATA               /  COPY "base_ProductUOM" ("Id", "ProductStoreId", "ProductId", "UOMId", "BaseUnitNumber", "RegularPrice", "QuantityOnHand", "AverageCost", "Price1", "Price2", "Price3", "Price4", "MarkDownPercent1", "MarkDownPercent2", "MarkDownPercent3", "MarkDownPercent4", "MarginPercent", "MarkupPercent") FROM stdin;
    public       postgres    false    1886   �!      r          0    245169    base_Promotion 
   TABLE DATA               �  COPY "base_Promotion" ("Id", "Name", "Description", "PromotionTypeId", "TakeOffOption", "TakeOff", "BuyingQty", "GetingValue", "IsApplyToAboveQuantities", "Status", "AffectDiscount", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource", "CouponExpire", "IsCouponExpired", "PriceSchemaRange", "ReasonReActive", "Sold", "TotalPrice", "CategoryId", "VendorId", "CouponBarCode", "BarCodeNumber", "BarCodeImage") FROM stdin;
    public       postgres    false    1802   r#      q          0    245155    base_PromotionAffect 
   TABLE DATA               �   COPY "base_PromotionAffect" ("Id", "PromotionId", "ItemId", "Price1", "Price2", "Price3", "Price4", "Price5", "Discount1", "Discount2", "Discount3", "Discount4", "Discount5") FROM stdin;
    public       postgres    false    1800   d$      k          0    245023    base_PromotionSchedule 
   TABLE DATA               X   COPY "base_PromotionSchedule" ("Id", "PromotionId", "EndDate", "StartDate") FROM stdin;
    public       postgres    false    1788   �$      �          0    266551    base_PurchaseOrder 
   TABLE DATA               Y  COPY "base_PurchaseOrder" ("Id", "PurchaseOrderNo", "VendorId", "VendorCode", "Status", "ShipAddress", "PurchasedDate", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "PaymentDueDate", "PaymentMethodId", "Remark", "ShipDate", "SubTotal", "DiscountPercent", "DiscountAmount", "Freight", "Fee", "Total", "Paid", "Balance", "ItemCount", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "DateUpdate", "UserUpdated", "Resource", "CancelDate", "IsFullWorkflow", "StoreCode", "RecRemark", "PaymentName", "IsPurge", "IsLocked") FROM stdin;
    public       postgres    false    1860   �$      �          0    266530    base_PurchaseOrderDetail 
   TABLE DATA               ,  COPY "base_PurchaseOrderDetail" ("Id", "PurchaseOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "ReceivedQty", "DueQty", "UnFilledQty", "Amount", "Serial", "LastReceived", "Resource", "IsFullReceived", "Discount") FROM stdin;
    public       postgres    false    1858   v%      �          0    267535    base_PurchaseOrderReceive 
   TABLE DATA               �   COPY "base_PurchaseOrderReceive" ("Id", "PurchaseOrderDetailId", "POResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "RecQty", "IsReceived", "ReceiveDate", "Resource", "Price") FROM stdin;
    public       postgres    false    1866   Z&      v          0    245733    base_QuantityAdjustment 
   TABLE DATA               �   COPY "base_QuantityAdjustment" ("Id", "Resource", "CostDifference", "NewQuantity", "OldQuantity", "ItemCount", "LoggedTime", "Reason", "StoreNumber") FROM stdin;
    public       postgres    false    1810   N'      w          0    245745    base_QuantityAdjustmentItem 
   TABLE DATA               �   COPY "base_QuantityAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewQty", "AdjustmentOldQty", "AdjustmentQtyDiff", "LoggedTime", "ParentResource") FROM stdin;
    public       postgres    false    1812   k'      �          0    256178    base_ResourceAccount 
   TABLE DATA               �   COPY "base_ResourceAccount" ("Id", "Resource", "UserResource", "LoginName", "Password", "ExpiredDate", "IsLocked", "IsExpired") FROM stdin;
    public       postgres    false    1841   �'      z          0    246083    base_ResourceNote 
   TABLE DATA               X   COPY "base_ResourceNote" ("Id", "Note", "DateCreated", "Color", "Resource") FROM stdin;
    public       postgres    false    1818   X)      �          0    270150    base_ResourcePayment 
   TABLE DATA               (  COPY "base_ResourcePayment" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalPaid", "Balance", "Change", "DateCreated", "UserCreated", "Remark", "Resource", "SubTotal", "DiscountPercent", "DiscountAmount", "Mark", "IsDeposit", "TaxCode", "TaxAmount", "LastRewardAmount") FROM stdin;
    public       postgres    false    1882   �+      �          0    270072    base_ResourcePaymentDetail 
   TABLE DATA               �   COPY "base_ResourcePaymentDetail" ("Id", "PaymentType", "ResourcePaymentId", "PaymentMethodId", "PaymentMethod", "CardType", "Paid", "Change", "Tip", "GiftCardNo", "Reason", "Reference") FROM stdin;
    public       postgres    false    1880   �/      �          0    272122    base_ResourcePaymentProduct 
   TABLE DATA               �   COPY "base_ResourcePaymentProduct" ("Id", "ResourcePaymentId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "Amount") FROM stdin;
    public       postgres    false    1895   �0      g          0    244922    base_ResourcePhoto 
   TABLE DATA               �   COPY "base_ResourcePhoto" ("Id", "ThumbnailPhoto", "ThumbnailPhotoFilename", "LargePhoto", "LargePhotoFilename", "SortId", "Resource") FROM stdin;
    public       postgres    false    1780   N1      �          0    270193    base_ResourceReturn 
   TABLE DATA               �   COPY "base_ResourceReturn" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalRefund", "Balance", "DateCreated", "UserCreated", "Resource", "Mark", "DiscountPercent", "DiscountAmount", "Freight", "SubTotal", "ReturnFee") FROM stdin;
    public       postgres    false    1884   �7      �          0    272099    base_ResourceReturnDetail 
   TABLE DATA               �   COPY "base_ResourceReturnDetail" ("Id", "ResourceReturnId", "OrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Price", "ReturnQty", "Amount", "IsReturned", "ReturnedDate", "Discount") FROM stdin;
    public       postgres    false    1893   :      �          0    266843    base_RewardManager 
   TABLE DATA               �  COPY "base_RewardManager" ("Id", "StoreCode", "PurchaseThreshold", "RewardAmount", "RewardAmtType", "RewardExpiration", "IsAutoEnroll", "IsPromptEnroll", "IsInformCashier", "IsRedemptionLimit", "RedemptionLimitAmount", "IsBlockRedemption", "RedemptionAfterDays", "IsBlockPurchaseRedeem", "IsTrackingPeriod", "StartDate", "EndDate", "IsNoEndDay", "TotalRewardRedeemed", "IsActived", "ReasonReActive", "DateCreated") FROM stdin;
    public       postgres    false    1864   �:      �          0    266606    base_SaleCommission 
   TABLE DATA               �   COPY "base_SaleCommission" ("Id", "GuestResource", "SOResource", "SONumber", "SOTotal", "SODate", "ComissionPercent", "CommissionAmount", "Sign", "Remark") FROM stdin;
    public       postgres    false    1862   E;      �          0    266093    base_SaleOrder 
   TABLE DATA               L  COPY "base_SaleOrder" ("Id", "SONumber", "OrderDate", "OrderStatus", "BillAddressId", "BillAddress", "ShipAddressId", "ShipAddress", "PromotionCode", "SaleRep", "CustomerResource", "PriceSchemaId", "DueDate", "RequestShipDate", "SubTotal", "TaxLocation", "TaxCode", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "Paid", "Balance", "RefundFee", "IsMultiPayment", "Remark", "IsFullWorkflow", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "Resource", "BookingChanel", "ShippedCount", "Deposit", "Transaction", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "IsTaxExemption", "TaxExemption", "ShippedBox", "PackedQty", "TotalWeight", "WeightUnit", "StoreCode", "IsRedeeem", "IsPurge", "IsHold") FROM stdin;
    public       postgres    false    1850   B      �          0    266084    base_SaleOrderDetail 
   TABLE DATA               x  COPY "base_SaleOrderDetail" ("Id", "SaleOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "TaxCode", "Quantity", "PickQty", "DueQty", "UnFilled", "RegularPrice", "SalePrice", "UOMId", "BaseUOM", "DiscountPercent", "DiscountAmount", "SubTotal", "OnHandQty", "SerialTracking", "Resource", "BalanceShipped", "Comment", "TotalDiscount") FROM stdin;
    public       postgres    false    1848   TD      �          0    266236    base_SaleOrderInvoice 
   TABLE DATA               �   COPY "base_SaleOrderInvoice" ("Id", "InvoiceNo", "SaleOrderId", "SaleOrderResource", "ItemCount", "SubTotal", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "DateCreated") FROM stdin;
    public       postgres    false    1854   �E      �          0    266180    base_SaleOrderShip 
   TABLE DATA               �   COPY "base_SaleOrderShip" ("Id", "SaleOrderId", "SaleOrderResource", "Weight", "TrackingNo", "IsShipped", "Resource", "Remark", "Carrier", "ShipDate", "BoxNo") FROM stdin;
    public       postgres    false    1852   �E      �          0    266357    base_SaleOrderShipDetail 
   TABLE DATA               �   COPY "base_SaleOrderShipDetail" ("Id", "SaleOrderShipId", "SaleOrderShipResource", "SaleOrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Description", "SerialTracking", "PackedQty", "IsPaid") FROM stdin;
    public       postgres    false    1856   G      m          0    245103    base_SaleTaxLocation 
   TABLE DATA               �   COPY "base_SaleTaxLocation" ("Id", "ParentId", "Name", "IsShipingTaxable", "ShippingTaxCodeId", "IsActived", "LevelId", "TaxCode", "TaxCodeName", "TaxPrintMark", "TaxOption", "IsPrimary", "SortIndex", "IsTaxAfterDiscount") FROM stdin;
    public       postgres    false    1792   �H      l          0    245084    base_SaleTaxLocationOption 
   TABLE DATA               �   COPY "base_SaleTaxLocationOption" ("Id", "SaleTaxLocationId", "ParentId", "TaxRate", "TaxComponent", "TaxAgency", "TaxCondition", "IsApplyAmountOver", "IsAllowSpecificItemPriceRange", "IsAllowAmountItemPriceRange", "PriceFrom", "PriceTo") FROM stdin;
    public       postgres    false    1790   sI      }          0    255675 
   base_Store 
   TABLE DATA               G   COPY "base_Store" ("Id", "Code", "Name", "Street", "City") FROM stdin;
    public       postgres    false    1823   �I      �          0    269925    base_TransferStock 
   TABLE DATA                 COPY "base_TransferStock" ("Id", "TransferNo", "FromStore", "ToStore", "TotalQuantity", "ShipDate", "Carier", "ShippingFee", "Comment", "Resource", "UserCreated", "DateCreated", "Status", "SubTotal", "Total", "DateApplied", "UserApplied", "DateReversed", "UserReversed") FROM stdin;
    public       postgres    false    1876   VJ      �          0    269941    base_TransferStockDetail 
   TABLE DATA               �   COPY "base_TransferStockDetail" ("Id", "TransferStockId", "TransferStockResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Quantity", "UOMId", "BaseUOM", "Amount", "SerialTracking", "AvlQuantity") FROM stdin;
    public       postgres    false    1878   BN      p          0    245147    base_UOM 
   TABLE DATA               �   COPY "base_UOM" ("Id", "Code", "Name", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsActived", "Resource") FROM stdin;
    public       postgres    false    1798   UR      o          0    245131    base_UserLog 
   TABLE DATA               y   COPY "base_UserLog" ("Id", "IpSource", "ConnectedOn", "DisConnectedOn", "ResourceAccessed", "IsDisconected") FROM stdin;
    public       postgres    false    1796   �R      b          0    244282    base_UserLogDetail 
   TABLE DATA               m   COPY "base_UserLogDetail" ("Id", "UserLogId", "AccessedTime", "ModuleName", "ActionDescription") FROM stdin;
    public       postgres    false    1770   �S      �          0    256244    base_UserRight 
   TABLE DATA               9   COPY "base_UserRight" ("Id", "Code", "Name") FROM stdin;
    public       postgres    false    1843   c�      �          0    269643    base_VendorProduct 
   TABLE DATA               t   COPY "base_VendorProduct" ("Id", "ProductId", "VendorId", "Price", "ProductResource", "VendorResource") FROM stdin;
    public       postgres    false    1873   2�      n          0    245115    base_VirtualFolder 
   TABLE DATA               �   COPY "base_VirtualFolder" ("Id", "ParentFolderId", "FolderName", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource") FROM stdin;
    public       postgres    false    1794   ��      ~          0    255696    tims_Holiday 
   TABLE DATA               �   COPY "tims_Holiday" ("Id", "Title", "Description", "HolidayOption", "FromDate", "ToDate", "Month", "Day", "DayOfWeek", "WeekOfMonth", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedByID") FROM stdin;
    public       postgres    false    1825   N�                0    255705    tims_HolidayHistory 
   TABLE DATA               8   COPY "tims_HolidayHistory" ("Date", "Name") FROM stdin;
    public       postgres    false    1826    �      �          0    255849    tims_TimeLog 
   TABLE DATA               �  COPY "tims_TimeLog" ("Id", "EmployeeId", "WorkScheduleId", "PayrollId", "ClockIn", "ClockOut", "ManualClockInFlag", "ManualClockOutFlag", "WorkTime", "LunchTime", "OvertimeBefore", "Reason", "DeductLunchTimeFlag", "LateTime", "LeaveEarlyTime", "ActiveFlag", "ModifiedDate", "ModifiedById", "OvertimeAfter", "OvertimeLunch", "OvertimeDayOff", "OvertimeOptions", "GuestResource") FROM stdin;
    public       postgres    false    1834   ��      �          0    255865    tims_TimeLogPermission 
   TABLE DATA               L   COPY "tims_TimeLogPermission" ("TimeLogId", "WorkPermissionId") FROM stdin;
    public       postgres    false    1836   �      �          0    255795    tims_WorkPermission 
   TABLE DATA               �   COPY "tims_WorkPermission" ("Id", "EmployeeId", "PermissionType", "FromDate", "ToDate", "Note", "NoOfDays", "HourPerDay", "PaidFlag", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById", "OvertimeOptions") FROM stdin;
    public       postgres    false    1832   8�      �          0    255738    tims_WorkSchedule 
   TABLE DATA               �   COPY "tims_WorkSchedule" ("Id", "WorkScheduleName", "WorkScheduleType", "Rotate", "Status", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById") FROM stdin;
    public       postgres    false    1828   ��      �          0    255781    tims_WorkWeek 
   TABLE DATA               �   COPY "tims_WorkWeek" ("Id", "WorkScheduleId", "Week", "Day", "WorkIn", "WorkOut", "LunchOut", "LunchIn", "LunchBreakFlag") FROM stdin;
    public       postgres    false    1830   ��      '
           2606    235700    pga_exception_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_pkey PRIMARY KEY (jexid);
 K   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_pkey;
       pgagent         postgres    false    1751    1751            )
           2606    235702    pga_job_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_pkey PRIMARY KEY (jobid);
 ?   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_pkey;
       pgagent         postgres    false    1753    1753            +
           2606    235704    pga_jobagent_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY pga_jobagent
    ADD CONSTRAINT pga_jobagent_pkey PRIMARY KEY (jagpid);
 I   ALTER TABLE ONLY pgagent.pga_jobagent DROP CONSTRAINT pga_jobagent_pkey;
       pgagent         postgres    false    1755    1755            .
           2606    235706    pga_jobclass_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_jobclass
    ADD CONSTRAINT pga_jobclass_pkey PRIMARY KEY (jclid);
 I   ALTER TABLE ONLY pgagent.pga_jobclass DROP CONSTRAINT pga_jobclass_pkey;
       pgagent         postgres    false    1756    1756            1
           2606    235708    pga_joblog_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_pkey PRIMARY KEY (jlgid);
 E   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_pkey;
       pgagent         postgres    false    1758    1758            4
           2606    235710    pga_jobstep_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_pkey PRIMARY KEY (jstid);
 G   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_pkey;
       pgagent         postgres    false    1760    1760            7
           2606    235712    pga_jobsteplog_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_pkey PRIMARY KEY (jslid);
 M   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_pkey;
       pgagent         postgres    false    1762    1762            :
           2606    235714    pga_schedule_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_pkey PRIMARY KEY (jscid);
 I   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_pkey;
       pgagent         postgres    false    1764    1764            
           2606    245348    FK_base_Department_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_Id";
       public         postgres    false    1804    1804            �
           2606    256188    FPK_base_ResourceAccount_Id 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "FPK_base_ResourceAccount_Id" PRIMARY KEY ("Id", "Resource");
 ^   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "FPK_base_ResourceAccount_Id";
       public         postgres    false    1841    1841    1841            k
           2606    245266    PF_base_SaleTaxLocation 
   CONSTRAINT     i   ALTER TABLE ONLY "base_SaleTaxLocation"
    ADD CONSTRAINT "PF_base_SaleTaxLocation" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_SaleTaxLocation" DROP CONSTRAINT "PF_base_SaleTaxLocation";
       public         postgres    false    1792    1792            �
           2606    255762    PF_tims_Holiday_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "tims_Holiday"
    ADD CONSTRAINT "PF_tims_Holiday_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."tims_Holiday" DROP CONSTRAINT "PF_tims_Holiday_Id";
       public         postgres    false    1825    1825            �
           2606    245385    PK_GuestAdditional_Id 
   CONSTRAINT     g   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "PK_GuestAdditional_Id" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "PK_GuestAdditional_Id";
       public         postgres    false    1806    1806            E
           2606    244286    PK_UserLogDetail_Id 
   CONSTRAINT     c   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "PK_UserLogDetail_Id" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "PK_UserLogDetail_Id";
       public         postgres    false    1770    1770            q
           2606    245136    PK_UserLog_Id 
   CONSTRAINT     W   ALTER TABLE ONLY "base_UserLog"
    ADD CONSTRAINT "PK_UserLog_Id" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_UserLog" DROP CONSTRAINT "PK_UserLog_Id";
       public         postgres    false    1796    1796            _
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
       public         postgres    false    1814    1814                       2606    271757    PK_base_CounStockDetail_Id 
   CONSTRAINT     m   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "PK_base_CounStockDetail_Id" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "PK_base_CounStockDetail_Id";
       public         postgres    false    1890    1890                       2606    271755    PK_base_CounStock_Id 
   CONSTRAINT     a   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "PK_base_CounStock_Id" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "PK_base_CounStock_Id";
       public         postgres    false    1888    1888            <
           2606    238143    PK_base_EmailAttachment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "PK_base_EmailAttachment" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "PK_base_EmailAttachment";
       public         postgres    false    1766    1766            >
           2606    238253    PK_base_Email_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Email"
    ADD CONSTRAINT "PK_base_Email_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Email" DROP CONSTRAINT "PK_base_Email_Id";
       public         postgres    false    1767    1767            B
           2606    238418    PK_base_GuestFingerPrint_Id 
   CONSTRAINT     n   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "PK_base_GuestFingerPrint_Id" PRIMARY KEY ("Id");
 _   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "PK_base_GuestFingerPrint_Id";
       public         postgres    false    1769    1769            R
           2606    244879    PK_base_GuestHiringHistory_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "PK_base_GuestHiringHistory_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "PK_base_GuestHiringHistory_Id";
       public         postgres    false    1776    1776            W
           2606    244890    PK_base_GuestPayRoll_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "PK_base_GuestPayRoll_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "PK_base_GuestPayRoll_Id";
       public         postgres    false    1778    1778            \
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
       public         postgres    false    1837    1837    1837    1837            O
           2606    244869    PK_base_Guest_Id 
   CONSTRAINT     _   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "PK_base_Guest_Id" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "PK_base_Guest_Id";
       public         postgres    false    1774    1774            c
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
       public         postgres    false    1821    1821                       2606    270271    PK_base_ProductUOM_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "PK_base_ProductUOM_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "PK_base_ProductUOM_Id";
       public         postgres    false    1886    1886            �
           2606    255615    PK_base_Product_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "PK_base_Product_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "PK_base_Product_Id";
       public         postgres    false    1808    1808            y
           2606    245160    PK_base_PromotionAffect_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "PK_base_PromotionAffect_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "PK_base_PromotionAffect_Id";
       public         postgres    false    1800    1800            f
           2606    245030    PK_base_PromotionSchedule_Id 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "PK_base_PromotionSchedule_Id" PRIMARY KEY ("Id");
 a   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "PK_base_PromotionSchedule_Id";
       public         postgres    false    1788    1788            |
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
       public         postgres    false    1812    1812            �
           2606    245742    PK_base_QuantityAdjustment_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "PK_base_QuantityAdjustment_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "PK_base_QuantityAdjustment_Id";
       public         postgres    false    1810    1810            �
           2606    246089    PK_base_ResourceNote_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ResourceNote"
    ADD CONSTRAINT "PK_base_ResourceNote_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ResourceNote" DROP CONSTRAINT "PK_base_ResourceNote_Id";
       public         postgres    false    1818    1818                       2606    270163     PK_base_ResourcePaymentDetail_Id 
   CONSTRAINT     x   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "PK_base_ResourcePaymentDetail_Id" PRIMARY KEY ("Id");
 i   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "PK_base_ResourcePaymentDetail_Id";
       public         postgres    false    1880    1880                       2606    272130     PK_base_ResourcePaymentProductId 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "PK_base_ResourcePaymentProductId" PRIMARY KEY ("Id");
 j   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "PK_base_ResourcePaymentProductId";
       public         postgres    false    1895    1895                       2606    270161    PK_base_ResourcePayment_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_ResourcePayment"
    ADD CONSTRAINT "PK_base_ResourcePayment_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_ResourcePayment" DROP CONSTRAINT "PK_base_ResourcePayment_Id";
       public         postgres    false    1882    1882            Y
           2606    270190    PK_base_ResourcePhoto_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_ResourcePhoto"
    ADD CONSTRAINT "PK_base_ResourcePhoto_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_ResourcePhoto" DROP CONSTRAINT "PK_base_ResourcePhoto_Id";
       public         postgres    false    1780    1780                       2606    272108    PK_base_ResourceReturnDetail_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "PK_base_ResourceReturnDetail_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "PK_base_ResourceReturnDetail_Id";
       public         postgres    false    1893    1893            
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
       public         postgres    false    1850    1850            i
           2606    245268    PK_base_SaleTaxLocationOption 
   CONSTRAINT     u   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "PK_base_SaleTaxLocationOption" PRIMARY KEY ("Id");
 f   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "PK_base_SaleTaxLocationOption";
       public         postgres    false    1790    1790            �
           2606    255680    PK_base_Store_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "PK_base_Store_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "PK_base_Store_Id";
       public         postgres    false    1823    1823                       2606    269949    PK_base_TransferStockDetail_Id 
   CONSTRAINT     t   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "PK_base_TransferStockDetail_Id" PRIMARY KEY ("Id");
 e   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "PK_base_TransferStockDetail_Id";
       public         postgres    false    1878    1878            �
           2606    269936    PK_base_TransferStock_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "PK_base_TransferStock_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "PK_base_TransferStock_Id";
       public         postgres    false    1876    1876            s
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
       public         postgres    false    1873    1873            o
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
       public         postgres    false    1846    1846            I
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
       public         postgres    false    1816    1816                       2606    271770    uni_base_CountStock_Resource 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "uni_base_CountStock_Resource" UNIQUE ("Resource");
 Z   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "uni_base_CountStock_Resource";
       public         postgres    false    1888    1888            M
           2606    256327    uni_base_Guest_Resource 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "uni_base_Guest_Resource" UNIQUE ("Resource");
 P   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "uni_base_Guest_Resource";
       public         postgres    false    1772    1772            �
           2606    268201    uni_base_PricingManager 
   CONSTRAINT     i   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "uni_base_PricingManager" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "uni_base_PricingManager";
       public         postgres    false    1868    1868            �
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
       public         postgres    false    1841    1841                       2606    270205     uni_base_ResourceReturn_Resource 
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
       public         postgres    false    1876    1876            v
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
       public         postgres    false    1873    1873    1873            $
           1259    235939    pga_exception_datetime    INDEX     \   CREATE UNIQUE INDEX pga_exception_datetime ON pga_exception USING btree (jexdate, jextime);
 +   DROP INDEX pgagent.pga_exception_datetime;
       pgagent         postgres    false    1751    1751            %
           1259    235940    pga_exception_jexscid    INDEX     K   CREATE INDEX pga_exception_jexscid ON pga_exception USING btree (jexscid);
 *   DROP INDEX pgagent.pga_exception_jexscid;
       pgagent         postgres    false    1751            ,
           1259    235941    pga_jobclass_name    INDEX     M   CREATE UNIQUE INDEX pga_jobclass_name ON pga_jobclass USING btree (jclname);
 &   DROP INDEX pgagent.pga_jobclass_name;
       pgagent         postgres    false    1756            /
           1259    235942    pga_joblog_jobid    INDEX     D   CREATE INDEX pga_joblog_jobid ON pga_joblog USING btree (jlgjobid);
 %   DROP INDEX pgagent.pga_joblog_jobid;
       pgagent         postgres    false    1758            8
           1259    235943    pga_jobschedule_jobid    INDEX     K   CREATE INDEX pga_jobschedule_jobid ON pga_schedule USING btree (jscjobid);
 *   DROP INDEX pgagent.pga_jobschedule_jobid;
       pgagent         postgres    false    1764            2
           1259    235944    pga_jobstep_jobid    INDEX     F   CREATE INDEX pga_jobstep_jobid ON pga_jobstep USING btree (jstjobid);
 &   DROP INDEX pgagent.pga_jobstep_jobid;
       pgagent         postgres    false    1760            5
           1259    235945    pga_jobsteplog_jslid    INDEX     L   CREATE INDEX pga_jobsteplog_jslid ON pga_jobsteplog USING btree (jsljlgid);
 )   DROP INDEX pgagent.pga_jobsteplog_jslid;
       pgagent         postgres    false    1762            �
           1259    255547 .   FKI_baseProductStore_ProductId_base_Product_Id    INDEX     p   CREATE INDEX "FKI_baseProductStore_ProductId_base_Product_Id" ON "base_ProductStore" USING btree ("ProductId");
 D   DROP INDEX public."FKI_baseProductStore_ProductId_base_Product_Id";
       public         postgres    false    1821            w
           1259    245166 5   FKI_basePromotionAffect_PromotionId_base_Promotion_Id    INDEX     |   CREATE INDEX "FKI_basePromotionAffect_PromotionId_base_Promotion_Id" ON "base_PromotionAffect" USING btree ("PromotionId");
 K   DROP INDEX public."FKI_basePromotionAffect_PromotionId_base_Promotion_Id";
       public         postgres    false    1800            ]
           1259    246209 9   FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    INDEX        CREATE INDEX "FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" ON "base_Attachment" USING btree ("VirtualFolderId");
 O   DROP INDEX public."FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public         postgres    false    1784                       1259    271763 8   FKI_base_CounStockDetail_CountStockId_base_CountStock_id    INDEX     �   CREATE INDEX "FKI_base_CounStockDetail_CountStockId_base_CountStock_id" ON "base_CountStockDetail" USING btree ("CountStockId");
 N   DROP INDEX public."FKI_base_CounStockDetail_CountStockId_base_CountStock_id";
       public         postgres    false    1890            }
           1259    245354    FKI_base_Department_Id_ParentId    INDEX     ^   CREATE INDEX "FKI_base_Department_Id_ParentId" ON "base_Department" USING btree ("ParentId");
 5   DROP INDEX public."FKI_base_Department_Id_ParentId";
       public         postgres    false    1804            �
           1259    245391 &   FKI_base_GuestAdditional_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestAdditional_base_Guest_Id" ON "base_GuestAdditional" USING btree ("GuestId");
 <   DROP INDEX public."FKI_base_GuestAdditional_base_Guest_Id";
       public         postgres    false    1806            U
           1259    244891 +   FKI_base_GuestPayRoll_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestPayRoll_GuestId_base_Guest_Id" ON "base_GuestPayRoll" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestPayRoll_GuestId_base_Guest_Id";
       public         postgres    false    1778            Z
           1259    244942 +   FKI_base_GuestProfile_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestProfile_GuestId_base_Guest_Id" ON "base_GuestProfile" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestProfile_GuestId_base_Guest_Id";
       public         postgres    false    1782            �
           1259    268373 *   FKI_base_GuestReward_GuestId_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestReward_GuestId_base_Guest_Id" ON "base_GuestReward" USING btree ("GuestId");
 @   DROP INDEX public."FKI_base_GuestReward_GuestId_base_Guest_Id";
       public         postgres    false    1870            G
           1259    245510 %   FKI_base_Guest_ParentId_base_Guest_Id    INDEX     _   CREATE INDEX "FKI_base_Guest_ParentId_base_Guest_Id" ON "base_Guest" USING btree ("ParentId");
 ;   DROP INDEX public."FKI_base_Guest_ParentId_base_Guest_Id";
       public         postgres    false    1772            a
           1259    245006 )   FKI_base_MemberShip_GuestId_base_Guest_Id    INDEX     g   CREATE INDEX "FKI_base_MemberShip_GuestId_base_Guest_Id" ON "base_MemberShip" USING btree ("GuestId");
 ?   DROP INDEX public."FKI_base_MemberShip_GuestId_base_Guest_Id";
       public         postgres    false    1786            �
           1259    268532 >   FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id    INDEX     �   CREATE INDEX "FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id" ON "base_PricingChange" USING btree ("PricingManagerId");
 T   DROP INDEX public."FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public         postgres    false    1872                       1259    270282 .   FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id    INDEX     s   CREATE INDEX "FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id" ON "base_ProductUOM" USING btree ("BaseUnitNumber");
 D   DROP INDEX public."FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id";
       public         postgres    false    1886                       1259    270283 -   FKI_base_ProductUOM_ProductId_base_Product_Id    INDEX     m   CREATE INDEX "FKI_base_ProductUOM_ProductId_base_Product_Id" ON "base_ProductUOM" USING btree ("ProductId");
 C   DROP INDEX public."FKI_base_ProductUOM_ProductId_base_Product_Id";
       public         postgres    false    1886            d
           1259    245041 8   FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id    INDEX     �   CREATE INDEX "FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id" ON "base_PromotionSchedule" USING btree ("PromotionId");
 N   DROP INDEX public."FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public         postgres    false    1788            z
           1259    245178 8   FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id    INDEX     }   CREATE INDEX "FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id" ON "base_Promotion" USING btree ("PromotionTypeId");
 N   DROP INDEX public."FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id";
       public         postgres    false    1802            �
           1259    266544 ?   FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder" ON "base_PurchaseOrderDetail" USING btree ("PurchaseOrderId");
 U   DROP INDEX public."FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder";
       public         postgres    false    1858            �
           1259    267550 ?   FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha" ON "base_PurchaseOrderReceive" USING btree ("PurchaseOrderDetailId");
 U   DROP INDEX public."FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha";
       public         postgres    false    1866                       1259    272136 ?   FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource    INDEX     �   CREATE INDEX "FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource" ON "base_ResourcePaymentProduct" USING btree ("ResourcePaymentId");
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
       public         postgres    false    1852            g
           1259    245099 1   FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id    INDEX     �   CREATE INDEX "FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id" ON "base_SaleTaxLocationOption" USING btree ("SaleTaxLocationId");
 G   DROP INDEX public."FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id";
       public         postgres    false    1790                        1259    269955 ?   FKI_base_TransferStockDetail_TransferStockId_base_TransferStock    INDEX     �   CREATE INDEX "FKI_base_TransferStockDetail_TransferStockId_base_TransferStock" ON "base_TransferStockDetail" USING btree ("TransferStockId");
 U   DROP INDEX public."FKI_base_TransferStockDetail_TransferStockId_base_TransferStock";
       public         postgres    false    1878            �
           1259    269666 .   FKI_base_VendorProduct_ProductId_base_Guest_Id    INDEX     q   CREATE INDEX "FKI_base_VendorProduct_ProductId_base_Guest_Id" ON "base_VendorProduct" USING btree ("ProductId");
 D   DROP INDEX public."FKI_base_VendorProduct_ProductId_base_Guest_Id";
       public         postgres    false    1873            �
           1259    256148 0   FKI_tims_WorkPermission_EmployeeId_base_Guest_Id    INDEX     u   CREATE INDEX "FKI_tims_WorkPermission_EmployeeId_base_Guest_Id" ON "tims_WorkPermission" USING btree ("EmployeeId");
 F   DROP INDEX public."FKI_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public         postgres    false    1832            C
           1259    244035    idx_GuestFingerPrint_GuestId    INDEX     `   CREATE INDEX "idx_GuestFingerPrint_GuestId" ON "base_GuestFingerPrint" USING btree ("GuestId");
 2   DROP INDEX public."idx_GuestFingerPrint_GuestId";
       public         postgres    false    1769            J
           1259    244839    idx_GuestName    INDEX     _   CREATE INDEX "idx_GuestName" ON "base_Guest" USING btree ("FirstName", "LastName", "Company");
 #   DROP INDEX public."idx_GuestName";
       public         postgres    false    1772    1772    1772            F
           1259    244292    idx_UserLogDetail    INDEX     T   CREATE INDEX "idx_UserLogDetail" ON "base_UserLogDetail" USING btree ("UserLogId");
 '   DROP INDEX public."idx_UserLogDetail";
       public         postgres    false    1770            `
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
       public         postgres    false    1814            �
           1259    255517    idx_base_Department_Id    INDEX     W   CREATE INDEX "idx_base_Department_Id" ON "base_Department" USING btree ("Id", "Name");
 ,   DROP INDEX public."idx_base_Department_Id";
       public         postgres    false    1804    1804            ?
           1259    238254    idx_base_Email    INDEX     B   CREATE INDEX "idx_base_Email" ON "base_Email" USING btree ("Id");
 $   DROP INDEX public."idx_base_Email";
       public         postgres    false    1767            @
           1259    238260    idx_base_Email_Address    INDEX     N   CREATE INDEX "idx_base_Email_Address" ON "base_Email" USING btree ("Sender");
 ,   DROP INDEX public."idx_base_Email_Address";
       public         postgres    false    1767            P
           1259    244870    idx_base_GuestAddress_Id    INDEX     S   CREATE INDEX "idx_base_GuestAddress_Id" ON "base_GuestAddress" USING btree ("Id");
 .   DROP INDEX public."idx_base_GuestAddress_Id";
       public         postgres    false    1774            S
           1259    244880     idx_base_GuestHiringHistory_Date    INDEX     �   CREATE INDEX "idx_base_GuestHiringHistory_Date" ON "base_GuestHiringHistory" USING btree ("StartDate", "RenewDate", "PromotionDate");
 6   DROP INDEX public."idx_base_GuestHiringHistory_Date";
       public         postgres    false    1776    1776    1776            T
           1259    244881    idx_base_GuestHiringHistory_Id    INDEX     d   CREATE INDEX "idx_base_GuestHiringHistory_Id" ON "base_GuestHiringHistory" USING btree ("GuestId");
 4   DROP INDEX public."idx_base_GuestHiringHistory_Id";
       public         postgres    false    1776            �
           1259    257338 !   idx_base_GuestPaymentCard_GuestId    INDEX     e   CREATE INDEX "idx_base_GuestPaymentCard_GuestId" ON "base_GuestPaymentCard" USING btree ("GuestId");
 7   DROP INDEX public."idx_base_GuestPaymentCard_GuestId";
       public         postgres    false    1846            K
           1259    256328    idx_base_Guest_Resource    INDEX     Q   CREATE INDEX "idx_base_Guest_Resource" ON "base_Guest" USING btree ("Resource");
 -   DROP INDEX public."idx_base_Guest_Resource";
       public         postgres    false    1772            �
           1259    257571    idx_base_Product_Code    INDEX     M   CREATE INDEX "idx_base_Product_Code" ON "base_Product" USING btree ("Code");
 +   DROP INDEX public."idx_base_Product_Code";
       public         postgres    false    1808            �
           1259    245794    idx_base_Product_Id    INDEX     I   CREATE INDEX "idx_base_Product_Id" ON "base_Product" USING btree ("Id");
 )   DROP INDEX public."idx_base_Product_Id";
       public         postgres    false    1808            �
           1259    254639    idx_base_Product_Name    INDEX     c   CREATE INDEX "idx_base_Product_Name" ON "base_Product" USING btree ("ProductName", "Description");
 +   DROP INDEX public."idx_base_Product_Name";
       public         postgres    false    1808    1808            �
           1259    271771    idx_base_Product_Resource    INDEX     U   CREATE INDEX "idx_base_Product_Resource" ON "base_Product" USING btree ("Resource");
 /   DROP INDEX public."idx_base_Product_Resource";
       public         postgres    false    1808            �
           1259    245793    idx_base_QuantityAdjustment    INDEX     b   CREATE INDEX "idx_base_QuantityAdjustment" ON "base_QuantityAdjustment" USING btree ("Resource");
 1   DROP INDEX public."idx_base_QuantityAdjustment";
       public         postgres    false    1810            �
           1259    256315 !   idx_base_ResourceAccount_Resource    INDEX     u   CREATE INDEX "idx_base_ResourceAccount_Resource" ON "base_ResourceAccount" USING btree ("Resource", "UserResource");
 7   DROP INDEX public."idx_base_ResourceAccount_Resource";
       public         postgres    false    1841    1841                       1259    270298 ,   idx_base_ResourcePayment_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourcePayment_DocumentResource_No" ON "base_ResourcePayment" USING btree ("DocumentNo", "DocumentResource");
 B   DROP INDEX public."idx_base_ResourcePayment_DocumentResource_No";
       public         postgres    false    1882    1882                       1259    270208    idx_base_ResourcePayment_Id    INDEX     Y   CREATE INDEX "idx_base_ResourcePayment_Id" ON "base_ResourcePayment" USING btree ("Id");
 1   DROP INDEX public."idx_base_ResourcePayment_Id";
       public         postgres    false    1882                       1259    271706 +   idx_base_ResourceReturn_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourceReturn_DocumentResource_No" ON "base_ResourceReturn" USING btree ("DocumentNo", "DocumentResource");
 A   DROP INDEX public."idx_base_ResourceReturn_DocumentResource_No";
       public         postgres    false    1884    1884            �
           1259    266266    idx_base_SaleOrder_Resource    INDEX     Y   CREATE INDEX "idx_base_SaleOrder_Resource" ON "base_SaleOrder" USING btree ("Resource");
 1   DROP INDEX public."idx_base_SaleOrder_Resource";
       public         postgres    false    1850            l
           1259    245314    idx_base_SaleTaxLocation_Id    INDEX     Y   CREATE INDEX "idx_base_SaleTaxLocation_Id" ON "base_SaleTaxLocation" USING btree ("Id");
 1   DROP INDEX public."idx_base_SaleTaxLocation_Id";
       public         postgres    false    1792            m
           1259    245313     idx_base_SaleTaxLocation_TaxCode    INDEX     c   CREATE INDEX "idx_base_SaleTaxLocation_TaxCode" ON "base_SaleTaxLocation" USING btree ("TaxCode");
 6   DROP INDEX public."idx_base_SaleTaxLocation_TaxCode";
       public         postgres    false    1792            t
           1259    245807    idx_base_UOM_Id    INDEX     A   CREATE INDEX "idx_base_UOM_Id" ON "base_UOM" USING btree ("Id");
 %   DROP INDEX public."idx_base_UOM_Id";
       public         postgres    false    1798            �
           1259    256314    idx_base_UserRight_Code    INDEX     Q   CREATE INDEX "idx_base_UserRight_Code" ON "base_UserRight" USING btree ("Code");
 -   DROP INDEX public."idx_base_UserRight_Code";
       public         postgres    false    1843            �
           1259    255787    idx_tims_WorkWeek_ScheduleId    INDEX     _   CREATE INDEX "idx_tims_WorkWeek_ScheduleId" ON "tims_WorkWeek" USING btree ("WorkScheduleId");
 2   DROP INDEX public."idx_tims_WorkWeek_ScheduleId";
       public         postgres    false    1830            T           2620    235953    pga_exception_trigger    TRIGGER     �   CREATE TRIGGER pga_exception_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_exception FOR EACH ROW EXECUTE PROCEDURE pga_exception_trigger();
 =   DROP TRIGGER pga_exception_trigger ON pgagent.pga_exception;
       pgagent       postgres    false    1751    19            �           0    0 .   TRIGGER pga_exception_trigger ON pga_exception    COMMENT     ~   COMMENT ON TRIGGER pga_exception_trigger ON pga_exception IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    2900            U           2620    235954    pga_job_trigger    TRIGGER     j   CREATE TRIGGER pga_job_trigger BEFORE UPDATE ON pga_job FOR EACH ROW EXECUTE PROCEDURE pga_job_trigger();
 1   DROP TRIGGER pga_job_trigger ON pgagent.pga_job;
       pgagent       postgres    false    1753    21            �           0    0 "   TRIGGER pga_job_trigger ON pga_job    COMMENT     U   COMMENT ON TRIGGER pga_job_trigger ON pga_job IS 'Update the job''s next run time.';
            pgagent       postgres    false    2901            V           2620    235955    pga_schedule_trigger    TRIGGER     �   CREATE TRIGGER pga_schedule_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_schedule FOR EACH ROW EXECUTE PROCEDURE pga_schedule_trigger();
 ;   DROP TRIGGER pga_schedule_trigger ON pgagent.pga_schedule;
       pgagent       postgres    false    1764    23            �           0    0 ,   TRIGGER pga_schedule_trigger ON pga_schedule    COMMENT     z   COMMENT ON TRIGGER pga_schedule_trigger ON pga_schedule IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    2902                       2606    235956    pga_exception_jexscid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_jexscid_fkey FOREIGN KEY (jexscid) REFERENCES pga_schedule(jscid) ON UPDATE RESTRICT ON DELETE CASCADE;
 S   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_jexscid_fkey;
       pgagent       postgres    false    1764    1751    2617                       2606    235961    pga_job_jobagentid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobagentid_fkey FOREIGN KEY (jobagentid) REFERENCES pga_jobagent(jagpid) ON UPDATE RESTRICT ON DELETE SET NULL;
 J   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobagentid_fkey;
       pgagent       postgres    false    2602    1753    1755                        2606    235966    pga_job_jobjclid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobjclid_fkey FOREIGN KEY (jobjclid) REFERENCES pga_jobclass(jclid) ON UPDATE RESTRICT ON DELETE RESTRICT;
 H   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobjclid_fkey;
       pgagent       postgres    false    2605    1753    1756            !           2606    235971    pga_joblog_jlgjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_jlgjobid_fkey FOREIGN KEY (jlgjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 N   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_jlgjobid_fkey;
       pgagent       postgres    false    2600    1753    1758            "           2606    235976    pga_jobstep_jstjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_jstjobid_fkey FOREIGN KEY (jstjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 P   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_jstjobid_fkey;
       pgagent       postgres    false    2600    1753    1760            #           2606    235981    pga_jobsteplog_jsljlgid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljlgid_fkey FOREIGN KEY (jsljlgid) REFERENCES pga_joblog(jlgid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljlgid_fkey;
       pgagent       postgres    false    2608    1758    1762            $           2606    235986    pga_jobsteplog_jsljstid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljstid_fkey FOREIGN KEY (jsljstid) REFERENCES pga_jobstep(jstid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljstid_fkey;
       pgagent       postgres    false    1760    1762    2611            %           2606    235991    pga_schedule_jscjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_jscjobid_fkey FOREIGN KEY (jscjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 R   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_jscjobid_fkey;
       pgagent       postgres    false    1753    2600    1764            6           2606    255621 -   FK_baseProductStore_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id";
       public       postgres    false    1808    2692    1821            .           2606    246204 8   FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" FOREIGN KEY ("VirtualFolderId") REFERENCES "base_VirtualFolder"("Id");
 v   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public       postgres    false    2670    1794    1784            Q           2606    271772 7   FK_base_CounStockDetail_CountStockId_base_CountStock_id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id" FOREIGN KEY ("CountStockId") REFERENCES "base_CountStock"("Id");
 {   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id";
       public       postgres    false    2834    1888    1890            4           2606    245349 .   FK_base_Department_ParentId_base_Department_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_ParentId_base_Department_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Department"("Id");
 l   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_ParentId_base_Department_Id";
       public       postgres    false    2686    1804    1804            &           2606    238255 -   FK_base_EmailAttachment_EmailId_base_Email_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id" FOREIGN KEY ("EmailId") REFERENCES "base_Email"("Id");
 p   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id";
       public       postgres    false    1766    2621    1767            5           2606    256202 %   FK_base_GuestAdditional_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id";
       public       postgres    false    1772    1806    2632            *           2606    256207 "   FK_base_GuestAddress_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "FK_base_GuestAddress_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "FK_base_GuestAddress_base_Guest_Id";
       public       postgres    false    2632    1774    1772            '           2606    256212 .   FK_base_GuestFingerPrint_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id";
       public       postgres    false    2632    1772    1769            +           2606    256217 0   FK_base_GuestHiringHistory_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id";
       public       postgres    false    2632    1772    1776            ,           2606    256222 *   FK_base_GuestPayRoll_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id";
       public       postgres    false    2632    1778    1772            ?           2606    257333 .   FK_base_GuestPaymentCard_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id";
       public       postgres    false    1846    1772    2632            -           2606    256197 *   FK_base_GuestProfile_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id";
       public       postgres    false    1782    2632    1772            F           2606    268363 )   FK_base_GuestReward_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id";
       public       postgres    false    1870    2632    1772            G           2606    268368 2   FK_base_GuestReward_RewardId_base_RewardManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id" FOREIGN KEY ("RewardId") REFERENCES "base_RewardManager"("Id");
 q   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id";
       public       postgres    false    1864    2792    1870            >           2606    256031 +   FK_base_GuestSchedule_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id";
       public       postgres    false    1837    1772    2632            =           2606    256023 9   FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1837    1828    2733            )           2606    245511 $   FK_base_Guest_ParentId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Guest"("Id");
 ]   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id";
       public       postgres    false    2632    1772    1772            /           2606    245230 (   FK_base_MemberShip_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id");
 f   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id";
       public       postgres    false    2632    1772    1786            H           2606    268533 =   FK_base_PricingChange_PricingManagerId_base_PricingManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id" FOREIGN KEY ("PricingManagerId") REFERENCES "base_PricingManager"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public       postgres    false    1872    2797    1868            I           2606    268526 /   FK_base_PricingChange_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id";
       public       postgres    false    1872    1808    2692            P           2606    270272 ,   FK_base_ProductUOM_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 j   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductId_base_Product_Id";
       public       postgres    false    2692    1808    1886            N           2606    270285 6   FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id" FOREIGN KEY ("ProductStoreId") REFERENCES "base_ProductStore"("Id");
 t   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id";
       public       postgres    false    2723    1821    1886            O           2606    270277 $   FK_base_ProductUOM_UOMId_base_UOM_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id" FOREIGN KEY ("UOMId") REFERENCES "base_UOM"("Id");
 b   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id";
       public       postgres    false    2674    1886    1798            3           2606    245248 5   FK_base_PromotionAffect_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 x   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id";
       public       postgres    false    2683    1800    1802            0           2606    245253 7   FK_base_PromotionSchedule_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 |   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public       postgres    false    2683    1788    1802            D           2606    266570 ?   FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_" FOREIGN KEY ("PurchaseOrderId") REFERENCES "base_PurchaseOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_";
       public       postgres    false    2786    1858    1860            E           2606    267545 ?   FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas" FOREIGN KEY ("PurchaseOrderDetailId") REFERENCES "base_PurchaseOrderDetail"("Id");
 �   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas";
       public       postgres    false    1866    2784    1858            M           2606    270170 ?   FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id");
 �   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa";
       public       postgres    false    1880    2821    1882            S           2606    272137 ?   FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP";
       public       postgres    false    2821    1895    1882            R           2606    272109 ?   FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu" FOREIGN KEY ("ResourceReturnId") REFERENCES "base_ResourceReturn"("Id");
 �   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu";
       public       postgres    false    1893    1884    2825            @           2606    266129 5   FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2769    1850    1848            B           2606    266260 6   FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1850    2769    1854            C           2606    266363 ?   FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_" FOREIGN KEY ("SaleOrderShipId") REFERENCES "base_SaleOrderShip"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_";
       public       postgres    false    1852    2773    1856            A           2606    266222 3   FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1850    2769    1852            L           2606    270034 ?   FK_base_TransferStockDetail_TransferStockId_base_TransferStock_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_" FOREIGN KEY ("TransferStockId") REFERENCES "base_TransferStock"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_";
       public       postgres    false    1876    2812    1878            (           2606    266390 /   FK_base_UserLogDetail_UserLogId_base_UserLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id" FOREIGN KEY ("UserLogId") REFERENCES "base_UserLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id";
       public       postgres    false    1796    2672    1770            K           2606    270029 /   FK_base_VendorProduct_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 p   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id";
       public       postgres    false    1873    1808    2692            J           2606    269667 ,   FK_base_VendorProduct_VendorId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id" FOREIGN KEY ("VendorId") REFERENCES "base_Guest"("Id");
 m   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id";
       public       postgres    false    1772    2632    1873            2           2606    245123 9   FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId" FOREIGN KEY ("ParentFolderId") REFERENCES "base_VirtualFolder"("Id");
 z   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId";
       public       postgres    false    1794    2670    1794            :           2606    256119 (   FK_tims_TimeLog_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 c   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id";
       public       postgres    false    2632    1834    1772            9           2606    255858 3   FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1828    2733    1834            ;           2606    255871 3   FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id" FOREIGN KEY ("TimeLogId") REFERENCES "tims_TimeLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id";
       public       postgres    false    1834    1836    2741            <           2606    255876 >   FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission" FOREIGN KEY ("WorkPermissionId") REFERENCES "tims_WorkPermission"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission";
       public       postgres    false    1832    1836    2739            8           2606    256143 /   FK_tims_WorkPermission_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id");
 q   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public       postgres    false    1772    1832    2632            7           2606    255788 4   FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2733    1830    1828            1           2606    245269 ?   base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati" FOREIGN KEY ("SaleTaxLocationId") REFERENCES "base_SaleTaxLocation"("Id");
 �   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati";
       public       postgres    false    1792    2666    1790            W      x������ � �      X      x������ � �      Y      x������ � �      Z   X   x�3��/-��KU�M��+I�K�KN�2�tI,IT��-�/*�2��\+�</�477�(�8�$3?�˔�7�895''1/5���+F��� �B�      [      x������ � �      \      x������ � �      ]      x������ � �      ^      x������ � �      i   �   x���kj�0F���*��of4��"���p�]�!M�_U��5�RB�"�  $P"s��ΏC�x�h�f������\�k�̆��nd���QA,,dl4����@�>EJ��S���U�p\r�z�8U���M���TH���~�k�^a��R���s����?����zx�/ㆼn>e)D>�3���M�J�Y���$�-ߞEKw�M��{��'��l�      �   H  x���;�1�X�\��Пe8��	����HAWWש��ԩ�v��^a4G@�m϶9�O�/���zc�Ŕ���{����:x>�:-|b�~�P-3�O
��ހm~Ũ�D���?V�ßZ9͹�N�}r�F����G�f�v�����w��9:��8c(�bN�n�$�BGڬ+g5�����C�W��3g�Ri�r2�5��s�|���[�ҧu&w�s)R�Y���z�ۋ�n:���d�l	�=:�E��I�'�'�-[7�㽶Y�*�`-�Upu�.��М��X���N�x�dv��7h~��d7��x�^� Q�      {      x�u�[��q��{žX�-�b@�dK���	~0�y��KA�`˿�ߩ�R2�Þ���ʈ�5���?������?�Ŕ��ן������_?���Wx����^-��J-��:�/����O��[�>�?}��������<C�����'?������?����?��7�������g?����~��^�����?~��/��ݻ�~�?~�����w����ϟ޽�b~�D����m�����ܻ��'p�8>J}~ઞ�����!���j��W���[��<Z_���׊����?���J5�)��3%�tSH9�4rO+m�qr̖C�l�u��<��u���\AI#.}����w}��~=��W~����c��9�O��w�,1�f�A�e%�LQ����QT+�/u��5����8�Q����ܟ���x�2pdb���L�y-��'�3��d�դb�Ny�-�����T'F��%�/�/W;�3��Z��վ]��ھ�������y����Q��k��|P弲���UO���pr-��5�8���}�z/�����1�Z�j1:G&W�`(k�Tv;��[�H���}{�Ϭ'Z����{��%�gg&5����{F�qY=�q{�{���u�O���͙�dt�^�v�Xs�\�kV������e�\�Rj�V>��.����r��_h\dg2���󾓭�[����2=۷�ԙ^*7/��"��=o��5�V�ư,
;���}�ƂOL3�DmF�|���!�5����J+�wۀn�9lX��8O�n��K\�.��h�e��Ʃ]�ӑ��]K�� ����$= ���� 9ŏ4aU�}S�CM�m�J��q�9�B�<h�\gU�v����9��㗏�5:h�P���^��
������g��k�S��'�}��4��.ÔWp$+O�r�i�:����x����<�L����}�Z;7�]R�t��	��^a)��8�5-�^h�{���)�]�\��/��R��V)�)f��n���y�� Ũ���O�֝��sz�-K��lL�|zKX�r��r��� %������ ��[+)/X�UB+����|^x(��z�?jM'��=�����=�A��΢�̫gA�wW;��2�p������c�E�Y��p.g^k^��s�����:l�bP�~��o���3+2K�΄���.�M�p��O��6~�����ӓ7bMw�ʫ�� D�����(ǸC?�)g�аM��Fش��z��m�m��� ���N�mF@�@�"1�R�����yTecfT�BΚ�B��j�9(ݷf{�z�)c��.��ee8=fuܣ��Fx1�(�%+fϒ'ǧ�9^�A㠘�I]��Y�Q֩S�m�R �x:�����u���8�G�����tƂ�L�wh�v�V��]4�,?�9�fwc&�a)�Z�13�f�6�J����L�Mph4���\�L����$����9u�-v�i��Xh����sh����j(��r@���:���^������b4��4H]�S��c����?,�؉v��*�2C��Z��I]gS)��C�I�-^�ޡ�=��"	jVc
of.����'���j���!�L�j�\�a>�AŚ���ZP��bq��E]�v���X�a~�˪��?`�d���B9�l�tѱ�X�J��	d�5�;�sh��>&,����U�`�p�d�ꉲt8�"ˑ1�ZxΛy!2���5�J
�ˍ�&�Ot�>�3�u`ɢ(c.���U���PA��9���g��eʊx��ƕaV�.�s�ko�9,"�������@/@3ױ8H
�\+O���r8�w>�lw86��׃F&�};pY(�͘WГn͌�*�N���Pl�0.��Xn!�Zv��pݬ�\~�9foC���s�8��5~��o�[G¡�%Dю�J���I�X�S@)���n<c�i�a�s�e@��
��rf�zn���ʋ���(y��,�dc.��qZ�β��+�ٚT�y(@b�^AG�F���ƕ'ހZ����HC��Q%^I擡���bFe6u���=ū蠜W=��U=��4	����Rj%�S8�٫�4G�9�\�|�o�!"^�?���)\C��"�A��p!�.�pTs����Rx���a�����p0C٭t�����2�a���Ƣ��3���B���x�s�s���E�r��Wp8�k@�O�F"1(�ҤbL�����3��[]tL��2'����M<L��8˃�TG����Ģ��r�|l>�zT�pZ���,��N�z�ְ��o�*H��z���h�w�� ɕPN��ʠ0u�+����y(�2�0�p�/x��:����G�WR��9���7�W��5�H��a��[�y�=o*�<���r�x��{�H4�D#h�	VH<�@�τ0VE
��)��D�z \@��}zS᪲GA������lc�T�g�f����R�"[�O��!�y@}����3GQ��	a|5�c�0�Pu;휬����kYP���6Xw��G� ���� -�z껉m c#W���\,ntSS�aܕ�mpU]78��#Q�P(�3bI:A���#mBh ª��R
-�s�l���4_8c�/�@���ar�Pk�cR
.���2�?�/ʀ��}e.�Sud���):��D|w%sm|JY� +��(04��@0I����M�� �@O����N��p��3^vReEA�s5\_�R*=�S'̶�_��x3���o�i#����A�&��U�T��sx PS��pn˚B1d��!Q6B	=�^���� �1p츅�-�"e���5��#CX�͇�5,-L�Y't*d��|�J�b��A��s �*q?�	�t��jS&�mU�T�*�΁�`,x�K� ��*2�]�SjN����!q5��h&�]��I[a�@�����RP��b]/�F��]Cﺐ%L<y�������
@bI|P�4�^�L"�v�s)?Z�0\%���۰��g�	8�y���E%�-�E�2#1���(�ĺ��X:��R(��aD(db��slj!l�I���q(>��p���52�T<��@a@��Tb�3T����S��芴���9d"=�����'WkͩLn�[�rS�pF�'�t���H5���.2�EƏ���*�ސ���ʼ���A�
P�(+7���A�<IkxD7"��R��G��p;����݅bi�s��Xmi�T��r u�'�h	̋�1��j��v�ݙ1wb^�&�V^����9J�
"� -űXb�)��3���Klx<,{r�X@R\>�++p)H.�Ǡ�D�	�&�[�c┦�2�x��N� ���14���A�e��'7��j��`�]��!�O0�v�����4� �|s�v�o�_����W�?��cY�M2�]C�	�}�Ґ �Bj���Kl�*��C	k ,쉌\8�m����Y;Q���[�e�Z��V�\	J��D~� K�(��)��TE{�'���B��cM*���|e��E��а>d�D�
�w��"q���F]��"���%v�y>z���6�ꘚ�.{0m2����Ec�5<7�B�]�{�c���1_;ބ� s�Ѡ�>N�U&�
�!aL�-��V~<:wPpq� :mIm��܎W@$��U�n妃��,���N&�]	 (
�)WH���k�BmĤ�Ӎ�6����@���7��X����a���6c���,��F�	j�*N���n��#�\������f�H����i�0b���P���E���,&`,�|N	�;Qja���ƍ1R��]�B�&7`��/�j뫈�����
*�ck %˓�J�J��l�ī��ӏ�ѵ�(nMY�CHj��t�_ҹ�G�#����)i�;C�����Bc{3�:Å��udř)������Ɛ��4��G�j@\,5�$�_Nbb�ۨ.���\����h�6��K�T�erQ#��E�?�S�N��L�HX� n�   |�v������_�5�D,|���z��ǈLm��Dĵ����E��~#W�M#�J{.ܸ[@���� �DH#mZe��P:��P	4���+^��g+x&�y>N����h�=��%B��H�( ����&2�v@,� Rím!�kL`7	�m*�㥐i�C�� C���b�-�n]6��YϞ��q:�����a��H��pM�b�����f� �$Z�����R3}����<т��c1� ��_��7+A�a*@�!k��MO�H�����	���K�u/�A�6m.cM�!@!g�jy��84�(����0M+�xr&�,�$8��ٴ�@^���}z@~JT�s��ҍ0�$B�>QF���ڭL
�'�j�ӠD����开���%��7r��4��-�0���Y)㤽�X<#���b�/�&�������lZ
$��In�HP��������$��M�	�� "1Ovu�P<�/�	�@
�2P�v�����bI)d�J\���#���*��6�&�����k��p�1��<ڢB�n��Ud)X����|'�7��ª�٫v�\��c������c�(WE�)��/O2}����p�Bx�ڒ�Q7ٷx&q�a��k��<w��a,hɤ�M�쌖6�������04K��t��0�Y{Bh9ӈ�v,Z�G ���]ـ-Od`j�9�Y�&�e�\d	N�oS�8f)13�����|��"��<�A]���i��h:���D28l�p�=1U���]�3���	q�*�Y�t ����F�1Ix�g_���;�L�g�\3� ��n��bí��:��ܗ6�`���.�9�L�!Į�����f���C��f2�҆U�|6t5y�O�Aڍи�Y)�Mr>�J���/�/��h\� by �= b��! �3�߃2��Ҧˆ7*��ۡ��ĤU��}�D32�<�|��]N/���5T� :�Ml��l!�1 ]<�г���h2)s+ baɨ}�H�����,�DCu�f�`��Xy?\�=���������u ��U(ܡ�*�D3(L����4Z���b��h� y��DΈ�H�`P��`��辚��DJʕ�;u��`�3����Np@�Q��4��>�NBFK�h�~�A� Yӫ���W�*%�\Lݼ��hF�AD�n7���L^$\�C�J� cu)(�e�V������]BH�P�d.�P>X���壝kD~(>}�&���!�o�'���[/�	�HO��5G���G�T�x��X�n�aRГ��*t���fS\��x�>�ߺ~���q-����@a���~p��b�ѽ������K�i<8̐(����pY����P��ސތѣV��ƫX	�f����u�ah+O�)YH4׭N�+<iK�u�;�n��XW~~�C���$�kp����#�ݛO^tJ�u�'0�b8)�Q��X7�7�F���v�m������D��Dt�h�|��M<̷LÏ���(�^����ڲ>��أ���Z�^�^���
/��}5����W
<w�~��:�>s����w�}��7�o i�      x      x������ � �      y      x������ � �      �     x���K�TAD�}W�R�.�8�/8��ͧ�-<l��&���!"2��F�FJ�(zc$4 ����������'7r��s	&��jLЍ&�Z�d˰~.�?(I�Qδ���텠}�8���'Ҋv��B��!h�D���Z��y��$P���j���N��/��(r��x�7��iDeG䠕 �>���+<Zc\�(7y5Dt�9�����uԀ��F9���(U%����Y�18-��ɣRt�gG�d����mn��3׈5n*{��&�m-h�qFz��/�;ʅ_D��j?PʐN���&�o�m��<�h�O�|~;����5hܪ��k�Z" ��͘~�<dÞ�u�M�5D�/#N�Fth.|f��w���YJ��	�+|3��倪��4@l��ɵ-����E�E�4H�	��І,\�ף�sWX�_����� ��zwS���U��\��xj5Ua׸6&;�˕�Q"��D��˶̶�
��u�>���J��5�8��_�c�e����{u�Ů������!9�����W�      �   �  x���K��0Dת�0��l������nͮ*�ϙ$i�Z���06l7�&�@�Q��b��r҄�D|�aҏZ"��#���T���$�%���r";��.Mk��r6&p�X}�U���x��X-�(՜�v��P�n�&
:w�.��h�f?���5�&�-D�s�`�	t/3�slף���׬���K�bg�
M�C��Yrw�$0	�=Z��DN��Σ9��s��p=����&�r$1R_͒؛��:��0�C�PU��Y�������jZ*mt�L�+B��K/�g������f�m�BD��Z�fJavZ9;����()v�f��j��c�Ƀ^\�o!.GQ�U�wHJ�q���Ͱ�1V��.M�1��� �X��*b�!�*�=j7|�<�t�E�� Kh��)ͭ�5����x�1�e�E�;�s��o}~[uJZ(����O����W�<��V�n�au�����B��7�nJ�f*�v'Nt�d�y.���z�#1����G���uu�9��~@�gf�Zk��\�'	�%*[�4���Έ�J/*ie�o5ʳ�f�����T�R���'��*��}ƃ�c��K4!\���?�-�%)|H��4��*���;�(�߻� �%����qr'j�`�n��E��
ә�� �%?h~�xA߾ۿ5�%o5�}�]�/y�?��i�K^iF�ܢrxz�y����R������`{�؊^kP^�B?�|>�� V�      s   �  x���Mn�����S�Mԫ��Ζ {��Gk<?�MYf[�%R�O:�m9A�M���>�o�Wٲ۲l�� �J��������}]ݞ׳��v�<���V�g��|��;)�\�L��T��YJ�S�2�^��I'���Kp�K��RZ�=K��� �NA�JF�8��n��!ߢ���Re^�X#ϖ����ß���I����t<�����z��/��{p)�\�\�L(p��YJ�g�����Ve�2�5��8���s�r�3g�K�'���N�Lѕ&�+u�	EC�	�yǵ�� Fn0ҋ�����Q�(�wN4����e��<����b=U#GR|.EƕR\�YJ��ˢJG�W�oסb�I��&�&�
v���,f�P�w?��W�w���E:��H�
��t����k���I���:��&��8@���?�d�� Ox�.ʻ��]�z,S��zh���ܔ7aIK����ԘC����jZ���=U�cV�*�ի�,4�AYMH���TE����˰&��/�����t@�R��q��$b5)�SJ"6�a�\c^�����?��a^Z���D�.�Ꞓ�Z��}S��x��^=�JK�6f�3����DJvV��@in�%��+�R��6頦	E:���C�tm,�=ԫzQO)����O���X,(����G=J�`�I9-�KJ(�6�夼F�ҕ\��<,V3l�_��.����T6��)��3{���z�A�\�Lk���Y�\ϙ¯��.�^K��,%��$,��1�T�5%Ұa�²��1%�n�����A�=;+o���� �ξ�VŔ)��/���ԐA%�KY�.WuEIE�/�m�*�d�G�j�O�rU,*J��Ov�a��}J�o��1��GC�Q �Q����t����d踝�F3"e<ǜ��*�M7W}�}B�~",�Ԛ�MF����ןG�D�%����)ؤ��=�*���
M�v�}��mT	�r��Y��Tc:�f��U�}E�4[����xA�s�(���['kt�gRJB��lT��DD�Ĳ��OO� k��d�b�,� kD|,�0��f:���g�������o��M`��Y)>��%��������]IJ�EtZ�*��*�
��ф�����[^�W������E�c��}C���w�6�"�<�I� h��cP&�"@��&:d�n����cW'#�� 3ܕ���S�������.9$�]LB��&u��fxу�<�� U��?
ez��,��ցc�:�����d4��F�<S�`*�\��q.�H��߇���w��aRp�ߜ���[���lŤe>�4�6V<TjO���B��w���Y/��.�w�(>d�)}�BY]�aqӲ��o��ِ���h`�R�n�~�N^��L&�2�t�6�;�b�����5|<��ϕ����w�ᏏLǙ�;�/t#�ƍ�	�N��V�b��Q^�٤2N�����N`p2xD�3h�i�q	n�zaYt�6XT=�v��3Ͻ2(�`���E'p�a!3e�!��1�z�`Oܿ���M�P1x�P����늙o7��Z#d���i�=�m��h��á�0�~��M��ftP��y^��qzq[�Zv�.���.���h2u�=�)Ŵ�j��q�� ˜cRQ� �Y3�WU��n�qx&���yl��.�)�i?�O�iMq���K/`6���J��Et�����|wQݐ��M��6W:��	8-�,%����G�N?�1K�V>|���I�m�+�u���3�p�h�/i�7��e0e��ڌ�v���9�+�B�5�ٓ�GKI�����.��:��<�66N¾����#�8��r7h�t��=[B�k�=/ڻ�*gl͜�^^�v�7*��=yi�]�6e��r��n>�M�!�������ic�����oY�$�\HX      `      x������ � �      _      x������ � �      c   �	  x��Y�r��]�_���ͺ�ߍ����=�D��7�@�X"���g?��U~ U������$��AYL� � H��s�=�)��$�������V��G�u�yv~/��y}�o��J�R-��,kRKiȲ.g�aÊ4�ױ$UQi����X��%ѽ���ᛤZ��pN9���"J�
Oʩ乵:No��,�/w?2���wO��՛���s9>��߿�~���|��xu��(���'�5on>�W���u��ҵ�KIXn!A�
,���G�����L�����]�I4��BK#E4ي�sgM�z>(�2V�r�m���)+���$a�N�B.�
+�YZb�~zJ�Sl��|�����l�G��kRt��¨���FD/�*�s�	�������T/���_�����E��w�z��5��xIq}�����9��a��ڄ�V��4�T�O��ώ6VZ���o�!N�����P'��I��X��"i�ÖLlO�3PД��.��]�Y�(_��!U����Jj%��푤�ge�,��r�`�X_��(�ؚM-��rlOA��ȄZCw��F(�Bˆ�\�dg|����Kt�V%!��5��$��M"���<��^Ɗw�#X���<L���jEK���ZRF�]���0�=�v����4$t�F�Ve�mL�}w�#b\���z}~�V����j딗��弿��o3�kPޥ��K��H�Sԇ\@$99��'�P�N��p�n�v*���	�*%[k;g�n=	����^9|]�X^�o>VlC���e�ȫ���n�)��xwM�`��� �C}B�b�tV��8��ڸZ"�p���r�L�N1$2��|�RqECқ`��dN�(�fW��źW(7p.Tg����E�^�+R~�'�!�WCnV�uE���)X�J���\��hdҰ�Lr�L���2��Y��{�{��jGʬ�(4'��6�Sh��}�h�/b���d��.G��?}nW��)Pz��G@�o.�GZv��6"��	(���է�U��¡u�����?��@,YI��,��4kx��0ƍZX�����K&;F��}�_\���Eo/��̵� �pdʥ�\�Uۙ�!��+[+����[�C����.ǹK��"9�f�(h�Ϯ��$��f�v9�������b�c�Z�[GJ��W ���ܩ�p2k�߁�݁Q̫;H�� �����(S=���L�G�ai:ⱜ�`�Z���R8D�<l>���H1d"�,�*5"E����X�a��wd��b���q*�T��N����hܧR�t$an�06�AZ�]"6�T*4,
ϓ�HM�JiD2�:���	�(��*+��l�p�z���� ��y1T�ʻ�󼇊�Ӱ�f[��׿Zg��>ǋ�z�����!���8�O���Х�
/��� �lQr4<�2@�g�kT�<"����J�����{�FSт �e��mRW䣒�w��ί�)^-�7����a`eu߬Ť@�m��L���E��	��$j�Q`s�;����kS�U;�T_���A�9�F���'c�+����{�����H{���uH�;N�>��b�'�z[��U�Ҿjݳk��z{7׷��=f�Ԛj��_�ܛ$��F/�f%"�C���mz�ܐ�}j<%�'� ��QY�tl|�v]��[�#���aƹ���&Ÿ��c��	�W��P���i�����(f;<���`3t��uM*^UZ���?ejT���Ӊ\FO�ݧI����I������OUo��vHw���*�V>H�Vdsn�=?��g}�{`6d��zV�zց��Mt���H<����&���,ȗ9g����+�'��C���M��0�x��;/������y1�T?qv��&ng���-�f|)����l�Ζ5e��8�u<3���*�e��l���g�h9t���B��z�޷��A �|�Q�?��Z�ǚ��r���!�Zm�0��Fp�")v�w]G!��t�ٷ�����:�f}�寗?���n�)*?���eӽ�b��ʰ�J=5A  �cs� {�J�s�B��"k�(��������`Z���s��s�>h�,�T?\A���t&�2^i�^;g�z�	p7hvfh�9x���A�ʛ��������㩏��b�9���φL瑆F��5�%��>8�š�N9�Ǣc�C�������K���&��D,f�}l}V���b����E���zyS�?Vۿz�� D���&]K��\Z��-S�rd��9W�]��G�Vt�AL�(Z|�Hp��)�����]��U�L����ܸ�`���������L���R�����-�Vz.g��К�Og:�VQHJ������:���!d&[���k����p+��3IV���B�(�勴+�%� �'Ns\�"#xFzW@IG�7���k�R�w��q�uw�zwqy=BhHy�Jw��
מJycq?L�X����@>��'�ę��� ӑl��8������ɓ'�����      t   �   x�����0E���A�ˎ�`�;��$����8REǏk��'f,��K�ܰ"��x���h�����q���v����!mF�)Ѫ��"�tt���,:t��EL��J�^�=;�p-��z$�c+���C�������#}��d��jW�R�½􀈄����i�g�h��S��H[e��LD���      d   �  x��W�n#U]w��~���Uu��C��#2QЌH�i%vl��c�f� ��X!�FB��2#��B�m���ѝ8�-�����T5���Lg�N��/�IYeoO��ѫ2$Ӝ3�5���.�ɶ^�'##;�N���^Y��o�A��Xϰ�oG�><W���K����R��P�Og��+������p�^���������|3;rR����P�CD6� *��	��Mv(�C�1ȡ\:�����_g�D?8B�ē9D60�3I��s߰G��JN/����zþ��u�� ��0�-��G��hP��Q)��|�Y��.�e�I�)����FC�4&lJ��J}3�0
\��`rc�Z;�D�k�q�do�e�zS�:.�u.N+�k�.��u�"�K+�4(s�9���H�t*|H>3vDN�lata0����Qm�q=�ʲ�.�@a-I�/rS;��4u���^��t�9A\� K��j;J�?���Vof��
V�|�}A./�"=�S�D7������b��X'�q�o�C>�q�B�BS��<��Oj�� 	4
��WN��]���0&i���=�(�X_��Ѣ���i�M��9�Jc*�EԖqxVe)%�g(�M�D�����H�)�r�.�����p�0��$c�M��ս
���cn<9
�)��'E�,�)�~31@��l��;���9h��;�P'��^"d�W:ks�fӁ,�%3�Ug�����Z����M�}QN�㫺���g݉���1�=�hk������O��?�Y�fw?�4��~�D߿�6%oYϥ�ƽYh,�M��N���j4����N�vz�OFU�1n�KV�-��sx�����S�o��fw?,��#�W�A�`���r��VΛ�-���n����c�����n:���oΙĚ�kMp��x,�x���EQv�9&.|�Wk��r0�w�I#R��|{<�L��M�&��������ϳ���w�<��{<���-�x*ս �Uź�t��ps��<.�0	;�[�ڊ����G�:�F�!١ =8���o=|���&��EY�׌�M#�,-Z����>�� +z�12ju�<�Q��(�R;��h���<�\�ء����e�i���zD�	{��9��[V�u�x�I�m�v��,�^&�n���fy����f      a      x��[�%7�E�3GQ(C)R� z�уA}��{���HWU�B��:�|��9!r�\[��k־�o�[+U�\��M�T��?��d��:���/�~��_KyC��3�
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
?���c�k�����o�N+?z��hv��~�q/�O��w�H����~-f+'�h�R���7��mjd�I�%��W�� �p��T�I��Cj����1�e�걿ުv&X�w�A�ڡ�2mR8��R6���t�&����Z+�ؘ��XB]O��|mq��/I�9��~�m�G:gRn��\�7��'5�Z�i_�����*r^��Q�s�2%iS�(׾�k�6�k�Z��ff �o��E{xW��=��g�K�Aa��r���	�J��QVQ�Jn�L���-��7�6? �n� ������y����9H��b��W�<�5��v�� �  /���\�/.��pwn�"L�vj� x>���\kW{����G/�����et��-�u_�������?�^���>k�;fz)s��;%�6s�>�j�z���|�狺��������iZ�oɁ��t��g�I��象cmk�Jbj��=�0��~r��v���Es��⹊n���t��ec�b�/�O�%�;�ו+���܎���p5�H*(��{CH1:�-%Sq��r�w/�.�*H�a�1�c�]�?L�ӎh1k$BU�AL�6,<�×��"��ՒoSx��������pԔ{�����^�H��Z�t��cn�tALzvK�v T�^O����A�cpb���QF�=55�	������V>Z�/i�s|����~|���߿�g�_$      e      x������ � �      f      x������ � �      �      x������ � �      h   r  x��UKN�@];���l�ǓaV��B] X@�c�Cb�-��	�i�&�jD��b��{�����b'���=0��Qjj��9�9��w]����驧�\Ȝ�9����� �����i� (0O,��\�7�aEB|v��[��$�L���/�����|;�ҡmb�qF<�I�g��]
GCMmט��%�'b7(h�sGtW�9�F�G�`�ǣc>��4�s��k��&Fņ�U��B�̏�?&K0{4>��'�yC0�����V����u?����1緇ՖG�$�	�-:/bxwi�8%ac��&�L���kwopLM��M�N@���?E��r��J���K��Ҡ�򪪪?'��k      �      x������ � �      �   �   x���M
�0�ur�^ A�����cl6-�x+��P���6�A;�[@BW'Lq)\az;k<�]��`�A��p�Y�����7���l�˩�8sv�y�mOC��5`ߗn<{^����鋉ճ�Z?0=P      j      x������ � �      �   '  x���kr��;��D�����X�=���,��:7g�6@�0$�������!,�,ç\��cNǣG'Q���s�K[B�^��C� ��>+;�H��>ݘ�m�84��{�~��D�YS���7�1x��'��7�5�5�%����	P�x]%z@V�D��HZjkQK�;�,-Ւ�_������㢳�9�2i�tgJ�~,\nǠ��Q܈�\9\B �Hт��Yz�zU�VRN�6q��K�$9-達HJ�v�9>Gb[�p�R��eߐ�Z��px� ��I��@*$�Eo8�s�d����L٭����E�Ņ1R�!i�E�lc�e�RB	�ҍi��*����>&R��"S��..�Ur��!O�c��Z/��fB�Q��
RZZ������+�t�kE��ZS�I
��ڐ��)�z�TWd]�xqs�����������@kA���@J������:�D}z�lHyQ/�2\�QK��Z��"S��Y"�9��s ���T�:��TYkߤI\#�NV�A�8�sj��d�L/3����=3����w$1�W��NKeߵ#]����k*N�L���\���8��Z��S�$�\�˺b�5^*㑈:Bm���>$-�����_1L�}��#���t��`w��IƦ۽#8>��YWRd��ZX���fN)	�Κ��X��VҀ܃�\u���Em9���j�	����*���b�,�#�������Q/9R��H��=���7�'ZY/%�H�������o�|�	?G���e�:�`(QZ����\�%9�� ��'iǜ�y܅��ԛK� �ٻP}9��u�ν�����躹�r�'b`�[�|�r�(���aX>R�B�-C'�~��>`�S�dR�����`�ȕ_���+P�\��������&ן��Yy��EvgnO��m�<�MZ�YlP'�I�{�y:�trq+t'�ρ�V_'���@x7&��+��$ڧ���5$&�>����J~rW3 �T���v�7�"���^�,D&�>���t?D&�>��Y�Lj����I�͖__�="�y``�8,P}JI��i�9)%�����ͽ�ˇ�}�&�$x�@a������9'
 *�����:��/R�W�V�-H��8�E^�-;�<��fZ��a�D��D�R\˫+�/8����'
Q��pN7����w�$�l�A�lK�I�x���dZԢ	Dl�:�¨�C<���j����� �`�)����I_xƨ�'�A����� �Q�-�J�@L�{� �1i��-� �MMF��+1i�������)a��{Ҥ�1)/A�5��='G4��u�����[�"��$��q6�ļ}qҺS��M
k��w�A�C>S[p>��0��Y�˶����a�)Jo��{����>x�6����L�zy��Ð�>x�l8���{����a�Z�~��1j�{}�Ĥ����{��~��al��f�1i�'|�&���>xc�����=�Iy?��0P޼�Q
�H��0irsm��"�[��:x�T�oJWQ��r������� yr��      �   �  x��TKn9\�O��x_��w�@$[o��d`�q��\^�-y���PKbA�z�*�@,L�>���gP_	�����|ۗجeL���}@IE��0ږ�)EjҢ�e�V�U��-F�r�ڿ�yX�_����~?�|�-��8���$v����ù9 �(�!q�����1Y5�z	��i&I*"� ��m�E#/E�pU�`��m]�%��$ �j��&������jI* �MĻ�lD׀��D����VQ�F̺�(��,ZbE�P�p�_(�] ������T�PU�70��W ���O�sRf`=�ݲ]b��7��ԋ+�6��ܒ��?}	㌶�ʳ8+�Ԭ���`�['�+������ �čڈb��Z�#<�ʽ\:�ӯ_����w�'q�7�qq�֌�6;٣˥�E��n)��ܷ*bQ���C��@��s��������9���*���Z���Y��VJ������F����J��A/X|��Sr~N�� �,1����%V�/G� �y�`Bw_�� ��Q�4�0��1T���(��XWv�-h������/�G{���g������g�� ��~��^A��g��x�*�.jW���+��
 ͥEk�]�6��+F26:�����Y<��ϓ����n܂Δ����)ֽ�!Kyg߼
Sq��T�{�\Bۭ�o�F��V�ue+ۇ-����w�q���4M�-!��      u      x��[Mo$�u]�~E-@��>���.�{��@����9$F�TF�exᕀxa���F�v A�0������󪪛��b��!1�&��U�]���{�9����n:�8I)���[�;�)'����z�o����_]�z��?��������'���oR�:���U�}�U)��H�Uܬ���m�X�ޘ�w�����[|$�������;!�;!��\u�hgU.LYE!YTD��+��2�n���U��g�'�Ȥ�bR��T��V�B��g��B1NL�^�#����Ɖ?>]=?>}Y�c�Z�}$�+N�pj�0����䮷N��ڹ���8�7p4��\)'�����3�>ПҋN�9�Kq�����Oa�4?�[��m���bO���ݦ"x��nJr�E^�U�d��l�vv�.��
�VF�$�����<a8�.�'�MT�hQ��H����cƹ`�o������v[�撤����~�׷�v��{2���+C�=�*�K�	��P���ֆ��g�h�H�`������P�O�`s9��{OOl��Xv��K|v�[v~x�K�3������\]�������W'x���8F�z��6]����9���I�"�DŕK7��ާ!�;&��R]`�J�cȥ�zz�!1�����촋ϸ	���s����'��)�kՌ$��cY�dj�#��3�:r FB��
�*�c�*{e�Mi���Q�*���4�#��8����j&���]�"��ǜ�0ޏ[��i��I%�w���El?:���>_~Ր�z�k`�2�|�_]����gW��?�z��ԟ]~y�N3�t�M	��v��-J�3�h;Q�!��8�e���V�Oj���A���m�
��,
�<�>�5h��6��,E�l�U���Xq��w��L�)q���QR`n���(��ݚ�PԆZ˷? ��%�2D�iE�:gQr���jѕKun�X�#�οs7�Ȝ5��j�)6U�L�m��P-1����u�ؕ��5��z`Q�FEe|V�RX�'���W�AY����'��n���Vl�EAQXYGʴ't���Ca.�{��Po�I�M+�;i��]]�w�O[f\���$|؟�#��O/�D�8���e�	�[�N�eW��T�lA���KL�bY~�S���IdZ%�;��U�%!C}3��V, ~L�kqE��A/�J\V�B)�e���-T��|?W��������
�$��ޖ��5D�uߊ��B�j�#��Mƿ�����q��H�3cu�� ��� �H� : �R��u?���#��Go�_��Fnx��PN�0E�1�0$<����VHk_c1��*�I���%��H⑊����D�=@�s��23�H��X�,g���M�#[^����FI>�0P#B�����n� ��� S�-�3o|�>�(&>F�p
��A�,��de�����&�	����#!�!�@$7��ty�� ����B!Rm
��[H���3�j����3H^gP��#5�
p|��-R�����i5���ů��Bק���嗧}��}:��Nȴ�����d�7�Y���c�.���.�p�|�:�^�f����g�0�Ϯ.�y��x���B����f�.~��h�l�l����_������}<?o'����΄�Nm\���츜b���'m|}Spr���v�/���k\L8폿�U��Ǫ �
�'��g���E��f�ih�7~�����Q��΍Ou�u�Y���/�8�_���5���F
�� �ڈ�������EI^|�덽iCtn6�j��1��(�%�s�xg��"ƍw��X�j����@N�hXv��dR.m���G3`@qu���n�`��h�;���m�C	��a�=��%���@
\�T�T�����E�I��������-��#����ARS�Fj��$��鈝�O�z�}T>M�N>9;9E���3da<?+�ߜ�S����Y]���i��̔7�{%Y�ܷ�
���%-�H)���Y{�\)G@(!7�9癐�GSr�t��~t���a) �e���W?��aR��������'�'无��FSL̓��4���ԓ�^y`�[�D�Ƥ�	e!eFF[ٺ=epa�l䶘9i��Pcli�^w����b�!!��m��ܑݚR��g	��}���~��5a��ج��� �fhT'7*�ʔ���O�� q�|R��u[�Հ�s��/(pU����F�T�n���~9"�ސ��D��ri��y�"Dms�����\�-�)[���B/�-��~��ݬOt{��'���*p�\F�(���3��)++�a��E�[:��<wJ�O\1%j(�T��^}rc��c����'���T!��T��W�K�>�Ex��Bmy���,�����cQ� YE��چ��S������ �tO�p���	�i�J7+�����k'ޖO!��"�GA�"C	bN��*���VZ�݆ԃ�� (�M)F�'��F{�k���p��̓\�5�$�ƒ�D�\tS�����S��.��.5�z|l�+L�#�r�6�hs^G�!�T��\o��I\�ϡ�C⒚���[���ц���	�5�;��Eo��h��<Ƭ��R�� [o�J�,FJ��ja�8e���
�8P�q��qH�`A�.�PB0u9���J� ��O""o� ��~1����w��c�N�T��qSj�g���!�	zWV�h������YG.�u1���}�40x�:�%��+�¥]���Eqe�կ��	\�f�V���g�/�R�- <�n|Bѭ[���_�Ʒy-�w���ɦ�ͤ�G�!�Dqp��d �a!@��TT�@=��F+�T�1z^"Q��,��L�"՜�ۇI�^p���TD8I/ta�1P�i=�����xm!s��C�4�n��gFA?��\g$���Ǟџ��w��a��{��RR�&Hfx���,ʨ��q�d��MF�$#?�f@��u�)4��/�,�S�`�}���.�����\��`�uԜa�"�U���=mv�"����o|� ���'+�E/3�_t<�EEg3���L�	��i��nF�E����5�.�|�U;� �T��L���/!�A$浪R�Oa@7S�s^�EwM�լ�1�d�+J҈=?�y�#ե����G�P��/�T���m�n/z�A�K���O�7�7�些%g���^�����n��y=�<�kS���(���C�0��E��>�O������!N��Z[fs���Bfq�o��r�(���}�!�����r���]�~#۳�w�k�43���n}
��<+��!���R�8���{"y��M[b�@�E�����&�%{O��
�k�VP�7z3(����r_M��҆q6x:���<��mO6}�u^�}_�"B4�@>X��i�Z �L����U�Qn����RLU��U�
�6�.��)����d;����g\�)r�q"'��8~���Ӓm�0����ȃ@���0̛���%
�FomN��,#�:�Z.�l��Mt�� ��P���l�5ܠ��F.��X�0� �a�a�~L5D�*�C,� g��<�0��m)�F*�y�=�ᖁT0���hR��R�
UCbS�͑B ��V���1�&U�!$��ݛ˽��rݏ�K�*�ЇW�N>��]��V�����#����1���C����n~�3:���H\��0<Sk��+81�PS����t<D�#����<P���L�訑$�o�8n�p���.�65/���Vr���<��mN�:�-�o����;7A����{�'�_���'�e֙�Z��9{y#t��{�d��F�9A�p���+<	6;
�kJ܀O���FdY��`�d�٘S]�i-u��I--�X��Szq�����}��w���kH��e���{rǻB�@DYP�jm��I�c�C�"Fg��&�W:�G0����ݽ��A�0z�/H��j�8�5<h�X��#��yTq�"	ns������3+
�h��q�(���煑S����Ԇ�da)o��Z��#2v���p\�GY� �  &�\����B���o�J�'�;���?}cn1����1��-Ϣfx?K��F��Xd�H��l���kHβ��xl`�H�rHV@���o�Y�������/p=��!SkȚ���= �V��,��)�q�;�~����8f��V8h�z�|�P襐J�F�6�H���c�륽�H���=ηGgJ7�&ۂ�]>)�x�io7���?��%[>n�)Z�bɾ��a����H�����& �f���!�h�"�m2�*g
����2��]��V�ifp\��Zw������Q�d����8{~/N}G��~�UI��!�:��*e��-*���]�ZR2�fc�ʍВ�F-ƾ�\?�e[�3��|���������/�������ڔ)��$x���g�����YEg*��{)�\̔�66��l�k�-_���^"��Q��n-X?>����@0�      |     x�E���@!Cϱw@@��l�u,�=xxC �#D?�2h��GR<�fфF�z4�Ԓ��,�� I��G��2�t��QU	,�5'�6�S-�7�������;�"·��|נ�3ɫj�}r���(�+v,NQ��l�SU��L*�<�ZC�y��V݁ �<�����Zh?t��0��I�s��$c<��;��u��@�:o��iw��E��ߥYe)��x���*v_-sWr��������sDe¸H՛���vNL�5Z�d����r���d��{~��Ok��Wk"      �   �  x���[�� E�u1�����CԤ}ĚT��Iu��� 62Ā�b0l������ޣ�U@���Z�s��&)G_K5u�B)Ԁi,qNP7��喖u�JА.X3"�U�M,�8���7FF��j��P�%���Ps���K0�:x�C�+/L<<`�1=�n���Q��ǣ�D��A��F�~��n����Q8Ud��rتceWK�o�䆝Pܡzi��%��(�FM�d|�A�hX�;/�������@�`�H�^����|��^��X�*�@�Y�]xĥ�i�2<$�8؏���HZ��{��j��%=,���W�T�4���CR�*��g�ކ���[J�nkdO��=��I�Ӈ�����k��N���\�Ȯ���C���`?�Z��
��      r   �   x�mνN�0��}���#$��ȂX`��I���ޞ�HBH������˥��ލ�p��
d�B�HIs��E-�q���r��e�sʎ�F�A�\_��[
3i�F.���K�.�&'���qMT('3��ǚL�h����3���{�7�{�6�f���7�C����4��NSH
���K�#�x��SJ�ؙgtSqmf/�2}{y��Oz��0|nON9      q      x������ � �      k      x�35�45��".SCN ��c���� Qw7      �   �   x�mO��0��Wd����y�va)_��ҥ]�пo�P+d�ϻ�,Y������bqe�)Q����j�<���v��n��k��f�����WW[b1���v|jّyGBu��ĪpB�~��̒�{P�=Z�_a��G��J���*���@R!�sd���7��$Po>�4A��<ܴ=�VS�٪��c�@�      �   �   x�M�=n1���#���6M�9�6�m6J�������@z� }��Ч*+3�ꊩNB/%a�Br�C"�I�D�R��Q�V:( ���se��t�(��q활�7/hL��iXD�X�ܸ�^1m�6���y�r�X[mT[�>�d&b�(p�|��2��q#o`py�]z`��3��K�	%�V�xWW�^��q��*AHE      �   �   x���1N1���s
.���ر�%ݞ�&�$�q��ѹ���1�0Pa�*�Q�0�5J,2ґŲቑ�P�X�G1�1{��5K.�"��^�U^>���*_ ��P���r393�#@ژ�L蔌ڪaNu�qiYmme��It0Ս��᯵�.����Ř�)�J�8#~�\[�9�w���x�}����oݧ�)x5���GɅ��ެ�k�G��z���bV�      v      x������ � �      w      x������ � �      �   �  x���=o@���_AF>ߗo�@#�J�jځ]�� !_4$@��\�2�Ųl���G&g�8*6��"x	Ꚃ6��\���PyL�
�ԡ��]$4����r�mf�>����t��nz�v����ﮮ�ӭ�#Lț��+U�ilс���;��Rk8��2+T���J�*Rzel����ySop���Y�u�yv�����f0=U�Z=�8�|3�������PO�cE(�x�hʘ�>d�ܬ-b�KY�ǻ�������a��~��?��L��,�!��Sg�A
�QCL�|C#\$d�ʃ��r���X΂��Y�λ����fM��8�k���5}ڑk�_/e��8iA�X8���6����Uq�e����W�8�X�v��ޕج���j�x�X�T21��r-�V;�`j�f4���pj�e}.�9l��Z����,�׿���d2�.�|      z   5  x���;��0��>� �%�%R�R�il=.����ֻ���1�R��)��?6���XZ+����������0\�5U�5	yOn0���w:���sOs�b�����,���fêj7��-���:�@�tCJ�
>��+�gP�`�b�B7�8��G]�$�G:,�d���������BmT3 �;(��+ɂ#�`�p��K���s�2Td}{J.�K�?KF+_�s�3"�W�P7),�>�Bc��L*���l��#��H�DIE�S�s�l|��Y��8����?�f�X�^7�2�Ē ��4���]�ɽyx;���߳�����,e�e�:���U�lJ�c�����)�j:��$�crpO�)_y����������ɍ_W�:Y޾w8�FL[�\���ova-�P�c�g3V]�/�ȟ��R��A�Q1f�7����ڶ��5�;*�V�Z��^���Pb@�Ǻb�9V1���5)�8w��rP��!z������+D��L����B:)|��w�vq�;HrM	�I��T�{��t���X�(3�>�B�y�v̵�����ŭ}K      �   ?  x���ˊ�7��?E^@�,K�T��v6�BH2�!��}T5u���S�@S���$�J%mC���C���sY`c5��;�=�l)��gϺq1< n������0e@�_ЏُlqW����K�s>����V)^!����*�+[�J]���������������_�R�[bm���8�+���*ku_>�RA¬)� �1�1�A���$�QJp��L�΄PI��N���T��[�V[_�p.�Bô@DM���n�
,.�'*�#��I�M����NCmcBRlV���uO�eĹ&uh	'�;��["�j��o�sٌ���?���#�k�hչ���]-y��J-��0�E���{�_���#����K>(�����RK2UC�3��x���6mu��x�dY<W�n<iA��!��FHj��\�'T#�H~�bM��EϗB��N�O`� ��[�!+7܃n��_X�e͟�KhJ�n�:Fx�8"T���N�⡏d&�k���`���e������N"�RW��M�������,3��h~��t�Я�v���D��c\�z-ͽ ���&�Xz-�s�����G�/�r�|���{\ѹH���[t=� ����Z�b�=b�����WVy����3<���LqN�Q`�Z���\�c��m�!Ϗ��;�p��K��|af&j�KF��v�N�9����p�L{�1��|��i���1U%�5�E��~O�ȯ�}f�x������3���{Y���qA=��-�Aż����������9R$�A��F�%�/ge�I�9�h�$�����?t�t�pr>�������y�m�.a/1�8FS�15���Z� Q.?�,�Ck�P�)F���+F���좡��=n��^�0���C���ӧ:?����H���3�2v%��g;e�ń]UFJi�m�Wu�S�4�C�5��?�r_<�^х#�l����Q�ݏb��\�>F���]4exK��F���cE����"��D��PHåZ0L�˶�L8��=�	*��}ח�}�Tg�^2t���Ҁ�\[R�����{O���P�5��������/2�       �   �   x���1
�0��Y>L�e�r��%�ҭ{�m��$�"0�>��_�J�^����yS�0Ӽh�N��-CT3��ڤ*P��h��z�tLZ��N+!u��6���QcP�P������[b]�w�g,*��X�E?��E�������n���RJ�9q��      �   �   x���;1E��ًGv;v���o$-�g MAC7�-�t�T�U��5S\	���Vl��U��i�(��FQ'8K$8=��q��>�9AV����^A��D��,;��fю��p$�>j4'�U�w������lL9��=F��I��o��\����rV      g   ~  x�mWIn$�<��F���	�`.�
�A���dAMe��H�DdZ�b�7�����IR����Hl6m�������f��9�H�,�3-�g��;V���E��h졕s����<<{+��(ꦥ��e����C�4{CRr
;g��x���BIEQ�V朦�̧
��-H(9r�S����WgL��tg�֗�\�y��2�j}���U�it����Wm�q��U�B:�,�x��ʃ�n��Z}CS��fKr�ny�娭nZ%�q��+:�R��K3�	�@�O4�닝�Q�ІxM�XL
����n.N�4W����V}��q^h�U�<.s�����o�׶V[W�|���F�^;a4�)�ۺ|k%���9�� A M@s�֮�/4��V+cGE�����!o�m���h�y��o�����!T9�K���y��i����Zz=$�x��>�,�,x5�F�+�S�Yo����M*D4�����Ǡ��vk�V�d��zH*]��_�6�����V��g�����=Jje���Yڻ.�[����N�6'ͲH���@�c���_�����S׎������B���S���&��PV��6;�%�@��1W��b����I=W�Y�v�Ufq��x*�z��f��o���&�%D�Q����c3m�ɱh�Z n�#9���B����fة/+ma�no��Z�;V��{���ٹd`ڂJ?sa�s���\�O-@T�v�}��f=���`H�y=$
��g�Sh�Z��a���%�QV��0��4�@���� �N��wڧ��΀PgVԨs�(�ދa�N��s�7x"h�Z������ө�����r���j����!���yL0�[wR��[ȫӊo!�}�+�w�:����^�Q�S��k��^-Ȅ#U-C�;��ʢw��.I������֋�z�FR���A����n�w��#�k ��W�F�H��l�1���#O�
Z<�@{g/����;�q˰l�j+Ց{zNw��:�@5����7P[�&�3#�q˓p���qw
�+=:��7.��� 흽@�	�O��T�~�^b����[(�	��+��������#0C}��0�����̎�h��pJY��4�z�B��DTk�T�tyO��i)W��$�
�}��ːw-\4��y{{��q41�A�)#������!���h�� |���ֱa�J򸗃Bn�a��%��~W�7����p�GY8M�9��o����P�SmxF�񻲀�.x%"�}���s��Ɨ�����f����Ɵ;�{�;���O��ҩ� s�ɪ7&ZV�>�h��j��q:�T��:= A�:����&ra.�L��we����q����!�3��H���v/�w�oL�v����֌A&�!�����E��VJE���.l�f̳����F�K��"ǅ���q�7C&{�͡A�����U��M�%.�u	]�xrA`c��Q�o��N�v\f���@�O4�����>+�4���<��mܲN/X��S���O���c�(x�,i��-�џ�Nbg�8�7�6��R?А��?3������-
t���v�|�� +墽i���a��3�q$X�;�����/�U��ͳp|�������r�      �     x���KnA�םSpG~v۹Hl��SB���I�J�4ӚYt�sU����Fs,5%Z�m2Xe����
	+U�����F@��w�O����e]n Ȭ
�����0���e.�?�|:n����FY�f�>��	t3���|�3�>:{K��x�AF?�Q��&�no�EA!����H�i�H��4�����1J�����@H�^�R�S�#�E��1=�sa���A�Mp�ҡ��xQ��1k����T<d*��@Ez;���[���Е;� ~���~�Ee�飣6�.���Y�s��o��o�ٜ��
�Y���L�K�����������k�\ܳI��z �����7�(�XH�n�:�K{$�Y�}��g"��^I5��w��� U����uz�Ah�����V؜�ګ%�Q&.Mg����l��!�u�5���4LwF�,7�pe?�_�����>r�r���m�-Ŕ3�1��X.���~�L��]ŉ1E�E��I��}Z�*��g����_nonn~3��      �   �   x�U�;N�@��Yp��w���f2�
Q �?y���t���#�C)����)�́�T��"R�bg_J�ۜ5G�3��ح��EHK-�i�I�0&x�����v"`H�;-`*�d��R�:���k�³�Y\ĄV�a���*����go�rD,��z$���}��Ő�<k�)�&2��ab��K��5�'8�ut�X�������[�����}߶�y�IS      �   C   x�3���450�30�4F�&�%Ph.�4���uLt-�������ȉ������ �[`      �   �  x��X��#9�5_��+  A���çu���q��in[ܨ�nEH�����DB��W^J�B�U���P���jũL�^�k��=d
�W
��Vk#Qk�t{��ÑID�-]D�H,�,���S�)���b����_�z���w��ǘ�Д5g)1o���j�E�`I�R�4��A���9�'P����3(�ʓzOʁ�fP^+x�)Hk:����U)������B�S���2I%�S����j2�Wut/ip�3����D���I���?Jv���̗����,�@�2WӘQ#ѠMzpGG��p���b�σI�(~��3�x��R*���z�+E�B��C˥��s�_�Xѽ���}ab��+)i)'LN71�Ush>l�Q�Bo�tZ���/LѴ<����=�\F%ұy�7��,��V�8���G�ܴ/�֋Q�`�GL�T{F�p��\���[�x�H ��O(OS
--�T������p�d��xi.~&y�Ir23GMĂF)g8<3�̚�z�Ʉ�c�!\8�u*7{'F�C!nC�Ҁ.���Zm4A���+�DV(��
x~e��������Ŋ�Pl{���v Ý�t)��OP0j*�A���
�:G7�&t����	P�p�>���������昐Z��p��SH�Gô0z+��٢򮔕+g�x���y��f�c�|���W�y���Wz���&��	�"i�4k!�JW���(��>�<?���Rv"9b�Ս"f.w�X$Ԅ�9�k��7�D~��Wf󘏘nJ���X���*.���q�U\ڜڼ�1�}�`�E$�$~��]�xgS�N��EP/�2=#�@��A̘gP��9�p/�Q�)L�ɻ��]�Fc���D�_��"�ꄡ%�P�^'�'ɕKR:	���[�)!�.Ca���"�Ww�H�WL�3��;-�w1��ʡ�{��u�XB�#O��X-�� �F��W�S*R���I�H�	���)�ô;DC5�5�	�`��<� u�Kw�7������(H"i<���-LT���a6\_9AY+���H�K_����gL�c����\��O�2L2��񠖾"��G�@���$	d�л�(���\���b��a��q�4ô�5H�2��S�[yc���S޺#�֎��K�u�E�a �`�:�5,|k��ʈ�`���r�a��������0��wm�+�e`j(ڛ�����o�+j�~Ĥ�09,]ѽP������}�V,0�:���O����=�p-�Ǜ|"d`Q_0N�G��v��
q����a>ߢ柘��ǌz��;��[��t �Ɏ��I��UK3��F������/rǖwo.�KoF��Z �/j�I<1
�a�>ޠ������)���4Y.7J��y�1,	UCq�0|��jR������Խ�J��Y�7�@�Rn���d �oui��ր,'B�/ݣ	�g�(^���PGL7S�ڷN*űnj��Kd�|yL��d�rM��������AD�a��Aݴ��:�A��U���A&#�wD�X��l+�����6�����{��}�.(T������1P^�cM�Z���������GL7���^�_R+;��魵���s��w�P|��$����B9��n�������a(�Z�P��`H3%��-�H����gB��<��AvusO��N��P��R�{w�A������*շJ�mQ e�����~���?�      �   ,  x�͓�n�0���S��[e��(Q���`���\dYn��.к���d�	�8�Z�R�!�4���I�`��֡Wh4�&[!6�5�k"RF��Z�=���������C�m��~��Mu���﷏��#�۵�����zV6��W�<���ƔQ[�ڨ�;�"�N_^��X�Ђ�@����F͇K�S&�qgZ?U��)��%o�i!scM�T;
삒�C����)i���md�`��Ƀ��[1�d�W��UX����u+:X�r����[�wG�\���vl\�1��������	����
 �zV���Ѐ���q����2�̡�F�u۶�{	���ێ��Hg������`�_^\cCcL-ŋ�	i@5�Q�T�S� N�]j�������pV93A��Ġm�^�^w�o}m�Kp'W�;'Ž�P�i�ol�)iyZB���~xk����aT�[ߦ�������ȉ4;��r�����6*2`���8�8��._�~�U<�-+�r��r����3ʕ����9�+:sBF���s:w�i1iA)�f�[)w��m�֫��7�؞�      �   p  x�����1���wɒ�I�\у��.�{Q<����YE�"HB����,9�*��͘7B[k�$��6o�U�J�D��4��:��dL�|_�H>���J,LxG<Jgd�3�DG�������Fi��x��֙��J���L����D�S7��������E��K�׊Uk+��5���lso����E�tz��4W�V:��
A�c���}�2�7?�1�Jz�I����>ź�I��L����O�_�W�qx[.*2����;�T�^z���V})��w:$,m�-�:�����X�j�s�v*�L�k	����-�O:�KB���m������M���P�+�D�Xt̹��1�j����v�� ��3      �      x������ � �      �     x��нm- ��;Epdc�"��?L��n���œ@�0��1wi<G�8XI
kD�PN�=��)�1�S�wDl���e�`S��~�&���=��Jk��e�8�����i��>���偃��d�l���ȓ�J�����v���5��"�wg8��zI޿ݺ�,�g�u焕E7]����ί�y��r����qs����lK������#Uk����%��5���׮]��0)�P�wܽ�m^�v����^ˢㆴ�Rnu0� ق��r�r?���7���      �   �  x���1�]1��/
�,�R�f���i,�nR�d���B���W"�F�[n2UX�5�\ơ���NZc�(Kt"OX��}�~.IJ�}�"�sN�);Ĵۗ��M	l} w�J.���)����ߧ�����ۯ�O�zlJ��.��ֻ�Ｉx����yT��X<m�0��S�oN�<���`�j��8��l`e�2Z���^��d��y�x�e��s|q �׉-���Ȼ�vm�*�HB���@V����.�LjJ��F���*���}!/{d��j㓇D�*�����5�k��|T>k�����ª4�&�b�΋�})e��7�Y!�V�k�,�3�ڻ]�+{z]m���{�>a��y����NS|�r=�To?��Q�����y< "��%      m   �   x�}б� ��x
_�A@GӤ[Z�Ʌ-���ؤ��;�XL`��w�~�)AV5<�FP��D���/�LJ�,E��(+��P�$5��P�IJ�6/iQ�����2���yt4Kz��XjN�(;>�޹�J�cv�gX\�-V�����眵�Ҳ7?�u)��yEﾹ�:�'���Z�      l   5   x�344�463�4�45��Ri`h�i�ehh�2��RF �%p�=... .b	f      }   �   x�E�;
�0D��S�	�?��I�Bv�ʍ
a�Y����>�@0l���0ҡk����tX��	ʛWv%����%�$��2�
���?}FU�Y�d�\��y#c�k�V8Z�z�;���/v$-Z��>)���T�̏�x�s�}5      �   �  x���[n�8E��U�ԓdy����93�����.Ƕ�(~� �������dP0�j,�?� �~ �?����/~b��V�t	@��͂M�����-J�����ׯ/��^m�L$-��x(�>\�X�	^�����؋\|��#J����0�ăRP�\B��B�\{Jç�[��aO�'z��h�0+����� �F�d�g��q+r�.��	ʘmb���r�U�xJ�9_�{�=��EH��A�O�p���@��~/�j��5;-��<O�83\�ù-|��t{d�3�0)(���@���0��ڨ��i@�D05���3�i�ׇ���ɩn��8���VC��Aj/����b��j��9�����<
Ƅ��� 1[(�փC������p�?����~Ep�����B�����'W�܃$s�-� ��Ex3�e%���>�<�i����a�P��=��0��h.��-4k�"Nʽw�"�w9/'u�>٣Г�}�$�CT�l�`�ܷ���s�k�o�g�ں�A�XvO3v	��ag37��t-�p��<�ҽD��J��*�-z���L�h�E[Ȩ��q�=��Лɔ���uY��|��>���%�����c�=��]��JM�9(��Y��<�>r����,!z<o�Sv�cG@F�D6]�*�������kUWxN�{��,.�������q��G�:w���o�%N���*ox��u�l���l���iQ��ªEz�J��������IT7��m��ѳ@�8h�D��=ask��-�-}���U��n��>#����$�D��L��������\�D�{��'�]�*�O�^;ɱ�
.7?�o{��b���X�ܸb:B�u����e���ۉ��н�I�sF����ЍW9�89S���ZN1�9����f�o9�4�[[,�̰�?;�k�9��]lV=��ۃgg���$�QSo��fk�ĸ�Ai��^_v��o̓��      �     x���1��7�gݧ��(�������k���C��H.@�O��"�ː)W0l�m��||d�T�
��I�)�MKg���x��ky��>|;����I�{�����Gی�k�ܤ��6��N�~��>��9�\�ۻ��w+3�fN�n���򖨅j�L����$�m��ZS�nbe����R�4��G�ڻ�Q�Wan�9	0�A���ġ���M�@a��˃FkB� Նni��,5�Od1:�'��d�Y��b�i���(���5�2�ߍ�b����!5_(eN����UI����[_�#�~�O<�)W��H%��3�}D��� �ϰ^W)yI�2�Z4*�Aj�i�4�T���σM*�0���&�:�7�">BV�'�8�M$?�(����(8��>�%����{{y�mNq���6E�4ʊS2��'�CP"}y��#��s�x�Q|�
�3Ʌ ��q���T�ݖt��y�4��"[cR�u�1w�b/��K�Ў(�E�M�"4v�4M�9#\�<
���c.�eQ�R����Ĳ[9�y�;�C
�ҵ�*�BmD�А�OA;jmC�m��o�>�O��T��+��rGg�cr��A:�ᄾ�i;�傑��P�ݑ*\�~f ̻��T)��V�1U4�gZ� 
�,a��1-�k�g0h�t�}�G�8>_8^.�Gd������Q�J�"Zu)��� �.�[�]�1r�v���r���(�c����8%!^׀Ѫv�ᮝ�/&�e���͓1
���v��,(�p�eE�6���Y��~1�e����+M���8����FT�9ƺ�T̴��v��ѯ8�Yז����Q#�&��CCFC���!+�G�`�$��z�[�S7&u�I�5{z�<ζ��A�a�����O�����?_��gz������a!6Y�M,h��:*C�3OMa/����!d��~���=�#O�c���m��������N� �5ż=4z͗A}���省>utK�Nokb � �9haa��+��B|�3�:zy̳G��ڻ�777�� �K      p   r   x�344�440��LMN�4204�50�5�P04�20�20�357130��# [$�M��8����fh�gh`jll��4Y�i���@Ӝ�+Е���[as�,ذ=... �/z      o     x���In�0E��)r�I�,�x�O���
�h�F�~��I�:Qe O `�I
Za�5��,'dͤi�*ӂ�љ���g�J{ɕ��Ł��N�3Ռg�f��)C��lg����Ҧ�7׭d��(i/��4n�}��c��C�O:�s�(���/�����.����؄XQ-�H)��'��s�ԍ2�)_P6C�����.u�Y��+�Ez�,�񉎯t#S\��4��M���}`��=�*@�<)��1�`�o�`��|��      b      x��}K�-���Z��< 3�|ܽw�w�y��x��h�� �|G��6�_�1�0h�H��[U�̈|DJ�3�%75sn�֠ίV%EI���������ͅ����K��h��/���׿��_������5���������k�������������g�b���Ŧӵ���R�������Ai�%�I9Z�˃B�iΆ����iW�n���y���^��$|Ts��/����߿��ۿ��!9�8{�.�\�;����d�Twk!����f�W�O����idj��KcEK�s��N�ڣ����t��k0�3$>�.�u��l:�QaR\�i���d�YC.?�4���)�|��>1�����WWcH��^rݦq����WL�T���s�Ha����o\\;����U�g^��<GE�K�-�taAWV�U��,ml�lm��-�ҭ��gIx�^���!�5�6��y���H��>]��z�~��0<H�p��d���;M?��SpE�N��t��!�ڲ=L ٯ�>�����,�aI��K��Zh0��F��ż���i��$�t�p`Ò{t���k�ŕZ�kҊ-�Q����~\�G2���Aq�Z�/7�ZtuV���-���ROd�W�H�������Z���<^݂1�ѓK�
4ę�OTE<Q��փ��ڄ�)N�ve�0|�<%������'oN���mL��x(�cD��L��q�Ҧ�;����p��� �&���Y�������m����۞�K*3��A�B��=\9��G������xWm%��
op��!�W������p�l��Ux6T\!N�����]^!��O�ٷ�헴3�3&�����
1dQ�_�*�\���W�xm���T���볷��fF��#��YVz>��ʧ$o�i�^�vt��@xD�h�MF�/��
�����6��ۦHޕؠ��q�B6�<<���r>�� ��t��o����O��ߟGG�!'#>De��h?j�/75
CC��>�5+{����rB�6:P�j-C4���@?��?>NFv���.��v�Eܮs��zY����c��ڹM�a_?�6]0�R��h@�?
�����Q�tkob��ڍw��6z��.�.i��pn8Hq�i�Io�Ͳ�b��t� ��C�sU_���0j�hZ��B(���O�o�퐀C{�A��͠c����A��S,i�7H'X[�t	�����je�< �+
K�>��xPk]�� ���K怘S�j
�y��/)�����?�q��+jbxד�d��7����Dz�?H���7�WpfQ��iZ��n��
/�怪^��E����w;O��� 
��2BD��F���3���H�7Y�������k��?n鏣.�PֵJ| ��������W+i� h�ry��B�����|�9�.L�.8�5~3�eyh���A ���ZrB�x���͘qH@@h�d�o~.P{`�L���iL��{�`�0�
{�u��
��=]A&���1�|kז�%�/���y�k}^����'��÷e^R<P5���a��!�$�<|иb�;�:vQ_�~��L���u.�&#J�>��i�����f�?(�ln�b�Ro�����;���ٙ1�������f�÷e�D�!��S���
c�e��I�{�K �26�yHa5��&�k.�?m�X��^�F��ـ1�>B��O� b�Z.� ��DR�#� �]`�al���Y�|�u=@`���53Y��N�@p���0iR�I����|	)(��5����r��c�e��g7��R�6po��@7�^e���[3���Uj~M8�О����r�v����rcv�%0+�����#����!� <�Ƥ�A:��9`��i^KNt���*����5�����T[En�C��P�6焥������z,�R��|&����X$îK��=�z?��D1�z��+n���2����r�cl�x5����m�n_�ϡ��y݆ـ9Kx�,hȏ�IP�@��|�4x�2��5 �(D&p7!ɀc�}�=Mb� �鄛��w�_	D��	����@M�;�G��{H�$��t�ʊmŏM�C�Sm�K�2D�׃�n�/ O��wl��	� �/�I*�40��}�/�T�ϊ?�}6bv]�'��@;hda,@i�Sz�k�
���[)��\w@�P�4I�Y�S������{�$�6@v�e���7�v�C��>t�� � y}oRl�O0����ì�߫����M�/�&F�
~�^�yo� #�gr��$����͇���so��j�� <����tv ��ٚ��+K����n=�Ƒ�Ḷ��5��#^��Z�N���������-�6 �
�&�Ʒ�@��B/>���ƃ�Ǥ�R66��g/��g�@�~����o׆ �c���V#h���Yp7.�6�����0��$���'܈�gS��6�M+���e�w�.��vf�����=�G3@��1���7���D�5 � �����o´�W_�5T�+�t�0�8�i:���[X�r5-r(BR�����O��FUb�
�:V�r��v~�j���a�\�B��%�K]�+��U�[� |'����q�ɮ)�B[�Şl���2 '�.��E�A�b`���D��W�(p�5��굉�� � �Ս�W3(o)�q}p����.In��R��m�g�
���7��A��<�D�O���ݍ�+���$��N���f�8
g��rH��	 '�߲q6�C�
=,��%J�RZ� ���9J0�}�����3Û#5��X� ��.���_��*��eV?��	d� ��J?8<�R������(���|^ ��n�fV`�4�3M�W�[�Q�s}p��uA�O8���XuF�+ײ�Z������ϟbn�h9����m�E�����K��0K{����8D�����S�( �S�m&\wc~��&jï����w�jc�Z��J��@�d�� ?��4<�B8�@� ��G���³�bF|r׉��2@V�@����WWo�.G W��S@8�Y/���D���֓�C����]e$qUF(�!��Z5 ���3����و�lG��\F���9:b]�v�Ί���Jk��Q� õ	
�{��`dA@:X�e�VC��it��r�zѓč��)��m��Vg��Ilpm����JP����q���Q�0�ۈ��N2�`�i�p���0��h�˪��qNS������Ln���t�t�~f�:�i���eu��_A�m�}D�ݢ���xLP�T��`�B0�Hwa��D���I�z�$����	f�:��(!O.�_��M?����׶�nY�?U�U&Yi YTK�tV�/р�PWO����S��d^Z��&�,��z=$�U����.qaÃ�L �	�C�KF���dn�����ɮ\�6g���F�p׍�� �y��4 ;��Z�uI��N:��
?�0��h�Bj:F���d�@*��ܤ�+x�펏1�9�?��yv�h��@��9B�׹od֝� Ŭ�F���b����Oz�0l(�����:(  �Q�"|h�Ҳ��l��� ����Xt��	�`�ͬ��2�C�:�Q��L����kDȚ`.������_FH�)⳵�xe�B��:1^g	�`
����efPX�fp�3��L�d�"$\���V��2Y���ci/	>Y���9c�cf����,
�jv��z�.:����&�"F�Y:� ka��۫�gd��C��� ��w�~��%?��G��9��oR�W^�@[�8q�<5�:����Oe��u���t��Y�jA&|Z�#�4^J�|�t�f��ϔvw��&_�˅�,xT�����@�$�ݼ�nT�'L�$j���5�v�K��i�a,��{�듴�xa?тν��� �����Iğ�����
,<5v�!��A���{o��;�� �    g����	�3�� *  /�ͶP����*u/�}��{���7qFp5q�f׽���K����"x^��gE���D ����w��������|@����:{Eb�&��l{ [k�q��	���&&�m�Չ�٘���OD��� a���*�ˏ��+�5�@�7蹁d6�#DG��7���/�G#3� �ש\�Y[K��d��Z]@w��Y8��J�G	W�S�{����8t����aO,�2X<�� �^���0t��鑹�ml)�y��gP�x;(|��׭��(�o�i5&%��i �
ӡ����r��
��]���-����`F�0q}*pNV�9?�l�r���#p���V5�����A�px���Z�|9$D JqSo�S"x=�c-�� �7e�IoO��������2�
��=��cDr�&l+g�H�_N<�!�#�:7�wp���/���KW�+�}v�a�ڭA�'�'�Y���Nn_;�����A��}�7���[�d�]"�����!�R�f<OH/��K>p7s�����2of1�����s"���� ~9Ș��BDxA��pĀȶ�U��W֭�P@��d�8j௟&H���	Gd,tf���L�gc��_�?1�����a� �)�"^�:��=jv
�����tK�瘋��-���M9��4t���X�X��j9�	&�*u�3s2#���0��``�R��`�����vJ�����]F��bA�y�)�䤷��p�6����(�	�2���7˾��t���J�n�����R�8h��z7q�-X�U�q�=��:!^g׌Xz �n��D�[� b3�t��?�(Ѵ}�����?M)��R��|��������A�F�L��������^�vcC&�]��cn��&`�M;����׆+b=gf�{#�ª�p�8 mEY=������A����`���Ma!��02���,�&[+M�i��`�����o�y�r�ӏq���#D�J��,+h((|#�5+��J/�M
��goz��2�dV7LK� ��d0,���@�ӣF�v&_�>�3� �:�s�5gvJjr!�]M�R�×�Ϙ��ɮs�`�����,³��n�����!<Y�9(������vR��#V�̾?k�:"��+�H^���["�)�ẉ?Q�r�21�#�4��hU ��^��r|�S�4�ӈ�0t|F�������8���'1���ӄ23�ga� rĶ'8���������7#n��,�-vdΘXw�k �p?��� �ʋŁ��[&W��D@��f���VX��=�K:�NWV���a� �5Df�9,����s�/G>>8F�;1v�4'>�V���'�` D��^\\<m�9 ��'R#���ux��op�/.��X�/WKE������}fg���2%Tݽ����LPY���A�d�X�,��l����f˶����/q}�^$ �qt��H�3n{�<�lA��/���L�:7�Wc�����SH����os��'�yT�Y�%�A��J�"�� ��%������Ԝ���9�r1��� h��^�c3VE�=@��4�x�1���[�n_�`�$>�U��|� �D _ܞ��3��{�^磖�'�[l��Sq���`�^n�:7ë��l��ԋ��l��`%�0��,�-}9�$òO!\1���@b�x��z���s���E�Y~��	�疖�+|�~-�<�75��oɷj�kz��>~���͵��V
s
y�ɹ��H�~�W-���懌o�
;u�k�s�9������?R.`s0j�V-�{G��)l�ܬ� �D��^���XS��Nd�qն�9O�Z#��C��Ԫ(.U�?z̾��Ӫ�B���ؙw>kpp�t�ua��>7��8�8��	Ec�N��� �D���.@���r+����A�}F@���\[i�gk1�6e�ʼbG�f\H�Ώo���}R��!U���t�LTU���� ��TZ���c�HGk�[�L��l�d��n�4Hd�����I���J��%�>ʂ/[�ů�W�����i*�'W/[2;���uf�������H�b�o��L�S-��+�VK>���F������]�g�!�/�c�����hIB�<������fE}).;��,s7��eH���9�sEa� t����k��J�_=g��4��������൝4�zXX�~�A<�ƚ^���4����9]�т�a��f��ʀ7�9Q����ڈ@�1��J��a����c̃���p����E{��d2KP�H�_uw&è#�q�:ͱl�n�c���}
��2������=�wZ��-x`�=e��v�/%����'�|�9,�¿}�w:�n��qF�t�G�*�^�]�dV�@��؅P��� ��npK�O_p���Zһ9!��O`7#�����x����0�4�R��֢�ZR�xY��8o�x������i�a�� 0Rc���ɤ�Z
�f��σkçb�nbp@%>Cu�W�F�W�S����8�����tO��<![�$6�ޜ4YC���yM������35�b�|�P�A�[�j@}Pߋm�:�6�8nH�gY���!v�lyy�;���P�}SG@ �-2�_ݤ�1[}9Ⱦ~���{0���qmr��u2ϿSBY����uḽ�zp�lԄ�nf8�[n �ާ�rE��qn�Fr������6���p�W�O�	Bx���������,�i�|$0�D�Pj���![ �zxS��TLw�l%9��E;>�#����p-�λ�͂G ]�"�&p�Y�c`�����:�i���ɻMW`�8��	?b^����*ۢ�Q�q?�A�ӹX�RN�6Gd�i�ʢ/��v��꟟�h�+���k�ޝ�V��+�r�G^.9��S�w4|9�h
ᔜnػ�#�n)E�<�ʀ�a�Fm��Jy�m�5��T�9v�aq'�GP&�g�6�����oQ>���G����͘%�DŞՍC��җs5� =��F��f�IQ��
f�4j����u��g�{�dpGK(7������ �Md��������E��fG#�[��G�e��s�n��5� ��douK��������S|(v��%Op��zRE�zlс����D���!�6���Y��r�87��9؆;J�f��~h㜃(�>���^WHA���v;2�7�z�����BT��"��}����9�:�b�w���������#Jg��W���o���m���yq��@������?���0r2y��^��&+��.�R,#���V�s�Z~9H�Ua�^�#ٌ�/7����S���۪Ik��qx�����ծih�[�Z��}��!Nn��sUAd��q8�Hub��� �-N����T��ҳ[Qs�`!?	�����E}�P�\2�g������OÃ0�Z��{΁���&�v r_�v� xc��'K�9x��V����N#=��%��$^U܎�����-J���G�3fчÑ3k�g���|�
x�Yݛ� �r2�X$TJ �<D���T`�kEm���}�l���&����_�6?��Aj�R/��&�D+ ���q����+n������d#��uU����5�� hW�N�G�ve��x��ӤG�xMu�V��LO��@s�׶㊎8ߞ(����7�d��f&��W�v�w�T^�?-�:m�a��5�}�a��';�A��鬬�rװ��� �f�����L��ʃ62݀��v��@7߃,��a�7�_$w딒�,oL��l�j�:%r��y�RH��q�k}p�B���l�@Ŵ�[ �է���+��[���k�X��3@��`95EP�4���Q^�OLG:�(�s}}�ȱ���J�`���K6<]l?q;�GE"F�����Y;my̎��ȶ�v�9�K}{m����x ���Q^��M}p����!��z���r}�Q86 �+��	6`�-O?�7em���    �C)n��7����������J5JBG [�RD�/�-I��b���'�^�5o �t������Br���4�ޢ����Κ��Ƶ�W
�'뇜�g����8��H)v����TD���z�z �5����?
%�0�0ӗ�H���_i��N��:xH �AnV/��:�<�����H*��f�u���s|%������.���NM�	pS��^s��΋��i��%(�敞8�{P��xF�.�ND�.P0�`�	�4��ف�������\iL�ԩ�&��8�^���"��`�f��Ǻ�=���%��p�# <�&.R�xh1������� j" {����] ���bS���|�4�X�����C*	 �ׂB$-2���RM�L�W&|��T���A�|$��݉$��Tf$)�۩A�`��Z����I�Ӛ��,�]���U�OCmۓ1
u���>b������7�w)�Y� �eVי��wA��e���y��>���:�4o�Z�H�mԅ��<�G���������'��M��"�T���Tjx�]\�l-KO��ݞ�׮J骻LT�V�L�|g����ᎈ�p=3{���C�'�9W��QցB�A�J���Ϋy�b��$�ͫU����d�0����9yPY����hƟ�?->�&%\wwۡ�ԝo��kӧu.��-���h^˿�4�'%�+�3��-�h"5�L��%��'���!��s�� <7@e�>=B�vԧt�^af��! ��+YξwL쁨e�� ,��3�{�%�7K#�@䄿���+�eX/����ڠ������O�QnnЫ�(���9"Pi4.lHx�2- V�����~Ŗ��:�)9��0p�)6x���:�$���)������7��'�4��H��zB�8�2����oF@�d�M�^kn��:��w>�4zn��9 �}��frP|�Jz%2�Y�o�=PB�2ݣ.ʍ�{Ql��)5K��D^l �HQxN7*�P�q"��1Ϳ�����o���n+ٻ�#�c5�R2+ @�k��*�k�z���z2 ?R2�j?��}��DU��A���ľ�k���T�Q�pdp��(:&��&����MJ�\�4&�����N)�S�`�|5�	�'d��P/�y�h�\�j&>Q�O�2��cz+sv�k�7����l��'g�0���\�~���_�Q�GJ_?f��Z ��f�%���-�UpV����f��q{�����	�Q��@��ճ~�G��ZRN��Bˋ��#r�<���XH=�~�;��6b��\Wo��?���G-��L/��&{��"���J�h���
�ߞF�.�a��}u�@fS7��8RqT�  ph �/jB�h�BH�m1{4a�[�ٛ��>�Z�I���p����LC\l��B��m���upݜ�T.;�o�������� 0Zϼ��߃�gW�U� 8��;?�V(�d��8s��Ύ�π9��Ͳ[s�6ZC|{L�%!����"sag4r3��T~� 7}SBעr��%T�Ww��J����vxEnA+$�D�; ��־[5�#4��l��0A:��^�ƓHӘ�o����.� �}TŮU��pĠ�^�w��-�T����A�uj.Ǽ�bn �qW�}���NcFt�5���Iv1�z�ᓃ���vT�~��݌�v�`���;��'��A���q�뚹!`v��-�o87̣�k�9cے����+®%�׹��n���s-��v�������>t4�}��V}�)�(9M�p��O4:�����>�oN }�U�^�5(Ey�������A(��@a���M�Q����m�Ҵ'M��\�������OA.�m�D�+JU�e^:zx��)S�������7nȺ�V�R%1�EJ
�������X��A�R�DW���*���d�d5�X݉�*�����i2A��_C��O���]���	�I��n �-s��g�췻�F�q%m;2G��4�Ln�qt�[��{6N>���)�����^�4�lKt`k���Ә�hqts��P�i]
�@�ׯ-7N�/��\�_,]Wݠ@���w]^�ڨb�J� ��(M�Z�1��6�#�5�6WK�^�҉}����E����Y�����f�n��� ���V�G5�\�����ϠF ���ԏ�sJ��&���מ�.��l2B�v�% �E�G�g�RD�%�^D�!xg���t��)W�̴ךoאּi%�r_`�R7k�#����ڹ�9� ]xymg����/2^� ��ށ��V��n���~�q]�N�5]����g�>	� �T������|�f����x��`z�4
�Z���g΂�5�F�6橓���4�d"���↮�~e��δ�'e��m�!W����!�;&T6�_�[�aS�O�˝A�n[]��൸k���ʔ����� H��T2v*Q]�0�\Z>PhO=�s 5�W�`�ֆ��4��ډ�'�;G]/�F���������S���=|��ͽ u<u-9�ςh�⋛X���ip�������s���s^���+puzJ��c�<��B�G�'���Z���c�(|�e�"�X�	gL�*e=�-[ڠ&?ez�!�C`���m�x�m1��W�ߺ`��Q4����1rr��Os����,W��\�՛�'P��vQ��f�<`��=�O�.���{��|��&D�$T'��8x��fhv/���o3 �)׭�
��)������8�p���s{�5��#Ђмzf:���P���Ut��������)��r�g�t�ʎ��k�km�J������={i�>K��nLzɈ%P:�5f��ɥ�.T;��cB��f	�=8�c22���K�[|v�[Ŀ��a�g�sL�,����	&U��F��3���u��=��f�ݲ��\�!�0n-T�<I\B�B��!� �?\�9�[$�`�t.�;����>����{O�����v��k��oT��Z*�Y���ũ�G��d�N���@�=BĖ�N#`P>o&�f@��#�.(v��L`w�&{�(�_��SƜ��|D��'�!��1�?8���'ʑ�%D�!��I���yᣜW�[�떥n:Q���[(~OI@��<����o$W_p��S��'![��A�$`��$;>*�����p��ٛ�ф"ѫ�'�S�ۛ����J�zթύ>ܺ�N��3}w�-j5���[9��a;z��b��;w�o�q��&X|���_�r�W$��yz�b�����
�̓���E�H	1*�?� �2E���ܜ� ����6@C���!�pJ��X���{V�8�ܪ1hsIr)N�X����#��������������^:��G)��nm��@����m׻��i�>8{B�[��W0+P�n�okcN�����=� r-�`��%@�]�Y@g�]��}�inG�<�n_���R�>!���.������>\SFPU�k���ߩ0P�ͳߋ�rkb�&�h��k{A���'V6o�	o�*��ۀCF�0����)���k���x|�E9���:�%��;���_��w;c�7kE�l������q-�x�����Ǭ����� ���2��f�T%~os�ۚ������7��k}����A��pIi�6�Y��d�#p����e���u�z �@dD�N�k����\�VY�Y/��u��	��yk��2s���R�+�����#�����DG��T��!8W�۹϶ΎA_��})�m��'#��Nl��|χ~�s^�[��׳�%�q�6q���,;q�EәG�%b���2е��#ҝ�p���sD�̚��'���ӻ\vO�6�q�Q�ryD�_'FY�ȡ��zR��(��X�Y�<�r'x( P����@��l��C`l�<\Q�|d�$�S�I~;8��}�m�!Z�o'/W4&��:�*��?��G�k�L�ޒ9���@��XO
��}t�@zڰ��� �z�����xU�>��,'���Q�W �f�`}\�k�%\    �wh#:��W� ρGu����$�ì��E���go����]��n��Y�'+�֤G�]�<��0+���ץR-3M�:��gѽ�L�*<��	=���rz��6e��p�Y�I$�@A}�g��=�������S�u!nc�l<�xk��/�Vd��\zf��5��[�����ϳ�Gh�o�{J��*�(�?]��q��p��7rSk!=ip����~s?F�m��-n������m�Jϭy�+���#���u,=Ie�ޖ�{V.ʭb��|�����DJv��|�f�� 5LT��5L���#�O=�\-T\f(�;�ыy�����@�@V� ����*�{~����Ƞ�I�=x����?�cJ�ǀ�	�K�$8�bN!4��oY5MI�WP�t!ӽ�c�=(�I���b_������3 w@F������}��l����fp�O��.�ky� ݌\�r�H���ΐ<�|���H��Qr�������z�9�ι5/c�:)� XlBc��#_W(f���Ĉt
L%��U#H������J�}�>h�5J���0q6��AeyA��A���Q�6q1h�^N�W��M��qH�F&�χ.Sޮ/웢)��?��p�/�ΪM*(5����+���Vڳ@� �W����l �=B�pPn��r���T�ϟ
�r��%p�5�cb5��N�����1��qG�N`��w�;N�zˍ3��i�	����{��;ߘ��r�r�l@������³Rwk`�O�o��L^:usxF�1���'��|}J^F����<ʕt#�;�cN��3�H[<O���?��<C<�)��gحG���/H�
�U#��{�Hi�l9�|�Kk��NR1hKg2��T�=8P���P�ؒ�K5�P�#r<�F��V�l����N�eP�\�c~���R@�P�� �-i�N3�	�����./	���p�\��9����>�*a����*��:T3��D�[/��zfob��7l���'Zy0����k��e ��r;��Lk��d�.��5�_T�sn�m'GD9��S|�(���W���겒�(���c��'�G���3ٓNc�����u(Qw���@z6�d��kԬVl�s��S8(ws�K �u�AV���څ}��zζ
I��qLD Aߌ�����X���"C�E��������- �n)�J;�F#-���9)9̏(���|:�2�g�W
�$��9�L�H�[2���g�)���-Sh�+�sz��WV�A�c�P�T��>?"�#��}*��P��̱HKL���6�vz`i~���%��|�_�@�V�)��E��r�b�p�u�4�3K�ٌ���˵��uO.T��W>k_���^��z�D���G�(�p����7&E�鱔�\9(�۳�X�0�1��1hǽğ�Ċ�g�� $46+�
����?�"�m��yB��5����p�Xf��:J��%�m?g�y�	���z��6w@ٲg�� �݀�����=D��mІ���\���	â���i��]+�ZZ��N;��К8mм�V�)oĴĨ0v8�2��'�,pT�7?_��<�o|ez����
���T��7!-qOW+�Pݻ<"��#�v?ĚrB�s��
P�t�bVB�*#?�&N�(���-~ ;!(0-�)8�\`�_)&M�!���t�;�|��Q���@6�5����T���Ea�+�4i���,�9�in�����XpIz|{/}�����m���%݈ە�ڸ���)��ʤ?�@D =�1� 8A+!�ʰo~g�|���!��+Ӥ���H��9"�Y̅�u~�:�"^d<��0���O��I%_a��3�PR��*�EO�P?�\�$@���!ƀ�� �N�nm�|����H�C~(���S��W��5H�d;��b��q�&������ɩnSMFp�ݗ�l۳�C=�ƻ��@�g0���SG�l��Y�u#�������T��y�i��H�㐣�xv�i��<��	?ʭO�+�\��E\�s��'`*�ޚ*�W�Yn�:N?W�x���l��,q��㿲�d<�r�8�����l��S&Nc���T�x�㰴�����GҐG P_�S��;��G��o�S��Q	�� �GG��F�c�n�� �F�}��bJߎ�#naI�L�Rķ�B�fᔐ D��
�-�h/�Ml��\��Y��y�8s>����� �ͫ�H{><GI2�DY�vs�~��YԎb�;a�5�C}�}�T	T��R�!�Ѹ0~�`�  
�+5D�����:k�#P�U:o�����}����ӡ8� ��e��Ybu��Y\u(�j��[O�8�FW.6������"gG
f��H���U���0_fE�K��4�K�s��Ӳ׉�dԸ����
��SF�-js�.A���+������b�5n,�Ѡ�A���j����p��I3�7�̦i�ݷE�Y;��pWeJ껶��O'f�"�d�i�dǦJ��_�
��I�~K;�u���rڨ�)��atd� �\�+�����x�܀�k��j@�C٭�K��C����)���Cw}��sWNF��N	���z���z���=h��%�����&��|<���Q6��{1����u,��6��］��3�Kگ��`Y}u̩2�A�C[5e���zĵ��'�����`���y�&{D'���2A?l�'18�����T��]�L��e��6�OT��l엜��7�{�BCH3��L���n0���VD��ǃ�Tա���7�+�E1�ZIE�ٲ��W��U� �t�ʍ� �{mq}3��δ�x�$�[����l�~/���=w�� vС��M�t+mrw;�}���
�g,��j����\��]2ia%-���2�R8# �Y���!�d߲��/ �S�L�/���?��S�2���@��#KRr0bP�AQ ���v�Nm~����Q"�?�\'	$��7�����[��Vb�>���#�UVbr�f���@e����d���C��H��D4���t�ǐ��-L�/��ˑY�M�'��q{�A��*�V/���"��Οf��:���0�sD��"�1��k�f�Gm�ϠR\Ȕ����N��T��A�i��V嬒�� !�zP�(pYks���pv6�d'3�׹��qv�*���10.����/��ٸ�/�G� �]U,������@LMfj�Э��H��v���$� �rki���;;�3�-��k���͚���A��!�7�?�#��qxq�M����y{y���J�]kT�_9��s�8|���7�q����n�<�[��'����3T�nl��,RPG/�������Y�t�����$��O1�9�[����l��/���/��ή۳ r6׾Ş��xJ���:38jOzb"�L�R���� ��KgM9G
x����Cbh��~�x��)6�Z5.d�����c��w����{)��K�������C)����V�/n-��X�}wcN Od��}JGh4�Ě ����׎���nj�^+u�Y'�I��Y��k�5KY���̶�n�.s�[3U;(X*np��q[�����������CM��)m&^2 [+��a��Bp)�����^.��'`�)�$A�V��DbO)ӻ��c�.�_��}B���wY��k� �����t�/}(~��./���$��ɮ��7�H��`�D�e:*X�0��(l�P|�uIL|��-��c�a���l��m{��cֳe.M�&]�ʈ��m	g�S*�±��\��A'��`r-��m��K����{�x��r�y{"e��4\e�*���1��Q,N�,�j\�>,>x|a� 5������C.`���n� ���Z��s=aͭ|��{�IV �$�3�ɥ��bW�wA�XqD�AF�A�����֒r�UH�؍��:�o��+���=��uZ�0K���z��]-S�g��f����-2ǂ8r���t��C���Ip*��sg��2    ���=ΓQ�8�GWˬZ�1�,��}%��� ��Vx`�p��"�d-vUx���i�yJ��� �(�ג���=�
�@�Nlpɨw�UvR��s�ԛ������rNb�-,~���1sI)c@���@@��3�����{��fD����tSMc�3j�w��L�Vcy�z�2I�W�<iI碔E��<��Z��pG}��$��K��m\�x�����k�1mٜi���g��V~�@<
��XqU��:AO��4X�,�4m�4�F���&�狃�����^��(��6�+rwo=ࣰ嫳/����M�` �����f���Ҏ\��D;�, n%��N�<�?|��z�\��(e�c�ʉ$&���B �����<�)�o�+@�w�Ħ�Ά�ɦ6[c$��^��>:������I�G�Y�6F �ԝ���{i|�6�~�?������������` g��,�a����&�{�W������V鱩�hK��
w��I����C�>�f���Pv�mz��Ĥ���	�JW/����M�1��ң�� �Ξ���� 
��rP��)5���,|�2��8˙'��I��,ߙ�|{v�~� �j�ʘI���{I(������S{q�rF;9-p���!ܑ�<����l�"��	Ϥ�o��t ص��pY̅C����a���׮�G�n7�A)֫��I�>�HG�lF�QX�����^��w�-QV��� ���r2�?ُ��Nd�zo0�����HT�kM�m]I�6�!��d���
驗�':�BM�	7^�T 1H+��a���C��A����AD��;�f�<�8Y.(X���9�\P6����^O����^�,*���P71��3���8�����i�e^�n��苛�T7�h̨hn�Q�9���X�z��Z����j���`gE_; ��c�ѣ�GQ������il�~,>r�������!�۴bt9�^��=W�''�� ��jrt���X[�Ի%�I�kE8�Ts-g���G�y�1����M��^;OJ�:���ڬf�Д������9(��#�{ݣͭ���>\���������^��	]�7�pUG�5.���\�GY_s��) ������{�!n��vã#��l��hSZo�R\iy���ײ��ur���֘�&��}ĸ�����_.��L�~|�w�������ͮL5�k=���P��m�IHY��as�L���.���S�x_)�>���8��J
�]�����7�dX[Y�k�SO��2EnG��v�ŝ+!7��I-xυ��Q��^l��x��8�Z{
L�h���v6~�u�Se�J9��:����ԗ�~+}�H���@-d0��A]3p|�Q^|��*� M]�P\���"�I���`q��Wg�Y�u������a�������������mrG��ػժ�M�%��: ��no���>��ݔ[R���V���he�G�C�L��0igZ?!sC��!/�4�
8�����ޭ=L�>j��k��z��r!@��!h����
B2�{y�2c���@�_g�W��ʔ]�. �:�7����J�|J_��	w/��X|�p$��[
ǭ�[����<���>쇿���,�p�TY�����r�a��/߃NE�H�����f.!-jkM��2*���X����r]z��?��d��u�r;������TGJ{$��a��-�F8u�։�FTB-c#��9�yH<K@���+k*'�����	�t���F}�#��9'���5�Q �t����8���GF+���r48I���:�".�g���*�Y&� {-�G��9t���׻�� ���*�r)�����)��^������cV��+5?j����4�낓f� �F7�b�N����/i߈��	�l�U"����gb���A���+bu�&v6BR�&l���f��h�/� �H�s���lr`�h�q�"�o9Q�Ӳ�����Nʑ{+�O@��ZF)p59룑����=��΂H�.����^��- 6sm8�Xa/������!Rߗ�͛��/�-9M8d��^�)^<���~�|��`�w8������\(��J�n������rjƋ��r�ϔg�N�B'
�W�-��rЙ}�t�UΠ�Z��Pс�;I���(G�/��#�^�����͝]�T'冁F�R��Je��5+V$W-������/�mʶ��S���i-�|��^|[<{�3���U+.!� J·�#��v g-}A:�
�?0~���H��J-G�γHǚM`�����6cuF�Z�
�s��띵*Jb�A	�D���3��� c�v���	��{f��I��[m2}��TAz��^����B`��R|kc�>L�݀���!ԙ�d��A��*W{v��O[8)g�^�?R��®�O�]��z�$��F�������y�9���yБvf�F�}�>��:[� \Ya ��jy��Dl��A����T�J�ܩ��X���?;���,��Q� E�8�x�ɠ��9 ��p�8�jG�R�]5>�P:tX��r�ܓ���q;)��j�W/�t����j.�>�(�f��;��o m���H<�b�N+���O-�3�DQdX�u��w��)yD8Xљ8HE��o��'��7��\G�Yc�V�nM���^R���~�g�E�b�DqH�~���\�bg9��z�f���r��И���6 �T OU�Г�����q����(����ق�9؅��(_V;��n����\{�1���*;B�ƫ^�5�@*5��6ǆ��XK���G*�;�ƔA�������܄��3�.�����%J�p=V�D�x����GPc�ؑ=HI�9���A1��t��tmf"9�c/�ag�\�$���l$�&tυz�L��@?�+�[���M�vb��rA��:��$7�yn`P|D��5`#,���f;�����0�X x�|��RM*�Q�;VE�	rH�j3�˷�g�j��>h0�n�e�^��
��֚�o��D/e�cYP�`pI��&���Uqg��rﶻZq�g�ڂ�nM9u��|��4j\hC�}�3����;����b���
f��_&&��%��.S��ҽ�+�#��A����xk��>T��q�Z�>ώ(�Y-�큧�i
�-�^����f΃NSљ݆ќ����'�e��R��^aJrٸ"n��r�=�T~�t�0��0�|������Թ��g �Qˬy&����h����k;�������k�P|����Ƙ} O7 �b3�����B�z�۪��:=��SG�i�}��$�"�r�A��[u��gK_5bj���z�S�yp�#��#�"0��$�ʂoޑ�|�K����0&\�ge��"(<��[[�:~�3u�h�	U�7@�s���RP_�z��>�P�0ni��������'�^o]������)ug�Nv�2�V)�?B*�V~N@�?V���v�0G�\���k�g�G���W@��ޞ�T�����tU�YWV�`8�R�A��Sb�p��1!���
��M�7F�n�fn|e�:�s��h`�����̡�p=	��l�Y]��K�xm�����d��靳mȗ�6����%`�9Rě#׬�E��ښOm������W�5@e�`c��ZfJKTc�g��	h��8$��\��t�qh�U-ɧ�6��n�f5&y9$�D�&޾�H�9�''�B H贄kκ_{��o��%��%@�a܁��i(���6��#�.o�M�$ ���6A�ó��S~��p`�A�����֛�A.�6^�x.q��;�=;�Q�3��aD��L����LA����s�I%]-�Q�_��u6�+���(/]�v A���^u�jےvrƝqQa�� �,�%��q�[�D�_\�3�U�$�����j�� f9�Uw}{"�	q���g�H =�1dW}��N����O�6�x�;�n�7Y��0��g�� �9P�m��JQ��:�2����vMBs��je^    SQqݸ��#)��+8�xxO��]A��W?��6M�E����7ɲ%Wrl�sQ[��KvX���^��'2���_MUI@(-�J�vΦ�:<g����5��� ��qڍb�>�������P�s���T�L3J�?,N�3	�'ԫ�,�%���n�!�ĕ�EC}��D�(=7�X�T�e��(�m|E�Ӽ�OV���C�O��ج�ݨx5
��f�]�xݽ�g!f����K�5��玒��P~F�P�N{���s����.�ݹ{w�^��;�P�x@��D���3��zF�OF��z6��.�t�]��xb�F$կW�����(��"�����ʙ�M���RG(�����e�ሒ����m+�(l�mpݯ�V��|TƯޣ��e���*���
��x����Y�gk���7aw�:��� ��b���U���;�ɉ"N;vp��y�!bmt`_m��>���}#��= a]Cl+��;f��Z�m�D/�q.)��<�ng�e�ʍ���:;ѫ�M���Eޯ��S���X/uM?~�9H,�s��
�$���~�\��k읶e��螗��Xc���,+$[�w��v}������b����Ё�������V�����`:�vzm!��"�n[x��yA ��l�
7�Tv���B���������;4T���
�����+����b\Q����^n�C��yaD�g�~N�F���O��?�f{��[{`�d�]��O��Ʀ^]SI�ɤ�ݛ?��Z��L������1��|�f.��+"�8�m��V����#���D��E�[���MUm/.��%��h����Ŷ�ã�j�*��Q�$-vĤ͘��H�NX��oyN+��)�[2�[��;���oG`H�'
]�P�� X����J�-�/Q�\��/O�^��Q>5W"�]?+��x�Q�[�V�=�%�?���p��m�Sm}X��vp���[A,��B>�x�R�z��W.i	u$�mXo�t��o���"����>���VM�gC����H�8{o��.Xf��Cp�|l�]L����਱�����c$1���k�8�4��n�����K}�,������n�W^��P�E�s�4���	P���T$�jk|���˯��[���]�����^#6�B��O�j[�P�/߆�D-��f������;W@ذL�vG�޽pu9���,r�\����c���e��D�F�
������N�y��6a3�I�<p�$�fmy���k���3iP�.��:�2�Ŷ�4�+��s��W���u�޷L�մ���:.˴LQ�Z�M�����~͹��֦i�?+P�-��7����Pͯ�
Ô�G�o��l@$Y��%�u�>\T��8�%�H0ʃJ�uy�g-���)�V�,�2�K�صF��j�9%��6�^��k҇�#���D*���D՚�ߞ(����ẛ�&�5��s�)Y|�|�3�d���7@7_��v��Y�W6h9���Bs	k�n^;�E2Ӗk��w.O�D��>����\�PS߯Krs|���N�W��3��1�|�~��cj5�����HЛE�Mmo:t$�d�f'��L~��sV�*��=�9�������ur8&��W��c)��'N�Bkf��etT�g��\P�-R�Ev�C�":tf���^���w��9���r���_y%�9r���Ԑ��T��[h��^*%�/��B�@T�3E(ࣄ:� Y����(����hR^�W���_��e�
	�N/Zj�4��ŗE�p��(�\k��D{�
�_�T���dM忙���O��׹^�cl"8����^�:
�}�v�Dl���v�_��gxxOBV�)O"B��hכ�Z��~7��W~�z�)�3JLd�,�o��t�j��W/�e
J߇f��i�a�'�9g�A�����ט�w�GW�б,�����ƶ����p�sӎz|yW�_�͞����t�Ɣ�M�c*-|#�'�7X�k#���+�#�Px��\��2�� *nAo���L�GJ�F�cDl�O�8�ܲ!�>��#I��j&����q��Ψ��v�:6�R+7g���u�.��w�)�Yǒ���L���>�f�n�^��2��m�$�#���	��IH[�kթ�JI�-^�Em����vm���6 l��O�^MGǮc�-�AԤ��ڶ���v���(��������95�/�xp�G��3�jS�e��u����u>�O����#���rC�:��/�h�\ ��-�ܵi<:Ym���ipd��Y���FZ��s��j���޴�O�JD�2���A+NS����?��?��eB�+׽S6�.B�5�I���[Y1�Q��c����_c����x�iA��Wڜ����2�b����>Ɩ�mE���^	�VOEʜ��8i1�^*?�jr~m����5���咗�0��Lw��(U�g!R_S�-Y�x�cp�]R�t
f��+�f�g[3�2B ��- ���㍃(����t0WA��*�����z�^x�ů�"���t����1�K�Q�cr�9��w�˴��?�D~:�V>�{#�T�h��
Lx��&Pv�Y��ݗ��b[\?�fҹ.�J�Hm*��	�r�ę�?��{!��X\������}T�6�^�]A�qb���w�y]�Y�O;{U�M]����$��Q!�e��S�����B(�b30�fD;�M� �8�5FQ���)�߅�Ll��B�8�=�ƻ�nG�3��h�E�l���uW�f��zZ$a8:1��׸�aEW0������&q�p���+�l�S2��t�r�AR^1��%~���M,���:TGa��D�||�i�Ut�|�_�h0ﻩ�*!g�h����S|U<}DQ?zn��e��"��ݝQ޹��Mby��r��B�1k#�S�����"�"�wM�Es6�ڈ�֏�q5[)�̒D�g��^�=�Ku���&QP����fǧ�����/1�����k��^gc�gL&�+v\Vh���2ѷt����*�r�*��\'~�H���x�hB"y��w���Ɨ��C�`ZJ.�XE	'�(�:�0z���m?b�����,�M�k(+�lD��F�xl�c�]^���g�*bwS� �(m
�"F�T�6���n^��>#�*�};����3
�,�ީHA�X�GH��
6}�F���+|}h�z '��;��$�K7J6����e�t�̈^�cTg'�{��g�IP��g#�:Ǭ��=��~����_��2�K��c�Šr]䊤�-��e!F �a�Մ��{;<0��-�j��
5áe���h����E�ې6DqQW5ǧ�c���˺�l{�t=���s��P�L4�4���+m�@G�ׅ�M*�]5��9[ց�	�h�=!(�^G�G]�K �G#Ek�D�j��7<��E���2��9�KHs�����.�alX1�3��̓�DL=����J���7��_ރ�5��СR�t��B�:�id}�<s�rF�=r�����QN�kl&�;p)�<��E2��ep[r��ʶhV.�W{��x&"���|b�}Ɯ0�y/��
ֶ�7Ҏ^�l���a��yh^_[f�5|zA5�k]��g3:�Yϑ��!`Q��z��;!��b��XE���n:�9�ANJ��z���:��F�/�fϜ����ÿ�eJ���k=A[{�-��7����_��(��(�_7횴�+ j(�"I9f�i��k�Y(�i������1hl�����VXMV1l�gӗ���"p�*G�哞c<��BP��1��l4d�qGߎ^g��� ��٘[F�ҺѺ�	/�G���OQ?(�����z�epi�.qS���۵j3�3䏴��R�=Ce;T��z�}�2n�{������_g���ec��͋{<�*���Е-���D���=z`?��t�ݝ��U>}\��"�cq]w���>��$S��u}r�N�k}�y����a��'�l�+5��Q�V�]�$2�.�(-�>U��}pE�0�iA�,��a".����mE{A�GXÞ/ak��T���I�P����mݟƼS�    �%l��VW�&�oOt�ڻF�k�8�h;�E��ӵv�V�"_�5r��'1�xC�<�YVD43��#��i�Fv�I��W��AJ!
T\�t��Mz+����4ڐG�hf.�p��x��|�~�h��9�E�fNKC�>�+�z�'��tq�U�c?~��D�B������_��5t��l�B��N��_��
8�Y��L��wa�xB.�y�c����d��P6��sxz�zf�T�e�O%�����f3�t�Fl�B�����ާ6s_v����'�_^͋������k;y�fQ߬�0!�?����#������ŵ%2��>6�vC��2[�ێ>��Hkʵ������b0Ň�,L}v���.l�e���]�
Z��3z��	8:=�-V��AH�k��u�`�s-h5B����*hE�0*�DQ��p$w/OGg';�����:���)'��mW~�k�n|joh�O��@��2c;1*k�ޠg�p��EW�%�Qݴ�K�����.��&G[CB��#^ח�a�w�/���d������7�$JhH�3kUQ<m5����_�2�t}�ȴ[�e(�qHs�+�2O�:3<6��	��g�:����CW"6���8~I��}{��/�xe��%W��5�������ajbjמD�sǴ����.dQnغ�d�gScgօ�f�i�4���]ǫ~ۋ��u�n�yYj���P:RRP{�@�M!w�O9�Og��k��8��
��<�����Cը;�����Y(~�%J�7���3Q�!��E9<Un.��f'S�_�k�Ag����C!�=a�B���W��ol�Wgc>��!�k{K�ז�֓'��B���Ю6��u�� �O40��Jn���|�'��^������a��R�Y'}�1�k��r�μ�O&3��D/�6��W�L9���w��u��⌥���olY����-j�h�~}��o��۵:�V�2�xF�#eW��d_L�	�/�_�H��=f]�m�ȧJ�T(���\"]�%��,Gm[.�|�і��6s�8z�E	=(�Y&��l�%����T ��^���*��>������'N�{���L}<4-�=��ʉ:��ԅ���X���Ǉ���#��j=��(t�6�W.q��x��v�b8���b;oY�r�)x�����v'�f�G0ۄ�x��f�W H�ߗ���E�P4
�;MN���jѯV_���km�뉱�z(�m˞AH$���uk�vg�pt�D�o��M	0a�8�}��
y���_ݦ�W|�dlo7�pv��9K��
�&q��m�e
����ye�,���⊜��� <�g�t��p�T'#QZ�8\�BF�y��v<�t`��6e3� U��P8]�?�^�#։1�a��}Trs�뇷j�i�B̥�E���U� Zя=*q�
S�z�j���k.F>��;{uKm�>�>�yt�q��cj���3��&�۪�S����3�1z���Gλ��P�T�Ć���߹� `Ȕ�-\��*�gz'�����%<����$�Α���W�*�9}���|ri��gc������Su��+���$��}���~J������8�P��=?+	t�-�/����X��:V<�)��"XѶ6�0�R
U����O��>�u����\s�Κ�����Y��A&n�c��Ѱ�"�a;��R�WvK�)r�ÿ��q!F/�Mw�����א��"�D�ᗷ�)Oo�+�S���c��E��FIjm��]̴v��e���ڂ~�k���2�p4dx�mݢ���%������>�l���|� �T�V�ѫ���������N�]Q�ݹA*��,�fLh"�I���Dd��ھ���� �8��&�����?���m���������c	�EWO�7�x�'��%�O:f�-9����W�WEL�7�S<׸ɼA��K�TA GQ�*�c���9���H����{K?��c�/���B�=���[s�<�,���tLi��VKsT*J;�<��	a��L��:������کW�5#Da��dJ^�h[
=z�`q`��7�PP����VS%��%�fT�[��n<��儑��'j\+�	n����ӕA�{zkCd��6|+_��g⭝�U����TV��q�1O#���m[3����{�pp���r�3�;��?7�ml���x�F?��
AL�~�Ϥ,��:F�Ga?�O(���ҷ��?�T&]���ͬVHm���2i7��q@l_qg0����-!��n�H����tͼ˶��+�}�p�/��ءt��-c��hu+��[+h��C��5�*����=e�vNr(�WZ�O6Ո2*x�e!wz�ڮ�'�B�xmX�h���r�	��?����p{��@i��.��$<jL�7�҄Ĵ���N v�h�>�J~�H�'�,-�a\	�1�+}�㣛yc�B��'�No�9D�cC�۬�����=��ُ�_��K,B�}<z�c�Q��(���-A/�a�����,��I�X���{�E{U��Y{0'i-ϑ�t�~*����&���?i�k�<�7�l�V	�ב�|T1��)��pH꧟�&&Q,q�	)VL�.������_De�#s�=��g�k�g�9|�h��� zr�պۊ�K���ier�H��aMk�D�jo�w�>�Xćʳ! 8�8���k�Q!�_�@*���{&�R�(}�0���Z�)B�%��%<�ߑ�|5�<����7솛�ju<Ꝼ��5ⱍ�1��! m�@�9j���<����������@��s:�N?�!;Z�ܹe������W�/��΀h��^y���dbB�d8�6��]a�:��+��$���j���gs%� l�a�������\�{�糐bNW2�އ>���r��)!+D��s�b#�k¯j�1G���E�5rW*볓��Q��r����ׅ\�A��:D�iE�|����ʀ���(tz�D�ַז��������;3�Dz���!�c���/*�ʑ�W�-3��kbkQ���ܸD�h���ߡ���gL#�{鐾�a��-;S�pV��gϽ��{�����B��$���Y��{�C_<a��`kO�]��.�̋C̣l���.���]|<
�Uzm�^����kj�؟�ݥ~��z���,�-���RcEmϚ/����D���ne^"��j�3��V��s���BgD�j�E*��|��:1bt�t1g/�e� � >)u��Ƽg~Lf�1�]��$1�]݄͋��Pڝ�`��1o��rT��D���B�H��"���a��+1����0hB���*Q	���.܅�����6+�V�B�С��r�����i�w��fs�	\jR��.�Q��7�Z���z���͜kI&\�E��.{�h����ؠ6�Z������@��"K���N��:h�z�o8�e��g.�R��p�*`���m�qi���3i 
������� ��<*p����2U��q�����A��\�aW�@�ӗt��y,�^;hn��z}��7J+��'�왷�x	�7�9ڬ�_4W��H���Kg$/ -��C���Wʉ�U	Y�诼"<�D'ѶҘxWu�w#�\sv�� �r?&f�o�'�zF���|\�#�Z,��X�(Nyт#w�?H�Z-6Z�G5�.��;��R�P�s�;���m��Ho�Ӯ���B*�g�]qr��?����֑N�#PR���I9�ɅdpPq�C)O�G�0Ƽ_�������]^[A�M�v�2>� �G!8�����i��O�����'��Ee"ר꟦l}1��6M��~�2i<�����%��!ƅ���#�����pز����Ƨ�-�k��N�l$4T=��Gz�"a��D:ٽ�Iɥ��z��b�0��
��:��g�U�܈;}�,xg��/lID����׆�Wm�3�������eG�D��uKT�_>��8����:=|��Z��.es����h��Zk�Q�J[I�8�[���}�3J�[�+�Y��o<}�̤�ĉM��h^���� FT]B�S�Q��:aAl:�b��5���oT_9��&3��Ȥ��g)�    gk��:�K%���k��m�|�q��voimF�Z/Kq2��$Nq��p�	�˂t)�N���"93���8�'�D���czӗM��$�K�6�=4GH0M� < �)E>:8��&�ؿ<�=�`E��$Q�0w�D�xEmk��2�7��+���8?.�_��<�!|
���3N�V���0쪯�C]1)+
^{<a�;i��T�F��/7��l+z�N��~Ÿp�]֍��S�1x��Y��ң���W�j�e�3
�,i7�4r�����3�rE��������"��N��ީ�Dz�1�Q{��V��"���k�_��;3�.��J%C]mڢ�r[�9&ig��_)�8@`���[k��R���B`R�Rsm	���6��@s^����sV�r�'mFDD��H�FkL}��4�X�\��$��[}ı�4��]n*���LX�� 9�+�.�sS�2f�[��t[�D̑�?c3�t��t�?��|]6��p4�����=��!v�E�,�`�GB�捵��Ԩ��J��������[��b�9�G��^׾�$u�G<�RN�6�%H]�N��^� �.��C��>�m�@���{-~��	F.z�p9m����i0w�fD���kʝ���0h�����h��|Y������~=|�m��;
[to�(ws7أ2_zq��#I��9�+��*�����d��+���f��b =DH(��_PP���Z��q�.�[�$I5�a�ü^�YCֈ��M|4�ݵ�b^iSX�������Q�]䈘do]�7�8H9h�����X�wT2-����}C��Gq�wZ a�G�&rX�E,xv���c7W}�+���ڴ�6�j\L����Yc.�GB�_���쯇�ޠ�a��0�ެpM
�-f�G����n�z٨Rb��~;i�+	`Ě�5�����~�8���8�5�ձ��
7C�B�ZH_ɍbi��3|Y�r9ac�j0'5�S��"h��w�>���#��g��,b��i��~j�$=�v�aH��1~̬Qk��kn��/�S�i�
5Y��?�	u8�yƲ+L9���"̚Ul�u�F���!��:n78D�ؕ��(�yw9���(E�(�+�J��1/S�e���VfZ{�?^���DrZ0����VJ�	)S�lx�l�q]\.��o_b�656h����J�ذR���A�������EӘ��4�t;������iV �t<���}Y��L���r�z<15lH�5��F.!��7���	�++j��)�����U���	Q�jQ,*�k�Tw���U0�c��;�V?��z��P^�ۂW���4�LOz��+��TSPN~�K窅+j�<f悏E�q�	�=���}�|آ��{s|�,F�w2��0���PbG�Q]��7+��i)��?�ic�����H�?9��]���
Ӂ��Ě�<7i=nE�qf��J�2�M)��kNঐ`����p�4��'�4�(��K�A ��(Jߠ[|�(_�	#��ܶG~~�lk�rݯ񄶴���H+���h�8�DQ��U��]rY�Ԧ/�d��t2WT�ϕqExOtj*I{CW����r��5� *�^�c��4�wC��W�{n�͠,�JG�S��b���ʎ�˗�<e�^�6�ϣ(��^��HdZ���}*{�at4e�c�*��x5����}��Ok9~"�Ww�6
��Y��*6�Y���23MK��|t8�J^~O�w,ZT�"�Yu>��e��&8~ &��R��iZK�p�c:�F[G_�����R�#���Q��"s��=�~�r��P\��
Dq:�6T�w~%����^�����m���Q|�"�}�kM�=�i#@5��l���������+ln,�0��-|�/l����vz�EU={�J��~*mn#2EF=R$i��47F\�.�͌I�Cz���{���f�6k��	~�R�M���ꉙtlyu�8�T0f;Xsż�x�6K�^MK�~&ډ�����)4D���5�@c{�ӵ&$�?"�WS��J2(� G���u��b���=~�n�K�h��I�ދ @��K2*,����NC��)��pQ��v�E#.�ՈzF�tjQ(���?
؈�Z؛����Ul7q�ǭTG�/tF]15�/� MVa�t=v��^
�zO��	/�H������ư��^@�#�}��/)s���a,���G�*a*�/I!@�������p}�����X�U�d_�w������L�|�����|��e�^�6�ǂ�:��Г4B�����mVi�3W섃;#��ǔZ�C�LL8.�X�]�����<j�@��H�P{�Q��(\���?��Ӵfn��Ez��%�m�-"VaV��+;%iP�U��r~[d摻h���l�0q+)��z��-W;m�{,H�h�莻!��������+�|f�.�����Uz��F��K�S�I���B'������DMI��3G�oc�B\��bG�R���)��r�N��ύ����0n� �m�u�`����g"8�|���8<�Y����`�`�]b��m�i#����1m᩸<-����S:�m��wF���Eo�<ݵ/n�}
�����B'��頼껋y�
gh"��W]�c�=��Q�W:e�(��+��o8�u�b���m�,Z�V�G��0全1=Ŵ���os��Ĺq鸌i��)0�lOSniО��h��6���j�WQ���1�� ��b�{���(��_H��
H�;ٚ[�L�1@2(�����6㗅��D���[�ƪ�c9����+H�КU���۶�ld^�J��*�$%N�@�j�.>}�6i���(pFth�N�(W1Bi�& .ٓ�S�=
��9����G�,|��J��5ˢy� C�t�t���ج�����(G��"�|-�4�h�t{�v*P%�u>����La�^V��ǈ�\��L󶙢��t~/EXS����s�v�6�;3m���+>GS�ܝ?7`�#T�qa�:��UWq�A�}B��\�Z{A��i;��
�V�2������3����q��4�b?
���';��$����S�qQ�{�ڝ��3,1�:��u��u�ցy�#e�0?:��=����c:7����"&Q�����|P��K`�SX�kzAݦ�ţ����>�׀�f�붨��/�ߙ8X��_oi��LyU@�B�������nO�+Ĥ����vE�����7	���T��v5��V({V�t�b�	΅�T�K��/av��	n�&���QJ4�����7�I��h�϶V���B5; j���vSAb?ND��h(�)����;��C?�U�Sj��43��br���O�8���V6���s���KO�('�1�_�<�Ԃ��P�Z&�3�,���Ԟ>/�w���_7�4�C����xW�4�Y����_K]4_�\��9����>�j?���pi//^}�߭����W{a1!!f�&N��ࠡ�m��u�(.�mG�ݸ���'�G���[x�P=��i=ƌ>�Xލ/���3�L���Kd�$2ڨ�QyO��y|;>4G�8z��7z����#MP+�߇j�~�1�����������z���ʹ�v�>��tb���Ia������E�
p��0�>�@�+F����y�I~��D�*�9]T��4rw��8Z�F�t��:���+��鯇��t�?ܺC��(�[;A����}�q���%|�a�w"rs ]�/�+�+~��t.WD�ݻ�g���MK�ty����:���4Y�*�7+=����q�5�v�}��7JBO�4�jkt������8@���[}���b.(�i��{M��V��*��Bׁ��{�R�d�pDE�D�69"�í>�W@��J��{E��������Gx�)eE!���#���@��R6�޼�P+
��Y�t��jF�)-�%��T:K}t7�������C����@�(�|;���
���a5����Z�W!m/v���~������?������߄�i2��4U}�G;ś���[X�,�"�}��۸��{���)�*�=��ܹ��r�իl���H    I��.��3ԕAv���<)���������'�^-�|M3���1x�c����ߩ����,U������l��o+N�t�xԬR��M]Qi�o��,����OՆ�8zc��T���6���$e��H��z�Ll��>*}��𘰻ҿ��s?Kec����r�;��s��<%��(�o�D����T�4$��;�����а/Zd2&��PB�1+ڦ��D��Z�͸iLMʮ���v�̃�&ƚ�e7��Ž|*���|˓KC{�<�r�$�C��=��\L�u���[o(��9��L���E(��	7�v�_���J>��\m�h{�8�8��'�_g���}�;��%?�c�������#6?��n��]v)�־,RO�!��ҌIL
��y�xA��
�9(g��N����3�撿vl�	N1�4>3���L�Q,e��Bb"�S��;��pHPe��a�h�	��"]�~9U��k�H\��!H��jb�>\s�om����=�����M���N'���F��W��Ҿ1��vk.g-��1�i������)k�:rPNm�t)��%�̹π�Z��P�߱j����io*�I��G����8��?���P�N�K�k񮈚�e���� /�uL�����"�ȃ	������ԡQ:�B Y-�|RD�f}�>��w��T���
��8��e-�i�BZ�)1ء�5�l�^�"��CV�65�D,��h�9}����eŗ�,�O	�!~z]=)ڝ�栟Ġ��2��9��U�oKy�R@� M�����ԝh����}c�I{c���b=h(d�K�q*�n��1I[�:�e�y/pMB?��z����*mJ�Ƌ������i�H�ץ��d���Ŭ�l�N�|T\:28zǉܮ`�/�k��s�܂�9�Ci����5����Pdˮ����L%y���8�1a��tʕ�S����K�˻��-��%ҙwM1
�]����O����a����U�q�e����Lד��݉�����6�{��{7�����`��0#�uO���E5��P�ĴmAj[�ӯ����XP
���3A@;�$d[��W���Wg�k-��ɿ�	8 ����Tl+���W�Ԇ��֫�f��!�|[ʝ���b��6�;��ŗ9���"��ѩ"e��Y&�]��g�ҵ�P����
3}tZ1�L���E{�ſ��G��#���Us������1�6����	a��|e���0�Q��u]~5��=���N�i�"t��0��巅*��Ip[�2������˦����(M���bv��>H����@�����y���xJ�$�����_M1���������0�WZW ��8� ��J�}�L�s�h�)��S_�b΍6]M�Cbvi�����^�~,��y(�^K�P����P�s�柏P��V���Ě���w��� ��:�=�䤸7����	��t?SU���Ay�g�m�A�) 9�(A����:B�) ���GRuvG�L�szl�V0��Vc���$����็p��SWeb��`naI��jӻ;3[B�3��r�N��3��Cu	�e�t:�2C�����1(�¨�0a��9�ʡ������XWmV|qdȗg*=��[�U���!��<5_�"���q���Y���3�}5*��nKǀ�F7Q�[/N�ͼH^$~��䝽��vx�ik=�����<BO�T����n�pt�D��U�X1�H��gB���N�,��U����㏆�=#�Ǵ���6�0f~�@���B����C����y�~!L_���^6��vf6��4�;��p۾���J�'
���^ؗ��F"-d�?N���lӚc>��B����]H���dzBp](��Ws�K?,���3X��B��R�EF����zb��h��9_����V�W������$���%ZI0��0�i��t&�����ꎷ��+R�lGC����4���7K�c�l�I�N���b;��x4ӹt8�|�=�&���H؜��c���Z�Kܣ<�+�P(�&�5b,{��7X���
����g窌�9���zcg�(i�������H��E���}B {$Efb�N::&Lm�1�FM�_J4��H�g]��l�>�C!6������ۼ����� e��b�Y�2��:�G�-�7~��zyelf9Oe�=�◱v�S=�so��=��v�`� ��%���5݉��:uFE�2���� 4�	�m&E�?7��E��"�;XB b�Ӹ@U��*3U}-��/1�h����z����l�ȉ)h���|� v�;�1_�:Q�epX�~��Q�0<B� �A:�H���W1�6A�8��ke�ؽ���4�t���[ld~�����؛�r7u8c�T"��	HpE�������/����)#es����
@���H�0hK���k����#�?
W�Z�g::��A\@�ϗ��w�3m�-�Ѿl��W���� �+|fz���"u�#�Է��m>��E�\m�"~�S�1$��Լ�c���%��ra?7�#��I}W����k3��}��k���h��A����������{�<|�7���	x}z�c��o(�z��.3tb���?�x�*�=�w�Y}/S�p���I�޲�����L�� �GAtZ;|��<oyq��/�>�(��������Q�g�ˣ5��	z�Z�$�%�P��B��U^o��Q�
:(U�0A��=m�/h<&Fg��n��֑D?�e.�M1�ӮH�}���Ԅx�&�·OS�8�-�qGz2�nkb~���V���ڐ+�N.^/�q�X3?s-�ߟPV��tQ��%��*��mF["<�ܰ��Ɔg\���ì�Q���Gu����MF���q��(X%���֚5|	:��G�$tt=�T�f��YZ�����p�A\l�����+l(W��M��WS�84��3���u��͸oQGA;No��w	�v;�v��Z���hlL���k�[����e�H��Rf�E?�4������i�5��a���Yr������%�!����&@��i�G߫/������c��&�kD��h�5N�m)�e�i��>����#��{���-��v(��&2M��Ip�'������b�Y���˩T�h�ʌ���gV�-�0���+�S��?����!d>�9�`U��'���ۢ\n��^Ŷ���DD2����Tɬ4۞x��ZG�D?sƥ���;�uN�O��z�+F#��l�6X�<dRY�Ά���g~4����An�o�`Ff�A�����w��ӑ��O��[a�kq�M�,-	�Vv�wO�G��n�z������f�3����ze��d���:Q!!�i���}
?6bSw���^4&:�]��Ϸ���M�y_���z*���r�2�߇�U:-�P�������<��ɿ�J��]�����:��
tO��[vZ\Θ����~m��	+����&m.������{DG�h���Ծ:^;H}\����vO\�6�I��"�)Uب�_;:�hWŞ��|���O��LD7�I�2m���o34싹�f]�k}۸�6J.�*��������v���
�� F%��BF��g���_���BL��E�n�O!�rW����(Y)P�<ݘ{
��9��?��GhT0�y�>]�*#X����6š�e!ǭU1���~˥M	}��gĵP�j8��ݨپ
��T���9:׷ʭ��Fx����4�	�vc�\�H_��;� �xs}��M��MV�n��G��X��)���m������a@����đ��!��WX&.����Ôҿ=QV�����:�m5>T��!7�����y��^U��+�^E���t}]Y����f���C�W\���B�������oM�*��lZp��(�	�3u�b�"'묘u���ٵ��$K�1���y]�~z��D��3�\���h�w됖��8�-fRZ���h]g�����Tq`%QDK$���1�,�O�ݷ��3�-$w�!�%*���e�Y�ғU�k������T��[�kO��V��cT�;�������R�����n��2�͕A�}�>�n\���yƶ����"�� 2	  �y�m��S)~3�B�go�ŷ�5�K ��Q��L_zi`�-`��=���T>��6*������{I�eܤ߇�hL���|�n@����o�%����eEP��,?1��s������o�&�	bq�k���B#]�j���9��S''����u����ލ�t.D"�Vl,�[i�f2���"�%�﫻.��n��CҲG�8AR�>�T��.�`����4�`��_���Lo����èuX�&�|��V��"��#Шe�Fkr����~�=�)�3�|�4�����E�4��P"�w*J�o�3㑼�/��RED��D�:����%|�i� ]��람S����Su�Qu*X�wM}��v�L��/t�3��tZ�*ʓ�0�&҅�+OL��L`\з����FK�����H��J��9J�_��L	G��
V f�����x"w|گ��o���(�u�Y��� &n.��T��(z+܆�- �H>9ߏ�tg{�4|8�)��O՟��L����x����蕧)��+���z|G�5�#��WN!�����W�%ի{��I�����&C�����[�+��*Q���Ȍ�������!�`�%���C��#��n��~��])< *��4ܓ��_�?\N��L2C!ۋ"��@�e�#���=��}�q���GA��>����<�>rg���9W<��zTZ|:�G[f������n=�/�͝�t1sg ���[�bQ����<f����<�p}���y�>����{B��hf������l�?݁���^+��<�p�i�X��'&�QP-�ek�?���1��K+��*���re}.��˲���W��?C��]�X8*ʓ��f�ѹ[I\g�8E����y|�\�׷���eР5�i�7r}ܐ��å��^��c��\0�M�g�����Q�O$������+\��ܗ#�qƑ�6�l��y^��n\�]^(漪��Xk0}L�[az8?�Կ��X.���E+����i7b<ײp�9P���1E1���	�V�]k/��/S�*�o�m]�����>�Ԃ:�ό.�j�\޵Èl�D�ן�V�V���Z�� *B��C͞n}�r8!D�<�nө�}%�Zt�S{���
��n���X�Ie�b'7pHE+F�nq:B&�[��������9
�_�++���֣胶�Xia0<�W�{-tzi9=΅k�O����*��>�hsM�������,�(T��Jʻ����4��>�U*򥸚����4���ח}�80��x��֪<Y�2����B��Lb�4�r5κ�L���ӎ����	���Q.գ<�J�����}�x�ţ�Cy�s
9��j��Ľ���6g��9_�.�,�l:��<m�h�v&�F�.������FqeDe��p��
�z���`�4��JOG(�5N����w�^H:ml����;q�`������1���/�[��i������w��ᑯ"�imī]Os��7�A��qdi����~$�����o����'\�����ʙZ�F� �b[u���a:��/(�q��Y�$l�ˎ��#��D�Nn.���KOC�CO������ׂޓ��H��S�=��aR��b�H>�/����f�ws[�t>� �b��_^�q(��/g�;=m�n�J�� =�\�GGJ�Y�w�������>~�:�!]��f�*�T܍a����Ps�S�����ҹ�H��i�^�Tl����PT8]i֡��_s��B�a	��[X�So�k��r:C��u϶���x�&ґ��?:`�-�б>���/��]��&tc��j.+*2��2r��Lo`��C�w���_��{/��}?��,۶g�:>e�ƓC��[��Օ-F���~�1NM�^݈�9��s�P(P�at��sv��9���ŧ{���z̾���K� :1�(�(�"��e�cso]�W������<���A;����O_u8���~�k�θ�st`
�e!v����Ό�
 �U�D������f`ce	Ng��CV�,�µ���*�}Å���{��G;1_�G�v�=W���O�ک��-~��dJ�x]�o%(�3��D�,r�XP���8��]$2���(�p���@#08�������u9�������t57�}��=l�԰���W~Ug܍����tҺ/Dy� .��pwt�*h��έ#�so��jg�]������nq�t.�<s����F�O�{�j�w�R��RǴ&���ޤ�8\�k?y�����:�1h�E�|K��ԩx��N�rK�	ǒ�+��֦C��D�*ݧ�^�ʟ�/tU�??��_���fU._      �   �  x���MS�H��ү���̡K�,��&XF�M�c/�\�
dɣ��ϓ�/�fc�@���*�}3+3�9�x�23�Tz���ĨR�FU�n��|�eūY��(vJWU��w&��)���Z5{S��3Cg:����Q�z�E��7p�X�n^UZ����7�m��w�K�?�|��4un��_3gY��I�A'B8��^�M�/�b�;�Tdi�J�/J�c��T��u^�;uk�]
�"w=�9g�m��u��ƷJ'5˕�MY����3?ԛ�9����Fhnu�iU����+���lR������H�z!¹���}��j_T`X [�%~Jۿno�캰�E΅7�L�3�<7{����s	�΢�h���t�-rc����s��}Zg���|�.kK�?C2%M^��9s�k��3ן�8gm��r��� n������ZsV�aCU4%����JU��7�t� r�u^�4�o�sE�-�N��zY캸vb�xɖ��������!�AC��u�����v�+Bv�7I�3(/_$6������{͕��)@N,W�%5��p�#{��qs�#3������Ɂ0˥�i�
�B�v[ �J��k` K!$�lHL]�]!N�Me?�Nŝ����Q������|��d`#�����F�jL�tIk- �.IIvVu�����ڟ$5�!�T��M�tB��6}��3����\��I�f8!�:�� �@�gM��m��ʽ��J��YW���yV� V�tf���H���X�98��gZ�� \6���)O.���]�ս]F 4�l���H���۲�ػ[�TSu�Gb��ڦ{w\-H1��rm�y�I�N)"�Ps)UźI(���6Zs ͧ�7�$e>Y�9q�E���ֹ�)厇�>�Hj��I.�y�jm�g7$��gi�	�!��kަԳ1{+���s>Ctj�u��0D9��͖�լS*�¹�\��n��s rnp�.�7�lM�,�Sj��r��۔�zԙ���w�V�Q	m�r| n�8��o�KvثLI'����Y�x�z��t�xP�9tM5S�c[~+$T����M�8��Y�BREA��~�.B�����{���T����Űoa�"�s�(�v�%N��u��y�0����s#�ǎ��4���;Jn�oh4sbZɏJj\���g�s�K7�#;Je$�����58ȿ��,�Z&[e���/ǾA�ڐKm]�3��v�.Wt)�Vo��>L������)�]����U0A��$Zz*�c1�l�0H��6[=<�!���aD�zK�_?/o�`ӛe~?��yz���1�<��t�oW�9�FbK����Ǵ��uC�����r!U�p�`E�D��u2��&���T<������c�'v�E�vL�&��.zxۺ(1{�f��>� �;�`���������E�Ǵ��<�h�`=�����N�1��G�fi�6�<�����uGC��h�<��rM�zb��A�<�gA�$q��u_�<�����R댌ob���u���})�4�vx[���Z�kZo�����T !E���i7y�i5�m�é�>�.���ˬK���
���l�b܅�M�7o{1��}Qdb�~�D�C�θ�MF�����HAͼ����9�}�_�cb����a�iu�sj뾇O1C�З�K$`:1���;3���N��Š0�L�%��@W��gcyt�SxyT�H����Ĥ{q&t�q4R�׺ֈ�������Y�F�Pld ����4�.i�C��<�J�_����m�(9�:�GJ3��_�W�X3@s��q��Q��/iE��B�y��� yT�T�&'��7���6������������[yN��]��So��������H��!%e�V�CV�u����N*2H���� �>���w�ih�T��x�1Mڷϡ��1&y��u���~����t����mw��Pa�/'�o�C���e߽�,�a׶:0:�h8�MV��Lj�FV"Y�Oo�Ӳ'YfoCڿZ%WRa[#L~���H�����U����Wn�}Q���0��Te]I)VG�}p�Q�q�f��� !�ʛ���t�ӡP�.�������?�	�c~�s�؆Caz��y<�za8�9Z�|`�]�d����!�1���?!�J�"i�wO�Ǭ�6�2�� m�f�R35W�*����?��)w����;H�F6t��ä����u� Vp��      �   �  x���9v%!E㪽�G#��t"��/�����v�#�����ņ���\��x�{�-���iЦu"�����un��4f����t�X7�x�<�Jm@�:A��`��Dx������������>���V�WΣ�Ӱ�w�M׾̨G���dQd���������%_~��7fiEdK�/��'��]}����!�>|�'��>����#��̵�B��0�tĶɋ��o~�|D��_i�>�d�y��h�
>�.8�ʞ7��_�?qS�U@�|� D�~lyԱי��������/��=��|1���g_���E p$�*r_�%��Y�"d7|��	=�	��2���-���C��������[m��
ۦ�Q��3F����B�����?���������m��5?9t.Ρ"����(��fg�o���v?k>�����9T      n   G   x�33���t��Qp��II-*�,�4204�50�50V04�20�2��362573��/k �X������ rF�      ~   �  x�}��n�0�g�)�b�GI�/�)y�,�ӘB,�	� �LE�!c7EQ�k��20/�7)��V�8���#%�/���>[��P�}�Ł+*��+8k��vM�~�EQ��m�}ssv8�}�}w��7��%,�����O8XߕqB=�4��/�qҏ�d�	�ل�Dh��l�P��c��p\��E��p2�N3)�4�Q��HJ󜸖�9����Nc��cΖl�n����å��ml���n��V���CN�hw&���&�o��iC�r>U�^�(���)�ҔG���~:?.�o��!��:���_�t{þ-Z��Z��w��#����y��M�Rq���2�g��B���>90��b	;�bD���j^�?3�mp��x���
_!Ո"��E#�4�S�T�9J�$����M         �   x�3204�50�52W00�#N���̇�g*dޜ�����pwc�B^b�B��
��wOLV(y�k�BNb��w��K�2��f�0�(/�R�,�Qj���4���L3�50D�fJ�if(��c����������K�22����zS]C����/�H-JI��ʚ�"����� ��*�&V*p��qqq 

�	      �   :  x���Kn� �5>���f���dc�ɣ�#5��޾�Q�MR�� ����	bd��U�V�
YBh ��AdS]?�]Y���>��1�Ѯf&�0���ZH�#b9ye&�#e�u�jK��*Cq��Axs�2�%J��c�sJ�|�%�~���06kk\�7����!�M��hϪEmU�Ⱦ�r��+��3��=n�V8�\:��e��}�}��^�}����0ȅ��,k"D�7+�m�0���9��9�D��r��y�[g�p��-����'4t���d�M�[��������R�t�������6`�� :�9]�����UU}�؞n      �      x������ � �      �      x�����0C����u[�!��3����/qA�)��e
z��E���ӯ�x59B�Ъ�9�L���>B�p&��.�&������g�nnpj�A��:'|��t+��Ǧ�62�T���9�t\<      �     x�e��n�0���S�r�Jd;vN�'�4����
H@U��@WvP.����:�C���?4�z�q<�l�c�MWjr�p9hk���F��$x![�5J"�2�\�j�R�mʭ=��T.O�ZK	ϻo�G���S3_ǵ�ҢPT����.�\�.�Q
4dgk�gZȜ�����vj�K�k�U$	9Q41k,�sdI�&��G��[ٲ�����M����lݴ^gW�Db�;���xmrBkl 	+jPs;�v���IB�p>C@~]{SU���j�      �   e  x���An� �ur��@*l0=K�=A�z�I���)R�/�$��i�M�G�Hr��H�3%�ƌd���~vݘ�ׂ�H��2� c��xs<�
p�\'�L�K_� � � �f�?�P��eo5��f#Ύ���)@���ީږ�v� ��g���<h��w<P�j^@�˄W�G]��Z�Ha��,�a�<qu|�0D�E�CT��/ }nY�A���皞8�Op� \f��J�[,�8���E��a���bY���B�ݢ�&��U_S���%}���5Z_r����5g��egp�2͸8~���p�W�p���8;�[��K��%b���Iq|�������/`��     