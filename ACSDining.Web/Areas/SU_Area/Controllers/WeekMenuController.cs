using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.Services;
using ACSDining.Web.Areas.SU_Area.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private ApplicationDbContext _db;
        private readonly IMenuForWeekService _weekmenuService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IMfdDishPriceService _mfdDishPriceService;

        public WeekMenuController(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _db = _unitOfWork.GetContext();
            _weekmenuService = new MenuForWeekService(_unitOfWork.RepositoryAsync<MenuForWeek>());
            _mfdDishPriceService=new MfdDishPriceService(_unitOfWork.RepositoryAsync<MfdDishPriceRelations>());
        }

        // GET api/WeekMenu
        [HttpPut]
        [Route("")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromBody] WeekYearDto wyDto)
        {

            if (wyDto == null)
            {
                wyDto = YearWeekHelp.GetCurrentWeekYearDto();
            }

            WeekMenuDto
                resultdto = await Task.FromResult(_weekmenuService.GetWeekMenuDto(wyDto));

            return resultdto;
        }

        [HttpPost]
        [Route("create")]
        [ResponseType(typeof (bool))]
        public async Task<WeekYearDto> Create()
        {
            WeekYearDto nextDto = YearWeekHelp.GetNextWeekYear(YearWeekHelp.GetCurrentWeekYearDto());
            _db.CreateNewWeekMenu(nextDto);
         
            return await Task.FromResult(nextDto);
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


        [HttpGet]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDto))]
        public Task<WeekYearDto> GetNextWeekYear()
        {
            return Task.FromResult(YearWeekHelp.GetNextWeekYear(YearWeekHelp.GetCurrentWeekYearDto()));
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
        
        [HttpPut]
        [Route("menuupdatemessage")]
        public async Task<IHttpActionResult> SendEmailUpdateMenu([FromBody] MenuUpdateMessageDto messageDto)
        {
            if (messageDto == null)
            {
                return BadRequest();
            }
            MessageService.SendUpdateDayMenuMessage(_unitOfWork.RepositoryAsync<WeekOrderMenu>(), messageDto);

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
            MenuForWeek menu = _weekmenuService.FindById(messageDto.WeekMenuId);
            menu.OrderCanBeCreated = true;
            _db.Entry(menu).State=EntityState.Modified;
            await _db.SaveChangesAsync();
            ApplicationUserManager userManager = new ApplicationUserManager(new UserStore<User>(_unitOfWork.GetContext()));

            List<User> users = userManager.Users.ToList();

            MessageService.SendEmailAsync(users, MessageTopic.MenuCreated, messageDto.DateTime);

            return Ok(true);
        }

        /// <summary>
        /// Подтверждает выбор рабочих дней в неделе
        /// </summary>
        /// <param name="wwUpDto"></param>
        /// <returns></returns>
        /// 
        [HttpPut]
        [Route("workweekapply")]
        public async Task<IHttpActionResult> WorkWeekCommit([FromBody] WorkDaysUpdateDto wwUpDto)
        {
            if (wwUpDto == null)
            {
                return BadRequest();
            }

            WorkingWeek week = _weekmenuService.FindById(wwUpDto.MenuId).WorkingWeek;
            for (int i = 0; i < wwUpDto.WorkDays.Length; i++)
            {
                WorkingDay workday = week.WorkingDays.ElementAt(i);
                workday.IsWorking = wwUpDto.WorkDays[i];
                _db.Entry(workday).State=EntityState.Modified;
            }

            MenuForWeek menu = _weekmenuService.FindById(wwUpDto.MenuId);
            menu.WorkingDaysAreSelected = true;
            _db.Entry(menu).State = EntityState.Modified;

            week.CanBeChanged = false;
            await _db.SaveChangesAsync();
            return Ok(true);
        }


        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayDto menuforday)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _mfdDishPriceService.UpdateMenuForDay(menuforday);

            MenuForWeek mfw = _unitOfWork.RepositoryAsync<MenuForWeek>().GetWeekMenuMfdContains(menuforday.Id);

            mfw.SummaryPrice = mfw.MenuForDay.Sum(mw => mw.TotalPrice);

            _unitOfWork.GetContext().Entry(mfw).State = EntityState.Modified;
            _unitOfWork.SaveChanges();

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
            _db.MenuForWeeks.Remove(mfw);

            await _db.SaveChangesAsync();

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