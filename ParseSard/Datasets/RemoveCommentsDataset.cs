using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParseSard.Datasets
{
    public class RemoveCommentsDataset : Dataset
    {
        protected override string FileName { get; set; } = "RemoveComments";

        public override void AddExample(string className, bool isFlawed, List<string> fileContents)
        {
            string combinedContents = string.Join("\r\n\r\n", fileContents);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(combinedContents);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            CommentRemovalRewriter rewriter = new CommentRemovalRewriter();
            SyntaxNode newRoot = rewriter.Visit(root);
            string processedContents = newRoot.ToFullString();
            dataset.Add(new Example { ClassName = className, IsFlawed = isFlawed, Features = processedContents });
        }
    }
}
