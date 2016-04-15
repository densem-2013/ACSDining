using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;
using LinqKit;

namespace ACSDining.Infrastructure.DAL
{
    public class Repository<TEntity> : IRepositoryAsync<TEntity> where TEntity : class
    {
        #region Private Fields

        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IUnitOfWorkAsync _unitOfWork;

        #endregion Private Fields

        public Repository(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = ((UnitOfWork)_unitOfWork).GetContext();
            //var dbContext = _context ;

            if (_unitOfWork != null)
            {
                _dbSet = ((UnitOfWork)_unitOfWork).GetContext().Set<TEntity>();
            }
        }
        
        public virtual IRepositoryAsync<T> GetRepositoryAsync<T>() where T : class
        {
            return _unitOfWork.RepositoryAsync<T>();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return _dbSet.SqlQuery(query, parameters).AsQueryable();
        }

        public virtual void Insert(TEntity entity)
        {
            try
            {   
                _context.Entry(entity).State=EntityState.Added;
                _dbSet.Attach(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
        }

        public virtual void InsertGraphRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _dbSet.Attach(entity);
            _unitOfWork.SaveChanges();
        }

        public virtual void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
            _dbSet.Attach(entity);
            _unitOfWork.SaveChanges();
        }

        public IQueryFluent<TEntity> Query()
        {
            return new QueryFluent<TEntity>(this);
        }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject)
        {
            return new QueryFluent<TEntity>(this, queryObject);
        }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return new QueryFluent<TEntity>(this, query);
        }

        public IQueryable<TEntity> Queryable()
        {
            return _dbSet;
        }

        public List<TEntity> GetAll()
        {
            return Queryable().ToList();
        }

        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _dbSet.FindAsync(cancellationToken, keyValues);
        }

        public virtual async Task<bool> DeleteAsync(params object[] keyValues)
        {
            return await DeleteAsync(CancellationToken.None, keyValues);
        }

        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            var entity = await FindAsync(cancellationToken, keyValues);

            if (entity == null)
            {
                return false;
            }

            _context.Entry(entity).State = EntityState.Deleted;
            _dbSet.Attach(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }
            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        internal async Task<IEnumerable<TEntity>> SelectAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            return await Select(filter, orderBy, includes, page, pageSize).ToListAsync();
        }

        public virtual void InsertOrUpdateGraph(TEntity entity)
        {
            _entitesChecked = null;
            _dbSet.Attach(entity);
        }

        HashSet<object> _entitesChecked; // tracking of all process entities in the object graph when calling SyncObjectGraph

    }
}
