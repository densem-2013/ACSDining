using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class PlanUserDayOrderDto
    {
        public int PlanDayOrderId { get; set; }
        public WorkDayDto WorkDayDto { get; set; }
        public double[] DishQuantities { get; set; }
        public double DayOrderSummary { get; set; }

        /// <summary>
        /// Получает представление плановой заявки
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="planDayOrderMenu"></param>
        /// <param name="quaList"></param>
        /// <param name="catlength"></param>
        /// <returns></returns>
        public static PlanUserDayOrderDto MapUserDayOrderDto(IUnitOfWorkAsync unitOfWork, PlannedDayOrderMenu planDayOrderMenu, int catlength)
        {
            WorkingDay workday = planDayOrderMenu.DayOrderMenu.MenuForDay.WorkingDay;

            List<DishQuantityRelations> quaList = unitOfWork.Repository<DishQuantityRelations>()
                .Query()
                .Include(dq => dq.DishQuantity)
                .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
                .Select()
                .Where(dqr => dqr.MenuForDayId == planDayOrderMenu.DayOrderMenu.MenuForDay.ID && dqr.DayOrderMenuId == planDayOrderMenu.DayOrderMenu.Id)
                .ToList();

            double[] dquantities = new double[catlength];

            for (int j = 1; j <= catlength; j++)
            {
                var firstOrDefault = quaList.FirstOrDefault(
                    q => q.PlannedDayOrderMenu.DayOrderMenu.MenuForDay.WorkingDay.Id == workday.Id && q.DishTypeId == j
                    );
                if (firstOrDefault != null)
                    dquantities[j - 1] = firstOrDefault.DishQuantity.Quantity;
            }
            return new PlanUserDayOrderDto
            {
                PlanDayOrderId = planDayOrderMenu.Id,
                WorkDayDto = WorkDayDto.MapDto(workday),
                DishQuantities = dquantities,
                DayOrderSummary = planDayOrderMenu.DayOrderMenu.DayOrderSummaryPrice
            };
        }
    }
}
