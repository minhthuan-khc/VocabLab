using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VocabLab.Models;
using VocabLab.Services;
namespace VocabLab.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string Term { get; set; }          // Từ tiếng Anh
        public string Definition { get; set; }    // Nghĩa
        public string Example { get; set; }       // Ví dụ
        public string Category { get; set; }      // Chủ đề (Animals, Food...)
        public int DifficultyLevel { get; set; }  // 1=Easy, 2=Medium, 3=Hard
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<UserWordProgress> Progresses { get; set; }

    }
}