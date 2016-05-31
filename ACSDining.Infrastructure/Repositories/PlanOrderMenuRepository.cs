using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;

namespace ACSDining.Infrastructure.Repositories
{
    public static class PlanOrderMenuRepository
    {
        public static List<PlannedWeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<PlannedWeekOrderMenu> repository,
              WeekYearDto wyDto)
        {
            List<PlannedWeekOrderMenu> pagedOrders = repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.PlannedDayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.PlannedDayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.PlannedDayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year)
                .OrderBy(wp => wp.User.UserName)
                .ToList();
            return pagedOrders;
        }
    }
}
