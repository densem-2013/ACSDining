using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        
        ApplicationDbContext DB { get; set; }
        List<WeekMenuModel> WeekModels { get; set; }

        public WeekMenuController()
        {
            DB = new ApplicationDbContext();
            WeekModels = DB.MenuForWeeks.AsEnumerable().Select(wmenu => new WeekMenuModel(wmenu)).ToList();
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri]int? numweek = null, [FromUri]int? year = null)
        {
            numweek = numweek ?? DB.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            WeekMenuModel model = WeekModels.FirstOrDefault(wm => wm.WeekNumber == numweek && wm.YearNumber == year);
            if (model == null)
            {
                model = WeekModels.FirstOrDefault();
                if (model == null)
                {
                    return NotFound();
                }
            }

            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(DB.CurrentWeek());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks =  WeekModels.Select(wm => wm.WeekNumber).Reverse().ToList();
            if (numweeks == null)
            {
                 return NotFound();
            }

            return Ok(numweeks);
        }

        [HttpPut]
        [Route("{numweek}")]
        public async Task<IHttpActionResult> UpdateMenuForWeek([FromUri()]int numweek, [FromBody] WeekMenuModel menuforweek)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (numweek != menuforweek.WeekNumber)
            {
                return BadRequest();
            }
            
            foreach (MenuForDayModel mfd in menuforweek.MFD_models)
            {
                List<Dish> dishes = DB.Dishes.AsEnumerable().Join(  mfd.Dishes.AsEnumerable(),
                                                                    dm => dm.DishID,
                                                                    md => md.DishID,
                                                                    (dm, md) => dm).ToList();

                MenuForDay menuFD = await DB.MenuForDays.Include("Dishes").SingleOrDefaultAsync( m=>m.ID==mfd.ID);
                menuFD.Dishes = dishes;
                menuFD.TotalPrice = mfd.TotalPrice;
                DB.Entry(menuFD).State = EntityState.Modified;

               
            }
            MenuForWeek mfwModel = await DB.MenuForWeeks.FindAsync(menuforweek.ID);

            mfwModel.SummaryPrice = menuforweek.SummaryPrice;
            DB.Entry(mfwModel).State = EntityState.Modified;
            try
            {
                await DB.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuForWeekExists(numweek))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE api/WeekMenu/5
        [ResponseType(typeof(MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int id)
        {
            MenuForWeek menuforweek = await DB.MenuForWeeks.FindAsync(id);
            if (menuforweek == null)
            {
                return NotFound();
            }

            DB.MenuForWeeks.Remove(menuforweek);
            await DB.SaveChangesAsync();

            return Ok(menuforweek);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MenuForWeekExists(int id)
        {
            return DB.MenuForWeeks.Count(e => e.ID == id) > 0;
        }
    }
}