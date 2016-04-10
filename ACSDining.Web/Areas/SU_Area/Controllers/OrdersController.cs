using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork)unitOfWorkAsync).GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }


        //Получить все фактические заявки на неделю
        [HttpGet]
        [Route("fact")]
        [Route("fact/{numweek}")]
        [Route("fact/{numweek}/{year}")]
        public async Task<List<UserWeekOrderDto>> GetFactMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var cats = _unitOfWork.RepositoryAsync<DishType>();
            await cats.Queryable().LoadAsync();
            string[] categories = await cats.Queryable()
                  .Select(dt => dt.Category)
                  .AsQueryable()
                  .ToArrayAsync();
            int catLength = categories.Length;

            List<WeekOrderMenu> orderList = _orderMenuService.GetOrderMenuByWeekYear(week, yearnum);

            List<UserWeekOrderDto> userWeekOrderDtos =
                orderList.Select(uwo => UserWeekOrderDto.MapDto(_unitOfWork, uwo, catLength)).ToList();

            return await Task.FromResult(userWeekOrderDtos);
        }

        //Получить все плановые заявки заявки на неделю
        [HttpGet]
        [Route("plan")]
        [Route("plan/{numweek}")]
        [Route("plan/{numweek}/{year}")]
        public async Task<List<PlanUserWeekOrderDto>> GetPlanMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var cats = _unitOfWork.RepositoryAsync<DishType>();
            await cats.Queryable().LoadAsync();
            string[] categories = await cats.Queryable()
                  .Select(dt => dt.Category)
                  .AsQueryable()
                  .ToArrayAsync();
            int catLength = categories.Length;

            List<PlannedWeekOrderMenu> planOrderList =
                _orderMenuService.GetOrderMenuByWeekYear(week, yearnum).Select(uwo => uwo.PlannedWeekOrderMenu).ToList();

            List<PlanUserWeekOrderDto> planUserWeekOrderDtos =
                planOrderList.Select(planuwo => PlanUserWeekOrderDto.MapDto(_unitOfWork, planuwo, catLength)).ToList();

            return await Task.FromResult(planUserWeekOrderDtos);
        }

        //Получить стоимость плановой заявки указанного пользователя за указанную неделю
        [HttpPut]
        [Route("summary/{numweek}/{year}")]
        [ResponseType(typeof (double))]
        public async Task<double> GetSummaryPrice([FromBody] UserOrdersDto usorder, [FromUri] int? numweek = null,
            [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            return await Task.FromResult(_weekMenuService.SummaryPrice(usorder, week, yearnum));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
