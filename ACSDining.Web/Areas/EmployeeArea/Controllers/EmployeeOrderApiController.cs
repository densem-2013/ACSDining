using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ACSDining.Core.Domains;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Web.Areas.SU_Area.Models;
using Microsoft.AspNet.Identity;
using ACSDining.Web.Areas.EmployeeArea.Models;
using Microsoft.Owin.Security.Provider;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee,Administrator")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        readonly ApplicationDbContext _context=new ApplicationDbContext();

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(EmployeeOrderDTO))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            numweek = numweek ?? _context.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            MenuForWeek mfw =
                _context.MenuForWeek.AsEnumerable()
                    .FirstOrDefault(wm => wm.WeekNumber == numweek && wm.Year.YearNumber == year);
            if (mfw == null) return Content(HttpStatusCode.BadRequest, "not created");


            OrderMenu ordmenu =  _context.OrderMenu.FirstOrDefault( ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == mfw.ID);
            EmployeeOrderDTO model = new EmployeeOrderDTO
            {
                UserId = userid,
                MenuId = mfw.ID,
                SummaryPrice = ordmenu.SummaryPrice,
                WeekIsPaid = ordmenu.CurrentWeekIsPaid
            };
            if (ordmenu != null)
            {
                model.OrderId = ordmenu.Id;
                model.Dishquantities = _context.GetUserWeekOrderDishes(ordmenu.Id);
            }
            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(_context.CurrentWeek());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks =  _context.MenuForWeek.AsEnumerable().Select(wm => wm.WeekNumber).Reverse().ToList();
            if (numweeks == null)
            {
                 return NotFound();
            }

            return Ok(numweeks);
        }
    }
}
