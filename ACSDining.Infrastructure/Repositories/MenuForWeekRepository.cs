using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.Repositories
{
    public static class MenuForWeekRepository
    {

        public static double[] GetUnitWeekPrices(this IRepositoryAsync<MenuForWeek> repository, int menuforweekid, string[] categories)
        {

            double[] unitprices = new double[20];

            MenuForWeek mfw = repository.Find(menuforweekid);
            for (int i = 0; i < 5; i++)
            {
                MenuForDay daymenu = mfw.MenuForDay.ElementAt(i);
                for (int j = 0; j < categories.Length; j++)
                {
                    unitprices[i*4 + j] = daymenu.Dishes.ElementAt(j).Price;
                }
            }
            return unitprices;
        }
        
        public static MenuForWeek GetWeekMenuByWeekYear(this IRepositoryAsync<MenuForWeek> repository, int numweek,
            int year)
        {
            MenuForWeek mfw =
                repository.Query()
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(wm => wm.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishDetail)))
                    .Include(wm => wm.Orders)
                    .Include(wm => wm.PlannedOrderMenus)
                    .Include(wm => wm.WorkingWeek.Year)
                    .Include(wm => wm.WorkingWeek.WorkingDays.Select(d=>d.DayOfWeek))
                    .Select()
                    .FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);

            return mfw;
        }

        public static List<int> GetWeekNumbers(this IRepositoryAsync<MenuForWeek> repository)
        {
            List<MenuForWeek> list = repository.Query().Include(mfw => mfw.WorkingWeek.Year).Select().ToList();
            List<int> years = list.Select(wm => wm.WorkingWeek.Year.YearNumber).Distinct().ToList();
            years.Sort();
            List<int> numweeks = new List<int>();

            foreach (int year in years)
            {
                var yearweeks =
                    list.Where(m => m.WorkingWeek.Year.YearNumber == year)
                        .Select(wm => wm.WorkingWeek.WeekNumber)
                        .ToList();
                yearweeks.Sort();
                numweeks = numweeks.Concat(yearweeks).ToList();
            }

            return repository.Query().Include(m=>m.WorkingWeek).Select(wm => wm.WorkingWeek.WeekNumber).Reverse().ToList();
        }

        public static double GetSummaryPrice(this IRepositoryAsync<MenuForWeek> repository, UserOrdersDto usorder, int numweek, int year)
        {
            MenuForWeek weekNeeded = repository.Queryable().FirstOrDefault(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year);
            double summary = 0;
            if (weekNeeded != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        summary += weekNeeded.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price *
                                   usorder.Dishquantities[4 * i + j];
                    }
                }
            }
            return summary;
        }

    }
}
