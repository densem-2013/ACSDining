using System.Web.Mvc;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    [System.Web.Http.Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        [EmplSessionExpireFilter]
        public ActionResult Index()
        {
            return View();
        }
    }
}
