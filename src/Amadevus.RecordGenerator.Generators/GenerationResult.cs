using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class GenerationResult
    {
        public static readonly GenerationResult Empty =
            new GenerationResult(TokenList(),
                           ImmutableArray<BaseTypeSyntax>.Empty,
                           ImmutableArray<MemberDeclarationSyntax>.Empty);

        public SyntaxTokenList Modifiers { get; }
        public ImmutableArray<BaseTypeSyntax> BaseTypes { get; }
        public ImmutableArray<MemberDeclarationSyntax> Members;

        GenerationResult(SyntaxTokenList modifiers,
                   ImmutableArray<BaseTypeSyntax> baseTypes,
                   ImmutableArray<MemberDeclarationSyntax> members)
        {
            Members = members;
            Modifiers = modifiers;
            BaseTypes = baseTypes;
        }

        public GenerationResult Add(GenerationResult result)
            => result is null ? throw new ArgumentNullException(nameof(result))
             : ReferenceEquals(result, Empty) ? this
             : ReferenceEquals(this, Empty) ? result
             : new GenerationResult(Modifiers.AddRange(result.Modifiers),
                              BaseTypes.AddRange(result.BaseTypes),
                              Members.AddRange(result.Members));

        public GenerationResult WithModifiers(SyntaxTokenList value) =>
            new GenerationResult(value, BaseTypes, Members);

        public GenerationResult WithBaseList(ImmutableArray<BaseTypeSyntax> value) =>
            new GenerationResult(Modifiers, value, Members);

        public GenerationResult WithMembers(ImmutableArray<MemberDeclarationSyntax> value) =>
            new GenerationResult(Modifiers, BaseTypes, value);

        public GenerationResult AddMember(MemberDeclarationSyntax member) =>
            new GenerationResult(Modifiers, BaseTypes, Members.Add(member));

        public GenerationResult AddMembers(IEnumerable<MemberDeclarationSyntax> members) =>
            new GenerationResult(Modifiers, BaseTypes, Members.AddRange(members));

        public GenerationResult AddMembers(params MemberDeclarationSyntax[] members) =>
            new GenerationResult(Modifiers, BaseTypes, Members.AddRange(members));
    }
}
