using System;
using System.Linq;
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
        #region Defines

        private base_UOMRepository _uomRepository = new base_UOMRepository();

        #endregion

        #region Properties

        private base_UOMModel _uomModel = new base_UOMModel();
        /// <summary>
        /// Gets or sets the UOMModel.
        /// </summary>
        public base_UOMModel UOMModel
        {
            get { return _uomModel; }
            set
            {
                if (_uomModel != value)
                {
                    _uomModel = value;
                    OnPropertyChanged(() => UOMModel);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PopupUOMViewModel()
        {
            InitialCommand();
        }

        #endregion

        #region Command Methods

        #region OkCommand

        /// <summary>
        /// Gets the OkCommand command.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Method to check whether the OkCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOkCommandCanExecute()
        {
            return UOMModel.IsDirty || !UOMModel.HasError;
        }

        /// <summary>
        /// Method to invoke when the OkCommand command is executed.
        /// </summary>
        private void OnOkCommandExecute()
        {
            try
            {
                if (IsDuplicateName(UOMModel))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("This uom is existed", "POS", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UOMModel.IsActived = true;
                UOMModel.DateCreated = DateTime.Now;
                UOMModel.UserCreated = Define.USER.LoginName;

                // Map data from model to entity
                UOMModel.ToEntity();

                // Add new uom to database
                _uomRepository.Add(UOMModel.base_UOM);

                // Accept changes
                _uomRepository.Commit();

                // Update uom ID
                UOMModel.Id = UOMModel.base_UOM.Id;

                // Turn off IsDirty & IsNew
                UOMModel.EndUpdate();

                Window window = FindOwnerWindow(this);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "POS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region CancelCommand

        /// <summary>
        /// Gets the CancelCommand command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Method to check whether the CancelCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCancelCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the CancelCommand command is executed.
        /// </summary>
        private void OnCancelCommandExecute()
        {
            Window window = FindOwnerWindow(this);
            window.DialogResult = false;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands for binding on form
        /// </summary>
        private void InitialCommand()
        {
            OkCommand = new RelayCommand(OnOkCommandExecute, OnOkCommandCanExecute);
            CancelCommand = new RelayCommand(OnCancelCommandExecute, OnCancelCommandCanExecute);
        }

        /// <summary>
        /// Check duplicate product
        /// </summary>
        /// <param name="productModel"></param>
        /// <returns></returns>
        private bool IsDuplicateName(base_UOMModel uomModel)
        {
            try
            {
                return _uomRepository.GetIQueryable(x => x.Name.ToLower().Equals(uomModel.Name.ToLower())).Count() > 0;
            }
            catch (Exception ex)
            {
                _log4net.Error(ex);
                return true;
            }
        }

        #endregion
    }
}