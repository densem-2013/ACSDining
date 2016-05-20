using System;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using ACSDining.Infrastructure.Identity;
using ACSDining.Web.Models.ViewModels;
using ACSDining.Core.Domains;
using Microsoft.AspNet.Identity.EntityFramework;
using NLog;

namespace ACSDining.Web.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly PrincipalContext _ad;
        public AccountController( ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
           _ad = new PrincipalContext(ContextType.Domain, "srv-main.infocom-ltd.com", @"infocom-ltd\ldap_ro", "240#gbdj");
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Сбои при входе не приводят к блокированию учетной записи
            // Чтобы ошибки при вводе пароля инициировали блокирование учетной записи, замените на shouldLockout: true
            var result = await SignInManager.ValidateUserFromAd(model.LogIn, model.Password);
            //var result = await _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = UserManager.FindByName(model.LogIn);
                    if (user != null)
                    {
                        var passres =
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);
                        if (passres == SignInStatus.Success)
                        {
                            if (await UserManager.IsInRoleAsync(user.Id, "Employee") )
                            {
                                user.LastLoginTime = DateTime.UtcNow;
                                Session["EmployeeFullname"] = user.LastName + " " + user.FirstName;
                                //Session["Fname"] = user.FirstName;
                                //Session["Lname"] = user.LastName;
                                user.LastLoginTime = DateTime.Now;
                                await
                                    _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                        shouldLockout: false);

                                return RedirectToAction("Index", "Employee", new {Area = "EmployeeArea"});
                            }
                            if (await UserManager.IsInRoleAsync(user.Id, "SuperUser"))
                            {
                                user.LastLoginTime = DateTime.UtcNow;
                                Session["FullName"] = user.LastName + " " + user.FirstName;
                                //Session["Fname"] = user.FirstName;
                                //Session["Lname"] = user.LastName;
                                user.LastLoginTime = DateTime.Now;
                                await
                                    _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                        shouldLockout: false);

                                return RedirectToAction("WeekMenu", "SU_", new {Area = "SU_Area"});
                            }

                        }
                        else
                        {
                            User userchangePass = UserManager.FindByName(model.LogIn);
                            if (userchangePass != null)
                            {
                                userchangePass.PasswordHash = _userManager.PasswordHasher.HashPassword(model.Password);
                               var  updateres=_userManager.Update(userchangePass);
                               if (updateres==IdentityResult.Success)
                               {
                                   await Login(model, returnUrl);
                                    
                                }
                            }
                        }
                    }
                    else
                    {

                        UserPrincipal u = new UserPrincipal(_ad) { SamAccountName = model.LogIn };
                        PrincipalSearcher search = new PrincipalSearcher(u);
                        UserPrincipal usprincrezult = (UserPrincipal)search.FindOne();
                        search.Dispose();
                        if (usprincrezult != null)
                            user = new User
                            {
                                FirstName = usprincrezult.GivenName,
                                LastName = usprincrezult.Surname,
                                Email = usprincrezult.EmailAddress,
                                UserName = usprincrezult.SamAccountName,
                                LastLoginTime = DateTime.UtcNow,
                                RegistrationDate = DateTime.UtcNow,
                                EmailConfirmed = true,
                                PasswordHash = (new PasswordHasher()).HashPassword(model.Password)
                            };


                        var res = UserManager.CreateAsync(user).Result;
                        if (res == IdentityResult.Success)
                        {
                            if (user != null) await UserManager.AddToRoleAsync(user.Id, "Employee");
                        }

                        await
                            _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                shouldLockout: false);

                        if (user != null)
                        {
                            user.LastLoginTime = DateTime.UtcNow;
                            Session["EmployeeFullname"] = user.LastName + " " + user.FirstName;
                        }

                        return RedirectToAction("Index", "Employee", new { Area = "EmployeeArea" });
                    }
                    return RedirectToLocal(returnUrl);
                //return RedirectToLocal("Login");

                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "Ваша учётная запись заблокирована.");
                    return View(model);

                case SignInStatus.Failure:
                    var specuser = UserManager.FindByName(model.LogIn);
                    if (specuser != null)
                    {
                        if (await UserManager.IsInRoleAsync(specuser.Id, "Administrator"))
                        {
                            specuser.LastLoginTime = DateTime.UtcNow;
                            Session["FullName"] = specuser.LastName + " " + specuser.FirstName;
                            //Session["Fname"] = user.FirstName;
                            //Session["Lname"] = user.LastName;
                            specuser.LastLoginTime = DateTime.Now;
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                            return RedirectToAction("Accounts", "Admin", new {Area = "AdminArea"});
                        }
                        if (await UserManager.IsInRoleAsync(specuser.Id, "SuperUser"))
                        {
                            specuser.LastLoginTime = DateTime.UtcNow;
                            Session["FullName"] = specuser.LastName + " " + specuser.FirstName;
                            //Session["Fname"] = user.FirstName;
                            //Session["Lname"] = user.LastName;
                            specuser.LastLoginTime = DateTime.Now;
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                            return RedirectToAction("WeekMenu", "SU_", new {Area = "SU_Area"});
                        }
                        if (await UserManager.IsInRoleAsync(specuser.Id, "Employee"))
                        {
                            specuser.LastLoginTime = DateTime.UtcNow;
                            Session["EmployeeFullname"] = specuser.LastName + " " + specuser.FirstName;
                            //Session["Fname"] = user.FirstName;
                            //Session["Lname"] = user.LastName;
                            specuser.LastLoginTime = DateTime.Now;
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                            return RedirectToAction("Index", "Employee", new {Area = "EmployeeArea"});
                        }
                    }
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);

                default:
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }

       
        // POST: /Account/LogOff
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session["FullName"] = null;
            //Session["Fname"] = null;
            //Session["Lname"] = null;
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        /// <summary>
        /// Handle HttpAntiForgeryException and redirect if user is already authenticated
        /// </summary>
        /// <param name="filterContext"></param>
        /// <remarks>
        /// See: http://stackoverflow.com/questions/19096723/login-request-validation-token-issue
        /// </remarks>
        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            var action = filterContext.RequestContext.RouteData.Values["action"] as string;
            var controller = filterContext.RequestContext.RouteData.Values["controller"] as string;

            if ((filterContext.Exception is HttpAntiForgeryException) &&
                action == "Login" &&
                controller == "MyController" &&
                filterContext.RequestContext.HttpContext.User != null &&
                filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                LogManager.GetCurrentClassLogger().Warn( filterContext.Exception,
                    "Handled AntiForgery exception because user is already Authenticated: " +
                        filterContext.Exception.Message);

                filterContext.ExceptionHandled = true;

                // redirect/show error/whatever?
                filterContext.Result = new RedirectResult("/warning");
            }
        }
        #endregion
    }
}