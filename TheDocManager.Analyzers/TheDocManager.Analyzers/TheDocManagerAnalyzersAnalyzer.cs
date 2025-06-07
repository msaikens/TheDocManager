using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TheDocManager.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CustomWarningsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly (string id, string title, string attributeName)[] CustomWarnings =
        {
            ("TDM001", "Avoid Use If Possible", "AvoidUseIfPossibleAttribute"),
            ("TDM002", "Soon To Be Deprecated", "SoonToBeDeprecatedAttribute"),
            ("TDM003", "Do Not Use Without Admin Approval", "DoNotUseWithoutAdminApprovalAttribute"),
            ("TDM004", "Unsafe Code", "UnsafeCodeAttribute"),
            ("TDM005", "Vulnerabilities Identified", "VulnerabilitiesIdentifiedAttribute"),
            ("TDM006", "Problematic", "ProblematicAttribute")
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            CustomWarnings.Select(warning => new DiagnosticDescriptor(
                warning.id, warning.title, $"'{warning.title}' - flagged by attribute.",
                "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true)).ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;
            var attributes = symbol.GetAttributes();

            foreach (var (id, title, attributeName) in CustomWarnings)
            {
                if (attributes.Any(attr => attr.AttributeClass?.Name == attributeName))
                {
                    var descriptor = SupportedDiagnostics.First(d => d.Id == id);
                    var diagnostic = Diagnostic.Create(descriptor, symbol.Locations[0]);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
