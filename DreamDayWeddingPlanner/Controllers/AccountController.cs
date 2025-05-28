using System.Web.Mvc;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace DreamDayWeddingPlanner.Controllers
{
    public class AccountController : Controller
    {
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    string sql = "INSERT INTO users (username, email, password, role) VALUES (@username, @email, @password, @role)";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.ExecuteNonQuery();
                }

                ViewBag.Message = "Registration successful!";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT * FROM users WHERE email = @Email";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string hashedPassword = reader["password"].ToString();
                    string role = reader["role"].ToString();

                    if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                    {
                        Session["UserId"] = reader["user_id"].ToString();
                        Session["Username"] = reader["username"].ToString();
                        Session["Role"] = role;

                        // Redirect based on role
                        if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
                        if (role == "Planner") return RedirectToAction("Dashboard", "Planner");
                        return RedirectToAction("Dashboard", "Couple");
                    }
                }
            }

            ViewBag.Message = "Invalid email or password.";
            return View();
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
