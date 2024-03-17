# DisposableSubscription

<note>
    <p>
        You have to use my U-Tools library that contains the DisposeAction class to use this feature.
    </p>
    <p>
        You can find it here: 
        <a href="https://github.com/dr-vij/UTools" >UTools</a>
    </p>
</note>

The **DisposableSubscription** attribute is used to create a subscription method for a field in a class. 
This method returns an IDisposable which, when disposed, removes the subscription. 
This allows for easy management of event subscriptions. 

The **DisposableSubscription** attribute also generates a property for the field it's applied to. 
This property's getter returns the current value of the field, and its setter updates the field's value and triggers the event if the new value is different from the old one.

**DisposableSubscription** attribute example usage:
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