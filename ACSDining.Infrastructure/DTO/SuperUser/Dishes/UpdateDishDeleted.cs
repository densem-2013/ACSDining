using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACSDining.Infrastructure.DTO.SuperUser.Dishes
{
    public class UpdateDishDeleted
    {
        public int DishId { get; set; }
        public bool Deleted { get; set; }
    }
}
