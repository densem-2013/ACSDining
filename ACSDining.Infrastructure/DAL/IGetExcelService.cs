using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Service
{
    public interface IGetExcelService: IService<OrderMenu>
    {
        string PaimentsDtoToExcelFile(PaimentsDTO paimodel);
    }
}