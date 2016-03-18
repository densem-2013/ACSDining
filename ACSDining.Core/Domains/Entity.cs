using System.ComponentModel.DataAnnotations.Schema;
using ACSDining.Core.Infrastructure;

namespace ACSDining.Core.Domains
{
    public abstract class Entity : IObjectState
    {
        [NotMapped]
        public ObjectState ObjectState { get; set; }
    }
}