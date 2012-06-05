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
        private int _viewTimeSecond;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int ViewTimeSecond
        {
            get { return _viewTimeSecond; }
            set
            {
                if (_viewTimeSecond != value)
                {
                    this.OnViewTimeSecondChanging(value);
                    _viewTimeSecond = value;
                    RaisePropertyChanged(() => ViewTimeSecond);
                    this.OnViewTimeSecondChanged();
                }
            }
        }

        protected virtual void OnViewTimeSecondChanging(int value) { }
        protected virtual void OnViewTimeSecondChanged() { }
        #endregion

        #region DistanceTime

        private int _distanceTimeSecond;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int DistanceTimeSecond
        {
            get { return _distanceTimeSecond; }
            set
            {
                if (_distanceTimeSecond != value)
                {
                    this.OnDistanceTimeSecondChanging(value);
                    _distanceTimeSecond = value;
                    RaisePropertyChanged(() => DistanceTimeSecond);
                    this.OnDistanceTimeSecondChanged();
                }
            }
        }

        protected virtual void OnDistanceTimeSecondChanging(int value) { }
        protected virtual void OnDistanceTimeSecondChanged() { }



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
