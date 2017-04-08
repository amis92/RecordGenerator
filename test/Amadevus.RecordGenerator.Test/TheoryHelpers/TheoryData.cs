using System.Collections;
using System.Collections.Generic;

namespace TestHelper
{
    /// <summary>
    /// Provides data sets for theory tests. Extend this class to have strongly-typed
    /// dataset provider method: <see cref="GetDataSets"/>.
    /// </summary>
    public abstract class TheoryDataProvider : IEnumerable<object[]>
    {
        public abstract IEnumerable<ITheoryDatum> GetDataSets();

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var dataSet in GetDataSets())
            {
                yield return dataSet.ToParameterArray();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
