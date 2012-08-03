using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data;
using System.Linq.Expressions;

namespace BlogMVCDemo.Data
{
    public abstract class RepositoryBase<T> : UnitOfWork where T : class
    {
        public RepositoryBase()
        {
        }

        public void Add<T>(T _entity) where T : class
        {
            //var table = _service.GetTable<T>();            
            //_service.InsertOnSubmit(_entity);

            //_service.AddObject(typeof(T).Name, _entity);

            _context.CreateObjectSet<T>().AddObject(_entity);
        }

        //Delete an existing entity from the model
        public void Delete<T>(T _entity) where T : class
        {
            //var table = _service.GetTable<T>();
            //table.DeleteOnSubmit(_entity);

            //object originalItem;
            //EntityKey key = _context.CreateEntityKey(typeof(T).Name, _entity);
            //if (_context.TryGetObjectByKey(key, out originalItem))
            //{
            //    _context.DeleteObject(originalItem);
            //}
            _context.CreateObjectSet<T>().DeleteObject(_entity);
            //this.Entities.DeleteObject(_entity);
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class
        {
            IEnumerable<T> objects = _context.CreateObjectSet<T>().Where(expression);
            foreach (T obj in objects)
                _context.CreateObjectSet<T>().DeleteObject(obj);
        }

        //Update an existing entity
        public void Update<T>(T _entity) where T : class
        {
            _context.CreateObjectSet<T>().ApplyCurrentValues(_entity);
        }

        public void Refresh<T>(T item) where T : class
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, item);
        }

        public void Refresh<T>() where T : class
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, _context.CreateObjectSet<T>());
        }

        //Get the entire Entity table
        public IList<T> GetAll<T>() where T : class
        {
            IList<T> list = _context.CreateObjectSet<T>().ToList();
            return list;
        }

        public IList<T> GetAll<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var query = _context.CreateObjectSet<T>().Where(expression);
            if (query != null && query.Count() > 0)
            {
                IList<T> list = query.ToList();
                return list;
            }
            return default(IList<T>);
        }
        public IList<T> Skip<T>(int count, Func<T, object> orderby) where T : class
        {
            IList<T> list = _context.CreateObjectSet<T>()
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }

        public IList<T> Skip<T>(int count, Func<T, int> orderby) where T : class
        {
            IList<T> list = _context.CreateObjectSet<T>()
                .OrderBy(orderby)
                .Skip(count).Take(100)
                .ToList();
            return list;
        }
    }
}
