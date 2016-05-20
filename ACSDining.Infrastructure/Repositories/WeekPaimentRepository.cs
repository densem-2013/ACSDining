using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;

namespace ACSDining.Infrastructure.Repositories
{
    public static class WeekPaimentRepository
    {
        public static WeekPaiment WeekPaimentByUseridWeekYear(this IRepositoryAsync<WeekPaiment> repository,  string userid, WeekYearDto wyDto)
        {
            return repository.Query()
                .Include(wp => wp.WeekOrderMenu.User)
                .Include(wp => wp.WeekOrderMenu.MenuForWeek.WorkingWeek.Year)
                .Include(wp => wp.WeekOrderMenu.MenuForWeek.WorkingWeek.WorkingDays.Select(wd => wd.DayOfWeek))
                .Include(wpai => wpai.WeekOrderMenu.MenuForWeek.MenuForDay.SelectMany(mfd => mfd.DishPriceMfdRelations.Select(dpr=>dpr.Dish.DishType)))
                .Select()
                .FirstOrDefault(
                    wp =>
                        string.Equals(wp.WeekOrderMenu.User.Id, userid) &&
                        wp.WeekOrderMenu.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        wp.WeekOrderMenu.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year);
        }

        public static List<WeekPaiment> WeekPaiments(this IRepositoryAsync<WeekPaiment> repository, WeekYearDto wyDto)
        {
             return repository.Query()
                .Include(wp => wp.WeekOrderMenu.User)
                .Include(wp => wp.WeekOrderMenu.MenuForWeek.WorkingWeek.Year)
                .Include(wp => wp.WeekOrderMenu.MenuForWeek.WorkingWeek.WorkingDays.Select(wd => wd.DayOfWeek))
                .Include(wpai => wpai.WeekOrderMenu.MenuForWeek.MenuForDay.SelectMany(mfd => mfd.DishPriceMfdRelations.Select(dpr => dpr.Dish.DishType)))
                .Select().Where(
                    wp =>
                        wp.WeekOrderMenu.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        wp.WeekOrderMenu.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year)
                .OrderBy(wp=>wp.WeekOrderMenu.User.UserName)
                .ToList();
        }
    }
}
