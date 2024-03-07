using System;

namespace UTools.Attributes
{
    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a field to make IDisposable subscription for it
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Field)]
    public class DisposableSubscription : Attribute
    {
        public bool InitializeOnStart { get; set; }
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to generate automatic subscriptions
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SubscriptionField : Attribute
    {
        public string PropertyName { get; set; } = string.Empty;
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to partial static class to make it int properties provider
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ShaderPropertiesProviderAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a string constant with shader name to make a shader property for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a string constant with a shader property name to make a shader property for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyAttribute : Attribute
    {
    }
}