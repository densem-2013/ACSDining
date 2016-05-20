using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Core.Domains
{
    public class MfdDishPriceRelations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DishPriceId { get; set; }
        public int DishId { get; set; }
        public int MenuForDayId { get; set; }
        public DishPrice DishPrice { get; set; }
        public Dish Dish { get; set; }
        public MenuForDay MenuForDay { get; set; }
    }
}
