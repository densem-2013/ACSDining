using System.ComponentModel.DataAnnotations.Schema;
using ACSDining.Core.Infrastructure;

namespace ACSDining.Infrastructure.DAL
{
    public abstract class Entity : IObjectState
    {
        [NotMapped]
        public ObjectState ObjectState { get; set; }
    }
}