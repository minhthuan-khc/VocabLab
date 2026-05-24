using Microsoft.AspNetCore.Mvc;
using VocabLab.Helpers;
using VocabLab.Models;
using VocabLab.Services;

namespace VocabLab.Controllers
{
    public class WordController : Controller
    {
        private readonly IWordService _wordService;

        public WordController(IWordService wordService)
        {
            _wordService = wordService;
        }

        public async Task<IActionResult> Index(string? category = null)
        {
            var words = category == null
                ? await _wordService.GetAllAsync()
                : await _wordService.GetByCategoryAsync(category);

            return View(words);
        }

        public async Task<IActionResult> Quiz()
        {
            var userId = UserHelper.GetUserId(HttpContext);

            var questions =
                await _wordService.GenerateQuizAsync(userId);

            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(
            Dictionary<int, bool> answers)
        {
            var userId = UserHelper.GetUserId(HttpContext);

            foreach (var answer in answers)
            {
                await _wordService.SaveProgressAsync(
                    userId,
                    answer.Key,
                    answer.Value);
            }

            TempData["Score"] =
                answers.Count(a => a.Value);

            return RedirectToAction("Index");
        }
    }
}