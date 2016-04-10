using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var xml = XDocument.Load(path);
            Assert.IsNotNull(xml);
            if (xml.Root != null)
            {
                var collection = xml.Root.Descendants("dish");

                List<DishType> dtList = _unitOfWork.RepositoryAsync<DishType>().Queryable().ToList();

                Func<string, DishType> getDishType =
                    el1 =>
                    {
                        DishType dtype = dtList.FirstOrDefault(dt => string.Equals(el1, dt.Category));

                        return dtype;
                    };

                Func<string, double> parseDouble = str =>
                {
                    double num;
                    Double.TryParse(str, out num);
                    return num/100;
                };
                Dish[] dishes = (from el in collection.AsEnumerable()
                    let xElement = el.Element("description")
                    where xElement != null
                    let element = el.Element("cost")
                    where element != null
                    let xElement1 = el.Element("foods")
                    where xElement1 != null
                    let element1 = el.Element("recept")
                    where element1 != null
                    select new Dish
                    {
                        DishType = getDishType(el.Attribute("dishtype").Value),
                        Title = el.Attribute("title").Value,
                        Description = xElement.Value,
                        ProductImage = el.Attribute("image").Value,
                        Price = parseDouble(element.Value),
                        DishDetail = new DishDetail
                        {
                            Title = el.Attribute("title").Value,
                            Foods = xElement1.Value,
                            Recept = element1.Value
                        }
                    }).ToArray();
                Assert.IsTrue(dishes.Any());
            }
        }

        public void WeekNumber_Test()
        {
            Func<int> nweek = () =>
            {
                CultureInfo myCi = new CultureInfo("uk-UA");
                Calendar myCal = myCi.Calendar;

                // Gets the DTFI properties required by GetWeekOfYear.
                CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
                System.DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
                DateTime lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                return myCal.GetWeekOfYear(lastDay, myCwr, myFirstDow);

            };
            int wn = nweek();
            Assert.IsTrue(wn > 40);
        }
        [TestMethod]
        public void CreateMenuForWeek()
        {
            Random rand = new Random(); 
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => _dishService.Queryable().Count(d => string.Equals(d.DishType.Category, count)));
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(_dishService.Queryable().Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
                }
                return ds;
            };
            Func<List<Dish>, double> getTotalPrice = dishList =>
            {
                return dishList.AsEnumerable().ToList().Select(d => d.Price).Sum();
            };
            Func<WorkingWeek> getWorkingWeek = () =>
            {
                int i = 0;
                WorkingWeek workingWeek = null;
                while (i<10)
                {
                    WeekYearDto nextweekDto = YearWeekHelp.GetNextWeekYear(new WeekYearDto
                    {
                        Week = YearWeekHelp.CurrentWeek() + i,
                        Year = (YearWeekHelp.CurrentWeek() + i > YearWeekHelp.YearWeekCount(DateTime.Now.Year)) ? DateTime.Now.Year + 1 : DateTime.Now.Year
                    });
                    workingWeek =
                _workDaysService.Queryable().FirstOrDefault(
                    w => w.WeekNumber == (nextweekDto.Week) && w.Year.YearNumber == nextweekDto.Year);
                    i++;
                }
                return workingWeek;
            };
            List<MenuForDay> mfdays = new List<MenuForDay>();

            WorkingWeek week = getWorkingWeek();

            for (int i = 1; i <= 7; i++)
            {
                WorkingDay day = week.WorkingDays.FirstOrDefault(wd => wd.Id == i);
                List<Dish> dishes = getDishes();
                MenuForDay dayMenu = new MenuForDay
                {
                    Dishes = dishes,
                    WorkingDay = day,
                    WorkingWeek = week,
                    TotalPrice = getTotalPrice(dishes)
                };
                mfdays.Add(dayMenu);
            }
            _menuForWeekService.Insert(new MenuForWeek
            {
                MenuForDay = mfdays,
                WorkingWeek = week
            });
            Assert.IsTrue(_menuForWeekService.Queryable().Select(w => w.MenuForDay.Where(m => m.TotalPrice > 0)).Any());
        }

       
        private double[] PaimentsByDishes(int numweek, int year)
        {
            string[] categories =
                    _unitOfWork.Repository<DishType>()
                        .Queryable()
                        .ToList()
                        .OrderBy(d => d.Id)
                        .Select(dt => dt.Category)
                        .AsQueryable()
                        .ToArray();

            MenuForWeek weekmenu = _menuForWeekService.Queryable().FirstOrDefault(m => m.WorkingWeek.WeekNumber == numweek && m.WorkingWeek.Year.YearNumber == year);
           

            double[] paiments = new double[21];
            double[] weekprices = _menuForWeekService.UnitWeekPrices(weekmenu.ID, categories);


            WeekOrderMenu[] weekOrderMenus = _orderMenuService.Query().Include(or => or.MenuForWeek.WorkingWeek.Year).Select().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == numweek && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
                        .ToArray();
            for (int i = 0; i < weekOrderMenus.Length; i++)
            {
                int menuforweekid = weekOrderMenus[i].MenuForWeek.ID;
                List<DishQuantityRelations> quaList = _unitOfWork.RepositoryAsync<DishQuantityRelations>()
                        .Queryable()
                        .Where(dqr => dqr.OrderMenuId == weekOrderMenus[i].Id && dqr.MenuForWeekId == menuforweekid)
                        .ToList();
                double[] dishquantities = _orderMenuService.UserWeekOrderDishes(quaList, categories, weekOrderMenus[i].MenuForWeek);
                for (int j = 0; j < 20; j++)
                {
                    paiments[j] += weekprices[j] * dishquantities[j];
                }
            }
            paiments[20] = paiments.Sum();
            return paiments;
        }

        [TestMethod]
        public void CreateWorkingDays()
        {
            IEnumerable<Year> startyears = _unitOfWork.RepositoryAsync<Year>().Queryable();
            if (!startyears.Any())
            {
                _unitOfWork.RepositoryAsync<Year>().Insert(new Year
                {
                    YearNumber = DateTime.Now.Year
                });
                _unitOfWork.RepositoryAsync<Year>().Insert(new Year
                {
                    YearNumber = DateTime.Now.Year - 1
                });

            }
            _unitOfWork.SaveChanges();

            List<Year> years = _unitOfWork.RepositoryAsync<Year>().Queryable().ToList();

            IEnumerable<WorkingDay> startWorkingDays =
                _workDaysService.Query()
                    .Include(ww => ww.WorkingDays.Select(w => w.DayOfWeek))
                    .Select()
                    .SelectMany(ww => ww.WorkingDays)
                    .ToList();

            if (!startWorkingDays.Any())
            {
                foreach (Year year in years)
                {
                    int weekcount = YearWeekHelp.YearWeekCount(year.YearNumber);
                    List<WorkingWeek> workweeks = new List<WorkingWeek>();
                    for (int i = 0; i < weekcount; i++)
                    {
                        WorkingWeek workingWeek = new WorkingWeek
                        {
                            WeekNumber = i + 1,
                            Year = year,
                            WorkingDays = new List<WorkingDay>()
                        };
                        List<WorkingDay> workdays = new List<WorkingDay>();
                        for (int j = 0; j < 7; j++)
                        {
                            var firstOrDefault = startWorkingDays.FirstOrDefault(wd=>wd.DayOfWeek.ID==j + 1);
                            if (firstOrDefault != null)
                            {
                                WorkingDay workday = new WorkingDay
                                {
                                    IsWorking = j < 5,
                                    DayOfWeek = firstOrDefault.DayOfWeek
                                };
                                workdays.Add(workday);
                                workingWeek.WorkingDays.Add(workday);
                            }
                        }

                        _unitOfWork.RepositoryAsync<WorkingDay>().InsertRange(workdays);
                        _unitOfWork.SaveChanges();
                        year.WorkingWeeks.Add(workingWeek);
                        _unitOfWork.RepositoryAsync<Year>().Update(year);
                    }
                    _workDaysService.InsertRange(workweeks);
                    _unitOfWork.SaveChanges();
                }
            }
            Assert.IsTrue(_workDaysService.Queryable().Any());
        }

        [TestMethod]
        public void CreateUsersFromXml()
        {
            string userpath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\Employeers.xml";
            var xml = XDocument.Load(userpath);
            if (xml.Root != null)
            {
                var collection = xml.Root.Descendants("Employeer");
                try
                {
                    var hasher = new PasswordHasher();
                    IdentityRole role = _roleManager.FindByNameAsync("Employee").Result;
                    User[] users = collection.AsEnumerable().Select(el =>
                    {
                        XElement xElement = el.Element("FirstName");
                        if (xElement != null)
                        {
                            XElement element = el.Element("LastName");
                            if (element != null)
                                return new User
                                {
                                    FirstName = xElement.Value,
                                    LastName = element.Value,
                                    UserName =
                                        string.Format("{0} {1}", element.Value, xElement.Value),
                                    PasswordHash = hasher.HashPassword("777123"),
                                    Email = "test@test.com",
                                    EmailConfirmed = true,
                                    SecurityStamp = Guid.NewGuid().ToString(),
                                    RegistrationDate = DateTime.UtcNow,
                                    LastLoginTime = DateTime.UtcNow
                                };
                        }
                        return null;
                    }).ToArray();
                    foreach (User user in users)
                    {
                        if (role != null)
                            user.Roles.Add(new IdentityUserRole
                            {
                                RoleId = role.Id,
                                UserId = user.Id
                            });
                        _userManager.Create(user);
                    }
                    Assert.IsTrue(_userManager.Users.Any());
                }
                
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.InnerException, ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateWeekMenu()
        {
            Random rand = new Random();
            Dish[] dishArray = GetDishesFromXml();
            string[] categories =
                _dishService.Queryable()
                    .OrderBy(t => t.DishID)
                    .Select(dt => dt.DishType.Category)
                    .ToArray();

            Func<string, IEnumerable<Dish>, int> countDish = (str, list) =>
            {
                int coun = list.Count(el => string.Equals(el.DishType.Category, str));
                return coun;
            };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => countDish(count, dishArray));
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(
                        dishArray.Where(d => string.Equals(d.DishType.Category, pair.Key))
                            .ElementAt(rand.Next(pair.Value)));
                }
                return ds;
            };

            Year year = _unitOfWork.RepositoryAsync<Year>().Queryable().FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            bool weekLessZero = false;
            Year correct_year = _unitOfWork.RepositoryAsync<Year>().Queryable().FirstOrDefault(y => y.YearNumber == DateTime.Now.Year - 1);
            int correct_week = 0;
            List<MenuForWeek> menus = new List<MenuForWeek>();
            List<WorkingDay> workdays = _unitOfWork.RepositoryAsync<WorkingDay>().Queryable().Include("DayOfWeek").Include("WorkingWeek").ToList();
            List<WorkingWeek> workweeks = _workDaysService.Queryable().Include("WorkingDays").ToList();
            for (int week = 0; week < 25; week++)
            {
                int curweek = YearWeekHelp.CurrentWeek() - week + correct_week;
                weekLessZero = YearWeekHelp.CurrentWeek() - week <= 0;
                if (weekLessZero)
                {
                    year = correct_year;
                    correct_week = YearWeekHelp.YearWeekCount(DateTime.Now.Year - 1);
                }
                List<MenuForDay> mfdays = new List<MenuForDay>();
                WorkingWeek workweek = _workDaysService.Queryable().FirstOrDefault(
                    w => w.WeekNumber == curweek);
                for (int i = 1; i <= 7; i++)
                {
                    List<Dish> dishes = getDishes();
                    WorkingDay workday = workdays.FirstOrDefault(
                        wd =>
                            wd.DayOfWeek.ID == i &&
                            wd.WorkingWeek.WeekNumber == curweek);
                    if (workday != null && workday.IsWorking)
                    {
                        MenuForDay dayMenu = new MenuForDay
                        {
                            Dishes = dishes,
                            WorkingDay = workday,
                            WorkingWeek = workweek,
                            TotalPrice = dishes.Select(d => d.Price).Sum()
                        };

                        mfdays.Add(dayMenu);

                    }
                }
                menus.Add(new MenuForWeek
                {
                    MenuForDay = mfdays,
                    WorkingWeek = workweek,
                    SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum()
                });
            }
            Assert.IsTrue(menus.Count == 25);
        }

        private Dish[] GetDishesFromXml()
        {
            string userspath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
            var xml = XDocument.Load(userspath);
            if (xml.Root != null)
            {
                var collection = xml.Root.Descendants("dish");

                List<DishType> dtList = _unitOfWork.RepositoryAsync<DishType>().Queryable().ToList();

                Func<string, DishType> getDishType =
                    el1 =>
                    {
                        DishType dtype = dtList.FirstOrDefault(dt => string.Equals(el1, dt.Category));

                        return dtype;
                    };

                Func<string, double> parseDouble = str =>
                {
                    double num = Double.Parse(str);
                    return num;
                };
                Dish[] dishes = (from el in collection.AsEnumerable()
                    let xElement = el.Element("description")
                    where xElement != null
                    let element = el.Element("cost")
                    where element != null
                    let xElement1 = el.Element("foods")
                    where xElement1 != null
                    let element1 = el.Element("recept")
                    where element1 != null
                    select new Dish
                    {
                        DishType = getDishType(el.Attribute("dishtype").Value),
                        Title = el.Attribute("title").Value,
                        Description = xElement.Value,
                        ProductImage = el.Attribute("image").Value,
                        Price = parseDouble(element.Value),
                        DishDetail = new DishDetail
                        {
                            Title = el.Attribute("title").Value,
                            Foods = xElement1.Value,
                            Recept = element1.Value
                        }
                    }).ToArray();

                return dishes;
            }
            return null;
        }
    }
}
