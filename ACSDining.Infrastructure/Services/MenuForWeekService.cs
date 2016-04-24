using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IMenuForWeekService: IService<MenuForWeek>
    {
        //Получить цены на каждое блюдо в меню на указанной неделе
        double[] GetUnitWeekPricesByWeekYear(WeekYearDto wyDto, int catLenth);
        
        MenuForWeek GetWeekMenuByWeekYear(WeekYearDto wyDto);

        List<int> WeekNumbers();
        
        IQueryable<MenuForWeek> GetAll();
        
        Task<bool> DeleteMenuForWeek(int menuid);
        MenuForWeek CreateByWeekYear(WeekYearDto wyDto);
        MenuForWeek FindById(int menuid);
    }

    public class MenuForWeekService : Service<MenuForWeek>, IMenuForWeekService
    {
        private readonly IRepositoryAsync<MenuForWeek> _repository;

        public MenuForWeekService(IRepositoryAsync<MenuForWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public MenuForWeek CreateByWeekYear(WeekYearDto wyDto)
        {
            return _repository.CreateMenuForWeekOnWeekYear(wyDto);
        }
        public double[] GetUnitWeekPricesByWeekYear(WeekYearDto wyDto, int catLenth)
        {
            return _repository.UnitWeekPricesByWeekYear(wyDto, catLenth);
        }

        public List<int> WeekNumbers()
        {
            return _repository.GetWeekNumbers();
        }

        public MenuForWeek FindById(int menuid)
        {
            return _repository.GetMenuById(menuid);
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
