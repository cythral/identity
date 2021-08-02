using System;
using System.Linq;
using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Brighid.Identity
{
    public class HttpStatusCodeExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            var result = context.Exception switch
            {
                HttpStatusCodeException httpStatusCodeException => new ObjectResult(new { httpStatusCodeException.Message })
                {
                    StatusCode = (int)httpStatusCodeException.Status,
                },

                AggregateException aggregateException => new ObjectResult(new
                {
                    Message = "Multiple errors occurred.",
                    ValidationErrors = from innerException in aggregateException.InnerExceptions
                                       where innerException is HttpStatusCodeException
                                       select innerException.Message,
                })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                },

                _ => null,
            };

            if (result != null)
            {
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }
    }
}
