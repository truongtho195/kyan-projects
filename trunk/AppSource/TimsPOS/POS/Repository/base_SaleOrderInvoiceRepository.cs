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
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using CPC.POS.Database;

namespace CPC.POS.Repository
{
    /// <summary>
    /// Repository for table base_SaleOrderInvoice 
    /// </summary>
    public partial class base_SaleOrderInvoiceRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_SaleOrderInvoiceRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_SaleOrderInvoice.
        /// </summary>
        /// <param name="base_SaleOrderInvoice">base_SaleOrderInvoice to add.</param>
        /// <returns>base_SaleOrderInvoice have been added.</returns>
        public base_SaleOrderInvoice Add(base_SaleOrderInvoice base_SaleOrderInvoice)
        {
            UnitOfWork.Add<base_SaleOrderInvoice>(base_SaleOrderInvoice);
            return base_SaleOrderInvoice;
        }

        /// <summary>
        /// Adds a sequence of new base_SaleOrderInvoice.
        /// </summary>
        /// <param name="base_SaleOrderInvoice">Sequence of new base_SaleOrderInvoice to add.</param>
        /// <returns>Sequence of new base_SaleOrderInvoice have been added.</returns>
        public IEnumerable<base_SaleOrderInvoice> Add(IEnumerable<base_SaleOrderInvoice> base_SaleOrderInvoice)
        {
            UnitOfWork.Add<base_SaleOrderInvoice>(base_SaleOrderInvoice);
            return base_SaleOrderInvoice;
        }

        /// <summary>
        /// Delete a existed base_SaleOrderInvoice.
        /// </summary>
        /// <param name="base_SaleOrderInvoice">base_SaleOrderInvoice to delete.</param>
        public void Delete(base_SaleOrderInvoice base_SaleOrderInvoice)
        {
            Refresh(base_SaleOrderInvoice);
            if (base_SaleOrderInvoice.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_SaleOrderInvoice>(base_SaleOrderInvoice);
        }

        /// <summary>
        /// Delete a sequence of existed base_SaleOrderInvoice.
        /// </summary>
        /// <param name="base_SaleOrderInvoice">Sequence of existed base_SaleOrderInvoice to delete.</param>
        public void Delete(IEnumerable<base_SaleOrderInvoice> base_SaleOrderInvoice)
        {
            int total = base_SaleOrderInvoice.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_SaleOrderInvoice.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_SaleOrderInvoice of a sequence that satisfies a specified condition or 
        /// a default value if no such base_SaleOrderInvoice is found.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderInvoice for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_SaleOrderInvoice passes the test specified by expression; 
        /// otherwise, the first base_SaleOrderInvoice in source that passes the test specified by expression.
        /// </returns>
        public base_SaleOrderInvoice Get(Expression<Func<base_SaleOrderInvoice, bool>> expression)
        {
            return UnitOfWork.Get<base_SaleOrderInvoice>(expression);
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice.
        /// </summary>
        /// <returns>The new IList&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IList<base_SaleOrderInvoice> GetAll()
        {
            return UnitOfWork.GetAll<base_SaleOrderInvoice>().ToList();
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderInvoice for a condition.</param>
        /// <returns>The new IList&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IList<base_SaleOrderInvoice> GetAll(Expression<Func<base_SaleOrderInvoice, bool>> expression)
        {
            return UnitOfWork.GetAll<base_SaleOrderInvoice>(expression).ToList();
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IEnumerable<base_SaleOrderInvoice> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_SaleOrderInvoice>();
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderInvoice for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IEnumerable<base_SaleOrderInvoice> GetIEnumerable(Expression<Func<base_SaleOrderInvoice, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_SaleOrderInvoice>(expression);
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IQueryable<base_SaleOrderInvoice> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_SaleOrderInvoice>();
        }

        /// <summary>
        /// Get all base_SaleOrderInvoice that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderInvoice for a condition.</param>
        /// <returns>The new IQueryable&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IQueryable<base_SaleOrderInvoice> GetIQueryable(Expression<Func<base_SaleOrderInvoice, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_SaleOrderInvoice>(expression);
        }

        /// <summary>
        /// Take a few base_SaleOrderInvoice in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleOrderInvoice will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderInvoice will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IList<base_SaleOrderInvoice> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_SaleOrderInvoice>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_SaleOrderInvoice in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleOrderInvoice will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderInvoice will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_SaleOrderInvoice for a condition.</param>
        /// <returns>The new IList&lt;base_SaleOrderInvoice&gt; instance.</returns>
        public IList<base_SaleOrderInvoice> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_SaleOrderInvoice, bool>> expression)
        {
            return UnitOfWork.GetRange<base_SaleOrderInvoice>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an base_SaleOrderInvoice in the object context with data from the data source.
        /// </summary>
        /// <param name="base_SaleOrderInvoice">The base_SaleOrderInvoice to be refreshed.</param>
        public base_SaleOrderInvoice Refresh(base_SaleOrderInvoice base_SaleOrderInvoice)
        {
            UnitOfWork.Refresh<base_SaleOrderInvoice>(base_SaleOrderInvoice);
            if (base_SaleOrderInvoice.EntityState != System.Data.EntityState.Detached)
                return base_SaleOrderInvoice;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_SaleOrderInvoice in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_SaleOrderInvoice">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_SaleOrderInvoice">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_SaleOrderInvoice> base_SaleOrderInvoice)
        {
            UnitOfWork.Refresh<base_SaleOrderInvoice>(base_SaleOrderInvoice);
        }

        /// <summary>
        /// Updates a sequence of base_SaleOrderInvoice in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_SaleOrderInvoice>();
        }

        /// <summary>
        /// Persists all updates to the data source and resets change tracking in the object context.
        /// </summary>
        public void Commit()
        {
            UnitOfWork.Commit();
        }

        /// <summary>
        /// Persists all updates to the data source with the specified System.Data.Objects.SaveOptions.
        /// </summary>
        /// <param name="options">A System.Data.Objects.SaveOptions value that determines the behavior of the operation.</param>
        public void Commit(SaveOptions options)
        {
            UnitOfWork.Commit(options);
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            UnitOfWork.BeginTransaction();
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            UnitOfWork.CommitTransaction();
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public void RollbackTransaction()
        {
            UnitOfWork.RollbackTransaction();
        }

        #endregion

        #endregion

        #region Custom Code


        #endregion
    }
}