using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ACSDining.Core.Domains;

namespace ACSDining.Web.Areas.SU_Area.Models
{
    public class DayMenuChangedMessage
    {
        public MenuForDay OldMenuForDay { get; set; }
        public MenuForDay NewMenuForDay { get; set; }

        public DayMenuChangedMessage(MenuForDay oldmenu, MenuForDay newmenu)
        {
            OldMenuForDay = oldmenu;
            NewMenuForDay = newmenu;
        }
        //public string
    }

}