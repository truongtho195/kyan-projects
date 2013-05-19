using System;
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
    public class PopupDepartmentCategoryBrandViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Indicates current location is department, category, or brand.
        /// </summary>
        DCBLocation _currentLocation = DCBLocation.Department;

        #endregion

        #region Contructors

        /// <summary>
        /// Initialize DepartmentCategoryBrandViewModel with specify location.
        /// </summary>
        /// <param name="location">DCBLocation is department, category, or brand.</param>
        /// <param name="parentID">Parent base_DepartmentModel's Id.</param>
        public PopupDepartmentCategoryBrandViewModel(DCBLocation location, int? parentID)
        {
            _ownerViewModel = this;
            _currentLocation = location;

            ArrangeUI();
            InitializeNewItem(parentID);
        }

        #endregion

        #region Properties

        #region NewItem

        private base_DepartmentModel _newItem;
        /// <summary>
        /// Gets a new item if exists.
        /// </summary>
        public base_DepartmentModel NewItem
        {
            get
            {
                return _newItem;
            }
            private set
            {
                if (_newItem != value)
                {
                    _newItem = value;
                    OnPropertyChanged(() => NewItem);
                }
            }
        }

        #endregion

        #region SaleTaxLocations

        private CollectionBase<base_SaleTaxLocationModel> _saleTaxLocations;
        /// <summary>
        /// Gets SaleTaxLocations that contains all base_SaleTaxLocationModels.
        /// </summary>
        public CollectionBase<base_SaleTaxLocationModel> SaleTaxLocations
        {
            get
            {
                return _saleTaxLocations;
            }
            private set
            {
                if (_saleTaxLocations != value)
                {
                    _saleTaxLocations = value;
                    OnPropertyChanged(() => SaleTaxLocations);
                }
            }
        }

        #endregion

        #region VisibilityDepartment

        private Visibility _visibilityDepartment;
        /// <summary>
        /// Gets a value that indicates whether 'Create New Department' part is show or collapsed.
        /// </summary>
        public Visibility VisibilityDepartment
        {
            get
            {
                return _visibilityDepartment;
            }
            private set
            {
                if (_visibilityDepartment != value)
                {
                    _visibilityDepartment = value;
                    OnPropertyChanged(() => VisibilityDepartment);
                }
            }
        }

        #endregion

        #region VisibilityCategory

        private Visibility _visibilityCategory;
        /// <summary>
        /// Gets a value that indicates whether 'Create New Category' part is show or collapsed.
        /// </summary>
        public Visibility VisibilityCategory
        {
            get
            {
                return _visibilityCategory;
            }
            private set
            {
                if (_visibilityCategory != value)
                {
                    _visibilityCategory = value;
                    OnPropertyChanged(() => VisibilityCategory);
                }
            }
        }

        #endregion

        #region VisibilityBrand

        private Visibility _visibilityBrand;
        /// <summary>
        /// Gets a value that indicates whether 'Create New Brand' part is show or collapsed.
        /// </summary>
        public Visibility VisibilityBrand
        {
            get
            {
                return _visibilityBrand;
            }
            private set
            {
                if (_visibilityBrand != value)
                {
                    _visibilityBrand = value;
                    OnPropertyChanged(() => VisibilityBrand);
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
            if (_newItem == null || _newItem.HasError)
            {
                return false;
            }

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
        /// Save department, category, brand.
        /// </summary>
        private void Save()
        {
            try
            {
                base_DepartmentRepository departmentRepository = new base_DepartmentRepository();

                DateTime now = DateTime.Now;
                _newItem.DateCreated = now;
                if (Define.USER != null)
                    _newItem.UserCreated = Define.USER.LoginName;
                _newItem.ToEntity();
                departmentRepository.Add(_newItem.base_Department);
                departmentRepository.Commit();
                _newItem.Id = _newItem.base_Department.Id;
                _newItem.IsNew = false;
                _newItem.IsDirty = false;

                FindOwnerWindow(this).DialogResult = true;
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
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
            NewItem = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #region ArrangeUI

        /// <summary>
        /// Arrange user interface,
        /// </summary>
        private void ArrangeUI()
        {
            VisibilityDepartment = Visibility.Collapsed;
            VisibilityCategory = Visibility.Collapsed;
            VisibilityBrand = Visibility.Collapsed;

            switch (_currentLocation)
            {
                case DCBLocation.Department:
                    VisibilityDepartment = Visibility.Visible;
                    break;

                case DCBLocation.Category:
                    VisibilityCategory = Visibility.Visible;
                    break;

                case DCBLocation.Brand:
                    VisibilityBrand = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region InitializeNewItem

        /// <summary>
        /// Initialize base_DepartmentModel.
        /// </summary>
        /// <param name="parentID">Parent base_DepartmentModel's Id.</param>
        private void InitializeNewItem(int? parentID)
        {
            switch (_currentLocation)
            {
                case DCBLocation.Department:

                    NewItem = new base_DepartmentModel
                    {
                        ParentId = null,
                        LevelId = Define.ProductDeparmentLevel,
                        IsActived = true,
                        IsNew = true,
                        IsDirty = false
                    };

                    break;

                case DCBLocation.Category:

                    GetSaleTaxLocations();

                    base_SaleTaxLocationModel defaultTaxCodeItem = _saleTaxLocations.FirstOrDefault(x =>
                        string.Compare(x.TaxCode, Define.CONFIGURATION.DefaultTaxCodeNewDepartment, false) == 0);

                    NewItem = new base_DepartmentModel
                    {
                        ParentId = parentID,
                        TaxCodeId = defaultTaxCodeItem != null ? defaultTaxCodeItem.TaxCode : null,
                        LevelId = Define.ProductCategoryLevel,
                        IsActived = true,
                        IsNew = true,
                        IsDirty = false
                    };

                    break;

                case DCBLocation.Brand:

                    NewItem = new base_DepartmentModel
                    {
                        ParentId = parentID,
                        LevelId = Define.ProductBrandLevel,
                        IsActived = true,
                        IsNew = true,
                        IsDirty = false
                    };

                    break;
            }
        }

        #endregion

        #region GetSaleTaxLocations

        /// <summary>
        /// Gets SaleTaxLocations
        /// </summary>
        private void GetSaleTaxLocations()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_SaleTaxLocationRepository saleTaxLocationRepository = new base_SaleTaxLocationRepository();

                    //Get SaleTaxLocation primary.
                    base_SaleTaxLocation saleTaxLocationPrimary = saleTaxLocationRepository.Get(x => x.IsPrimary && x.IsActived);
                    if (saleTaxLocationPrimary != null)
                    {
                        SaleTaxLocations = new CollectionBase<base_SaleTaxLocationModel>(saleTaxLocationRepository.GetAll(x =>
                            x.ParentId == saleTaxLocationPrimary.Id).Select(x => new base_SaleTaxLocationModel(saleTaxLocationRepository.Refresh(x))));
                    }
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion
    }
}
