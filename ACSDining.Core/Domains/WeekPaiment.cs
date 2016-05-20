using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class WeekPaiment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Note { get; set; }
        public double Paiment { get; set; }
        public bool WeekIsPaid { get; set; }
        public double PreviousWeekBalance { get; set; }
        [Required]
        public virtual WeekOrderMenu WeekOrderMenu { get; set; }
    }
}
