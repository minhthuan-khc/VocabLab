namespace VocabLab.ViewModels
{
    public class LeaderboardViewModel
    {
        public string ActiveTab { get; set; } = "score";
        public List<LeaderboardEntryViewModel> ScoreRanking  { get; set; } = new();
        public List<LeaderboardEntryViewModel> StreakRanking { get; set; } = new();

        // Heatmap: key = "yyyy-MM-dd", value = số từ đã học
        public Dictionary<string, int> HeatmapData { get; set; } = new();
    }

    public class LeaderboardEntryViewModel
    {
        public string UserId   { get; set; } = "";
        public string FullName { get; set; } = "";
        public int    Value    { get; set; }
    }
}
