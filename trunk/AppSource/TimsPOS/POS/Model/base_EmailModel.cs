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
    /// Model for table base_Email
    /// </summary>
    [Serializable]
    public partial class base_EmailModel : ModelBase, IDataErrorInfo
    {
        #region Constructor

        // Default constructor
        public base_EmailModel()
        {
            this.IsNew = true;
            this.base_Email = new base_Email();
        }

        // Default constructor that set entity to field
        public base_EmailModel(base_Email base_email, bool isRaiseProperties = false)
        {
            this.base_Email = base_email;
            if (!isRaiseProperties)
                this.ToModel();
            else
                this.ToModelAndRaise();
            this.IsDirty = false;
        }

        #endregion

        #region Entity Properties

        public base_Email base_Email { get; private set; }

        #endregion

        #region Primitive Properties

        protected System.Guid _id;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Id</para>
        /// </summary>
        public System.Guid Id
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

        protected string _recipient;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Recipient</para>
        /// </summary>
        public string Recipient
        {
            get { return this._recipient; }
            set
            {
                if (this._recipient != value)
                {
                    this.IsDirty = true;
                    this._recipient = value;
                    OnPropertyChanged(() => Recipient);
                    PropertyChangedCompleted(() => Recipient);
                }
            }
        }

        protected string _cC;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the CC</para>
        /// </summary>
        public string CC
        {
            get { return this._cC; }
            set
            {
                if (this._cC != value)
                {
                    this.IsDirty = true;
                    this._cC = value;
                    OnPropertyChanged(() => CC);
                    PropertyChangedCompleted(() => CC);
                }
            }
        }

        protected string _bCC;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the BCC</para>
        /// </summary>
        public string BCC
        {
            get { return this._bCC; }
            set
            {
                if (this._bCC != value)
                {
                    this.IsDirty = true;
                    this._bCC = value;
                    OnPropertyChanged(() => BCC);
                    PropertyChangedCompleted(() => BCC);
                }
            }
        }

        protected string _subject;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Subject</para>
        /// </summary>
        public string Subject
        {
            get { return this._subject; }
            set
            {
                if (this._subject != value)
                {
                    this.IsDirty = true;
                    this._subject = value;
                    OnPropertyChanged(() => Subject);
                    PropertyChangedCompleted(() => Subject);
                }
            }
        }

        protected string _body;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Body</para>
        /// </summary>
        public string Body
        {
            get { return this._body; }
            set
            {
                if (this._body != value)
                {
                    this.IsDirty = true;
                    this._body = value;
                    OnPropertyChanged(() => Body);
                    PropertyChangedCompleted(() => Body);
                }
            }
        }

        protected bool _isHasAttachment;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsHasAttachment</para>
        /// </summary>
        public bool IsHasAttachment
        {
            get { return this._isHasAttachment; }
            set
            {
                if (this._isHasAttachment != value)
                {
                    this.IsDirty = true;
                    this._isHasAttachment = value;
                    OnPropertyChanged(() => IsHasAttachment);
                    PropertyChangedCompleted(() => IsHasAttachment);
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

        protected string _dateCreated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateCreated</para>
        /// </summary>
        public string DateCreated
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

        protected string _dateUpdated;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the DateUpdated</para>
        /// </summary>
        public string DateUpdated
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

        protected string _attachmentType;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AttachmentType</para>
        /// </summary>
        public string AttachmentType
        {
            get { return this._attachmentType; }
            set
            {
                if (this._attachmentType != value)
                {
                    this.IsDirty = true;
                    this._attachmentType = value;
                    OnPropertyChanged(() => AttachmentType);
                    PropertyChangedCompleted(() => AttachmentType);
                }
            }
        }

        protected string _attachmentResult;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the AttachmentResult</para>
        /// </summary>
        public string AttachmentResult
        {
            get { return this._attachmentResult; }
            set
            {
                if (this._attachmentResult != value)
                {
                    this.IsDirty = true;
                    this._attachmentResult = value;
                    OnPropertyChanged(() => AttachmentResult);
                    PropertyChangedCompleted(() => AttachmentResult);
                }
            }
        }

        protected Nullable<int> _guestId;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the GuestId</para>
        /// </summary>
        public Nullable<int> GuestId
        {
            get { return this._guestId; }
            set
            {
                if (this._guestId != value)
                {
                    this.IsDirty = true;
                    this._guestId = value;
                    OnPropertyChanged(() => GuestId);
                    PropertyChangedCompleted(() => GuestId);
                }
            }
        }

        protected string _sender;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Sender</para>
        /// </summary>
        public string Sender
        {
            get { return this._sender; }
            set
            {
                if (this._sender != value)
                {
                    this.IsDirty = true;
                    this._sender = value;
                    OnPropertyChanged(() => Sender);
                    PropertyChangedCompleted(() => Sender);
                }
            }
        }

        protected Nullable<short> _status;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Status</para>
        /// </summary>
        public Nullable<short> Status
        {
            get { return this._status; }
            set
            {
                if (this._status != value)
                {
                    this.IsDirty = true;
                    this._status = value;
                    OnPropertyChanged(() => Status);
                    PropertyChangedCompleted(() => Status);
                }
            }
        }

        protected Nullable<short> _importance;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Importance</para>
        /// </summary>
        public Nullable<short> Importance
        {
            get { return this._importance; }
            set
            {
                if (this._importance != value)
                {
                    this.IsDirty = true;
                    this._importance = value;
                    OnPropertyChanged(() => Importance);
                    PropertyChangedCompleted(() => Importance);
                }
            }
        }

        protected Nullable<short> _sensitivity;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the Sensitivity</para>
        /// </summary>
        public Nullable<short> Sensitivity
        {
            get { return this._sensitivity; }
            set
            {
                if (this._sensitivity != value)
                {
                    this.IsDirty = true;
                    this._sensitivity = value;
                    OnPropertyChanged(() => Sensitivity);
                    PropertyChangedCompleted(() => Sensitivity);
                }
            }
        }

        protected bool _isRequestDelivery;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsRequestDelivery</para>
        /// </summary>
        public bool IsRequestDelivery
        {
            get { return this._isRequestDelivery; }
            set
            {
                if (this._isRequestDelivery != value)
                {
                    this.IsDirty = true;
                    this._isRequestDelivery = value;
                    OnPropertyChanged(() => IsRequestDelivery);
                    PropertyChangedCompleted(() => IsRequestDelivery);
                }
            }
        }

        protected bool _isRequestRead;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsRequestRead</para>
        /// </summary>
        public bool IsRequestRead
        {
            get { return this._isRequestRead; }
            set
            {
                if (this._isRequestRead != value)
                {
                    this.IsDirty = true;
                    this._isRequestRead = value;
                    OnPropertyChanged(() => IsRequestRead);
                    PropertyChangedCompleted(() => IsRequestRead);
                }
            }
        }

        protected Nullable<bool> _isMyFlag;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsMyFlag</para>
        /// </summary>
        public Nullable<bool> IsMyFlag
        {
            get { return this._isMyFlag; }
            set
            {
                if (this._isMyFlag != value)
                {
                    this.IsDirty = true;
                    this._isMyFlag = value;
                    OnPropertyChanged(() => IsMyFlag);
                    PropertyChangedCompleted(() => IsMyFlag);
                }
            }
        }

        protected Nullable<short> _flagTo;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the FlagTo</para>
        /// </summary>
        public Nullable<short> FlagTo
        {
            get { return this._flagTo; }
            set
            {
                if (this._flagTo != value)
                {
                    this.IsDirty = true;
                    this._flagTo = value;
                    OnPropertyChanged(() => FlagTo);
                    PropertyChangedCompleted(() => FlagTo);
                }
            }
        }

        protected Nullable<int> _flagStartDate;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the FlagStartDate</para>
        /// </summary>
        public Nullable<int> FlagStartDate
        {
            get { return this._flagStartDate; }
            set
            {
                if (this._flagStartDate != value)
                {
                    this.IsDirty = true;
                    this._flagStartDate = value;
                    OnPropertyChanged(() => FlagStartDate);
                    PropertyChangedCompleted(() => FlagStartDate);
                }
            }
        }

        protected Nullable<int> _flagDueDate;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the FlagDueDate</para>
        /// </summary>
        public Nullable<int> FlagDueDate
        {
            get { return this._flagDueDate; }
            set
            {
                if (this._flagDueDate != value)
                {
                    this.IsDirty = true;
                    this._flagDueDate = value;
                    OnPropertyChanged(() => FlagDueDate);
                    PropertyChangedCompleted(() => FlagDueDate);
                }
            }
        }

        protected Nullable<bool> _isAllowReminder;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsAllowReminder</para>
        /// </summary>
        public Nullable<bool> IsAllowReminder
        {
            get { return this._isAllowReminder; }
            set
            {
                if (this._isAllowReminder != value)
                {
                    this.IsDirty = true;
                    this._isAllowReminder = value;
                    OnPropertyChanged(() => IsAllowReminder);
                    PropertyChangedCompleted(() => IsAllowReminder);
                }
            }
        }

        protected Nullable<System.DateTime> _remindOn;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the RemindOn</para>
        /// </summary>
        public Nullable<System.DateTime> RemindOn
        {
            get { return this._remindOn; }
            set
            {
                if (this._remindOn != value)
                {
                    this.IsDirty = true;
                    this._remindOn = value;
                    OnPropertyChanged(() => RemindOn);
                    PropertyChangedCompleted(() => RemindOn);
                }
            }
        }

        protected Nullable<short> _myRemindTimes;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the MyRemindTimes</para>
        /// </summary>
        public Nullable<short> MyRemindTimes
        {
            get { return this._myRemindTimes; }
            set
            {
                if (this._myRemindTimes != value)
                {
                    this.IsDirty = true;
                    this._myRemindTimes = value;
                    OnPropertyChanged(() => MyRemindTimes);
                    PropertyChangedCompleted(() => MyRemindTimes);
                }
            }
        }

        protected Nullable<bool> _isRecipentFlag;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsRecipentFlag</para>
        /// </summary>
        public Nullable<bool> IsRecipentFlag
        {
            get { return this._isRecipentFlag; }
            set
            {
                if (this._isRecipentFlag != value)
                {
                    this.IsDirty = true;
                    this._isRecipentFlag = value;
                    OnPropertyChanged(() => IsRecipentFlag);
                    PropertyChangedCompleted(() => IsRecipentFlag);
                }
            }
        }

        protected Nullable<short> _recipentFlagTo;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the RecipentFlagTo</para>
        /// </summary>
        public Nullable<short> RecipentFlagTo
        {
            get { return this._recipentFlagTo; }
            set
            {
                if (this._recipentFlagTo != value)
                {
                    this.IsDirty = true;
                    this._recipentFlagTo = value;
                    OnPropertyChanged(() => RecipentFlagTo);
                    PropertyChangedCompleted(() => RecipentFlagTo);
                }
            }
        }

        protected Nullable<bool> _isAllowRecipentReminder;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the IsAllowRecipentReminder</para>
        /// </summary>
        public Nullable<bool> IsAllowRecipentReminder
        {
            get { return this._isAllowRecipentReminder; }
            set
            {
                if (this._isAllowRecipentReminder != value)
                {
                    this.IsDirty = true;
                    this._isAllowRecipentReminder = value;
                    OnPropertyChanged(() => IsAllowRecipentReminder);
                    PropertyChangedCompleted(() => IsAllowRecipentReminder);
                }
            }
        }

        protected Nullable<System.DateTime> _recipentRemindOn;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the RecipentRemindOn</para>
        /// </summary>
        public Nullable<System.DateTime> RecipentRemindOn
        {
            get { return this._recipentRemindOn; }
            set
            {
                if (this._recipentRemindOn != value)
                {
                    this.IsDirty = true;
                    this._recipentRemindOn = value;
                    OnPropertyChanged(() => RecipentRemindOn);
                    PropertyChangedCompleted(() => RecipentRemindOn);
                }
            }
        }

        protected Nullable<short> _recipentRemindTimes;
        /// <summary>
        /// Property Model
        /// <para>Gets or sets the RecipentRemindTimes</para>
        /// </summary>
        public Nullable<short> RecipentRemindTimes
        {
            get { return this._recipentRemindTimes; }
            set
            {
                if (this._recipentRemindTimes != value)
                {
                    this.IsDirty = true;
                    this._recipentRemindTimes = value;
                    OnPropertyChanged(() => RecipentRemindTimes);
                    PropertyChangedCompleted(() => RecipentRemindTimes);
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
                this.base_Email.Id = this.Id;
            this.base_Email.Recipient = this.Recipient;
            this.base_Email.CC = this.CC;
            this.base_Email.BCC = this.BCC;
            this.base_Email.Subject = this.Subject;
            this.base_Email.Body = this.Body;
            this.base_Email.IsHasAttachment = this.IsHasAttachment;
            this.base_Email.UserCreated = this.UserCreated;
            this.base_Email.DateCreated = this.DateCreated;
            this.base_Email.UserUpdated = this.UserUpdated;
            this.base_Email.DateUpdated = this.DateUpdated;
            this.base_Email.AttachmentType = this.AttachmentType;
            this.base_Email.AttachmentResult = this.AttachmentResult;
            this.base_Email.GuestId = this.GuestId;
            this.base_Email.Sender = this.Sender;
            this.base_Email.Status = this.Status;
            this.base_Email.Importance = this.Importance;
            this.base_Email.Sensitivity = this.Sensitivity;
            this.base_Email.IsRequestDelivery = this.IsRequestDelivery;
            this.base_Email.IsRequestRead = this.IsRequestRead;
            this.base_Email.IsMyFlag = this.IsMyFlag;
            this.base_Email.FlagTo = this.FlagTo;
            this.base_Email.FlagStartDate = this.FlagStartDate;
            this.base_Email.FlagDueDate = this.FlagDueDate;
            this.base_Email.IsAllowReminder = this.IsAllowReminder;
            this.base_Email.RemindOn = this.RemindOn;
            this.base_Email.MyRemindTimes = this.MyRemindTimes;
            this.base_Email.IsRecipentFlag = this.IsRecipentFlag;
            this.base_Email.RecipentFlagTo = this.RecipentFlagTo;
            this.base_Email.IsAllowRecipentReminder = this.IsAllowRecipentReminder;
            this.base_Email.RecipentRemindOn = this.RecipentRemindOn;
            this.base_Email.RecipentRemindTimes = this.RecipentRemindTimes;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModel()
        {
            this._id = this.base_Email.Id;
            this._recipient = this.base_Email.Recipient;
            this._cC = this.base_Email.CC;
            this._bCC = this.base_Email.BCC;
            this._subject = this.base_Email.Subject;
            this._body = this.base_Email.Body;
            this._isHasAttachment = this.base_Email.IsHasAttachment;
            this._userCreated = this.base_Email.UserCreated;
            this._dateCreated = this.base_Email.DateCreated;
            this._userUpdated = this.base_Email.UserUpdated;
            this._dateUpdated = this.base_Email.DateUpdated;
            this._attachmentType = this.base_Email.AttachmentType;
            this._attachmentResult = this.base_Email.AttachmentResult;
            this._guestId = this.base_Email.GuestId;
            this._sender = this.base_Email.Sender;
            this._status = this.base_Email.Status;
            this._importance = this.base_Email.Importance;
            this._sensitivity = this.base_Email.Sensitivity;
            this._isRequestDelivery = this.base_Email.IsRequestDelivery;
            this._isRequestRead = this.base_Email.IsRequestRead;
            this._isMyFlag = this.base_Email.IsMyFlag;
            this._flagTo = this.base_Email.FlagTo;
            this._flagStartDate = this.base_Email.FlagStartDate;
            this._flagDueDate = this.base_Email.FlagDueDate;
            this._isAllowReminder = this.base_Email.IsAllowReminder;
            this._remindOn = this.base_Email.RemindOn;
            this._myRemindTimes = this.base_Email.MyRemindTimes;
            this._isRecipentFlag = this.base_Email.IsRecipentFlag;
            this._recipentFlagTo = this.base_Email.RecipentFlagTo;
            this._isAllowRecipentReminder = this.base_Email.IsAllowRecipentReminder;
            this._recipentRemindOn = this.base_Email.RecipentRemindOn;
            this._recipentRemindTimes = this.base_Email.RecipentRemindTimes;
        }

        /// <summary>
        /// Public Method
        /// <para>Method for set Entity to PropertyModel</para>
        /// </summary
        public void ToModelAndRaise()
        {
            this.Id = this.base_Email.Id;
            this.Recipient = this.base_Email.Recipient;
            this.CC = this.base_Email.CC;
            this.BCC = this.base_Email.BCC;
            this.Subject = this.base_Email.Subject;
            this.Body = this.base_Email.Body;
            this.IsHasAttachment = this.base_Email.IsHasAttachment;
            this.UserCreated = this.base_Email.UserCreated;
            this.DateCreated = this.base_Email.DateCreated;
            this.UserUpdated = this.base_Email.UserUpdated;
            this.DateUpdated = this.base_Email.DateUpdated;
            this.AttachmentType = this.base_Email.AttachmentType;
            this.AttachmentResult = this.base_Email.AttachmentResult;
            this.GuestId = this.base_Email.GuestId;
            this.Sender = this.base_Email.Sender;
            this.Status = this.base_Email.Status;
            this.Importance = this.base_Email.Importance;
            this.Sensitivity = this.base_Email.Sensitivity;
            this.IsRequestDelivery = this.base_Email.IsRequestDelivery;
            this.IsRequestRead = this.base_Email.IsRequestRead;
            this.IsMyFlag = this.base_Email.IsMyFlag;
            this.FlagTo = this.base_Email.FlagTo;
            this.FlagStartDate = this.base_Email.FlagStartDate;
            this.FlagDueDate = this.base_Email.FlagDueDate;
            this.IsAllowReminder = this.base_Email.IsAllowReminder;
            this.RemindOn = this.base_Email.RemindOn;
            this.MyRemindTimes = this.base_Email.MyRemindTimes;
            this.IsRecipentFlag = this.base_Email.IsRecipentFlag;
            this.RecipentFlagTo = this.base_Email.RecipentFlagTo;
            this.IsAllowRecipentReminder = this.base_Email.IsAllowRecipentReminder;
            this.RecipentRemindOn = this.base_Email.RecipentRemindOn;
            this.RecipentRemindTimes = this.base_Email.RecipentRemindTimes;
        }

        #endregion

        #region Custom Code


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
                    case "Recipient":
                        break;
                    case "CC":
                        break;
                    case "BCC":
                        break;
                    case "Subject":
                        break;
                    case "Body":
                        break;
                    case "IsHasAttachment":
                        break;
                    case "UserCreated":
                        break;
                    case "DateCreated":
                        break;
                    case "UserUpdated":
                        break;
                    case "DateUpdated":
                        break;
                    case "AttachmentType":
                        break;
                    case "AttachmentResult":
                        break;
                    case "GuestId":
                        break;
                    case "Sender":
                        break;
                    case "Status":
                        break;
                    case "Importance":
                        break;
                    case "Sensitivity":
                        break;
                    case "IsRequestDelivery":
                        break;
                    case "IsRequestRead":
                        break;
                    case "IsMyFlag":
                        break;
                    case "FlagTo":
                        break;
                    case "FlagStartDate":
                        break;
                    case "FlagDueDate":
                        break;
                    case "IsAllowReminder":
                        break;
                    case "RemindOn":
                        break;
                    case "MyRemindTimes":
                        break;
                    case "IsRecipentFlag":
                        break;
                    case "RecipentFlagTo":
                        break;
                    case "IsAllowRecipentReminder":
                        break;
                    case "RecipentRemindOn":
                        break;
                    case "RecipentRemindTimes":
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
