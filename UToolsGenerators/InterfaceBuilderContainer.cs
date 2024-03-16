using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeSyntax;

namespace UTools.SourceGenerators
{
    public class InterfaceBuilderContainer
    {
        public string Key { get; }
        public ITypeSymbol ReferenceInterface { get; set; }

        public List<MemberDeclarationSyntax> Members { get; } = new();

        public InterfaceBuilderContainer(string key, ITypeSymbol referenceInterface)
        {
            Key = key;
            ReferenceInterface = referenceInterface;
        }
        
        public SimpleBaseTypeSyntax ToSimpleBaseTypeSyntax()
        {
            var interfaceIdentifier = ReferenceInterface.Name;
            var interfaceType = SyntaxFactory.ParseTypeName(interfaceIdentifier);
            return SyntaxFactory.SimpleBaseType(interfaceType);
        }

        public MemberDeclarationSyntax ToSyntaxNode()
        {
            var interfaceNode = SyntaxFactory.InterfaceDeclaration(ReferenceInterface.Name)
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(SyntaxFactory.List(Members));

            ReferenceInterface.TryGetAnyDeclaration(out var declaration);
            return declaration.CopyHierarchyTo(interfaceNode);
        }
    }
}