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
        public int DishQuantityID { get; set; }
        public int PlannedOrderMenuID { get; set; }
        public int DishTypeID { get; set; }
        public int DayOfWeekID { get; set; }
        public int MenuForWeekID { get; set; }
        public int OrderMenuID { get; set; }

        [JsonIgnore]
        public virtual DishQuantity DishQuantity { get; set; }
        [JsonIgnore]
        public virtual DishType DishType { get; set; }
        [JsonIgnore]
        public virtual DayOfWeek DayOfWeek { get; set; }
        [JsonIgnore]
        public virtual MenuForWeek MenuForWeek { get; set; }
        [JsonIgnore]
        public virtual OrderMenu OrderMenu { get; set; }
        [JsonIgnore]
        public virtual PlannedOrderMenu PlannedOrderMenu { get; set; }
    }
}
