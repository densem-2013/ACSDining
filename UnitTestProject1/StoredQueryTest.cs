using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using User = ACSDining.Core.Domains.User;

namespace UnitTestProject1
{
    [TestClass]
    public class StoredQueryTest
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ApplicationUserManager _userManager;
        private readonly IRepositoryAsync<WeekOrderMenu> _weekOrderMenuRepository;
        private readonly IRepositoryAsync<MenuForWeek> _weekMenuRepository;
        private readonly IWeekPaimentService _weekPaimentService;

        public StoredQueryTest()
        {
            _unitOfWork = new UnitOfWork(new ApplicationDbContext());
            _userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));
            _weekOrderMenuRepository = _unitOfWork.RepositoryAsync<WeekOrderMenu>();
            _weekMenuRepository = _unitOfWork.RepositoryAsync<MenuForWeek>();
            _weekPaimentService = new WeekPaimentService(_unitOfWork.RepositoryAsync<WeekPaiment>());
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

            List<WeekOrderMenu> needOrders =
                pagedOrders.OrderBy(po => po.User.LastName).Skip(pageSize*(page - 1)).Take(pageSize).ToList();

            Assert.IsNotNull(needOrders);
        }

        [TestMethod]
        public void StoredSumTest()
        {
            int? week = 17;
            int? year = 2016;
            var weekParameter = week.HasValue
                ? new SqlParameter("@Week", week)
                : new SqlParameter("@Week", typeof (int));
            var yearParameter = week.HasValue
                ? new SqlParameter("@Year", year)
                : new SqlParameter("@Year", typeof (int));

            var res =
                _unitOfWork.GetContext()
                    .Database.SqlQuery(typeof (double[]), "GetSumDishCounts" /*, weekParameter, yearParameter*/)
                    .ToListAsync()
                    .Result;
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void StoredFuncTest()
        {
            WeekYearDto wyDto = YearWeekHelp.GetCurrentWeekYearDto();

            //var res = _unitOfWork.GetContext().GetWeekUserOrderDishQuantyties(wyDto).ToList();
            {
                var weekParameter = new SqlParameter("@Week", wyDto.Week);
                var yearParameter = new SqlParameter("@Year", wyDto.Year);
                var res = _unitOfWork.GetContext().GetDayNames(wyDto).Result;
                //.Database.SqlQuery(typeof(WeekUserOrder), "Select * From WeekOrdersByWeekYear(@Week,@Year)",
                //    weekParameter, yearParameter)
                //    .ToListAsync()
                //    .Result; ;
                Assert.IsNotNull(res);
            }
        }

        [TestMethod]
        public void AnyStoredFuncTest()
        {
            int[] ordarray = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            int weekuserorderid = 1;
            WeekYearDto wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            double[] res = null;
            foreach (int item in ordarray)
            {
                var weekorderParameter = new SqlParameter("@Weekuserorderid", weekuserorderid);
                res = _unitOfWork.GetContext()
                    .Database.SqlQuery<double>(
                        "Select DishQuant From dbo. [FactDishQuantByWeekOrderId](@Weekuserorderid)", weekorderParameter)
                    .ToArrayAsync()
                    .Result;

            }

            //var res = _unitOfWork.GetContext().GetWeekUserOrderDishQuantyties(wyDto).ToList();
            //var weekParameter = new SqlParameter("@Week", wyDto.Week);
            //var yearParameter = new SqlParameter("@Year", wyDto.Year);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void NewMenuProcTest()
        {
            WeekYearDto wyDto = new WeekYearDto
            {
                Week = 21,
                Year = 2016
            };
            var weekParameter = new SqlParameter("@Week", wyDto.Week);
            var yearParameter = new SqlParameter("@Year", wyDto.Year);
            _unitOfWork.GetContext()
                .Database.ExecuteSqlCommand("exec CreateNewWeekMenu @Week,@Year",
                    weekParameter, yearParameter);
            ;

            var res = _weekMenuRepository.Query().Include(wm => wm.WorkingWeek.Year).Select()
                .SingleOrDefault(
                    wm => wm.WorkingWeek.WeekNumber == wyDto.Week && wm.WorkingWeek.Year.YearNumber == wyDto.Year);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void ProcMigrationTest()
        {
            string _path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"UnitTestProject1\bin\Debug", "") +
                                    @"ACSDining.Web\App_Data\DBinitial\TestProc.sql";
            var connectionString = ConfigurationManager.ConnectionStrings["ApplicationDbContext"].ConnectionString;

            var file = new FileInfo(_path);
            var script = file.OpenText().ReadToEnd();

            using (var connection = new SqlConnection(connectionString))
            {
                var server = new Server(new ServerConnection(connection));

                file = new FileInfo(_path);
                script = file.OpenText().ReadToEnd();
                server.ConnectionContext.ExecuteNonQuery(script);
            }
            var res =
                _unitOfWork.GetContext()
                    .Database.SqlQuery<object>("Select OBJECT_ID ('[new_paiment_on_new_weekorder] ', 'TR')")
                    .FirstAsync()
                    .Result;
            Assert.IsNotNull(res);

        }

        [TestMethod]
        public void InsertUserTest()
        {
            User  user = new User
                            {
                                FirstName = "TestUser",
                                LastName = "User",
                                Email = "test@test.com",
                                UserName = "testuser",
                                LastLoginTime = DateTime.UtcNow,
                                RegistrationDate = DateTime.UtcNow,
                                EmailConfirmed = true,
                                PasswordHash = (new PasswordHasher()).HashPassword("test")
                            };


            var res = _userManager.CreateAsync(user).Result;
            if (res == IdentityResult.Success)
            {
                var roleres= _userManager.AddToRoleAsync(user.Id, "Employee").Result;
            }
            _unitOfWork.SaveChanges();
            user = _userManager.FindByNameAsync("testuser").Result;
            Assert.IsNotNull(user);
            //var paiidParameter = new SqlParameter("@Paid", userweekpaiDto.Id);
            //var paivalueParameter = new SqlParameter("@PaiValue", userweekpaiDto.Paiment);

            //context.Database.ExecuteSqlCommand("exec UpdateWeekPaiment @Paid,PaiValue",
            //    paiidParameter, paivalueParameter);
        }

        [TestMethod]
        public void UpdatePaimentTest()
        {

            var paiidParameter = new SqlParameter("@Paid", 1);
            var paivalParameter = new SqlParameter("@PaiValue", 54.05);
            _unitOfWork.GetContext()
                .Database.ExecuteSqlCommand("exec UpdateWeekPaiment @Paid,@PaiValue",
                    paiidParameter, paivalParameter);
            ;
            double Pai = _unitOfWork.RepositoryAsync<WeekPaiment>().Find(1).Paiment;
            Assert.IsTrue(54.05 == Pai);
        }
    }
}
