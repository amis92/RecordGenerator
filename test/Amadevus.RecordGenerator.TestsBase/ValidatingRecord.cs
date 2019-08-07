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

        static partial void Validate(ref string name, ref string @switch)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
        }
    }
}
