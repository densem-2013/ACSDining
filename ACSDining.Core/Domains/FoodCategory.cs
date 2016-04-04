using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class FoodCategory 
    {
        public FoodCategory()
        {
            Foods = new List<Food>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string MeasureUnit { get; set; }
        public virtual ICollection<Food> Foods { get; set; }
    }
}
