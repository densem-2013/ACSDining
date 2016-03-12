using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ACSDining.Core.DAL
{
    public interface IRepository<T>
    {
        void Insert(T entity);
        void AddRange(IEnumerable<T> list);
        void Update(T entity);
        void Delete(T entity);
        T Find(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetAll();
        T GetById(int id);
        T GetById(string id);
    }

}
