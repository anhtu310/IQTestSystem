using Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models.ViewModels;
using System.Security.Claims;
using static Project.Models.ViewModels.QuizResultVM;
using Microsoft.AspNetCore.Authorization;
using X.PagedList.Extensions;

namespace FinalProject.Controllers
{
    public class TestController : Controller
    {
        private readonly IqtestSystemContext context;
        public TestController(IqtestSystemContext context) => this.context = context;
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string searchString, int? page, int? categoryId)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var tests = context.Tests
                .Include(t => t.Category)
                .Include(t => t.Questions)
                .AsQueryable();

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                tests = tests.Where(t => t.CategoryId == categoryId.Value);
            }

            // Thêm điều kiện tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                tests = tests.Where(t =>
                    t.TestName.Contains(searchString) ||
                    t.Description.Contains(searchString) ||
                    (t.Category != null && t.Category.CategoryName.Contains(searchString)));
            }

            // Sắp xếp và phân trang
            var pagedTests = tests.OrderBy(t => t.TestName)
                                .ToPagedList(pageNumber, pageSize);

            // Lấy danh sách tất cả các danh mục để hiển thị trong dropdown
            var categories = context.Categories.ToList();
            ViewBag.Categories = categories;

            // Lưu lại trạng thái tìm kiếm và danh mục đã chọn
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = categoryId;

            return View(pagedTests);
        }

        [Authorize(Roles = "Admin")]
        // GET: TestController/Create
        public ActionResult Create()
        {
            // Truyền danh sách Category đến View
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
                test.CreatedAt = existingTest.CreatedAt;

                context.Entry(existingTest).CurrentValues.SetValues(test);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(context.Categories, "CategoryId", "CategoryName", test.CategoryId);

            return View(test);
        }

        [Authorize(Roles = "Admin")]
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

            var userId = GetCurrentUserId();

            if (userId <= 0)
            {
                return RedirectToAction("Login", "Authentication");
            }

            HttpContext.Session.SetInt32("CurrentTestId", test.TestId);
            HttpContext.Session.SetInt32("CurrentUserId", userId);
            HttpContext.Session.SetString("StartTime", DateTime.Now.ToString());

            return View(test);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                throw new Exception("User chưa đăng nhập hoặc không có claim NameIdentifier");
            }

            return int.Parse(claim.Value);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(int testId, IFormCollection form)
        {
            // Lấy thông tin từ Session
            var userId = HttpContext.Session.GetInt32("CurrentUserId");
            var startTimeStr = HttpContext.Session.GetString("StartTime");
            DateTime startTime;

            if (!userId.HasValue || !DateTime.TryParse(startTimeStr, out startTime))
            {
                return RedirectToAction("Error", "Home");
            }

            var test = context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == testId);

            if (test == null) return NotFound();

            int correctCount = 0;
            var result = new QuizResultVM
            {
                TestName = test.TestName,
                TotalQuestions = test.Questions.Count,
                QuestionResults = new List<QuizResultVM.QuestionResult>()
            };

            foreach (var question in test.Questions)
            {
                string key = $"answers[{question.QuestionId}]";
                int userAnswerId = form.ContainsKey(key) ? int.Parse(form[key]) : 0;

                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                bool isCorrect = userAnswerId != 0 && correctAnswer?.AnswerId == userAnswerId;
                if (isCorrect) correctCount++;

                result.QuestionResults.Add(new QuizResultVM.QuestionResult
                {
                    QuestionText = question.QuestionText,
                    UserAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == userAnswerId)?.AnswerText ?? "Không trả lời",
                    CorrectAnswer = correctAnswer?.AnswerText ?? "Không có",
                });
            }

            result.CorrectAnswers = correctCount;
            result.Score = correctCount * 100 / result.TotalQuestions;

            var userTest = new UserTest
            {
                TestId = testId,
                UserId = userId.Value,
                StartTime = startTime,
                EndTime = DateTime.Now,
                Score = result.Score
            };

            context.UserTests.Add(userTest);
            context.SaveChanges();

            foreach (var question in test.Questions)
            {
                string key = $"answers[{question.QuestionId}]";
                int userAnswerId = form.ContainsKey(key) ? int.Parse(form[key]) : 0;

                var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == userAnswerId);
                if (selectedAnswer == null)
                {
                    continue;
                }

                var userAnswer = new UserAnswer
                {
                    UserTestId = userTest.UserTestId,
                    QuestionId = question.QuestionId,
                    AnswerId = selectedAnswer.AnswerId,
                    IsCorrect = selectedAnswer.IsCorrect
                };

                context.UserAnswers.Add(userAnswer);
            }

            context.SaveChanges();

            // Xóa session sau khi hoàn thành
            HttpContext.Session.Remove("CurrentTestId");
            HttpContext.Session.Remove("CurrentUserId");
            HttpContext.Session.Remove("StartTime");

            return View("Result", result);
        }
        public IActionResult Result(int userTestId)
        {
            var userTest = context.UserTests
                .Include(ut => ut.Test)
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(ut => ut.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .Include(ut => ut.UserAnswers)
                    .ThenInclude(ua => ua.Answer)
                .FirstOrDefault(ut => ut.UserTestId == userTestId);

            if (userTest == null) return NotFound();

            var test = userTest.Test;
            var allQuestions = test.Questions.ToList();

            var answeredQuestions = userTest.UserAnswers
                .Where(ua => ua.AnswerId != null)
                .ToList();

            var totalQuestions = allQuestions.Count;
            if (totalQuestions == 0)
            {
                return View("Error");
            }

            var correctAnswersCount = answeredQuestions.Count(ua => ua.IsCorrect == true);
            var score = (correctAnswersCount * 100) / totalQuestions;

            var result = new QuizResultVM
            {
                TestName = test.TestName,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswersCount,
                Score = score,
                QuestionResults = allQuestions.Select(q => new QuizResultVM.QuestionResult
                {
                    QuestionText = q.QuestionText,
                    UserAnswer = answeredQuestions.FirstOrDefault(ua => ua.QuestionId == q.QuestionId)?.Answer?.AnswerText ?? "Không trả lời",
                    CorrectAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect)?.AnswerText ?? "Không có"
                }).ToList()
            };

            return View(result);
        }

        public IActionResult TestHistory(string searchString, int? page, DateTime? fromDate, DateTime? toDate)
        {
            var userId = GetCurrentUserId();

            var userTests = context.UserTests
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Test)
                .Include(ut => ut.UserAnswers)
                .AsQueryable();

            // Thêm điều kiện tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                userTests = userTests.Where(ut =>
                    ut.Test.TestName.Contains(searchString) ||
                    ut.Test.Description.Contains(searchString));
            }

            // Lọc theo khoảng thời gian nếu có
            if (fromDate.HasValue)
            {
                userTests = userTests.Where(ut => ut.StartTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                userTests = userTests.Where(ut => ut.StartTime <= toDate.Value);
            }

            // Sắp xếp và phân trang
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var pagedUserTests = userTests
                .OrderByDescending(ut => ut.StartTime)
                .ToPagedList(pageNumber, pageSize);

            // Trả về view với dữ liệu đã phân trang
            var history = userTests
                .OrderByDescending(ut => ut.StartTime)
                .Select(ut => new UserTestHistoryVM
                {
                    UserTestId = ut.UserTestId,
                    TestName = ut.Test.TestName,
                    StartTime = ut.StartTime ?? DateTime.MinValue,
                    EndTime = ut.EndTime,
                    Score = ut.Score ?? 0,
                    TotalQuestions = ut.UserAnswers.Count,
                    CorrectAnswers = ut.UserAnswers.Count(ua => ua.IsCorrect == true)
                }).ToPagedList(pageNumber, pageSize);

            // Truyền ViewBag cho tìm kiếm và ngày
            ViewBag.SearchString = searchString;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(history);
        }
    }
}