using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VocabLab.Data;
using Microsoft.EntityFrameworkCore;

namespace VocabLab.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalWords = await _context.Words.CountAsync();
            ViewBag.TotalWords = totalWords;
            ViewBag.DueWords = 0;
            ViewBag.LearnedWords = 0;
            ViewBag.Progress = 0;
            ViewBag.Streak = 1;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var learned = await _context.UserWordProgresses
                    .Where(p => p.UserId == userId && p.IsLearned)
                    .CountAsync();
                var due = await _context.UserWordProgresses
                    .Where(p => p.UserId == userId && !p.IsLearned)
                    .CountAsync();

                ViewBag.LearnedWords = learned;
                ViewBag.DueWords = due;
                ViewBag.Progress = totalWords > 0 ? (int)((double)learned / totalWords * 100) : 0;
            }

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}