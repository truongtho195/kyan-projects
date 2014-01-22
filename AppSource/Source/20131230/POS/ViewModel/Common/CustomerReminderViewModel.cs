using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    class CustomerReminderViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Original Collection.
        /// </summary>
        private CollectionBase<base_CustomerReminderModel> _customerReminderList;

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        #endregion

        #region Constructors

        public CustomerReminderViewModel(CollectionBase<base_CustomerReminderModel> customerReminderList)
        {
            _customerReminderList = customerReminderList;
            _backgroundWorker.DoWork += new DoWorkEventHandler(Send);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SendCompleted);

            AlarmList = new CollectionBase<base_CustomerReminderModel>(customerReminderList);
            IncludePropetyChanged();
        }

        #endregion

        #region Properties

        #region IsBusy

        private bool _isBusy;
        /// <summary>
        /// Gets or sets IsBusy.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(() => IsBusy);
                }
            }
        }

        #endregion

        #region AlarmList

        private CollectionBase<base_CustomerReminderModel> _alarmList;
        /// <summary>
        /// Gets work version AlarmList.
        /// </summary>
        public CollectionBase<base_CustomerReminderModel> AlarmList
        {
            get
            {
                return _alarmList;
            }
            private set
            {
                if (_alarmList != value)
                {
                    _alarmList = value;
                    OnPropertyChanged(() => AlarmList);
                }
            }
        }

        #endregion

        #region IsCheckedAll

        private bool? _isCheckedAll = false;
        /// <summary>
        /// Gets or sets IsCheckedAll.
        /// </summary>
        public bool? IsCheckedAll
        {
            get
            {
                return _isCheckedAll;
            }
            set
            {
                if (_isCheckedAll != value)
                {
                    _isCheckedAll = value;
                    OnPropertyChanged(() => IsCheckedAll);
                    OnIsCheckedAllChanged();
                }
            }
        }

        #endregion

        #region ImageFile

        private string _imageFile;
        /// <summary>
        /// Gets or sets ImageFile.
        /// </summary>
        public string ImageFile
        {
            get
            {
                return _imageFile;
            }
            set
            {
                if (_imageFile != value)
                {
                    _imageFile = value;
                    OnPropertyChanged(() => ImageFile);
                }
            }
        }

        #endregion

        #region ImageVisibility

        private Visibility _imageVisibility = Visibility.Collapsed;
        /// <summary>
        /// Gets or sets ImageVisibility.
        /// </summary>
        public Visibility ImageVisibility
        {
            get
            {
                return _imageVisibility;
            }
            set
            {
                if (_imageVisibility != value)
                {
                    _imageVisibility = value;
                    OnPropertyChanged(() => ImageVisibility);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SendCommand

        private ICommand _sendCommand;
        /// <summary>
        /// Send email message.
        /// </summary>
        public ICommand SendCommand
        {
            get
            {
                if (_sendCommand == null)
                {
                    _sendCommand = new RelayCommand(SendExecute, CanSendExecute);
                }
                return _sendCommand;
            }
        }

        #endregion

        #region ChooseTemplateCommand

        private ICommand _chooseTemplateCommand;
        /// <summary>
        /// Chooses template to send email.
        /// </summary>
        public ICommand ChooseTemplateCommand
        {
            get
            {
                if (_chooseTemplateCommand == null)
                {
                    _chooseTemplateCommand = new RelayCommand(ChooseTemplateExecute, CanChooseTemplateExecute);
                }
                return _chooseTemplateCommand;
            }
        }

        #endregion

        #region CancelCommand

        private ICommand _cancelCommand;
        /// <summary>
        /// Cancel.
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelExecute);
                }
                return _cancelCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region SendExecute

        /// <summary>
        /// Send email message.
        /// </summary>
        private void SendExecute()
        {
            _backgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region CanSendExecute

        /// <summary>
        /// Check whether SendExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanSendExecute()
        {
            return _alarmList.Any(x => x.IsChecked) && !string.IsNullOrWhiteSpace(_imageFile);
        }

        #endregion

        #region ChooseTemplateExecute

        /// <summary>
        /// Choose image template.
        /// </summary>
        private void ChooseTemplateExecute()
        {
            ChooseTemplate();
        }

        #endregion

        #region CanChooseTemplateExecute

        /// <summary>
        /// Check whether ChooseTemplateExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanChooseTemplateExecute()
        {
            return _alarmList.Any();
        }

        #endregion

        #region CancelExecute

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Property Changed Methods

        #region OnIsCheckedAllChanged

        /// <summary>
        /// Occurs when IsCheckedAll property changed.
        /// </summary>
        private void OnIsCheckedAllChanged()
        {
            if (_alarmList.Any() && _isCheckedAll.HasValue)
            {
                foreach (base_CustomerReminderModel item in _alarmList)
                {
                    item.PropertyChanged -= GuestPropertyChanged;
                    item.IsChecked = _isCheckedAll.Value;
                    item.PropertyChanged += GuestPropertyChanged;
                }
            }
        }

        #endregion

        #region GuestPropertyChanged

        private void GuestPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                VerifyingIsCheckedAll();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Send

        /// <summary>
        /// Send email message.
        /// </summary>
        private void Send()
        {
            // Checks SMTP information.
            if (string.IsNullOrWhiteSpace(Define.CONFIGURATION.EmailAccount) ||
                string.IsNullOrWhiteSpace(Define.CONFIGURATION.base_Configuration.EmailPassword) ||
                string.IsNullOrWhiteSpace(Define.CONFIGURATION.EmailPop3Server) ||
                 !Define.CONFIGURATION.EmailPop3Port.HasValue)
            {
                throw new Exception("Please config full email account and SMTP server information.");
            }

            // Checks content to send.
            if (string.IsNullOrWhiteSpace(Define.ContentHappyBirthdayFile))
            {
                throw new Exception("Content happy birthday file not found.");
            }

            // Init SMTP.
            string pass = AESSecurity.Decrypt(Define.CONFIGURATION.base_Configuration.EmailPassword);
            SmtpClient smtpClient = new SmtpClient(Define.CONFIGURATION.EmailPop3Server, Define.CONFIGURATION.EmailPop3Port.Value);
            smtpClient.Credentials = new NetworkCredential(Define.CONFIGURATION.EmailAccount, pass);

            // Creates Content
            AlternateView alternateView = new AlternateView(Define.ContentHappyBirthdayFile, "text/html");

            LinkedResource linkedResource = new LinkedResource(_imageFile);
            linkedResource.ContentId = "imageBirthday";
            alternateView.LinkedResources.Add(linkedResource);

            // Send email message.
            int count = 0;
            MailMessage mailMessage;
            foreach (base_CustomerReminderModel reminder in _alarmList)
            {
                if (reminder.IsChecked)
                {
                    mailMessage = new MailMessage();
                    mailMessage.Subject = "Happy birthday";
                    mailMessage.AlternateViews.Add(alternateView);
                    mailMessage.From = new MailAddress(Define.CONFIGURATION.EmailAccount);
                    mailMessage.To.Add(reminder.Email);
                    // Send 5 messages per one time. Then sleep 2s.
                    if (count > 0 && count % 5 == 0)
                    {
                        Thread.Sleep(2000);
                    }
                    smtpClient.Send(mailMessage);
                    UpdateCustomerReminder(reminder);
                    count++;
                }
            }

            ExcludePropetyChanged();
        }

        #endregion

        #region ChooseTemplate

        /// <summary>
        /// Choose image template.
        /// </summary>
        private void ChooseTemplate()
        {
            TemplateCategoryViewModel templateCategoryViewModel = new TemplateCategoryViewModel();
            bool? result = _dialogService.ShowDialog<TemplateCategoryView>(App.Current.MainWindow.DataContext, templateCategoryViewModel, "Choose Image");
            if (result == true)
            {
                ImageFile = templateCategoryViewModel.SelectedImage;
                ImageVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            ExcludePropetyChanged();
            Close(false);
        }

        #endregion

        #region Close

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Close(bool result)
        {
            FindOwnerWindow(this).DialogResult = result;
        }

        #endregion

        #region VerifyingIsCheckedAll

        /// <summary>
        /// Verifying IsCheckedAll property's value.
        /// </summary>
        private void VerifyingIsCheckedAll()
        {
            int totalChecked = _alarmList.Count(x => x.IsChecked);
            if (totalChecked == 0)
            {
                _isCheckedAll = false;
                OnPropertyChanged(() => IsCheckedAll);
            }
            else if (totalChecked < _alarmList.Count)
            {
                _isCheckedAll = null;
                OnPropertyChanged(() => IsCheckedAll);
            }
            else
            {
                _isCheckedAll = true;
                OnPropertyChanged(() => IsCheckedAll);
            }
        }

        #endregion

        #region IncludePropetyChanged

        /// <summary>
        /// Include PropetyChanged event of item in alarm list.  
        /// </summary>
        private void IncludePropetyChanged()
        {
            if (_alarmList.Any())
            {
                foreach (base_CustomerReminderModel item in _alarmList)
                {
                    item.IsChecked = false;
                    item.PropertyChanged += GuestPropertyChanged;
                }
            }
        }

        #endregion

        #region ExcludePropetyChanged

        /// <summary>
        /// Exclude PropetyChanged event of item in alarm list.  
        /// </summary>
        private void ExcludePropetyChanged()
        {
            if (_alarmList.Any())
            {
                foreach (base_CustomerReminderModel item in _alarmList)
                {
                    item.PropertyChanged -= GuestPropertyChanged;
                }
            }
        }

        #endregion

        #region UpdateCustomerReminder

        /// <summary>
        /// Update reminder.
        /// </summary>
        private void UpdateCustomerReminder(base_CustomerReminderModel reminder)
        {
            try
            {
                base_CustomerReminderRepository customerReminderRepository = new base_CustomerReminderRepository();
                reminder.IsSend = true;
                reminder.DateSend = DateTime.Now;
                reminder.ToEntity();
                customerReminderRepository.Commit();
                //_alarmList.Remove(reminder);
                _customerReminderList.Remove(reminder);
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #endregion

        #region Events

        private void Send(object sender, DoWorkEventArgs e)
        {
            try
            {
                IsBusy = true;
                Send();
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void SendCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            Close(true);
        }

        #endregion

        #region WriteLog

        private void WriteLog(Exception exception)
        {
            _log4net.Error(string.Format("Message: {0}. Source: {1}.", exception.Message, exception.Source));
            if (exception.InnerException != null)
            {
                _log4net.Error(exception.InnerException.ToString());
            }
        }

        #endregion
    }
}