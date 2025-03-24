using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public virtual List<Answer> Answers { get; set; } = new List<Answer>();

    public virtual List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual List<Test> Tests { get; set; } = new List<Test>();
}
