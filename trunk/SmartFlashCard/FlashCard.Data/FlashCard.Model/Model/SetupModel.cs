using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FlashCard.Model
{
    public class SetupModel : SetupModelBase, IDataErrorInfo
    {
        /// <summary>
        ///               TimeOut
        /// |----------------|---------------------------|
        ///    DistanceTime       ViewTime
        /// </summary>

        public SetupModel()
        {
            //initial defaul data
            this.IsEnableSlideShow = true;
            this.DistanceTimeSecond = 7;
            this.ViewTimeSecond = 8;
            this.IsEnableLoop = true;
            this.IsLimitCard = false;
            this.LimitCardNum = 5;
            this.IsShuffle = false;
            this.IsNew = true;
        }

        #region Properties
        private TimeSpan _timeOut;
        /// <summary>
        /// This is Extend Properties
        ///<para> Gets or sets the property value.</para>
        /// </summary>
        public TimeSpan TimeOut
        {
            get { return _timeOut; }
            set
            {
                if (_timeOut != value)
                {
                    _timeOut = value;
                    RaisePropertyChanged(() => TimeOut);
                }
            }
        }

        #endregion

        #region Overide
        protected override void OnViewTimeSecondChanged()
        {
            base.OnViewTimeSecondChanged();
            var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
            this._timeOut = new TimeSpan(0, 0, timeOutSecond);
        }

        protected override void OnDistanceTimeSecondChanged()
        {
            base.OnDistanceTimeSecondChanged();
            var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
            this._timeOut = new TimeSpan(0, 0, timeOutSecond);

        }
        #endregion

        #region Methods

        #endregion


        #region DataErrorInfo
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        private Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    RaisePropertyChanged(() => Errors);
                }
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = String.Empty;
                this.Errors.Remove(columnName);
                switch (columnName)
                {
                    case "DistanceTimeSecond":
                        if (IsEnableSlideShow)
                            if (DistanceTimeSecond < 0)
                                message = "Distance time is not accepted!";
                        break;
                    case "ViewTimeSecond":
                        if (IsEnableSlideShow)
                            if (ViewTimeSecond < 0)
                                message = "Time to view is not accepted!";
                        break;
                    case "LimitCardNum":
                        if (IsLimitCard)
                        {
                            if (LimitCardNum <= 0)
                                message = "Limit card number is not accepted!";
                        }
                        break;

                }
                if (!String.IsNullOrEmpty(message))
                {
                    this.Errors.Add(columnName, message);
                }
                return message;
            }
        } 
        #endregion
    }
}
