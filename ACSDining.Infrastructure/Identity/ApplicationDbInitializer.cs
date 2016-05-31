using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.HelpClasses;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Infrastructure.Identity
{
    public class DishHelp
    {
        public DishType DishType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public double Price { get; set; }
        public DishDetail DishDetail { get; set; }
    }
    //public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
        public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {

        private static readonly Random Rand = new Random();

        protected override void Seed(ApplicationDbContext context)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //    System.Diagnostics.Debugger.Launch();

            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") +
            //                          @"ACSDining.Web\App_Data\DBinitial\DishDetails.xml";

            //string _path = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\DBinitial\DishDetails.xml";

            string _path = HostingEnvironment.MapPath("~/App_Data/DBinitial/DishDetails.xml");

            InitializeIdentityForEf(context, _path);
            var dishes = GetDishesFromXml(context, _path);
            CreateWorkingDays(context);
            CreateMenuForWeek(context, dishes);
            _path = _path.Replace(@"DishDetails", "Employeers");
            GetUsersFromXml(context, _path);
            CreateOrders(context);
            _path = _path.Replace(@"Employeers.xml", "storedfunc.sql");
            //_path = _path.Replace(@"DishDetails.xml", "storedfunc.sql");
            Utility.CreateStoredFuncs(_path);
            base.Seed(context);
        }

        public static void AddUser( ApplicationDbContext context)
        {

            var userManager = new ApplicationUserManager(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!roleManager.RoleExists("SuperUser"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "SuperUser",
                    Description = "Can Update List Employee and DiningEmployee"
                });
            }
           // WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.MapPath("~/Web.config")).AppSettings.Settings["defaultCreditValue"].Value
            
            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //                          @"ACSDining.Web\Web.config";
            //var config = ConfigurationManager.AppSettings.(_path);
            string sulogin = ConfigurationManager.AppSettings["sulogin"];
            User usersu = userManager.FindByName(sulogin);
            if (usersu == null)
            {
                usersu = new User
                {
                    UserName = sulogin,
                    Email = "test@test.com",
                    FirstName = "Super",
                    LastName = "User",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LastLoginTime = DateTime.UtcNow,
                    RegistrationDate = DateTime.UtcNow,
                    PasswordHash = userManager.PasswordHasher.HashPassword(ConfigurationManager.AppSettings["supass"])
                };
                IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "SuperUser"));
                usersu.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = usersu.Id });
               // var suresult = userManager.Create(usersu, ConfigurationManager.AppSettings["supass"]);
                context.Entry(usersu).State=EntityState.Added;
                context.SaveChanges();
            }

        }

            public static void InitializeIdentityForEf(ApplicationDbContext context, string path)
            {

                context.DishQuantities.AddOrUpdate(dq => dq.Quantity,
                    new DishQuantity {Quantity = 0.0},
                    new DishQuantity {Quantity = 0.5},
                    new DishQuantity {Quantity = 1.0},
                    new DishQuantity {Quantity = 2.0},
                    new DishQuantity {Quantity = 3.0},
                    new DishQuantity {Quantity = 4.0},
                    new DishQuantity {Quantity = 5.0}
                    );


                context.DishTypes.AddOrUpdate(dt => dt.Category,
                    new DishType {Category = "Первое блюдо"},
                    new DishType {Category = "Второе блюдо"},
                    new DishType {Category = "Салат"},
                    new DishType {Category = "Напиток"}
                    );

                context.Days.AddOrUpdate(d => d.Name,
                    new DayOfWeek {Name = "Понедельник"},
                    new DayOfWeek {Name = "Вторник"},
                    new DayOfWeek {Name = "Среда"},
                    new DayOfWeek {Name = "Четверг"},
                    new DayOfWeek {Name = "Пятница"},
                    new DayOfWeek {Name = "Суббота"},
                    new DayOfWeek {Name = "Воскресенье"}
                    );


                var userManager = new ApplicationUserManager(new UserStore<User>(context));
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));


                if (!roleManager.RoleExists("SuperUser"))
                {
                    roleManager.Create(new UserRole
                    {
                        Name = "SuperUser",
                        Description = "Can Update List Employee and DiningEmployee"
                    });
                }
                if (!roleManager.RoleExists("Employee"))
                {
                    roleManager.Create(new UserRole
                    {
                        Name = "Employee",
                        Description = "Сan order food in the dining room"
                    });
                }


                string sulogin = WebConfigurationManager.AppSettings["sulogin"];
                User usersu = userManager.FindByName(sulogin);
                if (usersu == null)
                {
                    usersu = new User
                    {
                        UserName = sulogin,
                        Email = "test@test.com",
                        FirstName = "admin",
                        LastName = "super",
                        SecurityStamp = Guid.NewGuid().ToString(),
                        LastLoginTime = DateTime.UtcNow,
                        RegistrationDate = DateTime.UtcNow,
                        PasswordHash =
                            userManager.PasswordHasher.HashPassword(WebConfigurationManager.AppSettings["supass"])
                    };
                    IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "SuperUser"));
                    usersu.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = usersu.Id});
                    context.Entry(usersu).State = EntityState.Added;
                }


                User userEmpl = userManager.FindByName("employee");
                if (userEmpl == null)
                {
                    userEmpl = new User
                    {
                        UserName = "employee",
                        Email = "densem-2013@yandex.ua",
                        FirstName = "Employee",
                        LastName = "User",
                        LastLoginTime = DateTime.UtcNow,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        RegistrationDate = DateTime.UtcNow,
                        AllowableDebt = 200,
                        PasswordHash = userManager.PasswordHasher.HashPassword("777123")
                    };
                    IdentityRole emplrole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee"));
                    userEmpl.Roles.Add(new IdentityUserRole {RoleId = emplrole.Id, UserId = userEmpl.Id});
                    context.Entry(userEmpl).State = EntityState.Added;
                }

                //пустые блюда для каждой категории
                List<DishType> dishTypes = context.DishTypes.OrderBy(dt => dt.Id).ToList();
                //пустые блюда для каждой категории
                DishPrice nulldp = new DishPrice {Price = 0.00};
                context.Dishes.AddRange(dishTypes.Select(dt => new Dish
                {
                    DishType = dt,
                    CurrentPrice = nulldp
                }).ToArray());
                context.SaveChanges();
            }

            public static DishHelp[] GetDishesFromXml(ApplicationDbContext context, string userspath)
            {

                var xml = XDocument.Load(userspath);
                var collection = xml.Root.Descendants("dish");

                List<DishType> dtList = context.DishTypes.ToList();

                Func<string, DishType> getDishType =
                    el1 =>
                    {
                        DishType dtype = dtList.FirstOrDefault(dt => string.Equals(el1, dt.Category));

                        return dtype;
                    };

                Func<string, double> parseDouble = str =>
                {
                    double val = double.Parse(str);
                    return val > 30.0 ? val/100 : val;
                };

               
                try
                {
                    DishHelp[] dishes = collection.AsEnumerable().Select(el =>
                    {
                        DishHelp dish = new DishHelp
                        {
                            DishType = getDishType(el.Attribute("dishtype").Value),
                            Title = el.Attribute("title").Value,
                            Description = el.Element("description").Value,
                            ProductImage = el.Attribute("image").Value,
                            DishDetail = new DishDetail
                            {
                                Title = el.Attribute("title").Value,
                                Foods = el.Element("foods").Value,
                                Recept = el.Element("recept").Value
                            },
                            Price = parseDouble(el.Element("cost").Value)
                        };
                        return dish;
                    }).ToArray();

                    double[] prices = dishes.Select(d => d.Price).Distinct().ToArray();
                    context.DishPrices.AddRange(prices.Select(pr => new DishPrice { Price = pr })
                            .Concat(new[] { new DishPrice { Price = 0.00 } })
                            .ToArray());

                    context.SaveChanges();

                    DishPrice[] dishPrices = context.DishPrices.ToArray();

                    Func<double, DishPrice> getDishPrice = (price) =>
                    {
                        return dishPrices.FirstOrDefault(d => Math.Abs(price - d.Price) < 0.001);
                    };

                   context.Dishes.AddRange(dishes.Select(d => new Dish
                    {
                        DishType = d.DishType,
                        Title = d.Title,
                        Description = d.Description,
                        ProductImage = d.ProductImage,
                        DishDetail = d.DishDetail,
                        CurrentPrice = getDishPrice(d.Price)
                    }).ToArray());
                    context.SaveChanges();
                    return dishes;
                }
                catch (NullReferenceException ex)
                {
                    throw;
                }

            }

            public static void CreateWorkingDays(ApplicationDbContext context)
        {
            for (int i = 0; i < 2; i++)
            {
                Year newyear = new Year
                {
                    YearNumber = DateTime.Now.Year - i,
                    WorkingWeeks = new List<WorkingWeek>()
                };
                context.Years.Add(newyear);
                context.SaveChanges();
            }
            foreach (Year year in context.Years.ToList())
            {
                int weekcount = YearWeekHelp.YearWeekCount(year.YearNumber);

                for (int i = 0; i < weekcount; i++)
                {
                    WorkingWeek workingWeek = new WorkingWeek
                    {
                        WeekNumber = i + 1,
                        Year = year,
                        WorkingDays = new List<WorkingDay>(),
                        CanBeChanged = true
                    };
                    for (int j = 0; j < 7; j++)
                    {
                        WorkingDay workday = new WorkingDay
                        {
                            IsWorking = j < 5,
                            DayOfWeek = context.Days.FirstOrDefault(d => d.Id == j + 1)

                        };
                        workingWeek.WorkingDays.Add(workday);
                    }

                    year.WorkingWeeks.Add(workingWeek);
                }

            }
            context.SaveChanges();
        }

        public static void CreateMenuForWeek(ApplicationDbContext context, DishHelp[] dishArray)
        {
            string[] categories = context.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();
            Dish[] darray = context.Dishes.Where(d => d.Title != null).ToArray();
            DishPrice[] dishPrices = context.DishPrices.ToArray();
            Func<string, IEnumerable<DishHelp>, int> countDish = (str, list) =>
            {
                int coun = list.Count(el => string.Equals(el.DishType.Category, str));
                return coun;
            };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => countDish(count, dishArray));
            Func<List<Dish>> getDishes = () =>
            {
                return
                    catCount.Select(
                        pair =>
                            darray.Where(d => string.Equals(d.DishType.Category, pair.Key))
                                .ElementAt(Rand.Next(pair.Value))).ToList();
            };
            Func<double,DishPrice> getDishPrice=(price) =>
            {
                return dishPrices.FirstOrDefault(d => Math.Abs(price - d.Price) < 0.001);
            };

            Year year = context.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            Year correct_year = context.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year - 1);
            int correct_week = 0;
            List<MenuForWeek> weekmenus = new List<MenuForWeek>();
            List<MfdDishPriceRelations> mfdDishPriceRelationses=new List<MfdDishPriceRelations>();
            for (int week = 0; week < 5; week++)
            {
                var weekLessZero = YearWeekHelp.CurrentWeek() - week <= 0;
                if (weekLessZero)
                {
                    year = correct_year;
                    correct_week = YearWeekHelp.YearWeekCount(DateTime.Now.Year - 1);
                }
                List<MenuForDay> mfdays = new List<MenuForDay>();
                WorkingWeek workweek =
                    context.WorkingWeeks.Include("Year").ToList().FirstOrDefault(
                        w => year != null && (w.WeekNumber == YearWeekHelp.CurrentWeek() - week + correct_week &&
                                              w.Year.YearNumber == year.YearNumber));

                //bool ordCanCreated = true; //
                for (int i = 1; i <= 7; i++)
                {
                    List<Dish> dishes = getDishes();
                    dishes.OrderBy(d => d.DishType.Id);
                    WorkingDay workday =
                        context.WorkingDays.Include("WorkingWeek").ToList().FirstOrDefault(
                            wd => workweek != null && (wd.WorkingWeek.ID == workweek.ID && wd.DayOfWeek.Id == i));

                    if (workday != null /* && workday.IsWorking*/)
                    {
                        MenuForDay dayMenu = new MenuForDay
                        {
                            //Dishes = dishes,
                            WorkingDay = workday,
                            //TotalPrice = dishes.Select(d => d.Price).Sum(),
                            DayMenuCanBeChanged =
                                week == 0 && ((int) DateTime.Now.DayOfWeek) >= i - 1 && DateTime.Now.Hour < 9,
                            OrderCanBeChanged = true//week == 0 && ((int)DateTime.Now.DayOfWeek) >= i - 1 && DateTime.Now.Hour < 9
                        };
                        mfdDishPriceRelationses.AddRange(dishes.Select(d=>new MfdDishPriceRelations
                        {
                            Dish = d,
                            MenuForDay = dayMenu,
                            DishPrice = d.CurrentPrice
                        }));
                        dayMenu.TotalPrice =
                            mfdDishPriceRelationses.Where(mdr => mdr.MenuForDay == dayMenu)
                                .Sum(mdr => mdr.Dish.CurrentPrice.Price);
                        mfdays.Add(dayMenu);

                    }
                }
                weekmenus.Add(new MenuForWeek
                {
                    MenuForDay = mfdays,
                    WorkingWeek = workweek,
                    SummaryPrice = mfdays.Where(mfd => mfd.WorkingDay.IsWorking).Select(d => d.TotalPrice).Sum(),
                    OrderCanBeCreated = true,
                    SUCanChangeOrder = week == 0,
                    WorkingDaysAreSelected = true
                });
            }
            context.MenuForWeeks.AddRange(weekmenus);
            context.MfdDishPriceRelations.AddRange(mfdDishPriceRelationses);
            context.SaveChanges();
        }

        public static void GetUsersFromXml(ApplicationDbContext context, string userpath)
        {
            var xml = XDocument.Load(userpath);
            if (xml.Root != null)
            {
                //System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(HostingEnvironment.MapPath("~/Web.config")); //Path to your config fileWebConfigurationManager.AppSettings["sulogin"]
                //System.Configuration.Configuration configuration = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
                string configpath = userpath.Replace("App_Data/DBinitial/DishDetails.xml", "Web.config");
                double defaultDebt;
                //double.TryParse(WebConfigurationManager.OpenWebConfiguration(configpath).AppSettings.Settings["defaultCreditValue"].Value, out defaultDebt);double defaultDebt;
                double.TryParse(WebConfigurationManager.AppSettings["defaultCreditValue"], out defaultDebt);
                var collection = xml.Root.Descendants("Employeer");
                try
                {
                    var hasher = new PasswordHasher();
                    IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee"));
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
                                    LastLoginTime = DateTime.UtcNow,
                                    IsExisting = true,
                                    CanMakeBooking = true,
                                    AllowableDebt = defaultDebt
                                };
                        }
                        return null;
                    }).ToArray();
                    foreach (User user in users)
                    {
                        if (role != null) user.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = user.Id});
                        context.Users.Add(user);
                    }
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        System.Diagnostics.Debug.Print(
                            "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
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

        public static void CreateOrders(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            IdentityRole role = roleManager.FindByName("Employee");
            List<User> users = context.Users.Where(u=>u.Roles.Select(r=>r.RoleId).Contains(role.Id)).ToList();
            List<MenuForWeek> weekmenus = context.MenuForWeeks.ToList();
            List<DishQuantity> dquaQuantities = context.DishQuantities.ToList();
            List<WeekOrderMenu> weekOrders=new List<WeekOrderMenu>();
            List<PlannedWeekOrderMenu> plannedWeekOrders=new List<PlannedWeekOrderMenu>();
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
            List<PlanDishQuantityRelations> plandquaList = new List<PlanDishQuantityRelations>();
            foreach (User user in users)
            {
                foreach (MenuForWeek mfw in weekmenus)
                {
                    WeekOrderMenu weekOrder = new WeekOrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw,
                        WeekOrderSummaryPrice = 0.0
                    };
                    PlannedWeekOrderMenu plannedWeekOrderMenu = new PlannedWeekOrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw,
                        WeekOrderSummaryPrice = 0.0
                    };

                    List<DayOrderMenu> dayOrderMenus = new List<DayOrderMenu>();
                    List<PlannedDayOrderMenu> plandayOrderMenus = new List<PlannedDayOrderMenu>();

                    List<DishType> dishTypes = context.DishTypes.ToList();
                    foreach (MenuForDay daymenu in mfw.MenuForDay)
                    {
                        DayOrderMenu dayOrderMenu = new DayOrderMenu
                        {
                            MenuForDay = daymenu,
                        };
                        PlannedDayOrderMenu plannedDayOrderMenu = new PlannedDayOrderMenu
                        {
                            MenuForDay = daymenu
                        };
                        foreach (MfdDishPriceRelations mdrs in context.MfdDishPriceRelations.Include("DishPrice").Include("Dish.DishType").Where(mdpr => mdpr.MenuForDayId == daymenu.ID).ToList())
                        {
                            DishType first = dishTypes.FirstOrDefault(dy => dy.Id == mdrs.Dish.DishType.Id);
                            if (first != null)
                            {
                                int catindex = first.Id - 1;

                                rnd = Rand.Next(numsForCourses[catindex]);
                                DishQuantity dqu = dquaQuantities.FirstOrDefault(
                                        dq => dq.Quantity == coursesnums[catindex][rnd]);
                                DishQuantityRelations dqrs = new DishQuantityRelations
                                {
                                    DishQuantity = dqu,
                                    DishType = first,
                                    DayOrderMenu = dayOrderMenu
                                };
                                PlanDishQuantityRelations plandqrs = new PlanDishQuantityRelations
                                {
                                    DishQuantity = dqu,
                                    DishType = first,
                                    PlannedDayOrderMenu = plannedDayOrderMenu
                                };
                                if (dqu != null) dayOrderMenu.DayOrderSummaryPrice += dqu.Quantity * mdrs.DishPrice.Price;
                                dquaList.Add(dqrs);
                                plandquaList.Add(plandqrs);
                            }

                        }
                        //dayOrderMenu.DayOrderSummaryPrice=
                        plannedDayOrderMenu.DayOrderSummaryPrice = dayOrderMenu.DayOrderSummaryPrice;
                        dayOrderMenus.Add(dayOrderMenu);
                        plandayOrderMenus.Add(plannedDayOrderMenu);
                    }
                    weekOrder.DayOrderMenus = dayOrderMenus;
                    plannedWeekOrderMenu.PlannedDayOrderMenus = plandayOrderMenus;
                    weekOrder.WeekOrderSummaryPrice =
                        weekOrder.DayOrderMenus.Where(dom => dom.MenuForDay.WorkingDay.IsWorking)
                            .Sum(dom => dom.DayOrderSummaryPrice);

                    plannedWeekOrderMenu.WeekOrderSummaryPrice = weekOrder.WeekOrderSummaryPrice;

                    weekOrders.Add(weekOrder);
                    plannedWeekOrders.Add(plannedWeekOrderMenu);
                }
            }
            List<WeekPaiment> weekPaiments=new List<WeekPaiment>();
            weekOrders.ForEach(x =>
            {
                weekPaiments.Add(new WeekPaiment
                {
                    Paiment = x.WeekOrderSummaryPrice,
                    WeekIsPaid = true,
                    WeekOrderMenu = x
                });
            });
            context.WeekPaiments.AddRange(weekPaiments);
            context.WeekOrderMenus.AddRange(weekOrders);
            context.PlannedWeekOrderMenus.AddRange(plannedWeekOrders);
            context.DQRelations.AddRange(dquaList);
            context.PlanDQRelations.AddRange(plandquaList);
            context.SaveChanges();
        }

    }
}
