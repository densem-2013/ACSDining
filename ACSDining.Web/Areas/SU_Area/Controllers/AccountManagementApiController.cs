using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Account")]
    public class AccountManagementApiController : ApiController
    {
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IUserAccountService _userAccountService;

        public AccountManagementApiController(IUnitOfWorkAsync unitOfWorkAsync, IUserAccountService userAccountService)
        {
            //IRepositoryAsync<User> userRepo = unitOfWorkAsync.RepositoryAsync<User>();
            //_userAccountService = userAccountService;
            _unitOfWorkAsync = unitOfWorkAsync;
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
        public async Task<IHttpActionResult> DeleteAccount(int id)
        {
            User user = _userAccountService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            user.ObjectState = ObjectState.Deleted;

             _userAccountService.Delete(user);
            await _unitOfWorkAsync.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
