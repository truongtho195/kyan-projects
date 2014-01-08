using System;
using System.Linq;
using CPC.Toolkit.Base;
using System.Windows.Input;
using CPC.Toolkit.Command;
using CPC.POS.Model;
using CPC.Helper;
using CPC.POS.Repository;

namespace CPC.POS.ViewModel
{
    class NewTaskViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Contructors

        public NewTaskViewModel(base_ReminderModel reminder)
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

        #region UserList

        private CollectionBase<base_GuestModel> _userList;
        /// <summary>
        /// Gets or sets user list.
        /// </summary>
        public CollectionBase<base_GuestModel> UserList
        {
            get
            {
                return _userList;
            }
            set
            {
                if (_userList != value)
                {
                    _userList = value;
                    OnPropertyChanged(() => UserList);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// Save reminder.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SaveExecute, CanSaveExecute);
                }
                return _saveCommand;
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

        #region SaveExecute

        /// <summary>
        /// Save reminder.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Check whether SaveExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanSaveExecute()
        {
            if (_reminder == null || _reminder.HasError || !_reminder.IsDirty)
            {
                return false;
            }

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

        #region Private Methods

        #region Save

        /// <summary>
        /// Save reminder.
        /// </summary>
        private void Save()
        {
            try
            {
                base_ReminderRepository reminderRepository = new base_ReminderRepository();

                if (_reminder.IsNew)
                {
                    // Insert.
                    _reminder.Time = new DateTime(_reminder.OnDate.Year, _reminder.OnDate.Month, _reminder.OnDate.Day, _reminder.OnTime.Hour, _reminder.OnTime.Minute, 0);
                    _reminder.UserCreated = Define.USER.LoginName;
                    _reminder.UserUpdated = Define.USER.LoginName;
                    _reminder.DateCreated = DateTime.Now;
                    _reminder.DateUpdated = DateTime.Now;
                    _reminder.ToEntity();
                    reminderRepository.Add(_reminder.base_Reminder);
                    reminderRepository.Commit();
                    _reminder.Id = _reminder.base_Reminder.Id;
                    _reminder.IsNew = false;
                    _reminder.IsDirty = false;
                }
                else
                {
                    // Update.
                    _reminder.Time = new DateTime(_reminder.OnDate.Year, _reminder.OnDate.Month, _reminder.OnDate.Day, _reminder.OnTime.Hour, _reminder.OnTime.Minute, 0);
                    _reminder.UserUpdated = Define.USER.LoginName;
                    _reminder.DateUpdated = DateTime.Now;
                    _reminder.ToEntity();
                    reminderRepository.Commit();
                    _reminder.IsDirty = false;
                }

                Close(true);
            }
            catch (Exception exception)
            {
                WriteLog(exception);
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            //Restore reminder.
            if (_reminder.IsDirty)
            {
                _reminder.ToModelAndRaise();
            }

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

        #region Initialize

        /// <summary>
        /// Initialize data.
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Gets user list.
                base_GuestRepository guestRepository = new base_GuestRepository();
                string employeeMark = MarkType.Employee.ToDescription();
                UserList = new CollectionBase<base_GuestModel>(guestRepository.GetAll(x =>
                    !x.IsPurged && x.IsActived && x.Mark == employeeMark).Select(x => new base_GuestModel(x, false)));
                // Inserts empty user.
                UserList.Insert(0, new base_GuestModel());
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
