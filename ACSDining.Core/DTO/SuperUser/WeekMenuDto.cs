using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int ID { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MFD_models { get; set; }
    }

}