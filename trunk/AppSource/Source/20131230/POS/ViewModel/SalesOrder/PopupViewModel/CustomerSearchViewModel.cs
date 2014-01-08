using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Collections.ObjectModel;
using CPC.POS.Model;
using System.ComponentModel;
using System.Windows.Data;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Helper;
using System.Globalization;

namespace CPC.POS.ViewModel
{
    class CustomerSearchViewModel : ViewModelBase
    {
        #region Define
        private ICollectionView _customerCollectionView;
        private base_GuestGroupRepository _guestGroupRepository = new base_GuestGroupRepository();
        public enum ActionView
        {
            Cancel = 0,
            SelectedItem = 1,
            NewCustomer = 2,
            GoToList = 3
        }

        public ActionView CurrentViewAction { get; set; }
        public object ParentViewModel { get; set; }
        #endregion

        #region Constructors
        public CustomerSearchViewModel(ObservableCollection<base_GuestModel> customerSource, object parent)
            : base()
        {
            LoadDynamicData();

            _ownerViewModel = this;
            this.ParentViewModel = parent;
            InitialCommand();
            CustomerCollection = new ObservableCollection<base_GuestModel>();
            foreach (base_GuestModel customerModel in customerSource)
                CustomerCollection.Add(customerModel.CloneItem());

            TotalCustomer = CustomerCollection.Count();
            if (_customerCollectionView == null)
                _customerCollectionView = CollectionViewSource.GetDefaultView(CustomerCollection);
        }


        #endregion

        #region Properties


        #region Keyword
        private string _keyword;
        /// <summary>
        /// Gets or sets the Keyword.
        /// </summary>
        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (_keyword != value)
                {
                    _keyword = value;
                    OnPropertyChanged(() => Keyword);
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

        #region CustomerCollection
        private ObservableCollection<base_GuestModel> _customerCollection = new ObservableCollection<base_GuestModel>();
        /// <summary>
        /// Gets or sets the CustomerCollection.
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

        #region TotalCustomer
        private int _totalCustomer;
        /// <summary>
        /// Gets or sets the TotalCustomer.
        /// </summary>
        public int TotalCustomer
        {
            get { return _totalCustomer; }
            set
            {
                if (_totalCustomer != value)
                {
                    _totalCustomer = value;
                    OnPropertyChanged(() => TotalCustomer);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods

        #region SearchCommand
        /// <summary>
        /// Gets the Search Command.
        /// <summary>

        public RelayCommand<object> SearchCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Search command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSearchCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Search command is executed.
        /// </summary>
        private void OnSearchCommandExecute(object param)
        {
            if (_customerCollectionView == null)
                _customerCollectionView = CollectionViewSource.GetDefaultView(CustomerCollection);

            if (_customerCollectionView != null)
            {
                SearchWithCollectionSource();
            }
        }

        #endregion

        #region NewCustomerCommand

        /// <summary>
        /// Gets the CreatedNewCustomer Command.
        /// <summary>

        public RelayCommand<object> NewCustomerCommand { get; private set; }



        /// <summary>
        /// Method to check whether the CreatedNewCustomer command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnNewCustomerCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CreatedNewCustomer command is executed.
        /// </summary>
        private void OnNewCustomerCommandExecute(object param)
        {
            CurrentViewAction = ActionView.NewCustomer;
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }
        #endregion

        #region GoToCustomerListCommand
        /// <summary>
        /// Gets the GotoCustomerList Command.
        /// <summary>

        public RelayCommand<object> GotoCustomerListCommand { get; private set; }



        /// <summary>
        /// Method to check whether the GotoCustomerList command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnGotoCustomerListCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the GotoCustomerList command is executed.
        /// </summary>
        private void OnGotoCustomerListCommandExecute(object param)
        {
            if ((ParentViewModel as SalesOrderViewModel).ChangeViewExecute(false))
            {
                CurrentViewAction = ActionView.GoToList;
                FindOwnerWindow(_ownerViewModel).DialogResult = true;
            }
        }
        #endregion

        #region SelectedItemCommand
        /// <summary>
        /// Gets the SelectedItem Command.
        /// <summary>

        public RelayCommand<object> SelectedItemCommand { get; private set; }



        /// <summary>
        /// Method to check whether the SelectedItem command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSelectedItemCommandCanExecute(object param)
        {
            if (SelectedCustomer == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the SelectedItem command is executed.
        /// </summary>
        private void OnSelectedItemCommandExecute(object param)
        {
            CurrentViewAction = ActionView.SelectedItem;
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
            CurrentViewAction = ActionView.Cancel;
            FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            SearchCommand = new RelayCommand<object>(OnSearchCommandExecute, OnSearchCommandCanExecute);
            NewCustomerCommand = new RelayCommand<object>(OnNewCustomerCommandExecute, OnNewCustomerCommandCanExecute);
            GotoCustomerListCommand = new RelayCommand<object>(OnGotoCustomerListCommandExecute, OnGotoCustomerListCommandCanExecute);
            SelectedItemCommand = new RelayCommand<object>(OnSelectedItemCommandExecute, OnSelectedItemCommandCanExecute);
            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void LoadDynamicData()
        {
            // Load guest group collection
            GuestGroupCollection = new ObservableCollection<base_GuestGroupModel>(_guestGroupRepository.GetAll().
                Select(x => new base_GuestGroupModel(x) { GuestGroupResource = x.Resource.ToString() }));
        }

        private void SearchWithCollectionSource()
        {

            _customerCollectionView.Filter = obj =>
            {
                bool result = false;
                base_GuestModel guestModel = obj as base_GuestModel;
                if (guestModel == null)
                {
                    result = false;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Keyword))
                    {
                        result = true;
                    }
                    else
                        try
                        {
                                //Search Customer By status
                                IEnumerable<bool> statusList = Common.StatusBasic.Where(x => x.Text.ToLower().Contains(Keyword.ToLower())).Select(x => Convert.ToInt32(x.ObjValue).Is(StatusBasic.Active));
                                if (statusList.Any())
                                    result |= statusList.Contains(guestModel.IsActived);

                                //Search Customer By Guest No
                                result |= guestModel.GuestNo.Contains(Keyword.ToLower());

                                //Search Customer By First Name
                                result |= !string.IsNullOrWhiteSpace(guestModel.FirstName) && guestModel.FirstName.ToLower().Contains(Keyword.ToLower());

                                //Search Customer By Last Name
                                result |= !string.IsNullOrWhiteSpace(guestModel.LastName) && guestModel.LastName.ToLower().Contains(Keyword.ToLower());

                                //Serach Customer By Company
                                result |= !string.IsNullOrWhiteSpace(guestModel.Company) && guestModel.Company.ToLower().Contains(Keyword.ToLower());

                                //Search Customer by Phone
                                result |= (!string.IsNullOrWhiteSpace(guestModel.Phone1) && guestModel.Phone1.Contains(Keyword.ToLower()))
                                         || (!string.IsNullOrWhiteSpace(guestModel.Phone2) && guestModel.Phone2.Contains(Keyword.ToLower()));

                                //Search Customer by CellPhone
                                if (!string.IsNullOrWhiteSpace(guestModel.CellPhone))
                                    result |= guestModel.CellPhone.ToLower().Contains(Keyword.ToLower());

                                //Search Customer by Email
                                if (!string.IsNullOrWhiteSpace(guestModel.Email))
                                    result |= guestModel.Email.ToLower().Contains(Keyword.ToLower());

                                //Search Customer by Website
                                if (!string.IsNullOrWhiteSpace(guestModel.Website))
                                    result |= guestModel.Website.ToLower().Contains(Keyword.ToLower());

                                //Search Customer by City
                                result |= guestModel.base_Guest.base_GuestAddress.Any(x => x.IsDefault && x.City.ToLower().Contains(Keyword.ToLower()));

                                //Search Customer by Address
                                result |= guestModel.base_Guest.base_GuestAddress.Any(x => x.IsDefault && x.AddressLine1.ToLower().Contains(Keyword.ToLower()));

                                // Search Customer by  Country
                                IEnumerable<int> countryList = Common.Countries.Where(x => x.Text.ToLower().Contains(Keyword.ToLower())).Select(x => Convert.ToInt32(x.ObjValue));
                                if (countryList.Any())
                                    result |= guestModel.base_Guest.base_GuestAddress.Any(x => x.IsDefault && countryList.Contains(x.CountryId));

                                decimal decimalValue = 0;

                                if (decimal.TryParse(Keyword, NumberStyles.Number, Define.ConverterCulture.NumberFormat, out decimalValue))
                                {
                                    //Total Purchase
                                    result |= guestModel.PurchaseDuringTrackingPeriod.Equals(decimalValue);


                                    //TotalReward

                                    //TotalReedem
                                    result |= guestModel.TotalRewardRedeemed.Equals(decimalValue);
                                }


                                //Search Customer by Group
                                // Get all guest group that contain keyword
                                IEnumerable<base_GuestGroupModel> guestGroups = GuestGroupCollection.Where(x => x.Name != string.Empty && x.Name.ToLower().Contains(Keyword.ToLower()));
                                IEnumerable<string> guestGroupResourceList = guestGroups.Select(x => x.Resource.ToString());

                                // Get all product that contain in guest group resource list
                                if (guestGroupResourceList.Any())
                                {
                                    result |= guestGroupResourceList.Contains(guestModel.GroupResource);
                                }

                                //Search by Custom
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom1) && guestModel.Custom1.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom2) && guestModel.Custom2.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom3) && guestModel.Custom3.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom4) && guestModel.Custom4.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom5) && guestModel.Custom5.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom6) && guestModel.Custom6.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom7) && guestModel.Custom7.Equals(Keyword.ToLower());
                                result |= !string.IsNullOrWhiteSpace(guestModel.Custom8) && guestModel.Custom8.Equals(Keyword.ToLower());

                        }
                        catch (Exception ex)
                        {
                        }
                }
                return result;
            };

            TotalCustomer = _customerCollectionView.OfType<object>().Count();

        }
        #endregion
    }


}

