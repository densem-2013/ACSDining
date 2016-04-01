using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACSDining.Core.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ACSDining.Core.Domains
{
    public partial class UserRoleRelation : IdentityUserRole, IObjectState
    {
        [NotMapped]
        public ObjectState ObjectState { get; set; }
        //public new string UserId { get; set; }
        //public new string RoleId { get; set; }
    }
}
