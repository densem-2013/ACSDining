using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.DTO.Employee;
using ACSDining.Core.DTO.SuperUser;

namespace ACSDining.Repository.Repositories
{
    public static class OrderMenuRepository
    {
        //public static OrdersDTO GetOrdersDtoByWeekYear(this IRepositoryAsync<OrderMenu> repository, int week, int year)
        //{
        //    List<OrderMenu> orderMenus = repository.Queryable().Where(
        //        om =>
        //            om.MenuForWeek.WorkingWeek.WeekNumber == week && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
        //        .ToList();

        //    OrdersDTO model = new OrdersDTO
        //    {
        //        WeekNumber = week,
        //        UserOrders = orderMenus
        //            .Select(order =>
        //            {
        //                MenuForWeek mfw = order.MenuForWeek;
        //                int menuforweekid = mfw.ID;
        //                int ordid = order.Id;
        //                List<DishQuantityRelations> quaList = DbSet<DishQuantityRelations>()
        //                        .Queryable()
        //                        .Where(dqr => dqr.OrderMenuID == ordid && dqr.MenuForWeekID == menuforweekid)
        //                        .ToList();
        //                return new UserOrdersDTO()
        //                {
        //                    UserId = order.User.Id,
        //                    UserName = order.User.UserName,
        //                    Dishquantities = repository.GetUserWeekOrderDishes(quaList, categories, mfw),
        //                    WeekPaid = order.WeekPaid,
        //                    SummaryPrice = order.SummaryPrice
        //                };
        //            }).OrderBy(uo => uo.UserName).ToList(),
        //        YearNumber = (int)year
        //    };
        //    return model;
        //}

        public static double[] GetUserWeekOrderDishes(this IRepositoryAsync<OrderMenu> repository,  List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
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

        public static double[] GetUserWeekOrderPaiments(this IRepositoryAsync<OrderMenu> repository, List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
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

        //public static double GetSummaryPriceUserByWeekMenu(this IRepositoryAsync<OrderMenu> repository,
        //    UserOrdersDTO usorder, MenuForWeek menuForWeek)
        //{
        //    double summary = 0;
        //    if (menuForWeek != null)
        //    {
        //        for (int i = 0; i < 5; i++)
        //        {
        //            for (int j = 0; j < 4; j++)
        //            {
        //                summary += menuForWeek.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price*
        //                           usorder.Dishquantities[4*i + j];
        //            }
        //        }
        //    }
        //    return summary;
        //}

        //public static EmployeeOrderDto EmployeeOrderByWeekYear(this IRepositoryAsync<OrderMenu> repository,
        //    WeekMenuDto weekmodel, string userid, int numweek, int year)
        //{
        //    OrderMenu ordmenu =
        //        repository.Queryable()
        //            .FirstOrDefault(ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == weekmodel.ID);
        //    EmployeeOrderDto model = new EmployeeOrderDto
        //    {
        //        UserId = userid,
        //        MenuId = weekmodel.ID,
        //        SummaryPrice = ordmenu.SummaryPrice,
        //        WeekPaid = ordmenu.WeekPaid,
        //        MfdModels = weekmodel.MFD_models,
        //        Year = (int) year,
        //        WeekNumber = (int) numweek
        //    };
        //    model.OrderId = ordmenu.Id;
        //    model.Dishquantities = repository.GetUserWeekOrderDishes(quaList, categories, mfw);
        //    return model;
        //}

        public static OrderMenu OrderMenuByWeekYear(this IRepositoryAsync<OrderMenu> repository, int week, int year)
        {
            return repository.Queryable().FirstOrDefault(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == week && om.MenuForWeek.WorkingWeek.Year.YearNumber == year);
        }

    }
}
