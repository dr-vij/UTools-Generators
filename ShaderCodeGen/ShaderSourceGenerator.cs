using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public static IEnumerable<ClassDeclarationSyntax> GetClassesByAttribute(this Compilation compilation, string attribute)
        {
            return compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(classDec => classDec.HasAttribute(attribute));
        }

        public static bool HasAttribute(this FieldDeclarationSyntax fieldSyntax, string attribute)
        {
            return fieldSyntax.AttributeLists.HasAttribute(attribute);
        }
        
        public static bool HasAttribute(this ClassDeclarationSyntax classSyntax, string attribute)
        {
            return classSyntax.AttributeLists.HasAttribute(attribute);
        }

        private static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributeListSyntaxes, string attribute)
        {
            var longAttribute = attribute + "Attribute";
            return attributeListSyntaxes.Any(list =>
                list.Attributes.Any(attr =>
                    attr.Name.ToString() == attribute || attr.Name.ToString() == longAttribute));
        }
    }

    [Generator]
    public class ShaderSourceGenerator : ISourceGenerator
    {
        private const string ShaderAttribute = "Shader";
        private const string ShaderPropertiesProviderAttribute = "ShaderPropertiesProvider";
        private const string ShaderPropertyAttribute = "ShaderProperty";

        private readonly  StringBuilder _declarationsBuilder = new StringBuilder();
        private readonly StringBuilder _initializationBuilder = new StringBuilder();
        private readonly StringBuilder _classBuilder = new StringBuilder();

        public void Execute(GeneratorExecutionContext context)
        {
            //find classes with ShaderPropertiesProvider attribute
            var classes = context.Compilation.GetClassesByAttribute(ShaderPropertiesProviderAttribute);

            foreach (var classDeclaration in classes)
            {
                _declarationsBuilder.Clear();
                _initializationBuilder.Clear();
                _classBuilder.Clear();
                
                var shaderPropertyFields = classDeclaration.GetConstantsOfTypeByAttribute(ShaderPropertyAttribute, "string");
                var shaders = classDeclaration.GetConstantsOfTypeByAttribute(ShaderAttribute, "string");
                
                //get shader properties and create in propertyIds for them;
                foreach (var shaderPropertyField in shaderPropertyFields)
                {
                    var stringField = shaderPropertyField.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shaderPropertyField.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ToCamelCase() + "PropertyId";
                   
                    //add declaration and initialization for shaders
                    _declarationsBuilder.AppendLine($"public readonly int {intField};");
                    _initializationBuilder.AppendLine($"{intField} = Shader.PropertyToID({stringFieldValue})");
                }
                
                //get shaders and create in shaderIds for them;
                var shaderCounter = 0;
                foreach (var shader in shaders)
                {
                    var stringField = shader.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shader.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ToCamelCase() + "ShaderId";
                    
                    //add declaration and initialization for shaders
                    _declarationsBuilder.AppendLine($"public readonly int {intField};");
                    
                    var shaderVariable = stringField + shaderCounter++;
                    _initializationBuilder.AppendLine($"{intField} = Shader.PropertyToID({stringFieldValue});");
                    _initializationBuilder.AppendLine($"var {shaderVariable} = Shader.Find({stringFieldValue});");
                    _initializationBuilder.AppendLine($"m_shaders[{intField}] = {shaderVariable};");
                    _initializationBuilder.AppendLine($"m_materials[{intField}] = new Material({shaderVariable});");
                }

                //TODO:
                //get class current namespace:
                _classBuilder.Append(
                    $"public partial class {classDeclaration.Identifier.Text}" + 
                                     @"
                {
                " +
                                     _declarationsBuilder +
                                     @"
                    private void Init()
                    {
                " + 
                                     _initializationBuilder +
                                     @"
                    }
                }
                ");
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}