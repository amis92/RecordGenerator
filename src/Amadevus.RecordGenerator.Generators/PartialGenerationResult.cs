using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class PartialGenerationResult
    {
        public static readonly PartialGenerationResult Empty =
            new PartialGenerationResult(TokenList(),
                                        ImmutableArray<BaseTypeSyntax>.Empty,
                                        ImmutableArray<MemberDeclarationSyntax>.Empty,
                                        ImmutableArray<Diagnostic>.Empty);

        public SyntaxTokenList Modifiers { get; }
        public ImmutableArray<BaseTypeSyntax> BaseTypes { get; }
        public ImmutableArray<MemberDeclarationSyntax> Members { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        PartialGenerationResult(
            SyntaxTokenList modifiers,
            ImmutableArray<BaseTypeSyntax> baseTypes,
            ImmutableArray<MemberDeclarationSyntax> members,
            ImmutableArray<Diagnostic> diagnostics)
        {
            Members = members;
            Modifiers = modifiers;
            BaseTypes = baseTypes;
            Diagnostics = diagnostics;
        }

        public bool IsEmpty => ReferenceEquals(this, Empty);

        public PartialGenerationResult Add(PartialGenerationResult result)
            => result is null ? throw new ArgumentNullException(nameof(result))
             : result.IsEmpty ? this
             : this.IsEmpty ? result
             : Update(Modifiers.AddRange(result.Modifiers),
                      BaseTypes.AddRange(result.BaseTypes),
                      Members.AddRange(result.Members),
                      Diagnostics.AddRange(result.Diagnostics));

        public PartialGenerationResult WithModifiers(SyntaxTokenList value) =>
            Update(value, BaseTypes, Members, Diagnostics);

        public PartialGenerationResult WithBaseList(ImmutableArray<BaseTypeSyntax> value) =>
            Update(Modifiers, value, Members, Diagnostics);

        public PartialGenerationResult WithMembers(ImmutableArray<MemberDeclarationSyntax> value) =>
            Update(Modifiers, BaseTypes, value, Diagnostics);

        public PartialGenerationResult WithDiagnostics(ImmutableArray<Diagnostic> value) =>
            Update(Modifiers, BaseTypes, Members, value);

        public PartialGenerationResult AddMember(MemberDeclarationSyntax member) =>
            Update(Modifiers, BaseTypes, Members.Add(member), Diagnostics);

        public PartialGenerationResult AddMembers(IEnumerable<MemberDeclarationSyntax> members) =>
            Update(Modifiers, BaseTypes, Members.AddRange(members), Diagnostics);

        public PartialGenerationResult AddMembers(params MemberDeclarationSyntax[] members) =>
            Update(Modifiers, BaseTypes, Members.AddRange(members), Diagnostics);

        public PartialGenerationResult AddDiagnostic(Diagnostic diagnostic) =>
            Update(Modifiers, BaseTypes, Members, Diagnostics.Add(diagnostic));

        public PartialGenerationResult AddDiagnostics(IEnumerable<Diagnostic> diagnostics) =>
            Update(Modifiers, BaseTypes, Members, Diagnostics.AddRange(diagnostics));

        public PartialGenerationResult AddDiagnostics(params Diagnostic[] diagnostics) =>
            Update(Modifiers, BaseTypes, Members, Diagnostics.AddRange(diagnostics));

        private static PartialGenerationResult
            Update(
                SyntaxTokenList modifiers,
                ImmutableArray<BaseTypeSyntax> baseTypes,
                ImmutableArray<MemberDeclarationSyntax> members,
                ImmutableArray<Diagnostic> diagnostics)
            => modifiers.Count == 0 && baseTypes.IsEmpty && members.IsEmpty && diagnostics.IsEmpty
             ? Empty
             : new PartialGenerationResult(modifiers, baseTypes, members, diagnostics);
    }
}
