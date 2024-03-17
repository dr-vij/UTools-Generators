# EventSubscription

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
