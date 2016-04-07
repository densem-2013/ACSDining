using System;
using System.Threading.Tasks;
using System.Web.Http;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Core.HelpClasses;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WorkDays")]
    public class WorkDayApiController : ApiController
    {
        private readonly IWorkDaysService _workDayService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public WorkDayApiController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWork = unitOfWorkAsync;
            _workDayService = new WorkDaysService(_unitOfWork.RepositoryAsync<WorkingWeek>());
        }

        [HttpGet]
        [Route("{numweek}/{year}")]
        public async Task<WorkWeekDto> GetWorkWeek([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? YearWeekHelp.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;
            return
                await
                    Task.FromResult(_workDayService.GetWorkWeekDtoByWeekYear(week, yearnum));
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek workweek = _workDayService.Find(weekModel.WorkWeekId);
            if (workweek == null)
            {
                return NotFound();
            }


            _workDayService.UpdateWorkDays(weekModel);

            await _unitOfWork.SaveChangesAsync();

            return Ok();
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
