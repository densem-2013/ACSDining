using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class DishQuantityRelations 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DishQuantityId { get; set; }
        public int DishTypeId { get; set; }

        public int MenuForDayId { get; set; }
        public int DayOrderMenuId { get; set; }
        public int PlannedDayOrderMenuId { get; set; }

        //public int MenuForWeekId { get; set; }
        //public int WeekOrderMenuId { get; set; }
        //public int PlannedWeekOrderMenuId { get; set; }

        [JsonIgnore]
        public virtual DishQuantity DishQuantity { get; set; }

        [JsonIgnore]
        public virtual DishType DishType { get; set; }

        [JsonIgnore]
        public virtual MenuForDay MenuForDay { get; set; }
        [JsonIgnore]
        public virtual DayOrderMenu DayOrderMenu { get; set; }
        [JsonIgnore]
        public virtual PlannedDayOrderMenu PlannedDayOrderMenu { get; set; }

        //[JsonIgnore]
        //public virtual MenuForWeek MenuForWeek { get; set; }

        //[JsonIgnore]
        //public virtual WeekOrderMenu WeekOrderMenu { get; set; }

        //[JsonIgnore]
        //public virtual PlannedWeekOrderMenu PlannedWeekOrderMenu { get; set; }
    }
}
