using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base($"Not Found:\n{message}", 404)
        {
        }
    }
}
