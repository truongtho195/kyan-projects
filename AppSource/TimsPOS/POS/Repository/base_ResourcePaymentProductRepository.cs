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
    /// Repository for table base_ResourcePaymentProduct 
    /// </summary>
    public partial class base_ResourcePaymentProductRepository
    {
        #region Auto Generate Code

        #region Constructors

        // Default constructor
        public base_ResourcePaymentProductRepository()
        {
        }

        #endregion

        #region Basic C.R.U.D. Operations

        /// <summary>
        /// Add new base_ResourcePaymentProduct.
        /// </summary>
        /// <param name="base_ResourcePaymentProduct">base_ResourcePaymentProduct to add.</param>
        /// <returns>base_ResourcePaymentProduct have been added.</returns>
        public base_ResourcePaymentProduct Add(base_ResourcePaymentProduct base_ResourcePaymentProduct)
        {
            UnitOfWork.Add<base_ResourcePaymentProduct>(base_ResourcePaymentProduct);
            return base_ResourcePaymentProduct;
        }

        /// <summary>
        /// Adds a sequence of new base_ResourcePaymentProduct.
        /// </summary>
        /// <param name="base_ResourcePaymentProduct">Sequence of new base_ResourcePaymentProduct to add.</param>
        /// <returns>Sequence of new base_ResourcePaymentProduct have been added.</returns>
        public IEnumerable<base_ResourcePaymentProduct> Add(IEnumerable<base_ResourcePaymentProduct> base_ResourcePaymentProduct)
        {
            UnitOfWork.Add<base_ResourcePaymentProduct>(base_ResourcePaymentProduct);
            return base_ResourcePaymentProduct;
        }

        /// <summary>
        /// Delete a existed base_ResourcePaymentProduct.
        /// </summary>
        /// <param name="base_ResourcePaymentProduct">base_ResourcePaymentProduct to delete.</param>
        public void Delete(base_ResourcePaymentProduct base_ResourcePaymentProduct)
        {
            Refresh(base_ResourcePaymentProduct);
            if (base_ResourcePaymentProduct.EntityState != System.Data.EntityState.Detached)
                UnitOfWork.Delete<base_ResourcePaymentProduct>(base_ResourcePaymentProduct);
        }

        /// <summary>
        /// Delete a sequence of existed base_ResourcePaymentProduct.
        /// </summary>
        /// <param name="base_ResourcePaymentProduct">Sequence of existed base_ResourcePaymentProduct to delete.</param>
        public void Delete(IEnumerable<base_ResourcePaymentProduct> base_ResourcePaymentProduct)
        {
            int total = base_ResourcePaymentProduct.Count();
            for (int i = total - 1; i >= 0; i--)
                Delete(base_ResourcePaymentProduct.ElementAt(i));
        }

        /// <summary>
        /// Returns the first base_ResourcePaymentProduct of a sequence that satisfies a specified condition or 
        /// a default value if no such base_ResourcePaymentProduct is found.
        /// </summary>
        /// <param name="expression">A function to test each base_ResourcePaymentProduct for a condition.</param>
        /// <returns>    
        /// Null if source is empty or if no base_ResourcePaymentProduct passes the test specified by expression; 
        /// otherwise, the first base_ResourcePaymentProduct in source that passes the test specified by expression.
        /// </returns>
        public base_ResourcePaymentProduct Get(Expression<Func<base_ResourcePaymentProduct, bool>> expression)
        {
            return UnitOfWork.Get<base_ResourcePaymentProduct>(expression);
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct.
        /// </summary>
        /// <returns>The new IList&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IList<base_ResourcePaymentProduct> GetAll()
        {
            return UnitOfWork.GetAll<base_ResourcePaymentProduct>().ToList();
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_ResourcePaymentProduct for a condition.</param>
        /// <returns>The new IList&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IList<base_ResourcePaymentProduct> GetAll(Expression<Func<base_ResourcePaymentProduct, bool>> expression)
        {
            return UnitOfWork.GetAll<base_ResourcePaymentProduct>(expression).ToList();
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct.
        /// </summary>
        /// <returns>The new IEnumerable&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IEnumerable<base_ResourcePaymentProduct> GetIEnumerable()
        {
            return UnitOfWork.GetIEnumerable<base_ResourcePaymentProduct>();
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_ResourcePaymentProduct for a condition.</param>
        /// <returns>The new IEnumerable&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IEnumerable<base_ResourcePaymentProduct> GetIEnumerable(Expression<Func<base_ResourcePaymentProduct, bool>> expression)
        {
            return UnitOfWork.GetIEnumerable<base_ResourcePaymentProduct>(expression);
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct.
        /// </summary>
        /// <returns>The new IQueryable&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IQueryable<base_ResourcePaymentProduct> GetIQueryable()
        {
            return UnitOfWork.GetIQueryable<base_ResourcePaymentProduct>();
        }

        /// <summary>
        /// Get all base_ResourcePaymentProduct that satisfies a specified condition.
        /// </summary>
        /// <param name="expression">A function to test each base_ResourcePaymentProduct for a condition.</param>
        /// <returns>The new IQueryable&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IQueryable<base_ResourcePaymentProduct> GetIQueryable(Expression<Func<base_ResourcePaymentProduct, bool>> expression)
        {
            return UnitOfWork.GetIQueryable<base_ResourcePaymentProduct>(expression);
        }

        /// <summary>
        /// Take a few base_ResourcePaymentProduct in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_ResourcePaymentProduct will ignore.</param>
        /// <param name="takeCount">Number of base_ResourcePaymentProduct will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IList<base_ResourcePaymentProduct> GetRange(int ignoreCount, int takeCount, string keys)
        {
            return UnitOfWork.GetRange<base_ResourcePaymentProduct>(ignoreCount, takeCount, keys);
        }

        /// <summary>
        /// Take a few base_ResourcePaymentProduct in a sequence was sorted on server.
        /// </summary>
        /// <param name="ignoreCount">Number of base_ResourcePaymentProduct will ignore.</param>
        /// <param name="takeCount">Number of base_ResourcePaymentProduct will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each base_ResourcePaymentProduct for a condition.</param>
        /// <returns>The new IList&lt;base_ResourcePaymentProduct&gt; instance.</returns>
        public IList<base_ResourcePaymentProduct> GetRange(int ignoreCount, int takeCount, string keys, Expression<Func<base_ResourcePaymentProduct, bool>> expression)
        {
            return UnitOfWork.GetRange<base_ResourcePaymentProduct>(ignoreCount, takeCount, keys, expression);
        }

        /// <summary>
        /// Updates an base_ResourcePaymentProduct in the object context with data from the data source.
        /// </summary>
        /// <param name="base_ResourcePaymentProduct">The base_ResourcePaymentProduct to be refreshed.</param>
        public base_ResourcePaymentProduct Refresh(base_ResourcePaymentProduct base_ResourcePaymentProduct)
        {
            UnitOfWork.Refresh<base_ResourcePaymentProduct>(base_ResourcePaymentProduct);
            if (base_ResourcePaymentProduct.EntityState != System.Data.EntityState.Detached)
                return base_ResourcePaymentProduct;
            return null;
        }

        /// <summary>
        /// Updates a sequence of base_ResourcePaymentProduct in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="base_ResourcePaymentProduct">Type of object in a sequence to refresh.</typeparam>
        /// <param name="base_ResourcePaymentProduct">Object collection to be refreshed.</param>
        public void Refresh(IEnumerable<base_ResourcePaymentProduct> base_ResourcePaymentProduct)
        {
            UnitOfWork.Refresh<base_ResourcePaymentProduct>(base_ResourcePaymentProduct);
        }

        /// <summary>
        /// Updates a sequence of base_ResourcePaymentProduct in the object context with data from the data source.
        /// </summary>
        public void Refresh()
        {
            UnitOfWork.Refresh<base_ResourcePaymentProduct>();
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
