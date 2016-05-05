using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ApplicationUserManager UserManager;

        public PaimentController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork)unitOfWorkAsync).GetContext();
            UserManager = new ApplicationUserManager(new UserStore<User>(_db)); 
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }

        /// <summary>
        /// Получить все оплаты за неделю
        /// </summary>
        /// <param name="wyDto">Объект, инкапсулирующий запрашиваемую неделю в году</param>
        /// <returns></returns>
        [HttpPut]
        [Route("weekPaiments")]
        [ResponseType(typeof (List<UserWeekPaimentDto>))]
        public async Task<List<UserWeekPaimentDto>> GetWeekPaiments([FromBody] WeekYearDto wyDto)
        {
            if (wyDto==null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            List<WeekOrderMenu> orderMenus = _orderMenuService.GetOrderMenuByWeekYear(wyDto).ToList();

            MenuForWeek mfw = _weekMenuService.GetWeekMenuByWeekYear(wyDto);
            if (mfw == null)
            {
                return null;
            }
            List<UserWeekPaimentDto> userWeekPaiments =
                orderMenus.Select(om => UserWeekPaimentDto.MapDto(_unitOfWork, om, catLength)).ToList();

            return await Task.FromResult(userWeekPaiments);
        }

        /// <summary>
        /// Получает массив, содержащий суммы заказов по каждому блюду на каждый рабочий день
        /// </summary>
        /// <param name="wyDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("paimentsByDish")]
        [ResponseType(typeof(double[]))]
        public async Task<double[]> PaimentsByDishes([FromBody]WeekYearDto wyDto)
        {
            MenuForWeek weekmenu = _weekMenuService.GetWeekMenuByWeekYear(wyDto);
            if (weekmenu == null)
            {
                return null;
            }
            int workDayCount = weekmenu.WorkingWeek.WorkingDays.Count;
            int catLength = MapHelper.GetDishCategoriesCount(_unitOfWork);

            //Выделяем память для искомых данных ( +1 для хранения суммы всех ожидаемых проплат)
            double[] paiments = new double[workDayCount*catLength + 1];

            WeekOrderMenu[] weekOrderMenus = _orderMenuService.GetOrderMenuByWeekYear(wyDto).ToArray();

            List<UserWeekPaimentDto> userWeekPaiments =
                weekOrderMenus.Select(om => UserWeekPaimentDto.MapDto(_unitOfWork, om, catLength)).ToList();

            {

                for (int i = 0; i < userWeekPaiments.Count; i++)
                {
                    UserWeekPaimentDto uwp = userWeekPaiments[i];

                    double[] userweekpaiments = uwp.UserDayPaiments.SelectMany(udp=>udp.Paiments).ToArray();
                    for (int j = 0; j < workDayCount*catLength; j++)
                    {
                        paiments[j] += userweekpaiments[j];
                    }
                }
            }
            paiments[workDayCount*catLength + 1] = paiments.Sum();

            return paiments;
        }

        [HttpPut]
        [Route("updatePaiment/{orderid}")]
        [ResponseType(typeof(double))]
        public async Task<IHttpActionResult> UpdatePaiment( int orderid, double pai)
        {
            WeekOrderMenu weekOrder = _db.WeekOrderMenus.Find(orderid);
            if (weekOrder == null)
            {
                return NotFound();
            }
            weekOrder.User.Balance += weekOrder.WeekPaid;
            weekOrder.WeekPaid = pai;
            weekOrder.User.Balance -= weekOrder.WeekPaid;

            _db.Entry(weekOrder).State=EntityState.Modified;
            _db.Entry(weekOrder.User).State = EntityState.Modified;
            _db.WeekOrderMenus.Attach(weekOrder);
           // _db.WeekOrderMenus.Add(weekOrder);

            await _unitOfWork.SaveChangesAsync();

            return Ok(weekOrder.User.Balance);
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
