using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Model
{
    public class SetupModelBase : ModelBase
    {

        #region Variables
        public enum ShowType
        { 
            FrontSide = 1,
            BackSide=2
        }
        #endregion

        #region Properties

        #region ViewTime
        private TimeSpan _viewTime;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public TimeSpan ViewTime
        {
            get { return _viewTime; }
            set
            {
                if (_viewTime != value)
                {
                    this.OnViewTimeChanging(value);
                    _viewTime = value;
                    RaisePropertyChanged(() => ViewTime);
                    this.OnViewTimeChanged();
                }
            }
        }

        protected virtual void OnViewTimeChanging(TimeSpan value) { }
        protected virtual void OnViewTimeChanged() { }
        #endregion

        #region DistanceTime

        private TimeSpan _distanceTime;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public TimeSpan DistanceTime
        {
            get { return _distanceTime; }
            set
            {
                if (_distanceTime != value)
                {
                    this.OnDistanceTimeChanging(value);
                    _distanceTime = value;
                    RaisePropertyChanged(() => DistanceTime);
                    this.OnDistanceTimeChanged();
                }
            }
        }

        protected virtual void OnDistanceTimeChanging(TimeSpan value) { }
        protected virtual void OnDistanceTimeChanged() { }



        #endregion


        private ShowType _sideShow;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ShowType SideShow
        {
            get { return _sideShow; }
            set
            {
                if (_sideShow != value)
                {
                    this.OnSideShowChanging(value);
                    _sideShow = value;
                    RaisePropertyChanged(() => SideShow);
                    this.OnSideShowChanged();
                }
            }
        }

        protected virtual void OnSideShowChanging(ShowType value) { }
        protected virtual void OnSideShowChanged() { } 
        #endregion




    }
}
