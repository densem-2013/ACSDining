using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class OrdersDTO
    {
        public int Id { get; set; }
        public List<UserOrdesDTO> UserOrders { get; set; } 
    }
}