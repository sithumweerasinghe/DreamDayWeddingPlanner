using System.Collections.Generic;
using System.Web.Mvc;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;
using System;

public class AdminController : Controller
{
    public ActionResult Dashboard()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        ViewBag.Username = Session["Username"];

        // Get all users with role "Planner"
        List<User> planners = new List<User>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM users WHERE role = 'Planner'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                planners.Add(new User
                {
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Username = reader["username"].ToString(),
                    Email = reader["email"].ToString()
                });
            }
        }

        return View(planners);
    }

    // GET: Admin/AddPlanner
    public ActionResult AddPlanner()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Admin/AddPlanner
    [HttpPost]
    public ActionResult AddPlanner(User user, string Password)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO users (username, email, password, role) VALUES (@username, @email, @password, 'Planner')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Dashboard");
        }

        return View(user);
    }

    // GET: Admin/DeletePlanner/{id}
    public ActionResult DeletePlanner(int? id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        if (id == null)
            return new HttpStatusCodeResult(400, "Invalid request: ID is required");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "DELETE FROM users WHERE user_id = @id AND role = 'Planner'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Dashboard");
    }
}
