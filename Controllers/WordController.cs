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

        // Đã mở lại Authorize để đảm bảo userId không bị null gây sập web
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Word word)
        {
            // 1. Kiểm tra null ngay từ đầu
            if (word == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // 2. Xử lý gán giá trị mặc định TRƯỚC KHI xử lý logic
            if (word.DifficultyLevel == 0)
            {
                word.DifficultyLevel = 2;
            }
            
            if (string.IsNullOrEmpty(word.Example))
            {
                word.Example = "";
                // Bỏ qua lỗi validation cho trường Example
                ModelState.Remove("Example"); 
                ModelState.Remove("Progresses");
            }

            Console.WriteLine($"[Create POST] Received: Term={word.Term}, Category={word.Category}, Level={word.DifficultyLevel}");
            Console.WriteLine($"[Create POST] IsValid: {ModelState.IsValid}");

            // 3. Kiểm tra hợp lệ và Lưu
            if (ModelState.IsValid)
            {
                Console.WriteLine($"[Create POST] Saving to database...");
                await _wordService.CreateAsync(word);
                
                TempData["SuccessMessage"] = "Đã lưu từ vựng thành công.";
                
                // Trở về trang danh sách Word sau khi lưu
                return RedirectToAction("Index", "Word"); 
            }

            // 4. In lỗi nếu validation thất bại
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine($"[Create POST] Error: {error.ErrorMessage}");
            }

            Console.WriteLine($"[Create POST] Returning view (validation failed)");
            return View(word);
        }

        // ĐÃ THÊM HÀM NÀY: Xử lý chức năng "Nhập Nhiều Từ" (Bulk Insert) an toàn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(List<Word> words)
        {
            try
            {
                // 1. Kiểm tra nếu không có dữ liệu
                if (words == null || words.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu hợp lệ được gửi lên.";
                    return RedirectToAction("Create"); 
                }

                int successCount = 0;

                // 2. Duyệt qua từng từ và xử lý an toàn
                foreach (var word in words)
                {
                    // Bỏ qua nếu dòng đó không có từ vựng (chỉ có khoảng trắng)
                    if (string.IsNullOrWhiteSpace(word.Term)) continue;

                    // Tự động điền giá trị mặc định để chống lỗi Database (Crash)
                    if (word.DifficultyLevel == 0) word.DifficultyLevel = 2;
                    if (string.IsNullOrEmpty(word.Example)) word.Example = "";
                    if (string.IsNullOrEmpty(word.Definition)) word.Definition = "Chưa có định nghĩa";
                    if (string.IsNullOrEmpty(word.Category)) word.Category = "Chưa phân loại";

                    // Xóa bộ đệm lỗi Validation trước khi lưu để bỏ qua các lỗi gò bó của Model
                    ModelState.Clear();

                    // Lưu vào DB
                    await _wordService.CreateAsync(word);
                    successCount++;
                }

                // 3. Thông báo thành công và chuyển hướng về trang danh sách
                TempData["SuccessMessage"] = $"Đã nhập nhanh thành công {successCount} từ vựng mới.";
                return RedirectToAction("Index", "Word");
            }
            catch (Exception ex)
            {
                // In lỗi ra màn hình đen (Console) để lập trình viên dễ tìm nguyên nhân
                Console.WriteLine($"[LỖI BULK CREATE]: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[LỖI CHI TIẾT DB]: {ex.InnerException.Message}");
                }

                // Chuyển hướng lại trang Create và báo lỗi thay vì văng trang trắng
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu vào Database. Vui lòng kiểm tra màn hình Console.";
                return RedirectToAction("Create");
            }
        }

        [HttpGet("api/word/auto-fetch")]
        public IActionResult AutoFetch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new { error = "Vui lòng cung cấp từ cần tra cứu." });
            }

            var data = new
            {
                term = term,
                definition = $"Định nghĩa giả định cho '{term}'.",
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
        public async Task<IActionResult> Delete(int id, string? confirmed)
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