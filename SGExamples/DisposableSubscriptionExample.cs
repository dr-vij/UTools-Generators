using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial interface IInterface
    {
    }
    
    public partial class DisposableSubscriptionExample
    {
        [DisposableSubscription(Interface = typeof(IInterface))]
        private static bool m_TestFieldDisposable;
    }
}