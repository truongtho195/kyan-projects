//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CPC.POS.Database;
using CPC.Toolkit.Base;
using CPC.POS.Interfaces;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_VirtualFolder
    /// </summary>
    [Serializable]
    public partial class base_VirtualFolderModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_VirtualFolderModel()
        {
            this.IsNew = true;
            this.base_VirtualFolder = new base_VirtualFolder();
        }

        // Default constructor that set entity to field
        public base_VirtualFolderModel(base_VirtualFolder base_virtualfolder, bool isRaiseProperties = false)
        {
            this.base_VirtualFolder = base_virtualfolder;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_VirtualFolder base_VirtualFolder { get; private set; }

        #endregion

        #region Primitive Properties

        protected int _id;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
        /// </summary>
        public int Id
        {
            get { return this._id; }
            set
            {
                if (this._id != value)
                {
                    this.IsDirty = true;
                    this._id = value;
                    OnPropertyChanged(() => Id);
                    PropertyChangedCompleted(() => Id);
                }
            }
        }

        protected Nullable<int> _parentFolderId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ParentFolderId</para>
        /// </summary>
        public Nullable<int> ParentFolderId
        {
            get { return this._parentFolderId; }
            set
            {
                if (this._parentFolderId != value)
                {
                    this.IsDirty = true;
                    this._parentFolderId = value;
                    OnPropertyChanged(() => ParentFolderId);
                    PropertyChangedCompleted(() => ParentFolderId);
                }
            }
        }

        protected string _folderName;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the FolderName</para>
        /// </summary>
        public string FolderName
        {
            get { return this._folderName; }
            set
            {
                if (this._folderName != value)
                {
                    this.IsDirty = true;
                    this._folderName = value;
                    OnPropertyChanged(() => FolderName);
                    PropertyChangedCompleted(() => FolderName);
                }
            }
        }

        protected bool _isActived;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsActived</para>
        /// </summary>
        public bool IsActived
        {
            get { return this._isActived; }
            set
            {
                if (this._isActived != value)
                {
                    this.IsDirty = true;
                    this._isActived = value;
                    OnPropertyChanged(() => IsActived);
                    PropertyChangedCompleted(() => IsActived);
                }
            }
        }

        protected Nullable<System.DateTime> _dateCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateCreated</para>
        /// </summary>
        public Nullable<System.DateTime> DateCreated
        {
            get { return this._dateCreated; }
            set
            {
                if (this._dateCreated != value)
                {
                    this.IsDirty = true;
                    this._dateCreated = value;
                    OnPropertyChanged(() => DateCreated);
                    PropertyChangedCompleted(() => DateCreated);
                }
            }
        }

        protected string _userCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserCreated</para>
        /// </summary>
        public string UserCreated
        {
            get { return this._userCreated; }
            set
            {
                if (this._userCreated != value)
                {
                    this.IsDirty = true;
                    this._userCreated = value;
                    OnPropertyChanged(() => UserCreated);
                    PropertyChangedCompleted(() => UserCreated);
                }
            }
        }

        protected Nullable<System.DateTime> _dateUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateUpdated</para>
        /// </summary>
        public Nullable<System.DateTime> DateUpdated
        {
            get { return this._dateUpdated; }
            set
            {
                if (this._dateUpdated != value)
                {
                    this.IsDirty = true;
                    this._dateUpdated = value;
                    OnPropertyChanged(() => DateUpdated);
                    PropertyChangedCompleted(() => DateUpdated);
                }
            }
        }

        protected string _userUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the UserUpdated</para>
        /// </summary>
        public string UserUpdated
        {
            get { return this._userUpdated; }
            set
            {
                if (this._userUpdated != value)
                {
                    this.IsDirty = true;
                    this._userUpdated = value;
                    OnPropertyChanged(() => UserUpdated);
                    PropertyChangedCompleted(() => UserUpdated);
                }
            }
        }

        protected System.Guid _resource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Resource</para>
        /// </summary>
        public System.Guid Resource
        {
            get { return this._resource; }
            set
            {
                if (this._resource != value)
                {
                    this.IsDirty = true;
                    this._resource = value;
                    OnPropertyChanged(() => Resource);
                    PropertyChangedCompleted(() => Resource);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// <para>Public Method</para>
        /// Method for set IsNew & IsDirty = false;
        /// </summary>
        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set PropertyModel to Entity</para>
        /// </summary>
        public void ToEntity()
        {
            if (IsNew)
                this.base_VirtualFolder.Id = this.Id;
            this.base_VirtualFolder.ParentFolderId = this.ParentFolderId;
            this.base_VirtualFolder.FolderName = this.FolderName;
            this.base_VirtualFolder.IsActived = this.IsActived;
            this.base_VirtualFolder.DateCreated = this.DateCreated;
            this.base_VirtualFolder.UserCreated = this.UserCreated;
            this.base_VirtualFolder.DateUpdated = this.DateUpdated;
            this.base_VirtualFolder.UserUpdated = this.UserUpdated;
            this.base_VirtualFolder.Resource = this.Resource;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_VirtualFolder.Id;
            this._parentFolderId = this.base_VirtualFolder.ParentFolderId;
            this._folderName = this.base_VirtualFolder.FolderName;
            this._isActived = this.base_VirtualFolder.IsActived;
            this._dateCreated = this.base_VirtualFolder.DateCreated;
            this._userCreated = this.base_VirtualFolder.UserCreated;
            this._dateUpdated = this.base_VirtualFolder.DateUpdated;
            this._userUpdated = this.base_VirtualFolder.UserUpdated;
            this._resource = this.base_VirtualFolder.Resource;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_VirtualFolder.Id;
            this.ParentFolderId = this.base_VirtualFolder.ParentFolderId;
            this.FolderName = this.base_VirtualFolder.FolderName;
            this.IsActived = this.base_VirtualFolder.IsActived;
            this.DateCreated = this.base_VirtualFolder.DateCreated;
            this.UserCreated = this.base_VirtualFolder.UserCreated;
            this.DateUpdated = this.base_VirtualFolder.DateUpdated;
            this.UserUpdated = this.base_VirtualFolder.UserUpdated;
            this.Resource = this.base_VirtualFolder.Resource;
        }

        #endregion

        #region Custom Code

        #region Navigation Properties

        #region ParentFolder

        private base_VirtualFolderModel _parentFolder;
        /// <summary>
        /// Gets or sets parent folder.
        /// </summary>
        public base_VirtualFolderModel ParentFolder
        {
            get
            {
                return _parentFolder;
            }
            set
            {
                if (_parentFolder != value)
                {
                    _isDirty = true;
                    _parentFolder = value;
                    OnPropertyChanged(() => ParentFolder);
                }
            }
        }

        #endregion

        #region FolderCollection

        private CollectionBase<base_VirtualFolderModel> _folderCollection;
        /// <summary>
        /// Gets or sets folder collection that contains child folders.
        /// </summary>
        public CollectionBase<base_VirtualFolderModel> FolderCollection
        {
            get
            {
                return _folderCollection;
            }
            set
            {
                if (_folderCollection != value)
                {
                    _isDirty = true;
                    _folderCollection = value;
                    OnPropertyChanged(() => FolderCollection);
                }
            }
        }

        #endregion

        #region FileCollection

        private CollectionBase<base_AttachmentModel> _fileCollection;
        /// <summary>
        /// Gets or sets file collection that contains child files.
        /// </summary>
        public CollectionBase<base_AttachmentModel> FileCollection
        {
            get
            {
                return _fileCollection;
            }
            set
            {
                if (_fileCollection != value)
                {
                    _isDirty = true;
                    _fileCollection = value;
                    OnPropertyChanged(() => FileCollection);
                }
            }
        }

        #endregion

        #endregion

        #region Properties

        #region EditFolderProvider

        private IEditableFolder _editFolderProvider;
        /// <summary>
        /// Used for update folder.
        /// </summary>
        public IEditableFolder EditFolderProvider
        {
            get
            {
                return _editFolderProvider;
            }
            set
            {
                if (_editFolderProvider != value)
                {
                    _editFolderProvider = value;
                }
            }
        }

        #endregion

        #region FileTotal

        private int _fileTotal;
        /// <summary>
        /// Gets or sets file total.
        /// </summary>
        public int FileTotal
        {
            get
            {
                return _fileTotal;
            }
            set
            {
                if (_fileTotal != value)
                {
                    _isDirty = true;
                    _fileTotal = value;
                    OnPropertyChanged(() => FileTotal);
                }
            }
        }

        #endregion

        #region IsRoot

        /// <summary>
        /// Determine whether folder is root.
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return _parentFolderId == null;
            }
        }

        #endregion

        #region IsFilesLoaded

        /// <summary>
        /// Determine whether FileCollection was loaded.
        /// </summary>
        public bool IsFilesLoaded
        {
            get
            {
                return _fileCollection != null;
            }
        }

        #endregion

        #region IsExpanded

        private bool _isExpanded;
        /// <summary>
        /// Gets or sets whether the child items in this object are expanded or collapsed. 
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsExpanded);
                }
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected;
        /// <summary>
        /// Gets or sets whether this object is selected. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        #endregion

        #region IsEdit

        private bool _isEdit;
        public bool IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                if (_isEdit != value)
                {
                    _isEdit = value;
                    OnPropertyChanged(() => IsEdit);
                    PropertyChangedCompleted(() => IsEdit);
                }
            }
        }

        #endregion

        #region IsCheckedAllFiles

        private bool? _isCheckedAllFiles = false;
        /// <summary>
        /// Determine whether all files in FileCollection was checked.
        /// </summary>
        public bool? IsCheckedAllFiles
        {
            get
            {
                return _isCheckedAllFiles;
            }
            set
            {
                if (_isCheckedAllFiles != value)
                {
                    _isCheckedAllFiles = value;
                    OnPropertyChanged(() => IsCheckedAllFiles);
                    PropertyChangedCompleted(() => IsCheckedAllFiles);
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region DetermineIsCheckedAllFiles

        /// <summary>
        /// Determine IsCheckedAllFiles property.
        /// </summary>
        public void DetermineIsCheckedAllFiles()
        {
            if (_fileCollection != null)
            {
                int countItems = _fileCollection.Count;
                int countChecked = _fileCollection.Count(x => x.IsChecked);

                if (countChecked == 0)
                {
                    _isCheckedAllFiles = false;
                    OnPropertyChanged(() => IsCheckedAllFiles);
                }
                else if (countChecked < countItems)
                {
                    _isCheckedAllFiles = null;
                    OnPropertyChanged(() => IsCheckedAllFiles);
                }
                else
                {
                    _isCheckedAllFiles = true;
                    OnPropertyChanged(() => IsCheckedAllFiles);
                }
            }
        }

        #endregion

        #endregion

        #region Override Methods

        #region PropertyChangedCompleted

        protected override void PropertyChangedCompleted(string propertyName)
        {
            switch (propertyName)
            {
                case "IsEdit":

                    if (!_isEdit)
                    {
                        if (_isDirty)
                        {
                            if (_editFolderProvider != null)
                            {
                                _editFolderProvider.Update(this);
                            }
                        }
                    }

                    break;

                case "IsCheckedAllFiles":

                    if (_fileCollection != null)
                    {
                        if (_isCheckedAllFiles == true)
                        {
                            foreach (base_AttachmentModel file in _fileCollection)
                            {
                                file.Checked();
                            }
                        }
                        else if (_isCheckedAllFiles == false)
                        {
                            foreach (base_AttachmentModel file in _fileCollection)
                            {
                                file.Unchecked();
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string message = null;

                switch (columnName)
                {
                    case "FolderName":
                        break;
                }

                return message;
            }
        }

        #endregion

        #endregion
    }
}