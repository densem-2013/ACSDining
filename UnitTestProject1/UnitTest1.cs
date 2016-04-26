using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.Services;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayOfWeek = System.DayOfWeek;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly Random Rand = new Random();
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDishService _dishService;
        private readonly IMenuForWeekService _menuForWeekService;
        private readonly IOrderMenuService _orderMenuService;

        public UnitTest1()
        {
            _unitOfWork = new UnitOfWork();
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_unitOfWork.GetContext()));
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
            _menuForWeekService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService=new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }

        [TestMethod]
        public void ConfigXmlLoad_Test()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";

            Dish[] dishes = ApplicationDbInitializer.GetDishesFromXml(_unitOfWork.GetContext(), path);

                Assert.IsTrue(dishes.Any());
        }

        [TestMethod]
        public void WeekNumber_Test()
        {
            Func<int> nweek = () =>
            {
                CultureInfo myCI = new CultureInfo("en-US");
                Calendar myCal = myCI.Calendar;

                // Gets the DTFI properties required by GetWeekOfYear.
                CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
                
                // Displays the total number of weeks in the current year.
                //DateTime LastDay = new System.DateTime(DateTime.Now.Year, 12, 31);
                return myCal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, myFirstDOW);

            };
            int wn = nweek();
            Assert.IsTrue(wn == 15 );
        }
        [TestMethod]
        public void CreateMenuForWeek()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
            Dish[] dishes = ApplicationDbInitializer.GetDishesFromXml(_unitOfWork.GetContext(), path);
            ApplicationDbInitializer.CreateMenuForWeek(_unitOfWork.GetContext(), dishes);
           
            Assert.IsTrue(_menuForWeekService.Queryable().Select(w => w.MenuForDay.Where(m => m.TotalPrice > 0)).Any());
        }

       

        [TestMethod]
        public void CreateWorkingDays()
        {
            ApplicationDbInitializer.CreateWorkingDays(_unitOfWork.GetContext());
            Assert.IsTrue(_unitOfWork.GetContext().WorkingDays.Any());
        }

        [TestMethod]
        public void CreateUsersFromXml()
        {
            string userpath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                              @"ACSDining.Core\DBinitial\Employeers.xml";
            ApplicationDbInitializer.GetUsersFromXml(_unitOfWork.GetContext(), userpath);
            Assert.IsTrue(_userManager.Users.Count() > 10);
        }

        [TestMethod]
        public void OrderSummaryPriceTest()
        {
            //Rand=new Random();

            User user = _userManager.FindByName("employee");
            WeekYearDto wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            MenuForWeek weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);
            int rnd;

            double[][] coursesnums =
            {
                new[] {0, 0.5, 0.5, 1.0, 1.0, 1.0, 1.0},
                new[] {0, 0, 1.0, 1.0, 1.0, 1.0, 2.0},
                new[] {0, 1.0},
                new[] {0, 1.0}
            };
            int[] numsForCourses = new int[4];

            for (int i = 0; i < 4; i++)
            {
                numsForCourses[i] = coursesnums[i].Length;
            }
            List<DishQuantityRelations> dquaList = new List<DishQuantityRelations>();

            WeekOrderMenu weekOrder = new WeekOrderMenu
            {
                User = user,
                MenuForWeek = weekmenu,
                WeekOrderSummaryPrice = 0.0
            };
            PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu { WeekOrderMenu = weekOrder };
            //context.PlannedWeekOrderMenus.Add(plannedWeekOrderMenu);

            List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();

            List<DishType> dishTypes = MapHelper.GetDishCategories(_unitOfWork);
            foreach (MenuForDay daymenu in weekmenu.MenuForDay)
            {
                DayOrderMenu dayOrderMenu = new DayOrderMenu
                {
                    MenuForDay = daymenu,
                };
                ///PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu { DayOrderMenu = dayOrderMenu };

                foreach (Dish dish in daymenu.Dishes)
                {
                    DishType first = null;
                    foreach (var dy in dishTypes)
                    {
                        if (string.Equals(dy.Category, dish.DishType.Category))
                        {
                            first = dy;
                            break;
                        }
                    }
                    if (first != null)
                    {
                        int catindex = first.Id - 1;

                        rnd = Rand.Next(numsForCourses[catindex]);
                        DishQuantity dqu =_unitOfWork.RepositoryAsync<DishQuantity>().GetAll()
                            .FirstOrDefault(
                                dq => dq.Quantity == coursesnums[catindex][rnd]);
                        DishQuantityRelations dqrs = new DishQuantityRelations
                        {
                            DishQuantity = dqu,
                            DishType = first,
                            MenuForDay = daymenu,
                            DayOrderMenu = dayOrderMenu//,
                            //PlannedDayOrderMenu = plannedDayOrderMenu
                        };
                        if (dqu != null) dayOrderMenu.DayOrderSummaryPrice += dqu.Quantity * dish.Price;
                        dquaList.Add(dqrs);
                    }

                }
                dayOrderMenus.Add(dayOrderMenu);
                weekOrder.WeekOrderSummaryPrice += dayOrderMenu.DayOrderSummaryPrice;
            }
            weekOrder.DayOrderMenus = dayOrderMenus;

            WeekOrderMenu checkOrder = _orderMenuService.FindByUserIdWeekYear(user.Id, wyDto);
            double checkprice = checkOrder.DayOrderMenus.Select(dom =>
            {
                List<DishQuantityRelations> dqualist =
                    _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                        .GetRelationsListByDayIdMenuId(dom.Id, dom.MenuForDay.ID);
                double sum = 0;
                foreach (DishType dishType in dishTypes)
                {
                    var dishQuantityRelations = dqualist.FirstOrDefault(q => q.DishTypeId == dishType.Id);
                    if (dishQuantityRelations != null)
                    {
                        DishQuantity qua = dishQuantityRelations.DishQuantity;
                        sum += dom.MenuForDay.Dishes.ElementAt(dishType.Id - 1).Price*qua.Quantity;
                    }
                }
                return sum;
            }).Sum();

            Assert.IsTrue(Math.Abs(weekOrder.WeekOrderSummaryPrice - checkprice) < 0.0001);
        }
    }
}
