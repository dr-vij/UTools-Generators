using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UTools.Attributes;

namespace UTools.SourceGenerators
{
    [Generator]
    public class DisposableSubscriptionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var attributeName = nameof(DisposableSubscription);

            var classNodes = compilation.GetClassesByFieldAttribute(attributeName);

            foreach (var classNode in classNodes)
            {
                var className = classNode.Identifier.Text;
                var fieldNodes = classNode.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(fieldNode => fieldNode.HasAttribute(attributeName));

                var newClass = classNode.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>()).WithoutTrivia();
                foreach (var field in fieldNodes)
                {
                    var isStatic = field.Modifiers.Any(SyntaxKind.StaticKeyword);
                    var fieldType = field.Declaration.Type;
                
                    var fieldName = field.Declaration.Variables.First().Identifier.Text;
                    var eventName = $"{fieldName}Changed";
                    var propertyName = fieldName.RemovePrefix();
                    var subscriptionMethodName = $"SubscribeTo{propertyName}";
                    var partialMethodName = $"On{propertyName}Change";
                
                    var eventField = CreateEventField(fieldType, eventName, isStatic);
                    var propertyDeclaration = CreatePropertyAndCallbacks(fieldType, propertyName, fieldName, partialMethodName, eventName, isStatic);
                    var subscriptionMethod = CreateDisposableSubscriptionMethod(fieldType, subscriptionMethodName, fieldName, eventName, isStatic);
                    var partialMethod = CreatePartialMethod(partialMethodName, fieldType, isStatic);
                
                    newClass = newClass.AddMembers(eventField, propertyDeclaration, subscriptionMethod, partialMethod);
                }

                var compilationUnit = SyntaxFactory.CompilationUnit()
                    .AddUsings(classNode.GetUsingArr());

                var classNamespace = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (classNamespace != null)
                {
                    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(classNamespace.Name).AddMembers(newClass);
                    compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
                }
                else
                {
                    compilationUnit = compilationUnit.AddMembers(newClass);
                }

                var code = compilationUnit
                    .NormalizeWhitespace()
                    .ToFullString();
                context.AddSource(className + "Generated.cs", SourceText.From(code, Encoding.UTF8));
            }
        }

        /// <summary>
        /// Creates the event subscription for the given event name and field name
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private static EventDeclarationSyntax CreateEventSubscription(TypeSyntax fieldType, string eventName, string fieldName, bool isStatic)
        {
            var eventHandlerType = isStatic ? $"Action<{fieldType}>" : $"EventHandler<{fieldType}>";
            var optionalStatic = isStatic ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)) : SyntaxFactory.TokenList();

            var addAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration).WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName(fieldName)),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")))))),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, SyntaxFactory.IdentifierName(eventName),
                        SyntaxFactory.IdentifierName("value")))
                )
            );

            var removeAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration).WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, SyntaxFactory.IdentifierName(eventName),
                        SyntaxFactory.IdentifierName("value")))
                )
            );

            var eventDeclaration = SyntaxFactory.EventDeclaration(SyntaxFactory.ParseTypeName(eventHandlerType), eventName)
                .AddModifiers(optionalStatic.ToArray())
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { addAccessor, removeAccessor })));

            return eventDeclaration;
        }

        /// <summary>
        /// Create the disposable subscription method for the given event name and field name
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="subscriptionMethodName"></param>
        /// <param name="fieldName"></param>
        /// <param name="eventName"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private static MethodDeclarationSyntax CreateDisposableSubscriptionMethod(
            TypeSyntax fieldType,
            string subscriptionMethodName,
            string fieldName,
            string eventName,
            bool isStatic)
        {
            var eventHandlerTypeName = isStatic ? "Action" : "EventHandler";
            var eventHandlerTypeArgumentList = SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(fieldType));
            var eventHandlerType = SyntaxFactory.GenericName(SyntaxFactory.Identifier(eventHandlerTypeName))
                .WithTypeArgumentList(eventHandlerTypeArgumentList);

            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("handler"))
                .WithType(eventHandlerType);

            var eventNameExpression = SyntaxFactory.IdentifierName(eventName);
            var addAssignment = SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, eventNameExpression, SyntaxFactory.IdentifierName("handler"));
            var removeAssignment =
                SyntaxFactory.AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, eventNameExpression, SyntaxFactory.IdentifierName("handler"));

            var handlerInvokeArguments = isStatic
                ? SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName))))
                : SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName))
                        }
                    )
                );

            var handlerInvokeStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.ConditionalAccessExpression(
                    SyntaxFactory.IdentifierName("handler"),
                    SyntaxFactory.InvocationExpression(SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Invoke")))
                        .WithArgumentList(handlerInvokeArguments)));

            var returnStatement = SyntaxFactory.ReturnStatement(
                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("Subscription"))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(removeAssignment))))));

            var methodBody = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(addAssignment),
                handlerInvokeStatement,
                returnStatement);

            var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName("IDisposable"), SyntaxFactory.Identifier(subscriptionMethodName))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(parameter)))
                .WithBody(methodBody);

            return methodDeclaration;
        }

        /// <summary>
        /// Create the event field declaration
        /// </summary>
        /// <param name="fieldType">the event parameters type</param>
        /// <param name="eventName">name of generated field</param>
        /// <param name="isStatic">makes event declaration static when true</param>
        /// <returns></returns>
        private static EventFieldDeclarationSyntax CreateEventField(TypeSyntax fieldType, string eventName, bool isStatic = false)
        {
            var eventHandlerType = isStatic ? $"Action<{fieldType}>" : $"EventHandler<{fieldType}>";

            var modifiers = isStatic ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)) : SyntaxFactory.TokenList();
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(eventHandlerType))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(eventName))))
                .WithModifiers(modifiers);
        }

        /// <summary>
        /// Create the partial void method with given parameter
        /// </summary>
        /// <param name="methodName">the name of generated method</param>
        /// <param name="parameterType">type of generated method parameter</param>
        /// <param name="isStatic">makes method declaration static when true</param>
        /// <returns></returns>
        private static MethodDeclarationSyntax CreatePartialMethod(string methodName, TypeSyntax parameterType, bool isStatic)
        {
            var modifiers = isStatic
                ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                : SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), methodName)
                .WithModifiers(modifiers)
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Parameter(SyntaxFactory.Identifier("newValue")).WithType(parameterType))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        /// <summary>
        /// Create the property declaration with given field name and type.
        /// the property checks the field changes, and invokes OnChanged event and partial class
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="fieldName"></param>
        /// <param name="methodCallbackName"></param>
        /// <param name="eventCallbackName"></param>
        /// <param name="fieldType"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private PropertyDeclarationSyntax CreatePropertyAndCallbacks(
            TypeSyntax fieldType,
            string propertyName,
            string fieldName,
            string methodCallbackName,
            string eventCallbackName,
            bool isStatic)
        {
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (isStatic)
                modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            var eventInvocation = isStatic ? $"{eventCallbackName}?.Invoke(value);" : $"{eventCallbackName}?.Invoke(this, m_TestField)";

            var getAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithBody(
                SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName)))));

            var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName("value")),
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName("value"))),
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(eventInvocation)),
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(methodCallbackName),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))))))
                        )
                    )
                )
            );

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(fieldType, propertyName)
                .WithModifiers(modifiers)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { getAccessor, setAccessor })));

            return propertyDeclaration;
        }
    }
}