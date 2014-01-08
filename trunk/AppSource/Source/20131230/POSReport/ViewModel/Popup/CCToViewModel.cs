using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.POSReport.View;
using System.Windows.Input;
using Toolkit.Command;
using Xceed.Wpf.Toolkit;
using Toolkit.Base;
using System.Collections.ObjectModel;
using CPC.POSReport.Model;
using CPC.POSReport.Function;
using System.ComponentModel;

namespace CPC.POSReport.ViewModel
{
    class CCToViewModel : ViewModelBase
    {
        #region -Property- 
        private ObservableCollection<EmailModel> _customerEmailCollection;

        public ObservableCollection<EmailModel> CustomerEmailCollection
        {
            get { return _customerEmailCollection; }
            set
            {
                if (_customerEmailCollection != value)
                {
                    _customerEmailCollection = value;
                    OnPropertyChanged(() => CustomerEmailCollection);
                }
            }
        }

        private ObservableCollection<EmailModel> _addressList;

        public ObservableCollection<EmailModel> EmailAddressList
        {
            get { return _addressList; }
            set 
            {
                if (_addressList != value)
                {
                    _addressList = value;
                    OnPropertyChanged(() => EmailAddressList);
                }
            }
        }

        private EmailModel _addressModel;

        public EmailModel AddressModel
        {
            get { return _addressModel; }
            set
            {
                if (_addressModel != value)
                {
                    _addressModel = value;
                    OnPropertyChanged(() => AddressModel);
                }
            }
        }

        private string _newAdd;

        public string NewAdd
        {
            get { return _newAdd; }
            set
            {
                if (_newAdd != value)
                {
                    _newAdd = value;
                    OnPropertyChanged(() => NewAdd);
                }
            }
        }

        //private bool _checkAll;

        //public bool CheckAll
        //{
        //    get { return _checkAll; }
        //    set
        //    {
        //        if (_checkAll != value)
        //        {
        //            _checkAll = value;
        //            OnPropertyChanged(() => CheckAll);
        //        }
        //    }
        //}

        private bool _isShowAutoComplete;

        public bool IsShowAutoComplete
        {
            get { return _isShowAutoComplete; }
            set
            {
                if (_isShowAutoComplete != value)
                {
                    _isShowAutoComplete = value;
                    OnPropertyChanged(() => IsShowAutoComplete);
                }
            }
        }
        #endregion 

        #region -Defines-
        public CCToView cCToView { get; set; }
        Repository.base_GuestRepository guestRepo = new Repository.base_GuestRepository();
        public MainViewModel  MainViewModel  { get; set; }
        List<EmailModel> lstEmailModel;
        #endregion

        #region -Contructor-

        public CCToViewModel(CCToView ccView, string CCReport, MainViewModel mainViewModel)
        {
            try
            {
                InitCommand();
                cCToView = ccView;
                ConvertStringToCollection(CCReport);
                CheckIsSendToAll();
                IsShowAutoComplete = false;
                GetAllCustomerEmail();
                MainViewModel = mainViewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #region -Command-

        #region -Init command-
        /// <summary>
        /// Init all command
        /// </summary>
        private void InitCommand()
        {
            LookupCustomerEmailCommand = new RelayCommand(LookupCustomerEmailExecute, CanLookupCustomerEmailExecute);
            AddCommand = new RelayCommand(AddExecute, CanAddExecute);
            DeleteCCCommand = new RelayCommand(DeleteCCExecute, CanDeleteCCExecute);
            CCToOKCommand = new RelayCommand(CCToOKExecute);
            CloseCommand = new RelayCommand(CloseExecute);
            CheckAllEmailCommand = new RelayCommand<object>(CheckAllEmailExecute, CanCheckAllEmailExecute);
            CheckEmailCommand = new RelayCommand<object>(CheckEmailExecute, CanCheckEmailExecute);
        }
        #endregion

        #region -Lookup Customer Email-
        public RelayCommand LookupCustomerEmailCommand { get; set; }

        public void LookupCustomerEmailExecute()
        {                         
            CustomerEmailCollection = new ObservableCollection<EmailModel>(
                    lstEmailModel.Where(w => w.Address.Contains(NewAdd))
                );
            int customerEmailCount = CustomerEmailCollection.Count;            
            if (string.IsNullOrEmpty(NewAdd.Trim()) || customerEmailCount == 0)
            {
                IsShowAutoComplete = false;
            }
            else if (customerEmailCount > 0)
            {
                IsShowAutoComplete = true;
            }
        }

        public bool CanLookupCustomerEmailExecute()
        {
            return (lstEmailModel != null && lstEmailModel.Count() > 0);
        }
        #endregion

        #region - Add Command-
        /// <summary>
        /// Set or get Add Command
        /// </summary>
        public ICommand AddCommand { get; private set; }

        private void AddExecute()
        {
            IsShowAutoComplete = false;
            if (ValidateEmail())
            {                
                EmailModel add = new EmailModel(true, NewAdd);
                // Check is email exist in email address list
                var email = EmailAddressList.FirstOrDefault(w => w.Address == add.Address);
                if (email == null)
                {
                    // Add new email address
                    EmailAddressList.Add(add);
                } 
            }
            else
            {
                int customerEmailCount = CustomerEmailCollection.Count;
                for (int i = 0; i < customerEmailCount; i++)
                {
                    if (!CustomerEmailCollection[i].IsSend)
                    {
                        continue;
                    }
                    var email = EmailAddressList.FirstOrDefault(w => w.Address == CustomerEmailCollection[i].Address);
                    if (email == null)
                    {
                        // Add new email address
                        EmailAddressList.Add(CustomerEmailCollection[i]);
                    }
                }
            }
            NewAdd = string.Empty;
        }
        private bool CanAddExecute()
        {
            return (ValidateEmail() || CheckCustomerList());
        }

        private bool CheckCustomerList()
        {
            if (CustomerEmailCollection != null && CustomerEmailCollection.Count > 0)
            { 
                int customerEmailCount = CustomerEmailCollection.Count;
                for (int i = 0; i < customerEmailCount; i++)
                {
                    if (CustomerEmailCollection[i].IsSend)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region -Delete Email-
        public RelayCommand DeleteCCCommand { get; set; }

        public void DeleteCCExecute()
        {
            System.Windows.MessageBoxResult resuilt = MessageBox.Show("Do you want to delete?", "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (resuilt.Equals(System.Windows.MessageBoxResult.Yes))
            {
                EmailAddressList.Remove(AddressModel);
                AddressModel = null;
            }
        }

        public bool CanDeleteCCExecute()
        {
            return (AddressModel != null);
        }
        #endregion

        #region -CCToOKCommand-
        /// <summary>
        /// Set or get CCToOKCommand
        /// </summary>
        public ICommand CCToOKCommand { get; private set; }

        private void CCToOKExecute()
        {
            MainViewModel.UpdateCCReport(ConvertCollectionToString());
            cCToView.Close();
        }
        #endregion

        #region -CloseCommand-
        /// <summary>
        /// Set or get CCToCancelCommand
        /// </summary>
        public ICommand CloseCommand { get; private set; }

        private void CloseExecute()
        {
            cCToView.Close();
        }

        #endregion
        
        #endregion 

        #region -Private method-

        #region -Check Is Send To All-
        private void CheckIsSendToAll()
        {
            //if (EmailAddressList.Count == 0)
            //{
            //    CheckAll = false;
            //    return;
            //}
            //CheckAll = true;
            //foreach (EmailModel email in EmailAddressList)
            //{
            //    if (email.IsSend != CheckAll)
            //    {
            //        CheckAll = false;
            //        break;
            //    }
            //}
        }
        #endregion

        #region -Convert String To Collection-
        /// <summary>
        /// Convert String To Collection
        /// </summary>
        /// <param name="emailList">string emails list</param>
        private void ConvertStringToCollection(string emailList)
        {
            EmailAddressList = new ObservableCollection<EmailModel>();
            if (!string.IsNullOrWhiteSpace(emailList))
            {
                string[] emails = emailList.Split(';');
                if (emails.Count() > 0)
                {
                    for (int i = 0; i < emails.Count(); i++)
                    {
                        EmailModel add = new EmailModel(true,emails[i]);
                        EmailAddressList.Add(add);
                    }
                }
            }
        }
        #endregion

        #region -Convert Collection To String -
        /// <summary>
        /// Convert Collection To String
        /// </summary>
        /// <returns>string emails list</returns>
        private string ConvertCollectionToString()
        {
            string email = string.Empty;
            StringBuilder emails = new StringBuilder();
            int count = EmailAddressList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (EmailAddressList[i].IsSend)
                    {
                        emails.Append(EmailAddressList[i].Address + ";");
                    }
                }
                if (emails.Length > 2)
                {
                    email = emails.Remove(emails.Length - 1, 1).ToString();
                }
            }
            return email;
        }
        #endregion

        #region -Get all customer email-
        /// <summary>
        /// Get all customer email
        /// </summary>
        private void GetAllCustomerEmail()
        {            
            if (CustomerEmailCollection == null || CustomerEmailCollection.Count == 0)
            {
                var customerList = new List<Model.base_GuestModel>(
                        guestRepo.GetAll()
                        .Select(g => new Model.base_GuestModel(g))
                        .Where(w => w.Mark == "C" && w.Email != "" && w.Email != null)
                        .OrderBy(o => o.Email)
                    );
                lstEmailModel = new List<EmailModel>();
                int count = customerList.Count;
                if (customerList.Count > 0)
                {                    
                    for (int i = 0; i < count; i++)
                    {
                        EmailModel email = new EmailModel(false, customerList[i].Email);
                        lstEmailModel.Add(email);
                    }
                }
                foreach (var item in EmailAddressList)
                {
                    // Check is exist email address
                    var check = lstEmailModel.Find(w => w.Address == item.Address);
                    if (check != null)
                    {                       
                        lstEmailModel.Remove(check);                        
                    }
                    lstEmailModel.Add(item);
                }
            }
        }
        #endregion
        
        #region -Check valid email- 
        /// <summary>
        /// Check valid email
        /// </summary>
        /// <returns></returns>
        public bool ValidateEmail()
        {
            System.Text.RegularExpressions.Regex regex;
            if (!string.IsNullOrEmpty(NewAdd) && !NewAdd.Contains(' '))
            {                            
                regex = new System.Text.RegularExpressions.Regex(Common.EMAIL_FORMAT, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return regex.IsMatch(NewAdd);
            }
            return false;
        }
        #endregion

        #region -Check command-
        public void CheckEmailExecute(object obj)
        {
            //System.Windows.Controls.CheckBox chk = obj as System.Windows.Controls.CheckBox;
            //CheckAll = true;
            //foreach (EmailModel email in EmailAddressList)
            //{
            //    if (email.IsSend != CheckAll)
            //    {
            //        CheckAll = false;
            //        break;
            //    }
            //}
        }

        public bool CanCheckEmailExecute(object obj)
        {
            return (EmailAddressList.Count > 0 && obj != null);
        }

        public RelayCommand<object> CheckEmailCommand { get; set; }
        #endregion

        #region -Check all command-
        public void CheckAllEmailExecute(object obj)
        {
            //System.Windows.Controls.CheckBox chk = obj as System.Windows.Controls.CheckBox;
            //if (chk.IsChecked.HasValue)
            //{
            //    CheckAll = chk.IsChecked.Value;
            //    foreach (EmailModel email in EmailAddressList)
            //    {
            //        if (email.IsSend != CheckAll)
            //        {
            //            email.IsSend = CheckAll;
            //        }
            //    }
            //}
        }

        public bool CanCheckAllEmailExecute(object obj)
        {
            return (EmailAddressList.Count > 0 && obj != null);
        }

        public RelayCommand<object> CheckAllEmailCommand { get; set; }
        #endregion

        #endregion        
    }
}
