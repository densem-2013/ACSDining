using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ACSDining.Core.Domains
{
    public class PlanDishQuantityRelations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DishQuantityId { get; set; }
        public int DishTypeId { get; set; }

        //public int MenuForDayId { get; set; }
        //public int DayOrderMenuId { get; set; }
        public int PlannedDayOrderMenuId { get; set; }

        [JsonIgnore]
        public virtual DishQuantity DishQuantity { get; set; }

        [JsonIgnore]
        public virtual DishType DishType { get; set; }

        //[JsonIgnore]
        //public virtual MenuForDay MenuForDay { get; set; }
        //[JsonIgnore]
        //public virtual DayOrderMenu DayOrderMenu { get; set; }
        [JsonIgnore]
        public virtual PlannedDayOrderMenu PlannedDayOrderMenu { get; set; }
    }
}
