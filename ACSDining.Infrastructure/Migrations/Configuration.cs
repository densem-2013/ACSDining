using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;

namespace ACSDining.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"ACSDining.Infrastructure\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 

            //
            ApplicationDbInitializer.InitializeIdentityForEF(context);
            var dishes = ApplicationDbInitializer.GetDishesFromXML(context, path);
            ApplicationDbInitializer.CreateMenuForWeek(context, dishes);
            path = path.Replace(@"DishDetails", "Employeers");
            ApplicationDbInitializer.GetUsersFromXml(context, path);
            ApplicationDbInitializer.CreateOrders(context);

        }

    }
}
