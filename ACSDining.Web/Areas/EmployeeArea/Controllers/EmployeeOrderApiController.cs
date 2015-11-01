using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ACSDining.Core.Domains;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        ApplicationDbContext context=new ApplicationDbContext();

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri]int? numweek = null, [FromUri]int? year = null)
        {
            numweek = numweek ?? context.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            MenuForWeek mfw = context.MenuForWeek.AsEnumerable().FirstOrDefault(wm => wm.WeekNumber == numweek && wm.Year.YearNumber == year);
            WeekMenuModel model=new WeekMenuModel(mfw);
            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(context.CurrentWeek());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks =  context.MenuForWeek.AsEnumerable().Select(wm => wm.WeekNumber).Reverse().ToList();
            if (numweeks == null)
            {
                 return NotFound();
            }

            return Ok(numweeks);
        }
    }
}
