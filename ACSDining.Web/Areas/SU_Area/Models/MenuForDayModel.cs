using ACSDining.Core.Domains;
using DelegateDecompiler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class MenuForDayModel
    {
        public int ID { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModel> Dishes { get; set; }
        public bool Editing { get; set; }
        [Computed]
        public void Editable()
        {
            Editing = true;
        }
        public void UnEditable()
        {
            Editing = false;
        }

        //[Computed]
        //public string SelectDishId()
        //{
        //    return Dishes.FirstOrDefault(d=>d.IsSelected == true).DishID;
        //}
    }
}