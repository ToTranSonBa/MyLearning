using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs.CourseDtos;
using Domain.Courses;
using MediatR;

namespace Application.Features.Courses.Queries;

/// <summary>
/// Gets a single course by ID.
/// Returns only active courses.
/// </summary>
/// <example>
/// <code>
/// GET /api/courses/550e8400-e29b-41d4-a716-446655440000
/// 
/// Response (200 OK):
/// {
///   "id": "550e8400-e29b-41d4-a716-446655440000",
///   "title": "Beginner Japanese",
///   "description": "Learn basic Japanese",
///   "level": 5,
///   "imageUrl": "https://example.com/course.jpg",
///   "isActive": true,
///   "totalLessons": 10,
///   "estimatedDurationHours": 40,
///   "instructorName": "John Doe",
///   "createdAt": "2024-01-15T10:30:00Z",
///   "updatedAt": null
/// }
/// 
/// Error Response (404 Not Found):
/// {
///   "message": "Course not found."
/// }
/// </code>
/// </example>
public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>;

public class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, CourseDto>
{
    private readonly ICourseRepository _courseRepository;

    public GetCourseByIdHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Handles retrieving a single course:
    /// 1. Queries course by ID (active only)
    /// 2. Throws NotFoundException if not found
    /// 3. Maps to DTO
    /// 4. Returns result
    /// </summary>
    public async Task<CourseDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetActiveCourseByIdAsync(request.CourseId);

        if (course == null)
            throw new NotFoundException("Course not found.");

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
