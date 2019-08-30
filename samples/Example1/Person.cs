using System;
using Amadevus.RecordGenerator;

namespace Example
{
    [Record]
    public sealed partial class Person<TDetails>
    {
        public string FirstName { get; }

        public string LastName { get; }

        public string Address { get; }

        public DateTime Birthday { get; }

        public TDetails Details { get; }
    }
}