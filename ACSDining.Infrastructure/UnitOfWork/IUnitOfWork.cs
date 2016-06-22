using System;
using System.Data;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    { 
        int SaveChanges();
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}