using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Npgsql;

namespace CPC.POSCustomInstaller
{
    public partial class frmConfiguration : Form
    {
        NpgsqlConnection m_NpgsqlConnection = null;

     #region Properties
    public string Server { get { return this.txtServer.Text; } }
    public int Port
    {
        get
        {
            int port = 0;
            Int32.TryParse(this.txtPort.Text, out port);
            return port;
        }
    }
    public string Database { get { return this.txtDatabase.Text; } }
    public string UserID { get { return this.txtUserID.Text; } }
    public string Password { get { return this.txtPassword.Text; } }
    
    #endregion

    #region Constructors & destructors
    public frmConfiguration(bool isRestore = true)
    {
        InitializeComponent();

        this.btnOK.Click += new EventHandler(btnOK_Click);
        this.btnCancel.Click += new EventHandler(btnCancel_Click);

        //this.label1.Visible = isRestore;
    }
    #endregion

    #region Events

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    protected void btnOK_Click(object sender, EventArgs e)
    {
        if (!TestConnection("postgres"))
        {
            MessageBox.Show("PostgreSQL is not ready. Please check parameters ... ", "Setup wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        string postgreApplicationLocation = InstallHelper.AssemblyFolder;
        string postgreInstallLocation = InstallHelper.GetInstallLocation("PostgreSQL");
        if (string.IsNullOrEmpty(postgreInstallLocation))
        {
            throw new Exception("PostgreSQL install location not found.");
        }
        
        RestoreDataBase(postgreInstallLocation, postgreApplicationLocation);

        UpdateAppConfig(postgreApplicationLocation);

        this.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.Close();
    }

    #endregion

    #region Methods
    private bool TestConnection(string database)
    {
        bool isResult = false;
        m_NpgsqlConnection = new Npgsql.NpgsqlConnection(String.Format("Server={0};Port={1};Database={2};UserID={3};Password={4}", Server, Port, database, UserID, Password));

        try
        {
            m_NpgsqlConnection.Open();
            isResult = true;
        }
        catch
        {
            isResult = false;
        }
        finally
        {
            if (m_NpgsqlConnection.State == ConnectionState.Open)
                m_NpgsqlConnection.Close();
        }

        return isResult;
    }
    private void UpdateApplicationConfigFile(string appConfigPath, string server, string database, string userid, string password, int port)
    {
        bool flagUpdate = false;

        if (File.Exists(appConfigPath))
        {
            string s = File.ReadAllText(appConfigPath);
            Match match = Regex.Match(s, "provider connection string=&quot;.*?&quot;", RegexOptions.IgnoreCase);
            if (match.Success)
           {
                string connectionString = BuildConnectionString(match.Value, server, port, database, userid, password);
                s = Regex.Replace(s, "provider connection string=&quot;.*?&quot;", connectionString, RegexOptions.IgnoreCase);

                flagUpdate = true;
            }
            if (flagUpdate)   File.WriteAllText(appConfigPath, s);
        }
    }
    private string BuildConnectionString(string connectionString, string server, int port, string database, string userID, string password)
    {
        Match matchServer = Regex.Match(connectionString, "Server=.*?;", RegexOptions.IgnoreCase);
        if (!matchServer.Success) matchServer = Regex.Match(connectionString, "Server=.*", RegexOptions.IgnoreCase);
        if (matchServer.Success) connectionString = Regex.Replace(connectionString, matchServer.Value.TrimEnd(';', '\''), "Server=" + server, RegexOptions.IgnoreCase);

        Match matchPort = Regex.Match(connectionString, "Port=.*?;", RegexOptions.IgnoreCase);
        if (!matchPort.Success) matchPort = Regex.Match(connectionString, "Port=.*", RegexOptions.IgnoreCase);
        if (matchPort.Success) connectionString = Regex.Replace(connectionString, matchPort.Value.TrimEnd(';', '\''), "Port=" + port, RegexOptions.IgnoreCase);

        Match matchDatabase = Regex.Match(connectionString, "Database=.*?;", RegexOptions.IgnoreCase);
        if (!matchDatabase.Success) matchDatabase = Regex.Match(connectionString, "Database=.*", RegexOptions.IgnoreCase);
        if (matchDatabase.Success) connectionString = Regex.Replace(connectionString, matchDatabase.Value.TrimEnd(';', '\''), "Database=" + database, RegexOptions.IgnoreCase);

        Match matchUserID = Regex.Match(connectionString, "UserId=.*?;", RegexOptions.IgnoreCase);
        if (!matchUserID.Success) matchUserID = Regex.Match(connectionString, "UserId=.*", RegexOptions.IgnoreCase);
        if (matchUserID.Success) connectionString = Regex.Replace(connectionString, matchUserID.Value.TrimEnd(';', '\''), "UserID=" + userID, RegexOptions.IgnoreCase);

        Match matchPassword = Regex.Match(connectionString, "Password=.*?;", RegexOptions.IgnoreCase);
        if (!matchPassword.Success) matchPassword = Regex.Match(connectionString, "Password=.*", RegexOptions.IgnoreCase);
        if (matchPassword.Success) connectionString = Regex.Replace(connectionString, matchPassword.Value.TrimEnd(';', '\''), "Password=" + password, RegexOptions.IgnoreCase);

        return connectionString + "&quot;";
    }
    private void RestoreDataBase(string postgreInstallLocation, string postgreApplicationLocation)
    {
        string postgreDataBasePath = postgreApplicationLocation + "\\DataBase";
        string postgreBinPath = postgreInstallLocation.Substring(2) + "\\bin";
        string postgreInstallPathRoot = postgreInstallLocation.Substring(0, 2);
        string batchFile = InstallHelper.AssemblyFolder + @"\createdb.bat";

        //Check Existed
        string sql = "SELECT datname FROM pg_database WHERE datistemplate IS FALSE AND datallowconn IS TRUE AND datname ='"+ Database +"';";
        m_NpgsqlConnection = new Npgsql.NpgsqlConnection(String.Format("Server={0};Port={1};Database={2};UserID={3};Password={4}", Server, Port, "postgres", UserID, Password));
        m_NpgsqlConnection.Open();
        NpgsqlCommand objSqlCommand = new NpgsqlCommand(sql, m_NpgsqlConnection);
        object result = objSqlCommand.ExecuteScalar();

        //Dispose Connection
        if (m_NpgsqlConnection.State == ConnectionState.Open)   m_NpgsqlConnection.Close();
        if (m_NpgsqlConnection != null)     m_NpgsqlConnection.Dispose();

        if (result!=null)
        {
            txtDatabase.BackColor = Color.Yellow;
            txtDatabase.ForeColor = Color.Red;
            if (MessageBox.Show(Database + " DataBase has existed. Do you want to overwrite or keep it?\r\n[Yes] = Overwrite \r\n[No] = Keep", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                StreamWriter streamWriter = System.IO.File.CreateText(batchFile);
                streamWriter.WriteLine("cls");
                streamWriter.WriteLine("echo On");
                streamWriter.WriteLine(string.Format("set PGHOST={0}", Server));
                streamWriter.WriteLine(string.Format("set PGUSERNAME={0}", UserID));
                streamWriter.WriteLine(string.Format("set PGPASSWORD={0}", Password));
                streamWriter.WriteLine("cd \\");
                streamWriter.WriteLine(postgreInstallPathRoot);
                streamWriter.WriteLine(string.Format(@"cd {0}", postgreInstallPathRoot + "\\" + postgreBinPath));
                streamWriter.WriteLine(string.Format("dropdb -U %PGUSERNAME% -e {0}", Database));
                streamWriter.WriteLine(string.Format("createdb -U %PGUSERNAME% -e {0}", Database));
                streamWriter.WriteLine(string.Format("pg_restore.exe  --host {0} --port {1} --username {2} --dbname \"{3}\" --verbose \"{4}\"", Server, Port, UserID, Database, postgreDataBasePath + "\\smartPOS.backup"));
                streamWriter.WriteLine("exit");
                streamWriter.Close();
                Process processDB = Process.Start(batchFile);

            }
        }
        else
        {
            txtDatabase.BackColor = Color.White;
            txtDatabase.ForeColor = Color.Black;
            StreamWriter streamWriter = System.IO.File.CreateText(batchFile);
            streamWriter.WriteLine("cls");
            streamWriter.WriteLine("echo On");
            streamWriter.WriteLine(string.Format("set PGHOST={0}", Server));
            streamWriter.WriteLine(string.Format("set PGUSERNAME={0}", UserID));
            streamWriter.WriteLine(string.Format("set PGPASSWORD={0}", Password));
            streamWriter.WriteLine("cd \\");
            streamWriter.WriteLine(postgreInstallPathRoot);
            streamWriter.WriteLine(string.Format(@"cd {0}", postgreInstallPathRoot + "\\" + postgreBinPath));
            streamWriter.WriteLine(string.Format("createdb -U %PGUSERNAME% -e {0}", Database));
            streamWriter.WriteLine(string.Format("pg_restore.exe  --host {0} --port {1} --username {2} --dbname \"{3}\" --verbose \"{4}\"", Server, Port, UserID, Database, postgreDataBasePath + "\\smartPOS.backup"));
            streamWriter.WriteLine("exit");
            streamWriter.Close();
            Process processDB = Process.Start(batchFile);

        }
    }
    private void UpdateAppConfig(string postgreApplicationLocation)
    {
        string appConfigPath = postgreApplicationLocation + "\\POS.exe.config";
        UpdateApplicationConfigFile(appConfigPath, Server, Database, UserID, Password, Port);
    }
     #endregion

    }
}
