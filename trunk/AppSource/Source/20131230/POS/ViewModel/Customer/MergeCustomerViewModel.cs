using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Helper;

namespace CPC.POS.ViewModel
{
    class MergeCustomerViewModel : ViewModelBase
    {
        #region Define

        #endregion

        #region Constructors
        public MergeCustomerViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        public MergeCustomerViewModel(base_GuestModel guestModel, ObservableCollection<base_GuestModel> customerList)
            : this()
        {
            CustomerCollection = new ObservableCollection<base_GuestModel>(customerList.CloneList());
            //foreach (base_GuestModel customerModel in customerList)
            //{
            //    CustomerCollection.Add(customerModel.CloneItem());
            //}

            CustomerSource = CustomerCollection.SingleOrDefault(x => x.Id.Equals(guestModel.Id));
            //Hidden item in Combobox
            CustomerSource.IsChecked = true;
        }
        #endregion

        #region Properties

        #region CustomerCollection
        private ObservableCollection<base_GuestModel> _customerCollection;
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

        #region CustomerSource
        private base_GuestModel _customerSource;
        /// <summary>
        /// Gets or sets the CustomerSource.
        /// </summary>
        public base_GuestModel CustomerSource
        {
            get { return _customerSource; }
            set
            {
                if (_customerSource != value)
                {
                    _customerSource = value;
                    OnPropertyChanged(() => CustomerSource);
                    OnPropertyChanged(() => CustomerSourceDetail);

                }
            }
        }
        #endregion

        #region CustomerTarget
        private base_GuestModel _customerTarget;
        /// <summary>
        /// Gets or sets the CustomerTarget.
        /// </summary>
        public base_GuestModel CustomerTarget
        {
            get { return _customerTarget; }
            set
            {
                if (_customerTarget != value)
                {
                    _customerTarget = value;
                    OnPropertyChanged(() => CustomerTarget);
                    OnPropertyChanged(() => CustomerTargetDetail);
                }
            }
        }
        #endregion

        #region CustomerSourceDetail
        /// <summary>
        /// Gets the CustomerSourceDetail.
        /// </summary>
        public string CustomerSourceDetail
        {
            get { return CustomerDetail(CustomerSource); }

        }
        #endregion

        #region CustomerTargetDetail
        /// <summary>
        /// Gets the CustomerTargetDetail.
        /// </summary>
        public string CustomerTargetDetail
        {
            get { return CustomerDetail(CustomerTarget); }

        }
        #endregion

        #endregion

        #region Commands Methods

        #region Ok Command
        /// <summary>
        /// Gets the Ok Command.
        /// <summary>
        public RelayCommand OkCommand { get; private set; }




        /// <summary>
        /// Method to check whether the Ok command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {

            if (CustomerSource == null || CustomerTarget == null)
                return false;
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Language.GetMsg("CUS_MSG_QMergeCustomer"), Language.POS, MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.Yes);
            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                Window window = FindOwnerWindow(this);
                window.DialogResult = true;
            }
        }
        #endregion

        #region Cancel Command
        /// <summary>
        /// Gets the Cancel Command.
        /// <summary>
        public RelayCommand CancelCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Cancel command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the Cancel command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            this.FindOwnerWindow(_ownerViewModel).DialogResult = false;
        }
        #endregion
        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }
     
        private string CustomerDetail(base_GuestModel guestModel)
        {
            if (guestModel == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(guestModel.Company))
                sb.AppendFormat(Language.GetMsg("C170") +": {0}\n\n", guestModel.Company);//Company

            sb.AppendFormat(Language.GetMsg("C184") + " : {0}\n\n", guestModel.Email);//Email
            sb.AppendFormat(Language.GetMsg("AddressControl_Text_Address")+ " : {0}\n\n", guestModel.AddressModel.Text);//Address
            sb.AppendFormat(Language.GetMsg("Text_Phone")+ " : {0}\n", guestModel.Phone1);//Phone

            if (!string.IsNullOrWhiteSpace(guestModel.Fax))
            {
                sb.AppendLine();
                sb.AppendFormat(Language.GetMsg("C183")+ " : {0}\n", guestModel.Fax);
            }
            return sb.ToString();
        }
        #endregion
    }
}