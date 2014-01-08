using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Windows;
using System.ComponentModel;
using KeyGenerator.Model;

namespace KeyGenerator.ViewModel
{

    class KeyGeneratorViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define
        public Window CurrentView { get; set; }
        #endregion

        #region Constructors
        public KeyGeneratorViewModel()
        {
            InitialCommand();
        }

        public KeyGeneratorViewModel(CustomerDetailModel customerDetailModel, CustomerModel customerModel)
            : this()
        {
            this.CustomerDetailModel = customerDetailModel.Clone();
            this.LicenseName = customerModel.Company;
            this.TotalStore = Convert.ToInt32(customerModel.TotalStore);
            this.ApplicationId = CustomerDetailModel.ApplicationId;
            this.StoreCode = customerDetailModel.StoreCode.Value;
            this.PosId = customerDetailModel.POSId;
            this.Period = customerDetailModel.Period.HasValue ? customerDetailModel.Period.Value : 0;
            if (customerDetailModel.ExpireDate.HasValue && customerDetailModel.ExpireDate>0)
                this.ExpiredDate = DateTime.FromOADate(customerDetailModel.ExpireDate.Value);
            else
                this.ExpiredDate = null;
            
            this.LicenseKey = customerDetailModel.LicenceCode;

            if (!string.IsNullOrWhiteSpace(this.LicenseKey))//Get Project Id From Key
            {
                string licenseDecrypt = ProductKeyGenerator.DescrytProductKey(this.LicenseKey,this.ApplicationId);
                var licenseArray = licenseDecrypt.Split('|');

                //Get StoreCode| POSID | ProjectId | ExpiredDates
                int projectId = Convert.ToInt32(licenseArray[3]);
                int intExpiredDate = Convert.ToInt32(licenseArray[4]);
                this.ProjectId = projectId;
            }
            IsDirty = false;
            IsInfoDirty = false;
        }
        #endregion

        #region Properties

        #region TitleView
        /// <summary>
        /// Gets the Title.
        /// </summary>
        public string TitleView
        {
            get { return "Key Generator"; }
        }
        #endregion

        #region IsDirty
        private bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty.
        /// <para>License Key Dirty</para>
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


        #region IsInfoDirty
        private bool _isInfoDirty;
        /// <summary>
        /// Gets or sets the IsInfoDirty.
        /// </summary>
        public bool IsInfoDirty
        {
            get { return _isInfoDirty; }
            set
            {
                if (_isInfoDirty != value)
                {
                    _isInfoDirty = value;
                    OnPropertyChanged(() => IsInfoDirty);
                }
            }
        }
        #endregion


        #region LicenseName
        private string _licenseName;
        /// <summary>
        /// Gets or sets the LicenseName.
        /// </summary>
        public string LicenseName
        {
            get { return _licenseName; }
            set
            {
                if (_licenseName != value)
                {
                    _licenseName = value;
                    OnPropertyChanged(() => LicenseName);
                }
            }
        }
        #endregion

        #region ApplicationId
        private string _applicationId;
        /// <summary>
        /// Gets or sets the ApplicationId.
        /// </summary>
        public string ApplicationId
        {
            get { return _applicationId; }
            set
            {
                if (_applicationId != value)
                {
                    _applicationId = value;
                    IsInfoDirty = true;
                    OnPropertyChanged(() => ApplicationId);
                }
            }
        }
        #endregion

        #region TotalStore
        private int _totalStore;
        /// <summary>
        /// Gets or sets the TotalStore.
        /// </summary>
        public int TotalStore
        {
            get { return _totalStore; }
            set
            {
                if (_totalStore != value)
                {
                    _totalStore = value;
                    OnPropertyChanged(() => TotalStore);
                }
            }
        }
        #endregion

        #region StoreCode
        private int _storeCode;
        /// <summary>
        /// Gets or sets the StoreCode.
        /// </summary>
        public int StoreCode
        {
            get { return _storeCode; }
            set
            {
                if (_storeCode != value)
                {
                    _storeCode = value;
                    OnPropertyChanged(() => StoreCode);
                }
            }
        }
        #endregion

        #region PosId
        private string _posId;
        /// <summary>
        /// Gets or sets the PosId.
        /// </summary>
        public string PosId
        {
            get { return _posId; }
            set
            {
                if (_posId != value)
                {
                    _posId = value;
                    OnPropertyChanged(() => PosId);
                }
            }
        }
        #endregion

        #region Period
        private int _period;
        /// <summary>
        /// Gets or sets the Period.
        /// </summary>
        public int Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    IsInfoDirty = true;
                    OnPropertyChanged(() => Period);
                    PeriodChanged();
                    
                }
            }
        }
        #endregion

        #region ExpiredDate
        private DateTime? _expiredDate;
        /// <summary>
        /// Gets or sets the ExpireDate.
        /// </summary>
        public DateTime? ExpiredDate
        {
            get { return _expiredDate; }
            set
            {
                if (_expiredDate != value)
                {
                    _expiredDate = value;
                    IsInfoDirty = true;
                    OnPropertyChanged(() => ExpiredDate);
                    ExpiredDateChanged();
                }
            }
        }
        #endregion

        #region ProjectId
        private int _projectId;
        /// <summary>
        /// Gets or sets the ProjectId.
        /// </summary>
        public int ProjectId
        {
            get { return _projectId; }
            set
            {
                if (_projectId != value)
                {
                    _projectId = value;
                    IsInfoDirty = true;
                    OnPropertyChanged(() => ProjectId);
                }
            }
        }
        #endregion

        #region LicenseKey
        private string _licenseKey;
        /// <summary>
        /// Gets or sets the LicenseKey.
        /// </summary>
        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                if (_licenseKey != value)
                {
                    _licenseKey = value;
                    IsDirty = true;
                    OnPropertyChanged(() => LicenseKey);
                }
            }
        }
        #endregion

        #region CustomerDetailModel
        private CustomerDetailModel _customerDetailModel;
        /// <summary>
        /// Gets or sets the CustomerDetailModel.
        /// </summary>
        public CustomerDetailModel CustomerDetailModel
        {
            get { return _customerDetailModel; }
            set
            {
                if (_customerDetailModel != value)
                {
                    _customerDetailModel = value;
                    OnPropertyChanged(() => CustomerDetailModel);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods
        #region GeneralKeyCommand
        /// <summary>
        /// Gets the GeneralKey Command.
        /// <summary>

        public RelayCommand<object> GeneralKeyCommand { get; private set; }

        /// <summary>
        /// Method to check whether the GeneralKey command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnGeneralKeyCommandCanExecute(object param)
        {
            if (string.IsNullOrWhiteSpace(this.LicenseName) || string.IsNullOrWhiteSpace(ApplicationId) || string.IsNullOrWhiteSpace(PosId))
                return false;
            return IsInfoDirty;
        }


        /// <summary>
        /// Method to invoke when the GeneralKey command is executed.
        /// </summary>
        private void OnGeneralKeyCommandExecute(object param)
        {
            int intExpiredDate = 0;
            if (ExpiredDate.HasValue)
                intExpiredDate = Convert.ToInt32(ExpiredDate.Value.ToOADate());
            ProductKeyModel productKeyModel = new ProductKeyModel(this.ApplicationId, this.TotalStore, this.StoreCode, this.PosId, this.ProjectId, intExpiredDate);
            this.LicenseKey = productKeyModel.ProductKey;
            IsInfoDirty = false;
        }
        #endregion

        #region CopyCommand

        /// <summary>
        /// Gets the Copy Command.
        /// <summary>

        public RelayCommand<object> CopyCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Copy command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCopyCommandCanExecute(object param)
        {
            return !string.IsNullOrWhiteSpace(this.LicenseKey);
        }


        /// <summary>
        /// Method to invoke when the Copy command is executed.
        /// </summary>
        private void OnCopyCommandExecute(object param)
        {
            Clipboard.SetText(LicenseKey);
        }
        #endregion

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
            return !string.IsNullOrWhiteSpace(LicenseKey) && IsDirty;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            this.CustomerDetailModel.ApplicationId = this.ApplicationId;
            this.CustomerDetailModel.Period = this.Period;
            if (ExpiredDate.HasValue)
                this.CustomerDetailModel.ExpireDate =Convert.ToInt32(ExpiredDate.Value.ToOADate());
            else
                this.CustomerDetailModel.ExpireDate = 0;

            this.CustomerDetailModel.LicenceCode = LicenseKey;
            //this.CustomerDetailModel.RequestBy=??
            this.CustomerDetailModel.GenDate = DateTime.Now;

            CurrentView.DialogResult = true;
        }
        #endregion


        #region CancelComand
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
            CurrentView.DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            GeneralKeyCommand = new RelayCommand<object>(OnGeneralKeyCommandExecute, OnGeneralKeyCommandCanExecute);

            CopyCommand = new RelayCommand<object>(OnCopyCommandExecute, OnCopyCommandCanExecute);

            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);

            CancelCommand = new RelayCommand<object>(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }


        /// <summary>
        /// Period Change =>Set ExpiredDate
        /// </summary>
        private void PeriodChanged()
        {
            if (Period == 0)
                this._expiredDate = null;
            else
                this._expiredDate = DateTime.Today.AddDays(Period);

            OnPropertyChanged(() => ExpiredDate);
        }

        /// <summary>
        /// ExpiredDate change => set Period Day
        /// </summary>
        private void ExpiredDateChanged()
        {
            if (this.ExpiredDate.HasValue)
            {
                TimeSpan delta = ExpiredDate.Value.Subtract(DateTime.Today);
                _period = delta.Days;
            }
            else
            {
                _period = 0;
            }
            OnPropertyChanged(() => Period);
        }

        #endregion

        #region IDataErrorInfo
        public string Error
        {
            get
            {
                List<string> errors = new List<string>();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string msg = this[prop.Name];
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Add(msg);
                    }
                }
                return string.Join(Environment.NewLine, errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;
                switch (columnName)
                {
                    case "ExpiredDate":
                        if (ExpiredDate.HasValue && ExpiredDate.Value.Date < DateTime.Today)
                            message = "Expired Date is greater than current date";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
                return null;

            }
        }
        #endregion
    }


}
