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
    /// Repository for table base_GuestPayRoll 
    /// </summary>
    public partial class base_GuestPayRollRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_GuestPayRollRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_GuestPayRoll.
        /// </summary>
        /// <param name="base_GuestPayRoll">base_GuestPayRoll to add.</param>
        /// <returns>base_GuestPayRoll have been added.</returns>
        public base_GuestPayRoll Add(base_GuestPayRoll base_GuestPayRoll)
        {
            UnitOfWork.Add<base_GuestPayRoll>(base_GuestPayRoll);
            return base_GuestPayRoll;
        }

        /// <summary>
        /// Adds a sequence of new base_GuestPayRoll.
        /// </summary>
        /// <param name="base_GuestPayRoll">Sequence of new base_GuestPayRoll to add.</param>
        /// <returns>Sequence of new base_GuestPayRoll have been added.</returns>
        public IEnumerable<base_GuestPayRoll> Add(IEnumerable<base_GuestPayRoll> base_GuestPayRoll)
        {
            UnitOfWork.Add<base_GuestPayRoll>(base_GuestPayRoll);
            return base_GuestPayRoll;
        }

        /// <summary>
        /// Delete a existed base_GuestPayRoll.
        /// </summary>
        /// <param name="base_GuestPayRoll">base_GuestPayRoll to delete.</param>
        public void Delete(base_GuestPayRoll base_GuestPayRoll)
        {
            Refresh(base_GuestPayRoll);
            if (base_GuestPayRoll.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_GuestPayRoll>(base_GuestPayRoll);
        }

        /// <summary>
        /// Delete a sequence of existed base_GuestPayRoll.
        /// </summary>
        /// <param name="base_GuestPayRoll">Sequence of existed base_GuestPayRoll to delete.</param>
        public void Delete(IEnumerable<base_GuestPayRoll> base_GuestPayRoll)
        {
            int total = base_GuestPayRoll.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_GuestPayRoll.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_GuestPayRoll of a sequence that satisfies a specified condition or 
        /// a default value if no such base_GuestPayRoll is found.
        /// </summary>
        /// <param name="expression">A function to test each base_GuestPayRoll for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_GuestPayRoll passes the test specified by expression; 
        /// otherwise, the first base_GuestPayRoll in source that passes the test specified by expression.
        /// </returns>
        public base_GuestPayRoll Get(Expression<Func<base_GuestPayRoll, bool>> expression)
        {
            return UnitOfWork.Get<base_GuestPayRoll>(expression);
        }

        /// <summary>
        /// Get all base_GuestPayRoll.
        /// </summary>
        /// <returns>The new IList&lt;base_GuestPayRoll&gt; instance.</returns>
        public IList<base_GuestPayRoll> GetAll()
        {
            return UnitOfWork.GetAll<base_GuestPayRoll>().ToList();
        }

        /// <summary>
        /// Get all base_GuestPayRoll that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_GuestPayRoll for a condition.</param>
        /// <returns>The new IList&lt;base_GuestPayRoll&gt; instance.</returns>
        public IList<base_GuestPayRoll> GetAll(Expression<Func<base_GuestPayRoll, bool>> expression)
        {
            return UnitOfWork.GetAll<base_GuestPayRoll>(expression).ToList();
        }

        /// <summary>
        /// Get all base_GuestPayRoll.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_GuestPayRoll&gt; instance.</returns>
        public IEnumerable<base_GuestPayRoll> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_GuestPayRoll>();
        }

        /// <summary>
        /// Get all base_GuestPayRoll that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_GuestPayRoll for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_GuestPayRoll&gt; instance.</returns>
        public IEnumerable<base_GuestPayRoll> GetIEnumerable(Expression<Func<base_GuestPayRoll, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_GuestPayRoll>(expression);
        }

        /// <summary>
        /// Get all base_GuestPayRoll.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_GuestPayRoll&gt; instance.</returns>
        public IQueryable<base_GuestPayRoll> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_GuestPayRoll>();
        }

        /// <summary>
        /// Get all base_GuestPayRoll that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_GuestPayRoll for a condition.</param>
        /// <returns>The new IQueryable&lt;base_GuestPayRoll&gt; instance.</returns>
        public IQueryable<base_GuestPayRoll> GetIQueryable(Expression<Func<base_GuestPayRoll, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_GuestPayRoll>(expression);
        }

        /// <summary>
        /// Take a few base_GuestPayRoll in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_GuestPayRoll will ignore.</param>
        /// <param name="takeCount">Number of base_GuestPayRoll will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_GuestPayRoll&gt; instance.</returns>
        public IList<base_GuestPayRoll> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_GuestPayRoll>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_GuestPayRoll in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_GuestPayRoll will ignore.</param>
        /// <param name="takeCount">Number of base_GuestPayRoll will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_GuestPayRoll for a condition.</param>
        /// <returns>The new IList&lt;base_GuestPayRoll&gt; instance.</returns>
        public IList<base_GuestPayRoll> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_GuestPayRoll, bool>> expression)
        {
            return UnitOfWork.GetRange<base_GuestPayRoll>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an base_GuestPayRoll in the object context with data from the data source.
        /// </summary>
        /// <param name="base_GuestPayRoll">The base_GuestPayRoll to be refreshed.</param>
        public base_GuestPayRoll Refresh(base_GuestPayRoll base_GuestPayRoll)
        {
            UnitOfWork.Refresh<base_GuestPayRoll>(base_GuestPayRoll);
            if (base_GuestPayRoll.EntityState != System.Data.EntityState.Detached)
                return base_GuestPayRoll;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_GuestPayRoll in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_GuestPayRoll">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_GuestPayRoll">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_GuestPayRoll> base_GuestPayRoll)
        {
            UnitOfWork.Refresh<base_GuestPayRoll>(base_GuestPayRoll);
        }

        /// <summary>
        /// Updates a sequence of base_GuestPayRoll in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_GuestPayRoll>();
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