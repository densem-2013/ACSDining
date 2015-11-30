using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Web.Areas.SU_Area.Models;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser,Administrator")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DayOfWeek> _dayRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;
        private readonly IRepository<MenuForDay> _daymenuRepository;
        private readonly IRepository<Year> _yearRepository;
        private readonly IRepository<DishType> _dishtypeRepository;
        private readonly IRepository<Dish> _dishRepository; 

        public WeekMenuController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dayRepository = _unitOfWork.Repository<DayOfWeek>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
            _daymenuRepository = _unitOfWork.Repository<MenuForDay>();
            _yearRepository = _unitOfWork.Repository<Year>();
            _dishtypeRepository = _unitOfWork.Repository<DishType>();
            _dishRepository = _unitOfWork.Repository<Dish>();
        }


        private List<WeekMenuModel> WeekModels
        {
            get { return _weekmenuRepository.GetAll().Select(wmenu => new WeekMenuModel(wmenu)).ToList(); }
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuModel))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
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
        public  Task<bool> IsNexWeekMenuExist()
        {
            int curweek = UnitOfWork.CurrentWeek();
            int nextweeknumber = _unitOfWork.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year {YearNumber = DateTime.Now.Year + 1}
                : _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year);
            MenuForWeek nextWeek =
                _weekmenuRepository.Find(
                        mfw => mfw.WeekNumber == nextweeknumber && mfw.Year.YearNumber == year.YearNumber);

            return Task.FromResult(nextWeek != null);
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<IHttpActionResult> GetNexWeekMenu()
        {
            int curweek = UnitOfWork.CurrentWeek();
            int nextweeknumber = _unitOfWork.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year {YearNumber = DateTime.Now.Year + 1}
                : _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year);
            MenuForWeek nextWeek = _weekmenuRepository.Find(
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
                    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                    new Year {YearNumber = year.YearNumber},
                    MenuForDay = _dayRepository.GetAll().OrderBy(d => d.ID).Select(day => new MenuForDay
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
            return await Task.FromResult(UnitOfWork.CurrentWeek());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {
            return
                await
                    Task.FromResult(_dishtypeRepository.GetAll().OrderBy(d => d.Id).Select(dt => dt.Category).ToArray());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof (List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks = WeekModels.Select(wm => wm.WeekNumber).ToList();
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
                menuforday.Dishes.SelectMany(d => _dishRepository.GetAll().Where(dish => dish.DishID == d.DishID)).ToList();

            MenuForDay menuFD = _daymenuRepository.Find(m => m.ID == menuforday.ID);
            menuFD.Dishes = dishes;
            menuFD.TotalPrice = menuforday.TotalPrice;
            _daymenuRepository.Update(menuFD);

            MenuForWeek mfwModel =_weekmenuRepository.Find(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);
            _weekmenuRepository.Update(mfwModel);

            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateNextWeekMenu()
        {

            int curweek = UnitOfWork.CurrentWeek();
            int nextweeknumber = _unitOfWork.GetNextWeekYear();
            Year year = nextweeknumber < curweek
                ? new Year { YearNumber = DateTime.Now.Year + 1 }
                : _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year);

            WeekMenuModel model = null;

            if (year != null)
            {
                int maxID = _daymenuRepository.GetAll().Max(m => m.ID);
                MenuForWeek nextWeek = new MenuForWeek
                {
                    WeekNumber = nextweeknumber,
                    Year =
                        _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                        new Year { YearNumber = year.YearNumber },
                    MenuForDay = _dayRepository.GetAll().OrderBy(d => d.ID).Select(day => new MenuForDay
                    {
                        ID = ++maxID,
                        DayOfWeek = day
                    }).ToList()

                };
                try
                {
                    _weekmenuRepository.Insert(nextWeek);
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
            MenuForWeek menuforweek = _weekmenuRepository.Find(mfw => mfw.WeekNumber == numweek);
            if (menuforweek == null)
            {
                return NotFound();
            }
            try
            {
                _weekmenuRepository.Delete(menuforweek);
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
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}