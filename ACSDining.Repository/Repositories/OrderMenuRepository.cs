using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Repository.Repositories
{
    public static class OrderMenuRepository
    {
        public static OrdersDTO GetOrdersDtoByWeekYear(this IRepositoryAsync<OrderMenu> repository, int week, int year)
        {
            List<OrderMenu> orderMenus = repository.Queryable().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == week && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
                        .ToList();

            OrdersDTO model = new OrdersDTO()
            {
                WeekNumber = week,
                UserOrders = orderMenus
                    .Select(order => new UserOrdersDTO()
                    {
                        UserId = order.User.Id,
                        UserName = order.User.UserName,
                        Dishquantities = repository.GetUserWeekOrderDishes(order.Id),
                        WeekPaid = order.WeekPaid,
                        SummaryPrice = order.SummaryPrice
                    }).OrderBy(uo => uo.UserName).ToList(),
                YearNumber = (int)year
            };
            return model;
        }

        public static double[] GetUserWeekOrderDishes(this IRepositoryAsync<OrderMenu> repository,int orderid)
        {

            double[] dquantities = new double[20];
            OrderMenu order = repository.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList =
                repository.GetRepository<DishQuantityRelations>()
                    .Queryable()
                    .Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories =
                repository.GetRepository<DishType>().Queryable().OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = repository.GetRepository<MenuForWeek>().Find(menuforweekid);
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
                            dquantities[(i - 1) * 4 + j - 1] = firstOrDefault.DishQuantity.Quantity;
                    }
                }
            }
            return dquantities;
        }
        public static double[] GetUserWeekOrderPaiments(this IRepositoryAsync<OrderMenu> repository, int orderid)
        {

            double[] paiments = new double[20];
            OrderMenu order = repository.Find(orderid);
            int menuforweekid = order.MenuForWeek.ID;
            List<DishQuantityRelations> quaList = repository.GetRepository<DishQuantityRelations>()
                    .Queryable().Where(dqr => dqr.OrderMenuID == orderid && dqr.MenuForWeekID == menuforweekid)
                    .ToList();

            string[] categories =
                repository.GetRepository<DishType>().Queryable().OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            MenuForWeek mfw = repository.GetRepository<MenuForWeek>().Find(menuforweekid);
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
                            paiments[(i - 1) * 4 + j - 1] = firstOrDefault.DishQuantity.Quantity *
                                                          daymenu.Dishes.ElementAt(j - 1).Price;
                    }
                }
            }
            return paiments;
        }

        public static double GetSummaryPriceUserByWeekYear(this IRepositoryAsync<OrderMenu> repository, UserOrdersDTO usorder, int numweek, int year)
        {
            MenuForWeek menuForWeekRepository =
                repository.GetRepository<MenuForWeek>()
                    .Query(wm => wm.WorkingWeek.WeekNumber == numweek && wm.WorkingWeek.Year.YearNumber == year)
                    .Include(mfw => mfw.MenuForDay)
                    .Select(mfw => mfw)
                    .FirstOrDefault();
            double summary = 0;
            if (menuForWeekRepository != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        summary += menuForWeekRepository.MenuForDay.ElementAt(i).Dishes.ElementAt(j).Price *
                                   usorder.Dishquantities[4 * i + j];
                    }
                }
            }
            return summary;
        }

        public static EmployeeOrderDto EmployeeOrderByWeekYear(this IRepositoryAsync<OrderMenu> repository,WeekMenuDto weekmodel, string userid, int numweek, int year)
        {
            OrderMenu ordmenu =
                repository.Queryable()
                    .FirstOrDefault(ord => string.Equals(ord.User.Id, userid) && ord.MenuForWeek.ID == weekmodel.ID);
            EmployeeOrderDto model = new EmployeeOrderDto
            {
                UserId = userid,
                MenuId = weekmodel.ID,
                SummaryPrice = ordmenu.SummaryPrice,
                WeekPaid = ordmenu.WeekPaid,
                MfdModels = weekmodel.MFD_models,
                Year = (int)year,
                WeekNumber = (int)numweek
            };
            if (ordmenu != null)
            {
                model.OrderId = ordmenu.Id;
                model.Dishquantities = repository.GetUserWeekOrderDishes(ordmenu.Id);
            }
            return model;
        }

    }
}
