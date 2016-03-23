using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Infrastructure.DTO.SuperUser;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IGetExcelService
    {
        string PaimentsDtoToExcelFile(PaimentsDTO paimodel);
    }
    public class GetExcelService : Service<OrderMenu>, IGetExcelService
    {
        private readonly IRepositoryAsync<OrderMenu> _repository;

        public GetExcelService(IRepositoryAsync<OrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public string PaimentsDtoToExcelFile(PaimentsDTO paimodel)
        {
            return _repository.GetExcelFileFromPaimentsModel(paimodel);
        }
    }
}
