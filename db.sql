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

