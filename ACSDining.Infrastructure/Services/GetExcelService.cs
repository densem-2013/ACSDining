using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IGetExcelService : IService<WeekOrderMenu>
    {
        string/*Task<FileStream> */GetExcelFileFromPaimentsModel(WeekPaimentDto wpDto);
    }

    public class GetExcelService : Service<WeekOrderMenu>, IGetExcelService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public GetExcelService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public string/*Task<FileStream> */ GetExcelFileFromPaimentsModel(WeekPaimentDto wpDto)
        {
            return _repository.GetExcelFileFromPaimentsModel(wpDto);
        }
    }
}
