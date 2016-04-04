using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        private string _path =
            AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") +
            @"ACSDining.Core\DBinitial\DishDetails.xml";

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 

            //
            ApplicationDbInitializer.InitializeIdentityForEF(context, _path);
            var dishes = ApplicationDbInitializer.GetDishesFromXML(context, _path);
            ApplicationDbInitializer.CreateWorkingDays(context);
            ApplicationDbInitializer.CreateMenuForWeek(context, dishes);
            _path = _path.Replace(@"DishDetails", "Employeers");
            ApplicationDbInitializer.GetUsersFromXml(context, _path);
            ApplicationDbInitializer.CreateOrders(context);

        }

    }
}
