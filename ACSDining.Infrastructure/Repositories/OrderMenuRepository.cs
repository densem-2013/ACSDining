using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;

namespace ACSDining.Infrastructure.Repositories
{
    public static class OrderMenuRepository
    {
      public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekYearDto wyDto)
        {
            List<WeekOrderMenu> pagedOrders = repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year)
                .OrderBy(wp => wp.User.LastName)
                .ToList();
            
            return pagedOrders;
        }

        public static WeekOrderMenu FindOrderMenuById(this IRepositoryAsync<WeekOrderMenu> repository, int orderid)
        {
            return repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.WorkingDay.DayOfWeek))
                .Select()
                .FirstOrDefault(om => om.Id == orderid);
        }


        public static List<User> GetUsersMadeOrder(this IRepositoryAsync<WeekOrderMenu> repository, int daymenuid)
        {
            List<WeekOrderMenu> whohasweekorder = repository.Query()
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om => om.DayOrderMenus.Select(dom => dom.MenuForDay.ID).Contains(daymenuid)).ToList();

            return whohasweekorder.Select(wom => wom.User).Distinct().ToList();
        }

        public static int GetCountByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository, WeekYearDto wyDto)
        {
            return repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year).Select().Count(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year);
        }
    }
}
