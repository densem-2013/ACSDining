using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO.SuperUser;

namespace ACSDining.Service
{
    public interface IWorkDaysService:IService<WorkingWeek>
    {
        WorkWeekDto GetWorkWeekByWeekYear(int week, int year);
        int UpdateWorkDays(WorkWeekDto weekModel);
    }
}