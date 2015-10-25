using System.Web.Mvc;

namespace ACSDining.Web.Areas.SU_Area
{
    public class SU_AreaAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SU_Area";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SU_Area_default",
                "SU_Area/{controller}/{action}/{id}",
                new { action = "WeekMenu", id = UrlParameter.Optional }
            );
        }
    }
}