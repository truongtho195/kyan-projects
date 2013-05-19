using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace CPC.POSCustomInstaller
{
    public partial class frmRestore : Form
    {
        public string _server;
        public int _port;
        public string _userID;
        public string _password;

        public string BackupPath
        {
            get { return this.txtBackupFilePath.Text; }
        }

        #region Constructors & destructors
        public frmRestore(string server, int port, string userid, string password)
        {
            InitializeComponent();

            this.btnBrowse.Click += new EventHandler(btnBrowse_Click);
            this.btnOK.Click += new EventHandler(btnOK_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            _server = server;
            _port = port;
            _userID = userid;
            _password = password;
        }
        #endregion

        #region Events

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = this.txtBackupFilePath.Text;
            openFileDialog.Filter = "File (*.backup)|*.backup";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtBackupFilePath.Text = openFileDialog.FileName;
            }
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {
            if (!TestConnection("postgres"))
            {
                Console.WriteLine("You must install PostgreSQL 9.x before run the program ... ");
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        #endregion

        #region Methods
        private bool TestConnection(string database)
        {
            bool isResult = false;
            Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(String.Format("Server={0};Port={1};Database={2};UserID={3};Password={4}", _server, _port, database, _userID, _password));

            try
            {
                connection.Open();
                isResult = true;
            }
            catch
            {
                isResult = false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return isResult;
        }

        #endregion

    }

}