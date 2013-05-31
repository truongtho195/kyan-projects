using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
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
    class VendorViewModel : ViewModelBase
    {
        #region Defines

        private base_GuestRepository _guestRepository = new base_GuestRepository();
        private base_GuestAddressRepository _addressRepository = new base_GuestAddressRepository();
        private base_ResourcePhotoRepository _photoRepository = new base_ResourcePhotoRepository();
        private base_GuestAdditionalRepository _additionalRepository = new base_GuestAdditionalRepository();
        private base_ResourceNoteRepository _noteRepository = new base_ResourceNoteRepository();
        private base_SaleTaxLocationRepository _saleTaxLocationRepository = new base_SaleTaxLocationRepository();
        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_DepartmentRepository _departmentRepository = new base_DepartmentRepository();
        private base_UOMRepository _uomRepository = new base_UOMRepository();
        private base_PurchaseOrderRepository _purchaseOrderRepository = new base_PurchaseOrderRepository();

        private string _vendorMark = MarkType.Vendor.ToDescription();

        public List<base_SaleTaxLocation> AllSaleTax { get; set; }

        #endregion

        #region Constructors

        // Default constructor
        public VendorViewModel()
            : base()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            LoadStaticData();

            InitialCommand();
        }

        public VendorViewModel(bool isList, object param = null)
            : this()
        {
            ChangeSearchMode(isList, param);
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

        private ObservableCollection<base_GuestModel> _vendorCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the VendorCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorCollection
        {
            get { return _vendorCollection; }
            set
            {
                if (_vendorCollection != value)
                {
                    _vendorCollection = value;
                    OnPropertyChanged(() => VendorCollection);
                }
            }
        }

        private base_GuestModel _selectedVendor;
        /// <summary>
        /// Gets or sets the SelectedVendor.
        /// </summary>
        public base_GuestModel SelectedVendor
        {
            get { return _selectedVendor; }
            set
            {
                if (_selectedVendor != value)
                {
                    _selectedVendor = value;
                    OnPropertyChanged(() => SelectedVendor);
                }
            }
        }

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

        /// <summary>
        /// Gets the IsDirtyContactCollection
        /// </summary>
        public bool IsDirtyContactCollection { get; set; }

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

        #region Parameter
        /// <summary>
        /// Gets or sets the SelectedItemEmployee.
        /// </summary>
        private Common _parameter;
        public Common Parameter
        {
            get
            {
                return _parameter;
            }
            set
            {
                _parameter = value;
                OnPropertyChanged(() => Parameter);
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
                    return "Show stickies";
                else if (NotePopupCollection.Count == SelectedVendor.ResourceNoteCollection.Count && NotePopupCollection.Any(x => x.IsVisible))
                    return "Hide stickies";
                else
                    return "Show stickies";
            }
        }

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

        #region SetSearchFocus
        private bool _setSearchFocus;
        /// <summary>
        /// Gets or sets the SetFocus.
        /// </summary>
        public bool SetSearchFocus
        {
            get { return _setSearchFocus; }
            set
            {
                if (_setSearchFocus != value)
                {
                    _setSearchFocus = value;
                    OnPropertyChanged(() => SetSearchFocus);
                }
            }
        }
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

        private int _totalProducts;
        /// <summary>
        /// Gets or sets the TotalProducts.
        /// </summary>
        public int TotalProducts
        {
            get { return _totalProducts; }
            set
            {
                if (_totalProducts != value)
                {
                    _totalProducts = value;
                    OnPropertyChanged(() => TotalProducts);
                }
            }
        }

        /// <summary>
        /// Gets or sets the CategoryList
        /// </summary>
        public List<base_DepartmentModel> CategoryList { get; set; }

        /// <summary>
        /// Gets or sets the UOMList
        /// </summary>
        public ObservableCollection<CheckBoxItemModel> UOMList { get; set; }

        private base_PurchaseOrderModel _totalPurchaseOrder;
        /// <summary>
        /// Gets or sets the TotalPurchaseOrder.
        /// </summary>
        public base_PurchaseOrderModel TotalPurchaseOrder
        {
            get { return _totalPurchaseOrder; }
            set
            {
                if (_totalPurchaseOrder != value)
                {
                    _totalPurchaseOrder = value;
                    OnPropertyChanged(() => TotalPurchaseOrder);
                }
            }
        }

        #endregion

        #region Command Methods

        #region SearchCommand

        /// <summary>
        /// Gets the SearchCommand command.
        /// </summary>
        public ICommand SearchCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SearchCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SearchCommand command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            try
            {
                SearchAlert = string.Empty;
                if ((param == null || string.IsNullOrWhiteSpace(param.ToString())) && SearchOption == 0)//Search All
                {
                    Expression<Func<base_Guest, bool>> predicate = CreateSearchVendorPredicate(Keyword);
                    LoadVendorDataByPredicate(predicate, false, 0);

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
                        Expression<Func<base_Guest, bool>> predicate = CreateSearchVendorPredicate(Keyword);
                        LoadVendorDataByPredicate(predicate, false, 0);
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

        #region NewCommand

        /// <summary>
        /// Gets the NewCommand command.
        /// </summary>
        public ICommand NewCommand { get; private set; }

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
            if (ShowNotification(null))
            {
                NewVendor();
            }
        }

        #endregion

        #region SaveCommand

        /// <summary>
        /// Gets the SaveCommand command.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveCommandCanExecute(object param)
        {
            return IsValid && IsEdit();
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnSaveCommandExecute(object param)
        {
            if (param == null)
            {
                // Save draft vendor
            }
            else
            {
                SaveVendor(param.ToString());
            }
        }

        #endregion

        #region DeleteCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute()
        {
            if (SelectedVendor == null)
                return false;
            return !IsEdit() && !SelectedVendor.IsNew;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeleteCommandExecute()
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this vendor?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {

                if (SelectedVendor.IsNew)
                {
                    DeleteNote();

                    SelectedVendor = null;
                }
                else if (IsValid)
                {
                    DeleteNote();

                    SelectedVendor.IsPurged = true;
                    SelectedVendor.ToEntity();
                    foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection.Where(x => !x.IsAcceptedRow))
                        contactModel.base_Guest.IsPurged = true;
                    _guestRepository.Commit();

                    SelectedVendor.EndUpdate();
                    VendorCollection.Remove(SelectedVendor);
                }
                else
                    return;

                IsSearchMode = true;
            }
        }

        #endregion

        #region ClearCommand

        /// <summary>
        /// Gets the ClearCommand command.
        /// </summary>
        public ICommand ClearCommand { get; private set; }

        /// <summary>
        /// Method to check whether the ClearCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnClearCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ClearCommand command is executed.
        /// </summary>
        private void OnClearCommandExecute()
        {
            NewVendor();
        }

        #endregion

        #region DoubleClickViewCommand

        /// <summary>
        /// Gets the DoubleClickViewCommand command.
        /// </summary>
        public ICommand DoubleClickViewCommand { get; private set; }

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
                // Update selected vendor
                SelectedVendor = param as base_GuestModel;
                if (SelectedVendor.AdditionalModel != null)
                {
                    SelectedVendor.AdditionalModel.PropertyChanged -= new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
                    SelectedVendor.AdditionalModel.PropertyChanged += new PropertyChangedEventHandler(AdditionalModel_PropertyChanged);
                }

                // Load product by selected vendor id
                LoadProductVendorCollection(SelectedVendor);

                // Load purchase order collection
                LoadPurchaseOrderCollection(SelectedVendor);

                // Show detail form
                IsSearchMode = false;
            }
            else if (IsSearchMode)
            {
                // Show detail form
                IsSearchMode = false;
            }
            else if (ShowNotification(null))
            {
                // Show list form
                IsSearchMode = true;
            }
        }

        #endregion

        #region NewContactCommand

        /// <summary>
        /// Gets the NewContactCommand command.
        /// </summary>
        public ICommand NewContactCommand { get; private set; }

        /// <summary>
        /// Method to check whether the NewContactCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewContactCommandCanExecute(object param)
        {
            //return SelectedVendor.ContactCollection.Count(x => x.IsNew && !x.IsAcceptedRow) == 0;
            return true;
        }

        /// <summary>
        /// Method to invoke when the NewContactCommand command is executed.
        /// </summary>
        private void OnNewContactCommandExecute(object param)
        {
            ContactViewModel contactViewModel = new ContactViewModel();
            contactViewModel.ContactModel = SelectedContact;
            contactViewModel.ContactCollection = SelectedVendor.ContactCollection;
            bool? result = _dialogService.ShowDialog<CPC.POS.View.ContactView>(_ownerViewModel, contactViewModel, "Create Contact");
            if (result.HasValue && result.Value)
            {
                SelectedContact.IsAcceptedRow = false;
                IsDirtyContactCollection = true;
                SelectedVendor.ContactCollection.Add(CreateNewContact());
            }
        }

        #endregion

        #region SaveContactCommand

        /// <summary>
        /// Gets the SaveContactCommand command.
        /// </summary>
        public ICommand SaveContactCommand { get; private set; }

        /// <summary>
        /// Method to check whether the SaveContactCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveContactCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SaveContactCommand command is executed.
        /// </summary>
        private void OnSaveContactCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion

        #region DeleteContactCommand

        /// <summary>
        /// Gets the DeleteContactCommand command.
        /// </summary>
        public ICommand DeleteContactCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteContactCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteContactCommandCanExecute(object param)
        {
            return param != null;
        }

        /// <summary>
        /// Method to invoke when the DeleteContactCommand command is executed.
        /// </summary>
        private void OnDeleteContactCommandExecute(object param)
        {
            base_GuestModel contactModel = param as base_GuestModel;
            if (!contactModel.IsAcceptedRow && SelectedVendor.ContactCollection.Count(x => !x.IsAcceptedRow) > 1)
            {
                MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this contact?", "POS", MessageBoxButton.YesNo);
                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    SelectedVendor.ContactCollection.Remove(contactModel);
                    if (contactModel.IsPrimary)
                        SelectedVendor.ContactCollection.FirstOrDefault().IsPrimary = true;
                    IsDirtyContactCollection = true;
                }
            }
        }

        #endregion

        #region PopupContactCommand

        /// <summary>
        /// Gets the PopupContactCommand command.
        /// </summary>
        public ICommand PopupContactCommand { get; private set; }

        /// <summary>
        /// Method to check whether the PopupContactCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnPopupContactCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the PopupContactCommand command is executed.
        /// </summary>
        private void OnPopupContactCommandExecute(object param)
        {
            if (param != null && !(param as base_GuestModel).IsAcceptedRow)
            {
                ContactViewModel contactViewModel = new ContactViewModel();
                contactViewModel.ContactModel = SelectedContact;
                contactViewModel.ContactCollection = SelectedVendor.ContactCollection;
                bool? result = _dialogService.ShowDialog<CPC.POS.View.ContactView>(_ownerViewModel, contactViewModel, "Update Contact");
                if (result.HasValue && result.Value)
                {
                    IsDirtyContactCollection = true;
                }
            }
        }

        #endregion

        #region NewNoteCommand

        /// <summary>
        /// Gets the NewNoteCommand command.
        /// </summary>
        public ICommand NewNoteCommand { get; private set; }

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
            if (SelectedVendor.ResourceNoteCollection.Count == Define.CONFIGURATION.DefaultMaximumSticky)
                return;

            // Create a new note
            base_ResourceNoteModel noteModel = new base_ResourceNoteModel
            {
                Resource = SelectedVendor.Resource.ToString(),
                Color = Define.DefaultColorNote,
                DateCreated = DateTimeExt.Now
            };

            // Create default position for note
            Point position = new Point(400, 200);
            if (SelectedVendor.ResourceNoteCollection.Count > 0)
            {
                Point lastPostion = SelectedVendor.ResourceNoteCollection.LastOrDefault().Position;
                if (lastPostion != null)
                    position = new Point(lastPostion.X + 10, lastPostion.Y + 10);
            }

            // Update position
            noteModel.Position = position;

            // Add new note to collection
            SelectedVendor.ResourceNoteCollection.Add(noteModel);

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
        public ICommand ShowOrHiddenNoteCommand { get; private set; }

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
            if (NotePopupCollection.Count == SelectedVendor.ResourceNoteCollection.Count)
            {
                // Created popup notes, only show or hidden them
                if (ShowOrHiddenNote.Equals("Hide stickies"))
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

                Point position = new Point(400, 200);
                foreach (base_ResourceNoteModel noteModel in SelectedVendor.ResourceNoteCollection)
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

        #region LoadStepCommand

        /// <summary>
        /// Gets the LoadStepCommand command.
        /// </summary>
        public ICommand LoadStepCommand { get; private set; }

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
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(FilterText))//Load Step Current With Search Current with Search
                predicate = CreateSearchVendorPredicate(Keyword);
            else
                predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_vendorMark));

            LoadVendorDataByPredicate(predicate, false, VendorCollection.Count);
        }

        #endregion

        #region AddTermCommand

        /// <summary>
        /// Gets the AddTerm Command.
        /// <summary>
        public ICommand AddTermCommand { get; private set; }

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
            short dueDays = SelectedVendor.TermNetDue;
            decimal discount = SelectedVendor.TermDiscount;
            short discountDays = SelectedVendor.TermPaidWithinDay;
            PaymentTermViewModel paymentTermViewModel = new PaymentTermViewModel(dueDays, discount, discountDays);
            bool? dialogResult = _dialogService.ShowDialog<PaymentTermView>(_ownerViewModel, paymentTermViewModel, "Add Term");
            if (dialogResult == true)
            {
                SelectedVendor.TermNetDue = paymentTermViewModel.DueDays;
                SelectedVendor.TermDiscount = paymentTermViewModel.Discount;
                SelectedVendor.TermPaidWithinDay = paymentTermViewModel.DiscountDays;
                SelectedVendor.PaymentTermDescription = paymentTermViewModel.Description;
            }
        }

        #endregion

        #region LoadStepProductCommand

        /// <summary>
        /// Gets the LoadStepProductCommand command.
        /// </summary>
        public ICommand LoadStepProductCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadStepProductCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoadStepProductCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadStepProductCommand command is executed.
        /// </summary>
        private void OnLoadStepProductCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion

        #region DeletesCommand

        /// <summary>
        /// Gets the DeleteCommand command.
        /// </summary>
        public ICommand DeletesCommand { get; private set; }

        /// <summary>
        /// Method to check whether the DeleteCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeletesCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return true;
        }

        /// <summary>
        /// Method to invoke when the DeleteCommand command is executed.
        /// </summary>
        private void OnDeletesCommandExecute(object param)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you want to delete this vendor?", "POS", MessageBoxButton.YesNo);
            if (msgResult.Is(MessageBoxResult.Yes))
            {
                for (int i = 0; i < (param as ObservableCollection<object>).Count; i++)
                {
                    base_GuestModel model = (param as ObservableCollection<object>)[i] as base_GuestModel;
                    string resource = model.Resource.Value.ToString();
                    if (!_purchaseOrderRepository.GetAll().Select(x => x.VendorResource).Contains(resource))
                    {
                        model.IsPurged = true;
                        model.ToEntity();
                        foreach (base_GuestModel contactModel in model.ContactCollection.Where(x => !x.IsAcceptedRow))
                            contactModel.base_Guest.IsPurged = true;
                        _guestRepository.Commit();
                        model.EndUpdate();
                        VendorCollection.Remove(model);
                        this.DeleteNoteExt(model);
                        i--;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            // Created by Thaipn
            Parameter = new Common();

            ///To get addressType. Createdby Thaipn.
            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
            AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });

            NotePopupCollection = new ObservableCollection<PopupContainer>();
            NotePopupCollection.CollectionChanged += (sender, e) => { OnPropertyChanged(() => ShowOrHiddenNote); };

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

            // Load category list
            IEnumerable<base_DepartmentModel> categories = _departmentRepository.
                GetAll(x => x.IsActived == true && x.LevelId == 1).
                Select(x => new base_DepartmentModel(x));

            CategoryList = new List<base_DepartmentModel>(categories);

            // Load UOM list
            UOMList = new ObservableCollection<CheckBoxItemModel>(_uomRepository.GetIQueryable(x => x.IsActived).
                OrderBy(x => x.Name).Select(x => new CheckBoxItemModel { Value = x.Id, Text = x.Name }));
        }

        /// <summary>
        /// Initial commands
        /// </summary>
        private void InitialCommand()
        {
            // Route the commands
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            NewCommand = new RelayCommand(OnNewCommandExecute, OnNewCommandCanExecute);
            SaveCommand = new RelayCommand<object>(OnSaveCommandExecute, OnSaveCommandCanExecute);
            DeleteCommand = new RelayCommand(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            ClearCommand = new RelayCommand(OnClearCommandExecute, OnClearCommandCanExecute);
            DoubleClickViewCommand = new RelayCommand<object>(OnDoubleClickViewCommandExecute, OnDoubleClickViewCommandCanExecute);
            NewContactCommand = new RelayCommand<object>(OnNewContactCommandExecute, OnNewContactCommandCanExecute);
            SaveContactCommand = new RelayCommand(OnSaveContactCommandExecute, OnSaveContactCommandCanExecute);
            DeleteContactCommand = new RelayCommand<object>(OnDeleteContactCommandExecute, OnDeleteContactCommandCanExecute);
            PopupContactCommand = new RelayCommand<object>(OnPopupContactCommandExecute, OnPopupContactCommandCanExecute);
            NewNoteCommand = new RelayCommand(OnNewNoteCommandExecute, OnNewNoteCommandCanExecute);
            ShowOrHiddenNoteCommand = new RelayCommand(OnShowOrHiddenNoteCommandExecute, OnShowOrHiddenNoteCommandCanExecute);
            LoadStepCommand = new RelayCommand<object>(OnLoadStepCommandExecute, OnLoadStepCommandCanExecute);
            AddTermCommand = new RelayCommand(OnAddTermCommandExecute, OnAddTermCommandCanExecute);
            LoadStepProductCommand = new RelayCommand(OnLoadStepProductCommandExecute, OnLoadStepProductCommandCanExecute);
            DeletesCommand = new RelayCommand<object>(OnDeletesCommandExecute, OnDeletesCommandCanExecute);
        }

        /// <summary>
        /// Check has edit on form
        /// </summary>
        /// <returns></returns>
        private bool IsEdit()
        {
            if (SelectedVendor == null)
                return false;

            //Repaired by Thaipn.
            return (SelectedVendor.IsDirty || SelectedVendor.AdditionalModel.IsDirty ||
                (SelectedVendor.AddressControlCollection != null && SelectedVendor.AddressControlCollection.IsEditingData) ||
                (SelectedVendor.PhotoCollection != null && SelectedVendor.PhotoCollection.IsDirty) ||
                IsDirtyContactCollection);
        }

        /// <summary>
        /// Notify when exit or change form
        /// </summary>
        /// <returns>True is continue action</returns>
        private bool ShowNotification(bool? isClosing)
        {
            bool result = true;

            // Check data is edited
            if (IsEdit())
            {
                // Show notification when data has changed
                MessageBoxResult msgResult = MessageBox.Show("Data has changed. Do you want to save?", "POS", MessageBoxButton.YesNo);

                if (msgResult.Is(MessageBoxResult.Yes))
                {
                    if (OnSaveCommandCanExecute(null))
                    {
                        // Call Save function
                        result = SaveVendor("SaveClose");
                    }
                    else
                    {
                        result = false;
                    }

                    // Remove popup note
                    CloseAllPopupNote();
                }
                else
                {
                    if (SelectedVendor.IsNew)
                    {
                        DeleteNote();

                        SelectedVendor = null;
                        if (isClosing.HasValue && !isClosing.Value)
                            IsSearchMode = true;
                    }
                    else
                    {
                        // Rollback vendor
                        SelectedVendor.AddressCollection = null;
                        SelectedVendor.PhotoCollection = null;
                        SelectedVendor.AdditionalModel = null;
                        SelectedVendor.ContactCollection = null;
                        SelectedVendor.ToModelAndRaise();
                        SelectedVendor.EndUpdate();
                        IsDirtyContactCollection = false;
                        LoadRelationVendorData(SelectedVendor);
                        // Remove popup note
                        CloseAllPopupNote();
                    }
                }
            }
            else
            {
                if (SelectedVendor != null && SelectedVendor.IsNew)
                    DeleteNote();
                else
                    // Remove popup note
                    CloseAllPopupNote();
            }

            return result;
        }

        /// <summary>
        /// Create a new contact
        /// </summary>
        /// <param name="isPrimary">Default is false</param>
        /// <returns>New contact</returns>
        private base_GuestModel CreateNewContact(bool isPrimary = false)
        {
            return new base_GuestModel
            {
                IsPrimary = isPrimary,
                DateCreated = DateTimeExt.Now,
                GuestNo = DateTimeExt.Now.ToString(Define.GuestNoFormat),
                Mark = MarkType.Contact.ToDescription(),
                PositionId = 0,
                Resource = Guid.NewGuid(),
                IsAcceptedRow = true
            };
        }

        /// <summary>
        /// Create new a VendorModel and some default value
        /// </summary>
        private void NewVendor()
        {
            SelectedVendor = new base_GuestModel { Mark = MarkType.Vendor.ToDescription() };
            SelectedVendor.IsActived = true;
            SelectedVendor.IsPrimary = true;
            SelectedVendor.DateCreated = DateTimeExt.Now;
            SelectedVendor.GuestNo = DateTimeExt.Now.ToString(Define.GuestNoFormat);
            //SelectedVendor.Mark = MarkType.Vendor.ToDescription();
            SelectedVendor.PositionId = 0;
            if (Define.USER != null)
                SelectedVendor.UserCreated = Define.USER.LoginName;
            // Initial address control collection
            // Created by Thaipn.
            SelectedVendor.AddressControlCollection = new AddressControlCollection();
            SelectedVendor.AddressControlCollection.Add(new AddressControlModel
            {
                IsDefault = true,
                IsNew = true
            });
            SelectedVendor.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>();
            SelectedVendor.AdditionalModel = new base_GuestAdditionalModel();

            SelectedVendor.ContactCollection = new CollectionBase<base_GuestModel>();
            SelectedVendor.Resource = Guid.NewGuid();
            SelectedVendor.ResourceNoteCollection = new CollectionBase<base_ResourceNoteModel>();
            SelectedVendor.IsDirty = false;
        }

        /// <summary>
        /// Function save Vendor
        /// </summary>
        /// <param name="param"></param>
        private bool SaveVendor(string param)
        {
            if (!CheckDuplicateVendor(SelectedVendor))
            {

                //Handle clear data if user not choose "Tax Information" in Additional with FexTaxId & TaxLocation
                if (SelectedVendor.AdditionalModel.TaxInfoType.Is(TaxInfoType.FedTaxID))
                    SelectedVendor.AdditionalModel.SaleTaxLocation = 0;
                else
                    SelectedVendor.AdditionalModel.FedTaxId = string.Empty;

                // Vendor is create new
                if (SelectedVendor.IsNew)
                {
                    // Insert a new vendor
                    SaveNew();

                    if (SelectedVendor.ContactCollection.Count == 0)
                    {
                        SelectedVendor.ContactCollection.Add(CreateNewContact(true));
                    }
                }
                else    // Vendor is edited
                {
                    // Update vendor
                    SaveUpdate();
                }

                // Turn off IsDirty & IsNew
                SelectedVendor.EndUpdate();
                SelectedVendor.AdditionalModel.EndUpdate();
                IsDirtyContactCollection = false;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Save when create new Vendor
        /// </summary>
        private void SaveNew()
        {
            // Map data from model to entity
            SelectedVendor.ToEntity();

            // Insert address
            // Created by Thaipn
            foreach (AddressControlModel addressControlModel in this.SelectedVendor.AddressControlCollection)
            {
                base_GuestAddressModel addressModel = new base_GuestAddressModel();
                addressModel.DateCreated = DateTimeExt.Now;
                //addressModel.UserCreated = string.Empty;
                // Map date from AddressControlModel to AddressModel
                addressModel.ToModel(addressControlModel);

                // Update default address
                if (addressModel.IsDefault)
                    SelectedVendor.AddressModel = addressModel;

                // Map data from model to entity
                addressModel.ToEntity();
                SelectedVendor.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);

                // Turn off IsDirty & IsNew
                addressModel.EndUpdate();

                addressControlModel.IsNew = false;
                addressControlModel.IsDirty = false;
            }

            // Save image
            if (SelectedVendor.PhotoCollection != null && SelectedVendor.PhotoCollection.Count > 0)
            {
                foreach (base_ResourcePhotoModel photoModel in SelectedVendor.PhotoCollection.Where(x => x.IsNew))
                {
                    //photoModel.LargePhotoFilename = new System.IO.FileInfo(photoModel.ImagePath).Name;
                    photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;

                    // Update resource photo
                    photoModel.Resource = SelectedVendor.Resource.ToString();

                    // Map data from model to entity
                    photoModel.ToEntity();
                    _photoRepository.Add(photoModel.base_ResourcePhoto);

                    // Copy image from client to server
                    SaveImage(photoModel);

                    // Turn off IsDirty & IsNew
                    photoModel.EndUpdate();
                }
            }

            // Update default photo if it is deleted
            SelectedVendor.PhotoDefault = SelectedVendor.PhotoCollection.FirstOrDefault();

            foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection.Where(x => x.IsDirty))
            {
                // Create new personal info for contact
                if (contactModel.PersonalInfoModel != null)
                {
                    // Map data from model to entity
                    contactModel.PersonalInfoModel.ToEntity();

                    // Create new personal info
                    contactModel.base_Guest.base_GuestProfile.Add(contactModel.PersonalInfoModel.base_GuestProfile);

                    // Turn off IsDirty & IsNew
                    contactModel.PersonalInfoModel.EndUpdate();
                }

                // Map data from model to entity
                contactModel.ToEntity();
                SelectedVendor.base_Guest.base_Guest1.Add(contactModel.base_Guest);
            }

            SelectedVendor.AdditionalModel.ToEntity();
            SelectedVendor.base_Guest.base_GuestAdditional.Add(SelectedVendor.AdditionalModel.base_GuestAdditional);

            _guestRepository.Add(SelectedVendor.base_Guest);
            _guestRepository.Commit();

            // Update ID from entity to model
            SelectedVendor.Id = SelectedVendor.base_Guest.Id;
            SelectedVendor.AdditionalModel.GuestId = SelectedVendor.base_Guest.Id;
            SelectedVendor.AdditionalModel.Id = SelectedVendor.AdditionalModel.base_GuestAdditional.Id;
            foreach (base_ResourcePhotoModel photoModel in SelectedVendor.PhotoCollection)
            {
                photoModel.Id = photoModel.base_ResourcePhoto.Id;

                // Turn off IsDirty & IsNew
                photoModel.EndUpdate();
            }

            foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection)
            {
                contactModel.Id = contactModel.base_Guest.Id;
                contactModel.ParentId = SelectedVendor.base_Guest.Id;

                // Turn off IsDirty & IsNew
                contactModel.EndUpdate();
            }

            // Push new vendor to collection
            VendorCollection.Add(SelectedVendor);
        }

        /// <summary>
        /// Update a vendor
        /// </summary>
        private void SaveUpdate()
        {
            SelectedVendor.DateUpdated = DateTimeExt.Now;
            if (Define.USER != null)
                SelectedVendor.UserUpdated = Define.USER.LoginName;

            // Map data from model to entity
            SelectedVendor.ToEntity();

            #region Save address

            // Insert or update address
            // Created by Thaipn
            foreach (AddressControlModel addressControlModel in this.SelectedVendor.AddressControlCollection.Where(x => x.IsDirty))
            {
                base_GuestAddressModel addressModel = new base_GuestAddressModel();

                // Insert new address
                if (addressControlModel.IsNew)
                {
                    addressModel.DateCreated = DateTimeExt.Now;
                    //addressModel.UserCreated = string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);

                    // Map data from model to entity
                    addressModel.ToEntity();
                    SelectedVendor.base_Guest.base_GuestAddress.Add(addressModel.base_GuestAddress);
                }
                // Update address
                else
                {
                    base_GuestAddress address = SelectedVendor.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId == addressControlModel.AddressTypeID);
                    addressModel = new base_GuestAddressModel(address);

                    addressModel.DateUpdated = DateTimeExt.Now;
                    //addressModel.UserUpdated = string.Empty;
                    // Map date from AddressControlModel to AddressModel
                    addressModel.ToModel(addressControlModel);
                    addressModel.ToEntity();
                }

                // Update default address
                if (addressModel.IsDefault)
                    SelectedVendor.AddressModel = addressModel;

                // Turn off IsDirty & IsNew
                addressModel.EndUpdate();

                addressControlModel.IsNew = false;
                addressControlModel.IsDirty = false;
            }

            #endregion

            #region Save photo

            // Remove photo were deleted
            if (SelectedVendor.PhotoCollection != null &&
                SelectedVendor.PhotoCollection.DeletedItems != null && SelectedVendor.PhotoCollection.DeletedItems.Count > 0)
            {
                foreach (base_ResourcePhotoModel photoModel in SelectedVendor.PhotoCollection.DeletedItems)
                {
                    //System.IO.FileInfo fileInfo = new System.IO.FileInfo(photoModel.ImagePath);
                    //fileInfo.MoveTo(photoModel.ImagePath + "temp");
                    //System.IO.FileInfo fileInfoTemp = new System.IO.FileInfo(photoModel.ImagePath + "temp");
                    //fileInfoTemp.Delete();

                    _photoRepository.Delete(photoModel.base_ResourcePhoto);
                }
                SelectedVendor.PhotoCollection.DeletedItems.Clear();
            }

            // Update photo
            if (SelectedVendor.PhotoCollection != null && SelectedVendor.PhotoCollection.Count > 0)
            {
                foreach (base_ResourcePhotoModel photoModel in SelectedVendor.PhotoCollection.Where(x => x.IsDirty))
                {
                    //photoModel.LargePhotoFilename = new System.IO.FileInfo(photoModel.ImagePath).Name;
                    photoModel.LargePhotoFilename = DateTimeExt.Now.ToString(Define.GuestNoFormat) + Guid.NewGuid().ToString().Substring(0, 8) + new System.IO.FileInfo(photoModel.ImagePath).Extension;

                    // Update resource photo
                    if (string.IsNullOrWhiteSpace(photoModel.Resource))
                        photoModel.Resource = SelectedVendor.Resource.ToString();

                    // Map data from model to entity
                    photoModel.ToEntity();

                    if (photoModel.IsNew)
                        _photoRepository.Add(photoModel.base_ResourcePhoto);

                    // Copy image from client to server
                    SaveImage(photoModel);

                    // Turn off IsDirty & IsNew
                    photoModel.EndUpdate();
                }
            }

            // Update default photo if it is deleted
            SelectedVendor.PhotoDefault = SelectedVendor.PhotoCollection.FirstOrDefault();

            #endregion

            #region Save contact

            if (SelectedVendor.ContactCollection != null)
            {
                // Remove contact were deleted
                if (SelectedVendor.ContactCollection.DeletedItems != null)
                {
                    foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection.DeletedItems)
                    {
                        if (!contactModel.IsNew)
                            contactModel.base_Guest.IsPurged = true;

                        //if (contactModel.IsNew)
                        //    SelectedVendor.ContactCollection.Remove(contactModel);
                        //else
                        //{
                        //    SelectedVendor.base_Guest.base_Guest1.Remove(contactModel.base_Guest);
                        //    _guestRepository.Delete(contactModel.base_Guest);
                        //}
                    }
                    SelectedVendor.ContactCollection.DeletedItems.Clear();
                }

                // Update contact
                foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection.
                    Where(x => !x.IsAcceptedRow && (x.IsDirty || (x.PersonalInfoModel != null && x.PersonalInfoModel.IsDirty))))
                {
                    // Update personal info for contact
                    if (contactModel.PersonalInfoModel != null)
                    {
                        // Map data from model to entity
                        contactModel.PersonalInfoModel.ToEntity();

                        // Create new personal info
                        if (contactModel.PersonalInfoModel.IsNew)
                            contactModel.base_Guest.base_GuestProfile.Add(contactModel.PersonalInfoModel.base_GuestProfile);

                        // Turn off IsDirty & IsNew
                        contactModel.PersonalInfoModel.EndUpdate();
                    }

                    // Create new contact
                    if (contactModel.IsNew)
                    {
                        //if (contactModel.IsPrimary)
                        //    contactModel.IsPrimary = false;

                        // Map data from model to entity
                        contactModel.ToEntity();
                        SelectedVendor.base_Guest.base_Guest1.Add(contactModel.base_Guest);
                    }
                    else // Update contact
                        // Map data from model to entity
                        contactModel.ToEntity();

                    // Turn off IsDirty & IsNew
                    contactModel.EndUpdate();
                }
            }

            #endregion

            SelectedVendor.AdditionalModel.GuestId = SelectedVendor.Id;

            // Map data from model to entity
            SelectedVendor.AdditionalModel.ToEntity();

            if (SelectedVendor.base_Guest.base_GuestAdditional.Count == 0)
                SelectedVendor.base_Guest.base_GuestAdditional.Add(SelectedVendor.AdditionalModel.base_GuestAdditional);

            _guestRepository.Commit();

            foreach (base_GuestModel contactModel in SelectedVendor.ContactCollection.Where(x => !x.ParentId.HasValue))
            {
                contactModel.Id = contactModel.base_Guest.Id;
                contactModel.ParentId = SelectedVendor.base_Guest.Id;

                // Turn off IsDirty & IsNew
                contactModel.EndUpdate();
            }
            if (SelectedVendor.AdditionalModel.IsNew)
                SelectedVendor.AdditionalModel.Id = SelectedVendor.AdditionalModel.base_GuestAdditional.Id;
        }

        /// <summary>
        /// Create predicate
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreateSearchVendorPredicate(string keyword)
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
                if (SearchOption.Has(SearchOptions.Email))
                {
                    predicate = predicate.And(x => x.Email.ToLower().Contains(keyword.ToLower()));
                }
                if (SearchOption.Has(SearchOptions.Phone))
                {
                    predicate = predicate.And(x => x.Phone1.ToLower().Contains(keyword.ToLower()) || x.Phone2.ToLower().Contains(keyword.ToLower()));
                }
            }
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_vendorMark));
            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadVendorDataByPredicate(Expression<Func<base_Guest, bool>> predicate, bool refreshData = false, int currentIndex = 0)
        {
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            if (currentIndex == 0)
                VendorCollection.Clear();
            bgWorker.DoWork += (sender, e) =>
            {
                if (Define.DisplayLoading)
                    IsBusy = true;
                SetSearchFocus = false;

                if (refreshData)
                {
                    _guestRepository.Refresh();
                    _addressRepository.Refresh();
                    _photoRepository.Refresh();
                    _additionalRepository.Refresh();
                }

                //Cout all Customer in Data base show on grid
                //TotalCustomers = _guestRepository.GetIQueryable(predicate).Count();

                //Get data with range
                //IList<base_Guest> customers = _guestRepository.GetRange<string>(currentIndex, NumberOfDisplayItems, x => x.GuestNo, predicate);
                IList<base_Guest> customers = _guestRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.GuestNo", predicate);
                foreach (base_Guest customer in customers)
                {
                    bgWorker.ReportProgress(0, customer);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                base_GuestModel vendorModel = new base_GuestModel((base_Guest)e.UserState);
                LoadRelationVendorData(vendorModel);
                VendorCollection.Add(vendorModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                IsBusy = false;
                SetSearchFocus = true;
            };
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for vendor
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadRelationVendorData(base_GuestModel vendorModel)
        {
            // Load Address
            if (vendorModel.AddressCollection == null)
            {
                vendorModel.AddressCollection = new ObservableCollection<base_GuestAddressModel>(
                    vendorModel.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));
                vendorModel.AddressModel = vendorModel.AddressCollection.SingleOrDefault(x => x.IsDefault);
                vendorModel.AddressControlCollection = new AddressControlCollection();
                foreach (base_GuestAddressModel addressModel in vendorModel.AddressCollection)
                {
                    AddressControlModel addressControlModel = addressModel.ToAddressControlModel();
                    addressControlModel.IsDirty = false;
                    vendorModel.AddressControlCollection.Add(addressControlModel);
                }
            }

            // Load Photo
            if (vendorModel.PhotoCollection == null)
            {
                string resource = vendorModel.Resource.ToString();
                vendorModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                    _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourcePhotoModel(x)
                    {
                        ImagePath = IMG_VENDOR_DIRECTORY + vendorModel.GuestNo + "\\" + x.LargePhotoFilename,
                        IsDirty = false
                    }));

                // Set default photo
                vendorModel.PhotoDefault = vendorModel.PhotoCollection.FirstOrDefault();
            }

            // Load Additional
            if (vendorModel.AdditionalModel == null)
            {
                if (vendorModel.base_Guest.base_GuestAdditional.Count > 0)
                {
                    vendorModel.AdditionalModel = new base_GuestAdditionalModel(
                        vendorModel.base_Guest.base_GuestAdditional.FirstOrDefault());
                    //Set Tax Infomation with FeedTaxId & TaxLocation
                    if (vendorModel.AdditionalModel.SaleTaxLocation == 0)
                        vendorModel.AdditionalModel.TaxInfoType = (int)TaxInfoType.FedTaxID;
                    else
                        vendorModel.AdditionalModel.TaxInfoType = (int)TaxInfoType.TaxLocation;
                    vendorModel.AdditionalModel.EndUpdate();
                }
                else
                    vendorModel.AdditionalModel = new base_GuestAdditionalModel();
            }

            // Load Contact
            if (vendorModel.ContactCollection == null)
            {
                vendorModel.ContactCollection = new CollectionBase<base_GuestModel>(
                    vendorModel.base_Guest.base_Guest1.Where(y => !y.IsPurged).Select(y => new base_GuestModel(y)));

                vendorModel.ContactCollection.Add(
                    CreateNewContact(!vendorModel.ContactCollection.Any(x => x.IsPrimary)));
            }

            // Add new temporary contact
            vendorModel.ContactModel = new base_GuestModel();

            // Load Note
            LoadNote(vendorModel);
        }

        /// <summary>
        /// Load purchase order collection
        /// </summary>
        /// <param name="vendorModel"></param>
        private void LoadPurchaseOrderCollection(base_GuestModel vendorModel)
        {
            if (vendorModel.PurchaseOrderCollection == null)
            {
                // Get vendor resource
                string vendorResource = vendorModel.Resource.ToString();

                // Initial purchase order collection
                vendorModel.PurchaseOrderCollection = new ObservableCollection<base_PurchaseOrderModel>(_purchaseOrderRepository.
                    GetAll(x => x.VendorResource.Equals(vendorResource)).Select(x => new base_PurchaseOrderModel(x)));

                TotalPurchaseOrder = new base_PurchaseOrderModel
                {
                    Total = vendorModel.PurchaseOrderCollection.Sum(x => x.Total),
                    Paid = vendorModel.PurchaseOrderCollection.Sum(x => x.Paid),
                    Balance = vendorModel.PurchaseOrderCollection.Sum(x => x.Balance)
                };
            }
        }

        private void LoadProductVendorCollection(base_GuestModel vendorModel)
        {
            if (vendorModel.ProductCollection == null)
            {
                // Initial purchase order collection
                vendorModel.ProductCollection = new ObservableCollection<base_ProductModel>();

                foreach (base_Product product in _productRepository.GetAll(x => x.IsPurge == false && x.VendorId.Equals(vendorModel.Id)))
                {
                    base_ProductModel productModel = new base_ProductModel(product);
                    LoadRelationProductData(productModel);
                    vendorModel.ProductCollection.Add(productModel);

                    // Turn off IsDirty & IsNew
                    productModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Check Customer is duplicate
        /// <para>Value Compare : Email,Phone1</para>
        /// </summary>
        /// 
        /// <param name="vendorModel"></param>
        /// 
        /// <returns></returns>
        private bool CheckDuplicateVendor(base_GuestModel vendorModel)
        {
            bool result = false;
            IQueryable<base_Guest> query = _guestRepository.GetIQueryable(x => !x.GuestNo.Equals(vendorModel.GuestNo)
                                                                               && x.Mark.Equals(_vendorMark)
                                                                               && !x.IsPurged
                                                                               && (x.Phone1.Equals(vendorModel.Phone1)
                                                                               || x.Email.ToLower().Equals(vendorModel.Email.ToLower())));
            if (query.Count() > 0)
            {
                result = true;
                MessageBoxResult resultMsg = MessageBox.Show("This vendor has existed. Please recheck Email or Phone. Do you want to view profiles?", "POS", MessageBoxButton.YesNo);
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

        #region VendorProductTab

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Product, bool>> CreateSearchProductPredicate(string keyword)
        {
            // Initial predicate
            Expression<Func<base_Product, bool>> predicate = PredicateBuilder.True<base_Product>();

            // Default condition
            predicate = predicate.And(x => x.IsPurge == false && x.VendorId.Equals(SelectedVendor.Id));

            return predicate;
        }

        /// <summary>
        /// Method get Data from database
        /// <para>Using load on the first time</para>
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="refreshData"></param>
        /// <param name="currentIndex">index to load if index =0 , clear collection</param>
        private void LoadProductDataByPredicate(bool refreshData = false, int currentIndex = 0)
        {
            // Create predicate
            Expression<Func<base_Product, bool>> predicate = CreateSearchProductPredicate(Keyword);

            // Create background worker
            BackgroundWorker bgWorker = new BackgroundWorker { WorkerReportsProgress = true };

            if (currentIndex == 0)
                SelectedVendor.ProductCollection = new ObservableCollection<base_ProductModel>();

            bgWorker.DoWork += (sender, e) =>
            {
                // Turn on BusyIndicator
                if (Define.DisplayLoading)
                    IsBusy = true;

                if (refreshData)
                {

                }

                // Get total products with condition in predicate
                TotalProducts = _productRepository.GetIQueryable(predicate).Count();

                // Get data with range
                IList<base_Product> products = _productRepository.GetRange(currentIndex, NumberOfDisplayItems, "It.Id", predicate);
                foreach (base_Product product in products)
                {
                    bgWorker.ReportProgress(0, product);
                }
            };

            bgWorker.ProgressChanged += (sender, e) =>
            {
                // Create product model
                base_ProductModel productModel = new base_ProductModel((base_Product)e.UserState);

                // Load relation data
                LoadRelationProductData(productModel);

                // Add to collection
                SelectedVendor.ProductCollection.Add(productModel);
            };

            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Turn off BusyIndicator
                IsBusy = false;
            };

            // Run async background worker
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Load relation data for product
        /// </summary>
        /// <param name="productModel"></param>
        private void LoadRelationProductData(base_ProductModel productModel)
        {
            // Load Photo
            if (productModel.PhotoCollection == null)
            {
                string resource = productModel.Resource.ToString();
                productModel.PhotoCollection = new CollectionBase<base_ResourcePhotoModel>(
                    _photoRepository.GetAll(x => x.Resource.Equals(resource)).
                    Select(x => new base_ResourcePhotoModel(x)
                    {
                        ImagePath = Path.Combine(IMG_PRODUCT_DIRECTORY, productModel.Code, x.LargePhotoFilename),
                        IsDirty = false
                    }));

                // Set default photo
                productModel.PhotoDefault = productModel.PhotoCollection.FirstOrDefault();
            }

            // Get category name for product
            if (string.IsNullOrWhiteSpace(productModel.CategoryName))
            {
                base_DepartmentModel categoryItem = CategoryList.FirstOrDefault(x => x.Id.Equals(productModel.ProductCategoryId));
                if (categoryItem != null)
                    productModel.CategoryName = categoryItem.Name;
            }

            // Get uom name for product
            if (string.IsNullOrWhiteSpace(productModel.UOMName))
            {
                CheckBoxItemModel uomItem = UOMList.FirstOrDefault(x => x.Value.Equals(productModel.BaseUOMId));
                if (uomItem != null)
                    productModel.UOMName = uomItem.Text;
            }
        }

        #endregion

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
                    _noteRepository.GetAll(x => x.Resource.Equals(resource)).
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
                _noteRepository.Add(noteModel.base_ResourceNote);
            _noteRepository.Commit();
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
            foreach (base_ResourceNoteModel noteModel in SelectedVendor.ResourceNoteCollection)
                _noteRepository.Delete(noteModel.base_ResourceNote);
            _noteRepository.Commit();
            SelectedVendor.ResourceNoteCollection.Clear();
        }

        /// <summary>
        /// Close popup notes
        /// </summary>
        private void CloseAllPopupNote()
        {
            // Remove popup note
            foreach (PopupContainer popupContainer in NotePopupCollection)
                popupContainer.Close();
            NotePopupCollection.Clear();
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
            noteViewModel.ResourceNoteCollection = SelectedVendor.ResourceNoteCollection;
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

        private void DeleteNoteExt(base_GuestModel model)
        {
            // Remove popup note
            CloseAllPopupNote();

            // Delete note
            foreach (base_ResourceNoteModel noteModel in model.ResourceNoteCollection)
                _noteRepository.Delete(noteModel.base_ResourceNote);
            _noteRepository.Commit();
            model.ResourceNoteCollection.Clear();
        }
        #endregion

        #region Override Methods

        /// <summary>
        /// Load data when open form
        /// </summary>
        public override void LoadData()
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            if (!string.IsNullOrWhiteSpace(Keyword) && SearchOption > 0)//Load with Search Condition
            {
                predicate = CreateSearchVendorPredicate(Keyword);
            }
            else
            {
                predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_vendorMark));
            }
            LoadVendorDataByPredicate(predicate, true);



        }

        /// <summary>
        /// Switch to search mode
        /// </summary>
        /// <param name="isList"></param>
        public override void ChangeSearchMode(bool isList, object param = null)
        {
            if (param == null)
            {
                if (ShowNotification(null))
                {
                    if (!isList)
                    {
                        NewVendor();
                        IsSearchMode = false;
                    }
                    else
                        IsSearchMode = true;
                }
            }
        }

        /// <summary>
        /// Show notification when exit or change form
        /// </summary>
        /// <param name="isClosing"></param>
        /// <returns></returns>
        protected override bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return ShowNotification(isClosing);
        }

        #endregion

        #region Save Image

        /// <summary>
        /// Get product image folder
        /// </summary>
        private string IMG_PRODUCT_DIRECTORY = Path.Combine(Define.ImageFilesFolder, "Product");

        /// <summary>
        /// Get vendor image folder
        /// </summary>
        private string IMG_VENDOR_DIRECTORY = Path.Combine(Define.ImageFilesFolder, "Vendor");

        /// <summary>
        /// Copy image to server folder
        /// </summary>
        /// <param name="model"></param>
        private void SaveImage(base_ResourcePhotoModel model)
        {
            try
            {
                // Server image path
                string imgGuestDirectory = Path.Combine(IMG_VENDOR_DIRECTORY, SelectedVendor.GuestNo);

                // Create folder image on server if is not exist
                if (!Directory.Exists(imgGuestDirectory))
                    Directory.CreateDirectory(imgGuestDirectory);

                // Check client image to copy to server
                FileInfo clientFileInfo = new FileInfo(model.ImagePath);
                if (clientFileInfo.Exists)
                {
                    // Get file name image
                    string serverFileName = Path.Combine(imgGuestDirectory, model.LargePhotoFilename);
                    FileInfo serverFileInfo = new FileInfo(serverFileName);
                    if (!serverFileInfo.Exists)
                        clientFileInfo.CopyTo(serverFileName, true);
                    model.ImagePath = serverFileName;
                }
                else
                    model.ImagePath = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save Image" + ex.ToString());
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
