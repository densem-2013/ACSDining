//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ACSDining.Core.Domains
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DishQuantity
    {
        public DishQuantity()
        {
            this.DishTypes = new HashSet<DishType>();
            this.DayOfWeeks = new HashSet<DayOfWeek>();
            this.MenuForWeeks = new HashSet<MenuForWeek>();
            this.OrderMenus = new HashSet<OrderMenu>();
            this.PlannedOrderMenus = new HashSet<PlannedOrderMenu>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public double Quantity { get; set; }
        //public int PlannedOrderMenuID { get; set; }
        //public int DishTypeID { get; set; }
        //public int DayOfWeekID { get; set; }
        //public int MenuForWeekID { get; set; }
        //public int OrderMenuID { get; set; }

        [JsonIgnore]
       // [InverseProperty("DishQuantities")]
        public virtual ICollection<DishType> DishTypes { get; set; }
        [JsonIgnore]
        //[InverseProperty("DishQuantities")]
        public virtual ICollection<DayOfWeek> DayOfWeeks { get; set; }
        [JsonIgnore]
        //[InverseProperty("DishQuantities")]
        public virtual ICollection<MenuForWeek> MenuForWeeks { get; set; }
        [JsonIgnore]
        //[InverseProperty("DishQuantities")]
        public virtual ICollection<OrderMenu> OrderMenus { get; set; }
        [JsonIgnore]
        //[InverseProperty("DishQuantities")]
        public virtual ICollection<PlannedOrderMenu> PlannedOrderMenus { get; set; }
    }
}
