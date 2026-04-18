using Application.Common.Interfaces;
using Application.DTOs.CourseDto;
using Domain.Courses;
using MediatR;

namespace Application.Features.Courses.Queries;

/// <summary>
/// Gets list of all active courses, optionally filtered by level.
/// </summary>
/// <example>
/// <code>
/// GET /api/courses
/// GET /api/courses?level=N5
/// 
/// Response (200 OK):
/// [
///   {
///     "id": "550e8400-e29b-41d4-a716-446655440000",
///     "title": "Beginner Japanese",
///     "description": "Learn basic Japanese",
///     "level": 5,
///     "imageUrl": "https://example.com/course.jpg",
///     "isActive": true,
///     "totalLessons": 10,
///     "estimatedDurationHours": 40,
///     "instructorName": "John Doe",
///     "createdAt": "2024-01-15T10:30:00Z",
///     "updatedAt": null
///   }
/// ]
/// </code>
/// </example>
public record GetCoursesQuery(CourseLevel? Level = null) : IRequest<IEnumerable<CourseDto>>;

public class GetCoursesHandler : IRequestHandler<GetCoursesQuery, IEnumerable<CourseDto>>
{
    private readonly ICourseRepository _courseRepository;

    public GetCoursesHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Handles retrieving courses:
    /// 1. Queries all active courses (optionally filtered by level)
    /// 2. Maps to DTOs
    /// 3. Returns list
    /// </summary>
    public async Task<IEnumerable<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await _courseRepository.GetAllActiveCoursesByLevelAsync(request.Level);

        return courses.Select(MapToCourseDto).OrderBy(c => c.Level).ToList();
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
