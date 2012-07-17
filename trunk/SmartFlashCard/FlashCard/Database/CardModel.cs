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
using FlashCard.Models;


namespace FlashCard.Database
{
    /// <summary>
    /// Model for table Card 
    /// </summary>
    public partial class CardModel : ViewModelBase, IDataErrorInfo
    {
        #region Ctor

        // Default contructor
        public CardModel()
        {
            this.IsNew = true;
            this.Card = new Card();
        }

        // Default contructor that set entity to field
        public CardModel(Card card)
        {
            this.Card = card;
            ToModel();
        }

        #endregion

        #region Entity Properties

        public Card Card { get; private set; }

        protected bool _isNew;
        /// <summary>
        /// Gets or sets the IsNew
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    RaisePropertyChanged(() => IsNew);
                }
            }
        }

        protected bool _isDirty;
        /// <summary>
        /// Gets or sets the IsDirty
        /// </summary>
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        protected bool _isDeleted;
        /// <summary>
        /// Gets or sets the IsDeleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    RaisePropertyChanged(() => IsDeleted);
                }
            }
        }

        protected bool _isChecked;
        /// <summary>
        /// Gets or sets the IsChecked
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged(() => IsChecked);
                }
            }
        }

        public void EndUpdate()
        {
            this.IsNew = false;
            this.IsDirty = false;
        }

        public void ToEntity()
        {
            if (IsNew)
                this.Card.CardID = this.CardID;
            this.Card.CardName = this.CardName.Trim();
            this.Card.Remark = this.Remark.Trim();
        }

        public void ToModel()
        {
            this.CardID = this.Card.CardID;
            this.CardName = this.Card.CardName;
            this.Remark = this.Card.Remark;
        }

        #endregion

        #region Primitive Properties

        protected string _cardID;
        /// <summary>
        /// Gets or sets the CardID.
        /// </summary>
        public string CardID
        {
            get { return this._cardID; }
            set
            {
                if (this._cardID != value)
                {
                    this.IsDirty = true;
                    this._cardID = value;
                    RaisePropertyChanged(() => CardID);
                }
            }
        }

        protected string _cardName;
        /// <summary>
        /// Gets or sets the CardName.
        /// </summary>
        public string CardName
        {
            get { return this._cardName; }
            set
            {
                if (this._cardName != value)
                {
                    this.IsDirty = true;
                    this._cardName = value;
                    RaisePropertyChanged(() => CardName);
                }
            }
        }

        protected string _remark;
        /// <summary>
        /// Gets or sets the Remark.
        /// </summary>
        public string Remark
        {
            get { return this._remark; }
            set
            {
                if (this._remark != value)
                {
                    this.IsDirty = true;
                    this._remark = value;
                    RaisePropertyChanged(() => Remark);
                }
            }
        }


        #endregion

        #region Custom Code
     
        
        #region DataErrorInfo
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        private Dictionary<string, string> _errors = new Dictionary<string, string>();
        public Dictionary<string, string> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                if (_errors != value)
                {
                    _errors = value;
                    RaisePropertyChanged(() => Errors);
                }
            }
        }
        public string this[string columnName]
        {
            get
            {
                string message = String.Empty;
                this.Errors.Remove(columnName);
                switch (columnName)
                {
                   
                    case "Content":
                   
                        break;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    this.Errors.Add(columnName, message);
                }
                return message;
            }
        }
        #endregion

        #endregion
    }
}
