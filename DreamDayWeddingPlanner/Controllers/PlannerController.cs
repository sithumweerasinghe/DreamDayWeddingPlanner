using System.Collections.Generic;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Web.Mvc;
using System;

public class PlannerController : Controller
{
    public ActionResult Dashboard()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        ViewBag.Username = Session["Username"];

        // For now, show list of vendors (all vendors)
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

    // GET: Planner/AddVendor
    public ActionResult AddVendor()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        return View();
    }

    // POST: Planner/AddVendor
    [HttpPost]
    public ActionResult AddVendor(Vendor vendor)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO vendors (name, category, description, price, rating) VALUES (@name, @category, @description, @price, @rating)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", vendor.Name);
                cmd.Parameters.AddWithValue("@category", vendor.Category);
                cmd.Parameters.AddWithValue("@description", vendor.Description);
                cmd.Parameters.AddWithValue("@price", vendor.Price);
                cmd.Parameters.AddWithValue("@rating", vendor.Rating);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Dashboard");
        }
        return View(vendor);
    }

    // GET: Planner/EditVendor/{id}
    public ActionResult EditVendor(int id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        Vendor vendor = null;
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
                vendor = new Vendor
                {
                    VendorId = Convert.ToInt32(reader["vendor_id"]),
                    Name = reader["name"].ToString(),
                    Category = reader["category"].ToString(),
                    Description = reader["description"].ToString(),
                    Price = Convert.ToDecimal(reader["price"]),
                    Rating = Convert.ToDecimal(reader["rating"])
                };
            }
        }
        if (vendor == null) return HttpNotFound();
        return View(vendor);
    }

    // POST: Planner/EditVendor/{id}
    [HttpPost]
    public ActionResult EditVendor(Vendor vendor)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"UPDATE vendors SET name=@name, category=@category, description=@description, price=@price, rating=@rating 
                           WHERE vendor_id=@vendorId";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", vendor.Name);
                cmd.Parameters.AddWithValue("@category", vendor.Category);
                cmd.Parameters.AddWithValue("@description", vendor.Description);
                cmd.Parameters.AddWithValue("@price", vendor.Price);
                cmd.Parameters.AddWithValue("@rating", vendor.Rating);
                cmd.Parameters.AddWithValue("@vendorId", vendor.VendorId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Dashboard");
        }
        return View(vendor);
    }

    // GET: Planner/DeleteVendor/{id}
    public ActionResult DeleteVendor(int id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
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
        return RedirectToAction("Dashboard");
    }

    public ActionResult Weddings()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        List<Wedding> weddings = new List<Wedding>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = @"SELECT w.wedding_id, w.title, w.wedding_date, w.location, u.username 
                       FROM weddings w 
                       JOIN users u ON w.user_id = u.user_id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                weddings.Add(new Wedding
                {
                    WeddingId = Convert.ToInt32(reader["wedding_id"]),
                    Title = reader["title"].ToString(),
                    WeddingDate = Convert.ToDateTime(reader["wedding_date"]),
                    Location = reader["location"].ToString(),
                    CoupleUsername = reader["username"].ToString()
                });
            }
        }

        return View(weddings);
    }

    // GET: Show assign vendors form
    public ActionResult AssignVendors(int weddingId)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        Wedding wedding = null;
        List<Vendor> vendors = new List<Vendor>();
        List<Vendor> assignedVendors = new List<Vendor>();

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            // Load wedding info
            string sqlWedding = "SELECT * FROM weddings WHERE wedding_id = @weddingId";
            MySqlCommand cmdWedding = new MySqlCommand(sqlWedding, conn);
            cmdWedding.Parameters.AddWithValue("@weddingId", weddingId);
            var readerWedding = cmdWedding.ExecuteReader();
            if (readerWedding.Read())
            {
                wedding = new Wedding
                {
                    WeddingId = Convert.ToInt32(readerWedding["wedding_id"]),
                    Title = readerWedding["title"].ToString()
                };
            }
            readerWedding.Close();

            // Load all vendors
            string sqlVendors = "SELECT * FROM vendors";
            MySqlCommand cmdVendors = new MySqlCommand(sqlVendors, conn);
            var readerVendors = cmdVendors.ExecuteReader();
            while (readerVendors.Read())
            {
                vendors.Add(new Vendor
                {
                    VendorId = Convert.ToInt32(readerVendors["vendor_id"]),
                    Name = readerVendors["name"].ToString(),
                    Category = readerVendors["category"].ToString()
                });
            }
            readerVendors.Close();

            // Load assigned vendors for this wedding
            string sqlAssigned = @"SELECT v.vendor_id, v.name, v.category 
                               FROM vendors v 
                               JOIN wedding_vendors wv ON v.vendor_id = wv.vendor_id
                               WHERE wv.wedding_id = @weddingId";
            MySqlCommand cmdAssigned = new MySqlCommand(sqlAssigned, conn);
            cmdAssigned.Parameters.AddWithValue("@weddingId", weddingId);
            var readerAssigned = cmdAssigned.ExecuteReader();
            while (readerAssigned.Read())
            {
                assignedVendors.Add(new Vendor
                {
                    VendorId = Convert.ToInt32(readerAssigned["vendor_id"]),
                    Name = readerAssigned["name"].ToString(),
                    Category = readerAssigned["category"].ToString()
                });
            }
        }

        ViewBag.AllVendors = vendors;
        ViewBag.AssignedVendors = assignedVendors;

        return View(wedding);
    }

    // POST: Assign vendor to wedding
    [HttpPost]
    public ActionResult AssignVendors(int weddingId, int vendorId)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Planner")
            return RedirectToAction("Login", "Account");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            string sqlCheck = "SELECT COUNT(*) FROM wedding_vendors WHERE wedding_id=@weddingId AND vendor_id=@vendorId";
            MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn);
            cmdCheck.Parameters.AddWithValue("@weddingId", weddingId);
            cmdCheck.Parameters.AddWithValue("@vendorId", vendorId);

            int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
            if (count == 0)
            {
                string sql = "INSERT INTO wedding_vendors (wedding_id, vendor_id) VALUES (@weddingId, @vendorId)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@weddingId", weddingId);
                cmd.Parameters.AddWithValue("@vendorId", vendorId);
                cmd.ExecuteNonQuery();
            }
        }

        return RedirectToAction("AssignVendors", new { weddingId = weddingId });
    }


}
