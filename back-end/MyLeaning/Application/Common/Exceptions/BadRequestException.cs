using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base($"Bad Request:\n{message}", 400)
        {
        }
    }
}
