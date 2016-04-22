using System.Web.Mvc;

namespace ACSDining.Web.Areas.EmployeeArea
{
    public class EmployeeAreaAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "EmployeeArea";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "EmployeeArea_default",
                "EmployeeArea/{controller}/{action}/{id}",
                new {  controller = "Employee", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}