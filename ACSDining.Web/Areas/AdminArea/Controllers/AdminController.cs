using System.Web.Mvc;

namespace ACSDining.Web.Areas.AdminArea.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/Admin/
        public ActionResult Accounts()
        {
            return View();
        }
        public ActionResult Roles()
        {
            return View();
        }
	}
}