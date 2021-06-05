using System;

namespace Brighid.Identity
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DelegatingRoleAttribute : Attribute
    {
        public DelegatingRoleAttribute(string role)
        {
            Role = role;
        }

        public string Role { get; }
    }
}
