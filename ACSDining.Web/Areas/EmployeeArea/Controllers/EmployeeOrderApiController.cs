using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using Microsoft.AspNet.Identity;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [EmplSessionExpireFilter]
    [Authorize(Roles = "Employee")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IWeekPaimentService _weekPaimentService;

        public EmployeeOrderApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork) unitOfWorkAsync).GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _weekPaimentService=new WeekPaimentService(_unitOfWork.RepositoryAsync<WeekPaiment>());
        }
        
        /// <summary>
        /// Получить представление фактической заявки  запрашивающего пользователя за заданную неделю в году
        /// </summary>
        /// <param name="wyDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof(EmployeeWeekOrderDto))]
        public async Task<IHttpActionResult> GetUserWeekOrderDto([FromBody] WeekYearDto wyDto)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            WeekPaiment weekPaiment = _weekPaimentService.GetByUseridWeekYear(userid, wyDto);

            EmployeeWeekOrderDto model = null;

            if (weekPaiment != null)
            {
                model = EmployeeWeekOrderDto.MapDto(_db, weekPaiment, wyDto);
                //return Content(HttpStatusCode.BadRequest,
                //    string.Format(" menu on week {0} year {1} not created", wyDto.Week, wyDto.Year));
            }


            return Ok(model);
        }


        [HttpGet]
        [Route("curWeekYear")]
        [ResponseType(typeof (WeekYearDto))]
        public async Task<WeekYearDto> CurrentWeekNumber()
        {
            return await Task.FromResult(YearWeekHelp.GetCurrentWeekYearDto());
        }

        /// <summary>
        /// Обновляем пользовательскую заявку на неделю
        /// </summary>
        /// <param name="userOrderDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [ResponseType(typeof (bool))]
        public async Task<IHttpActionResult> UpdateUserWeekOrder([FromBody] UpdateUserOrderDto userOrderDto)
        {
            if (userOrderDto == null)
            {
                return BadRequest("Bad Request Object");
            }

            _unitOfWork.GetContext().UpdateDishQuantity(userOrderDto);

            return Ok(true);

        }

        /// <summary>
        /// Возвращает уведомление о созданном меню на следующую неделю
        /// Это уведомление позволяет или запрещает создание заказа на следующую неделю
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("canCreateOrderOnNextWeek")]
        [ResponseType(typeof (bool))]
        public async Task<IHttpActionResult> CanCreateOrderOnNextWeek()
        {
            WeekYearDto curWeekYearDto = YearWeekHelp.GetCurrentWeekYearDto();
            WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYearDto);
            MenuForWeek nextWeekMenu = _weekMenuService.GetWeekMenuByWeekYear(nextWeekYearDto);

            return Ok(nextWeekMenu != null && nextWeekMenu.OrderCanBeCreated);
        }

        /// <summary>
        /// Возвращает уведомление о созданном меню на следующую неделю
        /// Это уведомление позволяет или запрещает создание заказа на следующую неделю
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("isNextWeekYear")]
        [ResponseType(typeof (bool))]
        public async Task<IHttpActionResult> IsNextWeek([FromBody] WeekYearDto wyDto)
        {
            WeekYearDto curWeekYearDto = YearWeekHelp.GetCurrentWeekYearDto();
            WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYearDto);

            return Ok(wyDto.Week == nextWeekYearDto.Week && wyDto.Year == nextWeekYearDto.Year);
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("isCurWeekYear")]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> IsCurWeek([FromBody] WeekYearDto wyDto)
        {
            WeekYearDto curWeekYearDto = YearWeekHelp.GetCurrentWeekYearDto();
           // WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYearDto);

            return Ok(wyDto.Week == curWeekYearDto.Week && wyDto.Year == curWeekYearDto.Year);
        }
        /// <summary>
        /// Возвращает уведомление о созданном заказе на следующую неделю
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nextWeekOrderExists")]
        [ResponseType(typeof (bool))]
        public async Task<IHttpActionResult> NextWeekOrderExists()
        {
            WeekYearDto curWeekYearDto = YearWeekHelp.GetCurrentWeekYearDto();
            WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYearDto);

            string userid = RequestContext.Principal.Identity.GetUserId();

            WeekOrderMenu wom = _orderMenuService.FindByUserIdWeekYear(userid, nextWeekYearDto);

            return Ok(wom != null);
        }

        [HttpGet]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDto))]
        public Task<WeekYearDto> GetNextWeekYear()
        {
            return Task.FromResult(YearWeekHelp.GetNextWeekYear(YearWeekHelp.GetCurrentWeekYearDto()));
        }
    }
}
