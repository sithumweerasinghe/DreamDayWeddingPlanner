using System.Web.Mvc;

namespace DreamDayWeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                string role = (string)Session["Role"];
                // Redirect logged-in users directly to their dashboard
                if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (role == "Planner") return RedirectToAction("Dashboard", "Planner");
                if (role == "Couple") return RedirectToAction("Dashboard", "Couple");
            }
            return View();
        }
    }
}
