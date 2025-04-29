using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; }

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
