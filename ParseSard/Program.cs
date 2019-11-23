using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ParseSard
{
    class Program
    {
        static void Main(string[] args)
        {
            // Point at where SARD archive has been downloaded and extracted to.
            // https://samate.nist.gov/SARD/archive/sard_archive.zip
            XDocument document = XDocument.Load(@"..\..\..\..\sard_archive\full_manifest.xml");
            var testCases = document.Descendants("testcase");
            int totalCount = 0;
            int cSharpCount = 0;
            int flawedCount = 0;
            int fixedCount = 0;
            int mixedCount = 0;
            int noFlawOrFix = 0;
            int candidateCount = 0;
            int approvedCount = 0;
            int deprecatedCount = 0;
            Dictionary<string, int> cweCounts = new Dictionary<string, int>();
            foreach (var testCase in testCases)
            {
                totalCount++;
                if (testCase.Attribute("language").Value == "C#")
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

                    var files = testCase.Elements("file");
                    if (files.Any(file => file.Elements("flaw").Any()))
                    {
                        flawedCount++;
                        foreach (var flaw in files.Descendants("flaw"))
                        {
                            string name = flaw.Attribute("name").Value;
                            if (cweCounts.ContainsKey(name))
                            {
                                cweCounts[name]++;
                            }
                            else
                            {
                                cweCounts.Add(name, 1);
                            }
                        }
                    }
                    else if (files.Any(file => file.Elements("fix").Any()))
                    {
                        fixedCount++;
                    }
                    else if (files.Any(file => file.Elements("mixed").Any()))
                    {
                        mixedCount++;
                    }
                    else
                    {
                        noFlawOrFix++;
                        //Console.WriteLine("Test case with no flaw: ");
                        //Console.WriteLine(testCase);
                        //Console.Read();
                    }
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
            Console.WriteLine("CWE counts:");
            foreach (var cweCount in cweCounts)
            {
                Console.WriteLine(cweCount.Key + ": " + cweCount.Value);
            }
            Console.Read();
        }
    }
}
