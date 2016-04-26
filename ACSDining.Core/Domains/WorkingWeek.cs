using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public sealed class WorkingWeek 
    {
        public WorkingWeek()
        {
            WorkingDays = new List<WorkingDay>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int WeekNumber { get; set; }
        public ICollection<WorkingDay> WorkingDays { get; set; }
        public Year Year { get; set; }
        public bool CanBeChanged { get; set; }
    }
}
