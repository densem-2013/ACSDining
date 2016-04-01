using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.Infrastructure;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private readonly IMenuForWeekService _weekmenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IDishService _dishService;
        private readonly IMenuforDayService _daymenuRepository; 

        public WeekMenuController(IUnitOfWorkAsync unitOfWork, IMenuForWeekService weekmenuService,
            IDishService dishService, IMenuforDayService daymenuRepository)
        {
            //IRepositoryAsync<MenuForWeek> _menuRepositoryAsync = unitOfWork.RepositoryAsync<MenuForWeek>();
            //_weekmenuService = new MenuForWeekService(_menuRepositoryAsync);
            _unitOfWork = unitOfWork;
            _weekmenuService = weekmenuService;
            _dishService = dishService;
            _daymenuRepository = daymenuRepository;
        }



        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var dto = _weekmenuService.WeekMenuDtoByWeekYear(week, yearnum);

            return await Task.FromResult(dto);
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public Task<bool> IsNexWeekMenuExist(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = UnitOfWork.GetNextWeekYear(weekyear);
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
            return Task.FromResult(UnitOfWork.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof (WeekYearDTO))]
        public Task<WeekYearDTO> GetPrevWeekYear([FromBody] WeekYearDTO weekyear)
        {
            return Task.FromResult(UnitOfWork.GetPrevWeekYear(weekyear));
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
            return await _weekmenuService.GetCategories().ToArrayAsync();
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

            menuFd.ObjectState = ObjectState.Modified;
            _daymenuRepository.Update(menuFd);


            MenuForWeek mfwModel = _weekmenuService.GetAll().ToList().FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID));

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);

            mfwModel.ObjectState = ObjectState.Modified;
            _weekmenuService.Update(mfwModel);

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

            nextWeek.ObjectState = ObjectState.Added;

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

            mfw.ObjectState = ObjectState.Deleted;
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