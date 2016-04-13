using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ACSDining.Core.Domains
{
    public class DayOrderMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double DayOrderSummaryPrice { get; set; }
        //Пользователь может изменить заказ на это меню
        public bool OrderCanBeChanged { get; set; }
        [JsonIgnore]
        public virtual MenuForDay MenuForDay { get; set; }
        [JsonIgnore]
        public virtual WeekOrderMenu WeekOrderMenu { get; set; }
    }
}
