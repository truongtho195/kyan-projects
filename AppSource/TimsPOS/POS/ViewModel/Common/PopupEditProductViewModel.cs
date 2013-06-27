using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupEditProductViewModel : ViewModelBase
    {
        #region Defines

        private base_PromotionRepository _promotionRepository = new base_PromotionRepository();
        private base_PromotionAffectRepository _promotionAffectRepository = new base_PromotionAffectRepository();

        private PriceTypes _priceSchemaID = 0;
        private bool _raiseCurrentPrice = true;

        #endregion

        #region Properties

        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                }
            }
        }

        private base_ProductUOMModel _selectedProductUOM;
        /// <summary>
        /// Gets or sets the SelectedProductUOM.
        /// </summary>
        public base_ProductUOMModel SelectedProductUOM
        {
            get { return _selectedProductUOM; }
            set
            {
                if (_selectedProductUOM != value)
                {
                    _selectedProductUOM = value;
                    OnPropertyChanged(() => SelectedProductUOM);
                    if (SelectedProductUOM != null)
                        OnSelectedProductUOMChanged();
                }
            }
        }

        private ObservableCollection<base_PromotionModel> _promotionCollection;
        /// <summary>
        /// Gets or sets the PromotionCollection.
        /// </summary>
        public ObservableCollection<base_PromotionModel> PromotionCollection
        {
            get { return _promotionCollection; }
            set
            {
                if (_promotionCollection != value)
                {
                    _promotionCollection = value;
                    OnPropertyChanged(() => PromotionCollection);
                }
            }
        }

        private base_PromotionModel _selectedPromotion;
        /// <summary>
        /// Gets or sets the SelectedPromotion.
        /// </summary>
        public base_PromotionModel SelectedPromotion
        {
            get { return _selectedPromotion; }
            set
            {
                if (_selectedPromotion != value)
                {
                    _selectedPromotion = value;
                    OnPropertyChanged(() => SelectedPromotion);
                    if (SelectedPromotion != null)
                        OnSelectedPromotionChanged();
                }
            }
        }

        private decimal _discountPercent;
        /// <summary>
        /// Gets or sets the DiscountPercent.
        /// </summary>
        public decimal DiscountPercent
        {
            get { return _discountPercent; }
            set
            {
                if (_discountPercent != value)
                {
                    _discountPercent = value;
                    OnPropertyChanged(() => DiscountPercent);
                    OnDiscountPercentChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsEditFromPO
        /// </summary>
        public bool IsEditFromPO { get; private set; }

        /// <summary>
        /// Gets or sets the IsEditUOM
        /// </summary>
        public bool IsEditUOM { get; private set; }

        private bool _isDiscountManual;
        /// <summary>
        /// Gets or sets the IsDiscountManual.
        /// </summary>
        public bool IsDiscountManual
        {
            get { return _isDiscountManual; }
            set
            {
                if (_isDiscountManual != value)
                {
                    _isDiscountManual = value;
                    OnPropertyChanged(() => IsDiscountManual);
                }
            }
        }

        /// <summary>
        /// Gets the IsEditPromotion.
        /// </summary>
        public bool IsEditPromotion
        {
            get { return SelectedProductUOM.UOMId.Equals(SelectedProduct.BaseUOMId); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="productModel"></param>
        public PopupEditProductViewModel(base_ProductModel productModel, bool isEditUOM)
        {
            InitialCommand();

            IsEditUOM = isEditUOM;

            // Update selected product
            SelectedProduct = productModel;

            // Set product UOM default
            SelectedProductUOM = SelectedProduct.ProductUOMCollection.SingleOrDefault(x => x.UOMId.Equals(SelectedProduct.BaseUOMId));

            // Register property changed event
            SelectedProduct.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SelectedProduct_PropertyChanged);
        }

        public PopupEditProductViewModel(base_ProductModel productModel, PriceTypes priceSchemaId, bool isEditUOM, int promotionID)
            : this(productModel, isEditUOM)
        {
            _priceSchemaID = priceSchemaId;

            // Load promotion collection
            LoadPromotionCollection(SelectedProduct, promotionID);

            // Update Amount
            if (SelectedPromotion == null)
                SelectedPromotion = PromotionCollection.FirstOrDefault();
        }

        public PopupEditProductViewModel(base_ProductModel productModel, bool isEditFromPO, bool isEditUOM)
            : this(productModel, isEditUOM)
        {
            IsEditFromPO = isEditFromPO;

            SelectedProduct.Amount = SelectedProduct.CurrentPrice * SelectedProduct.OnHandStore;
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Load promotion collection
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadPromotionCollection(base_ProductModel productModel, int promotionID)
        {
            if (PromotionCollection == null)
            {
                // Initial promotion collection
                PromotionCollection = new ObservableCollection<base_PromotionModel>();

                foreach (base_PromotionModel promotionItem in _promotionRepository.GetAll().OrderByDescending(x => x.DateUpdated).Select(x => new base_PromotionModel(x)))
                {
                    // Check selected product is affected by promotion
                    bool isAffected = false;
                    switch (promotionItem.AffectDiscount)
                    {
                        case 0:
                            isAffected = true;
                            break;
                        case 1:
                            if (productModel.ProductCategoryId.Equals(promotionItem.CategoryId))
                                isAffected = true;
                            break;
                        case 2:
                            if (productModel.VendorId.Equals(promotionItem.VendorId))
                                isAffected = true;
                            break;
                        case 3:
                            base_PromotionAffect promotionAffect = _promotionAffectRepository.Get(x => x.ItemId.Equals(productModel.Id));
                            if (promotionAffect != null)
                                isAffected = true;
                            break;
                    }

                    // Check promotion is expired
                    bool isExpired = false;
                    promotionItem.PromotionScheduleModel = new base_PromotionScheduleModel(promotionItem.base_Promotion.base_PromotionSchedule.SingleOrDefault());
                    if (promotionItem.PromotionScheduleModel.EndDate != null)
                    {
                        DateTime today = DateTimeExt.Today;
                        if (promotionItem.PromotionScheduleModel.StartDate > today || today > promotionItem.PromotionScheduleModel.EndDate)
                            isExpired = true;
                    }

                    // Check selected price schema id have in selected promotion range
                    if (isAffected && !isExpired && _priceSchemaID.In(promotionItem.PriceSchemaRange))
                        PromotionCollection.Add(promotionItem);
                }

                // Set promotion default
                SelectedPromotion = PromotionCollection.FirstOrDefault(x => x.Id.Equals(promotionID));

                PromotionCollection.Insert(0, new base_PromotionModel { Name = "Discount Manual" });
            }
        }

        /// <summary>
        /// Processs when selected product UOM changed
        /// </summary>
        private void OnSelectedProductUOMChanged()
        {
            if (IsEditFromPO)
            {
                // Get regurlar price
                SelectedProduct.CurrentPrice = SelectedProductUOM.RegularPrice;
                return;
            }

            // Get regular price by price schema
            switch (_priceSchemaID)
            {
                case PriceTypes.RegularPrice:
                    SelectedProduct.RegularPrice = SelectedProductUOM.RegularPrice;
                    break;
                case PriceTypes.SalePrice:
                    SelectedProduct.RegularPrice = SelectedProductUOM.Price1;
                    break;
                case PriceTypes.WholesalePrice:
                    SelectedProduct.RegularPrice = SelectedProductUOM.Price2;
                    break;
                case PriceTypes.Employee:
                    SelectedProduct.RegularPrice = SelectedProductUOM.Price3;
                    break;
                case PriceTypes.CustomPrice:
                    SelectedProduct.RegularPrice = SelectedProductUOM.Price4;
                    break;
            }

            OnPropertyChanged(() => IsEditPromotion);
            if (!IsEditPromotion)
            {
                // Switch promotion to manual when selected product UOM difference with base UOM
                SelectedPromotion = PromotionCollection.FirstOrDefault();
            }
        }

        /// <summary>
        /// Process when selected promotion changed
        /// </summary>
        private void OnSelectedPromotionChanged()
        {
            if (IsEditPromotion)
            {
                if (SelectedPromotion.TakeOffOption == 1)
                {
                    // Get discount percent when promotion discount by percent
                    DiscountPercent = SelectedPromotion.TakeOff;
                }
                else if (SelectedProduct.RegularPrice != 0)
                {
                    // Update current price from discount percent
                    SelectedProduct.CurrentPrice = SelectedProduct.RegularPrice - SelectedPromotion.TakeOff;
                }
            }
            else
            {
                // Get discount percent when have no discount
                DiscountPercent = 0;
            }

            // Turn on/off discount manual
            IsDiscountManual = SelectedPromotion.Id == 0;
        }

        /// <summary>
        /// Process when discount percent changed
        /// </summary>
        private void OnDiscountPercentChanged()
        {
            // Turn off update discount percent
            _raiseCurrentPrice = false;

            // Update current price by discount percent
            SelectedProduct.CurrentPrice = Math.Round(SelectedProduct.RegularPrice * (1 - DiscountPercent / 100), 2);

            // Turn on update discount percent
            _raiseCurrentPrice = true;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Process when product property changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedProduct_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Get product model
            base_ProductModel productModel = sender as base_ProductModel;

            switch (e.PropertyName)
            {
                case "CurrentPrice":
                    if (!IsEditFromPO && _raiseCurrentPrice)
                    {
                        // Update discount percent
                        if (productModel.RegularPrice != 0)
                            _discountPercent = 100 - Math.Round(productModel.CurrentPrice * 100 / productModel.RegularPrice, 2);
                        else
                            _discountPercent = 0;
                        OnPropertyChanged(() => DiscountPercent);
                    }

                    // Update Amount
                    productModel.Amount = productModel.CurrentPrice * productModel.OnHandStore;
                    break;
                case "OnHandStore":
                    // Update Amount
                    productModel.Amount = productModel.CurrentPrice * productModel.OnHandStore;
                    break;
            }
        }

        #endregion
    }
}
