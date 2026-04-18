using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        protected AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
