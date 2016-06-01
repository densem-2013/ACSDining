using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WeekMenuDto
    {
        public int Id { get; set; }
        public WeekYearDto WeekYear { get; set; }
        public double SummaryPrice { get; set; }
        public List<MenuForDayDto> MfdModels { get; set; }
        //На это недельное меню может быть сделан заказ
        public bool OrderCanBeCreated { get; set; }
        //Представление информации о рабочей неделе
        public bool[] WorkWeekDays { get; set; }
        //Рабочие дни установлены
        public bool WorkingDaysAreSelected { get; set; }
        public string[] DayNames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="wmenu"></param>
        /// <param name="wyDto"></param>
        /// <returns></returns>
        public static WeekMenuDto MapDto(IUnitOfWorkAsync unitOfWork, MenuForWeek wmenu, WeekYearDto wyDto)
        {
            if (wmenu == null) return null;
            WeekMenuDto dtoModel = new WeekMenuDto
            {
                Id = wmenu.ID,
                SummaryPrice = wmenu.SummaryPrice,
                MfdModels =
                    wmenu.MenuForDay.ToList()
                        .Select(mfd =>new MenuForDayDto
                        {
                            Id = mfd.ID,
                            TotalPrice = mfd.TotalPrice,
                            Dishes = mfd.DishPriceMfdRelations.Select(dp=>dp.Dish).OrderBy(d => d.DishType.Id).Select(d=>
                            {
                                var mfdDishPriceRelations = mfd.DishPriceMfdRelations.FirstOrDefault(dp => dp.DishId == d.DishID);
                                if (mfdDishPriceRelations != null)
                                    return new Dishes.DishModelDto
                                    {
                                        DishId = d.DishID,
                                        Title = d.Title,
                                        ProductImage = d.ProductImage,
                                        Price =
                                            mfdDishPriceRelations
                                                .DishPrice.Price,
                                        Category = mfdDishPriceRelations.Dish.DishType != null ? mfdDishPriceRelations.Dish.DishType.Category : null,
                                        Description = mfdDishPriceRelations.Dish.Description
                                    };
                                return null;
                            }).ToList(),
                            OrderCanBeChanged = mfd.OrderCanBeChanged,
                            OrderWasBooking = unitOfWork.RepositoryAsync<WeekOrderMenu>().GetUsersMadeOrder(mfd.ID).Any()
                            
                        })
                        .ToList(),
                OrderCanBeCreated = wmenu.OrderCanBeCreated,
                WorkWeekDays = wmenu.WorkingWeek.WorkingDays.Select(wd=>wd.IsWorking).ToArray(),
                WorkingDaysAreSelected = wmenu.WorkingDaysAreSelected,
                DayNames = unitOfWork.GetContext().GetDayNames(wyDto).Result,
                WeekYear=wyDto
            };

            return dtoModel;
        }
    }

}