using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace SynchronizationService
{
    [DataContract]
    public class StoreModel
    {
        #region Properties

        private int _index;
        [DataMember]
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        private string _storeCode;
        /// <summary>
        /// Gets or sets the StoreCode
        /// </summary>
        [DataMember]
        public string StoreCode
        {
            get { return _storeCode; }
            set { _storeCode = value; }
        }

        private string _name;
        /// <summary>
        /// Gets or sets the StoreName
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _status;
        /// <summary>
        /// 0 : disconnect
        /// 1 : connect
        /// Gets or sets the Status
        /// </summary>
        [DataMember]
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _password;
        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [DataMember]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _newPassword;
        /// <summary>
        /// Gets or sets the NewPassword
        /// </summary>
        [DataMember]
        public string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; }
        }

        private bool _isMainStore;
        /// <summary>
        /// Gets or sets the IsServer
        /// </summary>
        [DataMember]
        public bool IsMainStore
        {
            get { return _isMainStore; }
            set { _isMainStore = value; }
        }

        private bool _isChangePassword;
        /// <summary>
        /// Gets or sets the IsChangePassword
        /// </summary>
        [DataMember]
        public bool IsChangePassword
        {
            get { return _isChangePassword; }
            set { _isChangePassword = value; }
        }
        #endregion
    }

    [DataContract]
    public class FileInfoModel
    {
        private int _storeIndex;
        [DataMember(Name = "StoreIndex", Order = 0, IsRequired = true)]
        public int StoreIndex
        {
            get { return _storeIndex; }
            set { _storeIndex = value; }
        }

        private string _storeName;
        /// <summary>
        /// Gets or sets the StoreName
        /// </summary>
        [DataMember(Name = "StoreName", Order = 1, IsRequired = true)]
        public string StoreName
        {
            get { return _storeName; }
            set { _storeName = value; }
        }

        [DataMember(Name = "FileName", Order = 2, IsRequired = true)]
        public string FileName;

        [DataMember(Name = "Length", Order = 3, IsRequired = true)]
        public long Length;

        [DataMember(Name = "FilePath", Order = 4)]
        public string FilePath;

        [DataMember(Name = "FileByteStream", Order = 5)]
        public System.IO.Stream FileByteStream;

        [DataMember(Name = "ByteSize", Order = 6)]
        public byte[] ByteSize;

        private int _numbersOfFiles;
        /// <summary>
        /// Gets or sets the NumbersOfFiles
        /// </summary>
        [DataMember(Name = "NumbersOfFiles", Order = 7)]
        public int NumbersOfFiles
        {
            get { return _numbersOfFiles; }
            set { _numbersOfFiles = value; }
        }
    }

    [DataContract]
    public class ResultModel
    {
        private int _storeIndex;
        [DataMember]
        public int StoreIndex
        {
            get { return _storeIndex; }
            set { _storeIndex = value; }
        }

        [DataMember(Name = "Resource", Order = 0, IsRequired = true)]
        public string[] Resource;
    }
}