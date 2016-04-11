using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Infrastructure.Repositories
{
    public static class OrderMenuRepository
    {
        //public static List<UserWeekOrderDto> GetUserWeekOrderDtos(this IRepositoryAsync<WeekOrderMenu> repository, int week, int yearnum)
        //{

        //    var cats = repository.GetRepository<DishType>();
        //    cats.Queryable().LoadAsync().RunSynchronously();
        //    int catlength = cats.Queryable()
        //          .Select(dt => dt.Category)
        //          .AsQueryable()
        //          .ToArrayAsync().Result.Length;

        //    List<WeekOrderMenu> orderList = repository.OrdersMenuByWeekYear(week, yearnum);
        //    List<UserWeekOrderDto> usersWeekOrderDtos=orderList.Select(uwo=>UserWeekOrderDto.MapDto())
        //    OrdersDto OrderDTO = new OrdersDto
        //    {
        //        WeekNumber = week,
        //        YearNumber = yearnum,
        //        UserWeekOrderDtos =  orderList.Select(om =>
        //        {
        //            List<DishQuantityRelations> quaList = repository.GetRepository<DishQuantityRelations>()
        //                .Query()
        //                .Include(dq => dq.DishQuantity)
        //                .Include(dq => dq.MenuForDay.WorkingDay.DayOfWeek)
        //                .Select()
        //                .Where(dqr => dqr.WeekOrderMenuId == om.Id && dqr.MenuForWeekId == om.MenuForWeek.ID)
        //                .ToList();
        //            MenuForWeek mfw = repository.GetRepository<MenuForWeek>().Find(om.MenuForWeek.ID);
        //            return new UserWeekOrderDto
        //            {
        //                UserId = om.User.Id,
        //                UserName = om.User.UserName,
        //                Dishquantities = repository.GetUserWeekOrderDishes(quaList, categories, mfw)
        //            };
        //        }).ToList()
        //    };
        //        return null;
        //}
        public static double[] GetUserWeekOrderDishes(this IRepositoryAsync<WeekOrderMenu> repository,
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
                            q => q.MenuForDay.WorkingDay.DayOfWeek.ID == i && q.DishTypeId == j
                            );
                        if (firstOrDefault != null)
                            dquantities[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity;
                    }
                }
            }
            return dquantities;
        }

        public static double[] GetUserWeekOrderPaiments(this IRepositoryAsync<WeekOrderMenu> repository,
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
                            q => q.MenuForDay.WorkingDay.DayOfWeek.ID == i && q.DishTypeId == j
                            );
                        if (firstOrDefault != null)
                            paiments[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity*
                                                          daymenu.Dishes.ElementAt(j - 1).Price;
                    }
                }
            }
            return paiments;
        }

        public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository, int week, int year)
        {
            return repository.Query()
                    .Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(om => om.User)
                    .Include(om => om.PlannedWeekOrderMenu)
                    .Include(om => om.MenuForWeek.WorkingWeek.Year)
                    .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == week && om.MenuForWeek.WorkingWeek.Year.YearNumber == year).ToList();
        }

    }
}
