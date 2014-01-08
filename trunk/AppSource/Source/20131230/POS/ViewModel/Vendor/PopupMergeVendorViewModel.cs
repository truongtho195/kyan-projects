using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PopupMergeVendorViewModel : ViewModelBase
    {
        #region Defines

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the VendorList
        /// </summary>
        public ObservableCollection<base_GuestModel> VendorList { get; set; }

        private base_GuestModel _sourceVendor;
        /// <summary>
        /// Gets or sets the SourceVendor.
        /// </summary>
        public base_GuestModel SourceVendor
        {
            get { return _sourceVendor; }
            set
            {
                if (_sourceVendor != value)
                {
                    _sourceVendor = value;
                    OnPropertyChanged(() => SourceVendor);
                    OnPropertyChanged(() => SourceVendorDetail);
                }
            }
        }

        private base_GuestModel _targetVendor;
        /// <summary>
        /// Gets or sets the TargetVendor.
        /// </summary>
        public base_GuestModel TargetVendor
        {
            get { return _targetVendor; }
            set
            {
                if (_targetVendor != value)
                {
                    _targetVendor = value;
                    OnPropertyChanged(() => TargetVendor);
                    OnPropertyChanged(() => TargetVendorDetail);
                }
            }
        }

        /// <summary>
        /// Gets the SourceVendorDetail
        /// </summary>
        public string SourceVendorDetail
        {
            get { return GetVendorDetail(SourceVendor); }
        }

        /// <summary>
        /// Gets the TargetVendorDetail
        /// </summary>
        public string TargetVendorDetail
        {
            get { return GetVendorDetail(TargetVendor); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupMergeVendorViewModel()
            : base()
        {
            InitialCommand();
        }

        public PopupMergeVendorViewModel(base_GuestModel vendorModel, ObservableCollection<base_GuestModel> vendorList)
            : this()
        {
            // Clone vendor list
            VendorList = new ObservableCollection<base_GuestModel>(vendorList.CloneList());

            // Get source vendor to merge
            SourceVendor = VendorList.SingleOrDefault(x => x.Id.Equals(vendorModel.Id));

            // Hidden selected vendor in comboBox
            SourceVendor.IsChecked = true;
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
            if (SourceVendor == null || TargetVendor == null)
                return false;

            return true;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            MessageBoxResult msgResult = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to merge these vendor?", "POS", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult.Equals(MessageBoxResult.Yes))
            {
                Window window = FindOwnerWindow(this);
                window.DialogResult = true;
            }
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

        /// <summary>
        /// Get vendor detail
        /// </summary>
        /// <param name="vendorModel"></param>
        /// <returns></returns>
        private string GetVendorDetail(base_GuestModel vendorModel)
        {
            if (vendorModel == null)
                return string.Empty;
            return string.Format("{0}\n\n{1}\n{2} {3}\n\n{4} {5}",
                vendorModel.Company, vendorModel.AddressModel.Text, "Phone:", vendorModel.Phone1, "Fax:", vendorModel.Fax);
        }

        #endregion
    }
}