using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;

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

            List<DishQuantityRelations> quaList = unitOfWork.Repository<DishQuantityRelations>()
                .Query()
                .Include(dq => dq.DishQuantity)
                .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
                .Select()
                .Where(dqr => dqr.MenuForDayId == dayOrder.MenuForDay.ID && dqr.DayOrderMenuId == dayOrder.Id)
                .ToList();

            double[] paiments = new double[catLength];

            MenuForDay mfd = dayOrder.MenuForDay;

            for (int j = 1; j <= catLength; j++)
            {
                var firstOrDefault = quaList.FirstOrDefault(
                    q => q.MenuForDay.WorkingDay.DayOfWeek.ID == workday.Id && q.DishTypeId == j);
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
