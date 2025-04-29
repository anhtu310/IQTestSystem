using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Test
{
    public int TestId { get; set; }

    public string TestName { get; set; }

    public string Description { get; set; }

    public int CategoryId { get; set; }

    public int TimeLimit { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Category Category { get; set; }

    public virtual ICollection<UserTest> UserTests { get; set; } = new List<UserTest>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
