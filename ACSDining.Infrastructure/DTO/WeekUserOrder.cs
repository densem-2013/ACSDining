using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO
{
    public class WeekUserOrder
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int WeekOrderId { get; set; }
    }
}
