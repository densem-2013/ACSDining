using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Core.DTO.SuperUser
{
    public class MenuForDayDto
    {
        public int ID { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModelDto> Dishes { get; set; }
        public bool Editing { get; set; }

        public static MenuForDayDto MapDto(MenuForDay daymenu)
        {
            return new MenuForDayDto
            {
                ID = daymenu.ID,
                DayOfWeek = daymenu.WorkingDay.DayOfWeek.Name,
                TotalPrice = daymenu.TotalPrice,
                Dishes = daymenu.Dishes.Select(DishModelDto.MapDto).ToList()
            };
        }
    }
}