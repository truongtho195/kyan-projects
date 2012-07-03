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
            BackSide = 2
        }
        #endregion

        #region Properties

        #region SetupID
        private int _setupID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int SetupID
        {
            get { return _setupID; }
            set
            {
                if (_setupID != value)
                {
                    this.OnSetupIDChanging(value);
                    _setupID = value;
                    RaisePropertyChanged(() => SetupID);
                    this.OnSetupIDChanged();
                }
            }
        }

        protected virtual void OnSetupIDChanging(int value) { }
        protected virtual void OnSetupIDChanged() { }
        #endregion

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
        protected virtual void OnViewTimeSecondChanged() { OnChanged(); }
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
        protected virtual void OnDistanceTimeSecondChanged() { OnChanged(); }



        #endregion

        #region IsLimitCard
        private bool _isLimitCard;
        /// <summary>
        /// Gets or sets the IsLimitCard.
        /// </summary>
        public bool IsLimitCard
        {
            get { return _isLimitCard; }
            set
            {
                if (_isLimitCard != value)
                {
                    this.OnIsLimitCardChanging(value);
                    _isLimitCard = value;
                    RaisePropertyChanged(() => IsLimitCard);
                    this.OnIsLimitCardChanged();
                }
            }
        }

        protected virtual void OnIsLimitCardChanging(bool value) { }
        protected virtual void OnIsLimitCardChanged()
        {
            OnChanged();
        }
        #endregion

        #region LimitCardNum
        private int _limitCardNum;
        /// <summary>
        /// Gets or sets the NumberLimitCard.
        /// </summary>
        public int LimitCardNum
        {
            get { return _limitCardNum; }
            set
            {
                if (_limitCardNum != value)
                {
                    OnLimitCardNumChanging(value);
                    _limitCardNum = value;
                    RaisePropertyChanged(() => LimitCardNum);
                    OnLimitCardNumChanged();
                }
            }
        }

        protected virtual void OnLimitCardNumChanging(int value) { }
        protected virtual void OnLimitCardNumChanged()
        {
            OnChanged();
        }
        #endregion

        #region IsEnableSlideShow
        private bool _isEnableSlideShow;
        /// <summary>
        /// Gets or sets the IsEnableSlideShow.
        /// </summary>
        public bool IsEnableSlideShow
        {
            get { return _isEnableSlideShow; }
            set
            {
                if (_isEnableSlideShow != value)
                {
                    OnIsEnableSlideShowhanging(value);
                    _isEnableSlideShow = value;
                    RaisePropertyChanged(() => IsEnableSlideShow);
                    OnIsEnableSlideShowChanged();
                }
            }
        }
        protected virtual void OnIsEnableSlideShowhanging(bool value) { }
        protected virtual void OnIsEnableSlideShowChanged()
        {
            OnChanged();
        }
        #endregion

        #region IsEnableLoop
        private bool _isEnableLoop;
        /// <summary>
        /// Gets or sets the IsEnableLoop.
        /// </summary>
        public bool IsEnableLoop
        {
            get { return _isEnableLoop; }
            set
            {
                if (_isEnableLoop != value)
                {
                    OnIsEnableLoopChanging(value);
                    _isEnableLoop = value;
                    RaisePropertyChanged(() => IsEnableLoop);
                    OnIsEnableLoophanged();
                }
            }
        }

        protected virtual void OnIsEnableLoopChanging(bool value) { }
        protected virtual void OnIsEnableLoophanged()
        {
            OnChanged();
        }
        #endregion

        #region IsShuffle
        private bool _isShuffle;
        /// <summary>
        /// Gets or sets the IsEnableLoop.
        /// </summary>
        public bool IsShuffle
        {
            get { return _isShuffle; }
            set
            {
                if (_isShuffle != value)
                {
                    OnIsShuffleChanging(value);
                    _isShuffle = value;
                    RaisePropertyChanged(() => IsShuffle);
                    OnIsShufflechanged();
                }
            }
        }

        protected virtual void OnIsShuffleChanging(bool value) { }
        protected virtual void OnIsShufflechanged()
        {
            OnChanged();
        }
        #endregion

        #region IsEnableSoundForShow
        private bool _isEnableSoundForShow;
        /// <summary>
        /// Gets or sets the IsEnableLoop.
        /// </summary>
        public bool IsEnableSoundForShow
        {
            get { return _isEnableSoundForShow; }
            set
            {
                if (_isEnableSoundForShow != value)
                {
                    OnIsEnableSoundForShowChanging(value);
                    _isEnableSoundForShow = value;
                    RaisePropertyChanged(() => IsEnableSoundForShow);
                    OnIsEnableSoundForShowchanged();
                }
            }
        }

        protected virtual void OnIsEnableSoundForShowChanging(bool value) { }
        protected virtual void OnIsEnableSoundForShowchanged()
        {
            OnChanged();
        }
        #endregion

        #endregion




    }
}
