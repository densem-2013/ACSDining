using System.Collections.Generic;
using System.Linq;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.Identity;
using ACSDining.Infrastructure.Repositories;
using ACSDining.Infrastructure.UnitOfWork;

namespace ACSDining.Infrastructure.DTO.SuperUser
{
    public class WeekPaimentDto
    {
        public WeekYearDto WeekYearDto { get; set; }
        //недельные оплаты каждого клиента 
        public List<UserWeekPaimentDto> UserWeekPaiments{ get; set; }
        //Сумма оплат по заказу каждого блюда на неделе
        public double[] SummaryDishPaiments { get; set; }
        //названия рабочих дней недели
        public string[] DayNames { get; set; }
        //цены на единицу каждого блюда
        public double[] WeekDishPrices { get; set; }
        //su может редактировать заявку и оплату
        public bool SuCanChangeOrder { get; set; }

        public static WeekPaimentDto GetMapDto(IRepositoryAsync<WeekPaiment> repository , WeekYearDto wyDto)
        {
            ApplicationDbContext context = repository.Context;
            MenuForWeek menuForWeek = repository.GetRepositoryAsync<MenuForWeek>().GetWeekMenuByWeekYear(wyDto);

            if (menuForWeek == null) return null;
            List<WeekPaiment> weekPaiments = repository.WeekPaiments(wyDto).OrderBy(wo => wo.WeekOrderMenu.User.UserName).ToList();

            return new WeekPaimentDto
            {
                WeekYearDto = wyDto,
                UserWeekPaiments = weekPaiments.Select(updto => UserWeekPaimentDto.MapDto(context, updto)).ToList(),
                SummaryDishPaiments = context.SumWeekPaimentsByDishes(wyDto).Result,
                DayNames = context.GetDayNames(wyDto,true).Result,
                WeekDishPrices = context.GetWeekDishPrices(wyDto).Result,
                SuCanChangeOrder = menuForWeek.SUCanChangeOrder
            };
        }
    }
}
