﻿using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser.Accounts;
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

        public AccountManagementApiController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                            .OrderBy(u => u.LastName)
                            .Select(AccountDto.MapDto)
                            .ToList());
        }

        [HttpPut]
        [Route("updateEmail")]
        public async Task<IHttpActionResult> UpdateAccount([FromBody] UpdateAccountEmailDto accountEmailDto)
        {
            User user = await UserManager.FindByIdAsync(accountEmailDto.UserId);
            user.Email = accountEmailDto.Email;
            _unitOfWork.GetContext().Entry(user).State = EntityState.Modified;
            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPut]
        [Route("updateCheckDebt")]
        public async Task<IHttpActionResult> UpdateAccountCheckDebt([FromBody] UpdateCheckDebtDto accountCheckDebtDto)
        {
            User user = await UserManager.FindByIdAsync(accountCheckDebtDto.UserId);
            user.CheckDebt = accountCheckDebtDto.CheckDebt;
            _unitOfWork.GetContext().Entry(user).State = EntityState.Modified;
            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPut]
        [Route("updateExists")]
        public async Task<IHttpActionResult> UpdateAccountExists([FromBody] UpdateExistingDto accountExistsDto)
        {
            User user = await UserManager.FindByIdAsync(accountExistsDto.UserId);
            user.IsExisting = accountExistsDto.IsExisting;
            _unitOfWork.GetContext().Entry(user).State = EntityState.Modified;
            await _unitOfWork.SaveChangesAsync();
            return Ok(true);
        }
        /// <summary>
        /// Получить текущий размер допустимой величины кредита
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("debt")]
        public async Task<IHttpActionResult> GetDebt()
        {
            int debt;
            int.TryParse(WebConfigurationManager.AppSettings["defaultCreditValue"], out debt);
            return Ok(debt);
        }

        [HttpPut]
        [Route("updateDebt")]
        public async Task<IHttpActionResult> UpdateAllowDebt([FromBody] int debt)
        {
            Configuration Config =
               WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            Config.AppSettings.Settings.Remove("defaultCreditValue");
            Config.AppSettings.Settings.Add("defaultCreditValue", debt.ToString());
            Config.Save();

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
