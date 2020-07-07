using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
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
using System.Windows.Forms;

namespace Scan
{
    public class Scanner
    {
        string workingDirectory = Environment.CurrentDirectory;
        string pythonPath = @"python.exe"; // Should be in the path.
        string classifyPythonFilePath = Path.Combine(Application.StartupPath, "Classify.py");
        string modelPath = Path.Combine(Application.StartupPath, "model.pkl");
        string vocabularyPath = Path.Combine(Application.StartupPath, "vocabulary.pkl");
        private bool verbose;

        public Scanner(bool verbose)
        {
            this.verbose = verbose;
        }

        public async Task<List<Example>> ScanProject(string projectFilePath)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                Project project = await workspace.OpenProjectAsync(projectFilePath);
                IEnumerable<Document> documents = project.Documents.Where(d => d.FilePath != null && d.FilePath.EndsWith(".cs"));
                List<Example> scannedFiles = new List<Example>();
                ExternalArgumentsDataset externalArgumentsDataset = new ExternalArgumentsDataset(projectFilePath);
                foreach (Document document in documents)
                {
                    Example example = await ScanFile(document, externalArgumentsDataset);
                    scannedFiles.Add(example);
                }

                return await ClassifyFiles(scannedFiles);
            }
        }

        public async Task<List<Example>> ScanFolder(string directoryPath)
        {
            string[] filePaths = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
            List<Example> scannedFiles = new List<Example>();
            ExternalArgumentsDataset externalArgumentsDataset = new ExternalArgumentsDataset(null);
            foreach (string filePath in filePaths)
            {
                Example example = await ScanFile(filePath, externalArgumentsDataset);
                scannedFiles.Add(example);
            }

            return await ClassifyFiles(scannedFiles);
        }

        private async Task<Example> ScanFile(string filePath, ExternalArgumentsDataset externalArgumentsDataset)
        {
            string fileContent = File.ReadAllText(filePath);
            (_, Example example) = await externalArgumentsDataset.AddExample("", false, fileContent);
            example.SourcePath = filePath;
            example.Features = example.Features.Trim();
            return await ClassifyFile(example);
        }

        private async Task<Example> ScanFile(Document document, ExternalArgumentsDataset externalArgumentsDataset)
        {
            (_, Example example) = await externalArgumentsDataset.AddExample(document);
            example.SourcePath = document.FilePath;
            example.Features = example.Features.Trim();
            return await ClassifyFile(example);
        }

        private async Task<Example> ClassifyFile(Example example)
        {
            List<Example> examples = new List<Example>
            {
                example
            };
            return (await ClassifyFiles(examples)).Single();
        }

        private async Task<List<Example>> ClassifyFiles(List<Example> examples)
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

            if (verbose)
            {
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

            string classifiedFileContent = File.ReadAllText(classifiedFilePath);
            List<Example> classifiedExamples = JsonConvert.DeserializeObject<List<Example>>(classifiedFileContent);
            return classifiedExamples;
        }
    }
}
