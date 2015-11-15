using System.Collections.Generic;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class OrdersDTO
    {
        public int WeekNumber { get; set; }
        public List<UserOrdesDTO> UserOrders { get; set; }
        public int YearNumber { get; set; }
    }
}