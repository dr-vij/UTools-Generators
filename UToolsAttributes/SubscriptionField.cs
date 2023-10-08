using System;

namespace UTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SubscriptionField : Attribute
    {
        public string PropertyName { get; set; }
    }
}