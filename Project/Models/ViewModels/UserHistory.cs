namespace Project.Models.ViewModels
{
    public class UserTestHistoryVM
    {
        public int UserTestId { get; set; }
        public string TestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
