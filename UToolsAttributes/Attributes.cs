using System;

namespace UTools.SourceGeneratorAttributes
{
    #region Subscription Source Generator attributes

    public interface IAttributeInterface
    {
        public Type? Interface { get; set; }
    }

    /// <summary>
    /// TODO: implement optional parameters and property name
    /// This attribute is used for SourceGenerator. Add it to a field to make IDisposable subscription for it
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Field)]
    public class DisposableSubscription : Attribute, IAttributeInterface
    {
        public bool InitializeOnStart { get; set; } = false;
        public string PropertyName { get; set; } = string.Empty;
        public Type? Interface { get; set; }
    }

    /// <summary>
    /// TODO: implement optional parameters and property name
    /// This attribute is used for SourceGenerator. Add it to generate automatic subscriptions
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EventSubscription : Attribute
    {
        public bool InitializeOnStart { get; set; } = false;
        public string PropertyName { get; set; } = string.Empty;
        public Type? Interface { get; set; }
    }

    #endregion

    #region ShaderProvider Source Generator attributes

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to partial static class to make it int properties provider
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ShaderPropertiesProvider : Attribute
    {
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a string constant with shader name to make a shader property for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderName : Attribute
    {
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a string constant with a shader property name to make a shader property for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderProperty : Attribute
    {
    }

    #endregion
}