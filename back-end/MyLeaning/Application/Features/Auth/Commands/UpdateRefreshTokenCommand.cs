using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Updates user's refresh token and stores it in database.
/// Called after successful login or token refresh.
/// </summary>
public record UpdateRefreshTokenCommand(Guid UserId, string RefreshToken, int ExpiryDays = 7) : IRequest<Unit>;

public class UpdateRefreshTokenHandler : IRequestHandler<UpdateRefreshTokenCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRefreshTokenHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Updates user's refresh token and expiry time in database.
    /// </summary>
    public async Task<Unit> Handle(UpdateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        user.RefreshToken = request.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(request.ExpiryDays);

        await _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
