using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Infrastructure.DAL;
using ACSDining.Web.Areas.SU_Area.Controllers;
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
        private readonly IRepository<OrderMenu> _orderRepository;

        public UnitTest1()
        {
            _unitOfWork = new UnitOfWork();
            _dishtypeRepository = _unitOfWork.Repository<DishType>();
            _weekmenuRepository = _unitOfWork.Repository<MenuForWeek>();
            _dishRepository = _unitOfWork.Repository<Dish>();
            _dayRepository = _unitOfWork.Repository<DayOfWeek>();
            _orderRepository = _unitOfWork.Repository<OrderMenu>();
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

        [TestMethod]
        public void PrintExcelTest()
        {
            List<OrderMenu> orderMenus = _orderRepository.GetAll().Where(
                        om => om.MenuForWeek.WeekNumber == 49 && om.MenuForWeek.Year.YearNumber == 2015)
                        .ToList();
            MenuForWeek mfw = _weekmenuRepository.GetAll().FirstOrDefault(m => m.WeekNumber == 49 && m.Year.YearNumber == 2015);
            PaimentsDTO model = null;

            model = new PaimentsDTO()
            {
                WeekNumber = 49,
                YearNumber = 2015,
                UserPaiments = orderMenus
                    .Select(order => new UserPaimentDTO()
                    {
                        UserId = order.User.Id,
                        OrderId = order.Id,
                        UserName = order.User.UserName,
                        Paiments = _unitOfWork.GetUserWeekOrderPaiments(order.Id),
                        SummaryPrice = order.SummaryPrice,
                        WeekPaid = order.WeekPaid,
                        Balance = order.Balance,
                        IsDiningRoomClient = order.User.IsDiningRoomClient,
                        Note = order.Note
                    }).OrderBy(uo => uo.UserName).ToList(),
                UnitPrices = _unitOfWork.GetUnitWeekPrices(mfw.ID),
                UnitPricesTotal = PaimentsByDishes(49, 2015)
            };

            PrintExcelController peController = new PrintExcelController(_unitOfWork);
            peController.ExportToExcel(model);
            string path = string.Format(@"{0}\ExcelData.xlsx",
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Assert.IsTrue(File.Exists(path));
        }
        private double[] PaimentsByDishes(int numweek, int year)
        {
            double[] paiments = new double[21];
            MenuForWeek weekmenu = _weekmenuRepository.GetAll().FirstOrDefault(m => m.WeekNumber == numweek && m.Year.YearNumber == year);
            double[] weekprices = _unitOfWork.GetUnitWeekPrices(weekmenu.ID);


            OrderMenu[] orderMenus = _orderRepository.GetAll().Where(
                        om => om.MenuForWeek.WeekNumber == numweek && om.MenuForWeek.Year.YearNumber == year)
                        .ToArray();
            for (int i = 0; i < orderMenus.Length; i++)
            {
                double[] dishquantities = _unitOfWork.GetUserWeekOrderDishes(orderMenus[i].Id);
                for (int j = 0; j < 20; j++)
                {
                    paiments[j] += weekprices[j] * dishquantities[j];
                }
            }
            paiments[20] = paiments.Sum();
            return paiments;
        }
    }
}
