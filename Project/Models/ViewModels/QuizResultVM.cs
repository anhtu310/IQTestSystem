namespace Project.Models.ViewModels
{
    public class QuizResultVM
    {
        public string TestName { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
        public List<QuestionResult> QuestionResults { get; set; }

        public class QuestionResult
        {
            public string QuestionText { get; set; }
            public string UserAnswer { get; set; }
            public string CorrectAnswer { get; set; }
            public bool IsCorrect => UserAnswer == CorrectAnswer;
        }
    }

}
