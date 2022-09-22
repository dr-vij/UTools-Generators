using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace ShaderCodeGen
{
    public static class CodeGenHelpers
    {
        public static string ToCamelCase(this string str)
        {
            var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                m => m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
            var tailWords = words.Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();
            return $"{leadWord}{string.Join(string.Empty, tailWords)}";
        }
    }

    [Generator]
    public class ShaderSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var attributeWord = "Attribute";

            var classAttribute = "ShaderPropertiesProvider";
            var classAttributeLong = classAttribute + attributeWord;

            var methodAttribute = "ShaderProperty";
            var methodAttributeLong = methodAttribute + attributeWord;

            //find classes with ShaderPropertiesProvider attribute
            var classes = context.Compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.AttributeLists.Any(y =>
                    y.Attributes.Any(
                        z => z.Name.ToString() == classAttributeLong || z.Name.ToString() == classAttribute)))
                .ToList();

            var declarationsBuilder = new StringBuilder();
            var initializationBuilder = new StringBuilder();

            foreach (var classDeclaration in classes)
            {
                var stringFields = classDeclaration.Members.OfType<FieldDeclarationSyntax>().Where(
                    fieldDec =>
                    {
                        var hasAttr = fieldDec.AttributeLists.Any(list => list.Attributes.Any(attr =>
                            attr.Name.ToString() == methodAttributeLong ||
                            attr.Name.ToString() == methodAttribute));
                        var isConst = fieldDec.Modifiers.Any(x => x.Text == "const");
                        var isStringType = fieldDec.Declaration.Type.ToString() == "string";
                        return hasAttr && isConst && isStringType;
                    }
                );

                //get string fields, and add int declaration for them
                foreach (var field in stringFields)
                {
                    var stringField = field.Declaration.Variables.First().Identifier.Text;
                    var intField = stringField.ToCamelCase() + "PropertyId";
                    declarationsBuilder.AppendLine($"public int {intField};");

                    //initialization
                    initializationBuilder.Append(
                        @"
                        " +
                        $"{intField} = Shader.PropertyToID({stringField});" +
                        $""
                    );
                }
            }
        }
        
        public IEnumerable<FieldDeclarationSyntax> GetConstantsOfTypeByAttribute(
            ClassDeclarationSyntax classDeclaration,
            string attribute,
            string type)
        {
            return classDeclaration.Members.OfType<FieldDeclarationSyntax>().Where(
                fieldDeclaration =>
                {
                    var hasAttr = fieldDeclaration.AttributeLists.Any(list => list.Attributes.Any(attr => attr.Name.ToString() == attribute));
                    var isConst = fieldDeclaration.Modifiers.Any(x => x.Text == "const");
                    var isStringType = fieldDeclaration.Declaration.Type.ToString() == type;
                    return hasAttr && isConst && isStringType;
                }
            );
        }

        private IEnumerable<ClassDeclarationSyntax> GetClassesByAttribute(Compilation compilation, string attribute)
        {
            var attributeLong = attribute + "Attribute";
            return compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.AttributeLists.Any(y =>
                    y.Attributes.Any(
                        z => z.Name.ToString() == attributeLong || z.Name.ToString() == attribute)));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}