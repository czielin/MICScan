using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseSard.Datasets
{
    public class MethodCallsDataset : Dataset
    {
        private bool includeSource;

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
            //StringBuilder features = includeSource ? new StringBuilder(RemoveComments(fileContents)) : new StringBuilder();
            //SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(features.ToString());
            SyntaxNode root = syntaxTree.GetRoot();
            //PrintDescendentNodes(root);
            CSharpCompilation compilation = CSharpCompilation.Create("ExamineSnippet")
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(System.Text.ASCIIEncoding).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(System.Diagnostics.Activity).Assembly.Location))
                                               //.AddReferences(MetadataReference.CreateFromFile(typeof(System.ComponentModel.Win32Exception).Assembly.Location))
                                               .AddSyntaxTrees(syntaxTree);
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            //Console.WriteLine(features);
            //features.AppendLine();

            //foreach (MemberAccessExpressionSyntax methodCall in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            foreach (InvocationExpressionSyntax expression in root.DescendantNodes().Where(n => n.IsKind(SyntaxKind.InvocationExpression)))
            {
                //Console.WriteLine(expression);
                //string signature;
                //ExpressionSyntax expressionSyntax;
                //MemberAccessExpressionSyntax methodCall = (MemberAccessExpressionSyntax)expression.DescendantNodes().SingleOrDefault(e => e.IsKind(SyntaxKind.SimpleMemberAccessExpression));
                //if (methodCall != null)
                //{
                //    //Console.WriteLine($"Method call: {methodCall}");
                //    expressionSyntax = methodCall;
                //}
                //else
                //{
                //    InvocationExpressionSyntax invocationExpressionSyntax = (InvocationExpressionSyntax)expression.DescendantNodes().Single(e => e.IsKind(SyntaxKind.InvocationExpression));
                //    IdentifierNameSyntax identifierNameSyntax = invocationExpressionSyntax.ChildNodes().OfType<IdentifierNameSyntax>().Single();
                //    //Console.WriteLine($"Invocation: {invocationExpressionSyntax}");
                //    //PrintDescendentNodes(invocationExpressionSyntax);
                //    expressionSyntax = identifierNameSyntax;
                //    //signature = BuildMethodSignature(invocationExpressionSyntax, semanticModel);
                //}
                string signature = BuildMethodSignature(expression, semanticModel);
                features.AppendLine(signature);

                //foreach (ArgumentSyntax argumentSyntax in expression.DescendantNodes().OfType<ArgumentSyntax>())
                //{
                //    //Console.WriteLine(argumentSyntax);
                //    var argumentChild = argumentSyntax.ChildNodes().Single();
                //    Console.WriteLine($"Argument Child: {argumentChild.Kind()} {semanticModel.GetTypeInfo(argumentChild).Type} {argumentChild}");
                //}
            }
            //foreach (SyntaxNodeOrToken syntaxNode in root.DescendantNodesAndTokens())
            //{
            //    if (syntaxNode.IsKind(SyntaxKind.InvocationExpression))
            //    {
            //        Console.WriteLine(syntaxNode);
            //        //foreach (var expressionChild in syntaxNode.ChildNodesAndTokens())
            //        //{
            //        //    Console.WriteLine($"{expressionChild.Kind()} {expressionChild.IsNode} {expressionChild.IsToken} {expressionChild.ToString()}");
            //        //}
            //        var method = syntaxNode.ChildNodesAndTokens().Single(s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression));
            //        Console.WriteLine(method);
            //        var argumentList = (ArgumentListSyntax)syntaxNode.ChildNodesAndTokens().Single(s => s.IsKind(SyntaxKind.ArgumentList));
            //        Console.WriteLine(argumentList.ToString());
            //        foreach (var argument in argumentList.ChildNodesAndTokens())
            //        {
            //            Console.WriteLine($"{argument.Kind()}: {argument.ToString()}");
            //            Console.WriteLine(argument.GetType());
            //            ArgumentSyntax argumentSyntax = (ArgumentSyntax)argument;
            //            foreach (var argumentChild in argument.ChildNodesAndTokens())
            //            {
            //                Console.WriteLine($"{argument.Kind()}: {argumentChild}");
            //            }
            //        }
            //    }
            //    //Console.WriteLine($"{syntaxNode.Kind()} {syntaxNode.IsNode} {syntaxNode.IsToken} {syntaxNode.ToString()}");
            //}
            Console.WriteLine(features);
            Console.Read();
            dataset.Add(new Example { ClassName = className, IsFlawed = isFlawed, Features = features.ToString() });
        }

        private string BuildMethodSignature(InvocationExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            MemberAccessExpressionSyntax memberAccessExpressionSyntax = expressionSyntax.ChildNodes().OfType<MemberAccessExpressionSyntax>().SingleOrDefault();
            ExpressionSyntax identifierExpression = memberAccessExpressionSyntax ?? (ExpressionSyntax)expressionSyntax;
            //var methodSymbol = semanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;
            //var arguments = expressionSyntax.DescendantNodes().OfType<ArgumentSyntax>();
            var symbol = semanticModel.GetSymbolInfo(expressionSyntax).Symbol;
            string signature = $"{BuildExpressionName(identifierExpression, semanticModel)}({string.Join(",", expressionSyntax.ArgumentList.Arguments.Select(a => GetArgumentType(a.Expression, semanticModel).ToDisplayString()))})";
            //Console.WriteLine(signature);
            //PrintDescendentNodes(expressionSyntax);
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

            //Console.WriteLine($"{argument} - {type}");
            //PrintDescendentNodes(argument);
            return type;
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
