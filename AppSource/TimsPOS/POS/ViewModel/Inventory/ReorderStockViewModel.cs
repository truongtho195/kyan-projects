using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Repository;
using System.Collections.ObjectModel;
using CPC.POS.Database;
using System.Linq.Expressions;
using CPC.POS.Model;
using System.ComponentModel;
using System.Diagnostics;

namespace CPC.POS.ViewModel
{
    class ReorderStockViewModel : ViewModelBase
    {
        #region Define
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_PurchaseOrderDetailRepository _purchaseOrderDetailRepository = new base_PurchaseOrderDetailRepository();
        private base_SaleOrderDetailRepository _saleOrderDetailRepository = new base_SaleOrderDetailRepository();
        private base_ProductStoreRepository _productStoreRepository = new base_ProductStoreRepository();
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand<object> SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        protected string VendorType = MarkType.Vendor.ToDescription();
        #endregion

        #region Constructors
        public ReorderStockViewModel()
        {
            base._ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();
            this.LoadStore();
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
            get { return isSearchMode; }
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

        #region SelectedStore
        /// <summary>
        /// Gets or sets the SelectedStore.
        /// </summary>
        private base_Store _selectedStore;
        public base_Store SelectedStore
        {
            get
            {
                return _selectedStore;
            }
            set
            {
                _selectedStore = value;
                this.OnPropertyChanged(() => SelectedStore);
                if (this.SelectedStore != null)
                {
                    if (this.SelectedStore.Id == -1)
                        this.LoadAllProduct();
                    else
                    {
                        Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
                        predicate = this.CreateProductPredicate(this.StoreCollection.IndexOf(this.SelectedStore));
                        this.LoadProductWithStore(predicate, true);
                    }
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

        #endregion

        #region Commands Methods

        #region NewCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnNewCommandExecute()
        {
            // TODO: Handle command logic here
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute(object param)
        {
            if (param != null && (param as ObservableCollection<object>).Count > 0)
                return true;
            return false;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute(object param)
        {
            try
            {
                // TODO: Handle command logic here
                if (param is ObservableCollection<object>)
                {
                    ObservableCollection<base_ProductModel> ProductSelectedCollection = new ObservableCollection<base_ProductModel>();
                    foreach (var item in (param as ObservableCollection<object>))
                    {
                        ProductSelectedCollection.Add(item as base_ProductModel);
                    }
                    if (ProductSelectedCollection.Count > 0)
                        (_ownerViewModel as MainViewModel).OpenViewExecute("PurchaseOrder", ProductSelectedCollection);
                    App.WriteLUserLog("ReOrderStock", "Re-Order stock.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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
        private bool OnSearchCommandCanExecute()
        {
            return true;
        }

        private void OnSearchCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion

        #endregion

        #region Private Methods

        #region InitialCommand
        private void InitialCommand()
        {
            // Route the commands
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand(OnSearchCommandExecute, OnSearchCommandCanExecute);
        }
        #endregion

        #region CreateProductPredicate
        private Expression<Func<base_ProductStore, bool>> CreateProductPredicate(int storeId)
        {
            // Initial predicate
            Expression<Func<base_ProductStore, bool>> predicate = PredicateBuilder.True<base_ProductStore>();
            // Set conditions for predicate
            predicate = predicate.And(x => x.StoreCode == storeId);
            return predicate;
        }
        #endregion

        #region ChangeViewExecute
        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            return true;
        }
        #endregion

        #region LoadStore
        private void LoadStore()
        {
            //To get data from store table.
            this.StoreCollection = new ObservableCollection<base_Store>(_storeRepository.GetAll().OrderBy(x => x.Id));
            this.StoreCollection.Insert(0, new base_Store { Id = -1, Name = "All Store" });
        }
        #endregion

        private void LoadProductWithStore(Expression<Func<base_ProductStore, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            try
            {
                //To add item.
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                this.ProductCollection = new ObservableCollection<base_ProductModel>();
                bgWorker.DoWork += (sender, e) =>
                {
                    base.IsBusy = true;
                    IEnumerable<base_ProductStore> productStores = _productStoreRepository.GetAll(predicate).OrderBy(x => x.StoreCode);
                    if (productStores.Count() == 0)
                        base.IsBusy = false;
                    foreach (var productStore in productStores)
                        bgWorker.ReportProgress(0, productStore);
                };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                    //To add item.
                    base_ProductStore productStore = e.UserState as base_ProductStore;
                    if (productStore.ReorderPoint > 0)
                    {
                        base_ProductModel productModel = new base_ProductModel(productStore.base_Product);
                        productModel.VendorName = _guestRepository.GetAll(x => x.Mark.Equals(VendorType) && x.IsActived && !x.IsPurged && x.Id.Equals(productModel.VendorId)).SingleOrDefault().Company;
                        //To get purchase order detail
                        string productResource = productModel.Resource.ToString();
                        //To get product with all of stores
                        productModel.OnHandStore = productStore.QuantityOnHand;
                        productModel.QuantityOnSO = productStore.QuantityOnCustomer;
                        productModel.QuantityOnHand = productStore.QuantityOnHand - productStore.QuantityOnCustomer;
                        productModel.QuantityOnPO = productStore.QuantityOnOrder;
                        productModel.CompanyReOrderPoint = productStore.ReorderPoint;
                        productModel.EndUpdate();
                        if (productModel.QuantityOnHand + productModel.QuantityOnPO < productModel.CompanyReOrderPoint)
                        {
                            productModel.ReOrderQuatity = productModel.CompanyReOrderPoint - (productModel.QuantityOnHand + productModel.QuantityOnPO);
                            this.ProductCollection.Add(productModel);
                        }
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    //To count all User in Data base show on grid
                    this.TotalProducts = this.ProductCollection.Count;
                    base.IsBusy = false;
                };
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private void LoadAllProduct()
        {
            try
            {
                //To add item.
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                this.ProductCollection = new ObservableCollection<base_ProductModel>();
                bgWorker.DoWork += (sender, e) =>
                {
                    base.IsBusy = true;
                    IEnumerable<base_Product> products = _productRepository.GetAll();
                    if (products.Count() == 0)
                        base.IsBusy = false;
                    foreach (var productItem in products)
                        bgWorker.ReportProgress(0, productItem);
                };
                bgWorker.ProgressChanged += (sender, e) =>
                {
                    //To add item.
                    base_Product product = e.UserState as base_Product;
                    if (product.CompanyReOrderPoint > 0)
                    {
                        base_ProductModel productModel = new base_ProductModel(product);
                        productModel.VendorName = _guestRepository.GetAll(x => x.Mark.Equals(VendorType) && x.IsActived && !x.IsPurged && x.Id.Equals(productModel.VendorId)).SingleOrDefault().Company;
                        //To get purchase order detail
                        string productResource = productModel.Resource.ToString();
                        //To get product with all of stores
                        productModel.OnHandStore = productModel.QuantityOnHand;
                        productModel.QuantityOnSO = productModel.QuantityOnCustomer;
                        productModel.QuantityOnHand = productModel.QuantityOnHand - productModel.QuantityOnSO;
                        productModel.QuantityOnPO = productModel.QuantityOnOrder;
                        productModel.EndUpdate();
                        if (productModel.QuantityOnHand + productModel.QuantityOnPO < productModel.CompanyReOrderPoint)
                        {
                            productModel.ReOrderQuatity = productModel.CompanyReOrderPoint - (productModel.QuantityOnHand + productModel.QuantityOnPO);
                            this.ProductCollection.Add(productModel);
                        }
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    //To count all User in Data base show on grid
                    this.TotalProducts = this.ProductCollection.Count;
                    base.IsBusy = false;
                };
                bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

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
            if (this.ChangeViewExecute(null))
            {
                this.IsSearchMode = true;
            }
        }

        #endregion

        #region LoadData
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            this.SelectedStore = this.StoreCollection[0];
        }
        #endregion

        #endregion
    }
}
