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
using CPC.POS.SynchronizationService;
using System.ServiceModel;
using CPC.POS.ViewModel.Synchronization;
using System.IO;
using Npgsql;
using System.Reflection;
namespace CPC.POS.ViewModel
{
    public class SynchronizationViewModel : ViewModelBase
    {
        #region Defines
        private base_StoreRepository _storeRepository = new base_StoreRepository();
        protected ServiceClient ServiceClient;
        private ServiceCallBack _serviceCallBack;
        private string LogPath = @"D:\hunter\Log\";
        private DateTime LastDate = DateTime.Now.AddDays(-10);
        protected string ConnectString = "Port=5432;Server=localhost;Database=pos2013;UserId=postgres;Password=pd4pg9.0";
        private int NumbersOfFiles;
        private string StorePath = Define.CONFIGURATION.StoreFolder + @"\SendFiles\" + DateTime.Now.ToString("MM-dd-yyyy") + @"\";
        private string DirectoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        #endregion

        #region Constructor
        public SynchronizationViewModel()
        {
            //To create service
            this._serviceCallBack = new ServiceCallBack();
            this._serviceCallBack.ClientNotified += ChatServiceCallback_ClientNotified;
            this.ServiceClient = new ServiceClient(new InstanceContext(this._serviceCallBack));
        }
        public SynchronizationViewModel(int storeIndex)
        {
            this.GetStoreInfomation(storeIndex);
            //To create service
            this._serviceCallBack = new ServiceCallBack();
            this._serviceCallBack.ClientNotified += ChatServiceCallback_ClientNotified;
            this.ServiceClient = new ServiceClient(new InstanceContext(this._serviceCallBack));
            this.ServiceClient.UserAuthentication(this.Store);
            this.Tables.Add(new QueryModel { Id = 1, Text = "base_Product" });
        }
        #endregion

        #region Properties

        #region Tables
        private List<QueryModel> _tables = new List<QueryModel>();
        /// <summary>
        /// Gets or sets the Tables.
        /// </summary>
        public List<QueryModel> Tables
        {
            get { return _tables; }
            set
            {
                if (_tables != value)
                {
                    _tables = value;
                    OnPropertyChanged(() => Tables);
                }
            }
        }
        #endregion

        #region Store
        private StoreModel _store;
        /// <summary>
        /// Gets or sets the Store.
        /// </summary>
        public StoreModel Store
        {
            get { return _store; }
            set
            {
                if (_store != value)
                {
                    _store = value;
                    OnPropertyChanged(() => Store);
                }
            }
        }
        #endregion

        #region StoreCollection
        private ObservableCollection<StoreModel> _storeCollection = new ObservableCollection<StoreModel>();
        /// <summary>
        /// Gets or sets the StoreCollection.
        /// </summary>
        public ObservableCollection<StoreModel> StoreCollection
        {
            get { return _storeCollection; }
            set
            {
                if (_storeCollection != value)
                {
                    _storeCollection = value;
                    OnPropertyChanged("StoreCollection");
                }
            }
        }
        #endregion

        #region IsConnection
        private bool _isConnection;
        /// <summary>
        /// Gets or sets the IsConnection.
        /// </summary>
        public bool IsConnection
        {
            get { return _isConnection; }
            set
            {
                if (_isConnection != value)
                {
                    _isConnection = value;
                    OnPropertyChanged(() => IsConnection);
                }
            }
        }
        #endregion

        #endregion

        #region Private methods

        #region ChatServiceCallback_ClientNotified
        private void ChatServiceCallback_ClientNotified(object sender, ClientNotifiedEventArgs e)
        {
            try
            {
                switch (e.Message.ToString())
                {
                    //To connect to main store.
                    case "Connect":
                        if (e.Content is StoreModel)
                        {
                            StoreModel connectModel = e.Content as StoreModel;
                            int result = this.Authentication(connectModel);
                            if (result > 0)
                            {
                                e.IntResult = result;
                                this.SaveLog(this.LogPath + connectModel.Name, string.Empty, string.Format("{0} connect.", connectModel.Name));
                                var item = this.StoreCollection.SingleOrDefault(x => x.StoreCode == (e.Content as StoreModel).StoreCode);
                                item.Status = 1;
                                //To change password of client.
                                if (connectModel.IsChangePassword)
                                    this.ChangeStorePassword(connectModel);
                            }
                        }
                        break;

                    //To check that this store is main store.
                    case "CheckingMainStore":
                        if (e.Content is StoreModel)
                        {
                            StoreModel mainModel = e.Content as StoreModel;
                            bool result = this.CheckMainStore(mainModel);
                            if (result)
                            {
                                e.Result = result;
                                this.SaveLog(this.LogPath, string.Empty, string.Format("{0} connect.", mainModel.Name));
                                //To change password of client.
                                if (mainModel.IsChangePassword)
                                    this.ChangeStorePassword(mainModel);
                            }
                        }
                        break;

                    //To get result after this store connected to main store.
                    case "AuthenticationResult":
                        this.IsConnection = e.Result;
                        if (this.IsConnection)
                            MessageBox.Show("Conneting store is successful", this.Store.Name);
                        else
                            MessageBox.Show("Conneting store is failture.Please try it again.", this.Store.Name);
                        break;

                    //To get result when another store send a request to get file from it.
                    case "GetFile":
                        // Save log.
                        this.NumbersOfFiles += 1;
                        FileInfoModel FileModel = e.Content as FileInfoModel;
                        this.SaveLog(this.LogPath + FileModel.StoreName, string.Empty, string.Format("{0} get {1}", FileModel.StoreName, FileModel.FileName));
                        // Save file to folder
                        this.GetFile(FileModel);
                        //Execute import file.
                        if (this.NumbersOfFiles == FileModel.NumbersOfFiles)
                        {
                            this.NumbersOfFiles = 0;
                            this.ImportData(FileModel);
                        }
                        break;

                    //To get result when another store send a request to get file from this store.
                    case "SendFile":
                        MessageBox.Show(string.Format("Server send a request that server will get data of {0}.", this.Store.Name), Language.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                        try
                        {
                            ExportFileHelper.SyncFilePathCollection = null;
                            ExportFileHelper.ExportFile(this.ConnectString);
                            if (this.ServiceClient.State == CommunicationState.Opened && this.IsConnection && ExportFileHelper.SyncFilePathCollection.Count > 0)
                                foreach (var item in this.StoreCollection.Where(x => x.Status == 1))
                                {
                                    FileInfoModel FileInfoModel = new FileInfoModel();
                                    FileInfoModel.StoreIndex = this.Store.Index;
                                    FileInfoModel.NumbersOfFiles = ExportFileHelper.SyncFilePathCollection.Count();
                                    foreach (var itemPath in ExportFileHelper.SyncFilePathCollection)
                                    {
                                        FileInfo filInfo = new FileInfo(itemPath);
                                        FileInfoModel = new FileInfoModel();
                                        FileInfoModel.ByteSize = File.ReadAllBytes(itemPath);
                                        FileInfoModel.Length = FileInfoModel.ByteSize.LongLength;
                                        FileInfoModel.FileName = filInfo.Name;
                                        this.ServiceClient.SynchronizeWithMainStore(FileInfoModel);
                                    }
                                }
                        }
                        catch (TimeoutException ex)
                        {
                            MessageBox.Show(ex.ToString(), "Server");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        break;

                    case "DisConnect":
                        StoreModel DisConnectModel = e.Content as StoreModel;
                        this.StoreCollection.Remove(this.StoreCollection.SingleOrDefault(x => x.Index == DisConnectModel.Index));
                        this.SaveLog(this.LogPath, DateTime.Now.ToString("MMddyyyyhhmmss"), string.Format("{0} disconnect.", DisConnectModel.Name));
                        break;

                    case "Error":
                        FileInfoModel ErrorModel = e.Content as FileInfoModel;
                        this.SaveLog(this.LogPath + ErrorModel.StoreName, string.Empty, string.Format("{0} get {1}", ErrorModel.StoreName, ErrorModel.FileName));
                        // Save file to folder
                        if (ErrorModel.ByteSize.Count() > 0)
                        {
                            MessageBox.Show("Data Synchronization isn't completed.You should update status of this items.");
                            this.GetFile(ErrorModel);
                            this.UpdateStatusFile(this.StorePath + ErrorModel.FileName);
                        }
                        else
                            MessageBox.Show("Data Synchronization is completed.");
                        break;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Set Data
        private void SetDataForMainStore()
        {

        }
        private void SetDataForStore()
        {

        }
        #endregion

        #region Authentication
        private int Authentication(StoreModel storeModel)
        {
            // Check validation
            int result = 0;
            try
            {
                List<base_Store> Stores = this._storeRepository.GetAll().OrderBy(x => x.Id).ToList();
                for (int i = 0; i < Stores.Count(); i++)
                {
                    //Client has connected to server before.
                    if (this.StoreCollection != null
                        && this.StoreCollection.Count > 0 && this.StoreCollection.Count(x => x.Index == storeModel.Index) == 0
                        && i == storeModel.Index
                        && Stores[i].Password.Equals(storeModel.Password))
                    {
                        result = 1;
                        //To add store to StoreCollection to save it.
                        StoreModel model = storeModel;
                        model.Status = 1;
                        this.StoreCollection.Add(model);
                        break;
                    }
                    //Client has connected to server.
                    else if (this.StoreCollection != null && this.StoreCollection.Count > 0 && this.StoreCollection.Count(x => x.Index == storeModel.Index) > 0
                        && i == storeModel.Index
                        && Stores[i].Password.Equals(storeModel.Password)
                        && this.StoreCollection.SingleOrDefault(x => x.Index == storeModel.Index).Status == 1)
                    {
                        //To send request to this store to confirm that it is connecting to main store.
                        result = 2;
                        break;
                    }
                }
            }
            catch
            {
                return 0;
            }
            return result;
        }
        private bool CheckMainStore(StoreModel storeModel)
        {
            try
            {
                base_Store mainStore = this._storeRepository.GetAll().ElementAt(storeModel.Index);
                if (mainStore != null)
                {
                    //To check that main store don't connect to service.
                    if (((this.StoreCollection == null || this.StoreCollection.Count == 0) || (this.StoreCollection != null && this.StoreCollection.Count == 0 && this.StoreCollection.Count(x => x.Index == storeModel.Index) == 0))
                        && storeModel.Index == 0)
                    {
                        //To add store to StoreCollection to save it.
                        StoreModel model = storeModel;
                        model.Status = 1;
                        model.IsMainStore = true;
                        this.StoreCollection.Add(model);
                        return true;
                    }
                    else if (this.StoreCollection != null && this.StoreCollection.Count > 0 && this.StoreCollection.Count(x => x.Index == storeModel.Index) > 0
                        && storeModel.Index == 0
                         && this.StoreCollection.SingleOrDefault(x => x.IsMainStore).Status == 1)
                    {
                        //To send request to this main store to confirm that it is connecting to service.
                        this.StoreCollection.Remove(this.StoreCollection.SingleOrDefault(x => x.Index == storeModel.Index));
                        StoreModel model = storeModel;
                        model.Status = 1;
                        model.IsMainStore = true;
                        this.StoreCollection.Add(model);
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        #endregion

        #region SaveLog
        private void SaveLog(string path, string filename, string content)
        {
            try
            {
                string currentPath = path + "\\" + DateTime.Now.ToString("MMddyyyy");
                if (!Directory.Exists(currentPath))
                    Directory.CreateDirectory(currentPath);
                string file = currentPath + string.Format("\\Log_{0}.txt", DateTime.Now.ToString("MMddyyyy"));
                CreateFile.Path = file;
                CreateFile.WriteText(content);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        #endregion

        #region ChangeStorePassword
        private void ChangeStorePassword(StoreModel storeModel)
        {
            var store = _storeRepository.GetIEnumerable().SingleOrDefault(x => x.Code == storeModel.StoreCode);
            if (store != null)
            {
                store.Password = storeModel.NewPassword;
                _storeRepository.Commit();
            }
        }
        #endregion

        #region GetStoreInfomation
        private void GetStoreInfomation(int storeIndex)
        {
            base_Store store = this._storeRepository.GetAll().OrderBy(x => x.Id).ElementAt(storeIndex);
            this.Store = new StoreModel();
            this.Store.Index = storeIndex;
            this.Store.StoreCode = store.Code;
            this.Store.Password = store.Password;
            this.Store.Status = 0;
            this.Store.Name = store.Name;
            if (storeIndex == 0)
                this.Store.IsMainStore = true;
        }
        #endregion

        #region GetQuery
        private List<QueryModel> GetQuery(string path)
        {
            List<QueryModel> QueryModel = new List<QueryModel>();
            try
            {
                List<string> content = new List<string>();
                //Get the root directory and print out some information about it.
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
                Debug.WriteLine(dirInfo.Attributes.ToString());
                // Get the files in the directory and print out some information about them.
                System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (System.IO.FileInfo file in fileNames.OrderBy(x => x.CreationTime).Where(x => x.CreationTime > LastDate && x.Extension.Equals(".zip")))
                {
                    string OutPutFile = file.Directory + "\\" + file.Name;
                    ExportFileHelper.DeCompress(file.FullName, file.Directory.ToString());
                    List<string> line = System.IO.File.ReadAllLines(OutPutFile.Replace(".zip", "")).ToList();
                    content.AddRange(line);
                }
                if (content != null && content.Count > 0)
                {
                    foreach (var itemQuery in content)
                    {
                        QueryModel model = new QueryModel();
                        model.Id = content.IndexOf(itemQuery);
                        model.Resource = itemQuery.Substring(0, 36);
                        model.Text = itemQuery.Substring(37);
                        QueryModel.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return QueryModel;
        }
        #endregion

        #region ImportData
        private void ImportData(FileInfoModel FileInfoModel)
        {
            List<string> failtureData = new List<string>();
            try
            {
                // PostgeSQL-style connection string
                using (NpgsqlConnection pgsqlConnection = new NpgsqlConnection(this.ConnectString))
                {
                    // Open the PgSQL Connection.                
                    pgsqlConnection.Open();
                    List<QueryModel> Commands = this.GetQuery(this.StorePath + @"\" + FileInfoModel.StoreName.Replace(" ", ""));
                    if (Commands != null && Commands.Count > 0)
                    {
                        //Disable trigger in table.
                        foreach (var itemTable in this.Tables)
                        {
                            string query = string.Format("ALTER TABLE \"{0}\" DISABLE TRIGGER \"proccess_product\"", itemTable.Text);
                            using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(query, pgsqlConnection))
                            {
                                try
                                {
                                    pgsqlcommand.CommandType = System.Data.CommandType.Text;
                                    int result = pgsqlcommand.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write(ex.ToString());
                                }
                            }
                        }
                        //Insert ,update, delete data
                        foreach (var item in Commands)
                        {
                            string selectCommand = item.Text;
                            using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(selectCommand, pgsqlConnection))
                            {
                                using (NpgsqlTransaction tran = pgsqlConnection.BeginTransaction())
                                {
                                    try
                                    {
                                        pgsqlcommand.CommandType = System.Data.CommandType.Text;
                                        int result = pgsqlcommand.ExecuteNonQuery();
                                        tran.Commit();
                                        if (result == 0)
                                            failtureData.Add(item.Resource);
                                    }
                                    catch
                                    {
                                        failtureData.Add(item.Resource);
                                    }
                                }
                            }
                        }
                        //Enable trigger in table.
                        foreach (var itemTable in this.Tables)
                        {
                            string query = string.Format("ALTER TABLE \"{0}\" ENABLE TRIGGER \"proccess_product\"", itemTable);
                            using (NpgsqlCommand pgsqlcommand = new NpgsqlCommand(query, pgsqlConnection))
                            {
                                try
                                {
                                    pgsqlcommand.CommandType = System.Data.CommandType.Text;
                                    int result = pgsqlcommand.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    Debug.Write(ex.ToString());
                                }
                            }
                        }
                    }
                    //Send message to another store.
                    FileInfoModel.ByteSize = null;
                    FileInfoModel.Length = 0;
                    this.ServiceClient.SendResultToStore(FileInfoModel);
                    MessageBox.Show("Data Synchronization is completed.");
                }

                //Save data
                if (failtureData.Count > 0)
                {//string.Format(@"\Fail_{0}.txt", DateTime.Now.ToString("MM-dd-yyyy")
                    string fileName = string.Format("{0}_fail.txt", DateTime.Now.ToString("yyMMddHHmmssfff"));
                    string folder = Path.Combine(DirectoryName, fileName);
                    StreamWriter tempFile = new StreamWriter(folder);
                    foreach (var itemFail in failtureData)
                        tempFile.WriteLine(itemFail);
                    tempFile.Flush();
                    tempFile.Close();
                    FileInfoModel infoModel = new FileInfoModel();
                    infoModel.StoreIndex = FileInfoModel.StoreIndex;
                    infoModel.StoreName = FileInfoModel.StoreName;
                    FileInfo fileInfo = new FileInfo(folder);
                    ExportFileHelper.Compress(fileInfo);
                    infoModel.ByteSize = File.ReadAllBytes(folder.Replace(".txt", "zip"));
                    infoModel.Length = FileInfoModel.ByteSize.LongLength;
                    infoModel.FileName = fileInfo.Name;
                    this.ServiceClient.SendResultToStore(infoModel);
                }
                else
                    this.ServiceClient.SendResultToStore(new FileInfoModel { StoreName = FileInfoModel.StoreName, StoreIndex = FileInfoModel.StoreIndex });
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                MessageBox.Show(msg.ToString());
            }
        }
        #endregion

        #region GetFile
        private void GetFile(FileInfoModel FileInfoModel)
        {
            FileStream targetStream = null;
            string folder = this.StorePath + @"\" + FileInfoModel.StoreName.Replace(" ", "");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, FileInfoModel.FileName);
            using (targetStream = new FileStream(filePath, FileMode.Create,
                                  FileAccess.Write, FileShare.None))
            {
                //read from the input stream in 65000 byte chunks
                byte[] buffer = FileInfoModel.ByteSize;
                // save to output stream
                targetStream.Write(buffer, 0, (int)FileInfoModel.Length);
                int length = (int)targetStream.Length;
                targetStream.Close();
            }
        }
        #endregion

        #region SendFile
        public void SendFile()
        {
            try
            {
                if (this.StoreCollection.Count == 0)
                {
                    MessageBox.Show("No anystore to synchrnize data.");
                    return;
                }
                ExportFileHelper.SyncFilePathCollection = null;
                ExportFileHelper.ExportFile(this.ConnectString);
                if (this.ServiceClient.State == CommunicationState.Opened && ExportFileHelper.SyncFilePathCollection.Count > 0)//&& this.IsConnection
                {
                    FileInfoModel FileInfoModel = new FileInfoModel();
                    FileInfoModel.StoreIndex = this.Store.Index;
                    FileInfoModel.StoreName = this.Store.Name;
                    FileInfoModel.NumbersOfFiles = ExportFileHelper.SyncFilePathCollection.Count();
                    foreach (var itemPath in ExportFileHelper.SyncFilePathCollection)
                    {
                        FileInfo filInfo = new FileInfo(itemPath);
                        FileInfoModel.ByteSize = File.ReadAllBytes(itemPath);
                        FileInfoModel.Length = FileInfoModel.ByteSize.LongLength;
                        FileInfoModel.FileName = filInfo.Name;
                        this.ServiceClient.SynchronizeWithStore(FileInfoModel);
                    }
                }
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show(ex.ToString(), "Server");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region SyncData
        public void SyncData()
        {
            try
            {
                if (this.ServiceClient.State == CommunicationState.Faulted)
                    this.ServiceClient.Open();
                if (this.ServiceClient.State == CommunicationState.Opened)
                    this.ServiceClient.SendSyncRequest();
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show(ex.ToString(), "Server");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        //Update status of item when item have error.
        private void UpdateStatusFile(string path)
        {

        }
        #endregion
    }

}
