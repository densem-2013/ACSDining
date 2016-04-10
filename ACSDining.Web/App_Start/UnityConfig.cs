using System;
using System.Data.Entity;
using System.Web;
////using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;

namespace ACSDining.Web
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();
            // TODO: Register your types here
            container
                .RegisterType<IUnitOfWorkAsync, UnitOfWork>(new HierarchicalLifetimeManager())
                .RegisterType<IAuthenticationManager>(
                    new InjectionFactory(o => HttpContext.Current.GetOwinContext().Authentication))
                .RegisterType<IUserStore<User>, UserStore<User>>(new TransientLifetimeManager(),
                    new InjectionConstructor(new ApplicationDbContext()))
                .RegisterType<IRoleStore<UserRole, string>>(new TransientLifetimeManager());
            //.RegisterType<DbContext>(new InjectionFactory(o => new ApplicationDbContext()))
            //.RegisterType<DbContext, ApplicationDbContext>(new HierarchicalLifetimeManager())
            //.RegisterType<DbContext>(new InjectionFactory(o=>UnitOfWork.GetContext()))
            //.RegisterType<IAuthenticationManager>(
            //    new InjectionFactory(o => HttpContext.Current.GetOwinContext().Authentication))
            //.RegisterType
            //<Microsoft.AspNet.Identity.IUserStore<User>, Microsoft.AspNet.Identity.EntityFramework.UserStore<User>>(
            //    new HierarchicalLifetimeManager());
            //.RegisterType<IMenuForWeekService, MenuForWeekService>(new HierarchicalLifetimeManager())
            //.RegisterType<IRepositoryAsync<MenuForWeek>, Repository<MenuForWeek>>(new HierarchicalLifetimeManager())
            //.RegisterType<IOrderMenuService, OrderMenuService>(new HierarchicalLifetimeManager())
            //.RegisterType<IRepositoryAsync<WeekOrderMenu>, Repository<WeekOrderMenu>>(new HierarchicalLifetimeManager())
            //.RegisterType<IGetExcelService, GetExcelService>(new HierarchicalLifetimeManager())
            ////.RegisterType<IUserAccountService, UserAccountService>(new HierarchicalLifetimeManager())
            //.RegisterType<IRepositoryAsync<User>, Repository<User>>(new HierarchicalLifetimeManager())
            //.RegisterType<IWorkDaysService, WorkDaysService>(new HierarchicalLifetimeManager())
            //.RegisterType<IRepositoryAsync<WorkingWeek>, Repository<WorkingWeek>>(new HierarchicalLifetimeManager())
            //.RegisterType<IDishService, DishService>(new HierarchicalLifetimeManager())
            //.RegisterType<IRepositoryAsync<Dish>, Repository<Dish>>(new HierarchicalLifetimeManager());
        }
    }
}
