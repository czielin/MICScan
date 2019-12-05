using ParseSardClassic.Datasets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ParseSardClassic
{
    class Program
    {
        private static List<Dataset> datasets = new List<Dataset>();
        private static readonly string sardRoot = @"..\..\..\sard_archive\";
        private static readonly string outputDirectory = @"..\..\Output";
        private static readonly string cwePattern = @"cwe-*_*\d+";

        private static int totalCount = 0;
        private static int cSharpCount = 0;
        private static int flawedCount = 0;
        private static int fixedCount = 0;
        private static int mixedCount = 0;
        private static int noFlawOrFix = 0;
        private static int candidateCount = 0;
        private static int approvedCount = 0;
        private static int deprecatedCount = 0;
        private static int numbersOnlyRegexFlawed = 0;
        private static int numbersOnlyRegexUnflawed = 0;

        static async Task Main(string[] args)
        {
            // Point at where SARD archive has been downloaded and extracted to.
            // https://samate.nist.gov/SARD/archive/sard_archive.zip
            XDocument document = XDocument.Load(Path.Combine(sardRoot, "full_manifest.xml"));
            var testCases = document.Descendants("testcase");

            Dictionary<string, int> cweCounts = new Dictionary<string, int>();
            Dictionary<string, int> flawlessCweCounts = new Dictionary<string, int>();
            //datasets.Add(new RemoveCommentsDataset());
            //datasets.Add(new MethodCallsDataset());
            datasets.Add(new ExternalArgumentsDataset());
            //datasets.Add(new NumbersOnlyRegexDataset());

            List<Task> parserTasks = new List<Task>();

            foreach (var testCase in testCases)
            {
                totalCount++;
                if
                (
                    //totalCount > 228000 &&
                    testCase.Attribute("language").Value == "C#"
                    && testCase.Elements("file").Count(f => f.Attribute("path").Value.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)) == 1
                )
                {
                    cSharpCount++;
                    switch (testCase.Attribute("status").Value)
                    {
                        case "Approved":
                            approvedCount++;
                            break;
                        case "Candidate":
                            candidateCount++;
                            break;
                        case "Deprecated":
                            deprecatedCount++;
                            break;
                    }

                    string className = null;
                    bool? isFlawed = null;

                    var files = testCase.Elements("file").Where(f => f.Attribute("path").Value.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase));
                    if (files.Any(file => file.Elements("flaw").Any()))
                    {
                        isFlawed = true;
                        flawedCount++;
                        foreach (var flaw in files.Descendants("flaw"))
                        {
                            className = ParseCweId(flaw.Attribute("name").Value);
                            if (className != null)
                            {
                                break;
                            }
                        }
                        if (files.Any(file => file.Descendants("flaw").Any(flawToCheck => className != ParseCweId(flawToCheck.Attribute("name").Value))))
                        {
                            Console.WriteLine($"Test case found with more than one CWE type:");
                            Console.WriteLine(testCase);
                            Console.ReadLine();
                        }
                        if (cweCounts.ContainsKey(className))
                        {
                            cweCounts[className]++;
                        }
                        else
                        {
                            cweCounts.Add(className, 1);
                        }
                    }
                    else if (files.Any(file => file.Elements("fix").Any()))
                    { // There aren't currently any fixed C# examples in the SARD. Will need to be updated if this changes.
                        fixedCount++;
                    }
                    else if (files.Any(file => file.Elements("mixed").Any()))
                    { // There aren't currently any fixed C# examples in the SARD. Will need to be updated if this changes.
                        mixedCount++;
                    }
                    else
                    {
                        noFlawOrFix++;
                        isFlawed = false;
                        foreach (XElement file in files)
                        {
                            string cweId = ParseCweId(file.Attribute("path").Value);
                            if (cweId != null)
                            {
                                className = cweId;
                                break;
                            }
                        }
                        if (files.Any(file => className != ParseCweId(file.Attribute("path").Value)))
                        {
                            Console.WriteLine($"Test case found with more than one CWE type:");
                            Console.WriteLine(testCase);
                            Console.ReadLine();
                        }
                        if (className == null)
                        {
                            Console.WriteLine($"No CWE match found for test case:");
                            Console.WriteLine(testCase);
                            Console.ReadLine();
                        }
                        if (flawlessCweCounts.ContainsKey(className))
                        {
                            flawlessCweCounts[className]++;
                        }
                        else
                        {
                            flawlessCweCounts.Add(className, 1);
                        }
                    }

                    if (isFlawed == null)
                    {
                        throw new NotImplementedException("The 'fix' and 'mixed' test case support is not yet implemented.");
                    }

                    await AddExample(className, isFlawed.Value, files);
                }

                if (totalCount % 1000 == 0)
                {
                    Console.WriteLine($"Test cases processed: {totalCount}");
                    Console.WriteLine($"C# Test cases processed: {cSharpCount}");
                }
            }
            Console.WriteLine("Number of test cases in manifest: " + totalCount);
            Console.WriteLine("Number of C# test cases: " + cSharpCount);
            Console.WriteLine("Number of test cases with flaws: " + flawedCount);
            Console.WriteLine("Number of test cases with fixes: " + fixedCount);
            Console.WriteLine("Number of test cases with mixed: " + mixedCount);
            Console.WriteLine("Number of tets cases with no flaw or fix: " + noFlawOrFix);
            Console.WriteLine("Number of approved test cases: " + approvedCount);
            Console.WriteLine("Number of candidate test cases: " + candidateCount);
            Console.WriteLine("Number of deprecated test cases: " + deprecatedCount);
            Console.WriteLine("CWE counts with flaws:");
            foreach (var cweCount in cweCounts)
            {
                Console.WriteLine(cweCount.Key + ": " + cweCount.Value);
            }
            Console.WriteLine("CWE counts for test cases without flaws:");
            foreach (var cweCount in flawlessCweCounts)
            {
                Console.WriteLine(cweCount.Key + ": " + cweCount.Value);
            }

            Console.WriteLine("Numbers only flawed: " + numbersOnlyRegexFlawed);
            Console.WriteLine("Numbers only without flaws: " + numbersOnlyRegexUnflawed);

            SerializeDatasets();

            Console.Read();
        }

        private static string ParseCweId(string fullValue)
        {
            string cweId = null;
            Match match = Regex.Match(fullValue, cwePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                cweId = match.Value;
            }
            cweId = cweId.Replace("-", "").Replace("_", "").ToUpper();
            if (cweId.Length == 5)
            {
                cweId = cweId.Insert(3, "0");
            }
            return cweId;
        }

        private static void SerializeDatasets()
        {
            foreach (Dataset dataset in datasets)
            {
                dataset.SaveToFile(outputDirectory);
                Console.WriteLine($"{Environment.NewLine}{dataset.GetType().Name} complete.");
                Console.WriteLine(Environment.NewLine + dataset.GetPostExecutionOutput());
            }
        }

        private static async Task AddExample(string className, bool isFlawed, IEnumerable<XElement> files)
        {
            List<string> fileContents = new List<string>();
            foreach (XElement file in files)
            {
                string fileName = Path.Combine(sardRoot, "testcases", file.Attribute("path").Value);
                fileContents.Add(File.ReadAllText(fileName));
            }
            if (fileContents[0].Contains("[0-9]"))
            {
                if (isFlawed)
                {
                    numbersOnlyRegexFlawed++;
                }
                else
                {
                    numbersOnlyRegexUnflawed++;
                }
            }
            foreach (Dataset dataSet in datasets)
            {
                await dataSet.AddExample(className, isFlawed, fileContents);
            }
        }
    }
}
