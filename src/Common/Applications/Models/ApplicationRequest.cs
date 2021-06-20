using System;

namespace Brighid.Identity.Applications
{
    public class ApplicationRequest : Application
    {
        public new string[] Roles { get; set; }

        protected new Guid Id { get; set; }
    }
}
