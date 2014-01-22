using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    class EmployeeAdvanceSearchViewModel : ViewModelBase
    {
        #region Define
        private string _employeeMark = MarkType.Employee.ToDescription();
        #endregion

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

        #region AddressTypeCollection
        private ObservableCollection<ComboItem> _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressCollection.
        /// </summary>
        public ObservableCollection<ComboItem> AddressTypeCollection
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

        #region AddressTypeId
        private int _addressTypeId;
        /// <summary>
        /// Gets or sets the AddressTypeId.
        /// </summary>
        public int AddressTypeId
        {
            get { return _addressTypeId; }
            set
            {
                if (_addressTypeId != value)
                {
                    _addressTypeId = value;
                    this.IsDirty = true;
                    OnPropertyChanged(() => AddressTypeId);
                }
            }
        }
        #endregion

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

        #region JobTitleCollection
        private ObservableCollection<ComboItem> _jobTitleCollection;
        /// <summary>
        /// Gets or sets the JobTitleCollection.
        /// </summary>
        public ObservableCollection<ComboItem> JobTitleCollection
        {
            get { return _jobTitleCollection; }
            set
            {
                if (_jobTitleCollection != value)
                {
                    _jobTitleCollection = value;
                    OnPropertyChanged(() => JobTitleCollection);
                }
            }
        }
        #endregion

        private Nullable<short> _positionId;
        /// <summary>
        /// Gets or sets the PositionId.
        /// </summary>
        public Nullable<short> PositionId
        {
            get { return _positionId; }
            set
            {
                if (_positionId != value)
                {
                    this.IsDirty = true;
                    _positionId = value;
                    OnPropertyChanged(() => PositionId);
                }
            }
        }

        private string _SSN;
        /// <summary>
        /// Gets or sets the SSN.
        /// </summary>
        public string SSN
        {
            get { return _SSN; }
            set
            {
                if (_SSN != value)
                {
                    this.IsDirty = true;
                    _SSN = value;
                    OnPropertyChanged(() => SSN);
                }
            }
        }

        private string _ID;
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    this.IsDirty = true;
                    _ID = value;
                    OnPropertyChanged(() => ID);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmployeeAdvanceSearchViewModel()
        {
            LoadStaticData();
            InitialCommand();
            AddressTypeId = Convert.ToInt32(AddressTypeCollection.FirstOrDefault().ObjValue);
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
            AdvanceSearchPredicate = CreateAdvanceSearchPredicate();
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
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
            //Load Collection address Type to combobox
            this.AddressTypeCollection = new ObservableCollection<ComboItem>();
            this.AddressTypeCollection.Add(new ComboItem { ObjValue = 0, Text = Language.GetMsg("SO_TextBlock_Home") });
            this.AddressTypeCollection.Add(new ComboItem { ObjValue = 1, Text = Language.GetMsg("SO_TextBlock_Business") });
            this.AddressTypeCollection.Add(new ComboItem { ObjValue = 2, Text = Language.GetMsg("SO_TextBlock_Billing") });
            this.AddressTypeCollection.Add(new ComboItem { ObjValue = 3, Text = Language.GetMsg("SO_TextBlock_Shipping") });

            base_GenericCodeRepository _genericCodeRepository = new base_GenericCodeRepository();
            string JobTitleCode = GenericCode.JT.ToString();
            IList<base_GenericCode> codes = _genericCodeRepository.GetAll(x => x.Code.Equals(JobTitleCode));
            this.JobTitleCollection = new ObservableCollection<ComboItem>();
            foreach (var item in codes)
            {
                ComboItem ItemJobTitle = new ComboItem()
                {
                    ObjValue = item.Id,
                    Value = Convert.ToInt16(item.Id),
                    Text = item.Name,
                    Symbol = item.Code
                };
                this.JobTitleCollection.Add(ItemJobTitle);
            }
        }

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            this.OkCommand = new RelayCommand(this.OnOkCommandExecute, this.OnOkCommandCanExecute);
            this.CancelCommand = new RelayCommand(this.OnCancelCommandExecute, this.OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Create predicate with condition for search
        /// </summary>
        /// <param name="keyword">Keyword</param>
        /// <returns>Expression</returns>
        private Expression<Func<base_Guest, bool>> CreateAdvanceSearchPredicate()
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();
            predicate = predicate.And(x => !x.IsPurged && x.Mark.Equals(_employeeMark));
            //Seach empployee By FirstName
            if (!string.IsNullOrWhiteSpace(FirstName))
                // Get all empployee that FirstName contain keyword
                predicate = predicate.And(x => x.FirstName.ToLower().Contains(FirstName.ToLower()));

            //Seach empployee By LastName
            if (!string.IsNullOrWhiteSpace(LastName))
                // Get all empployee that FirstName contain keyword
                predicate = predicate.And(x => x.LastName.ToLower().Contains(LastName.ToLower()));


            //Seach empployee By Phone
            if (!string.IsNullOrWhiteSpace(Phone))
                // Get all empployee that Phone contain keyword
                predicate = predicate.And(x => x.Phone1.ToLower().Contains(Phone.ToLower()));

            //Seach empployee By Email
            if (!string.IsNullOrWhiteSpace(Email))
                // Get all empployee that Email contain keyword
                predicate = predicate.And(x => x.Email.ToLower().Contains(Email.ToLower()));

            //Search empployee by Status
            if (this.Status > 0)
                // Get all empployee that Status equal keyword
                predicate = predicate.And(x => x.IsActived.Equals(Status == 1));

            //Search empployee by JobTitle
            if (this.PositionId.HasValue && this.PositionId.Value > 0)
                // Get all empployee that Group equal keyword
                predicate = predicate.And(x => x.PositionId.HasValue && x.PositionId.Value == PositionId);

            //Search empployee by SSN
            if (!string.IsNullOrWhiteSpace(this.SSN))
                // Get all empployee that SSN equal keyword
                predicate = predicate.And(x => x.base_GuestProfile.Count() > 0 && x.base_GuestProfile.FirstOrDefault().SSN.ToLower().Contains(this.SSN.ToLower()));

            //Search empployee by ID
            if (!string.IsNullOrWhiteSpace(this.ID))
                // Get all empployee that ID equal keyword
                predicate = predicate.And(x => x.base_GuestProfile.Count() > 0 && x.base_GuestProfile.FirstOrDefault().Identification.ToLower().Contains(this.ID.ToLower()));

            //Search empployee by ID
            if (this.PositionId.HasValue && this.PositionId.Value > 0)
                // Get all empployee that Group equal keyword
                predicate = predicate.And(x => x.PositionId.HasValue && x.PositionId.Value == PositionId);

            //Search empployee by Address Type
            if (this.AddressTypeId > 0)
                // Get all empployee that AddressTypeID equal keyword
                predicate = predicate.And(x => x.base_GuestAddress.Any(y => y.AddressTypeId == AddressTypeId));


            // Search customer by address
            if (!string.IsNullOrWhiteSpace(Address))
            {
                Expression<Func<base_Guest, bool>> addressPredicate = PredicateBuilder.False<base_Guest>();

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

                // Get all vendors that AddressLine equal keyword
                addressPredicate = addressPredicate.Or(x => x.base_GuestAddress.Any(y => y.AddressLine1.ToLower().Contains(Address.ToLower())));

                // Get all vendors that AddressTypeID equal keyword
                addressPredicate = addressPredicate.And(x => x.base_GuestAddress.Any(y => y.AddressTypeId.Equals(AddressTypeId)));

                // Get all vendors that Address contain keyword
                predicate = predicate.And(addressPredicate);
            }
            return predicate;
        }

        #endregion
    }
}