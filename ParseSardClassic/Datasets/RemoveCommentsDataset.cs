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
        protected override string FileName { get; } = "RemoveComments";

        public override void AddExample(string className, bool isFlawed, List<string> fileContents)
        {
            string processedContents = RemoveComments(fileContents);
            dataset.Add(new Example { ClassName = className, IsFlawed = isFlawed, Features = processedContents });
        }
    }
}
