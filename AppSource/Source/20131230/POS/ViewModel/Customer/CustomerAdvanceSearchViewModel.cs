using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Collections.ObjectModel;
using CPC.Helper;
using CPC.POS.Repository;
using System.Linq.Expressions;
using CPC.POS.Database;

namespace CPC.POS.ViewModel
{
    class CustomerAdvanceSearchViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public CustomerAdvanceSearchViewModel()
        {
            InitialCommand();
            LoadStaticData();

            AddressTypeId = Convert.ToInt32(AddressTypeCollection.FirstOrDefault().ObjValue);
        }
        #endregion

        #region Properties

        #region IsDirty
        private bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        #endregion



        #region FirstName
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
                    _firstName = value;
                    IsDirty = true;
                    OnPropertyChanged(() => FirstName);
                }
            }
        }
        #endregion

        #region LastName
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
                    _lastName = value;
                    IsDirty = true;
                    OnPropertyChanged(() => LastName);
                }
            }
        }
        #endregion

        #region Phone
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
                    _phone = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Phone);
                }
            }
        }
        #endregion

        #region Email
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
                    _email = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Email);
                }
            }
        }
        #endregion

        #region Company
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
                    _company = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Company);
                }
            }
        }
        #endregion

        #region Address
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
                    _address = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Address);
                }
            }
        }
        #endregion

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
                    IsDirty = true;
                    OnPropertyChanged(() => AddressTypeId);
                }
            }
        }
        #endregion

        #region DOB
        private DateTime? _dob;
        /// <summary>
        /// Gets or sets the DOB.
        /// </summary>
        public DateTime? DOB
        {
            get { return _dob; }
            set
            {
                if (_dob != value)
                {
                    _dob = value;
                    IsDirty = true;
                    OnPropertyChanged(() => DOB);
                }
            }
        }
        #endregion

        #region TaxLocation
        private int _taxLocation;
        /// <summary>
        /// Gets or sets the TaxLocation.
        /// </summary>
        public int TaxLocation
        {
            get { return _taxLocation; }
            set
            {
                if (_taxLocation != value)
                {
                    _taxLocation = value;
                    IsDirty = true;
                    OnPropertyChanged(() => TaxLocation);
                }
            }
        }
        #endregion

        #region ResellerTaxNo
        private string _resellerTaxNo;
        /// <summary>
        /// Gets or sets the ResellerTaxNo.
        /// </summary>
        public string ResellerTaxNo
        {
            get { return _resellerTaxNo; }
            set
            {
                if (_resellerTaxNo != value)
                {
                    _resellerTaxNo = value;
                    IsDirty = true;
                    OnPropertyChanged(() => ResellerTaxNo);
                }
            }
        }
        #endregion

        #region TaxExemption
        private string _taxExemption;
        /// <summary>
        /// Gets or sets the TaxExemption.
        /// </summary>
        public string TaxExemption
        {
            get { return _taxExemption; }
            set
            {
                if (_taxExemption != value)
                {
                    _taxExemption = value;
                    IsDirty = true;
                    OnPropertyChanged(() => TaxExemption);
                }
            }
        }
        #endregion

        #region Status
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
                    _status = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Status);
                }
            }
        }
        #endregion

        #region GroupResource
        private string _groupResource;
        /// <summary>
        /// Gets or sets the Group.
        /// </summary>
        public string GroupResource
        {
            get { return _groupResource; }
            set
            {
                if (_groupResource != value)
                {
                    _groupResource = value;
                    IsDirty = true;
                    OnPropertyChanged(() => GroupResource);
                }
            }
        }
        #endregion


        #region AdvanceSearchPredicate
        /// <summary>
        /// Gets or sets the AdvanceSearchPredicate.
        /// </summary>
        public Expression<Func<base_Guest, bool>> AdvanceSearchPredicate
        {
            get;
            set;
        }
        #endregion


        //Extention

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

        #region GuestGroupCollection
        private ObservableCollection<ComboItem> _guestGroupCollection;
        /// <summary>
        /// Gets or sets the GuestGroupCollection.
        /// </summary>
        public ObservableCollection<ComboItem> GuestGroupCollection
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

        #region SaleTaxCollection
        private ObservableCollection<ComboItem> _saleTaxCollection;
        /// <summary>
        /// Gets or sets the SaleTaxCollection.
        /// </summary>
        public ObservableCollection<ComboItem> SaleTaxCollection
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

        #endregion

        #region Commands Methods

        #region OkCommand

        /// <summary>
        /// Gets the Ok Command.
        /// <summary>

        public RelayCommand<object> OkCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute(object param)
        {
            return IsDirty;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            AdvanceSearchPredicate = CreateAdvanceSearchPredicate();
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>

        public RelayCommand<object> CancelCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute(object param)
        {
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        ///Load what data for search 
        /// </summary>
        private void LoadStaticData()
        {

            //Load Collection address Type to combobox
            AddressTypeCollection = new ObservableCollection<ComboItem>();
            AddressTypeCollection.Add(new ComboItem { ObjValue = 0, Text = Language.GetMsg("SO_TextBlock_Home") });
            AddressTypeCollection.Add(new ComboItem { ObjValue = 1, Text = Language.GetMsg("SO_TextBlock_Business") });
            AddressTypeCollection.Add(new ComboItem { ObjValue = 2, Text = Language.GetMsg("SO_TextBlock_Billing") });
            AddressTypeCollection.Add(new ComboItem { ObjValue = 3, Text = Language.GetMsg("SO_TextBlock_Shipping") });

            //Load Guest Group
            base_GuestGroupRepository guestGroupRepository = new base_GuestGroupRepository();
            GuestGroupCollection = new ObservableCollection<ComboItem>(guestGroupRepository.GetAll().
                    Select(x => new ComboItem{ ObjValue = x.Resource.ToString(), Text= x.Name}));

            GuestGroupCollection.Insert(0, new ComboItem { ObjValue = string.Empty, Text = string.Empty });

            base_SaleTaxLocationRepository saleTaxLocationRespository = new base_SaleTaxLocationRepository();

            SaleTaxCollection = new ObservableCollection<ComboItem>(saleTaxLocationRespository.GetAll(x => x.ParentId == 0).Select(x => new ComboItem {
                ObjValue = x.Id,
                Text = x.Name
            }));

            //None Tax
            SaleTaxCollection.Insert(0, new ComboItem()
            {
                ObjValue = 0,
                Text = ""
            });

        }

        /// <summary>
        /// Create Predicate for advance Search
        /// </summary>
        /// <returns></returns>
        private Expression<Func<base_Guest, bool>> CreateAdvanceSearchPredicate()
        {
            Expression<Func<base_Guest, bool>> predicate = PredicateBuilder.True<base_Guest>();

            //Seach Customer By FirstName
            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                // Get all vendors that FirstName contain keyword
                predicate = predicate.And(x => x.FirstName.ToLower().Contains(FirstName.ToLower()));
            }
            //Seach Customer By LastName
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                // Get all vendors that FirstName contain keyword
                predicate = predicate.And(x => x.LastName.ToLower().Contains(LastName.ToLower()));
            }

            //Seach Customer By Phone
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                // Get all vendors that Phone contain keyword
                predicate = predicate.And(x => x.Phone1.ToLower().Contains(Phone.ToLower()));
            }

            //Seach Customer By Email
            if (!string.IsNullOrWhiteSpace(Email))
            {
                // Get all vendors that Email contain keyword
                predicate = predicate.And(x => x.Email.ToLower().Contains(Email.ToLower()));
            }

            //Search Customer by Company
            if (!string.IsNullOrWhiteSpace(Company))
            {
                // Get all vendors that Company contain keyword
                predicate = predicate.And(x => x.Company.ToLower().Contains(Company.ToLower()));
            }

            //Search customer by Address Type
            if (AddressTypeId > 0)
            {
                // Get all vendors that AddressTypeID equal keyword
                predicate = predicate.And(x => x.base_GuestAddress.Any(y => y.AddressTypeId == AddressTypeId));
            }

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

            //Search customer by status
            if (Status > 0)
            {
                bool actived = Status.Is(StatusBasic.Active);
                // Get all vendors that Status equal keyword
                predicate = predicate.And(x => x.IsActived.Equals(actived));
            }

            //Search customer by group
            if (!string.IsNullOrWhiteSpace(GroupResource))
            {
                // Get all vendors that Group equal keyword
                predicate = predicate.And(x => x.GroupResource.Equals(GroupResource));
            }

            //Search customer by DOB
            if (DOB.HasValue)
            {
                // Get all vendors that Group equal keyword
                predicate = predicate.And(x => x.base_GuestProfile.Any(y=>y.DOB!=null && y.DOB.Value.Equals(DOB.Value)));
            }
            
            //Search Customer by TaxLocation
            if (TaxLocation > 0)
            {
                predicate = predicate.And(x => x.base_GuestAdditional.Any(y => y.SaleTaxLocation.Equals(TaxLocation)));
            }

            //Search Customer by Reseller Tax number
            if (!string.IsNullOrWhiteSpace(ResellerTaxNo))
            {
                predicate = predicate.And(x => x.base_GuestAdditional.Any(y => y.ResellerTaxId.ToLower().Contains(ResellerTaxNo.ToLower())));
            }

            //Search customer by Tax Exemtion
            if (!string.IsNullOrWhiteSpace(TaxExemption))
            {
                predicate = predicate.And(x => x.base_GuestAdditional.Any(y => y.TaxExemptionNo.ToLower().Contains(TaxExemption.ToLower())));
            }


            return predicate;
        }
        #endregion

        #region Public Methods
        #endregion
    }


}
