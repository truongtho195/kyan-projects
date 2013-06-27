PGDMP         
                q            pos2013    9.0.3    9.0.3    �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
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
                  postgres    false    7            )           2612    11574    plpgsql    PROCEDURAL LANGUAGE     /   CREATE OR REPLACE PROCEDURAL LANGUAGE plpgsql;
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
       pgagent       postgres    false    6    553            �           0    0     FUNCTION pga_exception_trigger()    COMMENT     p   COMMENT ON FUNCTION pga_exception_trigger() IS 'Update the job''s next run time whenever an exception changes';
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
       pgagent       postgres    false    6    553            �           0    0 #   FUNCTION pga_is_leap_year(smallint)    COMMENT     W   COMMENT ON FUNCTION pga_is_leap_year(smallint) IS 'Returns TRUE is $1 is a leap year';
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
       pgagent       postgres    false    6    553            �           0    0    FUNCTION pga_job_trigger()    COMMENT     M   COMMENT ON FUNCTION pga_job_trigger() IS 'Update the job''s next run time.';
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
       pgagent       postgres    false    6    553            �           0    0 �   FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[])    COMMENT     �   COMMENT ON FUNCTION pga_next_schedule(integer, timestamp with time zone, timestamp with time zone, boolean[], boolean[], boolean[], boolean[], boolean[]) IS 'Calculates the next runtime for a given schedule';
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
       pgagent       postgres    false    6    553            �           0    0    FUNCTION pga_schedule_trigger()    COMMENT     m   COMMENT ON FUNCTION pga_schedule_trigger() IS 'Update the job''s next run time whenever a schedule changes';
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
       pgagent       postgres    false    553    6                        1255    234863 7   checkserialnumber(character varying, character varying)    FUNCTION     %  CREATE FUNCTION checkserialnumber("partNumber" character varying, "serialNumber" character varying) RETURNS boolean
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
       public       postgres    false    7    553                        1255    285708    clearalldata(character varying)    FUNCTION     s	  CREATE FUNCTION clearalldata(username character varying) RETURNS void
    LANGUAGE plpgsql
    AS $_$
DECLARE
    statements CURSOR FOR
        SELECT tablename FROM pg_tables
        WHERE tableowner = username AND schemaname = 'public';
BEGIN
    FOR stmt IN statements LOOP
        EXECUTE 'TRUNCATE TABLE ' || quote_ident(stmt.tablename) || ' CASCADE;';
    END LOOP;

INSERT INTO "base_Configuration"(
            "CompanyName", "Address", "City", "State", "ZipCode", "CountryId", 
            "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", 
            "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", 
            "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", 
            "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", 
            "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", 
            "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", 
            "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", 
            "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", 
            "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", 
            "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", 
            "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", 
            "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", 
            "IsAllowRGO", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", 
            "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", 
            "ReturnFeePercent", "IsRewardLessThanDiscount", "CurrencySymbol", 
            "DecimalPlaces", "FomartCurrency", "PasswordLength")
    VALUES ('Smart POS Company', '', '', 0, 0, 0, 
            '', '', '', '', '', 0, 
            '', '', true, 
            false, false, false, 
            false, 5, 0, 
            0, 0, '', 
            '', 12, now(), 
            'admin', 1, false, 0, 
            0, 3, null, 0, 
            0, 0, 0, false, 
            8, 1, 0, false, 
            7, false, 'EN', 10, 
            true, true, 1, '', 
            false, false, false, 0, 
            false, false, true, 
            0, false, '$', 
            2, 'en-US', '((?=.*[^a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?!\s).{8,})');

END;
$_$;
 ?   DROP FUNCTION public.clearalldata(username character varying);
       public       postgres    false    7    553                        1255    234864    newid()    FUNCTION     �   CREATE FUNCTION newid() RETURNS uuid
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
       pgagent       postgres    false    6    1745            �           0    0    pga_exception_jexid_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE pga_exception_jexid_seq OWNED BY pga_exception.jexid;
            pgagent       postgres    false    1746            �           0    0    pga_exception_jexid_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('pga_exception_jexid_seq', 1, false);
            pgagent       postgres    false    1746            �           1259    234870    pga_job    TABLE     �  CREATE TABLE pga_job (
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
       pgagent         postgres    false    2163    2164    2165    2166    2167    6            �           0    0    TABLE pga_job    COMMENT     .   COMMENT ON TABLE pga_job IS 'Job main entry';
            pgagent       postgres    false    1747            �           0    0    COLUMN pga_job.jobagentid    COMMENT     S   COMMENT ON COLUMN pga_job.jobagentid IS 'Agent that currently executes this job.';
            pgagent       postgres    false    1747            �           1259    234881    pga_job_jobid_seq    SEQUENCE     s   CREATE SEQUENCE pga_job_jobid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE pgagent.pga_job_jobid_seq;
       pgagent       postgres    false    1747    6            �           0    0    pga_job_jobid_seq    SEQUENCE OWNED BY     9   ALTER SEQUENCE pga_job_jobid_seq OWNED BY pga_job.jobid;
            pgagent       postgres    false    1748            �           0    0    pga_job_jobid_seq    SEQUENCE SET     9   SELECT pg_catalog.setval('pga_job_jobid_seq', 1, false);
            pgagent       postgres    false    1748            �           1259    234883    pga_jobagent    TABLE     �   CREATE TABLE pga_jobagent (
    jagpid integer NOT NULL,
    jaglogintime timestamp with time zone DEFAULT now() NOT NULL,
    jagstation text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobagent;
       pgagent         postgres    false    2169    6            �           0    0    TABLE pga_jobagent    COMMENT     6   COMMENT ON TABLE pga_jobagent IS 'Active job agents';
            pgagent       postgres    false    1749            �           1259    234890    pga_jobclass    TABLE     U   CREATE TABLE pga_jobclass (
    jclid integer NOT NULL,
    jclname text NOT NULL
);
 !   DROP TABLE pgagent.pga_jobclass;
       pgagent         postgres    false    6            �           0    0    TABLE pga_jobclass    COMMENT     7   COMMENT ON TABLE pga_jobclass IS 'Job classification';
            pgagent       postgres    false    1750            �           1259    234896    pga_jobclass_jclid_seq    SEQUENCE     x   CREATE SEQUENCE pga_jobclass_jclid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_jobclass_jclid_seq;
       pgagent       postgres    false    6    1750            �           0    0    pga_jobclass_jclid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_jobclass_jclid_seq OWNED BY pga_jobclass.jclid;
            pgagent       postgres    false    1751            �           0    0    pga_jobclass_jclid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobclass_jclid_seq', 5, true);
            pgagent       postgres    false    1751            �           1259    234898 
   pga_joblog    TABLE     v  CREATE TABLE pga_joblog (
    jlgid integer NOT NULL,
    jlgjobid integer NOT NULL,
    jlgstatus character(1) DEFAULT 'r'::bpchar NOT NULL,
    jlgstart timestamp with time zone DEFAULT now() NOT NULL,
    jlgduration interval,
    CONSTRAINT pga_joblog_jlgstatus_check CHECK ((jlgstatus = ANY (ARRAY['r'::bpchar, 's'::bpchar, 'f'::bpchar, 'i'::bpchar, 'd'::bpchar])))
);
    DROP TABLE pgagent.pga_joblog;
       pgagent         postgres    false    2171    2172    2174    6            �           0    0    TABLE pga_joblog    COMMENT     0   COMMENT ON TABLE pga_joblog IS 'Job run logs.';
            pgagent       postgres    false    1752            �           0    0    COLUMN pga_joblog.jlgstatus    COMMENT     �   COMMENT ON COLUMN pga_joblog.jlgstatus IS 'Status of job: r=running, s=successfully finished, f=failed, i=no steps to execute, d=aborted';
            pgagent       postgres    false    1752            �           1259    234904    pga_joblog_jlgid_seq    SEQUENCE     v   CREATE SEQUENCE pga_joblog_jlgid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE pgagent.pga_joblog_jlgid_seq;
       pgagent       postgres    false    1752    6            �           0    0    pga_joblog_jlgid_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE pga_joblog_jlgid_seq OWNED BY pga_joblog.jlgid;
            pgagent       postgres    false    1753            �           0    0    pga_joblog_jlgid_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('pga_joblog_jlgid_seq', 1, false);
            pgagent       postgres    false    1753            �           1259    234906    pga_jobstep    TABLE       CREATE TABLE pga_jobstep (
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
       pgagent         postgres    false    2175    2176    2177    2178    2179    2181    2182    2183    2184    6            �           0    0    TABLE pga_jobstep    COMMENT     ;   COMMENT ON TABLE pga_jobstep IS 'Job step to be executed';
            pgagent       postgres    false    1754            �           0    0    COLUMN pga_jobstep.jstkind    COMMENT     L   COMMENT ON COLUMN pga_jobstep.jstkind IS 'Kind of jobstep: s=sql, b=batch';
            pgagent       postgres    false    1754            �           0    0    COLUMN pga_jobstep.jstonerror    COMMENT     �   COMMENT ON COLUMN pga_jobstep.jstonerror IS 'What to do if step returns an error: f=fail the job, s=mark step as succeeded and continue, i=mark as fail but ignore it and proceed';
            pgagent       postgres    false    1754            �           1259    234921    pga_jobstep_jstid_seq    SEQUENCE     w   CREATE SEQUENCE pga_jobstep_jstid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE pgagent.pga_jobstep_jstid_seq;
       pgagent       postgres    false    1754    6            �           0    0    pga_jobstep_jstid_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE pga_jobstep_jstid_seq OWNED BY pga_jobstep.jstid;
            pgagent       postgres    false    1755            �           0    0    pga_jobstep_jstid_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('pga_jobstep_jstid_seq', 1, false);
            pgagent       postgres    false    1755            �           1259    234923    pga_jobsteplog    TABLE     �  CREATE TABLE pga_jobsteplog (
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
       pgagent         postgres    false    2185    2186    2188    6            �           0    0    TABLE pga_jobsteplog    COMMENT     9   COMMENT ON TABLE pga_jobsteplog IS 'Job step run logs.';
            pgagent       postgres    false    1756            �           0    0    COLUMN pga_jobsteplog.jslstatus    COMMENT     �   COMMENT ON COLUMN pga_jobsteplog.jslstatus IS 'Status of job step: r=running, s=successfully finished,  f=failed stopping job, i=ignored failure, d=aborted';
            pgagent       postgres    false    1756            �           0    0    COLUMN pga_jobsteplog.jslresult    COMMENT     I   COMMENT ON COLUMN pga_jobsteplog.jslresult IS 'Return code of job step';
            pgagent       postgres    false    1756            �           1259    234932    pga_jobsteplog_jslid_seq    SEQUENCE     z   CREATE SEQUENCE pga_jobsteplog_jslid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE pgagent.pga_jobsteplog_jslid_seq;
       pgagent       postgres    false    6    1756            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE pga_jobsteplog_jslid_seq OWNED BY pga_jobsteplog.jslid;
            pgagent       postgres    false    1757            �           0    0    pga_jobsteplog_jslid_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('pga_jobsteplog_jslid_seq', 1, false);
            pgagent       postgres    false    1757            �           1259    234934    pga_schedule    TABLE       CREATE TABLE pga_schedule (
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
       pgagent         postgres    false    2189    2190    2191    2192    2193    2194    2195    2196    2198    2199    2200    2201    2202    6            �           0    0    TABLE pga_schedule    COMMENT     <   COMMENT ON TABLE pga_schedule IS 'Job schedule exceptions';
            pgagent       postgres    false    1758            �           1259    234953    pga_schedule_jscid_seq    SEQUENCE     x   CREATE SEQUENCE pga_schedule_jscid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE pgagent.pga_schedule_jscid_seq;
       pgagent       postgres    false    1758    6            �           0    0    pga_schedule_jscid_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE pga_schedule_jscid_seq OWNED BY pga_schedule.jscid;
            pgagent       postgres    false    1759            �           0    0    pga_schedule_jscid_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('pga_schedule_jscid_seq', 1, false);
            pgagent       postgres    false    1759            �           1259    244946    base_Attachment    TABLE       CREATE TABLE "base_Attachment" (
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
       public         postgres    false    2260    2261    2262    7            �           1259    244944    base_Attachment_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Attachment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Attachment_Id_seq";
       public       postgres    false    7    1778            �           0    0    base_Attachment_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Attachment_Id_seq" OWNED BY "base_Attachment"."Id";
            public       postgres    false    1777            �           0    0    base_Attachment_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_Attachment_Id_seq"', 40015, true);
            public       postgres    false    1777            !           1259    256168    base_Authorize    TABLE     �   CREATE TABLE "base_Authorize" (
    "Id" bigint NOT NULL,
    "Resource" character varying(36) NOT NULL,
    "Code" character varying(10) NOT NULL
);
 $   DROP TABLE public."base_Authorize";
       public         postgres    false    7                        1259    256166    base_Authorize_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Authorize_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Authorize_Id_seq";
       public       postgres    false    1825    7            �           0    0    base_Authorize_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Authorize_Id_seq" OWNED BY "base_Authorize"."Id";
            public       postgres    false    1824            �           0    0    base_Authorize_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_Authorize_Id_seq"', 360, true);
            public       postgres    false    1824                       1259    254557    base_Configuration    TABLE     �
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
    "PasswordFormat" character varying(70),
    "KeepBackUp" integer DEFAULT 0
);
 (   DROP TABLE public."base_Configuration";
       public         postgres    false    2347    2348    2349    2350    2351    2352    2353    2354    2355    2356    2357    2358    2359    2360    2362    2363    2364    2365    2366    2367    2368    2369    2370    2371    2372    2373    2374    2375    2376    2377    2378    7            �           0    0 .   COLUMN "base_Configuration"."DefautlImagePath"    COMMENT     T   COMMENT ON COLUMN "base_Configuration"."DefautlImagePath" IS 'Apply to Attachment';
            public       postgres    false    1805            �           0    0 9   COLUMN "base_Configuration"."DefautlDiscountScheduleTime"    COMMENT     k   COMMENT ON COLUMN "base_Configuration"."DefautlDiscountScheduleTime" IS 'Apply to Discount Schedule Time';
            public       postgres    false    1805            �           0    0 (   COLUMN "base_Configuration"."LoginAllow"    COMMENT     \   COMMENT ON COLUMN "base_Configuration"."LoginAllow" IS 'So lan cho phep neu dang nhap sai';
            public       postgres    false    1805            �           0    0 5   COLUMN "base_Configuration"."IsRequireDiscountReason"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRequireDiscountReason" IS 'Reason box apprear when changing deactive to active status';
            public       postgres    false    1805            �           0    0 -   COLUMN "base_Configuration"."DefaultShipUnit"    COMMENT     f   COMMENT ON COLUMN "base_Configuration"."DefaultShipUnit" IS 'Don vi tinh trong luong khi van chuyen';
            public       postgres    false    1805            �           0    0 +   COLUMN "base_Configuration"."TimeOutMinute"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."TimeOutMinute" IS 'The time out application';
            public       postgres    false    1805            �           0    0 *   COLUMN "base_Configuration"."IsAutoLogout"    COMMENT     U   COMMENT ON COLUMN "base_Configuration"."IsAutoLogout" IS 'Combine to TimeOutMinute';
            public       postgres    false    1805            �           0    0 .   COLUMN "base_Configuration"."IsBackupWhenExit"    COMMENT     ]   COMMENT ON COLUMN "base_Configuration"."IsBackupWhenExit" IS 'Backup when exit application';
            public       postgres    false    1805            �           0    0 )   COLUMN "base_Configuration"."BackupEvery"    COMMENT     R   COMMENT ON COLUMN "base_Configuration"."BackupEvery" IS 'The time when back up ';
            public       postgres    false    1805            �           0    0 (   COLUMN "base_Configuration"."IsAllowRGO"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsAllowRGO" IS 'Is allow receive the quantity more than order quantity';
            public       postgres    false    1805            �           0    0 2   COLUMN "base_Configuration"."IsAllowNegativeStore"    COMMENT     V   COMMENT ON COLUMN "base_Configuration"."IsAllowNegativeStore" IS 'Cho phép kho âm';
            public       postgres    false    1805            �           0    0 +   COLUMN "base_Configuration"."IsRewardOnTax"    COMMENT     q   COMMENT ON COLUMN "base_Configuration"."IsRewardOnTax" IS 'T: SubTotal - Discount + Tax
S: SubTotal - Discount';
            public       postgres    false    1805            �           0    0 6   COLUMN "base_Configuration"."IsRewardLessThanDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Configuration"."IsRewardLessThanDiscount" IS 'T: Cho phep ap dung reward khi Reward < Discount
F: Canh bao va khong cho phep ap dung reward';
            public       postgres    false    1805            &           1259    257302    base_Configuration_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_Configuration_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_Configuration_Id_seq";
       public       postgres    false    1805    7            �           0    0    base_Configuration_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_Configuration_Id_seq" OWNED BY "base_Configuration"."Id";
            public       postgres    false    1830            �           0    0    base_Configuration_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_Configuration_Id_seq"', 3, true);
            public       postgres    false    1830            Z           1259    283360    base_CostAdjustment    TABLE     �  CREATE TABLE "base_CostAdjustment" (
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
    "UserCreated" character varying(100),
    "IsReversed" boolean DEFAULT false,
    "StoreCode" integer,
    "Resource" character varying(36),
    "Status" smallint NOT NULL,
    "Reason" smallint NOT NULL
);
 )   DROP TABLE public."base_CostAdjustment";
       public         postgres    false    2590    2591    2592    2593    2594    2595    2596    2597    7            �           0    0 -   COLUMN "base_CostAdjustment"."CostDifference"    COMMENT     Q   COMMENT ON COLUMN "base_CostAdjustment"."CostDifference" IS 'NewCost - OldCost';
            public       postgres    false    1882            �           0    0 &   COLUMN "base_CostAdjustment"."NewCost"    COMMENT     S   COMMENT ON COLUMN "base_CostAdjustment"."NewCost" IS 'AdjustmentNewCost*Quantity';
            public       postgres    false    1882            �           0    0 &   COLUMN "base_CostAdjustment"."OldCost"    COMMENT     T   COMMENT ON COLUMN "base_CostAdjustment"."OldCost" IS 'AdjustmentOldCost*Quantity
';
            public       postgres    false    1882            �           0    0 3   COLUMN "base_CostAdjustment"."AdjustCostDifference"    COMMENT     k   COMMENT ON COLUMN "base_CostAdjustment"."AdjustCostDifference" IS 'AdjustmentNewCost - AdjustmentOldCost';
            public       postgres    false    1882            Y           1259    283358    base_CostAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CostAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_CostAdjustmentItem_Id_seq";
       public       postgres    false    7    1882            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CostAdjustmentItem_Id_seq" OWNED BY "base_CostAdjustment"."Id";
            public       postgres    false    1881            �           0    0    base_CostAdjustmentItem_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_CostAdjustmentItem_Id_seq"', 95, true);
            public       postgres    false    1881            P           1259    271738    base_CountStock    TABLE     �  CREATE TABLE "base_CountStock" (
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
       public         postgres    false    2569    2570    7            �           0    0 !   COLUMN "base_CountStock"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_CountStock"."Status" IS 'Get from "CountStockStatus" tag in XML';
            public       postgres    false    1872            R           1259    271745    base_CountStockDetail    TABLE     j  CREATE TABLE "base_CountStockDetail" (
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
       public         postgres    false    2572    2573    2574    2575    7            �           0    0 +   COLUMN "base_CountStockDetail"."Difference"    COMMENT     W   COMMENT ON COLUMN "base_CountStockDetail"."Difference" IS 'Diff = Counted - Quantity';
            public       postgres    false    1874            Q           1259    271743    base_CountStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_CountStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_CountStockDetail_Id_seq";
       public       postgres    false    7    1874            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_CountStockDetail_Id_seq" OWNED BY "base_CountStockDetail"."Id";
            public       postgres    false    1873            �           0    0    base_CountStockDetail_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_CountStockDetail_Id_seq"', 187, true);
            public       postgres    false    1873            O           1259    271736    base_CountStock_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_CountStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_CountStock_Id_seq";
       public       postgres    false    7    1872            �           0    0    base_CountStock_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_CountStock_Id_seq" OWNED BY "base_CountStock"."Id";
            public       postgres    false    1871            �           0    0    base_CountStock_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_CountStock_Id_seq"', 32, true);
            public       postgres    false    1871                       1259    245340    base_Department    TABLE       CREATE TABLE "base_Department" (
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
       public         postgres    false    2307    2308    2309    2310    2311    2312    2313    7            �           0    0    TABLE "base_Department"    COMMENT     ,   COMMENT ON TABLE "base_Department" IS '

';
            public       postgres    false    1798            �           0    0 "   COLUMN "base_Department"."LevelId"    COMMENT     8   COMMENT ON COLUMN "base_Department"."LevelId" IS 'ddd';
            public       postgres    false    1798                       1259    245338    base_Department_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_Department_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_Department_Id_seq";
       public       postgres    false    7    1798            �           0    0    base_Department_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_Department_Id_seq" OWNED BY "base_Department"."Id";
            public       postgres    false    1797            �           0    0    base_Department_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Department_Id_seq"', 423, true);
            public       postgres    false    1797            �           1259    238237 
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
       public         postgres    false    2203    2204    2205    2206    2207    2208    2209    2210    2211    2212    2213    7            �           0    0 %   COLUMN "base_Email"."IsHasAttachment"    COMMENT     p   COMMENT ON COLUMN "base_Email"."IsHasAttachment" IS 'Nếu có file đính kèm thì sẽ bật lên là true';
            public       postgres    false    1761            �           0    0 $   COLUMN "base_Email"."AttachmentType"    COMMENT     [   COMMENT ON COLUMN "base_Email"."AttachmentType" IS 'Sử dụng khi IsHasAttachment=true';
            public       postgres    false    1761            �           0    0 &   COLUMN "base_Email"."AttachmentResult"    COMMENT     y   COMMENT ON COLUMN "base_Email"."AttachmentResult" IS 'Sử dụng khi IsHasAttachment=true và phụ thuộc vào Type';
            public       postgres    false    1761            �           0    0    COLUMN "base_Email"."Sender"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Sender" IS 'Thông tin người gủi dựa và GuestId';
            public       postgres    false    1761            �           0    0    COLUMN "base_Email"."Status"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Status" IS '0: Outbox
1: Inbox
2: Sent
3: Draft
4: Trash';
            public       postgres    false    1761            �           0    0     COLUMN "base_Email"."Importance"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."Importance" IS 'Message Option
0: Normal
1: Importance
';
            public       postgres    false    1761            �           0    0 !   COLUMN "base_Email"."Sensitivity"    COMMENT     [   COMMENT ON COLUMN "base_Email"."Sensitivity" IS 'Message Option
0: Personal
1: Bussiness';
            public       postgres    false    1761            �           0    0 '   COLUMN "base_Email"."IsRequestDelivery"    COMMENT     o   COMMENT ON COLUMN "base_Email"."IsRequestDelivery" IS 'Message Option
Request a delivery receipt for message';
            public       postgres    false    1761            �           0    0 #   COLUMN "base_Email"."IsRequestRead"    COMMENT     g   COMMENT ON COLUMN "base_Email"."IsRequestRead" IS 'Message Option
Request a read receipt for message';
            public       postgres    false    1761            �           0    0    COLUMN "base_Email"."IsMyFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsMyFlag" IS 'Custom Reminder Active Flag For Me';
            public       postgres    false    1761            �           0    0    COLUMN "base_Email"."FlagTo"    COMMENT     >   COMMENT ON COLUMN "base_Email"."FlagTo" IS 'My Flag Options';
            public       postgres    false    1761            �           0    0 #   COLUMN "base_Email"."FlagStartDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagStartDate" IS 'Active My Flag Date';
            public       postgres    false    1761            �           0    0 !   COLUMN "base_Email"."FlagDueDate"    COMMENT     I   COMMENT ON COLUMN "base_Email"."FlagDueDate" IS 'DeActive My Flag Date';
            public       postgres    false    1761            �           0    0 %   COLUMN "base_Email"."IsAllowReminder"    COMMENT     L   COMMENT ON COLUMN "base_Email"."IsAllowReminder" IS 'Allow remind my flag';
            public       postgres    false    1761            �           0    0    COLUMN "base_Email"."RemindOn"    COMMENT     X   COMMENT ON COLUMN "base_Email"."RemindOn" IS 'My Flag is going to remind on this date';
            public       postgres    false    1761            �           0    0 #   COLUMN "base_Email"."MyRemindTimes"    COMMENT     H   COMMENT ON COLUMN "base_Email"."MyRemindTimes" IS 'The reminder times';
            public       postgres    false    1761            �           0    0 $   COLUMN "base_Email"."IsRecipentFlag"    COMMENT     S   COMMENT ON COLUMN "base_Email"."IsRecipentFlag" IS 'Custom Reminder For Recipent';
            public       postgres    false    1761            �           0    0 $   COLUMN "base_Email"."RecipentFlagTo"    COMMENT     L   COMMENT ON COLUMN "base_Email"."RecipentFlagTo" IS 'Recipent Flag Options';
            public       postgres    false    1761            �           0    0 -   COLUMN "base_Email"."IsAllowRecipentReminder"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."IsAllowRecipentReminder" IS 'Allow remind Recipent Flag';
            public       postgres    false    1761            �           0    0 &   COLUMN "base_Email"."RecipentRemindOn"    COMMENT     f   COMMENT ON COLUMN "base_Email"."RecipentRemindOn" IS 'Recipent Flag is going to remind on this date';
            public       postgres    false    1761            �           0    0 )   COLUMN "base_Email"."RecipentRemindTimes"    COMMENT     Z   COMMENT ON COLUMN "base_Email"."RecipentRemindTimes" IS 'The Reminder Times of Recipent';
            public       postgres    false    1761            �           1259    238137    base_EmailAttachment    TABLE     p   CREATE TABLE "base_EmailAttachment" (
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
       public         postgres    false    2216    2217    2218    2219    2220    2221    2222    2223    2224    2225    2227    2228    2229    2230    2231    2232    2233    2234    2235    2236    2237    2238    2239    2240    2241    2242    2243    7            �           0    0    COLUMN "base_Guest"."GuestNo"    COMMENT     <   COMMENT ON COLUMN "base_Guest"."GuestNo" IS 'YYMMDDHHMMSS';
            public       postgres    false    1766            �           0    0     COLUMN "base_Guest"."PositionId"    COMMENT     >   COMMENT ON COLUMN "base_Guest"."PositionId" IS 'Chức vụ';
            public       postgres    false    1766            �           0    0     COLUMN "base_Guest"."Department"    COMMENT     =   COMMENT ON COLUMN "base_Guest"."Department" IS 'Phòng ban';
            public       postgres    false    1766            �           0    0    COLUMN "base_Guest"."Mark"    COMMENT     [   COMMENT ON COLUMN "base_Guest"."Mark" IS '-- E: Employee C: Company V: Vendor O: Contact';
            public       postgres    false    1766            �           0    0    COLUMN "base_Guest"."IsPrimary"    COMMENT     ^   COMMENT ON COLUMN "base_Guest"."IsPrimary" IS 'Áp dụng nếu đối tượng là contact';
            public       postgres    false    1766            �           0    0 '   COLUMN "base_Guest"."CommissionPercent"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."CommissionPercent" IS 'Apply khi Mark = E';
            public       postgres    false    1766            �           0    0 )   COLUMN "base_Guest"."TotalRewardRedeemed"    COMMENT     o   COMMENT ON COLUMN "base_Guest"."TotalRewardRedeemed" IS 'Total reward redeemed earned during tracking period';
            public       postgres    false    1766            �           0    0 2   COLUMN "base_Guest"."PurchaseDuringTrackingPeriod"    COMMENT     `   COMMENT ON COLUMN "base_Guest"."PurchaseDuringTrackingPeriod" IS '= Total(SaleOrderSubAmount)';
            public       postgres    false    1766                        0    0 /   COLUMN "base_Guest"."RequirePurchaseNextReward"    COMMENT     �   COMMENT ON COLUMN "base_Guest"."RequirePurchaseNextReward" IS 'F = RewardAmount - PurchaseDuringTrackingPeriod Mod RewardAmount';
            public       postgres    false    1766                       0    0 '   COLUMN "base_Guest"."IsBlockArriveLate"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBlockArriveLate" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0 '   COLUMN "base_Guest"."IsDeductLunchTime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsDeductLunchTime" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0 '   COLUMN "base_Guest"."IsBalanceOvertime"    COMMENT     O   COMMENT ON COLUMN "base_Guest"."IsBalanceOvertime" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0 !   COLUMN "base_Guest"."LateMinutes"    COMMENT     I   COMMENT ON COLUMN "base_Guest"."LateMinutes" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0 $   COLUMN "base_Guest"."OvertimeOption"    COMMENT     L   COMMENT ON COLUMN "base_Guest"."OvertimeOption" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0 #   COLUMN "base_Guest"."OTLeastMinute"    COMMENT     K   COMMENT ON COLUMN "base_Guest"."OTLeastMinute" IS '-- Apply to TimeClock';
            public       postgres    false    1766                       0    0    COLUMN "base_Guest"."SaleRepId"    COMMENT     C   COMMENT ON COLUMN "base_Guest"."SaleRepId" IS 'Apply to customer';
            public       postgres    false    1766                       1259    245376    base_GuestAdditional    TABLE        CREATE TABLE "base_GuestAdditional" (
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
       public         postgres    false    2315    2316    2317    7                       0    0 $   COLUMN "base_GuestAdditional"."Unit"    COMMENT     K   COMMENT ON COLUMN "base_GuestAdditional"."Unit" IS '0: Amount 1: Percent';
            public       postgres    false    1800            	           0    0 .   COLUMN "base_GuestAdditional"."IsTaxExemption"    COMMENT     N   COMMENT ON COLUMN "base_GuestAdditional"."IsTaxExemption" IS 'Miễn thuế';
            public       postgres    false    1800            
           0    0 .   COLUMN "base_GuestAdditional"."TaxExemptionNo"    COMMENT     a   COMMENT ON COLUMN "base_GuestAdditional"."TaxExemptionNo" IS 'Require if IsTaxExemption = true';
            public       postgres    false    1800                       1259    245374    base_GuestAdditional_Id_seq    SEQUENCE        CREATE SEQUENCE "base_GuestAdditional_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_GuestAdditional_Id_seq";
       public       postgres    false    7    1800                       0    0    base_GuestAdditional_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_GuestAdditional_Id_seq" OWNED BY "base_GuestAdditional"."Id";
            public       postgres    false    1799                       0    0    base_GuestAdditional_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestAdditional_Id_seq"', 112, true);
            public       postgres    false    1799            �           1259    244863    base_GuestAddress    TABLE     �  CREATE TABLE "base_GuestAddress" (
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
       public         postgres    false    2245    2246    2247    7            �           1259    244861    base_GuestAddress_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestAddress_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestAddress_Id_seq";
       public       postgres    false    1768    7                       0    0    base_GuestAddress_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestAddress_Id_seq" OWNED BY "base_GuestAddress"."Id";
            public       postgres    false    1767                       0    0    base_GuestAddress_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestAddress_Id_seq"', 257, true);
            public       postgres    false    1767            �           1259    238413    base_GuestFingerPrint    TABLE     3  CREATE TABLE "base_GuestFingerPrint" (
    "Id" bigint NOT NULL,
    "GuestId" bigint NOT NULL,
    "FingerIndex" integer NOT NULL,
    "HandFlag" boolean NOT NULL,
    "DateUpdated" timestamp without time zone DEFAULT now() NOT NULL,
    "UserUpdaed" character varying(30),
    "FingerPrintImage" bytea
);
 +   DROP TABLE public."base_GuestFingerPrint";
       public         postgres    false    2214    7            �           1259    238411    base_GuestFingerPrint_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestFingerPrint_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestFingerPrint_Id_seq";
       public       postgres    false    1763    7                       0    0    base_GuestFingerPrint_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestFingerPrint_Id_seq" OWNED BY "base_GuestFingerPrint"."Id";
            public       postgres    false    1762                       0    0    base_GuestFingerPrint_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestFingerPrint_Id_seq"', 12, true);
            public       postgres    false    1762            �           1259    244873    base_GuestHiringHistory    TABLE     Q  CREATE TABLE "base_GuestHiringHistory" (
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
       public         postgres    false    2249    7            �           1259    244871    base_GuestHiringHistory_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestHiringHistory_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."base_GuestHiringHistory_Id_seq";
       public       postgres    false    1770    7                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE "base_GuestHiringHistory_Id_seq" OWNED BY "base_GuestHiringHistory"."Id";
            public       postgres    false    1769                       0    0    base_GuestHiringHistory_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('"base_GuestHiringHistory_Id_seq"', 1, false);
            public       postgres    false    1769            �           1259    244884    base_GuestPayRoll    TABLE     �  CREATE TABLE "base_GuestPayRoll" (
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
       public         postgres    false    2251    2252    2253    7            �           1259    244882    base_GuestPayRoll_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestPayRoll_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestPayRoll_Id_seq";
       public       postgres    false    7    1772                       0    0    base_GuestPayRoll_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestPayRoll_Id_seq" OWNED BY "base_GuestPayRoll"."Id";
            public       postgres    false    1771                       0    0    base_GuestPayRoll_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_GuestPayRoll_Id_seq"', 1, false);
            public       postgres    false    1771            (           1259    257325    base_GuestPaymentCard    TABLE     Z  CREATE TABLE "base_GuestPaymentCard" (
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
       public         postgres    false    2400    2401    7            '           1259    257323    base_GuestPaymentCard_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_GuestPaymentCard_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."base_GuestPaymentCard_Id_seq";
       public       postgres    false    7    1832                       0    0    base_GuestPaymentCard_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE "base_GuestPaymentCard_Id_seq" OWNED BY "base_GuestPaymentCard"."Id";
            public       postgres    false    1831                       0    0    base_GuestPaymentCard_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_GuestPaymentCard_Id_seq"', 14, true);
            public       postgres    false    1831            �           1259    244922    base_ResourcePhoto    TABLE        CREATE TABLE "base_ResourcePhoto" (
    "Id" integer NOT NULL,
    "ThumbnailPhoto" bytea,
    "ThumbnailPhotoFilename" character varying(60),
    "LargePhoto" bytea,
    "LargePhotoFilename" character varying(60),
    "SortId" smallint DEFAULT 0,
    "Resource" character varying(36)
);
 (   DROP TABLE public."base_ResourcePhoto";
       public         postgres    false    2255    7            �           1259    244920    base_GuestPhoto_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_GuestPhoto_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_GuestPhoto_Id_seq";
       public       postgres    false    1774    7                       0    0    base_GuestPhoto_Id_seq    SEQUENCE OWNED BY     L   ALTER SEQUENCE "base_GuestPhoto_Id_seq" OWNED BY "base_ResourcePhoto"."Id";
            public       postgres    false    1773                       0    0    base_GuestPhoto_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_GuestPhoto_Id_seq"', 247, true);
            public       postgres    false    1773            �           1259    244934    base_GuestProfile    TABLE     �  CREATE TABLE "base_GuestProfile" (
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
       public         postgres    false    2257    2258    7            �           1259    244932    base_GuestProfile_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_GuestProfile_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_GuestProfile_Id_seq";
       public       postgres    false    1776    7                       0    0    base_GuestProfile_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_GuestProfile_Id_seq" OWNED BY "base_GuestProfile"."Id";
            public       postgres    false    1775                       0    0    base_GuestProfile_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestProfile_Id_seq"', 162, true);
            public       postgres    false    1775            >           1259    268354    base_GuestReward    TABLE     �  CREATE TABLE "base_GuestReward" (
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
       public         postgres    false    2499    2500    2501    2502    7                       0    0 '   COLUMN "base_GuestReward"."AppliedDate"    COMMENT     Z   COMMENT ON COLUMN "base_GuestReward"."AppliedDate" IS 'Ngay ap dung chuong trinh reward';
            public       postgres    false    1854                       0    0 '   COLUMN "base_GuestReward"."ActivedDate"    COMMENT     �   COMMENT ON COLUMN "base_GuestReward"."ActivedDate" IS 'Ngay bat dau reward co hieu luc
Active Date = EearnedDate + Block Day After Earn.
Status = Pending';
            public       postgres    false    1854                       0    0 "   COLUMN "base_GuestReward"."Status"    COMMENT     g   COMMENT ON COLUMN "base_GuestReward"."Status" IS 'Available = 1
Redeemed = 2
Pending = 3
Removed = 4';
            public       postgres    false    1854            =           1259    268352    base_GuestReward_Id_seq    SEQUENCE     {   CREATE SEQUENCE "base_GuestReward_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."base_GuestReward_Id_seq";
       public       postgres    false    7    1854                       0    0    base_GuestReward_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE "base_GuestReward_Id_seq" OWNED BY "base_GuestReward"."Id";
            public       postgres    false    1853                       0    0    base_GuestReward_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_GuestReward_Id_seq"', 3717, true);
            public       postgres    false    1853                       1259    256013    base_GuestSchedule    TABLE     �   CREATE TABLE "base_GuestSchedule" (
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
       public       postgres    false    7    1766                        0    0    base_Guest_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Guest_Id_seq" OWNED BY "base_Guest"."Id";
            public       postgres    false    1765            !           0    0    base_Guest_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('"base_Guest_Id_seq"', 297, true);
            public       postgres    false    1765            �           1259    244997    base_MemberShip    TABLE       CREATE TABLE "base_MemberShip" (
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
       public         postgres    false    2264    2265    7            "           0    0 %   COLUMN "base_MemberShip"."MemberType"    COMMENT     f   COMMENT ON COLUMN "base_MemberShip"."MemberType" IS 'P = Platium, G = Gold, S = Silver, B = Bronze.';
            public       postgres    false    1780            #           0    0 !   COLUMN "base_MemberShip"."Status"    COMMENT     Z   COMMENT ON COLUMN "base_MemberShip"."Status" IS '-1 = Pending
0 = DeActived
1 = Actived';
            public       postgres    false    1780            �           1259    244995    base_MemberShip_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_MemberShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_MemberShip_Id_seq";
       public       postgres    false    1780    7            $           0    0    base_MemberShip_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_MemberShip_Id_seq" OWNED BY "base_MemberShip"."Id";
            public       postgres    false    1779            %           0    0    base_MemberShip_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_MemberShip_Id_seq"', 1, false);
            public       postgres    false    1779            @           1259    268511    base_PricingChange    TABLE     �  CREATE TABLE "base_PricingChange" (
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
       public         postgres    false    2503    2505    2506    2507    7            ?           1259    268509    base_PricingChange_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PricingChange_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PricingChange_Id_seq";
       public       postgres    false    7    1856            &           0    0    base_PricingChange_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PricingChange_Id_seq" OWNED BY "base_PricingChange"."Id";
            public       postgres    false    1855            '           0    0    base_PricingChange_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingChange_Id_seq"', 533, true);
            public       postgres    false    1855            <           1259    268185    base_PricingManager    TABLE       CREATE TABLE "base_PricingManager" (
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
       public         postgres    false    2495    2496    2497    7            (           0    0 %   COLUMN "base_PricingManager"."Status"    COMMENT     u   COMMENT ON COLUMN "base_PricingManager"."Status" IS '- Pending
- Applied
- Restored

-> Get From PricingStatus Tag';
            public       postgres    false    1852            )           0    0 (   COLUMN "base_PricingManager"."BasePrice"    COMMENT     H   COMMENT ON COLUMN "base_PricingManager"."BasePrice" IS 'Cost or Price';
            public       postgres    false    1852            *           0    0 .   COLUMN "base_PricingManager"."CalculateMethod"    COMMENT     j   COMMENT ON COLUMN "base_PricingManager"."CalculateMethod" IS '+-*/
- Get from PricingAdjustmentType Tag';
            public       postgres    false    1852            +           0    0 )   COLUMN "base_PricingManager"."AmountUnit"    COMMENT     D   COMMENT ON COLUMN "base_PricingManager"."AmountUnit" IS '- % or $';
            public       postgres    false    1852            ,           0    0 (   COLUMN "base_PricingManager"."ItemCount"    COMMENT     W   COMMENT ON COLUMN "base_PricingManager"."ItemCount" IS 'Tong so product duoc ap dung';
            public       postgres    false    1852            ;           1259    268183    base_PricingManager_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_PricingManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_PricingManager_Id_seq";
       public       postgres    false    7    1852            -           0    0    base_PricingManager_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_PricingManager_Id_seq" OWNED BY "base_PricingManager"."Id";
            public       postgres    false    1851            .           0    0    base_PricingManager_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PricingManager_Id_seq"', 46, true);
            public       postgres    false    1851            
           1259    245412    base_Product    TABLE     �  CREATE TABLE "base_Product" (
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
    "Location" character varying(200),
    "QuantityOnCustomer" integer DEFAULT 0 NOT NULL
);
 "   DROP TABLE public."base_Product";
       public         postgres    false    2318    2320    2321    2322    2323    2324    2325    2326    2327    2328    2329    2330    2331    2332    2333    2334    2335    2336    2337    2338    2339    2340    2341    2342    2343    2344    7            /           0    0 &   COLUMN "base_Product"."QuantityOnHand"    COMMENT     b   COMMENT ON COLUMN "base_Product"."QuantityOnHand" IS 'Total From OnHandStore1 to OnHandStore 10';
            public       postgres    false    1802            0           0    0 '   COLUMN "base_Product"."QuantityOnOrder"    COMMENT     a   COMMENT ON COLUMN "base_Product"."QuantityOnOrder" IS 'Total quantity on "Open" purchase order';
            public       postgres    false    1802            1           0    0 $   COLUMN "base_Product"."RegularPrice"    COMMENT     I   COMMENT ON COLUMN "base_Product"."RegularPrice" IS 'Apply to Base Unit';
            public       postgres    false    1802            2           0    0    COLUMN "base_Product"."Price1"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price1" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1802            3           0    0    COLUMN "base_Product"."Price2"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price2" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1802            4           0    0    COLUMN "base_Product"."Price3"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price3" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1802            5           0    0    COLUMN "base_Product"."Price4"    COMMENT     a   COMMENT ON COLUMN "base_Product"."Price4" IS 'Price = RegularPrice - RegularPrice*MarkDown/100';
            public       postgres    false    1802            6           0    0 !   COLUMN "base_Product"."OrderCost"    COMMENT     F   COMMENT ON COLUMN "base_Product"."OrderCost" IS 'Apply to Base Unit';
            public       postgres    false    1802            7           0    0 '   COLUMN "base_Product"."AverageUnitCost"    COMMENT     L   COMMENT ON COLUMN "base_Product"."AverageUnitCost" IS 'Apply to Base Unit';
            public       postgres    false    1802            8           0    0    COLUMN "base_Product"."TaxCode"    COMMENT     D   COMMENT ON COLUMN "base_Product"."TaxCode" IS 'Apply to Base Unit';
            public       postgres    false    1802            9           0    0 %   COLUMN "base_Product"."MarginPercent"    COMMENT     q   COMMENT ON COLUMN "base_Product"."MarginPercent" IS 'Margin =100*(RegularPrice - AverageUnitCode)/RegularPrice';
            public       postgres    false    1802            :           0    0 %   COLUMN "base_Product"."MarkupPercent"    COMMENT     t   COMMENT ON COLUMN "base_Product"."MarkupPercent" IS 'Markup =100*(RegularPrice - AverageUnitCost)/AverageUnitCost';
            public       postgres    false    1802            ;           0    0 "   COLUMN "base_Product"."IsOpenItem"    COMMENT     Q   COMMENT ON COLUMN "base_Product"."IsOpenItem" IS 'Can change price during sale';
            public       postgres    false    1802            <           0    0 *   COLUMN "base_Product"."QuantityOnCustomer"    COMMENT     O   COMMENT ON COLUMN "base_Product"."QuantityOnCustomer" IS 'Total of SaleOrder';
            public       postgres    false    1802                       1259    255536    base_ProductStore    TABLE     �  CREATE TABLE "base_ProductStore" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "QuantityOnHand" integer DEFAULT 0 NOT NULL,
    "StoreCode" integer DEFAULT 0 NOT NULL,
    "QuantityOnCustomer" integer DEFAULT 0 NOT NULL,
    "QuantityOnOrder" integer DEFAULT 0 NOT NULL,
    "ReorderPoint" integer DEFAULT 0 NOT NULL,
    "QuantityAvailable" integer DEFAULT 0 NOT NULL
);
 '   DROP TABLE public."base_ProductStore";
       public         postgres    false    2379    2380    2381    2382    2384    2385    7                       1259    255534    base_ProductStore_Id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ProductStore_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ProductStore_Id_seq";
       public       postgres    false    7    1807            =           0    0    base_ProductStore_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ProductStore_Id_seq" OWNED BY "base_ProductStore"."Id";
            public       postgres    false    1806            >           0    0    base_ProductStore_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ProductStore_Id_seq"', 191, true);
            public       postgres    false    1806            N           1259    270252    base_ProductUOM    TABLE     U  CREATE TABLE "base_ProductUOM" (
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
       public         postgres    false    2553    2555    2556    2557    2558    2559    2560    2561    2562    2563    2564    2565    2566    2567    7            ?           0    0    TABLE "base_ProductUOM"    COMMENT     B   COMMENT ON TABLE "base_ProductUOM" IS 'Use when allow multi UOM';
            public       postgres    false    1870            M           1259    270250    base_ProductUOM_Id_seq    SEQUENCE     z   CREATE SEQUENCE "base_ProductUOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."base_ProductUOM_Id_seq";
       public       postgres    false    7    1870            @           0    0    base_ProductUOM_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE "base_ProductUOM_Id_seq" OWNED BY "base_ProductUOM"."Id";
            public       postgres    false    1869            A           0    0    base_ProductUOM_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_ProductUOM_Id_seq"', 103, true);
            public       postgres    false    1869            	           1259    245410    base_Product_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_Product_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_Product_Id_seq";
       public       postgres    false    1802    7            B           0    0    base_Product_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_Product_Id_seq" OWNED BY "base_Product"."Id";
            public       postgres    false    1801            C           0    0    base_Product_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('"base_Product_Id_seq"', 250241, true);
            public       postgres    false    1801                       1259    245169    base_Promotion    TABLE     �  CREATE TABLE "base_Promotion" (
    "Id" integer NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(200) NOT NULL,
    "PromotionTypeId" smallint NOT NULL,
    "TakeOffOption" smallint NOT NULL,
    "TakeOff" numeric(12,2) NOT NULL,
    "BuyingQty" integer NOT NULL,
    "GetingValue" numeric(12,2) DEFAULT 0 NOT NULL,
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
    "BarCodeImage" bytea,
    "IsConflict" boolean DEFAULT false NOT NULL
);
 $   DROP TABLE public."base_Promotion";
       public         postgres    false    2298    2299    2300    2301    2302    2303    2304    2305    7            D           0    0     COLUMN "base_Promotion"."Status"    COMMENT     U   COMMENT ON COLUMN "base_Promotion"."Status" IS '0: Deactived
1: Actived
2: Pending';
            public       postgres    false    1796            E           0    0 (   COLUMN "base_Promotion"."AffectDiscount"    COMMENT     �   COMMENT ON COLUMN "base_Promotion"."AffectDiscount" IS '0: All items
1: All items in category
2: All items from vendors
3: Custom';
            public       postgres    false    1796            F           0    0 $   COLUMN "base_Promotion"."IsConflict"    COMMENT     `   COMMENT ON COLUMN "base_Promotion"."IsConflict" IS 'T: Khi con hon 2 chuong trinh cung active';
            public       postgres    false    1796                       1259    245155    base_PromotionAffect    TABLE     j  CREATE TABLE "base_PromotionAffect" (
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
       public         postgres    false    2286    2287    2289    2290    2291    2292    2293    2294    2295    2296    7                       1259    245153    base_PromotionAffect_Id_seq    SEQUENCE        CREATE SEQUENCE "base_PromotionAffect_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_PromotionAffect_Id_seq";
       public       postgres    false    7    1794            G           0    0    base_PromotionAffect_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_PromotionAffect_Id_seq" OWNED BY "base_PromotionAffect"."Id";
            public       postgres    false    1793            H           0    0    base_PromotionAffect_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_PromotionAffect_Id_seq"', 609, true);
            public       postgres    false    1793            �           1259    245023    base_PromotionSchedule    TABLE     �   CREATE TABLE "base_PromotionSchedule" (
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
       public       postgres    false    1782    7            I           0    0    base_PromotionSchedule_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE "base_PromotionSchedule_Id_seq" OWNED BY "base_PromotionSchedule"."Id";
            public       postgres    false    1781            J           0    0    base_PromotionSchedule_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('"base_PromotionSchedule_Id_seq"', 62, true);
            public       postgres    false    1781                       1259    245167    base_Promotion_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_Promotion_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_Promotion_Id_seq";
       public       postgres    false    7    1796            K           0    0    base_Promotion_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_Promotion_Id_seq" OWNED BY "base_Promotion"."Id";
            public       postgres    false    1795            L           0    0    base_Promotion_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_Promotion_Id_seq"', 62, true);
            public       postgres    false    1795            4           1259    266551    base_PurchaseOrder    TABLE     T  CREATE TABLE "base_PurchaseOrder" (
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
       public         postgres    false    2465    2467    2468    2469    2470    2471    2472    2473    2474    2475    2476    2477    2478    2479    2480    2481    2482    2483    7            M           0    0 (   COLUMN "base_PurchaseOrder"."QtyOrdered"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyOrdered" IS 'Order Quantity: In the purchase order item list. Enter the quantity being ordered for the item.
';
            public       postgres    false    1844            N           0    0 $   COLUMN "base_PurchaseOrder"."QtyDue"    COMMENT     q   COMMENT ON COLUMN "base_PurchaseOrder"."QtyDue" IS 'Due Quantity: The item quantity remaining to be received.
';
            public       postgres    false    1844            O           0    0 )   COLUMN "base_PurchaseOrder"."QtyReceived"    COMMENT     �   COMMENT ON COLUMN "base_PurchaseOrder"."QtyReceived" IS 'Received Quantity: The ordered item quantity already received on receiving vouchers.
';
            public       postgres    false    1844            P           0    0 &   COLUMN "base_PurchaseOrder"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_PurchaseOrder"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100

';
            public       postgres    false    1844            2           1259    266530    base_PurchaseOrderDetail    TABLE     �  CREATE TABLE "base_PurchaseOrderDetail" (
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
    "Discount" numeric(10,2) DEFAULT 0 NOT NULL,
    "OnHandQty" numeric(12,2) DEFAULT 0 NOT NULL
);
 .   DROP TABLE public."base_PurchaseOrderDetail";
       public         postgres    false    2454    2456    2457    2458    2459    2460    2461    2462    2463    2464    7            Q           0    0 *   COLUMN "base_PurchaseOrderDetail"."Amount"    COMMENT     S   COMMENT ON COLUMN "base_PurchaseOrderDetail"."Amount" IS 'Amount = Cost*Quantity';
            public       postgres    false    1842            1           1259    266528    base_PurchaseOrderDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_PurchaseOrderDetail_Id_seq";
       public       postgres    false    7    1842            R           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_PurchaseOrderDetail_Id_seq" OWNED BY "base_PurchaseOrderDetail"."Id";
            public       postgres    false    1841            S           0    0    base_PurchaseOrderDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_PurchaseOrderDetail_Id_seq"', 200, true);
            public       postgres    false    1841            :           1259    267535    base_PurchaseOrderReceive    TABLE     o  CREATE TABLE "base_PurchaseOrderReceive" (
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
       public         postgres    false    2489    2490    2491    2493    7            T           0    0 *   COLUMN "base_PurchaseOrderReceive"."Price"    COMMENT     G   COMMENT ON COLUMN "base_PurchaseOrderReceive"."Price" IS 'Sale Price';
            public       postgres    false    1850            9           1259    267533     base_PurchaseOrderReceive_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_PurchaseOrderReceive_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_PurchaseOrderReceive_Id_seq";
       public       postgres    false    7    1850            U           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_PurchaseOrderReceive_Id_seq" OWNED BY "base_PurchaseOrderReceive"."Id";
            public       postgres    false    1849            V           0    0     base_PurchaseOrderReceive_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_PurchaseOrderReceive_Id_seq"', 166, true);
            public       postgres    false    1849            3           1259    266549    base_PurchaseOrder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_PurchaseOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_PurchaseOrder_Id_seq";
       public       postgres    false    7    1844            W           0    0    base_PurchaseOrder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_PurchaseOrder_Id_seq" OWNED BY "base_PurchaseOrder"."Id";
            public       postgres    false    1843            X           0    0    base_PurchaseOrder_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_PurchaseOrder_Id_seq"', 110, true);
            public       postgres    false    1843            X           1259    282642    base_QuantityAdjustment    TABLE     a  CREATE TABLE "base_QuantityAdjustment" (
    "Id" bigint NOT NULL,
    "ProductId" bigint NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "CostDifference" numeric(12,2) DEFAULT 0 NOT NULL,
    "OldQty" integer DEFAULT 0 NOT NULL,
    "NewQty" integer DEFAULT 0 NOT NULL,
    "AdjustmentQtyDiff" integer DEFAULT 0 NOT NULL,
    "LoggedTime" timestamp without time zone NOT NULL,
    "UserCreated" character varying(100),
    "IsReversed" boolean DEFAULT false,
    "StoreCode" integer,
    "Resource" character varying(36),
    "Status" smallint NOT NULL,
    "Reason" smallint NOT NULL
);
 -   DROP TABLE public."base_QuantityAdjustment";
       public         postgres    false    2583    2584    2585    2586    2587    7            Y           0    0 1   COLUMN "base_QuantityAdjustment"."CostDifference"    COMMENT     �   COMMENT ON COLUMN "base_QuantityAdjustment"."CostDifference" IS '-- AverageUnitCost*OldQuantity - AverageUnitCost*NewQuantity';
            public       postgres    false    1880            Z           0    0 4   COLUMN "base_QuantityAdjustment"."AdjustmentQtyDiff"    COMMENT     j   COMMENT ON COLUMN "base_QuantityAdjustment"."AdjustmentQtyDiff" IS 'AdjustmentNewQty - AdjustmentOldQty';
            public       postgres    false    1880            W           1259    282640 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_QuantityAdjustmentItem_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ;   DROP SEQUENCE public."base_QuantityAdjustmentItem_Id_seq";
       public       postgres    false    1880    7            [           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_QuantityAdjustmentItem_Id_seq" OWNED BY "base_QuantityAdjustment"."Id";
            public       postgres    false    1879            \           0    0 "   base_QuantityAdjustmentItem_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_QuantityAdjustmentItem_Id_seq"', 98, true);
            public       postgres    false    1879            #           1259    256178    base_ResourceAccount    TABLE       CREATE TABLE "base_ResourceAccount" (
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
       public         postgres    false    2395    2396    2397    7            "           1259    256176    base_ResourceAccount_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourceAccount_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourceAccount_Id_seq";
       public       postgres    false    1827    7            ]           0    0    base_ResourceAccount_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourceAccount_Id_seq" OWNED BY "base_ResourceAccount"."Id";
            public       postgres    false    1826            ^           0    0    base_ResourceAccount_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceAccount_Id_seq"', 30, true);
            public       postgres    false    1826                       1259    246083    base_ResourceNote    TABLE     �   CREATE TABLE "base_ResourceNote" (
    "Id" bigint NOT NULL,
    "Note" character varying(300),
    "DateCreated" timestamp without time zone DEFAULT now(),
    "Color" character(9),
    "Resource" character varying(36) NOT NULL
);
 '   DROP TABLE public."base_ResourceNote";
       public         postgres    false    2346    7                       1259    246081    base_ResourceNote_id_seq    SEQUENCE     |   CREATE SEQUENCE "base_ResourceNote_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."base_ResourceNote_id_seq";
       public       postgres    false    7    1804            _           0    0    base_ResourceNote_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "base_ResourceNote_id_seq" OWNED BY "base_ResourceNote"."Id";
            public       postgres    false    1803            `           0    0    base_ResourceNote_id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_ResourceNote_id_seq"', 831, true);
            public       postgres    false    1803            J           1259    270150    base_ResourcePayment    TABLE     �  CREATE TABLE "base_ResourcePayment" (
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
    "LastRewardAmount" numeric(12,2) DEFAULT 0 NOT NULL,
    "Cashier" character varying(50)
);
 *   DROP TABLE public."base_ResourcePayment";
       public         postgres    false    2528    2529    2531    2532    2533    2534    2535    2536    2537    2538    2539    7            a           0    0 $   COLUMN "base_ResourcePayment"."Mark"    COMMENT     <   COMMENT ON COLUMN "base_ResourcePayment"."Mark" IS 'SO/PO';
            public       postgres    false    1866            H           1259    270072    base_ResourcePaymentDetail    TABLE       CREATE TABLE "base_ResourcePaymentDetail" (
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
       public         postgres    false    2525    2526    2527    7            b           0    0 1   COLUMN "base_ResourcePaymentDetail"."PaymentType"    COMMENT     W   COMMENT ON COLUMN "base_ResourcePaymentDetail"."PaymentType" IS 'P:Payment
C:Correct';
            public       postgres    false    1864            c           0    0 ,   COLUMN "base_ResourcePaymentDetail"."Reason"    COMMENT     ^   COMMENT ON COLUMN "base_ResourcePaymentDetail"."Reason" IS 'Apply to Correct payment action';
            public       postgres    false    1864            G           1259    270070 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourcePaymentDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_ResourcePaymentDetail_Id_seq";
       public       postgres    false    7    1864            d           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_ResourcePaymentDetail_Id_seq" OWNED BY "base_ResourcePaymentDetail"."Id";
            public       postgres    false    1863            e           0    0 !   base_ResourcePaymentDetail_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_ResourcePaymentDetail_Id_seq"', 477, true);
            public       postgres    false    1863            I           1259    270148    base_ResourcePayment_Id_seq    SEQUENCE        CREATE SEQUENCE "base_ResourcePayment_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_ResourcePayment_Id_seq";
       public       postgres    false    1866    7            f           0    0    base_ResourcePayment_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_ResourcePayment_Id_seq" OWNED BY "base_ResourcePayment"."Id";
            public       postgres    false    1865            g           0    0    base_ResourcePayment_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_ResourcePayment_Id_seq"', 398, true);
            public       postgres    false    1865            L           1259    270193    base_ResourceReturn    TABLE     j  CREATE TABLE "base_ResourceReturn" (
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
    "ReturnFeePercent" numeric(5,2) DEFAULT 0 NOT NULL,
    "Redeemed" numeric(12,2) DEFAULT 0
);
 )   DROP TABLE public."base_ResourceReturn";
       public         postgres    false    2540    2541    2542    2544    2545    2546    2547    2548    2549    2550    2551    2552    7            h           0    0 #   COLUMN "base_ResourceReturn"."Mark"    COMMENT     ;   COMMENT ON COLUMN "base_ResourceReturn"."Mark" IS 'SO/PO';
            public       postgres    false    1868            i           0    0 '   COLUMN "base_ResourceReturn"."Redeemed"    COMMENT     d   COMMENT ON COLUMN "base_ResourceReturn"."Redeemed" IS 'Gia tri reward duoc ap dung trong don hang';
            public       postgres    false    1868            T           1259    272099    base_ResourceReturnDetail    TABLE     �  CREATE TABLE "base_ResourceReturnDetail" (
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
       public         postgres    false    2576    2577    2579    2580    2581    7            S           1259    272097     base_ResourceReturnDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_ResourceReturnDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."base_ResourceReturnDetail_Id_seq";
       public       postgres    false    1876    7            j           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE "base_ResourceReturnDetail_Id_seq" OWNED BY "base_ResourceReturnDetail"."Id";
            public       postgres    false    1875            k           0    0     base_ResourceReturnDetail_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('"base_ResourceReturnDetail_Id_seq"', 159, true);
            public       postgres    false    1875            K           1259    270191    base_ResourceReturn_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_ResourceReturn_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_ResourceReturn_Id_seq";
       public       postgres    false    7    1868            l           0    0    base_ResourceReturn_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_ResourceReturn_Id_seq" OWNED BY "base_ResourceReturn"."Id";
            public       postgres    false    1867            m           0    0    base_ResourceReturn_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_ResourceReturn_Id_seq"', 393, true);
            public       postgres    false    1867            8           1259    266843    base_RewardManager    TABLE     �  CREATE TABLE "base_RewardManager" (
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
       public         postgres    false    2486    2487    2488    7            n           0    0 *   COLUMN "base_RewardManager"."IsAutoEnroll"    COMMENT     p   COMMENT ON COLUMN "base_RewardManager"."IsAutoEnroll" IS 'Automatically enroll new customer in Reward Program';
            public       postgres    false    1848            o           0    0 ,   COLUMN "base_RewardManager"."IsPromptEnroll"    COMMENT     p   COMMENT ON COLUMN "base_RewardManager"."IsPromptEnroll" IS 'Prompt to enroll when making sales to non-member
';
            public       postgres    false    1848            p           0    0 -   COLUMN "base_RewardManager"."IsInformCashier"    COMMENT     l   COMMENT ON COLUMN "base_RewardManager"."IsInformCashier" IS 'Inform cashier when sales rewards are earned';
            public       postgres    false    1848            q           0    0 /   COLUMN "base_RewardManager"."IsRedemptionLimit"    COMMENT     _   COMMENT ON COLUMN "base_RewardManager"."IsRedemptionLimit" IS 'Reward redeemption limit $???';
            public       postgres    false    1848            r           0    0 /   COLUMN "base_RewardManager"."IsBlockRedemption"    COMMENT     s   COMMENT ON COLUMN "base_RewardManager"."IsBlockRedemption" IS 'Block reward redeemption for ?? days after earned';
            public       postgres    false    1848            s           0    0 3   COLUMN "base_RewardManager"."IsBlockPurchaseRedeem"    COMMENT     m   COMMENT ON COLUMN "base_RewardManager"."IsBlockPurchaseRedeem" IS 'Block reward earn with  purchase redeem';
            public       postgres    false    1848            7           1259    266841    base_RewardManager_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_RewardManager_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_RewardManager_Id_seq";
       public       postgres    false    7    1848            t           0    0    base_RewardManager_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_RewardManager_Id_seq" OWNED BY "base_RewardManager"."Id";
            public       postgres    false    1847            u           0    0    base_RewardManager_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"base_RewardManager_Id_seq"', 3, true);
            public       postgres    false    1847            6           1259    266606    base_SaleCommission    TABLE     �  CREATE TABLE "base_SaleCommission" (
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
       public         postgres    false    7            v           0    0 %   COLUMN "base_SaleCommission"."Remark"    COMMENT     U   COMMENT ON COLUMN "base_SaleCommission"."Remark" IS 'SO:SaleOrder
SR:Sale Returned';
            public       postgres    false    1846            5           1259    266604    base_SaleCommission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "base_SaleCommission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."base_SaleCommission_Id_seq";
       public       postgres    false    1846    7            w           0    0    base_SaleCommission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "base_SaleCommission_Id_seq" OWNED BY "base_SaleCommission"."Id";
            public       postgres    false    1845            x           0    0    base_SaleCommission_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('"base_SaleCommission_Id_seq"', 815, true);
            public       postgres    false    1845            ,           1259    266093    base_SaleOrder    TABLE     �
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
    "Cashier" character varying(30),
    "IsQuationConverted" boolean DEFAULT false NOT NULL
);
 $   DROP TABLE public."base_SaleOrder";
       public         postgres    false    2416    2418    2419    2420    2421    2422    2423    2424    2425    2426    2427    2428    2429    2430    2431    2432    2433    2434    2435    2436    2437    2438    2439    2440    2441    2442    2443    2444    2445    2446    2447    2448    7            y           0    0 &   COLUMN "base_SaleOrder"."RewardAmount"    COMMENT     c   COMMENT ON COLUMN "base_SaleOrder"."RewardAmount" IS 'Tong so tien can thanh toan sau khi reward';
            public       postgres    false    1836            *           1259    266084    base_SaleOrderDetail    TABLE     �  CREATE TABLE "base_SaleOrderDetail" (
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
    "UOM" character varying(20),
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
       public         postgres    false    2402    2403    2404    2405    2406    2407    2409    2410    2411    2412    2413    2414    2415    7            z           0    0 (   COLUMN "base_SaleOrderDetail"."UnFilled"    COMMENT     e   COMMENT ON COLUMN "base_SaleOrderDetail"."UnFilled" IS 'Unfilled % = (Qty Due / Qty Ordered) X 100';
            public       postgres    false    1834            {           0    0 .   COLUMN "base_SaleOrderDetail"."SerialTracking"    COMMENT     Z   COMMENT ON COLUMN "base_SaleOrderDetail"."SerialTracking" IS 'Apply to Serial Tracking ';
            public       postgres    false    1834            |           0    0 .   COLUMN "base_SaleOrderDetail"."BalanceShipped"    COMMENT     s   COMMENT ON COLUMN "base_SaleOrderDetail"."BalanceShipped" IS 'Số lượng sản phẩm được vận chuyển';
            public       postgres    false    1834            }           0    0 (   COLUMN "base_SaleOrderDetail"."IsManual"    COMMENT     M   COMMENT ON COLUMN "base_SaleOrderDetail"."IsManual" IS 'Apply to promotion';
            public       postgres    false    1834            )           1259    266082    base_SaleOrderDetail_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleOrderDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleOrderDetail_Id_seq";
       public       postgres    false    7    1834            ~           0    0    base_SaleOrderDetail_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleOrderDetail_Id_seq" OWNED BY "base_SaleOrderDetail"."Id";
            public       postgres    false    1833                       0    0    base_SaleOrderDetail_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleOrderDetail_Id_seq"', 911, true);
            public       postgres    false    1833            .           1259    266180    base_SaleOrderShip    TABLE     �  CREATE TABLE "base_SaleOrderShip" (
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
       public         postgres    false    2450    2451    7            0           1259    266357    base_SaleOrderShipDetail    TABLE     2  CREATE TABLE "base_SaleOrderShipDetail" (
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
       public         postgres    false    2453    7            /           1259    266355    base_SaleOrderShipDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleOrderShipDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_SaleOrderShipDetail_Id_seq";
       public       postgres    false    7    1840            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_SaleOrderShipDetail_Id_seq" OWNED BY "base_SaleOrderShipDetail"."Id";
            public       postgres    false    1839            �           0    0    base_SaleOrderShipDetail_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('"base_SaleOrderShipDetail_Id_seq"', 604, true);
            public       postgres    false    1839            -           1259    266178    base_SaleOrderShip_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_SaleOrderShip_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_SaleOrderShip_Id_seq";
       public       postgres    false    7    1838            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_SaleOrderShip_Id_seq" OWNED BY "base_SaleOrderShip"."Id";
            public       postgres    false    1837            �           0    0    base_SaleOrderShip_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_SaleOrderShip_Id_seq"', 472, true);
            public       postgres    false    1837            +           1259    266091    base_SaleOrder_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_SaleOrder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_SaleOrder_Id_seq";
       public       postgres    false    7    1836            �           0    0    base_SaleOrder_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_SaleOrder_Id_seq" OWNED BY "base_SaleOrder"."Id";
            public       postgres    false    1835            �           0    0    base_SaleOrder_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_SaleOrder_Id_seq"', 577, true);
            public       postgres    false    1835            �           1259    245103    base_SaleTaxLocation    TABLE     n  CREATE TABLE "base_SaleTaxLocation" (
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
       public         postgres    false    2269    2271    2272    2273    2274    2275    7            �           0    0 )   COLUMN "base_SaleTaxLocation"."SortIndex"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocation"."SortIndex" IS 'ParentId ==0 -> Id"-"DateTime
ParnetId !=0 -> ParentId"-"DateTime
Order By Asc';
            public       postgres    false    1786            �           1259    245084    base_SaleTaxLocationOption    TABLE     (  CREATE TABLE "base_SaleTaxLocationOption" (
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
       public         postgres    false    2268    7            �           0    0 .   COLUMN "base_SaleTaxLocationOption"."ParentId"    COMMENT     h   COMMENT ON COLUMN "base_SaleTaxLocationOption"."ParentId" IS 'Apply For Multi-rate has multi tax code';
            public       postgres    false    1784            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."TaxRate"    COMMENT     k   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxRate" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1784            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxComponent"    COMMENT     Y   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxComponent" IS 'Apply For Multi-rate';
            public       postgres    false    1784            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."TaxAgency"    COMMENT     m   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxAgency" IS 'Apply For Single,Price-Depedent, Multi-rate';
            public       postgres    false    1784            �           0    0 2   COLUMN "base_SaleTaxLocationOption"."TaxCondition"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."TaxCondition" IS 'Apply For Price-Depedent: Collect this tax on an item if the unit price or shiping is more than';
            public       postgres    false    1784            �           0    0 7   COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsApplyAmountOver" IS 'Apply For Price-Depedent: Apply sale tax only to the amount over the pricing unit or shipping threshold';
            public       postgres    false    1784            �           0    0 C   COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowSpecificItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to a specific item price range';
            public       postgres    false    1784            �           0    0 A   COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange"    COMMENT     �   COMMENT ON COLUMN "base_SaleTaxLocationOption"."IsAllowAmountItemPriceRange" IS 'Apply For Multi-rate: Apply this tax rate to the mount of an item''s price within this range';
            public       postgres    false    1784            �           0    0 /   COLUMN "base_SaleTaxLocationOption"."PriceFrom"    COMMENT     V   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceFrom" IS 'Apply For Multi-rate';
            public       postgres    false    1784            �           0    0 -   COLUMN "base_SaleTaxLocationOption"."PriceTo"    COMMENT     T   COMMENT ON COLUMN "base_SaleTaxLocationOption"."PriceTo" IS 'Apply For Multi-rate';
            public       postgres    false    1784            �           1259    245082 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_SaleTaxLocationOption_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 :   DROP SEQUENCE public."base_SaleTaxLocationOption_Id_seq";
       public       postgres    false    1784    7            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE "base_SaleTaxLocationOption_Id_seq" OWNED BY "base_SaleTaxLocationOption"."Id";
            public       postgres    false    1783            �           0    0 !   base_SaleTaxLocationOption_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('"base_SaleTaxLocationOption_Id_seq"', 122, true);
            public       postgres    false    1783            �           1259    245101    base_SaleTaxLocation_Id_seq    SEQUENCE        CREATE SEQUENCE "base_SaleTaxLocation_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."base_SaleTaxLocation_Id_seq";
       public       postgres    false    1786    7            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE "base_SaleTaxLocation_Id_seq" OWNED BY "base_SaleTaxLocation"."Id";
            public       postgres    false    1785            �           0    0    base_SaleTaxLocation_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('"base_SaleTaxLocation_Id_seq"', 378, true);
            public       postgres    false    1785                       1259    255675 
   base_Store    TABLE     �   CREATE TABLE "base_Store" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(30),
    "Street" character varying(200),
    "City" character varying(200)
);
     DROP TABLE public."base_Store";
       public         postgres    false    7                       1259    255673    base_Store_Id_seq    SEQUENCE     u   CREATE SEQUENCE "base_Store_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."base_Store_Id_seq";
       public       postgres    false    7    1809            �           0    0    base_Store_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "base_Store_Id_seq" OWNED BY "base_Store"."Id";
            public       postgres    false    1808            �           0    0    base_Store_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('"base_Store_Id_seq"', 51, true);
            public       postgres    false    1808            D           1259    269925    base_TransferStock    TABLE     �  CREATE TABLE "base_TransferStock" (
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
       public         postgres    false    2510    2512    2513    2514    2515    2516    2517    2518    2519    7            F           1259    269941    base_TransferStockDetail    TABLE     |  CREATE TABLE "base_TransferStockDetail" (
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
       public         postgres    false    2521    2522    2523    7            E           1259    269939    base_TransferStockDetail_Id_seq    SEQUENCE     �   CREATE SEQUENCE "base_TransferStockDetail_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."base_TransferStockDetail_Id_seq";
       public       postgres    false    1862    7            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE "base_TransferStockDetail_Id_seq" OWNED BY "base_TransferStockDetail"."Id";
            public       postgres    false    1861            �           0    0    base_TransferStockDetail_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('"base_TransferStockDetail_Id_seq"', 52, true);
            public       postgres    false    1861            C           1259    269923    base_TransferStock_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_TransferStock_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_TransferStock_Id_seq";
       public       postgres    false    1860    7            �           0    0    base_TransferStock_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_TransferStock_Id_seq" OWNED BY "base_TransferStock"."Id";
            public       postgres    false    1859            �           0    0    base_TransferStock_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_TransferStock_Id_seq"', 41, true);
            public       postgres    false    1859                        1259    245147    base_UOM    TABLE     �  CREATE TABLE "base_UOM" (
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
       public         postgres    false    2283    2284    2285    7            �           1259    245145    base_UOM_Id_seq    SEQUENCE     s   CREATE SEQUENCE "base_UOM_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."base_UOM_Id_seq";
       public       postgres    false    1792    7            �           0    0    base_UOM_Id_seq    SEQUENCE OWNED BY     ;   ALTER SEQUENCE "base_UOM_Id_seq" OWNED BY "base_UOM"."Id";
            public       postgres    false    1791            �           0    0    base_UOM_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"base_UOM_Id_seq"', 128, true);
            public       postgres    false    1791            �           1259    245131    base_UserLog    TABLE     #  CREATE TABLE "base_UserLog" (
    "Id" bigint NOT NULL,
    "IpSource" character varying(17),
    "ConnectedOn" timestamp without time zone DEFAULT now() NOT NULL,
    "DisConnectedOn" timestamp without time zone,
    "ResourceAccessed" character varying(36),
    "IsDisconected" boolean
);
 "   DROP TABLE public."base_UserLog";
       public         postgres    false    2281    7            �           1259    244282    base_UserLogDetail    TABLE     �   CREATE TABLE "base_UserLogDetail" (
    "Id" uuid NOT NULL,
    "UserLogId" bigint,
    "AccessedTime" timestamp without time zone,
    "ModuleName" character varying(30),
    "ActionDescription" character varying(200)
);
 (   DROP TABLE public."base_UserLogDetail";
       public         postgres    false    7            �           1259    245129    base_UserLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "base_UserLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."base_UserLog_Id_seq";
       public       postgres    false    7    1790            �           0    0    base_UserLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "base_UserLog_Id_seq" OWNED BY "base_UserLog"."Id";
            public       postgres    false    1789            �           0    0    base_UserLog_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"base_UserLog_Id_seq"', 3003, true);
            public       postgres    false    1789            %           1259    256244    base_UserRight    TABLE     �   CREATE TABLE "base_UserRight" (
    "Id" integer NOT NULL,
    "Code" character varying(10) NOT NULL,
    "Name" character varying(200)
);
 $   DROP TABLE public."base_UserRight";
       public         postgres    false    7            $           1259    256242    base_UserRight_Id_seq    SEQUENCE     y   CREATE SEQUENCE "base_UserRight_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."base_UserRight_Id_seq";
       public       postgres    false    7    1829            �           0    0    base_UserRight_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE "base_UserRight_Id_seq" OWNED BY "base_UserRight"."Id";
            public       postgres    false    1828            �           0    0    base_UserRight_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('"base_UserRight_Id_seq"', 189, true);
            public       postgres    false    1828            A           1259    269643    base_VendorProduct    TABLE       CREATE TABLE "base_VendorProduct" (
    "Id" integer NOT NULL,
    "ProductId" bigint NOT NULL,
    "VendorId" bigint NOT NULL,
    "Price" numeric(12,2) DEFAULT 0 NOT NULL,
    "ProductResource" character varying(36) NOT NULL,
    "VendorResource" character varying(36) NOT NULL
);
 (   DROP TABLE public."base_VendorProduct";
       public         postgres    false    2509    7            B           1259    269646    base_VendorProduct_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VendorProduct_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VendorProduct_Id_seq";
       public       postgres    false    1857    7            �           0    0    base_VendorProduct_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VendorProduct_Id_seq" OWNED BY "base_VendorProduct"."Id";
            public       postgres    false    1858            �           0    0    base_VendorProduct_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"base_VendorProduct_Id_seq"', 43, true);
            public       postgres    false    1858            �           1259    245115    base_VirtualFolder    TABLE     �  CREATE TABLE "base_VirtualFolder" (
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
       public         postgres    false    2277    2278    2279    7            �           1259    245113    base_VirtualFolder_Id_seq    SEQUENCE     }   CREATE SEQUENCE "base_VirtualFolder_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."base_VirtualFolder_Id_seq";
       public       postgres    false    1788    7            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE "base_VirtualFolder_Id_seq" OWNED BY "base_VirtualFolder"."Id";
            public       postgres    false    1787            �           0    0    base_VirtualFolder_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('"base_VirtualFolder_Id_seq"', 103, true);
            public       postgres    false    1787            U           1259    282433 	   rpt_Group    TABLE     �   CREATE TABLE "rpt_Group" (
    "Id" integer NOT NULL,
    "Code" character varying(3),
    "Name" character varying(200),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(30)
);
    DROP TABLE public."rpt_Group";
       public         postgres    false    7            V           1259    282436    rpt_Group_Id_seq    SEQUENCE     t   CREATE SEQUENCE "rpt_Group_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."rpt_Group_Id_seq";
       public       postgres    false    7    1877            �           0    0    rpt_Group_Id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE "rpt_Group_Id_seq" OWNED BY "rpt_Group"."Id";
            public       postgres    false    1878            �           0    0    rpt_Group_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"rpt_Group_Id_seq"', 1, false);
            public       postgres    false    1878            \           1259    283470 
   rpt_Report    TABLE     �  CREATE TABLE "rpt_Report" (
    "Id" integer NOT NULL,
    "GroupId" integer DEFAULT 0 NOT NULL,
    "ParentId" integer DEFAULT 0 NOT NULL,
    "Code" character varying(4) NOT NULL,
    "Name" character varying(200),
    "FormatFile" character varying(50),
    "IsShow" boolean DEFAULT false NOT NULL,
    "PreProcessName" character varying(50),
    "SamplePicture" bytea,
    "PrintTimes" integer,
    "LastPrintDate" timestamp without time zone,
    "LastPrintUser" character varying(30),
    "ExcelFile" character varying(50),
    "PrinterName" character varying(100),
    "PrintCopy" smallint DEFAULT 0 NOT NULL,
    "Remark" character varying(200),
    "DateCreated" timestamp without time zone,
    "UserCreated" character varying(35),
    "DateUpdated" timestamp without time zone,
    "UserUpdated" character varying(35),
    "PaperSize" smallint DEFAULT 0 NOT NULL,
    "ScreenTimes" integer DEFAULT 0 NOT NULL,
    "PrepProcessDescription" character varying(200)
);
     DROP TABLE public."rpt_Report";
       public         postgres    false    2599    2600    2601    2602    2603    2604    7            [           1259    283468    rpt_Report_Id_seq    SEQUENCE     u   CREATE SEQUENCE "rpt_Report_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."rpt_Report_Id_seq";
       public       postgres    false    1884    7            �           0    0    rpt_Report_Id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE "rpt_Report_Id_seq" OWNED BY "rpt_Report"."Id";
            public       postgres    false    1883            �           0    0    rpt_Report_Id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('"rpt_Report_Id_seq"', 3, true);
            public       postgres    false    1883                       1259    255696    tims_Holiday    TABLE     #  CREATE TABLE "tims_Holiday" (
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
       public         postgres    false    7                       1259    255705    tims_HolidayHistory    TABLE     {   CREATE TABLE "tims_HolidayHistory" (
    "Date" timestamp without time zone NOT NULL,
    "Name" character varying(200)
);
 )   DROP TABLE public."tims_HolidayHistory";
       public         postgres    false    7                       1259    255694    tims_Holiday_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_Holiday_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_Holiday_Id_seq";
       public       postgres    false    1811    7            �           0    0    tims_Holiday_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_Holiday_Id_seq" OWNED BY "tims_Holiday"."Id";
            public       postgres    false    1810            �           0    0    tims_Holiday_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_Holiday_Id_seq"', 10, true);
            public       postgres    false    1810                       1259    255849    tims_TimeLog    TABLE     <  CREATE TABLE "tims_TimeLog" (
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
       public         postgres    false    7                       1259    255865    tims_TimeLogPermission    TABLE     u   CREATE TABLE "tims_TimeLogPermission" (
    "TimeLogId" integer NOT NULL,
    "WorkPermissionId" integer NOT NULL
);
 ,   DROP TABLE public."tims_TimeLogPermission";
       public         postgres    false    7                       1259    255863 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE     �   CREATE SEQUENCE "tims_TimeLogPermission_TimeLogId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 =   DROP SEQUENCE public."tims_TimeLogPermission_TimeLogId_seq";
       public       postgres    false    1822    7            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE "tims_TimeLogPermission_TimeLogId_seq" OWNED BY "tims_TimeLogPermission"."TimeLogId";
            public       postgres    false    1821            �           0    0 $   tims_TimeLogPermission_TimeLogId_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('"tims_TimeLogPermission_TimeLogId_seq"', 1, false);
            public       postgres    false    1821                       1259    255847    tims_TimeLog_Id_seq    SEQUENCE     w   CREATE SEQUENCE "tims_TimeLog_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."tims_TimeLog_Id_seq";
       public       postgres    false    1820    7            �           0    0    tims_TimeLog_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE "tims_TimeLog_Id_seq" OWNED BY "tims_TimeLog"."Id";
            public       postgres    false    1819            �           0    0    tims_TimeLog_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('"tims_TimeLog_Id_seq"', 11, true);
            public       postgres    false    1819                       1259    255795    tims_WorkPermission    TABLE     Z  CREATE TABLE "tims_WorkPermission" (
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
       public         postgres    false    7                       1259    255793    tims_WorkPermission_Id_seq    SEQUENCE     ~   CREATE SEQUENCE "tims_WorkPermission_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."tims_WorkPermission_Id_seq";
       public       postgres    false    1818    7            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE "tims_WorkPermission_Id_seq" OWNED BY "tims_WorkPermission"."Id";
            public       postgres    false    1817            �           0    0    tims_WorkPermission_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('"tims_WorkPermission_Id_seq"', 7, true);
            public       postgres    false    1817                       1259    255738    tims_WorkSchedule    TABLE     �  CREATE TABLE "tims_WorkSchedule" (
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
       public         postgres    false    7                       1259    255736    tims_WorkSchedule_Id_seq    SEQUENCE     |   CREATE SEQUENCE "tims_WorkSchedule_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."tims_WorkSchedule_Id_seq";
       public       postgres    false    1814    7            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE "tims_WorkSchedule_Id_seq" OWNED BY "tims_WorkSchedule"."Id";
            public       postgres    false    1813            �           0    0    tims_WorkSchedule_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('"tims_WorkSchedule_Id_seq"', 28, true);
            public       postgres    false    1813                       1259    255781    tims_WorkWeek    TABLE     �  CREATE TABLE "tims_WorkWeek" (
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
       public         postgres    false    7                       1259    255779    tims_WorkWeek_Id_seq    SEQUENCE     x   CREATE SEQUENCE "tims_WorkWeek_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."tims_WorkWeek_Id_seq";
       public       postgres    false    7    1816            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE "tims_WorkWeek_Id_seq" OWNED BY "tims_WorkWeek"."Id";
            public       postgres    false    1815            �           0    0    tims_WorkWeek_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('"tims_WorkWeek_Id_seq"', 187, true);
            public       postgres    false    1815            r           2604    235589    jexid    DEFAULT     g   ALTER TABLE pga_exception ALTER COLUMN jexid SET DEFAULT nextval('pga_exception_jexid_seq'::regclass);
 C   ALTER TABLE pgagent.pga_exception ALTER COLUMN jexid DROP DEFAULT;
       pgagent       postgres    false    1746    1745            x           2604    235590    jobid    DEFAULT     [   ALTER TABLE pga_job ALTER COLUMN jobid SET DEFAULT nextval('pga_job_jobid_seq'::regclass);
 =   ALTER TABLE pgagent.pga_job ALTER COLUMN jobid DROP DEFAULT;
       pgagent       postgres    false    1748    1747            z           2604    235591    jclid    DEFAULT     e   ALTER TABLE pga_jobclass ALTER COLUMN jclid SET DEFAULT nextval('pga_jobclass_jclid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_jobclass ALTER COLUMN jclid DROP DEFAULT;
       pgagent       postgres    false    1751    1750            }           2604    235592    jlgid    DEFAULT     a   ALTER TABLE pga_joblog ALTER COLUMN jlgid SET DEFAULT nextval('pga_joblog_jlgid_seq'::regclass);
 @   ALTER TABLE pgagent.pga_joblog ALTER COLUMN jlgid DROP DEFAULT;
       pgagent       postgres    false    1753    1752            �           2604    235593    jstid    DEFAULT     c   ALTER TABLE pga_jobstep ALTER COLUMN jstid SET DEFAULT nextval('pga_jobstep_jstid_seq'::regclass);
 A   ALTER TABLE pgagent.pga_jobstep ALTER COLUMN jstid DROP DEFAULT;
       pgagent       postgres    false    1755    1754            �           2604    235594    jslid    DEFAULT     i   ALTER TABLE pga_jobsteplog ALTER COLUMN jslid SET DEFAULT nextval('pga_jobsteplog_jslid_seq'::regclass);
 D   ALTER TABLE pgagent.pga_jobsteplog ALTER COLUMN jslid DROP DEFAULT;
       pgagent       postgres    false    1757    1756            �           2604    235595    jscid    DEFAULT     e   ALTER TABLE pga_schedule ALTER COLUMN jscid SET DEFAULT nextval('pga_schedule_jscid_seq'::regclass);
 B   ALTER TABLE pgagent.pga_schedule ALTER COLUMN jscid DROP DEFAULT;
       pgagent       postgres    false    1759    1758            �           2604    244949    Id    DEFAULT     k   ALTER TABLE "base_Attachment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Attachment_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Attachment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1777    1778    1778            Y	           2604    256171    Id    DEFAULT     i   ALTER TABLE "base_Authorize" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Authorize_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Authorize" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1824    1825    1825            9	           2604    257304    Id    DEFAULT     q   ALTER TABLE "base_Configuration" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Configuration_Id_seq"'::regclass);
 H   ALTER TABLE public."base_Configuration" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1830    1805            
           2604    283363    Id    DEFAULT     w   ALTER TABLE "base_CostAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CostAdjustmentItem_Id_seq"'::regclass);
 I   ALTER TABLE public."base_CostAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1881    1882    1882            
           2604    271741    Id    DEFAULT     k   ALTER TABLE "base_CountStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStock_Id_seq"'::regclass);
 E   ALTER TABLE public."base_CountStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1872    1871    1872            
           2604    271748    Id    DEFAULT     w   ALTER TABLE "base_CountStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_CountStockDetail_Id_seq"'::regclass);
 K   ALTER TABLE public."base_CountStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1874    1873    1874            	           2604    245343    Id    DEFAULT     k   ALTER TABLE "base_Department" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Department_Id_seq"'::regclass);
 E   ALTER TABLE public."base_Department" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1797    1798    1798            �           2604    244820    Id    DEFAULT     a   ALTER TABLE "base_Guest" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Guest_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Guest" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1765    1766    1766            
	           2604    245379    Id    DEFAULT     u   ALTER TABLE "base_GuestAdditional" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAdditional_Id_seq"'::regclass);
 J   ALTER TABLE public."base_GuestAdditional" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1800    1799    1800            �           2604    244866    Id    DEFAULT     o   ALTER TABLE "base_GuestAddress" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestAddress_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestAddress" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1767    1768    1768            �           2604    238416    Id    DEFAULT     w   ALTER TABLE "base_GuestFingerPrint" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestFingerPrint_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestFingerPrint" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1762    1763    1763            �           2604    244876    Id    DEFAULT     {   ALTER TABLE "base_GuestHiringHistory" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestHiringHistory_Id_seq"'::regclass);
 M   ALTER TABLE public."base_GuestHiringHistory" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1770    1769    1770            �           2604    244887    Id    DEFAULT     o   ALTER TABLE "base_GuestPayRoll" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPayRoll_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestPayRoll" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1772    1771    1772            _	           2604    257328    Id    DEFAULT     w   ALTER TABLE "base_GuestPaymentCard" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPaymentCard_Id_seq"'::regclass);
 K   ALTER TABLE public."base_GuestPaymentCard" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1832    1831    1832            �           2604    244937    Id    DEFAULT     o   ALTER TABLE "base_GuestProfile" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestProfile_Id_seq"'::regclass);
 G   ALTER TABLE public."base_GuestProfile" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1775    1776    1776            �	           2604    268357    Id    DEFAULT     m   ALTER TABLE "base_GuestReward" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestReward_Id_seq"'::regclass);
 F   ALTER TABLE public."base_GuestReward" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1853    1854    1854            �           2604    245000    Id    DEFAULT     k   ALTER TABLE "base_MemberShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_MemberShip_Id_seq"'::regclass);
 E   ALTER TABLE public."base_MemberShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1780    1779    1780            �	           2604    268514    Id    DEFAULT     q   ALTER TABLE "base_PricingChange" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingChange_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PricingChange" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1855    1856    1856            �	           2604    268188    Id    DEFAULT     s   ALTER TABLE "base_PricingManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PricingManager_Id_seq"'::regclass);
 I   ALTER TABLE public."base_PricingManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1851    1852    1852            	           2604    245415    Id    DEFAULT     e   ALTER TABLE "base_Product" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Product_Id_seq"'::regclass);
 B   ALTER TABLE public."base_Product" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1802    1801    1802            O	           2604    255539    Id    DEFAULT     o   ALTER TABLE "base_ProductStore" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductStore_Id_seq"'::regclass);
 G   ALTER TABLE public."base_ProductStore" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1806    1807    1807            �	           2604    270255    Id    DEFAULT     k   ALTER TABLE "base_ProductUOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ProductUOM_Id_seq"'::regclass);
 E   ALTER TABLE public."base_ProductUOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1869    1870    1870            �           2604    245172    Id    DEFAULT     i   ALTER TABLE "base_Promotion" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Promotion_Id_seq"'::regclass);
 D   ALTER TABLE public."base_Promotion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1796    1795    1796            �           2604    245158    Id    DEFAULT     u   ALTER TABLE "base_PromotionAffect" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionAffect_Id_seq"'::regclass);
 J   ALTER TABLE public."base_PromotionAffect" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1793    1794    1794            �           2604    245026    Id    DEFAULT     y   ALTER TABLE "base_PromotionSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PromotionSchedule_Id_seq"'::regclass);
 L   ALTER TABLE public."base_PromotionSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1781    1782    1782            �	           2604    266554    Id    DEFAULT     q   ALTER TABLE "base_PurchaseOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_PurchaseOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1844    1843    1844            �	           2604    266533    Id    DEFAULT     }   ALTER TABLE "base_PurchaseOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_PurchaseOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1842    1841    1842            �	           2604    267538    Id    DEFAULT        ALTER TABLE "base_PurchaseOrderReceive" ALTER COLUMN "Id" SET DEFAULT nextval('"base_PurchaseOrderReceive_Id_seq"'::regclass);
 O   ALTER TABLE public."base_PurchaseOrderReceive" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1850    1849    1850            
           2604    282645    Id    DEFAULT        ALTER TABLE "base_QuantityAdjustment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_QuantityAdjustmentItem_Id_seq"'::regclass);
 M   ALTER TABLE public."base_QuantityAdjustment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1879    1880    1880            Z	           2604    256181    Id    DEFAULT     u   ALTER TABLE "base_ResourceAccount" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceAccount_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourceAccount" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1826    1827    1827            )	           2604    246086    Id    DEFAULT     o   ALTER TABLE "base_ResourceNote" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceNote_id_seq"'::regclass);
 G   ALTER TABLE public."base_ResourceNote" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1803    1804    1804            �	           2604    270153    Id    DEFAULT     u   ALTER TABLE "base_ResourcePayment" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePayment_Id_seq"'::regclass);
 J   ALTER TABLE public."base_ResourcePayment" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1865    1866    1866            �	           2604    270075    Id    DEFAULT     �   ALTER TABLE "base_ResourcePaymentDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourcePaymentDetail_Id_seq"'::regclass);
 P   ALTER TABLE public."base_ResourcePaymentDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1864    1863    1864            �           2604    244925    Id    DEFAULT     n   ALTER TABLE "base_ResourcePhoto" ALTER COLUMN "Id" SET DEFAULT nextval('"base_GuestPhoto_Id_seq"'::regclass);
 H   ALTER TABLE public."base_ResourcePhoto" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1774    1773    1774            �	           2604    270196    Id    DEFAULT     s   ALTER TABLE "base_ResourceReturn" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturn_Id_seq"'::regclass);
 I   ALTER TABLE public."base_ResourceReturn" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1867    1868    1868            
           2604    272102    Id    DEFAULT        ALTER TABLE "base_ResourceReturnDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_ResourceReturnDetail_Id_seq"'::regclass);
 O   ALTER TABLE public."base_ResourceReturnDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1875    1876    1876            �	           2604    266846    Id    DEFAULT     q   ALTER TABLE "base_RewardManager" ALTER COLUMN "Id" SET DEFAULT nextval('"base_RewardManager_Id_seq"'::regclass);
 H   ALTER TABLE public."base_RewardManager" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1848    1847    1848            �	           2604    266609    Id    DEFAULT     s   ALTER TABLE "base_SaleCommission" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleCommission_Id_seq"'::regclass);
 I   ALTER TABLE public."base_SaleCommission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1845    1846    1846            q	           2604    266096    Id    DEFAULT     i   ALTER TABLE "base_SaleOrder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrder_Id_seq"'::regclass);
 D   ALTER TABLE public."base_SaleOrder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1836    1835    1836            h	           2604    266087    Id    DEFAULT     u   ALTER TABLE "base_SaleOrderDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderDetail_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleOrderDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1833    1834    1834            �	           2604    266183    Id    DEFAULT     q   ALTER TABLE "base_SaleOrderShip" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShip_Id_seq"'::regclass);
 H   ALTER TABLE public."base_SaleOrderShip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1838    1837    1838            �	           2604    266360    Id    DEFAULT     }   ALTER TABLE "base_SaleOrderShipDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleOrderShipDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_SaleOrderShipDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1839    1840    1840            �           2604    245106    Id    DEFAULT     u   ALTER TABLE "base_SaleTaxLocation" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocation_Id_seq"'::regclass);
 J   ALTER TABLE public."base_SaleTaxLocation" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1785    1786    1786            �           2604    245087    Id    DEFAULT     �   ALTER TABLE "base_SaleTaxLocationOption" ALTER COLUMN "Id" SET DEFAULT nextval('"base_SaleTaxLocationOption_Id_seq"'::regclass);
 P   ALTER TABLE public."base_SaleTaxLocationOption" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1784    1783    1784            R	           2604    255678    Id    DEFAULT     a   ALTER TABLE "base_Store" ALTER COLUMN "Id" SET DEFAULT nextval('"base_Store_Id_seq"'::regclass);
 @   ALTER TABLE public."base_Store" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1808    1809    1809            �	           2604    269928    Id    DEFAULT     q   ALTER TABLE "base_TransferStock" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStock_Id_seq"'::regclass);
 H   ALTER TABLE public."base_TransferStock" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1859    1860    1860            �	           2604    269944    Id    DEFAULT     }   ALTER TABLE "base_TransferStockDetail" ALTER COLUMN "Id" SET DEFAULT nextval('"base_TransferStockDetail_Id_seq"'::regclass);
 N   ALTER TABLE public."base_TransferStockDetail" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1862    1861    1862            �           2604    245150    Id    DEFAULT     ]   ALTER TABLE "base_UOM" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UOM_Id_seq"'::regclass);
 >   ALTER TABLE public."base_UOM" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1791    1792    1792            �           2604    245134    Id    DEFAULT     e   ALTER TABLE "base_UserLog" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserLog_Id_seq"'::regclass);
 B   ALTER TABLE public."base_UserLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1789    1790    1790            ^	           2604    256247    Id    DEFAULT     i   ALTER TABLE "base_UserRight" ALTER COLUMN "Id" SET DEFAULT nextval('"base_UserRight_Id_seq"'::regclass);
 D   ALTER TABLE public."base_UserRight" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1829    1828    1829            �	           2604    269648    Id    DEFAULT     q   ALTER TABLE "base_VendorProduct" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VendorProduct_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VendorProduct" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1858    1857            �           2604    245118    Id    DEFAULT     q   ALTER TABLE "base_VirtualFolder" ALTER COLUMN "Id" SET DEFAULT nextval('"base_VirtualFolder_Id_seq"'::regclass);
 H   ALTER TABLE public."base_VirtualFolder" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1788    1787    1788            
           2604    282438    Id    DEFAULT     _   ALTER TABLE "rpt_Group" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Group_Id_seq"'::regclass);
 ?   ALTER TABLE public."rpt_Group" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1878    1877            &
           2604    283473    Id    DEFAULT     a   ALTER TABLE "rpt_Report" ALTER COLUMN "Id" SET DEFAULT nextval('"rpt_Report_Id_seq"'::regclass);
 @   ALTER TABLE public."rpt_Report" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1883    1884    1884            S	           2604    255699    Id    DEFAULT     e   ALTER TABLE "tims_Holiday" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_Holiday_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_Holiday" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1811    1810    1811            W	           2604    255852    Id    DEFAULT     e   ALTER TABLE "tims_TimeLog" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_TimeLog_Id_seq"'::regclass);
 B   ALTER TABLE public."tims_TimeLog" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1820    1819    1820            X	           2604    255868 	   TimeLogId    DEFAULT     �   ALTER TABLE "tims_TimeLogPermission" ALTER COLUMN "TimeLogId" SET DEFAULT nextval('"tims_TimeLogPermission_TimeLogId_seq"'::regclass);
 S   ALTER TABLE public."tims_TimeLogPermission" ALTER COLUMN "TimeLogId" DROP DEFAULT;
       public       postgres    false    1822    1821    1822            V	           2604    255798    Id    DEFAULT     s   ALTER TABLE "tims_WorkPermission" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkPermission_Id_seq"'::regclass);
 I   ALTER TABLE public."tims_WorkPermission" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1817    1818    1818            T	           2604    255741    Id    DEFAULT     o   ALTER TABLE "tims_WorkSchedule" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkSchedule_Id_seq"'::regclass);
 G   ALTER TABLE public."tims_WorkSchedule" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1814    1813    1814            U	           2604    255784    Id    DEFAULT     g   ALTER TABLE "tims_WorkWeek" ALTER COLUMN "Id" SET DEFAULT nextval('"tims_WorkWeek_Id_seq"'::regclass);
 C   ALTER TABLE public."tims_WorkWeek" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    1816    1815    1816            Q          0    234865    pga_exception 
   TABLE DATA               B   COPY pga_exception (jexid, jexscid, jexdate, jextime) FROM stdin;
    pgagent       postgres    false    1745   A�      R          0    234870    pga_job 
   TABLE DATA               �   COPY pga_job (jobid, jobjclid, jobname, jobdesc, jobhostagent, jobenabled, jobcreated, jobchanged, jobagentid, jobnextrun, joblastrun) FROM stdin;
    pgagent       postgres    false    1747   ^�      S          0    234883    pga_jobagent 
   TABLE DATA               A   COPY pga_jobagent (jagpid, jaglogintime, jagstation) FROM stdin;
    pgagent       postgres    false    1749   {�      T          0    234890    pga_jobclass 
   TABLE DATA               /   COPY pga_jobclass (jclid, jclname) FROM stdin;
    pgagent       postgres    false    1750   ��      U          0    234898 
   pga_joblog 
   TABLE DATA               P   COPY pga_joblog (jlgid, jlgjobid, jlgstatus, jlgstart, jlgduration) FROM stdin;
    pgagent       postgres    false    1752    �      V          0    234906    pga_jobstep 
   TABLE DATA               �   COPY pga_jobstep (jstid, jstjobid, jstname, jstdesc, jstenabled, jstkind, jstcode, jstconnstr, jstdbname, jstonerror, jscnextrun) FROM stdin;
    pgagent       postgres    false    1754   �      W          0    234923    pga_jobsteplog 
   TABLE DATA               t   COPY pga_jobsteplog (jslid, jsljlgid, jsljstid, jslstatus, jslresult, jslstart, jslduration, jsloutput) FROM stdin;
    pgagent       postgres    false    1756   :�      X          0    234934    pga_schedule 
   TABLE DATA               �   COPY pga_schedule (jscid, jscjobid, jscname, jscdesc, jscenabled, jscstart, jscend, jscminutes, jschours, jscweekdays, jscmonthdays, jscmonths) FROM stdin;
    pgagent       postgres    false    1758   W�      c          0    244946    base_Attachment 
   TABLE DATA               �   COPY "base_Attachment" ("Id", "FileOriginalName", "FileName", "FileExtension", "VirtualFolderId", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Counter") FROM stdin;
    public       postgres    false    1778   t�      |          0    256168    base_Authorize 
   TABLE DATA               =   COPY "base_Authorize" ("Id", "Resource", "Code") FROM stdin;
    public       postgres    false    1825   #�      q          0    254557    base_Configuration 
   TABLE DATA               �  COPY "base_Configuration" ("CompanyName", "Address", "City", "State", "ZipCode", "CountryId", "Phone", "Fax", "Email", "Website", "EmailPop3Server", "EmailPop3Port", "EmailAccount", "EmailPassword", "IsBarcodeScannerAttached", "IsEnableTouchScreenLayout", "IsAllowTimeClockAttached", "IsAllowCollectTipCreditCard", "IsAllowMutilUOM", "DefaultMaximumSticky", "DefaultPriceSchema", "DefaultPaymentMethod", "DefaultSaleTaxLocation", "DefaultTaxCodeNewDepartment", "DefautlImagePath", "DefautlDiscountScheduleTime", "DateCreated", "UserCreated", "TotalStore", "IsRequirePromotionCode", "DefaultDiscountType", "DefaultDiscountStatus", "LoginAllow", "Logo", "DefaultScanMethod", "TipPercent", "AcceptedPaymentMethod", "AcceptedCardType", "IsRequireDiscountReason", "WorkHour", "Id", "DefaultShipUnit", "DefaultCashiedUserName", "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", "IsAllowRGO", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", "ReturnFeePercent", "IsRewardLessThanDiscount", "CurrencySymbol", "DecimalPlaces", "FomartCurrency", "PasswordFormat", "KeepBackUp") FROM stdin;
    public       postgres    false    1805   @�      �          0    283360    base_CostAdjustment 
   TABLE DATA                 COPY "base_CostAdjustment" ("Id", "ProductId", "ProductResource", "CostDifference", "NewCost", "OldCost", "AdjustmentNewCost", "AdjustmentOldCost", "AdjustCostDifference", "LoggedTime", "UserCreated", "IsReversed", "StoreCode", "Resource", "Status", "Reason") FROM stdin;
    public       postgres    false    1882   0�      �          0    271738    base_CountStock 
   TABLE DATA               �   COPY "base_CountStock" ("Id", "DocumentNo", "DateCreated", "UserCreated", "CompletedDate", "UserCounted", "Status", "Resource") FROM stdin;
    public       postgres    false    1872    �      �          0    271745    base_CountStockDetail 
   TABLE DATA               �   COPY "base_CountStockDetail" ("Id", "CountStockId", "ProductId", "ProductResource", "StoreId", "Quantity", "CountedQuantity", "Difference") FROM stdin;
    public       postgres    false    1874   e�      m          0    245340    base_Department 
   TABLE DATA               �   COPY "base_Department" ("Id", "Name", "ParentId", "TaxCodeId", "Margin", "MarkUp", "LevelId", "IsActived", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated") FROM stdin;
    public       postgres    false    1798   ��      Z          0    238237 
   base_Email 
   TABLE DATA               �  COPY "base_Email" ("Id", "Recipient", "CC", "BCC", "Subject", "Body", "IsHasAttachment", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "AttachmentType", "AttachmentResult", "GuestId", "Sender", "Status", "Importance", "Sensitivity", "IsRequestDelivery", "IsRequestRead", "IsMyFlag", "FlagTo", "FlagStartDate", "FlagDueDate", "IsAllowReminder", "RemindOn", "MyRemindTimes", "IsRecipentFlag", "RecipentFlagTo", "IsAllowRecipentReminder", "RecipentRemindOn", "RecipentRemindTimes") FROM stdin;
    public       postgres    false    1761   ��      Y          0    238137    base_EmailAttachment 
   TABLE DATA               J   COPY "base_EmailAttachment" ("Id", "EmailId", "AttachmentId") FROM stdin;
    public       postgres    false    1760   ��      ]          0    244817 
   base_Guest 
   TABLE DATA                 COPY "base_Guest" ("Id", "FirstName", "MiddleName", "LastName", "Company", "Phone1", "Ext1", "Phone2", "Ext2", "Fax", "CellPhone", "Email", "Website", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "IsPurged", "GuestTypeId", "IsActived", "GuestNo", "PositionId", "Department", "Mark", "AccountNumber", "ParentId", "IsRewardMember", "CheckLimit", "CreditLimit", "BalanceDue", "AvailCredit", "PastDue", "IsPrimary", "CommissionPercent", "Resource", "TotalRewardRedeemed", "PurchaseDuringTrackingPeriod", "RequirePurchaseNextReward", "HireDate", "IsBlockArriveLate", "IsDeductLunchTime", "IsBalanceOvertime", "LateMinutes", "OvertimeOption", "OTLeastMinute", "IsTrackingHour", "TermDiscount", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "SaleRepId") FROM stdin;
    public       postgres    false    1766   ��      n          0    245376    base_GuestAdditional 
   TABLE DATA               3  COPY "base_GuestAdditional" ("Id", "TaxRate", "IsNoDiscount", "FixDiscount", "Unit", "PriceSchemeId", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Custom8", "GuestId", "LayawayNo", "ChargeACNo", "FedTaxId", "IsTaxExemption", "SaleTaxLocation", "TaxExemptionNo") FROM stdin;
    public       postgres    false    1800   	�      ^          0    244863    base_GuestAddress 
   TABLE DATA               �   COPY "base_GuestAddress" ("Id", "GuestId", "AddressTypeId", "AddressLine1", "AddressLine2", "City", "StateProvinceId", "PostalCode", "CountryId", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsDefault") FROM stdin;
    public       postgres    false    1768   b�      [          0    238413    base_GuestFingerPrint 
   TABLE DATA               �   COPY "base_GuestFingerPrint" ("Id", "GuestId", "FingerIndex", "HandFlag", "DateUpdated", "UserUpdaed", "FingerPrintImage") FROM stdin;
    public       postgres    false    1763   B�      _          0    244873    base_GuestHiringHistory 
   TABLE DATA               �   COPY "base_GuestHiringHistory" ("Id", "GuestId", "StartDate", "RenewDate", "PromotionDate", "TerminateDate", "IsTerminate", "ManagerId") FROM stdin;
    public       postgres    false    1770   _�      `          0    244884    base_GuestPayRoll 
   TABLE DATA               �   COPY "base_GuestPayRoll" ("Id", "PayrollName", "PayrollType", "Rate", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "GuestId") FROM stdin;
    public       postgres    false    1772   |�                0    257325    base_GuestPaymentCard 
   TABLE DATA               �   COPY "base_GuestPaymentCard" ("Id", "GuestId", "CardTypeId", "CardNumber", "ExpMonth", "ExpYear", "CCID", "BillingAddress", "NameOnCard", "ZipCode", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated") FROM stdin;
    public       postgres    false    1832   ��      b          0    244934    base_GuestProfile 
   TABLE DATA               s  COPY "base_GuestProfile" ("Id", "Gender", "Marital", "SSN", "Identification", "DOB", "IsSpouse", "FirstName", "LastName", "MiddleName", "State", "SGender", "SFirstName", "SLastName", "SMiddleName", "SPhone", "SCellPhone", "SSSN", "SState", "SEmail", "IsEmergency", "EFirstName", "ELastName", "EMiddleName", "EPhone", "ECellPhone", "ERelationship", "GuestId") FROM stdin;
    public       postgres    false    1776   ��      �          0    268354    base_GuestReward 
   TABLE DATA               �   COPY "base_GuestReward" ("Id", "GuestId", "RewardId", "Amount", "IsApply", "EarnedDate", "AppliedDate", "RewardValue", "SaleOrderResource", "SaleOrderNo", "Remark", "ActivedDate", "ExpireDate", "Reason", "Status") FROM stdin;
    public       postgres    false    1854   >�      {          0    256013    base_GuestSchedule 
   TABLE DATA               i   COPY "base_GuestSchedule" ("GuestId", "WorkScheduleId", "StartDate", "AssignDate", "Status") FROM stdin;
    public       postgres    false    1823   ��      d          0    244997    base_MemberShip 
   TABLE DATA               �   COPY "base_MemberShip" ("Id", "GuestId", "MemberType", "CardNumber", "Status", "IsPurged", "UserCreated", "UserUpdated", "DateCreated", "DateUpdated", "Code", "TotalRewardRedeemed") FROM stdin;
    public       postgres    false    1780   �      �          0    268511    base_PricingChange 
   TABLE DATA               �   COPY "base_PricingChange" ("Id", "PricingManagerId", "PricingManagerResource", "ProductId", "ProductResource", "Cost", "CurrentPrice", "NewPrice", "PriceChanged", "DateCreated") FROM stdin;
    public       postgres    false    1856   3�      �          0    268185    base_PricingManager 
   TABLE DATA               +  COPY "base_PricingManager" ("Id", "Name", "Description", "DateCreated", "UserCreated", "DateApplied", "UserApplied", "DateRestored", "UserRestored", "AffectPricing", "Resource", "PriceLevel", "Status", "BasePrice", "CalculateMethod", "AmountChange", "AmountUnit", "ItemCount", "Reason") FROM stdin;
    public       postgres    false    1852   P�      o          0    245412    base_Product 
   TABLE DATA               �  COPY "base_Product" ("Id", "Code", "ItemTypeId", "ProductDepartmentId", "ProductCategoryId", "ProductBrandId", "StyleModel", "ProductName", "Description", "Barcode", "Attribute", "Size", "IsSerialTracking", "IsPublicWeb", "OnHandStore1", "OnHandStore2", "OnHandStore3", "OnHandStore4", "OnHandStore5", "OnHandStore6", "OnHandStore7", "OnHandStore8", "OnHandStore9", "OnHandStore10", "QuantityOnHand", "QuantityOnOrder", "CompanyReOrderPoint", "IsUnOrderAble", "IsEligibleForCommission", "IsEligibleForReward", "RegularPrice", "Price1", "Price2", "Price3", "Price4", "OrderCost", "AverageUnitCost", "TaxCode", "MarginPercent", "MarkupPercent", "BaseUOMId", "GroupAttribute", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6", "Custom7", "Resource", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "WarrantyType", "WarrantyNumber", "WarrantyPeriod", "PartNumber", "SellUOMId", "OrderUOMId", "IsPurge", "VendorId", "UserAssignedCommission", "AssignedCommissionPercent", "AssignedCommissionAmount", "Serial", "OrderUOM", "MarkdownPercent1", "MarkdownPercent2", "MarkdownPercent3", "MarkdownPercent4", "IsOpenItem", "Location", "QuantityOnCustomer") FROM stdin;
    public       postgres    false    1802   m�      r          0    255536    base_ProductStore 
   TABLE DATA               �   COPY "base_ProductStore" ("Id", "ProductId", "QuantityOnHand", "StoreCode", "QuantityOnCustomer", "QuantityOnOrder", "ReorderPoint", "QuantityAvailable") FROM stdin;
    public       postgres    false    1807   9�      �          0    270252    base_ProductUOM 
   TABLE DATA               "  COPY "base_ProductUOM" ("Id", "ProductStoreId", "UOMId", "BaseUnitNumber", "RegularPrice", "QuantityOnHand", "AverageCost", "Price1", "Price2", "Price3", "Price4", "MarkDownPercent1", "MarkDownPercent2", "MarkDownPercent3", "MarkDownPercent4", "MarginPercent", "MarkupPercent") FROM stdin;
    public       postgres    false    1870   ;�      l          0    245169    base_Promotion 
   TABLE DATA               �  COPY "base_Promotion" ("Id", "Name", "Description", "PromotionTypeId", "TakeOffOption", "TakeOff", "BuyingQty", "GetingValue", "IsApplyToAboveQuantities", "Status", "AffectDiscount", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource", "CouponExpire", "IsCouponExpired", "PriceSchemaRange", "ReasonReActive", "Sold", "TotalPrice", "CategoryId", "VendorId", "CouponBarCode", "BarCodeNumber", "BarCodeImage", "IsConflict") FROM stdin;
    public       postgres    false    1796   8�      k          0    245155    base_PromotionAffect 
   TABLE DATA               �   COPY "base_PromotionAffect" ("Id", "PromotionId", "ItemId", "Price1", "Price2", "Price3", "Price4", "Price5", "Discount1", "Discount2", "Discount3", "Discount4", "Discount5") FROM stdin;
    public       postgres    false    1794   �      e          0    245023    base_PromotionSchedule 
   TABLE DATA               X   COPY "base_PromotionSchedule" ("Id", "PromotionId", "EndDate", "StartDate") FROM stdin;
    public       postgres    false    1782   $�      �          0    266551    base_PurchaseOrder 
   TABLE DATA               _  COPY "base_PurchaseOrder" ("Id", "PurchaseOrderNo", "VendorCode", "Status", "ShipAddress", "PurchasedDate", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "PaymentDueDate", "PaymentMethodId", "Remark", "ShipDate", "SubTotal", "DiscountPercent", "DiscountAmount", "Freight", "Fee", "Total", "Paid", "Balance", "ItemCount", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "DateUpdate", "UserUpdated", "Resource", "CancelDate", "IsFullWorkflow", "StoreCode", "RecRemark", "PaymentName", "IsPurge", "IsLocked", "VendorResource") FROM stdin;
    public       postgres    false    1844   Q�      �          0    266530    base_PurchaseOrderDetail 
   TABLE DATA               9  COPY "base_PurchaseOrderDetail" ("Id", "PurchaseOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "BaseUOM", "UOMId", "Price", "Quantity", "ReceivedQty", "DueQty", "UnFilledQty", "Amount", "Serial", "LastReceived", "Resource", "IsFullReceived", "Discount", "OnHandQty") FROM stdin;
    public       postgres    false    1842   �      �          0    267535    base_PurchaseOrderReceive 
   TABLE DATA               �   COPY "base_PurchaseOrderReceive" ("Id", "PurchaseOrderDetailId", "POResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "RecQty", "IsReceived", "ReceiveDate", "Resource", "Price") FROM stdin;
    public       postgres    false    1850   !�      �          0    282642    base_QuantityAdjustment 
   TABLE DATA               �   COPY "base_QuantityAdjustment" ("Id", "ProductId", "ProductResource", "CostDifference", "OldQty", "NewQty", "AdjustmentQtyDiff", "LoggedTime", "UserCreated", "IsReversed", "StoreCode", "Resource", "Status", "Reason") FROM stdin;
    public       postgres    false    1880   >�      }          0    256178    base_ResourceAccount 
   TABLE DATA               �   COPY "base_ResourceAccount" ("Id", "Resource", "UserResource", "LoginName", "Password", "ExpiredDate", "IsLocked", "IsExpired") FROM stdin;
    public       postgres    false    1827   ,�      p          0    246083    base_ResourceNote 
   TABLE DATA               X   COPY "base_ResourceNote" ("Id", "Note", "DateCreated", "Color", "Resource") FROM stdin;
    public       postgres    false    1804   T�      �          0    270150    base_ResourcePayment 
   TABLE DATA               3  COPY "base_ResourcePayment" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalPaid", "Balance", "Change", "DateCreated", "UserCreated", "Remark", "Resource", "SubTotal", "DiscountPercent", "DiscountAmount", "Mark", "IsDeposit", "TaxCode", "TaxAmount", "LastRewardAmount", "Cashier") FROM stdin;
    public       postgres    false    1866   ��      �          0    270072    base_ResourcePaymentDetail 
   TABLE DATA               �   COPY "base_ResourcePaymentDetail" ("Id", "PaymentType", "ResourcePaymentId", "PaymentMethodId", "PaymentMethod", "CardType", "Paid", "Change", "Tip", "GiftCardNo", "Reason", "Reference") FROM stdin;
    public       postgres    false    1864   X�      a          0    244922    base_ResourcePhoto 
   TABLE DATA               �   COPY "base_ResourcePhoto" ("Id", "ThumbnailPhoto", "ThumbnailPhotoFilename", "LargePhoto", "LargePhotoFilename", "SortId", "Resource") FROM stdin;
    public       postgres    false    1774   ~�      �          0    270193    base_ResourceReturn 
   TABLE DATA                 COPY "base_ResourceReturn" ("Id", "DocumentResource", "DocumentNo", "TotalAmount", "TotalRefund", "Balance", "DateCreated", "UserCreated", "Resource", "Mark", "DiscountPercent", "DiscountAmount", "Freight", "SubTotal", "ReturnFee", "ReturnFeePercent", "Redeemed") FROM stdin;
    public       postgres    false    1868   ��      �          0    272099    base_ResourceReturnDetail 
   TABLE DATA               �   COPY "base_ResourceReturnDetail" ("Id", "ResourceReturnId", "OrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Price", "ReturnQty", "Amount", "IsReturned", "ReturnedDate", "Discount") FROM stdin;
    public       postgres    false    1876   �      �          0    266843    base_RewardManager 
   TABLE DATA               �  COPY "base_RewardManager" ("Id", "StoreCode", "PurchaseThreshold", "RewardAmount", "RewardAmtType", "RewardExpiration", "IsAutoEnroll", "IsPromptEnroll", "IsInformCashier", "IsRedemptionLimit", "RedemptionLimitAmount", "IsBlockRedemption", "RedemptionAfterDays", "IsBlockPurchaseRedeem", "IsTrackingPeriod", "StartDate", "EndDate", "IsNoEndDay", "TotalRewardRedeemed", "IsActived", "ReasonReActive", "DateCreated") FROM stdin;
    public       postgres    false    1848   a
      �          0    266606    base_SaleCommission 
   TABLE DATA               �   COPY "base_SaleCommission" ("Id", "GuestResource", "SOResource", "SONumber", "SOTotal", "SODate", "ComissionPercent", "CommissionAmount", "Sign", "Remark") FROM stdin;
    public       postgres    false    1846   �
      �          0    266093    base_SaleOrder 
   TABLE DATA               �  COPY "base_SaleOrder" ("Id", "SONumber", "OrderDate", "OrderStatus", "BillAddressId", "BillAddress", "ShipAddressId", "ShipAddress", "PromotionCode", "SaleRep", "CustomerResource", "PriceSchemaId", "DueDate", "RequestShipDate", "SubTotal", "TaxLocation", "TaxCode", "DiscountAmount", "TaxAmount", "DiscountPercent", "TaxPercent", "Shipping", "Total", "Paid", "Balance", "RefundedAmount", "IsMultiPayment", "Remark", "IsFullWorkflow", "QtyOrdered", "QtyDue", "QtyReceived", "UnFilled", "UserCreated", "DateCreated", "UserUpdated", "DateUpdated", "Resource", "BookingChanel", "ShippedCount", "Deposit", "Transaction", "TermDiscountPercent", "TermNetDue", "TermPaidWithinDay", "PaymentTermDescription", "IsTaxExemption", "TaxExemption", "ShippedBox", "PackedQty", "TotalWeight", "WeightUnit", "StoreCode", "IsRedeeem", "IsPurge", "IsLocked", "RewardAmount", "Cashier", "IsQuationConverted") FROM stdin;
    public       postgres    false    1836   �
      �          0    266084    base_SaleOrderDetail 
   TABLE DATA               �  COPY "base_SaleOrderDetail" ("Id", "SaleOrderId", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "TaxCode", "Quantity", "PickQty", "DueQty", "UnFilled", "RegularPrice", "SalePrice", "UOMId", "UOM", "DiscountPercent", "DiscountAmount", "SubTotal", "OnHandQty", "SerialTracking", "Resource", "BalanceShipped", "Comment", "TotalDiscount", "PromotionId", "IsManual") FROM stdin;
    public       postgres    false    1834   �      �          0    266180    base_SaleOrderShip 
   TABLE DATA               �   COPY "base_SaleOrderShip" ("Id", "SaleOrderId", "SaleOrderResource", "Weight", "TrackingNo", "IsShipped", "Resource", "Remark", "Carrier", "ShipDate", "BoxNo") FROM stdin;
    public       postgres    false    1838   �      �          0    266357    base_SaleOrderShipDetail 
   TABLE DATA               �   COPY "base_SaleOrderShipDetail" ("Id", "SaleOrderShipId", "SaleOrderShipResource", "SaleOrderDetailResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Description", "SerialTracking", "PackedQty", "IsPaid") FROM stdin;
    public       postgres    false    1840   �      g          0    245103    base_SaleTaxLocation 
   TABLE DATA               �   COPY "base_SaleTaxLocation" ("Id", "ParentId", "Name", "IsShipingTaxable", "ShippingTaxCodeId", "IsActived", "LevelId", "TaxCode", "TaxCodeName", "TaxPrintMark", "TaxOption", "IsPrimary", "SortIndex", "IsTaxAfterDiscount") FROM stdin;
    public       postgres    false    1786         f          0    245084    base_SaleTaxLocationOption 
   TABLE DATA               �   COPY "base_SaleTaxLocationOption" ("Id", "SaleTaxLocationId", "ParentId", "TaxRate", "TaxComponent", "TaxAgency", "TaxCondition", "IsApplyAmountOver", "IsAllowSpecificItemPriceRange", "IsAllowAmountItemPriceRange", "PriceFrom", "PriceTo") FROM stdin;
    public       postgres    false    1784   r      s          0    255675 
   base_Store 
   TABLE DATA               G   COPY "base_Store" ("Id", "Code", "Name", "Street", "City") FROM stdin;
    public       postgres    false    1809   �      �          0    269925    base_TransferStock 
   TABLE DATA                 COPY "base_TransferStock" ("Id", "TransferNo", "FromStore", "ToStore", "TotalQuantity", "ShipDate", "Carier", "ShippingFee", "Comment", "Resource", "UserCreated", "DateCreated", "Status", "SubTotal", "Total", "DateApplied", "UserApplied", "DateReversed", "UserReversed") FROM stdin;
    public       postgres    false    1860   �      �          0    269941    base_TransferStockDetail 
   TABLE DATA               �   COPY "base_TransferStockDetail" ("Id", "TransferStockId", "TransferStockResource", "ProductResource", "ItemCode", "ItemName", "ItemAtribute", "ItemSize", "Quantity", "UOMId", "BaseUOM", "Amount", "SerialTracking", "AvlQuantity") FROM stdin;
    public       postgres    false    1862   e      j          0    245147    base_UOM 
   TABLE DATA               �   COPY "base_UOM" ("Id", "Code", "Name", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "IsActived", "Resource") FROM stdin;
    public       postgres    false    1792         i          0    245131    base_UserLog 
   TABLE DATA               y   COPY "base_UserLog" ("Id", "IpSource", "ConnectedOn", "DisConnectedOn", "ResourceAccessed", "IsDisconected") FROM stdin;
    public       postgres    false    1790   �      \          0    244282    base_UserLogDetail 
   TABLE DATA               m   COPY "base_UserLogDetail" ("Id", "UserLogId", "AccessedTime", "ModuleName", "ActionDescription") FROM stdin;
    public       postgres    false    1764   _      ~          0    256244    base_UserRight 
   TABLE DATA               9   COPY "base_UserRight" ("Id", "Code", "Name") FROM stdin;
    public       postgres    false    1829   �9      �          0    269643    base_VendorProduct 
   TABLE DATA               t   COPY "base_VendorProduct" ("Id", "ProductId", "VendorId", "Price", "ProductResource", "VendorResource") FROM stdin;
    public       postgres    false    1857   :      h          0    245115    base_VirtualFolder 
   TABLE DATA               �   COPY "base_VirtualFolder" ("Id", "ParentFolderId", "FolderName", "IsActived", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "Resource") FROM stdin;
    public       postgres    false    1788   �:      �          0    282433 	   rpt_Group 
   TABLE DATA               R   COPY "rpt_Group" ("Id", "Code", "Name", "DateCreated", "UserCreated") FROM stdin;
    public       postgres    false    1877   M;      �          0    283470 
   rpt_Report 
   TABLE DATA               \  COPY "rpt_Report" ("Id", "GroupId", "ParentId", "Code", "Name", "FormatFile", "IsShow", "PreProcessName", "SamplePicture", "PrintTimes", "LastPrintDate", "LastPrintUser", "ExcelFile", "PrinterName", "PrintCopy", "Remark", "DateCreated", "UserCreated", "DateUpdated", "UserUpdated", "PaperSize", "ScreenTimes", "PrepProcessDescription") FROM stdin;
    public       postgres    false    1884   �;      t          0    255696    tims_Holiday 
   TABLE DATA               �   COPY "tims_Holiday" ("Id", "Title", "Description", "HolidayOption", "FromDate", "ToDate", "Month", "Day", "DayOfWeek", "WeekOfMonth", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedByID") FROM stdin;
    public       postgres    false    1811   b�      u          0    255705    tims_HolidayHistory 
   TABLE DATA               8   COPY "tims_HolidayHistory" ("Date", "Name") FROM stdin;
    public       postgres    false    1812   �      y          0    255849    tims_TimeLog 
   TABLE DATA               �  COPY "tims_TimeLog" ("Id", "EmployeeId", "WorkScheduleId", "PayrollId", "ClockIn", "ClockOut", "ManualClockInFlag", "ManualClockOutFlag", "WorkTime", "LunchTime", "OvertimeBefore", "Reason", "DeductLunchTimeFlag", "LateTime", "LeaveEarlyTime", "ActiveFlag", "ModifiedDate", "ModifiedById", "OvertimeAfter", "OvertimeLunch", "OvertimeDayOff", "OvertimeOptions", "GuestResource") FROM stdin;
    public       postgres    false    1820   ��      z          0    255865    tims_TimeLogPermission 
   TABLE DATA               L   COPY "tims_TimeLogPermission" ("TimeLogId", "WorkPermissionId") FROM stdin;
    public       postgres    false    1822   ��      x          0    255795    tims_WorkPermission 
   TABLE DATA               �   COPY "tims_WorkPermission" ("Id", "EmployeeId", "PermissionType", "FromDate", "ToDate", "Note", "NoOfDays", "HourPerDay", "PaidFlag", "ActiveFlag", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById", "OvertimeOptions") FROM stdin;
    public       postgres    false    1818   ��      v          0    255738    tims_WorkSchedule 
   TABLE DATA               �   COPY "tims_WorkSchedule" ("Id", "WorkScheduleName", "WorkScheduleType", "Rotate", "Status", "CreatedDate", "CreatedById", "ModifiedDate", "ModifiedById") FROM stdin;
    public       postgres    false    1814   ��      w          0    255781    tims_WorkWeek 
   TABLE DATA               �   COPY "tims_WorkWeek" ("Id", "WorkScheduleId", "Week", "Day", "WorkIn", "WorkOut", "LunchOut", "LunchIn", "LunchBreakFlag") FROM stdin;
    public       postgres    false    1816   �      0
           2606    235700    pga_exception_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_pkey PRIMARY KEY (jexid);
 K   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_pkey;
       pgagent         postgres    false    1745    1745            2
           2606    235702    pga_job_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_pkey PRIMARY KEY (jobid);
 ?   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_pkey;
       pgagent         postgres    false    1747    1747            4
           2606    235704    pga_jobagent_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY pga_jobagent
    ADD CONSTRAINT pga_jobagent_pkey PRIMARY KEY (jagpid);
 I   ALTER TABLE ONLY pgagent.pga_jobagent DROP CONSTRAINT pga_jobagent_pkey;
       pgagent         postgres    false    1749    1749            7
           2606    235706    pga_jobclass_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_jobclass
    ADD CONSTRAINT pga_jobclass_pkey PRIMARY KEY (jclid);
 I   ALTER TABLE ONLY pgagent.pga_jobclass DROP CONSTRAINT pga_jobclass_pkey;
       pgagent         postgres    false    1750    1750            :
           2606    235708    pga_joblog_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_pkey PRIMARY KEY (jlgid);
 E   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_pkey;
       pgagent         postgres    false    1752    1752            =
           2606    235710    pga_jobstep_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_pkey PRIMARY KEY (jstid);
 G   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_pkey;
       pgagent         postgres    false    1754    1754            @
           2606    235712    pga_jobsteplog_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_pkey PRIMARY KEY (jslid);
 M   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_pkey;
       pgagent         postgres    false    1756    1756            C
           2606    235714    pga_schedule_pkey 
   CONSTRAINT     X   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_pkey PRIMARY KEY (jscid);
 I   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_pkey;
       pgagent         postgres    false    1758    1758            �
           2606    245348    FK_base_Department_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_Id";
       public         postgres    false    1798    1798            �
           2606    256188    FPK_base_ResourceAccount_Id 
   CONSTRAINT     y   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "FPK_base_ResourceAccount_Id" PRIMARY KEY ("Id", "Resource");
 ^   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "FPK_base_ResourceAccount_Id";
       public         postgres    false    1827    1827    1827            t
           2606    245266    PF_base_SaleTaxLocation 
   CONSTRAINT     i   ALTER TABLE ONLY "base_SaleTaxLocation"
    ADD CONSTRAINT "PF_base_SaleTaxLocation" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_SaleTaxLocation" DROP CONSTRAINT "PF_base_SaleTaxLocation";
       public         postgres    false    1786    1786                       2606    282455    PF_rpt_Group_Id 
   CONSTRAINT     V   ALTER TABLE ONLY "rpt_Group"
    ADD CONSTRAINT "PF_rpt_Group_Id" PRIMARY KEY ("Id");
 G   ALTER TABLE ONLY public."rpt_Group" DROP CONSTRAINT "PF_rpt_Group_Id";
       public         postgres    false    1877    1877            �
           2606    255762    PF_tims_Holiday_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "tims_Holiday"
    ADD CONSTRAINT "PF_tims_Holiday_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."tims_Holiday" DROP CONSTRAINT "PF_tims_Holiday_Id";
       public         postgres    false    1811    1811            �
           2606    245385    PK_GuestAdditional_Id 
   CONSTRAINT     g   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "PK_GuestAdditional_Id" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "PK_GuestAdditional_Id";
       public         postgres    false    1800    1800            N
           2606    244286    PK_UserLogDetail_Id 
   CONSTRAINT     c   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "PK_UserLogDetail_Id" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "PK_UserLogDetail_Id";
       public         postgres    false    1764    1764            z
           2606    245136    PK_UserLog_Id 
   CONSTRAINT     W   ALTER TABLE ONLY "base_UserLog"
    ADD CONSTRAINT "PK_UserLog_Id" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_UserLog" DROP CONSTRAINT "PK_UserLog_Id";
       public         postgres    false    1790    1790            h
           2606    244954    PK_base_Attachment_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "PK_base_Attachment_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "PK_base_Attachment_Id";
       public         postgres    false    1778    1778            �
           2606    256191    PK_base_Authorize_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Authorize"
    ADD CONSTRAINT "PK_base_Authorize_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Authorize" DROP CONSTRAINT "PK_base_Authorize_Id";
       public         postgres    false    1825    1825                       2606    283493    PK_base_CostAdjustment_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "PK_base_CostAdjustment_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "PK_base_CostAdjustment_Id";
       public         postgres    false    1882    1882                       2606    271757    PK_base_CounStockDetail_Id 
   CONSTRAINT     m   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "PK_base_CounStockDetail_Id" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "PK_base_CounStockDetail_Id";
       public         postgres    false    1874    1874                       2606    271755    PK_base_CounStock_Id 
   CONSTRAINT     a   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "PK_base_CounStock_Id" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "PK_base_CounStock_Id";
       public         postgres    false    1872    1872            E
           2606    238143    PK_base_EmailAttachment 
   CONSTRAINT     i   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "PK_base_EmailAttachment" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "PK_base_EmailAttachment";
       public         postgres    false    1760    1760            G
           2606    238253    PK_base_Email_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Email"
    ADD CONSTRAINT "PK_base_Email_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Email" DROP CONSTRAINT "PK_base_Email_Id";
       public         postgres    false    1761    1761            K
           2606    238418    PK_base_GuestFingerPrint_Id 
   CONSTRAINT     n   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "PK_base_GuestFingerPrint_Id" PRIMARY KEY ("Id");
 _   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "PK_base_GuestFingerPrint_Id";
       public         postgres    false    1763    1763            [
           2606    244879    PK_base_GuestHiringHistory_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "PK_base_GuestHiringHistory_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "PK_base_GuestHiringHistory_Id";
       public         postgres    false    1770    1770            `
           2606    244890    PK_base_GuestPayRoll_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "PK_base_GuestPayRoll_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "PK_base_GuestPayRoll_Id";
       public         postgres    false    1772    1772            e
           2606    244941    PK_base_GuestProfile_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "PK_base_GuestProfile_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "PK_base_GuestProfile_Id";
       public         postgres    false    1776    1776            �
           2606    268362    PK_base_GuestReward_Id 
   CONSTRAINT     d   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "PK_base_GuestReward_Id" PRIMARY KEY ("Id");
 U   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "PK_base_GuestReward_Id";
       public         postgres    false    1854    1854            �
           2606    256030    PK_base_GuestSchedule 
   CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "PK_base_GuestSchedule" PRIMARY KEY ("GuestId", "WorkScheduleId", "StartDate");
 V   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "PK_base_GuestSchedule";
       public         postgres    false    1823    1823    1823    1823            X
           2606    244869    PK_base_Guest_Id 
   CONSTRAINT     _   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "PK_base_Guest_Id" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "PK_base_Guest_Id";
       public         postgres    false    1768    1768            l
           2606    245005    PK_base_MemberShip 
   CONSTRAINT     _   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "PK_base_MemberShip" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "PK_base_MemberShip";
       public         postgres    false    1780    1780            �
           2606    268520    PK_base_PricingChange_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "PK_base_PricingChange_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "PK_base_PricingChange_Id";
       public         postgres    false    1856    1856            �
           2606    268194    PK_base_PricingManager_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "PK_base_PricingManager_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "PK_base_PricingManager_Id";
       public         postgres    false    1852    1852            �
           2606    255541    PK_base_ProductStore_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "PK_base_ProductStore_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "PK_base_ProductStore_Id";
       public         postgres    false    1807    1807                       2606    270271    PK_base_ProductUOM_Id 
   CONSTRAINT     b   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "PK_base_ProductUOM_Id" PRIMARY KEY ("Id");
 S   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "PK_base_ProductUOM_Id";
       public         postgres    false    1870    1870            �
           2606    255615    PK_base_Product_Id 
   CONSTRAINT     \   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "PK_base_Product_Id" PRIMARY KEY ("Id");
 M   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "PK_base_Product_Id";
       public         postgres    false    1802    1802            �
           2606    245160    PK_base_PromotionAffect_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "PK_base_PromotionAffect_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "PK_base_PromotionAffect_Id";
       public         postgres    false    1794    1794            o
           2606    245030    PK_base_PromotionSchedule_Id 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "PK_base_PromotionSchedule_Id" PRIMARY KEY ("Id");
 a   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "PK_base_PromotionSchedule_Id";
       public         postgres    false    1782    1782            �
           2606    245177    PK_base_Promotion_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Promotion"
    ADD CONSTRAINT "PK_base_Promotion_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_Promotion" DROP CONSTRAINT "PK_base_Promotion_Id";
       public         postgres    false    1796    1796            �
           2606    266538    PK_base_PurchaseOrderItem_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "PK_base_PurchaseOrderItem_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "PK_base_PurchaseOrderItem_Id";
       public         postgres    false    1842    1842            �
           2606    267544    PK_base_PurchaseOrderReceive_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "PK_base_PurchaseOrderReceive_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "PK_base_PurchaseOrderReceive_Id";
       public         postgres    false    1850    1850            �
           2606    266567    PK_base_PurchaseOrder_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "PK_base_PurchaseOrder_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "PK_base_PurchaseOrder_Id";
       public         postgres    false    1844    1844                       2606    283506    PK_base_QuantityAdjustment_Id 
   CONSTRAINT     r   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "PK_base_QuantityAdjustment_Id" PRIMARY KEY ("Id");
 c   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "PK_base_QuantityAdjustment_Id";
       public         postgres    false    1880    1880            �
           2606    246089    PK_base_ResourceNote_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "base_ResourceNote"
    ADD CONSTRAINT "PK_base_ResourceNote_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."base_ResourceNote" DROP CONSTRAINT "PK_base_ResourceNote_Id";
       public         postgres    false    1804    1804            �
           2606    270163     PK_base_ResourcePaymentDetail_Id 
   CONSTRAINT     x   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "PK_base_ResourcePaymentDetail_Id" PRIMARY KEY ("Id");
 i   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "PK_base_ResourcePaymentDetail_Id";
       public         postgres    false    1864    1864            �
           2606    270161    PK_base_ResourcePayment_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_ResourcePayment"
    ADD CONSTRAINT "PK_base_ResourcePayment_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_ResourcePayment" DROP CONSTRAINT "PK_base_ResourcePayment_Id";
       public         postgres    false    1866    1866            b
           2606    270190    PK_base_ResourcePhoto_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_ResourcePhoto"
    ADD CONSTRAINT "PK_base_ResourcePhoto_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_ResourcePhoto" DROP CONSTRAINT "PK_base_ResourcePhoto_Id";
       public         postgres    false    1774    1774                       2606    272108    PK_base_ResourceReturnDetail_Id 
   CONSTRAINT     v   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "PK_base_ResourceReturnDetail_Id" PRIMARY KEY ("Id");
 g   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "PK_base_ResourceReturnDetail_Id";
       public         postgres    false    1876    1876            �
           2606    270203    PK_base_ResourceReturn_Id 
   CONSTRAINT     j   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "PK_base_ResourceReturn_Id" PRIMARY KEY ("Id");
 [   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "PK_base_ResourceReturn_Id";
       public         postgres    false    1868    1868            �
           2606    266851    PK_base_RewardManager 
   CONSTRAINT     e   ALTER TABLE ONLY "base_RewardManager"
    ADD CONSTRAINT "PK_base_RewardManager" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_RewardManager" DROP CONSTRAINT "PK_base_RewardManager";
       public         postgres    false    1848    1848            �
           2606    266611    PK_base_SaleCommission 
   CONSTRAINT     g   ALTER TABLE ONLY "base_SaleCommission"
    ADD CONSTRAINT "PK_base_SaleCommission" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_SaleCommission" DROP CONSTRAINT "PK_base_SaleCommission";
       public         postgres    false    1846    1846            �
           2606    266090    PK_base_SaleOrderDetail_Id 
   CONSTRAINT     l   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "PK_base_SaleOrderDetail_Id" PRIMARY KEY ("Id");
 ]   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "PK_base_SaleOrderDetail_Id";
       public         postgres    false    1834    1834            �
           2606    266362    PK_base_SaleOrderShipDetail 
   CONSTRAINT     q   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "PK_base_SaleOrderShipDetail" PRIMARY KEY ("Id");
 b   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "PK_base_SaleOrderShipDetail";
       public         postgres    false    1840    1840            �
           2606    266219    PK_base_SaleOrderShip_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "PK_base_SaleOrderShip_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "PK_base_SaleOrderShip_Id";
       public         postgres    false    1838    1838            �
           2606    266117    PK_base_SaleOrder_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_SaleOrder"
    ADD CONSTRAINT "PK_base_SaleOrder_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_SaleOrder" DROP CONSTRAINT "PK_base_SaleOrder_Id";
       public         postgres    false    1836    1836            r
           2606    245268    PK_base_SaleTaxLocationOption 
   CONSTRAINT     u   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "PK_base_SaleTaxLocationOption" PRIMARY KEY ("Id");
 f   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "PK_base_SaleTaxLocationOption";
       public         postgres    false    1784    1784            �
           2606    255680    PK_base_Store_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "PK_base_Store_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "PK_base_Store_Id";
       public         postgres    false    1809    1809            �
           2606    269949    PK_base_TransferStockDetail_Id 
   CONSTRAINT     t   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "PK_base_TransferStockDetail_Id" PRIMARY KEY ("Id");
 e   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "PK_base_TransferStockDetail_Id";
       public         postgres    false    1862    1862            �
           2606    269936    PK_base_TransferStock_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "PK_base_TransferStock_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "PK_base_TransferStock_Id";
       public         postgres    false    1860    1860            |
           2606    245152    PK_base_UOM_Id 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "PK_base_UOM_Id" PRIMARY KEY ("Id");
 E   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "PK_base_UOM_Id";
       public         postgres    false    1792    1792            �
           2606    256249    PK_base_UserRight_Id 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "PK_base_UserRight_Id" PRIMARY KEY ("Id");
 Q   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "PK_base_UserRight_Id";
       public         postgres    false    1829    1829            �
           2606    269660    PK_base_VendorProduct_Id 
   CONSTRAINT     h   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "PK_base_VendorProduct_Id" PRIMARY KEY ("Id");
 Y   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "PK_base_VendorProduct_Id";
       public         postgres    false    1857    1857            x
           2606    245122    PK_base_VirtualFolder 
   CONSTRAINT     e   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "PK_base_VirtualFolder" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "PK_base_VirtualFolder";
       public         postgres    false    1788    1788                       2606    283482    PK_rpt_Report_Id 
   CONSTRAINT     X   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "PK_rpt_Report_Id" PRIMARY KEY ("Id");
 I   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "PK_rpt_Report_Id";
       public         postgres    false    1884    1884            �
           2606    255709    PK_tims_HolidayHistory_Date 
   CONSTRAINT     n   ALTER TABLE ONLY "tims_HolidayHistory"
    ADD CONSTRAINT "PK_tims_HolidayHistory_Date" PRIMARY KEY ("Date");
 ]   ALTER TABLE ONLY public."tims_HolidayHistory" DROP CONSTRAINT "PK_tims_HolidayHistory_Date";
       public         postgres    false    1812    1812            �
           2606    255743    PK_tims_WorkSchedule_Id 
   CONSTRAINT     f   ALTER TABLE ONLY "tims_WorkSchedule"
    ADD CONSTRAINT "PK_tims_WorkSchedule_Id" PRIMARY KEY ("Id");
 W   ALTER TABLE ONLY public."tims_WorkSchedule" DROP CONSTRAINT "PK_tims_WorkSchedule_Id";
       public         postgres    false    1814    1814            �
           2606    255786    PK_tims_WorkWeek_Id 
   CONSTRAINT     ^   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "PK_tims_WorkWeek_Id" PRIMARY KEY ("Id");
 O   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "PK_tims_WorkWeek_Id";
       public         postgres    false    1816    1816            �
           2606    257312    base_Configuration_pkey 
   CONSTRAINT     g   ALTER TABLE ONLY "base_Configuration"
    ADD CONSTRAINT "base_Configuration_pkey" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."base_Configuration" DROP CONSTRAINT "base_Configuration_pkey";
       public         postgres    false    1805    1805            �
           2606    257332    base_GuestPaymentCard_Id 
   CONSTRAINT     k   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "base_GuestPaymentCard_Id" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "base_GuestPaymentCard_Id";
       public         postgres    false    1832    1832            R
           2606    244846    base_Guest_pkey 
   CONSTRAINT     W   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "base_Guest_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "base_Guest_pkey";
       public         postgres    false    1766    1766            �
           2606    255870    key 
   CONSTRAINT     p   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT key PRIMARY KEY ("TimeLogId", "WorkPermissionId");
 F   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT key;
       public         postgres    false    1822    1822    1822            �
           2606    255857    pk_tims_timelog 
   CONSTRAINT     W   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT pk_tims_timelog PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT pk_tims_timelog;
       public         postgres    false    1820    1820            �
           2606    255803    pk_tims_workpermission 
   CONSTRAINT     e   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT pk_tims_workpermission PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT pk_tims_workpermission;
       public         postgres    false    1818    1818                       2606    271770    uni_base_CountStock_Resource 
   CONSTRAINT     j   ALTER TABLE ONLY "base_CountStock"
    ADD CONSTRAINT "uni_base_CountStock_Resource" UNIQUE ("Resource");
 Z   ALTER TABLE ONLY public."base_CountStock" DROP CONSTRAINT "uni_base_CountStock_Resource";
       public         postgres    false    1872    1872            V
           2606    256327    uni_base_Guest_Resource 
   CONSTRAINT     `   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "uni_base_Guest_Resource" UNIQUE ("Resource");
 P   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "uni_base_Guest_Resource";
       public         postgres    false    1766    1766            �
           2606    268201    uni_base_PricingManager 
   CONSTRAINT     i   ALTER TABLE ONLY "base_PricingManager"
    ADD CONSTRAINT "uni_base_PricingManager" UNIQUE ("Resource");
 Y   ALTER TABLE ONLY public."base_PricingManager" DROP CONSTRAINT "uni_base_PricingManager";
       public         postgres    false    1852    1852            �
           2606    269972    uni_base_Product_Resource 
   CONSTRAINT     d   ALTER TABLE ONLY "base_Product"
    ADD CONSTRAINT "uni_base_Product_Resource" UNIQUE ("Resource");
 T   ALTER TABLE ONLY public."base_Product" DROP CONSTRAINT "uni_base_Product_Resource";
       public         postgres    false    1802    1802            �
           2606    266569    uni_base_PurchaseOrder_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_PurchaseOrder"
    ADD CONSTRAINT "uni_base_PurchaseOrder_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_PurchaseOrder" DROP CONSTRAINT "uni_base_PurchaseOrder_Resource";
       public         postgres    false    1844    1844            �
           2606    256317 !   uni_base_ResourceAccount_Resource 
   CONSTRAINT     t   ALTER TABLE ONLY "base_ResourceAccount"
    ADD CONSTRAINT "uni_base_ResourceAccount_Resource" UNIQUE ("Resource");
 d   ALTER TABLE ONLY public."base_ResourceAccount" DROP CONSTRAINT "uni_base_ResourceAccount_Resource";
       public         postgres    false    1827    1827                       2606    270205     uni_base_ResourceReturn_Resource 
   CONSTRAINT     r   ALTER TABLE ONLY "base_ResourceReturn"
    ADD CONSTRAINT "uni_base_ResourceReturn_Resource" UNIQUE ("Resource");
 b   ALTER TABLE ONLY public."base_ResourceReturn" DROP CONSTRAINT "uni_base_ResourceReturn_Resource";
       public         postgres    false    1868    1868            �
           2606    266303    uni_base_SaleOrderDetail 
   CONSTRAINT     k   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "uni_base_SaleOrderDetail" UNIQUE ("Resource");
 [   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "uni_base_SaleOrderDetail";
       public         postgres    false    1834    1834            �
           2606    266221    uni_base_SaleOrderShip_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "uni_base_SaleOrderShip_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "uni_base_SaleOrderShip_Resource";
       public         postgres    false    1838    1838            �
           2606    255948    uni_base_Store_Code 
   CONSTRAINT     X   ALTER TABLE ONLY "base_Store"
    ADD CONSTRAINT "uni_base_Store_Code" UNIQUE ("Code");
 L   ALTER TABLE ONLY public."base_Store" DROP CONSTRAINT "uni_base_Store_Code";
       public         postgres    false    1809    1809            �
           2606    269938    uni_base_TransferStock_Resource 
   CONSTRAINT     p   ALTER TABLE ONLY "base_TransferStock"
    ADD CONSTRAINT "uni_base_TransferStock_Resource" UNIQUE ("Resource");
 `   ALTER TABLE ONLY public."base_TransferStock" DROP CONSTRAINT "uni_base_TransferStock_Resource";
       public         postgres    false    1860    1860            
           2606    254600    uni_base_UOM_Code 
   CONSTRAINT     T   ALTER TABLE ONLY "base_UOM"
    ADD CONSTRAINT "uni_base_UOM_Code" UNIQUE ("Code");
 H   ALTER TABLE ONLY public."base_UOM" DROP CONSTRAINT "uni_base_UOM_Code";
       public         postgres    false    1792    1792            �
           2606    283719    uni_base_UserRight_Code 
   CONSTRAINT     `   ALTER TABLE ONLY "base_UserRight"
    ADD CONSTRAINT "uni_base_UserRight_Code" UNIQUE ("Code");
 T   ALTER TABLE ONLY public."base_UserRight" DROP CONSTRAINT "uni_base_UserRight_Code";
       public         postgres    false    1829    1829            �
           2606    269675 5   uni_base_VendorProduct_VendorResource_ProductResource 
   CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource" UNIQUE ("ProductResource", "VendorResource");
 v   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "uni_base_VendorProduct_VendorResource_ProductResource";
       public         postgres    false    1857    1857    1857            -
           1259    235939    pga_exception_datetime    INDEX     \   CREATE UNIQUE INDEX pga_exception_datetime ON pga_exception USING btree (jexdate, jextime);
 +   DROP INDEX pgagent.pga_exception_datetime;
       pgagent         postgres    false    1745    1745            .
           1259    235940    pga_exception_jexscid    INDEX     K   CREATE INDEX pga_exception_jexscid ON pga_exception USING btree (jexscid);
 *   DROP INDEX pgagent.pga_exception_jexscid;
       pgagent         postgres    false    1745            5
           1259    235941    pga_jobclass_name    INDEX     M   CREATE UNIQUE INDEX pga_jobclass_name ON pga_jobclass USING btree (jclname);
 &   DROP INDEX pgagent.pga_jobclass_name;
       pgagent         postgres    false    1750            8
           1259    235942    pga_joblog_jobid    INDEX     D   CREATE INDEX pga_joblog_jobid ON pga_joblog USING btree (jlgjobid);
 %   DROP INDEX pgagent.pga_joblog_jobid;
       pgagent         postgres    false    1752            A
           1259    235943    pga_jobschedule_jobid    INDEX     K   CREATE INDEX pga_jobschedule_jobid ON pga_schedule USING btree (jscjobid);
 *   DROP INDEX pgagent.pga_jobschedule_jobid;
       pgagent         postgres    false    1758            ;
           1259    235944    pga_jobstep_jobid    INDEX     F   CREATE INDEX pga_jobstep_jobid ON pga_jobstep USING btree (jstjobid);
 &   DROP INDEX pgagent.pga_jobstep_jobid;
       pgagent         postgres    false    1754            >
           1259    235945    pga_jobsteplog_jslid    INDEX     L   CREATE INDEX pga_jobsteplog_jslid ON pga_jobsteplog USING btree (jsljlgid);
 )   DROP INDEX pgagent.pga_jobsteplog_jslid;
       pgagent         postgres    false    1756            �
           1259    255547 .   FKI_baseProductStore_ProductId_base_Product_Id    INDEX     p   CREATE INDEX "FKI_baseProductStore_ProductId_base_Product_Id" ON "base_ProductStore" USING btree ("ProductId");
 D   DROP INDEX public."FKI_baseProductStore_ProductId_base_Product_Id";
       public         postgres    false    1807            �
           1259    245166 5   FKI_basePromotionAffect_PromotionId_base_Promotion_Id    INDEX     |   CREATE INDEX "FKI_basePromotionAffect_PromotionId_base_Promotion_Id" ON "base_PromotionAffect" USING btree ("PromotionId");
 K   DROP INDEX public."FKI_basePromotionAffect_PromotionId_base_Promotion_Id";
       public         postgres    false    1794            f
           1259    246209 9   FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    INDEX        CREATE INDEX "FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" ON "base_Attachment" USING btree ("VirtualFolderId");
 O   DROP INDEX public."FKI_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public         postgres    false    1778                       1259    283499 $   FKI_base_CostAdjustment_base_Product    INDEX     h   CREATE INDEX "FKI_base_CostAdjustment_base_Product" ON "base_CostAdjustment" USING btree ("ProductId");
 :   DROP INDEX public."FKI_base_CostAdjustment_base_Product";
       public         postgres    false    1882            	           1259    271763 8   FKI_base_CounStockDetail_CountStockId_base_CountStock_id    INDEX     �   CREATE INDEX "FKI_base_CounStockDetail_CountStockId_base_CountStock_id" ON "base_CountStockDetail" USING btree ("CountStockId");
 N   DROP INDEX public."FKI_base_CounStockDetail_CountStockId_base_CountStock_id";
       public         postgres    false    1874            �
           1259    245354    FKI_base_Department_Id_ParentId    INDEX     ^   CREATE INDEX "FKI_base_Department_Id_ParentId" ON "base_Department" USING btree ("ParentId");
 5   DROP INDEX public."FKI_base_Department_Id_ParentId";
       public         postgres    false    1798            �
           1259    245391 &   FKI_base_GuestAdditional_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestAdditional_base_Guest_Id" ON "base_GuestAdditional" USING btree ("GuestId");
 <   DROP INDEX public."FKI_base_GuestAdditional_base_Guest_Id";
       public         postgres    false    1800            ^
           1259    244891 +   FKI_base_GuestPayRoll_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestPayRoll_GuestId_base_Guest_Id" ON "base_GuestPayRoll" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestPayRoll_GuestId_base_Guest_Id";
       public         postgres    false    1772            c
           1259    244942 +   FKI_base_GuestProfile_GuestId_base_Guest_Id    INDEX     k   CREATE INDEX "FKI_base_GuestProfile_GuestId_base_Guest_Id" ON "base_GuestProfile" USING btree ("GuestId");
 A   DROP INDEX public."FKI_base_GuestProfile_GuestId_base_Guest_Id";
       public         postgres    false    1776            �
           1259    268373 *   FKI_base_GuestReward_GuestId_base_Guest_Id    INDEX     i   CREATE INDEX "FKI_base_GuestReward_GuestId_base_Guest_Id" ON "base_GuestReward" USING btree ("GuestId");
 @   DROP INDEX public."FKI_base_GuestReward_GuestId_base_Guest_Id";
       public         postgres    false    1854            P
           1259    245510 %   FKI_base_Guest_ParentId_base_Guest_Id    INDEX     _   CREATE INDEX "FKI_base_Guest_ParentId_base_Guest_Id" ON "base_Guest" USING btree ("ParentId");
 ;   DROP INDEX public."FKI_base_Guest_ParentId_base_Guest_Id";
       public         postgres    false    1766            j
           1259    245006 )   FKI_base_MemberShip_GuestId_base_Guest_Id    INDEX     g   CREATE INDEX "FKI_base_MemberShip_GuestId_base_Guest_Id" ON "base_MemberShip" USING btree ("GuestId");
 ?   DROP INDEX public."FKI_base_MemberShip_GuestId_base_Guest_Id";
       public         postgres    false    1780            �
           1259    268532 >   FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id    INDEX     �   CREATE INDEX "FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id" ON "base_PricingChange" USING btree ("PricingManagerId");
 T   DROP INDEX public."FKI_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public         postgres    false    1856                       1259    270282 .   FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id    INDEX     s   CREATE INDEX "FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id" ON "base_ProductUOM" USING btree ("BaseUnitNumber");
 D   DROP INDEX public."FKI_base_ProductUOM_BaseUnitNumber_base_UOM_Id";
       public         postgres    false    1870            m
           1259    245041 8   FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id    INDEX     �   CREATE INDEX "FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id" ON "base_PromotionSchedule" USING btree ("PromotionId");
 N   DROP INDEX public."FKI_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public         postgres    false    1782            �
           1259    245178 8   FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id    INDEX     }   CREATE INDEX "FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id" ON "base_Promotion" USING btree ("PromotionTypeId");
 N   DROP INDEX public."FKI_base_Promotion_PromotionTypeId_base_PromotionType_Id";
       public         postgres    false    1796            �
           1259    266544 ?   FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder" ON "base_PurchaseOrderDetail" USING btree ("PurchaseOrderId");
 U   DROP INDEX public."FKI_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder";
       public         postgres    false    1842            �
           1259    267550 ?   FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha    INDEX     �   CREATE INDEX "FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha" ON "base_PurchaseOrderReceive" USING btree ("PurchaseOrderDetailId");
 U   DROP INDEX public."FKI_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purcha";
       public         postgres    false    1850            �
           1259    266128 6   FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    INDEX     }   CREATE INDEX "FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderDetail" USING btree ("SaleOrderId");
 L   DROP INDEX public."FKI_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1834            �
           1259    266368 ?   FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip    INDEX     �   CREATE INDEX "FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip" ON "base_SaleOrderShipDetail" USING btree ("SaleOrderShipId");
 U   DROP INDEX public."FKI_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip";
       public         postgres    false    1840            �
           1259    266227 4   FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    INDEX     y   CREATE INDEX "FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" ON "base_SaleOrderShip" USING btree ("SaleOrderId");
 J   DROP INDEX public."FKI_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public         postgres    false    1838            p
           1259    245099 1   FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id    INDEX     �   CREATE INDEX "FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id" ON "base_SaleTaxLocationOption" USING btree ("SaleTaxLocationId");
 G   DROP INDEX public."FKI_base_SaleTaxLocationOption_SaleTaxLocation_Id";
       public         postgres    false    1784            �
           1259    269955 ?   FKI_base_TransferStockDetail_TransferStockId_base_TransferStock    INDEX     �   CREATE INDEX "FKI_base_TransferStockDetail_TransferStockId_base_TransferStock" ON "base_TransferStockDetail" USING btree ("TransferStockId");
 U   DROP INDEX public."FKI_base_TransferStockDetail_TransferStockId_base_TransferStock";
       public         postgres    false    1862            �
           1259    269666 .   FKI_base_VendorProduct_ProductId_base_Guest_Id    INDEX     q   CREATE INDEX "FKI_base_VendorProduct_ProductId_base_Guest_Id" ON "base_VendorProduct" USING btree ("ProductId");
 D   DROP INDEX public."FKI_base_VendorProduct_ProductId_base_Guest_Id";
       public         postgres    false    1857                       1259    283488 #   FKI_rpt_Report_GroupId_rpt_Group_Id    INDEX     \   CREATE INDEX "FKI_rpt_Report_GroupId_rpt_Group_Id" ON "rpt_Report" USING btree ("GroupId");
 9   DROP INDEX public."FKI_rpt_Report_GroupId_rpt_Group_Id";
       public         postgres    false    1884            �
           1259    256148 0   FKI_tims_WorkPermission_EmployeeId_base_Guest_Id    INDEX     u   CREATE INDEX "FKI_tims_WorkPermission_EmployeeId_base_Guest_Id" ON "tims_WorkPermission" USING btree ("EmployeeId");
 F   DROP INDEX public."FKI_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public         postgres    false    1818            L
           1259    244035    idx_GuestFingerPrint_GuestId    INDEX     `   CREATE INDEX "idx_GuestFingerPrint_GuestId" ON "base_GuestFingerPrint" USING btree ("GuestId");
 2   DROP INDEX public."idx_GuestFingerPrint_GuestId";
       public         postgres    false    1763            S
           1259    244839    idx_GuestName    INDEX     _   CREATE INDEX "idx_GuestName" ON "base_Guest" USING btree ("FirstName", "LastName", "Company");
 #   DROP INDEX public."idx_GuestName";
       public         postgres    false    1766    1766    1766            O
           1259    244292    idx_UserLogDetail    INDEX     T   CREATE INDEX "idx_UserLogDetail" ON "base_UserLogDetail" USING btree ("UserLogId");
 '   DROP INDEX public."idx_UserLogDetail";
       public         postgres    false    1764            i
           1259    255513    idx_base_Attachment    INDEX     S   CREATE UNIQUE INDEX "idx_base_Attachment" ON "base_Attachment" USING btree ("Id");
 )   DROP INDEX public."idx_base_Attachment";
       public         postgres    false    1778            �
           1259    256319    idx_base_Authorize_Code    INDEX     Q   CREATE INDEX "idx_base_Authorize_Code" ON "base_Authorize" USING btree ("Code");
 -   DROP INDEX public."idx_base_Authorize_Code";
       public         postgres    false    1825            �
           1259    256318    idx_base_Authorize_Resource    INDEX     Y   CREATE INDEX "idx_base_Authorize_Resource" ON "base_Authorize" USING btree ("Resource");
 1   DROP INDEX public."idx_base_Authorize_Resource";
       public         postgres    false    1825            �
           1259    255517    idx_base_Department_Id    INDEX     W   CREATE INDEX "idx_base_Department_Id" ON "base_Department" USING btree ("Id", "Name");
 ,   DROP INDEX public."idx_base_Department_Id";
       public         postgres    false    1798    1798            H
           1259    238254    idx_base_Email    INDEX     B   CREATE INDEX "idx_base_Email" ON "base_Email" USING btree ("Id");
 $   DROP INDEX public."idx_base_Email";
       public         postgres    false    1761            I
           1259    238260    idx_base_Email_Address    INDEX     N   CREATE INDEX "idx_base_Email_Address" ON "base_Email" USING btree ("Sender");
 ,   DROP INDEX public."idx_base_Email_Address";
       public         postgres    false    1761            Y
           1259    244870    idx_base_GuestAddress_Id    INDEX     S   CREATE INDEX "idx_base_GuestAddress_Id" ON "base_GuestAddress" USING btree ("Id");
 .   DROP INDEX public."idx_base_GuestAddress_Id";
       public         postgres    false    1768            \
           1259    244880     idx_base_GuestHiringHistory_Date    INDEX     �   CREATE INDEX "idx_base_GuestHiringHistory_Date" ON "base_GuestHiringHistory" USING btree ("StartDate", "RenewDate", "PromotionDate");
 6   DROP INDEX public."idx_base_GuestHiringHistory_Date";
       public         postgres    false    1770    1770    1770            ]
           1259    244881    idx_base_GuestHiringHistory_Id    INDEX     d   CREATE INDEX "idx_base_GuestHiringHistory_Id" ON "base_GuestHiringHistory" USING btree ("GuestId");
 4   DROP INDEX public."idx_base_GuestHiringHistory_Id";
       public         postgres    false    1770            �
           1259    257338 !   idx_base_GuestPaymentCard_GuestId    INDEX     e   CREATE INDEX "idx_base_GuestPaymentCard_GuestId" ON "base_GuestPaymentCard" USING btree ("GuestId");
 7   DROP INDEX public."idx_base_GuestPaymentCard_GuestId";
       public         postgres    false    1832            T
           1259    256328    idx_base_Guest_Resource    INDEX     Q   CREATE INDEX "idx_base_Guest_Resource" ON "base_Guest" USING btree ("Resource");
 -   DROP INDEX public."idx_base_Guest_Resource";
       public         postgres    false    1766            �
           1259    257571    idx_base_Product_Code    INDEX     M   CREATE INDEX "idx_base_Product_Code" ON "base_Product" USING btree ("Code");
 +   DROP INDEX public."idx_base_Product_Code";
       public         postgres    false    1802            �
           1259    245794    idx_base_Product_Id    INDEX     I   CREATE INDEX "idx_base_Product_Id" ON "base_Product" USING btree ("Id");
 )   DROP INDEX public."idx_base_Product_Id";
       public         postgres    false    1802            �
           1259    254639    idx_base_Product_Name    INDEX     c   CREATE INDEX "idx_base_Product_Name" ON "base_Product" USING btree ("ProductName", "Description");
 +   DROP INDEX public."idx_base_Product_Name";
       public         postgres    false    1802    1802            �
           1259    271771    idx_base_Product_Resource    INDEX     U   CREATE INDEX "idx_base_Product_Resource" ON "base_Product" USING btree ("Resource");
 /   DROP INDEX public."idx_base_Product_Resource";
       public         postgres    false    1802            �
           1259    256315 !   idx_base_ResourceAccount_Resource    INDEX     u   CREATE INDEX "idx_base_ResourceAccount_Resource" ON "base_ResourceAccount" USING btree ("Resource", "UserResource");
 7   DROP INDEX public."idx_base_ResourceAccount_Resource";
       public         postgres    false    1827    1827            �
           1259    270298 ,   idx_base_ResourcePayment_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourcePayment_DocumentResource_No" ON "base_ResourcePayment" USING btree ("DocumentNo", "DocumentResource");
 B   DROP INDEX public."idx_base_ResourcePayment_DocumentResource_No";
       public         postgres    false    1866    1866            �
           1259    270208    idx_base_ResourcePayment_Id    INDEX     Y   CREATE INDEX "idx_base_ResourcePayment_Id" ON "base_ResourcePayment" USING btree ("Id");
 1   DROP INDEX public."idx_base_ResourcePayment_Id";
       public         postgres    false    1866            �
           1259    271706 +   idx_base_ResourceReturn_DocumentResource_No    INDEX     �   CREATE INDEX "idx_base_ResourceReturn_DocumentResource_No" ON "base_ResourceReturn" USING btree ("DocumentNo", "DocumentResource");
 A   DROP INDEX public."idx_base_ResourceReturn_DocumentResource_No";
       public         postgres    false    1868    1868            �
           1259    266266    idx_base_SaleOrder_Resource    INDEX     Y   CREATE INDEX "idx_base_SaleOrder_Resource" ON "base_SaleOrder" USING btree ("Resource");
 1   DROP INDEX public."idx_base_SaleOrder_Resource";
       public         postgres    false    1836            u
           1259    245314    idx_base_SaleTaxLocation_Id    INDEX     Y   CREATE INDEX "idx_base_SaleTaxLocation_Id" ON "base_SaleTaxLocation" USING btree ("Id");
 1   DROP INDEX public."idx_base_SaleTaxLocation_Id";
       public         postgres    false    1786            v
           1259    245313     idx_base_SaleTaxLocation_TaxCode    INDEX     c   CREATE INDEX "idx_base_SaleTaxLocation_TaxCode" ON "base_SaleTaxLocation" USING btree ("TaxCode");
 6   DROP INDEX public."idx_base_SaleTaxLocation_TaxCode";
       public         postgres    false    1786            }
           1259    245807    idx_base_UOM_Id    INDEX     A   CREATE INDEX "idx_base_UOM_Id" ON "base_UOM" USING btree ("Id");
 %   DROP INDEX public."idx_base_UOM_Id";
       public         postgres    false    1792            �
           1259    283717    idx_base_UserRight_Code    INDEX     Q   CREATE INDEX "idx_base_UserRight_Code" ON "base_UserRight" USING btree ("Code");
 -   DROP INDEX public."idx_base_UserRight_Code";
       public         postgres    false    1829            �
           1259    255787    idx_tims_WorkWeek_ScheduleId    INDEX     _   CREATE INDEX "idx_tims_WorkWeek_ScheduleId" ON "tims_WorkWeek" USING btree ("WorkScheduleId");
 2   DROP INDEX public."idx_tims_WorkWeek_ScheduleId";
       public         postgres    false    1816            N           2620    235953    pga_exception_trigger    TRIGGER     �   CREATE TRIGGER pga_exception_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_exception FOR EACH ROW EXECUTE PROCEDURE pga_exception_trigger();
 =   DROP TRIGGER pga_exception_trigger ON pgagent.pga_exception;
       pgagent       postgres    false    19    1745            �           0    0 .   TRIGGER pga_exception_trigger ON pga_exception    COMMENT     ~   COMMENT ON TRIGGER pga_exception_trigger ON pga_exception IS 'Update the job''s next run time whenever an exception changes';
            pgagent       postgres    false    2894            O           2620    235954    pga_job_trigger    TRIGGER     j   CREATE TRIGGER pga_job_trigger BEFORE UPDATE ON pga_job FOR EACH ROW EXECUTE PROCEDURE pga_job_trigger();
 1   DROP TRIGGER pga_job_trigger ON pgagent.pga_job;
       pgagent       postgres    false    21    1747            �           0    0 "   TRIGGER pga_job_trigger ON pga_job    COMMENT     U   COMMENT ON TRIGGER pga_job_trigger ON pga_job IS 'Update the job''s next run time.';
            pgagent       postgres    false    2895            P           2620    235955    pga_schedule_trigger    TRIGGER     �   CREATE TRIGGER pga_schedule_trigger AFTER INSERT OR DELETE OR UPDATE ON pga_schedule FOR EACH ROW EXECUTE PROCEDURE pga_schedule_trigger();
 ;   DROP TRIGGER pga_schedule_trigger ON pgagent.pga_schedule;
       pgagent       postgres    false    23    1758            �           0    0 ,   TRIGGER pga_schedule_trigger ON pga_schedule    COMMENT     z   COMMENT ON TRIGGER pga_schedule_trigger ON pga_schedule IS 'Update the job''s next run time whenever a schedule changes';
            pgagent       postgres    false    2896                       2606    235956    pga_exception_jexscid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_exception
    ADD CONSTRAINT pga_exception_jexscid_fkey FOREIGN KEY (jexscid) REFERENCES pga_schedule(jscid) ON UPDATE RESTRICT ON DELETE CASCADE;
 S   ALTER TABLE ONLY pgagent.pga_exception DROP CONSTRAINT pga_exception_jexscid_fkey;
       pgagent       postgres    false    1758    1745    2626                       2606    235961    pga_job_jobagentid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobagentid_fkey FOREIGN KEY (jobagentid) REFERENCES pga_jobagent(jagpid) ON UPDATE RESTRICT ON DELETE SET NULL;
 J   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobagentid_fkey;
       pgagent       postgres    false    2611    1747    1749                       2606    235966    pga_job_jobjclid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_job
    ADD CONSTRAINT pga_job_jobjclid_fkey FOREIGN KEY (jobjclid) REFERENCES pga_jobclass(jclid) ON UPDATE RESTRICT ON DELETE RESTRICT;
 H   ALTER TABLE ONLY pgagent.pga_job DROP CONSTRAINT pga_job_jobjclid_fkey;
       pgagent       postgres    false    1750    1747    2614                       2606    235971    pga_joblog_jlgjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_joblog
    ADD CONSTRAINT pga_joblog_jlgjobid_fkey FOREIGN KEY (jlgjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 N   ALTER TABLE ONLY pgagent.pga_joblog DROP CONSTRAINT pga_joblog_jlgjobid_fkey;
       pgagent       postgres    false    1752    1747    2609                       2606    235976    pga_jobstep_jstjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobstep
    ADD CONSTRAINT pga_jobstep_jstjobid_fkey FOREIGN KEY (jstjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 P   ALTER TABLE ONLY pgagent.pga_jobstep DROP CONSTRAINT pga_jobstep_jstjobid_fkey;
       pgagent       postgres    false    2609    1747    1754                       2606    235981    pga_jobsteplog_jsljlgid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljlgid_fkey FOREIGN KEY (jsljlgid) REFERENCES pga_joblog(jlgid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljlgid_fkey;
       pgagent       postgres    false    1752    1756    2617                       2606    235986    pga_jobsteplog_jsljstid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_jobsteplog
    ADD CONSTRAINT pga_jobsteplog_jsljstid_fkey FOREIGN KEY (jsljstid) REFERENCES pga_jobstep(jstid) ON UPDATE RESTRICT ON DELETE CASCADE;
 V   ALTER TABLE ONLY pgagent.pga_jobsteplog DROP CONSTRAINT pga_jobsteplog_jsljstid_fkey;
       pgagent       postgres    false    1754    2620    1756                       2606    235991    pga_schedule_jscjobid_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY pga_schedule
    ADD CONSTRAINT pga_schedule_jscjobid_fkey FOREIGN KEY (jscjobid) REFERENCES pga_job(jobid) ON UPDATE RESTRICT ON DELETE CASCADE;
 R   ALTER TABLE ONLY pgagent.pga_schedule DROP CONSTRAINT pga_schedule_jscjobid_fkey;
       pgagent       postgres    false    1747    2609    1758            0           2606    255621 -   FK_baseProductStore_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductStore"
    ADD CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_ProductStore" DROP CONSTRAINT "FK_baseProductStore_ProductId_base_Product_Id";
       public       postgres    false    2701    1802    1807            (           2606    246204 8   FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Attachment"
    ADD CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id" FOREIGN KEY ("VirtualFolderId") REFERENCES "base_VirtualFolder"("Id");
 v   ALTER TABLE ONLY public."base_Attachment" DROP CONSTRAINT "FK_base_Attachment_VirtualFolderId_base_VirtualFolder_Id";
       public       postgres    false    1778    1788    2679            L           2606    283533 #   FK_base_CostAdjustment_base_Product    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CostAdjustment"
    ADD CONSTRAINT "FK_base_CostAdjustment_base_Product" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 e   ALTER TABLE ONLY public."base_CostAdjustment" DROP CONSTRAINT "FK_base_CostAdjustment_base_Product";
       public       postgres    false    1882    1802    2701            I           2606    271772 7   FK_base_CounStockDetail_CountStockId_base_CountStock_id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_CountStockDetail"
    ADD CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id" FOREIGN KEY ("CountStockId") REFERENCES "base_CountStock"("Id");
 {   ALTER TABLE ONLY public."base_CountStockDetail" DROP CONSTRAINT "FK_base_CounStockDetail_CountStockId_base_CountStock_id";
       public       postgres    false    1874    2821    1872            .           2606    245349 .   FK_base_Department_ParentId_base_Department_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Department"
    ADD CONSTRAINT "FK_base_Department_ParentId_base_Department_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Department"("Id");
 l   ALTER TABLE ONLY public."base_Department" DROP CONSTRAINT "FK_base_Department_ParentId_base_Department_Id";
       public       postgres    false    1798    2695    1798                        2606    238255 -   FK_base_EmailAttachment_EmailId_base_Email_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_EmailAttachment"
    ADD CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id" FOREIGN KEY ("EmailId") REFERENCES "base_Email"("Id");
 p   ALTER TABLE ONLY public."base_EmailAttachment" DROP CONSTRAINT "FK_base_EmailAttachment_EmailId_base_Email_Id";
       public       postgres    false    1760    2630    1761            /           2606    256202 %   FK_base_GuestAdditional_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAdditional"
    ADD CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestAdditional" DROP CONSTRAINT "FK_base_GuestAdditional_base_Guest_Id";
       public       postgres    false    1766    2641    1800            $           2606    256207 "   FK_base_GuestAddress_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestAddress"
    ADD CONSTRAINT "FK_base_GuestAddress_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."base_GuestAddress" DROP CONSTRAINT "FK_base_GuestAddress_base_Guest_Id";
       public       postgres    false    1768    2641    1766            !           2606    256212 .   FK_base_GuestFingerPrint_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestFingerPrint"
    ADD CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestFingerPrint" DROP CONSTRAINT "FK_base_GuestFingerPrint_GuestId_base_Guest_Id";
       public       postgres    false    2641    1766    1763            %           2606    256217 0   FK_base_GuestHiringHistory_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestHiringHistory"
    ADD CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."base_GuestHiringHistory" DROP CONSTRAINT "FK_base_GuestHiringHistory_GuestId_base_Guest_Id";
       public       postgres    false    1770    2641    1766            &           2606    256222 *   FK_base_GuestPayRoll_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPayRoll"
    ADD CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestPayRoll" DROP CONSTRAINT "FK_base_GuestPayRoll_GuestId_base_Guest_Id";
       public       postgres    false    1772    2641    1766            9           2606    257333 .   FK_base_GuestPaymentCard_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestPaymentCard"
    ADD CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."base_GuestPaymentCard" DROP CONSTRAINT "FK_base_GuestPaymentCard_GuestId_base_Guest_Id";
       public       postgres    false    2641    1832    1766            '           2606    256197 *   FK_base_GuestProfile_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestProfile"
    ADD CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."base_GuestProfile" DROP CONSTRAINT "FK_base_GuestProfile_GuestId_base_Guest_Id";
       public       postgres    false    1776    2641    1766            ?           2606    268363 )   FK_base_GuestReward_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_GuestId_base_Guest_Id";
       public       postgres    false    2641    1854    1766            @           2606    282522 2   FK_base_GuestReward_RewardId_base_RewardManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestReward"
    ADD CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id" FOREIGN KEY ("RewardId") REFERENCES "base_RewardManager"("Id") ON DELETE CASCADE;
 q   ALTER TABLE ONLY public."base_GuestReward" DROP CONSTRAINT "FK_base_GuestReward_RewardId_base_RewardManager_Id";
       public       postgres    false    1848    2780    1854            8           2606    256031 +   FK_base_GuestSchedule_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_GuestId_base_Guest_Id";
       public       postgres    false    1823    1766    2641            7           2606    256023 9   FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_GuestSchedule"
    ADD CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 z   ALTER TABLE ONLY public."base_GuestSchedule" DROP CONSTRAINT "FK_base_GuestSchedule_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1823    1814    2724            #           2606    245511 $   FK_base_Guest_ParentId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_Guest"
    ADD CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id" FOREIGN KEY ("ParentId") REFERENCES "base_Guest"("Id");
 ]   ALTER TABLE ONLY public."base_Guest" DROP CONSTRAINT "FK_base_Guest_ParentId_base_Guest_Id";
       public       postgres    false    2641    1766    1766            )           2606    245230 (   FK_base_MemberShip_GuestId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_MemberShip"
    ADD CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id" FOREIGN KEY ("GuestId") REFERENCES "base_Guest"("Id");
 f   ALTER TABLE ONLY public."base_MemberShip" DROP CONSTRAINT "FK_base_MemberShip_GuestId_base_Guest_Id";
       public       postgres    false    2641    1780    1766            A           2606    268533 =   FK_base_PricingChange_PricingManagerId_base_PricingManager_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id" FOREIGN KEY ("PricingManagerId") REFERENCES "base_PricingManager"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_PricingManagerId_base_PricingManager_Id";
       public       postgres    false    2785    1852    1856            B           2606    268526 /   FK_base_PricingChange_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PricingChange"
    ADD CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_PricingChange" DROP CONSTRAINT "FK_base_PricingChange_ProductId_base_Product_Id";
       public       postgres    false    2701    1802    1856            H           2606    270285 6   FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id" FOREIGN KEY ("ProductStoreId") REFERENCES "base_ProductStore"("Id");
 t   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_ProductStoreId_base_ProductStore_Id";
       public       postgres    false    1870    1807    2714            G           2606    270277 $   FK_base_ProductUOM_UOMId_base_UOM_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ProductUOM"
    ADD CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id" FOREIGN KEY ("UOMId") REFERENCES "base_UOM"("Id");
 b   ALTER TABLE ONLY public."base_ProductUOM" DROP CONSTRAINT "FK_base_ProductUOM_UOMId_base_UOM_Id";
       public       postgres    false    2683    1870    1792            -           2606    282481 5   FK_base_PromotionAffect_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionAffect"
    ADD CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id") ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_PromotionAffect" DROP CONSTRAINT "FK_base_PromotionAffect_PromotionId_base_Promotion_Id";
       public       postgres    false    1796    1794    2692            *           2606    282486 7   FK_base_PromotionSchedule_PromotionId_base_Promotion_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PromotionSchedule"
    ADD CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id" FOREIGN KEY ("PromotionId") REFERENCES "base_Promotion"("Id") ON DELETE CASCADE;
 |   ALTER TABLE ONLY public."base_PromotionSchedule" DROP CONSTRAINT "FK_base_PromotionSchedule_PromotionId_base_Promotion_Id";
       public       postgres    false    2692    1782    1796            =           2606    266570 ?   FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderDetail"
    ADD CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_" FOREIGN KEY ("PurchaseOrderId") REFERENCES "base_PurchaseOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_PurchaseOrderDetail" DROP CONSTRAINT "FK_base_PurchaseOrderDetail_PurchaseOrderId_base_PurchaseOrder_";
       public       postgres    false    1844    2774    1842            >           2606    267545 ?   FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas    FK CONSTRAINT     �   ALTER TABLE ONLY "base_PurchaseOrderReceive"
    ADD CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas" FOREIGN KEY ("PurchaseOrderDetailId") REFERENCES "base_PurchaseOrderDetail"("Id");
 �   ALTER TABLE ONLY public."base_PurchaseOrderReceive" DROP CONSTRAINT "FK_base_PurchaseOrderReceive_PurchaseOrderDetailId_base_Purchas";
       public       postgres    false    1850    2772    1842            K           2606    283500 '   FK_base_QuantityAdjustment_base_Product    FK CONSTRAINT     �   ALTER TABLE ONLY "base_QuantityAdjustment"
    ADD CONSTRAINT "FK_base_QuantityAdjustment_base_Product" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 m   ALTER TABLE ONLY public."base_QuantityAdjustment" DROP CONSTRAINT "FK_base_QuantityAdjustment_base_Product";
       public       postgres    false    1880    2701    1802            F           2606    270170 ?   FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourcePaymentDetail"
    ADD CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa" FOREIGN KEY ("ResourcePaymentId") REFERENCES "base_ResourcePayment"("Id");
 �   ALTER TABLE ONLY public."base_ResourcePaymentDetail" DROP CONSTRAINT "FK_base_ResourcePaymentDetail_ResourcePaymentId_base_ResourcePa";
       public       postgres    false    2809    1866    1864            J           2606    272109 ?   FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu    FK CONSTRAINT     �   ALTER TABLE ONLY "base_ResourceReturnDetail"
    ADD CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu" FOREIGN KEY ("ResourceReturnId") REFERENCES "base_ResourceReturn"("Id");
 �   ALTER TABLE ONLY public."base_ResourceReturnDetail" DROP CONSTRAINT "FK_base_ResourceReturnDetail_ResourceReturnId_base_ResourceRetu";
       public       postgres    false    1868    2813    1876            :           2606    266129 5   FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderDetail"
    ADD CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."base_SaleOrderDetail" DROP CONSTRAINT "FK_base_SaleOrderDetail_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    2760    1834    1836            <           2606    266363 ?   FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShipDetail"
    ADD CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_" FOREIGN KEY ("SaleOrderShipId") REFERENCES "base_SaleOrderShip"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_SaleOrderShipDetail" DROP CONSTRAINT "FK_base_SaleOrderShipDetail_SaleOrderShipId_base_SaleOrderShip_";
       public       postgres    false    1840    1838    2764            ;           2606    266222 3   FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleOrderShip"
    ADD CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id" FOREIGN KEY ("SaleOrderId") REFERENCES "base_SaleOrder"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."base_SaleOrderShip" DROP CONSTRAINT "FK_base_SaleOrderShip_SaleOrderId_base_SaleOrder_Id";
       public       postgres    false    1838    2760    1836            E           2606    270034 ?   FK_base_TransferStockDetail_TransferStockId_base_TransferStock_    FK CONSTRAINT     �   ALTER TABLE ONLY "base_TransferStockDetail"
    ADD CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_" FOREIGN KEY ("TransferStockId") REFERENCES "base_TransferStock"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."base_TransferStockDetail" DROP CONSTRAINT "FK_base_TransferStockDetail_TransferStockId_base_TransferStock_";
       public       postgres    false    1860    1862    2800            "           2606    266390 /   FK_base_UserLogDetail_UserLogId_base_UserLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_UserLogDetail"
    ADD CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id" FOREIGN KEY ("UserLogId") REFERENCES "base_UserLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."base_UserLogDetail" DROP CONSTRAINT "FK_base_UserLogDetail_UserLogId_base_UserLog_Id";
       public       postgres    false    1764    1790    2681            D           2606    270029 /   FK_base_VendorProduct_ProductId_base_Product_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id" FOREIGN KEY ("ProductId") REFERENCES "base_Product"("Id");
 p   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_ProductId_base_Product_Id";
       public       postgres    false    1857    2701    1802            C           2606    269667 ,   FK_base_VendorProduct_VendorId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VendorProduct"
    ADD CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id" FOREIGN KEY ("VendorId") REFERENCES "base_Guest"("Id");
 m   ALTER TABLE ONLY public."base_VendorProduct" DROP CONSTRAINT "FK_base_VendorProduct_VendorId_base_Guest_Id";
       public       postgres    false    2641    1766    1857            ,           2606    245123 9   FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId    FK CONSTRAINT     �   ALTER TABLE ONLY "base_VirtualFolder"
    ADD CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId" FOREIGN KEY ("ParentFolderId") REFERENCES "base_VirtualFolder"("Id");
 z   ALTER TABLE ONLY public."base_VirtualFolder" DROP CONSTRAINT "FK_base_VirtualFolder_ParentFolderId_base_VirtualFolderId";
       public       postgres    false    1788    1788    2679            M           2606    283483 "   FK_rpt_Report_GroupId_rpt_Group_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "rpt_Report"
    ADD CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id" FOREIGN KEY ("GroupId") REFERENCES "rpt_Group"("Id");
 [   ALTER TABLE ONLY public."rpt_Report" DROP CONSTRAINT "FK_rpt_Report_GroupId_rpt_Group_Id";
       public       postgres    false    2830    1884    1877            4           2606    256119 (   FK_tims_TimeLog_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 c   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_EmployeeId_base_Guest_Id";
       public       postgres    false    1820    2641    1766            3           2606    255858 3   FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLog"
    ADD CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."tims_TimeLog" DROP CONSTRAINT "FK_tims_TimeLog_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    1820    1814    2724            5           2606    255871 3   FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id" FOREIGN KEY ("TimeLogId") REFERENCES "tims_TimeLog"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_TimelogId_tims_TimeLog_Id";
       public       postgres    false    1822    1820    2732            6           2606    255876 >   FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_TimeLogPermission"
    ADD CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission" FOREIGN KEY ("WorkPermissionId") REFERENCES "tims_WorkPermission"("Id") ON UPDATE CASCADE ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."tims_TimeLogPermission" DROP CONSTRAINT "FK_tims_TimelogPermission_WorkPermissionId_tims_WorkPermission";
       public       postgres    false    1822    1818    2730            2           2606    256143 /   FK_tims_WorkPermission_EmployeeId_base_Guest_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkPermission"
    ADD CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id" FOREIGN KEY ("EmployeeId") REFERENCES "base_Guest"("Id");
 q   ALTER TABLE ONLY public."tims_WorkPermission" DROP CONSTRAINT "FK_tims_WorkPermission_EmployeeId_base_Guest_Id";
       public       postgres    false    1766    1818    2641            1           2606    255788 4   FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id    FK CONSTRAINT     �   ALTER TABLE ONLY "tims_WorkWeek"
    ADD CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id" FOREIGN KEY ("WorkScheduleId") REFERENCES "tims_WorkSchedule"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."tims_WorkWeek" DROP CONSTRAINT "FK_tims_WorkWeek_WorkScheduleId_tims_WorkSchedule_Id";
       public       postgres    false    2724    1814    1816            +           2606    245269 ?   base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati    FK CONSTRAINT     �   ALTER TABLE ONLY "base_SaleTaxLocationOption"
    ADD CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati" FOREIGN KEY ("SaleTaxLocationId") REFERENCES "base_SaleTaxLocation"("Id");
 �   ALTER TABLE ONLY public."base_SaleTaxLocationOption" DROP CONSTRAINT "base_SaleTaxLocationOption_SaleTaxLocationId_base_SaleTaxLocati";
       public       postgres    false    1784    1786    2675            Q      x������ � �      R      x������ � �      S      x������ � �      T   X   x�3��/-��KU�M��+I�K�KN�2�tI,IT��-�/*�2��\+�</�477�(�8�$3?�˔�7�895''1/5���+F��� �B�      U      x������ � �      V      x������ � �      W      x������ � �      X      x������ � �      c   �   x�}�;�0Dk��@���sN)�'����
�L�vf� �8d0DNZ���d�*��I�JKYaz���mj���{:��'�3�,a`cV7��C�����_0i �c�.A)�:G� J*B�V�y��K�����!c��n(ه�W�~��/��>�      |      x������ � �      q      x���ˮe�Y���O��b��T�WWK	!�(`l!����ݎ�$�"&�����d��0O���q"!p�>�}�^���/�Z�����z�����|�ч>����/����/���O����?	O��ً���K�}��|�ޓ��{O���/�/���ۇ�����n�֖R��-�TSH����ׯ�?��5?�?~��_����뗷���?���|���e�����o��9Y�[�_�?��p?��p�]�����V��W���������u�+�</+�����v�?��3��3����}vw����|��7ww��>���?�������w^�����Kp�n>�����O����1�9˭��/^ݼ㒦�q��<g�s�sλ�c�-���������\���\v�G���������?>���s�����u��o��`�_�p�Jha�Żۼ%s�[
9�m\���k�����;�~r���گ��������=^��������/~�}=g*,�U�f#6Ù�!�f�
��+G-?#t�۸��DL�EW8��>|6�ȁ]�6�W�#�u.�6C�k䚉Ex�L�,�BBv�B����c%<��m1���1�b3��8l��V�����܁��Y�}�����괧x}�J�����\�g��P�Y��L0j�s�|��u��lǹ�C�A��z?v��fف�Ϲ���ŲE����$���	L�ͻӣ��k1���fs�C��|�H��fmZ���J�F{U�ʩ9��|*������Z:���J6O.��EZR?��\,)��sZ�I��p�c"��+����1�S��8;W��F�(�L�m�<w(���QLu����Vf;��(>��\���J.n;�����q";i�-��ʧ����?μ{����.Εl�F"�.@���p�ke�5�bY-����gW�+��h��l$���k�KF�dO|8ֵ��9N������Qv�$�R?z*VV�qP�W9�6����B�5�����s�hGf�>Da���N���_�������TG��|����o��N�Z���m��A0��z۶}�dv�����ש���lJ5����~/�K���Lp"g���.eXp됂~�uc@��v6�o���J1!�+��p���]Ѐ���P�pu��i�3f�3*2G��2���`���Sg���3�Y5��ٗ_�<�7�N����(�}�ޜ�|�k�^m,���i3��6��#�[�ؓj���U$��d�Q��|j���A)�����Zg/j�im��b��g�>�+��T��'j�-�>X&P�T��},�`[v�\yι���(�U�2B_�;��b�QL��c��G�,���Xm#4�Qދ"Э���Ȥ��2rᓵ�@��N�I��[,�'8!�rjS����3M��W��w7��jQ�&}\����#�v*�6HA홿=4��Pj]soֱ���`w��U�x��l**Ag}-�����z
� &?��A8��n��4� ��D���X�򯈆�5g��jA;5̰F1�����'���_����I���
�	`֠1i�%�e'ƲhH��]�r�!�|K}6�,G$�˕�Cs��:B� �
s�����g�&=u|,hG��^�d�ϱ��(�k�U�柼���pj{"ڀA�V؜������ ��(�'%���Qg��V'7UY�����@��)dוޠ�A�4$V�,�%v���'��B-�Md��(a���0�Z����R���H�8�D,�ݜ�ɝ
ݝ:>���S�D1�a�3�F�q�X�� ���(��ɠ�s#�Vg^=j!=��Q^è3���Z�9~E3��a�aS3��*�A���[':�^��@7x�@������	��x��]� kf`��ɥ4Cu#^M�f�' ��ߥRQ��B&%�F�%q��)����� ����S�qI�}��թ�IPvD2R��(ˁ�舻�eA�_I�&"6]xB����E��J	|�~.1|Y��Vl]��r���������n�\�-�ٵJ� !����� ��(9'�LTI�<�u.��wQ���o�kAE⺃]��388� 9���x$�#�	Q��aZY�͔��%9S nu�L`�f�@? !��-�1��:3"��W�h"0�͆B�HTZ6���Q/`}uR�US��J�䙧ʀN�`���[��!�i�a�i�w�a�o�O1f�;:��pI`Ɖ��kׁ�:�z��vdd�N�N��
��
�>�exN�`��q!�|�tܪ:���U`.��R�9�epr�W�s*U���
����J\H�/5ׁ�Ah@Q2�����"���<���r�4a>�yr�jm�e:q��q�6:�0Ж��1
\ƆO�#�i����2{0QV!����H"��S��̴g�Jh=>�Z+�F�=��^J8 y���f@����^� �Q�RBU#�9�,M��
n79z��G��f�T��-�?�hR
�{���ѳ�Oѳk�E�Լi	Q8	��`�h�A���J�y�#���)W9Hɒ����L� =��q���M�@����R��8�_m�'@I���T[յ�!���cfC.=A)@IT�,h.B%,5�{4�\$b4��&�H�8���t� ��^QH�2"���������B�$���H�%sX}�X	0���ji���J� U,nͲ�4M���hA�W��(:=��E��h��E�t(�	ma��1P՝c���W����(^X������S$=-�	G��(�����,�9�B��7I��`b�'�BP+�h�>\�3JAB��1��|�<���4¡]E',��<�P�ɥh!�Z'f<����$>�%˹�C�CL��e8�ē0r%�C�`�S����M��+�Tn�YZ� VЕKǇN|k���pu<05 PR&�ƪ��4
@�(�Q94,����A!�"R�n��wFl �X/� ����z�������u�|b�)2ZI i�l:q�0�i�%�ƾQY��� �@�*/�z��Ϟ�s���> 1�^d�aO<��0�W�|x�����̩��b(��!a�!���k�<~%#;��EM" t��&��O=7j �� 4<����0�1t�� @����j��T�z%��IVVN��$�]@ �R��lh�w`_0���l���+>=iLѹ�Ҵ�6C�lX]�ԃH\p��`�ʐg�P�q3�F�~�k�f�|� �:�2G��.��vo�r��K4����=���І�@S׳2��R�<H���#�%�J�	C#X�.���̅	 [Hy+l��yE�T*m��ώ��(���ԐHҁ ��i�sm4��%I�V�a\��tx@ٹMp��灉&�dY��6�Q�Ƣ)zH��WK� ��<&,�F���4��L�uj����F�A� ��2�EJ)M-��i�ZFiO�F�#r�%OX���a��'�(p��i�iؔ-�_�i ���:�
@a�A�3C�,�IU�L���-q*N>q�pNT*j/,��
�r@�)n�"B�m� ��/�'��w��9����L�hx���j��J�Q�G`� X�ϒ�l:�GA���$�"P��$'�k@S�RH���MKP�F����A��,��P�b�^YB�6����^��i#����4f��x��$��2��~5y��`@Y��4j�������)˖4��6y�=.���N?4����`O$���`妲��bX	'
	C�a��*i�_������߰�z���:A��G��<��h#�0 �  [��J8���������2�
���A����)w�e҆2��PT�����A��b�ࢱ:�1m���%*N½�����:K�K+:�,-�<� $Ǭ�	��hp�f`���@!�R���<� �P@��&(E��?1�C^�r�G��4,$O+��@� Jδ�n���1�{�M
z@BKҁ��,ڕ_��(�B��T�)`�iZ�
�Tq4�:#�Ji7�K�o�+k�L�P�l��H�x�G�<��Q�<��	�'�� �  �������t}�2)MjL�k�6�<��	}�B��bD�u�v��(�Af��$��"�����Jl@T� {4�Bu�
�����t+�%�J�ih�Zw˃�e�;M`�=�a�^��&��& ����?k-��P��q]�PHTE��=�N��| ��[���A<�@���<����>��#|5�+�~D4B�s�Ƅ�"��-�nDz�91��5>�E��ސP$�2%���G��Ύ�*���36����(=1�;(<����=�k��w�8�~��xdT'�S5 ����\�
�ba4i�Nl\
��c��q���C���v �M��$����pPÆ#� �J�]��a*�P��t)$@rU����t8���g��Rؗ-�L��R�K�\���8	�'A�A��4�H��]t��T�
2T @Tج3�A3 (^rxa���Cl����C����ZX��R��
 �1�T���| oP[�'�X��Ppȏ�{C��'�rt+M���qLt"��}��%�<� ଦy,����#�4�a��Q��� �_�:<o���E�t�L���QP�Ĺ�İK�]�X]H�B<&u뫦��s�K�����cu��LzJ�� �͘�-.� Ӡ��Q��0�-ɿ���{B���N�M�Ԁ\����(�M<�����k�&�1D��b.!��.��A"�	�E���Vġ�aK�������D�2����̴����V��0 F3�:��+�k��K�[$Wѭ�뮊��A�aH��"ٞN�Y�3QE- �2i<	��4�H��մM��=�2��Y��q#�iTݑ���1i<]s��1�x��;�&cE#�~yrֻ�	�{]P,r�����>�z�g���(��4�s��yv-Ч�6�b����ԉ�j�H��,��K�i<@�u~�&6��=�J���eBr��.���c�zO�}�k�@��T����N�&�Ͷ��M��(	4 <�H W��]�Ъ��N�����Z���S�>��:[�A�T�|4QWm��/]F�tu|j`�}f57��ǘQ���	�(��QC-?��.�ߠAEo��[IsG�3L��*��{#zW�'�K�O�-p�vؓ�N�����y&k1�F9Q���VIٶ8�-*UZ��)�Xh"Ae5��#����?���K��O#肥Q0�A�m���Vi͢`�FƉ6��+���[�M+�v��~\��,(_n`C���o���	�!�Ck�u_ �]�Qt�H6�n ��A=ɣY�'���vk^�
�ǝ4 �7 @N蟌t_P���M�Q�Vɸ!�&d~�z��4��<F47W���<��pG��8R2�X�7�^�*/�up���"�6@pX�ź��} ��QP��3+E^��	���:mNJ��O�Z�|h���N#�Œ�[�G�X�ă����M~��_r0��}��zB�[Bv�ۨ����_��*�#��0n�R���%_�=� k	��>y���v�z�GO1Tb����7�i��� ���ヺ{�S�Hd��bԆהa�q=.ˎw�u�����]�LhB�.����xM;g&=K@NI'��A��M�F���7�,�]�r�;Ś�5�}�n]��(��:���[�L�V�)UZ?z �ւD�ug)���0��M�V�V���h�ai��56�pM�VD_4�n3'��9K4��>�uoe^Z������&A��)��䌛)M�6=_�S�A�ۥV���&�/�$ĳ�\��PU�2�uJ�a�.�B�R'�|�*E�b�)b��0 x��X�yt�VO�lN
��/��C�!�Nf\Z��o�g5=$�̻[n7��[�!�x���d��������'#�3������3W�������������Y���~����ï��Տ�����ݛw��M}�o߽�{���;���g�      �   �  x��XK��8];��H )R$}�9Ao�f1���c�$�mݮ�g �d����G�PC����M�y]�9p��`�����ҋ#�C��Lr ��ڂ��#!&������fn���G��$]��e�"��*�� NM�M'	���@]�I�@��<͘�n�]`����I�	��
�V`��[N���l�8_���U�_"��Z�f��9롸�&���
��,ѓk�'�?��K'�2\Lm�H���1��f��Y)W"�뀵<���5W"N�[9% �D|�s�RՉ���t�c�嫜���N�*�*���EjB���*�*��J�������㫜+�-`c3�D�W��;0hKe,"oz�o�x�S�8���(�>}��q�9L8{`sp������J���~d� �Wޝ[*��g�6qL����R�� Bߤ�6�>}���OaHb��ЋM&�g����I`�[
yq�eS��X��+���)�O?�?�~��)�W^�<Tٜ�� �b�nL�*J�˓!�Z���#|Rxz`^JJd�>L���,�!��[a�����g�W�����-�/UVl1f��5�´�D���K�?.����(���So��Hm���g)�/���{�C�܏8e�|��5-���m`x%.Q�y��w��5�kl�K��r5���;����w�6�e~�a@�\�*��c��e*�R���y1�5�=��߼�t���)-�euB.]��r#���d�u�Z3�Ǵ��*�.�ID�"��b�aA[�^�����y���(���n�wG����d9A����55�/�g�FI�dƣ"���c#Ŗ�^��U�`h�@�jIұH���r���Y�\�Z��]+[�P�i�ޘ�>vP�t�ax,��Ѕ�w�)S�*��e��/��e�55��p�?�7D��!'�R�C��Ӕ�0�/y�Fr�ĕ���x���4��      �   U   x�}ɱ�0�:����vf��L"Q��% ]w�	&UG1O*0�ʊEd��~���3���3Ogm-8+�77�>�F�.u+�,�W      �   C   x�ʱ�0 �v�#�@��&��?���?��
�bQ��[�Qi��Iۢ)������+C0�"~�j      m   �   x��ϱ
�0���y��\�IM7Ap:up�":4������T��vT|���I+����.|�G���`���<Dp!>4���l�R �B�2a(2E\�U�Q�篻�/k6���Z��m��b�@�I �P1S)�z�(9�ϳu��f��o�7L3��b�$���Z��u����r8`��H"n$��.yEo)�Q�      Z      x������ � �      Y      x������ � �      ]   6  x��VMs�6=ÿ�����7xJ�v<�x<���)� $5ٱ�x�� e[��J������{o!�e�w��m
I%�rl�l6g��X�M���!�]�@��p��J�J�Ңw�i�P��F�L���ݴ�A	��i��z/��H.Dk��M�u��&�D� �t�0q>`8��`|���l��'V�����)!��%w��!�K��Ć�^���a���Ch�9FK�+%+eJ��e\�\�����0j�
�1�F����F�|S�`t�^i�1*�=	��QA�
,��M!8�c��n��V��a�w��f($�=F-i�j����X��Rxa�A���Z��� ���n�p3��g�dKG
�%Wsep�:˓����R��	�6�t��׾��ٲe�:ihz#�F�):���)ͩ���U^t8�B+�4�R.�
�bc ��*p�C�C��iӂ���r��`��n͖��˶c���-��&"��*W+��re��s?��7�����¼���	�uI]�-��ۃ&����(�@��TxR�=[x;h�U�x� ���\�\��J�OB�C8S�C!���a���������@=�xP<�����?i��m=>�9�m�|%U%��d~V�k}����R�Ӝ_Lǘ�H��I��׵!�؄Z�R���x=E���h�VS�MpxSn� �����+7�"z�ܸ~N�'e�MS+��'��+��W�Ũр �/~o`ߺů�a��׾^taXU��O�6��c߾^3�AZca����9�w��YO��)�� R���wlj������=W�,�5�y�hk��i�����t@�!PdPh����#^��Iq�)��H�� J��"j�ҧ�\7Nqejǃni�$�dK޴�J�ƚ���<���l}�����g��	\R�e<+e� �ﮔ��WzJ֤�I���9<(��6��x�H�QDo���#TM�V�5�#��/�;nn(C���"�2��n�����k2Q��F�)N꿗���W��������-�5f����@�?���S�h>�>�8��61��
2�L��@�
(�Ke���������#      n   I   x�344���#
Y�X0���	��eh`�i�g`�Y�8q�jda����s���C�	�M�t��qqq 8�4      ^   �  x���Kn�0���)�6�/IԮu6�t����$4&�B���w�EvY�=r���� ��8@P�F����p�t*T�	_���C\�b{�q�"�@,xDÞǛD錰9�O��k(��a�� ���U�C�C�؎��\g����A�}��c�ڢ&���<ّJ=�t�$Ȭ��@3�i�Zy@�e�r�,�W�ȵ��!�P��d=���ۧ����9^6PV+�y���:��9|��8y�}���w�P�N�{B}��?otz<R���T6��7���U��Yu�.�ڷ��B�:NB�ĳ m�]�q�\����P0M����E!�h��op�_�D�(mh�P�� JՙX�����ؤL��L�E�1Y�@Ι��D��LG��>t���t���=�l�{������N��O+����G*m�-ߝ��V��Sf�^��,Ɵv��	.h��&�G��/��0I�_s�÷      [      x������ � �      _      x������ � �      `      x������ � �            x������ � �      b   x   x���A� E��)���"X<�'`i��$����Ӵ���J�V�"�It����JG�]��t�����^��O��a?͌(/�Cჴ<|{�����1�XdJ5�w2s':��p�����y�      �   �   x����	�@D��*Ҁ����r���T��Il�[�a�贈7��Պ�
`�i,���,�|�k�������-��<#�D�qՍ��F�k"{�#r�{|[��tn�"�W)�I�]Jw)}H�!�; �SJW��P��P��P��P��P��P��6��FR�HJIi#)l�:��^�C̻      {      x������ � �      d      x������ � �      �      x������ � �      �      x������ � �      o   �  x���Ks�6��ԯ`�:`� �ލ�*of\Y��f�@ �k)��k���~�n�c;J7�I��n���x$��|��1L0�/�~z3Mo�4ݞ����d�?����o���ۿ���k98��)C_����8�bG�]�XHc�e:>�v��ۈ�����2�ٵ�QZ���\yw7�r>8�3Ȏy���m�� �IX�1b �q�'T��=lx����>>�78(�3e���k��By�3��1nU|��p��ۡKy�:��|��ĸռB1�mEv,6Lr�R��C�D�q�a���]c���/Ƙ�EɉD0h�:�������m�{ӛ���0�ߵ�-]����e/�l�J�Հ�C��hsi��E��2\e=�U��1��j��\*������l�5ٶ��F�=�������~�D����pA��ʧ2n�}�۷�߿����w�����os�ۼ?x\����~�0�����>��}�}��W����)ͯ_}�*r*�Q�t�v
[����
\��4��8ik��#�qCu�:��Z�ڌ��Ne*�{�YN�ɱ��|��M��K�P�v�tq1b��k��MnNeb�t��I�/n�Q's�k�<�E�Q��ER��T7v���_<H����24p���D]�!������3�	�UWd�b�@�0�Օ�ɭD)s���>�a���y��N�:�r������,��v����Ckw�c�'�ve�4��#�D�o�'����a/���cc�'���	9��V����&ݒVgs8�R5̫�V��5ֵ��k�������j��l�klR��y��q1g`�����Q�|B1�M;�/eG!�V
Q^ܓZ2���%�Ϙ㔖�k��g���K����dc���6VRs��DI���b���41��*�}����"����	���s��F�ʝ�;��ĉ�.%3�(Y\�f��ej-X��̜4r>qV������v��c$�‬֍R����̑���3��[X԰\q~��.pl��XT���ւP�����}��>���p~�������_��C��'�ps^�h��r�MZ�Թ�Q�{Ƈ��jt����Gˮ��%�u(V���~Aq��v�w� D[�N�PXˁ��-�zy�=�j��_���pV��MU��)����G��#9�<��qx�I�q2��V��V&b�V��u�zŞ$a2���%�����j��� ��܊�[�]���w����)� �K6f�4Bф���"z:wc!��[l�Ƃ/\N����Z+ͭAL���.AV�z�,4b����4��u����Q[4�Ͼ���U׾�
�<,v�I��!k��\F-������� �MV��c�z��k&�̆�И)־7ǂt0�1�џ4���co��УEΑc�J�j�h�o�>{|���V�	�c����mU��Q�u�.�v���Em�lER�
:fC�6�^�vn~Znnn� ���|      r   �   x�U�Q�� D��.;%��w���cu*_h!lL��"�TeR�X�H�˒�Č\��F<��WB���jb���$��fy	ر�+a�L��ȑ܂F��^׍���#�qb�P���h�}�t�SWQ�I�ZҦ���]�����-*�&;氉���{^�?.e\�N�����ܶ�F����|�<��s���EZ�Zl�R8��{?���WԺ�g����}�c���wH.,0���9�RJ_�@b�      �   �   x��RA�!<�c�( �e����F3s��v=P%&�U��PP�5�
Jh�x�ڸ��ҡ>0KqB!gyB�?0$�̉ڎx�i8,�iq^v4.�� krݽj�W�,��+�<m�}%w*�����B�y��������l���v�¨��ْ�u&7��p!�.�|j��O��+����l��7�4��c�s]����c�D�#^u�8Uh���i`O��������7!��I��R~J��      l   �   x���K�1D��S�d\���}�0�&��,�����c��T �*�g_���]�۹G�rL)�����8A(����rUk!D<����[�Z�9��g^RT�V���u&��>����>���n�]������H�h�)�e��v葫H�Z�sy�u7�E'%�2�( �4�2'������c�u?�NL>      k      x������ � �      e      x�33�43��".3#N ��c���� Q�9      �   �   x�uO��0|�U�4,B ��T�����#�dl�`9��8
8��d	������_��i���N�{��� ��]n�r��W�w;��*Ѳ��JF��U��1h�-�T���]+-��,5���-���__�Q�ִ6K$Ҝt��/Fe�z��)�S���A�      �     x���?OD! ��)�}i)Xuqrr���w��������'5�@h��*����=%���Y� +5D���UG��$z���ݟ􎙝sG=9��ED\��g��ľ;<m��!ʢ�`�v�\:r ߐ�w����P�a}|{�����g�E�7R��b	F4?����Xf�ƭ��J�"Z3Oh���^A㵳�aSo�B�`�+X4�JU��MG�����Ǐח)�?4uO|�a��b���L��.Z�Q�h)�A�ú,�'��!      �      x������ � �      �   �  x���]��6���U�H %R$������C����T�q&cM16R���ux(��K)CWdXD����@e�[�h�Ȇ0OD�:��+� %$��҃�XbB̉�a��Ͽ��o��G�޸�'��(�=QdN�R%T�FȦ�pX�<�4�1��.+�uX���s�b����QLvN�9�6�������;'���)�x}&�c�xQ$�'E{�
^R��u��F-T�i|p�V�<���~����５t���;�z�h�$�+*WT�(���a�8�c�gI�!�.�6^EɊ�Ґ�e���%~yK1Ii�*P���G��`�rІ�1�	��0'/�X:4��BTT}��7�gg]Ȋ"�v��@�k��q��J�(�9G*��@��R��2��Tz�>y�c:���B�U9\���+
����j��]ހ�_�V9�L�na�:�y��(,�Z �K0X�0L����S�su7���I�GM�O׉��g��P�aw�S����G���u�iX�1�)�ʔ�ȺX��u]ن.#?z��`{�J��:�5
���0��".�y��I�AJ���f�8�E��P6in�����=&�wf�hC��b}��{%e�1Ŝw�"�Ň0O4�e�ѝ1Ma�M�u�X���������<�r���Kb;ZQ�p����~��ͨ�n���P��ஏ����%܂C�e&�]Q��h6��qƥ��ﶝ�-���p�'�����GNJ��N�W��W��.�z��O�������B;ENQ��k��#'ILj��P�oSs�<���߁rN.V���-�����;�x�	��}ǀ��%�Cڍ�5RG%fi�ȩǵ��rCO:ũ�[h'��}҈��u�|
���F���g
�����U??HE��d΍u�q�	�7EB�Z�-�S[m��\'���!MCXF^J*{��c���(��H�-Ir
�_�j�F(�
7$�9\"�f:U������_̀�[      }     x���MO1��ݿbF����9pP�(D�&�å���q��_o=y'��Û7y�Q$�ne��Xz@�H+:j���JFa��S<P� ����.�RJ��c�o�V����|�����{w�B�f�Q|����j�[�܎o8�gQ�i���3�x[��-��RN-IE�:N���Z
U���@�@��M�����a�w���VH�>���5�����5猾��J!MJUb4���%�����,��i�o��^��K�3�����|�^aם��f��]_7M� n+      p   �   x��̱� ����]jq $1@�L��6h�x��/MR�����i�p��2Rl�i!��:i2�bn��C;��7P��(�Np�M&��^�f���B�k��%�M��@�q�
�٠�h`��c��&�Ǹ@�	�����ܚb�/d:s      �   W	  x��XKrc9\�:�\�
P����ֆ߈Y��9I��زݚٔ�
�)�Ld&����L&�D��Ρq��l5�QZY��3�X4q:�b<�x{��ÑR�9p�G,g�3�;�u��_�9~�q��RW��8H����tSv�||z�?�<��������W�xjluRP��� �XatZ5�%������0__��^ΑN� \H�ޘK28����r��,Y�/��J�.�+h�D�B���a>G��8�S:�{�\_�N��Y��>�^5y3.aq� y�P2K���&�;�;z;�hDpj�-HI�,7�̽h7��įI�I���^��Ԣ+ł8{(<<pK��j1�ȼ���r�������9ް+��2vM ����r�Zz�� ����/��6^B�E����Q�ZhI6�i��t�,��nW�đ/z�`=}���L�d��$�����Q@(a��RCjsl���5�%�o�μ�'�F5�սG�ݗ�ϪR�L���OSz;��wL��4B�v`��]�;�Z��������08cM���ϔ�h?��[곐��Np�|ɛDno������1,$w�*�J3d���
e�c��e�M�_Ԋ���C/|S+��V<qͦLP���5� G�?���r��A��,	v0k�V�c=�6�k���W���u�6�I�)�6�B4v��p��=*>*~���=��ә����m�	�k_�[FHD]Ν�le�z 9�A�0�[�Jr�� �7��7�&���;�L�0�/`g�s�d�'�	�㾽��M9`q ̤���"~=��Q#x��m�*j�s�<��������_�v�&9�Kz<@-�� z�H�=8�c�TR�	"}� �<;�(�/(h�A͠
K�d����R��>��o�k����~���W�	�3P�����B�q�1��U� _���(%+�����6B׬��i�+xT�w�oo����0{*��Q���9�@�a<#u\7ΑbϦ���U�q��v��v��4=A���d w��F��k���[�]�l=���jsHtV���.�ٓ���U�#�u
x�!��m�2o7U��ȹ*��!��=�;�n>��-;"o�\�<	Rw1��5�S~�1}��'2c�X�$X�W�h��D��[,�{p�n�ߜ��| ۭX�Y��x�j9�o�� �U���	���l/⧣� HY	��	���s��ը\{��W�ayC_����n��9��&�����z�z_�Sq�_�R^�^�_�:3bc����V�8q��pVy׽�r��y{�_Sόr9%*�|���Z-&t�8a�X�P,[a��|����/��|d�:'� ��K��L��ב��9_���v�@��/੪�6�c#�+j8V�r鴕K��ŪV��E�;�9��x�[l���?�&�L���f[�)x�F������|�1��c�[S
#a�j}�L�m��N���0�[���/��+�ՐPu�hSP�jaF��}c�i��w9Z�]M)Xb�E4t�ӐxMc�~o���0�����פǳ�9�ɟ�;ș̘� ai���r������O���i:\�w<"���9�
.x�Zyo���+]�}����wΞ�SGо�)mo5hC��R��5�(�t�:ﶔћ
�}@�J����}�u,�r������?�f�>e;�ױ�Xt�z1�
�F����dcGjP8*Z�8:.�aY���ĵvEx��W�G����{��9��y'd�l�ò8�L�<��������ds�U���]6�K%Fjc�ρzR�HFc��C=��R������O�/�c�ֆ�k�MA5F�q�{��V��'��XuYGO<J�����rT�)�����@T��>Z�ǟ~�v���7eWK�P:fڍc���Z��A�`>��\��D`��jN�1����f�h��^r�z�g.'ݽ�A=���Ø�ʅ�S��cS�amO���z
�a�'63d��̌#�	+^�ܛ�շ^���]���A�����;V�D���.%�5�m��m&�O߷~ǿ�uh���`c��%%`�[@v��܄�7������-%dd�HD\1�^Ã
��5��4�P��w�ZĝI7F�{L}�3���l�`6���~�:)�gb�At�j+:e�V�1��L�_�:��>�m�5a��� ��	�0�@;�AO����2��ׯu��I��Om��-j*���9࣠sp��j�-��p�y�}���,�Hh䫎K5P�b�;��v�,���wo�=2_�˹IB��7�UZ�ȥA�����up��G޴�7]�~T�Yz߀%��I���f�h�ЦE��G��Z���ͷ'(�+��� ���%� ��VD�D�Õc�z��O�~��/#6��      �     x���;��@EѸ����ē^�gν���.�yL�PtiJ:]s�r-Cfi���|�Z��z���W����cΉv�V����vHc������R���в�?��]~�萮������&Xq�Ѷ���I
4h�	Ia��� 2=��`���1b��@����f���
�,�9;U`f=qd62)�,�ig��Vt�W�f�j�l�s�fqφ�"1��3C��3��0�Ia�q�\ɰ
4hF?L��3�0�1�|%��B�%���psM��B���_v���<�4      a   @  x�m�;nc1�z�^h�Oi��4�$Hd��(�؅P���+V�����#Ag�@��j$K������S��j
���t�Q����|��/�X��Da>4�q��j	��P�Gl������ƹqF+�}}޵05S`�5PQ���w��V����E32�B#l����Mv�Z(�;&t���[9���?��f��އ<{�&c4n��dP\	���8J��{F�^� �Fh3��G�w.Qz�mP%�p< 960�]�5�p��3m��9Ω��|�5�������L߰'"�{����,�܆d>�Krq�SZ���Ý[� �WF�r���v]�?�P�5      �   �  x��XKne��^E6��D��E���M�2H�?L��m_�����ݐ�Ū��l�L�'%e�n�B�>C*��o��rT��)�w��ob��'ŔC�@���N_G����Z���q��ӃM�Қ\�y����|8�|p���t9���2�Q��Rg
�>N��x�0zZ5��J편�<\����y-/9Z0i3�-��J��ن~������;b�c�KK5tW���ES�fJ�� N,%��r\Q���5�禦-�@����j1a2�ކi}�zK���b:���L)N�
V��*?� _8�qL��G�@=�Ʃ!�XS\�� ս�AK�����ѝm�	�o�sp^1̲P^��k�!��#ę���,x�� Ї�q�%�4k���������ѥ<�`e��mki��� .��"�|&�c�3'�'"���VB���c�fk�!�8U���8�A�3�)�g1�)�Ф��Za�h����"���n���u�������Ǆ"7�	�6��Mt<�2��΅�tX��@��{��FI!2��`4od�����D�j��Ss�=*(�%:Aԣ���Ej��,GoQ\V-�&F[(�7Y<=������e�NC���P�9%[q�搊X6\����Sn������������t�����N���c�ݖ]Y�B����ȉ��X�b��$�\U����B��9H�Un֬r��s�/u�D>���g (����[6���V�"�N��4���-��i@4 q�r�P���*\���g�w���{]V=�Ξ�̠bd@̘�T�h�s�e��e�i�+�S�*в-k`� �̭ 5'XӀu�n#h\��Ąn�>�0C�k�~�ۙn'�����p=e�S *�F`}��f��Ź�AϤ�÷�x�d3�0���}�?*��sM�T��jD?�9�z).X���d�^ZI
�����>P���8��#/�<"_��A�M�Kͱ#At��w����S����m"�p�P�w!�̞�'�t��*嘭�:�����x�h-�`X\���6a�,�3I�Ɩ�~�F,�`˭�iB�l�'�Y�������
5c
Ж���W�y�+^�O��R�W̺B�lwZ�:c��;z�P�Tl�<��]��{��B:�¡+O�PnD&�@krߍ�8y37�C���kT��:�Ô��L�g����k�E�����;�n��-hD±��vh捻B�^�n$�J���Z�91 `B��0\�w�o�{����C�cбE%a��i"��U��o�%p��z�i�>���UlA�v/Q�)���<y��`kcAx��_���9�
C�{ꌱp�@�y2*�tџC����e{�ތ���5l'�6��3wxR\0�5^�F&�f�2İI����o��,�~��l%iN+�4feG2�5ܻS��Y�9�2~ �����e,U}��,����ٯ`\˯>��b�P�aY����ܐc�ZQ����RD���83�Ʒ���.1lн���1}+��̆���׶��Cw��b��R��X��=B��}\vl��.v?	�,"��y�
�^S+	�W��	5�;��}~�UV��/���C�w�gC��w�3
�~�9�+NtD�O��5�����n�1�i�-,�hYX[z+���!�ԕ�#h��a1�(h::Ѹ�K����5�KP]b�����������$N�7"3��/VUpMCjG�!�� N�a�V�T���v���B�w�jiS[�0��m��)Hݴj���p�^d|a�����`���ωh�v��$�$�2R��>"N����7��z�cSثf��n������%�����z��;�6A�&V�0��X��������5�kq��O{�9��4|��|�L�*|�:D����(�&�$��D������+?\~z
�ƈ;�;�V.5��fC�k=!7�M(��ɋ�]��{�`�C��`zm[�� 6Ho0�܍]�g.�v�<���0��z3u 9��+�!�TC�kt�F�7ڔH���Gn�q�]8!�F���Oߏ|��̡A�s�o�Ɋ�#�@�i*uo7�P.�ъ͒|�7�<��#�=�Շ{h�W],�Z��8��	���$֚6��m��P�1�Z�Y�Ed6G>������0�P(>����o)uj�[���w��=�=cg��!!��v��a�3՘���շH�F�K��j�H�l���B�#�#{�>�8=�:_���߿~��?��$�      �   �  x�͘��[GE��W���V�r�L�C'�������o4�P�h�#({>>��{kr�P���ɓ��)Y�.â�詎M+�`:�d����H<%z�3�1�o��s�\��)�R�k���XS:����PR�1I,�6������ثr8�x���C��T�����9�z��R.�֘9(��G�qT�H�j�l+��l{����$�����~���_m�����B�����3�9��\�Dxҕ�B�hَ[����b������m��$����dP��e�-�nc�H�e�ǢZl�&�J���|��mM �Ф[1|�N�#�%�Wnq2�9��A���9�9�S)IH/hh��\(����F�6� �|�<��{�|����3�S�lY.��
s��+�h'���'GnP�i�q�{�K�K���N���t����3y%v���e(�/���#:��|��>p�u��-��\ݢ�9c�<�h��׵t�|��ߦD�Y�xJ��\+(�=� �]�GI�=[B����t�$8��f4Ld���F່t@�dexL��额;�M�l�:��-���,(��&���y/hX;��W�5�u���=��R��N�:�*��<㜯j����2R��(
kE��8�]k���LO�rz��X$�6���b/��͆�bh������r�VX;�{�~���,�=C�船�	��Y&�O��5��3:���3����j�R��K+���t�Y,0�1\�8h��Ӭm���,�2�����W���3��#Y<����G�
G+�x�k�JJj}Q��&X�s�'��j�O�>���)4�$�5Ԅ^���&�ښ�.I_�_#�#S�x;��M�O�FG5o���p-^�K�hО�J$��gi��V~A"�X5ߋ;��ٽV�"����{Z+N�9fCx��/���l��i ч����o��c�Q�9��H�s���+x�cc2�;f>F֝��<P툞vk��M�*E&Q�>� ��{��i�.<N�-U�Ղ�l�&�c�A��P�Ԝ���W�������ȟ�W�^�(��ka�.���X6F;|6������UP�Q�q,j	�<#�"�`0���>!֯�C#l���偰�B�Vp\ם��^y��Fk/%�`z��nBX�a���ya��B���qEﵗ�=F���_<O�Uhn��n��YpYm,܈�wDҾbSL=b�h�X��c�̃z\�q����|8j�F-��~�T(��､c8>N��ŧhUΉ���_vb�����E����klj��u�z��ė��_�æ��r�Cq=������#�A:��P*X@���_��֢�È��
g�ֲ�[֣��7�.T��IH�a�Kh�.�<>U���n�5(	�RK���ɭp4�F���t7�a�*�@oh:rށ���͛7���}}      �   7   x�3���4500�30�4���i@X�`�4� P!�q��K \1z\\\ �.      �      x������ � �      �   #  x�͘Ko�6��ʯ�R�}�]�M�謊��l(��d1)P����~Ȗ,{
8V�Ⱦ0���\J\h��C�ܠ2��;���nɅ@��)6���������߿������V>�ӷ�_�g�ow+_�ԃ9�ca3�`3E�wƏ8�KP��ZyG k[k��󗿛���ө��d�C��4���W�����ިkZ�r`��@��RL�3�l0�6)�x�Y������>����ݗ�CU�Ep��T��qR_e���MD���N�Ch�ƪ��hNQ�h��&��ҵ�#���tqeV�C����q(<R6�O�p����D}1���8��@�ԑ]�ri���)P��ޮ�n�< ��2Ɂ�^��2�G�%*"�@/�L����ouC� �)�7��%շф�`�\X�:�3�1��&�N$�;p��;�:[��G�*Ofͅ@��[;��r��Q�Ղ~�ǯ�Z��V ���:���{�3�g�'�`B�b x*.������������|e�رm	���=�=/
ou�T9�sU?���v���kK(���Ҿ��yt�8�a��D�zC�R�a����� '�G�r��
��`jcO	�R�# )F��NZ�|�c5���hyΆ�B�=��ժ�IB��a(�����I�d��x�̳��r�N�=����B ����T��E��b�f��Z/��`��Ϊ�Ѡw�p��X�^Cl?jW��Ú����$���U� v0c�x�L�3?�T��޷���Ogĥ���䲉�S|���7��'�9�1��u�0���9��|w���3"��:Z��Y�����<�ʩ�^�"�>���7
ĶxXe�ً�M)qT�5�2NRI)��6}�I�|���EЉ��iqu����p��v�� ;�`Wnhz8%�kJ��o�E�5g���u�K!k���P{��C�%r�1�>���.�cOa!�}G��^�5,?_�Y���2ֿ�V���٨��#nY�cn�L֒��.�"�σn2 rBȱ��HW�L���CbY2$������)�B�x,�������6���oq��56���Y�G5�&0�/�"E���g��I��A�i�󖪩;�����B�-u�M7�q1�j��r�~���9�q��ݬ-U3�q���Q������᪥�B1m��q!�!����z9���c��}��/1��F�f�'̺=tޔT(JQ�Z��Oʏ����.��׬��cQ'�3F���8N8�����ݓ��W��( 7�Ӎ�&����#z���H���a���C�|m_^^�Q^��      �   �  x�͖K�E��9����xed.�X��啅Xx�O��C��s��$܄����G$�fAW���5���T.4q�̵fʐs'�8T*
������rL�hQ)�ax�%��Bx��W��!��1`�|���x�d����>�ӧ��e�XGdJ�+Ȓ�ۄ̃h�X�o{�{��(QXw�d���M)6���
��@FF(1)W�XI��?��ѐm��E`�* :�/��>��bS��a#F"S�t�Ġ~��˫���	qNo��U/2r�8��	4��3�1�ʎ��ʶVJ�X�	 ;�E�e���CKrLFA�(R��g��F �����d'� ��l�}cUd?��Pbn�4������`Ju���Y3C�<!΢�y--�$0J,	9�����������_|���ѳ4�����s6zl/�n=gLj"?�9�,9P�*�-	�NBO@�A�
�,��)(���ҞV:����^l'-a��X���>Er�"A-��d�X����*4�	n
qͅi���U��`x���w�ۇG���U�گ=�1�>zb�/�cZ47Wi	E��J��`����"~_"������)=V�]��9��m��g¹J���v���&�)�-ռ���|�H9����_��pM���a�m�N�}{#��A�Yr��Ю�AO.�n�N~�����C��/���l�d�{v�C(i�>��G_�=y��Et!���y�#c�Z�7ic/�:�R����|��ʂV�hӂq�]<bRb,�f�>��"���(D���s��Nd�#h37O�\}kɭC��9�e]�������[�ɍ^z%��c~ꤛH2� �3�c�����5�I
�C�S�nl�v���E��5J���R��U�v�h)�Ɩ4>�{�[��M���3��� ��e­���I,�=�����{/�[{�-�i�3I��L0���UI!;$f���O�g��yww���d      �   �  x���ͭd9F��E1	в� &�ހ"���_��զZj�v��c�|�ƣ�����E���T���1re�O��Z{~���z��0_L�#H*'��Mֹ�ά������:S�4�������/�\y��һ��$.A���H\'��Ѿs����}���J�R���EWn}�vp�Y�x�Z$�;I+�����{�����G\��E�u(:2�yϮ�?s��ᒾ�<4[���hY�#s�^{����9'Z3Sq��A��ZjZx�G.�׆��B�,џ��j֥�)���f��p��y��s�܅yes�v�2��\��{X�\t䒙h��{v����g�>j���c�^��o�������[��s��w�v���}��1��]>s�_p�*�j
�9��w�n���w`쟹.�J��ৄ!���!��`fp��%�|�L�8F�S�]to-���.?��pkL�ޯ�P�l��Cr���Ṵ>�$� �%�l�X�s����\�+��N��IC$����N��X<zЅ��Z����nz_]��r1��.��φ}e��Ta[�/�hi�ώ�d���!���Ŷ@b�����0����(�g����rYy榸8&*�PI|�^�|��זN���|���b%��c����r����QMž#�xz6��[��|��-�}�/�s�@����Օ�?q��������      �   j  x���͊e7����P�eɒ�=�lB�BV��� $��ǟ��$0Y_��`��U��F+�Q��0_��G��r6Y��v�<=��j���[�\q�>y��G��Պy�tvr_LR���PJ�-l�:X�U�*��o�ӯ�~.�|~|��.2j��f�=���Y��~irLԮ�Y���b%9��lou(qO�,���dd��ʎ���$y(M±��GS�3P:�� [&4�O�@���&)�m��!tڅ���~��rm�0���d�j�̦�|��������>u���F*L:�jzZK-C��>Ū��#�[B�9.yN;�BB�}���cK�:�,5D'�昗Z��k���c��� ���f�E�}	�ݳC�l�B+˖{I9T� 1�7�j
�9��~�y�촺:� ���C���z5�_*�ё�^�ϗ�~���W�?������rF-6�� �"��1t��qG����:��2Վ(cH�5�)��;Fs�Uu�$�
�ZX��p���}0ZL|��\����k(�
��,>�5&έ<�ˈߏ���J��B�[��!vCic�sw]7K���B�.�1Z�"�
kz��O�k�
.Ǒam�)^�	�]��Z�n���`>�g��u!��Q4�[Y�ay��B�n��g,щ)�{}p�6�{���|���h(�n-i+��bú.�tFe���&É���0PQ���AP�!��S�sT?�Q�(�G,�F�����-��e{VY�?�D[�v��l�j�C듨P�xmpzVʋ*GmA_J�k�+����s���Q)����2�8$;��Ǔ�P��e���hV댨f���������Pyxv�v�(�d%)�ޥ�M�8O�������W���      g   L   x�367�4�N�IUI�P��ON,����L�� q������i\����@m!�pe 6�i@u�@u1z\\\ K��      f      x������ � �      s   ?   x�31�4�.�/JU0��".N#��LȒ�*d25�4�
���9M�B�P�=... ��!      �   w  x���Kn�0���)z���"'���@���U�8uN0 @H����[H0���/���7ą�0 �/���Y�/����A0C��@��
ٺ3'���%�?�/�d¯:}�rˬvl#��iÖObv���hлGО�
ZY��K���OJ��n��}Wu���wY%Ȕw�??��XK�a4[m���D�F��䏨'%:9��8�#o��S8��Q�Wp�*Z���Go�t���Op��	F,�;�OI�Fs����#�eF%�R�[	����w�Hs+T/�"^be���`p6}[���@dkR%�O��x�"<����RM��eO`�YO�f��s���<:��y'���7X��a����<��ۺ�� ��      �   �  x�M�;n�1F��Wq70Ѽm�7HP�T�i�T�� ���K`�����(\Bk��;��eP��4ʌ��y��*��L)z�A�5C#-9M��te$���B��\�.`�cr%cx����C��  ,o>~�!��;��
h@I}rƶ�ޝ����ĈaC�Jjr}x1��
���Bk`�#�wm� L"�neG�6�}��Lt��H�h)Q��"eg��'V�)��t��	�K]�)v�����|O�,�<��^�Z�������)��}폐O��Ph�$����^p�	��6�ިb���-��ȕ�s��A�:�aU����([������8��\��RO��Wǵz�`3=\`��K��p�溅���ͷ��A��#r��p�Ҷ�ޱ��wG��U��3EګM,T�m��������W��7�q�R$��      j   �   x�mν�@���7E����}?m
DE�&�� P� ���L�&�������#��C�w�'��jԚ�"�Ⲑ�}��a�v���C,�8ߛ9�,�%L�%������ۙً%Q_�<"4��ϳ���9eR�a���q�l��w�"dk�1O�0D�      i   �   x�}ͱ1��ڞ"��`n�4�� �+NE�{�_|B�
{C���J#�m<����u�(��,Z�9�r���4�mBv_���)�*D�S�`jJ�ߩԑ�͹��:O ����!l$D��q�b��=�����07      \      x���ˮ]7r���S��A�g�,� �YOx��[F'o���q'�Z+i�lI>�IV��bq�Vt+���4��ij�ά��
a�V�����X�MF�g��B|s��V[>���?���ӿ��~��ǯ�Ú��~��������O�q�o?~���A���gz�ń�)e�r/1��c��ꛭr���@�萖,���&�Mo՛J	9��Ê�}���K>��?��g���/Ĺ�F�4��i��>u����V���!�{��e���mk����6��M ���^MR��Kr)��ǿ��5��@�'�F	��9��M����w�6Ǖ�S��f�+�O�g֍�e���������|ie?ĩo.�JH����O?�������/���h_֟?�������|΄��4���۴�\p>.�n��9��U��~�������.J���|���%���i|nV#���![WF��]#�{��*�����(��4�{��h6zY�����aQ@!�"��h�3>�7W_6�H6��R�5�ΏM�}���*.�5cɕf��
�����_���I.�mͲ-��]L�����4�� �W��k����'ė�:��	��	Ջ);cs��1�ݪ�=J~y�%E٣P��jܑ�]�����8ɰ�(�M�+ǒ�B�|���ߖɯ��uw){81�N϶5E�Y@�4Ǣ���=T�O`��N�J���f�Y��\����� ����zX�%��w�JUt���q��>F�y>U���K�7�$	:�Rb3P{���N�jz v�!	\xRM�"�@?ٔF�����H�vϧ켓<���  .�Y����`�*3�em�-��I�v���� �����]��_���_�}��ۚ�����E���aw���e� NfCbe.�3;W,���P�D�Z���r�r��(W���i.�5�S6ZJ4"� n���ڞ�z����}M��]
��`�B��K�SO�n��M�L^�J��әvN�q:>���)��H8�ʮ>{H�o!�j.YuX:T"��M��d�{�l��y�ܭ���@�j]�0P�y.+� 3���ѧ����~��@^��	'��j���0kR[6�t�m$�dVrZ��{�3%�x�m���H��́8�[3�.���{z�jt��ڄ@md�բ`��$W;����#G�����Kh���>ض�����4@6��Por�d��ي�� I[�T�M�!Ɣ�zc�pB&̅(�c<���_>8֛ ��F���r���|��^{�-���W,.���yEi�E�����s7�%�V���l@��
��p�!���b5jyN�,�j�J��x�l������f��Pe�d�J9�J;͌E�ZY�GH�EJ��-�]S� اL�P*ΠҴ �o�;�g[���`_?����щK �`5~R���9 ����|��RVS��3G8�6������gC�<Ҋ�A�#H�`�!
�8VB��T�m�E�H�!y�ٮ�U_���0P�a�F~�q�� p�d~��Aی�HQ�Y�G|t�ϔ���`:��Xs�����@A�G\ɧ�3v���ai<���I��-�w����q�2�KRLr��Pqͨ�IzS��w��i�sW��z�%�������pt(=qP�)�v�	ӪB༏��XQ�=Q�ʣ���f��v������?-Ӧ�}�(4�H�u�[p+�p<Ѿ9�=|� (���0D֐nXw|h������<�!�L<4s�ƊQ���v؝䝩-��BDԅ�9��Q���|_���۬�3M���$�ym�>4�}������`��@�P�i��Nf�ӮRB��6����{S��@%�sy5<P�f���Ս�@AS�Q^�"dd�LYd�:v.�����v�F���`�~D�B��ȓ[�	05�N$H�[t�]&��<ߧ��IW+�D� �eЀ�齪!��N !΅�#��=�j��� [�;�	)V�ɰ�	�[2B�\k��L_����&m���D�Rܦ!{���� j[m�Oz��!ߜ芢�5��(�8֒eJ�ڛ�s��=I��ac����")| L�ݩ��Mv
	%+h$<g��.�?U��Z���<θU~���
�z����g���^�h����}���;'&��zNEŷ**e��P��jW?�,�D)�&k{}[4)��$�8x�T+����rۯ�N����5�j$Ѣ��*u��cے���3=��%��;�Ҍƀ`�iu�:�6��y���k�ц
�Sa���̱A{�u�GC�Q-� �W����jP�>�*�{쬂x��$c��\���6f>!?F��<��m�֐}:�14Յ���#�{B\[o��lO+������ж�s%�d��� �.��j�.�9� �iٔD�߶��%v1����h)7�-�0i�><bE.���z�J��ֹ���:�6"B|I+ �@��RO��Y9hzϊAlE�T(PT����U]~@<�Ȣx��p&9*�xj�mRpd��0�C����5q�������<`�A��S�yJXR�xZ�Y^ Lz�9
ca�YqFզ��&�i��ھ)Q��n+���1�rs����F7Ŭ5{�na>��j�Um�/�K@њ1G�ՙT�l��]���TpTϫ&�u,,��P�I/*�L�F4Hv�d` ��@=����N��.W��d|'Fq�:�Zἔ��P�!��-�D8����ő��
���D���]�q��C?">� ��Πa���^U#�1=i���S�i�JR*�љvS{-�sL�,�B�B���*��1P�sұ7ۆ1���ȁv�����ji�^�^;`�=�����-�2��RcC��q����n�[�D}@O���N�=F8��XV�A/~���I�͞W�_��	�D���*w�N{\��A6�F�ߜ��z>�j����O,$Jف=����_R�QҬY�S��t
Vk��}��э���M��5?��h�ܹ�c���%Ĥ�j묜���{Mk� ����-����Z7Vm�[8���O�t��0��+���0���Hl(��ؚi�)���;㻈�ɴy	E����K�l,�P��1���R�q���4;j��2D;N�Tw�y��-�r�ku������fX�� �i^���Z���N-��jB�S^ p�� ��b��E�M�����+:��:W�h�|o�w�V�/�jF��FHGbIV�I�����7
�.ԉ��V+��*{t��T�������/�A���{��<=�5Ѓ.+��	�9����Ё�[��ܛ�/}!⽜j�u����6��va����vź.7-r�/���a��R�&]���)�~Q揓�hwy������Os�mvX���O�� �k�+}�#��2%P�/r O~"zk��A��ZeB�v �˝
�?�F��\r���
퉱bC��fٸ�-�K�H��@x��t<-�F:m�d�;)� f]�+x�����혵5��/��	 4�,�q�yd�Kx@8���/D|:��������V��4�i��i�h�x�k�:ר�|l����U��E(�qK�e��Umo��~�&�BΡ4�6*���PӀj;f�=ɸ��v�@ �i�� ��?' 	I��)��o<ϱ���q��Hj{k�)�f)݅��q:q�[��R��mK>z�x��zcE'ON�0��{H1���qT"�D�K%O����HA�֩���J$�͝�L	e��3r������{ �$l򧈍H�%�ʆ)l/~��Q������@�DH5��D��b�X�Z�.:�8 nq�SiH��������\�GsA-;A��*�Q,ZXI��o�=�-h'���sn��j�kz6��7�\C�{z}H�w�����D4/ƾ�,�rYI[H٬��=�o��
심�G�Ѿ��z�#E	W�8�B{b��=��p��`�I�S6�N1b,&Z��u�k�> ����j��^��<8��.�������[�:Q](V��۽��t]����r|s��.��8�]���|@���=����KSM�x�l{�=��`�t�H�I�9D[a�,��mֽ�T�r �,
�W{��h��9���Ry�5�o@ [
  ����4�S.�<��A6$���V�0�4�A;!:�OV��^E�P�>�v-q&::�]��=��T�+��i}&u��:F�f��c#�\A�.�^M�P���v���R;��Z�:��1��*G�닏��isܦ%����N�@xH����m)>��d���$�Mw���%ź��V��B�7�k�Y�S��c'��X��ΑГ<(�\Զ+����@$t�O��F</qӬK62Л��cR�]wb�k�ٿ�
�DF%;3Eo�fE�y�Yp�J�]����^��`�9�SU��ݢԧ�¹����=���c�vͫ{S���a���Ť8t��Z ɘPP���c��hKhwt�NrHO -\�M�ܞ��A�ۂ�{\IT����)8I ��Է�v���ݴ�8��h��)�қu/��U���^���Q�:����.e��AXy��iˮ����?Rޢ�g��Q-P�mB�=k�Q�n�v/���P��t~����e|A]A4-�2v��,:1ާs�5�Ɩe9oC:�4�"j�l	�Q���0�+:���z�bkr�Ml� 1�цN��5�h�2VO������_�#��>���G�cǠE�N�g}_A��\���g�7�!PQ����GwVq�e�l�[�i4��Ķ!rp=k��oL�/i[�'U�wI�ǐ�������O��P��P�t��]Zٗ6Z��jV�aZ6*�o��8�B��R;�����'�K��D;n{��R��5z�"Ee��M�*�<�F�DIu�h�u��l��_����������դ�9`a����E�;}��2��=K
(�tln�;�H]UkTD�5J���������cQC$����)d�+ 69�Rl���p��Z���G3�\��TU�/����F�;��$����ñ�ׅ0�>���/����~�RT>GH/?L
��˸��C_TY��^�z�� 9E��z9�|" V�N���9{\���h�+��}^�R-Tt�a�����f�P������j�6�'|���9���J��f�z(LN��ϖ�j�m6�!��Oh-�����7-���&�r3u��jl_�)�\�׹`��=��Ɯ��ei]�&f�N�$(.����N<T(cK��`N�y��"�ȶ٤�z��_��������؊z���$ׂ��l�w�4���/+�:���Bé"��$l3���*���� ֞��2n�Ƞ��G(ԇ�7p��S'P��?�|����i5Y�M���q��A6mx�8JV���6�����  �5�R>"=�l��5��IP϶5��v5d��[��k�4�`�S`Cs��R�\�3�Q�S�� �>pZ2nz�	��%�?�_}Tp_P�t@�V�6���u�$�;�p͛Z�9�OAB��:���z �/��I�cA���g��F���{��Վ:N�Ȃj�5��	�ov*\/H������~�e\ ��A�����c��ot�1M�FN��Z��v7��Io(� 
� �Q�j�[�Oɺ��B����N���,J2dJ�}�K�C��]D�=�߅��>��2Y	�^32�NjG���r�e��z�p0͖�1	���I�.��7����b����kzV^59Ќ������p�F����f%	9u���E�-��'��͒��:1�	Dka_��~aN{�=� U�T�����}�����>жZl��ܥ���q����Ip�i�y?)O*^� #Rm�:T�,ih���J�T멣�\��]�F������/#G��l��m^'�P����B1k�$p������e���e܇p��6�+����`i�[c4�k�v�9}�9w�Q�Z$�Σ��{&��,�7V-hA�&�fA/�[�A�}�˸��@��y�_��ޅ������J[�HZ�YQ�,�i6���"�,������s�ACo�]���=�{�<�zMhP[P���P5���� ��z��j��P�����ѵ/�������2Bg��� 5�o�j��8��*B�pBYb�O��p� \��뇞��QY�B@V�6�^|H�ٴ�r��=/�}H�K+1,�ߌҵ�_Vș�H@?�>�q����ڭ�xJ� vU��^Złڑ!z� G;_�6��;'	��ɛu�Q/&F��¤٧��]����_n_~�������o����>��_x�6�v{��Т5B��mܽ�m{*��T�
��* 0����o��J�����~����������_Ƿ_�L�b������mSb$/� /��Y�u�N��ݲ���Pm�6�A9�6Zqe�(�R�>&�K2���`t�����c�cV�W7�s�Nȅ��F��U�ղ��fuyIV�1����W�&���,��힆L�덱�{��qg0��M�)ښ1����=H�fꇞfK�i�|�ɹN�Y�p&���Q�r=r��#�Z�y�R���ӄ�齸��(�!�k:�#F�e���.�� �b^�&��E/O�w���\����v��ɏk~�}�5��P����䭕�����V��y�8m��W�h����P�^\Х��YTJ6�_���U現�S��°�k@���0��K^�d�Q�wQ�%����4]��T��;�>ZB���4qcų��y�7����dE������ '�      ~   (   x�3����q70���O���2�����7��KK�b���� �
X      �   �   x��λC!D�z��B�8��/��8u�s�J+�H��>���<f=`2%H��{��]�w��=�;�;
TXCPy/MkU����_�>�(d�A���N̓���rd��� �De��A�rh?�~����7h��T'�l��N�}l���멵�,�C�      h   �   x���=
�0�zr�\ a~�������k���.(���WL����Rl?�״���$�q���8��1���[��*�!dHӺ���l��r�9���[m�rc�ʪg���:{n��7�l��H�      �   I   x�3���(�O)M.Q(J-�/*�4204�50�52W00�#N�ļt.#NW� N�܂����T� �c���� ��      �      x��ˮ%ɲ]�>�/���M���6;� B$.� }���Y�YU���+
(
;wU>�^+V��=���M������������/��?����������e����/�_������>���������o������������O�}]W���5�\�?"�_���.^Bm%��먾��O~�+����׵+�ȟ���·��=������U��p����b��u��o��߯�?6�����koux��^�ϟ����ן��w5׷�Z"7Y{}�n����K-no2�t��R��Y��<�ɿ}'�q}����~{��_�e~�����EN�~����*?I�[���۟�?;�����������+���}��ۯ������ ��ܵ~�KysR��ϼ���3���8���v�q!�~�_�>��u��w�\/�]�J�\�O�f����_���p=|����zx�}�������o����O�u��F��e�M���!���;+�|�Uvge�����
?}��+}۹��`\gV��5�g���Y��;?��w�n��26^��3]+��;�?���%�m5`bs�UCa�;�S��%�P#��[��Q�Z,#s�7��R�"3ޱ�Xs� ������TC{b)Y{�z^�I����\c�r/wea,w~�,wY�)����Z[՜�z�U��[.�J����F��n�=m������ok�Z'ע��c"�%Ο5h�lH�"�0B���`��S�Wb5j��U1c�B9Ϟ����+���ĳ?�3��j��*v�u��*�{� �?[�q��i�]er��<����l�ր�ߩ>���m=�9�|���ͥw}�����F�?e�#=��쥗�),�ӟ�֚*R������^uc�6*4w����&w��5�џ|5��W}Z�k����1׻�~�\}�}���vK����7�uO�9u���,p�77�S�.w���4�y�	i�T�}�qv��}�O����w.��z����OY�[ٖ��1�����o}'�y�y��o�䥰E?��b��G�{e���R��-�)��\],J�#ݭ�'�͸�{s�'e���q#+�yݑqdQ&�WxŻKiw��NFf�wbsƬ�Ns$�n�r���ڷ̻��xm��c�3ǌ������냐�w�ť�d+6�k`��>��5���dE��9��z？�j��l_�j\w|�;Ų�XI�kML�
��-V4޵���+�9��ߎ�,�5+�v/]}�=9���Y�5D��=�ݐ�:�^�tt�oT4�J��~ۈ���S��ye�;E�_��"o�,澹x�a�'����ɏ���o����Gx��t�}�-w�����R�}�kW��-�
D�QP���`?_��s�%%�u.���<+���ɟ�o�BEM���|�;/s`ؼ��Ry�x�Κm4 �P>�a����{�6G���ju���{�q�JA~���&6�aJK�Dq���.��`�t��҉�[�����i��v�rl��j���5��u�o.(Kj���]��*c�޹��y)�z�U:��}��Z7�u�=Z
�]GLП� ���Sێ�.�(:��&���oMν�����M���{��1��h<�ʳ����{`�S���
cc�P���{E�R�zyS�����	~옜����{�o�(�.,ɨ�=o��b
6���lgGQL+zi�%��F�9��S�2픶��^k��Sfٜ'�q��J(-\��o�q4��j�λ��c��r_|ֳ0V���;�UE�����9�c��O��0���XV��=���wz J��=����b)"�a,Bsչ�Yn+΁ �Y�ޖڏ��д���r���Ϻu����f	G��M�:�jc�wM�~H�e���<��7����W��*������K��C��!�Oee�{﹗��E��='�j����8�0z�Hk`86v�F�h���UW�ob��0,IN��~�@E���̩���#;ht��?�u�c���C��GC-/n�c�?���� �`�7�<\`
�O֍�99��:;3��q�7r�U@#����I{\8l�	��4,��|�1��Fq�)����c>���<()/نv����N��s�"!���,�i\"�!6��ȋ `��	6{oVG��YE�0#|*��3����z���?h��Y�zxkNJGW��_���|�fg5�=d�g�y����?�Av�:�{�x����	���Ә.��X��� �&�I:�#�H�"�a�Q͠��"��Ј�֌֑&�46��l.r�� �Rp_�)�;���i��ƀ~�ח�֍l @4�~�X�7��r^�Ē��L�>�t ؙu���'0a?���Ǟ�yV
?�5��ud��(������J	�4�;�����F+�F����3�ʥ�O{���/�����N��s~��5��[Xd���s�^�3�=��k�g�� ������� �R{�������eS0���	��X@(�s����	W�$�v�Z掿@=��h�G���H�����,�Ŭ�2����p�\B/:�W?&������Y���c׍jvm*��6?޺�`�hEa�* �3~�Xy=}wśD�	�����Ⱦ�W�g�G\"��[���[A2�ˋL���@+���s%(��K6<6h%rg�H��4Q7�"�x��b���[�u��"�d����(�`	���1+��H{_����T4���85P��������Z��v�H��,c�	�2W��K,��O ٘3��Bw���рn,��cV��I@-��<��Ӝ�0(:�q��`�n��;�g�����Wb�����h�{�#�����HYƝE❉�}���5)6P�60�F��� ,� �����,Ѧ�b*�U�E/�J���LL��z�c��HWAm��ta1� �gqN�O�A���``�R�B���b���eF	�n��{;�]C�����e���>�� �\�Sۋ�$���2̜�$�V��2.���¼�q +QxD����A-�	�����(<�� h�l�S�\��)�h ����gM �I���S,�6]�h���]g�TZ웉"��� ���iX�) ������?�?zd�ׁ��O��{EO�Q��{�yH��d���\y��|"�S���ʂ~���F`�@H!�z�(��
� _A taa�����&�3"@��]�n�K�:ߑ:�D�*�;	��K�� n)u���P�i�t�	�<AW��0¬�4d@�ф�`�{�l`�R�@����V$y���gz63���4ё6��	��	EĿ*�<���;g�C�����.��s͙��ä��g{1/�VeĎ�vC���И_MJ���M��*-�1�A�e	��u�!��Ї�p�HQ11�Z!�>�5�C^�ja�!����A{>� �X�#f���,L����W���e0߽Ϊ�Pl��"!�I�W�t��D�>�ώ��׌��-��Q(�u	�(�����c��u~ˈ�7��e�W�§2X����p�N���@@<��waX5�8@D{����~^`��Q\��nA��Z�Q�)��2D?I5�p�O��7��6s|b0�
ޣ���p��;�	�A�\?� :9����Xx���K�*�h�	w�s<Q��Nv	g���`G���F�6���P���-Ҽ �o��%[eh�R$��w�v0Ѹf�H�W������\�.�Kp�ŀmw�)�׌ڃ������\�8�'�d��ܓ�փ����2�,(�5�	t��#���^D��;'$1��'`Q@>-���A��^wp*�A�|�j�mD�o�I@B�X����|�H��"t0|{�^EvWɺ^�)�ƻ�!B6�^D�ī�\]�8;��P��'�¢?�+\�c.Q�c"�`��9yT� �:W䃮�2Nt�oj2��a���h���k��i4�$��d���Pp0� �U�J���NW	i�X,:f����'�5a}̦|Qi��6�#~    �&g�L�� ��H����7ۆQ�8:@�|��ր�D7���nT P��"I���"fE������+g��B�B	��Ϛo��@���-��6u����#ly͌=� ��wG���H#�ˑ3<�wp�
X�a4z�ă�3�/�IEСE<��Y���Nj�1�w	3�0�����-D�@�	0q_�2��-���Pn��@uϟ�8�'@��fk�t�D�H?�Tp�,z{	�Q\�����B+�˥��#���T0�<y=�WT��}�iO	Pt�� �$���!7��0���4���`/O�bn����<��oW��"�F,��|�3s�\��{���)�7�jOBt���"0^�~�4�K<��Ʊa1�XQ �{�O�X���c%,q����v��g�^
+���b�/�l��X��ƾi�X��rM�,�~dM����~h����蟶fZ�{��b$��	w~a��l�|v��>�1�a�: �b�����'�p��ވp�y�w��Dg�\+Ǝ�|K� d<AƷ��1,��3��v�j�u'6bU�����j#�p�`��d����Ҕ<X뎫Lh[���.��X\A��d%E�9�\���8 /�`�@XD��
���*�LBǟ��f�2Kcƅ S� �.~i|-����@�Z0f<�8�1���J≌��=� �tQ�)@�ϩ���NL�M%�W�E����·�`X����^ގh����݈��&���B9�Ih�>|n��nx�;^�0�} &��$0��¢L�~��895`"��\�p%t��3%���1��u��)@� 1���lte")�?p@0TI������m�׎S"T�y�Mw>�a�Se�<�0C}c-:��J�'S�XIn{��� /�""X[Lp\��w�Vd��4l����@v�%�A�%�nbL7b:�4o9�����x	B���	o
��<-f��f�|	��;�^b;;�'PL��ׅA��o��H���� 0���.
��xn�*p�HR��o��C�����>X�2�f'��(^--�-A-{�n��=UD�+��`��}��j^z7��K⮟����@	�]��[3x��i������@�(����Xy�B�ح��� !3�ǣ,7�����OS�DY��7u�7�=l3a3�����E��>�+ 2���Öv������l�-0j%ޢ+6���5�ώ�����X�q� 7 +�g��Q;w�9�A���|d51cF�1�,�v{r��|bm�)�-�H� A)����{�evZ��M>
�9�ℱ#�`#v^��bkY�r����i�i(:�~cY$�Sa1�F�8?s�Ө�R�fpj�O��X��&�TyM�MA������T|	�z��o�Eǒ�������r��m6~6͈Gf��!. ZZ�IX<��j&�ߘ�Lw�����p�'��+�"����ðu��}�	��z�.ph��+����y���٥��˨+�C<����	�}`*����S����ds�# ��Ӊ���=�XN��[Y��Դ��5����5{.U�4_X=��� �<��e4K�>��8e3�u��L�1��1�[�V�{ƺ�r��^�'F�.�~��S�cWǎ���9B`���$� FD�|�иy���FnK������iȢ�O����<`Գ�
���N����m{��C�Z�g�#��qچ��O���4`8.|�4�������3�>y]&V�ǹ(-Fx}r�,��Jq���c,E����S��*kX.�z�|�MD��|�an^K�uX�x��	���_���E�e��u��D3�$C���0Sɳ��H�H��K!na��^��0��v3p�$�Ľj�����a�I����v��SA��p��@F�����Spj_��{�l��X�="�f^��
D��ѐ�OL7V�B��:) 4��sjH�����-�������V��s��'� `��	�@ ���Bs2b�z��I޿'*��#�Vv�M�"�sB�q����� �̍�����J�6OV�|��o�6�����qS��5��YI�:�c�&�P��i�]|@��۝'����Ղ1E�z��xi3}`,���se��~��Đ�k6N!��9n��<q{geMߧ�,5�fax^��f5�Hęw��L�ZIsa���=a��.|<!�������6:e�,!�ß�@)s�J��p�c���,)���v�m�&%���AD ��j���A��}V.vb�]/��AJ�F*������ ���d=��zi�5h�ɞ?��M�*~�=�~�X~-`��~{_:��?��������y����?�����Y������J�������?O�ϕ�?���;���?���~Y�y�q��O��Ӻ��Z?U� �����"�����I�����{��k~���ϟ����?��)x~���Z�������>����׿�o"���_�~���'?�!??j�^��>�[|X��4Zg��~c��_�����c����
��e���b�=�����OS+������5k����h�p����V�����@���z|��@������G�8����|n��w����I�� x~�qv����<O��o?�~���'H�?�o+��ʵ럼�`�W� �o�"�����-j��I?�����T��Fݟ�+�5����oD�|��+ϟ���5���`������~�Ss]V�F���u�|{�o���^��N���O^C�l��{ڔ����$�>�=�γa-��f�Ym��o���?��8�X4����2��"Sx��W��U��'�~u�֒��W]�������7On;��%���%7�_�����m�������9�jd6u�%Κ�#���מ��5GB����iq��]�'_���_qN�?��7���?���X~����S��&|������9u������]n�:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:��:���Q��=�T�X~��|��{c���|g�é�T�e��o?�/���VL���g�w�6��w�J?����}#��ԋ��2_|9������&��� �k
���}%��,<�e"���%H���x���j��wl� J���t��82��Yӛ< *��fY`J��h�M��~�Ow�|����9��"�bO�+�������ϻA����	�fDO�?O�ϼ���������<ŏ���<OG�ًS�-H���uH�U���(Kqqo�>�s�v�3n�h�
��?w�|�E!�$���??�.ٱv~�5���u��C2s4�Ź��w򹆩^ ������y�<ߏ���?~�?�_�C:>OtM�����;V��֦[���iGB��i�f[�����;%@�P���?�Y]�����z���[���#�.I`޼��ﷻ蔗�3�^����Y���������#�����Ӷ?�c�!�H<ieМ�
�����ٝ}v+��|�>����>���`?����}u�����$���'��}}=�_���I�z__O����z����ד���������$���'��}}=�_���I�z__O����z����ד���������$���'��}}=�_���I������B����|�O�ϟs��FE�3�_�����o?���?~��������v��{�Α�.?�d���מ��sf_9��U.�{�5�S]߸�|�����ϻ�%1+wv���s�������K���p�Vκ��JY��d�j��Y��l���0�C�!攂4#R4�G�%[3�+V����^�	VX�PR�=�a�s.�Ξb3˹c����0�(ӝ�v5w~8Rϵ�t'��v�|��7���F���asPe~'��Aa׋v����ȣb��ݓ7M^j��mqw�9�����ʇ�����%�i֒��;��klk�mqho�;�q~�Kt���Z@v�>�U�u�/�*)X%    (O���WK����ʱ��F�/q��b�q��hN-�W`U�|R�=�=H9�A�@�`;j.V�]���r�����=.�Z����/k��w��������	����E��8��������'K�nɸ��Ϳ�H�u��i����g��%`�l�����]�e��J�l�� gؼͳ<�۟�G�a4�Gj
�"��~+W�C}َ�o��=��ӦJa��l^f��>��
�Nw?e�*s��'m�����]� ���oYk#?����x@	�*��e%�o��s��5c�^�[�~��Z�j]��+�NI�R�~�R�����SQ�2��ؖ]�K湝����v6W�:]?��E2�G~�-����n�ݬV�R~���٠H�}u�>g�c�P���)�.�?��߼�?ۚr4(����犽����j�6:���v��{�>��/�"�qY�y���T��٘�62^;뉠�lvi��!�����T#v���g�-��l��d��ucGmR�<1r���O�\Y�N,�`�߸����}@�X�P��.�p��k?#���?�4��5�Ӈ�����}�q�9_���6!"e2
�ؓ���* ˅ar��~�֙�H���	Sk��#�I��Ƴ���>�7q��Tp�b30&��;��EƤ:�$=v{���y���H��}�6>�(���6�-�A��� �^11�v��1l�k��lBw���`�-z����~����̓l�4�=�e5v#?��C�#��0,H�m��?B�؟lGh�Zw|O�XN	H��~��bU�1��l��(��%���,ʭ#��;Z�1�(��E�d|y%��95׵mJ�r��l���:��E��>�2b��{�4'k��,L�L:X��Z�ǃ��*�G7���T�m���?�G���>`ԕc�/�P��D�?v�h]m<���ޡ�rJҫ�Juع7J	�W/�+ޗ$1��`��A}ݮ`��41�psOD�%��x�Y�Y�a��a�(`@�V��|�w�ΐ�*ɉ}ː`_�T�[fֆ���'lj{e���Ɵn��}+�w>rs�o�ĺ^	fG���W�xAa�Cl&c\�{���eK+�3�����`9Z�lK�%sym����o�i��s<���.Mf���~����2���4��k*��e��v�#Ͷ�#��U�|i�F�w���L?��t ��+�)r�;J���o��aG~	Rx`��lk�ndW���4�en�!m������V�:���j��~-ٿr�Ʊ��c�R��&�M�{�vp�2<�w;hn�9&�@�O��@!P_vZ��.�_���#Rל�{.?�2t�5�r˕�� z������,�)*/��]�lѣ�>��`�%w@�yKG��-l�,ȷu���,��5�'V4]�$.��_d�BZ����ߩ�Ư��5�-��x5*���]X��sK���4O����-��MB��4�>��5�����]!. jFėHl�Ta�_��ԇ�,��c���b*[-�):6�-QeEPzܓu��Ql>��1���͊�RAnA|��и�]��K�mqaam@h�9�n,�$rt��|  ������*�nغ9�p�L����?Yk@<H�8��]WdHP/�|
��-�ɲ㡖m�`_��Wr� ���%K���E�䙷�o�^���~q�1��Z����^ݬ2���1�Y��� ��lO������y�U;���[�n���hɵ�ݺ�B7݅(�&:#i�N��9�Me3��|(���o���� 	Z�ڷ_�����O<\����Htym�5_C&����$h�X�����?��[n�[;z�����&�h6p�O���&��s\0��@��ر ȣ�R"==� ԱhO���%�$�M�������%D���:!��hC�,i�LB�n]E}^�Eg�,?��:�[ ��`Q��vJ������eM`�RF���Gbl�GD��w����h�� 
�f�w�5%�z��-��=��q���Fj���x��Q�ZP���9�Ϩ���?]��
|?�|QG�0N�<�/�tI"��`a�.Wa5�x���0�m�}���XXP�ӎ?�n��ubO���#�H�c3H�ff,��{Bt���%�ϯ<�ě���>>��� �
�>K�x/<^��e94���X���y�,A�%�e�^i�ANl�J:��E qh/��fX����F`����^4���OCm`�%W�1T!� �U�����=���1�@(X4r/�y�BmTe�Qj��l����+��Mpq����h,�`)�	b���I��z��DU�W0鄩�Gq��8��0@����6)G�=��&1��0"ȅ-��W���bU�[~`��<L,��h���nk�s��aQ�`{|8u�0,�3�io%�B*^�1�,&�!�k0�$2/p��1+FJ���!(�6��0��"R`� ���׈9"��+T�7��ǥ$��fd�q��0aN7�mkw���>H����x%�ͨ&hmڒ����(@w+q�LY�6B�bs56�͟xAY�O7��/g/8�ᾀ��5�L��)L��:0{0K�a�* �	�V�]�e,��؎%��=hᲱ�\����u|՚��* >c�$�6�NV&��9<�*b�rK��F4WZ��d����`��I��W���i4:�> '��E8#��sMc�(�:1��La�_�ҌA:r��e_#
�Aa�k��X_�4�W �> wK��0�
��DҀx���������(�w���%��G0Kp	f᷊�ʜ���g��
W.��_<�I�+�u�R(Ȋ��x2a�CBt�@��N�|I~*q�,K�d�|�� H^2k��^�d2hI���[��P�8�ro�Hl�7~�F~��)���һ��"�Kg�N�|#�iN0#�����|oI�0Շ#(�T�8���Ʃl��Y���C\�t]Vhl��?�'�6e�(��{p��P,�x	Ue٩+�����g��++�	�#�	7Nw50��P��'�4kVD`�Pm[O\��0������[���iS`.Uz���(�.7����%[�(�'�I��kM����ígH1)�<���&*b�g&J&7(E�I��\٬ T�Fgb��х�ǚ�"���`7>��3`��V4� ��~�zG�[)Ieg+�㕛�`q�T��R�0��^)�Au�)�k�����bd|�p��/)���I�2�d�q�ID�F�$j�-#F��C��,�����ժě3y���y[��`	]Y�NDֈ�0���>��6"�����p���!���;�)!(���2nS��䧒l�ܱ|��-C�r����W�Î.R�I He{�=���P"���~u��g��@il�J�7�Vl9�l�+�*{Y	¦�:�R������`6p+�,�7��
2�L�Ő�1�8��b�uƭ�
 2J���ս��&� �9u:�L�^���d�7
�QɉݥhAv�-�I�2ۓ��+�Iơ��wXN�ήT�;�^�hɬF���_ǣ�R@� 1�-{I�J�ԍH �#!����6��;��	t� ���ڂ��:�eD5�2%���� ^�cǱ �XD6��Kg�qX"iBD<WN(��7�����]��p��&}t���U$��S�Q$ν�.��1齾����(��[O$cr�%e9�oY�y�ĲH���9���G�o�6|�(�*/@�"~l��sy
Mm�*�M���4�|�7��7<N@,.��$=65#� �b\�j�G�@��Xޙ��肘�`|�*BZ٦���.g��;<kD���*��[�!� RL,��"Z�h�6��"!Լ.�WT�����k�Ed��C�ݰZN�y�y�2OKl�@6�ceKV����X���hwJi�^�{%���6��4��߀W�Q7��q ���V��V!%v	ӳ�x��Ӑ�yXZ�y|p7И^2	ذ�/1h��H�Hc��}E3 BrF�>Q����JW���mf��Wb���C4��B4��\�ej1 �q���qӱ�r�s���\�f�Ȼ��#�#~    tH>�7:x�{Ba/
���ь"���� 3:fnO��Ȓ' AjXX	�C@�h��xK�H�#?Ut<V��sx��3�凔sSΠ,5���2ǻz��>ȍ�\�!�1<�Z�!�R�_�pķkB4I@=���d|2�S��dL�¿R79]�ci�0�|.�qI�a[�DZHj��,��p����qo
[�a�Hv�����	o��IW��q���[b-����]f�+��*? 7<Nq�dqG_���A�� ������]8��U%��oO��(sS���!p�sG �b�֬X*>!�sd'��5LF��@�|���є��cB.O@"*_y��=_E=3Z�<���Ȓ!O��_�ݹ���'�Z��=��F�;�0w���y�8	��>Ob��z f@Iލ���9�5"]r�Ia)�@�u����bk�+����X|لqbԞ�$>�iMT h�9�5�D�ل2�;��ɞLE�+c��_�;6Rs���8δ
=j���е�LGɽ/��)2�2.�h���!��S����i����[Ӓ�ibҮ(�u��)uX����=����ǫ���QA�p�SX��nёEʓ8�G, 3~�_��@��l��#��7S�-�\M�;@6����F��z 	���i����������? �l:<����1���䝏r7�rO�R�8���vJ��NxD��d�L .G��~9뫼J�-!3p�N�Eʧ=H�c�]����n@���{���a����E��c�A8ZD�� �w��6��ԉU
��lt�Ҳ��i:�a��� .��S�S"Ƶ:L,p�7;��
z%Y��s\���0:�����ؠn���c���D��b8*��n�{2��ͷ�� ������1�ɜ��眃�ߐ�%�D�WZ��-������?w�J���Hi�''>,�F������~q�/�]�Q|���Hؤ�D��[���l��|����1Y")�T�[�O�$�,4�"��iGsƣ7.�bi�۩j��H�l~�)M��a �ޏ�;��@����g�
��z��QW�,Z��P��Y���-ʄ,�;:�3�
����o���w �x�����019��#�4[�^,9��Ӕ��b�Q� �\;�a����!�6ʈ�G�\X����OF{�Moj�/����%�����r��O�5�r�n�v�3Pq]c����!~^�Λ9g?�	;D�.���L'�vi�B�9�ϩ���c�p	��7�@R��D�R�c �ym ��%�����!��l����?�Ngz��<�>|��`��N�r��vpD��3�(8�VB�r(��mlͅ@ӺC���Bw��#���LK8_�{Ȱ=����SVٶb3�l��5:�x�J �9܅k���p��J�w�V_	]*v{zZ����X� �y�W�ß�Wr�',�n����,�i�+;�����D��a�Dc�ì�	"=ԗ*�&0x�xmٰ	dA��#�h;�V:�G��sM/=v�w%�#4���E�Ni±N�ƒ8`/b%�3�ғo����uec�xˁ�	Ýk	:q�K̈́��	쏲X�v1�z�gt��N�}�h��Syt����q�D5�G#�k�n9FS�8���$ֵs�2zcmNʦ�[����<U&C?��i(��^&yh��׋8�#�A��I��X]�=HGP���8v��x�d��@���!by=qĕ�q�9XsVGE7/O;�"N�Z:�K�_L����c3:��.�l�R�X�ѩ����!Uq]��7f���+:��g��(Ih!��I�����\��t�j� UA\oi�c\`��S9+�?�nK�j�F\�%�D]Q{�1�����G�1�X���AYE�zG��c�򙺎4Y�Rp8:S	J�l�����gB����)��mD���*F���"?�:̥�']��"�|R{�GԪM�Y1�[	�w�����������$�EIx-�!KO=$�,{/�p�'0�`�;�<z ���~�[�D	c�Tl��q��
�t&��f�Q��o6QY�
��=Ze!b��Ƴ�KNV�OM�	�j��<3�޸�?L�ca�� �RN�yl�'�ɲ8�˴�C�_g�N��iՠ}��OŦ�a^--�op�Lt�#��k��������:���H[�J����Vv�&2�k���z����F<	��/L,�aF�`�����5("�À<�<�x�z�؞{�80b%=y�>��j��I��\bfv̞"M�;=L`���%��3��0U��7�j�@�p
}����@C�֭�`XQ�nV+^��$+y��E�`���1B��O��	W�4i`��\o�t���?q���6z���H��1>��W��9j�O�t��8]I�d���{���$?�5+��9ɀ�&���&�:W�˨,�3�t#y
�Q,bʺp�Vr;�U�]Q�#Kk�z�-��:ĸ&��!7��n×��cH[3����ʃ�Y/`��+��D@o�yD;xj�K^��~�;2�D��-ڴ��ed�0$��r�3�6��ý"���Eo�l=�.�HN?gry`�v?��y���\H�"�e&z�٪��{ �n}o4�9�y}x��TqO1jv�q�q�"��F  ���#7�_Z�̓�މ�_ܴE��X�0Kbc�)R,2@2�}f{< K˗����'4ļ�����vP�7��m1UM���@��|	b�et��S�<���	ݞ���c�Qѝ}
�e�9X��k�c�G���ئ|R6�)A���Qf,F�t[*H\�B�}g.׉���LvU��.�Ar�Gױ��z-F&Ys�~�=��d��9*3	�q"��@�x!#([��h��'�|� 	�˥||\�Db���'F����N�����n������ˌ��U�	�F,�U��WX��L��S��@����6FSq��auD�� �H�=��du���;y��IEpd��`�v2��?pA�~��K88�l�f��f��v�����k��&�T�X�i:я�hANá���T)����Jk*��-s�2�ؤJ��y/B�=v���:�p�5�S\n�,��݁�pDw�l8���݌eJq ����v�[!?D�ى�Y|Lw-+�� ��7�~�g�sn�`�N�C��:��2��v�%�p�i�Ӭ��|����vxPbM�v��uKH��Հ>c�kuB���㕚��B�,����ߗ�v�x�ۓ�&��0�km��
��	�/��TH���I���T��)��N�;�S�w� 0ᱰ S�->ѕtN�0���ބ�7�6�Jq Wh��u���ˏh�d��<�/4����Hd86
э�M=���#�k;�ʚ��I��$�"�"��yI�b��1�D$X�>�� P'�ηuK��u��z�+	�-�n��!��&�Y+���ߜ�C,��& ,T�:P�"��u+'�w>9��P��V���ߎHz�u�$�v���^�=0"���� T�Q�M&܋-�T�]��eY�C�4��&��m�*H�n
#��,+�.(��Ѓ��++}O�8�E��pqش��,L�ӳ���ns�^nl��l=�����8C͙��3d� �'*�M67�ɞ80�E$����b_ē�
{u�g'x��M=8�x��_�ψ4k���Kظ�g̚�2�*�1&iˠ��'r�t�|F\�!'��MI��@,#:��2:�M���]5ɦt,XZE�v2�8 ��,���*H��̹�1u�����ю�����?`?x�ڎ��=}�̍��y.���}�Lh���і�"��D�	�\�`^tyP����u$B� i�y�%j������u��1��K�P�P���<��M�r�%�3@I��G��#��a����,�a�P[d ��$`�� $��D���Q���,��4����������3����X�0@�.��rt�;ۺ�S����:H#x�֥��g��`���� A2-'�_��O /"�3��X��F�rǓ���ʺc,�R����!w�r���V���T    "�2o�Y��`�q;(r��Stܢ34�ī��3&�}����h��G���N��t�Gl/#����@,~�Ʒ�X{v�s�4yV����
�c�x:�q?����~�;{8�/�o�!�ÒL���ȂwYN���ϟS�z&Z��Þ;+"Kk���7��	 !���	󳷈�&����3�[��\�U��EX]o�Ø��S5�H�=Ӓ��d��Vwu&�/0@���D�<�\gtW[���
P]�D��z�V�:�/��K(��T��o��E�)�s��ɰ��q0�����d�z��q�6I���<4��y~xl���q���O,Y>����4��63i�T^b�u�����cc˧�������9cS�C��������)d�gK�N@!j�V:i����U宝��5ׇ(��z���'<݌H��)xc��Rqh﷗q����¢��08�?����A'���Z�pY��'��k'�B��=���Z?�'ˎ'q����ne�UN�{���J�X���N߉���������@�^]���&L�V�׀�|��%���P�"��o��'��~q�dC:y+����޷�0���������p�]�:Ұw��W?=Q	�p�p�x�K��C�T�+�<ΎE*_B�qgT8L2��,؍|YK`+���8'd �������l�}̓Ot`�{������r���"z��e<>�����Q��( Y�>����f�@��a�D���ȁ%�,�Ѫ&!:�͡ަ}�@
����t�3_�cpPLu�2�7t�&�7�+{�.=}�ŉ��L,�� ��mbv��"f��L:����5�����[O~����Ϫ�����SN��J�fj�	
p�)V��J��x>�=^���l++���a���:lx*����ԏAtw� �q�x��}�`%�@��=�OW�9�d�+4N�����S��	�MT����@��=sv���' ����0[oh_3��YL��/ uC��t��ܬg�|_�����z����(��0Aã=��u��OW:m�F����a�(gL��d!9�p�n%��tU�����I�:�,��q%�gh�:�e��C����$���!EV�����0C��|��fBPa���֓j��A�������5���L��4L(	��]��>��B/�M��[��*DB�#���=L'061��GZ��y��g >ܠ%�F�/v4Hp���ll��F�Nk�ʟ����t��(�0i��i`p&���q\�HSh��ͪ��d7<15���N�_<S��i�"�~�����h���c��n/�e"�fS����):���w���<���i$l`�^�=Fx�{��;�7]�l;0N_�hT�� �����z���Ѐ�m|	���L���.����F;��(�#2u�D����P��s4�����2bi�ȡ���Y���t�����	�/V��3[��_���S�UUO�'�kp*�.{e��o}9��d
ϓ�O��Z�Fa�l�骦�L7ػ�Xdf�-BB���ԺI=�p�3�0���T�^�K�I5���a��Mf(H�Z��;z���b $*b��h�O���['oX��:�,�e\h�K f�m���&8����(�H�M֩%�}�徠��8�-?�ں2��7��`��+mD'��JXe w СJ0O�>�$7�RԦ��M�5@��q<�~ǣ�{���u����t���}�er4��&�C>��vR
 �pB+�;��
�?�f��u�8F�� �Ҍ��h*��dx�
��E?hM��i#A��X��Gk8�r�e/�^ݧ����m�q�87zڔ�r�����O~=�j�{� @]���b�şW3�	8��k��8<�/s�q�<����g8%8��[�b�:����������
&碞�|������O��n|��t�`2LM�Ǵ�=:\򜮎l͚��B��á#qY�`�l��Uc@t����1�R��[Kz	O��%g��m��oz^̣������],� s�D�����P>��
��!���g�h(`S�o��p��U\�� �`uMY��
3�`w��vv��J�Ũ�������4�Ьc �H�s;����pߢ	��g���/Bg�B��j���L�z�Y&n�j�i�<YQh	�u�2-�DR�k�<ah���+�f�{�����i��]���e�ڧGr�S��K�>��#u�5�}k9:�x��"g/c&T��4CO�lKf�X8�MX��ՒA��q�}X3zCӃy��Q��\X�1��)��@+U�����SЦ)I��VI!������i柉�1��V~N�ɬq?��O9R���*ۿ�^Vz<a��}	�	�/������c���r�|����Y�JHm�a��͆�(OJ��̦Sp�ݿVy9��։�,a:�sF.mϔ�ȊJ>p��D!4�����˻+S�q$��������?<��rsgiX�uI�Co���A4�c8��8���[�� >䝧�5[�klfD�m��a���&&��v�,HD��� �EV{:�Ci������*�ǣo�$̘OT	a�P�S��F� ����8���dʗ+�.��ZT�=0�ŵ��2}������IT�-A�{Wv�6���(ɭ�¨�	���xD�@qC����s;�Rc���D
[N��-X������)���cO���ٴ�3a �_��f���QY+�`�0 ��FSͯ�P�`�m�D�¶�hs�9���54A=�i��"z�M�/J=M9`�h'�c���W��j��#g��v[7�&�����k�9g8��o�P��JʓKn
�$�OoO�����I�}����Ȭ:�뤴�������=��(��'&��Ђ>5A­wd�+��q��7\o+`	����[m��lY�yȲ��+�~��,9wS����1Us�b��u;��
��G�	�G��!禛u���KZ5l�^@�ՙ�قZ�Fup��O�J.��u��vu��|�2|0�j㋄`0�s�/�x�E)�Bmӣ,<��K�C���nQwɞ`�y���j���{P���'`�X�w|�rZ	�LV��t�w?�'���nh�-�n
��nI��@�,��a�x �e������������X�5�
�)$v��Xb�Q�j�a*��.�5q�㉯��ьQ�ƪˊ��
�5�* z�3� ��^GM㽧0|����&���2�~�E^�fq��t�W����/X��%&D�����^��C�$���]�y��MHĮ,|���AW*�Foɛ��O���<�e�O$!/²d�~N" �W�|�kg�Dg�Vӌ�xO�%(�
��P��x��aXZ��{�^��%@�bY�n>4zZ�l��\3k����\��^�P.t" {	2��9���y�4;fko�1FD6�7c���ѱ�����Bm�<��f������3�ր��f~�D��Y'� �L���Q`MJ�XTG=�'޳�zT�9X�FO��s[���!���*�����&���[u���.���
Xj����LpB��V���#l�����ú����U���~�!ebt�e�_�3�}Ȏf�6�ǂ��O���'",�`�΁yN)'��BL[��xt[�#���=˒��_�va۩���`q����L���-�,lJCz+�Jy��{>����-�����Xo�۩�~-���M�� �?�W�N u�&"�e�Kd;s�����nI�oE����(�`�(�*?�������r\v�y=��ko4|��,qc�RK�=���@{j'6*�y��~����p�����R�tk�d���^cW1���+�剫�|��i)��Y���:1��Ɠ-���-�{�?T�r�c�-���)��iχ&�VF˄�]�ծ��2Z^ʄ���f��I'���N=����x�'I��&��L1:{}�`����	�;�%r�n�܂��\�\��FM`�Wr�87�ED�� -A���0Ohum�Q�N������A�~=XN    �no@<%���,#�S�p?��Z���Ԉ�I� �f��ƓYn��C���X�v�7.�k���Ž��Œ"��C��jL�-�#�'`7��� ��N��MI�d-��7��v�z�yz�g#p�nGu�%;����0$��9e�����+��0��Q�ȳLGr���1�VC���z�ڷ���� �<Y��v���'�Hy'uG*�������t�}AB��[~�U�� ޮ�j��j�.H��9�����7w�76q�߅�S���$����1�zL�z�l`c3����IP�X�ן�@t��@N�����&[�o9q3{zVO&?���%����Ϛѧ[��w��z�i��.�:bvA�~�,� ���U=��3���OG�2{�����	���]V�lm�����7K���jO���>6��E���`?$��2�dZ�{l�(d9>m��<��b�z��3cY�1%o��ϒ�b�t��fJ}�i������\��x����ZpW�j��pm`���Uw�c�-V��]g�#���z�a�(���Ma{Z[�n�F(Y[w��aBx���.��e�=C ��Ry��bEu���t� >�VxW���m�6>��O:������:(�n)���9O~�L�3i�:v�
��OzX[.	\0��3.T�A�o�fGk���#���<��2�P�g�B���pI+��\�z��%#4��J��$ݨ���l�G�]�N3���+O.���o�6p��hkưm;��$ ����n�ڗ5�Kc�I�r��Hrj?�����z�#��H��yK�y׺�x$�ۃ5��>���Nh��_[Ml��>�F{�-�|L��Ҳ���v��X=�M	�=��E��oQe�1�H�"	e���l�������D[��3�D��a��^N��{�vй^D �\��r���n�v��v�(�i�j�š� 6RM$w����l�u��(/��3�&��n����:�������10+�Z2Qj�ax̨q A%�L��*�U��ޖ"�9�'S�講@�{��S��)UJ<3x�a{؎�L�jf�w�O������X��.T��Q3��i����*�a�F�U�k�6�x�n�ϼމ�_�z!��e	�<����tX./� ��\劶'�@1�K��fI�C`�sHVM�DHc��k3�mo�k�ȵ%�������=`wb�f6���궲�]]��"�1����!�v�H�ۑ�7���E<���"U�=W�#�,n�eʙ�@[��ֽ/��%���Oɗ��l�q�}�&V�����k;��F�=�����W���G~)�Fo6y���#:��y���'��/����s��V����I�g[N?��%��c��zJ��@�����t��Ap��e�ݸ���N��]fq��]�܃"���O���Ή�(��Jی�;�}�8�b�v�9��7�����T�[���Df�"v�9���X�[�o�o'����\�]��e�<�O-�M� �2iD��)�T
���`����}>伇�Y�����r� =� h��G�M��;����S�M��~�uNh#O&/%Nmɾ���[��l�������:h![�_ʶ:f����Kp\�7�����	�}	�ӝ�%<A�
���~\i�s���tm�(e�.s�Ē�"�U�S>�����S�x/9s�p�ls}����y ��s	�%`&^xl�M���T9l �q#�M{�'�H��L|��F{�<��$�
�=�ׅ�%4���G��t`�l?���X�����>ak�g��M^��
��-v��j� n��DvzI�Jm���z[�����']�y�VQ�+�~�Z?c�ǐC_C�0���b�y�t�x�g-���!1�zOlIL�p1r��.���]�/n����&�-K7U�ֱ���.��h,N�'6Iu�����]�0��qs�]��+!��$c�E5�q7��j[	#�Pm��D��[�'�x������!ɋ���={n��SF�Ԣ���)d�!w{�rʰ<�EgP;3�#o�q�g�ղ�Ǌ�i�/wP_��p��<	v���}Ԑ�+w9Me��Z��Q��	0$���5<vscJ�5%��Q7����v���Fz<$*jIه�\�=����S��esm^<�#\��^벬���&��A��c���=[�e ?���f�Q�GW��f� �����8=�:Z��@v��ky�B;%/f>?\�Q2C���V�Hw�=7��ab�@A�y�\�r�6u��L��dF%h��I��y&��5��-���I$��6@�8�	�ꟳ[��)�e��EpZ��'c{�6��`6�2凗�eJn�n�a,����rei��*3=S-��^�$'�<h��)w��å�l� <(_��=���oo�H�xN
ҹ;9�N�w�El#�$�����Ȭ��t�4'�-�~�q���">Q�Z9(�������r�����|�废-e���R������Ĕn�9&Z^=��$�/�"�1�|��D^C2y��1�<f_jU̸6K-yEHN��c<�u�z�MXK0��_Ft�  ���gk�e�:L*�J7��`��c�)�ݒ�˃A|�����h������ J����p?����Љ���D��,��S�3o��)=��4�2+ͫL��-Ir�8�R���W�i�c ���$��k��.ACG<Zė_MVH���>l��4�"e	7B�qu�BK����1� �ѣ�X��t��6�:O7�g6�Y�7�ݴĞ|2��n��ҧ`y.A�e|�Pr��Ńil]��D��-��8�� '`��;]��v�U�z��X���-���ڧ����sjB��:����ۈ��"�Z)�-щN�Ą-2��5�A�N8�J-�����=l@$���|$8@����m,�2綔d��6ۦ�)�sJme�7�%��(�l�k��E�P�Q��%мAZw�������[�x�L���'�E�� 9 ,���}<&Ȭ�o��AB,]{L���={���.��dc��E¥.X��i�m �'�'�fzfBۄ��SZ�������Vn���FM��pK,�g)A����O?\6�'嶃����W]gP	@���������5ᆙ�����@r�X@�n��8R�cy�!?����%�1~�&�Iۮ��[O��u�K��5�ёy�O��K�"7����"@�Ҳ�T̓�ӸH0��Iexd��v�%����a�F��X&����[;.$���ƜR�&{���w���ة.�b5�)s��I޸C�&3�su�yA�Q��%�qy���6�x � �͇lt9[�ɂ��1eM\D�j����r9N-`cap�2�ԶŴ�vJE��)O��cB��MA�;Ke����gR0JN�r�Pp`H �(K��y���j��Q��w�R�5/���)�4.-�X6�y$�:.�\ݕ�8���l��e�/˯WZ�e��]��p��_� �o�!,�U�:��S��hȄ8�t���������B����:����z�?4���:��+���C{�#(^Vk������H m�W���sŨ,�zʄ�hl��/���d_�h�3�]9���p4�k�g��[�j��J^�j��7�3�*���=������HJ���_~ϓ�3����v�ٌ;q�ze!��.?ޢ�l��0�Ѧ��X�/KB$m���n$��n7Hen�v.�ϙv>eJa��=��$¾��Kں��R*w��J��X�,YC�����=F�̹S�����Yd���J�iq(u��J��g]o�+R�vK#�Jw�p�r�RAٕ�Փr3�Vc��J)��ZCW�a��f��$~����`�jːlS[
?�\�*�=��U��|c��r1���&y��Rv�B����:֏s^n�z�?y�I�N�I�J._�"�d��p
����C��?��m�%�K+�6<$�i�w����1��=���O���M��*Ff�:�����-�|�g�S=pٟ�����ʩOj63��'�g��>�Oу���\fb@tds�S'R�V����S'�P�����,'!�;�Ij��X�i�n�(�1�Nw
ghvu���XƓ`M�K�q�r����S令�2    �@�L�����e�f�X���'�̈��ЖN����8":������"�v59����J�\I�w�S�I��d�t=�0G��V��$��LO\؁��G��Z�XJ���$���o/�ԭ\��@�?�B�#�uАH)x��c�2��P�	p�U�K�gP��}ʯNS��s� ��B�r΄s�b&?�4*`>û�V���Ů����'�Z<ŷB �S��~I��ir�0O���짩(���-u*�(\m0�7EDJ�f�r�M�@����疧]:��]丽�<��'c�jO����){s[K��uQX���7
��45i~�o3�T�ёKgU�TK��F�i�V#�h��c4�~z�ҧKS�J{J�>
#WCPW*�X�[����)<��eUX��$��TP%_}�φN@ùuB�c���H`<4�m1���P/U�i<=���Lavwh�2 L&t�AM�F2�g�o�k��4J�9�)���fR��*�v!n��n�0������L��Zs�<WZ�����TZ]y� �xg��擦�^&��7[A&����i)zO��\]Pt�h%Ɋ�v�z��?H�~�_rT'&p�\;���k���X'$Z|�< ���&�� J��䛇�����Fn� 1QJ�f�f!��F]� �J`D���u�B#��D
+\��ʣ=�I�AȐ�
7˻ƫ����AI���I@AbX�/�!�=�Al�"o���iM�L�7q��Ֆ�	8G�<�6i9��W=SXW����G��yND����ּ�,�<��6Τн�~�U�vsC�	��������O�m����M��X�~����:n[�e��T����Ը	G�z~��%���H�KU� :�{S�Z N �˕Ӟ�4U�����:�S꺱�FM�Fz��$˧"�C����Dzln:W],��R�DE��i�|{=�<��dFSI��N�R6RzU'��[	�����z9��]Ώ��2͊y\�j�f�F�p���*��;>��lz�����g��)��N	��L&˦�Ձ���g�D�� rRH�'��T���N���hL��r�_�|n`q�!� E�K$��w��⺱��T��T8E��ZR��6��b6��g�"r�8v��O�g�Ŷe�mH�Ҏ�oK�V���	���$wn��=K�㑎����Љ��8��>�J���$Vn�7�<����V1����|�S>�>9�����d��³c�$;ŏ1<�,uI�Ȝ&5�G��h��r�E�M!f���kp�{�����6f�9'���<ף8A5<�ֻO�gg�X,SS@ ~�k5�ͯq�r�f
A��) ʝ>���U���r/)��e�k�̞�����麖����d(F\�W���O^�<��g��TLw�~X (#]���	�~�r0�UZ�S�*�Q�	���o�W��Bf2�W�%��my����K�|����L
�Ӥ������$&P�K�ӛ�[��O�>�d�#��Z�ǝ𐓓v=uĕ���8��3������H��i�������?@c�U(n��q}8���ޏ1�-�dwm��Jt���@�L�J[����	��Hw��iH�n"�K5��x�i6n��|�-W�̔�y�7��:J���,}�(>�Ry�X����Ķgo��� s��g�o�+�Z�P�Z�Bk	#�;Xw+Z�	E����p���>�x�T�iִ�Iĉ�F�K�?�S��y�.&	��P̪��(�Mg��L{���M���Lg�r&��0��6͋��s��\&����ɫ���/�Ɠ�0� =�;�7�Y��c��G�k��$#q����黫b?��Q<�3e�w�կ}e�Zb�)o;�f�p�����L��m-e�E�OW��Þ���H4� u]}E	��r�&�t.Lb~��E�8'�U<��$��yz�O9��I��_�Y���8�ߧh�M����Ø�r�m_�s�fy�����P�XHu�����^I��%N?+���[�н2�bk��:�S�y
:�2򚯴fi����<�)cn ��8�\�D��i|y��H��(�/eU�V-���r�/io�r�:�̋=ؑ�D1�s;��/�˛l�f��[��t¾�D^�cW�� �e�`vl�7���U��!G֬HF���F��Eo*�s1��j��Z$��s��+�>�%>�<ċ�I�7ZE��?4�a��)�폞����b�k|2B��D�m1��iW�<�&�3V��B���H���a��Hn�������e��?�徐,x��؟�]i �,�-̹#A�!'k�-�@�F����Fgc�J�iM�h�yؔ�o'b�ڡ�VP]pb��69yD:����]=~���:����NH�5��yF�F�jc[��)�ZԵtYsq_^t�T~{R��H��t@�˚4�z��G��~_�tZS( ��-
�u�r&;TP��Q�s�k�p&�)7OM���ah�We��y�Ŧ*�xf�;�+o��#���M�xxRJ���4���
y�����h)��|�x *wKU���l����;�шd��	�sBD���z�J�%���O��H�/U[�E�~ʩ��\W4�K��)R�K��B����Nxͱ[�h��{3�����?��9�<��\]h��~��Ӵ9���Q��U�]�\��7E�Q@��L('��dq������f�@[O���]�d/v"�V���S-@�|y�9�������Gw�>v�-���tL5m��ec��je2t]���P�)��i+e��!�=eQ�#E2�{7O�4��N�P�Y��`�M&��(X����LIo7N�׾vg����&�E>!��Ҟ�|�<akJ܄���O[�{Q\>D��T�����8���?��IW���
�e��5�$Br-�D�Δ$�E��ئPgL�3�	���<`��"-�������R_7��3�Mc�x=,p�5���Q��o*&��ɹ'E�K��9g����:��~2���h��=�	w�i��R�i��oz���4��Y�:�'g�;�d3E��#��q�U�n�X7&.�*g�|�(V�"�7�=��X��)�#�2u�(���p���ۏ�k��!�#a�����^:����#\�Mщ�ߨl���B�'��K-�9Ӑ ��'��J}��O����C�Fp�KR޹	e�g�D��	����rM��"A%��Vse���t�>(�������)�n�,is:g�<r�p�Ů�:JO�#�K�naJ�6����5�3k����^`w3�|��9��h���Q�߉ce���lbv^&F��RC<O��/gp�ʉ�B�W!6Qm�`�1"��{-�ӱAR��S7�G�_�
oJۙ�-���x�H�$��֡_�d-A���&QI�.l�Ώ"�^�����r�s�@36���ғ�x)��siri�l�K����e�H�q)e�`$�+�+y�V_˹���8�Qq�g#��`Lyj:�-Ip���C�Sy�h'� D|+
~yd�K�)9�]�5Kp�T�v����dMej<��i:�W��x�8h��O��k&i� 5�b9�r���9,���R����:Lq��ٱc�ԨSur����#)\R�ҩ�˞��</�9���]Z�g���te�OxoŰ]h�$T���n�DW�����f��NeT�`�`{�gya~	�K9H+Lp@~�������nEv�o��l�-�d6�?�u��y�@��%D�MoY��]7�U���J&�=QOY��tL]���e�H���z�(�4#�T�br���=��;�;8�?�慤m�:�$�xg��>��M��K�$x����~��ٜ*�5j�+�Lk��A��*;0�҆���5(Ю��������'�@���vQ��N���.u?nzTfɟ�����л+�6�k�aM�������0ve��H�.i�:�1g)ˬ�,�p��r-�h��er5����>@n���,�ӱ��x�SӤ�F	��G*�����<�f�0L�m܍���3od�p�Ǭy�}����<�Y��?��|n��������1OXO}�۵?��o�.�����X����ldA��'޼�2�^N{M�e]6J2��|jS���    �;����g�[N���P�T�I+���U�@�y���|?�ü�$�[�O�
.��>x
�J��B�;+K����u�k�nT:+�'(�-a�V��F������o3�\�+�d��|��W&q�2�2e%��,�^� ��	�/���`x	����1�I��R/�i��H�yl_q����>Z줒o*���I�jŧy�st��@9M3�v��O%�o���EY9?�cTR6~���~���F>�W�lW���h�+��Y�z[�{J����f%�-��\�D�^��M�F>!��{���XB���<�[���F`_H�2o|Ñ'�:�Gp�S��Q��$/�}�_�%���$�B�j��:k�K��]oM<(�'�%��">�	�����R8ѿ`�d�ҭ%o�U9�y-[�h�J�Xs$,�T�ׄ{X�wq�	>M%���r�CJ�d|5{"o��[��,��9������%�������۵�.�F+y!Kx���XH�%���Vf�d|q�_��,&�/�����;O@�f	�������zL���v��2��!%��`�A5�Ob4�h��ǙB!cZ�ݲ!���&����lֱ�,Z���Y�ұ&L�>F�h�B�ja��]����1k6�Olȓ;٬�s�u�)��E�'o���u�r���&\�9���5T������b݁3�Ԯ�y��{0.~���dw��;'^�ȹ,є'3A��r��$���ɟ��b��n�z��P�	B%���N�[fN�C����e}���D�N|��ݣ�U���*v��达�w�Sd��V�n�� B��;��
Ã���<�����g�!�/}���xy���ps+�g"�9@�B(�ȉ�S�EJ�����9{�?J����&_��M�w3�ݨ�0��[�55��`=�s/�ci9��3h-ާ�,�vw�w��$��LZ��:8\'!�A��Ê9!s��8���M�Y����1��D����g��%JL�E�{YrM�|I*>fU�
񵺺ˏ��ph*�| -܄̑�#]�3pӂU��fR؟];�o�c;��aM�4��:3�j���3Һ^��6��o�;�ד�ϙ�əG�uz_Jn@y9��ib�ԧ$�G3K�Rm�k(�c�"IN�|_������<������;iR8~^'MwW�a�}N�����5�MR%��Ⱥ�dTrt��8%�U��ƍ�ߔm3amG�E��������� �4�$�����rg�=�Y�m?���1��o�>KT�����S�M����.2������uf��~~��3�ޠ������ �y�|I���Kr�NOo�$�����#��c¿��òWb����@͌�Z`5�>4Jgԙ��	*�����=�))�bZ���d������2Ӂ��R ;�=T,ʾ�̨.�����H���<��ݚJ�51�C��F&K�'h�{�4'��u�
��d��Б�����	� U�_�
p|꽙���~�exZ��Š�[�i��tR��~B���>���t&�y�$Ct0����,U�r.a6��ڷ"ơ��:����q��$�/g),M�-����c.�nT,;+i��;/��K%�r��k��;�rLm��~��.�˻ ��x��G��߻�	�J�=��S��n9�[1� n�桬�p�JoZ��1Gs�ƙ���j$Z�D�~R�v-�=i#Q7���dZR��[N{�vj�DP��|��5΋K�K�c!`GIc#4������	�gH�wq��C��f&`�ұ���Po-�!��f��D./��B�:o���^l��"����']���+qb�s�hP/됩�ݖ�����@��Rs�p������Dλ����[�Rr��������E����S!��T��O���o��O���
K��*��m�2�/�����v �0=U�j�=Y��b��?L8�w4IX7\J�I�YfEk��,8��I�z%kxX�<k~��3��),�;G�A!}a�z'ևd��Xp��P���P�6��-k�{7炥���^���~�>`�M^0�:�Q����t��o�,
Q�i�n}���`��@H�?�)5�AN$����*gǼ>8Q:`�$�s�A3����3�_��Iԃ��2�o�� �N|:w��"|u�,;����Y�LN(Z]N����VHO�@�C&g;M23B8��ҶNȞ�D����<�c-���bj���[���s�P+;w��cc��C��&����M��r��-����m�t��L�+z�y�[��+�� ��!W�R�Dg�Mҵ�=�_������<VT���:1�\TkE�䚼op8琝��ZI�&�Rx5`2C��p��KϮ8�&�:EEl�\� ����\��o�mc�������ζ&�p ��b�42�'׿d��;i�rAؾްY�J��\`���.��ީ�S2L�!��;�in�W9{[��U�Oעl����l4 ��ԟ[!G�<�T�D�n_����G�Fg�2Ⴜ�$`-�����Z��3�ˑk9�<��6�}.Ȭ5 {��8QM�GM�Gg�
]�G�������j]�X���
l�7��A�n�`��|Ah�鏆w�I[�	 ��gjgNqk�H ,�m�j�!�q������p�e_ ��w�Ip��w�nbvo�zo��`2'ތ4'����)� �����M�֫��Qn ,h�"�X88��*��tN�,�/!Sx
 s)��W�T�WHl�o�r���z����'���e|�R>?��ۂ��b�Y+���T�ɡn���uҝ�tۋ@r*s5�)^Z`��r�Ss�A��_��k6R�T F��3�����ؗ�ki6N�!��$�|�L�~J��A#i��n>��ה�M+�ѥ0�`�P�����^8�$7���N�)�ތ3F~耾���Z���I�tE��	�h�|ҟ,�fNǘ)-�F{��g��� r�3L|nK����
b}GMS�T]���G�(�^4�g)�"�����U-��{�G�{h$-?M$���r����t+�t��re����C�¬�Z�r�bLk��5��Y>4z<Ɂ��y'dA���v���+'�s.sc�T�{
���/�X�G޲ P�;Tz�IjӀ�r�B��""�NqW�H-���
���İ �R��ZϽ��ޞ�2�)@�^:hW�P�"����'�4�G�Τ�����W>X��}�Qp���4�|6�<���N�M�������f�Y� �K�H#
�*�I�g�cJ��>b_��f����̪#?�-~f/�;�{�4U E���z�0�/k�&8� ݼ�{�����pF4_�Au+����,�_J�������H�s�-'�,ש��x��8��4O����͎ ��qμЖd3a���_�Kխ��T��O���Y���S6�$G>$BO�ϴ�|��d���oE��:��j��?C_j�_��<�=��L�z�5,ir�%du&���$�R�}�1�&�Y=|���
�����ICYU׎�!x~��n���X��٨���X�jۿq���/�zϛ��g]�i'd�s̥KM��(��)Qv����B�Hl�bL��2>	`��JG� �~��9-yJ��S;�/���%ë����,�\��1����=?�ͱ5��gF�ː���7��>�Wnc[؜tmy��6�I_#9���7m$��IZ����+�ts�f���B�����^��A�@�w������Bn�?!֖lk�2oh<��(3�zϺ�1�O�m��*�HX�����iN�/���P�\ˬq���1)���W�4�Ly��<�����>����vFr0z)�	\%��o�e��O�LH�Ėn���HG'�e��dʹMy��������詥�V��^�����<��W&]��<��ZI]�,�U�Ղ5�F7uɥMe���& .������d�j"�SP,rT�_ Zj�ш�>�����޹g��̕�����?Գ���`~��)�q>��[?o��<�����b⿪����0q�\Sݵ���}SȦ'� �թ�6��o���D��+	���ҫ��̅�L�-l��RH���ؾ��Z�	��    W#��4i���U���������dI뽈Nbj@Ӝ�����Y����	"{pz�~\��� �pf#Wa\^�Eko����W��ˌ��u���v ��T�c������M�1q2���-tm�lɗ^0��Y��pٝ���� �*OV����!��� �[+H<3���������3oWo=�ᗼ��X�Ԯ��J,�b?�`J,)�R�OO3o� sv�d`��o}Ї���A�91��!�kx3iN�bsq�ͩ�-q���{�J�7�[��<�(�<F��N����go���m����̀J�ԅ�����I��HO����5Eɷ^��X�'�o�C�]�<-��J��<���y3����M85-������B��>>����P�Ƨ�.5�����4���O�Gl*��N�ڲ	���l6�T7U�'<�aA��	w�p�Ρ�&����"�qC��Y�	��`(loS��uR��8_ֶ�7o��n
��,/#���2`E������S��r!^IɘP��mL���/7&|���u����bK��A�?�r��E�f�o���c9���d�c�5�7O�2f3���H\}�����t7F��6�M�H�78(�<���|~��X��琝+�<9ti�.	O4����O/���cںt���*���FG���%WCG���e7N�|��\I�IV���_R�)��*ǫL�^���94�F�~� �g��!*�f��Fp�'I)�]z���������WW�&~�wxޚ���d�{��:1��[���9w���}�j�$�d��3�np���3�u���*@��6�җ�B�-E�?j����fL�mUE�/�V�������6�L������$'���ӷ��9�|J[���f�:4�N����l�=�J�{�dB���aP�)�v���ac�qҴ�S��3R:�'���:���ѥ���S
>U<��c�ep�c��6�iп�VN��k
@�RkY/)���Us$��p�U�u�5�<l~�5k摊�E���;��P�ZL��O��Pr��g;8�(R��J�%�����m8N��%ћ}++�e�,�	%��H��g����I�W���;�0��[�Z�W]+�%�r�ɿ̬d@&�_;C�f!�(�	��r9��%����>��\�		<�K�G�N�H�I�����)��;�$�v��d��0:G�bsR/?=m�ŭ7�nL���Tqv��ZK�n+ﶗ{�@}��2�/�`��b=o[����r��N����d9;�o�>,Ė3��8�����f��&����z!�<���O��2m���k�5��KX���D�lE��2�x�}%�E��mI�ͩLZ��F����S��,��J}����k�����`4DA[�����޿��q9�t�<q�b���|���� Qr�G
��NW�MO���0�t���5��D#ĺ�-W��Nh���t9��vү�=�Y?��4I��,hk�`8"= ���[F9N����2�d�i
J�dIvJ���n�1�ex�i N�IOʫ���,�m���W�:��sFP�O�9}�I�p��7��w-��oX���*ؔ$6��k����!�U�4�N���Ol���������~f�M�2�W�CK��G~L�H" 	i��-q/�~�c�q�9���O�츧|���*�k�P.�*�����x�S2�k��ٽ����/�+�<��n\��{�Uc�N;�T�}+�0�֮9]��qy�Ho\����D��.%�KCw���v��^�l�T2m��2���w��]x0�S��r��ea~�/:پ��:aV�;	0-�B$�V&��-�������~Ri����ld�Ԫ�	��VDmsĿ����W�kWb<*���{hZR���I�N(aڙ����{�%Cֹ�z������rȅY�m��x�����)�M,?wyi�3t*���^kcO9��S��I����y�h�EyO[JL���W�/&C)������?�CF���Pم���+�~s���N.��+p�,�xA�D��2��ꓞ�M�E�u�\����w"���)U�7��TJ8]�i�4Նu���\�����j�D�z,�e����B`'�`tk��>C���ͅ{�SJk_��"}��isj��Hq~�Zi��
?�(yjw)_��ܖM�}�o���h4�-�<�/����Du8I.�izR�m0W��:%i$������Z�7��(��F�	D��7�Fn*kU��L��1��$l�g�MI`5����$}f�4\
��뮻č�\�sް���ޗ����	_�0���W/	�=��!m�Z���*e���Ƨ��Md���9���ʫ�r��S���6�NK�"���5�������
K�q�уa��la7�?�e�UT.,�^�x``�nt���'z�/�4�8�a��1�\�Ѷ��C�]N���	`���~ �YV����tYT$��ԄCԢ��_��zQ�K��&��#��"�<^���@G%7�RJ�D˝ɇ
h��u��,��Ja^ڼ�/��r��IE�S6��m߰�ː%�3���ĈY�'��,O�|��+�ή�_Jpx�R,�F�#���돤ݞR<�p,�	��anՊAn�^ă�g���Ÿ&�#7uB�]KEy+e����4?���1M��}���?PT��G'a�D��<��y^���v��cE4�8d�y塒�B�����ifܔ�l ��L:ǠY7�K��Ib�>�lv�ݭ���քr-�"���y���V��M՞��z�Iȃu���MH�((@h4>���")�F�VT4�~vM��Y�"@�&��i�6�����h'��`�XY��,�����\�V��I=u�����a�,F+)}4�e�[�k����
-�Q
		�P'�����d��s:sb.��/(�mA��`��[�<����r#�A�O)�;�1Q��e[�;Nz9��t�LŠT��ӈW[	�U�R��f3t��B�x|F�Uɪ�L6D�H��Vf�0	����|�z��A�j����z���M�4Ds{�6�AL]�0� k�W�/v��݉�U�(�{oy�w�^�+0Q���~��$^�Rg��&=�^e;<��_d'��ƕ> ӓ��,c��ԭ�A�H$K�\~N�FX��9O�x�V�|O�1�w���T�6�%�<�"s�C<w��5��KX�
t��;o�1k�i���m;͟t�I�J�� �@Y^f<7��"�dvmhd,�tV��[(��NpV���A:]F�*�6�%2�ޠ*��Qg/�TC���ۙ���3�?�\�Rs1-h���)�����On�d(�����`4���'����!���1WK�9}.�뉯�>̈��z��V9a��S9�;a�zc�����^�o��<�*��'I͟t�b�J����W�r�I߷�k=��v�i��F���1LJ)������A��s��������&���5�:S�YE!٩���R��7�c=�`�p)9��D)o?g�]�?Bϝ�a�Q����JZr�:;���p-�zQ���(c�Y|����h����lt'�Q��	�湌�f����B&eE�#�/f�7�_��V�F���t �$=��N�J�C#DW,,����J��$�KMF��9�Ia4���u\�;8��dk\L��l:�K�'&�zOm>��&�<|O
�����R�s�t�����؁�Z-�t�v���.;+�˧/H��V����N:�2��G���FƼ$>MJ��B��i�R��
^uZ�-o)����=L����2f�x����ͫ��e	� �������Р3/�b�.����ܪ�6�#�Ȱ5%�/i6����C|�+_��P�̙�s��)a�4����G��k�s�(�)��K�P�8�!��,�X�\�� �ߑ�y��$w��zS���@�<�����4p�a3��#���y� Z%�A�ڪڥm�{�;r
�m�����a=���{�P^8���ߟ�A�jR�C��:ԣл��W���gv�7�I���s�����k�8�3�DN�i�8�ٙ���I���������G�t�参T�Xk`WL������	=��d��rL
�t��    P=�|�h�0��R�g]�|��p�;�tA�^驯�6(��t�u�ٓFP,HgբcNl~@~f�	��y+=����JwF`T���L�������WF-iŏ�{�;=>�T����<܉JTԄ�\W�S��y�����^�V||%A�Wѭ�N�%^��(��t�N]>�8i.�#��OI'��V]��{;����0��籼�+����X]��������b$��kT)��:�hz}Jsl��u����y��0��g1��>�A'�����p�'Q�y�cbFȞ�c�.�L�.���s�n�ɔ��a��~ُxBn 4U��$ �!z�Ը]��	\o�\͆)��/���9�Cg����^Dl���uפ॥�_��
�!3��YL�Կ�sR�XBO5���	h�nAt'#������~I)8=PĽ����>Ӑ�޸ίi4Ov��U��)qr"��h{��)	��fR,]7��s�~u=@��Ϋ�:5��rJ�Q�j��
m}=����
�Ck��K�x{�Jߐ�oLs
�ɰs ��	U��$�Y��ךE�śHFLv��ݡ�GaC�����h3�8�|7�����K�b��!��b����Ԋ��Ay#�����j%w_�^K��2*�@��IiA�e�Ku��'X�w��yG�47M,%�O�� ����`����)�<-���W�l( �H�Xt8��$�zy��Q�{��z��lŭi�Û�w>.b��<����fj�J�F&z�S/���%7^�D�n���L�<��|����J~Hu�rrO�v��c�M��v���L"�T+�_+�z�,z�c��P�I��g �~��ܐa�F���DQ:�Ä4�2M�7:�!=t���8*5H���uɁ�g����px'^m�u�� -�]��3	g�:���y�|R��)��Ha�Xf�LU<��z@?{�Ě<ؗ�Ǭ��>/:�9�y�J�d�p��or%�!Al?�N$�� �B���7P�T�*?gn~�h�vk٤N����쬯����j�(�&Um�J��p�(])������;��o�-&������>�wzy���{�d/B;2��ZF�m9�3�˾,q>����C��Q�2�[��x,aK_�.�J��k˳���r Ĝ~����C�*�2/���@�0�E]&���.�٘R^�g@Z01*1�=���?�r�"�L���9���z��JTc��T�.u����>�|���3�C1)�� ���m���#�C����%�<��9��Q7�?DMa�dč��0�>n_i����7�ڊ*Þ$�N�L��D��Ggư��k��;��WːN1	g�����R2�P�	%)��r
۸��^����"�j�c�z~?7v�r$]$�$��K~u�g&����+�k�Y��=q+]G/;�����ǆ����.�N�CH�|��?����Ҙ�a�1(�����k�v�C�$sS��>̱;KY �&���~�q]����n�A�{��+/�5e�uTÆʍQހ��F��
��g�R.h/�U� }	�i�RRmm�[�inI��.}�0��`�'o�
iأo��Ta��fk�u�xY;h��)�����d��ᬭKR�^m�,:���S�ũw�ɝ�������pV���]}B�x�(/�k�Ͻ�:Ĉ~�f��c6;!���
��%N�!<��We�/��D���D�hR�1�"��y�lm��9�c��W�]�) S�w��T;/k���V"'�_BU�F.��܅sg*؀
�s�+�_��2����_����So��:0��ܥ'�X�]���z��z�uX�] 	��K�������T_ЫCwឞ<�㉢��'P���)������(}�zE\�.��[	=m�Y�P0�lwR�R��PH��'3;�����et����A#��L��o�x<��R�mI��
D��l����Ѩq�Ux�>BǀV��J�|Z��x��1`!�H��?7���ܯ��h&�^蒋t�I�ja���H��N[xׄ��U�L���B8�.}Rw>���֔?q.=����V�4��r I����ʙC�K	|Ic4��#I�,R9�"Nn�<IΗ�)��̦�2���4)[�����K�>��tJ��^I�� �'�.߹҉�&� Rܢ�<D5�)k��a��a�\Y����L�- ��l�0�|؞.e��tU�����b��W^f%/@z�`z��"`��^)�_�2ZJ'��@'�Ҡ_!S)��(=�D��J�������oE�;paM�Y�C<�����Y��Rc�_�ע�ӵa�d���#��8�W��_�J}��K'\�:��G�~�}N�5��5�٧J�X��لpG6�B���8�co�+#u:��*)��N��>�I�5��H�$�I��.!��R�W�RL���>��M�ڽ��@{O��i�-(k3g�cf�L9�F��%|����U�x�v˟�s�TF)$t�~@&�P�,^$���'+.���]	LKw<�I����)�Or�Es�l ��QT�H��=�����I�﹭��(�W����UD�8�@��֘��Y.Ύ����}������e$L�x�aSs5��{5C��]��D胕���E�����r�$��n?,�{ʮ7�nJ?l�8�_�sy{�P~Y�;���p���D�qkJޗ��Fet[nF�_9�%��&�ŵ/i��BR�Y�*=�I� }��߅�6��]�c��͵��e��]���=ik�{��\KD1��8Jר��!W��� �i�{�g�K) ��
Z��ƙ�H�o�[�:^#H���{x��Sm[���D�� �����3�����6�����`f�ך$��ۀ�&\�Ęc�8�Y��;����L��N�w:�Z�d��p2�v��a�a�3y��R k/9����%�R���(�.��}���5>x������{yd�� xNz�M*��T5,�P�lݝ"8��IY�^�n3��iz�1l�<��+D��^:rs��mBI���B���|�ST쿽>x)��l:�mO�����xZ;~��ە`��\I;�ް�^s�l}B��Yח�颫�@�
�6�Nb.����-:-0�������5�8�<���ڂ$�`VuuZ������*��u(�OPC����*����N���')��JOF5��+�[�����E���I�-4�(��?}k�f ���)�e���og��b������%)����ڝ���֧n���O�X�����L[>�`l�O����D��7�`�(_��-�YƧ�N'̤B���֝#�z%�A�7!�*+�Ҋ��^9�:����P��Lq��ۍ˟�ғ)^R(xޙ��Z�h@�����|�d~�k%j�/*X���4����.U#A@�t�<���_�55��vze;%|P'h,�oi�!�9$���r� ��,e(����=��7' ]��h��t�I������( ��OY������jeMƐ[2�&]�
7�#LZ�12�S�=�E������i�r17�<]ՉI�'4��O���iK�����$jv�Z;U]�A,��!�MO�=G+��Y͙W׶�JF6�9��?��)��}�|3��k�=�Y'ᵔ�g������r�j6F7b�Iw�R@�,���;p���7�C��4�j�`�ִ��7�6�}_�u�|��6�ͺ?���I���%n�8������v���S�a�y��7�X�w�(	8.��wd=��Jb��v݉�Fdۚ�`}��JŸѰ2� S���6���#j�1����
߂]�LT1��]�T3i.�y�#Cb깕߷􆈒C�"$U�he�Ч�A���OaS���k@�s��<�5o�����2+�>�7�?��%�꯼�����,�-6� ������x��^�����X�*/W��RU�.�C�4F�rlr�ؓ��N�����h�J�#��O�F�\���K���}zr��i#+g�VJaX�ܱr@�K��1��#�r������w��zLKAݐ�Iw��eH����<;�lQ�èdy7��NZ�瑇�͓�>���� �r���J�W���a�ʝ�$����a�ϼ�N��EE�%P|15ɚ�3�4=�    ��ly�[�F&{&ǖ�-W�e97����n���/�F�����7':ᅰ�XT��'q\9j���u�˘U���m�(����(XKa�4ؾi9���۩4$��c��R�2���2mˌ�Iy�����cM_�xLp��U2�OJ����f�ѣx�x���Z��.c��J�z_��� iW	>q3H�M8I�����D��7�Oϛ���Y!�O8i�B��x7pX�}`�N�\�z-A��0����ꖜt���s��jM�}TBӏ��.)�4��M�iF:�TJ�'��K޲#�Y��)��8��
/�7�D�#6��I<�	�Hs�ܛ-���~w 7pԲ�L��xHH��5sO�7s
v���iB���S�D��Q�!G��t�6��������E��{�����P�����W%ZS�	��������ΰ������z���V}�Yi��I}Ქ{f���LŌI�`��P�&0v�dU���zZ��R��⼃��Ӣ;�#�4p,�@}��8f�l-&l>�(�Ƨ�"���4I�x_.K�V6�۟5h�,t2�F��O c`V��v*���r?Ɣ��D7,��6%�s@���-�q�/�������S	�k�#	�¤��n9�	�(�	�7���/R���+��ao�89Ak��]���ji�!S����x:J��i���7F熯e���2	t.�`�'�׳��4һ��O�Oi	��8���a� �RU=|z���׊�ƾ����Q����V"�L��--�7�5&�����	�۷�Ҵ�I�/lAJ Vu4��C�Gy�ԓy���T(�X�QK��?�2?�ܽ�1}.���wA�Wt�V�0�iB�J��[Z�Öd�g�b�z���G	S$q ���]̒�T�\g��^޾ e�V-�����S��3��4t)�Ʋ쬡n�����g�6Ԟ�61J��?Ğ:$M�Sro3ц�|��M"!���f2�^J�\�R�)ɱ.y�\���`@:��7��XԞ_��6�������y���Q|�V���y_�rI�n,��ώ³�*�g-��D�m�Հ1ӪS�>S)�A	d��T�� 4h�egI��M��μ ��`!9�.q�7�>
vs>��Zʭ�)����j�-�}E�fu�J��|�D�f���x�$���P�w�e��s5N����y�U;�󿈙�S5��敟���=x�=�K1hg��|�8�����ua鑔q��.���N�%6c⥫~V5���^#�-Bq���6����S���v�D�;��-�I�:ǉ��E]��}���,�ۻ�f#d ~�A��O�4,��"<Mv����Q���#E<��d�9�"}-סܘ=�"S�����x��S�ӣ�Gl}]:R	����y��Uh��P��5�+��mّ���9��\����:��B���Ud�/�o�Ea-��F��k���Z��;�q�b}i� h�Tٿ��j���_ʮ��tf���neP� PI#�s�j����x��� \i:'t'??<��	�~�Gu��V驙�nV�::����$7��y�S(��R=�^��H���ݐ����.R�W�>�izMl)��[��e�w~SaڸE�?7�����[7%M��/a���?�9I@IU7W�����Υ�@���cf_�gd߿�=�������Y�19����lO�̫ ����ykk���� �3�<�N�B�����u�����9�Y��Ke��5i�?�P~�Q�9�3���B�~�Np�_ZӋ��p��w�'�s�~X�t��)����;$R�a��@��_S^�X���\�N%�C?�`��`�Ij����A)����d%WFu��m3�l��ba����	�$Y���Ko�.x�}�lam|l��7_�ؙ�w�Tn$�kpgz�`9�PhO5�()˞����;�׸��s	#<)�o��a��S

�v�iƭ�g����g�&�IF7+�6p#N@t@���Q${4,8e����>�
H�l�Z]�b�u`I�&��@�a�]�j:��=K�bA��鳣�nڝ���fG���.W�������|0zS��!����>��F��Ĥu^l%��K�Sa���|f�+y���f���%���B+�@����ZП��O�p
C���a,ow�Zi2��L`�*G�����&���<�0?�Ӿ|"�SJ��B:`Þ'��IL|�d��\�mǂ�+��wK�����Pao�O�R>��K�>rf=�I���4��B6��r�G �rީ$?�,D�/��`��kI�=�J�.D�*y�8Li�6<�yI���;.�U%��=p1��Ie䁵Ʊ��_J�)l�R�*��dKiP�g�k�?f�)�n"iD�W:z��9w��<��=�/3Q:���Q>(0'e���4PUK��NzW��]��D�/X�&�s�p���$=Pt<�q�=�:j45�e&>-��z���jcH��'��_Id)}��ہ���<��l�-�*K[v+��	J�s(�	a$!t�=V0����b��]�,�[�sJK��D ~��������^Kak9�fR����ʌ���4���~��'���0������ �;@��DB%r�L�1���9&��^������O��*�$ޒ8����4&��(����f�M*yQ�NR�&D'�=;�"��-7��#[I��͹g聰H�Zۊt����PT��:�	l���U�) �6n�t���;�www�q	^>V^�������}�u8�*76��;���`)��vx�P#�mR�
��[�|��-s��k���ܨ&b�)����O�Z�*���᫁E�6�I�;)��)�毮�E��X��|x��ʡ��[����t���i���{��̰��R��d�9t��K.��r�>%P�V�D	�i�=1V�ụ̆��+k-�d��x>��xRy/1ǌ�:�0��<?I��6�� ϲ%K������n�vs.-('!��5{���kt�iƲc
O��GXJ���L�K84iЕ��\T��̈́&צ�M$�^UxB�
�d^r�7c��Ms�Q�F�5mCOߐ⎽�JRn��U�9��?�6c���Bo=L�)�|ki��'���_���w��/�a�d�������|���I��}6&�49�S粎MF�a)?Ysݤͅ��f��b|�WD��֖h_X����U$S�mṚݤX�6���'v�	G�\g,����{!ÒO�����%�v��6�/�v�
-�=m�D�kaa3�2�W��qe�lh��N4��Kfl*6�v��?T��~����(��>��cr����j^k�eO�]���+��5W�d��pX������(�=8�`�v/�OTeN�����	��C`dT}
��4��21�@�h?��֫�)he�U���nW����X��;���ؤ�@p��M�a��#��E�`%����~�� �Ys]��ϕ��N&�͚�P�Fk#TK�%�����'bt-g'��D�������r�e C0.}w�̊�[
[~r:0�����>4��؈I�J�~BIr�N^�Dz�Jc���j�h�u����o���s�|6��#7(e؛8*���~\����$W�C]�RU��MN>��7�����r�� �D��� 3׺!�y����V>��,4R��8�sGU�D�:'E)�T�]aκ�>��k�,�(n�����.���,��WOc@��B5M��L����#���]�E�t���*��g����B���u7�4�Ff�t��#��=�A��i�a���_�� ���!HP���Z�$}Ȱ���U8�fB���ܖ��')��^4�RФ�H+�`!�ܖ�����h��
���0^�O�Av=��jt8S)�v	�I�_�0A�+����u2��y8�e��" %Ǿ_(�3�u��hr�O������(�� p��2�-��;=Fb-�S�F�4�_�㐌��)�M����M0i�37U<�[��c����lO9�3�E¥�K������	L�=~����j~�\0U!=/�y���Un��(��G�o��T*ϵ3�$��w��
�G
jyJJ$��y�����cI���FZ�Eަ��*�b���s H���߽0��D�����ǆ�~�_�h�6��    H%�5z��P��J;�������I�TrVD��l�!W��54O5|@��f�{yޛ��Fp�t�P����[�{�[ݛ�)|�ύEy�4g�p��n��Ϧ���Iw����$���<洛��7�����N!}�����)S4*)9/�,I1�d�КQ��L3=j����:��Zq�K�թ^�x��t���ޚ��B�ϕW��َ���h��g뺷fuA�k�$�O��(a�ԏ�N�%gw>�2�O�I{����{+��5ݰ�@e�.ͣ��T��<���f�Kl`p'��`���;P�s.���n{X��2NS|`5��'�qUC�&n�e����75�;���R@;���ȋ��q���e�d��K��N�{wN���Qb55/ah�B� ��Y�>]���My��p?O��R@s�F����g�{�����^3{���9�~��Yd%\�tڝ.t*����u.��<���Dߔ�9�9Jy�Iۼ��-�w|�RJ1��`1<ּ�<�<'�G�_���<=�`r����K����`�%,�6D���S��P.\4��ߏ�s^O��B��g����'b�	�ɘˍ��	�6��r�aK�W4��a&}��tOS�}}�Nd�z y�kV$%���>�(��Z����pѽ'���q�e����d�^l�4�'���v]}�?U}�U��Jw���޽�O��>1�����k��я&�.%��pC;�־��)�9`��I�{(Y�D�1�z�_��J��z��q����2�ɟ�`�F����u��y�~�������9�c�A�sC�>/����J�3�O>��u����J��Yc^��T�	lԅ�P���I�4_r%���^�����tA~N��${��8�O��ŷ��!&��N�:p�M[��mdp�6�
/�������'�x}��L]��R˼�)��L��(�Pes>>qw-�MTS<1gP��㳅�F���TӚ#�;m�P���*;C�#�h�Sʵ=�x���0�b�@���B�AZA���f7�c ���A�H�)io�Ϗ�S��Ǔp#WC3��<��z��>_��3�e�����zDP@dS��ȻQ��'@0;��� ��J��	�r� �Psf�R�~�?��G��"+7F�7�C��#_�D]��͹�%�&�Ni�c�O{e�Y2���MmB�2T�fWK_�~df6�+Zo8�KӞS�.���H��s�D�����M��O�7m�[ޖ�i�W��x�ֶ��&�g(���c*��D�d�}�ukQc�Z�X�e�a�)�w��I?�=ȻJ�����7���"Ƭ�j@�V�ۊ��\D�s�y��3/ݽϕ�HR��M��t
�j�7��F�{\�]j�<\�ey!����t��>Rg��c�s�����ò}wm��k�>'*��$�Ѱ�&*��"Ej�t�୺S��T�h
�}�A��75#��+���[�u�8����D��0ס/q��r�	R5VSb��`��}ir��HܥC��#�Q_��D߆n;�EywX[�G&J0�����)����-�Uv;̼��;��d�}-��ʏ4
	p��f�Bi�S98X�
���A^�����E+���	2*a-��1% W�cZ��7�6=f%���&N`ffg���@>�ؿx�i��yu�j��1%�q��)z������sA���F����Ƭ��^�R�k!G��N��"�8�oS���Z� Ve�oE57_ȗ���6\{�	}�
���e�#�[J�������x��l�����V�U�S���IW��|چ��dm���􁇞���g��S�C���J�Mm`v�_��{��Jk�?��Ͽ��j�����h�.5 ����;��lw�~�>�e6�d�Vd��;��͜���S��Ce�ٱ��ReN�V:�B�Ψ��U�29���8���XKU�Tr��Ȝ���\�T
/#$�y�dkuf�Vމ�ً���j�RbK�k�<3�����(����>��T�i��V����?& ��i-ͥ�T����ɗ�͜��q-_�n����}�j�"'Y����L����O���g��NO?}Զ�Ǝ� ��Ts4�4�Q�� ��u���E�7/m���H'X*窑?½8L�F��&�����{&N���F�'B�5�ea�q��-��XS^K��"���� �;[�L���P��׹�$�@�����B���^���]��]��栚jee�43����s��rf����ƦO]xwS���Wĺ��>�?,��`��7�=��!keN��7Rh�a+J̹�쌟��x�"=�p�	���{f�r�T@۱Ѿ��K.�=��KX���҃�f���؋52��c�/�rfk2�A&0���iN�o��b��輽?��,�#�*�dq*AT�F�ʥ�����KE�tf
�<�kKy�����ȯ����+e�ӏ�4�#`�3HfH��(�G�'e_ӆ�,�@�����a��fE���I�ؘ��/\���&�<i��,Ir�ɷ������u��e:�1Ȥ�s�@��p�%�[����Rv��Љ�|�{�3@����k�A�;�kE�6�xL�R|��,%<=�Fڹ5��&�։��g�ZI�p:�Ϫ���p�z^�{���*"dӹ=B��M/��T⪅Q���C�l��Jif˷�~:�\�ҩ'D�P8�S�8V�;�~��N��ҙ�h$�B?�!уV��n
�����t������)�	��+,�;�Y���+�F��#��qC�b<y�0�z�t�e��c�k�s�����o�)���̛ߪج�0�-x���$����nk�1,�-T4��A^�y�Tz��m�aC�l~��&[Z�j�z"�B��Ɣd[$�FAL�&�s-����A��g
��SfN�6��y���HE�P3 9��[��c�'N��vw��t�ORVm�y�Ao��;�t�(	�2�`$ړ��u��+�Z,�:�i]�O�J%nms1Jiy�s۫�����dv����wR�k��]7�XO�1t4tZ�l[�U�{*�Gw��B������9�3m×/�a�p�#%0V�J+��v�|�k���+m����k���8oIo]��J ���.oFQ\	��v\4�l����S&q'6��.d5���X$��ǣ%�P�ڒW�<��y2�2�ޮ2�I&��_ ��*�R�|�:yG�;���O�s�;<"�F)��W�����r&#�ľ<���t��J;�?�@Ĳ����ݭ�;d�0r;�=�����!&r��F�\`�ʾ/�"�9i#��{=`�7�n:�f	A�i�A���EB��3�)��+C�D"U�5_�
�f�@Z*�3����TKnu�i��'f��Y��oZ~lxɁ��v���jJv�˖	���J� ��MʧTk���'���ڃ[e֤I�lě�g"Sx�14��|��J{:!ޕK���$HX~:7:���3���IS�czt�����^iȒ<M�.�%ǔD�-�&H�A4�s]Ѩ�pi���DX����;�1���$�y���c��kZ�����v�5��Q�0Rh�<kd��.9��>����[�d(�|�4�U9%�� Zt$(�`Ӫ��wXZ��e��$B�r�*�U�C�<�E�4G{�J����
�	k?����*�U�o�����"���y��S�&r�0o���{����~�\,�ƻ#Y���0�����7L�|�b��z@���i�

�J�z[�Y<_��7�Q�z��:�ܽ�"S�}�u�sq�)A#y����B+0'�i[��hOq��?OܫZ��]K�{$`̚�t�E��9��4�7;����͹;�B+�Eq���~[��=s	%�����<�qۛ�SФ�Q���7M���6�y?Xӣ<s3��Y�(���,p�J�ތ��i�_:����o�'������� (���Լ�"O@��Xx��B�S��c@?d�#�u\�(�B5�6�ha--�&	��;J��D-uSYӌ�U��>0��l�� �?�E�,�R���O->��ǅ��貤
�xQ>ǁ/���B�p��Զ[�1oh��?c�����T (�Ō�@{r<�_=K3�ɝb&d2Z~�5V�d;��F[d����8�i�qE��Ƅ    ��
E�DI�m�.���rg�B�b�&M��r�y}�3�7z&��ko�8$�椒�fզ�����pHO����ﳯ�)E��z��y0� ��?�o�"���(��$6��Zg�3qч��5S H�?��Wn�g�9_8�|!XI�(���>��IKo�-�u��pLH^#�bN�On���HV��S7$��V���MK�PS�]`��}e+��\k��M�p�}������CT���j�{UM�_x�ɡG�N��u�3M�*�1��gP������}�t4]�]U�Gi(���r�p
�A7�}����C#~��K�|u��[y�Z-�_Q  ��1Yhň�/~�x�}O��Q61K���zam'��~���ǖ*��g˺������-�]�-�d���������p�s�Ƀ<<g�G�X��!m���`9�n���6��|g��T&y��w:�������6i�M�{���(2��8r޵W�^t{F�����H,�/i����K��=���ۦj��4�o�4�����+(���b���L(��߅�� �G��J���N�HAT��֒�^�����5¾�%ٻ3�-���2ڠ��;�]R��n�*C��d*�7�A���L'��:���	�>g8�(�s���x�!� Xf�.
������F׉s��δ�g�)q7�. ��-<u���ѭ��`zC_��5��Wg����3N;@L�h�3��u�c7�O�j�^ �c�N]��9Y����ݐ�I5�qc� 
���s�	ݸ�!3�'=�_����z�v"9?�S�d��'�ƛ��CDǷ{{����\����U�ԓ.��a<ӭ�M��ͬ|aݍ��������y��6�Uϥŝk��8T�v}Ѧyr Ӧۺ5����9��E	�-����s��6��F0�J��l�ts��RQ��E=O��mz��p?%Hu����9MI����ܞʚSt�TW�o�ג��c�U\��/���s�I6c�.�i�n"���)�a�[pMfc�b�$���b
�:��"�?�Á:.S�@�b�tlO'�{���9^��M�;�_2tڃ��9���xiԟ����h��gΔ�4)}9lRɟ����҂t���f1�ϒ����Y����sF8G�!��]�
9�v[�iŊa��j/�49�:�nĞ�TS��Z��9����Я�h�`c<��sm�H���ś�l�>R�^��ϭ�5�m)��i�lqը�Z�����udF�Z����htΖ���*��N�)���"���&I��!�h�DdS�Mi���Zry�2�L0��X��!	��!$Yoc�T�Z��p��f3F��Ђn#%(]�ؗE�[j[Jl*;�?�w8qo�g�$1j-��Η�nLM������dٸ�[	F*�����?��KrI� p^��/��ydu��~gd��	�(�cw�mH���K�y��Ox�d��*�
�H�-���8��B��2֌2rR'I<�VD ���^A)���i'j�/^����b� ��;�VR�Q��.���=F�r�Swwu'��9�Q� ��U�/��H��tQ#�v�L�:'�����Gu��l}*@�V�H�?Q�3�Q��[�N��.l0SaV绌���C��s�P���܉�K�(�)���=�^c�_6T~D�+-�Mm�����7`[�5�XhUz٨�'��˩g^_��)�`g�q�H�.o��:� EQHY��ۛ�.�Κ�����`��wY>�G�6�n�u��Ih�p7:��ܑIG����2f�+�d/���@~.�Sc@��t��O��V��	ɍ۝�y7�9����m50�����o�pN�bS�S`D��f�Ʉ��KB���^���m�iW�����Yf�h�)�UX^�y�o������]<������c|L�bZc�}@I��/��/}���L��fr�gR[��0�sOy�ԕ��ݫ'�U��\��1̱�K��s��sQn�l�� 9>m���
���R�̸񡫡&WV3M�Jr}�z�|���)����_���r{��W�W[K!5q�m@h�^���,�\�ܠ\_���'��s�)a��ҫLO ���O Y�1R��Kq8݉83��Jjo��{� p狲�J�b�����SYi%��'COX��Q󉜪�c7v���,'�;U��ˍ���7����5s
��~)�M��'%n���P����p��O��i���r3�	>Sr�㖳̲�m���O�`0���&�bMVJS�k4��y��R�d�a`e�ѓ�8qhKSh&�\�f�c&�$�'Ǥ^ow������]9(�p(��y���Z�|���D^ô$���،���QVs���%���4%�%5�JPI0�ك� X���.��Y�V�>7yFƏ��53��X�S��&E[B�c
�B�Q�K��[�����x	�/��ޥo]˺}�R� �n��1贿5��9�KRC�d:w*�I	%#Oc�/u.R���l�z�%�|����T2�_�TL\^�W�j��+`��
��c�;Y������O�gL��=R;$<��ġ.m�t`m�����i�C!�j�Z8g�Xl�n���.NC���g�����,���6�&�8������:�t�6T�D��r�j����lIS]�l���=_'/���R�R}��X+ߐgʦ��y��Q�V,L�ck,���%��gI1h}��q������~�Yu���H'!'x�)7�0`�=����~P"f���Ts�	%�����gٮ,>t"�U�ߓE�|��J� ���R���L������'�|;�0:I�)�X ����ѝ�����Y����(�p���9����ϵNRK�YORt��c���qT�����J�2�w�cGڼk\Bn���KS�$�jC�պ`-�h��
���c������,��
3�\��#�&@Ŵr�� _j@�l�b0�s��>M��-��si�,��Đ��5�5`O��/��br��İd)����Zv|�5[�ؤe�3�(��Ty �_�^z,��l*��J�^��f���<7�3�7��-���]�� U�j%kX������9P�D�$��"4��׏��:)�"]6���Bo��h3C���D��i�Gܜgz��N��#��1���li��ɝ9�=��A=/�TJ�\�t�/"�M�7�$�?��lR!~gnZs���b=rU����H�b�F�����.+�Cc-��yI7��r{�M�b�wV��=���uZ��N�E	��[y���`�ap�
N�-J�J���g��)�`K�3�B�i5�q�WI�a�x�C�=�-�4�珛�r</�b�p8e���~�R��<���?WB�����Oj����+7���qx݉�j�䉻r*��$�|�R_?S)��-�����H<F�)D�J�kn��4��G�)[�a��ͱ�]��	ж"��җ�఼Ze��|�I?���ӈh�^�sW~G�h@RD^�`��4���YL���dJ���y�֥i�ȥ��K�>�f:# ܱ�%)��g���3�}T��O�]����p�`�C�2k�JipU�4���O!C�"EW�\��e��Y.�������2���8�p��I�t0�(s���6W�쇧0�J��(8a�Ֆr�g�~'�X �G��wJ�x�e�=)���r�Q�K�R)�����)%���0�ͯ���d8�V1D+֕�2r��7����x&ԩ�i�v(��;(�Lf����[I\�^~���#�G��k��ML3�^�o�8'N�%���N	y'�	¼��V�~��q���$j�^6k)�N��A~;���ve7����gQ|C\���{n�.Y �?�q�[:__L�vrq	�y�i��~�g�L�e�?���� i����ՇA�r��&���}kKd"|����+H�o+1�R�`�M�	#�r�'�ԅ��L����Ĭ`�mo�b"^�h2�k�t�vIZ��}�m�J듓��T��I��7j�u[�՗��3�1?'*z9�餐�·�eX�Kѧ�n�f=��#ggm���&�>�B�`�����黠�8휆��OD�Y-΀�������ܓ�*Վ    !�mbx���r[�w��
���.��Z�����;�/�����\4D��0TH�0�,�@C�|�Q��3=��q:z�Zw�Ȝ����(�MC�M]��ʠč��Ђ��ֹO��*�>G!�� ӊ��U^�cO�9K(���3L27�������o�>��l�H�K����#fn�Y霶i�Be��崛%	�V9��$^.[�d��&*m7���ⵕ}�(N��mć( 1��h´�w3S���� '3Nב�7�SM�Ht]h��=��,˖�����n��;�8�켂t%����4Y�a��'��&t����c@��1��p!="����|j=[�i��PB��&E����w��L,i#O�e��Q=��5�-�q7��$��nɜ��L*Q��`���[f9y���_�!�h�F?�)s�_�Q��G����S(9��w0�K;;�y���)r<������͛.��3+�t&j�us���o�{f������)UM�j	�'O�Vԛ�Ĳ�AMs[/����)�%��Ky������R$�.V�9-�^+O������T���C��_�� $I� ���1�fS|���9tw�E�[r(|�0��N�a�4@��YB���v��<�|��B.�én�����!���<4)�Sn���tQJÊ((����a��c !�&���[�ڷ�2]D��}�F���#^OмW�H�,��T�r�g��Ӳ()�,%�a�4�&0�һj�F5I%ܐ�-�YFCK�V��+�c��k=8���[z�lC����p%�L���*9k���ߢ�x��#�41U�b�jff�s!���lnZ���eL��1Zb!�6ju�����B%k�Z,�FJ���:8r<�ḌL����!ga.R	-�1�J3=�E��؀-����?��qX�<�lTN�Sr�DŇ|m�n	���`�4-���k(�nɘ �`�!�'�nD���� ~ԧ���S��u����a��jp%���62S&��S{;k<�����*��t٩�s���4Mjy�N7�ׂ7j_���M8��<7Zu[�zJ�뭕
ӽ�ݽ�f�U� ���,�E������J���o#�
���&K���HM��j���{�%�xʯ�3����1H�����A���Uq�ć���<���b�w}
�?HC@�P!l� ,�Lo9Q�'�~? f1�٧�Ty!o�����1t�l��� ���T>���z�^#	�a[+�Ė�{���6c�c]X��3J[O�h����xR�n+�ڎ�H��~O
����p1ɝ��pB�yao��ޗf�� M��B&]����.�Q�>d*�7�w������ښ� �s�r	W;����[���|" ��,eQ��K�k,�$匜��0�2n0C�����u�v��Q�����:;�	��~�#)z���W�o�[1~&��䘜�� ��0H��P\ݓ �4�'��UC�!�m Cpi��%��!�L�����B���K�;�n/�TQ@��E��bR"�\����u��X�S��eaO�/�oOD�R�y�9�)Ŕ��#��i�L����f��)���l5�<�)�N�B����A_�����s/��=Ӎ���/�u�[2q���yy�<��J�9�6R��^����T��rQ���NQb����V��a��W̤�N���NJp� ����_S,=�H��oՐJ	�[���R�>�6��\��2?�պ��M�E��lf�P����?7����@���Y�é"%R��'�d>S姡�B�H���Z#�����N���%t��8�Z�K��xS�_�s�`���L͠�P�8�$I�8/�R��f�a>� 5/5o����g,��]M��|���^��?%E}A������n��.��R�+{"[:C0d�tX5f�z�]J���4iu:J��P�/�ZZ{��jx8���%�z1��B��,���$��U�<�[����ܰ���G�T^i��}[���w��V�h���+�����j�F�9�ڞf=�Ngݙc��AM$x�:P�!��8=74�1�KԠ�n�3�Ι� x��ܒR��p#6�G۵4���}��=�KK[���*]�̮I��-N��v��RWOS��O~M?q�+U� ��!B}Վ�z��ws��WeΆ����A�%�e�P����~q##b��Dk֩\鍇f7� �?���3� 9Ɇ@��M�)5&D:�U+C�k-�I�8p�n�ɽ��������Ҡ����d��x0�rk�g"�T���Y,�P	x��7��� ���=
b^'�B��k[�� ��򦋺b�n3��C�MU���	%�ސ���}�\��U��_UF�h����!����.�����{�����rQ�B^��D�-H���!������|kZ��ur�>|��E_6:E��5�#�Ц�-?��T�ē"鿜g��$��{��W^?`�>��T�̀��Mi3��"Uٲ���x����8�Y�%"l�gZ���F�^h��8�ćp4�����-Kg6��汥��z:mo���:�R91�/`�p���dl`:�2b��}T
u_�_>��=V9A9��6jJw)�������,���Sd���EQݽYSp��i�����ٴ�;�������l�a�����>
#���\ғR4[��K+�(�/�M|�\���Ei�Ow?K��\�W)�l�x��:�X'w\Րw����>��\���	q�<�\�����qlnv/;e?nf�q*��xi��ꤗ�>P�̇�����x�	)����A��fI��l���'��r]0I��4m�#m���/�)�+��i�e�~o|��<���i�`�o=�������b�|v�I�Q�,6�{0�����Y�R�[E��k���Wc����# ������C�k�c�b=S�k��@_ʇf�;Rʣ��R���;�
�ﵛ�(m�^� ?��G�w��.P7/@�"�'^+7*CJ���v��cvnM��I�)=:ҍe�@��wo�JaJ60�'�B:�K1í��<P� �/�s��y3���9HL��I���ƫД��`u2Y/MȌ�%}ڽ�^�ے�O&�D���.�)]�d���`��޻���\�\˴<�C)���Jy2D��wI�O/{�ƺ��en�;�,:�D>	�}�F+�/)c�/	�R�W2��ڶ��4[7�uh����$#��y��$�l� �����x����Y����Ap'8[kl%��ȋ�ٍR�Y*�)��M �OF*ݱ��C�i�J8�̧m�����p��6�b���<�U�!Yy3��%��6h�g�[Q��-)?B�|r�(pvރ�ʆ>�^r�7�6�][��̨Ndk�T%P
4��ix�XPB5$�Q�_K/js]܅�P�r}���.�������$g������o�Q�V��G�C�E5@��!�uZ���<J���j6N�`��Ys��ۙ�@�����eAL[&<;k�@�c�+���F��	}�}9�����cAM]�ݼQ�[�<"��0�I
�i7s]?l�(��2A"�~J��㝬E����W3�Ɂ�Gj@���)�2qv��?l����O�q.UUp ���G��L��bc��~J��"ѝ4"��ΔC�EI�D�b����f��)m��j� �t�ӭ1|����AaQ���^4��D�I�*|6Ȭ⟥��^˷�Ct:�����d�	#�ś%_xλZ���ۗÕ�M�g$�rD��ENS��.���[Z�(!�f��[�B�)��%��f�[�L��'��[�Wi*�жy���	J(��]=��"�|�sܶ�tF���ͮ�;ۨi�RuaԏI!�c�w�(���&������7��� �@^H���u�{�<�;��+��L�+�Z|Nr�3�-��<��h�������OO���J/��,�3�T���S����C~�{�;�HG�[�����]=��ɬdJJz�:\�yS���9��/���c���]X��7[���� =���9���`ЅE����mg1C*����Q9,v�7�%#��`W�R��I����]�B�L|{�8��9��xu�@# �9�VI{׾�ަ/xo3:�����DP*    b��b.�Nx����-��G�h�ܸ
�4ț�"D����mM^Ӂ�U��O|�M��aZ��UԊҚ��|�S�H?-�������P�;MC���0�L����L�J�}���L����Ʀ*+�-�IJ/U�����Y�r���������ܶ��d?����S�D���2�Ib+�{�s�.5&��O���6��ZA��v���ZO���h�,1�� �^h�
�@4il��>4�sII�K�nK���-U0:�F��~1�}�G���ǭ�Z+��\u�T
;�U�+��(?���.�9�߼�TG�0(�%&���>7�݃���3��1!#.�.-�����.���/Y�����3��,������$Ô8ٮ���T�4�����.���yf��ʿ\��P0|k��-��S��eٕ����V���������g�l�����Ɛk=�)e벗��6���nz�䕆�8�{Z��\~2^t9ߟ(��֮�iE���J����~2���C'f;���+dy�Zg-��W��>Oɧ�圔#G�hdl�ً�==I�����V=;��Ӷ�k�oߧPo߮b�r������x&�V�J2��1���"���Ǆ۟�(IM�Q?~�[S�yz�L琤Â&2�j%�I8j�Ѣ����<��Z���y6�D_hٯ��皓?HE�P����9y\yaoKz����R�J}���|n���gZz�\jer�g��q�!�-h�9��.gj�y�G��"
�M�g*��`�-zR._-�
��9�զF�>`���l�֋���j��O�a#�&��Ʈ4�;��m;�E��T�;V�s;�}}EG%�)����ݢ����.-t\^I�9aĜ#��"��([�������%�r�����ǖ��(������[���L��1d�w@�s%=���J�ժ�[���_X!�K�W�Hͻ'v᩟�0�$��FJ��2�)껳�0��1��ĔީZ[._#�@Tr�٪'EJ��cNƔ�H��wE��a�o��$�U��n��i�&)^��V%��hM����S��[>Jޅ�Y
��5;�}b���.�W};��y�a�V!��3@x�)��^9l���g6���9����V���䚋���~��ӫ��ea�c�}Ȃ!RkOw����B���*��I@S�msi� ���JB�ݷ�LW���p~QC�x�c��dR?��mPU���k�v�=�-��b�~��ߓ&,�M�̃�a�w���bs½�CwM����\�.`2$.���Bĕ��m��%.�#}��y"|�La:@�$%��J�Ϝ�!yu�Qs�]-�L�Č<m풖�5y�ІH�V�Vz��D�DpBc��sS�`�';���)���n��N�׎�X
s��'݅1��!s�8�C���wyh�5��7n�F�l�k�=ai���9:���A7�Ge`
w��)eo@�y�Gn*�y��q��¢��������C���]5Vd�A)����Ŕ��}K����:K��,8�? i3�����MK����ZM������R��V�3���$jǳ&�������I�0\�g�	��(�V�W΄N*��TRG��!Hw`(�D�P7��y��7\�B���"��]#�=O��KJ(tC�������p�]bKͷʽzRX<��o�UWV�P(e�S�:F���g�	�d����1����,s�d�>5���%�D���2�6�Lda�(� #ZBS��0�@�7�o&x�3�:��=8p&�%���u�v��5��w	
����b�鎎���vMX9�m�쎧O�����(Ƀx";�mt�9���xIK�.���ʀ����vM���3�o
|&/1�Z0�۶�����m|�AXhN�%���x:�뼥Q��A~���:=�ԕ_y����>�N��hhW��,.�A:s	c>wN��d�_*NJ������)-!�8�0�i�c\\���
�2������Q,�/���冑��i���������e��œT�	�Ң8n�O���L�AH��S�����η������X�5{��Q�J�\��+���4~z^A6Y�^��$��M�%:wp��%E���}�[��<�m�i�۠�h�:+�N�t��y�����M�b/�P64�lS~*�WV紝�{6�S?F#o.6�E�+O�;ga�H�{NQ{�M&q��[�߹R���z��BK��O�E3�<z�����./J���hdu��O��[釸�W=�c�L�����0͉m@���&��!́�Ϥ�V%�X"����:�j�j�!u6b+�EI�����d���zv������Ą��զ蹜�3ͭs>:W���l�7�O8�:���yçQ�cetS}k�>)���8hC<��y�0�J[ہ����>פ����R�R�W�41lbȾ\�"n��
:��H��烑�K����؇�4xU�cv��	�D�d�|ʉ*IҡqsJ�ƃ;O�	��t��P�r�X�W6��h�$mor-�S��O{`��4O�<ќN��X.�p���{$S�)ӿ��y��W����rǷ�)�V~��e�.f�d��D߅��K-�ғ�þ�f��jtn&��ۼ���,F+�Rs� .��S�5�D"0�6�HΛ�S32�)讞�g�����8,�F��M3k��b��d�A�?�Uˤ���:	)�[����~� ,j��������N���N����I�s����S��~JE�×��|�s��:)5�������l绚�� ��[8�y�w��|q��/%[3�ݘ��,n�Q�{�-�/�����O���ߠ��s:1(h�Ĳ�S�T�ߟ�f2��Vr\�4�yZ��������Ź~�𘟸�FX�ϸ3Q�h�!ۍy`�w�t�zm���;O���4�,ͤ���0�����
F'e+��jث��5��e��A��S��Ly�R�N���%�>{9�P���>�*�mɖ���}��$��"h�M@�IgՓ%��0�Y�ؙX�� 9;�'g��[<��d� $��1�J+Y >/>Z7����&���*����$(��g��I�3k���\p�KO�?ʥ>m|j���z8b��B�v�`���U��Cm���)�}MＦ��*�H9�!qo�7��_�[`���oDZQ�[;L�sk\��E���!O~9��Q���9q�YF(�WN�_�@�RARHJXt�w�/��+���}�INx���5�i1�F꩙�ж�I�f��}}$ VsH���"���3%|���4r����%�ժ��#��0��|���it�ȴ�^�x�R����.�K�����+Z�'�iD���t���r����R�z���{��R.U;S�D�\N�� ��9'!s�`9��"}�#�)�xa�μ�۔��-w�܊�e�O��Ma����ovS�s)I#.9�N�io���l}�3A�}*��̺,9}�Qwi������?���˯v6l�!X�?0NwWKoV���Β����3E@7S��j�P♗��״PX(��)L�:�b�8�8=hB�u��a�w�\�g�;8C�&,�	҃��xS�	Kv�;���ҁ�gF:���Wz��,cJq���P�=�O,{n:3 �9H�S�*�ƌs��!&Ae�N}�x�ԉ�9��B�S�e6\���A8��H��۔k��'��*Գ�v��w@��P750�֧�Z���휃9��n���P#?� �`YL\�V�I�U�]v�v�x^)C:�M�]����.�w�o;���""�s�j�d��Io%(�	́!!5_8��v뫈O64_-��6|�sZ�kJ�����h�:y��"J�	�WRoÈ�*��.]:�VK{��q&�
ū9^*���5�*�26m��%j�����ŽJ��1(X٢.0-�b0iy��3�� B9��ryy�Zېsg�Ø.���N�������d�\	�'�>�x�K+�m밄������՝�����b�?7z1�L�M
6�'z�NI�H��r-�X���n��jZ�v�,�y̤ ���NV���eG��y�U�K�E=�n���Z��sy�i�m-����'-��#�ne.�lb�e����{��5XȑJ�C    �9_fM��A4�������=�\���PR�<��۴�V sq�S��E�|F��^#�	=�Q�O7�_.4+���^p�>%�h��Y�>�MOy��!��.�CRN>�y�G�T73*������sJ��"�s��h��B��Vb��$��&�vE�:�ܒ���w�MNǓ&��,��	Ƨ+�S��ݜC�����Q�I-��l`��O�`B�+m�4�d�����/�'�igЖ�@;R/�qV��B�/�o���;ʹN������V	���7=.s2��D�Ҥ��}��{/ͤ�L���9��Ӗ��~���J�3��	.��\	���{�j�@���*?F��l�33>(�Uz�Ôd�����:J�Y��#��r�1��w޹'�e��R��f �Z��pn
��wO�g�\WMI�f��%�y���팤��n��41p��C�������;�|����F�a��srr�7�	"�����B�Y :�h��h?��Nad��N��C��#�n(�iU!m�'�Ƚ9Pn����N�a���G���>H�P�6����CaE�M����%t�q���|E�V�T�y����J&SsY���;��q��I"�>���2�^KŤ�O�y-�M
)?	�/w/ux��\+P0�➈��,�މ����gú���N�I a�#Ƽ�^������#��=*^�_M��6��|9'[�������S���X*9!%���{Q=��A8LkAQ��)��J�(J�B���>�qs��r���ctmb9�_ܺ|�T��#����0~��x�)��eFm_�Y�Gy��S"-`p:��W���SJ��mL[�p��n�މ��;�O�LD�5��15�H+�@�.ח"�I�T"�L�>�M{E�����2�ʑ	��)G츑#�t�̐V�}K���),��S)y�.�����p�3z�Ho휢�$������ W�=�.h���yoG�d6�� HdBbˉ�m/���pX����S����7?,��X0�c�W����$F����X�7���9��.Q#�0E�q���E��dӄd¿�Q�]�)w�7��˹+h�lGt�^Fb�U��q���棱�K7�#L1!��JMj��!�	5��AA��\�Fga�����`�������,�TܩXO����S�ɉ�[Cy P�]�A?�َ�A�`?��j2W��I��Fۉ�i�{V�?x��'�}�`}0'��Z�T��w�+�)ߒ��Z3� ��/���ژ/�2��>N��Q؏���v�r���X��̣��&#>�We�Ѫ�	�5 ���6�k���c�7S�\׋1�����2��F�&�ig�)��5�[~f��=�붥2C:�23H>�N���J|�	�Y��.B�9ם���?���#�i���?_  D�Se�O���e�ճV�払������6t���T��J��#k����,t�Gw��4ll��u���s�,wU5w"���@K��pT���m���̜I*��MT%�0��`,n�$�q��u��H_&֠F�Z��o�&�Tb��(m�rk^�!�� )�n��&v�R���L�ϋ��`"��@q�A?�5���0�{�rG2�Sw͏d������\O�z����ȩ��y�����V��d.���,�z�z|�O��Lےн��a�&���h���)����<҉�>��?��m�L����ş:b�.��//����\��4�N�K�6_����&R��bi�o7�@p�L�2�>!��S���4P���<�v�V��}PP���V��ZaK���0�����'�)��yG�s�МO���^ӎ�J[���r*��K�����Z���#f�)�Ҍ��Ij��v���oiد�	�+77�y*����*Y��`|�'ƕ�4�N�K��n����L6JB��	���K����t��9eBN{����e%���0F�O�i-:B�����X�}M�mЮ��r���!/���C�%�M=�LdKΰ��C�E��0�����m!'4
���ߏ�Bo^1�����Y+0W�ha��R�D-��fk����?LL7J���3+}y�栬��e�|X��`0о��Xoh��� v�G�Ճ�
��@?g�	Cö+JJ9� �9-N�)�I�9_��d�	dѩ��7�1+K�@!ϧScA��� ��Ro"-ě��@:�{�4;`��j0R��#L���Ve:&����3i9��u�l(5U�G����6R
-+��9s�2`�"U�\�K���Ҳ��-4�/k�y7�N�}�k5�A��].��7��4���H�*/�I�r0��|��X��m���Er�3�@����N�A��b�୉N䎷�9�FdB�'�����Q;�-I
��1)!nux�F*�|n�KFe�\��[�1��5%M��9���vl?�}
ZZ��u�A�C�I���.�4��)�R��4��^�Ssn�tt����4���X�{o�\��AK�e�#�K�W��t7����Ѕce�m��q+�K�ǙЄ��.����KL�� v#i~���VR�l��4�+!������*��~�9��x��+�b�1�s$Fec��.�`�k����+��`O	<
�@8�+��������Y��w�K��V��(Ƃ�%�A|� ��f`e�w��w�.��Ԇ$�_�z�'h� #���(bm4gT��Yo�K%4������\(�J�c@��#��<�����&���9�0�bI����Iβ���I\G&w}�T5�	ib>���_j�yȍ�=�$�ɏ�C���|��*�3C��b`[�d���2�K�x��t��K0�p$��uhck��g�qC��K>���K�M��x���R��~�9P����i�X.X�:%l[X�"��)��_��K|ɋ?�%�W>k��
v�Zbp<��A�>�@���:P�����e5�16���q0��(�6������zPRK��:0���I�b��n�!R)Ȏ��a<ؖ��;Ǣ�@��Kuש���=����|e���>>��<�$���6X�>q�q��a=� �x�c�r��������9w�L͌�A5�Ʌw>+K�a�<���F#��:+��i\���E�ꮳ���N�6Y:�T}-�N��J��ً=�*
��-=S�5�*+����]����VV�5�.?=+�4��b�&�����p=ˊ��HBi��\%�=�K6�U#8]Ji���sno���lV�g�ʋ��	e�f������	�%p���"��ў��峦�J՚(l��t���D�/�A��˙��Z�,:O��)�i{����sN�#�a�Y���S�%S��`K�Sz��R��Q�>d��֭3 �^j[UCp"�H�4[���*����]��³o�8��%��w�Z���(����w�\-�
�j���D9�r�̴_�E��2��M}�+�ߝd��=�����Q���K-�[��N��P":/��{U'�h,?t(.��,;�d����L��h�����sB�<��w�wf:|����xT+���@mj���Oݓ'33��٭h���,_�]0=�ǉ\ >2r���������tj�4SQ��Z7<N� �j�i��Co�/ҹ�+j'q.xi>;<~p��F��r��6d���V��o�����$���ߜ@r������D�ڸG�	W�#��]�����Ky�W�6S����߇Y��5�mb����i�
�@@�M�
���xOE�<-�0�6�E[�x�89�GA� �mM��l�jq1�bMܖܜ�3�lk�x;u���6X'.'��	IHj$�J'�� E��:���%uC�fNIr5�>�G�pv�.����i�Su�4�}�x�ݟ
<���y��ӗ��H�{3'ȓ3��"���f��x�<���:]Z.}���I��5�F�@��!}n���SG�8��T��޶����ѓ�`q b�`�Y.��R����jނӚO��F�2��hL�y�R�D�VD�r�G�Ԡ�=9߻��.��G�*Ue#�)��C�z7�Sry��Z-zӜ�����Ϛj�X烟�	    �p���O�p��]��]P8��MDU�4[*���:��K�Ժ�+�ByO�IM%V��c4����=�ˮ01���y@�M��eF�YW�{��E�MA��NK@�T�L�C%4_@�8`سͷ��.�����kB��IT��_
3+��>?�H:�	S/*��6���YA9r���>r����S��5�	�gٞ띑�prH�-��<*Sy�z�_vJ@�9㹋�ѫ��b#!���F�k�
՗�=/!ߜ͔��]٨<{�;/�}j�g��ķ����A�=}D�=�6{A8��'��y�^��U�SXI)�~�ep�'�΄(m�����.&����Խ�[	��b������ʱ+�Rj*�T%t��Ā�$϶SJE����^���W�*�^�j�hB�ӂF��i�o��u�o�x1w�g���z���@d�MҤ��&Ҿ̩gQ|�ǒ/�M#}y���F0��nм���M>>���`j�G��(�oz�|�������R������8:���铤 �ML����Gʉ�Xi6���y�e��y���e�8ʥb�tx[�'��ێL���M�D�1�1աލ�7�sZc�\���,eQ�({yŤ}P���Q�c��,�Q�m͎��
�����K}����6'0�P��j��ZL�S���l6����J�h>�	�� r�/��v-w�������%�\PR'۲�8�IX���=��_��������%�v���w@�LVj̭ra�V'��~b��T
���s�B>2_KQ�J�E�_ ���;�,�CW��
ܐ� ����L���/����5Wt�|A�m{���RJ�^|�ə�sA���{����$��+��{��\Mpd^P~�Қ�I�.?�Ő$������#��ѫE:Ȟ5ܩ�)����,%���5C�2���F�<WRjn�=���Wum9B)Ϸp#9�Gʨ�ߚO�bg�GE5�L"/�9T����7
�	�KM�nS����ϔ}�M���r�QO�%�y�ԺSWa7'���O�ϒ�Cs&�A/��~���+�|?��3fZ���A&�
��|�y�����ǂ`�.}-�M���٥rM�%��ރz+���=�8=N�����]	6Pga��t�)r�ֳ(�qR8��7{���>$���j�"����&�c�3�Yޜ�S��˩(ES�#]��Sg��N���(�<��2�,m�4I�1X�����T�{��"�1�h/�T �5����DN�����d@1J���˚f���W���r�MG��swEL-��9eǑ*`��5��μF�����n>t^Ჿ�Ө�JB��N��+����l'-t���L��@̔,^�N~��0���$ˑU����z��ܹ�[���d`����A{w��j��^���}��i�z���0�2P������\�Z��fO�g^H��ם8��&�rR4�$F��A4�M)2c�����puB�8˓,�x� �c�б��RA�bܐJ?)ƦH��:�~~^���ӹ�RY��OZ�'���?m��bS��ߕW�w����:�>a]Y�y�;��6�<�/��N�D�K�v��OԴa���������|�D!��J9{P'��l�Vz�;̯�����ns��{;����mO?�߶��rLɉɎ�񜩩�@8g�����ZX1�EF��7M�o�^Ie�^95,�$�9O���IU�Zr�`/�:�C��1+�(&r2;���o'pum[
�|��|r���N|��YY��knl�
�M�=�,�/ŝAPJMm3��)~�+�V�*4�A�(eEO�+�~57]n�Y��U�[g�`��y��-=���u���M��N00;o�<R��ٕ^�q�w�g�U��؇�2)J����뻐�?�Y����9OS ��x�1I�C�}mb��"o�1;��$���&B��΍6=;j�
M<Ầޱ�����A�����H
�3)N�pU-�I5�(�n�̡���-��,�����9�E!z��� �wHE%GӳE-�߂4��z�^Y*oOj�>�5�{y��+E��L�G=V7mb({ׁ^�C������(�.\}h"咥E�}9.�c��%��m����2�,���<�j����u����0�d�t�

/��{�/��;(	���MEG&���E޸5M}�ιWPg�d#K�>a�)k�%����$ ��y<P���M�'�l��~)/&&\/���i�цl��|�t�Ԕ��R����莌E�|8��⚳� }[YYc��T���&�-�޿{�)�]�|`���Q�����V*74�AY��� Vx_%� b�hyI-զ����y�h�*�d���v |��!�s��23�T����
b���z3���A\QA��^�KX��Ә~wb-�v9��ۜ�[f�w����\�oN��NV>eǜ����Gd9MdJ���!��i{��x�@�Rp��H�_p�J���Luv��4�&�&ott���?�y^�r�ܴo?t�0�b��-�iD����+�^���m�@���3�/��Ռ�D�)��� �2�F�嘶�m���������P�&ǒ��r��[���ϒB�|�*VRCl�FS8�nk���~�5�u.G�\��,�\�+��Yγ�S����c��a��Pz���zֽ6D�nl�՞l�Y�<I{<�v&���oHS	 ����P]�t5����V�M��V�=i��ᵁ��p�;���lE�I��ʞ�`��+]�H��3���L��_��N����{֜ȗ�H����!�3=�LEr�k뚘���uP~��k5 �kj7̐.��A������M��l�K��z��ɵ��e#�:�؃$/�����|Ů��w�Zl>x:��`����"��<���]�p>����1���H�"�N)�V�'�����Н��4�X*w���W��X��^@�\yt�|���%���r�������lXm�<�r/��R���� �M����ș�kV�r%�eR��dqzj�!���wf�T�=E�3����><5�������l=�]R`�6�j���a��7j /�HN�~�:T�f<����C�yz�Q�U	Ŗ� %Z��4���t	�w�_� R3s���b3�ݣ����䊃24]�Q�M��
&����me�|��*g��x�*��|�{+SMT�t��O�I.]՛�`�;'�}v5�i8k��=�����v�����(�֋�\��r+m��O,�|w���Ie%I�H4I������N�v#ɞ�'H�9 9�t�hݖ�	S�[�W���LH�s��P��'{�$w����@�r5��?B�Z��{)P����V�p#�c4eX����MI��!)4L��V�y8g;o��W6�q�ɛL	�c�Jd3M�Ks�)�u���S��$����K�����3���˸��`R�õ(	ᾙ����y$�_4�A�t�o/������"B�_�5<��b�qqX��R���I��ע��H���_<� �\z�)Z���'%
��Hi����W'�;�ܼ����1��o2=��Aoeݔ0vAfʴn��1��̽��=��<e�ߗ^*�#����`�9M�W�I�Uy�����a�vRt�1;*q���!d렦e�F��6���Y:�	���|�pW���t���$ǈ�ۻr6c�d�9���M^,�hn�ǚr��ogV����F��(M�*L�'!bϟ/�IB��/��,�����f�E*nt�TVSm���O(�*��V ���lQ�XM�D���t��KWe���&;E��X/3�tU���OJ�o��-�����8u_ �/`�����~�����*�T��:|��c]�N���&(	:�{os�WɓZ�-�+�VmŒ�{����z�0N3�"�D�|��$��^��@8w7�J���P�=6��&�+.�$ݐCt�
3�T�ߗ��;n>�N���2��t*�MM{la�<u^mF:2�=x��cO肟i|>��`֧���ï=�d�qt���^��C�:��*r*2�.%��$$ไ�Lpӂ^�N�� ��}(v1U٭�SL�r���^��5��ݹ��-���^^�/w<e�҂ީ5ҠH�WHC�{����P�;)�    +�_{�2��fC��WSIQȞ)��}ι��UN��]�C��B�=�e+O��Bb�"u&	w~Z���ԗ�#4]��`E;��)��Dzǹ��*Y��jb}�Ix�/��ò(����W2G�s����Cq� t�\�Si旡�6p�-���&/o�M���1��^A�E�*��D&j�7�G"=>fz�ۮ�L�ظ��tщ�2���X�+�e�;�@�vt��	����I���IxZˎ;���oGC,���+G��r���g^+�l�%���Ȉ��Xj/w�Ǹ#_���?��>�c{�A���6r�=�t�(��T4!� p��@h����O�1YWsg��fMÐj# K��ө9S�Hy�í5�,�\�60J޴�)�����_:�.s�|I��D��g�?eX��ꋍ$�7��^* E�%=9ۣ
Ԝ�ɝ5ڸl��r1�p��\X�npPU�/���Lգ��%TP�Rq�� ��Ə�F��J�*�)�@A�۱�4~��.�1��DUk����݃�}WZ���,�7+�[��9
Zm�riύ����ri
�R1^�3J9}IS�d�����7:(�˟Nb��� [�,e��>��<P1i�j���XIO�1J0�Jt&�G�R�yt%�._%_�����ēIA3�NCƚ��SP�\��M����YA�CxO9�B��H�����3��$�=َ>��OuR,
��.������1EUDʬd�d�Sw�k%d���iJ��o"��Y���gA��#��)�0�7|}�Ӽ�HN�L���}���yk���]���8����_ڵd�\�s���$c���Z�#V�gz���*H�y�T�˲}�L�6p�oG�E�?R��>t�)�tv�0��h��D���p\�X���o�5Q^FK�"t��9-�~	H�p�H/�f�<}[r��G/%��9��z1�e�H11��M�<��Xz���~�s>�����Xǝ����ȓ�\/�OI秺;a��T�k<���6_����諑 AH@�[y�Z��w�/��K2v^*a��k�[���,
,n�:����<�����\��8�ӣ�`д�6�-~g�i���B�q��5�����Ydo)�lDs9L�s��k��#�4���s�U�~��0�;#�u�5a�܉�"�"���LC���[#��kD ��e��7B�~����~��	�qP:��rB +��2ɻ�<%����wj�G��'��N���Q��F�B,G1"v�|.�j�Q�ɋi8#�K�50�6V�[�~��Ӓ�{2p���G����^���
�
��ѓ�����i�1�������|���-��N�5�5�S�N|f)�dLN�:��g{�H'���W���K�<��
J~�dZ�l
?�
m�m�y]���Apj���<����c��6�3?K���O�ԭO��2��^�S�t&D�%�Ɏ�"�c��sO�3��P0Eɔ)+�_������߶j�35�&�.~��K3�F�!^���j���G�@]���M�%����߉�J�:)�Ƞ(���5�P� �J�o�����+����ъ�<��%;?��vg����=�N���i�}4���bk�-ޗ��m �u!`��s�u8�/*�e��b}S�J�&q=a���]��ʽ(�����`�0�/jX�;��Rj0�5�p����q�kd�2%�=
-��cU�4S�'�WK���N����a*O��L�i"s������Q���}��.�K<���KcW5��;q�>`�����f}=Y�[��e��8�����3���[�فȓM�h����-����eQ���q�p~v�bN>";�W}ڇ��o�=���V<��A�nO���)2�g�����b�~�=�]ۯ�7�a��̟�����&�ۘr@�0Z�:y��XY�����iɟ�?&���i�r�Λy�{���gԺ���cU	���>ㅥ�A�,��������Íc����yomBO p�EX�s8xN$�<�߿\��&P6d�5�������R���U5�Υ�}���ϵSr�<��K ����X́t e.��P��D�l�}�{��1~[Y�<�P=	�ղ/�p�Rf��ה�{��wk]:�8�jw3VL@�Q�*{���R�8z�[�I��M	~�|�;HgS$F�����~O0�;��4���b}~5����]bv˯���^"��y^��P�����琚�QJ��9_Z�:æ�/!��Ӕ3^ܣ���Yy��1)��(�"C'ҫ�A�>�����l���v�ܙ��<�;XC�|��;(���18ӇT�%?�4O-��{�Z�g��b�m�vX�2�<�:{/����;"��1��ˈ��Օؒ^����d�|+tG����j���m��I^i��ͷ���*Dݳ���ʄ���֜�rA���y���f�*��O�P�o~�5���d�����2�1�!R��q��m����4��딟�hH��}ƴӗ�:'��O��*oK�N~�Ly����yT��	�����ޟ�ׄ;����q��7ٹ��	���CJ"b�GI��$2�;2=��4D�Ifi��<�s�q֓�i���O��ɉ�k�1eR�b���?8�q] 1z�',@2�^�;E���0z�h��K� ��a����·`����t�`���h3����7@��wꯟ
�������1��"��X`b�L�x�l퐯���X�]�^,��q4v�t�!8��|��������T��9>"��"����s�����B��5���l��� 7P3H�uGIlo��?3�|����Q�
h%���x)I\����ȕ(b�p�[y�{n^:�+5���8���� �fb�FJ�<eSe����	�&��R�5�X�S>'+�$�������l�6qO]�R(��R����-�)�)i�����&��H	�T�M���=a��|:i�[�Vo�7��������x�dn�/O�ˋ��/N�7|�%pC��T�FJ��/��$�m�As��aQR�V��W���Ѻ
a3-&).������/�%�UJ����R+���@~e����� m}[�W\�Ma��K�����r��\0 � ��HS����k�s�^���B�hI%^��6���}J������ K��O۬x�M}@�{�Gn_p�r噸��і@�� :� �ϒ��k�&�dׯy-���DV[H���g��*��(Վ��3Os�f�]J��;���h8<�\�vPӕ����3����K�T�ף>�i΂�Z�^�Ĥ*G��?�5�X6�Ѕ�-U�NwfA9h�0n�ǹmn��>�[c~��Ո�����A������n��c�u������Y�W����Ԋr)�	M,w��lde�m�2E��,J'b� ۗAƛ���Ԕ�:���2O�e��ʃ�չNI�"*�`�F@ù���j��)�^�V�/k�$%��z�Tx�]E��N<��<�������B����:'"�uJJ%������ƒWK�bD�d�tL�k|Z�%��m�`�iOzE4��GWD�&L�3��Kt�v\�#@+;W��QC�e-A'�MeH���Yui�݅^N���U�s�G���z��d��c�6�Y6��ٔ���j���{���7�	esn���a�ߤ�N}k#���gaX��/-�ʥ�O�V���L5�~y��"�LN8X�°n�H#�	pq~KJi3���354�70l�t�A�
����/?/���{]\,R!��� �������7��J8��i�����L����eL��T핷=
��֨�d�X���5�L���_ڂ}ډj�2_&5�����/p ��L���"� ����h�y[rF0�@���d�N�˖��^�u�O>碗��Kx����gA%Oe�w��.��i��v>Gw7����V�����iMF75�.d��_��#x��&拆�y��7�f�y�)�󾎣|RV���g�q{����9G�F�������y����-�Li��B�i���6�P���dr�����GFT�ٽ��o�ƀ�S�͵��������/b�Z�%p���ji�S+�蠧nX�Kw��ݭ�Lx���
�5�����1\'+)��    ���6TҶ��{D����*1�-�D��;IyoL�J�M�p7�z�Z6�����ӛ&j�+�"�LKi��ݩMG��'<� h�霠y�*��}����W����o;>�<��^��%%��K�����5�?�RǰB�����&oB<.�[�R=,��c��_��ٙ�)�pi_�p�K�u�2���@)9����<|�MKQ!�Z����k1ƹY�&�"X���C�ݴ�v�aP��saM���mIa_�'iv���HDA`�����S�Ke�E##�|ɊyN��\NSNV*��~�ʯM'v�����Q̑�W3�
ڞڜ>ǨY#�`I8�_n-*:|�"U�����Y�Z}��s;�Uh�z��)H�b�
9�W�ܛg9Q��v���R\��h�,�6XB�76���~橮Asq&r��W��K*�-'���l�l��ȃ�wW��>g����`Z�ˮ�9��A�m��r��AC�gF~-Mpj�B|Iy�$Jm�L��1��9Tު��5��Es"���M�'��)M�J����n��c����R��9xL�n����]�=���D�3�:��U#	�~ u�R��]R�H�ۖ֠�7��ä��+$5Dw��ac�=�d��
�N�!~���6�4�QB��j+TRZϛ��<�~�E���uorH,��-ѧ4,��)/X��Z�7��Z�	��$'�MU���PWK�X���SjK( �qP3�i%{����(v�o �LD���V-�7�F�SZ�IWf��	fc�ip�h�:I>�y�����
����5>!�I��*����l$Aה��zՈ�R��r�Fڌ����K�t��Q6��O���tT��?�dFaO�Tg�,"A�%�3'0�s~����\�]&���dH�;K���Р����������N<���0�vLv�k�X;�°o��y�=�>?�pz���F��|�$�T�'��yW�%��T���̙��@�п_�Cw�%v��Mi�&_����KH�4q��%Y��>��[\mI0?���'u��4F{hF����k_3��Ҁ�v����j���/��m$(֛��v�bb�Mm�K�#mB��2�:�Ur�:���$�"�2��/G�xB�B�LUF�4�;m��"w��f��\wܺ��f3iGC��H��*������敝@>�V�@O��5��������lF�qx�i?�$��(�`�6�))2�^M�7���'d�ڄ����*LS/$G%�xnԑ�?5K�u�*����	:/9�8[�=~�?���t�_��FB͙H���я�>��2�����\L���|<J5�(I�ǒ��	��ˤh�9�PҶR�K%c��s9oO��{�b��T�H�tR3������旅=����l�w��廾s����?����(�#��A��/3e���L��Г�P	IeO.�r�MK�`/l;j��r)a���zy1����5�AJ��Zǉ;��t�9�{�jN��K}���Ub�{�l6�ЉI-G/�Nr� 2[<s��V(?��+r���'�|I�V�����Q�N�_������,AI
�B���+�g���ԾIt�G���?�;I>3��A>�	IzLP��O"�)�S�P$a�H���햟��;A�v/\^�Ď��z�I�tA�K#������b��x~b�iWYuV��C%�, o��pe�ms5H"���6(7k����d��N��a��a��4c?��;^�Z@��׍����8狜��s��o=��[���5�S:<��}G���� �@�zB�˂��x����0���xy�m�<*ig�D핡,#X��@e;!��Y99�V3A��!xK��Vx�����]�G�.����l��O��T��9p�����A��L��I�����ZE��gy�e��>�=�G��=|�|?���-�����,��*���t���Xܽ0�����,����_	@?�ٔ���ڵr Ly��a�$@ܱZ�A'ݾ=�<���:^��)v�Po�C��jC<�lyE�
S7Q��j�7'��E�$W��T=I���ԫQ���6��5�8�>5=�X.{�|:N�R�.��|����ጙhӹ��z�윎+��\��;��[�G��<
�N[W�μ�S-/�!UYs/B�AG7�>󴘒�g)zzS���>���Y�XK|m?��󭒲�Y�u�)H�>O����s��q���<sP�>�s�se��<���	m��,��.u���6,��!��=%&i֍Ǹ��I�y���]�K$��,�v�4v��>j�s�d�b��ڍu��/J?�qi%�hO�'O��]�	D╕�+�L�
8|oD�֖*ˊ�޶�$��.x��R�P
�����H�c]�.\��I��E��9�o���&�m��Pԛ�P.�R5�^�z��0�p�
Bk�I^?�(_�e�iIh�` ����$v�U�c$3SM���)A�g��_��L�x���:Q�lx����-�(_:�v�
�Z��çY�V}pT�M���)���h(���N/�H�3����&��]D�~�]�	��L4V�|��b�6�2$lb�	jCK�W��F���Vy�>����ޛz�~��)c6Z��bP��,�j�;���fJ�w����﾿��4�9uy��9 s�A*D<h�r������Ljpf��Z/�����𹻏Cm-�H��+�^��v���kҖlG�丹��=h�N��aA�Qf ӑ>�����ȣ9�o6���x��2��ɇG5K�ɚ��Z�NR�\J����H�K�����#��/KRh� �\&�m���6�8j�R��)N[y�X�P9�w�}S�^U0*�R�ΰ�<�R�]��9�i�X.Z�$���Mv�0ګ�8��ʓI�IT&1:� �X�Fji[���&�#)0O+q�S��:��6��q�-(�P��@�bфґ"oDR7�2�Kw���꿘r��I'�jY�B1�o^b��Ҍ�	��[}PL����t��:SKaE�Y	t-�ڙs��'��jMf+�5���7���Q�K C�?}��f�	�o�o�>���#I�\Ҧ		k��_y����:d��K	��3Y��ρ����r� Sg=�;�����t�X�������V������5���ۂ�o�3DB��1���?<h�+�=m`�_ ��X	�C���u��s��~��e���7���� k{.�&��+/O��.3)��5�1�s�f�`J/�`�ۧ�,��ES@ɳw�(㎛�jJ�FE��Qˉ���թ�&��<}�܌Q��Ei)u��/sj�Nx%^dE�0.��t�f�8xB�_~sN�*sY�LF��:�m�����i�Ga�����ّ�
�[b��r8�	>��z�ÀC�,u���$#�x�E;����;)ye��͚>���ȯ��P(p���n:A4=�^E2K�z�q{��\& �1�q7/��iJ��G��~�.�lQ��jb�*P���[ }j�P��'$����,j��
��@�x��e�U9k���^�<!x�[��;Ah_�C8�t�R�~)��Hb'����� #"}١����ͥr�ӜLZ?z�K�Wl�r$�L�r�Ǔ�rϞ���o��~��l]�lʉ&�v&�"���k9}���T0�m=�'�0�XcN��7�:Ю9��I��:Q2!ŕ��h�̃���3���GJ�#_���XO"'�*U/�R9�J�h�Tʄ����$�&b�.M��Fr;�Kڤ���)z�����G����9h$T'9���I*�.��)��[ {�??�	A�t|r��i#�V����z:��	';X��/���mڨ{�bL�ț8;���& ����ո�6@t[�硔�;5���h�9č��W��;�9?� z�8���Ǟ�����l��"�/���y��f��k�{�փ�=DV{5�4FDU#j	�x��/��d�XR1��z�W��xEɥ������	�>�T�$�-�����@�4���pc���^�$�}�[�'��й�܎�"E2r"H��9~��<���?���0�����G���a�r��S@� �1����Z�gĲ>���� �#����i����= ��Ѳ�f%    ]��RZLG��W�����'����똞D!���df𾹻d+r��ZH��L{	u���rLQ���=f�:�Y����2�!މ������/��H��mcq��������(6�#Xx��R�X�4�,�r�IYg��,c@ub��~��2ua��#7��F@�����D~��ڧ` �E�v��jP� �vt{����@�MӃ>����{��W�6P/�5t�_��{i�'�>�gFX��'�| ��,��8rwlr�g���f	]�� 1�M�nT�c��9x��0�o7�aKrKAj�J`�f�v0?|��rs�Hs>�b�7��AXP7�^�c``D�Mۛ�� R P>�Y�^n� f�sTؽ�2�A7�sOS���z��6������O�9�,�f���=d�=?�i�R��=8*3=�^$�TJ��#77%���˄E�T+�c�4��թ�����5T7��$�Fk��ǖ�M��r�{�V]���iP�3�O�t��/}.���]�+@L��E?��*;&�I�P�S0�n|��6J-����"�W9Jي��ȍkS䗄Ğj�1��K�2��p����amH��_���h&��Ü�o!��5	���&]���lds�d�
��pR���3~:��g1��V�c����V�	��>�f۞��x�ÇN����e[K́�@�����F�&R��3<�ce�s���I]ɗ��E�g���P��+��hr�heyGYj��`r_�2�4֖�jM(�x�+w�:����n�W�Yk"���4M�w��;��D0�c��r]Q�^���s�S�2�R!B�^V V��+r�aV��F��@��5�'�6��h��T>�H����<]Ώ��W/�`�ߤ*nS��|�I4_�[ؽ���ŕv,ql�G�Q�z}��`
��$��X��`���{��8����L�S�{"i���?��G������^����͎2i���)I~li>����#��g(�[sc�s�w��dȏg>;?4#�R�j��O$~l�h���l���x]�R�2��	�~�5(���ĕy�~��C�2򏚉�iH�B��~)���tw.��?���r���D���_����'���?�ͳ�Lڙ��H�M��r] �k=�d�?��Ԯ��!�=��sv�,%rU�� ����M3[r��28�(<��^����N&_ ��c��2"���2q��Jл�� �lگ6���<σ��>�$�ӗ������D]k��\�^�J`=�JĘ)��63�eIս ��ƎN�:�&�D@�}P"�����fn�~1�H�c[2<I�i�o�39���dc�P:q`դ&����m��:*ɟ�aq�|�����Ԭ��R�蹼�ZQ�N�p
Q��\Pɵ9� i��g�T)�՛�l�|��n���k>���-���9	�$(�����M�d�|~��R�����Əs.)���[����$�d�� �P���n�"���	^���j$��kQ��)Y��l�O:��A��L�( �����ƤsI]�&Q�����N�b��!s��M9U�Ck��mSLL��a�������N�	����x����>��v"1��-��\���&���q1=y&�|�T%8>�"��0+�nu������~4��s�cs�K���vq��72:���q���6P���m+�
����r>H��4%�6t鰉�%9P��=́�d
 r,ח����Tf	��]��E�έ#��:@K@��>�Hř���Z大(\a�tv���3���HžbG�N�Ofn;S�|�]J�
��pұ���ڏ��P���k$&@/%�s��"��`��WI���Ϣ$��ߒ��ҝ�'���B�B�8��zj��|W�.�yɌ�k�!R��$���^J�ؙ�����TI����-}g��LjX�ˁ>c1�Q������𭓆[	ĥ�)Sq���r���~�y�9�9��=q�0&)����ջ��C����	h��@�H�nfr���������;�'�RQ��O��"��]���;���i+'ʗN4���y箜��y֔X�
��փY�l_�5�0�#̖�E�� PܑY���_��/����Mr�����*�=����r�t+�j�!h.��DԴĜOE ���������ד5��`yq��);/��׹� ��k1[��U�RV���tTǝ��r{�:g�^}&ݲ��Z+J%sb�j-E&�*�*c�RԂ\�qGr	R-��]Զ��̓t$���"���%��\���hl��w���>	�]�J[]H���P=��w�j$���1�+�1[D�ob����p?�B�9������G�E=�J~����J��b���c
�2
�X�x�C�8K�v	%��)�fN1��6�i��ݸ��tOd����HZ�պ��2����fa�����<ߌ��w����+gZ=�ME�����B����DV03�4,`��c7��J��Ai�Ө��5q�{ON��y+�F�,v1����Ht�fHs;g��O+�ݢ���;T��> {z���li��lV\��-��[X.��HzĢ��-�g�m���7��(��z۠�e�@~
���������6�vz/!o@�F^e"��2%]Y"哣�h�rU����R�Is:=��z9$B���0���C�T�^��*]ZAn/�(�;�4 ���9a�q� ���*��04@�UE�g[����QB�uIU�ga��0��Z<��"���x9ȉ�*XM<j~�W,���fz��~�1q����˙O�#9_+���4g��j��+U�%Ԏ��#ۓ�c�8�%̹I���H*)MѬ�}}h�G&����J��@�4�)�2���Q��Ѵ&�/�ԍn�j:�T�ҿ6�yR$��x��%ѕ\^�9��K^V<����ߛz't�!]��?�D��P
��lHd��}6�_z�^���ϗ�˳?~��Z,��t} ������NX�_~�������z��8R3�Վ�����Y7�y�XH'�\�,�����O������2�H��RK��KI�\�\MM�_S\�����$J)Ќ����{+���.�b��v䷢�'n�p���`��-��\�Ą���#}���v`9�<~�7���e�BN�ާ�?�OgSt��$<+��q�Ce!�D�����' ��+��l�v��������;P�_�C<]C33R��3�5gO:�4���l�,���&N�y#N�T���͒���F@�'�.�v���j���r{,Z�d(z��\�R2A��)�ǜ:�ʧ�Yn�+iz�;m�z&�c�ҧ�j��Ϭ�٭)�{K��&�y�'�R���̡�U�u����5z���So�[���9�<���)����*�܃\�|=x�dk��\�e�FK�1(��騰F����g �Oo���OjĂ
�?C$6�t�Y@����A��ʒ떅3�<����I����V��T���j��ϋ�j��K�ШI��0,�0�T�:$? ��ߖ�Q������첕JG����>C��ͬ�w�M�>s�>$���F��t���'!�Z�I���w`��t��mJ�JA��y,�¥-_v?(m��fҿqBz«�{l��E�
ZU�}Mz:����3ݙ8�r�Y�M�]2>��%e#��GҼ��<���h���w�޶�� ��W��c*�7M�i@H�Q�}�f��Sּk�*ZR ���M+��R���300;��y�7r;i�sM+���Vy�S�, �B7�x��a|2��U��.k1����<�$5��mM��ai��%,��n���Aw�82{c����!���<�Z��>Ȝ�Aj���7I�$%N�ڭ-%�)�dO^������5��)�{b3���oN�fA]�dw�D�a+{iݺ�+��^PhV�G�|�釠+����8�~������p83�,q�H��M~��u�冼�N}m@�ʹ�ot�j����_��zY��g~ �|��uIY�}*���eٳ��J�_T�7�=u>	0m<MP#��L�R�~jaJe�BP�}č)K
g��f�6�o�%�K�dMo+��MT����+bⓠ�	����'Z,    %�]BMw9m��΍�Q��Jޯv�1��m6&#S��f׌X`o��p��<��B�I6���ΐ�z���%��r7]O0�l_��	�)ߕ�.�!q��E��.��Lb����yM�R�4�w��CUڞw}��򑛹٢
���Ͼ'����1d�g�6��Jk�Q�KG�(
����Y^�b��IM��%�o#�=8?�ٓ.��<�f´6~�=�[�G>>�ќ����N��|�L6���I�$�dc�mt�WO�)��R��_�ц$�/�
{��v�R>
If^z/u�d�b�#���y������2I�i�E;M��I�;T�TO�r�$?�"�k���s����rΝ���I�L�������DҚ����T��H��@vM�Ｗر��sYm�p0�XhE��l���G�*(�[>`^���=�.nS"��"���Ku=���U��=˘`;m�`�RQ&eÜ٩��Ь��L��l�_�6��'�&�6U� �R �O�s$�!�����@5�@��$���d8����������W��A]����O�Vt�2@��I�I��.L��P5��L�2l�S��:�t������<��3=(��w�آA*S��x�{}ػLP�#�Ռ�������$��~�aj����?ѽ�:U�|��%�2`�������W~[g�Ք��V�v�����
��R�P99��myZ�=�B�1�+U�7���r���PdO�<�@i�d�>�t�i�"xq1K��*�����|�*�a�.���p;���T	R�� X�|f���f�R���F�77���4Ǒ��(���a�obxJW����� )p˛Ѩ��ңS9���7���K$K�����rʍ۲L`�p�j��� ��g"S�Ϟ�=����9=�K�頦F�nn?c.?�Gr+���!�;�P��=I�<�4��=�J�-�����i��I#jm$,�[�#շ:=�P��4ȼ�x�{��+?� �����٪Sx ����lΝ��IÉM�aN��Lx�c�k�y��eY6�����t|������.Ϟ3��W4�o�':<�x5���u�������sn�%u�d����h��R%b<�U�T�8�Gt{�E�?F/H���:���E$��a=>PN.�S��iJ�Yx�䦍������a#�
��@N;�s��1�#��퐥�����'.8��|�}�M��������Q����{@�xS�WϦ����T��jW���j�;�>ob�eP���6�;;����l,�
E���}Я�g��S�� >�Y�ѠW��a�o��I&�Lչ*W�����`�4�޼=H9���I�x.�a�L$E�1ؐ�����,����!}�,Nt�2����6��)�7=uMap&����b�*� �ҕ��!��+Ow����LK����:dY(ޑ�J?�J_"<s.�V>�|Hd�X�����E1��V�7ף�du�Nş�QM�r�:͡~ w�B"���&B�ᇑ��-���^mTK�\�ߖP��wN�8^���z�j�le����{��LLو�5�v�Ӓ��͠
5�J�=�rt��M@Go�����dv�G�_�a)M�Ϟ%�����g�(ɭ�v)�}W����>'(�F8� Y�~~�2�Ǧ]�Yӓ4��k��i08��c'��|̮8pk�/
�W�w����H#�k�E$#";>&(I�`ʃ��Ƅ�Z�NĢ{7����һ�p�'�܇��g^Z��.4��vI��m��y���_��{9���P����-��˲�%K �<��T?l: �Wp	�-����!]Soy{�+^�[i��M|1o(,@��2��B�ǜ����C��2(~�Qk��oI[���{��(*5����u3%��J��t�*���ָJgޖ/G���yܛ����ʲ��ہ��Qs�T@7����N���'¥�"Mj�D�=��ݥ��&�>��E�r�j�s ��	}�$�2γ����ӤۃX��^���Ms�@��&��tH��`ڼ���^KZ�$"hJH��RK$!���^p���s*��Y&� /׳K��R�%�5���S9��R5�f�_�rLL���a��T�ީ��3#�3e���<��Uo���e������x#��<r��	Y��i�h����eo�g�C_�a���P3��w/yck!�����q&��>қn0ݨ+�#�������$?�uhz-A �m9�7}�9���)O�u�&�L�����ڰʛ�*y~$���u�j���qU��7��BZ��zoh���DYK~*�ऱ�Lt^H�YH��R<�:��ۿ�xF�9O��r=k����ny�s>|S
E�C$>�:��]zߵVXiQ祥������L��kH��G}S�����(�u�
���R��E���b���-�c0؞��Ӟ�Wf���s'��fotJ��.�C��<%\y�'r���� �­w+��m�M=����d�}��Z��-%_{bN��� ƥڏ��S�РK>�o;�d����i�P%J�3���^�����}r[��>%����%5��JI��f��Z����N2-R�F��s���Fe��Ԕ-8�)r�:��v`�SN1�ݩ:�R�T����}�U|L�|�;鹩Y�3Ѷ�ǝ��@n�	� ��e3�H�K=��%���q�i�DO����q1�*E��1	=9L7�i�{{����4���V	�w��ǂ��=�|9l4�����R���~�;ۓć&�Pp�"q��,)�fpf'�7�� I*a}@J�.�d��|���������Vp�<�Ɏ�(��{��|��~T���ho�М���bv�V[���7߂4h��7U�35+B��*���A��|n-�s˟]����j�f,o=�(E�u�����-n�WYz�8�#�=٬�:�D�pɅ���)^��'Ҏ:OȞ����	?����p�\,2���7���krn�>?x�G�ǔ�����oE�}e����^?�{I�|u���}�lir����R,��y�y�y��B��&<�K�@&�\�` :����˖�d�#�����;ų� .��c��ӭ/��X�K�������8��$�r`R�3�M�f�^�`ò\�k�H"�ò���-���H���%%��e���B�e�\l�G��f�@�C�Rb\Sƭ�Q��$����n]\^��^!ǹ�E���/�$�%�Ek��?q��Qj�I$o�}س�ȸԤS�ZU�k��Eq#v¯5e.0���Z�F���px7��^ƻ���w[M&�,��Շ|�~s�d�KQ�v<B�3���@����- �� H�����@$�e�+�$vc��Gq�$ޙ�{��I����"dY�^�]9��zM��*�^Y�%ǥkΓ�r8���O.8�_�n����ϗSy2ͳ��f摲�J���TU?�]�c�X���·�%K(�n:�T��ܫ���K�J7��l0F�MU�k�@��#̿-��'��1P@v���\�My��Cv_ʼy���^6�)HR�i8Y��f@�n~�Q=e+l���۾ɔ,I�56VT��A��;!4$�R�M��e��Ķ��cO�.����VүyY������bs��=��^��xr�)�''�֧ǧyٹ���9*��t�1�Ǜ�y���@�nȹ���O�zxp�f��i�dӭ���'�4/0�F�"��M���T�wyaҡ�f��&��2(E�ܽ�BRÞ��S����-R�')�èq|Kc��� o�WM��He2j�R���m�q�ߘ����D�gNKy�:�ɬ?o� 9�d�)w*���V/��2�d݋fZ�����I������w����� =k=�K�rNH6]f^�v؅'&L���~�>�P
`�I��]x��(���:�ӆIe^��p>P۞z�L"�$rG�Nh����W� �$��7N&��0��KR�p}�P���!n�{�\
�h�-?Ȕ$,KSξ���R"%�q;g~j�<������,f�[�lؒ_�!��1�x��Yh|�>��H�#�:e����+��ڂ֍�c/O�4�S��F�    ��l��%S�T��2IO�\�cr)YL�{i��'�t7��W���B,|2Ku2iY+����Ã�5BV	��q2��J��?`���Q��k��itJ�
��B!�� Zޒ�1v�w��i�Gݮ�%	sK��i��6����~q�w��7e4K=�n ��l:i�����*���RQ^�h�][~�j��ByJM�<��wV�����𵰴,��Ԉ�1��y03g$T���8�U�`ޚ
||@%rV�I̟�	��H�^4�(8䋀Y?Oy��W�վ5Ǆ4&_\c�1X��K[��&)(�TrGYn0�ˊ
�$����h��G���zdN"�T˱dIDM�����7-=�^B�S�m.A�$k�b����-P�e�K�	���R�=��&̺��k�d�O�P��`��c���9eH9���!�iWN.7�N��A���x���̓��/rtpnb�s�>P�f�z3VN]�+�L�h��V"9j7G�����qЍ0{�u^m'+ۿ�>�!��v�K���ƿ���E94j�á}���w��!^B̄m�k��ؖ�Tn�������(BW�;��#���dK����S.o��o���q�v�u�k��j�bC��F�=ST�!�צ�{o�~(Y�~��\Gz5��p�銭�K(��4E��\�d�t�Q����V�~�~�o?	�����r�����u�o�� ����;��?+ �_�Q�g9���l��w1$--K���f���QS�v��h�pEo-��6�{���!<�RK���1����e~9�R-�0'3.��2B��ܞN����5H崲��1�c����(?�<ya'��L�D�asY5���''߭]���)7�2Y�3[�pY��0�N�ʆe�ľ2�|~���,"��x�q�0�	�?�(n63B$�� 47d(��kvr�/�[��(�H]�Zj�U�$����r���L�s�ޢhר-�>1A%M޹+��fNx~�>H��Qk�C|)@�&X,���(�(�G9�����T�^�gf���.5�&?�A˒o�KQ�@{䍋��"槰&��L��,��T���W�ЙcA�WL��`	�_��O��>�I+07=5|q%w�U�:��?n�� ���_M��S�`m���(R��RB%�-�ƻ=�'4[e��2(�F.�A.���ւ�H5����]��z
f�&n�	�S�a�ϗ��^���sFM��Sւ<l(N Swq5Y���:5n�]�`C	'K���9�RAM��7�LF���}���BԛFfZ���g� �$�L�|m˥P�P��r�:�Q"HN���<�����n�J�y����Gd�pzBiw�
�Y�6�[r�\_r"vo`3i�S��/�/U҅�0إ���[
-���9P���bU�bj-?h�{�lW�h�����;��#Y�J���=|s�[�����t�?(���o��|�y�SS��ON��(���fnޡo3!��;\y��ڻ|e$����9~�ߦg�Zu��gն%ձb�R+�!D*S7���M'�1┚
_蚧��T�%<c��#?ι;NN�2����D�A:����l�+�N9�\6]�҇��-{�)��U~5ףd�ܔ��V��i>�rЂ��|�ߖ,
������H��0b#x�L x�P��[iЦ�
���V�]=�ku�n��cu�~٩�0�f�=��r̒��Q��m'p�D!��Z&�?�=�<\�䀠�&��`�Z6�e�$I�ܦ�D�QM��O��H@�4�˥�n����x�}��)V�Q�=�3�얜�ʫ0.����#52ȳ4�w��넘�H-���n��� y�-�� J>�Mq��@�q���ċ�A�8�>���!Gs���-��.*����l�]�W��M�ݹ$aAq���8�
�O��Ty9�E&��9w���iݗA˾��	\̼�d㼱��tC�I5��0��i�u�H��N���F�9�ũf����?O����ձC@�zy�m�{'�)��f�=a����U�Ζ8�J�C�J�
6JZHY�sGu��%����)o7�Ё2��P4ZR�'�N�;�i�m�a],���e�d��Y��_�M�i�w������uRR�T��7H�F'���� ^
a���Y.�kT��3:�٭��5�@Ql&�����%6�-��W������P��9��g��S��M{�2��W@�is#RN�i����y���y�{=~�����D��+�+�{/����2� V�c0^��V�o�e�f��e�JB�d��~7!�C7b^t3�]��"{���8Þh���F��8� ����.y/�wf0=�𚼌�|�ɣ�q��c�I�z7E���fQ�:�Ѱڿ���&O[2��I�	
3G'�x�_���`���S�L$�>��e�x�O��@�Jg�[2�v��6��3-��
�g.�r7��ď8�qt��Q�kW>OC!�ϲ|��;�˚�,`O�
�9�'�	ۼ#��z3S��'�uu��)�r�rk֤��d$c�k��0�(ɦŹ�t��-w<�G&S�
���D�Hu-���vo��!o�(>��| -�o~?�#�[�*��;s�����R 1�V���@AÿiU�^�6^��|�����	l�Lw#����V�~VP���"��JbBJ��`!�h�{0��J�5���J?����������n���d�{U&�>i-@�i���F��z�Z[� x�	�!�şqAN9-�y�fR���G��J��Q)���P�5�[�8�;��]�_'�p{Ϲ�Ylא�czϽ5��5Q��Bz>������q�d�4-� �e^w�V"[{�����ۃ������(1��(��
� �G_h�z�ռ� /+�E&����Gһ�2��ƨ�:���g��tL#� ����Fo��A����cd�y���} !l0i���ɯ�&ۻ��X��ؽ��䅡S"%+b����u��w��u�zDK�\ 5�Y�L�|Y����y(Pȡ�[E��:�4I�B���BU���m����_c؍��q��C7/��I3���~�+o[Q7Ԯ��Q����L�W�E�;�`^��4�nZ�C�j&_}̈́����^%�H��ǒ5� ��)�Y��6#t��΋ml2��d�Omnc�HP"!6�����0��1;�8s91~���ХS�{�Jknt�15���k']a*Mi�N���2��D/�ׁ��pn�_9�Ŗ������ڋ�w~X+��I��.�ͩ:��F�! ;����nl*���i�ZZ/j���?u�
mҹL7�z2�r<��'5�O����[�|���d걅p��\ *�f}~S
��X�VR:'k�bd�RF�镦a`��O31�9��ɷ��l��E��"�]�f	I�G� ��s�^T��G�L�%�����G
!��j.BS俤M���l�qy�O.��Յ��2��&�3��Y�?sV�QI�(�m��Ua��T��Rd�����T�0K����5���^�[��o"&�e�s��/�3��3����曤d_jAMO��4cC���ԩI3�����ߪC�]�A����ƾ�z�s�+]�	���h0�	/�"BK.X:����[�Q�i"�࣍r�Fd�݈�'��w��`�~��9�c�_!�u�;}��eu1t���>31�b�)^��ʀ�A�U!-E_��D�.�)yi�_l�t O{ϒB�-J��l�q���gj�9Gz3c�k��x��?���yMi�����pWVR�	g�e���R�f���9)�3�>���m�<R���\��T�Q���9��ȇ����`��n�P^}���[m��jZ�Vx���7Ffe��1����>���n���Ѭ�?Ǒ�W�t��R;�H6FZc�g�ٹ��Xh���!�Yv�	��=o�}@������$��M$�����&-uM/آ�S��ҫ��*������b�F�{��H�c��+�J0̦���3��O`��0��ל��dn]d=Ώ��}�5^�4c5lr�ق�H�J!e( �Fp �#������Ӱ��N�RSͅޣg���H1�M!>C��    �Nd�J�y��<�Nz�4[)��ZQ�OJx�(}M�@�v�0�{�\�>A��tehxnD71���I�ԟ�=��cw�^����Ro�L�L�
�4��)��	��8V���	H�L��=7`��S�m�E�� VS����&��Lz �T��5�����4�v/���˦@��~�z������'��œ;����d��"b?xR�Ž�8��W�{ǜ���>/����v�pM^���j뒶.�[�}-��F9؏ޓ��� _p��x
&%� p�����T��&eH����~o;�C/���G��P��S������y���R#o��aY�����m*oi�i�ͽ��Ms�A�rW��	��cѸ`%j_?g��r�Vϯ��!��_&�"���	�y�g�vHi��X�-\p[r1��99ғY�
��ܥ#{�lt�ܤ�䰹��Q���ea�?�
U�Z?�0_�ܲsC(�b��eĶ!����5c��g�[f��7w��|r�9LM�YI�t/���Xx&��IۗT�)��5S���r�(�����]�)S'PT^��3&@�SC�y%�C�15�v��Ý��[6�Ζ#�c��eY�?A�[ϡ�kOK�
�5�H��e�n�Ӓv|p;X����XW�F�(U&xn��c��ʰl��T8�EB,D�~yc @�����s�R�#���=�d;�����Aآ	C丘@Y�!�� rc���q��՞iK�y��]as.3�)'i����9�nDX�|�FՔ����&��}~�CG����1DA�u{�8U� Y~�H����J�7�hhv���;�[�k�TNi����D�5WQ����� M�%��a�,��iN���7�]�|�O�(W��MgF@��4���q�s���k5��h��X�T:l)���7��)	����v�pb*�.�Խ�4�8�=�p'�}��fe9�]ƍ���f=u���� jo������9F�f:���`���G� V�Aƙ�N!��s�8�ƪ�H7�b���3}^σa���T��'��-��R��|y��i���K����H��y�#�M������9�t�,��r���h�l�r��{��b�G�EJ�M�Ub�� �R�L���i �
���C��G�WrC+��D�#�.!8��Pk����߼h����}��J2aeeMdp�u��v�=�p
�3��#�z�
�T@��� :��51nf�>y��U�jeN��Y8$����N��e-���哾Ⱦc�N��$���z�r�s�M�o�����K��v�i�捏�blX-m"@�c[Nb�f�@+� _e\�S�^o>={m�ۗ$�1Q��H��l)7 H$v[j��;e߅Ġ�Ɠ������e.�?R.ʒ���9�2>_ȧ��'�.ˈp�W�zﲗ���<^��(I��h��|j�����n�x�j�0��V*<G�Á���|�K����i..�T�]\vҺr��pT~��n˛eK���o`3)��@y��}�v�x:�!=arQ��$9�x!�Z,��BӔE��M��S>�T�gO�^�d��(Ps�����lc����삔kƊ�*hR0�"1�b`#�b0��	_�����RіppMYf8�'�[*>ܰDs�˶f�!�=I�M|���� h����?���Hi�p��e�	i僘*����!"��c �b?J �hBj�9�/�YD�PA�)w�K�9~�@��roR⤤��HgQgIZ� @�����'~�d��4=��خV��lܝB��T�<�O��x�P�w)�Ɍ����T�$Yr%s�(W���H;o���$)������Ȝ���=�U�g56y�&J($�r����~�M�k�6()?�m!.��6K��,b�q� _�~�b�����,��"I2���ȷ�Z��Q� TuW[BF�9wK+���y�� �����n�tU��T��0�ԋ�+єsyu�!����Ly��V!�#$�.k�ۢ�D��`M꒟��i�I�_E�d���_��js�f��z��nT�'�Y�m�S�iItNASʾ����������a|	)�ป�8���A����F��r1�K�}ۏC��?oY�&���T��c���x�d�����NV����Tu���N'a�{NS!tO� ��K���dք����p���?���ᰤiK@Kɘ�k~��fI�*Ϋ�
Ly���'�T���UXbcXd�;��EL��4�����	S���S��Ʊ�SmN�|O�]�Cu�n{�5_W2�ݯZ�q7J8J��YK��cGM݇�JHg/M����j�XP����_rl�R�'�I�����ge�L��+���ȵmiT���A䭭%Z�ZqE�q9��;�a�	Q��s�[�衲���j��^� t���6��^�O�z���gǞoTS�2�%Y}YX�k���\��c;"԰�DvLU�=qb6	���i�5״k3�����
:�e䔬;m�G��>u�ĖF����,��E�ٺhN�?4�G=�-a#�g����P�O�8��N>>$��Ϥ97��I~?Y�7%9M�/�K����FJ�tQ���ܫ�ÿ����!e�@��^S����IY���{���@oR��!a�N�� �	��!�T���X�8G��)��Km���i��t{Y���Y�
kt�˕��*
��&Z�\����% �_��H1�͆C�Ҷc�&�+��~�5���4�g
0�B���2��笰��L̺����{|ʕ�w�IZ>���c��+��Ž���鐝����Љ鲓/Qd��S�]+���~z��2���̏J}rSkAP��g���a,��c)��x��2��IaK�8�� ���`
��=�F�'���+HS~3���݉�Qc?�6�@X�TQi�|��_��{
T�����#9�	IW�_�^������s����R�D���i@�ZbkO$ ��l���^�%QQb�i����F��>�I���E��đʐi3�QA09L�[ 2J�P�7Wv%d	�_���@�+�ԗF6��Mʴ��H���Ԙ���V���_	��N����c0[~�}����O�#$[w���n"t��K�
I�e�O�Q��Va�'P�"�lMބ�Ex^�#�$���8����&F�I�y7CX��O���-�Q���g7�Ҥ퍂�c��*�Ы�<r��Hc�-�vi�R����9Şj�PT�ؑ��/S���y;�%��J\W5�nEt��O=��m/�;���a��@�O����q�����h��4�E$��G2�0�ع��"p�#��Y�N6�NfL��/9�>;%��Nι�rp�4At���H5�Ee1/L9�^���p����à�9���%��<�9=��mJ瓧��yO?�<"ӝ��F�s�(9SA�hn����'�uˁ���&s`���6.'�m�5�''��3Me��ċ̅'+es�*m3����f��'��N�S�9�D�����>o7������h��R��
)���Q����Q�w�R�LGiQ�~�B�9���H��GU����J�b�@���E���N�ȇo[���ޝ#MB�V�d r�|H�O{r@� ����Ixo����u�D�t�_�����e�×'��`��(?Íe)'��ן��q�ʣ�k���g��d�!48���sd���D��)���Ú{�@������r0�W�F	h�N�#�ir�'�\]��aX��xd�<}�0�3��y�?L����Ѿ�NY�wS�����z�����Λ�0�O��0-�}jX�oB2U9�"NI�|�֞$E���P�3&�����Вw�<o�9T0n��-Ա8��ME�������o���/���@�����m�6���]�!��ǔ�H��!��E�J���{�%7�����Sŭ��{�Q�pDk�#0)m���pg�l�(�!=�I�fie�<�ߎ�ou��O�$~���@�U/->BU��D���� ��_$gHd؝]��g���B�^�316v�1��ܰw7a$��cB��8X0�{ZW�b�5s�O7h��'��i�s��S��`x*��;i�D.�F#_��nŽG���/e�T*i"��/g�����IH&��	�    �"�<{�u���K#s-�pF�,r/9��Q҄<��v�r%���.T�Xy��鿼?�6_�ns�\���	���ـ�d\.2�F��m�^ںP)�z?3��p��*��N?��$
����X���SZ��_Z�Tf{:��N�ވ�<A8���eE�Iw3�v��G�����g�mʋJ�:`p�����K�)4��Qx+D�Tʌ���G[	3���C��gL?�;��z��IN�ƃ���K��nAW�b�q+|�(�x�V�?��� �Δ��vQ��1��s�y�z�Ykv�̺����L"�M䯴s����ʢ����/b��eJʼ��78#������{�S� É�ն_�-�%)!qn��f:�����@<�5���m���N�ڧtjvlkX��r�A#����m���.x�Þ��©n�Xw��:��=�
XY���{ʋ7G~L���|��t=�� R�3�� �^*h�i�V��'��KP�<�9��7��Tl��$L�f֜,A-���Q�W<�<�V�$h���GՇ�ES%?��ଙ��L4B�6ON?i��{� ������*��8��Y�MWV��9Y�F,�'3�[c��*�������Dғ�R��.*�ɕ|1x��ۀ5��mv;k��K�}#���f*8G���z}�1���QP��%{Jd>�D�(�.kK�[
NJ� /�
/��Г�Ĵ>��!F����q�&�ϨY�U��
7�G,
E:�Fp���i��yP�]y�%h���1�i!&No��|��ϥNy���{�~2c`Q=��uT9c5��_	�S����.�v>m��òJ���sUz��y	i@u�cR)�~R�=�G�`�̓!K�e.�Т|�S`}�t��ޜ�D�A V��^��y��BfG{ȏҏor|�;c�����������r�h�2�ˋ��1�pH$�8ԕyOu@G�l.���$,sSU֯�]�de��n1��δ�Ui�>]{s}�3�U"ݜ��5�����n�Q��H�h:v��
�꒭�!"�$ar��x+:��>�W��?����S;�ì6%��5�6�ͦ�ɻ�0}Z��c��f�W=�&j����~��� �)(��k����W��8Ā'Yƒ�"��f�''/��+[��b/a۔5�"���׮�<&�&׹$M�a]0��}���Le��梨}���xI63��9��i���?��D��S�IrI]�2y|�.���8 �o"LkAϧdG��� Ld?���6A�3?���K��T+0���|(��ٰ�~�Dp�PK�
��e�n�(�}��h�Y	t����*ܷ�oT>s�{j���Hzz�s3\�4�%ǡ�p�K�;FÅ-�]��1�$�Q���w<# NL�I�Q��0�b7
��[s�{��gKэ�rh�}�_�>�U�~&U��6=ρ��NuB9���(��&�\U�N�\���ډ^���5�j�6`�.�itV����3��җ]J�.y�UF�,��.�2MK���etV�٭XP���8��R��Zq�����E�R�ܙ��a����K���K�D����*2'w�4A_���Wҁ�ϓ�K�I�<�י�B�͕~��C��"g賶�w��D��� �A��I��χ�WM$4'�B��}��):NB�zs��C���O~Lgܘ��˓)4Œ��Ԝ��T��&��Ml\ԜE�O�8+J�6䟴����^	F�6oy�
�,!!y���h��Nb��<�Mfְy�j�Tj�7O=C[�!�p�D��42R�hN�Rax������p
?��}'��C`ƊU�œ<�Q�+67��綍~�	0)��9ˣ��J��Мf4u.j�[On�%+�9�^�N������k�:��`^
��%��D��7q&��o	�o#էzyk=�'Ao�'řt�.��,��^���Lq%�;�J��;�F�r�~� }�,�U��}�R���8(-%ᛵ�,�.��y���XTӵ�MþU2޴�ʸs�+9�f��k�����γX��u��r�0��J�ϋ�.n�b�����LY+P�N��:Y�H�;��@��a�o������x��ؼ��Q&�#� c�e,���V�5�#��Nš])�T0{���,�rB��|TGTj�8ϙ��8ؕ"κm�Rd����xl2ٍ�,I&�/�cz���eP莇k"�QQS���5G �����9�l�44��eO1�Q��J������Y#%;�@'��4�hrU��ĹL���)g~��H���b���X��1��٨���]j1z��U����@c�r^֒0��ugA!�����z�:�M�n��+X钨���d+ќmS����� 	RL����mk�I�\�x�i͹���v�m'��CJLm�t���q0�ƐDD� �R���|F��
�O���G9=�E9���l�ҖN&�>u4d3��P@�j<X;�L���f���^5d[ʪ����_�"q�m�s���,3�>��"�,�Z �ذP�8���e���i�w�uV�z���g"ʂv%����t��x�^JZ�n���P��	a���LU�B���S�L)V���i�/K^�:�]Jgn���R����Lt�֟>��đkiH!^�)�KFMz�}���R�[��jA'{#ϱ����
�[���L�ǆ��g�vm���D�ڞ�mE�'������,#.P��iI+m�xq�O���֒�rxh�\m�QsA8V�9륡Or\�7�L�#�)�D�Js��#?��w{K�������p[1�K�j�Q�b�jИY�Y��\�a��&�:���8�rs�ѭB�����4Ի��DҮ�B\������ܿS�VO�Cp}�F��y ��(A�/�qv���䓼7��U5�µ�v�{�Ω���;��A�sЦK᦮�3T�S¯e�L������`���y��\��9�7u�KM�)���3�x�M��i�t�"��;��T��+��9i���Ǆ��⸫^ҕ?�r�?R�����+���kץ�PA��i��e��
a���G5�;��JІqb~2�L���X��H�O��`���l_i��Նp�>�خ�o%?܋q2������:����0/�d����+��J)� t	đ'�n���sܔn����͜@	�is=�V���wS����_]��H��$���?%�w�?���^k��Gc2O�jIa9���8�=�&���k8E��#��Ǐ�`�J�-�ԦnO���m����ʚ(%��Vc��j?���R��L��?�]��0��sϵ��������S�7vYx�.H3V��6-�����;�R&� �����[���^16��)���5/ �ZI��!�R°X'��q����B����Ӵ�q�ԑw����F9<V=�z�"&�2 z��4Tq'�"q�-�����۞���.��)"�	�){`=l�d���\0�b�M�-V*8���x#�C��H��	w���ҹY���M}M�:�b?%�O��?WB�=�s��S��n�F^��v�)��PC��m�Zؙ����@Cǻh0�9(���H�+���뼒

��4�R�}��M���v��^�W~�o/�,��_�"�3��~ʗ����X��X�Y�r����y.؇�����ĄU��|'���=!v[w �2J��)򫠮����9,��M�D��� @毬����4����~�/���/����OI�����&p��==Ӄd�����+�c��5��@��2%w��~!���a{+FyZ)�����?lş2T��[�9�)�i:��
�E s�G�bM{�Fv	� 9%v�'�q�!�y� б�1 ����X��F�d��!�_�'���{�;kM��<�M�k��|�Rf�LU���s&�C�� �f����}nt����qd�$dF�Qrtys�)�%�W�x�),TxU'.��2)���ɵ�E<�=z�8���椶�>��͐�L/Tlޜ��g�P�	G¦yU.,���7iX��S���Pz�s�ָ���砦S8a�HJ�<gӴQ����	�A��@�:�]'��g�c�^��q�G�������ܴ��e�Ӽ>��	%y�    �(W��~F"��Ph渊sq�o�o���!9����>���{j�d��PR�9M�j6y��r�R�R���,�]yN����m/^��\z��i��Ʋ�H|O8�0Qn���Ӊ�7N9��*�	,��[�T_b�h��D���V�y��/�n��K���p>iǞN���JSC�Q"Y.��9�G��u�l\�ż��̤��朙��F��Ż��Gؼ}���2�6���s%���J�5�|VI���@�P�9M���Ɗ�`V߾��t���I�I��Qq���H9�r�d��"���i/�����"��4�v��Kh��.~�	N5�%��ٓ�^��P�ڴ9w�`r�\;'xR/��l+dy�5����XvS�N"�$�9Y~��\���:h
�$%��[/��>���Ġ���LZR��3���l�hf���__z��r|M�N��Z�Ҍ�]�m��ZQR�l��	+L���l��[���u_�����_�Br��,�m �P��N{���
$Y"0�ˀ��2����[�'����Q�ꮦn����#�r~U�s��y9���$=kFܷ�6�hj��<'��#['�����ORJ��	�:�	S�%s����-aU��'_�����kI���	8H��υ����o�=���(>���J�s	b�W��s?���(��b���M2��p_0���ۄ�a<�y�	6*�͞T��,V>�=)�WT�}n
���s	+p�:7�7�a�p��JU�@�V�uC����=���΍H�i98�k�s��I�(�o�E�bк"qp��F���5�8e�tX�3�7���`P�7~�>צ�au��@�J����ȣN��s���J��y���Iq�)	
�c�q�؈�����ϗ�p�d����:��@{�il�s�g���P!�}�Y��y4i����W1�FA�����Z�`���g|`�%�r�Y�d~L�Y��g�mY�+��1�䥵�D��,�j����AB}i|䣁/1�Ņ����mℚ����kk_�c�CV����|R���D�D[�}ӟ>\���]�%X��@�1��>GTO��PT��M�UA��ɓ��jv+n���&s��V�H�+�S>{�E�K�4���L
u���+��|_��s�}k�*���v�x���Z����$�ԋ�dw��m}��0~�Bg��!�>8W���|�j�A'��t�,A�Ǿ�w����?�lD�w�mW��th�Y,��̏XE枣��JIʲ[����E���䘤�l/��#7}Mtd^o�u⹧�gGC&��"���!PQ��O����8Bz�<�}�I�� �:Ş��������9� #��v+���%�|��DY��$o��p�LI�m;�<��sl��n�u ��K(�|���"E����q��'Ѵ��uV|sr�����^?�`!�^�����/�a�f��f�t0�"I��l�r5�<Q�v����M��"���/�Cym�lE����XW2qS�Q��d���c�5(�)�c��*C;T\��K���{q�	�(p�{ߓaL�Nڮ�w��Io���g`C�X��2y8|*"3M��_:~���aR,��19x��]'���
��_ޝ��J
��T�4 <~��T�{�ac0��P��g���gVB�ĥ�p�0����#"�'Q��!H�/���x/�4>Ն��%N���i����-cZ��T�R @�d��(׹oiCƯ�}�Of�Nr9�[V�.O�Q�e+�ү�珒�?<���?׹�Ax6V���T�S��Uv��Gr�A�	�T>�v�RO>J�9U��^�1O^��q
d�6���=�X�M�Ԉ{���`��FY�3SF�jU�"�=I0M[7쓖JQ�'�����*�u�#�[���}�L@��J K� p�tBRU�aZ&�ŹT�nǐ���P�37���X��2��M|*@bmi?���uZ�a����6�����^�a�{L80ɥA2�Z�����������؃^�ަ�ߜ`�vW�.�yˊ�fx�LK)i��
����%<�i{���Ɲ��L� �-�c�Sϑ�i�9��^Ӊh\��PFV�,=��S���,�#�NNo#ş�lcMN�.	���b+f�Aί��v�l?�)�����)��lD�P
��N�pRڇt4�~:��T�#���	Г�vRGzMB��N���He���>�1����x��z�\�:	�����
Y��F-e@1��J&�3�ߗ:��E�E��t>sY���H��^K��N�jO�aC�y�T��n�<�ٓ͠nvQ=+��TJX�T���$�t�NMC3���!s.c�w����>tR��J�vG���G�E>~-�K��&'J�C@�Tny&;3Cț���|�<39�Z�d�A���8�,���`��Ȅ�Z~L�|(H���c�<@w��z�R�BW�:Z9\�f�'|,ɔ�<k�D���ZJ{����fF�Deʟ����g.ў<FD��@`&��.�援,-�S��_:sݩF^���y@E<�\���T�B�l��D��6�kp��$꼛m�5�~�S�tcf5�])�]��o�9w��)){�}޶�l�(���W��,�="�����6����o�
��`bvܴc��%P,)Hw�	��d�J�:�&1��;H�9,���{Ҷ�0K�F��F˪Y5\��iC�)j�_�\*��1?��ݾbbk����L����S��B�]��9�8�SJ�4��/Ɩ�d�)L�ߚ�[��M|+�0�Dzm�����yt�Ӓ%��4&ୣ�E;�L~��[��myL������`X� am�m)��6  Ĥn"-�*�&�i�Ǝ#���YmY]���rxv7�P�o�bE�����t���u��m�|�c�11nW��������:://���D��Z�q�#'�fs$�$?͍<��F����]��x>�S���QJ��.&� �/{*|� �zMڒ�R���I��ܔg��N�'t�_��q���k���v,��wN,��4x��o*fД��"L��n��Nr}��Þ��l)m6ҬV��-���R\떂�f%_+��4u#������8{-��v*E	 ���L�6��qG�t3I�y�,��I|T?%�R,"`}�\1&B�{)�'��Q[p����q�s�'L�\�����<�u�1'��ۺ�L���p������e�H� �4�C-l�f�ķ����5)k��R�k	r̧��듯9J�,��}�:�T�8��YvL��P� ����Nn���e�~+8֙�g
1��y�9�95��s)Sp~���r��'��� ��z��r�	��r��YϫX���M�K;y�%+ ��k"uŘp@^z�b2� ��r�-^��5�5׋F�}\TTT�F������'��R
k���3S����9�6���?O5N�o�.���Z�S�	�JDK*���*E|�=
$|?u�>�f�/˙�#�ffþ\���dQ?خ|��]@y��X��P(w��u&X<���ܕ��79�*<�b�y}��[o�D�VJI�`�'Y�f�2%������?i�G���/ޮ�$� ����)8͊?�M)�|{-��Vw��%��\��'/%��z)	Dz<�]-��F"��9/��^�����^�N��qYr��E��v+FG��Iw�½��2RJ)"P1JsJ��8A�3��A�=$Rs��B�ic����z�^]��O0��#-��ʉ�L��J�=�k\�j�ҥ�$��z��AwP1�Z޲_�'�P�����^�WG�zVj���ĺ\_d�d�뱗F���\j�<�Ǟ.}_9�����ߗ�1���<[����ێP�ȕ�X�:��< hAݕ��B�!W$1'	 ������j��$���[E��������oKJ:�'�͸���"t���ߖ1��ff�$��$88��a]�߼�`A
%��|f�����Iq���;��ZnN��� �dܫ���m�J���F����n�%�7������y,���g�,):�t/\�s6��a	ʯ[�E^,�K�։��6/i�bҦ$	���Ρ����V1�ڗ�w�K�N�9vqH�;H||�Ř,E�Dܣ�:~��P?� �v���    v<i�^J���ey��R� �Yq;1!m�&�r�S��X0`��^o�j��t���ϐY�1%��4R���6���d�uY|j�ҽ��	���)m2=�s!�zx�:7�+�7C�q~6�
	^b�T���Iב�*�~�'�2t<q�&�E��k*�K)�˅v�Zح�m;Q���,�J���[c�i��1=����g5��r�M�n#���IZ��?7�H���O��/O�'�|�%��(K�o��zxZiRp�� ��I�� 7Ѧs�JR�� ���Z
8�@m�����[�Z`���Z��\		�b�}��sI,+����s��	R����GW�П���/�I�K$;gП�K�)�v*_k��؂�x�˦�e���V?{�@�$ɮ�)r2�Ƽ���k�Hc[a��m.����)M�/%y�Q9j�M������M�� ����)C�O����1{4�J�Hu�o;�p;+���:�����+1qs��j��q]]hReX�|sl�$iSk];�8��E�LZ���K!�Z�fz����>饵8A���E��j�g@ʩX9���y3s��\lx;L2yxD���Ħ*�w{�4 �RT�8�$�^�{A.:�	\�~��؞��X�����$�&J����{J���ӗ��{n�F�u��n{�]��A%�>Q�K+$��X'�Τ߉�i,�}�$r�ʤ�~i��幕3@�*��6�Y
� ����	Ky?>B��|�ܗ-xp�j��Zt�KR'yM�״@�K�%�Y�4%�+T����~�E�%E����(��h��T���_&�ؕ����y~7��s���J�tމz�g�W��3����F>������;��E޲�Ο�P��ڬ�F!D�u�vJ�E��+%Ad��w�������)�S�a�X��RVS����v����/ro��1�Brc�1�skK	B��*iM��D�[23��v���&VV�F¥V�H���D,��1�Ic�K�Z�����#�Ig�*���;]�ʬ�Ҙ��!�Z���L�"��d݌H5;���`���)n��R��1#�xM�����š�q�ӄt�A^"5'	��JǼ|��J	�i QK�v���ɨKэ�f�s["r)9E���5J�}��3�}TGJ�����JI�s���5Ԅ�>��U:��j'.�	^�j8Q�Ol;
��̽��Ꭱ�4��nv~G�	fO�}���Sȅd�����{j��8���9�%'i�)����/�{*kuB預�����L��J=�'�pb{��y3��BH�3Nl!g�r���?l�	��0r%�+�;x.���0�yd�Zc�I�QWUf	���4˧��R:$sꡩF��YMY	v۷�ӛN�9	S�bp��y���0��#*�Q^`�!&hv��7�nh@l�P5����b�K3�˒uM.3qms�A�B�*�d�����8��w��4u��̡MD����"mt6���r%�TB� .�ةLTx��k;}�a�ǁB��{�i��K"j�3H<	��WB��	���� ���]Nr4��gkd�?���Z��G��6~5���9��]�S���W��>},�h��f�9��+�����&f��,�����_�~s�<
\�ԜKj��!�q�sd 2J]m��FS��?:9>Ό����4N��^�D̈́�gbu�P�+�����嵰*�˶���o�[�J�o|�#�NB��!5C^�Ͻ��U��XI�e�cZXǑyNz��s���A��"-��J7�4�˿2[�j�"��m%�6���R��u3WEm�	�I�u�PN���J�Zf�uMN�ķ����ˑl�)K�w7s.��{��d�z���&E9�:up�d�*�ӈ�c~8O���c��,n��-!-�U0�9O/eûԺ8��;dG71�w�sOqubpP��e1��\>4�,n{�җ^x�4�nW�
0�Ǧ�D}I�$��A����Ѯ�'�OrU�&.��L���N���U<nk/������]Qqu��9� :�M�i��ւ��xz�t���<:���c��M%������P������tI&���d!",j�㔲ǰh�٣^5����i)��a޼�<��J��L���F�KvυOE�}�!������Iz;��`���r���~��<`����@]��LN�-F߹p���^��ۡ�_&^|�H���|��HT��ڃ��9�����J(Z����脗	��N�!�+V�I���GT,u��T6d��ir����¤�g��d
��v�m��._j�"��<�伭��̬O�4�Nu�YV��G� ���o�LU�9�+��K��Q�&yVh6�N�ӟf�@��Q�yNyf^u��o{������7՜o�'���A�X��l��H�A��IZ�\H&��`�*>����r S�m?b�N�3�i��Z�2���8_n6�+V��T߀B�).Z��4��%+�9~�`�NY܎4E�QF��vl��ҷ����͑k���Ce���zҭL.�Tހ����hz��k'��'�
VFA`�&T��'r���/X��׼��Q��(ŝ�s���(u���d�MY���S�hxX�'q����ob��Yh�z��b2a�03���[K'�܂q<�b��p=qjI��JjC��!�?�+<j)�ڡ���EcQ�Y�L)UH%H*��N�g��p�T+㓼�+��v�֮y��|O��7>�6燎Tad���en8�Ʌw&C�w�替���m ��*5��P�1+Ve�}H���@L�	cQ����c�D��S�5�A�/U/c�sߧ�ܝ�ԙ`���;���7�VB��#��,�m��6Mf!)��c'�P�I��%0*��7k�S0W>�2X������1,���rDKٲ�/}��L���y�l���|�쟑:z}���dR�4"���4O!ar��z�<���9��!���L�|�Y�n3^�I�$ ���� �5� }d'Ck)�4���.k\ƏE� ]��I}��� ��?��n�����j"	>��p�P���rw��y��ȼE̍���:�EM&'��|�!` 3o>(�N�h^QMw��|䞞[�������p��Ǚ�xP�:�:!�����8�n���I�������8E�y!����6�'�ׯ��i��7^~�����e|o�,�(o��QNVl����8�<�SL�yo�
�M{�z��	t�S��J)oy���~N6�}gC���4�jH	��4_�F�P=�=3���7>��a�gy�z�FM(��;��M�c�ȇ`��NM�z���#������d�M*��Z&���LCb�Q�Wn��YH���l3�����l�^���*�E�{���g�"|Zg� �C/i�=F��&(�yd)]�Q���x?H�-���f1_p��.B����x�g��i�����������p6L�MK�=VZ��g�)�7���O��s;�?i�J-0�.?���\�H�XRݤ���L��N��J喢{I��X�D�*��n����P���w$�#鋲�
'����L{)��m{���C�,�m�E�5I!M�7�Ysd��9DzӬ��#�m%���Đ����@R�*.rO�Lǚ����%&�b�m�8G��~ �vC���Q[��U�>�8��/o�c�pT�0-%*.j���u^<	�M{�>��qׅ�|�SX�N&��辆/�#��5��յ
�)I�A7���I�����S"'0��T�����`&\�0���]�ih�֒l�6̩)��	K�sv":��]��Q�8�9r� �^�z�	��\�a�V�O(�'la�!���mH�(cԽ��Oe~Q�s��wM��W��\:��~�,�Y�3^*��IO7*��-��ZJg�(Ƞe}�b����gJy�aq���r����������ؓ�L�0�f
�����S���#�SU1.k/��p�A��R�آ�UL+�p\�e���Mh��ౙyp�)���`x����i� ;��1�=ϰL�DE?R_�H��l�����E`��ɝL��Tm%���(܊��,OvI�iLksRI���u���eO����f����<�G�4����f���j�s1C~��m��S�C�r    ��5xNL�	�.OH2'�:�8��1�dL�/(*��Ec83ֳ9H�,^���{�G�2��S ձU��!�:��j�v S%椀D��)�H�J����a�����έ��g�
4���f��j��I�I ��EO>�|C���i[��KxMt����V;?�O����Z
���p��<���������Y���_�M2B�wu���*�cQ�ˢ�apZ��i0~�Ee�j��7�G����<�C�[:פ*���3M�:@�al���q�OOS>�u�H���,0��N5�$����GO�E��!u6�J�j>il��L��)��`4,��В�(tޘ93r����v̧iu��`Ӕ%�T�M��2�� _,3��#�5bř��62|�Q\sC�&o�F�ZbJ&��V�RNz'�2+>�:��ț�Ҍ@�vc/�:��y�|�n��W2A��Tye���f^�����|���qNG���;_b*�,2^�^X�Q�$�I��:7�M�h���S%����
�_rwM'a#H�2���h��s����ۓ��,��mSG�Y��7�m�rf六�_8�Z�wI�Dr� ���^v��_�_�DB��C�*c�9_-%J}�$���t�Ř�������7���7g|L��6�;Ɍ6��K;L9��r@᭬�\�D��o��k�J�*�D3�W��&b���� F`_���w�lc�v�r]~S��:B9W�f7Yܙn�J�9�s��$�M@r�"'�̅]��x��`���^��$��{j��'5+LXo�,0�cx��$7���;�0F���:%��Ђ�ٖǬ,��i�z���fഞe��=��yI��K���!8�[�n8�g���F��L�ضB�K�h_��9
�u�X�lS�����i'S���yڃ�I|�	vj��֧j4�o�ɐ�Fʛz]��c~S+*�a}��#����|0��t\O�i5��x��)s:2D:���0�[R���DA�o~]9&���"��\��55Y���b��l��Y��6��|�>V��Ta�{���o}��Q�YzK˓I�-`q���9E�	3�Eڌ���h�@*�isr�5L�͍P��)��F�����PG�`4���9�Z�^�t�
Ϫ��u��+��Kf����;���a�7��$����U�Kg2_� {g1PU�!ثqV����K���_{��Ѣ��b���	l[�������u���P�u��;zְH�yIm@q�,����>�g
��+&@�R�X��a�-O�N����%̙'7Rm�h���O����0�X�v��	V-�s��B�q'd�u���V3y�`$*~��������2��&��:���V9)�!�G�@%���l.W6�1m[�V���c����2݅%oq�D�g� ��=�`�S�[V�0��N���@�,��}5�͡�
�'���y�>���GƦ)/�.^i`��K��R0D2��������"@˾��Aa-�P�X'�k��{|:���t��|�$���.��L��o1�9�s<#�xN�r2���ɴƔ�؝P�(�l�7MZdQ�ޔ����4�X����,L�M���l�a���N�**W�=�YW���G¯P+�u��<�hQ�7T{͢;�R��<\�A_s}�F~܈�"P>--!�-�����1������Vc�V�H
����\'l�i�6͜�w��a+E�h�!��b+� k����{��6q|XC*v��:�3�e�?E9L��j�]r�D3&t �_hLK�D���&yhs�y�:fße�2��q?#QZ�"ɥ�MD�q�&
��߮�w[cH�������y�G*��}�MPzi�^9��2�(ӝ��@(H}�����mM��5�H*Q9�@H�d�3�>��Qm=����������]Ϲ?ɏis�4�0B[�w�C��ҏ$�sV���3�6����m�LV8�ʠ�B$˷�E�δ 䣫�L��a�b��8Yy��V���#F�-nNa�$!��"��N��E�]e�O!�4ǂ�)mX������7�"9+"��9pɞ�(2'��8 �nr�)�0��"��9����wX����E�<��ʋe�X�uNKy$)՟�D���@��rN<�)��hq�
���K�zA+��י_Ϋ�uؠ|!���6(k�ܛ��~Kf��bѹS�*�=��GL����X��,A��rP5�ݞ����ɋ���7?L䮶%w�Mz3L:�b�%�(w��h�l��y�(�I�&���C5,�~N�Эv�Z0�f���M����ݒ�ޜ�e+ʂ���neTh���4�	I�e]s�ޱ���'J�Й�]�#{��������7�L.��n�m��ތGs�QϗG�:� ��ʥ��u���)"嚤��Z1���G+������&#,��5�K^���V��=iٳKe@ta+�w�1��}\�X�)�O����_[P���z�`hŦj4���V�[H��)E�'w�M������s�L/�L"-��3�o�/�y���'��5x)<|;�#}hp��W]>Y����>�����i`o�)��͔(o{�N�<�H2�O.ͅ�����.�H�������k��R��Ծ�y��4U���E��=��nx�kz�oc�',����}7��떶'���7������Ƚ�7�i$4R����C�)���uL��CN�#��ٟw.I�tE~��̃M;l�wM��y�P��{���f
UV�����@�x�:������(׃���CN��0���1x���	V�2'U��g��,�:�J��۝����>X����[!W�>��.�A?7Ҽ�f�B��;I���؈�ku���R��^��Mɿ���e,p'j�Q�J_uIRbM
No�.��ȁ��r�r5���iV�&W��MF��U�NI� ї`��QZ5�Sz�׀�,O.�K�_�\���9'�|�j����9��TRi����I.!��@E�i*�֞H�\,��} ��Xl|H�a��ÿ���f0��fW0Y���,.�w3��<OM���x��ԑ|��n"#���4A�
F�ހ9��Sq?�F0�b8�O�=H���'9�L�С��-О��vO���3���˼�9����o�i��t@�ˡ�Smr�"����x[J�A���H[LZr)��cD��D�k�[@��𽝭en���J��)�钨+V ����;�	-��W�Ȭ���U�%3���� �ir�k�|3��1WCKظ�A&��	=6%���,>�;̀?��ְ8��/�<��}&��$�WB垒�2�>��R5q�0�N Jqg��'������y$��?�["�(}���R�2�"������n^���L�	
�1Q��̒���>�`nHM�\c�|���L��w���gz�9G��o$H�vlU.�@�,j�Tb	�F6��"}��e�����$`+%o���O6�H&d?]7�Q�x�[;�ߙ��c�3�&C�r��4�3���
�㒚r�i_&?V�����G:��ٓ�e
�`]����?�\NjB�<x���8�)�>2�i�e�� '��$��<	O�S�F�C��A���6�<y��I��d-�#S���W��l��s^I��2�,��8���4a�R��8
���4
�Vz�΍��m��0/�������>�(�"�<-B���RS�v�v�����B6�y3��=�F�?@���J�K[�NMa�ҭ���V�'��&���}�{0�����Dƻ�+�K,R�ht��-��cVƉ4fL71۹RѧS���]L��3�9�1��՝�v�-M:�/���s���-�/�A裎�`������F���V��D���q�84�	��g�A��!����w��ZGB�Ur
�D���w�>l{�.�ћ;����d.�U
qp%��f�@O������ER�ً�%�R��������AX��1��U�&����+\��N��	��9_����x >�V�AR�����A�OsZ������f�++���aʿ�J?�"�k�#�FC�Sy��~,��h4���*�=��B�r�҈q���I?~y�;3G�u�    mNK�+��a�ˣ��J;z*���^�9�ђ0��ދ������Ӻa������M0y�\[�^	�a�a��_Y��e�ȫ)[+��U��@���~��l��d�l�(�蟟J^MZ�S��%�C$&�<���!d���*0C������� Z?%�Ƚ���C��mxtf>̼3}e���@A_3��C���D�1�Wʁ�}G28��
���+� ���i���*�P:Nera3�j3��a��:����I���d��~�j� �ئ#�T�+Ig���l���f}P\藉C2��Y�z���6����&U��(x��V������g���GX
^;?&�$�m��\�R���`�!/��D����LR~��Op����k_��t��L�J�Sl���IX�G���A��:�%�[R�3<ԽFJ��S~�+kJ��X�$���nS��u�R�g�كk8M�s���fNZ�ng����u��Az��k��(�Sc���du���^7A_rS��H���kK!���9=]���Ί"�����]���vo���<��X�)�ѭ}I��j�-�>�ϝ����$��yH�)����KI��t`��c֎4��KxSR�ǣH���[��#�x�ɏ)��Dm����9]�߀����sOE���@>tzR��ύ'�%Z%3�dMEġ'�ԏ���x���6W�������=�贳��3����L��E�d���Q;�m�{�����dO1�W�E�ޓ{m�P�s��\r�������/~;��۞����/�(9HN-ELO��@�{��+�/���7�Ց`1�����k����"�1#����k� �w.|V'my�IvR�Y
J�b"q%�]�b�K-���+���sm�
hz���}�".u��g($0�s�U�s';ߋi��-N^�NK�]Lݮ��n�<a��1#�������è���[�����O����2`���Ϭb��p�����ut�H�1�Kl�A~kA͐i����܊TFe<�H�L��+��K�=�-;��=y00�efh�'q��zX8;-��(����,ˤ��SmD�rH0|�5���ou�l"��H�9�ɯ��-_$��jѐ��xZ�J��ߒH���اkuX�`pN:J�M��z��K�8�c�u�E�`]�|�㖾�[2�Q��v�V�[��Y��yC3�#����L��-��;U3�����P���|LIτ��{PK�p�i����:���P�����Jc� d��1�6�[���'ԟ�nK��t����h�^a�}gS���6x7�f��:r�*�_����	Mqb����哇Yz��ܗQ'�2��A	S��sQC튞�t P�CGI|�ip�/}�\������)d������z���sRbPB���m����q���7$ys6_XHmhr�Ɔ��J>[i�=���4��}h-sz �C�c��(Υ���F��T|s��U�}�=r&��~���#��-/y���̳8�p�� 0�6��Aj�T�1���6C���*�������O
�D��)
�7JZ�y�ps�b��O��/v�1>=���[�|�ϼ�բ�liD�پ�&\Nr�T̎�ZO��T�@۴��סH�q��H�jFr�J�8�N;ͅ��#03�H���s;��a�C�[�$�L�BG�nD�d������|W����C�ıSwG�<�c� ����3�t+YA;�|���4e��"爋��8ԋ)��&T@������"�k�$���H�-�����K|Y?[����4!̊���/����j-�2��v�V�A�s\|��Dr.U����J�Qm�����\�p2I	�n޽�}H�Dte�b\��^��cށ6r}�|S՝:�Є��吣�?���VO���'��R��L
���Ց�z``9]��J��a�pL9�3Ͱt]T<o�x��H��cS�R��G�E���2��+h�3���e#�Y�_���'U�Li���E�F2�l�%�Ot���'Gq{�^�D"߇�ZF�oNe�KL�Χ,ND+����O��u�J2g�n���j���{"�'�2}�6"w�8e��O���S?X���5P�vJt	�u	%�;����6�]��~��X�[��W�R��g-�t��y���0̇���y�5��L3FqB�ڞ�b$s	��yy��|b��zI��̰<<�FB˗ I#���_?	�`S�~K�x猧�X�l�҅�8o������+�}�G	l}�T�ި`G���y[@� !�U��&�K��G�n3�;_�|]~�7`���?s��L�t����%��X�����ODMIa��z"�y�{�$*#'u���b��|�a{/���R���Y�_/h�$;�B��W/�1ۆYs���;lE�����(�*5Oy��b���d(yj0KU�&�$��S�'	H�������lX�onnR��L�Y���89����y�R�ת��'��[�#I�mN��Z���~��v�#� ��)�&)j?HB�ߓv�"HH#�z%GT����4���nsʹ|~���ɔv^� ިa�Y3Yl�g���t��$�����OMR�]8�w�ncGQ�fc�r�:�OJ�<�29���\%&`�@��8M#�<�e�1�_��~!�SC�ߡȷ�����^J�!۾���$�FD�2I�����)a3� ���L3����?��N�����	��v"Uٔ�z4>	3��~���9@i�	�Q�^{�<D(��eӋ!�
�� JH���d��%&�T���%����:K���'���[��-���ha�!��(2�)�n�j�����A��aj����bۗs}��wX��y���A�T�o9���8�V�L�V8}ԝoR��z��\;}}D�I� 9+���?U����o8�h��'#E)���}j/$b�AIk%`�$ ��/�q\ +����N�����o�b�6ު�Ҩ)~D�r:Uc���匞s���'�%ӕL�������7�}:2�R�",��f;�i�Ys��B߫"@�J�CK�g@\��LXi��=����?��5�s�\�R��Zm{�V�x{����L�TuƠZ�Fv� r������o���w���Tfg3a����]9��{�P�f���S�q"���[���<EN�V[��-��"���=q�`�un�F
����Z���z�!�UvJ|���,!(w=w�|q��ȧA�C{�*��ο<�qr��趲�b�0����p���}�v��L��w$ϕ�eS��h#��'����~�1��2�g�i�����I2�p����p�k���'�ٮ</óta)���a�3��MJ+ ZJp���9OeA`�B�̩TS[���y�˔X3�[�����ج��Ƕ���i�_�tۤ�ª�d)�e!�ݏ����ż��V-��X��$Y�{8n������8R���n�F�ҙdW屗�R���3�2�D������R\nd��D�P��Sټp+�C��#t��r�S��e�|��'��. ���ٞ�<%�]�&����CA�`���A�Uȫ\(nbyy'F{�������]�$��!�.�(cq�p.#���牤�Ǭg����c�S��(�+��A���5���fF?�y�܆h����6��FB7OՐ\3ۋ܆�BJ,�I2|*	��4bEĆ�Z�A؏=�hg_�J⽪���5/9�t1��t���t�Q�f�9v��%]Њ� E�kB�ar�Y^[�HH��
a��3�yC�S�N�_rRRPZA�p:�^��L&�3D!��A�SP�k�a2u��vY���Nҭ�8����t��lҸ����X���z�f��/�&���KW��ᰯ� �P���|, �i��[�)�J�&a�V��(G�z��N�UP����O�w�^A�7��Χs���.����͸`R㿣��t���Sw��媾�K�K3���/{ƼQ~��Dre�Y���R�|S�K�� z~or��a����L̤�����ڄ�cc-���x �i�лMWǀ*;Tb�.�"/i]q5B ��_!�R��!P^��;X����ƕ|�� ɒtR��@�����Rf=h,�5�U	yоU�i؝/a0���Z    Z�x���\$GjN�C#6Q ��?k��V`Ћ"9�	l�f>�ە�`k�Ξ��D�
�.P�B�xm��s����G&L���!�Ly�K=�� >�-,EU�}%�n.�w\��w�e ��?�2,6_�B������H�&_~�i��:����b�^�ǣ&z)�|4aۜo�,5u.��$:�	G��ܒ*;�l�IL7�z�|H���3�\)FI�6]RMm�R:&���͸ʜ�f��$h�Ȁ{��z\�G<1��55�CLȾ�p�����Ȼ'����O��5�m~����NxK��B)H�|4�x��Lا���]�l"��~��,~�Ѯj��X�ϣ��-J�7��*��'ʥ�ϝP�7L��1+MmNAe�@K-$k���Rj�c�����8�y�����ȵ���.�����7a���AۆXM�R��;t.�٧�"����y
��29���,��#��GM����\ht@|�\� �L������e�p����1�9|5�h� �m=	C�����\�k)?�&���_��q�)XDҧ�Rӽ@o�9�{��r��2qJ{��1@}~�a�@~�h�4�u��ՌL���O��T�4"��w�.�H�nب|�2h�`�9}�y%�~v�{ JN����U��7��)�����A��?��7��|�93���dO�k;i���8�3� ٧��"��Z��Χ;:a��Z]ٹ��6�Z�:vV�Xɛ� w��^S1�"�О/�*�"t*�B��ŵ��μ�|�{r^p+�4ӧ}h�`y����gJ�鲦�%j����IQV�4�3�A�]�9tB�=ؓ���"��|~S¦|�L�;E�Ѽ}�,?�F�-��j"�B#�����W��J�#�p�>$e��<�iF?:��1@\��M%v��4m.�B�	�lg#k ��Φ&���1_�_4�Mf�1ț ,Z��%��4w�Af���p��gr}��˸\<o�>��6�|Ζ��r�P�{�F811O\&�0���@��L��A��V�&_��txRUp?��^�>G+��D�5D�U�����掻�S���L�N]��j˹ʩ�L�f�'}��2e"��BN�3_s��=�r�hkw�� ���L@��=�c)_�峢P��v}����y\[7�ALq��GI��9�|��y�R`�T i>J������)ʁU���DD�J�H^N>S\�S�me�ф�2�T�p
�+��^�>�v�����̧��&��2u~?�s�-=����t��wki���T��⋒wVe�����O9�ר�l����=��8}��J(9i�NA���e� qOZf��4���[��f#��
ifN�
Mr�E9�~��J<9S3����),Je��N5!U�Z~s	7ez��:���Ew�}ʴ*	�3u�Qh�Ɗ&O�{_
�$>������|*�g �q��c߈a.���ْQ���M��ZMe��5}3�͖���u�jww߷7�ӱ"	��fOhR@�*.���T��.�&��t�X�-���স��'�,ݯ�)O�{�A�0NH�.B]�%��&.Ь����TW�[���[��"(�/�,�NZt9$T���K)!�@Z+Z��C�*�!�q?��To	�7ќ:o#����f�fO&8$>.w�TO������
>�[�t_��7�#���{*��Im'K���T.v0O�5J={������tq��Δ�2F�cnƜ��l#��[�x��h"p�S��\!| ���e���.�z�3���װ�:,�����7��u�]�<Ǥ���Ko4��2�V,�C;d.���e���u'�Z��$l&$�&�o�sno4���9�ILה�J&�ǀ��Rk��J['�*'�,��r��[R>Qp�H���F��U�5FR{C��4���,\�<��\���C���M��|��gB8�K:���蠟���ؖ(/}��b��nAkNY���R�N�K��d��Q$����;Nk�Q�J��	jQu'I5�%��V�m	�0\��Z�w���۩�YT)��`U&�)8 [z�Hw�^e:xNi@j��E09���B�ַ4Ň��C�sC�M�V.Z{w���J�]s�\���@_�^{�!P��C��K��JI8O�Z�E^增FhM�SGiɉ��13¡�V70��0 ��H�'F
;>sJ�s�f.=���[c%�y��2�W�p�CY��0�N�G'����YK�J�i�RV�=�ƴ{2AE��H[;�K�I
����^�H�lRWg[�����1}N���xIŶ]z��g"n^��Q��ϔ�%�@�H2��9�z�4�}��QބI���d09\���v,�����_��i�s)�XS��j�Pk����Na�N�i�Y;���/�����;�oa�,�X��w���딛�[�S	J9�̉6�Ə/ȋ>��Ɣ���ˏ��:��ͤ��X-� ��s1R!>Ij�2?�S�%z�Ѳ`��E���1|Q]�[~mMOM��3;��%��m���|��9�SQ?׀eF`��>P� a�P�	?���a6���{Ҋ*?��f^6=�_oZ�I�WapY��� �lTYy��@d�����7�Jpְ�6bFB��q�>�ۜ��g�h��gٵ��8�4RE+�q���;�~H�P���N��o�+��\|�GG� ��穽ⵕ6Z���2o~q�n�����Dk�L�1��&�[�|��>=i��h��c���j���/����k�4���=,��I��_�kE8u�/�N8����S��R�L	=��|jS`&5lOl�D3�0�s ��K���.�b��� �X�$Ѯh���&7��j�@HG�(~8b�XP�,:���܃��LC����m蔷����֘���׶#�|9X,�j����kL�SW�1hY�����f���v�:�m�0^bߦpt��{%�|�|)���쀙�SF�H�;�)�x�/Ûj�^��i�Q
O7�F�-<�-��t��;Y'�,ۇ�8�+\�ɐ���V�1��?6�%�3�$�nh�}M����ʭ��L����%>�yn1�o�c�������)�}�\(d�3���6��ܔ}>,�D��B>L� ��'#���kZT^}�;՛|���N�=��{˔ᙔ��h� gp��3WHiG1�S'�4��lZ�Nރ�\r��}vI<����CY�SA��-���� �E���7�u*e;#�� �y,��@et���e�>�r�N���t��$.SGk2Y�q�������_%A�9�g���}�����B�*Q�=y�~'�* ͢�?J�!�4v��;~�Ir�(��d����z������1� �,����ɭ��7������RW�}`F�\����3���'�91�4)#ڡ��� �I!Zz�����,9���Y��%X�����1r'x=l8�)�ؠC�iՠyVj����=�_.���ӑ�6��{qJ����)�Uc{y&�pzC���/�P�bB��Ne�^�@���o���g�����a<~��5�'ă��|�T^'۳ݴ��
E�gމ�x�[st`W��Ħ���~��hO�"���%�ѩ��&!@.�= ]�HW	<F�gj�\�#-�?^�eGU8�L$�0�g&r�$-0��Z��|܀WQqI9�c�_���`��t����#D��04YLb����{=�8K�6���7��m�a�b��)H�F�~if5�z��� ��a���:8�XB	4��o~�?�E���L�&权��,�:H$D���z�[��aJ6Z������n#�������8Į9lmx���3��酤xX���0L�c��=����N��4!��-��,�ц��|�
!��0LO�=!@c�YR�fa{̥6U�̐f������r	{���Ѣk���������Z�RK��Q�����$�s��	��=U��f~H�����G�1����=���h�׮�<y���g�}����L'�t?��󛦒�I�X��A{Y�Dp��*
�˅���IV��~�������(���J��i=K5����FK��G�\\�=:OjK5A��	U6K
}�NģXb4rr�k�qa�}G���I�7�T    u& �7��.ށ�� ����c�R��\���|M��!�p�sm�>�&��$���K0N=iݖR�w^
U��~~�6��=��ۋ��[n}���v��{�-�p�3��N���k��	4рc���Y�x��T-JT�\Y���]����Z��>�)^CQ��Mw^�K:%!s�͟m�n(�%�2�M�r���K	��� $7��,yK;t%��-�v�EI�oI�x�t�u�}P���oB?Ϛ͘��U�r�ɪ��WN��f	*��H���.���)��'asP��mo��89V�y�[�FL���h�d?pNWPD,��A%�2�|��x�n|�E�c8{1aIe���	�y���T2���Wun���J �X�|�hBqC'�Cr��[!�ܨ�,�a�v����Zϧ�(f���Y�N��u�-�8K�sT��泌�im��&�y��}]�.
�^#uV.e�A_g�����F<v��O'`ۙ�*M�o�(NCb�cO�z1�� �ƔҼ�V��n�C���8�f�o*1/�w9Q:F�t�F�ɏzK�$�v��L|7��Z��ģtl���R4���*;��̅N���C@*ӕ.�!Ҳz�|��l~���g����K�y��s��6	��~Ƃ�'gz�������8�\�k��u��Y�B����q1m�?{J��6����S����>�Ń�ڢZəzȺu��dv�|�
j�S�1m�~K�ea3�`��oRP�"objS�wI�Ϊ$�ͽl&�Yd��%=,�^�)�h�h��q�(&��-��	͗$;o:�8y�^`H[+i@���6�5�U��4�/�T�у|G�"w�.ƻ�	AT"��.���׈��Uи�A�������NK�ta� nE�D����@6�y#;�?O�>�,��gq=��츐�G0�
����}/�?O�m'����q�\�9��D�Vk��=�x?L�=��+�d���e����ĸ?�G���NN	AT�j�V:�+��|d ���:'`���T;hP*ܔ&X������$'d�?n�Q��<�QҊ\�j��B' G�u�l^6�xD�FJ�8'Á>�X��Z��!U?�n�vU����w0��*W���S4�ǔ�` �#pvL�O�cd�����^�Pr�r<��o�v<eѭ���>�T})��^n��3aeH�8O(ct5t�}��bSf-�f!��}˪�*f�T��мLu�̛���ߚ�������ff�;�����fY��QX��8bz�H�L��LZ!J�RXY�$����40�#]]���fC�-�$�4,bg=�$\�Ԣ�~ԩG��{�Hs�ȹ�ob����4�X��K^�͈3�����#dxP�����Zf��X:	EI$�G/B�v=���z��:G�?�����	_���"�����)!id�<�Q�$eE$�P��y�A_+��4kM-�@x��L���/w�|�h@�5�~g���]����(��y);E�\%���{����_ɲ�j���V���n���B�F�r�I�-)�R��Q�����0U�R��
�����j��u��0���%*��Je�R0�Nt)V�3w��/�'��6�@��݂�Q��b�7��8%�'�&k�����?~zX��~���2���Kv`\*��� ��(��^j��-/:0:���w"d�v*"��z��2v����Jn��Z���Ӷ�� GJ��?�F���p�	e��%'��g��w���!����Ii�%A�䷤��e/�Vm�4(�ǋ���{2Nr�GO��_�2WL��sp�@��f�,k���#Ui�T��q��x��g/>��ڨ��ި��K3�7�+~/��k��qt�(���%^���yd,�$�������Jɽ��x����~^R�r�1l�9q��;��ZG��m�։ߐZUJ�����+�rm�ni��j���Y����kB���fQ8�2���}$��d<Ŋ�������0��������S>˴�����@��O��G�t�O+�1��X39���=�.��f�A���&������<d[���ĄG�q�3Zi#>끟~�Y�n���9�ˆh�EG���@�O쥧Q45���9nB���KRİAj7ՖWS�M�m������(����Q�x;޵ju��6�1BsG�ʬpvwU�39��$�v(�T��Br��"Ѧ��M��I;�c��N���i�W0�����D��_�Lϭ�-h���9��Vs�=�Ej��(�=z��q�d_�n/Kʴ& �qч��C��/��n���w�s��5F��h�����1� Y��&ɸ;�έ����m9��%��?�H�3*S���l��4b��nh�8�Ի<��O��)W�0FKQ���x�"��V/��7�~���)��tE,7�FV1e�����ԟf������y����|�]� ���|hI_<r�d�\�M1��
̖�m���z?���fK94%ab���э�j�Q��^S��c� NK�I�p|2i�u
k��{^�AX���ӭ��)ip��*���N��q1 �~.6H�g.zp��z7Q�M��IEӺ3�O0�iUR8����/,;�y�z9�f�Z�|�����`�ɿ_�g�\|�y!h���Ų�H80�������^���NX!�ۛ'�����~�'�����t�SA`U���|hΠ���^N�c�ƨv���?g-}O��V^�t�x�=����C��s�2I2�j�=e�@��f���k�ړ���z^�5yd�kN�����EgN$�Ü;�m�$o�;Z��T(%��fg����(Ο5�p�[/�K�eyk�?�L��^�BQ��1�I*�'�[���O���
�	�c)5����ơ�kf�j�O�94r��JG���j[[yA?�y�W�5�	����"�,<����+��T�'^���w=8y�y������P�=���Iw[����6���P�)��"���nV����ׁ.Z����y3@�=o���D�\J��3\�N���F�\�8�aGW�p��P\���?�x��-��S���D��<ふ���U��-_Ӳ���?��O( !��l��k{��LM	�3Z�W@��cdAgb�+k�|���h�\&��nN�sJY��"m�I�'�&19{�w������%�a���g��=�����U���a�峟��iq����a|3�� Aq��q[�׏"j�"���Nީ��¢� K��"�ħ9Ɏ.פG����t�?�ϼ����a;͘�I�Ytꜛ��&�s):�n�������^vm��
�T�Ө����D�Y]�V�20)�䓼�Fی�%1lXA�w����}W��Y:�w��� 7�v���ҡ?��}��8/�����E����٧�%�&:��,i�Yi�/Uf�s#ɕ� �x�ﳜ��-��-Q���^qC��)����|cf�0�6ǆ�V���Lϴ�{���y��5�_�)¨uI�n�����B�K�삐�N������(A9)q �0�L�lYtp�`��:]ʧ��ߒ��aH���X�'~ޞ�i�4�օw��r�qG��]v�=Y�<�j������<���C|L�V�ع\_��CN|�4��T+9|gv_���HӬ.eԂ++RU�V�A�����®;�0 w��m^�OPQΠ����gJa�y�n��0zΉ?�}s+�/�i>�.A�i�$�rNγ�MRB�"��t`�U�0�ĨA�-��VR�([�� ��n_%$�R�z�-�4�O�2�N�M/$�D�ۣ�ا	Ώ+"�c����<렭�&k���0�y[	v3z�q����m���S�L,MmT�ބ^V_�0�����ݍ�n�b��d���˟��p����ө>�ʔ3̙�S^�%��M~ruجOL�%=`�֒��voN�T���7Ly�^��*�ypu1��5���|�,#a
ݕA"�%A��>e�^��%%fl�pSa������r��v͈��g���I^�AQǀ�&[�e*OR"�t� G
�r��_x~ �/E�i��\[S��(sR���=G�8J�y���m/�i�~�����vl��1oʥ�_�ʍF��ij    ��Y#%�Ow��	b͆2�xBM���j1���S)=������8һ�Kw�.Hb0�=�~�#R�ڭ?�.6�ؖ�r���x�{�<���|c&�L��m���o�&[[3�T��r�4Ju1"���J5����Tt�̖���4�7.�J�1�����'��h�h#Rv�����Ӯ=ED�p����n w>(�?Ή��mi�嶡�n���])CQ�̹!��Q��з�TѠSR��W�|�\��NڿUȑ$��(�C5^��\���/���-]z�|Rh��=��6�} 4�ļ�Z�����k�c��<ѕ-!�9=����r)�WzG��|x��$��,Z&�+���4�K(ɉI_v�%?L,~���~ߴmEL-�uµ�$��W�b��`=�(]3�ͼ���Mb*K�����_=���~ҁ�ߍ�	0�Rj�?��ľ�[���ޭjXJ1�ȡ�".{:��d>���n�_&��K}�τ���Q��	%�k �o
��`C���I�,M��D~�}<T���=7��ɿ�vLNߜ��B����^��
���\��VH�oU�H��4���^%�l�2H�%�=�� /�5e��s2@��,�?�ԵI��+�s���(7��e�W�<���$g$��̑I���Z���4���7���ȉ�w_ڞ�ڭ�@IN�#��)U��Zn2b1�h�h�������S���~�Y���$@��Q��z#��Ғ�*�sg���7h��Iz������ɍ�8m�L���.��.֘����Ȓ8:����(����,N�*��N��Q��d���y���|o@�\��d�H7f�>���_��t�Ľ�.MN�2]��E:��z���ޱ�����É���;S�y>��5zR��z��Xa��(-�V���~�/@���������а��i��x�S��� ���2�L,����w�4��q�aj� �\���=�������^V�SA驚-%9N�&�A��8�,�0�M��<�]����=$��%�h$��F_m�$��↛ȱ�
S�ж�y��y��#�^U.�g����Ӊ�������UbR���1�ͷyO��j�L����ZѮ�Z
/f�sq�s���F�u&m�(!H�`�q����q(t�׾�|���tr�ar<�R/���N�����o.�`V�:�qI��W�d�@�d�q� �:�K��I�6�w?Ԝ�&.�hW�������xS�w��J��R��MnyI<�	D��K����N|�#_�K������ב:y�T��3լ%��
�T촌�s}�2MW>�f�
�lf(@[�+.6s��]E�I���.��[͊{)s���H=��ޚ>y���)Ey��C.� wO�������!E�$m�Qa�[y����� �� �;@W��s���{�;4Te����=?&��NQ }}��o�I�U�GŲCzL;�����n��]i.�L�=-]䎉>-my.�^��oԬ��P�B؃;(�e�Q��O�"�H.���B����vF��i���&���xX!ߐ�;y&|g��΍����
図JJ�4bdV�j����m!zW�}<�-΃
5���Ւ�ISv#��VQJ�珞n���|Aְ��G�����K� ��F�'�1W@ j �_2�����c��Rs����Q����;��J�ȝ��'z�!t��|���N�)�Z�q���ȕ������m��*�	N�W��I���U�tO"��PɬH������Ċ�J���j6�kj[J�%5�|clt;SZ%�k��魾��	(�@}��&>+�tH��Js��S��D��3(V�i]�|�5��5�
�I�2����d��+e�A�'!0!l'L'
��$��Dۤ��%�>�� =Ԧ9��w.K��k����&�
 Ih��΄�����݂%�US�頟��\A�����7�c�N���HS�~2��<�]<��Q\|�����I�/��7S㗝�2�+����	�^���i��~�5��<:�܏��d-�9�pNC28f>�26���1���\Rŧ�����uY^S�*���kPۖ�f:χ
����{�;Z	�_[ʤ��|$��+Eǳ+���D<�g)�?P��M]R��9�-_�����dL�3�aAi�Հ����½{�#^3!=����<%:�ףqM.��<���Om�s�*V�w]�Y��k���(�n>>���TB����Lw��n��lN�Egh&��4��% ��ME��O�j����m�L�o�Mմ5�6�o���d����z�b~J�c��Bz�K�s�y��<6a���9�����ؕ5uY�حO(N���xKO$����1�F���դ�Ǥ!�$��sSM�ysS�~��-�g�B��f�Kb
|�,��J�s[���$əg�>95Q#y���KO�����ܺ�c����l�#��������������´b�`y9�(%垸%��bM�%+l队]�����H=uۂ@��(!8�����}*6�׹pyf	�M.r���0j-��d�=ŝ���1x�0��Am�,n��q덠Ls?�:O��)��udO�	>KL*�p�(��k�x����.��J:�~��O��=����٨�HL�k&�9�;U�����Zy 59"'�L�HĚ*M`�����S��Y7$�ƌԦ���0��!�͵�h�o��r�	>lP���R��9���IV���I��J;��
���$�k` ��e׭�\br�L��$g��貭Hpd}�&Z*�^�K����l:Nn�DjS@J��\����L��Ė+�y�h�'L�������b윾�V@W>06��\I�	����|��_*����a�Q�v����9�l�,�� ����l�w�h�w�^�G������!�e����
�؈ZE�l��G�5�n*��eNiI��.�	���S9��� ��=_��T}d�=HQ�p�]�`t�s�A9�Me@n�&��������̍���`�iOx���wJ7�/�\r�X#i�,���de.�:���$7��L4x�n�Y�\6aJ�;�"��΅o��P ���ԈN;=Ӓ$�"5��]?���7�
1��"2<�.�/D^(`َ҇��lL~�
  i��\?�q���{���KM]r����J�9orG�"v���r�s�]�ǒ�7��H�����r��4$
tOlZ�3�W��� �����y7w]��ߙ�ܨ�y����o��#-�����u쿃n�\����F?w$U�B�b����נ⧏L����N�s�-����T���:��4!�$���!*o��d(�-�a��`PUL���y��>'�0Y��d���!����0�'a,ؙlֶ;���T�S���4�O�`v�#��ҽ~!��K��j�J�M/�>'���I�/.VJT
�]y��?����{T\��?O��eK��.�ko�ސQ����:������\����3���$�r��e�^o��mJ]��I;w���G�/տu��a�V6��A��HNM���Qj����SgeJ�8��jyc!��9o%|�g�W��$���w9y	)�rZ���Ǉ�{ג�L�6:ĢE*d; U]Gb�R���J��d)}\�D����_�)�q�� &���-�6�_~hǔ�P�u<��GS�)���;�K�~J9��:&��)%���?�F�s�I�I��IS��ۆ E��A5壒��M/�l�T�K�(m�<Ȗ���d�i�+�Y9xC<CP�(<���Hs;i�i����0g�i9�Q>jI������(�Rxb����qp�[,t[�=�S�!7w�<�u��[#��qN���o��vLiA�`S�1��aq�f`����觩�l͞�,9��`��P��&��n��J�������`���R��Ӝ��Kz�^,�؜�]�Z8_	L��(�D����D��5�����:�"��Z�������|9q����y���[���M��R���R;��(�0q������ߚ�AE͔��Բ]�
�#D�e"9v�k[i"���g�tv	Cs]-    L��/�,��{U�=�ebq<�D�?����� ��k< ������Sb^嶳������e���r��� ��D)�y�(e�>'��\p-^V����T3~��I��"?�&��S;'4A5�^
/!\#��cX�S�0��R�AM���}v��DP����a^w��[ڋ<�-���7�ul�-�s�6���(�><��;�40�6�����[�\���4{�O�f��
�@��	��>+|D�$��\#r�=��L�tJ���@��:�mo���3�O��`jE\ȡ25��4�ڿi���Q�X�9�1�j�~yy��,
�G�ܶƂ��ڏ�W'6Y��L����V��#0����G]k>��ð��V�G[�9ܩ�sG�d�L��i2S��ͽN���fϞ�Oî�_=$�32M�O�ҋy'^Ľ%�0x�)q���.9��8) ���5��V�=s:Ŕ��%� ����D37�n�a[R/�m&=�BY��r�'�k^�;dV����B��}{�5��{�6/*o9�i��tv�V��Mj6���i==��_yj,݄�jy�+5��R�-���J�D�v�H��}ӓ怗wpi^D�d�����eA�&����Lm�J��	�� �ɩ�0����+�M�a�)y0�\��Ô&a�*bC���=ׄ���\��H����KL��C��r��'�^VWJP�ƽ��0 �|�s�59��,���_�乳�0�6=]v#� ���b�P(Y~"���Y~��t���i����L._Z^i5��r ���XD.�<��XJ���V_H�X�� W&���ޅ�3��
 &���11�9�4ln�y�+M��FP�ƺ�6�
��3���!���%/7f�	UK��<��5	1�hi�K�%���|\PΏ5ma47_�tl��tT��9'磡��xK�F��p�	:������j�:	��?4�\���������7�i����O�^v�,|��dMc�f��@�Ck�����M�H�����' �I (h"jy}iKӏ����M�1fHާ�U>�ȹ��\�� {+��:B�N��}��w`��!I�x\��k����:7�$���Vxd�7������RS����ղ�L�1a�&|���R�آ�#�>��kr��BTv� �H�|������C���Ա#��g!>��Z#`��{����\��U��ɚ�ۄ�#�ƞ������ý��ϱ\�}�%��l�`W@��xX������3]w�b:3Hwz!�~,^��,�g���<k�S2��b{��dz�S��U�x�q^/�@���+#?��_�YtHi���3�`��Ҵ�j"'��^�������Nل;B2�`��l�fh�Ä�K�F6�@�㛟���b(ꚦ'6h�������E��V��~�)�f�z^4pH�g��qKʉ;�Nq�23P5����sQ�[�zC��B�_H&�	G�>)��f�[��^��+f�������L확v!<Sy�QXI����S��׎K�D(-נ�=8��,W e�&�&�$�p$Mt,F����'�E�ޑ�`]�5̋9� (z��({��3ڄJ���(���H���B����&k�@b�/���s����,�Y�|�i.��3��@r�N�+ky�C_�B�ͪ��;PC��qX�%iO��ͱEJ���|꽀���Η��e�˦8ݱ6�H{�ޅ�'/5c�e+tL����d�e&�<nu>�ܤ͓(!'+3<� `ɜ���/�;�OOe169y5�y{j���J\$/�ߣ�Р��e'����h��C�h䌤��@���+E��dh�c��p�{A��	C�3;O �ǝWf���*��`#��|��̞$Km�S�����Ux�r�LA�j�2ĈJ�M�c|[$9�
5�Ly7#�ʞ�5tS�0L��`��z���P��I蓄��<�\��M�BY_Z���^_��QE/��Z�^u�s8��{#m�<��I�V�|־6��a�ƘB{2/Ӛ���&�fq�6k,�]A�O�p�yi#��A����<�Qlek�!� ��)\�+J簆�� f���ݾ�8ͺ��{��)�٣�?EWg�]%ۊ �@$=�܄���1��2�!H�k&�@z�n�%����9G�[@EEK�ح<����K`rg��k�^�tٓ�sc���{�AԴ�7���nZr&ou�e�V�y�c$LΆ<�&y�6(q=А�Nˏk�'�4�/W���bs�a���~�~�\,o�J�5�4�:��ܷ*W R���w�)񡴛��U�S)M���别]���i3���+*݋/�-d�R.��f��y$�t�T��`o!Đ��`��?q��H�,�*�">]�R
��m�YdJ��?�N���s�O�T3I,H�����n�@����Y����]���O:b{�[�p����WN{��n,	礔Z> �l��LD ��F� x�	qĻ����^���Hf�|#���a�
���z>�p��T�O�},VV�˷�We��LT�j����+��4P,ͦ�I~ćxIpX��fg��K�z��/�`��Y@��a�ʋ����@K�N��D:�:Z�3������O���{�v�Di�>��Vp)�� <��X�XNb���?'>4�{��:,�u�H��iRש(�Mk�׌���K���o�f�f�(����v���?��K�Nݘ�e�dW�)�m	�_����
L��v.A�Ѫ����(]�TQ�X����>��E5���?Ul�{�6�=05�187`����i���n���/�5(������Iܹ3y+��3փ�:D?�V�yo�'o��4�	�I�9�/b蕾F��9c�O����{�F~��<5}c�ˊ����C��׽�G�xQ�6P{�d��{K���k^3d�3؍��>���s��j?-�E�f���]��K�):{^�hX
��1[�d�f�O:�~2s=n�<�{�����b�2�xڸ��,O�����A��o��`,�U�>��I=LT����A0&E�N�yX�dO3gJ�c��Rsh�Չ|���f����3S�����W$�2��b�:��}�RX�+���᏿�m���!�0vai<�$#h��$�w
����	�܅�>�tZ�s36O�H��[�A�g9ӹ������ם I�c��8\��/A�<��d_m�Ԓ���"�I��w��f��J
��"A�����y��an���#y�M���i,�L��Sj�$�D��>񏨜�Qy�ط�~@�w�bI'�gY�#]�R0�f?�r����	����)�O�b3�:-i�r�w�r3�y���␚�-�ˈ�4%[��JH����235���;�$��Y��캸�a�D�%�HO%�fkb�}�-�b�	#pEy�_��F�焟vڄN���fq��-i���������M�õ���~̟{+ߛ}�	��#S���D��-�C��g�\��&�t��Mj���S;%��!E�f��U���l��;�o��H�6��G*�Q����#�o���;�bC7|*S���v�rSi��3e�
�Ӂ���6�J]V��*�&9�Q���w�C��;�ˁo;�]�$
�K���;π�,6_�S<��ˣ���p�J�PN!ɦ�3(�y��OAXS桅&F�ԭ�28Ǚ��|��!lҌ˳�ȩ��3��=��ZShv���Efމ�f�[��Ē3��:�u����k��C)���Jn}k\��7�}fR�4W(y�ߍTx�7V���	���4[�vm�I��	x9d�sˬ���T;,�^����<����)ʡ�!���/q��$�<xӵ10'�]Rȳ������s٪�)q��?��������_/6�~��!�ȷ�K��/wd01��Gq1��5ǿ�b����"ɽ�KU/�R�X�N��iEbP� J���*�n��||�{�"ٽv��[}���j8a%�Q*q�31�K9�yR)����	E�@)D��c��~���6v�v�a���Fۋ��7�����w%f ��ۙ�*�u
�b�˰	��S����Cf؋��^)A��΍[�7JDt��d-�F7K]|��┯վ���ͯ���Jي��q�qJ�    rA���s�v���.�B���x�f:%+�t0�=�2�`���4�^���f�8�{���)u�T&~D�	���Y����Q@����{��|v: $+��,p���e�M�?�'���*dv»	3_@rߍ��N7�"RMn']}�^�~/G��T���'��Rw��N�V^��#b��4�D<��oe�LS,�]�(mꤘ1�.�A5�L�L�~Ȗ���P� 't��0�W��-�W�M ?�����J���AీŔق�����9ߠj$���MW�
� yM����Le{�B�.��W*>Vf���Yc�U��k|��r��)^�^!�hiC��R�?�M̷C�4:�E"3�DX���6�x��}�����ϗ��fO9�� Mw�\��?��*�R���H�y�"`�'���볤�S&n�G�3�#��3mf �5�yڶ�O5�'#±k�K|�c�̟t�1�2a��+�DN�����O9�NA�>�K:��y���Bt���`�oL��]N"��	� ���7I�ǝ6����|%y4[�#���$�%��M����<�����)��|%�*����y��S�}eG�L�+��>U�OI�>84o5��2�;��4.s��i>CC���LJm�(���C�*���Hq@�}c=�"����.7uk���+	e��Ճ�ч����9+���Rv��7�J������k����I�<�K�K١U��4@���ے����-�G�������nu�ǑĐڧ�:R"�W�4����0LQ��.{�k#�5q�;� �Q��\�D���Z���lޘ�L�2��@��Kd��K��e��@)�[^ETد�<�[���c\�ڄ��� #I�"��ڰT������oG)(^gI%|������RM伤�p�/5z����LĤ��29�+�~OXp�)�Z ��o�@�og#w^�n��,���ר7�p]a���F.���R�&8%�`����� �h��"��ڒ�շ��A#�YJ=A���qj��Y/|F xF�Џ��8�Yå�,r`���4���.�i;��������iHS7��t��O�P�E�
r����^��+^]����6���~�@��y[-1������[���Ү������K��A���./�7���u�	hR�%�	�����>jqȮ����G��݈i6�%n}�k��%��]5��H��o��A��.4�Z+F|���������ލ-�~���|�Fh�n�Q���[��L�?�9u@D�1S�ޓ�\2�լSY5�J䣬���t��$E�G+Cat�<��&�4�b$
�Xap�)�?��/�nUc� y5���E�N��S���sb]������DKk���)�,}R�R�^�ٷ�_��<�O}"��^���u���?tu=-h��Ӓ�gL6��Ϭ���6��D��\�Ē�ͅ���s���6�����F�}��R��b�hcKM���>o����,����A��3ip��:�"�0W�I�����`�ہ$I�������s�h�Pb���sRK_�"���+����JK��T⹸K��+�p�G�̖�X5�=*I5�4�3���z�)��`� ��*�$K*�r!��²$����Nن��:��n��Ϭ�6���9a�K���T��׷��yzL����:C����rb�[�����)�����d<��P#L�#�E�|%̳ںy6�� ��4�T؝n�_�k9��	*_rq:��,�̟&0�o��A=��v���UFEyR7_�԰{M$Iⰰ�hl���fг�8����1��MkO!�?������^^��Z�x9]w�x84�6�٨���$Ǹs"�߃�6��x�.���a(7�t�����\�/�`���6!s疘�C8��;��� @�k�t�g������R
3M�KbH��b7�`w�ܗ~ ��3L_�������}3��-�r���W���$a��y_f+�9'6=B���&lC�z�-�W7hD7�)�mF��FR;ɐ��67BX�g��z���0����e�uk��5��N3Sp��)T�b�U ֻ!�>J�ݩ<,szS�����o}��JRS>9N�OJ���I`nBҗ�cLܙg����DN��>����;�z�d��+���$:'u,0���mg�Y������H~��A�v||�
ҲK�3�'�mw~Q:G}և(����qN�o0T��K�5��H�jx���~0���ӂq��[��(Q��r�ΡCI�gX��6��C�ϳ��P�3�6��}�uv^L���)7q���1y5,+��|.�)7L�4��� Y"s'��3�ᣨ�MH5r��֚�6r��g����3(}���+%�D&�tw�"�َ\�Ͼ:U B/��T���g(���)S��K��_�y4;Ba�[����J��id�tuҿ̃}���-�t7L���n�7����~9v-E��=w2ה.���'�%���.�
i���A���m)������c����' .D���V�&j�;9%[NFi3�K�R,2��8��
r$4��2O�D��'��
�4%5KAǜ�=]zr �wZY�t��3�
r���&�� Ŏ1{�}I��L��<�s4s�Ǖ�Q9�a<ȁ3��ɘI�/EpH�s���� =Y� ���;F�}R
�~r���}�l��sDU��V�*�"RH�?�O#j��l�i��Jpd�G�+&R�fM�x1�=�
Gӥ��X�7�jΐm����4�S��ɸ���E�-��!��|��^�|KǠ��i��t��9�Q�����׍����PQyLӴ�=9r��kC��y<�a!��^%<��r���tt�b|'��U�n�$���.mr,L�����+�jBSx(w"�peIx>��X�%M�kf������f*e����w��V�6x���nw8u�Lf�!�C�e>+h$ �{�������Z���D�LR�%:�%�򝑞)
�@��S:!�v�b��͏A�Cȵ�|�.�m5 p�+(���1����͸rK�V
��A���Ml��]w�^�?g"���P���fa+���� ��Y9
wrp��տ=�Wj�G���;̵�uD�R�?d�,�2�l02�;#��Ƈ����=�E�E�yL�R�-��I�~��=�]���1��I�C�$���K�"ca炭N� -p:3,����*JG�j-<�65�,��a�/`��z��ἄ��7����\�Yq����H���~(��=����#uB��4�O♺��)����8��. ak��F ~L�n�H$��^Sb�
(��.�=�J�:.���ʓ���i�:v�\�����PE�K����v~��<ݸ"�_��T2+UDN�m��btcs��{%ӽԦ�揳��tTޤb��
��?; �R�l3u�{~��o�c������,�		�cFX����岤�d,�l������އ��&��x�=#��=���p�,v�5�냢[RQF�\S����xY�����`.]=S�C9��Lz3��צ�K�U,l
'	��	4��1��B.U��қԘ'l�.�U��9�&;wG,�9?�5�翟�+
Z���ẓE�s��l��4o�H��M�%��X EK^�k��Zx�߀:Se���!��Z�jx���K�ͅ!QJA���)�'�˕����P6N�}k=������]�q9�F�'�5�c��|W��vm.�0<�5�����ɗ(,M+kB�Nl P&������5K���]R
�̥�&���$-�
�;rZ���}ug�>j�����CRLP�"�s�xK�"]���E� �9�	/���E}heH��b����F$��O��W����9�;A5Ew"v~�ϙ��g�]���S�pzBD7�4*����S����L�mS�i�xo>uil�D>��+�����mS��u��AӖ��UH�zs������d!X�ŕ�Z���)��u��O'*ȴ~x���Gnhq6���Q�ϙ��
��!�!U����7N�OkNa493�n���~����;��h�e����$v��7|��m[~��������ڶ�\}�?D��"���h�/��M��ל6릿���-�    ��p�i�-���r�X;�V�����u.�]��TK��١�J�����n�I<Ŝ1�$پ"�̤��֧(I�H �8�]:#� �)��Ĕ���~sg��(�`����g�Z$pQͩ]'(����	I�}	i�q�Fany�3�V�$��w�8LY�x«�Nߛ�q���:E/T<�eg�7h������V���p6�iDtQHs�;Qޅ̅C�]��k�fX>|�	�]�p��k~�Iԕ|{����^p�i�Ǔ��pE�<'\�ԡ�h�Ui~�·U�ɄK
�S��C�^,��j��0砀��H�����K�����
j��W$��MFɝ-�ǳ���� M�LTS�'Uy��@�1J�rQ=��g?��������d��y��2��9�i*�-�+�(��S���o�9���Fj�ą����W�pǁ�h�y//C�O����~�k<`�)Q�*�����.U���OIq�(���H�ӌ(�n� ���F��w���ƺuɅ��������D��?�Ek�@@WZP�_U�V3��i#|Rd\�I��Y�|���į1n�"���Q��V�,��3���u^r��^ڿ�x|�]��Z�Z����I4Wi�Gձ���HS�'HЄ`)c�ޛ��'��y��Uڹ{��$Ră���I�;g2����Cj�p/��J�4�	�;3@B���P�U�5qfO�����a�(��g�zD=lu�-}�����4o۴yJ�y�W�O{z�La�E��b>q��f���
�ӈV'��~�=�����9F�Rv�npH���8��w���!���`��Ľ� Z�Snm0�O�Kv��0Eu�(��4�c���Wߗ=u)k���\�>���c�)#� ���O=o\V����$���f�n0���r'EK�iߨ���08)��X�;b�������q�o���^$5�8�x�&��2m\�S�/n�LORNp��W���M^[�oJ܍���1%5���7��p�CM��Ù9_��I~HJ�`y��+��O(��}B�T�{'!�E��i~;:O:=Օ���rh�#���)����-
>֬�D����Ǟ�f�X���$��<���0�{Jax�PFֲ�;F�	;�ŕ�A�{�����{�A�K�`Os�����H��T�k�$6;sٴE��&��kP�t�𻪘"��NT�F0�ĄMIg�i�9"��?ŹN2^_Г��u(��dٓM'���=��!��|�zZb�scI;�����D*�#MK.�u��0J��.*�
r�q��$G �OK�zĘ�dҗM5��U�?xI�ԜV�|��e��}:��r#c@����u.@� ��.�-��6%I;M���m�a^|i�n-)�A�?�ʟ<a� �R�z��a� ����-�blrN�Y�� r%x�~�$�$���5���11��>�V���Z�l�_=6��{v�Ia���bs3���\O�#�%�qF�O�[WA�~ӕ,�*d����t���c�s:�>�\�U؅�����Q�x�?��M��~P���c9"z	P�����o"E����eLX���b�(0�O�O`Ik�-��P�+{�M�=駆"��G���N���1��� ^-�.T>Lj"�뇬��z1L �����|�D��d��;9epHX`:r�9m>��Q�+L���As�i���:�4u#s��an��&�R3y����	�%�%�'�Dw�]Q坃zq|+|��&��k����W�\R���x�o#K��i�X���|��w�-�H�Jib�R���"ɟ[��*ݼ�(!���),a���6p�Ҹ�3�iJ����#�� :��N�r���R�h��6�I�۹�=4�����R5/��s&�Yqz3-�P��i���^s�<���txU�����b�
���Z��	X��1�M���R�]L�A�1����y�&Vs�'3��ysG:,N�T��Pms��@uYm%{LX3����-f���J�,g(���1� ��6�Qfj)�� ��T'�J��D���h-c��4����n4�ڐگ�dYbIN6g҇&CG�˯~~3�|
Z������;Ű��"��O�K^�V;�p�}��p�"��|�b���܍��>{������H;~h�O�6�?���A�ႛ�vm؎��<a���ٞ��F���~9D���e0����y�/�vz dL�5d�섴��(U�RޓWߴ�08^ʐxM�;1�6<��%$��~g�Cʼ��������D�ԽN���w���;���=�_�6Ozʯ��Jz�;��X��|��#�O`r���_.R��-�dbw>�!Z_�'E;?Aաx��vz�n����л^dbL�"U�r�M��k���O#�Anj��F��	�����B,�fs1���&&�n>����"��j�&�H��d �oӗ����z ;�Bʬ%�;�{�3ϛ}x�$h%�s'U*�y�P��rCr�H�5���k�`L �B��b��
L�:/����-�S6·������[�8F�,�9��z������G�9��r��~FyLۦ'�ɣ�]�hP^,]� "ȋ~��~)u�D�<ʣ�A����#����3��G�:��:(��o�h�7�Ȼ��Δb~k�,���}}�p�(o$3�p_x݁��x��m(����	٫�N3Bw(���.&�%������!�'ߩ�	| _*������k򓍌�4��_��U"@xJt�4-J(3��c��Cp/�aTR��}&�?�.=I���q�Rd*�O�&��pM˲�;�J"e�ds�ST�sL0=�}�S,oxh��L��e'��.��8@
ۀ����{��S��������0Q���A^�>,�˔���U�,)��8</�o� �����+S���iX˭�g��h	�,�a2��Pe�bl�ըȟw�N�@�U�*�����L�}��H�F
u��ө�VR%�6��OV�.H15L��d˝v�~e�E�`7�N_�=��TZ�|έb�¥��>~Iz�p:u��{��?��Vc���Vz)�d���j��i�G7;��R]�B{ �A:P��7�py%eS��	�����L
S ������+��͟e�ᥰ�	D��H��犓-�����K�K=[�S��|�uȖ#������7�ʜ��ľ�հ�2�䨾����~�k���U"f��ɢ45��������4�����lB����f���]9Z���^�/���B����4�j��%�Z�1ቛ�!I�GƟb9_�'{��^���]�
y"Ӥbg�5�Xu�R^�0k���y���]�e��e���<�G�?�`,�V��v�L�R��	8��^��C��q��`�6m*�uꏻ�Q���FM9���6o�9��0S*�39iᣖD��z.� U�ywƓV�Ƨ��<�����a͙�+EB�}NV�?;M!e�S>�)��'��h����?@m�t�l3�&�&�r�pc���x9a�Z<ܚL��9q��[��ADH���ۼ��n)���+ЦT�D��7z�9c�x8M�8k�:m�p�o�iؿ�I�L��&*�K=�:����]�0j��J�5�uZ�Yb���E����ؠ#Ϊ��?�V��d͛�n�xca���p�{���FMa]��� ��`@P�=��(�\z œ�N�M�ֆ�N����ɚ`�@Q�%�j���yօ9Xwz���'O�6�m��S/�O��`��D�a���=D;�;Aـ�Ma���t\�z�35���%ٓ�С ��&(�$�� M�~��`�r�xv�H��>����o��]�Ɩ��dZk�������y!Ɠ5��so7f>���uz���퓗�����ԁ;�VJ���ر���V������H�PN�x-�܉S9+��+�3��To�T/� �"9�r�}�&�K�RSJ�+��|s���=�*F��)����j-n >���梳_yW����a��^�T4�or@�S���$����Tޒ��h�YP��= ���W9�<w(}�a�n���I,6=3��Sz��K�A�AX�^��	�	�y��6z�op�3��5�i�F�D3U�t �.���-&v̾����+�"xoy7)!�"�E    ��ݕ��j��BɯԄ�ȯ�����gT�z�d �+E��p�"�"q��O�Rs�A/'�rp��\���ZH�$���.ۆ�µ���İ8UM��Ɔ�ơ��rz�T6K�#�w�Fg�ԇ�9Qk�x�L���~@�v~�Vޔ;	($	tfHI� ���͝���/7KhgR�
�Y�KU����:|9Tg���	��.�5��[���Z�WS����+R9�B^�U�j8�'x%��㧴6�/;���t%isY҈�����}���Ղ��k��&�0	��Q�]Є��A��L��N!����y�]`�Μ���l��wrώ�hJ�t`�޸�䋘@���X����Ҳ/��)�����aӿ�#4']l�#�6+�{��bV�3��l�rjޔj����9�_/���IkZ���g���.�/kN�T��CP����S�x>���zB�5��,�l�V�mp'v�d\u���oIn��HG��j���P�2O7!�|��<=����IJ �]/s;3�~|�ʫdL�r��<�/S��W�u��YB����yU'�w:�[KjmV�!���ԤdM۴��9u�ʹ	k{���.�ڗ��W^��_R��^������S*��vӿ|�S��S�D���|�F�j�z��ٺ�B�k���?_hP�H̹�F����trh��Ө&
@��Q�~sK�Dq!�6ޗ����E������?��;�C���ѕ/ыX�8�g臥�z ����
��^>l���aРT���r�^Ҥ�/�=-Δ�]	7�*X�Ro�Λȥ-��d�ğz��_�)�5�Fn9y`���8豦H_�	C��D��@d#�}�k�߲���	�w�$�Z�p����9��[�%�t���د�gYӀ�> ���v6������h��bn��]�Ta�iѥ���q1ws:ie��)m�4"
;6��t�Þ�>bq�n���� do�m�Z��m��0r�F��L�X�MG�t���9��I��)_�GV��.wR��v��i�|�6.<+�c.��B�~p�<����0�E�g�s���cσ<�Ib��F���y�ʁ��s��ߏr�n3_�����2�����9���L�r_	�/��CM3	 �P���`\��_T:����CWJ�}Ͽ��2�d-��_l��&e��$L�9-솷Ĳ
yu?ъ�x4��WY��	���h��`q���L�`��LF��LD�1c�LP�i��ښ�$�<m��!��&*g�M?`ӝߛ�+[���K��/���9�o�I�ک�Ѷ�\����<H�(Qr{E��s���Tٚ26a���a�K���Sa�9�z��*F�#���;�Es�۳'�>yӖ����ۓ�G��M��~���kϝ��Gq���V�E��!�:����L_��<b�Z��ݨa�zL Z��unq�>�yC:KAҤ��H%����n�Q��;8�Y˝P{� O���L���8%O 0�o��%i�t�SJ��b�4��V�����ɪknnb�\殙!��	Y��r�b�������}&����j��K��U;Aѓ���}/£<��~?�{o��]��ٟ"������D��~J��b�M�7�%;I�N�/��l�kI�O�l�Kd�^
:���/w�}ش"�?�{�iP���zۚ�f��P,�3O�p6��8�(> ���/�qW���d\+9w�eC��ѵԢv筙��͌����9�aY5[#^� ������Ҿ����%�f��'���5=\����|ʓ�!��j;L"R꽧m�F>˲x=Q�� En����q��a�%�H����{���M�Z.�m��w��FbS��'����P+��J�u��P!��g�(�Y�.W9S�f�Cs�K���gr�y��=/�鯴(�\z=H�|���(���#K�Μ��o/�5���Y���|\��n}����$��F=M�{j.��6��5���o�JU��ΖQ��CE^��;�����z��4v���J�1c�Ձ]��E��pS�4�ǜ>7��\�W�)�
����Y�7���Ցƛx+�%G��9�"��N��7��D����*��uߊ�2:�W�#ؐ�=���)�Hz(΃'��}���"�Ri*1�ꃢ���T���q|Kv ����w�P�X͝�Ei�0�jC%9�����W�YRU>��q)J��D���d��i�οF0�����K9�o#!;S�:u?�����\�l�e�W��+ɻI�N�]Tw�1۬��W�qy�>��nbQ�*7���b�����xR�K9��B_���V>X��t�M�ZR�,|j.n����ϚS���B�$��YC|��
��<7��Ի�/�Z���H��ٖ�׭P�q)"U�dF5�ʩEK٢i�����re:'2���S�0g�qz��!5�L0���z`)�������%sF���r}��#�*��y��K���2�B�/�w�hE�&f�Ó3�ǂ�"m�ԝJ��%)3��!,�D��l��	��*��āSa �rm��I��q�j!\�8R�Ho�5ϗ�}�)����U��t�S��⬥�)�oM�?�FbOI�H���pܹ���A�*�\�6��I�i��m��E�I��W�$�(�H:Z����8����c$�yO�+��|�7�vWD�b���M��9�ղf�̛z÷�&������řε�ej�i��q�1�Ɖ�ҿ�̑W\Z�d$DY�"����[�v��H0N������Wt�ȅ/ ��V�$�������7�g��6%��Dl���2)��SHg�rS��L�>
Y�mnLb�ŷ=�}c*p�IW*�?`Ёq��<e�}��"uww5��ɕ$��æ��@�X�=�}�}Wq��Μ�;����J�!�3)�9ͯ�sfm�Y�߰��rԘ6 g�L����o��ϲ�d�KG�i��.?)ǉwO�
��bR���NZ��*�-)�����A�/
I���:��xi��� ]���zK]��kf)h�;O�O�2�A�|�_��ƒz��ݎ�T�g��QϴCɩ�
n�1+�7�XHO�$GoNL'9��c5�̭#��l�,��jY�7���Ө��!2���!�TZ��{��C
7�̏�@v !��fo���n��J,-]�D2�!�7�{*�"��4-X�2�������\��p� :ζ|+*��{���q35&�g3�K�]�1(�;�y40Ȗ�y!�����}P^8~W�`�B��D���Aі��L{�$������R��X���r�eQB����#Ϫ�H�O��mi�yn�&d%៝ɇO�f���o
���	}�E*�,���u�?�\
.륾G&���	�C{܄�S����b��<xX�ݘ�9%�r��)E�S�#�~f�SQh&9����RR*��Gӛ�/|�U:u�l1����Hv�"HI�)�>��$�i
@$5�9*��G��ڻk<wJ?���ָ���$Qoh��凚�-�^����LU7f9�Hf)G)f�yV]��xZl��FWy�Z�:u縗.u*�c7�<h�/w�I�զ[�A%"mj�� ,��^���N�K����U�;	� }�ẇ;g��͵�c׸�hK���[����6��7�Ҝ�dK�D�N�+��]���r�E2���.��ak�6�ŕ�7B5�)j.6G

�3g��Pt����ّ�R|J.y��^����E�JKS�| ���c�hdN4d�epά5eWҙ.�K��=�L�ss����<i♿/�ϲ�п���s�����(v���@�y�BŹ&(�<���Q��s��c��B�kW3���Y	����W����JL�~��;Z�������n�@���'����{�ܓ�ׂ�F����)��:;���exːD���8)(k�`3Y�t�d�+�޾�G?�r��-��X�� ���L�nR&���?b�����������CU&�+�����'oՉ')
b�@�Cxp��&�K�>R.���#��(�(@ܬ��d��x"\`�C�{�i���P�A��|�t���w��I�H�'���(SL|Y��.����V7� ��,Ph�¾�    𥮜~�*��|4�/ ��j�m��(n�Uٕ"<�8�D�!�/0��%�UbE[��Q{L$ �{�͂�j���s��A�)���T�N�n)�����D��\d�wI�&���O��l��%�>�Y��|�g��+�B��)��#\��ɶ�|#�pM��.g�ۮb�\�ƟLD�	��*L>��;�L�N�ۏ�R<D�x����IQ�o�S��໕���qnO	��I��I�m�_K�����*8��&E<���َ��:+ו�sBc>�^�<����S�3��a��WPy9q�X.$a�%I'�0q`�ДK�_��߱�ɧ�ʤ|Gu�-�P�p����&�����$i��4��7Ӭ��&ܖ�۔*��f���m{��{�r7SA����-8�����x���'/����LSW�D%�_mf\�ϲ�v��T�R�8�����Tә�z0{���|p�7yny�y�C�i�zVfu�S��) �z.�'��{25\ӳ/��f��vrK�7��5�����*���TUr� �D�&�W��|P,˅$���p��/�,��b��Q6Y��R�{�;IW[>[h�L5�BZi<V���{�/�Ei��x����.\d2���B�ֿ� �{�ࢯ����ӹd-�;��ۉf�`a��{W�9VF)��^@�����TePH�/�߀l�~{E�lX@�������Y�a	W��n��Z���#� �1*e�o�Ф�K�&RcX���ũ)r�o]�{�9~�e���L䭭آ����͗��95Vh��uF�j͓���/���R�8���/<���g�%.z� �<U$�(ވ:1��E�k�R�����B�K�{��-]о�>%F$~h*'H�y7�2�J����@W[K#h���Ӑ1��9NK�Q%ǢC��p2������9�1mc�It�"oʎ�Vܪ�������	¦�P<'Y9ar�)x���C[����r@R�t��3�s����$P�QZR��9V
��گt&03�d��T�*��;������=���B�uf���:�|l��g��,.����q��-K�K�T:��s[ٹ��"�cE+o�6,o�����X�s����LN�3"Q�G;V���cK$��@~"�b�Xg��Ԗ4���N��@������Rڢ-�Q/�d٭��,0�E�ɻR{�^ʩN�&�k$6C�y)�2���C�U�1�I-��s����|�a��Wj���`ߌɍ�	�W�k<���#�� ��!�R��D����>��&K�C.�EM~��a~K��M\Om�����e�0}����W��#��4H��N�-�q3��r?]��O�ko���O �E�v�g��7g.G����m�|B�:�:!������Mz"n�di�����a���V�1sOw�F�o��t���
y������ݺ�T�����!�,Ew�N�$g�^p"
�L�����M�㘎�m��yj�����c�&��)����x�#�>m���P�{��9�t����M��*E�Y6x؅0�K����m/W��|��eak����$����%��.��g2돱���:m��Ks�f��>�P �3%p
����y^Sd�����gWE%�2�a9_�.�c�5�[<�e�r�I�9RA�p��,��^xj�ƽn� 8K�,�"/�2���`���I�_����xC��Se���]f�0,(@�r����;�yK̠�f��gّ�R6���!��՝fr3w�N��Sؾd-��0!n��6�����L%"��E2��e�;]N}/V�li��H��ёʉ�<c�i��m:�7����I�Sa���LF->��!�D��(�Ob2���b�Q����������i��w2ek�GW�}��ǂ0=RZ���*����JM�8(�@�� 9.���Oyg'��%��7>f�N��{�!k=Y��1����:=�-�x�-� k��ωR.n����,5c�����[���J��f�%�t�&C��%N�����JV�|@VtD>��/%�=�����n9t�.�t��D��Y!�+ �zi��$�f"���2��U�H�q��TI;X�F'>���=���&�.���A�zeN��0�F@�y����ܐf��� �י�v^��D���hڸ�O\������ U�
�}��:/i����ĝwHQ����x�&1-}�4�c������r�ecV��:A�͌0�M�~,��t�y��m���ӿ��T�Q�0�����y��L�*5��&w���4���L�����,����z�2�:�a�ς�*|SN���蠵�_C9_2E�k��P����/�E��Ό9�0}f�ue<Ph��]P�H-B1g��IEp���)4 @��6y�?�/��5��7yヰ���7�A��"�47�ڭa�efu	x�<�t��	�X��+�EB��;��.�ٓ�Mɷ�nw��$�	�V�@�):�>��d|7Z����S��fpC�eOS��W><��Y���N������@���^C�#��c#��N� )GڊGF��En�F�w�;&CUzb���y$yr��h�n$DH����Z�47=m��`��w�4�MiE!�i���o"i�)�Ĵ�/��KY-�Lt��Ώ�4��WO�=ԑ�)��T���ο��z5֍�ՕZV�?|�f�N���}����n8V��0:��0o��y���I?��)'�\<S�x\	+$ҿ���]n.�҉�&�%����MQI���W���37w���[8�J���jSfB<ȣo��T��p���ʟe|��aTķ!1̃�D�B
�]��u�E�L�}��27O��Le�^�j��k"��i,�m;��m$՟m���)d�c�r��?���f��2!i���(���H �G_r�k�LY�����+Iى�j泙����|~�[lr2_�n��t��˫;G(��cd�y�.�IV��g/}��p���|ӭ{�ES�-�ۋ%�ʀDl�'XXe6�H�ʐ(է��jWy��&b��qՄ��1�
�(x��3qya����p0،\9aN��t]'��7��a�v��?���v�K���L����{qs�Ҕ� A�B��YP����	��[�M	mm���(��F�m�@s�Yi5T�>SQ"p�=ɊM��Cv�6�k>�.����,�.�ٷҼ\s��-'���Ģ�w����������Zrb���gN�/p���cN����X��·\����B�?�&%ϣ׺�@?�p�tI�̩�p�{A\ض��%%Vz�03�:���.�`b���MK>-�u/���ֵ�e�MI1�5}b�l!�ʥ�x�⪤���M��iYIx-eϼ�Ǡ����p�)�@�v��r^%���f�P䧙�39��|P��&��Ų�L��"�n�������}$�I1�ue@�[��o�[�ѪZ�@5OZ鋭P��U}-��E�hn<�.2���K�>o�N��>�9���A*��K?�Cl,i�˓C9��^.5�w�ٛ#����1�|�l��<�-.)��0S�D �YzJ�E* �9g��1¹�މե�>K(�Ӟڴu��Q���fa�.��Ǿ)Y���J����e���'���=�GNT@�<�;L�l�U�-�=�\J�I���+i�m/?���L1���n|C0<?g�Yv��׏i����!�&�OG��������N
;�����K�>|�cb&)��]��Rf!�8��Ѯ�.�!HJ��S}�����,<���!�֨vƴCS8�˄Ŋ{�>�x*ߛ]�cO��v�A��{ �8V�T_�ۛ��*���R-_�Nz��ult�����@����ꎰI@<��n��$S2z:����Po���+Jױ抶5�sG>t~)��:C;�1�.�)��1&��T�&�6��>���Q�	�z�IqmV�Y4El.�e?��O��@u� �~n�9-eBS�����k�LX|�y�u]E5���1��_{׵��NO�R���56(��f�~Φ!I[�3S�̝N��Ʉ#)���H�:��hb�P1���3]��H�Ah7�-%����沧n-T�Q�|�h/��F2�� ��M    њv��ܮdޜz3�r�Ο�?�R���~��VR�T���z��4�I�i� ��f���`���~�[9Sќ
>3�j�YL�	Z?�ɐ$�u*�a�Y_
-t����a�i����&J��(%ē�fJH+��U��ڮn#����[��fQ�h C�b��M�Y�D�R0~�:4M1�����(�g�ȴ�萦*d⾴zi;�}/w��Wn7P�����tF����É�ם�0���k�+6��%@A�A��|��Fy�i����}p-Ld]zi#c.'0�cN�> _���)�3ȽLY;�@ux�FZ�9Fn*��l͵M�u�)�|�s}�O틸�n,�/�ܙZ�
�&�>�4B^��lu�c��[S�g��O��[l������[ܩ\�1(Q��1��1�i�)O	c�3�P�N<�|�4�җ^��H�k�IۃM�����y�1��9O��Ji5�F����4�sa�V�p��fNy�ɱv��3!E��\�|��c�{b�|�&Q��7k9��`�Lа���]����"d��|f� {��}�Y���`d�͹|l[��b r���,O�s;�^J�Cy���L2��n����06zH�^���h�����y�i�Q���O2�ט��<)_+�o�!��-��Ag���uGKp��S��.$�S�q��P�rn�P)P�Zh{��2k]m�7������������y2s-��
 �֜z�28IX�[�}�Yy����f��j�I$>��[���8�IZVB�#��N��R��_��������+�����3�b1��iR�,�NN���
1��"��C���G��R��T.ϕ��f�ml:���<�UԓIE�õ��r��㱖s�����t�l�.:"S��tj���(�<��6o�]�\��e�*��D+.�8�y:��W�7���t�=�9)ۊ��mc�Zv�e|��/�؇�ix}��0�`��?�]�/��:C�N�p����m��(�R-}u �M�~���m|K'�r���[j
�(f�H��.}@Bǥ��IX�a�kSpD%<�*����7"�������۟Yn?�o�i�ɳB�D�IP}�J�W��ݠ���K�.e�A�����2M'^�7��dX`�1�f+�=���?�[D�o߄N:�2�t���X$C��3fHiF�`���|�����t�Ue+�J�'�5�f0=~�$~$)�r���$Y�$�3��O�2��-:sc[���s�t(q0.��`'+�,�Vj;6�en�R�8�Ӿ���DǗ^��Ч���+QuR��q�=��eut�hw�#���΋g��{�`yr�qߛ�1w*g�Tgn�<�ۆi��mAF§��L����;E��nT#.��+�'a�b�́!�`�ځo7t���d`Z[���ɔ)r����Gqڿyϗ����S�o�����α�����u�粣�c�?b�O��3<�j"%�$�w�}�17�2��t�l�'Zit�d��,7Ag�/d(IG�E����t4��q�,��2�X�T�?�\XG��N`�s�G&�T�S`�r�^243�$I��ŧn�sqp�M��0��<���<�i���:������]��h͉S�C�,�Zp{���M1���p	�=���V_q�Щ�?Ɩ۴�z�θ?KvJ՛�tBOjY��%��9]R��\�0�C/5H�|� 6.�"�I�'�?orpW�4A����fB6��@g��U���	��z}����E�ap��ވ���a޲�䔐U����)��S��'��)�!Ҕ�}��M�ҫ ��M��F��{�r�3��f��8��å��i����1>���eZ�d%T�t+����w��,��+��G!� P��3�Z���#���~�<WNk� e��&0����3|�D0�C����]�sg7M�d��/���@�$�a�u��}�\���}l���t�fg�6兦�x�7�ƪ�)lz)�,Z�^)��$��I�l0��z���SOx�d�����\֝GG�O���q���)�8���*�a���6Z2i����~Cݥ뛁�g�A�{�pFI�-�r)��,4͙���D�#���B;��}d�j?��Yg6%�5�çR27:y}�J{6[{�q���p�<�0r�:v�]��6�p\�v��=/��G�0j�0���(��m���5U��#��}L7�c��-���p#s�^5�<?xj�u��;��I�]�SħMK?Y�~��WJ�T��B.p#��QOuеa������W���f�<���!ϱ�C�d`�hM�4��s�o=?�Onq��[��pi!}u�-4J��d����%l��^\�2�IX���o�㒄Nn9(���^��f.���X�3 ᄄ4�����u���� t/!�3��L�FSF)]� ���u���h�ٙؐ���Bw˗��$f��P,'���=ՠ뻱sI�E�g�?�=��b��q�8E�<K��wÂ��x	�ҊzcR���?���[��΅s^~鑣5����ן���r�"�Z������e�E�� *�汩�S忤8@UK�E�J����N�=3�	���n�]@K*������	|߹�׫���l��p/�6p�Rdb�`\|�
:c�|���D35��M�%�'G��ܟ�
a`j��q�a���VQ�5��vړ����RF2e,�z�7E- ��z��	_�V_S�0Q̗��9�];���F.eF�u� ��S���χ%m�^7�K\i��R�_�>'���'Ľe���͢���7�1�+DK��Q�E@�qF`�YE���
C�lʠ�<�[��K2�YYq�����i*ݜ�2,A�m�ԗ1�~zZ]`�����a��>�Z�yȸq�X�$V�*��6�Y���4��d[v���\��	c��h�a,A"�ajM�h���'�C��$!�XWX��B�UC���S��m#�<�'T�?n��b����pK��o㯃P4�:�gD�#���ːȼ�7錜å�἗��wT�{�����Vɋ�YI�����12������k����IJ<��E(Sj�Q�T�I@>��h��	�۝�6���?�*- oy�TV��Ӱ���2������	 -�޳�b�=�'�����Ս�虈��s���P�P-̠����T���6D+ŷZ �]7�H`����xV�}��^��ڝL������=�9��O�k�ԓ�Y9��CWZ�oyl���-K�d�F�9PF�:��L���ً��Ij�0��R
����)�V�78�~�m�5J��Q��9�B)�N{�~��M�g ���F��.��-�),,��h-yFY�����K���M�D�q�,�dR���-���U(�q0K4u�� �S3��K��4��s���y��A
P��6����kZ�S>{y�g�~4"����b��
&�����H�L��uȾ���u?y`֝���3S@�9n���T��5�#��n��Uw�8�)ϯ�P��vo�����3��t	c���W�ѽ$�p. 0B��.�#uN@:��I<|n�r���ܹ(�pr[�%���D�N�h��\���.��P���)�nokus��<X���xQL�+-�2��J�m�@���^bNϐ��J����*|�̛˳����8A���"`r��_6���ku�hf�ҼH\q;V�Ϲ�|�
�� '�m:x]�
�f|0:�|����K��"O��#e�K0}�Q��RA囏�nU�e�c?K��DIU��Ct)�8��I����Cw��3��G��M�oI�J+�>�V��iu���n~��|g[�I���nzP^**�'�0��qp��V$�y���Pܷf�F9�o����`���h��)\h邌`a�N�t7�@�?!���I|���ú�Zi��k�&�3�x�����f��[�����$��<8��������C>�����9�4��='�@�/���񉘦�U)A���R_<��T�r��S�������-Ҧ�NK[�����)6P]�7۹Ė��؉��/�YM�������`ǛMV|��:o�t�˫�C�ڑ��n��i�9���#��獽&�y�4    i ��v�fG��|�絘�[%?A��D����YR�'����Ƕ�WB��+� �˩`��9�	&grU~b����y�9m�w�i�I�8���zҤ���v�e��3�ǜb��˵���VR�X�v��";C��ya����crG����cco�d����.#���]�ڣn'�B7���Qy�&A�n���=Lz�=�~y_��PB��H�^�!�
Dx!��}��E|o)��,d�:)��3ѐ�����T��� ��G��t�m���tl%��s߁gsuj��k�v�(��y���\��w�zy?����,�\_swF<�
i�0>�|,�_��o���q�����z+W�^t�|�d�u+)��~�Sl�x�}��G:�#	9_ �����֒q��4���o+%���� �C�޳is���odo���aʛ?U�����l; �{^g��Ap��k*e]�q��m�yc7�}!#�����1�w����Ԉ�|�o�=+��N}��:�g[�o
�{��H�C��
1;ݓ|��G��s�L��zvB�;�FZ�uz�ƷTD;o>$a����l͓��zgU�w�<	�N���m�ng
l���*�)�E�tJ����}>F5�?��ǖF��hS��>i /�ͤZ�A~V��﯏�����w��e�8c���&��<�1��҆ �`6������z�/��;�|�DG�TW�*�-\��IJ�$�5��8݄��xr�[1�ɨ���!XQ8�
Q�o��-0sM�z���}����:X�n�TZNʳ_Wz�D�6B1�*��훡9���� ���e�(=��(��vQ}�H�D�`�L���61!Ws�j��#���s�$�U��sJ��� e��ڕp�\�.9I�K�e����guJ9��${.��]7�e���/�8�2 ���|i8����Ls��Jұ�T���b��
��N���)���&��Dat�y�r�R��;h1L��"u
��&�x�$�Dm���zȧL2H["��T66o&�����
���:ٹ@h���@��w*���e���eܽ�m{I��6�o4�6����d��qϮ�k7����4����oUoOj�|�������Oc� ��{O)����g�Q�͏�A4w�\I�/Չ�{���j΂��%�f/Iɤo��A�	�io�Q�<�d�[I��~�)��bކT�HOҏ��^���Nr� �9@0���_��`��u�["!)��&�u���rK�а�ft$��+���k�������3��>+���Rq[�Πm]�Cd"�l�i�/D��\#��{�<(^��u�U�m=AM�:%L�Z}0������J� [��[�\R�Kn�Ǖ8��aMb_��D�w�qЍBw�>�"�%���,�%LN5]�B���f&+w�<��f�p.B;Y8�|6�v���l%��m��S_0bY�Ԁ��%�������s�)�$A��+�����;�hJ�+�Ϸ  �\Xި�~�f�U��-�D�)�Ƴ��gbC�Z6���ʣV`\`�eհI�},~[�Z��g�?�Y��i��A�<�T�3tn}���_CA�44��^���!�Z�P攕),r<�X�� Kї~�Q�d�a����g��Gq?�oWɌ'���������8խ9F;{��/�De��T3T������KK?㵴�ҧ��I�JF`�S�D��Te��n�S
����]K_HV]�Nd7�'#���S��K)��l$$ ���:����孶0��'�L����X�X[�>�!�	8��cf@��ԵK��ܛħ4&�z1b+�"�`�󳂫3.~u��|�S.�g��+�
D}8�z���O/��T��ŝx� I���ɟ=�祳JL�o���)��p�$�a�$���2�s)��Z�V'0^�'�ڦ�ey�+�M�|R���M	����?���%�6hKI%ѭ�*�)�x�F��V!ws����3M���9c�d����%r_/V����`����梮v���E����o�3��qX����&��t���gF�r��L#R�����i��S�m���ג1��������|R��ԖQ,�s�2�L��{�q�5I�JW��H���`e�{�i�^t�@�Go�I垬]���¦�uv؊�E0%�:64[����m{7ʉ���Ӂ��!����i�����睇�ѓ�@��[�[�Ɓ����uM��WV��J�n���q%��ܪ3��[Gz}p�:Ѿ"�B���U���'gC��Q?�
��+������LO��ݬ��1��,$��4�7�i*%��X�3G�z>��@ԥ֑���A�)E����.�P�O�����ר�K��nn�'�ȕD;u�Q��<�a̙{�uI��6�$A�W�Ŧm�a����S��#oR��!Z'V��;琑\,ҧO-���Q�hK�j�9�fRq0�oz�x��[�c/ў��_*�ɼ4Q���v�!�8���n�aR�q�}�#&n��-��zS��5@:���AA*~�42�y�;y��@)���Z귣x��N\M���d��_B�L/5���s���t��y�عt���W�y��֌�P��L������Q��Ӊ7$��ݘ�%�
>���5/ck��Y�lx����T��jw�䥾 ���`��I<#��Ԭ��_�'�$�$xIi=�UY4���N�>�[�A{Y��K��K~�Ρ{�� ��Q����'�eb0^��0�������o>=��F���\���GX��)��J���w�#���+���"���)�r�r$�]K���"#	��2~�vF�}b{CX��/ZR.4��f m Ŵ�¹a᫱���8L��3O{/e�4}�r��͐$�|e����j7HC�p�7��b*��Zo�Tq W�^�sRZ���*v���n'ųo,@��0�Z�M]�inO�}����v�i��W��^�ڕ+�8��B@tSĻ�-7Re}7��N�u�Ot��k�2�';X:v�\��-k.U���X�p��p�F���c3���j�i��}$��%5��^W�/օ3�tP$���*��N��P,0���\�/��q����Ml[�tn���4ye��w>���o߉��[5�dw��;�Kj�����T�Z}>����(8�r��,���ZH��W�u�p*OFYH�6 @+7]��I��]�'G#�d�s��?���8s�DF�������͌7��G��)��}]Qe�w*���Ŏ~[�e�'���4A��G�}yH��~/G��\E�)�}M�����nǓ
���[�Ss�(���+N�-a�3�7)(ߑ�Bb:�R��yy��ڈҔr�aƪ.��z��M�aJC����,4t36���Z�	�eǬ�J�:m�{
A �7�!���`����,&>�����b�zc�~bK��H'�Æ�{���3��S/��j�v����ʨ�)�/��`���f��`���F:"�]�+{��d��h/8�AG"R���R$C��8�Q�&_'E�NW��Vq�W�(Y���W�p��4,�Ny��$��:��qӆƊ;�_f�so(<��5Ľb�L����3���MO��6�P�:E�<�A���DO��0w��$�W����%�'h�Y4Z |�`�T���;DOa���#��[%秹��k
�R�����3��ގ��s� �����pO�$׌s�!=T��8{�.��{�r�>MS�e�l�Ȟ�H�.?H��7�����)\3fXH��|,��QtlxV�U�X�i	M�TjFr�I;�M@ە3��RB}�C�Q@��7s��/���4�k �	|}F�����s�|M
�KZ�IO
K7sR{�����+t_���-Oٓ��kx�5��8˨#�$�>t�rx�2`h7�#���;�"x�2����9y�T]���;����g]��b���d�8t�S|0��X�h�657���F�p�����;��\$^w9���Π��z��9�,����ff�܁�F��;��71љ�L	8$�&��N�	��i}?ڀa��'Zz�|*>��w��p�}�wn���b���p��85���f��6��,������T��̧P�TGL,S)g��0������<W|��k�(�c-�$'Ӿ�ī�������NyMۜ    �>��Z��	���a�G�I��xFKM��j�H�����th>q��VW�?0�=a�bCu�|V��� r�K���3�̂V��9�]�޽F�� ri7C�I쩱(��}S:��R���Q�rIHh�P?iF��ߘ0+Jb����T(^b�s!�F9��@^<��h_�8�QYLϐO��[9�򗼟
�w顽e_��=O�4��M�4�I�-���w#�C�l+����w���ޝ}��n�r�0xe�
B�C ��©Z��zʿ+�;�K�Փ��"���ĸ���2ZT�r�{�(9��߈v	O�l ���~��0Y�|	�$���K�]�Wy��V$Q����J��9@��T1��d`P�CϒS`�������C<#�vPo�to)O_E���/�x���_}��V&�9 d����B����g!�R*���<�/���?g���z�i���k�y�#S�O}�|�Xײ}�KWU�|I��j�~�����Kn�Ǻ�*�L-\A'iN�8�߰u;�@1�B�sk7 ��J�f/W�D�H�҂��d�]���tᨗ!���q���$V�����hi0=�L_j�7%���O)�z3O�� �d��C��J�F_���u���?{���[n�O�[��.ҩ��Y�^#��
 �����Jk���Ι���-��dū����]��~q2/���"4<>��4��h&�ڭ|�޲�3v��JڏDX�G��L�z��j7칒Ps2�A?��r-��N=�Ɲ�sy��HfS�f�"i��:�7x@�o�Xb�la�vK�� A�n̂5�C�Xʹ�捻��r��x�?���m[��L��gr�i�����H_:�3��lfJ��N0~�iyJ!K�rr�+Q�Y���板ޱ)���Vz[�5e��?�yK�uf��п�G������S@��_S�=(;	D�J0����HG��0`e�EpL޾��c�>(��Q]��?lw�����_u�ɮ��u&�q�)�6�756jd� �0H�a���U����-��0 �\���7�ٛ�X��tb�)��q�rS󈉢O\�R��#?{��e����ɇ`�ѼN.�eI3AEI�m5�LR�̈�]Ӱ�4C�}�K��B�z"k�J���O[�F`0�F�����U�3�sg Y2Q�}jۜ����P'���ڤ�Ƶ�^�����]��ski��8��:�oU��%*Bf�hq�E|�@&���t��H�I��_O`�2r{)0b��e/�xW�řJUn�V��D��u�q'�y�e-C9��+$�%ܛժ�Ƀ:Fdb%K]y6��425)e�u�M$�l��5�\��IL�g,I�4� �I,��x�n%@;�B�����C�˟r?i>������S0�'q�q�0'�R��;�ka��0��m+��� c;�r$��ч5�=��jh�.^j'Z�'�&_*�W���h�ꃶ�1�"1��=�FS"�c��S� �{�/�B�A6�e�҄�Vx""c~��c,iJ�WGm�Az���6(��`��\i��Kf�>��f&�_�x*��g�U؁
��>l5��<��i,�T	��6P�S��#a7˜ٔ=�$T�2��I��h�lk>�a����/c�ͨ�o޵Yݾaz�B�w#r$��`s���u^=L��^Bx�pMe���M$]�*�ꗭ��h(�>�A�$����![rȦ��Ii��>d}��uĴ��>Xͪhdɚ�r%��ʇ�bn��wô4�xl��ߪk�~o(/c R�5�Zm'Rm*貞�O���I�/�7�,[�h��{d��!���[�D�TFLZ��3�1;J����� ��9N?7?�i���Jr,����ܴ��<�S��5����R��e�i�rP�nI #Ë��0��̩b15AJ)��L��&�6md��ǹ�j�4�]w��
�)���iF���n�s�xٰJ�$�b>ٖ�o�K�*?>�cJ8I�_jIy]9�%l��	�o�n�0�q,�F,kM���8{�kn«���6以�5�I�T��r!g�n�p�MIi�E8o�X�I�����>O�H�h���RGq;c��:Mח"<��y�����&���ú�V[�>y���n����!w�:�3���w�X��|rL�ҥ�o�9�0~Ź��.�g�c�$�\����2l*"^i[��#�ID���>���k�ZG�Z�8$wE7JS����i6����4��;-��OCS�(%���2'd�t��g��(����|\���S�|Zx��"R��kET��,f]�:��?Ui���3�;pf>eK�g�����}r)�W����#��wљR/�u��dh��S�/�3g��r��&�I�C
}��㕞�
�o�U/ �2s*�m���my��|Ϻƹ��"�3��Kz��WJ��l��V�2�ڶ����@�
?f~�W-e��Χɣ=���p�8�35F�2�g���Z��kG���ag�EO�:e_�gf @y������@���zZ�Rh�wM������.<��TakF�𚧙��b�@�bp̘d@l05J��<�)N��T�2�>HϨ�y*�)�6'�V�S���V�+kD�w��d�c3vD���H��0.��.<��B�6Ǒ���S'���DM��	�T驍��6�L$�����YL�,���/�F�v����{y���������f���� ��H���g�w�(Y�^�v.^n�Z�׶O����y�-av�8F� ]�o
�7��ɾ�c���a�y��_���������x�h��h�W 0_kR'~�Z>P�Td�5��R�u�lziO�t�{BԌ�[R�����;��\�\h����څÿ��	q��m21!��ݜ�5u9���ab�vHE��������	2�N�I$+���X$!h��sv��� wI��5cL뒓HK���:kև\�!�k\Su{�֤J�~Jf��*2������-����y3z|NnC�#JF���]{���y�B���e[������o6߬��g���F�0%�$z���L�8mx��Pǂ�����ťoJ+�̞�`Nܼ*W�H"���+�R�.Q�R)�`I���8+K"}�'��KM�=x0��n�٩O��2�-&r�}���筳+^���N��KH�(��#.�X��ZN��:���I_Ʀ�3d������Vӷ>����}�q&D_���ST�fxL밮(L�Q���Q���ni��a���{u���v�M#�6�)��5�}��j�%0���=�l(��7�R�����C�ѳ�1ౣaR$��5��6<�,����e˨#�׈��ӊ� ʞ�eK��N�;�����=?Ƕ��3�gnε��я��nmI�K.���N5����,&���QI�ӵ��p�p�[J����I��]�*BZ���h�H4J�uf@�����v^�z����m��J�l����Pnn��1��"}v�}�&A�i�� P%6�#I%��,�Λ��NH�n�>�`��u{L�3R:�m7(�|e�ۍ��2#L��z��P��s��"x$Ѿ�����NϺ��KRh] �tc� ��Ʈ��+���/���h�0�,�P����?�K>�XA_NhG�� �R�"���N	4G<1��h���d���n*m�>�85=�ۃ��Z!�0��9G��9&�]@Rd�ס%(n��V.�Cnd�����e�¤MXO���7�l-�d���n�O�c�{$N<�v
2<3wVC �����RKp�ȵ�5�0x8���$��Df��&]?¨��{�'$8&'�҉��髐ŉ�yO�����q�[�/8J��vZ� cS��T� CJ5�����u����2/X�Eu^���㐂F��	d0k�%Ԍ�'Efp��)�!m����ʗ,}Tr�������ڡ���B����㩝��)e�~穨w�~9Dv�5�%�$ʔG��y4��Ta	��iΜ���IF#�ږP_��gS�&��5�If� ͈3�%��5j*݉wyQ}T^����)�ry��l��Ǟ����/>k����%������A�鋉"�pS[�k.ʭ�17Y�ѿR��(�L�/�MHI.    G"��I��R"&o���_ꐵN���O����������wJ��?��:�!)��\&�~���*S�y�ћ�<nd�zoǜ��a4d��t�[���XX����r�>.���@L�,[>���XJ�*����W�.)���`F�V5�Ay8���A @R�� �M�
�^�ļؖ�Bǧx���7M�\z�� ~���	4�l�x�����O��6C��,���pWWj�����`��t/�˱�s82ۓE��}��6�H�8���<�������Q��}L�K�R��.I�ﰤZЀ�������h��KDy���JG)txː��C��-U��f�-6H8)���6�;'ɳ���K���Xx�(����ܗ��x3��M�z���9�_&�&������?�� A��gR`:�#]�DJ�j�yG���殻\�ĝd�;���y���3I9��&�$B�[���R6.E�V���5?��9e���xȷ謌�� -GBl�{F����5�0���j����x믿���|�Ϗ�=�*��;͒;�QߚI:P�א��>B�8t2x\�m6�ɹ�Y���J�A�fL-�@����g����ܛPw,��V|lU��~y�_��}%�����K?��p���i3a�VfLh��1�A?hK��m3�?��|B��nB�P��A��A����+�o�-m8�Z���|x�;cS�T6R#3ʣ � +~��=$�&��\�0?s�g2���ⴓr7��N~���j^�[�ΰb.o5y����z,��"�w[,���m�~.���n��<�-�R7���d��|�ߚ����Yu���$Y�����t9.}�ϻ&>C��aF2Iٜ�@g�PV'�-�W����Ӟ�"<����`!����k,0�$�����*�r�7�3�h|�i��:��V#�����!�*��\�-.*��x܌�̈ڮ�M�r���JO.��c;�lc��ey0�V�� �*�����W*��P�?0Z��I�<Vx�$��q�iVN�ۨo�;o����KY��?k���N9������B���]�Γ�z�f�{p�L)4�4A�%A}i@<xbG~A"�������!O9� ������}ީM��ބky[����`����t�S�,7VR�v�a;�"��H��M_���,�p�k�;��&vwD&e�ތt��P�;����a^B�� cu���0�b�(�e]�P����������(s#b%��sC�`8���,��O�ݗw��2�)���߷쟶B��վ����E��8g�Ef;���@l�������V I%B��P��-zw9�[~�9=��
֘���c�dl�CE|�GP���"Ƚ-���pz���k��mrh�����r6��xJ]I�|�	�NI��A�0�9���Q�M_�R�}�;�t����"�ɹ1��H!.o�UR�f�iz��s��J�:;�pԕ�B�,�*�aU�7c���F�4/x��� ��:
1#�ڥ�U^e�Y"<�oG3 ��X>��V+�Uw~z"�H�H�;�$�?������w��W3DN�
𬂧��o<��l�|�V��X��s�a�O'j�M>�Oz����_����uyC-YcYk��M�οj���e�)�Հ=��)���_c���k��"Fg���z7��L'�	Kj�ĶR�>�Q	�i!���)����DAz>��a���ˆR��������ГJ7Ұ����D����]&�����F��%64Ԡ��t�EIo���h��´Ι��=w�뉪6./�=�e�: �1W?Y�reqT��Z����$��M�	��30
����~�S�g��� D��$��&.���N4t0�F����dgo����c��R�ݬc
�OD�}E���O�j<;�x~ҵ��͛��G� E�z��؎��&	��3�%~&��i���k��q져�~��O?���Ռ��!���5:��Y�:os�ϐ�,�\BN^�kAf��_ߘ,�I�z�<��T�dQpy�4i�כ�ر�1��������t�t^� CY�l�R�{�١�M���,%d�i��wj~$����Z勮�����&�*�����9(��d��Ԥ�]��p"R�a��U����]f!	}�xk����vd����2�=���s`���P��2�J���>���&:��E˾�qg.��0�� V��O�i&F$V`�<�y�=�L��|W�ɜ�Į�[�Ǳ=j��$��o'"�(u.���R�%���������ee_-0�A����+S�%��eDw�B�P.�2'�h��sM����!W<a$u%�S_��3~D���GL�K�}��c��p"<	������Uq�S�8H�TӬ����v�D�5�sI�ϝcA�rT͞6�H:�
�j#�.�%�Y	5�*�^���$�h$�9WZ�N�u���;OL	��dJ��i9a���_AjЍ�C�<=����q*�V�0��_��_���+��`3�	7����+����	�o\w�@��ߔ&ٕzTj���-|�u*R��7M�J�!mi:�V¶��F881��;��F���S�B�T<񰼟����I��[Hk.�vL��L=�s�Xә�9�9�b�D��;�̓,���eر�i�f˂�)�;���-�\c���$��H\@�~M�h����/�B	ԿK����nJ:2usk�G\[�{�����|\�ʺ�D�վ���bp�o��k��}n��S����EMh<!�qO	�c����H#�kFb9���i_��ӕP�_�����-���ݨ���_n�!�u���ջ'��']J�7/5}��A��|�u�Q�q�)y\����<����[�u���O�pr�+Ԙ2���b��&�=C��O���vN�ؒ�&��]x'7�1�C�d��[��<���k�S۷��$��g��
ƞ���~0���g����w�磦oN'�&,�iN�2��h�%�di��;�!��WT��6�cEǱh(���tؘț�'�����W���%��0���Fr��+�6k]�����LADېdRo��"�*�c�E�;�V����������H2c7����+�:"fqyr4�'��f�e$L��8Zm}R �ڎ�Agz��o���T�s<@�bG�+ΫmH�N�%�­'��2�kAt�Lf;8�`��~���ִ��̴@r-O{��<<���m�P1�&!+	]JC���q-�Wn�3�B���F=>_�]
�T7H��j�O�L������#�	�[o�H�.4L��'Gz�3N���sÎ��2ʌ�G�b~��ɘ�A�������QDo�}b�J������e�>䥾�i�$��"�8���<m�e⻁�s,�A
%{�K�_�ւF�q1�lL������D��[�̘	}!'m)����Tʫ�2~	��2�
D ��DJ��M9]�l�����L�T����_�7�(�+lO����6��x�`4�#�3e���S�
�⮲TdƇ|�X�te4�h�S�1�R �O���Pk��X�!�M���k��|s�R�y�;{��]}Vчy�\崷�ǝ����<3�����e�u�N�I���>Q��lv]V�f	��X�߻��r���Z@LxH�+��+�;���A��=c��!'� 9�<%�p�,�n㰹uNS��{����2L�s�j�*��zܣ��q�Í�:{������+qע��nP|�jR�4,�xx
�+m�i���i �|����#�?���OJ]z3+N���$ ]<��j��GtƣY�H��|}5��`^�bnxo��Rҏ4-�M ��q&3�+�>���S%�&���a)��@a;��?������^�����ZR�vV�gq.ȶ����{V��{ϋ\z�Y4W��>L�!E�3�4�I�Ȉ�LnQ�ʯ
���N��l�!4�ߜs���\[�R�׃�������U�_�a��J,�(/$��q�bڻ�6��&y�yD���k�8$�����邞U�@"b�ծ��:Wb�	��<S^d����rX�W��7[���Y��c���+�[����T��i{�xpeke�]><L1Z�� �p�sn;�iI�8    ̐j&U:byhX'��:({9ris�����)�n�k㿚_�|�&G����Mz�ƭ�o�r_��}��5�(#�u4r	�<=|�u[>!�e�o��_~���ܠ*~�y�}��-��-�����]�t��V��Q9Vڷ�-Os7�r��Q:h_6�DZGb��;�9r�ĉYJL�;Vg�|�H[/("P)�(���fh�ݥoN�kt�aWw���Μ��5*s ,v����T�)�v���c�c{o�?k!)9!_���� *�yjQr'�=�)x�gk'�w�t�H1��h��4�����Sc��<d�t#��D�=��]N�������nuFej4�O�f�w�2\[���Eo��6��D���f����@������T�FH<Tp�qS����L�S�E?er�%�rJ#��f"�����¬��-e�a�����0B����L6J���� �<�:�=
,���A D}�́��y$��q�~����-ei1�ȴ��s�oR�|�|c�qhNB�7�A�������t��\,#f$ A�1m�lp�0��ZsJ-Ǡ&5@�[z��4�b>�V�y��H�G�bU��;N"���s�)p��>v	b������mc(����#G�K�[u�%Y����Za{LfK�s���p�rtk0���O���&�m�7S��,f��?h0��ބ��z�niIL�N��4��=]�$O��J����Æ{��/���. ���Hn#�
��L�?T�:�^�r��6x����b5a��gN!���e�y<����~FS��
ؒ>9ڢ*=�cr#���
�K�ld��R���ui.�@ �����OrèZko�EI����>��%���P0:�Lq��\O��Dμ2�B��W9��z���Q5g����Y@
;�)]avRl��\?N��t��~^�AIvY�����!�$FH/�%u������\�]_6W��3���~�U�9���8�\����q;6
��_F�z"d�4�����Z���R,�֝L��Լ�u9�	��������/����A��$aҥ#��j��#Kokm���_id�z��+3F�}���҉pͰ��9L��£/��K���]�~�� ��[�Ϥ��\��L`[�3o���rz���8Y��C�;���T9�h����rv`�i��)�j���fS#�])�?_��$�7)�J�
4�eW��TC�ւ]N�tvXe5bp3Y( �=���c��H�0� �M��R�6�p9�Hհ�(�>��l,R�	��K֨��[Myy�-ʮi��f�f�deH���*���$[7���b��W @9P~��q�8�Y������u�ت΋A��2�CdD�9y#�y=�Á@�It��Й�K*���Pi���l��(�<N�<�#����+���C���F?�tB\N�6�3"<���Ų�7Cf=�(�܎Z��.9dD]s��m����&%x��I`#�P.CJ��p��w>ua>-y�˪/���t�9�0��rιa%8Ϻ�/��@�y�^G��F�d����D���A�J �ּ�iE�O��n����1�g�v��]�8��壇�i����d��`��|�� g���:������q�X"l��Nk������	��4�S��|��[	f�"+���p��u$&-��eN$�O���֑0�*��0���Ư%�Y�8��[���ٸ�Ql��j�f[���T��>&�,���v�n�t�e~Q��`'$2��Y1�"MNu^>&�c/s3-HOQF�`�ƿ��SV�cB�L峠s��qlfI�f{'zMh1?�Pu=��aɷ|�DuJg��i�����vH[�G�H��ݪ<�It�,����L��v�f7�GJ����.?�Tym[^�J�_�GVN3��,sګ7ed�JY��WDu1U	B[���q�@N�gǷT}l��Z���B���t� 3y�9D�Y����Q+�_��.�X_�d���%9��bYd��̍Z�ކ�{'��z8cj�Ǔ4�:ye�u2�(�Z6���5�k�7��:�$�e��.1r���+��=ܾi��>��Jܗ!�o^5?be��堩�vuA�-�<)�X�YO�,Aht:��L]���~='���tWE��~��ۡÏT7\����(�H�+-p�­IH|�1�B¶�)CJƘ,9I���ǟ����HE��K�͸��
��X��8W!3,*	�''���2*��9=cޏ�@�������m�sk�����4����׸{)��G�:�i���A��Ԑ���]貃��7�m��s2V��Ph� ��H��U{�KN��&�$����LZ(��J��j�9KP#ǌ�A���ӄ�j.�W�E�\��\Hzd�L�4=O4���rl���7�V�ui��,�hJl���| �/�d�&u����i�1�Hɂ�-e��S�X
h�@i}��f1M��%<h�[~��2d���2�H��Υ|8"��G.�Aj�/S�jvg���x0?JξH���r~MK����h�tL6F�a1�p���ʃ������#I�Ք@:o��8(��~A]����Sm�R����M��j�J�{�^�[Ɓ�r��Z���ZJl +{���
P{c��X�	�k*O5�J����	��We�`���K�rA����_g"���^W��e���^�ҍg�`,y	��B���&%��,�e-�ְu)3{Z�\���M��V��pa����}`��E�k����$�����'n��%�h��@���CJ��3Tޢ�r���|ъȦ=fB���~S���U>!������]����"��9��ɔ��`G�͒��#̖"�3H��ON���9���y7���脡��B�ԯD���P=�������)9�݌lJwy�*��]��~�,pR��ؓ�/*~O)Z'�$u��a��#uL�g*���`U�~*�=߈m6�ƽ\�ޞ��%����jZ�T�&��i�@(�Y�k�����M���6Lc���%���j��
z_4sD�P`�G̏�4�Pg��&3��o^�=|�*J��)�>�p���bL�;�W������Cα��%Z�I�;mn-�=��%{>�CW� ��`8{�Za0�9Z	q�7�-�o�gsڟH.��/DPo.񸖛�P�Ξb�L�j)� ].�d�f��۲6�&[JI2���".	�T)��)fh�4lK�v2�%�L��M�7)��V�{/�e�Ƚ$,'��-�M�6]�I^�����+\�0��%'kb,��T�����%��M�+=I ��[.;��An��+Z���1M/dAN��g��*���j��\�	GϺ�5�����c4�m���{�Gn\ �MVN;u[q�g�4-.��3�Z�t�ϝ�7�6��ك-��$[�ʠ�1)�8��K�h��}��>��0~MAr�_s@�-�y�<%V�g	�Z�lS�4��R_E�[[ � ߤ����q��l�^Zַ��߰�<1�F3��ӟ`��i^F+�k��0�P�O��O&�N��dE�wpp9���/[����r&&˷��Ӆ&u�+���>�D(H0�~%=@�d��aȉE}Q�����T��֏��_Â�%m�)vPeM�U�sH9	Ĵ+Q�-AM֙y�����ܯM�����S�=-����������"�҉.����'����Oe���6Wi�9�a��@��s��݃>1�0�K�����������$7fXBx0��H K�!�fؗ�@�'�����	�ӭ���RB�BT��*��{0<�ג���J@I�{�mB��{�� �-|E�=�>G��"%V�i\K]wL`$�O�	H�c9c\~��������v�j��w�`��� ���s9�����#M�>?a�!��3��	�����,ck��r�g�=�Y����ZҢYe��,�9�ϫ�� �:1��^�B0�3�3�J��<�Ϣ|��5SZ��q]��z'���3U�l�_pS^#K�p%��ae�OSz��/OyrY���P�`����`��Ԓ����k�v���u�A
���<�i�2��䧴>L���,u�˫�q[*��ߖq����Rn��Lw �  ���A!^����ut�_?�����:��>f,��iy��(�v�$ᗲ�����~��k�����+j�2l����}~9��#�T��'�2�@h�ʺ"%q�wzq��_�P�@���ԿE}�\U���ևϏ����V��/%�%�u��m����qנ�xԕ�!{�W��ٕ��4_�X�9=���ؒ����! ��s�z�y����Ⱋv�Х����%�����I�l|�&��O��]��Ɖ����F���{�;�(�F�Dc'!��������R�Im���S���'UN�j�ԙg�a�С�����[5�c�ɵ�wz���)
�^ݞv{/�w�on;������(?~0�W���\���`�JL���O~�#��П4H�p�unoMs^_+��{Xu�	�E�B:����U�B���+�U��2P�]��5���?���과�S�M��.:���BY��Ӽ��F[�)
��M����ۇuI-H����i�����eԴ9�(p��|���ާ�ܥ<QuUU�ަN+�;z7(  x��F��xk�
I�x_�=�S[U���uq�)���Zr93b�mɜW�&��0(� ����Jp����M�S������5\LT�|�./?)uߵ�������`�,���n�{>��J�<�k��r�S��0��+����qXR)s��-]�߿Eg;�)�?���B�x.@���"ZM���(�Z�L�ו�)E�������Z�u����O?+m��[�B8�S�agj6�$=ŋ+���va-|Μ�$Dh�4�B\+ �@����HԆ���[�����Y
iʛ��!�C
��Sr_��?�YID�4��י��������Å����Ù9��_�=���;��H�mo,"v|�&>׹�X�vXO���IXd���T�����w�<�oU�(}��L���"��c$������SjG9Z%C�$Z�'\X��yk��/�'ou�����:�T�h/+A��m�۳�>Q�Ii�ITŤm�<5��W���.e�3$Z����z��'W���K"xX�S�q�%]�9C���.��Ԑ��N�����������7>�T�X����f;�k�/��Ny��-G����vw"������*yLx���HRc���������܄xWKĕ�'4��i9��g�BDl���iK�A���M�rL!���<�TW�E����t�/x�3�$�%_~�"�a���)I���Tl���j}0��Ӑ�?���3��?J���#߫(��%�yÕvn���S��QR���rf%A����7j�ũ)RN��ЍƄ?��s�$g�����2�	����Ra�#��xh��
�u^��p �Zr��F9�V/9��^	��|S.�@3�{yVq�Z2��>��7�OWo�*o'g��9�9�w�j�o��ӓ����\�n���h"�ـM��S��pxb�uCwR����8��K��o�X̟�
���|TNis"��Q�2�N�H��=���>V�xǋ�?��@�zY #�t)�*R�K�d�@��d�ж���sz
n��Vĭ��_����3��Ŝ�VK��� NW�}Sq�KScb���M���r�)l�Ꙇ�W\�;Q
���OW>o�/��Gk�M����OW�����[���QY:S�7}y �,���Ծ�,(�O9v��m�_���;RJ���^�:�}�4U��N��:u����5�\��v��
�Ht}�V<�6��Me2
��=��L8�&$�n�K�"�������4�Ғ�&\�Dۧ��X�N4�f�Й��Y� ����έU��f�y�H�U���6 �>I;
US��O������L�\⣛|V����L�]\��g}['�\��`_&n�W��W^`*GU����8/�TN&�XC�	�����q}���m��D�{�*��U�vu�S�����HΣ��qS����:m*��W����4J�G�	#y�Z������CSїHo�S�༳�ʑo���e3sV�Q�N,��;��<���l�Wț�'{���A�tY�*0����)�ngɻKc�P�H[�:�H^*�)��U�ɠ�z�;���Ug:˸>�_�5V*.����K���g_���9��W�'~�K��a�M^��&9j��������#�sV������Ł8?0�?U}��D\�)pn�Z���/���#-�z��_�?��Y'���O��,��D���	�� MT��,N�^��Щ�z������u뿎���Z��o�I�����7�2�]����pO�\�w����*0�KSj����RCE��Y�GU�y_��G�/�=��`����Lz+�BɫzQ#�- ��4�f�iyb�4�iow����GA�q�Eޡ��NC���3p�]��Z�Cl�p����j)�_o+ �̸Q���À���F21E\\7w�ܐ��}f��)ȓ�,]G�����?d3�A��w.�Q�~�	���F�j>�9D~~#��G�*����K~��MQ}���a���IriP@��l�S_���;��v�x]��F<	����C�>
|�P��؜�4a"WtXt>�o'�"���9�)�*��M���դ#�`�dq����M�b��W�>���`��k���l��||^��1_�y���F�a:����D��zi�-	o�#��]�*��[z҃����>)@�E ���$��&8�+&�YK�/2���R�
�l>s=�u��|��wy.�@��
�R����>�5��F�S�/Ŏ�5��4�����uiMoj�ҫԬi�T��l>�(��ϯO���v����Q��$f,����FwS�ꆏA�_�(y�s��a��y�����h�����Fh��չ�+8�E�;�"J����㐟�B��k}by� �ɘ�<���y�<��l���f*9�f����j���]n�~���g�o�0��M�8�ð�ô�q�W���������/���������������O�������������������g��O}����~���?������������?�������?�X�      t      x������ � �      u      x������ � �      y      x������ � �      z      x������ � �      x      x������ � �      v      x������ � �      w      x������ � �     