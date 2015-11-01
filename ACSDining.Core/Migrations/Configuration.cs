namespace ACSDining.Core.Migrations
{
    using Identity;
    using System;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Domains.ApplicationDbContext>
    {
        string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", "") + @"DBinitial\DishDetails.xml";
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(Domains.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 

            //
            ApplicationDbInitializer.InitializeIdentityForEF(context);
            var dishes = ApplicationDbInitializer.GetDishesFromXML(context,path);
            ApplicationDbInitializer.CreateMenuForWeek(context, dishes);
            path = path.Replace(@"DishDetails", "Employeers");
            ApplicationDbInitializer.GetUsersFromXml(context, path);
            ApplicationDbInitializer.CreateOrders(context);

        }

    }
}
