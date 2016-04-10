using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using LinqKit;

namespace ACSDining.Service
{
    public interface IWorkDaysService:IService<WorkingWeek>
    {
        WorkWeekDto GetWorkWeekDtoByWeekYear(int week, int year);
        WorkingWeek GetWorkWeekByWeekYear(int week, int year);
        WorkingWeek UpdateWorkDays(WorkWeekDto weekModel);
    }

    public class WorkDaysService : Service<WorkingWeek>, IWorkDaysService
    {
        private readonly IRepositoryAsync<WorkingWeek> _repository;

        public WorkDaysService(IRepositoryAsync<WorkingWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public WorkWeekDto GetWorkWeekDtoByWeekYear(int week, int year)
        {
            return
                WorkWeekDto.MapWorkWeekDto(
                    _repository.Queryable().FirstOrDefault(ww => ww.WeekNumber == week && ww.Year.YearNumber == year));
        }

        public WorkingWeek GetWorkWeekByWeekYear(int week, int year)
        {
            return
                _repository.Query()
                    .Include(ww => ww.WorkingDays.Select(wd => wd.DayOfWeek))
                    .Include(ww => ww.Year)
                    .Select()
                    .FirstOrDefault(ww => ww.WeekNumber == week && ww.Year.YearNumber == year);
        }

        public WorkingWeek UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek week = _repository.Find(weekModel.WorkWeekId);

            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.Id);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            return week;
        }

    }
}
