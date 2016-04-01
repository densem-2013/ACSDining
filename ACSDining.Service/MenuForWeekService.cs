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
            try
            {
                return _repository.GetWeekNumbers();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public double SummaryPrice(UserOrdersDTO usorder, int numweek, int year)
        {
            try
            {
                return _repository.GetSummaryPrice(usorder, numweek, year);
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        public IQueryable<MenuForWeek> GetAll()
        {
            return _repository.Queryable();
        }

        public WeekMenuDto GetNextWeekMenu(WeekYearDTO weekyear)
        {
            return _repository.GetNextWeekMenuByCurrentWekYear(weekyear);
        }

        public IQueryable<string> GetCategories()
        {
            try
            {
                return _repository.GetRepository<DishType>().Queryable().ToList().OrderBy(d => d.Id).Select(dt => dt.Category).AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateMenuForDay(MenuForDayDto menuforday)
        {
            _repository.UpdateMenuForDay(menuforday);
        }

        public MenuForWeek CreateNextWeekMenu(WeekYearDTO weekyear)
        {
            return _repository.CreateNextWeekMenu(weekyear);
        }

        public async Task<bool> DeleteMenuForWeek(int menuid)
        {
            return await _repository.DeleteAsync(menuid);
        }

        public MenuForWeek GetWeekMenuByWeekYear(int week, int year)
        {
            return _repository.WeekMenuByWeekYear(week, year);
        }
    }
}
