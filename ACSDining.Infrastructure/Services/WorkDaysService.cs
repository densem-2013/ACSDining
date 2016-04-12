using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using LinqKit;

namespace ACSDining.Infrastructure.Services
{
    public interface IWorkDaysService:IService<WorkingWeek>
    {
        WorkWeekDto GetWorkWeekDtoByWeekYear(WeekYearDto wyDto);
        WorkingWeek GetWorkWeekByWeekYear(WeekYearDto wyDto);
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

        public WorkWeekDto GetWorkWeekDtoByWeekYear(WeekYearDto wyDto)
        {
            return
                WorkWeekDto.MapWorkWeekDto(
                    _repository.Queryable().FirstOrDefault(ww => ww.WeekNumber == wyDto.Week && ww.Year.YearNumber == wyDto.Year));
        }

        public WorkingWeek GetWorkWeekByWeekYear(WeekYearDto wyDto)
        {
            return
                _repository.Query()
                    .Include(ww => ww.WorkingDays.Select(wd => wd.DayOfWeek))
                    .Include(ww => ww.Year)
                    .Select()
                    .FirstOrDefault(ww => ww.WeekNumber == wyDto.Week && ww.Year.YearNumber == wyDto.Year);
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
