using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.Employee;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
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
        private readonly IUnitOfWorkAsync _unitOfWork;

        public EmployeeOrderApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<OrderMenu>());
        }

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(EmployeeOrderDto))]
        public async Task<IHttpActionResult> GetEmployeeOrderDto([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnumber = year ?? DateTime.Now.Year;

            WeekMenuDto weekmodel = WeekMenuDto.MapDto(_unitOfWork as UnitOfWork,
                _weekMenuService.GetWeekMenuByWeekYear(week, yearnumber));

            if (weekmodel == null) return Content(HttpStatusCode.BadRequest, "not created");

            OrderMenu ordmenu =
                _orderMenuService.GetAllByWeekYear(week, yearnumber)
                    .FirstOrDefault(ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == weekmodel.ID);

            List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                    .Queryable()
                    .Where(dqr => dqr.OrderMenuID == ordmenu.Id && dqr.MenuForWeekID == weekmodel.ID)
                    .ToList();

            string[] categories =
                    _unitOfWork.Repository<DishType>()
                        .Queryable()
                        .ToList()
                        .OrderBy(d => d.Id)
                        .Select(dt => dt.Category)
                        .AsQueryable()
                        .ToArray();
            EmployeeOrderDto model = null;
            if (ordmenu != null)
            {
                MenuForWeek mfw = ordmenu.MenuForWeek;
                model = new EmployeeOrderDto
                {
                    UserId = userid,
                    MenuId = weekmodel.ID,
                    SummaryPrice = ordmenu.SummaryPrice,
                    WeekPaid = ordmenu.WeekPaid,
                    MfdModels = weekmodel.MFD_models,
                    Year = yearnumber,
                    WeekNumber = week,
                    OrderId = ordmenu.Id,
                    Dishquantities = _orderMenuService.UserWeekOrderDishes(quaList, categories, mfw)
                };

            }
            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(YearWeekHelp.CurrentWeek());
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
