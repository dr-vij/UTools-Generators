# Interface output

You can use OutputInterface parameter to export generated subscriptions and properties to the interface.
To do this:
- Create your partial interface
- Use it as OutputInterface parameter for the attribute.

Example usage:

```C#

```

Example of generated code:

```C#
using UTools.SourceGeneratorAttributes;
using UTools;
using System;

namespace ExampleNamespace.NameSpace
{
    public partial interface ITestInterface
    {
        public bool TestFieldDisposable { get; set; }

        public IDisposable SubscribeToTestFieldDisposable(EventHandler<bool> handler);
    }
}

namespace ExampleNamespace.NameSpace
{
    public partial class DisposableSubscriptionExample : ITestInterface
    {
        partial void OnTestFieldDisposableChange(bool newValue);
        event EventHandler<bool> m_TestFieldDisposableChanged;
        public bool TestFieldDisposable
        {
            get
            {
                return m_TestFieldDisposable;
            }

            set
            {
                if (m_TestFieldDisposable != value)
                {
                    m_TestFieldDisposable = value;
                    m_TestFieldDisposableChanged?.Invoke(this, value);
                    OnTestFieldDisposableChange(value);
                }
            }
        }

        public IDisposable SubscribeToTestFieldDisposable(EventHandler<bool> handler)
        {
            m_TestFieldDisposableChanged += handler;
            handler?.Invoke(this, m_TestFieldDisposable);
            return new DisposeAction(() => m_TestFieldDisposableChanged -= handler);
        }
    }
}

```