using System;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
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
                .RegisterType<IDataContextAsync, ApplicationDbContext>(new PerRequestLifetimeManager())
                .RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager())
                .RegisterType
                <Microsoft.AspNet.Identity.IUserStore<User>, Microsoft.AspNet.Identity.EntityFramework.UserStore<User>>()
                .RegisterType<IMenuForWeekService, MenuForWeekService>()
                .RegisterType<IRepositoryAsync<MenuForWeek>, Repository<MenuForWeek>>()
                .RegisterType<IOrderMenuService, OrderMenuService>()
                .RegisterType<IRepositoryAsync<OrderMenu>, Repository<OrderMenu>>()
                .RegisterType<IGetExcelService, GetExcelService>()
                .RegisterType<IUserAccountService, UserAccountService>()
                .RegisterType<IRepositoryAsync<User>, Repository<User>>()
                .RegisterType<IWorkDaysService, WorkDaysService>()
                .RegisterType<IRepositoryAsync<WorkingWeek>, Repository<WorkingWeek>>()
                .RegisterType<IDishService, DishService>()
                .RegisterType<IRepositoryAsync<Dish>, Repository<Dish>>();
        }
    }
}
