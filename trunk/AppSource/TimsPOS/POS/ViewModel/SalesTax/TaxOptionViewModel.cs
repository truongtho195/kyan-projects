using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using System.Reflection;
using System.Collections.ObjectModel;

namespace CPC.POS.ViewModel
{
    class TaxOptionViewModel : ViewModelBase
    {
        #region Define
        public RelayCommand OKCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        #region Constructors
        public TaxOptionViewModel()
        {
            InitialCommand();
        }
        #endregion

        #region Properties

        public SalesTaxOption TaxOption { get; set; }

        #region SelectedSalesTaxLocationOptionModel
        private base_SaleTaxLocationOptionModel _selectedSalesTaxLocationOptionModel;
        /// <summary>
        /// Gets or sets the SalesTaxLocationOption.
        /// </summary>
        public base_SaleTaxLocationOptionModel SelectedSalesTaxLocationOptionModel
        {
            get { return _selectedSalesTaxLocationOptionModel; }
            set
            {
                if (_selectedSalesTaxLocationOptionModel != value)
                {
                    _selectedSalesTaxLocationOptionModel = value;
                    OnPropertyChanged(() => SelectedSalesTaxLocationOptionModel);

                }
            }
        }
        #endregion

        #region SaleTaxLocationOptionModel
        private base_SaleTaxLocationOptionModel _saleTaxLocationOptionModel;
        /// <summary>
        /// Gets or sets the SaleTaxLocationOptionModel.
        /// </summary>
        public base_SaleTaxLocationOptionModel SaleTaxLocationOptionModel
        {
            get { return _saleTaxLocationOptionModel; }
            set
            {
                if (_saleTaxLocationOptionModel != value)
                {
                    _saleTaxLocationOptionModel = value;
                    OnPropertyChanged(() => SaleTaxLocationOptionModel);
                    SaleTaxLocationOptionChanged();
                }
            }
        }
        #endregion

        #region SaleTaxLocationOptionCollection
        private CollectionBase<base_SaleTaxLocationOptionModel> _saleTaxLocationOptionCollection;
        /// <summary>
        /// Gets or sets the SaleTaxLocationOptionCollection.
        /// </summary>
        public CollectionBase<base_SaleTaxLocationOptionModel> SaleTaxLocationOptionCollection
        {
            get { return _saleTaxLocationOptionCollection; }
            set
            {
                if (_saleTaxLocationOptionCollection != value)
                {
                    _saleTaxLocationOptionCollection = value;
                    OnPropertyChanged(() => SaleTaxLocationOptionCollection);
                    SaleTaxOptionCollectionChanged();
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
            return IsValid;
        }

        /// <summary>
        /// Method to invoke when the NewCommand command is executed.
        /// </summary>
        private void OnOKCommandExecute()
        {
            System.Windows.Window window = FindOwnerWindow(this);
            if (!TaxOption.Is(SalesTaxOption.Multi))
            {
                //Convert Object
                SaleTaxLocationOptionModel = ConvertObject(SelectedSalesTaxLocationOptionModel, SaleTaxLocationOptionModel) as base_SaleTaxLocationOptionModel;
            }

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

        #region AddTaxRateOption
        /// <summary>
        /// Gets the AddTaxRateOption Command.
        /// <summary>

        public RelayCommand<object> AddTaxRateOptionCommand { get; private set; }

        /// <summary>
        /// Method to check whether the AddTaxRateOption command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAddTaxRateOptionCommandCanExecute(object param)
        {
            if (SaleTaxLocationOptionCollection == null)
                return false;
            return !SaleTaxLocationOptionCollection.Any(x => x.IsError) && SaleTaxLocationOptionCollection.Count()< Define.MaxTaxCodeOption;//All Item in collection Not error
        }


        /// <summary>
        /// Method to invoke when the AddTaxRateOption command is executed.
        /// </summary>
        private void OnAddTaxRateOptionCommandExecute(object param)
        {
            SetIndexForCollection();
            AddNewTaxCodeOption();

        }


        #endregion

        #region EditCommand
        /// <summary>
        /// Gets the Edit Command.
        /// <summary>

        public RelayCommand<object> EditCommand { get; private set; }


        /// <summary>
        /// Method to check whether the Edit command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnEditCommandCanExecute(object param)
        {
            if (SaleTaxLocationOptionCollection == null)
                return false;
            return !SaleTaxLocationOptionCollection.Any(x => x.IsError);//All Item in collection Not error
        }


        /// <summary>
        /// Method to invoke when the Edit command is executed.
        /// </summary>
        private void OnEditCommandExecute(object param)
        {
            if (param != null)
            {
                base_SaleTaxLocationOptionModel saleTaxOptionModel = param as base_SaleTaxLocationOptionModel;
                if (SaleTaxLocationOptionCollection != null && SaleTaxLocationOptionCollection.Any(x => x.IsEditing))
                {
                    foreach (base_SaleTaxLocationOptionModel saleTaxOptionEditing in SaleTaxLocationOptionCollection.Where(x => x.IsEditing))
                        saleTaxOptionEditing.IsEditing = false;
                }
                saleTaxOptionModel.IsEditing = true;
            }
        }

        #endregion

        #region RemoveCommand

        /// <summary>
        /// Gets the Remove Command.
        /// <summary>

        public RelayCommand<object> RemoveCommand { get; private set; }

        /// <summary>
        /// Method to check whether the Remove command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnRemoveCommandCanExecute(object param)
        {
            if (SaleTaxLocationOptionCollection == null)
                return false;
            return !SaleTaxLocationOptionCollection.Any(x => x.IsError);//All Item in collection Not error
        }


        /// <summary>
        /// Method to invoke when the Remove command is executed.
        /// </summary>
        private void OnRemoveCommandExecute(object param)
        {
            if (param != null)
            {
                SaleTaxLocationOptionCollection.Remove(param as base_SaleTaxLocationOptionModel);
                SetIndexForCollection();
            }
        }
        #endregion

        #region AcceptTaxOptionCommand
        /// <summary>
        /// Gets the AcceptTaxOption Command.
        /// <summary>

        public RelayCommand<object> AcceptTaxOptionCommand { get; private set; }

        /// <summary>
        /// Method to check whether the AcceptTaxOption command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnAcceptTaxOptionCommandCanExecute(object param)
        {
            if (param == null)
                return false;
            return !(param as base_SaleTaxLocationOptionModel).IsError;
        }


        /// <summary>
        /// Method to invoke when the AcceptTaxOption command is executed.
        /// </summary>
        private void OnAcceptTaxOptionCommandExecute(object param)
        {
            base_SaleTaxLocationOptionModel TaxCodeOptionModel = (param as base_SaleTaxLocationOptionModel);

            if (TaxCodeOptionModel.IsNew)
                TaxCodeOptionModel.IsDirty = true;
            TaxCodeOptionModel.IsEditing = false;
        }

        #endregion

        #endregion

        #region Private Methods
        private void InitialCommand()
        {
            // Route the commands
            OKCommand = new RelayCommand(OnOKCommandExecute, OnOKCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
            AddTaxRateOptionCommand = new RelayCommand<object>(OnAddTaxRateOptionCommandExecute, OnAddTaxRateOptionCommandCanExecute);
            RemoveCommand = new RelayCommand<object>(OnRemoveCommandExecute, OnRemoveCommandCanExecute);
            EditCommand = new RelayCommand<object>(OnEditCommandExecute, OnEditCommandCanExecute);
            AcceptTaxOptionCommand = new RelayCommand<object>(OnAcceptTaxOptionCommandExecute, OnAcceptTaxOptionCommandCanExecute);
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddNewTaxCodeOption()
        {
            if (SaleTaxLocationOptionCollection != null)
            {
                base_SaleTaxLocationOptionModel saleTaxOption = new base_SaleTaxLocationOptionModel();
                saleTaxOption.Index = SaleTaxLocationOptionCollection.Count() + 1;
                saleTaxOption.TaxRate = 0;
                saleTaxOption.TaxComponent = string.Empty;
                saleTaxOption.TaxCondition = 0;
                
                saleTaxOption.IsTemporary = true;
                saleTaxOption.TaxAgency = string.Empty;
                saleTaxOption.IsDirty = false;
                SaleTaxLocationOptionCollection.Add(saleTaxOption);
                saleTaxOption.IsEditing = true;
            }
        }

        /// <summary>
        /// Using for Tax Code Option is Multi
        /// </summary>
        private void SaleTaxOptionCollectionChanged()
        {
            SetIndexForCollection();
            //Create Tax Code Option
            if (SaleTaxLocationOptionCollection != null && SaleTaxLocationOptionCollection.Count == 0)
                AddNewTaxCodeOption();
        }
        
        /// <summary>
        /// Using for Taxcode option Single or Price
        /// </summary>
        private void SaleTaxLocationOptionChanged()
        {
            //Backup
            SelectedSalesTaxLocationOptionModel = Clone(SaleTaxLocationOptionModel) as base_SaleTaxLocationOptionModel;
            SelectedSalesTaxLocationOptionModel.TaxCodeOption = this.TaxOption;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetIndexForCollection()
        {
            for (int i = 0; i < SaleTaxLocationOptionCollection.Count; i++)
            {
                SaleTaxLocationOptionCollection[i].Index = i + 1;
                SaleTaxLocationOptionCollection[i].TaxCodeOption = this.TaxOption;
                SaleTaxLocationOptionCollection[i].IsEditing = false;


            }
        }

        #endregion

        #region Public Methods
        private static object ConvertObject(object object_1, object object_2)
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

        private static object Clone(object obj)
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
    }
}
