using ACSDining.Core.Domains;
using ACSDining.Web.Areas.SU_Area.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Globalization;

namespace ACSDining.Web.Areas.SU_Area.Controllers
{
    public class SU_Controller : Controller
    {
        // GET: /SU_Area/SU_/
        public ActionResult WeekMenu()
        {
            ViewBag.Title = "Welcome " + Session["Lname"] + " " + Session["Fname"];
            return View();
        }

    }
}