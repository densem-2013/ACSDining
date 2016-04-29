using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using ACSDining.Infrastructure.UnitOfWork;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestWeekMenuController
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekMenuService;

        public TestWeekMenuController()
        {
            _unitOfWork = new UnitOfWork();
            _db = _unitOfWork.GetContext();
            _weekMenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
        }

        [TestMethod]
        public void TestCreateWeekMenuOnRepository()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            MenuForWeek newMenuForWeek = _weekMenuService.GetWeekMenuByWeekYear(wyDto);
            if (newMenuForWeek==null)
            {
                _weekMenuService.CreateByWeekYear(wyDto);
                
                newMenuForWeek = _weekMenuService.GetWeekMenuByWeekYear(wyDto);
            }
            newMenuForWeek.MenuForDay.ToList().ForEach(x =>
            {
                if (!x.Dishes.Any())
                {
                    x.Dishes = TestHelper.GetDishes(_unitOfWork);
                }
            });
            _unitOfWork.GetContext().Entry(newMenuForWeek).State = EntityState.Modified;
            _unitOfWork.SaveChanges();

            Assert.IsNotNull(newMenuForWeek);
        }

    }
}
