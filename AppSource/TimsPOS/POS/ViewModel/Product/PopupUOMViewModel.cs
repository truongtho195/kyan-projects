using System;
using System.Windows;
using System.Windows.Input;
using CPC.POS.Model;
using CPC.POS.Repository;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;

namespace CPC.POS.ViewModel
{
    public class PopupUOMViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Contructors

        public PopupUOMViewModel()
        {
            _ownerViewModel = this;
            NewItem = new base_UOMModel();
        }

        #endregion

        #region Properties

        #region NewItem

        private base_UOMModel _newItem;
        /// <summary>
        /// Gets a new item if exists.
        /// </summary>
        public base_UOMModel NewItem
        {
            get
            {
                return _newItem;
            }
            private set
            {
                if (_newItem != value)
                {
                    _newItem = value;
                    OnPropertyChanged(() => NewItem);
                }
            }
        }

        #endregion

        #endregion

        #region Command Properties

        #region SaveCommand

        private ICommand _saveCommand;
        /// <summary>
        /// When 'Save' button clicked, command will executes.
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
        /// When 'Cancel' button clicked, command will executes.
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
        /// Save.
        /// </summary>
        private void SaveExecute()
        {
            Save();
        }

        #endregion

        #region CanSaveExecute

        /// <summary>
        /// Determine SaveExecute method can execute or not.
        /// </summary>
        /// <returns>True is execute.</returns>
        private bool CanSaveExecute()
        {
            if (_newItem == null || _newItem.HasError)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region CancelExecute

        private void CancelExecute()
        {
            Cancel();
        }

        #endregion

        #endregion

        #region Private Methods

        #region Save

        /// <summary>
        /// Save department, category, brand.
        /// </summary>
        private void Save()
        {
            try
            {
                base_UOMRepository UOMRepository = new base_UOMRepository();
                DateTime now = DateTime.Now;

                if (CheckDupCodeUOM(_newItem))
                {
                    throw new Exception(string.Format("{0} is Duplication. Please try another code.", _newItem.Code));
                }
                else
                {
                    _newItem.IsActived = true;
                    _newItem.DateCreated = now;
                    if (Define.USER != null)
                        _newItem.UserCreated = Define.USER.LoginName;
                    _newItem.ToEntity();
                    UOMRepository.Add(_newItem.base_UOM);
                    UOMRepository.Commit();
                    _newItem.Id = _newItem.base_UOM.Id;
                    _newItem.IsNew = false;
                    _newItem.IsDirty = false;
                }

                FindOwnerWindow(this).DialogResult = true;
            }
            catch (Exception exception)
            {
                _log4net.Error(string.Format("Message: {0}. Source: {1}", exception.Message, exception.Source));
                MessageBox.Show(exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            NewItem = null;
            FindOwnerWindow(this).DialogResult = false;
        }

        #endregion

        #region CheckDupCodeUOM

        /// <summary>
        /// Check duplicate UOM's code.
        /// </summary>
        /// <param name="UOM">base_UOMModel to check duplicate.</param>
        /// <returns>True is duplicate.</returns>
        private bool CheckDupCodeUOM(base_UOMModel UOM)
        {
            bool isDuplicate = false;

            try
            {
                base_UOMRepository UOMRepository = new base_UOMRepository();
                isDuplicate = UOMRepository.Get(x => x.Id != UOM.Id && x.Code.Trim().ToLower() == UOM.Code.Trim().ToLower()) != null;
            }
            catch
            {
                throw;
            }

            return isDuplicate;
        }

        #endregion

        #endregion
    }
}
