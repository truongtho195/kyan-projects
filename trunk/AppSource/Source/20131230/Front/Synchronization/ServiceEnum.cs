using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPC.POS.ViewModel.Synchronization
{
    #region ServiceStatus
    public enum ServiceStatus
    {
        Connect = 0,
        DisConnect = 1,
        SendFile = 2,
        GetFile = 3,
        Sync = 4,
        Error = 5,
        CheckingMainStore = 6,
        AuthenticationResult,
        SendFileResult = 8,
        GetFileResult = 9,
        NoFile = 10,
        ServerDisconnect = 11
    }
    #endregion

    #region SynchronizationColumn

    public enum SynchronizationColumn
    {
        ID = 0,
        Resource = 1,
        TableName = 2,
        Status = 3,
        CreatedDate = 4,
        UpdatedDate = 5,
        SynchronizationDate = 6,
        IsSynchronous = 7
    }

    #endregion

    #region SynchronizationStatus

    public enum SynchronizationStatus
    {
        Insert = 1,
        Update = 2,
        Delete = 3
    }

    #endregion
}
