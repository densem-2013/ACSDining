using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;
using LinqKit;

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
        void CreateByWeekYear(WeekYearDto wyDto);
        MenuForWeek FindById(int menuid);

        WorkingWeek GetWorkWeekByWeekYear(WeekYearDto wyDto);
        WorkingWeek UpdateWorkDays(WorkWeekDto weekModel);
    }

    public class MenuForWeekService : Service<MenuForWeek>, IMenuForWeekService
    {
        private readonly IRepositoryAsync<MenuForWeek> _repository;

        public MenuForWeekService(IRepositoryAsync<MenuForWeek> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public void CreateByWeekYear(WeekYearDto wyDto)
        {
             _repository.CreateMenuForWeekOnWeekYear(wyDto);
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
        public WorkingWeek GetWorkWeekByWeekYear(WeekYearDto wyDto)
        {
            return
                _repository.WorkWeekByWeekYear(wyDto);
        }

        public WorkingWeek UpdateWorkDays(WorkWeekDto weekModel)
        {
            WorkingWeek week = GetWorkWeekByWeekYear(weekModel.WeekYear);

            week.WorkingDays.ForEach(x =>
            {
                var firstOrDefault = weekModel.WorkDays.FirstOrDefault(wd => wd.WorkdayId == x.Id);
                var isWorking = firstOrDefault != null && firstOrDefault.IsWorking;
                x.IsWorking = isWorking;
            });
            return week;
        }
    }
}
