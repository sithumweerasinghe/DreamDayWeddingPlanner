using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System;
using DreamDayWeddingPlanner.Models;


public class TimelineController : Controller
{
    private int GetWeddingIdByUser(int userId)
    {
        int weddingId = 0;
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT wedding_id FROM weddings WHERE user_id = @userId";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                weddingId = Convert.ToInt32(result);
            }
        }

        return weddingId;
    }

    public ActionResult Index()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        int userId = int.Parse(Session["UserId"].ToString());
        int weddingId = GetWeddingIdByUser(userId);

        List<TimelineEvent> events = new List<TimelineEvent>();
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "SELECT * FROM timeline_events WHERE wedding_id = @weddingId ORDER BY start_time";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@weddingId", weddingId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    events.Add(new TimelineEvent
                    {
                        EventId = Convert.ToInt32(reader["event_id"]),
                        WeddingId = weddingId,
                        EventTitle = reader["event_title"].ToString(),
                        EventDescription = reader["event_description"].ToString(),
                        StartTime = Convert.ToDateTime(reader["start_time"]),
                        EndTime = reader["end_time"] as DateTime?
                    });
                }
            }
        }

        return View(events);
    }

    // GET: Timeline/Add
    public ActionResult Add()
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public ActionResult Delete(int id)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();
            string sql = "DELETE FROM timeline_events WHERE event_id = @eventId";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@eventId", id);
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Index");
    }


    // POST: Timeline/Add
    [HttpPost]
    public ActionResult Add(TimelineEvent model)
    {
        if (Session["UserId"] == null || (string)Session["Role"] != "Couple")
            return RedirectToAction("Login", "Account");

        if (ModelState.IsValid)
        {
            int userId = int.Parse(Session["UserId"].ToString());
            int weddingId = GetWeddingIdByUser(userId);
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO timeline_events 
                            (wedding_id, event_title, event_description, start_time, end_time) 
                            VALUES (@weddingId, @title, @description, @startTime, @endTime)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@weddingId", weddingId);
                cmd.Parameters.AddWithValue("@title", model.EventTitle);
                cmd.Parameters.AddWithValue("@description", model.EventDescription);
                cmd.Parameters.AddWithValue("@startTime", model.StartTime);
                cmd.Parameters.AddWithValue("@endTime", (object)model.EndTime ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        return View(model);
    }
}
