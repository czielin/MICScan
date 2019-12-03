using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseSardClassic.Datasets
{
    public class ExternalArgumentsDataset : MethodCallsDataset
    {
        public ExternalArgumentsDataset(bool includeSource) : base(includeSource)
        {
        }

        protected override string FileName
        {
            get
            {
                return includeSource ? "MethodCallsWithSource" : "MethodCallsWithoutSource";
            }
        }

        protected override string BuildMethodName(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            string methodName = base.BuildMethodName(invocationExpressionSyntax, semanticModel);
            methodName += BuildArgumentsExternal(invocationExpressionSyntax, semanticModel);
            //PrintDescendentNodes(syntaxNode, semanticModel);
            IMethodSymbol methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            return methodSymbol.ToDisplayString().Replace(" ", "");
        }

        private string BuildArgumentsExternal(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            List<bool?> externals = new List<bool?>();
            foreach (ArgumentSyntax argument in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                bool? hasExternalInput = null;
                var argumentChild = argument.ChildNodes().Single();
                ISymbol argumentSymbol = semanticModel.GetSymbolInfo(argumentChild).Symbol;
                if (argumentChild.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    hasExternalInput = false;
                }
                else if (argumentChild.IsKind(SyntaxKind.IdentifierName))
                {

                }
                //switch (argumentSymbol)
                //{
                //    case ILocalSymbol localSymbol:
                //        //hasExternalInput = await IsAssignedFromUserInputMethod(localSymbol);
                //        break;
                //}
                if (hasExternalInput == null)
                {
                    Console.WriteLine($"Don't yet know how to parse arguments of this form: {argument}");
                    PrintDescendentNodes(invocationExpressionSyntax, semanticModel);
                    Console.ReadLine();
                }
                externals.Add(hasExternalInput);
            }
            return $"IsExternal({string.Join(",", externals.Select(e => e == null ? "?" : e.ToString()))})";
        }

        private async Task<bool> IsAssignedFromUserInputMethod(ILocalSymbol localSymbol, SemanticModel semanticModel)
        {
            //var argumentReferences = await SymbolFinder.FindReferencesAsync(localSymbol, semanticModel.doc);
            //SymbolFinder.FindReferencesAsync()
            //semanticModel.
            //foreach (var reference in argumentReferences)
            //{
            //    Console.WriteLine(reference);
            //}
            throw new NotImplementedException();
        }
    }
}
