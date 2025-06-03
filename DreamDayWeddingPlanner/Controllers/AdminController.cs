// Controllers/AdminController.cs
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;

public class AdminController : Controller
{
    public ActionResult Dashboard()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        ViewBag.Username = Session["Username"];

        List<User> planners = new List<User>();
        List<User> couples = new List<User>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM users WHERE role = 'Planner' OR role = 'Couple'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var user = new User
                {
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Username = reader["username"].ToString(),
                    Email = reader["email"].ToString(),
                    Role = reader["role"].ToString()
                };
                if (user.Role == "Planner")
                    planners.Add(user);
                else if (user.Role == "Couple")
                    couples.Add(user);
            }
        }

        ViewBag.Couples = couples;
        return View(planners);
    }

    public ActionResult AddPlanner()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

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

    public ActionResult SystemUsage()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        // Future implementation: track and display login logs, active sessions, etc.
        return View();
    }

    public ActionResult VendorManagement()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        List<Vendor> vendors = new List<Vendor>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM vendors";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                vendors.Add(new Vendor
                {
                    VendorId = Convert.ToInt32(reader["vendor_id"]),
                    Name = reader["name"].ToString(),
                    Category = reader["Category"].ToString(),
                    Rating = Convert.ToDecimal(reader["Rating"])
                });
            }
        }

        return View(vendors);
    }

    public ActionResult AddVendor()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public ActionResult AddVendor(Vendor vendor)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO vendors (name, email, phone) VALUES (@name, @email, @phone)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", vendor.Name);
                cmd.Parameters.AddWithValue("@Category", vendor.Category);
                cmd.Parameters.AddWithValue("@Rating", vendor.Rating);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("VendorManagement");
        }

        return View(vendor);
    }

    public ActionResult EditVendor(int id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        Vendor vendor = new Vendor();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM vendors WHERE vendor_id = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                vendor.VendorId = Convert.ToInt32(reader["vendor_id"]);
                vendor.Name = reader["name"].ToString();
                vendor.Category = reader["Category"].ToString();
                vendor.Rating = Convert.ToDecimal(reader["Rating"]);
            }
        }

        return View(vendor);
    }

    [HttpPost]
    public ActionResult EditVendor(Vendor vendor)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "UPDATE vendors SET name = @name, email = @email, phone = @phone WHERE vendor_id = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", vendor.Name);
                cmd.Parameters.AddWithValue("@Category", vendor.Category);
                cmd.Parameters.AddWithValue("@Rating", vendor.Rating);
                cmd.Parameters.AddWithValue("@id", vendor.VendorId);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("VendorManagement");
        }

        return View(vendor);
    }

    public ActionResult DeleteVendor(int id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "DELETE FROM vendors WHERE vendor_id = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("VendorManagement");
    }

    public ActionResult GenerateReports()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        // Future implementation: generate user, event, and system reports
        return View();
    }


    // GET: Admin/UserReport
    public ActionResult UserReport()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

    // GET: Admin/PlannerPerformanceReport
    public ActionResult PlannerPerformanceReport()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

    // GET: Admin/VendorReport
    public ActionResult VendorReport()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Admin")
            return RedirectToAction("Login", "Account");

        return View();
    }

}
