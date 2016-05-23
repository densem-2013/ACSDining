﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

//using ACSDining.Core.Infrastructure;

namespace ACSDining.Core.Domains
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public partial class User : IdentityUser
    {
        public User()
            : base()
        {
            CanMakeBooking = true;
            IsExisting = true;
        }
        
        [Required]
        public virtual string FirstName { get; set; }

        [Required]
        public virtual string LastName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public virtual DateTime LastLoginTime { get; set; }

        public double Balance { get; set; }
        //Допустимый размер задолженности
        public double AllowableDebt { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public virtual DateTime RegistrationDate { get; set; }
        public virtual bool CanMakeBooking { get; set; }
        public virtual bool IsExisting { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, string> manager)
        {
            // Îáðàòèòå âíèìàíèå, ÷òî authenticationType äîëæåí ñîâïàäàòü ñ òèïîì, îïðåäåëåííûì â CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Çäåñü äîáàâüòå óòâåðæäåíèÿ ïîëüçîâàòåëÿ
            return userIdentity;
        }
    }
}
