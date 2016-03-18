using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ACSDining.Core.DAL;
using ACSDining.Infrastructure.Identity;
using NLog;

namespace ACSDining.Infrastructure.DAL
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext AcsContext;
        protected DbSet<T> DbSet;
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public Repository(ApplicationDbContext acsContext)
        {
            AcsContext = acsContext;
            DbSet = AcsContext.Set<T>();
        }


        #region IRepository<T> Members

        public virtual async void Insert(T entity)
        {
            DbSet.Add(entity);
            await AcsContext.SaveChangesAsync();
        }

        public virtual void AddRange(IEnumerable<T> list)
        {
            var collection = AcsContext.Set(typeof(T)).Local;
            foreach (T entity in list)
            {
                collection.Add(entity);
            }
            AcsContext.SaveChanges();
        }
        public virtual void Update(T entity)
        {
            AcsContext.Entry(entity).State = EntityState.Modified;
            AcsContext.SaveChanges();
        }

        public async void Delete(T entity)
        {
            DbSet.Remove(entity);
            await AcsContext.SaveChangesAsync();
        }

        public virtual async Task<T> Find(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.FirstOrDefaultAsync<T>(predicate);
        }

        public virtual async Task<List<T>> GetAll()
        {
            try
            {
                return await DbSet.ToListAsync<T>();
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        public virtual T GetById(int id)
        {
            return DbSet.Find(id);
        }
        public virtual T GetById(string id)
        {
            return DbSet.Find(id);
        }


        #endregion

  

    }
}
