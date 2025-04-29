using Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList.Extensions;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IqtestSystemContext _context;
        private const int PageSize = 5; // Số lượng item mỗi trang

        public UserController(IqtestSystemContext context)
        {
            _context = context;
        }

        // GET: UserController
        public ActionResult Index(string searchString, string statusFilter, int? page)
        {
            // Lấy danh sách người dùng trừ admin
            var users = _context.Users
                .Where(u => u.IsAdmin != true)
                .OrderBy(u => u.Username)
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.Email.Contains(searchString));
            }

            // Áp dụng bộ lọc trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "active";
                users = users.Where(u => u.Status == isActive);
            }

            // Phân trang
            int pageNumber = page ?? 1;
            var pagedUsers = users.ToPagedList(pageNumber, PageSize);

            // Truyền các tham số tìm kiếm/lọc để giữ lại khi phân trang
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Tất cả", Value = "" },
                new SelectListItem { Text = "Hoạt động", Value = "active" },
                new SelectListItem { Text = "Tạm khóa", Value = "inactive" }
            };

            return View(pagedUsers);
        }

        // POST: UserController/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, string searchString, string statusFilter, int? page)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái
            user.Status = !user.Status;
            _context.SaveChanges();

            // Giữ lại các tham số tìm kiếm/lọc khi redirect
            return RedirectToAction(nameof(Index), new
            {
                searchString,
                statusFilter,
                page
            });
        }
    }
}