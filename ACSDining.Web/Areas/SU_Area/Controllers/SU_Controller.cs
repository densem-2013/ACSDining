using System.Web.Mvc;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    //[SessionExpireFilter]
    [System.Web.Http.Authorize(Roles = "SuperUser")]
    public class SU_Controller : Controller
    {
        [CheckSessionOut]
        public ActionResult WeekMenu()
        {
            return View();
        }

        [CheckSessionOut]
        public ActionResult Orders()
        {
            return View();
        }
        [CheckSessionOut]
        public ActionResult Dishes()
        {
            return View();
        }
        [CheckSessionOut]
        public ActionResult Payment()
        {
            return View();
        }
        [CheckSessionOut]
        public ActionResult AccountManagement()
        {
            return View();
        }
       

    }
}