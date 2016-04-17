using System;
using System.Globalization;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
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
        private readonly UnitOfWork _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDishService _dishService;
        private readonly IWorkDaysService _workDaysService;
        private readonly IMenuForWeekService _menuForWeekService;
        private readonly IOrderMenuService _orderMenuService;

        public UnitTest1()
        {
            _unitOfWork = new UnitOfWork();
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_unitOfWork.GetContext()));
            _dishService = new DishService(_unitOfWork.RepositoryAsync<Dish>());
            _workDaysService = new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
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
            Assert.IsTrue(_workDaysService.Queryable().Any());
        }

        [TestMethod]
        public void CreateUsersFromXml()
        {
            string userpath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                              @"ACSDining.Core\DBinitial\Employeers.xml";
            ApplicationDbInitializer.GetUsersFromXml(_unitOfWork.GetContext(), userpath);
            Assert.IsTrue(_userManager.Users.Count() > 10);
        }


    }
}
