using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Service;
using Microsoft.Ajax.Utilities;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IOrderMenuService _orderMenuService;
        private readonly IDishQuantityRelationsService _dishQuantityService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _dishQuantityService = new DishQuantityRelationsService(_unitOfWork.RepositoryAsync<DishQuantityRelations>());
        }

        //Получить все фактические заявки на неделю
        [HttpGet]
        [Route("fact")]
        [Route("fact/{numweek}")]
        [Route("fact/{numweek}/{year}")]
        public async Task<List<UserWeekOrderDto>> GetFactMenuOrders([FromUri] int? numweek = null,
            [FromUri] int? year = null)
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

        //Получить все плановые заявки на неделю
        [HttpGet]
        [Route("plan")]
        [Route("plan/{numweek}")]
        [Route("plan/{numweek}/{year}")]
        public async Task<List<PlanUserWeekOrderDto>> GetPlanMenuOrders([FromUri] int? numweek = null,
            [FromUri] int? year = null)
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

        //Изменить фактическую заявку указанного пользователя на меню соответствующей недели в году
        [HttpPut]
        [Route("update")]
        [ResponseType(typeof (int))]
        public async Task<IHttpActionResult> UpdateUserWeekOrder([FromBody] UserWeekOrderDto userWeekOrderDto)
        {
            if (userWeekOrderDto == null)
            {
                return BadRequest("Bad Request Object");
            }
            WeekOrderMenu forUpdateOrder = _orderMenuService.Find(userWeekOrderDto.UserId);
            if (forUpdateOrder == null)
            {
                return NotFound();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            forUpdateOrder.DayOrderMenus.ForEach(x =>
            {
                foreach (UserDayOrderDto udoDto in userWeekOrderDto.DayOrderDtos.ToList())
                {
                    if (udoDto.DayOrderId == x.Id)
                    {
                        if (x.OrderCanBeChanged)
                        {
                            List<DishQuantityRelations> dqaList =
                                _dishQuantityService.GetByDayOrderMenuForDay(udoDto.DayOrderId, udoDto.MenuForDayId);

                            for (int j = 1; j <= catLength; j++)
                            {
                                //Находим связь, указывающую на текущее значение фактической дневной заявки на блюдо
                                var firstOrDefault = dqaList.FirstOrDefault(
                                    q => q.DayOrderMenuId == x.Id && q.DishTypeId == j);
                                if (firstOrDefault != null)
                                {
                                    double curQuantity = firstOrDefault.DishQuantity.Quantity;
                                    //если заказанное количество изменилось
                                    if (Math.Abs(curQuantity - udoDto.DishQuantities[j - 1]) > 0.001)
                                    {
                                        var dishQuantityRelations = _dishQuantityService.Query()
                                            .Include(dq => dq.DishQuantity)
                                            .Include(dq => dq.MenuForDay)
                                            .Include(dq => dq.DayOrderMenu)
                                            .Select()
                                            .FirstOrDefault(
                                                dq =>
                                                    Math.Abs(dq.DishQuantity.Quantity - udoDto.DishQuantities[j - 1]) <
                                                    0.001);
                                        if (dishQuantityRelations != null)
                                        {
                                            //переустанавливаем связь на найденную сущность, содержащую необходимое количество
                                            firstOrDefault.DishQuantity = dishQuantityRelations.DishQuantity;
                                            _dishQuantityService.Update(firstOrDefault);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            int res = await _unitOfWork.SaveChangesAsync();

            return Ok(res);
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
