//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FlashCard.Models;


namespace FlashCard.Database
{
    /// <summary>
    /// Model for table Setup 
    /// </summary>
    public partial class SetupModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public SetupModel()
        {
            this.IsNew = true;
            this.Setup = new Setup();
        }

        // Default contructor that set entity to field
        public SetupModel(Setup setup)
        {
            this.Setup = setup;
            ToModel();
        }

        #endregion

        #region Entity Properties

        public Setup Setup { get; private set; }

        protected bool _isNew;
        /// <summary>
        /// Gets or sets the IsNew
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    RaisePropertyChanged(() => IsNew);
                }
            }
        }

        protected bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        protected bool _isDeleted;
        /// <summary>
        /// Gets or sets the IsDeleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    RaisePropertyChanged(() => IsDeleted);
                }
            }
        }

        protected bool _isChecked;
        /// <summary>
        /// Gets or sets the IsChecked
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged(() => IsChecked);
                }
            }
        }

        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        public void ToEntity()
        {
            if (IsNew)
                this.Setup.SetupID = this.SetupID;
            this.Setup.ViewTimeSecond = this.ViewTimeSecond;
            this.Setup.DistanceTimeSecond = this.DistanceTimeSecond;
            this.Setup.IsLimitCard = this.IsLimitCard;
            this.Setup.LimitCardNum = this.LimitCardNum;
            this.Setup.IsEnableSlideShow = this.IsEnableSlideShow;
            this.Setup.IsEnableLoop = this.IsEnableLoop;
            this.Setup.IsEnableSoundForShow = this.IsEnableSoundForShow;
            this.Setup.IsShuffle = this.IsShuffle;
        }

        public void ToModel()
        {
            this.SetupID = this.Setup.SetupID;
            this.ViewTimeSecond = this.Setup.ViewTimeSecond;
            this.DistanceTimeSecond = this.Setup.DistanceTimeSecond;
            this.IsLimitCard = this.Setup.IsLimitCard;
            this.LimitCardNum = this.Setup.LimitCardNum;
            this.IsEnableSlideShow = this.Setup.IsEnableSlideShow;
            this.IsEnableLoop = this.Setup.IsEnableLoop;
            this.IsEnableSoundForShow = this.Setup.IsEnableSoundForShow;
            this.IsShuffle = this.Setup.IsShuffle;
        }

        #endregion

        #region Primitive Properties

        protected string _setupID;
        /// <summary>
        /// Gets or sets the SetupID.
        /// </summary>
        public string SetupID
        {
            get { return this._setupID; }
            set
            {
                if (this._setupID != value)
                {
                    this.IsDirty = true;
                    this._setupID = value;
                    RaisePropertyChanged(() => SetupID);
                }
            }
        }

        protected int _viewTimeSecond;
        /// <summary>
        /// Gets or sets the ViewTimeSecond.
        /// </summary>
        public int ViewTimeSecond
        {
            get { return this._viewTimeSecond; }
            set
            {
                if (this._viewTimeSecond != value)
                {
                    this.IsDirty = true;
                    this._viewTimeSecond = value;
                    RaisePropertyChanged(() => ViewTimeSecond);
                }
            }
        }

        protected int _distanceTimeSecond;
        /// <summary>
        /// Gets or sets the DistanceTimeSecond.
        /// </summary>
        public int DistanceTimeSecond
        {
            get { return this._distanceTimeSecond; }
            set
            {
                if (this._distanceTimeSecond != value)
                {
                    this.IsDirty = true;
                    this._distanceTimeSecond = value;
                    RaisePropertyChanged(() => DistanceTimeSecond);
                }
            }
        }

        protected Nullable<bool> _isLimitCard;
        /// <summary>
        /// Gets or sets the IsLimitCard.
        /// </summary>
        public Nullable<bool> IsLimitCard
        {
            get { return this._isLimitCard; }
            set
            {
                if (this._isLimitCard != value)
                {
                    this.IsDirty = true;
                    this._isLimitCard = value;
                    RaisePropertyChanged(() => IsLimitCard);
                }
            }
        }

        protected Nullable<int> _limitCardNum;
        /// <summary>
        /// Gets or sets the LimitCardNum.
        /// </summary>
        public Nullable<int> LimitCardNum
        {
            get { return this._limitCardNum; }
            set
            {
                if (this._limitCardNum != value)
                {
                    this.IsDirty = true;
                    this._limitCardNum = value;
                    RaisePropertyChanged(() => LimitCardNum);
                }
            }
        }

        protected Nullable<bool> _isEnableSlideShow;
        /// <summary>
        /// Gets or sets the IsEnableSlideShow.
        /// </summary>
        public Nullable<bool> IsEnableSlideShow
        {
            get { return this._isEnableSlideShow; }
            set
            {
                if (this._isEnableSlideShow != value)
                {
                    this.IsDirty = true;
                    this._isEnableSlideShow = value;
                    RaisePropertyChanged(() => IsEnableSlideShow);
                }
            }
        }

        protected Nullable<bool> _isEnableLoop;
        /// <summary>
        /// Gets or sets the IsEnableLoop.
        /// </summary>
        public Nullable<bool> IsEnableLoop
        {
            get { return this._isEnableLoop; }
            set
            {
                if (this._isEnableLoop != value)
                {
                    this.IsDirty = true;
                    this._isEnableLoop = value;
                    RaisePropertyChanged(() => IsEnableLoop);
                }
            }
        }

        protected Nullable<bool> _isEnableSoundForShow;
        /// <summary>
        /// Gets or sets the IsEnableSoundForShow.
        /// </summary>
        public Nullable<bool> IsEnableSoundForShow
        {
            get { return this._isEnableSoundForShow; }
            set
            {
                if (this._isEnableSoundForShow != value)
                {
                    this.IsDirty = true;
                    this._isEnableSoundForShow = value;
                    RaisePropertyChanged(() => IsEnableSoundForShow);
                }
            }
        }

        protected Nullable<bool> _isShuffle;
        /// <summary>
        /// Gets or sets the IsShuffle.
        /// </summary>
        public Nullable<bool> IsShuffle
        {
            get { return this._isShuffle; }
            set
            {
                if (this._isShuffle != value)
                {
                    this.IsDirty = true;
                    this._isShuffle = value;
                    RaisePropertyChanged(() => IsShuffle);
                }
            }
        }


        #endregion

        #region Custom Code

        /// <summary>
        ///               TimeOut
        /// |----------------|---------------------------|
        ///    DistanceTime       ViewTime
        /// </summary>
        #region Properties
        private TimeSpan _timeOut;
        /// <summary>
        /// This is Extend Properties
        ///<para> Gets or sets the property value.</para>
        /// </summary>
        public TimeSpan TimeOut
        {
            get
            {
                var timeOutSecond = this.Setup.ViewTimeSecond + this.Setup.DistanceTimeSecond;
                this._timeOut = new TimeSpan(0, 0, timeOutSecond);
                return _timeOut;
            }
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

        #region Override Changed
        protected override void RaisePropertyChangedCompleted(string propertyName)
        {
            //switch (propertyName)
            //{
            //    case "ViewTimeSecond":
            //    case "DistanceTimeSecond":
            //        {
            //            var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
            //            this._timeOut = new TimeSpan(0, 0, timeOutSecond);
            //        }
            //        break;
            //    default:
            //        break;
            //}
            base.RaisePropertyChangedCompleted(propertyName);
        }
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
                        if (IsEnableSlideShow == true)
                            if (DistanceTimeSecond < 0)
                                message = "Distance time is not accepted!";
                        break;
                    case "ViewTimeSecond":
                        if (IsEnableSlideShow == true)
                            if (ViewTimeSecond < 0)
                                message = "Time to view is not accepted!";
                        break;
                    case "LimitCardNum":
                        if (IsLimitCard == true)
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

        #endregion
    }
}