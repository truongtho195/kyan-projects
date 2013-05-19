using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;
using System.Linq.Expressions;
using CPC.POS.Database;
using CPC.POS.Repository;
using System.Windows;

namespace CPC.POS.ViewModel
{
    public class POAdvanceSearchViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public POAdvanceSearchViewModel()
        {

        }

        #endregion

        #region Properties

        #region Predicate

        /// <summary>
        /// Gets Predicate.
        /// </summary>
        private Expression<Func<base_PurchaseOrder, bool>> _predicate;
        public Expression<Func<base_PurchaseOrder, bool>> Predicate
        {
            get
            {
                return _predicate;
            }
        }

        #endregion

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

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, command will executes.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
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

        #region SaveExecute

        /// <summary>
        /// Save.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine SaveExecute method can execute or not.
        /// </summary>
        /// <returns>True is execute.</returns>
        private bool CanSaveExecute()
        {
            return true;
        }

        #endregion

        #region CancelExecute

        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Save

        /// <summary>
        /// Save.
        /// </summary>
        private void Save()
        {
            try
            {
                base_ProductRepository productRepository = new base_ProductRepository();
                base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                _predicate = PredicateBuilder.True<base_PurchaseOrder>();
                if (!string.IsNullOrWhiteSpace(_code))
                {
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        x.Code != null && x.Code.ToLower().Contains(_code.ToLower())).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }
                if (!string.IsNullOrWhiteSpace(_productName))
                {
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        x.ProductName != null && x.ProductName.ToLower().Contains(_productName.ToLower())).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }
                if (!string.IsNullOrWhiteSpace(_category))
                {
                    IEnumerable<int> depIdList = departmentRepository.GetAll(x =>
                            x.LevelId == Define.ProductCategoryLevel && x.Name != null && x.Name.ToLower().Contains(_category.ToLower())).Select(x => x.Id);
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        depIdList.Contains(x.ProductCategoryId)).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }
                if (!string.IsNullOrWhiteSpace(_attributeSize))
                {
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        (x.Attribute != null && x.Attribute.ToLower().Contains(_attributeSize.ToLower())) ||
                        (x.Size != null && x.Size.ToLower().Contains(_attributeSize.ToLower()))).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }
                if (!string.IsNullOrWhiteSpace(_partNumber))
                {
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        x.PartNumber != null && x.PartNumber.ToLower().Contains(_partNumber.ToLower())).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }
                if (!string.IsNullOrWhiteSpace(_barcode))
                {
                    IEnumerable<string> GUIDList = productRepository.GetIEnumerable(x =>
                        x.Barcode != null && x.Barcode.ToLower().Contains(_barcode.ToLower())).Select(x => x.Resource.ToString());
                    _predicate = _predicate.And(x => x.base_PurchaseOrderDetail.Select(y => y.ProductResource).Intersect(GUIDList).Count() > 0);
                }

                FindOwnerWindow(this).DialogResult = true;
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            _predicate = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

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
