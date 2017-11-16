using System;

namespace Project.Net
{
    public class SubDomainAttribute : Attribute
    {
        public SubDomainAttribute(string subDomain)
        {
            SubDomain = subDomain;
        }

        public string SubDomain { get; }
    }
}