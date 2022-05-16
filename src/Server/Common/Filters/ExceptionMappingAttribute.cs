using System;
using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Brighid.Identity
{
    public class ExceptionMappingAttribute<TException> : ActionFilterAttribute, IExceptionMapping
        where TException : Exception
    {
        public ExceptionMappingAttribute(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
        }

        public Type Exception => typeof(TException);

        public int StatusCode { get; }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is TException exception)
            {
                var error = exception is IErrorException errorException
                    ? errorException.Error
                    : new { exception.Message };

                context.ExceptionHandled = true;
                context.Result = new ObjectResult(error)
                {
                    StatusCode = StatusCode,
                };
            }
        }
    }
}
