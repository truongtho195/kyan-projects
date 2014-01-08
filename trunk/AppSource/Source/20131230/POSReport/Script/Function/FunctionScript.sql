-------------------18-12-2013: Get all Function Script-------------------


------------------------------------------ORTHER FUNCTION------------------------------------------


-- Function: checkserialnumber(character varying, character varying)

DROP FUNCTION checkserialnumber(character varying, character varying);

CREATE OR REPLACE FUNCTION checkserialnumber("partNumber" character varying, "serialNumber" character varying)
  RETURNS boolean AS
$BODY$BEGIN
RETURN (SELECT COUNT(*)
FROM 
  stockadjustmentdetailserial sads
WHERE 
  sads.serialnumber = $2 AND 
  sads.partnumber = $1) > 0;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION checkserialnumber(character varying, character varying) OWNER TO postgres;


-- Function: clearalldata(character varying)

DROP FUNCTION clearalldata(character varying);

CREATE OR REPLACE FUNCTION clearalldata(username character varying)
  RETURNS void AS
$BODY$
DECLARE
    statements CURSOR FOR
        SELECT tablename FROM pg_tables
        WHERE tablename not like 'rpt_%' and tablename <> 'base_CustomField' and tablename <> 'base_UserRight' and tableowner = username AND schemaname = 'public';
BEGIN
    FOR stmt IN statements LOOP
          IF stmt.tablename = 'tims_TimeLogPermission' THEN
	     EXECUTE 'TRUNCATE TABLE ' || quote_ident(stmt.tablename) || ' CASCADE; ALTER SEQUENCE ' || quote_ident(stmt.tablename || '_TimeLogId_seq') || ' RESTART WITH 1;';
          ELSE
             EXECUTE 'TRUNCATE TABLE ' || quote_ident(stmt.tablename) || ' CASCADE; ALTER SEQUENCE ' || quote_ident(stmt.tablename || '_Id_seq') || ' RESTART WITH 1;';
          END IF; 
    END LOOP;
    
UPDATE "base_CustomField" set "Label" = "FieldName";

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
            "WorkHour", "DefaultShipUnit", "DefaultCashiedUserName", 
            "KeepLog", "IsAllowShift", "DefaultLanguage", "TimeOutMinute", 
            "IsAutoLogout", "IsBackupWhenExit", "BackupEvery", "BackupPath", 
            "IsAllowRGO", "IsAllowChangeOrder", "IsAllowNegativeStore", "AcceptedGiftCardMethod", 
            "IsRewardOnTax", "IsRewardOnMultiPayment", "IsIncludeReturnFee", 
            "ReturnFeePercent", "IsRewardLessThanDiscount", "CurrencySymbol", 
            "DecimalPlaces", "FomartCurrency", "PasswordFormat", "KeepBackUp", 
            "CostMethod", 
            "StoreCode", "IsLive", "POSId", "IsRewardOnDiscount", 
            "IsCalRewardAfterRedeem", "IsRewardOnRetailer", "ReceiptMessage",
            "NegativeNumber","TextNumberAlign","IsAUPPG","IsStateCode","IsManualGenerate","IsAllowFirstCap","DataSource",
            "IsSendEmailCustomer","IsAllowAntiExemptionTax","IsManualPriceCalculation","IsAllowPayMultiReward","IsSumCashReward",
            "IsAllwayCommision","ReminderDay","WeekHour","RefundVoucherThresHold")
    VALUES ('Smart POS Company', 'Default Address', 'Default City', 0, 0, 0, 
            '', '', '', '', '', 0, 
            '', '', true, 
            false, false, false, 
            true, 5, 0, 
            0, 0, '', 
            '', 12, now(), 
            '', 1, false, 0, 
            0, 3, null, 0, 
            0, 0, 0, true, 
            8, 0, true, 
            7, false, 'EN', 10, 
            true, true, 1, '', 
            false, false, true, 0, 
            false, false, false, 
            0, false, '$', 
            2, 'en-US', '((?=.*[^a-zA-Z])(?=.*[a-z])(?=.*[A-Z])(?!\s).{8,})',7,
            0,  
            0, false, 0, false, 
            false,false, 'Thank you',
            0,1,true,true,false,true,'train_pos2013',
            false,false,false,true,true,
            false,0,40,0);
           
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION clearalldata(character varying) OWNER TO postgres;


-- Function: newid()

DROP FUNCTION newid();

CREATE OR REPLACE FUNCTION newid()
  RETURNS uuid AS
$BODY$
 SELECT CAST(md5(current_database()|| user ||current_timestamp ||random()) as uuid)
$BODY$
  LANGUAGE sql VOLATILE
  COST 100;
ALTER FUNCTION newid() OWNER TO postgres;


-- Function: sp_check_report_code(character)

DROP FUNCTION sp_check_report_code(character);

CREATE OR REPLACE FUNCTION sp_check_report_code(code character)
  RETURNS boolean AS
$BODY$
DECLARE resuilt BOOLEAN;
BEGIN
	SELECT count(*) > 0 INTO resuilt FROM "rpt_Report" r WHERE lower(r."Code") = lower($1);
	RETURN resuilt;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sp_check_report_code(character) OWNER TO postgres;
COMMENT ON FUNCTION sp_check_report_code(character) IS '(POSReport) Check duplicate Report code';


-- Function: sp_check_user_login(character varying, character varying)

DROP FUNCTION sp_check_user_login(character varying, character varying);

CREATE OR REPLACE FUNCTION sp_check_user_login(IN usr character varying, IN pwd character varying)
  RETURNS TABLE("IsActive" boolean, "NotExpiry" boolean, "Resource" character varying) AS
$BODY$
BEGIN
return query(
		SELECT u."IsActive", CASE WHEN COALESCE(u."ExpiryDate"::date - now()::date, 0) >= 0 THEN true
	ELSE false END , u."Resource"::character varying
		FROM "rpt_User" u
		WHERE lower(u."LoginName") = lower(usr) and  pwd = u."Password");

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_check_user_login(character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_check_user_login(character varying, character varying) IS '(POSReport) Check user login';


-- Function: sp_get_payment_method(integer, date, character varying)

DROP FUNCTION sp_get_payment_method(integer, date, character varying);

CREATE OR REPLACE FUNCTION sp_get_payment_method(IN paymentmethod_id integer, IN payment_date date, IN shift character varying)
  RETURNS TABLE("CardName" character varying, "Count" bigint, "Paid" numeric, "CardType" smallint) AS
$BODY$
DECLARE sql text;
tem text;
BEGIN
	sql = 'SELECT rd."PaymentMethod", COUNT(rd."PaymentMethodId"), SUM(rd."Paid"), rd."CardType" 
		   FROM "base_ResourcePayment" r
		   JOIN "base_ResourcePaymentDetail" rd ON rd."ResourcePaymentId" = r."Id"
		   WHERE r."DateCreated"::Date  =''' || payment_date || ''' AND rd."PaymentMethodId" =' || paymentmethod_id ||'::integer';
	IF shift <> '' THEN 
		sql = sql || ' AND r."Shift" =''' || shift::character varying || '''';
	END IF;
	sql = sql || ' GROUP BY rd."PaymentMethod", rd."CardType"
		   HAVING rd."CardType" <> 0';
	RETURN QUERY EXECUTE sql;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_get_payment_method(integer, date, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_get_payment_method(integer, date, character varying) IS '(POS) Get payment method';


-- Function: sp_get_reminder()

 DROP FUNCTION sp_get_reminder();
 
 
CREATE OR REPLACE FUNCTION sp_get_reminder()
  RETURNS void AS
$BODY$
BEGIN
	DELETE FROM "base_CustomerReminder" cr
		WHERE cr."ReminderTypeId" = 0 AND EXTRACT(MONTH FROM now()) > EXTRACT(MONTH FROM cr."DOB")
			OR (EXTRACT(MONTH FROM now()) = EXTRACT(MONTH FROM cr."DOB") AND EXTRACT(DAY FROM now()) > EXTRACT(DAY FROM cr."DOB"));
	INSERT INTO "base_CustomerReminder"(
		    "GuestResource", "ReminderTypeId", "DOB", "Name", "Company", "Phone", "Email")
	    ( SELECT g."Resource", 0 AS "ReminderTypeId", gp."DOB"::date, 
		CASE g."Title" 
			WHEN 0 THEN ''
			WHEN 1 THEN 'Mr.'
			WHEN 2 THEN 'Ms. '
			WHEN 3 THEN 'Mrs. '
			WHEN 4 THEN 'Prof. '			
		END
	    ||(g."LastName"::text || ', '::text || g."FirstName"::text || ' '::text || COALESCE(g."MiddleName"::text, ''::text)) AS "Name", g."Company", COALESCE(g."Phone1", g."Phone2") AS "Phone", g."Email"
	FROM "base_GuestProfile" gp 
		INNER JOIN "base_Guest" g on g."Resource" = gp."GuestResource"::uuid and g."Mark"='C' and g."IsPurged" = false
	WHERE  (EXTRACT(MONTH FROM gp."DOB") || '') || (EXTRACT(DAY FROM gp."DOB") || '')  
	       in 
	       (SELECT (EXTRACT(MONTH FROM CURRENT_DATE + s.a) || '') || (EXTRACT(DAY FROM CURRENT_DATE + s.a) || '') 
	        FROM GENERATE_SERIES(0, (Select "ReminderDay" From "base_Configuration")) AS s(a))
	
		AND gp."GuestId" > 0
		AND g."Resource" not in (SELECT cr."GuestResource"::uuid FROM "base_CustomerReminder" cr WHERE cr."ReminderTypeId" = 0));
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sp_get_reminder() OWNER TO postgres;
COMMENT ON FUNCTION sp_get_reminder() IS '(POS) Get all reminder
';


-- Function: sp_get_user_permission(character varying, integer)

DROP FUNCTION sp_get_user_permission(character varying, integer);

CREATE OR REPLACE FUNCTION sp_get_user_permission(IN resource character varying, IN permission_type integer)
  RETURNS TABLE("Type" smallint, "Code" character varying, "IsView" boolean, "IsPrint" boolean, "Right" boolean) AS
$BODY$
DECLARE sql text;
BEGIN
	SET session_replication_role = DEFAULT;
	sql = 'SELECT up."Type", up."Code", up."IsView", up."IsPrint", up."Right"
	FROM "rpt_Permission" up 
	WHERE up."UserResource" =''' || resource || '''';
	IF (permission_type = -1) THEN
		sql = sql || ' ORDER BY up."Type"';
	ELSEIF (permission_type = -2) THEN
		sql = sql || ' AND up."Type" <> 0 ORDER BY up."Type"';
	ELSE 
		sql = sql || ' AND up."Type" = ' || permission_type || ' ORDER BY up."Type"';
	END IF;	
	RETURN query EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_get_user_permission(character varying, integer) OWNER TO postgres;
COMMENT ON FUNCTION sp_get_user_permission(character varying, integer) IS '(POSReport) Check user permission
permission_type: 
-2 Report Permission and Menu Permission
-1. All Permission
0. Group Permission
1. Report Permission
2. Menu Permission
';




------------------------------------------INV FUNCTION------------------------------------------


-- Function: sp_inv_get_category_list(integer, integer)

DROP FUNCTION sp_inv_get_category_list(integer, integer);

CREATE OR REPLACE FUNCTION sp_inv_get_category_list(IN department_id integer, IN category_id integer)
  RETURNS TABLE("Department" character varying, "Category" character varying, "TaxCode" character, "Margin" numeric, "MarkUp" numeric) AS
$BODY$ 
DECLARE sql text;
BEGIN
	sql = 'SELECT d."Name" AS "Department", c."Name", c."TaxCodeId", c."Margin", c."MarkUp"
				FROM "base_Department" d
					JOIN "base_Department" c ON c."ParentId" = d."Id" AND c."LevelId" = 1
				WHERE d."LevelId" <> 2';		
	IF department_id <> -1 THEN
		sql = sql || ' AND d."Id" = ' || department_id;
	END IF;
	IF category_id <> -1 THEN
		sql = sql || ' AND c."Id" = ' || category_id;
	END IF;
	sql = sql || ' ORDER BY d."Name", c."Name"';
	return QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_category_list(integer, integer) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_category_list(integer, integer) IS '(POSReport)
List all Category 
rptCategoryList';


-- Function: sp_inv_get_cost_adjustment(integer, integer, character varying, integer, integer, character varying, character varying)

DROP FUNCTION sp_inv_get_cost_adjustment(integer, integer, character varying, integer, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_cost_adjustment(IN store_code integer, IN category_id integer, IN product_resource character varying, IN status integer, IN reason integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("Cate" character varying, "Rea" smallint, "Stt" smallint, "ProductName" character varying, attribute character varying, size character varying, loggedtime date, oldcost numeric, newcost numeric, diff numeric, "StoreCode" integer, "ProductResource" character varying, "CategoryId" integer) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;	

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF reason <> -1 THEN
		tem = ' v."Reason" = ' || reason::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."Status" = ' || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."DateChanged"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."DateChanged"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."DateChanged"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."DateChanged"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_inv_cost_adjustment';
	ELSE
		sql = 'SELECT * FROM v_rpt_inv_cost_adjustment v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_cost_adjustment(integer, integer, character varying, integer, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_cost_adjustment(integer, integer, character varying, integer, integer, character varying, character varying) IS '(POSReport) - rptCostAdjustment';


-- Function: sp_inv_get_product_list(integer, integer, character varying)

DROP FUNCTION sp_inv_get_product_list(integer, integer, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_product_list(IN store_code integer, IN category_id integer, IN product_resource character varying)
  RETURNS TABLE("CategoryName" character varying, "ProductName" character varying, "Attribute" character varying, "Size" character varying, "QuantityOnHand" numeric, "MarginPercent" numeric, "AverageUnitCost" numeric, "RegularPrice" numeric, "ExtCost" numeric, "ExtPrice" numeric, "StoreName" integer, "CategoryId" integer, "ProductResource" character varying) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;	

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;	

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_inv_product_list';
	ELSE
		sql = 'SELECT * FROM v_rpt_inv_product_list v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_product_list(integer, integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_product_list(integer, integer, character varying) IS '(POSReport)
List all Product and filter by StoreCode

rptProductList (Item List)''';


-- Function: sp_inv_get_product_summary_with_activity(integer, integer)

DROP FUNCTION sp_inv_get_product_summary_with_activity(integer, integer);

CREATE OR REPLACE FUNCTION sp_inv_get_product_summary_with_activity(IN store_code integer, IN category_id integer)
  RETURNS TABLE("Category" character varying, "StoreCode" integer, "On-Hand" numeric, "ExtPrice" numeric, "SoleQty" numeric, "SoldPrice" numeric) AS
$BODY$ 
DECLARE sql text;
BEGIN
	sql = 'SELECT d."Name", s."StoreCode", sum(s."QuantityOnHand") AS "On Hand", sum(s."QuantityOnHand"::numeric * p."RegularPrice") AS "Ext Price", sum(s."SoldQuantity") AS "Sold Quantity", sum(s."TotalSale") AS "Sold Price"
				 FROM "base_ProductStore" s
					INNER JOIN "base_Product" p ON p."Resource" = s."ProductResource"::uuid	   
					INNER JOIN "base_Department" d ON d."Id" = p."ProductCategoryId" 	
				 WHERE d."LevelId" = 1';
	IF store_code <> -1 THEN
		sql = sql || ' AND s."StoreCode" = ' || store_code;
	END IF;
	IF category_id <> -1 THEN
		sql = sql || 'AND d."Id" =' || category_id;
	END IF;
	sql = sql || 	'GROUP BY d."Name", s."StoreCode"
			ORDER BY d."Name", s."StoreCode"';
	RETURN QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_product_summary_with_activity(integer, integer) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_product_summary_with_activity(integer, integer) IS '(POSReport)

rptProductSummaryActivity';



-- Function: sp_inv_get_quantity_adjustment(integer, integer, character varying, integer, integer, character varying, character varying)

DROP FUNCTION sp_inv_get_quantity_adjustment(integer, integer, character varying, integer, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_quantity_adjustment(IN store_code integer, IN category_id integer, IN product_resource character varying, IN status integer, IN reason integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("Cate" character varying, "Rea" smallint, "Stt" smallint, "ProductName" character varying, attribute character varying, size character varying, loggedtime date, oldcost numeric, newcost numeric, diff numeric, "StoreCode" integer, "ProductResource" character varying, "CategoryId" integer) AS
$BODY$ 

DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;	

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF reason <> -1 THEN
		tem = ' v."Reason" = ' || reason::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."Status" = ' || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;		

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."DateChanged"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."DateChanged"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."DateChanged"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."DateChanged"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_inv_quantity_adjustment';
	ELSE
		sql = 'SELECT * FROM v_rpt_inv_quantity_adjustment v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_quantity_adjustment(integer, integer, character varying, integer, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_quantity_adjustment(integer, integer, character varying, integer, integer, character varying, character varying) IS '(POSReport)

List all Quantity Ajustment and filter  by StoreCode

rptQuantityAdjustment''
';


-- Function: sp_inv_get_reorder_stock(integer, character varying)

DROP FUNCTION sp_inv_get_reorder_stock(integer, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_reorder_stock(IN store_code integer, IN product_resource character varying)
  RETURNS TABLE("Code" character varying, "ProductName" character varying, "Attribute" character varying, "Size" character varying, "QuantityOnHand" numeric, "QuantityAvailable" numeric, "QuantityOnOrder" numeric, "ReorderPoint" numeric, "ReOrderQty" numeric, "Vendor" character varying) AS
$BODY$ 
BEGIN
	IF store_code <> -1 THEN
		IF product_resource <> '' THEN
			RETURN QUERY (
				  SELECT DISTINCT p."Code", p."ProductName", p."Attribute", p."Size", s."QuantityOnHand", s."QuantityAvailable", s."QuantityOnOrder", s."ReorderPoint", 
					(s."ReorderPoint" - (s."QuantityAvailable" + s."QuantityOnOrder")) AS "ReOrderQty", g."Company"--, s."StoreCode"
				   FROM "base_Product" p 
				   RIGHT JOIN "base_ProductStore" s ON p."Resource" = s."ProductResource"::uuid AND s."StoreCode" = store_code
				   INNER JOIN "base_Guest" g on g."Id" = p."VendorId" AND (lower(g."Mark") = 'v')
				   WHERE s."ReorderPoint" > 0 AND (s."QuantityOnOrder" + s."QuantityAvailable") < s."ReorderPoint"
					AND CAST(p."Resource" AS character varying) = product_resource 
				   ORDER BY p."ProductName"
			  );
		ELSE
			RETURN QUERY (
				  SELECT DISTINCT p."Code", p."ProductName", p."Attribute", p."Size", s."QuantityOnHand", s."QuantityAvailable", s."QuantityOnOrder", s."ReorderPoint", 
					(s."ReorderPoint" - (s."QuantityAvailable" + s."QuantityOnOrder")) AS "ReOrderQty", g."Company"--, s."StoreCode"
				   FROM "base_Product" p 
				   RIGHT JOIN "base_ProductStore" s ON p."Resource" = s."ProductResource"::uuid AND s."StoreCode" = store_code
				   INNER JOIN "base_Guest" g on g."Id" = p."VendorId" AND (lower(g."Mark") = 'v')
				   WHERE s."ReorderPoint" > 0 AND (s."QuantityOnOrder" + s."QuantityAvailable") < s."ReorderPoint"
				   ORDER BY p."ProductName"
			  );
		END IF;	  
	ELSE  	
		IF product_resource <> '' THEN
			RETURN QUERY (
				   SELECT DISTINCT p."Code", p."ProductName", p."Attribute", p."Size", p."QuantityOnHand", p."QuantityAvailable", p."QuantityOnOrder", p."CompanyReOrderPoint", 
					(p."CompanyReOrderPoint" - (p."QuantityAvailable" + p."QuantityOnOrder")) AS "ReOrderQty", g."Company" --, s."StoreCode"
				   FROM "base_Product" p 
				   RIGHT JOIN "base_ProductStore" s ON p."Resource" = s."ProductResource"::uuid
				   INNER JOIN "base_Guest" g on g."Id" = p."VendorId" AND (lower(g."Mark") = 'v')
				   WHERE p."CompanyReOrderPoint" > 0 AND (p."QuantityAvailable" + p."QuantityOnOrder") < p."CompanyReOrderPoint" 
					AND CAST(p."Resource" AS character varying) = product_resource
				   ORDER BY p."ProductName"
			);
		ELSE	
			RETURN QUERY (
				   SELECT DISTINCT p."Code", p."ProductName", p."Attribute", p."Size", p."QuantityOnHand", p."QuantityAvailable", p."QuantityOnOrder", p."CompanyReOrderPoint", 
					(p."CompanyReOrderPoint" - (p."QuantityAvailable" + p."QuantityOnOrder")) AS "ReOrderQty", g."Company" --, s."StoreCode"
				   FROM "base_Product" p 
				   RIGHT JOIN "base_ProductStore" s ON p."Resource" = s."ProductResource"::uuid
				   INNER JOIN "base_Guest" g on g."Id" = p."VendorId" AND (lower(g."Mark") = 'v')
				   WHERE p."CompanyReOrderPoint" > 0 AND (p."QuantityAvailable" + p."QuantityOnOrder") < p."CompanyReOrderPoint"
				   ORDER BY p."ProductName"
			);
		END IF;	
	END IF;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_reorder_stock(integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_reorder_stock(integer, character varying) IS '(POSReport)

List all  ReOrder Stock and filter by StoreCode

Report: ReOrderStock
';



-- Function: sp_inv_get_transfer_stock(integer, integer, integer, character varying, character varying)

DROP FUNCTION sp_inv_get_transfer_stock(integer, integer, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_transfer_stock(IN from_store integer, IN to_store integer, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("TransferNo" character varying, "DateCreated" date, "Status" smallint, "FromStore" smallint, "ToStore" smallint, "TotalQuantity" numeric, "UserCreated" character varying, "DateApplied" date, "UserApplied" character varying, "DateReversed" date, "UserReversed" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF from_store <> -1 THEN
		condition = true;
		sql = ' v."FromStore" = ' || from_store::text;
	END IF;
	
	IF to_store <> -1 THEN		
		tem = ' v."ToStore" = ' || to_store::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF status <> -1 THEN
		tem = ' v."Status" = ' || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."DateCreated"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."DateCreated"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."DateCreated"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."DateCreated"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_inv_transfer_stock';
	ELSE
		sql = 'SELECT * FROM v_rpt_inv_transfer_stock v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_transfer_stock(integer, integer, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_transfer_stock(integer, integer, integer, character varying, character varying) IS '(POSReport)
(Report) TransferStockSummary';

-- Function: sp_inv_get_transfer_stock_details(integer, integer, character varying)

DROP FUNCTION sp_inv_get_transfer_stock_details(integer, integer, character varying);

CREATE OR REPLACE FUNCTION sp_inv_get_transfer_stock_details(IN store_code integer, IN category_id integer, IN product_resource character varying)
  RETURNS TABLE("TransferNo" character varying, "ItemCode" character varying, "ItemName" character varying, "FromStore" smallint, "Attribute" character varying, "ItemSize" character varying, "Quantity" numeric, "BaseUOM" character varying, "Price" numeric, "Amount" numeric, "CategoryId" integer, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_inv_transfer_stock_details';
	ELSE
		sql = 'SELECT * FROM v_rpt_inv_transfer_stock_details v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_inv_get_transfer_stock_details(integer, integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_inv_get_transfer_stock_details(integer, integer, character varying) IS '(POSReport)

List all Transfer Stock history details and filter by StoreCode

Report: TransferStockDetails';






------------------------------------------PO FUNCTION------------------------------------------



-- Function: sp_pur_get_po_details(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_pur_get_po_details(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_pur_get_po_details(IN store_code integer, IN product_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("PONumber" character varying, "Product Name" character varying, "StoreCode" integer, "Purchase Date" date, "Status" smallint, "Qty" numeric, "Amount" numeric, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."Status" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."PurchasedDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."PurchasedDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_pur_po_details';
	ELSE
		sql = 'SELECT * FROM v_rpt_pur_po_details v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pur_get_po_details(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pur_get_po_details(integer, character varying, integer, character varying, character varying) IS '(POSReport)
Get Purchase Order Details

rptPODetails';


-- Function: sp_pur_get_po_summary(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_pur_get_po_summary(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_pur_get_po_summary(IN store_code integer, IN vendor_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("PONumber" character varying, "Company" character varying, "StoreCode" integer, "Status" smallint, "Purchase Date" date, "Ship Date" date, "Payment Due Date" date, "Total" numeric, "Paid" numeric, "Balance" numeric, "VendorResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF vendor_resource <> '' THEN		
		tem = ' v."VendorResource" = ''' || vendor_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."Status" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."PurchasedDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."PurchasedDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_pur_po_summary';
	ELSE
		sql = 'SELECT * FROM v_rpt_pur_po_summary v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pur_get_po_summary(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pur_get_po_summary(integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get Purchase Order Summary

rptPOSummary';


-- Function: sp_pur_get_product_cost(integer, character varying, integer, character varying, character varying, character varying)

DROP FUNCTION sp_pur_get_product_cost(integer, character varying, integer, character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_pur_get_product_cost(IN store_code integer, IN vendor_resource character varying, IN category_id integer, IN product_resource character varying, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("Category" character varying, "Product Name" character varying, "PONumber" character varying, "Purchase Date" date, "Company" character varying, "StoreCode" integer, "Qty" numeric, "Price" numeric, "Total Cost" numeric, "VendorResource" character varying, "CategoryId" integer, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;

	IF vendor_resource <> '' THEN		
		tem = ' v."VendorResource" = ''' || vendor_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."PurchasedDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."PurchasedDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_pur_product_cost';
	ELSE
		sql = 'SELECT * FROM v_rpt_pur_product_cost v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pur_get_product_cost(integer, character varying, integer, character varying, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pur_get_product_cost(integer, character varying, integer, character varying, character varying, character varying) IS '(POSReport)

Get Product Cost

rptProductCost';


-- Function: sp_pur_get_vendor_list(integer, character varying)

DROP FUNCTION sp_pur_get_vendor_list(integer, character varying);

CREATE OR REPLACE FUNCTION sp_pur_get_vendor_list(IN country_value integer, IN vendor_resource character varying)
  RETURNS TABLE("Company" character varying, "Phone" character varying, "Email" character varying, "Address" text, "StateProvinceId" integer, "PostalCode" character varying, "CountryId" integer, "VendorResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;				
	IF country_value <> -1 THEN
		condition = true;
		sql = ' v."CountryId" = ' || country_value::text;
	END IF;

	IF vendor_resource <> '' THEN		
		tem = ' v."VendorResource" = ''' || vendor_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_pur_vendor_list';
	ELSE
		sql = 'SELECT * FROM v_rpt_pur_vendor_list v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pur_get_vendor_list(integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pur_get_vendor_list(integer, character varying) IS '(POSReport)

Get Vendor list

rptVendorList';


-- Function: sp_pur_get_vendor_product_list(integer, character varying, integer, character varying)

DROP FUNCTION sp_pur_get_vendor_product_list(integer, character varying, integer, character varying);

CREATE OR REPLACE FUNCTION sp_pur_get_vendor_product_list(IN store_code integer, IN vendor_resource character varying, IN category_id integer, IN product_resource character varying)
  RETURNS TABLE("Category" character varying, "Product Name" character varying, "StoreCode" integer, "Company" character varying, "AUCCost" numeric, "VendorResource" character varying, "CategoryId" integer, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;

	IF vendor_resource <> '' THEN		
		tem = ' v."VendorResource" = ''' || vendor_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;	

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_pur_vendor_product_list';
	ELSE
		sql = 'SELECT * FROM v_rpt_pur_vendor_product_list v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pur_get_vendor_product_list(integer, character varying, integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pur_get_vendor_product_list(integer, character varying, integer, character varying) IS '(POSReport)

Get Vendor Product List

rptVendorProductList';






------------------------------------------SALE FUNCTION------------------------------------------


-- Function: sp_sale_commission_details(integer, character varying, character varying, character varying, character varying)

DROP FUNCTION sp_sale_commission_details(integer, character varying, character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_commission_details(IN store_code integer, IN sale_rep character varying, IN product_resource character varying, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Sale Rep" character varying, "ItemName" character varying, "StoreCode" integer, "Order Date" date, "Close Sale" numeric, "Commission Amount" numeric, "Remark" character varying, "Total Cost" numeric, "SaleRepResource" character varying, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;

	IF sale_rep <> '' THEN		
		tem = ' v."SaleRepResource" = ''' || sale_rep || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;	

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."PurchasedDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."PurchasedDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."PurchasedDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_commission_details';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_commission_details v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_commission_details(integer, character varying, character varying, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_commission_details(integer, character varying, character varying, character varying, character varying) IS '(POSReport)

Get Sale Commission Details

rptSaLeCommissionDetails';


-- Function: sp_sale_customer_order_history(integer, character varying, integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_customer_order_history(integer, character varying, integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_customer_order_history(IN store_code integer, IN customer_resource character varying, IN category_id integer, IN product_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Customer" text, "ProductName" character varying, "Category" character varying, "StoreCode" integer, "OrderStatus" smallint, "OrderDate" date, "Quantity" numeric, "Amount" numeric, "CustomerResource" character varying, "ProductResource" character varying, "CategoryId" integer) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN	
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN
		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
	
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_customer_order_history'::text;
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_customer_order_history v  WHERE '::text || sql;
	END IF;

RETURN QUERY EXECUTE sql;

END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_customer_order_history(integer, character varying, integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_customer_order_history(integer, character varying, integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get Customer order history

rptCustomerOrderHistory';


-- Function: sp_sale_get_customer_payment_details(integer, character varying, integer, character varying, character varying, character varying, character varying)

DROP FUNCTION sp_sale_get_customer_payment_details(integer, character varying, integer, character varying, character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_customer_payment_details(IN store_code integer, IN customer_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying, IN ship_from character varying, IN ship_to character varying)
  RETURNS TABLE("Customer" text, "SONumber" character varying, "OrderStatus" smallint, "StoreCode" integer, "InvoiceDate" date, "DateLeft" integer, "DatePaid" date, "Sale Total" numeric, "Amount Paid" numeric, "Balance" numeric, "CustomerResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF status <> -1 THEN
		tem = ' v."OrderStatus" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;	

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."InvoiceDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."InvoiceDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."InvoiceDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."InvoiceDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF ship_from <> '' AND ship_to <> '' THEN
		IF ship_from::date < ship_to::date THEN
			tem = 'v."Date Paid"::Date BETWEEN ''' || ship_from || ''' AND ''' ||ship_to || '''';
		ELSE		
			tem = 'v."Date Paid"::Date BETWEEN ''' || ship_to || ''' AND ''' ||ship_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF ship_from <> '' THEN
			tem = ' v."Date Paid"::Date >= ''' || ship_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF ship_to <> '' THEN
			tem =  'v."Date Paid"::Date <= ''' || ship_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_customer_payment_details';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_customer_payment_details v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_customer_payment_details(integer, character varying, integer, character varying, character varying, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_customer_payment_details(integer, character varying, integer, character varying, character varying, character varying, character varying) IS '(POSReport)

Get customer payment details

rptCustomerPaymentDetails';


-- Function: sp_sale_get_customer_payment_summary(integer, character varying, character varying, character varying, character varying, character varying)

DROP FUNCTION sp_sale_get_customer_payment_summary(integer, character varying, character varying, character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_customer_payment_summary(IN store_code integer, IN customer_resource character varying, IN order_from character varying, IN order_to character varying, IN ship_from character varying, IN ship_to character varying)
  RETURNS TABLE("Customer" text, "StoreCode" integer, "TotalAmount" numeric, "TotalPaid" numeric, "Balance" numeric, "LastOrder" date, "LastPayment" date, "CustomerResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."LastOrder"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."LastOrder"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."LastOrder"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."LastOrder"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF ship_from <> '' AND ship_to <> '' THEN
		IF ship_from::date < ship_to::date THEN
			tem = 'v."LastPayment"::Date BETWEEN ''' || ship_from || ''' AND ''' ||ship_to || '''';
		ELSE		
			tem = 'v."LastPayment"::Date BETWEEN ''' || ship_to || ''' AND ''' ||ship_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF ship_from <> '' THEN
			tem = ' v."LastPayment"::Date >= ''' || ship_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF ship_to <> '' THEN
			tem =  'v."LastPayment"::Date <= ''' || ship_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;		
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_customer_payment_summary';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_customer_payment_summary v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_customer_payment_summary(integer, character varying, character varying, character varying, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_customer_payment_summary(integer, character varying, character varying, character varying, character varying, character varying) IS '(POSReport)

Get customer payment summary

rptCustomerPaymentSummary';


-- Function: sp_sale_get_product_customer(integer, character varying, integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_get_product_customer(integer, character varying, integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_product_customer(IN store_code integer, IN customer_resource character varying, IN category_id integer, IN product_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("Category" character varying, "ProductName" character varying, "StoreCode" integer, "SONumber" character varying, "Customer" text, "OrderDate" date, "Status" smallint, "SoldQty" numeric, "CloseAmount" numeric, "CategoryId" integer, "CustomerResource" character varying, "ProductResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = ''' || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = ' || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;


	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_product_customer';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_product_customer v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_product_customer(integer, character varying, integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_product_customer(integer, character varying, integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get product customer

rptProductCustomer
';


-- Function: sp_sale_get_sale_by_product_details(integer, integer, character varying, character varying, character varying)

DROP FUNCTION sp_sale_get_sale_by_product_details(integer, integer, character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_sale_by_product_details(IN store_code integer, IN category_id integer, IN product_resource character varying, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("Category" character varying, "Code" character varying, "ProductName" character varying, "StoreCode" integer, "Attribute" character varying, "Size" character varying, "OrderDate" date, "OrderNumber" character varying, "SoldQuantity" numeric, "UOM" character varying, "SaleAmount" numeric, "Product Resource" character varying, "Category Id" integer) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;	

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = '''::text || product_resource::text || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_by_product_details';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_by_product_details v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_sale_by_product_details(integer, integer, character varying, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_sale_by_product_details(integer, integer, character varying, character varying, character varying) IS '(POSReport)

Get sale by product details

rptSaleByProductDetails';


-- Function: sp_sale_get_sale_by_product_summary(integer, integer, character varying)

DROP FUNCTION sp_sale_get_sale_by_product_summary(integer, integer, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_sale_by_product_summary(IN store_code integer, IN category_id integer, IN product_resource character varying)
  RETURNS TABLE("Category" character varying, "Code" character varying, "ProductName" character varying, "StoreName" integer, "Attribute" character varying, "Size" character varying, "SoldQuantity" numeric, "TotolSale" numeric, "TotalCOGS" numeric, "SaleProfit" numeric, "PurchasedQuantity" numeric, "PurchasedSubTotal" numeric, "TotalProfit" numeric, "CategoryId" integer, "ProductResource" character varying) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;	

	IF category_id <> -1 THEN
		tem = ' v."CategoryId" = '::text || category_id::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
				
	IF product_resource <> '' THEN		
		tem = ' v."ProductResource" = '''::text || product_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_by_product_summary';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_by_product_summary v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_sale_by_product_summary(integer, integer, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_sale_by_product_summary(integer, integer, character varying) IS '(POSReport)

rptSaleByProductSummary';


-- Function: sp_sale_get_sale_order_operational(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_get_sale_order_operational(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_sale_order_operational(IN store_code integer, IN customer_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Status" smallint, "Customer" text, "StoreCode" integer, "Order Date" date, "SaleAmount" numeric, "CustomerResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_order_operational';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_order_operational v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_sale_order_operational(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_sale_order_operational(integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get Sale Order Operational

rptSaleOrderOperational';


-- Function: sp_sale_get_sale_order_summary(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_get_sale_order_summary(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_sale_order_summary(IN store_code integer, IN customer_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Customer" text, "StoreCode" integer, "OrderDate" date, "OrderStatus" smallint, "SubTotal" numeric, "TaxAmount" numeric, "DiscountAmount" numeric, "Shipping" numeric, "Total" numeric, "Deposit" numeric, "CustomerResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN
		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_order_summary';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_order_summary v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;


END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_sale_order_summary(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_sale_order_summary(integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get Sale Order Summary

rptSaleOrderSummary';


-- Function: sp_sale_get_sale_profit_summary(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_get_sale_profit_summary(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_sale_profit_summary(IN store_code integer, IN customer_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Status" smallint, "Customer" text, "StoreCode" integer, "DateCreate" date, "SaleAmount" numeric, "COGS" numeric, "Gross Profit" numeric, "CustomerResource" character varying) AS
$BODY$
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = '::text || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
		

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_profit_summary';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_profit_summary v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_sale_profit_summary(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_sale_profit_summary(integer, character varying, integer, character varying, character varying) IS '(POSReport) 

Get sale Profit Summary

rptSaleProfitSummary';


-- Function: sp_sale_get_voided_invoice(integer, character varying, character varying)

DROP FUNCTION sp_sale_get_voided_invoice(integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_get_voided_invoice(IN store_code integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "SalePrice" numeric, "DateCreated" timestamp without time zone, "VoidedReason" character varying, "UserCreated" character varying, "StoreCode" integer) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
BEGIN
	condition = false;	
				
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;		
	
	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."DateUpdated"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."DateUpdated"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."DateUpdated"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."DateUpdated"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;

	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_voided_invoice';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_voided_invoice v  WHERE ' || sql;
	END IF;

	RETURN QUERY EXECUTE sql;
	
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_get_voided_invoice(integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_get_voided_invoice(integer, character varying, character varying) IS '(POSReport)

Get Voided invoice

rptVoidedInvoice';


-- Function: sp_sale_representative(integer, character varying, integer, character varying, character varying)

DROP FUNCTION sp_sale_representative(integer, character varying, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION sp_sale_representative(IN store_code integer, IN customer_resource character varying, IN status integer, IN order_from character varying, IN order_to character varying)
  RETURNS TABLE("SONumber" character varying, "Customer" text, "StoreCode" integer, "OrderStatus" smallint, "SaleRep" character varying, "Close Sale" numeric, "OrderDate" date, "CustomerResource" character varying) AS
$BODY$ 
DECLARE sql text;
	tem text;
	condition boolean;
	orderto text;	
BEGIN	
	condition = false;	
	
	IF store_code <> -1 THEN
		condition = true;
		sql = ' v."StoreCode" = ' || store_code::text;
	END IF;
	
	IF customer_resource <> '' THEN
		
		tem = ' v."CustomerResource" = ''' || customer_resource || '''';
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;
			
	IF status <> -1 THEN
		tem = ' v."OrderStatus" = ' || status::text;
		IF condition = true THEN
			sql = sql || ' AND ' || tem;
		ELSE
			condition = true;
			sql = tem;
		END IF;
	END IF;

	IF order_from <> '' AND order_to <> '' THEN
		IF order_from::date < order_to::date THEN
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_from || ''' AND ''' ||order_to || '''';
		ELSE		
			tem = 'v."OrderDate"::Date BETWEEN ''' || order_to || ''' AND ''' ||order_from || '''';
		END IF;
		IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
	ELSE	
		IF order_from <> '' THEN
			tem = ' v."OrderDate"::Date >= ''' || order_from || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
		
		IF order_to <> '' THEN
			tem =  'v."OrderDate"::Date <= ''' || order_to || '''';
			IF condition = true THEN
				sql = sql || ' AND ' || tem;
			ELSE
				condition = true;
				sql = tem;
			END IF;
		END IF;
	END IF;
	
	IF condition = FALSE THEN
		sql = 'SELECT * FROM v_rpt_sale_representative';
	ELSE
		sql = 'SELECT * FROM v_rpt_sale_representative v  WHERE ' || sql;
	END IF;

RETURN QUERY EXECUTE sql;

END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_sale_representative(integer, character varying, integer, character varying, character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_sale_representative(integer, character varying, integer, character varying, character varying) IS '(POSReport)

Get sale Representative

rptSaleRepresentative';




------------------------------------------POS FUNCTION------------------------------------------

-- Function: sp_pos_get_customer_profile(character varying)

DROP FUNCTION sp_pos_get_customer_profile(character varying);

CREATE OR REPLACE FUNCTION sp_pos_get_customer_profile(IN resource character varying)
  RETURNS TABLE("GuestNo" character varying, "Title" smallint, "Customer" text, "Phone" character varying, "Email" character varying, "Website" character varying, "Remark" character varying, "CellPhone" character varying, "IM" character varying, "BillAddress" character varying, "BillCity" character varying, "BillSate" integer, "BillPostalCode" character varying, "BillCountry" integer, "ShipAddress" character varying, "ShipCity" character varying, "ShipSate" integer, "ShipPostalCode" character varying, "ShipCountry" integer, "ETitle" smallint, "EmegencyContact" text, "EPhone" character varying, "ECellPhone" character varying, "ERelationship" character varying, "Image" bytea) AS
$BODY$
DECLARE sql text;
BEGIN
	
	RETURN QUERY (
		SELECT g."GuestNo",
		g."Title", COALESCE(g."LastName"::text,'') || ' '::text || COALESCE(g."FirstName"::text,'') || ' '::text as "Customer", 
		COALESCE(g."Phone1", g."Phone2") as "Phone", g."Email",  g."Website", g."Remark", g."CellPhone", g."IM",
		ga."AddressLine1" as "BillAddress", ga."City" AS "BillCity", ga."StateProvinceId" as "BillStateProvinceId", ga."PostalCode" as "BillPostalCode", ga."CountryId" as "BillCountryId",
		gs."AddressLine1" as "ShipAddress", gs."City" AS "ShipCity", gs."StateProvinceId", gs."PostalCode", gs."CountryId", gp."ETitle", 
		COALESCE(gp."ELastName"::text,'') || ' '::text || COALESCE(gp."EFirstName"::text,'') || ' '::text || COALESCE(gp."EMiddleName"::text, ''::text) AS "Emegency Contact",
		 gp."EPhone", gp."ECellPhone", gp."ERelationship", g."Picture"
		FROM "base_Guest" g 
			JOIN "base_GuestProfile" gp ON g."Id" = gp."GuestId"
			LEFT JOIN "base_GuestAddress" ga ON g."Id" = ga."GuestId" AND ga."AddressTypeId" = 2
			LEFT JOIN "base_GuestAddress" gs ON g."Id" = gs."GuestId" AND gs."AddressTypeId" = 3	
		WHERE g."Mark"	= 'C' --and g."Id" = 71
		AND g."Resource" = resource::uuid
	);
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_customer_profile(character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_customer_profile(character varying) IS '(POS) Get customer profile
';

-- Function: sp_pos_get_employee_information(character varying)

DROP FUNCTION sp_pos_get_employee_information(character varying);

CREATE OR REPLACE FUNCTION sp_pos_get_employee_information(IN resource character varying)
  RETURNS TABLE("Picture" bytea, "Title" smallint, "Employee" text, "Street" character varying, "City" character varying, "Sate" integer, "PostalCode" character varying, "Country" integer, "Phone1" character varying, "CellPhone" character varying, "Email" character varying, "SSN" character varying, "DOB" date, "Marital" smallint, "STitle" smallint, "SEmployee" text, "SRelationship" character varying, "SDOB" date, "SSSN" character varying, "SID" character varying, "SPhone" character varying, "SCellPhone" character varying, "SEmail" character varying, "SIM" character varying, "ETitle" smallint, "EmegencyContact" text, "EPhone" character varying, "ECellPhone" character varying, "ERelationship" character varying) AS
$BODY$
DECLARE sql text;
BEGIN
	
	RETURN QUERY (
		SELECT g."Picture", g."Title", g."LastName"::text || ' '::text || g."FirstName"::text || ' '::text || COALESCE(g."MiddleName", ''::bpchar)::text AS "Employee",  
		ga."AddressLine1" AS "PStreet", ga."City" AS "PCity", ga."StateProvinceId" AS "pState", 
		ga."PostalCode" AS "PPostalCode", ga."CountryId" AS "PCountry", g."Phone1", g."CellPhone", g."Email", gp."SSN", gp."DOB"::date AS "DOB", gp."Marital", gp."STitle", 
		COALESCE(gp."SLastName"::text,'') || ' '::text || COALESCE(gp."SFirstName"::text,'') || ' '::text || COALESCE(gp."SMiddleName", ''::bpchar)::text AS "Other", gp."SRelationShip",
		 gp."SDOB"::date AS "SDOB", gp."SSSN", gp."SIdentification", gp."SPhone", gp."SCellPhone", gp."SEmail", gp."SIM", gp."ETitle", 
		 COALESCE(gp."ELastName"::text,'') || ' '::text || COALESCE(gp."EFirstName"::text,'') || ' '::text || COALESCE(gp."EMiddleName", ''::bpchar)::text AS "Emegency", gp."EPhone", gp."ECellPhone", gp."ERelationship"
		   FROM "base_Guest" g
		   LEFT JOIN "base_GuestProfile" gp ON g."Id" = gp."GuestId"
		   LEFT JOIN "base_GuestAddress" ga ON ga."GuestId" = g."Id"
		  WHERE g."Mark" = 'E'::bpchar 
		AND g."Resource" = resource::uuid
	);
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_employee_information(character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_employee_information(character varying) IS '(POS) Get Employee inforamtion
';

-- Function: sp_pos_get_layaway_payment_plan(uuid)

DROP FUNCTION sp_pos_get_layaway_payment_plan(uuid);

CREATE OR REPLACE FUNCTION sp_pos_get_layaway_payment_plan(IN resource uuid)
  RETURNS TABLE("StartDate" date, "EndDate" date, "Title" smallint, "Customer" text, "SONumber" character varying, "OrderDate" timestamp without time zone, "Total" numeric, "Deposit" numeric, "PaymentMethod" smallint) AS
$BODY$
BEGIN
	RETURN QUERY (
		SELECT lw."StartDate"::date, lw."EndDate"::date, g."Title", COALESCE(g."LastName"::text,'') || ' '::text || COALESCE(g."FirstName"::text,'') || ' '::text as "Customer", 
		so."SONumber", so."OrderDate", so."Total", so."Deposit", lw."PaymentPeriod"
		FROM "base_SaleOrder" so 
			JOIN "base_Guest" g ON g."Resource" = so."CustomerResource"::uuid
			JOIN "base_LayawayManager" lw on so."SaleReference"::uuid = lw."Resource"
		WHERE so."Resource" = resource
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_layaway_payment_plan(uuid) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_layaway_payment_plan(uuid) IS '(POS) Get Payment plan information';
-- Function: sp_pos_get_layaway_payment_plan(uuid)

DROP FUNCTION sp_pos_get_layaway_payment_plan(uuid);

CREATE OR REPLACE FUNCTION sp_pos_get_layaway_payment_plan(IN resource uuid)
  RETURNS TABLE("StartDate" date, "EndDate" date, "Title" smallint, "Customer" text, "SONumber" character varying, "OrderDate" timestamp without time zone, "Total" numeric, "Deposit" numeric, "PaymentMethod" smallint) AS
$BODY$
BEGIN
	RETURN QUERY (
		SELECT lw."StartDate"::date, lw."EndDate"::date, g."Title", COALESCE(g."LastName"::text,'') || ' '::text || COALESCE(g."FirstName"::text,'') || ' '::text as "Customer", 
		so."SONumber", so."OrderDate", so."Total", so."Deposit", lw."PaymentPeriod"
		FROM "base_SaleOrder" so 
			JOIN "base_Guest" g ON g."Resource" = so."CustomerResource"::uuid
			JOIN "base_LayawayManager" lw on so."SaleReference"::uuid = lw."Resource"
		WHERE so."Resource" = resource
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_layaway_payment_plan(uuid) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_layaway_payment_plan(uuid) IS '(POS) Get Payment plan information';
-- Function: sp_pos_get_layaway_payment_plan(uuid)

DROP FUNCTION sp_pos_get_layaway_payment_plan(uuid);

CREATE OR REPLACE FUNCTION sp_pos_get_layaway_payment_plan(IN resource uuid)
  RETURNS TABLE("StartDate" date, "EndDate" date, "Title" smallint, "Customer" text, "SONumber" character varying, "OrderDate" timestamp without time zone, "Total" numeric, "Deposit" numeric, "PaymentMethod" smallint) AS
$BODY$
BEGIN
	RETURN QUERY (
		SELECT lw."StartDate"::date, lw."EndDate"::date, g."Title", COALESCE(g."LastName"::text,'') || ' '::text || COALESCE(g."FirstName"::text,'') || ' '::text as "Customer", 
		so."SONumber", so."OrderDate", so."Total", so."Deposit", lw."PaymentPeriod"
		FROM "base_SaleOrder" so 
			JOIN "base_Guest" g ON g."Resource" = so."CustomerResource"::uuid
			JOIN "base_LayawayManager" lw on so."SaleReference"::uuid = lw."Resource"
		WHERE so."Resource" = resource
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_layaway_payment_plan(uuid) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_layaway_payment_plan(uuid) IS '(POS) Get Payment plan information';
-- Function: sp_pos_get_layaway_payment_plan(uuid)

DROP FUNCTION sp_pos_get_layaway_payment_plan(uuid);

CREATE OR REPLACE FUNCTION sp_pos_get_layaway_payment_plan(IN resource uuid)
  RETURNS TABLE("StartDate" date, "EndDate" date, "Title" smallint, "Customer" text, "SONumber" character varying, "OrderDate" timestamp without time zone, "Total" numeric, "Deposit" numeric, "PaymentMethod" smallint) AS
$BODY$
BEGIN
	RETURN QUERY (
		SELECT lw."StartDate"::date, lw."EndDate"::date, g."Title", COALESCE(g."LastName"::text,'') || ' '::text || COALESCE(g."FirstName"::text,'') || ' '::text as "Customer", 
		so."SONumber", so."OrderDate", so."Total", so."Deposit", lw."PaymentPeriod"
		FROM "base_SaleOrder" so 
			JOIN "base_Guest" g ON g."Resource" = so."CustomerResource"::uuid
			JOIN "base_LayawayManager" lw on so."SaleReference"::uuid = lw."Resource"
		WHERE so."Resource" = resource
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_layaway_payment_plan(uuid) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_layaway_payment_plan(uuid) IS '(POS) Get Payment plan information';



-- Function: sp_pos_get_purchase_order(integer, boolean)

DROP FUNCTION sp_pos_get_purchase_order(integer, boolean);

CREATE OR REPLACE FUNCTION sp_pos_get_purchase_order(IN po_id integer, IN is_return boolean)
  RETURNS TABLE("PONo" character varying, "PODate" date, "PaymentTermDescription" character varying, "POCardImg" bytea, "ReturnFee" numeric, "Refund" numeric, "Balance" numeric) AS
$BODY$
BEGIN
	IF is_return THEN
		RETURN QUERY(	
			Select po."PurchaseOrderNo", po."PurchasedDate"::date, po."PaymentTermDescription", po."POCardImg", rt."ReturnFee", rt."TotalRefund", rt."Balance"
				From "base_PurchaseOrder" po 
				JOIN "base_ResourceReturn" rt ON po."Resource" = rt."DocumentResource"::uuid
				Where po."Id" = po_id AND rt."Mark" = 'PO'			
		);
	ELSE
		RETURN QUERY(	
			Select po."PurchaseOrderNo", po."PurchasedDate"::date, po."PaymentTermDescription", po."POCardImg",  po."Paid", po."Balance", 0.0
			    From "base_PurchaseOrder" po 
			    Where po."Id" = po_id
		);
	END IF;	

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_purchase_order(integer, boolean) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_purchase_order(integer, boolean) IS '(POS) get Purchase Order 
rptPurchaseOrder';


-- Function: sp_pos_get_purchase_order_details(integer, boolean)

DROP FUNCTION sp_pos_get_purchase_order_details(integer, boolean);

CREATE OR REPLACE FUNCTION sp_pos_get_purchase_order_details(IN po_id integer, IN is_return boolean)
  RETURNS TABLE("ItemCode" character varying, "ItemName" character varying, "Attribute" character varying, "Size" character varying, "Qty" numeric, "BaseUOM" character varying, "Price" numeric, "Amount" numeric, "IsReturn" boolean) AS
$BODY$
BEGIN
	IF is_return THEN
		RETURN QUERY(	
			Select rd."ItemCode", rd."ItemName", rd."ItemAtribute", rd."ItemSize", rd."ReturnQty", pd."BaseUOM", rd."Price", rd."Amount", rd."IsReturned"
			From "base_PurchaseOrder" po INNER JOIN  "base_PurchaseOrderDetail" pd ON po."Id" = pd."PurchaseOrderId"						
				INNER JOIN "base_ResourceReturnDetail" rd ON  pd."Resource" = rd."OrderDetailResource"::uuid
				INNER JOIN "base_ResourceReturn" rt on rt."Id" = rd."ResourceReturnId" AND rt."Mark" = 'PO'
			Where pd."PurchaseOrderId" = po_id
		);
	ELSE
		RETURN QUERY(	
			Select pd."ItemCode", pd."ItemName", pd."ItemAtribute", pd."ItemSize", pd."Quantity", pd."BaseUOM", pd."Price", pd."Amount" , false
			    From "base_PurchaseOrderDetail" pd
			    Where pd."PurchaseOrderId" = po_id
		);
	END IF;
		

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_purchase_order_details(integer, boolean) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_purchase_order_details(integer, boolean) IS '(POS) Get purchase order details by PO resource';



-- Function: sp_pos_get_vendor_profile(character varying)

DROP FUNCTION sp_pos_get_vendor_profile(character varying);

CREATE OR REPLACE FUNCTION sp_pos_get_vendor_profile(IN resource character varying)
  RETURNS TABLE("Picture" bytea, "GuestNo" character varying, "Company" character varying, "Street" character varying, "City" character varying, "State" integer, "PostalCode" character varying, "Country" integer, "Website" character varying, "Phone1" character varying, "Phone2" character varying, "CellPhone" character varying, "Fax" character varying, "Email" character varying, "IM" character varying, "FedTaxId" character varying, "PaymentTermDescription" character varying, "CreaditLine" numeric, "Remark" character varying) AS
$BODY$
DECLARE sql text;
BEGIN
	
	RETURN QUERY (		
		SELECT g."Picture", g."GuestNo", g."Company",
			ga."AddressLine1", ga."City", ga."StateProvinceId", ga."PostalCode", ga."CountryId",
			g."Website", g."Phone1", g."Phone2", g."CellPhone", g."Fax", g."Email", g."IM", gad."FedTaxId", g."PaymentTermDescription", g."CreditLine", g."Remark"
		FROM "base_Guest" g 
			LEFT JOIN "base_GuestAddress" ga on ga."GuestId" = g."Id"
			LEFT JOIN "base_GuestAdditional" gad on gad."GuestId" = g."Id"
		WHERE g."Mark" = 'V'::bpchar --AND "GuestNo" like '%3734'
		AND g."Resource" = resource::uuid
	);
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_get_vendor_profile(character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_get_vendor_profile(character varying) IS '(POS) Get Vendor Profile';


-- Function: sp_pos_so_get_pick_pack(character varying)

DROP FUNCTION sp_pos_so_get_pick_pack(character varying);

CREATE OR REPLACE FUNCTION sp_pos_so_get_pick_pack(IN resource character varying)
  RETURNS TABLE("ItemCode" character varying, "ItemName" character varying, "Attribute" character varying, "Size" character varying, "PackedQty" numeric, "IsShipped" boolean, "ShipDate" date, "Id" bigint) AS
$BODY$
BEGIN
	RETURN QUERY (
		SELECT ssd."ItemCode", ssd."ItemName", ssd."ItemAtribute", ssd."ItemSize", ssd."PackedQty", ss."IsShipped", ss."ShipDate"::date, ss."Id"
		FROM "base_SaleOrder" so
			JOIN "base_SaleOrderShip" ss on ss."SaleOrderResource"::uuid = so."Resource"
			JOIN "base_SaleOrderShipDetail" ssd on ss."Resource" = ssd."SaleOrderShipResource"::uuid
		WHERE so."Resource"::character varying = resource
		ORDER BY ssd."Id"
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_so_get_pick_pack(character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_so_get_pick_pack(character varying) IS '(POS) get Pick and Pack by Sale Order resource';



-- Function: sp_pos_so_get_sale_order(character varying, boolean)

DROP FUNCTION sp_pos_so_get_sale_order(character varying, boolean);

CREATE OR REPLACE FUNCTION sp_pos_so_get_sale_order(IN so_resource character varying, IN is_returned boolean)
  RETURNS TABLE("StoreCode" integer, "SOCardImg" bytea, "SONumber" character varying, "OrderDate" date, "Cashier" character varying, "SubTotal" numeric, "TaxCode" character varying, "Discount" numeric, "Shipping" numeric, "Total" numeric, "Tip" numeric, "TaxAmount" numeric, "Remark" character varying, "IsRedeeem" boolean, "RewardAmount" numeric) AS
$BODY$
BEGIN
	IF (is_returned = TRUE) THEN
		RETURN QUERY (
			  SELECT so."StoreCode", so."SOCardImg", so."SONumber", so."OrderDate"::date, so."UserCreated"
				, so."Paid", so."TaxCode", rt."Redeemed", rt."TotalRefund", rt."Balance", 0.0, 0.0, so."Remark", false, 0.0
			 FROM "base_SaleOrder" so
				JOIN "base_ResourceReturn" rt on rt."DocumentResource"::uuid = so."Resource" AND rt."Mark" = 'SO'
			WHERE CAST(so."Resource" AS CHARACTER VARYING) = so_resource	 
		);
	ELSE	
		RETURN QUERY (
			  SELECT so."StoreCode", so."SOCardImg", so."SONumber", so."OrderDate"::date, so."UserCreated", so."SubTotal", so."TaxCode", 
				so."DiscountAmount" AS "Discount", so."Shipping", so."Total", sum(rpd."Tip") AS "Tip", 
				so."TaxAmount", so."Remark", so."IsRedeeem", so."RewardAmount"
			 FROM "base_SaleOrder" so
				 JOIN "base_ResourcePayment" rp ON rp."DocumentResource"::uuid = so."Resource"
				 JOIN "base_ResourcePaymentDetail" rpd ON rpd."ResourcePaymentId" = rp."Id"
			WHERE CAST(so."Resource" AS CHARACTER VARYING) = so_resource
			 GROUP BY so."StoreCode", so."SOCardImg", so."SONumber", so."OrderDate"::date, so."UserCreated", so."SubTotal", so."TaxCode", 
				so."DiscountAmount", so."Shipping", so."Total", so."TaxAmount", so."Remark", so."IsRedeeem", so."RewardAmount"	 
		);
	END IF;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_so_get_sale_order(character varying, boolean) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_so_get_sale_order(character varying, boolean) IS '(POS) Get Sale Order
rptSODetails';


-- Function: sp_pos_so_get_sale_order_details(integer, boolean)

DROP FUNCTION sp_pos_so_get_sale_order_details(integer, boolean);

CREATE OR REPLACE FUNCTION sp_pos_so_get_sale_order_details(IN sale_order_id integer, IN is_return boolean)
  RETURNS TABLE("ProductName" character varying, "RegularPrice" numeric, "DiscountAmount" numeric, "Qty" numeric, "UOM" character varying, "SubTotal" numeric, "IsReturned" boolean) AS
$BODY$
BEGIN
	IF is_return THEN
		RETURN QUERY (		
			SELECT sd."ItemName", rd."Price" , rd."Discount", rd."ReturnQty", sd."UOM",  rd."Amount" + rd."VAT", rd."IsReturned"
			FROM "base_SaleOrderDetail" sd 
				JOIN "base_ResourceReturnDetail" rd ON rd."OrderDetailResource"::uuid = sd."Resource"
				JOIN "base_ResourceReturn" rt on rt."Id" = rd."ResourceReturnId" AND rt."Mark" = 'SO'
			WHERE sd."SaleOrderId" = sale_order_id 
			Order by sd."Id"

		);
	ELSE	
		RETURN QUERY (		
			SELECT sd."ItemName", 
				CASE (sd."ItemCode" <> '111111111111111' AND sd."ItemCode" <> '222222222222222')
					WHEN TRUE THEN sd."RegularPrice"
					ELSE sd."SalePrice"
				END AS "RegularPrice"
				, sd."TotalDiscount", sd."Quantity", sd."UOM",  sd."SubTotal", false
			FROM "base_SaleOrderDetail" sd 
			WHERE sd."SaleOrderId" = sale_order_id
			Order by sd."Id"
			   
		);
	END IF;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_so_get_sale_order_details(integer, boolean) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_so_get_sale_order_details(integer, boolean) IS '(POS) Get sale order details
rptSODetails';



-- Function: sp_pos_so_resource_payment_by_resource(character varying)

DROP FUNCTION sp_pos_so_resource_payment_by_resource(character varying);

CREATE OR REPLACE FUNCTION sp_pos_so_resource_payment_by_resource(IN so_resource character varying)
  RETURNS TABLE("PaymentMethod" character varying, "CardType" smallint, "Paid" numeric, "Reference" character varying) AS
$BODY$
BEGIN
	RETURN QUERY (
			SELECT  pd."PaymentMethod", pd."CardType", pd."Paid", pd."Reference"
			FROM "base_ResourcePayment" rp INNER JOIN "base_ResourcePaymentDetail" pd on rp."Id" = pd."ResourcePaymentId"
			WHERE rp."DocumentResource"::character varying = so_resource
		EXCEPT 
			SELECT  pd."PaymentMethod", pd."CardType", pd."Paid", pd."Reference"
			FROM "base_ResourcePayment" rp INNER JOIN "base_ResourcePaymentDetail" pd on rp."Id" = pd."ResourcePaymentId"
			WHERE pd."PaymentMethodId" = 4 AND pd."CardType" = 0 
			AND rp."DocumentResource"::character varying = so_resource
			ORDER BY 1	 
	);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sp_pos_so_resource_payment_by_resource(character varying) OWNER TO postgres;
COMMENT ON FUNCTION sp_pos_so_resource_payment_by_resource(character varying) IS '(POS) - Get all resource payment
rptSODetails';

