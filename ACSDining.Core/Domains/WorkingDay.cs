using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Core.Domains
{
    public class WorkingDay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public bool IsWorking { get; set; }
        public virtual DayOfWeek DayOfWeek { get; set; }
        public virtual WorkingWeek WorkingWeek { get; set; }
    }
}
