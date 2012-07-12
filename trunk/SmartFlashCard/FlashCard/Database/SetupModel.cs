//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FlashCard.Models;
using FlashCard.Database;
using System.ComponentModel;


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
        }

        #endregion

        #region Entity Properties

        public Setup Setup { get; private set; }

        public bool IsNew { get; private set; }
        public bool IsDirty { get; private set; }
        public bool Deleted { get; set; }
        public bool Checked { get; set; }
        
        public void EndUpdate()
        {
            IsNew = false;
            IsDirty = false;
        }
        

        #endregion

        #region Primitive Properties

        public string SetupID
        {
            get { return this.Setup.SetupID; }
            set
            {
                if (this.Setup.SetupID != value)
                {
                    this.IsDirty = true;
                    this.Setup.SetupID = value;
                    RaisePropertyChanged(() => SetupID);
                }
            }
        }
        public int ViewTimeSecond
        {
            get { return this.Setup.ViewTimeSecond; }
            set
            {
                if (this.Setup.ViewTimeSecond != value)
                {
                    this.IsDirty = true;
                    this.Setup.ViewTimeSecond = value;
                    RaisePropertyChanged(() => ViewTimeSecond);
                }
            }
        }
        public int DistanceTimeSecond
        {
            get { return this.Setup.DistanceTimeSecond; }
            set
            {
                if (this.Setup.DistanceTimeSecond != value)
                {
                    this.IsDirty = true;
                    this.Setup.DistanceTimeSecond = value;
                    RaisePropertyChanged(() => DistanceTimeSecond);
                }
            }
        }
        public Nullable<bool> IsLimitCard
        {
            get { return this.Setup.IsLimitCard; }
            set
            {
                if (this.Setup.IsLimitCard != value)
                {
                    this.IsDirty = true;
                    this.Setup.IsLimitCard = value;
                    RaisePropertyChanged(() => IsLimitCard);
                }
            }
        }
        public Nullable<int> LimitCardNum
        {
            get { return this.Setup.LimitCardNum; }
            set
            {
                if (this.Setup.LimitCardNum != value)
                {
                    this.IsDirty = true;
                    this.Setup.LimitCardNum = value;
                    RaisePropertyChanged(() => LimitCardNum);
                }
            }
        }
        public Nullable<bool> IsEnableSlideShow
        {
            get { return this.Setup.IsEnableSlideShow; }
            set
            {
                if (this.Setup.IsEnableSlideShow != value)
                {
                    this.IsDirty = true;
                    this.Setup.IsEnableSlideShow = value;
                    RaisePropertyChanged(() => IsEnableSlideShow);
                }
            }
        }
        public Nullable<bool> IsEnableLoop
        {
            get { return this.Setup.IsEnableLoop; }
            set
            {
                if (this.Setup.IsEnableLoop != value)
                {
                    this.IsDirty = true;
                    this.Setup.IsEnableLoop = value;
                    RaisePropertyChanged(() => IsEnableLoop);
                }
            }
        }
        public Nullable<bool> IsEnableSoundForShow
        {
            get { return this.Setup.IsEnableSoundForShow; }
            set
            {
                if (this.Setup.IsEnableSoundForShow != value)
                {
                    this.IsDirty = true;
                    this.Setup.IsEnableSoundForShow = value;
                    RaisePropertyChanged(() => IsEnableSoundForShow);
                }
            }
        }
        public Nullable<bool> IsShuffle
        {
            get { return this.Setup.IsShuffle; }
            set
            {
                if (this.Setup.IsShuffle != value)
                {
                    this.IsDirty = true;
                    this.Setup.IsShuffle = value;
                    RaisePropertyChanged(() => IsShuffle);
                }
            }
        }

        #endregion

        #region all the custom code

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

        #region Override Changed
        protected override void RaisePropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "ViewTimeSecond":
                case "DistanceTimeSecond":
                    {
                        var timeOutSecond = this.ViewTimeSecond + this.DistanceTimeSecond;
                        this._timeOut = new TimeSpan(0, 0, timeOutSecond);
                    }
                    break;
                default:
                    break;
            }
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
                        if (IsEnableSlideShow==true)
                            if (DistanceTimeSecond < 0)
                                message = "Distance time is not accepted!";
                        break;
                    case "ViewTimeSecond":
                        if (IsEnableSlideShow==true)
                            if (ViewTimeSecond < 0)
                                message = "Time to view is not accepted!";
                        break;
                    case "LimitCardNum":
                        if (IsLimitCard==true)
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
