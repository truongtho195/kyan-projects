using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;
using CPC.POSReport.View;
using System.Windows.Input;
using Toolkit.Command;
using CrystalDecisions.CrystalReports.Engine;

namespace CPC.POSReport.ViewModel
{
    class ReportViewViewModel : ModelBase
    {
        #region -property- 

        public Model.rpt_ReportModel ReportModel { get; set; }

        #region -Report Source-
        private object _reportSource = null;
        public object ReportSource
        {
            get { return _reportSource; }
            set { _reportSource = value; }
        }
        #endregion

        #region -Report Name-
        private object _reportName;
        public object ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }
        #endregion

        #region -Is Show Report Property-
        /// <summary>
        /// Set or get Is Show Report Property
        /// </summary>
        private string _isShowReport = "Visible";
        public string IsShowReport
        {
            get { return _isShowReport; }
            set
            {
                if (_isShowReport != value)
                {
                    _isShowReport = value;
                }
            }
        }
        #endregion

        #region -Is Show Image report Property-
        /// <summary>
        /// Set or get Is Show Print Property
        /// </summary>
        private string _isShowImageReport = string.Empty;
        public string IsShowImageReport
        {
            get { return _isShowImageReport; }
            set
            {
                if (_isShowImageReport != value)
                {
                    _isShowImageReport = value;
                }
            }
        }
        #endregion

        #region -Is Show Menu Height Property-
        /// <summary>
        /// Set or get Is Show Height menu Property
        /// </summary>
        private int _heigh = 50;
        public int Height
        {
            get { return _heigh; }
            set
            {
                if (_heigh != value)
                {
                    _heigh = value;
                    OnPropertyChanged(() => Height);
                }
            }
        }
        #endregion

        protected byte[] _samplePicture = null;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the SamplePicture</para>
        /// </summary>
        public byte[] SamplePicture
        {
            get { return this._samplePicture; }
            set { this._samplePicture = value; }
        }
        #endregion

        #region -Defines-
        int totalPage;
        string currentPrinter = string.Empty;
        public ViewReportWindow ReportWindow { get; set; }
        #endregion

        #region -Contructor-
        /// <summary>
        /// View image
        /// </summary>
        /// <param name="source">Source is ReportSource or Byte array Image</param>
        /// <param name="reportName">Name of report</param>
        /// <param name="reportWindow">Report Window</param>
        /// /// <param name="isReport">true is Report, false is Image</param>
        public ReportViewViewModel(byte[] source, string reportName, ViewReportWindow reportWindow)
        {
            ReportName = reportName;            
            SetVisibilityReport(false);
            SamplePicture = source;
            Height = 0;
            InitCommand();
            reportWindow.btnClose1.Visibility = System.Windows.Visibility.Visible;
            ReportWindow = reportWindow;            
        }

        /// <summary>
        /// View Report
        /// </summary>
        /// <param name="source">Source is ReportSource or Byte array Image</param>
        /// <param name="reportName">Name of report</param>
        /// <param name="reportWindow">Report Window</param>
        /// /// <param name="isReport">true is Report, false is Image</param>
        public ReportViewViewModel(object source, Model.rpt_ReportModel report, ViewReportWindow reportWindow, string crrPrinter)
        {
            ReportModel = report;
            ReportName = report.Name;
            this.currentPrinter = crrPrinter;
            SetVisibilityReport(true);
            ReportSource = source;
            totalPage = (ReportSource as ReportDocument).FormatEngine.GetLastPageNumber(new CrystalDecisions.Shared.ReportPageRequestContext());
            reportWindow.TotalPage = totalPage;
            reportWindow.SetEnalbeButton();            
            InitCommand();
            ReportWindow = reportWindow;
        }

        public ReportViewViewModel()
        { }
        #endregion

        #region -Command-
        private void InitCommand()
        {
            CloseCommand = new RelayCommand(CloseWindowExecute);
        }

        /// <summary>
        /// Close window 
        /// </summary>
        public ICommand CloseCommand {get; private set;}
        /// <summary>
        /// Execute close window
        /// </summary>
        public void CloseWindowExecute()
        {
            ReportWindow.Close();            
        }
        #endregion

        #region -Set visibility report-
        /// <summary>
        /// Set visibility report (image).
        /// if value is:
        /// - true: show report
        /// - false: show image report
        /// </summary>
        /// <param name="value"> </param>
        private void SetVisibilityReport(bool value)
        {
            IsShowImageReport = (value == true) ? "Hidden" : "Visible";
            IsShowReport = (value == true) ? "Visible" : "Hidden";
        }
        #endregion


        
    }
}
