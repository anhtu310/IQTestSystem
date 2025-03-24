using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers.Admin
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Giả lập dữ liệu thống kê
            ViewBag.TestCount = 120; // Số bài test
            ViewBag.UserCount = 350; // Số người dùng
            ViewBag.QuestionCount = 850; // Số câu hỏi
            ViewBag.CategoryCount = 20; // Số danh mục


            return View();
        }
    }
}
