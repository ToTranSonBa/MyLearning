using Domain.Courses;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Course aggregate.
/// Defines contracts for course data access operations.
/// </summary>
public interface ICourseRepository
{
    /// <summary>
    /// Gets all active courses, optionally filtered by level.
    /// </summary>
    Task<IEnumerable<Course>> GetAllActiveCoursesByLevelAsync(CourseLevel? level = null);

    /// <summary>
    /// Gets a course by ID if it's active.
    /// </summary>
    Task<Course?> GetActiveCourseByIdAsync(Guid id);

    /// <summary>
    /// Checks if a course with the given title already exists.
    /// </summary>
    Task<bool> CourseExistsByTitleAsync(string title);

    /// <summary>
    /// Gets courses with pagination.
    /// </summary>
    Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPaginatedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Searches courses by title or description.
    /// </summary>
    Task<IEnumerable<Course>> SearchCoursesByTermAsync(string searchTerm);

    /// <summary>
    /// Adds a new course to the repository.
    /// </summary>
    Task AddAsync(Course entity);

    /// <summary>
    /// Updates an existing course.
    /// </summary>
    void Update(Course entity);

    /// <summary>
    /// Deletes a course.
    /// </summary>
    void Delete(Course entity);
}
