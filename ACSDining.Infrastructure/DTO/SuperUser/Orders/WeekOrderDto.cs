using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser.Orders
{
    public class WeekOrderDto
    {
        public WeekYearDto WeekYearDto { get; set; }
        //недельный заказ каждого клиента 
        public List<UserWeekOrderDto> UserWeekOrders { get; set; }
        //Суммарное количество заказа каждого блюда на неделе
        public double[] SummaryDishQuantities { get; set; }
        //названия рабочих дней недели
        public string[] DayNames { get; set; }
        //названия всех дней недели
        public string[] AllDayNames { get; set; }
        //цены на единицу каждого блюда
        public double[] WeekDishPrices { get; set; }
        //SU может редактировать заказ
        public bool SuCanChangeOrder { get; set; }

        public static WeekOrderDto GetMapDto(IUnitOfWorkAsync unitOfWork, WeekYearDto wyDto, bool needfact = true)
        {
            ApplicationDbContext context = unitOfWork.GetContext();

            MenuForWeek menuForWeek = unitOfWork.RepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

            if (menuForWeek == null) return null;

            List<WeekOrderMenu> weekOrderMenus = unitOfWork.RepositoryAsync<WeekOrderMenu>()
                .OrdersMenuByWeekYear(wyDto).Where(word=>word.User.IsExisting).OrderBy(wo=>wo.User.LastName).ToList();


            return new WeekOrderDto
            {
                WeekYearDto = wyDto,
                SuCanChangeOrder = menuForWeek.SUCanChangeOrder,
                UserWeekOrders =
                    weekOrderMenus.Select(woDto => UserWeekOrderDto.MapDto(context, woDto)).ToList(),
                DayNames = context.GetDayNames(wyDto, true).Result,
                WeekDishPrices = context.GetWeekDishPrices(wyDto).Result,
                SummaryDishQuantities = context.GetFactSumWeekUserCounts(wyDto).Result,
                AllDayNames = context.GetDayNames(wyDto).Result
            };
        }
    }
}
