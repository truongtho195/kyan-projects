using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace SynchronizationService
{
    /// <summary>
    /// The Callback interface
    /// </summary>
    public interface IServiceCallback
    {
        //Main store
        [OperationContract]
        bool IsMainStore(StoreModel storeModel);

        [OperationContract]
        // 0 : disconnect.
        // 1 : connect.
        // 2 : reconnect.
        int IsStore(StoreModel storeModel);

        [OperationContract]
        bool IsConnect();

        [OperationContract(IsOneWay = true)]
        void StoreDisConnect(StoreModel storeModel);

        [OperationContract(IsOneWay = true)]
        void SendResult(string message, bool result);

        //Send file to sync
        [OperationContract(IsOneWay = true)]
        void SyncAtStore(FileInfoModel fileInfoModel);

        //Send file to sync
        [OperationContract(IsOneWay = true)]
        void SyncAtMainStore(FileInfoModel fileInfoModel);

        //Don't transfer file.
        [OperationContract]
        bool Untransfer();

        //Main store send a request to another stores to get file.It is only used by store.
        [OperationContract(IsOneWay = true)]
        void SyncRequest();

         //Send error to another store. 
        [OperationContract(IsOneWay = true)]
        void SendError(FileInfoModel fileInfoModel);

        //Main store send a request to another stores to disconnect.
        [OperationContract(IsOneWay = true)]
        void MainStoreDisConnect(bool result);
        
        //To notify to another store about anything.
        [OperationContract(IsOneWay = true)]
        void Notification();
    }
}