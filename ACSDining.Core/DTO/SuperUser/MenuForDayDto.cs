using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class MenuForDayDto
    {
        public int ID { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModelDto> Dishes { get; set; }
        public bool Editing { get; set; }
    }
}