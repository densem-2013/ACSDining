using System;
using System.Web.Mvc;
using ACSDining.Core.UnitOfWork;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    public class SU_Controller : Controller
    {
        private IUserAccountService _accountService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        //private ApplicationUserManager _userManager;
        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}

        // GET: /SU_Area/SU_/
        public SU_Controller(IUnitOfWorkAsync unitOfWork, IUserAccountService accountService)
        {
            //_unitOfWork = unitOfWork;
            //IRepositoryAsync<User> useRepositoryAsync = _unitOfWork.RepositoryAsync<User>();
            //_accountService = new UserAccountService(useRepositoryAsync);
            _unitOfWork = unitOfWork;
            _accountService = accountService;
        }
        public ActionResult WeekMenu()
        {
            AddInfoToViewData();
            return View();
        }

        public ActionResult Orders()
        {
            AddInfoToViewData();
            return View();
        }
        public ActionResult Dishes()
        {
            AddInfoToViewData();
            return View();
        }
        public ActionResult Payment()
        {
            AddInfoToViewData();
            return View();
        }
        public ActionResult AccountManagement()
        {
            AddInfoToViewData();
            return View();
        }
        public ActionResult WorkDayManagement()
        {
            AddInfoToViewData();
            return View();
        }

        private void AddInfoToViewData()
        {
            Core.Domains.User user =
                _accountService.GetUserByName(User.Identity.Name);
            user.LastLoginTime = DateTime.UtcNow;
            ViewBag.Fname = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}