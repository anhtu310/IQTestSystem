using Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using X.PagedList.Extensions;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {

        private readonly IqtestSystemContext context;
        public CategoryController(IqtestSystemContext context)
        {
            this.context = context;
        }


    public ActionResult Index(string searchString, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var categories = context.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            categories = categories.Where(c => c.CategoryName.Contains(searchString) || c.Description.Contains(searchString));
        }

        ViewBag.SearchString = searchString;

        var pagedCategories = categories.OrderBy(c => c.CategoryName).ToPagedList(pageNumber, pageSize);

        return View(pagedCategories);
    }

    // GET: CategoryController/Create
    public ActionResult Create()
        {
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
           if (ModelState.IsValid)
            {
                context.Add(category);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
           return View(category);
        }

        // GET: CategoryController/Edit/5
        public ActionResult Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var category = context.Categories.Find(id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, Category category)
        {
           if(id != category.CategoryId)
            {
                return NotFound();
            }
           if(ModelState.IsValid)
            {
                context.Update(category);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
           return View(category);
        }

        // GET: CategoryController/Delete/5
        public ActionResult Delete(int? id)
        {
            var category = context.Categories.Find(id);
            context.Categories.Remove(category);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // POST: CategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
