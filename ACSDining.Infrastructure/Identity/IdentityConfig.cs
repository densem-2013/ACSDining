﻿using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace ACSDining.Infrastructure.Identity
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
            UserStore<User> store = new UserStore<User>(context.Get<ApplicationDbContext>());

            var manager = new ApplicationUserManager(store);
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

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options,
            IOwinContext context)
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

    public class ApplicationSignInManager : SignInManager<User, string>
    {
        private PrincipalContext _ad;

        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
            //_ad = new PrincipalContext(ContextType.Domain, "srv-main.infocom-ltd.com", @"infocom-ltd\ldap_ro", "240#gbdj");
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager) UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options,
            IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public Task<SignInStatus> ValidateUserFromAd(string login, string password)
        {
            bool res = false;
            UserPrincipal u = null;
            try
            {
                u = new UserPrincipal(_ad) { SamAccountName = login };
                res = _ad.ValidateCredentials(u.SamAccountName, password);
            }
            catch (Exception)
            {
                res = false;
                throw;
            }
            if (res)
            {
                if (!u.IsAccountLockedOut())
                {
                    return Task.FromResult(SignInStatus.Success);
                }
                else
                {
                    return Task.FromResult(SignInStatus.LockedOut);
                }
            }
            else
            {
                return Task.FromResult(SignInStatus.Failure);
            }
        }
    }
}
