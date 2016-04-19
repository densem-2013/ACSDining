using System;
using System.Collections.Generic;
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
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IDishService _dishService;
        private readonly IWorkDaysService _workDaysService;

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _db = _unitOfWork.GetContext();
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
            _workDaysService = new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
        }

        // GET api/WeekMenu
        [HttpPut]
        [Route("")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromBody] WeekYearDto wyDto=null)
        {

            if (wyDto==null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            MenuForWeek weekmenu = _weekmenuService.GetWeekMenuByWeekYear(wyDto);
            if (weekmenu != null)
            {
                return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork,weekmenu));
            }
            else
            {
                weekmenu = _weekmenuService.CreateByWeekYear(wyDto);
                _db.MenuForWeeks.Add(weekmenu);
                await _unitOfWork.SaveChangesAsync();

                weekmenu = _weekmenuService.GetWeekMenuByWeekYear(wyDto);

                return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork, weekmenu,true));
            }

        }

        [HttpGet]
        [Route("isnextweekmenuexists")]
        public Task<bool> IsNexWeekMenuExist()
        {
            WeekYearDto curWeekYear = YearWeekHelp.GetCurrentWeekYearDto();
            WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextWeekYearDto);

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

            string[] categories = MapHelper.GetCategoriesStrings(_unitOfWork);

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
                menuforday.Dishes.SelectMany(d => _dishService.AllDish().Where(dish => dish.DishID == d.DishId))
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

       
    }
}