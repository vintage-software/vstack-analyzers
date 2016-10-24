using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Vstack.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VS1300NamespaceMustMatchFileLocation : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VS1300";

        private const string Title = "Namespace must match file location.";
        private const string MessageFormat = "Namespace must match file location. Expected '{0}'.";
        private const string Description = "The namespaces must match file location.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ValidateNotNullParameter(nameof(context));

            context.RegisterSyntaxTreeAction(AnyalyzeSyntaxTree);
        }

        private static void AnyalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetRoot();

            NamespaceDeclarationSyntax namespaceDeclaration = root
                .DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (namespaceDeclaration != null)
            {
                string actualNamespace = namespaceDeclaration.Name.ToString();
                string expectedNamespace = GetExpectedNamepace(context.Tree);

                if (actualNamespace != expectedNamespace)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, namespaceDeclaration.Name.GetLocation(), expectedNamespace));
                }
            }
        }

        private static string GetExpectedNamepace(SyntaxTree tree)
        {
            string projectPath = Path.GetDirectoryName(tree.FilePath);
            while (Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly).Length == 0)
            {
                projectPath = Directory.GetParent(projectPath).ToString();
            }

            string projectParentPath = Directory.GetParent(projectPath).ToString();
            string documentRelativePath = Path.GetDirectoryName(tree.FilePath).Replace(projectParentPath, string.Empty).Substring(1);
            string expectedNamespace = documentRelativePath.Replace(Path.DirectorySeparatorChar, '.');

            return expectedNamespace;
        }
    }
}
