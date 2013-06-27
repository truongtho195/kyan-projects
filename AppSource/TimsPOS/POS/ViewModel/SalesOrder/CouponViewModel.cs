using System;
using System.Collections.Generic;
using System.ComponentModel;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class CouponViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Define

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CouponViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
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

        #region Amount
        private decimal _amount;
        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    IsDirty = true;
                    OnPropertyChanged(() => Amount);
                }
            }
        }
        #endregion

        #region CouponCode
        private string _couponCode;
        /// <summary>
        /// Gets or sets the CouponCode.
        /// </summary>
        public string CouponCode
        {
            get { return _couponCode; }
            set
            {
                if (_couponCode != value)
                {
                    _couponCode = value;
                    IsDirty = true;
                    OnPropertyChanged(() => CouponCode);
                }
            }
        }
        #endregion

        #region SaleOrderDetailModel
        private base_SaleOrderDetailModel _saleOrderDetailModel;
        /// <summary>
        /// Gets or sets the SaleOrderDetailModel.
        /// </summary>
        public base_SaleOrderDetailModel SaleOrderDetailModel
        {
            get { return _saleOrderDetailModel; }
            set
            {
                if (_saleOrderDetailModel != value)
                {
                    _saleOrderDetailModel = value;
                    OnPropertyChanged(() => SaleOrderDetailModel);
                    SaleOrderDetailModelChanged();
                }
            }
        }

        private void SaleOrderDetailModelChanged()
        {
            if (SaleOrderDetailModel != null)
            {
                Amount = SaleOrderDetailModel.SalePrice > 0 ? SaleOrderDetailModel.SalePrice : 0;
                this.CouponCode = !string.IsNullOrWhiteSpace(SaleOrderDetailModel.SerialTracking) ? SaleOrderDetailModel.SerialTracking : string.Empty;

                this.IsDirty = false;
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
            return IsValid;
        }

        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
            if (IsDirty)
            {
                SaleOrderDetailModel.SalePrice = this.Amount;
                SaleOrderDetailModel.SerialTracking = this.CouponCode;
            }
            FindOwnerWindow(_ownerViewModel).DialogResult = true;
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            OkCommand = new RelayCommand<object>(OnOkCommandExecute, OnOkCommandCanExecute);
        }

        #endregion

        #region Public Methods

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets or sets the IsError.
        /// </summary>
        public bool IsError
        {
            get { return !string.IsNullOrWhiteSpace(Error); }

        }

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
                    case "Amount":
                        if (Amount == 0)
                            message = "Amount is required";
                        break;
                    case "CouponCode":
                        if (string.IsNullOrWhiteSpace(CouponCode))
                            message = "Coupon Code is required";
                        break;
                }


                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion
    }
}
