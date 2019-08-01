using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class PartialGenerationResult
    {
        public static readonly PartialGenerationResult Empty =
            new PartialGenerationResult(TokenList(),
                                        ImmutableArray<BaseTypeSyntax>.Empty,
                                        ImmutableArray<MemberDeclarationSyntax>.Empty);

        public SyntaxTokenList Modifiers { get; }
        public ImmutableArray<BaseTypeSyntax> BaseTypes { get; }
        public ImmutableArray<MemberDeclarationSyntax> Members;

        PartialGenerationResult(SyntaxTokenList modifiers,
                   ImmutableArray<BaseTypeSyntax> baseTypes,
                   ImmutableArray<MemberDeclarationSyntax> members)
        {
            Members = members;
            Modifiers = modifiers;
            BaseTypes = baseTypes;
        }

        public PartialGenerationResult Add(PartialGenerationResult result)
            => result is null ? throw new ArgumentNullException(nameof(result))
             : ReferenceEquals(result, Empty) ? this
             : ReferenceEquals(this, Empty) ? result
             : new PartialGenerationResult(Modifiers.AddRange(result.Modifiers),
                                           BaseTypes.AddRange(result.BaseTypes),
                                           Members.AddRange(result.Members));

        public PartialGenerationResult WithModifiers(SyntaxTokenList value) =>
            new PartialGenerationResult(value, BaseTypes, Members);

        public PartialGenerationResult WithBaseList(ImmutableArray<BaseTypeSyntax> value) =>
            new PartialGenerationResult(Modifiers, value, Members);

        public PartialGenerationResult WithMembers(ImmutableArray<MemberDeclarationSyntax> value) =>
            new PartialGenerationResult(Modifiers, BaseTypes, value);

        public PartialGenerationResult AddMember(MemberDeclarationSyntax member) =>
            new PartialGenerationResult(Modifiers, BaseTypes, Members.Add(member));

        public PartialGenerationResult AddMembers(IEnumerable<MemberDeclarationSyntax> members) =>
            new PartialGenerationResult(Modifiers, BaseTypes, Members.AddRange(members));

        public PartialGenerationResult AddMembers(params MemberDeclarationSyntax[] members) =>
            new PartialGenerationResult(Modifiers, BaseTypes, Members.AddRange(members));
    }
}
