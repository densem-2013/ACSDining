using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ACSDining.Infrastructure.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser,Administrator")]
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
        public ActionResult WeekMenu()
        {
            Core.Domains.User user =
                UserManager.Users.FirstOrDefault(u => string.Equals(u.UserName, HttpContext.User.Identity.Name));
            user.LastLoginTime = DateTime.UtcNow;
            ViewBag.Fname = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
            return View();
        }

        public ActionResult Orders()
        {
            Core.Domains.User user =
                UserManager.Users.FirstOrDefault(u => string.Equals(u.UserName, HttpContext.User.Identity.Name));
            user.LastLoginTime = DateTime.UtcNow;
            ViewData["Fname"] = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
            return View();
        }
        public ActionResult Dishes()
        {
            Core.Domains.User user =
                UserManager.Users.FirstOrDefault(u => string.Equals(u.UserName, HttpContext.User.Identity.Name));
            user.LastLoginTime = DateTime.UtcNow;
            ViewData["Fname"] = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
            return View();
        }
        public ActionResult Payment()
        {
            Core.Domains.User user =
                UserManager.Users.FirstOrDefault(u => string.Equals(u.UserName, HttpContext.User.Identity.Name));
            user.LastLoginTime = DateTime.UtcNow;
            ViewData["Fname"] = user.FirstName;
            ViewData["Lname"] = user.LastName;
            ViewData["LastLoginDate"] = user.LastLoginTime;
            return View();
        }
    }
}