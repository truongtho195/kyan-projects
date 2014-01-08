using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace CPC.POSReport.Database
{
    public sealed class UnitOfWork : IDisposable
    {
        #region Fields

        private static IDbTransaction _transaction;
        private static ObjectContext _objectContext;
        public static readonly object Locker;

        #endregion

        #region Contructors

        static UnitOfWork()
        {
            if (_objectContext == null)
            {
                _objectContext = new POSEntities(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString);
            }

            if (Locker == null)
            {
                Locker = new object();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an object to the object context in the current entity set.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entity">The object to add.</param>
        public static void Add<T>(T entity) where T : class
        {
            _objectContext.CreateObjectSet<T>().AddObject(entity);
        }

        /// <summary>
        /// Adds a sequence of objects to the object context in the current entity set.
        /// </summary>
        /// <typeparam name="T">Type of object in collection.</typeparam>
        /// <param name="collection">A collection contains objects that represents the entities to add.</param>
        public static void Add<T>(IEnumerable<T> collection) where T : class
        {
            foreach (T entity in collection)
            {
                Add<T>(entity);
            }
        }

        /// <summary>
        /// Marks an object for deletion.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entity">
        /// An object that represents the entity to delete.
        /// The object can be in any state except System.Data.EntityState.Detached.
        /// </param>
        public static void Delete<T>(T entity) where T : class
        {
            _objectContext.DeleteObject(entity);
        }

        /// <summary>
        /// Marks a sequence of objects for deletion.
        /// </summary>
        /// <typeparam name="T">Type of object in collection.</typeparam>
        /// <param name="collection">
        /// A collection contains objects that represents the entities to delete.
        /// The objects can be in any state except System.Data.EntityState.Detached.
        /// </param>
        //public static void Delete<T>(IEnumerable<T> collection) where T : class
        //{
        //    int total = collection.Count();
        //    for (int i = total - 1; i >= 0; i--)
        //    {
        //        Delete<T>(collection.ElementAt(i));
        //    }
        //}

        /// <summary>
        /// Returns the first object of a sequence that satisfies a specified condition or 
        /// a default value if no such object is found.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>
        /// default(TSource) if source is empty or if no object passes the test specified by expression; 
        /// otherwise, the first object in source that passes the test specified by expression.
        /// </returns>
        public static T Get<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _objectContext.CreateObjectSet<T>().FirstOrDefault(expression);
        }

        /// <summary>
        /// Returns the first object of a sequence or a default value if no such object is found.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <returns>
        /// default(TSource) if source is empty; otherwise, the first element in source.
        /// </returns>
        public static T Get<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get all objects that type is T.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetAll<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>().ToList();
        }

        /// <summary>
        /// Get all objects that type is T and satisfies a specified condition.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetAll<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _objectContext.CreateObjectSet<T>().Where(expression).ToList();
        }

        /// <summary>
        /// Get all objects that type is T.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <returns>The new IEnumerable<T> instance.</returns>
        public static IEnumerable<T> GetIEnumerable<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>();
        }

        /// <summary>
        /// Get all objects that type is T and satisfies a specified condition.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>The new IEnumerable<T> instance.</returns>
        public static IEnumerable<T> GetIEnumerable<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _objectContext.CreateObjectSet<T>().Where(expression);
        }

        /// <summary>
        /// Get all objects that type is T.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <returns>
        /// An new System.Linq.IQueryable<T> instance.
        /// </returns>
        public static IQueryable<T> GetIQueryable<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>();
        }

        /// <summary>
        /// Get all objects that type is T and satisfies a specified condition.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>
        /// An System.Linq.IQueryable<T> that contains objects from the input sequence that 
        /// satisfy the condition specified by predicate.
        /// </returns>
        public static IQueryable<T> GetIQueryable<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _objectContext.CreateObjectSet<T>().Where(expression);
        }

        /// <summary>
        /// Take a few objects in a sequence was sorted on server.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="ignoreCount">Number of objects will ignore.</param>
        /// <param name="takeCount">Number of objects will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetRange<T>(int ignoreCount, int takeCount, string keys) where T : class
        {
            return _objectContext.CreateObjectSet<T>().Skip(keys, ignoreCount.ToString()).Take(takeCount).ToList();
        }

        /// <summary>
        /// Take a few objects in sequence was sorted by descending on server
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.<</typeparam>
        /// <typeparam name="TKey">Type of object to sort</typeparam>
        /// <param name="ignoreCount">Number of objects will ignore.</param>
        /// <param name="takeCount">Number of objects will take.</param>
        /// <param name="keySelector">The key columns by which to order the results.</param>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetRange<T, TKey>(int ignoreCount, int takeCount, Expression<Func<T, TKey>> keySelector) where T : class
        {
            return _objectContext.CreateObjectSet<T>().OrderByDescending(keySelector).Skip(ignoreCount).Take(takeCount).ToList();
        }

        /// <summary>
        /// Take a few objects in a sequence was sorted on server.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.<</typeparam>
        /// <param name="ignoreCount">Number of objects will ignore.</param>
        /// <param name="takeCount">Number of objects will take.</param>
        /// <param name="keys">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetRange<T>(int ignoreCount, int takeCount, string keys, Expression<Func<T, bool>> expression) where T : class
        {
            return (_objectContext.CreateObjectSet<T>().OrderBy(keys).Where(expression)).Skip(ignoreCount).Take(takeCount).ToList();
        }

        /// <summary>
        /// Take a few objects in sequence was sorted by descending on server
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.<</typeparam>
        /// <typeparam name="TKey">Type of object to sort</typeparam>
        /// <param name="ignoreCount">Number of objects will ignore.</param>
        /// <param name="takeCount">Number of objects will take.</param>
        /// <param name="keySelector">The key columns by which to order the results.</param>
        /// <param name="expression">A function to test each object for a condition.</param>
        /// <returns>The new IList<T> instance.</returns>
        public static IList<T> GetRange<T, TKey>(int ignoreCount, int takeCount, Expression<Func<T, TKey>> keySelector, Expression<Func<T, bool>> expression) where T : class
        {
            return (_objectContext.CreateObjectSet<T>().OrderByDescending(keySelector).Where(expression)).Skip(ignoreCount).Take(takeCount).ToList();
        }

        /// <summary>
        /// Updates an object in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entity">The object to be refreshed.</param>
        public static void Refresh<T>(T entity) where T : class
        {
            _objectContext.Refresh(RefreshMode.StoreWins, entity);
        }

        /// <summary>
        /// Updates a sequence of objects in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence to refresh.</typeparam>
        /// <param name="entityCollection">Object collection to be refreshed.</param>
        public static void Refresh<T>(IEnumerable<T> entityCollection) where T : class
        {
            _objectContext.Refresh(RefreshMode.StoreWins, entityCollection);
        }

        /// <summary>
        /// Updates a sequence of objects in the object context with data from the data source.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence to refresh.</typeparam>
        public static void Refresh<T>() where T : class
        {
            _objectContext.Refresh(RefreshMode.StoreWins, _objectContext.CreateObjectSet<T>());
        }

        /// <summary>
        /// Persists all updates to the data source and resets change tracking in the object context.
        /// </summary>
        public static void Commit()
        {
            int result = _objectContext.SaveChanges();
        }

        /// <summary>
        /// Persists all updates to the data source with the specified System.Data.Objects.SaveOptions.
        /// </summary>
        /// <param name="options">A System.Data.Objects.SaveOptions value that determines the behavior of the operation.</param>
        public static void Commit(SaveOptions options)
        {
            _objectContext.SaveChanges(options);
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        public static void BeginTransaction()
        {
            if (_objectContext.Connection.State == ConnectionState.Closed)
            {
                _objectContext.Connection.Open();
            }

            if (_transaction == null)
            {
                _transaction = _objectContext.Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public static void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
                _objectContext.Connection.Close();
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public static void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
                _objectContext.Connection.Close();
            }
        }

        /// <summary>
        /// Returns an object that has the specified entity key.
        /// </summary>
        /// <typeparam name="T">Type of object in a sequence.</typeparam>
        /// <param name="keyName">A System.String that is the name of the key.</param>
        /// <param name="keyValue">An System.Object that is the key value.</param>
        /// <param name="value">When this method returns, contains the object.</param>
        /// <returns>True if the object was retrieved successfully. false if the key is temporary, the connection is null, or the value is null.</returns>
        public static bool TryGetObjectByKey<T>(string keyName, object keyValue, out object value) where T : class
        {
            EntityKey entityKey = new EntityKey(string.Format("{0}.{1}", _objectContext.DefaultContainerName, typeof(T).Name), keyName, keyValue);
            return _objectContext.TryGetObjectByKey(entityKey, out value);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _objectContext.Connection.Close();
            _objectContext.Connection.Dispose();
            _objectContext.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
