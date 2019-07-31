using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class Generation
    {
        public static readonly Generation Empty =
            new Generation(TokenList(),
                           ImmutableArray<BaseTypeSyntax>.Empty,
                           ImmutableArray<MemberDeclarationSyntax>.Empty);

        public SyntaxTokenList Modifiers { get; }
        public ImmutableArray<BaseTypeSyntax> BaseTypes { get; }
        public ImmutableArray<MemberDeclarationSyntax> Members;

        Generation(SyntaxTokenList modifiers,
                   ImmutableArray<BaseTypeSyntax> baseTypes,
                   ImmutableArray<MemberDeclarationSyntax> members)
        {
            Members = members;
            Modifiers = modifiers;
            BaseTypes = baseTypes;
        }

        public Generation Add(Generation generation)
            => generation is null ? throw new ArgumentNullException(nameof(generation))
             : ReferenceEquals(generation, Empty) ? this
             : ReferenceEquals(this, Empty) ? generation
             : new Generation(Modifiers.AddRange(generation.Modifiers),
                              BaseTypes.AddRange(generation.BaseTypes),
                              Members.AddRange(generation.Members));

        public Generation WithModifiers(SyntaxTokenList value) =>
            new Generation(value, BaseTypes, Members);

        public Generation WithBaseList(ImmutableArray<BaseTypeSyntax> value) =>
            new Generation(Modifiers, value, Members);

        public Generation WithMembers(ImmutableArray<MemberDeclarationSyntax> value) =>
            new Generation(Modifiers, BaseTypes, value);

        public Generation AddMember(MemberDeclarationSyntax member) =>
            new Generation(Modifiers, BaseTypes, Members.Add(member));

        public Generation AddMembers(IEnumerable<MemberDeclarationSyntax> members) =>
            new Generation(Modifiers, BaseTypes, Members.AddRange(members));

        public Generation AddMembers(params MemberDeclarationSyntax[] members) =>
            new Generation(Modifiers, BaseTypes, Members.AddRange(members));
    }
}
