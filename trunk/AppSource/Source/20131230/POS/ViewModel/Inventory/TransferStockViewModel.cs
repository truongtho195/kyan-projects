using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using CPC.DragDrop;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;

namespace CPC.POS.ViewModel
{
    class TransferStockViewModel : ViewModelBase, IDropTarget
    {
        #region Define
        //To define repositories to use them in class.
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_TransferStockRepository _transferStockRepository = new base_TransferStockRepository();
        private base_TransferStockDetailRepository _transferStockDetailRepository = new base_TransferStockDetailRepository();
        private base_ProductStoreRepository _productStoreRepository = new base_ProductStoreRepository();
        base_UOMRepository _UOMRepository = new base_UOMRepository();
        //To define commands to use them in class.
        public RelayCommand<object> NewCommand
        {
            get;
            private set;
        }
        public RelayCommand SaveCommand
        {
            get;
            private set;
        }
        public RelayCommand<object> EditCommand
        {
            get;
            private set;
        }
        public RelayCommand DeleteCommand
        {
            get;
            private set;
        }
        public RelayCommand<object> SearchCommand
        {
            get;
            private set;
        }
        public RelayCommand<object> DoubleClickViewCommand
        {
            get;
            private set;
        }
        public RelayCommand<object> TransferCommand
        {
            get;
            private set;
        }
        public RelayCommand RevertCommand
        {
            get;
            private set;
        }
        //To define CollectionView to use them in class.
        private ICollectionView _fromStoreCollectionView;
        private ICollectionView _toStoreCollectionView;
        public bool IsTransferStockInProduct
        {
            get;
            set;
        }
        private bool _isTransferFromProduct = false;
        private object _productCloneCollection = null;


        /// <summary>
        /// Timer for searching
        /// </summary>
        protected DispatcherTimer _waitingTimer;

        /// <summary>
        /// Flag for count timer user input value
        /// </summary>
        protected int _timerCounter = 0;

        #endregion

        #region Constructors

        public TransferStockViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            this.InitialCommand();
            this.LoadStaticData();

            //Initial Auto Complete Search
            if (Define.CONFIGURATION.IsAutoSearch)
            {
                _waitingTimer = new DispatcherTimer();
                _waitingTimer.Interval = new TimeSpan(0, 0, 0, 1);
                _waitingTimer.Tick += new EventHandler(_waitingTimer_Tick);
            }
        }

        public TransferStockViewModel(bool isList, object param = null)
            : this()
        {
            this.ChangeSearchMode(isList, param);
        }

        #endregion

        #region Properties

        #region IsSearchMode
        private bool isSearchMode = false;
        /// <summary>
        /// Search Mode: 
        /// true open the Search grid.
        /// false close the search grid and open data entry.
        /// </summary>
        public bool IsSearchMode
        {
            get
            {
                return isSearchMode;
            }
            set
            {
                if (value != isSearchMode)
                {
                    isSearchMode = value;
                    OnPropertyChanged(() => IsSearchMode);
                }
            }
        }
        #endregion

        #region SelectedTransferStock
        /// <summary>
        /// Gets or sets the SelectedTransferStock.
        /// </summary>
        private base_TransferStockModel _selectedTransferStock;
        public base_TransferStockModel SelectedTransferStock
        {
            get
            {
                return _selectedTransferStock;
            }
            set
            {
                if (_selectedTransferStock != value)
                {
                    _selectedTransferStock = value;
                    this.OnPropertyChanged(() => SelectedTransferStock);
                    if (this.SelectedTransferStock != null)
                        this.SelectedTransferStock.PropertyChanged += new PropertyChangedEventHandler(SelectedTransferStock_PropertyChanged);
                }
            }
        }
        #endregion

        #region SelectedProduct
        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets the SelectedProduct.
        /// </summary>
        public base_ProductModel SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(() => SelectedProduct);
                    this.SelectedProductChanged();
                }
            }
        }
        #endregion

        #region TransferStockCollection
        /// <summary>
        /// Gets or sets the TransferStockCollection.
        /// </summary>
        private ObservableCollection<base_TransferStockModel> _transferStockCollection = new ObservableCollection<base_TransferStockModel>();
        public ObservableCollection<base_TransferStockModel> TransferStockCollection
        {
            get
            {
                return _transferStockCollection;
            }
            set
            {
                if (_transferStockCollection != value)
                {
                    _transferStockCollection = value;
                    OnPropertyChanged(() => TransferStockCollection);
                }
            }
        }

        #endregion

        #region ProductCollection
        /// <summary>
        /// Gets or sets the ProductCollection.
        /// </summary>
        private ObservableCollection<base_ProductModel> _productCollection = new ObservableCollection<base_ProductModel>();
        public ObservableCollection<base_ProductModel> ProductCollection
        {
            get
            {
                return _productCollection;
            }
            set
            {
                if (_productCollection != value)
                {
                    _productCollection = value;
                    OnPropertyChanged(() => ProductCollection);
                }
            }
        }

        #endregion

        #region TotalTransferStock
        private int _totalTransferStock;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalTransferStock
        {

            get
            {
                return _totalTransferStock;
            }
            set
            {
                if (_totalTransferStock != value)
                {
                    _totalTransferStock = value;
                    OnPropertyChanged(() => TotalTransferStock);
                }
            }
        }
        #endregion

        #region TotalProducts
        private int _totalProducts;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalProducts
        {

            get
            {
                return _totalProducts;
            }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }
        #endregion

        #region ProductFieldCollection
        private DataSearchCollection _productFieldCollection;
        /// <summary>
        /// Gets or sets the ProductFieldCollection.
        /// </summary>
        public DataSearchCollection ProductFieldCollection
        {
            get
            {
                return _productFieldCollection;
            }
            set
            {
                if (_productFieldCollection != value)
                {
                    _productFieldCollection = value;
                    OnPropertyChanged(() => ProductFieldCollection);
                }
            }
        }
        #endregion

        #region FromStoreCollection

        private ObservableCollection<base_Store> _fromStoreCollection;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<base_Store> FromStoreCollection
        {
            get
            {
                return _fromStoreCollection;
            }
            set
            {
                if (_fromStoreCollection != value)
                {
                    _fromStoreCollection = value;
                    OnPropertyChanged(() => FromStoreCollection);
                }
            }
        }
        #endregion

        #region ToStoreCollection

        private ObservableCollection<base_Store> _toStoreCollection;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<base_Store> ToStoreCollection
        {
            get
            {
                return _toStoreCollection;
            }
            set
            {
                if (_toStoreCollection != value)
                {
                    _toStoreCollection = value;
                    OnPropertyChanged(() => ToStoreCollection);
                }
            }
        }
        #endregion

        #region Search And Filter

        private int _searchOption;
        /// <summary>
        /// Gets or sets the SearchOption.
        /// </summary>
        public int SearchOption
        {
            get
            {
                return _searchOption;
            }
            set
            {
                if (_searchOption != value)
                {
                    _searchOption = value;
                    OnPropertyChanged(() => SearchOption);
                    if (!string.IsNullOrWhiteSpace(FilterText))
                        this.OnSearchCommandExecute(FilterText);
                }
            }
        }

        private string _filterText;
        /// <summary>
        /// Gets or sets the FilterText.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    ResetTimer();
                    OnPropertyChanged(() => FilterText);
                    this.Keyword = this.FilterText;
                }
            }
        }

        public string Keyword
        {
            get;
            set;
        }

        private string _searchAlert;
        /// <summary>
        /// Gets or sets the SearchAlert.
        /// </summary>
        public string SearchAlert
        {
            get
            {
                return _searchAlert;
            }
            set
            {
                if (_searchAlert != value)
                {
                    _searchAlert = value;
                    OnPropertyChanged(() => SearchAlert);
                }
            }
        }

        #endregion

        #region CurrentPageIndex
        /// <summary>
        /// Gets or sets the CurrentPageIndex.
        /// </summary>
        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get
            {
                return _currentPageIndex;
            }
            set
            {
                _currentPageIndex = value;
                OnPropertyChanged(() => CurrentPageIndex);
            }
        }

        #endregion

        #region SelectedIndex
        /// <summary>
        /// Gets or sets the SelectedIndex.
        /// </summary>
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(() => SelectedIndex);
                }
            }
        }

        #endregion

        #region StoreCollection
        private ObservableCollection<base_Store> _storeCollection;
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<base_Store> StoreCollection
        {
            get { return _storeCollection; }
            set
            {
                if (_storeCollection != value)
                {
                    _storeCollection = value;
                    OnPropertyChanged(() => StoreCollection);
                }
            }
        }
        #endregion

        #region ColumnCollection
        /// <summary>
        /// Gets or sets the ColumnCollection.
        /// </summary>
        public ObservableCollection<string> _columnCollection;
        public ObservableCollection<string> ColumnCollection
        {
            get
            {
                return _columnCollection;
            }
            set
            {
                if (_columnCollection != value)
                {
                    _columnCollection = value;
                    OnPropertyChanged(() => ColumnCollection);
                }
            }

        }
        #endregion

        #region BarcodeProduct
        private string _barcodeProduct;
        /// <summary>
        /// Gets or sets the BarcodeProduct.
        /// </summary>
        public string BarcodeProduct
        {
            get { return _barcodeProduct; }
            set
            {
                if (_barcodeProduct != value)
                {
                    _barcodeProduct = value;
                    OnPropertyChanged(() => BarcodeProduct);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute(object param)
        {
            if (this.ChangeViewExecute(null))
            {
                this.CreateNewTransferStock();
                this.ChangeDataToStore(this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue));
                this.SelectedTransferStock.IsLoad = false;
                this.SelectedTransferStock.IsDirty = false;
            }
        }
        #endregion

        #region EditCommand
        /// <summary>
        /// Method to check whether the EditCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            return (param == null || (param is ObservableCollection<object> && ((param as ObservableCollection<object>).Count == 0 || (param as ObservableCollection<object>).Count > 1))) ? false : true;
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            return this.IsValid && (this.SelectedTransferStock != null && (this.SelectedTransferStock.IsDirty || this.SelectedTransferStock.IsChangeProductCollection));
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            try
            {
                this.SelectedTransferStock.FromStore = Int16.Parse(this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue).ToString());
                this.SelectedTransferStock.ToStore = Int16.Parse(this.FromStoreCollection.IndexOf(this.SelectedTransferStock.ToStoreValue).ToString());
                this.SelectedTransferStock.Shift = Define.ShiftCode;
                // TODO: Handle command logic here
                if (this.SelectedTransferStock.IsNew)
                    this.Insert();
                else
                    this.Update();
                this.SelectedTransferStock.ToModelAndRaise();
                this.SelectedTransferStock.EndUpdate();
                this.SelectedTransferStock.IsChangeProductCollection = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("SaveCommand" + ex.ToString());
            }
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region SearchCommand
        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        private void OnSearchCommandExecute(object param)
        {
            // TODO: Handle command logic here
            try
            {
                if (_waitingTimer != null)
                    _waitingTimer.Stop();

                this.SearchAlert = string.Empty;

                if (string.IsNullOrWhiteSpace(this.Keyword))//Search All
                {
                    Expression<Func<base_TransferStock, bool>> predicate = PredicateBuilder.True<base_TransferStock>();
                    this.LoadTransferStock(predicate, false, 0);
                }
                else
                {
                    Expression<Func<base_TransferStock, bool>> predicate = this.CreateSearchPredicate(this.Keyword);
                    this.LoadTransferStock(predicate, false, 0);
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }
        private bool IsDateTime()
        {
            return (!this.Keyword.Equals("1/1/0001") && this.Keyword.Length > 8);
        }
        #endregion

        #region DoubleClickCommand

        /// <summary>
        /// Method to check whether the DoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            this._isTransferFromProduct = true;
            this._productCloneCollection = null;
            if (param != null && this.IsSearchMode)
            {
                if ((param is ObservableCollection<object>) && (param as ObservableCollection<object>).Count > 0)
                {
                    this.SelectedTransferStock = (param as ObservableCollection<object>)[0] as base_TransferStockModel;
                    //To set detail of TransferStock.
                    this.SetTransferStockDetail();
                    //To show detail form.
                    this.IsSearchMode = false;
                }
            }
            //To add item of TransferStockDetail.
            else if (this.IsSearchMode)
                //To show detail form.
                this.IsSearchMode = false;
            else if (this.ShowNotification(null))
            {
                // Show list form
                this.IsSearchMode = true;
            }
        }
        #endregion

        #region SearchProductAdvanceCommand
        /// <summary>
        /// Gets the SearchProductAdvance Command.
        /// <summary>

        public RelayCommand<object> SearchProductAdvanceCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the SearchProductAdvance command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchProductAdvanceCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchProductAdvance command is executed.
        /// </summary>
        private void OnSearchProductAdvanceCommandExecute(object param)
        {
            this.SearchProductAdvance();
        }
        #endregion

        #region QuantityChangedCommand
        /// <summary>
        /// Gets the QtyChanged Command.
        /// <summary>

        public RelayCommand<object> QtyChangedCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the QtyChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnQtyChangedCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the QtyChanged command is executed.
        /// </summary>
        private void OnQtyChangedCommandExecute(object param)
        {
            if (param != null && this.SelectedTransferStock != null)
            {
                base_TransferStockDetailModel model = param as base_TransferStockDetailModel;
                if (model.IsDirty)
                {
                    if (model.Quantity == 0)
                        model.Amount = 0;
                    else
                    {
                        this.SetAmountProduct(model);
                        //this.SetAvlQuantity(model);
                        this.SelectedTransferStock.IsChangeProductCollection = true;
                        this.CalSubTotalProduct();
                    }
                }
            }
        }

        #endregion

        #region DeleteSaleOrderDetailCommand
        /// <summary>
        /// Gets the DeleteSaleOrderDetail Command.
        /// <summary>

        public RelayCommand<object> DeleteSaleOrderDetailCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the DeleteSaleOrderDetail command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteSaleOrderDetailCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            if (this.SelectedTransferStock != null && this.SelectedTransferStock.Status > 1)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteSaleOrderDetail command is executed.
        /// </summary>
        private void OnDeleteSaleOrderDetailCommandExecute(object param)
        {
            if (param != null)
            {
                base_TransferStockDetailModel transferStockDetailModel = param as base_TransferStockDetailModel;
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.DeleteItems, MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.Yes))
                {
                    this.SelectedTransferStock.IsDirty = true;
                    this.SelectedTransferStock.IsChangeProductCollection = true;
                    this.SelectedTransferStock.TransferStockDetailCollection.Remove(transferStockDetailModel);
                    this.CalSubTotalProduct();
                    this.TotalProducts = this.SelectedTransferStock.TransferStockDetailCollection.Count();
                }
            }
        }
        #endregion

        #region TransferCommand
        /// <summary>
        /// Method to check whether the ApplyCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnTransferCommandCanExecute(object param)
        {
            if (this.IsValid && param != null
                && param is ObservableCollection<object>
                && (param as ObservableCollection<object>).Count == 1)
            {
                base_TransferStockModel model = (param as ObservableCollection<object>)[0] as base_TransferStockModel;
                if (model != null && model.TotalQuantity > 0 && !model.IsDirty && !model.IsNew && model.Status == 1)
                    return true;
            }
            return this.IsValid && (this.SelectedTransferStock != null && this.SelectedTransferStock.TotalQuantity > 0 && !this.SelectedTransferStock.IsDirty && !this.SelectedTransferStock.IsNew && this.SelectedTransferStock.Status == 1);
        }

        /// <summary>
        /// Method to invoke when the ApplyCommand command is executed.
        /// </summary>
        private void OnTransferCommandExecute(object param)
        {
            try
            {
                //To close product view
                if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text24, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //To apply that change product.
                if (param != null && param is ObservableCollection<object>)
                    this.SelectedTransferStock = (param as ObservableCollection<object>)[0] as base_TransferStockModel;
                string quantity = string.Empty;
                if (this.SelectedTransferStock.TotalQuantity <= 1)
                    quantity = string.Format(Application.Current.FindResource("TS_Message_Product") as string, this.SelectedTransferStock.TotalQuantity);
                else
                    quantity = string.Format(Application.Current.FindResource("TS_Message_Products") as string, this.SelectedTransferStock.TotalQuantity);
                string content = string.Format(Application.Current.FindResource("TransferMessage") as string, quantity, this.SelectedTransferStock.FromStoreValue.Name, this.SelectedTransferStock.ToStoreValue.Name);
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(content, Language.Information, MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    //To close product view
                    this.SelectedTransferStock.IsEnable = false;
                    foreach (var item in this.SelectedTransferStock.TransferStockDetailCollection)
                        item.IsEnable = false;
                    this.TransferStock();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("OnTransferCommandExecute" + ex);
            }
        }
        #endregion

        #region RevertCommand
        /// <summary>
        /// Method to check whether the RestoreCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRevertCommandCanExecute()
        {
            return this.IsValid && (this.SelectedTransferStock != null && this.SelectedTransferStock.TotalQuantity > 0 && !this.SelectedTransferStock.IsDirty && !this.SelectedTransferStock.IsNew && this.SelectedTransferStock.Status == 2);
        }
        /// <summary>
        /// Method to invoke when the RestoreCommand command is executed.
        /// </summary>
        private void OnRevertCommandExecute()
        {
            try
            {
                //To close product view
                if ((this._ownerViewModel as MainViewModel).IsOpenedView("Product"))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text25, Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //To apply that restore pricing.
                string quantity = string.Empty;
                if (this.SelectedTransferStock.TotalQuantity <= 1)
                    quantity = string.Format(Application.Current.FindResource("TS_Message_Product") as string, this.SelectedTransferStock.TotalQuantity);
                else
                    quantity = string.Format(Application.Current.FindResource("TS_Message_Products") as string, this.SelectedTransferStock.TotalQuantity);
                string content = string.Format(Application.Current.FindResource("RevertMessage") as string, quantity, this.SelectedTransferStock.ToStoreValue.Name, this.SelectedTransferStock.FromStoreValue.Name);
                MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(content, Language.Information, MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    //To close product view
                    this.SelectedTransferStock.IsEnable = false;
                    foreach (var item in this.SelectedTransferStock.TransferStockDetailCollection)
                        item.IsEnable = false;
                    this.RevertStock();
                }
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("OnRevertCommandExecute" + ex);
            }

        }
        #endregion

        #region LoadDataByStepCommand

        public RelayCommand<object> LoadStepCommand
        {
            get;
            private set;
        }
        /// <summary>
        /// Method to check whether the LoadStep command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStep command is executed.
        /// </summary>
        private void OnLoadStepCommandExecute(object param)
        {
            BackgroundWorker bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            Expression<Func<base_TransferStock, bool>> predicate = PredicateBuilder.True<base_TransferStock>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadTransferStock(predicate, true, this.CurrentPageIndex);
        }
        #endregion

        #region SearchProductCommand
        /// <summary>
        /// Gets the SearchProduct Command.
        /// <summary>

        public RelayCommand<object> SearchProductCommand { get; private set; }
        private bool OnSearchProductCommandCanExecute(object param)
        {
            return !string.IsNullOrWhiteSpace(BarcodeProduct); ;

        }

        private void OnSearchProductCommandExecute(object param)
        {
            try
            {
                Expression<Func<base_ProductStore, bool>> productCondition = PredicateBuilder.True<base_ProductStore>();
                short itemTypeGroup = (short)ItemTypes.Group;
                short itemTypeServices = (short)ItemTypes.Services;
                short itemInsurance = (short)ItemTypes.Insurance;
                int storeID = this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue);
                //To get product with store.
                productCondition = productCondition.And(x => x.StoreCode == storeID);
                //To get product with condition.
                productCondition = productCondition.And(x => x.base_Product.ItemTypeId != itemTypeGroup && x.base_Product.ItemTypeId != itemTypeServices && x.base_Product.ItemTypeId != itemInsurance);
                //To get Product by barcode
                productCondition = productCondition.And(x => x.base_Product.Barcode.Equals(this.BarcodeProduct));
                base_ProductStore productStores = _productStoreRepository.Get(productCondition);
                //To add item.
                if (productStores != null && productStores.base_Product != null)
                {
                    base_ProductModel productModel = new base_ProductModel(productStores.base_Product);
                    this.ProductCollection.Add(productModel);
                    //To count all User in Data base show on grid
                    this.TotalProducts = this.ProductCollection.Count;
                    this.SelectedProduct = this.ProductCollection.First();
                }
                this.BarcodeProduct = string.Empty;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region SelectedTransferStock
        /// <summary>
        /// To execute when a property of TransferStock changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedTransferStock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!this.SelectedTransferStock.IsLoad)
                switch (e.PropertyName)
                {
                    case "FromStoreValue":
                        this.SelectedTransferStock.IsChangeProductCollection = true;
                        this.SelectedTransferStock.TransferStockDetailCollection.Clear();
                        this.ChangeDataToStore(this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue));
                        break;

                    case "ShippingFee":
                        if (this.SelectedTransferStock.TransferStockDetailCollection != null)
                            this.CalTotalProduct();
                        break;
                }
        }
        #endregion

        #region Create Command
        /// <summary>
        /// To create command.
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            this.NewCommand = new RelayCommand<object>(OnNewCommandExecute, OnNewCommandCanExecute);
            this.EditCommand = new RelayCommand<object>(this.OnDoubleClickViewCommandExecute, this.OnEditCommandCanExecute);
            this.SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            this.DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            this.SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            this.DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            this.QtyChangedCommand = new RelayCommand<object>(OnQtyChangedCommandExecute, OnQtyChangedCommandCanExecute);
            this.DeleteSaleOrderDetailCommand = new RelayCommand<object>(OnDeleteSaleOrderDetailCommandExecute, OnDeleteSaleOrderDetailCommandCanExecute);
            this.SearchProductAdvanceCommand = new RelayCommand<object>(OnSearchProductAdvanceCommandExecute, OnSearchProductAdvanceCommandCanExecute);
            this.TransferCommand = new RelayCommand<object>(OnTransferCommandExecute, OnTransferCommandCanExecute);
            this.RevertCommand = new RelayCommand(OnRevertCommandExecute, OnRevertCommandCanExecute);
            this.LoadStepCommand = new RelayCommand<object>(this.OnLoadStepCommandExecute, this.OnLoadStepCommandCanExecute);
            this.SearchProductCommand = new RelayCommand<object>(OnSearchProductCommandExecute, OnSearchProductCommandCanExecute);
        }
        #endregion

        #region CreateNewTransferStock
        private void CreateNewTransferStock()
        {
            // TODO: Handle command logic here
            this.SelectedTransferStock = new base_TransferStockModel();
            this.SelectedTransferStock.Resource = Guid.NewGuid();
            this.SelectedTransferStock.IsLoad = true;
            this.SelectedTransferStock.UserCreated = Define.USER.LoginName;
            this.SelectedTransferStock.FromStore = Int16.Parse(Define.StoreCode.ToString());
            this.SelectedTransferStock.FromStoreValue = FromStoreCollection[Define.StoreCode];
            this.SelectedTransferStock.ToStore = -1;
            this.SelectedTransferStock.ShippingFee = 0;
            this.SelectedTransferStock.Status = 1;
            this.SelectedTransferStock.ShipDate = DateTimeExt.Today;
            this.SelectedTransferStock.DateCreated = DateTimeExt.Today;
            this.SelectedTransferStock.IsLoad = false;
            this.SelectedTransferStock.IsDirty = false;
            //To set enable of detail grid.
            this.IsSearchMode = false;
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            this._isTransferFromProduct = true;
            this._productCloneCollection = null;
            if (this.SelectedTransferStock != null && this.SelectedTransferStock.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else //Has Error
                        result = false;
                }
                else
                {
                    if (this.SelectedTransferStock.IsNew)
                    {
                        //this.DeleteNote();
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                        this.SelectedTransferStock = null;
                    }
                    else //Old Item Rollback data
                    {
                        //this.DeleteNote();
                        this.RollBackTransfer();
                    }
                }
            }
            return result;
        }

        private void LoadStaticData()
        {
            //To create collection for search products
            ProductFieldCollection = new DataSearchCollection();
            ProductFieldCollection.Add(new DataSearchModel
            {
                ID = 1,
                Level = 0,
                DisplayName = "Code",
                KeyName = "Code"
            });
            ProductFieldCollection.Add(new DataSearchModel
            {
                ID = 2,
                Level = 0,
                DisplayName = "Barcode",
                KeyName = "Barcode"
            });
            ProductFieldCollection.Add(new DataSearchModel
            {
                ID = 3,
                Level = 0,
                DisplayName = "Product Name",
                KeyName = "ProductName"
            });
            ProductFieldCollection.Add(new DataSearchModel
            {
                ID = 4,
                Level = 0,
                DisplayName = "Attribute",
                KeyName = "Attribute"
            });
            ProductFieldCollection.Add(new DataSearchModel
            {
                ID = 6,
                Level = 0,
                DisplayName = "Size",
                KeyName = "Size"
            });
            //Get Store
            this.StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            //Get Store
            this.FromStoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            this._fromStoreCollectionView = CollectionViewSource.GetDefaultView(this.FromStoreCollection);

            //Get Store
            this.ToStoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            this._toStoreCollectionView = CollectionViewSource.GetDefaultView(this.ToStoreCollection);

        }
        #endregion

        #region SearchProductAdvance
        /// <summary>
        /// To search product with advance options..
        /// </summary>
        private void SearchProductAdvance()
        {
            int storeIndex = this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue);
            ProductSearchViewModel productSearchViewModel = new ProductSearchViewModel(false, false, false, false, false, false, storeIndex);
            bool? dialogResult = _dialogService.ShowDialog<ProductSearchView>(_ownerViewModel, productSearchViewModel, "Search Product");
            if (dialogResult == true)
            {
                foreach (var item in productSearchViewModel.SelectedProducts)
                    this.SelectedProduct = item;
            }
        }
        #endregion

        #region Insert Update TransferStock
        /// <summary>
        /// To insert data into base_TransferStock table.
        /// </summary>
        private void Insert()
        {
            this.SelectedTransferStock.DateCreated = DateTime.Now;
            this.SelectedTransferStock.UserCreated = Define.USER.LoginName;
            this.SelectedTransferStock.TransferNo = DateTime.Now.ToString(Define.GuestNoFormat);
            this.SelectedTransferStock.ToEntity();
            this._transferStockRepository.Add(this.SelectedTransferStock.base_TransferStock);
            //To insert data into base_TransferStockDetail table.
            if (this.SelectedTransferStock.TransferStockDetailCollection != null
                    && this.SelectedTransferStock.TransferStockDetailCollection.Count > 0)
                foreach (base_TransferStockDetailModel item in this.SelectedTransferStock.TransferStockDetailCollection)
                {
                    item.ToEntity();
                    this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail.Add(item.base_TransferStockDetail);
                    item.EndUpdate();
                }
            this._transferStockRepository.Commit();
            this.SelectedTransferStock.Id = this.SelectedTransferStock.base_TransferStock.Id;
            this.SelectedTransferStock.EndUpdate();
            //To add new item into TransferStockCollection.
            this.TransferStockCollection.Insert(0, this.SelectedTransferStock);
            this.TotalTransferStock = this.TransferStockCollection.Count();
            App.WriteUserLog("TransferStock", "User insert a new  TransferStock." + this.SelectedTransferStock.Id);
        }
        /// <summary>
        /// To update item in base_TransferStock table. 
        /// </summary>
        private void Update()
        {
            /// To update item in base_TransferStock table. 
            this.SelectedTransferStock.ToEntity();
            /// To update item in base_TransferStockDetail table. 
            if (this.SelectedTransferStock.IsChangeProductCollection)
            {
                _transferStockDetailRepository.Delete(this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail);
                this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail.Clear();
                if (this.SelectedTransferStock.TransferStockDetailCollection != null
                    && this.SelectedTransferStock.TransferStockDetailCollection.Count > 0)
                    foreach (base_TransferStockDetailModel item in this.SelectedTransferStock.TransferStockDetailCollection)
                    {
                        item.CreateNewTransferDetail();
                        item.ToEntity();
                        this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail.Add(item.base_TransferStockDetail);
                        item.EndUpdate();
                    }
            }
            this.SelectedTransferStock.EndUpdate();
            this._transferStockRepository.Commit();
            App.WriteUserLog("TransferStock", "User update a TransferStock." + this.SelectedTransferStock.Id);
        }

        /// <summary>
        /// To transfer product from this store to another store.
        /// </summary>
        private void TransferStock()
        {
            try
            {
                this.SelectedTransferStock.Shift = Define.ShiftCode;
                this.SelectedTransferStock.UserApplied = Define.USER.LoginName;
                this.SelectedTransferStock.DateApplied = DateTimeExt.Today;
                this.SelectedTransferStock.Status = 2;
                this.SelectedTransferStock.ToEntity();
                this.SelectedTransferStock.VisibilityReversed = Visibility.Visible;
                this.SelectedTransferStock.VisibilityApplied = Visibility.Collapsed;
                //To revert data of product.
                foreach (var item in this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail)
                {
                    base_TransferStockDetailModel model = new base_TransferStockDetailModel(item);
                    Guid resource = Guid.Parse(model.ProductResource);
                    model.ProductModel = new base_ProductModel(_productRepository.GetIQueryable(x => x.Resource == resource).SingleOrDefault());
                    if (model.ProductModel != null)
                    {
                        this.GetProductUOMforTransferStockDetail(model);
                        if (model.ProductUOMCollection.SingleOrDefault(x => x.UOMId == model.UOMId) != null)
                        {
                            decimal baseUnitNumber = model.ProductUOMCollection.SingleOrDefault(x => x.UOMId == model.UOMId).BaseUnitNumber;
                            this._productRepository.TransferStock(item.ProductResource, this.SelectedTransferStock.FromStore, this.SelectedTransferStock.ToStore, model.Quantity, false, baseUnitNumber);
                        }
                    }
                }
                this._transferStockRepository.Commit();
                this.SelectedTransferStock.EndUpdate();
                this.SelectedTransferStock.IsChangeProductCollection = false;
                App.WriteUserLog("TransferStock", "User transfered numbers of products in stock." + this.SelectedTransferStock.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("TransferStock" + ex.ToString());
            }
        }

        /// <summary>
        /// To Revert product from this store to another store.
        /// </summary>
        private void RevertStock()
        {
            try
            {
                this.SelectedTransferStock.Shift = Define.ShiftCode;
                this.SelectedTransferStock.UserReversed = Define.USER.LoginName;
                //To update data of item in base_TransferStock.
                this.SelectedTransferStock.DateReversed = DateTimeExt.Today;
                this.SelectedTransferStock.Status = 3;
                this.SelectedTransferStock.ToEntity();
                //To revert data of product.
                foreach (var item in this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail)
                {
                    base_TransferStockDetailModel model = new base_TransferStockDetailModel(item);
                    Guid resource = Guid.Parse(model.ProductResource);
                    model.ProductModel = new base_ProductModel(_productRepository.GetIQueryable(x => x.Resource == resource).SingleOrDefault());
                    if (model.ProductModel != null)
                    {
                        this.GetProductUOMforTransferStockDetail(model);
                        if (model.ProductUOMCollection.SingleOrDefault(x => x.UOMId == model.UOMId) != null)
                        {
                            decimal baseUnitNumber = model.ProductUOMCollection.SingleOrDefault(x => x.UOMId == model.UOMId).BaseUnitNumber;
                            this._productRepository.TransferStock(item.ProductResource, this.SelectedTransferStock.FromStore, this.SelectedTransferStock.ToStore, model.Quantity, true, baseUnitNumber);
                        }
                    }
                }
                this._transferStockRepository.Commit();
                this.SelectedTransferStock.EndUpdate();
                this.SelectedTransferStock.IsChangeProductCollection = false;
                App.WriteUserLog("TransferStock", "User reverted numbers of products in stock." + this.SelectedTransferStock.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("RevertStock" + ex.ToString());
            }
        }
        #endregion

        #region Change data of store
        private void ChangeDataFromStore(int id)
        {
            this._fromStoreCollectionView.Filter = x =>
            {
                if (this.FromStoreCollection.IndexOf((x as base_Store)) == id)
                    return false;
                return true;
            };
        }
        private void ChangeDataToStore(int id)
        {
            try
            {
                this._toStoreCollectionView.Filter = x =>
                {
                    if (this.ToStoreCollection.IndexOf((x as base_Store)) == id)
                        return false;
                    return true;
                };
                // Initial predicate
                //Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
                //// Set conditions for predicate
                //int storeID = this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue);
                //predicate = predicate.And(x => x.StoreCode == storeID);
                //this.LoadProductWithStore(predicate, true);
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("ChangeDataToStore" + ex);
            }

        }
        #endregion

        #region Change Product to TransferDetail
        private base_TransferStockDetailModel ChangeProduct(base_ProductModel productModel, bool IsNew)
        {
            base_TransferStockDetailModel _transferStockDetailModel = new base_TransferStockDetailModel();
            _transferStockDetailModel.ProductResource = productModel.Resource.ToString();
            _transferStockDetailModel.ItemCode = productModel.Code;
            _transferStockDetailModel.ItemName = productModel.ProductName;
            _transferStockDetailModel.ItemAtribute = productModel.Attribute;
            _transferStockDetailModel.ItemSize = productModel.Size;
            _transferStockDetailModel.UOMId = productModel.BaseUOMId;
            _transferStockDetailModel.SerialTracking = productModel.Serial;
            _transferStockDetailModel.ProductModel = productModel;
            return _transferStockDetailModel;
        }
        #endregion

        #region LoadTransferStock
        private void LoadTransferStock(Expression<Func<base_TransferStock, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            try
            {
                BackgroundWorker bgWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                if (currentIndex == 0)
                    this.TransferStockCollection.Clear();
                bgWorker.DoWork += (sender, e) =>
                {
                    base.IsBusy = true;
                    //To get data with range
                    int indexItem = 0;
                    if (currentIndex > 1)
                        indexItem = (currentIndex - 1) * base.NumberOfDisplayItems;
                    IList<base_TransferStock> transferStocks = _transferStockRepository.GetRangeDescending(indexItem, base.NumberOfDisplayItems, x => x.DateCreated, predicate);
                    foreach (var item in transferStocks)
                        bgWorker.ReportProgress(0, item);
                };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                    //To add item of TransferStock.
                    base_TransferStockModel model = new base_TransferStockModel(e.UserState as base_TransferStock);
                    //To set visibility of Button.
                    if (model.Status == 2)
                    {
                        model.VisibilityApplied = Visibility.Collapsed;
                        model.VisibilityReversed = Visibility.Visible;
                    }
                    if (model.Status > 1)
                        model.IsEnable = false;
                    model.FromStoreValue = this.FromStoreCollection.ElementAt(model.FromStore);
                    model.ToStoreValue = this.FromStoreCollection.ElementAt(model.ToStore);
                    model.EndUpdate();
                    this.TransferStockCollection.Add(model);
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    base.IsBusy = false;
                    if (this._isTransferFromProduct && this._productCloneCollection != null)
                    {
                        this.TransferStockFromProduct(_productCloneCollection as IEnumerable<base_ProductModel>);
                        this._isTransferFromProduct = false;
                        this._productCloneCollection = null;
                    }
                    //To count all User in Data base show on grid
                    this.TotalTransferStock = _transferStockRepository.GetIQueryable().Count();
                };
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region SearchTransferStock
        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_TransferStock, bool>> CreateSearchPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_TransferStock, bool>> predicate = PredicateBuilder.True<base_TransferStock>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Set conditions for predicate
                predicate = PredicateBuilder.False<base_TransferStock>();
                //To search with Transfer No.
                if (this.ColumnCollection.Contains(SearchOptions.AccountNum.ToString()))
                    predicate = predicate.Or(x => x.TransferNo.Contains(keyword));
                //To search with Status. 
                if (this.ColumnCollection.Contains(SearchOptions.Status.ToString()))
                {
                    IEnumerable<short> transferStockStatus = Common.TransferStockStatus.Where(x => x.Text.ToLower().Contains(keyword.ToLower())).Select(x => x.Value);
                    predicate = predicate.Or(x => transferStockStatus.Contains(x.Status));
                }
                //To search with From Store.
                if (this.ColumnCollection.Contains(SearchOptions.FromStore.ToString()))
                {
                    IEnumerable<base_Store> FromStore = this.StoreCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    //Collection Store index , cause StoreCode sale order storage by index
                    IList<int> FromStoreIndex = new List<int>();
                    foreach (base_Store item in FromStore)
                    {
                        int storeIndex = this.StoreCollection.IndexOf(item);
                        if (!FromStoreIndex.Any(x => x.Equals(storeIndex)))
                            FromStoreIndex.Add(storeIndex);
                    }
                    predicate = predicate.Or(x => FromStoreIndex.Contains(x.FromStore));
                }
                //To search with To Store.
                if (this.ColumnCollection.Contains(SearchOptions.ToStore.ToString()))
                {
                    IEnumerable<base_Store> ToStore = this.StoreCollection.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));
                    //Collection Store index , cause StoreCode sale order storage by index
                    IList<int> TostoreIndex = new List<int>();
                    foreach (base_Store item in ToStore)
                    {
                        int storeIndex = this.StoreCollection.IndexOf(item);
                        if (!TostoreIndex.Any(x => x.Equals(storeIndex)))
                            TostoreIndex.Add(storeIndex);
                    }
                    predicate = predicate.Or(x => TostoreIndex.Contains(x.ToStore));
                }
                //To search with Date.
                DateTime date;
                if (DateTime.TryParseExact(keyword, Define.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    int year = date.Year;
                    int month = date.Month;
                    int day = date.Day;
                    //To search with Date Created.
                    if (this.ColumnCollection.Contains(SearchOptions.DateCreated.ToString()))
                        predicate = predicate.Or(x => x.DateCreated.Year == year && x.DateCreated.Month == month && x.DateCreated.Day == day);
                    //To search with Date Applied.
                    if (this.ColumnCollection.Contains(SearchOptions.DateApplied.ToString()))
                        predicate = predicate.Or(x => x.DateApplied.HasValue && x.DateApplied.Value.Year == year && x.DateApplied.Value.Month == month && x.DateApplied.Value.Day == day);
                    //To search with Date Reversed.
                    if (this.ColumnCollection.Contains(SearchOptions.DateReversed.ToString()))
                        predicate = predicate.Or(x => x.DateReversed.HasValue && x.DateReversed.Value.Year == year && x.DateReversed.Value.Month == month && x.DateReversed.Value.Day == day);
                }
                //To search with User Created.
                if (this.ColumnCollection.Contains(SearchOptions.UserCreated.ToString()))
                    predicate = predicate.Or(x => x.UserCreated.ToLower().Equals(keyword.ToLower()));
                //To search with User User Reversed.
                if (this.ColumnCollection.Contains(SearchOptions.UserReversed.ToString()))
                    predicate = predicate.Or(x => x.UserReversed.ToLower().Equals(keyword.ToLower()));
                //To search with User User Applied.
                if (this.ColumnCollection.Contains(SearchOptions.UserApplied.ToString()))
                    predicate = predicate.Or(x => x.UserApplied.ToLower().Equals(keyword.ToLower()));
            }
            return predicate;
        }
        #endregion

        #region SelectedProductChanged
        /// <summary>
        /// To execute when a property of SelectedProduct changed.
        /// </summary>
        private void SelectedProductChanged()
        {
            try
            {
                if (this.SelectedProduct != null)
                {
                    this.SelectedTransferStock.IsLoad = true;
                    this.SelectedTransferStock.IsDirty = true;
                    this.SelectedTransferStock.IsChangeProductCollection = true;
                    string productResource = this.SelectedProduct.Resource.ToString();
                    base_TransferStockDetailModel transferStockDetailModel = null;
                    if (!this.SelectedTransferStock.TransferStockDetailCollection.Select(x => x.ProductResource).Contains(productResource))
                    {
                        //To create new TransferStockDetail
                        transferStockDetailModel = this.ChangeProduct(this.SelectedProduct, this.SelectedTransferStock.IsNew);
                        transferStockDetailModel.TransferStockResource = Guid.NewGuid().ToString();
                        transferStockDetailModel.Quantity = 1;
                        //To get Product UOMCollection
                        if (transferStockDetailModel.ProductModel != null)
                            this.GetProductUOMforTransferStockDetail(transferStockDetailModel);
                        //To set amount of product.
                        this.SetAmountProduct(transferStockDetailModel);
                        //To set available quantity of product.
                        this.SetAvlQuantity(transferStockDetailModel);
                        transferStockDetailModel.PropertyChanged += new PropertyChangedEventHandler(TransferStockDetailModel_PropertyChanged);
                        this.SelectedTransferStock.TransferStockDetailCollection.Add(transferStockDetailModel);
                    }
                    else
                    {
                        transferStockDetailModel = this.SelectedTransferStock.TransferStockDetailCollection.SingleOrDefault(x => (x.ParentResource == null || x.ParentResource.Length == 0) && x.ProductResource.Equals(productResource));
                        transferStockDetailModel.Quantity = transferStockDetailModel.Quantity + 1;
                        //To set amount of product.
                        this.SetAmountProduct(transferStockDetailModel);
                        //To set available quantity of product.
                        this.SetAvlQuantity(transferStockDetailModel);
                    }
                    this.CalSubTotalProduct();
                    this.TotalProducts = this.SelectedTransferStock.TransferStockDetailCollection.Count();
                    this.SelectedTransferStock.IsLoad = false;
                }
                this.SelectedProduct = null;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Debug.WriteLine("SelectedProductChanged" + ex.ToString());
            }
        }

        private void TransferStockDetailModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.SelectedTransferStock.IsLoad)
                return;
            base_TransferStockDetailModel model = sender as base_TransferStockDetailModel;
            switch (e.PropertyName)
            {
                case "UOMId":
                    this.SetPriceUOM(model);
                    this.SetAmountProduct(model);
                    this.SetAvlQuantity(model);
                    this.CalSubTotalProduct();
                    this.SelectedTransferStock.IsChangeProductCollection = true;
                    break;
            }
        }
        #endregion

        #region GetProductUOMforTransferStockDetail
        /// <summary>
        /// Get UOM Collection For sale order detail
        /// </summary>
        /// <param name="salesOrderDetailModel"></param>
        private void GetProductUOMforTransferStockDetail(base_TransferStockDetailModel transferStockDetailModel, bool SetPrice = true)
        {
            transferStockDetailModel.ProductUOMCollection = new ObservableCollection<base_ProductUOMModel>();
            // Add base unit in UOMCollection.
            base_UOM UOM = _UOMRepository.Get(x => x.Id == transferStockDetailModel.ProductModel.BaseUOMId);
            if (UOM != null)
            {
                if (transferStockDetailModel.IsNew)
                    transferStockDetailModel.BaseUOM = UOM.Name;
                transferStockDetailModel.Price = transferStockDetailModel.ProductModel.AverageUnitCost;
                transferStockDetailModel.ProductUOMCollection.Add(new base_ProductUOMModel
            {
                UOMId = UOM.Id,
                Name = UOM.Name,
                BaseUnitNumber = 1,
                RegularPrice = transferStockDetailModel.Price,
                Price1 = transferStockDetailModel.ProductModel.Price1,
                Price2 = transferStockDetailModel.ProductModel.Price2,
                Price3 = transferStockDetailModel.ProductModel.Price3,
                Price4 = transferStockDetailModel.ProductModel.Price4,
                IsNew = false,
                IsDirty = false
            });
            }
            // Get product store by default store code
            base_ProductStore productStore = transferStockDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));
            if (productStore != null)
            {
                // Gets the remaining units.
                foreach (base_ProductUOM item in productStore.base_ProductUOM)
                {
                    transferStockDetailModel.ProductUOMCollection.Add(new base_ProductUOMModel(item)
                    {
                        RegularPrice = transferStockDetailModel.ProductModel.AverageUnitCost,
                        BaseUnitNumber = item.BaseUnitNumber,
                        Name = item.base_UOM.Name,
                        IsDirty = false
                    });
                }
            }
        }
        #endregion

        #region SetPriceUOM
        /// <summary>
        /// To set price to TransferStockDetailModel with UOMId.
        /// </summary>
        /// <param name="saleOrderDetailModel"></param>
        private void SetPriceUOM(base_TransferStockDetailModel transferStockDetailModel)
        {
            if (transferStockDetailModel.ProductUOMCollection != null)
            {
                base_ProductUOMModel productUOM = transferStockDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == transferStockDetailModel.UOMId);
                if (productUOM != null)
                {
                    transferStockDetailModel.BaseUOM = productUOM.Name;
                    transferStockDetailModel.Price = productUOM.RegularPrice;
                }
            }
        }

        /// <summary>
        /// To set Amount of products.
        /// </summary>
        /// <param name="transferStockDetailModel"></param>
        private void SetAmountProduct(base_TransferStockDetailModel transferStockDetailModel)
        {
            //To set amount of product
            decimal unitNumber = transferStockDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == transferStockDetailModel.UOMId).BaseUnitNumber;
            transferStockDetailModel.Amount = transferStockDetailModel.Quantity * unitNumber * transferStockDetailModel.Price;
        }

        /// <summary>
        /// To set on hand quantity of products.
        ///(QuantityOnHand - QuantityOnCustomer) / BaseUnitNumber
        /// </summary>
        /// <param name="transferStockDetailModel"></param>
        private void SetAvlQuantity(base_TransferStockDetailModel transferStockDetailModel)
        {
            base_ProductUOMModel productUOMModel = transferStockDetailModel.ProductUOMCollection.SingleOrDefault(x => x.UOMId == transferStockDetailModel.UOMId);
            // Get product store by default store code
            base_ProductStore productStore = transferStockDetailModel.ProductModel.base_Product.base_ProductStore.SingleOrDefault(x => x.StoreCode.Equals(Define.StoreCode));
            if (transferStockDetailModel.UOMId.Equals(transferStockDetailModel.ProductModel.BaseUOMId))
                transferStockDetailModel.AvlQuantity = Convert.ToDecimal(productStore.QuantityOnHand - productStore.QuantityOnCustomer);
            else
                transferStockDetailModel.AvlQuantity = Convert.ToDecimal(productStore.QuantityOnHand - productStore.QuantityOnCustomer) / Convert.ToDecimal(productUOMModel.BaseUnitNumber);
        }

        #endregion

        #region CalTotalProduct
        /// <summary>
        /// To calulate all of product.
        /// </summary>
        private void CalSubTotalProduct()
        {
            if (this.SelectedTransferStock != null)
            {
                if (this.SelectedTransferStock.TransferStockDetailCollection == null
                    || this.SelectedTransferStock.TransferStockDetailCollection.Count == 0)
                {
                    this.SelectedTransferStock.SubTotal = 0;
                }
                else
                    this.SelectedTransferStock.SubTotal = this.SelectedTransferStock.TransferStockDetailCollection.Sum(x => x.Amount);
                this.CalTotalProduct();
            }
        }

        /// <summary>
        /// To calulate all of product with freight.
        /// </summary>
        private void CalTotalProduct()
        {
            if (this.SelectedTransferStock != null)
            {
                this.SelectedTransferStock.TotalQuantity = this.SelectedTransferStock.TransferStockDetailCollection.Sum(x => x.Quantity);
                this.SelectedTransferStock.Total = this.SelectedTransferStock.SubTotal + this.SelectedTransferStock.ShippingFee;
            }
        }
        #endregion

        #region IsEditData
        /// <summary>
        /// To check that user edit data.
        /// </summary>
        /// <returns></returns>
        private bool IsEditData()
        {
            if (this.SelectedTransferStock == null)
                return false;

            return ((this.SelectedTransferStock.IsDirty || this.SelectedTransferStock.IsChangeProductCollection));
        }
        #endregion

        #region ShowNotification
        /// <summary>
        /// To notify when user opens another view or close a view.
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;

            //To check data is edited
            if (this.IsEditData())
            {
                //To show notification when data has changed
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text13, Language.Save, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (msgResult.Is(MessageBoxResult.Cancel))
                    return false;
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (this.OnSaveCommandCanExecute())
                    {
                        // Call Save function
                        this.OnSaveCommandExecute();
                        result = true;
                    }
                    else
                        result = false;
                    //// Remove popup note
                    //this.CloseAllPopupNote();
                }
                else
                {
                    if (this.SelectedTransferStock.IsNew && isClosing.HasValue && !isClosing.Value)
                        this.IsSearchMode = true;
                    if (this.SelectedTransferStock.IsNew)
                    {
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                        this.SelectedTransferStock = null;
                    }
                    else //Old Item Rollback data
                    {
                        //this.DeleteNote();
                        this.RollBackTransfer();
                    }
                }
            }
            else
            {
                if (this.SelectedTransferStock.IsNew)
                    this.SelectedTransferStock = null;
            }

            return result;
        }
        #endregion

        #region RollBackTransfer
        /// <summary>
        /// To rollback data when user don't save it.
        /// </summary>
        private void RollBackTransfer()
        {
            this.SelectedTransferStock.IsLoad = true;
            this.SelectedTransferStock.ToModelAndRaise();
            this.SelectedTransferStock.IsLoad = false;
            this.SelectedTransferStock.EndUpdate();
            this.SelectedTransferStock.IsChangeProductCollection = false;
        }
        #endregion

        #region TransferStockFromProduct
        private void TransferStockFromProduct(IEnumerable<base_ProductModel> productCollection)
        {
            //To show detail form.
            this.SelectedTransferStock = null;
            this.IsSearchMode = false;
            this.CreateNewTransferStock();
            foreach (var item in productCollection)
                this.SelectedProduct = item;
            this.ChangeDataToStore(this.FromStoreCollection.IndexOf(this.SelectedTransferStock.FromStoreValue));
            this.SelectedTransferStock.IsDirty = true;
        }
        #endregion

        #region SetTransferStockDetail
        private void SetTransferStockDetail()
        {
            List<string> TransferStockResource = new List<string>();
            if (this.SelectedTransferStock != null)
            {
                BackgroundWorker bgWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                this.SelectedTransferStock.TransferStockDetailCollection.Clear();
                bgWorker.DoWork += (sender, e) =>
                {
                    this.SelectedTransferStock.IsLoad = true;

                    if (this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail.Count > 0)
                    {
                        foreach (base_TransferStockDetail item in this.SelectedTransferStock.base_TransferStock.base_TransferStockDetail.OrderBy(x => x.Id))
                            bgWorker.ReportProgress(0, item);
                    }
                };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                    base_TransferStockDetailModel detailModel = new base_TransferStockDetailModel(e.UserState as base_TransferStockDetail, true);
                    Guid resource = Guid.Parse(detailModel.ProductResource);
                    lock (UnitOfWork.Locker)
                    {
                        var itemProduct = _productRepository.GetIQueryable(x => x.Resource == resource).SingleOrDefault();
                        if (itemProduct != null)
                        {
                            detailModel.ProductModel = new base_ProductModel(itemProduct);
                            if (detailModel.ProductModel != null)
                                this.GetProductUOMforTransferStockDetail(detailModel);
                            detailModel.PropertyChanged += new PropertyChangedEventHandler(TransferStockDetailModel_PropertyChanged);
                            this.SelectedTransferStock.IsChangeProductCollection = false;
                            this.SelectedTransferStock.TransferStockDetailCollection.Add(detailModel);
                        }
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    if (this.SelectedTransferStock.Status <= 1)
                        this.ChangeDataFromStore(this.SelectedTransferStock.ToStore);
                    this.ChangeDataToStore(this.SelectedTransferStock.FromStore);
                    if (this.SelectedTransferStock.TransferStockDetailCollection != null)
                        this.TotalProducts = this.SelectedTransferStock.TransferStockDetailCollection.Count();
                    this.SelectedTransferStock.EndUpdate();
                    this.SelectedTransferStock.IsLoad = false;
                };
                bgWorker.RunWorkerAsync();
            }
        }
        #endregion

        #region Auto Searching
        /// <summary>
        /// Event Tick for search ching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void _waitingTimer_Tick(object sender, EventArgs e)
        {
            _timerCounter++;
            if (_timerCounter == Define.DelaySearching)
            {
                OnSearchCommandExecute(null);
                _waitingTimer.Stop();
            }
        }

        /// <summary>
        /// Reset timer for Auto complete search
        /// </summary>
        protected virtual void ResetTimer()
        {
            if (Define.CONFIGURATION.IsAutoSearch && this._waitingTimer != null)
            {
                this._waitingTimer.Stop();
                this._waitingTimer.Start();
                _timerCounter = 0;
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region OnViewChangingCommandCanExecute
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ChangeViewExecute(isClosing);
        }
        #endregion

        #region ChangeSearchMode

        /// <summary>
        /// ChangeSearchMode
        /// </summary>
        /// <param name="isList"></param>
        /// <param name="param"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (this.ChangeViewExecute(null))
                {
                    if (!isList)
                    {
                        this.SelectedIndex = -1;
                        this.OnNewCommandExecute(null);
                        this.IsSearchMode = false;
                    }
                    else
                        this.IsSearchMode = true;
                }
            }
            else
            {
                this._isTransferFromProduct = true;
                this._productCloneCollection = param;
            }
        }

        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            this.TransferStockCollection.Clear();
            Expression<Func<base_TransferStock, bool>> predicate = PredicateBuilder.True<base_TransferStock>();
            if (!string.IsNullOrWhiteSpace(this.Keyword))
                predicate = this.CreateSearchPredicate(this.Keyword);
            this.LoadTransferStock(predicate, true);
        }
        #endregion

        #endregion

        #region IDropTarget Members

        public void DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(DropInfo dropInfo)
        {
            if (dropInfo.Data is base_ProductModel || dropInfo.Data is IEnumerable<base_ProductModel>)
            {
                (_ownerViewModel as MainViewModel).OpenViewExecute("Transfer Stock", dropInfo.Data);
            }
        }

        #endregion
    }
}