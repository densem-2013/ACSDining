using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.SuperUser.Menu
{
    public class ForMenuExcelDto
    {
        public int MenuId { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public string MenuTitle { get; set; }
    }
}
