using System;
using MVVMHelper.Common;
using System.Waf.Applications;
using System.Windows;
using MVVMHelper.Services;

namespace MVVMHelper.ViewModels
{
    /// <summary>
    /// Base class for all view models
    /// </summary>
    public abstract class ViewModelBase<TView> : System.Waf.Applications.ViewModel, IDisposable where TView : IView
    {
        #region Variables
        private readonly TView view;

        #endregion

        #region Properties
        ServiceLocator serviceLocator = new ServiceLocator();

        /// <summary>
        /// Gets the service locator 
        /// </summary>
        public ServiceLocator ServiceLocator
        {
            get
            {
                return serviceLocator;
            }
        }

        /// <summary>
        /// Retrieves a service object identified by <typeparamref name="TServiceContract"/>.
        /// </summary>
        /// <typeparam name="TServiceContract">The type identifier of the service.</typeparam>
        protected IMessageBoxService MessageBoxService
        {
            get
            {
                return GetService<IMessageBoxService>();
            }
        }

        /// <summary>
        /// OpenFileDialog
        /// </summary>
        /// <typeparam name="TServiceContract">The type identifier of the service.</typeparam>
        protected IOpenFileDialogService OpenFileDialogService
        {
            get
            {
                return GetService<IOpenFileDialogService>();
            }
        }
        private bool _isValid = true;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    RaisePropertyChanged(() => IsValid);
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets a service from the service locator
        /// </summary>
        /// <typeparam name="T">The type of service to return</typeparam>
        /// <returns>Returns a service that was registered with the Type T</returns>
        public T GetService<T>()
        {
            return serviceLocator.GetService<T>();
        }
        #endregion

        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;"/> class and
        /// attaches itself as <c>DataContext</c> to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ViewModelBase(TView view)
            : this(view, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;"/> class and
        /// attaches itself as <c>DataContext</c> to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ViewModelBase(TView view, Window popup)
            : this(view, false)
        {
            this._popupWindow = popup;
        }

        private void RegisterServices()
        {
            // Register services
            serviceLocator.RegisterService<IMessageBoxService>(new MessageBoxService());
            serviceLocator.RegisterService<IOpenFileDialogService>(new OpenFileDialogService());
            //serviceLocator.RegisterService<Services.ISaveFileDialogService>(new Services.SaveFileDialogService());
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isChild">if set to <c>true</c> then the ViewModel is a child of another ViewModel.</param>
        protected ViewModelBase(TView view, bool isChild)
            : base(view, isChild)
        {
            RegisterServices();

            this.view = view;
        }

        protected Window _popupWindow;

        protected void CloseDialog()
        {
            if (_popupWindow != null)
                _popupWindow.Close();
        }

        protected bool? DialogResult
        {
            get { return _popupWindow.DialogResult; }
            set
            {
                if (_popupWindow.DialogResult != value)
                    _popupWindow.DialogResult = value;
            }
        }

        /// <summary>
        /// Gets the associated view as specified view type.
        /// </summary>
        /// <remarks>
        /// Use this property in a ViewModel class to avoid casting.
        /// </remarks>
        protected TView ViewCore { get { return view; } }

        #endregion

        #region Disposable
        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // Note disposing has been done.
                disposed = true;
            }

            serviceLocator.Dispose();
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ViewModelBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion

    }
}
