------------------------18/12/2013: Get all View Script in POS--------------------------

----------------------INVENTORY ORDER ----------------------

-- View: v_rpt_inv_cost_adjustment

DROP VIEW v_rpt_inv_cost_adjustment;

CREATE OR REPLACE VIEW v_rpt_inv_cost_adjustment AS 
 SELECT d."Name", c."Reason", c."Status", p."ProductName", p."Attribute", p."Size", c."LoggedTime"::date AS "DateChanged", c."OldCost", c."NewCost", c."AdjustCostDifference", c."StoreCode", p."Resource"::character varying AS "ProductResource", p."ProductCategoryId" AS "CategoryId"
   FROM "base_CostAdjustment" c
   JOIN "base_Product" p ON p."Resource" = c."ProductResource"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId";

ALTER TABLE v_rpt_inv_cost_adjustment OWNER TO postgres;
COMMENT ON VIEW v_rpt_inv_cost_adjustment IS '(POSReport) Get all Cost Adjustment';


-- View: v_rpt_inv_product_list

DROP VIEW v_rpt_inv_product_list;

CREATE OR REPLACE VIEW v_rpt_inv_product_list AS 
 SELECT distinct d."Name" AS "Category", p."ProductName", p."Attribute", p."Size", p."QuantityOnHand", p."MarginPercent", p."AverageUnitCost", p."RegularPrice", p."AverageUnitCost" * p."QuantityOnHand" AS "Ext Cost", p."RegularPrice" * p."QuantityOnHand" AS "Ext Price", s."StoreCode", p."ProductCategoryId" AS "CategoryId", p."Resource"::character varying AS "ProductResource"
   FROM "base_Product" p
   JOIN "base_ProductStore" s ON p."Id" = s."ProductId"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId"
  WHERE p."IsPurge" = false
  ORDER BY d."Name", p."ProductName", s."StoreCode";

ALTER TABLE v_rpt_inv_product_list OWNER TO postgres;
COMMENT ON VIEW v_rpt_inv_product_list IS '(POSReport) Get all Product List';


-- View: v_rpt_inv_quantity_adjustment

DROP VIEW v_rpt_inv_quantity_adjustment;

CREATE OR REPLACE VIEW v_rpt_inv_quantity_adjustment AS 
 SELECT d."Name", q."Reason", q."Status", p."ProductName", p."Attribute", p."Size", q."LoggedTime"::date AS "DateChanged", q."OldQty", q."NewQty", q."AdjustmentQtyDiff", q."StoreCode", p."Resource"::character varying AS "ProductResource", p."ProductCategoryId" AS "CategoryId"
   FROM "base_QuantityAdjustment" q
   JOIN "base_Product" p ON p."Resource" = q."ProductResource"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId";

ALTER TABLE v_rpt_inv_quantity_adjustment OWNER TO postgres;
COMMENT ON VIEW v_rpt_inv_quantity_adjustment IS '(POSReport) Get all Quantity Adjustment
';


-- View: v_rpt_inv_transfer_stock

DROP VIEW v_rpt_inv_transfer_stock;

CREATE OR REPLACE VIEW v_rpt_inv_transfer_stock AS 
 SELECT t."TransferNo", t."DateCreated"::date AS "DateCreated", t."Status", t."FromStore", t."ToStore", t."TotalQuantity", t."UserCreated", t."DateApplied"::date AS "DateApplied", t."UserApplied", t."DateReversed"::date AS "DateReversed", t."UserReversed"
   FROM "base_TransferStock" t;

ALTER TABLE v_rpt_inv_transfer_stock OWNER TO postgres;
COMMENT ON VIEW v_rpt_inv_transfer_stock IS '
List all Transfer Stock and filter by StoreCode

Report: TransferHistory';



-- View: v_rpt_inv_transfer_stock_details

DROP VIEW v_rpt_inv_transfer_stock_details;

CREATE OR REPLACE VIEW v_rpt_inv_transfer_stock_details AS 
 SELECT t."TransferNo", td."ItemCode", td."ItemName", t."FromStore" AS "StoreCode", td."ItemAtribute", td."ItemSize", td."Quantity", td."BaseUOM", p."AverageUnitCost", td."Amount", p."ProductCategoryId" AS "CategoryId", p."Resource"::character varying AS "ProductResource"
   FROM "base_TransferStock" t
   JOIN "base_TransferStockDetail" td ON t."Id" = td."TransferStockId"
   JOIN "base_Product" p ON p."Resource"::character varying::text = td."ProductResource"::text
  ORDER BY t."TransferNo", td."ItemCode", td."ItemName", t."FromStore";

ALTER TABLE v_rpt_inv_transfer_stock_details OWNER TO postgres;
COMMENT ON VIEW v_rpt_inv_transfer_stock_details IS 'List all Transfer Stock Details 

Report: TransferStockDetails'';
';

--------------------PURCHASE ORDER --------------------


-- View: v_rpt_pur_get_po_locked

DROP VIEW v_rpt_pur_get_po_locked;

CREATE OR REPLACE VIEW v_rpt_pur_get_po_locked AS 
 SELECT p."PurchaseOrderNo", p."PurchasedDate"::date AS "PurchasedDate", p."Status", g."Company", p."Total", p."StoreCode", p."UserCreated"
   FROM "base_PurchaseOrder" p
   JOIN "base_Guest" g ON g."Resource" = p."VendorResource"::uuid
  WHERE p."IsLocked";

ALTER TABLE v_rpt_pur_get_po_locked OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_get_po_locked IS '(POS & POSReport) Get all Purchase Order Locked ';


-- View: v_rpt_pur_po_details

DROP VIEW v_rpt_pur_po_details;

CREATE OR REPLACE VIEW v_rpt_pur_po_details AS 
 SELECT po."PurchaseOrderNo", pod."ItemName", po."StoreCode", po."PurchasedDate"::date AS "PurchasedDate", po."Status", pod."Quantity", pod."Amount", pod."ProductResource"
   FROM "base_PurchaseOrder" po
   JOIN "base_PurchaseOrderDetail" pod ON po."Id" = pod."PurchaseOrderId"
  ORDER BY pod."ItemName", po."PurchasedDate"::date;

ALTER TABLE v_rpt_pur_po_details OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_po_details IS '(POSReport) Use for function sp_pur_get_po_details';

-- View: v_rpt_pur_po_summary

DROP VIEW v_rpt_pur_po_summary;

CREATE OR REPLACE VIEW v_rpt_pur_po_summary AS 
 SELECT p."PurchaseOrderNo", g."Company", p."StoreCode", p."Status", p."PurchasedDate"::date AS "PurchasedDate", p."ShipDate"::date AS "ShipDate", p."PaymentDueDate"::date AS "PaymentDueDate", p."Total", p."Paid", p."Balance", g."Resource"::character varying AS "VendorResource"
   FROM "base_PurchaseOrder" p
   JOIN "base_Guest" g ON p."VendorResource"::uuid = g."Resource"
  ORDER BY g."Company", p."PurchasedDate"::date;

ALTER TABLE v_rpt_pur_po_summary OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_po_summary IS '(POSReport) Use for function sp_pur_get_po_summary';

-- View: v_rpt_pur_product_cost

DROP VIEW v_rpt_pur_product_cost;

CREATE OR REPLACE VIEW v_rpt_pur_product_cost AS 
 SELECT d."Name" AS "Category", p."ProductName", po."PurchaseOrderNo", po."PurchasedDate"::date AS "PurchasedDate", g."Company", po."StoreCode", pd."Quantity" - COALESCE(rt."ReturnQty", 0::numeric) AS "Qty", pd."Price", (pd."Quantity" - COALESCE(rt."ReturnQty", 0::numeric)) * pd."Price" AS "Total Cost", po."VendorResource", d."Id" AS "CategoryId", pd."ProductResource"
   FROM "base_PurchaseOrder" po
   JOIN "base_PurchaseOrderDetail" pd ON po."Id" = pd."PurchaseOrderId"
   JOIN "base_Product" p ON p."Resource" = pd."ProductResource"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId"
   JOIN "base_Guest" g ON g."Resource" = po."VendorResource"::uuid
   LEFT JOIN ( SELECT rd."OrderDetailResource", rd."ReturnQty"
   FROM "base_ResourceReturnDetail" rd
   JOIN "base_Product" p ON rd."ProductResource" = p."Resource"
   JOIN "base_ProductStore" ps ON p."Resource" = ps."ProductResource"
  GROUP BY rd."OrderDetailResource", rd."ReturnQty") rt ON pd."Resource" = rt."OrderDetailResource"::uuid
  ORDER BY g."Company", po."PurchasedDate"::date;

ALTER TABLE v_rpt_pur_product_cost OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_product_cost IS '(POSReport) Use for function sp_pur_get_product_cost';


-- View: v_rpt_pur_vendor_list

DROP VIEW v_rpt_pur_vendor_list;

CREATE OR REPLACE VIEW v_rpt_pur_vendor_list AS 
 SELECT g."Company", COALESCE(g."Phone1", g."Phone2") AS "Phone", g."Email", (COALESCE(ga."AddressLine1", ga."AddressLine2")::text || ', '::text) || ga."City"::text AS "Address", ga."StateProvinceId", ga."PostalCode", ga."CountryId", g."Resource"::character varying AS "VendorResource"
   FROM "base_Guest" g
   JOIN "base_GuestAddress" ga ON g."Id" = ga."GuestId"
  WHERE g."Mark" = 'V'::bpchar
  ORDER BY g."Company";

ALTER TABLE v_rpt_pur_vendor_list OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_vendor_list IS '(POSReport) Use for function sp_pur_get_vendor_list';


-- View: v_rpt_pur_vendor_product_list

DROP VIEW v_rpt_pur_vendor_product_list;

CREATE OR REPLACE VIEW v_rpt_pur_vendor_product_list AS 
 SELECT distinct d."Name" AS "Category", p."ProductName", ps."StoreCode", g."Company", p."AverageUnitCost", g."Resource"::character varying AS "VendorResource", d."Id" AS "CategoryId", p."Resource"::character varying AS "ProductResource"
   FROM "base_Product" p
   JOIN "base_Guest" g ON p."VendorId" = g."Id"
   JOIN "base_ProductStore" ps ON p."Resource" = ps."ProductResource"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId"
  WHERE g."Mark" = 'V'::bpchar
  ORDER BY p."ProductName", ps."StoreCode", g."Company";

ALTER TABLE v_rpt_pur_vendor_product_list OWNER TO postgres;
COMMENT ON VIEW v_rpt_pur_vendor_product_list IS '(POSReport) Use for function sp_pur_get_vendor_product_list';



----------------------SALE ORDER ----------------------

-- View: v_rpt_sale_by_product_details

DROP VIEW v_rpt_sale_by_product_details;

CREATE OR REPLACE VIEW v_rpt_sale_by_product_details AS 
 SELECT de."Name" AS "Category Name", p."Code", p."ProductName", s."StoreCode", p."Attribute", p."Size", s."OrderDate"::date AS "OrderDate", s."SONumber", sum(DISTINCT 
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN sh."PackedQty"
            ELSE sh."PackedQty" * u."BaseUnitNumber"
        END) - 
        CASE COALESCE(rt."ReturnQtyUOM", 0::bigint::numeric)
            WHEN 0 THEN 0::bigint::numeric
            ELSE rt."ReturnQtyUOM"
        END AS "Sold Quantity", pu."Name" AS "UOM", sum(DISTINCT d."SubTotal") - sum(DISTINCT 
        CASE COALESCE(rt."ReturnQtyUOM", 0::bigint::numeric)
            WHEN 0 THEN 0::numeric
            ELSE rt."ReturnAmount"
        END) AS "Sale Amount", p."Resource"::character varying AS "ProductResource", de."Id" AS "CategoryId"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_SaleOrderShip" sos ON sos."SaleOrderResource"::uuid = s."Resource" AND sos."IsShipped" = true
   JOIN "base_SaleOrderShipDetail" sh ON sh."SaleOrderDetailResource"::uuid = d."Resource"
   JOIN "base_Product" p ON sh."ProductResource" = p."Resource"
   JOIN "base_Department" de ON de."Id" = p."ProductCategoryId"
   JOIN "base_ProductStore" st ON p."Resource" = st."ProductResource"
   LEFT JOIN "base_ProductUOM" u ON u."UOMId" = d."UOMId"
   LEFT JOIN "base_UOM" pu ON pu."Id" = p."BaseUOMId"
   LEFT JOIN ( SELECT rd."OrderDetailResource", rd."ResourceReturnId", rd."ItemName", sum(rd."ReturnQtyUOM") AS "ReturnQtyUOM", sum(DISTINCT rd."Amount") AS "ReturnAmount", p."Resource"
   FROM "base_ResourceReturnDetail" rd
   JOIN "base_Product" p ON rd."ProductResource" = p."Resource"
   JOIN "base_ProductStore" ps ON p."Resource" = ps."ProductResource"
  GROUP BY rd."ResourceReturnId", rd."ItemName", rd."OrderDetailResource", p."Resource") rt ON d."Resource" = rt."OrderDetailResource"::uuid AND rt."Resource" = d."ProductResource"
  GROUP BY de."Name", p."Code", p."ProductName", s."StoreCode", p."Attribute", p."Size", s."OrderDate", s."SONumber", pu."Name", rt."ReturnQtyUOM", p."Resource", de."Id"
  ORDER BY de."Name", p."ProductName", s."StoreCode", s."OrderDate" DESC;

ALTER TABLE v_rpt_sale_by_product_details OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_by_product_details IS '
Report: [Sale]-rptSaleByProductDetails';



-- View: v_rpt_sale_by_product_summary

DROP VIEW v_rpt_sale_by_product_summary;

CREATE OR REPLACE VIEW v_rpt_sale_by_product_summary AS 
 SELECT DISTINCT d."Name", p."Code", p."ProductName", s."StoreCode", p."Attribute", p."Size", s."SoldQuantity", s."TotalSale", s."TotalCOGS", s."SaleProfit", s."PurchasedQuantity", s."PurchasedSubTotal", s."TotalProfit", d."Id" AS "CategoryId", p."Resource"::character varying AS "ProductResource"
   FROM "base_Product" p
   JOIN "base_ProductStore" s ON p."Resource" = s."ProductResource"
   JOIN "base_Department" d ON d."Id" = p."ProductCategoryId"
  WHERE p."IsPurge" = false
  ORDER BY d."Name", p."ProductName", s."StoreCode";

ALTER TABLE v_rpt_sale_by_product_summary OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_by_product_summary IS '(POSReport) Get Sale by Product Summary
';


-- View: v_rpt_sale_card_management

DROP VIEW v_rpt_sale_card_management;

CREATE OR REPLACE VIEW v_rpt_sale_card_management AS 
 SELECT c."CardNumber", c."CardTypeId", c."PurchaseDate"::date AS "PurchaseDate", c."InitialAmount", c."LastUsed"::date AS "LastUsed", c."RemainingAmount", COALESCE((((g."LastName"::text || ', '::text) || g."FirstName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), 'System'::text) AS "Guest Purchased", COALESCE((((gr."LastName"::text || ', '::text) || gr."FirstName"::text) || ' '::text) || COALESCE(gr."MiddleName"::text, ''::text), 'System'::text) AS "Guest Gifted", c."Status", c."IsSold", c."DateCreated"::date AS "DateCreated"
   FROM "base_CardManagement" c
   LEFT JOIN "base_Guest" g ON g."Resource"::character varying::text = c."GuestResourcePurchased"::text
   LEFT JOIN "base_Guest" gr ON gr."Resource"::character varying::text = c."GuestGiftedResource"::text
  WHERE c."IsPurged" = false;

ALTER TABLE v_rpt_sale_card_management OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_card_management IS '(POS & POSReport) Get all card list';


-- View: v_rpt_sale_commission

DROP VIEW v_rpt_sale_commission;

CREATE OR REPLACE VIEW v_rpt_sale_commission AS 
 SELECT s."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Sale Rep", s."StoreCode", sum(d."SubTotal") AS "Close Sale", s."OrderDate"::date AS "OrderDate", sc."ComissionPercent", 
        CASE sc."Sign"
            WHEN '+'::bpchar THEN sc."CommissionAmount"
            ELSE - sc."CommissionAmount"
        END AS "case", sc."Sign"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_SaleCommission" sc ON s."Resource"::character varying::text = sc."SOResource"::text
   JOIN "base_Guest" g ON g."Resource"::character varying::text = sc."GuestResource"::text
  WHERE s."OrderStatus" = 6
  GROUP BY s."SONumber", s."OrderStatus", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."StoreCode", sc."ComissionPercent", sc."CommissionAmount", sc."Sign", s."OrderDate"
  ORDER BY (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."OrderDate", s."StoreCode", s."OrderStatus";

ALTER TABLE v_rpt_sale_commission OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_commission IS '(POSReport) Get all sale commission
';


-- View: v_rpt_sale_commission_details

DROP VIEW v_rpt_sale_commission_details;

CREATE OR REPLACE VIEW v_rpt_sale_commission_details AS 
 SELECT DISTINCT s."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Sale Rep", d."ItemName", s."StoreCode", s."OrderDate"::date AS "OrderDate", 
        CASE sc."Remark"
            WHEN 'SO'::text THEN sum(d."SubTotal")
            ELSE - sum(d."SubTotal")
        END AS "Close Sale", 
        CASE sc."Remark"
            WHEN 'SO'::text THEN d."Quantity" * d."SubTotal" * sc."ComissionPercent" / 100::numeric
            ELSE - (d."Quantity" * d."SubTotal" * sc."ComissionPercent" / 100::numeric)
        END AS "CommissionAmount", sc."Remark", g."Resource"::character varying AS "SaleRepResource", d."ProductResource"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_SaleCommission" sc ON s."Resource"::character varying::text = sc."SOResource"::text
   JOIN "base_Guest" g ON g."Resource"::character varying::text = sc."GuestResource"::text
  WHERE s."OrderStatus" = 6
  GROUP BY (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."SONumber", s."OrderStatus", s."StoreCode", sc."CommissionAmount", sc."Remark", s."OrderDate", d."ItemName", d."Quantity" * d."SubTotal" * sc."ComissionPercent" / 100::numeric, d."ProductResource", g."Resource"
  ORDER BY s."SONumber", sc."Remark";

ALTER TABLE v_rpt_sale_commission_details OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_commission_details IS '(POSReport) Get all sale commission details';


-- View: v_rpt_sale_customer_order_history

DROP VIEW v_rpt_sale_customer_order_history;

CREATE OR REPLACE VIEW v_rpt_sale_customer_order_history AS 
 SELECT s."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", p."ProductName", de."Name" AS "Category", s."StoreCode", s."OrderStatus", s."OrderDate"::date AS "OrderDate", d."Quantity", d."Quantity" * d."SalePrice" AS "Amount", s."CustomerResource", d."ProductResource", de."Id" AS "CategoryId"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_Product" p ON d."ProductResource" = p."Resource"
   JOIN "base_Department" de ON de."Id" = p."ProductCategoryId"
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar;

ALTER TABLE v_rpt_sale_customer_order_history OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_customer_order_history IS '(POSReport) Get all sale customer order history';

-- View: v_rpt_sale_customer_payment_details

DROP VIEW v_rpt_sale_customer_payment_details;

CREATE OR REPLACE VIEW v_rpt_sale_customer_payment_details AS 
 SELECT (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."SONumber", s."OrderStatus", s."StoreCode", s."DateCreated"::date AS "InvoiceDate", rp."DateCreated"::date - s."DateCreated"::date AS "Date Left", rp."DateCreated"::date AS "Date Paid", rp."TotalAmount" AS "Sale Total", rp."TotalPaid" AS "Amount Paid", rp."Balance", s."CustomerResource"
   FROM "base_SaleOrder" s
   JOIN "base_ResourcePayment" rp ON s."Resource" = rp."DocumentResource"::uuid
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
  WHERE s."OrderStatus" <> 6
  ORDER BY rp."Id";

ALTER TABLE v_rpt_sale_customer_payment_details OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_customer_payment_details IS '(POSReport) Get all sale customer payment details

Except Paid in full
WHERE s."OrderStatus" != 6;';



-- View: v_rpt_sale_customer_payment_summary

DROP VIEW v_rpt_sale_customer_payment_summary;

CREATE OR REPLACE VIEW v_rpt_sale_customer_payment_summary AS 
 SELECT DISTINCT (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."StoreCode", s."Total", s."Paid", s."Balance", COALESCE(s."DateUpdated", s."OrderDate")::date AS "LastOrder", rp."DateCreated"::date AS "LastPayment", s."CustomerResource"
   FROM "base_SaleOrder" s
   JOIN "base_ResourcePayment" rp ON s."Resource" = rp."DocumentResource"::uuid
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
  WHERE s."OrderStatus" <> 6;

ALTER TABLE v_rpt_sale_customer_payment_summary OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_customer_payment_summary IS '(POSReport) Get all sale customer payment summary

Except Paid in full
WHERE s."OrderStatus" != 6;';


-- View: v_rpt_sale_get_so_locked

DROP VIEW v_rpt_sale_get_so_locked;

CREATE OR REPLACE VIEW v_rpt_sale_get_so_locked AS 
 SELECT s."SONumber", s."OrderDate"::date AS "OrderDate", s."OrderStatus", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."Total", s."StoreCode", s."UserCreated"
   FROM "base_SaleOrder" s
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid
  WHERE s."IsLocked";

ALTER TABLE v_rpt_sale_get_so_locked OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_get_so_locked IS '(POSReport) Get all sale order locked';



-- View: v_rpt_sale_order_operational

DROP VIEW v_rpt_sale_order_operational;

CREATE OR REPLACE VIEW v_rpt_sale_order_operational AS 
 SELECT DISTINCT s."SONumber", s."OrderStatus", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."StoreCode", s."OrderDate"::date AS "OrderDate", s."Total" AS "Sale Amount", s."CustomerResource"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_Product" p ON d."ProductResource" = p."Resource"
   JOIN "base_ProductStore" st ON p."Resource" = st."ProductResource"
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
  ORDER BY (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."OrderStatus", s."StoreCode", s."OrderDate"::date;

ALTER TABLE v_rpt_sale_order_operational OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_order_operational IS '(POSReport) Get all sale order operational';



-- View: v_rpt_sale_order_summary

DROP VIEW v_rpt_sale_order_summary;

CREATE OR REPLACE VIEW v_rpt_sale_order_summary AS 
 SELECT t."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", t."StoreCode", t."OrderDate"::date AS "OrderDate", t."OrderStatus", t."SubTotal", t."TaxAmount", t."DiscountAmount", t."Shipping", t."Total", t."Deposit", t."CustomerResource"
   FROM "base_SaleOrder" t
   JOIN "base_Guest" g ON g."Resource" = t."CustomerResource"::uuid
  ORDER BY g."LastName", g."FirstName"::text, t."OrderDate", t."OrderStatus";

ALTER TABLE v_rpt_sale_order_summary OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_order_summary IS 'List all Sale Order Summary 

Report: SaleOrderSummary';


-- View: v_rpt_sale_product_customer

DROP VIEW v_rpt_sale_product_customer;

CREATE OR REPLACE VIEW v_rpt_sale_product_customer AS 
 SELECT de."Name" AS "Category", p."ProductName", s."StoreCode", s."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."OrderDate"::date AS "OrderDate", s."OrderStatus", sum(DISTINCT 
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN sh."PackedQty"
            ELSE sh."PackedQty" * u."BaseUnitNumber"
        END) - 
        CASE COALESCE(rt."ReturnQtyUOM", 0::bigint::numeric)
            WHEN 0 THEN 0::bigint::numeric
            ELSE rt."ReturnQtyUOM"
        END AS "Sold Quantity", sum(DISTINCT d."SubTotal") - sum(DISTINCT 
        CASE COALESCE(rt."ReturnQtyUOM", 0::bigint::numeric)
            WHEN 0 THEN 0::numeric
            ELSE rt."ReturnAmount"
        END) AS "Close Sale", de."Id" AS "CategoryId", s."CustomerResource", d."ProductResource"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_SaleOrderShip" sos ON sos."SaleOrderResource"::uuid = s."Resource" AND sos."IsShipped" = true
   JOIN "base_SaleOrderShipDetail" sh ON sh."SaleOrderDetailResource"::uuid = d."Resource"
   JOIN "base_Product" p ON sh."ProductResource" = p."Resource"
   JOIN "base_Department" de ON de."Id" = p."ProductCategoryId"
   JOIN "base_ProductStore" st ON p."Resource" = st."ProductResource"
   LEFT JOIN "base_ProductUOM" u ON u."UOMId" = d."UOMId"
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
   LEFT JOIN ( SELECT rd."OrderDetailResource", rd."ResourceReturnId", rd."ItemName", sum(rd."ReturnQtyUOM") AS "ReturnQtyUOM", sum(DISTINCT rd."Amount") AS "ReturnAmount", p."Resource"
   FROM "base_ResourceReturnDetail" rd
   JOIN "base_Product" p ON rd."ProductResource" = p."Resource"
   JOIN "base_ProductStore" ps ON p."Resource" = ps."ProductResource"
  GROUP BY rd."ResourceReturnId", rd."ItemName", rd."OrderDetailResource", p."Resource") rt ON d."Resource" = rt."OrderDetailResource"::uuid AND rt."Resource" = d."ProductResource"
  GROUP BY de."Name", p."ProductName", s."StoreCode", s."OrderDate", s."SONumber", s."OrderStatus", rt."ReturnQtyUOM", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."CustomerResource", d."ProductResource", de."Id"
  ORDER BY de."Name", p."ProductName", s."StoreCode", s."OrderDate";

ALTER TABLE v_rpt_sale_product_customer OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_product_customer IS '
List all product customer';


-- View: v_rpt_sale_profit_summary

DROP VIEW v_rpt_sale_profit_summary;

CREATE OR REPLACE VIEW v_rpt_sale_profit_summary AS 
 SELECT s."SONumber", s."OrderStatus", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."StoreCode", s."OrderDate"::date AS "OrderDate", sum(d."SubTotal") AS "Sale Amount", sum(
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN d."PickQty"
            ELSE d."PickQty" * u."BaseUnitNumber"
        END * p."AverageUnitCost") AS "COGS", sum(d."SubTotal") - sum(
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN d."PickQty"
            ELSE d."PickQty" * u."BaseUnitNumber"
        END * p."AverageUnitCost") AS "Gross Profit", s."CustomerResource"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_Product" p ON d."ProductResource" = p."Resource"
   JOIN "base_ProductStore" st ON p."Resource" = st."ProductResource"
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
   LEFT JOIN "base_ProductUOM" u ON u."ProductStoreResource" = st."Resource" AND u."UOMId" = d."UOMId"
  GROUP BY s."SONumber", s."OrderStatus", s."CustomerResource", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."StoreCode", s."OrderDate"
  ORDER BY s."StoreCode", s."OrderDate";

ALTER TABLE v_rpt_sale_profit_summary OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_profit_summary IS '(POSReport) Get all sale profit summary';


-- View: v_rpt_sale_representative

DROP VIEW v_rpt_sale_representative;

CREATE OR REPLACE VIEW v_rpt_sale_representative AS 
 SELECT s."SONumber", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text) AS "Customer", s."StoreCode", s."OrderStatus", s."SaleRep", sum(d."SubTotal") AS "Close Sale", s."OrderDate"::date AS "OrderDate", s."CustomerResource"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
  GROUP BY s."SONumber", s."OrderStatus", s."CustomerResource", (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."StoreCode", s."SaleRep", s."OrderDate"
  ORDER BY (((g."FirstName"::text || ', '::text) || g."LastName"::text) || ' '::text) || COALESCE(g."MiddleName"::text, ''::text), s."OrderDate", s."StoreCode", s."OrderStatus";

ALTER TABLE v_rpt_sale_representative OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_representative IS '(POSReport) Get all sale representative';


-- View: v_rpt_sale_voided_invoice

DROP VIEW v_rpt_sale_voided_invoice;

CREATE OR REPLACE VIEW v_rpt_sale_voided_invoice AS 
 SELECT so."SONumber", so."Total", so."DateUpdated", so."VoidReason", so."UserUpdated", so."StoreCode"
   FROM "base_SaleOrder" so
  WHERE so."IsVoided" = true
  ORDER BY so."DateUpdated" DESC;

ALTER TABLE v_rpt_sale_voided_invoice OWNER TO postgres;
COMMENT ON VIEW v_rpt_sale_voided_invoice IS '(POSReport) Get all sale voided invoice';




--------------------------ORTHER-----------------------------


DROP VIEW v_configuration;

CREATE OR REPLACE VIEW v_configuration AS 
 SELECT c."CompanyName", (c."Address"::text || ' '::text) || c."City"::text, c."Email", c."Phone", c."Website", c."Logo", c."Fax", c."DefaultLanguage", c."CurrencySymbol", c."DecimalPlaces", c."FomartCurrency", c."ReceiptMessage"
   FROM "base_Configuration" c;

ALTER TABLE v_configuration OWNER TO postgres;


-- View: v_get_all_store_name

DROP VIEW v_get_all_store_name;

CREATE OR REPLACE VIEW v_get_all_store_name AS 
 SELECT s."Name"
   FROM "base_Store" s
  ORDER BY s."Id";

ALTER TABLE v_get_all_store_name OWNER TO postgres;


-- View: v_get_email_config

DROP VIEW v_get_email_config;

CREATE OR REPLACE VIEW v_get_email_config AS 
 SELECT c."EmailPop3Server", c."EmailPop3Port", c."EmailAccount", c."EmailPassword"
   FROM "base_Configuration" c;

ALTER TABLE v_get_email_config OWNER TO postgres;


-- View: v_rpt_get_all_product

DROP VIEW v_rpt_get_all_product;

CREATE OR REPLACE VIEW v_rpt_get_all_product AS 
 SELECT p."Id", p."ProductName", p."Resource", p."ProductCategoryId"
   FROM "base_Product" p;

ALTER TABLE v_rpt_get_all_product OWNER TO postgres;
COMMENT ON VIEW v_rpt_get_all_product IS '(POSReport) Get all product';



----------------------------------POS-------------------------------------

-- View: v_sale_order_summary

DROP VIEW v_sale_order_summary;

CREATE OR REPLACE VIEW v_sale_order_summary AS 
 SELECT t."SONumber", g."Resource" AS "CustomerResource", (g."LastName"::text || ','::text) || g."FirstName"::text AS "Customer", t."StoreCode", t."OrderDate", t."OrderStatus", t."SubTotal", t."TaxAmount", t."DiscountAmount", t."Shipping", t."Total", t."Deposit"
   FROM "base_SaleOrder" t
   JOIN "base_Guest" g ON g."Resource" = t."CustomerResource"::uuid
  WHERE t."OrderStatus" <> 8;

ALTER TABLE v_sale_order_summary OWNER TO postgres;



-- View: v_sale_profit_summary

DROP VIEW v_sale_profit_summary;

CREATE OR REPLACE VIEW v_sale_profit_summary AS 
 SELECT s."SONumber", s."OrderStatus", g."Resource" AS "CustomerResource", (((g."LastName"::text || ', '::text) || g."FirstName"::text) || ' '::text) || g."MiddleName"::text AS "Customer", s."StoreCode", s."OrderDate", sum(d."SubTotal") AS "Sale Amount", sum(
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN d."PickQty"
            ELSE d."PickQty" * u."BaseUnitNumber"
        END * p."AverageUnitCost") AS "COGS", sum(d."SubTotal") - sum(
        CASE COALESCE(u."BaseUnitNumber", 0::numeric)
            WHEN 0 THEN d."PickQty"
            ELSE d."PickQty" * u."BaseUnitNumber"
        END * p."AverageUnitCost") AS "Gross Profit"
   FROM "base_SaleOrder" s
   JOIN "base_SaleOrderDetail" d ON d."SaleOrderId" = s."Id"
   JOIN "base_Product" p ON d."ProductResource"::text = p."Resource"::text
   JOIN "base_ProductStore" st ON p."Resource"::text = st."ProductResource"::text AND s."StoreCode" = 0
   JOIN "base_Guest" g ON g."Resource" = s."CustomerResource"::uuid AND s."Mark" = 'SO'::bpchar
   LEFT JOIN "base_ProductUOM" u ON u."ProductStoreResource"::text = st."Resource"::text AND u."UOMId" = d."UOMId"
  GROUP BY s."SONumber", s."OrderStatus", g."Resource", (((g."LastName"::text || ', '::text) || g."FirstName"::text) || ' '::text) || g."MiddleName"::text, s."StoreCode", s."OrderDate"
  ORDER BY s."SONumber";

ALTER TABLE v_sale_profit_summary OWNER TO postgres;

