using System;
using System.Web.Mvc;
using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

public class CoupleController : Controller
{
    public ActionResult Dashboard()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        Wedding wedding = null;
        decimal totalAmount = 0;
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            // Get wedding details
            string weddingSql = "SELECT * FROM weddings WHERE user_id = @userId";
            using (MySqlCommand weddingCmd = new MySqlCommand(weddingSql, conn))
            {
                weddingCmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = weddingCmd.ExecuteReader())
                {
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
            }

            // Get total amount from cart stored in DB
            string totalSql = @"
            SELECT SUM(v.price) AS total_amount
            FROM carts c
            JOIN cart_items ci ON c.cart_id = ci.cart_id
            JOIN vendors v ON ci.vendor_id = v.vendor_id
            WHERE c.user_id = @userId";

            using (MySqlCommand totalCmd = new MySqlCommand(totalSql, conn))
            {
                totalCmd.Parameters.AddWithValue("@userId", userId);
                object result = totalCmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    totalAmount = Convert.ToDecimal(result);
                }
            }
        }

        ViewBag.TotalAmount = totalAmount;
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

    // Add vendor to cart
    public ActionResult AddToCart(int vendorId)
    {
        if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        int cartId = 0;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            // Check if cart exists for this user
            string getCartSql = "SELECT cart_id FROM carts WHERE user_id = @userId ORDER BY created_at DESC LIMIT 1";
            MySqlCommand getCartCmd = new MySqlCommand(getCartSql, conn);
            getCartCmd.Parameters.AddWithValue("@userId", userId);
            object result = getCartCmd.ExecuteScalar();

            if (result != null)
            {
                cartId = Convert.ToInt32(result);
            }
            else
            {
                // Create a new cart
                string createCartSql = "INSERT INTO carts (user_id) VALUES (@userId)";
                MySqlCommand createCartCmd = new MySqlCommand(createCartSql, conn);
                createCartCmd.Parameters.AddWithValue("@userId", userId);
                createCartCmd.ExecuteNonQuery();
                cartId = (int)createCartCmd.LastInsertedId;
            }

            // Check if vendor already in cart
            string checkSql = "SELECT COUNT(*) FROM cart_items WHERE cart_id = @cartId AND vendor_id = @vendorId";
            MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@cartId", cartId);
            checkCmd.Parameters.AddWithValue("@vendorId", vendorId);
            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists == 0)
            {
                // Add vendor to cart
                string addItemSql = "INSERT INTO cart_items (cart_id, vendor_id) VALUES (@cartId, @vendorId)";
                MySqlCommand addItemCmd = new MySqlCommand(addItemSql, conn);
                addItemCmd.Parameters.AddWithValue("@cartId", cartId);
                addItemCmd.Parameters.AddWithValue("@vendorId", vendorId);
                addItemCmd.ExecuteNonQuery();
            }
        }

        return RedirectToAction("BrowseVendors");
    }


    // View Cart
    public ActionResult Cart()
    {
        if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        List<CartItem> cart = new List<CartItem>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = @"
            SELECT ci.cart_item_id, v.vendor_id, v.name, v.category, v.description, v.price, v.rating
            FROM carts c
            JOIN cart_items ci ON c.cart_id = ci.cart_id
            JOIN vendors v ON ci.vendor_id = v.vendor_id
            WHERE c.user_id = @userId
            ORDER BY ci.cart_item_id DESC";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cart.Add(new CartItem
                {
                    Vendor = new Vendor
                    {
                        VendorId = Convert.ToInt32(reader["vendor_id"]),
                        Name = reader["name"].ToString(),
                        Category = reader["category"].ToString(),
                        Description = reader["description"].ToString(),
                        Price = Convert.ToDecimal(reader["price"]),
                        Rating = Convert.ToDecimal(reader["rating"])
                    }
                });
            }
        }

        return View(cart);
    }


    // Remove item from cart
    public ActionResult RemoveFromCart(int vendorId)
    {
        if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            string getCartSql = "SELECT cart_id FROM carts WHERE user_id = @userId ORDER BY created_at DESC LIMIT 1";
            MySqlCommand getCartCmd = new MySqlCommand(getCartSql, conn);
            getCartCmd.Parameters.AddWithValue("@userId", userId);
            object result = getCartCmd.ExecuteScalar();

            if (result != null)
            {
                int cartId = Convert.ToInt32(result);
                string deleteSql = "DELETE FROM cart_items WHERE cart_id = @cartId AND vendor_id = @vendorId";
                MySqlCommand deleteCmd = new MySqlCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@cartId", cartId);
                deleteCmd.Parameters.AddWithValue("@vendorId", vendorId);
                deleteCmd.ExecuteNonQuery();
            }
        }

        return RedirectToAction("Cart");
    }







}
