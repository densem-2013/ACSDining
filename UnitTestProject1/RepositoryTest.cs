﻿using System;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class RepositoryTest
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly IRepositoryAsync<WeekOrderMenu> _weekOrderMenuRepository;
        private readonly IRepositoryAsync<MenuForWeek> _weekMenuRepository; 

        public RepositoryTest()
        {
            _unitOfWork = new UnitOfWork();
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _weekOrderMenuRepository = _unitOfWork.RepositoryAsync<WeekOrderMenu>();
            _weekMenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
        }

        [TestMethod]
        public void NewMenuForWeekTest()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 17,
                Year = 2016
            };

            _weekMenuRepository.CreateMenuForWeekOnWeekYear(wyDto);
            MenuForWeek weekmenu = _weekMenuRepository.GetWeekMenuByWeekYear(wyDto);
            _unitOfWork.GetContext().MenuForWeeks.Add(weekmenu);
            //_weekMenuRepository.Insert(weekmenu);
            _unitOfWork.SaveChanges();
            weekmenu = _weekMenuRepository.GetWeekMenuByWeekYear(wyDto);
            Assert.IsNotNull(weekmenu);
        }

    }
}
