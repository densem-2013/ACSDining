using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Domains
{
    public class DishDetail 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Title { get; set; }

        [Column(TypeName = "text")]
        public string Recept { get; set; }

        [Column(TypeName = "text")]
        public string Foods { get; set; }
    }
}
