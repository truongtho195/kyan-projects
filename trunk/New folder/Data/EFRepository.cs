using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;


namespace BlogNet.Data
{
    public partial class EFRepository<T> : IRepository<T> where T : class
    {
        private ObjectContext _context;
        private ObjectSet<T> _entities;
        //DbSet
        private ObjectSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _context.CreateObjectSet<T>();
                return _entities;
            }
        }
        //public BlogNetDBEntities DB { get; set; }

        #region Constructors
        public EFRepository(ObjectContext context)
        {

            this._context = context;
        }

        public EFRepository()
        {
            var db = new BlogNetEntitiesDB();
            this._context = ((IObjectContextAdapter)db).ObjectContext;
        }

        #endregion


        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return this.Entities.Where(predicate);
        }

        public void Insert(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                //this.Entities.Add(entity);
                this.Entities.AddObject(entity);
                this._context.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {

                var msg = string.Empty;

                foreach (var validationErrors in ex.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        msg += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;

                var fail = new Exception(msg, ex);
                //Debug.WriteLine(fail.Message, fail);
                throw fail;
            }
        }

        public void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                this._context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

                var msg = string.Empty;

                foreach (var validationErrors in ex.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        msg += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;

                var fail = new Exception(msg, ex);
                throw fail;
            }
        }

        public void Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                //this.Entities.Remove(entity);
                this.Entities.DeleteObject(entity);

                this._context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                var msg = string.Empty;

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        msg += Environment.NewLine + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);

                var fail = new Exception(msg, dbEx);
                throw fail;
            }
        }


        public IQueryable<T> GetAll
        {
            get
            {
                return this.Entities;
            }
        }
    }
}
