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
        ApplicationDbContext DB { get; set; }
        List<WeekMenuModel> WeekModels { get; set; }
        List<DishModel> Dishes { get; set; }

        public SU_Controller()
        {
            DB = new ApplicationDbContext();
            WeekModels = DB.MenuForWeek.Select(wmenu => new WeekMenuModel()
            {
                ID = wmenu.ID,
                WeekNumber = wmenu.WeekNumber,
                SummaryPrice = wmenu.SummaryPrice,
                MFD_models = wmenu.MenuForDay.Select(m => new MenuForDayModel()
                {
                    ID = m.ID,
                    DayOfWeek = m.DayOfWeek.Name,
                    Dishes = DB.Dishes.AsEnumerable().Join(
                            m.Dishes.AsEnumerable(),
                            dm => dm.DishID,
                            md => md.DishID,
                            (dm, md) => new DishModel()
                                {
                                    DishID = dm.DishID,
                                    Title = dm.Title,
                                    ProductImage = dm.ProductImage,
                                    Price = dm.Price,
                                    Category = dm.DishType.Category,
                                    IsSelected = true
                                }).ToList()
                }).ToList()
            }).ToList();

            Dishes = DB.Dishes.AsEnumerable().Select(d => new DishModel()
            {
                DishID = d.DishID,
                Title = d.Title,
                ProductImage = d.ProductImage,
                Price = d.Price,
                Category = d.DishType.Category,
                IsSelected = false

            }).ToList();
        }
        // GET: /SU_Area/SU_/
        public ActionResult Index()
        {
            ViewBag.Title = "Welcome " + Session["Lname"] + " " + Session["Fname"];
            //WeekMenuModel model = WeekModels.FirstOrDefault(wm => wm.WeekNumber == DB.CurrentWeek());
            return View(/*model*/);
        }

        //// GET: /SU_Area/Dishes/
        //public ActionResult Dishes()
        //{
        //    ViewBag.Title = "Dishes Management";
        //    return View();
        //}
        // GET: /SU_Area/Dishes/
        public ActionResult SelectDish(int dishID)
        {
            DishModel dish = Dishes.Find(dm => dm.DishID == dishID);

            SelectDishModel model = new SelectDishModel()
            {
                SelectedDishID = dishID,
                Items = Dishes.Where(d => String.Equals(dish.Category, d.Category)).Select(dm => new DishModel()
                {
                    DishID = dm.DishID,
                    Title = dm.Title,
                    ProductImage = dm.ProductImage,
                    Price = dm.Price,
                    Category = dish.Category,
                    IsSelected = dm.DishID == dishID
                }).ToList()
            };
            return View(model);
        }

        //Get: /SU_Area/WeekMenu
        public ActionResult WeekMenu()
        {
            return PartialView("PartialWeekMenu");
        }
        public JsonResult EnableEditing(MenuForDayModel model)
        {
            //int ind;
            //Int32.TryParse(menuindex, out ind);
            //MenuForDayModel model = WeekModels.Find(w => w.WeekNumber == DB.CurrentWeek()).MFD_models.ElementAtOrDefault(ind);
            model.Editing = true;
            return Json(model);
        }
    }
}