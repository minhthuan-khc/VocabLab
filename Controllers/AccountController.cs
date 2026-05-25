using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VocabLab.Data;
using VocabLab.Models;

namespace VocabLab.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // 1. GIAO DIỆN ĐĂNG NHẬP (GET)
        // =======================================================
        [HttpGet]
        public IActionResult Login()
        {
            // Luôn luôn hiển thị trang Login khi gọi tới, không chặn tự động
            return View();
        }

        // =======================================================
        // 2. XỬ LÝ ĐĂNG NHẬP (POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ ID người dùng và mật khẩu.";
                return RedirectToAction("Login", "Account");
            }

            // Tìm tài khoản theo đúng cấu trúc Identity của ApplicationUser
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username.Trim() && u.PasswordHash == password);

            if (user != null)
            {
                // Cấp Cookie lưu danh tính người dùng trong 7 ngày trên toàn hệ thống
                Response.Cookies.Append("vocab_user", user.UserName, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true,
                    Path = "/" // Đảm bảo Cookie có hiệu lực ở mọi trang con
                });

                TempData["SuccessMessage"] = $"Chào mừng quay trở lại, {user.UserName}!";
                
                // ĐIỀU HƯỚNG CHUẨN: Thành công thì đẩy qua trang danh sách từ vựng Word
                return RedirectToAction("Index", "Word");
            }

            // Nếu sai tài khoản mật khẩu, bắn lỗi và ép tải lại trang Login sạch sẽ
            TempData["ErrorMessage"] = "ID người dùng hoặc mật khẩu không chính xác.";
            return RedirectToAction("Login", "Account");
        }

        // =======================================================
        // 3. GIAO DIỆN ĐĂNG KÝ (GET)
        // =======================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =======================================================
        // 4. XỬ LÝ ĐĂNG KÝ TÀI KHOẢN (POST)
        // =======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "ID người dùng và mật khẩu không được bỏ trống.";
                return RedirectToAction("Register", "Account");
            }

            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không trùng khớp.";
                return RedirectToAction("Register", "Account");
            }

            if (password.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu phải chứa ít nhất 6 ký tự.";
                return RedirectToAction("Register", "Account");
            }

            // Kiểm tra trùng ID (UserName) dưới Database
            bool isIdTaken = await _context.Users
                .AnyAsync(u => u.UserName.ToLower() == username.Trim().ToLower());

            if (isIdTaken)
            {
                TempData["ErrorMessage"] = $"ID '{username}' đã có người sử dụng. Vui lòng chọn một ID khác!";
                // ĐIỀU HƯỚNG CHUẨN KHI LỖI: Ép trình duyệt Redirect lại chính trang Register để hiển thị TempData lỗi
                return RedirectToAction("Register", "Account");
            }

            // Khởi tạo User mới dựa trên Class ApplicationUser lõi của bạn
            var newUser = new ApplicationUser
            {
                UserName = username.Trim(),
                PasswordHash = password
            };

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Tạo thành công, điều hướng sang trang Đăng nhập và chúc mừng
                TempData["SuccessMessage"] = "Tạo tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi hệ thống xảy ra khi tạo tài khoản. Vui lòng thử lại.";
                Console.WriteLine($"[REGISTER DATABASE ERROR]: {ex.Message}");
                return RedirectToAction("Register", "Account");
            }
        }

        // =======================================================
        // 5. CHỨC NĂNG ĐĂNG XUẤT (LOGOUT)
        // =======================================================
        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa bỏ hoàn toàn Cookie lưu danh tính
            Response.Cookies.Delete("vocab_user");
            
            // Đưa người dùng quay lại trang Đăng nhập từ đầu
            return RedirectToAction("Login", "Account");
        }
    }
}