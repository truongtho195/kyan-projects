-- User Created: Jerry Truong
-- Date Created: 10/12/2013
truncate table "base_Attachment" RESTART IDENTITY cascade;
truncate table "base_Authorize" RESTART IDENTITY cascade;
truncate table "base_CardManagement" RESTART IDENTITY cascade;
truncate table "base_CashFlow" RESTART IDENTITY cascade;
truncate table "base_CostAdjustment" RESTART IDENTITY cascade;
truncate table "base_CountStock" RESTART IDENTITY cascade;
truncate table "base_CustomerReminder" RESTART IDENTITY cascade;
truncate table "base_Department" RESTART IDENTITY cascade;
truncate table "base_GenericCode" RESTART IDENTITY cascade;
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Set Print Property', 'M01','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Print Report', 'M02','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Preview Report', 'M03','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Set Copy', 'M04','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('New Set Copy', 'M05','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Detelte Set Copy', 'M06','EN', 'MR');    
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Add New Report', 'M11','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Edit Report', 'M12','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Delete Report', 'M13','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Change Group Report', 'M14','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('No Show Report', 'M15','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Assign Authorize Report', 'M16','EN', 'MR');
INSERT INTO "base_GenericCode"("Name", "Code", "Language", "Type") VALUES ('Set Permission', 'M17','EN', 'MR');	
truncate table "base_Guest" RESTART IDENTITY cascade;
truncate table "base_GuestGroup" RESTART IDENTITY cascade;
truncate table "base_GuestRewardSaleOrder" RESTART IDENTITY cascade;
truncate table "base_LayawayManager" RESTART IDENTITY cascade;
truncate table "base_MemberShip" RESTART IDENTITY cascade;
truncate table "base_PricingChange" RESTART IDENTITY cascade;
truncate table "base_PricingManager" RESTART IDENTITY cascade;
truncate table "base_Product" RESTART IDENTITY cascade;
truncate table "base_Promotion" RESTART IDENTITY cascade;
truncate table "base_PurchaseOrder" RESTART IDENTITY cascade;
truncate table "base_QuantityAdjustment" RESTART IDENTITY cascade;
truncate table "base_Reminder" RESTART IDENTITY cascade;
truncate table "base_ResourceAccount" RESTART IDENTITY cascade;
truncate table "base_ResourceNote" RESTART IDENTITY cascade;
truncate table "base_ResourcePayment" RESTART IDENTITY cascade;
truncate table "base_ResourcePhoto" RESTART IDENTITY cascade;
truncate table "base_ResourceReturn" RESTART IDENTITY cascade;
truncate table "base_RewardManager" RESTART IDENTITY cascade;
truncate table "base_SaleCommission" RESTART IDENTITY cascade;
truncate table "base_SaleOrder" RESTART IDENTITY cascade;

truncate table "base_SaleTaxLocation" RESTART IDENTITY cascade;
truncate table "base_Store" RESTART IDENTITY cascade;
truncate table "base_TransferStock" RESTART IDENTITY cascade;
truncate table "base_UOM" RESTART IDENTITY cascade;
truncate table "base_UserLog" RESTART IDENTITY cascade;
truncate table "base_VendorProduct" RESTART IDENTITY cascade;

truncate table "base_VirtualFolder" RESTART IDENTITY cascade;
truncate table "rpt_Department" RESTART IDENTITY cascade;
truncate table "rpt_Permission" RESTART IDENTITY cascade;
truncate table "rpt_User" RESTART IDENTITY cascade;
truncate table "tims_Holiday" RESTART IDENTITY cascade;
truncate table "tims_TimeLog" RESTART IDENTITY cascade;

truncate table "tims_TimeLogPermission" RESTART IDENTITY cascade;
truncate table "tims_WorkPermission" RESTART IDENTITY cascade;
truncate table "tims_WorkSchedule" RESTART IDENTITY cascade;
truncate table "tims_WorkWeek" RESTART IDENTITY cascade;


