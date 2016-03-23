using System;
using System.Threading.Tasks;
using System.Web.Http;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WorkDays")]
    public class WorkDayApiController : ApiController
    {
        private readonly IWorkDaysService _workingWeekService;

        public WorkDayApiController(IWorkDaysService workingWeekService)
        {
            _workingWeekService = workingWeekService;
        }

        [HttpGet]
        [Route("{numweek}/{year}")]
        public async Task<WorkWeekDto> GetWorkWeek([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;
            return
                await
                    Task.FromResult(_workingWeekService.GetWorkweekByWeekYear(week, yearnum));
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateWorkDays(WorkWeekDto weekModel)
        {
            return Ok(_workingWeekService.UpdateWorkDays(weekModel));
        }
    }
}
