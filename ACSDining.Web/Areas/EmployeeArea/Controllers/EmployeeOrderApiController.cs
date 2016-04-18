﻿using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Web;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee,SuperUser")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IDishQuantityRelationsService _dishQuantityService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ApplicationUserManager _userManager;

        public EmployeeOrderApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork) unitOfWorkAsync).GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _dishQuantityService = new DishQuantityRelationsService(_unitOfWork.RepositoryAsync<DishQuantityRelations>());
            _userManager = new ApplicationUserManager(new UserStore<User>(_db));
        }
        
        /// <summary>
        /// Получить представление фактической заявки  запрашивающего пользователя за заданную неделю в году
        /// </summary>
        /// <param name="wyDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        [ResponseType(typeof (UserWeekOrderDto))]
        public async Task<IHttpActionResult> GetUserWeekOrderDto([FromBody] WeekYearDto wyDto)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }
            WeekMenuDto weekmodel = WeekMenuDto.MapDto(_unitOfWork as UnitOfWork,
                _weekMenuService.GetWeekMenuByWeekYear(wyDto));

            //Меню на запрашиваемую неделю не было создано
            if (weekmodel == null)
            {
                return Content(HttpStatusCode.BadRequest,
                    string.Format(" menu on week {0} year {1} not created", wyDto.Week, wyDto.Year));
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            UserWeekOrderDto model = null;

            WeekOrderMenu ordmenu = _orderMenuService.FindByUserIdWeekYear(userid, wyDto);

            if (ordmenu != null)
            {
                model = UserWeekOrderDto.MapDto(_unitOfWork, ordmenu, catLength);

            }
            else
            {
                User user =
                    await _userManager.FindByNameAsync(ControllerContext.RequestContext.Principal.Identity.GetUserName());

                if (!YearWeekHelp.WeekIsCurrentOrNext(wyDto))
                {
                    return Content(HttpStatusCode.BadRequest,
                    string.Format(" order on week {0} year {1} not can be created", wyDto.Week, wyDto.Year));
                }
                ordmenu = _orderMenuService.CreateNew(user, wyDto);

                model = UserWeekOrderDto.MapDto(_unitOfWork, ordmenu, catLength);
            }

            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeekYear")]
        [ResponseType(typeof (WeekYearDto))]
        public async Task<WeekYearDto> CurrentWeekNumber()
        {
            return await Task.FromResult(YearWeekHelp.GetCurrentWeekYearDto());
        }

        /// <summary>
        /// Обновляем пользовательскую заявку на неделю
        /// </summary>
        /// <param name="userWeekOrderDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [ResponseType(typeof (int))]
        public async Task<IHttpActionResult> UpdateUserWeekOrder([FromBody] UserWeekOrderDto userWeekOrderDto)
        {
            if (userWeekOrderDto == null)
            {
                return BadRequest("Bad Request Object");
            }
            WeekOrderMenu forUpdateOrder = _orderMenuService.Find(userWeekOrderDto.OrderId);
            if (forUpdateOrder == null)
            {
                return NotFound();
            }


            int res = await Task.FromResult(_orderMenuService.UpdateUserWeekOrder(_unitOfWork,userWeekOrderDto));

            return Ok(res);
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

    }
}
