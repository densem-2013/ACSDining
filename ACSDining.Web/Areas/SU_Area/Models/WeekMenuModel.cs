using System.Collections.Generic;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class WeekMenuModel
    {
        public int ID { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayModel> MFD_models { get; set; }
    }
}