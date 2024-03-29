using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial interface IEventInterface
    {
    }

    public partial class EventSubscriptionExample
    {
        [EventSubscription(OutputInterface = typeof(IEventInterface))]
        private bool m_TestFieldEvent;
    }
}