using System;
using System.Data;
using ACSDining.Core.Repositories;

namespace ACSDining.Core.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    { 
        //IMenuForWeekService
        int SaveChanges();
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}