using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Project.Models;

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

    public IActionResult Index(string searchTitle, int? categoryID)
    {
        var categories = context.Categories.ToList();
        var test = context.Tests.Include(x => x.Category).Where(t=>t.IsActive).AsQueryable();

        if(!string.IsNullOrEmpty(searchTitle) )
        {
            test = test.Where(n=> n.TestName.Contains(searchTitle));
        }

        if(categoryID.HasValue )
        {
            test = test.Where(n => n.CategoryId == categoryID);
            ViewBag.selectedCategory = categoryID;
        }
        ViewBag.Categories = categories;
        ViewBag.Test = test;
        ViewBag.SearchTitle = searchTitle;
        return View();
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
