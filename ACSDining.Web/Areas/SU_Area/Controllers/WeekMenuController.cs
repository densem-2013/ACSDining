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

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = (UnitOfWork) unitOfWork;
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            WeekYearDto dto = new WeekYearDto
            {
                Week = numweek ?? YearWeekHelp.CurrentWeek(),
                Year = year ?? DateTime.Now.Year
            };

            return await Task.FromResult(WeekMenuDtoByWeekYear(dto));
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public Task<bool> IsNexWeekMenuExist(WeekYearDto weekyear)
        {
            WeekYearDto nextweeknumber = YearWeekHelp.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber.Week, nextweeknumber.Year);

            return Task.FromResult(nextWeek != null);
        }
        //получить 
        //[HttpGet]
        //[Route("nextWeekMenu")]
        //public async Task<WeekMenuDto> GetNextWeekMenu(WeekYearDto weekyear)
        //{
        //    WeekYearDto nextweeknumber = YearWeekHelp.GetNextWeekYear(weekyear);

        //    MenuForWeek nextWeek = _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber.Week, nextweeknumber.Year);

        //    WeekMenuDto weekmenudto = null;
        //    if (nextWeek != null)
        //    {
        //        weekmenudto = WeekMenuDto.MapDto(_unitOfWork, nextWeek);
        //    }

        //    return await Task.FromResult(weekmenudto);
        //}

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
            var cats = _unitOfWork.RepositoryAsync<DishType>();
            await cats.Queryable().LoadAsync();
                  string[] categories = await cats.Queryable()
                        .Select(dt => dt.Category)
                        .AsQueryable()
                        .ToArrayAsync();

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
            MenuForDay menuFd = _unitOfWork.RepositoryAsync<MenuForDay>().Find(menuforday.ID);
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

            _unitOfWork.RepositoryAsync<MenuForDay>().Update(menuFd);
            _unitOfWork.SaveChanges();


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

        //Запрашивает объект MenuForWeek из базы
        //Если объект не существует, проверяет является ли запрашиваемый объект меню на следующую неделю или на текущую неделю в системе
        //Если является то создаётся новое меню (пустое), сохраняет его в базе и отправляет его DTO клиенту
        private WeekMenuDto WeekMenuDtoByWeekYear(WeekYearDto weekyear)
        {
            MenuForWeek weekmenu = _weekmenuService.GetWeekMenuByWeekYear(weekyear.Week, weekyear.Year);
            if (weekmenu != null)
            {
                var dto = WeekMenuDto.MapDto(_unitOfWork, weekmenu);
                return dto;
            }
            if (YearWeekHelp.WeekIsCurrentOrNext(weekyear))
            {
                List<WorkingDay> workdays = new List<WorkingDay>();
                for (int i = 0; i < 7; i++)
                {
                    workdays.Add(new WorkingDay
                    {
                        DayOfWeek = _unitOfWork.RepositoryAsync<ACSDining.Core.Domains.DayOfWeek>().FindAsync(i + 1).Result,
                        IsWorking = i < 5
                    });
                }
                WorkingWeek workingWeek = new WorkingWeek
                {
                    WeekNumber = weekyear.Week,
                    Year = _unitOfWork.RepositoryAsync<Year>().Queryable().FirstOrDefault(y=>y.YearNumber==weekyear.Year),
                    WorkingDays = workdays
                };
                weekmenu = new MenuForWeek
                {
                    WorkingWeek = workingWeek,
                    MenuForDay = workdays.Select(wd => new MenuForDay
                    {
                        WorkingDay = wd,
                        WorkingWeek = workingWeek
                    }).ToList()

                };

                _weekmenuService.Insert(weekmenu);
                _unitOfWork.SaveChanges();

                weekmenu = _weekmenuService.GetWeekMenuByWeekYear(weekyear.Week, weekyear.Year);
            }
            return WeekMenuDto.MapDto(_unitOfWork, weekmenu, true);
        }
    }
}