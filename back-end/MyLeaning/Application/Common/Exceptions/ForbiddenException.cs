using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base($"Forbidden:\n{message}", 403)
        {
        }
    }
}
