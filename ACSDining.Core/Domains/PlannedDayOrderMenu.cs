using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ACSDining.Core.Domains
{
    public class PlannedDayOrderMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonIgnore]
        public virtual DayOrderMenu DayOrderMenu { get; set; }
        [JsonIgnore]
        public virtual PlannedWeekOrderMenu PlannedWeekOrderMenu { get; set; }
    }
}
