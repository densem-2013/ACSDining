using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using ACSDining.Core.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace ACSDining.Core.Identity
{

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Core.Identity and is used by the application.

    public class ApplicationUserManager : UserManager<User>
    {
        public ApplicationUserManager(IUserStore<User> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<User>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<User>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<User>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<User>(dataProtectionProvider.Create("ASP.NET Core.Identity"));
            }
            return manager;
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Core.Identity core assembly
    public class ApplicationRoleManager : RoleManager<UserRole>
    {
        public ApplicationRoleManager(IRoleStore<UserRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<UserRole>(context.Get<ApplicationDbContext>()));
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // This is useful if you do not want to tear down the database each time you run the application.
    // public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    // This example shows you how to create a new database if the Model changes
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        private static string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Web\", "") +
                                      @"ACSDining.Core\DBinitial\DishDetails.xml";

        private static Random rand = new Random();

        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            var dishes = GetDishesFromXML(context, _path);
            CreateMenuForWeek(context, dishes);
            _path = _path.Replace(@"DishDetails", "Employeers");
            GetUsersFromXml(context, _path);
            CreateOrders(context);
            base.Seed(context);
        }

        public static void InitializeIdentityForEF(ApplicationDbContext context)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Web\", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";

            //context.DishQuantities.AddOrUpdate(dq => dq.Quantity, 
            //    new DishQuantity{ Quantity = 0.0}, 
            //    new DishQuantity { Quantity = 0.5 }, 
            //    new DishQuantity { Quantity = 1.0 },
            //    new DishQuantity { Quantity = 1.5 }, 
            //    new DishQuantity { Quantity = 2.0 }, 
            //    new DishQuantity { Quantity = 2.5 },
            //    new DishQuantity { Quantity = 3.0 }, 
            //    new DishQuantity { Quantity = 3.5 }, 
            //    new DishQuantity { Quantity = 4.0 }, 
            //    new DishQuantity { Quantity = 4.5 },
            //    new DishQuantity { Quantity = 5.0 }
            //);
            //var saveres = context.SaveChangesAsync();
            //saveres.Wait();
            context.Years.AddOrUpdate(y => y.YearNumber, new Year
            {
                YearNumber = DateTime.Now.Year
            });

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
                new DayOfWeek {Name = "Пятница"}
                );


            var userManager = new ApplicationUserManager(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!roleManager.RoleExists("Administrator"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "Administrator",
                    Description = "All Rights in Application"
                });
            }

            if (!roleManager.RoleExists("SuperUser"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "SuperUser",
                    Description = "Can Update List Employee and DiningEmployee"
                });
            }
            User useradmin = userManager.FindByName("admin");
            if (useradmin == null)
            {
                useradmin = new User
                {
                    UserName = "admin",
                    Email = "test@test.com",
                    FirstName = "Admin",
                    LastName = "User",
                    IsDiningRoomClient = true,
                    RegistrationDate = DateTime.UtcNow
                };
                var adminresult = userManager.Create(useradmin, "777123");
                if (adminresult.Succeeded)
                {
                    userManager.AddToRole(useradmin.Id, "Administrator");
                }
            }
            User usersu = userManager.FindByName("su");
            if (usersu == null)
            {
                usersu = new User
                {
                    UserName = "su",
                    Email = "test@test.com",
                    FirstName = "Super",
                    LastName = "User",
                    IsDiningRoomClient = true,
                    RegistrationDate = DateTime.UtcNow
                };
                var suresult = userManager.Create(usersu, "777123");
                if (suresult.Succeeded)
                {
                    userManager.AddToRole(usersu.Id, "SuperUser");
                }
            }

            if (!roleManager.RoleExists("DiningEmployee"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "DiningEmployee",
                    Description = "Сan edit the list of dishes in the dining room"
                });
            }
            User userdinEmpl = userManager.FindByName("diningemployee");
            if (userdinEmpl == null)
            {
                userdinEmpl = new User
                {
                    UserName = "diningemployee",
                    Email = "test@test.com",
                    FirstName = "DiningEmployee",
                    LastName = "User",
                    IsDiningRoomClient = true,
                    RegistrationDate = DateTime.UtcNow
                };
                var result = userManager.Create(userdinEmpl, "777123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(userdinEmpl.Id, "DiningEmployee");
                }
            }

            if (!roleManager.RoleExists("Employee"))
            {
                roleManager.Create(new UserRole
                {
                    Name = "Employee",
                    Description = "Сan order food in the dining room"
                });
            }
            User userEmpl = userManager.FindByName("employee");
            if (userEmpl == null)
            {
                userEmpl = new User
                {
                    UserName = "employee",
                    Email = "test@test.com",
                    FirstName = "Employee",
                    LastName = "User",
                    IsDiningRoomClient = true,
                    RegistrationDate = DateTime.UtcNow
                };
                var result = userManager.Create(userEmpl, "777123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(userEmpl.Id, "Employee");
                }
            }

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
                        }
                    }).ToArray();

                context.Dishes.AddOrUpdate(c => c.Title, dishes);
                return dishes;
            }
            catch (NullReferenceException ex)
            {
                throw;
            }

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

            for (int week = 0; week < 15; week++)
            {
                List<MenuForDay> mfdays = new List<MenuForDay>();

                for (int i = 1; i <= 5; i++)
                {
                    List<Dish> dishes = getDishes();
                    MenuForDay dayMenu = new MenuForDay
                    {
                        Dishes = dishes,
                        DayOfWeek = context.Days.FirstOrDefault(day => day.ID == i),
                        TotalPrice = dishes.Select(d => d.Price).Sum()
                    };

                    mfdays.Add(dayMenu);
                }
                context.MenuForWeek.AddOrUpdate(m => m.WeekNumber, new MenuForWeek
                {
                    Year = year,
                    MenuForDay = mfdays,
                    WeekNumber = context.CurrentWeek() - week,
                    SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum()
                });
            }
        }

        public static void GetUsersFromXml(ApplicationDbContext context, string userpath)
        {
            var xml = XDocument.Load(userpath);
            var collection = xml.Root.Descendants("Employeer");
            try
            {
                var hasher = new PasswordHasher();
                IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee"));
                User[] users = collection.AsEnumerable().Select(el => new User
                {
                    FirstName = el.Element("FirstName").Value,
                    LastName = el.Element("LastName").Value,
                    UserName = string.Format("{0} {1}", el.Element("LastName").Value, el.Element("FirstName").Value),
                    PasswordHash = hasher.HashPassword("777123"),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    IsDiningRoomClient = true,
                    RegistrationDate = DateTime.Now
                }).ToArray();
                foreach (User user in users)
                {
                    if (role != null) user.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = user.Id});
                    context.Users.Add(user);
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void CreateOrders(ApplicationDbContext context)
        {
            List<User> users = context.Users.ToList();
            List<MenuForWeek> weekmenus = context.MenuForWeek.ToList();
            int rnd;

            double[][] coursesnums =
            {
                new[] {0, 0.5, 0.5, 1.0, 1.0, 1.0, 1.5},
                new[] {0, 0, 1.0, 1.0, 1.0, 1.0, 2.0},
                new[] {0, 1.0},
                new[] {0, 1.0}
            };
            int[] numsForCourses = new int[4];

            for (int i = 0; i < 4; i++)
            {
                numsForCourses[i] = coursesnums[i].Length;
            }
            foreach (User user in users)
            {
                foreach (MenuForWeek mfw in weekmenus)
                {

                    OrderMenu order = new OrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw,
                        SummaryPrice = 0.0
                    };
                    context.OrderMenu.Add(order);
                    List<DishQuantity> dquaList = new List<DishQuantity>();
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
                                DishQuantity dqu = new DishQuantity
                                {
                                    Quantity = coursesnums[catindex][rnd],
                                    DishType = dish.DishType,
                                    DayOfWeek = daymenu.DayOfWeek,
                                    MenuForWeek = mfw,
                                    OrderMenu = order
                                };
                                order.SummaryPrice += dqu.Quantity*dish.Price;
                                dquaList.Add(dqu);
                            }

                        }
                    }
                    context.DishQuantities.AddRange(dquaList);
                    context.SaveChanges();
                }
            }
        }
    }

    public class ApplicationSignInManager : SignInManager<User, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
