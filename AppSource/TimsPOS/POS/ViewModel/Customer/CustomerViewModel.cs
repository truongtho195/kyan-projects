using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using CPC.Control;
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
    class CustomerViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand NewCommand { get; private set; }
        public RelayCommand<object> SaveCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<object> SearchCommand { get; private set; }
        public RelayCommand PrintCommand { get; private set; }
        public RelayCommand<object> DoubleClickViewCommand { get; private set; }
        public RelayCommand<object> OpenContactFormCommand { get; private set; }
        public RelayCommand<object> CreateNewContactCommand { get; private set; }
        public RelayCommand<object> DeleteContactCommand { get; private set; }
        public RelayCommand<object> OpenPopupPaymendCardCommand { get; private set; }
        public RelayCommand<object> CreateNewPaymentCardCommand { get; private set; }
        public RelayCommand<object> DeletePaymentCardCommand { get; private set; }
        public RelayCommand<object> LoadStepCommand { get; private set; }
        public RelayCommand NoteCommand { get; private set; }

        //Repository
        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_ResourceNoteRepository _resourceNoteRepository = new base_ResourceNoteRepository();
        private base_GuestAddressRepository _guestAddressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_GuestAdditionalRepository _guestAdditionalRepository = new base_GuestAdditionalRepository();

        private base_SaleTaxLocationRepository _saleTaxLocationRepository = new base_SaleTaxLocationRepository();
        private base_GuestProfileRepository _guestProfileRepository = new base_GuestProfileRepository();
        private base_GuestPaymentCardRepository _guestPaymentCardRepository = new base_GuestPaymentCardRepository();
        private base_SaleOrderRepository _saleOrderRepository = new base_SaleOrderRepository();

        private BackgroundWorker _bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        private string CUSTOMER_IMAGE_FOLDER = System.IO.Path.Combine(Define.ImageFilesFolder, "Customer");

        private string _customerMarkType = MarkType.Customer.ToDescription();
        private string EMPLOYEE_MARK = MarkType.Employee.ToDescription();

        private bool _doubleClickRaise = false;
        public List<base_SaleTaxLocation> AllSaleTax { get; set; }

        #endregion

        #region Constructors
        public CustomerViewModel(bool isSearchMode = false)
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            InitialCommand();
            InitialStaticData();
            ChangeSearchMode(isSearchMode);
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
                    || SelectedCustomer.PaymentCardCollection.Any(x => x.IsDirty)
                    || SelectedCustomer.PaymentCardCollection.DeletedItems.Any()
                    || (this.SelectedCustomer.AddressControlCollection != null && this.SelectedCustomer.AddressControlCollection.IsEditingData)
                    || (this.SelectedCustomer.PhotoCollection != null && this.SelectedCustomer.PhotoCollection.IsDirty)
                    || SelectedCustomer.PersonalInfoModel.IsDirty
                    || SelectedCustomer.ContactCollection.Any(x => x.IsDirty || (x.PersonalInfoModel != null && x.PersonalInfoModel.IsDirty))
                    || SelectedCustomer.ContactCollection.DeletedItems.Any());
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

        //Customer
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
            MessageBoxResult result = MessageBox.Show("Do you want to delete?", "POS", MessageBoxButton.YesNo);
            if (result.Is(MessageBoxResult.Yes))
            {
                DeleteNote();
                if (!SelectedCustomer.IsNew)
                {
                    SelectedCustomer.IsPurged = true;
                    SaveCustomer();
                    CustomerCollection.Remove(SelectedCustomer);
                    SelectedCustomer = CustomerCollection.First();
                    TotalCustomers = TotalCustomers - 1;
                }
                IsSearchMode = true;
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
                SearchAlert = string.Empty;
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)//Search All
                {
                    Expression<Func<base_Guest, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);
                    LoadDataByPredicate(predicate, false, 0);

                }
                else if (param != null)
                {
                    Keyword = param.ToString();
                    if (SearchOption == 0)
                    {
                        //Thong bao Can co dk
                        SearchAlert = "Search Option is required";
                    }
                    else
                    {
                        Expression<Func<base_Guest, bool>> predicate = CreatePredicateWithConditionSearch(Keyword);
                        LoadDataByPredicate(predicate, false, 0);
                    }
                }
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the Print command is executed.
        /// </summary>
        private void OnPrintCommandExecute()
        {
            // TODO: Handle command logic here
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
            if (param != null && IsSearchMode)
            {
                _doubleClickRaise = true;
                SelectedCustomer = param as base_GuestModel;
                if (SelectedCustomer.AdditionalModel != null)
                {
                    SelectedCustomer.AdditionalModel.PropertyChanged -= new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
                    SelectedCustomer.AdditionalModel.PropertyChanged += new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
                }
                //SetSaleTaxFromAdditional();
                IsSearchMode = false;
                OnPropertyChanged(() => ContactTotalItem);
                OnPropertyChanged(() => CreditCardTotalItem);

                // Load sale order collection
                LoadSaleOrderCollection(SelectedCustomer);
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

        #region OpenContactForm
        /// <summary>
        /// Method to check whether the OpenEditContactForm command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenContactFormCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the OpenEditContactForm command is executed.
        /// </summary>
        private void OnOpenContactFormCommandExecute(object param)
        {
            if (param != null)
            {
                base_GuestModel contactModel = param as base_GuestModel;
                OpenContactView(contactModel);
            }
        }
        #endregion

        #region Create New Contact
        /// <summary>
        /// Method to check whether the CreateNewContact command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCreateNewContactCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CreateNewContact command is executed.
        /// Create new contact for Customer Type is retailer
        /// <para>When in datagrid not error & user press enter, create new contact</para>
        /// </summary>
        private void OnCreateNewContactCommandExecute(object param)
        {
            base_GuestModel contactModel = SelectedCustomer.ContactCollection.SingleOrDefault(x => x.IsTemporary);
            OpenContactView(contactModel);
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
                    msgResult = MessageBox.Show("Do you want to delete this Contact?", "POS", MessageBoxButton.YesNo);
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
                OpenCreditCardView(paymentCardModel);
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
            base_GuestPaymentCardModel paymentCardModel = SelectedCustomer.PaymentCardCollection.SingleOrDefault(x => x.IsTemporary);
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
                    MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this credit Card?", "POS", MessageBoxButton.YesNo);
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
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_customerMarkType));
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreatePredicateWithConditionSearch(Keyword);

            LoadDataByPredicate(predicate, false, CustomerCollection.Count);
        }
        #endregion

        #region NewNoteCommand

        /// <summary>
        /// Gets the NewNoteCommand command.
        /// </summary>
        public RelayCommand NewNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewNoteCommand command is executed.
        /// Create and show a new note
        /// </summary>
        private void OnNewNoteCommandExecute()
        {
            if (SelectedCustomer.ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
                return;

            // Create a new note
            base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            {
                Resource = SelectedCustomer.Resource.ToString(),
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Create default position for note
            Point position = new Point(600, 200);
            if (SelectedCustomer.ResourceNoteCollection.Count > 0)
            {
                Point lastPostion = SelectedCustomer.ResourceNoteCollection.LastOrDefault().Position;
                if (lastPostion != null)
                    position = new Point(lastPostion.X + 10, lastPostion.Y + 10);
            }

            // Update position
            noteModel.Position = position;

            // Add new note to collection
            SelectedCustomer.ResourceNoteCollection.Add(noteModel);

            // Show new note
            PopupContainer popupContainer = CreatePopupNote(noteModel);
            popupContainer.Show();
            NotePopupCollection.Add(popupContainer);
        }

        #endregion

        #region ShowOrHiddenNoteCommand

        /// <summary>
        /// Gets the ShowOrHiddenNoteCommand command.
        /// </summary>
        public RelayCommand ShowOrHiddenNoteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ShowOrHiddenNoteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnShowOrHiddenNoteCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ShowOrHiddenNoteCommand command is executed.
        /// </summary>
        private void OnShowOrHiddenNoteCommandExecute()
        {
            if (NotePopupCollection.Count == SelectedCustomer.ResourceNoteCollection.Count)
            {
                // Created popup notes, only show or hidden them
                if (ShowOrHiddenNote.Equals("Hide Stickies"))
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (PopupContainer popupContainer in NotePopupCollection)
                        popupContainer.Show();
                }
            }
            else
            {
                // Close all note
                CloseAllPopupNote();

                Point position = new Point(600, 200);
                foreach (base_ResourceNoteModel noteModel in SelectedCustomer.ResourceNoteCollection)
                {
                    noteModel.Position = position;
                    PopupContainer popupContainer = CreatePopupNote(noteModel);
                    popupContainer.Show();
                    NotePopupCollection.Add(popupContainer);
                    position = new Point(position.X + 10, position.Y + 10);
                }
            }

            // Update label "Show/Hidden Note"
            OnPropertyChanged(() => ShowOrHiddenNote);
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
            OpenContactFormCommand = new RelayCommand<object>(OnOpenContactFormCommandExecute, OnOpenContactFormCommandCanExecute);
            CreateNewContactCommand = new RelayCommand<object>(OnCreateNewContactCommandExecute, OnCreateNewContactCommandCanExecute);
            DeleteContactCommand = new RelayCommand<object>(OnDeleteContactCommandExecute, OnDeleteContactCommandCanExecute);
            OpenPopupPaymendCardCommand = new RelayCommand<object>(OnOpenPopupPaymentCardCommandExecute, OnOpenPopupPaymentCardCommandCanExecute);
            CreateNewPaymentCardCommand = new RelayCommand<object>(OnCreateNewPaymentCardCommandExecute, OnCreateNewPaymentCardCommandCanExecute);
            DeletePaymentCardCommand = new RelayCommand<object>(OnDeletePaymentCardCommandExecute, OnDeletePaymentCardCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);
        }

        /// <summary>
        /// Initial Data for Customer View
        /// </summary>
        private void InitialStaticData()
        {

            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });

            NotePopupCollection = new ObservableCollection<PopupContainer>();
            NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

            EmployeeCollection = new ObservableCollection<base_GuestModel>(_guestRepository.GetAll(x => x.Mark.Equals(EMPLOYEE_MARK) && !x.IsPurged && x.IsActived).Select(x => new base_GuestModel(x)));

            //Get All Sale Tax
            SaleTaxCollection = new ObservableCollection<base_SaleTaxLocationModel>();
            AllSaleTax = _saleTaxLocationRepository.GetAll().ToList();
            foreach (base_SaleTaxLocation saleTaxLocation in AllSaleTax.Where(x => x.ParentId == 0))
            {
                base_SaleTaxLocationModel saleTaxModel = new base_SaleTaxLocationModel(saleTaxLocation);
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
                    //Handle clear data if user not choose "Tax Information" in Additional with FexTaxId & TaxLocation
                    if (SelectedCustomer.AdditionalModel.TaxInfoType.Is(TaxInfoType.FedTaxID))
                        SelectedCustomer.AdditionalModel.SaleTaxLocation = 0;
                    else
                        SelectedCustomer.AdditionalModel.FedTaxId = string.Empty;

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

                    if (!SelectedCustomer.AdditionalModel.IsTaxExemption)
                        SelectedCustomer.AdditionalModel.TaxExemptionNo = string.Empty;
                    else
                    {
                        SelectedCustomer.AdditionalModel.SaleTaxLocation = 0;
                        SelectedCustomer.AdditionalModel.TaxRate = 0;
                    }



                    //For New Item
                    if (SelectedCustomer.IsNew)
                    {
                        SaveNewCustomer();
                    }
                    else//For Update Item
                    {
                        UpdateCustomer();
                    }
                    TotalCustomers = _guestRepository.GetIQueryable(x => !x.IsPurged && x.Mark.Equals(_customerMarkType)).Count();
                    result = true;
                }
                else
                    result = false;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                result = false;
                MessageBox.Show(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// save Item For customer
        /// </summary>
        private void SaveNewCustomer()
        {
            //Mapping Additional 
            SelectedCustomer.AdditionalModel.ToEntity();
            SelectedCustomer.base_Guest.base_GuestAdditional.Add(SelectedCustomer.AdditionalModel.base_GuestAdditional);
            SelectedCustomer.AdditionalModel.EndUpdate();

            //Mapping Personal Info
            if (SelectedCustomer.PersonalInfoModel != null)
            {
                //Mapping Personal Info
                SelectedCustomer.PersonalInfoModel.ToEntity();
                SelectedCustomer.base_Guest.base_GuestProfile.Add(SelectedCustomer.PersonalInfoModel.base_GuestProfile);
                SelectedCustomer.PersonalInfoModel.EndUpdate();
            }
            //Mapping contact Collection
            if (SelectedCustomer.ContactCollection != null && SelectedCustomer.ContactCollection.Count > 0)
            {
                //Mapping Contact Collection
                foreach (base_GuestModel contactModel in SelectedCustomer.ContactCollection.Where(x => x.IsDirty && !x.IsTemporary))
                {
                    if (string.IsNullOrWhiteSpace(contactModel.Error))
                    {
                        // Map data from model to entity
                        contactModel.ToEntity();
                        if (contactModel.PersonalInfoModel != null)
                        {
                            contactModel.PersonalInfoModel.ToEntity();
                            contactModel.base_Guest.base_GuestProfile.Add(contactModel.PersonalInfoModel.base_GuestProfile);
                            contactModel.PersonalInfoModel.EndUpdate();
                        }
                        SelectedCustomer.base_Guest.base_Guest1.Add(contactModel.base_Guest);
                        contactModel.EndUpdate();
                    }
                }
            }

            //Save photo
            SavePhotoResource(SelectedCustomer);

            ///Created by Thaipn.
            base_GuestAddressModel addressModel;
            bool firstAddress = true;
            //To insert an address. 
            //Convert from AddressControlCollection To AddressModel 
            foreach (AddressControlModel addressControlModel in this.SelectedCustomer.AddressControlCollection)
            {
                addressModel = new base_GuestAddressModel();
                addressModel.UserCreated = string.Empty;
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
                paymentCardModel.EndUpdate();
            }

            this.SelectedCustomer.ToEntity();
            _guestRepository.Add(this.SelectedCustomer.base_Guest);
            _guestRepository.Commit();
            SelectedCustomer.EndUpdate();
            CustomerCollection.Add(SelectedCustomer);
            this.SetDataToModel(SelectedCustomer);
        }

        /// <summary>
        /// Update Customer
        /// </summary>
        private void UpdateCustomer()
        {
            SelectedCustomer.DateUpdated = DateTime.Now;
            SelectedCustomer.UserUpdated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedCustomer.ToEntity();
            SelectedCustomer.AdditionalModel.ToEntity();
            if (SelectedCustomer.AdditionalModel.IsNew)
                SelectedCustomer.base_Guest.base_GuestAdditional.Add(SelectedCustomer.AdditionalModel.base_GuestAdditional);
            SelectedCustomer.AdditionalModel.EndUpdate();

            //Map Personal Info Or ContactCollection
            if (SelectedCustomer.PersonalInfoModel.IsDirty || SelectedCustomer.PersonalInfoModel.IsNew)//Individual
            {
                SelectedCustomer.PersonalInfoModel.ToEntity();
                if (SelectedCustomer.PersonalInfoModel.IsNew)
                    SelectedCustomer.base_Guest.base_GuestProfile.Add(SelectedCustomer.PersonalInfoModel.base_GuestProfile);
                SelectedCustomer.PersonalInfoModel.EndUpdate();
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
                    addressModel.UserCreated = string.Empty;
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
                    //addressModel.UserUpdated = string.Empty;
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
            SavePhotoResource(SelectedCustomer);


            //Deleted Contact
            if (SelectedCustomer.ContactCollection.DeletedItems.Count > 0)
            {
                foreach (base_GuestModel contactModel in SelectedCustomer.ContactCollection.DeletedItems)
                    _guestRepository.Delete(contactModel.base_Guest);
                SelectedCustomer.ContactCollection.DeletedItems.Clear();
            }
            //Mapping Contact Collection
            if (SelectedCustomer.ContactCollection != null)
            {
                //Mapping or add new Contact Dirty
                if (SelectedCustomer.ContactCollection.Any(x => x.IsDirty || (x.PersonalInfoModel != null && x.PersonalInfoModel.IsDirty)))
                {
                    foreach (base_GuestModel contactModel in SelectedCustomer.ContactCollection.Where(x => x.IsDirty && !x.IsTemporary || (x.PersonalInfoModel != null && x.PersonalInfoModel.IsDirty)))
                    {
                        if (string.IsNullOrWhiteSpace(contactModel.Error))
                        {
                            // Map data from model to entity

                            if (contactModel.PersonalInfoModel != null)
                            {
                                contactModel.PersonalInfoModel.ToEntity();

                                if (contactModel.PersonalInfoModel.IsNew)
                                    contactModel.base_Guest.base_GuestProfile.Add(contactModel.PersonalInfoModel.base_GuestProfile);
                                contactModel.PersonalInfoModel.EndUpdate();
                            }
                            contactModel.ToEntity();
                            if (contactModel.IsNew)
                                SelectedCustomer.base_Guest.base_Guest1.Add(contactModel.base_Guest);

                            contactModel.EndUpdate();
                        }
                    }
                }
            }

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
                guestPaymentCardModel.EndUpdate();
            }
            _guestRepository.Commit();
            SelectedCustomer.EndUpdate();

            //for mapping id for model
            SetDataToModel(SelectedCustomer);
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
                msgResult = MessageBox.Show("Some data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {
                        //if (SaveCustomer())
                        result = SaveCustomer();
                    }
                    else //Has Error
                        result = false;

                    // Remove popup note
                    CloseAllPopupNote();
                }
                else
                {
                    if (SelectedCustomer.IsNew)
                    {
                        DeleteNote();
                        SelectedCustomer = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;

                    }
                    else //Old Item Rollback data
                    {
                        DeleteNote();
                        SelectedCustomer.ToModelAndRaise();
                        SetDataToModel(SelectedCustomer, true);
                    }
                }
            }
            else
            {
                if (SelectedCustomer != null && SelectedCustomer.IsNew)
                    DeleteNote();
                else
                    // Remove popup note
                    CloseAllPopupNote();
            }
            return result;
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
        /// Create New Contact for collection 
        /// </summary>
        /// <param name="contactCollection"></param>
        private base_GuestModel CreateNewContact(CollectionBase<base_GuestModel> contactCollection)
        {
            base_GuestModel contactModel = new base_GuestModel();
            contactModel.Resource = Guid.NewGuid();
            contactModel.Mark = MarkType.Contact.ToDescription();
            contactModel.DateCreated = DateTime.Now;
            contactModel.GuestNo = DateTime.Now.ToString(Define.GuestNoFormat);
            contactModel.IsPrimary = contactCollection.Any(x => x.IsPrimary == true && !x.GuestNo.Equals(contactModel.GuestNo)) ? false : true;
            contactModel.PositionId = 0;
            contactModel.IsDirty = false;
            contactModel.IsTemporary = true;
            return contactModel;
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
                 IsTemporary = true,
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
            SelectedCustomer.IsRewardMember = false;

            SelectedCustomer.CheckLimit = 0;
            SelectedCustomer.GuestTypeId = Common.CustomerTypes.First().Value;
            SelectedCustomer.PositionId = Common.JobTitles.First().Value;
            SelectedCustomer.DateCreated = DateTime.Today;
            SelectedCustomer.UserCreated = Define.USER != null ? Define.USER.LoginName : string.Empty;
            SelectedCustomer.Mark = MarkType.Customer.ToDescription();
            SelectedCustomer.Email = string.Empty;
            SelectedCustomer.IsPrimary = false;
            SelectedCustomer.Resource = Guid.NewGuid();
            SelectedCustomer.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();

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
            //SelectedCustomer.PersonalInfoModel.State = Common.States.First().Value;

            SelectedCustomer.AdditionalModel = new base_GuestAdditionalModel();
            SelectedCustomer.AdditionalModel.PriceSchemeId = 1;
            SelectedCustomer.AdditionalModel.Unit = 0;
            SelectedCustomer.AdditionalModel.IsDirty = false;
            SelectedCustomer.AdditionalModel.TaxRate = 0;
            SelectedCustomer.AdditionalModel.IsNoDiscount = false;
            SelectedCustomer.AdditionalModel.IsDirty = false;

            //Create Photo Collection
            SelectedCustomer.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();

            //Create Address Collection Creaetd by Thaipn.
            SelectedCustomer.AddressControlCollection = new AddressControlCollection() { new AddressControlModel { IsNew = true, AddressTypeID = 0, IsDefault = true, IsDirty = false } };

            //Create contact for Customer Retailer
            SelectedCustomer.ContactCollection = new CollectionBase<base_GuestModel>();
            SelectedCustomer.ContactCollection.Add(CreateNewContact(SelectedCustomer.ContactCollection));

            //Payment collection
            SelectedCustomer.PaymentCardCollection = new CollectionBase<base_GuestPaymentCardModel>();
            SelectedCustomer.PaymentCardCollection.Add(CreateNewPaymentCard());
            SelectedCustomer.IsDirty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCardModel"></param>
        private void OpenCreditCardView(base_GuestPaymentCardModel paymentCardModel)
        {
            string titlePopup = paymentCardModel.IsNew ? "Create new credit card" : "Update credit card";
            CreditCardViewModel creditCardViewModel = new CreditCardViewModel();
            creditCardViewModel.PaymentCardModel = paymentCardModel;
            bool? result = _dialogService.ShowDialog<CreditCardView>(_ownerViewModel, creditCardViewModel, titlePopup);
            if (result == true)
            {
                if (paymentCardModel.IsNew)
                {
                    if (!this.SelectedCustomer.PaymentCardCollection.Any(x => x.IsTemporary))
                        this.SelectedCustomer.PaymentCardCollection.Add(CreateNewPaymentCard());
                }
            }
            OnPropertyChanged(() => CreditCardTotalItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCardModel"></param>
        private void OpenContactView(base_GuestModel contactModel)
        {
            string titlePopup = contactModel.IsNew ? "Create new Contact" : "Update Contact";
            ContactViewModel contactViewModel = new ContactViewModel();
            contactViewModel.ContactModel = contactModel;
            contactViewModel.ContactCollection = SelectedCustomer.ContactCollection;
            bool? result = _dialogService.ShowDialog<ContactView>(_ownerViewModel, contactViewModel, titlePopup);
            if (result == true)
            {
                if (contactModel.IsNew)
                {
                    if (!SelectedCustomer.ContactCollection.Any(x => x.IsTemporary))
                    {
                        base_GuestModel contactTemporary = CreateNewContact(SelectedCustomer.ContactCollection);
                        SelectedCustomer.ContactCollection.Add(contactTemporary);
                    }
                }
            }
            OnPropertyChanged(() => ContactTotalItem);
        }

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreatePredicateWithConditionSearch(string keyword)
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(keyword) && SearchOption > 0)
            {
                if (SearchOption.Has(SearchOptions.AccountNum))
                {
                    predicate = predicate.And(x => x.GuestNo.Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.FirstName))
                {
                    predicate = predicate.And(x => x.FirstName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.LastName))
                {
                    predicate = predicate.And(x => x.LastName.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Company))
                {
                    predicate = predicate.And(x => x.Company.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Phone))
                {
                    predicate = predicate.And(x => x.Phone1.ToLower().Contains(keyword.ToLower()) || x.Phone2.ToLower().Contains(keyword.ToLower()));
                }
            }
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_customerMarkType));
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
            IQueryable<base_Guest> query = _guestRepository.GetIQueryable(x => !x.GuestNo.Equals(customerModel.GuestNo)
                                                                               && x.Mark.Equals(_customerMarkType)
                                                                               && !x.IsPurged
                                                                               && (x.Phone1.Equals(customerModel.Phone1)
                                                                               || x.Email.ToLower().Equals(customerModel.Email.ToLower())));
            if (query.Count() > 0)
            {
                result = true;
                MessageBoxResult resultMsg = MessageBox.Show("This customer has existed. Please recheck Email or Phone. Do you want to view profiles?", "POS", MessageBoxButton.YesNo);
                if (MessageBoxResult.Yes.Is(resultMsg))
                {
                    base_GuestModel guestModel = new base_GuestModel(query.FirstOrDefault());
                    if (guestModel.base_Guest.base_GuestProfile.Count > 0)
                        guestModel.PersonalInfoModel = new base_GuestProfileModel(guestModel.base_Guest.base_GuestProfile.FirstOrDefault());
                    else
                        guestModel.PersonalInfoModel = new base_GuestProfileModel();
                    ViewProfileViewModel viewProfileViewModel = new ViewProfileViewModel();
                    viewProfileViewModel.GuestModel = guestModel;
                    _dialogService.ShowDialog<ViewProfile>(_ownerViewModel, viewProfileViewModel, "View Profile");
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
                IsBusy = true;
                //Cout all Customer in Data base show on grid
                TotalCustomers = _guestRepository.GetIQueryable(predicate).Count();
                //Get data with range
                IList<base_Guest> customers = _guestRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                _guestRepository.Refresh(customers);
                foreach (base_Guest customer in customers)
                {
                    bgWorker.ReportProgress(0, customer);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_GuestModel customerModel = new base_GuestModel((base_Guest)e.UserState);
                SetDataToModel(customerModel);
                CustomerCollection.Add(customerModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                IsBusy = false;
            };
            bgWorker.RunWorkerAsync();
        }

        ///// <summary>
        ///// Refresh Customer & relation
        ///// </summary>
        //private void Refresh()
        //{
        //    //Refresh Table relation
        //    _guestRepository.Refresh();
        //    _resourceNoteRepository.Refresh();
        //    _guestAddressRepository.Refresh();
        //    _guestAdditionalRepository.Refresh();
        //    _paymentCardRepository.Refresh();
        //}

        /// <summary>
        /// Set data & relation of customer to model
        /// <para>Using for rollback data or Set data on first load</para>
        /// </summary>
        /// <param name="customerModel"></param>
        /// <param name="RaisePropertyChanged">Raise Property Changed when rollback data</param>
        private void SetDataToModel(base_GuestModel customerModel, bool RaisePropertyChanged = false)
        {
            // ==== Load PersonalInfoModel ====
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

            //==== Load AdditionalModel ====
            _guestAdditionalRepository.Refresh(customerModel.base_Guest.base_GuestAdditional);
            if (customerModel.base_Guest.base_GuestAdditional.Count > 0)
            {
                base_GuestAdditional customerAdditional = customerModel.base_Guest.base_GuestAdditional.First();
                customerModel.AdditionalModel = new base_GuestAdditionalModel(customerAdditional, RaisePropertyChanged);

                //Set Tax Infomation with FeedTaxId & TaxLocation
                if (customerModel.AdditionalModel.SaleTaxLocation == 0)
                    customerModel.AdditionalModel.TaxInfoType = (int)TaxInfoType.FedTaxID;
                else if (customerModel.AdditionalModel.SaleTaxLocation > 0)
                    customerModel.AdditionalModel.TaxInfoType = (int)TaxInfoType.TaxLocation;

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
                customerModel.AdditionalModel.SaleTaxType = 0;
                customerModel.AdditionalModel.PriceLevelType = 0;
                customerModel.AdditionalModel.TaxInfoType = 0;
                customerModel.AdditionalModel.TaxRate = 0;
            }
            customerModel.AdditionalModel.IsDirty = false;
            //End Additional;

            //Load PhotoCollection
            LoadResourcePhoto(customerModel);



            //==== Load DefaultAdress Address ====
            _guestAddressRepository.Refresh(customerModel.base_Guest.base_GuestAddress);
            if (customerModel.base_Guest.base_GuestAddress.Count > 0)
                customerModel.AddressModel = new base_GuestAddressModel(customerModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.IsDefault), RaisePropertyChanged);

            //==== Load Address Model ====
            customerModel.AddressCollection = new ObservableCollection<base_GuestAddressModel>(customerModel.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));
            //AddressCollection For Control
            customerModel.AddressControlCollection = new AddressControlCollection();
            //Set AddresssModel to address for control
            foreach (base_GuestAddressModel guestAddressModel in customerModel.AddressCollection)
            {
                AddressControlModel addressControlModel = guestAddressModel.ToAddressControlModel();
                addressControlModel.IsDirty = false;
                customerModel.AddressControlCollection.Add(addressControlModel);
            }

            //==== Load ContactCollection ====
            _guestRepository.Refresh(customerModel.base_Guest.base_Guest1);
            if (customerModel.base_Guest.base_Guest1.Count > 0)
                customerModel.ContactCollection = new CollectionBase<base_GuestModel>(customerModel.base_Guest.base_Guest1.Select(x => new base_GuestModel(x)));
            else
                customerModel.ContactCollection = new CollectionBase<base_GuestModel>();

            customerModel.ContactCollection.Add(CreateNewContact(customerModel.ContactCollection));

            //====Load PaymentCard ====
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
            customerModel.PaymentCardCollection.Add(CreateNewPaymentCard());

            LoadNote(customerModel);
            customerModel.IsDirty = false;
        }

        /// <summary>
        /// Load Resource Photo Collection & DefaultPhoto for GuestModel
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadResourcePhoto(base_GuestModel guestModel)
        {
            if (guestModel.PhotoCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                    _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourcePhotoModel(x)
                    {
                        ImagePath = System.IO.Path.Combine(CUSTOMER_IMAGE_FOLDER, guestModel.GuestNo, x.LargePhotoFilename),
                        IsDirty = false
                    }));

                if (guestModel.PhotoCollection.Count > 0)
                    guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
                else
                    guestModel.PhotoDefault = new base_ResourcePhotoModel();
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
            if (guestModel.PhotoCollection != null && guestModel.PhotoCollection.Count > 0)
            {
                foreach (base_ResourcePhotoModel photoModel in guestModel.PhotoCollection.Where(x => x.IsDirty))
                {
                    photoModel.Resource = guestModel.Resource.ToString();
                    photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;
                    //To map data from model to entity
                    photoModel.ToEntity();
                    if (photoModel.IsNew)
                        _photoRepository.Add(photoModel.base_ResourcePhoto);

                    //To save image to store.
                    this.SaveImage(photoModel, guestModel.GuestNo);
                    _photoRepository.Commit();

                    //set Id
                    photoModel.Id = photoModel.base_ResourcePhoto.Id;
                    photoModel.EndUpdate();
                }

                if (guestModel.PhotoCollection.Count > 0)
                    guestModel.PhotoDefault = guestModel.PhotoCollection.FirstOrDefault();
                else
                    guestModel.PhotoDefault = new base_ResourcePhotoModel();
            }
        }

        /// <summary>
        /// Load sale order collection
        /// </summary>
        /// <param name="customerModel"></param>
        private void LoadSaleOrderCollection(base_GuestModel customerModel)
        {
            if (customerModel.SaleOrderCollection == null)
            {
                string customerGuid = customerModel.Resource.ToString();

                // Initial sale order collection
                customerModel.SaleOrderCollection = new ObservableCollection<base_SaleOrderModel>(_saleOrderRepository.
                    GetAll(x => x.CustomerResource.Equals(customerGuid)).Select(x => new base_SaleOrderModel(x)));

                TotalSaleOrder = new base_SaleOrderModel
                {
                    Total = customerModel.SaleOrderCollection.Sum(x => x.Total),
                    Paid = customerModel.SaleOrderCollection.Sum(x => x.Paid),
                    Balance = customerModel.SaleOrderCollection.Sum(x => x.Balance)
                };
            }
        }

        #endregion

        #region Note Methods

        /// <summary>
        /// Load notes
        /// </summary>
        /// <param name="guestModel"></param>
        private void LoadNote(base_GuestModel guestModel)
        {
            // Load Note
            if (guestModel.ResourceNoteCollection == null)
            {
                string resource = guestModel.Resource.ToString();
                guestModel.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>(
                    _resourceNoteRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourceNoteModel(x)));
            }
        }

        /// <summary>
        /// Create or update note
        /// </summary>
        /// <param name="noteModel"></param>
        private void SaveNote(base_ResourceNoteModel noteModel)
        {
            noteModel.ToEntity();
            if (noteModel.IsNew)
                _resourceNoteRepository.Add(noteModel.base_ResourceNote);
            _resourceNoteRepository.Commit();
            noteModel.EndUpdate();
        }

        /// <summary>
        /// Delete and close popup notes
        /// </summary>
        private void DeleteNote()
        {
            // Remove popup note
            CloseAllPopupNote();

            // Delete note
            foreach (base_ResourceNoteModel noteModel in SelectedCustomer.ResourceNoteCollection)
                _resourceNoteRepository.Delete(noteModel.base_ResourceNote);
            _resourceNoteRepository.Commit();

            SelectedCustomer.ResourceNoteCollection.Clear();
        }

        /// <summary>
        /// Close popup notes
        /// </summary>
        private void CloseAllPopupNote()
        {
            if (NotePopupCollection != null)
            {
                // Remove popup note
                foreach (PopupContainer popupContainer in NotePopupCollection)
                    popupContainer.Close();
                NotePopupCollection.Clear();
            }
        }

        /// <summary>
        /// Create popup note
        /// </summary>
        /// <param name="noteModel"></param>
        /// <returns></returns>
        private PopupContainer CreatePopupNote(base_ResourceNoteModel noteModel)
        {
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.SelectedNote = noteModel;
            noteViewModel.NotePopupCollection = NotePopupCollection;
            noteViewModel.ResourceNoteCollection = SelectedCustomer.ResourceNoteCollection;
            CPC.POS.View.NoteView noteView = new View.NoteView();

            PopupContainer popupContainer = new PopupContainer(noteView, true);
            popupContainer.WindowStartupLocation = WindowStartupLocation.Manual;
            popupContainer.DataContext = noteViewModel;
            popupContainer.Width = 150;
            popupContainer.Height = 120;
            popupContainer.MinWidth = 150;
            popupContainer.MinHeight = 120;
            popupContainer.FormBorderStyle = PopupContainer.BorderStyle.None;
            popupContainer.Deactivated += (sender, e) => { SaveNote(noteModel); };
            popupContainer.Loaded += (sender, e) =>
            {
                popupContainer.Left = noteModel.Position.X;
                popupContainer.Top = noteModel.Position.Y;
            };
            return popupContainer;
        }

        #endregion

        #region Override Methods
        /// <summary>
        /// Loading data in Change view or Inititial
        /// </summary>
        public override void LoadData()
        {

            //InitialStaticData();
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)//Load with Search Condition
            {
                predicate = CreatePredicateWithConditionSearch(Keyword);
            }
            else
            {
                predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_customerMarkType));
            }
            LoadDataByPredicate(predicate, true);
            //LoadSyncWithTask(predicate, true);

        }

        /// <summary>
        /// Check save data when changing view
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ChangeViewExecute(isClosing);
        }

        /// <summary>
        /// Change view from Ribbon
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList)
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

        #endregion

        #region PropertyChanged
        void AdditionalModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base_GuestAdditionalModel additionalModel = sender as base_GuestAdditionalModel;
            switch (e.PropertyName)
            {
                case "SaleTaxLocation":
                    if (additionalModel.SaleTaxLocation == 0)
                    {
                        base_SaleTaxLocation taxCode = AllSaleTax.FirstOrDefault(x => x.ParentId == additionalModel.SaleTaxLocation);
                        //This TaxLocation has only one TaxCode &  this sale Code is Single or Price
                        if (AllSaleTax.Count(x => x.ParentId == additionalModel.SaleTaxLocation) == 1 && taxCode != null && taxCode.TaxOption != (int)SalesTaxOption.Multi && taxCode.base_SaleTaxLocationOption.Any())
                            additionalModel.TaxRate = taxCode.base_SaleTaxLocationOption.FirstOrDefault().TaxRate;
                        else
                            additionalModel.TaxRate = 0;
                    }
                    else
                        additionalModel.TaxRate = 0;
                    break;
                case "IsTaxExemption":
                    additionalModel.SaleTaxLocation = 0;
                    break;
            }
        }
        #endregion
    }
}

