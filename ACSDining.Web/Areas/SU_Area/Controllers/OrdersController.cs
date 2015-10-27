using ACSDining.Core.Domains;
using System;
using System.Data.Entity;
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
                //Id=orderMenu.Id,
                //UserOrders = new 
            };
            if (model == null)
            { 
                return NotFound();
            }

            return Ok(orderMenu);
        }
    }
}
