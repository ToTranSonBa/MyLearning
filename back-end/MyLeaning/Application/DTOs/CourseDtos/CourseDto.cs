using Domain.Courses;

namespace Application.DTOs.CourseDtos;

/// <summary>
/// Data Transfer Object for course response.
/// </summary>
public record CourseDto(
    /// <summary>Course ID</summary>
    Guid Id,
    /// <summary>Course title</summary>
    string Title,
    /// <summary>Course description</summary>
    string Description,
    /// <summary>JLPT level (N5-N1)</summary>
    CourseLevel Level,
    /// <summary>Course image URL</summary>
    string? ImageUrl,
    /// <summary>Is course active</summary>
    bool IsActive,
    /// <summary>Total lessons in course</summary>
    int TotalLessons,
    /// <summary>Estimated duration in hours</summary>
    int EstimatedDurationHours,
    /// <summary>Instructor name</summary>
    string? InstructorName,
    /// <summary>Course creation date</summary>
    DateTime CreatedAt,
    /// <summary>Course last update date</summary>
    DateTime? UpdatedAt);
