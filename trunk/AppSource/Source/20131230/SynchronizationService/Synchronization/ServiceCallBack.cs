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
        public void SendResult(string message, bool result)
        {
            if (ClientNotified != null)
            {
                if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.AuthenticationResult.ToString(),null, result));
                else if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.GetFileResult.ToString(), null, result));
                else if (message.Equals(ServiceStatus.AuthenticationResult.ToString()))
                    ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.SendFileResult.ToString(), null, result));
            }
        }

        public bool IsConnect()
        {
            return true;
        }

        public void StoreDisConnect(StoreModel storeModel)
        {
            if (ClientNotified != null)
            {
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.DisConnect.ToString(), storeModel));
            }
        }

        public void SyncAtStore()
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.GetFile.ToString(), null));
        }

        public void SyncAtMainStore()
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.SendFile.ToString(), null));
        }

        public void SyncRequest()
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.SendFile.ToString(), null));
        }


        public bool Untransfer()
        {
           if (this.ClientNotified != null)
            {
                this.NotifiedEventArgs = new ClientNotifiedEventArgs(ServiceStatus.NoFile.ToString(), null);
                this.ClientNotified(this, NotifiedEventArgs);
            }
            return NotifiedEventArgs.Result;
        }

        public void SendError(FileInfoModel fileInfoModel)
        {
            if (ClientNotified != null)
                ClientNotified(this, new ClientNotifiedEventArgs(ServiceStatus.Error.ToString(), fileInfoModel));
        }

        #endregion
    }
}
