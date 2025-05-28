using System;
using System.Web.Mvc;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Collections.Generic;

public class CoupleController : Controller
{
    public ActionResult Dashboard()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        Wedding wedding = null;
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM weddings WHERE user_id = @userId";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                wedding = new Wedding
                {
                    WeddingId = Convert.ToInt32(reader["wedding_id"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Title = reader["title"].ToString(),
                    WeddingDate = Convert.ToDateTime(reader["wedding_date"]),
                    Location = reader["location"].ToString()
                };
            }
        }

        return View(wedding);

    }

    public ActionResult CreateWedding()
    {
        return View();
    }

    [HttpPost]
    public ActionResult CreateWedding(Wedding wedding)
    {
        if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "INSERT INTO weddings (user_id, title, wedding_date, location) VALUES (@userId, @title, @weddingDate, @location)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@title", wedding.Title);
            cmd.Parameters.AddWithValue("@weddingDate", wedding.WeddingDate);
            cmd.Parameters.AddWithValue("@location", wedding.Location);
            cmd.ExecuteNonQuery();
        }

        ViewBag.Message = "Wedding created successfully!";
        return RedirectToAction("Dashboard");
    }

    public ActionResult BrowseVendors()
    {
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
                    Category = reader["category"].ToString(),
                    Description = reader["description"].ToString(),
                    Price = Convert.ToDecimal(reader["price"]),
                    Rating = Convert.ToDecimal(reader["rating"])
                });
            }
        }

        return View(vendors);
    }

    public ActionResult MyWeddings()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        List<Wedding> weddings = new List<Wedding>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM weddings WHERE user_id = @userId";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                weddings.Add(new Wedding
                {
                    WeddingId = Convert.ToInt32(reader["wedding_id"]),
                    Title = reader["title"].ToString(),
                    WeddingDate = Convert.ToDateTime(reader["wedding_date"]),
                    Location = reader["location"].ToString()
                });
            }
        }

        return View(weddings);
    }

    // View checklist for a wedding
    public ActionResult Checklist(int weddingId)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        List<Checklist> checklistItems = new List<Checklist>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM checklists WHERE wedding_id = @weddingId";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@weddingId", weddingId);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                checklistItems.Add(new Checklist
                {
                    ChecklistId = Convert.ToInt32(reader["checklist_id"]),
                    WeddingId = Convert.ToInt32(reader["wedding_id"]),
                    Task = reader["task"].ToString(),
                    IsCompleted = Convert.ToBoolean(reader["is_completed"])
                });
            }
        }

        ViewBag.WeddingId = weddingId;
        return View(checklistItems);
    }

    // Add new checklist task (POST)
    [HttpPost]
    public ActionResult AddChecklistTask(int weddingId, string task)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        if (!string.IsNullOrEmpty(task))
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO checklists (wedding_id, task) VALUES (@weddingId, @task)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@weddingId", weddingId);
                cmd.Parameters.AddWithValue("@task", task);
                cmd.ExecuteNonQuery();
            }
        }
        return RedirectToAction("Checklist", new { weddingId = weddingId });
    }

    // Mark checklist task completed/uncompleted (toggle)
    public ActionResult ToggleChecklistStatus(int checklistId, int weddingId)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            // Get current status
            string selectSql = "SELECT is_completed FROM checklists WHERE checklist_id = @id";
            MySqlCommand selectCmd = new MySqlCommand(selectSql, conn);
            selectCmd.Parameters.AddWithValue("@id", checklistId);
            bool currentStatus = Convert.ToBoolean(selectCmd.ExecuteScalar());

            // Toggle status
            string updateSql = "UPDATE checklists SET is_completed = @status WHERE checklist_id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@status", !currentStatus);
            updateCmd.Parameters.AddWithValue("@id", checklistId);
            updateCmd.ExecuteNonQuery();
        }

        return RedirectToAction("Checklist", new { weddingId = weddingId });
    }

    // Delete checklist task
    public ActionResult DeleteChecklistTask(int checklistId, int weddingId)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "DELETE FROM checklists WHERE checklist_id = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", checklistId);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Checklist", new { weddingId = weddingId });
    }


}
