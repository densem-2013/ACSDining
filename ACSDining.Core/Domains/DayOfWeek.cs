using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class DayOfWeek
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public System.DayOfWeek SysDayOfWeek { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
