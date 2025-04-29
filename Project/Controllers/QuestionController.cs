using Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using X.PagedList.Extensions;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IqtestSystemContext context;
        public QuestionController(IqtestSystemContext context) => this.context = context;

        private const int PageSize = 5;
        // GET: QuestionController
        public IActionResult Index(string searchString, int? testFilter, int? page)
        {
            var questions = context.Questions
                .Include(q => q.Tests)
                .OrderBy(q => q.QuestionText)
                .AsQueryable();

            // Tìm kiếm theo nội dung câu hỏi
            if (!string.IsNullOrEmpty(searchString))
            {
                questions = questions.Where(q => q.QuestionText.Contains(searchString));
            }

            // Lọc theo bài test
            if (testFilter.HasValue && testFilter > 0)
            {
                questions = questions.Where(q => q.Tests.Any(t => t.TestId == testFilter));
            }

            // Danh sách bài test cho dropdown
            ViewBag.TestList = new SelectList(context.Tests, "TestId", "TestName");

            // Thông tin phân trang và filter
            int pageNumber = page ?? 1;
            var pagedQuestions = questions.ToPagedList(pageNumber, PageSize);

            ViewBag.SearchString = searchString;
            ViewBag.TestFilter = testFilter;

            return View(pagedQuestions);
        }

        // Hiển thị trang tạo câu hỏi
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName");
            return View(new Question { Answers = new List<Answer>() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Question question, int TestId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName", TestId);
                    return View(question);
                }

                // Lưu câu hỏi
                var newQuestion = new Question
                {
                    QuestionText = question.QuestionText
                };

                context.Questions.Add(newQuestion);
                context.SaveChanges();

                // Xử lý câu trả lời
                if (question.Answers != null)
                {
                    foreach (var answer in question.Answers)
                    {
                        if (!string.IsNullOrWhiteSpace(answer.AnswerText))
                        {
                            context.Answers.Add(new Answer
                            {
                                AnswerText = answer.AnswerText,
                                IsCorrect = answer.IsCorrect,
                                QuestionId = newQuestion.QuestionId
                            });
                        }
                    }
                    context.SaveChanges();
                }

                // Gán bài test
                if (TestId > 0)
                {
                    var test = context.Tests.Find(TestId);
                    if (test != null)
                    {
                        newQuestion.Tests.Add(test);
                        context.SaveChanges();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName", TestId);
                return View(question);
            }
        }

        // GET: QuestionController/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = context.Questions
                .Include(q => q.Answers)
                .Include(q => q.Tests)
                .FirstOrDefault(q => q.QuestionId == id);

            if (model == null) return NotFound();

            ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName", model.Tests.FirstOrDefault()?.TestId);
            return View(model);
        }

        // POST: QuestionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, Question question, int TestId)
        {
            if (id != question.QuestionId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName", TestId);
                return View(question);
            }

            try
            {
                // Lấy question hiện tại từ database
                var existingQuestion = context.Questions
                    .Include(q => q.Answers)
                    .Include(q => q.Tests)
                    .FirstOrDefault(q => q.QuestionId == id);

                if (existingQuestion == null) return NotFound();

                // Cập nhật thông tin cơ bản
                existingQuestion.QuestionText = question.QuestionText;

                // Xử lý các câu trả lời
                // 1. Lấy danh sách AnswerId hiện tại
                var existingAnswerIds = existingQuestion.Answers.Select(a => a.AnswerId).ToList();

                // 2. Lấy danh sách AnswerId từ form
                var submittedAnswerIds = question.Answers?
                    .Where(a => a.AnswerId != 0)
                    .Select(a => a.AnswerId)
                    .ToList() ?? new List<int>();

                // 3. Tìm các Answer cần xóa (có trong DB nhưng không có trong form)
                var answersToDelete = existingQuestion.Answers
                    .Where(a => !submittedAnswerIds.Contains(a.AnswerId))
                    .ToList();

                // 4. Xóa các Answer không còn được tham chiếu bởi UserAnswer
                foreach (var answer in answersToDelete)
                {
                    // Kiểm tra xem Answer có được tham chiếu trong UserAnswer không
                    var isReferenced = context.UserAnswers.Any(ua => ua.AnswerId == answer.AnswerId);
                    if (!isReferenced)
                    {
                        context.Answers.Remove(answer);
                    }
                }

                // 5. Cập nhật hoặc thêm mới các Answer
                if (question.Answers != null)
                {
                    foreach (var answer in question.Answers)
                    {
                        if (!string.IsNullOrWhiteSpace(answer.AnswerText))
                        {
                            if (answer.AnswerId != 0) // Answer đã tồn tại
                            {
                                var existingAnswer = existingQuestion.Answers
                                    .FirstOrDefault(a => a.AnswerId == answer.AnswerId);
                                if (existingAnswer != null)
                                {
                                    existingAnswer.AnswerText = answer.AnswerText;
                                    existingAnswer.IsCorrect = answer.IsCorrect;
                                }
                            }
                            else // Answer mới
                            {
                                var newAnswer = new Answer
                                {
                                    AnswerText = answer.AnswerText,
                                    IsCorrect = answer.IsCorrect,
                                    QuestionId = existingQuestion.QuestionId
                                };
                                context.Answers.Add(newAnswer);
                            }
                        }
                    }
                }

                // Xử lý bài test
                existingQuestion.Tests.Clear();
                if (TestId > 0)
                {
                    var test = context.Tests.Find(TestId);
                    if (test != null)
                    {
                        existingQuestion.Tests.Add(test);
                    }
                }

                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.QuestionId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool QuestionExists(int id)
        {
            return context.Questions.Any(e => e.QuestionId == id);
        }

        // GET: QuestionController/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var question = context.Questions
                .Include(q => q.Answers)
                .Include(q => q.UserAnswers)
                .Include(q => q.Tests)
                .ThenInclude(t => t.UserTests)
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null) return NotFound();

            context.Answers.RemoveRange(question.Answers);
            context.UserAnswers.RemoveRange(question.UserAnswers);

            foreach (var test in question.Tests.ToList())
            {
                test.Questions.Remove(question);
            }

            context.Questions.Remove(question);
            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
