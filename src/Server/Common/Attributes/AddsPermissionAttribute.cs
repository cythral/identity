using System;

namespace Brighid.Identity
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class AddsPermissionAttribute : Attribute
    {
        public AddsPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
