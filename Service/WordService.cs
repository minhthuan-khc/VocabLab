using Microsoft.EntityFrameworkCore;
using VocabLab.Data;
using VocabLab.Models;

namespace VocabLab.Services
{
    public class WordService : IWordService
    {
        private readonly AppDbContext _context;

        public WordService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Word>> GetAllAsync()
        {
            return await _context.Words.ToListAsync();
        }

        public async Task<IEnumerable<Word>> GetByCategoryAsync(string category)
        {
            return await _context.Words
                .Where(w => w.Category == category)
                .ToListAsync();
        }

        public async Task<List<QuizQuestion>> GenerateQuizAsync(
            string userId,
            int count = 10)
        {
            // Lấy dữ liệu trước
            var allWords = await _context.Words.ToListAsync();

            // Random bằng C#
            var words = allWords
                .OrderBy(w => Guid.NewGuid())
                .Take(count)
                .ToList();

            return words.Select(w => new QuizQuestion
            {
                CorrectWord = w,
                Options = GenerateOptions(w, allWords),
                QuestionType = "Definition"
            }).ToList();
        }

        private List<string> GenerateOptions(
            Word correct,
            List<Word> allWords)
        {
            var wrong = allWords
                .Where(w => w.Id != correct.Id)
                .OrderBy(w => Guid.NewGuid())
                .Take(3)
                .Select(w => w.Definition)
                .ToList();

            wrong.Add(correct.Definition);

            return wrong
                .Distinct()
                .OrderBy(x => Guid.NewGuid())
                .ToList();
        }

        public async Task SaveProgressAsync(
            string userId,
            int wordId,
            bool isCorrect)
        {
            var progress =
                await _context.UserWordProgresses
                .FirstOrDefaultAsync(
                    p => p.UserId == userId &&
                         p.WordId == wordId);

            if (progress == null)
            {
                progress = new UserWordProgress
                {
                    UserId = userId,
                    WordId = wordId
                };

                _context.UserWordProgresses.Add(progress);
            }

            if (isCorrect)
                progress.CorrectCount++;
            else
                progress.WrongCount++;

            progress.LastReviewed = DateTime.Now;

            progress.IsLearned =
                progress.CorrectCount >= 3;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserScoreAsync(string userId)
        {
            return await _context.UserWordProgresses
                .Where(p =>
                    p.UserId == userId &&
                    p.IsLearned)
                .CountAsync();
        }
    }
}