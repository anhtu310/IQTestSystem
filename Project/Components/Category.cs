using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Components
{
    public class Category:ViewComponent
    {
        private readonly IqtestSystemContext context;
        public Category(IqtestSystemContext context)
        {
            this.context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var catergories = await context.Categories
                .AsNoTracking()
                .ToListAsync();
            return View(catergories);
        }
    }
}
