using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using LinqKit;

namespace ACSDining.Infrastructure.Repositories
{
    public static class WorkWeekRepository
    {
        public static WorkingWeek WorkWeekByWeekYear(this IRepositoryAsync<WorkingWeek> repository, WeekYearDto wyDto)
        {
            return repository.Query()
                    .Include(ww => ww.WorkingDays.Select(wd => wd.DayOfWeek))
                    .Include(ww => ww.Year)
                    .Select()
                    .FirstOrDefault(ww => ww.WeekNumber == wyDto.Week && ww.Year.YearNumber == wyDto.Year);
        }

        public static void DayUpdates(this IRepositoryAsync<WorkingWeek> repository, WorkWeekDto weekModel)
        {

            WorkingWeek week = repository.Find(weekModel.WorkWeekId);

            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.Id);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            repository.Update(week);
        }
    }
}
