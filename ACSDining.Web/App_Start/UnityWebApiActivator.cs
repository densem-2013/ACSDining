using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using ACSDining.Web;
using Microsoft.Practices.Unity.Mvc;
using Microsoft.Practices.Unity.WebApi;
using UnityDependencyResolver = Microsoft.Practices.Unity.WebApi.UnityDependencyResolver;


[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(UnityWebApiActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(UnityWebApiActivator), "Shutdown")]

namespace ACSDining.Web
{
    /// <summary>Provides the bootstrapping for integrating Unity with WebApi when it is hosted in ASP.NET</summary>
    public static class UnityWebApiActivator
    {
        /// <summary>Integrates Unity when the application starts.</summary>
        public static void Start() 
        {
            // Use UnityHierarchicalDependencyResolver if you want to use a new child container for each IHttpController resolution.
            var resolver = new UnityHierarchicalDependencyResolver(UnityConfig.GetConfiguredContainer());
            //var resolver = new UnityDependencyResolver(UnityConfig.GetConfiguredContainer());
            // Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(UnityPerRequestHttpModule));
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }

        /// <summary>Disposes the Unity container when the application is shut down.</summary>
        public static void Shutdown()
        {
            var container = UnityConfig.GetConfiguredContainer();
            container.Dispose();
        }
    }
}
