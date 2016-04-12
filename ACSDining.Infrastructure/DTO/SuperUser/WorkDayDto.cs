using ACSDining.Core.Domains;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WorkDayDto
    {
        public int WorkdayId { get; set; }
        public bool IsWorking { get; set; }
        public int DayNumber { get; set; }
        public string DayName { get; set; }

        public static WorkDayDto MapDto(WorkingDay workday)
        {
            return new WorkDayDto
            {
                WorkdayId = workday.Id,
                IsWorking = workday.IsWorking,
                DayNumber = workday.DayOfWeek.Id,
                DayName = workday.DayOfWeek.Name
            };
        }
    }
}
