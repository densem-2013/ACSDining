using System;
using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WorkWeekDto
    {
        public int WorkWeekId { get; set; }
        public int WeekNumber { get; set; }
        public int YearNumber { get; set; }
        public List<WorkDayDto> WorkDays { get; set; }
        public bool CanBeChanged { get; set; }

        public static WorkWeekDto MapWorkWeekDto(WorkingWeek workweek)
        {
            return new WorkWeekDto
            {
                WeekNumber = workweek.WeekNumber,
                YearNumber = workweek.Year.YearNumber,
                WorkDays = workweek.WorkingDays.Select(WorkDayDto.MapDto).ToList(),
                CanBeChanged = UnitOfWork.WeekDaysCanBeChanged(workweek)
            };
        }
    }
}
