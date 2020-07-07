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
            Scanner scanner = new Scanner(true);
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
                    foreach (Example file in classifiedFiles)
                    {
                        if (file.ClassName == "No Flaw")
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        Console.WriteLine($"[{file.ClassName}] {file.SourcePath}");
                    }
                    Console.ForegroundColor = originalColor;
                    exitCode = classifiedFiles.Count(f => f.ClassName != "No Flaw");
                }
            }

            return exitCode;
        }
    }
}
