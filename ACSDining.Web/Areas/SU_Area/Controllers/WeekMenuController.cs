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

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/WeekMenu")]
    [EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
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
                                    Category = dm.DishType.Category,
                                    IsSelected = true
                                }).ToList()
                }).ToList()
            }).ToList();

        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("{numweek}")]
        [ResponseType(typeof(WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu(int? numweek)
        {
            WeekMenuModel model =
                WeekModels.FirstOrDefault(wm => wm.WeekNumber == (numweek == null ? DB.CurrentWeek() : numweek));
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
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks =  WeekModels.Select(wm => wm.WeekNumber).ToList();
            if (numweeks == null)
            {
                 return NotFound();
            }

            return Ok(numweeks);
        }
        // GET api/WeekMenu/5
        //[ResponseType(typeof(MenuForWeek))]
        //public async Task<IHttpActionResult> GetMenuForWeek(int id)
        //{
        //    MenuForWeek menuforweek = await DB.MenuForWeek.FindAsync(id);
        //    if (menuforweek == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(menuforweek);
        //}

        //// GET api/WeekMenu/5
        //[ResponseType(typeof(MenuForWeek))]
        //public async Task<IHttpActionResult> GetMenuForWeekNumber(int weeknumber)
        //{
        //    MenuForWeek menuforweek = await DB.MenuForWeek.Where(m => m.WeekNumber == weeknumber).FirstOrDefaultAsync();
        //    if (menuforweek == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(menuforweek);
        //}
        // PUT api/WeekMenu/5
        [HttpPut]
        [Route("{numweek}")]
        [ResponseType(typeof(WeekMenuModel))]
        public async Task<IHttpActionResult> UpdateMFD([FromUri()]int numweek,[FromBody()] /*[ModelBinder(typeof(PoolRequestModelBinder))]*/WeekMenuModel menuforweek)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (numweek != menuforweek.WeekNumber)
            {
                return BadRequest();
            }
            MenuForWeek mfwModel = await DB.MenuForWeek.FindAsync(menuforweek.ID);
            mfwModel.MenuForDay.Clear();
            foreach (MenuForDayModel mfd in menuforweek.MFD_models)
            {
                List<Dish> dishes = DB.Dishes.AsEnumerable().Join(  mfd.Dishes.AsEnumerable(),
                                                                    dm => dm.DishID,
                                                                    md => md.DishID,
                                                                    (dm, md) => dm).ToList();

                MenuForDay menuFD = DB.MenuForDay.Find( mfd.ID);
                menuFD.Dishes = dishes;
                mfwModel.MenuForDay.Add(menuFD);
                DB.Entry(menuFD).State = EntityState.Added;
                try
                {
                    await DB.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            //DB.Entry(mfwModel).State = EntityState.Modified;

            //try
            //{
            //    await DB.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!MenuForWeekExists(numweek))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}
            return StatusCode(HttpStatusCode.NoContent);
        }

        //// POST api/WeekMenu
        //[ResponseType(typeof(MenuForWeek))]
        //public async Task<IHttpActionResult> PostMenuForWeek(MenuForWeek menuforweek)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    DB.MenuForWeek.Add(menuforweek);

        //    try
        //    {
        //        await DB.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (MenuForWeekExists(menuforweek.ID))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtRoute("DefaultApi", new { id = menuforweek.ID }, menuforweek);
        //}

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