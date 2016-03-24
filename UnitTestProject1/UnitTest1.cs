using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Identity;
using ACSDining.Repository.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IDataContextAsync dbcontext=new DataContext();
        private readonly IRepositoryAsync<DishType> _dishtypeRepository;
        private readonly IRepositoryAsync<MenuForWeek> _weekmenuRepository;
        private readonly IRepositoryAsync<Year> _yearRepository;
        private readonly IRepositoryAsync<Dish> _dishRepository;
        private readonly IRepositoryAsync<DayOfWeek> _dayRepository;
        private readonly IRepositoryAsync<OrderMenu> _orderRepository;
        private readonly IRepositoryAsync<WorkingWeek> _workingWeekRepository;
        private readonly IRepositoryAsync<WorkingDay> _workingDayRepository;
        private readonly IRepositoryAsync<UserRole> _userRoleRepository;
        private readonly IRepositoryAsync<User> _userRepository; 

        public UnitTest1()
        {
            //_db = UnitOfWork.GetContext();
            _unitOfWork = new UnitOfWork(dbcontext);
            _dishtypeRepository = _unitOfWork.RepositoryAsync<DishType>();
            _weekmenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
            _yearRepository = _unitOfWork.RepositoryAsync<Year>();
            _dishRepository = _unitOfWork.RepositoryAsync<Dish>();
            _dayRepository = _unitOfWork.RepositoryAsync<DayOfWeek>();
            _orderRepository = _unitOfWork.RepositoryAsync<OrderMenu>();
            _workingWeekRepository = _unitOfWork.RepositoryAsync<WorkingWeek>();
            _workingDayRepository = _unitOfWork.RepositoryAsync<WorkingDay>();
            _userRoleRepository = _unitOfWork.RepositoryAsync<UserRole>();
            _userRepository = _unitOfWork.RepositoryAsync<User>();
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

                List<DishType> dtList = _dishtypeRepository.Queryable().ToList();

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
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => _dishRepository.Queryable().Count(d => string.Equals(d.DishType.Category, count)));
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(_dishRepository.Queryable().Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
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
                while (workingWeek != null&&i<10)
                {
                    WeekYearDTO nextweekDto = UnitOfWork.GetNextWeekYear(new WeekYearDTO
                    {
                        Week = UnitOfWork.CurrentWeek() + i,
                        Year = (UnitOfWork.CurrentWeek() + i > UnitOfWork.YearWeekCount(DateTime.Now.Year)) ? DateTime.Now.Year + 1 : DateTime.Now.Year
                    });
                    workingWeek =
                _workingWeekRepository.Queryable().FirstOrDefault(
                    w => w.WeekNumber == (nextweekDto.Week) && w.Year.YearNumber == nextweekDto.Year);
                    i++;
                }
                return workingWeek;
            };
            List<MenuForDay> mfdays = new List<MenuForDay>();

            WorkingWeek week = getWorkingWeek();

            for (int i = 1; i <= 7; i++)
            {
                WorkingDay day = week.WorkingDays.FirstOrDefault(wd => wd.ID == i);
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
            _weekmenuRepository.Insert(new MenuForWeek
            {
                MenuForDay = mfdays,
                WorkingWeek = week
            });
            Assert.IsTrue(_weekmenuRepository.Queryable().Select(w => w.MenuForDay.Where(m => m.TotalPrice > 0)).Any());
        }

       
        private double[] PaimentsByDishes(int numweek, int year)
        {
            double[] paiments = new double[21];
            MenuForWeek weekmenu = _weekmenuRepository.Queryable().FirstOrDefault(m => m.WorkingWeek.WeekNumber == numweek && m.WorkingWeek.Year.YearNumber == year);
            double[] weekprices = _weekmenuRepository.GetUnitWeekPrices(weekmenu.ID);


            OrderMenu[] orderMenus = _orderRepository.Queryable().Where(
                        om => om.MenuForWeek.WorkingWeek.WeekNumber == numweek && om.MenuForWeek.WorkingWeek.Year.YearNumber == year)
                        .ToArray();
            for (int i = 0; i < orderMenus.Length; i++)
            {
                double[] dishquantities = _orderRepository.GetUserWeekOrderDishes(orderMenus[i].Id);
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
            IEnumerable<Year> startyears = _yearRepository.Queryable();
            if (!startyears.Any())
            {
                _yearRepository.Insert(new Year
                {
                    YearNumber = DateTime.Now.Year
                });
                _yearRepository.Insert(new Year
                {
                    YearNumber = DateTime.Now.Year - 1
                });

            }
            List<Year> years = _yearRepository.Queryable().ToList();
            IEnumerable<WorkingDay> startWorkingDays = _workingDayRepository.Queryable();
            if (!startWorkingDays.Any())
            {
                foreach (Year year in years)
                {
                    int weekcount = UnitOfWork.YearWeekCount(year.YearNumber);
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
                            WorkingDay workday = new WorkingDay
                            {
                                IsWorking = j < 5,
                                DayOfWeek = _dayRepository.Find(j + 1)
                            };
                            workdays.Add(workday);
                            workingWeek.WorkingDays.Add(workday);
                        }

                        _workingDayRepository.InsertRange(workdays);

                        year.WorkingWeeks.Add(workingWeek);
                        _yearRepository.Update(year);
                    }
                    _workingWeekRepository.InsertRange(workweeks);
                }
            }
            Assert.IsTrue(_workingDayRepository.Queryable().Any());
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
                    IdentityRole role = _userRoleRepository.Queryable().FirstOrDefault(r => string.Equals(r.Name, "Employee"));
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
                        if (role != null) user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = user.Id });
                        _userRepository.Insert(user);
                    }
                    Assert.IsTrue(_userRepository.Queryable().Any());
                    //_db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        System.Diagnostics.Debug.Print("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            System.Diagnostics.Debug.Write(ve.ErrorMessage, ve.PropertyName);
                        }
                    }
                    throw;
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
            Dish[] dishArray = GetDishesFromXML();
            string[] categories = _dishtypeRepository.Queryable().OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();

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

            Year year = _yearRepository.Queryable().FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            bool weekLessZero = false;
            Year correct_year = _yearRepository.Queryable().FirstOrDefault(y => y.YearNumber == DateTime.Now.Year - 1);
            int correct_week = 0;
            List<MenuForWeek> menus = new List<MenuForWeek>();
            //_db.Set(typeof (WorkingDay)).Include("DayOfWeek").Include("WorkingWeek").Load();
            List<WorkingDay> workdays = _workingDayRepository.Queryable().Include("DayOfWeek").Include("WorkingWeek").ToList();
            List<WorkingWeek> workweeks = _workingWeekRepository.Queryable().Include("WorkingDays").ToList();
            for (int week = 0; week < 25; week++)
            {
                int curweek = UnitOfWork.CurrentWeek() - week + correct_week;
                weekLessZero = UnitOfWork.CurrentWeek() - week <= 0;
                if (weekLessZero)
                {
                    year = correct_year;
                    correct_week = UnitOfWork.YearWeekCount(DateTime.Now.Year - 1);
                }
                List<MenuForDay> mfdays = new List<MenuForDay>();
                WorkingWeek workweek = _workingWeekRepository.Queryable().FirstOrDefault(
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
                //context.MenuForWeeks.AddOrUpdate(m => m.WorkingWeek, new MenuForWeek
                //{
                //    MenuForDay = mfdays,
                //    WorkingWeek = workweek,
                //    SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum()
                //});
            }
            Assert.IsTrue(menus.Count == 25);
        }

        private Dish[] GetDishesFromXML()
        {
            string userspath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
            var xml = XDocument.Load(userspath);
            var collection = xml.Root.Descendants("dish");

            List<DishType> dtList = _dishtypeRepository.Queryable().ToList();

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
            try
            {

                Dish[] dishes = (from el in collection.AsEnumerable()
                                 select new Dish
                                 {
                                     DishType = getDishType(el.Attribute("dishtype").Value),
                                     Title = el.Attribute("title").Value,
                                     Description = el.Element("description").Value,
                                     ProductImage = el.Attribute("image").Value,
                                     Price = parseDouble(el.Element("cost").Value),
                                     DishDetail = new DishDetail
                                     {
                                         Title = el.Attribute("title").Value,
                                         Foods = el.Element("foods").Value,
                                         Recept = el.Element("recept").Value
                                     }
                                 }).ToArray();

                //context.Dishes.AddOrUpdate(c => c.Title, dishes);
                return dishes;
            }
            catch (NullReferenceException ex)
            {
                throw;
            }

        }
    }
}
