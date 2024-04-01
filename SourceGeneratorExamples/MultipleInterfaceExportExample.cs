using UTools.SourceGeneratorAttributes;

namespace ExampleNamespace.NameSpace
{
    public partial interface ITestInterface
    {
    }

    public partial interface ITestInterface2
    {
    }

    public partial interface ITestInterface3
    {
    }

    public partial class MultipleInterfaceExportExample
    {
        [DisposableSubscription(OutputInterfaces = new[] { typeof(ITestInterface) })]
        private bool m_TestFieldDisposable;

        [DisposableSubscription(typeof(ITestInterface), typeof(ITestInterface2))]
        private bool m_TestField2;

        [DisposableSubscription(typeof(ITestInterface3))]
        private bool m_TestField3;
    }
}