using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class SetupModel : SetupModelBase
    {
        public SetupModel()
        {

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
            if (this.ViewTimeSecond != null && this.DistanceTimeSecond != null)
            {
                var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
                this._timeOut = new TimeSpan(0,0,timeOutSecond);
            }
        }

        protected override void OnDistanceTimeSecondChanged()
        {
            if (this.ViewTimeSecond != null && this.DistanceTimeSecond != null)
            {
                var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
                this._timeOut = new TimeSpan(0, 0, timeOutSecond);
            }
        }
        #endregion

        #region Methods
       
        #endregion


    }
}
