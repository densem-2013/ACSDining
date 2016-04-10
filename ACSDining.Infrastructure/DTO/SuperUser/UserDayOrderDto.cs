﻿using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    //Заказ пользователя на день
    public class UserDayOrderDto
    {
        public WorkDayDto WorkDayDto { get; set; }
        public double[] DishQuantities { get; set; }
        public double DayOrderSummary { get; set; }

        /// <summary>
        /// Получает представление фактической заявки
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="dayOrderMenu"></param>
        /// <param name="catlength"></param>
        /// <returns></returns>
        public static UserDayOrderDto MapUserDayOrderDto(IUnitOfWorkAsync unitOfWork, DayOrderMenu dayOrderMenu, int catlength)
        {
            WorkingDay workday = dayOrderMenu.MenuForDay.WorkingDay;

            List<DishQuantityRelations> quaList = unitOfWork.Repository<DishQuantityRelations>()
                .Query()
                .Include(dq => dq.DishQuantity)
                .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
                .Select()
                .Where(dqr => dqr.MenuForDayId == dayOrderMenu.MenuForDay.ID && dqr.DayOrderMenuId == dayOrderMenu.Id)
                .ToList();

            double[] dquantities = new double[catlength];


            for (int j = 1; j <= catlength; j++)
            {
                var firstOrDefault = quaList.FirstOrDefault(
                    q => q.MenuForDay.WorkingDay.DayOfWeek.ID == workday.Id && q.DishTypeId == j);
                if (firstOrDefault != null)
                    dquantities[j - 1] = firstOrDefault.DishQuantity.Quantity;
            }
            return new UserDayOrderDto
            {
                WorkDayDto = WorkDayDto.MapDto(workday),
                DishQuantities = dquantities,
                DayOrderSummary = dayOrderMenu.DayOrderSummaryPrice
            };
        }

    }
}