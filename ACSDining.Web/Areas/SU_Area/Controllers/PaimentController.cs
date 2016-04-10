using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public PaimentController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _db = ((UnitOfWork)unitOfWorkAsync).GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }

        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (PaimentsDto))]
        public async Task<IHttpActionResult> GetWeekPaiments([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var cats = _unitOfWork.RepositoryAsync<DishType>();
            await cats.Queryable().LoadAsync();
            string[] categories = await cats.Queryable()
                  .Select(dt => dt.Category)
                  .AsQueryable()
                  .ToArrayAsync();

            List<WeekOrderMenu> orderMenus = _orderMenuService.GetAllByWeekYear(week,yearnum).ToList();

            MenuForWeek mfw = _weekMenuService.GetWeekMenuByWeekYear(week, yearnum);
            if (mfw == null)
            {
                return NotFound();
            }

            PaimentsDto model = new PaimentsDto
            {
                WeekNumber = week,
                YearNumber = yearnum,
                UserPaiments = orderMenus
                    .Select(order =>
                    {
                        int menuforweekid = order.MenuForWeek.ID;
                        MenuForWeek weekmenu = order.MenuForWeek;
                        List<DishQuantityRelations> quaList = _unitOfWork.Repository<DishQuantityRelations>()
                            .Query().Include(dq=>dq.DishQuantity).Select().Where(dqr => dqr.OrderMenuId == order.Id && dqr.MenuForWeekId == menuforweekid)
                            .ToList();
                        return new UserPaimentDto
                        {
                            UserId = order.User.Id,
                            OrderId = order.Id,
                            UserName = order.User.UserName,
                            Paiments = _orderMenuService.UserWeekOrderPaiments(quaList, categories, weekmenu),
                            SummaryPrice = order.WeekOrderSummaryPrice,
                            WeekPaid = order.WeekPaid,
                            Balance = order.Balance,
                            Note = order.Note
                        };
                    }).OrderBy(uo => uo.UserName).ToList(),
                UnitPrices = _weekMenuService.UnitWeekPrices(mfw.ID, categories),
                UnitPricesTotal = PaimentsByDishes(week, yearnum)
            };

            return Ok(model);
        }

        private double[] PaimentsByDishes(int numweek, int year )
        {
            double[] paiments = new double[21];
            MenuForWeek weekmenu = _weekMenuService.GetWeekMenuByWeekYear(numweek,year);

            var cats = _unitOfWork.RepositoryAsync<DishType>();
            cats.Queryable().LoadAsync().RunSynchronously();
            string[] categories = cats.Queryable()
                  .Select(dt => dt.Category)
                  .AsQueryable()
                  .ToArrayAsync().Result;

            if (weekmenu != null)
            {
                double[] weekprices = _weekMenuService.UnitWeekPrices(weekmenu.ID, categories);

                WeekOrderMenu[] weekOrderMenus = _orderMenuService.GetAllByWeekYear(numweek,year).ToArray();

                for (int i = 0; i < weekOrderMenus.Length; i++)
                {
                    WeekOrderMenu weekOrder = weekOrderMenus[i];
                    MenuForWeek mfw = weekOrder.MenuForWeek;
                    int menuforweekid = mfw.ID;
                    int ordid = weekOrder.Id;
                    List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                            .Query().Include(dq => dq.DishQuantity).Select()
                            .Where(dqr => dqr.OrderMenuId == ordid && dqr.MenuForWeekId == menuforweekid)
                            .ToList();
                    double[] dishquantities = _orderMenuService.UserWeekOrderDishes( quaList, categories, mfw);
                    for (int j = 0; j < 20; j++)
                    {
                        paiments[j] += weekprices[j]*dishquantities[j];
                    }
                }
            }
            paiments[20] = paiments.Sum();
            return paiments;
        }

        [HttpPut]
        [Route("updatePaiment/{orderid}")]
        [ResponseType(typeof(double))]
        public async Task<IHttpActionResult> UpdatePaiment( int orderid, double pai)
        {
            WeekOrderMenu weekOrder = _orderMenuService.Find(orderid);
            if (weekOrder == null)
            {
                return NotFound();
            }
            weekOrder.Balance += weekOrder.WeekPaid;
            weekOrder.WeekPaid = pai;
            weekOrder.Balance -= weekOrder.WeekPaid;

            _db.WeekOrderMenus.Remove(weekOrder);
            _db.WeekOrderMenus.Add(weekOrder);

            //_orderMenuService.Update(weekOrder);
            await _unitOfWork.SaveChangesAsync();

            return Ok(weekOrder.Balance);
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
