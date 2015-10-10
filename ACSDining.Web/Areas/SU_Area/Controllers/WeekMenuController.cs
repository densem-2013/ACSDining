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

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    public class WeekMenuController : ApiController
    {
        
        ApplicationDbContext DB { get; set; }
        List<WeekMenuModel> WeekModels { get; set; }
        List<DishModel> Dishes { get; set; }

        //DishModel Compare(DishModel a, DishModel b)
        //{
        //    int id_one = DB.DishTypes.ToList().Where(dt => string.Equals(dt.Category, a.Category)).Select(dtype => dtype.Id).FirstOrDefault();
        //    int id_two = DB.DishTypes.ToList().Where(dt => string.Equals(dt.Category, b.Category)).Select(dtype => dtype.Id).FirstOrDefault();
        //    // Return result of CompareTo with lengths of both strings.
        //    return id_one>id_two?a:b;
        //}

        public WeekMenuController()
        {
            //Comparison<DishModel> comparison = new Comparison<DishModel>(Compare);
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

            Dishes = DB.Dishes.AsEnumerable().Select(d => new DishModel()
            {
                DishID = d.DishID,
                Title = d.Title,
                ProductImage = d.ProductImage,
                Price = d.Price,
                Category = d.DishType.Category,
                IsSelected = false

            }).ToList();
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("api/WeekMenu")]
        [ResponseType(typeof(WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu()
        {
            WeekMenuModel model = WeekModels.FirstOrDefault(wm => wm.WeekNumber == DB.CurrentWeek());
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
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
        public async Task<IHttpActionResult> PutMenuForWeek(int id, MenuForWeek menuforweek)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != menuforweek.ID)
            {
                return BadRequest();
            }

            DB.Entry(menuforweek).State = EntityState.Modified;

            try
            {
                await DB.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuForWeekExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/WeekMenu
        [ResponseType(typeof(MenuForWeek))]
        public async Task<IHttpActionResult> PostMenuForWeek(MenuForWeek menuforweek)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DB.MenuForWeek.Add(menuforweek);

            try
            {
                await DB.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MenuForWeekExists(menuforweek.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = menuforweek.ID }, menuforweek);
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