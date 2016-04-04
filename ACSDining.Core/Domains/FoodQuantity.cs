using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class FoodQuantity 
    {
        public FoodQuantity()
        {
            Relations = new List<FoodQuantityRelations>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public double Quantity { get; set; }
        public virtual ICollection<FoodQuantityRelations> Relations { get; set; }
    }
}
