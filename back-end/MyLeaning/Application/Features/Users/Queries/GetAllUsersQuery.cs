using Application.Common.Interfaces;
using Application.DTOs.UserDto;
using MediatR;

namespace Application.Features.Users.Queries;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto(u.Id, u.UserName, u.Email, u.FullName));
    }
}
