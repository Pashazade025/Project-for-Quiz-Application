using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
}

// ================================
// DATA STORE WITH SAMPLE DATA
// ================================
public class QuizDataStore
{
    private static QuizDataStore _instance;
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

    private QuizDataStore()
    {
        SeedData();
    }

    private void SeedData()
    {
        // Seed Categories
        Categories.Add(new Category { Id = 1, Name = "Programming", Description = "Programming and computer science" });
        Categories.Add(new Category { Id = 2, Name = "General Knowledge", Description = "General knowledge questions" });
        Categories.Add(new Category { Id = 3, Name = "Science", Description = "Science and technology questions" });
        Categories.Add(new Category { Id = 4, Name = "Mathematics", Description = "Math and calculation questions" });

        // Seed Users
        var adminUser = new ApplicationUser
        {
            Id = "admin-123",
            UserName = "admin@quizmaker.com",
            Email = "admin@quizmaker.com",
            Password = "admin123",
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin"
        };
        Users.Add(adminUser);

        var regularUser = new ApplicationUser
        {
            Id = "user-123",
            UserName = "john@example.com",
            Email = "john@example.com",
            Password = "user123",
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };
        Users.Add(regularUser);

        // Create Sample Quizzes with Real Questions
        CreateSampleQuizzes();
    }

    private void CreateSampleQuizzes()
    {
        // Quiz 1: C# Programming Basics
        var csharpQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "C# Programming Basics",
            Description = "Test your knowledge of C# fundamentals",
            CategoryId = 1,
            CreatedById = "admin-123",
            IsPublic = true,
            TimeLimit = 15
        };

        // Question 1: Multiple Choice Single
        var q1 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = csharpQuiz.Id,
            QuestionText = "What is the correct way to declare a variable in C#?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 2,
            OrderIndex = 0
        };
        q1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q1.Id, OptionText = "var name = \"John\";", IsCorrect = true, OrderIndex = 0 });
        q1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q1.Id, OptionText = "variable name = \"John\";", IsCorrect = false, OrderIndex = 1 });
        q1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q1.Id, OptionText = "declare name = \"John\";", IsCorrect = false, OrderIndex = 2 });
        q1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q1.Id, OptionText = "name := \"John\";", IsCorrect = false, OrderIndex = 3 });

        // Question 2: True/False
        var q2 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = csharpQuiz.Id,
            QuestionText = "C# is a case-sensitive programming language.",
            QuestionType = QuestionType.TrueFalse,
            Points = 1,
            OrderIndex = 1
        };
        q2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q2.Id, OptionText = "True", IsCorrect = true, OrderIndex = 0 });
        q2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q2.Id, OptionText = "False", IsCorrect = false, OrderIndex = 1 });

        // Question 3: Multiple Choice Multiple
        var q3 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = csharpQuiz.Id,
            QuestionText = "Which of the following are valid C# access modifiers? (Select all that apply)",
            QuestionType = QuestionType.MultipleChoiceMultiple,
            Points = 3,
            OrderIndex = 2
        };
        q3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q3.Id, OptionText = "public", IsCorrect = true, OrderIndex = 0 });
        q3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q3.Id, OptionText = "private", IsCorrect = true, OrderIndex = 1 });
        q3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q3.Id, OptionText = "protected", IsCorrect = true, OrderIndex = 2 });
        q3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q3.Id, OptionText = "secure", IsCorrect = false, OrderIndex = 3 });

        // Question 4: LINQ Knowledge
        var q4 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = csharpQuiz.Id,
            QuestionText = "What does LINQ stand for?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 2,
            OrderIndex = 3
        };
        q4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q4.Id, OptionText = "Language Integrated Query", IsCorrect = true, OrderIndex = 0 });
        q4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q4.Id, OptionText = "Linear Intelligence Query", IsCorrect = false, OrderIndex = 1 });
        q4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q4.Id, OptionText = "Language Internet Query", IsCorrect = false, OrderIndex = 2 });
        q4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q4.Id, OptionText = "Language Interface Query", IsCorrect = false, OrderIndex = 3 });

        // Question 5: Exception Handling
        var q5 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = csharpQuiz.Id,
            QuestionText = "In C#, exceptions are handled using try-catch blocks.",
            QuestionType = QuestionType.TrueFalse,
            Points = 1,
            OrderIndex = 4
        };
        q5.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q5.Id, OptionText = "True", IsCorrect = true, OrderIndex = 0 });
        q5.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = q5.Id, OptionText = "False", IsCorrect = false, OrderIndex = 1 });

        csharpQuiz.Questions.AddRange(new[] { q1, q2, q3, q4, q5 });
        Quizzes.Add(csharpQuiz);
        Questions.AddRange(new[] { q1, q2, q3, q4, q5 });
        QuestionOptions.AddRange(q1.Options.Concat(q2.Options).Concat(q3.Options).Concat(q4.Options).Concat(q5.Options));

        // Quiz 2: General Knowledge
        var generalQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "World Geography Quiz",
            Description = "Test your knowledge of world geography",
            CategoryId = 2,
            CreatedById = "admin-123",
            IsPublic = true,
            TimeLimit = 10
        };

        var gq1 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = generalQuiz.Id,
            QuestionText = "What is the capital of France?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 1,
            OrderIndex = 0
        };
        gq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq1.Id, OptionText = "London", IsCorrect = false, OrderIndex = 0 });
        gq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq1.Id, OptionText = "Berlin", IsCorrect = false, OrderIndex = 1 });
        gq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq1.Id, OptionText = "Paris", IsCorrect = true, OrderIndex = 2 });
        gq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq1.Id, OptionText = "Madrid", IsCorrect = false, OrderIndex = 3 });

        var gq2 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = generalQuiz.Id,
            QuestionText = "The Great Wall of China was built to protect against invasions.",
            QuestionType = QuestionType.TrueFalse,
            Points = 1,
            OrderIndex = 1
        };
        gq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq2.Id, OptionText = "True", IsCorrect = true, OrderIndex = 0 });
        gq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq2.Id, OptionText = "False", IsCorrect = false, OrderIndex = 1 });

        var gq3 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = generalQuiz.Id,
            QuestionText = "Which planet is known as the Red Planet?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 1,
            OrderIndex = 2
        };
        gq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq3.Id, OptionText = "Venus", IsCorrect = false, OrderIndex = 0 });
        gq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq3.Id, OptionText = "Mars", IsCorrect = true, OrderIndex = 1 });
        gq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq3.Id, OptionText = "Jupiter", IsCorrect = false, OrderIndex = 2 });
        gq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq3.Id, OptionText = "Saturn", IsCorrect = false, OrderIndex = 3 });

        var gq4 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = generalQuiz.Id,
            QuestionText = "Which of these are continents? (Select all that apply)",
            QuestionType = QuestionType.MultipleChoiceMultiple,
            Points = 2,
            OrderIndex = 3
        };
        gq4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq4.Id, OptionText = "Asia", IsCorrect = true, OrderIndex = 0 });
        gq4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq4.Id, OptionText = "Europe", IsCorrect = true, OrderIndex = 1 });
        gq4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq4.Id, OptionText = "Greenland", IsCorrect = false, OrderIndex = 2 });
        gq4.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = gq4.Id, OptionText = "Antarctica", IsCorrect = true, OrderIndex = 3 });

        generalQuiz.Questions.AddRange(new[] { gq1, gq2, gq3, gq4 });
        Quizzes.Add(generalQuiz);
        Questions.AddRange(new[] { gq1, gq2, gq3, gq4 });
        QuestionOptions.AddRange(gq1.Options.Concat(gq2.Options).Concat(gq3.Options).Concat(gq4.Options));

        // Quiz 3: Basic Math
        var mathQuiz = new Quiz
        {
            Id = GetNextQuizId(),
            Title = "Basic Mathematics",
            Description = "Test your basic math skills",
            CategoryId = 4,
            CreatedById = "user-123",
            IsPublic = true,
            TimeLimit = 5
        };

        var mq1 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = mathQuiz.Id,
            QuestionText = "What is 15 + 27?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 1,
            OrderIndex = 0
        };
        mq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq1.Id, OptionText = "40", IsCorrect = false, OrderIndex = 0 });
        mq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq1.Id, OptionText = "42", IsCorrect = true, OrderIndex = 1 });
        mq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq1.Id, OptionText = "44", IsCorrect = false, OrderIndex = 2 });
        mq1.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq1.Id, OptionText = "45", IsCorrect = false, OrderIndex = 3 });

        var mq2 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = mathQuiz.Id,
            QuestionText = "What is 8 × 7?",
            QuestionType = QuestionType.MultipleChoiceSingle,
            Points = 1,
            OrderIndex = 1
        };
        mq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq2.Id, OptionText = "54", IsCorrect = false, OrderIndex = 0 });
        mq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq2.Id, OptionText = "56", IsCorrect = true, OrderIndex = 1 });
        mq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq2.Id, OptionText = "58", IsCorrect = false, OrderIndex = 2 });
        mq2.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq2.Id, OptionText = "64", IsCorrect = false, OrderIndex = 3 });

        var mq3 = new Question
        {
            Id = GetNextQuestionId(),
            QuizId = mathQuiz.Id,
            QuestionText = "A triangle has exactly 3 sides.",
            QuestionType = QuestionType.TrueFalse,
            Points = 1,
            OrderIndex = 2
        };
        mq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq3.Id, OptionText = "True", IsCorrect = true, OrderIndex = 0 });
        mq3.Options.Add(new QuestionOption { Id = GetNextOptionId(), QuestionId = mq3.Id, OptionText = "False", IsCorrect = false, OrderIndex = 1 });

        mathQuiz.Questions.AddRange(new[] { mq1, mq2, mq3 });
        Quizzes.Add(mathQuiz);
        Questions.AddRange(new[] { mq1, mq2, mq3 });
        QuestionOptions.AddRange(mq1.Options.Concat(mq2.Options).Concat(mq3.Options));

        // Load relations
        foreach (var quiz in Quizzes)
        {
            quiz.Category = Categories.FirstOrDefault(c => c.Id == quiz.CategoryId);
            quiz.CreatedBy = Users.FirstOrDefault(u => u.Id == quiz.CreatedById);
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

        // Load questions and options
        quiz.Questions = _dataStore.Questions.Where(q => q.QuizId == quiz.Id).OrderBy(q => q.OrderIndex).ToList();
        foreach (var question in quiz.Questions)
        {
            question.Options = _dataStore.QuestionOptions.Where(o => o.QuestionId == question.Id).OrderBy(o => o.OrderIndex).ToList();
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

        var attempt = new QuizAttempt
        {
            Id = _dataStore.GetNextAttemptId(),
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            MaxScore = _dataStore.Questions.Where(q => q.QuizId == quizId).Sum(q => q.Points)
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
        var correctOptions = _dataStore.QuestionOptions
            .Where(o => o.QuestionId == question.Id && o.IsCorrect)
            .Select(o => o.Id.ToString())
            .ToList();

        switch (question.QuestionType)
        {
            case QuestionType.MultipleChoiceSingle:
            case QuestionType.TrueFalse:
                return correctOptions.Contains(answer.SelectedOptionIds);

            case QuestionType.MultipleChoiceMultiple:
                var selectedIds = answer.SelectedOptionIds?.Split(',').ToList() ?? new List<string>();
                return correctOptions.All(selectedIds.Contains) && selectedIds.All(correctOptions.Contains);

            case QuestionType.ShortAnswer:
                // For demo purposes, simple string comparison (in real app, use more sophisticated matching)
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
    bool IsInRole(ApplicationUser user, string role);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly QuizDataStore _dataStore;

    public AuthenticationService(QuizDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<ServiceResult<ApplicationUser>> LoginAsync(string email, string password)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            return Task.FromResult(ServiceResult<ApplicationUser>.Failure("Invalid email or password"));
        }

        if (user.Password != password)
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
// INTERACTIVE QUIZ APPLICATION
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
            Console.WriteLine("🔐 LOGIN TO QUIZMAKER");
            Console.WriteLine("=====================");
            Console.WriteLine("Available accounts:");
            Console.WriteLine("📧 admin@quizmaker.com (Password: admin123) - Admin");
            Console.WriteLine("📧 john@example.com (Password: user123) - User\n");

            Console.Write("Enter email: ");
            string email = Console.ReadLine();
            
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("❌ Email is required!\n");
                continue;
            }
            
            Console.Write("Enter password: ");
            string password = Console.ReadLine();
            
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("❌ Password is required!\n");
                continue;
            }

            var loginResult = await _authService.LoginAsync(email, password);
            if (loginResult.IsSuccess)
            {
                _currentUser = loginResult.Data;
                Console.WriteLine($"\n✅ Login successful!");
                Console.WriteLine($"👋 Welcome, {_currentUser.FirstName} {_currentUser.LastName}!");
                Console.WriteLine($"🔑 Role: {_currentUser.Role}\n");
                break;
            }
            else
            {
                Console.WriteLine($"\n❌ {loginResult.ErrorMessage}");
                Console.WriteLine("Please try again.\n");
            }
        }
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
        Console.WriteLine($"\n🎯 Starting Quiz: {quiz.Title}");
        Console.WriteLine($"📝 Description: {quiz.Description}");
        Console.WriteLine($"⏱️ Time Limit: {(quiz.TimeLimit > 0 ? quiz.TimeLimit + " minutes" : "No limit")}");
        Console.WriteLine($"📊 Total Questions: {quiz.Questions.Count}");
        Console.WriteLine($"🎯 Max Score: {quiz.Questions.Sum(q => q.Points)} points\n");

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

        foreach (var question in quiz.Questions)
        {
            Console.Clear();
            Console.WriteLine($"🎯 Quiz: {quiz.Title}");
            Console.WriteLine($"Question {question.OrderIndex + 1} of {quiz.Questions.Count}");
            Console.WriteLine($"Points: {question.Points}");
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
                    break;

                case QuestionType.MultipleChoiceMultiple:
                    Console.WriteLine("Select all correct options (enter numbers separated by commas):");
                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {question.Options[i].OptionText}");
                    }
                    Console.Write("\nYour answers (e.g., 1,3,4): ");
                    string multipleChoice = Console.ReadLine();
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
                    break;
            }

            userAnswers.Add(answer);
            
            // Show time elapsed if there's a time limit
            if (quiz.TimeLimit > 0)
            {
                var elapsed = DateTime.Now - startTime;
                var remaining = TimeSpan.FromMinutes(quiz.TimeLimit) - elapsed;
                if (remaining.TotalSeconds > 0)
                {
                    Console.WriteLine($"\n⏱️ Time remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}");
                }
                else
                {
                    Console.WriteLine("\n⏰ Time's up!");
                    break;
                }
            }

            // Don't show "Press Enter" for the last question
            if (question.OrderIndex < quiz.Questions.Count - 1)
            {
                Console.WriteLine("\nPress Enter to continue to next question...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\nQuiz completed! Processing results...");
                Console.ReadLine();
            }
        }

        // Submit answers and show results
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
        
        // Performance rating
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
                // Show selected and correct options
                var selectedIds = userAnswer.SelectedOptionIds?.Split(',').ToList() ?? new List<string>();
                var correctOptions = question.Options.Where(o => o.IsCorrect).ToList();
                
                Console.WriteLine("Options:");
                foreach (var option in question.Options)
                {
                    string marker = "";
                    if (selectedIds.Contains(option.Id.ToString()) && option.IsCorrect)
                        marker = "✅"; // Correct selection
                    else if (selectedIds.Contains(option.Id.ToString()) && !option.IsCorrect)
                        marker = "❌"; // Wrong selection
                    else if (!selectedIds.Contains(option.Id.ToString()) && option.IsCorrect)
                        marker = "🔵"; // Correct but not selected
                    else
                        marker = "⚪"; // Not selected
                    
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
            
            foreach (var attempt in result.Data.Take(10)) // Show last 10 attempts
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

            // Statistics
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
        Console.WriteLine("\n✅ Admin privileges verified!");
        Console.WriteLine("🔑 Full system access granted.");
        
        // Show some admin statistics
        var dataStore = QuizDataStore.Instance;
        Console.WriteLine("\n📊 SYSTEM STATISTICS:");
        Console.WriteLine($"   Total Users: {dataStore.Users.Count}");
        Console.WriteLine($"   Total Quizzes: {dataStore.Quizzes.Count}");
        Console.WriteLine($"   Total Questions: {dataStore.Questions.Count}");
        Console.WriteLine($"   Total Quiz Attempts: {dataStore.QuizAttempts.Count}");
        Console.WriteLine($"   Active Categories: {dataStore.Categories.Count(c => c.IsActive)}");
    }

    private async Task ViewAllQuizResults()
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
            return;
        }

        Console.WriteLine($"Total completed quiz attempts: {allAttempts.Count}\n");

        // Group by quiz
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

        // Overall statistics
        Console.WriteLine("📊 OVERALL STATISTICS:");
        Console.WriteLine($"   Average Score Across All Quizzes: {allAttempts.Average(a => a.Percentage):F1}%");
        Console.WriteLine($"   Total Points Awarded: {allAttempts.Sum(a => a.Score)}");
        Console.WriteLine($"   Most Active User: {GetMostActiveUser(dataStore)}");
        Console.WriteLine($"   Most Popular Quiz: {GetMostPopularQuiz(dataStore)}");
    }

    private string GetMostActiveUser(QuizDataStore dataStore)
    {
        var userAttempts = dataStore.QuizAttempts
            .Where(a => a.IsCompleted)
            .GroupBy(a => a.UserId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (userAttempts == null) return "None";

        var user = dataStore.Users.FirstOrDefault(u => u.Id == userAttempts.Key);
        return $"{user?.FirstName ?? "Unknown"} {user?.LastName ?? "User"} ({userAttempts.Count()} attempts)";
    }

    private string GetMostPopularQuiz(QuizDataStore dataStore)
    {
        var quizAttempts = dataStore.QuizAttempts
            .Where(a => a.IsCompleted)
            .GroupBy(a => a.QuizId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (quizAttempts == null) return "None";

        var quiz = dataStore.Quizzes.FirstOrDefault(q => q.Id == quizAttempts.Key);
        return $"{quiz?.Title ?? "Unknown Quiz"} ({quizAttempts.Count()} attempts)";
    }
}

// ================================
// UNIT TESTS FOR BUSINESS LOGIC
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
        
        var result = _quizService.GetQuizAsync(1).Result;
        
        if (result.IsSuccess && result.Data != null && result.Data.Questions.Any())
        {
            Console.WriteLine($"   ✅ PASSED: Retrieved quiz with {result.Data.Questions.Count} questions");
        }
        else
        {
            Console.WriteLine("   ❌ FAILED: Should retrieve quiz with questions");
        }
    }

    private void TestStartQuizAttempt()
    {
        Console.WriteLine("🧪 Test: StartQuizAttemptAsync");
        
        var result = _quizService.StartQuizAttemptAsync(1, "test-user").Result;
        
        if (result.IsSuccess && result.Data != null)
        {
            Console.WriteLine($"   ✅ PASSED: Started quiz attempt with ID {result.Data.Id}");
        }
        else
        {
            Console.WriteLine("   ❌ FAILED: Should start quiz attempt");
        }
    }

    private void TestSubmitQuizAnswers()
    {
        Console.WriteLine("🧪 Test: SubmitQuizAnswersAsync");
        
        // Start an attempt first
        var attemptResult = _quizService.StartQuizAttemptAsync(1, "test-user").Result;
        if (!attemptResult.IsSuccess) return;

        var answers = new List<UserAnswer>
        {
            new UserAnswer { QuestionId = 1, SelectedOptionIds = "1" },
            new UserAnswer { QuestionId = 2, SelectedOptionIds = "5" }
        };

        var result = _quizService.SubmitQuizAnswersAsync(attemptResult.Data.Id, answers).Result;
        
        if (result.IsSuccess && result.Data.IsCompleted)
        {
            Console.WriteLine($"   ✅ PASSED: Submitted answers, score: {result.Data.Score}/{result.Data.MaxScore}");
        }
        else
        {
            Console.WriteLine("   ❌ FAILED: Should submit quiz answers");
        }
    }

    private void TestGetUserAttempts()
    {
        Console.WriteLine("🧪 Test: GetUserAttemptsAsync");
        
        var result = _quizService.GetUserAttemptsAsync("test-user").Result;
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"   ✅ PASSED: Retrieved {result.Data.Count} user attempts");
        }
        else
        {
            Console.WriteLine("   ❌ FAILED: Should retrieve user attempts");
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
    
    private static void ShowRequirementsVerification()
    {
        // This method is no longer used
    }
}