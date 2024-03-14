using UTools.SourceGeneratorAttributes;

namespace TestNamespace
{
    [ShaderPropertiesProvider]
    public static partial class ShaderProviderExample
    {
        [ShaderName] private const string ShaderName = "ShaderName";
        [ShaderProperty] private const string ShaderProperty = "ShaderPropertyName";
    }
}

#region Fake Unity mocks

namespace UnityEngine
{
    public class Shader
    {
        public static int PropertyToID(string name) => default;
    }

    public static class Resources
    {
        public static T Load<T>(string path) => default!;
    }

    public class Material
    {
        public Material(Shader shader)
        {
        }
    }
}

#endregion