using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;

namespace ACSDining.Repository.Repositories
{
    public static class OrderMenuRepository
    {

        public static double[] GetUserWeekOrderDishes(this IRepositoryAsync<OrderMenu> repository,
            List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {

            double[] dquantities = new double[20];

            for (int i = 1; i <= 7; i++)
            {
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.ID == i);
                if (workday != null && workday.IsWorking)
                {
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.WorkDay.DayOfWeek.ID == i && q.DishTypeID == j
                            );
                        if (firstOrDefault != null)
                            dquantities[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity;
                    }
                }
            }
            return dquantities;
        }

        public static double[] GetUserWeekOrderPaiments(this IRepositoryAsync<OrderMenu> repository,
            List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {

            double[] paiments = new double[20];
            for (int i = 1; i <= 7; i++)
            {
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.ID == i);
                if (workday != null && workday.IsWorking)
                {
                    MenuForDay daymenu = mfw.MenuForDay.ElementAt(i - 1);
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.WorkDay.DayOfWeek.ID == i && q.DishTypeID == j
                            );
                        if (firstOrDefault != null)
                            paiments[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity*
                                                          daymenu.Dishes.ElementAt(j - 1).Price;
                    }
                }
            }
            return paiments;
        }

        public static List<OrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<OrderMenu> repository, int week, int year)
        {
            return repository.Query()
                    .Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(om => om.User)
                    .Include(om => om.PlannedOrderMenu)
                    .Include(om => om.MenuForWeek.WorkingWeek.Year)
                    .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == week && om.MenuForWeek.WorkingWeek.Year.YearNumber == year).ToList();
        }

    }
}
