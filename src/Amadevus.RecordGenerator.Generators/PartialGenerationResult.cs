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

        public bool IsEmpty => ReferenceEquals(this, Empty);

        public PartialGenerationResult Add(PartialGenerationResult result)
            => result is null ? throw new ArgumentNullException(nameof(result))
             : result.IsEmpty ? this
             : this.IsEmpty ? result
             : Update(Modifiers.AddRange(result.Modifiers),
                      BaseTypes.AddRange(result.BaseTypes),
                      Members.AddRange(result.Members));

        public PartialGenerationResult WithModifiers(SyntaxTokenList value) =>
            Update(value, BaseTypes, Members);

        public PartialGenerationResult WithBaseList(ImmutableArray<BaseTypeSyntax> value) =>
            Update(Modifiers, value, Members);

        public PartialGenerationResult WithMembers(ImmutableArray<MemberDeclarationSyntax> value) =>
            Update(Modifiers, BaseTypes, value);

        public PartialGenerationResult AddMember(MemberDeclarationSyntax member) =>
            Update(Modifiers, BaseTypes, Members.Add(member));

        public PartialGenerationResult AddMembers(IEnumerable<MemberDeclarationSyntax> members) =>
            Update(Modifiers, BaseTypes, Members.AddRange(members));

        public PartialGenerationResult AddMembers(params MemberDeclarationSyntax[] members) =>
            Update(Modifiers, BaseTypes, Members.AddRange(members));

        private PartialGenerationResult Update(
                SyntaxTokenList modifiers,
                ImmutableArray<BaseTypeSyntax> baseTypes,
                ImmutableArray<MemberDeclarationSyntax> members)
            => modifiers.Count == 0 && baseTypes.IsEmpty && members.IsEmpty
             ? Empty
             : new PartialGenerationResult(modifiers, baseTypes, members);
    }
}
