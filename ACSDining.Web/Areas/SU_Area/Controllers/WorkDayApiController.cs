using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ACSDining.Core.DAL;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.DTO.SuperUser;
using WebGrease.Css.Extensions;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [RoutePrefix("api/WorkDays")]
    public class WorkDayApiController : ApiController
    {
        private readonly IRepository<WorkingWeek> _workingWeekRepository;

        public WorkDayApiController(IUnitOfWork unitOfWork)
        {
            var unitOfWork1 = unitOfWork;
            _workingWeekRepository = unitOfWork1.Repository<WorkingWeek>();
        }

        [HttpGet]
        [Route("{numweek}/{year}")]
        public async Task<WorkWeekDto> GetWorkWeek([FromUri] int? numweek = null, [FromUri] int? year = null)
        {
            numweek = numweek ?? UnitOfWork.CurrentWeek();
            year = year ?? DateTime.Now.Year;
            return
                await
                    Task.FromResult(
                        WorkWeekDto.MapWorkWeekDto(
                            _workingWeekRepository.Find(ww => ww.WeekNumber == numweek && ww.Year.YearNumber == year).Result));
        }

        [HttpPut]
        [Route("update")]
        [ResponseType(typeof (int))]
        public async Task<IHttpActionResult> UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek week = _workingWeekRepository.Find(w => w.ID == weekModel.WorkWeekId).Result;
            if (week == null)
            {
                return NotFound();
            }
            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.ID);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            return Ok(weekModel.WorkWeekId);
        }
    }
}
