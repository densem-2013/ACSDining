using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class UpdateDishDto
    {
        public int DishId { get; set; }
        public string Title { get; set; }
        public string ProductImage { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int MenuForDayId { get; set; }
    }
}
