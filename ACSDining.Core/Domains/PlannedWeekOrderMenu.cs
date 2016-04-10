using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class PlannedWeekOrderMenu 
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonIgnore]
        public virtual WeekOrderMenu WeekOrderMenu { get; set; }
        [JsonIgnore]
        public virtual ICollection<PlannedDayOrderMenu> PlannedDayOrderMenus { get; set; }
    }
}
