using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs.CourseDtos;
using Domain.Courses;
using MediatR;

namespace Application.Features.Courses.Commands;

/// <summary>
/// Creates a new course.
/// Command enforces business rules and validates inputs.
/// </summary>
/// <example>
/// <code>
/// POST /api/courses
/// {
///   "title": "Beginner Japanese",
///   "description": "Learn basic Japanese for beginners",
///   "level": 5,
///   "imageUrl": "https://example.com/course.jpg",
///   "instructorName": "John Doe",
///   "estimatedDurationHours": 40
/// }
/// 
/// Response (201 Created):
/// {
///   "id": "550e8400-e29b-41d4-a716-446655440000",
///   "title": "Beginner Japanese",
///   "description": "Learn basic Japanese for beginners",
///   "level": 5,
///   "imageUrl": "https://example.com/course.jpg",
///   "isActive": true,
///   "totalLessons": 0,
///   "estimatedDurationHours": 40,
///   "instructorName": "John Doe",
///   "createdAt": "2024-01-15T10:30:00Z",
///   "updatedAt": null
/// }
/// </code>
/// </example>
public record CreateCourseCommand(
    string Title,
    string Description,
    CourseLevel Level,
    string? ImageUrl = null,
    string? InstructorName = null,
    int EstimatedDurationHours = 0) : IRequest<CourseDto>;

public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles course creation:
    /// 1. Validates title uniqueness
    /// 2. Creates course aggregate
    /// 3. Persists to database
    /// 4. Returns DTO
    /// </summary>
    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // Validate title uniqueness
        var courseExists = await _courseRepository.CourseExistsByTitleAsync(request.Title);
        if (courseExists)
            throw new BadRequestException($"A course with title '{request.Title}' already exists.");

        // Create course aggregate (business rules enforced)
        var course = Course.Create(request.Title, request.Description, request.Level);

        // Set optional fields
        course.ImageUrl = request.ImageUrl;
        course.InstructorName = request.InstructorName;
        course.EstimatedDurationHours = request.EstimatedDurationHours;

        // Persist to database
        await _courseRepository.AddAsync(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return MapToCourseDto(course);
    }

    private static CourseDto MapToCourseDto(Course course)
    {
        return new CourseDto(
            course.Id,
            course.Title,
            course.Description,
            course.Level,
            course.ImageUrl,
            course.IsActive,
            course.TotalLessons,
            course.EstimatedDurationHours,
            course.InstructorName,
            course.CreatedAt,
            course.UpdatedAt);
    }
}
