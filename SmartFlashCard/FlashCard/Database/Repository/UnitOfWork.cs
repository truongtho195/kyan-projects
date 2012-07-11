using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Data.Objects;

namespace FlashCard.Database
{
    public class UnitOfWork<T> : IDisposable where T : class
    {

        //A Static instance of the Linq Data Context
        private static System.Data.Objects.ObjectContext _context;
        private ObjectSet<T> _entities;
        private ObjectSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _context.CreateObjectSet<T>();
                return _entities;
            }
        }

        //The default constructor
        public UnitOfWork()
        {
            //if (_service == null)
            //{
            //    _service = new System.Data.Objects.ObjectContext("string");
            //}

            if (_context == null)
                _context = new SmartFlashCardDBEntities();
        }

        //Add a new entity to the model
        public void Add(T _entity)
        {
            //var table = _service.GetTable<T>();            
            //_service.InsertOnSubmit(_entity);

            //_service.AddObject(typeof(T).Name, _entity);

            this.Entities.AddObject(_entity);
        }

        //Delete an existing entity from the model
        public void Delete(T _entity)
        {
            //var table = _service.GetTable<T>();
            //table.DeleteOnSubmit(_entity);

            //object originalItem;
            //EntityKey key = _service.CreateEntityKey(typeof(T).Name, _entity);
            //if (_service.TryGetObjectByKey(key, out originalItem))
            //{
            //    _service.DeleteObject(originalItem);
            //}

            this.Entities.DeleteObject(_entity);
        }

        //Update an existing entity
        public void Update(T _entity)
        {
            //var table = _service.GetTable<T>();
            //table.Attach(_entity, true);

            //object originalItem;
            //EntityKey key = _service.CreateEntityKey(typeof(T).Name, _entity);
            //if (_service.TryGetObjectByKey(key, out originalItem))
            //{
            //    _service.ApplyCurrentValues(typeof(T).Name, _entity);
            //}

            this.Entities.ApplyCurrentValues(_entity);
        }

        public void Refresh(T item)
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, item);
        }

        public void Refresh()
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, this.Entities);
        }

        //Get the entire Entity table
        public IList<T> GetAll()
        {
            IList<T> list = this.Entities.ToList();
            return list;
        }

        public IList<T> GetAll(Expression<Func<T, bool>> expression)
        {
            var query = this.Entities.Where(expression);
            if (query != null && query.Count() > 0)
            {
                IList<T> list = query.ToList();
                return list;
            }
            return default(IList<T>);
        }

        public IList<T> Include(params string[] tableNames)
        {
            var query = _context
                .CreateQuery<T>(
                "[" + typeof(T).Name + "]");
            Array.ForEach(tableNames, new Action<string>(delegate(string item)
                {
                    query = query.Include(item);
                }));
            IList<T> list = query.ToList();
            return list;
        }

        public IList<T> Skip(int count, Func<T, object> orderby)
        {
            IList<T> list =this.Entities
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }

        public IList<T> Skip(int count, Func<T, int> orderby)
        {
            IList<T> list = this.Entities
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }

        public IQueryable<T> GetQuery(Expression<Func<T, bool>> expression)
        {
            return this.Entities
                .Where(expression);
        }

        //Get by query
        public T FindBy(Expression<Func<T, bool>> expression)
        {
            return this.Entities
                .Where(expression)
                .FirstOrDefault();
        }

        public T GetSingle(Expression<Func<T, bool>> expression)
        {
            T result = this.Entities
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
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException)
            {
                _context.SaveChanges();
            }
        }

        public void StartLazyLoading()
        {
            _context.ContextOptions.LazyLoadingEnabled = true;
        }

        public void StopLazyLoading()
        {
            _context.ContextOptions.LazyLoadingEnabled = false;
        }

        public bool IsDirty
        {
            get
            {
                if (_context.ObjectStateManager.GetObjectStateEntries
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
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}