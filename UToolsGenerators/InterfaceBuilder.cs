using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTools.SourceGenerators
{
    public class InterfaceBuilder
    {
        private readonly HashSet<string> m_UsedSignatures = new();

        public string Key { get; }
        public ITypeSymbol ReferenceInterface { get; }

        private readonly List<MemberDeclarationSyntax> m_Members = new();

        public InterfaceBuilder(string key, ITypeSymbol referenceInterface)
        {
            Key = key;
            ReferenceInterface = referenceInterface;
        }

        public void AddInterfaceSubscriptionMethod(TypeSyntax fieldType, string methodName)
        {
            var key = CreateKey(fieldType, methodName);
            if (!m_UsedSignatures.Add(key))
                return;

            m_Members.Add(CreateInterfaceSubscriptionMethod(fieldType, methodName));
        }

        public void AddInterfaceSubscriptionEvent(TypeSyntax fieldType, string eventName)
        {
            var key = CreateKey(fieldType, eventName);
            if (!m_UsedSignatures.Add(key))
                return;

            m_Members.Add(CreateInterfaceSubscriptionEvent(fieldType, eventName));
        }

        public void AddInterfaceProperty(TypeSyntax fieldType, string propertyName)
        {
            var key = CreateKey(fieldType, propertyName);
            if (!m_UsedSignatures.Add(key))
                return;

            m_Members.Add(CreateInterfaceProperty(fieldType, propertyName));
        }

        private string CreateKey(TypeSyntax fieldType, string methodName) => $"{fieldType}{methodName}";

        /// <summary>
        /// Converts the reference interface to a simple base type syntax.
        /// </summary>
        /// <returns>A <see cref="SimpleBaseTypeSyntax"/> representing the reference interface.</returns>
        public SimpleBaseTypeSyntax ToSimpleBaseTypeSyntax()
        {
            var interfaceIdentifier = ReferenceInterface.Name;
            var interfaceType = SyntaxFactory.ParseTypeName(interfaceIdentifier);
            return SyntaxFactory.SimpleBaseType(interfaceType);
        }

        /// <summary>
        /// Converts the interface builder container to a syntax node.
        /// </summary>
        /// <returns>A <see cref="MemberDeclarationSyntax"/> representing the interface builder container.</returns>
        public MemberDeclarationSyntax ToSyntaxNode()
        {
            var interfaceNode = SyntaxFactory.InterfaceDeclaration(ReferenceInterface.Name)
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(SyntaxFactory.List(m_Members));

            ReferenceInterface.TryGetAnyDeclaration(out var declaration);
            return declaration.CopyHierarchyTo(interfaceNode);
        }

        /// <summary>
        /// Creates a method declaration for an interface that allows subscribing to an event.
        /// </summary>
        /// <param name="fieldType">The type of the event's data.</param>
        /// <param name="methodName">The name of the method to be created.</param>
        /// <returns>A MethodDeclarationSyntax object representing the created method.</returns>
        public static MethodDeclarationSyntax CreateInterfaceSubscriptionMethod(TypeSyntax fieldType, string methodName)
        {
            var returnType = SyntaxFactory.IdentifierName("IDisposable");
            var parameterType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("EventHandler"))
                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(fieldType)));
            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("handler")).WithType(parameterType);

            var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(parameter)))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            return methodDeclaration;
        }

        /// <summary>
        /// Creates an event field declaration for an interface.
        /// </summary>
        /// <param name="fieldType">The type of the event's data.</param>
        /// <param name="eventName">The name of the event to be created.</param>
        /// <returns>An EventFieldDeclarationSyntax object representing the created event field.</returns>
        public static EventFieldDeclarationSyntax CreateInterfaceSubscriptionEvent(TypeSyntax fieldType, string eventName)
        {
            var genericType = $"EventHandler<{fieldType}>";
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            return SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(genericType))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(eventName))))
                .WithModifiers(modifiers);
        }

        /// <summary>
        /// Creates a property with simple get and set accessors.
        /// </summary>
        /// <param name="fieldType">The type of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>A PropertyDeclarationSyntax representing the created property.</returns>
        public static PropertyDeclarationSyntax CreateInterfaceProperty(TypeSyntax fieldType, string propertyName)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            var getAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(fieldType, propertyName)
                .WithModifiers(modifiers)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { getAccessor, setAccessor })));

            return propertyDeclaration;
        }
    }
}