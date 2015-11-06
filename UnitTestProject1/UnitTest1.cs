using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity.Migrations;
using System.Linq;
using ACSDining.Core.Domains;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        ApplicationDbContext context = new ApplicationDbContext();
        [TestMethod]
        public void ConfigXmlLoad_Test()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
            var xml = XDocument.Load(path);
            Assert.IsNotNull(xml);
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
                return num/100;
            };
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
            Assert.IsTrue(dishes.Count() > 0);
        }

        public void WeekNumber_Test()
        {
            Func<int> nweek = () =>
            {
                CultureInfo myCI = new CultureInfo("uk-UA");
                Calendar myCal = myCI.Calendar;

                // Gets the DTFI properties required by GetWeekOfYear.
                CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                System.DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
                DateTime LastDay = new System.DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                return myCal.GetWeekOfYear(LastDay, myCWR, myFirstDOW);

            };
            int wn = nweek();
            Assert.IsTrue(wn > 40);
        }
        [TestMethod]
        private void CreateMenuForWeek(ApplicationDbContext context)
        {
            Random rand = new Random();
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => context.Dishes.AsEnumerable().Where(d => string.Equals(d.DishType.Category, count)).Count());
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(context.Dishes.AsEnumerable().Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
                }
                return ds;
            };
            Func<List<Dish>, double> getTotalPrice = (dishList) =>
            {
                return dishList.AsEnumerable().ToList().Select(d => d.Price).Sum();
            };
            List<MenuForDay> mfdays = new List<MenuForDay>();
            
            for (int i = 1; i <= 5; i++)
            {
                List<Dish> dishes = getDishes();
                string MFD_Id = Guid.NewGuid().ToString();
                MenuForDay dayMenu = new MenuForDay()
                {
                    Dishes = dishes,
                    DayOfWeek = context.Days.Find(i),
                    TotalPrice = getTotalPrice(dishes)
                };
                mfdays.Add(dayMenu);
            }
            context.MenuForWeeks.AddOrUpdate(n => n.WeekNumber, new MenuForWeek()
            {
                MenuForDay = mfdays,
                WeekNumber = context.CurrentWeek()
            });
            Assert.IsTrue(context.MenuForWeeks.AsEnumerable().Select(w => w.MenuForDay.Where(m => m.TotalPrice > 0)).Count()>0);
        }
    }
}
