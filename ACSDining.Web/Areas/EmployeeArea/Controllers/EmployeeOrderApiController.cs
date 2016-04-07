using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Web;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.Employee;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee,Administrator")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private ApplicationUserManager _userManager;

        public EmployeeOrderApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<OrderMenu>());
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                _userManager = _userManager ??
                               HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return _userManager;
            }
        }

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (EmployeeOrderDto))]
        public async Task<IHttpActionResult> GetEmployeeOrderDto([FromUri] int? numweek = null,
            [FromUri] int? year = null)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnumber = year ?? DateTime.Now.Year;

            WeekMenuDto weekmodel = WeekMenuDto.MapDto(_unitOfWork as UnitOfWork,
                _weekMenuService.GetWeekMenuByWeekYear(week, yearnumber));

            //Меню на запрашиваемую неделю не было создано
            if (weekmodel == null)
            {
                return Content(HttpStatusCode.BadRequest,
                    string.Format(" menu on week {0} year {1} not created", week, yearnumber));
            }

            var cats = _unitOfWork.RepositoryAsync<DishType>();
            await cats.Queryable().LoadAsync();
            string[] categories = await cats.Queryable()
                .Select(dt => dt.Category)
                .AsQueryable()
                .ToArrayAsync();

            EmployeeOrderDto model = null;

            OrderMenu ordmenu = null;
            ordmenu =
                _orderMenuService.GetAllByWeekYear(week, yearnumber)
                    .FirstOrDefault(ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == weekmodel.ID);

            if (ordmenu != null)
            {
                List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                    .Query().Include(dq => dq.DishQuantity).Select()
                    .Where(
                        dqr => ordmenu != null && (dqr.OrderMenuID == ordmenu.Id && dqr.MenuForWeekID == weekmodel.ID))
                    .ToList();

                
                MenuForWeek mfw = ordmenu.MenuForWeek;
                model = new EmployeeOrderDto
                {
                    UserId = userid,
                    MenuId = weekmodel.ID,
                    SummaryPrice = ordmenu.OrderSummaryPrice,
                    WeekPaid = ordmenu.WeekPaid,
                    MfdModels = weekmodel.MFD_models,
                    Year = yearnumber,
                    WeekNumber = week,
                    OrderId = ordmenu.Id,
                    Dishquantities = _orderMenuService.UserWeekOrderDishes(quaList, categories, mfw)
                };

            }
            else
            {
                User user =
                    await UserManager.FindByNameAsync(ControllerContext.RequestContext.Principal.Identity.GetUserName());
                MenuForWeek weekmenu = _weekMenuService.GetWeekMenuByWeekYear(week, yearnumber);
                PlannedOrderMenu planmenu = new PlannedOrderMenu
                {
                    User = user,
                    MenuForWeek = weekmenu
                };
                ordmenu = new OrderMenu
                {
                    User = user,
                    MenuForWeek = weekmenu,
                    PlannedOrderMenu = planmenu
                };
                _orderMenuService.Insert(ordmenu);
                await _unitOfWork.SaveChangesAsync();

                ordmenu = _orderMenuService.FindByUserIdWeekYear(user.Id, week, yearnumber);

                model = new EmployeeOrderDto
                {
                    UserId = userid,
                    MenuId = weekmodel.ID,
                    SummaryPrice = ordmenu.OrderSummaryPrice,
                    WeekPaid = ordmenu.WeekPaid,
                    MfdModels = weekmodel.MFD_models,
                    Year = yearnumber,
                    WeekNumber = week,
                    OrderId = ordmenu.Id,
                    Dishquantities = new double[20]
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
