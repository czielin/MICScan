using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseSardClassic.Datasets
{
    public class MethodCallsDataset : Dataset
    {
        protected bool includeSource;

        public MethodCallsDataset(bool includeSource)
        {
            this.includeSource = includeSource;
        }

        protected override string FileName
        {
            get
            {
                return includeSource ? "MethodCallsWithSource" : "MethodCallsWithoutSource";
            }
        }

        public override void AddExample(string className, bool isFlawed, List<string> fileContents)
        {
            StringBuilder features;
            SyntaxTree syntaxTree;
            if (includeSource)
            {
                string withoutComments = RemoveComments(fileContents);
                features = new StringBuilder(withoutComments);
                features.AppendLine();
                syntaxTree = CSharpSyntaxTree.ParseText(withoutComments);
            }
            else
            {
                string combinedContents = CombineContents(fileContents);
                features = new StringBuilder();
                syntaxTree = CSharpSyntaxTree.ParseText(combinedContents);
            }

            SyntaxNode root = syntaxTree.GetRoot();

            CSharpCompilation compilation = CreateCompilation(syntaxTree);
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                foreach (Diagnostic diagnostic in semanticModel.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine(diagnostic.GetMessage());
                    Console.WriteLine($"{diagnostic.Location.GetLineSpan().Path} {diagnostic.Location.GetLineSpan().StartLinePosition} {diagnostic.Location.GetLineSpan().EndLinePosition}");
                }
                Console.WriteLine(root);
                Console.WriteLine("Included assemblies: ");
                foreach (var assembly in compilation.References)
                {
                    Console.WriteLine(assembly.Display);
                }
                Console.ReadLine();
            }

            foreach (InvocationExpressionSyntax expression in root.DescendantNodes().Where(n => n.IsKind(SyntaxKind.InvocationExpression)))
            {
                string signature = BuildMethodSignature(expression, semanticModel);
                features.AppendLine(signature);
            }

            PrintDescendentNodes(root, semanticModel);
            Console.WriteLine(features.ToString());
            Console.ReadLine();

            dataset.Add(new Example { ClassName = className, IsFlawed = isFlawed, Features = features.ToString() });
        }

        private string BuildMethodSignature(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            string signature = BuildMethodName(invocationExpressionSyntax, semanticModel);
            return signature;
        }

        private ITypeSymbol GetArgumentType(CSharpSyntaxNode argument, SemanticModel semanticModel)
        {
            ITypeSymbol type;

            IMethodSymbol methodSymbol = semanticModel.GetSymbolInfo(argument).Symbol as IMethodSymbol;
            type = methodSymbol?.ReturnType;
            if (type == null)
            {
                type = semanticModel.GetTypeInfo(argument).Type;
            }
            return type;
        }

        protected virtual string BuildMethodName(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            IMethodSymbol methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            return methodSymbol.ToDisplayString().Replace(" ", "");
        }

        private string BuildExpressionName(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            foreach (IdentifierNameSyntax identifierNameSyntax in syntaxNode.ChildNodes().OfType<IdentifierNameSyntax>())
            {
                Console.WriteLine($"Identifier type: {identifierNameSyntax} {semanticModel.GetSymbolInfo(identifierNameSyntax).CandidateSymbols.FirstOrDefault()}");
            }
            return string.Join(".", syntaxNode.ChildNodes().OfType<IdentifierNameSyntax>());
        }
    }
}
