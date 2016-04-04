using System;
using System.Threading.Tasks;
using System.Web.Http;
using ACSDining.Core.Domains;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Service;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WorkDays")]
    public class WorkDayApiController : ApiController
    {
        private readonly IWorkDaysService _workDayService;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public WorkDayApiController(IUnitOfWorkAsync unitOfWorkAsync, IWorkDaysService workDayService)
        {
            _workDayService = workDayService;
            _unitOfWork = unitOfWorkAsync;
        }

        [HttpGet]
        [Route("{numweek}/{year}")]
        public async Task<WorkWeekDto> GetWorkWeek([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            int week = numweek ?? UnitOfWork.CurrentWeek();
            int yearnum = year ?? DateTime.Now.Year;
            return
                await
                    Task.FromResult(_workDayService.GetWorkWeekByWeekYear(week, yearnum));
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
