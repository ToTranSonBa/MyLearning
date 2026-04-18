# Japanese Learning Application - Entity Design

## Domain Entities (C# Code Structure)

### User Management Aggregates

```csharp
// Domain/Aggregates/UserAggregate/User.cs
namespace Domain.Aggregates.UserAggregate;

public class User : AggregateRoot
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public Username Username { get; private set; }
    public string PasswordHash { get; private set; }
    public UserProfile Profile { get; private set; }
    public JlptLevel PreferredLevel { get; private set; }
    public Subscription CurrentSubscription { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public static User Create(Email email, Username username, string passwordHash, UserProfile profile)
    {
        var user = new User
        {
            Id = UserId.CreateNew(),
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            Profile = profile,
            PreferredLevel = JlptLevel.N5, // Default
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, email, username));
        return user;
    }

    public void UpdateProfile(UserProfile newProfile)
    {
        Profile = newProfile;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserProfileUpdatedEvent(Id));
    }

    public void SetPreferredLevel(JlptLevel level)
    {
        PreferredLevel = level;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Domain/Aggregates/UserAggregate/UserProfile.cs
public class UserProfile : ValueObject
{
    public string FullName { get; }
    public string? Bio { get; }
    public string? AvatarUrl { get; }
    public string? NativeLanguage { get; }
    public LearningStyle PreferredLearningStyle { get; }

    public UserProfile(string fullName, string? bio = null, string? avatarUrl = null, 
        string? nativeLanguage = null, LearningStyle preferredStyle = LearningStyle.Visual)
    {
        FullName = fullName;
        Bio = bio;
        AvatarUrl = avatarUrl;
        NativeLanguage = nativeLanguage;
        PreferredLearningStyle = preferredStyle;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FullName;
        yield return Bio;
        yield return AvatarUrl;
        yield return NativeLanguage;
        yield return PreferredLearningStyle;
    }
}

// Domain/Aggregates/UserAggregate/Subscription.cs
public class Subscription : ValueObject
{
    public SubscriptionPlan Plan { get; }
    public DateTime StartDate { get; }
    public DateTime ExpiryDate { get; }
    public bool IsActive => DateTime.UtcNow < ExpiryDate;

    public Subscription(SubscriptionPlan plan, DateTime startDate, DateTime expiryDate)
    {
        Plan = plan;
        StartDate = startDate;
        ExpiryDate = expiryDate;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Plan;
        yield return StartDate;
        yield return ExpiryDate;
    }
}

// Domain/Aggregates/UserAggregate/ValueObjects.cs
public class UserId : StronglyTypedId
{
    public UserId(Guid value) : base(value) { }
    public static UserId CreateNew() => new(Guid.NewGuid());
}

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValid(value))
            throw new DomainException("Email format is invalid");
        Value = value.ToLower();
    }

    private static bool IsValid(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.ToLower();
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

public class Username : ValueObject
{
    public string Value { get; }
    private const int MinLength = 3;
    private const int MaxLength = 20;

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < MinLength || value.Length > MaxLength)
            throw new DomainException($"Username must be between {MinLength} and {MaxLength} characters");
        
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

public enum JlptLevel
{
    N5 = 1, // Beginner
    N4 = 2,
    N3 = 3,
    N2 = 4,
    N1 = 5  // Advanced
}

public enum LearningStyle
{
    Visual,
    Auditory,
    ReadingWriting,
    Kinesthetic
}

public enum SubscriptionPlan
{
    Free,
    Premium,
    PremiumPlus
}
```

### Content Management Aggregates

```csharp
// Domain/Aggregates/ContentAggregate/Course.cs
namespace Domain.Aggregates.ContentAggregate;

public class Course : AggregateRoot
{
    public CourseId Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public JlptLevel Level { get; private set; }
    public int DurationHours { get; private set; }
    public string? ImageUrl { get; private set; }
    
    private List<Lesson> _lessons = new();
    public IReadOnlyList<Lesson> Lessons => _lessons.AsReadOnly();

    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Course() { }

    public static Course Create(string title, string description, JlptLevel level, 
        int durationHours, string createdBy, string? imageUrl = null)
    {
        var course = new Course
        {
            Id = CourseId.CreateNew(),
            Title = title,
            Description = description,
            Level = level,
            DurationHours = durationHours,
            ImageUrl = imageUrl,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        course.AddDomainEvent(new CourseCreatedEvent(course.Id, title, level));
        return course;
    }

    public void AddLesson(Lesson lesson)
    {
        if (_lessons.Count >= 100)
            throw new DomainException("Course cannot have more than 100 lessons");
        
        _lessons.Add(lesson);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(string title, string description, string? imageUrl = null)
    {
        Title = title;
        Description = description;
        if (!string.IsNullOrEmpty(imageUrl))
            ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Domain/Aggregates/ContentAggregate/Lesson.cs
public class Lesson : Entity
{
    public LessonId Id { get; private set; }
    public CourseId CourseId { get; private set; }
    public int LessonNumber { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public int OrderIndex { get; private set; }

    private List<Topic> _topics = new();
    public IReadOnlyList<Topic> Topics => _topics.AsReadOnly();

    private Lesson() { }

    public static Lesson Create(CourseId courseId, int lessonNumber, string title, 
        string description, int durationMinutes, int orderIndex)
    {
        return new Lesson
        {
            Id = LessonId.CreateNew(),
            CourseId = courseId,
            LessonNumber = lessonNumber,
            Title = title,
            Description = description,
            DurationMinutes = durationMinutes,
            OrderIndex = orderIndex
        };
    }

    public void AddTopic(Topic topic)
    {
        if (_topics.Count >= 50)
            throw new DomainException("Lesson cannot have more than 50 topics");
        
        _topics.Add(topic);
    }
}

// Domain/Aggregates/ContentAggregate/Topic.cs
public class Topic : Entity
{
    public TopicId Id { get; private set; }
    public LessonId LessonId { get; private set; }
    public TopicType TopicType { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int OrderIndex { get; private set; }

    private List<IContent> _contentItems = new();
    public IReadOnlyList<IContent> ContentItems => _contentItems.AsReadOnly();

    private Topic() { }

    public static Topic Create(LessonId lessonId, TopicType topicType, string title,
        string description, int orderIndex)
    {
        return new Topic
        {
            Id = TopicId.CreateNew(),
            LessonId = lessonId,
            TopicType = topicType,
            Title = title,
            Description = description,
            OrderIndex = orderIndex
        };
    }

    public void AddVocabulary(VocabularyItem vocab)
    {
        if (TopicType != TopicType.Vocabulary)
            throw new DomainException("Cannot add vocabulary to non-vocabulary topic");
        
        _contentItems.Add(vocab);
    }

    public void AddGrammar(GrammarRule grammar)
    {
        if (TopicType != TopicType.Grammar)
            throw new DomainException("Cannot add grammar to non-grammar topic");
        
        _contentItems.Add(grammar);
    }

    public void AddKanji(KanjiCharacter kanji)
    {
        if (TopicType != TopicType.Kanji)
            throw new DomainException("Cannot add kanji to non-kanji topic");
        
        _contentItems.Add(kanji);
    }
}

// Domain/Aggregates/ContentAggregate/Content Items
public interface IContent
{
    TopicId TopicId { get; }
    string Title { get; }
}

public class VocabularyItem : Entity, IContent
{
    public VocabularyId Id { get; private set; }
    public TopicId TopicId { get; private set; }
    public string KanjiForm { get; private set; }
    public string HiraganaForm { get; private set; }
    public string KatakanaForm { get; private set; }
    public string Romaji { get; private set; }
    public string Meaning { get; private set; }
    public string PartOfSpeech { get; private set; }
    public string? AudioUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public List<string> ExampleSentences { get; private set; } = new();
    public string Title => KanjiForm;

    public static VocabularyItem Create(TopicId topicId, string kanjiForm, string hiraganaForm,
        string katakanaForm, string romaji, string meaning, string partOfSpeech,
        string? audioUrl = null, string? imageUrl = null)
    {
        return new VocabularyItem
        {
            Id = VocabularyId.CreateNew(),
            TopicId = topicId,
            KanjiForm = kanjiForm,
            HiraganaForm = hiraganaForm,
            KatakanaForm = katakanaForm,
            Romaji = romaji,
            Meaning = meaning,
            PartOfSpeech = partOfSpeech,
            AudioUrl = audioUrl,
            ImageUrl = imageUrl
        };
    }

    public void AddExampleSentence(string sentence)
    {
        if (ExampleSentences.Count >= 10)
            throw new DomainException("Maximum 10 example sentences allowed");
        ExampleSentences.Add(sentence);
    }
}

public class GrammarRule : Entity, IContent
{
    public GrammarRuleId Id { get; private set; }
    public TopicId TopicId { get; private set; }
    public string RuleName { get; private set; }
    public string Explanation { get; private set; }
    public JlptLevel Level { get; private set; }
    public string? Usage { get; private set; }
    public List<string> ExampleSentences { get; private set; } = new();
    public List<GrammarRuleId> RelatedGrammarIds { get; private set; } = new();
    public string Title => RuleName;

    public static GrammarRule Create(TopicId topicId, string ruleName, string explanation,
        JlptLevel level, string? usage = null)
    {
        return new GrammarRule
        {
            Id = GrammarRuleId.CreateNew(),
            TopicId = topicId,
            RuleName = ruleName,
            Explanation = explanation,
            Level = level,
            Usage = usage
        };
    }
}

public class KanjiCharacter : Entity, IContent
{
    public KanjiId Id { get; private set; }
    public TopicId TopicId { get; private set; }
    public string Character { get; private set; }
    public string Radical { get; private set; }
    public int StrokeCount { get; private set; }
    public List<string> OnReadings { get; private set; } = new();
    public List<string> KunReadings { get; private set; } = new();
    public List<string> Meanings { get; private set; } = new();
    public List<string> ExampleWords { get; private set; } = new();
    public JlptLevel? JlptLevel { get; private set; }
    public int? KyoikuGrade { get; private set; }
    public string Title => Character;

    public static KanjiCharacter Create(TopicId topicId, string character, string radical,
        int strokeCount, JlptLevel? jlptLevel = null)
    {
        return new KanjiCharacter
        {
            Id = KanjiId.CreateNew(),
            TopicId = topicId,
            Character = character,
            Radical = radical,
            StrokeCount = strokeCount,
            JlptLevel = jlptLevel
        };
    }
}

// Value Objects for Content
public class CourseId : StronglyTypedId { public CourseId(Guid value) : base(value) { } public static CourseId CreateNew() => new(Guid.NewGuid()); }
public class LessonId : StronglyTypedId { public LessonId(Guid value) : base(value) { } public static LessonId CreateNew() => new(Guid.NewGuid()); }
public class TopicId : StronglyTypedId { public TopicId(Guid value) : base(value) { } public static TopicId CreateNew() => new(Guid.NewGuid()); }
public class VocabularyId : StronglyTypedId { public VocabularyId(Guid value) : base(value) { } public static VocabularyId CreateNew() => new(Guid.NewGuid()); }
public class GrammarRuleId : StronglyTypedId { public GrammarRuleId(Guid value) : base(value) { } public static GrammarRuleId CreateNew() => new(Guid.NewGuid()); }
public class KanjiId : StronglyTypedId { public KanjiId(Guid value) : base(value) { } public static KanjiId CreateNew() => new(Guid.NewGuid()); }

public enum TopicType
{
    Vocabulary,
    Grammar,
    Kanji
}
```

### Learning & Exercise Aggregates

```csharp
// Domain/Aggregates/ExerciseAggregate/Exercise.cs
namespace Domain.Aggregates.ExerciseAggregate;

public class Exercise : AggregateRoot
{
    public ExerciseId Id { get; private set; }
    public TopicId TopicId { get; private set; }
    public ExerciseType ExerciseType { get; private set; }
    public string Title { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public int? TimeLimit { get; private set; } // in minutes

    private List<Question> _questions = new();
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Exercise() { }

    public static Exercise Create(TopicId topicId, ExerciseType type, string title,
        Difficulty difficulty, int? timeLimit = null)
    {
        return new Exercise
        {
            Id = ExerciseId.CreateNew(),
            TopicId = topicId,
            ExerciseType = type,
            Title = title,
            Difficulty = difficulty,
            TimeLimit = timeLimit,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddQuestion(Question question)
    {
        if (_questions.Count >= 100)
            throw new DomainException("Exercise cannot have more than 100 questions");
        _questions.Add(question);
    }
}

// Domain/Aggregates/ExerciseAggregate/Question.cs
public class Question : Entity
{
    public QuestionId Id { get; private set; }
    public ExerciseId ExerciseId { get; private set; }
    public QuestionType QuestionType { get; private set; }
    public string QuestionText { get; private set; }
    public string? AudioUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public string CorrectAnswer { get; private set; }
    public string? Explanation { get; private set; }
    public int Points { get; private set; }
    public int OrderIndex { get; private set; }

    private List<QuestionOption> _options = new();
    public IReadOnlyList<QuestionOption> Options => _options.AsReadOnly();

    private Question() { }

    public static Question CreateMultipleChoice(ExerciseId exerciseId, string questionText,
        string correctAnswer, int points, int orderIndex, string? imageUrl = null)
    {
        return new Question
        {
            Id = QuestionId.CreateNew(),
            ExerciseId = exerciseId,
            QuestionType = QuestionType.MultipleChoice,
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Points = points,
            OrderIndex = orderIndex,
            ImageUrl = imageUrl
        };
    }

    public static Question CreateFillBlank(ExerciseId exerciseId, string questionText,
        string correctAnswer, int points, int orderIndex)
    {
        return new Question
        {
            Id = QuestionId.CreateNew(),
            ExerciseId = exerciseId,
            QuestionType = QuestionType.FillInTheBlank,
            QuestionText = questionText,
            CorrectAnswer = correctAnswer,
            Points = points,
            OrderIndex = orderIndex
        };
    }

    public static Question CreateListening(ExerciseId exerciseId, string audioUrl,
        string correctAnswer, int points, int orderIndex, string? questionText = null)
    {
        return new Question
        {
            Id = QuestionId.CreateNew(),
            ExerciseId = exerciseId,
            QuestionType = QuestionType.Listening,
            QuestionText = questionText ?? "Listen and select the correct answer",
            AudioUrl = audioUrl,
            CorrectAnswer = correctAnswer,
            Points = points,
            OrderIndex = orderIndex
        };
    }

    public void AddOption(QuestionOption option)
    {
        if (_options.Count >= 10)
            throw new DomainException("Question cannot have more than 10 options");
        _options.Add(option);
    }
}

// Domain/Aggregates/ExerciseAggregate/QuestionOption.cs
public class QuestionOption : ValueObject
{
    public string Text { get; }
    public bool IsCorrect { get; }
    public int OrderIndex { get; }

    public QuestionOption(string text, bool isCorrect, int orderIndex)
    {
        Text = text;
        IsCorrect = isCorrect;
        OrderIndex = orderIndex;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Text;
        yield return IsCorrect;
        yield return OrderIndex;
    }
}

// Value Objects
public class ExerciseId : StronglyTypedId { public ExerciseId(Guid value) : base(value) { } public static ExerciseId CreateNew() => new(Guid.NewGuid()); }
public class QuestionId : StronglyTypedId { public QuestionId(Guid value) : base(value) { } public static QuestionId CreateNew() => new(Guid.NewGuid()); }

public enum ExerciseType
{
    MultipleChoice,
    FillInTheBlank,
    Listening,
    Mixed
}

public enum QuestionType
{
    MultipleChoice,
    FillInTheBlank,
    Listening
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
```

### Learning Session & User Answers

```csharp
// Domain/Aggregates/UserSessionAggregate/UserSession.cs
namespace Domain.Aggregates.UserSessionAggregate;

public class UserSession : AggregateRoot
{
    public UserSessionId Id { get; private set; }
    public UserId UserId { get; private set; }
    public ExerciseId ExerciseId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public SessionStatus Status { get; private set; }

    private List<UserAnswer> _answers = new();
    public IReadOnlyList<UserAnswer> Answers => _answers.AsReadOnly();

    public int TotalPoints { get; private set; }
    public decimal PercentageScore { get; private set; }

    private UserSession() { }

    public static UserSession Create(UserId userId, ExerciseId exerciseId)
    {
        return new UserSession
        {
            Id = UserSessionId.CreateNew(),
            UserId = userId,
            ExerciseId = exerciseId,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.InProgress,
            TotalPoints = 0,
            PercentageScore = 0
        };
    }

    public void RecordAnswer(UserAnswer answer)
    {
        if (Status != SessionStatus.InProgress)
            throw new DomainException("Cannot record answers for completed session");
        
        _answers.Add(answer);
        AddDomainEvent(new AnswerRecordedEvent(Id, answer.QuestionId, answer.IsCorrect));
    }

    public void CompleteSession(List<Question> allQuestions)
    {
        Status = SessionStatus.Completed;
        EndTime = DateTime.UtcNow;

        // Calculate scores
        var correctAnswers = _answers.Count(a => a.IsCorrect);
        var maxPoints = allQuestions.Sum(q => q.Points);
        var earnedPoints = _answers.Where(a => a.IsCorrect).Sum(a => a.Points);

        TotalPoints = earnedPoints;
        PercentageScore = maxPoints > 0 ? (decimal)earnedPoints / maxPoints * 100 : 0;

        AddDomainEvent(new SessionCompletedEvent(Id, UserId, ExerciseId, PercentageScore, TotalPoints));
    }

    public bool IsTimeExpired(int? timeLimitMinutes)
    {
        if (!timeLimitMinutes.HasValue) return false;
        var elapsed = DateTime.UtcNow - StartTime;
        return elapsed.TotalMinutes > timeLimitMinutes.Value;
    }
}

// Domain/Aggregates/UserSessionAggregate/UserAnswer.cs
public class UserAnswer : Entity
{
    public UserAnswerId Id { get; private set; }
    public UserSessionId SessionId { get; private set; }
    public QuestionId QuestionId { get; private set; }
    public string UserResponse { get; private set; }
    public bool IsCorrect { get; private set; }
    public int Points { get; private set; }
    public int TimeSpentSeconds { get; private set; }
    public DateTime Timestamp { get; private set; }

    private UserAnswer() { }

    public static UserAnswer Create(UserSessionId sessionId, QuestionId questionId,
        string userResponse, bool isCorrect, int points, int timeSpentSeconds)
    {
        return new UserAnswer
        {
            Id = UserAnswerId.CreateNew(),
            SessionId = sessionId,
            QuestionId = questionId,
            UserResponse = userResponse,
            IsCorrect = isCorrect,
            Points = points,
            TimeSpentSeconds = timeSpentSeconds,
            Timestamp = DateTime.UtcNow
        };
    }
}

// Value Objects
public class UserSessionId : StronglyTypedId { public UserSessionId(Guid value) : base(value) { } public static UserSessionId CreateNew() => new(Guid.NewGuid()); }
public class UserAnswerId : StronglyTypedId { public UserAnswerId(Guid value) : base(value) { } public static UserAnswerId CreateNew() => new(Guid.NewGuid()); }

public enum SessionStatus
{
    InProgress,
    Completed,
    Failed,
    Abandoned
}
```

### Spaced Repetition (SRS) Aggregate

```csharp
// Domain/Aggregates/SrsAggregate/SrsCard.cs
namespace Domain.Aggregates.SrsAggregate;

public class SrsCard : AggregateRoot
{
    public SrsCardId Id { get; private set; }
    public UserId UserId { get; private set; }
    public VocabularyId VocabularyId { get; private set; }
    public SrsLevel SrsLevel { get; private set; }
    public decimal EaseFactor { get; private set; } // SM-2 algorithm
    public int IntervalDays { get; private set; }
    public int ReviewCount { get; private set; }
    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }
    public DateTime NextReviewDate { get; private set; }
    public DateTime? LastReviewDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private List<SrsReview> _reviews = new();
    public IReadOnlyList<SrsReview> Reviews => _reviews.AsReadOnly();

    private SrsCard() { }

    public static SrsCard Create(UserId userId, VocabularyId vocabularyId)
    {
        return new SrsCard
        {
            Id = SrsCardId.CreateNew(),
            UserId = userId,
            VocabularyId = vocabularyId,
            SrsLevel = SrsLevel.Level1,
            EaseFactor = 2.5m, // SM-2 default
            IntervalDays = 1,
            ReviewCount = 0,
            CorrectCount = 0,
            IncorrectCount = 0,
            NextReviewDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ReviewCard(int quality)
    {
        // SM-2 Algorithm Implementation
        if (quality < 3)
        {
            SrsLevel = SrsLevel.Level1;
            IntervalDays = 1;
            IncorrectCount++;
        }
        else
        {
            IncorrectCount = 0;
            if (SrsLevel == SrsLevel.Level1)
            {
                SrsLevel = SrsLevel.Level2;
                IntervalDays = 3;
            }
            else if (SrsLevel == SrsLevel.Level2)
            {
                SrsLevel = SrsLevel.Level3;
                IntervalDays = 7;
            }
            else if (SrsLevel < SrsLevel.Level8)
            {
                SrsLevel = (SrsLevel)((int)SrsLevel + 1);
                IntervalDays = (int)(IntervalDays * EaseFactor);
            }

            CorrectCount++;
        }

        // Update ease factor
        EaseFactor = Math.Max(1.3m, EaseFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m)));

        ReviewCount++;
        LastReviewDate = DateTime.UtcNow;
        NextReviewDate = DateTime.UtcNow.AddDays(IntervalDays);

        var review = SrsReview.Create(Id, quality, (SrsLevel)((int)SrsLevel - 1), SrsLevel);
        _reviews.Add(review);

        AddDomainEvent(new CardReviewedEvent(Id, UserId, VocabularyId, SrsLevel, quality));
    }
}

// Domain/Aggregates/SrsAggregate/SrsReview.cs
public class SrsReview : Entity
{
    public SrsReviewId Id { get; private set; }
    public SrsCardId CardId { get; private set; }
    public DateTime ReviewDate { get; private set; }
    public int Quality { get; private set; } // 0-5
    public SrsLevel PreviousLevel { get; private set; }
    public SrsLevel NewLevel { get; private set; }
    public int DurationSeconds { get; private set; }

    public static SrsReview Create(SrsCardId cardId, int quality, SrsLevel previousLevel, SrsLevel newLevel)
    {
        return new SrsReview
        {
            Id = SrsReviewId.CreateNew(),
            CardId = cardId,
            ReviewDate = DateTime.UtcNow,
            Quality = quality,
            PreviousLevel = previousLevel,
            NewLevel = newLevel,
            DurationSeconds = 0
        };
    }
}

// Value Objects
public class SrsCardId : StronglyTypedId { public SrsCardId(Guid value) : base(value) { } public static SrsCardId CreateNew() => new(Guid.NewGuid()); }
public class SrsReviewId : StronglyTypedId { public SrsReviewId(Guid value) : base(value) { } public static SrsReviewId CreateNew() => new(Guid.NewGuid()); }

public enum SrsLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7,
    Level8 = 8 // Mastered
}
```

### Progress Tracking Aggregate

```csharp
// Domain/Aggregates/ProgressAggregate/UserProgress.cs
namespace Domain.Aggregates.ProgressAggregate;

public class UserProgress : AggregateRoot
{
    public UserProgressId Id { get; private set; }
    public UserId UserId { get; private set; }
    public CourseId CourseId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastAccessDate { get; private set; }
    public decimal OverallPercentage { get; private set; }
    public int TotalTimeSpentMinutes { get; private set; }

    private List<LessonProgress> _lessonProgress = new();
    public IReadOnlyList<LessonProgress> LessonProgress => _lessonProgress.AsReadOnly();

    private UserProgress() { }

    public static UserProgress Create(UserId userId, CourseId courseId)
    {
        return new UserProgress
        {
            Id = UserProgressId.CreateNew(),
            UserId = userId,
            CourseId = courseId,
            StartDate = DateTime.UtcNow,
            LastAccessDate = DateTime.UtcNow,
            OverallPercentage = 0,
            TotalTimeSpentMinutes = 0
        };
    }

    public void RecordLessonProgress(LessonProgress progress)
    {
        _lessonProgress.Add(progress);
        LastAccessDate = DateTime.UtcNow;
        CalculateOverallProgress();
    }

    private void CalculateOverallProgress()
    {
        if (_lessonProgress.Count == 0)
            OverallPercentage = 0;
        else
            OverallPercentage = _lessonProgress.Average(lp => lp.Percentage);
    }

    public void AddTimeSpent(int minutes)
    {
        TotalTimeSpentMinutes += minutes;
    }
}

// Domain/Aggregates/ProgressAggregate/LessonProgress.cs
public class LessonProgress : Entity
{
    public LessonProgressId Id { get; private set; }
    public UserProgressId ProgressId { get; private set; }
    public LessonId LessonId { get; private set; }
    public decimal Percentage { get; private set; }
    public int TopicsCompleted { get; private set; }
    public int ExercisesCompleted { get; private set; }
    public int TimeSpentMinutes { get; private set; }
    public DateTime? LastAccessDate { get; private set; }

    public static LessonProgress Create(UserProgressId progressId, LessonId lessonId)
    {
        return new LessonProgress
        {
            Id = LessonProgressId.CreateNew(),
            ProgressId = progressId,
            LessonId = lessonId,
            Percentage = 0,
            TopicsCompleted = 0,
            ExercisesCompleted = 0,
            TimeSpentMinutes = 0
        };
    }

    public void UpdateProgress(int topicsTotal, int exercisesTotal)
    {
        Percentage = topicsTotal > 0 ? (decimal)(TopicsCompleted + ExercisesCompleted) / (topicsTotal + exercisesTotal) * 100 : 0;
        LastAccessDate = DateTime.UtcNow;
    }
}

// Value Objects
public class UserProgressId : StronglyTypedId { public UserProgressId(Guid value) : base(value) { } public static UserProgressId CreateNew() => new(Guid.NewGuid()); }
public class LessonProgressId : StronglyTypedId { public LessonProgressId(Guid value) : base(value) { } public static LessonProgressId CreateNew() => new(Guid.NewGuid()); }

// Domain/Aggregates/ProgressAggregate/UserStatistics.cs
public class UserStatistics : AggregateRoot
{
    public UserStatisticsId Id { get; private set; }
    public UserId UserId { get; private set; }
    public int TotalExercisesCompleted { get; private set; }
    public int TotalCorrectAnswers { get; private set; }
    public decimal AverageScore { get; private set; }
    public int TotalStudyTimeMinutes { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public DateTime LastStudyDate { get; private set; }
    public int TotalPoints { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public decimal CorrectPercentage => TotalExercisesCompleted > 0 
        ? (decimal)TotalCorrectAnswers / TotalExercisesCompleted * 100 
        : 0;

    public static UserStatistics Create(UserId userId)
    {
        return new UserStatistics
        {
            Id = UserStatisticsId.CreateNew(),
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void RecordSessionCompletion(decimal score, int points, int timeSpentMinutes, int correctAnswers, int totalAnswers)
    {
        TotalExercisesCompleted++;
        TotalCorrectAnswers += correctAnswers;
        TotalStudyTimeMinutes += timeSpentMinutes;
        TotalPoints += points;

        AverageScore = (AverageScore * (TotalExercisesCompleted - 1) + score) / TotalExercisesCompleted;

        UpdateStreak();
        LastStudyDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateStreak()
    {
        if ((DateTime.UtcNow - LastStudyDate).TotalDays <= 1)
        {
            CurrentStreak++;
            if (CurrentStreak > LongestStreak)
                LongestStreak = CurrentStreak;
        }
        else
        {
            CurrentStreak = 1;
        }
    }
}

public class UserStatisticsId : StronglyTypedId { public UserStatisticsId(Guid value) : base(value) { } public static UserStatisticsId CreateNew() => new(Guid.NewGuid()); }
```
