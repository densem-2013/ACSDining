﻿using ACSDining.Core.Domains;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Web.Areas.SU_Area.Models;
using System.Collections.Generic;
using ACSDining.Core.DAL;
using ACSDining.Infrastructure.DAL;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<OrderMenu> _orderRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = _unitOfWork.Repository<OrderMenu>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
        }

        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (OrdersDTO))]
        public async Task<IHttpActionResult> GetMenuOrders([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            List<OrderMenu> orderMenus =_orderRepository.GetAll().Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToList();

            OrdersDTO model = new OrdersDTO()
            {
                WeekNumber = (int) numweek,
                UserOrders = orderMenus
                    .Select(order => new UserOrdesDTO()
                    {
                        UserId = order.User.Id,
                        UserName = order.User.UserName,
                        Dishquantities = _unitOfWork.GetUserWeekOrderDishes(order.Id),
                        WeekIsPaid = false,
                        SummaryPrice = order.SummaryPrice
                    }).OrderBy(uo => uo.UserName).ToList(),
                YearNumber = (int) year
            };

            return Ok(model);
        }

        [HttpPut]
        [Route("summary/{numweek}/{year}")]
        [ResponseType(typeof (double))]
        public async Task<double> GetSummaryPrice([FromBody] UserOrdesDTO usorder, [FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            MenuForWeek weekNeeded = _weekmenuRepository.Find(wm => wm.WeekNumber == numweek && wm.Year.YearNumber == year);
            double summary = 0;
            if (weekNeeded != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        summary += weekNeeded.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price*
                                   usorder.Dishquantities[4*i + j];
                    }
                }
            }
            return await Task.FromResult(summary);
        }
    }
}
