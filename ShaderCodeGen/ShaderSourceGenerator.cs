using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CanvasTextureSourceGenerators
{
    [Generator]
    public class ShaderSourceGenerator : ISourceGenerator
    {
        const string TripleTab = "\t\t\t";
        const string DoubleTab = "\t\t";

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

            foreach (var classDeclaration in classes)
            {
                _classBuilder.Clear();
                _declarationsBuilder.Clear();
                _initializationBuilder.Clear();

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
                    _declarationsBuilder.AppendLine($"{DoubleTab}public static int {intField} {{get; private set;}}");

                    _initializationBuilder.AppendLine($"{TripleTab}{intField} = Shader.PropertyToID({stringFieldValue});");
                }

                _declarationsBuilder.AppendLine("");
                _initializationBuilder.AppendLine("");

                //get shaders and create in shaderIds for them;
                foreach (var shader in shaders)
                {
                    var stringField = shader.Declaration.Variables.First().Identifier.Text;
                    var stringFieldValue = shader.Declaration.Variables.First().Identifier.Value;
                    var intField = stringField.ConstToCamelCase() + "ShaderId";

                    //add declaration and initialization for shaders
                    _declarationsBuilder.AppendLine($"{DoubleTab}public static int {intField} {{get; private set;}}");
                    _initializationBuilder.Append(
                        $@"
            {intField} = Shader.PropertyToID({stringFieldValue});
            Shaders[{intField}] = Resources.Load<Shader>({stringFieldValue});
            Materials[{intField}] = new Material(Shaders[{intField}]);
                        ");
                }

                //CODE OF GENERATED CLASS                    
                _classBuilder.Append(
                    $@"
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace {namespaceStr}
{{
    public static partial class {classDeclaration.Identifier.Text} 
    {{ 
{_declarationsBuilder} 

        private static readonly Dictionary<int, Shader> Shaders = new Dictionary<int, Shader>();
        private static readonly Dictionary<int, Material> Materials = new Dictionary<int, Material>();
        
        static {classDeclaration.Identifier.Text}()
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

            // test purpose only to check if codegen works
            // StringBuilder codeGenResultTest = new StringBuilder();
            // var test = "";
            // codeGenResultTest.AppendLine("namespace CodeGenTest{");
            // codeGenResultTest.AppendLine("public static class CodeGenResultTest{");
            // codeGenResultTest.AppendLine($"public static string Result = \"{test}\"; }} ");
            // codeGenResultTest.AppendLine("}");
            //
            // context.AddSource("CodeGenTest.cs", SourceText.From(codeGenResultTest.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Dont need it for now
        }
    }
}