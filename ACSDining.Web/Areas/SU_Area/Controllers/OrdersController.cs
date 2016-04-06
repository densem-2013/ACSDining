using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<OrderMenu>());
        }


        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        public async Task<OrdersDTO> GetMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;
            string[] categories =
                    _unitOfWork.Repository<DishType>()
                        .Queryable()
                        .ToList()
                        .OrderBy(d => d.Id)
                        .Select(dt => dt.Category)
                        .AsQueryable()
                        .ToArray();

            List<OrderMenu> orderList = _orderMenuService.GetOrderMenuByWeekYear(week, yearnum);
            OrdersDTO OrderDTO = new OrdersDTO
            {
                WeekNumber = week,
                YearNumber = yearnum,
                UserOrders = orderList.Select(om =>
                {
                    List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                            .Queryable()
                            .Where(dqr => dqr.OrderMenuID == om.Id && dqr.MenuForWeekID == om.MenuForWeek.ID)
                            .ToList();
                    MenuForWeek mfw = _weekMenuService.Find(om.MenuForWeek.ID);
                    return new UserOrdersDTO
                    {
                        UserId = om.User.Id,
                        UserName = om.User.UserName,
                        Dishquantities = _orderMenuService.UserWeekOrderDishes(quaList,categories,mfw)
                    };
                }).ToList()
            };


            return await Task.FromResult(OrderDTO);
        }

        [HttpPut]
        [Route("summary/{numweek}/{year}")]
        [ResponseType(typeof (double))]
        public async Task<double> GetSummaryPrice([FromBody] UserOrdersDTO usorder, [FromUri] int? numweek = null,
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
