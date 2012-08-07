using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data;
using System.Linq.Expressions;
using System.Data.Objects;

namespace BlogMVCDemo.Data
{
    public abstract class RepositoryBase<T> : UnitOfWork where T : class
    {
        #region Ctor
        public RepositoryBase()
        {
        } 
        #endregion
        #region Properties
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

        #endregion

        public void Add(T _entity)
        {
            //var table = _service.GetTable<T>();            
            //_service.InsertOnSubmit(_entity);

            //_service.AddObject(typeof(T).Name, _entity);

            //_context.CreateObjectSet<T>().AddObject(_entity);

            Entities.AddObject(_entity);
        }

        //Delete an existing entity from the model
        public void Delete(T _entity) 
        {
            //var table = _service.GetTable<T>();
            //table.DeleteOnSubmit(_entity);

            object originalItem;
            
            EntityKey key = _context.CreateEntityKey(typeof(T).Name, _entity);
            if (_context.TryGetObjectByKey(key, out originalItem))
            {
                _context.DeleteObject(originalItem);
            }
            //_context.CreateObjectSet<T>().DeleteObject(_entity);
            this.Entities.DeleteObject(_entity);
        }

        public void Delete(Expression<Func<T, bool>> expression)
        {
            //IEnumerable<T> objects = _context.CreateObjectSet<T>().Where(expression);
            //foreach (T obj in objects)
            //    _context.CreateObjectSet<T>().DeleteObject(obj);

            IEnumerable<T> objects = Entities.Where(expression);
            foreach (T obj in objects)
                Entities.DeleteObject(obj);
        }

        //Update an existing entity
        public void Update(T _entity)
        {
            this.Entities.ApplyCurrentValues(_entity);
        }

        public void Refresh(T item) 
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, item);
        }

        public void Refresh() 
        {
            _context.Refresh(System.Data.Objects.RefreshMode.StoreWins, Entities);
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
        public IList<T> Skip(int count, Func<T, object> orderby)
        {
            IList<T> list = this.Entities
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
    }
}
