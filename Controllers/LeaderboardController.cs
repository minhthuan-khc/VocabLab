using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabLab.Data;
using VocabLab.ViewModels;

namespace VocabLab.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly AppDbContext _db;

        public LeaderboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string tab = "score")
        {
            // Tab 1: Xếp hạng theo TotalScore
            var scoreRanking = await _db.Users
                .OrderByDescending(u => u.TotalScore)
                .Take(20)
                .Select(u => new LeaderboardEntryViewModel
                {
                    UserId   = u.Id,
                    FullName = u.FullName ?? u.UserName ?? "Ẩn danh",
                    Value    = u.TotalScore
                })
                .ToListAsync();

            // Tab 2: Xếp hạng theo Streak (UTC+7 Việt Nam)
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone).Date;

            var allProgress = await _db.UserWordProgresses
                .Select(p => new { p.UserId, p.LastReviewed })
                .ToListAsync();

            var streakByUser = allProgress
                .GroupBy(p => p.UserId)
                .Select(g =>
                {
                    var days = g
                        .Select(p => p.LastReviewed.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    int streak = 0;
                    if (days.Count > 0 && (days[0] == today || days[0] == today.AddDays(-1)))
                    {
                        var expected = days[0];
                        foreach (var day in days)
                        {
                            if (day == expected) { streak++; expected = expected.AddDays(-1); }
                            else break;
                        }
                    }
                    return new { UserId = g.Key, Streak = streak };
                })
                .OrderByDescending(x => x.Streak)
                .Take(20)
                .ToList();

            var userIds = streakByUser.Select(x => x.UserId).ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FullName ?? u.UserName ?? "Ẩn danh" })
                .ToListAsync();

            var streakRanking = streakByUser
                .Join(users, s => s.UserId, u => u.Id,
                    (s, u) => new LeaderboardEntryViewModel
                    {
                        UserId   = s.UserId,
                        FullName = u.Name,
                        Value    = s.Streak
                    })
                .ToList();

            // Heatmap: đếm số từ học theo ngày trong 90 ngày gần nhất (toàn bộ user)
            var since = today.AddDays(-89); // today already in VN timezone
            var heatmapRaw = allProgress
                .Where(p => p.LastReviewed.Date >= since)
                .GroupBy(p => p.LastReviewed.Date)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.Count()
                );

            var vm = new LeaderboardViewModel
            {
                ActiveTab     = tab,
                ScoreRanking  = scoreRanking,
                StreakRanking = streakRanking,
                HeatmapData   = heatmapRaw
            };

            return View(vm);
        }
    }
}
