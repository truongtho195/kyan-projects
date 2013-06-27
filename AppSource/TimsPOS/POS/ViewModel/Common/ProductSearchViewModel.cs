using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Windows;
using CPC.POS.Repository;
using CPC.POS.Database;
using System.ComponentModel;
using System.Linq.Expressions;
using CPC.Helper;

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

        private readonly string _doubleClick = "DoubleClick";

        private CollectionBase<base_ProductModel> _productCollectionOutSide = new CollectionBase<base_ProductModel>();

        private int _productOutSideCount = 0;

        #endregion

        #region Constructors

        public ProductSearchViewModel()
        {
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(WorkerDoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunWorkerCompleted);

            if (Define.CONFIGURATION.AcceptedPaymentMethod.Value.Has(64))
            {
                //Get PaymentCard with config
                foreach (ComboItem paymentCard in Common.GiftCardTypes)
                {
                    if (Define.CONFIGURATION.AcceptedGiftCardMethod.Has((int)paymentCard.Value))
                    {
                        base_ProductModel couponItem = new base_ProductModel();
                        couponItem.ProductName = paymentCard.Text;
                        string strGuidPatern = "{0}{0}{0}{0}{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}-{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}";
                        couponItem.Code = string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", Convert.ToInt32(paymentCard.ObjValue));
                        string guidID = string.Format(strGuidPatern, Convert.ToInt32(paymentCard.ObjValue));
                        couponItem.Resource = Guid.Parse(guidID);
                        couponItem.IsOpenItem = true;
                        couponItem.IsCoupon = true;
                        _productCollectionOutSide.Add(couponItem);
                    }
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

        private CollectionBase<base_ProductModel> _productCollection;
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

        private base_ProductModel _selectedProduct;
        /// <summary>
        /// Gets or sets SelectedProduct.
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
                    _cancelCommand = new RelayCommand<string>(CancelExecute);
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

        #region CancelExecute

        /// <summary>
        /// Close popup.
        /// </summary>
        private void CancelExecute(string parameter)
        {
            Cancel(parameter);
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
                base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                if (argument == "SearchProduct")
                {
                    _predicate = PredicateBuilder.True<base_Product>();
                    _predicateModel = PredicateBuilder.True<base_ProductModel>();
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
                        IEnumerable<int> depIdList = departmentRepository.GetAll(x =>
                            x.LevelId == Define.ProductCategoryLevel && x.Name != null && x.Name.ToLower().Contains(_category.ToLower())).Select(x => x.Id);
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

                    // Initialize ProductCollection.
                    ProductCollection = new CollectionBase<base_ProductModel>();
                    _productOutSideCount = 0;
                    ProductTotal = productRepository.GetIQueryable(_predicate).Count();
                }

                IList<base_Product> products = productRepository.GetRange(_productCollection.Count - _productOutSideCount, NumberOfDisplayItems, _productColumnSort, _predicate);
                foreach (base_Product product in products)
                {
                    if (productRepository.Refresh(product) != null)
                    {
                        _backgroundWorker.ReportProgress(0, product);
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetProduct

        /// <summary>
        /// Gets a product.
        /// </summary>
        /// <param name="product">product to gets.</param>
        private void GetProduct(base_Product product)
        {
            if (_productCollection.FirstOrDefault(x => x.Id == product.Id) == null)
            {
                _productCollection.Add(new base_ProductModel(product));
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Cancel(string parameter)
        {
            if (parameter == _doubleClick)
            {
                FindOwnerWindow(this).DialogResult = true;
            }
            else
            {
                _selectedProduct = null;
                FindOwnerWindow(this).DialogResult = false;
            }
        }

        #endregion

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
            GetProduct(e.UserState as base_Product);
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result.ToString() == "SearchProduct")
            {
                IQueryable<base_ProductModel> productOutSideList = _productCollectionOutSide.AsQueryable().Where(_predicateModel);
                _productOutSideCount = productOutSideList.Count();
                ProductTotal += _productOutSideCount;
                foreach (base_ProductModel product in productOutSideList)
                {
                    _productCollection.Add(product);
                }
            }

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
