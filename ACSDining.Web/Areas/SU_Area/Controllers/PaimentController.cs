using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;

        public PaimentController(IMenuForWeekService weekMenuService, IOrderMenuService orderMenuService)
        {
            _weekMenuService = weekMenuService;
            _orderMenuService = orderMenuService;
        }

        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (PaimentsDTO))]
        public async Task<IHttpActionResult> GetWeekPaiments([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus = _orderMenuService.GetAllByWeekYear(week,yearnum)
                        .ToList();
            MenuForWeek mfw = _weekMenuService.GetWeekMenuByWeekYear(week, yearnum);
            if (mfw == null)
            {
                return NotFound();
            }

            PaimentsDTO model = new PaimentsDTO
            {
                WeekNumber = week,
                YearNumber = yearnum,
                UserPaiments = orderMenus
                    .Select(order => new UserPaimentDTO
                    {
                        UserId = order.User.Id,
                        OrderId = order.Id,
                        UserName = order.User.UserName,
                        Paiments = _orderMenuService.UserWeekOrderPaiments(order.Id),
                        SummaryPrice = order.SummaryPrice,
                        WeekPaid = order.WeekPaid,
                        Balance = order.Balance,
                        Note = order.Note
                    }).OrderBy(uo => uo.UserName).ToList(),
                UnitPrices = _weekMenuService.UnitWeekPrices(mfw.ID),
                UnitPricesTotal = PaimentsByDishes(week, yearnum)
            };

            return Ok(model);
        }

        private double[] PaimentsByDishes(int numweek, int year )
        {
            double[] paiments = new double[21];
            MenuForWeek weekmenu = _weekMenuService.GetAll().FirstOrDefault(m => m.WorkingWeek.WeekNumber == numweek && m.WorkingWeek.Year.YearNumber == year);
            double[] weekprices = _weekMenuService.UnitWeekPrices(weekmenu.ID);


            OrderMenu[] orderMenus = _orderMenuService.GetAllByWeekYear(numweek,year)
                        .ToArray();
            for (int i = 0; i < orderMenus.Length; i++)
            {
                double[] dishquantities = _orderMenuService.UserWeekOrderDishes(orderMenus[i].Id);
                for (int j = 0; j < 20; j++)
                {
                    paiments[j] += weekprices[j]*dishquantities[j];
                }
            }
            paiments[20] = paiments.Sum();
            return paiments;
        }

        [HttpPut]
        [Route("updatePaiment/{orderid}")]
        [ResponseType(typeof(double))]
        public async Task<IHttpActionResult> UpdatePaiment( int orderid, double pai)
        {
            OrderMenu order = _orderMenuService.Find(orderid);
            if (order == null)
            {
                return NotFound();
            }
            order.Balance += order.WeekPaid;
            order.WeekPaid = pai;
            order.Balance -= order.WeekPaid;

            _orderMenuService.UpdateOrderMenu(order);

            return Ok(order.Balance);
        }
    }
}
