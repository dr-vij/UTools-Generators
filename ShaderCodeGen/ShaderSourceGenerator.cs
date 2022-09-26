using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShaderCodeGen
{
    [Generator]
    public class ShaderSourceGenerator : ISourceGenerator
    {
        private const string ShaderAttribute = "Shader";
        private const string ShaderPropertiesProviderAttribute = "ShaderPropertiesProvider";
        private const string ShaderPropertyAttribute = "ShaderProperty";

        private readonly StringBuilder _declarationsBuilder = new StringBuilder();
        private readonly StringBuilder _initializationBuilder = new StringBuilder();
        private readonly StringBuilder _classBuilder = new StringBuilder();

        public void Execute(GeneratorExecutionContext context)
        {
            //find classes with ShaderPropertiesProvider attribute
            var classes = context.Compilation.GetClassesByAttribute(ShaderPropertiesProviderAttribute);
            string namespaceStr = string.Empty;

            foreach (var classDeclaration in classes)
            {
                _classBuilder.Clear();
                _declarationsBuilder.Clear();
                _initializationBuilder.Clear();

                var shaderPropertyFields = classDeclaration.GetConstantsOfTypeByAttribute(ShaderPropertyAttribute, "string");
                var shaders = classDeclaration.GetConstantsOfTypeByAttribute(ShaderAttribute, "string");
                
                //get shader properties and create in propertyIds for them;
                foreach (var shaderPropertyField in shaderPropertyFields)
                {
                    var stringField = shaderPropertyField.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shaderPropertyField.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ConstToCamelCase() + "PropertyId";
                   
                    //add declaration and initialization for shaders
                    _declarationsBuilder.AppendLine($"public static int {intField} {{get; private set;}}");
                    _initializationBuilder.AppendLine($"{intField} = Shader.PropertyToID({stringFieldValue});");
                }
                
                //get shaders and create in shaderIds for them;
                var shaderCounter = 0;
                foreach (var shader in shaders)
                {
                    var stringField = shader.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shader.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ConstToCamelCase() + "ShaderId";
                    
                    //add declaration and initialization for shaders
                    _declarationsBuilder.AppendLine($"public static int {intField} {{get; private set;}}");
                    
                    var shaderVariable = stringField + shaderCounter++;
                    _initializationBuilder.AppendLine($"{intField} = Shader.PropertyToID({stringFieldValue});");
                    _initializationBuilder.AppendLine($"var {shaderVariable} = Shader.Find({stringFieldValue});");
                    _initializationBuilder.AppendLine($"Shaders[{intField}] = {shaderVariable};");
                    _initializationBuilder.AppendLine($"Materials[{intField}] = new Material({shaderVariable});");
                    _initializationBuilder.AppendLine("");
                }

                //TODO:
                //get class current namespace:
                namespaceStr = classDeclaration.GetNamespace();

                //CODE OF GENERATED CLASS                    
                _classBuilder.Append(
                    $@"
using UnityEngine;
using System.Collections.Generic;

namespace {namespaceStr}
{{
    public static partial class {classDeclaration.Identifier.Text} 
    {{ 
        {_declarationsBuilder} 

        private static readonly Dictionary<int, Shader> Shaders = new();
        private static readonly Dictionary<int, Material> Materials = new();
        
        static MaterialProvider()
        {{
            Init();
        }}

        private static void Init() 
        {{ 
            {_initializationBuilder} 
        }}

        public static Shader GetShader(int shader) => Shaders[shader];

        public static Material GetMaterial(int shader) => Materials[shader];
    }}
}}");
                //END OF CODE OF GENERATED CLASS
                context.AddSource(classDeclaration.Identifier.Text + "Generated.cs", SourceText.From(_classBuilder.ToString(), Encoding.UTF8));
            }

            StringBuilder codeGenResultTest = new StringBuilder();
            codeGenResultTest.AppendLine("namespace CodeGenTest{");
            codeGenResultTest.AppendLine("public static class CodeGenResultTest{");
            codeGenResultTest.AppendLine($"public static string Result = \"{namespaceStr}\"; }} ");
            codeGenResultTest.AppendLine("}");
            
            context.AddSource("CodeGenTest.cs", SourceText.From(codeGenResultTest.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}