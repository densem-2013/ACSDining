using System.Collections.Generic;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IGetExcelService : IService<WeekOrderMenu>
    {
        string GetExcelFileFromPaimentsModel(List<UserWeekPaimentDto> paimentList,
            string[] dishCategories, WorkingWeek workingWeek);
    }

    public class GetExcelService : Service<WeekOrderMenu>, IGetExcelService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public GetExcelService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public string GetExcelFileFromPaimentsModel(List<UserWeekPaimentDto> paimentList,
            string[] dishCategories, WorkingWeek workingWeek)
        {
            return _repository.GetExcelFileFromPaimentsModel(paimentList, dishCategories, workingWeek);
        }
    }
}
