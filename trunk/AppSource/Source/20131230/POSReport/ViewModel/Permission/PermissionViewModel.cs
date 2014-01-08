using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;
using CPC.POSReport.Model;
using System.Collections.ObjectModel;
using CPC.POSReport.Repository;
using Toolkit.Command;
using CPC.POSReport.Function;
using SecurityLib;
using CPC.POSReport.Properties;
using Xceed.Wpf.Toolkit;
using System.Data;

namespace CPC.POSReport.ViewModel
{
    class PermissionViewModel  : ViewModelBase
    {
        public PermissionViewModel(View.PermissionView PermissionView)
        {
            RefreshRepository();
            InitCommand();
            LoadData();
            this.PermissionView = PermissionView;
        }

        #region -Properties- 

        #region -Set focus default-
        private bool _focusDefault;
        /// <summary>
        /// Set or Get ForcusDefault
        /// </summary>
        public bool FocusDefault
        {
            get { return _focusDefault; }
            set
            {
                if (_focusDefault != value)
                {
                    _focusDefault = value;
                    OnPropertyChanged(() => FocusDefault);
                }
            }
        }
        #endregion

        #region -All permission name Collection-
        private ObservableCollection<rpt_ReportModel> _reportCollection;
        /// <summary>
        /// Set or get Report Collection
        /// </summary>
        public ObservableCollection<rpt_ReportModel> ReportCollection
        {
            get { return _reportCollection; }
            set
            {
                if (_reportCollection != value)
                {
                    _reportCollection = value;
                    OnPropertyChanged(() => ReportCollection);
                }
            }
        }


        private ObservableCollection<base_GenericCodeModel> _menuCollection;
        /// <summary>
        /// Set or get Menu Collection
        /// </summary>
        public ObservableCollection<base_GenericCodeModel> MenuCollection
        {
            get { return _menuCollection; }
            set
            {
                if (_menuCollection != value)
                {
                    _menuCollection = value;
                    OnPropertyChanged(() => MenuCollection);
                }
            }
        }

        private ObservableCollection<rpt_GroupModel> _groupCollection;
        /// <summary>
        /// Set or get Group Collection
        /// </summary>
        public ObservableCollection<rpt_GroupModel> GroupCollection
        {
            get { return _groupCollection; }
            set
            {
                if (_groupCollection != value)
                {
                    _groupCollection = value;
                    OnPropertyChanged(() => GroupCollection);
                }
            }
        } 
        #endregion

        #region -Depart model-
        private ObservableCollection<rpt_DepartmentModel> _departModelCollection;
        /// <summary>
        /// Set or get DepartModel Collection
        /// </summary>
        public ObservableCollection<rpt_DepartmentModel> DepartModelCollection
        {
            get { return _departModelCollection; }
            set
            {
                if (_departModelCollection != value)
                {
                    _departModelCollection = value;
                }
            }
        }
        #endregion

        #region -Level model-
        private ObservableCollection<rpt_DepartmentModel> _levelModelCollection;
        /// <summary>
        /// Set or get LevelModel Collection
        /// </summary>
        public ObservableCollection<rpt_DepartmentModel> LevelModelCollection
        {
            get { return _levelModelCollection; }
            set
            {
                if (_levelModelCollection != value)
                {
                    _levelModelCollection = value;
                }
            }
        }
        #endregion
       
        #region -Checkbox-

        private bool _isSetPasswordToDefault;
        /// <summary>
        /// Set or get Is Set password to default
        /// </summary>
        public bool IsSetPasswordToDefault
        {
            get { return _isSetPasswordToDefault; }
            set
            {
                if (_isSetPasswordToDefault != value)
                {
                    _isSetPasswordToDefault = value;
                    OnPropertyChanged(() => IsSetPasswordToDefault);
                }
            }
        }

        private bool _isCheckView;
        /// <summary>
        /// Set or get Is check view
        /// </summary>
        public bool IsCheckView
        {
            get { return _isCheckView; }
            set
            {
                if (_isCheckView != value)
                {
                    _isCheckView = value;
                    OnPropertyChanged(() => IsCheckView);
                }
            }
        }

        private bool _isCheckPrint;
        /// <summary>
        /// Set or get Is check print
        /// </summary>
        public bool IsCheckPrint
        {
            get { return _isCheckPrint; }
            set
            {
                if (_isCheckPrint != value)
                {
                    _isCheckPrint = value;
                    OnPropertyChanged(() => IsCheckPrint);
                }
            }
        }

        private bool _isCheckRight;
        /// <summary>
        /// Set or get Is Check Right
        /// </summary>
        public bool IsCheckRight
        {
            get { return _isCheckRight; }
            set
            {
                if (_isCheckRight != value)
                {
                    _isCheckRight = value;
                    OnPropertyChanged(() => IsCheckRight);
                }
            }
        }
        #endregion        

        #region -User model-
        private rpt_UserModel _userModel;
        /// <summary>
        /// Set or get UserModel
        /// </summary>
        public rpt_UserModel UserModel
        {
            get { return _userModel; }
            set
            {
              //  if (_userModel != value)
                //{
                    _userModel = value;
                    OnPropertyChanged(() => UserModel); 
                //}
            }
        }

        private ObservableCollection<rpt_UserModel> _userModelCollection;
        /// <summary>
        /// Set or get UserModel Collection
        /// </summary>
        public ObservableCollection<rpt_UserModel> UserModelCollection
        {
            get { return _userModelCollection; }
            set
            {
                if (_userModelCollection != value)
                {
                    _userModelCollection = value;
                    OnPropertyChanged(() => UserModelCollection);
                }
            }
        }
        #endregion

        #region -User Report Permission Model-
        private rpt_PermissionModel _uRPermissionModel;
        /// <summary>
        /// Set or get User Report Permisstion Model
        /// </summary>
        public rpt_PermissionModel URPermissionModel
        {
            get { return _uRPermissionModel; }
            set
            {
                if (_uRPermissionModel != value)
                {
                    _uRPermissionModel = value;
                    OnPropertyChanged(() => URPermissionModel);
                }
            }
        }

        private ObservableCollection<rpt_PermissionModel> _uRPermissionModelCollection;
        /// <summary>
        /// Set or get UserModel Collection
        /// </summary>
        public ObservableCollection<rpt_PermissionModel> URPermissionModelCollection
        {
            get { return _uRPermissionModelCollection; }
            set
            {
                if (_uRPermissionModelCollection != value)
                {
                    _uRPermissionModelCollection = value;
                    OnPropertyChanged(() => URPermissionModelCollection);
                }
            }
        }        
        #endregion

        #region -User Menu Permission model-
        private rpt_PermissionModel _uMPermissionModel;
        /// <summary>
        /// Set or get User Menu Model
        /// </summary>
        public rpt_PermissionModel UMPermissionModel
        {
            get { return _uMPermissionModel; }
            set
            {
                if (_uMPermissionModel != value)
                {
                    _uMPermissionModel = value;
                    OnPropertyChanged(() => UMPermissionModel);
                }
            }
        }

        private ObservableCollection<rpt_PermissionModel> _uMPermissionModelCollection;
        /// <summary>
        /// Set or get User Menu Model Collection
        /// </summary>
        public ObservableCollection<rpt_PermissionModel> UMPermissionModelCollection
        {
            get { return _uMPermissionModelCollection; }
            set
            {
                if (_uMPermissionModelCollection != value)
                {
                    _uMPermissionModelCollection = value;
                    OnPropertyChanged(() => UMPermissionModelCollection);
                }
            }
        }
        #endregion

        #region -User Group Permission model-
        private rpt_PermissionModel _uGPermissionModel;
        /// <summary>
        /// Set or get User Group
        /// </summary>
        public rpt_PermissionModel UGPermissionModel
        {
            get { return _uGPermissionModel; }
            set
            {
                if (_uGPermissionModel != value)
                {
                    _uGPermissionModel = value;
                    OnPropertyChanged(() => UGPermissionModel);
                }
            }
        }

        private ObservableCollection<rpt_PermissionModel> _uGPermissionModelCollection;
        /// <summary>
        /// Set or get User Group Model Collection
        /// </summary>
        public ObservableCollection<rpt_PermissionModel> UGPermissionModelCollection
        {
            get { return _uGPermissionModelCollection; }
            set
            {
                if (_uGPermissionModelCollection != value)
                {
                    _uGPermissionModelCollection = value;
                    OnPropertyChanged(() => UGPermissionModelCollection);
                }
            }
        }
        #endregion

        #endregion

        #region -Defines-
        public View.PermissionView  PermissionView { get; set; }

        rpt_UserModel UserModelStore;
        ObservableCollection<rpt_PermissionModel> lstURPermissionStore;
        ObservableCollection<rpt_PermissionModel> lstUMPermissionStore;
        ObservableCollection<rpt_PermissionModel> lstUGPermissionStore;

        rpt_UserRepository userRepo = new rpt_UserRepository();
        rpt_PermissionRepository permissionRepo = new rpt_PermissionRepository();
        rpt_DepartmentRepository departRepo = new rpt_DepartmentRepository();
        rpt_ReportRepository reportRepo = new rpt_ReportRepository();
        rpt_GroupRepository groupRepo = new rpt_GroupRepository();
        base_GenericCodeRepository genericRepo = new base_GenericCodeRepository();

        List<rpt_PermissionModel> crrListURPermission;
        List<rpt_PermissionModel> crrListUMPermission;
        List<rpt_PermissionModel> crrListUGPermission;
        bool isSave = false;
        bool isClone = false;
        string resource = string.Empty;
        string oldLoginName = string.Empty;
        // First user in database
        bool isFirstUser = true;
        #endregion        
        
        #region -Command-

        #region -Init all Command-

        /// <summary>
        /// Init all command
        /// </summary>
        private void InitCommand()
        {
            RowSelectionChangedCommand = new RelayCommand(RowSelectionChangedExecute, CanRowSelectionChangedExecute);
            CurrentCellChangedCommand = new RelayCommand(CurrentCellChangedExecute, CanCellChangedExecute);
            CheckAllPrintCommand = new RelayCommand<object>(CheckAllPrintExecute, CanCheckAllPrintExecute);
            CheckIsPrintCommand = new RelayCommand<object>(CheckIsPrintExecute, CanCheckIsPrintExecute);            
            CheckAllViewCommand = new RelayCommand<object>(CheckAllViewExecute, CanCheckAllViewExecute);            
            CheckIsViewCommand = new RelayCommand<object>(CheckIsViewExecute, CanCheckIsViewExecute);
            CheckAllRightCommand = new RelayCommand<object>(CheckAllRightExecute, CanCheckAllRighttExecute);
            CheckRightCommand = new RelayCommand<object>(CheckRightExecute, CanCheckRightExecute);
            CheckGroupRightCommand = new RelayCommand<object>(CheckGroupRightExecute, CanCheckGroupRightExecute);
            SetPasswordToDefaultCommand = new RelayCommand(SetPasswordToDefaultExecute);
            NewCommand = new RelayCommand(NewExecute);
            SaveCommand = new RelayCommand(SaveCommandExecute, CanSaveCommandExecute);
            ApplyUserLevelCommand = new RelayCommand(ApplyUserLevelExecute, CanApplyUserLevelExecute);
            CloneCommand = new RelayCommand(CloneUserExecute, CanCloneUserExecute);
            DeleteCommand = new RelayCommand(DeleteExecute, CanDeleteExecute);
            CancelCommand = new RelayCommand(CancelExecute, CanCancelExecute);
            CloseCommand = new RelayCommand(CloseExecute);
        }
        #endregion

        #region -Set Password To Default Command-
        public RelayCommand SetPasswordToDefaultCommand { get; set; }
        /// <summary>
        /// Set Password To Default 
        /// </summary>
        public void SetPasswordToDefaultExecute()
        {
            if (UserModel != null)
            {
                // Set password
                UserModel.ConfirmPassword = UserModel.Password = (IsSetPasswordToDefault == true) ? Common.PWD_TEMP : string.Empty;
            }
        }
        #endregion        

        #region -New Command-

        /// <summary>
        /// Set or Get New Command
        /// </summary>
        public RelayCommand NewCommand { get; private set; }

        private void NewExecute()
        {            
            //UserModel = null;
            // Create new User 
            UserModel = new rpt_UserModel();
            UserModel.Resource = Guid.NewGuid();                      
            ClonePermission(false);
            IsSetPasswordToDefault = false;
            IsCheckRight = false;
            IsCheckPrint = false;
            IsCheckView = false;  
            FocusDefault = false;
            FocusDefault = true;            
        }
        #endregion        

        #region -Save Command-
        /// <summary>
        /// Set or get Save command
        /// </summary>
        public RelayCommand SaveCommand { get; set; }

        public void SaveCommandExecute()
        {
            try
            {
                if (!isClone && !isFirstUser)
                {
                    // Update User Menu permission
                    int count = UMPermissionModelCollection.Count;
                    int i = 0;
                    for (i = 0; i < count; i++)
                    {
                        if (UMPermissionModelCollection[i].IsDirty)
                        {
                            UMPermissionModelCollection[i].ToEntity();
                        }
                    }
                    permissionRepo.Commit();
                    // Update User group permission
                    count = UGPermissionModelCollection.Count;
                    for (i = 0; i < count; i++)
                    {
                        if (UGPermissionModelCollection[i].IsDirty)
                        {
                            UGPermissionModelCollection[i].ToEntity();
                        }
                    }
                    permissionRepo.Commit();
                    // Update User report permission
                    count = URPermissionModelCollection.Count;
                    for (i = 0; i < count; i++)
                    {
                        if (URPermissionModelCollection[i].IsDirty)
                        {
                            URPermissionModelCollection[i].ToEntity();
                        }
                    }
                    permissionRepo.Commit();
                } 
                isSave = false;
                // Save permission
                SaveData();
                isFirstUser = false;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Check can execute Save user and permission
        /// </summary>
        /// <returns></returns>
        public bool CanSaveCommandExecute()
        {
            if (UserModel != null && UserModel.Errors.Count == 0)
            {
                return (isSave || UserModel.IsDirty);
            }
            return false;
        }
        #endregion

        #region -ApplyUserLevelCommand-

        public RelayCommand ApplyUserLevelCommand { get; set; }

        public void ApplyUserLevelExecute()
        {
            switch (UserModel.LevelId)
            {                 
                case 1:
                    #region -Set View Report-
                    // Set User Report permission right                    
                    foreach (rpt_PermissionModel urp in URPermissionModelCollection)
                    {
                        if (urp.IsView != true)
                        {
                            urp.IsView = true;
                        }
                        if (urp.IsPrint != false)
                        {
                            urp.IsPrint = false;
                        }
                    }
                    // Set User Menu permission right
                    foreach (rpt_PermissionModel ump in UMPermissionModelCollection)
                    {
                        if (ump.Code == "M03" || ump.Code == "M02")
                        {
                            if (ump.Right != true)
                            {
                                ump.Right = true;
                            }
                        }
                        else
                        {
                            if (ump.Right != false)
                            {
                                ump.Right = false;
                            }
                        }
                    }
                    // Set User Group permission right
                    foreach (rpt_PermissionModel ump in UGPermissionModelCollection)
                    {
                        if (ump.Right != true)
                        {
                            ump.Right = true;
                        }
                    }
                    break;
                    #endregion
                case 2:
                    #region -Set Print Report permission-
                    // Set User Report permission right                    
                    foreach (rpt_PermissionModel urp in URPermissionModelCollection)
                    {
                        if (urp.IsView != true)
                        {
                            urp.IsView = true;
                        }
                        if (urp.IsPrint != true)
                        {
                            urp.IsPrint = true;
                        }
                    }
                    // Set User Menu permission right
                    foreach (rpt_PermissionModel ump in UMPermissionModelCollection)
                    {
                        if (ump.Code == "M03" || ump.Code == "M02")
                        {
                            if (ump.Right != true)
                            {
                                ump.Right = true;
                            }
                        }
                        else
                        {
                            if (ump.Right != false)
                            {
                                ump.Right = false;
                            }
                        }
                    }
                    // Set User Group permission right
                    foreach (rpt_PermissionModel ump in UGPermissionModelCollection)
                    {
                        if (ump.Right != true)
                        {
                            ump.Right = true;
                        }
                    }
                    break;
                    #endregion
                case 3:
                    #region -Set Print Report permission-
                    // Set User Report permission right                    
                    foreach (rpt_PermissionModel urp in URPermissionModelCollection)
                    {
                        if (urp.IsView != true)
                        {
                            urp.IsView = true;
                        }
                        if (urp.IsPrint != true)
                        {
                            urp.IsPrint = true;
                        }
                    }
                    // Set User Menu permission right
                    foreach (rpt_PermissionModel ump in UMPermissionModelCollection)
                    {
                        if (ump.Code == "M17")
                        {
                            if (ump.Right != false)
                            {
                                ump.Right = false;
                            }
                        }
                        else
                        {
                            if (ump.Right != true)
                            {
                                ump.Right = true;
                            }
                        }
                    }
                    // Set User Group permission right
                    foreach (rpt_PermissionModel ump in UGPermissionModelCollection)
                    {
                        if (ump.Right != true)
                        {
                            ump.Right = true;
                        }
                    }
                    break;
                    #endregion
                case 4:
                    #region -Manage all-
                    // Set User Report permission right                    
                    foreach (rpt_PermissionModel urp in URPermissionModelCollection)
                    {
                        if (urp.IsView != true)
                        {
                            urp.IsView = true;
                        }
                        if (urp.IsPrint != true)
                        {
                            urp.IsPrint = true;
                        }
                    }
                    // Set User Menu permission right
                    foreach (rpt_PermissionModel ump in UMPermissionModelCollection)
                    {
                        if (ump.Right != true)
                        {
                            ump.Right = true;
                        }
                    }
                    // Set User Group permission right
                    foreach (rpt_PermissionModel ump in UGPermissionModelCollection)
                    {
                        if (ump.Right != true)
                        {
                            ump.Right = true;
                        }
                    }                    
                    break;
                    #endregion
                default:
                    #region -Set default right-
                    // Set User Report permission right                    
                    foreach (rpt_PermissionModel urp in URPermissionModelCollection)
                    {
                        if (urp.IsView != false)
                        {
                            urp.IsView = false;
                        }
                        if (urp.IsPrint != false)
                        {
                            urp.IsPrint = false;
                        }
                    }
                    // Set User Menu permission right
                    foreach (rpt_PermissionModel ump in UMPermissionModelCollection)
                    {
                        if (ump.Right != false)
                        {
                            ump.Right = false;
                        }
                    }
                    // Set User Group permission right
                    foreach (rpt_PermissionModel ump in UGPermissionModelCollection)
                    {
                        if (ump.Right != false)
                        {
                            ump.Right = false;
                        }
                    }
                    break;
                    #endregion
            }
            CheckAllReportView_Print();
            CheckAllMenuRight();
        }

        public bool CanApplyUserLevelExecute()
        {
            return (UserModel != null && UserModel.IsDirty);
        }
        #endregion

        #region -Clone Command-
        public RelayCommand CloneCommand { get; set; }
        /// <summary>
        /// Execute Clone user command
        /// </summary>
        public void CloneUserExecute()
        {            
            var usertem = UserModel;
            resource = UserModel.Resource.ToString();
            UserModel = null;
            // Create new user 
            UserModel = new rpt_UserModel();
            UserModel.Resource = Guid.NewGuid();
            // Clone user information from selected user
            UserModel.UserName = string.Empty;
            UserModel.LoginName = string.Empty;
            UserModel.Password = string.Empty;
            UserModel.CreatedDate = DateTime.Now;
            UserModel.DepartId = usertem.DepartId;
            UserModel.Position = usertem.Position;
            UserModel.LevelId = usertem.LevelId;
            UserModel.IsActive = usertem.IsActive;
            ClonePermission(true);
            isClone = true;
            IsSetPasswordToDefault = false;
        }
        
        /// <summary>
        /// Check can clone user execute
        /// </summary>
        /// <returns></returns>
        public bool CanCloneUserExecute()
        {
            return (UserModel != null && UserModel.Errors.Count == 0 && !UserModel.IsDirty && !isSave);
        }
        #endregion

        #region -Delete Command-
        /// <summary>
        /// Set or get Delete command
        /// </summary>
        public RelayCommand DeleteCommand { get; set; }
        /// <summary>
        /// Execute delete user
        /// </summary>
        public void DeleteExecute()
        {
            isSave = false;
            System.Windows.MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show(
                                "Do you want to delete?", "Delete",
                                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning
                            );
            if (resuilt.Equals(System.Windows.MessageBoxResult.Yes))
            {
                try
                {
                    // Delete User
                    userRepo.Delete(UserModel.rpt_User);
                    userRepo.Commit();
                    UserModelCollection.Remove(UserModel);
                    if (UserModelCollection.Count > 0)
                    {
                        UserModel = UserModelCollection[0];
                    }
                    else
                    {
                        // Remove User report permission
                        if (URPermissionModelCollection != null && URPermissionModelCollection.Count > 0)
                        {
                            foreach (rpt_PermissionModel per in URPermissionModelCollection)
                            {
                                per.IsPrint = per.IsView = false;
                                per.EndUpdate();
                            }
                            CheckAllReportView_Print();
                        }
                        // Remove User menu permission
                        if (UMPermissionModelCollection != null && UMPermissionModelCollection.Count > 0)
                        {
                            foreach (rpt_PermissionModel per in UMPermissionModelCollection)
                            {
                                per.Right = false;
                                per.EndUpdate();
                            }
                            CheckAllMenuRight();
                        }
                        // Remove User group permission
                        if (UGPermissionModelCollection != null && UGPermissionModelCollection.Count > 0)
                        {
                            foreach (rpt_PermissionModel per in UGPermissionModelCollection)
                            {
                                per.Right = false;
                                per.EndUpdate();
                            }
                        }
                        isFirstUser = true;
                    }
                }
                catch (Exception ex)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Delete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }                
            }            
        }
        /// <summary>
        /// Check can delete User execute
        /// </summary>
        /// <returns></returns>
        public bool CanDeleteExecute()
        {
            return (UserModel != null && !UserModel.IsDirty && UserModel.Errors.Count==0 && UserModel.LoginName != Common.LOGIN_NAME);
        }
        #endregion

        #region -Cancel Command-
        /// <summary>
        /// Set or get Delete command
        /// </summary>
        public RelayCommand CancelCommand { get; set; }
        /// <summary>
        /// Execute Canel command
        /// </summary>
        public void CancelExecute()
        {
            if (UserModelCollection.Count > 0)
            {
                // Restore all user permission
                if (isSave || isClone)
                {
                    isSave = false;
                    isClone = false;
                    RestoreListURModel();
                    RestoreListUMModel();
                    CheckAllReportView_Print();
                    CheckAllMenuRight();
                }
                // Restore last selected User
                RestoreUserModel();
            }
            else
            {
                UserModel = null;
                IsSetPasswordToDefault = false;
            }
        }
        /// <summary>
        /// Check can Cancel Execute
        /// </summary>
        /// <returns></returns>
        public bool CanCancelExecute()
        {
            return isSave || (UserModel != null  && (UserModel.IsDirty || UserModel.IsNew));
        }
        #endregion

        #region -Close command-
        public RelayCommand CloseCommand { get; set; }

        public void CloseExecute()
        {
            if (isSave || (UserModel != null && UserModel.IsDirty && !isClone))
            {
                System.Windows.MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show(
                        "Do you want to save changes?", "Save",
                        System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning
                    );
                if (resuilt.Equals(System.Windows.MessageBoxResult.Yes))
                {
                    if (CanSaveCommandExecute())
                    {
                        // Save changes
                        SaveCommandExecute();
                    }
                    else
                    {
                        return;
                    }
                }
                else if (resuilt.Equals(System.Windows.MessageBoxResult.Cancel))
                {
                    return;
                }
                //isSave = false;
                //UserModel.EndUpdate();
            }
            PermissionView.Close();
        }
        #endregion

        #region -Row Selection Changed Command-

        public RelayCommand RowSelectionChangedCommand { get; set; }
        /// <summary>
        /// Execute save changes or restore data
        /// </summary>
        public void RowSelectionChangedExecute()
        {
            try
            {
                GetAllPermission();                     
                CheckAllReportView_Print();
                CheckAllMenuRight();
                isSave = false;
                // Store all permission
                StoreListUMModel();
                StoreListURModel();
                // Store user
                StoreUserModel();
                oldLoginName = UserModel.LoginName;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Check can Row selection changed execute
        /// </summary>
        /// <returns></returns>
        public bool CanRowSelectionChangedExecute()
        {
            return (UserModel != null);
        }
        #endregion

        #region -Current Cell Changed Command-

        /// <summary>
        /// Current Cell Changed Command
        /// </summary>
        public RelayCommand CurrentCellChangedCommand { get; set; }
        /// <summary>
        /// Cell changed execute
        /// </summary>
        public void CurrentCellChangedExecute()
        {
            if (UserModel.IsNew)
            {
                return;
            }
            if (UserModel.Errors.Count != 0)
            { 
                // Restore all permission
                RestoreListUMModel();
                RestoreListURModel();
                RestoreUserModel();
                isSave = false;
                return;
            }   
            System.Windows.MessageBoxResult resuilt = Xceed.Wpf.Toolkit.MessageBox.Show(
                        "Do you want to save changes?", "Save",
                        System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning
                    );
            if (resuilt.Equals(System.Windows.MessageBoxResult.Yes))
            {
                // Save changes
                SaveData();
            }
            else if (resuilt.Equals(System.Windows.MessageBoxResult.No))
            {
                // Restore all permission
                RestoreListUMModel();
                RestoreListURModel();
                RestoreUserModel();
            }
            isSave = false;
        }
        /// <summary>
        /// Check can Cell changed execute
        /// </summary>
        /// <returns></returns>
        private bool CanCellChangedExecute()
        {
            return (isSave || (UserModel != null && UserModel.IsDirty && !isClone));
        }
        #endregion

        #region -Check ALL Print Command-
        public RelayCommand<object> CheckAllPrintCommand { get; set; }

        public void CheckAllPrintExecute(object obj)
        {
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            int count = URPermissionModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (URPermissionModelCollection[i].IsPrint != check.IsChecked.Value)
                {
                    URPermissionModelCollection[i].IsPrint = check.IsChecked.Value;
                }
            }
            isSave = true;
        }

        public bool CanCheckAllPrintExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #region -Check Is Print Command-
        public RelayCommand<object> CheckIsPrintCommand { get; set; }

        public void CheckIsPrintExecute(object obj)
        {
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            if (IsCheckPrint)
            {
                if (!check.IsChecked.Value)
                {
                    IsCheckPrint = false;
                }
            }
            else
            {
                IsCheckPrint = true;
                int count = URPermissionModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!(bool)URPermissionModelCollection[i].IsPrint)
                    {
                        IsCheckPrint = false;
                        break;
                    }
                }
            }
            isSave = true;
        }

        public bool CanCheckIsPrintExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #region -Check All View Command-
        public RelayCommand<object> CheckAllViewCommand { get; set; }

        public void CheckAllViewExecute(object obj)
        {
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            int count = URPermissionModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (URPermissionModelCollection[i].IsView != check.IsChecked.Value)
                {
                    URPermissionModelCollection[i].IsView = check.IsChecked.Value;
                }
            }
            isSave = true;
        }

        public bool CanCheckAllViewExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion        

        #region -Check is View Command-
        public RelayCommand<object> CheckIsViewCommand { get; set; }

        public void CheckIsViewExecute(object obj)
        {
            if (IsCheckView)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                if (!check.IsChecked.Value)
                {
                    IsCheckView = false;
                }
            }
            else
            {
                IsCheckView = true;
                int count = URPermissionModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!(bool)URPermissionModelCollection[i].IsView)
                    {
                        IsCheckView = false;
                        break;
                    }
                }
            }
            isSave = true;
        }

        public bool CanCheckIsViewExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #region -Check ALL Right Command-
        public RelayCommand<object> CheckAllRightCommand { get; set; }

        public void CheckAllRightExecute(object obj)
        {            
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            int count = UMPermissionModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (UMPermissionModelCollection[i].Right != check.IsChecked.Value)
                {
                    UMPermissionModelCollection[i].Right = check.IsChecked.Value;                    
                }
            }
            isSave = true;
        }

        public bool CanCheckAllRighttExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #region -Check right Command-
        public RelayCommand<object> CheckRightCommand { get; set; }

        public void CheckRightExecute(object obj)
        {            
            if (IsCheckRight)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                if (!check.IsChecked.Value)
                {
                    IsCheckRight = false;
                }
            }
            else
            {
                IsCheckRight = true;
                int count = UMPermissionModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!(bool)UMPermissionModelCollection[i].Right)
                    {
                        IsCheckRight = false;
                        break;
                    }
                }
            }
            isSave = true;
        }

        public bool CanCheckRightExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #region -Check Group Right Command-
        public RelayCommand<object> CheckGroupRightCommand { get; set; }
        /// <summary>
        /// Check Group right execute
        /// </summary>
        /// <param name="obj"></param>
        public void CheckGroupRightExecute(object obj)
        { 
            isSave = true;
        }

        public bool CanCheckGroupRightExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                return check.IsChecked.HasValue;
            }
            return false;
        }
        #endregion

        #endregion

        #region -Methods-

        #region -Refresh Repository-
        /// <summary>
        /// Refresh Repository
        /// </summary>
        private void RefreshRepository()
        {
            userRepo.Refresh();            
            permissionRepo.Refresh();
        }
        #endregion

        #region -Check is duplicate login name-
        /// <summary>
        /// Check is duplicate Login Name
        /// Return: 
        ///     true: duplicate Login Name
        ///     false: not duplicate
        /// </summary>
        /// <returns></returns>
        private bool CheckDuplicateLoginName()
        {
            if (!string.IsNullOrWhiteSpace(UserModel.LoginName) && !string.IsNullOrWhiteSpace(UserModel.UserName))
            {
                var resuilt = userRepo.Get(x => x.LoginName.Equals(UserModel.LoginName, StringComparison.OrdinalIgnoreCase));
                return (resuilt != null);
            }
            return false;
        }
        #endregion

        #region -Save User Report Permission and User Menu Permission-
        /// <summary>
        /// Save User Report Permission and User Menu Permission
        /// </summary>
        private void SaveData()
        {
            if (UserModel.IsNew || UserModel.IsDirty && oldLoginName != "" && UserModel.LoginName != oldLoginName)
            {
                // Check duplicate login name
                if (CheckDuplicateLoginName())
                {
                    MessageBox.Show("Login Name is duplicate!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }
            if (UserModel.IsNew)
            {
                
                // Save new User 
                SaveNewUser(); 
            }
            else if (UserModel.IsDirty)
            {
                // Upadate user and permission
                UpdateUser();
            }
            // Check major group permission to current login name
            if (UserModel.LoginName == Common.LOGIN_NAME)
            {
                // Clear list major group
                Common.LST_GROUP.Clear();
                foreach (var group in UGPermissionModelCollection)
                {
                    if ((bool)group.Right)
                    {
                         Common.LST_GROUP.Add(group.Code);                         
                    }
                }
                Common.IS_CHANGE_MAJOR_GROUP = true;
            }
            oldLoginName = UserModel.LoginName;
            UserModel.EndUpdate();
        }

        #region -Save new user-
        /// <summary>
        /// Save new User
        /// </summary>
        private void SaveNewUser()
        {
            // Save User model
            UserModel.Password = AESSecurity.Encrypt(UserModel.Password);
            UserModel.StorePassword = UserModel.Password;
            userRepo.Add(UserModel.rpt_User);
            UserModel.ToEntity();
            userRepo.Commit();
            UserModel.ToModel();
            // Update new user right
            UpdateNewUserRight();
            // Get permission
            GetAllPermission();
        }
        #endregion

        #region -Update user and Permission-
        /// <summary>
        /// Update user 
        /// </summary>
        private void UpdateUser()
        {
            // Get password
            UserModel.Password = (UserModel.Password != Common.PWD_TEMP) ? AESSecurity.Encrypt(UserModel.Password) : UserModel.StorePassword;                        
            UserModel.ToEntity();
            // update user
            userRepo.Commit();
            bool isSaveSettings = false;            
            if (Common.LOGIN_NAME == UserModel.LoginName)
            {
                if (UserModel.LoginName != Settings.Default.LoginName || UserModel.Password != Common.PWD_TEMP)
                {
                    isSaveSettings = true;
                }
            }
            if (isSaveSettings && Settings.Default.IsRemember)
            {
                // Clear password and login name 
                Settings.Default.LoginName = string.Empty;
                Settings.Default.Password = string.Empty;
                Settings.Default.IsRemember = false;
                Settings.Default.Save();
            }
            // Store password to process
            UserModel.StorePassword = UserModel.Password;
            // Show password temporary
            UserModel.ConfirmPassword = UserModel.Password = Common.PWD_TEMP;  
        }
        #endregion

        #region -Update new user right-
        /// <summary>
        /// Update new user right
        /// </summary>
        private void UpdateNewUserRight()
        {
            // Get all permission by User resource
            var permissions = new ObservableCollection<rpt_PermissionModel>(
                    permissionRepo.GetAll()
                    .Select(u => new rpt_PermissionModel(u))
                    .Where(w => w.UserResource == UserModel.Resource.ToString())       
                ).ToList();
            var uRPermissions = new ObservableCollection<rpt_PermissionModel>(
                    permissions.Where(w => w.Type == 1)
                    .OrderBy(o => o.Code)
                );
            // Get User menu permissiom
            var uMPermissions = new ObservableCollection<rpt_PermissionModel>(
                        permissions.Where(w => w.Type == 2)
                        .OrderBy(o => o.Code)
                    );
            // Get User group permission
            var uGPermissions = new ObservableCollection<rpt_PermissionModel>(
                        permissions.Where(w => w.Type == 0)
                        .OrderBy(o=> o.Code)
                    );
            // Update permission if clone user
            int rowCount = crrListUMPermission.Count;
            int i = 0;
            int j = 0;
            // Update User menu permission
            for (i = 0; i < rowCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    if (uMPermissions[i].Code == crrListUMPermission[j].Code)
                    {
                        uMPermissions[i].Right = crrListUMPermission[j].Right;
                        uMPermissions[i].ToEntity();
                        break;
                    }
                }
            }
            // Update User group permission
            rowCount = crrListUGPermission.Count;
            for (i = 0; i < rowCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    if (uGPermissions[i].Code == crrListUGPermission[j].Code)
                    {
                        uGPermissions[i].Right = crrListUGPermission[j].Right;
                        uGPermissions[i].ToEntity();
                        break;
                    }
                }
            }
            // Update User report permission
            rowCount = crrListURPermission.Count;
            for (i = 0; i < rowCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    if (uRPermissions[i].Code == crrListURPermission[j].Code)
                    {
                        uRPermissions[i].IsPrint = crrListURPermission[j].IsPrint;
                        uRPermissions[i].IsView = crrListURPermission[j].IsView;
                        uRPermissions[i].ToEntity();
                        break;
                    }
                }
            }
            // Save changes all User Report, Menu and Group permission
            permissionRepo.Commit();
            isClone = false;
            if (IsSetPasswordToDefault)
            {
                IsSetPasswordToDefault = false;
            }
            // Reset to display on window
            UserModel.ConfirmPassword = UserModel.Password = Common.PWD_TEMP;
            UserModel.EndUpdate();
            // Add new user to current user list
            UserModelCollection.Add(UserModel);
        }
        #endregion

        #endregion

        #region -Clone Permission-
        /// <summary>
        /// Clone Permission.
        /// </summary>
        /// <param name="isClone"></param>
        private void ClonePermission(bool isClone)
        {
            crrListURPermission = new List<rpt_PermissionModel>();
            rpt_PermissionModel uRPermissionModel;
            crrListUMPermission = new List<rpt_PermissionModel>();
            rpt_PermissionModel uMPermissionModel;
            crrListUGPermission = new List<rpt_PermissionModel>();
            if (URPermissionModelCollection != null && URPermissionModelCollection.Count > 0)
            {                
                // Save current User Report permission
                foreach (rpt_PermissionModel uRPermission in URPermissionModelCollection)
                {
                    uRPermissionModel = new rpt_PermissionModel();
                    uRPermissionModel.Code = uRPermission.Code;
                    uRPermissionModel.UserResource = UserModel.Resource.ToString();
                    uRPermissionModel.IsPrint = (isClone == true) ? uRPermission.IsPrint : false;
                    uRPermissionModel.IsView = (isClone == true) ? uRPermission.IsView : false;
                    crrListURPermission.Add(uRPermissionModel);
                }
                URPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(crrListURPermission.OrderBy(o => o.Code));
                SetURPermission();
            }
            if (UMPermissionModelCollection != null && UMPermissionModelCollection.Count > 0)
            {                
                // Save current User Menu permission           
                foreach (rpt_PermissionModel uMPermission in UMPermissionModelCollection)
                {
                    uMPermissionModel = new Model.rpt_PermissionModel();
                    uMPermissionModel.Code = uMPermission.Code;
                    uMPermissionModel.UserResource = UserModel.Resource.ToString();
                    uMPermissionModel.Right = (isClone == true) ? uMPermission.Right : false;
                    crrListUMPermission.Add(uMPermissionModel);
                }
                UMPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(crrListUMPermission.OrderBy(o => o.Code));
                SetUMPermission();
            }
            if (UGPermissionModelCollection != null && UGPermissionModelCollection.Count > 0)
            {                
                // Save current User Group permission
                foreach (rpt_PermissionModel uGPermission in UGPermissionModelCollection)
                {
                    uMPermissionModel = new Model.rpt_PermissionModel();
                    uMPermissionModel.Code = uGPermission.Code;
                    uMPermissionModel.UserResource = UserModel.Resource.ToString();
                    uMPermissionModel.Right = (isClone == true) ? uGPermission.Right : false;
                    crrListUGPermission.Add(uMPermissionModel);
                }
                UGPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(crrListUGPermission.OrderBy(o => o.Code));
                SetUGPermission();
            }                                    
        }
        #endregion

        #region -Load data-

        /// <summary>
        /// Load Major group, User and Permission
        /// </summary>
        private void LoadData()
        {
            try
            {               
                GetDepart_Level();
                GetAllUser();
                GetAllPermissionName();
                GetAllPermission();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Get all Permission name
        /// </summary>
        private void GetAllPermissionName()
        {
            // Get all report collection
            ReportCollection = new ObservableCollection<rpt_ReportModel>(
                    reportRepo.GetAll()
                    .Select(r => new rpt_ReportModel(r))
                    .Where(w => w.ParentId != 0)
                    .OrderBy(o => o.Code)
                );
            // Get all Menu collection
            MenuCollection = new ObservableCollection<base_GenericCodeModel>(
                    genericRepo.GetAll()
                    .Select(m => new base_GenericCodeModel(m))
                    .Where(w => w.Type == "MR")
                    .OrderBy(o => o.Code)
                );
            // Get all group collection
            GroupCollection = new ObservableCollection<rpt_GroupModel>(
                    groupRepo.GetAll()
                    .Select(g => new rpt_GroupModel(g))
                    .OrderBy(o => o.Code)
                );
        }

        /// <summary>
        /// Get all Department model and Level model
        /// </summary>
        private void GetDepart_Level()
        {
            // Get all department and level
            var departLevel = new ObservableCollection<rpt_DepartmentModel>(
                    departRepo.GetAll().Select(d => new rpt_DepartmentModel(d))
                );
            // Get all department 
            DepartModelCollection = new ObservableCollection<rpt_DepartmentModel>(departLevel.Where(w => w.Type == "D"));
            // Load all Level
            LevelModelCollection = new ObservableCollection<rpt_DepartmentModel>(departLevel.Where(w => w.Type == "L"));
        }

        /// <summary>
        /// Get all user
        /// </summary>
        private void GetAllUser()
        {
            // Get all User 
            UserModelCollection = new ObservableCollection<rpt_UserModel>(
                    userRepo.GetAll()
                    .Select(u => new rpt_UserModel(u))
                    .OrderBy(o => o.UserName)
                );
            if (UserModelCollection.Count > 0)
            {
                // Selected first user in user list
                UserModel = UserModelCollection[0];
                oldLoginName = UserModel.LoginName;
                // Store first user's information
                StoreUserModel();
                isFirstUser = false;
            }
        }

        /// <summary>
        /// Get all User Report, Menu and Group Permission by user resource
        /// </summary>
        private void GetAllPermission()
        {
            List<rpt_PermissionModel> lstPermission = new List<rpt_PermissionModel>();
            if (UserModel != null)
            {
                // Get all Permission
                lstPermission = new ObservableCollection<rpt_PermissionModel>(
                        permissionRepo.GetAll()
                        .Select(u => new rpt_PermissionModel(u))
                        .Where(w => w.UserResource == UserModel.Resource.ToString())
                        .OrderBy(o => o.Type)
                    ).ToList();
            }
            // Get User report permission
            URPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                    lstPermission.Where(w => w.Type == 1)
                    .OrderBy(o=> o.Code)
                );
            SetURPermission();
            // Store list user report permisson
            StoreListURModel();
            CheckAllReportView_Print();

            // Get all User Menu permission
            UMPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                     lstPermission.Where(w => w.Type == 2)
                     .OrderBy(o => o.Code)
                );
            SetUMPermission();
            // Get all User Group permission
            UGPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                    lstPermission.Where(w => w.Type == 0)
                    .OrderBy(o => o.Code)
                );
            SetUGPermission();
            // Store all User Menu and User Group Permission
            StoreListUMModel();
            CheckAllMenuRight();
        }

        private void SetURPermission()
        {
            int count = ReportCollection.Count;
            if (URPermissionModelCollection != null && URPermissionModelCollection.Count > 0)
            {                
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (ReportCollection[i].Code == URPermissionModelCollection[j].Code)
                        {
                            URPermissionModelCollection[i].Name = ReportCollection[j].Name;
                            URPermissionModelCollection[i].EndUpdate();
                            break;
                        }
                    }
                }                
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    rpt_PermissionModel permission = new rpt_PermissionModel();
                    permission.Type = 1;
                    permission.Code = ReportCollection[i].Code;
                    permission.Name = ReportCollection[i].Name;
                    URPermissionModelCollection.Add(permission);
                }
            }
            URPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                        URPermissionModelCollection.OrderBy(o => o.Name)
                    );
        }

        private void SetUMPermission()
        {
            int count = MenuCollection.Count;
            if (UMPermissionModelCollection != null && UMPermissionModelCollection.Count > 0)
            {                
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (MenuCollection[i].Code == UMPermissionModelCollection[j].Code)
                        {
                            UMPermissionModelCollection[i].Name = MenuCollection[i].Name;
                            UMPermissionModelCollection[i].EndUpdate();
                            break;
                        }
                    }
                }                
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    rpt_PermissionModel permission = new rpt_PermissionModel();
                    permission.Type = 2;
                    permission.Code = MenuCollection[i].Code;
                    permission.Name = MenuCollection[i].Name;
                    UMPermissionModelCollection.Add(permission);
                }
            }
            UMPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                        UMPermissionModelCollection.OrderBy(o => o.Name)
                    );
        }

        private void SetUGPermission()
        {
            int count = GroupCollection.Count;
            if (UGPermissionModelCollection != null && UGPermissionModelCollection.Count > 0)
            {                
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (GroupCollection[i].Code == UGPermissionModelCollection[j].Code)
                        {
                            UGPermissionModelCollection[i].Name = GroupCollection[i].Name;
                            UGPermissionModelCollection[i].EndUpdate();
                            break;
                        }
                    }
                }                
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    rpt_PermissionModel menuPermission = new rpt_PermissionModel();
                    menuPermission.Type = 2;
                    menuPermission.Code = GroupCollection[i].Code;
                    menuPermission.Name = GroupCollection[i].Name;
                    UGPermissionModelCollection.Add(menuPermission);
                }
            }
            UGPermissionModelCollection = new ObservableCollection<rpt_PermissionModel>(
                        UGPermissionModelCollection.OrderBy(o => o.Name)
                    );
        }

        private void CheckAllReportView_Print()
        {
            if (URPermissionModelCollection != null && URPermissionModelCollection.Count > 0)
            {
                IsCheckView = true;
                IsCheckPrint = true;
                int rowCount = URPermissionModelCollection.Count;
                bool viewBreak = false;
                bool printBreak = false;
                for (int i = 0; i < rowCount; i++)
                {
                    if (!(bool)URPermissionModelCollection[i].IsView && !viewBreak)
                    {
                        IsCheckView = false;
                        viewBreak = true;
                    }
                    if (!(bool)URPermissionModelCollection[i].IsPrint && !printBreak)
                    {
                        IsCheckPrint = false;
                        printBreak = true;
                    }
                    if (viewBreak && printBreak)
                    {
                        break;
                    }
                }
            }
        }

        private void CheckAllMenuRight()
        {
            if (UMPermissionModelCollection != null && UMPermissionModelCollection.Count > 0)
            {
                IsCheckRight = true;
                int rowCount = UMPermissionModelCollection.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    if (!(bool)UMPermissionModelCollection[i].Right)
                    {
                        IsCheckRight = false;
                        break;
                    }
                }
            }
        }

        #endregion

        #region -Store and restore UM & UR & UG permission-
        /// <summary>
        /// Store list User report permission
        /// </summary>
        private void StoreListURModel()
        {
            lstURPermissionStore = new ObservableCollection<rpt_PermissionModel>();
            int count = URPermissionModelCollection.Count;
            for (int i = 0; i < count; i++)
            {
                rpt_PermissionModel URPermission = new rpt_PermissionModel();                
                URPermission.IsPrint = URPermissionModelCollection[i].IsPrint;
                URPermission.IsView = URPermissionModelCollection[i].IsView;
                lstURPermissionStore.Add(URPermission);
            }
        }
        /// <summary>
        /// Restore list User report permission
        /// </summary>
        private void RestoreListURModel()
        {
            if (URPermissionModelCollection.Count > 0)
            {
                int count = URPermissionModelCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    URPermissionModelCollection[i].IsPrint = lstURPermissionStore[i].IsPrint;
                    URPermissionModelCollection[i].IsView = lstURPermissionStore[i].IsView;
                }
            }
        }
        /// <summary>
        /// Store list User menu permission
        /// </summary>
        private void StoreListUMModel()
        {
            lstUMPermissionStore = new ObservableCollection<rpt_PermissionModel>();
            int count = UMPermissionModelCollection.Count;
            int i = 0;
            for (i = 0; i < count; i++)
            {
                rpt_PermissionModel UMPermission = new rpt_PermissionModel();
                UMPermission.Right = UMPermissionModelCollection[i].Right;
                lstUMPermissionStore.Add(UMPermission);
            }

            lstUGPermissionStore = new ObservableCollection<rpt_PermissionModel>();
            count = UGPermissionModelCollection.Count;
            for (i = 0; i < count; i++)
            {
                rpt_PermissionModel UGPermission = new rpt_PermissionModel();
                UGPermission.Right = UGPermissionModelCollection[i].Right;
                lstUGPermissionStore.Add(UGPermission);
            }
        }
        /// <summary>
        /// Restore list User menu permission
        /// </summary>
        private void RestoreListUMModel()
        {
            if (UMPermissionModelCollection.Count > 0)
            {
                int count = lstUMPermissionStore.Count;
                for (int i = 0; i < count; i++)
                {
                    UMPermissionModelCollection[i].Right = lstUMPermissionStore[i].Right;
                }
            }
            if (UGPermissionModelCollection.Count > 0)
            {
                int count = lstUGPermissionStore.Count;
                for (int i = 0; i < count; i++)
                {
                    UGPermissionModelCollection[i].Right = lstUGPermissionStore[i].Right;
                }
            }
        }
        #endregion

        #region -Store and restore User Model-
        /// <summary>
        /// Store User Model-
        /// </summary>
        private void StoreUserModel()
        {
            UserModelStore = new rpt_UserModel();
            UserModelStore.UserName = UserModel.UserName;
            UserModelStore.LoginName = UserModel.LoginName;
            UserModelStore.Password = UserModel.Password;
            UserModelStore.Resource = UserModel.Resource;
            UserModelStore.CreatedDate = UserModel.CreatedDate;
            UserModelStore.ExpiryDate = UserModel.ExpiryDate;
            UserModelStore.DepartId = UserModel.DepartId;
            UserModelStore.Position = UserModel.Position;
            UserModelStore.LevelId = UserModel.LevelId;
            UserModelStore.IsActive = UserModel.IsActive;
        }
        /// <summary>
        /// Restore User Model
        /// </summary>
        private void RestoreUserModel()
        {
            if (UserModelStore != null)
            {
                UserModel.UserName = UserModelStore.UserName;
                UserModel.LoginName = UserModelStore.LoginName;
                UserModel.Resource = UserModelStore.Resource;
                UserModel.CreatedDate = UserModelStore.CreatedDate;
                UserModel.ExpiryDate = UserModelStore.ExpiryDate;
                UserModel.DepartId = UserModelStore.DepartId;
                UserModel.Position = UserModelStore.Position;
                UserModel.LevelId = UserModelStore.LevelId;
                UserModel.IsActive = UserModelStore.IsActive;
                UserModel.ConfirmPassword = UserModel.Password = Common.PWD_TEMP;
                UserModel.EndUpdate();
            }
        }
        #endregion
        #endregion        
    }
}
