using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class UserTest
{
    public int UserTestId { get; set; }

    public int UserId { get; set; }

    public int TestId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? Score { get; set; }

    public virtual Test Test { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
