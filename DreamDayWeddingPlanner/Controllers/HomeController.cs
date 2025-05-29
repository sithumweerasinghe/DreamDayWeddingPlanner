using System.Web.Mvc;

namespace DreamDayWeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Landing()
        {
            Session.Clear();
            Session.Abandon();

            TempData["Message"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }


        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                string role = (string)Session["Role"];
                if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (role == "Planner") return RedirectToAction("Dashboard", "Planner");
                if (role == "Couple") return RedirectToAction("Dashboard", "Couple");
            }
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

    }
}
