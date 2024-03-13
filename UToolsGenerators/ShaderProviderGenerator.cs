using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using UTools.SourceGeneratorAttributes;

namespace UTools.SourceGenerators
{
    [Generator]
    public class ShaderProviderGenerator : ISourceGenerator
    {
        private const string ShaderAttribute = nameof(Shader);
        private const string ShaderPropertiesProviderAttribute = nameof(ShaderPropertiesProvider);
        private const string ShaderPropertyAttribute = nameof(ShaderProperty);

        private readonly StringBuilder m_DeclarationsBuilder = new();
        private readonly StringBuilder m_InitializationBuilder = new();
        private readonly StringBuilder m_ClassBuilder = new();

        public void Execute(GeneratorExecutionContext context)
        {
            //find classes with ShaderPropertiesProvider attribute
            var classes = context.Compilation.GetClassesByAttributes(ShaderPropertiesProviderAttribute);

            foreach (var classDeclaration in classes)
            {
                m_ClassBuilder.Clear();
                m_DeclarationsBuilder.Clear();
                m_InitializationBuilder.Clear();

                var shaderPropertyFields = classDeclaration.GetConstantsOfTypeByAttribute(ShaderPropertyAttribute, "string");
                var shaders = classDeclaration.GetConstantsOfTypeByAttribute(ShaderAttribute, "string");
                var namespaceStr = classDeclaration.GetNamespace();

                //get shader properties and create in propertyIds for them;
                foreach (var shaderPropertyField in shaderPropertyFields)
                {
                    var stringField = shaderPropertyField.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shaderPropertyField.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ConstToCamelCase() + "PropertyId";

                    //add declaration and initialization for shaders
                    m_DeclarationsBuilder.AppendLine($"public static int {intField} {{get; private set;}}");

                    m_InitializationBuilder.AppendLine($"{TripleTab}{intField} = Shader.PropertyToID({stringFieldValue});");
                }

                m_DeclarationsBuilder.AppendLine("");
                m_InitializationBuilder.AppendLine("");

                //get shaders and create in shaderIds for them;
                foreach (var shader in shaders)
                {
                    var stringField = shader.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shader.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ConstToCamelCase() + "ShaderId";

                    //add declaration and initialization for shaders
                    m_DeclarationsBuilder.AppendLine($"{DoubleTab}public static int {intField} {{get; private set;}}");
                    m_InitializationBuilder.Append(
                        $@"
            {intField} = Shader.PropertyToID({stringFieldValue});
            Shaders[{intField}] = Resources.Load<Shader>({stringFieldValue});
            Materials[{intField}] = new Material(Shaders[{intField}]);
                        ");
                }

                //CODE OF GENERATED CLASS                    
                m_ClassBuilder.Append(
                    $@"
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace {namespaceStr}
{{
    public static partial class {classDeclaration.Identifier.Text} 
    {{ 
{m_DeclarationsBuilder} 

        private static readonly Dictionary<int, Shader> Shaders = new Dictionary<int, Shader>();
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();
        
        static {classDeclaration.Identifier.Text}()
        {{
{m_InitializationBuilder} 
        }}

        public static Shader GetShader(int shader) => Shaders[shader];

        public static Material GetMaterial(int shader) => Materials[shader];
    }}
}}");
                //END OF CODE OF GENERATED CLASS
                context.AddSource(classDeclaration.Identifier.Text + "Generated.cs", SourceText.From(m_ClassBuilder.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Dont need it for now
        }
    }
}