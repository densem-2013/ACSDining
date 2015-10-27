﻿using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ACSDining.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
              ModelBinders.Binders.DefaultBinder = new PerpetuumSoft.Knockout.KnockoutModelBinder();
              AreaRegistration.RegisterAllAreas();
              GlobalConfiguration.Configure(WebApiConfig.Register);
              GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
              FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
              RouteConfig.RegisterRoutes(RouteTable.Routes);
              BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
