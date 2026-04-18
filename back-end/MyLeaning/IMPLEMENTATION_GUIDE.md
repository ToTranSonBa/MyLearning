# Implementation Quick Start Guide

## ?? Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server 2019+ or PostgreSQL 13+
- Redis 7+
- Git
- Visual Studio 2022 or VS Code

### Setup Steps

#### 1. Clone & Install Dependencies

```bash
# Clone repository
git clone https://github.com/yourusername/japanese-learning.git
cd japanese-learning

# Restore NuGet packages
dotnet restore

# Install EF Core tools
dotnet tool install --global dotnet-ef
```

#### 2. Database Setup

```bash
# Create database
dotnet ef database create -p Infrastructure -s Presentation

# Run migrations
dotnet ef database update -p Infrastructure -s Presentation

# Seed test data (optional)
dotnet run --seed-data
```

#### 3. Configuration

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JapaneseLearning;Integrated Security=true;"
  },
  "Redis": {
    "Host": "localhost",
    "Port": 6379,
    "Database": 0
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "your-sendgrid-api-key"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### 4. Run Application

```bash
# Start API
cd Presentation
dotnet run

# Application available at: https://localhost:5001
# Swagger UI: https://localhost:5001/swagger
```

---

## ??? Creating New Features

### Example: Adding New Exercise Type

#### Step 1: Domain Layer

```csharp
// Domain/Aggregates/ExerciseAggregate/ExerciseEnums.cs
public enum QuestionType
{
    MultipleChoice,
    FillInTheBlank,
    Listening,
    Dragging        // NEW
}

// Domain/Aggregates/ExerciseAggregate/Question.cs
public static Question CreateDragging(ExerciseId exerciseId, string questionText,
    List<string> options, string correctAnswer, int points, int orderIndex)
{
    return new Question
    {
        Id = QuestionId.CreateNew(),
        ExerciseId = exerciseId,
        QuestionType = QuestionType.Dragging,
        QuestionText = questionText,
        CorrectAnswer = correctAnswer,
        Points = points,
        OrderIndex = orderIndex
    };
}
```

#### Step 2: Application Layer

```csharp
// Application/Features/Exercises/Commands/CreateDraggingQuestionCommand.cs
public record CreateDraggingQuestionCommand(
    ExerciseId ExerciseId,
    string QuestionText,
    List<string> Options,
    string CorrectAnswer,
    int Points,
    int OrderIndex
) : IRequest<QuestionDto>;

public class CreateDraggingQuestionHandler : IRequestHandler<CreateDraggingQuestionCommand, QuestionDto>
{
    private readonly IExerciseRepository _repository;
    private readonly IMapper _mapper;

    public async Task<QuestionDto> Handle(CreateDraggingQuestionCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _repository.GetByIdAsync(request.ExerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");

        var question = Question.CreateDragging(
            request.ExerciseId,
            request.QuestionText,
            request.Options,
            request.CorrectAnswer,
            request.Points,
            request.OrderIndex
        );

        exercise.AddQuestion(question);
        await _repository.UpdateAsync(exercise);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QuestionDto>(question);
    }
}

// Application/Common/Validators/CreateDraggingQuestionValidator.cs
public class CreateDraggingQuestionValidator : AbstractValidator<CreateDraggingQuestionCommand>
{
    public CreateDraggingQuestionValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(500);

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options are required")
            .Must(o => o.Count >= 2).WithMessage("At least 2 options required");

        RuleFor(x => x.CorrectAnswer)
            .NotEmpty()
            .Must((cmd, ans) => cmd.Options.Contains(ans))
            .WithMessage("Correct answer must be in options list");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Points must be greater than 0");
    }
}
```

#### Step 3: Infrastructure Layer

```csharp
// Infrastructure/Persistence/Configurations/QuestionConfiguration.cs
// Add discriminator mapping for new type
builder.HasDiscriminator()
    .HasValue(QuestionType.MultipleChoice, "MC")
    .HasValue(QuestionType.FillInTheBlank, "FIB")
    .HasValue(QuestionType.Listening, "LST")
    .HasValue(QuestionType.Dragging, "DRG");   // NEW
```

#### Step 4: Presentation Layer

```csharp
// Presentation/Controllers/ExercisesController.cs
[HttpPost("{exerciseId}/questions/dragging")]
[Authorize]
public async Task<IActionResult> CreateDraggingQuestion(
    Guid exerciseId,
    [FromBody] CreateDraggingQuestionDto dto)
{
    var command = new CreateDraggingQuestionCommand(
        new ExerciseId(exerciseId),
        dto.QuestionText,
        dto.Options,
        dto.CorrectAnswer,
        dto.Points,
        dto.OrderIndex
    );

    var result = await _mediator.Send(command);
    return Created($"/questions/{result.Id}", result);
}

// Presentation/Validators/CreateDraggingQuestionDtoValidator.cs
public class CreateDraggingQuestionDtoValidator : AbstractValidator<CreateDraggingQuestionDto>
{
    public CreateDraggingQuestionDtoValidator()
    {
        RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Options).NotEmpty().Must(o => o.Count >= 2);
        RuleFor(x => x.CorrectAnswer).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}
```

#### Step 5: Testing

```csharp
// Tests/UnitTests/Domain/Questions/DraggingQuestionTests.cs
public class DraggingQuestionTests
{
    [Fact]
    public void CreateDragging_WithValidData_CreatesQuestion()
    {
        // Arrange
        var exerciseId = ExerciseId.CreateNew();

        // Act
        var question = Question.CreateDragging(
            exerciseId,
            "Drag the correct kanji",
            new List<string> { "?", "?", "?" },
            "?",
            10,
            1
        );

        // Assert
        Assert.NotNull(question);
        Assert.Equal(QuestionType.Dragging, question.QuestionType);
        Assert.Equal("?", question.CorrectAnswer);
    }

    [Fact]
    public void CreateDragging_WithInvalidAnswer_ThrowsException()
    {
        // Arrange
        var exerciseId = ExerciseId.CreateNew();

        // Act & Assert
        Assert.Throws<DomainException>(() => Question.CreateDragging(
            exerciseId,
            "Question",
            new List<string> { "A", "B", "C" },
            "D", // Not in options
            10,
            1
        ));
    }
}

// Tests/IntegrationTests/Exercises/CreateDraggingQuestionTests.cs
public class CreateDraggingQuestionTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;
    private readonly IMediator _mediator;

    [Fact]
    public async Task CreateDraggingQuestion_WithValidCommand_ReturnsQuestionDto()
    {
        // Arrange
        var exerciseId = ExerciseId.CreateNew();
        var command = new CreateDraggingQuestionCommand(
            exerciseId,
            "Drag the correct option",
            new List<string> { "Option1", "Option2" },
            "Option1",
            10,
            1
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QuestionType.Dragging, result.Type);
    }
}
```

---

## ?? Workflow: User Registration to Exercise Completion

### Complete Flow Example

```csharp
// 1. Register User
var registerCmd = new RegisterCommand(
    "john_doe",
    "john@example.com",
    "SecurePass@123",
    "John Doe"
);
var authResponse = await mediator.Send(registerCmd);

// 2. Get Courses
var coursesQuery = new GetCoursesQuery();
var courses = await mediator.Send(coursesQuery);

// 3. Get Course Details
var courseQuery = new GetCourseByIdQuery(courses[0].Id);
var courseDetails = await mediator.Send(courseQuery);

// 4. Get Exercises for Topic
var exercisesQuery = new GetExercisesQuery(topicId);
var exercises = await mediator.Send(exercisesQuery);

// 5. Start Exercise
var startCmd = new StartExerciseCommand(exercises[0].Id);
var session = await mediator.Send(startCmd);

// 6. Answer Question 1
var answerCmd1 = new SubmitAnswerCommand(
    session.SessionId,
    session.Questions[0].Id,
    "option-1",
    5  // seconds
);
var answer1 = await mediator.Send(answerCmd1);

// 7. Answer Question 2
var answerCmd2 = new SubmitAnswerCommand(
    session.SessionId,
    session.Questions[1].Id,
    "option-2",
    3
);
var answer2 = await mediator.Send(answerCmd2);

// 8. Complete Exercise
var completeCmd = new CompleteExerciseCommand(session.SessionId);
var result = await mediator.Send(completeCmd);

// 9. Check Progress
var progressQuery = new GetUserProgressQuery(courseId);
var progress = await mediator.Send(progressQuery);

// 10. Add Vocabulary to SRS
foreach (var vocab in courseDetails.Vocabulary)
{
    var srsCmd = new AddVocabToSrsCommand(vocab.Id);
    await mediator.Send(srsCmd);
}

// 11. Get SRS Schedule
var srsQuery = new GetSrsScheduleQuery();
var srsSchedule = await mediator.Send(srsQuery);

// 12. Review SRS Card
var reviewCmd = new ReviewCardCommand(srsSchedule.Cards[0].Id, 4);
var reviewed = await mediator.Send(reviewCmd);
```

---

## ?? Common Tasks

### Add New Repository Method

```csharp
// 1. Add to Interface
public interface ICourseRepository : IRepository<Course>
{
    Task<IEnumerable<Course>> GetByLevelAsync(JlptLevel level);
    Task<decimal> GetAverageCompletionRateAsync(CourseId courseId);
}

// 2. Implement in Repository
public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public async Task<IEnumerable<Course>> GetByLevelAsync(JlptLevel level)
    {
        return await _context.Courses
            .Where(c => c.Level == level)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageCompletionRateAsync(CourseId courseId)
    {
        return await _context.UserProgress
            .Where(up => up.CourseId == courseId)
            .AverageAsync(up => up.OverallPercentage);
    }
}

// 3. Update Unit of Work (if needed)
// Already in IUnitOfWork: Task<int> SaveChangesAsync()
```

### Add New Query

```csharp
// Application/Features/Courses/Queries/GetCoursesByLevelQuery.cs
public record GetCoursesByLevelQuery(JlptLevel Level) : IRequest<IEnumerable<CourseDto>>;

public class GetCoursesByLevelHandler : IRequestHandler<GetCoursesByLevelQuery, IEnumerable<CourseDto>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public async Task<IEnumerable<CourseDto>> Handle(
        GetCoursesByLevelQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"courses_level_{request.Level}";
        
        // Try cache
        var cached = await _cache.GetAsync<IEnumerable<CourseDto>>(cacheKey);
        if (cached != null)
            return cached;

        // Query database
        var courses = await _courseRepository.GetByLevelAsync(request.Level);
        var dtos = _mapper.Map<IEnumerable<CourseDto>>(courses);

        // Cache for 24 hours
        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromHours(24));

        return dtos;
    }
}
```

### Add Cache Layer

```csharp
// Infrastructure/Caching/CacheService.cs
public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _redis.GetStringAsync(key);
        return value != null ? JsonSerializer.Deserialize<T>(value) : null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _redis.SetStringAsync(key, json, expiration);
    }

    public async Task InvalidateAsync(string key)
    {
        await _redis.KeyDeleteAsync(key);
    }
}
```

---

## ?? Debugging Tips

### Enable SQL Logging

```csharp
// Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    
    if (environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine);
        options.EnableSensitiveDataLogging();
    }
});
```

### Check Cache Status

```csharp
// Access Redis CLI
redis-cli

# Check key pattern
KEYS "jl:*"

# Get cache value
GET "jl:course:550e8400-e29b-41d4-a716-446655440000"

# Monitor all commands
MONITOR
```

### View Database Queries

```csharp
// DbContext.Database.Log in older versions
// Use SQL Profiler or Azure Data Studio for SQL Server
```

---

## ?? Performance Testing

```bash
# Load testing with k6
k6 run load-tests/api.js --vus 100 --duration 60s

# Results analysis
# - Check p95, p99 response times
# - Monitor error rates
# - Review resource utilization
```

---

## ?? Deployment Checklist

- [ ] All tests passing (unit + integration)
- [ ] Code reviewed and approved
- [ ] Security scan completed
- [ ] Performance tested
- [ ] Database backups configured
- [ ] Logging/monitoring set up
- [ ] Documentation updated
- [ ] Deployment automated
- [ ] Rollback plan ready
- [ ] Post-deployment verification

---

## ?? Troubleshooting

| Issue | Solution |
|-------|----------|
| Migration fails | Check connection string, run `dotnet ef database update --verbose` |
| Redis connection error | Verify Redis running: `redis-cli ping` |
| JWT token invalid | Check secret key in appsettings, verify expiration |
| N+1 query problem | Use `.Include()` for related entities |
| Cache inconsistency | Implement cache invalidation strategy |
| Performance degradation | Profile with Application Insights, check slow queries |

---

**Status**: ? Ready to Implement
**Last Updated**: January 2024
