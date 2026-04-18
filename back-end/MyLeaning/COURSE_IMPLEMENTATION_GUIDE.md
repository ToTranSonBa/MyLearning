# COURSE Module - Complete Implementation Guide

## ? What Was Implemented

### 1. Domain Layer (`Domain\Courses\`)

**Course.cs** - Aggregate Root
- Enforces all business rules via factory methods
- `Course.Create()` - Creates new course with validation
- `Update()` - Updates course with validation
- `Activate()` / `Deactivate()` - Manages course state
- Properties: Id, Title, Description, Level, ImageUrl, IsActive, TotalLessons, EstimatedDurationHours, InstructorName, CreatedAt, UpdatedAt

**CourseLevel.cs** - Value Object (Enum)
- Represents JLPT levels: N5 (Beginner) to N1 (Advanced)
- Strongly typed, prevents invalid values

### 2. Application Layer

#### Commands (`Application\Features\Courses\Commands\`)
**CreateCourseCommand + CreateCourseHandler**
- Validates title uniqueness via repository
- Creates aggregate using factory method
- Persists via repository
- Returns CourseDto
- Implements single responsibility principle

#### Queries (`Application\Features\Courses\Queries\`)
**GetCoursesQuery + GetCoursesHandler**
- Retrieves all active courses
- Optional level filtering
- Returns sorted list
- Uses AsNoTracking for performance

**GetCourseByIdQuery + GetCourseByIdHandler**
- Retrieves single course by ID
- Throws NotFoundException if not found
- Only returns active courses

#### DTOs (`Application\DTOs\CourseDto\`)
**CreateCourseDto** - Request model for course creation
**CourseDto** - Response model for course data

#### Validators (`Application\Features\Courses\Validators\`)
**CreateCourseValidator**
- Title: 3-200 chars, alphanumeric + special chars
- Description: 10-2000 chars
- Level: Valid enum value
- ImageUrl: Valid URI
- InstructorName: Optional, max 100 chars
- EstimatedDurationHours: 0-1000 hours

#### Interfaces (`Application\Common\Interfaces\`)
**ICourseRepository**
- `GetAllActiveCoursesByLevelAsync()` - List with optional level filter
- `GetActiveCourseByIdAsync()` - Get single course
- `CourseExistsByTitleAsync()` - Check uniqueness
- `GetCoursesPaginatedAsync()` - Pagination support
- `SearchCoursesByTermAsync()` - Full-text search

### 3. Infrastructure Layer

#### Repository (`Infrastructure.SqlServer\Repositories\CourseRepository.cs`)
- Implements ICourseRepository
- Uses EF Core with AsNoTracking for reads
- Supports filtering, pagination, search
- Follows repository pattern

#### Database Mapping (`Infrastructure.SqlServer\Mappings\CourseMapping.cs`)
- EF Core entity configuration
- Table: Courses
- 5 strategic indexes for performance
- Unique constraint on Title

#### DependencyInjection (`Infrastructure.SqlServer\DependencyInjection.cs`)
- Registered ICourseRepository ? CourseRepository

#### Migration (`Infrastructure.SqlServer\Migrations\20240120100000_AddCourseEntity.cs`)
- Creates Courses table
- Creates all indexes
- Ready to apply with `dotnet ef database update`

### 4. Presentation Layer

#### Controller (`Web\Controllers\CoursesController.cs`)
- **POST /api/courses** - Create course (requires auth)
- **GET /api/courses** - Get all courses (with optional level filter)
- **GET /api/courses/{id}** - Get course by ID
- Proper HTTP status codes (201, 200, 404, 400, 401)
- Swagger documentation via XML comments
- Request/response logging

## ?? API Endpoints Summary

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/courses | ? Required | Create new course |
| GET | /api/courses | ? Optional | List all active courses |
| GET | /api/courses?level=N5 | ? Optional | List courses filtered by level |
| GET | /api/courses/{id} | ? Optional | Get course details by ID |

## ?? CQRS Flow Example - Create Course

```
1. Client sends POST request
   POST /api/courses
   Content-Type: application/json
   Authorization: Bearer <token>
   
   {
     "title": "Beginner Japanese",
     "description": "Learn basic Japanese",
     "level": 5,
     "imageUrl": "...",
     "instructorName": "...",
     "estimatedDurationHours": 40
   }

2. CoursesController receives request
   - Maps DTO to command
   - Calls mediator.Send(command)

3. MediatR routes to CreateCourseCommand
   - Invokes validation pipeline (FluentValidation)
   - Validates all input constraints

4. CreateCourseHandler executes
   - Queries ICourseRepository.CourseExistsByTitleAsync()
   - Throws BadRequestException if duplicate
   - Calls Course.Create() (aggregate factory)
   - Calls repository.AddAsync()
   - Calls unitOfWork.SaveChangesAsync()

5. Repository executes
   - Maps domain object to EF entity
   - Generates INSERT SQL
   - Executes against SQL Server

6. Database transaction commits
   - Courses table updated
   - Indexes updated

7. Controller returns
   - Maps Course to CourseDto
   - Returns 201 Created with Location header
   - Body contains created course data
```

## ??? Clean Architecture Compliance Checklist

### Dependency Rule ?
- [x] Domain has no dependencies
- [x] Application depends only on Domain
- [x] Infrastructure depends on Application
- [x] Presentation depends on Application
- [x] NO circular dependencies

### CQRS Implementation ?
- [x] Separate commands (CreateCourseCommand)
- [x] Separate queries (GetCoursesQuery, GetCourseByIdQuery)
- [x] Dedicated handlers for each
- [x] Commands modify state
- [x] Queries don't modify state

### Separation of Concerns ?
- [x] Domain: Business rules (Course aggregate)
- [x] Application: Use cases (Commands/Queries)
- [x] Infrastructure: Data access (Repository)
- [x] Presentation: HTTP concerns (Controller)

### SOLID Principles ?
- [x] Single Responsibility: Each handler = one use case
- [x] Open/Closed: Extend via new commands/queries
- [x] Liskov Substitution: ICourseRepository is substitutable
- [x] Interface Segregation: Focused interfaces
- [x] Dependency Inversion: Depend on interfaces, not implementations

### Testability ?
- [x] All dependencies are injectable
- [x] Easy to mock (ICourseRepository, IUnitOfWork)
- [x] Commands/Queries are POCOs
- [x] No static dependencies
- [x] No side effects in handlers (except DB)

## ?? File Structure

```
D:\MyLearning\back-end\MyLeaning\
??? Domain\
?   ??? Courses\
?       ??? Course.cs                    (Aggregate Root)
?       ??? CourseLevel.cs               (Value Object)
?
??? Application\
?   ??? Features\Courses\
?   ?   ??? Commands\
?   ?   ?   ??? CreateCourseCommand.cs
?   ?   ??? Queries\
?   ?   ?   ??? GetCoursesQuery.cs
?   ?   ?   ??? GetCourseByIdQuery.cs
?   ?   ??? Validators\
?   ?   ?   ??? CreateCourseValidator.cs
?   ?   ??? COURSE_MODULE_README.md
?   ??? DTOs\CourseDto\
?   ?   ??? CreateCourseDto.cs
?   ?   ??? CourseDto.cs
?   ??? Common\Interfaces\
?       ??? ICourseRepository.cs
?
??? Infrastructure.SqlServer\
?   ??? Repositories\
?   ?   ??? CourseRepository.cs
?   ??? Mappings\
?   ?   ??? CourseMapping.cs
?   ??? Migrations\
?   ?   ??? 20240120100000_AddCourseEntity.cs
?   ??? DependencyInjection.cs           (updated)
?
??? Web\
    ??? Controllers\
        ??? CoursesController.cs
```

## ?? How to Use

### 1. Apply Migration
```bash
cd Infrastructure.SqlServer
dotnet ef database update
```

### 2. Test Create Endpoint
```bash
curl -X POST https://localhost:5001/api/courses \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Beginner Japanese",
    "description": "Learn basic Japanese for complete beginners",
    "level": 5,
    "imageUrl": "https://example.com/course.jpg",
    "instructorName": "John Doe",
    "estimatedDurationHours": 40
  }'
```

### 3. Test Get All Courses
```bash
curl https://localhost:5001/api/courses
curl https://localhost:5001/api/courses?level=5
```

### 4. Test Get Course by ID
```bash
curl https://localhost:5001/api/courses/550e8400-e29b-41d4-a716-446655440000
```

## ?? Unit Test Example

```csharp
[Fact]
public async Task CreateCourseHandler_WithValidInput_CreatesCourseSuccessfully()
{
    // Arrange
    var mockRepository = new Mock<ICourseRepository>();
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var handler = new CreateCourseHandler(mockRepository.Object, mockUnitOfWork.Object);
    
    var command = new CreateCourseCommand(
        title: "Beginner Japanese",
        description: "Learn basic Japanese",
        level: CourseLevel.N5,
        imageUrl: null,
        instructorName: null,
        estimatedDurationHours: 40);

    mockRepository.Setup(r => r.CourseExistsByTitleAsync(It.IsAny<string>()))
        .ReturnsAsync(false);
    
    mockRepository.Setup(r => r.AddAsync(It.IsAny<Course>()))
        .Returns(Task.CompletedTask);
    
    mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Beginner Japanese", result.Title);
    Assert.Equal(CourseLevel.N5, result.Level);
    Assert.True(result.IsActive);
    
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
    mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

## ?? Database Schema

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

-- Performance Indexes
CREATE UNIQUE INDEX IDX_Courses_Title_Unique ON [dbo].[Courses]([Title]);
CREATE INDEX IDX_Courses_Level ON [dbo].[Courses]([Level]);
CREATE INDEX IDX_Courses_IsActive ON [dbo].[Courses]([IsActive]);
CREATE INDEX IDX_Courses_Active_Level ON [dbo].[Courses]([IsActive], [Level]);
CREATE INDEX IDX_Courses_CreatedAt ON [dbo].[Courses]([CreatedAt]);
```

## ?? Key Design Decisions

1. **Aggregate Root**: Course enforces business rules via factory method `Course.Create()`
2. **Repository Pattern**: ICourseRepository abstracts data access
3. **CQRS Separation**: Read (GetCoursesQuery) vs Write (CreateCourseCommand)
4. **FluentValidation**: Input validation at application layer
5. **DTOs**: Separate data transfer objects for API contracts
6. **Logging**: All endpoints include comprehensive logging
7. **Authorization**: Create endpoint requires authentication
8. **Pagination**: Repository supports pagination for scalability
9. **Full-Text Search**: SearchCoursesByTermAsync for future features
10. **Indexing**: Strategic indexes for performance optimization

## ?? Security Features

- ? Authorization required for create (requires JWT token)
- ? Input validation prevents injection attacks
- ? Parameter sanitization in search
- ? Read-only queries (AsNoTracking)
- ? No direct DbContext access in handlers

## ?? Performance Optimizations

- ? AsNoTracking on queries (no change tracking overhead)
- ? Multiple indexes for filtering
- ? Connection pooling via SQL Server
- ? Pagination support for large datasets
- ? Minimal data transfer (only DTOs)

## ?? Future Enhancements

- [ ] Update course command
- [ ] Delete course command
- [ ] Publish course command
- [ ] Get courses paginated query
- [ ] Search courses query
- [ ] Get course statistics
- [ ] Archive course
- [ ] Course reviews/ratings
- [ ] Course prerequisites
- [ ] User enrollments

## ? Production Ready

- [x] All Clean Architecture rules followed
- [x] CQRS properly implemented
- [x] Comprehensive validation
- [x] Error handling
- [x] Logging
- [x] Authorization
- [x] Database migration
- [x] Documentation

---

**Implementation Status**: ? COMPLETE
**Build Status**: ? SUCCESSFUL
**Ready for Use**: ? YES

Next: Apply migration and test endpoints!
