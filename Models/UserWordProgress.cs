using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VocabLab.Models;
using VocabLab.Services;
namespace VocabLab.Models
{
    public class UserWordProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int WordId { get; set; }
        public bool IsLearned { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public DateTime LastReviewed { get; set; }

        // Navigation
        public Word Word { get; set; }
        public ApplicationUser User { get; set; }
    }
}