using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class WorkingDay : Entity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public bool IsWorking { get; set; }
        public virtual DayOfWeek DayOfWeek { get; set; }
        public virtual WorkingWeek WorkingWeek { get; set; }
    }
}
