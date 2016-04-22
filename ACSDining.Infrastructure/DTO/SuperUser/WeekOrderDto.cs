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
        //недельный заказ каждого клиента 
        public List<UserWeekOrderDto> UserWeekOrders { get; set; }
        public WeekMenuDto MenuForWeekDto { get; set; }
        //Суммарное количество заказа каждого блюда на неделе
        public double[] SummaryDishQuantities { get; set; }
        //названия рабочих дней недели
        public string[] DayNames { get; set; }
        //цены на единицу каждого блюда
        public double[] WeekDishPrices { get; set; }

        public static WeekOrderDto GetMapDto(IUnitOfWorkAsync unitOfWork, WeekYearDto wyDto, int catLength)
        {
            List<WeekOrderMenu> weekOrderMenus = unitOfWork.RepositoryAsync<WeekOrderMenu>().OrdersMenuByWeekYear(wyDto);

            MenuForWeek menuForWeek = unitOfWork.RepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

            return new WeekOrderDto
            {
                WeekYearDto = wyDto,
                UserWeekOrders =
                    weekOrderMenus.Select(woDto => UserWeekOrderDto.MapDto(unitOfWork, woDto, catLength, true)).ToList(),
                MenuForWeekDto = WeekMenuDto.MapDto(unitOfWork, menuForWeek, true),
                SummaryDishQuantities =
                    unitOfWork.RepositoryAsync<WeekOrderMenu>().SummaryDishesQuantities(wyDto, catLength),
                DayNames =
                    menuForWeek.WorkingWeek.WorkingDays.Where(wd => wd.IsWorking)
                        .Select(wd => wd.DayOfWeek.Name)
                        .ToArray(),
                WeekDishPrices = unitOfWork.RepositoryAsync<MenuForWeek>().UnitWeekPricesByWeekYear(wyDto, catLength)
            };
        }
    }
}
