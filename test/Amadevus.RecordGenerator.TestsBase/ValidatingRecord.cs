using System;
using System.Collections.Generic;
using System.Text;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public partial class ValidatingRecord
    {
        public string Name { get; }

        partial void Validate()
        {
            if (Name is null) throw new ArgumentNullException(nameof(Name));
        }
    }
}
