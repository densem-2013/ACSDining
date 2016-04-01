using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using LinqKit;

namespace ACSDining.Service
{
    public interface IWorkDaysService:IService<WorkingWeek>
    {
        WorkWeekDto GetWorkWeekByWeekYear(int week, int year);
        int UpdateWorkDays(WorkWeekDto weekModel);
    }

    public class WorkDaysService : Service<WorkingWeek>, IWorkDaysService
    {
        private readonly IRepositoryAsync<WorkingWeek> _repository;

        public WorkDaysService(IRepositoryAsync<WorkingWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public WorkWeekDto GetWorkWeekByWeekYear(int week, int year)
        {
            return
                WorkWeekDto.MapWorkWeekDto(
                    _repository.Queryable().FirstOrDefault(ww => ww.WeekNumber == week && ww.Year.YearNumber == year));
        }

        public int UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek week = _repository.Find(weekModel.WorkWeekId);

            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.ID);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            return weekModel.WorkWeekId;
        }
    }
}
