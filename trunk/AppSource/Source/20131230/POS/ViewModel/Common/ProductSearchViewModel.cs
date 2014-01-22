using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class ProductSearchViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Column on product table used for sort.
        /// </summary>
        private readonly string _productColumnSort = "It.Id";

        /// <summary>
        /// Product predicate.
        /// </summary>
        Expression<Func<base_Product, bool>> _predicate = PredicateBuilder.True<base_Product>();
        Expression<Func<base_ProductModel, bool>> _predicateModel = PredicateBuilder.True<base_ProductModel>();

        private CollectionBase<base_ProductModel> _productCollectionOutSide = new CollectionBase<base_ProductModel>();

        private int _productOutSideCount = 0;

        private bool _isIncludeCoupon = true;

        private bool _isIncludeGroupType = true;

        private bool _isIncludeServiceType = true;

        private bool _isIncludeInsuranceType = true;

        private bool? _isIncludeLayaway = false;

        private bool? _isIncludeOpenItem = false;

        private int _storeCode = -1;

        #endregion

        #region Constructors

        public ProductSearchViewModel(bool isIncludeCoupon = true, bool isIncludeGroupType = true, bool isIncludeServiceType = true, bool isIncludeInsuranceType = true, bool? isIncludeLayaway = null, bool? isIncludeOpenItem = null, int storeCode = -1)
        {

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);

            _isIncludeCoupon = isIncludeCoupon;
            _isIncludeGroupType = isIncludeGroupType;
            _isIncludeServiceType = isIncludeServiceType;
            _isIncludeInsuranceType = isIncludeInsuranceType;
            _isIncludeLayaway = isIncludeLayaway;
            _isIncludeOpenItem = isIncludeOpenItem;
            _storeCode = storeCode;

            if (_isIncludeCoupon)
            {
                //Get PaymentCard with config

                string strGuidPatern = "{0}{0}{0}{0}{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                string strCodePatern = "{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCard))
                {
                    ComboItem paymentCard = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCard));
                    base_ProductModel couponItem = new base_ProductModel();
                    couponItem.ProductCategoryId = paymentCard.Value;
                    couponItem.ProductName = paymentCard.Text;
                    couponItem.Code = string.Format(strCodePatern, 1);
                    string guidID = string.Format(strGuidPatern, 1);
                    couponItem.Resource = Guid.Parse(guidID);
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    _productCollectionOutSide.Add(couponItem);
                }

                if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(PaymentMethod.GiftCertificate))
                {
                    ComboItem paymentCard = Common.PaymentMethods.FirstOrDefault(x => x.Value.Equals((short)PaymentMethod.GiftCertificate));
                    base_ProductModel couponItem = new base_ProductModel();
                    couponItem.ProductName = paymentCard.Text;
                    couponItem.ProductCategoryId = paymentCard.Value;
                    couponItem.Code = string.Format(strCodePatern, 2);
                    string guidID = string.Format(strGuidPatern, 2);
                    couponItem.Resource = Guid.Parse(guidID);
                    couponItem.IsOpenItem = true;
                    couponItem.IsCoupon = true;
                    _productCollectionOutSide.Add(couponItem);
                }
            }
        }


        #endregion

        #region Properties

        #region Code

        private string _code;
        /// <summary>
        /// Gets or sets Code.
        /// </summary>
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(() => Code);
                }
            }
        }

        #endregion

        #region ProductName

        private string _productName;
        /// <summary>
        /// Gets or sets ProductName.
        /// </summary>
        public string ProductName
        {
            get
            {
                return _productName;
            }
            set
            {
                if (_productName != value)
                {
                    _productName = value;
                    OnPropertyChanged(() => ProductName);
                }
            }
        }

        #endregion

        #region Category

        private string _category;
        /// <summary>
        /// Gets or sets Category.
        /// </summary>
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged(() => Category);
                }
            }
        }

        #endregion

        #region AttributeSize

        private string _attributeSize;
        /// <summary>
        /// Gets or sets AttributeSize.
        /// </summary>
        public string AttributeSize
        {
            get
            {
                return _attributeSize;
            }
            set
            {
                if (_attributeSize != value)
                {
                    _attributeSize = value;
                    OnPropertyChanged(() => AttributeSize);
                }
            }
        }

        #endregion

        #region PartNumber

        private string _partNumber;
        /// <summary>
        /// Gets or sets PartNumber.
        /// </summary>
        public string PartNumber
        {
            get
            {
                return _partNumber;
            }
            set
            {
                if (_partNumber != value)
                {
                    _partNumber = value;
                    OnPropertyChanged(() => PartNumber);
                }
            }
        }

        #endregion

        #region Barcode

        private string _barcode;
        /// <summary>
        /// Gets or sets Barcode.
        /// </summary>
        public string Barcode
        {
            get
            {
                return _barcode;
            }
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged(() => Barcode);
                }
            }
        }

        #endregion

        #region ProductCollection

        private CollectionBase<base_ProductModel> _productCollection = new CollectionBase<base_ProductModel>();
        /// <summary>
        /// Gets ProductCollection.
        /// </summary>
        public CollectionBase<base_ProductModel> ProductCollection
        {
            get
            {
                return _productCollection;
            }
            private set
            {
                if (_productCollection != value)
                {
                    _productCollection = value;
                    OnPropertyChanged(() => ProductCollection);
                }
            }
        }

        #endregion

        #region ProductTotal

        private int _productTotal;
        /// <summary>
        /// Gets ProductTotal.
        /// </summary>
        public int ProductTotal
        {
            get
            {
                return _productTotal;
            }
            private set
            {
                if (_productTotal != value)
                {
                    _productTotal = value;
                    OnPropertyChanged(() => ProductTotal);
                }
            }
        }

        #endregion

        #region SelectedProduct

        private List<base_ProductModel> _selectedProducts;
        /// <summary>
        /// Gets or sets SelectedProduct.
        /// </summary>
        public List<base_ProductModel> SelectedProducts
        {
            get
            {
                return _selectedProducts;
            }
            set
            {
                if (_selectedProducts != value)
                {
                    _selectedProducts = value;
                }
            }
        }

        #endregion




        #endregion

        #region Command Properties

        #region SearchCommand

        private ICommand _searchCommand;
        /// <summary>
        /// When 'Search' button clicked, command will executes.
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(SearchExecute);
                }
                return _searchCommand;
            }
        }

        #endregion

        #region LoadNextCommand

        private ICommand _loadNextCommand;
        /// <summary>
        /// When DataGrid scroll, command will executes.
        /// </summary>
        public ICommand LoadNextCommand
        {
            get
            {
                if (_loadNextCommand == null)
                {
                    _loadNextCommand = new RelayCommand(LoadNextExecute);
                }
                return _loadNextCommand;
            }
        }

        #endregion

        #region OKCommand

        private ICommand _OKCommand;
        /// <summary>
        /// When 'OK' button clicked, command will executes.
        /// </summary>
        public ICommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new RelayCommand<object>(OKExecute, CanOKExecute);
                }
                return _OKCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// When 'Cancel' button clicked, command will executes.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SearchExecute

        /// <summary>
        /// Search product.
        /// </summary>
        private void SearchExecute()
        {
            SearchProduct();
        }

        #endregion

        #region LoadNextExecute

        /// <summary>
        /// Gets a next range of product.
        /// </summary>
        private void LoadNextExecute()
        {
            LoadNext();
        }

        #endregion

        #region OKExecute

        /// <summary>
        /// OK and close popup.
        /// </summary>
        private void OKExecute(object parameter)
        {
            OK(parameter);
        }

        #endregion

        #region CanOKExecute

        /// <summary>
        /// Determine whether can call OKExecute method.
        /// </summary>
        /// <returns></returns>
        private bool CanOKExecute(object parameter)
        {
            if (parameter == null || !(parameter as ObservableCollection<object>).Any())
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Close popup.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Private Methods

        #region SearchProduct

        /// <summary>
        /// Search product.
        /// </summary>
        private void SearchProduct()
        {
            // Initialize ProductCollection.
            ProductCollection.Clear();
            //Reset Counter
            ProductTotal = 0;

            CreatePredicate("SearchProduct");

            //Search Coupon First
            if (_isIncludeCoupon)
            {
                IQueryable<base_ProductModel> productOutSideList = _productCollectionOutSide.AsQueryable().Where(_predicateModel);
                _productOutSideCount = productOutSideList.Count();
                foreach (base_ProductModel productExt in productOutSideList)
                {
                    _productCollection.Add(productExt);
                }
                _productOutSideCount = productOutSideList.Count();
                ProductTotal = _productOutSideCount;
            }
            _backgroundWorker.RunWorkerAsync("SearchProduct");
        }

        #endregion

        #region LoadNext

        /// <summary>
        /// Gets a next range of product.
        /// </summary>
        private void LoadNext()
        {
            _backgroundWorker.RunWorkerAsync("LoadNext");
        }

        #endregion

        #region GetProducts

        /// <summary>
        /// Gets a range of product.
        /// </summary>
        private void GetProducts(string argument)
        {
            try
            {
                base_ProductRepository productRepository = new base_ProductRepository();

                ProductTotal = productRepository.GetIQueryable(_predicate).Count();

                IList<base_Product> products = productRepository.GetRange(_productCollection.Count - _productOutSideCount, NumberOfDisplayItems, _productColumnSort, _predicate);
                foreach (base_Product product in products)
                {
                    if (productRepository.Refresh(product) != null)
                    {
                        _backgroundWorker.ReportProgress(0, new base_ProductModel(product));
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region GetProduct

        /// <summary>
        /// Gets a product.
        /// </summary>
        /// <param name="product">product to gets.</param>
        private void GetProduct(base_ProductModel productModel)
        {
            if (!_productCollection.Any(x => x.Resource == productModel.Resource))
            {
                _productCollection.Add(productModel);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Cancel()
        {
            _selectedProducts = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #region OK

        /// <summary>
        /// OK and close popup.
        /// </summary>
        private void OK(object parameter)
        {
            SelectedProducts = new List<base_ProductModel>((parameter as ObservableCollection<object>).Cast<base_ProductModel>());
            FindOwnerWindow(this).DialogResult = true;
        }

        #endregion

        /// <summary>
        /// Create Predicate for search condition
        /// </summary>
        /// <param name="argument"></param>
        private void CreatePredicate(string argument)
        {
            base_DepartmentRepository departmentRepository = new base_DepartmentRepository();
            int storeCode = Define.StoreCode;
            short groupType = (short)ItemTypes.Group;
            short serviceType = (short)ItemTypes.Services;
            short insuranceType = (short)ItemTypes.Insurance;

            if (argument == "SearchProduct")
            {
                _predicate = PredicateBuilder.True<base_Product>();
                _predicateModel = PredicateBuilder.True<base_ProductModel>();
                _predicate = _predicate.And(x => x.IsPurge != true);
                if (!_isIncludeGroupType)
                {
                    _predicate = _predicate.And(x => x.ItemTypeId != groupType);
                }
                if (!_isIncludeServiceType)
                {
                    _predicate = _predicate.And(x => x.ItemTypeId != serviceType);
                }
                if (!_isIncludeInsuranceType)
                {
                    _predicate = _predicate.And(x => x.ItemTypeId != insuranceType);
                }
                if (_isIncludeLayaway.HasValue && _isIncludeLayaway.Value)
                {
                    _predicate = _predicate.And(x => x.IsLayaway == _isIncludeLayaway.Value);
                }
                if (_isIncludeOpenItem.HasValue)
                {
                    _predicate = _predicate.And(x => x.IsOpenItem == _isIncludeOpenItem.Value);
                }
                if (_storeCode == -1)
                {
                    // Gets with Define.StoreCode.
                    if (storeCode != 0)
                    {
                        _predicate = _predicate.And(x => x.base_ProductStore.Any(y => y.StoreCode == storeCode));
                    }
                }
                else
                {
                    // Gets with parameter StoreCode.
                    _predicate = _predicate.And(x => x.base_ProductStore.Any(y => y.StoreCode == _storeCode));
                }

                if (!string.IsNullOrWhiteSpace(_code))
                {
                    _predicate = _predicate.And(x => x.Code != null && x.Code.ToLower().Contains(_code.ToLower()));
                    _predicateModel = _predicateModel.And(x => x.Code != null && x.Code.ToLower().Contains(_code.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(_productName))
                {
                    _predicate = _predicate.And(x => x.ProductName != null && x.ProductName.ToLower().Contains(_productName.ToLower()));
                    _predicateModel = _predicateModel.And(x => x.ProductName != null && x.ProductName.ToLower().Contains(_productName.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(_category))
                {
                    short categoryID = (short)ProductDeparmentLevel.Category;

                    IEnumerable<int> depIdList = departmentRepository.GetAll(x =>
                        x.LevelId == categoryID && x.Name != null && x.Name.ToLower().Contains(_category.ToLower())).Select(x => x.Id);
                    _predicate = _predicate.And(x => depIdList.Contains(x.ProductCategoryId));
                    _predicateModel = _predicateModel.And(x => depIdList.Contains(x.ProductCategoryId));
                }
                if (!string.IsNullOrWhiteSpace(_attributeSize))
                {
                    _predicate = _predicate.And(x => (x.Attribute != null && x.Attribute.ToLower().Contains(_attributeSize.ToLower())) ||
                        (x.Size != null && x.Size.ToLower().Contains(_attributeSize.ToLower())));
                    _predicateModel = _predicateModel.And(x => (x.Attribute != null && x.Attribute.ToLower().Contains(_attributeSize.ToLower())) ||
                        (x.Size != null && x.Size.ToLower().Contains(_attributeSize.ToLower())));
                }
                if (!string.IsNullOrWhiteSpace(_partNumber))
                {
                    _predicate = _predicate.And(x => x.PartNumber != null && x.PartNumber.ToLower().Contains(_partNumber.ToLower()));
                    _predicateModel = _predicateModel.And(x => x.PartNumber != null && x.PartNumber.ToLower().Contains(_partNumber.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(_barcode))
                {
                    _predicate = _predicate.And(x => x.Barcode != null && x.Barcode.ToLower().Contains(_barcode.ToLower()));
                    _predicateModel = _predicateModel.And(x => x.Barcode != null && x.Barcode.ToLower().Contains(_barcode.ToLower()));
                }


            }
        }

        #endregion

        #region Events

        #endregion

        #region BackgroundWorker Events

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;

            e.Result = e.Argument.ToString();

            GetProducts(e.Argument.ToString());
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GetProduct(e.UserState as base_ProductModel);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            IsBusy = false;
        }

        #endregion

        #region WriteLog

        private void WriteLog(Exception exception)
        {
            _log4net.Error(string.Format("Message: {0}. Source: {1}.", exception.Message, exception.Source));
            if (exception.InnerException != null)
            {
                _log4net.Error(exception.InnerException.ToString());
            }
        }

        #endregion
    }
}