using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.DTO;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Service
{
    public interface IMenuForWeekService: IService<MenuForWeek>
    {
        double[] UnitWeekPrices(int menuforweekid, string[] categories);
        
        MenuForWeek GetWeekMenuByWeekYear(WeekYearDto wyDto);

        List<int> WeekNumbers();

        double SummaryPrice(UserWeekOrderDto usorder, int numweek, int year);

        IQueryable<MenuForWeek> GetAll();
        
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

        public double[] UnitWeekPrices(int menuforweekid,string[] categories)
        {
            return _repository.GetUnitWeekPrices(menuforweekid,categories);
        }

        public List<int> WeekNumbers()
        {
            return _repository.GetWeekNumbers();
        }

        public double SummaryPrice(UserWeekOrderDto usorder, int numweek, int year)
        {
            return _repository.GetSummaryPrice(usorder, numweek, year);
        }

        public IQueryable<MenuForWeek> GetAll()
        {
            return _repository.Queryable();
        }

        public async Task<bool> DeleteMenuForWeek(int menuid)
        {
            return await _repository.DeleteAsync(menuid);
        }

        public MenuForWeek GetWeekMenuByWeekYear(WeekYearDto wyDto)
        {
            return _repository.GetWeekMenuByWeekYear(wyDto);
        }
    }
}
