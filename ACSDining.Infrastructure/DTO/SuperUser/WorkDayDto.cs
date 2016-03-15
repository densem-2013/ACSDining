using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WorkDayDto
    {
        public int WorkdayId { get; set; }
        public bool IsWorking { get; set; }
        public int DayNumber { get; set; }

        public static WorkDayDto MapDto(WorkingDay workday)
        {
            return new WorkDayDto
            {
                WorkdayId = workday.ID,
                IsWorking = workday.IsWorking,
                DayNumber = workday.DayOfWeek.ID
            };
        }
    }
}
