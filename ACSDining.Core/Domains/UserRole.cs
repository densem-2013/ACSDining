//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ACSDining.Core.Domains
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserRole : IdentityRole
    {
        public UserRole() : base() { }
        public UserRole(string name) : base(name) { }
        public UserRole(string name, string descr)
            : base(name)
        {
            Description = descr;
        }
        public string Description { get; set; }

    }
}
