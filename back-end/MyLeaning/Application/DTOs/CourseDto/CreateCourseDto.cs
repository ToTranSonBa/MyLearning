using Domain.Courses;

namespace Application.DTOs.CourseDto;

/// <summary>
/// Data Transfer Object for creating a new course.
/// </summary>
public record CreateCourseDto(
    /// <summary>Course title</summary>
    string Title,
    /// <summary>Course description</summary>
    string Description,
    /// <summary>JLPT level (N5-N1)</summary>
    CourseLevel Level,
    /// <summary>Optional course image URL</summary>
    string? ImageUrl = null,
    /// <summary>Optional instructor name</summary>
    string? InstructorName = null,
    /// <summary>Estimated duration in hours</summary>
    int EstimatedDurationHours = 0);
