using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTools.SourceGenerators
{
    public static class ClassDeclarationHelpers
    {
        /// <summary>
        /// Retrieves all syntax nodes of a specific type from the given compilation.
        /// </summary>
        /// <typeparam name="T">The type of syntax nodes to retrieve.</typeparam>
        /// <param name="compilation">The compilation to retrieve the syntax nodes from.</param>
        /// <returns>An IEnumerable of syntax nodes of the specified type.</returns>
        public static IEnumerable<T> GetNodesOfType<T>(this Compilation compilation) where T : SyntaxNode
        {
            return compilation.GetNodes().OfType<T>();
        }

        /// <summary>
        /// Retrieves all syntax nodes of a specific type from the given compilation that satisfy a specified condition.
        /// </summary>
        /// <typeparam name="T">The type of syntax nodes to retrieve.</typeparam>
        /// <param name="compilation">The compilation to retrieve the syntax nodes from.</param>
        /// <param name="predicate">A function to test each syntax node for a condition.</param>
        /// <returns>An IEnumerable of syntax nodes of the specified type that satisfy the condition specified by predicate.</returns>
        public static IEnumerable<T> GetNodesOfType<T>(this Compilation compilation, Func<T, bool> predicate) where T : SyntaxNode
        {
            return compilation.GetNodesOfType<T>().Where(predicate);
        }

        /// <summary>
        /// Retrieves all syntax nodes from the given compilation.
        /// </summary>
        /// <param name="compilation">The compilation to retrieve the syntax nodes from.</param>
        /// <returns>An IEnumerable of all syntax nodes in the compilation.</returns>
        public static IEnumerable<SyntaxNode> GetNodes(this Compilation compilation)
        {
            return compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodes());
        }

        public static IEnumerable<(ClassDeclarationSyntax Class, SyntaxList<UsingDirectiveSyntax>? Usings)> GetClassWithUsing(this Compilation compilation)
        {
            return compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Select(classNode => (
                    Class: classNode,
                    Usings: classNode.AncestorsAndSelf()
                        .OfType<CompilationUnitSyntax>()
                        .FirstOrDefault()?
                        .Usings
                ));
        }

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
        ///  Get all classes that have a specific attribute
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static IEnumerable<ClassDeclarationSyntax> GetClassesByFieldAttribute(this Compilation compilation, string attribute)
        {
            return compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.Members.OfType<FieldDeclarationSyntax>()
                    .Any(field => field.HasAttribute(attribute)));
        }

        /// <summary>
        ///  Get all classes that have a specific attribute
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static IEnumerable<ClassDeclarationSyntax> GetClassesByAttribute(this Compilation compilation, string attribute)
        {
            return compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.HasAttribute(attribute));
        }
    }

    public static class SourceGeneratorHelpers
    {
        /// <summary>
        /// Changes the TEST_TEXT_EXAMPLE to TestTextExample
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConstToCamelCase(this string str)
        {
            var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                words[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower();
            }

            return string.Concat(words);
        }


        public static bool HasAttribute(this MemberDeclarationSyntax fieldSyntax, string attribute)
        {
            return fieldSyntax.AttributeLists.HasAttribute(attribute);
        }

        private static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributeListSyntax, string attribute)
        {
            var longAttribute = attribute + "Attribute";
            return attributeListSyntax.Any(list =>
                list.Attributes.Any(attr =>
                    attr.Name.ToString() == attribute || attr.Name.ToString() == longAttribute));
        }

        // determine the namespace the class/enum/struct is declared in, if any
        //implementation from https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/
        public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = string.Empty;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while (potentialNamespaceParent != null &&
                   !(potentialNamespaceParent is NamespaceDeclarationSyntax))
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (!(namespaceParent.Parent is NamespaceDeclarationSyntax parent))
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }
    }
}