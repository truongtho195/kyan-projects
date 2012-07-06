using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;

namespace Tims.Database
{
    internal sealed class UnitOfWork : IUnitOfWork, IDisposable
    {

        //A Static instance of the Linq Data Context
        private static System.Data.Objects.ObjectContext _service;

        //The default constructor
        public UnitOfWork()
        {
            if (_service == null)
            {
                _service = new System.Data.Objects.ObjectContext(App.EntityConnString);
            }
        }

        //Add a new entity to the model
        public void Add<T>(T _entity) where T : class
        {
            //var table = _service.GetTable<T>();            
            //_service.InsertOnSubmit(_entity);

            //_service.AddObject(typeof(T).Name, _entity);

            _service.CreateObjectSet<T>().AddObject(_entity);
        }

        //Delete an existing entity from the model
        public void Delete<T>(T _entity) where T : class
        {
            //var table = _service.GetTable<T>();
            //table.DeleteOnSubmit(_entity);

            //object originalItem;
            //EntityKey key = _service.CreateEntityKey(typeof(T).Name, _entity);
            //if (_service.TryGetObjectByKey(key, out originalItem))
            //{
            //    _service.DeleteObject(originalItem);
            //}

            _service.CreateObjectSet<T>().DeleteObject(_entity);
        }

        //Update an existing entity
        public void Update<T>(T _entity) where T : class
        {
            //var table = _service.GetTable<T>();
            //table.Attach(_entity, true);

            //object originalItem;
            //EntityKey key = _service.CreateEntityKey(typeof(T).Name, _entity);
            //if (_service.TryGetObjectByKey(key, out originalItem))
            //{
            //    _service.ApplyCurrentValues(typeof(T).Name, _entity);
            //}

            _service.CreateObjectSet<T>().ApplyCurrentValues(_entity);
        }

        public void Refresh<T>(T item) where T : class
        {
            _service.Refresh(System.Data.Objects.RefreshMode.StoreWins, item);
        }

        public void Refresh<T>() where T : class
        {
            _service.Refresh(System.Data.Objects.RefreshMode.StoreWins, _service
                .CreateObjectSet<T>());
        }

        //Get the entire Entity table
        public IList<T> GetAll<T>() where T : class
        {
            IList<T> list = _service
                .CreateObjectSet<T>()
                .ToList();
            return list;
        }

        public IList<T> GetAll<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var query = _service
                .CreateObjectSet<T>()
                .Where(expression);
            if (query != null && query.Count() > 0)
            {
                IList<T> list = query.ToList();
                return list;
            }
            return default(IList<T>);
        }

        public IList<T> Include<T>(params string[] tableNames) where T : class
        {
            var query = _service
                .CreateQuery<T>(
                "[" + typeof(T).Name + "]");
            Array.ForEach(tableNames, new Action<string>(delegate(string item)
                {
                    query = query.Include(item);
                }));
            IList<T> list = query.ToList();
            return list;
        }

        public IList<T> Skip<T>(int count, Func<T, object> orderby) where T : class
        {
            IList<T> list = _service
                .CreateObjectSet<T>()
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }

        public IList<T> Skip<T>(int count, Func<T, int> orderby) where T : class
        {
            IList<T> list = _service
                .CreateObjectSet<T>()
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }

        public IQueryable<T> GetQuery<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _service
                .CreateObjectSet<T>()
                .Where(expression);
        }

        //Get by query
        public T FindBy<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _service
                .CreateObjectSet<T>()
                .Where(expression)
                .FirstOrDefault();
        }

        public T GetSingle<T>(Expression<Func<T, bool>> expression) where T : class
        {
            T result = _service
                .CreateObjectSet<T>()
                .Where(expression)
                .FirstOrDefault();

            return result;
        }

        //Get the first occurence that reflect the Linq Query
        //public T GetById<T>(Func<T, bool> _condition) where T : class
        //{
        //    return _service.GetTable<T>().Where(_condition).FirstOrDefault();
        //}

        //Commit all the pending changes in the data context
        public void Commit()
        {
            try
            {
                _service.SaveChanges();
            }
            catch (OptimisticConcurrencyException)
            {
                _service.SaveChanges();
            }
        }

        public void StartLazyLoading()
        {
            _service.ContextOptions.LazyLoadingEnabled = true;
        }

        public void StopLazyLoading()
        {
            _service.ContextOptions.LazyLoadingEnabled = false;
        }

        public bool IsDirty
        {
            get
            {
                if (_service.ObjectStateManager.GetObjectStateEntries
                  (EntityState.Added | EntityState.Deleted | EntityState.Modified)
                  .Count() > 0)
                    return true;
                else
                    return false;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _service.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}