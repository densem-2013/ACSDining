using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Identity;
using UserDayOrderDto = ACSDining.Infrastructure.DTO.Employee.UserDayOrderDto;

namespace ACSDining.Infrastructure.Repositories
{
    public static class OrderMenuRepository
    {
        //public static double[] SummaryDishesQuantities(this IRepositoryAsync<WeekOrderMenu> repository,
        //    WeekYearDto wyDto, int catLenth)
        //{
        //    List<WeekOrderMenu> weekOrderMenus = repository.OrdersMenuByWeekYear(wyDto);
        //    WorkingWeek workingWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(wyDto);
        //    int dayCount = workingWeek.WorkingDays.Count(d => d.IsWorking);
        //    int arLenth = dayCount*catLenth;
        //    double[] res = new double[arLenth];
        //    foreach (WeekOrderMenu wordmenu in weekOrderMenus)
        //    {
        //        double[] userWod = repository.UserWeekOrderDishes(wordmenu, dayCount, catLenth);
        //        for (int i = 0; i < dayCount; i++)
        //        {
        //            for (int j = 1; j <= catLenth; j++)
        //            {
        //                res[i*catLenth + j - 1] += userWod[i*catLenth + j - 1];
        //            }
        //        }
        //    }
        //    return res;
        //}

        //public static double[] UserWeekOrderDishes(this IRepositoryAsync<WeekOrderMenu> repository,
        //    WeekOrderMenu wordmenu, int dayCount, int catLength)
        //{

        //    double[] dquantities = new double[dayCount*catLength];

        //    for (int i = 0; i < dayCount; i++)
        //    {
        //        DayOrderMenu dayord = wordmenu.DayOrderMenus.ElementAt(i);

        //        List<DishQuantityRelations> dquaList =
        //            repository.GetRepositoryAsync<DishQuantityRelations>()
        //                .GetRelationsListByDayIdMenuId(dayord.Id, dayord.MenuForDay.ID);

        //        for (int j = 1; j <= catLength; j++)
        //        {
        //            var firstOrDefault = dquaList.FirstOrDefault(q => q.DishTypeId == j);
        //            if (firstOrDefault != null)
        //                dquantities[i*catLength + j - 1] += firstOrDefault.DishQuantity.Quantity;
        //        }
        //    }
        //    return dquantities;
        //}

      public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekYearDto wyDto, int? pageSize = null, int? page = null)
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
                .OrderBy(wp => wp.User.UserName)
                .ToList();
            if (pageSize != null && page != null)
            {
                pagedOrders =
                    pagedOrders.OrderBy(po => po.User.LastName)
                        .Skip(pageSize.Value*(page.Value - 1))
                        .Take(pageSize.Value)
                        .ToList();
            }
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

        //public static WeekOrderMenu CreateWeekOrderMenu(this IRepositoryAsync<WeekOrderMenu> repository, User user,
        //    WeekYearDto wyDto)
        //{
        //    ApplicationDbContext _db = repository.Context;

        //    MenuForWeek weekMenu = repository.GetRepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

        //    if (weekMenu == null) return null;

        //    List<MenuForDay> dayMenus = weekMenu.MenuForDay.ToList();

        //    if (!dayMenus.Any()) return null;

        //    WorkingWeek workingWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(wyDto);

        //    DishQuantity dqua =
        //        repository.GetRepositoryAsync<DishQuantity>()
        //            .GetAll()
        //            .FirstOrDefault(q => Math.Abs(q.Quantity) < 0.0001);

        //    dqua = _db.DishQuantities.Find(dqua.Id);

        //    WeekOrderMenu weekOrder = new WeekOrderMenu
        //    {
        //        User = user,
        //        MenuForWeek = weekMenu,
        //        WeekOrderSummaryPrice = 0.0
        //    };

        //   // PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu { WeekOrderMenu = weekOrder };
        //   // _db.PlannedWeekOrderMenus.Add(plannedWeekOrderMenu);

        //    List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();

        //    List<DishType> dishTypes = repository.GetRepositoryAsync<DishType>().GetAll();
        //    List<DishQuantityRelations> dquaList = new List<DishQuantityRelations>();
        //    foreach (MenuForDay daymenu in weekMenu.MenuForDay)
        //    {
        //        DayOrderMenu dayOrderMenu = new DayOrderMenu
        //        {
        //            MenuForDay = daymenu
        //        };
        //        //PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu {DayOrderMenu = dayOrderMenu};

        //        dquaList.AddRange(from dish in daymenu.Dishes
        //            select dishTypes.FirstOrDefault(dy => string.Equals(dy.Category, dish.DishType.Category))
        //            into first
        //            where first != null
        //            let catindex = first.Id - 1
        //            select new DishQuantityRelations
        //            {
        //                DishQuantity = dqua,
        //                DishType = first,
        //                //MenuForDay = daymenu,
        //                DayOrderMenu = dayOrderMenu,
        //                //PlannedDayOrderMenu = plannedDayOrderMenu
        //            });
        //        dayOrderMenu.DayOrderSummaryPrice = 0.00;
        //        dayOrderMenus.Add(dayOrderMenu);
        //        weekOrder.WeekOrderSummaryPrice += dayOrderMenu.DayOrderSummaryPrice;
        //    }
        //    weekOrder.DayOrderMenus = dayOrderMenus;
        //    _db.WeekOrderMenus.Add(weekOrder);
        //    _db.DQRelations.AddRange(dquaList);
        //    _db.SaveChanges();

        //    weekOrder =  repository.OrdersMenuByWeekYear(wyDto).FirstOrDefault(om => string.Equals(om.User.Id, user.Id));

        //    return weekOrder;
        //}

        public static List<User> GetUsersMadeOrder(this IRepositoryAsync<WeekOrderMenu> repository, int daymenuid)
        {
            List<WeekOrderMenu> whohasweekorder = repository.Query()
                //.Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishType)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.Dish.DishDetail)))
                .Include(om => om.DayOrderMenus.Select(dm => dm.MenuForDay.DishPriceMfdRelations.Select(dp => dp.DishPrice)))
                .Select().Where(
                    om => om.DayOrderMenus.Select(dom => dom.MenuForDay.ID).Contains(daymenuid)).ToList();

            return whohasweekorder.Select(wom => wom.User).ToList();
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
