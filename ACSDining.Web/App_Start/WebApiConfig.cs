using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace ACSDining.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //jsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;

            //config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "CurWeekMenu",
                routeTemplate: "api/{controller}",
                defaults: new { controller = "WeekMenu" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiWeekMenu",
                routeTemplate: "api/{controller}/{numweek}/{year}",
                defaults: new { numweek = RouteParameter.Optional, year = RouteParameter.Optional, controller = "WeekMenu" },
                constraints: new { numweek = @"d+", year = @"d?|d[4]" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
