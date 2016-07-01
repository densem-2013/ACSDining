using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO.SuperUser.Orders;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
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
            
            WeekOrderDto weekOrderDto = WeekOrderDto.GetMapDto(_unitOfWork, wyDto );

            return await Task.FromResult(weekOrderDto);
        }

        //Получить все плановые заявки на неделю
        [HttpPut]
        [Route("plan")]
        [ResponseType(typeof(PlanWeekOrderDto))]
        public async Task<PlanWeekOrderDto> GetPlanMenuOrders([FromBody] WeekYearDto wyDto)
        {
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            PlanWeekOrderDto weekOrderDto = PlanWeekOrderDto.GetMapDto(_unitOfWork, wyDto);

            return await Task.FromResult(weekOrderDto);
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
            _unitOfWork.GetContext().UpdateDishQuantityBySu(userOrderDto);

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
