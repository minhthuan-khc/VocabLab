using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VocabLab.Models;
using VocabLab.Services;
namespace VocabLab.Models 
{
public class QuizQuestion
    {
        public Word CorrectWord { get; set; }
        public List<string> Options { get; set; }  // 4 đáp án
        public string QuestionType { get; set; }   // "Definition" | "Term"
    }
}