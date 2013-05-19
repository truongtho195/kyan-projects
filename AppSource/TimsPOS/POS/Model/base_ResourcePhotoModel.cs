//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CPC.POS.Database;
using CPC.Toolkit.Base;

namespace CPC.POS.Model
{
    /// <summary>
    /// Model for table base_ResourcePhoto
    /// </summary>
    [Serializable]
    public partial class base_ResourcePhotoModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_ResourcePhotoModel()
        {
            this.IsNew = true;
            this.base_ResourcePhoto = new base_ResourcePhoto();
        }

        // Default constructor that set entity to field
        public base_ResourcePhotoModel(base_ResourcePhoto base_resourcephoto, bool isRaiseProperties = false)
        {
            this.base_ResourcePhoto = base_resourcephoto;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_ResourcePhoto base_ResourcePhoto { get; private set; }

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

        protected byte[] _thumbnailPhoto;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ThumbnailPhoto</para>
        /// </summary>
        public byte[] ThumbnailPhoto
        {
            get { return this._thumbnailPhoto; }
            set
            {
                if (this._thumbnailPhoto != value)
                {
                    this.IsDirty = true;
                    this._thumbnailPhoto = value;
                    OnPropertyChanged(() => ThumbnailPhoto);
                    PropertyChangedCompleted(() => ThumbnailPhoto);
                }
            }
        }

        protected string _thumbnailPhotoFilename;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ThumbnailPhotoFilename</para>
        /// </summary>
        public string ThumbnailPhotoFilename
        {
            get { return this._thumbnailPhotoFilename; }
            set
            {
                if (this._thumbnailPhotoFilename != value)
                {
                    this.IsDirty = true;
                    this._thumbnailPhotoFilename = value;
                    OnPropertyChanged(() => ThumbnailPhotoFilename);
                    PropertyChangedCompleted(() => ThumbnailPhotoFilename);
                }
            }
        }

        protected byte[] _largePhoto;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the LargePhoto</para>
        /// </summary>
        public byte[] LargePhoto
        {
            get { return this._largePhoto; }
            set
            {
                if (this._largePhoto != value)
                {
                    this.IsDirty = true;
                    this._largePhoto = value;
                    OnPropertyChanged(() => LargePhoto);
                    PropertyChangedCompleted(() => LargePhoto);
                }
            }
        }

        protected string _largePhotoFilename;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the LargePhotoFilename</para>
        /// </summary>
        public string LargePhotoFilename
        {
            get { return this._largePhotoFilename; }
            set
            {
                if (this._largePhotoFilename != value)
                {
                    this.IsDirty = true;
                    this._largePhotoFilename = value;
                    OnPropertyChanged(() => LargePhotoFilename);
                    PropertyChangedCompleted(() => LargePhotoFilename);
                }
            }
        }

        protected Nullable<short> _sortId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the SortId</para>
        /// </summary>
        public Nullable<short> SortId
        {
            get { return this._sortId; }
            set
            {
                if (this._sortId != value)
                {
                    this.IsDirty = true;
                    this._sortId = value;
                    OnPropertyChanged(() => SortId);
                    PropertyChangedCompleted(() => SortId);
                }
            }
        }

        protected string _resource;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Resource</para>
        /// </summary>
        public string Resource
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
                this.base_ResourcePhoto.Id = this.Id;
            this.base_ResourcePhoto.ThumbnailPhoto = this.ThumbnailPhoto;
            this.base_ResourcePhoto.ThumbnailPhotoFilename = this.ThumbnailPhotoFilename;
            this.base_ResourcePhoto.LargePhoto = this.LargePhoto;
            this.base_ResourcePhoto.LargePhotoFilename = this.LargePhotoFilename;
            this.base_ResourcePhoto.SortId = this.SortId;
            this.base_ResourcePhoto.Resource = this.Resource;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_ResourcePhoto.Id;
            this._thumbnailPhoto = this.base_ResourcePhoto.ThumbnailPhoto;
            this._thumbnailPhotoFilename = this.base_ResourcePhoto.ThumbnailPhotoFilename;
            this._largePhoto = this.base_ResourcePhoto.LargePhoto;
            this._largePhotoFilename = this.base_ResourcePhoto.LargePhotoFilename;
            this._sortId = this.base_ResourcePhoto.SortId;
            this._resource = this.base_ResourcePhoto.Resource;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_ResourcePhoto.Id;
            this.ThumbnailPhoto = this.base_ResourcePhoto.ThumbnailPhoto;
            this.ThumbnailPhotoFilename = this.base_ResourcePhoto.ThumbnailPhotoFilename;
            this.LargePhoto = this.base_ResourcePhoto.LargePhoto;
            this.LargePhotoFilename = this.base_ResourcePhoto.LargePhotoFilename;
            this.SortId = this.base_ResourcePhoto.SortId;
            this.Resource = this.base_ResourcePhoto.Resource;
        }

        #endregion

        #region Custom Code

        protected string _imagePath;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the ImagePath</para>
        /// </summary>
        public string ImagePath
        {
            get { return this._imagePath; }
            set
            {
                if (this._imagePath != value)
                {
                    this.IsDirty = true;
                    this._imagePath = value;
                    OnPropertyChanged(() => ImagePath);
                    PropertyChangedCompleted(() => ImagePath);
                }
            }
        }

        /// <summary>
        /// Copy values 
        /// </summary>
        /// <param name="resourcePhotoModel"></param>
        public void ToModel(base_ResourcePhotoModel resourcePhotoModel)
        {
            //this._id = resourcePhotoModel.Id;
            this._thumbnailPhoto = resourcePhotoModel.ThumbnailPhoto;
            this._thumbnailPhotoFilename = resourcePhotoModel.ThumbnailPhotoFilename;
            this._largePhoto = resourcePhotoModel.LargePhoto;
            this._largePhotoFilename = resourcePhotoModel.LargePhotoFilename;
            //this._sortId = resourcePhotoModel.SortId;
            //this._resource = resourcePhotoModel.Resource;
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string message = string.Empty;

                switch (columnName)
                {
                    case "Id":
                        break;
                    case "ThumbnailPhoto":
                        break;
                    case "ThumbnailPhotoFilename":
                        break;
                    case "LargePhoto":
                        break;
                    case "LargePhotoFilename":
                        break;
                    case "SortId":
                        break;
                    case "Resource":
                        break;
                }

                if (!string.IsNullOrWhiteSpace(message))
                    return message;
                return null;
            }
        }

        #endregion
    }
}
