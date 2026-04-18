using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record SendConfirmationEmailCommand(string Email, string FullName, string ConfirmationLink) : IRequest<Unit>;

public class SendConfirmationEmailHandler : IRequestHandler<SendConfirmationEmailCommand, Unit>
{
    private readonly IEmailService _emailService;

    public SendConfirmationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        await _emailService.SendConfirmationEmailAsync(request.Email, request.FullName, request.ConfirmationLink);
        return Unit.Value;
    }
}
