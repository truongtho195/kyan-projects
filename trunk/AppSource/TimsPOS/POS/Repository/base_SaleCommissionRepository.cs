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
    /// Repository for table base_SaleCommission 
    /// </summary>
    public partial class base_SaleCommissionRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_SaleCommissionRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_SaleCommission.
        /// </summary>
        /// <param name="base_SaleCommission">base_SaleCommission to add.</param>
        /// <returns>base_SaleCommission have been added.</returns>
        public base_SaleCommission Add(base_SaleCommission base_SaleCommission)
        {
            UnitOfWork.Add<base_SaleCommission>(base_SaleCommission);
            return base_SaleCommission;
        }

        /// <summary>
        /// Adds a sequence of new base_SaleCommission.
        /// </summary>
        /// <param name="base_SaleCommission">Sequence of new base_SaleCommission to add.</param>
        /// <returns>Sequence of new base_SaleCommission have been added.</returns>
        public IEnumerable<base_SaleCommission> Add(IEnumerable<base_SaleCommission> base_SaleCommission)
        {
            UnitOfWork.Add<base_SaleCommission>(base_SaleCommission);
            return base_SaleCommission;
        }

        /// <summary>
        /// Delete a existed base_SaleCommission.
        /// </summary>
        /// <param name="base_SaleCommission">base_SaleCommission to delete.</param>
        public void Delete(base_SaleCommission base_SaleCommission)
        {
            Refresh(base_SaleCommission);
            if (base_SaleCommission.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_SaleCommission>(base_SaleCommission);
        }

        /// <summary>
        /// Delete a sequence of existed base_SaleCommission.
        /// </summary>
        /// <param name="base_SaleCommission">Sequence of existed base_SaleCommission to delete.</param>
        public void Delete(IEnumerable<base_SaleCommission> base_SaleCommission)
        {
            int total = base_SaleCommission.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_SaleCommission.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_SaleCommission of a sequence that satisfies a specified condition or 
        /// a default value if no such base_SaleCommission is found.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleCommission for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_SaleCommission passes the test specified by expression; 
        /// otherwise, the first base_SaleCommission in source that passes the test specified by expression.
        /// </returns>
        public base_SaleCommission Get(Expression<Func<base_SaleCommission, bool>> expression)
        {
            return UnitOfWork.Get<base_SaleCommission>(expression);
        }

        /// <summary>
        /// Get all base_SaleCommission.
        /// </summary>
        /// <returns>The new IList&lt;base_SaleCommission&gt; instance.</returns>
        public IList<base_SaleCommission> GetAll()
        {
            return UnitOfWork.GetAll<base_SaleCommission>().ToList();
        }

        /// <summary>
        /// Get all base_SaleCommission that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleCommission for a condition.</param>
        /// <returns>The new IList&lt;base_SaleCommission&gt; instance.</returns>
        public IList<base_SaleCommission> GetAll(Expression<Func<base_SaleCommission, bool>> expression)
        {
            return UnitOfWork.GetAll<base_SaleCommission>(expression).ToList();
        }

        /// <summary>
        /// Get all base_SaleCommission.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_SaleCommission&gt; instance.</returns>
        public IEnumerable<base_SaleCommission> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_SaleCommission>();
        }

        /// <summary>
        /// Get all base_SaleCommission that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleCommission for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_SaleCommission&gt; instance.</returns>
        public IEnumerable<base_SaleCommission> GetIEnumerable(Expression<Func<base_SaleCommission, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_SaleCommission>(expression);
        }

        /// <summary>
        /// Get all base_SaleCommission.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_SaleCommission&gt; instance.</returns>
        public IQueryable<base_SaleCommission> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_SaleCommission>();
        }

        /// <summary>
        /// Get all base_SaleCommission that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleCommission for a condition.</param>
        /// <returns>The new IQueryable&lt;base_SaleCommission&gt; instance.</returns>
        public IQueryable<base_SaleCommission> GetIQueryable(Expression<Func<base_SaleCommission, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_SaleCommission>(expression);
        }

        /// <summary>
        /// Take a few base_SaleCommission in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleCommission will ignore.</param>
        /// <param name="takeCount">Number of base_SaleCommission will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_SaleCommission&gt; instance.</returns>
        public IList<base_SaleCommission> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_SaleCommission>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_SaleCommission in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleCommission will ignore.</param>
        /// <param name="takeCount">Number of base_SaleCommission will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_SaleCommission for a condition.</param>
        /// <returns>The new IList&lt;base_SaleCommission&gt; instance.</returns>
        public IList<base_SaleCommission> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_SaleCommission, bool>> expression)
        {
            return UnitOfWork.GetRange<base_SaleCommission>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an base_SaleCommission in the object context with data from the data source.
        /// </summary>
        /// <param name="base_SaleCommission">The base_SaleCommission to be refreshed.</param>
        public base_SaleCommission Refresh(base_SaleCommission base_SaleCommission)
        {
            UnitOfWork.Refresh<base_SaleCommission>(base_SaleCommission);
            if (base_SaleCommission.EntityState != System.Data.EntityState.Detached)
                return base_SaleCommission;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_SaleCommission in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_SaleCommission">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_SaleCommission">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_SaleCommission> base_SaleCommission)
        {
            UnitOfWork.Refresh<base_SaleCommission>(base_SaleCommission);
        }

        /// <summary>
        /// Updates a sequence of base_SaleCommission in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_SaleCommission>();
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
