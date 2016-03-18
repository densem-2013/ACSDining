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
using ACSDining.Infrastructure.DTO.SuperUser;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        
        private readonly UnitOfWork _unitOfWork;
        private readonly IRepository<DayOfWeek> _dayRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;
        private readonly IRepository<MenuForDay> _daymenuRepository;
        private readonly IRepository<DishType> _dishtypeRepository;
        private readonly IRepository<Dish> _dishRepository;
        private readonly IRepository<WorkingWeek> _workingWeekRepository;
        private readonly IRepository<WorkingDay> _workingDayRepository; 

        public WeekMenuController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _dayRepository = _unitOfWork.Repository<DayOfWeek>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
            _daymenuRepository = _unitOfWork.Repository<MenuForDay>();
            _dishtypeRepository = _unitOfWork.Repository<DishType>();
            _dishRepository = _unitOfWork.Repository<Dish>();
            _workingWeekRepository = unitOfWork.Repository<WorkingWeek>();
            _workingDayRepository = unitOfWork.Repository<WorkingDay>();
        }



        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<WeekMenuDto> listDto = _weekmenuRepository.GetAll().Result
                .Select(x => _unitOfWork.MenuForWeekToDto(x)).ToList();
            var dto = listDto.FirstOrDefault(wm => wm.WeekNumber == numweek && wm.YearNumber == year);
            if (dto == null)
            {
                dto = listDto.FirstOrDefault();
                if (dto == null)
                {
                    return null;
                }
            }

            return dto;
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public  Task<bool> IsNexWeekMenuExist(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = UnitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuRepository.Find(
                        mfw => mfw.WorkingWeek.WeekNumber == nextweeknumber.Week && mfw.WorkingWeek.Year.YearNumber == nextweeknumber.Year).Result;

            return Task.FromResult(nextWeek != null);
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<WeekMenuDto> GetNextWeekMenu(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = UnitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuRepository.Find(
                        mfw => mfw.WorkingWeek.WeekNumber == nextweeknumber.Week && mfw.WorkingWeek.Year.YearNumber == nextweeknumber.Year).Result;
            WeekMenuDto dto;
            if (nextWeek != null)
            {
                dto = _unitOfWork.MenuForWeekToDto(nextWeek);
                return dto;
            }
            WorkingWeek workingWeek =
                _workingWeekRepository.Find(
                    w => w.WeekNumber == nextweeknumber.Week && w.Year.YearNumber == nextweeknumber.Year).Result;
            nextWeek = new MenuForWeek
            {
                WorkingWeek = workingWeek,
                //Year =
                //    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                //    new Year { YearNumber = nextweeknumber.Year},
                MenuForDay = _dayRepository.GetAll().Result.OrderBy(d => d.ID).Select(day => new MenuForDay
                {
                    WorkingWeek = workingWeek,
                    WorkingDay =
                        _workingDayRepository.Find(
                            wd => wd.WorkingWeek.ID == workingWeek.ID && wd.DayOfWeek.ID == day.ID).Result

                }).ToList()

            };

            dto = _unitOfWork.MenuForWeekToDto(nextWeek, true);
            return dto;
        }

        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetNextWeekYear([FromBody]WeekYearDTO weekyear)
        {
            return Task.FromResult(UnitOfWork.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetPrevWeekYear([FromBody]WeekYearDTO weekyear)
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
            string[] categories;
            try
            {
                categories = await Task.Run(()=>_dishtypeRepository.GetAll().Result.OrderBy(d => d.Id).Select(dt => dt.Category).ToArray());

            }
            catch (Exception)
            {
                    
                throw;
            }
            return categories;
        }

        [HttpGet]
        [Route("WeekNumbers")]
        public async Task<List<int>> GetWeekNumbers()
        {
            List<WeekMenuDto> listDto = null;
            await Task.Run(() =>
            {
                listDto = _weekmenuRepository.GetAll().Result
                    .Select(x => _unitOfWork.MenuForWeekToDto(x)).ToList();
            });
            List<int> years = listDto.Select(wm => wm.YearNumber).Distinct().ToList();
            years.Sort();
            List<int> numweeks = new List<int>();
            await Task.Run(() =>
            {
                foreach (int year in years)
                {
                    var yearweeks = listDto.Where(m => m.YearNumber == year).Select(wm => wm.WeekNumber).ToList();
                    yearweeks.Sort();
                    numweeks = numweeks.Concat(yearweeks).ToList();
                }
            });

            return numweeks;
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
                menuforday.Dishes.SelectMany(d => _dishRepository.GetAll().Result.Where(dish => dish.DishID == d.DishID)).ToList();

            MenuForDay menuFD = _daymenuRepository.Find(m => m.ID == menuforday.ID).Result;
            menuFD.Dishes = dishes;
            menuFD.TotalPrice = menuforday.TotalPrice;
            _daymenuRepository.Update(menuFD);

            MenuForWeek mfwModel = _weekmenuRepository.Find(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.ID)).Result;

            mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);
            _weekmenuRepository.Update(mfwModel);

            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<WeekMenuDto> CreateNextWeekMenu(WeekYearDTO weekyear)
        {

            WeekYearDTO nextweekDto = UnitOfWork.GetNextWeekYear(weekyear);

            int maxID = _daymenuRepository.GetAll().Result.Max(m => m.ID);
            WorkingWeek workingWeek =
                _workingWeekRepository.Find(
                    w => w.WeekNumber == nextweekDto.Week && w.Year.YearNumber == nextweekDto.Year).Result;
            MenuForWeek nextWeek = new MenuForWeek
            {
                WorkingWeek = workingWeek,
                //Year =
                //    _yearRepository.Find(y => y.YearNumber == DateTime.Now.Year) ??
                //    new Year { YearNumber = nextweekDto.Year },
                MenuForDay = _dayRepository.GetAll().Result.OrderBy(d => d.ID).Select(day => new MenuForDay
                {
                    ID = ++maxID,
                    WorkingDay =
                        _workingDayRepository.Find(
                            wd => wd.WorkingWeek.ID == workingWeek.ID && wd.DayOfWeek.ID == day.ID).Result,
                    WorkingWeek = workingWeek
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
            WeekMenuDto dto = _unitOfWork.MenuForWeekToDto(nextWeek, true);
            return dto;
        }

        // DELETE api/WeekMenu/5
        [HttpDelete]
        [Route("delete/{numweek}")]
        [ResponseType(typeof (MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int numweek)
        {
            MenuForWeek menuforweek = _weekmenuRepository.Find(mfw => mfw.WorkingWeek.WeekNumber == numweek).Result;
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