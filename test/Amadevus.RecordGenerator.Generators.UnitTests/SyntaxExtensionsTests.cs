using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Xunit;

namespace Amadevus.RecordGenerator.Generators.UnitTests
{
    public class SyntaxExtensionsTests
    {
        [Theory]
        [InlineData("public int Prop { get; }")]
        [InlineData("public object Prop { get; }")]
        public void IsRecordViable_returns_true_for_valid_properties(string propertyDeclaration)
        {
            var root = SyntaxFactory.ParseCompilationUnit(
                "class X { " + propertyDeclaration + " }");
            var prop = (PropertyDeclarationSyntax)root.Members[0].ChildNodes().Single();

            prop.IsRecordViable().Should().BeTrue();
        }

        [Theory]
        [InlineData("public int Prop { get => 1; }")]
        [InlineData("public int Prop => 1;")]
        [InlineData("public int Prop { get; set; }")]
        [InlineData("public int Prop { get; private set; }")]
        public void IsRecordViable_returns_false_for_non_auto_get_only_properties(string propertyDeclaration)
        {
            var root = SyntaxFactory.ParseCompilationUnit(
                "class X { " + propertyDeclaration + " }");
            var prop = (PropertyDeclarationSyntax)root.Members[0].ChildNodes().Single();

            prop.IsRecordViable().Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("internal")]
        [InlineData("protected")]
        [InlineData("private")]
        [InlineData("public static")]
        public void IsRecordViable_returns_false_for_properties_with_invalid_modifiers(string modifiers)
        {
            var root = SyntaxFactory.ParseCompilationUnit(
                "class X { " + modifiers + " int Prop { get; } }");
            var prop = (PropertyDeclarationSyntax)root.Members[0].ChildNodes().Single();

            prop.IsRecordViable().Should().BeFalse();
        }
    }
}
