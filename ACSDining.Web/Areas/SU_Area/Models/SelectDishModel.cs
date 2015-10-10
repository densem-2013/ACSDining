using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class SelectDishModel
    {
        public List<DishModel> Items { get; set; }
        public int SelectedDishID { get; set; }
    }
}