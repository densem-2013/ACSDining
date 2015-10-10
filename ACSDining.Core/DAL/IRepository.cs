using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ACSDining.Core.DAL
{
    public interface IRepository<T>
    {
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll();
        T GetById(string id);
    }

}
