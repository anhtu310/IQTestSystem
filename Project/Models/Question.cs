using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; }

    public virtual List<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
