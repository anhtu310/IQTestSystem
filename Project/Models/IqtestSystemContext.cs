using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

public partial class IqtestSystemContext : DbContext
{
    public IqtestSystemContext()
    {
    }

    public IqtestSystemContext(DbContextOptions<IqtestSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserTest> UserTests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=IQTestSystem;User Id=sa;Password=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825004E6B29A47");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerText)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Answer__Question__30F848ED");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0BE7951797");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FAC3896C361");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionText)
                .IsRequired()
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Test__8CC33160D245BD39");

            entity.ToTable("Test");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TestName)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Test__CategoryId__286302EC");

            entity.HasMany(d => d.Questions).WithMany(p => p.Tests)
                .UsingEntity<Dictionary<string, object>>(
                    "TestQuestion",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .HasConstraintName("FK__Test_Ques__Quest__2E1BDC42"),
                    l => l.HasOne<Test>().WithMany()
                        .HasForeignKey("TestId")
                        .HasConstraintName("FK__Test_Ques__TestI__2D27B809"),
                    j =>
                    {
                        j.HasKey("TestId", "QuestionId").HasName("PK__Test_Que__5C1F379A7DAF5A64");
                        j.ToTable("Test_Question");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C130D3450");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E4BB4EED47").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D105342A1FB2DE").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__47CE237F09EC6158");

            entity.ToTable("UserAnswer");

            entity.HasOne(d => d.Answer).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.AnswerId)
                .HasConstraintName("FK__UserAnswe__Answe__412EB0B6");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__Quest__403A8C7D");

            entity.HasOne(d => d.UserTest).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__UserT__3F466844");
        });

        modelBuilder.Entity<UserTest>(entity =>
        {
            entity.HasKey(e => e.UserTestId).HasName("PK__UserTest__16685683BE5A77CA");

            entity.ToTable("UserTest");

            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Test).WithMany(p => p.UserTests)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__UserTest__TestId__3C69FB99");

            entity.HasOne(d => d.User).WithMany(p => p.UserTests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserTest__UserId__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
