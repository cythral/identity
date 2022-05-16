using System;

namespace Brighid.Identity
{
    public interface IExceptionMapping
    {
        public Type Exception { get; }

        public int StatusCode { get; }
    }
}
