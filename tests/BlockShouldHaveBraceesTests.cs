using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzer;
using CodeFix;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

using Xunit;

namespace Tests
{
    using VerifyCS = CSharpCodeFixVerifier<
        BlockShouldHaveBracesAnalyzer,
        BlockShouldHaveBracesFixer,
        XUnitVerifier>;

    public class BlockShouldHaveBraceesTests
    {
        [Fact]
        public Task TestIfStatement()
        {
            return VerifyCS.VerifyCodeFixAsync(
@"class C
{
    void M()
    {
        [|if (true)
            return;|]
    }
}",
@"class C
{
    void M()
    {
        if (true)
        {
            return;
        }
    }
}");
        }

        [Fact]
        public Task TestIfWithBlock()
        {
            return VerifyCS.VerifyAnalyzerAsync(
@"class C
{
    void M()
    {
        if (true)
        {
            return;
        }
    }
}");

        }
    }
}
