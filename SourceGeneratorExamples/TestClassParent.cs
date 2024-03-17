using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial class CombinedSubscriptionsExample
    {
        [DisposableSubscription] [EventSubscription]
        private static bool m_TwoSubscriptionsField;
    }
}