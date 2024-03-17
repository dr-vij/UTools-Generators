# UTools-SourceGenerators Documentation
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

## DisposableSubscription

<note>
    <p>
        You should use the U-Tools library that contains the DisposeAction class to use this feature, or implement your IDisposable unsubscriber by yourself (DisposeAction).
    </p>
</note>

DisposableSubscription attribute example usage:
```C#
namespace ExampleNamespace.NameSpace
{
    public partial class DisposableSubscriptionExample
    {
        [DisposableSubscription] private static bool m_TestFieldDisposable;
    }
}
```

The Subscription method is IDisposable, disposing it will remove the subscription.
Generated code:
```C#
using UTools;
using System;

namespace ExampleNamespace.NameSpace
{
    public partial class DisposableSubscriptionExample
    {
        static partial void OnTestFieldDisposableChange(bool newValue);
        static event Action<bool> m_TestFieldDisposableChanged;
        public static bool TestFieldDisposable
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
                    m_TestFieldDisposableChanged?.Invoke(value) ;;
                    OnTestFieldDisposableChange(value);
                }
            }
        }

        public static IDisposable SubscribeToTestFieldDisposable(Action<bool> handler)
        {
            m_TestFieldDisposableChanged += handler;
            handler?.Invoke(m_TestFieldDisposable);
            return new DisposeAction(() => m_TestFieldDisposableChanged -= handler);
        }
    }
}
```

EventSubscription example usage:
```C#
namespace ExampleNamespace.NameSpace
{
    public partial class EventSubscriptionExample
    {
        [EventSubscription] private static bool m_TestFieldEvent;
    }
}
```

Generated code:
```C#
using UTools;
using System;

namespace ExampleNamespace.NameSpace
{
    public partial class EventSubscriptionExample
    {
        static partial void OnTestFieldEventChange(bool newValue);
        static event Action<bool> m_TestFieldEventChanged;
        public static bool TestFieldEvent
        {
            get
            {
                return m_TestFieldEvent;
            }

            set
            {
                if (m_TestFieldEvent != value)
                {
                    m_TestFieldEvent = value;
                    m_TestFieldEventChanged?.Invoke(value) ;;
                    OnTestFieldEventChange(value);
                }
            }
        }

        public static event Action<bool> TestFieldEventChanged
        {
            add
            {
                value?.Invoke(m_TestFieldEvent) ;;
                m_TestFieldEventChanged += value;
            }

            remove
            {
                m_TestFieldEventChanged -= value;
            }
        }
    }
}
```



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
