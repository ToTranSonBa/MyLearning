using Application.Common.Interfaces;
using Domain.Courses;
using Infrastructure.SqlServer.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlServer.Repositories;

/// <summary>
/// Repository implementation for Course aggregate.
/// Provides database access for course operations.
/// </summary>
public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new course to the repository.
    /// </summary>
    public async Task AddAsync(Course entity)
    {
        await _context.AddAsync(entity);
    }

    /// <summary>
    /// Updates an existing course.
    /// </summary>
    public void Update(Course entity)
    {
        _context.Set<Course>().Update(entity);
    }

    /// <summary>
    /// Deletes a course.
    /// </summary>
    public void Delete(Course entity)
    {
        _context.Remove(entity);
    }

    /// <summary>
    /// Gets all active courses, optionally filtered by level.
    /// </summary>
    public async Task<IEnumerable<Course>> GetAllActiveCoursesByLevelAsync(CourseLevel? level = null)
    {
        var query = _context.Set<Course>()
            .AsNoTracking()
            .Where(c => c.IsActive);

        if (level.HasValue)
            query = query.Where(c => c.Level == level.Value);

        return await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a course by ID if it's active.
    /// </summary>
    public async Task<Course?> GetActiveCourseByIdAsync(Guid id)
    {
        return await _context.Set<Course>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    /// <summary>
    /// Checks if a course with the given title already exists.
    /// </summary>
    public async Task<bool> CourseExistsByTitleAsync(string title)
    {
        return await _context.Set<Course>()
            .AsNoTracking()
            .AnyAsync(c => c.Title == title);
    }

    /// <summary>
    /// Gets courses with pagination.
    /// </summary>
    public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPaginatedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Set<Course>()
            .AsNoTracking()
            .Where(c => c.IsActive);

        var totalCount = await query.CountAsync();

        var courses = await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (courses, totalCount);
    }

    /// <summary>
    /// Searches courses by title or description.
    /// </summary>
    public async Task<IEnumerable<Course>> SearchCoursesByTermAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();

        return await _context.Set<Course>()
            .AsNoTracking()
            .Where(c => c.IsActive && (
                c.Title.ToLower().Contains(lowerSearchTerm) ||
                c.Description.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(c => c.Title)
            .ToListAsync();
    }
}
