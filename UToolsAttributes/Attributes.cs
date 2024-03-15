using System;

namespace UTools.SourceGeneratorAttributes
{
    #region Subscription Source Generator attributes

    public interface IOutputInterfaceAttrParameter
    {
        public Type? OutputInterface { get; set; }
    }

    public interface IPropertyNameAttrParameter
    {
        public string PropertyName { get; set; }
    }

    public interface IInitializeOnStartAttrParameter
    {
        public bool InitializeOnStart { get; set; }
    }

    /// <summary>
    /// TODO: implement optional parameters and property name
    /// This attribute is used for SourceGenerator. Add it to a field to make IDisposable subscription for it
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Field)]
    public class DisposableSubscriptionAttribute : Attribute, IOutputInterfaceAttrParameter, IPropertyNameAttrParameter, IInitializeOnStartAttrParameter
    {
        public bool InitializeOnStart { get; set; } = false;
        public string PropertyName { get; set; } = string.Empty;
        public Type? OutputInterface { get; set; } = null;
    }

    /// <summary>
    /// TODO: implement optional parameters and property name
    /// This attribute is used for SourceGenerator. Add it to generate automatic subscriptions
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EventSubscriptionAttribute : Attribute, IOutputInterfaceAttrParameter, IPropertyNameAttrParameter, IInitializeOnStartAttrParameter
    {
        public bool InitializeOnStart { get; set; } = false;
        public string PropertyName { get; set; } = string.Empty;
        public Type? OutputInterface { get; set; } = null;
    }

    #endregion

    #region ShaderProvider Source Generator attributes

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
    public class ShaderNameAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used for SourceGenerator. Add it to a string constant with a shader property name to make a shader property for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyAttribute : Attribute
    {
    }

    #endregion
}