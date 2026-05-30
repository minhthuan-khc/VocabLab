// WordController.cs

using Microsoft.AspNetCore.Authorization;
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

        // =========================================================
        // FIX: Xóa method Quiz() cũ (server-side render), 
        // gộp vào Quiz() mới bên dưới — chỉ giữ 1 method duy nhất
        // =========================================================
        public async Task<IActionResult> Quiz()
        {
            var words = await _wordService.GetAllAsync();
            ViewBag.TotalWords = words?.Count() ?? 0;
            ViewBag.History = new List<dynamic>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(Dictionary<int, bool> answers)
        {
            var userId = UserHelper.GetUserId(HttpContext);

            foreach (var answer in answers)
            {
                await _wordService.SaveProgressAsync(userId, answer.Key, answer.Value);
            }

            TempData["Score"] = answers.Count(a => a.Value);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Word word)
        {
            if (word == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            if (word.DifficultyLevel == 0)
                word.DifficultyLevel = 2;

            ModelState.Remove("Progresses");

            if (string.IsNullOrEmpty(word.Example))
            {
                word.Example = "";
                ModelState.Remove("Example");
            }

            Console.WriteLine($"[Create POST] Received: Term={word.Term}, Category={word.Category}, Level={word.DifficultyLevel}");
            Console.WriteLine($"[Create POST] IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"[Create POST] Saving to database...");
                await _wordService.CreateAsync(word);
                TempData["SuccessMessage"] = "Đã lưu từ vựng thành công.";
                return RedirectToAction("Index", "Word");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
                Console.WriteLine($"[Create POST] Error: {error.ErrorMessage}");

            Console.WriteLine($"[Create POST] Returning view (validation failed)");
            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(List<Word> words)
        {
            try
            {
                if (words == null || words.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu hợp lệ được gửi lên.";
                    return RedirectToAction("Create");
                }

                int successCount = 0;

                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word.Term)) continue;

                    if (word.DifficultyLevel == 0) word.DifficultyLevel = 2;
                    if (string.IsNullOrEmpty(word.Example)) word.Example = "";
                    if (string.IsNullOrEmpty(word.Definition)) word.Definition = "Chưa có định nghĩa";
                    if (string.IsNullOrEmpty(word.Category)) word.Category = "Chưa phân loại";

                    ModelState.Clear();
                    await _wordService.CreateAsync(word);
                    successCount++;
                }

                TempData["SuccessMessage"] = $"Đã nhập nhanh thành công {successCount} từ vựng mới.";
                return RedirectToAction("Index", "Word");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI BULK CREATE]: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[LỖI CHI TIẾT DB]: {ex.InnerException.Message}");

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu vào Database. Vui lòng kiểm tra màn hình Console.";
                return RedirectToAction("Create");
            }
        }

        [HttpGet("api/word/auto-fetch")]
        public IActionResult AutoFetch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { error = "Vui lòng cung cấp từ cần tra cứu." });

            var data = new
            {
                term = term,
                definition = $"Định nghĩa giả định cho '{term}'.",
                pronunciation = "/.../",
                category = "General",
                example = $"Ví dụ: {term} được sử dụng trong một câu."
            };

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var word = await _wordService.GetByIdAsync(id);
            if (word == null) return NotFound();
            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Word word)
        {
            if (id != word.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                await _wordService.UpdateAsync(word);
                return RedirectToAction(nameof(Index));
            }

            return View(word);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var word = await _wordService.GetByIdAsync(id);
            if (word == null) return NotFound();
            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? confirmed)
        {
            if (confirmed == "true")
            {
                await _wordService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }

            var word = await _wordService.GetByIdAsync(id);
            if (word == null) return NotFound();
            return View(word);
        }

        // =========================================================
        // GAME ACTIONS
        // =========================================================

        public IActionResult Flashcard() => View();

        public IActionResult GameQuiz() => View();

        public IActionResult Match() => View();
        public IActionResult Typing() => View();
        public IActionResult Listening() => View();

        [HttpGet]
        public async Task<IActionResult> GetWordsForGame(string? category, string? status, string? sort, int count = 20)
        {
            var words = await _wordService.GetAllAsync();
            if (words == null) return Json(new List<object>());

            if (!string.IsNullOrEmpty(category) && category != "Tất cả bộ từ")
                words = words.Where(w => w.Category != null && w.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (sort == "Ngẫu nhiên")
                words = words.OrderBy(w => Guid.NewGuid()).ToList();
            else if (sort == "A → Z")
                words = words.OrderBy(w => w.Term).ToList();
            else if (sort == "Mới nhất")
                words = words.OrderByDescending(w => w.Id).ToList();

            if (count > 0)
                words = words.Take(count).ToList();

            var result = words.Select(w => new
            {
                id = w.Id,
                english = w.Term,
                vietnamese = w.Definition,
                pronunciation = w.Pronunciation ?? "/.../",
                example = w.Example,
                audioUrl = $"https://dict.youdao.com/dictvoice?audio={Uri.EscapeDataString(w.Term)}&type=2"
            });

            return Json(result);
        }

        // FIX: Đổi thành [HttpGet] vì JS gọi bằng fetch GET, không phải POST
        [HttpGet]
        public async Task<IActionResult> SaveGameHistory(string gameName, int score)
        {
            var userId = UserHelper.GetUserId(HttpContext);
            return Json(new { success = true, message = "Đã ghi nhận kết quả" });
        }
    }
}