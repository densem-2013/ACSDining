using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WorkDaysUpdateDto
    {
        public int MenuId { get; set; }
        public bool[] WorkDays { get; set; }
    }
}
