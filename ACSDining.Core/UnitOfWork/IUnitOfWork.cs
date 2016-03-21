using System;
using System.Data;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;

namespace ACSDining.Core.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, IObjectState;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
        //double[] GetUserWeekOrderDishes(int orderid);
        //double[] GetUserWeekOrderPaiments(int orderid);
        //double[] GetUnitWeekPrices(int menuforweekid);
    }
}