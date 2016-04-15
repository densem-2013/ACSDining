using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;

namespace ACSDining.Infrastructure.Repositories
{
    public static class OrderMenuRepository
    {
        public static double[] GetUserWeekOrderDishes(this IRepositoryAsync<WeekOrderMenu> repository,
            List<DishQuantityRelations> quaList, string[] categories, MenuForWeek mfw)
        {

            double[] dquantities = new double[20];

            for (int i = 1; i <= 7; i++)
            {
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.Id == i);
                if (workday != null && workday.IsWorking)
                {
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.MenuForDay.WorkingDay.DayOfWeek.Id == i && q.DishTypeId == j
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
                WorkingDay workday = mfw.WorkingWeek.WorkingDays.FirstOrDefault(wd => wd.DayOfWeek.Id == i);
                if (workday != null && workday.IsWorking)
                {
                    MenuForDay daymenu = mfw.MenuForDay.ElementAt(i - 1);
                    for (int j = 1; j <= categories.Length; j++)
                    {
                        var firstOrDefault = quaList.FirstOrDefault(
                            q => q.MenuForDay.WorkingDay.DayOfWeek.Id == i && q.DishTypeId == j
                            );
                        if (firstOrDefault != null)
                            paiments[(i - 1)*4 + j - 1] = firstOrDefault.DishQuantity.Quantity*
                                                          daymenu.Dishes.ElementAt(j - 1).Price;
                    }
                }
            }
            return paiments;
        }

        public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository, WeekYearDto wyDto)
        {
            return repository.Query()
                    .Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                    .Include(om => om.User)
                    //.Include(om => om.PlannedWeekOrderMenu)
                    .Include(om => om.MenuForWeek.WorkingWeek.Year)
                    .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                    .Select().Where(
                om =>
                    om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week && om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year).ToList();
        }

        /// <summary>
        /// Создаёт новый заказ на неделю для указанного пользователя
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="wyDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static WeekOrderMenu NewWeekOrdersMenuByWeekYearUser(this IRepositoryAsync<WeekOrderMenu> repository, WeekYearDto wyDto, User user)
        {
            MenuForWeek weekmenu = repository.GetRepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);
            if (weekmenu==null)
            {
                return null;
            }
            
            List<DishQuantityRelations> dquaList = new List<DishQuantityRelations>();
            List<DayOrderMenu> dayorders = weekmenu.MenuForDay.Select(daymenu =>
            {
                return new DayOrderMenu
                {
                    MenuForDay = daymenu
                };
            }).ToList();

            WeekOrderMenu newWeekOrder=new WeekOrderMenu
            {
                User=user,
                MenuForWeek = weekmenu,
                DayOrderMenus = weekmenu.MenuForDay.Select(daymenu=>new DayOrderMenu
                {
                    MenuForDay = daymenu
                }).ToList(),
                WeekOrderSummaryPrice = 0.00
            };
            PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu { WeekOrderMenu = newWeekOrder };

            List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();

            DishQuantity dqu = repository.GetRepositoryAsync<DishQuantity>().GetAll().FirstOrDefault(dq => dq.Quantity == 0.00);

            foreach (MenuForDay daymenu in weekmenu.MenuForDay)
            {
                DayOrderMenu dayOrderMenu = new DayOrderMenu
                {
                    MenuForDay = daymenu
                };
                PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu { DayOrderMenu = dayOrderMenu };
                plannedWeekOrderMenu.PlannedDayOrderMenus.Add(plannedDayOrderMenu);
                dayOrderMenus.Add(dayOrderMenu);
                foreach (Dish dish in daymenu.Dishes)
                {
                    DishType first =
                        repository.GetRepositoryAsync<DishType>()
                            .GetAll()
                            .FirstOrDefault(dy => string.Equals(dy.Category, dish.DishType.Category));

                    if (first != null)
                    {
                        DishQuantityRelations dqrs = new DishQuantityRelations
                        {
                            DishQuantity = dqu,
                            DishType = first,
                            MenuForDay = daymenu,
                            DayOrderMenu = dayOrderMenu,
                            PlannedDayOrderMenu = plannedDayOrderMenu
                        };
                        dayOrderMenu.DayOrderSummaryPrice = 0.00;
                        dquaList.Add(dqrs);
                    }
                }
                newWeekOrder.WeekOrderSummaryPrice = 0.00;
            }
            
            newWeekOrder.DayOrderMenus = dayOrderMenus;

            repository.GetRepositoryAsync<DishQuantityRelations>().InsertRange(dquaList);

            repository.GetRepositoryAsync<PlannedWeekOrderMenu>().Insert(plannedWeekOrderMenu);

            newWeekOrder =
                repository.OrdersMenuByWeekYear(wyDto).FirstOrDefault(om => string.Equals(om.User.Id, user.Id));

            return newWeekOrder;
        }
    }
}
