using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.POSReport.Model;
using CPC.POSReport.Repository;
using Toolkit.Base;
using System.Collections.ObjectModel;
using CPC.POSReport.View;
using Toolkit.Command;
using CPC.POSReport.Function;

namespace CPC.POSReport.ViewModel
{
    class ReportPermissionViewModel : ViewModelBase
    {
        #region -Properties-
        
        #region -Permission Model-
        private rpt_PermissionModel _permissionModel;
        public rpt_PermissionModel PermissionModel
        {
            get { return _permissionModel; }
            set
            {
                if (_permissionModel != value)
                {
                    _permissionModel = value;
                    OnPropertyChanged(() => PermissionModel);
                }
            }
        }

        private ObservableCollection<rpt_PermissionModel> _permissionCollection;
        public ObservableCollection<rpt_PermissionModel> PermissionCollection
        {
            get { return _permissionCollection; }
            set 
            {
                if (_permissionCollection != value)
                {
                    _permissionCollection = value;
                    OnPropertyChanged(()=> PermissionCollection);
                }
            }
        }
        #endregion

        #region -User Model-
        private ObservableCollection<rpt_UserModel> _userCollection;
        public ObservableCollection<rpt_UserModel> UserCollection
        {
            get { return _userCollection; }
            set
            {
                if (_userCollection != value)
                {
                    _userCollection = value;
                    OnPropertyChanged(() => UserCollection);
                }
            }
        }
        #endregion
        
        #endregion

        #region -Defines-
        public ReportPermissionView AssignAuthorizeWindow { get; set; }
        rpt_UserRepository userRepo = new rpt_UserRepository();
        rpt_PermissionRepository permissionRepo = new rpt_PermissionRepository();
        #endregion

        public ReportPermissionViewModel(ReportPermissionView window, string reportCode)
        {
            InitCommand();
            GetAllUserPermission(reportCode);
            AssignAuthorizeWindow = window;
        }

        #region -private Methods-
        /// <summary>
        /// Get all user permission
        /// </summary>
        private void GetAllUserPermission(string reportCode)
        {
            try
            {
                userRepo.Refresh();
                UserCollection = new ObservableCollection<rpt_UserModel>(
                        userRepo.GetAll()
                        .Select(u => new rpt_UserModel(u))
                        .OrderBy(o=> o.Resource)
                    );
                permissionRepo.Refresh();
                var permissionList = new ObservableCollection<rpt_PermissionModel>(
                        permissionRepo.GetAll()
                        .Select(p => new rpt_PermissionModel(p))
                        .Where(w => w.Type == 1 && w.Code == reportCode)
                        .OrderBy(o=> o.UserResource)
                    );
                int count = UserCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    if (permissionList[i].UserResource == UserCollection[i].Resource.ToString())
                    {
                        permissionList[i].LoginName = UserCollection[i].LoginName;
                        permissionList[i].Name = UserCollection[i].UserName;
                        permissionList[i].EndUpdate();
                        continue;
                    }
                    for (int j = 0; j < count; j++)
                    {
                        if (permissionList[i].UserResource == UserCollection[j].Resource.ToString())
                        {
                            permissionList[i].LoginName = UserCollection[j].LoginName;
                            permissionList[i].Name = UserCollection[j].UserName;
                            permissionList[i].EndUpdate();
                            break;
                        }
                    }
                }
                PermissionCollection = new ObservableCollection<rpt_PermissionModel>(permissionList.OrderBy(o => o.LoginName));
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #region -Save User report permission-
        /// <summary>
        /// Save User Report Permission        
        /// </summary>
        private void SaveUserReportPermission()
        {
            int count = PermissionCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (PermissionCollection[i].IsDirty)
                {
                    PermissionCollection[i].ToEntity();
                    string userResource = Common.USER_RESOURCE.Replace("'", "");
                    if (PermissionCollection[i].UserResource == userResource)
                    {
                        Common.IS_VIEW = PermissionCollection[i].IsView;
                        Common.IS_PRINT = PermissionCollection[i].IsPrint;
                        Common.IS_RIGHT_CHANGE = true;
                    }
                }
            }
            permissionRepo.Commit();
        }
        #endregion

        #endregion

        #region -Command-
        private void InitCommand()
        {
            CheckIsPrintCommand = new RelayCommand<object>(CheckIsPrintExecute, CanCheckIsPrintExecute);
            CheckIsViewCommand = new RelayCommand<object>(CheckIsViewExecute, CanCheckIsViewExecute);
            GrantAllCommand = new RelayCommand(GrantCommandExecute);
            RevokeAllCommand = new RelayCommand(RevokeCommandExecute);
            CloseCommand = new RelayCommand(CloseExecute);
        }

        #endregion

        #region -Close command-
        public RelayCommand CloseCommand { get; set; }

        public void CloseExecute()
        {
            bool closeWindow = true;
            if (CheckIsSaveData())
            {
                System.Windows.MessageBoxResult isSave = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save changes?", "Save", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);
                if (System.Windows.MessageBoxResult.Yes.Equals(isSave))
                {
                    SaveUserReportPermission();
                }
                else if (System.Windows.MessageBoxResult.Cancel.Equals(isSave))
                {
                    closeWindow = false;
                }
            }
            if (closeWindow)
            {
                AssignAuthorizeWindow.Close();
            }
        }

        private bool CheckIsSaveData()
        {
            int count = PermissionCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (PermissionCollection[i].IsDirty)
                {
                    return true;
                }                
            }
            return false;
        }
        #endregion

        #region -Revoke All Command -
        public RelayCommand RevokeAllCommand { get; set; }

        public void RevokeCommandExecute()
        {
            int count = PermissionCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (PermissionCollection[i].IsView)
                {
                    PermissionCollection[i].IsView = false;
                }
                if (PermissionCollection[i].IsPrint)
                {
                    PermissionCollection[i].IsPrint = false;
                }
            }
        }
        #endregion

        #region -Grant All Command -
        public RelayCommand GrantAllCommand { get; set; }

        public void GrantCommandExecute()
        {
            int count = PermissionCollection.Count;
            for (int i = 0; i < count; i++)
            {
                if (!PermissionCollection[i].IsView)
                {
                    PermissionCollection[i].IsView = true;
                }
                if (!PermissionCollection[i].IsPrint)
                {
                    PermissionCollection[i].IsPrint = true;
                }
            }
        }
        #endregion

        #region -Check Is Print Command-
        public RelayCommand<object> CheckIsPrintCommand { get; set; }

        public void CheckIsPrintExecute(object obj)
        {
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            PermissionModel.IsPrint = check.IsChecked.Value;
        }

        public bool CanCheckIsPrintExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                if (check.IsChecked.HasValue)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region -Check is View Command-
        public RelayCommand<object> CheckIsViewCommand { get; set; }

        public void CheckIsViewExecute(object obj)
        {
            System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
            PermissionModel.IsView = check.IsChecked.Value;
        }

        public bool CanCheckIsViewExecute(object obj)
        {
            if (obj != null)
            {
                System.Windows.Controls.CheckBox check = obj as System.Windows.Controls.CheckBox;
                if (check.IsChecked.HasValue)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion        

    }
}