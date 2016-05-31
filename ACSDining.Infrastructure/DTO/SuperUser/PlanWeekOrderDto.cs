using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class PlanWeekOrderDto
    {
        public WeekYearDto WeekYearDto { get; set; }
        //недельный заказ каждого клиента 
        public List<PlanUserWeekOrderDto> UserWeekOrders { get; set; }
        //Суммарное количество заказа каждого блюда на неделе
        public double[] SummaryDishQuantities { get; set; }
        //названия рабочих дней недели
        public string[] DayNames { get; set; }
        //цены на единицу каждого блюда
        public double[] WeekDishPrices { get; set; }
        //SU может редактировать заказ
        public bool SuCanChangeOrder { get; set; }

        public static PlanWeekOrderDto GetMapDto(IUnitOfWorkAsync unitOfWork, WeekYearDto wyDto, bool needfact = true)
        {
            ApplicationDbContext context = unitOfWork.GetContext();

            MenuForWeek menuForWeek = unitOfWork.RepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

            if (menuForWeek == null) return null;

            List<PlannedWeekOrderMenu> weekOrderMenus = unitOfWork.RepositoryAsync<PlannedWeekOrderMenu>()
                .OrdersMenuByWeekYear(wyDto).OrderBy(wo => wo.User.UserName).ToList();


            return new PlanWeekOrderDto
            {
                WeekYearDto = wyDto,
                SuCanChangeOrder = false,
                UserWeekOrders =
                    weekOrderMenus.Select(woDto => PlanUserWeekOrderDto.MapDto(context, woDto)).ToList(),
                DayNames = context.GetDayNames(wyDto, true).Result,
                WeekDishPrices = context.GetWeekDishPrices(wyDto).Result,
                SummaryDishQuantities = context.GetPlanSumWeekUserCounts(wyDto).Result
            };
        }
    }
}
