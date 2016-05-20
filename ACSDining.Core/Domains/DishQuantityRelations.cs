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
        public int DayOrderMenuId { get; set; }
        
        [JsonIgnore]
        public virtual DishQuantity DishQuantity { get; set; }
        [JsonIgnore]
        public virtual DishType DishType { get; set; }
        [JsonIgnore]
        public virtual DayOrderMenu DayOrderMenu { get; set; }

    }
}
