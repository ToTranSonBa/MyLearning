using Application.Features.Auth.Commands;
using FluentValidation;

namespace Application.Features.Auth.Validators;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Login.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required.")
            .MaximumLength(100).WithMessage("Email or username cannot exceed 100 characters.");

        RuleFor(x => x.Login.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.");
    }
}
