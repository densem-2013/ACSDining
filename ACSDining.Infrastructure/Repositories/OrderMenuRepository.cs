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
        public static double[] SummaryDishesQuantities(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekYearDto wyDto, int catLenth)
        {
            List<WeekOrderMenu> weekOrderMenus = repository.OrdersMenuByWeekYear(wyDto);
            WorkingWeek workingWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(wyDto);
            int dayCount = workingWeek.WorkingDays.Count(d => d.IsWorking);
            int arLenth = dayCount*catLenth;
            double[] res = new double[arLenth];
            foreach (WeekOrderMenu wordmenu in weekOrderMenus)
            {
                double[] userWod = repository.UserWeekOrderDishes(wordmenu, dayCount, catLenth);
                for (int i = 0; i < dayCount; i++)
                {
                    for (int j = 1; j <= catLenth; j++)
                    {
                        res[i*catLenth + j - 1] += userWod[i*catLenth + j - 1];
                    }
                }
            }
            return res;
        }

        public static double[] UserWeekOrderDishes(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekOrderMenu wordmenu, int dayCount, int catLength)
        {

            double[] dquantities = new double[dayCount*catLength];

            for (int i = 0; i < dayCount; i++)
            {
                DayOrderMenu dayord = wordmenu.DayOrderMenus.ElementAt(i);

                List<DishQuantityRelations> dquaList =
                    repository.GetRepositoryAsync<DishQuantityRelations>()
                        .GetRelationsListByDayIdMenuId(dayord.Id, dayord.MenuForDay.ID);

                for (int j = 1; j <= catLength; j++)
                {
                    var firstOrDefault = dquaList.FirstOrDefault(q => q.DishTypeId == j);
                    if (firstOrDefault != null)
                        dquantities[i*catLength + j - 1] += firstOrDefault.DishQuantity.Quantity;
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

        public static List<WeekOrderMenu> OrdersMenuByWeekYear(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekYearDto wyDto, int? pageSize = null, int? page = null)
        {
            List<WeekOrderMenu> pagedOrders = repository.Query()
                .Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.Dishes.Select(d => d.DishType)))
                .Select().Where(
                    om =>
                        om.MenuForWeek.WorkingWeek.WeekNumber == wyDto.Week &&
                        om.MenuForWeek.WorkingWeek.Year.YearNumber == wyDto.Year).ToList();
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

        /// <summary>
        /// Создаёт новый заказ на неделю для указанного пользователя
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="wyDto"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static WeekOrderMenu NewWeekOrdersMenuByWeekYearUser(this IRepositoryAsync<WeekOrderMenu> repository,
            WeekYearDto wyDto, User user)
        {
            MenuForWeek weekmenu = repository.GetRepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);
            if (weekmenu == null)
            {
                return null;
            }

            List<DishQuantityRelations> dquaList = new List<DishQuantityRelations>();
            List<DayOrderMenu> dayorders = weekmenu.MenuForDay.Select(daymenu => new DayOrderMenu
            {
                MenuForDay = daymenu
            }).ToList();

            WeekOrderMenu newWeekOrder = new WeekOrderMenu
            {
                User = user,
                MenuForWeek = weekmenu,
                DayOrderMenus = weekmenu.MenuForDay.Select(daymenu => new DayOrderMenu
                {
                    MenuForDay = daymenu
                }).ToList(),
                WeekOrderSummaryPrice = 0.00
            };
            PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu {WeekOrderMenu = newWeekOrder};

            List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();

            DishQuantity dqu =
                repository.GetRepositoryAsync<DishQuantity>().GetAll().FirstOrDefault(dq => dq.Quantity == 0.00);

            List<DishType> dishTypes = repository.GetRepositoryAsync<DishType>().GetAll();
            foreach (MenuForDay daymenu in weekmenu.MenuForDay)
            {
                DayOrderMenu dayOrderMenu = new DayOrderMenu
                {
                    MenuForDay = daymenu
                };
                PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu {DayOrderMenu = dayOrderMenu};
                plannedWeekOrderMenu.PlannedDayOrderMenus.Add(plannedDayOrderMenu);
                dayOrderMenus.Add(dayOrderMenu);
                foreach (DishType dishType in dishTypes)
                {
                    DishQuantityRelations dqrs = new DishQuantityRelations
                    {
                        DishQuantity = dqu,
                        DishType = dishType,
                        MenuForDay = daymenu,
                        DayOrderMenu = dayOrderMenu,
                        PlannedDayOrderMenu = plannedDayOrderMenu
                    };
                    dayOrderMenu.DayOrderSummaryPrice = 0.00;

                    dquaList.Add(dqrs);

                }
                newWeekOrder.WeekOrderSummaryPrice = 0.00;
            }

            newWeekOrder.DayOrderMenus = dayOrderMenus;

            repository.Context.DQRelations.AddRange(dquaList);

            repository.Context.PlannedWeekOrderMenus.Add(plannedWeekOrderMenu);
            repository.Context.SaveChanges();
            newWeekOrder =
                repository.OrdersMenuByWeekYear(wyDto).FirstOrDefault(om => string.Equals(om.User.Id, user.Id));

            return newWeekOrder;
        }

        public static WeekOrderMenu FindOrderMenuById(this IRepositoryAsync<WeekOrderMenu> repository, int orderid)
        {
            return repository.Query()
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(om => om.User)
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.Dishes.Select(d => d.DishType)))
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.WorkingDay.DayOfWeek))
                .Select()
                .FirstOrDefault(om => om.Id == orderid);
        }

        public static int UserWeekOrderUpdate(this IRepositoryAsync<WeekOrderMenu> repository, int catLength, UserWeekOrderDto userWeekOrderDto)
        {
            ApplicationDbContext _db = repository.Context;

            WeekOrderMenu forUpdateOrder = repository.FindOrderMenuById(userWeekOrderDto.OrderId);

            double[] userweekorderdishes = userWeekOrderDto.UserWeekOrderDishes;

            for (int i = 0; i < userWeekOrderDto.DayOrderDtos.Count; i++)
            {
                UserDayOrderDto doDto = userWeekOrderDto.DayOrderDtos.ElementAtOrDefault(i);
                DayOrderMenu dom = forUpdateOrder.DayOrderMenus.FirstOrDefault(dm => doDto != null && dm.Id == doDto.DayOrderId);
                if (dom != null && doDto != null)
                {
                    if (dom.MenuForDay.OrderCanBeChanged)
                    {
                        List<DishQuantityRelations> dqaList =
                            repository.GetRepositoryAsync<DishQuantityRelations>()
                                .GetRelationsListByDayIdMenuId(dom.Id, dom.MenuForDay.ID);

                        for (int j = 1; j <= catLength; j++)
                        {
                            //Находим связь, указывающую на текущее значение фактической дневной заявки на блюдо
                            var firstOrDefault = dqaList.FirstOrDefault(q => q.DishTypeId == j);
                            if (firstOrDefault != null)
                            {
                                double curQuantity = firstOrDefault.DishQuantity.Quantity;
                                //если заказанное количество изменилось
                                if (
                                    Math.Abs(curQuantity - userweekorderdishes[i * (j - 1) + j - 1]) >
                                    0.001)
                                {
                                    var dishQuantity = repository.GetRepositoryAsync<DishQuantity>().GetAll()
                                        .FirstOrDefault(
                                            dq =>
                                                Math.Abs(dq.Quantity -
                                                         userweekorderdishes[i * (j - 1) + j - 1]) <
                                                0.001);
                                    if (dishQuantity == null) continue;

                                    //переустанавливаем связь на найденную сущность, содержащую искомое количество
                                    firstOrDefault.DishQuantity = dishQuantity;

                                    _db.DQRelations.AddOrUpdate(firstOrDefault);

                                    //обновляем сумму заказа на день
                                    dom.DayOrderSummaryPrice = doDto.DayOrderSummary;
                                    _db.DayOrderMenus.AddOrUpdate(dom);

                                }
                            }
                        }
                    }
                }
            }

            //обновляем сумму заказа на неделю
            forUpdateOrder.WeekOrderSummaryPrice = userWeekOrderDto.WeekSummaryPrice;

            _db.WeekOrderMenus.AddOrUpdate(forUpdateOrder);

            return _db.SaveChangesAsync().Result;
        }

        public static WeekOrderMenu CreateWeekOrderMenu(this IRepositoryAsync<WeekOrderMenu> repository, User user,
            WeekYearDto wyDto)
        {
            ApplicationDbContext _db = repository.Context;

            MenuForWeek weekMenu = repository.GetRepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

            if (weekMenu == null) return null;

            List<MenuForDay> dayMenus = weekMenu.MenuForDay.ToList();

            if (!dayMenus.Any()) return null;

            WorkingWeek workingWeek = repository.GetRepositoryAsync<MenuForWeek>().WorkWeekByWeekYear(wyDto);

            DishQuantity dqua =
                repository.GetRepositoryAsync<DishQuantity>()
                    .GetAll()
                    .FirstOrDefault(q => Math.Abs(q.Quantity) < 0.0001);

            dqua = _db.DishQuantities.Find(dqua.Id);

            WeekOrderMenu weekOrder = new WeekOrderMenu
            {
                User = user,
                MenuForWeek = weekMenu,
                WeekOrderSummaryPrice = 0.0
            };

            PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu { WeekOrderMenu = weekOrder };
            _db.PlannedWeekOrderMenus.Add(plannedWeekOrderMenu);

            List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();

            List<DishType> dishTypes = repository.GetRepositoryAsync<DishType>().GetAll();
            List<DishQuantityRelations> dquaList = new List<DishQuantityRelations>();
            foreach (MenuForDay daymenu in weekMenu.MenuForDay)
            {
                DayOrderMenu dayOrderMenu = new DayOrderMenu
                {
                    MenuForDay = daymenu
                };
                PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu {DayOrderMenu = dayOrderMenu};

                dquaList.AddRange(from dish in daymenu.Dishes
                    select dishTypes.FirstOrDefault(dy => string.Equals(dy.Category, dish.DishType.Category))
                    into first
                    where first != null
                    let catindex = first.Id - 1
                    select new DishQuantityRelations
                    {
                        DishQuantity = dqua,
                        DishType = first,
                        MenuForDay = daymenu,
                        DayOrderMenu = dayOrderMenu,
                        PlannedDayOrderMenu = plannedDayOrderMenu
                    });
                dayOrderMenu.DayOrderSummaryPrice = 0.00;
                dayOrderMenus.Add(dayOrderMenu);
                weekOrder.WeekOrderSummaryPrice += dayOrderMenu.DayOrderSummaryPrice;
            }
            weekOrder.DayOrderMenus = dayOrderMenus;
            _db.WeekOrderMenus.Add(weekOrder);
            _db.DQRelations.AddRange(dquaList);
            _db.SaveChanges();

            weekOrder =  repository.OrdersMenuByWeekYear(wyDto).FirstOrDefault(om => string.Equals(om.User.Id, user.Id));

            return weekOrder;
        }

        public static List<User> GetUsersMadeOrder(this IRepositoryAsync<WeekOrderMenu> repository, int daymenuid)
        {
            List<WeekOrderMenu> whohasweekorder = repository.Query()
                .Include(om => om.MenuForWeek.MenuForDay.Select(dm => dm.Dishes.Select(d => d.DishType)))
                .Include(om => om.User)
                .Include(om => om.MenuForWeek.WorkingWeek.Year)
                .Include(wm => wm.MenuForWeek.WorkingWeek.WorkingDays.Select(d => d.DayOfWeek))
                .Include(om => om.DayOrderMenus.Select(dom => dom.MenuForDay.Dishes.Select(d => d.DishType)))
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
