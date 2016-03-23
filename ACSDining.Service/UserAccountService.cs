using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IUserAccountService
    {
        Task<List<AccountDto>> AllAccountsDtoAsync();
        Task<bool> DeleteAccount(int accountId);
        User GetUserById(int userid);
    }

    public class UserAccountService : Service<User>, IUserAccountService
    {
        private readonly IRepositoryAsync<User> _repository;

        public UserAccountService(IRepositoryAsync<User> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public async Task<List<AccountDto>> AllAccountsDtoAsync()
        {
            return await _repository.GetAccountsDto();
        }

        public async Task<bool> DeleteAccount(int accountId)
        {
           return await _repository.DeleteAsync(accountId);
        }

        public User GetUserById(int userid)
        {
            return _repository.Find(userid);
        }
    }
}
