using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO
{
    public class MenuUpdateMessageDto
    {
        public string DateTime { get; set; }
        public string Message { get; set; }
        public int[] UpdatedDayMenu { get; set; }
    }

    public class MenuCanBeOrderedMessageDto
    {
        public int WeekMenuId { get; set; }
        public string DateTime { get; set; }
    }
}
