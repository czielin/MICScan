using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParseSard.Datasets
{
    public abstract class Dataset
    {
        protected List<Example> dataset = new List<Example>();
        protected abstract string FileName { get; }

        public abstract void AddExample(string className, bool isFlawed, List<string> fileContents);
        public void SaveToFile(string directory)
        {
            string json = JsonConvert.SerializeObject(dataset);
            File.WriteAllText(Path.Combine(directory, $"{FileName}.json"), json);
        }

        protected static string CombineContents(List<string> fileContents)
        {
            return string.Join("\r\n\r\n", fileContents);
        }

        protected static string RemoveComments(List<string> fileContents)
        {
            string combinedContents = CombineContents(fileContents);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(combinedContents);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            CommentRemovalRewriter rewriter = new CommentRemovalRewriter();
            SyntaxNode newRoot = rewriter.Visit(root);
            string processedContents = newRoot.ToFullString();
            return processedContents;
        }

        protected void PrintDescendentNodes(IEnumerable<SyntaxNode> syntaxNodes)
        {
            foreach (SyntaxNode syntaxNode in syntaxNodes)
            {
                PrintDescendentNodes(syntaxNode);
            }
        }

        protected void PrintDescendentNodes(SyntaxNode syntaxNode, string indent = "")
        {
            Console.WriteLine($"{indent}{syntaxNode} ({syntaxNode.Kind()})");
            foreach (var child in syntaxNode.ChildNodes())
            {
                PrintDescendentNodes(child, indent + " ");
            }
        }
    }
}
