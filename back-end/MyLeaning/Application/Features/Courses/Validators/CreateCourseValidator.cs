using Application.Features.Courses.Commands;
using FluentValidation;

namespace Application.Features.Courses.Validators;

/// <summary>
/// Validates CreateCourseCommand inputs.
/// Ensures title, description meet requirements and level is valid.
/// </summary>
public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MinimumLength(3).WithMessage("Course title must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-()&'.]+$").WithMessage("Course title contains invalid characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Course description is required.")
            .MinimumLength(10).WithMessage("Course description must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Course description cannot exceed 2000 characters.");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid course level. Must be N5, N4, N3, N2, or N1.");

        RuleFor(x => x.ImageUrl)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ImageUrl must be a valid URI.");

        RuleFor(x => x.InstructorName)
            .MaximumLength(100).WithMessage("Instructor name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.InstructorName));

        RuleFor(x => x.EstimatedDurationHours)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated duration must be non-negative.")
            .LessThanOrEqualTo(1000).WithMessage("Estimated duration cannot exceed 1000 hours.");
    }
}
