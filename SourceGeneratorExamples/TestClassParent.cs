using UTools.Attributes;
using System;

namespace ExampleNamespace.NameSpace
{
    public partial class TestClassParent
    {
        public partial class TestClass
        {
            [DisposableSubscription] private static bool m_TestFieldDisposable;
        }

        public partial class TestClassOther
        {
            [DisposableSubscription] [EventSubscription]
            private static bool m_TwoSubscriptionsField;
        }
    }

    public partial class TestClassGrandParent
    {
        public partial class TestClassParent
        {
            public partial class TestClass
            {
                [EventSubscription] private static bool m_TestFieldEvent;
            }
        }
    }
}