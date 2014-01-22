using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using SecurityLib;

namespace CPC.POS.ViewModel
{
    class RewardCutOffServiceViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Gets data on a separate thread.
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        #endregion

        #region Constructors

        public RewardCutOffServiceViewModel(base_RewardManagerModel rewardManager, CollectionBase<base_GuestModel> rewardCustomerList)
        {
            _ownerViewModel = App.Current.MainWindow.DataContext;
            _backgroundWorker.DoWork += new DoWorkEventHandler(Send);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SendCompleted);

            RewardManager = rewardManager;
            RewardCustomerList = new CollectionBase<base_GuestModel>(rewardCustomerList);
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

        #region RewardManager

        private base_RewardManagerModel _rewardManager;
        public base_RewardManagerModel RewardManager
        {
            get
            {
                return _rewardManager;
            }
            set
            {
                if (_rewardManager != value)
                {
                    _rewardManager = value;
                    OnPropertyChanged(() => RewardManager);
                }
            }
        }

        #endregion

        #region RewardCustomerList

        private CollectionBase<base_GuestModel> _rewardCustomerList;
        public CollectionBase<base_GuestModel> RewardCustomerList
        {
            get
            {
                return _rewardCustomerList;
            }
            set
            {
                if (_rewardCustomerList != value)
                {
                    _rewardCustomerList = value;
                    OnPropertyChanged(() => RewardCustomerList);
                }
            }
        }

        #endregion

        #region ActiveDate

        public DateTime ActiveDate
        {
            get
            {
                return DateTime.Now.Date;
            }
        }

        #endregion

        #region ExpireDay

        public int ExpireDay
        {
            get
            {
                return _rewardManager.RewardExpiration + _rewardManager.RedemptionAfterDays;
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SendCommand

        private ICommand _sendCommand;
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
        /// Send.
        /// </summary>
        private void SendExecute()
        {
            _backgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region CanSendExecute

        /// <summary>
        /// Determine whether can call SendExecute method.
        /// </summary>
        /// <returns>True will call. Otherwise False.</returns>
        private bool CanSendExecute()
        {
            return true;
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

        #endregion

        #region Private Methods

        #region Close

        /// <summary>
        /// Close popup.
        /// </summary>
        private void Close(bool result)
        {
            FindOwnerWindow(this).DialogResult = result;
        }

        #endregion

        #region Send

        /// <summary>
        /// Send.
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
            if (!File.Exists(Define.RewardContentTemplateFile))
            {
                throw new Exception("Reward content template file not found.");
            }

            // Init SMTP.
            string pass = AESSecurity.Decrypt(Define.CONFIGURATION.base_Configuration.EmailPassword);
            SmtpClient smtpClient = new SmtpClient(Define.CONFIGURATION.EmailPop3Server, Define.CONFIGURATION.EmailPop3Port.Value);
            smtpClient.Credentials = new NetworkCredential(Define.CONFIGURATION.EmailAccount, pass);

            // Gets reward customer.
            base_GuestRewardSaleOrderRepository guestRewardSaleOrderRepository = new base_GuestRewardSaleOrderRepository();
            base_SaleOrderRepository saleOrderRepository = new base_SaleOrderRepository();
            base_GuestRewardRepository guestRewardRepository = new base_GuestRewardRepository();

            // Gets base_GuestRewardSaleOrder list.
            List<base_GuestRewardSaleOrderModel> guestRewardSaleOrderList = new List<base_GuestRewardSaleOrderModel>(guestRewardSaleOrderRepository.GetAll(
                x => x.GuestRewardId == 0).Select(x => new base_GuestRewardSaleOrderModel(x)));
            // Gets addition GuestId and CashReward information.
            base_SaleOrder saleOrder;
            Guid SOResource;
            base_GuestModel guest;
            foreach (base_GuestRewardSaleOrderModel item in guestRewardSaleOrderList)
            {
                if (!string.IsNullOrWhiteSpace(item.SaleOrderResource))
                {
                    SOResource = new Guid(item.SaleOrderResource);
                    saleOrder = saleOrderRepository.Get(x => x.Resource == SOResource);
                    if (saleOrder != null)
                    {
                        item.GuestResource = new Guid(saleOrder.CustomerResource);
                    }
                    guest = _rewardCustomerList.FirstOrDefault(x => x.Resource == item.GuestResource);
                    if (guest == null)
                    {
                        item.IsDeleted = true;
                    }
                    else
                    {
                        item.IsDeleted = false;
                        item.CashReward = guest.CashReward;
                    }
                }
                else
                {
                    item.IsDeleted = true;
                }
            }
            guestRewardSaleOrderList = new List<base_GuestRewardSaleOrderModel>(guestRewardSaleOrderList.Where(x => !x.IsDeleted));

            // Create new GuestRewards.
            List<base_GuestRewardModel> guestRewardList = guestRewardRepository.CreateNewGuestReward(guestRewardSaleOrderList);

            // Send email message.
            int count = 0;
            MailMessage mailMessage;
            AlternateView alternateView;
            LinkedResource linkedResource;
            string fileName;
            foreach (base_GuestRewardModel guestReward in guestRewardList)
            {
                // Create an image file.
                fileName = Path.Combine(Path.GetTempPath(), string.Format("{0}.jpg", DateTime.Now.ToString("ddMMyyhhmmssffff")));
                BinaryWriter fs = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write));
                fs.Write(guestReward.ScanCodeImg);
                fs.Close();

                guest = _rewardCustomerList.FirstOrDefault(x => x.Id == guestReward.GuestId);
                if (guest != null)
                {
                    // Creates Content
                    string content = File.ReadAllText(Define.RewardContentTemplateFile);
                    content = content.Replace("@barcode", guestReward.ScanCode);
                    content = content.Replace("@rewardAmount", string.Format("{0}{1}", guest.CashReward.ToString(), Define.CurrencySymbol));
                    content = content.Replace("@expDate", ActiveDate.AddDays(ExpireDay).ToString("d"));
                    content = content.Replace("@activeDate", ActiveDate.ToString("d"));
                    alternateView = AlternateView.CreateAlternateViewFromString(content, new System.Net.Mime.ContentType("text/html"));
                    linkedResource = new LinkedResource(fileName);
                    linkedResource.ContentId = "imageBarcode";
                    alternateView.LinkedResources.Add(linkedResource);

                    mailMessage = new MailMessage();
                    mailMessage.Subject = "Reward";
                    mailMessage.AlternateViews.Add(alternateView);
                    mailMessage.From = new MailAddress(Define.CONFIGURATION.EmailAccount);
                    mailMessage.To.Add(guest.Email);
                    // Send 5 messages per one time. Then sleep 2s.
                    if (count > 0 && count % 5 == 0)
                    {
                        Thread.Sleep(2000);
                    }
                    smtpClient.Send(mailMessage);
                    count++;
                }
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            Close(false);
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