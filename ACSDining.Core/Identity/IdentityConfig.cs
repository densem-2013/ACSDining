using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using ACSDining.Core.Domains;

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
                RequireUppercase = true,
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
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        public static void InitializeIdentityForEF(ApplicationDbContext context)
        {
            context.DishTypes.AddOrUpdate(dt => dt.Category, new DishType[]
                {
                  new DishType {  Category = "Первое блюдо" },
                  new DishType {  Category = "Второе блюдо" },
                  new DishType {  Category = "Салат" },
                  new DishType {  Category = "Десерт" },
                  new DishType {  Category = "Напиток" }
                });
            context.Days.AddOrUpdate(d => d.Name, new ACSDining.Core.Domains.DayOfWeek[]
                {
                    new ACSDining.Core.Domains.DayOfWeek{  Name="Понедельник"},
                    new ACSDining.Core.Domains.DayOfWeek{  Name="Вторник"},
                    new ACSDining.Core.Domains.DayOfWeek{  Name="Среда"},
                    new ACSDining.Core.Domains.DayOfWeek{  Name="Четверг"},
                    new ACSDining.Core.Domains.DayOfWeek{  Name="Пятница"}
                });

            var userManager = new ApplicationUserManager(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            //Create Role Admin if it does not exist
            var roleAdm = roleManager.FindByName("Administrator");
            if (roleAdm == null)
            {
                roleManager.Create(new UserRole("Administrator", "All Rights in Application"));
            }

            var roleSU = roleManager.FindByName("SuperUser");
            if (roleSU == null)
            {
                roleManager.Create(new UserRole("SuperUser", "Can Edit d_mfd_list Employee and DiningEmployee"));
            }

            var roleDE = roleManager.FindByName("DiningEmployee");
            if (roleDE == null)
            {
                roleManager.Create(new UserRole("DiningEmployee", "Сan edit the d_mfd_list of dishes in the dining room"));
            }

            var roleEMP = roleManager.FindByName("Employee");
            if (roleEMP == null)
            {
                roleManager.Create(new UserRole("Employee", "Сan order food in the dining room"));
            }

            var userAdmin = userManager.FindByName("admin");
            if (userAdmin == null)
            {
                userAdmin = new User() { UserName = "admin", Email = "densem-2013@yandex.ua", FirstName = "Admin", LastName = "User", IsDiningRoomClient = true, RegistrationDate = DateTime.UtcNow };
                var result = userManager.Create(userAdmin, "777123");
                result = userManager.SetLockoutEnabled(userAdmin.Id, false);
            }
            // Add user admin to Role Admin if not already added
            var rolesForUser = userManager.GetRoles(userAdmin.Id);
            if (!rolesForUser.Contains("Administrator"))
            {
                var result = userManager.AddToRole(userAdmin.Id, "Administrator");
            }

            //Create SU if it does not exist
            var userSU = userManager.FindByName("su");
            if (userSU == null)
            {
                userSU = new User() { UserName = "su", Email = "densem-2013@yandex.ua", FirstName = "Super", LastName = "User", IsDiningRoomClient = true, RegistrationDate = DateTime.UtcNow };
                var result = userManager.Create(userSU, "777123");
                result = userManager.SetLockoutEnabled(userSU.Id, false);
            }
            // Add userSU to Role SuperUser if not already added
            var rolesForUserSU = userManager.GetRoles(userSU.Id);
            if (!rolesForUserSU.Contains("SuperUser"))
            {
                var result = userManager.AddToRole(userSU.Id, "SuperUser");
            }

            //Create Employee if it does not exist
            var userEmployee = userManager.FindByName("employee");
            if (userEmployee == null)
            {
                userEmployee = new User() { UserName = "employee", Email = "densem-2013@yandex.ua", FirstName = "Employee", LastName = "User", IsDiningRoomClient = true, RegistrationDate = DateTime.UtcNow };
                var result = userManager.Create(userEmployee, "777123");
                result = userManager.SetLockoutEnabled(userEmployee.Id, false);
            }

            // Add userEmployee to Role Employee if not already added
            var rolesForUserEmployee = userManager.GetRoles(userEmployee.Id);
            if (!rolesForUserEmployee.Contains("Employee"))
            {
                var result = userManager.AddToRole(userEmployee.Id, "Employee");
            }

            //Create Employee if it does not exist
            var dinEmployee = userManager.FindByName("diningEmployee");
            if (dinEmployee == null)
            {
                dinEmployee = new User() { UserName = "diningEmployee", Email = "densem-2013@yandex.ua", FirstName = "DiningEmployee", LastName = "User", IsDiningRoomClient = false, RegistrationDate = DateTime.UtcNow };
                var result = userManager.Create(dinEmployee, "777123");
                result = userManager.SetLockoutEnabled(dinEmployee.Id, false);
            }

            // Add userEmployee to Role Employee if not already added
            var rolesForDinEmployee = userManager.GetRoles(userEmployee.Id);
            if (!rolesForDinEmployee.Contains("Employee"))
            {
                var result = userManager.AddToRole(dinEmployee.Id, "DiningEmployee");
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
