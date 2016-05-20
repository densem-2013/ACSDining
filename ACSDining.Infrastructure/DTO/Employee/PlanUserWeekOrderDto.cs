﻿using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.Employee
{
    //Представляет информацию о плановой заявке пользователя на неделю
    public class PlanUserWeekOrderDto
    {
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public List<PlanUserDayOrderDto> PlanDayOrderDtos { get; set; }
        public double WeekSummaryPrice { get; set; }

        /// <param name="unitOfWork"></param>
        /// <param name="planWeekOrderMenu"></param>
        /// <param name="catLength">Количество категорий блюд</param>
        /// <returns></returns>
        public static PlanUserWeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, PlannedWeekOrderMenu planWeekOrderMenu,
            int catLength)
        {
            return new PlanUserWeekOrderDto
            {
                //UserId = planWeekOrderMenu.WeekOrderMenu.User.Id,
                //OrderId = planWeekOrderMenu.WeekOrderMenu.Id,
                //UserName = planWeekOrderMenu.WeekOrderMenu.User.UserName,
                //PlanDayOrderDtos =
                //    planWeekOrderMenu.PlannedDayOrderMenus.Select(
                //        plandord => PlanUserDayOrderDto.MapUserDayOrderDto(unitOfWork, plandord, catLength)).ToList(),
                //WeekSummaryPrice = planWeekOrderMenu.WeekOrderMenu.WeekOrderSummaryPrice
            };
        }
    }
}
