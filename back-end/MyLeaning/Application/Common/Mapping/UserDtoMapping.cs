using Application.DTOs.UserDtos;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Mapping
{
    public static class UserDtoMapping
    {
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto(user.Id, user.Email, user.FullName);
        }
    }
}
