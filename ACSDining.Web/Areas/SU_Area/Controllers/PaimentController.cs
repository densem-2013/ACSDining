using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (PaimentsDTO))]
        public async Task<IHttpActionResult> GetWeekPaiments([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus =
                await
                    _db.OrderMenus.Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToListAsync();
            MenuForWeek mfw = _db.MenuForWeeks.FirstOrDefault(m => m.WeekNumber == numweek && m.Year.YearNumber == year);
            PaimentsDTO model = null;
            if (mfw != null)
            {
                model = new PaimentsDTO()
                {
                    WeekNumber = (int) numweek,
                    YearNumber = (int) year,
                    UserPaiments = orderMenus
                        .Select(order => new UserPaimentDTO()
                        {
                            UserId = order.User.Id,
                            OrderId = order.Id,
                            UserName = order.User.UserName,
                            Paiments = _db.GetUserWeekOrderPaiments(order.Id),
                            SummaryPrice = order.SummaryPrice,
                            WeekPaid = order.WeekPaid,
                            Balance = order.Balance,
                            IsDiningRoomClient = order.User.IsDiningRoomClient,
                            Note = order.Note
                        }).OrderBy(uo => uo.UserName).ToList(),
                    UnitPrices = _db.GetUnitWeekPrices(mfw.ID)
                };
            }
            return Ok(model);
        }

        //[HttpGet]
        //[Route("unitprices/{numweek}/{year}")]
        //[ResponseType(typeof (double[]))]
        //public async Task<IHttpActionResult> UnitPrices([FromUri] int? numweek = null, [FromUri] int? year = null)
        //{
        //    numweek = numweek ?? _db.CurrentWeek();
        //    year = year ?? DateTime.Now.Year;
        //    MenuForWeek weekmenu =
        //        await _db.MenuForWeeks.FirstOrDefaultAsync(m => m.WeekNumber == numweek && m.Year.YearNumber == year);
        //    if (weekmenu == null)
        //    {
        //        return NotFound();
        //    }

        //    double[] weekprices = _db.GetUnitWeekPrices(weekmenu.ID);

        //    return Ok(weekprices);
        //}

        [HttpGet]
        [Route("paimentsByDish/{numweek}/{year}")]
        [ResponseType(typeof (double[]))]
        public async Task<IHttpActionResult> PaimentsByDishes([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            double[] paiments = new double[20];
            MenuForWeek weekmenu =
                await _db.MenuForWeeks.FirstOrDefaultAsync(m => m.WeekNumber == numweek && m.Year.YearNumber == year);
            double[] weekprices = _db.GetUnitWeekPrices(weekmenu.ID);


            OrderMenu[] orderMenus =
                await
                    _db.OrderMenus.Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToArrayAsync();
            if (weekmenu == null || orderMenus == null)
            {
                return NotFound();
            }
            for (int i = 0; i < orderMenus.Length; i++)
            {
                double[] dishquantities = _db.GetUserWeekOrderDishes(orderMenus[i].Id);
                for (int j = 0; j < 20; j++)
                {
                    paiments[j] += weekprices[j]*dishquantities[j];
                }
            }
            return Ok(paiments);
        }

        [HttpPut]
        [Route("updatePaiment/{orderid}")]
        [ResponseType(typeof(double))]
        public async Task<IHttpActionResult> UpdatePaiment( int orderid, double pai)
        {
            OrderMenu order = await _db.OrderMenus.FindAsync(orderid);
            if (order == null)
            {
                return NotFound();
            }
            order.Balance += order.WeekPaid;
            order.WeekPaid = pai;
            order.Balance -= order.WeekPaid;
            _db.Entry(order).State=EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok(order.Balance);
        }
    }
}
