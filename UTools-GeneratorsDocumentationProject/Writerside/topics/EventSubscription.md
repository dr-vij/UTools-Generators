# EventSubscription

The **EventSubscription** attribute is used to create an event for a field in a class. 
This event is triggered when the field's value changes. 
This allows for easy management of event notifications. 

The **EventSubscription** attribute also generates a property for the field it's applied to. 
This property's getter returns the current value of the field, 
and its setter updates the field's value and triggers the event if the new value is different from the old one.

**EventSubscription** attribute example usage:
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
