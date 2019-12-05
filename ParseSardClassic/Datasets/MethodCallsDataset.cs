using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseSardClassic.Datasets
{
    public class MethodCallsDataset : Dataset
    {
        ScriptOptions scriptOptions = ScriptOptions.Default.WithImports("System", "System.Math");
        HashSet<string> unevaluatedIfs = new HashSet<string>();

        protected override string FileName
        {
            get
            {
                return nameof(MethodCallsDataset);
            }
        }

        public override async Task<(Example withSource, Example withoutSource)> AddExample(string className, bool isFlawed, List<string> fileContents)
        {
            StringBuilder features;
            SyntaxTree syntaxTree;
            string withoutComments = null;

            withoutComments = RemoveComments(fileContents);
            features = new StringBuilder();
            features.AppendLine();
            syntaxTree = CSharpSyntaxTree.ParseText(withoutComments);

            SyntaxNode root = syntaxTree.GetRoot();

            CSharpCompilation compilation = CreateCompilation(syntaxTree);
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

#if DEBUG
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
#endif

            foreach (SyntaxNode syntaxNode in root.DescendantNodes())
            {
                if
                (
                    syntaxNode is ExpressionSyntax expressionSyntax
                    &&
                    (
                        syntaxNode.IsKind(SyntaxKind.InvocationExpression)
                        || syntaxNode.IsKind(SyntaxKind.ObjectCreationExpression)
                    )
                    && await IsReachable(syntaxNode)
                )
                {
                    string signature = await BuildMethodSignature(expressionSyntax, semanticModel);
                    features.AppendLine(signature);
                }

                if (syntaxNode is AssignmentExpressionSyntax assignmentExpressionSyntax && await IsReachable(assignmentExpressionSyntax))
                {
                    ISymbol leftSymbol = semanticModel.GetSymbolInfo(assignmentExpressionSyntax.Left).Symbol;
                    if (leftSymbol is IPropertySymbol)
                    {
                        string signature = await BuildMethodSignature(assignmentExpressionSyntax.Left, semanticModel);
                        features.AppendLine(signature);
                    }
                }

                if (syntaxNode.IsKind(SyntaxKind.StringLiteralExpression) && syntaxNode.ToString().Contains("^[0-9]*") && await IsReachable(syntaxNode))
                {
                    features.AppendLine("HasNumbersOnlyRegex");
                    break;
                }
            }

            return AddExamples(className, isFlawed, features, withoutComments);
        }

        protected (Example withSource, Example withoutSource) AddExamples(string className, bool isFlawed, StringBuilder features, string source)
        {
            Example withoutSource = new Example { ClassName = className, IsFlawed = isFlawed, Features = features.ToString() };
            datasetWithoutSource.Add(withoutSource);
            features.Insert(0, source);
            Example withSource = new Example { ClassName = className, IsFlawed = isFlawed, Features = features.ToString() };
            datasetWithSource.Add(withSource);
            return (withSource, withoutSource);
        }

        private async Task<string> BuildMethodSignature(ExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            string signature = await BuildMethodName(invocationExpressionSyntax, semanticModel);
            return signature;
        }

        protected virtual async Task<string> BuildMethodName(ExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            ISymbol methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;
            return methodSymbol.ToDisplayString().Replace(" ", "");
        }

        protected async Task<bool> IsReachable(SyntaxNode syntaxNode)
        {
            bool isReachable = true;

            foreach (var ifNode in syntaxNode.Ancestors().OfType<IfStatementSyntax>())
            {
                string ifCondition = ifNode.Condition.ToString();

                try
                {
                    if (!unevaluatedIfs.Contains(ifCondition))
                    {
                        object result = await CSharpScript.EvaluateAsync(ifCondition, scriptOptions);
                        if (result is bool booleanResult)
                        {
                            if (!booleanResult)
                            {
                                isReachable = false;
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Unexpected return type: {result.GetType()}");
                            Console.ReadLine();
                        }
                    }
                }
                catch (CompilationErrorException ex)
                {
                    if (!unevaluatedIfs.Contains(ifCondition))
                    {
                        unevaluatedIfs.Add(ifCondition);
                    }
                }
            }

            return isReachable;
        }

        public override string GetPostExecutionOutput()
        {
            return string.Join(Environment.NewLine, unevaluatedIfs);
        }
    }
}
