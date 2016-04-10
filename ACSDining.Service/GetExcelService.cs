using System.Collections.Generic;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IGetExcelService: IService<WeekOrderMenu>
    {
        string PaimentsDtoToExcelFile(PaimentsDto paimodel, WorkingWeek workweek, List<DishType> dishtypes);
    }
    public class GetExcelService : Service<WeekOrderMenu>, IGetExcelService
    {
        private readonly IRepositoryAsync<WeekOrderMenu> _repository;

        public GetExcelService(IRepositoryAsync<WeekOrderMenu> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public string PaimentsDtoToExcelFile(PaimentsDto paimodel, WorkingWeek workweek, List<DishType> dishtypes)
        {
            return _repository.GetExcelFileFromPaimentsModel(paimodel, workweek, dishtypes);
        }
    }
}
