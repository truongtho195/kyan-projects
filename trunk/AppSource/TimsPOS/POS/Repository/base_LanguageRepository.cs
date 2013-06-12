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
    /// Repository for table base_Language 
    /// </summary>
    public partial class base_LanguageRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_LanguageRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_Language.
        /// </summary>
        /// <param name="base_Language">base_Language to add.</param>
        /// <returns>base_Language have been added.</returns>
        public base_Language Add(base_Language base_Language)
        {
            UnitOfWork.Add<base_Language>(base_Language);
            return base_Language;
        }

        /// <summary>
        /// Adds a sequence of new base_Language.
        /// </summary>
        /// <param name="base_Language">Sequence of new base_Language to add.</param>
        /// <returns>Sequence of new base_Language have been added.</returns>
        public IEnumerable<base_Language> Add(IEnumerable<base_Language> base_Language)
        {
            UnitOfWork.Add<base_Language>(base_Language);
            return base_Language;
        }

        /// <summary>
        /// Delete a existed base_Language.
        /// </summary>
        /// <param name="base_Language">base_Language to delete.</param>
        public void Delete(base_Language base_Language)
        {
            Refresh(base_Language);
            if (base_Language.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_Language>(base_Language);
        }

        /// <summary>
        /// Delete a sequence of existed base_Language.
        /// </summary>
        /// <param name="base_Language">Sequence of existed base_Language to delete.</param>
        public void Delete(IEnumerable<base_Language> base_Language)
        {
            int total = base_Language.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_Language.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_Language of a sequence that satisfies a specified condition or 
        /// a default value if no such base_Language is found.
        /// </summary>
        /// <param name="expression">A function to test each base_Language for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_Language passes the test specified by expression; 
        /// otherwise, the first base_Language in source that passes the test specified by expression.
        /// </returns>
        public base_Language Get(Expression<Func<base_Language, bool>> expression)
        {
            return UnitOfWork.Get<base_Language>(expression);
        }

        /// <summary>
        /// Get all base_Language.
        /// </summary>
        /// <returns>The new IList&lt;base_Language&gt; instance.</returns>
        public IList<base_Language> GetAll()
        {
            return UnitOfWork.GetAll<base_Language>().ToList();
        }

        /// <summary>
        /// Get all base_Language that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_Language for a condition.</param>
        /// <returns>The new IList&lt;base_Language&gt; instance.</returns>
        public IList<base_Language> GetAll(Expression<Func<base_Language, bool>> expression)
        {
            return UnitOfWork.GetAll<base_Language>(expression).ToList();
        }

        /// <summary>
        /// Get all base_Language.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_Language&gt; instance.</returns>
        public IEnumerable<base_Language> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_Language>();
        }

        /// <summary>
        /// Get all base_Language that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_Language for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_Language&gt; instance.</returns>
        public IEnumerable<base_Language> GetIEnumerable(Expression<Func<base_Language, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_Language>(expression);
        }

        /// <summary>
        /// Get all base_Language.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_Language&gt; instance.</returns>
        public IQueryable<base_Language> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_Language>();
        }

        /// <summary>
        /// Get all base_Language that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_Language for a condition.</param>
        /// <returns>The new IQueryable&lt;base_Language&gt; instance.</returns>
        public IQueryable<base_Language> GetIQueryable(Expression<Func<base_Language, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_Language>(expression);
        }

        /// <summary>
        /// Take a few base_Language in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_Language will ignore.</param>
        /// <param name="takeCount">Number of base_Language will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_Language&gt; instance.</returns>
        public IList<base_Language> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_Language>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_Language in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_Language will ignore.</param>
        /// <param name="takeCount">Number of base_Language will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_Language for a condition.</param>
        /// <returns>The new IList&lt;base_Language&gt; instance.</returns>
        public IList<base_Language> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_Language, bool>> expression)
        {
            return UnitOfWork.GetRange<base_Language>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an base_Language in the object context with data from the data source.
        /// </summary>
        /// <param name="base_Language">The base_Language to be refreshed.</param>
        public base_Language Refresh(base_Language base_Language)
        {
            UnitOfWork.Refresh<base_Language>(base_Language);
            if (base_Language.EntityState != System.Data.EntityState.Detached)
                return base_Language;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_Language in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_Language">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_Language">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_Language> base_Language)
        {
            UnitOfWork.Refresh<base_Language>(base_Language);
        }

        /// <summary>
        /// Updates a sequence of base_Language in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_Language>();
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
