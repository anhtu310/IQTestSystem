﻿-- Tạo database
CREATE DATABASE IQTestSystem;
GO
USE IQTestSystem;
GO

-- Bảng Category (Danh mục bài test)
CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL
);
GO

-- Bảng Test (Bài kiểm tra)
CREATE TABLE Test (
    TestId INT IDENTITY(1,1) PRIMARY KEY,
    TestName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CategoryId INT NOT NULL,
    TimeLimit INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE CASCADE
);
GO

-- Bảng Question (Câu hỏi)
CREATE TABLE Question (
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,
    QuestionText NVARCHAR(1000) NOT NULL
);
GO

-- Bảng Test_Question (Liên kết Test và Question)
CREATE TABLE Test_Question (
    TestId INT NOT NULL,
    QuestionId INT NOT NULL,
    PRIMARY KEY (TestId, QuestionId),
    FOREIGN KEY (TestId) REFERENCES Test(TestId) ON DELETE CASCADE,
    FOREIGN KEY (QuestionId) REFERENCES Question(QuestionId) ON DELETE CASCADE
);
GO

-- Bảng Answer (Đáp án)
CREATE TABLE Answer (
    AnswerId INT IDENTITY(1,1) PRIMARY KEY,
    QuestionId INT NOT NULL,
    AnswerText NVARCHAR(500) NOT NULL,
    IsCorrect BIT NOT NULL,
    FOREIGN KEY (QuestionId) REFERENCES Question(QuestionId) ON DELETE CASCADE
);
GO

-- Bảng User (Người dùng)
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(255) UNIQUE NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0, -- 0 = Customer, 1 = Admin
    Status BIT NOT NULL DEFAULT 1, -- 1 = Active, 0 = Inactive
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Bảng UserTest (Lịch sử làm bài của người dùng)
CREATE TABLE UserTest (
    UserTestId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TestId INT NOT NULL,
    StartTime DATETIME DEFAULT GETDATE(),
    EndTime DATETIME NULL,
    Score INT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE,
    FOREIGN KEY (TestId) REFERENCES Test(TestId) ON DELETE CASCADE
);
GO

CREATE TABLE UserAnswer (
    UserAnswerId INT IDENTITY(1,1) PRIMARY KEY,
    UserTestId INT NOT NULL,
    QuestionId INT NOT NULL,
    AnswerId INT NULL,
    IsCorrect BIT NULL,

    FOREIGN KEY (UserTestId) REFERENCES UserTest(UserTestId) ON DELETE NO ACTION,
    FOREIGN KEY (QuestionId) REFERENCES Question(QuestionId) ON DELETE NO ACTION,
    FOREIGN KEY (AnswerId) REFERENCES Answer(AnswerId) ON DELETE NO ACTION
);
GO

INSERT INTO Category (CategoryName, Description)
VALUES 
(N'Logic Test', N'Kiểm tra khả năng tư duy logic'),
(N'Math Test', N'Kiểm tra khả năng toán học'),
(N'Pattern Recognition', N'Nhận diện mẫu hình'),
(N'Memory Test', N'Kiểm tra trí nhớ'),
(N'Language Test', N'Kiểm tra ngôn ngữ');

INSERT INTO Test (TestName, Description, CategoryId, TimeLimit)
VALUES 
(N'Test Logic 1', N'Mô tả Test Logic 1', 1, 30),
(N'Test Logic 2', N'Mô tả Test Logic 2', 1, 30),
(N'Test Math 1', N'Mô tả Test Math 1', 2, 40),
(N'Test Math 2', N'Mô tả Test Math 2', 2, 40),
(N'Test Pattern 1', N'Mô tả Test Pattern 1', 3, 35),
(N'Test Pattern 2', N'Mô tả Test Pattern 2', 3, 35),
(N'Test Memory 1', N'Mô tả Test Memory 1', 4, 25),
(N'Test Memory 2', N'Mô tả Test Memory 2', 4, 25),
(N'Test Language 1', N'Mô tả Test Language 1', 5, 30),
(N'Test Language 2', N'Mô tả Test Language 2', 5, 30);

-- Tạo 150 câu hỏi
DECLARE @i INT = 1;
WHILE @i <= 150
BEGIN
    INSERT INTO Question (QuestionText)
    VALUES (N'Câu hỏi số ' + CAST(@i AS NVARCHAR(10)));

    SET @i = @i + 1;
END
-- Gán mỗi bài test 15 câu hỏi
DECLARE @testId INT = 1;
DECLARE @questionStart INT = 1;
WHILE @testId <= 10
BEGIN
    DECLARE @q INT = 0;
    WHILE @q < 15
    BEGIN
        INSERT INTO Test_Question (TestId, QuestionId)
        VALUES (@testId, @questionStart + @q);

        SET @q = @q + 1;
    END
    SET @questionStart = @questionStart + 15;
    SET @testId = @testId + 1;
END
-- Mỗi câu hỏi có 4 đáp án, 1 đáp án đúng (ví dụ: đáp án 1 luôn đúng)
DECLARE @questionId INT = 1;
WHILE @questionId <= 150
BEGIN
    INSERT INTO Answer (QuestionId, AnswerText, IsCorrect)
    VALUES 
    (@questionId, N'Đáp án A cho câu ' + CAST(@questionId AS NVARCHAR(10)), 1), -- đúng
    (@questionId, N'Đáp án B cho câu ' + CAST(@questionId AS NVARCHAR(10)), 0),
    (@questionId, N'Đáp án C cho câu ' + CAST(@questionId AS NVARCHAR(10)), 0),
    (@questionId, N'Đáp án D cho câu ' + CAST(@questionId AS NVARCHAR(10)), 0);

    SET @questionId = @questionId + 1;
END
