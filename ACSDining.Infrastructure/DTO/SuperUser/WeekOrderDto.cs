using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WeekOrderDto
    {
        public WeekYearDto WeekYearDto { get; set; }
        public List<UserWeekOrderDto> UserWeekOrders { get; set; }
        public WeekMenuDto MenuForWeekDto { get; set; }
        public double[] SummaryDishQuantities { get; set; }
        public string[] DayNames { get; set; }

        public static WeekOrderDto MapDto(IUnitOfWorkAsync unitOfWork, List<WeekOrderMenu> weekOrderMenus, int catLength)
        {
            WeekOrderMenu first = weekOrderMenus.FirstOrDefault();
            WeekYearDto wyDto = null;
            if (first != null)
            {
                wyDto = WeekYearDto.MapDto(first.MenuForWeek.WorkingWeek);
            }
            return new WeekOrderDto
            {
                WeekYearDto = wyDto,
                UserWeekOrders =
                    weekOrderMenus.Select(woDto => UserWeekOrderDto.MapDto(unitOfWork, woDto, catLength, true)).ToList(),
                MenuForWeekDto = WeekMenuDto.MapDto(unitOfWork, first != null ? first.MenuForWeek : null, true),
                SummaryDishQuantities =
                    unitOfWork.RepositoryAsync<WeekOrderMenu>().SummaryDishesQuantities(wyDto, catLength),
                DayNames =
                    first != null
                        ? first.DayOrderMenus.Select(dom => dom.MenuForDay.WorkingDay.DayOfWeek.Name).ToArray()
                        : null
            };
        }
    }
}
