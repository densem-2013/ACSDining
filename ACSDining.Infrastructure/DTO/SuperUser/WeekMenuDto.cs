using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int Id { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MfdModels { get; set; }

        public static WeekMenuDto MapDto(IUnitOfWork unitOfWork, MenuForWeek wmenu,
            bool emptyDishes = false)
        {
            if (wmenu == null) return null;
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                Id = wmenu.ID,
                WeekYear = WeekYearDto.MapDto(wmenu.WorkingWeek),
                SummaryPrice = wmenu.SummaryPrice
            };
            if (emptyDishes)
            {
                List<DishType> dtypes = unitOfWork.Repository<DishType>().Queryable().ToList();
                dtoModel.MfdModels = new List<MenuForDayDto>();
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

                    dtoModel.MfdModels.Add(new MenuForDayDto
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
                dtoModel.MfdModels = wmenu.MenuForDay.ToList().Select(MenuForDayDto.MapDto).ToList();
            }
            return dtoModel;
        }
    }

}