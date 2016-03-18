using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ACSDining.Core.DAL
{
    public interface IRepository<T>
    {
        void Insert(T entity);
        void AddRange(IEnumerable<T> list);
        void Update(T entity);
        void Delete(T entity);
        Task<T> Find(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAll();
        T GetById(int id);
        T GetById(string id);
    }

}
