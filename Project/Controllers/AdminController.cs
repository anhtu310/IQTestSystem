using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IqtestSystemContext context;

        public AdminController(IqtestSystemContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TestCount = context.Tests.Count();
            ViewBag.UserCount = context.Users.Count();
            ViewBag.QuestionCount = context.Questions.Count();
            ViewBag.CategoryCount = context.Categories.Count();

            return View();
        }
    }
}
