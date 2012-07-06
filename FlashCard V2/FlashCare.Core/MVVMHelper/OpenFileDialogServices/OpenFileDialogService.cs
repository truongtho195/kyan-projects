using System;
using System.Collections.Generic;
using System.Text;

namespace MVVMHelper.Services
{
    public class OpenFileDialogService : IOpenFileDialogService
    {

        #region Properties

        private Microsoft.Win32.OpenFileDialog _dialog = new Microsoft.Win32.OpenFileDialog();

        /// <summary>
        /// Filenames
        /// </summary>
        public string[] FileNames
        {
            get { return _dialog.FileNames; }
        }

        /// <summary>
        /// Filename
        /// </summary>
        public string FileName
        {
            get { return _dialog.FileName; }
            set { _dialog.FileName = value; }
        }

        /// <summary>
        /// InitialDirectory Property
        /// </summary>
        public string InitialDirectory
        {
            get { return _dialog.InitialDirectory; }
            set
            {
                if (_dialog.InitialDirectory != value)
                {
                    _dialog.InitialDirectory = value;
                }
            }
        }

        /// <summary>
        /// Filter Property
        /// </summary>
        public string Filter
        {
            get { return _dialog.Filter; }
            set
            {
                if (_dialog.Filter != value)
                {
                    _dialog.Filter = value;
                }
            }
        }

        /// <summary>
        /// FilterIndex Property
        /// </summary>
        public int FilterIndex
        {
            get { return _dialog.FilterIndex; }
            set
            {
                if (_dialog.FilterIndex != value)
                {
                    _dialog.FilterIndex = value;
                }
            }
        }

        /// <summary>
        /// Multiselect Property
        /// </summary>
        public bool Multiselect
        {
            get { return _dialog.Multiselect; }
            set { _dialog.Multiselect = value; }
        }

        /// <summary>
        /// CheckPathExists Property
        /// </summary>
        public bool CheckPathExists
        {
            get { return _dialog.CheckPathExists; }
            set { _dialog.CheckPathExists = value; }
        }

        /// <summary>
        /// CheckFileExists Property
        /// </summary>
        public bool CheckFileExists
        {
            get { return _dialog.CheckFileExists; }
            set { _dialog.CheckFileExists = value; }
        }

        public System.ComponentModel.CancelEventHandler FileOk
        {
            set {
                _dialog.FileOk += value;
            }
        }
        
        #endregion

        #region Methods
        public bool? ShowDialog()
        {
            return _dialog.ShowDialog();
        } 
        #endregion

    }
}
