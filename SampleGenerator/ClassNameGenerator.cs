using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace SampleGenerator;

[Generator]
public class ClassNameGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (c, _) => c is ClassDeclarationSyntax,
                transform: (n, ct) =>
                    n.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)n.Node, ct)?.ToDisplayString())
            .Where(m => m is not null);
        context.RegisterSourceOutput(provider.Collect(), Execute);
    }

    private void Execute(SourceProductionContext context, ImmutableArray<string> classDisplayStrings)
    {
        if (classDisplayStrings.Length == 0)
        {
            var description = new DiagnosticDescriptor(
                "SG001",
                "No classes found",
                "No classes found in current context",
                "Problem",
                DiagnosticSeverity.Warning,
                true);
            context.ReportDiagnostic(Diagnostic.Create(description, Location.None));
        }

        var builder = new StringBuilder();
        foreach (string displayString in classDisplayStrings)
        {
            builder.Append($"\"{displayString}\",");
        }

        if (builder.Length > 0) builder.Length--;
        var code = $$"""
                     using System.Diagnostics;
                     
                     namespace SampleCodeGenerator
                     {
                        public class ClassNames
                        {
                            public static List<string> GetNames()
                            {
                                Debugger.Break();
                                return new List<string>{ 
                                    {{builder}}
                                };
                            }
                        }
                     }
                     """;
        context.AddSource("ClassNames.g.cs", code);

    }
}