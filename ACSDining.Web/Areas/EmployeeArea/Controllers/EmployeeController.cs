using System.Web.Http;
using System.Web.Mvc;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    public class EmployeeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
