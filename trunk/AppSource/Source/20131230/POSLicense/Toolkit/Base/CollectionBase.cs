using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CPC.Toolkit.Base
{
    [Serializable]
    public class CollectionBase<T> : ObservableCollection<T> where T : ModelBase
    {

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the CollectionBase class.
        /// </summary>
        public CollectionBase()
            : base()
        {
            DeletedItems = new ObservableCollection<T>();
        }

        /// <summary>
        /// Initializes a new instance of the CollectionBase class that contains elements copied from 
        /// the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception>
        public CollectionBase(IEnumerable<T> collection)
            : base(collection)
        {
            DeletedItems = new ObservableCollection<T>();
        }

        /// <summary>
        /// Initializes a new instance of CollectionBase class that contains elements copied from 
        /// the specified list.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <exception cref="System.ArgumentNullException">The list parameter cannot be null.</exception>
        public CollectionBase(List<T> list)
            : base(list)
        {
            DeletedItems = new ObservableCollection<T>();
        }

        #endregion

        #region Properties

        #region IsDirty

        /// <summary>
        /// Gets a value that determines whether this collection has new items, dirty items, or deleted items.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return (DeletedItems.Count > 0)
                    || this.Count(x => !x.IsDeleted && x.IsNew && (x.GetType().GetProperty("IsTemporary") != null ? !(bool)x.GetType().GetProperty("IsTemporary").GetValue(x, null) : true)) > 0
                    || this.Count(x => !x.IsNew && !x.IsDeleted && x.IsDirty) > 0;
            }
        }

        #endregion

        #region NewItems

        /// <summary>
        /// Gets new items in this collection.
        /// </summary>
        public ObservableCollection<T> NewItems
        {
            get
            {
                return new ObservableCollection<T>(this.Where(x =>
                    !x.IsDeleted &&
                    x.IsNew &&
                    (x.GetType().GetProperty("IsTemporary") != null ?
                    !(bool)x.GetType().GetProperty("IsTemporary").GetValue(x, null) : true)));
            }
        }

        #endregion

        #region DirtyItems

        /// <summary>
        /// Gets dirty items in this collection.
        /// </summary>
        public ObservableCollection<T> DirtyItems
        {
            get
            {
                return new ObservableCollection<T>(this.Where(x =>
                    !x.IsNew &&
                    !x.IsDeleted &&
                    x.IsDirty &&
                    (x.GetType().GetProperty("IsTemporary") != null ?
                    !(bool)x.GetType().GetProperty("IsTemporary").GetValue(x, null) : true)));
            }
        }

        #endregion

        #region DeletedItems

        /// <summary>
        /// Gets deleted items in this collection.
        /// </summary>
        public ObservableCollection<T> DeletedItems
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            DeletedItems.Clear();
            base.ClearItems();
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            ModelBase modelBase = this[index] as ModelBase;
            base.RemoveItem(index);
            if (!modelBase.IsNew)
            {
                modelBase.IsDeleted = true;
                DeletedItems.Add((T)modelBase);
            }
        }

        #endregion

    }
}
