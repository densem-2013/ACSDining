using System.Collections.Generic;

namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class MenuForDayDto
    {
        public int Id { get; set; }
        public double TotalPrice { get; set; }
        public List<Dishes.DishModelDto> Dishes { get; set; }
        public bool OrderCanBeChanged { get; set; }
        public bool OrderWasBooking { get; set; }
        public bool DayMenuCanBeChanged { get; set; }
    }
}