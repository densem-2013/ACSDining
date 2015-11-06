using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Core.Domains
{
    public class PlannedOrderMenu
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Key, ForeignKey("OrderMenu")]
        //public int OrderMenuId { get; set; }

        //[JsonIgnore]
        //public virtual OrderMenu OrderMenu { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
        [JsonIgnore]
        public virtual MenuForWeek MenuForWeek { get; set; }
        //[JsonIgnore]
        //public virtual ICollection<DishQuantity> DishQuantities { get; set; }
    }
}
