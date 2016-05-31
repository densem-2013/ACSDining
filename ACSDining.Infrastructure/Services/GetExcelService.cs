using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IGetExcelService : IService<WeekOrderMenu>
    {
        string GetExcelFileFromPaimentsModel(ForExcelDataDto feDto);
        string GetExcelFileFromOrdersModel(ForExcelDataDto feDto);
        string GetExcelFromPlanOrdersModel(ForExcelDataDto feDto);
    }

    public class GetExcelService : Service<WeekOrderMenu>, IGetExcelService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public GetExcelService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public string GetExcelFileFromPaimentsModel(ForExcelDataDto feDto)
        {
            return _repository.GetExcelFileFromPaimentsModel(feDto);
        }
        public string GetExcelFileFromOrdersModel(ForExcelDataDto feDto)
        {
            return _repository.GetFactOrdersExcelFileWeekYearDto(feDto);
        }
        public string GetExcelFromPlanOrdersModel(ForExcelDataDto feDto)
        {
            return _repository.GetPlanOrdersExcelFileWeekYearDto(feDto);
        }
    }
}
