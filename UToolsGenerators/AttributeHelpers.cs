using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTools.SourceGenerators
{
    public static class AttributeHelpers
    {
        /// <summary>
        ///  Get all fields of a class that are constants and have a specific attribute
        /// </summary>
        /// <param name="classDeclaration"></param>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDeclarationSyntax> GetConstantsOfTypeByAttribute(
            this ClassDeclarationSyntax classDeclaration,
            string attribute,
            string type)
        {
            return classDeclaration.Members.OfType<FieldDeclarationSyntax>().Where(
                fieldDeclaration =>
                {
                    var hasAttr = fieldDeclaration.HasAttribute(attribute);
                    var isConst = fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword);
                    var isStringType = fieldDeclaration.Declaration.Type.ToString() == type;
                    return hasAttr && isConst && isStringType;
                }
            );
        }

        /// <summary>
        ///  Get all classes that have a specific attribute on a field
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IEnumerable<ClassDeclarationSyntax> GetClassesByFieldAttributes(this Compilation compilation, params string[] attributes)
        {
            return compilation.GetNodesOfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.Members.OfType<FieldDeclarationSyntax>()
                    .Any(field => attributes.Any(field.HasAttribute)));
        }

        /// <summary>
        ///  Get all classes that have a specific attribute
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IEnumerable<ClassDeclarationSyntax> GetClassesByAttributes(this Compilation compilation, params string[] attributes)
        {
            return compilation.GetNodesOfType<ClassDeclarationSyntax>()
                .Where(classDec => attributes.Any(classDec.HasAttribute));
        }

        public static bool TryGetTypeFromAttributeInterfaceProperty(
            this Compilation compilation,
            FieldDeclarationSyntax fieldNode,
            string attributeName,
            out List<ITypeSymbol> results)
        {
            results = new List<ITypeSymbol>();
            var model = compilation.GetSemanticModel(fieldNode.SyntaxTree);
            var attributes = fieldNode.AttributeLists.SelectMany(attrList => attrList.Attributes);

            foreach (var attribute in attributes)
            {
                var attributeSymbol = model.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                if (attributeSymbol?.ContainingType.Name != attributeName)
                    continue;
                if (attribute.ArgumentList == null)
                    continue;

                foreach (var arg in attribute.ArgumentList.Arguments)
                {
                    if (arg.NameEquals != null)
                    {
                        if (arg.Expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
                        {
                            foreach (var expr in implicitArrayCreation.Initializer.Expressions.OfType<TypeOfExpressionSyntax>())
                                AddTypeSymbolFromTypeOfExpression(model, results, expr);
                        }
                        else if (arg.Expression is ArrayCreationExpressionSyntax arrayCreation)
                        {
                            if (arrayCreation.Initializer != null)
                                foreach (var expr in arrayCreation.Initializer.Expressions.OfType<TypeOfExpressionSyntax>())
                                    AddTypeSymbolFromTypeOfExpression(model, results, expr);
                        }
                    }
                    else if (arg.Expression is TypeOfExpressionSyntax typeOfExpression)
                    {
                        AddTypeSymbolFromTypeOfExpression(model, results, typeOfExpression);
                    }
                }
            }

            return results.Count > 0;
        }

        private static void AddTypeSymbolFromTypeOfExpression(SemanticModel model, List<ITypeSymbol> results, TypeOfExpressionSyntax typeOfExpression)
        {
            var typeSymbol = model.GetTypeInfo(typeOfExpression.Type).Type;
            if (typeSymbol is { TypeKind: TypeKind.Interface })
                results.Add(typeSymbol);
            else
                throw new InvalidOperationException("Specified type must be an interface.");
        }
    }
}