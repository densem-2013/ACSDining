using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Infrastructure.DAL;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private readonly IMenuForWeekService _weekmenuService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDishService _dishService;
        private readonly IMenuforDayService _daymenuService;

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = (UnitOfWork) unitOfWork;
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
            _daymenuService = new MenuForDayService(_unitOfWork.RepositoryAsync<MenuForDay>());
        }



        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var menu = _weekmenuService.GetWeekMenuByWeekYear(week, yearnum);

            return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork, menu));
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public Task<bool> IsNexWeekMenuExist(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = YearWeekHelp.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber.Week, nextweeknumber.Year);

            return Task.FromResult(nextWeek != null);
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<WeekMenuDto> GetNextWeekMenu(WeekYearDTO weekyear)
        {

            return await Task.FromResult(GetNextWeekMenuDtoByCurrentWekYear(weekyear));
        }

        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof (WeekYearDTO))]
        public Task<WeekYearDTO> GetNextWeekYear([FromBody] WeekYearDTO weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof (WeekYearDTO))]
        public Task<WeekYearDTO> GetPrevWeekYear([FromBody] WeekYearDTO weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetPrevWeekYear(weekyear));
        }

        [HttpGet]
        [Route("curWeekNumber")]
        [ResponseType(typeof (Int32))]
        public async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(YearWeekHelp.CurrentWeek());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {
            return
                await
                    _unitOfWork.Repository<DishType>()
                        .Queryable()
                        .ToList()
                        .OrderBy(d => d.Id)
                        .Select(dt => dt.Category)
                        .AsQueryable()
                        .ToArrayAsync();
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
            MenuForDay menuFd = _daymenuService.Find(menuforday.ID);
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

            _daymenuService.Update(menuFd);


            MenuForWeek mfwModel =
                _weekmenuService.GetAll()
                    .ToList()
                    .FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            if (mfwModel != null)
            {
                mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);

                _weekmenuService.Update(mfwModel);
            }

            await _unitOfWork.SaveChangesAsync();

            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<WeekMenuDto> CreateNextWeekMenu(WeekYearDTO weekyear)
        {

            WeekYearDTO nextweekDto = YearWeekHelp.GetNextWeekYear(weekyear);
            Year nextYear =
                _unitOfWork.Repository<Year>().Queryable().FirstOrDefault(y => y.YearNumber == nextweekDto.Year) ??
                new Year
                {
                    YearNumber = nextweekDto.Year
                };

            WorkingWeek nextworkingWeek = _unitOfWork.RepositoryAsync<WorkingWeek>().Queryable().FirstOrDefault(
                w => w.WeekNumber == nextweekDto.Week && w.Year.YearNumber == nextweekDto.Year) ?? new WorkingWeek
                {
                    WeekNumber = nextweekDto.Week,
                    Year = nextYear
                };
            MenuForWeek nextWeek = new MenuForWeek
            {
                WorkingWeek = nextworkingWeek,
                MenuForDay = nextworkingWeek.WorkingDays.Select(day => new MenuForDay
                {
                    WorkingDay =
                        _unitOfWork.RepositoryAsync<WorkingDay>()
                            .Query()
                            .Include(wd => wd.WorkingWeek)
                            .Include(wd => wd.DayOfWeek)
                            .Select()
                            .FirstOrDefault(
                                wd => wd.WorkingWeek.ID == nextworkingWeek.ID && wd.DayOfWeek.ID == day.ID),
                    WorkingWeek = nextworkingWeek
                }).ToList()

            };


            try
            {
                _weekmenuService.Insert(nextWeek);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork, nextWeek, true));
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

            _weekmenuService.Delete(mfw);

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


        private WeekMenuDto GetNextWeekMenuDtoByCurrentWekYear(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = YearWeekHelp.GetNextWeekYear(weekyear);

            MenuForWeek nextWeek = _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber.Week, nextweeknumber.Year);

            if (nextWeek != null)
            {
                var dto = WeekMenuDto.MapDto(_unitOfWork, nextWeek);
                return dto;
            }
            Year nextYear =
                _unitOfWork.Repository<Year>().Queryable().FirstOrDefault(y => y.YearNumber == nextweeknumber.Year) ??
                new Year
                {
                    YearNumber = nextweeknumber.Year
                };
            WorkingWeek workingWeek = new WorkingWeek
            {
                WeekNumber = nextweeknumber.Week,
                Year = nextYear
            };

            var workdays = _daymenuService.Queryable().ToList();
            workdays.OrderBy(wd => wd.ID);
            nextWeek = new MenuForWeek
            {
                WorkingWeek = workingWeek,
                MenuForDay = workdays.Select(day =>
                {
                    var firstOrDefault = workdays.FirstOrDefault(
                        wd => wd.WorkingWeek.ID == workingWeek.ID && wd.WorkingDay.ID == day.ID);
                    if (firstOrDefault != null)
                        return new MenuForDay
                        {
                            WorkingWeek = workingWeek,
                            WorkingDay = firstOrDefault.WorkingDay
                        };
                    return null;
                }).ToList()

            };

            return WeekMenuDto.MapDto(_unitOfWork, nextWeek, true);
        }
    }
}