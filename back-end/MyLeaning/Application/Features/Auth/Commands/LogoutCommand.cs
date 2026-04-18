using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LogoutCommand(Guid UserId) : IRequest<Unit>;

public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public LogoutHandler(IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
