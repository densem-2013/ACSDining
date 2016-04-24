using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Services;
using ACSDining.Web.Areas.SU_Area.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser, Employee")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekmenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IOrderMenuService _orderMenuService;

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _db = _unitOfWork.GetContext();
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _orderMenuService = new OrderMenuService(_unitOfWork.RepositoryAsync<WeekOrderMenu>());
        }

        // GET api/WeekMenu
        [HttpPut]
        [Route("")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromBody] WeekYearDto wyDto)
        {

            if (wyDto==null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            MenuForWeek weekmenu = _weekmenuService.GetWeekMenuByWeekYear(wyDto);
            if (weekmenu != null)
            {
                return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork, weekmenu));
            }
            else
            {
                return null;
            }

        }

        [HttpPost]
        [Route("create")]
        [ResponseType(typeof (bool))]
        public IHttpActionResult Create()
        {
            WeekYearDto nextDto = YearWeekHelp.GetNextWeekYear(YearWeekHelp.GetCurrentWeekYearDto());
            MenuForWeek weekmenu = _weekmenuService.CreateByWeekYear(nextDto);

            //_db.Entry(weekmenu).State=EntityState.Added;
            //_db.MenuForWeeks.Attach(weekmenu);
            try
            {
                _db.MenuForWeeks.Add(weekmenu);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                    
                throw;
            }

            weekmenu = _weekmenuService.GetWeekMenuByWeekYear(nextDto);
            bool res = weekmenu != null;
            //return await Task.FromResult(WeekMenuDto.MapDto(_unitOfWork, weekmenu, true));
            return Ok(res);
        }

        [HttpGet]
        [Route("isnextweekmenuexists")]
        public Task<bool> IsNexWeekMenuExist()
        {
            WeekYearDto curWeekYear = YearWeekHelp.GetCurrentWeekYearDto();
            WeekYearDto nextWeekYearDto = YearWeekHelp.GetNextWeekYear(curWeekYear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextWeekYearDto);

            return Task.FromResult(nextWeek != null);
        }


        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDto))]
        public Task<WeekYearDto> GetNextWeekYear([FromBody] WeekYearDto weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof (WeekYearDto))]
        public Task<WeekYearDto> GetPrevWeekYear([FromBody] WeekYearDto weekyear)
        {
            return Task.FromResult(YearWeekHelp.GetPrevWeekYear(weekyear));
        }

        [HttpGet]
        [Route("curWeekYear")]
        [ResponseType(typeof (Int32))]
        public async Task<WeekYearDto> CurrentWeekYear()
        {
            return await Task.FromResult(YearWeekHelp.GetCurrentWeekYearDto());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {

            string[] categories = MapHelper.GetCategoriesStrings(_unitOfWork);

            return categories;

        }

        [HttpGet]
        [Route("WeekNumbers")]
        public async Task<List<int>> GetWeekNumbers()
        {
            return await Task.FromResult(_weekmenuService.WeekNumbers());
        }

        [HttpPut]
        [Route("menuupdatemessage")]
        public async Task<IHttpActionResult> SendEmailUpdateMenu([FromBody] MenuUpdateMessageDto messageDto)
        {
            if (messageDto == null)
            {
                return BadRequest();
            }
            int[] daymenusid = messageDto.UpdatedDayMenu;
            List<User> userBooking =
                daymenusid.SelectMany(dmi => _orderMenuService.GetUsersMedeBooking(dmi)).Distinct().ToList();
            await
                MessageService.SendEmailAsync(userBooking, MessageTopic.MenuChanged, messageDto.DateTime,
                    messageDto.Message);

            return Ok(true);
        }
        /// <summary>
        /// Отправляет всем клиентам уведомление о возможности сделать заказ на вновь созданное меню
        /// </summary>
        /// <param name="messageDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("setasorderable")]
        public async Task<IHttpActionResult> SetMenuAsCanMadeOrder([FromBody] MenuCanBeOrderedMessageDto messageDto)
        {
            if (messageDto == null)
            {
                return BadRequest();
            }

            ApplicationUserManager userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));

            List<User> users = userManager.Users.ToList();

            await MessageService.SendEmailAsync(users, MessageTopic.MenuCreated, messageDto.DateTime);

            return Ok(true);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayDto menuforday)
        {
            MenuForDay menuFd = _unitOfWork.RepositoryAsync<MenuForDay>().Find(menuforday.Id);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (menuFd == null)
            {
                return NotFound();
            }
            menuFd.Dishes =
                menuforday.Dishes.Select(d =>  _db.Dishes.FirstOrDefault(dbd => dbd.DishID == d.DishId)).ToList();

            menuFd.TotalPrice = menuforday.TotalPrice;

            menuFd.Dishes.ForEach(x =>
            {
                if (x!=null)
                {
                    _db.Entry(x).State = EntityState.Modified;
                    _db.Dishes.Attach(x);
                }
            });


            MenuForWeek mfwModel =
                _weekmenuService.GetAll()
                    .ToList()
                    .FirstOrDefault(mfw => mfw.MenuForDay.Any(mfd => mfd.ID == menuforday.Id));

            if (mfwModel != null)
            {
                mfwModel.SummaryPrice = mfwModel.MenuForDay.Sum(mfd => mfd.TotalPrice);

                _db.Entry(mfwModel).State = EntityState.Modified;
                _db.MenuForWeeks.Attach(mfwModel);
                _db.SaveChanges();
            }

            return StatusCode(HttpStatusCode.OK);
        }
        
        // DELETE api/WeekMenu/5
        [HttpDelete]
        [Route("delete/{menuid}")]
        [ResponseType(typeof (MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int menuid)
        {
            MenuForWeek mfw = _weekmenuService.FindById(menuid);
            if (mfw == null)
            {
                return NotFound();
            }
            try
            {
                //WorkingWeek weekdel = mfw.WorkingWeek;
                //_db.Entry(weekdel).State = EntityState.Deleted;
                //_db.WorkingWeeks.Attach(weekdel);
                //_db.Entry(mfw).State = EntityState.Deleted;
                //_db.MenuForWeeks.Attach(mfw);


                _db.MenuForWeeks.Remove(mfw);

                await _db.SaveChangesAsync();

            }
            catch (Exception)
            {
                    
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

       
    }
}