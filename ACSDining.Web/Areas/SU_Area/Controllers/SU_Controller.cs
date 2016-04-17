using System;
using System.Web;
using System.Web.Mvc;
using ACSDining.Infrastructure.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [System.Web.Http.Authorize(Roles = "Employee,SuperUser")]
    public class SU_Controller : Controller
    {
        private ApplicationUserManager _userManager;
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

        // GET: /SU_Area/SU_/
        public SU_Controller( ApplicationUserManager userManager)
        {
            _userManager = userManager;
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
                UserManager.FindByNameAsync(User.Identity.Name).Result;
            user.LastLoginTime = DateTime.UtcNow;
            ViewBag.Fname = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
        }

    }
}