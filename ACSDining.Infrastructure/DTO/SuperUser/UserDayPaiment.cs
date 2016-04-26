using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class UserDayPaiment
    {
        public int DayOrderId { get; set; }
        public double[] Paiments { get; set; }
        public double SummaryDayPaiment { get; set; }

        public static UserDayPaiment MapDto(IUnitOfWorkAsync unitOfWork, DayOrderMenu dayOrder, int catLength)
        {
            WorkingDay workday = dayOrder.MenuForDay.WorkingDay;

            List<DishQuantityRelations> quaList =
                unitOfWork.RepositoryAsync<DishQuantityRelations>()
                    .GetRelationsListByDayIdMenuId(dayOrder.Id, dayOrder.MenuForDay.ID);

            double[] paiments = new double[catLength];

            MenuForDay mfd = dayOrder.MenuForDay;

            for (int j = 1; j <= catLength; j++)
            {
                var firstOrDefault = quaList.FirstOrDefault(
                    q => q.MenuForDay.WorkingDay.DayOfWeek.Id == workday.Id && q.DishTypeId == j);
                if (firstOrDefault != null)
                    paiments[j - 1] = firstOrDefault.DishQuantity.Quantity*mfd.Dishes.ElementAt(j - 1).Price;
            }

            return new UserDayPaiment
            {
                DayOrderId = dayOrder.Id,
                Paiments = paiments,
                SummaryDayPaiment = paiments.Sum()
            };
        }
    }
}
