using ACSDining.Core.Domains;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        readonly ApplicationDbContext _db =new ApplicationDbContext();

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(OrdersDTO))]
        public async Task<IHttpActionResult> GetMenuOrders([FromUri]int? numweek = null, [FromUri]int? year = null)
        {
            numweek = numweek ?? _db.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            OrderMenu orderMenu = await _db.OrderMenu.Include("DishQuantities").Include("User").FirstOrDefaultAsync(om=>om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year);
            OrdersDTO model = new OrdersDTO
            {
                Id=orderMenu.Id,
                UserOrders = _db.Users.Include("OrderMenus").AsEnumerable().Select(u=>u.OrderMenus.FirstOrDefault(ord=>ord.MenuForWeek.WeekNumber==numweek)).Select(order=>new UserOrdesDTO()
                {
                    UserId = order.User.Id,
                    UserName = order.User.UserName,
                    Dishquantities = orderMenu.DishQuantities.Select(q=>q.Quantity).ToArray(),
                    WeekIsPaid = false
                }).ToList()
            };
            if (model == null)
            { 
                return NotFound();
            }

            return Ok(model);
        }
    }
}
