-- Tạo database
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
    (N'Trí Tuệ', N'Các bài kiểm tra về tư duy logic'),
    (N'Toán Học', N'Các bài kiểm tra về kiến thức toán học'),
    (N'Ngôn Ngữ', N'Các bài kiểm tra về ngôn ngữ và từ vựng');
GO


INSERT INTO Test (TestName, Description, CategoryId, TimeLimit, IsActive)
VALUES 
    (N'Test IQ Cơ Bản', N'Bài test IQ với các câu hỏi đơn giản', 1, 30, 1),
    (N'Toán Học Cơ Bản', N'Bài kiểm tra toán học cho học sinh lớp 5', 2, 45, 1),
    (N'Tiếng Anh Căn Bản', N'Trắc nghiệm về từ vựng và ngữ pháp tiếng Anh', 3, 40, 1);
GO
INSERT INTO Question (QuestionText)
VALUES 
    (N'2 + 2 bằng mấy?'),
    (N'Hình tam giác có bao nhiêu cạnh?'),
    (N'Từ nào đồng nghĩa với "Happy" trong tiếng Anh?');
GO
INSERT INTO Test_Question (TestId, QuestionId)
VALUES 
    (1, 1),  -- "2 + 2 bằng mấy?" thuộc "Test IQ Cơ Bản"
    (2, 2),  -- "Hình tam giác có bao nhiêu cạnh?" thuộc "Toán Học Cơ Bản"
    (3, 3);  -- "Từ nào đồng nghĩa với 'Happy'?" thuộc "Tiếng Anh Căn Bản"
GO
INSERT INTO Answer (QuestionId, AnswerText, IsCorrect)
VALUES 
    (1, N'3', 0),
    (1, N'4', 1),
    (1, N'5', 0),
    
    (2, N'2', 0),
    (2, N'3', 1),
    (2, N'4', 0),

    (3, N'Sad', 0),
    (3, N'Excited', 0),
    (3, N'Joyful', 1);
GO
INSERT INTO [User] (Username, Email, PasswordHash, IsAdmin)
VALUES 
    (N'admin', N'admin@example.com', N'hashed_password_admin', 1), -- Admin
    (N'user1', N'user1@example.com', N'hashed_password_user1', 0), -- Customer
    (N'user2', N'user2@example.com', N'hashed_password_user2', 0); -- Customer
GO
INSERT INTO UserTest (UserId, TestId, StartTime, Score)
VALUES 
    (2, 1, GETDATE(), 85), -- User1 làm bài Test IQ
    (3, 2, GETDATE(), 90); -- User2 làm bài Toán Học
GO
INSERT INTO UserAnswer (UserTestId, QuestionId, AnswerId, IsCorrect)
VALUES 
    (1, 1, 2, 1), -- User1 chọn "4" cho câu "2 + 2", đúng
    (2, 2, 5, 1); -- User2 chọn "3" cho câu "Hình tam giác có bao nhiêu cạnh?", đúng
GO
