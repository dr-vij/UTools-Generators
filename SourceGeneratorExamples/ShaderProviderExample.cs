using UTools.SourceGeneratorAttributes;

namespace TestNamespace
{
    [ShaderPropertiesProvider]
    public partial class ShaderProviderExample
    {
        [Shader] private const string Shader = "ShaderName";
        [ShaderProperty] private const string ShaderProperty = "ShaderPropertyName";
    }
}