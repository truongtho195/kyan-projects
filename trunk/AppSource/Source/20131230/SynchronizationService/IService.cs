using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Generic;
using System;

namespace SynchronizationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IServiceCallback))]
    public interface IService
    {
        //Authentication
        [OperationContract(IsOneWay = true)]
        void Authentication(StoreModel storeModel);

        //Synchronization
        [OperationContract(IsOneWay = true)]
        void SynchronizeWithStore(FileInfoModel FileInfoModel);

        //Synchronization
        [OperationContract(IsOneWay = true)]
        void SynchronizeWithMainStore(FileInfoModel FileInfoModel);

        //Main store send a request to another stores to get file.
        [OperationContract(IsOneWay = true)]
        void SendSyncRequest();

        //Send result to store.
        [OperationContract(IsOneWay = true)]
        void SendResultToStore(FileInfoModel resultModel);

        //Store disconnect
        [OperationContract(IsOneWay = true)]
        void Disconnect(StoreModel storeModel);

        //To refresh store
        [OperationContract]
        bool RefreshStore(StoreModel storeModel);

        //MainStore disconnect.
        [OperationContract]
        void MainStoreDisConnect(StoreModel storeModel);

        //Send notification to store.
        [OperationContract]
        void StoreNotification(StoreModel storeModel);
    }
}
