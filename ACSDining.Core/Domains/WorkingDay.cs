using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public sealed class WorkingDay 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //public DateTime DateTime { get; set; }
        public bool IsWorking { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public WorkingWeek WorkingWeek { get; set; }
    }
}
