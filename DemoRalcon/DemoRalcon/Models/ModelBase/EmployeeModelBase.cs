using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using DemoFalcon.Helper;

namespace DemoFalcon.Model
{
    public class EmployeeModelBase : ModelBase 
    {
        #region Constructors
        public EmployeeModelBase()
        {

        }
        #endregion

        #region Properties



        private int _employeeID;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int EmployeeID
        {
            get { return _employeeID; }
            set
            {
                if (_employeeID != value)
                {
                    this.OnEmployeeIDChanging(value);
                    _employeeID = value;
                    RaisePropertyChanged(() => EmployeeID);
                    OnChanged();

                    this.OnEmployeeIDChanged();
                }
            }
        }

        protected virtual void OnEmployeeIDChanging(int value) { }
        protected virtual void OnEmployeeIDChanged() { }



        private string _firstName;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    this.OnFirstNameChanging(value);
                    _firstName = value;
                    RaisePropertyChanged(() => FirstName);
                    OnChanged();

                    this.OnFirstNameChanged();
                }
            }
        }

        protected virtual void OnFirstNameChanging(string value) { }
        protected virtual void OnFirstNameChanged() { }



        private string _middleName;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string MiddleName
        {
            get { return _middleName; }
            set
            {
                if (_middleName != value)
                {
                    this.OnMiddleNameChanging(value);
                    _middleName = value;
                    RaisePropertyChanged(() => MiddleName);
                    OnChanged();

                    this.OnMiddleNameChanged();
                }
            }
        }

        protected virtual void OnMiddleNameChanging(string value) { }
        protected virtual void OnMiddleNameChanged() { }



        private string _lastName;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    this.OnLastNameChanging(value);
                    _lastName = value;
                    RaisePropertyChanged(() => LastName);
                    OnChanged();

                    this.OnLastNameChanged();
                }
            }
        }

        protected virtual void OnLastNameChanging(string value) { }
        protected virtual void OnLastNameChanged() { }



        private int _gender;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int Gender
        {
            get { return _gender; }
            set
            {
                if (_gender != value)
                {
                    this.OnGenderChanging(value);
                    _gender = value;
                    RaisePropertyChanged(() => Gender);
                    OnChanged();

                    this.OnGenderChanged();
                }
            }
        }

        protected virtual void OnGenderChanging(int value) { }
        protected virtual void OnGenderChanged() { }




        private DateTime _birthDate;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set
            {
                if (_birthDate != value)
                {
                    this.OnBirthDateChanging(value);
                    _birthDate = value;
                    RaisePropertyChanged(() => BirthDate);
                    OnChanged();

                    this.OnBirthDateChanged();
                }
            }
        }

        protected virtual void OnBirthDateChanging(DateTime value) { }
        protected virtual void OnBirthDateChanged() { }



        private string _phone;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value)
                {
                    this.OnPhoneChanging(value);
                    _phone = value;
                    RaisePropertyChanged(() => Phone);
                    OnChanged();

                    this.OnPhoneChanged();
                }
            }
        }

        protected virtual void OnPhoneChanging(string value) { }
        protected virtual void OnPhoneChanged() { }



        private string _email;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    this.OnEmailChanging(value);
                    _email = value;
                    RaisePropertyChanged(() => Email);
                    OnChanged();

                    this.OnEmailChanged();
                }
            }
        }

        protected virtual void OnEmailChanging(string value) { }
        protected virtual void OnEmailChanged() { }



        private string _address;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set
            {
                if (_address != value)
                {
                    this.OnAddressChanging(value);
                    _address = value;
                    RaisePropertyChanged(() => Address);
                    OnChanged();

                    this.OnAddressChanged();
                }
            }
        }

        protected virtual void OnAddressChanging(string value) { }
        protected virtual void OnAddressChanged() { }




        private string _note;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    this.OnNoteChanging(value);
                    _note = value;
                    RaisePropertyChanged(() => Note);
                    OnChanged();

                    this.OnNoteChanged();
                }
            }
        }

        protected virtual void OnNoteChanging(string value) { }
        protected virtual void OnNoteChanged() { }






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
                    this.OnCountryChanging(value);
                    _countryID = value;
                    RaisePropertyChanged(() => CountryID);
                    OnChanged();

                    this.OnCountryChanged();
                }
            }
        }

        protected virtual void OnCountryChanging(int value) { }
        protected virtual void OnCountryChanged() { }






        private CountryModel _countryModel;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CountryModel CountryModel
        {
            get { return _countryModel; }
            set
            {
                if (_countryModel != value)
                {
                    this.OnCountryModelChanging(value);
                    _countryModel = value;
                    RaisePropertyChanged(() => CountryModel);
                    OnChanged();

                    this.OnCountryModelChanged();
                }
            }
        }

        protected virtual void OnCountryModelChanging(CountryModel value) { }
        protected virtual void OnCountryModelChanged() { }





        private ObservableCollection<DepartmentDetailModel> _deparmentDetailCollection;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<DepartmentDetailModel> DepartmentDetailCollection
        {
            get { return _deparmentDetailCollection; }
            set
            {
                if (_deparmentDetailCollection != value)
                {
                    this.OnDepartmentDetailCollectionChanging(value);
                    _deparmentDetailCollection = value;
                    RaisePropertyChanged(() => DepartmentDetailCollection);
                    OnChanged();
                    this.OnDepartmentDetailCollectionChanged();
                }
            }
        }

        protected virtual void OnDepartmentDetailCollectionChanging(ObservableCollection<DepartmentDetailModel> value) { }
        protected virtual void OnDepartmentDetailCollectionChanged() { }















        #endregion
    }
}
