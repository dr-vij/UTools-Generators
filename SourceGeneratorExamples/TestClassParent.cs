using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public class MyTestAttr : System.Attribute
    {
    }
    
    [MyTestAttr]
    public partial class CombinedSubscriptionsExample
    {
        [DisposableSubscription] [EventSubscription]
        private static bool m_TwoSubscriptionsField;
    }
}