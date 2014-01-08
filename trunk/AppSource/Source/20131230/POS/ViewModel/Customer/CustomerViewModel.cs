using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using System.Text;
using System.Windows.Controls;
using System.Globalization;

namespace CPC.POS.ViewModel
{
    class CustomerViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand<object> SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand PrintCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand<object> DeleteContactCommand { get; private set; }
        public RelayCommand<object> OpenPopupPaymendCardCommand { get; private set; }
        public RelayCommand<object> CreateNewPaymentCardCommand { get; private set; }
        public RelayCommand<object> DeletePaymentCardCommand { get; private set; }
        public RelayCommand<object> LoadStepCommand { get; private set; }
        public RelayCommand NoteCommand { get; private set; }
        public RelayCommand<object> DeletesCommand { get; private set; }
        //Repository
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_GuestAdditionalRepository _guestAdditionalRepository = new base_GuestAdditionalRepository();

        private base_SaleTaxLocationRepository _saleTaxLocationRepository = new base_SaleTaxLocationRepository();
        private base_GuestProfileRepository _guestProfileRepository = new base_GuestProfileRepository();
        private base_GuestPaymentCardRepository _guestPaymentCardRepository = new base_GuestPaymentCardRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();
        private base_RewardManagerRepository _rewardManagerRepository = new base_RewardManagerRepository();
        private base_GuestRewardRepository _guestRewardRepository = new base_GuestRewardRepository();
        private base_SaleCommissionRepository _saleCommissionRepository = new base_SaleCommissionRepository();
        private base_GuestGroupRepository _guestGroupRepository = new base_GuestGroupRepository();
        private base_CustomFieldRepository _customFieldRepository = new base_CustomFieldRepository();
        private base_MemberShipRepository _memberShipRepository = new base_MemberShipRepository();

        private BackgroundWorker _bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        private string CUSTOMER_IMAGE_FOLDER = System.IO.Path.Combine(Define.CONFIGURATION.DefautlImagePath, "Customer");

        private string CUSTOMER_MARK = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();

        public List<base_SaleTaxLocation> AllSaleTax { get; set; }

        private ICollectionView _rewardCollectionView;

        private bool _viewExisted = false;

        private enum CustomerFormTab
        {
            [Description("CustomerInfoTab")]
            CustomerInfoTab = 0,
            [Description("PersonalInformationTab")]
            PersonalInformationTab = 1,
            [Description("AdditionalInfoTab")]
            AdditionalInfoTab = 2,
            [Description("PaymentTab")]
            PaymentTab = 3,
            [Description("RewardTab")]
            RewardTab = 4,
            [Description("SOHistoryTab")]
            SOHistoryTab = 5
        }

        public bool IsAdvanced { get; set; }

        private Expression<Func<base_Guest, bool>> AdvanceSearchPredicate;
        #endregion

        #region Constructors

        public CustomerViewModel(bool isSearchMode = false, object param = null)
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            StickyManagementViewModel = new PopupStickyViewModel();
            InitialCommand();
            InitialStaticData();
            ChangeSearchMode(isSearchMode, param);

            // Get permission
            GetPermission();
        }



        #endregion

        #region Properties

        #region POSConfig
        private base_ConfigurationModel _posConfig;
        /// <summary>
        /// Gets or sets the POSConfi.
        /// </summary>
        public base_ConfigurationModel POSConfig
        {
            get { return _posConfig; }
            set
            {
                if (_posConfig != value)
                {
                    _posConfig = value;
                    OnPropertyChanged(() => POSConfig);
                }
            }
        }
        #endregion

        #region IsForceFocused
        private bool _isForceFocused;
        /// <summary>
        /// Gets or sets the IsForceFocused.
        /// </summary>
        public bool IsForceFocused
        {
            get { return _isForceFocused; }
            set
            {
                if (_isForceFocused != value)
                {
                    _isForceFocused = value;
                    OnPropertyChanged(() => IsForceFocused);
                }
            }
        }
        #endregion

        #region IsVisibleSOHistory
        /// <summary>
        /// Gets the IsVisibleSOHistory.
        /// </summary>
        public bool IsVisibleSOHistory
        {
            get
            {
                if (SelectedCustomer == null)
                    return false;
                return AllowAccessSOHistory && !SelectedCustomer.IsNew;
            }

        }
        #endregion

        #region CustomerTabIndex
        private string _customerTabItem;
        /// <summary>
        /// Gets or sets the CustomerTabIndex.
        /// </summary>
        public string CustomerTabItem
        {
            get { return _customerTabItem; }
            set
            {
                if (_customerTabItem != value)
                {
                    _customerTabItem = value;
                    OnPropertyChanged(() => CustomerTabItem);
                    CustomerTabChanged(CustomerTabItem);
                }
            }
        }


        #endregion

        #region IsDirty
        //private bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedCustomer == null)
                    return false;
                return (SelectedCustomer.IsDirty
                    || SelectedCustomer.AdditionalModel.IsDirty
                    || (SelectedCustomer.PaymentCardCollection != null && (SelectedCustomer.PaymentCardCollection.Any(x => x.IsDirty) || SelectedCustomer.PaymentCardCollection.DeletedItems.Any()))
                    || (this.SelectedCustomer.AddressControlCollection != null && this.SelectedCustomer.AddressControlCollection.IsEditingData)
                    || (this.SelectedCustomer.PhotoCollection != null && this.SelectedCustomer.PhotoCollection.IsDirty)
                    || SelectedCustomer.PersonalInfoModel.IsDirty
                    || (SelectedCustomer.IsRewardMember && SelectedCustomer.GuestRewardCollection != null && SelectedCustomer.GuestRewardCollection.Any(x => x.IsDirty)));
            }
        }
        #endregion

        #region CustomFieldCollection
        private ObservableCollection<base_CustomFieldModel> _customFieldCollection;
        /// <summary>
        /// Gets or sets the CustomFieldCollection.
        /// </summary>
        public ObservableCollection<base_CustomFieldModel> CustomFieldCollection
        {
            get { return _customFieldCollection; }
            set
            {
                if (_customFieldCollection != value)
                {
                    _customFieldCollection = value;
                    OnPropertyChanged(() => CustomFieldCollection);
                }
            }
        }
        #endregion

        //Search
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

        #region IsAdvanceMode
        private bool _isAdvanceMode;
        /// <summary>
        /// Gets or sets the IsAdvanceMode.
        /// Using for Search. False is a simple Search
        /// </summary>
        public bool IsAdvanceMode
        {
            get { return _isAdvanceMode; }
            set
            {
                if (_isAdvanceMode != value)
                {
                    _isAdvanceMode = value;
                    OnPropertyChanged(() => IsAdvanceMode);
                }
            }
        }
        #endregion

        #region SearchOption
        private int _searchOption;
        /// <summary>
        /// Gets or sets the SearchOption.
        /// </summary>
        public int SearchOption
        {
            get { return _searchOption; }
            set
            {
                if (_searchOption != value)
                {
                    _searchOption = value;
                    OnPropertyChanged(() => SearchOption);
                    if (!string.IsNullOrWhiteSpace(FilterText))
                        OnSearchCommandExecute(FilterText);
                }
            }
        }
        #endregion

        #region FilterText & Keyword
        private string _filterText;
        /// <summary>
        /// Gets or sets the FilterText.
        /// <para>Keyword user input but not press enter</para>
        /// <remarks>Binding in textbox keyword</remarks>
        /// </summary>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(() => FilterText);
                }
            }
        }

        public string Keyword { get; set; }
        #endregion

        #region SearchAlert
        private string _searchAlert;
        /// <summary>
        /// Gets or sets the SearchAlert.
        /// </summary>
        public string SearchAlert
        {
            get { return _searchAlert; }
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

        #region ContactTotalItem
        /// <summary>
        /// Gets Count item in ContactCollection.
        /// </summary>
        public int ContactTotalItem
        {
            get
            {
                if (SelectedCustomer != null && SelectedCustomer.ContactCollection != null)
                    return SelectedCustomer.ContactCollection.Count(x => !x.IsTemporary);
                return 0;
            }
        }
        #endregion

        #region CreditCardTotalItem
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int CreditCardTotalItem
        {
            get
            {
                if (SelectedCustomer != null && SelectedCustomer.PaymentCardCollection != null)
                    return SelectedCustomer.PaymentCardCollection.Count(x => !x.IsTemporary);
                return 0;
            }
        }
        #endregion

        #region AddressTypeCollection
        private AddressTypeCollection _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection AddressTypeCollection
        {
            get { return _addressTypeCollection; }
            set
            {
                if (_addressTypeCollection != value)
                {
                    _addressTypeCollection = value;
                    OnPropertyChanged(() => AddressTypeCollection);
                }
            }
        }
        #endregion

        #region EmployeeCollection
        private ObservableCollection<base_GuestModel> _employeeCollection;
        /// <summary>
        /// Gets or sets the EmployeeCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> EmployeeCollection
        {
            get { return _employeeCollection; }
            set
            {
                if (_employeeCollection != value)
                {
                    _employeeCollection = value;
                    OnPropertyChanged(() => EmployeeCollection);
                }
            }
        }
        #endregion

        #region GuestGroupCollection
        private ObservableCollection<base_GuestGroupModel> _guestGroupCollection;
        /// <summary>
        /// Gets or sets the GuestGroupCollection.
        /// </summary>
        public ObservableCollection<base_GuestGroupModel> GuestGroupCollection
        {
            get { return _guestGroupCollection; }
            set
            {
                if (_guestGroupCollection != value)
                {
                    _guestGroupCollection = value;
                    OnPropertyChanged(() => GuestGroupCollection);
                }
            }
        }
        #endregion

        //Customer
        #region TotalCustomers
        private int _totalCustomers;
        /// <summary>
        /// Gets or sets the CountFilter For Search Control.
        /// </summary>
        public int TotalCustomers
        {

            get
            {
                return _totalCustomers;
            }
            set
            {
                if (_totalCustomers != value)
                {
                    _totalCustomers = value;
                    OnPropertyChanged(() => TotalCustomers);
                }
            }
        }
        #endregion

        #region CustomerCollection
        private ObservableCollection<base_GuestModel> _customerCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// Collection for DataGridSearch
        /// </summary>
        public ObservableCollection<base_GuestModel> CustomerCollection
        {
            get { return _customerCollection; }
            set
            {
                if (_customerCollection != value)
                {
                    _customerCollection = value;
                    OnPropertyChanged(() => CustomerCollection);
                }
            }
        }
        #endregion

        #region SelectedCustomer
        private base_GuestModel _selectedCustomer;
        /// <summary>
        /// Gets or sets the SelectedCustomer.
        /// </summary>
        public base_GuestModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(() => SelectedCustomer);
                }
            }
        }

        #endregion

        #region SelectedContact
        private base_GuestModel _selectedContact;
        /// <summary>
        /// Gets or sets the SelectedContact.
        /// </summary>
        public base_GuestModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                if (_selectedContact != value)
                {
                    _selectedContact = value;
                    OnPropertyChanged(() => SelectedContact);
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the NotePopupCollection.
        /// </summary>
        public ObservableCollection<PopupContainer> NotePopupCollection { get; set; }

        /// <summary>
        /// Gets the ShowOrHiddenNote
        /// </summary>
        public string ShowOrHiddenNote
        {
            get
            {
                if (NotePopupCollection.Count == 0)
                    return "Show Stickies";
                else if (NotePopupCollection.Count == SelectedCustomer.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                    return "Hide Stickies";
                else
                    return "Show Stickies";
            }
        }

        #region SaleTaxCollection
        private ObservableCollection<base_SaleTaxLocationModel> _saleTaxCollection;
        /// <summary>
        /// Gets or sets the SaleTaxCollection.
        /// </summary>
        public ObservableCollection<base_SaleTaxLocationModel> SaleTaxCollection
        {
            get { return _saleTaxCollection; }
            set
            {
                if (_saleTaxCollection != value)
                {
                    _saleTaxCollection = value;
                    OnPropertyChanged(() => SaleTaxCollection);
                }
            }
        }
        #endregion

        private base_SaleOrderModel _totalSaleOrder;
        /// <summary>
        /// Gets or sets the TotalSaleOrder.
        /// </summary>
        public base_SaleOrderModel TotalSaleOrder
        {
            get { return _totalSaleOrder; }
            set
            {
                if (_totalSaleOrder != value)
                {
                    _totalSaleOrder = value;
                    OnPropertyChanged(() => TotalSaleOrder);
                }
            }
        }

        #region RewardProgram
        private base_RewardManagerModel _rewardProgram;
        /// <summary>
        /// Gets or sets the RewardProgram.
        /// </summary>
        public base_RewardManagerModel RewardProgram
        {
            get { return _rewardProgram; }
            set
            {
                if (_rewardProgram != value)
                {
                    _rewardProgram = value;
                    OnPropertyChanged(() => RewardProgram);
                }
            }
        }
        #endregion

        #region StartDateReward
        private DateTime? _startDateReward;
        /// <summary>
        /// Gets or sets the StartDateReward.
        /// </summary>
        public DateTime? StartDateReward
        {
            get { return _startDateReward; }
            set
            {
                if (_startDateReward != value)
                {
                    _startDateReward = value;
                    OnPropertyChanged(() => StartDateReward);
                    FilterGuestReward(SelectedCustomer);
                }
            }
        }
        #endregion

        #region EndDateReward
        private DateTime? _endDateReward;
        /// <summary>
        /// Gets or sets the EndDateReward.
        /// </summary>
        public DateTime? EndDateReward
        {
            get { return _endDateReward; }
            set
            {
                if (_endDateReward != value)
                {
                    _endDateReward = value;
                    OnPropertyChanged(() => EndDateReward);
                    FilterGuestReward(SelectedCustomer);
                }
            }
        }
        #endregion

        #region StateCollection
        private ObservableCollection<ComboItem> _stateCollection;
        /// <summary>
        /// Gets or sets the StateCollection.
        /// </summary>
        public ObservableCollection<ComboItem> StateCollection
        {
            get { return _stateCollection; }
            set
            {
                if (_stateCollection != value)
                {
                    _stateCollection = value;
                    OnPropertyChanged(() => StateCollection);
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
            if (ChangeViewExecute(null))
            {
                CreateNewCustomer();
                IsSearchMode = false;
            }
        }
        #endregion

        #region Save Command
        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute(object param)
        {
            return IsValid && this.IsDirty &&
                (this.SelectedCustomer.AddressControlCollection != null && !this.SelectedCustomer.AddressControlCollection.IsErrorData);
        }
        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute(object param)
        {
            SaveCustomer();
        }
        #endregion

        #region DeleteCommand
        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedCustomer == null)
                return false;
            return !SelectedCustomer.IsNew && !IsDirty;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            //M103 Do you want to delete
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                if (SelectedCustomer.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();

                    SelectedCustomer = null;
                    IsSearchMode = true;
                }
                else
                {
                    List<ItemModel> ItemModel = new List<ItemModel>();
                    string resource = SelectedCustomer.Resource.Value.ToString();
                    if (!_saleOrderRepository.GetAll().Select(x => x.CustomerResource).Contains(resource))
                    {
                        SelectedCustomer.IsPurged = true;
                        SaveCustomer();
                        CustomerCollection.Remove(SelectedCustomer);
                        SelectedCustomer = CustomerCollection.First();
                        TotalCustomers = TotalCustomers - 1;

                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        IsSearchMode = true;
                    }
                    else
                    {
                        ItemModel.Add(new ItemModel { Id = SelectedCustomer.Id, Text = SelectedCustomer.GuestNo, Resource = resource });
                        _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "SaleOrder"), "Problem Detection");
                    }
                }

            }

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
            try
            {
                IsAdvanced = false;
                Expression<Func<base_Guest, bool>> predicate = CreateSimpleSearchPredicate(FilterText);
                LoadDataByPredicate(predicate, false, 0);

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        #endregion

        #region PrintCommand
        /// <summary>
        /// Method to check whether the Print command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPrintCommandCanExecute()
        {
            return (SelectedCustomer != null && !SelectedCustomer.IsNew && !SelectedCustomer.IsDirty);
        }


        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute()
        {
            // Show Customer report
            View.Report.ReportWindow rpt = new View.Report.ReportWindow();
            string param = "'" + SelectedCustomer.Resource.ToString() + "'";
            rpt.ShowReport("rptCustomerProfile", param);
        }
        #endregion

        #region DoubleClickCommand

        /// <summary>
        /// Method to check whether the DoubleClick command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDoubleClickViewCommandCanExecute(object param)
        {
            if (IsSearchMode && param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DoubleClick command is executed.
        /// </summary>
        private void OnDoubleClickViewCommandExecute(object param)
        {
            if (param != null && IsSearchMode)
            {
                base_GuestModel guestModel = (param as base_GuestModel);

                CustomerTabItem = CustomerFormTab.CustomerInfoTab.ToDescription();
                EditItem(guestModel);
                OnPropertyChanged(() => IsVisibleSOHistory);
            }
            else if (!IsSearchMode)//Change from Edit form to Search Gird check view has dirty
            {
                if (this.ChangeViewExecute(null))
                    this.IsSearchMode = true;
            }
            else
                this.IsSearchMode = !this.IsSearchMode;//Change View To
        }



        #endregion

        #region DeleteContactCommand

        /// <summary>
        /// Method to check whether the DeleteContact command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteContactCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteContact command is executed.
        /// </summary>
        private void OnDeleteContactCommandExecute(object param)
        {
            if (param != null)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                base_GuestModel contactModel = param as base_GuestModel;
                if (!contactModel.IsTemporary)
                {
                    msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    if (msgResult.Is(MessageBoxResult.Yes))
                    {
                        SelectedCustomer.ContactCollection.Remove(contactModel);
                        if (contactModel.IsNew)//Old item not remove in Collection delete.it will handle in Save proccess
                            SelectedCustomer.ContactCollection.DeletedItems.Remove(contactModel);

                        if (contactModel.IsPrimary == true)//Item want to delete need to check has another item in collection to set primary
                        {
                            base_GuestModel guestPrimary = SelectedCustomer.ContactCollection.FirstOrDefault(x => !x.IsTemporary && x.GuestNo != contactModel.GuestNo);
                            if (guestPrimary != null)//Set primary for another item
                            {
                                guestPrimary.IsPrimary = true;
                                guestPrimary.IsTemporary = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region OpenPopupPaymentCard
        /// <summary>
        /// Method to check whether the OpenPopup command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenPopupPaymentCardCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the OpenPopup command is executed.
        /// </summary>
        private void OnOpenPopupPaymentCardCommandExecute(object param)
        {
            if (param != null)
            {
                base_GuestPaymentCardModel paymentCardModel = param as base_GuestPaymentCardModel;
                OpenEditCardView(paymentCardModel);
            }
        }
        #endregion

        #region CreateNewPaymentCardCommand

        /// <summary>
        /// Method to check whether the CreateNewPaymentCard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCreateNewPaymentCardCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CreateNewPaymentCard command is executed.
        /// </summary>
        private void OnCreateNewPaymentCardCommandExecute(object param)
        {
            base_GuestPaymentCardModel paymentCardModel = CreateNewPaymentCard();
            OpenCreditCardView(paymentCardModel);
        }


        #endregion

        #region DeletePaymentCard
        /// <summary>
        /// Method to check whether the DeletePaymentCard command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletePaymentCardCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeletePaymentCard command is executed.
        /// </summary>
        private void OnDeletePaymentCardCommandExecute(object param)
        {
            if (param != null)
            {
                base_GuestPaymentCardModel paymentCardModel = param as base_GuestPaymentCardModel;
                if (!paymentCardModel.IsTemporary)
                {
                    MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    if (msgResult.Is(MessageBoxResult.Yes))
                    {
                        SelectedCustomer.PaymentCardCollection.Remove(paymentCardModel);
                    }
                }
            }
        }
        #endregion

        #region LoadDatByStepCommand
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
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            if (IsAdvanced) //Load Contitnue with advance Search
            {
                predicate = AdvanceSearchPredicate;
            }
            else if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreateSimpleSearchPredicate(FilterText);

            LoadDataByPredicate(predicate, false, CustomerCollection.Count);
        }
        #endregion

        #region AddTermCommand
        /// <summary>
        /// Gets the AddTerm Command.
        /// <summary>
        public RelayCommand AddTermCommand { get; private set; }


        /// <summary>
        /// Method to check whether the AddTerm command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddTermCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddTerm command is executed.
        /// </summary>
        private void OnAddTermCommandExecute()
        {
            short dueDays = SelectedCustomer.TermNetDue;
            decimal discount = SelectedCustomer.TermDiscount;
            short discountDays = SelectedCustomer.TermPaidWithinDay;
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, "Add Term");
            if (dialogResult == true)
            {
                SelectedCustomer.TermNetDue = paymentTermViewModel.DueDays;
                SelectedCustomer.TermDiscount = paymentTermViewModel.Discount;
                SelectedCustomer.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                SelectedCustomer.PaymentTermDescription = paymentTermViewModel.Description;
            }
        }
        #endregion

        #region DeletesCommand
        /// <summary>
        /// Method to check whether the DeletesCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletesCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as ObservableCollection<object>).Count > 0;
        }

        /// <summary>
        /// Method to invoke when the DeletesCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M103"), Language.GetMsg("DeleteCaption"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result.Is(MessageBoxResult.Yes))
            {
                bool flag = false;
                List<ItemModel> ItemModel = new List<ItemModel>();
                for (int i = 0; i < (param as ObservableCollection<object>).Count; i++)
                {
                    base_GuestModel model = (param as ObservableCollection<object>)[i] as base_GuestModel;
                    string resource = model.Resource.Value.ToString();
                    if (!_saleOrderRepository.GetAll().Select(x => x.CustomerResource).Contains(resource))
                    {
                        model.IsPurged = true;
                        model.ToEntity();
                        _guestRepository.Commit();
                        CustomerCollection.Remove(model);
                        TotalCustomers = TotalCustomers - 1;

                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote(model.ResourceNoteCollection);

                        i--;
                    }
                    else
                    {
                        ItemModel.Add(new ItemModel { Id = model.Id, Text = model.GuestNo, Resource = resource });
                        flag = true;
                    }
                }
                if (flag)
                    _dialogService.ShowDialog<ProblemDetectionView>(_ownerViewModel, new ProblemDetectionViewModel(ItemModel, "SaleOrder"), "Problem Detection");
            }
        }
        #endregion

        #region IssueRewardManagerCommand

        /// <summary>
        /// Gets the IssueRewardManager Command.
        /// <summary>
        public RelayCommand<object> IssueRewardManagerCommand { get; private set; }

        /// <summary>
        /// Method to check whether the IssueRewardManager command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnIssueRewardManagerCommandCanExecute(object param)
        {
            if (RewardProgram == null)
                return false;
            return RewardProgram.Id > 0 && AllowManualReward;
        }

        /// <summary>
        /// Method to invoke when the IssueRewardManager command is executed.
        /// </summary>
        /// 
        private void OnIssueRewardManagerCommandExecute(object param)
        {
            if (SelectedCustomer.GuestRewardCollection == null)
                SelectedCustomer.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
            RewardEarnViewModel rewardEarnViewModel = new RewardEarnViewModel(SelectedCustomer.Id, RewardProgram, SelectedCustomer.GuestRewardCollection);
            bool? result = _dialogService.ShowDialog<RewardEarnView>(_ownerViewModel, rewardEarnViewModel, "Earn Reward");

            if (result == true)
            {
                SelectedCustomer.GuestRewardCollection = rewardEarnViewModel.GuestRewardCollection;
                //ResetCollection view
                _rewardCollectionView = null;
                //FilterGuestReward(SelectedCustomer);
            }
        }

        #endregion

        #region SaleOrderCommand

        /// <summary>
        /// Gets the SaleOrder Command.
        /// <summary>
        public RelayCommand<object> SaleOrderCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaleOrder command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaleOrderCommandCanExecute(object param)
        {
            return param != null && (param as ObservableCollection<object>).Count == 1 && AllowSaleFromCustomer;
        }

        /// <summary>
        /// Method to invoke when the SaleOrder command is executed.
        /// </summary>
        private void OnSaleOrderCommandExecute(object param)
        {
            ObservableCollection<object> guestCollection = param as ObservableCollection<object>;
            ComboItem cmbValue = new ComboItem();
            cmbValue.Text = "Customer";

            cmbValue.Detail = (guestCollection.First() as base_GuestModel).Id;
            (_ownerViewModel as MainViewModel).OpenViewExecute("SalesOrder", cmbValue);
        }

        #endregion

        #region Merger Customer Command
        /// <summary>
        /// Gets the MergeCustomer Command.
        /// <summary>

        public RelayCommand<object> MergeCustomerCommand { get; private set; }



        /// <summary>
        /// Method to check whether the MergeCustomer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnMergeCustomerCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return param != null && (param as ObservableCollection<object>).Count == 1;
        }


        /// <summary>
        /// Method to invoke when the MergeCustomer command is executed.
        /// </summary>
        private void OnMergeCustomerCommandExecute(object param)
        {
            base_GuestModel guestModel = (param as ObservableCollection<object>).FirstOrDefault() as base_GuestModel;

            MergeCustomerViewModel viewModel = new MergeCustomerViewModel(guestModel, this.CustomerCollection);
            bool? result = _dialogService.ShowDialog<MergeCustomerView>(_ownerViewModel, viewModel, Language.GetMsg("CUS_MSG_MergeCustomer"));
            if (result == true)
            {
                if ((this._ownerViewModel as MainViewModel).IsOpenedView("SalesOrder"))
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("CUS_MSG_ShouldSaleOrder"), Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                        (this._ownerViewModel as MainViewModel).CloseItem("SalesOrder");
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }

                try
                {
                    UnitOfWork.BeginTransaction();
                    //Get Source & Target Customer 
                    base_GuestModel customerSource = CustomerCollection.SingleOrDefault(x => x.Id.Equals(viewModel.CustomerSource.Id));
                    base_GuestModel customerTarget = CustomerCollection.SingleOrDefault(x => x.Id.Equals(viewModel.CustomerTarget.Id));

                    string customerSourceResource = customerSource.Resource.ToString();
                    string customerTargetResource = customerTarget.Resource.ToString();

                    //Update Customer Reward
                    IList<base_GuestReward> customerRewards = _guestRewardRepository.GetAll(x => x.GuestId.Equals(customerSource.Id));
                    foreach (base_GuestReward customerReward in customerRewards)
                    {
                        customerReward.GuestId = customerTarget.Id;
                    }

                    //Delete Customer Note
                    IList<base_ResourceNote> notes = _resourceNoteRepository.GetAll(x => x.Resource.Equals(customerSourceResource));
                    foreach (base_ResourceNote note in notes)
                        _resourceNoteRepository.Delete(note);

                    //Update SaleOrder
                    bool updateBillAddress = false;//Check Customer Target not any bill Address
                    bool updateShipAddress = false;//Check Customer Target not any Ship Address
                    if (!customerTarget.base_Guest.base_GuestAddress.Any(x => x.AddressTypeId.Equals(2)))//BillAddress
                        updateBillAddress = true;
                    if (!customerTarget.base_Guest.base_GuestAddress.Any(x => x.AddressTypeId.Equals(3)))//ShipAddress
                        updateShipAddress = true;


                    IList<base_SaleOrder> saleOrders = _saleOrderRepository.GetAll(x => x.CustomerResource.Equals(customerSourceResource));
                    foreach (base_SaleOrder saleOrder in saleOrders)
                    {
                        //Update Address
                        if (updateBillAddress)//Get BillAdress from saleOrder update to CustomerAddress
                        {
                            int billAddressId = Convert.ToInt32(saleOrder.BillAddressId.Value);
                            base_GuestAddress billAddress = _guestAddressRepository.Get(x => x.Id.Equals(billAddressId));
                            billAddress.GuestId = customerTarget.Id;
                            customerTarget.base_Guest.base_GuestAddress.Add(billAddress);
                            updateBillAddress = false;
                        }
                        else
                        {
                            base_GuestAddress billAddressTargetCustomer = customerTarget.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId.Equals(2));
                            base_GuestAddressModel billAddressModel = new base_GuestAddressModel(billAddressTargetCustomer);
                            saleOrder.BillAddressId = billAddressModel.Id;
                            saleOrder.BillAddress = billAddressModel.Text;
                        }
                        //Update Address
                        if (updateShipAddress)//Get BillAdress from saleOrder update to CustomerAddress
                        {
                            int shipAddressId = Convert.ToInt32(saleOrder.ShipAddressId.Value);
                            base_GuestAddress shipAddress = _guestAddressRepository.Get(x => x.Id.Equals(shipAddressId));
                            shipAddress.GuestId = customerTarget.Id;
                            customerTarget.base_Guest.base_GuestAddress.Add(shipAddress);
                            updateShipAddress = false;
                        }
                        else
                        {
                            base_GuestAddress shipAddressTargetCustomer = customerTarget.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId.Equals(3));
                            base_GuestAddressModel shipAddressModel = new base_GuestAddressModel(shipAddressTargetCustomer);
                            saleOrder.ShipAddressId = shipAddressModel.Id;
                            saleOrder.ShipAddress = shipAddressModel.Text;
                        }

                        saleOrder.CustomerResource = customerTargetResource;
                    }

                    //Update customer Reminder
                    base_CustomerReminderRepository customerReminderRespository = new base_CustomerReminderRepository();
                    IList<base_CustomerReminder> customersourceReminders = customerReminderRespository.GetAll(x => x.GuestResource.Equals(customerSourceResource));
                    foreach (base_CustomerReminder sourceReminder in customersourceReminders)
                    {
                        sourceReminder.GuestResource = customerTarget.Resource.ToString();
                        if (customerTarget.PersonalInfoModel!=null)
                            sourceReminder.DOB = customerTarget.PersonalInfoModel.DOB;
                        sourceReminder.Name = customerTarget.LegalName;
                        sourceReminder.Company = customerTarget.Company;
                        sourceReminder.Phone = customerTarget.Phone1;
                        sourceReminder.Email = customerTarget.Email;
                    }

                    //Update Customer Reward
                    customerTarget.PurchaseDuringTrackingPeriod += customerSource.PurchaseDuringTrackingPeriod;
                    customerTarget.RequirePurchaseNextReward += customerSource.RequirePurchaseNextReward;
                    customerTarget.TotalRewardRedeemed += customerSource.TotalRewardRedeemed;
                    customerTarget.ToEntity();

                    //Delete Source Customer from database
                    _guestRepository.Delete(customerSource.base_Guest);

                    //Remove from collection
                    CustomerCollection.Remove(customerSource);

                    //Commit data
                    _guestRepository.Commit();
                    UnitOfWork.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _log4net.Error(ex);
                    UnitOfWork.RollbackTransaction();
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), Language.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }
        #endregion

        #region AddNewGuestGroupCommand
        /// <summary>
        /// Gets the AddNewGuestGroup Command.
        /// <summary>

        public RelayCommand<object> AddNewGuestGroupCommand { get; private set; }



        /// <summary>
        /// Method to check whether the AddNewGuestGroup command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewGuestGroupCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddNewGuestGroup command is executed.
        /// </summary>
        private void OnAddNewGuestGroupCommandExecute(object param)
        {
            PopupAddNewGroupViewModel viewModel = new PopupAddNewGroupViewModel();
            bool? result = _dialogService.ShowDialog<PopupAddNewGroupView>(_ownerViewModel, viewModel, Language.GetMsg("CUS_Text_AddNewGroup"));
            if (result.HasValue && result.Value)
            {
                // Add new guest group to collection
                GuestGroupCollection.Add(viewModel.SelectedGuestGroup);

                SelectedCustomer.GroupResource = viewModel.SelectedGuestGroup.Resource.ToString();
            }
        }
        #endregion

        #region DuplicateItem
        /// <summary>
        /// Gets the DuplicateItem Command.
        /// <summary>

        public RelayCommand<object> DuplicateItemCommand { get; private set; }


        /// <summary>
        /// Method to check whether the DuplicateItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDuplicateItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as ObservableCollection<object>).Count == 1;
        }


        /// <summary>
        /// Method to invoke when the DuplicateItem command is executed.
        /// </summary>
        private void OnDuplicateItemCommandExecute(object param)
        {
            base_GuestModel guestModel = (param as ObservableCollection<object>).FirstOrDefault() as base_GuestModel;
            SelectedCustomer = new base_GuestModel();
            SelectedCustomer = CopyFrom(guestModel);
            IsSearchMode = false;
        }
        #endregion

        #region AddNewStateCommand
        /// <summary>
        /// Gets the AddNewState Command.
        /// <summary>

        public RelayCommand<object> AddNewStateCommand { get; private set; }


        /// <summary>
        /// Method to check whether the AddNewState command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddNewStateCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the AddNewState command is executed.
        /// </summary>
        private void OnAddNewStateCommandExecute(object param)
        {
            AddNewStateViewModel viewModel = new AddNewStateViewModel();
            bool? result = _dialogService.ShowDialog<AddNewStateView>(_ownerViewModel, viewModel, "Add New State");
            if (result == true)
            {
                StateCollection.Add(viewModel.ItemState);

                if (param != null)
                {
                    if (param.ToString().Equals("Personal"))
                    {
                        SelectedCustomer.PersonalInfoModel.State = viewModel.ItemState.ObjValue.ToString();
                    }
                    else
                    {
                        SelectedCustomer.PersonalInfoModel.SState = viewModel.ItemState.ObjValue.ToString();
                    }
                }
            }

        }
        #endregion

        #region EditItem

        /// <summary>
        /// Gets the EditItem Command.
        /// <summary>

        public RelayCommand<object> EditItemCommand { get; private set; }



        /// <summary>
        /// Method to check whether the EditItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditItemCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return (param as ObservableCollection<object>).Count == 1;
        }


        /// <summary>
        /// Method to invoke when the EditItem command is executed.
        /// </summary>
        private void OnEditItemCommandExecute(object param)
        {
            base_GuestModel guestModel = (param as ObservableCollection<object>).FirstOrDefault() as base_GuestModel;
            EditItem(guestModel);
        }
        #endregion

        #region SearchAdvanceCommand
        /// <summary>
        /// Gets the SearchAdvance Command.
        /// <summary>

        public RelayCommand<object> SearchAdvanceCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SearchAdvance command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchAdvanceCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the SearchAdvance command is executed.
        /// </summary>
        private void OnSearchAdvanceCommandExecute(object param)
        {
            OpenSearchAdvance();
        }


        #endregion

        //Extent
        #region InsertDateStampCommand
        /// <summary>
        /// Gets the InsertDateStamp Command.
        /// <summary>

        public RelayCommand<object> InsertDateStampCommand { get; private set; }


        /// <summary>
        /// Method to check whether the InsertDateStamp command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnInsertDateStampCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the InsertDateStamp command is executed.
        /// </summary>
        private void OnInsertDateStampCommandExecute(object param)
        {
            CPCToolkitExt.TextBoxControl.TextBox remarkTextBox = param as CPCToolkitExt.TextBoxControl.TextBox;
            SetValueControlHelper.InsertTimeStamp(remarkTextBox);
        }


        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial Data For Customer View
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            PrintCommand = new RelayCommand(OnPrintCommandExecute, OnPrintCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            DeleteContactCommand = new RelayCommand<object>(OnDeleteContactCommandExecute, OnDeleteContactCommandCanExecute);
            OpenPopupPaymendCardCommand = new RelayCommand<object>(OnOpenPopupPaymentCardCommandExecute, OnOpenPopupPaymentCardCommandCanExecute);
            CreateNewPaymentCardCommand = new RelayCommand<object>(OnCreateNewPaymentCardCommandExecute, OnCreateNewPaymentCardCommandCanExecute);
            DeletePaymentCardCommand = new RelayCommand<object>(OnDeletePaymentCardCommandExecute, OnDeletePaymentCardCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);
            DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
            IssueRewardManagerCommand = new RelayCommand<object>(OnIssueRewardManagerCommandExecute, OnIssueRewardManagerCommandCanExecute);

            SaleOrderCommand = new RelayCommand<object>(OnSaleOrderCommandExecute, OnSaleOrderCommandCanExecute);
            MergeCustomerCommand = new RelayCommand<object>(OnMergeCustomerCommandExecute, OnMergeCustomerCommandCanExecute);

            AddNewGuestGroupCommand = new RelayCommand<object>(OnAddNewGuestGroupCommandExecute, OnAddNewGuestGroupCommandCanExecute);
            DuplicateItemCommand = new RelayCommand<object>(OnDuplicateItemCommandExecute, OnDuplicateItemCommandCanExecute);
            AddNewStateCommand = new RelayCommand<object>(OnAddNewStateCommandExecute, OnAddNewStateCommandCanExecute);

            InsertDateStampCommand = new RelayCommand<object>(OnInsertDateStampCommandExecute, OnInsertDateStampCommandCanExecute);
            EditItemCommand = new RelayCommand<object>(OnEditItemCommandExecute, OnEditItemCommandCanExecute);

            SearchAdvanceCommand = new RelayCommand<object>(OnSearchAdvanceCommandExecute, OnSearchAdvanceCommandCanExecute);
        }

        /// <summary>
        /// Initial Data for Customer View
        /// </summary>
        private void InitialStaticData()
        {
            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = Language.GetMsg("SO_TextBlock_Home") });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = Language.GetMsg("SO_TextBlock_Business") });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = Language.GetMsg("SO_TextBlock_Billing") });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = Language.GetMsg("SO_TextBlock_Shipping") });

            NotePopupCollection = new ObservableCollection<PopupContainer>();
            NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

            EmployeeCollection = new ObservableCollection<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived).Select(x => new base_GuestModel(x)));
            base_GuestModel defaultEmployee = new base_GuestModel()
            {
                Id = 0,
                FirstName = string.Empty,
                LastName = string.Empty
            };
            EmployeeCollection.Insert(0, defaultEmployee);

            // Load guest group collection
            GuestGroupCollection = new ObservableCollection<base_GuestGroupModel>(_guestGroupRepository.GetAll().
                Select(x => new base_GuestGroupModel(x) { GuestGroupResource = x.Resource.ToString() }));

            //Get All Sale Tax
            SaleTaxCollection = new ObservableCollection<base_SaleTaxLocationModel>();
            AllSaleTax = _saleTaxLocationRepository.GetAll().ToList();
            if (AllSaleTax != null)
            {
                foreach (base_SaleTaxLocation saleTaxLocation in AllSaleTax.Where(x => x.ParentId == 0))
                {
                    SaleTaxCollection.Add(new base_SaleTaxLocationModel(saleTaxLocation));
                }
                //Add Item null for sale tax using radio button set none sale tax
                base_SaleTaxLocationModel saleTaxNone = new base_SaleTaxLocationModel()
                {
                    Id = 0,
                    ParentId = 0,
                    Name = ""
                };

                SaleTaxCollection.Insert(0, saleTaxNone);
            }

            StateCollection = new ObservableCollection<ComboItem>(Common.States.OrderBy(x => x.Text));
        }

        /// <summary>
        /// Insert Customer 
        /// </summary>
        private bool SaveCustomer()
        {
            bool result = true;
            try
            {
                if (!CheckDuplicateCustomer(SelectedCustomer))
                {
                    //To save picture.
                    if (this.SelectedCustomer.PhotoCollection != null && this.SelectedCustomer.PhotoCollection.Count > 0)
                    {
                        this.SelectedCustomer.PhotoCollection.FirstOrDefault().IsNew = false;
                        this.SelectedCustomer.PhotoCollection.FirstOrDefault().IsDirty = false;
                        this.SelectedCustomer.Picture = this.SelectedCustomer.PhotoCollection.FirstOrDefault().ImageBinary;
                    }
                    else
                        this.SelectedCustomer.Picture = null;
                    if (this.SelectedCustomer.PhotoCollection.DeletedItems != null &&
                 this.SelectedCustomer.PhotoCollection.DeletedItems.Count > 0)
                        this.SelectedCustomer.PhotoCollection.DeletedItems.Clear();
                    //Handle clear data if user not choose "Pricing Level" in Additional
                    SelectedCustomer.AdditionalModel.IsNoDiscount = false;
                    if (SelectedCustomer.AdditionalModel.PriceLevelType == (int)PriceLevelType.NoDiscount)//Radio button choice "No Discount"
                    {
                        SelectedCustomer.AdditionalModel.FixDiscount = 0;
                        SelectedCustomer.AdditionalModel.PriceSchemeId = 0;
                        SelectedCustomer.AdditionalModel.IsNoDiscount = true;
                    }
                    else if (SelectedCustomer.AdditionalModel.PriceLevelType == (int)PriceLevelType.FixedDiscountOnAllItems)//Radio button choice "Fixed Discount"
                        SelectedCustomer.AdditionalModel.PriceSchemeId = 0;   //Set markdown Price Level
                    else
                        SelectedCustomer.AdditionalModel.FixDiscount = 0;

                    //For New Item
                    if (SelectedCustomer.IsNew)
                    {
                        SaveNewCustomer();
                    }
                    else//For Update Item
                    {
                        UpdateCustomer();
                    }
                    TotalCustomers = _guestRepository.GetIQueryable(x => !x.IsPurged && x.Mark.Equals(CUSTOMER_MARK)).Count();

                    //Raise Check show SO History Tab
                    OnPropertyChanged(() => IsVisibleSOHistory);

                    result = true;
                }
                else
                    result = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                result = false;
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            return result;
        }

        /// <summary>
        /// save Item For customer
        /// </summary>
        private void SaveNewCustomer()
        {
            //Set Customer
            base_GuestGroupModel guestGroupModel = GuestGroupCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedCustomer.GroupResource));
            if (guestGroupModel != null)
                SelectedCustomer.GroupName = guestGroupModel.Name;
            //Mapping Additional 
            if (SelectedCustomer.AdditionalModel != null)
            {
                SelectedCustomer.AdditionalModel.ToEntity();
                SelectedCustomer.base_Guest.base_GuestAdditional.Add(SelectedCustomer.AdditionalModel.base_GuestAdditional);
            }


            //Mapping Personal Info
            if (SelectedCustomer.PersonalInfoModel != null)
            {
                SelectedCustomer.PersonalInfoModel.GuestResource = SelectedCustomer.Resource.ToString();
                SelectedCustomer.PersonalInfoModel.ToEntity();
                SelectedCustomer.base_Guest.base_GuestProfile.Add(SelectedCustomer.PersonalInfoModel.base_GuestProfile);
            }

            //Save photo
            //SavePhotoResource(SelectedCustomer);

            ///Created by Thaipn.
            base_GuestAddressModel addressModel;
            bool firstAddress = true;
            //To insert an address. 
            //Convert from AddressControlCollection To AddressModel 
            foreach (AddressControlModel addressControlModel in this.SelectedCustomer.AddressControlCollection)
            {
                addressModel = new base_GuestAddressModel();
                addressModel.DateCreated = DateTime.Now;
                addressModel.DateUpdated = DateTime.Now;
                addressModel.UserCreated = Define.USER != null ? Define.USER.UserName : string.Empty;
                addressModel.ToModel(addressControlModel);
                addressModel.IsDefault = firstAddress;
                addressModel.EndUpdate();
                // To convert data from model to entity
                addressModel.ToEntity();
                this.SelectedCustomer.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                firstAddress = false;
                addressModel.EndUpdate();
                addressControlModel.IsDirty = false;
                addressControlModel.IsNew = false;

            }

            //Payment credit card
            foreach (base_GuestPaymentCardModel paymentCardModel in SelectedCustomer.PaymentCardCollection.Where(x => !x.IsTemporary))
            {
                paymentCardModel.ToEntity();
                SelectedCustomer.base_Guest.base_GuestPaymentCard.Add(paymentCardModel.base_GuestPaymentCard);

            }

            //Guest Reward Member
            if (SelectedCustomer.IsRewardMember)
            {
                base_MemberShipModel memberShipModel = NewMemberShipModel();
                memberShipModel.ToEntity();
                //Add To MemberShip Collection
                SelectedCustomer.base_Guest.base_MemberShip.Add(memberShipModel.base_MemberShip);

                if (SelectedCustomer.GuestRewardCollection != null)
                {
                    short memebershipActivedStatus = (short)MemberShipStatus.Actived;
                    base_MemberShip membership = SelectedCustomer.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
                    if (membership != null)
                        SelectedCustomer.MembershipValidated = new base_MemberShipModel(membership);

                    foreach (base_GuestRewardModel guestRewardModel in SelectedCustomer.GuestRewardCollection.Where(x => x.IsDirty))
                    {
                        guestRewardModel.ToEntity();
                        if (guestRewardModel.IsNew)
                            SelectedCustomer.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                    }

                    if (SelectedCustomer.MembershipValidated != null && SelectedCustomer.MembershipValidated.IsDirty)
                        SelectedCustomer.MembershipValidated.ToEntity();

                }
            }

            this.SelectedCustomer.ToEntity();
            _guestRepository.Add(this.SelectedCustomer.base_Guest);
            _guestRepository.Commit();
            CustomerCollection.Add(SelectedCustomer);

            SetIdNEndUpdate(SelectedCustomer);

        }

        /// <summary>
        /// Update Customer
        /// </summary>
        private void UpdateCustomer()
        {
            SelectedCustomer.DateUpdated = DateTime.Now;
            SelectedCustomer.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;

            //Set Customer Group Name For Show In Datagrid
            base_GuestGroupModel guestGroupModel = GuestGroupCollection.SingleOrDefault(x => x.Resource.ToString().Equals(SelectedCustomer.GroupResource));
            if (guestGroupModel != null)
                SelectedCustomer.GroupName = guestGroupModel.Name;

            SelectedCustomer.ToEntity();
            if (SelectedCustomer.AdditionalModel != null)
            {
                SelectedCustomer.AdditionalModel.ToEntity();
                if (SelectedCustomer.AdditionalModel.IsNew)
                    SelectedCustomer.base_Guest.base_GuestAdditional.Add(SelectedCustomer.AdditionalModel.base_GuestAdditional);
            }

            //Map Personal Info Or ContactCollection
            if (SelectedCustomer.PersonalInfoModel.IsDirty || SelectedCustomer.PersonalInfoModel.IsNew)//Individual
            {
                if (string.IsNullOrWhiteSpace(SelectedCustomer.PersonalInfoModel.GuestResource))
                    SelectedCustomer.PersonalInfoModel.GuestResource = SelectedCustomer.Resource.ToString();

                SelectedCustomer.PersonalInfoModel.ToEntity();
                if (SelectedCustomer.PersonalInfoModel.IsNew)
                    SelectedCustomer.base_Guest.base_GuestProfile.Add(SelectedCustomer.PersonalInfoModel.base_GuestProfile);
            }

            // Insert or update address
            // Created by Thaipn
            foreach (AddressControlModel addressControlModel in this.SelectedCustomer.AddressControlCollection.Where(x => x.IsDirty))
            {
                base_GuestAddressModel addressModel = new base_GuestAddressModel();

                // Insert new address
                if (addressControlModel.IsNew)
                {
                    addressModel.DateCreated = DateTimeExt.Now;
                    addressModel.DateUpdated = DateTimeExt.Now;
                    addressModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);

                    // Map data from model to entity
                    addressModel.ToEntity();
                    SelectedCustomer.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                    addressModel.EndUpdate();

                }
                // Update address
                else
                {
                    base_GuestAddress address = SelectedCustomer.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == addressControlModel.AddressTypeID);
                    addressModel = new base_GuestAddressModel(address);

                    addressModel.DateUpdated = DateTimeExt.Now;
                    addressModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);
                    addressModel.ToEntity();
                }

                // Update default address
                if (addressModel.IsDefault)
                    SelectedCustomer.AddressModel = addressModel;

                // Turn off IsDirty & IsNew
                addressModel.EndUpdate();

                addressControlModel.IsNew = false;
                addressControlModel.IsDirty = false;
            }
            //Save Photo
            //SavePhotoResource(SelectedCustomer);

            //Remove Payment Card
            if (SelectedCustomer.PaymentCardCollection.DeletedItems != null && SelectedCustomer.PaymentCardCollection.DeletedItems.Count > 0)
            {
                foreach (base_GuestPaymentCardModel paymentCardModel in SelectedCustomer.PaymentCardCollection.DeletedItems)
                {
                    _guestPaymentCardRepository.Delete(paymentCardModel.base_GuestPaymentCard);
                }
                SelectedCustomer.PaymentCardCollection.DeletedItems.Clear();
            }
            //Update Or Add New PaymentCard
            foreach (base_GuestPaymentCardModel guestPaymentCardModel in SelectedCustomer.PaymentCardCollection.Where(x => x.IsDirty && !x.IsTemporary))
            {
                guestPaymentCardModel.ToEntity();
                if (guestPaymentCardModel.IsNew)//new item add to entity table
                    SelectedCustomer.base_Guest.base_GuestPaymentCard.Add(guestPaymentCardModel.base_GuestPaymentCard);
            }

            if (SelectedCustomer.IsRewardMember)
            {
                _memberShipRepository.Refresh(SelectedCustomer.base_Guest.base_MemberShip);
                if (!SelectedCustomer.base_Guest.base_MemberShip.Any())
                {
                    base_MemberShipModel memberShipModel = NewMemberShipModel();
                    memberShipModel.ToEntity();
                    //Add To MemberShip Collection
                    SelectedCustomer.base_Guest.base_MemberShip.Add(memberShipModel.base_MemberShip);
                }
                else
                {
                    //Update Barcode for MemeberShip
                    short memberShipActivedStatus = (short)MemberShipStatus.Actived;
                    short membershipPendingStatus = (short)MemberShipStatus.Pending;
                    base_MemberShip membership = SelectedCustomer.base_Guest.base_MemberShip.SingleOrDefault(x => x.IsPurged == false && (x.Status.Equals(memberShipActivedStatus) || x.Status.Equals(membershipPendingStatus)));
                    if (membership != null)
                    {
                        base_MemberShipModel memberShipModel = new base_MemberShipModel(membership);
                        IdCardBarcodeGen(memberShipModel);
                        if (memberShipModel.IsDirty)
                        {
                            memberShipModel.ToEntity();
                        }
                    }
                }

                if (SelectedCustomer.GuestRewardCollection != null)
                {
                    short memebershipActivedStatus = (short)MemberShipStatus.Actived;
                    base_MemberShip membership = SelectedCustomer.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
                    if (membership != null && SelectedCustomer.MembershipValidated == null)
                        SelectedCustomer.MembershipValidated = new base_MemberShipModel(membership);

                    //Add New Or Update GuestReward
                    foreach (base_GuestRewardModel guestRewardModel in SelectedCustomer.GuestRewardCollection.Where(x => x.IsDirty))
                    {
                        guestRewardModel.ToEntity();
                        if (guestRewardModel.IsNew)
                            SelectedCustomer.base_Guest.base_GuestReward.Add(guestRewardModel.base_GuestReward);
                    }
                    if (SelectedCustomer.MembershipValidated.IsDirty)
                        SelectedCustomer.MembershipValidated.ToEntity();
                }
            }
            _guestRepository.Commit();

            //Set ID
            SetIdNEndUpdate(SelectedCustomer);
        }


        /// <summary>
        /// Save images into folder if this ImageCollection have data.
        /// </summary>
        private void SaveImage(base_ResourcePhotoModel model, string GuestNoFolder)
        {
            try
            {
                if (!System.IO.Directory.Exists(CUSTOMER_IMAGE_FOLDER))
                    System.IO.Directory.CreateDirectory(CUSTOMER_IMAGE_FOLDER);

                //Create Sub Folder For GuestNo
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(model.ImagePath);
                string guestNoFolderPath = System.IO.Path.Combine(CUSTOMER_IMAGE_FOLDER, GuestNoFolder);
                if (!System.IO.Directory.Exists(guestNoFolderPath))
                    System.IO.Directory.CreateDirectory(guestNoFolderPath);

                ///To check file on client and copy file to server
                if (fileInfo.Exists)
                {
                    ///To copy image to server
                    string filename = System.IO.Path.Combine(guestNoFolderPath, model.LargePhotoFilename);
                    System.IO.FileInfo file = new System.IO.FileInfo(filename);
                    if (!file.Exists)
                        fileInfo.CopyTo(filename, true);
                    model.ImagePath = filename;
                }
                else
                    model.ImagePath = string.Empty;
            }
            catch (Exception ex)
            {
                _log4net.Error("Save Image" + ex.ToString());
            }
        }

        /// <summary>
        /// SaveNew,Update or detete photo to resource & set photo default for guestModel
        /// </summary>
        private void SavePhotoResource(base_GuestModel guestModel)
        {
            //To remove image deleted.
            if (guestModel.PhotoCollection.DeletedItems != null
                && guestModel.PhotoCollection.DeletedItems.Count > 0)
            {
                foreach (base_ResourcePhotoModel item in guestModel.PhotoCollection.DeletedItems)
                {
                    _photoRepository.Delete(item.base_ResourcePhoto);
                }
                _photoRepository.Commit();
                guestModel.PhotoCollection.DeletedItems.Clear();
            }

            //To update image.
            //if (guestModel.PhotoCollection != null && guestModel.PhotoCollection.Count > 0)
            //{
            //    foreach (base_ResourcePhotoModel photoModel in guestModel.PhotoCollection.Where(x => x.IsDirty))
            //    {
            //        photoModel.Resource = guestModel.Resource.ToString();
            //        photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;
            //        //To map data from model to entity
            //        photoModel.ToEntity();
            //        if (photoModel.IsNew)
            //            _photoRepository.Add(photoModel.base_ResourcePhoto);

            //        //To save image to store.
            //        this.SaveImage(photoModel, guestModel.GuestNo);
            //        _photoRepository.Commit();

            //        //set Id
            //        photoModel.Id = photoModel.base_ResourcePhoto.Id;
            //    }

            //    if (guestModel.PhotoCollection.Count > 0)
            //        guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
            //    else
            //        guestModel.PhotoDefault = new base_ResourcePhotoModel();
            //}
        }

        /// <summary>
        /// Delete Image
        /// </summary>
        /// <param name="filePath"></param>
        private void DeleteImage(string filePath)
        {
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Exists)
                    fileInfo.Delete();
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private base_GuestPaymentCardModel CreateNewPaymentCard()
        {
            return new base_GuestPaymentCardModel()
             {
                 CardTypeId = 1,
                 DateCreated = DateTime.Now,
                 IsNew = true,
                 IsDirty = false
             };
        }

        /// <summary>
        /// Create new customer
        /// </summary>
        private void CreateNewCustomer()
        {
            SelectedCustomer = new base_GuestModel();
            SelectedCustomer.LastName = string.Empty;
            SelectedCustomer.FirstName = string.Empty;
            SelectedCustomer.Phone1 = string.Empty;

            //Set Account Number
            SelectedCustomer.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
            SelectedCustomer.IsActived = true;
            SetStatusItem(SelectedCustomer);

            base_RewardManager reward = _rewardManagerRepository.Get(x => true);

            //Auto enroll customer to reward member
            if (reward != null && reward.IsAutoEnroll)
                SelectedCustomer.IsRewardMember = true;
            else
                SelectedCustomer.IsRewardMember = false;

            OnPropertyChanged(() => AllowAccessReward);

            SelectedCustomer.CheckLimit = 0;
            SelectedCustomer.GuestTypeId = Common.CustomerTypes.First().Value;
            SelectedCustomer.DateCreated = DateTime.Today;
            SelectedCustomer.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedCustomer.Mark = MarkType.Customer.ToDescription();
            SelectedCustomer.Email = string.Empty;
            SelectedCustomer.IsPrimary = false;
            SelectedCustomer.Resource = Guid.NewGuid();
            SelectedCustomer.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            StickyManagementViewModel.SetParentResource(SelectedCustomer.Resource.ToString(), SelectedCustomer.ResourceNoteCollection);

            //Personal Info
            SelectedCustomer.PersonalInfoModel = new base_GuestProfileModel();
            SelectedCustomer.PersonalInfoModel.IsSpouse = false;
            SelectedCustomer.PersonalInfoModel.SEmail = string.Empty;
            SelectedCustomer.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);

            SelectedCustomer.PersonalInfoModel.IsEmergency = false;
            SelectedCustomer.PersonalInfoModel.Gender = Common.Gender.First().Value;
            SelectedCustomer.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
            SelectedCustomer.PersonalInfoModel.SGender = Common.Gender.First().Value;
            SelectedCustomer.PersonalInfoModel.IsDirty = false;

            SelectedCustomer.AdditionalModel = new base_GuestAdditionalModel();
            SelectedCustomer.AdditionalModel.SaleTaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);

            SetTaxRateFromTaxLocation(SelectedCustomer.AdditionalModel);

            SelectedCustomer.AdditionalModel.PriceSchemeId = 1;
            SelectedCustomer.AdditionalModel.Unit = 0;
            SelectedCustomer.AdditionalModel.IsDirty = false;
            SelectedCustomer.AdditionalModel.IsNoDiscount = false;
            SelectedCustomer.AdditionalModel.PropertyChanged += new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
            SelectedCustomer.AdditionalModel.IsDirty = false;

            //Create Photo Collection
            SelectedCustomer.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();

            //Create Address Collection Creaetd by Thaipn.
            SelectedCustomer.AddressControlCollection = new AddressControlCollection() { new AddressControlModel { IsNew = true, AddressTypeID = 0, IsDefault = true, IsDirty = false } };


            //Payment collection
            SelectedCustomer.PaymentCardCollection = new CollectionBase<base_GuestPaymentCardModel>();
            if (SelectedCustomer.IsRewardMember)
            {
                SelectedCustomer.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                //FilterGuestReward(SelectedCustomer);
            }
            IsForceFocused = true;


            SelectedCustomer.PropertyChanged += new PropertyChangedEventHandler(SelectedCustomer_PropertyChanged);
            OnPropertyChanged(() => AllowAccessReward);
            //Raise Check show SO History Tab
            OnPropertyChanged(() => IsVisibleSOHistory);
            SelectedCustomer.IsDirty = false;
            CustomerTabItem = CustomerFormTab.CustomerInfoTab.ToDescription();
        }


        /// <summary>
        /// Create predicate Simple Search Condition
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreateSimpleSearchPredicate(string keyword)
        {
            //Default Condition is Search All
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                //Set Condition for simple search
                predicate = PredicateBuilder.False<base_Guest>();

                // Search Customer by  Account Number
                predicate = predicate.Or(x => x.GuestNo.ToLower().Contains(keyword));

                // Search Customer by  Status 
                IEnumerable<bool> statusList = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(keyword)).Select(x => Convert.ToInt32(x.ObjValue).Is(StatusBasic.Active));
                predicate = predicate.Or(x => statusList.Contains(x.IsActived));

                // Search Customer by  FirstName
                predicate = predicate.Or(x => x.FirstName.ToLower().Contains(keyword));

                // Search Customer by  LastName
                predicate = predicate.Or(x => x.LastName.ToLower().Contains(keyword));

                // Search Customer by  Company
                predicate = predicate.Or(x => x.Company.ToLower().Contains(keyword));

                // Search Customer by Group
                IEnumerable<string> groupList = GuestGroupCollection.Where(x => x.Name.ToLower().Contains(keyword)).Select(x => x.Resource.ToString());

                if (groupList.Any())
                    predicate = predicate.Or(x => groupList.Contains(x.GroupResource));

                // Search Customer by  City
                predicate = predicate.Or(x => x.base_GuestAddress.Any(y => y.IsDefault && y.City.ToLower().Contains(keyword)));

                // Search Customer by  Country
                IEnumerable<int> countryList = Common.Countries.Where(x => x.Text.ToLower().Contains(keyword)).Select(x => Convert.ToInt32(x.ObjValue));
                if (countryList.Any())
                    predicate = predicate.Or(x => x.base_GuestAddress.Any(y => y.IsDefault && countryList.Contains(y.CountryId)));

                // Search Customer by  Phone
                predicate = predicate.Or(x => x.Phone1.ToLower().Contains(keyword));

                // Search Customer by CellPhone
                predicate = predicate.Or(x => x.CellPhone.ToLower().Contains(keyword));

                // Search Customer by  Email
                predicate = predicate.Or(x => x.Email.ToLower().Contains(keyword));

                // Search Customer by  Website
                predicate = predicate.Or(x => x.Website.ToLower().Contains(keyword));

                // Search Customer by  custom column
                predicate = predicate.Or(x => x.Custom1.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom2.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom3.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom4.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom5.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom6.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom7.ToLower().Contains(keyword));
                predicate = predicate.Or(x => x.Custom8.ToLower().Contains(keyword));


                decimal decimalValue = 0;

                if (decimal.TryParse(keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue))
                {
                    //Total Purchase
                    predicate = predicate.Or(x => x.PurchaseDuringTrackingPeriod.Equals(decimalValue));

                    //TotalReward

                    //TotalReedem
                    predicate = predicate.Or(x => x.TotalRewardRedeemed.Equals(decimalValue));
                }

            }
            return predicate;
        }

        /// <summary>
        /// Check Customer is duplicate
        /// <para>Value Compare : Email,Phone1</para>
        /// </summary>
        /// 
        /// <param name="customerModel"></param>
        /// 
        /// <returns></returns>
        private bool CheckDuplicateCustomer(base_GuestModel customerModel)
        {
            bool result = false;
            IQueryable<base_Guest> query = _guestRepository.GetIQueryable(x => x.Resource != customerModel.Resource
                                                                               && x.Mark.Equals(CUSTOMER_MARK)
                                                                               && !x.IsPurged
                                                                               && (x.Phone1.Equals(customerModel.Phone1)
                                                                               || (x.Email != null && x.Email != string.Empty && x.Email.ToLower().Equals(customerModel.Email.ToLower()))));

            if (query.Count() > 0)
            {
                result = true;
                MessageBoxResult resultMsg = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("CUS_MSG_ExisedCustViewProfile"), Language.POS, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (MessageBoxResult.Yes.Is(resultMsg))
                {
                    base_GuestModel guestModel = new base_GuestModel(query.FirstOrDefault());
                    if (guestModel.base_Guest.base_GuestProfile.Count > 0)
                        guestModel.PersonalInfoModel = new base_GuestProfileModel(guestModel.base_Guest.base_GuestProfile.FirstOrDefault());
                    else
                        guestModel.PersonalInfoModel = new base_GuestProfileModel();
                    ViewProfileViewModel viewProfileViewModel = new ViewProfileViewModel();
                    viewProfileViewModel.GuestModel = guestModel;
                    _dialogService.ShowDialog<ViewProfile>(_ownerViewModel, viewProfileViewModel, Language.GetMsg("CUS_MSG_ViewProfile"));
                }
            }
            return result;
        }

        private bool CheckDuplicateGuestNo(base_GuestModel customerModel)
        {
            bool result = false;
            try
            {
                lock (UnitOfWork.Locker)
                {
                    IQueryable<base_Guest> query = _guestRepository.GetIQueryable(x => x.Mark == CUSTOMER_MARK && x.Resource != customerModel.Resource && x.GuestNo.Equals(customerModel.GuestNo));
                    if (query.Any())
                        result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                _log4net.Error(ex);
            }
            customerModel.IsDuplicateGuestNo = result;
            return result;
        }

        /// <summary>
        /// Load Reaward Program
        /// </summary>
        private void LoadRewardProgram()
        {
            base_RewardManager reward = _rewardManagerRepository.Get(x => true);
            if (reward != null)
                _rewardManagerRepository.Refresh(reward);

            if (reward != null)
                RewardProgram = new base_RewardManagerModel(reward);
            else
                RewardProgram = new base_RewardManagerModel();
            RewardProgram.RewardInfo = RewardProgram.ToString();
        }

        /// <summary>
        /// Method check Item has edit & show message
        /// </summary>
        /// <returns></returns>
        private bool ChangeViewExecute(bool? isClosing)
        {
            bool result = true;
            if (this.IsDirty)
            {
                MessageBoxResult msgResult = MessageBoxResult.None;
                msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("M112"), Language.POS, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {
                        //if (SaveCustomer())
                        result = SaveCustomer();
                    }
                    else //Has Error
                        result = false;

                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
                else if (msgResult.Is(MessageBoxResult.No))
                {
                    if (SelectedCustomer.IsNew)
                    {
                        // Remove all popup sticky
                        StickyManagementViewModel.DeleteAllResourceNote();

                        SelectedCustomer = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;

                    }
                    else //Old Item Rollback data
                    {
                        // Close all popup sticky
                        StickyManagementViewModel.CloseAllPopupSticky();

                        SelectedCustomer.ToModelAndRaise();
                        SetDataToModel(SelectedCustomer, true);
                    }
                }
                else
                    result = false;
            }
            else
            {
                if (SelectedCustomer != null && SelectedCustomer.IsNew)
                {
                    // Remove all popup sticky
                    StickyManagementViewModel.DeleteAllResourceNote();
                }
                else
                {
                    // Close all popup sticky
                    StickyManagementViewModel.CloseAllPopupSticky();
                }
            }
            return result;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadDataByPredicate(Expression<Func<base_Guest, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                CustomerCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;

                predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(CUSTOMER_MARK));

                //Cout all Customer in Data base show on grid
                TotalCustomers = _guestRepository.GetIQueryable(predicate).Count();

                //Get data with range
                IList<base_Guest> customers = _guestRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Guest customer in customers)
                {
                    bgWorker.ReportProgress(0, customer);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_Guest guest = _guestRepository.Refresh((base_Guest)e.UserState);
                base_GuestModel customerModel = new base_GuestModel(guest);
                SetDataToModel(customerModel);
                CustomerCollection.Add(customerModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                //Refresh item is selected
                if (_viewExisted && SelectedCustomer != null && !IsSearchMode && !SelectedCustomer.IsNew)//current item selected in edit form & is not a new item
                {
                    //Get From Current Collection
                    if (CustomerCollection.Any(x => x.Id.Equals(SelectedCustomer.Id)))
                    {
                        SelectedCustomer = CustomerCollection.SingleOrDefault(x => x.Id.Equals(SelectedCustomer.Id));

                        SetDataToModel(SelectedCustomer, true);

                        EditItem(SelectedCustomer);

                        //Update Collection if User Selected tab Reward Or SO Hisotory
                        CustomerTabChanged(CustomerTabItem);
                    }
                    else
                    {
                        //Get from db if collection not existed item
                        base_Guest customer = _guestRepository.Get(x => x.Id.Equals(SelectedCustomer.Id));
                        if (customer != null)
                        {
                            _selectedCustomer = new base_GuestModel(customer);
                            SetDataToModel(_selectedCustomer, true);
                            EditItem(_selectedCustomer);
                            CustomerTabChanged(CustomerTabItem);
                            OnPropertyChanged(() => SelectedCustomer);

                            //Update Collection if User Selected tab Reward Or SO Hisotory
                            CustomerTabChanged(CustomerTabItem);
                        }
                        else
                        {
                            //Not any item existed , change to Grid Search
                            IsSearchMode = true;
                        }
                    }
                }
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Set data & relation of customer to model
        /// <para>Using for rollback data or Set data on first load</para>
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged">Raise Property Changed when rollback data</param>
        private void SetDataToModel(base_GuestModel customerModel, bool RaisePropertyChanged = false)
        {
            //Load Customer Infomation
            base_GuestGroupModel guestGroupModel = GuestGroupCollection.SingleOrDefault(x => x.Resource.ToString().Equals(customerModel.GroupResource));
            if (guestGroupModel != null)
                customerModel.GroupName = guestGroupModel.Name;
            //Set Status Item
            SetStatusItem(customerModel);
            //Load PhotoCollection
            LoadResourcePhoto(customerModel);

            // Load DefaultAdress Address (Binding in main dtgrid)
            LoadAddress(customerModel, RaisePropertyChanged);

            //load customer membership model
            LoadMemberShipModel(customerModel);
            customerModel.IsDirty = false;
        }


        /// <summary>
        /// Load Relation Property with customer model
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged"></param>
        private void LoadRelationCustomerModel(base_GuestModel customerModel, bool RaisePropertyChanged)
        {
            // ==== Load PersonalInfoModel ====
            LoadPersonalInfoModel(customerModel, RaisePropertyChanged);

            //==== Load AdditionalModel ====
            LoadGuestAdditional(customerModel, RaisePropertyChanged);

            //Load PhotoCollection                                                    
            LoadResourcePhoto(customerModel);

            //====Load PaymentCard ====
            LoadPayment(customerModel, RaisePropertyChanged);

            // Load resource note collection
            LoadResourceNoteCollection(customerModel);

        }

        /// <summary>
        /// LoadPersional Info Model
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged"></param>
        private void LoadPersonalInfoModel(base_GuestModel customerModel, bool RaisePropertyChanged)
        {
            _guestProfileRepository.Refresh(customerModel.base_Guest.base_GuestProfile);
            if (customerModel.base_Guest.base_GuestProfile.Count > 0)
                customerModel.PersonalInfoModel = new base_GuestProfileModel(customerModel.base_Guest.base_GuestProfile.First(), RaisePropertyChanged);
            else
            {
                customerModel.PersonalInfoModel = new base_GuestProfileModel();
                customerModel.PersonalInfoModel.SEmail = string.Empty;
                customerModel.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);
                customerModel.PersonalInfoModel.IsSpouse = false;
                customerModel.PersonalInfoModel.IsEmergency = false;
                customerModel.PersonalInfoModel.Gender = Common.Gender.First().Value;
                customerModel.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
                customerModel.PersonalInfoModel.SGender = Common.Gender.First().Value;
                customerModel.PersonalInfoModel.IsDirty = false;
            }
        }

        /// <summary>
        /// Load Guest Additional
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged"></param>
        private void LoadGuestAdditional(base_GuestModel customerModel, bool RaisePropertyChanged)
        {
            _guestAdditionalRepository.Refresh(customerModel.base_Guest.base_GuestAdditional);
            if (customerModel.base_Guest.base_GuestAdditional.Count > 0)
            {
                base_GuestAdditional customerAdditional = customerModel.base_Guest.base_GuestAdditional.First();
                customerModel.AdditionalModel = new base_GuestAdditionalModel(customerAdditional, RaisePropertyChanged);

                //Set value for radiobutton "Pricing Level" (No discount / Fixed Discount / Markdown Price Level)
                if (customerModel.AdditionalModel.PriceSchemeId > 0)
                    customerModel.AdditionalModel.PriceLevelType = (int)PriceLevelType.MarkdownPriceLevel;//Set radio button is Mark down Price Level
                else if (customerModel.AdditionalModel.FixDiscount != null && customerModel.AdditionalModel.FixDiscount > 0)
                    customerModel.AdditionalModel.PriceLevelType = (int)PriceLevelType.FixedDiscountOnAllItems;//Set radio button is Fixed Discount
                else
                    customerModel.AdditionalModel.PriceLevelType = (int)PriceLevelType.NoDiscount;//Set radio Button is No Discount
            }
            else
            {
                customerModel.AdditionalModel = new base_GuestAdditionalModel();
                customerModel.AdditionalModel.PriceSchemeId = 1;
                customerModel.AdditionalModel.Unit = 0;
                customerModel.AdditionalModel.PriceLevelType = 0;
                customerModel.AdditionalModel.TaxRate = 0;
            }

            if (customerModel.AdditionalModel != null)
            {
                customerModel.AdditionalModel.PropertyChanged -= new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
                customerModel.AdditionalModel.PropertyChanged += new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
            }
            customerModel.AdditionalModel.IsDirty = false;
        }

        /// <summary>
        /// Load Reward data of reward member
        /// <para> Load when user click on tab</para>
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadRewardMemberData(base_GuestModel customerModel, bool isForced = false)
        {
            if (isForced || (customerModel.IsRewardMember && customerModel.GuestRewardCollection == null || !customerModel.GuestRewardCollection.Any()))
            {
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                customerModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                bgWorker.DoWork += (sender, e) =>
                {
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    //Get data with range
                    IOrderedEnumerable<base_GuestReward> guestRewards = customerModel.base_Guest.base_GuestReward.OrderBy(x => x.EarnedDate);
                    //_guestRepository.Refresh(customers);
                    foreach (base_GuestReward guestReward in guestRewards)
                    {
                        bgWorker.ReportProgress(0, guestReward);
                    }
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    base_GuestRewardModel guestRewardModel = new base_GuestRewardModel((base_GuestReward)e.UserState);
                    guestRewardModel.StatusItem = Common.GuestRewardStatus.SingleOrDefault(x => x.Value.Equals(guestRewardModel.Status.Value));
                    customerModel.GuestRewardCollection.Add(guestRewardModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    if (customerModel.GuestRewardCollection != null)
                    {
                        customerModel.TotalOfRewardRedeemed = customerModel.base_Guest.base_GuestReward.Count(x => x.TotalRewardRedeemed > 0 && !x.Status.Value.Is(GuestRewardStatus.Removed));
                        customerModel.TotalRewardsAvaliable = customerModel.base_Guest.base_GuestReward.Count(x => !x.Status.Value.Is(GuestRewardStatus.Redeemed) && !x.Status.Value.Is(GuestRewardStatus.Removed) && (x.ExpireDate.HasValue && x.ExpireDate.Value <= DateTime.Today));
                        FilterGuestReward(customerModel);
                    }

                    IsBusy = false;
                };
                bgWorker.RunWorkerAsync();
            }
        }


        /// <summary>
        /// Load Payment Data
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged"></param>
        private void LoadPayment(base_GuestModel customerModel, bool RaisePropertyChanged)
        {
            customerModel.PaymentCardCollection = new CollectionBase<base_GuestPaymentCardModel>();
            _guestPaymentCardRepository.Refresh(customerModel.base_Guest.base_GuestPaymentCard);
            foreach (base_GuestPaymentCard guestPaymentCard in customerModel.base_Guest.base_GuestPaymentCard)
            {
                base_GuestPaymentCardModel paymentCardModel = new base_GuestPaymentCardModel(guestPaymentCard, RaisePropertyChanged);
                paymentCardModel.ExpDate = string.Format("{0}/{1}", guestPaymentCard.ExpMonth, guestPaymentCard.ExpYear);
                paymentCardModel.IsTemporary = false;
                paymentCardModel.IsDirty = false;
                customerModel.PaymentCardCollection.Add(paymentCardModel);
            }
        }

        /// <summary>
        /// Load AddressData
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged"></param>
        private void LoadAddress(base_GuestModel customerModel, bool RaisePropertyChanged)
        {
            //_guestAddressRepository.Refresh(customerModel.base_Guest.base_GuestAddress);
            if (customerModel.base_Guest.base_GuestAddress.Count > 0)
                customerModel.AddressModel = new base_GuestAddressModel(customerModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.IsDefault), RaisePropertyChanged);

            customerModel.AddressCollection = new ObservableCollection<base_GuestAddressModel>();
            //AddressCollection For Control
            customerModel.AddressControlCollection = new AddressControlCollection();

            foreach (base_GuestAddress guestAddress in customerModel.base_Guest.base_GuestAddress)
            {
                _guestAddressRepository.Refresh(guestAddress);
                //Add To Address Collection
                base_GuestAddressModel guestAddressModel = new base_GuestAddressModel(guestAddress);
                customerModel.AddressCollection.Add(guestAddressModel);

                //Add to Address For Control
                AddressControlModel addressControlModel = guestAddressModel.ToAddressControlModel();
                addressControlModel.IsDirty = false;
                customerModel.AddressControlCollection.Add(addressControlModel);
            }
        }

        /// <summary>
        /// Load Resource Photo Collection & DefaultPhoto for GuestModel
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourcePhoto(base_GuestModel guestModel)
        {
            if (guestModel.PhotoCollection == null)
            {
                guestModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
                if (guestModel.Picture != null && guestModel.Picture.Length > 0)
                {
                    base_ResourcePhotoModel ResourcePhotoModel = new base_ResourcePhotoModel();
                    ResourcePhotoModel.ImageBinary = guestModel.Picture;
                    ResourcePhotoModel.IsDirty = false;
                    ResourcePhotoModel.IsNew = false;
                    guestModel.PhotoCollection.Add(ResourcePhotoModel);
                }
            }
        }

        /// <summary>
        /// Load sale order collection
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadSaleOrderCollection(base_GuestModel customerModel)
        {
            if (customerModel.SaleOrderCollection == null || !customerModel.GuestRewardCollection.Any())
            {
                // Initial sale order collection
                string customerGuid = customerModel.Resource.ToString();
                customerModel.SaleOrderCollection = new ObservableCollection<base_SaleOrderModel>();
                BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
                customerModel.GuestRewardCollection = new CollectionBase<base_GuestRewardModel>();
                bgWorker.DoWork += (sender, e) =>
                {
                    if (Define.DisplayLoading)
                        IsBusy = true;

                    IEnumerable<base_SaleOrder> saleOrders = _saleOrderRepository.GetAll(x => x.CustomerResource.Equals(customerGuid));
                    foreach (base_SaleOrder saleOrder in saleOrders)
                    {
                        bgWorker.ReportProgress(0, saleOrder);
                    }
                };

                bgWorker.ProgressChanged += (sender, e) =>
                {
                    base_SaleOrderModel saleOrderModel = new base_SaleOrderModel((base_SaleOrder)e.UserState);
                    saleOrderModel.ItemStatus = Common.StatusSalesOrders.SingleOrDefault(x => Convert.ToInt16(x.ObjValue).Equals(saleOrderModel.OrderStatus));
                    customerModel.SaleOrderCollection.Add(saleOrderModel);
                };

                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    TotalSaleOrder = new base_SaleOrderModel
                    {
                        Total = customerModel.SaleOrderCollection.Sum(x => x.Total),
                        Paid = customerModel.SaleOrderCollection.Sum(x => x.Paid),
                        Balance = customerModel.SaleOrderCollection.Sum(x => x.Balance)

                    };

                    IsBusy = false;
                };
                bgWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Load Custom Field from db
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadCustomFieldCollection()
        {
            IEnumerable<base_CustomField> customerFields = _customFieldRepository.GetAll().Where(x => x.Mark.Equals(CUSTOMER_MARK));
            _customFieldRepository.Refresh(customerFields);
            if (customerFields != null && customerFields.Any())
            {
                CustomFieldCollection = new ObservableCollection<base_CustomFieldModel>(customerFields.OrderBy(x => x.Id).Select(x => new base_CustomFieldModel(x)));
            }
            else
            {
                CustomFieldCollection = new ObservableCollection<base_CustomFieldModel>();
                for (int i = 1; i < 9; i++)
                {
                    base_CustomFieldModel customFieldModel = new base_CustomFieldModel();
                    customFieldModel.Mark = CUSTOMER_MARK;
                    customFieldModel.FieldName = "Custom " + i;
                    customFieldModel.IsShow = true;
                    customFieldModel.Label = customFieldModel.FieldName;
                    customFieldModel.ToEntity();
                    //insert to db
                    _customFieldRepository.Add(customFieldModel.base_CustomField);
                    customFieldModel.EndUpdate();
                    //Add To Collection
                    CustomFieldCollection.Add(customFieldModel);
                }
                _customFieldRepository.Commit();
            }
        }

        /// <summary>
        /// Load Membership to Model
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadMemberShipModel(base_GuestModel customerModel)
        {
            if (!customerModel.IsRewardMember)
                return;

            short memebershipActivedStatus = (short)MemberShipStatus.Actived;
            base_MemberShip membership = customerModel.base_Guest.base_MemberShip.FirstOrDefault(x => x.Status.Equals(memebershipActivedStatus));
            if (membership != null)
            {
                _memberShipRepository.Refresh(membership);
                customerModel.MembershipValidated = new base_MemberShipModel(membership);
                switch (customerModel.MembershipValidated.MemberType)
                {
                    case "B":
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("RewardBroze") as System.Windows.Media.Brush);
                        break;
                    case "S":
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("RewardSilver") as System.Windows.Media.Brush);
                        break;
                    case "G":
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("RewardGold") as System.Windows.Media.Brush);
                        break;
                    case "P":
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("RewardPlatinum") as System.Windows.Media.Brush);
                        break;
                    case "D":
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("RewardDiamond") as System.Windows.Media.Brush);
                        break;
                    default:
                        customerModel.MembershipValidated.MemberShipIcon = (App.Current.FindResource("IssueReward") as System.Windows.Media.Brush);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerModel"></param>
        private void FilterGuestReward(base_GuestModel customerModel)
        {
            if (customerModel != null && customerModel.IsRewardMember && customerModel.GuestRewardCollection != null)
            {
                if (_rewardCollectionView == null)
                    _rewardCollectionView = CollectionViewSource.GetDefaultView(customerModel.GuestRewardCollection);

                _rewardCollectionView.Filter = obj =>
                {
                    bool result = true;
                    base_GuestRewardModel guestRewardModel = obj as base_GuestRewardModel;
                    if (!StartDateReward.HasValue && !EndDateReward.HasValue)
                        result = true;
                    else
                    {
                        if (StartDateReward.HasValue)
                        {
                            result &= (guestRewardModel.EarnedDate >= StartDateReward.Value);
                        }
                        if (EndDateReward.HasValue)
                        {
                            result &= (guestRewardModel.EarnedDate <= EndDateReward.Value);
                        }
                    }
                    return result;
                };

                customerModel.TotalRewardHistory = _rewardCollectionView.OfType<object>().Count();
            }
        }

        /// <summary>
        /// Generate Barcode with EAN13 Format (13 digit numberic)
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        private void IdCardBarcodeGen(base_MemberShipModel memberShipModel)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                //GenMemberShipCard
                if (string.IsNullOrWhiteSpace(memberShipModel.IdCard))
                {
                    using (BarcodeLib.Barcode barCode = new BarcodeLib.Barcode())
                    {
                        barCode.IncludeLabel = true;
                        string idCard = currentDate.ToString("yyMMddHHmmss");
                        barCode.Encode(BarcodeLib.TYPE.EAN13, idCard, 200, 70);
                        memberShipModel.IdCardImg = barCode.Encoded_Image_Bytes;
                        memberShipModel.IdCard = barCode.RawData;
                    }
                }

            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
            }
        }

        /// <summary>
        /// Create New Member Ship Model
        /// </summary>
        /// <returns></returns>
        private base_MemberShipModel NewMemberShipModel()
        {
            base_MemberShipModel memberShipModel = new base_MemberShipModel();
            memberShipModel.Status = (short)MemberShipStatus.Actived;
            memberShipModel.GuestResource = SelectedCustomer.Resource.ToString();
            memberShipModel.DateCreated = DateTime.Today;
            memberShipModel.DateUpdated = DateTime.Now;
            memberShipModel.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            memberShipModel.IsPurged = false;
            memberShipModel.Status = Convert.ToInt16(MemberShipStatus.Actived);
            memberShipModel.MemberType = MemberShipType.Normal.ToDescription();

            IdCardBarcodeGen(memberShipModel);
            return memberShipModel;
        }

        /// <summary>
        /// Copy Infomation from another Customer
        /// </summary>
        /// <param name="guestModel"></param>
        public base_GuestModel CopyFrom(base_GuestModel guestModel)
        {
            base_GuestModel customerModel = new base_GuestModel();
            customerModel.CopyAllFrom(guestModel);

            customerModel.IdCard = string.Empty;
            customerModel.IdCardImg = null;
            customerModel.FirstName = guestModel.FirstName + "(Copy)";
            customerModel.MiddleName = guestModel.MiddleName;
            customerModel.LastName = guestModel.LastName + "(Copy)";
            customerModel.Resource = Guid.NewGuid();
            customerModel.AccountNumber = string.Empty;
            customerModel.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat); ;
            customerModel.Remark = string.Empty;
            customerModel.Phone1 = string.Empty;//guestModel.Phone1;
            customerModel.Ext1 = string.Empty;//guestModel.Ext1;
            customerModel.Email = string.Empty;//guestModel.Email;
            customerModel.TotalRewardRedeemed = 0;
            customerModel.PurchaseDuringTrackingPeriod = 0;
            customerModel.RequirePurchaseNextReward = 0;
            customerModel.StatusItem = guestModel.StatusItem;

            customerModel.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            customerModel.UserUpdated = string.Empty;
            customerModel.DateCreated = DateTime.Now;
            customerModel.DateUpdated = DateTime.Now;

            //Relation
            customerModel.PersonalInfoModel = new base_GuestProfileModel();
            customerModel.PersonalInfoModel.IsSpouse = false;
            customerModel.PersonalInfoModel.SEmail = string.Empty;
            customerModel.PersonalInfoModel.DOB = DateTime.Today.AddYears(-10);

            customerModel.PersonalInfoModel.IsEmergency = false;
            customerModel.PersonalInfoModel.Gender = Common.Gender.First().Value;
            customerModel.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
            customerModel.PersonalInfoModel.SGender = Common.Gender.First().Value;
            customerModel.PersonalInfoModel.IsDirty = false;

            customerModel.AdditionalModel = new base_GuestAdditionalModel();
            //customerModel.AdditionalModel.TaxInfoType = (int)TaxInfoType.ResellerTaxNo;
            customerModel.AdditionalModel.SaleTaxLocation = Convert.ToInt32(Define.CONFIGURATION.DefaultSaleTaxLocation);

            SetTaxRateFromTaxLocation(customerModel.AdditionalModel);
            customerModel.AdditionalModel.PriceSchemeId = 1;
            customerModel.AdditionalModel.Unit = 0;
            customerModel.AdditionalModel.IsNoDiscount = false;
            customerModel.AdditionalModel.IsDirty = false;
            customerModel.Id = 0;
            //Address Collection
            customerModel.AddressControlCollection = new AddressControlCollection();
            if (guestModel.AddressControlCollection.Any())
            {
                foreach (AddressControlModel addressControlModel in guestModel.AddressControlCollection)
                {
                    AddressControlModel newAddressControlModel = new AddressControlModel();
                    newAddressControlModel.AddressID = 0;
                    newAddressControlModel.AddressTypeID = addressControlModel.AddressTypeID;
                    newAddressControlModel.AddressLine1 = addressControlModel.AddressLine1;
                    newAddressControlModel.AddressLine2 = addressControlModel.AddressLine2;
                    newAddressControlModel.City = addressControlModel.City;
                    newAddressControlModel.StateProvinceID = addressControlModel.StateProvinceID;
                    newAddressControlModel.PostalCode = addressControlModel.PostalCode;
                    newAddressControlModel.CountryID = addressControlModel.CountryID;
                    newAddressControlModel.IsDefault = addressControlModel.IsDefault;
                    newAddressControlModel.IsNew = true;
                    newAddressControlModel.IsDirty = true;
                    customerModel.AddressControlCollection.Add(newAddressControlModel);
                }
            }

            //Contact Collection
            customerModel.ContactCollection = new CollectionBase<base_GuestModel>();
            if (guestModel.ContactCollection != null && guestModel.ContactCollection.Any())
            {
                foreach (base_GuestModel contactModel in guestModel.ContactCollection.Where(x => !x.IsTemporary))
                {
                    base_GuestModel newContactModel = new base_GuestModel();

                    newContactModel.CopyAllFrom(contactModel);
                    newContactModel.Id = 0;
                    newContactModel.Resource = Guid.NewGuid();
                    newContactModel.Mark = MarkType.Contact.ToDescription();
                    newContactModel.DateCreated = DateTime.Now;
                    newContactModel.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
                    newContactModel.IsPrimary = guestModel.ContactCollection.Any(x => x.IsPrimary == true && !x.GuestNo.Equals(newContactModel.GuestNo)) ? false : true;
                    newContactModel.IsDirty = false;
                    newContactModel.IsTemporary = false;
                    customerModel.ContactCollection.Add(newContactModel);
                }
            }

            //Photo Resource
            customerModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            base_ResourcePhotoModel newPhotoModel = new base_ResourcePhotoModel();
            newPhotoModel.IsNew = true;
            newPhotoModel.IsDirty = true;
            newPhotoModel.ImageBinary = customerModel.Picture;
            customerModel.PhotoCollection.Add(newPhotoModel);
            if (customerModel.PhotoCollection.Count > 0)
                customerModel.PhotoDefault = customerModel.PhotoCollection.FirstOrDefault();
            else
                customerModel.PhotoDefault = new base_ResourcePhotoModel();
            //Note
            customerModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            StickyManagementViewModel.SetParentResource(customerModel.Resource.ToString(), customerModel.ResourceNoteCollection);
            //PaymentCard
            customerModel.PaymentCardCollection = new CollectionBase<base_GuestPaymentCardModel>();
            return customerModel;
        }

        /// <summary>
        /// Edit Item 
        /// </summary>
        /// <param name="param"></param>
        private void EditItem(base_GuestModel guestModel)
        {
            _selectedCustomer = guestModel;

            LoadRelationCustomerModel(SelectedCustomer, true);

            SelectedCustomer.PropertyChanged -= new PropertyChangedEventHandler(SelectedCustomer_PropertyChanged);
            SelectedCustomer.PropertyChanged += new PropertyChangedEventHandler(SelectedCustomer_PropertyChanged);

            if (SelectedCustomer.AdditionalModel != null)
                SetValueWhenTaxExemptionNResellerTaxChange(SelectedCustomer.AdditionalModel);

            //Reset filter
            _rewardCollectionView = null;

            //Filter
            FilterGuestReward(SelectedCustomer);

            OnPropertyChanged(() => ContactTotalItem);
            OnPropertyChanged(() => CreditCardTotalItem);
            OnPropertyChanged(() => AllowAccessReward);


            StickyManagementViewModel.SetParentResource(SelectedCustomer.Resource.ToString(), SelectedCustomer.ResourceNoteCollection);

            OnPropertyChanged(() => SelectedCustomer);

            IsSearchMode = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCardModel"></param>
        private void OpenCreditCardView(base_GuestPaymentCardModel paymentCardModel)
        {
            string titlePopup = paymentCardModel.IsNew ? Language.GetMsg("CUS_Title_CreateCreditCard") : Language.GetMsg("CUS_Title_UpdateCreditCard");
            CreditCardViewModel creditCardViewModel = new CreditCardViewModel(SelectedCustomer);
            creditCardViewModel.PaymentCardModel = paymentCardModel;
            bool? result = _dialogService.ShowDialog<CreditCardView>(_ownerViewModel, creditCardViewModel, titlePopup);
            if (result == true)
                if (paymentCardModel.IsNew)
                    this.SelectedCustomer.PaymentCardCollection.Add(creditCardViewModel.PaymentCardModel);
            OnPropertyChanged(() => CreditCardTotalItem);
        }

        private void OpenEditCardView(base_GuestPaymentCardModel paymentCardModel)
        {
            string titlePopup = paymentCardModel.IsNew ? Language.GetMsg("CUS_Title_CreateCreditCard") : Language.GetMsg("CUS_Title_UpdateCreditCard");
            CreditCardViewModel creditCardViewModel = new CreditCardViewModel(SelectedCustomer);
            creditCardViewModel.PaymentCardModel = paymentCardModel;
            bool? result = _dialogService.ShowDialog<CreditCardView>(_ownerViewModel, creditCardViewModel, titlePopup);
            OnPropertyChanged(() => CreditCardTotalItem);
        }

        /// <summary>
        /// Customer Tab Changed
        /// </summary>
        /// <param name="_customerTabIndex"></param>
        private void CustomerTabChanged(string _customerTabItem)
        {
            if (SelectedCustomer == null || string.IsNullOrWhiteSpace(_customerTabItem as string))
                return;

            switch (_customerTabItem)
            {
                case "CustomerInfoTab":
                    break;
                case "PersonalInformationTab":

                    break;
                case "AdditionalInfoTab":

                    break;
                case "PaymentTab":
                    break;
                case "RewardTab":
                    LoadRewardMemberData(SelectedCustomer);
                    break;
                case "SOHistoryTab":
                    // Load sale order collection
                    LoadSaleOrderCollection(SelectedCustomer);
                    break;

            }
        }

        /// <summary>
        /// Get & Set Tax Rate from Tax location fro Customer Additional
        /// </summary>
        /// <param name="additionalModel"></param>
        private void SetTaxRateFromTaxLocation(base_GuestAdditionalModel additionalModel)
        {
            if (additionalModel.SaleTaxLocation != 0)
            {
                additionalModel.TaxRate = 0;
                base_SaleTaxLocation taxCode = AllSaleTax.FirstOrDefault(x => x.ParentId == additionalModel.SaleTaxLocation && x.TaxCode.Equals(Define.CONFIGURATION.DefaultTaxCodeNewDepartment));

                //This TaxLocation has only one TaxCode &  this sale Code is Single or Price
                if (taxCode != null && taxCode.TaxOption != (int)SalesTaxOption.Multi && taxCode.base_SaleTaxLocationOption.Any())
                    additionalModel.TaxRate = taxCode.base_SaleTaxLocationOption.FirstOrDefault().TaxRate;
                else
                    additionalModel.TaxRate = 0;
            }
            else
            {
                additionalModel.TaxRate = 0;
            }
        }

        /// <summary>
        /// Set name is "No Tax" when IsTaxExemption true, or Set name is "Tax" TaxExemptionNo has value 
        /// </summary>
        /// <param name="additionalModel"></param>
        private void SetValueWhenTaxExemptionNResellerTaxChange(base_GuestAdditionalModel additionalModel)
        {
            if (SaleTaxCollection != null && SaleTaxCollection.Any(x => x.ParentId == 0 && x.Id == 0))
            {
                base_SaleTaxLocationModel saleTaxLocationModel = SaleTaxCollection.SingleOrDefault(x => x.ParentId == 0 && x.Id == 0);
                if (additionalModel.IsTaxExemption)
                {
                    saleTaxLocationModel.Name = "Tax";
                }
                else if (!string.IsNullOrWhiteSpace(additionalModel.TaxExemptionNo))
                {
                    saleTaxLocationModel.Name = "None Tax";
                }
                else
                {
                    saleTaxLocationModel.Name = string.Empty;
                }
            }

        }

        /// <summary>
        /// Set Id after save to server
        /// </summary>
        /// <param name="customerModel"></param>
        private void SetIdNEndUpdate(base_GuestModel customerModel)
        {
            //Set ID
            customerModel.ToModelAndRaise();
            customerModel.EndUpdate();

            //Isnew : add null PersonalInfoModel to server => is dirty not turn on
            if (customerModel.PersonalInfoModel != null && (customerModel.PersonalInfoModel.IsDirty || customerModel.PersonalInfoModel.IsNew))
            {
                customerModel.PersonalInfoModel.ToModelAndRaise();
                customerModel.PersonalInfoModel.EndUpdate();
            }
            //Isnew : add null addition to server => is dirty not turn on
            if (customerModel.AdditionalModel != null && (customerModel.AdditionalModel.IsDirty || customerModel.PersonalInfoModel.IsNew))
            {
                customerModel.AdditionalModel.ToModelAndRaise();
                customerModel.AdditionalModel.EndUpdate();
            }


            if (customerModel.IsRewardMember)
            {
                LoadMemberShipModel(customerModel);

                if (SelectedCustomer.MembershipValidated != null && SelectedCustomer.MembershipValidated.IsDirty)
                {
                    SelectedCustomer.MembershipValidated.ToModel();
                    SelectedCustomer.MembershipValidated.EndUpdate();
                }
                if (customerModel.GuestRewardCollection != null)
                {
                    foreach (base_GuestRewardModel guestRewardModel in customerModel.GuestRewardCollection.Where(x => x.IsDirty || x.IsNew))
                    {
                        guestRewardModel.ToModelAndRaise();
                        guestRewardModel.EndUpdate();
                    }
                }
            }

            if (customerModel.PaymentCardCollection != null)
            {
                foreach (base_GuestPaymentCardModel paymentCardModel in customerModel.PaymentCardCollection.Where(x => (x.IsDirty || x.IsNew) && !x.IsTemporary))
                {
                    paymentCardModel.ToModel();
                    paymentCardModel.EndUpdate();
                }
            }

            if (customerModel.PhotoCollection != null)
            {
                foreach (base_ResourcePhotoModel photoModel in customerModel.PhotoCollection.Where(x => x.IsDirty))
                {
                    photoModel.ToModelAndRaise();
                    photoModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Set Status for display in grid
        /// </summary>
        /// <param name="customerModel"></param>
        private void SetStatusItem(base_GuestModel customerModel)
        {
            int status = customerModel.IsActived ? 1 : 2;
            customerModel.StatusItem = Common.StatusBasic.SingleOrDefault(x => x.Value.Equals((short)status));
        }

        /// <summary>
        /// Open Search Advanced
        /// </summary>
        private void OpenSearchAdvance()
        {
            CustomerAdvanceSearchViewModel viewModel = new CustomerAdvanceSearchViewModel();
            bool? dialogResult = _dialogService.ShowDialog<CustomerAdvanceSearchView>(_ownerViewModel, viewModel, Language.GetMsg("C104"));
            if (dialogResult ?? false)
            {
                IsAdvanced = true;
                this.AdvanceSearchPredicate = viewModel.AdvanceSearchPredicate;
                LoadDataByPredicate(this.AdvanceSearchPredicate, false, 0);
            }
        }
        #endregion

        #region Override Methods

        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {
            POSConfig = Define.CONFIGURATION;

            LoadRewardProgram();

            LoadCustomFieldCollection();

            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load with Search Condition
            {
                predicate = CreateSimpleSearchPredicate(FilterText);
            }

            LoadDataByPredicate(predicate, true);
            _viewExisted = true;
        }

        /// <summary>
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            if (IsBusy)
                return false;
            return ChangeViewExecute(isClosing);
        }

        /// <summary>
        /// Change view from Ribbon
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ChangeViewExecute(null))
                {
                    if (!isList)
                    {
                        this.CreateNewCustomer();
                        IsSearchMode = false;
                    }
                    else
                        IsSearchMode = true;
                }
            }
        }

        #endregion

        #region PropertyChanged
        private void SelectedCustomer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_GuestModel customerModel = sender as base_GuestModel;

            switch (e.PropertyName)
            {
                case "GuestNo":
                    CheckDuplicateGuestNo(customerModel);
                    break;
                case "IsRewardMember":
                    OnPropertyChanged(() => AllowAccessReward);

                    break;
                case "IsActived":
                    SetStatusItem(customerModel);
                    break;

                default:
                    break;
            }
        }

        private void AdditionalModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_GuestAdditionalModel additionalModel = sender as base_GuestAdditionalModel;
            switch (e.PropertyName)
            {
                case "SaleTaxLocation":
                    SetTaxRateFromTaxLocation(additionalModel);
                    break;
                case "IsTaxExemption":
                    //additionalModel.SetDisableSaleTax();
                    if (additionalModel.IsTaxExemption)
                    {
                        additionalModel.SaleTaxLocation = 0;
                    }
                    else
                        additionalModel.ResellerTaxId = string.Empty;
                    additionalModel.RaisePropertyChanged("ResellerTaxId");//Raise Validation

                    SetValueWhenTaxExemptionNResellerTaxChange(additionalModel);
                    break;
                case "TaxExemptionNo":
                    SetValueWhenTaxExemptionNResellerTaxChange(additionalModel);
                    //if (!string.IsNullOrWhiteSpace(additionalModel.TaxExemptionNo))
                    //    if (!additionalModel.IsTaxExemption)
                    //        additionalModel.IsTaxExemption = true;
                    break;
            }
        }


        #endregion

        #region Note Module

        /// <summary>
        /// Initial resource note repository
        /// </summary>
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();

        /// <summary>
        /// Get or sets the StickyManagementViewModel
        /// </summary>
        public PopupStickyViewModel StickyManagementViewModel { get; set; }

        /// <summary>
        /// Load resource note collection
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourceNoteCollection(base_GuestModel guestModel)
        {
            // Load resource note collection
            if (guestModel.ResourceNoteCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        #endregion

        #region Permission

        #region Properties

        private bool _allowAccessPayment = true;
        /// <summary>
        /// Gets or sets the AllowAccessPayment.
        /// </summary>
        public bool AllowAccessPayment
        {
            get { return _allowAccessPayment; }
            set
            {
                if (_allowAccessPayment != value)
                {
                    _allowAccessPayment = value;
                    OnPropertyChanged(() => AllowAccessPayment);
                }
            }
        }

        private bool _allowAccessReward = true;
        /// <summary>
        /// Gets or sets the AllowAccessReward.
        /// </summary>
        public bool AllowAccessReward
        {
            get
            {
                if (SelectedCustomer == null)
                    return _allowAccessReward;
                return _allowAccessReward && SelectedCustomer.IsRewardMember;
            }
            set
            {
                if (_allowAccessReward != value)
                {
                    _allowAccessReward = value;
                    OnPropertyChanged(() => AllowAccessReward);
                }
            }
        }

        private bool _allowAccessSOHistory = true;
        /// <summary>
        /// Gets or sets the AllowAccessSOHistory.
        /// </summary>
        public bool AllowAccessSOHistory
        {
            get { return _allowAccessSOHistory; }
            set
            {
                if (_allowAccessSOHistory != value)
                {
                    _allowAccessSOHistory = value;
                    OnPropertyChanged(() => AllowAccessSOHistory);
                }
            }
        }

        private bool _allowSaleFromCustomer = true;
        /// <summary>
        /// Gets or sets the AllowSaleFromCustomer.
        /// </summary>
        public bool AllowSaleFromCustomer
        {
            get { return _allowSaleFromCustomer; }
            set
            {
                if (_allowSaleFromCustomer != value)
                {
                    _allowSaleFromCustomer = value;
                    OnPropertyChanged(() => AllowSaleFromCustomer);
                }
            }
        }

        private bool _allowManualReward = true;
        /// <summary>
        /// Gets or sets the AllowManualReward.
        /// </summary>
        public bool AllowManualReward
        {
            get { return _allowManualReward; }
            set
            {
                if (_allowManualReward != value)
                {
                    _allowManualReward = value;
                    OnPropertyChanged(() => AllowManualReward);
                }
            }
        }

        #endregion

        /// <summary>
        /// Get permissions
        /// </summary>
        public override void GetPermission()
        {
            if (!IsAdminPermission && !IsFullPermission)
            {
                // Get all user rights
                IEnumerable<string> userRightCodes = Define.USER_AUTHORIZATION.Select(x => x.Code);

                // Get access payment permission
                AllowAccessPayment = userRightCodes.Contains("SO100-01-05");

                // Get access reward permission
                AllowAccessReward = userRightCodes.Contains("SO100-01-06");

                // Get access sale order history permission
                AllowAccessSOHistory = userRightCodes.Contains("SO100-01-07");

                // Union add/copy sale order and sale from customer permission
                AllowSaleFromCustomer = userRightCodes.Contains("SO100-04-02") && userRightCodes.Contains("SO100-01-08");

                // Get allow manual reward permission
                AllowManualReward = userRightCodes.Contains("SO100-02-02");
            }
        }

        #endregion
    }
}