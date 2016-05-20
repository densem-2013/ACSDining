﻿using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.Employee
{
    //Представляет информацию о фактической заявке пользователя на неделю
    public class UserWeekOrderDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int OrderId { get; set; }
        public int[] DayOrdIdArray { get; set; }
        public double[] UserWeekOrderDishes { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="weekOrderMenu"></param>
        /// <returns></returns>
        public static UserWeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, WeekOrderMenu weekOrderMenu)
        {
            return new UserWeekOrderDto
            {
                UserId = weekOrderMenu.User.Id,
                UserName = string.Format("{0} {1}", weekOrderMenu.User.LastName, weekOrderMenu.User.FirstName),
                OrderId = weekOrderMenu.Id,
                DayOrdIdArray = weekOrderMenu.DayOrderMenus.Where(dord => dord.MenuForDay.WorkingDay.IsWorking).Select(dord=>dord.Id).ToArray(),
                UserWeekOrderDishes = unitOfWork.GetContext().FactDishQuantByWeekOrderId(weekOrderMenu.Id).Result
            };
        }
    }
}
