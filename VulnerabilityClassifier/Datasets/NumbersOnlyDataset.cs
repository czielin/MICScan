using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseSardClassic.Datasets
{
    /// <summary>
    /// This has been merged into the ExternalArgumentsDataset. Leaving here for now in case we decide to revive it.
    /// </summary>
    public class NumbersOnlyRegexDataset : ExternalArgumentsDataset
    {
        Regex ifRegex = new Regex(@"if\s*\((?<expression>.*)\)");
        ScriptOptions scriptOptions = ScriptOptions.Default.WithImports("System", "System.Math");
        //HashSet<string> unevaluatedIfs = new HashSet<string>();

        protected override string FileName
        {
            get
            {
                return nameof(NumbersOnlyRegexDataset);
            }
        }

        public override async Task<(Example withSource, Example withoutSource)> AddExample(string className, bool isFlawed, List<string> fileContents)
        {
            Example withSource = null, withoutSource = null;
            if (fileContents[0].Contains("[0-9]") && await HasUnreachableBlock(fileContents[0]))
            {
                (withSource, withoutSource) = await base.AddExample(className, isFlawed, fileContents);
                bool hasNumbersOnlyRegex = fileContents.Any(f => f.Contains("/^[0-9]*$/"));
                withSource.Features += $"HasNumbersOnlyRegex={hasNumbersOnlyRegex}";
                withoutSource.Features += $"HasNumbersOnlyRegex={hasNumbersOnlyRegex}";

                //bool hasUnreachableBlock = false;
                //foreach (string fileContent in fileContents)
                //{
                //    if (await HasUnreachableBlock(fileContent))
                //    {
                //        hasUnreachableBlock = true;
                //        break;
                //    }
                //}
                //withSource.Features += $"{Environment.NewLine}HasUnreachableBlock={hasUnreachableBlock}";
                //withoutSource.Features += $"{Environment.NewLine}HasUnreachableBlock={hasUnreachableBlock}";
                Console.WriteLine(withSource.Features.ToString());
                Console.ReadLine();
            }
            return (withSource, withoutSource);
        }

        private async Task<bool> HasUnreachableBlock(string snippet)
        {
            bool hasUnreachableBlock = false;

            foreach (Match match in ifRegex.Matches(snippet))
            {
                try
                {
                    object result = await CSharpScript.EvaluateAsync(match.Groups["expression"].Value, scriptOptions);
                    if (result is bool booleanResult)
                    {
                        if (!booleanResult)
                        {
                            hasUnreachableBlock = true;
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected return type: {result.GetType()}");
                        Console.ReadLine();
                    }
                }
                catch (CompilationErrorException ex)
                {
                    //if (!unevaluatedIfs.Contains(match.Groups["expression"].Value))
                    //{
                    //    unevaluatedIfs.Add(match.Groups["expression"].Value);
                    //}
                    //Console.WriteLine($"Error evaluating if expression: {match.Groups["expression"].Value}");
                    //Console.ReadLine();
                }
            }

            return hasUnreachableBlock;
        }

        //public override string GetPostExecutionOutput()
        //{
        //    return string.Join(Environment.NewLine, unevaluatedIfs);
        //}
    }
}
