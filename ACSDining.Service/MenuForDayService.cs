using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;

namespace ACSDining.Service
{
    public interface IMenuforDayService : IService<MenuForDay>
    {

    }

    public class MenuForDayService : Service<MenuForDay>, IMenuforDayService
    {
        private readonly IRepositoryAsync<MenuForDay> _repository;

        public MenuForDayService(IRepositoryAsync<MenuForDay> repository)
            : base(repository)
        {
            _repository = repository;
        }

    }
}
