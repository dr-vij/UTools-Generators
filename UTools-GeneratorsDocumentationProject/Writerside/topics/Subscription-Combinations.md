# Subscription Combinations

The plugin allows you to combine subscription attributes. both implementations will be created in such case.

Example Usage:
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
