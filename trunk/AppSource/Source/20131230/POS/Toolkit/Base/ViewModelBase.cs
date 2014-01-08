using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CPC.POS;
using CPC.Service;
using CPC.Toolkit.Command;
using log4net;

namespace CPC.Toolkit.Base
{
    public class ViewModelBase : NotifyPropertyChangedBase, IDisposable
    {
        #region Fields

        protected object _ownerViewModel;

        // dialog service
        protected readonly IDialogService _dialogService;

        // Static member variables
        protected readonly ILog _log4net = LogManager.GetLogger(typeof(App).Name);

        #endregion

        #region Properties

        private bool _isValid;
        /// <summary>
        /// Gets or sets the IsValid.
        /// </summary>
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        private bool _isBusy;
        /// <summary>
        /// Gets or sets the IsBusy.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(() => IsBusy);
                }
            }
        }

        public int NumberOfDisplayItems { get; set; }

        public int MaxNumberOfImages { get; set; }

        /// <summary>
        /// Check store is main
        /// </summary>
        public bool IsMainStore
        {
            get { return Define.StoreCode == 0; }
        }

        /// <summary>
        /// Check user is full permission
        /// </summary>
        public bool IsFullPermission
        {
            get { return Define.USER_AUTHORIZATION == null || Define.USER_AUTHORIZATION.Count == 0; }
        }

        /// <summary>
        /// Check user is admin
        /// </summary>
        public bool IsAdminPermission
        {
            get
            {
                if (Define.USER == null)
                    return false;
                return Define.ADMIN_ACCOUNT.Equals(Define.USER.LoginName);
            }
        }

        public bool AllowAccessPermission
        {
            get
            {
                return IsAdminPermission ? true : IsMainStore;
            }
        }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            // Initial dialog service
            _dialogService = ServiceLocator.Resolve<IDialogService>();

            // Use to show notification when change or close form
            ViewChangingCommand = new RelayCommand<bool>(OnViewChangingCommandExecute, OnViewChangingCommandCanExecute);

            // Get number of display items on DataGrid
            NumberOfDisplayItems = Define.NumberOfDisplayItems;

            // Get number of display image
            MaxNumberOfImages = Define.MaxNumberOfImages;
        }

        public ViewModelBase(FunctionTypes function)
            : this()
        {
            VerifyPermission(function);
        }

        public ViewModelBase(bool logOnView)
        {

        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the ViewChangingCommand command.
        /// </summary>
        public ICommand ViewChangingCommand { get; private set; }

        /// <summary>
        /// Method to check whether current form can be closed.
        /// </summary>
        /// <param name="param">Check form is changing or closing</param>
        /// <returns><c>true</c> close form; otherwise <c>false</c></returns>
        protected virtual bool OnViewChangingCommandCanExecute(bool isClosing)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ViewChangingCommand command is executed.
        /// </summary>
        private void OnViewChangingCommandExecute(bool isClosing)
        {
            // TODO: Handle command logic here
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override this method for load data purpose
        /// </summary>
        public virtual void LoadData()
        {
            // TODO: Handle command logic here
        }

        /// <summary>
        /// Override this method for refresh data purpose
        /// </summary>
        public virtual void RefreshData()
        {
            // TODO: Handle command logic here
        }

        public virtual void ChangeSearchMode(bool isList)
        {
            // TODO: Handle command logic here
        }

        public virtual void ChangeSearchMode(bool isList, object param = null)
        {
            // TODO: Handle command logic here
        }

        /// <summary>
        /// Get permission function
        /// </summary>
        public virtual void GetPermission()
        {
            // TODO: Handle command logic here
        }

        protected Window FindOwnerWindow(object viewModel)
        {
            FrameworkElement view = _dialogService.Views.SingleOrDefault(v => ReferenceEquals(v.DataContext, this));
            // Get owner window
            Window owner = view as Window;
            if (owner == null)
            {
                owner = Window.GetWindow(view);
            }

            // Make sure owner window was found
            if (owner == null)
            {
                throw new InvalidOperationException("View is not contained within a Window.");
            }

            return owner;
        }

        protected MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return _dialogService.ShowMessageBox(_ownerViewModel, messageBoxText, caption, button, icon);
        }

        #endregion

        #region IDisposable Members

        ///<summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose(bool isDisposing)
        {

        }

        ~ViewModelBase()
        {
            OnDispose(false);
        }

        #endregion

        #region Permission

        protected bool CanView; //, CanWrite, CanModify, CanDelete, CanCreate, CanPrint, CanExport;

        // Permission
        private void VerifyPermission(FunctionTypes function)
        {
            //int iFunction = (int)function;
            //if (null != define.Permissions && define.Permissions.ContainsKey(iFunction))
            //{
            //    PermissionTypes permission = (PermissionTypes)define.Permissions[iFunction];
            //    CanView = permission.Has(PermissionTypes.View);
            //    //CanWrite = permission.Has(PermissionTypes.Write);
            //    //CanModify = permission.Has(PermissionTypes.Modify);
            //    //CanDelete = permission.Has(PermissionTypes.Delete);
            //    //CanCreate = permission.Has(PermissionTypes.Create);
            //    //CanPrint = permission.Has(PermissionTypes.Print);
            //    //CanExport = permission.Has(PermissionTypes.Export);
            //}
        }

        #endregion

        #region window closing command

        private ICommand closingCommand;
        public ICommand ClosingCommand
        {
            get
            {
                if (closingCommand == null)
                {
                    closingCommand = new RelayCommand(
                        ExecuteClosing, CanExecuteClosing);
                }
                return closingCommand;
            }
        }

        protected virtual void ExecuteClosing()
        {

        }

        protected virtual bool CanExecuteClosing()
        {
            return true;
        }

        #endregion window close command
    }
}