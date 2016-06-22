using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.Repositories
{
    public static class RelationsRepository
    {
        public static List<DishQuantityRelations> GetRelationsListByDayOrdId(
            this IRepositoryAsync<DishQuantityRelations> repository, int dayorderid)
        {
            return repository.Query()
                .Include(dq => dq.DishQuantity)
                .Include(dq => dq.DishType)
                .Include(dq => dq.DayOrderMenu.MenuForDay)
                .Select()
                .Where(dqr => dqr.DayOrderMenuId == dayorderid)
                .OrderBy(dq=>dq.DishType.Id)
                .ToList();
        }
    }
}
