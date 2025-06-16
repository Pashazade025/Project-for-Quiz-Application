using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;



// ================================
// MODELS & ENUMS
// ================================
public enum QuestionType
{
    MultipleChoiceSingle = 1,
    MultipleChoiceMultiple = 2,
    TrueFalse = 3,
    ShortAnswer = 4
}

public class RegisterUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var salt = "QuizMaker2024!@#";
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}

public static class ValidationHelper
{
    public static List<string> ValidateRegistration(RegisterUserDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            errors.Add("First name is required");
        
        if (string.IsNullOrWhiteSpace(dto.LastName))
            errors.Add("Last name is required");
        
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required");
        
        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password is required");

        if (!string.IsNullOrWhiteSpace(dto.Email) && !IsValidEmail(dto.Email))
            errors.Add("Please enter a valid email address");

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password.Length < 6)
                errors.Add("Password must be at least 6 characters long");
            
            if (!dto.Password.Any(char.IsDigit))
                errors.Add("Password must contain at least one number");
            
            if (!dto.Password.Any(char.IsLetter))
                errors.Add("Password must contain at least one letter");
        }

        if (dto.Password != dto.ConfirmPassword)
            errors.Add("Passwords do not match");

        return errors;
    }

    public static List<string> ValidateLogin(LoginDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required");
        
        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password is required");

        if (!string.IsNullOrWhiteSpace(dto.Email) && !IsValidEmail(dto.Email))
            errors.Add("Please enter a valid email address");

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        return emailRegex.IsMatch(email);
    }
}

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
    public List<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Quiz> Quizzes { get; set; } = new List<Quiz>();
}

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int TimeLimit { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Category Category { get; set; }
    public ApplicationUser CreatedBy { get; set; }
    public List<Question> Questions { get; set; } = new List<Question>();
    public List<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public int Points { get; set; } = 1;
    public int OrderIndex { get; set; }
    public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    
    public double Percentage 
    { 
        get { return MaxScore > 0 ? (double)Score / MaxScore * 100 : 0; }
    }
    
    public bool IsCompleted 
    { 
        get { return CompletedAt.HasValue; }
    }
    
    // Navigation properties
    public Quiz Quiz { get; set; }
    public ApplicationUser User { get; set; }
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public class UserAnswer
{
    public int Id { get; set; }
    public int QuizAttemptId { get; set; }
    public int QuestionId { get; set; }
    public string SelectedOptionIds { get; set; }
    public string TextAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsAwarded { get; set; }
}

// ================================
// SERVICE RESULT PATTERN
// ================================
public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();

    public static ServiceResult Success()
    {
        return new ServiceResult { IsSuccess = true };
    }
    
    public static ServiceResult Failure(string error)
    {
        return new ServiceResult { IsSuccess = false, ErrorMessage = error };
    }


    public static ServiceResult Failure(List<string> errors)
    {
        return new ServiceResult { IsSuccess = false, Errors = errors, ErrorMessage = string.Join(", ", errors) };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T Data { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data };
    }
    
    public new static ServiceResult<T> Failure(string error)
    {
        return new ServiceResult<T> { IsSuccess = false, ErrorMessage = error };
    }

    // ADD THIS METHOD HERE:
    public new static ServiceResult<T> Failure(List<string> errors)
    {
        return new ServiceResult<T> { IsSuccess = false, Errors = errors, ErrorMessage = string.Join(", ", errors) };
    }
}

// ================================
// DATA STORE WITH ENHANCED SAMPLE DATA
// ================================


public class QuizDataStore
{
    private static QuizDataStore _instance;
    private const string DataFileName = "quizmaker_data.json"; 
    
    public static QuizDataStore Instance
    {
        get
        {
            if (_instance == null)
                _instance = new QuizDataStore();
            return _instance;
        }
    }

    public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public List<Question> Questions { get; set; } = new List<Question>();
    public List<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    public List<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    private int _nextQuizId = 1;
    private int _nextQuestionId = 1;
    private int _nextOptionId = 1;
    private int _nextAttemptId = 1;
    private int _nextAnswerId = 1;

    public int GetNextQuizId() { return _nextQuizId++; }
    public int GetNextQuestionId() { return _nextQuestionId++; }
    public int GetNextOptionId() { return _nextOptionId++; }
    public int GetNextAttemptId() { return _nextAttemptId++; }
    public int GetNextAnswerId() { return _nextAnswerId++; }


public void SaveData()
{
    try
    {
        var lines = new List<string>();
        
        // Save users
        lines.Add("USERS_START");
        foreach (var user in Users)
        {
            lines.Add($"{user.Id}|{user.UserName}|{user.Email}|{user.PasswordHash}|{user.FirstName}|{user.LastName}|{user.Role}|{user.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        lines.Add("USERS_END");
        
        // Save quiz attempts
        lines.Add("ATTEMPTS_START");
        foreach (var attempt in QuizAttempts.Where(a => a.IsCompleted))
        {
            lines.Add($"{attempt.Id}|{attempt.QuizId}|{attempt.UserId}|{attempt.StartedAt:yyyy-MM-dd HH:mm:ss}|{attempt.CompletedAt:yyyy-MM-dd HH:mm:ss}|{attempt.Score}|{attempt.MaxScore}");
        }
        lines.Add("ATTEMPTS_END");
        
        // Save user answers
        lines.Add("ANSWERS_START");
        foreach (var answer in UserAnswers)
        {
            lines.Add($"{answer.Id}|{answer.QuizAttemptId}|{answer.QuestionId}|{answer.SelectedOptionIds ?? ""}|{answer.TextAnswer ?? ""}|{answer.IsCorrect}|{answer.PointsAwarded}");
        }
        lines.Add("ANSWERS_END");
        
        // Save counters
        lines.Add("COUNTERS_START");
        lines.Add($"NextQuizId|{_nextQuizId}");
        lines.Add($"NextQuestionId|{_nextQuestionId}");
        lines.Add($"NextOptionId|{_nextOptionId}");
        lines.Add($"NextAttemptId|{_nextAttemptId}");
        lines.Add($"NextAnswerId|{_nextAnswerId}");
        lines.Add("COUNTERS_END");
        
        File.WriteAllLines(DataFileName, lines);
        Console.WriteLine("💾 Data saved successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error saving data: {ex.Message}");
    }
}


public void LoadData()
{
    try
    {
        if (!File.Exists(DataFileName))
        {
            Console.WriteLine("📁 No existing data file found. Starting fresh.");
            return;
        }

        var lines = File.ReadAllLines(DataFileName);
        string currentSection = "";
        
        foreach (var line in lines)
        {
            if (line.EndsWith("_START"))
            {
                currentSection = line.Replace("_START", "");
                continue;
            }
            
            if (line.EndsWith("_END"))
            {
                currentSection = "";
                continue;
            }
            
            var parts = line.Split('|');
            
            switch (currentSection)
            {
                case "USERS":
                    if (parts.Length >= 8)
                    {
                        var user = new ApplicationUser
                        {
                            Id = parts[0],
                            UserName = parts[1],
                            Email = parts[2],
                            PasswordHash = parts[3],
                            FirstName = parts[4],
                            LastName = parts[5],
                            Role = parts[6],
                            CreatedAt = DateTime.Parse(parts[7])
                        };
                        
                        // Only add if not already exists
                        if (!Users.Any(u => u.Id == user.Id))
                        {
                            Users.Add(user);
                        }
                    }
                    break;
                    
                case "ATTEMPTS":
                    if (parts.Length >= 7)
                    {
                        var attempt = new QuizAttempt
                        {
                            Id = int.Parse(parts[0]),
                            QuizId = int.Parse(parts[1]),
                            UserId = parts[2],
                            StartedAt = DateTime.Parse(parts[3]),
                            CompletedAt = DateTime.Parse(parts[4]),
                            Score = int.Parse(parts[5]),
                            MaxScore = int.Parse(parts[6])
                        };
                        
                        // Only add if not already exists
                        if (!QuizAttempts.Any(a => a.Id == attempt.Id))
                        {
                            QuizAttempts.Add(attempt);
                        }
                    }
                    break;
                    
                case "ANSWERS":
                    if (parts.Length >= 7)
                    {
                        var answer = new UserAnswer
                        {
                            Id = int.Parse(parts[0]),
                            QuizAttemptId = int.Parse(parts[1]),
                            QuestionId = int.Parse(parts[2]),
                            SelectedOptionIds = parts[3],
                            TextAnswer = parts[4],
                            IsCorrect = bool.Parse(parts[5]),
                            PointsAwarded = int.Parse(parts[6])
                        };
                        
                        // Only add if not already exists
                        if (!UserAnswers.Any(a => a.Id == answer.Id))
                        {
                            UserAnswers.Add(answer);
                        }
                    }
                    break;
                    
                case "COUNTERS":
                    if (parts.Length >= 2)
                    {
                        switch (parts[0])
                        {
                            case "NextQuizId":
                                _nextQuizId = int.Parse(parts[1]);
                                break;
                            case "NextQuestionId":
                                _nextQuestionId = int.Parse(parts[1]);
                                break;
                            case "NextOptionId":
                                _nextOptionId = int.Parse(parts[1]);
                                break;
                            case "NextAttemptId":
                                _nextAttemptId = int.Parse(parts[1]);
                                break;
                            case "NextAnswerId":
                                _nextAnswerId = int.Parse(parts[1]);
                                break;
                        }
                    }
                    break;
            }
        }

        Console.WriteLine("📂 Data loaded successfully!");
        Console.WriteLine($"   Loaded {Users.Count} users");
        Console.WriteLine($"   Loaded {QuizAttempts.Count} quiz attempts");
        Console.WriteLine($"   Loaded {UserAnswers.Count} user answers");
        
        
        LoadQuizRelationships();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error loading data: {ex.Message}");
        Console.WriteLine("Starting with fresh data.");
    }
}


private QuizDataStore()
{
    LoadData();
    
    // Always ensure we have sample quizzes and data
    if (Quizzes.Count == 0 || Questions.Count == 0)
    {
        Console.WriteLine("🏗️ Creating sample data...");
        
        // Create categories if missing
        if (Categories.Count == 0)
        {
            Categories.Add(new Category { Id = 1, Name = "Programming", Description = "Programming and computer science" });
            Categories.Add(new Category { Id = 2, Name = "General Knowledge", Description = "General knowledge questions" });
            Categories.Add(new Category { Id = 3, Name = "Science", Description = "Science and technology questions" });
            Categories.Add(new Category { Id = 4, Name = "Mathematics", Description = "Math and calculation questions" });
        }
        
        // Create users if missing
        if (Users.Count == 0)
        {
            Users.Add(new ApplicationUser
            {
                Id = "admin-123",
                UserName = "admin@quizmaker.com",
                Email = "admin@quizmaker.com",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin"
            });

            Users.Add(new ApplicationUser
            {
                Id = "user-123",
                UserName = "john@example.com",
                Email = "john@example.com",
                PasswordHash = PasswordHelper.HashPassword("user123"),
                FirstName = "John",
                LastName = "Doe",
                Role = "User"
            });
        }
        
        // Clear and recreate quizzes to ensure fresh data
        Quizzes.Clear();
        Questions.Clear();
        QuestionOptions.Clear();
        
        CreateSampleQuizzes();
        SaveData();
    }
    
    // Always load relationships
    LoadQuizRelationships();
}
    private void SeedData()
    {
        // Seed Categories
        Categories.Add(new Category { Id = 1, Name = "Programming", Description = "Programming and computer science" });
        Categories.Add(new Category { Id = 2, Name = "General Knowledge", Description = "General knowledge questions" });
        Categories.Add(new Category { Id = 3, Name = "Science", Description = "Science and technology questions" });
        Categories.Add(new Category { Id = 4, Name = "Mathematics", Description = "Math and calculation questions" });

        // Seed Users
        Users.Add(new ApplicationUser
{
    Id = "admin-123",
    UserName = "admin@quizmaker.com",
    Email = "admin@quizmaker.com",
    PasswordHash = PasswordHelper.HashPassword("admin123"),
    FirstName = "Admin",
    LastName = "User",
    Role = "Admin"
});

        Users.Add(new ApplicationUser
{
    Id = "user-123",
    UserName = "john@example.com",
    Email = "john@example.com",
    PasswordHash = PasswordHelper.HashPassword("user123"),
    FirstName = "John",
    LastName = "Doe",
    Role = "User"
});
        

        // Create Enhanced Sample Quizzes with More Questions
        CreateSampleQuizzes();
    }

    public Question CreateQuestion(int quizId, string text, QuestionType type, int points, int order, string[] optionTexts, bool[] optionCorrect)
    {
        var question = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = quizId,
            QuestionText = text,
            QuestionType = type,
            Points = points,
            OrderIndex = order
        };

        for (int i = 0; i < optionTexts.Length; i++)
        {
            question.Options.Add(new QuestionOption
            {
                Id = GetNextOptionId(),
                QuestionId = question.Id,
                OptionText = optionTexts[i],
                IsCorrect = optionCorrect[i],
                OrderIndex = i
            });
        }

        return question;
    }

    private void CreateSampleQuizzes()
    {
        // Enhanced Quiz 1: C# Programming Fundamentals (12 questions) - 1.5 minutes
        var csharpQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "C# Programming Fundamentals",
            Description = "Comprehensive test of C# programming concepts and syntax",
            CategoryId = 1,
            CreatedById = "admin-123",
            IsPublic = true,
            TimeLimit = 2 // 1.5 minutes (rounded to 2 for display)
        };

        var csharpQuestions = new Question[]
        {
            CreateQuestion(csharpQuiz.Id, "What is the correct way to declare a variable in C#?", QuestionType.MultipleChoiceSingle, 2, 0,
                new string[] { "var name = \"John\";", "variable name = \"John\";", "declare name = \"John\";", "name := \"John\";" },
                new bool[] { true, false, false, false }),
            
            CreateQuestion(csharpQuiz.Id, "C# is a case-sensitive programming language.", QuestionType.TrueFalse, 1, 1,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "Which of the following are valid C# access modifiers? (Select all that apply)", QuestionType.MultipleChoiceMultiple, 3, 2,
                new string[] { "public", "private", "protected", "secure" },
                new bool[] { true, true, true, false }),
            
            CreateQuestion(csharpQuiz.Id, "What does LINQ stand for?", QuestionType.MultipleChoiceSingle, 2, 3,
                new string[] { "Language Integrated Query", "Linear Intelligence Query", "Language Internet Query", "Language Interface Query" },
                new bool[] { true, false, false, false }),
            
            CreateQuestion(csharpQuiz.Id, "In C#, exceptions are handled using try-catch blocks.", QuestionType.TrueFalse, 1, 4,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "Which are valid C# data types? (Select all that apply)", QuestionType.MultipleChoiceMultiple, 2, 5,
                new string[] { "int", "string", "boolean", "bool" },
                new bool[] { true, true, false, true }),
            
            CreateQuestion(csharpQuiz.Id, "C# allows null values for reference types by default.", QuestionType.TrueFalse, 1, 6,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "What keyword is used for inheritance in C#?", QuestionType.MultipleChoiceSingle, 2, 7,
                new string[] { "extends", "inherits", ":", "implements" },
                new bool[] { false, false, true, false }),
            
            CreateQuestion(csharpQuiz.Id, "int is a value type in C#.", QuestionType.TrueFalse, 1, 8,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "Static members belong to the class rather than instances.", QuestionType.TrueFalse, 1, 9,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "C# uses automatic garbage collection for memory management.", QuestionType.TrueFalse, 1, 10,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(csharpQuiz.Id, "What is the correct way to declare an array of integers?", QuestionType.MultipleChoiceSingle, 2, 11,
                new string[] { "int[] numbers;", "int numbers[];", "array<int> numbers;", "integer[] numbers;" },
                new bool[] { true, false, false, false })
        };

        csharpQuiz.Questions.AddRange(csharpQuestions);
        Quizzes.Add(csharpQuiz);
        Questions.AddRange(csharpQuestions);
        QuestionOptions.AddRange(csharpQuestions.SelectMany(q => q.Options));

        // Enhanced Quiz 2: World Geography (12 questions) - 1.5 minutes
        var geographyQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "World Geography Quiz",
            Description = "Comprehensive test of world geography knowledge",
            CategoryId = 2,
            CreatedById = "admin-123",
            IsPublic = true,
            TimeLimit = 2 // 1.5 minutes
        };

        var geographyQuestions = new Question[]
        {
            CreateQuestion(geographyQuiz.Id, "What is the capital of France?", QuestionType.MultipleChoiceSingle, 1, 0,
                new string[] { "London", "Berlin", "Paris", "Madrid" },
                new bool[] { false, false, true, false }),
            
            CreateQuestion(geographyQuiz.Id, "The Great Wall of China was built to protect against invasions.", QuestionType.TrueFalse, 1, 1,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "Which planet is known as the Red Planet?", QuestionType.MultipleChoiceSingle, 1, 2,
                new string[] { "Venus", "Mars", "Jupiter", "Saturn" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(geographyQuiz.Id, "Which of these are continents? (Select all that apply)", QuestionType.MultipleChoiceMultiple, 2, 3,
                new string[] { "Asia", "Europe", "Greenland", "Antarctica" },
                new bool[] { true, true, false, true }),
            
            CreateQuestion(geographyQuiz.Id, "The Pacific Ocean is the largest ocean on Earth.", QuestionType.TrueFalse, 1, 4,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "What is the highest mountain in the world?", QuestionType.MultipleChoiceSingle, 1, 5,
                new string[] { "K2", "Mount Everest", "Kilimanjaro", "Mount McKinley" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(geographyQuiz.Id, "The Amazon River is longer than the Nile River.", QuestionType.TrueFalse, 1, 6,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "Which countries are in South America? (Select all that apply)", QuestionType.MultipleChoiceMultiple, 2, 7,
                new string[] { "Brazil", "Argentina", "Mexico", "Chile" },
                new bool[] { true, true, false, true }),
            
            CreateQuestion(geographyQuiz.Id, "The Sahara Desert is located in Africa.", QuestionType.TrueFalse, 1, 8,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "China has the largest population in the world.", QuestionType.TrueFalse, 1, 9,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "Russia spans 11 time zones.", QuestionType.TrueFalse, 1, 10,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(geographyQuiz.Id, "What is the largest island in the world?", QuestionType.MultipleChoiceSingle, 1, 11,
                new string[] { "Australia", "Greenland", "Madagascar", "New Guinea" },
                new bool[] { false, true, false, false })
        };

        geographyQuiz.Questions.AddRange(geographyQuestions);
        Quizzes.Add(geographyQuiz);
        Questions.AddRange(geographyQuestions);
        QuestionOptions.AddRange(geographyQuestions.SelectMany(q => q.Options));

        // Enhanced Quiz 3: Basic Mathematics (12 questions) - 1.5 minutes
        var mathQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "Basic Mathematics",
            Description = "Comprehensive test of basic mathematical concepts and calculations",
            CategoryId = 4,
            CreatedById = "user-123",
            IsPublic = true,
            TimeLimit = 2 // 1.5 minutes
        };

        var mathQuestions = new Question[]
        {
            CreateQuestion(mathQuiz.Id, "What is 15 + 27?", QuestionType.MultipleChoiceSingle, 1, 0,
                new string[] { "40", "42", "44", "45" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(mathQuiz.Id, "What is 8 × 7?", QuestionType.MultipleChoiceSingle, 1, 1,
                new string[] { "54", "56", "58", "64" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(mathQuiz.Id, "A triangle has exactly 3 sides.", QuestionType.TrueFalse, 1, 2,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(mathQuiz.Id, "What is 144 ÷ 12?", QuestionType.MultipleChoiceSingle, 1, 3,
                new string[] { "10", "12", "14", "16" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(mathQuiz.Id, "Zero is an even number.", QuestionType.TrueFalse, 1, 4,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(mathQuiz.Id, "What is 1/2 + 1/4?", QuestionType.MultipleChoiceSingle, 2, 5,
                new string[] { "2/6", "3/4", "2/4", "1/6" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(mathQuiz.Id, "The square root of 64 is 8.", QuestionType.TrueFalse, 1, 6,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(mathQuiz.Id, "Which are prime numbers? (Select all that apply)", QuestionType.MultipleChoiceMultiple, 3, 7,
                new string[] { "7", "9", "11", "15" },
                new bool[] { true, false, true, false }),
            
            CreateQuestion(mathQuiz.Id, "25% of 80 equals 20.", QuestionType.TrueFalse, 1, 8,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(mathQuiz.Id, "A right angle measures 90 degrees.", QuestionType.TrueFalse, 1, 9,
                new string[] { "True", "False" },
                new bool[] { true, false }),
            
            CreateQuestion(mathQuiz.Id, "What is the formula for the area of a circle?", QuestionType.MultipleChoiceSingle, 2, 10,
                new string[] { "πr", "πr²", "2πr", "πd" },
                new bool[] { false, true, false, false }),
            
            CreateQuestion(mathQuiz.Id, "(-5) × (-3) equals 15.", QuestionType.TrueFalse, 1, 11,
                new string[] { "True", "False" },
                new bool[] { true, false })
        };

        mathQuiz.Questions.AddRange(mathQuestions);
        Quizzes.Add(mathQuiz);
        Questions.AddRange(mathQuestions);
        QuestionOptions.AddRange(mathQuestions.SelectMany(q => q.Options));

        // Load relations
        foreach (var quiz in Quizzes)
        {
            quiz.Category = Categories.FirstOrDefault(c => c.Id == quiz.CategoryId);
            quiz.CreatedBy = Users.FirstOrDefault(u => u.Id == quiz.CreatedById);
        }
    }
    private void LoadQuizRelationships()
{

    foreach (var quiz in Quizzes)
    {
        quiz.Category = Categories.FirstOrDefault(c => c.Id == quiz.CategoryId);
        quiz.CreatedBy = Users.FirstOrDefault(u => u.Id == quiz.CreatedById);
        
        // Load questions for this quiz
        quiz.Questions = Questions.Where(q => q.QuizId == quiz.Id).OrderBy(q => q.OrderIndex).ToList();
        
        // Load options for each question
        foreach (var question in quiz.Questions)
        {
            question.Options = QuestionOptions.Where(o => o.QuestionId == question.Id).OrderBy(o => o.OrderIndex).ToList();
        }
    }
    
    Console.WriteLine($"🔗 Loaded relationships for {Quizzes.Count} quizzes");
    foreach (var quiz in Quizzes)
    {
        Console.WriteLine($"   Quiz '{quiz.Title}' has {quiz.Questions.Count} questions");
    }
}
}

// ================================
// QUIZ SERVICE
// ================================
public interface IQuizService
{
    Task<ServiceResult<List<Quiz>>> GetAvailableQuizzesAsync();
    Task<ServiceResult<Quiz>> GetQuizAsync(int quizId);
    Task<ServiceResult<QuizAttempt>> StartQuizAttemptAsync(int quizId, string userId);
    Task<ServiceResult<QuizAttempt>> SubmitQuizAnswersAsync(int attemptId, List<UserAnswer> answers);
    Task<ServiceResult<List<QuizAttempt>>> GetUserAttemptsAsync(string userId);
}

public class QuizService : IQuizService
{
    private readonly QuizDataStore _dataStore;

    public QuizService(QuizDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<ServiceResult<List<Quiz>>> GetAvailableQuizzesAsync()
    {
        var quizzes = _dataStore.Quizzes.Where(q => q.IsActive && q.IsPublic).ToList();
        return Task.FromResult(ServiceResult<List<Quiz>>.Success(quizzes));
    }

    public Task<ServiceResult<Quiz>> GetQuizAsync(int quizId)
    {
        var quiz = _dataStore.Quizzes.FirstOrDefault(q => q.Id == quizId);
        if (quiz == null)
        {
            return Task.FromResult(ServiceResult<Quiz>.Failure("Quiz not found"));
        }

        // Ensure questions and options are loaded
        if (quiz.Questions == null || quiz.Questions.Count == 0)
        {
            quiz.Questions = _dataStore.Questions.Where(q => q.QuizId == quiz.Id).OrderBy(q => q.OrderIndex).ToList();
            
            foreach (var question in quiz.Questions)
            {
                if (question.Options == null || question.Options.Count == 0)
                {
                    question.Options = _dataStore.QuestionOptions.Where(o => o.QuestionId == question.Id).OrderBy(o => o.OrderIndex).ToList();
                }
            }
        }

        return Task.FromResult(ServiceResult<Quiz>.Success(quiz));
    }

    public Task<ServiceResult<QuizAttempt>> StartQuizAttemptAsync(int quizId, string userId)
    {
        var quiz = _dataStore.Quizzes.FirstOrDefault(q => q.Id == quizId);
        if (quiz == null)
        {
            return Task.FromResult(ServiceResult<QuizAttempt>.Failure("Quiz not found"));
        }

        // Get questions directly from datastore
        var questions = _dataStore.Questions.Where(q => q.QuizId == quizId).ToList();
        if (!questions.Any())
        {
            return Task.FromResult(ServiceResult<QuizAttempt>.Failure("Quiz has no questions"));
        }

        var attempt = new QuizAttempt
        {
            Id = _dataStore.GetNextAttemptId(),
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            MaxScore = questions.Sum(q => q.Points)
        };

        _dataStore.QuizAttempts.Add(attempt);
        return Task.FromResult(ServiceResult<QuizAttempt>.Success(attempt));
    }

    public Task<ServiceResult<QuizAttempt>> SubmitQuizAnswersAsync(int attemptId, List<UserAnswer> answers)
    {
        var attempt = _dataStore.QuizAttempts.FirstOrDefault(a => a.Id == attemptId);
        if (attempt == null)
        {
            return Task.FromResult(ServiceResult<QuizAttempt>.Failure("Quiz attempt not found"));
        }

        var questions = _dataStore.Questions.Where(q => q.QuizId == attempt.QuizId).ToList();
        int totalScore = 0;

        foreach (var answer in answers)
        {
            answer.Id = _dataStore.GetNextAnswerId();
            answer.QuizAttemptId = attemptId;

            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null)
            {
                bool isCorrect = CheckAnswer(answer, question);
                answer.IsCorrect = isCorrect;
                answer.PointsAwarded = isCorrect ? question.Points : 0;
                totalScore += answer.PointsAwarded;
            }

            _dataStore.UserAnswers.Add(answer);
        }

        attempt.Score = totalScore;
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.UserAnswers = answers;
        _dataStore.SaveData();

        return Task.FromResult(ServiceResult<QuizAttempt>.Success(attempt));
    }

    public Task<ServiceResult<List<QuizAttempt>>> GetUserAttemptsAsync(string userId)
    {
        var attempts = _dataStore.QuizAttempts
            .Where(a => a.UserId == userId && a.IsCompleted)
            .OrderByDescending(a => a.CompletedAt)
            .ToList();

        // Load quiz titles
        foreach (var attempt in attempts)
        {
            var quiz = _dataStore.Quizzes.FirstOrDefault(q => q.Id == attempt.QuizId);
            if (quiz != null)
            {
                attempt.Quiz = quiz;
            }
        }

        return Task.FromResult(ServiceResult<List<QuizAttempt>>.Success(attempts));
    }

    private bool CheckAnswer(UserAnswer answer, Question question)
    {
        var correctOptionIds = _dataStore.QuestionOptions
            .Where(o => o.QuestionId == question.Id && o.IsCorrect)
            .Select(o => o.Id.ToString())
            .ToList();

        switch (question.QuestionType)
        {
            case QuestionType.MultipleChoiceSingle:
            case QuestionType.TrueFalse:
                return correctOptionIds.Contains(answer.SelectedOptionIds);

            case QuestionType.MultipleChoiceMultiple:
                var selectedIds = answer.SelectedOptionIds?.Split(',').ToList() ?? new List<string>();
                return correctOptionIds.All(selectedIds.Contains) && selectedIds.All(correctOptionIds.Contains);

            case QuestionType.ShortAnswer:
                return !string.IsNullOrEmpty(answer.TextAnswer);

            default:
                return false;
        }
    }
}

// ================================
// AUTHENTICATION SERVICE
// ================================
public interface IAuthenticationService
{
    Task<ServiceResult<ApplicationUser>> LoginAsync(string email, string password);
    Task<ServiceResult<ApplicationUser>> RegisterAsync(RegisterUserDto registerDto);
    bool IsInRole(ApplicationUser user, string role);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly QuizDataStore _dataStore;

    public AuthenticationService(QuizDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<ServiceResult<ApplicationUser>> RegisterAsync(RegisterUserDto registerDto)
    {
        var validationErrors = ValidationHelper.ValidateRegistration(registerDto);
        if (validationErrors.Any())
        {
            return Task.FromResult(ServiceResult<ApplicationUser>.Failure(validationErrors));
        }

        var existingUser = _dataStore.Users.FirstOrDefault(u => 
            u.Email.Equals(registerDto.Email, StringComparison.OrdinalIgnoreCase));
        
        if (existingUser != null)
        {
            return Task.FromResult(ServiceResult<ApplicationUser>.Failure("Email address is already registered"));
        }

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = registerDto.Email,
            Email = registerDto.Email.ToLowerInvariant(),
            PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _dataStore.Users.Add(newUser);
        _dataStore.SaveData();

        return Task.FromResult(ServiceResult<ApplicationUser>.Success(newUser));
    }

public Task<ServiceResult<ApplicationUser>> LoginAsync(string email, string password)
{
    var user = _dataStore.Users.FirstOrDefault(u => 
        u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    
    if (user == null)
    {
        return Task.FromResult(ServiceResult<ApplicationUser>.Failure("Invalid email or password"));
    }

    if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
    {
        return Task.FromResult(ServiceResult<ApplicationUser>.Failure("Invalid email or password"));
    }

    return Task.FromResult(ServiceResult<ApplicationUser>.Success(user));
}

    public bool IsInRole(ApplicationUser user, string role)
    {
        return user.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
    }
}



// ================================
// COMPLETE InteractiveQuizApplication CLASS
// Replace your ENTIRE InteractiveQuizApplication class with this
// ================================

public class InteractiveQuizApplication
{
    private readonly IQuizService _quizService;
    private readonly IAuthenticationService _authService;
    private ApplicationUser _currentUser;

    public InteractiveQuizApplication()
    {
        var dataStore = QuizDataStore.Instance;
        _quizService = new QuizService(dataStore);
        _authService = new AuthenticationService(dataStore);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🎯 Welcome to QuizMaker - Interactive Quiz Platform!");
        Console.WriteLine("===================================================\n");

        await LoginUser();

        if (_currentUser != null)
        {
            await ShowMainMenu();
        }
    }

    private async Task LoginUser()
    {
        while (_currentUser == null)
        {
            Console.WriteLine("🔐 WELCOME TO QUIZMAKER");
            Console.WriteLine("========================");
            Console.WriteLine("1. 🔑 Login to existing account");
            Console.WriteLine("2. 📝 Create new account");
            Console.WriteLine("3. 🧪 Use demo accounts");
            Console.WriteLine("0. 🚪 Exit");
            Console.Write("\nSelect an option: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await HandleLogin();
                    break;
                case "2":
                    await HandleRegistration();
                    break;
                case "3":
                    await ShowDemoAccounts();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("❌ Invalid option. Please try again.\n");
                    break;
            }
        }
    }

    private async Task HandleLogin()
    {
        Console.WriteLine("\n🔑 LOGIN");
        Console.WriteLine("=========");
        
        Console.Write("Email: ");
        string email = Console.ReadLine();
        
        Console.Write("Password: ");
        string password = ReadPassword();
        
        var loginDto = new LoginDto { Email = email, Password = password };
        var validationErrors = ValidationHelper.ValidateLogin(loginDto);
        
        if (validationErrors.Any())
        {
            Console.WriteLine("\n❌ Validation errors:");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($"   • {error}");
            }
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
            return;
        }

        var loginResult = await _authService.LoginAsync(email, password);
        if (loginResult.IsSuccess)
        {
            _currentUser = loginResult.Data;
            Console.WriteLine($"\n✅ Login successful!");
            Console.WriteLine($"👋 Welcome back, {_currentUser.FirstName}!");
            Console.WriteLine($"🔑 Role: {_currentUser.Role}");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"\n❌ {loginResult.ErrorMessage}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    private async Task HandleRegistration()
    {
        Console.WriteLine("\n📝 CREATE NEW ACCOUNT");
        Console.WriteLine("======================");
        
        var registerDto = new RegisterUserDto();
        
        Console.Write("First Name: ");
        registerDto.FirstName = Console.ReadLine();
        
        Console.Write("Last Name: ");
        registerDto.LastName = Console.ReadLine();
        
        Console.Write("Email: ");
        registerDto.Email = Console.ReadLine();
        
        Console.Write("Password (min 6 chars, include number and letter): ");
        registerDto.Password = ReadPassword();
        
        Console.Write("Confirm Password: ");
        registerDto.ConfirmPassword = ReadPassword();

        var registerResult = await _authService.RegisterAsync(registerDto);
        if (registerResult.IsSuccess)
        {
            Console.WriteLine($"\n🎉 Registration successful!");
            Console.WriteLine($"Welcome to QuizMaker, {registerResult.Data.FirstName}!");
            Console.WriteLine("You can now login with your credentials.");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"\n❌ Registration failed:");
            if (registerResult.Errors.Any())
            {
                foreach (var error in registerResult.Errors)
                {
                    Console.WriteLine($"   • {error}");
                }
            }
            else
            {
                Console.WriteLine($"   • {registerResult.ErrorMessage}");
            }
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    private async Task ShowDemoAccounts()
    {
        Console.WriteLine("\n🧪 DEMO ACCOUNTS");
        Console.WriteLine("=================");
        Console.WriteLine("1. admin@quizmaker.com (Password: admin123) - Admin");
        Console.WriteLine("2. john@example.com (Password: user123) - User");
        Console.WriteLine("0. Back to main menu");
        Console.Write("\nSelect demo account: ");

        string choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                var adminResult = await _authService.LoginAsync("admin@quizmaker.com", "admin123");
                if (adminResult.IsSuccess)
                {
                    _currentUser = adminResult.Data;
                    Console.WriteLine($"\n✅ Logged in as Admin!");
                }
                break;
            case "2":
                var userResult = await _authService.LoginAsync("john@example.com", "user123");
                if (userResult.IsSuccess)
                {
                    _currentUser = userResult.Data;
                    Console.WriteLine($"\n✅ Logged in as Regular User!");
                }
                break;
            case "0":
                return;
            default:
                Console.WriteLine("❌ Invalid option.");
                break;
        }
        
        if (_currentUser != null)
        {
            Console.WriteLine($"👋 Welcome, {_currentUser.FirstName}!");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    private string ReadPassword()
    {
        string password = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        return password;
    }

    private async Task ShowMainMenu()
{
    while (true)
    {
        Console.WriteLine("\n🎯 MAIN MENU");
        Console.WriteLine("=============");
        Console.WriteLine("1. 📝 View Available Quizzes");
        Console.WriteLine("2. 🎮 Take a Quiz");
        Console.WriteLine("3. 📊 View My Quiz History");
        if (_authService.IsInRole(_currentUser, "Admin"))
        {
            Console.WriteLine("4. 🔧 Admin Panel");
            Console.WriteLine("5. 📈 View All Quiz Results (Admin)");
            Console.WriteLine("6. 🎯 Create New Quiz (Admin)");
        }
        Console.WriteLine("0. 🚪 Exit");
        Console.Write("\nSelect an option: ");
        
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await ViewAvailableQuizzes();
                break;
            case "2":
                await TakeQuizInteractive();
                break;
            case "3":
                await ViewQuizHistory();
                break;
            case "4":
                if (_authService.IsInRole(_currentUser, "Admin"))
                    ShowAdminPanel();
                else
                    Console.WriteLine("❌ Access denied.");
                break;
            case "5":
                if (_authService.IsInRole(_currentUser, "Admin"))
                    await ViewAllQuizResults();
                else
                    Console.WriteLine("❌ Access denied.");
                break;
            case "6":
                if (_authService.IsInRole(_currentUser, "Admin"))
                    await CreateNewQuiz();
                else
                    Console.WriteLine("❌ Access denied.");
                break;
            case "0":
                Console.WriteLine("👋 Thanks for using QuizMaker!");
                return;
            default:
                Console.WriteLine("❌ Invalid option. Please try again.");
                break;
        }
    }
}
    private async Task ViewAvailableQuizzes()
    {
        Console.WriteLine("\n📝 AVAILABLE QUIZZES");
        Console.WriteLine("====================");

        var result = await _quizService.GetAvailableQuizzesAsync();
        if (result.IsSuccess && result.Data.Any())
        {
            foreach (var quiz in result.Data)
            {
                Console.WriteLine($"🎯 Quiz ID: {quiz.Id}");
                Console.WriteLine($"   Title: {quiz.Title}");
                Console.WriteLine($"   Description: {quiz.Description}");
                Console.WriteLine($"   Category: {quiz.Category?.Name}");
                Console.WriteLine($"   Questions: {quiz.Questions.Count}");
                Console.WriteLine($"   Time Limit: {(quiz.TimeLimit > 0 ? quiz.TimeLimit + " minutes" : "No limit")}");
                Console.WriteLine($"   Created by: {quiz.CreatedBy?.FirstName} {quiz.CreatedBy?.LastName}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("❌ No quizzes available.");
        }
    }

    private async Task TakeQuizInteractive()
    {
        Console.WriteLine("\n🎮 TAKE A QUIZ");
        Console.WriteLine("===============");

        await ViewAvailableQuizzes();
        
        Console.Write("Enter Quiz ID to take: ");
        if (int.TryParse(Console.ReadLine(), out int quizId))
        {
            await StartQuizSession(quizId);
        }
        else
        {
            Console.WriteLine("❌ Invalid Quiz ID.");
        }
    }

    private async Task StartQuizSession(int quizId)
    {
        var quizResult = await _quizService.GetQuizAsync(quizId);
        if (!quizResult.IsSuccess)
        {
            Console.WriteLine($"❌ {quizResult.ErrorMessage}");
            return;
        }

        var quiz = quizResult.Data;
        var timeInSeconds = quiz.TimeLimit * 60;
        
        Console.WriteLine($"\n🎯 Starting Quiz: {quiz.Title}");
        Console.WriteLine($"📝 Description: {quiz.Description}");
        Console.WriteLine($"⏱️ Time Limit: {quiz.TimeLimit} minute(s) ({timeInSeconds} seconds)");
        Console.WriteLine($"📊 Total Questions: {quiz.Questions.Count}");
        Console.WriteLine($"🎯 Max Score: {quiz.Questions.Sum(q => q.Points)} points");
        Console.WriteLine($"⚠️ WARNING: Quiz will auto-submit when time runs out!");
        Console.WriteLine();

        Console.Write("Press Enter to start the quiz...");
        Console.ReadLine();

        var attemptResult = await _quizService.StartQuizAttemptAsync(quizId, _currentUser.Id);
        if (!attemptResult.IsSuccess)
        {
            Console.WriteLine($"❌ {attemptResult.ErrorMessage}");
            return;
        }

        var attempt = attemptResult.Data;
        var userAnswers = new List<UserAnswer>();
        DateTime startTime = DateTime.Now;
        bool timeExpired = false;

        foreach (var question in quiz.Questions)
        {
            var elapsed = DateTime.Now - startTime;
            var remaining = TimeSpan.FromSeconds(timeInSeconds) - elapsed;
            
            if (remaining.TotalSeconds <= 0)
            {
                timeExpired = true;
                break;
            }

            Console.Clear();
            Console.WriteLine($"🎯 Quiz: {quiz.Title}");
            Console.WriteLine($"Question {question.OrderIndex + 1} of {quiz.Questions.Count}");
            Console.WriteLine($"Points: {question.Points}");
            Console.WriteLine($"⏱️ Time remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}");
            Console.WriteLine("=" + new string('=', 50));
            Console.WriteLine();
            Console.WriteLine($"❓ {question.QuestionText}");
            Console.WriteLine();

            var answer = new UserAnswer
            {
                QuestionId = question.Id,
                QuizAttemptId = attempt.Id
            };

            switch (question.QuestionType)
            {
                case QuestionType.MultipleChoiceSingle:
                case QuestionType.TrueFalse:
                    Console.WriteLine("Select one option:");
                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {question.Options[i].OptionText}");
                    }
                    Console.Write("\nYour answer (enter number): ");
                    
                    if (int.TryParse(Console.ReadLine(), out int selectedOption) && 
                        selectedOption > 0 && selectedOption <= question.Options.Count)
                    {
                        answer.SelectedOptionIds = question.Options[selectedOption - 1].Id.ToString();
                    }
                    
                    elapsed = DateTime.Now - startTime;
                    remaining = TimeSpan.FromSeconds(timeInSeconds) - elapsed;
                    if (remaining.TotalSeconds <= 0)
                    {
                        timeExpired = true;
                        Console.WriteLine("\n⏰ TIME'S UP! Quiz auto-submitting...");
                        break;
                    }
                    break;

                case QuestionType.MultipleChoiceMultiple:
                    Console.WriteLine("Select all correct options (enter numbers separated by commas):");
                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {question.Options[i].OptionText}");
                    }
                    Console.Write("\nYour answers (e.g., 1,3,4): ");
                    
                    string multipleChoice = Console.ReadLine();
                    
                    elapsed = DateTime.Now - startTime;
                    remaining = TimeSpan.FromSeconds(timeInSeconds) - elapsed;
                    if (remaining.TotalSeconds <= 0)
                    {
                        timeExpired = true;
                        Console.WriteLine("\n⏰ TIME'S UP! Quiz auto-submitting...");
                        break;
                    }
                    
                    if (!string.IsNullOrEmpty(multipleChoice))
                    {
                        var selectedNumbers = multipleChoice.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => int.TryParse(s, out int num) && num > 0 && num <= question.Options.Count)
                            .Select(s => int.Parse(s))
                            .ToList();
                        
                        if (selectedNumbers.Any())
                        {
                            var selectedOptionIds = selectedNumbers
                                .Select(num => question.Options[num - 1].Id.ToString())
                                .ToList();
                            answer.SelectedOptionIds = string.Join(",", selectedOptionIds);
                        }
                    }
                    break;

                case QuestionType.ShortAnswer:
                    Console.Write("Enter your answer: ");
                    answer.TextAnswer = Console.ReadLine();
                    
                    elapsed = DateTime.Now - startTime;
                    remaining = TimeSpan.FromSeconds(timeInSeconds) - elapsed;
                    if (remaining.TotalSeconds <= 0)
                    {
                        timeExpired = true;
                        Console.WriteLine("\n⏰ TIME'S UP! Quiz auto-submitting...");
                        break;
                    }
                    break;
            }

            userAnswers.Add(answer);

            if (timeExpired) break;

            elapsed = DateTime.Now - startTime;
            remaining = TimeSpan.FromSeconds(timeInSeconds) - elapsed;
            
            if (question.OrderIndex < quiz.Questions.Count - 1 && remaining.TotalSeconds > 5)
            {
                Console.WriteLine($"\n⏱️ Time remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}");
                Console.WriteLine("Press Enter to continue to next question...");
                Console.ReadLine();
            }
            else if (question.OrderIndex == quiz.Questions.Count - 1)
            {
                Console.WriteLine("\nQuiz completed! Processing results...");
                System.Threading.Thread.Sleep(1000);
            }
        }

        if (timeExpired)
        {
            Console.Clear();
            Console.WriteLine("⏰ TIME EXPIRED!");
            Console.WriteLine("================");
            Console.WriteLine($"🚨 Sorry! You ran out of time for quiz: {quiz.Title}");
            Console.WriteLine($"⏱️ Time limit was: {quiz.TimeLimit} minute(s)");
            Console.WriteLine($"📝 Questions answered: {userAnswers.Count} out of {quiz.Questions.Count}");
            Console.WriteLine();
            Console.WriteLine("💡 TIP: Try to answer questions more quickly next time!");
            Console.WriteLine("🔄 You can retake this quiz anytime to improve your score.");
            Console.WriteLine();
            Console.Write("Press Enter to return to main menu...");
            Console.ReadLine();
            return;
        }

        var submitResult = await _quizService.SubmitQuizAnswersAsync(attempt.Id, userAnswers);
        if (submitResult.IsSuccess)
        {
            ShowQuizResults(submitResult.Data, quiz);
        }
        else
        {
            Console.WriteLine($"❌ Error submitting quiz: {submitResult.ErrorMessage}");
        }
    }

    private void ShowQuizResults(QuizAttempt attempt, Quiz quiz)
    {
        Console.Clear();
        Console.WriteLine("🎉 QUIZ COMPLETED!");
        Console.WriteLine("===================");
        Console.WriteLine($"📝 Quiz: {quiz.Title}");
        Console.WriteLine($"⏱️ Completed: {attempt.CompletedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"🎯 Your Score: {attempt.Score} / {attempt.MaxScore} points");
        Console.WriteLine($"📊 Percentage: {attempt.Percentage:F1}%");
        
        string rating = attempt.Percentage >= 90 ? "🌟 Excellent!" :
                       attempt.Percentage >= 70 ? "👍 Good!" :
                       attempt.Percentage >= 50 ? "👌 Fair" : "📚 Keep practicing!";
        Console.WriteLine($"🏆 Rating: {rating}");

        Console.WriteLine("\n📋 DETAILED RESULTS:");
        Console.WriteLine("=====================");

        var questions = quiz.Questions.OrderBy(q => q.OrderIndex).ToList();
        for (int i = 0; i < questions.Count && i < attempt.UserAnswers.Count; i++)
        {
            var question = questions[i];
            var userAnswer = attempt.UserAnswers[i];
            
            Console.WriteLine($"\nQuestion {i + 1}: {question.QuestionText}");
            Console.WriteLine($"Points: {userAnswer.PointsAwarded}/{question.Points}");
            Console.WriteLine($"Result: {(userAnswer.IsCorrect ? "✅ Correct" : "❌ Incorrect")}");
            
            if (question.QuestionType == QuestionType.ShortAnswer)
            {
                Console.WriteLine($"Your Answer: {userAnswer.TextAnswer}");
            }
            else
            {
                var selectedIds = userAnswer.SelectedOptionIds?.Split(',').ToList() ?? new List<string>();
                
                Console.WriteLine("Options:");
                foreach (var option in question.Options)
                {
                    string marker = "";
                    if (selectedIds.Contains(option.Id.ToString()) && option.IsCorrect)
                        marker = "✅";
                    else if (selectedIds.Contains(option.Id.ToString()) && !option.IsCorrect)
                        marker = "❌";
                    else if (!selectedIds.Contains(option.Id.ToString()) && option.IsCorrect)
                        marker = "🔵";
                    else
                        marker = "⚪";
                    
                    Console.WriteLine($"  {marker} {option.OptionText}");
                }
            }
        }

        Console.WriteLine("\nPress Enter to return to main menu...");
        Console.ReadLine();
    }

    private async Task ViewQuizHistory()
    {
        Console.WriteLine("\n📊 YOUR QUIZ HISTORY");
        Console.WriteLine("=====================");

        var result = await _quizService.GetUserAttemptsAsync(_currentUser.Id);
        if (result.IsSuccess && result.Data.Any())
        {
            Console.WriteLine($"Total quizzes taken: {result.Data.Count}\n");
            
            foreach (var attempt in result.Data.Take(10))
            {
                Console.WriteLine($"🎯 Quiz: {attempt.Quiz?.Title ?? "Unknown Quiz"}");
                Console.WriteLine($"📅 Date: {attempt.CompletedAt:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"🎯 Score: {attempt.Score}/{attempt.MaxScore} ({attempt.Percentage:F1}%)");
                
                string performance = attempt.Percentage >= 90 ? "🌟 Excellent" :
                                   attempt.Percentage >= 70 ? "👍 Good" :
                                   attempt.Percentage >= 50 ? "👌 Fair" : "📚 Needs Improvement";
                Console.WriteLine($"🏆 Performance: {performance}");
                Console.WriteLine();
            }

            var avgScore = result.Data.Average(a => a.Percentage);
            var bestScore = result.Data.Max(a => a.Percentage);
            var totalPoints = result.Data.Sum(a => a.Score);
            
            Console.WriteLine("📈 STATISTICS:");
            Console.WriteLine($"   Average Score: {avgScore:F1}%");
            Console.WriteLine($"   Best Score: {bestScore:F1}%");
            Console.WriteLine($"   Total Points Earned: {totalPoints}");
        }
        else
        {
            Console.WriteLine("📭 No quiz history found. Take some quizzes to see your progress!");
        }
    }

    private void ShowAdminPanel()
    {
        Console.WriteLine("\n🔧 ADMIN PANEL");
        Console.WriteLine("===============");
        Console.WriteLine("👥 Admin Features Available:");
        Console.WriteLine("   • User Management");
        Console.WriteLine("   • Quiz Management");
        Console.WriteLine("   • Category Management");
        Console.WriteLine("   • System Analytics");
        Console.WriteLine("   • Quiz Creation Tools");
        Console.WriteLine("   • Performance Reports");
        Console.WriteLine("   • Quiz Creation & Management ⭐ ");
        Console.WriteLine("\n✅ Admin privileges verified!");
        Console.WriteLine("🔑 Full system access granted.");
        
        var dataStore = QuizDataStore.Instance;
        Console.WriteLine("\n📊 SYSTEM STATISTICS:");
        Console.WriteLine($"   Total Users: {dataStore.Users.Count}");
        Console.WriteLine($"   Total Quizzes: {dataStore.Quizzes.Count}");
        Console.WriteLine($"   Total Questions: {dataStore.Questions.Count}");
        Console.WriteLine($"   Total Quiz Attempts: {dataStore.QuizAttempts.Count}");
        Console.WriteLine($"   Active Categories: {dataStore.Categories.Count(c => c.IsActive)}");
    }

    private Task ViewAllQuizResults()
    {
        Console.WriteLine("\n📈 ALL QUIZ RESULTS (ADMIN VIEW)");
        Console.WriteLine("==================================");

        var dataStore = QuizDataStore.Instance;
        var allAttempts = dataStore.QuizAttempts
            .Where(a => a.IsCompleted)
            .OrderByDescending(a => a.CompletedAt)
            .ToList();

        if (!allAttempts.Any())
        {
            Console.WriteLine("📭 No quiz attempts found.");
            return Task.CompletedTask;
        }

        Console.WriteLine($"Total completed quiz attempts: {allAttempts.Count}\n");

        var attemptsByQuiz = allAttempts.GroupBy(a => a.QuizId).ToList();

        foreach (var quizGroup in attemptsByQuiz)
        {
            var quiz = dataStore.Quizzes.FirstOrDefault(q => q.Id == quizGroup.Key);
            var attempts = quizGroup.ToList();

            Console.WriteLine($"🎯 QUIZ: {quiz?.Title ?? "Unknown Quiz"}");
            Console.WriteLine($"   Total Attempts: {attempts.Count}");
            Console.WriteLine($"   Average Score: {attempts.Average(a => a.Percentage):F1}%");
            Console.WriteLine($"   Highest Score: {attempts.Max(a => a.Percentage):F1}%");
            Console.WriteLine($"   Lowest Score: {attempts.Min(a => a.Percentage):F1}%");
            Console.WriteLine();

            Console.WriteLine("   Recent Attempts:");
            foreach (var attempt in attempts.Take(5))
            {
                var user = dataStore.Users.FirstOrDefault(u => u.Id == attempt.UserId);
                Console.WriteLine($"   • {user?.FirstName ?? "Unknown"} {user?.LastName ?? "User"}: {attempt.Score}/{attempt.MaxScore} ({attempt.Percentage:F1}%) on {attempt.CompletedAt:MM/dd HH:mm}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("📊 OVERALL STATISTICS:");
        Console.WriteLine($"   Average Score Across All Quizzes: {allAttempts.Average(a => a.Percentage):F1}%");
        Console.WriteLine($"   Total Points Awarded: {allAttempts.Sum(a => a.Score)}");
        Console.WriteLine($"   Most Active User: {GetMostActiveUser(dataStore)}");
        Console.WriteLine($"   Most Popular Quiz: {GetMostPopularQuiz(dataStore)}");
        
        return Task.CompletedTask;
    }

    private string GetMostActiveUser(QuizDataStore dataStore)
    {
        var userAttempts = dataStore.QuizAttempts
            .Where(a => a.IsCompleted)
            .GroupBy(a => a.UserId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
            
        if (userAttempts == null) 
            return "None";
            
        var user = dataStore.Users.FirstOrDefault(u => u.Id == userAttempts.Key);
        var firstName = user?.FirstName ?? "Unknown";
        var lastName = user?.LastName ?? "User";
        var count = userAttempts.Count();
        
        return firstName + " " + lastName + " (" + count + " attempts)";
    }

    private string GetMostPopularQuiz(QuizDataStore dataStore)
    {
        var quizAttempts = dataStore.QuizAttempts
            .Where(a => a.IsCompleted)
            .GroupBy(a => a.QuizId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
            
        if (quizAttempts == null) 
            return "None";
            
        var quiz = dataStore.Quizzes.FirstOrDefault(q => q.Id == quizAttempts.Key);
        var title = quiz?.Title ?? "Unknown Quiz";
        var count = quizAttempts.Count();
        
        return title + " (" + count + " attempts)";
    }
    
    private Task CreateNewQuiz()
    {
        Console.WriteLine("\n🎯 CREATE NEW QUIZ (ADMIN ONLY)");
        Console.WriteLine("================================");
        
        var dataStore = QuizDataStore.Instance;
        
        // Display available categories
        Console.WriteLine("📂 Available Categories:");
        foreach (var category in dataStore.Categories.Where(c => c.IsActive))
        {
            Console.WriteLine($"   {category.Id}. {category.Name} - {category.Description}");
        }
        Console.WriteLine();
        
        // Get quiz basic info
        Console.Write("Enter Quiz Title: ");
        string title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("❌ Quiz title is required!");
            return Task.CompletedTask;
        }
        
        Console.Write("Enter Quiz Description: ");
        string description = Console.ReadLine();
        
        Console.Write("Enter Category ID: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId) || 
            !dataStore.Categories.Any(c => c.Id == categoryId && c.IsActive))
        {
            Console.WriteLine("❌ Invalid category ID!");
            return Task.CompletedTask;
        }
        
        Console.Write("Enter Time Limit (minutes, 0 for no limit): ");
        if (!int.TryParse(Console.ReadLine(), out int timeLimit) || timeLimit < 0)
        {
            timeLimit = 0;
        }
        
        Console.Write("Make quiz public? (y/n): ");
        bool isPublic = Console.ReadLine()?.ToLower().StartsWith("y") == true;
        
        // Create the quiz
        var newQuiz = new Quiz
        {
            Id = dataStore.GetNextQuizId(),
            Title = title,
            Description = description,
            CategoryId = categoryId,
            CreatedById = _currentUser.Id,
            IsPublic = isPublic,
            TimeLimit = timeLimit,
            CreatedAt = DateTime.UtcNow
        };
        
        Console.WriteLine($"\n📝 Creating quiz: {title}");
        Console.WriteLine("===========================");
        
        var questions = new List<Question>();
        int questionCount = 1;
        
        while (true)
        {
            Console.WriteLine($"\n❓ QUESTION {questionCount}");
            Console.WriteLine("=================");
            
            Console.Write("Enter question text (or 'done' to finish): ");
            string questionText = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(questionText) || questionText.ToLower() == "done")
            {
                if (questions.Count == 0)
                {
                    Console.WriteLine("❌ Quiz must have at least one question!");
                    continue;
                }
                break;
            }
            
            Console.WriteLine("\nQuestion Types:");
            Console.WriteLine("1. Multiple Choice (Single Answer)");
            Console.WriteLine("2. Multiple Choice (Multiple Answers)");
            Console.WriteLine("3. True/False");
            Console.WriteLine("4. Short Answer");
            Console.Write("Select question type (1-4): ");
            
            if (!int.TryParse(Console.ReadLine(), out int typeChoice) || typeChoice < 1 || typeChoice > 4)
            {
                Console.WriteLine("❌ Invalid question type!");
                continue;
            }
            
            var questionType = (QuestionType)typeChoice;
            
            Console.Write("Enter points for this question (default 1): ");
            if (!int.TryParse(Console.ReadLine(), out int points) || points < 1)
            {
                points = 1;
            }
            
            var question = new Question
            {
                Id = dataStore.GetNextQuestionId(),
                QuizId = newQuiz.Id,
                QuestionText = questionText,
                QuestionType = questionType,
                Points = points,
                OrderIndex = questionCount - 1
            };
            
            // Add options based on question type
            var options = new List<QuestionOption>();
            
            switch (questionType)
            {
                case QuestionType.TrueFalse:
                    options.Add(new QuestionOption
                    {
                        Id = dataStore.GetNextOptionId(),
                        QuestionId = question.Id,
                        OptionText = "True",
                        OrderIndex = 0
                    });
                    options.Add(new QuestionOption
                    {
                        Id = dataStore.GetNextOptionId(),
                        QuestionId = question.Id,
                        OptionText = "False",
                        OrderIndex = 1
                    });
                    
                    Console.Write("Is the correct answer True or False? (t/f): ");
                    string tfAnswer = Console.ReadLine()?.ToLower();
                    options[0].IsCorrect = tfAnswer?.StartsWith("t") == true;
                    options[1].IsCorrect = tfAnswer?.StartsWith("f") == true;
                    break;
                    
                case QuestionType.MultipleChoiceSingle:
                case QuestionType.MultipleChoiceMultiple:
                    Console.Write("How many options? (2-6): ");
                    if (!int.TryParse(Console.ReadLine(), out int optionCount) || optionCount < 2 || optionCount > 6)
                    {
                        optionCount = 4;
                    }
                    
                    for (int i = 0; i < optionCount; i++)
                    {
                        Console.Write($"Enter option {i + 1}: ");
                        string optionText = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(optionText))
                        {
                            optionText = $"Option {i + 1}";
                        }
                        
                        options.Add(new QuestionOption
                        {
                            Id = dataStore.GetNextOptionId(),
                            QuestionId = question.Id,
                            OptionText = optionText,
                            OrderIndex = i
                        });
                    }
                    
                    if (questionType == QuestionType.MultipleChoiceSingle)
                    {
                        Console.Write("Which option is correct? (enter number): ");
                        if (int.TryParse(Console.ReadLine(), out int correctOption) && 
                            correctOption > 0 && correctOption <= options.Count)
                        {
                            options[correctOption - 1].IsCorrect = true;
                        }
                    }
                    else // Multiple choice multiple
                    {
                        Console.Write("Which options are correct? (enter numbers separated by commas): ");
                        string correctAnswers = Console.ReadLine();
                        if (!string.IsNullOrEmpty(correctAnswers))
                        {
                            var correctNumbers = correctAnswers.Split(',')
                                .Select(s => s.Trim())
                                .Where(s => int.TryParse(s, out int num) && num > 0 && num <= options.Count)
                                .Select(s => int.Parse(s))
                                .ToList();
                            
                            foreach (int num in correctNumbers)
                            {
                                options[num - 1].IsCorrect = true;
                            }
                        }
                    }
                    break;
                    
                case QuestionType.ShortAnswer:
                    Console.WriteLine("ℹ️ Short answer question created. Answers will be manually reviewed.");
                    break;
            }
            
            question.Options = options;
            questions.Add(question);
            
            Console.WriteLine($"✅ Question {questionCount} added successfully!");
            questionCount++;
            
            if (questionCount > 10)
            {
                Console.WriteLine("⚠️ You've added 10 questions. Consider finishing the quiz.");
            }
            
            Console.Write("Add another question? (y/n): ");
            if (Console.ReadLine()?.ToLower().StartsWith("n") == true)
            {
                break;
            }
        }
        
        if (questions.Count == 0)
        {
            Console.WriteLine("❌ Quiz creation cancelled - no questions added.");
            return Task.CompletedTask;
        }
        
        // Save the quiz and questions to datastore
        newQuiz.Questions = questions;
        dataStore.Quizzes.Add(newQuiz);
        dataStore.Questions.AddRange(questions);
        
        foreach (var question in questions)
        {
            dataStore.QuestionOptions.AddRange(question.Options);
        }
        
        // Load relationships
        newQuiz.Category = dataStore.Categories.FirstOrDefault(c => c.Id == newQuiz.CategoryId);
        newQuiz.CreatedBy = _currentUser;
        
        // Save to file
        dataStore.SaveData();
        
        Console.WriteLine("\n🎉 QUIZ CREATED SUCCESSFULLY!");
        Console.WriteLine("==============================");
        Console.WriteLine($"📝 Title: {newQuiz.Title}");
        Console.WriteLine($"📂 Category: {newQuiz.Category?.Name}");
        Console.WriteLine($"❓ Questions: {questions.Count}");
        Console.WriteLine($"🎯 Total Points: {questions.Sum(q => q.Points)}");
        Console.WriteLine($"⏱️ Time Limit: {(timeLimit > 0 ? timeLimit + " minutes" : "No limit")}");
        Console.WriteLine($"🌍 Public: {(isPublic ? "Yes" : "No")}");
        Console.WriteLine($"🆔 Quiz ID: {newQuiz.Id}");
        Console.WriteLine("\n✅ The quiz is now available for users to take!");
        
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
        
        return Task.CompletedTask;
    }
    
}

// ================================
// UNIT TESTS FOR BUSINESS LOGIC
// ================================
// ================================
// COMPLETE InteractiveQuizApplication CLASS
// Replace your entire InteractiveQuizApplication class with this
// ================================

public class QuizServiceTests
{
    private readonly IQuizService _quizService;
    private readonly QuizDataStore _dataStore;

    public QuizServiceTests()
    {
        _dataStore = QuizDataStore.Instance;
        _quizService = new QuizService(_dataStore);
    }
    
    public void RunAllTests()
    {
        Console.WriteLine("\n🧪 RUNNING UNIT TESTS");
        Console.WriteLine("======================");

        TestGetAvailableQuizzes();
        TestGetQuizById();
        TestStartQuizAttempt();
        TestSubmitQuizAnswers();
        TestGetUserAttempts();

        Console.WriteLine("✅ All unit tests completed!");
    }

    private void TestGetAvailableQuizzes()
    {
        Console.WriteLine("🧪 Test: GetAvailableQuizzesAsync");
        
        var result = _quizService.GetAvailableQuizzesAsync().Result;
        
        if (result.IsSuccess && result.Data.Count > 0)
        {
            Console.WriteLine($"   ✅ PASSED: Retrieved {result.Data.Count} quizzes");
        }
        else
        {
            Console.WriteLine("   ❌ FAILED: Should retrieve available quizzes");
        }
    }

    private void TestGetQuizById()
    {
        Console.WriteLine("🧪 Test: GetQuizAsync");
        
        try
        {
            // Get the first available quiz ID from the datastore
            var firstQuiz = _dataStore.Quizzes.FirstOrDefault();
            if (firstQuiz == null)
            {
                Console.WriteLine("   ❌ FAILED: No quizzes found in datastore");
                return;
            }

            var result = _quizService.GetQuizAsync(firstQuiz.Id).Result;
            
            if (result.IsSuccess && result.Data != null)
            {
                // Check if questions are loaded
                if (result.Data.Questions != null && result.Data.Questions.Any())
                {
                    Console.WriteLine($"   ✅ PASSED: Retrieved quiz '{result.Data.Title}' with {result.Data.Questions.Count} questions");
                }
                else
                {
                    Console.WriteLine($"   ⚠️ PARTIAL: Retrieved quiz '{result.Data.Title}' but no questions loaded");
                    Console.WriteLine($"      Quiz ID: {result.Data.Id}, Questions in datastore: {_dataStore.Questions.Count(q => q.QuizId == result.Data.Id)}");
                }
            }
            else
            {
                Console.WriteLine($"   ❌ FAILED: Could not retrieve quiz. Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ FAILED: Exception occurred: {ex.Message}");
        }
    }

    private void TestStartQuizAttempt()
    {
        Console.WriteLine("🧪 Test: StartQuizAttemptAsync");
        
        try
        {
            // Get the first available quiz ID
            var firstQuiz = _dataStore.Quizzes.FirstOrDefault();
            if (firstQuiz == null)
            {
                Console.WriteLine("   ❌ FAILED: No quizzes found in datastore");
                return;
            }

            var result = _quizService.StartQuizAttemptAsync(firstQuiz.Id, "test-user").Result;
            
            if (result.IsSuccess && result.Data != null)
            {
                Console.WriteLine($"   ✅ PASSED: Started quiz attempt with ID {result.Data.Id}");
                Console.WriteLine($"      Quiz ID: {result.Data.QuizId}, User: {result.Data.UserId}, Max Score: {result.Data.MaxScore}");
            }
            else
            {
                Console.WriteLine($"   ❌ FAILED: Could not start quiz attempt. Error: {result.ErrorMessage}");
                
                // Debug info
                var questions = _dataStore.Questions.Where(q => q.QuizId == firstQuiz.Id).ToList();
                Console.WriteLine($"      Debug: Quiz {firstQuiz.Id} has {questions.Count} questions in datastore");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ FAILED: Exception occurred: {ex.Message}");
        }
    }

    private void TestSubmitQuizAnswers()
    {
        Console.WriteLine("🧪 Test: SubmitQuizAnswersAsync");
        
        try
        {
            // Get the first available quiz
            var firstQuiz = _dataStore.Quizzes.FirstOrDefault();
            if (firstQuiz == null)
            {
                Console.WriteLine("   ❌ FAILED: No quizzes found in datastore");
                return;
            }

            // Start an attempt first
            var attemptResult = _quizService.StartQuizAttemptAsync(firstQuiz.Id, "test-user").Result;
            if (!attemptResult.IsSuccess)
            {
                Console.WriteLine($"   ❌ FAILED: Could not start quiz attempt. Error: {attemptResult.ErrorMessage}");
                return;
            }

            // Get questions for this quiz
            var questions = _dataStore.Questions.Where(q => q.QuizId == firstQuiz.Id).Take(2).ToList();
            if (!questions.Any())
            {
                Console.WriteLine("   ❌ FAILED: No questions found for quiz");
                return;
            }

            // Create answers for the first two questions
            var answers = new List<UserAnswer>();
            
            foreach (var question in questions)
            {
                var firstOption = _dataStore.QuestionOptions.FirstOrDefault(o => o.QuestionId == question.Id);
                if (firstOption != null)
                {
                    answers.Add(new UserAnswer 
                    { 
                        QuestionId = question.Id, 
                        SelectedOptionIds = firstOption.Id.ToString() 
                    });
                }
            }

            if (!answers.Any())
            {
                Console.WriteLine("   ❌ FAILED: Could not create test answers");
                return;
            }

            var result = _quizService.SubmitQuizAnswersAsync(attemptResult.Data.Id, answers).Result;
            
            if (result.IsSuccess && result.Data.IsCompleted)
            {
                Console.WriteLine($"   ✅ PASSED: Submitted {answers.Count} answers, score: {result.Data.Score}/{result.Data.MaxScore} ({result.Data.Percentage:F1}%)");
            }
            else
            {
                Console.WriteLine($"   ❌ FAILED: Could not submit quiz answers. Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ FAILED: Exception occurred: {ex.Message}");
        }
    }

    private void TestGetUserAttempts()
    {
        Console.WriteLine("🧪 Test: GetUserAttemptsAsync");
        
        try
        {
            var result = _quizService.GetUserAttemptsAsync("test-user").Result;
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"   ✅ PASSED: Retrieved {result.Data.Count} user attempts");
                
                if (result.Data.Any())
                {
                    var latestAttempt = result.Data.First();
                    Console.WriteLine($"      Latest attempt: Quiz ID {latestAttempt.QuizId}, Score: {latestAttempt.Score}/{latestAttempt.MaxScore}");
                }
            }
            else
            {
                Console.WriteLine($"   ❌ FAILED: Could not retrieve user attempts. Error: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ FAILED: Exception occurred: {ex.Message}");
        }
    }
}
// ================================
// PROGRAM ENTRY POINT
// ================================
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Run unit tests
            Console.WriteLine("🧪 RUNNING SYSTEM TESTS");
            Console.WriteLine("========================");
            var tests = new QuizServiceTests();
            tests.RunAllTests();
            
            Console.WriteLine("\nPress Enter to start QuizMaker...");
            Console.ReadLine();
            
            // Run the interactive application
            var app = new InteractiveQuizApplication();
            app.RunAsync().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Application error: {ex.Message}");
        }
    }
}
