using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WeekMenu")]
    //[EnableCors(origins: "http://http://localhost:4229", headers: "*", methods: "*")]
    public class WeekMenuController : ApiController
    {
        private readonly IMenuForWeekService _weekmenuService;

        public WeekMenuController(IMenuForWeekService weekmenuService)
        {
            _weekmenuService = weekmenuService;
        }



        // GET api/WeekMenu
        [HttpGet]
        [Route("")]
        [Route("{numweek}")]
        [Route("{numweek}/{year}")]
        [ResponseType(typeof (WeekMenuDto))]
        public async Task<WeekMenuDto> GetWeekMenu([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;

            var dto = _weekmenuService.WeekMenuDtoByWeekYear(week, yearnum);

            return dto;
        }

        [HttpGet]
        [Route("nwMenuExist")]
        public  Task<bool> IsNexWeekMenuExist(WeekYearDTO weekyear)
        {
            WeekYearDTO nextweeknumber = UnitOfWork.GetNextWeekYear(weekyear);
            MenuForWeek nextWeek =
                _weekmenuService.GetWeekMenuByWeekYear(nextweeknumber.Week,nextweeknumber.Year);

            return Task.FromResult(nextWeek != null);
        }

        [HttpGet]
        [Route("nextWeekMenu")]
        public async Task<WeekMenuDto> GetNextWeekMenu(WeekYearDTO weekyear)
        {
            return await Task.FromResult(_weekmenuService.GetNextWeekMenu(weekyear));
        }

        [HttpPut]
        [Route("nextWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetNextWeekYear([FromBody]WeekYearDTO weekyear)
        {
            return Task.FromResult(UnitOfWork.GetNextWeekYear(weekyear));
        }

        [HttpPut]
        [Route("prevWeekYear")]
        [ResponseType(typeof(WeekYearDTO))]
        public Task<WeekYearDTO> GetPrevWeekYear([FromBody]WeekYearDTO weekyear)
        {
            return Task.FromResult(UnitOfWork.GetPrevWeekYear(weekyear));
        }

        [HttpGet]
        [Route("curWeekNumber")]
        [ResponseType(typeof (Int32))]
        public async Task<Int32> CurrentWeekNumber()
        {
            return await Task.FromResult(UnitOfWork.CurrentWeek());
        }

        [HttpGet]
        [Route("categories")]
        [ResponseType(typeof (string[]))]
        public async Task<string[]> GetCategories()
        {
            return await _weekmenuService.GetCategories().ToArrayAsync();
        }

        [HttpGet]
        [Route("WeekNumbers")]
        public async Task<List<int>> GetWeekNumbers()
        {
            return await Task.FromResult(_weekmenuService.WeekNumbers());
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateMenuForDay([FromBody] MenuForDayDto menuforday)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _weekmenuService.UpdateMenuForDay(menuforday);
            return StatusCode(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("create")]
        public async Task<WeekMenuDto> CreateNextWeekMenu(WeekYearDTO weekyear)
        {
            return await Task.FromResult(_weekmenuService.CreateNextWeekMenu(weekyear));
        }

        // DELETE api/WeekMenu/5
        [HttpDelete]
        [Route("delete/{menuid}")]
        [ResponseType(typeof (MenuForWeek))]
        public async Task<IHttpActionResult> DeleteMenuForWeek(int menuid)
        {

            var res = await _weekmenuService.DeleteMenuForWeek(menuid);

            return Ok(res);
        }

    }
}