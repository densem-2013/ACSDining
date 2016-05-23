using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }

        //Получить все фактические заявки на неделю 
        [HttpPut]
        [Route("fact")]
        [ResponseType(typeof (WeekOrderDto))]
        public async Task<WeekOrderDto> GetFactMenuOrders([FromBody] WeekYearDto wyDto)
        {
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }
            
            WeekOrderDto weekOrderDto = WeekOrderDto.GetMapDto(_unitOfWork, wyDto);

            return await Task.FromResult(weekOrderDto);
        }

        //Получить все плановые заявки на неделю
        [HttpPut]
        [Route("plan")]
        public async Task<List<PlanUserWeekOrderDto>> GetPlanMenuOrders([FromBody] WeekYearDto wyDto)
        {
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            List<WeekOrderMenu> weekOrderMenus = _orderMenuService.GetOrderMenuByWeekYear(wyDto);

            int[] womIds = weekOrderMenus.OrderBy(wom=>wom.User.FirstName).Select(wom => wom.Id).ToArray();

            List<PlannedWeekOrderMenu> planOrderList = _unitOfWork.RepositoryAsync<PlannedWeekOrderMenu>()
                .Query()
               // .Include(pom => pom.WeekOrderMenu)
                //.Include(
                //    pom =>
                //        pom.PlannedDayOrderMenus.Select(pdo => pdo.DayOrderMenu)
                //            .Where(pdo => womIds.Contains(pdo.WeekOrderMenu.Id)))
                .Select().ToList();

            List<PlanUserWeekOrderDto> planUserWeekOrderDtos =
                planOrderList.Select(planuwo => PlanUserWeekOrderDto.MapDto(_unitOfWork, planuwo, catLength)).ToList();

            return await Task.FromResult(planUserWeekOrderDtos);
        }

        //Изменить фактическую заявку указанного списка пользователей на меню соответствующей недели в году
        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateWeekOrder([FromBody] UpdateUserOrderDto userOrderDto)
        {
            if (userOrderDto == null)
            {
                return BadRequest("Bad Request Object");
            }
            
             _unitOfWork.GetContext().UpdateDishQuantity(userOrderDto);

            return Ok(true);
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
