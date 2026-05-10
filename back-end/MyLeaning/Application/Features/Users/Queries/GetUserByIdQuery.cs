using Application.Common.Interfaces;
using Application.Common.Mapping;
using Application.DTOs.UserDtos;
using MediatR;

namespace Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.Id);
        if (user == null)
            return null;

        return user.ToUserDto();
    }
}
