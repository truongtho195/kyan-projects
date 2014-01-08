using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CPC.Helper;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class ContactViewModel : ViewModelBase
    {
        #region Define

        public RelayCommand OKCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Require SelectedContact & ContactCollection
        /// </summary>
        public ContactViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
        }

        #endregion

        #region Properties

        #region IsDirty

        /// <summary>
        /// Gets the IsDirty.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (SelectedContact == null)
                    return false;
                return SelectedContact.IsDirty
                    || (SelectedContact.PersonalInfoModel != null && SelectedContact.PersonalInfoModel.IsDirty);
            }
        }

        #endregion

        #region SelectedContact

        private base_GuestModel _selectedContact;
        /// <summary>
        /// Gets or sets the SelectedContact.
        /// </summary>
        public base_GuestModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                if (_selectedContact != value)
                {
                    _selectedContact = value;
                    OnPropertyChanged(() => SelectedContact);
                }
            }
        }

        #endregion

        #region ContactModel

        private base_GuestModel _contactModel;
        /// <summary>
        /// Gets or sets the ContactModel.
        /// </summary>
        public base_GuestModel ContactModel
        {
            get { return _contactModel; }
            set
            {
                if (_contactModel != value)
                {
                    _contactModel = value;
                    OnPropertyChanged(() => ContactModel);
                    SelectedContactChanged();
                }
            }
        }

        #endregion

        #region ContactCollection

        private ObservableCollection<base_GuestModel> _contactCollection;
        /// <summary>
        /// Gets or sets the ContactCollection.
        /// </summary>
        public ObservableCollection<base_GuestModel> ContactCollection
        {
            get { return _contactCollection; }
            set
            {
                if (_contactCollection != value)
                {
                    _contactCollection = value;
                    OnPropertyChanged(() => ContactCollection);
                }
            }
        }

        #endregion

        #region backObject

        private object _backupObject;
        /// <summary>
        /// Gets or sets the backObject.
        /// </summary>
        public object backObject
        {
            get { return _backupObject; }
            set
            {
                if (_backupObject != value)
                {
                    _backupObject = value;
                    OnPropertyChanged(() => backObject);
                }
            }
        }

        #endregion

        #region IsEnablePrimary

        /// <summary>
        /// Gets or sets the IsEnablePrimary.
        /// </summary>
        public bool IsEnablePrimary
        {
            get
            {
                if (ContactCollection == null)
                    return false;
                return ContactCollection.Any(x => x.GuestNo != SelectedContact.GuestNo && x.IsPrimary == true);
            }
        }

        #endregion

        #endregion

        #region Commands Methods

        #region OKCommand

        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOKCommandCanExecute()
        {
            return IsValid && IsDirty;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            System.Windows.Window window = FindOwnerWindow(this);
            if (SelectedContact.IsPrimary == true)
            {
                foreach (base_GuestModel guestModel in ContactCollection)
                    guestModel.IsPrimary = false;
            }
            ContactModel = ConvertObject(SelectedContact, ContactModel) as base_GuestModel;
            if (ContactModel.IsTemporary)
                ContactModel.IsTemporary = false;
            if (!ContactModel.IsNew)
                ContactModel.DateUpdated = DateTime.Now;
            window.DialogResult = true;
        }

        #endregion

        #region Cancel Command

        /// <summary>
        /// Method to check whether the SaveCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the SaveCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            var window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitialCommand()
        {
            // Route the commands
            OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void SelectedContactChanged()
        {
            if (ContactModel.PersonalInfoModel == null)
            {
                if (ContactModel.base_Guest.base_GuestProfile.Any())
                {
                    ContactModel.PersonalInfoModel = new base_GuestProfileModel(ContactModel.base_Guest.base_GuestProfile.First());
                }
                else
                {
                    ContactModel.PersonalInfoModel = new base_GuestProfileModel();
                    ContactModel.PersonalInfoModel.DOB = DateTime.Today;
                    ContactModel.PersonalInfoModel.IsSpouse = false;
                    ContactModel.PersonalInfoModel.IsEmergency = false;
                    ContactModel.PersonalInfoModel.Gender = Common.Gender.First().Value;
                    ContactModel.PersonalInfoModel.Marital = Common.MaritalStatus.First().Value;
                    ContactModel.PersonalInfoModel.SGender = Common.Gender.First().Value;
                }
                ContactModel.PersonalInfoModel.IsDirty = false;
            }

            SelectedContact = Clone(ContactModel) as base_GuestModel;
        }

        public static object ConvertObject(object object_1, object object_2)
        {
            // Get all the fields of the type, also the privates.
            // Loop through all the fields and copy the information from the parameter class
            // to the newPerson class.
            foreach (PropertyInfo oPropertyInfo in
                object_1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.GetSetMethod(true).IsPublic))
            {
                oPropertyInfo.SetValue(object_2, oPropertyInfo.GetValue(object_1, null), null);
            }
            // Return the cloned object.
            return object_2;
        }

        public static object Clone(object obj)
        {
            try
            {
                // an instance of target type.
                object _object = (object)Activator.CreateInstance(obj.GetType());
                //To get type of value.
                Type type = obj.GetType();
                //To Copy value from input value.
                foreach (PropertyInfo oPropertyInfo in
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.CanRead && x.CanWrite)
                    .Where(x => x.GetSetMethod(true).IsPublic))
                {
                    oPropertyInfo.SetValue(_object, type.GetProperty(oPropertyInfo.Name).GetValue(obj, null), null);
                }

                return _object;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        #endregion

        #region Public Methods

        #endregion
    }
}