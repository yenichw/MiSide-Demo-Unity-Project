// based on the original game.Yen Chezky(yenichw)
using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Burst.Compiler.IL.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WindowsOnlyAttribute : Attribute
    {
        public WindowsOnlyAttribute(string reason)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class TestCompilerAttribute : TestCaseAttribute, ITestBuilder
    {
    }
}
