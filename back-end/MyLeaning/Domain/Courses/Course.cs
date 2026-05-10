using Domain.Common;

namespace Domain.Courses;

/// <summary>
/// Course aggregate root.
/// Represents a Japanese language course with hierarchy levels (N5-N1).
/// </summary>
public class Course : BaseEntity
{
    public Guid Id { get; set; } 
    /// <summary>
    /// Course title (e.g., "Beginner Japanese", "Business Japanese")
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Detailed course description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// JLPT level: N5 (Beginner) to N1 (Advanced)
    /// </summary>
    public required CourseLevel Level { get; set; }

    /// <summary>
    /// Optional course image/thumbnail URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Course is active and available for enrollment
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Number of lessons in this course
    /// </summary>
    public int TotalLessons { get; set; } = 0;

    /// <summary>
    /// Estimated duration in hours
    /// </summary>
    public int EstimatedDurationHours { get; set; } = 0;

    /// <summary>
    /// Course instructor/author name
    /// </summary>
    public string? InstructorName { get; set; }

    protected Course() { }

    /// <summary>
    /// Creates a new course with required fields.
    /// </summary>
    public static Course Create(string title, string description, CourseLevel level)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        return new Course
        {
             Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Level = level,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates course information.
    /// </summary>
    public void Update(string title, string description, CourseLevel level)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title;
        Description = description;
        Level = level;
        SetUpdated();
    }

    /// <summary>
    /// Deactivates the course.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }

    /// <summary>
    /// Activates the course.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }
}
