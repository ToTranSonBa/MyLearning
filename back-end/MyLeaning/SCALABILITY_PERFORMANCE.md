# Scalability & Performance Design

## 1. Database Optimization

### Indexing Strategy

```sql
-- User Management
CREATE INDEX idx_user_email ON [Users] (Email) WHERE Email IS NOT NULL;
CREATE INDEX idx_user_username ON [Users] (Username) WHERE Username IS NOT NULL;
CREATE INDEX idx_user_created ON [Users] (CreatedAt DESC);

-- Course Content
CREATE INDEX idx_course_level ON [Courses] (Level);
CREATE INDEX idx_lesson_course ON [Lessons] (CourseId, OrderIndex);
CREATE INDEX idx_topic_lesson ON [Topics] (LessonId, OrderIndex);
CREATE INDEX idx_vocabulary_topic ON [VocabularyItems] (TopicId);
CREATE INDEX idx_grammar_topic ON [GrammarRules] (TopicId);
CREATE INDEX idx_kanji_topic ON [KanjiCharacters] (TopicId);

-- Learning & Exercises
CREATE INDEX idx_exercise_topic ON [Exercises] (TopicId);
CREATE INDEX idx_question_exercise ON [Questions] (ExerciseId, OrderIndex);
CREATE INDEX idx_session_user ON [UserSessions] (UserId, ExerciseId, CreatedAt DESC);
CREATE INDEX idx_session_status ON [UserSessions] (UserId, Status, EndTime);
CREATE INDEX idx_answer_session ON [UserAnswers] (SessionId);
CREATE NONCLUSTERED INDEX idx_answer_question ON [UserAnswers] (QuestionId);

-- Progress Tracking
CREATE INDEX idx_progress_user_course ON [UserProgress] (UserId, CourseId);
CREATE INDEX idx_progress_percentage ON [UserProgress] (UserId, CourseId, OverallPercentage);
CREATE INDEX idx_lesson_progress_user ON [LessonProgress] (ProgressId, LessonId);
CREATE NONCLUSTERED INDEX idx_statistics_user ON [UserStatistics] (UserId);
CREATE INDEX idx_statistics_date ON [UserStatistics] (LastStudyDate DESC);

-- SRS
CREATE INDEX idx_srs_card_user ON [SrsCards] (UserId, NextReviewDate);
CREATE INDEX idx_srs_card_vocab ON [SrsCards] (UserId, VocabularyId, SrsLevel);
CREATE INDEX idx_srs_due ON [SrsCards] (UserId) 
  INCLUDE (NextReviewDate, VocabularyId, SrsLevel)
  WHERE NextReviewDate <= GETUTCDATE();
CREATE INDEX idx_srs_review_card ON [SrsReviews] (CardId, ReviewDate DESC);
```

### Partitioning Strategy

For high-volume tables (100M+ rows):

```sql
-- Partition UserSessions by date (daily partitions)
CREATE PARTITION FUNCTION DatePartitionFunction (datetime2)
AS RANGE RIGHT FOR VALUES
('2024-01-01', '2024-01-02', ... , '2024-12-31');

CREATE PARTITION SCHEME DatePartitionScheme
AS PARTITION DatePartitionFunction
TO (Filegroup1, Filegroup2, ... , FileGroupN);

CREATE CLUSTERED INDEX idx_session_date ON [UserSessions] (CreatedAt)
ON DatePartitionScheme (CreatedAt);

-- Partition UserAnswers by quarter
CREATE PARTITION FUNCTION QuarterPartition (datetime2)
AS RANGE RIGHT FOR VALUES ('2024-01-01', '2024-04-01', '2024-07-01', '2024-10-01');
```

### Query Optimization

```csharp
// Use pagination for large datasets
public async Task<PagedResult<CourseDto>> GetCoursesAsync(int page, int pageSize)
{
    return await _courseRepository.GetAsync(
        query => query
            .Include(c => c.Lessons)
            .Skip((page - 1) * pageSize)
            .Take(pageSize),
        trackChanges: false
    );
}

// Use projections to reduce data transfer
var courses = await _context.Courses
    .Select(c => new CourseDto
    {
        Id = c.Id,
        Title = c.Title,
        Level = c.Level,
        // Only select needed fields
    })
    .ToListAsync();

// Batch operations
public async Task UpdateBulkUserProgressAsync(List<UserProgress> progressList)
{
    const int batchSize = 1000;
    for (int i = 0; i < progressList.Count; i += batchSize)
    {
        var batch = progressList.Skip(i).Take(batchSize).ToList();
        _context.UpdateRange(batch);
        await _context.SaveChangesAsync();
    }
}

// Async queries
var users = await _userRepository.GetAsync(
    predicate: u => u.CreatedAt > DateTime.UtcNow.AddDays(-30),
    orderBy: u => u.OrderBy(x => x.CreatedAt),
    trackChanges: false
);
```

---

## 2. Caching Strategy

### Redis Cache Layers

```csharp
public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private const string CACHE_KEY_PREFIX = "jl:";
    private const int DEFAULT_TTL_HOURS = 24;

    // Level 1: Course Content (long-lived)
    public async Task<Course?> GetCourseAsync(CourseId courseId)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}course:{courseId}";
        var cached = await _redis.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<Course>(cached);

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            await _redis.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(course),
                expiry: TimeSpan.FromHours(24)
            );
        }
        return course;
    }

    // Level 2: User Progress (medium-lived)
    public async Task<UserProgress?> GetUserProgressAsync(UserId userId, CourseId courseId)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}progress:{userId}:{courseId}";
        var cached = await _redis.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<UserProgress>(cached);

        var progress = await _progressRepository.GetProgressAsync(userId, courseId);
        if (progress != null)
        {
            await _redis.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(progress),
                expiry: TimeSpan.FromHours(2)
            );
        }
        return progress;
    }

    // Level 3: SRS Schedule (short-lived, invalidated on review)
    public async Task<SrsSchedule?> GetSrsScheduleAsync(UserId userId)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}srs_schedule:{userId}";
        var cached = await _redis.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<SrsSchedule>(cached);

        var schedule = await _srsRepository.GetScheduleAsync(userId);
        if (schedule != null)
        {
            await _redis.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(schedule),
                expiry: TimeSpan.FromMinutes(30)
            );
        }
        return schedule;
    }

    // Cache invalidation
    public async Task InvalidateUserCacheAsync(UserId userId)
    {
        var pattern = $"{CACHE_KEY_PREFIX}progress:{userId}:*";
        await InvalidateByPatternAsync(pattern);
    }

    public async Task InvalidateSrsScheduleAsync(UserId userId)
    {
        await _redis.KeyDeleteAsync($"{CACHE_KEY_PREFIX}srs_schedule:{userId}");
    }
}

public class CacheKeys
{
    // Content (24 hours)
    public static string CourseKey(CourseId id) => $"jl:course:{id}";
    public static string LessonKey(LessonId id) => $"jl:lesson:{id}";
    public static string TopicKey(TopicId id) => $"jl:topic:{id}";
    public static string VocabKey(VocabularyId id) => $"jl:vocab:{id}";

    // Progress (2 hours)
    public static string UserProgressKey(UserId userId, CourseId courseId) 
        => $"jl:progress:{userId}:{courseId}";
    public static string StatisticsKey(UserId userId) => $"jl:stats:{userId}";

    // SRS (30 minutes, invalidated on review)
    public static string SrsScheduleKey(UserId userId) => $"jl:srs_schedule:{userId}";
    public static string SrsCardKey(SrsCardId cardId) => $"jl:srs_card:{cardId}";
    public static string SrsDueKey(UserId userId) => $"jl:srs_due:{userId}";

    // Session (10 minutes)
    public static string SessionKey(UserSessionId sessionId) => $"jl:session:{sessionId}";

    // User (6 hours)
    public static string UserKey(UserId userId) => $"jl:user:{userId}";
}
```

### Cache Invalidation Patterns

```csharp
// Event-based cache invalidation
public class UserAnswerRecordedEventHandler : INotificationHandler<AnswerRecordedEvent>
{
    private readonly ICacheService _cacheService;

    public async Task Handle(AnswerRecordedEvent notification, CancellationToken cancellationToken)
    {
        // Invalidate relevant caches
        await _cacheService.InvalidateSessionCacheAsync(notification.SessionId);
        await _cacheService.InvalidateStatisticsAsync(notification.UserId);
    }
}

// Background cache warming
public class CacheWarmingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Warm popular courses at off-peak hours
            var popularCourses = await _courseRepository.GetMostPopularAsync(topN: 10);
            foreach (var course in popularCourses)
            {
                await _cacheService.WarmCourseAsync(course.Id);
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
```

---

## 3. Database Connection Pooling

```csharp
// Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.MinBatchSize(100);
            sqlOptions.MaxBatchSize(200);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 5,
                errorNumbersToAdd: null
            );
        }
    ),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton
);

// Connection string with pooling
"Server=localhost;Database=JapaneseLearning;Integrated Security=true;
Max Pool Size=100;Min Pool Size=5;Pooling=true;
Connection Lifetime=300;Connection Idle Timeout=180;"
```

---

## 4. Async/Await & Concurrency

```csharp
// Proper async patterns
public async Task<UserProgress> UpdateProgressAsync(UserId userId, CourseId courseId)
{
    var progress = await _progressRepository.GetAsync(
        p => p.UserId == userId && p.CourseId == courseId,
        trackChanges: true
    );

    if (progress == null)
        throw new NotFoundException("Progress not found");

    // Optimistic locking
    try
    {
        progress.RecordLessonCompletion();
        await _unitOfWork.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        throw new ConflictException("Progress was modified by another operation");
    }

    return progress;
}

// Batch async operations
public async Task<List<SrsCard>> ProcessDueCardsAsync(UserId userId, int batchSize = 10)
{
    var dueCards = await _srsRepository.GetDueCardsAsync(userId);
    var tasks = dueCards
        .Chunk(batchSize)
        .Select(batch => ProcessCardBatchAsync(batch));
    
    await Task.WhenAll(tasks);
    return dueCards;
}

// Request throttling
public class ThrottlingMiddleware
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        var key = $"rate_limit:{userId}";
        var count = await redis.GetDatabaseAsync().StringIncrementAsync(key);

        if (count == 1)
            await redis.GetDatabaseAsync().KeyExpireAsync(key, TimeSpan.FromSeconds(60));

        if (count > 1000) // 1000 requests per minute
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        await _next(context);
    }
}
```

---

## 5. Load Balancing & Horizontal Scaling

### Docker & Kubernetes

```yaml
# docker-compose.yml
version: '3.9'
services:
  api_1:
    image: japanese-learning-api:latest
    ports:
      - "5001:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=JapaneseLearning...
      - Redis__Host=redis
    depends_on:
      - db
      - redis

  api_2:
    image: japanese-learning-api:latest
    ports:
      - "5002:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=JapaneseLearning...
      - Redis__Host=redis
    depends_on:
      - db
      - redis

  load_balancer:
    image: nginx:latest
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api_1
      - api_2

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: YourStrong@Password
      MSSQL_PID: Standard
    volumes:
      - db_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  db_data:
  redis_data:
```

```yaml
# kubernetes-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: japanese-learning-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: japanese-learning-api
  template:
    metadata:
      labels:
        app: japanese-learning-api
    spec:
      containers:
      - name: api
        image: japanese-learning-api:latest
        ports:
        - containerPort: 80
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
---
apiVersion: v1
kind: Service
metadata:
  name: japanese-learning-api-service
spec:
  selector:
    app: japanese-learning-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

---

## 6. Monitoring & Performance Metrics

```csharp
// Performance monitoring
public class PerformanceMonitoringBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();
            timer.Stop();

            if (timer.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Long-running request: {RequestName} took {ElapsedMilliseconds}ms",
                    typeof(TRequest).Name,
                    timer.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(
                ex,
                "Request failed: {RequestName} after {ElapsedMilliseconds}ms",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds);
            throw;
        }
    }
}

// Application Insights
services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableRequestTrackingTelemetryModule = true;
});

// Custom metrics
public class CustomMetrics
{
    private static readonly Counter ExerciseCompletions = Counter
        .Create("exercises_completed_total", "Total exercises completed");

    private static readonly Gauge ActiveSessions = Gauge
        .Create("active_sessions", "Number of active learning sessions");

    private static readonly Histogram SessionDuration = Histogram
        .Create("session_duration_seconds", "Exercise session duration");
}
```

---

## 7. Performance Targets

| Metric | Target |
|--------|--------|
| API Response Time (p95) | < 500ms |
| API Response Time (p99) | < 1000ms |
| Database Query Time (p95) | < 100ms |
| Cache Hit Ratio | > 80% |
| Throughput | > 10,000 req/sec |
| Error Rate | < 0.1% |
| Availability | 99.9% SLA |
| Max Concurrent Users | 100,000 |
| Data Recovery Time | < 1 hour |

---

## 8. Future Scalability Considerations

### CQRS Implementation for Writes/Reads Separation
```csharp
// Separate read models from write models
// WriteDb: Normalized, ACID, optimized for writes
// ReadDb: Denormalized, optimized for reads (updated via events)
```

### Event Sourcing
```csharp
// Store all domain events
// Replay events to reconstruct state
// Temporal queries
// Complete audit trail
```

### Microservices Architecture
```
- User Management Service
- Content Management Service  
- Learning Service
- SRS Service
- Progress Service
- Notification Service
```

### Elastic Search for Full-text Search
```csharp
// Index all vocabulary, grammar, content
// Fast fuzzy search
// Auto-complete
```

### CDN for Static Assets
```
- Course images, audio, video
- Frontend assets
- API documentation
```
