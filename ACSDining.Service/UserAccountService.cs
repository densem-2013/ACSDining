using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Repository.Repositories;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ACSDining.Service
{
    public interface IUserAccountService : IService<User>
    {
        Task<List<AccountDto>> AllAccountsDtoAsync();
        Task<bool> DeleteAccount(int accountId);
        User GetUserById(int userid);
        User GetUserByName(string username);
        bool IsUserInRole(string userid, string role);
        bool CreateUser(User user);

        bool AddUserToRole(string userid, string role);
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

        public User GetUserByName(string username)
        {
            return _repository.Queryable().FirstOrDefault(u => string.Equals(u.UserName, username));
        }

        public bool IsUserInRole(string userid, string role)
        {
            var firstOrDefault =
                _repository.GetRepository<UserRole>().Queryable().FirstOrDefault(ur => string.Equals(ur.Name, role));
            string roleid = null;
            if (firstOrDefault != null)
            {
                roleid = firstOrDefault.Id;
            }
            return _repository.FindAsync(userid).Result.Roles.Select(r => r.RoleId).Contains(roleid);
        }

        public bool CreateUser(User user)
        {
            try
            {
                _repository.Insert(user);
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        public bool AddUserToRole(string userid, string role)
        {
            var firstOrDefault =
                  _repository.GetRepository<UserRole>().Queryable().FirstOrDefault(ur => string.Equals(ur.Name, role)); 
            string roleid = null;
            if (firstOrDefault != null) roleid = firstOrDefault.Id;
            User user = _repository.Find(userid);
            try
            {
                user.Roles.Add(new UserRoleRelation {RoleId = roleid, UserId = user.Id, ObjectState = ObjectState.Added});
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }
    }
}
