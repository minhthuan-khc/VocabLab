using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VocabLab.Models;
using VocabLab.Services;

namespace VocabLab.Controllers
{
    public class WordController : Controller
    {
        private readonly IWordService _wordService;

        public WordController(IWordService wordService) =>
            _wordService = wordService;

        public async Task<IActionResult> Index(string? category = null)
        {
            var words = category == null
                ? await _wordService.GetAllAsync()
                : await _wordService.GetByCategoryAsync(category);
            return View(words);
        }

        [Authorize]
        public async Task<IActionResult> Quiz()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var questions = await _wordService.GenerateQuizAsync(userId!);
            return View(questions);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> SubmitQuiz(Dictionary<int, bool> answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var answer in answers)
                await _wordService.SaveProgressAsync(userId!, answer.Key, answer.Value);

            TempData["Score"] = answers.Count(a => a.Value);
            return RedirectToAction("Index");
        }
    }
}