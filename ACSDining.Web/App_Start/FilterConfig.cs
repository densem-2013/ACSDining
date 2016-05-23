using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace ACSDining.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;

            // If the browser session has expired...
            if (ctx.Session["FullName"] == null || !filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                         new RouteValueDictionary {
                        { "Controller", "Account" },
                        { "Action", "Login" }
                });
            }
            base.OnActionExecuting(filterContext);
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class EmplSessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;

            // If the browser session has expired...
            if (ctx.Session["EmployeeFullname"] == null || !filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                         new RouteValueDictionary {
                        { "Controller", "Account" },
                        { "Action", "Login" }
                });
            }
            base.OnActionExecuting(filterContext);
        }
    }
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new SessionExpireFilterAttribute());
            filters.Add(new EmplSessionExpireFilterAttribute());
        }
    }
}
