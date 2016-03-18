using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Account")]
    public class AccountManagementApiController : ApiController
    {
        private IUnitOfWork unitOfWork;
        private readonly IRepository<User> _userRepository;

        public AccountManagementApiController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            _userRepository = this.unitOfWork.Repository<User>();
        }

        [HttpGet]
        [Route("All")]
        [ResponseType(typeof (List<AccountDto>))]
        public async Task<List<AccountDto>> GetAccounts()
        {
            return _userRepository.GetAll().Result.Select(AccountDto.MapDto).ToList();
        }

        // DELETE api/Dishes/5
        [HttpDelete]
        [Route("delete/{id}")]
        [ResponseType(typeof(Dish))]
        public async Task<IHttpActionResult> DeleteAccount(int id)
        {
            User user = _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            _userRepository.Delete(user);

            return Ok();
        }
    }
}
