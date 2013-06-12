using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using CPC.POS.Model;
using System.Collections.ObjectModel;
using CPC.POS.Repository;
using System.ComponentModel;
using CPC.POS.Database;
using System.Windows.Data;
using CPC.POS.View;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class SalesTaxViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand<object> NewCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand<object> DeleteCommand { get; private set; }
        public RelayCommand SearchCommand { get; private set; }
        private base_SaleTaxLocationRepository _saleTaxRespository = new base_SaleTaxLocationRepository();
        private base_SaleTaxLocationOptionRepository _saleTaxOptionRespository = new base_SaleTaxLocationOptionRepository();
        private base_ConfigurationRepository _configurationRepository = new base_ConfigurationRepository();
        private base_DepartmentRepository _departmetReposistory = new base_DepartmentRepository();
        ICollectionView _saleTaxCollectionView;
        private int _selectedId = 0;

        #endregion

        #region Constructors
        public SalesTaxViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();
            Console.WriteLine("=========== Show Configuration  ===========");
            Console.WriteLine(Define.CONFIGURATION.DefaultSaleTaxLocation + " " + Define.CONFIGURATION.DefaultTaxCodeNewDepartment);

        }
        #endregion

        #region Properties

        #region SaleTaxLocationCollection
        private ObservableCollection<base_SaleTaxLocationModel> _saleTaxLocationCollection = new ObservableCollection<base_SaleTaxLocationModel>();
        /// <summary>
        /// Gets or sets the SaleTaxLocationCollection.
        /// </summary>
        public ObservableCollection<base_SaleTaxLocationModel> SaleTaxLocationCollection
        {
            get { return _saleTaxLocationCollection; }
            set
            {
                if (_saleTaxLocationCollection != value)
                {
                    _saleTaxLocationCollection = value;
                    OnPropertyChanged(() => SaleTaxLocationCollection);
                }
            }
        }
        #endregion

        #region SelectedSaleTaxLocation
        private base_SaleTaxLocationModel _selectedSaleTaxLocation;
        /// <summary>
        /// Gets or sets the SelectedSaleTaxLocation.
        /// </summary>
        public base_SaleTaxLocationModel SelectedSaleTaxLocation
        {
            get { return _selectedSaleTaxLocation; }
            set
            {
                if (_selectedSaleTaxLocation != value && EnableSelection)
                {
                    _selectedSaleTaxLocation = value;
                    OnPropertyChanged(() => SelectedSaleTaxLocation);
                    SaleTaxLocationChanged();
                }
            }
        }

        private void SaleTaxLocationChanged()
        {
            if (SelectedSaleTaxLocation != null)
            {
                if (SelectedSaleTaxLocation.ParentId == 0)
                {
                    IsParent = true;
                }
                else
                {//Tax Code
                    IsParent = false;
                    SelectedSaleTaxLocation.RaiseProperyChanged("HasTaxCodeOption");
                    //Get Tax Code Collection for validation duplicate Taxcode
                    SelectedSaleTaxLocation.TaxCodeCollection = new ObservableCollection<base_SaleTaxLocationModel>(SaleTaxLocationCollection.Where(x => x.ParentId == SelectedSaleTaxLocation.ParentId));
                }
                EnableEditForm = true;
                SelectedSaleTaxLocation.PropertyChanged -= new PropertyChangedEventHandler(SelectedSaleTaxLocation_PropertyChanged);
                SelectedSaleTaxLocation.PropertyChanged += new PropertyChangedEventHandler(SelectedSaleTaxLocation_PropertyChanged);
            }
            else
            {
                EnableEditForm = false;
            }

        }

        private void SelectedSaleTaxLocation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("IsDirty".Equals(e.PropertyName))
            {
                if ((sender as base_SaleTaxLocationModel).IsDirty)//Lock grid 
                    EnableSelection = false;
                else
                    EnableSelection = true;
            }
        }
        #endregion

        #region TaxLocationCollection
        private ObservableCollection<base_SaleTaxLocationModel> _defaultTaxLocationCollection;
        /// <summary>
        /// Gets or sets the TaxLocationCollection.
        /// </summary>
        public ObservableCollection<base_SaleTaxLocationModel> DefaultTaxLocationCollection
        {
            get { return _defaultTaxLocationCollection; }
            set
            {
                if (_defaultTaxLocationCollection != value)
                {
                    _defaultTaxLocationCollection = value;
                    OnPropertyChanged(() => DefaultTaxLocationCollection);
                }
            }
        }
        #endregion

        #region PrimaryTaxLocation
        private base_SaleTaxLocationModel _primaryTaxLocation;
        /// <summary>
        /// Gets or sets the PrimaryTaxLocation.
        /// Using for get Primary TaxCodeCollection & set select SaleTaxLocation To Primary
        /// </summary>
        public base_SaleTaxLocationModel PrimaryTaxLocation
        {
            get { return _primaryTaxLocation; }
            set
            {
                if (_primaryTaxLocation != value)
                {
                    _primaryTaxLocation = value;
                    OnPropertyChanged(() => PrimaryTaxLocation);
                    if (PrimaryTaxLocation != null)
                    {
                        PrimaryTaxLocation.PrimarySaleTaxEdited = true;
                        //Get Primary TaxCodeCollection
                        DefaultTaxCode = PrimaryTaxLocation.TaxCodeCollection.FirstOrDefault();

                        //If tax location set to ShippingTaxCodeID
                        if (SelectedSaleTaxLocation != null && SelectedSaleTaxLocation.ParentId == 0)
                        {
                            SelectedSaleTaxLocation.ShippingTaxCodeId = -1;
                            SelectedSaleTaxLocation.ShippingTaxCodeId = DefaultTaxCode.Id;
                        }
                    }
                }
            }
        }
        #endregion

        #region DefaultTaxCode
        private base_SaleTaxLocationModel _defaultTaxCode;
        /// <summary>
        /// Gets or sets the DefaultTaxCode.
        /// </summary>
        public base_SaleTaxLocationModel DefaultTaxCode
        {
            get { return _defaultTaxCode; }
            set
            {
                if (_defaultTaxCode != value)
                {
                    _defaultTaxCode = value;
                    OnPropertyChanged(() => DefaultTaxCode);
                    if (PrimaryTaxLocation != null)
                        PrimaryTaxLocation.PrimarySaleTaxEdited = true;
                    if (IsDirty)//Lock grid 
                        EnableSelection = false;
                    else
                        EnableSelection = true;
                }
            }
        }
        #endregion

        #region IsParent
        private bool _isParent = true;
        /// <summary>
        /// Gets or sets the IsParent.
        /// </summary>
        public bool IsParent
        {
            get { return _isParent; }
            set
            {
                if (_isParent != value)
                {
                    _isParent = value;
                    OnPropertyChanged(() => IsParent);
                }
            }
        }
        #endregion

        #region IsDirty

        /// <summary>
        /// Gets or sets the IsParent.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedSaleTaxLocation == null)
                    return false;
                return SelectedSaleTaxLocation.IsDirty ||
                       (SelectedSaleTaxLocation.SaleTaxLocationOptionCollection != null
                        && (SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Has(x => x.IsDirty)//Has Item Changed
                         || SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.DeletedItems.Count > 0) ||//Has Item Deleted
                         PrimaryTaxLocation.PrimarySaleTaxEdited);//TaxLocation defalt changed

            }
        }
        #endregion

        #region EnableSelection
        private bool _enableSelection = true;
        /// <summary>
        /// Gets or sets the EnableSelection.
        /// </summary>
        public bool EnableSelection
        {
            get { return _enableSelection; }
            set
            {
                if (_enableSelection != value)
                {
                    _enableSelection = value;
                    OnPropertyChanged(() => EnableSelection);
                }
            }
        }
        #endregion

        #region EnableEditForm
        private bool _enableEditForm = true;
        /// <summary>
        /// Gets or sets the EnableEditForm.
        /// </summary>
        public bool EnableEditForm
        {
            get { return _enableEditForm; }
            set
            {
                if (_enableEditForm != value)
                {
                    _enableEditForm = value;
                    OnPropertyChanged(() => EnableEditForm);
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
            if (ChangeViewExecute(null))
            {
                int parentId = int.Parse(param.ToString());
                SelectedSaleTaxLocation = CreateSaleTaxLocationOrTaxCode(parentId);
                EnableSelection = false;
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute()
        {
            if (SelectedSaleTaxLocation == null)
                return false;
            return IsDirty && IsValid;
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute()
        {
            if (SelectedSaleTaxLocation.IsNew)
                SelectedSaleTaxLocation = SaveNewSaleTax(SelectedSaleTaxLocation);
            else
                UpdateSaleTax(SelectedSaleTaxLocation);

            UpdatePrimaryTaxLocation();

            EnableSelection = true;
            SelectedSaleTaxLocation.IsSelected = true;
            SelectedSaleTaxLocation.RaiseProperyChanged("HasTaxCodeOption");
        }

        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            base_SaleTaxLocationModel deletedItem = (param as base_SaleTaxLocationModel);
            return !IsDirty && ((deletedItem.ParentId == 0 && !deletedItem.IsPrimary) ||
                        (deletedItem.ParentId != 0
                            && SaleTaxLocationCollection.Count(x => x.ParentId == deletedItem.ParentId) > 1));
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to delete", "POS", MessageBoxButton.YesNo);
            if (result.Is(MessageBoxResult.Yes))
            {
                SelectedSaleTaxLocation = (param as base_SaleTaxLocationModel);
                EnableSelection = true;
                if (!SelectedSaleTaxLocation.IsNew)
                {
                    //Check if TaxCode is default
                    if (SelectedSaleTaxLocation.ParentId != 0)
                    {
                        if (DefaultTaxCode.TaxCode == SelectedSaleTaxLocation.TaxCode)
                        {
                            if (PrimaryTaxLocation.TaxCodeCollection.Any(x => x.Id != SelectedSaleTaxLocation.Id))
                            {
                                DefaultTaxCode = PrimaryTaxLocation.TaxCodeCollection.FirstOrDefault(x => x.Id != SelectedSaleTaxLocation.Id);
                                UpdatePrimaryTaxLocation();
                            }
                        }
                        //check item Remove has using for shipping && update ShippingTaxCodeId
                        if (SaleTaxLocationCollection.Any(x => x.ShippingTaxCodeId.Equals(SelectedSaleTaxLocation.Id)))
                        {
                            foreach (base_SaleTaxLocationModel TaxLocationModel in SaleTaxLocationCollection.Where(x => x.ParentId==0 && x.ShippingTaxCodeId.Equals(SelectedSaleTaxLocation.Id)))
                            {
                                TaxLocationModel.ShippingTaxCodeId = DefaultTaxCode.Id;
                                TaxLocationModel.ToEntity();
                                TaxLocationModel.EndUpdate();
                            }
                            _saleTaxRespository.Commit();
                        }
                    }
                    
                    UpdateDepartment(SelectedSaleTaxLocation.TaxCode, DefaultTaxCode.TaxCode);
                    DeleteSaleTax(SelectedSaleTaxLocation);
                }

                SelectedSaleTaxLocation = SaleTaxLocationCollection.First();
                GetDefaultTaxLocationTaxCodeCollection();
            }


        }
        #endregion

        #region CancelCommand
        /// <summary>
        /// Gets the Edit Command.
        /// <summary>
        public RelayCommand CancelCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Edit command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            if (SelectedSaleTaxLocation == null)
                return false;
            return IsDirty || SelectedSaleTaxLocation.IsNew;
        }


        /// <summary>
        /// Method to invoke when the Edit command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            RollBackSaleTaxLocation();
            EnableSelection = true;//Unlock Grid
            if (SelectedSaleTaxLocation != null)
            {
                if (SelectedSaleTaxLocation.IsNew)
                    SelectedSaleTaxLocation = SaleTaxLocationCollection.FirstOrDefault();
                else
                    SelectedSaleTaxLocation.IsSelected = true;
            }

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

        #region SelectionChangedCommand
        /// <summary>
        /// Gets the SelectionChanged Command.
        /// <summary>

        public RelayCommand<object> SelectionChangedCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SelectionChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectionChangedCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SelectionChanged command is executed.
        /// </summary>
        private void OnSelectionChangedCommandExecute(object param)
        {

            //if (param != null)
            //{
            //    //Check Set Seleted Item to Datagrid
            //    var result = MessageBox.Show("Change Item", "POS", MessageBoxButton.YesNo);
            //    if (result.Equals(MessageBoxResult.Yes))
            //    {

            //        SelectedSaleTaxLocation = param as base_SaleTaxLocationModel;
            //        if (SelectedSaleTaxLocation.ParentId == 0)
            //            IsParent = true;
            //        else
            //            IsParent = false;
            //    }
            //    else
            //    {




            //    }
            //}

        }
        #endregion

        #region TaxOptionCommand
        /// <summary>
        /// Gets the TaxOption Command.
        /// <summary>

        public RelayCommand<object> TaxOptionCommand { get; private set; }

        /// <summary>
        /// Method to check whether the TaxOption command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnTaxOptionCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the TaxOption command is executed.
        /// </summary>
        private void OnTaxOptionCommandExecute(object param)
        {
            if (param != null)
            {
                int option = (int)param;
                TaxOptionViewModel taxOptionViewModel = new TaxOptionViewModel();
                if (option.Is(SalesTaxOption.Single))
                {
                    taxOptionViewModel.TaxOption = SalesTaxOption.Single;

                    base_SaleTaxLocationOptionModel saleTaxOption = new base_SaleTaxLocationOptionModel();
                    saleTaxOption.TaxComponent = string.Empty;
                    saleTaxOption.TaxCondition = 0;
                    saleTaxOption.TaxAgency = string.Empty;
                    saleTaxOption.IsTemporary = true;
                    //Get First sale tax Location 
                    if (SelectedSaleTaxLocation.TaxCodeOptionModel == null)
                    {
                        SelectedSaleTaxLocation.TaxCodeOptionModel = saleTaxOption;
                    }

                    taxOptionViewModel.SaleTaxLocationOptionModel = SelectedSaleTaxLocation.TaxCodeOptionModel;
                    bool? result = _dialogService.ShowDialog<SingleRateTaxView>(_ownerViewModel, taxOptionViewModel, "POS");
                    if (result == true)
                    {
                        if (SelectedSaleTaxLocation.TaxCodeOptionModel.IsTemporary)
                        {
                            SelectedSaleTaxLocation.TaxCodeOptionModel.IsTemporary = false;
                            SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Add(SelectedSaleTaxLocation.TaxCodeOptionModel);
                        }

                        //For if user has choice Multi Tax => remove all tax difference with saleTaxOption(need remove some value in database)
                        foreach (var taxOptionDel in SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Where(x => x.GuidID != SelectedSaleTaxLocation.TaxCodeOptionModel.GuidID).ToList())
                        {
                            SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Remove(taxOptionDel);
                        }
                        saleTaxOption.IsAllowSpecificItemPriceRange = false;

                        saleTaxOption.RaiseProperyChanged("TaxRateOption");
                        //For Under line TaxOption
                        SelectedSaleTaxLocation.RaiseProperyChanged("HasTaxCodeOption");
                    }
                }
                else if (option.Is(SalesTaxOption.Price))
                {
                    taxOptionViewModel.TaxOption = SalesTaxOption.Price;
                    base_SaleTaxLocationOptionModel saleTaxOption = new base_SaleTaxLocationOptionModel();
                    saleTaxOption.TaxComponent = string.Empty;
                    saleTaxOption.TaxCondition = 0;
                    saleTaxOption.IsTemporary = true;
                    saleTaxOption.TaxAgency = string.Empty;
                    //Get First sale tax Location 
                    if (SelectedSaleTaxLocation.TaxCodeOptionModel == null)
                        SelectedSaleTaxLocation.TaxCodeOptionModel = saleTaxOption;

                    taxOptionViewModel.SaleTaxLocationOptionModel = SelectedSaleTaxLocation.TaxCodeOptionModel;
                    bool? result = _dialogService.ShowDialog<PriceDependentView>(_ownerViewModel, taxOptionViewModel, "POS");
                    if (result == true)
                    {
                        if (SelectedSaleTaxLocation.TaxCodeOptionModel.IsTemporary)
                        {
                            SelectedSaleTaxLocation.TaxCodeOptionModel.IsTemporary = false;
                            SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Add(SelectedSaleTaxLocation.TaxCodeOptionModel);
                        }
                        //For if user has choice Multi Tax => remove all tax difference with saleTaxOption(need remove some value in database)
                        foreach (var taxOptionDel in SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Where(x => x.GuidID != SelectedSaleTaxLocation.TaxCodeOptionModel.GuidID).ToList())
                        {
                            SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.Remove(taxOptionDel);
                        }
                        SelectedSaleTaxLocation.TaxCodeOptionModel.IsAllowSpecificItemPriceRange = false;
                        SelectedSaleTaxLocation.TaxCodeOptionModel.RaiseProperyChanged("TaxRateOption");
                        //For Under line TaxOption
                        SelectedSaleTaxLocation.TaxCodeOptionModel.RaiseProperyChanged("HasTaxCodeOption");
                    }
                }
                else//Multi Tax Rate
                {
                    taxOptionViewModel.TaxOption = SalesTaxOption.Multi;
                    taxOptionViewModel.SaleTaxLocationOptionCollection = SelectedSaleTaxLocation.SaleTaxLocationOptionCollection;
                    bool? result = _dialogService.ShowDialog<MultiRateTaxView>(_ownerViewModel, taxOptionViewModel, "POS");
                    if (result == false)//Rollback data
                    {
                        SelectedSaleTaxLocation.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(
                                     SelectedSaleTaxLocation.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)
                                     {
                                         SaleTaxCodeHeader = x.TaxComponent
                                     }));
                    }
                    else
                    {
                        if (SelectedSaleTaxLocation.SaleTaxLocationOptionCollection.DeletedItems.Count > 0)
                            SelectedSaleTaxLocation.IsDirty = true;
                    }
                    foreach (base_SaleTaxLocationOptionModel saleTaxOptionModel in SelectedSaleTaxLocation.SaleTaxLocationOptionCollection)
                    {
                        saleTaxOptionModel.RaiseProperyChanged("TaxRateOption");
                        //For Under line TaxOption
                        SelectedSaleTaxLocation.RaiseProperyChanged("HasTaxCodeOption");
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            NewCommand = new RelayCommand<object>(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand(OnSearchCommandExecute, OnSearchCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            SelectionChangedCommand = new RelayCommand<object>(OnSelectionChangedCommandExecute, OnSelectionChangedCommandCanExecute);
            TaxOptionCommand = new RelayCommand<object>(OnTaxOptionCommandExecute, OnTaxOptionCommandCanExecute);
        }

        private void SetDataToModel(base_SaleTaxLocationModel saleTaxLocationModel)
        {

            //SaleTaxOption Collection
            saleTaxLocationModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>(
                saleTaxLocationModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Select(x => new base_SaleTaxLocationOptionModel(x)
                {
                    SaleTaxCodeHeader = x.TaxComponent
                }));
            if (saleTaxLocationModel.ParentId != 0 && saleTaxLocationModel.SaleTaxLocationOptionCollection.Any())//IsTaxCode
            {
                saleTaxLocationModel.TaxCodeOptionModel = saleTaxLocationModel.SaleTaxLocationOptionCollection.FirstOrDefault();
            }
            saleTaxLocationModel.IsDirty = false;
            //For Under line TaxOption
            saleTaxLocationModel.RaiseProperyChanged("HasTaxCodeOption");
        }

       

        /// <summary>
        /// Create New Sales Tax Location or Tax Code
        /// </summary>
        /// <param name="parentId">parentId>0 : Create TaxCode</param>
        /// <returns></returns>
        private base_SaleTaxLocationModel CreateSaleTaxLocationOrTaxCode(int parentId = 0, bool isTemporary = false)
        {
            base_SaleTaxLocationModel saleTaxLocationModel = new base_SaleTaxLocationModel();
            saleTaxLocationModel.Name = string.Empty;
            saleTaxLocationModel.IsTemporary = isTemporary;
            saleTaxLocationModel.ParentId = parentId;
            saleTaxLocationModel.IsShipingTaxable = false;
            saleTaxLocationModel.IsActived = true;
            saleTaxLocationModel.TaxOption = 0;
            
            if (saleTaxLocationModel.ParentId != 0)
                saleTaxLocationModel.SaleTaxLocationOptionCollection = new CollectionBase<base_SaleTaxLocationOptionModel>();

            saleTaxLocationModel.SortIndex = GetSortIndex(saleTaxLocationModel);
            saleTaxLocationModel.ShippingTaxCodeId = PrimaryTaxLocation.TaxCodeCollection.FirstOrDefault().Id;

            saleTaxLocationModel.IsDirty = false;
            return saleTaxLocationModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleTaxLocationModel"></param>
        private base_SaleTaxLocationModel SaveNewSaleTax(base_SaleTaxLocationModel saleTaxLocationModel)
        {
            if (saleTaxLocationModel.ParentId != 0)
                InsertNewTaxCode(saleTaxLocationModel);
            else
            {
                InsertNewTaxLocation(saleTaxLocationModel);
            }
            GetDefaultTaxLocationTaxCodeCollection();
            //this.SortSaleTaxCollection();
            return saleTaxLocationModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleTaxLocationModel"></param>
        private void UpdateSaleTax(base_SaleTaxLocationModel saleTaxLocationModel)
        {
            if (saleTaxLocationModel.ParentId != 0)//Tax Code
            {
                if (!saleTaxLocationModel.TaxCode.Equals(saleTaxLocationModel.base_SaleTaxLocation.TaxCode))
                {
                    UpdateDepartment(saleTaxLocationModel.base_SaleTaxLocation.TaxCode, saleTaxLocationModel.TaxCode);
                }
                saleTaxLocationModel.Name = saleTaxLocationModel.TaxCode;
                //Set For Tax Code Option
                if (saleTaxLocationModel.SaleTaxLocationOptionCollection != null)
                {
                    //Remove taxcode new delete
                    if (saleTaxLocationModel.SaleTaxLocationOptionCollection.DeletedItems != null && saleTaxLocationModel.SaleTaxLocationOptionCollection.DeletedItems.Count > 0)
                    {
                        foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxLocationModel.SaleTaxLocationOptionCollection.DeletedItems)
                        {
                            _saleTaxOptionRespository.Delete(taxCodeOptionModel.base_SaleTaxLocationOption);
                            _saleTaxOptionRespository.Commit();
                        }
                        saleTaxLocationModel.SaleTaxLocationOptionCollection.DeletedItems.Clear();
                    }

                    foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxLocationModel.SaleTaxLocationOptionCollection)
                    {
                        taxCodeOptionModel.ToEntity();
                        if (taxCodeOptionModel.IsNew)
                            saleTaxLocationModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Add(taxCodeOptionModel.base_SaleTaxLocationOption);
                        taxCodeOptionModel.EndUpdate();
                    }
                }

                if (saleTaxLocationModel.IsDirty) //If Tax code has Changed => update item has the same name with tax code
                {
                    foreach (base_SaleTaxLocationModel taxCodeModel in SaleTaxLocationCollection.Where(x => x.ParentId != 0 && x.TaxCode.Equals(saleTaxLocationModel.base_SaleTaxLocation.TaxCode)))
                    {
                        taxCodeModel.UpdateFrom(saleTaxLocationModel);
                        taxCodeModel.ToEntity();
                        taxCodeModel.EndUpdate();
                    }
                }
            }

            saleTaxLocationModel.ToEntity();
            _saleTaxRespository.Commit();

            //Set Id For Tax Option
            if (saleTaxLocationModel.SaleTaxLocationOptionCollection != null)
            {
                foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxLocationModel.SaleTaxLocationOptionCollection.Where(x => x.Id == 0))
                {
                    taxCodeOptionModel.Id = taxCodeOptionModel.base_SaleTaxLocationOption.Id;
                    taxCodeOptionModel.SaleTaxLocationId = taxCodeOptionModel.base_SaleTaxLocationOption.SaleTaxLocationId;
                    taxCodeOptionModel.EndUpdate();
                }
            }
            saleTaxLocationModel.EndUpdate();
        }

        /// <summary>
        /// Insert new Tax Location to Database with Parent ID =0
        /// </summary>
        /// <param name="saleTaxLocationModel"></param>
        private void InsertNewTaxLocation(base_SaleTaxLocationModel saleTaxLocationModel)
        {
            saleTaxLocationModel.ToEntity();
            //Add Entity To Database
            _saleTaxRespository.Add(saleTaxLocationModel.base_SaleTaxLocation);
            _saleTaxRespository.Commit();

            //Add To Collection binding datagrid
            SaleTaxLocationCollection.Add(saleTaxLocationModel);

            //Set id For tax Location
            saleTaxLocationModel.Id = saleTaxLocationModel.base_SaleTaxLocation.Id;

            //Check if any Tax code in collection => copy to new this TaxLocation
            if (SaleTaxLocationCollection.Any(x => x.ParentId != 0))
            {
                //get any TaxLocationID has tax Code Collection
                base_SaleTaxLocationModel taxLocation = SaleTaxLocationCollection.Where(x => x.ParentId == 0 && x.Id != saleTaxLocationModel.Id).FirstOrDefault();

                foreach (base_SaleTaxLocationModel taxCodeModel in SaleTaxLocationCollection.Where(x => x.ParentId == taxLocation.Id).ToList())
                {
                    base_SaleTaxLocationModel newTaxCodeModel = CreateSaleTaxLocationOrTaxCode(saleTaxLocationModel.Id);
                    newTaxCodeModel.UpdateFrom(taxCodeModel);
                    newTaxCodeModel.ToEntity();
                    _saleTaxRespository.Add(newTaxCodeModel.base_SaleTaxLocation);
                    SaleTaxLocationCollection.Add(newTaxCodeModel);
                }
                _saleTaxRespository.Commit();

                //Set Id to TaxCode Collection of this TaxLocation
                foreach (base_SaleTaxLocationModel taxCodeModel in SaleTaxLocationCollection.Where(x => x.IsDirty))
                {
                    taxCodeModel.ToModel();
                    taxCodeModel.EndUpdate();
                }
            }
            saleTaxLocationModel.EndUpdate();
        }

        /// <summary>
        /// Insert new Tax Code to Database with Parent ID >0
        /// </summary>
        /// <param name="saleTaxCodeModel"></param>
        private void InsertNewTaxCode(base_SaleTaxLocationModel saleTaxCodeModel)
        {
            saleTaxCodeModel.Name = saleTaxCodeModel.TaxCode;
            //Set For Tax Code Option
            if (saleTaxCodeModel.SaleTaxLocationOptionCollection != null)
            {
                foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxCodeModel.SaleTaxLocationOptionCollection)
                {
                    taxCodeOptionModel.ToEntity();
                    saleTaxCodeModel.base_SaleTaxLocation.base_SaleTaxLocationOption.Add(taxCodeOptionModel.base_SaleTaxLocationOption);
                    taxCodeOptionModel.EndUpdate();
                }
            }

            //If Create new Tax Code => add to another SaleTaxLocation
            foreach (base_SaleTaxLocationModel saleTaxModel in SaleTaxLocationCollection.Where(x => x.ParentId == 0 && x.Id != saleTaxCodeModel.ParentId).ToList())
            {
                base_SaleTaxLocationModel taxCodeModel = CreateSaleTaxLocationOrTaxCode(saleTaxModel.Id);
                taxCodeModel.UpdateFrom(saleTaxCodeModel);
                taxCodeModel.ToEntity();
                _saleTaxRespository.Add(taxCodeModel.base_SaleTaxLocation);
                SaleTaxLocationCollection.Add(taxCodeModel);
            }

            saleTaxCodeModel.ToEntity();
            //Add Entity To Database
            _saleTaxRespository.Add(saleTaxCodeModel.base_SaleTaxLocation);
            _saleTaxRespository.Commit();

            //Set ID
            //Set Id For Tax Option For New Item
            foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxCodeModel.SaleTaxLocationOptionCollection)
            {
                taxCodeOptionModel.Id = taxCodeOptionModel.base_SaleTaxLocationOption.Id;
                taxCodeOptionModel.EndUpdate();
            }

            //set id for relation item
            foreach (base_SaleTaxLocationModel saleTaxModel in SaleTaxLocationCollection.Where(x => x.ParentId != 0 && x.TaxCode.Equals(saleTaxCodeModel.TaxCode)))
            {
                if (saleTaxModel.SaleTaxLocationOptionCollection != null)
                {
                    foreach (base_SaleTaxLocationOptionModel taxCodeOptionModel in saleTaxModel.SaleTaxLocationOptionCollection)
                    {
                        taxCodeOptionModel.ToModel();
                        taxCodeOptionModel.EndUpdate();
                    }
                }
                saleTaxCodeModel.ToModel();
                saleTaxModel.EndUpdate();
            }
            saleTaxCodeModel.EndUpdate();
            SaleTaxLocationCollection.Add(saleTaxCodeModel);
        }

        /// <summary>
        /// Need Check Primary
        /// 06/06/2013: current no check primary
        /// </summary>
        /// <param name="saleTaxLocationModel"></param>
        private void DeleteSaleTax(base_SaleTaxLocationModel saleTaxLocationModel)
        {
            if (saleTaxLocationModel.ParentId == 0)//Delete Tax Location => Delete All relation Tax code & SaleTaxOption
            {
                foreach (base_SaleTaxLocationModel taxCodeModel in SaleTaxLocationCollection.Where(x => x.ParentId == saleTaxLocationModel.Id).ToList())
                {
                    DeleteTaxCodeTaxOption(taxCodeModel);
                }
                _saleTaxRespository.Delete(saleTaxLocationModel.base_SaleTaxLocation);
                _saleTaxRespository.Commit();
                SaleTaxLocationCollection.Remove(saleTaxLocationModel);
                //Get Default Tax Location
                DefaultTaxLocationCollection = new ObservableCollection<base_SaleTaxLocationModel>(SaleTaxLocationCollection.Where(x => x.ParentId == 0));
                SetTaxLocationPrimary();
            }
            else //Delete Tax Code => delete All The same name Tax code in collection
            {
                foreach (base_SaleTaxLocationModel taxCodeModel in SaleTaxLocationCollection.Where(x => x.ParentId != 0 && x.TaxCode.Equals(saleTaxLocationModel.TaxCode)).ToList())
                {
                    DeleteTaxCodeTaxOption(taxCodeModel);
                }
            }
        }

        private void SetTaxLocationPrimary()
        {
            //Get TaxCode
            base_SaleTaxLocationModel primaryTaxLocation = SaleTaxLocationCollection.SingleOrDefault(x => x.IsPrimary);
            if (primaryTaxLocation != null)
            {
                PrimaryTaxLocation = primaryTaxLocation;
                PrimaryTaxLocation.PrimarySaleTaxEdited = false;
            }
        }

        /// <summary>
        /// Get TaxCode default from Configuration
        /// </summary>
        private void SetDefaultTaxCode()
        {
            base_SaleTaxLocationModel primaryTaxLocation = SaleTaxLocationCollection.SingleOrDefault(x => x.IsPrimary);
            //Get default TaxCode
            IQueryable<base_Configuration> configQuery = _configurationRepository.GetIQueryable();
            DefaultTaxCode = PrimaryTaxLocation.TaxCodeCollection.FirstOrDefault();

            if (configQuery.Any())
            {
                base_Configuration config = configQuery.FirstOrDefault();
                base_SaleTaxLocationModel taxCodeDefault = SaleTaxLocationCollection.FirstOrDefault(x => x.ParentId == primaryTaxLocation.Id && x.TaxCode.Equals(config.DefaultTaxCodeNewDepartment));
                if (taxCodeDefault != null)
                    DefaultTaxCode = taxCodeDefault;
            }
        }

        /// <summary>
        /// Get TaxLocationCollection & tax Code
        /// </summary>
        private void GetDefaultTaxLocationTaxCodeCollection()
        {
            DefaultTaxLocationCollection = new ObservableCollection<base_SaleTaxLocationModel>(this.SaleTaxLocationCollection.Where(x => x.ParentId == 0).ToList());
            foreach (base_SaleTaxLocationModel taxLocationModel in DefaultTaxLocationCollection)
            {
                taxLocationModel.TaxCodeCollection = new ObservableCollection<base_SaleTaxLocationModel>(SaleTaxLocationCollection.Where(x => x.ParentId == taxLocationModel.Id));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taxCodeModel"></param>
        private void DeleteTaxCodeTaxOption(base_SaleTaxLocationModel taxCodeModel)
        {
            //Delete Sale Tax Option
            if (taxCodeModel.SaleTaxLocationOptionCollection != null)
            {
                foreach (base_SaleTaxLocationOptionModel taxCodeOption in taxCodeModel.SaleTaxLocationOptionCollection.ToList())
                {
                    if (!taxCodeOption.IsNew)
                        _saleTaxOptionRespository.Delete(taxCodeOption.base_SaleTaxLocationOption);
                    taxCodeModel.SaleTaxLocationOptionCollection.Remove(taxCodeOption);
                }
                _saleTaxOptionRespository.Commit();
            }
            //Delete Tax Code
            _saleTaxRespository.Delete(taxCodeModel.base_SaleTaxLocation);
            _saleTaxRespository.Commit();
            SaleTaxLocationCollection.Remove(taxCodeModel);
        }

        /// <summary>
        /// Insert Default Sale tax if Sale tax is Empty
        /// </summary>
        private void InsertDefaultTaxLocation()
        {
            base_SaleTaxLocationModel taxLocationModel = CreateSaleTaxLocationOrTaxCode();
            taxLocationModel.IsPrimary = true;
            taxLocationModel.Name = "Sale Tax Location";
            SaveNewSaleTax(taxLocationModel);
        }

        /// <summary>
        /// Update Primary TaxLocation & Update To Configuration Table
        /// </summary>
        private void UpdatePrimaryTaxLocation()
        {
            //Set Primary TaxLocation & Set ShippingTaxCodeID
            foreach (base_SaleTaxLocationModel saleTaxLocationModel in SaleTaxLocationCollection.Where(x => x.ParentId == 0))
            {
                //saleTaxLocationModel.ShippingTaxCodeId = DefaultTaxCode.Id;
                if (saleTaxLocationModel.Id == PrimaryTaxLocation.Id)
                    saleTaxLocationModel.IsPrimary = true;
                else
                    saleTaxLocationModel.IsPrimary = false;
                saleTaxLocationModel.ToEntity();
                saleTaxLocationModel.EndUpdate();
            }
            _saleTaxRespository.Commit();
            //Update To Configuration Table
            IQueryable<base_Configuration> configQuery = _configurationRepository.GetIQueryable();
            if (configQuery.Any())
            {
                base_Configuration config = configQuery.FirstOrDefault();

                string defaultTaxCode = PrimaryTaxLocation.TaxCodeCollection.SingleOrDefault(x => x.Id == DefaultTaxCode.Id).TaxCode;

                //Set taxCode for config
                config.DefaultSaleTaxLocation = (short)PrimaryTaxLocation.Id;
                config.DefaultTaxCodeNewDepartment = defaultTaxCode;

                _configurationRepository.Commit();

                //Update for define
                Define.CONFIGURATION = new base_ConfigurationModel(config);
                (_ownerViewModel as MainViewModel).LoadTaxLocationAndCode();
                
            }

            if (this.PrimaryTaxLocation.PrimarySaleTaxEdited)
                this.PrimaryTaxLocation.PrimarySaleTaxEdited = false;
        }

        /// <summary>
        /// Using for update taxcode for department when taxcode change or deleted
        /// </summary>
        /// <param name="fromTaxCode"></param>
        /// <param name="toTaxcode"></param>
        private void UpdateDepartment(string fromTaxCode, string toTaxcode)
        {
            IList<base_Department> listDepartment = _departmetReposistory.GetAll(x => x.LevelId == Define.ProductCategoryLevel && x.TaxCodeId.Equals(fromTaxCode));
            if (listDepartment != null && listDepartment.Count > 0)
            {
                foreach (base_Department department in listDepartment)
                {
                    department.TaxCodeId = toTaxcode;
                }
                _departmetReposistory.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SortSaleTaxCollection()
        {
            if (_saleTaxCollectionView == null)
                _saleTaxCollectionView = CollectionViewSource.GetDefaultView(SaleTaxLocationCollection);
            if (_saleTaxCollectionView.SortDescriptions.Count() == 0)
                _saleTaxCollectionView.SortDescriptions.Add(new SortDescription("SortIndex", ListSortDirection.Ascending));
            _saleTaxCollectionView.Refresh();
        }

        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute())
                    {
                        //if (SaveCustomer())
                        //result =
                        OnSaveCommandExecute();
                    }
                    else //Has Error
                        result = false;
                }
                else
                {
                    RollBackSaleTaxLocation();
                }
            }

            return result;
        }

        /// <summary>
        /// Roll back data saletax & relation
        /// </summary>
        private void RollBackSaleTaxLocation()
        {
            //Set RollBack tax code if has changed
            if (PrimaryTaxLocation != null && PrimaryTaxLocation.PrimarySaleTaxEdited)
            {
                SetTaxLocationPrimary();
                SetDefaultTaxCode();
                PrimaryTaxLocation.PrimarySaleTaxEdited = false;
            }

            if (!SelectedSaleTaxLocation.IsNew)//Old Item Rollback data
            {
                SelectedSaleTaxLocation.ToModelAndRaise();
                SetDataToModel(SelectedSaleTaxLocation);

            }
            else
            {
                SelectedSaleTaxLocation = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saleTaxLocationModel"></param>
        /// <returns></returns>
        private string GetSortIndex(base_SaleTaxLocationModel saleTaxLocationModel)
        {
            //Set SortIndex
            int parent = saleTaxLocationModel.ParentId;
            string lastSortIndex = string.Empty;
            if (this.SaleTaxLocationCollection.Any(x => x.ParentId == saleTaxLocationModel.ParentId))//Tax Location Has child
            {
                lastSortIndex = this.SaleTaxLocationCollection.LastOrDefault(x => x.ParentId == saleTaxLocationModel.ParentId).SortIndex;
            }
            else if (SaleTaxLocationCollection.Count > 0)//Not Any Item in Database
            {
                lastSortIndex = this.SaleTaxLocationCollection.LastOrDefault(x => x.Id == saleTaxLocationModel.ParentId).SortIndex;
            }

            if (!string.IsNullOrWhiteSpace(lastSortIndex))
            {
                string[] sortArray = lastSortIndex.Split('-');
                int parentIndex = saleTaxLocationModel.ParentId == 0 ? int.Parse(sortArray[0]) + 1 : int.Parse(sortArray[0]);
                int idIndex = saleTaxLocationModel.ParentId == 0 ? 0 : int.Parse(sortArray[1]) + 1;
                return string.Format("{0}-{1}", parentIndex, idIndex);
            }
            else
                return string.Format("{0}-{1}", 1, 0);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// 
        /// </summary>
        public override void LoadData()
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            this.SaleTaxLocationCollection.Clear();

            bgWorker.DoWork += (sender, e) =>
            {
                IsBusy = true;
                //Get data with range
                //NumberOfDisplayItems
                IList<base_SaleTaxLocation> saleTaxtLocations = _saleTaxRespository.GetRange(0, NumberOfDisplayItems, "It.SortIndex");
                foreach (base_SaleTaxLocation saleTax in saleTaxtLocations)
                {
                    bgWorker.ReportProgress(0, saleTax);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_SaleTaxLocationModel saleTaxtLocationModel = new base_SaleTaxLocationModel((base_SaleTaxLocation)e.UserState);
                SetDataToModel(saleTaxtLocationModel);
                this.SaleTaxLocationCollection.Add(saleTaxtLocationModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (this.SaleTaxLocationCollection.Count == 0)
                    InsertDefaultTaxLocation();

                SortSaleTaxCollection();
                GetDefaultTaxLocationTaxCodeCollection();

                //Primary TaxLocation
                SetTaxLocationPrimary();
                SetDefaultTaxCode();
                if (_selectedId > 0)
                {
                    SelectedSaleTaxLocation = this.SaleTaxLocationCollection.FirstOrDefault(x => x.Id == _selectedId);
                    SelectedSaleTaxLocation.IsSelected = true;
                }
                else
                    SelectedSaleTaxLocation = this.SaleTaxLocationCollection.First();
                SelectedSaleTaxLocation.IsDirty = false;
                PrimaryTaxLocation.PrimarySaleTaxEdited = false;
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (!isClosing && SelectedSaleTaxLocation!=null)
                _selectedId = SelectedSaleTaxLocation.Id;
            return ChangeViewExecute(isClosing);
        }

        
        #endregion
    }
}
