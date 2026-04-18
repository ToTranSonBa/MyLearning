using Application.Common.Interfaces;
using Application.DTOs.AuthDto;
using MediatR;

namespace Application.Features.Auth.Commands;

public record SendPasswordResetEmailCommand(string Email, string FullName, string ResetLink) : IRequest<Unit>;

public class SendPasswordResetEmailHandler : IRequestHandler<SendPasswordResetEmailCommand, Unit>
{
    private readonly IEmailService _emailService;

    public SendPasswordResetEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        await _emailService.SendPasswordResetEmailAsync(request.Email, request.FullName, request.ResetLink);
        return Unit.Value;
    }
}
