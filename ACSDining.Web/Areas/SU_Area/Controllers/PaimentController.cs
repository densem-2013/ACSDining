using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private readonly IMenuForWeekService _weekMenuService;
        private readonly IOrderMenuService _orderMenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public PaimentController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<OrderMenu>());
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

            List<OrderMenu> orderMenus = _orderMenuService.GetAllByWeekYear(week,yearnum).ToList();

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
                            .Query().Include(dq=>dq.DishQuantity).Select().Where(dqr => dqr.OrderMenuID == order.Id && dqr.MenuForWeekID == menuforweekid)
                            .ToList();
                        return new UserPaimentDto
                        {
                            UserId = order.User.Id,
                            OrderId = order.Id,
                            UserName = order.User.UserName,
                            Paiments = _orderMenuService.UserWeekOrderPaiments(quaList, categories, weekmenu),
                            SummaryPrice = order.OrderSummaryPrice,
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

                OrderMenu[] orderMenus = _orderMenuService.GetAllByWeekYear(numweek,year).ToArray();

                for (int i = 0; i < orderMenus.Length; i++)
                {
                    OrderMenu order = orderMenus[i];
                    MenuForWeek mfw = order.MenuForWeek;
                    int menuforweekid = mfw.ID;
                    int ordid = order.Id;
                    List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                            .Query().Include(dq => dq.DishQuantity).Select()
                            .Where(dqr => dqr.OrderMenuID == ordid && dqr.MenuForWeekID == menuforweekid)
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
            OrderMenu order = _orderMenuService.Find(orderid);
            if (order == null)
            {
                return NotFound();
            }
            order.Balance += order.WeekPaid;
            order.WeekPaid = pai;
            order.Balance -= order.WeekPaid;

            _orderMenuService.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Ok(order.Balance);
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
