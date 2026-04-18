# Japanese Learning Application - Complete System Design

## ?? Executive Summary

A comprehensive backend system design for a Japanese language learning application using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The system supports millions of users with high concurrency, utilizing modern .NET 10 technologies, EF Core, and Redis caching.

**Key Features:**
- User authentication and progress tracking
- Hierarchical content structure (Courses ? Lessons ? Topics ? Content)
- Multiple exercise types (MCQ, Fill-in-the-blank, Listening)
- Spaced Repetition System (SRS) with SM-2 algorithm
- Real-time progress analytics
- High-concurrency support (100K+ concurrent users)
- Cache-first architecture

---

## ??? Architecture Overview

### Bounded Contexts

1. **User Management Context**
   - User authentication, registration, profiles
   - Subscription management
   - User preferences and settings

2. **Content Management Context**
   - Course hierarchy management
   - Vocabulary, Grammar, Kanji content
   - Content versioning and publishing

3. **Learning Context**
   - Exercise management
   - Question and answer tracking
   - Session management
   - Performance scoring

4. **Spaced Repetition (SRS) Context**
   - Flashcard management
   - Review scheduling (SM-2 algorithm)
   - Review history and analytics

5. **Progress Tracking Context**
   - User progress per course/lesson
   - Statistics aggregation
   - Streak tracking
   - Leaderboards (future)

### Aggregates

Each bounded context has clear aggregate roots:

```
UserAggregate
  ?? User (Root)
  ?? UserProfile (Value Object)
  ?? Subscription (Value Object)

CourseAggregate
  ?? Course (Root)
  ?? Lesson (Entity)
  ?? Topic (Entity)
  ?? Content Items (VocabularyItem, GrammarRule, KanjiCharacter)

ExerciseAggregate
  ?? Exercise (Root)
  ?? Question (Entity)
  ?? QuestionOption (Value Object)

UserSessionAggregate
  ?? UserSession (Root)
  ?? UserAnswer (Entity)

SrsAggregate
  ?? SrsCard (Root)
  ?? SrsReview (Entity)

ProgressAggregate
  ?? UserProgress (Root)
  ?? LessonProgress (Entity)
  ?? UserStatistics (Root)
```

---

## ??? Database Design

### Key Tables

**User Management**
- Users: Core user data (email, password hash, profile)
- UserProfiles: Extended profile (bio, avatar, preferences)
- Subscriptions: Subscription plans and expiry

**Content**
- Courses: Japanese language courses (N5-N1 levels)
- Lessons: Course subdivisions
- Topics: Lesson subdivisions (Vocabulary, Grammar, Kanji)
- VocabularyItems: Japanese words (kanji, hiragana, katakana, romaji, meaning)
- GrammarRules: Grammar explanations and examples
- KanjiCharacters: Kanji information (radicals, readings, meanings)

**Learning**
- Exercises: Collections of questions
- Questions: Individual exercise items
- QuestionOptions: MCQ choices
- UserSessions: Exercise attempts
- UserAnswers: Individual question responses

**Progress**
- UserProgress: Course-level progress
- LessonProgress: Lesson-level progress
- UserStatistics: Aggregated statistics (scores, time, streaks)

**SRS**
- SrsCards: Individual flashcards
- SrsReviews: Review history
- SrsSchedules: Review scheduling data

### Indexing & Performance

- Composite indexes on foreign key + commonly filtered columns
- Covering indexes on frequently queried columns
- Partitioning for high-volume tables (UserSessions, UserAnswers)
- Query result caching with Redis

---

## ??? Project Structure

### Clean Architecture Layers

```
Presentation Layer (Controllers, DTOs, Validators)
        ? (depends on)
Application Layer (Commands, Queries, Services)
        ? (depends on)
Domain Layer (Entities, Aggregates, Value Objects)
        ? (referenced by)
Infrastructure Layer (Repositories, DbContext, External Services)
```

### Directory Organization

```
Domain/                          # Core business logic
??? Aggregates/                 # Domain aggregates
??? Common/                      # Base classes
??? Events/                      # Domain events

Application/                     # Business operations (CQRS)
??? Features/
?   ??? Auth/
?   ??? Courses/
?   ??? Exercises/
?   ??? SRS/
?   ??? Progress/
??? Common/
?   ??? Interfaces/             # Repository, service interfaces
?   ??? DTOs/                   # Data transfer objects
?   ??? Exceptions/             # Application exceptions
?   ??? Behaviors/              # MediatR pipelines

Infrastructure/                  # External dependencies
??? Persistence/
?   ??? Repositories/
?   ??? Configurations/
?   ??? Migrations/
??? Authentication/
??? ExternalServices/
??? Caching/
??? Logging/

Presentation/                    # API layer
??? Controllers/
??? Middleware/
??? Validators/
??? Filters/
??? Mappings/
```

---

## ?? Key Technologies

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Runtime | .NET 10 | Latest LTS framework |
| ORM | EF Core 9 | Database access |
| Database | SQL Server / PostgreSQL | Data persistence |
| Cache | Redis | High-speed caching |
| API | ASP.NET Core 10 | Web framework |
| CQRS | MediatR | Command/Query separation |
| Validation | FluentValidation | Input validation |
| Mapping | AutoMapper | DTO mapping |
| Logging | Serilog | Structured logging |
| Auth | JWT | Token-based authentication |
| DDD | Domain events | Event sourcing ready |

---

## ?? API Design

### REST Endpoints

```
Authentication
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh-token
POST   /api/auth/logout

Courses
GET    /api/courses
GET    /api/courses/{courseId}
POST   /api/courses                          [Admin]
GET    /api/courses/{courseId}/lessons
GET    /api/lessons/{lessonId}
GET    /api/lessons/{lessonId}/topics/{topicId}

Exercises
GET    /api/topics/{topicId}/exercises
POST   /api/exercises/{exerciseId}/start
POST   /api/sessions/{sessionId}/answers
POST   /api/sessions/{sessionId}/complete
GET    /api/exercises/{exerciseId}/stats

SRS
GET    /api/srs/schedule
GET    /api/srs/cards/due
POST   /api/srs/cards/{cardId}/review
POST   /api/srs/cards
GET    /api/srs/cards/{cardId}/history

Progress
GET    /api/progress/courses/{courseId}
GET    /api/progress/statistics
GET    /api/progress/streak

Users
GET    /api/users/me
PUT    /api/users/me
POST   /api/users/me/change-password
```

### Response Format

```json
{
  "success": true,
  "statusCode": 200,
  "data": { /* response data */ },
  "message": "Operation successful",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## ?? Security

### Authentication & Authorization

- JWT tokens (access + refresh)
- Password hashing (BCrypt/Argon2)
- Role-based access control (User, Premium, Admin)
- Rate limiting per user
- CORS policy configuration

### Data Protection

- Encrypted password storage
- HTTPS enforcement
- CSRF token validation
- SQL injection prevention (parameterized queries)
- XSS protection headers

---

## ? Performance & Scalability

### Caching Strategy

**Level 1: Static Content (24 hours)**
- Courses, lessons, topics
- Vocabulary, grammar, kanji
- Exercise questions

**Level 2: User Data (2 hours)**
- User progress
- Statistics
- Preferences

**Level 3: Session Data (30 minutes)**
- SRS schedule
- Active sessions
- Recent answers

### Optimization Techniques

- Database connection pooling (100 min, 200 max)
- Batch processing for bulk operations
- Pagination on all list endpoints
- Projection queries (select only needed fields)
- Lazy loading with explicit includes
- Async/await for non-blocking I/O

### Concurrency Support

- Optimistic locking (versioning)
- Row-level isolation levels
- Redis distributed locks for critical operations
- Async request handling
- Task.WhenAll for parallel operations

---

## ?? Spaced Repetition System

### SM-2 Algorithm Implementation

```
EaseFactor = max(1.3, EF + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02)))
Interval = 1 day (first), 3 days, 7 days, ...
NextReview = Today + Interval * EaseFactor
```

### SRS Levels

| Level | Meaning | Interval |
|-------|---------|----------|
| 1 | New card | 1 day |
| 2 | Learning | 3 days |
| 3 | Reviewing | 7 days |
| 4 | Known | 14 days |
| 5 | Well-known | 30 days |
| 6 | Very known | 60 days |
| 7 | Expert | 120 days |
| 8 | Mastered | 365+ days |

---

## ?? Testing Strategy

### Unit Tests
- Domain entities and value objects
- Application commands and queries
- Services and utilities
- Repository implementations

### Integration Tests
- End-to-end API flows
- Database operations
- Cache operations
- Authentication flows

### Performance Tests
- Load testing (Apache JMeter, k6)
- Stress testing database
- Cache performance
- API response time benchmarks

---

## ?? Monitoring & Diagnostics

### Metrics to Track

- API response time (p50, p95, p99)
- Database query performance
- Cache hit ratio
- Error rates by endpoint
- User session count
- SRS cards due
- Progress completion rates

### Logging

- Structured logging (JSON)
- Request/response logging
- Error tracking
- Performance monitoring
- User action audit trail

### Health Checks

```csharp
GET /health/live   - Liveness check
GET /health/ready  - Readiness check
```

---

## ?? Deployment

### Containerization

- Docker image with multi-stage build
- Docker Compose for local development
- Container registry (Docker Hub, ACR)

### Orchestration

- Kubernetes deployment
- Auto-scaling based on CPU/memory
- Service mesh (Istio) for advanced scenarios
- GitOps for deployment automation

### Database

- SQL Server on Azure (managed)
- Automated backups
- Point-in-time recovery
- Read replicas for scale-out

### CI/CD

- GitHub Actions
- Automated testing
- Build and push to registry
- Rolling deployment strategy

---

## ?? Future Enhancements

### Phase 2
- AI-powered recommendation engine
- Adaptive learning paths
- Pronunciation feedback (Azure Speech)
- Video content integration
- Community features (forums, chat)

### Phase 3
- Mobile apps (iOS, Android)
- Offline-first architecture
- Peer-to-peer learning
- Live lessons with instructors
- Advanced analytics dashboard

### Phase 4
- Multi-language support
- Gamification (achievements, badges)
- Social features (friends, competitions)
- AI tutor chatbot
- Integration with JLPT practice

---

## ?? Design Patterns Used

| Pattern | Purpose | Location |
|---------|---------|----------|
| **DDD** | Domain modeling | Domain layer |
| **CQRS** | Query/Command separation | Application layer |
| **Repository** | Data access abstraction | Infrastructure layer |
| **Specification** | Reusable query specifications | Infrastructure layer |
| **Mediator** | Request handling | Application layer |
| **Decorator** | Cross-cutting concerns | Application behaviors |
| **Strategy** | SRS algorithm | SRS service |
| **Factory** | Entity creation | Aggregate roots |
| **Value Object** | Immutable data | Domain layer |
| **Event Sourcing** | Event-based state | Ready for future |

---

## ?? Key Design Decisions

1. **Aggregate per Bounded Context**: Each context has independent aggregates enabling parallel development

2. **CQRS Pattern**: Separate read and write models allowing different optimization strategies

3. **Event-Driven Architecture**: Domain events enable loose coupling and audit trails

4. **Cache-First Approach**: Redis caching reduces database load significantly

5. **Strongly-Typed IDs**: Type-safe aggregate identifiers prevent mixing up IDs

6. **Value Objects**: Encapsulate business rules (Email, Password, JlptLevel)

7. **Pagination Everywhere**: Prevent memory issues with large datasets

8. **Async by Default**: Non-blocking I/O maximizes throughput

---

## ?? Implementation Checklist

### Phase 1: Core
- [x] Domain layer with aggregates
- [x] Application layer with CQRS
- [x] Database schema design
- [x] Repository pattern implementation
- [x] Authentication & authorization
- [ ] API endpoints implementation
- [ ] Unit tests (70% coverage)
- [ ] Integration tests
- [ ] Docker containerization
- [ ] Kubernetes manifests

### Phase 2: Enhancement
- [ ] Redis caching implementation
- [ ] Performance optimization
- [ ] Load testing
- [ ] Monitoring setup
- [ ] CI/CD pipeline
- [ ] Documentation

### Phase 3: Production
- [ ] Security audit
- [ ] Performance tuning
- [ ] Disaster recovery
- [ ] Scaling testing
- [ ] Production deployment

---

## ?? References

### Books
- "Clean Architecture" by Robert C. Martin
- "Domain-Driven Design" by Eric Evans
- "Implementing Domain-Driven Design" by Vaughn Vernon

### Articles
- Microsoft Docs: Clean Architecture
- DDD Community Resources
- CQRS Pattern Guide

### Tools
- .NET 10 documentation
- Entity Framework Core docs
- Redis documentation
- Kubernetes docs

---

## ?? Support & Questions

For architecture questions or clarifications:
1. Review the relevant design document
2. Check code examples in entity design
3. Refer to API documentation
4. Consult testing guidelines

---

**Status**: ? Design Complete
**Last Updated**: January 2024
**Version**: 1.0

---
