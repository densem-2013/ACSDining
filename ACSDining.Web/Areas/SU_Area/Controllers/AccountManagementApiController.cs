using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Account")]
    public class AccountManagementApiController : ApiController
    {

        private readonly IUserAccountService _userAccountService;

        public AccountManagementApiController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }
        [HttpGet]
        [Route("All")]
        [ResponseType(typeof (List<AccountDto>))]
        public async Task<List<AccountDto>> GetAccounts()
        {
            return await _userAccountService.AllAccountsDtoAsync();
        }

        // DELETE api/Dishes/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<bool> DeleteAccount(int id)
        {
            User user = _userAccountService.GetUserById(id);
            if (user == null)
            {
                return await Task.FromResult(false);
            }

            return await _userAccountService.DeleteAccount(id);
        }
    }
}
