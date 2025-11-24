using ChatByWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class StatsController : Controller
{
    private readonly HttpClient _client;

    public StatsController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
        _client.BaseAddress = new Uri("http://localhost:8081/");
    }

    private void SetAuth()
    {
        var token = HttpContext.Session.GetString("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IActionResult> Index()
    {
        SetAuth();

        // 7 ngày gần nhất
        var dates = Enumerable.Range(0, 7)
                              .Select(i => DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"))
                              .Reverse() // để ngày cũ → ngày mới
                              .ToList();

        // USER DAILY cho 7 ngày
        var userDailyList = new List<UserDailyStats>();
        foreach (var day in dates)
        {
            var userDailyJson = await _client.GetStringAsync($"/api/stats/user/daily?day={day}");
            var list = JsonConvert.DeserializeObject<List<UserDailyStats>>(userDailyJson);
            if (list?.Any() == true)
                userDailyList.Add(list.First());
        }

        // DAILY stats cho 7 ngày
        var dailyList = new List<DailyStats>();
        foreach (var day in dates)
        {
            var dailyJson = await _client.GetStringAsync($"/api/stats/daily?day={day}");
            var list = JsonConvert.DeserializeObject<List<DailyStats>>(dailyJson);
            if (list?.Any() == true)
                dailyList.Add(list.First());
        }

        // SUMMARY → OBJECT
        var summaryJson = await _client.GetStringAsync("/api/stats/summary");
        var summary = JsonConvert.DeserializeObject<SummaryStats>(summaryJson);

        ViewBag.Dates = dates;
        ViewBag.UserDailyList = userDailyList;
        ViewBag.DailyList = dailyList;
        ViewBag.Summary = summary;

        return View();
    }
}
