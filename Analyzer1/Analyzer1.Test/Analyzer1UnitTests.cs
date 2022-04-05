using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Analyzer1.Test.CSharpCodeFixVerifier<
    Analyzer1.Analyzer1Analyzer,
    Analyzer1.Analyzer1CodeFixProvider>;

namespace Analyzer1.Test
{
    [TestClass]
    public class Analyzer1UnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;

namespace System.Text.Json.Serialization
{
    public sealed class JsonIgnoreAttribute : Attribute
    {
        public JsonIgnoreCondition Condition { get; set; }
    }

    public enum JsonIgnoreCondition
    {
        WhenWritingDefault
    }
}

namespace DSharpPlus.Core.Entities
{
    public struct Optional<T> { }
}

namespace ConsoleApplication1
{
    class Test
    {   
        public Optional<int> {|#0:Prop|} { get; set; }
    }
}";

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;

namespace System.Text.Json.Serialization
{
    public sealed class JsonIgnoreAttribute : Attribute
    {
        public JsonIgnoreCondition Condition { get; set; }
    }

    public enum JsonIgnoreCondition
    {
        WhenWritingDefault
    }
}

namespace DSharpPlus.Core.Entities
{
    public struct Optional<T> { }
}

namespace ConsoleApplication1
{
    class Test
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> Prop { get; set; }
    }
}";

            var expected = VerifyCS.Diagnostic(Analyzer1Analyzer.DiagnosticId).WithLocation(0).WithArguments("Prop");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
