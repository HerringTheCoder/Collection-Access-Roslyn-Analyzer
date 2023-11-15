using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

using Xunit;

namespace Tests
{
    using VerifyCS = CSharpAnalyzerVerifier<UpdatingComputedCollectionEntryDoesNotModifyDataAnalyzer, XUnitVerifier>;

    public class UpdatingComputedCollectionEntryDoesNotModifyDataTests
    {
        [Fact]
        public Task TestIfWithBlock()
        {
            var test =
                @"
using System.Collections.Generic;
using System;
namespace Test{
class C
{
    public IDictionary<string, object?> Params => new Dictionary<string, object?>
    {
        { ""0"", Guid.Empty }
    };
    void M()
    {
        Params[""0""] = ""Hello World"";
    }
}
}";
            
            return VerifyCS.VerifyAnalyzerAsync(test, VerifyCS.Diagnostic("RD001").WithSpan(13, 9, 13, 36).WithSeverity(DiagnosticSeverity.Warning));
        }

        [Fact]
        public async Task DiagnosticWhenAssigningToExpressionBodiedDictionary()
        {
            var test = @"
using System;
using System.Collections.Generic;

public class MyClass
{
    public IEnumerable Params => new Dictionary<string, object?>();

    public void ModifyDictionary()
    {
        Params[""0""] = ""Hello World"";
    }
}";
            
            await VerifyCS.VerifyAnalyzerAsync(test, VerifyCS.Diagnostic("RD001").WithSpan(11, 9, 11, 36).WithSeverity(DiagnosticSeverity.Warning));
        }
    }
}
