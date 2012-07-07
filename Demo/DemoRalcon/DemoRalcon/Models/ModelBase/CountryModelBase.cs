using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;

namespace DemoFalcon.Model
{
    public class CountryModelBase : ModelBase 
    {
        #region Constructors
        public CountryModelBase()
        {

        }
        #endregion

        #region Properties




        private int _countryID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int CountryID
        {
            get { return _countryID; }
            set
            {
                if (_countryID != value)
                {
                    this.OnCountryIDChanging(value);
                    _countryID = value;
                    RaisePropertyChanged(() => CountryID);
                    OnChanged();

                    this.OnCountryIDChanged();
                }
            }
        }

        protected virtual void OnCountryIDChanging(int value) { }
        protected virtual void OnCountryIDChanged() { }




        private string _countryName;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string CountryName
        {
            get { return _countryName; }
            set
            {
                if (_countryName != value)
                {
                    this.OnCountryNameChanging(value);
                    _countryName = value;
                    RaisePropertyChanged(() => CountryName);
                    OnChanged();

                    this.OnCountryNameChanged();
                }
            }
        }

        protected virtual void OnCountryNameChanging(string value) { }
        protected virtual void OnCountryNameChanged() { }
















        #endregion
    }
}
