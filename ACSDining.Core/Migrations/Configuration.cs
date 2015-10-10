namespace ACSDining.Core.Migrations
{
    using ACSDining.Core.Domains;
    using ACSDining.Core.Identity;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal sealed class Configuration : DbMigrationsConfiguration<ACSDining.Core.Domains.ApplicationDbContext>
    {
        Random rand = new Random();
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ACSDining.Core.Domains.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 

            //
            ApplicationDbInitializer.InitializeIdentityForEF(context);

            string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", "") + @"DBinitial\DishDetails.xml";

            GetDishesFromXML(path, context);

        }
        private void GetDishesFromXML(string path, ApplicationDbContext context)
        {
            var xml = XDocument.Load(path);
            var collection = xml.Root.Descendants("dish");

            List<DishType> dtList = context.DishTypes.ToList();

            Func<string, DishType> getDishType =
                (el1) =>
                {
                    DishType dtype = dtList.FirstOrDefault(dt => string.Equals(el1, dt.Category));

                    return dtype;
                };

            Func<string, double> parseDouble = (str) =>
            {
                double num;
                Double.TryParse(str, out num);
                return num;
            };
            try
            {

                Dish[] dishes = (from el in collection.AsEnumerable()
                    select new Dish()
                    {
                        DishType = getDishType(el.Attribute("dishtype").Value),
                        Title = el.Attribute("title").Value,
                        Description = el.Element("description").Value,
                        ProductImage = el.Attribute("image").Value,
                        Price = parseDouble(el.Element("cost").Value),
                        DishDetail = new DishDetail()
                        {
                            Title = el.Attribute("title").Value,
                            Foods = el.Element("foods").Value,
                            Recept = el.Element("recept").Value
                        }
                    }).ToArray();

                context.Dishes.AddOrUpdate(c => c.Title, dishes);

                CreateMenuForWeek(context, dishes);
            }
            catch (System.NullReferenceException ex)
            {

                throw;
            }

        }
        private void CreateMenuForWeek(ApplicationDbContext context,Dish[] dishArray)
        {
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };
            //IEnumerable<Dish> dtList_2 = context.Dishes.ToList();
            Func<string, IEnumerable<Dish>, int> countDish = (str, list) =>
                {
                    int coun = list.Count(el => string.Equals(el.DishType.Category, str));
                    return coun;
                };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => countDish(count, dishArray));
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(dishArray.Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
                }
                return ds;
            };

            List<MenuForDay> mfdays = new List<MenuForDay>();

            for (int i = 1; i <= 5; i++)
            {
                List<Dish> dishes = getDishes();
                MenuForDay dayMenu = new MenuForDay()
                {
                    Dishes = dishes,
                    DayOfWeek = context.Days.FirstOrDefault(day => day.ID == i),
                    TotalPrice = dishes.Select(d => d.Price).Sum()
                };

                mfdays.Add(dayMenu);
            }
            context.MenuForWeek.AddOrUpdate(m=>m.WeekNumber, new MenuForWeek()
            {
                MenuForDay = mfdays,
                WeekNumber = context.CurrentWeek(),
                SummaryPrice = mfdays.AsEnumerable().Select(d => d.TotalPrice).Sum()
            });
        }
    }
}
