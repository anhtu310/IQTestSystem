using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int UserTestId { get; set; }

    public int QuestionId { get; set; }

    public int? AnswerId { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual Answer? Answer { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual UserTest UserTest { get; set; } = null!;
}
