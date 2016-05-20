using System.Collections.Generic;
using ACSDining.Core.Domains;
using ACSDining.Infrastructure.DTO;
using ACSDining.Infrastructure.Repositories;

namespace ACSDining.Infrastructure.Services
{
    public interface IWeekPaimentService
    {
        //Получить платёж пользователя по id и WeekYearDto
        WeekPaiment GetByUseridWeekYear(string userid, WeekYearDto wyDto);
        //Получить недельные оплаты
        List<WeekPaiment> GetWeekPaiments(WeekYearDto wyDto);
    }
    public class WeekPaimentService : Service<WeekPaiment>, IWeekPaimentService
    {
        private readonly IRepositoryAsync<WeekPaiment> _repository;

        public WeekPaimentService(IRepositoryAsync<WeekPaiment> repository)
            : base(repository)
        {
            _repository = repository;
        }

        public WeekPaiment GetByUseridWeekYear(string userid, WeekYearDto wyDto)
        {
            return _repository.WeekPaimentByUseridWeekYear(userid, wyDto);
        }

        public List<WeekPaiment> GetWeekPaiments(WeekYearDto wyDto)
        {
            return _repository.WeekPaiments(wyDto);
        }
    }
}
