using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class SimpleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SubscriptionSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SubscriptionSyntaxReceiver receiver)
            return;

        foreach (var field in receiver.FieldsWithSubscriptionAttribute)
        {
            var fieldName = field.Declaration.Variables.First().Identifier.Text;
            var fieldType = ((PredefinedTypeSyntax)field.Declaration.Type).Keyword.Text;
            var className = ((ClassDeclarationSyntax)field.Parent).Identifier.Text;
            var cleanedName = fieldName.TrimStart('m', '_');
            var propertyName = char.ToUpper(cleanedName[0]) + cleanedName.Substring(1);
            var eventName = $"On{propertyName}Changed";


            var eventDeclaration = GenerateEvent(fieldType, eventName);
            var propertyDeclaration = GenerateProperty(fieldType, propertyName, fieldName, eventName);

            var classNode = GeneratePartialClass(className)
                .AddMembers(eventDeclaration)
                .AddMembers(propertyDeclaration);

            var unit = SyntaxFactory.CompilationUnit().AddMembers(classNode);
            var code = unit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{className}_{fieldName}_Subscription.cs", code);
        }
    }

    public static ClassDeclarationSyntax GeneratePartialClass(string className)
    {
        return SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));
    }

    public EventFieldDeclarationSyntax GenerateEvent(string fieldType, string eventName)
    {
        var eventIdentifier = SyntaxFactory.Identifier(eventName);
        var actionType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("System.Action"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                        SyntaxFactory.IdentifierName(fieldType))));

        return SyntaxFactory.EventFieldDeclaration(
            SyntaxFactory.VariableDeclaration(actionType)
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(eventIdentifier))));
    }

    public PropertyDeclarationSyntax GenerateProperty(string fieldType, string propertyName, string fieldName, string eventName)
    {
        var propertyCode = $@"
  public {fieldType} {propertyName}
     {{
         get => {fieldName};
         set
         {{
             if ({fieldName} != value)
             {{
                 {fieldName} = value;
                 {eventName}?.Invoke({fieldName});
             }}
         }}
     }}
";
        return (PropertyDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(propertyCode);
    }
}


public class SubscriptionSyntaxReceiver : ISyntaxReceiver
{
    public readonly List<FieldDeclarationSyntax> FieldsWithSubscriptionAttribute = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            var attributes = fieldDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes);
            if (attributes.Any(a => a.Name.ToString() == "Subscription"))
            {
                FieldsWithSubscriptionAttribute.Add(fieldDeclarationSyntax);
            }
        }
    }
}
//
//
// public class SimpleSourceGenerator : ISourceGenerator
// {
//     public void Initialize(GeneratorInitializationContext context)
//     {
//     }
//
//     public void Execute(GeneratorExecutionContext context)
//     {
//         foreach (var syntaxTree in context.Compilation.SyntaxTrees)
//         {
//             foreach (var node in syntaxTree.GetRoot().DescendantNodes())
//             {
//             }
//         }
//     }
// }
//
//
// public class SyntaxReceiver : ISyntaxReceiver
// {
//     public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//     {
//     }
// }


//
// public void Execute(GeneratorExecutionContext context)
// {
//     if (context.SyntaxReceiver is not SubscriptionSyntaxReceiver receiver)
//         return;
//
//     foreach (var field in receiver.FieldsWithSubscriptionAttribute)
//     {
//         var fieldName = field.Declaration.Variables.First().Identifier.Text;
//         var fieldType = ((PredefinedTypeSyntax)field.Declaration.Type).Keyword.Text;
//         var className = ((ClassDeclarationSyntax)field.Parent).Identifier.Text;
//         var cleanedName = fieldName.TrimStart('m', '_');
//         var propertyName = char.ToUpper(cleanedName[0]) + cleanedName.Substring(1);
//         var eventName = $"On{propertyName}Changed";
//
//         var source = $@"
// public partial class {className}
// {{
//     public event System.Action<{fieldType}> {eventName};
//
//     public {fieldType} {propertyName}
//     {{
//         get => {fieldName};
//         set
//         {{
//             if ({fieldName} != value)
//             {{
//                 {fieldName} = value;
//                 {eventName}?.Invoke({fieldName});
//             }}
//         }}
//     }}
// }}";
//         context.AddSource($"{className}_{fieldName}_Subscription.cs", source);
//     }
// }