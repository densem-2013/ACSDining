using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser.Menu;
using ACSDining.Infrastructure.Repositories;
using ForExcelDataDto = ACSDining.Infrastructure.DTO.SuperUser.Orders.ForExcelDataDto;

namespace ACSDining.Infrastructure.Services
{
    public interface IGetExcelService : IService<WeekOrderMenu>
    {
        string GetExcelFileFromPaimentsModel(ForExcelDataDto feDto);
        string GetExcelFileFromOrdersModel(ForExcelDataDto feDto);
        string GetExcelFromPlanOrdersModel(ForExcelDataDto feDto);
        string GetMenuExcel(ForMenuExcelDto feDto);
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

        public string GetMenuExcel(ForMenuExcelDto feDto)
        {
            return _repository.GetRepositoryAsync<MenuForWeek>().GetMenuExcelFile(feDto);
        }
    }
}
