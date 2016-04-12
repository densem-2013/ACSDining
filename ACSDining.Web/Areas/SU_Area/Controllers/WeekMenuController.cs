using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser, Employee")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekmenuService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDishService _dishService;
        private readonly IWorkDaysService _workDaysService;

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = (UnitOfWork) unitOfWork;
            _db = _unitOfWork.GetContext();
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
            _workDaysService = new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromBody] WeekYearDto wyDto=null)
        {

            if (wyDto==null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            return await Task.FromResult(WeekMenuDtoByWeekYear(wyDto));
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public Task<bool> IsNexWeekMenuExist(WeekYearDto weekyear)
        {
            WeekYearDto nextweeknumber = YearWeekHelp.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber);

            return Task.FromResult(nextWeek != null);
        }

        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof (WeekYearDto))]
        public Task<WeekYearDto> GetNextWeekYear([FromBody] WeekYearDto weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof (WeekYearDto))]
        public Task<WeekYearDto> GetPrevWeekYear([FromBody] WeekYearDto weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetPrevWeekYear(weekyear));
        }

        [HttpGet]
        [Route("curWeekYear")]
        [ResponseType(typeof (Int32))]
        public async Task<WeekYearDto> CurrentWeekYear()
        {
            return await Task.FromResult(YearWeekHelp.GetCurrentWeekYearDto());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {

            string[] categories = MapHelper.GetCategories(_unitOfWork);

            return categories;

        }

        [HttpGet]
        [Route("WeekNumbers")]
        public async Task<List<int>> GetWeekNumbers()
        {
            return await Task.FromResult(_weekmenuService.WeekNumbers());
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayDto menuforday)
        {
            MenuForDay menuFd = _unitOfWork.RepositoryAsync<MenuForDay>().Find(menuforday.Id);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (menuFd == null)
            {
                return NotFound();
            }
            List<Dish> dishes =
                menuforday.Dishes.SelectMany(d => _dishService.AllDish().Where(dish => dish.DishID == d.DishID))
                    .ToList();

            menuFd.Dishes = dishes;
            menuFd.TotalPrice = menuforday.TotalPrice;

            _db.MenuForDays.Remove(menuFd);
            _db.MenuForDays.Add(menuFd);

            _unitOfWork.SaveChanges();


            MenuForWeek mfwModel =
                _weekmenuService.GetAll()
                    .ToList()
                    .FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.Id));

            if (mfwModel != null)
            {
                mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);

                _db.MenuForWeeks.Remove(mfwModel);
                _db.MenuForWeeks.Add(mfwModel);
                //_weekmenuService.Update(mfwModel);
            }

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(HttpStatusCode.OK);
        }
        
        // DELETE api/WeekMenu/5
        [HttpDelete]
        [Route("delete/{menuid}")]
        [ResponseType(typeof (MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int menuid)
        {
            MenuForWeek mfw = _weekmenuService.Find(menuid);
            if (mfw == null)
            {
                return NotFound();
            }

            _db.MenuForWeeks.Remove(mfw);

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Запрашивает объект MenuForWeek из базы
        /// Если объект не существует, проверяет является ли запрашиваемый объект меню на следующую неделю или на текущую неделю в системе
        /// Если является то создаётся новое меню (пустое), сохраняет его в базе и отправляет его DTO клиенту
        /// Если запрашивается меню на новую рабочую неделю, которой ещё нет в базе, создаётся новая рабочая неделя с рабочими 
        /// днями Понедельник - Пятница
        /// Если год не существует в базе, создаётся новый
        /// </summary>
        /// <param name="weekyear"></param>
        /// <returns></returns>
        private WeekMenuDto WeekMenuDtoByWeekYear(WeekYearDto weekyear)
        {
            MenuForWeek weekmenu = _weekmenuService.GetWeekMenuByWeekYear(weekyear);
            if (weekmenu != null)
            {
                var dto = WeekMenuDto.MapDto(_unitOfWork, weekmenu);
                return dto;
            }
           // if (!YearWeekHelp.WeekIsCurrentOrNext(weekyear)) return null;

            List<WorkingDay> workdays = new List<WorkingDay>();

            Year year =
                _unitOfWork.RepositoryAsync<Year>().Queryable().FirstOrDefault(y => y.YearNumber == weekyear.Year) ??
                new Year {YearNumber = weekyear.Year};

            List<WorkingDay> wdays=new List<WorkingDay>();
            for (int i = 0; i < 7; i++)
            {
                WorkingDay wday = new WorkingDay
                {
                    DayOfWeek =
                        _unitOfWork.RepositoryAsync<ACSDining.Core.Domains.DayOfWeek>().FindAsync(i + 1).Result,
                    IsWorking = i < 5
                };
            }

            WorkingWeek workWeek = _workDaysService.GetWorkWeekByWeekYear(weekyear);

            if (workWeek == null)
            {
                workWeek = new WorkingWeek
                {
                    WeekNumber = weekyear.Week,
                    Year = year,
                    WorkingDays = wdays
                };
                _db.WorkingWeeks.Add(workWeek);
                _unitOfWork.SaveChanges();

                workWeek = _workDaysService.GetWorkWeekByWeekYear(weekyear);
            }


            if (year.WorkingWeeks.FirstOrDefault(ww => ww.WeekNumber == workWeek.WeekNumber) == null)
            {
                year.WorkingWeeks.Add(workWeek);
            } 
            
            for (var i = 0; i < 7; i++)
            {
                if (workWeek != null)
                {
                    WorkingDay wday = workWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.Id == i + 1) ;
                    MenuForDay mfd = new MenuForDay
                    {
                        //WorkingWeek = workWeek,
                        WorkingDay = wday,
                        DayMenuCanBeChanged = true
                    };
                    _db.MenuForDays.Add(mfd);
                    _unitOfWork.SaveChanges();
                }
            }
            int[] daysid = workWeek != null
                ? workWeek.WorkingDays.OrderBy(wd => wd.Id).Select(wd => wd.Id).ToArray()
                : null;

             List<MenuForDay> mfdays =
                _unitOfWork.RepositoryAsync<MenuForDay>()
                    .Query()
                    .Include(mfd => mfd.Dishes)
                    .Include(mfd => mfd.WorkingDay)
                    .Select()
                    .Where(mfd => daysid != null && (workWeek != null && daysid.Contains(mfd.WorkingDay.Id)))
                    .ToList();
            weekmenu = new MenuForWeek
            {
                WorkingWeek = workWeek,
                MenuForDay = mfdays
            };

            try
            {
                _db.MenuForWeeks.Add(weekmenu);
                _unitOfWork.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

            weekmenu = _weekmenuService.GetWeekMenuByWeekYear(weekyear);
            return WeekMenuDto.MapDto(_unitOfWork, weekmenu, true);
        }
    }
}