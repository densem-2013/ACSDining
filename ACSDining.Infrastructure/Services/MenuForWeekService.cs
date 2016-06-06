using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IMenuForWeekService : IService<MenuForWeek>
    {
        MenuForWeek GetWeekMenuByWeekYear(WeekYearDto wyDto);
        WeekMenuDto GetWeekMenuDto(WeekYearDto wyDto);
        IQueryable<MenuForWeek> GetAll();

        Task<bool> DeleteMenuForWeek(int menuid);
        MenuForWeek FindById(int menuid);

        WorkingWeek GetWorkWeekByWeekYear(WeekYearDto wyDto);
    }

    public class MenuForWeekService : Service<MenuForWeek>, IMenuForWeekService
    {
        private readonly IRepositoryAsync<MenuForWeek> _repository;

        public MenuForWeekService(IRepositoryAsync<MenuForWeek> repository)
            : base(repository)
        {
            _repository = repository;
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

        public WeekMenuDto GetWeekMenuDto(WeekYearDto wyDto)
        {
            return _repository.MapWeekMenuDto(wyDto);
        }
    }
}
