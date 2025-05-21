using UnityEngine;

namespace DebuggingEssentials
{
    #pragma warning disable 0168 // variable declared but not used.
    #pragma warning disable 0219 // variable assigned but not used.
    #pragma warning disable 0414 // private field assigned but not used.

    [ConsoleAlias("test")]
    public class TestConsole : MonoBehaviour
    {
        enum TestMode { ModeA, ModeB, ModeC }

        void Awake()
        {
            RuntimeConsole.Register(this);
            doMath = Multiply;
        }

        void OnDestroy() { RuntimeConsole.Unregister(this); }

        // Field Examples ================================================================
        [ConsoleCommand("fields.")]
        float testValue1 = 10.23f;

        [ConsoleCommand("fields.", "This is testMode")]
        TestMode testMode = TestMode.ModeB;

        [ConsoleCommand("fields.testValue3Override", "This is testValue3")]
        float testValue3 = 15;

        [ConsoleCommand("fields.", "This is a test position")]
        Vector3 position;

        // Property Examples ============================================================
        [ConsoleCommand("props.AAA")]
        bool AutoProp1 { get; set; }

        int testProp2;
        [ConsoleCommand("props.", "This is testProp1")]
        public int TestProp2
        {
            get { return testProp2; }
            set { testProp2 = value; }
        }
        
        // Method Examples ==============================================================
        [ConsoleCommand("methods.")]
        void PrintHello() { Debug.Log("Hello!"); }

        [ConsoleCommand("methods.", "Print x amount of numbers")]
        int PrintNumbers(int count)
        {
            for (int i = 0; i < count; i++) RuntimeConsole.Log("number " + i, new Color((i / 255.0f) % 1, 1, 0), true);
            return count;
        }
        
        [ConsoleCommand("methods.")]
        float Multiply(float a, float b) { return a * b; }
        
        // Delegate Example ============================================================
        delegate float DoMath(float a, float b);

        [ConsoleCommand("delegate.")]
        DoMath doMath;
    }
}
