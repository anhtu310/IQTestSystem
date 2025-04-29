using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Project.Models;

using X.PagedList;
using X.PagedList.Extensions;

namespace Project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IqtestSystemContext context;

    public HomeController(ILogger<HomeController> logger, IqtestSystemContext context)
    {
        _logger = logger;
        this.context = context;
    }

    public IActionResult Index(string searchTitle, int? categoryID, int? page)
    {
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }
        int pageSize = 4;
        int pageNumber = page ?? 1;

        var categories = context.Categories.ToList();
        var test = context.Tests.Include(x => x.Category).Where(t => t.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(searchTitle))
        {
            var loweredSearch = searchTitle.ToLower();

            test = test.Where(n =>
                (n.TestName != null && n.TestName.ToLower().Contains(loweredSearch)) ||
                (n.Description != null && n.Description.ToLower().Contains(loweredSearch)) ||
                (n.Category != null && n.Category.CategoryName.ToLower().Contains(loweredSearch)) ||
                (n.CreatedAt.HasValue && n.CreatedAt.Value.ToString("yyyy-MM-dd").Contains(loweredSearch))
            );
        }

        if (categoryID.HasValue)
        {
            test = test.Where(n => n.CategoryId == categoryID);
            ViewBag.CategoryID = categoryID;
        }

        ViewBag.Categories = categories;
        ViewBag.SearchTitle = searchTitle;

        var pagedTests = test.OrderByDescending(t => t.CreatedAt).ToPagedList(pageNumber, pageSize);

        return View(pagedTests);
    }

public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
