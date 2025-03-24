using Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    public class TestController : Controller
    {
        private readonly IqtestSystemContext context;
        public TestController(IqtestSystemContext context) => this.context = context;
        // GET: TestController
        public ActionResult Index()
        {
            var model = context.Tests
                .Include(t => t.Category)
                .ToList();
            return View(model);
        }

        // GET: TestController/Create
        public ActionResult Create()
        {
            // Truyền danh sách Category đến View
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: TestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Test test)
        {
            if (ModelState.IsValid)
            {
                test.CreatedAt = DateTime.Now;
                // Thêm test vào cơ sở dữ liệu
                context.Tests.Add(test);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu không hợp lệ, truyền lại danh sách Category và hiển thị lại form
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName", test.CategoryId);
            return View(test);
        }

        // GET: TestController/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = context.Tests.Find(id);
            if (model == null)
            {
                return NotFound();
            }

            // Truyền danh sách các danh mục vào ViewBag
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName", model.CategoryId);

            return View(model);
        }

        // POST: TestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, Test test)
        {
            if (id != test.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingTest = context.Tests.Find(id);

                if (existingTest == null)
                {
                    return NotFound();
                }

                // Giữ nguyên giá trị CreatedAt từ cơ sở dữ liệu
                test.CreatedAt = existingTest.CreatedAt;

                // Cập nhật các trường khác
                context.Entry(existingTest).CurrentValues.SetValues(test);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, truyền lại danh sách các danh mục vào ViewBag
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName", test.CategoryId);

            return View(test);
        }

        // GET: TestController/Delete/5
        public ActionResult Delete(int? id)
        {
            var model = context.Tests.Find(id);
            context.Tests.Remove(model);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult TakeQuiz(int id)
        {
            var test = context.Tests
                .Include(t => t.Questions)
                  .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

    }
}
