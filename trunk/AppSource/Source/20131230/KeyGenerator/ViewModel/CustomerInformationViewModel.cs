using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Command;
using CPC.Toolkit.Base;
using KeyGenerator.Model;
using CPC.POS.Database;
using KeyGenerator.Database;
using System.Windows;
using System.IO;

namespace KeyGenerator.ViewModel
{
    class CustomerInformationViewModel : ViewModelBase
    {
        #region Define
        private string POS_ID_FORMAT = "yyMMddHHmmss";
        #endregion

        #region Constructors
        public CustomerInformationViewModel()
        {
            EntityDB = new UnitOfWork();

            InitialCommand();

            InitData();
        }


        #endregion

        #region Properties

        #region Title
        /// <summary>
        /// Gets the Title.
        /// </summary>
        public string Title
        {
            get { return "Customer Key Generation"; }

        }
        #endregion


        public UnitOfWork EntityDB { get; set; }


        #region IsEdited
        private bool _isEdited;
        /// <summary>
        /// Gets or sets the IsEdited.
        /// </summary>
        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                if (_isEdited != value)
                {
                    _isEdited = value;
                    OnPropertyChanged(() => IsEdited);
                }
            }
        }
        #endregion

        #region SelectedCustomer
        private CustomerModel _selectedCustomer;
        /// <summary>
        /// Gets or sets the SelectedCustomer.
        /// </summary>
        public CustomerModel SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(() => SelectedCustomer);
                    IsEdited = SelectedCustomer != null ? true : false;
                }
            }
        }
        #endregion

        #region CustomerCollection
        private CollectionBase<CustomerModel> _customerCollection;
        /// <summary>
        /// Gets or sets the CustomerCollection.
        /// </summary>
        public CollectionBase<CustomerModel> CustomerCollection
        {
            get { return _customerCollection; }
            set
            {
                if (_customerCollection != value)
                {
                    _customerCollection = value;
                    OnPropertyChanged(() => CustomerCollection);
                }
            }
        }
        #endregion

        #region SelectedCustomerDetail
        private CustomerDetailModel _selectedCustomerDetail;
        /// <summary>
        /// Gets or sets the SelectedCustomerDetail.
        /// </summary>
        public CustomerDetailModel SelectedCustomerDetail
        {
            get { return _selectedCustomerDetail; }
            set
            {
                if (_selectedCustomerDetail != value)
                {
                    _selectedCustomerDetail = value;
                    OnPropertyChanged(() => SelectedCustomerDetail);
                }
            }
        }
        #endregion

        #endregion

        #region Commands Methods
        #region SaveNewCommand
        /// <summary>
        /// Gets the SaveNew Command.
        /// <summary>

        public RelayCommand<object> SaveNewCommand { get; private set; }


        /// <summary>
        /// Method to check whether the SaveNew command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSaveNewCommandCanExecute(object param)
        {
            if (SelectedCustomer == null)
                return false;
            return string.IsNullOrWhiteSpace(SelectedCustomer.Error);
        }


        /// <summary>
        /// Method to invoke when the SaveNew command is executed.
        /// </summary>
        private void OnSaveNewCommandExecute(object param)
        {
            if (SelectedCustomer.IsDirty)
            {
                SaveCustomer(SelectedCustomer);
            }
            CreateNewCustomer();
        }
        #endregion

        #region DeleteCommand

        /// <summary>
        /// Gets the Delete Command.
        /// <summary>

        public RelayCommand<object> DeleteCommand { get; private set; }



        /// <summary>
        /// Method to check whether the Delete command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnDeleteCommandCanExecute(object param)
        {
            if (SelectedCustomer == null)
                return false;
            return !SelectedCustomer.IsNew;
        }


        /// <summary>
        /// Method to invoke when the Delete command is executed.
        /// </summary>
        private void OnDeleteCommandExecute(object param)
        {
            if (DeletedCustomer(SelectedCustomer))
            {
                CustomerCollection.Remove(SelectedCustomer);
                CreateNewCustomer();
            }

        }
        #endregion

        #region GenerateLicenseStore
        /// <summary>
        /// Gets the GenerateLicenseStore Command.
        /// <summary>

        public RelayCommand<object> GenerateLicenseStoreCommand { get; private set; }



        /// <summary>
        /// Method to check whether the GenerateLicenseStore command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnGenerateLicenseStoreCommandCanExecute(object param)
        {
            if (SelectedCustomerDetail == null || SelectedCustomer == null)
                return false;
            return true;

        }


        /// <summary>
        /// Method to invoke when the GenerateLicenseStore command is executed.
        /// </summary>
        private void OnGenerateLicenseStoreCommandExecute(object param)
        {
            CustomerDetailModel customerDetailModel = SelectedCustomerDetail;
            KeyGeneratorViewModel viewModel = new KeyGeneratorViewModel(customerDetailModel, SelectedCustomer);

            KeyGeneratorView keyGeneratorView = new KeyGeneratorView(viewModel);
            viewModel.CurrentView = keyGeneratorView;//Set View To Close
            keyGeneratorView.Owner = App.Current.MainWindow;
            bool? resultDialog = keyGeneratorView.ShowDialog();
            if (resultDialog == true)
            {
                SelectedCustomerDetail.ToModel(viewModel.CustomerDetailModel);

                SelectedCustomerDetail.ToEntity();
                EntityDB.Update<CustomerDetail>(SelectedCustomerDetail.CustomerDetail);
                EntityDB.Commit();
            }
        }

        #endregion

        #region CustomerSelectionChange
        /// <summary>
        /// Gets the CustomerSelectionChanged Command.
        /// <summary>

        public RelayCommand<object> CustomerSelectionChangedCommand { get; private set; }



        /// <summary>
        /// Method to check whether the CustomerSelectionChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCustomerSelectionChangedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CustomerSelectionChanged command is executed.
        /// </summary>
        private void OnCustomerSelectionChangedCommandExecute(object param)
        {
            SelectedCustomer = param as CustomerModel;

        }
        #endregion

        #region CustomerDetailSelectionChanged
        /// <summary>
        /// Gets the CustomerDetailSelectionChanged Command.
        /// <summary>

        public RelayCommand<object> CustomerDetailSelectionChangedCommand { get; private set; }



        /// <summary>
        /// Method to check whether the CustomerDetailSelectionChanged command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCustomerDetailSelectionChangedCommandCanExecute(object param)
        {
            return true;
        }


        /// <summary>
        /// Method to invoke when the CustomerDetailSelectionChanged command is executed.
        /// </summary>
        private void OnCustomerDetailSelectionChangedCommandExecute(object param)
        {
            SelectedCustomerDetail = param as CustomerDetailModel;
        }
        #endregion



        #region ExportLicense
        /// <summary>
        /// Gets the ExportLicense Command.
        /// <summary>

        public RelayCommand<object> ExportLicenseCommand { get; private set; }



        /// <summary>
        /// Method to check whether the ExportLicense command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnExportLicenseCommandCanExecute(object param)
        {
            if (SelectedCustomer == null)
                return false;
            return SelectedCustomer.CustomerDetailCollection != null && SelectedCustomer.CustomerDetailCollection.Any(x => x.IsGenerated);
        }


        /// <summary>
        /// Method to invoke when the ExportLicense command is executed.
        /// </summary>
        private void OnExportLicenseCommandExecute(object param)
        {
            ExportLicenseKey(SelectedCustomer);
        }
        #endregion


        #endregion

        #region Private Methods


        private void InitialCommand()
        {
            SaveNewCommand = new RelayCommand<object>(OnSaveNewCommandExecute, OnSaveNewCommandCanExecute);
            DeleteCommand = new RelayCommand<object>(OnDeleteCommandExecute, OnDeleteCommandCanExecute);
            GenerateLicenseStoreCommand = new RelayCommand<object>(OnGenerateLicenseStoreCommandExecute, OnGenerateLicenseStoreCommandCanExecute);
            CustomerSelectionChangedCommand = new RelayCommand<object>(OnCustomerSelectionChangedCommandExecute, OnCustomerSelectionChangedCommandCanExecute);
            CustomerDetailSelectionChangedCommand = new RelayCommand<object>(OnCustomerDetailSelectionChangedCommandExecute, OnCustomerDetailSelectionChangedCommandCanExecute);
            ExportLicenseCommand = new RelayCommand<object>(OnExportLicenseCommandExecute, OnExportLicenseCommandCanExecute);
        }

        /// <summary>
        /// Load Data
        /// </summary>
        private void InitData()
        {
            LoadCustomer();
        }

        /// <summary>
        /// Load All Customer
        /// </summary>
        private void LoadCustomer()
        {
            IList<Customer> customerList = EntityDB.GetAll<Customer>();
            CustomerCollection = new CollectionBase<CustomerModel>();
            foreach (Customer customer in customerList)
            {
                CustomerModel customerModel = new CustomerModel(customer);
                //Load CustomerDetailCollection
                customerModel.CustomerDetailCollection = new CollectionBase<CustomerDetailModel>(customer.CustomerDetail.OrderBy(x => x.StoreCode).Select(x => new CustomerDetailModel(x)));
                CustomerCollection.Add(customerModel);
            }
            CreateNewCustomer();
        }

        //Insert New Customer
        private bool SaveNewCustomer(CustomerModel customerModel)
        {
            bool result = false;
            if (customerModel.IsNew)
            {
                try
                {
                    customerModel.DateCreated = DateTime.Now;
                    customerModel.DateUpdated = DateTime.Now;
                    customerModel.ToEntity();
                    //Create CustomerDetail with TotalStore
                    CheckCreateDetail(customerModel);

                    //Map CustomerDetailCollection
                    if (customerModel.CustomerDetailCollection.Any())
                    {
                        foreach (CustomerDetailModel customerDetailModel in customerModel.CustomerDetailCollection)
                        {
                            customerDetailModel.ToEntity();
                            customerModel.Customer.CustomerDetail.Add(customerDetailModel.CustomerDetail);
                        }
                    }


                    EntityDB.Add<Customer>(customerModel.Customer);
                    EntityDB.Commit();

                    MapCustomerNRelation(customerModel);
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    _log4net.Error(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            return result;
        }

        /// <summary>
        /// Update Customer
        /// </summary>
        /// <param name="customerModel"></param>
        private bool UpdateCustomer(CustomerModel customerModel)
        {
            bool result = false;
            if (!customerModel.IsNew)
            {
                try
                {
                    customerModel.DateUpdated = DateTime.Now;
                    customerModel.ToEntity();

                    CheckCreateDetail(customerModel);

                    //Map CustomerDetailCollection
                    if (customerModel.CustomerDetailCollection.Any())
                    {
                        //Remove Item Deleted
                        //foreach (CustomerDetailModel customerDetailModel in customerModel.CustomerDetailCollection.DeletedItems)
                        //{
                        //    EntityDB.Delete<CustomerDetail>(customerDetailModel.CustomerDetail);
                        //    EntityDB.Commit();
                        //}

                        //Add Or Update Item
                        foreach (CustomerDetailModel customerDetailModel in customerModel.CustomerDetailCollection.Where(x => x.IsDirty))
                        {
                            customerDetailModel.ToEntity();
                            if (customerDetailModel.IsNew)
                            {
                                customerModel.Customer.CustomerDetail.Add(customerDetailModel.CustomerDetail);
                            }

                        }
                    }

                    EntityDB.Update<Customer>(customerModel.Customer);
                    EntityDB.Commit();

                    MapCustomerNRelation(customerModel);
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    _log4net.Error(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            return result;
        }

        /// <summary>
        /// Check Total store & Create new CustomerDetail
        /// </summary>
        /// <param name="customerModel"></param>
        private void CheckCreateDetail(CustomerModel customerModel)
        {
            //Create CustomerDetail with TotalStore
            if (customerModel.TotalStore > customerModel.CustomerDetailCollection.Count())
            {
                int numberCustomerAddition = Convert.ToInt32(customerModel.TotalStore) - customerModel.CustomerDetailCollection.Count();
                int j = 0;
                for (int i = 0; i < numberCustomerAddition; i++)
                {
                    CustomerDetailModel customerDetailModel = new CustomerDetailModel();
                    customerDetailModel.POSId = DateTime.Now.AddSeconds(j++).ToString(POS_ID_FORMAT);
                    int? storeCodeMax = SelectedCustomer.CustomerDetailCollection.Max(x => x.StoreCode);
                    customerDetailModel.StoreCode = storeCodeMax.HasValue ? storeCodeMax.Value + 1 : 0;
                    SelectedCustomer.CustomerDetailCollection.Add(customerDetailModel);
                }
            }
            else if (customerModel.TotalStore < customerModel.CustomerDetailCollection.Count())
            {
                MessageBox.Show("Delete License");
            }
        }

        /// <summary>
        /// Save or Update Customer
        /// </summary>
        /// <param name="customerModel"></param>
        /// <returns></returns>
        private bool SaveCustomer(CustomerModel customerModel)
        {
            if (customerModel.IsNew)
            {
                if (SaveNewCustomer(customerModel))
                {
                    CustomerCollection.Add(customerModel);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return UpdateCustomer(customerModel);
        }

        /// <summary>
        /// Set id Customer & Customer Detail
        /// </summary>
        /// <param name="customerModel"></param>
        private void MapCustomerNRelation(CustomerModel customerModel)
        {
            //Map ID
            if (customerModel.IsNew)
                customerModel.Id = customerModel.Customer.Id;
            customerModel.EndUpdate();

            if (customerModel.CustomerDetailCollection.Any())
            {
                foreach (CustomerDetailModel customerDetailModel in customerModel.CustomerDetailCollection.Where(x => x.IsDirty))
                {
                    if (customerDetailModel.IsNew)
                        customerDetailModel.Id = customerDetailModel.CustomerDetail.Id;
                    customerDetailModel.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Create new Item 
        /// </summary>
        private void CreateNewCustomer()
        {
            SelectedCustomer = new CustomerModel();
            SelectedCustomer.TotalStore = 1;
            SelectedCustomer.CustomerDetailCollection = new CollectionBase<CustomerDetailModel>();

            //CustomerDetailModel customerDetailModel = new CustomerDetailModel();
            //customerDetailModel.StoreCode = 0;
            //SelectedCustomer.CustomerDetailCollection.Add(customerDetailModel);

        }

        /// <summary>
        /// Delete Customer
        /// </summary>
        /// <param name="customerModel"></param>
        private bool DeletedCustomer(CustomerModel customerModel)
        {
            bool result = false;
            MessageBoxResult resultMsg = MessageBox.Show("Do you want to delete this item?", "Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (resultMsg.Equals(MessageBoxResult.Yes))
            {
                try
                {
                    EntityDB.Delete<Customer>(customerModel.Customer);
                    EntityDB.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    _log4net.Error(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return result;
        }

        /// <summary>
        /// Export Key to File
        /// </summary>
        /// <param name="customerModel"></param>
        private void ExportLicenseKey(CustomerModel customerModel)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.FileName = "License.txt";
                dlg.Filter = "Text File|*.txt";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StreamWriter sw = new StreamWriter(dlg.FileName, false);
                    sw.WriteLine("====================================================================");
                    sw.WriteLine("Company :" + customerModel.Company);
                    sw.WriteLine("Address :" + customerModel.Address);
                    sw.WriteLine("Total Store :" + customerModel.TotalStore);
                    sw.WriteLine("====================================================================");
                    foreach (CustomerDetailModel customerDetailModel in customerModel.CustomerDetailCollection.Where(x => x.IsGenerated))
                    {
                        StringBuilder sb = new StringBuilder();
                        string storeCode = Convert.ToString(customerDetailModel.StoreCode.Value);
                        string licenseCode = Convert.ToString(customerDetailModel.LicenceCode);
                        string genDate = customerDetailModel.GenDate.Value.ToString("d");
                        string expireDate = customerDetailModel.ExpireDate.Value.ToString();
                        sw.WriteLine("\tStore Code : {0}", storeCode);
                        sw.WriteLine("\tLisence Code :{0}", licenseCode);
                        sw.WriteLine("\tExpire Date :{0}", expireDate);
                        sw.WriteLine();
                    }

                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }



        #endregion

    }


}
