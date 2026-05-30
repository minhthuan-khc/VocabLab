using System.ComponentModel.DataAnnotations;

namespace VocabLab.ViewModels
{
    public class SettingsViewModel
    {
        // 1. Thông tin cá nhân
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        // 2. Cấu hình thông báo (Đã xóa IsDarkMode)
        public bool EnableReminder { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan ReminderTime { get; set; }

        public string? StatusMessage { get; set; }
    }
}