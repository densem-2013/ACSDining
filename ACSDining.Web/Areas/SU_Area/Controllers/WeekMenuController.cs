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

        private List<WeekMenuModel> WeekModels
        {
            get { return DB.MenuForWeeks.AsEnumerable().Select(wmenu => new WeekMenuModel(wmenu)).ToList(); }
        }

        public WeekMenuController()
        {
            DB = new ApplicationDbContext();
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
        public async Task<MenuForWeek> GetNexWeekMenu()
        {
            int nextnum = DB.CurrentWeek() + 1;
            if(MenuForWeekExists(nextnum))
            {
                return await DB.MenuForWeeks.FindAsync(nextnum);
            }
            return new MenuForWeek();
            //{
            //    WeekNumber = nextnum,,
            //    Year = await DB.Years.FirstOrDefaultAsync(y => y.YearNumber == menumodel.YearNumber)

            //}
        }

        [HttpGet]
        [Route("curWeekNumber")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(DB.CurrentWeek());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof(string[]))]
        public async Task<string[]> GetCategories()
        {
            return await Task.FromResult(DB.DishTypes.AsEnumerable().OrderBy(d=>d.Id).Select(dt => dt.Category).ToArray());
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
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayModel menuforday)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Dish> dishes =
                menuforday.Dishes.SelectMany(d => DB.Dishes.Where(dish => dish.DishID == d.DishID)).ToList();

            MenuForDay menuFD = await DB.MenuForDays.Include("Dishes").FirstOrDefaultAsync(m => m.ID == menuforday.ID);
                menuFD.Dishes = dishes;
                menuFD.TotalPrice = menuforday.TotalPrice;
                DB.Entry(menuFD).State = EntityState.Modified;

            MenuForWeek mfwModel =
                await DB.MenuForWeeks.FirstOrDefaultAsync(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd=>mfd.TotalPrice);
            DB.Entry(mfwModel).State = EntityState.Modified;
            await DB.SaveChangesAsync();

            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateNextWeekMenu([FromBody] WeekMenuModel menumodel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MenuForWeek target = new MenuForWeek
            {
                WeekNumber = menumodel.WeekNumber,
                SummaryPrice = menumodel.SummaryPrice,
                Year = await DB.Years.FirstOrDefaultAsync(y => y.YearNumber == menumodel.YearNumber),
                MenuForDay = menumodel.MFD_models.Select(m=>new MenuForDay
                {
                    DayOfWeek = DB.Days.FirstOrDefault(d=>string.Equals(d.Name,m.DayOfWeek)),
                    Dishes = m.Dishes.Select(d=>DB.Dishes.Find(d.DishID)).ToList()
                }).ToList()
            };

            DB.MenuForWeeks.Add(target);
            await DB.SaveChangesAsync();

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