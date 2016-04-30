using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class PagedQueryTest
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly IRepositoryAsync<WeekOrderMenu> _weekOrderMenuRepository;
        private readonly IRepositoryAsync<MenuForWeek> _weekMenuRepository;

        public PagedQueryTest()
        {
            _unitOfWork = new UnitOfWork();
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _weekOrderMenuRepository = _unitOfWork.RepositoryAsync<WeekOrderMenu>();
            _weekMenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
        }
        [TestMethod]
        public void WeekOrderPaged()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };
            int totCount;
            List<WeekOrderMenu> pagedOrders =
                _weekOrderMenuRepository.OrdersMenuByWeekYear(wyDto);
            int page = 1;
            int pageSize = 7;

            List<WeekOrderMenu> needOrders = pagedOrders.OrderBy(po=>po.User.LastName).Skip(pageSize*(page-1)).Take(pageSize).ToList();

            Assert.IsNotNull(needOrders);
        }
    }
}
