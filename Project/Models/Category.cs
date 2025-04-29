using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
