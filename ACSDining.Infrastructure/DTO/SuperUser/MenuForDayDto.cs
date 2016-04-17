using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class MenuForDayDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModelDto> Dishes { get; set; }
        public bool CanBeEditing { get; set; }
        public bool OrderCanBeChanged { get; set; }

        public static MenuForDayDto MapDto(IUnitOfWorkAsync unitOfWork, MenuForDay daymenu, bool forWeekOrder = false)
        {
            List<DishModelDto> dmodels = null;
            var canchangeorder = true;

            if (!forWeekOrder)
            {
                dmodels = new List<DishModelDto>();
                if (daymenu.Dishes.Any())
                {
                    dmodels = daymenu.Dishes.Select(DishModelDto.MapDto).ToList();
                    canchangeorder = daymenu.OrderCanBeChanged;
                }
                else
                {
                    int catLength = MapHelper.GetDishCategoriesCount(unitOfWork);

                    for (int i = 0; i < catLength; i++)
                    {
                        DishType firstOrDefault = unitOfWork.Repository<DishType>().Find(i + 1);
                        if (firstOrDefault != null)
                            dmodels.Add(new DishModelDto
                            {
                                //DishId = i + 1,
                                Title = ":",
                                Price = 0,
                                Category = firstOrDefault.Category,
                                Foods = ":"
                            });
                    }
                }

            }

            return new MenuForDayDto
            {
                Id = daymenu.ID,
                DayOfWeek = !forWeekOrder ? daymenu.WorkingDay.DayOfWeek.Name : null,
                TotalPrice = daymenu.TotalPrice,
                Dishes = dmodels,
                OrderCanBeChanged = canchangeorder
            };
        }

    }
}