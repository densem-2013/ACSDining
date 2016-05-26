using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.UnitOfWork;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Account")]
    public class AccountManagementApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        private readonly ApplicationRoleManager _roleManager;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public AccountManagementApiController(/*ApplicationUserManager userManager*/IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            //_userManager = userManager;
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _roleManager = new ApplicationRoleManager(new RoleStore<UserRole>(_unitOfWork.GetContext()));
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        [HttpGet]
        [Route("All")]
        [ResponseType(typeof (List<AccountDto>))]
        public async Task<List<AccountDto>> GetAccounts()
        {
            UserRole employrole = await _roleManager.FindByNameAsync("Employee");
            return
                await
                    Task.FromResult(
                        UserManager.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(employrole.Id))
                            .OrderBy(u => u.UserName)
                            .Select(AccountDto.MapDto)
                            .ToList());
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateAccount([FromBody]UpdateAccountDto accountDto)
        {
            User user = await UserManager.FindByIdAsync(accountDto.UserId);
            user.Email = accountDto.Email;
            user.CanMakeBooking = accountDto.CanMakeBooking;
            user.IsExisting = accountDto.IsExisting;
            _unitOfWork.GetContext().Entry(user).State=EntityState.Modified;
            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
        }
            // DELETE api/Dishes/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IHttpActionResult> DeleteAccount(string id)
        {
            User user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

           var res= UserManager.Delete(user);

            return res.Succeeded ? StatusCode(HttpStatusCode.NoContent) : StatusCode(HttpStatusCode.BadRequest);
        }

    }
}
