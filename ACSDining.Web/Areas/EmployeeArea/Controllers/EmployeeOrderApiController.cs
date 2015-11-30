using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using ACSDining.Core.Domains;
using System.Web.Http.Description;
using System.Threading.Tasks;
using ACSDining.Core.DAL;
using ACSDining.Infrastructure.DAL;
using ACSDining.Web.Areas.SU_Area.Models;
using Microsoft.AspNet.Identity;
using ACSDining.Web.Areas.EmployeeArea.Models;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [Authorize(Roles = "Employee,Administrator")]
    [RoutePrefix("api/Employee")]
    public class EmployeeOrderApiController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<OrderMenu> _orderRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;

        public EmployeeOrderApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = _unitOfWork.Repository<OrderMenu>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
        }

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof(EmployeeOrderDTO))]
        public async Task<IHttpActionResult> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            string userid = RequestContext.Principal.Identity.GetUserId();
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            MenuForWeek mfw =
                _weekmenuRepository.Find(wm => wm.WeekNumber == numweek && wm.Year.YearNumber == year);

            if (mfw == null) return Content(HttpStatusCode.BadRequest, "not created");

            WeekMenuModel weekmodel=new WeekMenuModel(mfw);

            OrderMenu ordmenu = _orderRepository.Find(ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == mfw.ID);
            EmployeeOrderDTO model = new EmployeeOrderDTO
            {
                UserId = userid,
                MenuId = weekmodel.ID,
                SummaryPrice = ordmenu.SummaryPrice,
                WeekPaid = ordmenu.WeekPaid,
                MFD_models = weekmodel.MFD_models,
                Year = (int) year,
                WeekNumber = (int) numweek
            };
            if (ordmenu != null)
            {
                model.OrderId = ordmenu.Id;
                model.Dishquantities = _unitOfWork.GetUserWeekOrderDishes(ordmenu.Id);
            }
            return Ok(model);
        }


        [HttpGet]
        [Route("CurrentWeek")]
        [ResponseType(typeof(Int32))]
        public  async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(UnitOfWork.CurrentWeek());
        }

        [HttpGet]
        [Route("WeekNumbers")]
        [ResponseType(typeof(List<int>))]
        public async Task<IHttpActionResult> GetWeekNumbers()
        {
            List<int> numweeks = _weekmenuRepository.GetAll().Select(wm => wm.WeekNumber).Reverse().ToList();

            return Ok(numweeks);
        }
    }
}
