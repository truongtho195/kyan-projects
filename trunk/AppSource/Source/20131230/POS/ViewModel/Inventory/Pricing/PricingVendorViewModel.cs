using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class PricingVendorViewModel : ViewModelBase
    {
        #region Defines

        private base_ProductRepository _productRepository = new base_ProductRepository();
        private base_GuestRepository _vendorRepository = new base_GuestRepository();

        private bool _isCheckAllFlag = false;
        private bool _isCheckItemFlag = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DepartmentList
        /// </summary>
        public List<ComboItem> CategoryList { get; set; }

        private int _totalVendor;
        /// <summary>
        /// Gets or sets the TotalVendor.
        /// </summary>
        public int TotalVendor
        {
            get { return _totalVendor; }
            set
            {
                if (_totalVendor != value)
                {
                    _totalVendor = value;
                    OnPropertyChanged(() => TotalVendor);
                }
            }
        }

        #region IsCheckedAll
        #region IsCheckedAll
        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets the IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get { return _isCheckedAll; }
            set
            {
                if (_isCheckedAll != value)
                {
                    this._isCheckAllFlag = true;
                    _isCheckedAll = value;
                    if (!this._isCheckItemFlag && value.HasValue)
                    {
                        foreach (base_GuestModel item in this.VendorCollection)
                            item.IsChecked = value.Value;
                    }
                    OnPropertyChanged(() => IsCheckedAll);
                    this._isCheckAllFlag = false;
                }
            }
        }
        #endregion
        #endregion

        public int CurrentPageIndexLeft { get; set; }

        #region VendorCollection
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
        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PricingVendorViewModel()
        {
            this.InitialCommand();
            this.LoadVendor();
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
            return this.VendorCollection != null && this.VendorCollection.Count(x => x.IsChecked) > 0;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            this.CategoryList = new List<ComboItem>();
            foreach (var item in this.VendorCollection.Where(x => x.IsChecked))
                this.CategoryList.Add(new ComboItem { LongValue = item.Id });
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
            this.OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            this.CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void LoadVendor()
        {

            IEnumerable<base_GuestModel> vendors = _vendorRepository.GetAll(x => x.Mark.Equals("V")).OrderBy(x => x.FirstName).Select(x => new base_GuestModel(x, false));
            foreach (base_GuestModel item in vendors)
            {
                item.AddressModel = new base_GuestAddressModel(item.base_Guest.base_GuestAddress.SingleOrDefault(x => x.IsDefault));
                item.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);
                this.VendorCollection.Add(item);
            }
        }

        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":
                    if (!this._isCheckAllFlag)
                    {
                        this._isCheckItemFlag = true;
                        if (this.VendorCollection.Count(x => x.IsChecked) == this.VendorCollection.Count)
                            this.IsCheckedAll = true;
                        else
                            this.IsCheckedAll = false;
                        this._isCheckItemFlag = false;
                        //To change IsCheckedAll property.
                        this.OnPropertyChanged(() => IsCheckedAll);
                    }
                    break;
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Process when left item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":

                    break;
            }
        }

        /// <summary>
        /// Process when right item checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightItemChecked(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsChecked":

                    break;
            }
        }

        #endregion
    }
}