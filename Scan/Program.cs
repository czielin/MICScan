using Data.Enums;
using ParseSardClassic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            int exitCode = -1;

            try
            {
                Scanner scanner = new Scanner(false);
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Missing expected arguments: Scan.exe [Project File Name] [Source Path]");
                }
                else
                {
                    string projectFileName = args[0];
                    string sourcePath = args[1];
                    string[] projectFilePaths = Directory.GetFiles(sourcePath, projectFileName, SearchOption.AllDirectories);
                    if (projectFilePaths.Length == 0)
                    {
                        Console.Error.WriteLine("The project file was not found.");
                    }
                    else if (projectFilePaths.Length > 1)
                    {
                        Console.Error.WriteLine("There was more than one project file with this name found.");
                    }
                    else
                    {
                        //var csFiles = await scanner.ScanFolder(@"C:\git\sard\TestApplication");
                        List<Example> classifiedFiles = await scanner.ScanProject(projectFilePaths[0]);
                        ConsoleColor originalColor = Console.ForegroundColor;
                        string flawDescription;
                        foreach (Example file in classifiedFiles)
                        {
                            if (file.ClassName == "No Flaw")
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                flawDescription = "";
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                flawDescription = $"{Environment.NewLine}\t{GetLabelDescription(file.ClassName)}";
                            }
                            Console.WriteLine($"[{file.ClassName}] {file.SourcePath} {flawDescription}");
                        }
                        Console.ForegroundColor = originalColor;
                        exitCode = classifiedFiles.Count(f => f.ClassName != "No Flaw");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            return exitCode;
        }

        private static string GetLabelDescription(string label)
        {
            string description = null;
            switch (label)
            {
                case "CWE022":
                    description = "Improper Limitation of a Pathname to a Restricted Directory ('Path Traversal')";
                    break;
                case "CWE078":
                    description = "Improper Neutralization of Special Elements used in an OS Command ('OS Command Injection')";
                    break;
                case "CWE089":
                    description = "Improper Neutralization of Special Elements used in an SQL Command ('SQL Injection')";
                    break;
                case "CWE090":
                    description = "Improper Neutralization of Special Elements used in an LDAP Query ('LDAP Injection')";
                    break;
                case "CWE091":
                    description = "XML Injection (aka Blind XPath Injection)";
                    break;
            }
            return description;
        }
    }
}
