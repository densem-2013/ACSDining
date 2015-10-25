using ACSDining.Web.Areas.SU_Area.Models;
using System.Web;
using System.Web.Mvc;

namespace ACSDining.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
           // filters.Add(new NotImplementedFilterAttribute());
        }
    }
}
