using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    public class base_TempModel : ModelBase
    {
        #region Constructors

        public base_TempModel()
        {

        }

        #endregion

        #region Primary Properties

        private int _id;
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(() => ID);
                }
            }
        }

        private string _resource;
        /// <summary>
        /// Gets or sets the Resource.
        /// </summary>
        public string Resource
        {
            get { return _resource; }
            set
            {
                if (_resource != value)
                {
                    _resource = value;
                    OnPropertyChanged(() => Resource);
                }
            }
        }

        private string _tableName;
        /// <summary>
        /// Gets or sets the TableName.
        /// </summary>
        public string TableName
        {
            get { return _tableName; }
            set
            {
                if (_tableName != value)
                {
                    _tableName = value;
                    OnPropertyChanged(() => TableName);
                }
            }
        }

        private short _status;
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public short Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(() => Status);
                }
            }
        }

        private DateTime _createdDate;
        /// <summary>
        /// Gets or sets the CreatedDate.
        /// </summary>
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                if (_createdDate != value)
                {
                    _createdDate = value;
                    OnPropertyChanged(() => CreatedDate);
                }
            }
        }

        private DateTime _updatedDate;
        /// <summary>
        /// Gets or sets the UpdatedDate.
        /// </summary>
        public DateTime UpdatedDate
        {
            get { return _updatedDate; }
            set
            {
                if (_updatedDate != value)
                {
                    _updatedDate = value;
                    OnPropertyChanged(() => UpdatedDate);
                }
            }
        }

        private DateTime _synchronizationDate;
        /// <summary>
        /// Gets or sets the SynchronizationDate.
        /// </summary>
        public DateTime SynchronizationDate
        {
            get { return _synchronizationDate; }
            set
            {
                if (_synchronizationDate != value)
                {
                    _synchronizationDate = value;
                    OnPropertyChanged(() => SynchronizationDate);
                }
            }
        }

        private bool _isSynchronous;
        /// <summary>
        /// Gets or sets the IsSynchronous.
        /// </summary>
        public bool IsSynchronous
        {
            get { return _isSynchronous; }
            set
            {
                if (_isSynchronous != value)
                {
                    _isSynchronous = value;
                    OnPropertyChanged(() => IsSynchronous);
                }
            }
        }

        #endregion
    }
}
