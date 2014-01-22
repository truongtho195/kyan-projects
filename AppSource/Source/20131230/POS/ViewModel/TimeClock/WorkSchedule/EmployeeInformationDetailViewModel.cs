using System.Windows;
using System.Linq;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;
using System.Collections.ObjectModel;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    class EmployeeInformationDetailViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        private base_GuestModel _selectedEmployee;
        /// <summary>
        /// Gets or sets the SelectedEmployee.
        /// </summary>
        public base_GuestModel SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged(() => SelectedEmployee);
                }
            }
        }

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

        #region Constructors

        /// <summary>
        /// Default contructor
        /// </summary>
        public EmployeeInformationDetailViewModel()
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;

            InitialCommand();

            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = "Home" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = "Business" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = "Billing" });
            this.AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = "Shipping" });
            this.JobTitleCollection = new ObservableCollection<ComboItem>(Common.JobTitles);
        }

        public EmployeeInformationDetailViewModel(base_GuestModel employeeModel)
            : this()
        {
            SelectedEmployee = employeeModel;

            SelectedEmployee.AddressCollection = new ObservableCollection<base_GuestAddressModel>(SelectedEmployee.base_Guest.base_GuestAddress.Select(x => new base_GuestAddressModel(x)));

            SelectedEmployee.AddressControlCollection = new AddressControlCollection();
            foreach (base_GuestAddressModel guestAddressModel in SelectedEmployee.AddressCollection)
            {
                AddressControlModel addressControlModel = guestAddressModel.ToAddressControlModel();
                addressControlModel.IsDirty = false;
                employeeModel.AddressControlCollection.Add(addressControlModel);
            }
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
            return true;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
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
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        #endregion
    }
}