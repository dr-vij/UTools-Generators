using System;

public partial class MyClass
{
    [Subscription] private int m_Counter;
}

// public partial class MyClass
// {
//     public event Action<int> OnCounterChanged;
//
//     public int Counter
//     {
//         get => m_Counter;
//         set
//         {
//             if (m_Counter != value)
//             {
//                 m_Counter = value;
//                 OnCounterChanged?.Invoke(m_Counter);
//             }
//         }
//     }
// }