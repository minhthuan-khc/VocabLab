using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocabLab.Data;
using VocabLab.ViewModels;

namespace VocabLab.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;

        public SettingsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Settings
        public async Task<IActionResult> Index()
        {
            // Giả định lấy ID của User hiện tại đang đăng nhập
            // Nếu dùng Identity: string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Tạm thời lấy User đầu tiên trong DB để test nếu chưa làm Đăng nhập:
            var user = await _db.Users.FirstOrDefaultAsync(); 
            
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new SettingsViewModel
            {
                FullName = user.FullName ?? user.UserName ?? "",
                // Điền thêm các trường cài đặt khác nếu bảng Users của bạn có sẵn:
                // IsDarkMode = user.IsDarkMode,
                // EnableReminder = user.EnableReminder,
                // ReminderTime = user.ReminderTime
            };

            return View(vm);
        }

        // POST: /Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Users.FirstOrDefaultAsync(); // Thay bằng logic tìm theo UserId chuẩn của bạn
            if (user == null) return NotFound();

            // Cập nhật thông tin từ ViewModel vào Database Model
            user.FullName = model.FullName;
            // user.IsDarkMode = model.IsDarkMode;
            // user.EnableReminder = model.EnableReminder;
            // user.ReminderTime = model.ReminderTime;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            // Gửi thông báo thành công về lại giao diện
            model.StatusMessage = "Cập nhật cấu hình thành công!";
            return View(model);
        }
    }
}