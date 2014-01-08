using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PricingCategoryViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();

        private bool _isCheckAllFlag = false;
        private bool _isCheckItemFlag = false;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DepartmentList
        /// </summary>
        public List<ComboItem> CategoryList { get; set; }

        private int _totalProducts;
        /// <summary>
        /// Gets or sets the TotalProducts.
        /// </summary>
        public int TotalProducts
        {
            get { return _totalProducts; }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }

        #region IsCheckedAll
        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets the IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get { return _isCheckedAll; }
            set
            {
                if (_isCheckedAll != value)
                {
                    this._isCheckAllFlag = true;
                    _isCheckedAll = value;
                    if (!this._isCheckItemFlag && value.HasValue)
                    {
                        foreach (base_DepartmentModel item in this.CategoryCollection)
                            item.IsChecked = value.Value;
                    }
                    OnPropertyChanged(() => IsCheckedAll);
                    this._isCheckAllFlag = false;
                }
            }
        }
        #endregion


        public int CurrentPageIndexLeft { get; set; }



        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public ObservableCollection<base_DepartmentModel> CategoryCollection { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PricingCategoryViewModel()
        {
            this.InitialCommand();
            this.LoadCategory();
        }

        /// <summary>
        /// Constructor with load data
        /// </summary>
        /// <param name="categoryList">Category list</param>
        /// <param name="promotionAffectList">Promotion affect list</param>
        public PricingCategoryViewModel(List<ComboItem> categoryList, CollectionBase<base_PromotionAffectModel> promotionAffectList)
            : this()
        {

        }

        #endregion

        #region Command Methods

        #region LeftSearchCommand

        /// <summary>
        /// Gets the LeftSearchCommand command.
        /// </summary>
        public ICommand LeftSearchCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the LeftSearchCommand command is executed.
        /// </summary>
        private void OnLeftSearchCommandExecute(object param)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }
        #endregion

        #region MoveCommand

        /// <summary>
        /// Gets the MoveCommand command.
        /// </summary>
        public ICommand MoveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the MoveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMoveCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the MoveCommand command is executed.
        /// </summary>
        private void OnMoveCommandExecute()
        {

        }

        #endregion

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
            return this.CategoryCollection != null && this.CategoryCollection.Count(x => x.IsChecked) > 0;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            this.CategoryList = new List<ComboItem>();
            foreach (var item in this.CategoryCollection.Where(x => x.IsChecked))
                this.CategoryList.Add(new ComboItem { IntValue = item.Id });
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
        private void LoadCategory()
        {
            this.CategoryCollection = new ObservableCollection<base_DepartmentModel>();
            IEnumerable<base_DepartmentModel> departments = _departmentRepository.
                GetAll(x => (x.IsActived.HasValue && x.IsActived.Value)).
                OrderBy(x => x.Name).Where(x => x.LevelId == 1).Select(x => new base_DepartmentModel(x, false));
            foreach (base_DepartmentModel item in departments)
            {
                item.PropertyChanged += new PropertyChangedEventHandler(CategoryModel_PropertyChanged);
                this.CategoryCollection.Add(item);
            }
        }

        private void CategoryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!this._isCheckAllFlag)
                    {
                        this._isCheckItemFlag = true;
                        if (this.CategoryCollection.Count(x => x.IsChecked) == this.CategoryCollection.Count)
                            this.IsCheckedAll = true;
                        else
                            this.IsCheckedAll = false;
                        this._isCheckItemFlag = false;
                        //To change IsCheckedAll property.
                        this.OnPropertyChanged(() => IsCheckedAll);
                    }
                    break;
            }
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Process when left item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    break;
            }
        }

        /// <summary>
        /// Process when right item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    break;
            }
        }

        #endregion
    }
}