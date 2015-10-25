using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using System.Globalization;
using ACSDining.Web.Areas.SU_Area.Models;
using System.Web.Http.Cors;
using System.Web.Http.ModelBinding;
using WebGrease.Css;
using System.Web.Http.Results;

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
            WeekModels = DB.MenuForWeek.Select(wmenu => new WeekMenuModel()
            {
                ID = wmenu.ID,
                WeekNumber = wmenu.WeekNumber,
                SummaryPrice = wmenu.SummaryPrice,
                YearNumber = wmenu.Year.YearNumber,
                MFD_models = wmenu.MenuForDay.Select(m => new MenuForDayModel()
                {
                    ID = m.ID,
                    DayOfWeek = m.DayOfWeek.Name,
                    TotalPrice = m.TotalPrice,
                    Dishes = DB.Dishes.AsEnumerable().Join(
                            m.Dishes.AsEnumerable(),
                            dm => dm.DishID,
                            md => md.DishID,
                            (dm, md) => new DishModel()
                                {
                                    DishID = dm.DishID,
                                    Title = dm.Title,
                                    ProductImage = dm.ProductImage,
                                    Price = dm.Price,
                                    Category = dm.DishType.Category
                                }).ToList()
                }).ToList()
            }).ToList();
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
        public async Task<IHttpActionResult> CurrentWeekNumber()
        {
            return Ok(DB.CurrentWeek());
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

                MenuForDay menuFD = await DB.MenuForDay.Include("Dishes").SingleOrDefaultAsync( m=>m.ID==mfd.ID);
                menuFD.Dishes = dishes;
                menuFD.TotalPrice = mfd.TotalPrice;
                DB.Entry(menuFD).State = EntityState.Modified;

               
            }
            MenuForWeek mfwModel = await DB.MenuForWeek.FindAsync(menuforweek.ID);

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
            MenuForWeek menuforweek = await DB.MenuForWeek.FindAsync(id);
            if (menuforweek == null)
            {
                return NotFound();
            }

            DB.MenuForWeek.Remove(menuforweek);
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
            return DB.MenuForWeek.Count(e => e.ID == id) > 0;
        }
    }
}