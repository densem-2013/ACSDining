using System.Collections.Generic;

namespace ACSDining.Core.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int ID { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MFD_models { get; set; }

        //public WeekMenuDto()
        //{
        //}

        //public WeekMenuDto(MenuForWeek wmenu,bool emptyDishes=false)
        //{
        //    ID = wmenu.ID;
        //    WeekNumber = wmenu.WeekNumber;
        //    SummaryPrice = wmenu.SummaryPrice;
        //    YearNumber = wmenu.Year.YearNumber;
        //    if (emptyDishes)
        //    {
        //        ApplicationDbContext context=new ApplicationDbContext();
        //        List<DishType> dtypes = context.DishTypes.ToList();
        //        MFD_models = new List<MenuForDayDto>();
        //        foreach (MenuForDay mfd in wmenu.MenuForDay)
        //        {
        //            var dmodels=new List<DishModelDto>();
        //            for (int i = 0; i <4; i++)
        //            {
        //                DishType firstOrDefault = dtypes.FirstOrDefault(dt=>dt.Id==i+1);
        //                if (firstOrDefault != null)
        //                    dmodels.Add(new DishModelDto
        //                    {
        //                        DishID = i+1,
        //                        Title = "_",
        //                        Price = 0,
        //                        Category = firstOrDefault.Category,
        //                        Foods = "_"
        //                    });
        //            }

        //            MFD_models.Add(new MenuForDayDto
        //           {
        //               ID = mfd.ID,
        //               DayOfWeek = mfd.DayOfWeek.Name,
        //               TotalPrice = mfd.TotalPrice,
        //               Dishes = dmodels
        //           });
        //        }
        //    }
        //    else
        //    {
        //        MFD_models = wmenu.MenuForDay.ToList().Select(m => new MenuForDayDto()
        //        {
        //            ID = m.ID,
        //            DayOfWeek = m.DayOfWeek.Name,
        //            TotalPrice = m.TotalPrice,
        //            Dishes = m.Dishes.AsEnumerable().Select(dm => new DishModelDto()
        //            {
        //                DishID = dm.DishID,
        //                Title = dm.Title,
        //                ProductImage = dm.ProductImage,
        //                Price = dm.Price,
        //                Category = dm.DishType.Category
        //            }).ToList()

        //        }).ToList();
        //    }
        //}
    }
}