using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public OrdersController(IUnitOfWorkAsync unitOfWorkAsync, IMenuForWeekService weekMenuService, IOrderMenuService orderMenuService)
        {
            _unitOfWork = unitOfWorkAsync;
            _weekMenuService = weekMenuService;
            _orderMenuService = orderMenuService;
        }


        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        public async Task<OrdersDTO> GetMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            return await Task.FromResult(_orderMenuService.OrdersDtoByWeekYear(week, yearnum));
        }

        [HttpPut]
        [Route("summary/{numweek}/{year}")]
        [ResponseType(typeof (double))]
        public async Task<double> GetSummaryPrice([FromBody] UserOrdersDTO usorder, [FromUri] int? numweek = null,
            [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
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
