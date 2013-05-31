PGDMP                         q            pos2013    9.0.3    9.0.3 (   �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
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
                  postgres    false    7            4           2612    11574    plpgsql    PROCEDURAL LANGUAGE     /   CREATE OR REPLACE PROCEDURAL LANGUAGE plpgsql;
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
       pgagent       postgres    false    564    6            �           0    0     FUNCTION pga_exception_trigger()    COMMENT     p   COMMENT ON FUNCTION pga_exception_trigger() IS 'Update the job''s next run time whenever an exception changes';
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
       pgagent       postgres    false    564    6            �           0    0 #   FUNCTION pga_is_leap_year(smallint)    COMMENT     W   COMMENT ON FUNCTION pga_is_leap_year(smallint) IS 'Returns TRUE is $1 is a leap year';
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
       pgagent       postgres    false    564    6            �           0    0    FUNCTION pga_job_trigger()    COMMENT     M   COMMENT ON FUNCTION pga_job_trigger() IS 'Update the job''s next run time.';
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
       pgagent       postgres    false    564    6            �           0    0 �   FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    COMMENT     �   COMMENT ON FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) IS 'Calculates the next runtime for a given schedule';
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
       pgagent       postgres    false    564    6            �           0    0    FUNCTION pga_schedule_trigger()    COMMENT     m   COMMENT ON FUNCTION pga_schedule_trigger() IS 'Update the job''s next run time whenever a schedule changes';
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
       pgagent       postgres    false    564    6                        1255    234863 7   checkserialnumber(character varying, character varying)    FUNCTION     %  CREATE FUNCTION checkserialnumber("partNumber" character varying, "serialNumber" character varying) RETURNS boolean
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
       public       postgres    false    564    7                        1255    234864    newid()    FUNCTION     �   CREATE FUNCTION newid() RETURNS uuid
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
       pgagent       postgres    false    1756    6            �           0    0    pga_exception_jexid_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE pga_exception_jexid_seq OWNED BY pga_exception.jexid;
            pgagent       postgres    false    1757            �           0    0    pga_exception_jexid_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('pga_exception_jexid_seq', 1, false);
            pgagent       postgres    false    1757            �           1259    234870    pga_job    TABLE     �  CREATE TABLE pga_job (
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
       pgagent         postgres    false    2182    2183    2184    2185    2186    6            �           0    0    TABLE pga_job    COMMENT     .   COMMENT ON TABLE pga_job IS 'Job main entry';
            pgagent       postgres    false    1758            �           0    0    COLUMN pga_job.jobagentid    COMMENT     S   COMMENT ON COLUMN pga_job.jobagentid IS 'Agent that currently executes this job.';
            pgagent       postgres    false    1758            �           1259    234881    pga_job_jobid_seq    SEQUENCE     s   CREATE SEQUENCE pga_job_jobid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE pgagent.pga_job_jobid_seq;
       pgagent       postgres    false    1758    6            �           0    0    pga_job_jobid_seq    SEQUENCE OWNED BY     9   ALTER SEQUENCE pga_job_jobid_seq OWNED BY pga_job.jobid;
            pgagent       postgres    false    1759            �           0    0    pga_job_jobid_seq    SEQUENCE SET     9   SELECT pg_catalog.setval('pga_job_jobid_seq', 1, false);
            pgagent       postgres    false    1759            �           1259    234883    pga_jobagent    TABLE     �   CREATE TABLE pga_jobagent (
    jagpid integer NOT NULL,
    jaglogintime timestamp with time zone DEFAULT now() NOT NULL,
    jagstation text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobagent;
       pgagent         postgres    false    2188    6            �           0    0    TABLE pga_jobagent    COMMENT     6   COMMENT ON TABLE pga_jobagent IS 'Active job agents';
            pgagent       postgres    false    1760            �           1259    234890    pga_jobclass    TABLE     U   CREATE TABLE pga_jobclass (
    jclid integer NOT NULL,
    jclname text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobclass;
       pgagent         postgres    false    6            �           0    0    TABLE pga_jobclass    COMMENT     7   COMMENT ON TABLE pga_jobclass IS 'Job classification';
            pgagent       postgres    false    1761            �           1259    234896    pga_jobclass_jclid_seq    SEQUENCE     x   CREATE SEQUENCE pga_jobclass_jclid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_jobclass_jclid_seq;
       pgagent       postgres    false    1761    6            �           0    0    pga_jobclass_jclid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_jobclass_jclid_seq OWNED BY pga_jobclass.jclid;
            pgagent       postgres    false    1762            �           0    0    pga_jobclass_jclid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobclass_jclid_seq', 5, true);
            pgagent       postgres    false    1762            �           1259    234898 
   pga_joblog    TABLE     v  CREATE TABLE pga_joblog (
    jlgid integer NOT NULL,
    jlgjobid integer NOT NULL,
    jlgstatus character(1) DEFAULT 'r'::bpchar NOT NULL,
    jlgstart timestamp with time zone DEFAULT now() NOT NULL,
    jlgduration interval,
    CONSTRAINT pga_joblog_jlgstatus_check CHECK ((jlgstatus = ANY (ARRAY['r'::bpchar, 's'::bpchar, 'f'::bpchar, 'i'::bpchar, 'd'::bpchar])))
);
    DROP TABLE pgagent.pga_joblog;
       pgagent         postgres    false    2190    2191    2193    6            �           0    0    TABLE pga_joblog    COMMENT     0   COMMENT ON TABLE pga_joblog IS 'Job run logs.';
            pgagent       postgres    false    1763            �           0    0    COLUMN pga_joblog.jlgstatus    COMMENT     �   COMMENT ON COLUMN pga_joblog.jlgstatus IS 'Status of job: r=running, s=successfully finished, f=failed, i=no steps to execute, d=aborted';
            pgagent       postgres    false    1763            �           1259    234904    pga_joblog_jlgid_seq    SEQUENCE     v   CREATE SEQUENCE pga_joblog_jlgid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE pgagent.pga_joblog_jlgid_seq;
       pgagent       postgres    false    1763    6            �           0    0    pga_joblog_jlgid_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE pga_joblog_jlgid_seq OWNED BY pga_joblog.jlgid;
            pgagent       postgres    false    1764            �           0    0    pga_joblog_jlgid_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('pga_joblog_jlgid_seq', 1, false);
            pgagent       postgres    false    1764            �           1259    234906    pga_jobstep    TABLE       CREATE TABLE pga_jobstep (
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
       pgagent         postgres    false    2194    2195    2196    2197    2198    2200    2201    2202    2203    6            �           0    0    TABLE pga_jobstep    COMMENT     ;   COMMENT ON TABLE pga_jobstep IS 'Job step to be executed';
            pgagent       postgres    false    1765            �           0    0    COLUMN pga_jobstep.jstkind    COMMENT     L   COMMENT ON COLUMN pga_jobstep.jstkind IS 'Kind of jobstep: s=sql, b=batch';
            pgagent       postgres    false    1765            �           0    0    COLUMN pga_jobstep.jstonerror    COMMENT     �   COMMENT ON COLUMN pga_jobstep.jstonerror IS 'What to do if step returns an error: f=fail the job, s=mark step as succeeded and continue, i=mark as fail but ignore it and proceed';
            pgagent       postgres    false    1765            �           1259    234921    pga_jobstep_jstid_seq    SEQUENCE     w   CREATE SEQUENCE pga_jobstep_jstid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE pgagent.pga_jobstep_jstid_seq;
       pgagent       postgres    false    6    1765            �           0    0    pga_jobstep_jstid_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE pga_jobstep_jstid_seq OWNED BY pga_jobstep.jstid;
            pgagent       postgres    false    1766            �           0    0    pga_jobstep_jstid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobstep_jstid_seq', 1, false);
            pgagent       postgres    false    1766            �           1259    234923    pga_jobsteplog    TABLE     �  CREATE TABLE pga_jobsteplog (
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
       pgagent         postgres    false    2204    2205    2207    6            �           0    0    TABLE pga_jobsteplog    COMMENT     9   COMMENT ON TABLE pga_jobsteplog IS 'Job step run logs.';
            pgagent       postgres    false    1767            �           0    0    COLUMN pga_jobsteplog.jslstatus    COMMENT     �   COMMENT ON COLUMN pga_jobsteplog.jslstatus IS 'Status of job step: r=running, s=successfully finished,  f=failed stopping job, i=ignored failure, d=aborted';
            pgagent       postgres    false    1767            �           0    0    COLUMN pga_jobsteplog.jslresult    COMMENT     I   COMMENT ON COLUMN pga_jobsteplog.jslresult IS 'Return code of job step';
            pgagent       postgres    false    1767            �           1259    234932    pga_jobsteplog_jslid_seq    SEQUENCE     z   CREATE SEQUENCE pga_jobsteplog_jslid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE pgagent.pga_jobsteplog_jslid_seq;
       pgagent       postgres    false    6    1767            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE pga_jobsteplog_jslid_seq OWNED BY pga_jobsteplog.jslid;
            pgagent       postgres    false    1768            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('pga_jobsteplog_jslid_seq', 1, false);
            pgagent       postgres    false    1768            �           1259    234934    pga_schedule    TABLE       CREATE TABLE pga_schedule (
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
       pgagent         postgres    false    2208    2209    2210    2211    2212    2213    2214    2215    2217    2218    2219    2220    2221    6            �           0    0    TABLE pga_schedule    COMMENT     <   COMMENT ON TABLE pga_schedule IS 'Job schedule exceptions';
            pgagent       postgres    false    1769            �           1259    234953    pga_schedule_jscid_seq    SEQUENCE     x   CREATE SEQUENCE pga_schedule_jscid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_schedule_jscid_seq;
       pgagent       postgres    false    1769    6            �           0    0    pga_schedule_jscid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_schedule_jscid_seq OWNED BY pga_schedule.jscid;
            pgagent       postgres    false    1770            �           0    0    pga_schedule_jscid_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('pga_schedule_jscid_seq', 1, false);
            pgagent       postgres    false    1770            �           1259    244946    base_Attachment    TABLE       CREATE TABLE "base_Attachment" (
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
       public         postgres    false    2279    2280    2281    7            �           1259    244944    base_Attachment_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Attachment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Attachment_Id_seq";
       public       postgres    false    7    1789            �           0    0    base_Attachment_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Attachment_Id_seq" OWNED BY "base_Attachment"."Id";
            public       postgres    false    1788            �           0    0    base_Attachment_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_Attachment_Id_seq"', 40012, true);
            public       postgres    false    1788            4           1259    256168    base_Authorize    TABLE     �   CREATE TABLE "base_Authorize" (
    "Id" bigint NOT NULL,
    "Resource" character varying(36) NOT NULL,
    "Code" character varying(10) NOT NULL
);
 $   DROP TABLE public."base_Authorize";
       public         postgres    false    7            3           1259    256166    base_Authorize_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Authorize_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Authorize_Id_seq";
       public       postgres    false    7    1844            �           0    0    base_Authorize_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Authorize_Id_seq" OWNED BY "base_Authorize"."Id";
            public       postgres    false    1843            �           0    0    base_Authorize_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_Authorize_Id_seq"', 359, true);
            public       postgres    false    1843                        1259    254557    base_Configuration    TABLE     
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
    "ReturnFeePercent" numeric(5,2) DEFAULT 0 NOT NULL
);
 (   DROP TABLE public."base_Configuration";
       public         postgres    false    2375    2376    2377    2378    2379    2380    2381    2382    2383    2384    2385    2386    2387    2388    2390    2391    2392    2393    2394    2395    2396    2397    2398    2399    2400    2401    2402    2403    2404    7            �           0    0 .   COLUMN "base_Configuration"."DefautlImagePath"    COMMENT     T   COMMENT ON COLUMN "base_Configuration"."DefautlImagePath" IS 'Apply to Attachment';
            public       postgres    false    1824            �           0    0 9   COLUMN "base_Configuration"."DefautlDiscountScheduleTime"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."DefautlDiscountScheduleTime" IS 'Apply to Discount Schedule Time';
            public       postgres    false    1824            �           0    0 (   COLUMN "base_Configuration"."LoginAllow"    COMMENT     \   COMMENT ON COLUMN "base_Configuration"."LoginAllow" IS 'So lan cho phep neu dang nhap sai';
            public       postgres    false    1824            �           0    0 5   COLUMN "base_Configuration"."IsRequireDiscountReason"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRequireDiscountReason" IS 'Reason box apprear when changing deactive to active status';
            public       postgres    false    1824            �           0    0 -   COLUMN "base_Configuration"."DefaultShipUnit"    COMMENT     f   COMMENT ON COLUMN "base_Configuration"."DefaultShipUnit" IS 'Don vi tinh trong luong khi van chuyen';
            public       postgres    false    1824            �           0    0 +   COLUMN "base_Configuration"."TimeOutMinute"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."TimeOutMinute" IS 'The time out application';
            public       postgres    false    1824            �           0    0 *   COLUMN "base_Configuration"."IsAutoLogout"    COMMENT     U   COMMENT ON COLUMN "base_Configuration"."IsAutoLogout" IS 'Combine to TimeOutMinute';
            public       postgres    false    1824            �           0    0 .   COLUMN "base_Configuration"."IsBackupWhenExit"    COMMENT     ]   COMMENT ON COLUMN "base_Configuration"."IsBackupWhenExit" IS 'Backup when exit application';
            public       postgres    false    1824            �           0    0 )   COLUMN "base_Configuration"."BackupEvery"    COMMENT     R   COMMENT ON COLUMN "base_Configuration"."BackupEvery" IS 'The time when back up ';
            public       postgres    false    1824            �           0    0 (   COLUMN "base_Configuration"."IsAllowRGO"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsAllowRGO" IS 'Is allow receive the quantity more than order quantity';
            public       postgres    false    1824            �           0    0 2   COLUMN "base_Configuration"."IsAllowNegativeStore"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."IsAllowNegativeStore" IS 'Cho phép kho âm';
            public       postgres    false    1824            �           0    0 +   COLUMN "base_Configuration"."IsRewardOnTax"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsRewardOnTax" IS 'T: SubTotal - Discount + Tax
S: SubTotal - Discount';
            public       postgres    false    1824            9           1259    257302    base_Configuration_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_Configuration_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_Configuration_Id_seq";
       public       postgres    false    7    1824            �           0    0    base_Configuration_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_Configuration_Id_seq" OWNED BY "base_Configuration"."Id";
            public       postgres    false    1849            �           0    0    base_Configuration_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_Configuration_Id_seq"', 3, true);
            public       postgres    false    1849                       1259    245754    base_CostAdjustment    TABLE     �  CREATE TABLE "base_CostAdjustment" (
    "Id" bigint NOT NULL,
    "Resource" uuid DEFAULT newid() NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "NewCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "OldCost" numeric(12,2) DEFAULT 0 NOT NULL,
    "ItemCount" integer DEFAULT 0,
    "LoggedTime" timestamp without time zone NOT NULL,
    "Reason" character varying(30) NOT NULL,
    "StoreNumber" integer DEFAULT 0 NOT NULL,
    "IsQuantityChanged" boolean DEFAULT false
);
 )   DROP TABLE public."base_CostAdjustment";
       public         postgres    false    2360    2361    2362    2363    2364    2365    2366    7            �           0    0    TABLE "base_CostAdjustment"    COMMENT     `   COMMENT ON TABLE "base_CostAdjustment" IS 'Chi show nhung record co IsQuantityChanged = false';
            public       postgres    false    1819            �           0    0 -   COLUMN "base_CostAdjustment"."CostDifference"    COMMENT     Q   COMMENT ON COLUMN "base_CostAdjustment"."CostDifference" IS 'NewCost - OldCost';
            public       postgres    false    1819            �           0    0 &   COLUMN "base_CostAdjustment"."NewCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."NewCost" IS 'NewCost*NewQty';
            public       postgres    false    1819            �           0    0 &   COLUMN "base_CostAdjustment"."OldCost"    COMMENT     G   COMMENT ON COLUMN "base_CostAdjustment"."OldCost" IS 'OldCost*OldQty';
            public       postgres    false    1819            �           0    0 (   COLUMN "base_CostAdjustment"."ItemCount"    COMMENT     ]   COMMENT ON COLUMN "base_CostAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1819            �           0    0 )   COLUMN "base_CostAdjustment"."LoggedTime"    COMMENT     w   COMMENT ON COLUMN "base_CostAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
            public       postgres    false    1819                       1259    245766    base_CostAdjustmentItem    TABLE     �  CREATE TABLE "base_CostAdjustmentItem" (
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
       public         postgres    false    2367    2369    2370    2371    2372    7            �           0    0 1   COLUMN "base_CostAdjustmentItem"."CostDifference"    COMMENT     i   COMMENT ON COLUMN "base_CostAdjustmentItem"."CostDifference" IS 'AdjustmentNewCost - AdjustmentOldCost';
            public       postgres    false    1821                       1259    245764    base_CostAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CostAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_CostAdjustmentItem_Id_seq";
       public       postgres    false    7    1821            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_CostAdjustmentItem_Id_seq" OWNED BY "base_CostAdjustmentItem"."Id";
            public       postgres    false    1820            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CostAdjustmentItem_Id_seq"', 3, true);
            public       postgres    false    1820                       1259    245752    base_CostAdjustment_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_CostAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_CostAdjustment_Id_seq";
       public       postgres    false    7    1819            �           0    0    base_CostAdjustment_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_CostAdjustment_Id_seq" OWNED BY "base_CostAdjustment"."Id";
            public       postgres    false    1818            �           0    0    base_CostAdjustment_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_CostAdjustment_Id_seq"', 6, true);
            public       postgres    false    1818            e           1259    271738    base_CountStock    TABLE     �  CREATE TABLE "base_CountStock" (
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
       public         postgres    false    2595    2596    7            �           0    0 !   COLUMN "base_CountStock"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_CountStock"."Status" IS 'Get from "CountStockStatus" tag in XML';
            public       postgres    false    1893            g           1259    271745    base_CountStockDetail    TABLE     F  CREATE TABLE "base_CountStockDetail" (
    "Id" bigint NOT NULL,
    "CountStockId" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "StoreId" smallint DEFAULT 0 NOT NULL,
    "Quantity" integer DEFAULT 0 NOT NULL,
    "CountedQuantity" integer DEFAULT 0 NOT NULL
);
 +   DROP TABLE public."base_CountStockDetail";
       public         postgres    false    2598    2599    2600    7            f           1259    271743    base_CountStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CountStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_CountStockDetail_Id_seq";
       public       postgres    false    7    1895                        0    0    base_CountStockDetail_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CountStockDetail_Id_seq" OWNED BY "base_CountStockDetail"."Id";
            public       postgres    false    1894                       0    0    base_CountStockDetail_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CountStockDetail_Id_seq"', 174, true);
            public       postgres    false    1894            d           1259    271736    base_CountStock_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_CountStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_CountStock_Id_seq";
       public       postgres    false    1893    7                       0    0    base_CountStock_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_CountStock_Id_seq" OWNED BY "base_CountStock"."Id";
            public       postgres    false    1892                       0    0    base_CountStock_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_CountStock_Id_seq"', 29, true);
            public       postgres    false    1892                       1259    245340    base_Department    TABLE       CREATE TABLE "base_Department" (
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
       public         postgres    false    2324    2325    2326    2327    2328    2329    2330    7                       0    0    TABLE "base_Department"    COMMENT     ,   COMMENT ON TABLE "base_Department" IS '

';
            public       postgres    false    1809                       0    0 "   COLUMN "base_Department"."LevelId"    COMMENT     8   COMMENT ON COLUMN "base_Department"."LevelId" IS 'ddd';
            public       postgres    false    1809                       1259    245338    base_Department_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Department_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Department_Id_seq";
       public       postgres    false    1809    7                       0    0    base_Department_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Department_Id_seq" OWNED BY "base_Department"."Id";
            public       postgres    false    1808                       0    0    base_Department_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Department_Id_seq"', 395, true);
            public       postgres    false    1808            �           1259    238237 
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
       public         postgres    false    2222    2223    2224    2225    2226    2227    2228    2229    2230    2231    2232    7                       0    0 %   COLUMN "base_Email"."IsHasAttachment"    COMMENT     p   COMMENT ON COLUMN "base_Email"."IsHasAttachment" IS 'Nếu có file đính kèm thì sẽ bật lên là true';
            public       postgres    false    1772            	           0    0 $   COLUMN "base_Email"."AttachmentType"    COMMENT     [   COMMENT ON COLUMN "base_Email"."AttachmentType" IS 'Sử dụng khi IsHasAttachment=true';
            public       postgres    false    1772            
           0    0 &   COLUMN "base_Email"."AttachmentResult"    COMMENT     y   COMMENT ON COLUMN "base_Email"."AttachmentResult" IS 'Sử dụng khi IsHasAttachment=true và phụ thuộc vào Type';
            public       postgres    false    1772                       0    0    COLUMN "base_Email"."Sender"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Sender" IS 'Thông tin người gủi dựa và GuestId';
            public       postgres    false    1772                       0    0    COLUMN "base_Email"."Status"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Status" IS '0: Outbox
1: Inbox
2: Sent
3: Draft
4: Trash';
            public       postgres    false    1772                       0    0     COLUMN "base_Email"."Importance"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Importance" IS 'Message Option
0: Normal
1: Importance
';
            public       postgres    false    1772                       0    0 !   COLUMN "base_Email"."Sensitivity"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Sensitivity" IS 'Message Option
0: Personal
1: Bussiness';
            public       postgres    false    1772                       0    0 '   COLUMN "base_Email"."IsRequestDelivery"    COMMENT     o   COMMENT ON COLUMN "base_Email"."IsRequestDelivery" IS 'Message Option
Request a delivery receipt for message';
            public       postgres    false    1772                       0    0 #   COLUMN "base_Email"."IsRequestRead"    COMMENT     g   COMMENT ON COLUMN "base_Email"."IsRequestRead" IS 'Message Option
Request a read receipt for message';
            public       postgres    false    1772                       0    0    COLUMN "base_Email"."IsMyFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsMyFlag" IS 'Custom Reminder Active Flag For Me';
            public       postgres    false    1772                       0    0    COLUMN "base_Email"."FlagTo"    COMMENT     >   COMMENT ON COLUMN "base_Email"."FlagTo" IS 'My Flag Options';
            public       postgres    false    1772                       0    0 #   COLUMN "base_Email"."FlagStartDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagStartDate" IS 'Active My Flag Date';
            public       postgres    false    1772                       0    0 !   COLUMN "base_Email"."FlagDueDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagDueDate" IS 'DeActive My Flag Date';
            public       postgres    false    1772                       0    0 %   COLUMN "base_Email"."IsAllowReminder"    COMMENT     L   COMMENT ON COLUMN "base_Email"."IsAllowReminder" IS 'Allow remind my flag';
            public       postgres    false    1772                       0    0    COLUMN "base_Email"."RemindOn"    COMMENT     X   COMMENT ON COLUMN "base_Email"."RemindOn" IS 'My Flag is going to remind on this date';
            public       postgres    false    1772                       0    0 #   COLUMN "base_Email"."MyRemindTimes"    COMMENT     H   COMMENT ON COLUMN "base_Email"."MyRemindTimes" IS 'The reminder times';
            public       postgres    false    1772                       0    0 $   COLUMN "base_Email"."IsRecipentFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsRecipentFlag" IS 'Custom Reminder For Recipent';
            public       postgres    false    1772                       0    0 $   COLUMN "base_Email"."RecipentFlagTo"    COMMENT     L   COMMENT ON COLUMN "base_Email"."RecipentFlagTo" IS 'Recipent Flag Options';
            public       postgres    false    1772                       0    0 -   COLUMN "base_Email"."IsAllowRecipentReminder"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."IsAllowRecipentReminder" IS 'Allow remind Recipent Flag';
            public       postgres    false    1772                       0    0 &   COLUMN "base_Email"."RecipentRemindOn"    COMMENT     f   COMMENT ON COLUMN "base_Email"."RecipentRemindOn" IS 'Recipent Flag is going to remind on this date';
            public       postgres    false    1772                       0    0 )   COLUMN "base_Email"."RecipentRemindTimes"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."RecipentRemindTimes" IS 'The Reminder Times of Recipent';
            public       postgres    false    1772            �           1259    238137    base_EmailAttachment    TABLE     p   CREATE TABLE "base_EmailAttachment" (
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
       public         postgres    false    2235    2236    2237    2238    2239    2240    2241    2242    2243    2244    2246    2247    2248    2249    2250    2251    2252    2253    2254    2255    2256    2257    2258    2259    2260    2261    2262    7                       0    0    COLUMN "base_Guest"."GuestNo"    COMMENT     <   COMMENT ON COLUMN "base_Guest"."GuestNo" IS 'YYMMDDHHMMSS';
            public       postgres    false    1777                       0    0     COLUMN "base_Guest"."PositionId"    COMMENT     >   COMMENT ON COLUMN "base_Guest"."PositionId" IS 'Chức vụ';
            public       postgres    false    1777                       0    0     COLUMN "base_Guest"."Department"    COMMENT     =   COMMENT ON COLUMN "base_Guest"."Department" IS 'Phòng ban';
            public       postgres    false    1777                        0    0    COLUMN "base_Guest"."Mark"    COMMENT     [   COMMENT ON COLUMN "base_Guest"."Mark" IS '-- E: Employee C: Company V: Vendor O: Contact';
            public       postgres    false    1777            !           0    0    COLUMN "base_Guest"."IsPrimary"    COMMENT     ^   COMMENT ON COLUMN "base_Guest"."IsPrimary" IS 'Áp dụng nếu đối tượng là contact';
            public       postgres    false    1777            "           0    0 '   COLUMN "base_Guest"."CommissionPercent"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."CommissionPercent" IS 'Apply khi Mark = E';
            public       postgres    false    1777            #           0    0 )   COLUMN "base_Guest"."TotalRewardRedeemed"    COMMENT     o   COMMENT ON COLUMN "base_Guest"."TotalRewardRedeemed" IS 'Total reward redeemed earned during tracking period';
            public       postgres    false    1777            $           0    0 2   COLUMN "base_Guest"."PurchaseDuringTrackingPeriod"    COMMENT     `   COMMENT ON COLUMN "base_Guest"."PurchaseDuringTrackingPeriod" IS '= Total(SaleOrderSubAmount)';
            public       postgres    false    1777            %           0    0 /   COLUMN "base_Guest"."RequirePurchaseNextReward"    COMMENT     �   COMMENT ON COLUMN "base_Guest"."RequirePurchaseNextReward" IS 'F = RewardAmount - PurchaseDuringTrackingPeriod Mod RewardAmount';
            public       postgres    false    1777            &           0    0 '   COLUMN "base_Guest"."IsBlockArriveLate"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBlockArriveLate" IS '-- Apply to TimeClock';
            public       postgres    false    1777            '           0    0 '   COLUMN "base_Guest"."IsDeductLunchTime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsDeductLunchTime" IS '-- Apply to TimeClock';
            public       postgres    false    1777            (           0    0 '   COLUMN "base_Guest"."IsBalanceOvertime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBalanceOvertime" IS '-- Apply to TimeClock';
            public       postgres    false    1777            )           0    0 !   COLUMN "base_Guest"."LateMinutes"    COMMENT     I   COMMENT ON COLUMN "base_Guest"."LateMinutes" IS '-- Apply to TimeClock';
            public       postgres    false    1777            *           0    0 $   COLUMN "base_Guest"."OvertimeOption"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."OvertimeOption" IS '-- Apply to TimeClock';
            public       postgres    false    1777            +           0    0 #   COLUMN "base_Guest"."OTLeastMinute"    COMMENT     K   COMMENT ON COLUMN "base_Guest"."OTLeastMinute" IS '-- Apply to TimeClock';
            public       postgres    false    1777            ,           0    0    COLUMN "base_Guest"."SaleRepId"    COMMENT     C   COMMENT ON COLUMN "base_Guest"."SaleRepId" IS 'Apply to customer';
            public       postgres    false    1777                       1259    245376    base_GuestAdditional    TABLE        CREATE TABLE "base_GuestAdditional" (
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
       public         postgres    false    2332    2333    2334    7            -           0    0 $   COLUMN "base_GuestAdditional"."Unit"    COMMENT     K   COMMENT ON COLUMN "base_GuestAdditional"."Unit" IS '0: Amount 1: Percent';
            public       postgres    false    1811            .           0    0 .   COLUMN "base_GuestAdditional"."IsTaxExemption"    COMMENT     N   COMMENT ON COLUMN "base_GuestAdditional"."IsTaxExemption" IS 'Miễn thuế';
            public       postgres    false    1811            /           0    0 .   COLUMN "base_GuestAdditional"."TaxExemptionNo"    COMMENT     a   COMMENT ON COLUMN "base_GuestAdditional"."TaxExemptionNo" IS 'Require if IsTaxExemption = true';
            public       postgres    false    1811                       1259    245374    base_GuestAdditional_Id_seq    SEQUENCE        CREATE SEQUENCE "base_GuestAdditional_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_GuestAdditional_Id_seq";
       public       postgres    false    1811    7            0           0    0    base_GuestAdditional_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_GuestAdditional_Id_seq" OWNED BY "base_GuestAdditional"."Id";
            public       postgres    false    1810            1           0    0    base_GuestAdditional_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestAdditional_Id_seq"', 107, true);
            public       postgres    false    1810            �           1259    244863    base_GuestAddress    TABLE     �  CREATE TABLE "base_GuestAddress" (
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
       public         postgres    false    2264    2265    2266    7            �           1259    244861    base_GuestAddress_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestAddress_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestAddress_Id_seq";
       public       postgres    false    7    1779            2           0    0    base_GuestAddress_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestAddress_Id_seq" OWNED BY "base_GuestAddress"."Id";
            public       postgres    false    1778            3           0    0    base_GuestAddress_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestAddress_Id_seq"', 234, true);
            public       postgres    false    1778            �           1259    238413    base_GuestFingerPrint    TABLE     3  CREATE TABLE "base_GuestFingerPrint" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "FingerIndex" integer NOT NULL,
    "HandFlag" boolean NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdaed" character varying(30),
    "FingerPrintImage" bytea
);
 +   DROP TABLE public."base_GuestFingerPrint";
       public         postgres    false    2233    7            �           1259    238411    base_GuestFingerPrint_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestFingerPrint_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestFingerPrint_Id_seq";
       public       postgres    false    1774    7            4           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestFingerPrint_Id_seq" OWNED BY "base_GuestFingerPrint"."Id";
            public       postgres    false    1773            5           0    0    base_GuestFingerPrint_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestFingerPrint_Id_seq"', 12, true);
            public       postgres    false    1773            �           1259    244873    base_GuestHiringHistory    TABLE     Q  CREATE TABLE "base_GuestHiringHistory" (
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
       public         postgres    false    2268    7            �           1259    244871    base_GuestHiringHistory_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestHiringHistory_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_GuestHiringHistory_Id_seq";
       public       postgres    false    1781    7            6           0    0    base_GuestHiringHistory_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_GuestHiringHistory_Id_seq" OWNED BY "base_GuestHiringHistory"."Id";
            public       postgres    false    1780            7           0    0    base_GuestHiringHistory_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_GuestHiringHistory_Id_seq"', 1, false);
            public       postgres    false    1780            �           1259    244884    base_GuestPayRoll    TABLE     �  CREATE TABLE "base_GuestPayRoll" (
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
       public         postgres    false    2270    2271    2272    7            �           1259    244882    base_GuestPayRoll_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestPayRoll_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestPayRoll_Id_seq";
       public       postgres    false    7    1783            8           0    0    base_GuestPayRoll_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestPayRoll_Id_seq" OWNED BY "base_GuestPayRoll"."Id";
            public       postgres    false    1782            9           0    0    base_GuestPayRoll_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_GuestPayRoll_Id_seq"', 1, false);
            public       postgres    false    1782            ;           1259    257325    base_GuestPaymentCard    TABLE     Z  CREATE TABLE "base_GuestPaymentCard" (
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
       public         postgres    false    2422    2423    7            :           1259    257323    base_GuestPaymentCard_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestPaymentCard_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestPaymentCard_Id_seq";
       public       postgres    false    1851    7            :           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestPaymentCard_Id_seq" OWNED BY "base_GuestPaymentCard"."Id";
            public       postgres    false    1850            ;           0    0    base_GuestPaymentCard_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestPaymentCard_Id_seq"', 14, true);
            public       postgres    false    1850            �           1259    244922    base_ResourcePhoto    TABLE        CREATE TABLE "base_ResourcePhoto" (
    "Id" integer NOT NULL,
    "ThumbnailPhoto" bytea,
    "ThumbnailPhotoFilename" character varying(60),
    "LargePhoto" bytea,
    "LargePhotoFilename" character varying(60),
    "SortId" smallint DEFAULT 0,
    "Resource" character varying(36)
);
 (   DROP TABLE public."base_ResourcePhoto";
       public         postgres    false    2274    7            �           1259    244920    base_GuestPhoto_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_GuestPhoto_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_GuestPhoto_Id_seq";
       public       postgres    false    7    1785            <           0    0    base_GuestPhoto_Id_seq    SEQUENCE OWNED BY     L   ALTER SEQUENCE "base_GuestPhoto_Id_seq" OWNED BY "base_ResourcePhoto"."Id";
            public       postgres    false    1784            =           0    0    base_GuestPhoto_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_GuestPhoto_Id_seq"', 237, true);
            public       postgres    false    1784            �           1259    244934    base_GuestProfile    TABLE     �  CREATE TABLE "base_GuestProfile" (
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
       public         postgres    false    2276    2277    7            �           1259    244932    base_GuestProfile_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestProfile_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestProfile_Id_seq";
       public       postgres    false    1787    7            >           0    0    base_GuestProfile_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestProfile_Id_seq" OWNED BY "base_GuestProfile"."Id";
            public       postgres    false    1786            ?           0    0    base_GuestProfile_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestProfile_Id_seq"', 155, true);
            public       postgres    false    1786            S           1259    268354    base_GuestReward    TABLE     �  CREATE TABLE "base_GuestReward" (
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
       public         postgres    false    2527    2528    2529    7            R           1259    268352    base_GuestReward_Id_seq    SEQUENCE     {   CREATE SEQUENCE "base_GuestReward_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."base_GuestReward_Id_seq";
       public       postgres    false    1875    7            @           0    0    base_GuestReward_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE "base_GuestReward_Id_seq" OWNED BY "base_GuestReward"."Id";
            public       postgres    false    1874            A           0    0    base_GuestReward_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestReward_Id_seq"', 3003, true);
            public       postgres    false    1874            2           1259    256013    base_GuestSchedule    TABLE     �   CREATE TABLE "base_GuestSchedule" (
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
       public       postgres    false    1777    7            B           0    0    base_Guest_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Guest_Id_seq" OWNED BY "base_Guest"."Id";
            public       postgres    false    1776            C           0    0    base_Guest_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"base_Guest_Id_seq"', 285, true);
            public       postgres    false    1776            �           1259    244997    base_MemberShip    TABLE       CREATE TABLE "base_MemberShip" (
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
       public         postgres    false    2283    2284    7            D           0    0 %   COLUMN "base_MemberShip"."MemberType"    COMMENT     f   COMMENT ON COLUMN "base_MemberShip"."MemberType" IS 'P = Platium, G = Gold, S = Silver, B = Bronze.';
            public       postgres    false    1791            E           0    0 !   COLUMN "base_MemberShip"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_MemberShip"."Status" IS '-1 = Pending
0 = DeActived
1 = Actived';
            public       postgres    false    1791            �           1259    244995    base_MemberShip_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_MemberShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_MemberShip_Id_seq";
       public       postgres    false    7    1791            F           0    0    base_MemberShip_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_MemberShip_Id_seq" OWNED BY "base_MemberShip"."Id";
            public       postgres    false    1790            G           0    0    base_MemberShip_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_MemberShip_Id_seq"', 1, false);
            public       postgres    false    1790            U           1259    268511    base_PricingChange    TABLE     �  CREATE TABLE "base_PricingChange" (
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
       public         postgres    false    2530    2532    2533    2534    7            T           1259    268509    base_PricingChange_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PricingChange_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PricingChange_Id_seq";
       public       postgres    false    1877    7            H           0    0    base_PricingChange_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PricingChange_Id_seq" OWNED BY "base_PricingChange"."Id";
            public       postgres    false    1876            I           0    0    base_PricingChange_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingChange_Id_seq"', 533, true);
            public       postgres    false    1876            Q           1259    268185    base_PricingManager    TABLE       CREATE TABLE "base_PricingManager" (
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
       public         postgres    false    2523    2524    2525    7            J           0    0 %   COLUMN "base_PricingManager"."Status"    COMMENT     u   COMMENT ON COLUMN "base_PricingManager"."Status" IS '- Pending
- Applied
- Restored

-> Get From PricingStatus Tag';
            public       postgres    false    1873            K           0    0 (   COLUMN "base_PricingManager"."BasePrice"    COMMENT     H   COMMENT ON COLUMN "base_PricingManager"."BasePrice" IS 'Cost or Price';
            public       postgres    false    1873            L           0    0 .   COLUMN "base_PricingManager"."CalculateMethod"    COMMENT     j   COMMENT ON COLUMN "base_PricingManager"."CalculateMethod" IS '+-*/
- Get from PricingAdjustmentType Tag';
            public       postgres    false    1873            M           0    0 )   COLUMN "base_PricingManager"."AmountUnit"    COMMENT     D   COMMENT ON COLUMN "base_PricingManager"."AmountUnit" IS '- % or $';
            public       postgres    false    1873            N           0    0 (   COLUMN "base_PricingManager"."ItemCount"    COMMENT     W   COMMENT ON COLUMN "base_PricingManager"."ItemCount" IS 'Tong so product duoc ap dung';
            public       postgres    false    1873            P           1259    268183    base_PricingManager_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_PricingManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_PricingManager_Id_seq";
       public       postgres    false    7    1873            O           0    0    base_PricingManager_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_PricingManager_Id_seq" OWNED BY "base_PricingManager"."Id";
            public       postgres    false    1872            P           0    0    base_PricingManager_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingManager_Id_seq"', 46, true);
            public       postgres    false    1872                       1259    245412    base_Product    TABLE     �  CREATE TABLE "base_Product" (
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
       public         postgres    false    2335    2337    2338    2339    2340    2341    2342    2343    2344    2345    2346    2347    2348    7            Q           0    0 &   COLUMN "base_Product"."QuantityOnHand"    COMMENT     b   COMMENT ON COLUMN "base_Product"."QuantityOnHand" IS 'Total From OnHandStore1 to OnHandStore 10';
            public       postgres    false    1813            R           0    0 '   COLUMN "base_Product"."QuantityOnOrder"    COMMENT     [   COMMENT ON COLUMN "base_Product"."QuantityOnOrder" IS 'Quantity on active purchase order';
            public       postgres    false    1813            S           0    0 $   COLUMN "base_Product"."RegularPrice"    COMMENT     I   COMMENT ON COLUMN "base_Product"."RegularPrice" IS 'Apply to Base Unit';
            public       postgres    false    1813            T           0    0    COLUMN "base_Product"."Price1"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price1" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1813            U           0    0    COLUMN "base_Product"."Price2"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price2" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1813            V           0    0    COLUMN "base_Product"."Price3"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price3" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1813            W           0    0    COLUMN "base_Product"."Price4"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price4" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1813            X           0    0 !   COLUMN "base_Product"."OrderCost"    COMMENT     F   COMMENT ON COLUMN "base_Product"."OrderCost" IS 'Apply to Base Unit';
            public       postgres    false    1813            Y           0    0 '   COLUMN "base_Product"."AverageUnitCost"    COMMENT     L   COMMENT ON COLUMN "base_Product"."AverageUnitCost" IS 'Apply to Base Unit';
            public       postgres    false    1813            Z           0    0    COLUMN "base_Product"."TaxCode"    COMMENT     D   COMMENT ON COLUMN "base_Product"."TaxCode" IS 'Apply to Base Unit';
            public       postgres    false    1813            [           0    0 %   COLUMN "base_Product"."MarginPercent"    COMMENT     q   COMMENT ON COLUMN "base_Product"."MarginPercent" IS 'Margin =100*(RegularPrice - AverageUnitCode)/RegularPrice';
            public       postgres    false    1813            \           0    0 %   COLUMN "base_Product"."MarkupPercent"    COMMENT     t   COMMENT ON COLUMN "base_Product"."MarkupPercent" IS 'Markup =100*(RegularPrice - AverageUnitCost)/AverageUnitCost';
            public       postgres    false    1813            ]           0    0 "   COLUMN "base_Product"."IsOpenItem"    COMMENT     Q   COMMENT ON COLUMN "base_Product"."IsOpenItem" IS 'Can change price during sale';
            public       postgres    false    1813            "           1259    255536    base_ProductStore    TABLE     �   CREATE TABLE "base_ProductStore" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL
);
 '   DROP TABLE public."base_ProductStore";
       public         postgres    false    2405    2407    7            !           1259    255534    base_ProductStore_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ProductStore_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ProductStore_Id_seq";
       public       postgres    false    1826    7            ^           0    0    base_ProductStore_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ProductStore_Id_seq" OWNED BY "base_ProductStore"."Id";
            public       postgres    false    1825            _           0    0    base_ProductStore_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_ProductStore_Id_seq"', 73, true);
            public       postgres    false    1825            c           1259    270252    base_ProductUOM    TABLE     O  CREATE TABLE "base_ProductUOM" (
    "Id" bigint NOT NULL,
    "ProductStoreId" bigint,
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
       public         postgres    false    2580    2581    2582    2583    2584    2585    2586    2587    2588    2589    2590    2591    2592    2593    7            `           0    0    TABLE "base_ProductUOM"    COMMENT     B   COMMENT ON TABLE "base_ProductUOM" IS 'Use when allow multi UOM';
            public       postgres    false    1891            b           1259    270250    base_ProductUOM_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_ProductUOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_ProductUOM_Id_seq";
       public       postgres    false    1891    7            a           0    0    base_ProductUOM_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_ProductUOM_Id_seq" OWNED BY "base_ProductUOM"."Id";
            public       postgres    false    1890            b           0    0    base_ProductUOM_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_ProductUOM_Id_seq"', 54, true);
            public       postgres    false    1890                       1259    245410    base_Product_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_Product_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_Product_Id_seq";
       public       postgres    false    1813    7            c           0    0    base_Product_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_Product_Id_seq" OWNED BY "base_Product"."Id";
            public       postgres    false    1812            d           0    0    base_Product_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Product_Id_seq"', 250202, true);
            public       postgres    false    1812                       1259    245169    base_Promotion    TABLE     �  CREATE TABLE "base_Promotion" (
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
       public         postgres    false    2317    2318    2319    2320    2321    2322    7            e           0    0     COLUMN "base_Promotion"."Status"    COMMENT     U   COMMENT ON COLUMN "base_Promotion"."Status" IS '0: Deactived
1: Actived
2: Pending';
            public       postgres    false    1807            f           0    0 (   COLUMN "base_Promotion"."AffectDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Promotion"."AffectDiscount" IS '0: All items
1: All items in category
2: All items from vendors
3: Custom';
            public       postgres    false    1807                       1259    245155    base_PromotionAffect    TABLE     j  CREATE TABLE "base_PromotionAffect" (
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
       public         postgres    false    2305    2306    2308    2309    2310    2311    2312    2313    2314    2315    7                       1259    245153    base_PromotionAffect_Id_seq    SEQUENCE        CREATE SEQUENCE "base_PromotionAffect_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_PromotionAffect_Id_seq";
       public       postgres    false    7    1805            g           0    0    base_PromotionAffect_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_PromotionAffect_Id_seq" OWNED BY "base_PromotionAffect"."Id";
            public       postgres    false    1804            h           0    0    base_PromotionAffect_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_PromotionAffect_Id_seq"', 609, true);
            public       postgres    false    1804                       1259    245023    base_PromotionSchedule    TABLE     �   CREATE TABLE "base_PromotionSchedule" (
    "Id" integer NOT NULL,
    "PromotionId" integer NOT NULL,
    "EndDate" timestamp without time zone,
    "StartDate" timestamp without time zone
);
 ,   DROP TABLE public."base_PromotionSchedule";
       public         postgres    false    7                        1259    245021    base_PromotionSchedule_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PromotionSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 6   DROP SEQUENCE public."base_PromotionSchedule_Id_seq";
       public       postgres    false    1793    7            i           0    0    base_PromotionSchedule_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE "base_PromotionSchedule_Id_seq" OWNED BY "base_PromotionSchedule"."Id";
            public       postgres    false    1792            j           0    0    base_PromotionSchedule_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_PromotionSchedule_Id_seq"', 55, true);
            public       postgres    false    1792                       1259    245167    base_Promotion_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Promotion_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Promotion_Id_seq";
       public       postgres    false    7    1807            k           0    0    base_Promotion_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Promotion_Id_seq" OWNED BY "base_Promotion"."Id";
            public       postgres    false    1806            l           0    0    base_Promotion_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_Promotion_Id_seq"', 55, true);
            public       postgres    false    1806            I           1259    266551    base_PurchaseOrder    TABLE     T  CREATE TABLE "base_PurchaseOrder" (
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
       public         postgres    false    2493    2495    2496    2497    2498    2499    2500    2501    2502    2503    2504    2505    2506    2507    2508    2509    2510    2511    7            m           0    0 (   COLUMN "base_PurchaseOrder"."QtyOrdered"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyOrdered" IS 'Order Quantity: In the purchase order item list. Enter the quantity being ordered for the item.
';
            public       postgres    false    1865            n           0    0 $   COLUMN "base_PurchaseOrder"."QtyDue"    COMMENT     q   COMMENT ON COLUMN "base_PurchaseOrder"."QtyDue" IS 'Due Quantity: The item quantity remaining to be received.
';
            public       postgres    false    1865            o           0    0 )   COLUMN "base_PurchaseOrder"."QtyReceived"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyReceived" IS 'Received Quantity: The ordered item quantity already received on receiving vouchers.
';
            public       postgres    false    1865            p           0    0 &   COLUMN "base_PurchaseOrder"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_PurchaseOrder"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100

';
            public       postgres    false    1865            G           1259    266530    base_PurchaseOrderDetail    TABLE     c  CREATE TABLE "base_PurchaseOrderDetail" (
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
       public         postgres    false    2483    2484    2485    2486    2488    2489    2490    2491    2492    7            q           0    0 *   COLUMN "base_PurchaseOrderDetail"."Amount"    COMMENT     S   COMMENT ON COLUMN "base_PurchaseOrderDetail"."Amount" IS 'Amount = Cost*Quantity';
            public       postgres    false    1863            F           1259    266528    base_PurchaseOrderDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_PurchaseOrderDetail_Id_seq";
       public       postgres    false    7    1863            r           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_PurchaseOrderDetail_Id_seq" OWNED BY "base_PurchaseOrderDetail"."Id";
            public       postgres    false    1862            s           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_PurchaseOrderDetail_Id_seq"', 137, true);
            public       postgres    false    1862            O           1259    267535    base_PurchaseOrderReceive    TABLE     o  CREATE TABLE "base_PurchaseOrderReceive" (
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
       public         postgres    false    2517    2518    2519    2521    7            t           0    0 *   COLUMN "base_PurchaseOrderReceive"."Price"    COMMENT     G   COMMENT ON COLUMN "base_PurchaseOrderReceive"."Price" IS 'Sale Price';
            public       postgres    false    1871            N           1259    267533     base_PurchaseOrderReceive_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderReceive_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_PurchaseOrderReceive_Id_seq";
       public       postgres    false    7    1871            u           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_PurchaseOrderReceive_Id_seq" OWNED BY "base_PurchaseOrderReceive"."Id";
            public       postgres    false    1870            v           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_PurchaseOrderReceive_Id_seq"', 113, true);
            public       postgres    false    1870            H           1259    266549    base_PurchaseOrder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PurchaseOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PurchaseOrder_Id_seq";
       public       postgres    false    1865    7            w           0    0    base_PurchaseOrder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PurchaseOrder_Id_seq" OWNED BY "base_PurchaseOrder"."Id";
            public       postgres    false    1864            x           0    0    base_PurchaseOrder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_PurchaseOrder_Id_seq"', 72, true);
            public       postgres    false    1864                       1259    245733    base_QuantityAdjustment    TABLE     �  CREATE TABLE "base_QuantityAdjustment" (
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
       public         postgres    false    2349    2351    2352    2353    2354    2355    7            y           0    0 1   COLUMN "base_QuantityAdjustment"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustment"."CostDifference" IS 'if(QtyChanged) AverageUnitCost*(NewQty - OldQty) elseif(CostChanged) Quantity*(NewCost - OldCost)
';
            public       postgres    false    1815            z           0    0 ,   COLUMN "base_QuantityAdjustment"."ItemCount"    COMMENT     a   COMMENT ON COLUMN "base_QuantityAdjustment"."ItemCount" IS 'Đếm số lượng sản phẩm ';
            public       postgres    false    1815            {           0    0 -   COLUMN "base_QuantityAdjustment"."LoggedTime"    COMMENT     {   COMMENT ON COLUMN "base_QuantityAdjustment"."LoggedTime" IS 'Thời gian thực hiên ghi nhận: YYYY/MM/DD HH:MM:SS TT';
            public       postgres    false    1815                       1259    245745    base_QuantityAdjustmentItem    TABLE     �  CREATE TABLE "base_QuantityAdjustmentItem" (
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
       public         postgres    false    2356    2358    7            |           0    0 5   COLUMN "base_QuantityAdjustmentItem"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."CostDifference" IS '-- AverageUnitCost*OldQuantity - AverageUnitCost*NewQuantity';
            public       postgres    false    1817            }           0    0 8   COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff"    COMMENT     n   COMMENT ON COLUMN "base_QuantityAdjustmentItem"."AdjustmentQtyDiff" IS 'AdjustmentNewQty - AdjustmentOldQty';
            public       postgres    false    1817                       1259    245743 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_QuantityAdjustmentItem_Id_seq";
       public       postgres    false    7    1817            ~           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_QuantityAdjustmentItem_Id_seq" OWNED BY "base_QuantityAdjustmentItem"."Id";
            public       postgres    false    1816                       0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_QuantityAdjustmentItem_Id_seq"', 2, true);
            public       postgres    false    1816                       1259    245731    base_QuantityAdjustment_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_QuantityAdjustment_Id_seq";
       public       postgres    false    1815    7            �           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_QuantityAdjustment_Id_seq" OWNED BY "base_QuantityAdjustment"."Id";
            public       postgres    false    1814            �           0    0    base_QuantityAdjustment_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_QuantityAdjustment_Id_seq"', 2, true);
            public       postgres    false    1814            6           1259    256178    base_ResourceAccount    TABLE       CREATE TABLE "base_ResourceAccount" (
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
       public         postgres    false    2417    2418    2419    7            5           1259    256176    base_ResourceAccount_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourceAccount_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourceAccount_Id_seq";
       public       postgres    false    1846    7            �           0    0    base_ResourceAccount_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourceAccount_Id_seq" OWNED BY "base_ResourceAccount"."Id";
            public       postgres    false    1845            �           0    0    base_ResourceAccount_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceAccount_Id_seq"', 27, true);
            public       postgres    false    1845                       1259    246083    base_ResourceNote    TABLE     �   CREATE TABLE "base_ResourceNote" (
    "Id" bigint NOT NULL,
    "Note" character varying(300),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "Color" character(9),
    "Resource" character varying(36) NOT NULL
);
 '   DROP TABLE public."base_ResourceNote";
       public         postgres    false    2374    7                       1259    246081    base_ResourceNote_id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ResourceNote_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ResourceNote_id_seq";
       public       postgres    false    7    1823            �           0    0    base_ResourceNote_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ResourceNote_id_seq" OWNED BY "base_ResourceNote"."Id";
            public       postgres    false    1822            �           0    0    base_ResourceNote_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ResourceNote_id_seq"', 687, true);
            public       postgres    false    1822            _           1259    270150    base_ResourcePayment    TABLE     �  CREATE TABLE "base_ResourcePayment" (
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
       public         postgres    false    2555    2556    2558    2559    2560    2561    2562    2563    2564    2565    2566    7            �           0    0 $   COLUMN "base_ResourcePayment"."Mark"    COMMENT     <   COMMENT ON COLUMN "base_ResourcePayment"."Mark" IS 'SO/PO';
            public       postgres    false    1887            ]           1259    270072    base_ResourcePaymentDetail    TABLE       CREATE TABLE "base_ResourcePaymentDetail" (
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
       public         postgres    false    2552    2553    2554    7            �           0    0 1   COLUMN "base_ResourcePaymentDetail"."PaymentType"    COMMENT     W   COMMENT ON COLUMN "base_ResourcePaymentDetail"."PaymentType" IS 'P:Payment
C:Correct';
            public       postgres    false    1885            �           0    0 ,   COLUMN "base_ResourcePaymentDetail"."Reason"    COMMENT     ^   COMMENT ON COLUMN "base_ResourcePaymentDetail"."Reason" IS 'Apply to Correct payment action';
            public       postgres    false    1885            \           1259    270070 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_ResourcePaymentDetail_Id_seq";
       public       postgres    false    7    1885            �           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_ResourcePaymentDetail_Id_seq" OWNED BY "base_ResourcePaymentDetail"."Id";
            public       postgres    false    1884            �           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentDetail_Id_seq"', 356, true);
            public       postgres    false    1884            k           1259    272122    base_ResourcePaymentProduct    TABLE       CREATE TABLE "base_ResourcePaymentProduct" (
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
       public         postgres    false    2608    2609    2610    7            j           1259    272120 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_ResourcePaymentProduct_Id_seq";
       public       postgres    false    1899    7            �           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE "base_ResourcePaymentProduct_Id_seq" OWNED BY "base_ResourcePaymentProduct"."Id";
            public       postgres    false    1898            �           0    0 "   base_ResourcePaymentProduct_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentProduct_Id_seq"', 81, true);
            public       postgres    false    1898            ^           1259    270148    base_ResourcePayment_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourcePayment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourcePayment_Id_seq";
       public       postgres    false    7    1887            �           0    0    base_ResourcePayment_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourcePayment_Id_seq" OWNED BY "base_ResourcePayment"."Id";
            public       postgres    false    1886            �           0    0    base_ResourcePayment_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_ResourcePayment_Id_seq"', 277, true);
            public       postgres    false    1886            a           1259    270193    base_ResourceReturn    TABLE     B  CREATE TABLE "base_ResourceReturn" (
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
       public         postgres    false    2567    2568    2570    2571    2572    2573    2574    2575    2576    2577    2578    7            �           0    0 #   COLUMN "base_ResourceReturn"."Mark"    COMMENT     ;   COMMENT ON COLUMN "base_ResourceReturn"."Mark" IS 'SO/PO';
            public       postgres    false    1889            i           1259    272099    base_ResourceReturnDetail    TABLE     �  CREATE TABLE "base_ResourceReturnDetail" (
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
       public         postgres    false    2601    2603    2604    2605    2606    7            h           1259    272097     base_ResourceReturnDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourceReturnDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_ResourceReturnDetail_Id_seq";
       public       postgres    false    7    1897            �           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_ResourceReturnDetail_Id_seq" OWNED BY "base_ResourceReturnDetail"."Id";
            public       postgres    false    1896            �           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_ResourceReturnDetail_Id_seq"', 88, true);
            public       postgres    false    1896            `           1259    270191    base_ResourceReturn_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_ResourceReturn_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_ResourceReturn_Id_seq";
       public       postgres    false    1889    7            �           0    0    base_ResourceReturn_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_ResourceReturn_Id_seq" OWNED BY "base_ResourceReturn"."Id";
            public       postgres    false    1888            �           0    0    base_ResourceReturn_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceReturn_Id_seq"', 216, true);
            public       postgres    false    1888            M           1259    266843    base_RewardManager    TABLE     �  CREATE TABLE "base_RewardManager" (
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
       public         postgres    false    2514    2515    2516    7            L           1259    266841    base_RewardManager_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_RewardManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_RewardManager_Id_seq";
       public       postgres    false    1869    7            �           0    0    base_RewardManager_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_RewardManager_Id_seq" OWNED BY "base_RewardManager"."Id";
            public       postgres    false    1868            �           0    0    base_RewardManager_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_RewardManager_Id_seq"', 2, true);
            public       postgres    false    1868            K           1259    266606    base_SaleCommission    TABLE     �  CREATE TABLE "base_SaleCommission" (
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
            public       postgres    false    1867            J           1259    266604    base_SaleCommission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_SaleCommission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_SaleCommission_Id_seq";
       public       postgres    false    1867    7            �           0    0    base_SaleCommission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_SaleCommission_Id_seq" OWNED BY "base_SaleCommission"."Id";
            public       postgres    false    1866            �           0    0    base_SaleCommission_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_SaleCommission_Id_seq"', 753, true);
            public       postgres    false    1866            ?           1259    266093    base_SaleOrder    TABLE     P
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
    "RewardAmount" numeric(12,2) DEFAULT 0 NOT NULL
);
 $   DROP TABLE public."base_SaleOrder";
       public         postgres    false    2437    2439    2440    2441    2442    2443    2444    2445    2446    2447    2448    2449    2450    2451    2452    2453    2454    2455    2456    2457    2458    2459    2460    2461    2462    2463    2464    2465    2466    2467    2468    7            �           0    0 &   COLUMN "base_SaleOrder"."RewardAmount"    COMMENT     c   COMMENT ON COLUMN "base_SaleOrder"."RewardAmount" IS 'Tong so tien can thanh toan sau khi reward';
            public       postgres    false    1855            =           1259    266084    base_SaleOrderDetail    TABLE     9  CREATE TABLE "base_SaleOrderDetail" (
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
       public         postgres    false    2424    2425    2426    2427    2428    2429    2430    2431    2432    2434    2435    2436    7            �           0    0 (   COLUMN "base_SaleOrderDetail"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_SaleOrderDetail"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100';
            public       postgres    false    1853            �           0    0 .   COLUMN "base_SaleOrderDetail"."SerialTracking"    COMMENT     Z   COMMENT ON COLUMN "base_SaleOrderDetail"."SerialTracking" IS 'Apply to Serial Tracking ';
            public       postgres    false    1853            �           0    0 .   COLUMN "base_SaleOrderDetail"."BalanceShipped"    COMMENT     s   COMMENT ON COLUMN "base_SaleOrderDetail"."BalanceShipped" IS 'Số lượng sản phẩm được vận chuyển';
            public       postgres    false    1853            <           1259    266082    base_SaleOrderDetail_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleOrderDetail_Id_seq";
       public       postgres    false    1853    7            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleOrderDetail_Id_seq" OWNED BY "base_SaleOrderDetail"."Id";
            public       postgres    false    1852            �           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderDetail_Id_seq"', 588, true);
            public       postgres    false    1852            C           1259    266236    base_SaleOrderInvoice    TABLE     }  CREATE TABLE "base_SaleOrderInvoice" (
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
       public         postgres    false    2473    2474    2475    2476    2477    2478    2479    2480    7            B           1259    266234    base_SaleOrderInvoice_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderInvoice_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_SaleOrderInvoice_Id_seq";
       public       postgres    false    7    1859            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_SaleOrderInvoice_Id_seq" OWNED BY "base_SaleOrderInvoice"."Id";
            public       postgres    false    1858            �           0    0    base_SaleOrderInvoice_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderInvoice_Id_seq"', 1, false);
            public       postgres    false    1858            A           1259    266180    base_SaleOrderShip    TABLE     �  CREATE TABLE "base_SaleOrderShip" (
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
       public         postgres    false    2470    2471    7            E           1259    266357    base_SaleOrderShipDetail    TABLE     2  CREATE TABLE "base_SaleOrderShipDetail" (
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
       public         postgres    false    2482    7            D           1259    266355    base_SaleOrderShipDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderShipDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_SaleOrderShipDetail_Id_seq";
       public       postgres    false    7    1861            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_SaleOrderShipDetail_Id_seq" OWNED BY "base_SaleOrderShipDetail"."Id";
            public       postgres    false    1860            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_SaleOrderShipDetail_Id_seq"', 423, true);
            public       postgres    false    1860            @           1259    266178    base_SaleOrderShip_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_SaleOrderShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_SaleOrderShip_Id_seq";
       public       postgres    false    1857    7            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_SaleOrderShip_Id_seq" OWNED BY "base_SaleOrderShip"."Id";
            public       postgres    false    1856            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_SaleOrderShip_Id_seq"', 335, true);
            public       postgres    false    1856            >           1259    266091    base_SaleOrder_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_SaleOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_SaleOrder_Id_seq";
       public       postgres    false    1855    7            �           0    0    base_SaleOrder_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_SaleOrder_Id_seq" OWNED BY "base_SaleOrder"."Id";
            public       postgres    false    1854            �           0    0    base_SaleOrder_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_SaleOrder_Id_seq"', 378, true);
            public       postgres    false    1854                       1259    245103    base_SaleTaxLocation    TABLE     n  CREATE TABLE "base_SaleTaxLocation" (
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
       public         postgres    false    2288    2290    2291    2292    2293    2294    7            �           0    0 )   COLUMN "base_SaleTaxLocation"."SortIndex"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocation"."SortIndex" IS 'ParentId ==0 -> Id"-"DateTime
ParnetId !=0 -> ParentId"-"DateTime
Order By Asc';
            public       postgres    false    1797                       1259    245084    base_SaleTaxLocationOption    TABLE     (  CREATE TABLE "base_SaleTaxLocationOption" (
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
       public         postgres    false    2287    7            �           0    0 .   COLUMN "base_SaleTaxLocationOption"."ParentId"    COMMENT     h   COMMENT ON COLUMN "base_SaleTaxLocationOption"."ParentId" IS 'Apply For Multi-rate has multi tax code';
            public       postgres    false    1795            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."TaxRate"    COMMENT     k   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxRate" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1795            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxComponent"    COMMENT     Y   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxComponent" IS 'Apply For Multi-rate';
            public       postgres    false    1795            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."TaxAgency"    COMMENT     m   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxAgency" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1795            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxCondition"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxCondition" IS 'Apply For Price-Depedent: Collect this tax on an item if the unit price or shiping is more than';
            public       postgres    false    1795            �           0    0 7   COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver" IS 'Apply For Price-Depedent: Apply sale tax only to the amount over the pricing unit or shipping threshold';
            public       postgres    false    1795            �           0    0 C   COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to a specific item price range';
            public       postgres    false    1795            �           0    0 A   COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to the mount of an item''s price within this range';
            public       postgres    false    1795            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."PriceFrom"    COMMENT     V   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceFrom" IS 'Apply For Multi-rate';
            public       postgres    false    1795            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."PriceTo"    COMMENT     T   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceTo" IS 'Apply For Multi-rate';
            public       postgres    false    1795                       1259    245082 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleTaxLocationOption_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_SaleTaxLocationOption_Id_seq";
       public       postgres    false    7    1795            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_SaleTaxLocationOption_Id_seq" OWNED BY "base_SaleTaxLocationOption"."Id";
            public       postgres    false    1794            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_SaleTaxLocationOption_Id_seq"', 117, true);
            public       postgres    false    1794                       1259    245101    base_SaleTaxLocation_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleTaxLocation_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleTaxLocation_Id_seq";
       public       postgres    false    7    1797            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleTaxLocation_Id_seq" OWNED BY "base_SaleTaxLocation"."Id";
            public       postgres    false    1796            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleTaxLocation_Id_seq"', 365, true);
            public       postgres    false    1796            $           1259    255675 
   base_Store    TABLE     �   CREATE TABLE "base_Store" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(30),
    "Street" character varying(200),
    "City" character varying(200)
);
     DROP TABLE public."base_Store";
       public         postgres    false    7            #           1259    255673    base_Store_Id_seq    SEQUENCE     u   CREATE SEQUENCE "base_Store_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."base_Store_Id_seq";
       public       postgres    false    1828    7            �           0    0    base_Store_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Store_Id_seq" OWNED BY "base_Store"."Id";
            public       postgres    false    1827            �           0    0    base_Store_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"base_Store_Id_seq"', 46, true);
            public       postgres    false    1827            Y           1259    269925    base_TransferStock    TABLE     �  CREATE TABLE "base_TransferStock" (
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
       public         postgres    false    2538    2539    2540    2541    2542    2543    2544    2545    2546    7            [           1259    269941    base_TransferStockDetail    TABLE     |  CREATE TABLE "base_TransferStockDetail" (
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
       public         postgres    false    2548    2549    2550    7            Z           1259    269939    base_TransferStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_TransferStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_TransferStockDetail_Id_seq";
       public       postgres    false    7    1883            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_TransferStockDetail_Id_seq" OWNED BY "base_TransferStockDetail"."Id";
            public       postgres    false    1882            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_TransferStockDetail_Id_seq"', 42, true);
            public       postgres    false    1882            X           1259    269923    base_TransferStock_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_TransferStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_TransferStock_Id_seq";
       public       postgres    false    1881    7            �           0    0    base_TransferStock_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_TransferStock_Id_seq" OWNED BY "base_TransferStock"."Id";
            public       postgres    false    1880            �           0    0    base_TransferStock_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_TransferStock_Id_seq"', 29, true);
            public       postgres    false    1880                       1259    245147    base_UOM    TABLE     �  CREATE TABLE "base_UOM" (
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
       public         postgres    false    2302    2303    2304    7            
           1259    245145    base_UOM_Id_seq    SEQUENCE     s   CREATE SEQUENCE "base_UOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."base_UOM_Id_seq";
       public       postgres    false    7    1803            �           0    0    base_UOM_Id_seq    SEQUENCE OWNED BY     ;   ALTER SEQUENCE "base_UOM_Id_seq" OWNED BY "base_UOM"."Id";
            public       postgres    false    1802            �           0    0    base_UOM_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"base_UOM_Id_seq"', 115, true);
            public       postgres    false    1802            	           1259    245131    base_UserLog    TABLE     #  CREATE TABLE "base_UserLog" (
    "Id" bigint NOT NULL,
    "IpSource" character varying(17),
    "ConnectedOn" timestamp without time zone DEFAULT now() NOT NULL,
    "DisConnectedOn" timestamp without time zone,
    "ResourceAccessed" character varying(36),
    "IsDisconected" boolean
);
 "   DROP TABLE public."base_UserLog";
       public         postgres    false    2300    7            �           1259    244282    base_UserLogDetail    TABLE     �   CREATE TABLE "base_UserLogDetail" (
    "Id" uuid NOT NULL,
    "UserLogId" bigint,
    "AccessedTime" timestamp without time zone,
    "ModuleName" character varying(30),
    "ActionDescription" character varying(200)
);
 (   DROP TABLE public."base_UserLogDetail";
       public         postgres    false    7                       1259    245129    base_UserLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_UserLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_UserLog_Id_seq";
       public       postgres    false    1801    7            �           0    0    base_UserLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_UserLog_Id_seq" OWNED BY "base_UserLog"."Id";
            public       postgres    false    1800            �           0    0    base_UserLog_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_UserLog_Id_seq"', 2285, true);
            public       postgres    false    1800            8           1259    256244    base_UserRight    TABLE     �   CREATE TABLE "base_UserRight" (
    "Id" integer NOT NULL,
    "Code" character varying(5) NOT NULL,
    "Name" character varying(200)
);
 $   DROP TABLE public."base_UserRight";
       public         postgres    false    7            7           1259    256242    base_UserRight_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_UserRight_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_UserRight_Id_seq";
       public       postgres    false    1848    7            �           0    0    base_UserRight_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_UserRight_Id_seq" OWNED BY "base_UserRight"."Id";
            public       postgres    false    1847            �           0    0    base_UserRight_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_UserRight_Id_seq"', 187, true);
            public       postgres    false    1847            V           1259    269643    base_VendorProduct    TABLE       CREATE TABLE "base_VendorProduct" (
    "Id" integer NOT NULL,
    "ProductId" bigint NOT NULL,
    "VendorId" bigint NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "VendorResource" character varying(36) NOT NULL
);
 (   DROP TABLE public."base_VendorProduct";
       public         postgres    false    2536    7            W           1259    269646    base_VendorProduct_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VendorProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VendorProduct_Id_seq";
       public       postgres    false    1878    7            �           0    0    base_VendorProduct_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VendorProduct_Id_seq" OWNED BY "base_VendorProduct"."Id";
            public       postgres    false    1879            �           0    0    base_VendorProduct_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VendorProduct_Id_seq"', 40, true);
            public       postgres    false    1879                       1259    245115    base_VirtualFolder    TABLE     �  CREATE TABLE "base_VirtualFolder" (
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
       public         postgres    false    2296    2297    2298    7                       1259    245113    base_VirtualFolder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VirtualFolder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VirtualFolder_Id_seq";
       public       postgres    false    1799    7            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VirtualFolder_Id_seq" OWNED BY "base_VirtualFolder"."Id";
            public       postgres    false    1798            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VirtualFolder_Id_seq"', 66, true);
            public       postgres    false    1798            l           1259    282433 	   rpt_Group    TABLE     �   CREATE TABLE "rpt_Group" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(200),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(30)
);
    DROP TABLE public."rpt_Group";
       public         postgres    false    7            m           1259    282436    rpt_Group_Id_seq    SEQUENCE     t   CREATE SEQUENCE "rpt_Group_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."rpt_Group_Id_seq";
       public       postgres    false    1900    7            �           0    0    rpt_Group_Id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE "rpt_Group_Id_seq" OWNED BY "rpt_Group"."Id";
            public       postgres    false    1901            �           0    0    rpt_Group_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"rpt_Group_Id_seq"', 1, false);
            public       postgres    false    1901            o           1259    282444 
   rpt_Report    TABLE     �  CREATE TABLE "rpt_Report" (
    "Id" integer NOT NULL,
    "GroupId" integer DEFAULT 0 NOT NULL,
    "ParentId" integer DEFAULT 0 NOT NULL,
    "Code" character varying(4) NOT NULL,
    "Name" character varying(200),
    "FormatFile" character varying(50),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(35),
    "IsShow" boolean DEFAULT false NOT NULL,
    "ProcessName" character varying(50),
    "SamplePicture" bytea
);
     DROP TABLE public."rpt_Report";
       public         postgres    false    2613    2614    2615    7            n           1259    282442    rpt_Report_Id_seq    SEQUENCE     u   CREATE SEQUENCE "rpt_Report_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."rpt_Report_Id_seq";
       public       postgres    false    1903    7            �           0    0    rpt_Report_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "rpt_Report_Id_seq" OWNED BY "rpt_Report"."Id";
            public       postgres    false    1902            �           0    0    rpt_Report_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"rpt_Report_Id_seq"', 1, false);
            public       postgres    false    1902            &           1259    255696    tims_Holiday    TABLE     #  CREATE TABLE "tims_Holiday" (
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
       public         postgres    false    7            '           1259    255705    tims_HolidayHistory    TABLE     {   CREATE TABLE "tims_HolidayHistory" (
    "Date" timestamp without time zone NOT NULL,
    "Name" character varying(200)
);
 )   DROP TABLE public."tims_HolidayHistory";
       public         postgres    false    7            %           1259    255694    tims_Holiday_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_Holiday_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_Holiday_Id_seq";
       public       postgres    false    1830    7            �           0    0    tims_Holiday_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_Holiday_Id_seq" OWNED BY "tims_Holiday"."Id";
            public       postgres    false    1829            �           0    0    tims_Holiday_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_Holiday_Id_seq"', 10, true);
            public       postgres    false    1829            /           1259    255849    tims_TimeLog    TABLE     <  CREATE TABLE "tims_TimeLog" (
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
       public         postgres    false    7            1           1259    255865    tims_TimeLogPermission    TABLE     u   CREATE TABLE "tims_TimeLogPermission" (
    "TimeLogId" integer NOT NULL,
    "WorkPermissionId" integer NOT NULL
);
 ,   DROP TABLE public."tims_TimeLogPermission";
       public         postgres    false    7            0           1259    255863 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE     �   CREATE SEQUENCE "tims_TimeLogPermission_TimeLogId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 =   DROP SEQUENCE public."tims_TimeLogPermission_TimeLogId_seq";
       public       postgres    false    1841    7            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE "tims_TimeLogPermission_TimeLogId_seq" OWNED BY "tims_TimeLogPermission"."TimeLogId";
            public       postgres    false    1840            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('"tims_TimeLogPermission_TimeLogId_seq"', 1, false);
            public       postgres    false    1840            .           1259    255847    tims_TimeLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_TimeLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_TimeLog_Id_seq";
       public       postgres    false    7    1839            �           0    0    tims_TimeLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_TimeLog_Id_seq" OWNED BY "tims_TimeLog"."Id";
            public       postgres    false    1838            �           0    0    tims_TimeLog_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_TimeLog_Id_seq"', 10, true);
            public       postgres    false    1838            -           1259    255795    tims_WorkPermission    TABLE     Z  CREATE TABLE "tims_WorkPermission" (
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
       public         postgres    false    7            ,           1259    255793    tims_WorkPermission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "tims_WorkPermission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."tims_WorkPermission_Id_seq";
       public       postgres    false    7    1837            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "tims_WorkPermission_Id_seq" OWNED BY "tims_WorkPermission"."Id";
            public       postgres    false    1836            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"tims_WorkPermission_Id_seq"', 6, true);
            public       postgres    false    1836            )           1259    255738    tims_WorkSchedule    TABLE     �  CREATE TABLE "tims_WorkSchedule" (
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
       public         postgres    false    7            (           1259    255736    tims_WorkSchedule_Id_seq    SEQUENCE     |   CREATE SEQUENCE "tims_WorkSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."tims_WorkSchedule_Id_seq";
       public       postgres    false    1833    7            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "tims_WorkSchedule_Id_seq" OWNED BY "tims_WorkSchedule"."Id";
            public       postgres    false    1832            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"tims_WorkSchedule_Id_seq"', 28, true);
            public       postgres    false    1832            +           1259    255781    tims_WorkWeek    TABLE     �  CREATE TABLE "tims_WorkWeek" (
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
       public         postgres    false    7            *           1259    255779    tims_WorkWeek_Id_seq    SEQUENCE     x   CREATE SEQUENCE "tims_WorkWeek_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."tims_WorkWeek_Id_seq";
       public       postgres    false    7    1835            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "tims_WorkWeek_Id_seq" OWNED BY "tims_WorkWeek"."Id";
            public       postgres    false    1834            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"tims_WorkWeek_Id_seq"', 187, true);
            public       postgres    false    1834            �           2604    235589    jexid    DEFAULT     g   ALTER TABLE pga_exception ALTER COLUMN jexid SET DEFAULT nextval('pga_exception_jexid_seq'::regclass);
 C   ALTER TABLE pgagent.pga_exception ALTER COLUMN jexid DROP DEFAULT;
       pgagent       postgres    false    1757    1756            �           2604    235590    jobid    DEFAULT     [   ALTER TABLE pga_job ALTER COLUMN jobid SET DEFAULT nextval('pga_job_jobid_seq'::regclass);
 =   ALTER TABLE pgagent.pga_job ALTER COLUMN jobid DROP DEFAULT;
       pgagent       postgres    false    1759    1758            �           2604    235591    jclid    DEFAULT     e   ALTER TABLE pga_jobclass ALTER COLUMN jclid SET DEFAULT nextval('pga_jobclass_jclid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_jobclass ALTER COLUMN jclid DROP DEFAULT;
       pgagent       postgres    false    1762    1761            �           2604    235592    jlgid    DEFAULT     a   ALTER TABLE pga_joblog ALTER COLUMN jlgid SET DEFAULT nextval('pga_joblog_jlgid_seq'::regclass);
 @   ALTER TABLE pgagent.pga_joblog ALTER COLUMN jlgid DROP DEFAULT;
       pgagent       postgres    false    1764    1763            �           2604    235593    jstid    DEFAULT     c   ALTER TABLE pga_jobstep ALTER COLUMN jstid SET DEFAULT nextval('pga_jobstep_jstid_seq'::regclass);
 A   ALTER TABLE pgagent.pga_jobstep ALTER COLUMN jstid DROP DEFAULT;
       pgagent       postgres    false    1766    1765            �           2604    235594    jslid    DEFAULT     i   ALTER TABLE pga_jobsteplog ALTER COLUMN jslid SET DEFAULT nextval('pga_jobsteplog_jslid_seq'::regclass);
 D   ALTER TABLE pgagent.pga_jobsteplog ALTER COLUMN jslid DROP DEFAULT;
       pgagent       postgres    false    1768    1767            �           2604    235595    jscid    DEFAULT     e   ALTER TABLE pga_schedule ALTER COLUMN jscid SET DEFAULT nextval('pga_schedule_jscid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_schedule ALTER COLUMN jscid DROP DEFAULT;
       pgagent       postgres    false    1770    1769            �           2604    244949    Id    DEFAULT     k   ALTER TABLE "base_Attachment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Attachment_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Attachment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1788    1789    1789            o	           2604    256171    Id    DEFAULT     i   ALTER TABLE "base_Authorize" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Authorize_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Authorize" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1844    1843    1844            U	           2604    257304    Id    DEFAULT     q   ALTER TABLE "base_Configuration" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Configuration_Id_seq"'::regclass);
 H   ALTER TABLE public."base_Configuration" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1849    1824            7	           2604    245757    Id    DEFAULT     s   ALTER TABLE "base_CostAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustment_Id_seq"'::regclass);
 I   ALTER TABLE public."base_CostAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1818    1819    1819            @	           2604    245769    Id    DEFAULT     {   ALTER TABLE "base_CostAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustmentItem_Id_seq"'::regclass);
 M   ALTER TABLE public."base_CostAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1820    1821    1821            "
           2604    271741    Id    DEFAULT     k   ALTER TABLE "base_CountStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStock_Id_seq"'::regclass);
 E   ALTER TABLE public."base_CountStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1893    1892    1893            %
           2604    271748    Id    DEFAULT     w   ALTER TABLE "base_CountStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStockDetail_Id_seq"'::regclass);
 K   ALTER TABLE public."base_CountStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1895    1894    1895            	           2604    245343    Id    DEFAULT     k   ALTER TABLE "base_Department" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Department_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Department" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1809    1808    1809            �           2604    244820    Id    DEFAULT     a   ALTER TABLE "base_Guest" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Guest_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Guest" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1777    1776    1777            	           2604    245379    Id    DEFAULT     u   ALTER TABLE "base_GuestAdditional" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAdditional_Id_seq"'::regclass);
 J   ALTER TABLE public."base_GuestAdditional" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1810    1811    1811            �           2604    244866    Id    DEFAULT     o   ALTER TABLE "base_GuestAddress" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAddress_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestAddress" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1779    1778    1779            �           2604    238416    Id    DEFAULT     w   ALTER TABLE "base_GuestFingerPrint" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestFingerPrint_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestFingerPrint" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1773    1774    1774            �           2604    244876    Id    DEFAULT     {   ALTER TABLE "base_GuestHiringHistory" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestHiringHistory_Id_seq"'::regclass);
 M   ALTER TABLE public."base_GuestHiringHistory" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1781    1780    1781            �           2604    244887    Id    DEFAULT     o   ALTER TABLE "base_GuestPayRoll" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPayRoll_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestPayRoll" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1782    1783    1783            u	           2604    257328    Id    DEFAULT     w   ALTER TABLE "base_GuestPaymentCard" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPaymentCard_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestPaymentCard" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1850    1851    1851            �           2604    244937    Id    DEFAULT     o   ALTER TABLE "base_GuestProfile" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestProfile_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestProfile" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1787    1786    1787            �	           2604    268357    Id    DEFAULT     m   ALTER TABLE "base_GuestReward" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestReward_Id_seq"'::regclass);
 F   ALTER TABLE public."base_GuestReward" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1875    1874    1875            �           2604    245000    Id    DEFAULT     k   ALTER TABLE "base_MemberShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_MemberShip_Id_seq"'::regclass);
 E   ALTER TABLE public."base_MemberShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1791    1790    1791            �	           2604    268514    Id    DEFAULT     q   ALTER TABLE "base_PricingChange" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingChange_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PricingChange" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1877    1876    1877            �	           2604    268188    Id    DEFAULT     s   ALTER TABLE "base_PricingManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingManager_Id_seq"'::regclass);
 I   ALTER TABLE public."base_PricingManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1873    1872    1873             	           2604    245415    Id    DEFAULT     e   ALTER TABLE "base_Product" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Product_Id_seq"'::regclass);
 B   ALTER TABLE public."base_Product" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1812    1813    1813            f	           2604    255539    Id    DEFAULT     o   ALTER TABLE "base_ProductStore" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductStore_Id_seq"'::regclass);
 G   ALTER TABLE public."base_ProductStore" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1825    1826    1826            
           2604    270255    Id    DEFAULT     k   ALTER TABLE "base_ProductUOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductUOM_Id_seq"'::regclass);
 E   ALTER TABLE public."base_ProductUOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1890    1891    1891            	           2604    245172    Id    DEFAULT     i   ALTER TABLE "base_Promotion" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Promotion_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Promotion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1807    1806    1807            	           2604    245158    Id    DEFAULT     u   ALTER TABLE "base_PromotionAffect" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionAffect_Id_seq"'::regclass);
 J   ALTER TABLE public."base_PromotionAffect" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1805    1804    1805            �           2604    245026    Id    DEFAULT     y   ALTER TABLE "base_PromotionSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionSchedule_Id_seq"'::regclass);
 L   ALTER TABLE public."base_PromotionSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1793    1792    1793            �	           2604    266554    Id    DEFAULT     q   ALTER TABLE "base_PurchaseOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PurchaseOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1865    1864    1865            �	           2604    266533    Id    DEFAULT     }   ALTER TABLE "base_PurchaseOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_PurchaseOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1862    1863    1863            �	           2604    267538    Id    DEFAULT        ALTER TABLE "base_PurchaseOrderReceive" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderReceive_Id_seq"'::regclass);
 O   ALTER TABLE public."base_PurchaseOrderReceive" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1870    1871    1871            .	           2604    245736    Id    DEFAULT     {   ALTER TABLE "base_QuantityAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustment_Id_seq"'::regclass);
 M   ALTER TABLE public."base_QuantityAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1814    1815    1815            5	           2604    245748    Id    DEFAULT     �   ALTER TABLE "base_QuantityAdjustmentItem" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustmentItem_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_QuantityAdjustmentItem" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1817    1816    1817            p	           2604    256181    Id    DEFAULT     u   ALTER TABLE "base_ResourceAccount" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceAccount_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourceAccount" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1846    1845    1846            E	           2604    246086    Id    DEFAULT     o   ALTER TABLE "base_ResourceNote" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceNote_id_seq"'::regclass);
 G   ALTER TABLE public."base_ResourceNote" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1822    1823    1823            �	           2604    270153    Id    DEFAULT     u   ALTER TABLE "base_ResourcePayment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePayment_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourcePayment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1886    1887    1887            �	           2604    270075    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentDetail_Id_seq"'::regclass);
 P   ALTER TABLE public."base_ResourcePaymentDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1884    1885    1885            /
           2604    272125    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentProduct_Id_seq"'::regclass);
 Q   ALTER TABLE public."base_ResourcePaymentProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1899    1898    1899            �           2604    244925    Id    DEFAULT     n   ALTER TABLE "base_ResourcePhoto" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPhoto_Id_seq"'::regclass);
 H   ALTER TABLE public."base_ResourcePhoto" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1785    1784    1785            	
           2604    270196    Id    DEFAULT     s   ALTER TABLE "base_ResourceReturn" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturn_Id_seq"'::regclass);
 I   ALTER TABLE public."base_ResourceReturn" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1889    1888    1889            *
           2604    272102    Id    DEFAULT        ALTER TABLE "base_ResourceReturnDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturnDetail_Id_seq"'::regclass);
 O   ALTER TABLE public."base_ResourceReturnDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1897    1896    1897            �	           2604    266846    Id    DEFAULT     q   ALTER TABLE "base_RewardManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_RewardManager_Id_seq"'::regclass);
 H   ALTER TABLE public."base_RewardManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1868    1869    1869            �	           2604    266609    Id    DEFAULT     s   ALTER TABLE "base_SaleCommission" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleCommission_Id_seq"'::regclass);
 I   ALTER TABLE public."base_SaleCommission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1867    1866    1867            �	           2604    266096    Id    DEFAULT     i   ALTER TABLE "base_SaleOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrder_Id_seq"'::regclass);
 D   ALTER TABLE public."base_SaleOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1855    1854    1855            �	           2604    266087    Id    DEFAULT     u   ALTER TABLE "base_SaleOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderDetail_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1853    1852    1853            �	           2604    266239    Id    DEFAULT     w   ALTER TABLE "base_SaleOrderInvoice" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderInvoice_Id_seq"'::regclass);
 K   ALTER TABLE public."base_SaleOrderInvoice" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1859    1858    1859            �	           2604    266183    Id    DEFAULT     q   ALTER TABLE "base_SaleOrderShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShip_Id_seq"'::regclass);
 H   ALTER TABLE public."base_SaleOrderShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1857    1856    1857            �	           2604    266360    Id    DEFAULT     }   ALTER TABLE "base_SaleOrderShipDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShipDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_SaleOrderShipDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1861    1860    1861            �           2604    245106    Id    DEFAULT     u   ALTER TABLE "base_SaleTaxLocation" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocation_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleTaxLocation" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1797    1796    1797            �           2604    245087    Id    DEFAULT     �   ALTER TABLE "base_SaleTaxLocationOption" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocationOption_Id_seq"'::regclass);
 P   ALTER TABLE public."base_SaleTaxLocationOption" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1794    1795    1795            h	           2604    255678    Id    DEFAULT     a   ALTER TABLE "base_Store" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Store_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Store" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1827    1828    1828            �	           2604    269928    Id    DEFAULT     q   ALTER TABLE "base_TransferStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStock_Id_seq"'::regclass);
 H   ALTER TABLE public."base_TransferStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1880    1881    1881            �	           2604    269944    Id    DEFAULT     }   ALTER TABLE "base_TransferStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStockDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_TransferStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1883    1882    1883            �           2604    245150    Id    DEFAULT     ]   ALTER TABLE "base_UOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UOM_Id_seq"'::regclass);
 >   ALTER TABLE public."base_UOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1803    1802    1803            �           2604    245134    Id    DEFAULT     e   ALTER TABLE "base_UserLog" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserLog_Id_seq"'::regclass);
 B   ALTER TABLE public."base_UserLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1801    1800    1801            t	           2604    256247    Id    DEFAULT     i   ALTER TABLE "base_UserRight" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserRight_Id_seq"'::regclass);
 D   ALTER TABLE public."base_UserRight" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1847    1848    1848            �	           2604    269648    Id    DEFAULT     q   ALTER TABLE "base_VendorProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VendorProduct_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VendorProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1879    1878            �           2604    245118    Id    DEFAULT     q   ALTER TABLE "base_VirtualFolder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VirtualFolder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VirtualFolder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1799    1798    1799            3
           2604    282438    Id    DEFAULT     _   ALTER TABLE "rpt_Group" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Group_Id_seq"'::regclass);
 ?   ALTER TABLE public."rpt_Group" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1901    1900            4
           2604    282447    Id    DEFAULT     a   ALTER TABLE "rpt_Report" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Report_Id_seq"'::regclass);
 @   ALTER TABLE public."rpt_Report" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1903    1902    1903            i	           2604    255699    Id    DEFAULT     e   ALTER TABLE "tims_Holiday" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_Holiday_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_Holiday" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1829    1830    1830            m	           2604    255852    Id    DEFAULT     e   ALTER TABLE "tims_TimeLog" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_TimeLog_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_TimeLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1838    1839    1839            n	           2604    255868 	   TimeLogId    DEFAULT     �   ALTER TABLE "tims_TimeLogPermission" ALTER COLUMN "TimeLogId" SET DEFAULT nextval('"tims_TimeLogPermission_TimeLogId_seq"'::regclass);
 S   ALTER TABLE public."tims_TimeLogPermission" ALTER COLUMN "TimeLogId" DROP DEFAULT;
       public       postgres    false    1841    1840    1841            l	           2604    255798    Id    DEFAULT     s   ALTER TABLE "tims_WorkPermission" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkPermission_Id_seq"'::regclass);
 I   ALTER TABLE public."tims_WorkPermission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1836    1837    1837            j	           2604    255741    Id    DEFAULT     o   ALTER TABLE "tims_WorkSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkSchedule_Id_seq"'::regclass);
 G   ALTER TABLE public."tims_WorkSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1833    1832    1833            k	           2604    255784    Id    DEFAULT     g   ALTER TABLE "tims_WorkWeek" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkWeek_Id_seq"'::regclass);
 C   ALTER TABLE public."tims_WorkWeek" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1834    1835    1835            o          0    234865    pga_exception 
   TABLE DATA               B   COPY pga_exception (jexid, jexscid, jexdate, jextime) FROM stdin;
    pgagent       postgres    false    1756   �      p          0    234870    pga_job 
   TABLE DATA               �   COPY pga_job (jobid, jobjclid, jobname, jobdesc, jobhostagent, jobenabled, jobcreated, jobchanged, jobagentid, jobnextrun, joblastrun) FROM stdin;
    pgagent       postgres    false    1758   /�      q          0    234883    pga_jobagent 
   TABLE DATA               A   COPY pga_jobagent (jagpid, jaglogintime, jagstation) FROM stdin;
    pgagent       postgres    false    1760   L�      r          0    234890    pga_jobclass 
   TABLE DATA               /   COPY pga_jobclass (jclid, jclname) FROM stdin;
    pgagent       postgres    false    1761   i�      s          0    234898 
   pga_joblog 
   TABLE DATA               P   COPY pga_joblog (jlgid, jlgjobid, jlgstatus, jlgstart, jlgduration) FROM stdin;
    pgagent       postgres    false    1763   ��      t          0    234906    pga_jobstep 
   TABLE DATA               �   COPY pga_jobstep (jstid, jstjobid, jstname, jstdesc, jstenabled, jstkind, jstcode, jstconnstr, jstdbname, jstonerror, jscnextrun) FROM stdin;
    pgagent       postgres    false    1765   ��      u          0    234923    pga_jobsteplog 
   TABLE DATA               t   COPY pga_jobsteplog (jslid, jsljlgid, jsljstid, jslstatus, jslresult, jslstart, jslduration, jsloutput) FROM stdin;
    pgagent       postgres    false    1767   �      v          0    234934    pga_schedule 
   TABLE DATA               �   COPY pga_schedule (jscid, jscjobid, jscname, jscdesc, jscenabled, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths) FROM stdin;
    pgagent       postgres    false    1769   (�      �          0    244946    base_Attachment 
   TABLE DATA               �   COPY "base_Attachment" ("Id", "FileOriginalName", "FileName", "FileExtension", "VirtualFolderId", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Counter") FROM stdin;
    public       postgres    false    1789   E�      �          0    256168    base_Authorize 
   TABLE DATA               =   COPY "base_Authorize" ("Id", "Resource", "Code") FROM stdin;
    public       postgres    false    1844   (�      �          0    254557    base_Configuration 
   TABLE DATA                 COPY "base_Configuration" ("CompanyName", "Address", "City", "State", "ZipCode", "CountryId", "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", "IsAllowRGO", "PasswordLength", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", "ReturnFeePercent") FROM stdin;
    public       postgres    false    1824   ��      �          0    245754    base_CostAdjustment 
   TABLE DATA               �   COPY "base_CostAdjustment" ("Id", "Resource", "CostDifference", "NewCost", "OldCost", "ItemCount", "LoggedTime", "Reason", "StoreNumber", "IsQuantityChanged") FROM stdin;
    public       postgres    false    1819   ��      �          0    245766    base_CostAdjustmentItem 
   TABLE DATA               �   COPY "base_CostAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewCost", "AdjustmentOldCost", "LoggedTime", "ParentResource") FROM stdin;
    public       postgres    false    1821   ��      �          0    271738    base_CountStock 
   TABLE DATA               �   COPY "base_CountStock" ("Id", "DocumentNo", "DateCreated", "UserCreated", "CompletedDate", "UserCounted", "Status", "Resource") FROM stdin;
    public       postgres    false    1893   ��      �          0    271745    base_CountStockDetail 
   TABLE DATA               �   COPY "base_CountStockDetail" ("Id", "CountStockId", "ProductId", "ProductResource", "StoreId", "Quantity", "CountedQuantity") FROM stdin;
    public       postgres    false    1895   �      �          0    245340    base_Department 
   TABLE DATA               �   COPY "base_Department" ("Id", "Name", "ParentId", "TaxCodeId", "Margin", "MarkUp", "LevelId", "IsActived", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated") FROM stdin;
    public       postgres    false    1809   ��      x          0    238237 
   base_Email 
   TABLE DATA               �  COPY "base_Email" ("Id", "Recipient", "CC", "BCC", "Subject", "Body", "IsHasAttachment", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "AttachmentType", "AttachmentResult", "GuestId", "Sender", "Status", "Importance", "Sensitivity", "IsRequestDelivery", "IsRequestRead", "IsMyFlag", "FlagTo", "FlagStartDate", "FlagDueDate", "IsAllowReminder", "RemindOn", "MyRemindTimes", "IsRecipentFlag", "RecipentFlagTo", "IsAllowRecipentReminder", "RecipentRemindOn", "RecipentRemindTimes") FROM stdin;
    public       postgres    false    1772   `�      w          0    238137    base_EmailAttachment 
   TABLE DATA               J   COPY "base_EmailAttachment" ("Id", "EmailId", "AttachmentId") FROM stdin;
    public       postgres    false    1771   }�      {          0    244817 
   base_Guest 
   TABLE DATA                 COPY "base_Guest" ("Id", "FirstName", "MiddleName", "LastName", "Company", "Phone1", "Ext1", "Phone2", "Ext2", "Fax", "CellPhone", "Email", "Website", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "IsPurged", "GuestTypeId", "IsActived", "GuestNo", "PositionId", "Department", "Mark", "AccountNumber", "ParentId", "IsRewardMember", "CheckLimit", "CreditLimit", "BalanceDue", "AvailCredit", "PastDue", "IsPrimary", "CommissionPercent", "Resource", "TotalRewardRedeemed", "PurchaseDuringTrackingPeriod", "RequirePurchaseNextReward", "HireDate", "IsBlockArriveLate", "IsDeductLunchTime", "IsBalanceOvertime", "LateMinutes", "OvertimeOption", "OTLeastMinute", "IsTrackingHour", "TermDiscount", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "SaleRepId") FROM stdin;
    public       postgres    false    1777   ��      �          0    245376    base_GuestAdditional 
   TABLE DATA               3  COPY "base_GuestAdditional" ("Id", "TaxRate", "IsNoDiscount", "FixDiscount", "Unit", "PriceSchemeId", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Custom8", "GuestId", "LayawayNo", "ChargeACNo", "FedTaxId", "IsTaxExemption", "SaleTaxLocation", "TaxExemptionNo") FROM stdin;
    public       postgres    false    1811   �      |          0    244863    base_GuestAddress 
   TABLE DATA               �   COPY "base_GuestAddress" ("Id", "GuestId", "AddressTypeId", "AddressLine1", "AddressLine2", "City", "StateProvinceId", "PostalCode", "CountryId", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsDefault") FROM stdin;
    public       postgres    false    1779   �      y          0    238413    base_GuestFingerPrint 
   TABLE DATA               �   COPY "base_GuestFingerPrint" ("Id", "GuestId", "FingerIndex", "HandFlag", "DateUpdated", "UserUpdaed", "FingerPrintImage") FROM stdin;
    public       postgres    false    1774   �
      }          0    244873    base_GuestHiringHistory 
   TABLE DATA               �   COPY "base_GuestHiringHistory" ("Id", "GuestId", "StartDate", "RenewDate", "PromotionDate", "TerminateDate", "IsTerminate", "ManagerId") FROM stdin;
    public       postgres    false    1781   )      ~          0    244884    base_GuestPayRoll 
   TABLE DATA               �   COPY "base_GuestPayRoll" ("Id", "PayrollName", "PayrollType", "Rate", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "GuestId") FROM stdin;
    public       postgres    false    1783   F      �          0    257325    base_GuestPaymentCard 
   TABLE DATA               �   COPY "base_GuestPaymentCard" ("Id", "GuestId", "CardTypeId", "CardNumber", "ExpMonth", "ExpYear", "CCID", "BillingAddress", "NameOnCard", "ZipCode", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1851   c      �          0    244934    base_GuestProfile 
   TABLE DATA               s  COPY "base_GuestProfile" ("Id", "Gender", "Marital", "SSN", "Identification", "DOB", "IsSpouse", "FirstName", "LastName", "MiddleName", "State", "SGender", "SFirstName", "SLastName", "SMiddleName", "SPhone", "SCellPhone", "SSSN", "SState", "SEmail", "IsEmergency", "EFirstName", "ELastName", "EMiddleName", "EPhone", "ECellPhone", "ERelationship", "GuestId") FROM stdin;
    public       postgres    false    1787   �      �          0    268354    base_GuestReward 
   TABLE DATA               �   COPY "base_GuestReward" ("Id", "GuestId", "RewardId", "Amount", "IsApply", "EearnedDate", "RedeemedDate", "RewardValue", "SaleOrderResource", "SaleOrderNo", "Remark") FROM stdin;
    public       postgres    false    1875         �          0    256013    base_GuestSchedule 
   TABLE DATA               i   COPY "base_GuestSchedule" ("GuestId", "WorkScheduleId", "StartDate", "AssignDate", "Status") FROM stdin;
    public       postgres    false    1842   2      �          0    244997    base_MemberShip 
   TABLE DATA               �   COPY "base_MemberShip" ("Id", "GuestId", "MemberType", "CardNumber", "Status", "IsPurged", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "Code", "TotalRewardRedeemed") FROM stdin;
    public       postgres    false    1791   �      �          0    268511    base_PricingChange 
   TABLE DATA               �   COPY "base_PricingChange" ("Id", "PricingManagerId", "PricingManagerResource", "ProductId", "ProductResource", "Cost", "CurrentPrice", "NewPrice", "PriceChanged", "DateCreated") FROM stdin;
    public       postgres    false    1877   �      �          0    268185    base_PricingManager 
   TABLE DATA               +  COPY "base_PricingManager" ("Id", "Name", "Description", "DateCreated", "UserCreated", "DateApplied", "UserApplied", "DateRestored", "UserRestored", "AffectPricing", "Resource", "PriceLevel", "Status", "BasePrice", "CalculateMethod", "AmountChange", "AmountUnit", "ItemCount", "Reason") FROM stdin;
    public       postgres    false    1873   �      �          0    245412    base_Product 
   TABLE DATA               �  COPY "base_Product" ("Id", "Code", "ItemTypeId", "ProductDepartmentId", "ProductCategoryId", "ProductBrandId", "StyleModel", "ProductName", "Description", "Barcode", "Attribute", "Size", "IsSerialTracking", "IsPublicWeb", "OnHandStore1", "OnHandStore2", "OnHandStore3", "OnHandStore4", "OnHandStore5", "OnHandStore6", "OnHandStore7", "OnHandStore8", "OnHandStore9", "OnHandStore10", "QuantityOnHand", "QuantityOnOrder", "CompanyReOrderPoint", "IsUnOrderAble", "IsEligibleForCommission", "IsEligibleForReward", "RegularPrice", "Price1", "Price2", "Price3", "Price4", "OrderCost", "AverageUnitCost", "TaxCode", "MarginPercent", "MarkupPercent", "BaseUOMId", "GroupAttribute", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Resource", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "WarrantyType", "WarrantyNumber", "WarrantyPeriod", "PartNumber", "SellUOMId", "OrderUOMId", "IsPurge", "VendorId", "UserAssignedCommission", "AssignedCommissionPercent", "AssignedCommissionAmount", "Serial", "OrderUOM", "MarkdownPercent1", "MarkdownPercent2", "MarkdownPercent3", "MarkdownPercent4", "IsOpenItem", "Location") FROM stdin;
    public       postgres    false    1813   �"      �          0    255536    base_ProductStore 
   TABLE DATA               X   COPY "base_ProductStore" ("Id", "ProductId", "QuantityOnHand", "StoreCode") FROM stdin;
    public       postgres    false    1826   "'      �          0    270252    base_ProductUOM 
   TABLE DATA               "  COPY "base_ProductUOM" ("Id", "ProductStoreId", "UOMId", "BaseUnitNumber", "RegularPrice", "QuantityOnHand", "AverageCost", "Price1", "Price2", "Price3", "Price4", "MarkDownPercent1", "MarkDownPercent2", "MarkDownPercent3", "MarkDownPercent4", "MarginPercent", "MarkupPercent") FROM stdin;
    public       postgres    false    1891   �'      �          0    245169    base_Promotion 
   TABLE DATA               �  COPY "base_Promotion" ("Id", "Name", "Description", "PromotionTypeId", "TakeOffOption", "TakeOff", "BuyingQty", "GetingValue", "IsApplyToAboveQuantities", "Status", "AffectDiscount", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource", "CouponExpire", "IsCouponExpired", "PriceSchemaRange", "ReasonReActive", "Sold", "TotalPrice", "CategoryId", "VendorId", "CouponBarCode", "BarCodeNumber", "BarCodeImage") FROM stdin;
    public       postgres    false    1807   ^(      �          0    245155    base_PromotionAffect 
   TABLE DATA               �   COPY "base_PromotionAffect" ("Id", "PromotionId", "ItemId", "Price1", "Price2", "Price3", "Price4", "Price5", "Discount1", "Discount2", "Discount3", "Discount4", "Discount5") FROM stdin;
    public       postgres    false    1805   �)      �          0    245023    base_PromotionSchedule 
   TABLE DATA               X   COPY "base_PromotionSchedule" ("Id", "PromotionId", "EndDate", "StartDate") FROM stdin;
    public       postgres    false    1793   �)      �          0    266551    base_PurchaseOrder 
   TABLE DATA               _  COPY "base_PurchaseOrder" ("Id", "PurchaseOrderNo", "VendorCode", "Status", "ShipAddress", "PurchasedDate", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "PaymentDueDate", "PaymentMethodId", "Remark", "ShipDate", "SubTotal", "DiscountPercent", "DiscountAmount", "Freight", "Fee", "Total", "Paid", "Balance", "ItemCount", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "DateUpdate", "UserUpdated", "Resource", "CancelDate", "IsFullWorkflow", "StoreCode", "RecRemark", "PaymentName", "IsPurge", "IsLocked", "VendorResource") FROM stdin;
    public       postgres    false    1865   ;*      �          0    266530    base_PurchaseOrderDetail 
   TABLE DATA               ,  COPY "base_PurchaseOrderDetail" ("Id", "PurchaseOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "ReceivedQty", "DueQty", "UnFilledQty", "Amount", "Serial", "LastReceived", "Resource", "IsFullReceived", "Discount") FROM stdin;
    public       postgres    false    1863   k+      �          0    267535    base_PurchaseOrderReceive 
   TABLE DATA               �   COPY "base_PurchaseOrderReceive" ("Id", "PurchaseOrderDetailId", "POResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "RecQty", "IsReceived", "ReceiveDate", "Resource", "Price") FROM stdin;
    public       postgres    false    1871   �+      �          0    245733    base_QuantityAdjustment 
   TABLE DATA               �   COPY "base_QuantityAdjustment" ("Id", "Resource", "CostDifference", "NewQuantity", "OldQuantity", "ItemCount", "LoggedTime", "Reason", "StoreNumber") FROM stdin;
    public       postgres    false    1815   ,      �          0    245745    base_QuantityAdjustmentItem 
   TABLE DATA               �   COPY "base_QuantityAdjustmentItem" ("Id", "Resource", "ProductId", "ProductCode", "CostDifference", "AdjustmentNewQty", "AdjustmentOldQty", "AdjustmentQtyDiff", "LoggedTime", "ParentResource") FROM stdin;
    public       postgres    false    1817   �,      �          0    256178    base_ResourceAccount 
   TABLE DATA               �   COPY "base_ResourceAccount" ("Id", "Resource", "UserResource", "LoginName", "Password", "ExpiredDate", "IsLocked", "IsExpired") FROM stdin;
    public       postgres    false    1846   ~-      �          0    246083    base_ResourceNote 
   TABLE DATA               X   COPY "base_ResourceNote" ("Id", "Note", "DateCreated", "Color", "Resource") FROM stdin;
    public       postgres    false    1823   M/      �          0    270150    base_ResourcePayment 
   TABLE DATA               (  COPY "base_ResourcePayment" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalPaid", "Balance", "Change", "DateCreated", "UserCreated", "Remark", "Resource", "SubTotal", "DiscountPercent", "DiscountAmount", "Mark", "IsDeposit", "TaxCode", "TaxAmount", "LastRewardAmount") FROM stdin;
    public       postgres    false    1887   �1      �          0    270072    base_ResourcePaymentDetail 
   TABLE DATA               �   COPY "base_ResourcePaymentDetail" ("Id", "PaymentType", "ResourcePaymentId", "PaymentMethodId", "PaymentMethod", "CardType", "Paid", "Change", "Tip", "GiftCardNo", "Reason", "Reference") FROM stdin;
    public       postgres    false    1885   G      �          0    272122    base_ResourcePaymentProduct 
   TABLE DATA               �   COPY "base_ResourcePaymentProduct" ("Id", "ResourcePaymentId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "Amount") FROM stdin;
    public       postgres    false    1899   J                0    244922    base_ResourcePhoto 
   TABLE DATA               �   COPY "base_ResourcePhoto" ("Id", "ThumbnailPhoto", "ThumbnailPhotoFilename", "LargePhoto", "LargePhotoFilename", "SortId", "Resource") FROM stdin;
    public       postgres    false    1785   �K      �          0    270193    base_ResourceReturn 
   TABLE DATA                 COPY "base_ResourceReturn" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalRefund", "Balance", "DateCreated", "UserCreated", "Resource", "Mark", "DiscountPercent", "DiscountAmount", "Freight", "SubTotal", "ReturnFee", "ReturnFeePercent") FROM stdin;
    public       postgres    false    1889   *S      �          0    272099    base_ResourceReturnDetail 
   TABLE DATA               �   COPY "base_ResourceReturnDetail" ("Id", "ResourceReturnId", "OrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Price", "ReturnQty", "Amount", "IsReturned", "ReturnedDate", "Discount") FROM stdin;
    public       postgres    false    1897    d      �          0    266843    base_RewardManager 
   TABLE DATA               �  COPY "base_RewardManager" ("Id", "StoreCode", "PurchaseThreshold", "RewardAmount", "RewardAmtType", "RewardExpiration", "IsAutoEnroll", "IsPromptEnroll", "IsInformCashier", "IsRedemptionLimit", "RedemptionLimitAmount", "IsBlockRedemption", "RedemptionAfterDays", "IsBlockPurchaseRedeem", "IsTrackingPeriod", "StartDate", "EndDate", "IsNoEndDay", "TotalRewardRedeemed", "IsActived", "ReasonReActive", "DateCreated") FROM stdin;
    public       postgres    false    1869   �i      �          0    266606    base_SaleCommission 
   TABLE DATA               �   COPY "base_SaleCommission" ("Id", "GuestResource", "SOResource", "SONumber", "SOTotal", "SODate", "ComissionPercent", "CommissionAmount", "Sign", "Remark") FROM stdin;
    public       postgres    false    1867   �i      �          0    266093    base_SaleOrder 
   TABLE DATA               c  COPY "base_SaleOrder" ("Id", "SONumber", "OrderDate", "OrderStatus", "BillAddressId", "BillAddress", "ShipAddressId", "ShipAddress", "PromotionCode", "SaleRep", "CustomerResource", "PriceSchemaId", "DueDate", "RequestShipDate", "SubTotal", "TaxLocation", "TaxCode", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "Paid", "Balance", "RefundedAmount", "IsMultiPayment", "Remark", "IsFullWorkflow", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "Resource", "BookingChanel", "ShippedCount", "Deposit", "Transaction", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "IsTaxExemption", "TaxExemption", "ShippedBox", "PackedQty", "TotalWeight", "WeightUnit", "StoreCode", "IsRedeeem", "IsPurge", "IsLocked", "RewardAmount") FROM stdin;
    public       postgres    false    1855   �l      �          0    266084    base_SaleOrderDetail 
   TABLE DATA               x  COPY "base_SaleOrderDetail" ("Id", "SaleOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "TaxCode", "Quantity", "PickQty", "DueQty", "UnFilled", "RegularPrice", "SalePrice", "UOMId", "BaseUOM", "DiscountPercent", "DiscountAmount", "SubTotal", "OnHandQty", "SerialTracking", "Resource", "BalanceShipped", "Comment", "TotalDiscount") FROM stdin;
    public       postgres    false    1853   �q      �          0    266236    base_SaleOrderInvoice 
   TABLE DATA               �   COPY "base_SaleOrderInvoice" ("Id", "InvoiceNo", "SaleOrderId", "SaleOrderResource", "ItemCount", "SubTotal", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "DateCreated") FROM stdin;
    public       postgres    false    1859   �u      �          0    266180    base_SaleOrderShip 
   TABLE DATA               �   COPY "base_SaleOrderShip" ("Id", "SaleOrderId", "SaleOrderResource", "Weight", "TrackingNo", "IsShipped", "Resource", "Remark", "Carrier", "ShipDate", "BoxNo") FROM stdin;
    public       postgres    false    1857   �u      �          0    266357    base_SaleOrderShipDetail 
   TABLE DATA               �   COPY "base_SaleOrderShipDetail" ("Id", "SaleOrderShipId", "SaleOrderShipResource", "SaleOrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Description", "SerialTracking", "PackedQty", "IsPaid") FROM stdin;
    public       postgres    false    1861   Gx      �          0    245103    base_SaleTaxLocation 
   TABLE DATA               �   COPY "base_SaleTaxLocation" ("Id", "ParentId", "Name", "IsShipingTaxable", "ShippingTaxCodeId", "IsActived", "LevelId", "TaxCode", "TaxCodeName", "TaxPrintMark", "TaxOption", "IsPrimary", "SortIndex", "IsTaxAfterDiscount") FROM stdin;
    public       postgres    false    1797   B|      �          0    245084    base_SaleTaxLocationOption 
   TABLE DATA               �   COPY "base_SaleTaxLocationOption" ("Id", "SaleTaxLocationId", "ParentId", "TaxRate", "TaxComponent", "TaxAgency", "TaxCondition", "IsApplyAmountOver", "IsAllowSpecificItemPriceRange", "IsAllowAmountItemPriceRange", "PriceFrom", "PriceTo") FROM stdin;
    public       postgres    false    1795   
}      �          0    255675 
   base_Store 
   TABLE DATA               G   COPY "base_Store" ("Id", "Code", "Name", "Street", "City") FROM stdin;
    public       postgres    false    1828   O}      �          0    269925    base_TransferStock 
   TABLE DATA                 COPY "base_TransferStock" ("Id", "TransferNo", "FromStore", "ToStore", "TotalQuantity", "ShipDate", "Carier", "ShippingFee", "Comment", "Resource", "UserCreated", "DateCreated", "Status", "SubTotal", "Total", "DateApplied", "UserApplied", "DateReversed", "UserReversed") FROM stdin;
    public       postgres    false    1881   �}      �          0    269941    base_TransferStockDetail 
   TABLE DATA               �   COPY "base_TransferStockDetail" ("Id", "TransferStockId", "TransferStockResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Quantity", "UOMId", "BaseUOM", "Amount", "SerialTracking", "AvlQuantity") FROM stdin;
    public       postgres    false    1883   ��      �          0    245147    base_UOM 
   TABLE DATA               �   COPY "base_UOM" ("Id", "Code", "Name", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsActived", "Resource") FROM stdin;
    public       postgres    false    1803   @�      �          0    245131    base_UserLog 
   TABLE DATA               y   COPY "base_UserLog" ("Id", "IpSource", "ConnectedOn", "DisConnectedOn", "ResourceAccessed", "IsDisconected") FROM stdin;
    public       postgres    false    1801         z          0    244282    base_UserLogDetail 
   TABLE DATA               m   COPY "base_UserLogDetail" ("Id", "UserLogId", "AccessedTime", "ModuleName", "ActionDescription") FROM stdin;
    public       postgres    false    1775   �      �          0    256244    base_UserRight 
   TABLE DATA               9   COPY "base_UserRight" ("Id", "Code", "Name") FROM stdin;
    public       postgres    false    1848   �      �          0    269643    base_VendorProduct 
   TABLE DATA               t   COPY "base_VendorProduct" ("Id", "ProductId", "VendorId", "Price", "ProductResource", "VendorResource") FROM stdin;
    public       postgres    false    1878   О      �          0    245115    base_VirtualFolder 
   TABLE DATA               �   COPY "base_VirtualFolder" ("Id", "ParentFolderId", "FolderName", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource") FROM stdin;
    public       postgres    false    1799   �      �          0    282433 	   rpt_Group 
   TABLE DATA               R   COPY "rpt_Group" ("Id", "Code", "Name", "DateCreated", "UserCreated") FROM stdin;
    public       postgres    false    1900   D�      �          0    282444 
   rpt_Report 
   TABLE DATA               �   COPY "rpt_Report" ("Id", "GroupId", "ParentId", "Code", "Name", "FormatFile", "DateCreated", "UserCreated", "IsShow", "ProcessName", "SamplePicture") FROM stdin;
    public       postgres    false    1903   a�      �          0    255696    tims_Holiday 
   TABLE DATA               �   COPY "tims_Holiday" ("Id", "Title", "Description", "HolidayOption", "FromDate", "ToDate", "Month", "Day", "DayOfWeek", "WeekOfMonth", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedByID") FROM stdin;
    public       postgres    false    1830   ~�      �          0    255705    tims_HolidayHistory 
   TABLE DATA               8   COPY "tims_HolidayHistory" ("Date", "Name") FROM stdin;
    public       postgres    false    1831   0�      �          0    255849    tims_TimeLog 
   TABLE DATA               �  COPY "tims_TimeLog" ("Id", "EmployeeId", "WorkScheduleId", "PayrollId", "ClockIn", "ClockOut", "ManualClockInFlag", "ManualClockOutFlag", "WorkTime", "LunchTime", "OvertimeBefore", "Reason", "DeductLunchTimeFlag", "LateTime", "LeaveEarlyTime", "ActiveFlag", "ModifiedDate", "ModifiedById", "OvertimeAfter", "OvertimeLunch", "OvertimeDayOff", "OvertimeOptions", "GuestResource") FROM stdin;
    public       postgres    false    1839   �      �          0    255865    tims_TimeLogPermission 
   TABLE DATA               L   COPY "tims_TimeLogPermission" ("TimeLogId", "WorkPermissionId") FROM stdin;
    public       postgres    false    1841   K�      �          0    255795    tims_WorkPermission 
   TABLE DATA               �   COPY "tims_WorkPermission" ("Id", "EmployeeId", "PermissionType", "FromDate", "ToDate", "Note", "NoOfDays", "HourPerDay", "PaidFlag", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById", "OvertimeOptions") FROM stdin;
    public       postgres    false    1837   h�      �          0    255738    tims_WorkSchedule 
   TABLE DATA               �   COPY "tims_WorkSchedule" ("Id", "WorkScheduleName", "WorkScheduleType", "Rotate", "Status", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById") FROM stdin;
    public       postgres    false    1833   ��      �          0    255781    tims_WorkWeek 
   TABLE DATA               �   COPY "tims_WorkWeek" ("Id", "WorkScheduleId", "Week", "Day", "WorkIn", "WorkOut", "LunchOut", "LunchIn", "LunchBreakFlag") FROM stdin;
    public       postgres    false    1835   �      ;
           2606    235700    pga_exception_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_pkey PRIMARY KEY (jexid);
 K   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_pkey;
       pgagent         postgres    false    1756    1756            =
           2606    235702    pga_job_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_pkey PRIMARY KEY (jobid);
 ?   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_pkey;
       pgagent         postgres    false    1758    1758            ?
           2606    235704    pga_jobagent_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY pga_jobagent
    ADD CONSTRAINT pga_jobagent_pkey PRIMARY KEY (jagpid);
 I   ALTER TABLE ONLY pgagent.pga_jobagent DROP CONSTRAINT pga_jobagent_pkey;
       pgagent         postgres    false    1760    1760            B
           2606    235706    pga_jobclass_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_jobclass
    ADD CONSTRAINT pga_jobclass_pkey PRIMARY KEY (jclid);
 I   ALTER TABLE ONLY pgagent.pga_jobclass DROP CONSTRAINT pga_jobclass_pkey;
       pgagent         postgres    false    1761    1761            E
           2606    235708    pga_joblog_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_pkey PRIMARY KEY (jlgid);
 E   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_pkey;
       pgagent         postgres    false    1763    1763            H
           2606    235710    pga_jobstep_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_pkey PRIMARY KEY (jstid);
 G   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_pkey;
       pgagent         postgres    false    1765    1765            K
           2606    235712    pga_jobsteplog_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_pkey PRIMARY KEY (jslid);
 M   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_pkey;
       pgagent         postgres    false    1767    1767            N
           2606    235714    pga_schedule_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_pkey PRIMARY KEY (jscid);
 I   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_pkey;
       pgagent         postgres    false    1769    1769            �
           2606    245348    FK_base_Department_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_Id";
       public         postgres    false    1809    1809            �
           2606    256188    FPK_base_ResourceAccount_Id 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "FPK_base_ResourceAccount_Id" PRIMARY KEY ("Id", "Resource");
 ^   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "FPK_base_ResourceAccount_Id";
       public         postgres    false    1846    1846    1846            
           2606    245266    PF_base_SaleTaxLocation 
   CONSTRAINT     i   ALTER TABLE ONLY "base_SaleTaxLocation"
    ADD CONSTRAINT "PF_base_SaleTaxLocation" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_SaleTaxLocation" DROP CONSTRAINT "PF_base_SaleTaxLocation";
       public         postgres    false    1797    1797            2           2606    282455    PF_rpt_Group_Id 
   CONSTRAINT     V   ALTER TABLE ONLY "rpt_Group"
    ADD CONSTRAINT "PF_rpt_Group_Id" PRIMARY KEY ("Id");
 G   ALTER TABLE ONLY public."rpt_Group" DROP CONSTRAINT "PF_rpt_Group_Id";
       public         postgres    false    1900    1900            �
           2606    255762    PF_tims_Holiday_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "tims_Holiday"
    ADD CONSTRAINT "PF_tims_Holiday_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."tims_Holiday" DROP CONSTRAINT "PF_tims_Holiday_Id";
       public         postgres    false    1830    1830            �
           2606    245385    PK_GuestAdditional_Id 
   CONSTRAINT     g   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "PK_GuestAdditional_Id" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "PK_GuestAdditional_Id";
       public         postgres    false    1811    1811            Y
           2606    244286    PK_UserLogDetail_Id 
   CONSTRAINT     c   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "PK_UserLogDetail_Id" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "PK_UserLogDetail_Id";
       public         postgres    false    1775    1775            �
           2606    245136    PK_UserLog_Id 
   CONSTRAINT     W   ALTER TABLE ONLY "base_UserLog"
    ADD CONSTRAINT "PK_UserLog_Id" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_UserLog" DROP CONSTRAINT "PK_UserLog_Id";
       public         postgres    false    1801    1801            s
           2606    244954    PK_base_Attachment_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "PK_base_Attachment_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "PK_base_Attachment_Id";
       public         postgres    false    1789    1789            �
           2606    256191    PK_base_Authorize_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Authorize"
    ADD CONSTRAINT "PK_base_Authorize_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Authorize" DROP CONSTRAINT "PK_base_Authorize_Id";
       public         postgres    false    1844    1844            �
           2606    245771    PK_base_CostAdjustmentItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_CostAdjustmentItem"
    ADD CONSTRAINT "PK_base_CostAdjustmentItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_CostAdjustmentItem" DROP CONSTRAINT "PK_base_CostAdjustmentItem_Id";
       public         postgres    false    1821    1821            �
           2606    245763    PK_base_CostAdjustment_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "PK_base_CostAdjustment_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "PK_base_CostAdjustment_Id";
       public         postgres    false    1819    1819            +           2606    271757    PK_base_CounStockDetail_Id 
   CONSTRAINT     m   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "PK_base_CounStockDetail_Id" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "PK_base_CounStockDetail_Id";
       public         postgres    false    1895    1895            &           2606    271755    PK_base_CounStock_Id 
   CONSTRAINT     a   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "PK_base_CounStock_Id" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "PK_base_CounStock_Id";
       public         postgres    false    1893    1893            P
           2606    238143    PK_base_EmailAttachment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "PK_base_EmailAttachment" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "PK_base_EmailAttachment";
       public         postgres    false    1771    1771            R
           2606    238253    PK_base_Email_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Email"
    ADD CONSTRAINT "PK_base_Email_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Email" DROP CONSTRAINT "PK_base_Email_Id";
       public         postgres    false    1772    1772            V
           2606    238418    PK_base_GuestFingerPrint_Id 
   CONSTRAINT     n   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "PK_base_GuestFingerPrint_Id" PRIMARY KEY ("Id");
 _   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "PK_base_GuestFingerPrint_Id";
       public         postgres    false    1774    1774            f
           2606    244879    PK_base_GuestHiringHistory_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "PK_base_GuestHiringHistory_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "PK_base_GuestHiringHistory_Id";
       public         postgres    false    1781    1781            k
           2606    244890    PK_base_GuestPayRoll_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "PK_base_GuestPayRoll_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "PK_base_GuestPayRoll_Id";
       public         postgres    false    1783    1783            p
           2606    244941    PK_base_GuestProfile_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "PK_base_GuestProfile_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "PK_base_GuestProfile_Id";
       public         postgres    false    1787    1787                       2606    268362    PK_base_GuestReward_Id 
   CONSTRAINT     d   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "PK_base_GuestReward_Id" PRIMARY KEY ("Id");
 U   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "PK_base_GuestReward_Id";
       public         postgres    false    1875    1875            �
           2606    256030    PK_base_GuestSchedule 
   CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "PK_base_GuestSchedule" PRIMARY KEY ("GuestId", "WorkScheduleId", "StartDate");
 V   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "PK_base_GuestSchedule";
       public         postgres    false    1842    1842    1842    1842            c
           2606    244869    PK_base_Guest_Id 
   CONSTRAINT     _   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "PK_base_Guest_Id" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "PK_base_Guest_Id";
       public         postgres    false    1779    1779            w
           2606    245005    PK_base_MemberShip 
   CONSTRAINT     _   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "PK_base_MemberShip" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "PK_base_MemberShip";
       public         postgres    false    1791    1791            
           2606    268520    PK_base_PricingChange_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "PK_base_PricingChange_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "PK_base_PricingChange_Id";
       public         postgres    false    1877    1877                       2606    268194    PK_base_PricingManager_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "PK_base_PricingManager_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "PK_base_PricingManager_Id";
       public         postgres    false    1873    1873            �
           2606    255541    PK_base_ProductStore_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "PK_base_ProductStore_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "PK_base_ProductStore_Id";
       public         postgres    false    1826    1826            $           2606    270271    PK_base_ProductUOM_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "PK_base_ProductUOM_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "PK_base_ProductUOM_Id";
       public         postgres    false    1891    1891            �
           2606    255615    PK_base_Product_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "PK_base_Product_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "PK_base_Product_Id";
       public         postgres    false    1813    1813            �
           2606    245160    PK_base_PromotionAffect_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "PK_base_PromotionAffect_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "PK_base_PromotionAffect_Id";
       public         postgres    false    1805    1805            z
           2606    245030    PK_base_PromotionSchedule_Id 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "PK_base_PromotionSchedule_Id" PRIMARY KEY ("Id");
 a   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "PK_base_PromotionSchedule_Id";
       public         postgres    false    1793    1793            �
           2606    245177    PK_base_Promotion_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Promotion"
    ADD CONSTRAINT "PK_base_Promotion_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Promotion" DROP CONSTRAINT "PK_base_Promotion_Id";
       public         postgres    false    1807    1807            �
           2606    266538    PK_base_PurchaseOrderItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "PK_base_PurchaseOrderItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "PK_base_PurchaseOrderItem_Id";
       public         postgres    false    1863    1863                        2606    267544    PK_base_PurchaseOrderReceive_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "PK_base_PurchaseOrderReceive_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "PK_base_PurchaseOrderReceive_Id";
       public         postgres    false    1871    1871            �
           2606    266567    PK_base_PurchaseOrder_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "PK_base_PurchaseOrder_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "PK_base_PurchaseOrder_Id";
       public         postgres    false    1865    1865            �
           2606    245750 !   PK_base_QuantityAdjustmentItem_Id 
   CONSTRAINT     z   ALTER TABLE ONLY "base_QuantityAdjustmentItem"
    ADD CONSTRAINT "PK_base_QuantityAdjustmentItem_Id" PRIMARY KEY ("Id");
 k   ALTER TABLE ONLY public."base_QuantityAdjustmentItem" DROP CONSTRAINT "PK_base_QuantityAdjustmentItem_Id";
       public         postgres    false    1817    1817            �
           2606    245742    PK_base_QuantityAdjustment_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "PK_base_QuantityAdjustment_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "PK_base_QuantityAdjustment_Id";
       public         postgres    false    1815    1815            �
           2606    246089    PK_base_ResourceNote_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ResourceNote"
    ADD CONSTRAINT "PK_base_ResourceNote_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ResourceNote" DROP CONSTRAINT "PK_base_ResourceNote_Id";
       public         postgres    false    1823    1823                       2606    270163     PK_base_ResourcePaymentDetail_Id 
   CONSTRAINT     x   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "PK_base_ResourcePaymentDetail_Id" PRIMARY KEY ("Id");
 i   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "PK_base_ResourcePaymentDetail_Id";
       public         postgres    false    1885    1885            0           2606    272130     PK_base_ResourcePaymentProductId 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "PK_base_ResourcePaymentProductId" PRIMARY KEY ("Id");
 j   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "PK_base_ResourcePaymentProductId";
       public         postgres    false    1899    1899                       2606    270161    PK_base_ResourcePayment_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_ResourcePayment"
    ADD CONSTRAINT "PK_base_ResourcePayment_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_ResourcePayment" DROP CONSTRAINT "PK_base_ResourcePayment_Id";
       public         postgres    false    1887    1887            m
           2606    270190    PK_base_ResourcePhoto_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_ResourcePhoto"
    ADD CONSTRAINT "PK_base_ResourcePhoto_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_ResourcePhoto" DROP CONSTRAINT "PK_base_ResourcePhoto_Id";
       public         postgres    false    1785    1785            -           2606    272108    PK_base_ResourceReturnDetail_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "PK_base_ResourceReturnDetail_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "PK_base_ResourceReturnDetail_Id";
       public         postgres    false    1897    1897                       2606    270203    PK_base_ResourceReturn_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "PK_base_ResourceReturn_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "PK_base_ResourceReturn_Id";
       public         postgres    false    1889    1889            �
           2606    266851    PK_base_RewardManager 
   CONSTRAINT     e   ALTER TABLE ONLY "base_RewardManager"
    ADD CONSTRAINT "PK_base_RewardManager" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_RewardManager" DROP CONSTRAINT "PK_base_RewardManager";
       public         postgres    false    1869    1869            �
           2606    266611    PK_base_SaleCommission 
   CONSTRAINT     g   ALTER TABLE ONLY "base_SaleCommission"
    ADD CONSTRAINT "PK_base_SaleCommission" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_SaleCommission" DROP CONSTRAINT "PK_base_SaleCommission";
       public         postgres    false    1867    1867            �
           2606    266090    PK_base_SaleOrderDetail_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "PK_base_SaleOrderDetail_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "PK_base_SaleOrderDetail_Id";
       public         postgres    false    1853    1853            �
           2606    266249    PK_base_SaleOrderInvoice 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "PK_base_SaleOrderInvoice" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "PK_base_SaleOrderInvoice";
       public         postgres    false    1859    1859            �
           2606    266362    PK_base_SaleOrderShipDetail 
   CONSTRAINT     q   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "PK_base_SaleOrderShipDetail" PRIMARY KEY ("Id");
 b   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "PK_base_SaleOrderShipDetail";
       public         postgres    false    1861    1861            �
           2606    266219    PK_base_SaleOrderShip_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "PK_base_SaleOrderShip_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "PK_base_SaleOrderShip_Id";
       public         postgres    false    1857    1857            �
           2606    266117    PK_base_SaleOrder_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_SaleOrder"
    ADD CONSTRAINT "PK_base_SaleOrder_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_SaleOrder" DROP CONSTRAINT "PK_base_SaleOrder_Id";
       public         postgres    false    1855    1855            }
           2606    245268    PK_base_SaleTaxLocationOption 
   CONSTRAINT     u   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "PK_base_SaleTaxLocationOption" PRIMARY KEY ("Id");
 f   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "PK_base_SaleTaxLocationOption";
       public         postgres    false    1795    1795            �
           2606    255680    PK_base_Store_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "PK_base_Store_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "PK_base_Store_Id";
       public         postgres    false    1828    1828                       2606    269949    PK_base_TransferStockDetail_Id 
   CONSTRAINT     t   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "PK_base_TransferStockDetail_Id" PRIMARY KEY ("Id");
 e   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "PK_base_TransferStockDetail_Id";
       public         postgres    false    1883    1883                       2606    269936    PK_base_TransferStock_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "PK_base_TransferStock_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "PK_base_TransferStock_Id";
       public         postgres    false    1881    1881            �
           2606    245152    PK_base_UOM_Id 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "PK_base_UOM_Id" PRIMARY KEY ("Id");
 E   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "PK_base_UOM_Id";
       public         postgres    false    1803    1803            �
           2606    256249    PK_base_UserRight_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "PK_base_UserRight_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "PK_base_UserRight_Id";
       public         postgres    false    1848    1848                       2606    269660    PK_base_VendorProduct_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "PK_base_VendorProduct_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "PK_base_VendorProduct_Id";
       public         postgres    false    1878    1878            �
           2606    245122    PK_base_VirtualFolder 
   CONSTRAINT     e   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "PK_base_VirtualFolder" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "PK_base_VirtualFolder";
       public         postgres    false    1799    1799            5           2606    282457    PK_rpt_Report_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "PK_rpt_Report_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "PK_rpt_Report_Id";
       public         postgres    false    1903    1903            �
           2606    255709    PK_tims_HolidayHistory_Date 
   CONSTRAINT     n   ALTER TABLE ONLY "tims_HolidayHistory"
    ADD CONSTRAINT "PK_tims_HolidayHistory_Date" PRIMARY KEY ("Date");
 ]   ALTER TABLE ONLY public."tims_HolidayHistory" DROP CONSTRAINT "PK_tims_HolidayHistory_Date";
       public         postgres    false    1831    1831            �
           2606    255743    PK_tims_WorkSchedule_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "tims_WorkSchedule"
    ADD CONSTRAINT "PK_tims_WorkSchedule_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."tims_WorkSchedule" DROP CONSTRAINT "PK_tims_WorkSchedule_Id";
       public         postgres    false    1833    1833            �
           2606    255786    PK_tims_WorkWeek_Id 
   CONSTRAINT     ^   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "PK_tims_WorkWeek_Id" PRIMARY KEY ("Id");
 O   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "PK_tims_WorkWeek_Id";
       public         postgres    false    1835    1835            �
           2606    257312    base_Configuration_pkey 
   CONSTRAINT     g   ALTER TABLE ONLY "base_Configuration"
    ADD CONSTRAINT "base_Configuration_pkey" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_Configuration" DROP CONSTRAINT "base_Configuration_pkey";
       public         postgres    false    1824    1824            �
           2606    257332    base_GuestPaymentCard_Id 
   CONSTRAINT     k   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "base_GuestPaymentCard_Id" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "base_GuestPaymentCard_Id";
       public         postgres    false    1851    1851            ]
           2606    244846    base_Guest_pkey 
   CONSTRAINT     W   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "base_Guest_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "base_Guest_pkey";
       public         postgres    false    1777    1777            �
           2606    255870    key 
   CONSTRAINT     p   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT key PRIMARY KEY ("TimeLogId", "WorkPermissionId");
 F   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT key;
       public         postgres    false    1841    1841    1841            �
           2606    255857    pk_tims_timelog 
   CONSTRAINT     W   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT pk_tims_timelog PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT pk_tims_timelog;
       public         postgres    false    1839    1839            �
           2606    255803    pk_tims_workpermission 
   CONSTRAINT     e   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT pk_tims_workpermission PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT pk_tims_workpermission;
       public         postgres    false    1837    1837            �
           2606    245773    uni_baseQuantityAdjustment 
   CONSTRAINT     p   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "uni_baseQuantityAdjustment" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "uni_baseQuantityAdjustment";
       public         postgres    false    1815    1815            �
           2606    245775    uni_baseQuantityAdjustmentItem 
   CONSTRAINT     x   ALTER TABLE ONLY "base_QuantityAdjustmentItem"
    ADD CONSTRAINT "uni_baseQuantityAdjustmentItem" UNIQUE ("Resource");
 h   ALTER TABLE ONLY public."base_QuantityAdjustmentItem" DROP CONSTRAINT "uni_baseQuantityAdjustmentItem";
       public         postgres    false    1817    1817            �
           2606    245783    uni_base_CostAdjustment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "uni_base_CostAdjustment" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "uni_base_CostAdjustment";
       public         postgres    false    1819    1819            �
           2606    245785    uni_base_CostAdjustmentItem 
   CONSTRAINT     q   ALTER TABLE ONLY "base_CostAdjustmentItem"
    ADD CONSTRAINT "uni_base_CostAdjustmentItem" UNIQUE ("Resource");
 a   ALTER TABLE ONLY public."base_CostAdjustmentItem" DROP CONSTRAINT "uni_base_CostAdjustmentItem";
       public         postgres    false    1821    1821            (           2606    271770    uni_base_CountStock_Resource 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "uni_base_CountStock_Resource" UNIQUE ("Resource");
 Z   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "uni_base_CountStock_Resource";
       public         postgres    false    1893    1893            a
           2606    256327    uni_base_Guest_Resource 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "uni_base_Guest_Resource" UNIQUE ("Resource");
 P   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "uni_base_Guest_Resource";
       public         postgres    false    1777    1777                       2606    268201    uni_base_PricingManager 
   CONSTRAINT     i   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "uni_base_PricingManager" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "uni_base_PricingManager";
       public         postgres    false    1873    1873            �
           2606    269972    uni_base_Product_Resource 
   CONSTRAINT     d   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "uni_base_Product_Resource" UNIQUE ("Resource");
 T   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "uni_base_Product_Resource";
       public         postgres    false    1813    1813            �
           2606    266569    uni_base_PurchaseOrder_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "uni_base_PurchaseOrder_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "uni_base_PurchaseOrder_Resource";
       public         postgres    false    1865    1865            �
           2606    256317 !   uni_base_ResourceAccount_Resource 
   CONSTRAINT     t   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "uni_base_ResourceAccount_Resource" UNIQUE ("Resource");
 d   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "uni_base_ResourceAccount_Resource";
       public         postgres    false    1846    1846            !           2606    270205     uni_base_ResourceReturn_Resource 
   CONSTRAINT     r   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "uni_base_ResourceReturn_Resource" UNIQUE ("Resource");
 b   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "uni_base_ResourceReturn_Resource";
       public         postgres    false    1889    1889            �
           2606    266303    uni_base_SaleOrderDetail 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "uni_base_SaleOrderDetail" UNIQUE ("Resource");
 [   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "uni_base_SaleOrderDetail";
       public         postgres    false    1853    1853            �
           2606    266221    uni_base_SaleOrderShip_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "uni_base_SaleOrderShip_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "uni_base_SaleOrderShip_Resource";
       public         postgres    false    1857    1857            �
           2606    255948    uni_base_Store_Code 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "uni_base_Store_Code" UNIQUE ("Code");
 L   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "uni_base_Store_Code";
       public         postgres    false    1828    1828                       2606    269938    uni_base_TransferStock_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "uni_base_TransferStock_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "uni_base_TransferStock_Resource";
       public         postgres    false    1881    1881            �
           2606    254600    uni_base_UOM_Code 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "uni_base_UOM_Code" UNIQUE ("Code");
 H   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "uni_base_UOM_Code";
       public         postgres    false    1803    1803            �
           2606    256251    uni_base_UserRight_Code 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "uni_base_UserRight_Code" UNIQUE ("Code");
 T   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "uni_base_UserRight_Code";
       public         postgres    false    1848    1848                       2606    269675 5   uni_base_VendorProduct_VendorResource_ProductResource 
   CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource" UNIQUE ("ProductResource", "VendorResource");
 v   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource";
       public         postgres    false    1878    1878    1878            8
           1259    235939    pga_exception_datetime    INDEX     \   CREATE UNIQUE INDEX pga_exception_datetime ON pga_exception USING btree (jexdate, jextime);
 +   DROP INDEX pgagent.pga_exception_datetime;
       pgagent         postgres    false    1756    1756            9
           1259    235940    pga_exception_jexscid    INDEX     K   CREATE INDEX pga_exception_jexscid ON pga_exception USING btree (jexscid);
 *   DROP INDEX pgagent.pga_exception_jexscid;
       pgagent         postgres    false    1756            @
           1259    235941    pga_jobclass_name    INDEX     M   CREATE UNIQUE INDEX pga_jobclass_name ON pga_jobclass USING btree (jclname);
 &   DROP INDEX pgagent.pga_jobclass_name;
       pgagent         postgres    false    1761            C
           1259    235942    pga_joblog_jobid    INDEX     D   CREATE INDEX pga_joblog_jobid ON pga_joblog USING btree (jlgjobid);
 %   DROP INDEX pgagent.pga_joblog_jobid;
       pgagent         postgres    false    1763            L
           1259    235943    pga_jobschedule_jobid    INDEX     K   CREATE INDEX pga_jobschedule_jobid ON pga_schedule USING btree (jscjobid);
 *   DROP INDEX pgagent.pga_jobschedule_jobid;
       pgagent         postgres    false    1769            F
           1259    235944    pga_jobstep_jobid    INDEX     F   CREATE INDEX pga_jobstep_jobid ON pga_jobstep USING btree (jstjobid);
 &   DROP INDEX pgagent.pga_jobstep_jobid;
       pgagent         postgres    false    1765            I
           1259    235945    pga_jobsteplog_jslid    INDEX     L   CREATE INDEX pga_jobsteplog_jslid ON pga_jobsteplog USING btree (jsljlgid);
 )   DROP INDEX pgagent.pga_jobsteplog_jslid;
       pgagent         postgres    false    1767            �
           1259    255547 .   FKI_baseProductStore_ProductId_base_Product_Id    INDEX     p   CREATE INDEX "FKI_baseProductStore_ProductId_base_Product_Id" ON "base_ProductStore" USING btree ("ProductId");
 D   DROP INDEX public."FKI_baseProductStore_ProductId_base_Product_Id";
       public         postgres    false    1826            �
           1259    245166 5   FKI_basePromotionAffect_PromotionId_base_Promotion_Id    INDEX     |   CREATE INDEX "FKI_basePromotionAffect_PromotionId_base_Promotion_Id" ON "base_PromotionAffect" USING btree ("PromotionId");
 K   DROP INDEX public."FKI_basePromotionAffect_PromotionId_base_Promotion_Id";
       public         postgres    false    1805            q
           1259    246209 9   FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    INDEX        CREATE INDEX "FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" ON "base_Attachment" USING btree ("VirtualFolderId");
 O   DROP INDEX public."FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public         postgres    false    1789            )           1259    271763 8   FKI_base_CounStockDetail_CountStockId_base_CountStock_id    INDEX     �   CREATE INDEX "FKI_base_CounStockDetail_CountStockId_base_CountStock_id" ON "base_CountStockDetail" USING btree ("CountStockId");
 N   DROP INDEX public."FKI_base_CounStockDetail_CountStockId_base_CountStock_id";
       public         postgres    false    1895            �
           1259    245354    FKI_base_Department_Id_ParentId    INDEX     ^   CREATE INDEX "FKI_base_Department_Id_ParentId" ON "base_Department" USING btree ("ParentId");
 5   DROP INDEX public."FKI_base_Department_Id_ParentId";
       public         postgres    false    1809            �
           1259    245391 &   FKI_base_GuestAdditional_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestAdditional_base_Guest_Id" ON "base_GuestAdditional" USING btree ("GuestId");
 <   DROP INDEX public."FKI_base_GuestAdditional_base_Guest_Id";
       public         postgres    false    1811            i
           1259    244891 +   FKI_base_GuestPayRoll_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestPayRoll_GuestId_base_Guest_Id" ON "base_GuestPayRoll" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestPayRoll_GuestId_base_Guest_Id";
       public         postgres    false    1783            n
           1259    244942 +   FKI_base_GuestProfile_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestProfile_GuestId_base_Guest_Id" ON "base_GuestProfile" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestProfile_GuestId_base_Guest_Id";
       public         postgres    false    1787                       1259    268373 *   FKI_base_GuestReward_GuestId_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestReward_GuestId_base_Guest_Id" ON "base_GuestReward" USING btree ("GuestId");
 @   DROP INDEX public."FKI_base_GuestReward_GuestId_base_Guest_Id";
       public         postgres    false    1875            [
           1259    245510 %   FKI_base_Guest_ParentId_base_Guest_Id    INDEX     _   CREATE INDEX "FKI_base_Guest_ParentId_base_Guest_Id" ON "base_Guest" USING btree ("ParentId");
 ;   DROP INDEX public."FKI_base_Guest_ParentId_base_Guest_Id";
       public         postgres    false    1777            u
           1259    245006 )   FKI_base_MemberShip_GuestId_base_Guest_Id    INDEX     g   CREATE INDEX "FKI_base_MemberShip_GuestId_base_Guest_Id" ON "base_MemberShip" USING btree ("GuestId");
 ?   DROP INDEX public."FKI_base_MemberShip_GuestId_base_Guest_Id";
       public         postgres    false    1791                       1259    268532 >   FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id    INDEX     �   CREATE INDEX "FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id" ON "base_PricingChange" USING btree ("PricingManagerId");
 T   DROP INDEX public."FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public         postgres    false    1877            "           1259    270282 .   FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id    INDEX     s   CREATE INDEX "FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id" ON "base_ProductUOM" USING btree ("BaseUnitNumber");
 D   DROP INDEX public."FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id";
       public         postgres    false    1891            x
           1259    245041 8   FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id    INDEX     �   CREATE INDEX "FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id" ON "base_PromotionSchedule" USING btree ("PromotionId");
 N   DROP INDEX public."FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public         postgres    false    1793            �
           1259    245178 8   FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id    INDEX     }   CREATE INDEX "FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id" ON "base_Promotion" USING btree ("PromotionTypeId");
 N   DROP INDEX public."FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id";
       public         postgres    false    1807            �
           1259    266544 ?   FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder" ON "base_PurchaseOrderDetail" USING btree ("PurchaseOrderId");
 U   DROP INDEX public."FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder";
       public         postgres    false    1863            �
           1259    267550 ?   FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha" ON "base_PurchaseOrderReceive" USING btree ("PurchaseOrderDetailId");
 U   DROP INDEX public."FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha";
       public         postgres    false    1871            .           1259    272136 ?   FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource    INDEX     �   CREATE INDEX "FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource" ON "base_ResourcePaymentProduct" USING btree ("ResourcePaymentId");
 U   DROP INDEX public."FKI_base_ResourcePaymentProduct_ResourcePaymentId_base_Resource";
       public         postgres    false    1899            �
           1259    266128 6   FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    INDEX     }   CREATE INDEX "FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderDetail" USING btree ("SaleOrderId");
 L   DROP INDEX public."FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1853            �
           1259    266265 7   FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    INDEX        CREATE INDEX "FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderInvoice" USING btree ("SaleOrderId");
 M   DROP INDEX public."FKI_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1859            �
           1259    266368 ?   FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip    INDEX     �   CREATE INDEX "FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip" ON "base_SaleOrderShipDetail" USING btree ("SaleOrderShipId");
 U   DROP INDEX public."FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip";
       public         postgres    false    1861            �
           1259    266227 4   FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    INDEX     y   CREATE INDEX "FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderShip" USING btree ("SaleOrderId");
 J   DROP INDEX public."FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1857            {
           1259    245099 1   FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id    INDEX     �   CREATE INDEX "FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id" ON "base_SaleTaxLocationOption" USING btree ("SaleTaxLocationId");
 G   DROP INDEX public."FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id";
       public         postgres    false    1795                       1259    269955 ?   FKI_base_TransferStockDetail_TransferStockId_base_TransferStock    INDEX     �   CREATE INDEX "FKI_base_TransferStockDetail_TransferStockId_base_TransferStock" ON "base_TransferStockDetail" USING btree ("TransferStockId");
 U   DROP INDEX public."FKI_base_TransferStockDetail_TransferStockId_base_TransferStock";
       public         postgres    false    1883                       1259    269666 .   FKI_base_VendorProduct_ProductId_base_Guest_Id    INDEX     q   CREATE INDEX "FKI_base_VendorProduct_ProductId_base_Guest_Id" ON "base_VendorProduct" USING btree ("ProductId");
 D   DROP INDEX public."FKI_base_VendorProduct_ProductId_base_Guest_Id";
       public         postgres    false    1878            3           1259    282463 #   FKI_rpt_Report_GroupId_rpt_Group_Id    INDEX     \   CREATE INDEX "FKI_rpt_Report_GroupId_rpt_Group_Id" ON "rpt_Report" USING btree ("GroupId");
 9   DROP INDEX public."FKI_rpt_Report_GroupId_rpt_Group_Id";
       public         postgres    false    1903            �
           1259    256148 0   FKI_tims_WorkPermission_EmployeeId_base_Guest_Id    INDEX     u   CREATE INDEX "FKI_tims_WorkPermission_EmployeeId_base_Guest_Id" ON "tims_WorkPermission" USING btree ("EmployeeId");
 F   DROP INDEX public."FKI_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public         postgres    false    1837            W
           1259    244035    idx_GuestFingerPrint_GuestId    INDEX     `   CREATE INDEX "idx_GuestFingerPrint_GuestId" ON "base_GuestFingerPrint" USING btree ("GuestId");
 2   DROP INDEX public."idx_GuestFingerPrint_GuestId";
       public         postgres    false    1774            ^
           1259    244839    idx_GuestName    INDEX     _   CREATE INDEX "idx_GuestName" ON "base_Guest" USING btree ("FirstName", "LastName", "Company");
 #   DROP INDEX public."idx_GuestName";
       public         postgres    false    1777    1777    1777            Z
           1259    244292    idx_UserLogDetail    INDEX     T   CREATE INDEX "idx_UserLogDetail" ON "base_UserLogDetail" USING btree ("UserLogId");
 '   DROP INDEX public."idx_UserLogDetail";
       public         postgres    false    1775            t
           1259    255513    idx_base_Attachment    INDEX     S   CREATE UNIQUE INDEX "idx_base_Attachment" ON "base_Attachment" USING btree ("Id");
 )   DROP INDEX public."idx_base_Attachment";
       public         postgres    false    1789            �
           1259    256319    idx_base_Authorize_Code    INDEX     Q   CREATE INDEX "idx_base_Authorize_Code" ON "base_Authorize" USING btree ("Code");
 -   DROP INDEX public."idx_base_Authorize_Code";
       public         postgres    false    1844            �
           1259    256318    idx_base_Authorize_Resource    INDEX     Y   CREATE INDEX "idx_base_Authorize_Resource" ON "base_Authorize" USING btree ("Resource");
 1   DROP INDEX public."idx_base_Authorize_Resource";
       public         postgres    false    1844            �
           1259    245792    idx_base_CostAdjustment    INDEX     Z   CREATE INDEX "idx_base_CostAdjustment" ON "base_CostAdjustment" USING btree ("Resource");
 -   DROP INDEX public."idx_base_CostAdjustment";
       public         postgres    false    1819            �
           1259    255517    idx_base_Department_Id    INDEX     W   CREATE INDEX "idx_base_Department_Id" ON "base_Department" USING btree ("Id", "Name");
 ,   DROP INDEX public."idx_base_Department_Id";
       public         postgres    false    1809    1809            S
           1259    238254    idx_base_Email    INDEX     B   CREATE INDEX "idx_base_Email" ON "base_Email" USING btree ("Id");
 $   DROP INDEX public."idx_base_Email";
       public         postgres    false    1772            T
           1259    238260    idx_base_Email_Address    INDEX     N   CREATE INDEX "idx_base_Email_Address" ON "base_Email" USING btree ("Sender");
 ,   DROP INDEX public."idx_base_Email_Address";
       public         postgres    false    1772            d
           1259    244870    idx_base_GuestAddress_Id    INDEX     S   CREATE INDEX "idx_base_GuestAddress_Id" ON "base_GuestAddress" USING btree ("Id");
 .   DROP INDEX public."idx_base_GuestAddress_Id";
       public         postgres    false    1779            g
           1259    244880     idx_base_GuestHiringHistory_Date    INDEX     �   CREATE INDEX "idx_base_GuestHiringHistory_Date" ON "base_GuestHiringHistory" USING btree ("StartDate", "RenewDate", "PromotionDate");
 6   DROP INDEX public."idx_base_GuestHiringHistory_Date";
       public         postgres    false    1781    1781    1781            h
           1259    244881    idx_base_GuestHiringHistory_Id    INDEX     d   CREATE INDEX "idx_base_GuestHiringHistory_Id" ON "base_GuestHiringHistory" USING btree ("GuestId");
 4   DROP INDEX public."idx_base_GuestHiringHistory_Id";
       public         postgres    false    1781            �
           1259    257338 !   idx_base_GuestPaymentCard_GuestId    INDEX     e   CREATE INDEX "idx_base_GuestPaymentCard_GuestId" ON "base_GuestPaymentCard" USING btree ("GuestId");
 7   DROP INDEX public."idx_base_GuestPaymentCard_GuestId";
       public         postgres    false    1851            _
           1259    256328    idx_base_Guest_Resource    INDEX     Q   CREATE INDEX "idx_base_Guest_Resource" ON "base_Guest" USING btree ("Resource");
 -   DROP INDEX public."idx_base_Guest_Resource";
       public         postgres    false    1777            �
           1259    257571    idx_base_Product_Code    INDEX     M   CREATE INDEX "idx_base_Product_Code" ON "base_Product" USING btree ("Code");
 +   DROP INDEX public."idx_base_Product_Code";
       public         postgres    false    1813            �
           1259    245794    idx_base_Product_Id    INDEX     I   CREATE INDEX "idx_base_Product_Id" ON "base_Product" USING btree ("Id");
 )   DROP INDEX public."idx_base_Product_Id";
       public         postgres    false    1813            �
           1259    254639    idx_base_Product_Name    INDEX     c   CREATE INDEX "idx_base_Product_Name" ON "base_Product" USING btree ("ProductName", "Description");
 +   DROP INDEX public."idx_base_Product_Name";
       public         postgres    false    1813    1813            �
           1259    271771    idx_base_Product_Resource    INDEX     U   CREATE INDEX "idx_base_Product_Resource" ON "base_Product" USING btree ("Resource");
 /   DROP INDEX public."idx_base_Product_Resource";
       public         postgres    false    1813            �
           1259    245793    idx_base_QuantityAdjustment    INDEX     b   CREATE INDEX "idx_base_QuantityAdjustment" ON "base_QuantityAdjustment" USING btree ("Resource");
 1   DROP INDEX public."idx_base_QuantityAdjustment";
       public         postgres    false    1815            �
           1259    256315 !   idx_base_ResourceAccount_Resource    INDEX     u   CREATE INDEX "idx_base_ResourceAccount_Resource" ON "base_ResourceAccount" USING btree ("Resource", "UserResource");
 7   DROP INDEX public."idx_base_ResourceAccount_Resource";
       public         postgres    false    1846    1846                       1259    270298 ,   idx_base_ResourcePayment_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourcePayment_DocumentResource_No" ON "base_ResourcePayment" USING btree ("DocumentNo", "DocumentResource");
 B   DROP INDEX public."idx_base_ResourcePayment_DocumentResource_No";
       public         postgres    false    1887    1887                       1259    270208    idx_base_ResourcePayment_Id    INDEX     Y   CREATE INDEX "idx_base_ResourcePayment_Id" ON "base_ResourcePayment" USING btree ("Id");
 1   DROP INDEX public."idx_base_ResourcePayment_Id";
       public         postgres    false    1887                       1259    271706 +   idx_base_ResourceReturn_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourceReturn_DocumentResource_No" ON "base_ResourceReturn" USING btree ("DocumentNo", "DocumentResource");
 A   DROP INDEX public."idx_base_ResourceReturn_DocumentResource_No";
       public         postgres    false    1889    1889            �
           1259    266266    idx_base_SaleOrder_Resource    INDEX     Y   CREATE INDEX "idx_base_SaleOrder_Resource" ON "base_SaleOrder" USING btree ("Resource");
 1   DROP INDEX public."idx_base_SaleOrder_Resource";
       public         postgres    false    1855            �
           1259    245314    idx_base_SaleTaxLocation_Id    INDEX     Y   CREATE INDEX "idx_base_SaleTaxLocation_Id" ON "base_SaleTaxLocation" USING btree ("Id");
 1   DROP INDEX public."idx_base_SaleTaxLocation_Id";
       public         postgres    false    1797            �
           1259    245313     idx_base_SaleTaxLocation_TaxCode    INDEX     c   CREATE INDEX "idx_base_SaleTaxLocation_TaxCode" ON "base_SaleTaxLocation" USING btree ("TaxCode");
 6   DROP INDEX public."idx_base_SaleTaxLocation_TaxCode";
       public         postgres    false    1797            �
           1259    245807    idx_base_UOM_Id    INDEX     A   CREATE INDEX "idx_base_UOM_Id" ON "base_UOM" USING btree ("Id");
 %   DROP INDEX public."idx_base_UOM_Id";
       public         postgres    false    1803            �
           1259    256314    idx_base_UserRight_Code    INDEX     Q   CREATE INDEX "idx_base_UserRight_Code" ON "base_UserRight" USING btree ("Code");
 -   DROP INDEX public."idx_base_UserRight_Code";
       public         postgres    false    1848            �
           1259    255787    idx_tims_WorkWeek_ScheduleId    INDEX     _   CREATE INDEX "idx_tims_WorkWeek_ScheduleId" ON "tims_WorkWeek" USING btree ("WorkScheduleId");
 2   DROP INDEX public."idx_tims_WorkWeek_ScheduleId";
       public         postgres    false    1835            l           2620    235953    pga_exception_trigger    TRIGGER     �   CREATE TRIGGER pga_exception_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_exception FOR EACH ROW EXECUTE PROCEDURE pga_exception_trigger();
 =   DROP TRIGGER pga_exception_trigger ON pgagent.pga_exception;
       pgagent       postgres    false    19    1756            �           0    0 .   TRIGGER pga_exception_trigger ON pga_exception    COMMENT     ~   COMMENT ON TRIGGER pga_exception_trigger ON pga_exception IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    2924            m           2620    235954    pga_job_trigger    TRIGGER     j   CREATE TRIGGER pga_job_trigger BEFORE UPDATE ON pga_job FOR EACH ROW EXECUTE PROCEDURE pga_job_trigger();
 1   DROP TRIGGER pga_job_trigger ON pgagent.pga_job;
       pgagent       postgres    false    1758    21            �           0    0 "   TRIGGER pga_job_trigger ON pga_job    COMMENT     U   COMMENT ON TRIGGER pga_job_trigger ON pga_job IS 'Update the job''s next run time.';
            pgagent       postgres    false    2925            n           2620    235955    pga_schedule_trigger    TRIGGER     �   CREATE TRIGGER pga_schedule_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_schedule FOR EACH ROW EXECUTE PROCEDURE pga_schedule_trigger();
 ;   DROP TRIGGER pga_schedule_trigger ON pgagent.pga_schedule;
       pgagent       postgres    false    1769    23            �           0    0 ,   TRIGGER pga_schedule_trigger ON pga_schedule    COMMENT     z   COMMENT ON TRIGGER pga_schedule_trigger ON pga_schedule IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    2926            6           2606    235956    pga_exception_jexscid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_jexscid_fkey FOREIGN KEY (jexscid) REFERENCES pga_schedule(jscid) ON UPDATE RESTRICT ON DELETE CASCADE;
 S   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_jexscid_fkey;
       pgagent       postgres    false    1756    2637    1769            7           2606    235961    pga_job_jobagentid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobagentid_fkey FOREIGN KEY (jobagentid) REFERENCES pga_jobagent(jagpid) ON UPDATE RESTRICT ON DELETE SET NULL;
 J   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobagentid_fkey;
       pgagent       postgres    false    1758    2622    1760            8           2606    235966    pga_job_jobjclid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobjclid_fkey FOREIGN KEY (jobjclid) REFERENCES pga_jobclass(jclid) ON UPDATE RESTRICT ON DELETE RESTRICT;
 H   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobjclid_fkey;
       pgagent       postgres    false    1758    1761    2625            9           2606    235971    pga_joblog_jlgjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_jlgjobid_fkey FOREIGN KEY (jlgjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 N   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_jlgjobid_fkey;
       pgagent       postgres    false    2620    1763    1758            :           2606    235976    pga_jobstep_jstjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_jstjobid_fkey FOREIGN KEY (jstjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 P   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_jstjobid_fkey;
       pgagent       postgres    false    1765    1758    2620            ;           2606    235981    pga_jobsteplog_jsljlgid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljlgid_fkey FOREIGN KEY (jsljlgid) REFERENCES pga_joblog(jlgid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljlgid_fkey;
       pgagent       postgres    false    2628    1763    1767            <           2606    235986    pga_jobsteplog_jsljstid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljstid_fkey FOREIGN KEY (jsljstid) REFERENCES pga_jobstep(jstid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljstid_fkey;
       pgagent       postgres    false    2631    1767    1765            =           2606    235991    pga_schedule_jscjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_jscjobid_fkey FOREIGN KEY (jscjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 R   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_jscjobid_fkey;
       pgagent       postgres    false    1758    1769    2620            N           2606    255621 -   FK_baseProductStore_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id";
       public       postgres    false    1813    2712    1826            F           2606    246204 8   FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" FOREIGN KEY ("VirtualFolderId") REFERENCES "base_VirtualFolder"("Id");
 v   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public       postgres    false    2690    1789    1799            h           2606    271772 7   FK_base_CounStockDetail_CountStockId_base_CountStock_id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id" FOREIGN KEY ("CountStockId") REFERENCES "base_CountStock"("Id");
 {   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id";
       public       postgres    false    1893    1895    2853            L           2606    245349 .   FK_base_Department_ParentId_base_Department_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_ParentId_base_Department_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Department"("Id");
 l   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_ParentId_base_Department_Id";
       public       postgres    false    2706    1809    1809            >           2606    238255 -   FK_base_EmailAttachment_EmailId_base_Email_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id" FOREIGN KEY ("EmailId") REFERENCES "base_Email"("Id");
 p   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id";
       public       postgres    false    1771    1772    2641            M           2606    256202 %   FK_base_GuestAdditional_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id";
       public       postgres    false    1811    2652    1777            B           2606    256207 "   FK_base_GuestAddress_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "FK_base_GuestAddress_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "FK_base_GuestAddress_base_Guest_Id";
       public       postgres    false    1779    1777    2652            ?           2606    256212 .   FK_base_GuestFingerPrint_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id";
       public       postgres    false    1774    1777    2652            C           2606    256217 0   FK_base_GuestHiringHistory_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id";
       public       postgres    false    1781    1777    2652            D           2606    256222 *   FK_base_GuestPayRoll_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id";
       public       postgres    false    1783    1777    2652            W           2606    257333 .   FK_base_GuestPaymentCard_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id";
       public       postgres    false    1851    1777    2652            E           2606    256197 *   FK_base_GuestProfile_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id";
       public       postgres    false    1787    1777    2652            ^           2606    268363 )   FK_base_GuestReward_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id";
       public       postgres    false    1875    1777    2652            _           2606    268368 2   FK_base_GuestReward_RewardId_base_RewardManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id" FOREIGN KEY ("RewardId") REFERENCES "base_RewardManager"("Id");
 q   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id";
       public       postgres    false    1875    2812    1869            V           2606    256031 +   FK_base_GuestSchedule_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id";
       public       postgres    false    1777    2652    1842            U           2606    256023 9   FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2753    1833    1842            A           2606    245511 $   FK_base_Guest_ParentId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Guest"("Id");
 ]   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id";
       public       postgres    false    1777    2652    1777            G           2606    245230 (   FK_base_MemberShip_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id");
 f   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id";
       public       postgres    false    2652    1791    1777            `           2606    268533 =   FK_base_PricingChange_PricingManagerId_base_PricingManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id" FOREIGN KEY ("PricingManagerId") REFERENCES "base_PricingManager"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public       postgres    false    1873    1877    2817            a           2606    268526 /   FK_base_PricingChange_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id";
       public       postgres    false    1877    2712    1813            f           2606    270285 6   FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id" FOREIGN KEY ("ProductStoreId") REFERENCES "base_ProductStore"("Id");
 t   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id";
       public       postgres    false    1891    2743    1826            g           2606    270277 $   FK_base_ProductUOM_UOMId_base_UOM_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id" FOREIGN KEY ("UOMId") REFERENCES "base_UOM"("Id");
 b   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id";
       public       postgres    false    2694    1803    1891            K           2606    245248 5   FK_base_PromotionAffect_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 x   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id";
       public       postgres    false    1807    2703    1805            H           2606    245253 7   FK_base_PromotionSchedule_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id");
 |   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public       postgres    false    1793    1807    2703            \           2606    266570 ?   FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_" FOREIGN KEY ("PurchaseOrderId") REFERENCES "base_PurchaseOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_";
       public       postgres    false    1863    1865    2806            ]           2606    267545 ?   FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas" FOREIGN KEY ("PurchaseOrderDetailId") REFERENCES "base_PurchaseOrderDetail"("Id");
 �   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas";
       public       postgres    false    2804    1871    1863            e           2606    270170 ?   FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id");
 �   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa";
       public       postgres    false    2841    1887    1885            j           2606    272137 ?   FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentProduct"
    ADD CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_ResourcePaymentProduct" DROP CONSTRAINT "FK_base_ResourcePaymentProduct_ResourcePaymentId_base_ResourceP";
       public       postgres    false    1887    1899    2841            i           2606    272109 ?   FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu" FOREIGN KEY ("ResourceReturnId") REFERENCES "base_ResourceReturn"("Id");
 �   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu";
       public       postgres    false    2845    1889    1897            X           2606    266129 5   FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1853    2789    1855            Z           2606    266260 6   FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderInvoice"
    ADD CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_SaleOrderInvoice" DROP CONSTRAINT "FK_base_SaleOrderInvoice_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2789    1859    1855            [           2606    266363 ?   FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_" FOREIGN KEY ("SaleOrderShipId") REFERENCES "base_SaleOrderShip"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_";
       public       postgres    false    1857    1861    2793            Y           2606    266222 3   FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1855    1857    2789            d           2606    270034 ?   FK_base_TransferStockDetail_TransferStockId_base_TransferStock_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_" FOREIGN KEY ("TransferStockId") REFERENCES "base_TransferStock"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_";
       public       postgres    false    1881    1883    2832            @           2606    266390 /   FK_base_UserLogDetail_UserLogId_base_UserLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id" FOREIGN KEY ("UserLogId") REFERENCES "base_UserLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id";
       public       postgres    false    1775    1801    2692            c           2606    270029 /   FK_base_VendorProduct_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 p   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id";
       public       postgres    false    1878    2712    1813            b           2606    269667 ,   FK_base_VendorProduct_VendorId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id" FOREIGN KEY ("VendorId") REFERENCES "base_Guest"("Id");
 m   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id";
       public       postgres    false    1878    1777    2652            J           2606    245123 9   FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId" FOREIGN KEY ("ParentFolderId") REFERENCES "base_VirtualFolder"("Id");
 z   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId";
       public       postgres    false    1799    1799    2690            k           2606    282458 "   FK_rpt_Report_GroupId_rpt_Group_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id" FOREIGN KEY ("GroupId") REFERENCES "rpt_Group"("Id");
 [   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id";
       public       postgres    false    2865    1903    1900            R           2606    256119 (   FK_tims_TimeLog_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 c   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id";
       public       postgres    false    2652    1839    1777            Q           2606    255858 3   FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2753    1833    1839            S           2606    255871 3   FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id" FOREIGN KEY ("TimeLogId") REFERENCES "tims_TimeLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id";
       public       postgres    false    1839    1841    2761            T           2606    255876 >   FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission" FOREIGN KEY ("WorkPermissionId") REFERENCES "tims_WorkPermission"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission";
       public       postgres    false    2759    1837    1841            P           2606    256143 /   FK_tims_WorkPermission_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id");
 q   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public       postgres    false    1777    1837    2652            O           2606    255788 4   FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1835    1833    2753            I           2606    245269 ?   base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati" FOREIGN KEY ("SaleTaxLocationId") REFERENCES "base_SaleTaxLocation"("Id");
 �   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati";
       public       postgres    false    1797    1795    2686            o      x������ � �      p      x������ � �      q      x������ � �      r   X   x�3��/-��KU�M��+I�K�KN�2�tI,IT��-�/*�2��\+�</�477�(�8�$3?�˔�7�895''1/5���+F��� �B�      s      x������ � �      t      x������ � �      u      x������ � �      v      x������ � �      �   �   x���kj�0F���*��of4��"���p�]�!M�_U��5�RB�"�  $P"s��ΏC�x�h�f������\�k�̆��nd���QA,,dl4����@�>EJ��S���U�p\r�z�8U���M���TH���~�k�^a��R���s����?����zx�/ㆼn>e)D>�3���M�J�Y���$�-ߞEKw�M��{��'��l�      �   k  x���K�!�g/�A`��,#��%�sng�������NE����c`�
�9"�h{��Q~b}Qow���-�Ľ��X(������0�8ԉh�S���O�e��Ia����w���Ѷc��d�;��@+�9w�	�O^�hY�n�Q�٢��%�;��)oq�N�-�2�n1'n�Xs�#m֕�� �c�}¡�����g�Ri�r2�5����k�*���I����|���o=��⡛N3�8$[c�b�7�CR�)�H���K���
�}��^۬V��*��A�nhv�e,��Q�r<A2�ß	���ߙ�&Z���s��z�<K��P�kۦ�f4�mbw��s��b~���z��5�
      �      x�u�I�]�q�ׯE�4r��m	��H�¨M�D0���-�z����p��z�}�'ΐ�~��?����o�����S.�_���?�������߿���7���h1�Tjy���c���~��������?}����>|���<C�����?������?�����?��׿��?�����~�����̗�"_��׿���^������~�?~�����ۯ?���7~H!��%����ޖ_oo�}�����Bq>J}~`UϿ��^7C�oG��>/����{�_����֗��b�����7���J5�)��#%�tSH9�4rO+m�qr̖C�l�u��<��u��𬠤������o}����z�����k}}���k̟��ޥ���C��oz�l+�`�*L�߼�j�������?��<�H\Ũm{g�����x�2��������\�Oz)f
I�"�IŌ���Z,�)��I�N�VsK*_�_V;�+�g���j�W�Uۗw�C����7�������C�4T9Ϭ�=��S�B�!�\K/}�0��s<�K��j��ּZ��+��T0��F*��x�c$Oq��=�߬'Z��͋��I�K���Lj��˩����zvc{�{���u�O����L}2:q/P;G�9Z.֋5+�}���}ʲY.])5��Շ�d�cw�����w���L���x�w�uz��Q�4Z�g���:�K��e~�d����7~w��U�1,����m����ӌ9Q����)�~�e��f�l������6��u�=$�Ӣ�9)�צ)%�`��qjW�td��h��KJ�^RYb����}h���G���׾)����6O��ʸ��{�_�R���E�R�A�����G��4�U�_�s� ܏Ls�s��ɋW��k�S��'�}��4��.ÔWp$;O�r�i�:����x����<�L����}�Z;7�]R�t��	��^a)��8/kZ
V��j�Ě��)�]�\��/�a)�B���pf
��Q��<�C��b���ЧD�N�V��kz�-K��lL�|zKX�rXW9�z�i��L��^S��齕����.��U�Fs>/<��@=���GeE�p��zO{e�孳���������m�LE-\ê1�y�1��e�e�k;�˕ך�����1�����������o-�̊���3�sEz���sS9��ɜ����uzz�B��.Zy��@DJ���q�;��r�
�tJa�Mkh~��Ȯцߦ�ob����f4 t� �+B�*Eah����!��\GU6fF���!���(���f����l/R@�7�cy�e����Ǭ�{T�C�/��d���Y���ii�׸m�8(&s�CW@��t�i�u�{[��%��9;>�x]e� ��Ñ.��4F=��`,S3�ڳ�����&��6���n�D9lQ+6f���u��T���r4�)�	�摫1��	w@����V�3�.��. :m m�Tt�>�1 Qe��Q�3��~���K����^��BS|���y*�w�"aC<���ǃ�;ю�U�Pf�Tk�=�+�l*7v�4i۠���a�;ԃ?��Y$B�jcL���̥�u��d4s@[��>$��[�5�G#2H�X��Q+ʻ,!�M�Q�mw�}���G*�YV�|��g &F^�`㤋����U�H� îI��C��1ai<,�Wك����v���'���܊,G�d`j�	8o�ȄP�<()�.7ʛ4�>�-��Tׁ%����0LXH�C�0㡂,�s(څϨ�˔��;ۍ+ìl:]�a�.f%�� (zs�D�����@/@3ױ8H
�\+O���r8�w>�lw86��׃F&�};pY(�͘WГn͌�*�N��i(�[�Y����-;_��nv\.?��7�!T
B�F���@�٭�#ᥨ%Dю�J���I�X�S@)���n<c�i�a�s�e@��
��rf�zn���ʋ���(y��,�dc.��qZ�β��+�ٚT�y(@b�^AG�F�����o@-�eh���!����$��P�xp1�2���|Ȟ�UtP�+��Ʈ��qq��v�pt)���!���U�5G�y����"FߌBD ��~N�/.SXC��"�A��p�CZ1$,�pTs����Rx���a������b��[�H��-�eJ�(�#�EcgX�3 �
#��@���j��R{��p�׀2��V�DbP�IŘÅ��gb3J���h�eNvkA=�xL�Y���tK��JD�QbQLY9J>6���=*�i8��LPc6�n�q=@kXB�7�$�{=�MB4�;�V�d%�a�2(L��J{�xʪ�:�:����!��x�z���ĕ�)cN�.��č��=�F�	"bB4�p�<�{��ME��;�_V�)���D#ZAA�1�v�`��q����LcU���n�r�L�nx��dp�ݧ�7�*{Oj�6&O�z�of�UΖb_�~������~x�9��X�y��H�)�TB���s��r2~t"�eAI3C�`�	3@����g�����&���\���X�覦�ø+c�઺np
WG�*8	�P0gĒt�$ݱG:�� �U�Z8�f�k�i�p�&_b!�0#����֞�I)�h���X��X�(�*���$NQԑ�6d��^1�ݕ̵�)e!�|S������4$��s.�7�/e Hz�u>w"F����*+
"����
�R�Q��`�u��ƛ� ׭�|;N�<l-�7��d��".�Y�1��o jbj�^�۲�PY�e�ACH��P�F�W�op���7��0u�a|A��Z�|dk��0����i7��C��ג��]ɼU̔6p�Pe#.�G�"���\�������
Xe�y!-�~�R)�*��fq�����$fxH\tF)ڀ	w���<�}�VX!�a�?g��`�G�e���kH��]���'�Q��XcAH,����܋�I��y.�G�C���$�a�xV�l">��9��� q����E��Uf�"&b�S��X�R['��T
|8�� ��CL�p�M-�-2��6^�hAg$��q���H*OT�0�BS*1��S\nk�)[�� tEZ��М2��ej�|��+���T&7���-P��y8#��Q:��g�W�h��"��H@msoH�@ee^PY� J(�F����Lؠ�,O��Q����T6��Q�5�N���w�X:�C+2V[�%UG�m�H���0f��B�B�j��x��&Awf�ŝ�W��ɦ��%o�4��DA�ұ�#@Kq���G�d�c��˞e ��'e.�%�t�h6�b���`krL���Q��t�)��B��9���B��;Ⱥu���7Ỳ[l��k89$8�	�ڮ?t 5zC��]o���-�k����Wr�#�c���aE�#X;���$��5t��l�W(	�-�6�����֫�:������Qmc|����:��]�@��-S�ʄ�Be%(��	�,�@.��CS�aK���:�
;�5��^��i�g�C���]:(x���ą�wu�#�`{�������#F�D�cj��I������S6�����
�w�9��C|�CP�x|Y�x�P�k���d�qz�2�V	c�m�L�����й����ѩ�Hj��v�"�u��t+7D�d�&w2��J �@a(PPOY!���
���N7�ۨ×U����b `-�އf�:�����"\u&@���8Id�{>�U�x �|7�20���ʑH7�����9`Ę��[	��s���<L�X��\�w�������c��\��Mn���o_j��W��A�?�T��@J�'ŕ��-�0*�W9;��7.���Q"ܚ�,����&vw������G�w�?.'�+R�qw��s�̅��n,t����udǙ)�����9Ɛ��4��G�j@\,5�$�.bb�ۨ.���R�v�t�u��X�%���2����"ԟ�)C�H&o$�D7u�   >|�WTU���/ǚ�R"�H�u=ҋ�cD��IuD"������"F�<��+��ա�D�=n�- ���f"��6�2OI(�Qd��}��/���(x&�y>N��	����=��%B�\H�( ����&2�v@,� Ríc!�kL`7	�*�㥐i�C�� C���b�-��Z6��YϞ��q:�����a:�H���&B�NG�sgc�c�q-�QEWD��>N��\��h�n��iH�:� ��0 ���JӦ'C$H�C������ی����P��1��݂����t����NF�T��r�A���h<��k���l:L �H��>= ?%��Y?�t#L:�P�OT��a~�N+�B�ɨZ�2(��g=')D9/`�~y�!鍜�&�%r/L>�AqV�8i�&ψ�$�����	�==��8���!c��0'`i��$�z	?a�x �@"�H̓]�#�}�%�C���T��k ��YR
ٮ��5B��H��F@Y�:f#�!��>��!Z~M�.7U �GGT��M���l+c}�y����Md���x���-�'�C/�8�2�sC@ĘA(�UQ(F�c�˃L�i�h9�������bT�M�m�I\oX`���.� �9u� Z2��B�%;���|�窣0͒;4d%b֙Z�4���@��sW6`���a�b���v��%� Y���T<N�YJ�̾�,n��B+�Hx#=aP�\�<��M���:�HG`�-�'�
�z����b�q1>!nR9��@��@�h7&	���k��uǑI�A���ka��|�MtWl�U��<�b@R���a��u܅1���I4d�صU[Zz߭�1[��<麙���D�1�]M��S�"�Fh�Ȭ���&�9t�g��קۗ��h\� by �= b��! �f�+�e8�%Z�C�oT���C#$�I!�2.���fd�y��L�8�^��	k�Tt���(9�B�	c �xJ�gK��dR�V �Q�Ƒ�?��YN�6��B��#���~X�=���������u ��U(ܡ�*�D3(L����4Z���b��h�!y��DΈ�H�`P��`��辚��DJ�J�ĝ��o�Ι���j'8��(�U@U�a'!�%�4A�ɠs��������p�%�\Lݼ��hF�AD�n7���L^$\�C�J� cu)(�e�V������SBH�P�d.�P>X���壝5"?��b�alrɐͷ��Qf���R$�����W���G�T�x��،n�aRГ��*t���fS\��x�>�ߺ~���q-����@a���~p��b�ѽ������K�a<8̐(����,�~�	~�a�lHo��Q+g�U����Ə:`� j���0t����,$��V'�����ݺ�t���a�+?�ʡ[~e�58Iiu���n����'/:�ͺ�	�L1��(|v��L��[{;ڋ��N�G��p��}�	Z(A;��o��-����o�ʪ��y���)�������V�.�W���>��K��u_����x��b�}��7�������}���7�|��jJ      �   �   x�m�;n�0Dk�{
�ğ��i,[*$)ֹ��1RxSL��	H��|S,�+���sG��6m/��ҙL��*l�d(�I��})�h�R����x������?IA� ����(#�2a�{]e5#P��&7�jg3�k�7ЙF�t��<��c�jؚ[�j2��K�����7�,�n��gN)� �Q�      �   �   x�}��m1೧�4���<��e<��/!>$������ � )�CG���&��a����[�`e)b&��p.|2c��w��|	.�+�tM4/�><A:a��{H'�%u�ǡ��bCm��n���©�iMOƊ��'��`�_T�q
VK<Ɂ�	s��3+�	�+t�}Un�+9��>�2��;ePC��j�=U�~Bћ�W���Kpz���7sҐ�w�g�ZCm=�O��u��V`N      �   h  x���KjQE�]���OYDV���Ɔ؞d�D��n�qcjR���+�HN$h�d��'F@�o�?�?�6��T�$���Ԙ��9ǎ��>H�P�7��iic[A�(}�,>�f���0AA���"'�R֊-��tT��u���vPyE����-(>��E��wD�Bo}C-�W�G�����&7����0�3y�@�=;�T�+?�T��+(�h�)��m�k�̪#�t�u�UB%/(�繑+��P�#V�2���]U��sB������`<�\��FTHQm�J6�4���G]eq��*h�W�|~2��#rиf.k��@@B�e��r�-vͬ�lp�!�}�e�RT�#�l����M���Fc
�Y|1ԉ�e�n�ت}�k�t0_:p.7��P)"� E��&���(�Xi��(���ʧ���x7$�H��������
+ǵ2ٮ�]�%�WϹ,��+?I�r`k3h=si���VÃ�EV�$۰�>m��q�F��y(7ε��C~9$��h��7Y��쟧��wO�zQN�z��#�=���R��2k�xfq�p��˼��{�����W1a��u�}T���e�x����@z�]      �   �  x��WI�k+S{�l{���/ዛ�����DWn�l's]P�L.S��h���xi1�J�|�D�M������X.q��Oʎ����?g�F�4���\p�7�JvMk2��s�K�%��%�զ���!=.���P���*�z���e/]*�4�4VE�X����9� �Œ�*�� �����}Z3��F_%m�'X=��45`��x�)���y���V� �����CI�z�+�Z������� ��FƧg�MIa#��NJ��j����BR|뉼%]:F�Ã��̇3�h$�{�xy)F�B�E+#vyՇ6�|���]���A��&6[~Ԟ��wO��O�è�������}�'���)q��,q3�j�$�*�Kݧ:�"��P]i!��L ����HN�.6?QtȮ_F�>���BL� #8�<�!vc���l��j�f��%�z�|ʑϷRg���`��SW�,���'u��a&�� L�J�|к����.>��8		�Q�\B�v��Y�S�u�lƧ����v�>@�����d	�e��:ux�\�5��;EJ�T݊I���l�D��Cy�6�q���`��f��ZE}x2	]\%B	�B&d���ȋ4ފ T� �6S M�ޒ�wu!�rm2�U����ܑ�m���A~M
d�a�>Q���DÃĀ���oN=�[N��۽�͙��,y�	)<:���6x~�y����%+��y��=Ht%]sn���"�?-P����A��Yv�������p���~-y��|
��Wzq�����1NrR9"څuD��(�KJ�(�6$��`Qr�>'&v)��=?���R�J�}y�%����rF�c
��)��6?"Q:�jzFa���[�8a��b1�>b�c���p�+7ɫ��{������b�W�GG�s_�+N��2:�DK�?є-�"�؂��48�h\[�ci��� 5�#���u���� ��⏔�򙺫�Lk�D��Z]��G�/�p8յ���"���Q,ay�^�Ӝ��ѓ��S����yk����5�GG��q�v�(���/x�Rh5e����T�AF9�����pS�.^ �e�b�t�GE����!����8{�����8d�- �ڧ�׌��n�u��P	�;�^�(���,��|r�_32�(/����t ����:^�1_��(A�}k��y?ȣ!�����z|�����UuRkŎu�1��o���������k��GD{���	f��6ώ�!;�:K�)��Wo��{=��'��H{�X�GE�G�Q�ݕ�F���}f��Oe�w�]�Jo����L�ڇ }.;��g����n%�o�Z���O[�Ƀ�Ֆ�i���K��$�����I4�mu�+g
�/�������Y������9�e?R�`�P\3�.�,mm�ۍ�����������_      �   �  x���Mn�����S�Mԫ��Ζ {��Gk<?�MYf[�%R�O:�m9A�M���>�o�Wٲ۲l�� �J��������}]ݞ׳��v�<���V�g��|��;)�\�L��T��YJ�S�2�^��I'���Kp�K��RZ�=K��� �NA�JF�8��n��!ߢ���Re^�X#ϖ����ß���I����t<�����z��/��{p)�\�\�L(p��YJ�g�����Ve�2�5��8���s�r�3g�K�'���N�Lѕ&�+u�	EC�	�yǵ�� Fn0ҋ�����Q�(�wN4����e��<����b=U#GR|.EƕR\�YJ��ˢJG�W�oסb�I��&�&�
v���,f�P�w?��W�w���E:��H�
��t����k���I���:��&��8@���?�d�� Ox�.ʻ��]�z,S��zh���ܔ7aIK����ԘC����jZ���=U�cV�*�ի�,4�AYMH���TE����˰&��/�����t@�R��q��$b5)�SJ"6�a�\c^�����?��a^Z���D�.�Ꞓ�Z��}S��x��^=�JK�6f�3����DJvV��@in�%��+�R��6頦	E:���C�tm,�=ԫzQO)����O���X,(����G=J�`�I9-�KJ(�6�夼F�ҕ\��<,V3l�_��.����T6��)��3{���z�A�\�Lk���Y�\ϙ¯��.�^K��,%��$,��1�T�5%Ұa�²��1%�n�����A�=;+o���� �ξ�VŔ)��/���ԐA%�KY�.WuEIE�/�m�*�d�G�j�O�rU,*J��Ov�a��}J�o��1��GC�Q �Q����t����d踝�F3"e<ǜ��*�M7W}�}B�~",�Ԛ�MF����ןG�D�%����)ؤ��=�*���
M�v�}��mT	�r��Y��Tc:�f��U�}E�4[����xA�s�(���['kt�gRJB��lT��DD�Ĳ��OO� k��d�b�,� kD|,�0��f:���g�������o��M`��Y)>��%��������]IJ�EtZ�*��*�
��ф�����[^�W������E�c��}C���w�6�"�<�I� h��cP&�"@��&:d�n����cW'#�� 3ܕ���S�������.9$�]LB��&u��fxу�<�� U��?
ez��,��ցc�:�����d4��F�<S�`*�\��q.�H��߇���w��aRp�ߜ���[���lŤe>�4�6V<TjO���B��w���Y/��.�w�(>d�)}�BY]�aqӲ��o��ِ���h`�R�n�~�N^��L&�2�t�6�;�b�����5|<��ϕ����w�ᏏLǙ�;�/t#�ƍ�	�N��V�b��Q^�٤2N�����N`p2xD�3h�i�q	n�zaYt�6XT=�v��3Ͻ2(�`���E'p�a!3e�!��1�z�`Oܿ���M�P1x�P����늙o7��Z#d���i�=�m��h��á�0�~��M��ftP��y^��qzq[�Zv�.���.���h2u�=�)Ŵ�j��q�� ˜cRQ� �Y3�WU��n�qx&���yl��.�)�i?�O�iMq���K/`6���J��Et�����|wQݐ��M��6W:��	8-�,%����G�N?�1K�V>|���I�m�+�u���3�p�h�/i�7��e0e��ڌ�v���9�+�B�5�ٓ�GKI�����.��:��<�66N¾����#�8��r7h�t��=[B�k�=/ڻ�*gl͜�^^�v�7*��=yi�]�6e��r��n>�M�!�������ic�����oY�$�\HX      x      x������ � �      w      x������ � �      {   O  x��Z�v��]���/����U�q�q�Gǫ��I�$�%���>��U~ �d���?�Or EpP�	��D�ݡ@��|���\4��6o/?|,w�׫pw�\��˿��3J��d�����^�n����p}Ӧ���"��뻆���+&�B�%�����v�*�$_����Rĕ�M׈f�ɕp��h�8~v���/ص<w�H���cʋ�\��%�qn�!��?�>y�\��>|�32�!!뜠�&,������`��Ar!,Nx�eK���/�_�y�s� .�����W�/��x�.GYN�Zi�rڱ@�0ǥ�щbT8q���yk�|S��?���������u2�7�ʢ�;�Z[ځb�xݗ��t���R��.�Z�i�s/h�d[��yŠPlJֻ�r�S���!��:����覶D�U��J�	ϝ��D�q�*h!x�
�[Ån�����d�N���дZ����,�Ĵ�S�$8ʬH���$8�n60����ܼ����o��E�j㬒F[��%���� ��?�1K�[%�4�%���h�4�3aC#>?�S8�[�A'�;E�B6�e�XTZe-ס��g(A]����^�-���(��p�\��������B4��ciFv!4xe�T+8x�8l��d��R���kMbrH�%c<S��,X:�dy���CN֊��|���������e���p7��I��,����iGi��Zj�a[�p�n�g5��"m����h$��N(�&�KHJ��P�X�K����q�EϱC]��%x|=�l����뱌6j��6�<�UA�uZ�-� ��+�/��I�m�j��JWQ�]2\2�S�t��)22�|��5���ƨ�͛�Ј��,/b�_摁-�]J���� �ڶ�V2�"Y�����.��%C�s�LE7RdB+�ȓ ���h�i�W�˛ҠN,�V+���~�]h��2)n���@���={P�����q1����'f;���2� �e�HIŘ��Sv� 4�P^�S�3�V����ewn�1�+L	�N��,�3��w;`�
��0�5�K�OMvI�<��S�zf�i��p6��� �%���i�nţ>	#��m��&(�P�d١�|sq��N�_���۪�;�a0;�$G��CZ�}���BmN��R����j���)���2�&ް~�B��G�B�q4eR]`�Wk2�r������u�|*'h�VunVB�@�q���7^_�΋V�>���jw���!fX/�������h�'�s���B�HY���Ql(�|��x��u����oV�y���MY��w�q�	����������_�:m�L�L/+�["2�N���b>����H���Y�&UC-XtN���F|&�j�;р�/LF��?O��	 /oB��e�ʼ3�m����z�X7�u�'��1���R����6��7�	^�ϖ@+��2-h^	t�W����a(�|S�F���i���*��ŋ���&kZǄx~$��E�^��]�^�l��5G� A�F���?Dɽ�ѧ�q%f�Oa��3O��YI^g�>X�Nb3�{P#��W�X7�Y-�^H&c������J���P�pL5���H`�EY�{-���V�C��Z���KȦ����,��*�F��К@��� d���M����#L�}dz~9�}�R�e�-ؘ�GK!f{jx��s扪A��� +TG��?�O�V6�?|��ot�_CZ��lFu.{���*�]=�qL�_>�p�V�?����QͷX�¤S����U���g�H[aB���"�9�G��t	�L�q���GVG��+D�}7�.���;�� ����E�l2�2l#NaO�Ǭ�G�M���-lw巇��~�z�g�"�T|�|+���5Db0�z�U���-i0����x�v�x���"�HB�����NhWJ�ErOA�����t�����:�y��q<�.7�����0��#�{�k�ƶ�Jr\/�Q���S���x(HpR3�O�2��t^J�1��z-wv0��������������hw��p���'7�Z�:X�=�!(
,��|��d��c9�:f�ym;Vx�J)�]����z��d����@r�c�'��*���}���]3TCE�R�&lg�Zr�b��Y��}P%R;s�$v"���pNL���`�tهbm�y�c}��/��T�� ʁ�e_7L��8�~Q�Bޑ��?i��R\Au��j����߆�#�V�A�D��Z������&![g0������ �]p�'���
�X���Xx�bXD�%9�� �zx��ɾ��b�CH��6�C��::��㐄�|��C�����CuPM[�s+L&p�����C>8h ��q�}�gz�߬��&����~o ��s�����Hz4n�*U�[6)��$ͮO�0:&RL���8��5Ι(��4�I�{�\p��7o�o���k4�}�{xߌw�ƛ�;��MjIn�@�ʑ�N�T!���Q���}�
�d�c,��6�Ex-2%�v=��;��}�|��PV�STL{uT���j�UK�n���Prv��,A)ô�����Cf���x8rs����e�^���὆�P�s{���Ik�Aq�3��T�<�Gr�H���Ta�v2X�5�oޠL�C�S�X�I�<h�iw������?�N�t�%������؍�&X�%�/:xo .��������,��{>��~l��(�l���M�������w���Ỏظj{���o呥f�o.�QPv�0ֱ(��;�u�{ݝH�sS9t��P�6͟?���17??��#c����JO�K([˥O��j!!�ڷ"gtR�Nh���EǜNH���p�����$ YӼ���� 4����}h���!���:'8��� i��\D�XY��©@�q��OX���#�vl�.G��t<~�5B�g{���g`�X��EK�\��b�	���?i��I�\�{҇�G,%�9�������'4��#���&��.�nF�\�#�X�OR$>��(�3���[J��~l����P�Q��_�����.�xR�v[#��!��?�'��ޫ���]���Y2��ʑ�	�5����x�ϻ�ٳg�k��      �   �   x���[�� E��S�?6��+��5��)�L51F�o\�n)a��?���Op54&O�ܖ!���+N����8��]Ger;!s�M���6�-���uU�$�@dV�%�@&D���^x>0�\#)��˝�Er5_?����ȓ� �w��]����9�FAE�@ak���ֽ�vt�~}}���~�6G�����C���7�rt���ۉ� Ϳ��FD?DQ�M      |   �  x��X�n�F]�_1�3�΃䮱
�)b�-��ȖP�Le�E��� -�ͪh��A�M��������%Eу��$��9�y���&�L���'�pR��ꨬ�g�I��qbb�\	%Zk�/�u�d�M<=�3�2�u�W��P��j0ǎ��h���x����Щ���]�އwչzb���j2�z[)�(��Z����-�-�!]����Q=�(i�������pA!͉�y��p( �=ɭ�����z�]�M�3τL�9P9B�2�����xO�w{k�D��|�_��~<�W6lH�-��X ApP^��Q5,�/K��$w��mЋ@�2�0��>�\n�恦�@M����R_O��d����`S�,�1k�T-ԾG�$'��R�	��z�i%���}�_�syɝ�`k
��9ϑ�Nw$r:�?}L>;"'`�����:�CN�Pm�q�D*�;���>�g+I��x9��Kt��˻4�J̼�sH��% ��RmO��/�)^��,uV�2U(t(ا�e6OO��x�ĳ����˳��+,���e�y���E�S�8o 	�$�Y���@��A�
�y<�[,<�#r	e��K����Δ��Ǽ���H��I�s� ��#�zg�uت��d�q�w��4|��}IL��J�ba�Za��m� Xi$�q
)�tW''8/]��3�]M'
����٨Y1���BC�͛L��]F��;�����	�B3���9��,�8�iKg�5���<��,��+ȏ������u��8����1�P3���}K��(�Ӧ�{�iq�4bϑ�����y�y�\`��Ȣq��P�ԓ���b�R=��M�~]�����ƒ��	�cO�ai
���UG7��?O��8��^���;��-r���S"z��*�uv>���	��Ug�j8���~7����h<��ŧ��%�B@�d7����?��~�jz��|�?@�2�i��l랿�	tyө#�z�s&2�z��_H������8�����G�a@c#ɂ�Ic3�r
�qQ䢻x�B�,5VN)|���x7�nO��¬i�x4���R�x�y����M��N��x�g3��9�N�����4:�\]�` ���m�S���;a#<u· �����fۼ6q��,۬�`��ռ6M��~�������^���FzY�짩�X��a� 5�V-���-@h�����C\��c�yL��6�� e�ih��9��س��7�<ݰd�z���e���jo:vY������"� ~`]#񃓭1~h>��)ȱG�أS����'"-2�z�P=,��hr�ՙ�{
�Whs �|y_HFJ3i��ú~A�Z�[�0�x��{?�@,X�&3�^|?�[�=t	Ν�1Zw��,u���чwޠG��r���o�>�]H��-��B;�]n��S�H���h���g����{gC~      y      x��[�%7�E�3GQ(C)R� z�уA}��{���HWU�B��:�|��9!r�\[��k־�o�[+U�\��M�T��?��d��:���/�~��_KyC��3�
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
?���c�k�����o�N+?z��hv��~�q/�O��w�H����~-f+'�h�R���7��mjd�I�%��W�� �p��T�I��Cj����1�e�걿ުv&X�w�A�ڡ�2mR8��R6���t�&����Z+�ؘ��XB]O��|mq��/I�9��~�m�G:gRn��\�7��'5�Z�i_�����*r^��Q�s�2%iS�(׾�k�6�k�Z��ff �o��E{xW��=��g�K�Aa��r���	�J��QVQ�Jn�L���-��7�6? �n� ������y����9H��b��W�<�5��v�� �  /���\�/.��pwn�"L�vj� x>���\kW{����G/�����et��-�u_�������?�^���>k�;fz)s��;%�6s�>�j�z���|�狺��������iZ�oɁ��t��g�I��象cmk�Jbj��=�0��~r��v���Es��⹊n���t��ec�b�/�O�%�;�ו+���܎���p5�H*(��{CH1:�-%Sq��r�w/�.�*H�a�1�c�]�?L�ӎh1k$BU�AL�6,<�×��"��ՒoSx��������pԔ{�����^�H��Z�t��cn�tALzvK�v T�^O����A�cpb���QF�=55�	������V>Z�/i�s|����~|���߿�g�_$      }      x������ � �      ~      x������ � �      �      x������ � �      �   �  x��U;N�0��S����cZ(�8�6��@P�d�A�D�-�{�N�Mg��x
��f�=O��A]Ķ��7�5���u
�_􁣻>�_��G��T�B���{R���*���������j���%#LD�ȗ � lTh1�O\_;ds�w1�\�����#`9:��S�Q-u�c�i�7�`ji����P�Dģ��:wFw�4��!5�2�{ş��r�1����6YqQ��4h;U��g�d^�9Y��X@��X�y��v��J6W�t�f{��n����|L�����q}ф���y�a�I�����'�x��ښ��sk���j��N�2ERc��K%��|�~�Q��2�|�"���#�QZ?�:J��K������+��      �   
  x����m!�3SE��O�N�`/�@�_�f��l�K$N ���߂H��������I __�uߣ��.pp�6:��T��4����["A�5-�z�2�e�oI�/�+9k-\�֢�]
xt�Vƈ�6�S!^5%f(�S���E�(:E�)J�����2�)�OQ��A�)ʙJy������_yC��T#��b[h�%od7����̩�Su��۷sٸpk�xݫ�^��>W��{�e�Sqd��\/˲| h�Z      �   �   x���M
�0�ur�^ A�����cl6-�x+��P���6�A;�[@BW'Lq)\az;k<�]��`�A��p�Y�����7���l�˩�8sv�y�mOC��5`ߗn<{^����鋉ճ�Z?0=P      �      x������ � �      �      x������ � �      �   �  x��TKn9\�O��x_��w�@$[o��d`�q��\^�-y���PKbA�z�*�@,L�>���gP_	�����|ۗجeL���}@IE��0ږ�)EjҢ�e�V�U��-F�r�ڿ�yX�_����~?�|�-��8���$v����ù9 �(�!q�����1Y5�z	��i&I*"� ��m�E#/E�pU�`��m]�%��$ �j��&������jI* �MĻ�lD׀��D����VQ�F̺�(��,ZbE�P�p�_(�] ������T�PU�70��W ���O�sRf`=�ݲ]b��7��ԋ+�6��ܒ��?}	㌶�ʳ8+�Ԭ���`�['�+������ �čڈb��Z�#<�ʽ\:�ӯ_����w�'q�7�qq�֌�6;٣˥�E��n)��ܷ*bQ���C��@��s��������9���*���Z���Y��VJ������F����J��A/X|��Sr~N�� �,1����%V�/G� �y�`Bw_�� ��Q�4�0��1T���(��XWv�-h������/�G{���g������g�� ��~��^A��g��x�*�.jW���+��
 ͥEk�]�6��+F26:�����Y<��ϓ����n܂Δ����)ֽ�!Kyg߼
Sq��T�{�\Bۭ�o�F��V�ue+ۇ-����w�q���4M�-!��      �   2  x�͎͗#5��=O��[U.�e��F!�^���D;� mFh���qa���i��a�E�&T�kf3����vw�Įv��K���-`pX4V��l�kyQ�����b�S��y��l��y��.VW/f�z����v~־�a��G����կ�v�z�G{�x*�6_Ng��T9�ȹ��@2`�=�wvv�����α���ɜ�w���+�"�h�V�5�dM�>4ۣ/6x3oST��W}�ʖ�j��5р��*����	p��0+�B��Z����7׶[ʳ���͵��ASe�D�h	i
�&�{���<������͘|��3"&��dU
de,��*W�ن>�K;F#Ϡ
c�X��f�z��S��9_�Ϧ��r@
݄t'����"���������"4�������.�~�ڭz���ƭ!��7ľ(c\U1Ĭ�jQ����]�33�D��ک��^���p��s+�Ĉ[52�Gv��q�F�:<�R}V�>Ƣ`��V����W�$LA�b��R� �Q`z"����6(nb��"j��Ы�BO���Cdp l�H�<g�������m�~!�.VW�̧�E�}�N�l��������.���ċ�Dx?42d1R%�$�ZI�jI�7�����&Q-*�h�A]��V�L�xL^�0��-���Q��zc��h?A��CA�{h�bQ�hmZ|�����Ӳ ���`�Z/QfP�-�I
z4Q��=�C�
�
)RU^�@BqڹR(�>FD�Qs�e9��6�(z����
#���^�ḧ́ճ5���/��(�_���Ӭ��|K �wY���#�;�n3��ٱ��f3�Li�]�O*�]Nӽ����H����l�v���S@�;nh e�ba��Je+cP�0��1�5��JE7A߉���Y�s ֺ�ꓠ0���}��=ðc���`=n%.+�%��!��l�B���,u^���P��Ÿ�t=��%k١�C7D������V��u�u�yhX_����7kZ�a�aL��@�H�2���d���NypAY
\#��'�剘�H����1�x��Ğt'''a�t�      �   U   x�5���0�3���'��������,�"�����r��D�����ʑ�Q��������Ş��M�q����^���f�i�      �   �   x���K�0��]�lǟp���uP(��4�a�G]���!���$2�x����xZC�_�Pm7u���Z@e��-S�w�w�I�8�~��n�$L݃�����v�� ���8_�$�I9L�&�h���܄	E�c�>H���cx��ቃ�)!��蔔P��9vb�ɟ�
N� M��0<Ǡ��=�R���!      �   c  x����n�0���S�&�/��q$8�b'v������g�VK���H�[�_&�
��ss_zw����	:�X�X�<�:�Y�,:�A��]�o�\rγ��kx�ΏO۩Ņ����KC�a�eŲJ��f�����QQ�]=��Q�&�}�f��ן�i{9ngǰcw.]�|Õ�(�>�$1����\mJ���u|衯l�5f4nSWA굗%�"T^��?�(��R`�Q���q�d��M}���a�h&�on���+��{�0����q�k֐���d���܇Oݯ��z�q^����/֐��!ՙm2�1+�A���������"��t<����;�V{�1	׼x���z�'߁>L���	Ӗ�      �      x������ � �      �   =   x�35�45��".SN 2204�50�5�T00�#��\��Ԕ��:9����� �5�      �      x����NC1�眧���#;�s9+K�]r�,e�o����B��������I�E1���_�"*T�g}|�O������vһsS�
���-�[����ӘY>c��g-��ٲco��f*�(=C�N�C*��v��$c�S���l۪�,��j�d3p���Sf�[��z���gDџ����wZ���h��� ���xE�Ҩx�$����_,K�+d��0D��$�T�`[	�
1��K팍\�?	|쐜���P1@f�*�PR��4MoG      �   {   x�5�1
�0k�/
�%��'�^��
6�>������2,,K��!�5��vv�A�1K�YMbjq�'��	�Z&k�{&�|�{�2�"ږ����t�+Q���hOi���"����"��LǖR���#=      �      x������ � �      �   �   x�u�1�0 ��yE?�Ȏ�8�/�Y�$�@����b�����M���ʊ�@���F�� s�D� ��@,H�'�9�9�א�(\�����5�~?��cl;��rKm��j��H0/]2�U�|egt�?Y�ټ�h)�ɮ�9��5      �   �   x�uιq1�X[��x����du�_��3v� �㲹��4d�������iUd5-�pa�+s�a(�|��J���O+�I���\o��f����h.s�W7u�E�oN�:�%Ez��O���ҵ���sm�-f��~�>���T>��G�2rԱZ#�z¸�(e��\�Y�������@�      �   �  x����r0E�ί0bd�%/����@�itэ�	yѐ ��qvYv��X�s}M�PqTl �9E��5m61��-9#X{#
���X�CI1�H,h6����~��j}����la��v�N�q�?�ܘ�{��1!o�b�T���1DZ<�K]��TJʬP�6�J�*Rzel����yW�p���Y�u�uv�O����Z3F�D���O�!)�b���"9 �U`;����b9z�fy:�*{��5?�����>��eG�-�\�FCq�jA�X��� �޵*�U��������"�n�ֻ�u��^������J%C+*�FˠՎ*؄|(��4����F��\�r��'hIZ7���i0=U�Z=�8��aT� {��r�6fC=u���~�D*c���s���9.ee��?�?ݯ���v��n��%_�O&����|      �   5  x���;��0��>� �%�%R�R�il=.����ֻ���1�R��)��?6���XZ+����������0\�5U�5	yOn0���w:���sOs�b�����,���fêj7��-���:�@�tCJ�
>��+�gP�`�b�B7�8��G]�$�G:,�d���������BmT3 �;(��+ɂ#�`�p��K���s�2Td}{J.�K�?KF+_�s�3"�W�P7),�>�Bc��L*���l��#��H�DIE�S�s�l|��Y��8����?�f�X�^7�2�Ē ��4���]�ɽyx;���߳�����,e�e�:���U�lJ�c�����)�j:��$�crpO�)_y����������ɍ_W�:Y޾w8�FL[�\���ova-�P�c�g3V]�/�ȟ��R��A�Q1f�7����ڶ��5�;*�V�Z��^���Pb@�Ǻb�9V1���5)�8w��rP��!z������+D��L����B:)|��w�vq�;HrM	�I��T�{��t���X�(3�>�B�y�v̵�����ŭ}K      �      x��[Ɏ$9r=G�~�͌4�Gh�U���	袿׳س*2坍���%��fo!��4�ɪ�����y�����He��IO�Q��ʕ�I��s���Eb�\/_R$QC������Yk-VO�����Nm[��\�8U�;��ъA�ph�s�lc�4J�|z~���_�r��ӿ���O���'��S�]��x�eΡ���uיn#�ȅ��(~p��rVܢ�_G@j3g�fnI�]CKZe�Um�##�S�6�u���w=���JʽI���O��>R>[Vʿ� Ǭ+��H)>8�>W���V�rdz��^i�Nq�UB5�0m�
5nM#�*�d���z��G��!|��Su��~�
�u�a�Z
�â4v�mu����+���7��0��|.5�p�S'�JA�/
��R�[/�M;}���rq�Yi�޶�Ӟ(�U�<�k��OT��T��{@��c�w	����)X�1�5����{<�~���nW�����Ě�˜���D��O��Ød��T�����w��v�������`Kݡ[Y;Qh�����YSX܀�W���K��;V{����Đz@oyF�5�d~���1K�%��#�~:����
G����&�ϸC�����1�`�PS.���JK-�r��!��~����9�+ӳM�Z1�L!R�AP�E2��{��d־� � �o �[�f1����")�P�Nyfsu�hfj��G���Ǝk<���fpŌ01hGd�'�T��r��/��>�(�L�wK� ��W
�Al�fh����?g9"�%�`�8_�_/�e�
jƍY�V��3�FZ�a�7P�ܻ4@�2V�ӑ�/?���m��Pd�N?E��!�]k��:��̐M������@�Ӗ�jZ�V�8HD�=�i<�����U ���y��W���C��Qrz
��1��
,����m��qn��4@�A,�k/�!02�>-]��A�x]��}"�L9]��v}�x���PM��O+C��S����Ѽk���[��sq�^O-���^.|�����H]G&����&�+�7+C�f�^�GJg��'��
������	����*��mhT�H�y�ہl��MQ㐥O�|Y�=B��:kÊ�d���3'�{�>���J�M6qc-�.!g��Ǎ�'P�!�`�Y[�J�u ����d{z������ǥP[k�L{�a_��n���o����L1G��	̬�a�w*RY�̐�͒e�]5t��]� /��;��*��%IG�i1�vU��O�_�~:BG~	qKG�FWM�34� "�)��5tΥv��"�/x�K����"7��4��8b�C'2��^�we�D�-v� �`9����3/Yu%�)�Ҫ�!���	�Xk��w���r� Rf�1z�й}�yy��f�LZ��C�ώ7ž�L�\A�`��c~���+�U��⦠�`^������v9� �/hï����V:���A�{Y-�M��i�Фt�����x���Nmg^s y_��_3�y�H^,����S=X/,���9q��t4�x�����W
*$2䎢�����1U>�U}J)th5�f� ~!���4�� З ~�+��]r*73�p4Z�K��9esN�+���;O`P�t�#��'�����ei�3��ӊ�Q����W(���kL�Mp�!��z*;�.��k� ��r��PR{YIO��B5���~��h��Ŋ���`~�VS��歉���l[��(&� :2_L���%��ә�py��S�T�CW&8wȢ�1Qf��v��z���<d��>R�߁�3�����3�U�z��MM:��'��L	��g��|8��
�6�s�*ld���C��� �K�ܮ�5�� �$��o��_������uY�����@�A⠀��)襉2������О��d��#��l���r<N�����ǘ�Kj�f��3�1xꡏCSm��L�����"u���Ƃ=l���1�)LY�`��e5����}�QP���	��f����3"����{̺�`�CϾƑ9���dV@��x�߮�i��{պ���e�t;�5�7���J<�)��J�}��n[7F�`����]�o��5c'i.�H����5�FM�#�)x�`+����m�|�F�`$�S�ib�)���mE�v�)з^c���V�SrO~u��o�[as;�u��VA*���SX+�!0<q3�#2��i�,?�>��W�z�v�����r�#�¾0	k��3"��G,pfth�#L�[���"��Q��3Zn�+�J,��B�bk�������D*�@=�2O���k��hт�cvI3���d��)7��9����c��pE_?��9���>�C7�Ų��t��"af��t$�,'��uȪf؋�՗ibA�gO��5�n�K� 2��ZUo����X�e�sRǀ{ߠi�{kDy���&��zg�9�(!'�š7�!��覆l���jz�4/�9}H:�%�ˬ��V�,9�a�����u�i����;� ����/�+Jn��R�y)��i��)�U@[#���ȾSb_Z7�k.���D03r��%OX��U�N�U���z������W��B���@_�5x����tc����(Z[�6+u̧=Lf[*�^}Z�����E㞭�'�,�	��ʔ�}�4�-?>+h�⁞�7�)��p;�xP^$�2a���>�����*Ҁ�.��TcF���&s��D_]i���7��諯��+Ѥ�!QK%$6Q�����k��Ҧ��zz�a���n�G���ej�mp�6��y��B�����~��|�R�\|��>�/�c}HĀ?�`m�e.�+5���,z�����^���Xk�D`J0�@���1��O���;_��&�/�'BM���}��<R7�j�����p~�mp-�0-wKՏLU`�����i<�'��z�������>������:"+�o<M���ŷrFm2�I�#�j=r�k�^`�1P[�Ȅ�µ��:?�'���O�\a����A����\��Y���oŁ����0(}r��~��=�xh�Q�f�d��� ��J���G;��nЃ�`Ϩ^>Bn��{���Hz����L}F8ٲ1מ�KH�����"u�#�/�'}_Vn�17�^���a|��B��ƿqzo7@G�*D;�6���'��D,Y</'E|0�k����l��tf$^4���-�w�+���i��3N�ݏ� *�����]��U��%XX��C\�~�o|R��h�A���7��_SB�D�r�훯�^�����cܥ�ub�v/�>��'�k����z$k$��"����:��NqU[����>Xt�?�y��?R��s�����,�QI-L%wT�Oe��,����[�gyN
��H�.��Sa�Z��~xm$��F��~�ޯ��_��bxC�?��Ihs�E�Qc(EC�.04+��=����>�z�B�,w|V�Z�P.T��ժp{b�����%��|���%e�����(���,?4(�k��wJ��?W��#e��j*���ʵyd%$]���X�=E_�9G
&����Jɯ���-P�\�e��lR
Z֚x~߬wK�;����3�X�}� wh�0Ǟm��c�#�#Ę�{~�%zv�������§@��@f�޴��gG��K�;�z�G���?C�R�nA��~�sa�S}Q)�T$'����A�;�9Y�|�ԁ�"(uW�\���ON��)�]"�:o��/��ӻ��{��.nn����ўU%�v����\bh��l���#�s��M�������Z���AI�{�]Lp�e
�S��t�y�^��v$(�T� ]f$�WmZ˒_��e��v��W��g������}"��(��*���?2��tl�L���5C�T�i����MC�N]��|mY�Uw�x�Wr�D� 砆����zX���%��)_�+v1&�'��;�I�G�|j��;6qů}{.�R�'���m���t�y�~�C���Ю#���[�������Oo�g?��� k  Yv�9a�4����j��s���$ƼZZ��CzYt�r�,�	��
l^G�y*<�[�çXcvc�y��}��1 V��[I�M�� �˽��6�FF�o0P ��z��~� �������`���[VV`�y�`N2 ]0#�/+�
�ʑ����3�w�k��6ĸOk��"Bq�h��d݋��H�7�pp�g�0g��;ȏ��B27�D��E���� J>);^;�|>�~`
�GQ��/�ζ=^Y����FYC��4!���̈6�&���!#©�oG����CП��~�
9_޵��}]�P�p�޸y�Guv��K���9��/�1"Z�����_��Z����Bn�nd�jQ_����H厳��`Ȓ�p���Sf˻tD�#��Tfn�#$��+���0��@el���&Ps�p��y�O�W}�~�#�3�wf�3N���3_��(e_(0���Yx��췳�DO��<,B�P���N�@�C�+ s�a��Ug���K��KYO�����&ksg� R�^$�]ڄ׿���{BO�_����?����aJ%ٗ���	��c��\z�G�>[��ऋ�3|RF�AD��H���\"p��|�/_�].�A/���^pC��to�%'7����.��n:��s��ܿ~�n�"ʼ�x�: 8a{�G
`B��ĩ����jMF���.�(	'r	�H���:���xT���=��U�r�w�e�׵�������� 9<���6��rHAQL�z�Fk��������&� p��%�����h��X�ݐh�^,<w�� !3���gJ�[1p��CN����}��'�wث��+���@MǨ��֖�k ��c�NqV��C�i�Np�;� �4Q2��k���_�3@����Z4�۱�1G��.�B��"m�K�
AMt��g�}p�v)�<D�%�v�(�Z��	�Uj�D��s˓��>�7C4��t9�}�8�\^W�j\��a~??6��6r��\y���D�9�z�
�mX����Kd��е1'��������/૿~�`���d�6h���Z~Lf�;���bP	��ߊ]N]8孡��G����1�f؎5��(vh��m��&���T�,d�e�#Kj�-�u.���7_�$HS���xI4E�J����U@	#�|�9�W�D�I��wR�q�}��t)��ZT.oؼ�h�U���V�=K_���o�<��4�{V�xÞO}4�4(�A|�Z��Ș�ߜ��uy�~��_Q��q����Q��p'͹yMb�tRi-������.0�p�qv���5L�Z~���(���g�@$�InoU�}W#v;�9~�ژ����0!C��6�E�~�}�ac&�6��z��?��ɕ      �   �  x����n1���S�	�Dj�Э��)K���פ1j���2}'K�H��>��I_�����{i���έ-�����D.W�����*�����˟/O/o���.����r"�n�������������]Ow]��
�
�@��
I�
�qG=�(X��` ��tY;�į���n�l�=Ǩ6�TA�˸r.��B�T��ea��1βKa���N���`^X����:.Џ
r�}����˯����~�����E0��+A}��A�g�@x�QH��n�=���`�dx����)aQ|�U6�⛕�3�Tf6C�Y�*|M��2����1����l����i�z2���+'���<��ڮ�z�.��XD��9n�W�;P�D�	�X&�+1x=�y���p�}E�p6n���U6�3iN|��rG�8�wj���h�����Ke���YtS<J,�)�WX~2tF��,w;�qT�^=	eҶŞB&!d2����E�$ƌ��&!h�g�"f�y��'�	Q�d/!l{�5�ל<*Bܴ�2!�5�z������\#�u�P%�$3ԩ҇aNus��7����˗�4v��ɠM+-0���}Cu�j��b��][��7b�<�����L6�k{N���G�|Q؋�N���0���vq	
{cӘ%$�+�0N`z�1+��⤣�0�Pu�=`��8�<`a,;X(�mÕ�0��,�v(;;�
gqؑl8|>�N���*��      �   �  x���;��0�Z��"�g�&� ������/�뙝�tNG�>��E��ik�
Β�1F&�7�����¦���:�xIWƒ���3ҭ��}�� j"�r)���ZDvalf��L,-��{-����*�wem?��3��~K�r��ɫ|(���h,�����1�P,�b��z\z+��ͼ�ʹ���*�Q�]A@e���*����c�J���0ߙ�Z8%���YGVeȤ�2_㳗2�����P��j �2(�H�g��Q-Z�9�g�I����Q$a4�lEC�����%�6D@�ԁ�㞅l��f���(G~#��j���^�{�o�?�l1�5̪�1�Mf�OfmͲ��uj+�����%����/&����3z"y���<{{��aU���m؍����F�;����,�;�N�%         S  x�m�K��EǮ�P`0�r^AO�5�������/�~�I� �M2��R�����S4�D��h�-�������Qc�C�z����C��!�d�W�������<����A����[�]F�Vr�m�0Ǫ1�#�
h�M%����u����UIk��8R�G$�ZK�s�\,�����R���M�ן����R�Y�	�g��B]�y�W��o�����Z��M�/;�u6�~��+����3Ԗfw�i��[���вDɥ,�3�/�:b�[V�]RȳGn:5��K���2A�O4�k+Y*M��6�b5��h�z5����+UfZ�߳��Z4�Uu��B�6��r�1�Z���ڥ�ֳ���h%K����-�v:��'�^��[�}��\��	���e�r�����Uc	c{��l�v8*֪��6@+?���ȝ��~��w���	N9�i(>����7i���RK�xD�x��>�.X�i5�&l!�zO������@ӈ�f\{>|�C�)�j�g��2�Ѻk�r��h�ڱ�:I/���8}V����#��G����.�@{��E��C`���i);U�aI��WuN9F�L��`�]�R��µ����ik��JۥK��ߦ��8V(��dҖ����I)B.���wW�u��+xVY�IY�S~P���*U�V�s6�>�ր�;g;�m�:�i�'��>� �唽1��+rJ���[a�v,�Z�0n{o���cM�qh/���2{
�Y@n.����\gr���S�
I�s��A�2�)�^C���!���^���}j�c�����QV5k^L�Y�E�B� �'>n�������t��;�<z��x0;x/�
�.���mx��O-hք���Zi�M��Pv����;����E����;{�{d�>a�3�n�ɭx[�W���\�}:���;{e���^�8�S���� +_-�đbC�x�BY��^��BI"�6>ZhY���;7�@
�2��;��w�r��k�g�W���i^�]�m3�Q['Oy��4�W���K6�`�������ڢ���{�T���:����w�:gkh��U�<�k� ��S�.t��H}�0Z��w��v�O��FH?V>n���[t�e����sh�[ �X�y3̏��[1o�e�Mv�*U�ͥ��].wK�+�'�G��j0���f�y]9P`�sF�y��E�l���کQ��M�L�(ZX��>�,]�"�����A
⫏��Ά����m-L�ļ`-K�[�]Y�h
��p�GYT�Q�{�ܝ���70:��ޕZ2�D �}PF�h�Ɨ��j�N�Y7�X
sK�;�ۀ���xt����ҙ��9�d�7&Z��>�ڻ��pt�S����]�x����H�&!�ҮdR^��+4�WCǨ��������u�Bi��}���f����ʺh�$Ȕ(B{o�A,�ﴪFnJNwM��0}�ڻ����w#�!?g���V�d��37CJ���I���k�MВ�i�������X�Q�o�ޛ�����D<�V?����q{|T_#c|/�#����e�XSu� �}�}��A)x��4���;���i vF�ҍ��澒�4�?7�g���{O.��y@G��s�է_���M�`�V/ԴǑ�X3n��F��]s�yֺ�Mߴ@P��W�l�����T��ՆN#��o�#Tm��:u�'�W������Fz��2ݹ�{�x;R��P����&h�4��~6���zU,h��i�{��'Ǫ2U��'��3��͒G�y���Nʫ5��u�L��8�E��r}$�u��C ?g�:��ׇ�E9���M����'�B�<���|��Vs`/�<m�8y��7�`{����____���      �      x���Krd9rE�Q��P8�ޛ�@SM�dRw��{'3"X� ���Hf&��t\���n1��J�.��\��ؚ��%��~vX�}�R�^�C+�������� >D����/�������o��-�H3l��1]��^�r[�1��ù�ۿ~X�Ň?B��q&���1+Eu���B�=�]cx�3�}��WzԘ~�1ŹK:�Oե�@�ǭ�Ә������g�t#�M/59m���N�)��{zԘ4�[K�.�)CR�����CW'�c�[}�Rf�%i1k��ϖ]�r��'���,m���Z��J��͐��8��Y|���Tq����Z�M/���o_|��o���L�P��jb��n�I�#3��we֟�<����n�9.`����؏u�"W��7�JZK�>���F��͕��y�I�B�'�֨��<Nu^����3���K ���5&��=e'�%U)`g6�r讵��GB��5���5~����?��?���}�1B^q���rI2�|��>�
�v����vF��t�J�!7���<�6���ۜ�IF"z���l���`�%_�6j�����W�l�gi�N���a�)u��3�jI缗Y}����	�"�~1.m� �v7�����Q]J˗Z	K�US�՝��6��j���Ok�.Hy<�G|�����Jngz�>��.h�K։L���L���~�;��Hs��}��Fm�{I�2�׬ɹ}_f:=h��=�d��p-��=4�	O%z����+��~J��Ic���P-���#i�'=��M�˛�2��gtԛ ia~Bg��D5�P��[��X)30�C������lx�%?k,h�s���]&�S���Dא"������Rv������Tos��Ζ��h����Y[Ra�G�ѧ����2Wy�\Io����{ě2��re���)��b@���㩸�RE���g��7�}±���ήhy�5��VVF �^��y�͘��Q��I�jr����9j�^�~Ԙ�/שh`i �V#�\�5M}r���K�-��<~/<����fqt���X��	E>�[H%�DE���c���\v�V����(d�)�53d�^Tz_�cC����~Ά�r|ohgl\ަMM������| �����!#��-q��������Q�op{����%m��L�hs*a]��1���P�AsfOf��3���ԩ�P����+�|��!ִ����9V��罹X+�r��v�	B�΀5���w���<iI��3c����9�2��j�����g�D�ϣ���(ğ��϶�lL�F�ɍ�Fǿ���I:{}��>�Ʊ�����o4#@ui�:N��2����";�ͯB�H(�Y�y���e��ST���G7���k�7:��)L?�
5\qo)�:����jtԷw�!ϐq�}��>�0��l�+3L�E!<� �r�4	7��ߔ��%1O�!��X�,����t�+=p�������{��x�`��,��hdr/򵯡m�$�p�>saT�H8����2\e=���o�1�9z�6K�		����,T\����+�q�u��HSi!���[a��W����j�?���Vf��b�_{�������9؃�|�m�1��<mF�����=?�Uƛ.��:<��v�j,�dT��^�?���g��j�����CC��b���L�b�j�#u�hPa��_q�)~�aO��̿�������a�50����äS���|l?~�|x|�}�d؃� E<�D�UFǀ�h:Ky�:^<�G�Mn�{d���8�&%|�4U�ݧ��7d� /J�z?�LU�J��+5��*V����RK���&Yvj��Ʃn�I�.�]��%�",�z�F�͏�:�a�R0����C��3ʹ°�X�M�1`��l����5nQ)eݙ�1{_{�O�{��a����fM��#\eu�����c�Q�3[Z�����`�?㩙��Ǽo�G���9�+Ub����F�����Q��;j�mV���D��9��ǢIA�U��"vn�Y��R$�䈹
��Gk�B�;��T�u��e4��fs�8߆����$�Jw�ug���:B��ȓ�3A~�s���Z�	����M�N�BB�;3�LҲ�l'p)=lԅk{���S`������驡��x�e��IY_36�C�������X*��lZ��V#163����F~��ږn�G�@P�8�Zp���܄���Y��I%�Љ|����s,ϰt��+��Q�+G�5[NI}Nqac̓g��*$(� N2?�:���[C������w��`���l"Dt��M
��73��ǣBշ'���]���(�t+�Z���Q�����N�������iҌ�����)yљ�+<�醂G-�5�iB������u�� �{�P3Fz�
�.a{I����sz���c������;֝P]�K>���3ٝ>1�m/��_H�>2�/zRm���[��ČP�ZX�Ah�}��W����������Bƻ�%�e���2�']R��²�x���~�L�
���sB��ĢK�o���eZ7�m8l��C)%����'F৮��<�}/8�>ع�卷ȕw!��W'�?c��[,l(���4d#/�͎	�5��M��>�+�����QsDA,bT���J1����1q����$El�_�v�,��߫d׿9�AO~��5�6�S�������GkLȕMg�|V���
�'�î��p{��1k��;3�"�n'����{Y�8p]v`Qi��,kt6��(}z�ݶz8���b7�v�LP�����NN�Q4A��]U�������kB�;v0��ڶ�,+�]|}E�?�os���[NS	����WHE2̷�!�<2��}��������;b!I �kF�䍮�\��i���Ůa��<k��Z��̖�
��SaV��t�OO�.�d�)�:V�\(�g�emM_�dM�!5?�)�'	�X��W^6��!��/)Se5b]����
�R����I�@�bE�hر �j-X���P���}����y8�C[ڻ�W(��eۡ��CS����22��ps��]�{��^�M�`�O�
�S,B����^}�X�:�t�[�a��Z��������������'�j��\с���"Z���S},S�p7+�I���:q�����ㅼ��*mB>�6�kV%�E�O��f&�v��M��㣭¿��3�IVR�i��"{�d��΁�[%�����>Sz�T�T1D�D��T�fۮ�=�o��4��[�jW��c�c�Ɖ@�:���h�Us[��k����q�4����6>��f��n1p�=�
I��]g�{�*a�{�!N/u:�U�3Wr��F��Z(���X���?\n#��t���Z+��VY�Lz>�?�
Q�KG���$�Jqs����3XVe���t&�n�`�Z:����;0m�ކ�ᜑ�<΄�|}D��Ls�Ω�I�I��F�b����K)O�G
���y{�!"ϫc��XC2f���u3٥��"��Y�_{Y���?��]�K��>���e1�Iޗ\�/_�^:�z��p���+2+L"����Jyv��un��=�W2�`����6��C2�<�����6q�qJ�S�_J��nBD�g���(4r�^W���u酫{�dP��h�n+<�K�]�S}%}�DI��A!��P�$�ۅ���[H~}��ܳ��&��3�_v��"|�.���Fe�G��I��d��؊uI���������u�Ν,�zƊ�p�4-�$@���ֿ���S�s�N����.�J����%C��ߊ�V�;�P"�+S�K`�_V�y��]B�hj�\5�n�ZC��cm�ޖX��_uU���v�ZQ��7��䱟W���v'̓���^C�M���E�Nd��~?����~��/�Sȶk����`G5lnP��Cf|%Q� ��n��g�~��M���b��|˿A�iW<�|z{ �fWPB�	��_��o��L�3��1av�D�Қw<�8%�f*�AQ~�>Y��k��S�>5��I�SM��0��Kkl�x%R�� T�t���������rf#��U���� �   T�r�[����BQT0k�jfm1�<�>�չ$TAn`�)W5�h`o�y� �-a�Ϧfi׫<97dɫX���;)ҝ�a�%��+Y�^8�K��i�/�a/���AHU�v��ϸW	��*_��Z��-v���/��b��U��/
]�)���ҹA�X~�ӾC2paX��L�~�[L���T|EH�,{��e�PS	D�^R}u��~X������������<�      �   g  x�͗��$���9O1/�E\223j�w�wB�M^�� �����T�������*i����8q���l!�&kF1J1-�����'V�Ŧ�>v"��Z+&�{�;o&�j�N��W��f�)|2��Ͽ�pR�t	��v�A�5�EI9�J'��?���y`�@9[��b��@�+V�-6/���S�Mu�(��)��P�4V���\�n�"�R̋Z��A����sȸ�ٝ�_��1��̧��������R4�듑�r��d>���G�����Ҵy�3V��,9:�Ӯ=�F-�~O9���%"���9&�_�k���d��Ԇ2J�m'��G��y������cM�F��c��q�B�K�}��hJ�T �%|�'�u� ��S���d=EG�G1-EC���h3��;Z�he��𾦧?�����T���bi�;c����dh0J��XJ����\Q�ܲJ� E$��@��������km�.+�WC	˭�m�p:$��5��`B�~*��c\
j^r���>�2�q6����m8�1�Э��	�Ԥ�I �@�w_YV��F�z��?~��'H�{���0�Izp%��@T �P5�1����F�k��&���hF��{"�f�+�zj=M�d���� &���&�5	�f�GF?���WO��X9g�0D;�0�6c�60W{�Uo��Ƥ������I	�;�?��|A�Or�a&������1�h%vuT2�Q��P*f�Z'T_�T�ql��21:s'�s��j{U�D��=����M5V?�y��B��c�ܱ��ejk	�y�&ݼE��ff����F��PZ:͏,f���_hg�j��[�g�2�*Q{���H�(˙��尯Z�e��$��;���k�6w�[t�������r�\I)�Ǧ�h_�@���z�-g,�|x�匔��l����4�B�?�zy38J7�,'A�nZ��v\Ba�*sT��� ��<�`%���QG=��+#�N_2bc�5���8山�3��:���B��$�.�n�ʺW[I�ZP����I5�J�#Vl٭��K8wƬp���݂�_�?�뷗�!�tA�k1�Q�y�≘��%����}#m@����ݰm[��Qk�:�wB�u�'q�P��"���;�w�����2ޚ���YH�|3ۄ������vle�H,؈iTCM�!���U���Ǘ�O�G.�1V_��̂u��kI�Fv,�ײ�&_=|/>o�ąbc_:��?��rX�m�"1��/�-��*A���@����x�~�	oW�uv�Q:x���te�iȏ��֊�M�o���䖾�3}�����w����H���Gߺ��M������{�P�1s���^��j�γ튳��+�|<?*�R~���>�1
�}      �   C   x�3���44 =Ne�i�Y�F`�NS(��X��D��R���
�@� ��(rb��b���� g      �   �  x���M��8���Sd?��_I�%�l��,��Gݮ�Ny '@fSv���||d6^*�9����ԡ4��牘$q��tS�jFMt�m�X��%�h�M���?�".�$�B�(oV6�5�3�� :~`���[���C{j��tUP���w���H�Ԧ�� ���+
~#�D7K�d1�|B�?����h"���|�;x�=��R�Y�Peu(�	p���/�<�Q���膲��D�.RS�$*�h0q�p�*�����t�H�(�Q򆴪�|����q5ה�K�V��]4�N��)?P�]�e�dcߴMN��H8��p���_�5�x���N�a�^CíBc��̵�󉓄�(�Mڄ7�k��ى�LC�h�2i��r���]��5�L�k�\N�|���L�&x��b���dO,7M�a�{,��T,��Q�h'Q(2T$,͢�������"xU&�Mⱻ��|.�M�K-M��s��jH<qBɪD�+X�.qb��h���1���l�z$��dUOP�uבj�d�|B�8�'&nd���5�M�k-Y�4B]@qƈ�����jCY>y��,?��v΁�^��
�����"6�,�uD�d�(�t2��Z�V�k���|��t�9��4�c�&���Af���q<h�p���q�1��g��ts��-��8Oٱ�f,�+�{X�����#h+��H��v�&�,�Ŋ�(�X�6�ՕL�������v��</��/����i����jy��D#����ۿ��o�      �   �  x�͗�r�F�k�)� :���ݻe�I��q�q�� �g<J���C�	�޽��o�%)P"i@����s���v�;Vk�AP#34�$0lP���-�	5�cs�����݇zW>�~���?�HwO���ts��y��Ͽ����S�{�y�������<���P�K!�cE��0&�{�����4�.>Y��ߛ����)p�>~����ݝ2|�p�<h�lq��-떹՘Ŭ�~�,�6�b�2Wr'�X{���x>�AN-��v1��� ��5M�$&H':e�R�<�gwԦ��uؖ>� �-�R��6� k3T"����B��9��#��I~	�Q���NߞV�V�,x�!o!o[I���4;j3L�?*�f+r�g���5�jJї�����y6v
Y�T�S���2���,Ɂ"��Ȁ-�vG�7�jo>]o>�q���֡�s�!2�t��a���š#ւ4Cu.Fעҧ�&^�����ϰBj��y�w���N��>�c
9�!X��c��{z�a��Ti�K��hJ��:kg.C��s�ͮ���\��5ŝ�=\�$�l�s`�Y��Vū&5cM��	��Χ�>�dxd%�C6�61�`�[,��tۈ�+mͻd�����5����,�^��$�o���ӱS��x��)x@�	]��3�r�u�Ƞ6B�3�h:�<�&��:��-2�4sGL�oۤ�K��cܼ����|�����˿��o_��77��6�kk#��n��+[�5�P"`�:VjJ��%y�W� ��˥�d�%��11;MU��s#b�C�G�ֆ]	���"$w�C4�0�p�>8�JP�Y� ��)c��~.O2�v!�y��+�'#�Cs�B[�6�
�F
a,Ѕ���E�-+�>��]�J�t������#5|�����D�+�^�5�z{{*�R/"K�d%��a�99Eq�Қ/]��32$��ի���X�Fr�ǳ0�����H|���[�S�$[�4w������I̪?��/�3�Џ���	�����faiw83 ��l�X����&TI2�L���MfD��j&&敨��1N���φ]���gk�7�A"C�jQ7���Ѝ�(��^9�m�ydu�g%Gט'VN�d������Y�Ĉ.��EϨ�%?y�����oi�97<P��ws��y����v���ve�^�-��z���m{uu�?��jd      �   �  x�͗�n#E��'O�PQ�uw�%<@.!�M�j������q�V���E������ӧN�S�M�o֤ga�u�w�S+ȜY����$H*L��ۃ1n�ͱ�?|��F���7A<|�F��/���.��,-� E�`�DL�'$��?<�ƸK��������yf�x]��۽���n�)A<���U���9[n�I������oRb�>bn`:,f�V�5�U�h�^�	y�������-䲅z�I��������
.YR�ԇ�+�Bo��g�*=��e0�I��Z�s)��]UϚ�4��$\��*��/Br�jĭCU�;��U���2[�Ko��|S��uz���\�
,ᗆ�B3[3�+��K�	r�t$LX+�:y�+�H֪�y6E^W�r&���Ͽ�>?������l2+���y`W�>eU�ךk�˭l���V�<��K 񌐈'�cą羴�&��A��z���=�jk{huP�!�U���'�n)&�s��[���
-���<a��Q��u-��H���J]��ܐ��9��"atΣ��I���$�%ﱦx�;v����\�7����ϖ�-rP�t�䀞��
�9Ӭ��S���'n/�|5?K���|��}8��~$�/N��6�xi�f9���7����|ɻW���3�ʢ��Wљ������.�;)Q���V"��*�Ӣ�0xp_|}%��4�ӱEGfE�ww�59�_�~���i{�Ͷ��$�$�Ȟ�tor\S����ټ��czZ9���?�W���'c�n3�Ÿ�RLf�|3�d��ur���{�Sޡ���㜟]/{�@{Wi`�DO�5b���%��̥���KW����ӧGO�^�ύ*nu���~a`��nX5S(\3DT&H����d9��n�sW��p�s�K�&�ێD� l7>g���g��=M�%�΅Ƶ�y1�Qw����www+�      �      x������ � �      �   �  x�����$9D���X�@��X#ւ� 	Y�����N��Q�C�$I���볏MqG��M-���s��V����������ɣ��t������"�O/n6�Ӿ�nJ�I�/�=O�R1p�fF4=�o��V��T�x��c!�s���I�w��U4GktPLw���gnt��S�%/�6��4�mJ��d꘿s���r��]�,�(*iɝC]��\�=M�P�%d�;-�C�2����2?�V}�3
��*�f=M�#��\yn�m�v�����5�I��H�\�s��4.��Ѡ���DF&o�!?�;��Ǘ_n�A�w�	���ì���{���S��>�S��(��ִ������=h΍�{>�p���;a����gU�	����mPv����p��5߹텫nւ�]�B��w��v(F/��R�rcJ%|����aC֊�o������ؒ�	7�O�U�{4q<.�9ۋ��&Ks����ϫ��(��y��W���ɷ�j�ub�����M������g.�
��GR�FZ脟�^����X�ʻ^�Ww�a[���P|ۘ�\�~�y5�s�)gO�tpk��k9ol$����UّHG�ˠ�
�b$캧n"7���7<��
p�;N��0Wd{�r?�9��������?�#f:      �   �  x���˪d7EǾ��`=��4ЃLB�'�-���L��٧/�3����:Uಗ��V[Q�2���|�Q>d3-�$����mβu�ȡ��bQ�M��4�MZ�Y���U&���S�ifW��Z��)6U�*��ӔZ~�S���Oϋ�}��Ag����=��v칒�`��ֻ/|)"i m��Y�Aˍie��̚�������_|�o���=]R�G��g�*w�p�Eʪ��#k|:�vm}�>s��ĤigB��j8���Y7��q<�G��_������M����-��B
���OU	ѳ�0�	e�&�����)����W;�c��L��y����4kwj��ƨ���������]�_�˗?���~�q@��Ciԁ����~�֜7�D$"^	�� �po2��A��d�,��E"#�n��*��x���(  �i;��<��N��F�R�4O;��R{.�r��w�������o���Ni�ڬ�I�Q�Ӎ���Ez�ޮR�D����+�����gi���|��yY33݄R�L����|w�(�i�]���� h�1�}� o}~QWZ�SZ�ޝLm��Pӏo�C���'�\�ayL���O���f�8��Ӓ����cC�JºV�����P.|���@;`�c���K���8�.�*�x.ժ�a%2��Ґ��0-y�p���=�"�G�y�H ������7�o)�`uW��1��M�FuU4����_��P���3�8�#�s_l?��Y�f�Ʋ�x'��<��/�}����ճe~�/{\�CPm|������O�6�������P+qd{���z�V����L}�pހ�:;��X4�@�v�?�4��(�9]��݁_��>}Ё�d鸷0,x\�;d���j<�8���WCfot��Ih�F���^W�����x�o��F<&���]\��彰�O�|5�c���
�Q�{s������X�ܚX�c=��Qk�\Ǡ�c΍�X�B�`��������� ���W      �   �   x�}Ͻ� ��x
_��M�nuhM'�$VC�>~�c1���q�Ȫ�G7�J	��D�V��01�%H�ҺlI*P��J��T$+��P�$k��ߣlH�8��]��U�;ڱO�S�)��p��l����〩(?&�p~�����.�sE�,9k����췵�~����~s5Ռ'����Z�      �   5   x�344�463�4�45��Ri`h�i�ehh�2��RF �%p�=... .b	f      �   �   x�E�;
�0D��S�	�?��I�Bv�ʍ
a�Y����>�@0l���0ҡk����tX��	ʛWv%����%�$��2�
���?}FU�Y�d�\��y#c�k�V8Z�z�;���/v$-Z��>)���T�̏�x�s�}5      �   �  x���Yn$7��ۧ�h��D"'��I�L��oa�܋۽%.@T�����w�I���Ni�v��!)$�%���k���]z�U��ډ���H�H����T/��I�����}�V��ߒ�*Ɏv���{����-N|��E7���X)�#��[�1�>���r�ITRa�P9w�^��y�v��������U����c�)�gb�}���ϙ��l��.�\�&9VJu��FR���h@�ϙ��+����-�+�[��3|��|H"2,D{����{j�k	���e/��83s�"�C$��ӏ��¤��Q}ĸ$��{�Y=�gﮀ<���<j�ψ'�y�I'N�?Ǚ���=ui�B�� �������ٜr�tM�q�-#p�bš���
m��;��������wvN�$֋��t]�ZLs�xjS�Hm�e���"�|�珿~����y5m�>a�??���᭪0(���J�N9�&���=2<�Ad��K�_��<^)�w�&?z EK����9��>G�lF��6��pQc�%�?��Mm����nBO��ճ���QXU�)GuY�(*,M�ma}!<���-��=�#�����*=����V��ʥL�2�d�^߿�J�Ȫ� �믝W��Ǭ�PP9�jg�b�Z�0Fޒ��]�<o|�H� �w$��ȑ<ޜ���3�uzn�ՙA	CΊ#���m%�Z%�ޓ����)9�6��1.��ي|�Q�9N[��Ř�-2�־۬�b�}�U�����a�~����*{mD���uN��|�D1��B�-�X3ms�G�t,�/Uv���L�z1�n��8�x�Ȕ -��x�谥���-��>O�zЪF��}�;�+~��e'�b��-���p�5	�� �<�Z�H��V��a#v�-�\n43>�K��K@+f;��!\q��Z[!�l%�⻟m�78is3hh�r�Юr�%�����Y�٠�(j�;Xÿ�1�4�n����a�O^Ў=,�/��E�U2����E��N&S�e���;��N���c̣I =ګ�,z�^��o�%CF� ��^���#$����+~�^�{�Bv����Zz?K��̀*���2��"�h�u�2�ToH�D���!%��!�=�y���^5�Vm̲Xj�����r�߈�k�������*�0�al��B�6K�s�?5��^_^^�;Ǒ]      �   �  x���͎d5�י��%,<r'q���]�-��q�� @3�<=�v#�!V]��V���w}��]YRM+LKt&�.�<6-��V��s��i*�����>��'-����;m3R.��r�Z:�HC8}���i���)璾{~}�2�;���C�7x�%��%j��7�f/d�*�q�y���Ԥ�XY�g*>T2���ѱ�.{��7	�0���� Us�̡���M���2I��ք�1 ��~o��,5|Od1:���ɺ7�\=���-�nU!���5<2���j�}[}␚/J���1>=�-��Ǐ�ׯ����G��)7�t���Lk����Es��gX����$eZ-�� ��4�	�E*���ϓM*�0���&�.�o?F���2<yÑom"��E7�wTG�)G��.�}�.���Qp�9�u߳m���(+Nɨ~p>���(G$��L�
��"�g��X��o��
SWϳo��5��j������?
�҄v�@�7����u�4=�pY|�&����E%#HoLv��n��y��Wo�#:J�*Tԅ(ڈ��!�?���I������#U��c�������9H�4��A���].���P6`�FE����g¼���*e�ܪ>����@�g�8K3}L���<�x�+��;�k�[#�٫mb�r>J������V]
o�D��~�]�9F.��0}��g�9J�د��$��$��0Z�N�!ܵ�b�Ť�l��b�y2�`�k'̂R��_��,+2����/��R%�ˤ�p��67����q�����s�uS��ie�1����o8�Eז����Q#�&��CCFC���!+�G�`\�h=ӭө���$k��=�gg��gaX�k{y��7?����7<��B/xZB�"�?,�&��ĂV��1P4�<5U��tj��mF�A������<6���_?���ȷ�!��u�C1o�^�%�/����է�nI��mM r�A#�^Q+0-���yfZG/�y��񟁆)"�H20ju�U�����K�Xѐ����5}��o�a$��d�k���R��̙zIس��WA�AP��Ѹ���p5$oU�Y�t�T�*��!���fH �����kT�<��>����Q�I�Hǯ����
��ѭC"�4*Î��ݼ�û7o��	��      �   r   x�344�440��LMN�4204�50�5�P04�20�20�357130��# [$�M��8����fh�gh`jll��4Y�i���@Ӝ�+Е���[as�,ذ=... �/z      �   !	  x��ZK��\ۧ�T�/	�s�ٴ�s�w�x	��XÊbD��!� d,�Z~p��Kl�	�b���$~1ݵܹl������}���!_���n���v3r���}؍�[P�d~���jw-m�ݩmVKir��r���ֳ�Lҝ+���w����^�&<	N��V]�Vp�2B������Bpq"(U�#w�p���ϒe~u��wÁ��*�|g��Qkѵ��\�0+�C^�݅�C\��*/E �E��1I�Y�(ڬYm���"�^�����>o���7Q��������k?I?;Id%���J� !j!�P1ᅋ��n�ޭlH<��ŝ�FRk�����n�&M�m8L�;#-5���c���nn[S�4^ i[A��BC)<ɔ��kf:l���� �,	��Rv���B������1�#�m�ս�;=sg�Nu3F��Bb2�VQq�]�i����0�)�^ �LQCtRVȠ�YtBE&:�4 �ˆvY���,�=��fA1�M�T�;%�܄��YBb�fR¯9������"D��L#����<^��_��PQ�[�֚��N���ۏ7�����o���5���o����/B��7�ݕ�;�N�C[mG��o@z��8�;d��~�!D��xR[��zZvyA�����jw�M�b�\Y��Z�LAO9H���RB�-D'�ѵ�|���Cf��V$���ݩ�E�%ň4�g��Bt��u�Lu�n�랄 6� H^����g:�MHdw��(0sl������t����A��O�ʿ-j㺠�c&gw�qt���j{e��� �m�a�ɜ��t�U�#�R�;�t��d��m�Z��l�0�)t
RE�.N�g�	w�*#��5���2���(�p�,4��3m�
:�\�5��}�c�%��� �V���۩��T��m�9&W�>�R;��� �A+ǪKfM�\�.LZ�$/�M�D'dc�S^D��B^���m��d���)��SF:͔�i�>��&9�y�>��vJ=�p��_�!Sr�+�����NV7h���%��d%$���|H�mB�9��#���Q��g:I�\�@u�C�T�j��Bp|"Rv��!�� �|����j뺻�"�m��nQu��5X��!�g{�o��k�^���Y���9�`<}Z��MJ�S�B�j�1�Dg3&o� ���N}u�E��,��������0D�2B�y
_`�rR�{�����*EPu���e����31�N���<�r�\	�Zqm5�T�Y�C6�*��'��G��ڈ�R䵢��b}��[��Z��d�D����@>��RƉ<����l�rX�g�ba"�3w�>�g5�O�����71�� ���f�/m�2�3AgE;BV7�[a�6��M�Kn�<B�����4��f�7���!�hImaDn'�"�Mhr��cth�RD�B�l�B���6��棻��K�|1��n�T65W�5�<����0�ޘh����	���j#TR1]�V��m�m$��u�Z���\�]/�	����zW)͞����5�"��&����*����G�RT�����r��Ypb�v��)�5\�zf
��룝d�0�C&~g�+�f��qw}U�6��q���"��OZ��ӣܛ�H��QF/�@�_oa��Sw���"�$W����S�T���&3*9���)}{f�.��ԝg����kO�RCy)�PsrR��"�:4�^ǯP��V���(���ݦ�Fek-�qvnRJwٝϊ�����lͧ��#T���I���nn��լcdCu�5���\�]W�b3���	@˪�?mSv$o�˴�B��,-���&���A���A������㧦l�h�9Sv���-<i�l{�YS��i����]+�$�̶7�M�f~�8��P���+��ʎo� r���*�@���RK�M��%w�d�)�P�g��'V�͊�8�f%(Zj��u�3]i)6��fy�j��x��a�_�fܳی� mϚ���[u_P�R�y��d�&�Lu��ɩd^xi�ِ�09�yU`��ɓ�xߦ^f�yb��j��F(WŊ	[���]!�5Y���8�X�L���o��:p�8B
��R�B�9�<N�s#$�5:�$�;2���ݕ�T�����7�r��q�v費�����r �R��]��ZpYswR�!�[�������AmkE�����Dt�)�F�k]��[qw��v��u
��]��/d�(��*�C�ܙС�IB֪�p���ݦ�7N&o#$�͑��Wf�ه���?�JR�j#�-|�!���6�$\8?��HVc��L��X�~�ɹC��)��?��_?��~��� �|      z      x���ˮ-;�$6N}E� Hw:g���3�4�T@��ѝ��ˌ�\AgG�Jn�ʼ�x"��n�s�u&����b�����ժ�(�������M<�Cs!�=�_>��D-������˿��������������/k������������������������V��^̻�u��t�6���CZJ#�5�8H����_"���U�<(���l8h���Vp�f*I������K�G5ך����������o���8�qH3����'����x.�>��ZH��,�
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
�s��u��b\[���uP83��jX��]y̰P�Mii�j��,:-�I�9&Wi6�`Z8�A�N�i�M��YG�I^	d�㖷�,}N��Iͼ�T-ᚳ��^,�[#w��z��%w`:���@�+��=�H���k�# ���V���l@h�߸Xb��g�pp��fiGd�������)J�{��i�ͮw�78���n�(S$�z�\cRXq%e`t���J}�=�K��@�b��7�ڶ���q�G\Tz���YH'�l    3~k�&y����s.�e�)g�5L��GNc�]ߞ��O\-rE���-@Of9)��P���֣����,z^�lx���W���ӭ�Q���l� e
)�ʈ�γ�@��'�>�9����d��h���<^�k�誧��E\{�jS�['Ǿ63	�n�h� ��F�Ih^��Ppn�=��7���.��r����z�K���C�9=�Y(e'�{\]��;7�y܃�����iߤT�[�~�\���*�i���X�{|����Q�s@�W��!�ڷK���$Җ�:�  ?�RO��G�<��}Y�.�@�؆c�6�F���������ퟅ�W���%�Tµ���h*��L����l��[�ߞ�x�ƥ�Y�v ���1
��@�Cb��X���h��1�!�ͽ{^[/
'[{NWd�� �9p��?^':�G)�n��AY"-�ٙCȖu\�Z�|X֣���s��`���>4��t�2Q�ܩ3�VC�~��4���Q�Ƃ�6�"4�;=kH U��/�MNs0HJ0�=��|���m"�����(.�.�Xp�)�|\H����<��ʥ���v��p�t� �w��.�|&�(����A���`�c-��!Kk"�=�ι���c��sʗ�+/Gݤ-v �+��]����`=�u!��C�SќGl�����ҁ�q�\<UF�����G?�y�4hxO��/�2m��<��ky��Ҩ��g0���������T��wP7G6t@y�c�2s@������
2�-�i=���rٺ��B���Yh�6� �G^%���"�^����X��T��
�m�|�M� ��.B��Y��)m���>z��hUo��)�Dwe�d^�4|�V9Z�{�(?���|>~ QԬ[z�O���HK]��@��n.Q�����Ř���]8B������A	�0��m�����D`ޚ���Ƿ�Ã�j&+�S("�[�H�Q�\���`�ɮzN��w�BʃRU 	��T5v��ߎ>8V�2�x��`�nU9	�ۗ�hJ��ų��4�;��3i�v������+���ܪXI{�<{�?���V9��(S�p�D@�G^7Я<�Tl��� B���䏸���iv_Z��'T�v��%��.xA�#R���-���u6�WH�̴Mc���1���ϳ��S�vj+��`S�N��9l�͈@2e�מq��GI8�P���s�ph�FA��2�{���5�os/D��8���Ċ���1�B֣��;I�?�u�6��e�@�Җ+u(�8��Ys�iC=�5�~�6\���Xv�bK�ܹ2���0�u���\]��h8���XA#\~,.�h'��|*~iO��rtB��p�M�&`&?�7��ަ��fm�%i~W�z�HE�����p@@�c]O9��^`�p.�y��&�С��x{w^M[�'XǤ��Y��V�uĠ�uґ0��5�Z��Qr��"rI��Y��)�U}\�8$��h�n���d�^	J'��д\t�ͰA�ݧ��SC�6�$�����1���q!�U�Z#�Gaǝ���vf�w��5���b)��5���֤�=Q<�=�hn�b���[�8�i;(����ã���I��㰰�;v���������B�U-q��c�qe@�獙�H�tpd���s!l/�p���q!o˳�6V�c��1���,	Ѐ��:&V�t�wG�m`�w��g�5H9г��:�^�,�*�Ǖ�;�#|Die�w�.(�U;��,��+M�B���hh:"�"��vf�μAJdg��@������	B糟ŝƌB��b������E5w
EF^d������T�:��{�ÿ�� 
Y�����"�꤂�
4+ؿD�,����z5��c����R̔=�R���>2X�,r栂P��,�S3�ؕW$��� ��\�o�����7��:�:}�c"Υ�x������h���^�=#vP��`�W8���/�' �RF��{��w�R �Bߍ ���5�eMg�E�Aa�� H�*6�~�Mx\�����<�0����� ����Ak�3H�?���B����4o�ŧ���������2tנ���Ͻ��aj��1de�؜��ɉ�Gi�XFh#���#��Xx�}�z���Nx��z�8D#��)?z�����z%YYe�D���mc���z�Q�:c`m~����7�Ѳ����e7�	��e�T:��G��)7����.�l���D5���=6��y�Le0o�Z�Ԙ�ڒ�k�+.P;v������㈮��<�&��_��cx�����o����sl�v�ɧ�`W��U��ԄB.#��YC��� �jU��*���x����FASG�R@��~�����km{�)��-��;��4x��	hn{��-�Օ��b���j��:��!|Z1E�"�t��t�98�ݨQ������?�'D���F��P�7�lK����F�>s�=jy��w\�^������_�mv�ym�i��96�z{��V狠��''�&�>]+30�s�ݴ=ⱦ+x̌�ס�~��78sw�e�m-��G��Oe��G�!c>J�v��h�Sd͕A�'H*ї���������v���t�|x(,,�s��&�@6Gc�Vcu���v�`�*���T��Qη���Ek���6}Y�8W�Ĝ�BFf	���\+����n�� ���&��:�82Q���0>�Z��Ɒ��z�^��E�sN<D�lf"���}ߩ�����˰�_��>���|:�j��ݾ$�v��"g8��z3���������kG'P#om� d�Eop 
�D#R�I YH�_)��,Ē�o�����Z�������plי��64܋�q����rx�@˝zc'�Q���Me��L�=�p�H�Eb1˙��9A��}���<'e��������~:S�%�����蛎Y��>�&VY�Ω�VZr��v����<���{��H������� TشI�/�-��2o^����V�����mA�kr42 �Na���x��KśG|��-���/����R����|�/�v����7d}e�s6v֬O/0{`��M�VC�Eҹ�y�6��A�'%��iCϔI�[��F5����E;����v�9OOI�z�mW���,�����g�pT䍚�kɢ9'�KQ�����Q`Oi-��4NV�'{�{���19�^i��vix�L����,� ���t?p��|�_E�	�gqXHDW�����U �UM��0-��Ic�+1����t��=���l1��Spb�,pg�EI�]#�lJ��"��6�H�<6��6��
>l�1KPw��/�Dj7n:�8߻{l :�h������Ag�YB�]0���1�$ �<wM�آ��Robm�Ј���,D􀝜�}�t�d�m��.�g�ڬ`R��t|���i4@5J�\q�9f���
3��=o�Jϯ�p��Z��iݼY*�NZ���T?���4ó�ށ�+�t-/�j�S���6���( ���H���45�c�t�z��B�q����&�'M�C*���c�|h�gf�Qs�S��������J
���1gSH�T)�����ɣ#�Lr��$�P�t���;�:HTr� �a����©�?s���Ӡ�:#(�J�_)�A����H���<�z���m�>�k�M��:n@����C��uj{9��t>"�b��s�S+���p��:E�]��~�����6�e���ծ�+�:)�%n^Ik�I3퍞��}�ؖv��$�d#ꅇ��[�p��� �֐��j�k���6�Juj�s(��4F��ﶼٷ�~���h��ȧ��W��@3 9Y_�5�!�| 9a�� n�L�<����=�Z{�g#`k϶�td�hY�%�q`أ8vs[��;����:��)�:f�i�]��_����о�3B3mA��`J���FX�1n�病Υ%"p6�#ܒ��רd!�S�qΊ�/�!�H8
��F3o��  =�ͦ�j�E    _wX7pO���܅D�dWa@άG���༂�,P%�C/~�������U�\�3Tv�N�����{���(�z}�!����\��L�7��fE���#[�% Á؁�=�>ѕl����#�^!���$��]�8��yP�$S������7�^�y4�rd#�%ƞ9��ANF%�*�T�P�U����mݥ��_���T��tnߋ�p�H��ާ�=h/ h: k4�̴$�i'|���r��2;O���I����g<� �S�s��#�m�����7J!�Q�;��Nm���tl������˶��|2�N-$N���AD�S��aB8ͼ�m��ڣ�AyH�
b�0+�Qɝ�Dvn���M�c#{��s!�o^"	4��S%�x�Ϧ������G���®S�tK6G�
DU�ؗ,�DP���Dz��w�Gꑣ�OSHM��n'=�o2����m�,*��0IY	�-�y��b��q�K8���ކ[��6��]	g����y�1't͹b&+]�J�Ol6�Ǽ�ha_�G�Mc6@2�8@m���N��
�xU~Tv���#囓}��������ʐ�K�5�t��|�ѧ����c��ع���Q��89BT��H{j�~Y(��i�4�TAK�s*��8��G����J�v�:{ԑ.�Z3�%�F�ʜ���V�P&�JW@�7M�%ϳ��}���q���~��I��}G~�k��$�'��S���ud�Ҙ���4|a�����z*�F�ϗ�yv@X4����N}�c�B�$x]_�g���6: �0OK�-��ق��F�}��Uj�6޲�Lu��Eh?�)��9$�{Hf<����W9�:G�|��9��!4����
T�H���i�.����e�9�,Ē�b&rC �htnu�3��RlWf�s���yjkl�Ts�u5u��+�j	�vu�)��G!,~�� ����9']��]�-�t�,��v9���v�W0�n_`�ϺD�bʐ�Я��ш:�ԝ�Dn�����-�Q�Z2)��@D�Y_,��ʛ�������E�����Z���M<9��'�|b���ګ{�7n�GC�����l��T��e�u�.-���,����mu=5��Dw�hMz}{ʒ�k
��{X�
���:��fs,b�ޚR6�ԠE2��L:hFO0� n���#������w�=�ؐ�(������[ߠ��ߏoS�
�44�g���n*g9��@b�**ť(���&	��Ub��AH����n5H�
9<Z'��C�SUS#`^8#47.��(xj�G�:�A.?[z�`�έ��Ts� Q��S�r�O"P�x*���g|���V�ڞ�dƠ1�ӞJ]�ۖ۠	P�]w��ˣ�,��I� s��,-i�u	�"+j��զ� �y
ߖ�u�r�����N6�?����`�g��f��"���{)�E,e&�Asa�z�ɀ�MA���W�|�56�y�n�r��g~�"wgtl�Yug�x��@�-P��F�bB����>Z��yU��G�n�ʃ�B�a�&���#C�?
Ǯ��������nSc�S���8�(L�W��ʚ��G\	^f�+1ܩN&Fi��hV�L���ùӹ�_m�O�L�F�*������1�V3�q�a�1fp�EDjIHnaw��V�_Cb.�q>fр�f*$��Ge\��z��fH�k?#�"Gg�̛�n��G��Py�^���F6��O���T�u)�����]�p�\�T����D���E�w)r��
�R��9��Ӕ������9�����f����f����祥��^p���K�nN'���+eKi���}\���ʜ���b/�t��z�+�z���/�<�,X'�:fI�*��p��ml���B
E�2�8�YJ��u��J��؟�r���w/��%��1-�hx�"�����$�얅����Y�������xˠ'	Ue\����JQ+�����<�UMֺx+��>/OE6J�Pc�eI��U��.��E�f����2�l��*#�%����)?�Eg��01��[��D��k���ll���������B���(l��j�@�-�.�Re��O�봶��i� �k��Q�e���7}/x�[��\K��x�X"h��iB7�y2��Ɏ��&�������qI��r>�s]��ȴf�)E��^Ax<��߁��"�7����V������\�s,x�5g��gy<��1��r�X-�)�Q��3ڼ� V�˵إ���Ɵͫ�j�1Þ&S�M�Q��π���
�h���z�L(u�����F��.s�r�H�υ�'a�5�7�t����`�{�xkd��6���':�ttWg�3]���}Nǐ�}/�}kN�h|��3�+�#���ܹ�~ߌ����s��|�s�S!HY�s�.�����j����I�!�|}�������r�dz�Kr�z �A$]h�K�,���h���"��N�f�ͩc��8��H���T�7�o=�Gj�� p�g�7~ ]R��ӚD��<��'ؠX��:���������o�s2��V8�f��RW(#����B���#���'�=ql���B�f3��P�Z�Qp�.��#���JC�H�R����ɦ�yi@bXʿ�	;S�O��l�v)]���I@˺8.D��1��>�qoV�1������N֑3�@�N���n)����=@1��_M%��>.<��Y)W�l��3,2����u��,���T<m�����E{UG��v�>���#�t����)�%���?٘�*%�X�蝷�*�:���SL$}(5c�+�H]�~�I|� '�^�h@��6��	%�3]�!T�?q���L2�Z�6^6���wAO�Y���h�p<� [���>2xt\�{����U�����fS�vd�D:M^Q�5Ҩ$�/kP̟Av�*O��"$P��q�+� X�!B��x	��;)��!�-���nxS @P=�砾�q'��F:�>)�l�  �Ɗl2�^oa����ha���||�p��~N'�y���guS���^�ܲ)���̩�������.XÑ*�L����Y�|�s5H��,O��~�ѪY�T ���$�dӍvU�W�Q(D��~>!�p��$�/���
�[CBFHTMt��X�b���]#��khG*�3��#�6 n�*�������J�9D��A�$���3��rǈ����e�h��6=~�ؚ��#�LJ��y����A;�{<�t<2G�9��[���k��b�#z^�z0A{�~���">�ȳ�W�I�m��
D���R�=�l���#E��h����L�Z�=��/�i�ӹ`kW���/	���e���j�ХxՋ�������֋�D�gM-������; ��o��l�K;}�R�)��e�q�6�C2�[�z;���K�緃�ZU# �#H��a�FK�<��9}��I�̰N�
htz��"I�R�}�6�|���s�53�Q^
LzW�`7����J�)2*]��7��R �W�
��d����}�����N�}t��$���*		���,������F8�W��
a��y�S�F��P���s��v�Mr[$�r0tI��q3�U�ݳOs��9�Z���Z'�Q����礴 ݖh�6��q��I��X�*拢�W&�I`�fu��]��\-�A�p����zGt�����Z㱩�)DzY�(�C�,m2�<���σ ���-�b�d�R�jaI��bu{R��[6�L")VS{O`s�Zb!�W`}.��+�U��%���t����;�-�j�� ʧ(�ɣ�^��mK?��;��_�uW�fy����2S�b	0;��$)�GvK�+���IL�!��yR�ҷ�8\�\�űϴ"�Q��e����1Su�x0��i	�x�@H�8��*r���vC�5�͛\��R��M�<�J&��_NPT�8�7S�il�nf���<���^j�!�[���}܋Y�9�����Ҷ,�g�]���gU]��?{o��K�1͚��kd�.љ��� t��)�~��^�:    +澵���&ːʋ	N8���I�9Y�/]{�t�.܇nS�t�g$:���^ᴙ㋅k��"��|`�t�mO���ɫѷ�W�!M���M�˓���;k�}�%4+Vu���M�d�~k�y�[8���d;���>9�l��I��:���$��eڼ��_>��tc�V8����x�k��\m�s�W��*[_v��� ��&��K�t��&�)L�b	�{��_v4:�[��K��V8#��<�e��2E*��/U�_�o�2��Ť<�vB�5�$����Jk,w��g]O�C�O��8J"��*+����ι���������#��n�$g&l��Y�.6v��;�U%�x�k���oZ^Ri*�E�w�!��������U˜`Y�@�G�x^䓯)��3�X���� ,I0�j�I�c����G��:R|m.�����p>�#������٭6����D�\��63�) �q�
,M�땰�)I���zs�=r�Y��'��R'���@�A���Ĵ� }�ï�=l)c�OVDA����'�=�RC������8�|�{ܷ�ob\4I�u�h�k}�#5z+�҅�J��:��"g�%{*�Yi.j��g�H�&����hY����"���͛-m]r�g��S<�#��	��\ܐ�˶g�:�P���R�p�n�F/En+ �e�l�b�#%�qGt�YQl�0)Y
���4�tWYEn�����yr;��F�b�r�+o9��;�+5���٧>�y�y���1���^�rx�@V޽�燙�Ŕ �,=(������������Zl�ͤ!ז3T�@���'��5�c����?7��{ ��Q���#�}Y�ʹ���卵AQ�Q/$J��)��pƍ��Z���ȩ �j��-N��.��3:�ڐ{ uA:z@=}���m>�m�@��E�dz`��2�꺏�y�'��Se�<��L�&�&���� s�a�&x4�����"���)�G����%qI��萻y��2_~�
nk�����1Z�hGu%��fpJ��A�up3W1�]Q?@ȳ_���Fu�Opw0�Bo�\���1�㵝�h�(5k)��~l�A�BA�/��hc�&?���E�����e��qn� �SU�
���ش���M8⢞�T����QV=!a�#���y�^L��,��+>�I|��6=�͵�ZZ!�H�Zc./�!��"r�꩘e��`M��iV�z��5(D[��#�����[oJ�3����Ĭ�#	��7;�:� �Oe%|�E� 8���XЌ��4���c!|�0�g�9��B��->�L�F����_��C����p�*�G"�,��,��j�Cw����r8P����kn�՗��)N���B�B�K�4""!��k,��D��~.��J�i5o��� �ֱ=��E�_Z�E�o��ӸP��K)x����j	� ��7���V������"�9-�Z��0��3~BV�XMv6��CZ!�%[��B�
NNLCm��
�I�ֿ,}��'*�)���8�g�4NÛ?q�נB�1Yj ���v����NaHsE�_�i����FQ��6�AG���[��%|�R]�J{N�[�!9z�A�yQf�zj0�ŋ�\w}yk��p4dM�[*Z��~4����&��}�����4�̃
=YM �戜|͐��/���8%���\$����8���;�y^M�1(�4gﴩs[OGx[(sG�Gܤ��]�r�᚞7��y� 5{��Y��ER�hY3�
���-�9K���J�㜞��	���lH9Λ9A�@6��끍�Wۿr�c��1_RN:�|`Di�e�{�W�̔���Ǉ`�����o�:�`K�d/Gz`٣]��Fm�z�"��@�>�����J�3݃�:����R��SIZ|�|�>�]��r������Ƣ�8;֢c���N���fD}$�#a��SR�q�E�Jj�I��q{���� (���~��v{J��=8��e���ʻ��v	;T�@w�<�A@�Mw�6��Z�C�}UZs+2A�e*۰��(���Hd��g�ݯ@*�)�Yq>���e��&8�0.}J�T�����x��:����ۗ�r�#���ʎ���șUƇ�*gΌrU*@�]GێU�zؓ�9{͆&'�rd��O�¾��bÃ�����`U�&h|Mv�F�l(�ߑ'.�-cKG���6w_1�t��0U��u"�R�� bs; �	2*W�つyc�K��1�35� ���R�� E����ݯ<⽐𐊺bW��S6T�嫋%�X��``��G~�,���e��lPA?3��b���kChH��j�!���4�HZn�lӜ&��5b(����u�b���=�D��R)9SL�{ (��(I����2d���b��<�e5zzثgсK��jD=�S8��\ۅf�?����0�@v=���=}o��� ��s�8�}Q95T�W�{���{f� �����# ���Mss�[���^���G܌����^��q�-����G�Tԗ��ƿS��Z��u��2`�Q�1^e��䋯/�-t�,X�W��!�n�i~�RH-�zz����� �kk�B��V�2uʭ�bvF�a[��^�K�"m�WP��ʾ-�q67�b�&:����"��^'���Y���m�E�}:��dA��qh*� <��+]�c8#ε�����#��y5��i�<����'&=E{t��N=aS;$��B)�`*۞�5��
�g&�l�A��*��~{e���#���u@V��i@9��ьA�aԸ����O�҇-��s3��)�h�8��a����]ж{���3����-Ka�4�9ƃ^Ҝ�L Z/�N7�~(����3���=�o�i�4��G)�x�)�B��b�iu�������*{"����u&H��Ӥ3���6U�H3(�G�w�I|��x�A��.��|�t�b�Ce��(�;e}��#����;݌A	�٤?��N� l�,�DqN&R���ɢ"�n+��8��w	�'�(4{J��'P4�4(��(�ͮ�("=$ݏ�ʙ��~K�h�;���5��@ �:˥�J���f΅��x��Va���N�8&9B�^q��u��^�͊�3�~���3�4F��4�uc�5��6�z�vڛB���@�<���}���R$�Q�_8[�͊�=�6T�jS/�-M0U�zC��4�$�_:�3U\+/��*���*t,dQp�`Ƿm����5�~t������9��6��Wj��o�5gN�]��Τ`�j&'`�fz���=���9��,r��Ż���]��-O�#D2v�we���Wl��8��+ ��rT^=���[���662/�"k�� �/���=^m��ǁ�ʑ��o���?��zCC�$��ӯ�Fg���T��wr����U�}iX;��:�8<Nm~��!���F���)[��{�a;G]��EX���V�����,�P�j ��F�sdx[ؗ���֟�@X�
��*v��k�3,��@��h�1p`�7/܃=��>.�;0#�Q�<s\�%�g�c#�-*���b��B�'�4l���IX�d?�"۝�U~��B����ׅ��]�N���"�9v�w���k�+���(��R�h1�"v��^I�i�ˍ�*�&�ȩ  �_V�^��Gl3:�˞��Z9�	�&���ó?����Op��׿G)�)g�"E��QT�[}���_����?�;��}Ҥ���f-�"�W�Tf��A��u�Ci�b�{�A�CMv ���=�7�l
e>b����G3?�QN�޼X%f��Ǉ��rYψ:��#Rh�֥ӿn�i�6abr�}W��54����K�U؏��lM�N ��Z�~ ����� ���� z5߆�u�j�&]����
��S�iVy��c������Q�=[T�����.�F�{,	�%X�z���S�9x�,��A���ݍ�j��Q���v|�O�	l�o�Ɗz���7���a_���o0f���-�����GմԐ�Z"o���9"��ҿ�g�����Y�p��o��8�v�l� ��    >��7
�|2؄Z��
s���)���u���t�w��ӏ���16H���7O��98�p�Q����8JV����]��^r5�	?�4ަ$8����̖��#~��t�G�}�g��g)NT�r*�ځ��a�b�צ=7�U�OVz��C�@3��2���o�tzzgO.�F�a��9�ήZ��,t'K��T����C��{���\ŋ
���Fs��� ���ѩ�&���u��.��u����J���������UG
	8��}���K�x��Q�V���kQ�(R죱��cUj}��_R8��g��^mZjvRM��!;�r�|� �D���c{/U83o�)�X���b~���%���]��/K�'pN��Tl}p��9�ux�RU|�;E\��o�V�(n��]XمK�<����������R�^��m�32O]r�3�Dv��@��+�������ʝkMK5�yj/W�Ou<]8�9��w�}=����G�|����W�u�	I(i�w����Vx�g�HCw��������S��"~^�!�G$)��_ w�ճ=0q3�E%�#5���`����;0 �㷜{/��g�� �
]�*x������t!�o�D)�ټq/)��bc#����E&�ԛ�6��u��k�x��ܜ�]]��wV��n�82��A�@�z�{z*�j6�i�FBw%ϛ-����5���UHYW��ѩd�t
��r�^�7����.��I�y�~ޣ"i�~�&L� �^<��i��_g����}�k����,e��?>\ a�v<T8�L���H=:�9YK3.s�
<O��E:*�Q���r��;A+/g�/d1[A6�VN0�<��|�e��� �!�2�s!ήRI��'bغ(��*3>�d���*`���]��S��M����۬.U�Ȼ��#�-��\���_g�����A�dsc�7��R��8��MRl�T�Xx.�Vt����T֬���k5��!��%�T�3R`O_��;U,�7�V �FWm���J7��W `��􍗅�u:-j밑P@��Z��& /S���;EZӏ�E�ш6����ġA:� 26�=HFD�n�|�p.Ui�U�pa��5M�F�z�BX�*)���5�'�=�����˄�o�멳&$Q�)l7�8Lx�q��YT����8(���Z���RyM�=����=��Ph���c(+1�L��Ħ�J��H�Շ���n�{���/-����wF���nq�-��:�̹��B-�"o>����p��.��š�hj:�#�̖�0��+�����Ź��d�P�GA�xU��R{�����I�v/�Z�O���RP�Ф��(W�@� ��=��]�U���Ys��86�&6�)��K��$m"I�o����K%�.�$��+�{�)N�4\k����P:@!�P��z�r�8�8��C�0]@��-Ӂ���V�|ɱ�A�u�" �Db�62O�Rl�"x�$ה6�v{�,e(z�-��=!�b��^�vDx
�����J�l,��!l�MW>�so��b .]��_��u��yJoJ��v�#"X�|�Ԋ�t����/KY�����@��:�{�k���R���&�.�M���k�9�v=ǬaD�2:�B!�=��Gn=ƍ�^C5�e�d�j�.�$�<��(�6�(�#���C�6�ȍM� ���)��*l��ph���=�֎��!��[�RcΘ����y���xH�L�%�x��WF� �X��3�)8�IG���T-|{%vy�(I��Л	���b�M�6J9!e٥m�q�s�f��/�-d@��o���f��4�,��h������쳳Vn�R�?x霛WE�*ν��=�Gi	/�g��Y��9�5�3��L�OD�!�D v�ӆ'_��>�JN&��\F]��e��P���Q4��Z_�Ǐ�'�b�ޏ�[�@�VQ9R|Ow�8����l�Lǉ��UL��9Uu�}t���
d�c 8�n��o-+A�'��3�ױ�|���Y���Ӌ�F%�.�$}j�iQ-�E�,�:��ލ�8�T��|�{Po.$�P���&	ܙ@�>�ޓ���N���ͅg�*�t��MC����ʹ�c��&���M���{p��@�)	��1苈=|�r�m� %����l1��&�x���m�{c������z�e�l���-����%G�ɟa�ce�-PtU/���Ia�ʁjŬs��kL|�Eb�`#j6��x��d3{�-"�鶀l�]l�0"�S ��Ң�q^t䈅<(P߹^�����y]H�'�T��A�RE?����%�GvJ���`�/���/�b��r�%�b���JuV����|$��N�m\M�ލVHd�k���V��hv
r�ڳ;?��=��i5]�H{����V͑�.���ml�כ�xLpH�~# ������]ƙ�l�R���`�jw�%�錚Ic9U�k�����2%�wa���m�f.xN՗`zmiv�������`ӌйQ�*��߈�]D?� ��M�4i�����3�N��c _�2�,�Y�@�Ok҉���`!�~f,�od�G�4��3׀` h`����w\`��S�P'���4�T-���k�y�F��n�(�T?@� �;���	3}hM[�M�F��m�8T ���>9�̈́��s�z/rVa��#�"�ٺ�~��\e抯V�C�:�t�U�+L_�\C3�с�� �, �췶1ȝg��n�ޏ/�u�B�sT��UWqo�@	tv�Y�*uA~9zٛ� �G�z�-5f~����GvO����$�\�H�r�bh�m�,���A��)�ǯ��ԏDuf�5W�'��� 춬��Q�]����וp&��Őf`>H��E�2�^�K\��e���p{]Ѿ���
�@��u�{��u�N7��}��os{G�b�ҋn�~m�CM@�W՝/ w,�%�V^�M�ɨ�(|*l��Wo��� ��E�	m��9��x�J�!���NB�k�|����s7��G�H@�/���*�;��/*QT�,#�]E:�����8�������z���pU6����?
UƱ�WJ.��y�����\���h�U�>�>�O����-�$.��7����2�@Сf]<#6cl{O���d�����eֻq$���w� �4WĮ�q�>����t�&'��OS�8��<G5��yu_3�B�����/���1 �9$�B��,�8M�6�\-RJ<޳9;�����tq����@x*%��gj�#L{{�5^u:=�,�Q[O�e`�ۻ��WI�U��K�֬�%���&���<QUǦ��ymA����(�~�;�ӵ�c1Y.7D�^]%�a��8#O��ӱ`o.�Ep�8�i���b�~f��v̌�~Ɍ�;������*�AZĤ��?��ip�>9���r�/����_f��s����Ԓ� O���&�NrÚ�W_�*�G��N��]fD�=h�w�m!�)�����D���qR(�P���c���B]������Q��'6\��WO�^��.�X*K�Ո�b�Ff%bK�g�Mg��s������joD��*>�Lcy�x�+v��+,�£�vtR���[��H��J��JG�u��x�</�W<��I������l�(��3]���4 r���GE��K�_K�#�ȆK�aÌ(�8� ��N�30���NiW<��Y�i����b�"쨄K�Q�j����69*y6#'p�̍W��̌�x���t����~:B�?
\��S4_+���b�cZ�:�.J[���L ��v��U�Z̖7�}�Y��������THX�x`�2+��`��;���:��tW���,�\}�4�G�>�EӶ���������k�@G;h���>:>N]1�:��WZ��{�A�I��'����ѧ߄]��~��Q���__�a�QV��S�D�im���dɼo,��Ñ�t72E�;���~��nO?�k_g�l <^�h���i��X����djɼ/D���� x
g>��3�и�0y�4j�֯    �Ͻ������,[�]�ʮ�u�HI�Vp�U���p�_�������>���";O��S�!�"!��Z����]��L�@8^��:%ِ��0枠�w<�?8A�쮃�i�t��;E-	�$�6X�D�/�.�j�t�j֔��T\�=)'�#֜9��?g(���J_�\mA� Ѥk1�Qע�[�O��)���Q����'��̍���F�dW�(/ �!�a즥����1��"��j@k%|�j�sܾ`Ǖ��r�qQƅ�Q�O�y+P�<�*/�+_�V�ڎm�p���'R �r��l]g����w��7�K`��M;��5<���kvP�$��ԥx�u�@�{�1tz����`� ��,$�V�ћ�Qry�l�7��^�Ħ�I�Z�}�_�Uv��!�
I��j�ʞ��Q�О;�.Sإ�P�n��I0+��>�j��M=��1��Z�|9��E�:��T?7�ȼg,��u�
D�X��oQN� ;�`�WFV�w�!T�����Y۠����4�w�-�bf%����Y��Ƙ����͹bÏ��V��RSr&���p���x-�d�3�5��E������ı'���HRz�����p�ۭ�@�)��e����F��A�A[s|*љۦ�N|��x���]�g�0�Ɩx�=vP��n���ᷣ�e���Z)"/�,_��{�������
T�4zF������E_p�IF�Q�c0�5{^��=��m~����*�c8r���V�9��sjrՏ��ɠ;�����Q��r"�+�|���!�H 33����ax5O%G#`z�oG�Q�+�Б�J9�I���z*rIb/5�z��^�ѕ��~ޞ�\�p<ݬg����?O5+�y�$R��OD	y��X�8�L�i�l�Ke�ՕVʣ 8`�T��NK�'����gv�9��i��u������З� ���֎w)e:s��O|f�q�N�P��Je�3�P���}��#8�]�0��2��v�=���z$O��/���	�|Y��m��߻�q%��b����:M����HNZ9�y��F��^<pm�	0���q�Q��a��;x�3/���U�W�kJ*���x����*�ӈfkY�~��]�)n���V8��㉧�W��æ����#ǁ��]2uh�RZ���e�i�M��[5��e�/Z��.
b���к���s����
��� {|�U��kd�h*f���-`�� P�l�c���Q��}��=�x�S���/��ȹ��
H��2'�ݹ�3&Fe3V���Z��`���M�n=���8�����I [�M]`Y/I#L�Kǌq;���@�g^�P �܂�	��(�Cmu��N�YU��%~�q��kV�:p�i��N��� �rZ����)G�XS��E��
�)���e�׋�u�Z��վ�g!�P�^�* �J}��(gu����"B{yuB��O(j�ώu-G�f�8ΚҦ^#�����qj�|}���'Dg�{�Q�/�����y�"	�u�^���.���^�M=��yq<�u�P=(��dңZ�xo��R�y+L!�a6ˇquڔ�KH��0Z����p`<f]��A`
�c�1
bv��40�=8�?���U��beG6�����=I�Px��f��,�&�v!16�D��Zߎ��8ݕݞa�d9��$���Y�);��I��&��K�"mݽ��T���F�'�J)�~�K�vK�r���u`��N��(n��F�_��ч�����`I*���z��	!��r�Z4Q���a��͆
Z��.ι����P��/*&���f��4^�R[��y��P�<foOS��z	�|-��CfӨ^�y�X�F]�2&�	�G^ȼ��Vxv�4ϼ�����Վ�����E�/-U(=�H������[2�j��(=z�$�BN�Zkep����&`k`<�f�Q@Ѕ`��#���E"���g�9��BH
ɹ,P4��(�G�htd��uT��N�ۏTzzwi��n�w�3�¤�֑p�u�G���N�n��-��8���o�N>HԟPD��&�{�a�(��&Tɟ���)�樃��\��ycs��!�-����V�a	\�ͷ~�r&֨�S����c�]t�NU���+���#����v>0��=����#�'/=�������r�C�
~;��
<��5{�J[�%z.�gHI�7�6W$���(��AJ��%`��g���p,���<1�,)�u��ҐSBUj���aȿ��G��d����eu���DTk�ϣ��N�����"s4;�|S�C���p�e)J�+�:�V�5�:��<˾so���N��v>s����mo`��M�$0�B t��+�j�+�f����q��v5/+*u.��24�|���f4�������6��E�܃�.��۞}���I+E�sn`P��#[ğ���m�Ӛ�R�Hs8��:����'i�8g�}p��$
^Z$�U�c�I�����u��D��BM-��/)�J|��d3�����)�ȉR�dЎ���&e�J~	��(�rJ'��[����)����>��̴* �Z�%�$J��;�j>�X�rP *�g��\{JwT�xÅ�\�"H2O�ҰH��<7"x��h�`���-~���J���2\�4 @��v(���D�d�����ߟ����V<s!�u�d�� �Z�����'-n��}~�BF lKH	�Z0Q��˹�������AB��\��R�'H5TSQ"�Ȗ�jj��YQjK8�3��/����h�SWV.��F����뙙cJ`�=y4���t���m>_��f���� $�p�qc/�+[jr��4��� ��� �o���'#����d\!c������6�'��b5;�f?<��Q�a�������{���K`K�y��x|�Q���/$;��	k:�q
p8[^��s���������P�ZW�cwD���bRr���V���N\�#d}�ރ��k����G�B <��bL��Cu��+��#�W�Ci�����|Uɪ{��u���]�w3Fs �ytFmO�XHX,@n7��z<��ڻЋ-�-��Z��~`��#NvSr"��Wf�}���i�R��O�T��V`sr�tJ��	��;��Q�.����.�=
 �ό��/Zϟr�w�+!_#ώg����זR�UK�����B����S��6�M�W�j���"�Z8Cs �w�{P��z{/� =g�dVr=�ZG!g��"ۍR~�s��T��	y����)iY^���V�J��E�R{i�%"��6)��8Ǡb@�F��:�N<c��4����`�J����Zdm���G��>��^A�愼y���VqrI��H��H���#���S�ү���JU�MŬ�&���o�:�({�Gi`y](�OU�K�	YV�H�Q~5�i�b���ި{{m�:�@�͍_kT�q^k��U��ǫ����Zd/�|YGY�"�����cX7Gn���ex V�:-��nA����`�y�b����j�U�;��E�0���C/��A�-y:!�ފ�I#�dV	4��Gk�%ޤ�kDd��@Q@J')$k|���L���#�xyY��6a��Z7)���X/�!W�]�K��"���M2�
���6;��(��!^����>K��1��P�O/��7�s�n�-����Τ@�T�7_�9���Y�m�t��2��,�?���[��P��;h�DJ�sr!�Y ��`��%��f [��&K!Pµ !�4ed3�Cp�9�4~��\$��������'�
�����"]Tu��$������hF6�����_TUDd!��H�(�����zTTMa�ƛ�+'��oj��o�@T�q/��e��R���)Hߋ�,�F�P7�R�cI���m�}�Th2�s#�EyRz���l*�KF_T��տ<�?�P�h����Gx��^VvZO�����F�{�ֱk�n��[o`K�l��i���<���mS�S"���n@'�    ���[�]?�p3I�l> ��H��5
=�՜��i��8՘xT�P�:T���/�&�=�{%��v=\05��q��uv2�.b�C®q=�۽T�xDk�<dr�"C��򪊍�)�+����}�7�r�cm���eW��
u%��:����pM�o�]ؿА��5k4�/��"�]\�9k�h�.9#�>Ql�8�dM��A���K8F�C�3�t�4�j���8>g��>��X��G�f��ID�k�iaţ�B�w�ER\$ݢ�^���kb�g`��,N���@�S[J����	�y�V^+���H�RI|�����KѺv�=>��g3��G��!=2&�ƹ��T��p@�b7�4^�:ˇ�9fD�ZA����fEes�"�R�' �b��o\���"�������Y�Cў�ok鵇��շ�)�ɻjFé�ɱ��h����"r60t�1~1� �E]�B��g��ɕ��iXh��v��%�ƗJi���-y4�ws���_W(����pBgio� �D�&�h�>�l�Xt�zm�n�-Yq呫������^Mfک�v���� �O�~����\������!=3����JS
#p�?n�ɏ� �+��r>y�S>�_��Rz���E��x|S9)4�Zy�_����H�@�{��t��Rt��F��>��3]h_6����Pͷ��:E+�-�W6v�C��J_0�����c�15�ĀM;�:;����hP?��һ������C5��r��*�%�Ӱ����G�Ŋy=h8]_,ǫ��C��' �N��H��� >zf�Rt��ƅ�� ��j����o�d術)`�ۮ����mn�D��y�ܷ��va�J��9�����_���$:��Z�M���3^ n�M1�!s�9��ܪ{D�c�
�\o�iaE6�]�኉:�� mi;�V�,t�Ǳ����΁w6��DRt�6P(6�Ĵ��~+���8m�L�.�<�����
"�`�$v��U�Q�~Dh�+�Ҵ O[껏�y����?�Z鏷�ܔ�Hlr���G��b��)�����fګ)�`�l����.�#�za�+||���=�{���k��;�O��q���ȑ	xmދy�����b�\�-v�Źԉ�o��֩w���s��[�`3��I}��a�E'��A���>�� d�y�<d��'����v��ؗ`����D�UM��{	0�Q;ț Eu�l`�jP�����s�x��o�렘��tu�"A�U�x�� ����c����k}Jx�{ �p��q+�G0��kփ��j�ͽQ"X8���{�ŗ<�O)��Cٗ&ۍ���~��	�$8�n8�	/�U]>��tGG�?W�H 1e�)�ЇOq1����&���9}R,.�S��{6��f���<�$��pԶ�+s�$��~��8_�,}Q���ru�,��FjH;�I#F�U.d��O�(��nD�R�qa%�wcy�r��46@
Ƹ;��Gמ_��v�ӄ3}�u�_��H��vm���ʋ��C��Z9�N��J�uH9 ����z�d�B5��x ���.�� �u^�H���Iv��l;x��+�d���{�8����R��e��,.o�E�T6[����Q��R_|�u��&��=�� Հ��D]G�gO�WK��u~X������h��Q�ŏ	�@�ڹ_<���k	�ᤉص�x�t�Yh��3��s�8���<�g��f�pF�x����Hm�όi�T��a�T���,�D4sw+�PNoi��)��p<�Vw���q�Aq���r�f\�t���1�[�_@n���Q#;I��!�U<�]�ӇI$������p���X�ZHF+��O;L*	O�[�@�����p���U���-��[�%W�G���¢��#����p:�����fȓD"y�g�p�B�_���<�ѕH$�8B���OC\9Ơ��kk�ݏ�|�2�6:&1���r������r	�') ����c�Zf�K9b�oh]�~���!H%]������G"b�v&_��1!����nc�6*@S���!(�S�K��������LF�><�M�P�
.
 S�R���Q�)��c�Ѐ�fs�l�m� S�W��]_%���#���c���q�7��]�P�x�4'6I�Oi!=K�����S�ay<g��;eͦ���/[!wx��$3���/DNZ������p�Q�wN4�4��\���0r�k=�F�⁵���1q�+]p[�z!�P[�R����E���kS�;
�Fm��/a�]����P��FF��
Wڞ.����}9�����d?�S�<p~g��=(@�D��i8o�ۃ�p̰��Td����c�l�7�ܧ� t��>J`f�@<��|V$���;ثv\��[\��i���n<�K!�|hR(���WV�� V� �	~�p�q��
!σ�����~r�D�AIC��]�	���*�E��� �����e��q}�Y&r���WP[
{5φ��z;[g�\Jeq�����,�������3s�J�����>��SsZ��A�����ȷg��^��S�q�iFƪdr�{��<?��Lu��^�M�F�G8�~R3�����0�$Ĵ�ܼ4������ӆ�����P��h�x��*.�A�Vy�*g�s*뿦82Kl�+���-���Fa�X�A�x/��X(/�^g>�X��'G�Lu��B�PW��m^;�S�����4\MO���?R|J��3%��(�w�P.������b�V;�	���,�Ҭ�z��)uDv�m�v�ii�P%�Ʊ�q@��G���Ը�ȗ���$�{1dc�;*�����8����$y6yf�-5^������sغ/�dR��&F��F}�b�Uתy��6�A^��3�ץ�j�� ��:��2�Q�L<o����?��9N�:���
PD1�l8d���	�K��=c
qG��	��!�U.�*dp���z<P���ą��V���.�(��0�R�'R�@A��{�>�d���8����8;�LB���+���w ��oS�}s��v�	�۵�5�	��x�6Y�?��m����XDqpR��E����Hs����^3���g�ߦGN�W=���q�I���;W��?>�}5�%�ki��L����IO���q�4ِ""�D�Z�K:@c�^ײ;���x)��e�o�HY�?�Q�F�U�j��)�����$s���<�#��d,��� ��pE#�. !��[DC��s�����9$>i����Ψ��W>Ñ�ݿd2��N��CG���ɉ�_4���9��+�d�a���5h=^"��;O���rJ�*���ta"���j���F�;g�nUqLϬs܄�E�r� �ֽ����`L�;�k�_�
��i]a6��O3��e��+��v 	]�JgnԹ��>l��s廉z��z�6CI�锈{�2A�(e�1�\��h�1S�K6�LrE� ����DUTD��{1S>�p<����沍t[Xpp8��4s�Gǫ��� �1?zy��6E3�9jM��$�"R"-����7"Y�Nj5O���J���"�#���|�����GjR=v�IF�>��W�,���t��'�4��LE2��tF� [���.��+q�ux��^<�m���o� �W7�~�k��^�L��9|��L�R� ��s�۷>\�Kx0d����Qn֍p_�oLq�O�by|�ճ�1řD�wF,-���4���p7��������Wox:�zp��!�B֧|)��U�:���x$E&|0����0�N���I�>�#��A#HN%��[�Dt\iS�d����a,�Q*[��#��r�?���Z�F�Xa�找a�pC;�c)��p���yϘ��w�Q�{+1⮅X����x1/t$���I�L6�P/ �! �Ef�9[�tE�^�S8��u�+����D��L�W�����xK\Ġ���6BLyX��\��� #�Dʔ������)�p�q~�
č �k!K�b�}s_?�#�+�R"�L�P�y�0��^�;+B	��9j:U�@_H�ex$�    �j��h�B�p<7V"�y�C���s���U�z1u�dwm�0��oң�q_$ʾBM;9*Ť�mD�?�Č4�s�-��nRr�6��G�/�����@�c��9���s��9��D�ix���t Hm2������~O����qa�bzd��I�2�E�*�|NO#�Ӹ[�?n�8ϕ�-dgxo�$RM�*��	�C���b8ϖ�T�A]�dd����0����r��oMՔ���i�͆����Ȕ���R�ؒrl����m�2
��:��>o�Qo�,�v�`FF�8{{9qj�[�3��U-T��&������Ff�f���ߚ*r��B޺laB6��l7�-��wT��5_'��mn��r�L��� ����x��1e�\�c�&��	)�O� (x����m�$ޜe�p��h<y�nן��ZɗmюEFhJ�Q#Bp9��oMy	G�N~�Ҫa:��tԄ8W-$\�ҡ��[Bur\�V�x$�#p�\��Z'�܋!6@
�C�1��u9߹��c��u�H�ҋ4]R��e�>x��&����r�RM��gj��js!�~ʦ	ΖN=���n�=�3uT{J}��%�GT�D��<�s��p7��kG`v�ڎT�X|?��f�5��3>�I1��#�z�T�j�<�ry4�a�8����M~�/Z ��e������M
�:xFp z�[�3�����%�PPq�G��9
W
�֫)��Q������c��
Ԕ�b��Ps�q�����L�g��pn N�q��́L*��{���S!m%���ҫ)+�Fœ"�&a�[<���NQ"������"3Bq��I���+�)a
C/%�R�,��ٴ�p����C?�xq5�cf���Z�C͑���q�����;5���1	�j	��{p�m�կb"y5�1Q�:�S_H��)�l�>��`<׈�gto�����rUYګ�l�k��q��iD�ve	��U�f*�"�kɡ1߫�����
N(�}M�nNh~@�w�\���}��6K�T�C���sX[�Fh���v.��]�L�#A�%�<_�)k�D����دج�s�-����Lb��\,z^�p'M��R7*X i�ށ���Øw1u���Ë;�t�4�/�I?�E�_�I�S��E���0�g3�6�*�0�\"�,�_H�C�J�==�w9m��i�
�V��&�����O#��6'�)X�����D`�����W���!,�<�7���\9��..W+���L��%)�DU�s��#�czT���-F�׍<��y��"t\s�&�3������B�v��Wm�8A)��ͮ6%�]ח��m$�>L��B�p�J0���M�f^�t]�g���P���?�j=64I�B�f$��e�'1�2m���a�P�i��qA�E�\g�j��$�&� 3om���}��x"* �¡�Me��kĈ�b&��#�n��l�F��-D��;?�n�%�/51Ϗ�� �/����)E 9Z$�E,Zg���8�E!P�u����J���yԱꓝ��[	�M!AW�J�p#. Fѱ�+����?��r�6�75p}L�{��ԣ_S30^ޒ�÷�ڥrIM�xv�S����M#����ׂ�!������GE�KG�L{��v[�D |Z ��r7&��Oc[��,��\	2"�wy�5���������p:��6�M���ș�?�e� �!$�O8i:��ǯF���sEɫ>�=;_��υ���]%�1��!�>Ǳ?�63�4���F4v$�aW���)��F�ₜ�x�|��/�B�ȩ�:\Eր;A:��'�j�p��ر&��_��5.��$>f��>�}f75i�����D��Rښ
G�)R�u'C�t͒w�W[���=������u�>=��ʩ<�K8�Q��V]�k}yq��:8�؎C9)sR�1hg'�in4���oG3&7H�1��C��0�Bn)v�b�n����Yc'֮�,���u[��q�[p���X��ؤ
."��fls��q|c
��"�>j�D�a�5�ĉWβt�ܫ�;�����\��P�N?p�XŪ��]$%��y&./��_e�x���jʴ�q`���NSd��C%ʂ6k��g!��#��ܻ�K��u���Rw8�a;#�"M�)������8�j%�d���l����|d��$�l�pL�y�1�>����I� .�qQ��->/��J���qH�$$}v|��j(��1�36��طOW�H�EVJύ�a��!H�kD�!���7(�t2�I�#�,�=W&���ȃ��Ӑr(||L�x�� 	.Ja�ϓ;��-xnƟ�7����2d<^P�ނ���*y���^͜�����W�u��6���p ���2!�-�w�ik�F���5{�!q�y<@�I�)ED�������2��<O��}��)���n���Aj�Ǎ���w������aXx��*%�J����?E����3�2�޸{59�ڹ ���$��
����O9��s�/6.�Ijd#��k���q��œ�d�=��"}���	k�t|���/�wmOCw��x �n�7�5��)� r�Ps��|��d2؉%�s���r�d�1D�<f|W�e!�W;��P���.���zmۉ��U���v��U��1Rmߓ��Ǣy� \uqXܓ��R0��ñ���igt��J9MPp�tN���D�t����|�YI[�b(3�
H~���T�PA��'������zy��k�L>3�fo��#�`��Թ\�q1&.b��y֠��x��j2�U�%5��d����[�~��_��O��c�E_Cqȸ�Q��q~��tq�����zM�8�B}�����r)�T��L� �/d�d>�7J�Y(���z6Uo:��y��9wn�̮s��W�'�b���/�5����n).�dr��`����\~�6cr�ʖ��KR>��x\{!��l�_NȀ)�!lzF\�.[���s����R�;_(���>��F�W�*�R3�p|�G?���V��?�g[Gؚ�Fr�ԔȊ_��\���{�~��哪1S�悇�p6��>p��42�O��2�񋩃R��h�����qQY)E�,�CI/�"���X�u�vÍD�N\Ҏ\�����ꪸ�~��̍
�}�|.Jp��D�(��Pb]�w2댜�#K�����I����W;�	��:�q���=� ���x�<�H�pI4u8��U����c�]��)�!�Q��|����H>*K�6�����TO��M!=b�$�N��^P/Ǽ��S~]7��	l�����S!�Q�(!���vg9S�
!�Ԋ��'�CvB������$"�$��k���$�u'?S����:Z-cվ��.nJ��n��5g��!6�7#�Ƥ�x̼���E��Ȯ������BbA����6�9�( ��f��D�l������(�/4�ECf���!�ET$sC�k�TxS�tř�V�Ņ�E��G!�j�]�i9��k�1��憮¥v��~b���^ٖ�'!~o���z�=����#9��MyKw��χ����]�?��;]x�ld>A:��20v!�x����U)Ga�+�v^Wټ�9�����9����=��m��#B��\Ni.�\Ma�
���ᴵ�V~�mS<���u���a��b����@�^�Cu�6����r�����8  ���
Q�Np��ܫ�Q)�LS�˔7{=�#��p�[�ßē̔���ܳ�&ǻ)�?)H=rs.��S��H`*7Cf�խ�Lu�N3��L5E��Db�J�L�\w���b��{�Z�8�m<d]g��!6�m��JY�
f��^�X0~6+�ۊ5��ގ�U�w(h�)9s)��h�~5��S���>������5R�~�3���BqO��R�S���{x�| ��b�,)�o� �
B2�	���c!�"�_�BNN�$K�x�%'�;l������=�tK�mcJ�{�pF͊�6Ҡ�[�]����,�-��3	򣕎����;�{����:Wy����V.D�Y�B6_�S��y    ���[H�;$��bEEJY<�������g?2�h����SL���pMe9�!D�9	_�� ]i|��q�0\�Fj���G������(ipmh?*�7�5�r=V=����y����"{��r-�U  ^��w XP�>���$yeS���x��R�d> �*n9�eF�6 ��xX�JH	9�h�D ��܉u�������L>���ǝ��f[��H�n���pc)>�(��������ѰvD����s��gS����nϯ �_���XC@�9�s*K #�!8����������הAj��˼1Y���T�<5�~d�y�6�"du����p�m��iʓ����Gb.�s��*W�󹤢8W)H�CS�J�L�2�c���]L�~8�҂�W g^do�T��A:Y{�|����T8zx<��V̯�%G:�ڷ��uV���z5�mHΧt&�~4GJ��8���
&\"q�6�<Wz�M�|,�f.�H4ʹ�#�aq�:\Y]����E ttp���۪.܅$'�kȩ�_��P?��G��x��" ���Fp����������8㺪�E�7C�լ=)NOd�U�57��8��*� 	\vZ8��Ъ?�B���Ǫ�>L�8)Y�VZ�r�+���/���Xm�ΉK�5&x5_"y�������u�)���ˉ#�PE�+���Bc�-��
�S��=��vE�o�vu�����:>�y�f���Rr��<�[��e B�N��u ���`�1��F���۸�7�r��:1�r��Wq!p�"�/pG;?�J����֣i� �Ix��`;3�0��J�D?؁B2İ��#ܥrp�Ԏ��sK��H����B��T��pAx|5��`�e �:+��5�����)�E�ѹ��*K?�d��\:<��"��'m�繥�Kh}��c>�;����DL�HK�~�O�z�:ӕҪ[�̚_����Ycu�A���L�Bjʹi�x��$�]�Ȓ���`��b��=OV��zH���@¢�딵!��M/�G��j��O����l����~��p�^f����xh[�;�O��ҹ��.�,d��t��T�A��F�����YE�ҩ��Ngc����Q��q($%�"}BX_Zm�;�� M�暯4&�j��(��3�r�yQ��	3�v9_��Uml2�x��wWq8� �~�=�P6��A�:��߂���j�
I��s+=��sL��U�=�ף�_EȝZr6<M13�U&`�~8�l��@��x�N���n�%��w����gB��T&yk԰#�f8o�́F֑���=ɾZ�R��������O��|$� � �w�%E�4ƌ$?kh8ֻZfR��L��R)zL�X�������8R8��x�������m��R�!�%�mďw��i��(p�w�t�A���Z�O�p��g���P�:�HI��y�O.���D4��D��S=?[��wJ&��U.� v��6�#a�h��Ƹ�厖�t�hOM6	���EB)�S ���-}S��L� ���0i!���C)��uе���*�^+��Y'_� Vs�c�0�z҈�'S�U����巟�1����9��λx�@nɤ XĊȃ:.X���t�4d��x���ŜVj��ߩ҅ˏ9v'��������d�>���w�r�	�;iy��g2~�oZ>�	�L��c�����e��A<-k��s��Ƃy���H�֨Z�Xi np�Y&U8R����B����B���SӶ#/E=��!r�uY��M�_�t�1��8S�]P�>�|n�1� r���'�����
�t+ı2��~>��^�� ���e���j����~���te/`\�z ��65�V;�#2z�D���V<�c��:��Fm(�dw��5Hf��$���G�qհq�h��*�t����'���Yv&�@Zʕ!8�G�)p.�X�L��|��(�5F$P�+C��"�m���Rt�k��.���W*G�d���9�֎U�n��E��� 䒆������|`���)m���֕�[������^�y�o5oǴ�*''.aO\U��|�,F���(-i��/OC���]�|\�\u\���&�r�t0�,H���{��;	�C:u< ����V�\�ǝ]��^����v�!�r ��L_&q��+E2�����*�Ȝ{��f�A�y���(;R�pn*�D
��&�*�H�p��D+[���C���O��L�Hl���*Y-nÓ�R*O	5Ğ���o���.��o��qӴu%����"��J�&�w>Ϛ
�U��z�8�W�5�(0�c��Z��f1 <��m;r���j��W
4�a� ����u�5OHV^���;M��I�V[p�*Մ�ݧ��#�����XC��|�����̾9|���������>抟Ů �N��e�ď"T����m�Ӧ���К'���Y[l����%�8����6��᭙z\y���Q�j�F�5�������^��rq�Ő)����cO'���5�n|#�.N[�Op�ZGD�<�cl E�I�fotT%p�('������F�S+��q�1I7�Т���H���8�;;N���`싥���A	�k� ��U�=ud�=9�Z6dSҷ�:L՟��ǖ�#�z��%�o2�;?�O���Eb�g���
�P�(y�hls�nr���m�ZX�&�i&�0Tl���~he�)J�LLJ�2EV[8R��X�MK,��"Y����t�>. �
d�'�ȫ)��ԣ���\����~N� �~Αg�7���hqS�Թ���X�n�=��7
�]�de�MeS�o�6d2�!}8�r�j�4g�Z�]	F��#���*��R�ST=xm��8�q�������8Ja���+��3ǻ�O�9u|jMb�*�[1wnb-�- �����"�'JNm��c��Lf�µҚ��Bf��8%����b����Z��z��H�2B����ZK-n7V��}M�,^�ɩ�J�8RJ(݆'̾Ɍ�Z���WCQq����4
W�� �; ���)�~#��ߌp��RS9=~塳�䞋Sv����D������x�?p��j;$�N[���8s���W�Tԗc�~���z�uD2L����3ǌ[���|Jc�l|	U���K��c���7��l{��U���ȃ�GJ��H��~�2Y���Z=A��Bۈ4x��i��/����>��$_t�MJd�ir^`W�Ľ�R�'+���J)��*�O:Na��@�R�Gjn�ǆb�n��t��C�Pַ�kO�X��.^�%�=��U$s��o����?M}ҋҹ�VIݒ��!���}H�����
��prUH	ǅ���n
,��P�/3=	�����3K%�W
Uj��,$��A�iv�[��s˃)3N���UgU��s3�v��9z�7�rF��x5�nhr���]R��\��Ū�7���3 ��� ���i*��,����Q�ھ�Ț�g�Y�L���#��cY��X9Q�G�HR�xJ�P;���Mg�B��<��it���t�O|~��F�T6�U�Eq�l�ۛ_��������,S���7����$W�ti��Ք��;�*m�!h��g��V� �l�8�[�O�z�BLU�ñ�;��H�ڕ9NJI������T�k|�����\���>$V};������W�K�u�U^ME�B� �Q�K��s��L	� ���Vp��S2�P�y���擯�[c�d|�$n�WI�]{�9R ��u�����V���*�/c��!�9�c�����h���'��*2_�j*��Ej��6��5<��!e)������	Nq�r�5�Č�.QC��f���D��Y�+�.�J_as����㒲r�Hy�a�tT�Y�H�h� xc!Q�"���`����T��iL�	!�|N�.0b�O����_�M���9 �q�����Ww�N8�_������'n��V�=�>Yk���8�K��)��n���q&�o-��H��ǆ�%��Q�}��It��v�G�����N��x1Ą�C~��V-�S��բ7~���R-~�X��    Տ!�Ѥj�g��z��lqwb�
B���pF�*yҋ�oS֬�<���+�>���7^�K�Ĩ�
:�n#����(���|9q�H��� H�B��7�7����ڰ|�L�_��9b��_..�Sz�Q�~�2zaG-�Ǧe�Y������k�۹���S����
���������c8<�|��P�J7U�RǸ��3q��m���C��_S�����C��"{1��9�L�%
�l�H������M'!>f<�*��i��Ώ�«L�Ҵ;�2Ivӥ�b�NQOB�;�]]�Ԗ�����{�T��>K����`�˗)RD��}:f�s%���3#C��DX_���\�*����x�a6���t>�00��&���@ⱳl��8�Z��sZ��l��~?�D�!����%��8�_�@��y�]��eJ��!� 
{=���!G��42#԰/ T*$�FmY.m8x1��܃�▃ό����8���:���ULlC���Ԁ[�H��7����5 $�2ߔ�2n~v�*���Q�q��˱���QE�_����#�#�d>�F9�q�͐F��p��/n� _GvGnZ�_����,7)��;SUH2{�XT}O��>)@Pgⱐ�ה���!�GK����F�c?�D�@q�βv�G��)�Bt/#S�z�E=�c��c��"�G�`K�t�G��M���,�I�A�<�E�i�h69��<`-=�R°��.�㕃 ��l� ��#-�Dfx~��$�W����;GM���3]IɄ�)F1¤��^�����C^��p���z�y8�����\�C���D[r�%�>_M�f<��]��XO�����E�"��=�lek�r��dTkD�y�Lt�C}�Zʺ��Bi��_xM�q�f'�!���������h�,b {����x�e��H�z�܀�ok.,�0�'��k��)��$�_SF�@���٩j9P�I9�#2��8�&۫�ʬ7�-{ �N-Hd��%�dW���c�Ӕj�~������y
��O'�v����m���������"sҬ��"�i�Y���Yi��1�Bj�Fz�?:�A�K���S$���U��h��`��zG��!#<DRw�R�f�;vX�.]�i�	d�B���T0B<I�Og�����8*� tZ�ͪ j*�`*i&~5%�\o�w���qD}PwI�hi+&4n�f�Žw�}Osr�P3���@�	n� �n.v�V��p�w�p�
C���B��B�>�[�����p��������� 1ЩzRA��Y�l��x�,!45)�E1���a���;��{�$�M�L�r�:��!6����&��9������p&�XA1��6'"�����M�o��e�؀�5ع��&~��J���ǖ/.X�;d��ť�긔�+fdH��8 ��
��"X�A>I]8{��9��ɑ���7y�?��\_����=�i,-��IR�$��Ι�A��������M�k/xu�]x<�a	|�r�/��6g��[}t~��X 4�N��|��6�y�H�vJ̻[@<~܋d��[~�[�Gua�FY�ϔ� ReA���ICv�M���'.��O�T�F~��3'��.�%�J!��"#C�gH|O"lë����� �/�p��a�0[����Dυ���&|	�B5,ٳ�_�oH�zM��c�7�>Rqf�W����@c�  R�����X�n���_��K���v�O[N�1ܐȐ-2.�3��J�Ȁ�e��/^J�;$��L�J��F�
D�Lyg汸c�L�<�c[#�YNZn���;����M�ydM��ё���!Sv��P�Γtt��������)�dә�����hkZ�C�W�kf�bE?����}��N���
W=�$O����-�oL�����_(�"�{��<}O%+_����
����Ѧ��N� 2�ۚZ�W��M�X{D=k�"EBx9�M
�H��.�z��C�鷛�f�n�%���f"�>QE��D�uv��sE�����jG�^Mer����g[��W�dt"%V��d+��pN�����Zϋ��!�h�1����>���4��Vm�Sf[Ss�9wG��7E�_�Qׅx�umJ��]H.Xܷ�lS��-븐rQŦ�d-�%]C���P>�Քb����ŧN�mV��IB���Ar�휓���rt@T@ޤ��ž8��yĥbo�B��S\7"8�\c�T�o�:�O��݌z�E7�\b�8M'���t���O�|�lU:z��=�M����G:z���0�&-)"ې��=zqa׸_:�>���ɱ�����FMF��
t+z�)!d[�	׻a�X�N�ieߛ$�����#<�r�Ik\��o�,����߿׮�N���YA���k�"J��u���ү�-�TN�x�!Z:��M��3�\�[ ����fHŊ��,�#gBp��\��nl%eo��ӝ)�����8q6�4��O�G�@䃧T ��y���U��j2�a)׶�yů
yM�z��CD z9�Al�����OW��%#L�"o�x��jG��r���Qc�ȭi�LP��������9R�7�z~��UK��m�42������u��\�����e;�.ij���-������ �����⩽C<$8��5Pe�%��J��MQu��5�Z^ϛ)�|���y}�jW�pՋ '©��w#1}�?��*c��TU9/�Z���4P%���	X���	��+�e�P�g��h���Y�Ĥ��K�4�!�d�!\��'���+�7�v�����m����ɸ��U���`��"��\�C���{����V"���>6 U�x�}�z$*U���?]-����Kџ��֥tk��21�u�wN�����@�8��<�4�k����]S�̓r�R���央��xk!��O]A��bu$�#��U�Ԓs��C�4ę��������c�V�mC�\���=�m�O���="���������Z{�-\*��^������2��1w|5��M� �(&LrN��{�����r5=\=�Xc%8�y)9��b?*�ɀ})�H�;~fG���������$˓�L�^%"�\;�ـ��sGɔ� |�xN�x��|��o_us�>��'���w}@�D~���/����-�Γ)�o��"H	�该�򘡴�F�Hr�I������Fj�� �>�6������v1����L��;J��,+�N�0�����}d��~ۅc������?�[E�ꐘ����-</��'S3��Q����9�Ǳ5�x7���p-����j�J�/ϧV<����f��4� ��! ��w���&��] ��+�q��
Ķ���'d ��d�Dz1D
,��Pb��L�&E�L�`qp���TG2ϗ7'�����;�*��e
Ǚ��)�LV�ߜ�|���Y�H���ϗ�q�V�/+r��]c
���na)��:���Ji�:K�	��"\�ˁ�s��A�RX���`F�m
u^�{�q�)�.��XT�I�fr�d�ڪ/wԊVl<�-���t6jr����ʽy�qu�B�}:��3���F�qY�O_��WY��Şz𩴯餌��I6�-��K9n�8��9r��9ޡg"?r�3>PHb2�Y4�'���ԋ��)�wz	��䞋�d7 �#O�6�����po��to��vZ8Q�U_�$��P�N|�j'�"���&����̸���B�qN'���?������k4V���lot�H��I�����P���Jp0.�!Rh���B�g1$����Q�-�]k5�cO��A2z���c�����nNȀ���H���Qe
PD�_
��<K&�K��w|��UWIY��lx!_)j��*\
y"-�ǥ3����*=MRq 4��4O�}��
e?��f�^ԇ�p�̜I��m�w�����ۧ��S��2Fw,��4��p�g�V#�������7�bȶ�q��9}�T�&�9��Au����p�a	�/ſ�*�    �Ǽ	�	�`�P��>xyy7'I�<�,3�������<�إ�U��wlN9ېȈ�{�>ɪ��t����͓D�nseq]���xٵ%w�)�g�.��+��-����^�����Gb(|R���b�3�c�;Y��E�j,��乿�41`j}��&s��YFX=�B2�Q8�H����4��F�Q�/���CF�y}l��bs�����f �YQ"
���oSF��pZ�{9�ڕ��	�$'����q��|W������jf\$�ԕ�|���·���Y��f��ٺU��@vB�KE!����\N�<�/P$N��+e���!|Å#������k� ��ə����i�ı�SC��nH�7��9w��{�5���^R�H��9�����/���f�`��"ܒ��G��袱�%�#���5w[\w$�A�~�M���bv�8/� Fb��+����G�F�{@B�d�k�֨p_�U?F�53YK;�����W�<���U�
�4;��K>���`�
7#@��Xd�XW��"�P����L��!i���:�R���@��lȲU�c.�D	�_�Z��ԗ(3��G�U��-�l�!�T�>���5B���丙��TƵ$AY���
 �������L�є��A[H��EY�mT�h�]c8l�8X�bȂB���'(Q>v�6��I��}no
B�����1[=�~4�zL��G���W7�)��")�T ����4��Ba�����b�^Ʋ�dN�	�c��~ny����S���#�v��j��N��Sk���	.y�ܓ�2��TV����pIc���,:ҷpq�p7��g{@�MQ9��2!}�gs�ta�7ę�A���vc6�������4�l�]�_�� A��b����D��p��\p�D:E��t��WA�ls�9�Vn�<��,��Q��7_�l �0ɕk۝�U%d��0��s��k�k�G=b��� ����>cS����HM�B���6z���6�?:)�l�N͗�E��&A�0�~���W�ED=�m��Nc*�?�;D��݀/<��v�+#��s����pq�S�A(�I��/�R(7��<)'8 �����{�X{��.�y�j,Ԅ�f���L�H�.���+��.��]�i�2�����D����h֡҆l�HُCW��췺8#^�{p3G&%�,l�q�+�2�����8o�Y��>v��*�)�W�Ȏ��It�BJI��〓69�|��b��ɂ�sF�5� x_bD�R)�ń{m�� ��G�+&T� �1���rِDp��LO8׮��a���#6ܦ�M91s�}��c��l�ePx�½U�;��4�),W�Ct�y��9���%Pz�0;5���u�^�#6�lA:�?f8�b{np�����+ܫH��&H����$TS+�x˽�cR�q�\M�<)�
i�ݩ���қ�`��.r��d��*
�Udċp�HL�57�+b-�G�?k��*'�OJ1��� ��*ZmU#�����,8�\���W�Q���k�qv\au;r�s�C.$�)l�+껽<��x�4�I� ��B�=r����$?>�J2���m&�@�q��-�=��n#��z@ �ɓqbᦅWS�J5���+e5���@��:]���}�^��Ηo3�m�Z���w8��}n�5`$B��:�܈y�a間
����.�����G��e�
4�Z�wv��OP�j�M �o�G�j!ŀ�!Q�S,U���s�C=�~�W"���י�I2-�Kp���bl��E���3�zau���e.w�\H��u�o
i�����ף.�9�p��𶼾��	Z�B"�Ќ|��|o��7��2�����x�<�jx�p$^��OSl��I8�Q��"̉L2��:)��y ��D+��
�\��� �(��685D���1MN�eڳ���ߘ��".Es����h	�f~��K�7�T#��I�׷:Em�0N��T_�5���W��/���/S����_�W������[s�Rf��=��z��S�đJ������i�:v�"�ڵp�3�����m�2�˷�ayi����`�Q��Z�4�^��,���`$�ⱟCF��#WK�I�v�ė+o��3�5KX�Y�w�#���H�g�\eX�X�,pC,�ZK 8�W��C���]�;[��a�6�E�m��;�֜3�����od�m��ui[�Ik��I�����^�d�_����ƣ.�q>��F��5Q`�����Y~�B&dVB:�$h��gVW<:��+�����6�����4{�*��n��PtSnR��j樗���������s"�u#B:�n�J�=84���3`�P �'�E�_�F�O����xǒc�H�HkM�֊+�C"Y��$V����ŔЙfM)��OB���A�E�Dr�_x���ݾ�'q�ǆ����QY����w!�Z�2��\/��ՔIj�H�w�!�N�L���Y����ح/�Mm�
x_���յ�#�SH�/D��T����dq���rv�� ��7�|��Ӆ�n`{��]����P%��u�\ !߃�*㢰�Tf����GZ��C�F	0⇺� �q��m�$�p�-���9�?�!��XҌL\���V�	MYM m�s��Wis6f~��B�����/�p�����..��C8�7�F 3LS�dNy1�x9_�_0�9D�Ne����+�� \fРy���h��*���kAR���N�� ,:n�wt]��Q�f8�,�	p ;�,ɋ���F-���b/b��%e��5�����?@c��!����:0���	}�s�dr�[�_��!��^ �[�6�o3j*PT�;�j]&�
IȞ��b`+r���� �Y:�{�-m��k�@0\8�C;��5�櫡>��tB�P�6�����s���J-�C1�<s�$�S�i"�"��}�Ww��ޮk_�Ő�!�[�_s�mk/~�@b��a�	Q�?��9,zj����rf;�h�6	T=�[k篝��N`yx=.����G��'C�m㳖����R�Q�A}�\�7^\�a��+��J�J$���v&M���ү`$ǐ����FO�u��N��v%G�Ka���,�bD�F*w2摀;��qp�^�mK�c��rU'�>U���=Ζ���9x��ZG�!R�}�nC�4ѫ�Ӓ��5WN�9vW
s�v��#7���r�9O⹀��'�R�an��T|�ʡ�+}�����P�!_����Mb߀!n�,����Z8�Jar$��ĩ��Ԙ�1O��IC3�3.y��8�^����뚏�@�x�m�z�&�p�SَP��>*7�\��?Rd3��"�I�#�c�p!:�3"��䆕�=�F��G���L�D���䀟�I(�A�dqd�My��=�P��g�y{--d��ڠB�r���-��Ǚ���G����/�S��$ ���#���.S�t��i3�H.�0w���:�\k�{؈u9̅[���w=���8Z�	�$�
���{F�Ld��G��%ݥ����^���.)[}��m����m8$��<[8m�#�eL՗�Y���R�]N����G�UKZr$d��Sj�|͒�F��2 5;�2�Z#���Fd����Y�9�9�YX
F�^�m�?^y�_����q� �x< N�� ;��r��c�r\C!�"�!򈋫�8�@!�I�����>!���G���#%��d�I����:�[�E���fe�s��U�8v�h{V��V��G�����%5j=g�7���ss+ �q�8�F7JfoS|��<����*��DI�:BF.��d��ꋱ�!;�q�T_H�=�g?�p�x���H���BS�n<�pz7,;��8D[�І���EMNq�s��(^ux�c#�l���QC�)�c��S�c�$W��)\����L��ÝF�% \tA�0>�E֕�+q���H�'��1T�Y�?�X�2U���;{�-u%�>�g���HW�g��=�����JJ�FV�RJ9^�%[�j!L    �+9x_��6oC�A��!��Ž�b���cq��\��
�^:�T�>����ɦD�mG������x��ܩK�-����	�`d�����J~�^I��DAdȣKd#SAl��;���ļ��~�cp��Z{��%y9�9��ӢGOSw�F�=e�7y���F�+]\��o�b�L��ArGU�0\G^,׎�?-�T#�nȓ�}��h�di�G��&V��G� 퍛k��vWa&���R�[�x1� c�E�m��x"|�u��:�����ߘ�q(�﨎���%佱�B�,؆��'�x�A��;ж��m�4+RcO:v&��ۀR�r�����p��A�@	x玡IJ9���͠jq���v$�~6�G�P���$�G�"/� �pʐ-����um�C�|�Q�:����2"�^;��@a�f���� �-жJ��T��+�RN]]�:�{$*d�֗*���g�S+gR����|jn�q�~!�D\���������ߦ2{_�?�r�{XM�0#[��*��ӌ�a�Ro���q�9��X(��w��
��oW��6[�/3�iЏ��c���卵.��E��.��(�}�_��M��Ѵ~OG���,ul�w��sh�h��Np�����=S H:
y"���y�t�y��y HT7�c(���1C��O@�r�90I�1�"�(��9aԃ� ��9�x7�����(B��P	Y
C�V� �]�����5���l����$�����Hٜ�p��2r�H#'Y�Mq7V�gPGo�O��տ�o�3�D��,�IK#�ވ��~q����(�b�I��@u?�|��-�x&���'g��z��E;R(LObr� *Pw<;ć�b�<c.td���Y����F�\���]m&��k�8� �E������#��U�I\B��n�{���H������'ٞs]�5C��Έo2�lGʇ�:�C
����g�Y��zL��g��Z�2Mk :B�6�F$Kv}t:��%|�;"�����q��Ձ2�=ǭ�iH\[�ϩ3BIf�����T(�Z�,U5�?l@�t+�۷��m�DWɋ|jHZ�����=�>R�\A�"�OC�(����V�b[E (����57�+K�B�ӽ^|\4��թS(�s��s�7�d�l"y�|��"��θ�ت8�P�4���T�3M7�h���y�`�܈��8~��3��<w�:���ti�5�l>����y��X�?6�w
�_3Yx�rO{8�<�n]�᳓<�O���S�0)��q�D��q5p)"n���z��Jm�>�cn��x~Vk�PJ�Ε+�&NƧ�'"�{�P�⸙;C#|	We�9$�&��	������iK>��N����R�*\!���Րc����\�� Rq�c2�ࡹ�m*�HXɐ&��t�&߆�M��gL�w驵z	��#��Jǳ������Ȑ���ȝX�8bÒ"��pҙi����	9�zy2 Q̚�]��>�J�[�yp�/N�E�w�M�f���i����	����T�c�w�������>�((����@�6 �g�C �Dh-��d���1������O���달���{�m�})7��\!��7.t��XPD�|�ya�ꁷK���	��.�uy-8[�2�8dj�C�ιԋ�˱���0��3�$���~�ur�EFG�Ų2
��Cd��(���B+7�(���!������<���p�y*ٵz�����1]&
�3���k
��0��FI��+�ڴ2p�[��������=����p����7.,ޜ���-S��s9.Zl_p��-�<x���z�nJт����	�S}.A��!p{׈���tAuE�H��xy �U��v2����C�ȗI�F�o���bP��o1�{-Nna��Ў�7��oOu���S��f�U ��FM\���~hgvy)�k�� K�k9M�.��<��%2�Q!��I�YsndJ���Q�Td�Yt����n"��R�����\�'#�3i tC&�#{i�z|�{�����)�!^���)�����peR*RR{�Բ�DA�����+�4YI)����Twp��R_E�6�MZ���k���k��!)c���!�lڗ�'��ZQ��:1�%-[�[B�N�_N���eF�s�i�q������&L��+���I���A���_�8m�Iޞ���bU ��O�%*N�꒦�T^M�����Y�J�޴8�N�F�����i_���E���G��G���lc7}S��4����,��1��0ؠ$	B<��Ky;G��١���nN�e�G>q��_�mWB��\��ѣ�� �yvR{��{$��>H+����ɕ�n�X��ʸ�j-����b�vW�U�c��ꬆp�Y'��������X�{��cc�HO���qvk9*�{�7n���/�t`*�Ӈ���`�M��_� �48T��)0^aHs�!�&��OCц�c��';�5f�j�^:RoD ؝Hh�C6()�}�G�^u�Y�43����f�L
g�\0.�i��M4�,����$ɒ��k�]"��h���G�?f��+㗌��*�J�pp�����740��bQRV��<�oL�� DM�}��,��܉��W"��g[���������3�鏓��� U:<]�c�jc���@�Ü\����y��c �P��N�U��g]�mȖ���?yt6,��� H�ؐe��`Be�ݿ��SW�4�u�G�϶�2ma+����4>�w~�o3.p�\��u筄�������C�T9�z�h��MD��!��}�`��o�w���1�D���V\�׮�}zKL˂3O6/�5Zjufԕ߆���{���`�5���	�#���) o-��V߇o�S���\�cϦy���)p�%��X�o���J��ѧI׃�fӤ(`�}��K:����r�u����SU����Y!�U���D��ذ����/tdL����#(�,�����:���A��D~�p�J��huBg�aεP�P8R�	����4��<����ܷi<��2��v��;Nǧ/H�~�ౢ�\�_k�-�7�A���um���p/���r����M_W�Y�Q��U%�BAQ:��Ey]/�~p:Uܱ|	�k��l[�I9.2�&�{����]���=��0��j���Bvb��:�!�	�ǅZf���)��ׅ���M�e���_/\�'�r�T�}9մ�Uy�O*MLV&�&�b?����$�P~5ϺՒ֖|�� ��D����2���۹^�����M���;�S��ד�iZE3����	����� ?@=}�:����9n#��6*�q��*�B]J��2v�}0�P�/�Y.c�N���Yv=s���I�������ª����RH���k��04�!\����LbT.5[��IG�������'h��Ӽ�P\pON~ ��aJ��z��3q��6��+0s/� ����d1�l3�Tm\���ߙ*�]��(��v�6�m�Q�R�WS���t���p�N���Ne��݀��AMQQ�*9����/���=�A^��~M[�3UUu��$pz�� �0u�n͇w������hY��9�#"�K�E�L^GG�S�����g������肺Vohu�1����/�~�m����QXT�p,1�ohg����~4��pe�TW�H�$�a��.Z�]�I�	6�nOg��o#џ-?Zr׳�aT���)���H���UU�><���З�KI7�g�����c��:;#,�,cP��u˔����,+�%whadڶ�-�U��U�u�φ����{<�7m��ᆀmG?*�)���G����W��?S�$�&7��Kq���x��,��و$n������o#'�*9�SO���5�=�����������8;2M���c�n��u��+�1-F���
�(�3w��|��y]�4܉+*�1�>k�˅�v�`�g%�R�;D���c*=��'D4� �_C���̌I_ֱ�O�!��-<o�6[�@:� �  L�lA����J�2��:��:'^�kL�����t���T��=����f@*��Y��o�`��T��<a�b����P kᣩ3��K�W��]tErg�Li@C}�Dbo��9_��hd�݊��5�J6��7��͵�@��&������:	�6ɪ8���PN�:��p[�D�ç�u�}�q���j��]qh���K��'ժ�ͨ.�N+�dF|y[>l�L���a%V�:�^H�����˪w�C��m�T��x;�y�Aư�7��e
:E�.��s��*x~��`S��S��k�H��C�Nз!*%W�O9�\��d�����b6�L#�L�ac���ܙ�//(�>� tYm�gYfЫyX�ж�I��b\�}�ޔ`d�yp��**~NxA�B��W2Ͳ���R8[+�Nv ��b��)��q/�!e����}\�f�����GS��{��Ë�G���L�Sůi���*z_�����:���:��b3�y<zX*�®}���&[�T�1�Xw7��F�T�&���ֻڪO��ۿ1#ԥ����nW�:�]�m������AӫuTH�w�(�6O�ײ&{U4�#>�Ȳ���~@cCi��<?��g#)1ޔ%��sA\)�e1ayĨ
�;���Y�/������ʯ��5	�C�d�&��E��u�������5&wE��i��ذ܇�	���$LN/Oi�׵�o#���6��%�fL�C�١?
*����<�
����oC�Ri��F����Cи���J+a1'^� ����s߆~�
	��n^��k8j8�·M|e�ak�v�_I�>��1s1�C�8��e��Ҭ*Z~U"��}����a��ϭR�J�r��N��-��M����
��U5W�*��zQ-�T��n%�^����O�,��˨6ͷ9��Ҍ�1`�#`��Q���e	���`���:-������j��%V��=�-VF�g�oC�T�9sZC(�AVV�jF��
�5e��Gh�m�W6R\��ɥ��3�\�~d$��n������0�F���&Ɖ��U�QQՒ_y�������%}]��UG��	�y9��M��c�m��:���t�C;[OL�
��%��Gf��:���(ꏾ^�BG᚜���{]�@�{m�M���f�Ҷ����e�s���-��|�����p�      �   �  x���MS�H��ү�����d˲��nh��a�D8�"��V K}�x~�<Y%��fc��A++�2�7�2�<q.Fc�<��w�T��U��M��,_�b�F�|8�˷�H����F�덜/#����lU��2�JU몬YB<�[�4�J�˲ި{]o������|璝�uV��ƶN�&I[����'�3?՛�9�>Ik����YgM[�[���1*Fx]��e"�"z���[����B�S��_U�˪*0̐����V%��wf]x�"���F�E��v��-z�\����:�q>0ݯ�Bb]�wnP{��\����\%uk��'H�μ����ω�=�I��S��H��IQh�!�3��(cϚ���64eW�܏�ΌT��J���ȹM�6�
p+��"n���'��r�ǵ�� ?�r(+\bpCHIٵ.�!z�I�
����*� ���J�Y�Cy�&�!7��~��J׍;���r��2��� �8R%�V5ݳqdX?_�"=���1ie�@!��V ��`���\�s3i�c�DHć�oe�hſe��N�Fdb�,Y:FP��N�w2��XԬ#`�ч�$]j�@��9)�Φ�3s#� pP����g�ʓb�q� ����^�� ��})�|QڤY N6�sHr���E��KI� ,P+��$��ˍq5 �^�eY�@`�!�u� ���\à;T�̚� ������u�t8��K�hց�8�=w-``��.W��60�5�i���Xۀ5�r������Vo�C�kS�Mm&�6���S@M�T��.m�)����Fk
���zCMR�ĐΉ���[S�ʗ�;z|��#�m	)�\�˚#�R7�nHr�/�<�C.	�ܦԫ֕�C�q�)�!:��{�n"����jM��Y�3��p�K���ށȹ�ůI�oU����O�ug���ߺ.�s��jp��;9��JH�w���y���d�θd�]E�t��)UO�l���J�_����K�\'��*���;X!�JU����������1$5�V�q3D:�/{~{Թ��b���f(�Csg�3GI������u�'�G��fՓtN�@
}n�#����Fc|G�-��M��V�|x��l���ڍ�Ȏ�Gi�kDa�|	�/"�K���Z�>�F��}ߠ�Sm�%[ׇL����ϕ��C�mCf��#[����`)�T����ۭ.��Y���u�HO�r,&�]]K�x����AY��#�cX�~���G�Mo�+���W��K���S��lL7�v��!��HLi3�������5�ꔇ���r%U�p�`E�D���2·�~�a*�������'v�Es;�����Ħ.J�>����t�)��NG;�@��3�{w��"�cZ�a�\Y+X���h`j��٘�������g�L;��C�끆~GѸ{Qw�����(�,���fA�$q�dˡvy����#���i��4�o&Z���})�4B;����=��=k�R���b* ��"}�*�8�VS��<���s29�L�T�Q��=�4�>��!��]�B�C���3O<�e.v ���g�&��3.��Q�W��)�����2�ᣰ���{L�~11�Z䜛���S��`��r�A�!6|gF0�i=|��v&SseI����K���\��n
��*��x��gB�QG'�~��	����^��$����3��@�K3�����Q�.YΆ�v(�y�xõ�[H.�Z��36L�lm��?24lk 6����^�[D>�X�3�OҮ���m�����6�W�n���V�����XG��Hi�R��A�@kh�>9��)���%kHS+$Bo�ʌ�G�U5DUP���$o�~��.��=�V����vMZ��]�ȃs�'O_/�]A�&�����aP[n{�7�2{٪d#�%	�A�2�-b��֟&^v͉�nq�!��}�*�Q�Ǥ��dR&�x>�cw�}�*(F;E���U}�0u�藓��P/D��'@﷐�Kzij삶w��mV��]Y�d�ia��=�2s'����@��:ou�0���'Z{"eJQ��n��^�l�Q�E=���i`�*��B�::��*�Ӽ������Z�У���B({>j�����jB�>?�4�Baz��y<{3a89Z�}`�^����j�s�����w���W�}I�(y >�%�����&�/f��Wxۏ��Ǭ���A��$H#��ÑV�xҍv#�Qy&��RX��s��?��w      �      x������ � �      �   G   x�33���t��Qp��II-*�,�4204�50�50V04�20�2��362573��/k �X������ rF�      �      x������ � �      �      x������ � �      �   �  x�}��n�0�g�)�b�GI�/�)y�,�ӘB,�	� �LE�!c7EQ�k��20/�7)��V�8���#%�/���>[��P�}�Ł+*��+8k��vM�~�EQ��m�}ssv8�}�}w��7��%,�����O8XߕqB=�4��/�qҏ�d�	�ل�Dh��l�P��c��p\��E��p2�N3)�4�Q��HJ󜸖�9����Nc��cΖl�n����å��ml���n��V���CN�hw&���&�o��iC�r>U�^�(���)�ҔG���~:?.�o��!��:���_�t{þ-Z��Z��w��#����y��M�Rq���2�g��B���>90��b	;�bD���j^�?3�mp��x���
_!Ո"��E#�4�S�T�9J�$����M      �   �   x�3204�50�52W00�#N���̇�g*dޜ�����pwc�B^b�B��
��wOLV(y�k�BNb��w��K�2��f�0�(/�R�,�Qj���4���L3�50D�fJ�if(��c����������K�22����zS]C����/�H-JI��ʚ�"����� ��*�&V*p��qqq 

�	      �   :  x���Kn� �5>���f���dc�ɣ�#5��޾�Q�MR�� ����	bd��U�V�
YBh ��AdS]?�]Y���>��1�Ѯf&�0���ZH�#b9ye&�#e�u�jK��*Cq��Axs�2�%J��c�sJ�|�%�~���06kk\�7����!�M��hϪEmU�Ⱦ�r��+��3��=n�V8�\:��e��}�}��^�}����0ȅ��,k"D�7+�m�0���9��9�D��r��y�[g�p��-����'4t���d�M�[��������R�t�������6`�� :�9]�����UU}�؞n      �      x������ � �      �      x�����0C����u[�!��3����/qA�)��e
z��E���ӯ�x59B�Ъ�9�L���>B�p&��.�&������g�nnpj�A��:'|��t+��Ǧ�62�T���9�t\<      �     x�e��n�0���S�r�Jd;vN�'�4����
H@U��@WvP.����:�C���?4�z�q<�l�c�MWjr�p9hk���F��$x![�5J"�2�\�j�R�mʭ=��T.O�ZK	ϻo�G���S3_ǵ�ҢPT����.�\�.�Q
4dgk�gZȜ�����vj�K�k�U$	9Q41k,�sdI�&��G��[ٲ�����M����lݴ^gW�Db�;���xmrBkl 	+jPs;�v���IB�p>C@~]{SU���j�      �   e  x���An� �ur��@*l0=K�=A�z�I���)R�/�$��i�M�G�Hr��H�3%�ƌd���~vݘ�ׂ�H��2� c��xs<�
p�\'�L�K_� � � �f�?�P��eo5��f#Ύ���)@���ީږ�v� ��g���<h��w<P�j^@�˄W�G]��Z�Ha��,�a�<qu|�0D�E�CT��/ }nY�A���皞8�Op� \f��J�[,�8���E��a���bY���B�ݢ�&��U_S���%}���5Z_r����5g��egp�2͸8~���p�W�p���8;�[��K��%b���Iq|�������/`��     