using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.Employee;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IMenuForWeekService
    {
        double[] UnitWeekPrices(int menuforweekid);

        WeekMenuDto MapWeekMenuDto(MenuForWeek wmenu, bool emptyDishes = false);

        WeekMenuDto WeekMenuDtoByWeekYear(int week, int year);

        List<int> WeekNumbers();
    }

    public class MenuForWeekService : Service<MenuForWeek>, IMenuForWeekService
    {
        private readonly IRepositoryAsync<MenuForWeek> _repository;

        public MenuForWeekService(IRepositoryAsync<MenuForWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public double[] UnitWeekPrices(int menuforweekid)
        {
            return _repository.GetUnitWeekPrices(menuforweekid);
        }
        public WeekMenuDto MapWeekMenuDto(MenuForWeek wmenu, bool emptyDishes = false)
        {
            return _repository.GetMapWeekMenuDto(wmenu, emptyDishes);
        }

        public WeekMenuDto WeekMenuDtoByWeekYear(int week, int year)
        {
            return _repository.GetWeekMenuDtoByWeekYear(week, year);
        }

        public List<int> WeekNumbers()
        {
            return _repository.GetWeekNumbers();
        } 
   }
}
