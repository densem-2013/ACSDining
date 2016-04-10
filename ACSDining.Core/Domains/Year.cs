using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public sealed class Year 
    {
        public Year()
        {
            WorkingWeeks = new List<WorkingWeek>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int YearNumber { get; set; }

        public ICollection<WorkingWeek> WorkingWeeks { get; set; }
    }
}
