PGDMP                         q            pos2013    9.0.3    9.0.3 %   �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
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
                  postgres    false    7            2           2612    11574    plpgsql    PROCEDURAL LANGUAGE     /   CREATE OR REPLACE PROCEDURAL LANGUAGE plpgsql;
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
       pgagent       postgres    false    6    562            �           0    0     FUNCTION pga_exception_trigger()    COMMENT     p   COMMENT ON FUNCTION pga_exception_trigger() IS 'Update the job''s next run time whenever an exception changes';
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
       pgagent       postgres    false    6    562            �           0    0 #   FUNCTION pga_is_leap_year(smallint)    COMMENT     W   COMMENT ON FUNCTION pga_is_leap_year(smallint) IS 'Returns TRUE is $1 is a leap year';
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
       pgagent       postgres    false    562    6            �           0    0    FUNCTION pga_job_trigger()    COMMENT     M   COMMENT ON FUNCTION pga_job_trigger() IS 'Update the job''s next run time.';
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
       pgagent       postgres    false    562    6            �           0    0 �   FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    COMMENT     �   COMMENT ON FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) IS 'Calculates the next runtime for a given schedule';
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
       pgagent       postgres    false    6    562            �           0    0    FUNCTION pga_schedule_trigger()    COMMENT     m   COMMENT ON FUNCTION pga_schedule_trigger() IS 'Update the job''s next run time whenever a schedule changes';
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
       pgagent       postgres    false    562    6                        1255    234863 7   checkserialnumber(character varying, character varying)    FUNCTION     %  CREATE FUNCTION checkserialnumber("partNumber" character varying, "serialNumber" character varying) RETURNS boolean
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
       public       postgres    false    562    7                        1255    234864    newid()    FUNCTION     �   CREATE FUNCTION newid() RETURNS uuid
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
       pgagent       postgres    false    6    1754            �           0    0    pga_exception_jexid_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE pga_exception_jexid_seq OWNED BY pga_exception.jexid;
            pgagent       postgres    false    1755            �           0    0    pga_exception_jexid_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('pga_exception_jexid_seq', 1, false);
            pgagent       postgres    false    1755            �           1259    234870    pga_job    TABLE     �  CREATE TABLE pga_job (
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
       pgagent         postgres    false    2178    2179    2180    2181    2182    6            �           0    0    TABLE pga_job    COMMENT     .   COMMENT ON TABLE pga_job IS 'Job main entry';
            pgagent       postgres    false    1756            �           0    0    COLUMN pga_job.jobagentid    COMMENT     S   COMMENT ON COLUMN pga_job.jobagentid IS 'Agent that currently executes this job.';
            pgagent       postgres    false    1756            �           1259    234881    pga_job_jobid_seq    SEQUENCE     s   CREATE SEQUENCE pga_job_jobid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE pgagent.pga_job_jobid_seq;
       pgagent       postgres    false    6    1756            �           0    0    pga_job_jobid_seq    SEQUENCE OWNED BY     9   ALTER SEQUENCE pga_job_jobid_seq OWNED BY pga_job.jobid;
            pgagent       postgres    false    1757            �           0    0    pga_job_jobid_seq    SEQUENCE SET     9   SELECT pg_catalog.setval('pga_job_jobid_seq', 1, false);
            pgagent       postgres    false    1757            �           1259    234883    pga_jobagent    TABLE     �   CREATE TABLE pga_jobagent (
    jagpid integer NOT NULL,
    jaglogintime timestamp with time zone DEFAULT now() NOT NULL,
    jagstation text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobagent;
       pgagent         postgres    false    2184    6            �           0    0    TABLE pga_jobagent    COMMENT     6   COMMENT ON TABLE pga_jobagent IS 'Active job agents';
            pgagent       postgres    false    1758            �           1259    234890    pga_jobclass    TABLE     U   CREATE TABLE pga_jobclass (
    jclid integer NOT NULL,
    jclname text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobclass;
       pgagent         postgres    false    6            �           0    0    TABLE pga_jobclass    COMMENT     7   COMMENT ON TABLE pga_jobclass IS 'Job classification';
            pgagent       postgres    false    1759            �           1259    234896    pga_jobclass_jclid_seq    SEQUENCE     x   CREATE SEQUENCE pga_jobclass_jclid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_jobclass_jclid_seq;
       pgagent       postgres    false    6    1759            �           0    0    pga_jobclass_jclid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_jobclass_jclid_seq OWNED BY pga_jobclass.jclid;
            pgagent       postgres    false    1760            �           0    0    pga_jobclass_jclid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobclass_jclid_seq', 5, true);
            pgagent       postgres    false    1760            �           1259    234898 
   pga_joblog    TABLE     v  CREATE TABLE pga_joblog (
    jlgid integer NOT NULL,
    jlgjobid integer NOT NULL,
    jlgstatus character(1) DEFAULT 'r'::bpchar NOT NULL,
    jlgstart timestamp with time zone DEFAULT now() NOT NULL,
    jlgduration interval,
    CONSTRAINT pga_joblog_jlgstatus_check CHECK ((jlgstatus = ANY (ARRAY['r'::bpchar, 's'::bpchar, 'f'::bpchar, 'i'::bpchar, 'd'::bpchar])))
);
    DROP TABLE pgagent.pga_joblog;
       pgagent         postgres    false    2186    2187    2189    6            �           0    0    TABLE pga_joblog    COMMENT     0   COMMENT ON TABLE pga_joblog IS 'Job run logs.';
            pgagent       postgres    false    1761            �           0    0    COLUMN pga_joblog.jlgstatus    COMMENT     �   COMMENT ON COLUMN pga_joblog.jlgstatus IS 'Status of job: r=running, s=successfully finished, f=failed, i=no steps to execute, d=aborted';
            pgagent       postgres    false    1761            �           1259    234904    pga_joblog_jlgid_seq    SEQUENCE     v   CREATE SEQUENCE pga_joblog_jlgid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE pgagent.pga_joblog_jlgid_seq;
       pgagent       postgres    false    1761    6            �           0    0    pga_joblog_jlgid_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE pga_joblog_jlgid_seq OWNED BY pga_joblog.jlgid;
            pgagent       postgres    false    1762            �           0    0    pga_joblog_jlgid_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('pga_joblog_jlgid_seq', 1, false);
            pgagent       postgres    false    1762            �           1259    234906    pga_jobstep    TABLE       CREATE TABLE pga_jobstep (
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
       pgagent         postgres    false    2190    2191    2192    2193    2194    2196    2197    2198    2199    6            �           0    0    TABLE pga_jobstep    COMMENT     ;   COMMENT ON TABLE pga_jobstep IS 'Job step to be executed';
            pgagent       postgres    false    1763            �           0    0    COLUMN pga_jobstep.jstkind    COMMENT     L   COMMENT ON COLUMN pga_jobstep.jstkind IS 'Kind of jobstep: s=sql, b=batch';
            pgagent       postgres    false    1763            �           0    0    COLUMN pga_jobstep.jstonerror    COMMENT     �   COMMENT ON COLUMN pga_jobstep.jstonerror IS 'What to do if step returns an error: f=fail the job, s=mark step as succeeded and continue, i=mark as fail but ignore it and proceed';
            pgagent       postgres    false    1763            �           1259    234921    pga_jobstep_jstid_seq    SEQUENCE     w   CREATE SEQUENCE pga_jobstep_jstid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE pgagent.pga_jobstep_jstid_seq;
       pgagent       postgres    false    6    1763            �           0    0    pga_jobstep_jstid_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE pga_jobstep_jstid_seq OWNED BY pga_jobstep.jstid;
            pgagent       postgres    false    1764            �           0    0    pga_jobstep_jstid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobstep_jstid_seq', 1, false);
            pgagent       postgres    false    1764            �           1259    234923    pga_jobsteplog    TABLE     �  CREATE TABLE pga_jobsteplog (
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
       pgagent         postgres    false    2200    2201    2203    6            �           0    0    TABLE pga_jobsteplog    COMMENT     9   COMMENT ON TABLE pga_jobsteplog IS 'Job step run logs.';
            pgagent       postgres    false    1765            �           0    0    COLUMN pga_jobsteplog.jslstatus    COMMENT     �   COMMENT ON COLUMN pga_jobsteplog.jslstatus IS 'Status of job step: r=running, s=successfully finished,  f=failed stopping job, i=ignored failure, d=aborted';
            pgagent       postgres    false    1765            �           0    0    COLUMN pga_jobsteplog.jslresult    COMMENT     I   COMMENT ON COLUMN pga_jobsteplog.jslresult IS 'Return code of job step';
            pgagent       postgres    false    1765            �           1259    234932    pga_jobsteplog_jslid_seq    SEQUENCE     z   CREATE SEQUENCE pga_jobsteplog_jslid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE pgagent.pga_jobsteplog_jslid_seq;
       pgagent       postgres    false    6    1765            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE pga_jobsteplog_jslid_seq OWNED BY pga_jobsteplog.jslid;
            pgagent       postgres    false    1766            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('pga_jobsteplog_jslid_seq', 1, false);
            pgagent       postgres    false    1766            �           1259    234934    pga_schedule    TABLE       CREATE TABLE pga_schedule (
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
       pgagent         postgres    false    2204    2205    2206    2207    2208    2209    2210    2211    2213    2214    2215    2216    2217    6            �           0    0    TABLE pga_schedule    COMMENT     <   COMMENT ON TABLE pga_schedule IS 'Job schedule exceptions';
            pgagent       postgres    false    1767            �           1259    234953    pga_schedule_jscid_seq    SEQUENCE     x   CREATE SEQUENCE pga_schedule_jscid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_schedule_jscid_seq;
       pgagent       postgres    false    1767    6            �           0    0    pga_schedule_jscid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_schedule_jscid_seq OWNED BY pga_schedule.jscid;
            pgagent       postgres    false    1768            �           0    0    pga_schedule_jscid_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('pga_schedule_jscid_seq', 1, false);
            pgagent       postgres    false    1768            �           1259    244946    base_Attachment    TABLE       CREATE TABLE "base_Attachment" (
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
       public         postgres    false    2275    2276    2277    7            �           1259    244944    base_Attachment_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Attachment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Attachment_Id_seq";
       public       postgres    false    1787    7            �           0    0    base_Attachment_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Attachment_Id_seq" OWNED BY "base_Attachment"."Id";
            public       postgres    false    1786            �           0    0    base_Attachment_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_Attachment_Id_seq"', 40012, true);
            public       postgres    false    1786            *           1259    256168    base_Authorize    TABLE     �   CREATE TABLE "base_Authorize" (
    "Id" bigint NOT NULL,
    "Resource" character varying(36) NOT NULL,
    "Code" character varying(10) NOT NULL
);
 $   DROP TABLE public."base_Authorize";
       public         postgres    false    7            )           1259    256166    base_Authorize_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Authorize_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Authorize_Id_seq";
       public       postgres    false    7    1834            �           0    0    base_Authorize_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Authorize_Id_seq" OWNED BY "base_Authorize"."Id";
            public       postgres    false    1833            �           0    0    base_Authorize_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_Authorize_Id_seq"', 359, true);
            public       postgres    false    1833                       1259    254557    base_Configuration    TABLE     �
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
    "IsAllowChangeOrder" boolean DEFAULT false,
    "IsAllowNegativeStore" boolean DEFAULT false,
    "AcceptedGiftCardMethod" integer DEFAULT 0 NOT NULL,
    "IsRewardOnTax" boolean DEFAULT false NOT NULL,
    "IsRewardOnMultiPayment" boolean DEFAULT false NOT NULL,
    "IsIncludeReturnFee" boolean DEFAULT false NOT NULL,
    "ReturnFeePercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "IsRewardLessThanDiscount" boolean DEFAULT false NOT NULL,
    "CurrencySymbol" character varying(5),
    "DecimalPlaces" smallint DEFAULT 0,
    "FomartCurrency" character varying(5),
    "PasswordLength" character varying(70)
);
 (   DROP TABLE public."base_Configuration";
       public         postgres    false    2359    2360    2361    2362    2363    2364    2365    2366    2367    2368    2369    2370    2371    2372    2374    2375    2376    2377    2378    2379    2380    2381    2382    2383    2384    2385    2386    2387    2388    2389    7            �           0    0 .   COLUMN "base_Configuration"."DefautlImagePath"    COMMENT     T   COMMENT ON COLUMN "base_Configuration"."DefautlImagePath" IS 'Apply to Attachment';
            public       postgres    false    1814            �           0    0 9   COLUMN "base_Configuration"."DefautlDiscountScheduleTime"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."DefautlDiscountScheduleTime" IS 'Apply to Discount Schedule Time';
            public       postgres    false    1814            �           0    0 (   COLUMN "base_Configuration"."LoginAllow"    COMMENT     \   COMMENT ON COLUMN "base_Configuration"."LoginAllow" IS 'So lan cho phep neu dang nhap sai';
            public       postgres    false    1814            �           0    0 5   COLUMN "base_Configuration"."IsRequireDiscountReason"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRequireDiscountReason" IS 'Reason box apprear when changing deactive to active status';
            public       postgres    false    1814            �           0    0 -   COLUMN "base_Configuration"."DefaultShipUnit"    COMMENT     f   COMMENT ON COLUMN "base_Configuration"."DefaultShipUnit" IS 'Don vi tinh trong luong khi van chuyen';
            public       postgres    false    1814            �           0    0 +   COLUMN "base_Configuration"."TimeOutMinute"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."TimeOutMinute" IS 'The time out application';
            public       postgres    false    1814            �           0    0 *   COLUMN "base_Configuration"."IsAutoLogout"    COMMENT     U   COMMENT ON COLUMN "base_Configuration"."IsAutoLogout" IS 'Combine to TimeOutMinute';
            public       postgres    false    1814            �           0    0 .   COLUMN "base_Configuration"."IsBackupWhenExit"    COMMENT     ]   COMMENT ON COLUMN "base_Configuration"."IsBackupWhenExit" IS 'Backup when exit application';
            public       postgres    false    1814            �           0    0 )   COLUMN "base_Configuration"."BackupEvery"    COMMENT     R   COMMENT ON COLUMN "base_Configuration"."BackupEvery" IS 'The time when back up ';
            public       postgres    false    1814            �           0    0 (   COLUMN "base_Configuration"."IsAllowRGO"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsAllowRGO" IS 'Is allow receive the quantity more than order quantity';
            public       postgres    false    1814            �           0    0 2   COLUMN "base_Configuration"."IsAllowNegativeStore"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."IsAllowNegativeStore" IS 'Cho phép kho âm';
            public       postgres    false    1814            �           0    0 +   COLUMN "base_Configuration"."IsRewardOnTax"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsRewardOnTax" IS 'T: SubTotal - Discount + Tax
S: SubTotal - Discount';
            public       postgres    false    1814            �           0    0 6   COLUMN "base_Configuration"."IsRewardLessThanDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRewardLessThanDiscount" IS 'T: Cho phep ap dung reward khi Reward < Discount
F: Canh bao va khong cho phep ap dung reward';
            public       postgres    false    1814            /           1259    257302    base_Configuration_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_Configuration_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_Configuration_Id_seq";
       public       postgres    false    1814    7            �           0    0    base_Configuration_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_Configuration_Id_seq" OWNED BY "base_Configuration"."Id";
            public       postgres    false    1839            �           0    0    base_Configuration_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_Configuration_Id_seq"', 3, true);
            public       postgres    false    1839            i           1259    283360    base_CostAdjustment    TABLE     �  CREATE TABLE "base_CostAdjustment" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "NewCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "OldCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "AdjustmentNewCost" numeric(12,2) DEFAULT 0,
    "AdjustmentOldCost" numeric(12,2) DEFAULT 0,
    "AdjustCostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "LoggedTime" timestamp without time zone DEFAULT now() NOT NULL,
    "Reason" character varying(30),
    "Status" character varying(50),
    "UserCreated" character varying(100),
    "IsReversed" boolean DEFAULT false,
    "StoreCode" integer
);
 )   DROP TABLE public."base_CostAdjustment";
       public         postgres    false    2609    2610    2611    2612    2613    2614    2615    2616    7            �           0    0 -   COLUMN "base_CostAdjustment"."CostDifference"    COMMENT     Q   COMMENT ON COLUMN "base_CostAdjustment"."CostDifference" IS 'NewCost - OldCost';
            public       postgres    false    1897            �           0    0 &   COLUMN "base_CostAdjustment"."NewCost"    COMMENT     S   COMMENT ON COLUMN "base_CostAdjustment"."NewCost" IS 'AdjustmentNewCost*Quantity';
            public       postgres    false    1897            �           0    0 &   COLUMN "base_CostAdjustment"."OldCost"    COMMENT     T   COMMENT ON COLUMN "base_CostAdjustment"."OldCost" IS 'AdjustmentOldCost*Quantity
';
            public       postgres    false    1897            �           0    0 3   COLUMN "base_CostAdjustment"."AdjustCostDifference"    COMMENT     k   COMMENT ON COLUMN "base_CostAdjustment"."AdjustCostDifference" IS 'AdjustmentNewCost - AdjustmentOldCost';
            public       postgres    false    1897            h           1259    283358    base_CostAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CostAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_CostAdjustmentItem_Id_seq";
       public       postgres    false    7    1897            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CostAdjustmentItem_Id_seq" OWNED BY "base_CostAdjustment"."Id";
            public       postgres    false    1896            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CostAdjustmentItem_Id_seq"', 3, true);
            public       postgres    false    1896            [           1259    271738    base_CountStock    TABLE     �  CREATE TABLE "base_CountStock" (
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
       public         postgres    false    2582    2583    7            �           0    0 !   COLUMN "base_CountStock"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_CountStock"."Status" IS 'Get from "CountStockStatus" tag in XML';
            public       postgres    false    1883            ]           1259    271745    base_CountStockDetail    TABLE     j  CREATE TABLE "base_CountStockDetail" (
    "Id" bigint NOT NULL,
    "CountStockId" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "StoreId" smallint DEFAULT 0 NOT NULL,
    "Quantity" integer DEFAULT 0 NOT NULL,
    "CountedQuantity" integer DEFAULT 0 NOT NULL,
    "Difference" integer DEFAULT 0
);
 +   DROP TABLE public."base_CountStockDetail";
       public         postgres    false    2585    2586    2587    2588    7            �           0    0 +   COLUMN "base_CountStockDetail"."Difference"    COMMENT     W   COMMENT ON COLUMN "base_CountStockDetail"."Difference" IS 'Diff = Counted - Quantity';
            public       postgres    false    1885            \           1259    271743    base_CountStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CountStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_CountStockDetail_Id_seq";
       public       postgres    false    7    1885            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CountStockDetail_Id_seq" OWNED BY "base_CountStockDetail"."Id";
            public       postgres    false    1884            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CountStockDetail_Id_seq"', 174, true);
            public       postgres    false    1884            Z           1259    271736    base_CountStock_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_CountStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_CountStock_Id_seq";
       public       postgres    false    1883    7            �           0    0    base_CountStock_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_CountStock_Id_seq" OWNED BY "base_CountStock"."Id";
            public       postgres    false    1882            �           0    0    base_CountStock_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_CountStock_Id_seq"', 29, true);
            public       postgres    false    1882                       1259    245340    base_Department    TABLE       CREATE TABLE "base_Department" (
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
       public         postgres    false    2320    2321    2322    2323    2324    2325    2326    7            �           0    0    TABLE "base_Department"    COMMENT     ,   COMMENT ON TABLE "base_Department" IS '

';
            public       postgres    false    1807                        0    0 "   COLUMN "base_Department"."LevelId"    COMMENT     8   COMMENT ON COLUMN "base_Department"."LevelId" IS 'ddd';
            public       postgres    false    1807                       1259    245338    base_Department_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Department_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Department_Id_seq";
       public       postgres    false    1807    7                       0    0    base_Department_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Department_Id_seq" OWNED BY "base_Department"."Id";
            public       postgres    false    1806                       0    0    base_Department_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Department_Id_seq"', 395, true);
            public       postgres    false    1806            �           1259    238237 
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
       public         postgres    false    2218    2219    2220    2221    2222    2223    2224    2225    2226    2227    2228    7                       0    0 %   COLUMN "base_Email"."IsHasAttachment"    COMMENT     p   COMMENT ON COLUMN "base_Email"."IsHasAttachment" IS 'Nếu có file đính kèm thì sẽ bật lên là true';
            public       postgres    false    1770                       0    0 $   COLUMN "base_Email"."AttachmentType"    COMMENT     [   COMMENT ON COLUMN "base_Email"."AttachmentType" IS 'Sử dụng khi IsHasAttachment=true';
            public       postgres    false    1770                       0    0 &   COLUMN "base_Email"."AttachmentResult"    COMMENT     y   COMMENT ON COLUMN "base_Email"."AttachmentResult" IS 'Sử dụng khi IsHasAttachment=true và phụ thuộc vào Type';
            public       postgres    false    1770                       0    0    COLUMN "base_Email"."Sender"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Sender" IS 'Thông tin người gủi dựa và GuestId';
            public       postgres    false    1770                       0    0    COLUMN "base_Email"."Status"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Status" IS '0: Outbox
1: Inbox
2: Sent
3: Draft
4: Trash';
            public       postgres    false    1770                       0    0     COLUMN "base_Email"."Importance"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Importance" IS 'Message Option
0: Normal
1: Importance
';
            public       postgres    false    1770            	           0    0 !   COLUMN "base_Email"."Sensitivity"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Sensitivity" IS 'Message Option
0: Personal
1: Bussiness';
            public       postgres    false    1770            
           0    0 '   COLUMN "base_Email"."IsRequestDelivery"    COMMENT     o   COMMENT ON COLUMN "base_Email"."IsRequestDelivery" IS 'Message Option
Request a delivery receipt for message';
            public       postgres    false    1770                       0    0 #   COLUMN "base_Email"."IsRequestRead"    COMMENT     g   COMMENT ON COLUMN "base_Email"."IsRequestRead" IS 'Message Option
Request a read receipt for message';
            public       postgres    false    1770                       0    0    COLUMN "base_Email"."IsMyFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsMyFlag" IS 'Custom Reminder Active Flag For Me';
            public       postgres    false    1770                       0    0    COLUMN "base_Email"."FlagTo"    COMMENT     >   COMMENT ON COLUMN "base_Email"."FlagTo" IS 'My Flag Options';
            public       postgres    false    1770                       0    0 #   COLUMN "base_Email"."FlagStartDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagStartDate" IS 'Active My Flag Date';
            public       postgres    false    1770                       0    0 !   COLUMN "base_Email"."FlagDueDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagDueDate" IS 'DeActive My Flag Date';
            public       postgres    false    1770                       0    0 %   COLUMN "base_Email"."IsAllowReminder"    COMMENT     L   COMMENT ON COLUMN "base_Email"."IsAllowReminder" IS 'Allow remind my flag';
            public       postgres    false    1770                       0    0    COLUMN "base_Email"."RemindOn"    COMMENT     X   COMMENT ON COLUMN "base_Email"."RemindOn" IS 'My Flag is going to remind on this date';
            public       postgres    false    1770                       0    0 #   COLUMN "base_Email"."MyRemindTimes"    COMMENT     H   COMMENT ON COLUMN "base_Email"."MyRemindTimes" IS 'The reminder times';
            public       postgres    false    1770                       0    0 $   COLUMN "base_Email"."IsRecipentFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsRecipentFlag" IS 'Custom Reminder For Recipent';
            public       postgres    false    1770                       0    0 $   COLUMN "base_Email"."RecipentFlagTo"    COMMENT     L   COMMENT ON COLUMN "base_Email"."RecipentFlagTo" IS 'Recipent Flag Options';
            public       postgres    false    1770                       0    0 -   COLUMN "base_Email"."IsAllowRecipentReminder"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."IsAllowRecipentReminder" IS 'Allow remind Recipent Flag';
            public       postgres    false    1770                       0    0 &   COLUMN "base_Email"."RecipentRemindOn"    COMMENT     f   COMMENT ON COLUMN "base_Email"."RecipentRemindOn" IS 'Recipent Flag is going to remind on this date';
            public       postgres    false    1770                       0    0 )   COLUMN "base_Email"."RecipentRemindTimes"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."RecipentRemindTimes" IS 'The Reminder Times of Recipent';
            public       postgres    false    1770            �           1259    238137    base_EmailAttachment    TABLE     p   CREATE TABLE "base_EmailAttachment" (
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
       public         postgres    false    2231    2232    2233    2234    2235    2236    2237    2238    2239    2240    2242    2243    2244    2245    2246    2247    2248    2249    2250    2251    2252    2253    2254    2255    2256    2257    2258    7                       0    0    COLUMN "base_Guest"."GuestNo"    COMMENT     <   COMMENT ON COLUMN "base_Guest"."GuestNo" IS 'YYMMDDHHMMSS';
            public       postgres    false    1775                       0    0     COLUMN "base_Guest"."PositionId"    COMMENT     >   COMMENT ON COLUMN "base_Guest"."PositionId" IS 'Chức vụ';
            public       postgres    false    1775                       0    0     COLUMN "base_Guest"."Department"    COMMENT     =   COMMENT ON COLUMN "base_Guest"."Department" IS 'Phòng ban';
            public       postgres    false    1775                       0    0    COLUMN "base_Guest"."Mark"    COMMENT     [   COMMENT ON COLUMN "base_Guest"."Mark" IS '-- E: Employee C: Company V: Vendor O: Contact';
            public       postgres    false    1775                       0    0    COLUMN "base_Guest"."IsPrimary"    COMMENT     ^   COMMENT ON COLUMN "base_Guest"."IsPrimary" IS 'Áp dụng nếu đối tượng là contact';
            public       postgres    false    1775                       0    0 '   COLUMN "base_Guest"."CommissionPercent"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."CommissionPercent" IS 'Apply khi Mark = E';
            public       postgres    false    1775                       0    0 )   COLUMN "base_Guest"."TotalRewardRedeemed"    COMMENT     o   COMMENT ON COLUMN "base_Guest"."TotalRewardRedeemed" IS 'Total reward redeemed earned during tracking period';
            public       postgres    false    1775                       0    0 2   COLUMN "base_Guest"."PurchaseDuringTrackingPeriod"    COMMENT     `   COMMENT ON COLUMN "base_Guest"."PurchaseDuringTrackingPeriod" IS '= Total(SaleOrderSubAmount)';
            public       postgres    false    1775                        0    0 /   COLUMN "base_Guest"."RequirePurchaseNextReward"    COMMENT     �   COMMENT ON COLUMN "base_Guest"."RequirePurchaseNextReward" IS 'F = RewardAmount - PurchaseDuringTrackingPeriod Mod RewardAmount';
            public       postgres    false    1775            !           0    0 '   COLUMN "base_Guest"."IsBlockArriveLate"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBlockArriveLate" IS '-- Apply to TimeClock';
            public       postgres    false    1775            "           0    0 '   COLUMN "base_Guest"."IsDeductLunchTime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsDeductLunchTime" IS '-- Apply to TimeClock';
            public       postgres    false    1775            #           0    0 '   COLUMN "base_Guest"."IsBalanceOvertime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBalanceOvertime" IS '-- Apply to TimeClock';
            public       postgres    false    1775            $           0    0 !   COLUMN "base_Guest"."LateMinutes"    COMMENT     I   COMMENT ON COLUMN "base_Guest"."LateMinutes" IS '-- Apply to TimeClock';
            public       postgres    false    1775            %           0    0 $   COLUMN "base_Guest"."OvertimeOption"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."OvertimeOption" IS '-- Apply to TimeClock';
            public       postgres    false    1775            &           0    0 #   COLUMN "base_Guest"."OTLeastMinute"    COMMENT     K   COMMENT ON COLUMN "base_Guest"."OTLeastMinute" IS '-- Apply to TimeClock';
            public       postgres    false    1775            '           0    0    COLUMN "base_Guest"."SaleRepId"    COMMENT     C   COMMENT ON COLUMN "base_Guest"."SaleRepId" IS 'Apply to customer';
            public       postgres    false    1775                       1259    245376    base_GuestAdditional    TABLE        CREATE TABLE "base_GuestAdditional" (
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
       public         postgres    false    2328    2329    2330    7            (           0    0 $   COLUMN "base_GuestAdditional"."Unit"    COMMENT     K   COMMENT ON COLUMN "base_GuestAdditional"."Unit" IS '0: Amount 1: Percent';
            public       postgres    false    1809            )           0    0 .   COLUMN "base_GuestAdditional"."IsTaxExemption"    COMMENT     N   COMMENT ON COLUMN "base_GuestAdditional"."IsTaxExemption" IS 'Miễn thuế';
            public       postgres    false    1809            *           0    0 .   COLUMN "base_GuestAdditional"."TaxExemptionNo"    COMMENT     a   COMMENT ON COLUMN "base_GuestAdditional"."TaxExemptionNo" IS 'Require if IsTaxExemption = true';
            public       postgres    false    1809                       1259    245374    base_GuestAdditional_Id_seq    SEQUENCE        CREATE SEQUENCE "base_GuestAdditional_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_GuestAdditional_Id_seq";
       public       postgres    false    7    1809            +           0    0    base_GuestAdditional_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_GuestAdditional_Id_seq" OWNED BY "base_GuestAdditional"."Id";
            public       postgres    false    1808            ,           0    0    base_GuestAdditional_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestAdditional_Id_seq"', 108, true);
            public       postgres    false    1808            �           1259    244863    base_GuestAddress    TABLE     �  CREATE TABLE "base_GuestAddress" (
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
       public         postgres    false    2260    2261    2262    7            �           1259    244861    base_GuestAddress_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestAddress_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestAddress_Id_seq";
       public       postgres    false    1777    7            -           0    0    base_GuestAddress_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestAddress_Id_seq" OWNED BY "base_GuestAddress"."Id";
            public       postgres    false    1776            .           0    0    base_GuestAddress_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestAddress_Id_seq"', 234, true);
            public       postgres    false    1776            �           1259    238413    base_GuestFingerPrint    TABLE     3  CREATE TABLE "base_GuestFingerPrint" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "FingerIndex" integer NOT NULL,
    "HandFlag" boolean NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdaed" character varying(30),
    "FingerPrintImage" bytea
);
 +   DROP TABLE public."base_GuestFingerPrint";
       public         postgres    false    2229    7            �           1259    238411    base_GuestFingerPrint_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestFingerPrint_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestFingerPrint_Id_seq";
       public       postgres    false    1772    7            /           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestFingerPrint_Id_seq" OWNED BY "base_GuestFingerPrint"."Id";
            public       postgres    false    1771            0           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestFingerPrint_Id_seq"', 12, true);
            public       postgres    false    1771            �           1259    244873    base_GuestHiringHistory    TABLE     Q  CREATE TABLE "base_GuestHiringHistory" (
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
       public         postgres    false    2264    7            �           1259    244871    base_GuestHiringHistory_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestHiringHistory_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_GuestHiringHistory_Id_seq";
       public       postgres    false    7    1779            1           0    0    base_GuestHiringHistory_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_GuestHiringHistory_Id_seq" OWNED BY "base_GuestHiringHistory"."Id";
            public       postgres    false    1778            2           0    0    base_GuestHiringHistory_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_GuestHiringHistory_Id_seq"', 1, false);
            public       postgres    false    1778            �           1259    244884    base_GuestPayRoll    TABLE     �  CREATE TABLE "base_GuestPayRoll" (
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
       public         postgres    false    2266    2267    2268    7            �           1259    244882    base_GuestPayRoll_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestPayRoll_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestPayRoll_Id_seq";
       public       postgres    false    1781    7            3           0    0    base_GuestPayRoll_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestPayRoll_Id_seq" OWNED BY "base_GuestPayRoll"."Id";
            public       postgres    false    1780            4           0    0    base_GuestPayRoll_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_GuestPayRoll_Id_seq"', 1, false);
            public       postgres    false    1780            1           1259    257325    base_GuestPaymentCard    TABLE     Z  CREATE TABLE "base_GuestPaymentCard" (
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
       public         postgres    false    2407    2408    7            0           1259    257323    base_GuestPaymentCard_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestPaymentCard_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestPaymentCard_Id_seq";
       public       postgres    false    1841    7            5           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestPaymentCard_Id_seq" OWNED BY "base_GuestPaymentCard"."Id";
            public       postgres    false    1840            6           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestPaymentCard_Id_seq"', 14, true);
            public       postgres    false    1840            �           1259    244922    base_ResourcePhoto    TABLE        CREATE TABLE "base_ResourcePhoto" (
    "Id" integer NOT NULL,
    "ThumbnailPhoto" bytea,
    "ThumbnailPhotoFilename" character varying(60),
    "LargePhoto" bytea,
    "LargePhotoFilename" character varying(60),
    "SortId" smallint DEFAULT 0,
    "Resource" character varying(36)
);
 (   DROP TABLE public."base_ResourcePhoto";
       public         postgres    false    2270    7            �           1259    244920    base_GuestPhoto_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_GuestPhoto_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_GuestPhoto_Id_seq";
       public       postgres    false    1783    7            7           0    0    base_GuestPhoto_Id_seq    SEQUENCE OWNED BY     L   ALTER SEQUENCE "base_GuestPhoto_Id_seq" OWNED BY "base_ResourcePhoto"."Id";
            public       postgres    false    1782            8           0    0    base_GuestPhoto_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_GuestPhoto_Id_seq"', 239, true);
            public       postgres    false    1782            �           1259    244934    base_GuestProfile    TABLE     �  CREATE TABLE "base_GuestProfile" (
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
       public         postgres    false    2272    2273    7            �           1259    244932    base_GuestProfile_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestProfile_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestProfile_Id_seq";
       public       postgres    false    7    1785            9           0    0    base_GuestProfile_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestProfile_Id_seq" OWNED BY "base_GuestProfile"."Id";
            public       postgres    false    1784            :           0    0    base_GuestProfile_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestProfile_Id_seq"', 156, true);
            public       postgres    false    1784            I           1259    268354    base_GuestReward    TABLE     �  CREATE TABLE "base_GuestReward" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "RewardId" integer NOT NULL,
    "Amount" numeric(15,2) DEFAULT 0 NOT NULL,
    "IsApply" boolean DEFAULT false NOT NULL,
    "EarnedDate" timestamp without time zone,
    "AppliedDate" timestamp without time zone,
    "RewardValue" numeric(15,2) DEFAULT 0 NOT NULL,
    "SaleOrderResource" character varying(36),
    "SaleOrderNo" character varying(15),
    "Remark" character varying(30) NOT NULL,
    "ActivedDate" timestamp without time zone,
    "ExpireDate" timestamp without time zone,
    "Reason" character varying(50),
    "Status" smallint DEFAULT 0
);
 &   DROP TABLE public."base_GuestReward";
       public         postgres    false    2513    2514    2515    2516    7            ;           0    0 '   COLUMN "base_GuestReward"."AppliedDate"    COMMENT     Z   COMMENT ON COLUMN "base_GuestReward"."AppliedDate" IS 'Ngay ap dung chuong trinh reward';
            public       postgres    false    1865            <           0    0 '   COLUMN "base_GuestReward"."ActivedDate"    COMMENT     �   COMMENT ON COLUMN "base_GuestReward"."ActivedDate" IS 'Ngay bat dau reward co hieu luc
Active Date = EearnedDate + Block Day After Earn.
Status = Pending';
            public       postgres    false    1865            =           0    0 "   COLUMN "base_GuestReward"."Status"    COMMENT     g   COMMENT ON COLUMN "base_GuestReward"."Status" IS 'Available = 1
Redeemed = 2
Pending = 3
Removed = 4';
            public       postgres    false    1865            H           1259    268352    base_GuestReward_Id_seq    SEQUENCE     {   CREATE SEQUENCE "base_GuestReward_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."base_GuestReward_Id_seq";
       public       postgres    false    7    1865            >           0    0    base_GuestReward_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE "base_GuestReward_Id_seq" OWNED BY "base_GuestReward"."Id";
            public       postgres    false    1864            ?           0    0    base_GuestReward_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestReward_Id_seq"', 3133, true);
            public       postgres    false    1864            (           1259    256013    base_GuestSchedule    TABLE     �   CREATE TABLE "base_GuestSchedule" (
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
       public       postgres    false    1775    7            @           0    0    base_Guest_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Guest_Id_seq" OWNED BY "base_Guest"."Id";
            public       postgres    false    1774            A           0    0    base_Guest_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"base_Guest_Id_seq"', 285, true);
            public       postgres    false    1774            e           1259    282548    base_Language    TABLE     �   CREATE TABLE "base_Language" (
    "Id" integer NOT NULL,
    "Code" character varying(2) NOT NULL,
    "Name" character varying(50) NOT NULL,
    "Flag" bytea,
    "IsLocked" boolean DEFAULT false NOT NULL,
    "Xml" character varying
);
 #   DROP TABLE public."base_Language";
       public         postgres    false    2601    7            d           1259    282546    base_Language_Id_seq    SEQUENCE     x   CREATE SEQUENCE "base_Language_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."base_Language_Id_seq";
       public       postgres    false    1893    7            B           0    0    base_Language_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "base_Language_Id_seq" OWNED BY "base_Language"."Id";
            public       postgres    false    1892            C           0    0    base_Language_Id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('"base_Language_Id_seq"', 1, false);
            public       postgres    false    1892            �           1259    244997    base_MemberShip    TABLE       CREATE TABLE "base_MemberShip" (
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
       public         postgres    false    2279    2280    7            D           0    0 %   COLUMN "base_MemberShip"."MemberType"    COMMENT     f   COMMENT ON COLUMN "base_MemberShip"."MemberType" IS 'P = Platium, G = Gold, S = Silver, B = Bronze.';
            public       postgres    false    1789            E           0    0 !   COLUMN "base_MemberShip"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_MemberShip"."Status" IS '-1 = Pending
0 = DeActived
1 = Actived';
            public       postgres    false    1789            �           1259    244995    base_MemberShip_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_MemberShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_MemberShip_Id_seq";
       public       postgres    false    1789    7            F           0    0    base_MemberShip_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_MemberShip_Id_seq" OWNED BY "base_MemberShip"."Id";
            public       postgres    false    1788            G           0    0    base_MemberShip_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_MemberShip_Id_seq"', 1, false);
            public       postgres    false    1788            K           1259    268511    base_PricingChange    TABLE     �  CREATE TABLE "base_PricingChange" (
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
       public         postgres    false    2517    2519    2520    2521    7            J           1259    268509    base_PricingChange_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PricingChange_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PricingChange_Id_seq";
       public       postgres    false    7    1867            H           0    0    base_PricingChange_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PricingChange_Id_seq" OWNED BY "base_PricingChange"."Id";
            public       postgres    false    1866            I           0    0    base_PricingChange_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingChange_Id_seq"', 533, true);
            public       postgres    false    1866            G           1259    268185    base_PricingManager    TABLE       CREATE TABLE "base_PricingManager" (
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
       public         postgres    false    2509    2510    2511    7            J           0    0 %   COLUMN "base_PricingManager"."Status"    COMMENT     u   COMMENT ON COLUMN "base_PricingManager"."Status" IS '- Pending
- Applied
- Restored

-> Get From PricingStatus Tag';
            public       postgres    false    1863            K           0    0 (   COLUMN "base_PricingManager"."BasePrice"    COMMENT     H   COMMENT ON COLUMN "base_PricingManager"."BasePrice" IS 'Cost or Price';
            public       postgres    false    1863            L           0    0 .   COLUMN "base_PricingManager"."CalculateMethod"    COMMENT     j   COMMENT ON COLUMN "base_PricingManager"."CalculateMethod" IS '+-*/
- Get from PricingAdjustmentType Tag';
            public       postgres    false    1863            M           0    0 )   COLUMN "base_PricingManager"."AmountUnit"    COMMENT     D   COMMENT ON COLUMN "base_PricingManager"."AmountUnit" IS '- % or $';
            public       postgres    false    1863            N           0    0 (   COLUMN "base_PricingManager"."ItemCount"    COMMENT     W   COMMENT ON COLUMN "base_PricingManager"."ItemCount" IS 'Tong so product duoc ap dung';
            public       postgres    false    1863            F           1259    268183    base_PricingManager_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_PricingManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_PricingManager_Id_seq";
       public       postgres    false    1863    7            O           0    0    base_PricingManager_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_PricingManager_Id_seq" OWNED BY "base_PricingManager"."Id";
            public       postgres    false    1862            P           0    0    base_PricingManager_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingManager_Id_seq"', 46, true);
            public       postgres    false    1862                       1259    245412    base_Product    TABLE     [  CREATE TABLE "base_Product" (
    "Id" bigint NOT NULL,
    "Code" character varying(15),
    "ItemTypeId" smallint NOT NULL,
    "ProductDepartmentId" integer NOT NULL,
    "ProductCategoryId" integer NOT NULL,
    "ProductBrandId" integer,
    "StyleModel" character varying(30) NOT NULL,
    "ProductName" character varying(300) NOT NULL,
    "Description" character varying(2000) NOT NULL,
    "Barcode" character varying(50) NOT NULL,
    "Attribute" character varying(30) NOT NULL,
    "Size" character varying(10) NOT NULL,
    "IsSerialTracking" boolean NOT NULL,
    "IsPublicWeb" boolean NOT NULL,
    "OnHandStore1" integer DEFAULT 0 NOT NULL,
    "OnHandStore2" integer DEFAULT 0 NOT NULL,
    "OnHandStore3" integer DEFAULT 0 NOT NULL,
    "OnHandStore4" integer DEFAULT 0 NOT NULL,
    "OnHandStore5" integer DEFAULT 0 NOT NULL,
    "OnHandStore6" integer DEFAULT 0 NOT NULL,
    "OnHandStore7" integer DEFAULT 0 NOT NULL,
    "OnHandStore8" integer DEFAULT 0 NOT NULL,
    "OnHandStore9" integer DEFAULT 0 NOT NULL,
    "OnHandStore10" integer DEFAULT 0 NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "QuantityOnOrder" integer DEFAULT 0 NOT NULL,
    "CompanyReOrderPoint" integer NOT NULL,
    "IsUnOrderAble" boolean NOT NULL,
    "IsEligibleForCommission" boolean NOT NULL,
    "IsEligibleForReward" boolean NOT NULL,
    "RegularPrice" numeric(15,2) NOT NULL,
    "Price1" numeric(15,2) NOT NULL,
    "Price2" numeric(15,2) NOT NULL,
    "Price3" numeric(15,2) NOT NULL,
    "Price4" numeric(15,2) NOT NULL,
    "OrderCost" numeric(12,2) NOT NULL,
    "AverageUnitCost" numeric(12,2) NOT NULL,
    "TaxCode" character(3) NOT NULL,
    "MarginPercent" numeric(8,2) NOT NULL,
    "MarkupPercent" numeric(8,2) NOT NULL,
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
       public         postgres    false    2331    2333    2334    2335    2336    2337    2338    2339    2340    2341    2342    2343    2344    2345    2346    2347    2348    2349    2350    2351    2352    2353    2354    2355    2356    7            Q           0    0 &   COLUMN "base_Product"."QuantityOnHand"    COMMENT     b   COMMENT ON COLUMN "base_Product"."QuantityOnHand" IS 'Total From OnHandStore1 to OnHandStore 10';
            public       postgres    false    1811            R           0    0 '   COLUMN "base_Product"."QuantityOnOrder"    COMMENT     a   COMMENT ON COLUMN "base_Product"."QuantityOnOrder" IS 'Total quantity on "Open" purchase order';
            public       postgres    false    1811            S           0    0 $   COLUMN "base_Product"."RegularPrice"    COMMENT     I   COMMENT ON COLUMN "base_Product"."RegularPrice" IS 'Apply to Base Unit';
            public       postgres    false    1811            T           0    0    COLUMN "base_Product"."Price1"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price1" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1811            U           0    0    COLUMN "base_Product"."Price2"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price2" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1811            V           0    0    COLUMN "base_Product"."Price3"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price3" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1811            W           0    0    COLUMN "base_Product"."Price4"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price4" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1811            X           0    0 !   COLUMN "base_Product"."OrderCost"    COMMENT     F   COMMENT ON COLUMN "base_Product"."OrderCost" IS 'Apply to Base Unit';
            public       postgres    false    1811            Y           0    0 '   COLUMN "base_Product"."AverageUnitCost"    COMMENT     L   COMMENT ON COLUMN "base_Product"."AverageUnitCost" IS 'Apply to Base Unit';
            public       postgres    false    1811            Z           0    0    COLUMN "base_Product"."TaxCode"    COMMENT     D   COMMENT ON COLUMN "base_Product"."TaxCode" IS 'Apply to Base Unit';
            public       postgres    false    1811            [           0    0 %   COLUMN "base_Product"."MarginPercent"    COMMENT     q   COMMENT ON COLUMN "base_Product"."MarginPercent" IS 'Margin =100*(RegularPrice - AverageUnitCode)/RegularPrice';
            public       postgres    false    1811            \           0    0 %   COLUMN "base_Product"."MarkupPercent"    COMMENT     t   COMMENT ON COLUMN "base_Product"."MarkupPercent" IS 'Markup =100*(RegularPrice - AverageUnitCost)/AverageUnitCost';
            public       postgres    false    1811            ]           0    0 "   COLUMN "base_Product"."IsOpenItem"    COMMENT     Q   COMMENT ON COLUMN "base_Product"."IsOpenItem" IS 'Can change price during sale';
            public       postgres    false    1811                       1259    255536    base_ProductStore    TABLE     �   CREATE TABLE "base_ProductStore" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL
);
 '   DROP TABLE public."base_ProductStore";
       public         postgres    false    2390    2392    7                       1259    255534    base_ProductStore_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ProductStore_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ProductStore_Id_seq";
       public       postgres    false    7    1816            ^           0    0    base_ProductStore_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ProductStore_Id_seq" OWNED BY "base_ProductStore"."Id";
            public       postgres    false    1815            _           0    0    base_ProductStore_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ProductStore_Id_seq"', 112, true);
            public       postgres    false    1815            Y           1259    270252    base_ProductUOM    TABLE     U  CREATE TABLE "base_ProductUOM" (
    "Id" bigint NOT NULL,
    "ProductStoreId" bigint,
    "UOMId" integer NOT NULL,
    "BaseUnitNumber" integer DEFAULT 0 NOT NULL,
    "RegularPrice" numeric(12,2) DEFAULT 0 NOT NULL,
    "QuantityOnHand" numeric(12,2) DEFAULT 0 NOT NULL,
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
       public         postgres    false    2566    2568    2569    2570    2571    2572    2573    2574    2575    2576    2577    2578    2579    2580    7            `           0    0    TABLE "base_ProductUOM"    COMMENT     B   COMMENT ON TABLE "base_ProductUOM" IS 'Use when allow multi UOM';
            public       postgres    false    1881            X           1259    270250    base_ProductUOM_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_ProductUOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_ProductUOM_Id_seq";
       public       postgres    false    7    1881            a           0    0    base_ProductUOM_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_ProductUOM_Id_seq" OWNED BY "base_ProductUOM"."Id";
            public       postgres    false    1880            b           0    0    base_ProductUOM_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_ProductUOM_Id_seq"', 69, true);
            public       postgres    false    1880                       1259    245410    base_Product_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_Product_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_Product_Id_seq";
       public       postgres    false    1811    7            c           0    0    base_Product_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_Product_Id_seq" OWNED BY "base_Product"."Id";
            public       postgres    false    1810            d           0    0    base_Product_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Product_Id_seq"', 250214, true);
            public       postgres    false    1810                       1259    245169    base_Promotion    TABLE     �  CREATE TABLE "base_Promotion" (
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
       public         postgres    false    2313    2314    2315    2316    2317    2318    7            e           0    0     COLUMN "base_Promotion"."Status"    COMMENT     U   COMMENT ON COLUMN "base_Promotion"."Status" IS '0: Deactived
1: Actived
2: Pending';
            public       postgres    false    1805            f           0    0 (   COLUMN "base_Promotion"."AffectDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Promotion"."AffectDiscount" IS '0: All items
1: All items in category
2: All items from vendors
3: Custom';
            public       postgres    false    1805                       1259    245155    base_PromotionAffect    TABLE     j  CREATE TABLE "base_PromotionAffect" (
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
       public         postgres    false    2301    2302    2304    2305    2306    2307    2308    2309    2310    2311    7            
           1259    245153    base_PromotionAffect_Id_seq    SEQUENCE        CREATE SEQUENCE "base_PromotionAffect_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_PromotionAffect_Id_seq";
       public       postgres    false    1803    7            g           0    0    base_PromotionAffect_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_PromotionAffect_Id_seq" OWNED BY "base_PromotionAffect"."Id";
            public       postgres    false    1802            h           0    0    base_PromotionAffect_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_PromotionAffect_Id_seq"', 609, true);
            public       postgres    false    1802            �           1259    245023    base_PromotionSchedule    TABLE     �   CREATE TABLE "base_PromotionSchedule" (
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
       public       postgres    false    7    1791            i           0    0    base_PromotionSchedule_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE "base_PromotionSchedule_Id_seq" OWNED BY "base_PromotionSchedule"."Id";
            public       postgres    false    1790            j           0    0    base_PromotionSchedule_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_PromotionSchedule_Id_seq"', 55, true);
            public       postgres    false    1790                       1259    245167    base_Promotion_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Promotion_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Promotion_Id_seq";
       public       postgres    false    1805    7            k           0    0    base_Promotion_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Promotion_Id_seq" OWNED BY "base_Promotion"."Id";
            public       postgres    false    1804            l           0    0    base_Promotion_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_Promotion_Id_seq"', 55, true);
            public       postgres    false    1804            ?           1259    266551    base_PurchaseOrder    TABLE     T  CREATE TABLE "base_PurchaseOrder" (
    "Id" bigint NOT NULL,
    "PurchaseOrderNo" character varying(15) NOT NULL,
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
    "Paid" numeric(12,2) DEFAULT 0 NOT NULL,
    "Balance" numeric(12,2) DEFAULT 0 NOT NULL,
    "ItemCount" integer DEFAULT 0 NOT NULL,
    "QtyOrdered" integer DEFAULT 0 NOT NULL,
    "QtyDue" integer DEFAULT 0 NOT NULL,
    "QtyReceived" integer DEFAULT 0 NOT NULL,
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
    "IsLocked" boolean DEFAULT false NOT NULL,
    "VendorResource" character varying(36) NOT NULL
);
 (   DROP TABLE public."base_PurchaseOrder";
       public         postgres    false    2479    2481    2482    2483    2484    2485    2486    2487    2488    2489    2490    2491    2492    2493    2494    2495    2496    2497    7            m           0    0 (   COLUMN "base_PurchaseOrder"."QtyOrdered"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyOrdered" IS 'Order Quantity: In the purchase order item list. Enter the quantity being ordered for the item.
';
            public       postgres    false    1855            n           0    0 $   COLUMN "base_PurchaseOrder"."QtyDue"    COMMENT     q   COMMENT ON COLUMN "base_PurchaseOrder"."QtyDue" IS 'Due Quantity: The item quantity remaining to be received.
';
            public       postgres    false    1855            o           0    0 )   COLUMN "base_PurchaseOrder"."QtyReceived"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyReceived" IS 'Received Quantity: The ordered item quantity already received on receiving vouchers.
';
            public       postgres    false    1855            p           0    0 &   COLUMN "base_PurchaseOrder"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_PurchaseOrder"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100

';
            public       postgres    false    1855            =           1259    266530    base_PurchaseOrderDetail    TABLE     c  CREATE TABLE "base_PurchaseOrderDetail" (
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
       public         postgres    false    2469    2471    2472    2473    2474    2475    2476    2477    2478    7            q           0    0 *   COLUMN "base_PurchaseOrderDetail"."Amount"    COMMENT     S   COMMENT ON COLUMN "base_PurchaseOrderDetail"."Amount" IS 'Amount = Cost*Quantity';
            public       postgres    false    1853            <           1259    266528    base_PurchaseOrderDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_PurchaseOrderDetail_Id_seq";
       public       postgres    false    1853    7            r           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_PurchaseOrderDetail_Id_seq" OWNED BY "base_PurchaseOrderDetail"."Id";
            public       postgres    false    1852            s           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_PurchaseOrderDetail_Id_seq"', 149, true);
            public       postgres    false    1852            E           1259    267535    base_PurchaseOrderReceive    TABLE     o  CREATE TABLE "base_PurchaseOrderReceive" (
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
       public         postgres    false    2503    2504    2505    2507    7            t           0    0 *   COLUMN "base_PurchaseOrderReceive"."Price"    COMMENT     G   COMMENT ON COLUMN "base_PurchaseOrderReceive"."Price" IS 'Sale Price';
            public       postgres    false    1861            D           1259    267533     base_PurchaseOrderReceive_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderReceive_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_PurchaseOrderReceive_Id_seq";
       public       postgres    false    7    1861            u           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_PurchaseOrderReceive_Id_seq" OWNED BY "base_PurchaseOrderReceive"."Id";
            public       postgres    false    1860            v           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_PurchaseOrderReceive_Id_seq"', 120, true);
            public       postgres    false    1860            >           1259    266549    base_PurchaseOrder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PurchaseOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PurchaseOrder_Id_seq";
       public       postgres    false    1855    7            w           0    0    base_PurchaseOrder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PurchaseOrder_Id_seq" OWNED BY "base_PurchaseOrder"."Id";
            public       postgres    false    1854            x           0    0    base_PurchaseOrder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_PurchaseOrder_Id_seq"', 80, true);
            public       postgres    false    1854            g           1259    282642    base_QuantityAdjustment    TABLE     C  CREATE TABLE "base_QuantityAdjustment" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "OldQty" integer DEFAULT 0 NOT NULL,
    "NewQty" integer DEFAULT 0 NOT NULL,
    "AdjustmentQtyDiff" integer DEFAULT 0 NOT NULL,
    "LoggedTime" timestamp without time zone NOT NULL,
    "Reason" character varying(30),
    "Status" character varying(50),
    "UserCreated" character varying(100),
    "IsReversed" boolean DEFAULT false,
    "StoreCode" integer
);
 -   DROP TABLE public."base_QuantityAdjustment";
       public         postgres    false    2602    2603    2604    2605    2606    7            y           0    0 1   COLUMN "base_QuantityAdjustment"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustment"."CostDifference" IS '-- AverageUnitCost*OldQuantity - AverageUnitCost*NewQuantity';
            public       postgres    false    1895            z           0    0 4   COLUMN "base_QuantityAdjustment"."AdjustmentQtyDiff"    COMMENT     j   COMMENT ON COLUMN "base_QuantityAdjustment"."AdjustmentQtyDiff" IS 'AdjustmentNewQty - AdjustmentOldQty';
            public       postgres    false    1895            f           1259    282640 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_QuantityAdjustmentItem_Id_seq";
       public       postgres    false    7    1895            {           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_QuantityAdjustmentItem_Id_seq" OWNED BY "base_QuantityAdjustment"."Id";
            public       postgres    false    1894            |           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_QuantityAdjustmentItem_Id_seq"', 6, true);
            public       postgres    false    1894            ,           1259    256178    base_ResourceAccount    TABLE       CREATE TABLE "base_ResourceAccount" (
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
       public         postgres    false    2402    2403    2404    7            +           1259    256176    base_ResourceAccount_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourceAccount_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourceAccount_Id_seq";
       public       postgres    false    1836    7            }           0    0    base_ResourceAccount_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourceAccount_Id_seq" OWNED BY "base_ResourceAccount"."Id";
            public       postgres    false    1835            ~           0    0    base_ResourceAccount_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceAccount_Id_seq"', 27, true);
            public       postgres    false    1835                       1259    246083    base_ResourceNote    TABLE     �   CREATE TABLE "base_ResourceNote" (
    "Id" bigint NOT NULL,
    "Note" character varying(300),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "Color" character(9),
    "Resource" character varying(36) NOT NULL
);
 '   DROP TABLE public."base_ResourceNote";
       public         postgres    false    2358    7                       1259    246081    base_ResourceNote_id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ResourceNote_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ResourceNote_id_seq";
       public       postgres    false    7    1813                       0    0    base_ResourceNote_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ResourceNote_id_seq" OWNED BY "base_ResourceNote"."Id";
            public       postgres    false    1812            �           0    0    base_ResourceNote_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ResourceNote_id_seq"', 764, true);
            public       postgres    false    1812            U           1259    270150    base_ResourcePayment    TABLE     �  CREATE TABLE "base_ResourcePayment" (
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
       public         postgres    false    2542    2543    2545    2546    2547    2548    2549    2550    2551    2552    2553    7            �           0    0 $   COLUMN "base_ResourcePayment"."Mark"    COMMENT     <   COMMENT ON COLUMN "base_ResourcePayment"."Mark" IS 'SO/PO';
            public       postgres    false    1877            S           1259    270072    base_ResourcePaymentDetail    TABLE       CREATE TABLE "base_ResourcePaymentDetail" (
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
       public         postgres    false    2539    2540    2541    7            �           0    0 1   COLUMN "base_ResourcePaymentDetail"."PaymentType"    COMMENT     W   COMMENT ON COLUMN "base_ResourcePaymentDetail"."PaymentType" IS 'P:Payment
C:Correct';
            public       postgres    false    1875            �           0    0 ,   COLUMN "base_ResourcePaymentDetail"."Reason"    COMMENT     ^   COMMENT ON COLUMN "base_ResourcePaymentDetail"."Reason" IS 'Apply to Correct payment action';
            public       postgres    false    1875            R           1259    270070 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_ResourcePaymentDetail_Id_seq";
       public       postgres    false    7    1875            �           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_ResourcePaymentDetail_Id_seq" OWNED BY "base_ResourcePaymentDetail"."Id";
            public       postgres    false    1874            �           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentDetail_Id_seq"', 402, true);
            public       postgres    false    1874            a           1259    272122    base_ResourcePaymentProduct    TABLE       CREATE TABLE "base_ResourcePaymentProduct" (
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
       public         postgres    false    2595    2597    2598    7            `           1259    272120 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_ResourcePaymentProduct_Id_seq";
       public       postgres    false    7    1889            �           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_ResourcePaymentProduct_Id_seq" OWNED BY "base_ResourcePaymentProduct"."Id";
            public       postgres    false    1888            �           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentProduct_Id_seq"', 81, true);
            public       postgres    false    1888            T           1259    270148    base_ResourcePayment_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourcePayment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourcePayment_Id_seq";
       public       postgres    false    1877    7            �           0    0    base_ResourcePayment_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourcePayment_Id_seq" OWNED BY "base_ResourcePayment"."Id";
            public       postgres    false    1876            �           0    0    base_ResourcePayment_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_ResourcePayment_Id_seq"', 323, true);
            public       postgres    false    1876            W           1259    270193    base_ResourceReturn    TABLE     B  CREATE TABLE "base_ResourceReturn" (
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
    "ReturnFee" numeric(12,2) DEFAULT 0 NOT NULL,
    "ReturnFeePercent" numeric(5,2) DEFAULT 0 NOT NULL
);
 )   DROP TABLE public."base_ResourceReturn";
       public         postgres    false    2554    2555    2557    2558    2559    2560    2561    2562    2563    2564    2565    7            �           0    0 #   COLUMN "base_ResourceReturn"."Mark"    COMMENT     ;   COMMENT ON COLUMN "base_ResourceReturn"."Mark" IS 'SO/PO';
            public       postgres    false    1879            _           1259    272099    base_ResourceReturnDetail    TABLE     �  CREATE TABLE "base_ResourceReturnDetail" (
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
       public         postgres    false    2589    2590    2592    2593    2594    7            ^           1259    272097     base_ResourceReturnDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourceReturnDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_ResourceReturnDetail_Id_seq";
       public       postgres    false    1887    7            �           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_ResourceReturnDetail_Id_seq" OWNED BY "base_ResourceReturnDetail"."Id";
            public       postgres    false    1886            �           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_ResourceReturnDetail_Id_seq"', 97, true);
            public       postgres    false    1886            V           1259    270191    base_ResourceReturn_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_ResourceReturn_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_ResourceReturn_Id_seq";
       public       postgres    false    7    1879            �           0    0    base_ResourceReturn_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_ResourceReturn_Id_seq" OWNED BY "base_ResourceReturn"."Id";
            public       postgres    false    1878            �           0    0    base_ResourceReturn_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceReturn_Id_seq"', 277, true);
            public       postgres    false    1878            C           1259    266843    base_RewardManager    TABLE     �  CREATE TABLE "base_RewardManager" (
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
       public         postgres    false    2500    2501    2502    7            �           0    0 *   COLUMN "base_RewardManager"."IsAutoEnroll"    COMMENT     p   COMMENT ON COLUMN "base_RewardManager"."IsAutoEnroll" IS 'Automatically enroll new customer in Reward Program';
            public       postgres    false    1859            �           0    0 ,   COLUMN "base_RewardManager"."IsPromptEnroll"    COMMENT     p   COMMENT ON COLUMN "base_RewardManager"."IsPromptEnroll" IS 'Prompt to enroll when making sales to non-member
';
            public       postgres    false    1859            �           0    0 -   COLUMN "base_RewardManager"."IsInformCashier"    COMMENT     l   COMMENT ON COLUMN "base_RewardManager"."IsInformCashier" IS 'Inform cashier when sales rewards are earned';
            public       postgres    false    1859            �           0    0 /   COLUMN "base_RewardManager"."IsRedemptionLimit"    COMMENT     _   COMMENT ON COLUMN "base_RewardManager"."IsRedemptionLimit" IS 'Reward redeemption limit $???';
            public       postgres    false    1859            �           0    0 /   COLUMN "base_RewardManager"."IsBlockRedemption"    COMMENT     s   COMMENT ON COLUMN "base_RewardManager"."IsBlockRedemption" IS 'Block reward redeemption for ?? days after earned';
            public       postgres    false    1859            �           0    0 3   COLUMN "base_RewardManager"."IsBlockPurchaseRedeem"    COMMENT     m   COMMENT ON COLUMN "base_RewardManager"."IsBlockPurchaseRedeem" IS 'Block reward earn with  purchase redeem';
            public       postgres    false    1859            B           1259    266841    base_RewardManager_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_RewardManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_RewardManager_Id_seq";
       public       postgres    false    1859    7            �           0    0    base_RewardManager_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_RewardManager_Id_seq" OWNED BY "base_RewardManager"."Id";
            public       postgres    false    1858            �           0    0    base_RewardManager_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_RewardManager_Id_seq"', 2, true);
            public       postgres    false    1858            A           1259    266606    base_SaleCommission    TABLE     �  CREATE TABLE "base_SaleCommission" (
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
       public         postgres    false    7            �           0    0 %   COLUMN "base_SaleCommission"."Remark"    COMMENT     U   COMMENT ON COLUMN "base_SaleCommission"."Remark" IS 'SO:SaleOrder
SR:Sale Returned';
            public       postgres    false    1857            @           1259    266604    base_SaleCommission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_SaleCommission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_SaleCommission_Id_seq";
       public       postgres    false    1857    7            �           0    0    base_SaleCommission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_SaleCommission_Id_seq" OWNED BY "base_SaleCommission"."Id";
            public       postgres    false    1856            �           0    0    base_SaleCommission_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_SaleCommission_Id_seq"', 794, true);
            public       postgres    false    1856            5           1259    266093    base_SaleOrder    TABLE     u
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
    "RefundedAmount" numeric(14,2) DEFAULT 0 NOT NULL,
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
    "IsLocked" boolean DEFAULT false NOT NULL,
    "RewardAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "Cashier" character varying(30)
);
 $   DROP TABLE public."base_SaleOrder";
       public         postgres    false    2423    2425    2426    2427    2428    2429    2430    2431    2432    2433    2434    2435    2436    2437    2438    2439    2440    2441    2442    2443    2444    2445    2446    2447    2448    2449    2450    2451    2452    2453    2454    7            �           0    0 &   COLUMN "base_SaleOrder"."RewardAmount"    COMMENT     c   COMMENT ON COLUMN "base_SaleOrder"."RewardAmount" IS 'Tong so tien can thanh toan sau khi reward';
            public       postgres    false    1845            3           1259    266084    base_SaleOrderDetail    TABLE     �  CREATE TABLE "base_SaleOrderDetail" (
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
    "OnHandQty" numeric(12,2) DEFAULT 0 NOT NULL,
    "SerialTracking" text,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "BalanceShipped" integer DEFAULT 0 NOT NULL,
    "Comment" character varying(100),
    "TotalDiscount" numeric(12,2) DEFAULT 0 NOT NULL,
    "PromotionId" integer,
    "IsManual" boolean DEFAULT false NOT NULL
);
 *   DROP TABLE public."base_SaleOrderDetail";
       public         postgres    false    2409    2410    2411    2412    2413    2414    2416    2417    2418    2419    2420    2421    2422    7            �           0    0 (   COLUMN "base_SaleOrderDetail"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_SaleOrderDetail"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100';
            public       postgres    false    1843            �           0    0 .   COLUMN "base_SaleOrderDetail"."SerialTracking"    COMMENT     Z   COMMENT ON COLUMN "base_SaleOrderDetail"."SerialTracking" IS 'Apply to Serial Tracking ';
            public       postgres    false    1843            �           0    0 .   COLUMN "base_SaleOrderDetail"."BalanceShipped"    COMMENT     s   COMMENT ON COLUMN "base_SaleOrderDetail"."BalanceShipped" IS 'Số lượng sản phẩm được vận chuyển';
            public       postgres    false    1843            �           0    0 (   COLUMN "base_SaleOrderDetail"."IsManual"    COMMENT     M   COMMENT ON COLUMN "base_SaleOrderDetail"."IsManual" IS 'Apply to promotion';
            public       postgres    false    1843            2           1259    266082    base_SaleOrderDetail_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleOrderDetail_Id_seq";
       public       postgres    false    1843    7            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleOrderDetail_Id_seq" OWNED BY "base_SaleOrderDetail"."Id";
            public       postgres    false    1842            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderDetail_Id_seq"', 705, true);
            public       postgres    false    1842            9           1259    266236    base_SaleOrderInvoice    TABLE     }  CREATE TABLE "base_SaleOrderInvoice" (
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
       public         postgres    false    2459    2460    2461    2462    2463    2464    2465    2466    7            8           1259    266234    base_SaleOrderInvoice_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderInvoice_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_SaleOrderInvoice_Id_seq";
       public       postgres    false    1849    7            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_SaleOrderInvoice_Id_seq" OWNED BY "base_SaleOrderInvoice"."Id";
            public       postgres    false    1848            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderInvoice_Id_seq"', 1, false);
            public       postgres    false    1848            7           1259    266180    base_SaleOrderShip    TABLE     �  CREATE TABLE "base_SaleOrderShip" (
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
       public         postgres    false    2456    2457    7            ;           1259    266357    base_SaleOrderShipDetail    TABLE     2  CREATE TABLE "base_SaleOrderShipDetail" (
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
       public         postgres    false    2468    7            :           1259    266355    base_SaleOrderShipDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderShipDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_SaleOrderShipDetail_Id_seq";
       public       postgres    false    1851    7            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_SaleOrderShipDetail_Id_seq" OWNED BY "base_SaleOrderShipDetail"."Id";
            public       postgres    false    1850            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_SaleOrderShipDetail_Id_seq"', 467, true);
            public       postgres    false    1850            6           1259    266178    base_SaleOrderShip_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_SaleOrderShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_SaleOrderShip_Id_seq";
       public       postgres    false    1847    7            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_SaleOrderShip_Id_seq" OWNED BY "base_SaleOrderShip"."Id";
            public       postgres    false    1846            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_SaleOrderShip_Id_seq"', 368, true);
            public       postgres    false    1846            4           1259    266091    base_SaleOrder_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_SaleOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_SaleOrder_Id_seq";
       public       postgres    false    1845    7            �           0    0    base_SaleOrder_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_SaleOrder_Id_seq" OWNED BY "base_SaleOrder"."Id";
            public       postgres    false    1844            �           0    0    base_SaleOrder_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_SaleOrder_Id_seq"', 443, true);
            public       postgres    false    1844                       1259    245103    base_SaleTaxLocation    TABLE     n  CREATE TABLE "base_SaleTaxLocation" (
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
       public         postgres    false    2284    2286    2287    2288    2289    2290    7            �           0    0 )   COLUMN "base_SaleTaxLocation"."SortIndex"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocation"."SortIndex" IS 'ParentId ==0 -> Id"-"DateTime
ParnetId !=0 -> ParentId"-"DateTime
Order By Asc';
            public       postgres    false    1795                       1259    245084    base_SaleTaxLocationOption    TABLE     (  CREATE TABLE "base_SaleTaxLocationOption" (
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
       public         postgres    false    2283    7            �           0    0 .   COLUMN "base_SaleTaxLocationOption"."ParentId"    COMMENT     h   COMMENT ON COLUMN "base_SaleTaxLocationOption"."ParentId" IS 'Apply For Multi-rate has multi tax code';
            public       postgres    false    1793            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."TaxRate"    COMMENT     k   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxRate" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1793            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxComponent"    COMMENT     Y   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxComponent" IS 'Apply For Multi-rate';
            public       postgres    false    1793            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."TaxAgency"    COMMENT     m   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxAgency" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1793            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxCondition"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxCondition" IS 'Apply For Price-Depedent: Collect this tax on an item if the unit price or shiping is more than';
            public       postgres    false    1793            �           0    0 7   COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver" IS 'Apply For Price-Depedent: Apply sale tax only to the amount over the pricing unit or shipping threshold';
            public       postgres    false    1793            �           0    0 C   COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to a specific item price range';
            public       postgres    false    1793            �           0    0 A   COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to the mount of an item''s price within this range';
            public       postgres    false    1793            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."PriceFrom"    COMMENT     V   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceFrom" IS 'Apply For Multi-rate';
            public       postgres    false    1793            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."PriceTo"    COMMENT     T   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceTo" IS 'Apply For Multi-rate';
            public       postgres    false    1793                        1259    245082 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleTaxLocationOption_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_SaleTaxLocationOption_Id_seq";
       public       postgres    false    1793    7            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_SaleTaxLocationOption_Id_seq" OWNED BY "base_SaleTaxLocationOption"."Id";
            public       postgres    false    1792            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_SaleTaxLocationOption_Id_seq"', 117, true);
            public       postgres    false    1792                       1259    245101    base_SaleTaxLocation_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleTaxLocation_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleTaxLocation_Id_seq";
       public       postgres    false    1795    7            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleTaxLocation_Id_seq" OWNED BY "base_SaleTaxLocation"."Id";
            public       postgres    false    1794            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleTaxLocation_Id_seq"', 370, true);
            public       postgres    false    1794                       1259    255675 
   base_Store    TABLE     �   CREATE TABLE "base_Store" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(30),
    "Street" character varying(200),
    "City" character varying(200)
);
     DROP TABLE public."base_Store";
       public         postgres    false    7                       1259    255673    base_Store_Id_seq    SEQUENCE     u   CREATE SEQUENCE "base_Store_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."base_Store_Id_seq";
       public       postgres    false    7    1818            �           0    0    base_Store_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Store_Id_seq" OWNED BY "base_Store"."Id";
            public       postgres    false    1817            �           0    0    base_Store_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"base_Store_Id_seq"', 46, true);
            public       postgres    false    1817            O           1259    269925    base_TransferStock    TABLE     �  CREATE TABLE "base_TransferStock" (
    "Id" bigint NOT NULL,
    "TransferNo" character varying(12) NOT NULL,
    "FromStore" smallint DEFAULT 0 NOT NULL,
    "ToStore" smallint DEFAULT 0 NOT NULL,
    "TotalQuantity" numeric(12,2) DEFAULT 0 NOT NULL,
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
       public         postgres    false    2524    2526    2527    2528    2529    2530    2531    2532    2533    7            Q           1259    269941    base_TransferStockDetail    TABLE     |  CREATE TABLE "base_TransferStockDetail" (
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
       public         postgres    false    2535    2536    2537    7            P           1259    269939    base_TransferStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_TransferStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_TransferStockDetail_Id_seq";
       public       postgres    false    7    1873            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_TransferStockDetail_Id_seq" OWNED BY "base_TransferStockDetail"."Id";
            public       postgres    false    1872            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_TransferStockDetail_Id_seq"', 42, true);
            public       postgres    false    1872            N           1259    269923    base_TransferStock_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_TransferStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_TransferStock_Id_seq";
       public       postgres    false    7    1871            �           0    0    base_TransferStock_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_TransferStock_Id_seq" OWNED BY "base_TransferStock"."Id";
            public       postgres    false    1870            �           0    0    base_TransferStock_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_TransferStock_Id_seq"', 29, true);
            public       postgres    false    1870            	           1259    245147    base_UOM    TABLE     �  CREATE TABLE "base_UOM" (
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
       public         postgres    false    2298    2299    2300    7                       1259    245145    base_UOM_Id_seq    SEQUENCE     s   CREATE SEQUENCE "base_UOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."base_UOM_Id_seq";
       public       postgres    false    1801    7            �           0    0    base_UOM_Id_seq    SEQUENCE OWNED BY     ;   ALTER SEQUENCE "base_UOM_Id_seq" OWNED BY "base_UOM"."Id";
            public       postgres    false    1800            �           0    0    base_UOM_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"base_UOM_Id_seq"', 115, true);
            public       postgres    false    1800                       1259    245131    base_UserLog    TABLE     #  CREATE TABLE "base_UserLog" (
    "Id" bigint NOT NULL,
    "IpSource" character varying(17),
    "ConnectedOn" timestamp without time zone DEFAULT now() NOT NULL,
    "DisConnectedOn" timestamp without time zone,
    "ResourceAccessed" character varying(36),
    "IsDisconected" boolean
);
 "   DROP TABLE public."base_UserLog";
       public         postgres    false    2296    7            �           1259    244282    base_UserLogDetail    TABLE     �   CREATE TABLE "base_UserLogDetail" (
    "Id" uuid NOT NULL,
    "UserLogId" bigint,
    "AccessedTime" timestamp without time zone,
    "ModuleName" character varying(30),
    "ActionDescription" character varying(200)
);
 (   DROP TABLE public."base_UserLogDetail";
       public         postgres    false    7                       1259    245129    base_UserLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_UserLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_UserLog_Id_seq";
       public       postgres    false    7    1799            �           0    0    base_UserLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_UserLog_Id_seq" OWNED BY "base_UserLog"."Id";
            public       postgres    false    1798            �           0    0    base_UserLog_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_UserLog_Id_seq"', 2658, true);
            public       postgres    false    1798            .           1259    256244    base_UserRight    TABLE     �   CREATE TABLE "base_UserRight" (
    "Id" integer NOT NULL,
    "Code" character varying(5) NOT NULL,
    "Name" character varying(200)
);
 $   DROP TABLE public."base_UserRight";
       public         postgres    false    7            -           1259    256242    base_UserRight_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_UserRight_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_UserRight_Id_seq";
       public       postgres    false    7    1838            �           0    0    base_UserRight_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_UserRight_Id_seq" OWNED BY "base_UserRight"."Id";
            public       postgres    false    1837            �           0    0    base_UserRight_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_UserRight_Id_seq"', 187, true);
            public       postgres    false    1837            L           1259    269643    base_VendorProduct    TABLE       CREATE TABLE "base_VendorProduct" (
    "Id" integer NOT NULL,
    "ProductId" bigint NOT NULL,
    "VendorId" bigint NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "VendorResource" character varying(36) NOT NULL
);
 (   DROP TABLE public."base_VendorProduct";
       public         postgres    false    2523    7            M           1259    269646    base_VendorProduct_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VendorProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VendorProduct_Id_seq";
       public       postgres    false    7    1868            �           0    0    base_VendorProduct_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VendorProduct_Id_seq" OWNED BY "base_VendorProduct"."Id";
            public       postgres    false    1869            �           0    0    base_VendorProduct_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VendorProduct_Id_seq"', 40, true);
            public       postgres    false    1869                       1259    245115    base_VirtualFolder    TABLE     �  CREATE TABLE "base_VirtualFolder" (
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
       public         postgres    false    2292    2293    2294    7                       1259    245113    base_VirtualFolder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VirtualFolder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VirtualFolder_Id_seq";
       public       postgres    false    1797    7            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VirtualFolder_Id_seq" OWNED BY "base_VirtualFolder"."Id";
            public       postgres    false    1796            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VirtualFolder_Id_seq"', 66, true);
            public       postgres    false    1796            b           1259    282433 	   rpt_Group    TABLE     �   CREATE TABLE "rpt_Group" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(200),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(30)
);
    DROP TABLE public."rpt_Group";
       public         postgres    false    7            c           1259    282436    rpt_Group_Id_seq    SEQUENCE     t   CREATE SEQUENCE "rpt_Group_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."rpt_Group_Id_seq";
       public       postgres    false    1890    7            �           0    0    rpt_Group_Id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE "rpt_Group_Id_seq" OWNED BY "rpt_Group"."Id";
            public       postgres    false    1891            �           0    0    rpt_Group_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"rpt_Group_Id_seq"', 1, false);
            public       postgres    false    1891            k           1259    283470 
   rpt_Report    TABLE     [  CREATE TABLE "rpt_Report" (
    "Id" integer NOT NULL,
    "GroupId" integer DEFAULT 0 NOT NULL,
    "ParentId" integer DEFAULT 0 NOT NULL,
    "Code" character varying(4) NOT NULL,
    "Name" character varying(200),
    "FormatFile" character varying(50),
    "IsShow" boolean DEFAULT false NOT NULL,
    "ProcessName" character varying(50),
    "SamplePicture" bytea,
    "PrintTimes" integer,
    "LastPrintDate" timestamp without time zone,
    "LastPrintUser" character varying(30),
    "ExcelFile" character varying(50),
    "PrinterName" character varying(100),
    "PrintCopy" smallint DEFAULT 0,
    "PaperSize" character varying(30),
    "Remark" character varying(200),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(35),
    "DateUpdated" timestamp without time zone,
    "UserUpdated" character varying(35)
);
     DROP TABLE public."rpt_Report";
       public         postgres    false    2618    2619    2620    2621    7            j           1259    283468    rpt_Report_Id_seq    SEQUENCE     u   CREATE SEQUENCE "rpt_Report_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."rpt_Report_Id_seq";
       public       postgres    false    1899    7            �           0    0    rpt_Report_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "rpt_Report_Id_seq" OWNED BY "rpt_Report"."Id";
            public       postgres    false    1898            �           0    0    rpt_Report_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"rpt_Report_Id_seq"', 1, false);
            public       postgres    false    1898                       1259    255696    tims_Holiday    TABLE     #  CREATE TABLE "tims_Holiday" (
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
       public         postgres    false    7                       1259    255705    tims_HolidayHistory    TABLE     {   CREATE TABLE "tims_HolidayHistory" (
    "Date" timestamp without time zone NOT NULL,
    "Name" character varying(200)
);
 )   DROP TABLE public."tims_HolidayHistory";
       public         postgres    false    7                       1259    255694    tims_Holiday_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_Holiday_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_Holiday_Id_seq";
       public       postgres    false    7    1820            �           0    0    tims_Holiday_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_Holiday_Id_seq" OWNED BY "tims_Holiday"."Id";
            public       postgres    false    1819            �           0    0    tims_Holiday_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_Holiday_Id_seq"', 10, true);
            public       postgres    false    1819            %           1259    255849    tims_TimeLog    TABLE     <  CREATE TABLE "tims_TimeLog" (
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
       public         postgres    false    7            '           1259    255865    tims_TimeLogPermission    TABLE     u   CREATE TABLE "tims_TimeLogPermission" (
    "TimeLogId" integer NOT NULL,
    "WorkPermissionId" integer NOT NULL
);
 ,   DROP TABLE public."tims_TimeLogPermission";
       public         postgres    false    7            &           1259    255863 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE     �   CREATE SEQUENCE "tims_TimeLogPermission_TimeLogId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 =   DROP SEQUENCE public."tims_TimeLogPermission_TimeLogId_seq";
       public       postgres    false    7    1831            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE "tims_TimeLogPermission_TimeLogId_seq" OWNED BY "tims_TimeLogPermission"."TimeLogId";
            public       postgres    false    1830            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('"tims_TimeLogPermission_TimeLogId_seq"', 1, false);
            public       postgres    false    1830            $           1259    255847    tims_TimeLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_TimeLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_TimeLog_Id_seq";
       public       postgres    false    1829    7            �           0    0    tims_TimeLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_TimeLog_Id_seq" OWNED BY "tims_TimeLog"."Id";
            public       postgres    false    1828            �           0    0    tims_TimeLog_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_TimeLog_Id_seq"', 11, true);
            public       postgres    false    1828            #           1259    255795    tims_WorkPermission    TABLE     Z  CREATE TABLE "tims_WorkPermission" (
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
       public         postgres    false    7            "           1259    255793    tims_WorkPermission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "tims_WorkPermission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."tims_WorkPermission_Id_seq";
       public       postgres    false    7    1827            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "tims_WorkPermission_Id_seq" OWNED BY "tims_WorkPermission"."Id";
            public       postgres    false    1826            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"tims_WorkPermission_Id_seq"', 7, true);
            public       postgres    false    1826                       1259    255738    tims_WorkSchedule    TABLE     �  CREATE TABLE "tims_WorkSchedule" (
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
       public         postgres    false    7                       1259    255736    tims_WorkSchedule_Id_seq    SEQUENCE     |   CREATE SEQUENCE "tims_WorkSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."tims_WorkSchedule_Id_seq";
       public       postgres    false    1823    7            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "tims_WorkSchedule_Id_seq" OWNED BY "tims_WorkSchedule"."Id";
            public       postgres    false    1822            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"tims_WorkSchedule_Id_seq"', 28, true);
            public       postgres    false    1822            !           1259    255781    tims_WorkWeek    TABLE     �  CREATE TABLE "tims_WorkWeek" (
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
       public         postgres    false    7                        1259    255779    tims_WorkWeek_Id_seq    SEQUENCE     x   CREATE SEQUENCE "tims_WorkWeek_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."tims_WorkWeek_Id_seq";
       public       postgres    false    1825    7            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "tims_WorkWeek_Id_seq" OWNED BY "tims_WorkWeek"."Id";
            public       postgres    false    1824            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"tims_WorkWeek_Id_seq"', 187, true);
            public       postgres    false    1824            �           2604    235589    jexid    DEFAULT     g   ALTER TABLE pga_exception ALTER COLUMN jexid SET DEFAULT nextval('pga_exception_jexid_seq'::regclass);
 C   ALTER TABLE pgagent.pga_exception ALTER COLUMN jexid DROP DEFAULT;
       pgagent       postgres    false    1755    1754            �           2604    235590    jobid    DEFAULT     [   ALTER TABLE pga_job ALTER COLUMN jobid SET DEFAULT nextval('pga_job_jobid_seq'::regclass);
 =   ALTER TABLE pgagent.pga_job ALTER COLUMN jobid DROP DEFAULT;
       pgagent       postgres    false    1757    1756            �           2604    235591    jclid    DEFAULT     e   ALTER TABLE pga_jobclass ALTER COLUMN jclid SET DEFAULT nextval('pga_jobclass_jclid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_jobclass ALTER COLUMN jclid DROP DEFAULT;
       pgagent       postgres    false    1760    1759            �           2604    235592    jlgid    DEFAULT     a   ALTER TABLE pga_joblog ALTER COLUMN jlgid SET DEFAULT nextval('pga_joblog_jlgid_seq'::regclass);
 @   ALTER TABLE pgagent.pga_joblog ALTER COLUMN jlgid DROP DEFAULT;
       pgagent       postgres    false    1762    1761            �           2604    235593    jstid    DEFAULT     c   ALTER TABLE pga_jobstep ALTER COLUMN jstid SET DEFAULT nextval('pga_jobstep_jstid_seq'::regclass);
 A   ALTER TABLE pgagent.pga_jobstep ALTER COLUMN jstid DROP DEFAULT;
       pgagent       postgres    false    1764    1763            �           2604    235594    jslid    DEFAULT     i   ALTER TABLE pga_jobsteplog ALTER COLUMN jslid SET DEFAULT nextval('pga_jobsteplog_jslid_seq'::regclass);
 D   ALTER TABLE pgagent.pga_jobsteplog ALTER COLUMN jslid DROP DEFAULT;
       pgagent       postgres    false    1766    1765            �           2604    235595    jscid    DEFAULT     e   ALTER TABLE pga_schedule ALTER COLUMN jscid SET DEFAULT nextval('pga_schedule_jscid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_schedule ALTER COLUMN jscid DROP DEFAULT;
       pgagent       postgres    false    1768    1767            �           2604    244949    Id    DEFAULT     k   ALTER TABLE "base_Attachment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Attachment_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Attachment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1787    1786    1787            `	           2604    256171    Id    DEFAULT     i   ALTER TABLE "base_Authorize" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Authorize_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Authorize" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1833    1834    1834            E	           2604    257304    Id    DEFAULT     q   ALTER TABLE "base_Configuration" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Configuration_Id_seq"'::regclass);
 H   ALTER TABLE public."base_Configuration" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1839    1814            0
           2604    283363    Id    DEFAULT     w   ALTER TABLE "base_CostAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustmentItem_Id_seq"'::regclass);
 I   ALTER TABLE public."base_CostAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1897    1896    1897            
           2604    271741    Id    DEFAULT     k   ALTER TABLE "base_CountStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStock_Id_seq"'::regclass);
 E   ALTER TABLE public."base_CountStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1883    1882    1883            
           2604    271748    Id    DEFAULT     w   ALTER TABLE "base_CountStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStockDetail_Id_seq"'::regclass);
 K   ALTER TABLE public."base_CountStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1884    1885    1885            	           2604    245343    Id    DEFAULT     k   ALTER TABLE "base_Department" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Department_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Department" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1806    1807    1807            �           2604    244820    Id    DEFAULT     a   ALTER TABLE "base_Guest" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Guest_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Guest" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1775    1774    1775            	           2604    245379    Id    DEFAULT     u   ALTER TABLE "base_GuestAdditional" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAdditional_Id_seq"'::regclass);
 J   ALTER TABLE public."base_GuestAdditional" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1808    1809    1809            �           2604    244866    Id    DEFAULT     o   ALTER TABLE "base_GuestAddress" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAddress_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestAddress" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1777    1776    1777            �           2604    238416    Id    DEFAULT     w   ALTER TABLE "base_GuestFingerPrint" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestFingerPrint_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestFingerPrint" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1772    1771    1772            �           2604    244876    Id    DEFAULT     {   ALTER TABLE "base_GuestHiringHistory" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestHiringHistory_Id_seq"'::regclass);
 M   ALTER TABLE public."base_GuestHiringHistory" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1779    1778    1779            �           2604    244887    Id    DEFAULT     o   ALTER TABLE "base_GuestPayRoll" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPayRoll_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestPayRoll" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1780    1781    1781            f	           2604    257328    Id    DEFAULT     w   ALTER TABLE "base_GuestPaymentCard" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPaymentCard_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestPaymentCard" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1840    1841    1841            �           2604    244937    Id    DEFAULT     o   ALTER TABLE "base_GuestProfile" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestProfile_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestProfile" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1785    1784    1785            �	           2604    268357    Id    DEFAULT     m   ALTER TABLE "base_GuestReward" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestReward_Id_seq"'::regclass);
 F   ALTER TABLE public."base_GuestReward" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1865    1864    1865            (
           2604    282551    Id    DEFAULT     g   ALTER TABLE "base_Language" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Language_Id_seq"'::regclass);
 C   ALTER TABLE public."base_Language" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1892    1893    1893            �           2604    245000    Id    DEFAULT     k   ALTER TABLE "base_MemberShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_MemberShip_Id_seq"'::regclass);
 E   ALTER TABLE public."base_MemberShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1789    1788    1789            �	           2604    268514    Id    DEFAULT     q   ALTER TABLE "base_PricingChange" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingChange_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PricingChange" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1867    1866    1867            �	           2604    268188    Id    DEFAULT     s   ALTER TABLE "base_PricingManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingManager_Id_seq"'::regclass);
 I   ALTER TABLE public."base_PricingManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1863    1862    1863            	           2604    245415    Id    DEFAULT     e   ALTER TABLE "base_Product" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Product_Id_seq"'::regclass);
 B   ALTER TABLE public."base_Product" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1811    1810    1811            W	           2604    255539    Id    DEFAULT     o   ALTER TABLE "base_ProductStore" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductStore_Id_seq"'::regclass);
 G   ALTER TABLE public."base_ProductStore" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1815    1816    1816            
           2604    270255    Id    DEFAULT     k   ALTER TABLE "base_ProductUOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductUOM_Id_seq"'::regclass);
 E   ALTER TABLE public."base_ProductUOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1881    1880    1881            	           2604    245172    Id    DEFAULT     i   ALTER TABLE "base_Promotion" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Promotion_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Promotion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1804    1805    1805            �           2604    245158    Id    DEFAULT     u   ALTER TABLE "base_PromotionAffect" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionAffect_Id_seq"'::regclass);
 J   ALTER TABLE public."base_PromotionAffect" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1803    1802    1803            �           2604    245026    Id    DEFAULT     y   ALTER TABLE "base_PromotionSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionSchedule_Id_seq"'::regclass);
 L   ALTER TABLE public."base_PromotionSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1791    1790    1791            �	           2604    266554    Id    DEFAULT     q   ALTER TABLE "base_PurchaseOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PurchaseOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1855    1854    1855            �	           2604    266533    Id    DEFAULT     }   ALTER TABLE "base_PurchaseOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_PurchaseOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1852    1853    1853            �	           2604    267538    Id    DEFAULT        ALTER TABLE "base_PurchaseOrderReceive" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderReceive_Id_seq"'::regclass);
 O   ALTER TABLE public."base_PurchaseOrderReceive" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1861    1860    1861            /
           2604    282645    Id    DEFAULT        ALTER TABLE "base_QuantityAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustmentItem_Id_seq"'::regclass);
 M   ALTER TABLE public."base_QuantityAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1895    1894    1895            a	           2604    256181    Id    DEFAULT     u   ALTER TABLE "base_ResourceAccount" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceAccount_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourceAccount" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1836    1835    1836            5	           2604    246086    Id    DEFAULT     o   ALTER TABLE "base_ResourceNote" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceNote_id_seq"'::regclass);
 G   ALTER TABLE public."base_ResourceNote" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1813    1812    1813            �	           2604    270153    Id    DEFAULT     u   ALTER TABLE "base_ResourcePayment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePayment_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourcePayment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1877    1876    1877            �	           2604    270075    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentDetail_Id_seq"'::regclass);
 P   ALTER TABLE public."base_ResourcePaymentDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1875    1874    1875            $
           2604    272125    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentProduct_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_ResourcePaymentProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1888    1889    1889            �           2604    244925    Id    DEFAULT     n   ALTER TABLE "base_ResourcePhoto" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPhoto_Id_seq"'::regclass);
 H   ALTER TABLE public."base_ResourcePhoto" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1783    1782    1783            �	           2604    270196    Id    DEFAULT     s   ALTER TABLE "base_ResourceReturn" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturn_Id_seq"'::regclass);
 I   ALTER TABLE public."base_ResourceReturn" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1879    1878    1879            
           2604    272102    Id    DEFAULT        ALTER TABLE "base_ResourceReturnDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturnDetail_Id_seq"'::regclass);
 O   ALTER TABLE public."base_ResourceReturnDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1886    1887    1887            �	           2604    266846    Id    DEFAULT     q   ALTER TABLE "base_RewardManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_RewardManager_Id_seq"'::regclass);
 H   ALTER TABLE public."base_RewardManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1858    1859    1859            �	           2604    266609    Id    DEFAULT     s   ALTER TABLE "base_SaleCommission" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleCommission_Id_seq"'::regclass);
 I   ALTER TABLE public."base_SaleCommission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1856    1857    1857            x	           2604    266096    Id    DEFAULT     i   ALTER TABLE "base_SaleOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrder_Id_seq"'::regclass);
 D   ALTER TABLE public."base_SaleOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1844    1845    1845            o	           2604    266087    Id    DEFAULT     u   ALTER TABLE "base_SaleOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderDetail_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1843    1842    1843            �	           2604    266239    Id    DEFAULT     w   ALTER TABLE "base_SaleOrderInvoice" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderInvoice_Id_seq"'::regclass);
 K   ALTER TABLE public."base_SaleOrderInvoice" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1848    1849    1849            �	           2604    266183    Id    DEFAULT     q   ALTER TABLE "base_SaleOrderShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShip_Id_seq"'::regclass);
 H   ALTER TABLE public."base_SaleOrderShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1847    1846    1847            �	           2604    266360    Id    DEFAULT     }   ALTER TABLE "base_SaleOrderShipDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShipDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_SaleOrderShipDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1851    1850    1851            �           2604    245106    Id    DEFAULT     u   ALTER TABLE "base_SaleTaxLocation" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocation_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleTaxLocation" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1794    1795    1795            �           2604    245087    Id    DEFAULT     �   ALTER TABLE "base_SaleTaxLocationOption" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocationOption_Id_seq"'::regclass);
 P   ALTER TABLE public."base_SaleTaxLocationOption" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1792    1793    1793            Y	           2604    255678    Id    DEFAULT     a   ALTER TABLE "base_Store" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Store_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Store" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1817    1818    1818            �	           2604    269928    Id    DEFAULT     q   ALTER TABLE "base_TransferStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStock_Id_seq"'::regclass);
 H   ALTER TABLE public."base_TransferStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1870    1871    1871            �	           2604    269944    Id    DEFAULT     }   ALTER TABLE "base_TransferStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStockDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_TransferStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1872    1873    1873            �           2604    245150    Id    DEFAULT     ]   ALTER TABLE "base_UOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UOM_Id_seq"'::regclass);
 >   ALTER TABLE public."base_UOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1800    1801    1801            �           2604    245134    Id    DEFAULT     e   ALTER TABLE "base_UserLog" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserLog_Id_seq"'::regclass);
 B   ALTER TABLE public."base_UserLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1798    1799    1799            e	           2604    256247    Id    DEFAULT     i   ALTER TABLE "base_UserRight" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserRight_Id_seq"'::regclass);
 D   ALTER TABLE public."base_UserRight" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1837    1838    1838            �	           2604    269648    Id    DEFAULT     q   ALTER TABLE "base_VendorProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VendorProduct_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VendorProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1869    1868            �           2604    245118    Id    DEFAULT     q   ALTER TABLE "base_VirtualFolder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VirtualFolder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VirtualFolder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1797    1796    1797            '
           2604    282438    Id    DEFAULT     _   ALTER TABLE "rpt_Group" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Group_Id_seq"'::regclass);
 ?   ALTER TABLE public."rpt_Group" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1891    1890            9
           2604    283473    Id    DEFAULT     a   ALTER TABLE "rpt_Report" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Report_Id_seq"'::regclass);
 @   ALTER TABLE public."rpt_Report" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1899    1898    1899            Z	           2604    255699    Id    DEFAULT     e   ALTER TABLE "tims_Holiday" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_Holiday_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_Holiday" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1819    1820    1820            ^	           2604    255852    Id    DEFAULT     e   ALTER TABLE "tims_TimeLog" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_TimeLog_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_TimeLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1828    1829    1829            _	           2604    255868 	   TimeLogId    DEFAULT     �   ALTER TABLE "tims_TimeLogPermission" ALTER COLUMN "TimeLogId" SET DEFAULT nextval('"tims_TimeLogPermission_TimeLogId_seq"'::regclass);
 S   ALTER TABLE public."tims_TimeLogPermission" ALTER COLUMN "TimeLogId" DROP DEFAULT;
       public       postgres    false    1831    1830    1831            ]	           2604    255798    Id    DEFAULT     s   ALTER TABLE "tims_WorkPermission" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkPermission_Id_seq"'::regclass);
 I   ALTER TABLE public."tims_WorkPermission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1826    1827    1827            [	           2604    255741    Id    DEFAULT     o   ALTER TABLE "tims_WorkSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkSchedule_Id_seq"'::regclass);
 G   ALTER TABLE public."tims_WorkSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1823    1822    1823            \	           2604    255784    Id    DEFAULT     g   ALTER TABLE "tims_WorkWeek" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkWeek_Id_seq"'::regclass);
 C   ALTER TABLE public."tims_WorkWeek" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1825    1824    1825            n          0    234865    pga_exception 
   TABLE DATA               B   COPY pga_exception (jexid, jexscid, jexdate, jextime) FROM stdin;
    pgagent       postgres    false    1754   ��      o          0    234870    pga_job 
   TABLE DATA               �   COPY pga_job (jobid, jobjclid, jobname, jobdesc, jobhostagent, jobenabled, jobcreated, jobchanged, jobagentid, jobnextrun, joblastrun) FROM stdin;
    pgagent       postgres    false    1756   �      p          0    234883    pga_jobagent 
   TABLE DATA               A   COPY pga_jobagent (jagpid, jaglogintime, jagstation) FROM stdin;
    pgagent       postgres    false    1758   6�      q          0    234890    pga_jobclass 
   TABLE DATA               /   COPY pga_jobclass (jclid, jclname) FROM stdin;
    pgagent       postgres    false    1759   S�      r          0    234898 
   pga_joblog 
   TABLE DATA               P   COPY pga_joblog (jlgid, jlgjobid, jlgstatus, jlgstart, jlgduration) FROM stdin;
    pgagent       postgres    false    1761   ��      s          0    234906    pga_jobstep 
   TABLE DATA               �   COPY pga_jobstep (jstid, jstjobid, jstname, jstdesc, jstenabled, jstkind, jstcode, jstconnstr, jstdbname, jstonerror, jscnextrun) FROM stdin;
    pgagent       postgres    false    1763   ��      t          0    234923    pga_jobsteplog 
   TABLE DATA               t   COPY pga_jobsteplog (jslid, jsljlgid, jsljstid, jslstatus, jslresult, jslstart, jslduration, jsloutput) FROM stdin;
    pgagent       postgres    false    1765   ��      u          0    234934    pga_schedule 
   TABLE DATA               �   COPY pga_schedule (jscid, jscjobid, jscname, jscdesc, jscenabled, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths) FROM stdin;
    pgagent       postgres    false    1767   �      �          0    244946    base_Attachment 
   TABLE DATA               �   COPY "base_Attachment" ("Id", "FileOriginalName", "FileName", "FileExtension", "VirtualFolderId", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Counter") FROM stdin;
    public       postgres    false    1787   /�      �          0    256168    base_Authorize 
   TABLE DATA               =   COPY "base_Authorize" ("Id", "Resource", "Code") FROM stdin;
    public       postgres    false    1834   �      �          0    254557    base_Configuration 
   TABLE DATA               �  COPY "base_Configuration" ("CompanyName", "Address", "City", "State", "ZipCode", "CountryId", "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", "IsAllowRGO", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", "ReturnFeePercent", "IsRewardLessThanDiscount", "CurrencySymbol", "DecimalPlaces", "FomartCurrency", "PasswordLength") FROM stdin;
    public       postgres    false    1814   o�      �          0    283360    base_CostAdjustment 
   TABLE DATA                 COPY "base_CostAdjustment" ("Id", "ProductId", "ProductResource", "CostDifference", "NewCost", "OldCost", "AdjustmentNewCost", "AdjustmentOldCost", "AdjustCostDifference", "LoggedTime", "Reason", "Status", "UserCreated", "IsReversed", "StoreCode") FROM stdin;
    public       postgres    false    1897   ��      �          0    271738    base_CountStock 
   TABLE DATA               �   COPY "base_CountStock" ("Id", "DocumentNo", "DateCreated", "UserCreated", "CompletedDate", "UserCounted", "Status", "Resource") FROM stdin;
    public       postgres    false    1883   ��      �          0    271745    base_CountStockDetail 
   TABLE DATA               �   COPY "base_CountStockDetail" ("Id", "CountStockId", "ProductId", "ProductResource", "StoreId", "Quantity", "CountedQuantity", "Difference") FROM stdin;
    public       postgres    false    1885   1�      �          0    245340    base_Department 
   TABLE DATA               �   COPY "base_Department" ("Id", "Name", "ParentId", "TaxCodeId", "Margin", "MarkUp", "LevelId", "IsActived", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated") FROM stdin;
    public       postgres    false    1807   ��      w          0    238237 
   base_Email 
   TABLE DATA               �  COPY "base_Email" ("Id", "Recipient", "CC", "BCC", "Subject", "Body", "IsHasAttachment", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "AttachmentType", "AttachmentResult", "GuestId", "Sender", "Status", "Importance", "Sensitivity", "IsRequestDelivery", "IsRequestRead", "IsMyFlag", "FlagTo", "FlagStartDate", "FlagDueDate", "IsAllowReminder", "RemindOn", "MyRemindTimes", "IsRecipentFlag", "RecipentFlagTo", "IsAllowRecipentReminder", "RecipentRemindOn", "RecipentRemindTimes") FROM stdin;
    public       postgres    false    1770   ��      v          0    238137    base_EmailAttachment 
   TABLE DATA               J   COPY "base_EmailAttachment" ("Id", "EmailId", "AttachmentId") FROM stdin;
    public       postgres    false    1769   ��      z          0    244817 
   base_Guest 
   TABLE DATA                 COPY "base_Guest" ("Id", "FirstName", "MiddleName", "LastName", "Company", "Phone1", "Ext1", "Phone2", "Ext2", "Fax", "CellPhone", "Email", "Website", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "IsPurged", "GuestTypeId", "IsActived", "GuestNo", "PositionId", "Department", "Mark", "AccountNumber", "ParentId", "IsRewardMember", "CheckLimit", "CreditLimit", "BalanceDue", "AvailCredit", "PastDue", "IsPrimary", "CommissionPercent", "Resource", "TotalRewardRedeemed", "PurchaseDuringTrackingPeriod", "RequirePurchaseNextReward", "HireDate", "IsBlockArriveLate", "IsDeductLunchTime", "IsBalanceOvertime", "LateMinutes", "OvertimeOption", "OTLeastMinute", "IsTrackingHour", "TermDiscount", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "SaleRepId") FROM stdin;
    public       postgres    false    1775   ��      �          0    245376    base_GuestAdditional 
   TABLE DATA               3  COPY "base_GuestAdditional" ("Id", "TaxRate", "IsNoDiscount", "FixDiscount", "Unit", "PriceSchemeId", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Custom8", "GuestId", "LayawayNo", "ChargeACNo", "FedTaxId", "IsTaxExemption", "SaleTaxLocation", "TaxExemptionNo") FROM stdin;
    public       postgres    false    1809   Y      {          0    244863    base_GuestAddress 
   TABLE DATA               �   COPY "base_GuestAddress" ("Id", "GuestId", "AddressTypeId", "AddressLine1", "AddressLine2", "City", "StateProvinceId", "PostalCode", "CountryId", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsDefault") FROM stdin;
    public       postgres    false    1777   K      x          0    238413    base_GuestFingerPrint 
   TABLE DATA               �   COPY "base_GuestFingerPrint" ("Id", "GuestId", "FingerIndex", "HandFlag", "DateUpdated", "UserUpdaed", "FingerPrintImage") FROM stdin;
    public       postgres    false    1772   
      |          0    244873    base_GuestHiringHistory 
   TABLE DATA               �   COPY "base_GuestHiringHistory" ("Id", "GuestId", "StartDate", "RenewDate", "PromotionDate", "TerminateDate", "IsTerminate", "ManagerId") FROM stdin;
    public       postgres    false    1779   �      }          0    244884    base_GuestPayRoll 
   TABLE DATA               �   COPY "base_GuestPayRoll" ("Id", "PayrollName", "PayrollType", "Rate", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "GuestId") FROM stdin;
    public       postgres    false    1781   �      �          0    257325    base_GuestPaymentCard 
   TABLE DATA               �   COPY "base_GuestPaymentCard" ("Id", "GuestId", "CardTypeId", "CardNumber", "ExpMonth", "ExpYear", "CCID", "BillingAddress", "NameOnCard", "ZipCode", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1841   �                0    244934    base_GuestProfile 
   TABLE DATA               s  COPY "base_GuestProfile" ("Id", "Gender", "Marital", "SSN", "Identification", "DOB", "IsSpouse", "FirstName", "LastName", "MiddleName", "State", "SGender", "SFirstName", "SLastName", "SMiddleName", "SPhone", "SCellPhone", "SSSN", "SState", "SEmail", "IsEmergency", "EFirstName", "ELastName", "EMiddleName", "EPhone", "ECellPhone", "ERelationship", "GuestId") FROM stdin;
    public       postgres    false    1785   �      �          0    268354    base_GuestReward 
   TABLE DATA               �   COPY "base_GuestReward" ("Id", "GuestId", "RewardId", "Amount", "IsApply", "EarnedDate", "AppliedDate", "RewardValue", "SaleOrderResource", "SaleOrderNo", "Remark", "ActivedDate", "ExpireDate", "Reason", "Status") FROM stdin;
    public       postgres    false    1865   �      �          0    256013    base_GuestSchedule 
   TABLE DATA               i   COPY "base_GuestSchedule" ("GuestId", "WorkScheduleId", "StartDate", "AssignDate", "Status") FROM stdin;
    public       postgres    false    1832   N       �          0    282548    base_Language 
   TABLE DATA               S   COPY "base_Language" ("Id", "Code", "Name", "Flag", "IsLocked", "Xml") FROM stdin;
    public       postgres    false    1893   �       �          0    244997    base_MemberShip 
   TABLE DATA               �   COPY "base_MemberShip" ("Id", "GuestId", "MemberType", "CardNumber", "Status", "IsPurged", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "Code", "TotalRewardRedeemed") FROM stdin;
    public       postgres    false    1789   �       �          0    268511    base_PricingChange 
   TABLE DATA               �   COPY "base_PricingChange" ("Id", "PricingManagerId", "PricingManagerResource", "ProductId", "ProductResource", "Cost", "CurrentPrice", "NewPrice", "PriceChanged", "DateCreated") FROM stdin;
    public       postgres    false    1867   !      �          0    268185    base_PricingManager 
   TABLE DATA               +  COPY "base_PricingManager" ("Id", "Name", "Description", "DateCreated", "UserCreated", "DateApplied", "UserApplied", "DateRestored", "UserRestored", "AffectPricing", "Resource", "PriceLevel", "Status", "BasePrice", "CalculateMethod", "AmountChange", "AmountUnit", "ItemCount", "Reason") FROM stdin;
    public       postgres    false    1863   7!      �          0    245412    base_Product 
   TABLE DATA               �  COPY "base_Product" ("Id", "Code", "ItemTypeId", "ProductDepartmentId", "ProductCategoryId", "ProductBrandId", "StyleModel", "ProductName", "Description", "Barcode", "Attribute", "Size", "IsSerialTracking", "IsPublicWeb", "OnHandStore1", "OnHandStore2", "OnHandStore3", "OnHandStore4", "OnHandStore5", "OnHandStore6", "OnHandStore7", "OnHandStore8", "OnHandStore9", "OnHandStore10", "QuantityOnHand", "QuantityOnOrder", "CompanyReOrderPoint", "IsUnOrderAble", "IsEligibleForCommission", "IsEligibleForReward", "RegularPrice", "Price1", "Price2", "Price3", "Price4", "OrderCost", "AverageUnitCost", "TaxCode", "MarginPercent", "MarkupPercent", "BaseUOMId", "GroupAttribute", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Resource", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "WarrantyType", "WarrantyNumber", "WarrantyPeriod", "PartNumber", "SellUOMId", "OrderUOMId", "IsPurge", "VendorId", "UserAssignedCommission", "AssignedCommissionPercent", "AssignedCommissionAmount", "Serial", "OrderUOM", "MarkdownPercent1", "MarkdownPercent2", "MarkdownPercent3", "MarkdownPercent4", "IsOpenItem", "Location") FROM stdin;
    public       postgres    false    1811   $      �          0    255536    base_ProductStore 
   TABLE DATA               X   COPY "base_ProductStore" ("Id", "ProductId", "QuantityOnHand", "StoreCode") FROM stdin;
    public       postgres    false    1816   =+      �          0    270252    base_ProductUOM 
   TABLE DATA               "  COPY "base_ProductUOM" ("Id", "ProductStoreId", "UOMId", "BaseUnitNumber", "RegularPrice", "QuantityOnHand", "AverageCost", "Price1", "Price2", "Price3", "Price4", "MarkDownPercent1", "MarkDownPercent2", "MarkDownPercent3", "MarkDownPercent4", "MarginPercent", "MarkupPercent") FROM stdin;
    public       postgres    false    1881   ,      �          0    245169    base_Promotion 
   TABLE DATA               �  COPY "base_Promotion" ("Id", "Name", "Description", "PromotionTypeId", "TakeOffOption", "TakeOff", "BuyingQty", "GetingValue", "IsApplyToAboveQuantities", "Status", "AffectDiscount", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource", "CouponExpire", "IsCouponExpired", "PriceSchemaRange", "ReasonReActive", "Sold", "TotalPrice", "CategoryId", "VendorId", "CouponBarCode", "BarCodeNumber", "BarCodeImage") FROM stdin;
    public       postgres    false    1805   G-      �          0    245155    base_PromotionAffect 
   TABLE DATA               �   COPY "base_PromotionAffect" ("Id", "PromotionId", "ItemId", "Price1", "Price2", "Price3", "Price4", "Price5", "Discount1", "Discount2", "Discount3", "Discount4", "Discount5") FROM stdin;
    public       postgres    false    1803   �.      �          0    245023    base_PromotionSchedule 
   TABLE DATA               X   COPY "base_PromotionSchedule" ("Id", "PromotionId", "EndDate", "StartDate") FROM stdin;
    public       postgres    false    1791   �.      �          0    266551    base_PurchaseOrder 
   TABLE DATA               _  COPY "base_PurchaseOrder" ("Id", "PurchaseOrderNo", "VendorCode", "Status", "ShipAddress", "PurchasedDate", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "PaymentDueDate", "PaymentMethodId", "Remark", "ShipDate", "SubTotal", "DiscountPercent", "DiscountAmount", "Freight", "Fee", "Total", "Paid", "Balance", "ItemCount", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "DateUpdate", "UserUpdated", "Resource", "CancelDate", "IsFullWorkflow", "StoreCode", "RecRemark", "PaymentName", "IsPurge", "IsLocked", "VendorResource") FROM stdin;
    public       postgres    false    1855   =/      �          0    266530    base_PurchaseOrderDetail 
   TABLE DATA               ,  COPY "base_PurchaseOrderDetail" ("Id", "PurchaseOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "ReceivedQty", "DueQty", "UnFilledQty", "Amount", "Serial", "LastReceived", "Resource", "IsFullReceived", "Discount") FROM stdin;
    public       postgres    false    1853   �2      �          0    267535    base_PurchaseOrderReceive 
   TABLE DATA               �   COPY "base_PurchaseOrderReceive" ("Id", "PurchaseOrderDetailId", "POResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "RecQty", "IsReceived", "ReceiveDate", "Resource", "Price") FROM stdin;
    public       postgres    false    1861   �5      �          0    282642    base_QuantityAdjustment 
   TABLE DATA               �   COPY "base_QuantityAdjustment" ("Id", "ProductId", "ProductResource", "CostDifference", "OldQty", "NewQty", "AdjustmentQtyDiff", "LoggedTime", "Reason", "Status", "UserCreated", "IsReversed", "StoreCode") FROM stdin;
    public       postgres    false    1895   8      �          0    256178    base_ResourceAccount 
   TABLE DATA               �   COPY "base_ResourceAccount" ("Id", "Resource", "UserResource", "LoginName", "Password", "ExpiredDate", "IsLocked", "IsExpired") FROM stdin;
    public       postgres    false    1836   <9      �          0    246083    base_ResourceNote 
   TABLE DATA               X   COPY "base_ResourceNote" ("Id", "Note", "DateCreated", "Color", "Resource") FROM stdin;
    public       postgres    false    1813   ;      �          0    270150    base_ResourcePayment 
   TABLE DATA               (  COPY "base_ResourcePayment" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalPaid", "Balance", "Change", "DateCreated", "UserCreated", "Remark", "Resource", "SubTotal", "DiscountPercent", "DiscountAmount", "Mark", "IsDeposit", "TaxCode", "TaxAmount", "LastRewardAmount") FROM stdin;
    public       postgres    false    1877   ->      �          0    270072    base_ResourcePaymentDetail 
   TABLE DATA               �   COPY "base_ResourcePaymentDetail" ("Id", "PaymentType", "ResourcePaymentId", "PaymentMethodId", "PaymentMethod", "CardType", "Paid", "Change", "Tip", "GiftCardNo", "Reason", "Reference") FROM stdin;
    public       postgres    false    1875   v_      �          0    272122    base_ResourcePaymentProduct 
   TABLE DATA               �   COPY "base_ResourcePaymentProduct" ("Id", "ResourcePaymentId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "Amount") FROM stdin;
    public       postgres    false    1889   �c      ~          0    244922    base_ResourcePhoto 
   TABLE DATA               �   COPY "base_ResourcePhoto" ("Id", "ThumbnailPhoto", "ThumbnailPhotoFilename", "LargePhoto", "LargePhotoFilename", "SortId", "Resource") FROM stdin;
    public       postgres    false    1783   �e      �          0    270193    base_ResourceReturn 
   TABLE DATA                 COPY "base_ResourceReturn" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalRefund", "Balance", "DateCreated", "UserCreated", "Resource", "Mark", "DiscountPercent", "DiscountAmount", "Freight", "SubTotal", "ReturnFee", "ReturnFeePercent") FROM stdin;
    public       postgres    false    1879   �l      �          0    272099    base_ResourceReturnDetail 
   TABLE DATA               �   COPY "base_ResourceReturnDetail" ("Id", "ResourceReturnId", "OrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Price", "ReturnQty", "Amount", "IsReturned", "ReturnedDate", "Discount") FROM stdin;
    public       postgres    false    1887   E�      �          0    266843    base_RewardManager 
   TABLE DATA               �  COPY "base_RewardManager" ("Id", "StoreCode", "PurchaseThreshold", "RewardAmount", "RewardAmtType", "RewardExpiration", "IsAutoEnroll", "IsPromptEnroll", "IsInformCashier", "IsRedemptionLimit", "RedemptionLimitAmount", "IsBlockRedemption", "RedemptionAfterDays", "IsBlockPurchaseRedeem", "IsTrackingPeriod", "StartDate", "EndDate", "IsNoEndDay", "TotalRewardRedeemed", "IsActived", "ReasonReActive", "DateCreated") FROM stdin;
    public       postgres    false    1859   �      �          0    266606    base_SaleCommission 
   TABLE DATA               �   COPY "base_SaleCommission" ("Id", "GuestResource", "SOResource", "SONumber", "SOTotal", "SODate", "ComissionPercent", "CommissionAmount", "Sign", "Remark") FROM stdin;
    public       postgres    false    1857   p�      �          0    266093    base_SaleOrder 
   TABLE DATA               n  COPY "base_SaleOrder" ("Id", "SONumber", "OrderDate", "OrderStatus", "BillAddressId", "BillAddress", "ShipAddressId", "ShipAddress", "PromotionCode", "SaleRep", "CustomerResource", "PriceSchemaId", "DueDate", "RequestShipDate", "SubTotal", "TaxLocation", "TaxCode", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "Paid", "Balance", "RefundedAmount", "IsMultiPayment", "Remark", "IsFullWorkflow", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "Resource", "BookingChanel", "ShippedCount", "Deposit", "Transaction", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "IsTaxExemption", "TaxExemption", "ShippedBox", "PackedQty", "TotalWeight", "WeightUnit", "StoreCode", "IsRedeeem", "IsPurge", "IsLocked", "RewardAmount", "Cashier") FROM stdin;
    public       postgres    false    1845   �      �          0    266084    base_SaleOrderDetail 
   TABLE DATA               �  COPY "base_SaleOrderDetail" ("Id", "SaleOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "TaxCode", "Quantity", "PickQty", "DueQty", "UnFilled", "RegularPrice", "SalePrice", "UOMId", "BaseUOM", "DiscountPercent", "DiscountAmount", "SubTotal", "OnHandQty", "SerialTracking", "Resource", "BalanceShipped", "Comment", "TotalDiscount", "PromotionId", "IsManual") FROM stdin;
    public       postgres    false    1843   �      �          0    266236    base_SaleOrderInvoice 
   TABLE DATA               �   COPY "base_SaleOrderInvoice" ("Id", "InvoiceNo", "SaleOrderId", "SaleOrderResource", "ItemCount", "SubTotal", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "DateCreated") FROM stdin;
    public       postgres    false    1849   ��      �          0    266180    base_SaleOrderShip 
   TABLE DATA               �   COPY "base_SaleOrderShip" ("Id", "SaleOrderId", "SaleOrderResource", "Weight", "TrackingNo", "IsShipped", "Resource", "Remark", "Carrier", "ShipDate", "BoxNo") FROM stdin;
    public       postgres    false    1847   ��      �          0    266357    base_SaleOrderShipDetail 
   TABLE DATA               �   COPY "base_SaleOrderShipDetail" ("Id", "SaleOrderShipId", "SaleOrderShipResource", "SaleOrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Description", "SerialTracking", "PackedQty", "IsPaid") FROM stdin;
    public       postgres    false    1851   0�      �          0    245103    base_SaleTaxLocation 
   TABLE DATA               �   COPY "base_SaleTaxLocation" ("Id", "ParentId", "Name", "IsShipingTaxable", "ShippingTaxCodeId", "IsActived", "LevelId", "TaxCode", "TaxCodeName", "TaxPrintMark", "TaxOption", "IsPrimary", "SortIndex", "IsTaxAfterDiscount") FROM stdin;
    public       postgres    false    1795   a�      �          0    245084    base_SaleTaxLocationOption 
   TABLE DATA               �   COPY "base_SaleTaxLocationOption" ("Id", "SaleTaxLocationId", "ParentId", "TaxRate", "TaxComponent", "TaxAgency", "TaxCondition", "IsApplyAmountOver", "IsAllowSpecificItemPriceRange", "IsAllowAmountItemPriceRange", "PriceFrom", "PriceTo") FROM stdin;
    public       postgres    false    1793   Y�      �          0    255675 
   base_Store 
   TABLE DATA               G   COPY "base_Store" ("Id", "Code", "Name", "Street", "City") FROM stdin;
    public       postgres    false    1818   ��      �          0    269925    base_TransferStock 
   TABLE DATA                 COPY "base_TransferStock" ("Id", "TransferNo", "FromStore", "ToStore", "TotalQuantity", "ShipDate", "Carier", "ShippingFee", "Comment", "Resource", "UserCreated", "DateCreated", "Status", "SubTotal", "Total", "DateApplied", "UserApplied", "DateReversed", "UserReversed") FROM stdin;
    public       postgres    false    1871   =�      �          0    269941    base_TransferStockDetail 
   TABLE DATA               �   COPY "base_TransferStockDetail" ("Id", "TransferStockId", "TransferStockResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Quantity", "UOMId", "BaseUOM", "Amount", "SerialTracking", "AvlQuantity") FROM stdin;
    public       postgres    false    1873   �      �          0    245147    base_UOM 
   TABLE DATA               �   COPY "base_UOM" ("Id", "Code", "Name", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsActived", "Resource") FROM stdin;
    public       postgres    false    1801   ��      �          0    245131    base_UserLog 
   TABLE DATA               y   COPY "base_UserLog" ("Id", "IpSource", "ConnectedOn", "DisConnectedOn", "ResourceAccessed", "IsDisconected") FROM stdin;
    public       postgres    false    1799   �      y          0    244282    base_UserLogDetail 
   TABLE DATA               m   COPY "base_UserLogDetail" ("Id", "UserLogId", "AccessedTime", "ModuleName", "ActionDescription") FROM stdin;
    public       postgres    false    1773   ��      �          0    256244    base_UserRight 
   TABLE DATA               9   COPY "base_UserRight" ("Id", "Code", "Name") FROM stdin;
    public       postgres    false    1838   VO      �          0    269643    base_VendorProduct 
   TABLE DATA               t   COPY "base_VendorProduct" ("Id", "ProductId", "VendorId", "Price", "ProductResource", "VendorResource") FROM stdin;
    public       postgres    false    1868   -X      �          0    245115    base_VirtualFolder 
   TABLE DATA               �   COPY "base_VirtualFolder" ("Id", "ParentFolderId", "FolderName", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource") FROM stdin;
    public       postgres    false    1797   JX      �          0    282433 	   rpt_Group 
   TABLE DATA               R   COPY "rpt_Group" ("Id", "Code", "Name", "DateCreated", "UserCreated") FROM stdin;
    public       postgres    false    1890   �X      �          0    283470 
   rpt_Report 
   TABLE DATA               0  COPY "rpt_Report" ("Id", "GroupId", "ParentId", "Code", "Name", "FormatFile", "IsShow", "ProcessName", "SamplePicture", "PrintTimes", "LastPrintDate", "LastPrintUser", "ExcelFile", "PrinterName", "PrintCopy", "PaperSize", "Remark", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1899   �X      �          0    255696    tims_Holiday 
   TABLE DATA               �   COPY "tims_Holiday" ("Id", "Title", "Description", "HolidayOption", "FromDate", "ToDate", "Month", "Day", "DayOfWeek", "WeekOfMonth", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedByID") FROM stdin;
    public       postgres    false    1820   �X      �          0    255705    tims_HolidayHistory 
   TABLE DATA               8   COPY "tims_HolidayHistory" ("Date", "Name") FROM stdin;
    public       postgres    false    1821   �Z      �          0    255849    tims_TimeLog 
   TABLE DATA               �  COPY "tims_TimeLog" ("Id", "EmployeeId", "WorkScheduleId", "PayrollId", "ClockIn", "ClockOut", "ManualClockInFlag", "ManualClockOutFlag", "WorkTime", "LunchTime", "OvertimeBefore", "Reason", "DeductLunchTimeFlag", "LateTime", "LeaveEarlyTime", "ActiveFlag", "ModifiedDate", "ModifiedById", "OvertimeAfter", "OvertimeLunch", "OvertimeDayOff", "OvertimeOptions", "GuestResource") FROM stdin;
    public       postgres    false    1829   ^[      �          0    255865    tims_TimeLogPermission 
   TABLE DATA               L   COPY "tims_TimeLogPermission" ("TimeLogId", "WorkPermissionId") FROM stdin;
    public       postgres    false    1831   �\      �          0    255795    tims_WorkPermission 
   TABLE DATA               �   COPY "tims_WorkPermission" ("Id", "EmployeeId", "PermissionType", "FromDate", "ToDate", "Note", "NoOfDays", "HourPerDay", "PaidFlag", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById", "OvertimeOptions") FROM stdin;
    public       postgres    false    1827   �\      �          0    255738    tims_WorkSchedule 
   TABLE DATA               �   COPY "tims_WorkSchedule" ("Id", "WorkScheduleName", "WorkScheduleType", "Rotate", "Status", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById") FROM stdin;
    public       postgres    false    1823   �]      �          0    255781    tims_WorkWeek 
   TABLE DATA               �   COPY "tims_WorkWeek" ("Id", "WorkScheduleId", "Week", "Day", "WorkIn", "WorkOut", "LunchOut", "LunchIn", "LunchBreakFlag") FROM stdin;
    public       postgres    false    1825   �^      A
           2606    235700    pga_exception_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_pkey PRIMARY KEY (jexid);
 K   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_pkey;
       pgagent         postgres    false    1754    1754            C
           2606    235702    pga_job_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_pkey PRIMARY KEY (jobid);
 ?   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_pkey;
       pgagent         postgres    false    1756    1756            E
           2606    235704    pga_jobagent_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY pga_jobagent
    ADD CONSTRAINT pga_jobagent_pkey PRIMARY KEY (jagpid);
 I   ALTER TABLE ONLY pgagent.pga_jobagent DROP CONSTRAINT pga_jobagent_pkey;
       pgagent         postgres    false    1758    1758            H
           2606    235706    pga_jobclass_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_jobclass
    ADD CONSTRAINT pga_jobclass_pkey PRIMARY KEY (jclid);
 I   ALTER TABLE ONLY pgagent.pga_jobclass DROP CONSTRAINT pga_jobclass_pkey;
       pgagent         postgres    false    1759    1759            K
           2606    235708    pga_joblog_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_pkey PRIMARY KEY (jlgid);
 E   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_pkey;
       pgagent         postgres    false    1761    1761            N
           2606    235710    pga_jobstep_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_pkey PRIMARY KEY (jstid);
 G   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_pkey;
       pgagent         postgres    false    1763    1763            Q
           2606    235712    pga_jobsteplog_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_pkey PRIMARY KEY (jslid);
 M   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_pkey;
       pgagent         postgres    false    1765    1765            T
           2606    235714    pga_schedule_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_pkey PRIMARY KEY (jscid);
 I   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_pkey;
       pgagent         postgres    false    1767    1767            �
           2606    245348    FK_base_Department_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_Id";
       public         postgres    false    1807    1807            �
           2606    256188    FPK_base_ResourceAccount_Id 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "FPK_base_ResourceAccount_Id" PRIMARY KEY ("Id", "Resource");
 ^   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "FPK_base_ResourceAccount_Id";
       public         postgres    false    1836    1836    1836            �
           2606    245266    PF_base_SaleTaxLocation 
   CONSTRAINT     i   ALTER TABLE ONLY "base_SaleTaxLocation"
    ADD CONSTRAINT "PF_base_SaleTaxLocation" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_SaleTaxLocation" DROP CONSTRAINT "PF_base_SaleTaxLocation";
       public         postgres    false    1795    1795            &           2606    282455    PF_rpt_Group_Id 
   CONSTRAINT     V   ALTER TABLE ONLY "rpt_Group"
    ADD CONSTRAINT "PF_rpt_Group_Id" PRIMARY KEY ("Id");
 G   ALTER TABLE ONLY public."rpt_Group" DROP CONSTRAINT "PF_rpt_Group_Id";
       public         postgres    false    1890    1890            �
           2606    255762    PF_tims_Holiday_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "tims_Holiday"
    ADD CONSTRAINT "PF_tims_Holiday_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."tims_Holiday" DROP CONSTRAINT "PF_tims_Holiday_Id";
       public         postgres    false    1820    1820            �
           2606    245385    PK_GuestAdditional_Id 
   CONSTRAINT     g   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "PK_GuestAdditional_Id" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "PK_GuestAdditional_Id";
       public         postgres    false    1809    1809            _
           2606    244286    PK_UserLogDetail_Id 
   CONSTRAINT     c   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "PK_UserLogDetail_Id" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "PK_UserLogDetail_Id";
       public         postgres    false    1773    1773            �
           2606    245136    PK_UserLog_Id 
   CONSTRAINT     W   ALTER TABLE ONLY "base_UserLog"
    ADD CONSTRAINT "PK_UserLog_Id" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_UserLog" DROP CONSTRAINT "PK_UserLog_Id";
       public         postgres    false    1799    1799            y
           2606    244954    PK_base_Attachment_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "PK_base_Attachment_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "PK_base_Attachment_Id";
       public         postgres    false    1787    1787            �
           2606    256191    PK_base_Authorize_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Authorize"
    ADD CONSTRAINT "PK_base_Authorize_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Authorize" DROP CONSTRAINT "PK_base_Authorize_Id";
       public         postgres    false    1834    1834            /           2606    283493    PK_base_CostAdjustment_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "PK_base_CostAdjustment_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "PK_base_CostAdjustment_Id";
       public         postgres    false    1897    1897                       2606    271757    PK_base_CounStockDetail_Id 
   CONSTRAINT     m   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "PK_base_CounStockDetail_Id" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "PK_base_CounStockDetail_Id";
       public         postgres    false    1885    1885                       2606    271755    PK_base_CounStock_Id 
   CONSTRAINT     a   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "PK_base_CounStock_Id" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "PK_base_CounStock_Id";
       public         postgres    false    1883    1883            V
           2606    238143    PK_base_EmailAttachment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "PK_base_EmailAttachment" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "PK_base_EmailAttachment";
       public         postgres    false    1769    1769            X
           2606    238253    PK_base_Email_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Email"
    ADD CONSTRAINT "PK_base_Email_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Email" DROP CONSTRAINT "PK_base_Email_Id";
       public         postgres    false    1770    1770            \
           2606    238418    PK_base_GuestFingerPrint_Id 
   CONSTRAINT     n   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "PK_base_GuestFingerPrint_Id" PRIMARY KEY ("Id");
 _   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "PK_base_GuestFingerPrint_Id";
       public         postgres    false    1772    1772            l
           2606    244879    PK_base_GuestHiringHistory_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "PK_base_GuestHiringHistory_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "PK_base_GuestHiringHistory_Id";
       public         postgres    false    1779    1779            q
           2606    244890    PK_base_GuestPayRoll_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "PK_base_GuestPayRoll_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "PK_base_GuestPayRoll_Id";
       public         postgres    false    1781    1781            v
           2606    244941    PK_base_GuestProfile_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "PK_base_GuestProfile_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "PK_base_GuestProfile_Id";
       public         postgres    false    1785    1785            �
           2606    268362    PK_base_GuestReward_Id 
   CONSTRAINT     d   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "PK_base_GuestReward_Id" PRIMARY KEY ("Id");
 U   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "PK_base_GuestReward_Id";
       public         postgres    false    1865    1865            �
           2606    256030    PK_base_GuestSchedule 
   CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "PK_base_GuestSchedule" PRIMARY KEY ("GuestId", "WorkScheduleId", "StartDate");
 V   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "PK_base_GuestSchedule";
       public         postgres    false    1832    1832    1832    1832            i
           2606    244869    PK_base_Guest_Id 
   CONSTRAINT     _   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "PK_base_Guest_Id" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "PK_base_Guest_Id";
       public         postgres    false    1777    1777            (           2606    282558    PK_base_Language 
   CONSTRAINT     [   ALTER TABLE ONLY "base_Language"
    ADD CONSTRAINT "PK_base_Language" PRIMARY KEY ("Id");
 L   ALTER TABLE ONLY public."base_Language" DROP CONSTRAINT "PK_base_Language";
       public         postgres    false    1893    1893            }
           2606    245005    PK_base_MemberShip 
   CONSTRAINT     _   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "PK_base_MemberShip" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "PK_base_MemberShip";
       public         postgres    false    1789    1789            �
           2606    268520    PK_base_PricingChange_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "PK_base_PricingChange_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "PK_base_PricingChange_Id";
       public         postgres    false    1867    1867            �
           2606    268194    PK_base_PricingManager_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "PK_base_PricingManager_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "PK_base_PricingManager_Id";
       public         postgres    false    1863    1863            �
           2606    255541    PK_base_ProductStore_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "PK_base_ProductStore_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "PK_base_ProductStore_Id";
       public         postgres    false    1816    1816                       2606    270271    PK_base_ProductUOM_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "PK_base_ProductUOM_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "PK_base_ProductUOM_Id";
       public         postgres    false    1881    1881            �
           2606    255615    PK_base_Product_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "PK_base_Product_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "PK_base_Product_Id";
       public         postgres    false    1811    1811            �
           2606    245160    PK_base_PromotionAffect_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "PK_base_PromotionAffect_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "PK_base_PromotionAffect_Id";
       public         postgres    false    1803    1803            �
           2606    245030    PK_base_PromotionSchedule_Id 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "PK_base_PromotionSchedule_Id" PRIMARY KEY ("Id");
 a   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "PK_base_PromotionSchedule_Id";
       public         postgres    false    1791    1791            �
           2606    245177    PK_base_Promotion_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Promotion"
    ADD CONSTRAINT "PK_base_Promotion_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Promotion" DROP CONSTRAINT "PK_base_Promotion_Id";
       public         postgres    false    1805    1805            �
           2606    266538    PK_base_PurchaseOrderItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "PK_base_PurchaseOrderItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "PK_base_PurchaseOrderItem_Id";
       public         postgres    false    1853    1853            �
           2606    267544    PK_base_PurchaseOrderReceive_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "PK_base_PurchaseOrderReceive_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "PK_base_PurchaseOrderReceive_Id";
       public         postgres    false    1861    1861            �
           2606    266567    PK_base_PurchaseOrder_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "PK_base_PurchaseOrder_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "PK_base_PurchaseOrder_Id";
       public         postgres    false    1855    1855            ,           2606    283506    PK_base_QuantityAdjustment_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "PK_base_QuantityAdjustment_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "PK_base_QuantityAdjustment_Id";
       public         postgres    false    1895    1895            �
           2606    246089    PK_base_ResourceNote_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ResourceNote"
    ADD CONSTRAINT "PK_base_ResourceNote_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ResourceNote" DROP CONSTRAINT "PK_base_ResourceNote_Id";
       public         postgres    false    1813    1813                       2606    270163     PK_base_ResourcePaymentDetail_Id 
   CONSTRAINT     x   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "PK_base_ResourcePaymentDetail_Id" PRIMARY KEY ("Id");
 i   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "PK_base_ResourcePaymentDetail_Id";
       public         postgres    false    1875    1875            $           2606    272130     PK_base_ResourcePaymentProductId 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "PK_base_ResourcePaymentProductId" PRIMARY KEY ("Id");
 j   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "PK_base_ResourcePaymentProductId";
       public         postgres    false    1889    1889                       2606    270161    PK_base_ResourcePayment_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_ResourcePayment"
    ADD CONSTRAINT "PK_base_ResourcePayment_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_ResourcePayment" DROP CONSTRAINT "PK_base_ResourcePayment_Id";
       public         postgres    false    1877    1877            s
           2606    270190    PK_base_ResourcePhoto_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_ResourcePhoto"
    ADD CONSTRAINT "PK_base_ResourcePhoto_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_ResourcePhoto" DROP CONSTRAINT "PK_base_ResourcePhoto_Id";
       public         postgres    false    1783    1783            !           2606    272108    PK_base_ResourceReturnDetail_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "PK_base_ResourceReturnDetail_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "PK_base_ResourceReturnDetail_Id";
       public         postgres    false    1887    1887                       2606    270203    PK_base_ResourceReturn_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "PK_base_ResourceReturn_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "PK_base_ResourceReturn_Id";
       public         postgres    false    1879    1879            �
           2606    266851    PK_base_RewardManager 
   CONSTRAINT     e   ALTER TABLE ONLY "base_RewardManager"
    ADD CONSTRAINT "PK_base_RewardManager" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_RewardManager" DROP CONSTRAINT "PK_base_RewardManager";
       public         postgres    false    1859    1859            �
           2606    266611    PK_base_SaleCommission 
   CONSTRAINT     g   ALTER TABLE ONLY "base_SaleCommission"
    ADD CONSTRAINT "PK_base_SaleCommission" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_SaleCommission" DROP CONSTRAINT "PK_base_SaleCommission";
       public         postgres    false    1857    1857            �
           2606    266090    PK_base_SaleOrderDetail_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "PK_base_SaleOrderDetail_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "PK_base_SaleOrderDetail_Id";
       public         postgres    false    1843    1843            �
           2606    266249    PK_base_SaleOrderInvoice 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "PK_base_SaleOrderInvoice" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "PK_base_SaleOrderInvoice";
       public         postgres    false    1849    1849            �
           2606    266362    PK_base_SaleOrderShipDetail 
   CONSTRAINT     q   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "PK_base_SaleOrderShipDetail" PRIMARY KEY ("Id");
 b   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "PK_base_SaleOrderShipDetail";
       public         postgres    false    1851    1851            �
           2606    266219    PK_base_SaleOrderShip_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "PK_base_SaleOrderShip_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "PK_base_SaleOrderShip_Id";
       public         postgres    false    1847    1847            �
           2606    266117    PK_base_SaleOrder_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_SaleOrder"
    ADD CONSTRAINT "PK_base_SaleOrder_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_SaleOrder" DROP CONSTRAINT "PK_base_SaleOrder_Id";
       public         postgres    false    1845    1845            �
           2606    245268    PK_base_SaleTaxLocationOption 
   CONSTRAINT     u   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "PK_base_SaleTaxLocationOption" PRIMARY KEY ("Id");
 f   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "PK_base_SaleTaxLocationOption";
       public         postgres    false    1793    1793            �
           2606    255680    PK_base_Store_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "PK_base_Store_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "PK_base_Store_Id";
       public         postgres    false    1818    1818            
           2606    269949    PK_base_TransferStockDetail_Id 
   CONSTRAINT     t   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "PK_base_TransferStockDetail_Id" PRIMARY KEY ("Id");
 e   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "PK_base_TransferStockDetail_Id";
       public         postgres    false    1873    1873                       2606    269936    PK_base_TransferStock_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "PK_base_TransferStock_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "PK_base_TransferStock_Id";
       public         postgres    false    1871    1871            �
           2606    245152    PK_base_UOM_Id 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "PK_base_UOM_Id" PRIMARY KEY ("Id");
 E   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "PK_base_UOM_Id";
       public         postgres    false    1801    1801            �
           2606    256249    PK_base_UserRight_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "PK_base_UserRight_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "PK_base_UserRight_Id";
       public         postgres    false    1838    1838                       2606    269660    PK_base_VendorProduct_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "PK_base_VendorProduct_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "PK_base_VendorProduct_Id";
       public         postgres    false    1868    1868            �
           2606    245122    PK_base_VirtualFolder 
   CONSTRAINT     e   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "PK_base_VirtualFolder" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "PK_base_VirtualFolder";
       public         postgres    false    1797    1797            2           2606    283482    PK_rpt_Report_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "PK_rpt_Report_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "PK_rpt_Report_Id";
       public         postgres    false    1899    1899            �
           2606    255709    PK_tims_HolidayHistory_Date 
   CONSTRAINT     n   ALTER TABLE ONLY "tims_HolidayHistory"
    ADD CONSTRAINT "PK_tims_HolidayHistory_Date" PRIMARY KEY ("Date");
 ]   ALTER TABLE ONLY public."tims_HolidayHistory" DROP CONSTRAINT "PK_tims_HolidayHistory_Date";
       public         postgres    false    1821    1821            �
           2606    255743    PK_tims_WorkSchedule_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "tims_WorkSchedule"
    ADD CONSTRAINT "PK_tims_WorkSchedule_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."tims_WorkSchedule" DROP CONSTRAINT "PK_tims_WorkSchedule_Id";
       public         postgres    false    1823    1823            �
           2606    255786    PK_tims_WorkWeek_Id 
   CONSTRAINT     ^   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "PK_tims_WorkWeek_Id" PRIMARY KEY ("Id");
 O   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "PK_tims_WorkWeek_Id";
       public         postgres    false    1825    1825            �
           2606    257312    base_Configuration_pkey 
   CONSTRAINT     g   ALTER TABLE ONLY "base_Configuration"
    ADD CONSTRAINT "base_Configuration_pkey" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_Configuration" DROP CONSTRAINT "base_Configuration_pkey";
       public         postgres    false    1814    1814            �
           2606    257332    base_GuestPaymentCard_Id 
   CONSTRAINT     k   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "base_GuestPaymentCard_Id" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "base_GuestPaymentCard_Id";
       public         postgres    false    1841    1841            c
           2606    244846    base_Guest_pkey 
   CONSTRAINT     W   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "base_Guest_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "base_Guest_pkey";
       public         postgres    false    1775    1775            �
           2606    255870    key 
   CONSTRAINT     p   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT key PRIMARY KEY ("TimeLogId", "WorkPermissionId");
 F   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT key;
       public         postgres    false    1831    1831    1831            �
           2606    255857    pk_tims_timelog 
   CONSTRAINT     W   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT pk_tims_timelog PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT pk_tims_timelog;
       public         postgres    false    1829    1829            �
           2606    255803    pk_tims_workpermission 
   CONSTRAINT     e   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT pk_tims_workpermission PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT pk_tims_workpermission;
       public         postgres    false    1827    1827                       2606    271770    uni_base_CountStock_Resource 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "uni_base_CountStock_Resource" UNIQUE ("Resource");
 Z   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "uni_base_CountStock_Resource";
       public         postgres    false    1883    1883            g
           2606    256327    uni_base_Guest_Resource 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "uni_base_Guest_Resource" UNIQUE ("Resource");
 P   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "uni_base_Guest_Resource";
       public         postgres    false    1775    1775            *           2606    282560    uni_base_Language 
   CONSTRAINT     Y   ALTER TABLE ONLY "base_Language"
    ADD CONSTRAINT "uni_base_Language" UNIQUE ("Code");
 M   ALTER TABLE ONLY public."base_Language" DROP CONSTRAINT "uni_base_Language";
       public         postgres    false    1893    1893            �
           2606    268201    uni_base_PricingManager 
   CONSTRAINT     i   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "uni_base_PricingManager" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "uni_base_PricingManager";
       public         postgres    false    1863    1863            �
           2606    269972    uni_base_Product_Resource 
   CONSTRAINT     d   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "uni_base_Product_Resource" UNIQUE ("Resource");
 T   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "uni_base_Product_Resource";
       public         postgres    false    1811    1811            �
           2606    266569    uni_base_PurchaseOrder_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "uni_base_PurchaseOrder_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "uni_base_PurchaseOrder_Resource";
       public         postgres    false    1855    1855            �
           2606    256317 !   uni_base_ResourceAccount_Resource 
   CONSTRAINT     t   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "uni_base_ResourceAccount_Resource" UNIQUE ("Resource");
 d   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "uni_base_ResourceAccount_Resource";
       public         postgres    false    1836    1836                       2606    270205     uni_base_ResourceReturn_Resource 
   CONSTRAINT     r   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "uni_base_ResourceReturn_Resource" UNIQUE ("Resource");
 b   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "uni_base_ResourceReturn_Resource";
       public         postgres    false    1879    1879            �
           2606    266303    uni_base_SaleOrderDetail 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "uni_base_SaleOrderDetail" UNIQUE ("Resource");
 [   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "uni_base_SaleOrderDetail";
       public         postgres    false    1843    1843            �
           2606    266221    uni_base_SaleOrderShip_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "uni_base_SaleOrderShip_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "uni_base_SaleOrderShip_Resource";
       public         postgres    false    1847    1847            �
           2606    255948    uni_base_Store_Code 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "uni_base_Store_Code" UNIQUE ("Code");
 L   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "uni_base_Store_Code";
       public         postgres    false    1818    1818                       2606    269938    uni_base_TransferStock_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "uni_base_TransferStock_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "uni_base_TransferStock_Resource";
       public         postgres    false    1871    1871            �
           2606    254600    uni_base_UOM_Code 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "uni_base_UOM_Code" UNIQUE ("Code");
 H   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "uni_base_UOM_Code";
       public         postgres    false    1801    1801            �
           2606    256251    uni_base_UserRight_Code 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "uni_base_UserRight_Code" UNIQUE ("Code");
 T   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "uni_base_UserRight_Code";
       public         postgres    false    1838    1838                       2606    269675 5   uni_base_VendorProduct_VendorResource_ProductResource 
   CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource" UNIQUE ("ProductResource", "VendorResource");
 v   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource";
       public         postgres    false    1868    1868    1868            >
           1259    235939    pga_exception_datetime    INDEX     \   CREATE UNIQUE INDEX pga_exception_datetime ON pga_exception USING btree (jexdate, jextime);
 +   DROP INDEX pgagent.pga_exception_datetime;
       pgagent         postgres    false    1754    1754            ?
           1259    235940    pga_exception_jexscid    INDEX     K   CREATE INDEX pga_exception_jexscid ON pga_exception USING btree (jexscid);
 *   DROP INDEX pgagent.pga_exception_jexscid;
       pgagent         postgres    false    1754            F
           1259    235941    pga_jobclass_name    INDEX     M   CREATE UNIQUE INDEX pga_jobclass_name ON pga_jobclass USING btree (jclname);
 &   DROP INDEX pgagent.pga_jobclass_name;
       pgagent         postgres    false    1759            I
           1259    235942    pga_joblog_jobid    INDEX     D   CREATE INDEX pga_joblog_jobid ON pga_joblog USING btree (jlgjobid);
 %   DROP INDEX pgagent.pga_joblog_jobid;
       pgagent         postgres    false    1761            R
           1259    235943    pga_jobschedule_jobid    INDEX     K   CREATE INDEX pga_jobschedule_jobid ON pga_schedule USING btree (jscjobid);
 *   DROP INDEX pgagent.pga_jobschedule_jobid;
       pgagent         postgres    false    1767            L
           1259    235944    pga_jobstep_jobid    INDEX     F   CREATE INDEX pga_jobstep_jobid ON pga_jobstep USING btree (jstjobid);
 &   DROP INDEX pgagent.pga_jobstep_jobid;
       pgagent         postgres    false    1763            O
           1259    235945    pga_jobsteplog_jslid    INDEX     L   CREATE INDEX pga_jobsteplog_jslid ON pga_jobsteplog USING btree (jsljlgid);
 )   DROP INDEX pgagent.pga_jobsteplog_jslid;
       pgagent         postgres    false    1765            �
           1259    255547 .   FKI_baseProductStore_ProductId_base_Product_Id    INDEX     p   CREATE INDEX "FKI_baseProductStore_ProductId_base_Product_Id" ON "base_ProductStore" USING btree ("ProductId");
 D   DROP INDEX public."FKI_baseProductStore_ProductId_base_Product_Id";
       public         postgres    false    1816            �
           1259    245166 5   FKI_basePromotionAffect_PromotionId_base_Promotion_Id    INDEX     |   CREATE INDEX "FKI_basePromotionAffect_PromotionId_base_Promotion_Id" ON "base_PromotionAffect" USING btree ("PromotionId");
 K   DROP INDEX public."FKI_basePromotionAffect_PromotionId_base_Promotion_Id";
       public         postgres    false    1803            w
           1259    246209 9   FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    INDEX        CREATE INDEX "FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" ON "base_Attachment" USING btree ("VirtualFolderId");
 O   DROP INDEX public."FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public         postgres    false    1787            -           1259    283499 $   FKI_base_CostAdjustment_base_Product    INDEX     h   CREATE INDEX "FKI_base_CostAdjustment_base_Product" ON "base_CostAdjustment" USING btree ("ProductId");
 :   DROP INDEX public."FKI_base_CostAdjustment_base_Product";
       public         postgres    false    1897                       1259    271763 8   FKI_base_CounStockDetail_CountStockId_base_CountStock_id    INDEX     �   CREATE INDEX "FKI_base_CounStockDetail_CountStockId_base_CountStock_id" ON "base_CountStockDetail" USING btree ("CountStockId");
 N   DROP INDEX public."FKI_base_CounStockDetail_CountStockId_base_CountStock_id";
       public         postgres    false    1885            �
           1259    245354    FKI_base_Department_Id_ParentId    INDEX     ^   CREATE INDEX "FKI_base_Department_Id_ParentId" ON "base_Department" USING btree ("ParentId");
 5   DROP INDEX public."FKI_base_Department_Id_ParentId";
       public         postgres    false    1807            �
           1259    245391 &   FKI_base_GuestAdditional_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestAdditional_base_Guest_Id" ON "base_GuestAdditional" USING btree ("GuestId");
 <   DROP INDEX public."FKI_base_GuestAdditional_base_Guest_Id";
       public         postgres    false    1809            o
           1259    244891 +   FKI_base_GuestPayRoll_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestPayRoll_GuestId_base_Guest_Id" ON "base_GuestPayRoll" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestPayRoll_GuestId_base_Guest_Id";
       public         postgres    false    1781            t
           1259    244942 +   FKI_base_GuestProfile_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestProfile_GuestId_base_Guest_Id" ON "base_GuestProfile" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestProfile_GuestId_base_Guest_Id";
       public         postgres    false    1785            �
           1259    268373 *   FKI_base_GuestReward_GuestId_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestReward_GuestId_base_Guest_Id" ON "base_GuestReward" USING btree ("GuestId");
 @   DROP INDEX public."FKI_base_GuestReward_GuestId_base_Guest_Id";
       public         postgres    false    1865            a
           1259    245510 %   FKI_base_Guest_ParentId_base_Guest_Id    INDEX     _   CREATE INDEX "FKI_base_Guest_ParentId_base_Guest_Id" ON "base_Guest" USING btree ("ParentId");
 ;   DROP INDEX public."FKI_base_Guest_ParentId_base_Guest_Id";
       public         postgres    false    1775            {
           1259    245006 )   FKI_base_MemberShip_GuestId_base_Guest_Id    INDEX     g   CREATE INDEX "FKI_base_MemberShip_GuestId_base_Guest_Id" ON "base_MemberShip" USING btree ("GuestId");
 ?   DROP INDEX public."FKI_base_MemberShip_GuestId_base_Guest_Id";
       public         postgres    false    1789            �
           1259    268532 >   FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id    INDEX     �   CREATE INDEX "FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id" ON "base_PricingChange" USING btree ("PricingManagerId");
 T   DROP INDEX public."FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public         postgres    false    1867                       1259    270282 .   FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id    INDEX     s   CREATE INDEX "FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id" ON "base_ProductUOM" USING btree ("BaseUnitNumber");
 D   DROP INDEX public."FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id";
       public         postgres    false    1881            ~
           1259    245041 8   FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id    INDEX     �   CREATE INDEX "FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id" ON "base_PromotionSchedule" USING btree ("PromotionId");
 N   DROP INDEX public."FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public         postgres    false    1791            �
           1259    245178 8   FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id    INDEX     }   CREATE INDEX "FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id" ON "base_Promotion" USING btree ("PromotionTypeId");
 N   DROP INDEX public."FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id";
       public         postgres    false    1805            �
           1259    266544 ?   FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder" ON "base_PurchaseOrderDetail" USING btree ("PurchaseOrderId");
 U   DROP INDEX public."FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder";
       public         postgres    false    1853            �
           1259    267550 ?   FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha" ON "base_PurchaseOrderReceive" USING btree ("PurchaseOrderDetailId");
 U   DROP INDEX public."FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha";
       public         postgres    false    1861            "           1259    272136 ?   FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource    INDEX     �   CREATE INDEX "FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource" ON "base_ResourcePaymentProduct" USING btree ("ResourcePaymentId");
 U   DROP INDEX public."FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource";
       public         postgres    false    1889            �
           1259    266128 6   FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    INDEX     }   CREATE INDEX "FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderDetail" USING btree ("SaleOrderId");
 L   DROP INDEX public."FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1843            �
           1259    266265 7   FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    INDEX        CREATE INDEX "FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderInvoice" USING btree ("SaleOrderId");
 M   DROP INDEX public."FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1849            �
           1259    266368 ?   FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip    INDEX     �   CREATE INDEX "FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip" ON "base_SaleOrderShipDetail" USING btree ("SaleOrderShipId");
 U   DROP INDEX public."FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip";
       public         postgres    false    1851            �
           1259    266227 4   FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    INDEX     y   CREATE INDEX "FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderShip" USING btree ("SaleOrderId");
 J   DROP INDEX public."FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1847            �
           1259    245099 1   FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id    INDEX     �   CREATE INDEX "FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id" ON "base_SaleTaxLocationOption" USING btree ("SaleTaxLocationId");
 G   DROP INDEX public."FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id";
       public         postgres    false    1793                       1259    269955 ?   FKI_base_TransferStockDetail_TransferStockId_base_TransferStock    INDEX     �   CREATE INDEX "FKI_base_TransferStockDetail_TransferStockId_base_TransferStock" ON "base_TransferStockDetail" USING btree ("TransferStockId");
 U   DROP INDEX public."FKI_base_TransferStockDetail_TransferStockId_base_TransferStock";
       public         postgres    false    1873            �
           1259    269666 .   FKI_base_VendorProduct_ProductId_base_Guest_Id    INDEX     q   CREATE INDEX "FKI_base_VendorProduct_ProductId_base_Guest_Id" ON "base_VendorProduct" USING btree ("ProductId");
 D   DROP INDEX public."FKI_base_VendorProduct_ProductId_base_Guest_Id";
       public         postgres    false    1868            0           1259    283488 #   FKI_rpt_Report_GroupId_rpt_Group_Id    INDEX     \   CREATE INDEX "FKI_rpt_Report_GroupId_rpt_Group_Id" ON "rpt_Report" USING btree ("GroupId");
 9   DROP INDEX public."FKI_rpt_Report_GroupId_rpt_Group_Id";
       public         postgres    false    1899            �
           1259    256148 0   FKI_tims_WorkPermission_EmployeeId_base_Guest_Id    INDEX     u   CREATE INDEX "FKI_tims_WorkPermission_EmployeeId_base_Guest_Id" ON "tims_WorkPermission" USING btree ("EmployeeId");
 F   DROP INDEX public."FKI_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public         postgres    false    1827            ]
           1259    244035    idx_GuestFingerPrint_GuestId    INDEX     `   CREATE INDEX "idx_GuestFingerPrint_GuestId" ON "base_GuestFingerPrint" USING btree ("GuestId");
 2   DROP INDEX public."idx_GuestFingerPrint_GuestId";
       public         postgres    false    1772            d
           1259    244839    idx_GuestName    INDEX     _   CREATE INDEX "idx_GuestName" ON "base_Guest" USING btree ("FirstName", "LastName", "Company");
 #   DROP INDEX public."idx_GuestName";
       public         postgres    false    1775    1775    1775            `
           1259    244292    idx_UserLogDetail    INDEX     T   CREATE INDEX "idx_UserLogDetail" ON "base_UserLogDetail" USING btree ("UserLogId");
 '   DROP INDEX public."idx_UserLogDetail";
       public         postgres    false    1773            z
           1259    255513    idx_base_Attachment    INDEX     S   CREATE UNIQUE INDEX "idx_base_Attachment" ON "base_Attachment" USING btree ("Id");
 )   DROP INDEX public."idx_base_Attachment";
       public         postgres    false    1787            �
           1259    256319    idx_base_Authorize_Code    INDEX     Q   CREATE INDEX "idx_base_Authorize_Code" ON "base_Authorize" USING btree ("Code");
 -   DROP INDEX public."idx_base_Authorize_Code";
       public         postgres    false    1834            �
           1259    256318    idx_base_Authorize_Resource    INDEX     Y   CREATE INDEX "idx_base_Authorize_Resource" ON "base_Authorize" USING btree ("Resource");
 1   DROP INDEX public."idx_base_Authorize_Resource";
       public         postgres    false    1834            �
           1259    255517    idx_base_Department_Id    INDEX     W   CREATE INDEX "idx_base_Department_Id" ON "base_Department" USING btree ("Id", "Name");
 ,   DROP INDEX public."idx_base_Department_Id";
       public         postgres    false    1807    1807            Y
           1259    238254    idx_base_Email    INDEX     B   CREATE INDEX "idx_base_Email" ON "base_Email" USING btree ("Id");
 $   DROP INDEX public."idx_base_Email";
       public         postgres    false    1770            Z
           1259    238260    idx_base_Email_Address    INDEX     N   CREATE INDEX "idx_base_Email_Address" ON "base_Email" USING btree ("Sender");
 ,   DROP INDEX public."idx_base_Email_Address";
       public         postgres    false    1770            j
           1259    244870    idx_base_GuestAddress_Id    INDEX     S   CREATE INDEX "idx_base_GuestAddress_Id" ON "base_GuestAddress" USING btree ("Id");
 .   DROP INDEX public."idx_base_GuestAddress_Id";
       public         postgres    false    1777            m
           1259    244880     idx_base_GuestHiringHistory_Date    INDEX     �   CREATE INDEX "idx_base_GuestHiringHistory_Date" ON "base_GuestHiringHistory" USING btree ("StartDate", "RenewDate", "PromotionDate");
 6   DROP INDEX public."idx_base_GuestHiringHistory_Date";
       public         postgres    false    1779    1779    1779            n
           1259    244881    idx_base_GuestHiringHistory_Id    INDEX     d   CREATE INDEX "idx_base_GuestHiringHistory_Id" ON "base_GuestHiringHistory" USING btree ("GuestId");
 4   DROP INDEX public."idx_base_GuestHiringHistory_Id";
       public         postgres    false    1779            �
           1259    257338 !   idx_base_GuestPaymentCard_GuestId    INDEX     e   CREATE INDEX "idx_base_GuestPaymentCard_GuestId" ON "base_GuestPaymentCard" USING btree ("GuestId");
 7   DROP INDEX public."idx_base_GuestPaymentCard_GuestId";
       public         postgres    false    1841            e
           1259    256328    idx_base_Guest_Resource    INDEX     Q   CREATE INDEX "idx_base_Guest_Resource" ON "base_Guest" USING btree ("Resource");
 -   DROP INDEX public."idx_base_Guest_Resource";
       public         postgres    false    1775            �
           1259    257571    idx_base_Product_Code    INDEX     M   CREATE INDEX "idx_base_Product_Code" ON "base_Product" USING btree ("Code");
 +   DROP INDEX public."idx_base_Product_Code";
       public         postgres    false    1811            �
           1259    245794    idx_base_Product_Id    INDEX     I   CREATE INDEX "idx_base_Product_Id" ON "base_Product" USING btree ("Id");
 )   DROP INDEX public."idx_base_Product_Id";
       public         postgres    false    1811            �
           1259    254639    idx_base_Product_Name    INDEX     c   CREATE INDEX "idx_base_Product_Name" ON "base_Product" USING btree ("ProductName", "Description");
 +   DROP INDEX public."idx_base_Product_Name";
       public         postgres    false    1811    1811            �
           1259    271771    idx_base_Product_Resource    INDEX     U   CREATE INDEX "idx_base_Product_Resource" ON "base_Product" USING btree ("Resource");
 /   DROP INDEX public."idx_base_Product_Resource";
       public         postgres    false    1811            �
           1259    256315 !   idx_base_ResourceAccount_Resource    INDEX     u   CREATE INDEX "idx_base_ResourceAccount_Resource" ON "base_ResourceAccount" USING btree ("Resource", "UserResource");
 7   DROP INDEX public."idx_base_ResourceAccount_Resource";
       public         postgres    false    1836    1836                       1259    270298 ,   idx_base_ResourcePayment_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourcePayment_DocumentResource_No" ON "base_ResourcePayment" USING btree ("DocumentNo", "DocumentResource");
 B   DROP INDEX public."idx_base_ResourcePayment_DocumentResource_No";
       public         postgres    false    1877    1877                       1259    270208    idx_base_ResourcePayment_Id    INDEX     Y   CREATE INDEX "idx_base_ResourcePayment_Id" ON "base_ResourcePayment" USING btree ("Id");
 1   DROP INDEX public."idx_base_ResourcePayment_Id";
       public         postgres    false    1877                       1259    271706 +   idx_base_ResourceReturn_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourceReturn_DocumentResource_No" ON "base_ResourceReturn" USING btree ("DocumentNo", "DocumentResource");
 A   DROP INDEX public."idx_base_ResourceReturn_DocumentResource_No";
       public         postgres    false    1879    1879            �
           1259    266266    idx_base_SaleOrder_Resource    INDEX     Y   CREATE INDEX "idx_base_SaleOrder_Resource" ON "base_SaleOrder" USING btree ("Resource");
 1   DROP INDEX public."idx_base_SaleOrder_Resource";
       public         postgres    false    1845            �
           1259    245314    idx_base_SaleTaxLocation_Id    INDEX     Y   CREATE INDEX "idx_base_SaleTaxLocation_Id" ON "base_SaleTaxLocation" USING btree ("Id");
 1   DROP INDEX public."idx_base_SaleTaxLocation_Id";
       public         postgres    false    1795            �
           1259    245313     idx_base_SaleTaxLocation_TaxCode    INDEX     c   CREATE INDEX "idx_base_SaleTaxLocation_TaxCode" ON "base_SaleTaxLocation" USING btree ("TaxCode");
 6   DROP INDEX public."idx_base_SaleTaxLocation_TaxCode";
       public         postgres    false    1795            �
           1259    245807    idx_base_UOM_Id    INDEX     A   CREATE INDEX "idx_base_UOM_Id" ON "base_UOM" USING btree ("Id");
 %   DROP INDEX public."idx_base_UOM_Id";
       public         postgres    false    1801            �
           1259    256314    idx_base_UserRight_Code    INDEX     Q   CREATE INDEX "idx_base_UserRight_Code" ON "base_UserRight" USING btree ("Code");
 -   DROP INDEX public."idx_base_UserRight_Code";
       public         postgres    false    1838            �
           1259    255787    idx_tims_WorkWeek_ScheduleId    INDEX     _   CREATE INDEX "idx_tims_WorkWeek_ScheduleId" ON "tims_WorkWeek" USING btree ("WorkScheduleId");
 2   DROP INDEX public."idx_tims_WorkWeek_ScheduleId";
       public         postgres    false    1825            k           2620    235953    pga_exception_trigger    TRIGGER     �   CREATE TRIGGER pga_exception_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_exception FOR EACH ROW EXECUTE PROCEDURE pga_exception_trigger();
 =   DROP TRIGGER pga_exception_trigger ON pgagent.pga_exception;
       pgagent       postgres    false    19    1754            �           0    0 .   TRIGGER pga_exception_trigger ON pga_exception    COMMENT     ~   COMMENT ON TRIGGER pga_exception_trigger ON pga_exception IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    2923            l           2620    235954    pga_job_trigger    TRIGGER     j   CREATE TRIGGER pga_job_trigger BEFORE UPDATE ON pga_job FOR EACH ROW EXECUTE PROCEDURE pga_job_trigger();
 1   DROP TRIGGER pga_job_trigger ON pgagent.pga_job;
       pgagent       postgres    false    1756    21            �           0    0 "   TRIGGER pga_job_trigger ON pga_job    COMMENT     U   COMMENT ON TRIGGER pga_job_trigger ON pga_job IS 'Update the job''s next run time.';
            pgagent       postgres    false    2924            m           2620    235955    pga_schedule_trigger    TRIGGER     �   CREATE TRIGGER pga_schedule_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_schedule FOR EACH ROW EXECUTE PROCEDURE pga_schedule_trigger();
 ;   DROP TRIGGER pga_schedule_trigger ON pgagent.pga_schedule;
       pgagent       postgres    false    23    1767            �           0    0 ,   TRIGGER pga_schedule_trigger ON pga_schedule    COMMENT     z   COMMENT ON TRIGGER pga_schedule_trigger ON pga_schedule IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    2925            3           2606    235956    pga_exception_jexscid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_jexscid_fkey FOREIGN KEY (jexscid) REFERENCES pga_schedule(jscid) ON UPDATE RESTRICT ON DELETE CASCADE;
 S   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_jexscid_fkey;
       pgagent       postgres    false    2643    1767    1754            4           2606    235961    pga_job_jobagentid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobagentid_fkey FOREIGN KEY (jobagentid) REFERENCES pga_jobagent(jagpid) ON UPDATE RESTRICT ON DELETE SET NULL;
 J   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobagentid_fkey;
       pgagent       postgres    false    1756    2628    1758            5           2606    235966    pga_job_jobjclid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobjclid_fkey FOREIGN KEY (jobjclid) REFERENCES pga_jobclass(jclid) ON UPDATE RESTRICT ON DELETE RESTRICT;
 H   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobjclid_fkey;
       pgagent       postgres    false    1759    1756    2631            6           2606    235971    pga_joblog_jlgjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_jlgjobid_fkey FOREIGN KEY (jlgjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 N   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_jlgjobid_fkey;
       pgagent       postgres    false    1756    1761    2626            7           2606    235976    pga_jobstep_jstjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_jstjobid_fkey FOREIGN KEY (jstjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 P   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_jstjobid_fkey;
       pgagent       postgres    false    2626    1756    1763            8           2606    235981    pga_jobsteplog_jsljlgid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljlgid_fkey FOREIGN KEY (jsljlgid) REFERENCES pga_joblog(jlgid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljlgid_fkey;
       pgagent       postgres    false    2634    1761    1765            9           2606    235986    pga_jobsteplog_jsljstid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljstid_fkey FOREIGN KEY (jsljstid) REFERENCES pga_jobstep(jstid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljstid_fkey;
       pgagent       postgres    false    2637    1765    1763            :           2606    235991    pga_schedule_jscjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_jscjobid_fkey FOREIGN KEY (jscjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 R   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_jscjobid_fkey;
       pgagent       postgres    false    2626    1756    1767            K           2606    255621 -   FK_baseProductStore_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id";
       public       postgres    false    1811    1816    2718            C           2606    246204 8   FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" FOREIGN KEY ("VirtualFolderId") REFERENCES "base_VirtualFolder"("Id");
 v   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public       postgres    false    1797    1787    2696            i           2606    283533 #   FK_base_CostAdjustment_base_Product    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "FK_base_CostAdjustment_base_Product" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 e   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "FK_base_CostAdjustment_base_Product";
       public       postgres    false    1811    2718    1897            e           2606    271772 7   FK_base_CounStockDetail_CountStockId_base_CountStock_id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id" FOREIGN KEY ("CountStockId") REFERENCES "base_CountStock"("Id");
 {   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id";
       public       postgres    false    1883    2841    1885            I           2606    245349 .   FK_base_Department_ParentId_base_Department_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_ParentId_base_Department_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Department"("Id");
 l   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_ParentId_base_Department_Id";
       public       postgres    false    1807    1807    2712            ;           2606    238255 -   FK_base_EmailAttachment_EmailId_base_Email_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id" FOREIGN KEY ("EmailId") REFERENCES "base_Email"("Id");
 p   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id";
       public       postgres    false    2647    1770    1769            J           2606    256202 %   FK_base_GuestAdditional_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id";
       public       postgres    false    1775    1809    2658            ?           2606    256207 "   FK_base_GuestAddress_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "FK_base_GuestAddress_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "FK_base_GuestAddress_base_Guest_Id";
       public       postgres    false    1775    2658    1777            <           2606    256212 .   FK_base_GuestFingerPrint_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id";
       public       postgres    false    1775    2658    1772            @           2606    256217 0   FK_base_GuestHiringHistory_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id";
       public       postgres    false    1775    2658    1779            A           2606    256222 *   FK_base_GuestPayRoll_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id";
       public       postgres    false    1775    1781    2658            T           2606    257333 .   FK_base_GuestPaymentCard_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id";
       public       postgres    false    1775    1841    2658            B           2606    256197 *   FK_base_GuestProfile_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id";
       public       postgres    false    2658    1775    1785            [           2606    268363 )   FK_base_GuestReward_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id";
       public       postgres    false    2658    1775    1865            \           2606    282522 2   FK_base_GuestReward_RewardId_base_RewardManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id" FOREIGN KEY ("RewardId") REFERENCES "base_RewardManager"("Id") ON DELETE CASCADE;
 q   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id";
       public       postgres    false    1859    2800    1865            S           2606    256031 +   FK_base_GuestSchedule_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id";
       public       postgres    false    1775    2658    1832            R           2606    256023 9   FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1823    2741    1832            >           2606    245511 $   FK_base_Guest_ParentId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Guest"("Id");
 ]   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id";
       public       postgres    false    1775    1775    2658            D           2606    245230 (   FK_base_MemberShip_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id");
 f   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id";
       public       postgres    false    1775    1789    2658            ]           2606    268533 =   FK_base_PricingChange_PricingManagerId_base_PricingManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id" FOREIGN KEY ("PricingManagerId") REFERENCES "base_PricingManager"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public       postgres    false    2805    1867    1863            ^           2606    268526 /   FK_base_PricingChange_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id";
       public       postgres    false    2718    1867    1811            c           2606    270285 6   FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id" FOREIGN KEY ("ProductStoreId") REFERENCES "base_ProductStore"("Id");
 t   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id";
       public       postgres    false    1881    1816    2731            d           2606    270277 $   FK_base_ProductUOM_UOMId_base_UOM_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id" FOREIGN KEY ("UOMId") REFERENCES "base_UOM"("Id");
 b   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id";
       public       postgres    false    1801    2700    1881            H           2606    282481 5   FK_base_PromotionAffect_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id") ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id";
       public       postgres    false    1805    2709    1803            E           2606    282486 7   FK_base_PromotionSchedule_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id") ON DELETE CASCADE;
 |   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public       postgres    false    2709    1791    1805            Y           2606    266570 ?   FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_" FOREIGN KEY ("PurchaseOrderId") REFERENCES "base_PurchaseOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_";
       public       postgres    false    1853    1855    2794            Z           2606    267545 ?   FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas" FOREIGN KEY ("PurchaseOrderDetailId") REFERENCES "base_PurchaseOrderDetail"("Id");
 �   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas";
       public       postgres    false    1853    2792    1861            h           2606    283500 '   FK_base_QuantityAdjustment_base_Product    FK CONSTRAINT     �   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "FK_base_QuantityAdjustment_base_Product" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "FK_base_QuantityAdjustment_base_Product";
       public       postgres    false    1811    1895    2718            b           2606    270170 ?   FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id");
 �   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa";
       public       postgres    false    1875    2829    1877            g           2606    272137 ?   FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP";
       public       postgres    false    2829    1889    1877            f           2606    272109 ?   FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu" FOREIGN KEY ("ResourceReturnId") REFERENCES "base_ResourceReturn"("Id");
 �   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu";
       public       postgres    false    1887    2833    1879            U           2606    266129 5   FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1845    1843    2777            W           2606    266260 6   FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2777    1849    1845            X           2606    266363 ?   FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_" FOREIGN KEY ("SaleOrderShipId") REFERENCES "base_SaleOrderShip"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_";
       public       postgres    false    1851    2781    1847            V           2606    266222 3   FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1845    1847    2777            a           2606    270034 ?   FK_base_TransferStockDetail_TransferStockId_base_TransferStock_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_" FOREIGN KEY ("TransferStockId") REFERENCES "base_TransferStock"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_";
       public       postgres    false    1873    2820    1871            =           2606    266390 /   FK_base_UserLogDetail_UserLogId_base_UserLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id" FOREIGN KEY ("UserLogId") REFERENCES "base_UserLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id";
       public       postgres    false    1799    2698    1773            `           2606    270029 /   FK_base_VendorProduct_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 p   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id";
       public       postgres    false    2718    1811    1868            _           2606    269667 ,   FK_base_VendorProduct_VendorId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id" FOREIGN KEY ("VendorId") REFERENCES "base_Guest"("Id");
 m   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id";
       public       postgres    false    2658    1868    1775            G           2606    245123 9   FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId" FOREIGN KEY ("ParentFolderId") REFERENCES "base_VirtualFolder"("Id");
 z   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId";
       public       postgres    false    1797    2696    1797            j           2606    283483 "   FK_rpt_Report_GroupId_rpt_Group_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id" FOREIGN KEY ("GroupId") REFERENCES "rpt_Group"("Id");
 [   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id";
       public       postgres    false    1890    2853    1899            O           2606    256119 (   FK_tims_TimeLog_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 c   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id";
       public       postgres    false    2658    1829    1775            N           2606    255858 3   FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1829    2741    1823            P           2606    255871 3   FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id" FOREIGN KEY ("TimeLogId") REFERENCES "tims_TimeLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id";
       public       postgres    false    1829    1831    2749            Q           2606    255876 >   FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission" FOREIGN KEY ("WorkPermissionId") REFERENCES "tims_WorkPermission"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission";
       public       postgres    false    1831    1827    2747            M           2606    256143 /   FK_tims_WorkPermission_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id");
 q   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public       postgres    false    1827    1775    2658            L           2606    255788 4   FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2741    1825    1823            F           2606    245269 ?   base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati" FOREIGN KEY ("SaleTaxLocationId") REFERENCES "base_SaleTaxLocation"("Id");
 �   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati";
       public       postgres    false    1793    2692    1795            n      x������ � �      o      x������ � �      p      x������ � �      q   X   x�3��/-��KU�M��+I�K�KN�2�tI,IT��-�/*�2��\+�</�477�(�8�$3?�˔�7�895''1/5���+F��� �B�      r      x������ � �      s      x������ � �      t      x������ � �      u      x������ � �      �   �   x���kj�0F���*��of4��"���p�]�!M�_U��5�RB�"�  $P"s��ΏC�x�h�f������\�k�̆��nd���QA,,dl4����@�>EJ��S���U�p\r�z�8U���M���TH���~�k�^a��R���s����?����zx�/ㆼn>e)D>�3���M�J�Y���$�-ߞEKw�M��{��'��l�      �   M  x���;n1�Zo/4(�_&�2���%�~�t)�b �+�M[�ְ���v��:b�Mk��]~a}5�;������Z:�u|��xZ@�0��-Zk�v��1��ء�m� `�1*�q�c����d�;��@/���m ���S�G�X�c�M>�JL���c�-��Y����-��t�%1�v�F�Y���ONۻ���c<��ς�|���)Qb�j�b�l�O�������t������C�=g�8��5�{�&�`+�Up��
ff��!}��?D��i��~������X9�:��%Vͩ�ܹ�@0VZnZs�������������x�^%6�      �      x�u�[�f�U��ۿ�H���i��"�D$$��0DZurF�v�8���y���I��{<���W�ֻ�C���w���/?��w���/1����o>��߼����}�������/�Ţ�TS��%��������o�y���J���W���͗_���x�!ė���������~��������_���?�������?��e/�����?�������_��������_|�_�߿}�)���D�������۷�u�����Bq>J}~`UϿ��^7C�oG��>/����{�_��������b�����7���J5�)��#%�tSH9�4rO+m�qr̖C�l�u��<��u��𬠤������o}����z���|�k}x���k���ޥ���C��oz�l+�`�*L�߼�j����������<�H\Ũm{}g�����x�2�����Õ��\�Oz)f
I�"�IŌ���Z,�)��I�N�VsK*_��W;�+�g���j_W�U��w�C����������C�4T9Ϭ�=��S�B�!�\K/}�0��s<�K��j��ּZ��+��T0��F*��x�c$Oq��=�߬'Z��͋��I�K���Lj��˩����zvc{�{���u�O����L}2:q/P;G�9Z.֋5+�}���}ʲY.])5��Շ�d�cw�����w���L���x�w�uz��Q�4Z�g���:�K��e~�d����7~w��U�1,����m����ӌ9Q����)�~�e��f�l������6��u�=$�Ӣ�9)�צ)%�`��qjW�td��h��KJ�^RYb����}h���G���׾)����6O��ʸ��{�_�R���E�R�A�����G��4�U�_�s� ܏Ls�s��ɋW��k�S��'�}��4��.ÔWp$;O�r�i�:����x����<�L����}�Z;7�]R�t��	��^a)��8/kZ
V��j�Ě��)�]�\��/�a)�B���pf
��Q��<�C��b���ЧD�N�V��kz�-K��lL�|zKX�rXW9�z�i��L��^S��鵕����.��U�Fs>/<��@=���GeE�p��zO{e�孳���������m�LE-\ê1�y�1��e�e�k;�˕ך�����1�����������o-�̊���3�sEz���sS9��ɜ����uzz�B��.Zy��@DJ���q�;��r�
�tJa�Mkh~��Ȯцߦ�ob����f4 t� �+B�*Eah����!��\GU6fF���!���(���f����l/R@�7�cy�e����Ǭ�{T�C�/��d���Y���ii�׸m�8(&s�CW@��t�i�u�{[��%��9;>�x]e� ��Ñ.��4F=��`,S3�ڳ�����&��6���n�D9lQ+6f���u��T���r4�)�	�摫1��	w@����V�3�.��. :m m�Tt�>�1 Qe��Q�3��~���K����^��BS|���y*�w�"aC<���ǃ�;ю�A�Pf�Tk�=�+�l*7v�4i۠���a�;ԃ?��Y$B�jcL���̥�u��d4s@[��>$��[�5�G#2H�X��Q+ʻ,!�M�Q�mw�}���G*�YV�|��g &F^�`㤋����U�H� îI��C��1ai<,�Wك����v���'���܊,G�d`j�	8o�ȄP�<()�.7ʛ4�>�-��Tׁ%����0LXH�C�0㡂,�s(څϨ�˔��;ۍ+ìl:]�a�.f%�� (zs�D�����@/@3ױ8H
�\+O���r8�w>�lw86��׃F&�};pY(�͘WГn͌�*�N��i(�[�Y����-;_��nv\.?��7�!T
B�F���@�٭�#ᥨ%Dю�J���I�X�S@)���n<c�i�a�s�e@��
��rf�zn���ʋ���(y��,�dc.��qZ�β��+�ٚT�y(@b�^AG�F�����o@-�eh���!����$��P�xp1�2���|Ȟ�UtP�+��Ʈ��qq��v�pt)���!���U�5G�y����"FߌBD ��~N�/.SXC��"�A��p�CZ1$,�pTs����Rx���a������b��[�H��-�eJ�(�#�EcgX�3 �
#��@���j��R{��p�׀2��V�DbP�IŘÅ��gb3J���h�eNvkA=�xL�Y���tK��JD�QbQLY9J>6���=*�i8��LPc6�j�q=@kXB�7�$�{=�MB4�;�V�d%�a�2(L��J{�xʪ�:�:����!��x�z���ĕ�)cN�.��č��=�F�	"bB4�p�<�{��ME��;�_V�)���D#ZAA�1�v�`��q����LcU���n�r�L�nx��dp�ݧ�7�*{Oj�6&O�z�of�UΖb_�~������~x�9��X�y��H�)�TB���s��r2~t"�eAI3C�`�	3@����g�����&���\���X�覦�ø+c�઺np
WG�*8	�P0gĒt�$ݱG:�� �U�Z8�f�k�i�p�&_b!�0#����֞�I)�h���X��X�(�*���$NQԑ�6d��^1�ݕ̵�)e!�|S������4$��s.�7�/e Hz�u>w"F����*+
"����
�R�Q��`�u��ƛ� ׭�|;N�<l-�7��d��".�Y�1��o jbj�^�۲�PY�e�ACH��P�F�W�op���7��0u�a|A��Z�|dk��0����i7��C��ג��]ɼU̔6p�Pe#.�G�"���\�������
Xe�y!-�~�R)�*��fq�����$fxH\tF)ڀ	w���<�}�VX!�a�?g��`�G�e���kH��]���'�Q��XcAH,����܋�I��y.�G�C���$�a�xV�l">��9��� q����E��Uf�"&b�S��X�R['��T
|8�� ��CL�p�M-�-2��6^�hAg$��q���H*OT�0�BS*1��S\nk�)[�� tEZ��М2��ej�|��+���T&7���-P��y8#��Q:��g�W�h��"��H@msoH�@ee^PY� J(�F����Lؠ�,O��Q����T6��Q�5�N���w�X:�C+2V[�%UG�m�H���0f��B�B�j��x��&Awf�ŝ�W��ɦ��%o�4��DA�ұ�#@Kq���G�d�c��˞e ��'e.�%�t�h6�b���`krL���Q��t�)��B��9���B��;Ⱥu���7Ỳ[l��k89$8�	�ڮ?t 5zC��]o���-�k����Wr�#�c���aE�#X;���$��5t��l�W(	�-�6�����֫�:������Qmc|����:��]�@��-S�ʄ�Be%(��	�,�@.��CS�aK���:�
;�5��^��i�g�C���]:(x���ą�wu�#�`{�������#F�D�cj��I������S6�����
�w�9��C|�CP�x|Y�x�P�k���d�qz�2�V	c�m�L�����й����ѩ�Hj��v�"�u��t+7D�d�&w2��J �@a(PPOY!���
���N7�ۨ×U����b `-�^�f�:�����"\u&@���8Id�{>�U�x �|7�20���ʑH7�����9`Ę��[	��s���<L�X��\�w�������c��\��Mn���o_j��W��A�?�T��@J�'ŕ��-�0*�W9;���7.���Q"ܚ�,����&vw������G�w�?.'�+R�qw��s�̅��j,t����udǙ)�����9Ɛ��4��G�j@\,5�$�.bb�ۨ.���R�v�t�u��X�%���2����"ԟ�)C�H&o$ `  �D7u�>|�WTU����ǚ�R"�H�u=ҋ�cD��IuD"������"F�<��+��ա�D�=n�- ���f"��6�2OI(�Qd��}��/���(x&�y>N��	����=��%B�\H�( ����&2�v@,� Ríc!�kL`7	�*�㥐i�C�� C���b�-��Z6��YϞ��q:�����a:�H���&B�NG�sgc�c�q-�QEWD��>N��\��h�n��iH�:� ��0 ���JӦ'C$H�C������ی����P��1��݂����t����NF�T��r�A���h<��k���l:L �H��>= ?%��Y?�t#L:�P�OT��a~�N+�B�ɨZ�2(��g=')D9/`���!鍜�&�%r/L>�AqV�8i�&ψ�$�����	�==��8���!c��0'`i��$�z	?a�x �@"�H̓]�#�}�%�C���T��k ��YR
ٮ��5B��H��F@Y�:f#�!��>��!Z~M�.7U �GGT��M���l+c}_y����Md���x���-�'�C/�8�2�sC@ĘA(�UQ(F�c�˃L�i�h9�������bT�M�m�I\oX`���.� �9u� Z2��B�%;���|�窣0͒;4d%b֙Z�4���@��sW6`���a�b���v��%� Y���T<N�YJ�̾�,n��B+�Hx#=aP�\�<��M���:�HG`�-�'�
�z����b�q1>!nR9��@��@�h7&	���k��uǑI�A���ka��|�MtWl�U��<�b@R���a��u܅1���I4d�صU[Zz_��1[��<麙���D�1�]M��S�"�Fh�Ȭ���&�9t�g������h\� by �= b��! �f�+�e8�%Z�C�oT���C#$�I!�2.���fd�y��L�8�^��	k�Tt���(9�B�	c �xJ�gK��dR�V �Q�Ƒ�?��YN�6��B��#���~X�=���������u ��U(ܡ�*�D3(L����4Z���b��h�!y��DΈ�H�`P��`��辚��DJ�J�ĝ��o�Ι���j'8��(�U@U�a'!�%�4A�ɠs��������p�%�\Lݼ��hF�AD�n7���L^$\�C�J� cu)(�e�V������SBH�P�d.�P>X���壝5"?��b�alrɐͷ��Qf���R$�����W���G�T�x��،n�aRГ��*t���fS\��x�>�ߺ~���q-����@a���~p��b�ѽ������K�a<8̐(����,�~�	~�a�lHo��Q+g�U����Ə:`� j���0t����,$��V'�����ݺ�t���a�+?�ʡ[~e�58Iiu���n����'/:�ͺ�	�L1��(|v��L��[{;ڋ��N�G��p��}�	Z(A;��o��-����o�ʪ��y���)�������Vx�4�b/߾���}i��3=�c��7���a�y���o�/?xI/��O���_����ٛ�ۯ��?���_��'�?��w߾��������������'?��훏>����z�      �   �   x���1n�0�Y>�/@ᓔl�k�� �l�-�&�޿N5:^�'�aq�T23!k��D6��,`�Rբ�~���������n�2�zNʝ{Y�k����u=���g�\.o�˹�ӫC#7�8�J�D)�D�hD6�k�8���s�y��yۉ]����kА� Q����=����æ�裚�$�|�4��v�      �   h  x���KjQE�]���OYDV���Ɔ؞d�D��n�qcjR���+�HN$h�d��'F@�o�?�?�6��T�$���Ԙ��9ǎ��>H�P�7��iic[A�(}�,>�f���0AA���"'�R֊-��tT��u���vPyE����-(>��E��wD�Bo}C-�W�G�����&7����0�3y�@�=;�T�+?�T��+(�h�)��m�k�̪#�t�u�UB%/(�繑+��P�#V�2���]U��sB������`<�\��FTHQm�J6�4���G]eq��*h�W�|~2��#rиf.k��@@B�e��r�-vͬ�lp�!�}�e�RT�#�l����M���Fc
�Y|1ԉ�e�n�ت}�k�t0_:p.7��P)"� E��&���(�Xi��(���ʧ���x7$�H��������
+ǵ2ٮ�]�%�WϹ,��+?I�r`k3h=si���VÃ�EV�$۰�>m��q�F��y(7ε��C~9$��h��7Y��쟧��wO�zQN�z��#�=���R��2k�xfq�p��˼��{�����W1a��u�}T���e�x����@z�]      �   �  x��XKr%����	${y��_�KNUqzh<�p��R�TJ8����J��JT�T�.M�Nj�P3W5����J��/��'Ƌ�I���z�ɋ �#h��n��ߐ�ⴐ��U�D3._�$a��/Nb�u��"P��	8<`6��)�<iMN�0jt��ڳ�V ����+�TTI\��%���)���̫-���;�|y/U(�*NJ��D�?!#_��"t)�}M�Ғ\���[�n �d��jO���d����ظy�%�$��:p'$���Yԩ����vc��MW0J�:�<�d#W=!�P���ܕ��_(z5Egi):���EN��EF�=��!wW*�f������(mU*���!a�,a��;��;Y��P˭7KG���ZB��ǉ�'5��w��oV���$as-�[v���|�Zɇ�j*d��[Q�W�1���Z�;!�R�Ց��rN�V�-��%��!�+���B�K�۩��}C_Aq�k@�Ï��'�=*>��Z��ZSd��
t�	~CY<'�7e���]��\E�2�&���α������*��6��foN:�+C�3Ier����R���y���!J�"5liPE�v�_-6���ޒ��b2gVW;�F��3h�Ȁ�[^�y�é,���>X9�X����aW���1�6n�Q�78-�9#N�2�^6�������<�'�,o���K��Bؼ��������{΋�_y���~��Q�)X����F��#f;d�7갱g��E��1��=�����'/�?�d��gf�+�1J��?Ghc�<1c��Ӄ.��~�{z�q��.�1���i#���)���簖 º��*�l4�����������/Ju�z�1��.����gxԔ�h�b����@ԀO�V��Y��6��$䰥�����$��; |=3:'7S�4ۘ�9�v����I{����-`3�2���s���hH�B=�Q��!�3��4�Nq!W�%]Us=��5L�sx����O�e�I]�F�%v�$�H�̐�g ^^�y��m
�I0�a�R8��t-��)"�U�'a	�Z/�g嫭�?�^��W[��g� ��pAV�+�!|�4g�U{����t��e�S��2�
:z\��n��N�F�@���Z��`���m�b��<B�?�z/4��V��"���E���%�9Z3�pL�����kZvMJS[̘��a~k�a�����6�G�M뷫a��ݺ�<f��H��C0em7857j�����6�߿���u��������
k]k�_��ôR_V��TI;�YF�)���F������>^��a�z��ˎu+��iǺ�u��c�ӭ�wƧK�[e�.��癅SM2|�;�+��7�������a�g����mR:]�}��݄����:�Nw�"~�|Z�}�F:�_h8���u���'�#Rlq�J�#�N�?u.����~��������dۿ      �   �  x���Mn�F���S�Mԫ��Ζ ;�����ٔe�EX"��g;�9�,f� �K��}�d^Qdˎm�R��рU��WￊO:�����i�(�/g�g�w����Lp��z"�k��ɔ�J�K�t�����P]1��c�x	nr�3PJk�c	᚝��)�\�(���XB�a �[4bR���\��!j��r=;���:������H��Ͽߥ����0����`ras�3���c)���뫢j�0g��������s�K�9c_�<�ؤX5��f��4�\�M�(�O��;�u1r��^<���F�� �9�r,g����i3���ϋ�X�AH�WJq�c)6,�*��_%�]��MfE:���*�q��ӲXB%�~:K/(w�ُE:
w�H�
��t����k����YX��:��&��0@�����d��� Ox��ʛ���"�P�f��"�2;�*��j{I_�1�0�)�ԴL��=U�cV�(����Z쨬f�X�y��CJ6�yX�B�[�A�}s<"�{)�ѴXR���9%̰X�1/Y�K�ޟP�0/��+J�e�uuGIt�i舾-Cd<�٠^�JK�.f�3���)�Ih�����
Kz}Sd�B�m(�AM�t<�ۇ�X�#z6��zY�)��������X.)���N'J�`�Y9/oW�P<m֫Yy�*�+�6f�۰l�Ŀz2�S:O�l�l.!S^+gv,!޿��M�����ٱ���3��W��{-�߱�h,Ƴ��%԰�I���)���CVuUN)�v�tP�u����Iy.�j8��j�9%R`�_�B��!�J����^5uEIE�/�u�*�d��j�Ū)�%���'��0���!%�w�����!� �言^_ϋe:
�N��d�x��D3"e��\�y⡛����>!T?��j�_MF�����'�D�%����)ؤ��=�(���
M�v���mT	�r��Y��Tk:�f�˦���d��_�Am�� �9vZ�UqS���癔��9���1�!�,5w�dM��l\ܮJ&���4L㹙�j��ោ-���uu��dV��
�0#�zv��u}S�R�Q �V�
 �����?�_x�
\�㶝T��0N��U�f?er)r D	o�it�t���8�0�����޼Oa��r�2.��xv6U:��=X�d��C�&�\C$�T	:�$��y�:�h�Rf�Q!��g��� ���k[M�L����s�2ǹ�"AOzn������C����j���fu(;��&���R�x_$��$/��tP/���^vq�{D�!�\H��l��˫��u_x�ʆ�\��ÕBu�]X��g2q��˴1�!S�p�u���e~�\�8WV&x���`:���^~�[Y7n,=H�dzqr�������
�&�qZ'x2���k����#�A�L+�K�h;���Շ��K�zpbb>D�h�G��3W�ǮAX���-�`�v��|�W'��6M���Z[�L�C��u��$��/���@��T�J�+�ŎjL-늙�}��Z#PÚة���������E@$���Pwȯ�T��Z���~�l]Oӳ��ح?���GHFk��d���1�t�j��u� ˜c�S� �Y;aWU��n�qx&���%lG���)��0E��)V� ���K`6���J���Ct������飺%�<�4�\�\K�l�ܱ�H���oU:���"�Gy���S����sY��:�7�Y��A�tI��4h��q����oy�<�^�@��#���'�o,%�w���������īn�`�W�L�8���
>��/��t������q|�?|��d	ٮ}Y���x��b��m����+�Z�V%�p�C#/M��K۶�;�ZnZu�O�vIz#$�Ѿs��~?0�Z<�%K���G�7�      w      x������ � �      v      x������ � �      z   |  x��Z˖��\C_����ӷ�͕ɱ�X����ʛ~af왡<Cő�����9'�x��П�����H���$�@�nU�	+���?������/������k�-�����zv}�;�Rw����,?�_��?��ŢO�����'ɸf���̕��k����[~FvNj�u/$n�[vT�I��#��X��q�UC�{Ώ�:�1�<vf�c������$r>p������(&���y[�y��u��\m�{;�g(��������,�ϯ�����qY��1��R�y�?�A�s-	.��������X��Q�VZ��v,[������wm�:l�{88o�;mT���	R��s�#��) Čx 0����d}��`�A��M��g 3��(�i���,8�Y��jkq�{10���y�g��դ���8���֩j�j��<?}�Տ0s�{%�4�ܑsr8�^����Q��F�����
j	�4G��
�0�QOş5�C9	�n�u��/fz��,gz�wW7����M����@]=�_@j�lR32g\䪼v

��Bօwp�sE��S�QA3ȍ�s�=T���*N-5��g*�(�Ϝ%5�,���Mi�/D�X�-��S�bmwU����xs����6�>*w p��M����4�f�TO�a;�5e����T�lʬ\'arH�%c<Sr�,X�I�e�<�x�!'+��������C�z�}��k�b���vN�;�^�����!S݁���0,H��v�«������rg��|�:�e��@!I�!)U,�eȀ�r}Q�W���C}}������tf�i��ڊ]t����Ԟ��t\����9WP�^p�I�����R1g�(Дd�(q0L�Z%:�Z*!���B���uY�V���{���]����s^� ���/���Ϗ��y��Op��@��mQՓ@��B��B��^�-!�89z��IՑ�!�h�RB�2B��O�c�Aq�ZpQ %�]5ڌ�"�D����򊍞�?�r�@��n�eח�;�9E��i	;�֖i ��B�q��ԗ���̜/2��..��*�`IEF&?dkMA\� �Vؼ��܆�6�X^Ĕ?oԣs;��7R;D�@j1��ia�"ǿK�ep�d���Q�g�LP�yrn}9"ڮ������t���x(c�G�r�6ۅ�ӌ�){�K/���� ��o!ǣ��أl�ӥ �`�k�Tf�ʀ^�[fZ�b�`���'����u=������ݞ3������i�)a�	x�U�E���0w!$�Lo�������T���������<��SA���v��lH%�̅�^;ţ>	�h�m������Dٱ
���1;�m��]�-b����O�ﲶZX�[i����F����S�u="	q$g�4��v����N��LEBrN<1O�YI^g}�.�ĉF��$�}1�!'��hd�mq��^�&�Y�m��cA���@�̯� �j���7�MF�J�o)��s�qO�E�m��ՙ��X�<E����<�2Ѿ&;pn$�j8U�G�t;�}ңS�4I}�HM���C�k�k#��Cr���x���bХ֎�>$4���˂�YV�/�$��� ��`�d��w!���eK�EI�ګ֎u�X7�u�O��^�F�6s�{Oh�����:_#~��]��u�EeW���+ց(����4:��YL
k,���T����Cm�����B߮{ �@�ӟ"Q�qd�<(-I��?���C4�'�s��H��	��K���PN�gG:�Ih��S���M�>0-f�G=�����C�hItqt�r�8g^��bf1�cET�Le�|�cD7���Yצ=�Ͽ��"������e�/q�i��l'�嗧ǰ|�xj/�6�\���Ҋ�`�F��s�z��`�4r�h%���_J<�9v������R�&є��側+e�p������V/����:�B�J�xB�ݼ?РPs�Ψ�p��?����Q0��=	�2��G�?�GQ��t,YGcV0���
Q)�"��<r���v̓o��w��G��^8e|;�ߟ�ݣ_W0�U��r�-5��{g��[��i'V�{�_��0r���p.�2
�[qb�هbmk�'�޷=�y������*^?���G��4' ��S�1�@r%T�s��Q�( �I���i���śA����M"� �,�`/%j�"�����e�N� [���ֺ���-��o�V51�h�r^���4V��Z��Y,�o��Ic�T�F���������Vs���fL�%KQ�_J1-bQ�	���,"b"+���P\��]��U����'�����w!���]�8 ����u���T�F��)���˰:�V�K&���b`��IH���b��>]fk��W���m�^}����C�#�>�x�N�Lu,��_i�kg�&�ż�ShP)b
�-�G[㜉�pp�� �u�[�����̻��Y�
���%
��5��V�V�wB���O�\����)'Lur1�O�jq����~"6ȌT���EDMY$�O5X�¦B׽���˗�w�i��1��ʵWG��,'��P����''%/n"�)(e���!�ꐙ�c))Ӝ�� �M�yYǉ�?��� #HT��m8�Z'���>�!s) ũ�yB���f*��BBc�� ��Zs������^K�+g>�q�;)�+k���z�w=�0���E�����]ǻH<����A����HΒp�B�P�mS��:���g�*j&={1U֥�% �JZ��О5�f��*�u����z����h���'PR]�3����?��a�0��1<���0NGoTD煮1���Ӂ́�.��Ol�X�մ��
�����J\:���؃�k���:%�G��a�����t�SWf9����t�vc�~z[d�\�H�4�Y���.�?�L�"����ig�L��4C��h`N'�`5D[�������� Yӽ���� 4�f5�܇�۞�M��nLs�9��R�VH������#�Z�,\�&�/������:-�#�r�	:~�MK�8�et7��e$~�9�)�M&�R������I#y�{�GF�s�7���:��J�k����:c<psqg��0�ˈ���uW'�_�들�WT���������gϞ���ŋ      �   �   x���[� E���8 �M����?mK;5�u��y1�0� ��SI�7X��B!�����h���ǆ��}8�	@+�Q��)�¢ba_h���Ϊ�m���ܻ���,W�E�<^h�&�h?�e�x��������#}��h�
l�++ZK�@� C��:f�,���Ƞ�����/�z��W)���fՍ�i��Qiq�e��A�@����.">�v      {   �  x��X�nU^O��>@3�{���h�pE�	���Nl���u�T�t�@���e���߄�ܱ�vfed;������kLFVe2�|"�'U}*�*{q����f*#m�+�LJ�v�%�,ҕm����;*8 k ;/�b��^Y����6��-����q�):��K����:ϔ~$��L/�UB�G�y��{��x:{m���x������TzW�]"�T(]��#iR�:�Cy`hG|�����}��&��iB&���� �q�b�����a���rO'���8�z�`��u��m!�0H�gÓzTK���o6�V*s�"�Z(S(_���y�)1PS�uuS�멜�� �+&�`(���Sq�P�ݓ��J�&��/�U.���U�����;W/��lT�)�:Fd����L�O7�cG��F�rk���	��n`�T�����0��Q鰒��/��G����OS���1����Z�E	@2W۳���Ay��z=K�U�L���r�2���ǒx��"�zyqr�r{�E\�c��7���wҸd��BꜤ�:6���P�,��ZK�J!{�A�t-��ٌk�Ӓ����3%R�Xo��1��Jb�/��dI���Yw�J��Yn\���H>;̾,�x� %��0�Za�m� n$m
�9��K�]�0��<w5�ϬǶ5��'(�����F͊�Zr1�*$����d���P�<�0���7/&$H
�6Zf�FB�8��I�Jg�5���<��,��	ȏ��bn�v:t��4�t6�cR�fv+^����Qh�ɭs��␊4aϑ������б�6�0ۘd�8�f(Z�l0<;����Q�iد�qtV�X|� {�K�Gp�(i�8��S|�yz��1����M*��կh��ww��[LV���7�i���s�(�:V�1��y�|&���xX��O{�s��������*����������|�	&ܺ�/{�C�dnI;��9��P=���/���lv�_�Ě�ϊ���0����p��̥#�Bf\���>^�7���)����W'>��ow��k
T~�4b<�^�^�A:�|�ez��^x���t�g3�i�9��O�����4Z�\]�� ��Ƕ����y�;a<u· ��J�҉f��y��Ra4o���r�N�Z!4�4zx#a����z�fh��x}����.h�;���<ט6byT?lB%X��0ڧE �=��$��n���P����:�Ʉ=;i}3���K���=��Ni�t�cGP�E��<�Xx�o�m!}��5���9<y>�H{d���������g[��ǥ8M�:rO>}3�m�/
����M�xXׯHZK�+ޕX��?�E�b��%0��������`�,�E:�Qv�7�v,p�tS�`�����!6�G�K�7��	���b��fJ������O߲���q�J�f�o�vq~������F�      x      x��[�%7�E�3GQ(C)R� z�уA}��{���HWU�B��:�|��9!r�\[��k־�o�[+U�\��M�T��?��d��:���/�~��_KyC��3�
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
?���c�k�����o�N+?z��hv��~�q/�O��w�H����~-f+'�h�R���7��mjd�I�%��W�� �p��T�I��Cj����1�e�걿ުv&X�w�A�ڡ�2mR8��R6���t�&����Z+�ؘ��XB]O��|mq��/I�9��~�m�G:gRn��\�7��'5�Z�i_�����*r^��Q�s�2%iS�(׾�k�6�k�Z��ff �o��E{xW��=��g�K�Aa��r���	�J��QVQ�Jn�L���-��7�6? �n� ������y����9H��b��W�<�5��v�� �  /���\�/.��pwn�"L�vj� x>���\kW{����G/�����et��-�u_�������?�^���>k�;fz)s��;%�6s�>�j�z���|�狺��������iZ�oɁ��t��g�I��象cmk�Jbj��=�0��~r��v���Es��⹊n���t��ec�b�/�O�%�;�ו+���܎���p5�H*(��{CH1:�-%Sq��r�w/�.�*H�a�1�c�]�?L�ӎh1k$BU�AL�6,<�×��"��ՒoSx��������pԔ{�����^�H��Z�t��cn�tALzvK�v T�^O����A�cpb���QF�=55�	������V>Z�/i�s|����~|���߿�g�_$      |      x������ � �      }      x������ � �      �      x������ � �         �  x����N�@���S��l��˱���tA �� }���+�=�&��Q�*��G�C�F�o�w\���yޅ[�:�-p���2�{L?�G߆8|j ��`�>d��N��������|~\,�	p4�x$�Y�hf���[2����C\��_S��������W�}z�v1�8��;I�Mg�z#��jn��Jm���4wr'|W��iA���?^��)�h��6�h��*F��w��BQL�ݟ�%��9����|�0�`bS���i[������~�n>�s~;��::$������Ar�>]xJI�����'�x��\���;w��h "0���d����r��Zl�J�:�4�Ȉ/�=T�w掌Wi=~��x����눯�u��Y^4M����      �   �  x�͙Kr�0���)r�v�S�!9�7B�U*���xf�	�3����XI����i� ��A�#>!e�:� ���z/�[�8ώ!��A�S�\*ٙ�x7�sѡQ�ix�����X�SDO�j�7H�i����جP�4��!Jb�3i �,�L�ȡbv�w%��љH�!�:�8��8���h0��8a*�|%�@�Y"�K�>�l/���.[�b3xv�9B��CH^
�4a����^�#=o�|ų���;xjX2��+�4�TM�4�$N��+�'�~�Gx�o��D���u6�PH��k����.#5�N��⥪'�y/]�^�;x~��4���9��u	�K��/ln$��U��;���y?V%d����.�=�/4���ε����?����W��������4Wp-hQ��Bs��Z��f�@|�F��������N�K��(TN�y4^��tE�qn��Vm�`��Wk^�yU��U�5�
��{xUh���nּ�B	��v�Q������%>�ݷ�@䥁���^�5=��9r�;ڝcW4G��74��U��i��ch�Հch�}x<��ާ�Ch証���ʛ}���f�樼٧�=��Q4�+<tE�MG��H7�JC]�pW4�OC���,�Ѫ�(����'��{,y�Bf��������+m��ߌ��P^�N��o��ր      �   �   x���M
�0�ur�^ A�����cl6-�x+��P���6�A;�[@BW'Lq)\az;k<�]��`�A��p�Y�����7���l�˩�8sv�y�mOC��5`ߗn<{^����鋉ճ�Z?0=P      �      x������ � �      �      x������ � �      �      x������ � �      �   �  x��TKn9\�O��x_��w�@$[o��d`�q��\^�-y���PKbA�z�*�@,L�>���gP_	�����|ۗجeL���}@IE��0ږ�)EjҢ�e�V�U��-F�r�ڿ�yX�_����~?�|�-��8���$v����ù9 �(�!q�����1Y5�z	��i&I*"� ��m�E#/E�pU�`��m]�%��$ �j��&������jI* �MĻ�lD׀��D����VQ�F̺�(��,ZbE�P�p�_(�] ������T�PU�70��W ���O�sRf`=�ݲ]b��7��ԋ+�6��ܒ��?}	㌶�ʳ8+�Ԭ���`�['�+������ �čڈb��Z�#<�ʽ\:�ӯ_����w�'q�7�qq�֌�6;٣˥�E��n)��ܷ*bQ���C��@��s��������9���*���Z���Y��VJ������F����J��A/X|��Sr~N�� �,1����%V�/G� �y�`Bw_�� ��Q�4�0��1T���(��XWv�-h������/�G{���g������g�� ��~��^A��g��x�*�.jW���+��
 ͥEk�]�6��+F26:�����Y<��ϓ����n܂Δ����)ֽ�!Kyg߼
Sq��T�{�\Bۭ�o�F��V�ue+ۇ-����w�q���4M�-!��      �     x��X�n$��{��?@5�[���I2��& C�!@Iu=��a@�06v��'C�8q�8���D�_3$��ݵH��a�LuM���{�=���J��XZ�IIb��6԰W��櫓�91J6M����4���_����89/�K)7z�=��L�?��!��g��D�8S�OŊR�m��}��:�*'��?N�Л�Yo��!�l^[�����~x��q��B�T'�c��Zy�)㫕�_����i��x:��^�]�9T�x��y���*��Ԁ����x��n���4�{��٧�9e�H�A� >ON�ӄW�#j��pC#���xn<�iT&y�D����"F[�C`(o)�h0���	kS/b��z�C�XU���|{q@�[��r���h���.���N�,m�*bR�{�n�]��h9:��i�ސa'�V܎t��9����P8�u�oƹ���<��q��%/�gx�S<�,]����:H#�RP,�י\Vj! :� �WVBא��}J�FY�[qnZ2�Nڍ#�l��ˋ秗�e��dK��6��t#��ؿgȓ�p�d�W�>7�n3^�����2Kʬ �Â�0|�'p�,%�1��!	�{Ӌ`l�HW�$��5�!���g@,��B�L�)(�#�6�N���:�C�wLg9�b�Ի��AZf��&�gu��r��{%�{��A�o�ïA�!����I<2�����[.d�9�!���ЙБ� �� �7Z��7��a�;40a��|H�����߼xy��E��~�xޞ_����i{���ׯ�۶o�~������W�I����m���H�䥝it3C��J�L�OB"k6�%%sDn�A�BTB�{�K9��(mՉk�'�X%|AW"��
t� #�Z�
�w��Z�K"w��ZM�n��6
)��B	�4*hͷ)�݋�mlN<�}����o/��������x��x�W�:��Ƌ��W�ڵ����nO.��W�B�gr�����P����r@�y�l�
��DD��d��!��O�l��1
ӻ t��R�dQk���k��Ѐ79TqF�A���!�[�ջ��P���e�mV����5e��ѥ�����B%C2%*�A����U�lB٦��@���݊#�eb�Z�s}��kL+�J�P���3S��O���K0�g�I+�5���`���	���saU��`��=l����0i[E�CU(���ޔ�߲���7q��ͨZ��=��Qq��b����i�
s��IaM�-�ҏ�[_���˿�����
5��.fo`3���N�-|��pp5����'\݉{��h�k��L��s���O[l�U�50�<c���(��#`mu�a���t�1���n׋~�e��$��`)B���?�|�+�e�0�&�sB��B&�C@ﯣ�R,��H��+���d=i�7�(D�!t��]o�6j!��#O��&-ܶ9	+��N��;��-��7�U�a�r������l��Ag���ً҄E&m�¨�w������kn�h
�k�$BXʚ�Q�{�m���4�Z@�R_}
�uᒴLY�:c-��V�'Mu1�5�sIȺ�����;���}��v�w ��ދ��^�Z�r����rƬji�n4;7���9.Z|?�&�s$�V��DH���R�!^�Lvu,t
	��6��,;����A�y��RR��\���gZ�s�t������io?N�i�I�[a��A{�C	5A��î��s�A^|���U�t���r��+�CY���Nt�-�-��r���}����Z`����W�gϞ���EI      �   �   x�U�Q� D��.� *�]z�sD��+���:@���@�����K�Ѐ$��*hh�׬A�	���)���܀gx��a�k�wN �̇	Z�<�C={�[��?2���fC��%�m^��������xy]_^��0���cW�X"U]<wV-��l`_,P��M���g?dm�C���Je�'�F��UȾQ��z�}���NL      �   &  x���K�� @��.A�p���9��$J��*͸j�Ol��@�YRJ1%�cA�wx<&y
ж몱�o��:(�=��� #Ay>aeP^��}��`w��.f���6h:�څ�N�M�<����E�x`D��Z���D5�Es8=�Pp�"��!���i��Q�J��$��W2��c��*��.�����XS�(կ(6�wT�D�*�3�u̟����R|
�X��������6{��@g/�D������L�T��\��V��
��G�LPh��]
Q�X�BܲK!���/Q�z	      �   {  x�}�Kj1�ךS���KR�Y� I [o�!�t�	�}��L0' �@?���*91_��f?�n�1j�1h��q2� :@�D�˄�{�ͷ�t��{ ���Ei
�5�c((m�0Sj�.4�\*�ʝC�ʩ��O���t��ܶQ'����:95_��㺝׋%���x�ʳŸ8� OH�ݣ�����
��|���B�A�E��0`�=���1�P���Y{Y7�Ji?6{m?ӵ���T���?M����b���j	����Ќ�lF�alwE\)мS�,��JQ̽#�)��C<��Sj:Q����5�L�Jʇ#z��.�{�v5_���vi8*��C�$q7 Qi̻>���=��3�����kJ��H��xk��S>L���k�      �      x������ � �      �   >   x�35�45��".SSN ��M8��6�"#Cc]C#]cC+0����"ĸb���� �3�      �   �  x�՗ϒ�6���Sp�QT��[-q�ŗ���?(�ž�y�4Lv��0񌝪��B���>zT$��(���<�����������|�Ƨ?���|?|[H��m`Z[>>���n4p��k���=g����W�u��|�!{WZ�[A���H5�蹂_���9n{}��C�5�BrL\֔�4�
��~R}�ǌq��'T�ͦ�?��a�8{�x=D0��p��-�А��g�F�0�%�ĊS=��KS{h..Ts.Pcn����k��qň<����9�m���6"�O���"�c�)BB�����B��q7�%Z�[8i������`�I�˜L�1w�qɮ���$29)�m���u����ӿi�b���-~>$��ސl�&��uu9� �������d��Oh.��^�+0H/�� +#S��ٵ�b��j����U���6��HL����1^��6�!2��H�8��H��;��U��.
6�=SS%v���-�]Iz���R�|�p��M�1���X�hň����R@h������1��dqH7<�3�Đ8Ƌ!�U�"����*���r1g#Z��d6��5
_e���߲�-_r�/?��=���Ūl�*oOF��
/2��Ι:�L���ꎯ��g/ZT�K�R3v���b�P�K�W`m�a�Pi=�y���VK���}�Zp�At���m ��I�XB�˞���b�n��v���pԳ۶Kӈ0�٪���� ӌi-�PL��]:����lR�����h�8amבp�g��".������7��
�0�ڇk�5�#��Z�.�z�����U4[�&qJb)��������h�E�uG�O��_�+]�Z��1G��U���\���?`و�[�5̕���:6D��:T���q:�N���      �   �  x�ŕ��$7Ec�Wt�.H��t?`R'��	6�ց��TU?f�qbc��j���ttyI� jiq��T	x���sƮ4��@	sB�D��C��R�ϻ�G�`G�|}�G]k�2������(U�����1�:��DI���f6M���^��Mh�DX�S[\4�~g��%=�~����3�֍-!�x�_�n�N�&l�ө�/�� U�
k�T���_���J �>�����а�)����V��
)����[����h\�� �@z#Ő�~v�ݣU��%������|1Q{��/���h��?:�/��'-{��fdkA���L`�zw5�BL��=�A]0�'������(8K,e��)��%�DQb��"/�����k����^P���AI�U�2�e���du�1���6��F��1͎��4�� *5ב}��NdP;��>�^
��o�I���k�L�W|tJ�Ix��'�f7�`q�zI��4	
�V�ð'�=���H5�
��
g7��<��]FM�>ԣ�b2<�;���z�����WY��5j���7���jx�iM�{e���H��\�a�7��&Υko�gZ^TK���,�CR� �C���D��J��Ft��-��Z�l�>�+�]���l�c4r�����I��������ȏ�zF��êC2��flջѺ�d�/��1�١���X�&X��x�ɯ�����N��,�E�-��6g~�p>��-�[c�4��S�f��Ó/����n�_??==�,�ؾ      �   
  x�͔;n\1Eky� $EJԔY��4i�� )����8��u G�<P�~���P��'�8��L>���
m4�v�G��3�t9�A��!���Lc�4Ԋ$ʨ#'?c�����)�����H��G�+ի��u���U̡'9|��/���1���R��@������+7;�\s!؈	Ԍ�;�
o�w�Vu����p/��G���W�����B���Ht��6�UJ���т�[n����V.��-��e.��E�S���9��!x���O��H�W|��U����N�����B��t`Ю����n[��KxM�@,�t;$�Z0��>�X1�C\[��dH��d��f&{��e'�d4zyL�ʗ�������D�+ڕ����"{L�4@�� ���j�9q���3}�7�d|��EF�y�3�xo�tC�m-��K�,䝌�_�A��z�.��j�"�m'y"����I~��V�OQ�%�U���RBu�Ʌ�<.����C�榒��i�F�]Ʒ����o�<�      �   )  x���In�0E��)|�$e�:D����M�����+ghS E��������8���@	4QC�<�+�c�xj=��a�%S�x�v���Qw���}�N��m֙�R�o��q�P��4����#*�1de0�@p�P���)J����$iڨLw��v�@i� O�L^�4x��M���qη�x�&�W�@5�8�4H��&����~����N�U�Y�aT$�tL^)��3?O������bp�b}�T
��AMVz(J��h�
��|�b�m�����16��Ц���w<��      �   �  x����r@E���0b�ڗ�H�ؓ�'.�H��Ə���t���ht5�{��(W�P�с���;��Rk8��2+T�<	%�H)=�B�|k��ټ�����ۺ��<�{��{yqanLeB�0ڀ%2Ā|�9H�8f�I�oh���Vy��A\.�4�Y�s6����Vy4������mOӻ�>ȵ��g��8�jA�X�W܆�޵*�U����E���"�n�ֻ�m�����5���x�|��0:f�=ZW9E����ǊP|������M�Cf���"�y-���}_}��,��W�ٗ��S@|���3T��W�9N	Ꚃ6��\�v,	�ވh�#�l+�R��:bt�X��_׏�f��:-?o����-hZ����!%c�R��Њ�@�Z��M������H�� �6$�p��a&[����,��>�2�3�\Z7�n��.�k҇����/���      �      x���;��8��>���IԻX:��{�I$�����Rc#wk�G�g>~��6|���A�\@
�ۂ����d������ؠ�[i<Z����(N�ӊ����=���wACy���.`U�B���s����^�y-+��4̦+<M�g�/�El�V QϠ@r=���"ֲ�{��sw�b�H�A�А�>(~JU��%�A���-�]�\ɾ���e�C��H,5���D���{���(E��:�b���üf=�Hx�����V����5]N��
�q�H�k��
�����"H��>,���}�h� _��Z�z�M��/Q乖EhA�!�r���������>oϙ�J�>`�Y|�y�Ϧ,�3x��=}�٬l��"��% ��N��=�#M�=ԯ*Bu�p��ڃ�����h7H�dy�ߟ�?f�����d�2 ����-N4W�D�G>wΤ6,8cbh�����]�/����I-��\�H�,�{~4う��� ^�Z�#W�.h~:A9e;�(W�	��=ƖP۷�t�0(�
ol�y�}}���D�u�l���mϝTDX�6=k�5d���G2�A�"F�A�0c����j��/-��V�[Z�Y�V ̔�l�\C��4a.���CP.��[����HTxlmh�s+����j&غ���>@����>��Ua��Mq�uC�J�\H�,�&��2������`�������y>ϛbc�F/[X��k�	����m�3�9B�a��d���n��g}>��ք.L      �      x��|ێ$���s�W�( ��(�G�`^�E��������=n���^	�A��쮪ps9i4�(�D��9��kS�����c5�g�A���B�%D_��^b��������>�_�'v^\��������R��r�������_������})���JuDӻ�#�Sr�s���Ɨ�o���?.������_����ڂ������q9qrEWZ��UF��yV�w@�����*����w@�GJ���1(�;ɻ�ġ�����A��\{�7_q�Vu��r"�5�Vcɯ;�x p�#�kNB��$�d����!��/v���H}˵j��g�@.��\3t��OK��d�n�%�r��y��t�1���#�����ר\�T���f*�^e,Y� ���
�rYG���BO��~��	�1p�F/�<���|h$N�*�~�+���M[�p�a�,���=��3,���Bj�'p�>�A�H�力͚��KjX�e�<�ܒws����͟�������#vAL����upDl��$�<Ńw}P΂oť�`/������=t���
&��:qֹ��*�mv\�%��8Z��?]�/�?���� �傞�k>��\�v��.>E�I4��ݷv:����G~���9f|�zL�*�;��6p%$u�G���z&�Q6���|��c���镦~N�G"�4�A2���w_�(m�AοA� ��-ל}J�%_�c�9]2P�+0g�NɌ�����r�}�9@zw���`E�5f��� N1����΍o��>"�1^�� �`�V���|u%��D�=����3U�/�����Mֈ�1a�KV~����ѽ�~B(�{�	�:��Bjq8���+���?F�I��"����j�1�jM#!Y;gSå,��� �3��e�UK���q(��� (F���]"J��|^��n��|H���)�
S�}h��ي�o�6Rg?�Y'�C v��JS�K�2�>5:6.u�����d�ac���p�p���E�<̄z�"x�ܳ�>.$�쳦@|(_�!.n�˥��Z�
ߠ�64R��(��	]��������U��K�ኂ��j2i��e��EzBո���
��}u�H������l~�m�&�*�ۈ��/5?�{Ծ��?�05���2r������ ��_�dF�b͠R���/��Q��v��@�k�ai��@��rM�)��{��� ����Vx�՝ɰ�_"�3ٚ���r%�<�� ff7�e*\�kC�CTRLq�3u5#S�.��X+`� �*�h����_�� �|
wy2��a:��t��6�j`�QcL���.cL�g�/\��Z���d.�M�CG�)�wЉ2�����C��A�������y�PO�f��� �i�ŗ�@o�H|� :4&����:P�[��W�UMi�[�����}�9��7��5����KC+v�Y�X�D�&�-f{����r��/h�W}�>j+]Ca���(�Mgus�qX�ըK��-������с�M��_�yK�i���'�{�a�X���5�y)�:4�N�j����RCЧ�HgzL�/}[Rr�D3!`����@`f��$�W���V��9�]�	�h�y�B���izh�.`w�`�	������ �𸵖8A����W�hD|�����w<�������"]!���_�� i�KLF B_3kx���}��|@�H)~'Š�8b}�VC�!�s���!f ��8E1��Q!�&
�o���\"�^C��R��Ze�rGY4=���#A������BE��7)��@c����e�)�zEl�<�t�M�;��N5/�a��+�iu��Y #�@��t(^���"��y��.��&�.��_��o���cn-�1�-�3��E���oTGz�Q�έ�f�>��p]HOpu%&��<�ݨ>�'|a����
Ř��5�A#�c��<u�z��)yR_L�������O����C�Ì�� z�+ڙ�P�"��w�z�	���jD��"�Q4��5ٖz�/����s���~z�G�;ה��M�&�\A���íY���,����z���Ì&|��*�ieT�e-�|ߺ��	R,<����t3yS��J*\.4�>�k��I���?/d%r4�r�֪�:��RGXqr]�OP��m� }m�7��H�t	��oj��ñ�&'��m}(d��;�h�)�9C�l�{Uir�F�/�@^��]q}��|����3�T�%v�֘�����T��+��Z��;��kr�����BQ�k%��,7��8f�'�*���}�?��'B�B�+(0������f�<RTa� �6�7�;�']R�m���w��}��}����I���D�����?�b����эD����m� �eU�Ekbm�[P��H� 7��QdҙjU,q�7���w랂 8:�����!��4G���Iy������.=uu)dP���Z��|]�l������&gHs���#�+�Y��V}m[H�`L��w�N��	����v���_~|�E.��b��'�҃O�=�^�S�1����Z�љ}���Zϼ�`)�}SX���jl�4 ��F��(��\�&�n|��.��J��a���U �aP�e�m���,:�|��h��B`����s�J�E���d}��KůQ[�O�6ADoLy���Ls��㫀�!-��yӍ�il����G���B�h��z��U:*�DՈ%3
F�m�?�1�z�/Vi����7�ź�h�l(QS8�(b�D����%UN��rz��A��Ԓ�֢��j�T�x�+r��z��=Y�2�U�wU^��`��b�H�:�2gN3T���N4= �.=��T�=(V��`!�$Xx x��\{��g��/�Ά\v�LM��v�}�Շ)�j��L�n�|��6�Z���%�臧RH&?���q�/���������?�\�)ݳ����Oy���V���(5vd
��պ'?悼��c!PmM#�
�2zm�����P�����~@�������w�c�G��ϵ>l�O�p���'�O->�:g� <m�sS�1ZF�#��c7��}�gT�������r�$�B�c�wK�`NV\Xks�:� �Lc��
g?����un~7����5C0�(GIk\�3F����a��h�Y����8`K&�mR�6 C��*��t1�K�QLТ���������,��ge"7MT��Q���~ �r��V�R�`aY#�h���7��7U��J4i���7�KH��n�s����K4���i���oU���\���g��s	�<��s���= �C�@�@���d��"������($z��~8��A�ϵx�(���P�2E�l*k*$r,%5)�'?bg<�k���EJ ��Q:B�z��^���J����H�KDM��>>��o=? >#�_�� �伊=j�B1#_'����|��[z���BUarj�.���:P��3�Z"�v��m��Y���[�ۖTN��m���3ˮ��`�T\��#�~��@�	}�,�S�xK��L�چG�{���9gG���p�jڋ3h�:A�cB�!g��q��в����d}Hz�QEm:#�i���I�F_�#�qaz��'Hc��l3b�ޢgcy|-{Ya>#
�Ԁ`�MM6~8�Ԛ1��z����l�/�5�����tUf(�]��N�s� �A��=���n���|b��F��:�+]*�g�&'s�R�K=��� �[��Qb���أ�.���i�a%�2�/�z.�;�먹C��3_�q��@�!_u��5�:��7"T��l\��`��7����<X�;*��$�.3��Y��q�+��zg����X�J�R��xx��w���2�ުԤ���d�s�3��m/	�I5#��T�쐙t�[ʲu�s�'u%{sT�P��?�ү��_ zH۝�~��M�P0�2O��_��穉r46q�/y{�(Q�'��&�ed�X��<H��Ca%�    �3��/��7a���>�O6�m��_ڮ<d�x7B��<g�I��g��<1$[����8�&h��+�y�ҥ!��|����|�Ʉ������{��"�[	����d�/7U�&��#^`�@J��Y}ˀ��;�;�f�� ���-++0PCy`��cG�x�yY�W V��A��<F~�6_Q�3lܧ�}-�P��ڢX��n��9�h��	�	L)�jd�<qg+�y�E�����:.��2@@?�У��'y��O��κ�^儺c}��쑵�~
9] &G��qy@�Y���JMl;2�z-<
��^=>�!����Gi�}7��"{Gb���idPgÿW͡���o�Qq���â<�h�3*����@Kв�<�ɶ;���:�����ao�Lq������@n���qԴ��
�A��*��٦W@f�Ɂ�X�ۛ@͡@M?������m�×+�vf_+N��9�3���.2��0B\�(j;�xM�7�ﳔD/�s!�)�s2%��#�6�:<7��S�e�5����>������2����d���+�TA�k���h��Ķ'���>`�|����P��͔0B�M���qy��� �P�%�aF�l�X�ꛃ�Vg���� �j��\"p+�nz �ݯ�p�6�5+�p�
�a�n-';Rj¿�.-K?��N��� ���)�2��t73�ч`B���Ay�~�[��}!�!aD]f8�
���fFI��{P��
m����&��v��Cl;�L`m����3���TE0�bn�!�"���6s6�;���퐫�?���$�����9䝄�&c*2�,!�aZ��D��Ӈ81����Ͻ�}z�?�^l�?f���U�PuKjrVQ�!8fm�G�X;���t�R^�:����Y͎m�ɿe ���4�v;2ِ[��l��U��2�X�R0jQ�?�����K��)J�pYi �Kqkx�X��g�b�8�4�6���"!�ANۼG�m�p�w������z66��6|_�c�>�<D���/�%A�l��mp�E#<��t-�I�B^�(�ow�|��g0��/f��&�dִ1�A/�؅n*�4i�������6��dg�Cv̑�N�`G-z���i�o��r�H��Z)�
��Ίv�~i�z���TF^>�8$T�teG�d�*�����f�ߙ$�cѳ��H�sz~��z�Z��	�]�jCQ�폕�����-
���}N	=����߱�K�urHۂF��j�)��e'gS߷g��E����Ϯ�Q*
e\4�j1�5�AZQTD���?���旕V�d(�
;;k��3&kN�+(U���+�(��*c�)F�}���'fN)�(Cvx�BQ?��e
��~��x�4�P�z2������2�K�Yg͝6��F�D~�ķe���=�̴���������Ѥ�r�֚%�Tf��Δ��g�}��n+����QAčw���X�+J��?DɆ�{�Ȼ,���q�n�*�� A�� �o]��(�h�E��M?5;��� (`���c��y�/sGSoP�FJ'�����6*���<� �y5�Nx3ݒ�g � ֆ�E5�2kU��F1�������~@� b]�ௌ��v�M��+�"@\�-�kQ�cx��ϙ�/��>F�+!Mv��޳� ��u���Q�+wn��ߐ��,��l�،ԄV+61F����'rV���T���r �N*����DlWvuގ���L��۾��o�u���$�%Be�l�5�P����D^N���0�3��~����/�-�YoNt�lK��Pv�yA1���#Ϡ�4@�X�dG��F�u(.��Z��L��*�|(�W��$���(i�=�����*����5҉�&���5�?s���l#	%'��
�y�b�0v5�&bNl�1�T�.:��-bP>��F�'t2b�"b��%v�L\�����ɢ�&m���|s�mW�l�@S�YN��%q���4Y ����E���;M{y8#�7�r���mۻo��A��y����h
�L������GWuy�fiH_jT�����\<�A`�!��Q��ʄ�_�q�c�<��y��PA��6!���f%�Q�+�Dz�5���h��e���R�l�h0�U_�0V�o}�6�B��rj��D,�i?�o��ш��M`��v�`zҪ�xH�N�����`��}<U^l���W��kx1ğ�f��/�Q��R_'�9r���U�n�(6��+�=�F�c�7��ņ���*}J�#�8��2L`C������m0��2��h%�o�w#|Y��])�W�KۏkE�i��$���}����!j+��p]�����<O)͍oe��d�l����ĈC�EK݆��/�!nP
Й#p/���E���Ɗ�{�PjĶ����|6}���vPL�5�P��s^=�8��)�5�I�	�6[+�ư�B�R��UW���x�9:���-�}�����m���v�S�BIJ9ϵ�Ԫ�hNQT����h��cฑ�ӎ�,X���9�gf>���Q�?�����Ol�8b��f�츕�����G<gm�_������@nͫh?��%+bX�h}�i����uQ�]�)y�'i�B���Ҳ�e�w+�1ŶC]�ܐ����r�݂��y��f���J�͵~:��*���>m3��1��%D#�6�}�R.`���2��l�G�W��Pd�#������ߐ���f�6�����!�Gu]@�1�ہ\O�����=(���o�v�N�9� 6����y����\� ���גE��-t1�?lC�S��X� +��=T��bd/�ǎ
t���^V����	Ɛ���(t9�+I�֦�g�l�Q�q�2��Pm�*�Qh/�Xa~�AG�a��`M&��,��6(��Ee�TB�k<���r02�s�O�A霮����!�8G�X�^�`��k���әWU��R�`L���S���Gu]���犋W�8�Ɗ�=n�,vd9��M�a9f���ؠq�N�1wo���������)�q���w¹�l�<�A�n����<���[l\�����Z����t�ly�����ŎQ!��fl�<]�C�4"�V �*����U�m�Д"�m�gK(��$�T��_� 폯[6����&k����;m��&����^JN�âK6��DY�s9����֘3=�������;�q{�(�cG�-��A�a����p����Z�z��m�Zr�D�#?��s�@��ۣ�\�9J(��9�i�d�+�q�O���"�-\F�W<U�o#�Y�s��bg�Q<˶e:��U�ΐf���9W�A�?����4k��ہ�k�i;b�3���s�V�&��m��$����8�k�v�wA������ޑj/B̡�5{5,ѣ{h����"Oۋ��~wk:ۄ��m��pL��>ޜbO(z�L�$�H�9P�w�b�z(5!�������C��<r�M+T�O����6��Y�m��lx�ivh��*t8�1|(�
g�^A�%�ӓ�C���+R�܎Q<^���d���j���"d�D�@�ֲ��C���gq�*���*Lq5�o�UA�k�=��e�_��$������?��q8���
"�d�mSrdW9�y�p{�C)'�9��2z)����n}$Dyc��	�̀�r���'�E���\)�g�5m�������dr�M��2uq�����N����m�Ω�"���Lu�g~�������x������(<�F�����-;�c��vnLzڟ)C�.v�<٬F!E��~ʮ�5X�2b{���7����a��E�0�6%de/�����Z��r��BI[}�UM��n�F���p4�I�xC��C>���Pٱ����kν�d�o�fl�Ң�p�BM�o�|*X�eT�1ۓ�aݽHŵ�'�������D.)"��v?���7��.�DԮ]�B�SBN(�OT{�хj�#�q�����WP)+���y��?TkIo�y �C����Mܐ�m���9̅w�fw���_Zvl5,{��� /  #�1��zf�vy�@�f��נ0�9��/����z!��Y�G	�-ѶRP@�h���(��$��mbM���e�A�0�����7�d;��F��`mvh/�VT;��+Hj���r,����i��ZyO{�不
�[�w,��D,9�T��-�U�THb�k�k���u�	)��������2����,���e�r�����|�>�}0ط]ʪI4�M�vg���z;��l$���l���};pt�->즲�N�ݠ�}��c-���Y�n�w�X��r���l�+@		������׿����;��=      �   K  x����nA�ϳO�@�m��1
���K�E� B	�����Lm,E9�x���ݮ6��I�L2��?���TRJ�)M��݇�o�=/��OZ��r����q���������I��??<��Ԗ�Hu����@���TR%�ywKX��
i�H�r�f�D�!�'i�I��YA���а�`��"mi�Z"����miI�jD
^�"�*��J ]%�K/�����
��q���@�ގ~�B�z�ޏ�%����Km�_2��@R� �`�#I�r.�I�z�剬��k��*���{wx
��w�P�v"m�RQQ�E�Y,�oD���� �`�5_�Q�k��^#R2���V+��h�~j 6�^�Ph 6��u`�p���J�IH\4Yr�3�ZG����@�#�
m	hM7�Z�yw�s֟��+��Y�1e)�����+s��{�)3��Q6���m�4&E�,G��?��ӔՐ��#�����+�ծ �yd��C�@�Ӻ�YS e٥,K�eS��e�3�+V$-��5D���սL�4Sd-�H�*Җ�,5W�\�[m��[�ܫ�(��J��0�z���a(����1it%��@���ֈ܊��G�>|� ��	���Jd6kG��@�}ka�2;����~r��3 z������Y�#�K�=ouxP��VG�mF�z�#RЫ���(������1� �E�T�z�S��_�[��^�ļٹ�����ļ�)��F������zs&�� �6ES ��@Tx�e���)	�L<������-zF�|,�!���А2��N�м��)��.�yO��b��b��� �mE��r�]Qo,摚i��1z����<�p,ꭅ0'd�,�E�ʴ����(D�@���6h%����*�D: ]��>��X��v���,ʭ����b8�΅o��~ޠ��g�ž��k���=���5��}��<����N��|�� J��a��xnJ�Jl�����MZho�&~���� 7�柯��Y�p��1�M����tf���K�r�>��7?Y�����������_Wo��~�?=�a��Oo�_�(��n>�w-���w׻��i�_      �   �  x���;��0�Z��"�g�&� ������/�뙝�tNG�>��E��ik�
Β�1F&�7�����¦���:�xIWƒ���3ҭ��}�� j"�r)���ZDvalf��L,-��{-����*�wem?��3��~K�r��ɫ|(���h,�����1�P,�b��z\z+��ͼ�ʹ���*�Q�]A@e���*����c�J���0ߙ�Z8%���YGVeȤ�2_㳗2�����P��j �2(�H�g��Q-Z�9�g�I����Q$a4�lEC�����%�6D@�ԁ�㞅l��f���(G~#��j���^�{�o�?�l1�5̪�1�Mf�OfmͲ��uj+�����%����/&����3z"y���<{{��aU���m؍����F�;����,�;�N�%      ~   Y  x�m�Krd�E��^XA$@n�+�	�������Pv>ugV�*C
���J������Ɯ��-��V������5�:4�����M)&۾�����O���<�T��Z�'����2B��Co��9V���V@+oh*YL\Ʈ��M�JZ��EI!�0j-a�Yr��NT����JM^'�_�������%j��B]�y�W��o����J-G�$ٗ���M����
gq�<����]b��ֿ��7�,Qr)K�Ly�����|��쑓N������L�����J�J��X�$-8^m,p�J���巶���sU��вE�<�s�c�V>��]Zl=h��V�d�Z��k�Ë�q��e]��P�g�ɵ/��A˿A�T�=��B��9j,al�xk۶�Q�V�M�Z�Zʹ�����^�m�[�&0D�r4�P|jO��o�����j��B�}v]���j�&L!�zO�������͸�|����S�S�vk��2�Ѻk�r��o���cCu�^hk�q������#��G�����@{��E��C`���i);U�aI��Wu�#l:Y0�_)~j��Q�p���^h��ҥ���oSzh��h;���f�}jAR��K|m���*���3�v�(k0)�x�ʂ޿A+�Jն��Ԧ�G�q�����3�6yr,�S�ZN�-9��b!Ǡ�.��fj�ªe1����Z���$���h��-��P������6�u&�oz�>����3�}4)3��J�0�ۼ�A����D�S�]},����Y�bp�B.(�RA�8�q��O-���#ԟ���q�����4+�
�k��F�>��Y.�fj�='�j�B�iZ�Cn�6N^<M���&��5D��)OߺI&�J�eL�^�F>\�y:���;{e���^Ө�)�Y�Z���t�H1����x�,yg/hw�$y-4�,ƋV��h �A�y�n�;{9Q�5�3�+O�u�4��.��iި����BV��+h��%��RXk��=n饶(��h�^>��ဣέ��x 흽NmM�3��㖧�x��~g�م�]B�o6���흽�݄A�����ҏՃ����$�e�`���6��)�V��<��G����7ܲvj���b�*���@��;	����Σ��jЭ��f�y]9P`�sF�y��E�L������4%�� 虔Q��&a}2X��=D�s�-��W�m�	+5��9d�0q�,�o-�we}�)���e���b�������m!�Sl�������,В)�$�2�8G4��%T�wBͺ�e)�-}���T�0ƣ�t��H��|���&k�1�j��1f�ޕՄ�Y���4��:=��C�EHgG�5	�0�v%���]Y�I�:Ɗk���0[�q��TXb����Si��{�we]�VdJ,B{o�A,�o��FNJNwM��a�<�we5)l��9�S[g�o�L{�fhA�6I�sm��iZ�9d�Ktu������(ǷN�Ik����D<�V?����v{|T_#m������e��f�����������maI�u7x�|;��~R�qt��W���F��F�L_?{���o�}��A|�� ��E{��-�,a��8kCx����{#Y��9�<k��Mߴ@Pr�+v6�����T��ՆN#���&G��>�u։��^͟h��M�т@��il�S(�e[�6�q�*} �o�.	d~�'�!!
/�:�Ć���ƹ��4k���)��sޛݳje*����Y�$�P�F~�7م����f���(�k�e����7f6����QC�+������P�\j#yM�+�4M�ey��LW������ǯ����B]��      �      x���[r$�ne��B�c����$Zf��?�l�$]}�쵐dd�*3ɨ>v�d�*z"����px8qGp�fJ͸������F���!ﶗ��6��$f[�CI��j�������K�_������?�_���|���MFBڦ�5��y��4������,���?�sG�a�����<+�jZ��q�[L+�a�8��t�Q~�Q�XI�7�J6�Hc�����w�a�l�w�(k���2c�dd�`j��d�˞{��ژxk(�mL<���6����y����͙2�6�G2U�0���e����H��Ik��Pӝ]�xSK,f���Z��'�J:�x��L߽�nq��X�Ms�Ϫ!���+�%�f�(������ń������ˆ[H*W�܉��2��m"?_{ڣ�4J�`f��]lg�o�`dq��7Y�&�J(>��b���73w���XMgs�$����f��m�S���M��b�1%�E
���f��Ɗ��i�s�߸jc]�ƾ��5 QH�8�	� 1��y�F��Z�/!�%{�GF1%�fJٮ쵽+�����������������k�wgf�<ɠ�/:_|`��t.��3�#�}>,��Q��%Y"�zS#��fu�O)��3�cȾ���f:�ټ���UqE��ڍ�	@&!��W2=�c�8�n֤�����6�J6�-c��{__��}/��
�$��o5����&���P9V��<�ʃr��mҰ��jJ�43�����S�xOzN�h&�r3�e㏍�j>�xa�]���%�lv�^��P$�61�RZ�Ը�Hso󟤹'G\�b(�
�Va~�j�����+i�!y��~�;�����������r�im���"1��͔�\�K
�饛"�uuJ� P%���sݯ����(�/e8�&G���0���M��7�e�]K�W���&r���5r�H�l״u�R$a�>&�����<Ň����X����f�nxg"9"j&�m֨+��Vɯp�)���cU�;]�Ł�@o�\^�s��-V޳��͜��9�3In�Ĵa�!�K%+v[�J
�#o�:��G��$ɀ����y`��1�A	�Tf1{eH�s�����t-��1�[{6�l�����c�Ԗú�(Ѧ�P��^ �N$�;��Z&c��"��p<��vM�J���q�bol�!S�yg�N���jH�s�9�s��	L��&�O��k腥�_��-�uC�)�m���Kk��&������N	%A���čC`�<}
cwY�Ҧ��� �˂�W�E���2�-EKnv�z7/���H�1��3��N���AI�'D�*a�ifކ+��ӣ��ie�9����j����Q%��\��tl�D�h�	"�XԻ7�(��-ӷ3>#
�}9�=2��$(h����Fl(5��R㫽`���@����2�J��J���哕H�M��p$�{Z�=�bV$�`�]%;��Ps�zGf��=`~�74�rn	�B�Zp�-��mG�y̻�	T�=��D`$,�R�D@5a��
�}�H"�Ţ�4��Rw���E���-�� H�C�\�(�5
%r���Q5d��w��,(=�Td4��F�Z�Hx[��\�O�4���2�,J0e���=UVܰ}L��"�}8(�P:"�u����ZJ^����>E~ý6�e�RQ`B((�t�o-���:Qe^�3mm�cc���_�i����y�c��@$	��X(|�ይ.�m��R�";�ݟ&���/uJQ��ǩ��f�C^M���O&k��@XmK3#k�l˚ڨ�.�-[GV�|��0�6P_�K U*3�囊©]$�(�$>Mi�X'��r�Rh�xK� �AP��r2CK���L+!�ɰwn��|nM2�fW@�+��"���0j���g3���ʬ���{�G1��'e�O���!�fA7+�L�u��q!X�Qm����r}�qO8�b߫����z�Zh_K��(��r�'TN��t�)L�Я���3����V��4���n������X��T��(*�j[k!�1�.�|�=C�E)]���z��:�4��O��V�V��>�Ro�B��p��u���N�~p)�dW���]���@-twi�eE^��>q���U9vj9u���=�F+([���c4���W]	ot�+��=y�s�u�o��k!�Z�f����{�:<�j�]�0
�יzM=�Y�c����F&$�խ�aՅ�4K(�ώ�(�]0,ܡǕ��'�ϭh��G�ǶX{�F���$>�bb�?�:S�3`s"
����6�l�D���g������GCAo�ג������������Q��<^��:pB��פ 1A5�>z9���꧕Z���x4i��¯�'�,�]�̇[�2��۴��@�\T|�H��֊��5<��]���x�lD� �vҨ:�$E��碜����ܻ?��R��r[;l����a�1��G��@T���(��Z��m��>X��'�%/?������'W��h{���W�ɰ?��R���#��R1���Y�b���s��*2�F���F��\a_������LλX�QjWz�ޖ#6�Y���"��`�-e(���|��`]�	:���F���U�����Y�X�2�47���fym�HwT����.�H	*��������IBxA�1R��٤I����n�h%3%8�ޕDw�u)���ej����H�<A��]OC�2�m�<[	O��}7��ĥ�[SG�X�����w��2uR�Q�ƅ�mn���@1^We��>���i?z��%�/����ߍ�{�AjC�����G#J:
�ͽ�+�2w���p������L!� 1ա�n�[As��f�=���R>4u����|xd��3Լe��y��)h�9R^2��������H� ��a�g��A4�Z��V���h�C{^GƳ�XU������槪�qס.h�\���u&�g���[�D��6����y&�n�3~�f�(��B߅I+�o�4%EO�7w��z�+��������+%-{`�(+�b���S}J�PE���T{vh[KVC8������$�����l���ާT<�(�5��D�6jkq�&a�h�l}�[oc�C򗕲��{kW�����Z���
�(��Ry��ˈ=?G�j4�h�9���H/�e���~�Gs%��kmD�}����UO 4T���P4/@�[�cuU�W���ú�h���ėצ���X�9䠆~=�I�S$��"�J&M=eңu�pp!6��7�a�g���� �Ψ�f���Ti@���Y�T4��"Չ�k	��3d`.�P3�[�~K�2bX��rŵE�bC@5���cJ/��h�u�}\��ԉx9f�+���+E�'�:u�f����<�}}icx���*Ҭ�	�ɏ/���NC�ٯ�Y�_�;{��Є�M�*13�ֲi���Z������Q.��'��������p$B7���8�(�9M���� !*�CpٿP��V`��W��u2��.Qs�
�V9�����X����,:h0V����4�!�D.,�sY/���xa�9-�g�1uO��p^vb��A�ڶ�ܔ r8C�v!�2��w�:���U�"��"�Vn�vr���B)9�N���Ѭ\��uA�����|�h�u��2�X~P�']�\�
�"�VMe3P�z�����`Ea^x��Z�C��t�A�V(�����(��J෮��U�{J�R?����=o}u�B�0����44!����ɵ�L��oF����w�
4����3��U�55�؂��.����H�3_�VB��X��2xoɤ ���f[�"���]�f�<��+[���R��gO��+�T�Mg��g�d�#}�+����d��S���,��?R�c�u��_��@�P#�f���i�R��N�j����N )���U=����ik�U S�QU�����*���k=�u��*�*K��~Ʋ�ͯ ���O7�c�X�tH%G�ˤ�*>�|�oD�]�����9�W  �hIGn�W�l�u=6'Ĝxg;��R<��=�0�e�PzI�Bנ@s�9����Ar����ş�T���3� :  �	��n׍���j�S�+���aV��Cí����{��UB{�@
�zzũُ�C�E"Vf֙2��G�KVֈ���Jȫ��u=F�\�	����Vtݻ����>Ψ�+��ft�c��.-=D~�T7�%,ȤL):�Y��#(�mPu�2ԃ�}{T*w�!�o�.�� �rl�yl"��l���#,h��s�]��ן����E<!T�eR��	x.Ҧ;ې����IV,�p=�Ufb�ގԶ��ͦ�g�)��^�*rL�D�l�7���z��J`��[=�]�ȩ���iiTU���o�7h]2J�~��Μ���"��$���_�fKGM-�_2�A�-��H��:���[�,A`���hҵ�2��c3�������z�s3Q�b�oS?0�Q�yEI8"��B��}�.hx�������:�UQګ�*�G"|�jp:z�IvUӺ��g\�XiF%�q��hR�.�
� ��=2ch��^?��Y�{ K�����+A�Z<��c��0-F�ܛۻK������'�/z��!wv6�G�$8A+�+r?�uI�����	x3K;�"��<$8��}�L~��ԛ�C�������a����RJOT��a�^) �\�I��n���	�]n|�6�5k���^�����A{�JYv�%�u,���e�+��C���>Z݈��|8r�V�VY��{�"J�oK�7"_w�9�Q��	z���&\g�rF�Gvl�.(��Jz��u�be��T�R�q�������/Gԯ�i�
�:�v��Ai�O�h
2w�h��zuDmo#��:�`I�)�eM����	���#C]��Rx|��`��:�s��RKZ���R��BP�2���?M�umC�븜�*o�efM 3�_�T��&�dڹ���h��d��T�洳�@^����^!�����g��'��չ7Q�,���#j�%��ʔT��Y��tzdk7h;e�2��՞�Q�7nԢ'Dڬg�:{�NJ�.�9��`_�\~��`�u^p
R֣������2dڕ���r�gԆ_�V����@�Ť���Y
�$�\�ޏZCѻ%oF�^d�ϳ�zL��է�@ؚ���=(�W�jf#�Z�q��S ��4ei9�Es�G� ��ed��������'T{�`s-���)x�W�mHA�A!Ъ}æl�����K����ܗ~	Q��Wq��':��#�%��N&ݒ�Wݔ�������7?�ޣ)]oX �:�V�����yv*�ԗ�[�?��jlU2�6�i=kRp+x5�y%�NLL��~�#�b�ޔ�(i�d��4��nx"a��VrI�z���wS��y�EJ��wAJ;�P@� �t�T�w����v^&/�u�a5�=d0��)g�����Wy?�$d5C��`+��I,�[P$����Z�k�o(u��0`R�#�BN�>����_3�i�9��G~is.���]g���!�]��X�n��� ��7������RB��8H�|���!w@�sF���?'��@,g�s�5�-�qs�.טW&B�=����	rw��׻ͥ`�Q�G����h�;�H���� ������6�(�%��C��������ez���f��^�e"�f�ێB�����F�<�<���TKh�I�Cv�֘y���O���X�LUI��T/Y[�:9���.�V�P������D�TO�A�ꁚ6�Ā&Jr=]����:J֬�K��۳�&dIh�>D�b�7Þ�)�.-�ڊ�z���´E�wB����|l�)�Е9� � ����fi������O���S�β�m<���I�fS�@������㞨(���e�G��<~�k�-)T��pi�~�%�Z����¢�
}S��X���L~�"sB�?$"�'4qU�ǝ���+߭H=Օ���n���I~�{��}�Q�z���M�1���ۗWe�O�x֧c�=����:���9��P�v�eq���hC���J��Z�gb�4�l�9�B���2(b�ʧ��kp I�,\;�^� ��]�qw������.A*{����� ��Yն�qV�JBQ�r=ۻ��>���A�l���;u((�e?���(tR����1�!Z��)��nt��%�E/��Z�H�+E��߿�V:���� mp(��f�B:up�u^�h���<O)/�p'� ��������Q�VJ ���$n�ȡ�]�N�����-���-�d{�����ۃ�C������ө8��t*|s0����e�@��$�s&�d�fj��QƮ����Q/���媙s�pF��u�g�X�s0��r�;�KU*Rvy���
i�0j�@���B�v�3������B�vg���z(]<F7
>Hנ4�#�>�O��$����e���jz�z�:�����W/���D��Uπ�P�`�Ǯ���pr8r�<�Y��p�ԡl(�9��k���� ���@���22�$F��3Q�zm�Z}� ���K�T�������0�ƳT��B�k�Њ\�M��������9)Y���ͦZ����4�Q�%ݤ�I:��zL��ܕ��z��c�f�d����ȶ*�rAD�th.��-s�X}<b9����ΏF;���?�����ߕ��o�u<	G,s�����o<���B��I�idBn�:�#C�G/���W��Z;�I���7�$�s<�D>/��G����Y���R�.�4�����T<"k�l��\XS�0��Qw�͇��y�o�{��� .�&%��N���2C"��������g@U����4����X��<���΍���o����$�S�sz%E�������B���'��4��4�E�u�6H�s���q�;��6R�ad���(گ[9�	m�xY���G��.�F+9�-
]O�P�v�' ���\uJq`����� ��S��{FE��}Z<AS�.��$G)>�U�!�y�=�˵�����>S�}��
�Nlbh~�-��l��AĎ�3�ٸ4�,(N��ܓ7�1[L�Tx��6����=���8�����'#�h��F_�B\V!� =t	�]�D)�����zu��q-}��)mB&3��HԖ}�<�F��p*%��P�����`K¬.�t�
\�+��K8���Z:B��yT��v?�� Wx=���!�����f�^0�UoE!�z�r�K�S��<�M_���ݳΤ��+Q����P�i�����u;���*:@�%��$C�t���X���;�1jl���R'j���fWYu��\�=ϥ��{;�A�΅�j$��s�K��U�>�ڝ��sKho
���?tj�)�tn�e,\���/�.��Rf�h�Sg2윦���F-u�K��=�0�g���z#[��@j������"����JK]�zkիV��@���7�R*����9��%�[��}�Ԉ��:v�_xȭC��~�G\~����K{�ۅ���ѝ���/�-i�4EIJt����3�����bzv�6 m"�CHN_�qkRL3nJc
.F�k��6ܗ�ďw�L I�դ�䟃֑VF����/]���������)�1M�zn�^��"��#*�ooh���T��
,�A��|���(����.�)D��_�8�#���v�8�I��v�vNޟ�z=�O���Fr����(:PG��!]��`��jW�D����eF����~�0�]�[�.>Y�l߽�腍����h����I�s�PݱOY!`]i}d�1"������	J�H�1p�׎��@VǱ�҇��ɹ�̌�c
af};A�)A�{}�V_�/�<A*�7��#0���ͫV��X�'S�ig��ޒ�g#[�K5��������>璚�{_��TG*D�V�FCߣ'ze�f$|o�q��|��i�FV��r&z-�:��V�N�K�	�yy��w#��_���?��]�      �   �  x��Xˎ�<S_�`���$�u����À/|��
�������9�]ݖFZ��������變�̌�hM�H]��j�)����k��[$��u�k}��+�:�������ݪrҒ�G"Jl�ɂ{��w�6�>�T��cg�n��8�����[���3��(n�㍚#fR�$��d] 1M_dT_-�<���V(K{6_�S_�uQ�l%gS�"3g�,��% ���s��n�z�H��G�[�S�5�cL�7����Qy`���t���U���X\�R-��j�q��F�q��z5����H�ӧQ�tF�c\��$ǘ��$=�RA#_Wd��7����tC��/q�n1�B5�f�rm�hT	D!�e����.����[*'��|(p�ĴdqMf˽O��PJA)Ko���9,~�*�[
���D�%Q�3ޞѕ!*

SD'�

k[��	�H5	�� $3���y�5�6K�d��g��Ԋ��|���N�LvæN��0ьv
!�k\28�ɅAL��	�8�zK�\�ڍJe�kZsJ�T��I"@fF[d[m&�eыV�{+����?���!=O/F>�T�H� 
 W$�>&���B�K�D�������kB�]U�0�)�T9��1]���+w��{RF���:�'�C�,m����Rb3�i)f[�O���c�۸�R�zҽ+�Z���Q����'(��s����zC����k�MLF 0���
�Y����υ�4���20:cEkc��j{2�CH���6�Xt���%�zp �lg�cJ�!�b$�+oj����Дپ����S�H�J�B��wF+:u�d�=�iԙ����J�����g4 =��>��7�I���*����c��Xz�R>^uv~m���rp�)_����K�=��kJX"���-J	.)�Q����iD�=�X~18n�π�4��(��-�M�y��~d$֟yc�DpXkh�c�b���S��ز�;*'_�^'S�Cޛ$���*���Yg��D�B��/��AbI��d+6����8w�I� 1ƻ�q~���ߟt)�)��O�ŨGI���ǂ'��L�L�腷N�� ��ا��m�24h��k���C��$��!AIa�^��;�?�����?�x+�d������ʨ�3f�Vk]��} �F��(8y�WT����nF��ӑ2��ՌE�/"�LHw\>��S����3�9u�A������K?����wb��"�%7S]2{�Hh���6�{3+�g}�>��G�[�#t������52
=78�<��q{����fK���oo� /��}����7x[l�:���j�:¹j�w�m�2�6�[pS�%.w�@R)��R�8�hBʳt����62��v)]f!;��դ���՚�z��̐�&�~4OA��b%2l�°e/�9����#�o������_����Ѕ����IF�%�YyH�v�S �N��Z��0�"Ee6$?��,ȑd�!�7�[����@C
x��"!l�}��js�(09g����H�qE����3��w[�R-Û��b�#�X:9�:ѮcPlV7(U����� ]��	G��[O�_ŵ���؈��;�X����
�LEW`�zE��C�%F�d!����F�K��Bx$�Q�i�T����=�0_�)e��Q��t`��n](!��Ҹ�Oe�$��G��j@F{���y�	?�������>���	g��/Æ�dܶF�K�`v_��ధ���PcTWl�����'���3V/s"Q�;��G��0-���~�[�1��ɟ7�peJ�����$������R��q�����.�B�蕖��?bQ���r��^���m����A��<�=�3B�Ck���,'��MJpPz�X�����f���!!��]�2����?��s,_%��n_CE��c\�\~�OǛ7o�kx�      �   M   x�3���4600�30�4���F�%`��	(�42K8��uLt-��*f�kh�K�T2;Ə+F��� �Q      �   �	  x���]�[;���U���Q")ћ`���w�K�Rw�i�}(�A�HN�_�dUQN�o��9BW�g�F���kiz�QC.��Rm���� r���rv~(͠�Z�7�^���$r#���߂��:/�<|z��%���X���o�����)n��க�˝�1�Um]��"��L[4=Qdћ�b�_D���{LQ��d������P�O��=<���<���S�v�%;��(��M����G7|���>�?||ݓD�tq4yE)��P�G(��fn��SW�4�%�k���t���'	{(]8�"ō�(-�:�p�LkYQ�(DI�G	��к���=�����}:�c��^RQu!����X]��SHS8���q�������I^�'��)"��C�_>���N�ŌbwmZ�<��j0���4S�l���x5��>bxp��%~H�O����=)��@�
��d�:tKsES"�Ĕ����&o0h`��ݢ�|̶<�l����%��65Kj�P�Sd��PW<d�
�g��J��EU&����6��i�*�l�)�PBkh��1��ѭI�0��Y���p��5ݏ wc��Nx��4�:=��B�8�+g���Lt�B�?���'?�N�y9ޔ���1ssș.{�]�n<�h'9�Ǔ���xp>���T/yd�|ZFhQCf�%#ך�I�`>�g��Hʟ^x'�G��Iw�@�M�����}�W	�5΋ʦ���+˻�8Â3��ܴs�6O��P-��w��V�Ďj��'�M��឵;8.�]���	M.6|���y�7���!�{�b�l�?I6���I�o�`�otӫ���|
�n
`��)QV�)ӕ��a�f�k�e�$��sE"��x���V#ݍZ��r]���"�M�!�1�\��8rtUKp�3ϴ`���v	z��X]�D��e>�ۧ'-ޜ�ZU��0ݣG�~buHܡ82�d��\�)�=�a,��(*�{K,�cp�
j��^����զrԊ>L�P8�=y�1�wp��`��Ɠ3���
2��N��_:K�DQ�أ�ҷ��u0k��{��g�	��27E�j�;��xH�88zl8��S�I>{���,��)�a���ͼ5cl�YX�e���x��nT�2X�OK`RI���
[��WB�C�]j6{��Qr&��YO>�j� �����@�Y\Mh�8�@=#����p���J��M9y>g�f�|�	�.���iR���%˛�\fc<���z\� ����WJ��:v�Y[1���Q�nk�.�G�_���P@<�ĳ)<�ˠ)�.��1�PC�I����H>�����ѽ�<�g=��r6���/+8�����
<�䠛RX{Œ�A��X�(WL+MV�H�\<�431:(�Ǻ�r�������u�'��%`��*fǫ�C!�۬��Y�����G�b~0�a�A�2n:�?��n]��y�.��õ|�T��Is)�D��k/%���5�+�[��'�]#B�yU��v�tе���.�����*@�����1a�A$�����ò����=m�emR��RP�o!�g��a%�IԻ��g�\��'= 8�3�/yv��!��RD81�)7�e"I�Iϻ��(t�C�(�TbI.y6��E¾9���T	��g�2Ҝ���Z�p8��G��w��O~�ٜ.˺�';奆!��~L6���>>o�?ԇ%,�W�jQ�3~�qC_�����RDJw�����9��'F#ƾZ�z�`�]������Κ�'oNf�v7jCS���BcL�Nß<DA��xP���Q�1]�³9]e�Ba�p&�97W$�����;��q��x��m�xtm8*F�/y6�+���C}�hp�
�����ֱ��D����K�3�l�t���^�¿���BGF���ʺ�����a�}|�d�_�)x2��FB<x��4o�U��S�!�w�c���Q�Ş��&�,�o�������?���n0o���̤�u��l������J;<X��\��9C�A#�w˯󵻘"�ё���8nݻ�(�򔚱���g?'�&�D5��z��|�sXw��.��������z��W�T�"S[�������ܓ�c7�3����͛�s�:[w�,3�����U[�O�D12��[;�������"�p�Λ��2J��R�p0&s�4�<��P22��#�r��i���c�QM��<gy�Tg|�lv5��x����`��n;pn/�^���1(�,!>�8y.�m�s����m�ԏ�n�rQS3o������n�\���[S����s�l�sن��l
�p���sq�
� Z	Kʁ��h����G�Bv�A�m�s,c$���DXu�e�Qr��ʧ���0�(߾��j6�36������fS�=�0yA�jD0�����&��E���X�8�@���l?W��?~�]1y4      �   �  x��ZKr#�]��`udVf�;�7��g�Њ����p"&��C��^{�C7q6H�F�8�& �JF��W��e7�2{�$N�-�-���ip��ax��f����懟��w���ۻ����l��Ƿ���K�ۼ���>�l��|�q3@��xck �=U�k%���fK.�p���� �0X������c�i�=<�)E��w�$�O?ݭg�A=��4FTL18�l�l�Zg2�nbd�^[sz`�4�=��pL1}��G}��[�S�!�p[�-đ��W�j	fF��ߎ�Ǭ���
mt!�AB�$�M�zX9�HɚV��-�װ��l!Z�@���nB
��l��s����:��D��<���+?%v2-6��]$S[�+6��-��el��菑�ց�ID	�gk��ؚg}"��ѩ:[��\�9����$��"�9[]U¾�-��E�	�mP�o����d�u5 l�����t��k�vc[�C��MLL(�S�\ڶ���Y��H�b��'k��\����5Ϻ`a���'�AW��-�Ƥm+E1|�BR�q)[8cKٚ@� �ѳ�����Bhi�����)O�|�����%􎡚��.=��U��X�yG���J��+�(�j�݂�"� 2mc N�D�J^�Ѥ��@�b���c��<��=�GV�8�E?9��Y�g��v�C�������[O����?������r�y�׿-�<7�)J{C4*8�'�и�kL��d���S�r.��@���Ӭk6l-�\dX��.��,�k�Q�$��U�Iʅ��\=þ��B=���I:���Û���NH�,J^/��E�� ��`$v�҆ݷ�L�*�\*��L蒃'L�/u/{�;J��R��D�^����0:�Md�
��ϟ�3yW)[rcԎ�yLz�s5!{�p�.��zv����{8->#��^��d�zA[�$cm����)���8�Y�dj�#9�B��{��'�؋���4a=�\�+����>�%��	���W��N�h�9������\͓���ivU.mo���_f�&��T+K�#����*�Qe��/;5(��yr|e���йe����q�U���Q���s��� jM�v4F�,qB�%W"D�y��|ΫJ�R�H�������j�5F�Ӭ�4u��V��P�xm�Aω�OF*�о[q���Q��Gd�;Y{��_"��?*� ~�+΋���������C-1J���@ӱEmk���[,B��.4!�+�����/
����be��k�̙d�5r����[���-��=6���Ǡ'�B�ČZs9�BB����4]����fv`��Q�p����JN���:a�gj��qc/��8�UD8z�˕�	x/r��8؛�*f�::vW����0ڃ.ƃ��66l9�^&kt�:�rIkOg��Y��u��-[�Y,����a�LQ�Fs���،0K�Y��^\E��CP���9��=�)wxE~V���]�;���J��Iw�%>Y������C��Ct�B�J���P���G�u��wӭ������]��8�!!N{������1V_ص�0��T�f\_g7z�āRk��4NɰגK�X���R($%]j/��>�+O�{��p��=�FH"M��6n�`��X���g��Iy-ҝ�O_Q�ii@5����C�tz_t$�ՔP:�*���R�����T2sE�nn�F���x�V� s�M��?�ij��+:�v"Ji��t�Ss�G
&v%���J"�=_\+L��:?ӣV
�ۮ�&I�"��yl�7�W��(�ɺL���]n UD�%S��j�d_wr�K��Xy����%a��^��p��5�6tc�.���SY��ӧ���:�x��0}Wk����tZֹ+7S�׾���g�[����_�9�����ɏ�h���yڣ�.���[ &=�s���($�=�����Jh�,�ɐ�.��PB��S�kM]J�#ԮLp{�J����ݻ�Iა      �   o  x��X��$7�k�Bp� H�M�9o��;G�qR�dH��F���dW�l�T�bu۳ڊ���bt�Hdc�T��[�<�r���M2�"�.�1|����E��\��Sr�M���߶�~,��n���_������y�|{�ki���g[�M\��N.q%���bg�f�-�!�?���y��1n���\C�$���L�b!�h�t|{�%2���Zܞ����~�el�Q�;��~85uT�L�e0�'5�mXk�A$��Ȟ}J�
@�Je8O�,��CtSv^"�a�a��?~�m�N��,��s��9��b�{���)�#�����۠�Ơh���M��c�ȱa�1��)�N�Ls$3�^gٙ�"��HN�SƦ��c��ݿ��E�W{�B.�p�x�&�����lf�̄�K�$�gsD8��@��g�Vd��ɂ)�����$%��[�8���x$��Hnx$�����s�.J
T[�K��gd��H�ȡJ��j컂��C�RE?$<R��1�ż;��.F�^
>��/紃(�kl�����,�[t�s��jo<����XY�����"�.�\<�p P���)�Т1l���L|�p'@��qK$a��\_t?�jc�G�I���͑--�	u�:;����:�������~>�����0i��P��Q6����J�6t����ȡ�����ev�����*�����h1�ء5wdԿ���W#7����yt�L��grNe�rnp��p�@/n�6�յK��0|qܘ�j^�~�bA�Z\��KɆ��H��H�!���>V�>S��*n4�V�N�֣�� ſ�«Q�m�`:i�i���U$���6�\Ky�-*��kKBm���"m W�#��^�+�p��3ؙ��nw��\�9���*���kD�� DeY��9OH�h!f�1/�o�a�C¾2ދ��#on�����7D[g�w%|Ȝ�\Nˡ
ƣ�y���R��b��
��.���A	����@�e�[��.�]�����➇���앂�`�s��S+-�s�Y�;�x�"�ۣ8X�.\�� `f]@On�?Gda�JNj� �?ϒ.���rF��i�ꗟm��s1�ՠ���J7��)��{$�Ɇ������r��X���8�a�jS"� �����S���i�m.�0��cax<�?�ڳ�}46]B��^���^ה60﯒va��c��n�a
�j���Q�����ɜl���e}:���@4e`n����.&�RO0t��Ŀ�a/�}gXSB���14�c�����V)��adw�z������
���mC���S�� �w�S������S�������?`����c��G�hb	^ј2�@^[/6�h�?h5Hq���b4��m�\�����><D�dAD��C�'93G��������墆{��(�B�54TMk�7�A��b;�3(�
�çU@ACZ���H�;���z��%Mo5ݩ���p����k�J0�1
�v��j@	�̒�.
��8�=�4]8-�d����iq����U�3Ӽ+F��T��e?���+�S���0��e��"\D6�̠|J��?���2o�I.\�t�^���@��'�+�@��^�����w��؏^�      �      x������ � �      �   }  x��ҽm1�Z�"� �?1Cd7%M���i|. �p�Bx��z���D��a,���m@,<��Ɩ���ۯ����K����Ȝ�4���3�s������ğ��7ai����Bz��4�G&��tmχkw.ք{IA�� �]�n׽n�ן�T_k;#4��#w���u�5{�2>��׾i�˵��GP��E����_\m"�Luኺ! ���L;Gs^����Z3cX��Y�^x��M9���%z��_n���2���G`Rm���,߻^no<�q\Tn�L<��C�̑�%�c�}���6ǭ�� ,;{�)����\nXюZUg��5�����#��gW���BpBkN���)�tyy���ܷח��?A���      �   !  x���=�XG��y��4�M�&�kӸ�����w��xcֵ!�.���20p9�tttŰ�Q�u����:D"`΍Н̧\�čLg�r�����2��m�B��q�Ą��룝�Dx����ݩuF��w����w�����߱�jum^���|��-c�n��
����.`Dby���m�e�y`呪����3���aCې���� ��%��>�d5��@r�Btm�������F�LLW(�ǽB�82�V�"7�L~�q�Iäur�b��3�+z����h>4���ΥSwZ��r)㖭r��}�����	>,��|�a�\?��vfqk�Zs��Ϗ_n�'J?�����J]w{������0	s����M��Y!�aU_tk��cL����m�~BjAZ+��Ԟ������X��2�O����)e]q<ANu2��%���$��@ZA�F.�q�L�_|v����ݴ�-�#u�9��Q�{��T�P^ZE�#�����1��
��Z��׿c� ���kll3p����er*�F��k�V�͝| ?������05�      �   �   x��б�0���� �Ph�hL�dP��Bb�&�1��e����5ߵ�O�RU�u_AB$0��6$x���LH"̓�㩌��Z���e�u'��	��^�g'���H�L1�����oZP�tu�4(J�� ㌼�|�^y_P�m�g�8e����3�q�Le�@�����l]V7;t��9��x���`p(w;4�[��
��hL����U޺�Mg�Ct���������S�c?ɴ�m      �   6   x�=��	  ���0r���X;����H�@bO���H��.�����u(��V��4�	f      �   �   x�E�;
�0D��S�	�?��I�Bv�ʍ
a�Y����>�@0l���0ҡk����tX��	ʛWv%����%�$��2�
���?}FU�Y�d�\��y#c�k�V8Z�z�;���/v$-Z��>)���T�̏�x�s�}5      �   �  x���[��FE�5��j�gU�����I�����٣i�L?�� @�b�b]��ӂ
Y�`�|X�h �px��o_8�b��V�t	@��͂M�����-J�����������U�HZPߖ�n���q�o__te��J��A�Ӈ�4��e�.L)�T �0�P8��,מ���%s��p >�jd�|\��m��Ӻ���?b�v�x#��:�~'(c������9͐#�����S"��r< �^�-Bz��;��]�g!p�L~/�j��5;0��<O�83���)-�f���L�'LaR~˫���u���(�Rh�k�����K{�������U]��Q���VC��Aj/����b��j�ݨrF5�ݨ�G��0p�$f��zTk5�ָ�T�G�w߼���3 �߯���v5GM�CjAZ��0�$j9�N[����?~7���-�}�����A��kP���8i��{�d��-� ��E�"K#�v��e��;��z��%�X�>��//�Y{�qR��%�?���ZO�����ْ�Q�Ί����x�a��o��
�t���?G�}���=�l�3���w0��Pb�0$�y �� W�F�=\�W�$׻�XX��@�}�6���\$��8��e_��WD�`�$��6�I��ǫ{8K�no�+55栄.m��[�D�b�"��6DW�tA�TCF�D��/�ML1c?v�����9�-hv�=����1���=`���E#:��Q$&��ca�� [p�x��}���"=å�֮��0���D�z��..���;�g���xtＹ5��7���QO�U/~{��}_~�O_�,U�2��Z��n䆽��\]D�{��'�C	+�<��N�����N�㳓�`��ƌ��=��"�j����s/C�,WV}o3��J�����Pn��7Q�89Sp+�ØSE{��$�������Oc�^�Ż����ƭ�9��Ir�gՓ�}rxO�����	�@�)�O�Z���� Ѹ�Bi��	�͈y��n#�*iN!alAz����m-c�.W��؈�^#����3�����A����}0�<��#7i@��j$)�A����E��m���]9�o+��K�#O���$:�;��,x�P�%5�ɍ�a$�k]C�Qj��RN�5��^_^^~ث�;      �   �  x���͎d5�י��%,<r'q���]�-��q�� @3�<=�v#�!V]��V���w}��]YRM+LKt&�.�<6-��V��s��i*�����>��'-����;m3R.��r�Z:�HC8}���i���)璾{~}�2�;���C�7x�%��%j��7�f/d�*�q�y���Ԥ�XY�g*>T2���ѱ�.{��7	�0���� Us�̡���M���2I��ք�1 ��~o��,5|Od1:���ɺ7�\=���-�nU!���5<2���j�}[}␚/J���1>=�-��Ǐ�ׯ����G��)7�t���Lk����Es��gX����$eZ-�� ��4�	�E*���ϓM*�0���&�.�o?F���2<yÑom"��E7�wTG�)G��.�}�.���Qp�9�u߳m���(+Nɨ~p>���(G$��L�
��"�g��X��o��
SWϳo��5��j������?
�҄v�@�7����u�4=�pY|�&����E%#HoLv��n��y��Wo�#:J�*Tԅ(ڈ��!�?���I������#U��c�������9H�4��A���].���P6`�FE����g¼���*e�ܪ>����@�g�8K3}L���<�x�+��;�k�[#�٫mb�r>J������V]
o�D��~�]�9F.��0}��g�9J�د��$��$��0Z�N�!ܵ�b�Ť�l��b�y2�`�k'̂R��_��,+2����/��R%�ˤ�p��67����q�����s�uS��ie�1����o8�Eז����Q#�&��CCFC���!+�G�`\�h=ӭө���$k��=�gg��gaX�k{y��7?����7<��B/xZB�"�?,�&��ĂV��1P4�<5U��tj��mF�A������<6���_?���ȷ�!��u�C1o�^�%�/����է�nI��mM r�A#�^Q+0-���yfZG/�y��񟁆)"�H20ju�U�����K�Xѐ����5}��o�a$��d�k���R��̙zIس��WA�AP��Ѹ���p5$oU�Y�t�T�*��!���fH �����kT�<��>����Q�I�Hǯ����
��ѭC"�4*Î��ݼ�û7o��	��      �   r   x�344�440��LMN�4204�50�5�P04�20�20�357130��# [$�M��8����fh�gh`jll��4Y�i���@Ӝ�+Е���[as�,ذ=... �/z      �   �  x��Z[r,+��Y�l��^HЋ�ܟ�W0��IQ�V�iQ��ՊF-�R��&oܥ���E�M������?̏Zҋ���f�?j�J,o�\����?ߍ��������K?>������~�5�v'��z�]8���C��\�ܩ�v�����sM����c�l�	r	������|~���_�O{����/�|��D�.�'ѵ����� -FM�Φ���88�of_��>��*K3�M�a�t���iN���A�iŐ)��ɬPtӾ�n�l���s�L#9K���Nt�r�� *��اB }p�ƱQ}Qȓ�M+T�N�b���*�z?����g��~��]g�"Up�~۝#�/����c�p��d�/z�����{K���R���lBya�0n�[ݿk��2�!S�Q�qm;���T�q��st��lŔ�63�5#C���X:R�H�O�hHI���(�x1�#L}��]%��=�-+�B�'���Xuڨ�J��4t�k!����dh�X�7j������^Э{�!Ő�T�p��n��XSsMCל�β1!:�ܝ�yi	)�JmXu�D�-�t��*/��L��yB�D퇢�j�~�աG/����p7�,(C�6ݨ�Dօ3����|���H��Nl�4���H�s_E��ih��"M*��Y���d��h	6�_b��;Y$�g����K���F�A��>8W[D�28G���w8��z�Y�K�̔�9�qS6�L�g�Ց(;�G��k2�/H�	�)��f{s��˺Q�fdkk螳	?ՏG=�o=WU7���BÞ�l�l=�N}��U�kF�(=;�� �I��ne�~3}1^Rud�L�y31O��?��/�`� "�R��6�q��H�:&�����j��� �L�fjN)(�`|s����n�G�����Q���9��Œ$���>S ��%�'o��Xlڿkv|f>�x���7�9��y�5��gS$��Δ@_�k���|qج%Z��.Ӯ�82Qg�`���!d���T�<�)t�xZ�4)�}o�J���g7�j���8Ӑl���-�#�o�7��ֲY\}�~�Y\���uL?��w���u�i95��������el�~^����*���׶|j�����6X�]
��Cu��i�5���7T�v�R��(�1�i��s
p��v�ϫ49T@JR[i��W�P��qld�b����� f��ʓ���xCbJ����$��[���R[K��mH}��Ts�}���Es�&�8���Td7���4�C��Qv�l�z+�X��������#�^u�Qt(.a$��ΊVjkV0�4O�O�iV�i0PL�����g8��V��Y&Y3��D���d��֒Ou��n_\>��Pȝ�g���ae�?�rh��z�����ݚ7��|N��./Zk�y�+H��vm}��C�|����M�1�	ml�ekJ��X��,��д���	m�a6��>��@���*�[�
���x�j/�z��p��?�J˳Kё��E �4E�#����ѱ�o�]j��BS�H1�(��:����*H9υ�R��rȎg�l�t8�C*�sK$\�ʡ;�3)*�Ϧ!�z*c�yɰO���l�9:7Wƭ����z����dJ� &F�A�x�*CxL�����ɔ��K^�F��"U��Y�ΐ�rvHَ�vt4[ݱ&>GǛM)X�e����lqtǚ��U0�}#����i�lqt��zh�O4?Kp#��ǎ�z�&Y�5�׬����Ƚ�I.��qkH>� }ޔ����\1�C��%8^��qP;���Ԋ�.������gyq�x����ۈsM�`�N&��Uu�
.�F�k�ᨓ�dJ)VK���Z���q9
j�H��um�s�x���>�AÚ��|�!G���-�2jT2����3́�*Ս��{�������/�ڸ��������2Na�MO1�/�Լ��I��v�.䳪�՞@�vd������E��Uѝ;���r_�֨���NS��;C��k��i�o�^-�7m��ļ�u��	�Y��&S*Ij'�7ԩ���4ŕ0�Φ�:j�v4��̔$��\�l����՜��\C_>�;��^jǙM�LE�#����d�}Hzq�Y�![O� �K���n�K9�1S$-�Ȕet�Y�&�e �� jl$��.�cH^:���dJ|˔�!���3�3�Z#y{vw(��%�����?�Ǐ�䓁z      y      x���ˮ-;�$6N}E� Hw:g���3�4�T@��ѝ��ˌ�\AgG�Jn�ʼ�x"��n�s�u&����b�����ժ�(�������M<�Cs!�=�_>��D-������˿��������������/k������������������������V��^̻�u��t�6���CZJ#�5�8H����_"���U�<(���l8h���Vp�f*I������K�G5ך����������o���8�qH3����'����x.�>��ZH��,�
��x��\>�L�]�ti��b	x��iZ{|�����;�A�c)��]~Ϧ� ŵ��+iH��5��� �K�/M��͗��C�al>-qQ}u5����%�m�x9�~��I5{�<�����	��ŵ����K>Y�y���s�W���>ѲOft�a�\����v���V���*�zy�����5��]"]Soc��',l�����I�g�w����_�(L&޾����û=W4��Jw;�¬-��p��
���~]<������Dۮ��nmd[�+<_�&�O2I�6,�G7���Y\�e�&���5>�?��|$��G���N�qs�EWg��Ϯ�r�.��D�yE��Xo��hޯ����-s=�t�@C�����D�W�%��n=���Mx����4oW6���S��;?.�}��$,�Ƅ�[��R<F4�΄+�+m��#Ko�/pn�Mn?�E��|������퉸�2���+$���Õ3�~\�H�w�Vr�l�0������)����>��Xn[�gC@���
�zJ���up"\�Ċ�}{��~Ik�7S=cZ8(�C���"����Z}��׆K9K5qx˸>{��mf��9Μe����O�|J����&�`G�<��@�GT���d��⫭��>	�Mo#i�mځ�]��+w(d����/���bIH����������(��6d"�dć�ڽ���GQ��FahHU���fe��AN��F�R�e�&?�������Ȏ�; �S���u�P/k��C{̼\;�	W�3�k���aӦ�]�t���Aᗗ_Q?� �n�M�5Y��n��&B��e�%���)�B4�>�ͷY[�@5��z �bp�}��K�F�CM^[��U���M��ph�8���t,@޼�z9� |�%-��k��! ��.�t���RY�������qEai��xj��w�DSu��s�RAa>2�%�S�����!YxEM���z�"��1��7}\�H/�' �^Â��F�
�l"�>M���q��3Y���P���6��h\B��n�	|�@��\F�H����ۀxƝ��µc	�&�w�7�@��Ӏ�� wm���-�q��ʺV	� ��st V���j%� �_!���Y�����ϣ6�߅)�����o��,m��<�7XKN�=O�82B�3	��|���j��I�C����>�i^~��,�]ao��Ay@!w��+�Ļ^?��o��2����U>~�/ë���~���K���{c9,�5ĝ��OW�ugY�.�ߏ[�	q�@����dD��Gt�?��4Ь��ŝ��^l�X��U�#��t'`��>;3x�� "��,by��L��HZ� ��xj�Qal�l`��8)~�y	�Rf�?)���Фt�E�M;������K�z���:0��GH�A��) D�[kÅ��7��H
t�@��̸"�m�<Q��8K�����q:�f&���z����[&�C
9����ܔ/!}�F�`UV��s|쳌]�t�J(�o��&�ptS�UVi`���1�i�IP��ׄS	�y����/�Pj�AH�)7f'Z�2p���?Rz8�B
nL�ë��:�6���DW i\��Z=Q!^3XI�ML�U4��;DQ|�msNXZ�iX~����,%`��|��'`�(��ڈE2캄�c���Oc�W)���� q-#`<.�,.�:ƶ�WS_aވ�����!����ژ�mx�����ς��H ��t9 �O��+co0^�B4`w�8�����$&
"�N���K{� ��@���[��DЄ��$���@�N����V��t�9DQ`=Ն��$P,C$}=�&�q�40)x�6��
����BLӝ޷�rH%���c�g#f�{r>����A���-1���� �J�AQ���
@�u�$��O����:�~���!�IϰwKARkd�\��{k':/�C'lI^��	����&ņ�CKɘ�8����Y*���rob$����굙�����0|&�N����|xq��~Y<�&�0���8�Sa`+@g���������ʁ�	o��Cky��<
^Yc�8��i�e��_�M��Q��ނ�Q`���@n�n|�
d�i.���Sx	l<(~LJ�!�`3a9 x���}�	��7lo���vm�>�k��k5�F\�w�"n��
Q�N"�}�zh�q6UZhc�ٴ���[v}��BagF�^�ܣ|4D_s)�ϑq�+oL$]
 :p��~��&L{z�5^C���H'; c���3q�ja%/W�"!�"$�ۚ�)����nT%�QP���c�-�m��f�Nv�u-Ċ�Q��ԕ�\e�u
�w��i����� ��[�9��h.r���Z�T!!��O�M|��g]���^�x`O22\݈:@p5��������,E��ж{֫�؛�p3�́��σN��d�)���H�2nMb9����H@k���pF�/�ޟ pr�0�-a�9���ò=^��(�%������އ�Z���03�9R�X����/����x�2�^f��i�@��!p
@j��S�ó,�Kݏ�z��a�ހi��"
��jfO3;Ӕ~U��5?9�79\���3��7q���UW`D�r-���[x9H��) �Ə���L��VXtK�<@�d8n	���;J��C�y���>��B8�f�u7���h�6�J�z!(|�ܠ�0F�X� �4H��
� N�#*�Ӊd�{�pT��{)<.f�'w�8�/d���z|u�V�rp�?�Ú�����A�i �m=��9$*�;(�UFWe��B ���U��]<��ޜ���v�ʚ�e�����#�%h�묈�I�D�Ou2\���7�F���\60m`5��F�m-W��ӈ�$n<L�Tn���:�5Ob�k�D�`U���z����-������F�w��LF��߇�9D�\V��g�9ML�'~�f�3�A�bӅ�����ꀧ�F����1�����v�
��>�1A	P�����h#݅e��ٷ'Y�����gN�c$����ޢ�H4<��~�66��O���/_��e��T�W�d��dQ-y�Y��DT�C-\=M��rN��yi9�w��0�8�����WV|����XąfX0�'1 /զ�y����g�_�&�VpI�ڜ�Kq�]7^@� �ͷ�� �3V�j��%k;�s+�8hä�A
��aƗ.C���s�.�๶;>� ��� w,����E{x%? �5^��Ywf��^	��w��ZR�s>�A�ð���L/�  46D�Od���J˖��Uꯃ�֒�vc�1O�'���6�^�K�1L��\G}6�2��C��!kz��0jf�Ow~!զ �����a�-����x�%h��) B�Su�Aa�ך�uΰ�3�x�1_��p���SZ;$�d�
ď��$�du��o�ŏ�I��#�(�5�I ;�M2��X;xo|��Y�g����E?�n�ƞI�Q&ak`��ix���������C��6X�I�^y�m��ĕ��4�ЗCxC?��7�}r��E�nd�6�Y��i����x9(1�i ���U{?S��U�8|a.��Q{`�Dpm�8w�ںmP�2�t0���	�Kְkڵ?,-��I����+���O����D�9��� �g��'2@r*����K� F�=����<O�    ��ᆶ7�&��Hl�P� �86�B@���C2��	���a�y O�K������	\��2.-����y}PǷ�q�O����7��q�/���[����������iX0���
��l�e.ĭ
'�~B�� ��V'�fc�s?AG_l�����.?�v���o Aޠ�V�� �MU�{[� @���|��L��_�r�fm-�{����juM �a�g\d���s*�;%\�^L�!":#��pЁ~�-<�<�`�4~�{�k�#����G�
���dL�o�A������j�^��6��e �՘�����*L�����A�+Tv�6��@7��������9Y���`���Aƚ��q�c[���'n*i��9���"k�/��t(�M��O�04��Ď���ܔ�&I�=}{��wsȜ+4v�����՚����J y	8��t�l���.L��Y�O��X�/]����U�U��	j����7��g-���68�U|�`��� <��m��;�n��Aw�0�7S��J-��<!�|�/�����6;�gW˼��hOCω n@��� c.R�M���1"�bV�:_Y�6By0Г0㨁�~� ]Z'�U�ЙY�o
3Qt�9<��Nq�� ~o��	C�L� @<{�8xe�����)����- ,�c.�#�|@�7�z���_b9<`�z_0��<&�4��A���8��7��菂��Jml��a�^R�B�{�q(-��v8�w�(���A�;�������ڠ�g\�8&�{��ϖ�,��7�r�)"+�K�n6��Km�Ւ��č�`eVy�Q;�0��x�]3b�l�9Jn�����С��x�D���篛�4�XDXdcKI"�Y�o�.�<��7:�%3�74��/�{qڍ��vM���~�G��}>4� ��o_������
����� �ey�� �6Xj�
��%�r`��-��7u�0����ȨN/���:�l�4)7��z���
K������w8L?���[��*��:�t�������׬�O+��6)4k���-j˜�Y�0-e�K<���@�g�O�Z�ۙ|��4��C� ���ל�)�Ʌv5�JM_�>c�C�'��M�q��b������Bk@@s��d	砈�?F��I�z�X�3���q�4.�#y�>jn�T|��v��&6�D�����Ԏ��t��U��zzq��O���O#�����R������Hk�~ ��Lx4DnO�@�𞅵��۞�F�;�s(,r&ތx4h��B�̶ؑ9cb�������6�*/>�n��]9DC�Κ��?Za˟��$/�sH8]Y�ʇ��@8�X��氘��Ή{�X�H������IҜ� [XE2������|yV@�{qq�M��Ho�HY��c��6����y2P  ��1��8�c��\Q,��7������cvʔPu�j '�C�3Ae!_O��q�b�� ����*Nj�a,�><r�6�|���z}� |��A�#��4 �m���g��kgT0��ܐ_����۟N!�N�V��U[�8�Q�g��z*�"�<���Ʒ�x;;NLvbSs�/W� ���;r ���zݎ�X1� ��\S�Q�x;� o��}a���`8�VE6��(7u�|q{^�`/̠��{��ZF��oi���N�]
�҃yL{��V���.^��S/��*�)ǃ��D���o�堓�>�p���" �AXK�=�A��J�ůguf���'�[Z"���U���3p�@���D[��$ߪ��5~k���g�z6�N�[)�)���s��/���2��Z�k���fv� ׄ�*s$�+��8��\��`�z�Z��J�Sت�Y��q�&;X�lo�����r�ms��F:��`ɩUQ\����}ɧU;�t���3�|�����j)���}n:�q�q����x�z��2�g]��[���V|+j%���2���`�
������b(m�ؕyŎ�͸��߂����p�C�2��香���%�`�����a�*���,���Q��-�<(�!��i
���孅�:�T���K*}:(�_��_#�S�l	#c�T�O��^�dv 7 ����5"Bř��|���Jq���Z��W4��| �ߍ�J�3 E[�V�jC~+^B��',uђ��x  �5 �1͊�R\v4QY�n�ːRٗs���d@�N��������{Ύ�i$)G!.Kq���k;i����T�`�x��5�0��i(ȃ��s���_�8jl�"摕os&�`���(c�W�R��V_Wǘ�)>��l?a �����d���u�T���L�QG��hu�c� �(��X����7�)e=F����{����7Z��{�:�L_J?)(_O���sX�����t�����8?�<���@U����ɬƁ"*˱����Z����v���P+��wsB����nF\#���"�$���a�in�TŭEﵤ����q�B�	j5Q9��â�A`�Ƭ�M�I���͸ٟ׆O�P����J|��L�F�$ʯ\��'8'<q$ �W)��Q3��ryB��Il���9i��f=�,�"K/fj�z�<������Հ����2Xu\m2q8ܐ�%βB+�C����^w���3�,���� 0[d�� �I�c��r�}�@�+6:�.`������
���d�����"��9��ؘ[��&٨	o��p��� J�O�� �r!���<��4��'B�!m�Q	1���^���
�F%]������KY,�Ә�H`��2>��x�C� ��:�Bi��*�h�Jr :"�v|�G=1�Z(�w�C�� ��E
Lଳ,��N-_��u��69h�w�>���q��~ļ81.�U�E���~���s�t��hm��"<���!�E_D�8�$��?��h�+���k�ޝ�V��+�r�G^.9��S�w4|9�h
ᔜnػ�#�n)E�<�ʀ�a�Fm��Jy�m�5��T�9v�aq'�GP&�g�6�����oQ>���G����͘%�DŞՍC��җs5� =��F��f�IQ��
f�4j����u��g�{�dpGK(7������ �Md��������E��fG#�[���2@�9�7M�@�q~�����Oc�Jo��)>��y��'8[U=��B=��@pz�p�K��|���,��� �I�w�l�%}3�i?�q�A��R�^�+� SIl����H=����}!���|��>
b}��zs�ʻ�{�	�V�n���3��ϫ�Cʷ`��6��E���qd N���m��|{y9�<�P��n��|f������|�+�E-��Ԫ0�/��l�З�E�W�vXt�mդ���8<�PU�`�j�44�`���>�H�'��繪 ���8Q�:1�z���'���l�tj�ف����N�����SB���"�>z(k.�3�����M���A���I�=��ޏ D|B; ��c;D��Dݓ%�<]l+��xc���c��u�*n���{q��%�Bv�#�����ș��3T�o>M������
 A9�	,*%���w�|*0Ե�6�J��>V6P��
��g�/x���� 5����r�Ռ��hg�H��UB[�	ov���y��rd���R �+M'�#S�2�z<��i�#y����C+�W���@j�9߃k�qEG�oON��xěR2�?~�
��+��
�»v*/����6�0|�t�>�0]�ڂ�P�tV�Z�kX��a�	q3Q�Zl
XS&x�R����n@
�n;Y���A�`԰ț��/��uJ�q�7&Pi�L5D�9F�<D)$��͵>8{�qy
g�k �b��-������E�ߕ��-]s�5F��� ��y���"(v
~��(/�'�#�jǹ�>I�� nXH�F�A��%�.�����"�^���֬��<f��ad�M;�Х��6j�}r<���(��Ҧ>��@u�Հb=�B~�>�( ��D��斧��    �Y����|��o�J�h�urmu|w�
%�#��k)"Ɓ�����~1kf	��pE���7 � :��ak�`!�XvsxoQ]�aFg�a� �x���+���CN����py�_~���x��`*"B�z=k=
՚yK`�����g����A$��X⯴ma'\}<$��֎ 7��z�Y��~�{$��o��:G���9���Ca	�DK�
���o�����\o�9��	�E��x��]�JO��=(}R<#Vw�"�f(��p0ل�G�������i�N�}�4&Y��a��t�|/��Ldj�E���c]�������x�� f�~<��Al�W���r5�=�_u�.��A��ln�>M~�iqG���!����kA!���X�S��j&�+>g�@�����X>�ᆃ�D��pQ*3����ԠO��g�����$�i��u�®�E�̧��������s�\1��br��v�s���ެ� �2��L�� ��2�I�<��ep�t����g-g��6��Zh\��@��\�r	�����Ђ{k*��y*	5<�..�������n��kW�t�]&�v+T&v�3zV��pGDI���=Om��!���֜+���(�@!ܠF%H�E��<K��V���*h�Uy2_���Fs��<��V�]�F4�C��N���;���K�ηJ݀���:Q��	r4��_G铒ڕę��z4��T&�������\��o���@ ���s�!z;�S:|��0������{ʕ,g�;&�@ԲOv �����������} r�_\gp�����J}�ymPߊ�����l�'�(77�Ug�o~��46$�A� ���B�i�b˅�rҔ�F8��@�Z��g���C�GJ���w��w�Jq$�u=�y�[�T[��7#�x2��U�57�Y�`�;�a=���ц>us39(�O%	���,��(�M��Q�F�ͽ(6�~�����f"/�  V�(<'��K��8������ynnq��7^ZD���]��±�N)�� ��GR���f�_FJ=�)P���>\J������wb_�5u�U*Ĩb828hk�sv�N�yP�&�n�U��?���bqd��ʩ^0
O����\O���<\4Q.��P5����N���1��9	;��]A�\�I�3��[�L.g��y�����#���P�- �~�Y��|
���*������G�y渽Q�����}N i��Y��}R-)�i`������g��zJ,��A?ĝ�
L���\��7]vПȀ�_���j��CN��^~�p�`%@4��Q�r�oO#l���վ�j ����k)��8�T 84 �5�sH4J!�붘=��ṷ����sU�K-���t��~�W�!.6�u�|�6
����:�n�U*�����Q[֍��M�G�g^@����+تs ��ѝ�+h�ki�9O\g��g�O�f٭�@�!�=��Β�ugn��`����jL��&ȭ�t�d��={��*x��_�4r�蔘�@Q}S�עr�a��W�����<�+�m%�\�V&آ���v �Q�wgu�U�G\�La����&��*+��1HG	�PP�u�d��]�?�����C9%�Y.#!-��y���"���y�.����l0�ɝ�af�G��'��.���v�O^���|�Q�f�2I4�����L���̍�^�Ɖ�k����)\��(����P�mK��VYv�ۣ���|0 ��h�P� )г:�P�?�x�����Z^ �P�t/��wD<��`4��>��_�^��?l���6��)�����.���Ha���������Ҵ'"Z�s^��T�\�����O�/����+�JU�e<zx��)s�������Q�⺞�V�R�1�K
����������Am�DWEĪ*���d��d5V�XF��*�����i2�G��_c��O���]��	Ƒ��n �:s��gg�F�s��;2g��4����q�[��{6�(>����$���_�4%sKt���5��>j�v�����
ᣀ�ׯ-7��/��\)��^W��Z���wVw^�~�b�J��HZ�Z�|1��6�#�5�6WK�^�҉O���6G���\� %��ffn�P�� ����T5酤�����DH l��ԏN�sJ��&���מ�1�׬�fBmx�% �E��G��g��E.@��KD�!xg����t��)w�̴ךoאּ;&�r_��R7��#�혃��:��cxymgk ��oL^ ��ށ��(�oV�5�~Nr/�N�5]����	d�X	�@�����|4���h�x���z�4
�����g�ς�5�F�6&ē���4�<�}�↮��h�����'�#�Pn�!W����!�'TN	\v��S�O�؝��n[]��൸�����ܱ��5� H�8U�(1F��TuAhM�@�=���! ��x��m[���H��Bo'�F�p�u�|�� 7®[8�N-���L����Q�S@����fnظ���O�f'��	Aqo(�؛��V��K����a�;�S�;�Y����<"8ї.�blU���H�-�U� :�̀iX���g��eK���9��gl�3��QϽ-�^�[ ̐ː&��u=f[�6�i.����+�z3����.J�\�������)פ��޳ F�C�4!�%�����7C���1x�(�x��N]ppu*\�w
��� ���/N �p��\�z��� 4���� �.;Tg�Bq)>,��^:~���Έ��'ݿ�ui��ua��"�7>4����}��^2b	��N��Gpr�b��p�9��kE�}��L�n��������/.bz��Y/����q�I
	��"���ymx]3x�*~ƇY��p �kH0��K% ������Pkn+���s�	z-8�:���p�4�x|�Ea����遧�χ�༦���{f�!��:��n���x�#�p��n'Ë^k-���!bKG��6��7b3 �Ñ~7!���uS&��Q��MY��/�g�)c�jy>��Y������܂yD����."y�|�����`��+�3u��7�(��ȃ�-T٧� {f���{P�7���28��)d����� i�R@���� ���p��ٛ{؄jԫ��'���ۛ����~�z5��Aܺ�N\R�3}w��p5���[a������X#��:���++>L���g���启9b�u��l�&!H�������G��9�8{b�F��A�e����9�Gq��m��Lkoӎ��z����/�,�q��Uc��6�R��ˌ�&��G���r��k)!�s?�P�t8�7�lP
���H{i�k)���wӨ}pȅx����`V�&>&����ӜL��/,zf2�����HK� ����<�Z���}��G$<�n_���>!���.7������>܇FPU������)eP�ͳH�[ykb?(�h��k{A,��'�Po��#�	o�*���ۀCF�j@���u0�I��k���x|�_9��:�%�-?���_��w�d�7kE��Ȝ�����q-�x�����Ǭ������ �
��z��f��&~os�ܚ������:��k!����A��BJ�6��ȗd��p�(���e���u�z �@dD�N�k�����VY�Y/�����	\2zk��2s�l�R�8�����#�����DG�T��!8W�۹n϶�2C_��})�m��'#�il��|6܇~�^�[(�׳�%�q�7q����,;q�F�|�%b���2е��#ҝt�J�6wD�̚��'�x�ӻ\vO�6�q��k�rKE��[F����re=�wqT�Y�ԃ,|{9��BT*(Welk��[�[�!06`�Bh>2qd�*%�l���ٶ����7���+��S�c���q�$ݵD&^oɜFϥ#|O�'D�>�K =mX��} D    =c��j�z��y�j��L��(tT2R3P
0�>��B϶/	�˺��ی �;d������UF����a�z��Oճ ����.f�Q�r�Ћ�wk�#�.MA��
����R����Q�il��дA�\�(%=K$D�L)���}/�T�9���f�'�0��3����`��*3<^D�N��	�P�����1�5���^X�}����O�p��n����AϮg�6���a�)շ'�|��tݴ�m��n��ȕ�����QDD>o�����U.Ѹ�s�:�v���*=������zֱ�$�9�[R��mX�(ח��[&���a(5����Q�=�KQ��K)����>���s�Pڙ���G/�z�S`�rMY=���ڭ�n�a�C#�J�b)�������/�ǔ������Lp
�6�Bh .^9�&�j������ "B,�{#��{P��&�l�ž�S=3�3#r=�g ��£?呋ϧ�·������]��j���c��P����C�!y.{	E����:g��1����$��r^�ZuR������HG��2P5s�����J
��F������A��&D�r}��k����'a(�.7���p���˫��n�RP��W�2W|RH���52q�x>t)�
�v}a�Tg���p���}��p(nR�����ź�M_V�Ҟ��{Q ���g� @�������ps0��s�Ϙw��}�T��=0���9�Av�_oN�(������;�?p��<��s��q
@u�[n��N�L�D��%�ΐn��j��u�S�{d�p��L�xl�����CXs~����f��)��3�؍	�>�����S[3�,��Q�4"��q�p��B�D�Z�y��B���3��RY�zX�:pT���$� X1ؾ��ɗ�̖s�����\���tF����ڃ��\B�-9�T3�be@"��nl��&�΁r*u���\��8�k�n��p/��ْ��4sp��N���򒰀��N�	I����^퓮Fk(�+wX(x5\M4@���}�g�'��y���x��s~�qn!�6��Qo�.���˴6�N���,\#�e@���6�vrԚ�N?U����|ux��.+y���?�E�H|r�Q�^=�=�4��(�]�N[J�gS)�1MF�bP�Fq�gŖ�j�?�y7�G�mrYw�d��^��B���?B�Y�B�#Iv5���� �{�����>�Yd賈����81X���-E_i�\��%qC=G2���[~�O�X��,�J*��d6� �)GI-�C�zb��,?����eJ��t��uNO���j3hpl�����G$8�J1|�O�^z���9i�I�Au��F�Cl ͏ ����̙��(�9�]��w/O_l��s8*��ϑ�uv��5�Wʅ�:��g�e�a��rYϔȷ�_�u��ϓڰ�qcR�pK��ձ����=K��(�3}��v�K��H�z&\@Bc�r�`?���㠿���p��5�;�.���2KMlZ�-im�9�σN ���kᶁ�ʖ�8;�Τy���l`���m�6Dl��jnN�w�O��Z)��v�9����i����Oy��%F)��AG��l?9�f�3Q���"A��+��)~W�.�Z��i�{�ZA�����yѴ��iٔb�#mV�2���ӰrW�!jqZF��8�n���	A�i�Le�* ��J1	 h
���vư�|��e��ؕ,, ������z��'/R�_��#~sՖ�G`�ρNsv��M`V�
�K����px�c5�o&n���輧��Fܮ�ǭ?eO�?w3�u"��Y �	Z�!���}��;��k���^��/^�&�^�G���Y<}.�������Ѕ��,^���O^x�+OJK�����=Wi.z��j��g2"��1<�F�v�w=d��T=�x:G��ǟ�f���A�'�Y�{`m��7��Fߵ?eTNu��]0�k쾬e۞e
�6���?�����:�`�Vφ����g7/5��� �LVހ@R��ȳ�N�5���O�Q��b�]����3g��<<S9�֔���r�r��q�A���S��g�f����S��W$�! ���a��Ȧ@'߭�zt+̼���#���pw���=��<��*������J�=�`���'�J�	>:�&72g3� ��M�6z���Tm�vK����"��^�7�� :6P��G{��bC�綱n�������ƙ�^�Nho^(G���	8J�AX�t'�Ҷ�3���͢v��	K���.���u��i�����p�C Q`^�!r�/��v�����y�5�,��3����u�)P�,�E������CV;��rx*�Q��r�<�H�}�%:;T03=F�-�8@w�r���xm��2+"_�Ґ����N�^'>�QL�&��*�VO����1�y8г`7��������ָ�ZG��	uP�>�I�*��%T@M̴޴O0��w��g����]�)�����<��������j�`����rt��~�r�8'yj>,�l���\���i�"�p��=Б�,Npk�J�RS�yH sڮ��ae�.����r���<������!�m�z$;%Dbj� b
��v����P��rN��T���
�F}�����3�� ��h���Ǝ�n��NJ��e��1�ʜ�f
mՔmZ����o�0:�W{��V�5����@�{���5���Ć �kR],X �wy3�◹"ڨrQMZ��_rR�|<�u!����2u�ػ��w[[%�rpRy��
�k��h�P�,4k%eg��^ТW����ґD7���=ќ�;�~��!�|;la
��V;�����O�\>s��A@�v�Cq����q���+^f
+����K��Ώ�s�
�wɤ�]���Z��(OHጀx�����C����eσ_ (%���2�_<�\y����e>��&��G���`����C���l�����O�#yFJ�NHMo�ei1Q�6:`�@��2}��UG��;����$5�ʪ���KX3�XM� ?�h>�0N麏!�\[��_�ܗ#�/��O������X�U��^$AwE���?���uf��<`�$>��9D�c����#�ԏ�(�AI��)���ݝ�JM�׃��x!\��Y%��ABX���R�V��Fљ��l>�Nf(5������UD�c`\���+f89ҳqi_��QZ��X�鹋�ၘ���ҡ[[#��=�8=IV@|���d��wvu��Z
׆�3ʛ5��P�"�Cx!n:\G0��ⴛ4��c���4�K�?Ԑ��V�rą�Tq�
��oV7��1��F�y�%�O�כ�g8������Y���^ܫ�)>�Ja�(��(W#��Ile���#r���a�� _9�e<�]�g�l=)�!�Tg3ufpԞ(��D��\��eg~0�% \�Κr�T
�u��9��������N�S0l:�j�����+,���&��$�QD����)��Z�����=��Ri��B���_�Z<�W����Ɯ >2��l�����h #�5���s�'1��8C�V꘳N�����ʝ��k��FK��m�g�7i�p�ϪvP�T��~�)�P�?�="����	�3������L�d �V(���t���R��^A���\PrO���S�I����kӉ&ĞR�w�`]��D�6���]/�J���A�٩����)^�P�K]^���If_�]	4p�o��9�����t���[+`X�Q��O�8��뒘�7h��Ⱦ�J��2B�����g�\��M������(n�&!
�cӉ����N�a��Zϱ���,�a/��*!�����D�Bi�ʶ#$T�ťfT��X�0YTո�}X|���3@j�]�#�O�\���M��Z<����z[��1��n��@�I�gہŮ�p��������	<���%�%�h��L�c�5&tB���W��{ʩ�$���a�Е��4ة�Z�    r�p-�'0p��Z.d�q�j�O��'�8���T���r��e���z�'��q4�_���Y��c0Y7�J!(�"jy���l�,�E��Z����	��\�����FQ�;,0 �?���!z6x�r�0��6S�B��2���T�7?c�ˋ��Ē[8X�N��b�Rƀ2Ͻ����g������1͈�Q���馚�8:gG�,�v������� .��诒yҒ�E)�H�z*7�����I�1ؗN%�۸���5�׼!bڲ9�`�s+��m����x:��6����:mi�JYi��]i����?�Md�/ᝯ���QZ�m�W��z�Ga�Wg_�#��]�V�@��[.T�@ե����v�Y �J+��y�~�x	���,�Q���ȕI&L|;�@8��AGyHS�_7W�:�'��M͝�Mm��H���}t:ScW��a�p����=l� ��;7'\�����mB�����\)j�ե����@��VY�݆/�/�MN���>]-P�����cSWіЧ.�V���/�(}ͮ���(���m�I/�3'���^^�'_�2cx�G��%@��=-l�'p�+���D�Sj���Y�&e �q:�3O���DKY�3a��4�2�$�A�e��1�(J�T"��P뿃Q��5���@�vrZ�:��C�#�y�S��ٸE\f@,�I��&�� �k-%1ಘ�F�;�@��/�wُ(�]�ȃR�Wݹ��}\Z��������_���9�|���[�~�u+A�a��dR��Ý*�(���`&/����ך�ۺ,*�r?'C��7����&�S/oOt�n
�� b�8VN��J�9��уDy1���pww�67�y�qV�4\P�8<<�r|��l6��	T	��(W��m��YT��#�nbj'�g�q����ӈ˼ݒ+\7�?�n�јQ��棪sQ����Z͵8/O��>�V��.�
v m+���G?����;n{��,h�:�8�X|�44	�+����GC޷i��r�[�6iz�V7NNp� I��$�N�/�3��@�wKz��,���p��ZΠӷ�^� c�/2#Z�)��b�v���u���\���)C珹�sP<җG���G��+���|�g�3.T۫�����<�o᪎�k\:��������:�S r��G����C�`#z=���!R��e6��]7��%�q����d�I8 DO^�k�xǈ898��^����,����]����2��µ~0��\y>96�=�n�?��Xޟ�O�������������K,�:9E�|k��	m�91nL�y���m���]�;�LC0+�_��T��Z�F+~����[9I6����ln���\�C!�x*U $K#%����_� K��k����՘��v��Tg� ��L���a�t՟��ұ)�=O�V S67Jg�����{�wpƵ�Vo �/L#[$��zv]N�U+%���{�Y`�B�����#)�u�;���Ȯ��M�����t�uX�m��*¶��,q�R���+���9Bn�2�����ġ���ǋ�
܈��{w�{��w��=��ݔ+f�����JoeK�C�L��jg�[?!s���!��4��R*��� ��[{��9�j��d�5���L�%����1����xeƌ񎘿�T�*=��)%��,@�uR�n2�c+������d��_46xcn�A�fBByK��ru+	�T��p�=���h~��[�*kʓ{g��|A.=l�C��{Щ�	��C�����#F�P D�QF�C�5�K���|(���h���n��'��\�̱3�w�p��xd��Gr��\�Bm������DE%��26�	.��ĳ(���ό�TN2Q�}O.鎭����G�&2
�}�	Bk�� m�$�;�D)(P�4X�z9�� ���{8E\7�6�uU��L�6������ P,j�_/Қ�X,������
;�g�X����WS;��Y����qP&�wn�
X'�&ލn�Ť)�j��_Ҿw��R,ب�D{�������(��%\W��M�Ӥ4O���1R&������#��� W��u�M��I��d�=�D�R�~�>�#��*e�\?M �5pm�<��R�^w�|#�y�{>p����E�wS���[@l�* ip���(�0^��/qGfC��/ś7��_ی�ppd�J�,���x=Z>���R��'����&.�t�w�Vt?%��S3^�*��#~�<�wڡ:Q���zo�͗��<'尮� e�Ҹ��*\��~P�8��G�8Z9$	����O�p�j��R��*�&4���Z6�T*�ŬY�#���m�65|�oS�&��*�MkQ�������Y"��y���q�cQr>��xO��U
8k��U�����t�}@�6�xj9�}��M@:���z��ڌ?A�k�. @ϥ��w��(��e������σ�M�j�&$#�1�yChf���&S�N�N��[[�����[����8�!����H��JiE���G��D1B0�r�kb�1�5��#�r6�(���#u��!�t�Da*����\�iT����z��g�#�J����awc�k$ڇ�>���* ����65P����K��*D���K���rɝʡ��l�����+��L�%�=P��Ø��zۜm@�GE�3�vd,*��U������y/W	Ki
����ٴ�pmN���.���0�#��l )�;|���FxlN��#, 6���i���<�\z�%\'(z�02�ReH��"���TT���I}�i�|c��U�'k�5���@`�
8�%�J.Z�'1i}6t���!�I���\���3v^I����x��r���wjJ�T�=�n��Z@gO��R��>�����]9��l����-�<�coܚ��^[e�+�xիVM�H��*�	����ò�j��
��1e���~"� A�A�&׈����@�������~r1�p�'\��,�2^-/V.�� v$DRR���uPLp!]�6]����DN�(���
X�f�O]O6�Nc��@�?&F�
�������צ\�1Gx���mDx����70�P D��B�y�zvܘ휺��oê`��uAaK(J���F���X�'�!�ͨ/�&���T����|�y\�� ,W(A��,|k�z9(���"��K��6q~P!��;��9�{�����0=���u{k��s$�g�T��x.Þi��_ޙE��4W(0�21A�,+��p���e�X�qbr|��[;������*w�vD����x:�Fwߒ�uτ��`�<�4U��A�� �x1��p�Y���ۚ�U�$��+��<m�^��N5�M�	îCȧ��M�n���~"�ٚg2<P�m��6ٿ����	xP�Z�'	
p
�%�E����t�+6�z�9�)�J�ڻM��;�ӓ�B1u���ח��L2(��!��(������q��UX�Ώ����Ș� <Bh>�2����9@��,���8H(0QjI,c��yV��$\��U���8'H���S�p4>7 9K/���GP���e��И��*qHy����i��=0�\_�Rw�d70Sm��"�#��a��Tg�kP�j�`WM p�ʵ�aZ|�z�l@H}�X��iN'����K5����P�8��pJ,.�?�^"sZ�ٹ���"�mB�̍�L�h�~n_@~ �^z�8t�9����u��2�������8:z�����1�t6(�rV]���>G
�s��u��b\[���uP83��jX��]y̰P�Mii�j��,:-�I�9&Wi6�`Z8�A�N�i�M��YG�I^	d�㖷�,}N��Iͼ�T-ᚳ��^,�[#w��z��%w`:���@�+��=�H���k�# ���V���l@h�߸Xb��g�pp��fiGd�������)J�{��i�ͮw�78���n�(S$�z�\cRXq%e`t���J}�=�K��@�b��7�ڶ���q�G\Tz���YH'�l    3~k�&y����s.�e�)g�5L��GNc�]ߞ��O\-rE���-@Of9)��P�-Ȓ�8�W�(�����{�#�;ն/��E٘ege�<�$p��#\;�Z���O<}:s¹ߩ�R�����y�\��͞zvT����Ֆyo���$4���yLE��F�Ii^�����#z�fo>�]hg�[Oi���l%\����,��=��U���y܃#�����iߤT�[�~�\���*�i���X�{|����Q�s@�W��!�ڷ���T�$Җ�:�  ?�RO��G�<��}2��]���fmލ�WS5>\�m�U���ퟅ�W���5k��w���GT�����u�����s#�U&w�;n���($'�z�<$6� 9 z��?��c�铩�n�����#}��.C���H��G�����@-�j�}�3r�i=e���)��VCD}�ityɟ#�o=��m��M�^�#��6��*^6���]01K0���a�����Գ�P�A]�]��D�8��܃�t9Awʕ�8�X�a����Á}�n�3�Ee�d���������TNr�+�܆O�@���6��v&_����t�^�zUxS�bw��`��u!	��q�ͣ�l����v��q�[=`NBLω���%��6��
D��^:���kJ���@��Q����"�o�_}�ѩ��t6a蠘��t�kh �1��c^[��\�m�>���` ��C�n�Tv��m��6��,�yEM���w�dP��.��rS���sʺ�q��6�d�[?���P{�sji>�O>I�r�����H�c�d޻4|�V9�z�����V�wo>|��Y���@Y�~a�+FZ���=��4�GD`�o�T=x���#��-�	�&�29��k��m{p�|.� Dk��&��T�N��7 i�#&l����,��-�B&�9=��9�T���Q$��6���i�G|;�I >��l��j��`�j�x
�Dd�ѓʝ�g��i�A��Sg���Y�-���1�5c��SX ���i��J���;�'�>��B� ���,�u!Mu���iv_��ԑXl�����:յ/�����>�����2�j�<E%��ĴM����1"���ϳ���S>v�V�|\F�X���t�	̪=�Z9�5�:mv��#p�j�$)�M���K���n��<�ͽP����4���0P�/��"y T{�1�G�._E��d��]��JqH� �mB|����j�|.��],;��Ōܹ���0���Tߗ�.�R�"��F�d����B�/t$�5⯠���T�x'vγ�ۄ��'�摵�tá�լ-o�9=&\=Ý����|n�� Hs,�eN�K��p �K�?�\��Q)��2�W�M�q�f���e����N<�5���ͦ%�V`e�D]ݬ&���j~�U8N	(E��Wn�,�$�����I�^��hS68�%��bp�������=e���[������V�k���wJ�;ۙ����dΎquL��;נ��Z���D�T�T����M$��@�㘒���>c$�ǵ�#tӂM�	^��r��q`�-���w�����5|w��E2�h̴G���	���`�υ��2/JXCߏKrw�y���W�����1��F� �cb5�O;ywd؛�&Kmo
�1(�)��%s�EA� V�=������L�&sr8���W�γү8���9�G��QnT��3�j�R�Av�P�#
�s"�/^�'��&w�%
��Y��5��/J�S�1�v�5�<:�H�)ﾗJI���P�
��|�P'eM��A`���P<�S��դ����N�[�sC!Q�TAK]�.��`��N�z�Yצ&�+�@�K��$?�&��L�e#����u�u���H�Km�Hq�1 �:ýt;{Flɜ���+������U�'#B�`׻_}�{��F�ZLM��R�g<�E�A�z� H�+6�~�2xNA����<-0��Й} �����j�S����{�c�Pa���c��q��pIyӎ"~yWɏ���e��aj��1de�1�E�#O��b�F*%�G"=�ʡ��\�r��%'<�I�r���L������\�ɧ��EV�D�Ǻ4��i���҂7�C\`@�3�_�
���d�9K��5�h�Q:x��)��,B�J[w��<�0>��� �����5�h���h�M�ϐM�\�7+i-)5$�����<��	]���g�/m@���O�5	k� ����82��i"v���������3�l��Ob�q�U��jB.#��Y#_s���j�+ux/��������;W�-lr [����ֶ��Q����#9!�W��N�c�!�|�LNR���rU��k�G�VM�h*|4�N_7
�g9M�`����@BrZ�(�&�|]Nȶr�@���蛧`����FZ������7vu��B�-�u^�sD�󚌝 ��`����2A���IQ�I��O�J�lga�kx��
3a�������9lؕbn9ö�����0�wȆ]���Өy�υ��1���Yse��	����,��2:B��~���p��N��z��<w.�dg��*�j�.��~�Y�֬�Y�><�����nc��]�~5����"�N���'	�E��)�V���������3�u�I�>��a|:P�P���ce%+����Ӌ��"C<8��D ��9��SX��M(���_��>��|:Jg��S$�v��+��z3�tIHH���������[[+ �{Ѱ���� ш�q�JF��W_�?�d �[-W�y�@�� �V��� 3\��:���.�{=�8v�TOh�S�$7
�ֲ)�T傱~���F(f��8���֒O�I����T�1��ǿ÷߅�(q��B;�=�����x����,\�XG+-�Gd�y�}e.�o3�^�,��=;�m88�$��%��嵅Cu\�͋��᯼���d���^�(9� XG�/�3�-�b���}/�e�ґ�[��_Zj�}�×��%׎>��􆬟�{ܔ��5k��������"�\W�Cޱ�'��Iݹrz��?��֦�P����w����h�G�n�Sg����m��2K�r?&��YH���Qh{-]t��b)
q�Ӳ�3
쩭����q�B1�hoB����5&�+��<>�.M�����픕d�;�3@�����5"�,N���U��"� ~U��#LKI2��X��L���:�px��<[L8�$N͝n�l�(Ij��M�6@^$��q���߮�T��-u"�c� �P��ac�@A�M��{w��O{=K����X� 7K)}�QYc�#$���N��[�<��M�������.��	�7��lr[����i����6ˊT�6���3"�F}�k #�:Ǭ��=�W�E#�ͼ ӈ+1\���(FZ7o�ʩS�ֽ,����4֬����V"��Y�r�f%���Y����/�@ގi\�Bۡ�x�z�~DvX^v|����zH��U�s��@�L�1jnq4����^��T�b�s����ñ���@��2it��@���H�ޛ�� j��7?xd�\�����I�^��/!MN������u8]9י�~���'r�#yz
��UO����h��6�=*븂Ht�3�F���3��3J��|�f_h���Z��,脃T��S���i�=�������&�Y٣�ڵw�}&լ��+��g̉�C�E�fjlK��zTd��9���x}m0����j��b���Ù��ϑy��C�[	�nK{� ����*�O9s�foM@3 9Y_	5ж�������͟�T7������Yk��ll��6��n�?#k��7N� {��n�uvi'��xP�:8�I�,8��+6��B�8�ڷvBh�Wg��z|l���æqzY�\Z"'S9�-Mx�q�JB�����8�B2�ȑd�8p�y#� �l6}N#}�ú��8ţ�GV�sH�]9�Qv�
v�@����js    �3�GN�����g�l�xm����{N���(�z}�!���T��L⛂{\�"���Б-���@��ޞ�2j��J2cw�ȧ�$v����\�]p�UO2������;w�׋;��O�l���g��#5�ɨ�VEL5�̻�V�r[w��Ma�ߘ:��ED���}�ۃ��ƣ*�F��D�`wħZ|�g2)��T�=M�}�w?�ٝ���[il��ޞ�\�Q�Ќz��qv�v
>��c �-�A^�5����8j!qʆ6"�9�Ms$��ċ��&˰�=��ld� D�
���ՙ�J`��jl��$�8F!��<���% ,��A��<�{�� Mf�ݜ�}D�p� ��:eM�$sĩ@T��}���A E�i@��ݷ�>R�a|:5�`B�� �&�Iy�T���f�R9<8��Ij=�'��g��(2��笲ǞCق��p+�܆|�+����6����HǬ��JW�b����1/;Z����CӘ�>P[�|m'o���.d����.R��n���
7��2�k�e�&�k�ڭ���Oa?��ǬC)ع���Q���09BT��H{j�~Y(܎f4�TAK�sf@�q6�N���J�v�{ԁ֭�%�u�F�9�[�T���=��$���ó�g'������c�~��)sG������ijOh��@IK���1c��taa�����z,��*�ϗ#y� ,��U��.G���D�,�I8���K��6��0Og	K�-��ق��FQ|��U
�6޲�D���E�	�c2_%rH��2��xHs�m^���}�Q �\��{��rz�K(����8�Y��}�K�s<2�Y�%W/j&rC �h�Su�3��7lWb�sǴT�y
^l٤l���j�bW%���&Rv�BX�ve���9ż<k�l[b�YA�r %>���_��}������2&C#Bv�`�F���P��������[���lɤ��%?d9|��*o.��n'W�/�ul��&�U��l�ɡ��>��#�@��^�#�q�=B�H�F��NA_[�ZW����Łv��C^O�|��;�&�=�Z���h����|�꠾N�zJ��V��Ƙ��/Ur�ĆƕO&��Q[�s�?z��QCg}���ϻO�&i�h�?�-��(�[ߠ֜hߏoS���t�3�Z��`7���g�?��ߕ���)T^}Y����n�0뒶A"?}@�f�"�[�ҵ$ɣu��r����0-�:'l0O��\�9��g�m��N��j. Jv�\��I� ������s3<{Џ��gmO��b��i��.�`����.���#���=��w0��m�+�8⺔2m�.��{jS�t��<�h��d�@5��
�N6�?����`�g��f�������"�ڏ����`��d�	䦠Zl󫏀S���<h7Z�m�3?J�3:6笺�O���L8�L �(�S��1a�M��>��yU��G�n�jvJ���u�; D���C?�±���co=~y�m���Ph�jr�N#���9����+�u�_�q�:����YN3��Gc�N;E�~��ߍ2�	���.N����Z�|ǁ�uƘ���5"����᭺�.�\��|̢��f*D��Ge\�<�z��Dĵ���{�#��?�f�[j��QIS��� �S���&{<��i�i9�4�T����{�.l8��W��&�j"rґ��"�;� 9\b�k�U�w�����]��RȠ9�ǀ��QK��g���I��+��N����6_wsy����R��&Y���Z�Ŭ�)��/�2N7����Q��թ���ʂu���`Ԯ©
W +���}!�P*ӈC���0 �����P�K��?�%�+lw/��$��1Mp4<o�q��@��B�GvK��{ɿ�sRSf���uШ ���2�Ql�J�)�����<�UM~�x+��>/OmC6J�Pc��,1�����]&���%�v�1e�!l�PZ#�E����?�Eg�ۥ/2��[��H��j��ll��������B���((6cPd����[���$�d�!�d���^�N�� ;8�u�Fŗ�/چ�����.l�ey,�����c	�E����n%�d*��Ɏ��K�������c����|��:��H�K�)E�Խ��x���Eo�=M����?Fv��3s]XhO���֜5?��i�),�¿�ji�@q�Ҏ������^����N��6�Dh^�T����4�2�mJ�]x�`p`��7�PPG/o�eB�����7ҍߴ~�Fb�x.�X�>;�,l�A���OW�[�[ ;e����<�iϤ�zv�>3��TVע��p	��r۷��k/{�a&@Bsez�ϝ����x��`z��:Η<�9���>����f��
G�RMb_��@��#�����k*�LNĻD7�RD҅n����n�~��,"gp�7��GC4��G6�0ܠ���|������MZ����tI�`NkES���4`�bl�g��_/�",)X�)k�l�de�p�b��]u����/��mGh3�O�{�؊'��B�f3�.��&?���Ex{D%b��Bi��]
��lڑ�$����:��3Fo�4+��lW�54P4	hYǅ��;f~�O,�3ߘ]_�i�c'�șq D�F%t�2����=@1��_M�%��>.<��>)W����SA����h�h6`*���H�=�E{UG߷v�Ӆn�#�t����)�%���?٘�*%�X�hh��*�:�����H�Pj�W���R�����N
��h@���|�)%�3]��T�?r��3.Ιd�ZM0^6��cB@O�Y��F4_8��l���@n`	<:��I�'o�D���T���H��+��F���e*�s"Ȯ]�U�
_>�s/��"t^ /�9|'��i�{&D0o�� T��9(z�k�ѿ���N�1�:������)�[�}q�'8�������/
����3�p��n����K�-[�/�ɜ������L�f޲�k8�A����86��a��J}d�p����s�8�+��M7zH!d_�oD!	 p��|B�ဓI]_5�)��%�[CBF�TMt��X�b���[#��k�T�ggR�G�m ���T+/�_���J�9D��A�4���3��Ĉ����%�h���|L`�5M�G�3���s��gv�}<N�����h�E�hn�y�]����:7.Ѓ	ڃ��;v��G��]q�/|��ؖ�Y����s/�vO}?R�Y�nH	��4��sO~��'���x.�ڕf�i�K�=���q��ϹX	�R86�E�Vo��v��i�Գ��ȁ��,F� ��̷[D�4���N1V�z���8Q��!)�V���N�&��c���`��9@�G��:#¦�YyP��s������NsV���,s��'�N�@m̋�֗��h�㮉���R`һ��	��P�M�1��q�p��Qi��x%�����4���� ����~Ʒ����Q�;_%"A�=���p�#-��ݝf����TX!��G*1	6ze��|��k���3mIm�����%r�.%�͸Vaw�~<�!�kI.Xj��G��o���҂�@�_� 6�Z��'ac\d��/�<�]<vZ<�A��H������\���|���`[k���%�qM6eU�uԠm!��'�
����#2U5����K�������s��]�e��t�'|,�.�4�.J�^Л5��'��Zo�i����Hچ�k]90�Aj��^8;��R ip\Q��Gʉ�U�j�QM �iR^ҷ�8�*�w#�\s���0����ߘ'�z��/{�i����M+��)Zp\�F�fI���*�x�)��Azg-�Tu����@۫Y�8���RZo�y+�g�]i����R]���#;U�
JU��I9���8�A]�� A 
c���o�s�F����6�X�,�e��9�{��mbſt��iQp&%,@T��N3�Ӕ�/&�nӕ��I��=����F�5^HJ�J	�L�9/O[�_��{�qw�S�Ϭ��;U=    ���X�)���Dv��|R�d��W��\Ex�}��j�YV�SI����r�0���R����6:��&�s�F���ˎ��@���%*�R�r�5�
�t�H�����eGc��V�������
'j��G�+�n��qԼ������Q�,��tz�|�|����L:L���n��דи��.�B�)ĈL�:a�Fw��kg�y�t���F����4x3���,�}� ;�}��{����۷\^r\,��]�!�́W�e!NƯ��'��~�q�yY�N"����Z[+�3�.QxF�B�/<���|zT$�����ڡ8"��i9 O�ݐ"/��ju�/O��5X�n3����@�T�^ۚ����7��#���x~$ۯ@R��1|�2ug�Zm?@���>B���2�����Ɠ��!Sq���ؗ�v�����ۯ�Li�!Z
r��uz��m�Q�\)�X5xY䌂$O6�"ͅ��Q��L��PG������gDk�f�ܻ�BO4稽G~+��"���5�m-g&Y�1�$J�u��������q	;��X�H	�8��I0�o�!����Hw�5�&�@ �,�k��<���5���D<��ZW�zF@D��T���֘�,~�lλ�iŔ��nn����P3��?_f�sQ| ��H��� =�ڗsߚ�a�nk�Qe��R[��Ly�c����fs�8���H��s��l|����uf?J�e�k�R�^H���c/rƍ�e��l�I���G5׾�I�<�RN�6�H]�N��� �.��a�6��6P N͢�/mi�y�ۇ����S2d��T�� ���)w����={��o�G�g����dDv�����o�Q�#ň�r7�{D�K�ԏ�L6�؁�MW̓��.SarPr{�̕G`����F�����Է��%��/{�xmg!��F
�Z
�q��yP�O�6��{#�X�鏈�w����(�hYdt������C�Be$��06���c#G��s����b>����V=5��#/ҹh�������Ú���jӃ�\��b�r�X�5��::R~YD�������7X��G����<pM
�-f��-��G����F��췣�/k�H��MλN��S�gD �3\Zw��űQ�y,��$�x�E�3�,Ty9�cb�IM5�|�U�Z8�;����u�ԟE��<͵ڧ�]�,`'6�
Ǐ9k��w͍���P9�\�6S�ɀ�G�R"��H�é�˯0� ���p֬�-9��7H]e� ���"�˯\�Eٝ��Ӹ�p� i���p:R�`ɳ�e*oY|���9���E�w�Ȝ\�vb�J	?!e?-i6�q%.�i����gh�S��`[�nF��{-k�Dh� �2������,����q�!N����8�D���e��Ӭ㽬S�\�j���J+�6J�56}�߄0�؊?��u���)Z�t���p i~EQ��XT�S���QT��S4��th���Q��3� J�Z�-(r���ș�T�T��E�) '_Sҹj�5^g�Ƃ�����=?���(������;/O�NS7'�zy��B�;��&�i�ʕ�kz��c���=@��sN�.�'�5��0;�J����.�KV�a�M�l_�fC�q��	d����~������qc�������ݲ���r��qdn���?s����\��u`K�h/Gz`ݣ]�tm�:�"�%�@�>���9xLM'g��
u(��+��@�&��"�8vł}�����	������a�E!
W:�w�*_���z�Y����'2N�ś����`_&�!X��uch@1���6G"�/�������,,C�\ PތW��l�����r�h���$ m�ӵ:G���t���m��ٴ���G���GP��m@<%�~	Y4���Df����>����ب��)�R��Ҵ�����L7�:�Jn_���^G\��Q�p�3kr�_���,*�U� Q�m;V�w~$���3^���lh	B9�+vF���(�k�14<��=�i#�jT�l����t�kt��+�y����t�<������	sQ�^'(u?�6�� �J)�4H�7F�4*��73&a�AS���jPt/��ۼ��RRͮ�5�z�L
ul��B�����`���o���Y��4-���N�{�^BC�RU�h췧9]k@�z�L��� d��B9���d�;�Ǯ��%�}��љ�I:� @�`�dD.(T�ԡ[bgC������a�v�E.������R�lZc�(�ɷ�؁�zXK�{��*�J��~�pD��ΨT�W�{��h�{&��q�{!\�=eL(0"�7��]o2�ח�u�h����y����(XJ�j/0����_�B8��.�,�zB8��d�� ��z��*k �F_|}�8��c�"��G�@v� L�B*o��i^�F0�[[���>���l��9�=_��tU���h���|_�w�����vL��H���x]�\���QR�0z���v[�:��{�Ԯ���	�Sf��S:��=����9�7�X=�oЬUwO�hO8:Hҁ�Gc��#6�C�y.�S��4�y@u`PxN3Yod�ZV����+;v ����D��h>�;�y9~��F�{�V<Ɏ�M�f�I-R�јq:-����cۻ��mw�?��=if<7ѱ����� (�r���9�@�^�}a"�CL 5%��+�{*���se�ŏ�RV�dS�� {���N����2�*{$�����L�����g�9k�i�!͠��'����]�����m�i��k��棔�2�}������J�̦�Ϩ�>a�dx��d"�M�
�.���b�����}�=�P�����v�@���,梴4{���v�y?j*g΄�M�[���E��4 �V*eW*���0s.$�G����ѹ`v�^��YSea�g�'J_7�QK��ެ�=E'�㵕}̥1
�L�P7�Y��kc�o��)��X��>��ٷ��)WF������m��a7m�5�����	�
[o(�'�6��BgФ�+j��"+]z�P����Ã�,��"��37�qM��#�%dg g�Nq�K���m��-?C͙jb�y�3�� ض��	����*�G��>}��?�ܒq��+ٮ[얧L"��{f����� �q�W.�F�h�z*K��F-nld^��8m Fs\{�6��Ǐ�3�#]S�\�[��@/���Q�7O�M� �M�v���J���!���Ұvj�uWx���(E	!���D�)[��{�a;��8G͋��i�\u��g����)��d6I��m��,xg	H�?��%�:$v�ߝ��ȠPP#	�', ��<�{pã��ǅsf$9|*M��yI�Y���p��.������ ����bR�"�h��v'`�E)�����>��la����!�Hti�ط��y�a�tE�P,�\TM��-{��gi��rc���I!�]��W������Lc�m��g�\�qB�ɫ�Џw���=��	n�?���(%��I�@	su����V�m��W}�j� ��ـ��qR��LӲ�GI�+ꘃ������:�C!�b�{�A�Cs ���k�7�l�J>b�Q��Ga>�QN�޼X%f��Ǉ��r)��np�G�V"PI֥�n�i��Zbz�cWڏ�52hpiu���2���=�5�h  �j�D��&q�Aޏb�A��|ֱu���D��#��n�\׌����|'�G���G��lQQ��*�iӺ�}�T�K��z�݃�v+� ]`9��!SNv7�AFG->����~���]ܾ(���}��D��}��1��ln=� `o�>����6�"y���W�����{O�y'���e#�1���q�V�� j]|.ԗo��s�$��l	�K攈����u���t��w��ӏ��ޱ����7�U�98�p�Q����8JV����]��^r�	_no�ݛ�j�t-f���?�Q:����Yi�    Yʀ�k)�~ma�dCءt��if�o듕�E|��kό��WF��͚.CO������@�U�
�3A������B/��8Jٜ�9*��GpZ]x�U�� �l4��*��ညUlr�&+=��슀�_�J�{�coxQ�
��[����nD!AH����/��^ʇ��h�
�R�_��>����j�2VZ�%�s�t���զ�f'��~�����mW
@��y;��R�����@�UJ/-�W m�[����5���T��T�O��:}�Y�g/U�w��S�5���oa�^�"�����JN.��6$Z���;w�o��K�z��طEJ�<u�5�\&��V��d]�l��-��T�\kZ�iM3�r�1�T����||������^�~������m��}��5���`Buh䦎���-�������Bu"�6��S�-������= I���ٸ���쁉�y-�va���cI�\�݁��r�Tv>ٷ0�t�U�bc{w�,�BH��R��y�^* R:&���F��hX��,�x(�ޘm�|=QG'�J�X��	���|��J����9ƚ^�Ĩ���O�Q���<�4���+z�l����ޮQ/�$&}\=�$����I���:�}>N2�u]T�Ӷ�~�6�GE����f0m��{�4kp��!`�= /��-^����g)��vo��r�ͻz��dPaI�l�e�z*t�>��f\�px���JZT ��#:' �,w�V^ΐ�$5'6�VN0�4�y�e��LQq�E���\���������UXe���l��ZY,���0�8}���`�H���b�>�K;=�r��3��kw��>�s3��lnL�#���Z
C�־�ى��$g,&υ8�ꁎ��]�ʚ��V}��68$����̜����/�����ͧ�у:Ovx+��|̗ 0�x,���B�u�\���F����e�N�I ^��0v4r/��QT6���&}
�� � -=hBD�n�|9����*v��B�9N�F��s�BX�*1���5�O�{.e��#�nq��nHl��TΎ�W����ޭߗ�w��s�:��4��-,B��	V��9w^����P�=!%C���صsW*M����~OlP���_y���,;n��]�tENLGj���0�AY��ē�'(�ˬ!����r�Hdaǭ�N�OÁ�㾁p��Ŏk�H�}C�j��Qf����aMm�o{��R�*'R��}_���lA\��$�'���SN��NRk����x����y�:�� ���!�u!�U�d;0�-Sȫ]�h��A�S�.��?�	��R�x,M=`�q�M���`)�?%��(M:ެ���b��{>#�$������P���g��L��]�K�����6�+U�C�䵰i~���P%��T�l*W8�ᩃ��*����ף�\ۣM�x�Q ��cK!�P����c���C�f�.�ǂT�*[�A(o�Ac*j�#���:�������#Ń{xS�͈��	
�8�Z;c���ҶO8�Ṏ�M�f�ɞ!@�~r�V�8~N4�hȪ@������{رꓳ��j�\��xVe��W�U���R/{�GSܑ��K��>eT)���r��>��Qt�(���]�WY'�G]I��Ƒʨ��D�
΂�- ��]=P��V㼩|R,��ܠ,3��Y6�����i�x��ӳ�/�>v�U5y_�Xq����9�*��d���t��V~�Q�|,;svf��:6��;�yz���D�e�x?5�r(�y��/�qf�F�_S
d^^*���\�)³�b�tJB�������Q�Rs��B���iJ3�%Z��:[�9[�)Y�r�Bu��A��P���"-����v9��­+5��Ul� ��+&�-гh�+��p��b�Zg��,��Zu��=�=�L7�k$hBM�|�**�

�N3 �iۿ��7񃍘��&�g��q	ީLv/��hܵ�ȯ���D�K���;�d0V�S�B(�o�����f�.�哲V�x_)�����B6�#;5;��^K\(^;��Z��
j�ENb��5�|��H
g��?t�4Y7:��E5 �zv)`%��e�מ���i��؂�ev}��l�P�I���Dgd�R��n=��َ Z[<r�,^�Q�~O��V3���ם���ɑ���-���m�*2T�����5b,{������f_�����ra�G�Q��W�s��Uv��{�����l� �Ig"Df�Н�u��&�z�y�e��Niv}��	�&��z�q��c�B�F��=�b�N#;����(�Vw�qB"k�e�ЬN����p��w��k�yfB��n�
�9�8�Av<�9��"&J�й�&��։3�q����Q@��	���&�^�P"�ڍ�=@/j=଎��L_��%�9�踫&ӌ�f0�FN@�n�ೀ�߹��@��0?3�xvs��~D��p��(��98C�^�*�md�N5�Ů�/GNx����P����̏�����)�X�:��D"��T���c�w�U����?&wZ?�3���qvH}`��L�����5����X)� �Y�f
;��A$PO˸z�m��6@K�h/�����|�B�
�8m��=�nđ���os[��b�ҋb��vV�) ī�. w,�K��a�~*և���c����|�v�~��O�N�i��ĩ�ǃ�W���<h u��E�\���݀�=�1��,�.���8N��*mhǮ��zV�e�_�y��Pf-{���pU7���=c��4��[$��\�E��`ok�_��3m���ʹ��`K��/���O�7�Ũc9g �P$�&����!����AxKȌљ�#ֻq$)�#��J ���îT[q�>��Ԅx�&Gu�OS�8�S<g���wu_� �����/��J! �I�y�M�S����ί(j�ٍ��Kj��x�} F[ <���aQ}16ڵE��=�/�:�&2��̨���0�mmƛ��D�*`ĕ�֬�%��n	��<BS��l=�^��M��h��~�;�sM�C19�6D�^]%�a��83.Y�c�ޜ�Ep�8�i�#�b�~&�8a�]c�\�����K?�5��悟I�48E���x��w���/ç�9Sa��mj�}��]}pP�a�߫/m�����c�xČ����N�R\��"�"C���xm�h��8��
?�/��B �}bõ4����@�p��-2%�@Q�J���, [�a"��#�S��⫽�
���0���c圱�lP.Y�ɣ�v�1D$�z��H��J��G֧uʆx^�.�v�?����	���*�(��3^���T 9Vf�CF���C_�أ��bK�aÌ��8� ��.�1�0�؍���X׬��sɫ�B��Vv�ʕi��j�y=P�ymzd�l>7�4@��1�����8!!�� �$����`�V,���7Q:�:z�i[��d; ��v�U�Z��#�}�Y��������XHY�x`��eV Ub��;���:z�tW��],�\}�4G8�>�CO����v	������\���~+g3���l�xH��z�We��d�b�C��g)>vt`Xc�#�1;�}g<~�@,ؘ���F��R�I6����CF�����������C�6JK.�#��DE���h-�|Frp#%3͖3*=uF*k����qʚ����}
3)���'�v�H!P�<e�=�{����@1���*��,�߼��##xAFh��6ġ����z��X�ҷ��)I_hJSµ("�hN�w�Q�Bé����u�����J9V�|+6A�2v˥��������|�M����<ݎ�c���i�����3�O�}�����!�"�W%.����Õ�ߞ(V;���:�o5^�<���8��mW6=�g�򨺆�fz�7+�j]��./���Pw�ae�q)��������x�7�.��R�[s�2�@��� ��{VO/�c"�����9��Q��z��
    =q%v��f����Ǽ<#�twbg��y�B���)롗�nq�����ځ-�\$eg~�
�����M�6N�0��6]�B�fB���f� �*�9VM�o�T��<�]�ѽߞ��"���]��
|���Φ�y�l੔*6�x�m�M5Fg�������/����mͥ�m�p��i_i�6a��9Y2��Ճ-�osLԭ�@ʲ��ɮ|�{l 
��K�J�LmS�#< u8
O �.���L6���~n`�@>7H�dg����B���j��H�<�עyb�Q��o�B�o�&�ITp+3r�uQ��j�'r���O@��<�_�	�m&�6�4x!i��ي�咕Fh.�V�\��>��Z�\t��a����(� IYo��SEfx]�Fx�<؁��/��7i��0F���BG}'��&���)죑���v�6^iWd#��=�)�s��,����ؕJQ��>)oz'�����x�Rm�4�2F	����B$W"j�, 8`�X��N��U�{2vL)�f�Z��GՉX����*��x���MH��T�:��H\��G�R�����#O����:���7зJY񁍖h	�oG2��G��",1�,B�0$P�,@̢Df8�\�Y�yǇ�*տ-tZ����.���r�,��2(��
��z�6z�`>���"g�8G�SfÇ𚢨^�_s2�����|��IE�F�0[Gb�s���N��p��df����<�x:~Es)bj���.U6�8��mf�(g	��J�>JԧI3r�oռ�clT_����8W��#���V���6x T(��ӓ��_�_��䎦���
�8
��F;n��NAx��9�Ў�/g��|�8�T���G�Yr�ӝ+#��->�2G�daW��"F7����_6��g0g��R[�ە�X���+��v@!�<�"�>�ΰ���s�=Q;�͜y��޳V��K��q)j�y4������[��0���j)._�K�ԣ��c���kj��UY���|.c�Jh��c(+z� G�L�lTŹ��V�Y����^^�҂�#%�oIC]�Q��5N�Ÿ)���>p�p��#_߮��#�ٻi�(s�;*� �ۼ�U��&/G�:��f���T�-{b���xJ�a�d�E�G� �6�;�-&�V�J�O�]ֿ��t;�%�NO}�����Ӌ��x��b��_.G���Z�(�=8�?6��U�-ն�l�}!g�{ P��Q"�f��Y;���;n$j�<lem�:���}B83�ܞ�^�M"H�����}%�zJ��v͕�"mݽ��X�? �F�'o��|\�v���H��:��K�'g�(nVF�_��ч3���w����A,�X�������&^K��{*��?�h����s�,��KV��v	A3�i/�(�Y(�^��4�f���ԣ�\ě/��q�lEp6o���(oY�<AX���/�(9�\1�E�0���jG�a)¢yP.U�`�H������[4�j��*T0��VN!�uP������m��,�>g��f@Ѕ`���������g����BH
ѹ((�m��F��#�HE�������G*=�t��[���0�u� ���w���3��˖��A���_�7R'�	��3"\w��{�G>�,���6�x��l�rZ��\i76��$ҷ�6�^[�pA2_[e?�3�F��t�mU�o��u��=��m�Z)�dH؈-�������'����E_�=y�i��p3K�u?��t&�*�Ԗk4v����n�\$�ї,޼�\Q����9]ΚA��ҥ`��g��9N.x�jn�H�Tӹ�(�P3��{Ȳ�_v�m%�#��$ˠ@�Bu���[<E���
�'vmy^[�sS�8p��j�����ݙR�B�t�Y֒__�{�ð ���;�֩������3I������D:2*���/� BǺ-7)
���-p$6�;���yYQ)�ME�������
�wH���d�7t�{.�n�Ov�^���ǧL�d��>�uyud������J�R:<X�qG�JG_�PX�Z��$-	sv��9���E�]<5��v�yY7 ����(���p�e��0�%x�[a�������=�A;*v�n+gM/�:aF�~D�d\�980��W!v�07 �W@X��D�H%�7S������l?�TO�=�;j���xn�$�gH�{����<7"x�ȾZ�`���-~���J��2�o$(�3�[9|�3�Eg� M�ck�H>d��kG���66���u�L�� ���{�Iz�ǹ9��su��>%�{؀��B����x�Pq�e��vΧ�V>
DijO�QX��;�4�o:.I��[ڃ�ݲ��2��\oe�����K2�P%�^��Le���#��9{Eͳ5��A�1F=�l�*��ٯ�,K�.�! �dyms�Y������o�cwl���%Q���k`V���p ��5}P�B��o���#��Gvr��&ɵ ����;�#qU�h�@�I�|�y�{O�uK�.����9�1��ۓ�л��]�'���@�]Y>V����qV����4}���63��Q�Ƭ<M�3�폭xMW?�?�J:��=��YH�a���;�;"��-lx��.�*(�gx'��0���w������@�g+��\o�k�1ΚK��6��B�:&�&�ܨ��^T����`�!���Eرp<����@�a��*��zuL����옉��ݳ_}���e\R�#�B�)o��LdXɒ�/���ƒ��i	�;} ��·4Ā2������N8�[�k5ujKulR�7p
��c,Oo6</����g�@i6��{E�*N/�s=v�z�Y��#���S��_axO��͂��څq≺V�t�������Q��u!��<�%�H��h"L|ze��kNoԽ��S`utj�V�ר�o��d�d�CoǚÇk��`�e�̲
��7��aݜET�u�G��X������U<VkΛ�����T#W�4�4�@�tNb�=H/���"-y:"�^��f!#�P�,n���|�7��ʛUPPF��
�5��E�x �����D�����M�e��MJ��c	K�����sW�R���ii����L�C���8�2'�9�8u9W��&�G�T>���IA�?������s��n�J�������ԍ6�9�9Pc���t����,�?���[� �P���@�DJĂ9���xW�SZi�U��[L ��T�&s���Z�a�����^�!8�UZ �F.�?f������I�%GҨ<G?�� �X���BMH�&P��+��yo�UA����p�z���^ّ��VOvٱ���J��ڛ"�G�h�� 0�e�-}^j���hߋw����P7�=(nP���m�}iTh��rC���ý�B<��\}Q�/U��4��iP�`n_�(�#�\q*�� ;
�>'�Z\$�F�{&ѱ��n�[N����u�y�4Rir�?����X':�i7��EJ@ӭ�Z���t+6 f|����n�ٜ��i�R	8՘xr^H�#|P�����c?>ٵ.�����[�m�2�%p	��]�z��{���֦A��&�W�5\�m{���+����}�o"�"��Q�|�eW��
���:�����J�[���_h�E��[9e`I�י���azs���C��q�o����1hnD��R�W�P���ugZ�tW��,�'�*�r�(y�"~Q�Z9[WX�h�S�]����}�u}6��#���O�A}����0�ŘW}�>r<kᵢ�>��$5u��o���\׎��G����q򡐺iP�#c���:��]@i�tf��˱�W]��Gb+��a]�Ҷ�lNͅ}4x헾��_<H@���3E؀jV紨���Z��Ç�B�oOS��wՌ�˺&G�"���kƺ��=������E]�B��g��ɕ&��iXh�t��5���Ju]�(u�-yTԻ9�lk��+�Jo�G8����M�E@�    |,��@]�d���8��H�h�Z��J#U}	t�+_}6���J���>qH���{s���o�3i6��%+? �Ֆ91���9@<@�nlh�����ɻ����u��[>�K���Ap�|)-hV��WI�E(�_��f��.+/��T*��Q��]���D?Ɨp/��T���N5���@
�l�  ���3��_:.h	��&�-ش4���=I�3(�LS����]��d	R����-I�u�6�4��|�yi�BZ�*�q)���lnW���	��cfg���GOl�	N~kl\��y o�o�\`L���I�n��ݻ��a���vs`�əGy}{��h6_�ژs�1��4O���_�0��XK�	�3�q��-���@�\�"�+���X����u�[c���>�k`7\!�L"��)��m��Bǉ[����xg3{�H
'n��
�~��P�췒�ۏ���t�J�<hqy��@_�����i�x${�7�4���օ��cz�P�����t��[njy$6=�[����X�tLx��?��FC���͹�U]*�Go�,�||��� �{����6w"�&��v�y|��t�GsA�ڼW�`2	'`��2޹�[,�sA�SͿ��[������#b݃���'�7"�=�&b������S�II��fݼB�;�=
h]�~u�/��]���D���&Y��#��^�M��:B60R5�}xB��˹N�����7�uP�e�A:������m<�@`���1g��$�Z�چ�G�HA.Z 4Q�d=�A8��T7T�<����S��!�|k����1&^l*�Ǭ��B6�2��&݅��VsN�/�A��S����l}+���o\8�_��XWr���e���"�,}ѝ����Mb�:���h�>���s�,v�)�f��xP�h.��7��&�� ��� RX�㏜�s��u��������n@��; 4����*4�������������AZ��P�-i`j��+a��x������.�.w�8���eRټw헴"���K}	l�'�,�L�t�׎�)`nl��=�����K(�j� )�t~�Ю�����ˁ���MQ͹_���\n���ԅ��Cj�%�7n��id��	
�i���K/�aqδ�o�o�b�4.�G�c�0͇gU%�Q�3A9h��2���������`U�;�Y��K��eq���Q�1�[Y.�)N�yzl2z�2z�&h��&��Қ���a�^r���,�� �>�!f��g4߇��B=6�M@���)�;�#B�Tz��eDVWͭ��.4F^S�ƞ2�i'0�gy[H(N�ĉ�A
{M�(r�_��ɒG]Q#	�����_�s�I骒1oPf�mR��̈ wpǁd)�������?F>�.�ЉԮ��!@zu��o��\�L�#��G�[^�|��{�d[k)�+{:�����+��6�1�������M�- B+��7�a�����B��R.vp���F�f�M5�G��P�3)�-��Oy�U�x����?3���dG�8�MEC�
6��>f_���Q�'R w����rlN5����N
��\_%����)�Y����͡W��)�[eo.�O�$���x��rr����~w�0MG�/:��B���IU?^�4�t
�u,�R��Լ�oǀ	'stg�q�5���^�H��RB4�ܺ�=NK}V/T��j�W���ٕ�צ$!
<E�I�F�X�= �,��n���ܠ+Bi�CΙ꧖a�ܸ0%��:6��6E��|s{�.	���i��Ӄ�`��e*(r�������I�N}֫.u:���$w��1d�Ȏcx���ꉞ}���g>�q� �;=�.�T|�ʒ���Tǡ��-��	����h�>��Q �r�VB����IY�Rd���B���V,�pv?#���}L�ΑWP[�f�.�ȆL.[g�vd�;M+�m��eG���Ε�%���:2Uq��I��ΝPsoj7���R�w\���a�����1��C����g�SVz96�I�g/���G�Zs�q�w��jr���K�3l�/ЮG�'��:�Hޜ	���W��� 8�A���/L4s!A6�Uy�H���B(�$��8�*3���}\N�����Y�f�c�Mq<��$���}� ��K��,Φ�@0�s
ȡ�v����8��([�8����������Hඒ䰅Y�k�����ߞ9�����,�yƴxCa�=�NMᧉ��<&J��Z5����iW�����iL�`3j��A��F	�g���#?]�,Ș4���(���+E��e�,�8Nɽ<MG���Sv4�۳u�T����2�_���6,��5�|����R�U�Je��Yڟ|���LX�钓 ��I-�m�н8&.��d�R�U������e������C!.�4�0-C�Mƾy`s K���	�Pf������0-djϊQ��ymK!c���	��K�&(&��9��#"(�8_
+o��EN�>x���f˒A�� �}8,��)��;����ml�W�Z�y��O����8��)��<�C�ї�_-�(j��mHL��.^'���ð&"B�y�e!VY�HE��T��8&���y�:�9�� |�K��f�)�(�V�!!���H�/��H"Х�rp�G�ߗj���<�j�[�1���yߚ��#'P
���h�9��f��^�2(][��	��E0��u�ԴAu�5��v��B��ܝ�Q����G�>A��!F ���!t�?;�����E5���Ḍ�����=Q�'�Bo4��ߟ��]
�M����s�@�m���Ӷ��h]%r��^$���s�̅0h�������F_S�	lʿ�
��bA��I�أ) �t�JG`j��R����},���_��)��om��� Ł�LӢ^?+)�6�	u �@=fY��%��"&����G�~��M�	����*Vr���z�twlgY�f�qT�(mf<o�_�*�V5@^[��oVtXxk��my���k^_'�k�3���w�H��5TQ��Wؑ/Xx��XB�����%E{>	�믽�
|��7�;�h��3��9.d�	�R��d;
���A����v��_�
^L����?ٰ���(p��}��/m��"r�*��}5g!��V����)�ũ{󚵃��Ղ{Y�䱰�=�'�]�d�=KYw�VE�<�	�������/Dך���3fDT�� X�|�E�3\����4U�B����W{��X%וp���#��1�'��P��veU����9�d���A\��	�{b%��H5�Ň	�W��a3Čl)�M7�ȟ\�A����.�&��F�l��6M�S�dҤ�-Xp>]:t�ne"E7�f!��,�ʧ�1#�[�C�I�>�!�8Y;Eׂj��)���q���c�yG�^v D�H��1����;5����cvs���aIJ=�9��v.��Μ<7���aM	=�ǝ;�J��P5�_'����GS O��_4=�G4���[��ʦQ@�Y���+��������(�Kl��tD�G&��n0�?��� "��xoX}�q6��-{�����;��[��i�Q6K��rVq�ʅ-|�}$�H�������to�Y$��F$ߚ�)���h0��ϻ}:�3GJ��x��_K8�v����4w,�pH�}�Yv]��D���i?�b���j�m�.��V,��.�g�eS��F��V�B��h����_�7��ހ���)@Ʌ��Ǐ��`#���~چ�I��d�ڜ����z"j��r⤘fc�D�����V̜�u����$��^$�È\�E٦�6�������8�_����r��o�,���F<q���<xs ����@m����
�����q���ۻ�<�M�<q�J��3g�,�cWWz�x[�jYy��?�����6t��`�}��L�Ȳ z�^��t�*{�7�j
C"�<UX��,U@����=ݑZ~�H=��i�t��A�d��|�D��F�k(�^�Ӝ�J�(K�{/D��N�,��m��^t?H�n    qe�+�|��m��`�%m+&
&������]�NS�<B����4�x�Y�Jf4����-���l®>�1��BGm���mff������т��+v����"����f�~_Cy3
�FY���U�6�	
��u��S�C�g�fl�_��5"u�4���gvS��+|Ɵ��e��5c*�$�,`܈i�K���*���w�M�����L�3���|�E���t]��+�.Vz����?�2�&J&�NWe�� ��?��tE�s���ҁ�'䔴��E�8� $`�Ǖ�ն�#���C���3˙,��"I��Y��]�3�̑1�^��(m{� ��H���!�f�J-�3Oz�$�>��j6���P\ f��Lse#�=ɗ�G̱�{��S���<�qs�}�t��hp���D� �����9��^��3q�	1����8N*Ph��ގ��g�G�^-�����1�D4_�ko�%�b?Y�g�m�納����y�]H�q+p�7�\66�������&�����JsG[�r��<灝=	!���%ԃ��v7䆸���W�9}@&�\�K��zo���t�,y�MRL�s>"rK�sL�>E���v�g�7�!M�:v6�(�@.��Q!��l՗bև�������`���No����q_�����%�: ���m���
<r�;���K��}��c2K���hhxka5*'��Ii�_�9v�5�z|�)~Z�]�x���F��3�H��3��;�*�]����l�4�~mm�٧������ng�8\�O㏟E��}�.����[}�u�"L�F�6w��@��ꅭ�xw���ǰp�tN�B#�D�20��`�2�RW�e!��rl>���u�*�.M,'R �s
��.@����r�M��Zyu��(���h46�^��)	�4 �A���rs�XAU���Y9���ɷƾ���SO������%������d��a4=�A��J���>34�e&�w�����-����`��@��8��-}�g�h~h�.�Gc��&f��pZfv��и�qT{�nS߻���\�����N�Z,�y��G
9zV�#�E��A���X�I�f�90�~:�!�ag��I����	⯢:�c��8�f��!�^���`�aTP-����u��Y���Y(�O�A�η�p�'"@��b�%���F���(/�g,e`�n �D<��A1����`��7�|I/�2�w��\�9+�c���Etx���4�*NQ��<I��P��/�=������������9�5r���]����v����;-:��w�+@F�;��݅�h�K'fB��E�V�,6��xoyСx��~����~��ͅ���x�)>A��]oX����ߥ�M#fh}��
�K�u_�K�^|���ϟ�M�\��.g;?D�E0��;�J�O+^�ܱ'�3�Ca�h~q�؈�~��C�-�����v�)����ߥ�xLMw ��/�ǕI�����3�0��"�.]�D�~j��qR#/FG���Lr��d�ї���K6a4o�h��]�7���t��r�(�7}2/�Dk�9xS4����I�Î��!�د��yV5�b�X�VÚu�!�����t��E}��mz&������j��+��3�W^'rr;�Mx����>������)��F�bB�����0d�1^����h���t늽��S6�j����|j S�

�;R'�fsBN�
�tDGei*k$N�;���J[���G'�G| @"Ә�h.�/l�aB���w	[Np�_��I� =ռN�H�Z¬�v���4>�-_��~��E�����nTsIV��,x\g��Ļ�͕�v�����˝��6������ƶ:�j���wz��/�����Ef9�@�l�3����J�E3\<�|lT�B~�/�����s�
�]�;Ǉ�X���7�4�鹯��We��������F�it5�<.kz�����0~�hC��>�{{0d,�J�]�s�?Ke����[N�|���º,R]6O��� ��B&R_�p���8��o�AG�6Ĥ�듸�Q���"!_��.;t
 ^��+~�>.�MN��B���7���a���rGʩ�y��d�ڜP�s3�bk7��q�[�Z�����;����Ze27S�� 5�k
dÑ�u�������D4|������ٯ��tf��Wio��ALx*&���,�^��]I�=��<l|(P��(^����tE�ิې
 :wgy���b��3�V�et���lwܞ��'�t������� ��������[����:O�bkD>��9=*�ǥ��AL9�^v\LGʖ֬�����M�rҍŲV�k�1n�]�(uf����u���q�q�6#��K,%ׇ�#���_.�(:��d�*!A���X���<c. �  �uU��n�SU�`��0Q'�K�8qF;?�J�����Ua�z��3�h*��z�նgu�Juf@�G���h�W|K�@8�I͓ �%�| �F�\�.3Ň���Q-I�os�0�(K���߾2Z>�QF�ndS���F��j8���y�`3>�gqZ3p��.�$�EZ��ʼeGe^5�r�Q�o�z����������N�����D��ۜ��/��=��ӀD8�۵�5��s����b�c� y��y�Q�J���㔵�46R��F�QɣĽ����Ҥ;���q~&�~����\h�
D�+s|���#��|�R^u�\şo`� ��;��z��ڟ(�F�-�Z�\S
: f/<'d�.��]���b8C�� ��5?i�*;0Zg���g��t�>��X^�`]�X�HE�jc�pt~�Sv:�AFdA�p��G�)}����7�co��Zx�HQ��J�su�[�qs����j"�9 w
؞(*�b3Ќ�L�* �}�|��A�������ҏ�b���3!�Eڡ��5
�r Z��9�^��ܔ��q'gH�bC��)�D<�I����}8�g��h17��Z���L)�{䱌`����u[y���}�1�����I ��u�aw�m@� 4�Jf4	�
6�6���&�s��c��V�M���>���.���~��[����s��;�����[ i��^]���H��a9�:ufpy�v�:�,e��q��ޞ�}eJ
����,��T�B����R��1�z�3g���Z�?�����/��CA`�L��T;sT"-�!&�G��8�X~O��;�0LW�&���*�&%'i�=q�Ծ�%J��a�ϕ�̅��[L�H/+l�/�o1|���S�U�wD��HV���?z�.~��8N�|s-yy*�e촐��׫�b�o{��"}R���ﮙ���<T?�,�v� ��ql.��-6��Xg��e�g!�W}s�M��0��+(5q8-�
�	�NHs��m�|������Qv���xi�{��F@�/{ '1E�P���O������8�	:�QAd/ ��9�@��OT	fe�B�+��3{N�y��uZ�%�����]��v|�qe�
`��a{*�SpL�|�Xq��ո���	Aɛ���M�����c�����#�����:sS�j/^�4��&��y�ë����:յyIz��A�`zJ,���o>H������>W��e|Y$��t��h*W�,VE�
"e&)��$~O�h�Bu8���.�ٮ ������p򿖕x��x|�P�Y��&��N1�LD�v��hf/yO<�n�ߖ�i/�'W�M��S�q�~h�6a7q�T�5\�]H�����櫒��B��N߯8ޤ��t�����L�p#K�`8��
B}+��h:����mrԞ����c6}΅z�A�_%��<aΑ����k�	/���(l�|
�)4�.D �#��I��T���/�s���*�R�I��#�������H�� e&�(��0jq�A�`�ű��z���L$�Ǽ�~��#�-�h8Hk����|�a}/f������u�y�6�p����D�}�ʜ�.lE�pVo,�o\
Pߤ�^���_[57u����=�P    :�w��������gwa��B��Ču*ufǢ�M^���by���V�����BQ��XM6�n�Q�f���YG�,��v��>x[D���W��桳q��rL��.;����9����qgJR�bޖ����E�/��]x�6�E}�֬W�,n��;�Q�k��n'��o�jI��e��~*����L��Q����4��-�� #��"�"��M�\�]�"� !�������[��_��o��R�eB4OiƵ@J}�Zz!��Z�'�q��v�Za�)�z�3UU��Zʵ<{r؝�@�<*M�/%�=��U@j�Tc�9EgS�/ofĩ�r*,��[y�>Ԍ�PҔHg��2gSM���yza!\�D�ދp$�~�y5����Ϊ�����Yu�l��0䷡�?���"�9���1����I&,�3�66�ē����B <�v�)�/H�8�� �l�ҀoXF�_"M��)��E�5_f�4]��8e�;�Z����3��6˖j��ѝ��/˩ܟ�{6͠�p��e�`!l�_���
��U�y���U�f�06-��E��if(8�)��.� �fMN�����m 8���ٔl���Ĺօ�~Z�<�q���G+�]���.7���T}0_/㧮T'��f7�WV=�Ǘ��P咜�B�OD�WӰv�oLE��'
V_�\�%������f��[��ht��<I�ia�գ|<�z�B0 W�O���8�'B�Q�1��7J[�מ��Nu�g��L���fڅ�a�ޱ'fh-��N��s��|d���[3*)�mf(,����_P|w����B�ނ]b.��X�ZgJ*<����#3�k��g!=�I�t��h��G����4��	�����9�'&P�l�<�^+��e���6'�r��[���_�%xS���:����}��^X��}�;g� ^���A�۹�l)T8�B���'0Lo�>�J�Tb�2���)ּ"B�r���{�շgs��ݿK�>Ʉ@ꢵ(�\���JLR+����.�OB���z�\�m2n�H�0�| q<	]U������g���+���@mh�["0�D��?���+�����4�Z���m���	�v����G��s�bר�0���k)Fp���1fU �X��-{]��gP+��c��xRe?�'�&V��7hӴc^c��H_}��Ԑ���!�e���2ŘX藱�N:o��KY��\J]�T��7�ι�B�0���&��o��e�q"U=��ҽL����Yu�̢��kk��hÅM��1;�%��!�Mu�Q�M���oKUOm
�{çV�y��4�N���5}��`��}@�y�Q!9P*�1�QƷ���{���8��P�O�"�>*�âA�P��&��mNSYXˋ����ߥ�ǹRMh���R�0��"`��A�@S�YKϯ#L�+P���I<U%{x������$(a�Ҽ�WHz���,�n�����;z�����uw�����6P9vB�V���$�/k�PI����u�~�Zq�ȢP��a9F��rǖF ݼ�n`�#^��nH�*�%2O_��:kܵ�s|k0V�,��4������Mi�F@��	����'4�!n�5�HMxØ�:3�ME%<V�$Í��>�Tb��|��u����W�fk<G�S8H;xo�1g���ꈣKl$�o	pv��=�v͉����.b�i��Ķb�jp�w���v�<"�N�4E��Ǧ��y@�����W'//XE@�cM�|�$#�A��+f:�P�gAfU�um9 ��:1+��c���m{��"}���v}�	MN��7YZ���ڧL'���K��v*<`�KG�"�ˈL��G�x��䯲<��"܎�1^i������$x��(cNk&)37*�N�6���&O��V�r�"��-J�:S7bf�i��	�b\Hd[ʾQ��w��&��;.y�M�t���
�mc�-��C��(=�>a!^�
������j�v��Nk6-��:�Nf���<L1|J�ð۽H���υEV�K���{���_�g��a�ޟ�@��'(��Ɩ$5�������R��pR�@il�@�*
,��!N_�pp��@8wf�ٖ����}8.�&m!ӊ��Z~9���i�.1�d���)���~��[K�hm�v	��u�\��k���Y��V��(/����1z|��!��*�`����l����/�>m������A)�Q�˰BF��fl������7��!�����+�o�9N�3�r�s��`�`�n+=�G/N6��K�h��U8�f�a�t�E�y�@~Z�'X�е��L=�H��'9K�ɡ7 N��M_�ZA�6���[c�����+��P���k�D���6�h��D6-1�ַ�ݽP�:ظj��n����Fi�#��	�V�9�oH�խ��f+���>:[��=a�ɖJǕ���uZ�9����A9V�m��K�kq=[،�6�; @!!�jV��&���y�H�4a��O߰�
G�AA%�xy&}����Y�Td��}d���~x�H���V�#8I�4���V��m*�0!P	�l>G���X�uo�?�Y�#R["A���l�y���?��M��۷9Y����S�[t�օ��i�F���;[ky�G��Pp��4~tV�l��{S�E�w��W��"1}\��y�1z��R6���6��ڈ����r<�r�5��
����'6Nr���I�$��>��(��D'�O�҇��l�(x���W)t���թ~2��"��9�+��L� ��ٲ�M�/v�8�g������U�D�����g��k�����եh�r�-���=����3]Κ8m.����$����f�M��$=5��m��Tj�:�6�W^6C�Jjӱќ�JW�`W9r���B���>�pS��Z
�Z���E��<~Z�8�.�jm;~��^Α��"$�^���	 �'��5}

;��o�����o�GQ,���Q����sS�"NԤ�t��k��*��n��UV���+E:��1(_���[(�#�������_��0&�Vࠜ��Y;�� i��$v�@�F���6D�c_Fù�㢛v��?�ǡ���)���x�зg鈳K}�byO�3Dt-}��J����VC�q�-ıY��x�p������uTY���nh5'�Z�+0����D��	 �]Xŏ�i���2n�ӛUT�v���'	O�bQIg�5��~xY�jkF�0��j������Pv�L7����/�Żw8q����q� ���3��u�
=CЁ�v�Y�P��f#�_c��d��
�#) ���6r��BTܡ�%Ҭ�d�5�4��E�,Y,�w���H����|��t�0l�$M�4�6%;�aP�R���d(͹�+�&�nD�7�`v���H�ʗ�C�׶h$��J8���u�d��*����4
Iz�w�&�,��M��K����u��)ԩ��s�H-�����`��=��b[�匞R���6p��85Y�;-M`p�:�B��u�iB�?���R1�Ph{\�ɱg�|*�k:_F��pҙh���K1W�C��?���N�<H	#�<�Bұu�h��Y�+-�⏈2�Fwz	�A�Hrw�q��&#�\֤$0��f���{�V������rF���B�_\��T�7���N"����N��.ګ�4�,$�����΃;.�7D��Iyzڱ4�}d��/;;=P�h���'"I�9�Uz*:}K���%����r��z�4۩�qu�S�<HD���`������vI`@fU���U� E��K@!�z��ec=��ϼH㪃ʬ���Qm�i��W�����q�6�'..�p�����*<����}W���A�-B��d�Ȥ��m�w�����;���S�N���|xҘ��9�b�o�?��Ҫ�x�/�I�l�΃} ����N�K��ݜO:�E~�Bl�u��(���,�V�_�^8�S�W@S�]�$���p'��p*�3�i���    �����6RbҸ���v�|1�`��I}b`1��2�$z�n6���,����"#Y^��*t����G<��k�`S���;��E�����]ٸS��'E�P�?����X����ٺ�jfX'��v`��dO������P�#�H����j'�m�p�����#õ�3$�䶺y{�'^4����wW���!E�AQ�K �j��3��Hԉ?�������O}7p8X
9m;���^�Z��Hoo0�H^���� ����6�R2 ��מ��%N��$�b0�[s?S��kJ:���Zl��/���QW�i�9؈<�n�)x/}G�Z��t�Rz�Y��+KA��N�{����-݃>�܅e��4;���v#
(��6��*@�q>�y���t@����7H,Q��� �\���XaS`鈔�u:먦�8e��)�Й�Mba	ؿ`�62�W�)������в�_b�.BK������Ο�I���)k��d�Qz]��2M�� y��a晟&iS��(K��j^�Ӗ�ac�Ħ���N��1�Q�:|�N�vV�c�s���8rq�c�A�=q�<�C � w��~���P�K�ˏH�׉�z/��xa�����ȭ��(;��Ѧeʷ�ЗV���2*�Oe5��ǉ���3N��Su8����$=�v%��*^���s�
�Z0hN�g��@~��32v]����G~1��F1��md�VxB��v^��`ȹ�I������q!�� �(:(T�ڰ�ʭqH�=�VXp�]��/Tv��UON��O������\�1��K���R��6�R�8e�ub��VY�q=�5��Xw��i�W����e?���MP�1h�@�S�բ���@R���,H�2&;^*x�!��g7$~z�mw5���Y�m��C��3��������/��})o.�{2�� ǡ���~�K�fZ���cO��X�n,�=�a(\Z3���gk��T�n~*��S��[��?g��JZ g��:;�U5��.Έ��.��H���S�2� DE%C�D#ڔ,z!�f��0������`n�Α������Z�NU)��8�=P�BD�W����<��㏂���3%g\�=`_�JB�(0�G5ƿ�<�#����8���M�׮P�9}��q��R��l���R��Gi��~А�"����ja#��"�e��������׊ ��1��;����u�^���m�̃�u�*^��b)�Ɲc?������*}��qu��fSsc�-O*
���XH��B�ȁ�k!j��3q��
��vF�ˈ�T��M>K�yv��Z��B�2���N7֩�FB@��%y����e��#Bq}s�)'+���Gt�S��<y�]=KR�.ѿqT����\'G��9��n�c�l�Kb��\�O�����'��/��sM����S��a�_��'�D�����}�{)���m+�U-R[��#��Y��ը}���H:JE>�/��qԚ��NR��KaE�H��=�o_�2I&�����\���Ȁ�N-����� ~�b_#���J�� ��wE��ɃF}�X7�� �w!�֏I���P��`��=�-�v�,gMM_Ύ�������T���0����'�BW������ծ�]�
ʀ;9:k4(%����$0�O��p7vv.���*�搬�y�(+��qǿu��t>`�)�@��k!��MvOC!�Joi;@Q$Oڪ��{��Y�!�J�x](
���[�}�L꜄�֊�v��@��[���h q�-��%��*��vW;qڊҵ��P=�!�uf�2kn�c��O$��8�]ֈ}&ĥp/�J
�5OUw��Y�������v�Nn��5�9Vf��j�'a�����9�~��%ҫ�u��_vf߅���|.�n$׳,��]'OQO_���V;�z�b�g79��T�"�]���{D��I�3����e/�~=`y5ٷ�C�	l�1	��ݮO"���U���9d�Y����]>��U�]i������g*T��A�,��6�>�a82���z��s�-e��lK�C��,�d�z�=�S�<R������$w'��<g�P���y#D��Q��^�ܗ�������2������kLb�D�{�������)t�M '�@X��_�'�P��,��vt �������;�Ԓs�_(�,��d�]z-�i�0;:����}��4
 ^hD�/��77 �!��(��n~��}���1gw�!��@�jN��A@[8�U��3f��[�;�5�P?�S;����/@���A7��PIft�)պ��ێ@'7J	f�1�xii���y�(��������]���ta���A��]���t���F `y�6l�w8�����Ƌ���mXh��=�6��������"	�'��b�%���"Fe+�@z���	�W=��H$� <s��5��3"���m�S���{����C)�6!�ۑ�G,��]��I�VH��w��Jr�-��2E�[�5k�cK��!��ۮ�ˁB��i��/-�m[��r��)��'�� �"�}٭��C y���� ĺ��X���<���a�w���H�R�V�.Շ���M���ލ�ۤ�
b�ꋊL�b��*�;��H��6�ܙQ"QĶ�gH��F�t��"&f3fi�����HL(P��E׀�����(M��
����=@�rd�$AM�DL×�����)��9�fTu���:-Q��s� ��s3�֮��7�����`ϝ�� �F��*^��Y
��b�w��� ��w�Ď��OZ�K;M���O.m��М^�p�īV�-��GwS,��x
6���E�ȠA��F��C� W�$t��I}�qV8z�iUA	n��J~�@/�\ٙ��8�ı��_�'6�>	9��8�e�ʲ�Ӣ��)�wdP}<����d$ڠܞ��/�F�1����f��YAO�θY��HpR n̠݉-����G	$ӤǺP�q�4�Ae��#�!�jA
�!^\A��6�qb0��v��WF�����ұ�S����h�����KM�J��lX�q����R��y��	WV=��!~�ЭyP�>�Hq�%� �3@P��x� �/eR��	͍pd(o7)�R;S@ �Nh_��/�{r�7��8�Q���37,��O��������g��q�%��HƃR�ȕM=��&%Q�E�J��+ٝ��[�]1c䲦�*��b�P}�PHHG&�^6)�B���{�)0^��d���D��%:�]�G����l^���洲sG0;r~9)u���WQ��'h��4��>$G���y�Y&��(��޴�pט�"��,�Z������59�UN�F�����[��P:�?��օ|+��U���ߊvs�HOs|�q,�8*S�Cl�x�����x*:H{�o�����KD���]��a6t��W�f 9�ud�}<Ź&5P����Lq6V��E��N�Ɩ�ys�i&����R9mr'��(���,��b��[l��t{��:,~<[��<㇁́��B}'Iᙼ��t�)9�!
7m!��P:<��t�X#¤���3by!T�F#��Q��
=gP��[NR6�e�����@��DB~Mm��<�d��C���&@��T"Ƹ����:��������P:z?%�n�t�����㽞���eW�Ԇu�.���Y����8��O���&�#U^�\������6��<MH�n����.������l:{
}�����CG��92���@���@�=���|؟�ĉ�^���@�X������>�S�R�� �`�?R�8�����YH�w�,���N���Ŝ)�3�n��L���_��8CF���t4����ɬ��0���#��ڲ�U��G>��������f�H��Ӂl�jdRs^��\��m�,���P�K������Ԟ�V	8����k#�M#t��(_4Wʜs�YϏ;6��h�.v��3���I�~:���6�Cq�E^\M���w����4�ݫ�.�)ig�vM/���q83@�{���    k�aF!Y�h�Q/�<i0r�	�
ԇR����6�m�:B������n�ؓ�ފ=��ɬ[�&tZƚ%��9 +���E�Iܹ���L3m�W�^�U�R@� x�ߋ Ů6��w0a�'�$�A����x��B�4��Dщ��4s������Y�ۀ���ֈ������\�۱�F������)���C��巇!M������<�d^j�f�D�����UWX�_*ˎ%�>J{Ka>�97�����JcP�^w��?W�QTe;��=W�s.��Q�� ��	��
�ƽ��Å������BY���)"��λ���pdR*��v�����x��Q���8M�N��ā#��-nU�"R��B���]��V� SHk� ���z��X�w���-�g�������qn����uiHE��E�c��EN���%�u:S��1���/�食��r��z	{�l^� !��!]�Vwy��(�Iu�A%���Di�w/��G ]�2ɠ�<��:F� �~�?�jY�9���H୦g��,MP���k��ٱ�&��@�<�L��֧s�� ��ۅ�[�Q�x�$4r(��������/���M,�\�W	���pP��S�i"lc!�uJ���~5��3�>���������G	/���N���z/����U6��b��d6t�`3M�
@KT��?�jb� �jt�|�(�v`S@9����lļ>w�7!���	��VW�sܶ�o���b{�5��3V�=:�n<�\���R���b�%�x[�i���Q.����N��ȏ� �A6�����[bAh�X�ϋ��-�g
i��A7��lg>lA�9�0�D ����ERO�剾�p�M�����=݃XE�C�����x�%-�:�� G����dn�qЈ
Y@	C��%��v�u�Xa\[N�j�Ǜ���5�Eͫ�zR':5���G�9�Ϳ=��s���$#"~,V�=ggxT���Ec��菏k����A��tm�T@��9���L�[tӽ}!9�]��m��i�FK�'�X�P׻p|@|�L�dwZEB�V��ډ���xC�3h����z�g$����p�:{��z�9���ǧ�h�F��x�CS�wW_6�jo��͵Xߚ�����4NNN�S艺J��y8�/>r��|������������G^%W�ˮ���h8�����RC=�^�@�ӜE�oW���AP]vvo��ADz�}��=�Q���-I�@��	����y����a/������xO�����_bw<�)ћ;��f���l�I��>9p�����e�)��'b��(��뙓4=ӟ��U� ���:��1Q����B^~��ĭe�m��=����o|��BN�ބ6�D�88t�p����^������m��A6OiE�i�P��ɛ+��E@/>Jlx�N��J�4{ ��Ta�Ԇʊ%�ȭ�}턯ܮ�]�����&�/�37!݈�����ڇ����"�w�R�Xj#>oD�$/�UbM���S�����șkp�s2KMup �/�:	��:��~�m��7�:ʾ��!�xjzq�&]���-=�x������Pߜ��X�Њ�xwvG���oKTE~Z�مx/�r:�Y7w��Qi����CT8�6�R�sn�=�ˋS*�}�d9?��4x�u\m�x���pW�oW��p���ilqdޓx��H����j�RqXw}_��«���K;��FG�E�:�#��أLvj�Ǧ�R�R6Y����cx sXN���cG%�~�g��\�mP�>����c�9@�QwJ��E�f�q�ͷ�S���m|�9���)eg!*� �ߖ�l��Bp�F�u�izJ/Ep��M}�{���N+�LW��$[hd��Q�p�l�ad�Y����^W8~d~o.�_ߩG�P��B��*+������T�U¦@:5�rg��&r\��X�ޢ�9��v���^�����{�N�{�M�
�I�.BE��b��k-dT�g��B��ϋ�"�4D��oI-6@��h��U��,�Z��E[�8Wbwl��#�x��)%7��6��Lؐ�������+@k�@$�x a@��{/� �N��q�g��&��6HZ�h�ڸ�����,
^���8�˹=���"�v�˹i���/�h�D8J��x�p������΋�x��?gP�� ؙ�!�S�T9�y,#e�)��L�o?��=���Yf���j8RԷDlr@a�cK�rj��[y4K4���G�;���~~$�"~��g��w!�Q2�e{�|���Kމ����/4�u� 	����1b;����g�Q|&Od�$��as?���M?K~1��c)�Uj��6b!�)1�!A[���O�a3����N_�Px�wz{"���jV`@@�.�}��)�H-�[�R����ݝj��	ۭ��n�@	���g� �S�S��T�8B��W��Q9�P���of�<?~�jn�Y�G ��~�c��a_#���e�z/Ģ��B��j�*L6l���f
/{�����+�.Wl�9������M��=sZ2�\���D� ���Y��cQc6���"��q"AO�#�hܒ�<��p���`�b�T[m�Q8J? lp�5�*t���^L	���A}a���!pDG�=ZM[4t��b����X��~�����m:w�ϳ� 5�.�yy����|����M���]���!�5rǞ�#N���-����"���;f#���=G���t��oف?������m(�g��1^͒w.�s��g6#䳣+'GS�JI��'��бRr���
�̃��;�ފ=!��4.Si �cJ/E����Y��O�Ƕ@D�?ب�ǵ%^���:��g���=x�|E�LN	Ry7���-+����w^������ؐ`�撐-y�^��'=�Z �A��_�O<=��8��P��� ���}*-�3��R���������_X���t��T5cW�`Ȧ�Q�3asm�����cy���#�����cv���}y9@l���9'��%E�E���P���������W��o��|Y��>�Y��2��bXT�Νھ�Ɉ������w��A�P�E��jtĨT\u	_��̙ ��* ����|�سy8���GJd8��bG�Ď(ZbN`��e!=���.�F�9w�	�C���IO���������R�̹[���HS����$�8)xs�QcY�w����0�`���V��s��兽q ?؁��n������:��G�@6'R��7Tț� nwp��_�(&^ne����7$
�.G!�~��n���D^ʬcS\��N7���Ф�g�8!���-7ْ����}?N�%�ȓ_�F�bC]�C5�C�,]-�l�X�s@�3����K���PX@������S��>���	8�y�z V]j-dt�����5��َ:M���{�lfNB�y����"��o:Y�)*��:�Y^�f�z�����'6䠖#������e��iȵ����1vA��"R'�'��V��ė��GJ-��?k�ۺ'�	uR6���!�>�龂��߰���E�d|Z{�R�T�8B�^hJ��]��4No��>�L���$gO����b/'��u� ���ٚ.^g�,D7�,���<��8��qm���T8��]O{�]ߞ�h~���eR�t2؊�����8�a��S����,�\�"�@�c�lFVE<I*8.�|v�G�e��i���u`MxI4��Ld��I�ľ�l	_�%�Y0z���8C�!����Q��7��<�o���/��eWP'�������5>+�W�r�#���Z~YHXR�N��\�k�I�X���Tz�`�$�*�x���w!^e5O�UI�s�d�t���P�����(���:Qqz��&뙘�͘�õU�C��gL�3��-���H�3�J��q�1?"O��]���������¨�H7�/~+�[�S�\ u"{`j=.',��P�x��X^� �6(�1��I[GJ�P�*2+!&r���7bu씗����iBs�[��z:Ȩ�V�:r�c����    ��x����V���X�8K�W(%Hh�7;�Ѻ�4��]:�`� ����"ʰ�pNݭ-	/��m��A��1�{p��!���%��3��`˽� ��Gg���X��u`m�2P��dva�B���D��X"S�Q$�|Y���.H>۝��*���9�ԠpPŌ�6�4i�~fs_�G�8Lv���/�p�='AD�v�9�@D���q�/��Y����]�`)6�/2ReԺВLc��}ʱ�n�</0�<X�{YHO�v{\�'�ۗ�v�+C]��(����og��3{f�a�O6Q�}��hB������L��H*Gؕ�f��]��?+�L%�i��ഷ�@��R�d�e39k�xL4R��;d����|ZÑ�zY�x�������*�I�PChh��P�6����Z��z�w!jR��D�C�b��4ޡ�Pp�����c����w6/���b�Z�:�����5"s�a3��B��Ӻ���j� H9���h�w�
�䓼$�xH�
��%��s�mg�QA  Q���}>Q�R��X�Q���9ϊ�����z��Y����șnq���d�4NK��;�B_Id6��<_*D�\���P��x�h������{�^p|�Ӟ�Y�5 ���Mn�*Bs}v�rҽ�.�1\�fq�T{o�� ��{J:}湪��a�-T�yRDB��h��U2]��f�3�(��dx��6 ������<������Ϧ]��>}m��['�����
[�[xC�����S�����P�Z�$�?�B6#ߒ�<ZS+�(������E��M�%u��đ�T�^pe }"�6����^lZ��+�����Z%��>�V(F�X ��"�������Fς��@,� M3*-��=�#[���Z�i����Ql`}ɩ����=e���.��ͧ��B�� ,2�M�w�Bv�4���J�s�On��CJ����r_�V�w���#�u>��e�iA±�l:5�3=�"�Ӫ׭ �gJ��$�ܰ��B5�z��}Fj�f�!��k�vv8�p�t���X� u2�d��NѬ��Zq.��I�	�C���@_���
uj��1AC����B�q8��M��\.���N��W3���W����
���~��j?Xy���g_;��s�f���]�q��@��H_�l�|qTvA�����7Ѐ���X��R_8�(�4����xm��BФ(2��8;���Q�,-�<�1*^Z�{�����6dr ��zT�2�>�dj��o�ܚ�O�6W_
p(o�TX9�0r���5��E�p�w�0?ɿ���om�tHY�Z�$��R�;�+�Q���،�Q��JM~��Rx��8�=�(<Vo��vk�
�.���3^nv8���t�u	�^�?}p�� P'pN?sMtbtxˠ�4F�yuzA��h��DM�'�=_��lbG6�7�@D���BXW�*��p��'��Ɩ��
�<�hJ�`B�#�]�Gx����,=H7`sm?�X��%��QJ�"- "\�G�4F�{�3���Z�)�I�Pv+Fl>�-����ߞ���n6�Jqhs<�O֑�4��O'^�v?�0-Ǧ0#�V5#������rB}w���g�l풋�
mr���J���^�(�OU�Z9FGU����,pR'/�M�%�R���Đ��6��k8��ayJv�L��t�lZ��a�`�
�:V����_9�^��fo^�i���F�u�-7f3h�R���%^�a:���١�ȹ�Ьj���DWh��y�?���p*�lve\��k�P�q�M�	��+Q�:y�����	�5���f@!��gs3lp�2��j�іym���\�Z��:�(�&�`-�s l��W��*�+��p�v�/���I��f�*i�]���ԝv�̗sNk��=x���,�X�u4ΙQj;�4j����z_���D4�W����p8;��J�a���%�x�=POwqa�A-�~hn�%���'�&ϩ��9=����B~{"9&=5��m�L��j��B�@����Uv/���{����ަi��|��u�6T�����0/wiW�uB�`�T��v�p6G`A��-t4r��f�V#��~�,�ݢ�<��Cd_hF��'�tf�2����,��Q��l�
���k"��;w{;s[-��ϖ�����jn����O�zPҎ�dt��=����q�࢝Uב�>�qWٿ�^��0����h^�3{��PJ�t�e�?~�*g>�uS�~^�,BSn�؛�\q�9dΝ�������q�������;�9��2ZA�x�v��?����|u��JE����3���_ �Me������#�g- W-�h��P86�)�dI�ɍ�+�H�i���N�&��q�_AZK�n��lu���L�N�����փx���������" ��yr���C�rJY�N%����2Joo��u�ď9�h���y�*C���D�l�p���}���ߔ-)4s,3"9�c���Q���b��8��ڳ�����b�V�85QI��݃j�ޣ̺��Gٛ%I��H���0���ҷ�z�#<Uzd͔�Z�-���K0̌TA@u�}�"�M5�RNlZ��JQ���lf &�q����'�6�i5B�<�Oy<b�{f����x�#�\��xڛ���B�³`�XW����ġ������n{���\�^�U���F$�9 �fxh���2s����.}E�#�0�s��5�>�(������:f�RQ��W�1�DaG���J{g6�5�0��ǅM���xJ�Iv�$_c��矊���
�ۛ|��by9��y��$�u#�Ơ�[輯Z�� �sm [�)�A���H���پӌ�/`�������J����`
=e�P�>�"�~"�)�ϚaV��u����B1{p ^��Ђ�qf��&"���Rk����)���̽�Z�����q8��i�t���_G�j�rlR*ho�1�$���1C�� �. ,�8����ax��ѻ<�P�>-���^ql��,B��OA65���CP��)�W8+�36�t�m���ŝ���i����A=�]����>�g|R�E��H�W^8���}��Pn�T��}{�� $>l�=T�G�{Y�4��(s'բ�a��Ny�Ss�����7�T`��`�7���a��#)���i{�޾�a}6v����������s$��(�LlwV��IYL��q�-�3T{�~�̔���E��q9m�``X�������g�*�,�C_��|.�Ed��԰�9b�:�	�\>C���7W�9�=;�Bȃ#U����j��"U�-���/���P�ʵ�H���0��:��G"�U*�N2י��'m[���Rl���p zz�j�uߨG��~*h@4wьz/Hq�*�o�lo�c�n�{��Q�Otx�+�{����x��(O��R��hp�Hpp�.�$�'�#�v_��Na��\�B������l� ޳P�Ȑ"i��N��Zg[��A߅��w2_�8a?������V|)�(W�.w�)�� ���K0 ���d�Ӵ�Ƕ8J��\H{ի��]\�&ͶR����������]E�s9#���}�O���և�Z�J�����i!��7�Hp~�D��s�5@ϣ�ӭ��'�`��mű��ȖQq��/��jH���!�2��9���v<��xuŹ��۾�.�Z����F��G�0 �ؑ*Am_~η�OZ�j�?>�Vx:T�(�f/��ȧ�-ci��}�* �gow�ǿ�c�����s��ӏ]sn���E�6[9%%5w���T*��V�ɹ���Z�α՗�C+�3����\����]VOMK�7�"�p����^���] �@I�� zרu�ݹ�U��G�3W-nmѷL��T�g�� ��w�̊D��`cT��8��Kl���@}6�|u�(`�3Y�M�����8��.��B���Q{���xv݃�� k�-�6�{�c�@�J�&Ϟ�p5�q�O#�=�&�~�U�aշ]��@� ���+�m��Wp�0���7��q�Д��'��    �����R�JY�qjx��[��B�� eK����ٗA5G| PG ��[�V�{�-��1ֻ�N4��f�@JY��<�5�ݘ844�ҷ�� g�]R?Ҧ�{����`)�g������o����否�;�\ʑd���>F�ճ-�%\ǯJQtvt��x�:K��3)ώ��S5�~��T� [͐ʫ����2q�o�YDF�5R��ju���Œ��ϝ�ZH�E8q\I�XY�B�)���S��-0���.�xrz����S
pn�
-���Na���a>'����U �`��o�c��������r��B�X5�!v�2��[����Ƞ~<`oZ��5��֖r�����3+��_��_����EYe��R�8����Ա�P����u'`Ψ���r�t�@��A5��%�B�e��}\�2�:�j�?�nJ��jF�4����R����,2\`����B9Cd��м�U���aC�6�p�ߺ��y��PTǕ�@��Q�#^��qU9�fQ�����lFM<E)��<�q�o��m�W,��Je���_��� WU�כu'����K�W��7�B�-Ёz_�m���ƍ}�RgݷKy�	XH�C6��#.�J!0za�f���4�Gc���6Wb8�����B;�GV���o�r+Q��
��B�kXA8���0��76Gݕ�eYֳ�<���\��ؗ�� ��9l����zfh9�0�αX��@.wj=)-%B6���R�p"�բ m❺[�"���Oz��F1�����ۘ�S�����Y��=^g�F�P�b��Y�s ��D�"�	�(ܣ�zԼ�}��u�N>���ZhG�dW�M G�׸���lV���ͽ����u��l���\�2�~����Z�|\��t#��z�����	���ƞ��`���l�����h���<������-O+�B��Lw��{Vc+盬@.�C-�W �"��۳���9�����ef��;�k��xz���N���x�,����f�N��m/�=�q���KuD�<J_��9���O�s��`O ��x4%N�ݘe��}�T=�,���-h��4�D�V	�7;�ui�������A����������5��hu�%�~!1(~�{��o�����şO/W��CBIn�t_�
=%�p�̏�D��ك���ѩ;!>���	w;�R� ��7���n�݌���NûQ���@Rt���=
�GW�-�Of�?���M�{��1&%���P�܇���P=�H@��I~e���/�>M�9�ݛ����O+~��j��M����̤���B��� Ym��g�|#�`��jv�GN��i��o�g��;6��Qjo��e���g{�o+�����)eĹ�j��)W��8���꾚�w���|�*^w;��-C�+a�m0���F_e���U>���*?s�jq�S�.D?�R�\]DT��(W%�ppR�����(���E�4ߩT�+�[l!
[�")~�DN���t���-���&U�Ӕ���#�?������'�.9헅���v�B �~s#��6����k�n_^vG��Gl�,n��͛"8a���?�LNΥ�����粸�]�C��c��W���K��U9� ��,jv}_�ѬR=n�������Vs-��܍]��Z�ȫ澇	���u׏`���v=?S�t��$�
��7�NJkn�RE�Fr}�k���4_����!쁽o��B���)��D�Jf͏B앥�X�v��1w��14����W`�c#0��ST�P��'�#������B�Q^��R�V��{�����
8��U�� �^�KtH�-S32gR���ѱk��(`v� "��-�u�8�5���𢚤R����(�n��H�k�{{��zs�C�n��4S�f��:��8�5zv����1;8�F��I�9k��'��2���ݒ�5����P�]h#.���;����'�w���{�d��i'��=�n6��G�l�%���;	d*%�0����r�"�_'�I�L� �%j+ cg�&���7%ȳ]W=p��n�����gw�Dg��xN>�R� Oy]�b����Z�fe��{7�N��
�q��LݜM֒V����Ry��#PV��O���tr����������AM�����V�I��Ɓ�*�p���pq���m��}����H�X�#���{ġu
�|���%�Bߥ��xqn;^�2���-~]�1�ql0���3����7�[���F\;n�H֒�����j8^j<��<��t�E{Jŕ�M�}�R�n�%(���R�5��O$4p;N!R�����V�vԬy]�&�r��s��8V=|8U�� �;��6� `�p�k��ǜ���}��k��P�2a��1��>�Q"�f�z ��Z���{k=��;b�QH�]H�,$����X�r������6��V�؋ s�o��^?�*j�DH	� �;��PZ��VveR1���#��kv-^��^B{42ȱ��
�:9��-���#�0��l1z�UJX��}�4+�8�RἾ����<��G�nPt'G�Ƴm�ݞ�*��<���D�_Kd=\�؃[ v���\f ���~]����@�`ŀ̓�5��ś��a*�g̭�u�zY0|M}�f+���'P\	O`9+6B$��5g���[��g)C�Ȭ�Qy�z�]��Z��t����p	�BcpkYd�Pi[��|P�M��Y��+~��/y�����fD1��$����6����p�[5 �u�%dws�w��B�c�U~���1N�>����]�5���8�p`#��	�}�����.��������<� �'Y�>ߩ|���ޟߩ��j�<���j0ӠB�ơ=mq���p
H�-�vχ��4h�d�v�H���"�4��^x=�� }T^tf0SaD��<� V�D"�m�(0�oZ���٘��K��'(9ax����#�G9-6�|K���
fJl�1�:ꝯ��E��@`
��ZBJG\��V��P>~���!4�8ӭ�3���еf���ϲ{�"�x�	y,[��T�j�����OKl~��D���Ktc��� ��o \L����7����;�+�Ex�˪�E^l�3����#^�1Q����-�ߡ���й���I�.O386ռ��<#��zK&�W � ���o(Pe�E��"��	�ʁj�(�����L�`l˸2���x�a��h�؞�8M��ɗE���|��׃�89��8WW'u��S�C�}Ӊ���tWT`l�̒yad7o��Ġ�=M�7V5�P�=wRdԜ��Qln�hT�bg3��
�	{j�0wA��,�biF�{ŉ�G�,a`�R�BVP0#�xWI�"f�j����A��N9��k��?l&ߐ�n���,%�M�O! ��`<QX��\$�#LD�Y2^�=�8n�����.|`5�(IL�1�J��nU����,a >!Q��Q��� �*�n�����2��r��f�Hgl�g��KR*t4:�έ2B�2�ɹ@s�!���h_�o�б��s�{�KĻ���R�t- ���˃�M}zZQ�����h�jT;��T��>$8)aP�u*"���\�OL������p�_8%�y7u�ΡT���l�l���b���%{��)�'=Z�����q|bݲ��X��N��7���R&�cJ���� �5�:��_:�Q����1sRm-N��F_ږ���?�-Ж6y�(�# ��(To������<0Y$�1�/���g����_G��6I�]Y�$5������q@�M����̓|�̟�v6���f��XP���������B�+~��y���2""�z�An��E�)��/{�°tI��9D[�|�qX�)�5f���7�S�]io��]�:	��3�S�uV9�Ho8�ȩ�8�u�ݩ��X��s����y�G3����"ơ��|����^ܹkR�x��!��
��)�-�]�I��]W�m��L��G��4��B-|���H��    �߂��^+&|�W���W����ڱ	�X� v�]�]H���G))�e����o_�QVF�..������˧;�|��mjl��RY�ǔ�}%��(�d�xO��s�D�@mn���e��<t}b��{��9�J��҆����я�A͵��Ǧc�k8i��蒝��?X��~9?z���<q�f펶���B�-���7����$�?:69g� �<},LP�Q;"�Ok��vg�+��� ҆u��Y�G����@�ɣ@�M��!εPǦ���T�L�U��������IKt��-��%f�j� �PP��r�X�ѥ�tW=���n������*lPڰ��$���!���Z��V��H�v���Z}t���[�*�����u;�?�e�s2���n�EJ��g�M��J�����oEV^���=k�̉(0�I���G�ݜ�\�b�{����ҿhK�⑞5r�����[�1I�j�s�,tLR�_��ӂ,&y��%"�ǁ?�W��l��:b�H��|u���yF��"�?H=�mρ̍�^vxY�\��T��p��@|���66J+v�Z[�u?�}߅���=�}0�8Q
���gE�R@{�k�7�T�O���g��dlF�5��!�w#rL�bU����.�r{���hB��ў���>��G�������l(��xY�L��~��إ��Q�1ػ��Yu��źL�=C\�:��
�kzm�3�c�����3�P:�s� X.�����]�S�T�ݞv�5�tMO��G ��k�^�5�����Om��5�o�R�u�ymy�U�����s|�O#)E����a�VFR�������nX�~�kDM��V��ۂP��u6��#�E�P���)��U���*�?U����h���q��7@ W�/PVǑ����oĜ�>�Fg�O۲)���p�p��?��Ե�!���o�4�Gc�1�������P��}�T���	�G��<�ж����®>$�>8����_B8}�Ur2)�,0zl^� �Ӗt*m�Ǡ.�����jޢ=tg�s��1�v-���:�eƈ�bN�G��Whn�sZh�M�6�cag4�[H{яz�\�~:�I�M���q�T]��r��e	�u�P�LC0ߛe�#%Q�P�iR�<]�m_z�Q��.T��J�=z8�����t�$�ڎ�jc�8���c1T���5��C+-v)nV)R�P�t�5���CL���Y�s$�D�z��)��U����Va8���[��n�ˑ<5�S�1���Y*�T����j������2f���&�=9���fų�շF�4�l5l��t���9[I�7�{����.T�mV8C��U��K�V���t�o�Q�_m����"�3AVp`�f���3s��t��G��8&�V-����� çj)�7�I\_穋e�9��ٛ���e[�@-8>fq����!�f6{o^���Z%�=��\��B�ڎ*P����hxc[y�� {���@x�"����d���l_7�Bb�"C�'�U�\�.?�?�5�q�/�@�1|wl���8u�PXߣ����X��������A(�@=��L�c6IC~}z��o36-'ժ��oAdΝZH6;3m��«2�)��������	�#��a�|��#.֟�Y�ܰ�>z�2M�-l@߷'*l%Q�L?0�H_G���Oj�G�F�&�"���}��nu��, Z�j�te �[,!8/�<8Kwء�	+p�g��� ��@=&�T�a��0�c�}��Q��#�
ћŦ"�|��RY��I��u�����QFtCx3�^�$0wj{$�Q$�2x�*4�v���RT0u�	��]䩓S&T�>�% �m��m7.��'��b:ӻ��~B�>�ԮY,*���%�Z�~	G�?�XC�gu?�=9�IɾP�mjm�	�U^^=z�C�z��]��f����&�Z��'̽��w*/'��?������ҹـ�r]f6�M���ۤw�!�go|���)Vmq)�&��T����"�;@�{!ޛ"��O���}�SV._#d��(4�e#������&�%�K�P9m����@-�5�k��ʥWe�����C��@	iV�XE�G%r�����ޞ�k��YpH:$��q��Ǘ��ym+��t�-Y~�A�,�\	}/e ˁ�lNh!Ts���"��JLW��g!*����6�L���8-��<���g��3"��B�.W�
 ��O�����y�Q��`p,��|���6(���>�=����a't:��!ˌ9�x�EO�hL7�G0�9$�Ѹ	�urzF���!��M@=l���!t�*`��itQ�D�j�Y�v��u4b�W�	�FlGc�r�	(.-	+��uɽ�|���2I9�U`;H;�k��m7l�&�\�K��x�/K��z���׼���ӳ���v��B�x2���U3]L�kd��I$�;��g������
��	3�-��MlpPRF^�k~2�����m\ �֚ǐ4R��^�s8��ƅ��ֵ�h�}Ƹk�_gEd A<�{��IA|���-�9_KG�䖱���^��#���B�[ZD���Za���0g)%f��������@̋v��9���% ƧO𠒎l����_O����˿�J�t��><xu�='�b`��X~���Uw�g���M��,�@��#�N�[yu�����d���W�Od_�^*6|�'�!x{�����DU���-P�Q�:9�ΑdQ3��Q=��F�@��F��B��r>���:�m��
�H����?�������Ww�(����G��6s9�;{6A�N�k��BWg!"�\�i^��X$�z���`��3ǎܳ"ʹ�g7��? ���ƾ]�X�n�9A�.IyXG,�l�e�c|���櫳�!��V�v`6�b�L!�$����_x������l�`}�⍹�k��)TRܓr�!�svw:`�@�63P�DQ��*vEe�"rkH~cS�����h μ۰�|�K%���#Œx���3���x=C�>`�M]�-,��FҀ�����n���e���ՙ�\����\'^�#�6�/�n��=h���<�л�9���AT�n��B����k��%"p
H?le5Y��Q�7���
�����h��kp�]L�Ь��[-��R2=ݱu�9������yY�\=�W{�H��@ؓR�2<����m��oh�](���;����TЍA S�)�42�w���/[�ʧO�n4�s4ET�v����B7��;��i������A9�B:��r+���W�m��ͯ�$6q/U�eZL�����&H�O�Z򠼏��v3L����.� ���A
�����<��|I�&�.�"��+Zc�|��� �l횱�g\��W��-�84�9�#3�k�Q����6�ݧx?��$�F�$[��7�`nPܥ�Iq)�Y�G<h���Sꈨ�J�g!b߻Yl��ԟ�)�m8�B��P:���v�X ���|6WA�D��MO��Ҏ
Њ~cW���]E�KU����3q4����s9a������?�Nǆ%���
t��O8�@adVc�
O*s��1o(�a�d��2Y9�O������u�ZY�e�;��ux��R��$M��P{4�v�&�(����y����U�����K(�}��}O�,��#�d���/��ʜe��>����+:�CvŘ:^�JEEp����k)� �s�(煃z�!X	c��x�ɽ����O��h��'��8�~!�^��HK9�K4��P�d���X�ye>����{愽�>;M!��x�T03SU�����<Ш�\��~�"���)�5���m�f6��v`6wZ�U�8ib�'G��T��r��P�f|r9(�RE�������R)P��P-�iͲ��;�"��c�!�=Ƿ�S�]�7W��y�<���B5�(��9���Kԡ���{6�Ԯ���A��K��1He���Z�~�>�J�P�'�fQ:�;N�L�����;�������В�by
�ތ�َ��L}    e������H^Ik�����K1򘄝���	�ԤO�������v�����/-%����>�� �s��a�P��F�l��ӍOjk�@Z�H=��i@�C[��}n��u�	l��H��c�d	/�	�8�����֗�C�6��T��;�.�>?9��M�p�Q�\2R�[e�� a�g�e���Լ��4g�2]$�i�[Ԥ܅�c�"���wy�J�}����|9��C�6���:��*I)�ҡ�9��3�g�	�����ؾ�^���"������/&��њ��3�(�G-�$i���V��>���$%��r41�e��ҏ�]�T��ǌƨ/{@O3�^Vs�e�/�!:���Á����w��J!_?�`�(�3�p��bm���k������]o�c�]>�Lj���Uu+{P�g��%��yɒ�P�3�g��㴪fS��ό�۸���>{� �C��j�.tz��q�]S�uǀ��7�TS���H8������5C�)�K���UD��؛T*GhD��Q�=��L)\��,��mW��n'@��D����2��S�m�cJ�q�|���K�cnˮN$����	������uxl=����]|��jv)�}7�R�FwB��^�$Vk]�*WR^i�y4��Vq��#���4a��� ����gl�ZJ�7hw�����X�<��$y�l,%n���R�(�+�%��݅=�+�Uxphw9p��R���Y�`B�G�d�qklm��|,"�d#p�Wg��	=��U�[Θ)܄wϵ'y<69%vKjW�;�J%l�ޙ����"+˻�A����e���n�����������^86�p>NOq=�o���\�����	��S���Z�4Rv��{n6pt���$�8���[�ÿ�L��i�9R`Md�k�-�hX'z7�}J��w��[N���PR(���1�<�_���LМ#SG9eN���C��%Ps`]�`38q��uVx�ׄ�Ƥ,��lVU+��x>�h��B�1	���԰� �&;7��@���ԮGձ�n8zlF�Y������l����#��/H���o�:Mb�̙o�@�@������x9� �2~�p�8�*u�-�-��2l��
�"�"a��#�)(�K$���?�n[�MP���Q������<�RV}��������mpt�.?'&U��Q�*m���_%����͙�^
�P�6�z$�^B�����4��=�c`Z��qq09�OϪdZK��GR�@�,P&�^H�&`��W������i]���^��۫Kg�M��6#�Sx�>q��3�"v�<�ѯ�����|!S�o/�N��M�%e�O��IX}���^)4�J�B+�s��
G�����琢Y�$�~�b�N���f�=s���E�*۟@�®
eǝ^�B<��7�7Fl��l�f����9@�ca_�wS�qϡ,
�.�D��)Ny1]��,��2�H�������mIX'�=r�>Rf�%$BR���@RЇ����9�٥z�}A	��X�	��t�ք�J�23��u��^>�)̉I4�@��Z�9,� �r�C�g�4��9�M5F����s�d?�EGHn��@T���^���^<�R�\%-��%?�ty�L��:�-��.鍯6���h�y�cee�����=�;;��KW���Q�7N #\Q�ʇ����#X2���9��}�9ʿa��?�=�S��r�8x��7��^���0�j	�yʁ�=(J�T�;������+�;�Q���~�w��.�hH��4(�i��^J� w�,j��z��~���6�Ia���N��{l��*ڲ��������5��@|� ���ߢ�Ǚ�v8I�]H�EPN�J6�C���-szH��C���[�T3����ɂ���p1��{-e��4!���}���r��,$�w�U�m��D�/|�0�;D��^�|V�8��k�k+�,e��j���R�<4�`ză8"0�I}��E�r�/�Pg�%/6�n�N7;��d�7�,Fb�ʹ�2�z/���0=UY����p��t���>���}�GmP�M��U?��<�@:����p��	N�,���,�g!Ñx'�w��Dm�����.	��� h�o}��x�����~_������e�%ӟ�du  ���M��P'��ѴZ� ���a]	_(f3����5jǙ�
	 ]7J�{�	�u�[Ytz�b���(��e�-(�*J!�t!����ݗ�
�do�N��&)a��¦;P�P
�B֮�����ƞs>�M�C
E��4謑Q_D{��B�!\e�DmOI �݆��H�I�z-xm=���)Y�_�'��� �v,s�P߀������4(��K�u��.D�r��
�'�(�F��>5KWT�o�DY�L!��^.��G��Z��زg�֞�5́��<RD@�����b\>��7��-֍0��{�����]�o��X"'�_^��Q���ʡ3^�>@��;��X��JK�(0�����L�@�A�N��O�mǻ���cVM�tw��#sG��f5� t��a�)nQ%�L6�춗� �/_��ܳ�cxYT�E�NQ�̱-j�U72^{{����n_s˷��nP*ac��h�8~�F�r�|����J+w+Wm^(����]�b=�f���Bq�j�ũY>�j����Z'pMG� a�x훢F8l��4�CiձVh�b��W��{
Vwl<D�ʵ�ĖO����*���k< ��ڙ�)�?UHs~yu� ��3�&3�lH��jc��X�G|�䗧�_�m�=����x�����P8� ]�ߝ��Y(b�Y����!Tk�� v3�"/�ۚTe��jP��xU?1�g�z~\�5�Ω��rؽ�B�W�QT<Q��!�d�ؘؚCOтus^<��٣r��_���BG4�͛Y�۾u_���Δ�X/#ݢ�6���D�	��O&���Ks�6�pB=�����
X����.r�_����3(��q�:����[�f ��ϳ�r��1�0 ��G�)�7ގ��ӝ �KpKs�/Ipag�6�{����
�>m;^"pqz����M��O����j�NS���IO�i���?��\×^�e��u]r�\�����Q����j��<���KȠ�?����J�<%�]�֯�v�l��%����2�xR���7:��j���z'��I�}^@<�q�L߾Q%A)>z�{��mm��Π o=��b��v���UR$�d6ɍ�u����aM�\94��W����A<a�<7���L��ɻu��#4h� �8��w�}UI�y��,�(m:�c��[��p�Ʒ�{1̷E���1gm�q������п7�}�=Z������NBt���S!��K������ �c�0�P~3�10&<��oMj�c�����d�7�8p��l)�"G��CGso�߅���e�*>N+��<������P,R���Uf�J�EG�콝�	����	���A0�s��솗�7�7Ђ��K��o �"G��Fz�ZYI��N�D�86@�ohpKۯ��w!�Q�Д�~���?)��mZ��,`a��v����Y���4p��s��= ������U3p�를D�#P{�i��8�������
	[�l��-�蟐����y~�U�C@xMlTS�oF�D��$�z��X('�|���!��<y�N�`���:�깆=w�M�<<�0�,�+����|4(O+�(���D����?}�߅��3KkF?y��Y,Y!� �����,���;����>)KjEVn�����q*֮Z��u������^������|�fO<�FV���.��[��/O�-����Y����X��]�｠���h�E�_-�n�8�X��y-<�`���l�-m�b�^Q�'�3��/��C��VO/���&v�fbU��;o��h��D̃��q��p�
�����髏_ay�Q��	��ٓYk�)t��c؏M��	��}?e��ү����R��x��rn�v`�    E��B��{��͠�W)zʏ�P8����G\��qz��k�1�8��QٌXj*W�8FUZ�\^�t�d �0<2_v�
s��a������u�P� 	rLK#7^JeD� �>O�@�QK��l�B/�'/�S�"���:�����.�O� 6��j^�ԅ��3�ZB�<L���k[a^�G�i䕜�d��q ����BA���:��pr
d�$�ф�XP���6�

���(���ӵ��,tRW�Y��E�Ti��ك|��G���+ 虽�l�h���6�5>zZxo[�+P'�t�+��E|�P�,��XxI4�Vk�Z�J�ʚ�$��뵝��Ț"�5�����5q(���Ҡ�P��(ԇt�l��}���Y���yb���62����y{@�TW���w!:��h��>�F3��b_���A!�l�i����ߞO�vK0C���u�L��ѹ��h��I����z:,�b��wZ�&['w\g�vK��e����(��ؒ�|��xNl��-r3;�6��2Z?�X���HP��!���i��6�G�-�џnO�R�ލ����c�ZjX7�SX���0��=�
;�O��́Cv��T��n��v*oOt���́u�D{�5�WM�1!������� � 6��!�5@�%ю������4Ț�K�Ӛ���f ����s�I��2,1�%l#�e�E���9&^�O����y����̺�
jnJ�a� ��':]��dŻ�5K��I]i�@ɏ�M@9�=M9��E���F�vy��Es �
}����O$�[�-1�A�Q��[�E��{��A�Y9�6x�A��8�Y�)#d����M��wub�7�>`{��˯��@7����WkNu#"�RC��1��V���j:*#����֝��#� �E<]�h�-�#|���k�u�X��R��]�a��F���:�e=�z
��Q5�ߞHO߯����j��}`���Ui� ���|�����UH����Q�b���z�t���S�Ўٿ%V�	G�0�R�c��=�u$sn>��H�#�/ȓ���B{_��Y�;J��t���q�NYnMiW��?Ka%K����A��+���n���H��ķE���a��|R�v��������z�vt��4��W�;-���h��o8 ���Qy����n�;�7r���\��T�p��<-[+��>���Ӑ*z��<�O��N��c,&�o6��c;B��:H�C�	�[}���RD�}|x!��H�.v#H�&��S�uo�q����?�L�[����tH�B�ײ�lc��ڵ_��r�,bV�ͽŝU0�A9B�ҩ��x�}t���J��5M|�N�3t�}t�Ce��5J)��؝NBnO�N˭:r����|*Ou��H����]WL��GC4���n,[6Zp
/�%��tz�D=�N�3-\��ꊏ����Md��{V� �V�EN�"�5Htr����]��sz�� 9�����;�Y�x��R		�ĴZ�"�f��Q�J���g"�9����BVt�a%)%�3�s�
vXl����A5����c5^���P��@�mKs9�Sϩ"��)��� ''��*�O���c�v?a���i�HE=�V6DM����Ϸtg�|
<ĉ!����Ȕx���_A�y��y��X�}ϐ���,��q�$SH�%��&�#��b��%g��=�Qo��/8?�߾Z���ΗTR����BL��8��\��
x	薒�Q�u*�Me˕s�:M������]���#>�c��v�c|p:J�s�*��X��$����cw,AX���<��㏵ h]���	��S5_n�6wb�2���E @���3�|W��~׏�>��T��ِ[<����Ş\͞�z����|�j=E>�3�$�G8�~��n�q�B��~�g�9
X���Z�S�0N��I�(�mՈ�,�����^��٤��[*���<�^�lK{����Ap�tF��n����e���q�bs�;-��ų��ЗkJ��0E��ǂֱ �q�[i0۽,Tx�šY�y�p����y�]�K�x|ou�Ư�A�k�}ɦ.(�����,ڶ̐�G�%��gO?�Ig9�a��`����v���'N�X��<���x��P ʮ��.m�w5d�R�gl�©(��%�0*J�_����Ό_��,@	�]m� �g�4+���,®�P��{l"��>�#�[�G�Y$Ղd԰߾u���i��~w+����y7�Bۥ��G��aN-�GE	'���`��ۛ1n1�q�#AV	��{Y�t�x�Y���N��� �>t�F)��ݗ�����4\�n��f�)O꠨��H[���LӶ���*��o�t2mn7���g
��6 k'�*x�����R�nv ���U�Q��P��������P*q���wE��Ls��Y�F�SdҎ�L�[�^b�M��]U�8�e�-��7��v�R�ڻ��{��:JyM�ùd�\J��'[WO���p�A�����L�$q�,h/�� o��ޞ@��W���qE��!�BGe�"���9+(���=;�A�X-��OZ�q�I��O#f�����S6 �x�%�a�H��:�3�*�����������(����K���O���O�µ��z1q���$p"����IO��K
!v��@W}Y���Z��!
н^l$js�ޅ/R�*`6��=�w�-&{��Z�b�.rR9��7����!�k�/��+�� s�G)�7�J�h��%� �t�^�1��Wz��3���j��O�g�K��I�q�!�s�M�Mz߅��f�雁�na������"08;�V/��q�� *f'1$��.��5W|-���*��_�M���L�.s߼lld|���rl�F�(u�2�q��u��I&��̳o:us���Xjt_� �����:/E�.&j�[��.d�͸�W��(�NO\����v���5�Ê=I- ��D�U�!Sp$��&���#��ڵ��Q)8C�˝���b����LHf킒:��ǐ�����z/C鳒�9�J �f�<$N+�R��­B_!ƕޖ:M�^_9��C!��t�#"��v�@d� 4�]� 
���3�Y�S�,����)��Jp�;��w��e����x��Վx�����;���e4Z��+��+n.~����<�.����MKeMzZ�g�%�д��\�dG*����\k�n�L�WT9��}s��Wka�t���[
������jǯ9��s�Hz:�j)p��Z����/2cT���v)�wn��<��+x
fwŹ�^/�eQ� �F�N���߇���R��=>�rŸ���G}v���?`V����N��� s����&8i��Θ�<�I��yoGe�2i������3_���!�Ԃ�0���r-�,e+�a�y�>-N	����4�m��'U��u~��O��qP�ex_k4�3{3J@�T�>�}R�hh�g�x��?�]&Q�	(Ϯ���H�M���Q��#��R��U��B	߅8A
�eM[�ʣN�@�kA���"I��{!9���l�cޔ�c��u��nL�o�Up�<�J�ε��3o�4҅�P����W,΅
fN}�q��c%&G��.���Z�Ukȼ/� X�և��ۯ��{a�x<ai���v�D���g���A�c3<�c/d��K(e���:o�AgSG�&��qV��x���8�=�W=::��fm �G��#��;��Bi�L�v�����X��E�fhe%p���n}ڢ��ӳb�X�ou�X+�\���O�����^"�^KӃ!����� ��ܨ-�AwĻ�GP�4�rm�&��� ��$�\�Į��S<���z��5���UWE�h^��T)դ�� +b'�H*��BǍL��H��E �Xw7y8D?4S�h	COU),9U�_��q^�Zw�`�${��)�9����$�畍w�xf)u���8;������� }3�����&m�Ft�J�_�7$ppHs�yّƔQ�����聾}���    /��o/|� e/�9�>x2�)]���.��I����qX�T��=3L�) Ґ���)O Y� �6�`�c)�9�\,0�9�������l����l��$�*��W�������E�=]ɠ޴���bA�'���uF��˦"�)���/W���PL'cCT�|�r��՗J�Q�
"�����}a�����4����a�T���±B���d/c!�PvQéS�0��>e��x{"=%�*Ố�^ ��\D���p(-R���8m �5 ��@�_K��K�+�wt�\,�Q�V�����^\"yP���Zp���#���4N��5�����̧IT�޼��V�R�<b~UiM��Sro��/e�w۱�c�F��!H�ȧ�q,��=qZ߾�c8f��N��>���o66�-�G᯶^w�_3N�'W�B%��3��	B�����쐚�t���.t�6���#������(_��`i���i���b�r��%���|u
 ��,��TT/����n��PA�~��n�� �-��������ߡ�4��.M�ʇ��l��Z�8����~�J���(^b#Է����/�Դ���%t��7�r~j���{,�@\��	mW@��}"����'�I�O�9�
�{�?�[L`�
54��f��lf��r�Ũ��j|)l�^ҷ�ci��mC�tQA��@;��� Nz���_����f�ռ	b��ʭ��@vk���m�3�['����t����C�%���O���m��K�у�9�o���}Ed!��>��QJs���E��� P�]M2�p���G��u�QK0նz{"6"ݩ]'+v��|����$�56 �u_�1q��BЪ���������}ďEՓu��cfFAU�p�:iֺ��9���I��8w���=�7+R=؃�.w@�8�8������X��	��
�[#9dKL�kI#�4�J��6 X�[ܵ��"�x`���TBot��=����i���{���^����Oq�|G�4 ul�Da&+N�39�Ps�ᮕ��Q�L��<d�f؜�e�n�nԨO�e��5����OK=N�m&�g� ��n@�z�`��C�$�HwA=��`��Es(�q4IS)�%�rp�gOb�uc��-��bQ�ܼb��fA�}��bk�_���}�;��rɟHa3[uÏ�8��,��� �-�h=	�u%�c��t���JA�{��ǔ��-�P6�H�v�}�2�O�����F������8�u2��\�ﴺ�P��W�!����h��߈�����qI�J2�Y�p�;������Y#U���v���H3���P铕B�����A��b���҇�=J׾VdK�
$�Y�֭9�y+DJ̀�~C�W`# `]^w����BlD� Q�bK]�X���r��X�^/���{�>��>M_>�[�(eT��k�A�8Bu*��m�ޫ��;��op�٨d:m�|�rD6����1�1z�0�F>��@�Bv�Y�M����)U�)�2[
m}����"���R�y܍&��o��}�o����2x�3�`�WD�7#d�A��Ԁ�Q��(
fP*�w�4�n�Ἑط����d��Ыo�M��� ���f8s��j�`��֥ةc`�x�pC���;�
6D��:ë�G�1���֞�\򦽵��C�����w����44Ź�I���f�*�D�<P�rm�3%�@�e�kk�q<`sl�J������~�>�7힢�f�X���&��H�=��# CC�H��%�>���>K�R��5�r`�|m���v���f��Ǖ�Ӱ"�>�{`0�L�4���9��H�p����߇#fI��]���b��`ED���"�Ol��WD��Րy��O)�H��f�(��	CRH�6^6�?>c��d���f�$qBc��[_+��g(���'��!�nh�H{�N�¦���\��4�����fl�w1����9�GM�:b��I]�j_�%�R�H?�D��B��X��6��k���p��j�Y�x�F�ȇ�X�\�p�/ω�9(�F7��O����˫�S7��ؕ|Z��PX�ę�|�����e�D�C��Kv�Ռ��|�ȱ�i�o��L�_�]�>�٧�����Y�9�
b����~�������U.�pf��ڳ���ıBN ӛX�Q�m I�r=�=Z|�CzL<S���u��X�<��KM�l���e���k|�u,�U,*j��u��Pwxꅊ ���u�vJ�l�J5J�9E.V�vj�:�xm�m�;�}��tTH҇�槉�q"��e�M�~�s;��ں���.<J�d9.K"�OH&�l�q[����#��� �Ö3ױ���1���
N�T�t$,u���S� !�f��\���.���}~�9�~���v\9�Y���ŧ�W��)��<�?93��~5�ӟ�?����>����g�T�ܳ�m��p��\�䫅$�Ύ���Yx/��s��^��Z8?^�k���~�8%Q�$����b����l�79��u�
�@m�r��dB��6��e�>���M�v߬Aɠt�2��#}����δM��y̭j�e�5�b�L B���⪓���xic7�t������4}+8kx@,Dk�u#��?G����1?ج��8@ıϚ��vcw��zό���F3(�H7;�s���rQ`�.R=�I�o���x�Bmgs���[�D������������E����Y��Ǽ;ȝW���x��P;�����e:M��� ��iJn��B11�|�J`��d�-/�.���#��V���	�=B����b��ʬ�4�p��]�V<�|��=�
�A)��1��Y�Z{Kx	A���d�;�D�,�pR3�gQaw�-��f��v&�c˩�7�a�	�6JhZ.Q(��D�3�q7��6awxv��R����������`y��T����o�����h{��+T� -�j������w�i>ʙnp�\`�f���6[�"3�>	�.�"-����q�ގ׻�8�<`��+tP����bs����������h�.b?�����^z ,��|IP��(�e� ��}6�+����@q�Ns���+s�L�!��?�l@^j]�s-IAO	-kz�y��"3%�6?�r�b |�!�����cr�H�Ǟ{�0�˧��� xan�E`{��������{!����!��o+]2�s��p-�w:��ٴ��An�p�c��wqfy��B���=%�"PՉT���V^����D3��������Y�����d��r
�u���?#�^��\wx_5ǯ�?+}+��2@����ZNy�-�jn�2��CXTZ/�T%�O�)TG�.67����b8�����cgZ/c�FWd^��	Ϝ��V5U�=��)x~yTP+~V�R+����^�X4}*��̽���`Q��O�=?.���H�Y�ɵRo2������ڈ���sf�O�����^(�6���~'�������Ưk�E���c2�]���S����M|��#��R���h<3W}I����tjYx���`�p�޻�?g���S��_^'n�7邹��S7��� ^8u�X�r.K*��zY��u�X�P��F��!q>:�җ;����Aq�?vw�zaWUp0���eRBfҦI���æ����(��fn�F,��Ā#���5ξ���6^�9a���f�5h~��y��G����t-�l�\1��L�����V)�b�Y���~�B� �.��������K7���e!5��B��#��P��z�F�p�6��_ٛ%ɒI��U{� ��^���q�Kx|4n�p�ݚ��$P�%�LU�eb���vݤ++�jVT��J'e7;�����?�X�r�^�F8���|֦��|�J�k�n ]�MG��X6�hIW�J�}�EEû�g��|��E�P�}K��8ua����f�yq%�S��Rq�6d���}��
�l���<Ju[��;יSp����:�    �E����^_�D��eB��1���ֹOG�����E���b�t�$�8�����C��0�GS:4�f�Q3�G;mP!��)������S��e�5�Hh6�z�1z��D����4��I1�7�r��8������v���b����r���m�To�(�Na/�D�����rbs��0���նv����\�_S�8{5%�����̄��Y��\R`�����/��3�L��8���ݔ:�y���q��X�Ş>�<��Pm��B�|�N���0\oP\M9?3�v:U�����|�X�o������M�ʨ��������|�G�k��ꔓ]o���)W.�!䫭`BZ�[Qb��X�6�P1��PJ���$sL����آ�X��3V[��k:*���Zo��|���"V��JYݢ��zd;>�=��G�=��,e)Y�F��4�l�ѿ	JWR��|�/!��9K�W��,�EG#
��_a��d֧�#أ�Y��u��o#�Cф���������������ٱX��m)���o�`�hO�ˈ"��G�_Q�?%ʓ^���΢'څ����)�y�RԿ�7C���u���0�k���L�Go���`�;x�r��G�Λ[C�^JP2����+,����w�+e����̌g���|�Ôu	����
��hy��>�'�Z}�BގO!���f�l��!4���jlQ�_^��e]I��:I�oh}�'8M&$�W��%�/>!�&��4&��o��E��)�"�m�.w��Kqg��<�iGy�Q�q�N�t���=���n����}�=�������:A��KV�!Bg-�΍�5�gR���y��������8��;xr!�d�`���Λ��g)��$�����I
���=��g3z��Κ?K
ga�R��5_˦t��A!/k�MVjr��'7F������!6C�������E�u���N{����z�}qt�
JR,�8���N
�ʋيbb j��X�.�F}3t���1�]���>ʵ#��|O-�>�C���`���ɺk&���6"'LkC�&�S!��Wtu��|�l��&.�*sP�?&�֣��g�b�ί�§���X�s0����~^�	o4��\��uU�Y�s�u�4ʵ�d"�*k��A�rx�_��u}zQ��~V�Fғ�a�ى��=��V6�����yW�=]���A�g�~6��Rʠa��W����<�
N(�Z�|��T9;���J����s���S��L��� ʲa)7�_c�`��Uy=IВ[�/X��Y�����<���(�P�v��x*��`��9���>�:�U��z{:�� �c5ZݞG��p���+�ϐ_đ�E�'	;�g�Bq�˭={ L�EV�K�s��[{��RjfoB^����k$���B3,�-9��f7��|!�����҅�[�N�B�a&A��hh�q���߾U�뽻ڕ��$�xLM�8�|�]Ir�~��Կ��V���nAGa۪1wQ��4(�N�@C�T^i��ʧ��Zo$�\����6Oߺ��['����\C�D5�/�P/b=# �~��{�T��և{��cb����P�.��C�
�����Nem[_����_�t==P<��y�t) ����lk�6�U�~�\��N�V��ȴ@'��k��3䲗���EL�뽞�B�JR���^�ExXil���}=g��t�x�@��uW2�oY1H���9Kj�|�F�B�qs�z����ܵ?�H4Cz�P9�q_\�� k�w�wG!M��r6�a:��Y���f���H��tb6\����C
��B���ro��Q�pvh�t�p�9m��Ҭ \Yc�@�"C^��ƚ�N��/..�Qj\כJf�#R+� cǔ����Z:�d��}�оƿy8�PH� 	��zv�3M�@�K����M8�Z�[	�� E�=��R6��^��0���c���k�N�A8��PpKL
��q+8��/=���F!�]]�ܐ������R�.On^=�C\v��	u���I�[ǋݫ�r��c[meGoWx���We�R�p��Zѣ��܉M%ta�!ɷ��'����Crq�v���\{��'gwVm� <�A�&0\�a���L�����Ay`yH��3Q�٦��^��U��7�o��`I	4l��Z��#/��[u�^��_#�A���n�����d���
h�����C�*��bn�au?7��ηi�r��O�:Խ۾��![�A�F����+h����v_��LzdcǗK�t���Q�*�Ȅg��XfK<�E������_N�b�L@���*�U��@F�N�����gZ�ܝp$*k����}���7S1��*	�՞˓�ST�8�/G�R�����/�V2kjJ��uH�~�����>�8��0�g��&ykJ�):��E_�X�[/����+����;%j{���W���cfɣ���|!��
�N����^�4�ѣ_HP��NcJ�W1��T�+,�����\��v"X'�fE�k����2�����k,
CG�^I�}3�A�������Z^ǁY�Q�<��$CqקN��	�R�hZ�_f�P3%�؜y%u���x��_U+}ڦwG�e�o����[�.�l:/gϚM�}�]M��`��tP���cnK�ܲm��Z|9S�U��=[b���6Q-aT��k��� Wzs�;���w�Z9gD}M������I]1@���"��/v~C���̌펅�GFv�|K�Bׅ�����/ώ�a��?Њ3�	��g+ ��o��&�,����{��_�t���m�V�z�a���N����K9��_���F�BI�RhOe} �i�u1e�a���o ,e�_�߂���/�����Tl�1Y�!X�R[�L�ކ�!�v��_�ou����q+�[�����>��͐=�>��z�<�BnCa�n�G&�T��2�UdͿ�9{��۾�C������nF�*����*@j��Xӯ�C#��9Ѝ���e�tr�WOy�-¿�V�}�i½Y����:Z\���x���E]c��$鋟�'�G��H,⍰� �؀ߠ%֌n k�_����w��:�����׍��As1ڧt]����4�?T��^z���IQL#Dxxn�-�+�-?�ͻ�~��B>t��cQ#Ĩ,e*�wxv��#a9�IYQ~dFvl�+-F��=��YG�L6�δ���D�CN�Ty�/�N%ӥ�뵯���ff�0Fxj������wQn��3���s��:U�q��l剎msC7V3�ʍj��:���)]����BD����k��I�N7���@ᰳ���EB0���~�oJ��%xk�r�]�9��>Ք�%$j�.�WCop?u)i�]�ԘP?� ��P�@_��}Y/�ڏ�R�s�)�α���}C��������n����	���g�#�Ǒ�OE�5^�]<�
�!_w�l��}�9��O���L��B���9�{�Ӝw��p��u�
Se�H���
��:�ֹp]Ͷ�9�xP0�d���U�q���2�x��ߡ�s�p��7��.~`3�)���(�Zz@�r��i�t?$�w#*&�q��1Ϝ1M�2�w%&��Pp�ZO18�v�A֑hn86yf�8��Y��۷�2����%��W�J�'l������2��Q&|?O:��U�x=2[P�#�v�O��q�SJ*�_1ӡK$�r�Ƽ�_(�(1b b �
�%�aFp��oO�%M8ͧ�@�
�P����E��Fɱ>����y;Do2 ��e-n8(Az˺AQ�~�u.aҠx����r��y%l��'�ܓc���� 
��1v�Q��}Nª���9F7�<���S�����ʵ�W:v�zL�ܔ�����Xd.����D����k3�]*;�k�r��u搗�v(����x�m$��5�H3^1���'TsO���5syA��V�@�h��#�0[1�!��3�|;��_��#¡jX�.M���?�H_�YyQ��:�	���ϡ��a;ѳRF��N�;\ǐ?ɔ�ThE�w�py2���u�I[�.����1L�O����    `��*|RI4�PI���z��+�;�r�"Ny߰	�6uc(�ӽUf�X�J?���:��R����ʬ��E7ǡ��p��c�\P�����!����V��]7�/bb�|��zf���h�|1�N�}�k���Z@��w����V��G+v��|�+�0�&htO�⊋�FP9DKU1��F��M�ޥ����&��1��3�����X�E��k]֔t+�[��zr�w,��'A5�]���AF�ːOk��[�1��4kl�׆f���[�g�鄮Qo@� Ż���P`
X�4�k�7�g��n'yzUi�uy�i��w���3�@�u�j�.���;T�\ae>�uE�/�sX�t�ˆ��Ng����a�P�R�KM=ח;�'�	(8V2n�l����E��1��$���nX`���#�����<C.Fn�B�9{4�Li�˛��@$\z]qN�V-����S��g�F�
��_�.��t���W��Qد���t6(b��iyt��R�X^��_�b�q��J�4�r�``K�A!�-���!�/���'�j�:j�昊�����Zr>~�XG����:�Gl�QV��C�@I�3<�H�M���^s��<���2�J�j�(�^$BB=y����L5�c������*x�nr��?Q�zS��eī݀�$ٰM!3>t��#�|N�'���,�zp����[Ǻ�y��
-��_�C�dˏ��t��י�P�CS7�I��⚉�U�C����񺠝[� �q��&`ʐ��0��4�_ݍ|����N��n��n�O<S0$�nLvP�n�Z�!>�����E���0[Dxu��z�,s\��{_9�w9^���^^!0��ܔ��q��;����'�G�����M5���
�o
��v9\�_�޷ ��N!�����Q��0&qp���=���/7����w2��-ƞ��a��uD��}��p,�)���������0�WC8��oco+=�U��?�p_nQ�(L� ��ª�V��2Ho�ڡ��s#��D�U�5��I�N28���(;
��l�6c����:���I5���{�8`��/�;�*xo;��K���k*�8��t��B�Sg̤I��U�,����U�Z���Xs�n��~{c�J����i��h� <��xZe�ո��PL����9D�V���i�&%�:m 7�e����_��3��)-@��(�����]i��8���!�̅��tS�Gs�r8��g��N��u!�4�	s���P�¤z��u���P�EDk�� {�WGt;-���t��O�����g����o���fCX������W��ԅ���i'��G�%a�V-�-F�*�=���ʄ6��'d:�V!��_ݶ��Hn�Q���>:t^t�*�/�/�҄D��	�m�$�_�C���$����'��%��'�{��݅���t�KIB����r!�O3�BY��Ԯ3;dt��L?dD��m;k��]���*��\�"�6vCr7���xx�I�a�:E��O3'W����a�!V?=�0��JD{S�t�r�{��_v|%��{i5����Si�T��0���齍��::������ca�Rw=�/^�g���0v�6�/F(��P޺�5+��Ni�@�xgR����N�B^��>MA�H�6^�+G�:��YN'����AuHGq����/trźpݷ��3:
ى�;YZ��F��k�\�⪞���<+D����ؒ�ax���G�M�³��
g��Ȋ���	i�?puCN����+�ԧ�pZ�썁���P��Ys5Vּ<��.���dѤ'�������c$�`	�����4�>;������n�C_G��J���!c��/n�P�J)#�z9u�Hܒs[#�����C�L�?���Jշ_��P�� %�+�~�ו��D�N��L�ж�ȴ��+a9`ڬ;d���3K�~5��Z���K�~��D
�:��وGOk_^����Mt0'��(�H�G�BPN�Rx��DM�����L�!�wո�����JJ�/:�y���/�`O22?���ʝЈ�Z���`���\��7?uy�U|5�>�\1Zf���-A�Yg��g�ռ`��2��D�jo/i��٩�S�[e]]0���䷫]O6_��M9� ��ްJ4ϐ�)J<���+�,�0Ӎ1�2�?lD駄�}s(;u%z��>�W�u�g�+��� �)<��{�\�v��0��Y���L!;j�b�*E$
nKix�w����ʺ@q�ئGQ:�˹�#wHG��(L��nf�R��ʿf�x_l,oCgg���{�[v	a@�f�d�g~߯��s/\Bv���y2��#yvh��&a������m�
���:!����
B�^_��l�iC�@*)p��|RXs���eDp9�v��p;+	G�@��{�{�1���p�4�����Y���E�wȈ��YS;�T�Ӄ �ԫ��Te(� ��?hp��V�/�vf<J��R�r?S���?�J��&��n��u8��r�u�g���D��W���۷?�R�-ֳ+Q�L�>X�9}�-���(��i*��k�Q�`a��ߑ�h1���?@X�FA����i*��_���#R}�e�c$ՃF�Ǩ�B�Ȇ��u��g�@����	`�a3�k	��A9���\%����ݦCpr��`k3:�x�3cE����?KQ7�(k5���t��k�y�=�0�����NǨ0V6�k�m(-D�M�5[vc�?{�#�W|�~���f�0�i�r�RR�|{��^�A�#tMk�7�>QGa�8�JV?Myp�t���v����\��9r���m{:�v.T��\(��{M n�$�E&�����0�M�
��Y���s���6ݍj�I}+>�{��:����#����3�7uQ��5#e�gT�vAU.�x?�XY������f�U(*_kVe*�(m��ќ�V��B{HS���y�b��s�1�d��(�Ͼ$��$�Z�43�x���;� �FX�Z �U���%L��=TeH�f�c���\��t�ڕ+���0�r� ��`�M�b����)����١�I�Bmy�~>�|4�{=��#��>ȹK��`%�(��p=�5�X�1h������pET�� Vʧ�@�)8���venA�EIx�;����x>�1�U�i�z]������� �Ga(0L��ƼcwL#a��:凞��$<x�ʳu��uT��!L)l̰����;�L�m*fe�\���.� ;ؑ��I0#mva*������Eի�?;�z��j�."7J�MsN�x�n_Y�nG�-es=��eAZv QP��MפE@�������2m%��{�I�0�1���Z�E��7�DN��S��+�Pw^O'��鸅}��ʓ���7�؄ɱ?-������]�[j�����Z��m5���g�{�:�)��of��7�u�G�OI'�4k5��y��zs����6H�Q�av��=L3+��l�����4ڜށ�>x���L-��̂=��*I�F��5�*���i�0����F«\�1��fž�Η7���X��S����j��k0V�m3d�����i:Ò�d�����7�l��>����/xB�6+���͘ﳥWNTJ?��{ɖ5W�E��{s��,
M��|ޙ��Ӕ=$��kJ��r�kO��Lm	�z���<�v`�}�:}��
h�	���^Kzz�=��1X _-��o᪁��!2�U>���Q��F��-�|:V�[n���_>M��)yFY�m��6ͱ�������Re;K&���7��t�
�h4�:M�
?XΊ!�`������Pnݚ�J����!��Hl�禣j]�j�����U�����cX���P� *����|��i�w���t�-��>I�I�a"o�u��˶:�[�c����
5��Zn���<{���V�q���Wh<i-�����k���vлΏ?�A�q� P�[EbGYX(��i�����u�8���-��Cy�يܦ-2x�/��a�u��ߌ���mB��(4�*�O��V��0�߅�l�Qzk���ČJ�3tw��no�"�    ��w�;NA1L�OS�CV�ｃUL���mi�{��@c��P��t�>M��gl��\��n��[PX�r��tNe��
�!�q��:���[)O�~���P,^O���0�P�+<��
���]3�
I�y_A	h#��x�sjq�>����j�\�tKюñ� ��imWe���/��:'܆O3�P{���ޔŬ���>%�?� 
��2�����F������sLe�9��kv)ʧ�p��+�3?M���I����c�h���U=�� ����h��J�.�T����,��JRJ�ܙ��m��e�Ul�4s�AJ��EO"�#����z"����,�����?��!�x{�ӎ{�k{���O�
]����P��w�랍�t]��>��������K`4O۲��!�"�O�Tf�O"��(K���.�7�RNW xM���TN�!��nn9!�� Or�+���V�㋷������79OwH�C�ER�9�w.�g&��i�L�G�ڐUN�"�'OO��)5�6aA�m��,�q�R��Ȇ.Hq��ԇ��F_�O���wtzu��ʧE���t�n-*�;O�#�����_��S��T<J�J@�uT�e����rX�:pe��fzG�+��ű��d�T�`����%�t��%@�^�m�9L��_�g��]x���lw'�y1LT�W_��ӰMrY׋;z;�����`l�ZHǶ�����ɷ�)�	y�t�벓�]1J��Y�t�t�ֶ6Wsn��k�����q{)��9���Tʠ�و�;���cA!��"}-3jY����J�B��s�5�K2�__�pL�ZU�Z�p�r���9����1�^�P<���N�1Ŧ�}�ԔHhr��8���XV��c�m�E�g�)h��;�����Y�Q_9!j�(��2g������hز��{ԘzG�"�!cT
N��g���x���.1�!���4	}�P[C�v��T��Ȭx�̧���e�?�(��X����I�����X��}뗖yW���:�)�;}{e-3pt��'3Ҍ\M�#�"�wa6�z^�P��Q4z�ث�7�#ҽ�S��Tz*W�߾�!v�	"�Kgg��~�^���AʙH�ގ����*�X�u8j�Z�{R߂�wBٌ�x*�s��%�p��T8���v��0Ղ�a�B�`�E��;}Mт/��|�7�z�A'G��M62���D(v6g�K_��2�麑�^K�2䩪��Dq�%���n_η?,�rx��5Eqa�	m�YY,BŞO��=G7_3�	�� ���#3�Ы�e��F]�=�YWӛ}�� �}}�kn�z_��BiV4��PSZ�됯W�B�sk��5$h]�*��6/�/�Q�k\̚�c�"!jv����p�<���#�}�R&<�ח+U!w���dj�A���$r���(�v���a���!�z=���ma��&�VK}��|�P|�b�R��n?QF���df|cC�ₛSwķ�BfĤ!^3P�\c��V�lr1W!��pG�=w�mŒ�����`�&�+il=k��'ӛ]4����\��B�_�XT�l��ޥ�Q�$5��{4{C�c��^A/��s.��~Zc�R��mr=K�:�y�����G�2��Y;� �}r>��0F�6�\�3]m��;���C	�ji#8N�2�C�Q	?L�ny!���=;��L|���{������HV�HR�m�4z&���������ZN�`�N�u�DeLa�<z������Y����Wj�R�V�(ڠ�H����k��d�x�����~��z��E]�9�8����Oc^���� ��	R�*g1��Py�f�E��#���?e���f�B��S���Ta)=�-△;	�"��G�Х�����hr,TB�0Š�2q��5"�s92#T�z�r}%f*�oSٙg|G ��T`/R(�\׼��LN K����/�;�/��S�k���L���\|G�WXн�i�Qy)@	����2�m�"If%$�����(���^d�!�����&�=�OƦ�6_U�nܥ��Q�'�#��Kpe��>ކ�9ԗax���kA�񌁋@����R�q�(��P�5D
������,��1A��&ၠH�<���j��#�����:���%$O���bei��}��[u���0Cw�X��A���I�r�p`qv;�2���D�c'�n $s�V2,��0���l�f#l�7��._EsB�7�gyn���簾��Ԫ�RIs1-F~�����!\�7g����a�pB�Լ�YI�Th{����_��t��s���X���>��0��8��09D�<��4U	p���u}z_�B|_3�a��������L�~w١g�Դn��4�n�è��>yc|�3�xFib�4u������ח��k+E��濠֐su�
m	���F���@?>!�y��.�3�-<�qE�������E�5��-wU�Ty���'�^�j�%(�&OkP}�b$�]Ғ�u
�{�qK�Q�UfGz���I����Q�g$�r���e�X�ӷ��+�|J=m�0��r���B��^�ӕ6�7=��#]�pЁ�rw_�'��"�~�h�牳��3�^��"�
S��^?[�n*��m'<L�4�=��ąPq���~1RN�?�4���s��p*���V�F�ߨ|��gx&d]m,��B����D*��g�E�g�2ݗ��c��7ǔA4H\�e	д�Ƈ4��ݵK�⟦.6��jh+ ��z�����$�D7��G�#f��pIZ]H��ge6 O�Z��M8�'��5��svVٯ^����0IB.2��_�#En[�=�P?a֧�+v<}��Q����'����Z���b�V�o�B���o�̐,POP�3�RQ	�_�,2�Z��i��6�p�rsP�Q�?�4J���z�4��?�rVC�z������V���k��ܗ��.Nv�a`V?��(r5��6��B��c����굦k�L�%v��
5<��W)����俳��?|z�m��ef��Ԧ�}����OM���Ln�n+��O�,��#�ix���~�����x�h���r����-��k��׷=K\��"1r�D������;;9�߉���{�3M��lt߳c����k�Sv�O<����S���R�/,�ŌdDo����h���v�_��/7��P���@��X.6���m�k�R�����C�J�`�M��ȏi\�n�i��t��4~_g�̋�ה�WZ�n����C�C�@��l��]�Da��98,��A�k��"(w6H�d7��R�<w������d���z�-�^�c�:ea�;ܖi	+���g��Q�2�>ӛQHw���9���.��(%��,�3Vi,�����c����h��{�E{*�Lm�?�RԻ�D�MGʑ-�n��u/訤k�О�5�����͸��}�}�'�'�lkC�Zmݹ�/>5P����7]�[�ӣ����*2����r�d(�Σ���(�0��v������,�4��������e�ږ�5��$j,VP�=��~7�˅e�Pd��QO��f��WX��eXk7+�T�п�x�
��������/J��
B6T���=�R����qf���E\�Bd1��o9"������*��*��'S�+��^M:�� ��Έ:�2����k�e�-��5���L����\�(ee\�6AZ����a�;9g˙��������로����@6\��%��L��m[��}?��R�P6����܊�U�3ɻ��E�1�7m5���1����
�_nֲ���߅x��Q2�r�+���%�v{������F;���^h�>}lQ��?Ft�2�=��'Ϲ+�h��t:����:ѯ�݇�>�5�3���W�89����l��j�L���l1�2��=ݶy]]#�'�q�S�Z�p��LPҌ�5:�X6��R�9�1=y�X,X�iL�G`bnN�]�<v�JV$�� 
�	�m&�@'2D����������Λ�溻�u3=��2����9�f��)W��Nv����������7={B^<cF_*(� 
  iq��7u��=�>�k��F"�-��L�ZJ�b��N :y�k�`S���}F0��U�
�=��>�s��(���\��5���W�JV��sd��V�ݭ^[�m�B���C���fX�'ɗ`Ȱ��2$�ͮ3��ېp���qK��֛�L����4D�$��*೓����s�����'��� awD�i��9vR�3���~t���|�
��0H��-䚸��w��o��)	����ٔ�����S�=z�zYh27]���_O6��9}Z	��مp=f�S4X�M ���vJa�lk�M8����%J���'�N��A�#��"[e0���c�K��͜?M�#te��&�NP*|"����5���u�WJi��8�[!���0:�����u���Is�PRc
_��S�jA&��j�xD�fc���a�ະ�zq��H�ݖ�=�N�K�3E	2]��m|�����nr�������z��H�<-T6��r���UQ5#X:3ñ�OS���gs=�����)W��*�-���g/g����(�1�����N��ZС�V�p\2�0��dz�G�oS�1箧����ך�f�?��������FCwy�Cq����hֺ&��,�D���N�s*;S:C^��
eg,���;�p�\S:���3b�o�mu$̛+ng�Z93@�δ�k*���S|��Th����0-9]ߌ�tU�낖�0|3%l��ҕƍ.�֟-L�=%4����c5-C{���y�a������[L���\晝J4����3e��*�-�8��y"�a�*��s]���F�w��8�o�'w'���n������~E��J�s�++b��lỹ9wJ�3C\3��Z�A�����Ā� �.��Aϰ������ku��*�C0�+�5ע�ym&f�T6ڠ1��'��0B9���k� �h��]}rXE����{J�����!�c?9�r�J\�Z�WӀ"-Sg�������_��|�:�)�it6&����yC8�� !���ݛ��ka��졣�~<B�מ�	q-sA-�+�V�JlLm�������Cb��r��l(#���܏zw�:�
��	���%���>֟兌ȅ+�	���;X�
�d�񉵏Z��J�>�JX���={uקgR�A��WU����7��<S�4��{p�:n�%���v8�Z6<z�(���b��j*�3���H�AavW��щm"&�&$�Ex$���l�4����J$0�	��s����iHLlٽ��@ء���a�9�K��1ԡ���\�����k�C�+��8�(�D�Z+�f�����!�����^oq��*vb��t�5�x<m�m�3��S�k8��5/0g���.���ͩ��f�
�W-͡Q_�_˙�^��%#����4[@xĶB�k2��!�ߔ�����ÄCVlM�� ��k)��ފb��З�޳�?
)�Z^�����{��/�^i�p	w՞��Y�M���r?�H��q��^��	����9;�E��q}�Q�O�@R��M!N<����<1ku��	����U9�2������>,y(��"õ_��)�O�A��Py!X@ԭ�����>Q�~͉ܕ�8��b�q��:Ŕ�����Q���_�)�,�Qx�֣=�u� {v+�W�G���OS�L��+x&����L	��+[tP�q+�����n_M�k����� ����XQ� 7���z��"�(dF��uc��Եg��h��ё�y㠿Ջ���4�޺�-�
��_'�\ө�>�[�l�j*Aw*P��M���t�d�CЧn�(Y]��k�0N
,~��gC��ܞ:�o�h&y`��g��	>�}�Jg)�3vrM�Q�vMa�{�O]���ޟmϲj�B�<�ߐ���������WMgZ�) d��0��!y:}�ϔN���w�BW&TMGN�X���ꊍ@[�{��q�D���0��a��f��Pp��[}4c�v�L�%VC8p�={�1h��2��Ф/�,;-��k��ǎJ��F��

\��f8�"RG+zB��	+E�'z#�c�&�p�����6�7�(�����S�� �:���PE�{��2�w��G��7�3��՛1��6�Ċ�z�7�{���T�یR�`D��S���j��:W��̡�.!�b�_�u�F�n��E1%��ҀK�b�~�:��9�k��aC����)��������D���M~�\�I�}�;S)ǦL"S<���V�T>���J�2�݇)hN�b^�w
Y�Wg1�{�Q
�3�4�����p���	�h$�D�ѵOu���Gj���ȸS��:y�c*|�:[4�R������gx��8�W樛�?9w4;e*~�J����x�"]ܐNֆ�P�-S���,�0�k�[�S��*�:{�M�ugIA�ޏ8�V���>p����S�̆(V�l��|�S�ζR��6�_Fy�Ε�]-�5�O��������u����xH�-*��>������R�w���Mﺮ�
n0s�nF'�R�R�4p{'��O�$��5����
6�A�'
� ���f;�X�8{��䘙F�3���������yi�      �   �  x���Ms�H���_ѧ��X-��xt��qi<vhǮr텦�2���G��_��-�S��� ���~���j=��U�ۇ�A�cy�[g>Al��֦�Hj�N�uX�,/u#�"��K&�`�`����(� �#p�*L#�$ag��mX���]�:���YUX�oӚ8�ZO�1,m/�.��է��^�T��Db�Bn���)qY�q�xsIm6'�$aeQ�q$9.��\u�a��D'a���-+�p�P7��S��A�3O]gqZ��I۴��K��e�*$�%����Tq��ӭ�О&IV�X��lW���%Y�;.���M�X�ͳj︾H��+��i��� �����IwA8\UE��l~rQ���F��z�pf���	i��:϶ Q����ݍd$�́���Y��J��76�G, �ESI��K�X�A����JgAZ�pӜւ��_GF���j��q�G�xu9�?�	���)��k �f�,g��ŋ�Q����'�����tUSR/��k��p�.ҿ��3�|zؔ6i�w<�����ϥ�M\:�R]I.}��U��+B���AG�6z�YŊ$9Y�_�<ӏa"ů?i��YNԵ��R ��Y�Ar�$#�Mxp�\2R�:��M �-�Pq�z���a2e�׸7��xZ%6L�U�[���ECAe:�@��Q參v�vH��F���:L���|����?s�?:�%��HM��Y�|.��4��·��Ynߨ �����^G���S���/�?C1UZn�n�?W�d�~R��|�����}��dG壭�����X�!�ϧ�I�jͣg}�%q��>2W}ɲA��2�����*��; ��V�\6-����"8�8�аE�M��6�>�%'�9�r�E��EX�p�MS�������H�
����.�����z�������k�J��60\�u�C�7���j4���kR��G`��\XKs��"l�I�W����;pCP��ЃNڦ�ء,~�E+h�n u�ix�AP��k9Ә��!%c���
��>�v���oՅ���5������S[w����e��%pgE}����?�8���[�+H��I_��ֈә�r��ː ��$�,ƛ��n��g�:�qM�ݓ�����}tR�Ci�5����z�f�;.���NO C@��Hl�0z	���g*�Ͻ�l��2oK�gz�z^� �*&��wnS�w���/`��ͲD�  ��]��&�θp�/�T����������'z�/��#�11tR��İ
iu�sZ�!����)苳rp��FB�����f��eT_Y������l������L�LD����l��`<u+���߄e�h��5W�w���QF�PЧQ$M�������
�`@�	�ofg�V��>t��C���d������2�q0���*+�X�젨�\f��~Ċ�6]L����8��h �t2ڟ��c(�V��W�������;���)�I�h��>��\v�uyt�K�
�v\����tk�.���#1�qAw�Q@����\�双1a����T\̱��j�#g�MV���Y���Q8���6�I)J���b2�m4gլ�O
*ZOZ��͘�� ���N�Śi�z���0���PhbMM6�vL�_�"�*���wڣ��f������}��F�U}mf��# &N����B��ɬ�Y.��UK��L�-��h3aZ��~���Y�^o����LS���,^���1��nT��cK��;۽c���C���B2�>��Ev4"�b�)4�C�^nq���__��5���N��|���
���t���O�߾諔C��F����ä+�]���:�I�ȋlD�<AFg���_uS{��+�&���δ.��06�a8�I��N?2�<:�~7
'�12t1�{C)��6,ԃ��٩�(i;��3C�q��7u�����vY%e�O������3p��I��m%n�J�7ك�*��$߳2b5 )���~��Vp���7�S1��m�4��!�Q�<�d��G���%1q�{Cj�d���>�����a��bw�E� �*�'yP��<�G��YO�^�q?��R��@��m�h����U��8��AӭH��M�T��D�N.�[�&�*Q
?o�m��߃���ôoM�R4�a�5�W����v�A�q2��x�A���}��$F�ݱ0Y�!�7�>t�O�&��u��L���0���ο?;��_@���      �      x������ � �      �   G   x�33���t��Qp��II-*�,�4204�50�50V04�20�2��362573��/k �X������ rF�      �      x������ � �      �      x������ � �      �   �  x�}��n�0�g�)�b�GI�/�)y�,�ӘB,�	� �LE�!c7EQ�k��20/�7)��V�8���#%�/���>[��P�}�Ł+*��+8k��vM�~�EQ��m�}ssv8�}�}w��7��%,�����O8XߕqB=�4��/�qҏ�d�	�ل�Dh��l�P��c��p\��E��p2�N3)�4�Q��HJ󜸖�9����Nc��cΖl�n����å��ml���n��V���CN�hw&���&�o��iC�r>U�^�(���)�ҔG���~:?.�o��!��:���_�t{þ-Z��Z��w��#����y��M�Rq���2�g��B���>90��b	;�bD���j^�?3�mp��x���
_!Ո"��E#�4�S�T�9J�$����M      �   �   x�3204�50�52W00�#N���̇�g*dޜ�����pwc�B^b�B��
��wOLV(y�k�BNb��w��K�2��f�0�(/�R�,�Qj���4���L3�50D�fJ�if(��c����������K�22����zS]C����/�H-JI��ʚ�"����� ��*�&V*p��qqq 

�	      �   [  x���Kn�0����2�{��=A68����Q���!��yH���K����V� #V� ���5���s<�,`��pJ����m�}�l�L@��sus"b8:�=F�e���(C�����K�+�"C����*Os�r���D �-�.-����h��N��Bt�t:��c١22fd�R΁\e��$}7e8�f�����X��i�Y����c�?�w"gTU\6D�l��X^+�˾�z���F��c��țg�M˾�����+D��d$�'wi�0i��dj(wIv�;Vf	�u���� 7�JVF�:"�8�������ұ��������fa���H�˾VMUU?�@��      �      x������ � �      �   �   x�����0Dkj�,`�?}�!2�KC$�#���6�d�;��)p `$Y�,hD�{y+��ىT��sϪ�\ O��ج,��'`���c���Į��M5B�<�eV�K�t�N��)Du�����mщ��}uA9�o�xmc;5�j^̱e�*lp���(�=wcU�f�5��>�yT      �     x�e��n�0���S�r�Jd;vN�'�4����
H@U��@WvP.����:�C���?4�z�q<�l�c�MWjr�p9hk���F��$x![�5J"�2�\�j�R�mʭ=��T.O�ZK	ϻo�G���S3_ǵ�ҢPT����.�\�.�Q
4dgk�gZȜ�����vj�K�k�U$	9Q41k,�sdI�&��G��[ٲ�����M����lݴ^gW�Db�;���xmrBkl 	+jPs;�v���IB�p>C@~]{SU���j�      �   e  x���M��0�ur�^ �%��s�Y�	z�F�؎�,��C|Yr(��F[�?�GH�Gh�!�o�H@� ���S��Iq��#�;~fTA� �fx�p� \&<�S/h o o o3N��
���s���;q6��z���.�w��G.�y�WA'����q��j'�x(�������	�ڏ����G������Y6�b�0-`�4�8X"���6�k/ s������<��8�O	�	�	�i����E3��i�yZ���-�y�-4�-�c"�FQ��kz�_2�R_s���%��}C{ϙ<���<�L3��z�Yw�g�?��B-q6��r7�gv�-c��x1|������d���     