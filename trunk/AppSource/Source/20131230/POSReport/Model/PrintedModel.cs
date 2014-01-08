using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolkit.Base;

namespace CPC.POSReport.Model
{
    public class PrintedModel : ModelBase
    {
        public PrintedModel()
        { }

        protected DateTime _createdDate;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Created Date</para>
        /// </summary>
        public DateTime CreatedDate
        {
            get { return this._createdDate; }
            set
            {
                if (this._createdDate != value)
                {
                    this._createdDate = value;
                    OnPropertyChanged(() => CreatedDate);
                    PropertyChangedCompleted(() => CreatedDate);
                }
            }
        }

        protected string _fileName;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the File Name</para>
        /// </summary>
        public string FileName
        {
            get { return this._fileName; }
            set
            {
                if (this._fileName != value)
                {
                    this.IsDirty = true;
                    this._fileName = value;
                    OnPropertyChanged(() => FileName);
                    PropertyChangedCompleted(() => FileName);
                }
            }
        }

        protected string _filePath;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the LoginName</para>
        /// </summary>
        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (this._filePath != value)
                {
                    this.IsDirty = true;
                    this._filePath = value;
                    OnPropertyChanged(() => FilePath);
                    PropertyChangedCompleted(() => FilePath);
                }
            }
        }
    }
}
