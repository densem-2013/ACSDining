using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;
using Microsoft.AspNet.Identity;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee,Administrator")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;

        public EmployeeOrderApiController(IMenuForWeekService weekMenuService, IOrderMenuService orderMenuService)
        {
            _weekMenuService = weekMenuService;
            _orderMenuService = orderMenuService;
        }

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(EmployeeOrderDto))]
        public async Task<IHttpActionResult> GetEmployeeOrderDto([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            int week = (int)(numweek ?? UnitOfWork.CurrentWeek());
            int yearnumber = (int)(year ?? DateTime.Now.Year);
           
            WeekMenuDto weekmodel = _weekMenuService.WeekMenuDtoByWeekYear(week, yearnumber);

            if (weekmodel == null) return Content(HttpStatusCode.BadRequest, "not created");

            EmployeeOrderDto model = _orderMenuService.EmployeeOrderByWeekYear(weekmodel, userid, week, yearnumber);
            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(UnitOfWork.CurrentWeek());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks = _weekMenuService.WeekNumbers();

            return Ok(numweeks);
        }
    }
}
