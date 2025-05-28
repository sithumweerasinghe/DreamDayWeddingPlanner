using System.Web.Mvc;
using DreamDayWeddingPlanner.DataAccess;

namespace DreamDayWeddingPlanner.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            var users = Db.GetUsers();
            return View(users);
        }
    }
}
