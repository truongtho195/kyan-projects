//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FlashCard.Models;
using FlashCard.Database;


namespace FlashCard.Database
{
    /// <summary>
    /// Model for table Kind 
    /// </summary>
    public partial class KindModel : ViewModelBase
    {
        #region Ctor

        // Default contructor
        public KindModel()
        {
            this.IsNew = true;
            this.Kind = new Kind();
        }

        // Default contructor that set entity to field
        public KindModel(Kind kind)
        {
            this.Kind = kind;
        }

        #endregion

        #region Entity Properties

        public Kind Kind { get; private set; }

        public bool IsNew { get; private set; }
        public bool IsDirty { get; private set; }
        public bool Deleted { get; set; }
        public bool Checked { get; set; }
        
        public void EndUpdate()
        {
            IsNew = false;
            IsDirty = false;
        }
        

        #endregion

        #region Primitive Properties

        public long KindID
        {
            get { return this.Kind.KindID; }
            set
            {
                if (this.Kind.KindID != value)
                {
                    this.IsDirty = true;
                    this.Kind.KindID = value;
                    RaisePropertyChanged(() => KindID);
                }
            }
        }
        public string Name
        {
            get { return this.Kind.Name; }
            set
            {
                if (this.Kind.Name != value)
                {
                    this.IsDirty = true;
                    this.Kind.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }
        public long KindOf
        {
            get { return this.Kind.KindOf; }
            set
            {
                if (this.Kind.KindOf != value)
                {
                    this.IsDirty = true;
                    this.Kind.KindOf = value;
                    RaisePropertyChanged(() => KindOf);
                }
            }
        }

        #endregion

        #region all the custom code


        #endregion
    }
}
