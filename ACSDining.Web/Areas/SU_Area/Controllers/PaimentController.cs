using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/Paiment")]
    public class PaimentController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<OrderMenu> _orderRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;

        public PaimentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = _unitOfWork.Repository<OrderMenu>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
        }

        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (PaimentsDTO))]
        public async Task<IHttpActionResult> GetWeekPaiments([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus =_orderRepository.GetAll().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == numweek && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
                        .ToList();
            MenuForWeek mfw = _weekmenuRepository.GetAll().FirstOrDefault(m => m.WorkingWeek.WeekNumber == numweek && m.WorkingWeek.Year.YearNumber == year);
            PaimentsDTO model = null;
            if (mfw == null || orderMenus == null)
            {
                return NotFound();
            }
            
            model = new PaimentsDTO
            {
                WeekNumber = (int) numweek,
                YearNumber = (int) year,
                UserPaiments = orderMenus
                    .Select(order => new UserPaimentDTO
                    {
                        UserId = order.User.Id,
                        OrderId = order.Id,
                        UserName = order.User.UserName,
                        Paiments = _unitOfWork.GetUserWeekOrderPaiments(order.Id),
                        SummaryPrice = order.SummaryPrice,
                        WeekPaid = order.WeekPaid,
                        Balance = order.Balance,
                        Note = order.Note
                    }).OrderBy(uo => uo.UserName).ToList(),
                UnitPrices = _unitOfWork.GetUnitWeekPrices(mfw.ID),
                UnitPricesTotal = PaimentsByDishes((int)numweek, (int)year)
            };

            return Ok(model);
        }

        private double[] PaimentsByDishes(int numweek, int year )
        {
            double[] paiments = new double[21];
            MenuForWeek weekmenu = _weekmenuRepository.GetAll().FirstOrDefault(m => m.WorkingWeek.WeekNumber == numweek && m.WorkingWeek.Year.YearNumber == year);
            double[] weekprices = _unitOfWork.GetUnitWeekPrices(weekmenu.ID);


            OrderMenu[] orderMenus = _orderRepository.GetAll().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == numweek && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
                        .ToArray();
            for (int i = 0; i < orderMenus.Length; i++)
            {
                double[] dishquantities = _unitOfWork.GetUserWeekOrderDishes(orderMenus[i].Id);
                for (int j = 0; j < 20; j++)
                {
                    paiments[j] += weekprices[j]*dishquantities[j];
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
            OrderMenu order =_orderRepository.Find(om=>om.Id==orderid);
            if (order == null)
            {
                return NotFound();
            }
            order.Balance += order.WeekPaid;
            order.WeekPaid = pai;
            order.Balance -= order.WeekPaid;

            _orderRepository.Update(order);

            return Ok(order.Balance);
        }
    }
}
