using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.HelpClasses;

namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class WorkWeekDto
    {
        public int WorkWeekId { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public List<WorkDayDto> WorkDays { get; set; }
        public bool CanBeChanged { get; set; }

        public static WorkWeekDto MapWorkWeekDto(WorkingWeek workweek)
        {
            return new WorkWeekDto
            {
                WorkWeekId = workweek.ID,
                WeekYear = WeekYearDto.MapDto(workweek),
                WorkDays = workweek.WorkingDays.Select(WorkDayDto.MapDto).ToList(),
                CanBeChanged = YearWeekHelp.WeekDaysCanBeChanged(workweek)
            };
        }
    }
}
