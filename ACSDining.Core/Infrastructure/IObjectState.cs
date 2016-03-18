
using System.ComponentModel.DataAnnotations.Schema;

namespace ACSDining.Core.Infrastructure
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}