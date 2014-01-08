using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{

    class ServiceFeeDetailViewModel : ViewModelBase
    {
        #region Define
     
        #endregion

        #region Constructors
        public ServiceFeeDetailViewModel(decimal openACFee = 0, decimal layawayFee = 0, decimal otherFee = 0,decimal layawayTotalFee=0)
        {
            InitialCommand();

            this.OpenACFee = openACFee;
            this.LayawayFee = layawayFee;
            this.OtherFee = otherFee;
            this.Total = layawayTotalFee;
        }
        #endregion

        #region Properties

        #region OpenACFee
        private decimal _openACFee;
        /// <summary>
        /// Gets or sets the OpenACFee.
        /// </summary>
        public decimal OpenACFee
        {
            get { return _openACFee; }
            set
            {
                if (_openACFee != value)
                {
                    _openACFee = value;
                    OnPropertyChanged(() => OpenACFee);
                }
            }
        }
        #endregion

        #region LayawayFee
        private decimal _layawayFee;
        /// <summary>
        /// Gets or sets the LayawayFee.
        /// </summary>
        public decimal LayawayFee
        {
            get { return _layawayFee; }
            set
            {
                if (_layawayFee != value)
                {
                    _layawayFee = value;
                    OnPropertyChanged(() => LayawayFee);
                }
            }
        }
        #endregion

        #region OtherFee
        private decimal _otherFee;
        /// <summary>
        /// Gets or sets the OtherFee.
        /// </summary>
        public decimal OtherFee
        {
            get { return _otherFee; }
            set
            {
                if (_otherFee != value)
                {
                    _otherFee = value;
                    OnPropertyChanged(() => OtherFee);
                }
            }
        }
        #endregion


        #region Total
        private decimal _total;
        /// <summary>
        /// Gets or sets the Total.
        /// </summary>
        public decimal Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(() => Total);
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
            return true;
        }


        /// <summary>
        /// Method to invoke when the Ok command is executed.
        /// </summary>
        private void OnOkCommandExecute(object param)
        {
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
    }


}
