using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class SetupModel:SetupModelBase
    {
        public SetupModel()
        {
            
        }

        #region Properties
        private TimeSpan _timeOut;
        /// <summary>
        /// Gets or sets the property value.
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
        protected override void OnViewTimeChanged()
        {
            if (this.ViewTime != null && this.DistanceTime!=null)
            {
                this._timeOut = this.ViewTime.Add(this.DistanceTime);
            }
        }

        protected override void OnDistanceTimeChanged()
        {
            if (this.ViewTime != null && this.DistanceTime != null)
            {
                this._timeOut = this.ViewTime.Add(this.DistanceTime);
            }
        }
        #endregion


    }
}
