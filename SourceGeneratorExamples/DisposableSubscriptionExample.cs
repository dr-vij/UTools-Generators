using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public interface ITestInterface
    {
    }
    
    public class TestClass 
    {
    }
    
    public partial class DisposableSubscriptionExample
    {
        [DisposableSubscription (OutputInterface = typeof(ITestInterface))] 
        private static bool m_TestFieldDisposable;
    }
}