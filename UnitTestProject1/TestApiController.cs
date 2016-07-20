using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.DTO.SuperUser.Orders;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using ACSDining.Web.Areas.EmployeeArea.Controllers;
using ACSDining.Web.Areas.SU_Area.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestApiController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMenuForWeekService _menuForWeekService;

        public TestApiController()
        {
            _unitOfWork = new UnitOfWork(new ApplicationDbContext());
            _menuForWeekService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
        }

        [TestMethod]
        public void WeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 18,
                Year = 2016
            };
            WeekMenuDto wmDto = weekMenuApi.GetWeekMenu(wyDto).Result;
            Assert.IsNotNull(wmDto);
        }

        [TestMethod]
        public void CreateWeekMenuTestApi()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 22,
                Year = 2016
            };
            // weekMenuApi.GetWeekMenu(wyDto).Result;
            Assert.IsNotNull(wyDto);
        }

        [TestMethod]
        public void GetCategoriesTest()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            string[] categories = weekMenuApi.GetCategories().Result;

            Assert.IsNotNull(categories);
        }

        [TestMethod]
        public void GetOrders()
        {
            OrdersController ord = new OrdersController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            WeekOrderDto uwoDtos = ord.GetFactMenuOrders(wyDto).Result;

            Assert.IsNotNull(uwoDtos);

        }

        [TestMethod]
        public void GetNextWeekMenu()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            WeekMenuDto dto = weekMenuApi.GetWeekMenu(wyDto).Result;

            Assert.IsNotNull(dto);

        }

        [TestMethod]
        public void TestWeekMenuDelete()
        {
            WeekMenuController weekMenuApi = new WeekMenuController(_unitOfWork);
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 18,
                Year = 2016
            };
            MenuForWeek weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);
            if (weekmenu == null)
            {
                WeekMenuDto dto = weekMenuApi.GetWeekMenu(wyDto).Result;
                weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);
            }
            var res = weekMenuApi.DeleteMenuForWeek(weekmenu.ID).Result;
            weekmenu = _menuForWeekService.GetWeekMenuByWeekYear(wyDto);

            Assert.IsNull(weekmenu);

        }

        [TestMethod]
        public void EmployeDtoGetTest()
        {
            EmployeeOrderApiController emplorderapi=new EmployeeOrderApiController(_unitOfWork);
        }

        [TestMethod]
        public void CreateUsersForReport()
        {
            //string usersstring = "Денисенко А.,Киричок А.,Панченко В.,Секретный С.,Швец А.,Лукъянов А.";
            //string usersstring = "Кичангин Николай";
            string usersstring = "Сидоренко Константин";
            Utility.CreateUsersForReport(_unitOfWork.GetContext(),usersstring);
        }
    }
}
