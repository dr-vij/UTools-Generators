# UTools-SourceGenerators
## Subscriptions

This plugin is a Roslyn Source Generators tool that can be used for the automation of some boilerplate routines. 
Add the DisposableSubscription or EventSubscription attribute to a field to automatically generate the subscription code and property for it.

It will create static subscriptions/properties/events in case you use static.

## Installation

- Add UToolsGenerators.dll and UToolsAttributes.dll to your UNITY project.
- Create an assembly for them.
- Mark UToolsGenerators.dll as a Roslyn Analyzer (add the RoslynAnalyzer tag on it; see [UNITY documentation](https://docs.unity3d.com/2023.3/Documentation/Manual/roslyn-analyzers.html) for more details).
- Make a link to the assembly in your project. The linked assembly will receive Source Generator benefits.

<note>
    <p>
        Your class must be partial to use this feature. Global namespaces are not supported yet.
    </p>
</note>


Combined Subscriptions (DisposableSubscription + EventSubscription) example usage:
```C#
namespace ExampleNamespace.NameSpace
{
    public partial class CombinedSubscriptionsExample
    {
        [DisposableSubscription] [EventSubscription]
        private static bool m_TwoSubscriptionsField;
    }
}
```

Generated code:
```C#
using UTools;
using System;

namespace ExampleNamespace.NameSpace
{
    public partial class CombinedSubscriptionsExample
    {
        static partial void OnTwoSubscriptionsFieldChange(bool newValue);
        static event Action<bool> m_TwoSubscriptionsFieldChanged;
        public static bool TwoSubscriptionsField
        {
            get
            {
                return m_TwoSubscriptionsField;
            }

            set
            {
                if (m_TwoSubscriptionsField != value)
                {
                    m_TwoSubscriptionsField = value;
                    m_TwoSubscriptionsFieldChanged?.Invoke(value) ;;
                    OnTwoSubscriptionsFieldChange(value);
                }
            }
        }

        public static IDisposable SubscribeToTwoSubscriptionsField(Action<bool> handler)
        {
            m_TwoSubscriptionsFieldChanged += handler;
            handler?.Invoke(m_TwoSubscriptionsField);
            return new DisposeAction(() => m_TwoSubscriptionsFieldChanged -= handler);
        }

        public static event Action<bool> TwoSubscriptionsFieldChanged
        {
            add
            {
                value?.Invoke(m_TwoSubscriptionsField) ;;
                m_TwoSubscriptionsFieldChanged += value;
            }

            remove
            {
                m_TwoSubscriptionsFieldChanged -= value;
            }
        }
    }
}
```
