using System.Web.Mvc;

namespace ACSDining.Web.Areas.EmployeeArea.Controllers
{
    public class OrderController : Controller
    {
        //private IUnitOfWork unitOfWork;
        //private IRepository<OrderMenu> orderRepository;
        //private int CurrentWeekNumber;
        //private string userID;

        //public OrderController(IUnitOfWork unit)
        //{
        //    this.unitOfWork = unit;
        //    orderRepository = this.unitOfWork.Repository<OrderMenu>();
        //    //userID = User.Identity.GetUserId();

        //    // Gets the Calendar instance associated with a CultureInfo.
        //    CultureInfo myCI = new CultureInfo("uk-UA");
        //    Calendar myCal = myCI.Calendar;

        //    // Gets the DTFI properties required by GetWeekOfYear.
        //    CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
        //    System.DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
        //    DateTime LastDay = new System.DateTime(DateTime.Now.Year, 12, 31);
        //    CurrentWeekNumber = myCal.GetWeekOfYear(LastDay, myCWR, myFirstDOW);
        //}

        //public string UserID { get { return User.Identity.GetUserId(); } }
        //// GET current week order
        //public async Task<OrderMenu> GetOrder()
        //{
        //    return await orderRepository.Find(o => o.User.Id == UserID && o.WeekNumber == CurrentWeekNumber).FirstOrDefaultAsync();
        //}
        //private ApplicationDbContext DB = new ApplicationDbContext();

        //// GET: /EmployeeArea/Order/
        //public async Task<ActionResult> Index()
        //{
        //    return View(await GetOrder());
        //}

        //// GET: /EmployeeArea/Order/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    OrderMenu ordermenu = await DB.OrderMenu.FindAsync(id);
        //    if (ordermenu == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ordermenu);
        //}

        //// GET: /EmployeeArea/Order/Create
        //public ActionResult Create()
        //{
        //    ViewBag.CurrentWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID");
        //    ViewBag.User.Id = new SelectList(DB.Users, "ClientId", "FirstName");
        //    ViewBag.NextWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID");
        //    return View();
        //}

        //// POST: /EmployeeArea/Order/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,UserID,CurrentWeekID,NextWeekID,CurrentWeekIsPaid")] OrderMenu ordermenu)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        DB.OrderMenu.Add(ordermenu);
        //        await DB.SaveChangesAsync();
        //        return RedirectToAction("WeekMenu");
        //    }

        //    ViewBag.CurrentWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.CurrentWeek.ID);
        //    ViewBag.User.Id = new SelectList(DB.Users, "ClientId", "FirstName", ordermenu.User.Id);
        //    ViewBag.NextWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.NextWeek.ID);
        //    return View(ordermenu);
        //}

        //// GET: /EmployeeArea/Order/Edit/5
        //public async Task<ActionResult> Edit(int id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    OrderMenu ordermenu = await DB.OrderMenu.FindAsync(id);
        //    if (ordermenu == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CurrentWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.CurrentWeek.ID);
        //    ViewBag.User.Id = new SelectList(DB.Users, "ClientId", "FirstName", ordermenu.User.Id);
        //    ViewBag.NextWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.NextWeek.ID);
        //    return View(ordermenu);
        //}

        //// POST: /EmployeeArea/Order/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,UserID,CurrentWeekID,NextWeekID,CurrentWeekIsPaid")] OrderMenu ordermenu)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        DB.Entry(ordermenu).State = EntityState.Modified;
        //        await DB.SaveChangesAsync();
        //        return RedirectToAction("WeekMenu");
        //    }
        //    ViewBag.CurrentWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.CurrentWeek.ID);
        //    ViewBag.User.Id = new SelectList(DB.Users, "ClientId", "FirstName", ordermenu.User.Id);
        //    ViewBag.NextWeek.ID = new SelectList(DB.MenuForWeek, "ID", "ID", ordermenu.NextWeek.ID);
        //    return View(ordermenu);
        //}

        //// GET: /EmployeeArea/Order/Delete/5
        //public async Task<ActionResult> Delete(int id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    OrderMenu ordermenu = await DB.OrderMenu.FindAsync(id);
        //    if (ordermenu == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ordermenu);
        //}

        //// POST: /EmployeeArea/Order/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    OrderMenu ordermenu = await DB.OrderMenu.FindAsync(id);
        //    DB.OrderMenu.Remove(ordermenu);
        //    await DB.SaveChangesAsync();
        //    return RedirectToAction("WeekMenu");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        DB.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
