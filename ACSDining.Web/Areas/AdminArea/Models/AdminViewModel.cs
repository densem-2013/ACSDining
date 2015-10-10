using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ACSDining.Areas.AdminArea.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Укажите имя")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Укажите фамилию")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Укажите логин")]
        public string LogIn { get; set; }

        [Display(Name = "Укажите роль пользователя")]
        public IEnumerable<SelectListItem> RolesList { get; set; }

        [Required]
        [Display(Name = "Может заказывать еду в столовой")]
        public bool IsDiningClient { get; set; }
    }
}