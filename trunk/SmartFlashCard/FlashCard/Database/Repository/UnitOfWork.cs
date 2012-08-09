namespace FlashCard.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Linq.Expressions;
    using System.Data.SqlClient;
    using System.Data.EntityClient;
    public class UnitOfWork : IDisposable
    {

        //A Static instance of the Linq Data Context
        private static System.Data.Objects.ObjectContext _context;
        //private ObjectSet<T> _entities;
        //private ObjectSet<T> Entities
        //{
        //    get
        //    {
        //        if (_entities == null)
        //            _entities = _context.CreateObjectSet<T>();
        //        return _entities;
        //    }
        //}

        //The default constructor
        public UnitOfWork()
        {
            //if (_service == null)
            //{
            //    _service = new System.Data.Objects.ObjectContext("string");
            //}

            if (_context == null)
                _context = new SmartFlashCardDBEntities(GetConnectionString());
        }

        private string GetConnectionString()
        {
            string connectionString = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            // Specify the provider name, server and database.
            string providerName = "System.Data.SQLite";
            string serverName = connectionString;
            string databaseName = "\\SmartFlashCardDB.s3db";

            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = serverName + databaseName;
            //sqlBuilder.InitialCatalog = databaseName;
            //sqlBuilder.IntegratedSecurity = true;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = @"res://*/Database.SmartFlashCardDB.csdl|res://*/Database.SmartFlashCardDB.ssdl|res://*/Database.SmartFlashCardDB.msl";
            return entityBuilder.ToString();
        }

        //Add a new entity to the model
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


        public IList<T> Include<T>(params string[] tableNames) where T : class
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

        public IQueryable<T> GetQuery<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _context.CreateObjectSet<T>()
                .Where(expression);
        }

        //Get by query
        public T FindBy<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return _context.CreateObjectSet<T>()
                .Where(expression)
                .FirstOrDefault();
        }

        public T GetSingle<T>(Expression<Func<T, bool>> expression) where T : class
        {
            T result = _context.CreateObjectSet<T>()
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