using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Amadevus.RecordGenerator.TestsBase;

namespace Amadevus.RecordGenerator.Test
{
    public class GeneratedTypesNamingConventionsTests : RecordTestsBase
    {
        public static readonly IEnumerable<object[]> ParameterDataSource =
            from t in new[]
            {
                typeof(Item)           , typeof(Item.Builder),
                typeof(Container)      , typeof(Container.Builder),
                typeof(GenericRecord<>), typeof(GenericRecord<>.Builder),
            }
            from m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public
                                                             | BindingFlags.Instance
                                                             | BindingFlags.Static)
            from p in m.GetParameters()
            select new[] { t.Name, m.Name, p.Name };

        [Theory]
        [MemberData(nameof(ParameterDataSource))]
        public void Parameter_Name_Uses_Camel_Case(
            #pragma warning disable xUnit1026
            string type, string method,
            #pragma warning restore xUnit1026
            string name)
        {
            Assert.Matches(@"^[a-z]", name);
        }
    }
}