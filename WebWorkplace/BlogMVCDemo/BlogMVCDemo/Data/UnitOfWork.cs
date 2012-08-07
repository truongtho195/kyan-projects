using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlogMVCDemo.Database;
using System.Data;
using System.Data.Objects;

namespace BlogMVCDemo.Data
{
    public class UnitOfWork : IDisposable
    {
        //A Static instance of the Linq Data Context
        protected static System.Data.Objects.ObjectContext _context;
        
        public UnitOfWork()
        {
            if (_context == null)
                _context = new BlogMvcEntities();
        }
        public UnitOfWork(string connectionString)
        {
            if (_context == null)
                _context = new BlogMvcEntities(connectionString);
        }

        public void StartLazyLoading()
        {
            _context.ContextOptions.LazyLoadingEnabled = true;
        }

        public void StopLazyLoading()
        {
            _context.ContextOptions.LazyLoadingEnabled = false;
        }

        /// <summary>
        /// Get ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return _context.Connection.ConnectionString; }
        }

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
        
        private void Disposing()
        {
            if (_context != null)
                _context.Dispose();
            DisposeCore();
        }

        public void Dispose()
        {
            Disposing();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeCore() { }
        
    }
}