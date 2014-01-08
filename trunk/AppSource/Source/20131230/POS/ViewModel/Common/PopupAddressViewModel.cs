using System;
using System.Linq;
using System.Reflection;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using System.Collections.ObjectModel;
using CPC.POS.Database;
using CPCToolkitExtLibraries;
using CPC.Helper;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class PopupAddressViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OKCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        #region Constructors
        public PopupAddressViewModel(base_GuestModel guestModel, base_GuestAddressModel guestAddressModel)
        {
            _ownerViewModel = this;
            InitialCommand();
            GuestModel = guestModel;
            AddressModel = guestAddressModel;
            this.AddressTypeCollection = new AddressTypeCollection();

            foreach (base_GuestAddress guestAddress in guestModel.base_Guest.base_GuestAddress)
            {
                ComboItem addressType = Common.AddressTypes.SingleOrDefault(x => Convert.ToInt32(x.ObjValue).Equals(guestAddress.AddressTypeId));
                if (addressType != null && SelectedAddressModel.AddressTypeId != Convert.ToInt32(addressType.ObjValue))
                {
                    AddressTypeCollection.Add(new AddressTypeModel { ID = Convert.ToInt32(addressType.ObjValue), Name = addressType.Text });
                }
            }

            if (AddressTypeCollection.Any())
                CopyFromVisibility = Visibility.Visible;
            else
                CopyFromVisibility = Visibility.Collapsed;

        }
        #endregion

        #region Properties

        public base_GuestModel GuestModel { get; set; }

        #region SelectedAddressModel
        private base_GuestAddressModel _selectedAddressModel;
        /// <summary>
        /// Gets or sets the AddressModel.
        /// <para>Using for binding in view</para>
        /// Not set to this property.
        /// </summary>
        public base_GuestAddressModel SelectedAddressModel
        {
            get { return _selectedAddressModel; }
            set
            {
                if (_selectedAddressModel != value)
                {
                    _selectedAddressModel = value;
                    OnPropertyChanged(() => SelectedAddressModel);
                }
            }
        }
        #endregion

        #region AddressModel
        private base_GuestAddressModel _addressModel;
        /// <summary>
        /// Gets or sets the AddressModel.
        /// </summary>
        public base_GuestAddressModel AddressModel
        {
            get { return _addressModel; }
            private set
            {
                if (_addressModel != value)
                {
                    _addressModel = value;
                    OnPropertyChanged(() => AddressModel);
                    AddressModelChanged();
                }
            }
        }

        private void AddressModelChanged()
        {
            SelectedAddressModel = Clone(AddressModel) as base_GuestAddressModel;
            SelectedAddressModel.IsDirty = false;
        }
        #endregion

        #region GuestAddressCollection

        #region AddressTypeCollection
        private AddressTypeCollection _addressTypeCollection;
        /// <summary>
        /// Gets or sets the AddressTypeCollection.
        /// </summary>
        public AddressTypeCollection AddressTypeCollection
        {
            get { return _addressTypeCollection; }
            set
            {
                if (_addressTypeCollection != value)
                {
                    _addressTypeCollection = value;
                    OnPropertyChanged(() => AddressTypeCollection);
                }
            }
        }
        #endregion

        #endregion

        #region CopyFromVisibility
        private Visibility _copyFromVisibility = Visibility.Visible;
        /// <summary>
        /// Gets or sets the CopyFromVisibility.
        /// </summary>
        public Visibility CopyFromVisibility
        {
            get { return _copyFromVisibility; }
            set
            {
                if (_copyFromVisibility != value)
                {
                    _copyFromVisibility = value;
                    OnPropertyChanged(() => CopyFromVisibility);
                }
            }
        }
        #endregion

        #region SelectedAddressType
        private AddressTypeModel _selectedAddressType;
        /// <summary>
        /// Gets or sets the SelectedAddressType.
        /// </summary>
        public AddressTypeModel SelectedAddressType
        {
            get { return _selectedAddressType; }
            set
            {
                if (_selectedAddressType != value)
                {
                    _selectedAddressType = value;
                    OnPropertyChanged(() => SelectedAddressType);
                    SelectedAddressTypeChanged();
                }
            }
        }

        private void SelectedAddressTypeChanged()
        {
            if (SelectedAddressModel != null)
            {
                base_GuestAddress guestAddress = GuestModel.base_Guest.base_GuestAddress.SingleOrDefault(x => x.AddressTypeId.Equals(SelectedAddressType.ID));
                if (guestAddress != null)
                {
                    //SelectedAddressModel.AddressTypeId = guestAddress.AddressTypeId;
                    SelectedAddressModel.AddressLine1 = guestAddress.AddressLine1;
                    SelectedAddressModel.AddressLine2 = guestAddress.AddressLine2;
                    SelectedAddressModel.City = guestAddress.City;
                    SelectedAddressModel.PostalCode = guestAddress.PostalCode;
                    SelectedAddressModel.StateProvinceId = guestAddress.StateProvinceId;
                    SelectedAddressModel.CountryId = guestAddress.CountryId;
                }
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
            if (SelectedAddressModel == null)
                return false;
            return IsValid && SelectedAddressModel.IsDirty;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            System.Windows.Window window = FindOwnerWindow(this);
            SelectedAddressModel = ConvertObject(SelectedAddressModel, AddressModel) as base_GuestAddressModel;
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
            System.Windows.Window window = FindOwnerWindow(this);
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

        #region Clone Methods
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
        #endregion

        #region Public Methods
        #endregion
    }
}