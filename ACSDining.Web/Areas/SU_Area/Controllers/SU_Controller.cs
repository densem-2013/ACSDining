using System.Web.Mvc;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser,Administrator")]
    public class SU_Controller : Controller
    {
        // GET: /SU_Area/SU_/
        public ActionResult WeekMenu()
        {
            ViewBag.Title = "Welcome " + Session["Lname"] + " " + Session["Fname"];
            return View();
        }

        public ActionResult Orders()
        {
            ViewBag.Title = "Welcome " + Session["Lname"] + " " + Session["Fname"];
            return View();
        }
        public ActionResult Dishes()
        {
            ViewBag.Title = "Welcome " + Session["Lname"] + " " + Session["Fname"];
            return View();
        }
    }
}