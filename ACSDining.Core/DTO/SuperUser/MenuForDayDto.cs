using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using Newtonsoft.Json;

namespace ACSDining.Core.DTO.SuperUser
{
    public class MenuForDayDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModelDto> Dishes { get; set; }
        public bool Editing { get; set; }

        public static MenuForDayDto MapDto(MenuForDay daymenu)
        {
            return new MenuForDayDto
            {
                Id = daymenu.ID,
                DayOfWeek = daymenu.WorkingDay.DayOfWeek.Name,
                TotalPrice = daymenu.TotalPrice,
                Dishes = daymenu.Dishes.Select(DishModelDto.MapDto).ToList()
            };
        }
    }
}