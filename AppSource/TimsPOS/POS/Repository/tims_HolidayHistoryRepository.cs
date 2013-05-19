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
    /// Repository for table tims_HolidayHistory 
    /// </summary>
    public partial class tims_HolidayHistoryRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public tims_HolidayHistoryRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new tims_HolidayHistory.
        /// </summary>
        /// <param name="tims_HolidayHistory">tims_HolidayHistory to add.</param>
        /// <returns>tims_HolidayHistory have been added.</returns>
        public tims_HolidayHistory Add(tims_HolidayHistory tims_HolidayHistory)
        {
            UnitOfWork.Add<tims_HolidayHistory>(tims_HolidayHistory);
            return tims_HolidayHistory;
        }

        /// <summary>
        /// Adds a sequence of new tims_HolidayHistory.
        /// </summary>
        /// <param name="tims_HolidayHistory">Sequence of new tims_HolidayHistory to add.</param>
        /// <returns>Sequence of new tims_HolidayHistory have been added.</returns>
        public IEnumerable<tims_HolidayHistory> Add(IEnumerable<tims_HolidayHistory> tims_HolidayHistory)
        {
            UnitOfWork.Add<tims_HolidayHistory>(tims_HolidayHistory);
            return tims_HolidayHistory;
        }

        /// <summary>
        /// Delete a existed tims_HolidayHistory.
        /// </summary>
        /// <param name="tims_HolidayHistory">tims_HolidayHistory to delete.</param>
        public void Delete(tims_HolidayHistory tims_HolidayHistory)
        {
            Refresh(tims_HolidayHistory);
            if (tims_HolidayHistory.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<tims_HolidayHistory>(tims_HolidayHistory);
        }

        /// <summary>
        /// Delete a sequence of existed tims_HolidayHistory.
        /// </summary>
        /// <param name="tims_HolidayHistory">Sequence of existed tims_HolidayHistory to delete.</param>
        public void Delete(IEnumerable<tims_HolidayHistory> tims_HolidayHistory)
        {
            int total = tims_HolidayHistory.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(tims_HolidayHistory.ElementAt(i));
        }

        /// <summary>
        /// Returns the first tims_HolidayHistory of a sequence that satisfies a specified condition or 
        /// a default value if no such tims_HolidayHistory is found.
        /// </summary>
        /// <param name="expression">A function to test each tims_HolidayHistory for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no tims_HolidayHistory passes the test specified by expression; 
        /// otherwise, the first tims_HolidayHistory in source that passes the test specified by expression.
        /// </returns>
        public tims_HolidayHistory Get(Expression<Func<tims_HolidayHistory, bool>> expression)
        {
            return UnitOfWork.Get<tims_HolidayHistory>(expression);
        }

        /// <summary>
        /// Get all tims_HolidayHistory.
        /// </summary>
        /// <returns>The new IList&lt;tims_HolidayHistory&gt; instance.</returns>
        public IList<tims_HolidayHistory> GetAll()
        {
            return UnitOfWork.GetAll<tims_HolidayHistory>().ToList();
        }

        /// <summary>
        /// Get all tims_HolidayHistory that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each tims_HolidayHistory for a condition.</param>
        /// <returns>The new IList&lt;tims_HolidayHistory&gt; instance.</returns>
        public IList<tims_HolidayHistory> GetAll(Expression<Func<tims_HolidayHistory, bool>> expression)
        {
            return UnitOfWork.GetAll<tims_HolidayHistory>(expression).ToList();
        }

        /// <summary>
        /// Get all tims_HolidayHistory.
        /// </summary>
        /// <returns>The new IEnumerable&lt;tims_HolidayHistory&gt; instance.</returns>
        public IEnumerable<tims_HolidayHistory> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<tims_HolidayHistory>();
        }

        /// <summary>
        /// Get all tims_HolidayHistory that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each tims_HolidayHistory for a condition.</param>
        /// <returns>The new IEnumerable&lt;tims_HolidayHistory&gt; instance.</returns>
        public IEnumerable<tims_HolidayHistory> GetIEnumerable(Expression<Func<tims_HolidayHistory, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<tims_HolidayHistory>(expression);
        }

        /// <summary>
        /// Get all tims_HolidayHistory.
        /// </summary>
        /// <returns>The new IQueryable&lt;tims_HolidayHistory&gt; instance.</returns>
        public IQueryable<tims_HolidayHistory> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<tims_HolidayHistory>();
        }

        /// <summary>
        /// Get all tims_HolidayHistory that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each tims_HolidayHistory for a condition.</param>
        /// <returns>The new IQueryable&lt;tims_HolidayHistory&gt; instance.</returns>
        public IQueryable<tims_HolidayHistory> GetIQueryable(Expression<Func<tims_HolidayHistory, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<tims_HolidayHistory>(expression);
        }

        /// <summary>
        /// Take a few tims_HolidayHistory in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of tims_HolidayHistory will ignore.</param>
        /// <param name="takeCount">Number of tims_HolidayHistory will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;tims_HolidayHistory&gt; instance.</returns>
        public IList<tims_HolidayHistory> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<tims_HolidayHistory>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few tims_HolidayHistory in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of tims_HolidayHistory will ignore.</param>
        /// <param name="takeCount">Number of tims_HolidayHistory will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each tims_HolidayHistory for a condition.</param>
        /// <returns>The new IList&lt;tims_HolidayHistory&gt; instance.</returns>
        public IList<tims_HolidayHistory> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<tims_HolidayHistory, bool>> expression)
        {
            return UnitOfWork.GetRange<tims_HolidayHistory>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an tims_HolidayHistory in the object context with data from the data source.
        /// </summary>
        /// <param name="tims_HolidayHistory">The tims_HolidayHistory to be refreshed.</param>
        public tims_HolidayHistory Refresh(tims_HolidayHistory tims_HolidayHistory)
        {
            UnitOfWork.Refresh<tims_HolidayHistory>(tims_HolidayHistory);
            if (tims_HolidayHistory.EntityState != System.Data.EntityState.Detached)
                return tims_HolidayHistory;
            return null;
        }

        /// <summary>
        /// Updates a sequence of tims_HolidayHistory in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="tims_HolidayHistory">Type of object in a sequence to refresh.</typeparam>
        /// <param name="tims_HolidayHistory">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<tims_HolidayHistory> tims_HolidayHistory)
        {
            UnitOfWork.Refresh<tims_HolidayHistory>(tims_HolidayHistory);
        }

        /// <summary>
        /// Updates a sequence of tims_HolidayHistory in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<tims_HolidayHistory>();
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
