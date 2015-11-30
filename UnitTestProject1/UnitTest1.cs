using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayOfWeek = ACSDining.Core.Domains.DayOfWeek;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DishType> _dishtypeRepository;
        private readonly IRepository<MenuForWeek> _weekmenuRepository;
        private readonly IRepository<Dish> _dishRepository;
        private readonly IRepository<DayOfWeek> _dayRepository;

        public UnitTest1(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dishtypeRepository = _unitOfWork.Repository<DishType>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
            _dishRepository = _unitOfWork.Repository<Dish>();
            _dayRepository = _unitOfWork.Repository<DayOfWeek>();
        }

        [TestMethod]
        public void ConfigXmlLoad_Test()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") + @"ACSDining.Core\DBinitial\DishDetails.xml";
            var xml = XDocument.Load(path);
            Assert.IsNotNull(xml);
            if (xml.Root != null)
            {
                var collection = xml.Root.Descendants("dish");

                List<DishType> dtList = _dishtypeRepository.GetAll().ToList();

                Func<string, DishType> getDishType =
                    el1 =>
                    {
                        DishType dtype = dtList.FirstOrDefault(dt => string.Equals(el1, dt.Category));

                        return dtype;
                    };

                Func<string, double> parseDouble = str =>
                {
                    double num;
                    Double.TryParse(str, out num);
                    return num/100;
                };
                Dish[] dishes = (from el in collection.AsEnumerable()
                    let xElement = el.Element("description")
                    where xElement != null
                    let element = el.Element("cost")
                    where element != null
                    let xElement1 = el.Element("foods")
                    where xElement1 != null
                    let element1 = el.Element("recept")
                    where element1 != null
                    select new Dish
                    {
                        DishType = getDishType(el.Attribute("dishtype").Value),
                        Title = el.Attribute("title").Value,
                        Description = xElement.Value,
                        ProductImage = el.Attribute("image").Value,
                        Price = parseDouble(element.Value),
                        DishDetail = new DishDetail
                        {
                            Title = el.Attribute("title").Value,
                            Foods = xElement1.Value,
                            Recept = element1.Value
                        }
                    }).ToArray();
                Assert.IsTrue(dishes.Any());
            }
        }

        public void WeekNumber_Test()
        {
            Func<int> nweek = () =>
            {
                CultureInfo myCi = new CultureInfo("uk-UA");
                Calendar myCal = myCi.Calendar;

                // Gets the DTFI properties required by GetWeekOfYear.
                CalendarWeekRule myCwr = myCi.DateTimeFormat.CalendarWeekRule;
                System.DayOfWeek myFirstDow = myCi.DateTimeFormat.FirstDayOfWeek;
                DateTime lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                return myCal.GetWeekOfYear(lastDay, myCwr, myFirstDow);

            };
            int wn = nweek();
            Assert.IsTrue(wn > 40);
        }
        [TestMethod]
        private void CreateMenuForWeek()
        {
            Random rand = new Random();
            string[] categories = { "Первое блюдо", "Второе блюдо", "Салат", "Напиток" };
            Dictionary<string, int> catCount = categories.ToDictionary(cat => cat, count => _dishRepository.GetAll().Count(d => string.Equals(d.DishType.Category, count)));
            Func<List<Dish>> getDishes = () =>
            {
                List<Dish> ds = new List<Dish>();
                foreach (KeyValuePair<string, int> pair in catCount)
                {
                    ds.Add(_dishRepository.GetAll().Where(d => string.Equals(d.DishType.Category, pair.Key)).ElementAt(rand.Next(pair.Value)));
                }
                return ds;
            };
            Func<List<Dish>, double> getTotalPrice = dishList =>
            {
                return dishList.AsEnumerable().ToList().Select(d => d.Price).Sum();
            };
            List<MenuForDay> mfdays = new List<MenuForDay>();
            
            for (int i = 1; i <= 5; i++)
            {
                List<Dish> dishes = getDishes();
                MenuForDay dayMenu = new MenuForDay
                {
                    Dishes = dishes,
                    DayOfWeek = _dayRepository.GetById(i),
                    TotalPrice = getTotalPrice(dishes)
                };
                mfdays.Add(dayMenu);
            }
            _weekmenuRepository.Insert(new MenuForWeek
            {
                MenuForDay = mfdays,
                WeekNumber = UnitOfWork.CurrentWeek()
            });
            Assert.IsTrue(_weekmenuRepository.GetAll().Select(w => w.MenuForDay.Where(m => m.TotalPrice > 0)).Any());
        }
    }
}
