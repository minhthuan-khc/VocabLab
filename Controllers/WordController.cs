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

        // Đã mở lại Authorize để đảm bảo userId không bị null gây sập web
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
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            if (word.DifficultyLevel == 0)
            {
                word.DifficultyLevel = 2;
            }

            ModelState.Remove("Progresses");

            if (string.IsNullOrEmpty(word.Example))
            {
                word.Example = "";
                ModelState.Remove("Example");
            }

            // Ghi log kiểm tra dữ liệu đầu vào bao gồm cả Pronunciation
            Console.WriteLine($"[Create POST] Received: Term={word.Term}, Category={word.Category}, Level={word.DifficultyLevel}");
            Console.WriteLine($"[Create POST] IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"[Create POST] Saving to database...");

                await _wordService.CreateAsync(word);

                TempData["SuccessMessage"] =
                    "Đã lưu từ vựng thành công.";

                return RedirectToAction("Index", "Word");
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors);

            foreach (var error in errors)
            {
                Console.WriteLine($"[Create POST] Error: {error.ErrorMessage}");
            }

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
                    TempData["ErrorMessage"] =
                        "Không có dữ liệu hợp lệ được gửi lên.";

                    return RedirectToAction("Create");
                }

                int successCount = 0;

                foreach (var word in words)
                {
                    if (string.IsNullOrWhiteSpace(word.Term))
                        continue;

                    if (word.DifficultyLevel == 0)
                        word.DifficultyLevel = 2;

                    if (string.IsNullOrEmpty(word.Example))
                        word.Example = "";

                    if (string.IsNullOrEmpty(word.Definition))
                        word.Definition = "Chưa có định nghĩa";

                    if (string.IsNullOrEmpty(word.Category))
                        word.Category = "Chưa phân loại";

                    ModelState.Clear();

                    await _wordService.CreateAsync(word);

                    successCount++;
                }

                TempData["SuccessMessage"] =
                    $"Đã nhập nhanh thành công {successCount} từ vựng mới.";

                return RedirectToAction("Index", "Word");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI BULK CREATE]: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[LỖI CHI TIẾT DB]: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] =
                    "Có lỗi xảy ra khi lưu vào Database. Vui lòng kiểm tra màn hình Console.";

                return RedirectToAction("Create");
            }
        }

        // =======================================================
        // ĐÃ CẬP NHẬT: THÊM TRƯỜNG PRONUNCIATION CHO API AUTO-FETCH
        // =======================================================
        [HttpGet("api/word/auto-fetch")]
        public IActionResult AutoFetch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new
                {
                    error = "Vui lòng cung cấp từ cần tra cứu."
                });
            }

            // Bổ sung dữ liệu giả lập cho pronunciation khi người dùng gõ từ để hệ thống tự điền
            var data = new
            {
                term = term,
                definition = $"Định nghĩa giả định cho '{term}'.",
                pronunciation = "/.../", // Chèn dữ liệu phiên âm mặc định/giả định vào đây
                category = "General",
                example = $"Ví dụ: {term} được sử dụng trong một câu."
            };

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var word = await _wordService.GetByIdAsync(id);

            if (word == null)
                return NotFound();

            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Word word)
        {
            if (id != word.Id)
                return BadRequest();

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

            if (word == null)
                return NotFound();

            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? confirmed)
        {
            if (confirmed == "true")
            {
                await _wordService.DeleteAsync(id);

                return RedirectToAction(nameof(Index));
            }

            var word = await _wordService.GetByIdAsync(id);

            if (word == null)
                return NotFound();

            return View(word);
        }
    }
}