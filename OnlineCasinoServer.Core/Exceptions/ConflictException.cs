﻿using System;

namespace OnlineCasinoServer.Core.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException() : base("Conflict!") { }
        public ConflictException(string message) : base(message) { }
    }
}