using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Infrastructure.DAL;
using WebGrease.Css.Extensions;
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


        private IEnumerable<WeekMenuDto> WeekModels
        {
            get
            {
                IEnumerable<MenuForWeek> mfwList = _weekmenuRepository.GetAll();
                foreach (var item in mfwList)
                {
                    yield return _unitOfWork.MenuForWeekToDto(item);
                }
            }
        }

        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            WeekMenuDto dto = WeekModels.FirstOrDefault(wm => wm.WeekNumber == numweek && wm.YearNumber == year);
            if (dto == null)
            {
                dto = WeekModels.FirstOrDefault();
                if (dto == null)
                {
                    return NotFound();
                }
            }

            return Ok(dto);
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public  Task<bool> IsNexWeekMenuExist(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = _unitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuRepository.Find(
                        mfw => mfw.WeekNumber == nextweeknumber.Week && mfw.Year.YearNumber == nextweeknumber.Year);

            return Task.FromResult(nextWeek != null);
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<IHttpActionResult> GetNextWeekMenu(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = _unitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuRepository.Find(
                        mfw => mfw.WeekNumber == nextweeknumber.Week && mfw.Year.YearNumber == nextweeknumber.Year);
            WeekMenuDto dto;
            if (nextWeek != null)
            {
                dto = _unitOfWork.MenuForWeekToDto(nextWeek);
                return Ok(dto);
            }
            nextWeek = new MenuForWeek
            {
                WeekNumber = nextweeknumber.Week,
                Year =
                    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                    new Year { YearNumber = nextweeknumber.Year},
                    MenuForDay = _dayRepository.GetAll().OrderBy(d => d.ID).Select(day => new MenuForDay
                    {
                        DayOfWeek = day
                    }).ToList()

            };

            dto = _unitOfWork.MenuForWeekToDto(nextWeek, true);
            return Ok(dto);
        }

        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetNextWeekYear([FromBody]WeekYearDTO weekyear)
        {
            return Task.FromResult(_unitOfWork.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetPrevWeekYear([FromBody]WeekYearDTO weekyear)
        {
            return Task.FromResult(_unitOfWork.GetPrevWeekYear(weekyear));
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
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayDto menuforday)
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
        public async Task<IHttpActionResult> CreateNextWeekMenu(WeekYearDTO weekyear)
        {

            WeekYearDTO nextweekDto = _unitOfWork.GetNextWeekYear(weekyear);

            int maxID = _daymenuRepository.GetAll().Max(m => m.ID);
            MenuForWeek nextWeek = new MenuForWeek
            {
                WeekNumber = nextweekDto.Week,
                Year =
                    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                    new Year { YearNumber = nextweekDto.Year },
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
            var dto = _unitOfWork.MenuForWeekToDto(nextWeek, true);
            return Ok(dto);
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