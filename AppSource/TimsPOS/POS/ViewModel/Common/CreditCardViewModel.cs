using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Reflection;
using System.Windows;

namespace CPC.POS.ViewModel
{
    class CreditCardViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        #endregion

        #region Constructors
        public CreditCardViewModel()
        {
            _ownerViewModel = this;
            InitialCommand();
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
            if (SelectedPaymentCard.IsDirty)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to close?", "POS", MessageBoxButton.YesNo);
                if (result.Is(MessageBoxResult.No))
                    return;
            }
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
