using ParseSardClassic;
using ParseSardClassic.Datasets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan
{
    public class Scanner
    {
        ExternalArgumentsDataset externalArgumentsDataset = new ExternalArgumentsDataset();

        public async Task<List<Example>> ScanFolder(string directoryPath)
        {
            List<Example> scannedFiles = new List<Example>();
            string[] filePaths = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

            foreach (string filePath in filePaths)
            {
                Example example = await ScanFile(filePath);
                scannedFiles.Add(example);
            }

            return scannedFiles;
        }

        public async Task<Example> ScanFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            (var example, _) = await externalArgumentsDataset.AddExample("", false, fileContent);
            example.SourcePath = filePath;
            return example;
        }
    }
}
