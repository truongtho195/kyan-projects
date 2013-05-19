using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CPC.POSCustomInstaller
{
    public partial class frmPrerequisites : Form
    {
        #region Property IsConfigAccount
        /// <summary>
        /// Property IsConfigAccount.
        /// </summary>
        public bool IsConfigAccount
        {
            get;
            private set;
        }
        #endregion // end Property IsConfigAccount

        #region Property IsRestoreDB
        /// <summary>
        /// Property IsRestoreDB.
        /// </summary>
        public bool IsRestoreDB
        {
            get;
            private set;
        }
        #endregion // end Property IsRestoreDB


        public frmPrerequisites()
        {
            InitializeComponent();

            this.btnInstall.Click += new EventHandler(btnInstall_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            this.IsConfigAccount = false;
            this.IsRestoreDB = false;

            this.Close();
        }

        protected void btnInstall_Click(object sender, EventArgs e)
        {
            if (this.chkDataBase.Checked ||
                this.chkRestore.Checked)
            {
                this.IsConfigAccount = this.chkDataBase.Checked;
                this.IsRestoreDB = this.chkRestore.Checked;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

    }
}
