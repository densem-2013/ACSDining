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
        [ResponseType(typeof(PaimentsDTO))]
        public async Task<IHttpActionResult> GetWeekPaiments([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus =
                await
                    _db.OrderMenus.Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToListAsync();

            PaimentsDTO model = new PaimentsDTO()
            {
                WeekNumber = (int)numweek,
                YearNumber = (int)year,
                UserPaiments = orderMenus
                    .Select(order => new UserPaimentDTO()
                    {
                        UserId = order.User.Id,
                        UserName = order.User.UserName,
                        Paiments = _db.GetUserWeekOrderPaiments(order.Id),
                        SummaryPrice = order.SummaryPrice,
                        WeekPaid = order.WeekPaid,
                        Balance = order.Balance
                    }).OrderBy(uo => uo.UserName).ToList()
            };
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }
    }
}
