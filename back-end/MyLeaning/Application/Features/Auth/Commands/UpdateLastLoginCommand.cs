using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Updates user's last login timestamp.
/// Called at the end of successful login process.
/// </summary>
public record UpdateLastLoginCommand(Guid UserId) : IRequest<Unit>;

public class UpdateLastLoginHandler : IRequestHandler<UpdateLastLoginCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLastLoginHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Updates user's last login timestamp to current UTC time.
    /// </summary>
    public async Task<Unit> Handle(UpdateLastLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        user.LastLoginAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
