using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class WeekMenuModel
    {
        public int ID { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayModel> MFD_models { get; set; }

        public WeekMenuModel()
        {
        }

        public WeekMenuModel(MenuForWeek wmenu,bool emptyDishes=false)
        {
            List<DishModel> dmodels=null;
            ID = wmenu.ID;
            WeekNumber = wmenu.WeekNumber;
            SummaryPrice = wmenu.SummaryPrice;
            YearNumber = wmenu.Year.YearNumber;
            if (emptyDishes)
            {
                ApplicationDbContext context=new ApplicationDbContext();
                List<DishType> dtypes = context.DishTypes.ToList();
                MFD_models = new List<MenuForDayModel>();
                foreach (MenuForDay mfd in wmenu.MenuForDay)
                {
                    dmodels=new List<DishModel>();
                    for (int i = 0; i <4; i++)
                    {
                        dmodels.Add(new DishModel
                        {
                            DishID = i+1,
                            Title = "_",
                            Price = 0,
                            Category = dtypes.FirstOrDefault(dt=>dt.Id==i+1).Category,
                            Foods = "_"
                        });
                    }

                   MFD_models.Add(new MenuForDayModel
                   {
                       ID = mfd.ID,
                       DayOfWeek = mfd.DayOfWeek.Name,
                       TotalPrice = mfd.TotalPrice,
                       Dishes = dmodels
                   });
                }
            }
            else
            {
                MFD_models = wmenu.MenuForDay.ToList().Select(m => new MenuForDayModel()
                {
                    ID = m.ID,
                    DayOfWeek = m.DayOfWeek.Name,
                    TotalPrice = m.TotalPrice,
                    Dishes = m.Dishes.AsEnumerable().Select(dm => new DishModel()
                    {
                        DishID = dm.DishID,
                        Title = dm.Title,
                        ProductImage = dm.ProductImage,
                        Price = dm.Price,
                        Category = dm.DishType.Category
                    }).ToList()

                }).ToList();
            }
        }
    }
}