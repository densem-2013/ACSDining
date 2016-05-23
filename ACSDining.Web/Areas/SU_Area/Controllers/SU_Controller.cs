using System;
using System.Web;
using System.Web.Mvc;
using ACSDining.Infrastructure.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [SessionExpireFilter]
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    public class SU_Controller : Controller
    {
        public ActionResult WeekMenu()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }
        public ActionResult Dishes()
        {
            return View();
        }
        public ActionResult Payment()
        {
            return View();
        }
        public ActionResult AccountManagement()
        {
            return View();
        }
       

    }
}