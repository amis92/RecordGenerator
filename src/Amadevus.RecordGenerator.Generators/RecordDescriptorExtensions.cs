using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class RecordDescriptorExtensions
    {
        public static RecordDescriptor.Entry ToRecordEntry(this PropertyDeclarationSyntax property, ISymbol symbol)
        {
            return new RecordDescriptor.SimpleEntry(
                property.Identifier.WithoutTrivia(),
                property.Type.WithoutTrivia(),
                property.WithoutTrivia(),
                symbol);
        }

        public static RecordDescriptor ToRecordDescriptor(this ClassDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            return new RecordDescriptor(
                typeDeclaration.GetTypeSyntax().WithoutTrivia(),
                typeDeclaration.Identifier.WithoutTrivia(),
                typeDeclaration.GetRecordProperties(semanticModel),
                typeDeclaration.WithoutTrivia(),
                typeDeclaration.GetLocation(),
                semanticModel.GetDeclaredSymbol(typeDeclaration),
                semanticModel);
        }

        private static ImmutableArray<RecordDescriptor.Entry> GetRecordProperties(this TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            return typeDeclaration.Members.GetRecordProperties().AsRecordEntries(semanticModel);
        }

        private static ImmutableArray<PropertyDeclarationSyntax> GetRecordProperties(this SyntaxList<MemberDeclarationSyntax> members)
        {
            return members
                .OfType<PropertyDeclarationSyntax>()
                .Where(
                    propSyntax => propSyntax.IsRecordViable())
                .ToImmutableArray();
        }

        private static ImmutableArray<RecordDescriptor.Entry> AsRecordEntries(this IEnumerable<PropertyDeclarationSyntax> properties, SemanticModel semanticModel)
        {
            return properties
                .Select(p => p.ToRecordEntry(semanticModel.GetSymbolInfo(p.Type).Symbol))
                .ToImmutableArray();
        }

        public static QualifiedNameSyntax ToNestedBuilderType(this NameSyntax type)
        {
            return QualifiedName(
                    type,
                    IdentifierName(Names.Builder));
        }

        public static string ToLowerFirstLetter(this string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : $"{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }

        public static Diagnostic CreateDiagnostic(this RecordDescriptor recordDescriptor,
                                                  DiagnosticDescriptor diagnosticDescriptor) =>
            Diagnostic.Create(
                diagnosticDescriptor,
                recordDescriptor.TypeDeclarationLocation,
                recordDescriptor.TypeIdentifier.Text);
    }
}
