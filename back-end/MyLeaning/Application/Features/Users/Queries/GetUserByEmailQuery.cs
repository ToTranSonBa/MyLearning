using Application.Common.Interfaces;
using Application.DTOs.UserDto;
using MediatR;

namespace Application.Features.Users.Queries;

public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return null;

        return new UserDto(user.Id, user.UserName, user.Email, user.FullName);
    }
}
