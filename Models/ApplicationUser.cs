using Microsoft.AspNetCore.Identity;

namespace VocabLab.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public int TotalScore { get; set; }
        public ICollection<UserWordProgress>? WordProgresses { get; set; }
    }
}