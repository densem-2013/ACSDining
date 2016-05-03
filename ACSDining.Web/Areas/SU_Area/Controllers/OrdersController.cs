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

        //Получить все фактические заявки на неделю постранично
        [HttpPut]
        [Route("fact/{pagesize}/{page}")]
        [ResponseType(typeof (WeekOrderDto))]
        public async Task<WeekOrderDto> GetFactMenuOrders([FromBody] WeekYearDto wyDto, [FromUri] int? pagesize,
            [FromUri] int? page)
        {
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            WeekOrderDto weekOrderDto = WeekOrderDto.GetMapDto(_unitOfWork, wyDto, catLength, pagesize, page);

            return await Task.FromResult(weekOrderDto);
        }

        //Получить все плановые заявки на неделю
        [HttpPut]
        [Route("plan/{pagesize}/{page}")]
        public async Task<List<PlanUserWeekOrderDto>> GetPlanMenuOrders([FromBody] WeekYearDto wyDto,[FromUri] int? pagesize,
            [FromUri] int? page)
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
                .Include(pom => pom.WeekOrderMenu)
                .Include(
                    pom =>
                        pom.PlannedDayOrderMenus.Select(pdo => pdo.DayOrderMenu)
                            .Where(pdo => womIds.Contains(pdo.WeekOrderMenu.Id)))
                .Select().ToList();

            List<PlanUserWeekOrderDto> planUserWeekOrderDtos =
                planOrderList.Select(planuwo => PlanUserWeekOrderDto.MapDto(_unitOfWork, planuwo, catLength)).ToList();

            return await Task.FromResult(planUserWeekOrderDtos);
        }

        //Изменить фактическую заявку указанного списка пользователей на меню соответствующей недели в году
        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateWeekOrder([FromBody] UserWeekOrderDto userWeekOrderDto)
        {
            if (userWeekOrderDto == null)
            {
                return BadRequest("Bad Request Object");
            }

            WeekOrderMenu forUpdateOrder = _orderMenuService.Find(userWeekOrderDto.OrderId);


            if (forUpdateOrder == null)
            {
                return NotFound();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            int res = await Task.FromResult(_orderMenuService.UpdateUserWeekOrder(catLength, userWeekOrderDto));

            return Ok(res);
        }

        /// <summary>
        /// Получить суммарные количества звказанных блюд за неделю
        /// </summary>
        /// <param name="wyDto"></param>
        [HttpPut]
        [Route("weeksummaryorderdishes")]
        public async Task<double[]> GetWeekSummaryOrders([FromBody] WeekYearDto wyDto)
        {
            if (wyDto.Week == 0 || wyDto.Year == 0) return null;
            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);
            return await Task.FromResult(_orderMenuService.SummaryWeekDishesOrderQuantities(wyDto, catLength));
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
