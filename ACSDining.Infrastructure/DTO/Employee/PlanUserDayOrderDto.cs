using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.Employee
{
    public class PlanUserDayOrderDto
    {
        public int PlanDayOrderId { get; set; }
        public double[] DishQuantities { get; set; }
        public double DayOrderSummary { get; set; }

        /// <summary>
        /// Получает представление плановой заявки
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="planDayOrderMenu"></param>
        /// <param name="catlength"></param>
        /// <returns></returns>
        public static PlanUserDayOrderDto MapUserDayOrderDto(IUnitOfWorkAsync unitOfWork, PlannedDayOrderMenu planDayOrderMenu, int catlength)
        {
            //WorkingDay workday = planDayOrderMenu.DayOrderMenu.MenuForDay.WorkingDay;

            //List<DishQuantityRelations> quaList =
            //    unitOfWork.RepositoryAsync<DishQuantityRelations>()
            //        .GetRelationsListByDayIdMenuId(planDayOrderMenu.DayOrderMenu.MenuForDay.ID,
            //            planDayOrderMenu.DayOrderMenu.Id);

            //double[] dquantities = new double[catlength];

            //for (int j = 1; j <= catlength; j++)
            //{
            //    var firstOrDefault = quaList.FirstOrDefault(
            //        q => q.PlannedDayOrderMenu.DayOrderMenu.MenuForDay.WorkingDay.Id == workday.Id && q.DishTypeId == j
            //        );
            //    if (firstOrDefault != null)
            //        dquantities[j - 1] = firstOrDefault.DishQuantity.Quantity;
            //}

            return new PlanUserDayOrderDto
            {
                PlanDayOrderId = planDayOrderMenu.Id//,
                //DishQuantities = dquantities,
                //DayOrderSummary = planDayOrderMenu.DayOrderMenu.DayOrderSummaryPrice
            };
        }
    }
}
