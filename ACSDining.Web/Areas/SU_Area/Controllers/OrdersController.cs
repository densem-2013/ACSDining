using ACSDining.Core.Domains;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Web.Areas.SU_Area.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("")]
        [System.Web.Http.Route("{numweek}")]
        [System.Web.Http.Route("{numweek}/{year}")]
        [ResponseType(typeof (OrdersDTO))]
        public async Task<IHttpActionResult> GetMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus =
                await
                    _db.OrderMenus.Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToListAsync();

            OrdersDTO model = new OrdersDTO()
            {
                WeekNumber = (int) numweek,
                UserOrders = orderMenus
                    .Select(order => new UserOrdesDTO()
                    {
                        UserId = order.User.Id,
                        UserName = order.User.UserName,
                        Dishquantities = _db.GetUnitWeekPrices(order.Id),
                        WeekIsPaid = false,
                        SummaryPrice = order.SummaryPrice
                    }).OrderBy(uo => uo.UserName).ToList(),
                YearNumber = (int) year
            };
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }

        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("summary/{numweek}/{year}")]
        [ResponseType(typeof (double))]
        public async Task<double> GetSummaryPrice([FromBody] UserOrdesDTO usorder, [FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            MenuForWeek weekNeeded =
                await _db.MenuForWeeks.FirstOrDefaultAsync(wm => wm.WeekNumber == numweek && wm.Year.YearNumber == year);
            double Summary = 0;
            if (weekNeeded != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Summary += weekNeeded.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price*
                                   usorder.Dishquantities[4*i + j];
                    }
                }
            }
            return await Task.FromResult(Summary);
        }
    }
}
