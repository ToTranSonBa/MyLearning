# COURSE Module - Vertical Slice CQRS Architecture

## Overview

The COURSE module implements Japanese language course management following strict Clean Architecture + CQRS patterns according to Rules.md.

## Features

- ? Create courses with validation
- ? Get all active courses (optionally filtered by level)
- ? Get course details by ID
- ? JLPT Level filtering (N5-N1)
- ? Business rule enforcement via domain entity
- ? Comprehensive input validation

## Architecture

### Layer Breakdown

```
Domain Layer (Domain\Courses\)
??? Course.cs          (Aggregate Root - enforces business rules)
??? CourseLevel.cs     (Value Object - enum)

Application Layer (Application\Features\Courses\)
??? Commands\
?   ??? CreateCourseCommand.cs + Handler
??? Queries\
?   ??? GetCoursesQuery.cs + Handler
?   ??? GetCourseByIdQuery.cs + Handler
??? Validators\
?   ??? CreateCourseValidator.cs
??? DTOs (Application\DTOs\CourseDto\)
    ??? CreateCourseDto.cs
    ??? CourseDto.cs

Interfaces (Application\Common\Interfaces\)
??? ICourseRepository.cs

Infrastructure Layer (Infrastructure.SqlServer\)
??? Repositories\
?   ??? CourseRepository.cs (implements ICourseRepository)
??? Mappings\
    ??? CourseMapping.cs (EF Core configuration)

Presentation Layer (Web\Controllers\)
??? CoursesController.cs (REST endpoints)
```

## CQRS Flow

### Create Course Command Flow
```
POST /api/courses (CreateCourseDto)
  ?
CoursesController.CreateCourse()
  ?
CreateCourseCommand
  ?
CreateCourseHandler
  ?
ICourseRepository (abstraction, not implementation)
  ?
CourseRepository (Infrastructure implementation)
  ?
ApplicationDbContext ? SQL Server
  ?
Returns CourseDto (201 Created)
```

### Get Courses Query Flow
```
GET /api/courses?level=N5
  ?
CoursesController.GetCourses()
  ?
GetCoursesQuery
  ?
GetCoursesHandler
  ?
ICourseRepository
  ?
CourseRepository
  ?
ApplicationDbContext ? SQL Server
  ?
Returns List<CourseDto> (200 OK)
```

### Get Course By ID Query Flow
```
GET /api/courses/{id}
  ?
CoursesController.GetCourseById(id)
  ?
GetCourseByIdQuery
  ?
GetCourseByIdHandler
  ?
ICourseRepository
  ?
CourseRepository
  ?
ApplicationDbContext ? SQL Server
  ?
Returns CourseDto (200 OK) or 404 Not Found
```

## API Endpoints

### 1. Create Course
```http
POST /api/courses
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Beginner Japanese",
  "description": "Comprehensive guide to learning Japanese for complete beginners",
  "level": 5,
  "imageUrl": "https://example.com/course-n5.jpg",
  "instructorName": "Tanaka Sensei",
  "estimatedDurationHours": 40
}

Response (201 Created):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Beginner Japanese",
  "description": "Comprehensive guide to learning Japanese for complete beginners",
  "level": 5,
  "imageUrl": "https://example.com/course-n5.jpg",
  "isActive": true,
  "totalLessons": 0,
  "estimatedDurationHours": 40,
  "instructorName": "Tanaka Sensei",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}

Error (400 Bad Request - Duplicate Title):
{
  "message": "A course with title 'Beginner Japanese' already exists."
}

Error (400 Bad Request - Validation):
{
  "message": "Course title must be at least 3 characters.; Course description must be at least 10 characters."
}

Error (401 Unauthorized):
{
  "message": "Unauthorized"
}
```

### 2. Get All Courses
```http
GET /api/courses
GET /api/courses?level=N5

Response (200 OK):
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Beginner Japanese",
    "description": "Comprehensive guide...",
    "level": 5,
    "imageUrl": "https://example.com/course-n5.jpg",
    "isActive": true,
    "totalLessons": 0,
    "estimatedDurationHours": 40,
    "instructorName": "Tanaka Sensei",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null
  },
  {
    "id": "660e9500-f30c-52e5-b827-557766551111",
    "title": "Elementary Japanese",
    "description": "Build on basics...",
    "level": 4,
    "imageUrl": "https://example.com/course-n4.jpg",
    "isActive": true,
    "totalLessons": 12,
    "estimatedDurationHours": 50,
    "instructorName": "Yamamoto Sensei",
    "createdAt": "2024-01-16T12:00:00Z",
    "updatedAt": null
  }
]
```

### 3. Get Course by ID
```http
GET /api/courses/550e8400-e29b-41d4-a716-446655440000

Response (200 OK):
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Beginner Japanese",
  "description": "Comprehensive guide...",
  "level": 5,
  "imageUrl": "https://example.com/course-n5.jpg",
  "isActive": true,
  "totalLessons": 10,
  "estimatedDurationHours": 40,
  "instructorName": "Tanaka Sensei",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}

Error (404 Not Found):
{
  "message": "Course not found."
}
```

## Validation Rules

### Title
- Required
- 3-200 characters
- Only alphanumeric, spaces, hyphens, parentheses, ampersand, apostrophe, period

### Description
- Required
- 10-2000 characters

### Level
- Required
- Must be valid CourseLevel enum (N5, N4, N3, N2, N1)

### ImageUrl (Optional)
- Must be valid absolute URI if provided

### InstructorName (Optional)
- Max 100 characters

### EstimatedDurationHours
- Non-negative
- Max 1000 hours

## Database Schema

```sql
CREATE TABLE [dbo].[Courses] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
    [Title] NVARCHAR(200) UNIQUE NOT NULL,
    [Description] NVARCHAR(2000) NOT NULL,
    [Level] INT NOT NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [TotalLessons] INT NOT NULL DEFAULT 0,
    [EstimatedDurationHours] INT NOT NULL DEFAULT 0,
    [InstructorName] NVARCHAR(100) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NULL
);

-- Indexes
CREATE UNIQUE INDEX IDX_Courses_Title_Unique ON [dbo].[Courses]([Title]);
CREATE INDEX IDX_Courses_Level ON [dbo].[Courses]([Level]);
CREATE INDEX IDX_Courses_IsActive ON [dbo].[Courses]([IsActive]);
CREATE INDEX IDX_Courses_Active_Level ON [dbo].[Courses]([IsActive], [Level]);
CREATE INDEX IDX_Courses_CreatedAt ON [dbo].[Courses]([CreatedAt]);
```

## Clean Architecture Compliance

### ? Dependency Rule Enforcement
- Domain (no dependencies) ? Contains Course, CourseLevel
- Application (depends only on Domain) ? Commands, Queries, Validators, DTOs, Interfaces
- Infrastructure (depends on Application) ? Repository, EF Mapping, DB access
- Presentation (depends on Application) ? Controller, uses MediatR

### ? CQRS Strict Implementation
- **Commands**: CreateCourseCommand (write operations)
- **Queries**: GetCoursesQuery, GetCourseByIdQuery (read operations)
- **Handlers**: Each command/query has dedicated handler with single responsibility
- **No Handler Mixing**: Queries never modify state, Commands never return collections

### ? Separation of Concerns
- Business logic in Domain (Course.Create, Course.Update)
- Use case orchestration in Application (Handlers)
- Data access in Infrastructure (CourseRepository)
- HTTP concerns in Presentation (CoursesController)

### ? Testability
- All dependencies are interfaces (ICourseRepository, IUnitOfWork)
- Easy to mock for unit testing
- Commands/Queries are POCOs (plain objects)
- Handlers have no side effects beyond data persistence

### ? SOLID Principles Applied

| Principle | Implementation |
|-----------|-----------------|
| Single Responsibility | Each handler focuses on one use case |
| Open/Closed | New features via new Command/Query, no modification |
| Liskov Substitution | All repositories implement ICourseRepository interface |
| Interface Segregation | ICourseRepository has focused methods |
| Dependency Inversion | Handlers depend on ICourseRepository, not CourseRepository |

## Usage Examples

### C# - Create Course
```csharp
var command = new CreateCourseCommand(
    title: "Beginner Japanese",
    description: "Learn basic Japanese",
    level: CourseLevel.N5,
    imageUrl: "https://example.com/course.jpg",
    instructorName: "Tanaka Sensei",
    estimatedDurationHours: 40);

var result = await mediator.Send(command);
Console.WriteLine($"Created course: {result.Id}");
```

### C# - Get Courses
```csharp
var query = new GetCoursesQuery(level: CourseLevel.N5);
var courses = await mediator.Send(query);

foreach (var course in courses)
{
    Console.WriteLine($"{course.Title} - {course.Level}");
}
```

### C# - Get Course by ID
```csharp
var query = new GetCourseByIdQuery(courseId);
var course = await mediator.Send(query);

Console.WriteLine($"{course.Title}: {course.Description}");
```

## Performance Considerations

- **AsNoTracking**: Queries use AsNoTracking for read performance
- **Indexes**: Multiple strategic indexes for filtering and lookups
- **Pagination**: Repository supports pagination for large datasets
- **Search**: Full-text search support via SearchCoursesByTermAsync
- **Connection**: Uses SQL Server connection pooling

## Error Handling

| Error | HTTP Status | Message |
|-------|-------------|---------|
| Duplicate Title | 400 | "A course with title '...' already exists." |
| Validation Failed | 400 | FluentValidation error messages |
| Course Not Found | 404 | "Course not found." |
| Unauthorized | 401 | "Unauthorized" |

## Feature Extensions (Future)

- [ ] Update course command
- [ ] Delete course command
- [ ] Publish course command
- [ ] Archive course command
- [ ] Get courses paginated query
- [ ] Search courses query
- [ ] Get course statistics query
- [ ] Course reviews/ratings

## Testing

### Unit Test Example (Handler)
```csharp
[Fact]
public async Task CreateCourseHandler_ValidInput_CreatesCourse()
{
    // Arrange
    var mockRepository = new Mock<ICourseRepository>();
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var handler = new CreateCourseHandler(mockRepository.Object, mockUnitOfWork.Object);
    var command = new CreateCourseCommand("Japanese 101", "Learn Japanese", CourseLevel.N5);

    mockRepository.Setup(r => r.CourseExistsByTitleAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Japanese 101", result.Title);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
}
```

## References

- Domain-Driven Design (DDD)
- Clean Architecture principles
- CQRS pattern
- Repository pattern
- Entity Framework Core
- FluentValidation

---

**Status**: Production Ready ?
**Created**: January 2024
**Version**: 1.0
