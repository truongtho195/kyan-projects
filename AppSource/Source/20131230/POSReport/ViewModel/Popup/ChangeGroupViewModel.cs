using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;
using System.Windows.Input;
using Toolkit.Command;
using CPC.POSReport.View;
using CPC.POSReport.Model;
using CPC.POSReport.Repository;
using System.Collections.ObjectModel;

namespace CPC.POSReport.ViewModel
{
    class ChangeGroupViewModel : ViewModelBase
    {
        #region -Properties- 

        #region -Group report-
        private rpt_GroupModel _groupReportModel;
        /// <summary>
        /// Set or get Group Report model
        /// </summary>
        public rpt_GroupModel GroupReportModel
        {
            get { return _groupReportModel; }
            set
            {
                if (_groupReportModel != value)
                {
                    _groupReportModel = value;
                    OnPropertyChanged(() => GroupReportModel);
                }
            }
        }

        private ObservableCollection<rpt_GroupModel> _groupReportCollection ;
        /// <summary>
        /// Set or get GroupReportCollection
        /// </summary>
        public ObservableCollection<rpt_GroupModel> GroupReportCollection
        {
            get { return _groupReportCollection; }
            set 
            {
                if (_groupReportCollection != value)
                {
                    _groupReportCollection = value;
                    OnPropertyChanged(()=> GroupReportCollection);
                }
            }
        }
        
        #endregion

        #region -Report-
        public rpt_ReportModel Report { get; set; }

        private rpt_ReportModel _reportModel;
        /// <summary>
        /// Set or get Report model
        /// </summary>
        public rpt_ReportModel ReportModel
        {
            get { return _reportModel; }
            set
            {
                if (_reportModel != value)
                {
                    _reportModel = value;
                    OnPropertyChanged(() => ReportModel);
                }
            }
        }

        private ObservableCollection<rpt_ReportModel> _reportCollection;
        /// <summary>
        /// Set or get ReportCollection
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

        private ObservableCollection<rpt_ReportModel> _allReportCollection;
        /// <summary>
        /// Set or get ReportCollection
        /// </summary>
        public ObservableCollection<rpt_ReportModel> AllReportCollection
        {
            get { return _allReportCollection; }
            set
            {
                if (_allReportCollection != value)
                {
                    _allReportCollection = value;
                }
            }
        }
        #endregion
        #endregion

        #region -Defines-
        private ChangeGroupView ChangeGroupView { get; set; }
        private MainViewModel MainVM { get; set; }
        rpt_ReportRepository reportRepo = new rpt_ReportRepository();
        rpt_GroupRepository groupRepo = new rpt_GroupRepository();
        #endregion

        #region -Constructor-
        public ChangeGroupViewModel(MainViewModel mainVM, ChangeGroupView changeGroupView, rpt_ReportModel reportModel)
        {
            try
            {
                InitCommand();
                GetAllGroupReport();
                GroupReportModel = GroupReportCollection.Where(w => w.Id == reportModel.GroupId).FirstOrDefault();
                GetAllParentReport();
                ReportCollection = new ObservableCollection<rpt_ReportModel>(
                        AllReportCollection.Where(w => w.GroupId == reportModel.GroupId)
                    );
                ChangeGroupView = changeGroupView;
                MainVM = mainVM;
                if (ReportCollection.Count > 0)
                {
                    ReportModel = ReportCollection[0];
                }
                Report = reportModel;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }            
        }
        #endregion

        #region -Command-

        #region -Cancel Change Group Command-
        /// <summary>
        /// Set or get Cancel change group command
        /// </summary>
        public ICommand CancelChangeGroupCommand { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        private void CancelChangeGroupExecute()
        {
            ChangeGroupView.Close();
        }
        #endregion

        #region -Change Group Command-
        /// <summary>
        /// Set or get Change group command
        /// </summary>
        public ICommand ChangeGroupCommand { get; private set; }

        private void ChangeGroupExecute()
        {
            try
            {
                CPC.POSReport.Function.Common.IS_CHANGING_GROUP = true;
                int oldGroupId = MainVM.GetGroupReportPosition(Report.rpt_Report.GroupId);
                int oldParentId = Report.rpt_Report.ParentId;
                rpt_ReportModel newReport = Report;
                newReport.GroupId = GroupReportModel.Id;
                newReport.ParentId = ReportModel.Id;
                newReport.ToEntity();
                // Save changes
                reportRepo.Commit();
                int newGroupId = MainVM.GetGroupReportPosition(newReport.GroupId);
                if (oldParentId != newReport.ParentId)
                {
                    // Change to orther Major group
                    if (newGroupId != oldGroupId)
                    {
                        if (MainVM.GroupReportModelCollection[newGroupId].RootReportColection != null)
                        {
                            rpt_ReportModel newParent = MainVM.GroupReportModelCollection[newGroupId].RootReportColection
                                    .FirstOrDefault(x => x.Id == ReportModel.Id);
                            if (newParent != null)
                            {
                                // Change Relationship
                                newReport.Parent = newParent;
                                newParent.Children.Add(newReport);
                            }
                        }
                        // Remove report from old Major group
                        MainVM.GroupReportModelCollection[oldGroupId].RootReportColection.Remove(Report);
                    }
                    else
                    {
                        if (MainVM.GroupReportModelCollection[newGroupId].RootReportColection != null)
                        {
                            rpt_ReportModel newParent = MainVM.GroupReportModelCollection[newGroupId].RootReportColection
                                    .FirstOrDefault(x => x.Id == newReport.ParentId);
                            if (newParent != null)
                            {
                                newReport.Parent = newParent;
                                newParent.Children.Add(newReport);
                            }
                        }
                        if (newReport.ParentId == 0)
                        {
                            // Show close icon
                            MainVM.SetTreeViewIcon(2);
                        }
                        else
                        {
                            // Show document icon
                            MainVM.SetTreeViewIcon(3);
                        }
                    }
                    // Delete old parent
                    rpt_ReportModel oldParent = MainVM.GroupReportModelCollection[oldGroupId].RootReportColection
                        .FirstOrDefault(x => x.Id == oldParentId);
                    if (oldParent != null)
                    {
                        int parentId = Report.ParentId;
                        oldParent.Children.Remove(Report);
                        newReport.ParentId = parentId;
                    }
                }
                ChangeGroupView.Close();
                CPC.POSReport.Function.Common.IS_CHANGING_GROUP = false;
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanChangeGroupExecute()
        {
            if (GroupReportModel != null && ReportModel != null)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region -Selected Group Report Change Execute-
                
        public ICommand SelectedGroupReportChangeCommand { get; private set; }
        private void SelectedGroupReportChangeExecute()
        {
            try
            {
                // Get all Report by group
                ReportCollection = new ObservableCollection<rpt_ReportModel>(
                        AllReportCollection.Where(w => w.GroupId == GroupReportModel.Id)
                    );
                if (ReportCollection.Count > 0)
                {
                    ReportModel = ReportCollection[0];
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #endregion

        #region -Private method-
        /// <summary>
        /// Init all command
        /// </summary>
        private void InitCommand()
        {
            CancelChangeGroupCommand = new RelayCommand(CancelChangeGroupExecute);
            ChangeGroupCommand = new RelayCommand(ChangeGroupExecute, CanChangeGroupExecute);
            SelectedGroupReportChangeCommand = new RelayCommand(SelectedGroupReportChangeExecute);
        }
        /// <summary>
        /// Get all Report
        /// </summary>
        private void GetAllParentReport()
        {
            AllReportCollection = new ObservableCollection<rpt_ReportModel>(
                    reportRepo.GetAll()
                    .Select(r => new rpt_ReportModel(r))
                    .Where(w => w.ParentId == 0 && w.IsShow)
                    .OrderBy(o => o.Name)
            );  
        }
        /// <summary>
        /// Get all Group report
        /// </summary>
        private void GetAllGroupReport()
        {
            var lstGroupReportModel = new ObservableCollection<rpt_GroupModel>(
                    groupRepo.GetAll()
                    .Select(g => new rpt_GroupModel(g))
                    .OrderBy(o => o.Name)
                );
            // Get Report Group by permission
            if (!Function.Common.IS_ADMIN)
            {
                GroupReportCollection = new ObservableCollection<rpt_GroupModel>(
                        lstGroupReportModel.Where(w => CPC.POSReport.Function.Common.LST_GROUP.Contains(w.Code)));
            }
            else
            {
                GroupReportCollection = new ObservableCollection<rpt_GroupModel>(lstGroupReportModel);
            }
        }
        #endregion
    }
}
