using System;

namespace Project.Net
{
    public class PluralAttribute : Attribute
    {
        public PluralAttribute(string plural)
        {
            Plural = plural;
        }

        public string Plural { get; }
    }
}