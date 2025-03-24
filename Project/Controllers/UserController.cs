using Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    public class UserController : Controller
    {
        private readonly IqtestSystemContext _context;

        public UserController(IqtestSystemContext context)
        {
            _context = context;
        }

        // GET: UserController
        public ActionResult Index()
        {
            // Lấy danh sách người dùng trừ admin
            var model = _context.Users.Where(u => u.IsAdmin != true).ToList();
            return View(model);
        }

        // POST: UserController/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái
            user.Status = !user.Status;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}