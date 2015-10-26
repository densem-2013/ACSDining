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
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public double Quantity { get; set; }

        [JsonIgnore]
        public virtual Dish Dish { get; set; }
        [JsonIgnore]
        public virtual MenuForDay MenuForDay { get; set; }
        [JsonIgnore]
        public virtual MenuForWeek WeekMenu { get; set; }
        [JsonIgnore]
        public virtual OrderMenu Order { get; set; }
    }
}
