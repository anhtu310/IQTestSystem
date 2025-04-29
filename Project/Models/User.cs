using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public bool IsAdmin { get; set; }

    public bool Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<UserTest> UserTests { get; set; } = new List<UserTest>();
}
