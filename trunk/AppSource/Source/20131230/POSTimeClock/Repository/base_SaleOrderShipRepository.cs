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
using CPC.TimeClock.Database;

namespace CPC.TimeClock.Repository
{
    /// <summary>
    /// Repository for table base_SaleOrderShip 
    /// </summary>
    public partial class base_SaleOrderShipRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_SaleOrderShipRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_SaleOrderShip.
        /// </summary>
        /// <param name="base_SaleOrderShip">base_SaleOrderShip to add.</param>
        /// <returns>base_SaleOrderShip have been added.</returns>
        public base_SaleOrderShip Add(base_SaleOrderShip base_SaleOrderShip)
        {
            UnitOfWork.Add<base_SaleOrderShip>(base_SaleOrderShip);
            return base_SaleOrderShip;
        }

        /// <summary>
        /// Adds a sequence of new base_SaleOrderShip.
        /// </summary>
        /// <param name="base_SaleOrderShip">Sequence of new base_SaleOrderShip to add.</param>
        /// <returns>Sequence of new base_SaleOrderShip have been added.</returns>
        public IEnumerable<base_SaleOrderShip> Add(IEnumerable<base_SaleOrderShip> base_SaleOrderShip)
        {
            UnitOfWork.Add<base_SaleOrderShip>(base_SaleOrderShip);
            return base_SaleOrderShip;
        }

        /// <summary>
        /// Delete a existed base_SaleOrderShip.
        /// </summary>
        /// <param name="base_SaleOrderShip">base_SaleOrderShip to delete.</param>
        public void Delete(base_SaleOrderShip base_SaleOrderShip)
        {
            Refresh(base_SaleOrderShip);
            if (base_SaleOrderShip.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_SaleOrderShip>(base_SaleOrderShip);
        }

        /// <summary>
        /// Delete a sequence of existed base_SaleOrderShip.
        /// </summary>
        /// <param name="base_SaleOrderShip">Sequence of existed base_SaleOrderShip to delete.</param>
        public void Delete(IEnumerable<base_SaleOrderShip> base_SaleOrderShip)
        {
            int total = base_SaleOrderShip.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_SaleOrderShip.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_SaleOrderShip of a sequence that satisfies a specified condition or 
        /// a default value if no such base_SaleOrderShip is found.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderShip for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_SaleOrderShip passes the test specified by expression; 
        /// otherwise, the first base_SaleOrderShip in source that passes the test specified by expression.
        /// </returns>
        public base_SaleOrderShip Get(Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.Get<base_SaleOrderShip>(expression);
        }

        /// <summary>
        /// Get all base_SaleOrderShip.
        /// </summary>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetAll()
        {
            return UnitOfWork.GetAll<base_SaleOrderShip>();
        }

        /// <summary>
        /// Get all base_SaleOrderShip that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderShip for a condition.</param>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetAll(Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.GetAll<base_SaleOrderShip>(expression);
        }

        /// <summary>
        /// Get all base_SaleOrderShip.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_SaleOrderShip&gt; instance.</returns>
        public IEnumerable<base_SaleOrderShip> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_SaleOrderShip>();
        }

        /// <summary>
        /// Get all base_SaleOrderShip that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderShip for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_SaleOrderShip&gt; instance.</returns>
        public IEnumerable<base_SaleOrderShip> GetIEnumerable(Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_SaleOrderShip>(expression);
        }

        /// <summary>
        /// Get all base_SaleOrderShip.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_SaleOrderShip&gt; instance.</returns>
        public IQueryable<base_SaleOrderShip> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_SaleOrderShip>();
        }

        /// <summary>
        /// Get all base_SaleOrderShip that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_SaleOrderShip for a condition.</param>
        /// <returns>The new IQueryable&lt;base_SaleOrderShip&gt; instance.</returns>
        public IQueryable<base_SaleOrderShip> GetIQueryable(Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_SaleOrderShip>(expression);
        }

        /// <summary>
        /// Take a few base_SaleOrderShip in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleOrderShip will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderShip will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_SaleOrderShip>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_SaleOrderShip in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_SaleOrderShip will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderShip will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_SaleOrderShip for a condition.</param>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.GetRange<base_SaleOrderShip>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Take a few base_SaleOrderShip in sequence was sorted by descending on server.
        /// </summary>
        /// <typeparam name="TKey">Type of base_SaleOrderShip to sort</typeparam>
        /// <param name="ignoreCount">Number of base_SaleOrderShip will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderShip will take.</param>
        /// <param name="keySelector">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetRangeDescending<TKey>(int ignoreCount, int takeCount, Expression<Func<base_SaleOrderShip, TKey>> keySelector)
        {
            return UnitOfWork.GetRangeDescending(ignoreCount, takeCount, keySelector);
        }

        /// <summary>
        /// Take a few base_SaleOrderShip in sequence was sorted by descending on server.
        /// </summary>
        /// <typeparam name="TKey">Type of base_SaleOrderShip to sort</typeparam>
        /// <param name="ignoreCount">Number of base_SaleOrderShip will ignore.</param>
        /// <param name="takeCount">Number of base_SaleOrderShip will take.</param>
        /// <param name="keySelector">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>The new IList&lt;base_SaleOrderShip&gt; instance.</returns>
        public IList<base_SaleOrderShip> GetRangeDescending<TKey>(int ignoreCount, int takeCount, Expression<Func<base_SaleOrderShip, TKey>> keySelector, Expression<Func<base_SaleOrderShip, bool>> expression)
        {
            return UnitOfWork.GetRangeDescending(ignoreCount, takeCount, keySelector, expression);
        }

        /// <summary>
        /// Updates an base_SaleOrderShip in the object context with data from the data source.
        /// </summary>
        /// <param name="base_SaleOrderShip">The base_SaleOrderShip to be refreshed.</param>
        public base_SaleOrderShip Refresh(base_SaleOrderShip base_SaleOrderShip)
        {
            UnitOfWork.Refresh<base_SaleOrderShip>(base_SaleOrderShip);
            if (base_SaleOrderShip.EntityState != System.Data.EntityState.Detached)
                return base_SaleOrderShip;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_SaleOrderShip in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_SaleOrderShip">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_SaleOrderShip">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_SaleOrderShip> base_SaleOrderShip)
        {
            UnitOfWork.Refresh<base_SaleOrderShip>(base_SaleOrderShip);
        }

        /// <summary>
        /// Updates a sequence of base_SaleOrderShip in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_SaleOrderShip>();
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