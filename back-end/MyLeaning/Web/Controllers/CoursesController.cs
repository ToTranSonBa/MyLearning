using Application.DTOs.CourseDtos;
using Application.Features.Courses.Commands;
using Application.Features.Courses.Queries;
using Domain.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

/// <summary>
/// Courses endpoint for managing Japanese language courses.
/// Handles course creation, listing, and retrieval operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(IMediator mediator, ILogger<CoursesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new course.
    /// </summary>
    /// <param name="dto">Course creation details</param>
    /// <returns>Created course with ID</returns>
    /// <response code="201">Course created successfully</response>
    /// <response code="400">Invalid input or course title already exists</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseDto dto)
    {
        try
        {
            _logger.LogInformation("Creating course: {Title}", dto.Title);

            var command = new CreateCourseCommand(
                dto.Title,
                dto.Description,
                dto.Level,
                dto.ImageUrl,
                dto.InstructorName,
                dto.EstimatedDurationHours);

            var result = await _mediator.Send(command);

            _logger.LogInformation("Course created successfully: {CourseId}", result.Id);

            return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating course: {Error}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets all active courses, optionally filtered by level.
    /// </summary>
    /// <param name="level">Optional course level filter (N5-N1)</param>
    /// <returns>List of courses</returns>
    /// <response code="200">Courses retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses([FromQuery] CourseLevel? level = null)
    {
        try
        {
            _logger.LogInformation("Retrieving courses. Level filter: {Level}", level);

            var query = new GetCoursesQuery(level);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} courses", result.Count());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving courses: {Error}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets a single course by ID.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Course details</returns>
    /// <response code="200">Course retrieved successfully</response>
    /// <response code="404">Course not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving course: {CourseId}", id);

            var query = new GetCourseByIdQuery(id);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Course retrieved successfully: {CourseId}", id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving course {CourseId}: {Error}", id, ex.Message);
            throw;
        }
    }
}
