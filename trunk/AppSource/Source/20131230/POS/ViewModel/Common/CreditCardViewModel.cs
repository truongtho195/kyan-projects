using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using CPC.Helper;
using CPC.POS.Model;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPCToolkitExtLibraries;

namespace CPC.POS.ViewModel
{
    class CreditCardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        #endregion

        #region Constructors
        public CreditCardViewModel(base_GuestModel guestModel)
        {
            _ownerViewModel = this;
            InitialCommand();
            GuestModel = guestModel;

            LoadStaticData();
        }

        private void LoadStaticData()
        {
            this.AddressTypeCollection = new CPCToolkitExtLibraries.AddressTypeCollection();

            if (GuestModel != null && GuestModel.AddressControlCollection != null)
            {
                foreach (AddressControlModel addressControlModel in GuestModel.AddressControlCollection)
                {
                    if (!string.IsNullOrWhiteSpace(addressControlModel.AddressLine1) || !string.IsNullOrWhiteSpace(addressControlModel.PostalCode))
                    {
                        if (addressControlModel.AddressTypeID.Equals(0))
                        {
                            AddressTypeCollection.Add(new AddressTypeModel { ID = 0, Name = Language.GetMsg("SO_TextBlock_Home") });
                        }
                        else if (addressControlModel.AddressTypeID.Equals(1))
                        {
                            AddressTypeCollection.Add(new AddressTypeModel { ID = 1, Name = Language.GetMsg("SO_TextBlock_Business") });
                        }
                        else if (addressControlModel.AddressTypeID.Equals(2))
                        {
                            AddressTypeCollection.Add(new AddressTypeModel { ID = 2, Name = Language.GetMsg("SO_TextBlock_Billing") });
                        }
                        else
                        {
                            AddressTypeCollection.Add(new AddressTypeModel { ID = 3, Name = Language.GetMsg("SO_TextBlock_Shipping") });
                        }
                    }
                }
            }

            //VisibilityAddressType = this.AddressTypeCollection.Any() ? Visibility.Visible : Visibility.Collapsed;

        }
        #endregion

        #region Properties

        #region SelectedPaymentCard
        private base_GuestPaymentCardModel _selectedPaymentCard;
        /// <summary>
        /// Gets or sets the SelectedPaymentCard.
        /// <para>This property using for binding to View</para>
        /// </summary>
        public base_GuestPaymentCardModel SelectedPaymentCard
        {
            get { return _selectedPaymentCard; }
            set
            {
                if (_selectedPaymentCard != value)
                {
                    _selectedPaymentCard = value;
                    OnPropertyChanged(() => SelectedPaymentCard);
                }
            }
        }
        #endregion

        #region PaymentCardModel
        private base_GuestPaymentCardModel _paymentCardModel;
        /// <summary>
        /// Gets or sets the PaymentCardModel.
        /// <para>Property for set from another ViewModel</para>
        /// </summary>
        public base_GuestPaymentCardModel PaymentCardModel
        {
            get { return _paymentCardModel; }
            set
            {
                if (_paymentCardModel != value)
                {
                    _paymentCardModel = value;
                    OnPropertyChanged(() => PaymentCardModel);
                    PaymentCardModelChanged();
                }
            }
        }


        #endregion

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

        public base_GuestModel GuestModel { get; set; }

        #region AddressTypeModel
        private AddressTypeModel _addressTypeModel;
        /// <summary>
        /// Gets or sets the AddressTypeModel.
        /// </summary>
        public AddressTypeModel AddressTypeModel
        {
            get { return _addressTypeModel; }
            set
            {
                if (_addressTypeModel != value)
                {
                    _addressTypeModel = value;
                    OnPropertyChanged(() => AddressTypeModel);
                    AddressTypeModelChanged();

                }
            }
        }

        private void AddressTypeModelChanged()
        {
            AddressControlModel addressControlModel = GuestModel.AddressControlCollection.SingleOrDefault(x => x.AddressTypeID.Equals(AddressTypeModel.ID));
            SelectedPaymentCard.BillingAddress = addressControlModel.AddressLine1;
            SelectedPaymentCard.ZipCode = addressControlModel.PostalCode;
        }
        #endregion


        #region VisibilityAddressType
        private Visibility _VisibilityAddressType = Visibility.Visible;
        /// <summary>
        /// Gets or sets the VisibilityAddressType.
        /// </summary>
        public Visibility VisibilityAddressType
        {
            get { return _VisibilityAddressType; }
            set
            {
                if (_VisibilityAddressType != value)
                {
                    _VisibilityAddressType = value;
                    OnPropertyChanged(() => VisibilityAddressType);
                }
            }
        }
        #endregion


        #endregion

        #region Commands Methods

        #region OkCommand
        /// <summary>
        /// Method to check whether the NewCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            if (SelectedPaymentCard == null)
                return false;
            return IsValid && SelectedPaymentCard.IsDirty;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            System.Windows.Window window = FindOwnerWindow(this);
            PaymentCardModel = ConvertObject(SelectedPaymentCard, PaymentCardModel) as base_GuestPaymentCardModel;
            PaymentCardModel.IsTemporary = false;
            if (!PaymentCardModel.IsNew)
                PaymentCardModel.DateUpdated = DateTime.Now;

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
            //if (SelectedPaymentCard.IsDirty)
            //{
            //    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to close?", "POS", MessageBoxButton.YesNo);
            //    if (result.Is(MessageBoxResult.No))
            //        return;
            //}
            window.DialogResult = false;
        }
        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        private void PaymentCardModelChanged()
        {
            SelectedPaymentCard = Clone(PaymentCardModel) as base_GuestPaymentCardModel;
            SelectedPaymentCard.IsDirty = PaymentCardModel.IsDirty;
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