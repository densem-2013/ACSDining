using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class WorkingWeek 
    {
        public WorkingWeek()
        {
            WorkingDays = new List<WorkingDay>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int WeekNumber { get; set; }
        public virtual ICollection<WorkingDay> WorkingDays { get; set; }
        public virtual Year Year { get; set; }
    }
}
