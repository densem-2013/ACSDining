using System.Collections.Generic;
using System.Diagnostics;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;
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
            _unitOfWork = new UnitOfWork();
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
        public void GetExcellTestApi()
        {
            //GetExcelController excelApi = new GetExcelController(_unitOfWork);
            GetExcelService excelService=new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 18,
                Year = 2016
            };
            string result = excelService.GetExcelFileFromOrdersModel(wyDto);
            Assert.IsNotNull(result);
            Process.Start(result);
        }
    }
}
