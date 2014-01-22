using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.Helper;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    class TaskListReminderViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Original Collection.
        /// </summary>
        private CollectionBase<base_ReminderModel> _reminderList;

        #endregion

        #region Constructors

        public TaskListReminderViewModel(CollectionBase<base_ReminderModel> reminderList)
        {
            _reminderList = reminderList;
            AlarmList = new CollectionBase<base_ReminderModel>(_reminderList.Where(x => !x.IsCompleted && x.IsActived));

            IncludePropetyChanged();
        }

        #endregion

        #region Properties

        #region AlarmList

        private CollectionBase<base_ReminderModel> _alarmList;
        /// <summary>
        /// Gets AlarmList.
        /// </summary>
        public CollectionBase<base_ReminderModel> AlarmList
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

        #endregion

        #region Command Properties

        #region DeleteCommand

        private ICommand _deleteCommand;
        /// <summary>
        /// Delete reminder.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(DeleteExecute, CanDeleteExecute);
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region CompleteCommand

        private ICommand _completeCommand;
        /// <summary>
        /// Complete reminder.
        /// </summary>
        public ICommand CompleteCommand
        {
            get
            {
                if (_completeCommand == null)
                {
                    _completeCommand = new RelayCommand(CompleteExecute, CanCompleteExecute);
                }
                return _completeCommand;
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

        #region DeleteExecute

        /// <summary>
        /// Delete reminder.
        /// </summary>
        private void DeleteExecute()
        {
            Delete();
        }

        #endregion

        #region CanDeleteExecute

        /// <summary>
        /// Check whether DeleteExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteExecute()
        {
            if (_alarmList == null || !_alarmList.Any(x => x.IsChecked))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CompleteExecute

        /// <summary>
        /// Complete reminder.
        /// </summary>
        private void CompleteExecute()
        {
            Complete();
        }

        #endregion

        #region CanCompleteExecute

        /// <summary>
        /// Check whether CompleteExecute method can execute.
        /// </summary>
        /// <returns></returns>
        private bool CanCompleteExecute()
        {
            if (_alarmList == null || !_alarmList.Any(x => x.IsChecked))
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

        #region Property Changed Methods

        #region OnIsCheckedAllChanged

        /// <summary>
        /// Occurs when IsCheckedAll property changed.
        /// </summary>
        private void OnIsCheckedAllChanged()
        {
            if (_alarmList.Any() && _isCheckedAll.HasValue)
            {
                foreach (base_ReminderModel item in _alarmList)
                {
                    item.PropertyChanged -= ReminderPropertyChanged;
                    item.IsChecked = _isCheckedAll.Value;
                    item.PropertyChanged += ReminderPropertyChanged;
                }
            }
        }

        #endregion

        #region ReminderPropertyChanged

        private void ReminderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                VerifyingIsCheckedAll();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Delete

        /// <summary>
        /// Delete reminder.
        /// </summary>
        private void Delete()
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text4, Language.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    base_ReminderRepository reminderRepository = new base_ReminderRepository();
                    List<base_ReminderModel> deletedList = _alarmList.Where(x => x.IsChecked).ToList();
                    foreach (base_ReminderModel item in deletedList)
                    {
                        item.IsActived = false;
                        reminderRepository.Delete(item.base_Reminder);
                        reminderRepository.Commit();
                        item.IsDirty = false;
                        item.PropertyChanged -= ReminderPropertyChanged;
                        _alarmList.Remove(item);
                        _reminderList.Remove(item);
                    }

                    ExcludePropetyChanged();
                    Close(true);
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, Language.Warning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Complete

        /// <summary>
        /// Complete reminder.
        /// </summary>
        private void Complete()
        {
            try
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Language.Text41, Language.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    base_ReminderRepository reminderRepository = new base_ReminderRepository();
                    List<base_ReminderModel> completedList = _alarmList.Where(x => x.IsChecked).ToList();
                    foreach (base_ReminderModel item in completedList)
                    {

                        item.IsCompleted = true;
                        item.IsActived = false;
                        item.UserUpdated = Define.USER.LoginName;
                        item.DateUpdated = DateTime.Now;
                        item.ToEntity();
                        reminderRepository.Commit();
                        item.IsDirty = false;
                        item.PropertyChanged -= ReminderPropertyChanged;
                        _alarmList.Remove(item);
                    }

                    ExcludePropetyChanged();
                    Close(true);
                }
            }
            catch (Exception exception)
            {
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
                foreach (base_ReminderModel item in _alarmList)
                {
                    item.IsChecked = false;
                    item.PropertyChanged += ReminderPropertyChanged;
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
                foreach (base_ReminderModel item in _alarmList)
                {
                    item.PropertyChanged -= ReminderPropertyChanged;
                }
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