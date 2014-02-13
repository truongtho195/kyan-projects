using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.IO;

namespace SynchronizationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class Service : IService
    {
        #region Defines
        /// <summary>
        /// Store callback interfaces for all connected clients
        /// </summary>
        //To save a server when it registered.
        protected static Dictionary<StoreModel, IServiceCallback> ServerChannelDictionary = new Dictionary<StoreModel, IServiceCallback>();

        //To save clients when they registered.
        protected static Dictionary<StoreModel, IServiceCallback> ClientChannelDictionary = new Dictionary<StoreModel, IServiceCallback>();

        //To get current Channel.
        protected IServiceCallback CurrentChannel;

        #endregion

        #region IService Members

        #region Authentication
        /// <summary>
        /// To authenticate that user can access data.
        /// </summary>
        /// <param name="storeModel"></param>
        public void Authentication(StoreModel storeModel)
        {
            try
            {
                CreateFile.WriteText(storeModel.Name + " Starting connection.");
                //To get current interface of current channel
                this.CurrentChannel = OperationContext.Current.GetCallbackChannel<IServiceCallback>();

                //To check that this store is main store.
                if (ServerChannelDictionary.Count == 0 && storeModel.IsMainStore)
                {
                    if (this.CurrentChannel.IsMainStore(storeModel))
                    {
                        ClientChannelDictionary.Clear();
                        ServerChannelDictionary.Clear();
                        ServerChannelDictionary.Add(storeModel, this.CurrentChannel);
                        this.CurrentChannel.SendResult("AuthenticationResult", true);
                    }
                    else
                        this.CurrentChannel.SendResult("AuthenticationResult", false);
                }

                //To check that this store is old main store and it reconnect.
                else if (ServerChannelDictionary.Count > 0 && storeModel.IsMainStore)
                {
                    //To check old main store.
                    bool isconnect = false;
                    try
                    {
                        isconnect = ServerChannelDictionary.First().Value.IsConnect();
                    }
                    catch
                    {
                        isconnect = false;
                    }
                    if (!isconnect)
                    {
                        if (storeModel.IsMainStore && this.CurrentChannel.IsMainStore(storeModel))
                        {
                            ClientChannelDictionary.Clear();
                            ServerChannelDictionary.Clear();
                            ServerChannelDictionary.Add(storeModel, this.CurrentChannel);
                        }
                        this.CurrentChannel.SendResult("AuthenticationResult", true);
                    }
                    else
                        this.CurrentChannel.SendResult("AuthenticationResult", false);
                }
                //Queues a method for execution. The method executes when a thread pool thread
                //becomes available.
                else
                {
                    // ThreadPool.QueueUserWorkItem
                    //(
                    //    delegate
                    //    {
                    //To check that this store is child store.
                    if (ServerChannelDictionary.Count() > 0)
                    {
                        //To check store.
                        var itemStore = ClientChannelDictionary.SingleOrDefault(x => x.Key.Index == storeModel.Index);
                        if (itemStore.Value != null && itemStore.Value.IsConnect())
                        {
                            this.CurrentChannel.SendResult("AuthenticationResult", false);
                            return;
                        }
                        int resultServer = ServerChannelDictionary.First().Value.IsStore(storeModel);
                        //Response result.
                        if (resultServer > 0)
                        {
                            this.CurrentChannel.SendResult("AuthenticationResult", true);
                            //Add Client to ClientChannelDictionary.
                            var query = ClientChannelDictionary.Keys.Where(x => x.Index == storeModel.Index);
                            if (query != null && query.Count() > 0)
                                foreach (var item in query)
                                    ClientChannelDictionary.Remove(item);
                            ClientChannelDictionary.Add(storeModel, this.CurrentChannel);
                        }
                        else
                            this.CurrentChannel.SendResult("AuthenticationResult", false);
                    }
                    else
                        this.CurrentChannel.SendResult("AuthenticationResult", false);
                    // }); SynchronizationService
                }
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(storeModel.Name + ex.ToString());
                this.CurrentChannel.SendResult("AuthenticationResult", false);
            }
            CreateFile.WriteText(storeModel.Name + " Ending connection.");
        } 
        #endregion

        #region SynchronizeWithStore
        /// <summary>
        /// To sync data with another store.
        /// </summary>
        /// <param name="FileInfoModel"></param>
        public void SynchronizeWithStore(FileInfoModel FileInfoModel)
        {
            try
            {
                // Call each client's callback method
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        CreateFile.WriteText(FileInfoModel.StoreName + " Starting Sync data that get from Main Store.");
                        foreach (KeyValuePair<StoreModel, IServiceCallback> channelItem in ClientChannelDictionary)
                        {
                            bool result = false;
                            try
                            {
                                result = channelItem.Value.IsConnect();
                            }
                            catch
                            {
                                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                    ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                            }
                            if (result)
                            {
                                //To call that client sends file.
                                channelItem.Value.SyncAtStore(FileInfoModel);
                            }
                            else
                                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                    ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                        }

                    });
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(FileInfoModel.StoreName + ex.ToString());
                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                    ServerChannelDictionary.First().Value.SendResult("Fail", true);
            }
        } 
        #endregion

        #region SynchronizeWithMainStore
        /// <summary>
        /// When mani store send getting data request ,another store will send file to main store. 
        /// </summary>
        /// <param name="FileInfoModel"></param>
        public void SynchronizeWithMainStore(FileInfoModel FileInfoModel)
        {
            try
            {
                // Call each client's callback method
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        CreateFile.WriteText(FileInfoModel.StoreName + " Starting Sync data that get from Main Store.");
                        bool result = false;
                        try
                        {
                            result = ServerChannelDictionary.First().Value.IsConnect();
                        }
                        catch
                        {
                            if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                            {
                                ServerChannelDictionary.First().Value.StoreDisConnect(ServerChannelDictionary.First().Key);
                                ServerChannelDictionary.Clear();
                            }
                        }
                        if (result)
                        {
                            if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                //To call that client sends file.
                                ServerChannelDictionary.First().Value.SyncAtMainStore(FileInfoModel);
                        }
                        else
                        {
                            if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                            {
                                ServerChannelDictionary.First().Value.StoreDisConnect(ServerChannelDictionary.First().Key);
                                ServerChannelDictionary.Clear();
                            }
                        }

                    });
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(FileInfoModel.StoreName + ex.ToString());
                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                    ServerChannelDictionary.First().Value.SendResult("Fail", true);
            }
        } 
        #endregion

        #region SendSyncRequest
        /// <summary>
        /// To send request to get file.
        /// </summary>
        public void SendSyncRequest()
        {
            try
            {
                // Call each client's callback method
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        if (ClientChannelDictionary.Count > 0)
                        {
                            foreach (KeyValuePair<StoreModel, IServiceCallback> channelItem in ClientChannelDictionary)
                            {
                                bool result = false;
                                try
                                {
                                    result = channelItem.Value.IsConnect();
                                }
                                catch
                                {
                                    if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                        ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                                }
                                if (result)
                                {
                                    if (!channelItem.Value.Untransfer())
                                        //To call that client sends file.
                                        channelItem.Value.SyncRequest();
                                    CreateFile.WriteText(" Main Store get data from " + channelItem.Key.Name);
                                }
                                else
                                    if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                        ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                            }
                        }
                        else
                            if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                ServerChannelDictionary.First().Value.SendResult(Function.NoStore.ToString(), true);
                    });
            }
            catch (Exception ex)
            {
                CreateFile.WriteText("SendSyncRequest" + ex.ToString());
                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                    ServerChannelDictionary.First().Value.SendResult("Fail", true);
            }
        } 
        #endregion

        #region SendResultToStore
        /// <summary>
        /// To send result to store.
        /// </summary>
        /// <param name="resultModel"></param>
        public void SendResultToStore(FileInfoModel resultModel)
        {
            try
            {
                if (ServerChannelDictionary.First().Key.Index == resultModel.StoreIndex)
                    ServerChannelDictionary.First().Value.SendError(resultModel);
                else
                {
                    var itemStore = ClientChannelDictionary.Where(x => x.Key.Index == resultModel.StoreIndex).SingleOrDefault();
                    if (itemStore.Value != null)
                        itemStore.Value.SendError(resultModel);
                }
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(resultModel.StoreName + ex.ToString());
            }
        } 
        #endregion

        #region Disconnect
        public void Disconnect(StoreModel storeModel)
        {
            try
            {
                if (storeModel.IsMainStore && ServerChannelDictionary.First().Key.Index == storeModel.Index)
                {
                    ServerChannelDictionary.First().Value.StoreDisConnect(storeModel);
                    ServerChannelDictionary.Clear();
                }
                else
                {
                    var itemStore = ClientChannelDictionary.Where(x => x.Key.Index == storeModel.Index).SingleOrDefault();
                    if (itemStore.Key != null)
                        ClientChannelDictionary.Remove(itemStore.Key);
                }
                CreateFile.WriteText(storeModel + " disconnect.");
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(storeModel.Name + ex.ToString());
            }
        } 
        #endregion

        #region RefreshStore
        public bool RefreshStore(StoreModel storeModel)
        {
            bool result = false;
            ThreadPool.QueueUserWorkItem
                 (
                     delegate
                     {
                         var queryStore = ClientChannelDictionary.SingleOrDefault(x => x.Key.Index == storeModel.Index);
                         CreateFile.WriteText("Checking connection of " + storeModel.Name);
                         try
                         {
                             result = queryStore.Value.IsConnect();
                         }
                         catch
                         {
                             ClientChannelDictionary.Remove(queryStore.Key);
                         }
                         if (!result)
                         {
                             ClientChannelDictionary.Remove(queryStore.Key);
                         }
                     });
            return result;
        } 
        #endregion

        #region MainStoreDisConnect
        public void MainStoreDisConnect(StoreModel storeModel)
        {
            try
            {
                // Call each client's callback method
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        CreateFile.WriteText(storeModel.Name + " disconnect.");
                        foreach (KeyValuePair<StoreModel, IServiceCallback> channelItem in ClientChannelDictionary)
                        {
                            bool result = false;
                            try
                            {
                                result = channelItem.Value.IsConnect();
                            }
                            catch
                            {
                                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                    ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                            }
                            if (result)
                            {
                                channelItem.Value.MainStoreDisConnect(true);
                            }
                            else
                                if (ServerChannelDictionary != null && ServerChannelDictionary.Count > 0)
                                    ServerChannelDictionary.First().Value.StoreDisConnect(channelItem.Key);
                        }
                        CreateFile.WriteText(" Main Store disconnect.");
                    });
            }
            catch (Exception ex)
            {
                CreateFile.WriteText(ServerChannelDictionary.First().Key.Name + ex.ToString());
                ServerChannelDictionary.First().Value.SendResult("Fail", true);
            }
        } 
        #endregion

        #region StoreNotification
        public void StoreNotification(StoreModel storeModel)
        {

        } 
        #endregion

        #endregion
    }

    public static class CreateFile
    {
        public static int ID = 0;

        public static string Name = "Service";

        public static string Path = @"D:\hunter\ServiceLog\" + string.Format("{0}_Log.txt", Name);
        public static void WriteText(string content)
        {
            try
            {
                if (!File.Exists(Path))
                    File.Create(Path);
                string contentstart = string.Format("{0}--", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
                string contentend = ".";
                string format = string.Format("{0}{1}{2}\n", contentstart, content, contentend);
                // Write the string to a file.
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path, true))
                {
                    file.WriteLine(format);
                }
                ID++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
