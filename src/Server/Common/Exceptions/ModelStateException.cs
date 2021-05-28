using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.ModelBinding;

#pragma warning disable CA1032

namespace Brighid.Identity
{
    public class ModelStateException : Exception
    {
        public ModelStateException(ModelStateDictionary modelState)
            : base("One or more errors occurred.")
        {
            Errors = from value in modelState.Values
                     from error in value.Errors
                     select error.ErrorMessage;
        }

        public IEnumerable<string> Errors { get; }
    }
}
