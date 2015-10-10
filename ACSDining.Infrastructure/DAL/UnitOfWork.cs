using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _acsContext;
        private Hashtable _repositories;

        public UnitOfWork()
        {
            _acsContext = new ApplicationDbContext();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType
                            .MakeGenericType(typeof(T)), _acsContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public void SubmitChanges()
        {
            _acsContext.SaveChanges();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _acsContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
