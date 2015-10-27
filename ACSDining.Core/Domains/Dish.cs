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
    
    public partial class Dish
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int DishID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public double Price { get; set; }

        [JsonIgnore]
        public virtual DishType DishType { get; set; }
        [JsonIgnore]
        public virtual DishDetail DishDetail { get; set; }
        [JsonIgnore]
        public virtual ICollection<DishQuantity> DishQuantities { get; set; }
        [JsonIgnore]
        public virtual ICollection<MenuForDay> MenusFD { get; set; }
    }
}
