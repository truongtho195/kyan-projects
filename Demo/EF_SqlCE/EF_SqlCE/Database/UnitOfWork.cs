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
        //    if (_context == null)
        //        _context = new ObjectContext(@"data source=F:\Workplace\WPF-WCF\SourceProject\kyan-projects\DemoRalcon\DemoRalcon\bin\Debug\FalconHRDB.s3db");
        }

        public void Add(T entity)
        {
            this.Entities.AddObject(entity);
            _context.SaveChanges();
        }

        public T Get()
        {
            return this.Entities.FirstOrDefault();
        }
    }
}
