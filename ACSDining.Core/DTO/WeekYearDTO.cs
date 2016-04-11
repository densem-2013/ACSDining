using ACSDining.Core.Domains;

namespace ACSDining.Core.DTO
{
    public class WeekYearDto
    {
        public int Week { get; set; }
        public int Year { get; set; }

        public static WeekYearDto MapDto(WorkingWeek ww)
        {
            return new WeekYearDto
            {
                Week = ww.WeekNumber,
                Year = ww.Year.YearNumber
            };
        }
    }
}