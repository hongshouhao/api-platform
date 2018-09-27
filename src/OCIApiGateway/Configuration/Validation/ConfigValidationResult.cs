﻿using Ocelot.Errors;
using System.Collections.Generic;

namespace OCIApiGateway.Configuration.Validation
{
    public class ConfigValidationResult
    {
        public bool IsError { get; private set; }
        public List<Error> Errors { get; private set; } = new List<Error>();

        public ConfigValidationResult(string msg)
        {
            Error error = new CheckError(msg);
            Errors.Add(error);
            IsError = true;
        }

        public ConfigValidationResult(List<Error> errors)
        {
            Errors.AddRange(errors);
            IsError = errors.Count > 0;
        }
    }
}
