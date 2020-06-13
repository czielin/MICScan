using Newtonsoft.Json;
using ParseSardClassic;
using ParseSardClassic.Datasets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan
{
    public class Scanner
    {
        ExternalArgumentsDataset externalArgumentsDataset = new ExternalArgumentsDataset();
        string workingDirectory = Environment.CurrentDirectory;
        string pythonPath = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64\python.exe";
        string classifyPythonFilePath = @"C:\git\sard\Classify\Classify.py";
        string modelPath = @"C:\git\sard\VulnerabilityClassifier\Notebooks\model.pkl";
        string vocabularyPath = @"C:\git\sard\VulnerabilityClassifier\Notebooks\vocabulary.pkl";

        public async Task<List<Example>> ScanFolder(string directoryPath)
        {
            List<Example> scannedFiles = new List<Example>();
            string[] filePaths = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

            foreach (string filePath in filePaths)
            {
                Example example = await ScanFile(filePath);
                scannedFiles.Add(example);
            }

            await ClassifyFiles(scannedFiles);

            return scannedFiles;
        }

        public async Task<Example> ScanFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            (_, var example) = await externalArgumentsDataset.AddExample("", false, fileContent);
            example.SourcePath = filePath;
            example.Features = example.Features.Trim();
            await ClassifyFile(example);
            return example;
        }

        private async Task ClassifyFile(Example example)
        {
            List<Example> examples = new List<Example>
            {
                example
            };
            await ClassifyFiles(examples);
        }

        private async Task ClassifyFiles(List<Example> examples)
        {
            string json = JsonConvert.SerializeObject(examples);
            string featuresFilePath = Path.Combine(workingDirectory, $"FileFeatures.json");
            string classifiedFilePath = Path.Combine(workingDirectory, $"Classified.json");
            File.WriteAllText(featuresFilePath, json);
            ProcessStartInfo pythonProcessInfo = new ProcessStartInfo();
            pythonProcessInfo.FileName = pythonPath;
            pythonProcessInfo.Arguments = $"\"{classifyPythonFilePath}\" \"{modelPath}\" \"{featuresFilePath}\" \"{vocabularyPath}\" \"{classifiedFilePath}\"";
            pythonProcessInfo.CreateNoWindow = true;
            pythonProcessInfo.RedirectStandardOutput = true;
            pythonProcessInfo.RedirectStandardError = true;
            pythonProcessInfo.UseShellExecute = false;

            string errors;
            string output;
            using (Process pythonProcess = Process.Start(pythonProcessInfo))
            {
                errors = await pythonProcess.StandardError.ReadToEndAsync();
                output = await pythonProcess.StandardOutput.ReadToEndAsync();
            }

            Console.WriteLine(output);
            Console.Error.WriteLine(errors);
        }
    }
}
