using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.UserDtos
{
    public record UserDto(Guid Id, string Email, string FullName);
}

