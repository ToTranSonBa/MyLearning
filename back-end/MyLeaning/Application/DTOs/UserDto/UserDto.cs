using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.UserDto
{
    public record UserDto(Guid Id, string UserName, string Email, string FullName);
}

