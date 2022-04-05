using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Analyzer1
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Analyzer1CodeFixProvider)), Shared]
    public class Analyzer1CodeFixProvider : CodeFixProvider
    {
        private const string CodeFixTitle = "Add JsonIgnore";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Analyzer1Analyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixTitle,
                    createChangedDocument: c => AddJsonIgnoreAttributeAsync(context.Document, declaration, c),
                    equivalenceKey: CodeFixTitle),
                diagnostic);
        }

        private static NameSyntax ToQualifiedName(params string[] names)
        {
            NameSyntax name = IdentifierName(names[0]);
            for (int i = 1; i < names.Length; i++)
            {
                name = QualifiedName(name, IdentifierName(names[i]));
            }

            return name;
        }

        private static ExpressionSyntax ToMemberAccess(params string[] names)
        {
            ExpressionSyntax expr = IdentifierName(names[0]);
            for (int i = 1; i < names.Length; i++)
            {
                expr = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, IdentifierName(names[i]));
            }

            return expr;
        }

        private async Task<Document> AddJsonIgnoreAttributeAsync(Document document, PropertyDeclarationSyntax propDecl, CancellationToken cancellationToken)
        {
            // Black Magic

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var attrArgs = SingletonSeparatedList(
                AttributeArgument(
                    ToMemberAccess("JsonIgnoreCondition", "WhenWritingDefault"))
                .WithNameEquals(
                    NameEquals(
                        IdentifierName("Condition"))));

            var attributes = propDecl.AttributeLists.Add(
                AttributeList(SingletonSeparatedList(
                    Attribute(
                        ToQualifiedName("JsonIgnore"),
                        AttributeArgumentList(attrArgs))
                )));

            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    propDecl,
                    propDecl.WithAttributeLists(attributes)
                ));
        }
    }
}
