using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using ACSDining.Infrastructure.Identity;
using ACSDining.Web.Models.ViewModels;
using ACSDining.Core.Domains;
using NLog;
using System.DirectoryServices.AccountManagement;
using ACSDining.Core.UnitOfWork;

namespace ACSDining.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AccountController : Controller
    {
       // private IUserAccountService _accountService;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly PrincipalContext _ad;
        //private readonly IUnitOfWorkAsync _unitOfWork;
        //private readonly IRepositoryAsync<User> _useRepositoryAsync; 
        //private readonly IRepository<User> _userRepository;
        //private readonly IRepository<UserRole> _roleRepository;
        public AccountController( ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _ad = new PrincipalContext(ContextType.Domain, "srv-main.infocom-ltd.com", @"infocom-ltd\ldap_ro", "240#gbdj");
           // _unitOfWork = unitOfWork;
            //_useRepositoryAsync = unitOfWork.RepositoryAsync<User>();
            //_accountService = new UserAccountService(_useRepositoryAsync);
            //UserStore<User> store = new UserStore<User>(UnitOfWork.GetContext());
            _userManager = userManager;
            _signInManager = signInManager;
            //    _unitOfWork = new UnitOfWork();
            //    _userRepository = _unitOfWork.Repository<User>();
            //    _roleRepository = _unitOfWork.Repository<UserRole>();
        }

        //public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IUnitOfWorkAsync unitOfWork)
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //    _ad = new PrincipalContext(ContextType.Domain, "srv-main.infocom-ltd.com", @"infocom-ltd\ldap_ro", "240#gbdj");
        //    _unitOfWork = unitOfWork;
        //    _useRepositoryAsync = _unitOfWork.RepositoryAsync<User>();
        //    _accountService = new UserAccountService(_useRepositoryAsync);
        //}

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
            //var result = await SignInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe, shouldLockout: false);
            var result = await _signInManager.ValidateUserFromAd(model.LogIn, model.Password);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = UserManager.FindByName(model.LogIn);
                    if (user != null)
                    {
                        result =
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);
                        if (result == SignInStatus.Success)
                        {
                            if (await UserManager.IsInRoleAsync(user.Id, "Employee"))
                            {
                                user.LastLoginTime = DateTime.UtcNow;
                                Session["Fname"] = user.FirstName;
                                Session["Lname"] = user.LastName;
                                Session["LastLoginDate"] = user.LastLoginTime;
                                return RedirectToAction("Index", "Employee", new {Area = "EmployeeArea"});
                            }

                        }
                        else
                        {
                        //    //using (ApplicationDbContext context = new ApplicationDbContext())
                        //    //{

                            User userchangePass = UserManager.FindByName(model.LogIn);
                                if (userchangePass != null)
                                {
                                    userchangePass.PasswordHash = _userManager.PasswordHasher.HashPassword(model.Password);
                                    //_userRepository.Update(userchangePass);
                                    _userManager.Update(userchangePass);
                                    //context.SaveChanges();
                                    await Login(model, returnUrl);
                                }
                        //        //context.Users.
                        //    //}
                        }
                    }
                    else
                    {
                        //using (ApplicationDbContext context = new ApplicationDbContext())
                        //{
                        //IdentityRole role = UserManager.Find(r => string.Equals(r.Name, "Employee")).Result;

                            UserPrincipal u = new UserPrincipal(_ad) {SamAccountName = model.LogIn};
                            PrincipalSearcher search = new PrincipalSearcher(u);
                            UserPrincipal usprincrezult = (UserPrincipal) search.FindOne();
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
                                   // SecurityStamp = Guid.NewGuid().ToString(),
                                    PasswordHash = (new PasswordHasher()).HashPassword(model.Password)
                                };


                            //user.Roles.Add(new IdentityUserRole {RoleId = role.Id, UserId = user.Id});
                            //_userRepository.Insert(user);
                            //context.SaveChanges();
                        var res = UserManager.CreateAsync(user).Result;
                        //var res = _accountService.CreateUser(user);
                        if (res==IdentityResult.Success)
                        {
                            if (user != null) await UserManager.AddToRoleAsync(user.Id, "Employee");
                        }

                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                        if (user != null)
                        {
                            user.LastLoginTime = DateTime.UtcNow;
                            Session["Fname"] = user.FirstName;
                            Session["Lname"] = user.LastName;
                            Session["LastLoginDate"] = user.LastLoginTime;
                        }

                        return RedirectToAction("Index", "Employee", new {Area = "EmployeeArea"});
                        //}
/*
                        ModelState.AddModelError("", "Неудачная попытка регистрации пользователя.");
*/
                    }
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "Ваша учётная запись заблокирована.");
                    return View(model);

                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    var specuser = UserManager.FindByName(model.LogIn);
                    if (specuser != null)
                    {
                        if (await UserManager.IsInRoleAsync(specuser.Id, "Administrator"))
                        {
                            specuser.LastLoginTime = DateTime.UtcNow;
                            Session["Fname"] = specuser.FirstName;
                            Session["Lname"] = specuser.LastName;
                            Session["LastLoginDate"] = specuser.LastLoginTime;
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                            return RedirectToAction("Accounts", "Admin", new {Area = "AdminArea"});
                        }
                        if (await UserManager.IsInRoleAsync(specuser.Id, "SuperUser"))
                        {
                            specuser.LastLoginTime = DateTime.UtcNow;
                            Session["Fname"] = specuser.FirstName;
                            Session["Lname"] = specuser.LastName;
                            Session["LastLoginDate"] = specuser.LastLoginTime;
                            await
                                _signInManager.PasswordSignInAsync(model.LogIn, model.Password, model.RememberMe,
                                    shouldLockout: false);

                            return RedirectToAction("WeekMenu", "SU_", new {Area = "SU_Area"});
                        }

                    }
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);

                default:
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        //[AllowAnonymous]
        //public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    // Требовать предварительный вход пользователя с помощью имени пользователя и пароля или внешнего имени входа
        //    if (!await SignInManager.HasBeenVerifiedAsync())
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Приведенный ниже код защищает от атак методом подбора, направленных на двухфакторные коды. 
        //    // Если пользователь введет неправильные коды за указанное время, его учетная запись 
        //    // будет заблокирована на заданный период. 
        //    // Параметры блокирования учетных записей можно настроить в IdentityConfig
        //    var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(model.ReturnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Неправильный код.");
        //            return View(model);
        //    }
        //}

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName,
                    RegistrationDate = DateTime.UtcNow,
                    PasswordHash = (new PasswordHasher()).HashPassword(model.Password)
                };
                var result = UserManager.Create(user);
                if (result==IdentityResult.Success)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
                    // Отправка сообщения электронной почты с этой ссылкой
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Подтвердите вашу учетную запись, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(IdentityResult.Failed());
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        //[AllowAnonymous]
        //public async Task<ActionResult> ConfirmEmail(string userId, string code)
        //{
        //    if (userId == null || code == null)
        //    {
        //        return View("Error");
        //    }
        //    var result = await UserManager.ConfirmEmailAsync(userId, code);
        //    return View(result.Succeeded ? "ConfirmEmail" : "Error");
        //}

        //
        //// GET: /Account/ForgotPassword
        //[AllowAnonymous]
        //public ActionResult ForgotPassword()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/ForgotPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByNameAsync(model.Email);
        //        if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        //        {
        //            // Не показывать, что пользователь не существует или не подтвержден
        //            return View("ForgotPasswordConfirmation");
        //        }

        //        // Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
        //        // Отправка сообщения электронной почты с этой ссылкой
        //        // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        //        // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        //        // await UserManager.SendEmailAsync(user.Id, "Сброс пароля", "Сбросьте ваш пароль, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
        //        // return RedirectToAction("ForgotPasswordConfirmation", "Account");
        //    }

        //    // Появление этого сообщения означает наличие ошибки; повторное отображение формы
        //    return View(model);
        //}

        ////
        //// GET: /Account/ForgotPasswordConfirmation
        //[AllowAnonymous]
        //public ActionResult ForgotPasswordConfirmation()
        //{
        //    return View();
        //}

        ////
        //// GET: /Account/ResetPassword
        //[AllowAnonymous]
        //public ActionResult ResetPassword(string code)
        //{
        //    return code == null ? View("Error") : View();
        //}

        ////
        //// POST: /Account/ResetPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var user = await UserManager.FindByNameAsync(model.Email);
        //    if (user == null)
        //    {
        //        // Не показывать, что пользователь не существует
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    AddErrors(result);
        //    return View();
        //}

        ////
        //// GET: /Account/ResetPasswordConfirmation
        //[AllowAnonymous]
        //public ActionResult ResetPasswordConfirmation()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/ExternalLogin
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult ExternalLogin(string provider, string returnUrl)
        //{
        //    // Запрос перенаправления к внешнему поставщику входа
        //    return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        //}

        ////
        //// GET: /Account/SendCode
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();
        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    // Создание и отправка маркера
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        return View("Error");
        //    }
        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        ////
        //// GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    // Выполнение входа пользователя посредством данного внешнего поставщика входа, если у пользователя уже есть имя входа
        //    var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        //        case SignInStatus.Failure:
        //        default:
        //            // Если у пользователя нет учетной записи, то ему предлагается создать ее
        //            ViewBag.ReturnUrl = returnUrl;
        //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        //    }
        //}

        ////
        //// POST: /Account/ExternalLoginConfirmation
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("WeekMenu", "Manage");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        // Получение сведений о пользователе от внешнего поставщика входа
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new User { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}

        //
        // POST: /Account/LogOff
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session["Fname"] = null;
            Session["Lname"] = null;
            Session["LastLoginDate"] = null;
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        //
        //// GET: /Account/ExternalLoginFailure
        //[AllowAnonymous]
        //public ActionResult ExternalLoginFailure()
        //{
        //    return View();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (_userManager != null)
        //        {
        //            _userManager.Dispose();
        //            _userManager = null;
        //        }

        //        if (_signInManager != null)
        //        {
        //            _signInManager.Dispose();
        //            _signInManager = null;
        //        }
        //       // _unitOfWork.Dispose();
        //    }

        //    base.Dispose(disposing);
        //}

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