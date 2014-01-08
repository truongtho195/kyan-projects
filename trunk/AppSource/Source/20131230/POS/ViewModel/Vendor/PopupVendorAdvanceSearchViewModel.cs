using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupVendorAdvanceSearchViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate
        /// </summary>
        public Expression<Func<base_Guest, bool>> AdvanceSearchPredicate { get; private set; }

        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty { get; set; }

        private string _firstName;
        /// <summary>
        /// Gets or sets the FirstName.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    this.IsDirty = true;
                    _firstName = value;
                    OnPropertyChanged(() => FirstName);
                }
            }
        }

        private string _lastName;
        /// <summary>
        /// Gets or sets the LastName.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    this.IsDirty = true;
                    _lastName = value;
                    OnPropertyChanged(() => LastName);
                }
            }
        }

        private string _phone;
        /// <summary>
        /// Gets or sets the Phone.
        /// </summary>
        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value)
                {
                    this.IsDirty = true;
                    _phone = value;
                    OnPropertyChanged(() => Phone);
                }
            }
        }

        private string _email;
        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    this.IsDirty = true;
                    _email = value;
                    OnPropertyChanged(() => Email);
                }
            }
        }

        private string _company;
        /// <summary>
        /// Gets or sets the Company.
        /// </summary>
        public string Company
        {
            get { return _company; }
            set
            {
                if (_company != value)
                {
                    this.IsDirty = true;
                    _company = value;
                    OnPropertyChanged(() => Company);
                }
            }
        }

        private int _addressTypeID;
        /// <summary>
        /// Gets or sets the AddressTypeID.
        /// </summary>
        public int AddressTypeID
        {
            get { return _addressTypeID; }
            set
            {
                if (_addressTypeID != value)
                {
                    this.IsDirty = true;
                    _addressTypeID = value;
                    OnPropertyChanged(() => AddressTypeID);
                }
            }
        }

        /// <summary>
        /// Gets or sets the AddressTypeList
        /// </summary>
        public List<ComboItem> AddressTypeList { get; set; }

        private string _address;
        /// <summary>
        /// Gets or sets the Address.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set
            {
                if (_address != value)
                {
                    this.IsDirty = true;
                    _address = value;
                    OnPropertyChanged(() => Address);
                }
            }
        }

        private string _fedTaxID;
        /// <summary>
        /// Gets or sets the FedTaxID.
        /// </summary>
        public string FedTaxID
        {
            get { return _fedTaxID; }
            set
            {
                if (_fedTaxID != value)
                {
                    this.IsDirty = true;
                    _fedTaxID = value;
                    OnPropertyChanged(() => FedTaxID);
                }
            }
        }

        private short _status;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    this.IsDirty = true;
                    _status = value;
                    OnPropertyChanged(() => Status);
                }
            }
        }

        private string _groupResource;
        /// <summary>
        /// Gets or sets the GroupResource.
        /// </summary>
        public string GroupResource
        {
            get { return _groupResource; }
            set
            {
                if (_groupResource != value)
                {
                    this.IsDirty = true;
                    _groupResource = value;
                    OnPropertyChanged(() => GroupResource);
                }
            }
        }

        /// <summary>
        /// Get or sets the GroupList
        /// </summary>
        public List<ComboItem> GroupList { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupVendorAdvanceSearchViewModel()
        {
            LoadStaticData();
            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return IsDirty;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            // Create advance search predicate
            AdvanceSearchPredicate = CreateAdvanceSearchPredicate();

            Window window = FindOwnerWindow(this);
            window.DialogResult = true;
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Load static data
        /// </summary>
        private void LoadStaticData()
        {
            // Load address type list
            AddressTypeList = new List<ComboItem>();
            AddressTypeList.Add(new ComboItem { IntValue = 0, Text = "Home" });
            AddressTypeList.Add(new ComboItem { IntValue = 1, Text = "Business" });
            AddressTypeList.Add(new ComboItem { IntValue = 2, Text = "Billing" });
            AddressTypeList.Add(new ComboItem { IntValue = 3, Text = "Shipping" });

            base_GuestGroupRepository guestGroupRepository = new base_GuestGroupRepository();

            // Load group list
            GroupList = new List<ComboItem>(guestGroupRepository.GetAll().
                OrderBy(x => x.Name).
                Select(x => new ComboItem
                {
                    ObjValue = x.Resource.ToString(),
                    Text = x.Name
                }));
            GroupList.Insert(0, new ComboItem());
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Guest, bool>> CreateAdvanceSearchPredicate()
        {
            // Create predicate
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                // Get all vendors that FirstName contain keyword
                predicate = predicate.And(x => x.FirstName.ToLower().Contains(FirstName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                // Get all vendors that FirstName contain keyword
                predicate = predicate.And(x => x.LastName.ToLower().Contains(LastName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                // Get all vendors that Phone contain keyword
                predicate = predicate.And(x => x.Phone1.ToLower().Contains(Phone.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Email))
            {
                // Get all vendors that Email contain keyword
                predicate = predicate.And(x => x.Email.ToLower().Contains(Email.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(Company))
            {
                // Get all vendors that Company contain keyword
                predicate = predicate.And(x => x.Company.ToLower().Contains(Company.ToLower()));
            }
            if (AddressTypeID > 0)
            {
                // Get all vendors that AddressTypeID equal keyword
                predicate = predicate.And(x => x.base_GuestAddress.Any(y => y.AddressTypeId == AddressTypeID));
            }
            if (!string.IsNullOrWhiteSpace(Address))
            {
                Expression<Func<base_Guest, bool>> addressPredicate = PredicateBuilder.False<base_Guest>();

                // Get all vendors that AddressLine contain keyword
                addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => y.AddressLine1.ToLower().Contains(Address.ToLower())));

                // Get all countries that contain keyword
                IEnumerable<ComboItem> countryItems = Common.Countries.Where(x => x.Text.ToLower().Contains(Address.ToLower()));
                IEnumerable<int> countryItemIDList = countryItems.Select(x => (int)x.Value);

                // Get all vendors that Country contain keyword
                if (countryItemIDList.Count() > 0)
                    addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => countryItemIDList.Contains(y.CountryId)));

                // Get all vendors that City contain keyword
                addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => y.City.ToLower().Contains(Address.ToLower())));

                // Get all states that contain keyword
                IEnumerable<ComboItem> stateItems = Common.States.Where(x => x.Text.ToLower().Contains(Address.ToLower()));
                IEnumerable<int> stateItemIDList = stateItems.Select(x => (int)x.Value);

                // Get all vendors that State contain keyword
                if (stateItemIDList.Count() > 0)
                    addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => stateItemIDList.Contains(y.StateProvinceId)));

                // Get all vendors that PostalCode contain keyword
                addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => y.PostalCode.ToLower().Contains(Address.ToLower())));

                // Get all vendors that AddressTypeID equal keyword
                addressPredicate = addressPredicate.And(x => x.base_GuestAddress.Any(y => y.AddressTypeId.Equals(AddressTypeID)));

                // Get all vendors that Address contain keyword
                predicate = predicate.And(addressPredicate);
            }
            if (!string.IsNullOrWhiteSpace(FedTaxID))
            {
                // Get all vendors that FedTaxID contain keyword
                predicate = predicate.And(x => x.base_GuestAdditional.Any(y => y.FedTaxId.ToLower().Contains(FedTaxID.ToLower())));
            }
            if (Status > 0)
            {
                // Get all vendors that Status equal keyword
                predicate = predicate.And(x => x.IsActived.Equals(Status == 1));
            }
            if (!string.IsNullOrWhiteSpace(GroupResource))
            {
                // Get all vendors that Group equal keyword
                predicate = predicate.And(x => x.GroupResource.Equals(GroupResource));
            }

            string vendorMark = MarkType.Vendor.ToDescription();

            // Default condition
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(vendorMark));

            return predicate;
        }

        #endregion
    }
}