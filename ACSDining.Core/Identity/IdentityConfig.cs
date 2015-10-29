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
        static string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Web\", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
        static Random rand = new Random();
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            var dishes = GetDishesFromXML(context,path);
            CreateMenuForWeek(context, dishes);
            path = path.Replace(@"DishDetails", "Employeers");
            GetUsersFromXML(context,path);
            CreateOrders(context);
            base.Seed(context);
        }

        public static  void InitializeIdentityForEF(ApplicationDbContext context)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Web\", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";

            context.DishQuantities.AddOrUpdate(dq => dq.Quantity, 
                new DishQuantity{ Quantity = 0.0}, 
                new DishQuantity { Quantity = 0.5 }, 
                new DishQuantity { Quantity = 1.0 },
                new DishQuantity { Quantity = 1.5 }, 
                new DishQuantity { Quantity = 2.0 }, 
                new DishQuantity { Quantity = 2.5 },
                new DishQuantity { Quantity = 3.0 }, 
                new DishQuantity { Quantity = 3.5 }, 
                new DishQuantity { Quantity = 4.0 }, 
                new DishQuantity { Quantity = 4.5 },
                new DishQuantity { Quantity = 5.0 }
            );
            //var saveres = context.SaveChangesAsync();
            //saveres.Wait();
            context.Years.AddOrUpdate(y => y.YearNumber, new Year
            {
                YearNumber = DateTime.Now.Year
            });
            //saveres = context.SaveChangesAsync();
            //saveres.Wait();
            context.DishTypes.AddOrUpdate(dt => dt.Category, 
                new DishType { Category = "Первое блюдо" }, 
                new DishType { Category = "Второе блюдо" }, 
                new DishType { Category = "Салат" }, 
                new DishType { Category = "Десерт" }, 
                new DishType { Category = "Напиток" }
                );
            //saveres = context.SaveChangesAsync();
            //saveres.Wait();
            context.Days.AddOrUpdate(d => d.Name, 
                new DayOfWeek{  Name="Понедельник"}, 
                new DayOfWeek{  Name="Вторник"}, 
                new DayOfWeek{  Name="Среда"}, 
                new DayOfWeek{  Name="Четверг"}, 
                new DayOfWeek{  Name="Пятница"}
                );

            //context.SaveChanges();
            //saveres.Wait();

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
                var suresult=userManager.Create(usersu, "777123");
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
        public static Dish[] GetDishesFromXML( ApplicationDbContext context, string userspath)
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
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };

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
                    ds.Add(dishArray.Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
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

        public static void GetUsersFromXML(ApplicationDbContext context, string userpath)
        {

            //string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", "") + @"DBinitial\Employeers.xml";
            var xml = XDocument.Load(userpath);
            var collection = xml.Root.Descendants("Employeer");
            try
            {
                User[] users = (from el in collection.AsEnumerable()
                    select new User
                    {
                        FirstName = el.Element("FirstName").Value,
                        LastName = el.Element("LastName").Value,
                        IsDiningRoomClient = true,
                        RegistrationDate = DateTime.Now
                    }).ToArray();

                var hasher = new PasswordHasher();
                for (int i = 0; i < users.Length; i++)
                {
                    users[i].UserName = users[i].LastName + " " + users[i].FirstName;
                    //Create Employee if it does not exist
                    var userEmployee = context.Users.AsEnumerable().FirstOrDefault(u => string.Equals(u.UserName, users[i].UserName));

                    if (userEmployee == null)
                    {
                        userEmployee = new User
                        {
                            UserName = users[i].UserName,
                            Email = "test@test.com",
                            FirstName = users[i].FirstName,
                            PasswordHash = hasher.HashPassword("777123"),
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            LastName = users[i].LastName,
                            IsDiningRoomClient = true,
                            RegistrationDate = DateTime.UtcNow
                        };
                        IdentityRole role = context.Roles.FirstOrDefault(r => string.Equals(r.Name, "Employee"));
                        userEmployee.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = userEmployee.Id});
                        context.Users.Add(userEmployee);
                    }


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
            int rnd = 0;
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };
            double[][] coursesnums = { 
            new[]{ 0, 0.5, 0.5, 1.0, 1.0, 1.0, 1.5 }, 
            new[]{ 0, 0, 1.0, 1.0, 1.0, 1.0, 2.0 }, 
            new[]{ 0, 1.0 }, 
            new[]{ 0, 1.0 } 
            };
            Dictionary<string, int> numsForCourses = new Dictionary<string, int>();

            for (int i = 0; i < 4; i++)
            {
                numsForCourses.Add(categories[i], coursesnums[i].Length);
            };
            foreach (User user in users)
            {
                foreach (MenuForWeek mfw in weekmenus)
                {
                    List<DishQuantity> quantyties = new List<DishQuantity>();
                    OrderMenu order = new OrderMenu
                    {
                        User = user,
                        MenuForWeek = mfw
                    };

                    context.OrderMenu.Add(order);
                    context.SaveChanges();
                    foreach (MenuForDay daymenu in mfw.MenuForDay)
                    {
                        foreach (Dish dish in daymenu.Dishes)
                        {
                            int catindex = 0;
                            for (int i = 0; i < 4; i++)
                                if (string.Equals(categories[i], dish.DishType.Category))
                                    catindex = i;
                            rnd = rand.Next(numsForCourses[dish.DishType.Category]);
                            DishQuantity dqua =
                                context.DishQuantities.Include("Dish").Include("MenuForDay").Include("MenuForWeek").Include("OrderMenu")
                                    .AsEnumerable()
                                    .FirstOrDefault(
                                        dq =>
                                            dq.Quantity.Equals(
                                                coursesnums[catindex][rnd]));

                            if (dqua != null)
                            {
                                dqua.Dish = dish;//context.Dishes.SingleOrDefault(d => d.DishID == dish.DishID);
                                dqua.MenuForDay = daymenu;//context.MenuForDay.SingleOrDefault(mfd => mfd.ID == daymenu.ID);
                                dqua.MenuForWeek = mfw;//context.MenuForWeek.SingleOrDefault(weekmenu => weekmenu.ID == mfw.ID);
                                dqua.OrderMenu = order;
                                quantyties.Add(dqua);
                            }
                        }
                    }
                    //order.DishQuantities = quantyties;
                    quantyties.ForEach(qu=>context.DishQuantities.Add(qu));
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
