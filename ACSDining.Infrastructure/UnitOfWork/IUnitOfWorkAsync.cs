using System.Threading;
using System.Threading.Tasks;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.UnitOfWork
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        ApplicationDbContext GetContext();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class;
    }
}