using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using ACSDining.Infrastructure.HelpClasses;
using PerpetuumSoft.Knockout;

namespace ACSDining.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ModelBinders.Binders.DefaultBinder = new KnockoutModelBinder();
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            TimerHandler.Init();
        }
        protected void Application_PostAuthorizeRequest()
        {
            System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
        }
        protected void Session_End(Object sender, EventArgs e)
        {

            HttpCookie authenticationCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authenticationCookie != null)
            {
                FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authenticationCookie.Value);
                if (!authenticationTicket.Expired)
                {
                    Response.Redirect("Login.cshtml");
                }
            }

        }
    }
}
