using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CPC.Control;
using CPC.Helper;
using CPC.POS.Database;
using CPC.POS.Repository;
using CPC.POS.View;
using CPC.Toolkit.Base;
using CPC.Toolkit.Command;
using CPC.Toolkit.Layout;
using SecurityLib;
using CPC.POS.Model;

namespace CPC.POS.ViewModel
{
    class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Layout Defines

        // Number of rows
        private int _rowNumbers = 4;

        // Number of columns
        private int _columnNumbers = 2;

        // Host view list
        private List<BorderLayoutHost> _hostList = new List<BorderLayoutHost>();

        // Grid contain hosts
        private Grid _grdHost;

        // Grid contain targets
        private Grid _grdTarget;

        // Store expand status of view
        private bool _isPanelExpanded;

        // Store columns to expand
        private ColumnDefinition _colSubItem;
        private ColumnDefinition _colSubItemExpanded;

        #endregion

        #region Defines

        private base_UserLogRepository _userLogRepository = new base_UserLogRepository();
        private base_ResourceAccountRepository _accountRepository = new base_ResourceAccountRepository();
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        private base_SaleTaxLocationRepository _saleTaxLocationRepository = new base_SaleTaxLocationRepository();

        private DispatcherTimer _idleTimer;
        private Regex _regexPassWord = new Regex("[a-zA-Z0-9!@#$%&*(){}|=]{3,50}");

        private string _defaultUsernName = "admin";
        private string _defaultPassword = "iktfcGzCJQ13CBk3uR6n9A==";
        private LockScreenView _lockScreenView;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IsAnimationCompletedAll
        /// </summary>
        public bool IsAnimationCompletedAll
        {
            get
            {
                return _hostList.Count(x => !x.IsAnimationCompleted) == 0;
                //return _hostList.Count(x => !x.IsAnimationCompleted || x.ViewModelBase.IsBusy) == 0;
            }
        }

        private ObservableCollection<string> _hiddenHostList = new ObservableCollection<string>();
        /// <summary>
        /// Gets or sets the HiddenHostList.
        /// </summary>
        public ObservableCollection<string> HiddenHostList
        {
            get
            {
                return _hiddenHostList;
            }
            set
            {
                if (_hiddenHostList != value)
                {
                    _hiddenHostList = value;
                    OnPropertyChanged(() => HiddenHostList);
                }
            }
        }

        private string _selectedHiddenHost;
        /// <summary>
        /// Gets or sets the SelectedHiddenHost.
        /// </summary>
        public string SelectedHiddenHost
        {
            get
            {
                return _selectedHiddenHost;
            }
            set
            {
                if (_selectedHiddenHost != value)
                {
                    _selectedHiddenHost = value;
                    OnPropertyChanged(() => SelectedHiddenHost);

                    if (SelectedHiddenHost != null)
                    {
                        int index = HiddenHostList.IndexOf(SelectedHiddenHost);
                        HiddenHostList.RemoveAt(index);
                        var host = _hostList.ElementAt(_hostList.Count - 1 - index);
                        ChangeLayoutItem(host.Container.btnImageIcon);
                        HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the HiddenHostVisibility
        /// </summary>
        public Visibility HiddenHostVisibility
        {
            get
            {
                return HiddenHostList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _loginName;
        /// <summary>
        /// Gets or sets the LoginName.
        /// </summary>
        public string LoginName
        {
            get
            {
                return _loginName;
            }
            set
            {
                if (_loginName != value)
                {
                    _loginName = value;
                    OnPropertyChanged(() => LoginName);
                }
            }
        }

        private string _userName = string.Empty;
        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                }
            }
        }

        private string _userPassword = string.Empty;
        /// <summary>
        /// Gets or sets the UserPassword.
        /// </summary>
        public string UserPassword
        {
            get
            {
                return _userPassword;
            }
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged(() => UserPassword);
                }
            }
        }

        /// <summary>
        /// Gets the Status
        /// </summary>
        public string Status
        {
            get
            {
                return "Connecting...";
            }
        }

        /// <summary>
        /// Gets the Server
        /// </summary>
        public string Server
        {
            get
            {
                if (Define.USER != null)
                    return Define.USER.IpAddress;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the Database
        /// </summary>
        public string Database
        {
            get;
            private set;
        }

        private bool _isLockScreen;
        /// <summary>
        /// Gets or sets the IsLockScreen.
        /// </summary>
        public bool IsLockScreen
        {
            get
            {
                return _isLockScreen;
            }
            set
            {
                if (_isLockScreen != value)
                {
                    _isLockScreen = value;
                    OnPropertyChanged(() => IsLockScreen);
                }
            }
        }

        private string _storeName;
        /// <summary>
        /// Gets or sets the StoreName.
        /// </summary>
        public string StoreName
        {
            get
            {
                return _storeName;
            }
            set
            {
                if (_storeName != value)
                {
                    _storeName = value;
                    OnPropertyChanged(() => StoreName);
                }
            }
        }

        private string _taxLocation;
        /// <summary>
        /// Gets or sets the TaxLocation.
        /// </summary>
        public string TaxLocation
        {
            get
            {
                return _taxLocation;
            }
            set
            {
                if (_taxLocation != value)
                {
                    _taxLocation = value;
                    OnPropertyChanged(() => TaxLocation);
                }
            }
        }

        private string _taxCode;
        /// <summary>
        /// Gets or sets the TaxCode.
        /// </summary>
        public string TaxCode
        {
            get
            {
                return _taxCode;
            }
            set
            {
                if (_taxCode != value)
                {
                    _taxCode = value;
                    OnPropertyChanged(() => TaxCode);
                }
            }
        }

        #region SelectedLanguage

        private ComboItem _selectedLanguage = Common.Languages.FirstOrDefault(x => x.Value == 2);
        public ComboItem SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(() => SelectedLanguage);
                    Common.ChangeLanguage(_selectedLanguage.CultureInfo);
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel()
        {
            InitialCommand();
            CheckIdleTime();
            LoadLayout();
            LoadStatusInformation();
        }

        #endregion

        #region Command Methods

        #region OpenViewCommand

        /// <summary>
        /// Gets the OpenViewCommand command.
        /// </summary>
        public ICommand OpenViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the OpenViewCommand command is executed.
        /// </summary>
        private void OnOpenViewCommandExecute(object param)
        {
            OpenViewExecute(param.ToString());
        }

        #endregion

        #region ChangeViewCommand

        /// <summary>
        /// Gets the ChangeViewCommand command.
        /// </summary>
        public ICommand ChangeViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the ChangeViewCommand command is executed.
        /// </summary>
        private void OnChangeViewCommandExecute()
        {
            if (HiddenHostList.Count > 0)
            {
                HiddenHostList.RemoveAt(0);
                HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
            }
            var host = _hostList.LastOrDefault();
            if (host != null)
                ChangeLayoutItem(host);
        }

        #endregion

        #region CloseViewCommand

        /// <summary>
        /// Gets the CloseViewCommand command.
        /// </summary>
        public ICommand CloseViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the CloseViewCommand command is executed.
        /// </summary>
        private void OnCloseViewCommandExecute(object param)
        {
            int index = HiddenHostList.IndexOf(param.ToString());
            if (index >= 0 && index < HiddenHostList.Count)
            {
                HiddenHostList.RemoveAt(index);
                OnPropertyChanged(() => HiddenHostVisibility);
                _hostList.RemoveAt(_hostList.Count - 1 - index);
            }
        }

        #endregion

        #region ClearViewCommand

        /// <summary>
        /// Gets the ClearViewCommand command.
        /// </summary>
        public ICommand ClearViewCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the ClearViewCommand command is executed.
        /// </summary>
        private void OnClearViewCommandExecute()
        {
            // Clear host list
            _hostList.Clear();
            HiddenHostList.Clear();
            OnPropertyChanged(() => HiddenHostVisibility);
            // Clear UIElement hosts
            _grdHost.Children.Clear();
        }

        #endregion

        #region CloseCommand

        /// <summary>
        /// Gets the CloseCommand command.
        /// </summary>
        public ICommand CloseCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the CloseCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCloseCommandCanExecute()
        {
            if (_hostList.Count == 0)
                return false;
            BorderLayoutHost currentHost = _hostList.ElementAt(0) as BorderLayoutHost;
            return !currentHost.ShowNotification(true);
        }

        /// <summary>
        /// Method to invoke when the CloseCommand command is executed.
        /// </summary>
        private void OnCloseCommandExecute()
        {
            //To write log into base_UserLogDetail table.
            App.WriteLUserLog("Exit", "User closed the application.");
            //To Update status of user.
            this.UpdateUserLog();
        }

        #endregion

        #region ChangeStyleCommand

        /// <summary>
        /// Gets the ChangeStyleCommand command.
        /// </summary>
        public ICommand ChangeStyleCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ChangeStyleCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangeStyleCommandCanExecute(object param)
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the ChangeStyleCommand command is executed.
        /// </summary>
        private void OnChangeStyleCommandExecute(object param)
        {
            int count = Application.Current.Resources.MergedDictionaries.Count;
            ResourceDictionary skin = Application.Current.Resources.MergedDictionaries[count - 2];

            switch (param.ToString())
            {
                case "BlueResources":
                    {
                        skin.Source = new Uri(@"..\Dictionary\Brushes\Blue\" + param + ".xaml", UriKind.Relative);
                        skin = Application.Current.Resources.MergedDictionaries[count - 1];
                        break;
                    }
                case "GreyResources":
                    {
                        skin.Source = new Uri(@"..\Dictionary\Brushes\Grey\" + param + ".xaml", UriKind.Relative);
                        skin = Application.Current.Resources.MergedDictionaries[count - 1];
                        break;
                    }
                case "RedResources":
                    {
                        skin.Source = new Uri(@"..\Dictionary\Brushes\Red\" + param + ".xaml", UriKind.Relative);
                        skin = Application.Current.Resources.MergedDictionaries[count - 1];
                        break;
                    }
            }

        }

        #endregion

        #region LoginCommand

        /// <summary>
        /// Gets the LoginCommand command.
        /// </summary>
        public ICommand LoginCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the LoginCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLoginCommandCanExecute()
        {
            return ExtensionErrors.Count == 0;
        }

        /// <summary>
        /// Method to invoke when the LoginCommand command is executed.
        /// </summary>
        private void OnLoginCommandExecute()
        {
            try
            {
                bool result = false;

                // Encrypt password
                string encryptPassword = AESSecurity.Encrypt(UserPassword);

                // Check default account
                if (UserName.Equals(_defaultUsernName) && encryptPassword.Equals(_defaultPassword))
                    result = true;
                else if (UserName.Equals(LoginName)) // Check login account
                {
                    // Get login account from database
                    base_ResourceAccount account = _accountRepository.Get(x => x.LoginName.Equals(UserName) && x.Password.Equals(encryptPassword));
                    result = account != null;
                }

                if (result)
                {
                    // Clear user password
                    UserPassword = string.Empty;

                    // Turn off lock screen view
                    _lockScreenView.DialogResult = true;
                }
                else
                {
                    // Show alert message
                    MessageBox.Show("Username or Password is not valid, please try again!", "POS", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnLoginCommand" + ex.ToString());
            }
        }

        #endregion

        #region LogOutCommand

        /// <summary>
        /// Gets the LogOutCommand command.
        /// </summary>
        public ICommand LogOutCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Enable the Login button if all required field are validated IsValid = true
        ///Created by Thaipn.
        /// </summary>
        /// <returns></returns>
        private bool CanOnLogOutExecute()
        {
            return true;
        }

        /// <summary>
        /// Check the user login
        /// </summary>
        private void OnLogOutExecuted()
        {
            if (App.Current.MainWindow is MainWindow)
            {
                //To Update status of user.
                //this.UpdateUserLog();
                //To write log into base_UserLogDetail table.
                App.WriteLUserLog("Logout", "User logged out the application.");
                // To LogOut Windows.
                App.Messenger.NotifyColleagues(Define.USER_LOGOUT_RESULT);
            }
        }

        #endregion

        #region OpenManagementUserCommand

        /// <summary>
        /// Gets the OpenManagementUserCommand command.
        /// </summary>
        public ICommand OpenManagementUserCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the OpenViewCommand command is executed.
        /// </summary>
        private void OnOpenManagermentUserCommandExecute(object param)
        {
            ManagementUserLogView view = new ManagementUserLogView();
            view.DataContext = new ManagementUserLogViewModel();
            view.Show();
        }

        #endregion

        #region ChangePasswordCommand

        /// <summary>
        /// Gets the ChangePasswordCommand command.
        /// </summary>
        public ICommand ChangePasswordCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the ChangePasswordCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnChangePasswordCommandCanExecute()
        {
            if (Define.USER == null)
                return false;
            return !Define.ADMIN_ACCOUNT.Equals(Define.USER.LoginName);
        }

        /// <summary>
        /// Method to invoke when the ChangePasswordCommand command is executed.
        /// </summary>
        private void OnChangePasswordCommandExecute()
        {
            ChangePasswordViewModel viewModel = new ChangePasswordViewModel();
            bool? result = _dialogService.ShowDialog<ChangePasswordView>(this, viewModel, "Change Password");
            if (result.HasValue && result.Value)
            {
                LoginName = viewModel.ResourceAccountModel.LoginName;
            }
        }

        #endregion

        #region LockScreenCommand

        /// <summary>
        /// Gets the LockScreenCommand command.
        /// </summary>
        public ICommand LockScreenCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to check whether the LockScreenCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnLockScreenCommandCanExecute()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LockScreenCommand command is executed.
        /// </summary>
        public void OnLockScreenCommandExecute()
        {
            // Stop idle timer
            _idleTimer.Stop();

            // Set user name default
            UserName = LoginName;

            // Get main window
            Window mainWindow = App.Current.MainWindow;

            // Initial lock screen view
            _lockScreenView = new LockScreenView();
            _lockScreenView.DataContext = this;

            // Register closing event
            _lockScreenView.Closing += (senderLockScreen, eLockScreen) =>
            {
                // Prevent closing lock screen view when login have not success
                if (!_lockScreenView.DialogResult.HasValue)
                    eLockScreen.Cancel = true;
            };

            // Set default position over main window
            _lockScreenView.Width = mainWindow.ActualWidth;
            _lockScreenView.Height = mainWindow.ActualHeight;
            switch (mainWindow.WindowState)
            {
                case WindowState.Maximized:
                    _lockScreenView.Top = 0;
                    _lockScreenView.Left = 0;
                    break;
                case WindowState.Minimized:
                case WindowState.Normal:
                    _lockScreenView.Top = mainWindow.Top;
                    _lockScreenView.Left = mainWindow.Left;
                    break;
            }

            // Set login binding
            Binding loginBinding = new Binding("LoginCommand");
            BindingOperations.SetBinding(_lockScreenView.btnLogin, Button.CommandProperty, loginBinding);

            // Get active window if main show popup
            Window activeWindow = mainWindow.OwnedWindows.Cast<Window>().SingleOrDefault(x => x.IsActive);
            if (activeWindow != null)
            {
                // Set active window is owner lock screen view
                _lockScreenView.Owner = activeWindow;
            }
            else
            {
                // Set main window is owner lock screen view
                _lockScreenView.Owner = mainWindow;
            }

            // Show lock screen view
            if (_lockScreenView.ShowDialog().HasValue)
            {
                IdleTimeHelper.LostFocusTime = null;

                // Star idle timer
                _idleTimer.Start();
            }
        }

        #endregion

        #endregion

        #region Layout Methods

        /// <summary>
        /// Open a UserControl as view by name
        /// </summary>
        /// <param name="viewName">Name of view</param>
        /// <param name="param">Opening with parameter. Default parameter is null</param>
        public void OpenViewExecute(string viewName, object param = null)
        {
            // Check all animation is completed
            if (IsAnimationCompletedAll)
            {
                // Create new target
                BorderLayoutTarget target = new BorderLayoutTarget();

                // Define position for target
                _grdTarget.Children.Add(target);
                Grid.SetRowSpan(target, _rowNumbers);
                if (_hostList.Count == 0) // The largest target
                    Grid.SetColumnSpan(target, _columnNumbers);

                // Create new host with defined target
                BorderLayoutHost host = new BorderLayoutHost(target);
                host.SetHostName(viewName);

                // Set parameter for view
                if (param != null)
                    host.Tag = param;

                // Check parameter to open view
                var hostClicked = _hostList.FirstOrDefault(x => x.KeyName.Equals(host.KeyName));
                if (hostClicked == null)
                {
                    // Show notification when open a new form
                    if (_hostList.Count > 0 && !_hostList.ElementAt(0).ShowNotification(false))
                        return;

                    if (!CreateContainerView(host))
                        return;

                    if (_hostList.Count > 0)
                    {
                        // Set focus to title bar to active keybinding
                        _hostList.ElementAt(0).Container.btnTitle.Focus();

                        // Turn on screenshot
                        _hostList.ElementAt(0).AllowScreenShot = true;
                    }

                    _hostList.Insert(0, host);
                    _grdHost.Children.Insert(0, host);
                    host.Loaded += new RoutedEventHandler(host_Loaded);
                    SetPositionTarget();

                    if (_hostList.Count > _rowNumbers + 1)
                    {
                        HiddenHostList.Add(_hostList.ElementAt(_rowNumbers + 1).Container.btnTitle.Content.ToString());
                        OnPropertyChanged(() => HiddenHostVisibility);
                    }
                }
                else
                {
                    ChangeLayoutItem(hostClicked);
                    hostClicked.ViewModelBase.ChangeSearchMode(host.IsOpenList, host.Tag);
                }
            }
        }

        /// <summary>
        /// Create container to contain view and register events
        /// </summary>
        /// <param name="host">Host contain container</param>
        /// <returns>True is create view. False is show popup</returns>
        private bool CreateContainerView(BorderLayoutHost host)
        {
            bool showPopup = false;
            host.Child = new ContainerView();
            UserControl view = new UserControl();
            ViewModelBase viewModel = new ViewModelBase();
            //TestViewModel testViewModel = new TestViewModel { TitleView = name };
            //host.SetDataContext(testViewModel);
            //host.DataContext = testViewModel;
            //host.Container.grdContent.Children.Add(view);
            host.Container.btnTitle.Content = host.DisplayName;
            host.Container.btnTitle.PreviewMouseDoubleClick += new MouseButtonEventHandler(btnTitle_MouseDoubleClick);
            host.Container.btnImageIcon.Click += new RoutedEventHandler(btnImageIcon_Click);
            host.Container.btnTitle.Click += new RoutedEventHandler(btnTitle_Click);
            host.Container.btnClose.Click += new RoutedEventHandler(btnClose_Click);
            host.Container.tgExpand.IsChecked = _isPanelExpanded;
            host.Container.tgExpand.Checked += new RoutedEventHandler(tgExpand_Checked);
            host.Container.tgExpand.Unchecked += new RoutedEventHandler(tgExpand_Unchecked);
            if (_hostList.Count == 0)
                host.Container.tgExpand.Visibility = Visibility.Collapsed;
            host.Container.btnImageIcon.Visibility = Visibility.Collapsed;

            switch (host.KeyName)
            {
                #region Application Menu

                case "CompanySetting":
                    view = new CompanySettingView();
                    viewModel = new CompanySettingViewModel();
                    //PopupGuestViewModel popupVendorViewModel = new PopupGuestViewModel(MarkType.Customer);
                    //_dialogService.ShowDialog<PopupGuestView>(this, popupVendorViewModel, null);
                    //var item = popupVendorViewModel.NewItem;
                    //showPopup = true;
                    break;
                case "Department":
                    view = new DepartmentView();
                    viewModel = new DepartmentViewModel();
                    break;

                #endregion

                #region Store Tab

                case "Employee":
                    view = new EmployeeInformationView();
                    viewModel = new EmployeeViewModel(host.IsOpenList);
                    break;
                #endregion

                #region Sales Tab

                case "Customer":
                    view = new CustomerView();
                    viewModel = new CustomerViewModel(host.IsOpenList);
                    break;
                case "RewardSetup":
                    view = new RewardSetupView();
                    viewModel = new RewardSetupViewModel();
                    break;
                case "RewardHistory":
                    view = new RewardListandHistoryView();
                    viewModel = new RewardListandHistoryViewModel();
                    break;
                case "GiftCard":
                    view = new GiftCardView();
                    viewModel = new GiftCardViewModel();
                    break;
                case "Quotation":
                    view = new QuotationView();
                    viewModel = new SalesOrderViewModel(host.IsOpenList, true);
                    break;
                case "SalesOrder":
                    view = new SalesOrderView();
                    viewModel = new SalesOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "SalesOrderLocked":
                    view = new LockSalesOrderView();
                    viewModel = new LockSalesOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "SOReturn":
                    view = new SalesOrderReturnView();
                    viewModel = new SalesOrderReturnViewModel();
                    break;
                case "SOReturnList":
                    view = new SalesOrderReturnView();
                    viewModel = new SalesOrderReturnViewModel(true);
                    break;

                #endregion

                #region Purchase Tab

                case "Vendor":
                    view = new VendorView();
                    viewModel = new VendorViewModel(host.IsOpenList);
                    break;
                case "PurchaseOrder":
                    view = new PurchaseOrderView();
                    viewModel = new PurchaseOrderViewModel(host.IsOpenList, host.Tag);
                    break;
                case "PurchaseOrderLocked":
                    view = new LockPOListView();
                    viewModel = new LockPOListViewModel(host.IsOpenList, host.Tag);
                    break;
                case "POReturn":
                    view = new PurchaseOrderReturnView();
                    viewModel = new PurchaseOrderReturnViewModel();
                    break;
                case "POReturnList":
                    view = new PurchaseOrderReturnView();
                    viewModel = new PurchaseOrderReturnViewModel();
                    break;

                #endregion

                #region Inventory Tab

                case "Product":
                    view = new ProductView();
                    viewModel = new ProductViewModel(host.IsOpenList);
                    break;
                case "MovementHistory":
                    view = new ProductMovementHistoryView();
                    viewModel = new ProductMovementHistoryViewModel();
                    break;
                case "Pricing":
                    view = new PricingView();
                    viewModel = new PricingViewModel(host.IsOpenList);
                    break;
                case "PricingList":
                    view = new PriceManagementView();
                    viewModel = new PriceManagementViewModel();
                    break;
                case "Discount":
                    view = new PromotionView();
                    viewModel = new PromotionViewModel(host.IsOpenList);
                    break;
                case "CountSheet":
                    view = new CountSheetView();
                    viewModel = new CountSheetViewModel(host.IsOpenList);
                    break;
                case "CountSheetList":
                    view = new CountSheetView();
                    viewModel = new CountSheetViewModel();
                    break;
                case "StockAdjustment":
                    view = new AdjustmentInformationView();
                    viewModel = new AdjustmentInformationViewModel();
                    break;
                case "StockAdjustmentList":
                    view = new AdjustmentInformationView();
                    viewModel = new AdjustmentInformationViewModel();
                    break;
                case "TransferStock":
                    view = new TransferStockView();
                    viewModel = new TransferStockViewModel(host.IsOpenList, host.Tag);
                    break;
                case "ReOrderStock":
                    view = new ReOrderStockView();
                    viewModel = new ReorderStockViewModel();
                    break;
                case "WorkOrder":
                    view = new WorkOrderView();
                    viewModel = new WorkOrderViewModel(false);
                    break;
                case "WorkOrderList":
                    view = new WorkOrderView();
                    viewModel = new WorkOrderViewModel(true);
                    break;
                case "CostAdjustment":
                    view = new CostAdjustmentHistoryView();
                    viewModel = new CostAdjustmentHistoryViewModel();
                    break;
                case "QuantityAdjustment":
                    view = new QuantityAdjustmentHistoryView();
                    viewModel = new QuantityAdjustmentHistoryViewModel();
                    break;

                #endregion

                #region Report Tab

                case "InventoryReport":
                    view = new InventoryReportView();
                    viewModel = new InventoryReportViewModel();
                    break;
                case "SalesReport":
                    break;
                case "PurchaseReport":
                    break;

                #endregion

                #region Configuration Tab

                case "Attachment":
                    view = new AttachmentView();
                    viewModel = new AttachmentViewModel();
                    break;
                case "SalesTax":
                    view = new SalesTaxView();
                    viewModel = new SalesTaxViewModel();
                    break;
                case "Style":
                    break;

                #endregion

                #region TimeClock

                case "Holiday":
                    view = new HolidayView();
                    viewModel = new HolidayViewModel();
                    break;
                case "WorkSchedule":
                    view = new WorkScheduleView();
                    viewModel = new WorkScheduleViewModel();
                    break;
                case "WorkPermission":
                    view = new WorkPermissionView();
                    viewModel = new WorkPermissionViewModel(host.IsOpenList);
                    break;
                case "ManualEventEditing":
                    view = new TimeClockManualEventEditingView();
                    viewModel = new TimeClockManualEventEditingViewModel(true);
                    break;
                case "PunctualityComparativeReport":
                    break;
                case "AnalysisReport":
                    break;
                case "EmployeeTimeClockDisplay":
                    break;
                case "Calculator":
                    _dialogService.ShowDialog<CalculatorView>(this, null, "Calculator");
                    showPopup = true;
                    break;

                #endregion

                #region Unknown Tab

                // Settings
                case "Archive":
                    break;
                case "Message":
                    break;
                case "UserPermission":
                    view = new UserListView();
                    viewModel = new UserListViewModel();
                    break;
                case "GroupPermission":
                    view = new GroupPermissionView();
                    viewModel = new GroupPermissionViewModel();
                    break;
                case "CurrentStock":
                    view = new CurrentStockView();
                    viewModel = new CurrentStockViewModel();
                    break;
                case "LayawayHistory":
                    view = new LayawayOrderHistoryView();

                    break;

                #endregion
            }

            host.DataContext = viewModel;
            host.Container.grdContent.Children.Add(view);

            return !showPopup;
        }

        /// <summary>
        /// Set shortcut key for usercontrol from main view
        /// </summary>
        /// <param name="host"></param>
        public void SetKeyBinding(InputBindingCollection inputBindingCollection)
        {
            SetKeyBinding(inputBindingCollection, App.Current.MainWindow);
        }

        /// <summary>
        /// Set shortcut key for usercontrol from main view
        /// </summary>
        /// <param name="host"></param>
        public void SetKeyBinding(InputBindingCollection inputBindingCollection, Window target)
        {
            // Get input binding collection from source window
            InputBindingCollection sourceInputBindingCollection = inputBindingCollection;

            if (sourceInputBindingCollection != null)
            {
                foreach (InputBinding sourceInputBindingItem in sourceInputBindingCollection)
                {
                    // Get key gesture of input binding
                    KeyGesture sourceKeyGesture = sourceInputBindingItem.Gesture as KeyGesture;

                    // Create key binding for main
                    KeyBinding targetKeyBinding = new KeyBinding(sourceInputBindingItem.Command, sourceKeyGesture);
                    //targetKeyBinding.CommandTarget = host;
                    targetKeyBinding.CommandParameter = sourceInputBindingItem.CommandParameter + "Main";

                    // Get key binding from main
                    InputBinding keyBinding = target.InputBindings.Cast<InputBinding>().FirstOrDefault(
                        x => ((KeyGesture)x.Gesture).Key.Equals(sourceKeyGesture.Key) &&
                            ((KeyGesture)x.Gesture).Modifiers.Equals(sourceKeyGesture.Modifiers));

                    // Check exist key binding
                    if (keyBinding != null)
                    {
                        // Remove key binding is existed from main
                        target.InputBindings.Remove(keyBinding);
                    }

                    // Add new key binding to main
                    target.InputBindings.Add(targetKeyBinding);
                }
            }
        }

        /// <summary>
        /// Expand current view when click expand button
        /// </summary>
        /// <param name="tgExpand"></param>
        private void ExpandItem(ToggleButton tgExpand)
        {
            if (IsAnimationCompletedAll)
            {
                _isPanelExpanded = tgExpand.IsChecked.Value;
                int colSubItemIndex = _grdTarget.ColumnDefinitions.IndexOf(_colSubItem);
                int colSubItemExpandedIndex = _grdTarget.ColumnDefinitions.IndexOf(_colSubItemExpanded);
                if (_isPanelExpanded)
                {
                    if (colSubItemIndex >= 0)
                        _grdTarget.ColumnDefinitions.Remove(_colSubItem);
                    if (colSubItemExpandedIndex < 0)
                        _grdTarget.ColumnDefinitions.Add(_colSubItemExpanded);
                }
                else
                {
                    if (colSubItemExpandedIndex >= 0)
                        _grdTarget.ColumnDefinitions.Remove(_colSubItemExpanded);
                    if (colSubItemIndex < 0)
                        _grdTarget.ColumnDefinitions.Add(_colSubItem);
                }
                for (int i = 1; i < _hostList.Count; i++)
                {
                    BorderLayoutHost host = _hostList.ElementAt(i);
                    host.RotateItem(_isPanelExpanded);
                }
            }
        }

        /// <summary>
        /// Call close view function when click close button
        /// </summary>
        /// <param name="btnClicked"></param>
        private void CloseItem(Button btnClicked)
        {
            BorderLayoutHost hostClicked = _hostList.SingleOrDefault(x => x.Container.btnClose.Equals(btnClicked));
            CloseItem(hostClicked);
        }

        /// <summary>
        /// Call close view function from other view
        /// </summary>
        /// <param name="viewName"></param>
        public void CloseItem(string viewName)
        {
            // Close hidden view
            OnCloseViewCommandExecute(viewName);

            // Close visible view
            BorderLayoutHost hostClicked = _hostList.FirstOrDefault(x => x.KeyName.Equals(viewName));
            CloseItem(hostClicked);
        }

        /// <summary>
        /// Process close view function
        /// </summary>
        /// <param name="hostClicked"></param>
        private void CloseItem(BorderLayoutHost hostClicked)
        {
            if (IsAnimationCompletedAll && hostClicked != null)
            {
                // Show notification when close a form
                int hostClickedPosition = _hostList.IndexOf(hostClicked);
                if (hostClickedPosition == 0 && !hostClicked.ShowNotification(true))
                    return;

                if (HiddenHostList.Count > 0)
                {
                    HiddenHostList.RemoveAt(HiddenHostList.Count - 1);
                    OnPropertyChanged(() => HiddenHostVisibility);
                }

                _hostList.Remove(hostClicked);
                _grdHost.Children.Remove(hostClicked);
                if (_hostList.Count > 0)
                {
                    BorderLayoutHost host = _hostList.ElementAt(0);
                    KeyboardNavigation.SetTabNavigation(hostClicked.View, KeyboardNavigationMode.Continue);
                    BorderLayoutTarget target = host.Target;
                    Grid.SetRow(target, 0);
                    Grid.SetRowSpan(target, _rowNumbers);
                    Grid.SetColumn(target, 0);
                    if (_hostList.Count > 1)
                    {
                        host.Container.tgExpand.IsChecked = _isPanelExpanded;
                        host.Container.tgExpand.Visibility = Visibility.Visible;
                    }
                    else // Only one view
                    {
                        Grid.SetColumnSpan(target, _columnNumbers);
                        //host.Container.tgExpand.IsChecked = false;
                        host.Container.tgExpand.Visibility = Visibility.Collapsed;
                    }
                    host.Container.btnImageIcon.Visibility = Visibility.Collapsed;
                    host.RotateItem(false);

                    // Refresh data
                    if (hostClickedPosition == 0)
                        host.IsRefreshData = true;
                    SetPositionTarget();
                    SetKeyBinding(host.View.InputBindings);
                }
            }
        }

        /// <summary>
        /// Call change view layout function
        /// </summary>
        /// <param name="btnClicked"></param>
        private void ChangeLayoutItem(Button btnClicked)
        {
            if (IsAnimationCompletedAll)
            {
                // Get clicked host
                var hostClicked = _hostList.SingleOrDefault(
                    x => x.Container.btnImageIcon.Equals(btnClicked) || x.Container.btnTitle.Equals(btnClicked));
                ChangeLayoutItem(hostClicked);
            }
        }

        /// <summary>
        /// Process change view layout
        /// </summary>
        /// <param name="hostClicked"></param>
        private void ChangeLayoutItem(BorderLayoutHost hostClicked)
        {
            if (IsAnimationCompletedAll)
            {
                // Turn on screenshot of main host
                if (_hostList.Count > 0)
                    _hostList.ElementAt(0).AllowScreenShot = true;

                // If clicked host is not the first host, swap position that host
                if (hostClicked != null && _hostList.IndexOf(hostClicked) > 0)
                {
                    // Show notification when change position form
                    if (!_hostList.ElementAt(0).ShowNotification(false))
                        return;

                    // Set position target of clicked host
                    BorderLayoutTarget target = hostClicked.Target;
                    Grid.SetRow(target, 0);
                    Grid.SetRowSpan(target, _rowNumbers);
                    Grid.SetColumn(target, 0);
                    if (_hostList.Count == 0)
                        Grid.SetColumnSpan(target, _columnNumbers);

                    _hostList.Remove(hostClicked);
                    _grdHost.Children.Remove(hostClicked);
                    _hostList.Insert(0, hostClicked);
                    _grdHost.Children.Insert(0, hostClicked);

                    KeyboardNavigation.SetTabNavigation(hostClicked.View, KeyboardNavigationMode.Continue);
                    hostClicked.Container.tgExpand.IsChecked = _isPanelExpanded;
                    hostClicked.Container.tgExpand.Visibility = Visibility.Visible;
                    hostClicked.Container.btnImageIcon.Visibility = Visibility.Collapsed;
                    hostClicked.RotateItem(false);

                    // Refresh data
                    hostClicked.IsRefreshData = true;
                    SetPositionTarget();
                    SetKeyBinding(hostClicked.View.InputBindings);
                }
            }
        }

        /// <summary>
        /// Set position of target from the second to last
        /// </summary>
        private void SetPositionTarget()
        {
            for (int i = 1; i < _hostList.Count; i++)
            {
                BorderLayoutHost host = _hostList.ElementAt(i);
                BorderLayoutTarget target = host.Target;
                Grid.SetRow(target, i - 1);
                Grid.SetRowSpan(target, 1);
                Grid.SetColumn(target, 1);
                Grid.SetColumnSpan(target, 1);

                // Update host is not main
                // Disable focusable of host's view
                KeyboardNavigation.SetTabNavigation(host.View, KeyboardNavigationMode.None);
                // Collapse expand button
                host.Container.tgExpand.Visibility = Visibility.Collapsed;
                // Visible image icon button
                host.Container.btnImageIcon.Visibility = Visibility.Visible;
                // If expand button is checked, rotate title bar of host
                host.RotateItem(_isPanelExpanded);
            }
        }

        /// <summary>
        /// Create default layout
        /// </summary>
        private void LoadLayout()
        {
            // Get all grid to layout
            Grid grdMainView = App.Current.MainWindow.FindName("grdMainView") as Grid;
            _grdTarget = new Grid
            {
                Name = "grdTarget"
            };
            _grdHost = new Grid
            {
                Name = "grdHost",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            grdMainView.Children.Add(_grdTarget);
            grdMainView.Children.Add(_grdHost);

            // Add column for target grid
            _grdTarget.ColumnDefinitions.Add(new ColumnDefinition());
            _colSubItem = new ColumnDefinition
            {
                Width = new GridLength(215)
            };
            _colSubItemExpanded = new ColumnDefinition
            {
                Width = new GridLength(36)
            };
            _grdTarget.ColumnDefinitions.Add(_colSubItem);

            // Add row for target grid
            for (int i = 0; i < _rowNumbers; i++)
                _grdTarget.RowDefinitions.Add(new RowDefinition());
            _grdTarget.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0)
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initial commands
        /// </summary>
        private void InitialCommand()
        {
            OpenViewCommand = new RelayCommand<object>(OnOpenViewCommandExecute);
            ChangeViewCommand = new RelayCommand(OnChangeViewCommandExecute);
            CloseViewCommand = new RelayCommand<object>(OnCloseViewCommandExecute);
            ClearViewCommand = new RelayCommand(OnClearViewCommandExecute);
            CloseCommand = new RelayCommand(OnCloseCommandExecute, OnCloseCommandCanExecute);
            ChangeStyleCommand = new RelayCommand<object>(OnChangeStyleCommandExecute, OnChangeStyleCommandCanExecute);
            LoginCommand = new RelayCommand(OnLoginCommandExecute, OnLoginCommandCanExecute);
            LogOutCommand = new RelayCommand(OnLogOutExecuted, CanOnLogOutExecute);
            OpenManagementUserCommand = new RelayCommand<object>(OnOpenManagermentUserCommandExecute);
            ChangePasswordCommand = new RelayCommand(OnChangePasswordCommandExecute, OnChangePasswordCommandCanExecute);
            LockScreenCommand = new RelayCommand(OnLockScreenCommandExecute, OnLockScreenCommandCanExecute);
        }

        /// <summary>
        /// To update data on base_UserLog table.
        /// </summary>
        private void UpdateUserLog()
        {
            try
            {
                if (Define.USER != null)
                {
                    CPC.POS.Repository.base_UserLogRepository userLogRepository = new Repository.base_UserLogRepository();
                    CPC.POS.Database.base_UserLog userLog = userLogRepository.GetIEnumerable(x => x.ResourceAccessed == Define.USER.UserResource && x.IsDisconected.HasValue && !x.IsDisconected.Value).SingleOrDefault();
                    if (userLog != null)
                    {
                        userLog.DisConnectedOn = DateTimeExt.Now;
                        userLog.IsDisconected = true;
                        userLogRepository.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Logout Fail" + ex.ToString());
            }
        }

        /// <summary>
        /// Check idle time function
        /// </summary>
        private void CheckIdleTime()
        {
            _idleTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            _idleTimer.Interval = TimeSpan.FromSeconds(1);
            _idleTimer.Tick += (sender, e) =>
            {
                if (IsIdle())
                {
                    if (App.Current.MainWindow.WindowState.Equals(WindowState.Minimized))
                    {
                        // Stop idle timer
                        _idleTimer.Stop();

                        // Turn on lock screen when window active
                        IsLockScreen = true;
                    }
                    else
                        OnLockScreenCommandExecute();
                }
            };

            // Star idle timer
            _idleTimer.Start();
        }

        /// <summary>
        /// Check system and application idle
        /// </summary>
        /// <returns></returns>
        private bool IsIdle()
        {
            // Check idle to LogOut application if TimeOutMinute is not null
            if (!Define.CONFIGURATION.TimeOutMinute.HasValue)
                return false;

            // Define time out minute value
            TimeSpan activityThreshold = TimeSpan.FromMinutes(Define.CONFIGURATION.TimeOutMinute.Value);
            //TimeSpan activityThreshold = TimeSpan.FromSeconds(5);

            // Get last input time to get system idle time
            TimeSpan machineIdle = IdleTimeHelper.GetIdleTime();

            // Get application idle time
            TimeSpan? appIdle = !IdleTimeHelper.LostFocusTime.HasValue ? null : (TimeSpan?)DateTime.Now.Subtract(IdleTimeHelper.LostFocusTime.Value);

            // Check is system idle
            bool isMachineIdle = machineIdle > activityThreshold;

            // Check is application idle
            bool isAppIdle = appIdle.HasValue && appIdle > activityThreshold;

            return isMachineIdle || isAppIdle;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check view is opened
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>True is opened</returns>
        public bool IsOpenedView(string viewName)
        {
            return _hostList.Count(x => x.KeyName.Equals(viewName)) > 0;
        }

        /// <summary>
        /// Load status information
        /// </summary>
        public void LoadStatusInformation()
        {
            LoadStoreName();
            LoadTaxLocationAndCode();

            // Get login name
            LoginName = Define.USER.LoginName;

            // Get database name
            GetConnectionStringInfo(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString);
        }

        /// <summary>
        /// Load store name
        /// </summary>
        public void LoadStoreName()
        {
            // Get store name

            IOrderedEnumerable<base_Store> store = _storeRepository.GetAll().OrderBy(x => x.Id);
            if (store != null && store.Count() > 0)
                StoreName = store.ElementAt(Define.StoreCode).Name;
        }

        /// <summary>
        /// Load tax location and tax code
        /// </summary>
        public void LoadTaxLocationAndCode()
        {
            // Get tax location
            base_SaleTaxLocation taxLocation = null;
            if (Define.CONFIGURATION.DefaultSaleTaxLocation.HasValue)
            {
                taxLocation = _saleTaxLocationRepository.Get(x => x.Id.Equals(Define.CONFIGURATION.DefaultSaleTaxLocation.Value));
            }
            if (taxLocation != null)
                TaxLocation = taxLocation.Name;
            else
            {
                taxLocation = _saleTaxLocationRepository.CreateDefaulSaleTaxLocation();
                TaxLocation = taxLocation.Name;
            }

            // Get tax code
            TaxCode = Define.CONFIGURATION.DefaultTaxCodeNewDepartment;
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Set keybinding after form loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void host_Loaded(object sender, RoutedEventArgs e)
        {
            BorderLayoutHost host = sender as BorderLayoutHost;
            SetKeyBinding(host.View.InputBindings);
        }

        /// <summary>
        /// Shrink current view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tgExpand_Unchecked(object sender, RoutedEventArgs e)
        {
            ExpandItem(sender as ToggleButton);
        }

        /// <summary>
        /// Expand current view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tgExpand_Checked(object sender, RoutedEventArgs e)
        {
            ExpandItem(sender as ToggleButton);
        }

        /// <summary>
        /// Call close view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseItem(sender as Button);
        }

        /// <summary>
        /// Call change view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTitle_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutItem(sender as Button);
        }

        /// <summary>
        /// Call change view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImageIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutItem(sender as Button);
        }

        /// <summary>
        /// Call expand current view function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsAnimationCompletedAll)
                _hostList.ElementAt(0).Container.tgExpand.IsChecked = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get connection string info
        /// </summary>
        /// <param name="connectionString">ConnectionString</param>
        /// <param name="server">Server</param>
        /// <param name="userID">UserName</param>
        /// <param name="password">Password</param>
        /// <param name="database">Database</param>
        private void GetConnectionStringInfo(string connectionString)
        {
            //connectionString = connectionString.Replace(";", "; ");
            Regex nameval = new Regex(@"(?<name>[^=]+)\s*=\s*(?<val>[^;]+?)\s*(;|$)",
                RegexOptions.Singleline);

            foreach (Match m in nameval.Matches(connectionString))
            {
                //Console.WriteLine("name=[{0}], val=[{1}]",
                //    m.Groups["name"].ToString(), m.Groups["val"].ToString());

                switch (m.Groups["name"].ToString())
                {
                    //case "Server":
                    //    Server = m.Groups["val"].ToString();
                    //    break;
                    //case "UserID":
                    //    userID = m.Groups["val"].ToString();
                    //    break;
                    //case "Password":
                    //    password = m.Groups["val"].ToString();
                    //    break;
                    case "Database":
                        Database = m.Groups["val"].ToString();
                        break;
                    //case "Port":
                    //    port = m.Groups["val"].ToString();
                    //    break;
                }
            }
        }

        #endregion

        #region IDataErrorInfo Members

        protected HashSet<string> _extensionErrors = new HashSet<string>();
        /// <summary>
        /// <para> Gets or sets the ExtensionErrors </para>
        /// </summary>
        public HashSet<string> ExtensionErrors
        {
            get
            {
                return _extensionErrors;
            }
            set
            {
                if (_extensionErrors != value)
                {
                    _extensionErrors = value;
                    OnPropertyChanged(() => ExtensionErrors);
                }
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = null;
                this.ExtensionErrors.Clear();

                switch (columnName)
                {
                    case "UserPassword":
                        if (string.IsNullOrEmpty(UserPassword))
                            message = "Password is required.";
                        else if (!_regexPassWord.IsMatch(UserPassword))
                            message = "Password must a-z and length of 3-50 characters";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                    this.ExtensionErrors.Add(columnName);

                return message;
            }
        }

        #endregion
    }
}
