using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DAL;

namespace ACSDining.Core.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int Id { get; set; }
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MFD_models { get; set; }

        public static WeekMenuDto MapDto(Infrastructure.DAL.UnitOfWork unitOfWork, MenuForWeek wmenu,
            bool emptyDishes = false)
        {
            if (wmenu == null) return null;
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                Id = wmenu.ID,
                WeekNumber = wmenu.WorkingWeek.WeekNumber,
                SummaryPrice = wmenu.SummaryPrice,
                YearNumber = wmenu.WorkingWeek.Year.YearNumber
            };
            if (emptyDishes)
            {
                List<DishType> dtypes = unitOfWork.Repository<DishType>().Queryable().ToList();
                dtoModel.MFD_models = new List<MenuForDayDto>();
                foreach (MenuForDay mfd in wmenu.MenuForDay)
                {
                    var dmodels = new List<DishModelDto>();
                    for (int i = 0; i < 4; i++)
                    {
                        DishType firstOrDefault = dtypes.FirstOrDefault(dt => dt.Id == i + 1);
                        if (firstOrDefault != null)
                            dmodels.Add(new DishModelDto
                            {
                                DishID = i + 1,
                                Title = "_",
                                Price = 0,
                                Category = firstOrDefault.Category,
                                Foods = "_"
                            });
                    }

                    dtoModel.MFD_models.Add(new MenuForDayDto
                    {
                        Id = mfd.ID,
                        DayOfWeek = mfd.WorkingDay.DayOfWeek.Name,
                        TotalPrice = mfd.TotalPrice,
                        Dishes = dmodels
                    });
                }
            }
            else
            {
                dtoModel.MFD_models = wmenu.MenuForDay.ToList().Select(MenuForDayDto.MapDto).ToList();
            }
            return dtoModel;
        }
    }

}