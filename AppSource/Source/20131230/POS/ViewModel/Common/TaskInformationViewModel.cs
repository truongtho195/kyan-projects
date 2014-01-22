using System;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class TaskInformationViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public TaskInformationViewModel(base_ReminderModel reminder)
        {
            _reminder = reminder;
            _reminder.OnDate = _reminder.Time.Date;
            _reminder.OnTime = _reminder.Time;
            Initialize();
        }

        #endregion

        #region Properties

        #region Reminder

        private base_ReminderModel _reminder;
        /// <summary>
        /// Gets or sets reminder.
        /// </summary>
        public base_ReminderModel Reminder
        {
            get
            {
                return _reminder;
            }
            set
            {
                if (_reminder != value)
                {
                    _reminder = value;
                    OnPropertyChanged(() => Reminder);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region OkCommand

        private ICommand _OKCommand;
        public ICommand OkCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new RelayCommand(OkExecute);
                }
                return _OKCommand;
            }
        }

        #endregion

        #endregion

        #region Command Methods

        #region OkExecute

        private void OkExecute()
        {
            Close(true);
        }

        #endregion

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

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_reminder.ResourceAssigned))
                {
                    base_GuestRepository guestRepository = new base_GuestRepository();
                    string employeeMark = MarkType.Employee.ToDescription();
                    Guid assignedGuid = new Guid(_reminder.ResourceAssigned);
                    base_Guest guest = guestRepository.Get(x => !x.IsPurged && x.IsActived && x.Mark == employeeMark && x.Resource == assignedGuid);
                    if (guest != null)
                    {
                        _reminder.AssignedTo = new base_GuestModel(guest).LegalName;
                    }
                }
                else
                {
                    _reminder.AssignedTo = null;
                }


            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

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