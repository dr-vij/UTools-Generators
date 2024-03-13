using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UTools.SourceGenerators
{
    public static class SourceGeneratorHelpers
    {
        private static readonly Regex PrefixRemover = new Regex("^[a-z_]+", RegexOptions.Compiled);

        public static string RemovePrefix(this string str)
        {
            return PrefixRemover.Replace(str, "");
        }

        /// <summary>
        /// Changes the TEST_TEXT_EXAMPLE to TestTextExample
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConstToCamelCase(this string str)
        {
            var words = str.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
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

        //implementation from https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/
        /// <summary>
        /// determine the namespace the class/enum/struct is declared in, if any
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
        {
            var nameSpace = string.Empty;
            SyntaxNode potentialNamespaceParent = syntax.Parent;
            while (potentialNamespaceParent != null && !(potentialNamespaceParent is NamespaceDeclarationSyntax))
                potentialNamespaceParent = potentialNamespaceParent.Parent;

            if (potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
            {
                nameSpace = namespaceParent.Name.ToString();
                while (true)
                {
                    if (!(namespaceParent.Parent is NamespaceDeclarationSyntax parent))
                        break;
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            return nameSpace;
        }
    }
}