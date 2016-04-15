using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Microsoft.AspNet.Identity.Owin;
using WebGrease.Css.Extensions;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IDishQuantityRelationsService _dishQuantityService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private ApplicationUserManager _userManager;

        public EmployeeOrderApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork) unitOfWorkAsync).GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            _dishQuantityService = new DishQuantityRelationsService(_unitOfWork.RepositoryAsync<DishQuantityRelations>());
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                _userManager = _userManager ??
                               HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return _userManager;
            }
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
                    await UserManager.FindByNameAsync(ControllerContext.RequestContext.Principal.Identity.GetUserName());
                MenuForWeek weekmenu = _weekMenuService.GetWeekMenuByWeekYear(wyDto);
                ordmenu = new WeekOrderMenu
                {
                    User = user,
                    MenuForWeek = weekmenu
                };

                PlannedWeekOrderMenu planmenu = new PlannedWeekOrderMenu
                {
                    WeekOrderMenu = ordmenu
                };
                _db.WeekOrderMenus.Add(ordmenu);

                await _unitOfWork.SaveChangesAsync();

                ordmenu = _orderMenuService.FindByUserIdWeekYear(user.Id, wyDto);

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
            WeekOrderMenu forUpdateOrder = _orderMenuService.Find(userWeekOrderDto.UserId);
            if (forUpdateOrder == null)
            {
                return NotFound();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            forUpdateOrder.DayOrderMenus.ForEach(x =>
            {
                foreach (UserDayOrderDto udoDto in userWeekOrderDto.DayOrderDtos.ToList())
                {
                    if (udoDto.DayOrderId == x.Id)
                    {
                        if (x.OrderCanBeChanged)
                        {
                            List<DishQuantityRelations> dqaList =
                                _dishQuantityService.GetByDayOrderMenuForDay(udoDto.DayOrderId, udoDto.MenuForDay.Id);

                            for (int j = 1; j <= catLength; j++)
                            {
                                //Находим связь, указывающую на текущее значение фактической дневной заявки на блюдо
                                var firstOrDefault = dqaList.FirstOrDefault(
                                    q => q.DayOrderMenuId == x.Id && q.DishTypeId == j);
                                if (firstOrDefault != null)
                                {
                                    double curQuantity = firstOrDefault.DishQuantity.Quantity;
                                    //если заказанное количество изменилось
                                    if (Math.Abs(curQuantity - udoDto.DishQuantities[j - 1]) > 0.001)
                                    {
                                        var dishQuantityRelations = _dishQuantityService.Query()
                                            .Include(dq => dq.DishQuantity)
                                            .Include(dq => dq.MenuForDay)
                                            .Include(dq => dq.DayOrderMenu)
                                            .Select()
                                            .FirstOrDefault(
                                                dq =>
                                                    Math.Abs(dq.DishQuantity.Quantity - udoDto.DishQuantities[j - 1]) <
                                                    0.001);
                                        if (dishQuantityRelations != null)
                                        {
                                            //переустанавливаем связь на найденную сущность, содержащую искомое количество
                                            firstOrDefault.DishQuantity = dishQuantityRelations.DishQuantity;
                                            _dishQuantityService.Update(firstOrDefault);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            int res = await _unitOfWork.SaveChangesAsync();

            return Ok(res);
        }

        /// <summary>
        /// Возвращает уведомление о созданном меню на следующую неделю
        /// Это уведомление позволяет или запрещает создание заказа на следующую неделю
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nextWeekMenuCanOrder")]
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
