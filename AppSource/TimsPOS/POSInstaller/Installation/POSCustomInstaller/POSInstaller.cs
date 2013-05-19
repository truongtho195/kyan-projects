using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;


namespace CPC.POSCustomInstaller
{
    [RunInstaller(true)]
    public partial class POSInstaller : System.Configuration.Install.Installer
    {
        const string SourceDir = "srcDir";
        const string ProductName = "POS";
        const string ImagesPath = "ImagesPath";

        #region Constructors & destructors
        public POSInstaller()
        {
            InitializeComponent();
        }
        #endregion

        #region Install
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }
        #endregion

        #region Committed
        protected override void OnCommitted(IDictionary savedState)
        {
            try
            {
                frmConfiguration configForm = new frmConfiguration(true);
                configForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                if (configForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)    base.OnCommitted(savedState);
            }
            catch
            {
                base.Rollback(savedState);
            }
        }
        #endregion

        #region Rollback
        protected override void OnAfterRollback(IDictionary savedState)
        {
            base.OnAfterRollback(savedState);
        }

        #endregion

        #region After Uninstall
        protected override void OnAfterUninstall(IDictionary savedState)
        {
            base.OnAfterUninstall(savedState);
        }
        #endregion

        #region Methods
        private void ShowParams()
        {
            string[] keys = new string[Context.Parameters.Keys.Count];
            Context.Parameters.Keys.CopyTo(keys, 0);
            string[] values = new string[Context.Parameters.Values.Count];
            Context.Parameters.Values.CopyTo(values, 0);
            MessageBox.Show(String.Join("\r\n", keys) + "\nValues: +++\n" + String.Join("\r\n", values));
        }
        #endregion

    }
}
