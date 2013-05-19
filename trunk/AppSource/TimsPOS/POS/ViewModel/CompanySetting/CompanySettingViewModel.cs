using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using System.Collections.ObjectModel;
using CPC.POS.Model;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using CPC.POS.Repository;
using CPC.POS.Database;
using System.Collections.Specialized;
using System.ComponentModel;
using CPC.Service.FrameworkDialogs.OpenFile;
using System.IO;
using System.Windows.Data;
using System.Reflection;
using CPC.Helper;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.Primitives;
using SecurityLib;


namespace CPC.POS.ViewModel
{
    public class CompanySettingViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Determine whether this object is loading data.
        /// </summary>
        private bool _isLoading = false;

        /// <summary>
        /// Holds current SettingParts (General).
        /// </summary>
        private SettingParts _currentSettingPart = SettingParts.General;

        /// <summary>
        /// Holds selected ItemSettingModel before (null).
        /// </summary>
        private ItemSettingModel _selectedItemSettingBefore = null;


        /// <summary>
        /// Used for Filter property of OpenFileDialogViewModel.
        /// (Image Files |*.jpg; *.jpeg; *.bmp; *.gif; *.png; *.tif)
        /// </summary>
        private readonly string _filter = "Image Files |*.jpg; *.jpeg; *.bmp; *.gif; *.png; *.tif";

        /// <summary>
        /// Price schemas key (PriceSchemas).
        /// </summary>
        private readonly string _priceSchemasKey = "PriceSchemas";

        /// <summary>
        /// Name of combo element (combo).
        /// </summary>
        private readonly string _comboElementName = "combo";

        /// <summary>
        /// Name of value element (value).
        /// </summary>
        private readonly string _valueElementName = "value";

        /// <summary>
        /// Name of markDown element (markDown).
        /// </summary>
        private readonly string _markDownElementName = "markDown";

        /// <summary>
        /// Name of currency element (currency).
        /// </summary>
        private readonly string _currencyElementName = "currency";

        /// <summary>
        /// Name of name element (name).
        /// </summary>
        private readonly string _nameElementName = "name";

        /// <summary>
        /// Key attribute name used for detect combo element (key).
        /// </summary>
        private readonly string _keyAttributeComboElement = "key";

        /// <summary>
        /// Default store's name (Store).
        /// </summary>
        private readonly string _defaultStoreName = "Store";

        /// <summary>
        /// Default store's code (1).
        /// </summary>
        private readonly string _defaultStoreCode = "1";

        /// <summary>
        /// Default total store (5).
        /// </summary>
        private readonly short _defaultTotalStore = 5;

        /// <summary>
        /// Default price schema (1).
        /// </summary>
        private readonly short _defaultPriceSchema = 1;

        #endregion

        #region Constructors

        public CompanySettingViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            _priceCollection = new CollectionBase<PriceModel>
            {
                new PriceModel(),
                new PriceModel(),
                new PriceModel(),
                new PriceModel()
            };
            _priceCollection.DeletedItems.Add(new PriceModel());
        }

        #endregion

        #region Properties

        #region ItemSettings

        private CollectionBase<ItemSettingModel> _itemSettings;
        /// <summary>
        /// Gets or sets ItemSetting collection. This property bindings TreeView.
        /// </summary>
        public CollectionBase<ItemSettingModel> ItemSettings
        {
            get
            {
                return _itemSettings;
            }
            set
            {
                if (_itemSettings != value)
                {
                    _itemSettings = value;
                    OnPropertyChanged(() => ItemSettings);
                }
            }
        }

        #endregion

        #region SelectedItemSetting

        private ItemSettingModel _selectedItemSetting;
        /// <summary>
        /// Gets or sets selected ItemSettingModel.
        /// </summary>
        public ItemSettingModel SelectedItemSetting
        {
            get
            {
                return _selectedItemSetting;
            }
            set
            {
                if (_selectedItemSetting != value)
                {
                    bool allowChangeItemSetting = true;
                    if (value != null)
                    {
                        allowChangeItemSetting = OnSelectedItemSettingChanging();
                    }
                    if (allowChangeItemSetting)
                    {
                        _selectedItemSetting = value;
                        OnPropertyChanged(() => SelectedItemSetting);
                        OnSelectedItemSettingChanged();
                    }
                }
            }
        }

        #endregion

        #region ConfigurationModel

        private base_ConfigurationModel _configurationModel;
        /// <summary>
        /// Gets or sets ConfigurationModel.
        /// </summary>
        public base_ConfigurationModel ConfigurationModel
        {
            get
            {
                return _configurationModel;
            }
            set
            {
                if (_configurationModel != value)
                {
                    _configurationModel = value;
                    OnPropertyChanged(() => ConfigurationModel);
                }
            }
        }

        #endregion

        #region UOMCollection

        private CollectionBase<base_UOMModel> _UOMCollection;
        /// <summary>
        /// Gets or sets unit of measure collection.
        /// </summary>
        public CollectionBase<base_UOMModel> UOMCollection
        {
            get
            {
                return _UOMCollection;
            }
            set
            {
                if (_UOMCollection != value)
                {
                    _UOMCollection = value;
                    OnPropertyChanged(() => UOMCollection);
                }
            }
        }

        #endregion

        #region NumberList

        private ObservableCollection<int> _numberList;
        /// <summary>
        /// Gets or sets number of store list used for select.
        /// </summary>
        public ObservableCollection<int> NumberList
        {
            get
            {
                return _numberList;
            }
            set
            {
                if (_numberList != value)
                {
                    _numberList = value;
                    OnPropertyChanged(() => NumberList);
                }
            }
        }

        #endregion

        #region SelectedNumber

        private int _selectedNumber;
        /// <summary>
        /// Gets or sets selected number of store.
        /// </summary>
        public int SelectedNumber
        {
            get
            {
                return _selectedNumber;
            }
            set
            {
                if (_selectedNumber != value)
                {
                    _selectedNumber = value;
                    OnPropertyChanged(() => SelectedNumber);
                    OnSelectedNumberChanged();
                }
            }
        }

        #endregion

        #region StoreCollection

        private CollectionBase<base_StoreModel> _storeCollection;
        /// <summary>
        /// Gets or sets store collection.
        /// </summary>
        public CollectionBase<base_StoreModel> StoreCollection
        {
            get
            {
                return _storeCollection;
            }
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

        #region PriceSchemaCollection

        private CollectionBase<PriceModel> _priceSchemaCollection;
        /// <summary>
        /// Gets or sets price collection.
        /// </summary>
        public CollectionBase<PriceModel> PriceSchemaCollection
        {
            get
            {
                return _priceSchemaCollection;
            }
            set
            {
                if (_priceSchemaCollection != value)
                {
                    _priceSchemaCollection = value;
                    OnPropertyChanged(() => PriceSchemaCollection);
                }
            }
        }

        #endregion

        #region PriceCollection

        private CollectionBase<PriceModel> _priceCollection;
        /// <summary>
        /// Gets or sets price collection.
        /// </summary>
        public CollectionBase<PriceModel> PriceCollection
        {
            get
            {
                return _priceCollection;
            }
            set
            {
                if (_priceCollection != value)
                {
                    _priceCollection = value;
                    OnPropertyChanged(() => PriceCollection);
                }
            }
        }

        #endregion

        #region PaymentMethodCollection

        private ObservableCollection<ComboItem> _paymentMethodCollection;
        /// <summary>
        /// Gets or sets payment methods.
        /// </summary>
        public ObservableCollection<ComboItem> PaymentMethodCollection
        {
            get
            {
                return _paymentMethodCollection;
            }
            set
            {
                if (_paymentMethodCollection != value)
                {
                    _paymentMethodCollection = value;
                    OnPropertyChanged(() => PaymentMethodCollection);
                }
            }
        }

        #endregion

        #region Visibility Properties

        #region GridGeneralVisibility

        private Visibility _gridGeneralVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'General' Grid.
        /// </summary>
        public Visibility GridGeneralVisibility
        {
            get
            {
                return _gridGeneralVisibility;
            }
            set
            {
                if (_gridGeneralVisibility != value)
                {
                    _gridGeneralVisibility = value;
                    OnPropertyChanged(() => GridGeneralVisibility);
                }
            }
        }

        #endregion

        #region GridStoreInfoVisibility

        private Visibility _gridStoreInfoVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Store Info' Grid.
        /// </summary>
        public Visibility GridStoreInfoVisibility
        {
            get
            {
                return _gridStoreInfoVisibility;
            }
            set
            {
                if (_gridStoreInfoVisibility != value)
                {
                    _gridStoreInfoVisibility = value;
                    OnPropertyChanged(() => GridStoreInfoVisibility);
                }
            }
        }

        #endregion

        #region GridInventoryVisibility

        private Visibility _gridInventoryVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Inventory' Grid.
        /// </summary>
        public Visibility GridInventoryVisibility
        {
            get
            {
                return _gridInventoryVisibility;
            }
            set
            {
                if (_gridInventoryVisibility != value)
                {
                    _gridInventoryVisibility = value;
                    OnPropertyChanged(() => GridInventoryVisibility);
                }
            }
        }

        #endregion

        #region GridUnitOfMeasureVisibility

        private Visibility _gridUnitOfMeasureVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Unit Of Measure' Grid.
        /// </summary>
        public Visibility GridUnitOfMeasureVisibility
        {
            get
            {
                return _gridUnitOfMeasureVisibility;
            }
            set
            {
                if (_gridUnitOfMeasureVisibility != value)
                {
                    _gridUnitOfMeasureVisibility = value;
                    OnPropertyChanged(() => GridUnitOfMeasureVisibility);
                }
            }
        }

        #endregion

        #region GridEmailSetupVisibility

        private Visibility _gridEmailSetupVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Email Setup' Grid.
        /// </summary>
        public Visibility GridEmailSetupVisibility
        {
            get
            {
                return _gridEmailSetupVisibility;
            }
            set
            {
                if (_gridEmailSetupVisibility != value)
                {
                    _gridEmailSetupVisibility = value;
                    OnPropertyChanged(() => GridEmailSetupVisibility);
                }
            }
        }

        #endregion

        #region GridMutliStoreVisibility

        private Visibility _gridMutliStoreVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Mutli Store' Grid.
        /// </summary>
        public Visibility GridMutliStoreVisibility
        {
            get
            {
                return _gridMutliStoreVisibility;
            }
            set
            {
                if (_gridMutliStoreVisibility != value)
                {
                    _gridMutliStoreVisibility = value;
                    OnPropertyChanged(() => GridMutliStoreVisibility);
                }
            }
        }

        #endregion

        #region GridStoreCodeVisibility

        private Visibility _gridStoreCodeVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Store Code' Grid.
        /// </summary>
        public Visibility GridStoreCodeVisibility
        {
            get
            {
                return _gridStoreCodeVisibility;
            }
            set
            {
                if (_gridStoreCodeVisibility != value)
                {
                    _gridStoreCodeVisibility = value;
                    OnPropertyChanged(() => GridStoreCodeVisibility);
                }
            }
        }

        #endregion

        #region GridPricingVisibility

        private Visibility _gridPricingVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Pricing' Grid.
        /// </summary>
        public Visibility GridPricingVisibility
        {
            get
            {
                return _gridPricingVisibility;
            }
            set
            {
                if (_gridPricingVisibility != value)
                {
                    _gridPricingVisibility = value;
                    OnPropertyChanged(() => GridPricingVisibility);
                }
            }
        }

        #endregion

        #region GridSalesVisibility

        private Visibility _gridSalesVisibility = Visibility.Collapsed;
        /// <summary>
        /// Show or collapsed 'Sales' Grid.
        /// </summary>
        public Visibility GridSalesVisibility
        {
            get
            {
                return _gridSalesVisibility;
            }
            set
            {
                if (_gridSalesVisibility != value)
                {
                    _gridSalesVisibility = value;
                    OnPropertyChanged(() => GridSalesVisibility);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Command Properties

        #region SelectedItemChangedCommand

        private ICommand _selectedItemChangedCommand;
        /// <summary>
        /// When event SelectedItemChanged on TreeView occurs, SelectedItemChangedCommand will executes.
        /// </summary>
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                {
                    _selectedItemChangedCommand = new RelayCommand<TreeView>(SelectedItemChangedCommandExecute);
                }
                return _selectedItemChangedCommand;
            }
        }

        #endregion

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' Button clicked, SaveCommand will executes.
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
        /// When 'Cancel' Button clicked, CancelCommand will executes.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute, CanCancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #region DeleteUOMCommand

        private ICommand _deleteUOMCommand;
        /// <summary>
        /// When 'Delete' key pressed while DataGrid focused, DeleteUOMCommand will executes.
        /// </summary>
        public ICommand DeleteUOMCommand
        {
            get
            {
                if (_deleteUOMCommand == null)
                {
                    _deleteUOMCommand = new RelayCommand<DataGrid>(DeleteUOMExecute);
                }
                return _deleteUOMCommand;
            }
        }

        #endregion

        #region ChangeLogoCommand

        private ICommand _changeLogoCommand;
        /// <summary>
        /// When 'Browser' Button clicked, ChangeLogoCommand will executes.
        /// </summary>
        public ICommand ChangeLogoCommand
        {
            get
            {
                if (_changeLogoCommand == null)
                {
                    _changeLogoCommand = new RelayCommand(ChangeLogoExecute, CanChangeLogoExecute);
                }
                return _changeLogoCommand;
            }
        }

        #endregion

        #region ClearLogoCommand

        private ICommand _clearLogoCommand;
        /// <summary>
        /// When 'Clear' Button clicked, ClearLogoCommand will executes.
        /// </summary>
        public ICommand ClearLogoCommand
        {
            get
            {
                if (_clearLogoCommand == null)
                {
                    _clearLogoCommand = new RelayCommand(ClearLogoExecute, CanClearLogoExecute);
                }
                return _clearLogoCommand;
            }
        }

        #endregion

        #region ItemSelectionChangedCommand

        private ICommand _itemSelectionChangedCommand;
        /// <summary>
        /// When event ItemSelectionChanged on CheckBoxList occurs, ItemSelectionChangedCommand will executes.
        /// </summary>
        public ICommand ItemSelectionChangedCommand
        {
            get
            {
                if (_itemSelectionChangedCommand == null)
                {
                    _itemSelectionChangedCommand = new RelayCommand<ItemSelectionChangedEventArgs>(ItemSelectionChangedExecute);
                }
                return _itemSelectionChangedCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SelectedItemChangedCommandExecute

        /// <summary>
        /// Corresponds with event SelectedItemChanged on TreeView.
        /// </summary>
        private void SelectedItemChangedCommandExecute(TreeView treeView)
        {
            // Gets SelectedItem on TreeView.
            SelectedItemSetting = treeView.SelectedItem as ItemSettingModel;
        }

        #endregion

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
        /// Determine whether can call SaveExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanSaveExecute()
        {
            switch (_currentSettingPart)
            {
                #region General

                case SettingParts.General:
                    return false;

                #endregion

                #region StoreInfo

                case SettingParts.StoreInfo:

                    if (_configurationModel == null || !_configurationModel.IsStoreInfoDirty || _configurationModel.HasError)
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Inventory

                case SettingParts.Inventory:
                    return false;

                #endregion

                #region UnitOfMeasure

                case SettingParts.UnitOfMeasure:

                    if (_configurationModel == null || _UOMCollection == null ||
                        (!_configurationModel.IsUnitOfMeasureDirty && !_UOMCollection.IsDirty) ||
                        _UOMCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Email

                case SettingParts.Email:

                    if (_configurationModel == null || !_configurationModel.IsEmailSetupDirty || _configurationModel.HasError)
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region StoreCodes

                case SettingParts.StoreCodes:

                    if (_storeCollection == null || !_storeCollection.IsDirty ||
                        _storeCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Pricing

                case SettingParts.Pricing:

                    if (_configurationModel == null || _priceCollection == null ||
                        (!_configurationModel.IsPricingDirty && !_priceCollection.Any(x => x.IsDirty) && !_priceCollection.DeletedItems.Any(x => x.IsDirty)))
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Sales

                case SettingParts.Sales:

                    if (_configurationModel == null || !_configurationModel.IsSalesDirty)
                    {
                        return false;
                    }

                    return true;

                #endregion

                default:
                    return false;
            }

        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #region CanCancelExecute

        /// <summary>
        /// Determine whether can call CancelExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanCancelExecute()
        {
            switch (_currentSettingPart)
            {
                #region General

                case SettingParts.General:
                    return false;

                #endregion

                #region StoreInfo

                case SettingParts.StoreInfo:

                    if (_configurationModel == null || !_configurationModel.IsStoreInfoDirty)
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Inventory

                case SettingParts.Inventory:
                    return false;

                #endregion

                #region UnitOfMeasure

                case SettingParts.UnitOfMeasure:

                    if (_configurationModel == null || _UOMCollection == null ||
                        (!_configurationModel.IsUnitOfMeasureDirty && !_UOMCollection.IsDirty))
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Email

                case SettingParts.Email:

                    if (_configurationModel == null || !_configurationModel.IsEmailSetupDirty)
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region StoreCodes

                case SettingParts.StoreCodes:

                    if (_storeCollection == null || !_storeCollection.IsDirty)
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Pricing

                case SettingParts.Pricing:

                    if (_configurationModel == null || _priceCollection == null ||
                        (!_configurationModel.IsPricingDirty && !_priceCollection.Any(x => x.IsDirty) && !_priceCollection.DeletedItems.Any(x => x.IsDirty)))
                    {
                        return false;
                    }

                    return true;

                #endregion

                #region Sales

                case SettingParts.Sales:

                    if (_configurationModel == null || !_configurationModel.IsSalesDirty)
                    {
                        return false;
                    }

                    return true;

                #endregion

                default:
                    return false;
            }
        }

        #endregion

        #region DeleteUOMExecute

        /// <summary>
        /// Delete unit of measures.
        /// </summary>
        /// <param name="dataGrid">DataGrid contains items will delete.</param>
        private void DeleteUOMExecute(DataGrid dataGrid)
        {
            DeleteUOMs(dataGrid);
        }

        #endregion

        #region ChangeLogoExecute

        /// <summary>
        /// Change company's logo.
        /// </summary>
        private void ChangeLogoExecute()
        {
            ChangeLogo();
        }

        #endregion

        #region CanChangeLogoExecute

        /// <summary>
        /// Determine whether can call ChangeLogoExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanChangeLogoExecute()
        {
            if (_configurationModel == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ClearLogoExecute

        /// <summary>
        /// Clear company's logo.
        /// </summary>
        private void ClearLogoExecute()
        {
            ClearLogo();
        }

        #endregion

        #region CanClearLogoExecute

        /// <summary>
        /// Determine whether can call ClearLogoExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanClearLogoExecute()
        {
            if (_configurationModel == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ItemSelectionChangedExecute

        /// <summary>
        /// Corresponds with event ItemSelectionChanged on CheckBoxList.
        /// </summary>
        private void ItemSelectionChangedExecute(ItemSelectionChangedEventArgs e)
        {
            AddRemovePaymentMethod(e.Item as ComboItem, e.IsSelected);
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnSelectedItemSettingChanging

        /// <summary>
        /// Occur before SelectedItemSetting property changed.
        /// </summary>
        private bool OnSelectedItemSettingChanging()
        {
            // Holds selected ItemSettingModel before.
            _selectedItemSettingBefore = _selectedItemSetting;

            bool allowChangeItemSetting = true;

            // ignore when this object is loading data.
            if (!_isLoading)
            {
                // Check and question.
                allowChangeItemSetting = SaveWithQuestion();
                if (_selectedItemSettingBefore != null && !allowChangeItemSetting)
                {
                    // Select old item.
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_selectedItemSettingBefore != null)
                        {
                            _selectedItemSettingBefore.IsSelected = true;
                        }
                    }));
                }
            }

            return allowChangeItemSetting;
        }

        #endregion

        #region OnSelectedItemSettingChanged

        /// <summary>
        /// Occur after SelectedItemSetting property changed.
        /// </summary>
        private void OnSelectedItemSettingChanged()
        {
            if (_selectedItemSetting == null)
            {
                return;
            }

            // Gets setting part.
            _currentSettingPart = _selectedItemSetting.SettingParts;
            if (_configurationModel != null)
            {
                _configurationModel.SettingParts = _currentSettingPart;
            }

            // Arrange user interface based on current setting part.
            ArrangeUI();
        }

        #endregion

        #region OnSelectedNumberChanged

        /// <summary>
        ///  Occur after SelectedNumber property changed.
        /// </summary>
        private void OnSelectedNumberChanged()
        {
            if (_selectedNumber == _storeCollection.Count)
            {
                return;
            }

            if (_selectedNumber > _storeCollection.Count)
            {
                int addCount = _selectedNumber - _storeCollection.Count;
                while (addCount > 0)
                {
                    // Find unique code.
                    int i = 0;
                    bool isExist = true;
                    do
                    {
                        i++;
                        isExist = _storeCollection.FirstOrDefault(x => x.Code.Trim() == (i).ToString()) != null;
                    }
                    while (isExist);

                    // Add store with unique code.
                    _storeCollection.Add(new base_StoreModel()
                    {
                        Code = i.ToString(),
                        Name = string.Format("{0} {1}", _defaultStoreName, i)
                    });
                    addCount--;
                }
            }
            else
            {
                int removeCount = _storeCollection.Count - _selectedNumber;
                while (removeCount > 0)
                {
                    _storeCollection.RemoveAt(_storeCollection.Count - 1);
                    removeCount--;
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            // Holds old selected item setting. Because when initialize ItemSettings collection,
            // old selected item setting is null.
            ItemSettingModel itemSetting = _selectedItemSetting;

            GetConfigurationItem();

            CreateItemSettings();

            GetUOMCollection();

            InitStoreCodesData();

            GetPriceCollection();

            InitPaymentMethodCollection();

            SelectDefaultItemSetting(itemSetting);
        }

        #endregion

        #region GetConfigurationItem

        /// <summary>
        /// Gets Configuration item.
        /// </summary>
        private void GetConfigurationItem()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_ConfigurationRepository configurationRepository = new base_ConfigurationRepository();

                    // Try get configuration from database.
                    base_Configuration configuration = configurationRepository.Get();

                    // Not have, add configuration default to database.
                    short valueAlwaysPaymentMethod = Common.PaymentMethods.FirstOrDefault(x => x.IntValue == Define.AlwaysPaymentMethod).Value;

                    if (configuration == null)
                    {
                        configuration = new base_Configuration()
                        {
                            DefaultPriceSchema = _defaultPriceSchema,
                            TotalStore = _defaultTotalStore,
                            IsAllowCollectTipCreditCard = false,
                            AcceptedPaymentMethod = valueAlwaysPaymentMethod,
                            DefaultPaymentMethod = valueAlwaysPaymentMethod
                        };
                        configurationRepository.Add(configuration);
                        configurationRepository.Commit();
                    }
                    else
                    {
                        configurationRepository.Refresh(configuration);

                        if (!configuration.TotalStore.HasValue || configuration.TotalStore == 0)
                        {
                            configuration.TotalStore = _defaultTotalStore;
                        }
                        if (!configuration.DefaultPriceSchema.HasValue || configuration.DefaultPriceSchema == 0)
                        {
                            configuration.DefaultPriceSchema = _defaultPriceSchema;
                        }
                        if (!configuration.IsAllowCollectTipCreditCard.HasValue)
                        {
                            configuration.IsAllowCollectTipCreditCard = false;
                        }
                        if (!configuration.AcceptedPaymentMethod.HasValue)
                        {
                            configuration.AcceptedPaymentMethod = valueAlwaysPaymentMethod;
                        }
                        else
                        {
                            if ((configuration.AcceptedPaymentMethod.Value & valueAlwaysPaymentMethod) != valueAlwaysPaymentMethod)
                            {
                                configuration.AcceptedPaymentMethod += valueAlwaysPaymentMethod;
                            }
                        }
                        if (!configuration.DefaultPaymentMethod.HasValue || configuration.DefaultPaymentMethod == 0)
                        {
                            configuration.DefaultPaymentMethod = valueAlwaysPaymentMethod;
                        }

                        if (configuration.EntityState == System.Data.EntityState.Modified)
                        {
                            configurationRepository.Commit();
                        }
                    }

                    // Init ConfigurationModel.
                    ConfigurationModel = new base_ConfigurationModel(configuration);

                    if (!_configurationModel.CountryId.HasValue)
                    {
                        _configurationModel.CountryId = 0;
                    }
                    if (!_configurationModel.State.HasValue)
                    {
                        _configurationModel.State = 0;
                    }
                    if (!string.IsNullOrWhiteSpace(configuration.EmailPassword))
                    {
                        _configurationModel.EmailPassword = Define.PasswordTemp;
                        _configurationModel.RetypeEmailPassword = Define.PasswordTemp;
                    }
                    _configurationModel.SettingParts = _currentSettingPart;

                    _configurationModel.IsDirty = false;
                    _configurationModel.IsStoreInfoDirty = false;
                    _configurationModel.IsUnitOfMeasureDirty = false;
                    _configurationModel.IsPricingDirty = false;
                    _configurationModel.IsSalesDirty = false;
                    _configurationModel.IsEmailSetupDirty = false;

                    // Update application.
                    Define.CONFIGURATION = _configurationModel.ShallowClone();

                    ConfigurationModel.PropertyChanged += ConfigurationPropertyChanged;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region CreateItemSettings

        /// <summary>
        /// Create ItemSettings.
        /// </summary>
        private void CreateItemSettings()
        {
            int id = 1;
            ItemSettings = new CollectionBase<ItemSettingModel>();

            ItemSettingModel itemSetting;

            itemSetting = new ItemSettingModel(SettingParts.General, id++, "General")
            {
                Childs = new CollectionBase<ItemSettingModel>(),
            };
            itemSetting.Childs.Add(new ItemSettingModel(SettingParts.StoreInfo, id++, "Store Information")
            {
                Parent = itemSetting
            });
            _itemSettings.Add(itemSetting);

            itemSetting = new ItemSettingModel(SettingParts.Inventory, id++, "Inventory")
            {
                Childs = new CollectionBase<ItemSettingModel>()
            };
            itemSetting.Childs.Add(new ItemSettingModel(SettingParts.UnitOfMeasure, id++, "Unit Of Measure")
            {
                Parent = itemSetting
            });
            _itemSettings.Add(itemSetting);

            itemSetting = new ItemSettingModel(SettingParts.Email, id++, "Email Setup");
            _itemSettings.Add(itemSetting);

            itemSetting = new ItemSettingModel(SettingParts.MultiStore, id++, "Multi-Store")
            {
                Childs = new CollectionBase<ItemSettingModel>(),
            };
            itemSetting.Childs.Add(new ItemSettingModel(SettingParts.StoreCodes, id++, "Store Codes")
            {
                Parent = itemSetting
            });
            _itemSettings.Add(itemSetting);

            itemSetting = new ItemSettingModel(SettingParts.Pricing, id++, "Pricing");
            _itemSettings.Add(itemSetting);

            itemSetting = new ItemSettingModel(SettingParts.Sales, id++, "Sales")
            {
                Childs = new CollectionBase<ItemSettingModel>(),
            };
            _itemSettings.Add(itemSetting);
        }

        #endregion

        #region GetUOMCollection

        /// <summary>
        /// Gets unit of measure collection.
        /// </summary>
        private void GetUOMCollection()
        {
            try
            {
                lock (UnitOfWork.Locker)
                {
                    base_UOMRepository UOMRepository = new base_UOMRepository();
                    UOMCollection = new CollectionBase<base_UOMModel>(UOMRepository.GetAll(x => x.IsActived).Select(x => new base_UOMModel(x)));
                    UOMCollection.CollectionChanged += UOMCollectionChanged;
                }
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region GetStoreCollection

        /// <summary>
        /// Gets store collection.
        /// </summary>
        private void GetStoreCollection()
        {
            base_StoreRepository storeRepository = new base_StoreRepository();

            try
            {
                lock (UnitOfWork.Locker)
                {
                    storeRepository.BeginTransaction();

                    // Get all stores.
                    List<base_Store> stores = storeRepository.GetAll().ToList();

                    if (stores.Count == 0)
                    {
                        // Create one default store.
                        base_Store defaultStore = new base_Store
                        {
                            Name = string.Format("{0} {1}", _defaultStoreName, _defaultStoreCode),
                            Code = _defaultStoreCode
                        };
                        storeRepository.Add(defaultStore);
                        storeRepository.Commit();
                        stores.Add(defaultStore);
                    }
                    else
                    {
                        if (_numberList.Count < stores.Count)
                        {
                            while (_numberList.Count < stores.Count)
                            {
                                storeRepository.Delete(stores[stores.Count - 1]);
                                storeRepository.Commit();
                                stores.RemoveAt(stores.Count - 1);
                            }
                        }
                    }

                    storeRepository.CommitTransaction();

                    StoreCollection = new CollectionBase<base_StoreModel>(stores.Select(x => new base_StoreModel(x)).OrderBy(x => x.Id));
                    _selectedNumber = _storeCollection.Count;
                    OnPropertyChanged(() => SelectedNumber);
                }
            }
            catch (Exception exception)
            {
                storeRepository.RollbackTransaction();
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region InitStoreCodesData

        /// <summary>
        /// Initialize StoreCodes's data.
        /// </summary>
        private void InitStoreCodesData()
        {
            // Create number of store list.
            NumberList = new ObservableCollection<int>();
            for (int i = 1; i <= _configurationModel.TotalStore; i++)
            {
                _numberList.Add(i);
            }

            // Get store list.
            GetStoreCollection();
        }

        #endregion

        #region GetPriceCollection

        /// <summary>
        /// Gets price collection.
        /// </summary>
        private void GetPriceCollection()
        {
            try
            {
                _priceSchemaCollection = new CollectionBase<PriceModel>();

                Stream stream = Common.LoadCurrentLanguagePackage();
                XDocument xDoc = XDocument.Load(stream);
                stream.Close();
                stream.Dispose();

                IEnumerable<XElement> xPrices = xDoc.Root.Elements(_comboElementName).FirstOrDefault(x => x.Attribute(_keyAttributeComboElement).Value == _priceSchemasKey).Elements();
                if (xPrices != null)
                {
                    int index = 0;
                    PriceModel price = null;
                    foreach (var xPrice in xPrices)
                    {
                        short id = Convert.ToInt16(xPrice.Element(_valueElementName).Value);
                        if (id > 0)
                        {
                            if (id == _configurationModel.DefaultPriceSchema)
                            {
                                price = _priceCollection.DeletedItems[0];
                                price.Id = id;
                                price.Name = xPrice.Element(_nameElementName).Value;
                                price.MarkDown = Convert.ToDecimal(xPrice.Element(_markDownElementName).Value);
                                price.Currency = xPrice.Element(_currencyElementName).Value;
                                price.PriceSchemaCollection = _priceSchemaCollection;
                                price.IsNew = false;
                                price.IsDirty = false;
                            }
                            else
                            {
                                price = _priceCollection[index++];
                                price.Id = id;
                                price.Name = xPrice.Element(_nameElementName).Value;
                                price.MarkDown = Convert.ToDecimal(xPrice.Element(_markDownElementName).Value);
                                price.Currency = xPrice.Element(_currencyElementName).Value;
                                price.PriceSchemaCollection = _priceSchemaCollection;
                                price.IsNew = false;
                                price.IsDirty = false;
                            }

                            _priceSchemaCollection.Add(price.ShallowClone());
                        }
                    }

                    // Holds DefaultPriceSchema value because when raise PriceSchemaCollection property changed,
                    // DefaultPriceSchema set to null value.
                    short? defaultPriceSchema = _configurationModel.DefaultPriceSchema;
                    OnPropertyChanged(() => PriceSchemaCollection);
                    _configurationModel.DefaultPriceSchema = defaultPriceSchema;
                    _configurationModel.IsDirty = false;
                    _configurationModel.IsPricingDirty = false;
                }

            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region InitPaymentMethodCollection

        /// <summary>
        /// Initialize PaymentMethodCollection.
        /// </summary>
        private void InitPaymentMethodCollection()
        {
            PaymentMethodCollection = new ObservableCollection<ComboItem>(Common.PaymentMethods.Where(x =>
                (_configurationModel.AcceptedPaymentMethod & x.Value) == x.Value).OrderBy(x => x.Text));
        }

        #endregion

        #region ArrangeUI

        /// <summary>
        /// Arrange user interface.
        /// </summary>
        private void ArrangeUI()
        {
            GridGeneralVisibility = Visibility.Collapsed;
            GridStoreInfoVisibility = Visibility.Collapsed;
            GridInventoryVisibility = Visibility.Collapsed;
            GridUnitOfMeasureVisibility = Visibility.Collapsed;
            GridEmailSetupVisibility = Visibility.Collapsed;
            GridMutliStoreVisibility = Visibility.Collapsed;
            GridStoreCodeVisibility = Visibility.Collapsed;
            GridPricingVisibility = Visibility.Collapsed;
            GridSalesVisibility = Visibility.Collapsed;

            switch (_currentSettingPart)
            {
                case SettingParts.General:

                    GridGeneralVisibility = Visibility.Visible;

                    break;

                case SettingParts.StoreInfo:

                    GridStoreInfoVisibility = Visibility.Visible;

                    break;

                case SettingParts.Inventory:

                    GridInventoryVisibility = Visibility.Visible;

                    break;

                case SettingParts.UnitOfMeasure:

                    GridUnitOfMeasureVisibility = Visibility.Visible;

                    break;

                case SettingParts.Email:

                    GridEmailSetupVisibility = Visibility.Visible;

                    break;

                case SettingParts.MultiStore:

                    GridMutliStoreVisibility = Visibility.Visible;

                    break;

                case SettingParts.StoreCodes:

                    GridStoreCodeVisibility = Visibility.Visible;

                    break;

                case SettingParts.Pricing:

                    GridPricingVisibility = Visibility.Visible;

                    break;

                case SettingParts.Sales:

                    GridSalesVisibility = Visibility.Visible;

                    break;
            }
        }

        #endregion

        #region FindBeforeSelectedItemSetting

        /// <summary>
        /// Find before selected item setting.
        /// </summary>
        /// <param name="id">Id of item setting.</param>
        /// <returns>Selected item setting before.</returns>
        private ItemSettingModel FindBeforeSelectedItemSetting(int id)
        {
            foreach (ItemSettingModel itemSetting in _itemSettings)
            {
                if (itemSetting.Id == id)
                {
                    return itemSetting;
                }

                if (itemSetting.Childs != null)
                {
                    foreach (ItemSettingModel child in itemSetting.Childs)
                    {
                        if (child.Id == id)
                        {
                            return child;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region SelectDefaultItemSetting

        /// <summary>
        /// Select default item setting.
        /// </summary>
        private void SelectDefaultItemSetting(ItemSettingModel itemSetting)
        {
            if (itemSetting == null)
            {
                // Select first item.
                SelectBeforeItemSetting(_itemSettings.FirstOrDefault());
            }
            else
            {
                ItemSettingModel beforeItem = FindBeforeSelectedItemSetting(itemSetting.Id);
                if (beforeItem == null)
                {
                    // Select first item.
                    SelectBeforeItemSetting(_itemSettings.FirstOrDefault());
                }
                else
                {
                    // Select before item.
                    SelectBeforeItemSetting(beforeItem);
                }
            }
        }

        #endregion

        #region SelectBeforeItemSetting

        /// <summary>
        /// Select before selected item setting.
        /// </summary>
        /// <param name="itemSetting">Before selected item setting.</param>
        private void SelectBeforeItemSetting(ItemSettingModel itemSetting)
        {
            if (itemSetting == null)
            {
                return;
            }

            if (itemSetting.Parent != null)
            {
                itemSetting.Parent.IsExpanded = true;
            }
            itemSetting.IsSelected = true;
            // (*) Fix error 'Dispatcher processing has been suspended, but messages are still being processed.'
            // by set SelectedItemSetting = itemSetting.
            SelectedItemSetting = itemSetting;
        }

        #endregion

        #region Save

        /// <summary>
        /// Save
        /// </summary>
        private void Save()
        {
            switch (_currentSettingPart)
            {
                case SettingParts.General:
                    break;

                case SettingParts.StoreInfo:

                    SaveStoreInfoConfiguration();

                    break;

                case SettingParts.Inventory:
                    break;

                case SettingParts.UnitOfMeasure:

                    SaveUOMConfiguration();

                    break;

                case SettingParts.Email:

                    SaveEmailConfiguration();

                    break;

                case SettingParts.StoreCodes:

                    SaveStoreCodesConfiguration();

                    break;

                case SettingParts.Pricing:

                    SavePricingConfiguration();

                    break;

                case SettingParts.Sales:

                    SaveSalesConfiguration();

                    break;
            }
        }

        #endregion

        #region SaveWithQuestion

        /// <summary>
        /// Question before save.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveWithQuestion()
        {
            bool isUnactive = true;

            switch (_currentSettingPart)
            {
                case SettingParts.General:
                    break;

                case SettingParts.StoreInfo:

                    isUnactive = SaveStoreInfoConfigurationWithQuestion();

                    break;

                case SettingParts.Inventory:
                    break;

                case SettingParts.UnitOfMeasure:

                    isUnactive = SaveUOMConfigurationWithQuestion();

                    break;

                case SettingParts.Email:

                    isUnactive = SaveEmailConfigurationWithQuestion();

                    break;

                case SettingParts.StoreCodes:

                    isUnactive = SaveStoreCodesConfigurationWithQuestion();

                    break;

                case SettingParts.Pricing:

                    isUnactive = SavePricingConfigurationWithQuestion();

                    break;

                case SettingParts.Sales:

                    isUnactive = SaveSalesConfigurationWithQuestion();

                    break;
            }

            return isUnactive;
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            switch (_currentSettingPart)
            {
                case SettingParts.General:
                    break;

                case SettingParts.StoreInfo:

                    RestoreStoreInfoConfiguration();

                    break;

                case SettingParts.Inventory:
                    break;

                case SettingParts.UnitOfMeasure:

                    RestoreUOMConfiguration();

                    break;

                case SettingParts.Email:

                    RestoreEmailConfiguration();

                    break;

                case SettingParts.StoreCodes:

                    RestoreStoreCodesConfiguration();

                    break;

                case SettingParts.Pricing:

                    RestorePricingConfiguration();

                    break;

                case SettingParts.Sales:

                    RestoreSalesConfiguration();

                    break;
            }
        }

        #endregion

        #region SaveConfiguration

        /// <summary>
        /// Save configuration.
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                base_ConfigurationRepository configurationRepository = new base_ConfigurationRepository();

                // Determine whether user changes password.
                if (!string.IsNullOrWhiteSpace(_configurationModel.base_Configuration.EmailPassword))
                {
                    if (string.Compare(_configurationModel.EmailPassword, Define.PasswordTemp, false) != 0)
                    {
                        // Has been changed.
                        if (!string.IsNullOrWhiteSpace(_configurationModel.EmailPassword))
                        {
                            _configurationModel.EmailPassword = AESSecurity.Encrypt(_configurationModel.EmailPassword);
                        }
                        else
                        {
                            _configurationModel.EmailPassword = null;
                        }
                    }
                    else
                    {
                        // Not change.
                        _configurationModel.EmailPassword = _configurationModel.base_Configuration.EmailPassword;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_configurationModel.EmailPassword))
                    {
                        _configurationModel.EmailPassword = AESSecurity.Encrypt(_configurationModel.EmailPassword);
                    }
                    else
                    {
                        _configurationModel.EmailPassword = null;
                    }
                }

                _configurationModel.ToEntity();
                configurationRepository.Commit();

                if (!string.IsNullOrWhiteSpace(_configurationModel.base_Configuration.EmailPassword))
                {
                    _configurationModel.EmailPassword = Define.PasswordTemp;
                    _configurationModel.RetypeEmailPassword = Define.PasswordTemp;
                }
                else
                {
                    _configurationModel.EmailPassword = null;
                    _configurationModel.RetypeEmailPassword = null;
                }

                _configurationModel.IsDirty = false;
                _configurationModel.IsStoreInfoDirty = false;
                _configurationModel.IsUnitOfMeasureDirty = false;
                _configurationModel.IsPricingDirty = false;
                _configurationModel.IsSalesDirty = false;
                _configurationModel.IsEmailSetupDirty = false;

                // Update application.
                Define.CONFIGURATION = _configurationModel.ShallowClone();
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region RestoreConfiguration

        /// <summary>
        /// Restore configuration.
        /// </summary>
        private void RestoreConfiguration()
        {
            _configurationModel.Restore();
        }

        #endregion

        #region SaveStoreInfoConfiguration

        /// <summary>
        /// Save StoreInfo configuration.
        /// </summary>
        private void SaveStoreInfoConfiguration()
        {
            SaveConfiguration();
        }

        #endregion

        #region SaveStoreInfoConfigurationWithQuestion

        /// <summary>
        /// Question before save StoreInfo configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveStoreInfoConfigurationWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (!_configurationModel.HasError)
            {
                if (_configurationModel.IsStoreInfoDirty)
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveStoreInfoConfiguration();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        RestoreStoreInfoConfiguration();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    RestoreStoreInfoConfiguration();
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region RestoreStoreInfoConfiguration

        /// <summary>
        /// Restore StoreInfo configuration.
        /// </summary>
        private void RestoreStoreInfoConfiguration()
        {
            RestoreConfiguration();
        }

        #endregion

        #region SaveUOMConfiguration

        /// <summary>
        /// Save UnitOfMeasure configuration.
        /// </summary>
        private void SaveUOMConfiguration()
        {
            base_UOMRepository UOMRepository = new base_UOMRepository();

            try
            {
                // Save Configuration.
                if (_configurationModel.IsUnitOfMeasureDirty)
                {
                    SaveConfiguration();
                }

                // Save UOM collection.
                if (_UOMCollection.IsDirty && !_UOMCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
                {
                    UOMRepository.BeginTransaction();
                    DateTime now = DateTime.Now;

                    // Delete items.
                    ObservableCollection<base_UOMModel> deletedItems = _UOMCollection.DeletedItems;
                    foreach (base_UOMModel UOM in deletedItems)
                    {
                        UOM.IsActived = false;
                        UOM.DateUpdated = now;
                        UOM.ToEntity();
                        UOMRepository.Commit();
                    }

                    // Update items.
                    ObservableCollection<base_UOMModel> dirtyItems = _UOMCollection.DirtyItems;
                    foreach (base_UOMModel UOM in dirtyItems)
                    {
                        if (CheckDupCodeUOM(UOM))
                        {
                            throw new Exception(string.Format("{0} is Duplication. Please try another code.", UOM.Code));
                        }
                        else
                        {
                            UOM.DateUpdated = now;
                            UOM.ToEntity();
                            UOMRepository.Commit();
                        }
                    }

                    // Add items.
                    ObservableCollection<base_UOMModel> newItems = _UOMCollection.NewItems;
                    foreach (base_UOMModel UOM in newItems)
                    {
                        if (CheckDupCodeUOM(UOM))
                        {
                            throw new Exception(string.Format("{0} is Duplication. Please try another code.", UOM.Code));
                        }
                        else
                        {
                            UOM.IsActived = true;
                            UOM.DateCreated = now;
                            UOM.DateUpdated = now;
                            UOM.ToEntity();
                            UOMRepository.Add(UOM.base_UOM);
                            UOMRepository.Commit();
                            UOM.Id = UOM.base_UOM.Id;
                        }
                    }

                    UOMRepository.CommitTransaction();

                    // Refresh.
                    _UOMCollection.DeletedItems.Clear();

                    foreach (base_UOMModel UOM in dirtyItems)
                    {
                        UOM.IsDirty = false;
                    }

                    foreach (base_UOMModel UOM in newItems)
                    {
                        UOM.IsNew = false;
                        UOM.IsDirty = false;
                    }
                }
            }
            catch (Exception exception)
            {
                UOMRepository.RollbackTransaction();
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region SaveUOMConfigurationWithQuestion

        /// <summary>
        /// Question before save UnitOfMeasure configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveUOMConfigurationWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (!_UOMCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
            {
                if (_UOMCollection.IsDirty || _configurationModel.IsUnitOfMeasureDirty)
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveUOMConfiguration();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        RestoreUOMConfiguration();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    RestoreUOMConfiguration();
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region RestoreUOMConfiguration

        /// <summary>
        /// Restore UOM configuration.
        /// </summary>
        private void RestoreUOMConfiguration()
        {
            if (_configurationModel.IsUnitOfMeasureDirty)
            {
                RestoreConfiguration();
            }

            if (_UOMCollection.IsDirty || _UOMCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
            {
                GetUOMCollection();
            }
        }

        #endregion

        #region SaveStoreCodesConfiguration

        /// <summary>
        /// Save StoreCodes configuration.
        /// </summary>
        private void SaveStoreCodesConfiguration()
        {
            base_StoreRepository storeRepository = new base_StoreRepository();

            try
            {
                // Save store collection.
                storeRepository.BeginTransaction();
                DateTime now = DateTime.Now;

                // Delete items.
                ObservableCollection<base_StoreModel> deletedItems = _storeCollection.DeletedItems;
                foreach (base_StoreModel store in deletedItems)
                {
                    storeRepository.Delete(store.base_Store);
                    storeRepository.Commit();
                }

                // Update items.
                ObservableCollection<base_StoreModel> dirtyItems = _storeCollection.DirtyItems;
                foreach (base_StoreModel store in dirtyItems)
                {
                    if (CheckDupCodeStore(store))
                    {
                        throw new Exception(string.Format("{0} is Duplication. Please try another code.", store.Code));
                    }
                    else
                    {
                        store.ToEntity();
                        storeRepository.Commit();
                    }
                }

                // Add items.
                ObservableCollection<base_StoreModel> newItems = _storeCollection.NewItems;
                foreach (base_StoreModel store in newItems)
                {
                    if (CheckDupCodeStore(store))
                    {
                        throw new Exception(string.Format("{0} is Duplication. Please try another code.", store.Code));
                    }
                    else
                    {
                        store.ToEntity();
                        storeRepository.Add(store.base_Store);
                        storeRepository.Commit();
                        store.Id = store.base_Store.Id;
                    }
                }

                storeRepository.CommitTransaction();

                // Refresh.
                _storeCollection.DeletedItems.Clear();

                foreach (base_StoreModel store in dirtyItems)
                {
                    store.IsDirty = false;
                }

                foreach (base_StoreModel store in newItems)
                {
                    store.IsNew = false;
                    store.IsDirty = false;
                }
            }
            catch (Exception exception)
            {
                storeRepository.RollbackTransaction();
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region SaveStoreCodesConfigurationWithQuestion

        /// <summary>
        /// Question before save StoreCodes configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveStoreCodesConfigurationWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (!_storeCollection.Any(x => !string.IsNullOrWhiteSpace(x.Error)))
            {
                if (_storeCollection.IsDirty)
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveStoreCodesConfiguration();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        RestoreStoreCodesConfiguration();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    RestoreStoreCodesConfiguration();
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region RestoreStoresCodeConfiguration

        /// <summary>
        /// Restore StoreCodes configuration.
        /// </summary>
        private void RestoreStoreCodesConfiguration()
        {
            GetStoreCollection();
        }

        #endregion

        #region SavePricingConfiguration

        /// <summary>
        /// Save Pricing configuration.
        /// </summary>
        private void SavePricingConfiguration()
        {
            try
            {
                // Save Configuration.
                if (_configurationModel.IsPricingDirty)
                {
                    SaveConfiguration();
                }

                // Save price collection.
                if (_priceCollection.Any(x => x.IsDirty) || _priceCollection.DeletedItems.Any(x => x.IsDirty))
                {
                    // Gets dirty items.
                    ObservableCollection<PriceModel> dirtyItems = new ObservableCollection<PriceModel>(_priceCollection.Where(x => x.IsDirty));
                    if (_priceCollection.DeletedItems.Any(x => x.IsDirty))
                    {
                        dirtyItems.Add(_priceCollection.DeletedItems.First());
                    }

                    // Load XML file.
                    Stream stream = Common.LoadCurrentLanguagePackage();
                    // Get file path.
                    string fileLanguage = (stream as FileStream).Name;
                    XDocument xDoc = XDocument.Load(stream);
                    stream.Close();
                    stream.Dispose();

                    // Get Prices in xml file.
                    IEnumerable<XElement> xPrices = xDoc.Root.Elements(_comboElementName).FirstOrDefault(x => x.Attribute(_keyAttributeComboElement).Value == _priceSchemasKey).Elements();
                    if (xPrices != null)
                    {
                        XElement xElement = null;
                        foreach (var dirtyItem in dirtyItems)
                        {
                            xElement = xPrices.FirstOrDefault(x => Convert.ToInt16(x.Element(_valueElementName).Value) == dirtyItem.Id);
                            if (xElement != null)
                            {
                                xElement.Element(_nameElementName).Value = dirtyItem.Name;
                                xElement.Element(_markDownElementName).Value = dirtyItem.MarkDown.ToString();
                            }
                        }

                        xDoc.Save(fileLanguage);
                    }

                    foreach (var dirtyItem in dirtyItems)
                    {
                        dirtyItem.IsDirty = false;
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

        #region SavePricingConfigurationWithQuestion

        /// <summary>
        /// Question before save Pricing configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SavePricingConfigurationWithQuestion()
        {
            bool isUnactive = true;

            if (_priceCollection.Any(x => x.IsDirty) ||
                _priceCollection.DeletedItems.Any(x => x.IsDirty) ||
                _configurationModel.IsPricingDirty)
            {
                // Question save.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Save.
                    SavePricingConfiguration();
                    isUnactive = true;
                }
                else
                {
                    // Not Save.
                    RestorePricingConfiguration();
                    isUnactive = true;
                }
            }
            else
            {
                // Item not edit.
                isUnactive = true;
            }

            return isUnactive;
        }

        #endregion

        #region RestorePricingConfiguration

        /// <summary>
        /// Restore Pricing configuration.
        /// </summary>
        private void RestorePricingConfiguration()
        {
            if (_configurationModel.IsPricingDirty)
            {
                RestoreConfiguration();
            }

            if (_priceCollection.Any(x => x.IsDirty) || _priceCollection.DeletedItems.Any(x => x.IsDirty))
            {
                GetPriceCollection();
            }
        }

        #endregion

        #region SaveSalesConfiguration

        /// <summary>
        /// Save Sales configuration.
        /// </summary>
        private void SaveSalesConfiguration()
        {
            SaveConfiguration();
        }

        #endregion

        #region SaveSalesConfigurationWithQuestion

        /// <summary>
        /// Question before save Sales configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveSalesConfigurationWithQuestion()
        {
            bool isUnactive = true;

            if (_configurationModel.IsSalesDirty)
            {
                // Question save.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Save.
                    SaveSalesConfiguration();
                    isUnactive = true;
                }
                else
                {
                    // Not Save.
                    RestoreSalesConfiguration();
                    isUnactive = true;
                }
            }
            else
            {
                // Item not edit.
                isUnactive = true;
            }

            return isUnactive;
        }

        #endregion

        #region RestoreSalesConfiguration

        /// <summary>
        /// Restore Sales configuration.
        /// </summary>
        private void RestoreSalesConfiguration()
        {
            RestoreConfiguration();
        }

        #endregion

        #region SaveEmailConfiguration

        /// <summary>
        /// Save Email configuration.
        /// </summary>
        private void SaveEmailConfiguration()
        {
            SaveConfiguration();
        }

        #endregion

        #region SaveEmailConfigurationWithQuestion

        /// <summary>
        /// Question before save Email configuration.
        /// </summary>
        /// <returns>True is save. False is not save.</returns>
        private bool SaveEmailConfigurationWithQuestion()
        {
            bool isUnactive = true;

            // No errors.
            if (!_configurationModel.HasError)
            {
                if (_configurationModel.IsEmailSetupDirty)
                {
                    // Question save.
                    MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Save.
                        SaveEmailConfiguration();
                        isUnactive = true;
                    }
                    else
                    {
                        // Not Save.
                        RestoreEmailConfiguration();
                        isUnactive = true;
                    }
                }
                else
                {
                    // Item not edit.
                    isUnactive = true;
                }

            }
            else // Errors.
            {
                // Quention continue.
                MessageBoxResult result = MessageBox.Show("Some data has been changed. Do you want to save?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Continue work.
                    isUnactive = false;
                }
                else
                {
                    // Not continue work.
                    RestoreEmailConfiguration();
                    isUnactive = true;
                }
            }

            return isUnactive;
        }

        #endregion

        #region RestoreEmailConfiguration

        /// <summary>
        /// Restore Email configuration.
        /// </summary>
        private void RestoreEmailConfiguration()
        {
            RestoreConfiguration();
        }

        #endregion

        #region DeleteUOMs

        /// <summary>
        /// Delete unit of measures.
        /// </summary>
        /// <param name="dataGrid">DataGrid contains items will delete.</param>
        private void DeleteUOMs(DataGrid dataGrid)
        {
            // Get selected UOMs.
            List<base_UOMModel> selectedUOMs = new List<base_UOMModel>((dataGrid.SelectedItems as ObservableCollection<object>).Where(x =>
                x.GetType() == typeof(base_UOMModel)).Cast<base_UOMModel>().Where(x => !x.IsTemporary));

            if (selectedUOMs.Count == 0)
            {
                return;
            }

            // Try to find error UOM.
            base_UOMModel UOMError = _UOMCollection.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Error));
            bool isContainsErrorItem = false;
            if (UOMError != null)
            {
                isContainsErrorItem = selectedUOMs.Contains(UOMError);
            }

            if (UOMError == null || isContainsErrorItem)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete item(s)?", "Delete item(s)", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var UOM in selectedUOMs)
                    {
                        _UOMCollection.Remove(UOM);
                    }
                }
            }
        }

        #endregion

        #region ChangeLogo

        /// <summary>
        /// Change company's logo.
        /// </summary>
        private void ChangeLogo()
        {
            OpenFileDialogViewModel openFileDialogViewModel = new OpenFileDialogViewModel();
            openFileDialogViewModel.Multiselect = false;
            openFileDialogViewModel.Filter = _filter;
            System.Windows.Forms.DialogResult result = _dialogService.ShowOpenFileDialog(_ownerViewModel, openFileDialogViewModel);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _configurationModel.Logo = File.ReadAllBytes(openFileDialogViewModel.FileName);
            }
        }

        #endregion

        #region ClearLogo

        /// <summary>
        /// Clear company's logo.
        /// </summary>
        private void ClearLogo()
        {
            _configurationModel.Logo = null;
        }

        #endregion

        #region CheckDupCodeUOM

        /// <summary>
        /// Check duplicate UOM's code.
        /// </summary>
        /// <param name="UOM">base_UOMModel to check duplicate.</param>
        /// <returns>True is duplicate.</returns>
        private bool CheckDupCodeUOM(base_UOMModel UOM)
        {
            bool isDuplicate = false;

            try
            {
                base_UOMRepository UOMRepository = new base_UOMRepository();
                isDuplicate = UOMRepository.Get(x => x.Id != UOM.Id && x.Code.Trim().ToLower() == UOM.Code.Trim().ToLower()) != null;
            }
            catch
            {
                throw;
            }

            return isDuplicate;
        }

        #endregion

        #region CheckDupCodeStore

        /// <summary>
        /// Check duplicate store's code.
        /// </summary>
        /// <param name="store">base_StoreModel to check duplicate.</param>
        /// <returns>True is duplicate.</returns>
        private bool CheckDupCodeStore(base_StoreModel store)
        {
            bool isDuplicate = false;

            try
            {
                base_StoreRepository storeRepository = new base_StoreRepository();
                isDuplicate = storeRepository.Get(x => x.Id != store.Id && x.Code != null &&
                    x.Code.Trim().ToLower() == store.Code.Trim().ToLower()) != null;
            }
            catch
            {
                throw;
            }

            return isDuplicate;
        }

        #endregion

        #region AddRemovePaymentMethod

        /// <summary>
        /// Add or remove a payment method on PaymentMethodCollection.
        /// </summary>
        private void AddRemovePaymentMethod(ComboItem item, bool isSelected)
        {
            if (isSelected)
            {
                // Add.
                if (_paymentMethodCollection.FirstOrDefault(x => x.Value == item.Value) == null)
                {
                    _paymentMethodCollection.Add(item);
                }
            }
            else
            {
                // Remove.
                if (item.IntValue != Define.AlwaysPaymentMethod)
                {
                    _paymentMethodCollection.Remove(_paymentMethodCollection.FirstOrDefault(x => x.Value == item.Value));
                }
            }

            PaymentMethodCollection = new ObservableCollection<ComboItem>(_paymentMethodCollection.OrderBy(x => x.Text));

            if (!_configurationModel.DefaultPaymentMethod.HasValue)
            {
                // Select difference item make default payment method.
                ComboItem alwaysPaymentMethod = _paymentMethodCollection.FirstOrDefault(x => x.IntValue == Define.AlwaysPaymentMethod);
                if (alwaysPaymentMethod != null)
                {
                    _configurationModel.DefaultPaymentMethod = alwaysPaymentMethod.Value;
                }
                else
                {
                    if (_paymentMethodCollection.Count > 0)
                    {
                        _configurationModel.DefaultPaymentMethod = _paymentMethodCollection.FirstOrDefault().Value;
                    }
                    else
                    {
                        _configurationModel.DefaultPaymentMethod = null;
                    }
                }
            }

            _configurationModel.RaiseDefaultPaymentMethodChanged();
        }

        #endregion

        #endregion

        #region Override Methods

        #region LoadData

        public override void LoadData()
        {
            _isLoading = true;
            Initialize();
            _isLoading = false;
        }

        #endregion

        #region OnViewChangingCommandCanExecute

        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return SaveWithQuestion();
        }

        #endregion

        #endregion

        #region Events

        #region UOMCollectionChanged

        /// <summary>
        /// Occurs when UOMCollection changed.
        /// </summary>
        private void UOMCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    (item as ModelBase).IsTemporary = true;
                }
            }
        }

        #endregion

        #region ConfigurationPropertyChanged

        private void ConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_ConfigurationModel configuration = sender as base_ConfigurationModel;
            if (e.PropertyName == "DefaultPriceSchema" && configuration.DefaultPriceSchema != null)
            {
                _priceCollection.Add(_priceCollection.DeletedItems.First());
                _priceCollection.DeletedItems.Clear();
                _priceCollection.Remove(_priceCollection.FirstOrDefault(x => x.Id == configuration.DefaultPriceSchema));
            }
        }

        #endregion

        #endregion
    }
}
