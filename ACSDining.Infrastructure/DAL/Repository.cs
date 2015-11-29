using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using ACSDining.Infrastructure.Identity;

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
            acsContext.SaveChanges();
        }

        public virtual void Update(T entity)
        {
            acsContext.Entry(entity).State = EntityState.Modified;
            acsContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
            acsContext.SaveChanges();
        }

        public virtual T Find(Expression<Func<T, bool>> predicate)
        {
            return DbSet.FirstOrDefault(predicate);
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public virtual T GetById(int id)
        {
            return DbSet.Find(id);
        }


        #endregion


    }
}
