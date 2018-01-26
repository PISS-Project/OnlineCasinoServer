using System;

namespace OnlineCasinoServer.Core.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("Access denied!") { }
        public ForbiddenException(string message) : base(message) { }
    }
}