using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using CPC.POS.SynchronizationService;
using CPC.POS.ViewModel.Synchronization;

namespace CPC.POS.ViewModel
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class ServiceCallBack : IServiceCallback
    {
        public delegate void ClientNotifiedEventHandler(object sender, ClientNotifiedEventArgs e);

        public event ClientNotifiedEventHandler ClientNotified;

        protected ClientNotifiedEventArgs NotifiedEventArgs;

        #region ICPCServiceCallback Members

        /// <summary>
        /// Checking main store.
        /// </summary>
        /// <returns></returns>
        public bool IsMainStore(StoreModel storeModel)
        {
            if (this.ClientNotified != null)
            {
                this.NotifiedEventArgs = new ClientNotifiedEventArgs(ServiceStatus.CheckingMainStore.ToString(), storeModel);
                this.ClientNotified(this, NotifiedEventArgs);
                return NotifiedEventArgs.Result;
            }
            return false;
        }

        /// <summary>
        /// Checking another store.
        /// </summary>
        /// <returns></returns>
        public int IsStore(StoreModel storeModel)
        {
            if (ClientNotified != null)
            {
                this.NotifiedEventArgs = new ClientNotifiedEventArgs(ServiceStatus.Connect.ToString(), storeModel);
                ClientNotified(this, NotifiedEventArgs);
                return NotifiedEventArgs.IntResult;
            }
            return 0;
        }

        /// <summary>
        /// To send result to store.
        /// </summary>
        /// <returns></returns>
        public void SendResult(string message, bool result)
        {
            if (ClientNotified != null)
            {
                if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.AuthenticationResult.ToString(), null, result));
                else if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.GetFileResult.ToString(), null, result));
                else if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.SendFileResult.ToString(), null, result));
            }
        }

        /// <summary>
        /// Checking connect of all stores.
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            return true;
        }

        /// <summary>
        /// Store disconnect .
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public void StoreDisConnect(StoreModel storeModel)
        {
            if (ClientNotified != null)
            {
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.DisConnect.ToString(), storeModel));
            }
        }
        /// <summary>
        /// Client get file from server.
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public void SyncAtStore(FileInfoModel fileInfoModel)
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.GetFile.ToString(), fileInfoModel));
        }

        /// <summary>
        /// Server get file from client.
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public void SyncAtMainStore(FileInfoModel fileInfoModel)
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.GetFile.ToString(), fileInfoModel));
        }

        /// <summary>
        /// Server send a request to client.
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public void SyncRequest()
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.SendFile.ToString(), null));
        }

        /// <summary>
        /// Checking file from client before it send to server.
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public bool Untransfer()
        {
            if (this.ClientNotified != null)
            {
                this.NotifiedEventArgs = new ClientNotifiedEventArgs(ServiceStatus.NoFile.ToString(), null);
                this.ClientNotified(this, NotifiedEventArgs);
            }
            return NotifiedEventArgs.Result;
        }

        /// <summary>
        /// To send error to client or server if file transfer have error.
        /// </summary>
        /// <param name="fileInfoModel"></param>
        public void SendError(FileInfoModel fileInfoModel)
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.Error.ToString(), fileInfoModel));
        }


        public void SendRequestConnect(bool result)
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.ServerDisconnect.ToString(), null, result));
        }

        #endregion

    }
}
