using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Service
{
    public interface IMenuForWeekService: IService<MenuForWeek>
    {
        double[] UnitWeekPrices(int menuforweekid);

        WeekMenuDto MapWeekMenuDto(MenuForWeek wmenu, bool emptyDishes = false);

        WeekMenuDto WeekMenuDtoByWeekYear(int week, int year);

        MenuForWeek GetWeekMenuByWeekYear(int week, int year);

        List<int> WeekNumbers();

        double SummaryPrice(UserOrdersDTO usorder, int numweek, int year);

        IQueryable<MenuForWeek> GetAll();

        WeekMenuDto GetNextWeekMenu(WeekYearDTO weekyear);

        IQueryable<string> GetCategories();

        void UpdateMenuForDay(MenuForDayDto menuforday);

        MenuForWeek CreateNextWeekMenu(WeekYearDTO weekyear);

        Task<bool> DeleteMenuForWeek(int menuid);
    }
}