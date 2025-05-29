using DreamDayWeddingPlanner.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

public class GuestController : Controller
{
    private int GetWeddingIdByUser(int userId)
    {
        int weddingId = 0;
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT wedding_id FROM weddings WHERE user_id = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            var result = cmd.ExecuteScalar();
            if (result != null)
                weddingId = Convert.ToInt32(result);
        }

        return weddingId;
    }

    public ActionResult Index()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        int weddingId = GetWeddingIdByUser(userId);
        List<Guest> guests = new List<Guest>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM guests WHERE wedding_id = @weddingId", conn);
            cmd.Parameters.AddWithValue("@weddingId", weddingId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    guests.Add(new Guest
                    {
                        GuestId = Convert.ToInt32(reader["guest_id"]),
                        WeddingId = weddingId,
                        GuestName = reader["guest_name"].ToString(),
                        Email = reader["email"].ToString(),
                        RsvpStatus = reader["rsvp_status"].ToString(),
                        MealPreference = reader["meal_preference"].ToString(),
                        SeatNumber = reader["seat_number"].ToString()
                    });
                }
            }
        }

        return View(guests);
    }

    public ActionResult Add() => View();

    [HttpPost]
    public ActionResult Add(Guest model)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        int weddingId = GetWeddingIdByUser(userId);
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            var cmd = new MySqlCommand(@"INSERT INTO guests (wedding_id, guest_name, email, rsvp_status, meal_preference, seat_number)
                                         VALUES (@weddingId, @name, @email, @rsvp, @meal, @seat)", conn);
            cmd.Parameters.AddWithValue("@weddingId", weddingId);
            cmd.Parameters.AddWithValue("@name", model.GuestName);
            cmd.Parameters.AddWithValue("@email", model.Email);
            cmd.Parameters.AddWithValue("@rsvp", model.RsvpStatus ?? "Pending");
            cmd.Parameters.AddWithValue("@meal", model.MealPreference);
            cmd.Parameters.AddWithValue("@seat", model.SeatNumber);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Delete(int id)
    {
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM guests WHERE guest_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Index");
    }
}
