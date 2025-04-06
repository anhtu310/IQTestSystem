﻿using Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace FinalProject.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IqtestSystemContext context;
        public QuestionController(IqtestSystemContext context) => this.context = context;

        // GET: QuestionController
        public ActionResult Index()
        {
            var model = context.Questions
                .Include(q => q.Tests) // Đảm bảo load danh sách bài test
                .ToList();
            return View(model);
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
            if (!ModelState.IsValid)
            {
                ViewBag.TestId = new SelectList(context.Tests, "TestId", "TestName", TestId);
                return View(question);
            }

            // Tạo mới câu hỏi (không bao gồm Answers)
            var newQuestion = new Question
            {
                QuestionText = question.QuestionText
                // Thêm các thuộc tính khác nếu có
            };

            context.Questions.Add(newQuestion);
            context.SaveChanges(); // Lưu để có QuestionId

            // Xử lý các câu trả lời
            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    if (!string.IsNullOrWhiteSpace(answer.AnswerText))
                    {
                        // Tạo mới Answer không bao gồm AnswerId
                        var newAnswer = new Answer
                        {
                            AnswerText = answer.AnswerText,
                            IsCorrect = answer.IsCorrect,
                            QuestionId = newQuestion.QuestionId
                        };
                        context.Answers.Add(newAnswer);
                    }
                }
            }

            // Gán bài test nếu có
            if (TestId > 0)
            {
                var test = context.Tests.Find(TestId);
                if (test != null)
                {
                    newQuestion.Tests.Add(test);
                }
            }

            context.SaveChanges();
            return RedirectToAction(nameof(Index));
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
