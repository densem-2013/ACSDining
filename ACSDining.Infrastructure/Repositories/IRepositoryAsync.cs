using System.Threading;
using System.Threading.Tasks;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Infrastructure.Repositories
{
    public interface IRepositoryAsync<TEntity> : IRepository<TEntity> where TEntity : class
    {
        ApplicationDbContext Context { get; }
        Task<TEntity> FindAsync(params object[] keyValues);
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<bool> DeleteAsync(params object[] keyValues);
        Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);
        IRepositoryAsync<T> GetRepositoryAsync<T>() where T : class; 
    }
}