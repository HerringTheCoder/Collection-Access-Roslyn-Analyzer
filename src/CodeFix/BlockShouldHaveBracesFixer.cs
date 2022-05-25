using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Analyzer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlockShouldHaveBracesFixer)), Shared]
    public class BlockShouldHaveBracesFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(BlockShouldHaveBracesAnalyzer.DiagnosticId);

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md
        // for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var cancellationToken = context.CancellationToken;
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the block that the diagnostic is reported on
            var block = root?.FindNode(diagnosticSpan);
            if (block is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add curly braces",
                    c => AddBraces(context.Document, block, c),
                    equivalenceKey: "Add curly braces"),
                diagnostic);
        }

        private static async Task<Document> AddBraces(Document document, SyntaxNode block, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var editor = new SyntaxEditor(root!, document.Project.Solution.Workspace);

            var embeddedStatement = BlockShouldHaveBracesAnalyzer.GetEmbeddedStatement(block);

            if (embeddedStatement is null)
            {
                throw new InvalidOperationException();
            }

            var updatedBlock = block.ReplaceNode(embeddedStatement, SyntaxFactory.Block(embeddedStatement));
            editor.ReplaceNode(block, updatedBlock);

            var newRoot = editor.GetChangedRoot();
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
