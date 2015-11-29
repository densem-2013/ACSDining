using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser,Administrator")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {

        private ApplicationDbContext Db { get; set; }

        private List<WeekMenuModel> WeekModels
        {
            get { return Db.MenuForWeeks.AsEnumerable().Select(wmenu => new WeekMenuModel(wmenu)).ToList(); }
        }

        public WeekMenuController()
        {
            Db = new ApplicationDbContext();
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? Db.CurrentWeek();
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
        [Route("nwMenuExist")]
        public async Task<bool> IsNexWeekMenuExist()
        {
            int curweek = Db.CurrentWeek();
            int nextweeknumber = Db.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year {YearNumber = DateTime.Now.Year + 1}
                : Db.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            MenuForWeek nextWeek =
                await
                    Db.MenuForWeeks.FirstOrDefaultAsync(
                        mfw => mfw.WeekNumber == nextweeknumber && mfw.Year.YearNumber == year.YearNumber);

            return nextWeek != null;
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<IHttpActionResult> GetNexWeekMenu()
        {
            int curweek = Db.CurrentWeek();
            int nextweeknumber = Db.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year {YearNumber = DateTime.Now.Year + 1}
                : Db.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            MenuForWeek nextWeek =
                await
                    Db.MenuForWeeks.FirstOrDefaultAsync(
                        mfw => mfw.WeekNumber == nextweeknumber && mfw.Year.YearNumber == year.YearNumber);
            WeekMenuModel model = null;
            if (nextWeek != null)
            {
                model = new WeekMenuModel(nextWeek);
                return Ok(model);
            }
            nextWeek = new MenuForWeek
            {
                WeekNumber = nextweeknumber,
                Year =
                    Db.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year) ??
                    new Year {YearNumber = year.YearNumber},
                    MenuForDay = Db.Days.ToList().OrderBy(d=>d.ID).Select(day=>new MenuForDay
                    {
                        DayOfWeek = day
                    }).ToList()

            };

            model = new WeekMenuModel(nextWeek,true);
            return Ok(model);
        }

        [HttpGet]
        [Route("curWeekNumber")]
        [ResponseType(typeof (Int32))]
        public async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(Db.CurrentWeek());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {
            return
                await
                    Task.FromResult(Db.DishTypes.AsEnumerable().OrderBy(d => d.Id).Select(dt => dt.Category).ToArray());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof (List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks = WeekModels.Select(wm => wm.WeekNumber).ToList();
            if (numweeks == null)
            {
                return NotFound();
            }
            numweeks.Sort();
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
                menuforday.Dishes.SelectMany(d => Db.Dishes.Where(dish => dish.DishID == d.DishID)).ToList();

            MenuForDay menuFD = await Db.MenuForDays.Include("Dishes").FirstOrDefaultAsync(m => m.ID == menuforday.ID);
            menuFD.Dishes = dishes;
            menuFD.TotalPrice = menuforday.TotalPrice;
            Db.Entry(menuFD).State = EntityState.Modified;

            MenuForWeek mfwModel =
                await Db.MenuForWeeks.FirstOrDefaultAsync(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);
            Db.Entry(mfwModel).State = EntityState.Modified;
            await Db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateNextWeekMenu()
        {

            int curweek = Db.CurrentWeek();
            int nextweeknumber = Db.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year { YearNumber = DateTime.Now.Year + 1 }
                : Db.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);

            WeekMenuModel model = null;

            if (year != null)
            {
                int maxID = Db.MenuForDays.Max(m => m.ID);
                MenuForWeek nextWeek = new MenuForWeek
                {
                    WeekNumber = nextweeknumber,
                    Year =
                        Db.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year) ??
                        new Year { YearNumber = year.YearNumber },
                    MenuForDay = Db.Days.ToList().OrderBy(d => d.ID).Select(day => new MenuForDay
                    {
                        ID = ++maxID,
                        DayOfWeek = day
                    }).ToList()

                };
                try
                {
                    Db.MenuForWeeks.Add(nextWeek);
                    await Db.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }
                model = new WeekMenuModel(nextWeek, true);
            }
            return Ok(model);
        }

        // DELETE api/WeekMenu/5
        [HttpDelete]
        [Route("delete/{numweek}")]
        [ResponseType(typeof (MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int numweek)
        {
            MenuForWeek menuforweek = await Db.MenuForWeeks.FirstOrDefaultAsync(mfw => mfw.WeekNumber == numweek);
            if (menuforweek == null)
            {
                return NotFound();
            }
            try
            {
                Db.MenuForWeeks.Remove(menuforweek);
                await Db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                    
                throw new Exception(ex.Message);
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}