using System;
using System.Collections.Generic;
using System.Text;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public partial class ValidatingRecord
    {
        public string Name { get; }

        /// <summary>
        /// This tests whether lowercasing it doesn't cause compilation error (as a keyword) and is '@'-escaped correctly.
        /// </summary>
        public string Switch { get; }

        partial void Validate()
        {
            if (Name is null) throw new ArgumentNullException("name");
        }
    }
}
