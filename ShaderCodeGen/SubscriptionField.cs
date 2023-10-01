using System;

namespace UTools.SourceGeneratorAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SubscriptionField : Attribute
    {
        public string PropertyName { get; set; }
    }
}