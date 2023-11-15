using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UpdatingComputedCollectionEntryDoesNotModifyDataAnalyzer : DiagnosticAnalyzer
    {
        private const string AssignmentToDictionaryEntry = "RD001";

        private static readonly DiagnosticDescriptor AssignmentToCollectionRule = new DiagnosticDescriptor(
            AssignmentToDictionaryEntry,
            "Do not assign values to collection defined using expression-bodied properties",
            "Do not assign values to collection defined using expression-bodied properties. Consider using a regular property declaration.",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(AssignmentToCollectionRule);
       

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }
        
        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot();
            var assignmentExpressions = root.DescendantNodes().OfType<AssignmentExpressionSyntax>();
            
            var expressionBodiedProperties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .Where(p => p.ExpressionBody != null &&
                            p.Identifier.ValueText == "Params");

            if (expressionBodiedProperties.Any())
            {
                var assignedToParams = assignmentExpressions
                    .Where(assignment => assignment.Left is ElementAccessExpressionSyntax { Expression: IdentifierNameSyntax });
                
                foreach (var assignment in assignedToParams)
                {
                    var diagnostic = Diagnostic.Create(AssignmentToCollectionRule, assignment.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}