using System.Threading;
using System.Threading.Tasks;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;

namespace ACSDining.Core.UnitOfWork
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState;
    }
}