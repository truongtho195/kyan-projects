using System;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class UpdateTransactionViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();

        #endregion

        #region Constructors

        public UpdateTransactionViewModel(base_ProductModel productModel)
        {
            _ownerViewModel = this;

            // Get category name
            if (string.IsNullOrWhiteSpace(productModel.CategoryName))
            {
                base_Department category = _departmentRepository.Get(x => x.LevelId == (short)ProductDeparmentLevel.Category && x.Id == productModel.ProductCategoryId);
                if (category != null)
                    productModel.CategoryName = category.Name;
            }

            ProductModel = productModel.Clone();
            SelectedProductModel = ProductModel;
            InitialCommand();
        }

        #endregion

        #region Properties

        #region IsDirty
        private bool _isDirty = false;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        #endregion

        #region SelectedProductModel
        private base_ProductModel _selectedProductModel;
        /// <summary>
        /// Gets or sets the SelectedProductModel.
        /// </summary>
        public base_ProductModel SelectedProductModel
        {
            get { return _selectedProductModel; }
            set
            {
                if (_selectedProductModel != value)
                {
                    _selectedProductModel = value;
                    OnPropertyChanged(() => SelectedProductModel);
                    SelectedProductChanged();
                }
            }
        }

        private void SelectedProductChanged()
        {
            if (SelectedProductModel != null)
            {
                _newPrice = SelectedProductModel.RegularPrice;
                OnPropertyChanged(() => NewPrice);
            }
        }
        #endregion

        public base_ProductModel ProductModel { get; set; }

        #region IsUpdateProductPrice
        private bool _isUpdateProductPrice;
        /// <summary>
        /// Gets or sets the IsUpdateProductPrice.
        /// </summary>
        public bool IsUpdateProductPrice
        {
            get { return _isUpdateProductPrice; }
            set
            {
                if (_isUpdateProductPrice != value)
                {
                    _isUpdateProductPrice = value;
                    OnPropertyChanged(() => IsUpdateProductPrice);
                }
            }
        }
        #endregion

        #region NewPrice
        private decimal _newPrice;
        /// <summary>
        /// Gets or sets the NewPrice.
        /// </summary>
        public decimal NewPrice
        {
            get { return _newPrice; }
            set
            {
                if (_newPrice != value)
                {
                    _newPrice = value;
                    IsDirty = true;
                    OnPropertyChanged(() => NewPrice);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region UpdateCommand

        public RelayCommand UpdateCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnUpdateCommandCanExecute()
        {
            if (SelectedProductModel == null)
                return false;
            return IsDirty;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnUpdateCommandExecute()
        {
            //Update regular price to product db
            if (IsUpdateProductPrice)
            {
                base_Product product = _productRepository.Get(x => x.Resource.Equals(SelectedProductModel.Resource));
                //Set new price to product
                SelectedProductModel.RegularPrice = NewPrice;
                SelectedProductModel.ToEntity();
                if (product != null)
                {
                    product.RegularPrice = SelectedProductModel.base_Product.RegularPrice;
                    product.UserUpdated = Define.USER != null ? Define.USER.LoginName : product.UserUpdated;
                    product.DateUpdated = DateTime.Now;
                    _productRepository.Commit();
                }
            }
            ProductModel = SelectedProductModel;

            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }

        #endregion

        #region DiscardCommand

        public RelayCommand DiscardCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDiscardCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnDiscardCommandExecute()
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            UpdateCommand = new RelayCommand(OnUpdateCommandExecute, OnUpdateCommandCanExecute);
            DiscardCommand = new RelayCommand(OnDiscardCommandExecute, OnDiscardCommandCanExecute);
        }

        #endregion
    }
}