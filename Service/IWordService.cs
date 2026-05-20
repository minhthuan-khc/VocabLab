using VocabLab.Models;

namespace VocabLab.Services
{
    public interface IWordService
    {
        Task<IEnumerable<Word>> GetAllAsync();
        Task<IEnumerable<Word>> GetByCategoryAsync(string category);
        Task<List<QuizQuestion>> GenerateQuizAsync(string userId, int count = 10);
        Task SaveProgressAsync(string userId, int wordId, bool isCorrect);
        Task<int> GetUserScoreAsync(string userId);
    }
}