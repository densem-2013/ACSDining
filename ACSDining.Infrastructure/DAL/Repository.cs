using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace ACSDining.Infrastructure.DAL
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext acsContext;
        protected DbSet<T> DbSet;
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public Repository(ApplicationDbContext acsContext)
        {
            this.acsContext = acsContext;
            this.DbSet = acsContext.Set<T>();
        }


        #region IRepository<T> Members

        public virtual void Insert(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            acsContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public virtual T GetById(string id)
        {
            return DbSet.Find(id);
        }


        #endregion


    }
}
