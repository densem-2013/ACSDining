using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.Repositories
{
    public static class RelationsRepository
    {
        public static List<DishQuantityRelations> GetRelationsListByDayIdMenuId(
            this IRepositoryAsync<DishQuantityRelations> repository, int dayorderid, int menufordayid)
        {
            return repository.Query()
                .Include(dq => dq.DishQuantity)
                //.Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
                .Include(dq => dq.DayOrderMenu.MenuForDay)
                .Select()
                .Where(dqr => /*dqr.MenuForDayId == menufordayid && */dqr.DayOrderMenuId == dayorderid)
                .ToList();
        }
    }
}
