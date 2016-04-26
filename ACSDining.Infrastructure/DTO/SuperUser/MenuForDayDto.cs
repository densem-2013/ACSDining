using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.HelpClasses;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class MenuForDayDto
    {
        public int Id { get; set; }
        public WorkDayDto WorkDay { get; set; }
        public double TotalPrice { get; set; }
        public List<DishModelDto> Dishes { get; set; }
        public bool CanBeEditing { get; set; }
        public bool OrderCanBeChanged { get; set; }
        public bool OrderWasBooking { get; set; }

        public static MenuForDayDto MapDto(IUnitOfWorkAsync unitOfWork, MenuForDay daymenu, bool forWeekOrder = false)
        {
            List<DishModelDto> dmodels = null;
            var canchangeorder = true;

            if (!forWeekOrder)
            {
                dmodels = new List<DishModelDto>();
                if (daymenu.Dishes.Any())
                {
                    dmodels = daymenu.Dishes.OrderBy(d=>d.DishType.Id).Select(DishModelDto.MapDto).ToList();
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
                WorkDay = !forWeekOrder ? WorkDayDto.MapDto(daymenu.WorkingDay) : null,
                TotalPrice = daymenu.TotalPrice,
                Dishes = dmodels,
                OrderCanBeChanged = canchangeorder,
                OrderWasBooking = unitOfWork.RepositoryAsync<WeekOrderMenu>().GetUsersMadeOrder(daymenu.ID).Any()
            };
        }

    }
}