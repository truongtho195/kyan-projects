using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

namespace EF_SqlCE.Database
{
    public class UnitOfWork<T> where T: class
    {
        private ObjectContext _context;
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
        public UnitOfWork()
        {
            if (_context == null)
                _context = new EF_SqlCE.Database.BlogEngineDBEntities();
        }

        public void Add(T entity)
        {
            this.Entities.AddObject(entity);
        }

        public T Get()
        {
            return this.Entities.FirstOrDefault();
        }
    }
}
