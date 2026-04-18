using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base($"Unauthorized:\n{message}", 401)
        {
        }
    }
}
