using System;
using System.Diagnostics;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.DTO.SuperUser.Orders;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class GetExcelTest
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMenuForWeekService _menuForWeekService;

        public GetExcelTest()
        {
            _unitOfWork = new UnitOfWork(new ApplicationDbContext());
            _menuForWeekService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
        }

        [TestMethod]
        public void GetExcellPaimentsTestApi()
        {
            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //                          @"ACSDining.Web\ExcelFiles\Paiments.xls";
            //GetExcelController excelApi = new GetExcelController(_unitOfWork);
            GetExcelService excelService = new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 23,
                Year = 2016
            };
            ForExcelDataDto feDto = new ForExcelDataDto
            {
                WeekYear = wyDto,
                DataString = "test string"
            };
            string result = excelService.GetExcelFileFromPaimentsModel(feDto);
            Assert.IsNotNull(result);
            Process.Start(result);
        }
        [TestMethod]
        public void GetOrdersExcellTestApi()
        {
            //string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
            //                          @"ACSDining.Web\ExcelFiles\PlanOrders.xls";
            //GetExcelController excelApi = new GetExcelController(_unitOfWork);
            GetExcelService excelService = new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 23,
                Year = 2016
            };
            ForExcelDataDto feDto = new ForExcelDataDto
            {
                WeekYear = wyDto,
                DataString = "test string"
            };
            string result = excelService.GetExcelFileFromOrdersModel(feDto);
            Assert.IsNotNull(result);
            Process.Start(result);
        }

        [TestMethod]
        public void GetMenuExcellTestApi()
        {
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                                      @"ACSDining.Web\ExcelFiles\Menu.xls";
            //GetExcelController excelApi = new GetExcelController(_unitOfWork);
            GetExcelService excelService = new GetExcelService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 22,
                Year = 2016
            };
            ForMenuExcelDto feDto = new ForMenuExcelDto
            {
                WeekYear = wyDto,
                MenuTitle = "test string"
            };
            string result = excelService.GetMenuExcel(feDto);
            Assert.IsNotNull(result);
            Process.Start(_path);
        }

        [TestMethod]
        public void GetWeekTitleTestApi()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 26,
                Year = 2016
            };
            string rezult = YearWeekHelp.GetWeekTitle(_unitOfWork.RepositoryAsync<MenuForWeek>(), wyDto);
            Assert.IsNotNull(rezult);
        }
    }
}
