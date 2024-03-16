using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial interface ITestInterface
    {
    }

    public partial class DisposableSubscriptionExample
    {
        [DisposableSubscription(OutputInterface = typeof(ITestInterface))]
        private bool m_TestFieldDisposable;

        [DisposableSubscription(OutputInterface = typeof(ITestInterface))]
        private bool m_TestField2;
    }
}