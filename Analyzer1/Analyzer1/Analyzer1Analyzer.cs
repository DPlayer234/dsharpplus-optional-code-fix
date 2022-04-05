using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer1Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JsonIgnoreOptional";

        private const string OptionalTName = "Optional";
        private const string JsonIgnoreAttrName = "JsonIgnoreAttribute";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly string Title = "Optional<T> requires JsonIgnore";
        private static readonly LocalizableString MessageFormat = "Property {0} requires a JsonIgnore attribute";
        private static readonly LocalizableString Description = "Any public static property of type Optional<T> requires a JsonIgnore attribute.";
        private const string Category = "Custom";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // Only apply if public
            if (context.Symbol.DeclaredAccessibility != Accessibility.Public || context.Symbol.IsStatic)
                return;

            var symbol = (IPropertySymbol)context.Symbol;
            var type = symbol.Type.OriginalDefinition;
            if (type.Name != OptionalTName)
                return;

            var attributes = symbol.GetAttributes();
            if (!attributes.Any(a => a.AttributeClass.Name == JsonIgnoreAttrName))
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
