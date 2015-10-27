using System.Collections.Generic;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class MenuForDayModel
    {
        public int ID { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModel> Dishes { get; set; }
        public bool Editing { get; set; }
    }
}