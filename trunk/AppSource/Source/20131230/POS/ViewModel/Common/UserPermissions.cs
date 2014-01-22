using System;
using System.Collections.Generic;
using System.Linq;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.Toolkit.Base;

namespace CPC.POS.ViewModel
{
    public class UserPermissions : ModelBase
    {
        #region Defines

        private base_CashFlowRepository _cashFlowRepository = new base_CashFlowRepository();

        #endregion

        #region Properties

        #region Base

        /// <summary>
        /// Check store is main
        /// </summary>
        public bool IsMainStore
        {
            get { return Define.StoreCode == 0; }
        }

        /// <summary>
        /// Check user is full permission
        /// </summary>
        public bool IsFullPermission
        {
            get { return Define.USER_AUTHORIZATION == null || Define.USER_AUTHORIZATION.Count == 0; }
        }

        /// <summary>
        /// Check user is admin
        /// </summary>
        public bool IsAdminPermission
        {
            get
            {
                if (Define.USER == null)
                    return false;
                return Define.ADMIN_ACCOUNT.Equals(Define.USER.LoginName);
            }
        }

        /// <summary>
        /// Gets the AllowAccessPermission
        /// </summary>
        public bool AllowAccessPermission
        {
            get
            {
                return IsAdminPermission ? true : IsMainStore;
            }
        }

        #endregion

        #region Application Menu

        private bool _allowRestoreData = true;
        /// <summary>
        /// Gets or sets the AllowRestoreData.
        /// </summary>
        public bool AllowRestoreData
        {
            get { return _allowRestoreData; }
            set
            {
                if (_allowRestoreData != value)
                {
                    _allowRestoreData = value;
                    OnPropertyChanged(() => AllowRestoreData);
                }
            }
        }

        private bool _allowAccessCashIn;
        /// <summary>
        /// Gets or sets the AllowAccessCashIn.
        /// </summary>
        public bool AllowAccessCashIn
        {
            get { return _allowAccessCashIn; }
            set
            {
                if (_allowAccessCashIn != value)
                {
                    _allowAccessCashIn = value;
                    OnPropertyChanged(() => AllowAccessCashIn);
                }
            }
        }

        private bool _allowAccessCashOut;
        /// <summary>
        /// Gets or sets the AllowAccessCashOut.
        /// </summary>
        public bool AllowAccessCashOut
        {
            get { return _allowAccessCashOut; }
            set
            {
                if (_allowAccessCashOut != value)
                {
                    _allowAccessCashOut = value;
                    OnPropertyChanged(() => AllowAccessCashOut);
                }
            }
        }

        #endregion

        #region Sale Module

        private bool _allowAccessSaleModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleModule.
        /// </summary>
        public bool AllowAccessSaleModule
        {
            get { return _allowAccessSaleModule; }
            set
            {
                if (_allowAccessSaleModule != value)
                {
                    _allowAccessSaleModule = value;
                    OnPropertyChanged(() => AllowAccessSaleModule);
                }
            }
        }

        #region Customer

        private bool _allowAccessCustomer = true;
        /// <summary>
        /// Gets or sets the AllowAccessCustomer.
        /// </summary>
        public bool AllowAccessCustomer
        {
            get { return _allowAccessCustomer; }
            set
            {
                if (_allowAccessCustomer != value)
                {
                    _allowAccessCustomer = value;
                    OnPropertyChanged(() => AllowAccessCustomer);
                }
            }
        }

        private bool _allowAddCustomer = true;
        /// <summary>
        /// Gets or sets the AllowAddCustomer.
        /// </summary>
        public bool AllowAddCustomer
        {
            get { return _allowAddCustomer; }
            set
            {
                if (_allowAddCustomer != value)
                {
                    _allowAddCustomer = value;
                    OnPropertyChanged(() => AllowAddCustomer);
                }
            }
        }

        private bool _allowAccessPayment = true;
        /// <summary>
        /// Gets or sets the AllowAccessPayment.
        /// </summary>
        public bool AllowAccessPayment
        {
            get { return _allowAccessPayment; }
            set
            {
                if (_allowAccessPayment != value)
                {
                    _allowAccessPayment = value;
                    OnPropertyChanged(() => AllowAccessPayment);
                }
            }
        }

        private bool _allowAccessCustomerReward = true;
        /// <summary>
        /// Gets or sets the AllowAccessCustomerReward.
        /// </summary>
        public bool AllowAccessCustomerReward
        {
            get { return _allowAccessCustomerReward; }
            set
            {
                if (_allowAccessCustomerReward != value)
                {
                    _allowAccessCustomerReward = value;
                    OnPropertyChanged(() => AllowAccessCustomerReward);
                }
            }
        }

        private bool _allowAccessSOHistory = true;
        /// <summary>
        /// Gets or sets the AllowAccessSOHistory.
        /// </summary>
        public bool AllowAccessSOHistory
        {
            get { return _allowAccessSOHistory; }
            set
            {
                if (_allowAccessSOHistory != value)
                {
                    _allowAccessSOHistory = value;
                    OnPropertyChanged(() => AllowAccessSOHistory);
                }
            }
        }

        private bool _allowSaleFromCustomer = true;
        /// <summary>
        /// Gets or sets the AllowSaleFromCustomer.
        /// </summary>
        public bool AllowSaleFromCustomer
        {
            get { return _allowSaleFromCustomer; }
            set
            {
                if (_allowSaleFromCustomer != value)
                {
                    _allowSaleFromCustomer = value;
                    OnPropertyChanged(() => AllowSaleFromCustomer);
                }
            }
        }

        private bool _allowManualReward = true;
        /// <summary>
        /// Gets or sets the AllowManualReward.
        /// </summary>
        public bool AllowManualReward
        {
            get { return _allowManualReward; }
            set
            {
                if (_allowManualReward != value)
                {
                    _allowManualReward = value;
                    OnPropertyChanged(() => AllowManualReward);
                }
            }
        }

        #endregion

        private bool _allowAccessReward = true;
        /// <summary>
        /// Gets or sets the AllowAddReward.
        /// </summary>
        public bool AllowAccessReward
        {
            get { return _allowAccessReward; }
            set
            {
                if (_allowAccessReward != value)
                {
                    _allowAccessReward = value;
                    OnPropertyChanged(() => AllowAccessReward);
                }
            }
        }

        #region Quotation

        private bool _allowAccessSaleQuotation = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleQuotation.
        /// </summary>
        public bool AllowAccessSaleQuotation
        {
            get { return _allowAccessSaleQuotation; }
            set
            {
                if (_allowAccessSaleQuotation != value)
                {
                    _allowAccessSaleQuotation = value;
                    OnPropertyChanged(() => AllowAccessSaleQuotation);
                }
            }
        }

        private bool _allowAddSaleQuotation = true;
        /// <summary>
        /// Gets or sets the AllowAddSaleQuotation.
        /// </summary>
        public bool AllowAddSaleQuotation
        {
            get { return _allowAddSaleQuotation; }
            set
            {
                if (_allowAddSaleQuotation != value)
                {
                    _allowAddSaleQuotation = value;
                    OnPropertyChanged(() => AllowAddSaleQuotation);
                }
            }
        }

        private bool _allowConvertToSalesOrder = true;
        /// <summary>
        /// Gets or sets the AllowConvertToSalesOrder.
        /// </summary>
        public bool AllowConvertToSalesOrder
        {
            get { return _allowConvertToSalesOrder; }
            set
            {
                if (_allowConvertToSalesOrder != value)
                {
                    _allowConvertToSalesOrder = value;
                    OnPropertyChanged(() => AllowConvertToSalesOrder);
                }
            }
        }

        private bool _allowDeleteProductQuotation = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProductQuotation.
        /// </summary>
        public bool AllowDeleteProductQuotation
        {
            get { return _allowDeleteProductQuotation; }
            set
            {
                if (_allowDeleteProductQuotation != value)
                {
                    _allowDeleteProductQuotation = value;
                    OnPropertyChanged(() => AllowDeleteProductQuotation);
                }
            }
        }

        #endregion

        #region Layaway

        private bool _allowAccessLayaway = true;
        /// <summary>
        /// Gets or sets the AllowAccessLayaway.
        /// </summary>
        public bool AllowAccessLayaway
        {
            get { return _allowAccessLayaway; }
            set
            {
                if (_allowAccessLayaway != value)
                {
                    _allowAccessLayaway = value;
                    OnPropertyChanged(() => AllowAccessLayaway);
                }
            }
        }

        private bool _allowAddLayaway = true;
        /// <summary>
        /// Gets or sets the AllowAddLayaway.
        /// </summary>
        public bool AllowAddLayaway
        {
            get { return _allowAddLayaway; }
            set
            {
                if (_allowAddLayaway != value)
                {
                    _allowAddLayaway = value;
                    OnPropertyChanged(() => AllowAddLayaway);
                }
            }
        }

        private bool _allowDeleteProductLayaway = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProductLayaway.
        /// </summary>
        public bool AllowDeleteProductLayaway
        {
            get { return _allowDeleteProductLayaway; }
            set
            {
                if (_allowDeleteProductLayaway != value)
                {
                    _allowDeleteProductLayaway = value;
                    OnPropertyChanged(() => AllowDeleteProductLayaway);
                }
            }
        }

        #endregion

        #region WorkOrder

        private bool _allowAccessWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowAccessWorkOrder.
        /// </summary>
        public bool AllowAccessWorkOrder
        {
            get { return _allowAccessWorkOrder; }
            set
            {
                if (_allowAccessWorkOrder != value)
                {
                    _allowAccessWorkOrder = value;
                    OnPropertyChanged(() => AllowAccessWorkOrder);
                }
            }
        }

        private bool _allowAddWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddWorkOrder.
        /// </summary>
        public bool AllowAddWorkOrder
        {
            get { return _allowAddWorkOrder; }
            set
            {
                if (_allowAddWorkOrder != value)
                {
                    _allowAddWorkOrder = value;
                    OnPropertyChanged(() => AllowAddWorkOrder);
                }
            }
        }

        private bool _allowSaleWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowSaleWorkOrder.
        /// </summary>
        public bool AllowSaleWorkOrder
        {
            get { return _allowSaleWorkOrder; }
            set
            {
                if (_allowSaleWorkOrder != value)
                {
                    _allowSaleWorkOrder = value;
                    OnPropertyChanged(() => AllowSaleWorkOrder);
                }
            }
        }

        private bool _allowDeleteProductWorkOrder = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProductWorkOrder.
        /// </summary>
        public bool AllowDeleteProductWorkOrder
        {
            get { return _allowDeleteProductWorkOrder; }
            set
            {
                if (_allowDeleteProductWorkOrder != value)
                {
                    _allowDeleteProductWorkOrder = value;
                    OnPropertyChanged(() => AllowDeleteProductWorkOrder);
                }
            }
        }

        #endregion

        #region SalesOrder

        private bool _allowAccessSaleOrder = true;
        /// <summary>
        /// Gets or sets the AllowAccessSaleOrder.
        /// </summary>
        public bool AllowAccessSaleOrder
        {
            get { return _allowAccessSaleOrder; }
            set
            {
                if (_allowAccessSaleOrder != value)
                {
                    _allowAccessSaleOrder = value;
                    OnPropertyChanged(() => AllowAccessSaleOrder);
                }
            }
        }

        private bool _allowAddSaleOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddSaleOrder.
        /// </summary>
        public bool AllowAddSaleOrder
        {
            get { return _allowAddSaleOrder; }
            set
            {
                if (_allowAddSaleOrder != value)
                {
                    _allowAddSaleOrder = value;
                    OnPropertyChanged(() => AllowAddSaleOrder);
                }
            }
        }

        private bool _allowDeleteProductSalesOrder = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProductSalesOrder.
        /// </summary>
        public bool AllowDeleteProductSalesOrder
        {
            get { return _allowDeleteProductSalesOrder; }
            set
            {
                if (_allowDeleteProductSalesOrder != value)
                {
                    _allowDeleteProductSalesOrder = value;
                    OnPropertyChanged(() => AllowDeleteProductSalesOrder);
                }
            }
        }

        private bool _allowSalesOrderShipping = true;
        /// <summary>
        /// Gets or sets the AllowSalesOrderShipping.
        /// </summary>
        public bool AllowSalesOrderShipping
        {
            get { return _allowSalesOrderShipping; }
            set
            {
                if (_allowSalesOrderShipping != value)
                {
                    _allowSalesOrderShipping = value;
                    OnPropertyChanged(() => AllowSalesOrderShipping);
                }
            }
        }

        private bool _allowSalesOrderReturn = true;
        /// <summary>
        /// Gets or sets the AllowSalesOrderReturn.
        /// </summary>
        public bool AllowSalesOrderReturn
        {
            get { return _allowSalesOrderReturn; }
            set
            {
                if (_allowSalesOrderReturn != value)
                {
                    _allowSalesOrderReturn = value;
                    OnPropertyChanged(() => AllowSalesOrderReturn);
                }
            }
        }

        #endregion

        #endregion

        #region Purchase Module

        private bool _allowAccessPurchaseModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessPurchaseModule.
        /// </summary>
        public bool AllowAccessPurchaseModule
        {
            get { return _allowAccessPurchaseModule; }
            set
            {
                if (_allowAccessPurchaseModule != value)
                {
                    _allowAccessPurchaseModule = value;
                    OnPropertyChanged(() => AllowAccessPurchaseModule);
                }
            }
        }

        private bool _allowAccessVendor = true;
        /// <summary>
        /// Gets or sets the AllowAccessVendor.
        /// </summary>
        public bool AllowAccessVendor
        {
            get { return _allowAccessVendor; }
            set
            {
                if (_allowAccessVendor != value)
                {
                    _allowAccessVendor = value;
                    OnPropertyChanged(() => AllowAccessVendor);
                }
            }
        }

        private bool _allowAddVendor = true;
        /// <summary>
        /// Gets or sets the AllowAddVendor.
        /// </summary>
        public bool AllowAddVendor
        {
            get { return _allowAddVendor; }
            set
            {
                if (_allowAddVendor != value)
                {
                    _allowAddVendor = value;
                    OnPropertyChanged(() => AllowAddVendor);
                }
            }
        }

        private bool _allowAccessPurchaseOrder = true;
        /// <summary>
        /// Gets or sets the allowAccessPurchaseOrder.
        /// </summary>
        public bool AllowAccessPurchaseOrder
        {
            get { return _allowAccessPurchaseOrder; }
            set
            {
                if (_allowAccessPurchaseOrder != value)
                {
                    _allowAccessPurchaseOrder = value;
                    OnPropertyChanged(() => AllowAccessPurchaseOrder);
                }
            }
        }

        private bool _allowAddPurchaseOrder = true;
        /// <summary>
        /// Gets or sets the AllowAddPO.
        /// </summary>
        public bool AllowAddPurchaseOrder
        {
            get { return _allowAddPurchaseOrder; }
            set
            {
                if (_allowAddPurchaseOrder != value)
                {
                    _allowAddPurchaseOrder = value;
                    OnPropertyChanged(() => AllowAddPurchaseOrder);
                }
            }
        }

        private bool _allowPurchaseReceive = true;
        /// <summary>
        /// Gets or sets the AllowPurchaseReceive.
        /// </summary>
        public bool AllowPurchaseReceive
        {
            get { return _allowPurchaseReceive; }
            set
            {
                if (_allowPurchaseReceive != value)
                {
                    _allowPurchaseReceive = value;
                    OnPropertyChanged(() => AllowPurchaseReceive);
                }
            }
        }

        private bool _allowPurchaseOrderReturn = true;
        /// <summary>
        /// Gets or sets the AllowPurchaseOrderReturn.
        /// </summary>
        public bool AllowPurchaseOrderReturn
        {
            get { return _allowPurchaseOrderReturn; }
            set
            {
                if (_allowPurchaseOrderReturn != value)
                {
                    _allowPurchaseOrderReturn = value;
                    OnPropertyChanged(() => AllowPurchaseOrderReturn);
                }
            }
        }

        private bool _allowDeleteProductPurchaseOrder = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProductPurchaseOrder.
        /// </summary>
        public bool AllowDeleteProductPurchaseOrder
        {
            get { return _allowDeleteProductPurchaseOrder; }
            set
            {
                if (_allowDeleteProductPurchaseOrder != value)
                {
                    _allowDeleteProductPurchaseOrder = value;
                    OnPropertyChanged(() => AllowDeleteProductPurchaseOrder);
                }
            }
        }

        #endregion

        #region Inventory Module

        private bool _allowAccessInventoryModule = true;
        /// <summary>
        /// Gets or sets the AllowAccessProductModule.
        /// </summary>
        public bool AllowAccessInventoryModule
        {
            get { return _allowAccessInventoryModule; }
            set
            {
                if (_allowAccessInventoryModule != value)
                {
                    _allowAccessInventoryModule = value;
                    OnPropertyChanged(() => AllowAccessInventoryModule);
                }
            }
        }

        #region Product

        /// <summary>
        /// Gets the AllowAccessProductPermission.
        /// </summary>
        public bool AllowAccessProductPermission
        {
            get { return IsMainStore; }
        }

        private bool _allowAccessProduct = true;
        /// <summary>
        /// Gets or sets the AllowAccessProduct.
        /// </summary>
        public bool AllowAccessProduct
        {
            get { return _allowAccessProduct; }
            set
            {
                if (_allowAccessProduct != value)
                {
                    _allowAccessProduct = value;
                    OnPropertyChanged(() => AllowAccessProduct);
                }
            }
        }

        private bool _allowAddProduct = true;
        /// <summary>
        /// Gets or sets the AllowAddProduct.
        /// </summary>
        public bool AllowAddProduct
        {
            get { return _allowAddProduct; }
            set
            {
                if (_allowAddProduct != value)
                {
                    _allowAddProduct = value;
                    OnPropertyChanged(() => AllowAddProduct);
                }
            }
        }

        private bool _allowEditQuantity = true;
        /// <summary>
        /// Gets or sets the AllowEditQuantity.
        /// </summary>
        public bool AllowEditQuantity
        {
            get { return _allowEditQuantity; }
            set
            {
                if (_allowEditQuantity != value)
                {
                    _allowEditQuantity = value;
                    OnPropertyChanged(() => AllowEditQuantity);
                }
            }
        }

        private bool _allowDeleteProduct = true;
        /// <summary>
        /// Gets or sets the AllowDeleteProduct.
        /// </summary>
        public bool AllowDeleteProduct
        {
            get { return _allowDeleteProduct; }
            set
            {
                if (_allowDeleteProduct != value)
                {
                    _allowDeleteProduct = value;
                    OnPropertyChanged(() => AllowDeleteProduct);
                }
            }
        }

        private bool _allowSaleProduct = true;
        /// <summary>
        /// Gets or sets the AllowSaleProduct.
        /// </summary>
        public bool AllowSaleProduct
        {
            get { return _allowSaleProduct; }
            set
            {
                if (_allowSaleProduct != value)
                {
                    _allowSaleProduct = value;
                    OnPropertyChanged(() => AllowSaleProduct);
                }
            }
        }

        private bool _allowReceiveProduct = true;
        /// <summary>
        /// Gets or sets the AllowReceiveProduct.
        /// </summary>
        public bool AllowReceiveProduct
        {
            get { return _allowReceiveProduct; }
            set
            {
                if (_allowReceiveProduct != value)
                {
                    _allowReceiveProduct = value;
                    OnPropertyChanged(() => AllowReceiveProduct);
                }
            }
        }

        private bool _allowTransferProduct = true;
        /// <summary>
        /// Gets or sets the AllowTransferProduct.
        /// </summary>
        public bool AllowTransferProduct
        {
            get { return _allowTransferProduct; }
            set
            {
                if (_allowTransferProduct != value)
                {
                    _allowTransferProduct = value;
                    OnPropertyChanged(() => AllowTransferProduct);
                }
            }
        }

        private bool _allowEditPrice = true;
        /// <summary>
        /// Gets or sets the AllowEditPrice.
        /// </summary>
        public bool AllowEditPrice
        {
            get { return _allowEditPrice; }
            set
            {
                if (_allowEditPrice != value)
                {
                    _allowEditPrice = value;
                    OnPropertyChanged(() => AllowEditPrice);
                }
            }
        }

        private bool _allowEditCost = true;
        /// <summary>
        /// Gets or sets the AllowEditCost.
        /// </summary>
        public bool AllowEditCost
        {
            get { return _allowEditCost; }
            set
            {
                if (_allowEditCost != value)
                {
                    _allowEditCost = value;
                    OnPropertyChanged(() => AllowEditCost);
                }
            }
        }

        private bool _allowAddProductImage = true;
        /// <summary>
        /// Gets or sets the AllowAddProductImage.
        /// </summary>
        public bool AllowAddProductImage
        {
            get { return _allowAddProductImage; }
            set
            {
                if (_allowAddProductImage != value)
                {
                    _allowAddProductImage = value;
                    OnPropertyChanged(() => AllowAddProductImage);
                }
            }
        }

        #endregion

        private bool _allowAddDepartment = true;
        /// <summary>
        /// Gets or sets the AllowAddDepartment.
        /// </summary>
        public bool AllowAddDepartment
        {
            get { return _allowAddDepartment; }
            set
            {
                if (_allowAddDepartment != value)
                {
                    _allowAddDepartment = value;
                    OnPropertyChanged(() => AllowAddDepartment);
                }
            }
        }

        private bool _allowAccessPricing = true;
        /// <summary>
        /// Gets or sets the AllowAccessPricing.
        /// </summary>
        public bool AllowAccessPricing
        {
            get { return _allowAccessPricing; }
            set
            {
                if (_allowAccessPricing != value)
                {
                    _allowAccessPricing = value;
                    OnPropertyChanged(() => AllowAccessPricing);
                }
            }
        }

        private bool _allowAddPricing = true;
        /// <summary>
        /// Gets or sets the AllowAddPricing.
        /// </summary>
        public bool AllowAddPricing
        {
            get { return _allowAddPricing; }
            set
            {
                if (_allowAddPricing != value)
                {
                    _allowAddPricing = value;
                    OnPropertyChanged(() => AllowAddPricing);
                }
            }
        }

        private bool _allowAccessDiscountProgram = true;
        /// <summary>
        /// Gets or sets the AllowAccessDiscountProgram.
        /// </summary>
        public bool AllowAccessDiscountProgram
        {
            get { return _allowAccessDiscountProgram; }
            set
            {
                if (_allowAccessDiscountProgram != value)
                {
                    _allowAccessDiscountProgram = value;
                    OnPropertyChanged(() => AllowAccessDiscountProgram);
                }
            }
        }

        private bool _allowAddPromotion = true;
        /// <summary>
        /// Gets or sets the AllowAddPromotion.
        /// </summary>
        public bool AllowAddPromotion
        {
            get { return _allowAddPromotion; }
            set
            {
                if (_allowAddPromotion != value)
                {
                    _allowAddPromotion = value;
                    OnPropertyChanged(() => AllowAddPromotion);
                }
            }
        }

        private bool _allowAccessStock = true;
        /// <summary>
        /// Gets or sets the AllowAccessCurrentStock.
        /// </summary>
        public bool AllowAccessStock
        {
            get { return _allowAccessStock; }
            set
            {
                if (_allowAccessStock != value)
                {
                    _allowAccessStock = value;
                    OnPropertyChanged(() => AllowAccessStock);
                }
            }
        }

        private bool _allowViewCurrentStock = true;
        /// <summary>
        /// Gets or sets the AllowViewCurrentStock.
        /// </summary>
        public bool AllowViewCurrentStock
        {
            get { return _allowViewCurrentStock; }
            set
            {
                if (_allowViewCurrentStock != value)
                {
                    _allowViewCurrentStock = value;
                    OnPropertyChanged(() => AllowViewCurrentStock);
                }
            }
        }

        private bool _allowAddCountSheet = true;
        /// <summary>
        /// Gets or sets the AllowAddCountSheet.
        /// </summary>
        public bool AllowAddCountSheet
        {
            get { return _allowAddCountSheet; }
            set
            {
                if (_allowAddCountSheet != value)
                {
                    _allowAddCountSheet = value;
                    OnPropertyChanged(() => AllowAddCountSheet);
                }
            }
        }

        private bool _allowAddTransferStock = true;
        /// <summary>
        /// Gets or sets the AllowAddTransferStock.
        /// </summary>
        public bool AllowAddTransferStock
        {
            get { return _allowAddTransferStock; }
            set
            {
                if (_allowAddTransferStock != value)
                {
                    _allowAddTransferStock = value;
                    OnPropertyChanged(() => AllowAddTransferStock);
                }
            }
        }

        private bool _allowAccessAdjustHistory = true;
        /// <summary>
        /// Gets or sets the AllowAccessAdjustHistory.
        /// </summary>
        public bool AllowAccessAdjustHistory
        {
            get { return _allowAccessAdjustHistory; }
            set
            {
                if (_allowAccessAdjustHistory != value)
                {
                    _allowAccessAdjustHistory = value;
                    OnPropertyChanged(() => AllowAccessAdjustHistory);
                }
            }
        }

        private bool _allowAccessCostAdjustment = true;
        /// <summary>
        /// Gets or sets the AllowAccessCostAdjustment.
        /// </summary>
        public bool AllowAccessCostAdjustment
        {
            get { return _allowAccessCostAdjustment; }
            set
            {
                if (_allowAccessCostAdjustment != value)
                {
                    _allowAccessCostAdjustment = value;
                    OnPropertyChanged(() => AllowAccessCostAdjustment);
                }
            }
        }

        private bool _allowAccessQuantityAdjustment = true;
        /// <summary>
        /// Gets or sets the AllowAccessQuantityAdjustment.
        /// </summary>
        public bool AllowAccessQuantityAdjustment
        {
            get { return _allowAccessQuantityAdjustment; }
            set
            {
                if (_allowAccessQuantityAdjustment != value)
                {
                    _allowAccessQuantityAdjustment = value;
                    OnPropertyChanged(() => AllowAccessQuantityAdjustment);
                }
            }
        }

        #endregion

        #region Configuration Module

        private bool _allowChangeConfiguration = true;
        /// <summary>
        /// Gets or sets the AllowChangeConfiguration.
        /// </summary>
        public bool AllowChangeConfiguration
        {
            get { return _allowChangeConfiguration; }
            set
            {
                if (_allowChangeConfiguration != value)
                {
                    _allowChangeConfiguration = value;
                    OnPropertyChanged(() => AllowChangeConfiguration);
                }
            }
        }

        private bool _allowEditAttachment = true;
        /// <summary>
        /// Gets or sets the AllowEditAttachment.
        /// </summary>
        public bool AllowEditAttachment
        {
            get { return _allowEditAttachment; }
            set
            {
                if (_allowEditAttachment != value)
                {
                    _allowEditAttachment = value;
                    OnPropertyChanged(() => AllowEditAttachment);
                }
            }
        }

        private bool _allowEditDocument = true;
        /// <summary>
        /// Gets or sets the AllowEditDocument.
        /// </summary>
        public bool AllowEditDocument
        {
            get { return _allowEditDocument; }
            set
            {
                if (_allowEditDocument != value)
                {
                    _allowEditDocument = value;
                    OnPropertyChanged(() => AllowEditDocument);
                }
            }
        }

        private bool _allowDeleteSaleTax = true;
        /// <summary>
        /// Gets or sets the AllowDeleteSaleTax.
        /// </summary>
        public bool AllowDeleteSaleTax
        {
            get { return _allowDeleteSaleTax; }
            set
            {
                if (_allowDeleteSaleTax != value)
                {
                    _allowDeleteSaleTax = value;
                    OnPropertyChanged(() => AllowDeleteSaleTax);
                }
            }
        }

        private bool _allowEditSaleTax = true;
        /// <summary>
        /// Gets or sets the AllowEditSaleTax.
        /// </summary>
        public bool AllowEditSaleTax
        {
            get { return _allowEditSaleTax; }
            set
            {
                if (_allowEditSaleTax != value)
                {
                    _allowEditSaleTax = value;
                    OnPropertyChanged(() => AllowEditSaleTax);
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public UserPermissions()
        {
            GetPermission();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get permission for CashIn or CashOut
        /// </summary>
        public void GetCashInOutPermission()
        {
            try
            {
                DateTime current = DateTime.Now.Date;

                // Get cash flow by user and shift
                base_CashFlow cashFlow = null;
                if (Define.ShiftCode == null)
                    cashFlow = _cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current);
                else
                    cashFlow = _cashFlowRepository.Get(x => x.CashierResource == Define.USER.UserResource && x.OpenDate == current && x.Shift.Equals(Define.ShiftCode));

                AllowAccessCashIn = false;
                AllowAccessCashOut = false;

                if (cashFlow == null)
                {
                    AllowAccessCashIn = !IsAdminPermission;
                }
                else if (!cashFlow.IsCashOut)
                {
                    AllowAccessCashOut = !IsAdminPermission;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Get permissions
        /// </summary>
        public void GetPermission()
        {
            GetCashInOutPermission();

            if (!IsAdminPermission)
            {
                if (IsFullPermission)
                {
                    // Set default permission
                    #region Sale Module

                    AllowAccessReward = IsMainStore;

                    #endregion

                    #region Purchase Module

                    AllowAddVendor = IsMainStore;

                    AllowAddPurchaseOrder = IsMainStore;

                    #endregion

                    #region Inventory Module

                    #region Product

                    AllowAddProduct = IsMainStore;
                    AllowEditQuantity = IsMainStore;
                    AllowDeleteProduct = IsMainStore;
                    AllowReceiveProduct = IsMainStore;
                    AllowTransferProduct = IsMainStore;
                    AllowEditPrice = IsMainStore;
                    AllowEditCost = IsMainStore;
                    AllowAddProductImage = IsMainStore;

                    #endregion

                    AllowAddDepartment = IsMainStore;

                    AllowAddPricing = IsMainStore;

                    AllowAddPromotion = IsMainStore;

                    AllowAccessCostAdjustment = IsMainStore;

                    AllowAccessQuantityAdjustment = IsMainStore;

                    #endregion

                    #region Configuration Module

                    AllowChangeConfiguration = IsMainStore;

                    #endregion
                }
                else
                {
                    // Get all user rights
                    IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                    // Get restore data permission
                    AllowRestoreData = userRightCodes.Contains("MN200");

                    #region Sale Module

                    // Get access sale module permission
                    AllowAccessSaleModule = userRightCodes.Contains("SO100");

                    #region Customer

                    // Get access customer permission
                    AllowAccessCustomer = userRightCodes.Contains("SO100-01") && AllowAccessSaleModule;

                    // Get add/copy customer permission
                    AllowAddCustomer = userRightCodes.Contains("SO100-01-01") && AllowAccessCustomer;

                    // Get access payment permission
                    AllowAccessPayment = userRightCodes.Contains("SO100-01-05");

                    // Get access reward permission
                    AllowAccessCustomerReward = userRightCodes.Contains("SO100-01-06");

                    // Get access sale order history permission
                    AllowAccessSOHistory = userRightCodes.Contains("SO100-01-07");

                    // Union add/copy sale order and sale from customer permission
                    AllowSaleFromCustomer = userRightCodes.Contains("SO100-04-02") && userRightCodes.Contains("SO100-01-08");

                    // Get allow manual reward permission
                    AllowManualReward = userRightCodes.Contains("SO100-02-02");

                    #endregion

                    // Get access reward permission
                    AllowAccessReward = userRightCodes.Contains("SO100-02") && AllowAccessSaleModule && IsMainStore;

                    #region Quotation

                    // Get access sale quotation permission
                    AllowAccessSaleQuotation = userRightCodes.Contains("SO100-03") && AllowAccessSaleModule;

                    // Get add/copy sale quotation permission
                    AllowAddSaleQuotation = userRightCodes.Contains("SO100-03-01") && AllowAccessSaleQuotation;

                    // Get delete product in quotation permission
                    AllowDeleteProductQuotation = userRightCodes.Contains("SO100-03-07");

                    // Get sale quotation permission
                    AllowConvertToSalesOrder = userRightCodes.Contains("SO100-03-08");

                    #endregion

                    #region Layaway

                    // Get access layaway permission
                    AllowAccessLayaway = userRightCodes.Contains("SO100-05") && AllowAccessSaleModule;

                    // Get add/copy layaway permission
                    AllowAddLayaway = userRightCodes.Contains("SO100-05-02") && AllowAccessLayaway;

                    // Get delete product in layaway permission
                    AllowDeleteProductLayaway = userRightCodes.Contains("SO100-05-08");

                    #endregion

                    #region WorkOrder

                    // Get access work order permission
                    AllowAccessWorkOrder = userRightCodes.Contains("SO100-06") && AllowAccessSaleModule;

                    // Get add/copy work order permission
                    AllowAddWorkOrder = userRightCodes.Contains("SO100-06-02") && AllowAccessWorkOrder;

                    // Union sale work order and add/copy sale order permission
                    AllowSaleWorkOrder = userRightCodes.Contains("SO100-06-04") && userRightCodes.Contains("SO100-04-02");

                    // Get delete product in work order permission
                    AllowDeleteProductWorkOrder = userRightCodes.Contains("SO100-06-08");

                    #endregion

                    #region SalesOrder

                    // Get access sale order permission
                    AllowAccessSaleOrder = userRightCodes.Contains("SO100-04") && AllowAccessSaleModule;

                    // Get add/copy sale order permission
                    AllowAddSaleOrder = userRightCodes.Contains("SO100-04-02") && AllowAccessSaleOrder;

                    // Get delete product in sale order permission
                    AllowDeleteProductSalesOrder = userRightCodes.Contains("SO100-04-13");

                    // Get sale order shipping permission
                    AllowSalesOrderShipping = userRightCodes.Contains("SO100-04-11");

                    // Get sale order return permission
                    AllowSalesOrderReturn = userRightCodes.Contains("SO100-04-05");

                    #endregion

                    #endregion

                    #region Purchase Module

                    // Get access purchase module permission
                    AllowAccessPurchaseModule = userRightCodes.Contains("PO100");

                    #region Vendor

                    // Get access vendor permission
                    AllowAccessVendor = userRightCodes.Contains("PO100-01") && AllowAccessPurchaseModule;

                    // Get add/copy vendor permission
                    AllowAddVendor = userRightCodes.Contains("PO100-01-01") && AllowAccessVendor && IsMainStore;

                    #endregion

                    #region PurchaseOrder

                    // Get access purchase order permission
                    AllowAccessPurchaseOrder = userRightCodes.Contains("PO100-02") && AllowAccessPurchaseModule;

                    // Get add purchase order permission
                    AllowAddPurchaseOrder = userRightCodes.Contains("PO100-02-02") && AllowAccessPurchaseOrder && IsMainStore;

                    // Get purchase order receive permission
                    AllowPurchaseReceive = userRightCodes.Contains("PO100-02-04");

                    // Get purchase order return permission
                    AllowPurchaseOrderReturn = userRightCodes.Contains("PO100-02-05");

                    // Get delete product in purchase order permission
                    AllowDeleteProductPurchaseOrder = userRightCodes.Contains("PO100-02-10");

                    #endregion

                    #endregion

                    #region Inventory Module

                    // Get access inventory module permission
                    AllowAccessInventoryModule = userRightCodes.Contains("IV100");

                    #region Product

                    // Get access product permission
                    AllowAccessProduct = userRightCodes.Contains("IV100-01") && AllowAccessInventoryModule;

                    // Get add/copy product permission
                    AllowAddProduct = userRightCodes.Contains("IV100-01-01") && AllowAccessProduct && IsMainStore;

                    // Get edit quantity permission
                    AllowEditQuantity = userRightCodes.Contains("IV100-01-07") && IsMainStore;

                    // Get delete product permission
                    AllowDeleteProduct = userRightCodes.Contains("IV100-01-04") && IsMainStore;

                    // Union sale product and add/copy sale order permission
                    AllowSaleProduct = userRightCodes.Contains("IV100-01-11") && userRightCodes.Contains("SO100-04-02");

                    // Union receive product and add/copy purchase order permission
                    AllowReceiveProduct = userRightCodes.Contains("IV100-01-12") && userRightCodes.Contains("PO100-02-02") && IsMainStore;

                    // Union transfer product and add transfer stock permission
                    AllowTransferProduct = userRightCodes.Contains("IV100-01-13") && userRightCodes.Contains("IV100-04-05") && IsMainStore;

                    // Get edit price permission
                    AllowEditPrice = userRightCodes.Contains("IV100-01-05") && IsMainStore;

                    // Get edit cost permission
                    AllowEditCost = userRightCodes.Contains("IV100-01-06") && IsMainStore;

                    // Get add/change/delete product image permission
                    AllowAddProductImage = userRightCodes.Contains("IV100-01-08") && IsMainStore;

                    #endregion

                    // Get add/copy department permission
                    AllowAddDepartment = userRightCodes.Contains("IV100-01-03") && AllowAccessProduct && IsMainStore;

                    // Get access pricing permission
                    AllowAccessPricing = userRightCodes.Contains("IV100-02") && AllowAccessInventoryModule;

                    // Get add/copy pricing permission
                    AllowAddPricing = userRightCodes.Contains("IV100-02-01") && AllowAccessPricing && IsMainStore;

                    // Get access discount program permission
                    AllowAccessDiscountProgram = userRightCodes.Contains("IV100-03") && AllowAccessInventoryModule;

                    // Get add/copy promotion permission
                    AllowAddPromotion = userRightCodes.Contains("IV100-03-01") && AllowAccessDiscountProgram && IsMainStore;

                    // Get access stock permission
                    AllowAccessStock = userRightCodes.Contains("IV100-04") && AllowAccessInventoryModule;

                    // Get view current stock permission
                    AllowViewCurrentStock = userRightCodes.Contains("IV100-04-01") && AllowAccessStock;

                    // Get add count sheet permission
                    AllowAddCountSheet = userRightCodes.Contains("IV100-04-02") && AllowAccessStock;

                    // Get add transfer stock permission
                    AllowAddTransferStock = userRightCodes.Contains("IV100-04-05") && AllowAccessStock;

                    // Get access adjust history permission
                    AllowAccessAdjustHistory = userRightCodes.Contains("IV100-05") && AllowAccessInventoryModule;

                    // Get access cost adjustment permission
                    AllowAccessCostAdjustment = userRightCodes.Contains("IV100-05-01") && AllowAccessAdjustHistory && IsMainStore;

                    // Get access quantity adjustment permission
                    AllowAccessQuantityAdjustment = userRightCodes.Contains("IV100-05-02") && AllowAccessAdjustHistory && IsMainStore;

                    #endregion

                    #region Configuration Module

                    // Get change configuration permission
                    AllowChangeConfiguration = userRightCodes.Contains("CF100-05") && IsMainStore;

                    // Get edit attachment permission
                    AllowEditAttachment = userRightCodes.Contains("CF100-01-02");

                    // Get edit document permission
                    AllowEditDocument = userRightCodes.Contains("CF100-01-05");

                    // Get edit sale tax permission
                    AllowEditSaleTax = userRightCodes.Contains("CF100-02-01");

                    // Get delete sale tax permission
                    AllowDeleteSaleTax = userRightCodes.Contains("CF100-02-02");

                    #endregion
                }
            }
            else if (!IsMainStore)
            {
                AllowAddProduct = false;
                AllowDeleteProduct = false;
            }
        }

        #endregion
    }
}