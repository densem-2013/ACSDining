using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class FoodQuantityRelations 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int FoodID { get; set; }
        public int FoodQuantityID { get; set; }
        public int DishID { get; set; }

        [JsonIgnore]
        public virtual Food Food { get; set; }

        [JsonIgnore]
        public virtual FoodQuantity FoodQuantity { get; set; }

        [JsonIgnore]
        public virtual Dish Dish { get; set; }
    }
}
