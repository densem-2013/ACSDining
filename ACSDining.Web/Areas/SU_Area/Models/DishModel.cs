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
    public class DishModel
    {
        public int DishID { get; set; }
        public string Title { get; set; }
        public string ProductImage { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public bool IsSelected { get; set; }
    }
}