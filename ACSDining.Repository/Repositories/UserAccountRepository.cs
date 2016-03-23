using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Repository.Repositories
{
    public static class UserAccountRepository
    {
        public static async Task<List<AccountDto>> GetAccountsDto(this IRepositoryAsync<User> repository)
        {
            return await Task.FromResult(repository.Queryable().Select(AccountDto.MapDto).ToList());
        }

    }
}
