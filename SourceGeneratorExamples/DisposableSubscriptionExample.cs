using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial interface ITestInterface
    {
    }
    
    public class TestClass 
    {
    }
    
    public partial class DisposableSubscriptionExample
    {
        [DisposableSubscription (OutputInterface = typeof(TestClass))] 
        private static bool m_TestFieldDisposable;
    }
}