using System.Collections.Generic;

namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class MenuDishChange
    {
        public int DayMenuId { get; set; }
        public int OldDishId { get; set; }
        public int NewDishId { get; set; }
        public string Category { get; set; }
    }

    public class MenuUpdateMessageDto
    {
        public string DateTime { get; set; }
        public string Message { get; set; }
        public List<MenuDishChange> UpdatedDayMenu { get; set; }
    }

    public class MenuCanBeOrderedMessageDto
    {
        public int WeekMenuId { get; set; }
        public string DateTime { get; set; }
    }
}
