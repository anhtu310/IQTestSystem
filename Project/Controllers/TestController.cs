using Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models.ViewModels;
using System.Security.Claims;
using static Project.Models.ViewModels.QuizResultVM;

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
                .Include(t=> t.Questions)
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
            var userId = GetCurrentUserId();
            var userTest = new UserTest
            {
                TestId = test.TestId,
                UserId = userId,
                StartTime = DateTime.Now
            };
            context.UserTests.Add(userTest);
            context.SaveChanges();
            HttpContext.Session.SetInt32("UserTestId", userTest.UserTestId);

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

            var userTestId = HttpContext.Session.GetInt32("UserTestId");
            if (userTestId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var userTest = context.UserTests
                .Include(ut => ut.UserAnswers)
                .FirstOrDefault(ut => ut.UserTestId == userTestId);

            if (userTest != null)
            {
                userTest.EndTime = DateTime.Now;
                userTest.Score = result.Score;
                userTest.UserAnswers.Clear();

                foreach (var question in test.Questions)
                {
                    string key = $"answers[{question.QuestionId}]";
                    int userAnswerId = form.ContainsKey(key) ? int.Parse(form[key]) : 0;

                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == userAnswerId);
                    if (selectedAnswer == null)
                    {
                        // Nếu không có câu trả lời hợp lệ, có thể bỏ qua hoặc thông báo lỗi
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
            }

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

        public IActionResult TestHistory()
        {
            var userId = GetCurrentUserId();

            var userTests = context.UserTests
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Test)
                .Include(ut => ut.UserAnswers)
                .OrderByDescending(ut => ut.StartTime)
                .ToList();

            var history = userTests.Select(ut => new UserTestHistoryVM
            {
                UserTestId = ut.UserTestId,
                TestName = ut.Test.TestName,
                StartTime = ut.StartTime ?? DateTime.MinValue,
                EndTime = ut.EndTime,
                Score = ut.Score ?? 0,
                TotalQuestions = ut.UserAnswers.Count,
                CorrectAnswers = ut.UserAnswers.Count(ua => ua.IsCorrect == true)
            }).ToList();

            return View(history);
        }

    }
}