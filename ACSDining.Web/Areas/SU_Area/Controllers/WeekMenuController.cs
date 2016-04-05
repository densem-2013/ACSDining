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
        private readonly IMenuforDayService _daymenuRepository; 

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _dishService = new DishService( _unitOfWork.RepositoryAsync<Dish>());
            _daymenuRepository = new MenuForDayService(_unitOfWork.RepositoryAsync<MenuForDay>());
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

            var dto = _weekmenuService.WeekMenuDtoByWeekYear(week, yearnum);

            return await Task.FromResult(dto);
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
            return await Task.FromResult(_weekmenuService.GetNextWeekMenu(weekyear));
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
            MenuForDay menuFd = _daymenuRepository.Find(menuforday.ID);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (menuFd == null)
            {
                return NotFound();
            }
            List<Dish> dishes =
                menuforday.Dishes.SelectMany(d => _dishService.AllDish().Where(dish => dish.DishID == d.DishID)).ToList();

            menuFd.Dishes = dishes;
            menuFd.TotalPrice = menuforday.TotalPrice;

            _daymenuRepository.Update(menuFd);


            MenuForWeek mfwModel = _weekmenuService.GetAll().ToList().FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

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
            MenuForWeek nextWeek = _weekmenuService.CreateNextWeekMenu(weekyear);

            if (nextWeek == null)
            {
                return null;
            }


            try
            {
                _weekmenuService.Insert(nextWeek);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(_weekmenuService.MapWeekMenuDto(nextWeek, true));
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
    }
}