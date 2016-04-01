using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Xml.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Infrastructure;
using ACSDining.Infrastructure.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Infrastructure.Identity
{
    public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {

        private static Random rand = new Random();

        protected override void Seed(ApplicationDbContext context)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
                System.Diagnostics.Debugger.Launch();

            
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") +
                                      @"ACSDining.Core\DBinitial\DishDetails.xml";

            InitializeIdentityForEF(context, _path); var dishes = GetDishesFromXML(context, _path);
            CreateWorkingDays(context);
            CreateMenuForWeek(context, dishes);
            _path = _path.Replace(@"DishDetails", "Employeers");
            GetUsersFromXml(context, _path);
            CreateOrders(context);
            base.Seed(context);
        }

        public static void AddUser(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            //IdentityRole adminrole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Administrator")) ??
            //                        new UserRole
            //                        {
            //                            Name = "Administrator",
            //                            Description = "All Rights in Application",
            //                            ObjectState = ObjectState.Added
            //                        };
            //PasswordHasher hasher = new PasswordHasher();
            //User useradmin = new User
            //{
            //    UserName = "admin",
            //    Email = "test@test.com",
            //    FirstName = "Admin",
            //    LastName = "User",
            //    LastLoginTime = DateTime.UtcNow,
            //    RegistrationDate = DateTime.UtcNow,
            //    PasswordHash = hasher.HashPassword("777123"),
            //    ObjectState = ObjectState.Added
            //};

            //useradmin.Roles.Add(new UserRoleRelation
            //{
            //    RoleId = adminrole.Id,
            //    UserId = useradmin.Id,
            //    ObjectState = ObjectState.Added
            //});
            User useradmin = userManager.FindByName("admin");
            if (useradmin == null)
            {
                useradmin = new User
                {
                    UserName = "admin",
                    Email = "test@test.com",
                    FirstName = "Admin",
                    LastName = "User",
                    // IsDiningRoomClient = true,
                    LastLoginTime = DateTime.UtcNow,
                    RegistrationDate = DateTime.UtcNow,
                    ObjectState = ObjectState.Added
                };
                var adminresult = userManager.Create(useradmin, "777123");
                if (adminresult.Succeeded)
                {
                    userManager.AddToRole(useradmin.Id, "Administrator");
                }
            }
            context.Users.Add(useradmin);
            if (!roleManager.RoleExists("Administrator"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "Administrator",
                    Description = "All Rights in Application",
                    ObjectState = ObjectState.Added
                });
            }

            context.SaveChanges();

        }
        public static void InitializeIdentityForEF(ApplicationDbContext context,string path)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Web\", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";

            context.DishQuantities.AddOrUpdate(dq => dq.Quantity,
                new DishQuantity { Quantity = 0.0 ,ObjectState = ObjectState.Added},
                new DishQuantity { Quantity = 0.5, ObjectState = ObjectState.Added },
                new DishQuantity { Quantity = 1.0, ObjectState = ObjectState.Added },
                new DishQuantity { Quantity = 2.0, ObjectState = ObjectState.Added },
                new DishQuantity { Quantity = 3.0, ObjectState = ObjectState.Added },
                new DishQuantity { Quantity = 4.0, ObjectState = ObjectState.Added },
                new DishQuantity { Quantity = 5.0, ObjectState = ObjectState.Added }
                );


            context.DishTypes.AddOrUpdate(dt => dt.Category,
                new DishType { Category = "Первое блюдо", ObjectState = ObjectState.Added },
                new DishType { Category = "Второе блюдо", ObjectState = ObjectState.Added },
                new DishType { Category = "Салат", ObjectState = ObjectState.Added },
                new DishType { Category = "Напиток", ObjectState = ObjectState.Added }
                );

            context.Days.AddOrUpdate(d => d.Name,
                new DayOfWeek { Name = "Понедельник", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Вторник", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Среда", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Четверг", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Пятница", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Суббота", ObjectState = ObjectState.Added },
                new DayOfWeek { Name = "Воскресенье", ObjectState = ObjectState.Added }
                );

            //var userManager = new ApplicationUserManager(new UserStore<User>(context));
            //var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            IdentityRole adminrole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Administrator")) ??
                                     new UserRole
                                     {
                                         Name = "Administrator",
                                         Description = "All Rights in Application",
                                         ObjectState = ObjectState.Added
                                     };
            IdentityRole surole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "SuperUser")) ??
                                  new UserRole
                                  {
                                      Name = "SuperUser",
                                      Description = "Can Update List Employee and DiningEmployee",
                                      ObjectState = ObjectState.Added
                                  };
            IdentityRole dinemprole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "DiningEmployee")) ??
                                  new UserRole
                                  {
                                      Name = "DiningEmployee",
                                      Description = "Сan edit the list of dishes in the dining room",
                                      ObjectState = ObjectState.Added
                                  };
            IdentityRole emploeerole = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee")) ??
                                      new UserRole
                                      {
                                          Name = "Employee",
                                          Description = "Сan order food in the dining room",
                                          ObjectState = ObjectState.Added
                                      };
            //if (!roleManager.RoleExists("Administrator"))
            //{
            //    roleManager.Create(new UserRole
            //    {
            //        Name = "Administrator",
            //        Description = "All Rights in Application",
            //        ObjectState = ObjectState.Added
            //    });
            //}

            //if (!roleManager.RoleExists("SuperUser"))
            //{
            //    roleManager.Create(new UserRole
            //    {
            //        Name = "SuperUser",
            //        Description = "Can Update List Employee and DiningEmployee",
            //        ObjectState = ObjectState.Added
            //    });
            //}
            //User useradmin = userManager.FindByName("admin");
            //if (useradmin == null)
            //{
            //    useradmin = new User
            //    {
            //        UserName = "admin",
            //        Email = "test@test.com",
            //        FirstName = "Admin",
            //        LastName = "User",
            //        // IsDiningRoomClient = true,
            //        LastLoginTime = DateTime.UtcNow,
            //        RegistrationDate = DateTime.UtcNow,
            //        ObjectState = ObjectState.Added
            //    };
            //    var adminresult = userManager.Create(useradmin, "777123");
            //    if (adminresult.Succeeded)
            //    {
            //        userManager.AddToRole(useradmin.Id, "Administrator");
            //    }
            //}
            PasswordHasher hasher=new PasswordHasher();
            User useradmin = new User
            {
                UserName = "admin",
                Email = "test@test.com",
                FirstName = "Admin",
                LastName = "User",
                LastLoginTime = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow,
                PasswordHash = hasher.HashPassword("777123"),
                ObjectState = ObjectState.Added
            };
            useradmin.Roles.Add(new IdentityUserRole
            {
                RoleId = adminrole.Id,
                UserId = useradmin.Id,
                ObjectState = ObjectState.Added
            });
            context.Users.Add(useradmin);

            User usersu = usersu = new User
                {
                    UserName = "su",
                    Email = "test@test.com",
                    FirstName = "Super",
                    LastName = "User",
                    LastLoginTime = DateTime.UtcNow,
                    RegistrationDate = DateTime.UtcNow,
                    PasswordHash = hasher.HashPassword("777123"),
                    ObjectState = ObjectState.Added
                };
            usersu.Roles.Add(new IdentityUserRole { RoleId = surole.Id, UserId = usersu.Id });
           // context.Users.Add(usersu);

            User userdinEmpl = new User
            {
                UserName = "diningemployee",
                Email = "test@test.com",
                FirstName = "DiningEmployee",
                LastName = "User",
                LastLoginTime = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow,
                PasswordHash = hasher.HashPassword("777123"),
                ObjectState = ObjectState.Added
            };
            userdinEmpl.Roles.Add(new IdentityUserRole { RoleId = dinemprole.Id, UserId = userdinEmpl.Id });
           // context.Users.Add(userdinEmpl);

            User userEmpl = new User
            {
                UserName = "employee",
                Email = "test@test.com",
                FirstName = "Employee",
                LastName = "User",
                LastLoginTime = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow,
                PasswordHash = hasher.HashPassword("777123"),
                ObjectState = ObjectState.Added
            };
            userEmpl.Roles.Add(new IdentityUserRole { RoleId = emploeerole.Id, UserId = userEmpl.Id });
           // context.Users.Add(userEmpl);

            context.SaveChanges();
        }

        public static Dish[] GetDishesFromXML(ApplicationDbContext context, string userspath)
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
                                     },
                                     ObjectState = ObjectState.Added
                                 }).ToArray();

                context.Dishes.AddOrUpdate(c => c.Title, dishes);
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
                    WorkingWeeks = new List<WorkingWeek>(),
                    ObjectState = ObjectState.Added
                };
                context.Years.Add(newyear);
                context.SaveChanges();
            }
            foreach (Year year in context.Years.ToList())
            {
                int weekcount = UnitOfWork.YearWeekCount(year.YearNumber);
                //List<WorkingWeek> workweeks = new List<WorkingWeek>();
                for (int i = 0; i < weekcount; i++)
                {
                    WorkingWeek workingWeek = new WorkingWeek
                    {
                        WeekNumber = i + 1,
                        Year = year,
                        WorkingDays = new List<WorkingDay>()
                    };
                   // List<WorkingDay> workdays = new List<WorkingDay>();
                    for (int j = 0; j < 7; j++)
                    {
                        WorkingDay workday = new WorkingDay
                        {
                            IsWorking = j < 5,
                            DayOfWeek = context.Days.FirstOrDefault(d => d.ID == j + 1)
                            
                        };
                       // workdays.Add(workday);
                        workday.ObjectState=ObjectState.Added;
                        workingWeek.WorkingDays.Add(workday);
                    }

                    //context.WorkingDays.AddRange(workdays);
                    workingWeek.ObjectState=ObjectState.Added;
                    year.WorkingWeeks.Add(workingWeek);
                }
                //context.WorkingWeeks.AddRange(workweeks);
                year.ObjectState=ObjectState.Modified;
            }
            context.SaveChanges();
        }

        public static void CreateMenuForWeek(ApplicationDbContext context, Dish[] dishArray)
        {
            string[] categories = context.DishTypes.OrderBy(t => t.Id).Select(dt => dt.Category).ToArray();

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

            Year year = context.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year);
            Year correct_year = context.Years.FirstOrDefault(y => y.YearNumber == DateTime.Now.Year - 1);
            int correct_week = 0;
            List<MenuForWeek> weekmenus=new List<MenuForWeek>();
            for (int week = 0; week < 25; week++)
            {
                var weekLessZero = UnitOfWork.CurrentWeek() - week <= 0;
                if (weekLessZero)
                {
                    year = correct_year;
                    correct_week = UnitOfWork.YearWeekCount(DateTime.Now.Year - 1);
                }
                List<MenuForDay> mfdays = new List<MenuForDay>();
                WorkingWeek workweek =
                    context.WorkingWeeks.Include("WorkingWeek.Year").ToList().FirstOrDefault(
                        w => year != null && (w.WeekNumber == UnitOfWork.CurrentWeek() - week + correct_week &&
                                              w.Year.YearNumber == year.YearNumber));
                for (int i = 1; i <= 7; i++)
                {
                    List<Dish> dishes = getDishes();
                    WorkingDay workday =
                        context.WorkingDays.Include("WorkingWeek").ToList().FirstOrDefault(
                            wd => workweek != null && (wd.WorkingWeek.ID == workweek.ID && wd.DayOfWeek.ID == i));

                    if (workday != null && workday.IsWorking)
                    {
                        MenuForDay dayMenu = new MenuForDay
                        {
                            Dishes = dishes,
                            WorkingDay = workday,
                            WorkingWeek = workweek,
                            TotalPrice = dishes.Select(d => d.Price).Sum(),
                            ObjectState = ObjectState.Added
                        };

                        mfdays.Add(dayMenu);

                    }
                }
                weekmenus.Add(new MenuForWeek
                {
                    MenuForDay = mfdays,
                    WorkingWeek = workweek,
                    SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum(),
                    ObjectState = ObjectState.Added
                });
                //context.MenuForWeeks.AddOrUpdate(m => m.WorkingWeek, new MenuForWeek
                //{
                //    MenuForDay = mfdays,
                //    WorkingWeek = workweek,
                //    SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum()
                //});
            }
            context.MenuForWeeks.AddRange(weekmenus);
            context.SaveChanges();
        }

        public static void GetUsersFromXml(ApplicationDbContext context, string userpath)
        {
            var xml = XDocument.Load(userpath);
            if (xml.Root != null)
            {
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
                                    ObjectState = ObjectState.Added
                                };
                        }
                        return null;
                    }).ToArray();
                    foreach (User user in users)
                    {
                        if (role != null) user.Roles.Add(new UserRoleRelation { RoleId = role.Id, UserId = user.Id });
                        context.Users.Add(user);
                    }
                    context.SaveChanges();
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

        public static void CreateOrders(ApplicationDbContext context)
        {
            List<User> users = context.Users.ToList();
            List<MenuForWeek> weekmenus = context.MenuForWeeks.ToList();
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
            foreach (User user in users)
            {
                foreach (MenuForWeek mfw in weekmenus)
                {

                    PlannedOrderMenu planorder = new PlannedOrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw,
                        ObjectState = ObjectState.Added
                    };

                    context.PlannedOrderMenus.Add(planorder);

                    OrderMenu order = new OrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw,
                        SummaryPrice = 0.0,
                        PlannedOrderMenu = planorder,
                        ObjectState = ObjectState.Added
                    };
                    context.OrderMenus.Add(order);

                    foreach (MenuForDay daymenu in mfw.MenuForDay)
                    {
                        foreach (Dish dish in daymenu.Dishes)
                        {
                            DishType first = null;
                            foreach (var dy in context.DishTypes.AsEnumerable())
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

                                rnd = rand.Next(numsForCourses[catindex]);
                                DishQuantity dqu =
                                    context.DishQuantities.ToList().FirstOrDefault(
                                        dq => dq.Quantity == coursesnums[catindex][rnd]);
                                DishQuantityRelations dqrs = new DishQuantityRelations
                                {
                                    DishQuantity = dqu,
                                    DishType = first,
                                    WorkDay = daymenu.WorkingDay,
                                    PlannedOrderMenu = planorder,
                                    MenuForWeek = mfw,
                                    OrderMenu = order,
                                    ObjectState = ObjectState.Added
                                };
                                order.SummaryPrice += dqu.Quantity * dish.Price;
                                dquaList.Add(dqrs);
                            }

                        }
                    }
                }
            }
            context.DQRelations.AddRange(dquaList);
            context.SaveChanges();
        }
    }
}
