using System.Web.Mvc;
using System.Web.Routing;

namespace ACSDining.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Root",
                "",
                new { area = "", controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Default_Account",
                url: "Account/{action}",
                defaults: new { area = "", controller = "Account", action = "Login" },
                namespaces: new string[] { "ACSDining.Web.Controllers" }
            );
            //routes.MapRoute(
            //    name: "LogOff",
            //    url: "Account/LogOff",
            //    defaults: new { area = "", controller = "Account", action = "Login" },
            //    namespaces: new string[] { "ACSDining.Web.Controllers" }
            //);
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);


            //routes.MapRoute(
            //    name: "Default_Area",
            //    url: "{area}/{controller}/{action}/{id}",
            //    defaults: new { area = "", controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new string[] { "ACSDining.Web.Controllers" }
            //);
       }
    }
}
