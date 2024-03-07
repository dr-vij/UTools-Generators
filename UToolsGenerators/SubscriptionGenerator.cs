using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UTools.SourceGenerators
{
    [Generator]
    public class SubscriptionGenerator : ISourceGenerator
    {
        static readonly Regex PrefixRemover = new Regex("^[a-z_]+", RegexOptions.Compiled);

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var originalClassesWithUsing = context.Compilation.GetClassWithUsing();

            foreach (var originalClassWithUsing in originalClassesWithUsing)
            {
                var classNamespace = originalClassWithUsing.Class.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                var fields = originalClassWithUsing.Class.Members.OfType<FieldDeclarationSyntax>().Where(field => field.HasAttribute("SubscriptionField")).ToArray();
                if (fields.Length != 0)
                {
                    var newClass = originalClassWithUsing.Class.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>());
                    foreach (var field in fields)
                    {
                        var isStatic = field.Modifiers.Any(SyntaxKind.StaticKeyword);
                        var fieldType = field.Declaration.Type;
                        var fieldName = field.Declaration.Variables.First().Identifier.Text;

                        var propertyName = PrefixRemover.Replace(fieldName, "");

                        //Prepare modifiers for the event. We want it to be private, and static if the field is static
                        var eventFieldModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                        if (isStatic)
                            eventFieldModifiers = eventFieldModifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

                        //Prepare the generic type of EventHandler<T> here based on field type
                        var eventHandlerType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("EventHandler"))
                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(fieldType)));

                        //Creates an event field with the name {PropertyName}Changed
                        var publicEventName = propertyName + "Changed";
                        var privateEventName = fieldName + "Changed";
                        var eventFieldDeclarationSyntax = SyntaxFactory.EventFieldDeclaration(
                            SyntaxFactory.VariableDeclaration(eventHandlerType)
                                .AddVariables(SyntaxFactory.VariableDeclarator(fieldName + "Changed"))).WithModifiers(eventFieldModifiers);

                        //Creates public part of event:
                        var statements = SyntaxFactory.List(new StatementSyntax[]
                        {
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"value?.Invoke(this, {fieldName})")),
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{privateEventName} += value"))
                        });

                        var addAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration)
                            .WithBody(SyntaxFactory.Block(statements));

                        var removeAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.ParseExpression($"{privateEventName} -= value;"))
                                ))
                            );

                        var eventSyntax = SyntaxFactory.EventDeclaration(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("EventHandler"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(fieldType))),
                                SyntaxFactory.Identifier(publicEventName))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { addAccessor, removeAccessor })));

                        //Creates a partial method with the name On{PropertyName}Change
                        var partialMethodName = "On" + propertyName + "Change";
                        var partialMethodSyntax = SyntaxFactory
                            .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), partialMethodName)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                            .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("newValue")).WithType(fieldType))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                        //Now we create a property with the name {PropertyName} and type {FieldType}
                        var getAccessorStatements = SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName))
                        );

                        var getAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithBody(SyntaxFactory.Block(getAccessorStatements));

                        var condition = SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression,
                            SyntaxFactory.IdentifierName(fieldName),
                            SyntaxFactory.IdentifierName("value")
                        );

                        var assignmentStatement = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(fieldName),
                                SyntaxFactory.IdentifierName("value")
                            )
                        );

                        var invokeExpression = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.ConditionalAccessExpression(
                                SyntaxFactory.IdentifierName(privateEventName),
                                SyntaxFactory.InvocationExpression(SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Invoke")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList(
                                                new[]
                                                {
                                                    SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                                                }
                                            )
                                        )
                                    )
                            )
                        );

                        var partialMethodInvocation = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName(partialMethodName),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                                    )
                                )
                            )
                        );

                        var ifStatement = SyntaxFactory.IfStatement(
                            condition,
                            SyntaxFactory.Block(SyntaxFactory.List(new StatementSyntax[]
                            {
                                assignmentStatement,
                                invokeExpression,
                                partialMethodInvocation
                            }))
                        );

                        var setAccessorStatements = SyntaxFactory.List(new StatementSyntax[]
                        {
                            ifStatement
                        });

                        var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithBody(SyntaxFactory.Block(setAccessorStatements));

                        var propertySyntax = SyntaxFactory.PropertyDeclaration(fieldType, propertyName)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                getAccessor,
                                setAccessor
                            );

                        newClass = newClass.AddMembers(eventFieldDeclarationSyntax);
                        newClass = newClass.AddMembers(eventSyntax);
                        newClass = newClass.AddMembers(propertySyntax);
                        newClass = newClass.AddMembers(partialMethodSyntax);
                    }

                    //We take the original usings and add them to the new class + System + UnityEngine
                    var compilationUnit = SyntaxFactory.CompilationUnit().AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("UnityEngine"))
                    );
                    if (originalClassWithUsing.Usings != null)
                    {
                        foreach (var usingDirective in originalClassWithUsing.Usings.Value)
                        {
                            if (compilationUnit.Usings.All(c => c.Name.ToString() != usingDirective.Name.ToString()))
                                compilationUnit = compilationUnit.AddUsings(usingDirective);
                        }
                    }

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
                    context.AddSource(originalClassWithUsing.Class.Identifier.Text + "Generated.cs", SourceText.From(code, Encoding.UTF8));
                }
            }
        }
    }
}